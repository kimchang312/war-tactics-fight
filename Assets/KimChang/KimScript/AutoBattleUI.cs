using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

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

    [SerializeField] private Transform myBackUnitsParent;
    [SerializeField] private Transform enemyBackUnitsParent;  

    [SerializeField] private GameObject relicBox;
    [SerializeField] private Transform myAbilityBox;
    [SerializeField] private Transform enemyAbilityBox;

    [SerializeField] private GameObject itemToolTip;
    [SerializeField] private Image background;
    private Vector3 myTeam = new(270, 280, 0);               
    private Vector3 enemyTeam = new(-270, 280, 0);          

    private float waittingTime = 500f;

    private void Start()
    {
        int fieldId = RogueLikeData.Instance.GetFieldId();
        switch (fieldId) 
        {
            case 2:
                {
                    background.sprite = SpriteCacheManager.GetSprite("EventImages/Forest");
                    break;
                }
            case 3:
                {
                    background.sprite = SpriteCacheManager.GetSprite("EventImages/Mountain");
                    break;
                }
            case 4:
                {
                    background.sprite = SpriteCacheManager.GetSprite("EventImages/Swampland");
                    break;
                }
        }

        ResetUIActive();
        
        myHpBar.interactable = false;
        enemyHpBar.interactable = false;

        UpdateMorale();
    }
    public void UpdateUnitCountUI(int myUnitCount, int enemyUnitCount)
    {
        _myUnitCountUI.text = $"{myUnitCount}";
        _enemyUnitCountUI.text = $"{enemyUnitCount}";

    }
    public void UpateUnitHPUI(float myUnitHP, float enemyUnitHP, float myMaxHp, float enemyMaxHp)
    {
        if (_myUnitHPUI != null && _emyUnitHPUI)
        {
            _myUnitHPUI.text = $"{myUnitHP}/{myMaxHp}";
            _emyUnitHPUI.text = $"{enemyUnitHP}/{enemyMaxHp}";
        }

        myHpBar.maxValue = myMaxHp;
        myHpBar.value = myUnitHP;

        enemyHpBar.maxValue = enemyMaxHp;
        enemyHpBar.value = enemyUnitHP;
    }

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
        Vector2 displayPos = team
            ? (unitIndex == 0 ? myTeam : GetUnitPosition(unitIndex, !team, offsetX))
            : (unitIndex == 0 ? enemyTeam : GetUnitPosition(unitIndex, !team, offsetX));

        yield return new WaitForSeconds(waittingTime * 0.0005f);

        ShowDamageInternalWithPosition(damage, text, displayPos);
    }
    private Vector2 GetUnitPosition(int unitIndex, bool isMyUnit, float offsetX)
    {
        GameObject unit = FindUnit(unitIndex, isMyUnit);
        if (unit == null)
        {
            Debug.LogWarning($"유닛을 찾을 수 없음: {unitIndex}, 팀: {isMyUnit}");
            return Vector2.zero;
        }

        RectTransform unitRect = unit.GetComponent<RectTransform>();
        return unitRect.anchoredPosition + new Vector2(offsetX, 0);
    }
    private void ShowDamageInternalWithPosition(float damage, string text, Vector2 anchoredPosition)
    {
        GameObject damageObj = objectPool.GetDamageText();
        damageObj.SetActive(true);

        var damagetext = damageObj.GetComponent<TextMeshProUGUI>();
        damagetext.color = damage >= 0 ? Color.green : Color.red;
        damagetext.text = damage == 0 ? $"{text}" : $"{damage} {text}";

        RectTransform rectTransform = damageObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;

        StartCoroutine(HideAfterDelay(damageObj));
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
                Debug.Log(unit);
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
                Debug.Log(unit);
                RectTransform unitRect = unit.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = unitRect.anchoredPosition + new Vector2(offsetX, 0);
            }
        }

        CreateAbility(text, team);
        StartCoroutine(HideAfterDelay(damageObj));
    }


    private void BattleAnimation(float damage, string text, bool team,bool isAttack)
    {
        if (!isAttack) return;

        GameObject unit = FindUnit(0, team);
        RectTransform rectTransform = unit.GetComponent<RectTransform>();

        Vector2 originPos = rectTransform.anchoredPosition;
        float direction = team ? 1f : -1f;

        Vector2 moveBackPos = originPos + new Vector2(direction * -10f, 0f);
        Vector2 moveForwardPos = originPos + new Vector2(direction * 25f, 0f);

        DG.Tweening.Sequence attackSequence = DOTween.Sequence();
        attackSequence.Append(rectTransform.DOAnchorPos(moveBackPos, 0.05f))
                      .AppendInterval(0.2f)
                      .Append(rectTransform.DOAnchorPos(moveForwardPos, 0.2f))
                      .Append(rectTransform.DOAnchorPos(originPos, 0.05f));

        rectTransform.DOKill();
    }
    private IEnumerator HideAfterDelay(GameObject damageObj)
    {
        yield return new WaitForSeconds(waittingTime/1000f);
        objectPool.ReturnDamageText(damageObj);
    }

    public void CreateUnitBox(List<RogueUnitDataBase> myUnits, List<RogueUnitDataBase> enemyUnits, float myDodge, float enemyDodge, List<RogueUnitDataBase> myRangeUnits, List<RogueUnitDataBase> enemyRangeUnits)
    {
        Vector3[] myPositions = { new Vector3(-180, 94, 0), new Vector3(-131, -385, 0)};
        Vector3[] enemyPositions = { new Vector3(200, 94, 0), new Vector3(152, -385, 0) };
        Vector3 myRangeUnitPos = new Vector3(-833, -95, 0);
        Vector3 enemyRangeUnitPos = new Vector3(837, -95, 0);

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

    private void ClearExistingUnitImages()
    {
        foreach (var unit in objectPool.GetActiveBattleUnits())
        {
            objectPool.ReturnBattleUnit(unit);
        }
    }

    private void CreateRangeUnit(int rangeUnitCount, Vector3 position, GameObject number, bool isMyTeam)
    {
        string myTeam = isMyTeam ? "My" : "Enemy";

        Image numberImg = number.GetComponent<Image>();

        if (rangeUnitCount == 0)
        {
            numberImg.color = new Color(1, 1, 1, 0);
            return;
        }
        numberImg.color = new Color(1, 1, 1, 1);

        GameObject unit = objectPool.GetBattleUnit();
        unit.transform.localScale = isMyTeam ? new Vector2(1,1): new Vector2(-1,1);
        RectTransform rectTransform = unit.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Image img = unit.GetComponent<Image>();
        img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
        img.sprite = SpriteCacheManager.GetSprite("KIcon/AbilityIcon/rangedAttack");

        Transform childUnit = unit.transform.GetChild(0);
        RectTransform childRectTransform = childUnit.GetComponent<RectTransform>();
        Image childImg = childUnit.GetComponent<Image>();
        childImg.sprite = SpriteCacheManager.GetSprite($"KIcon/UI_{myTeam}SecondUnit");

        numberImg.sprite = SpriteCacheManager.GetSprite($"KIcon/UI_{myTeam}X{rangeUnitCount}");

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
            Debug.Log(i + units[i].unitName);
            GameObject unitImage = objectPool.GetBattleUnit();
            unitImage.transform.localScale = isMyUnit? new(1,1,1) : new(-1,1,1);
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

            Image img = unitImage.GetComponent<Image>();
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
            img.sprite = SpriteCacheManager.GetSprite($"UnitImages/{units[i].unitImg}");

            unitFrame.sprite = SpriteCacheManager.GetSprite($"KIcon/UI_{unitTeam}");

            unitImage.name = $"{(isMyUnit ? "My" : "Enemy")}Unit{i}";
        }

        if (isMyUnit)
        {
            _myDodge.text = $"회피율: {dodge}%";
        }
        else
        {
            _enemyDodge.text = $"회피율: {dodge}%";
        }
    }

    private void CreateAbilityIcons(RogueUnitDataBase unit, bool isTeam)
    {
        var boolAttributes = unit.GetType().GetFields()
            .Where(f => f.FieldType == typeof(bool))
            .Select(f => new { Name = f.Name, Value = (bool)f.GetValue(unit) });

        foreach (var attr in boolAttributes)
        {
            if (!attr.Value || attr.Name == "rangedAttack" || attr.Name == "alive")
                continue;

            GameObject iconImage = objectPool.GetAbility();
            ItemInformation itemInfo = iconImage.GetComponent<ItemInformation>();
            ExplainItem explainItem = iconImage.GetComponent<ExplainItem>();

            Image img = iconImage.GetComponent<Image>();
            img.sprite = SpriteCacheManager.GetSprite($"KIcon/AbilityIcon/{attr.Name}");

            itemInfo.isItem = false;
            int? idx = GameTextData.GetIdxFromString(attr.Name);
            if (attr.Name == "defense")
            {
                itemInfo.abilityId = 129;
            }
            else if (idx.HasValue)
            {
                itemInfo.abilityId = idx.Value;
            }
            else if (int.TryParse(attr.Name, out int parsedId))
            {
                itemInfo.abilityId = parsedId;
            }
            else
            {
                Debug.LogWarning($"abilityId 파싱 실패: {attr.Name}");
                itemInfo.abilityId = -1; // 혹은 예외 처리 또는 기본값 지정
            }

            explainItem.ItemToolTip =itemToolTip;
            Transform abilityBox = isTeam? myAbilityBox: enemyAbilityBox;

            iconImage.transform.SetParent(abilityBox, false);

        }
    }

    private void ClearExistingAbilityIcons()
    {
        foreach (var unit in objectPool.GetActiveAbilitys())
        {
            objectPool.ReturnAbility(unit);
        }
    }

    //전투 종료
    public void FightEnd()
    {
        rewardUI.gameObject.SetActive(true);
        rewardUI.CreateRewardUI();
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
        Transform parent = unitIndex < 2 ? canvasTransform : (isMyUnit ? myBackUnitsParent : enemyBackUnitsParent);

        foreach (Transform child in parent)
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
            if (warRelics[i].used) return;
            GameObject relicObject = objectPool.GetWarRelic();
            ItemInformation itemInfo = relicObject.GetComponent<ItemInformation>();
            ExplainItem explainItem = relicObject.GetComponent<ExplainItem>();

            Image relicImg = relicObject.GetComponent<Image>();
            relicImg.sprite = SpriteCacheManager.GetSprite($"KIcon/WarRelic/{warRelics[i].id}");

            itemInfo.isItem =false;
            itemInfo.relicId = warRelics[i].id;

            explainItem.ItemToolTip = itemToolTip;

            relicObject.transform.SetParent(relicBox.transform, false);

        }
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

