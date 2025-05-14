using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.Pool;

#if UNITY_EDITOR
using UnityEditor.Playables;
#endif

public class AutoBattleUI : MonoBehaviour
{
    [SerializeField] private Transform canvasTransform;
    [SerializeField] private GameObject canvas;

    [SerializeField] private RewardUI rewardUI;

    [SerializeField] private TextMeshProUGUI _myUnitCountUI;
    [SerializeField] private TextMeshProUGUI _enemyUnitCountUI;
    [SerializeField] private TextMeshProUGUI _myUnitHPUI;
    [SerializeField] private TextMeshProUGUI _emyUnitHPUI;

    [SerializeField] private TextMeshProUGUI moraleText;       
    [SerializeField] private ObjectPool objectPool;          
    [SerializeField] private GameObject abilityPool;           
  
    [SerializeField] private TextMeshProUGUI _myDodge;       
    [SerializeField] private TextMeshProUGUI _enemyDodge;      
    [SerializeField] private Slider myHpBar;                    
    [SerializeField] private Slider enemyHpBar;                
    [SerializeField] private GameObject myRangeCount;             
    [SerializeField] private GameObject enemyRangeCount;

    [SerializeField] private Transform myFirtstUnitParent;
    [SerializeField] private Transform mySecondUnitParent;
    [SerializeField] private Transform enemyFirstUnitParent;
    [SerializeField] private Transform enemySecondUnitParent;

    [SerializeField] private Transform myBackUnitsParent;
    [SerializeField] private Transform enemyBackUnitsParent;

    [SerializeField] private GameObject loadingWindow;     

    [SerializeField] private GameObject relicBox;
    [SerializeField] private GameObject abilityBox;

    [SerializeField] private GameObject itemToolTip;    

    private Vector3 myTeam = new(270, 280, 0);               
    private Vector3 enemyTeam = new(-270, 280, 0);          

    private float waittingTime = 500f;

    private Dictionary<int, GameObject> myUnitCache = new();
    private Dictionary<int, GameObject> enemyUnitCache = new();

    private void Start()
    {
        ResetUIActive();
        
        myHpBar.interactable = false;
        enemyHpBar.interactable = false;

        UpdateMorale();
    }
    //유닛 갯수 
    public void UpdateUnitCountUI(int myUnitCount, int enemyUnitCount)
    {
        _myUnitCountUI.text = $"{myUnitCount}";
        _enemyUnitCountUI.text = $"{enemyUnitCount}";

    }
    //유닛 체력
    public void UpateUnitHPUI(float myUnitHP, float enemyUnitHP, float myMaxHp, float enemyMaxHp)
    {
        if (_myUnitHPUI != null && _emyUnitHPUI)
        {
            _myUnitHPUI.text = $"{myUnitHP}/{myMaxHp}";
            _emyUnitHPUI.text = $"{enemyUnitHP}/{enemyMaxHp}";
        }

        //체력바 업데이트
        myHpBar.maxValue = myMaxHp;
        myHpBar.value = myUnitHP;

        enemyHpBar.maxValue = enemyMaxHp;
        enemyHpBar.value = enemyUnitHP;
    }

    // 데미지 표시
    public void ShowDamage(float _damage, string text, bool team, bool isAttack, int unitIndex)
    {
        float offsetX = 50f;
        float damage = -_damage;

        BattleAnimation(damage, text, team, isAttack);

        if (isAttack)
            StartCoroutine(DelayedDamageDisplay(damage, text, team, unitIndex, offsetX));
        else
            ShowDamageImmediately(damage, text, team, unitIndex, offsetX);
    }

    private IEnumerator DelayedDamageDisplay(float damage, string text, bool team, int unitIndex, float offsetX)
    {
        yield return new WaitForSeconds(waittingTime * 0.0005f);
        ShowDamageInternal(damage, text, team, unitIndex, offsetX);
    }
    private void ShowDamageImmediately(float damage, string text, bool team, int unitIndex, float offsetX)
    {
        ShowDamageInternal(damage, text, team, unitIndex, offsetX);
    }
    private void ShowDamageInternal(float damage, string text, bool team, int unitIndex, float offsetX)
    {
        GameObject damageObj = objectPool.GetDamageText();
        damageObj.SetActive(true);

        TextMeshProUGUI damagetext = damageObj.GetComponent<TextMeshProUGUI>();
        damagetext.color = damage >= 0 ? Color.green : Color.red;
        damagetext.text = damage == 0 ? $"{text}" : $"{damage} {text}";

        RectTransform rectTransform = damageObj.GetComponent<RectTransform>();

        if (team)
        {
            if (unitIndex == 0)
                rectTransform.anchoredPosition = myTeam;
            else
            {
                GameObject unit = FindUnit(unitIndex, !team);
                RectTransform unitRect = unit.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = unitRect.anchoredPosition + new Vector2(offsetX, 0);
            }
        }
        else
        {
            if (unitIndex == 0)
                rectTransform.anchoredPosition = enemyTeam;
            else
            {
                GameObject unit = FindUnit(unitIndex, !team);
                RectTransform unitRect = unit.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = unitRect.anchoredPosition + new Vector2(offsetX, 0);
            }
        }

        CreateAbility(text, team);
        StartCoroutine(HideAfterDelay(damageObj));
    }


    private void BattleAnimation(float damage, string text, bool team,bool isAttack)
    {
        // team = true -> 상대 공격, false -> 나의 공격
        if (!isAttack) return;

        GameObject unit = FindUnit(0, team);
        RectTransform rectTransform = unit.GetComponent<RectTransform>();

        Vector2 originPos = rectTransform.anchoredPosition; // 원래 UI 위치
        float direction = team ? 1f : -1f; // team이 true면 반대 방향

        Vector2 moveBackPos = originPos + new Vector2(direction * -10f, 0f); // 뒤로 이동할 위치
        Vector2 moveForwardPos = originPos + new Vector2(direction * 25f, 0f); // 앞으로 이동할 위치

        // DOTween 애니메이션 시퀀스
        DG.Tweening.Sequence attackSequence = DOTween.Sequence();
        attackSequence.Append(rectTransform.DOAnchorPos(moveBackPos, 0.05f)) // 뒤로 0.05초 이동
                      .AppendInterval(0.2f) // 0.2초 대기
                      .Append(rectTransform.DOAnchorPos(moveForwardPos, 0.2f)) // 앞으로 0.2초 이동
                      .Append(rectTransform.DOAnchorPos(originPos, 0.05f)); // 원래 위치로 0.05초 이동

        rectTransform.DOKill();
    }
    private IEnumerator HideAfterDelay(GameObject damageObj)
    {
        yield return new WaitForSeconds(waittingTime/1000f);
        objectPool.ReturnDamageText(damageObj);
    }

    //유닛 갯수 만큼 유닛 이미지 생성
    public void CreateUnitBox(List<RogueUnitDataBase> myUnits, List<RogueUnitDataBase> enemyUnits, float myDodge, float enemyDodge, List<RogueUnitDataBase> myRangeUnits, List<RogueUnitDataBase> enemyRangeUnits)
    {
        Vector3[] myPositions = { new Vector3(-180, 94, 0), new Vector3(-131, -385, 0)};
        Vector3[] enemyPositions = { new Vector3(200, 94, 0), new Vector3(152, -385, 0) };
        Vector3 myRangeUnitPos = new Vector3(-833, -143, 0);
        Vector3 enemyRangeUnitPos = new Vector3(837, -143, 0);

        float firstSize = 250;
        float secondSize = 200;
        float unitInterval = 210;

        ClearExistingUnitImages();

        ClearExistingAbilityIcons();

        CreateAbilityIcons(myUnits[0], true);

        CreateAbilityIcons(enemyUnits[0], false);

        CreateUnitImages(myUnits, myPositions, firstSize, secondSize, unitInterval, true, myDodge);

        CreateRangeUnit(myRangeUnits.Count, myRangeUnitPos, myRangeCount, true);

        CreateUnitImages(enemyUnits, enemyPositions, firstSize, secondSize, unitInterval, false, enemyDodge);

        CreateRangeUnit(enemyRangeUnits.Count, enemyRangeUnitPos, enemyRangeCount, false);

    }

    // 기존 유닛 이미지 반환 및 비활성화
    private void ClearExistingUnitImages()
    {
        foreach (var unit in objectPool.GetActiveBattleUnits())
        {
            objectPool.ReturnBattleUnit(unit); // 유닛 비활성화 및 반환
        }
    }

    //원거리 유닛 생성
    private void CreateRangeUnit(int rangeUnitCount, Vector3 position, GameObject number, bool isMyTeam)
    {
        float size = 200f;
        string myTeam = isMyTeam ? "My" : "Enemy";

        Image numberImg = number.GetComponent<Image>();

        if (rangeUnitCount == 0)
        {
            numberImg.color = new Color(1, 1, 1, 0); // 0~1 범위로 수정
            return;
        }
        numberImg.color = new Color(1, 1, 1, 1);

        GameObject unit = objectPool.GetBattleUnit();
        RectTransform rectTransform = unit.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(size - 10, size - 10);
        rectTransform.anchoredPosition = position;

        // 이미지 설정 & 투명도 정상화
        Image img = unit.GetComponent<Image>();
        img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
        img.sprite = SpriteCacheManager.GetSprite("KIcon/AbilityIcon/rangedAttack");

        // 자식 유닛 이미지 설정
        Transform childUnit = unit.transform.GetChild(0);
        RectTransform childRectTransform = childUnit.GetComponent<RectTransform>();
        Image childImg = childUnit.GetComponent<Image>();
        childRectTransform.sizeDelta = new Vector2(size, size);
        childImg.sprite = SpriteCacheManager.GetSprite($"KIcon/UI_{myTeam}SecondUnit");

        // 숫자 이미지 설정
        numberImg.sprite = SpriteCacheManager.GetSprite($"KIcon/UI_{myTeam}X{rangeUnitCount}");

        // 숫자 이미지를 가장 뒤로
        number.transform.SetSiblingIndex(number.transform.parent.childCount - 1);

        // 유닛 이름 설정
        unit.name = $"{myTeam}RangeUnit";
    }

    //유닛 이미지 생성
    private void CreateUnitImages(List<RogueUnitDataBase> units, Vector3[] positions, float firstSize, float secondSize, float unitInterval, bool isMyUnit, float dodge)
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].health <= 0)
                continue;

            string unitTeam = isMyUnit ? "My" : "Enemy";
            Transform parent = isMyUnit ? myBackUnitsParent : enemyBackUnitsParent;

            GameObject unitImage = objectPool.GetBattleUnit();
            Transform childUnit = unitImage.transform.GetChild(0);
            RectTransform rectTransform = unitImage.GetComponent<RectTransform>();
            Image unitFrame = childUnit.GetComponent<Image>();

            // 위치 설정
            if (i < positions.Length)
            {
                rectTransform.anchoredPosition = positions[i];
                unitTeam += i switch
                {
                    0 => "FirstUnit",
                    1 => "SecondUnit",
                    _ => "BackUnit"
                };

                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
            else
            {
                unitTeam += "BackUnit";

                unitImage.transform.SetParent(parent,false);
            }


            // 유닛 본체 이미지 설정
            Image img = unitImage.GetComponent<Image>();
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
            img.sprite = SpriteCacheManager.GetSprite($"UnitImages/{units[i].unitImg}");

            // 유닛 테두리 프레임 이미지 설정
            unitFrame.sprite = SpriteCacheManager.GetSprite($"KIcon/UI_{unitTeam}");

            // 유닛 이름 설정
            unitImage.name = $"{(isMyUnit ? "My" : "Enemy")}Unit{i}";
        }

        // 회피율 UI 설정
        if (isMyUnit)
        {
            _myDodge.text = $"회피율: {dodge}%";
        }
        else
        {
            _enemyDodge.text = $"회피율: {dodge}%";
        }
    }

    // 능력치 아이콘 생성
    private void CreateAbilityIcons(RogueUnitDataBase unit, bool isTeam)
    {
        float posY = -103f;
        float myTeamPosX = -280f;
        float enemyTeamPosX = 88f;
        float interval = 62f;

        float teamPosX = isTeam ? myTeamPosX : enemyTeamPosX;

        var boolAttributes = unit.GetType().GetFields()
            .Where(f => f.FieldType == typeof(bool))
            .Select(f => new { Name = f.Name, Value = (bool)f.GetValue(unit) });

        int i = 0;

        foreach (var attr in boolAttributes)
        {
            if (!attr.Value || attr.Name == "rangedAttack" || attr.Name == "alive")
                continue;

            GameObject iconImage = objectPool.GetAbility();
            RectTransform rectTransform = iconImage.GetComponent<RectTransform>();
            ItemInformation itemInfo = iconImage.GetComponent<ItemInformation>();
            ExplainItem explainItem = iconImage.GetComponent<ExplainItem>();

            rectTransform.anchoredPosition = new Vector2(teamPosX + (i * interval), posY);
            rectTransform.localScale = Vector3.one;

            Image img = iconImage.GetComponent<Image>();
            img.sprite = SpriteCacheManager.GetSprite($"KIcon/AbilityIcon/{attr.Name}");

            // 이름 설정
            itemInfo.isItem = false;
            itemInfo.abilityId = attr.Name == "defense"
                ? 129
                : GameTextData.GetIdxFromString(attr.Name) ?? int.Parse(attr.Name);

            explainItem.ItemToolTip =itemToolTip;

            iconImage.transform.SetParent(abilityBox.transform, false);

            i++;
        }
    }

    //능력치 아이콘 제거
    private void ClearExistingAbilityIcons()
    {
        foreach (var unit in objectPool.GetActiveAbilitys())
        {
            objectPool.ReturnAbility(unit); // 능력 아이콘 비활성화 및 반환
        }
    }

    //전투 종료
    public void FightEnd()
    {
        rewardUI.gameObject.SetActive(true);
    }

    //능력 창 띄위기
    public void CreateAbility(string ability, bool myTeam)
    {
        List<string> abilityList = new List<string>(ability.Split(' '));

        int team = myTeam ? 0 : 5;

        if (abilityList.Count > 0)
        {
            //유격 착취 제외하고
            for (int i = 0; i < abilityList.Count - 1; i++)
            {
                // 첫 번째 능력이 "유격" 또는 "착취"인 경우 team 값을 반전
                if (abilityList[0] == "유격" || abilityList[0] == "착취")
                {
                    team = (team == 0) ? 5 : 0;
                }
                Transform child = abilityPool.transform.GetChild(i + team);
                child.gameObject.SetActive(true);
                child.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = abilityList[i];
            }

        }
    }

    //유닛 사망 시 투명하게
    public void ChangeInvisibleUnit(int unitIndex, bool isMyUnit)
    {
        GameObject unit= FindUnit(unitIndex, isMyUnit);

        FadeOutUnit(unit);
    }
    // 생성된 유닛 검색
    private GameObject FindUnit(int unitIndex, bool isMyUnit)
    {
        string unitName = $"{(isMyUnit ? "MyUnit" : "EnemyUnit")}{unitIndex}";

        foreach (Transform child in canvasTransform)
        {
            if (child.name == unitName && child.gameObject.activeSelf)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    //유닛 투명
    private void FadeOutUnit(GameObject unit)
    {
        Image unitImage = unit.GetComponent<Image>();
        unitImage.DOFade(0f, waittingTime / 1000f).OnComplete(() =>
        {
            unit.SetActive(false); // 투명화 후 유닛을 비활성화
        });
    }

    //대기 시간 변경
    public void ChangeWaittingTime(float multiple)
    {
        waittingTime *= multiple;

    }

    //유산 생성
    public void CreateWarRelic()
    {
        var warRelics = RogueLikeData.Instance.GetAllOwnedRelics();

        if (warRelics == null || warRelics.Count == 0)
            return;

        for (int i = 0; i < warRelics.Count; i++)
        {
            GameObject relicObject = objectPool.GetWarRelic();
            ItemInformation itemInfo = relicObject.GetComponent<ItemInformation>();
            ExplainItem explainItem = relicObject.GetComponent<ExplainItem>();

            Image relicImg = relicObject.GetComponent<Image>();
            relicImg.sprite = SpriteCacheManager.GetSprite($"KIcon/WarRelic/{warRelics[i].id}");

            // 이름 설정
            itemInfo.isItem =false;
            itemInfo.relicId = warRelics[i].id;

            explainItem.ItemToolTip = itemToolTip;

            relicObject.transform.SetParent(relicBox.transform, false);

        }
    }

    //로딩창 간단하게 구현
    public void ToggleLoadingWindow()
    {
        loadingWindow.SetActive(!loadingWindow.activeSelf);
    }

    //테스트 화면으로
    private void ClickGoTestBtn()
    {
        SceneManager.LoadScene("Upgrade");
    }

    //보상 창 열기+ 통계,점수,승패 창 닫기 
    private void OpenRewardWindow()
    {
        SceneManager.LoadScene("RLmap");
        /*
        staticsWindow.SetActive(false);
        endWindow.SetActive(false);
        staticsToggleBtn.gameObject.SetActive(false);

        rewardWindow.SetActive(true);*/
    }
    //보상 창 닫기
    private void CloseRewardWindow()
    {
        rewardUI.gameObject.SetActive(false);
    }

    //ui 활성화 비활성화 초기화
    private void ResetUIActive()
    {
        rewardUI.gameObject.SetActive(false);
        //staticsToggleBtn.gameObject.SetActive(true);
    }

    //사기 값 수정
    public void UpdateMorale()
    {
        int morale = RogueLikeData.Instance.GetMorale();
        moraleText.text = $"{morale}";
    }

   
}

