using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor.Playables;
#endif



public class AutoBattleUI : MonoBehaviour
{
    [SerializeField] private Transform canvasTransform;
    [SerializeField] private GameObject canvas;

    [SerializeField] private FightResult fightResult;           //전투 종료 시 보여주는 화면

    [SerializeField] private TextMeshProUGUI _myUnitCountUI;
    [SerializeField] private TextMeshProUGUI _enemyUnitCountUI;
    [SerializeField] private TextMeshProUGUI _myUnitHPUI;
    [SerializeField] private TextMeshProUGUI _emyUnitHPUI;

    [SerializeField] private ObjectPool objectPool;            //  obj풀링
    [SerializeField] private GameObject abilityPool;              //기술+특성 풀

    [SerializeField] private GameObject battleUnit;           // 전투 화면 유닛
    [SerializeField] private TextMeshProUGUI _myDodge;        // 내 회피율
    [SerializeField] private TextMeshProUGUI _enemyDodge;        // 상대 회피율
    [SerializeField] private TextMeshProUGUI myAbility;         // 내 특성+기술
    [SerializeField] private TextMeshProUGUI enemyAbility;      // 상대 특성+기술
    [SerializeField] private Slider myHpBar;                    // 내 체력 바
    [SerializeField] private Slider enemyHpBar;                 // 상대 체력 바
    [SerializeField] private GameObject myRangeCount;                    //내 원거리 유닛 수
    [SerializeField] private GameObject enemyRangeCount;                 //상대 원거리 유닛 수


    private Vector3 myTeam = new(270, 280, 0);                 // 아군 데미지 뜨는 위치
    private Vector3 enemyTeam = new(-270, 280, 0);           // 상대 데미지지 뜨는 위치

    private float waittingTime = 500f;        //애니메이션 대기 시간

    private void Start()
    {
        myHpBar.interactable = false;
        enemyHpBar.interactable = false;
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
        UpdateHpBar(myUnitHP, enemyUnitHP, myMaxHp, enemyMaxHp);
    }

    //유닛 체력 바
    private void UpdateHpBar(float myUnitHP, float enemyUnitHP, float myMaxHp, float enemyMaxHp)
    {
        myHpBar.maxValue = myMaxHp;
        myHpBar.value = myUnitHP;

        enemyHpBar.maxValue = enemyMaxHp;
        enemyHpBar.value = enemyUnitHP;
    }


    //데미지 표시
    public void ShowDamage(float damage, string text, bool team)
    {
        GameObject damageObj = objectPool.GetDamageText();
        TextMeshProUGUI damagetext = damageObj.GetComponent<TextMeshProUGUI>();

        damagetext.text = $"-{damage} {text}";

        //team = true== 나 false == 상대
        if (team)
        {
            RectTransform rectTransform = damageObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = myTeam;
        }
        else
        {
            RectTransform rectTransform = damageObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = enemyTeam;
        }

        //
        CreateAbility(text, team);

        // 데미지 제거
        StartCoroutine(HideAfterDelay(damageObj));
    }

    private IEnumerator HideAfterDelay(GameObject damageObj)
    {
        yield return new WaitForSeconds(waittingTime/1000f);
        objectPool.ReturnDamageText(damageObj);
    }


    //유닛 갯수 만큼 유닛 이미지 생성
    public void CreateUnitBox(List<UnitDataBase> myUnits, List<UnitDataBase> enemyUnits, float myDodge, float enemyDodge, List<UnitDataBase> myRangeUnits, List<UnitDataBase> enemyRangeUnits)
    {
        Vector3[] myPositions = { new Vector3(-180, 94, 0), new Vector3(-131, -385, 0), new Vector3(-397, -385, 0) };
        Vector3[] enemyPositions = { new Vector3(200, 94, 0), new Vector3(152, -385, 0), new Vector3(430, -385, 0) };
        Vector3 myRangeUnitPos = new Vector3(-833, -143, 0);
        Vector3 enemyRangeUnitPos = new Vector3(837, -143, 0);
        //Vector3 myRangeTextPos = new Vector3(-837, -59, 0);
        //Vector3 enemyRangeTextPos = new Vector3(837, -59, 0);

        float firstSize = 250;
        float secondSize = 200;
        float unitInterval = 210;


        //유닛 초기화
        ClearExistingUnitImages();

        //능력 아이콘 초기화
        ClearExistingAbilityIcons();

        //능력 아이콘 생성
        CreateAbilityIcons(myUnits[0], enemyUnits[0]);

        // 나의 유닛 생성
        CreateUnitImages(myUnits, myPositions, firstSize, secondSize, unitInterval, true, myDodge);

        //나의 원거리 유닛 생성
        CreateRangeUnit(myRangeUnits.Count, myRangeUnitPos, myRangeCount, true);

        // 적의 유닛 생성
        CreateUnitImages(enemyUnits, enemyPositions, firstSize, secondSize, unitInterval, false, enemyDodge);

        //상대 원거리 유닛 생성
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
        float size = 200;
        string myTeam = isMyTeam ? "My" : "Enemy";
        Image numberImg = number.GetComponent<Image>();

        if (rangeUnitCount == 0)
        {
            numberImg.color = new Color(255, 255, 255, 0);
            return;
        }
        numberImg.color = new Color(255, 255, 255, 255);

        GameObject unit = objectPool.GetBattleUnit();
        RectTransform rectTransform = unit.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(size - 10, size - 10);
        rectTransform.anchoredPosition = position;

        // 이미지 설정 & 투명도 정상화
        Sprite sprite = Resources.Load<Sprite>($"KIcon/rangedAttack");
        Image img = unit.GetComponent<Image>();
        Color originalColor = img.color;
        img.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        img.sprite = sprite;

        //자식 관리
        Transform childUnit = unit.transform.GetChild(0);
        RectTransform childReckTransform = childUnit.GetComponent<RectTransform>();
        Image childImg = childUnit.GetComponent<Image>();

        childReckTransform.sizeDelta = new Vector2(size, size);
        Sprite childSprite = Resources.Load<Sprite>($"KIcon/UI_{myTeam}SecondUnit");
        childImg.sprite = childSprite;

        //숫자 관리
        Sprite numberSprite = Resources.Load<Sprite>($"KIcon/UI_{myTeam}X{rangeUnitCount}");
        numberImg.sprite = numberSprite;

        //숫자 이미지 뒤로
        int siblingCount = number.transform.parent.childCount;
        number.transform.SetSiblingIndex(siblingCount - 1);

        //유닛 이름 변경
        unit.name = $"{myTeam}RangeUnit";
    }

    //유닛 이미지 생성
    private void CreateUnitImages(List<UnitDataBase> units, Vector3[] positions, float firstSize, float secondSize, float unitInterval, bool isMyUnit, float dodge)
    {
        string ability = "";
        int unitIndex = 0;

        for (int i = 0; (i < units.Count || unitIndex < units.Count); i++, unitIndex++)
        {
            string unitTeam = isMyUnit ? "My" : "Enemy";
            if (units[unitIndex].health > 0)
            {
                GameObject unitImage = objectPool.GetBattleUnit();
                Transform childUnit = unitImage.transform.GetChild(0);
                RectTransform rectTransform = unitImage.GetComponent<RectTransform>();
                RectTransform childRectTrasform = childUnit.GetComponent<RectTransform>();
                Image unitFrame = childUnit.GetComponent<Image>();

                // 위치와 크기 설정
                if (i < positions.Length)
                {
                    rectTransform.anchoredPosition = positions[i];
                    switch (i)
                    {
                        case 0:
                            unitTeam += "FirstUnit";
                            break;
                        case 1:
                            unitTeam += "SecondUnit";
                            break;
                        case 2:
                            unitTeam += "BackUnit";
                            break;
                    }

                }
                else
                {
                    float offsetX = isMyUnit ? -unitInterval : unitInterval;
                    rectTransform.anchoredPosition = new Vector3(positions[2].x + offsetX * (i - positions.Length + 1), positions[2].y);

                    unitTeam += "BackUnit";
                }
                //크기 설정
                rectTransform.sizeDelta = i == 0 ? new Vector2(firstSize - 10, firstSize - 10) : new Vector2(secondSize - 10, secondSize - 10);
                childRectTrasform.sizeDelta = i == 0 ? new Vector2(firstSize, firstSize) : new Vector2(secondSize, secondSize);

                // 이미지 설정 & 투명도 정상화
                Sprite sprite = Resources.Load<Sprite>($"UnitImages/{units[unitIndex].unitImg}");
                Image img = unitImage.GetComponent<Image>();
                Color originalColor = img.color;
                img.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f); 
                img.sprite = sprite;

                //유닛 테두리 설정
                Sprite frameSprite = Resources.Load<Sprite>($"KIcon/UI_{unitTeam}");
                //활성화
                unitFrame.sprite = frameSprite;

                //유닛 이름 설정
                unitImage.name = $"{(isMyUnit ? "My" : "Enemy")}Unit{unitIndex}";
            }
            else
            {
                i--;
            }

        }

        // 회피율 UI 업데이트
        if (isMyUnit)
        {
            _myDodge.text = $"회피율: {dodge}%";
            myAbility.text = ability;
        }
        else
        {
            _enemyDodge.text = $"회피율: {dodge}%";
            enemyAbility.text = ability;
        }
    }

    //능력치 아이콘 생성
    private void CreateAbilityIcons(UnitDataBase myUnit, UnitDataBase enemyUnit)
    {
        float posY = -103;
        float myTeamPosX = -280;
        float enemyTeamPosX = 88;
        float interval = 62;

        // 내 능력 아이콘 업데이트
        var myBoolAttributes = myUnit.GetType().GetFields()
            .Where(f => f.FieldType == typeof(bool))
            .Select(f => new { Name = f.Name, Value = (bool)f.GetValue(myUnit) });

        int i = 0;

        foreach (var attr in myBoolAttributes)
        {
            if (attr.Value)
            {
                //원거리 특성 제외
                if (attr.Name != "rangedAttack")
                {
                    GameObject iconImage = objectPool.GetAbility();
                    RectTransform rectTransform = iconImage.GetComponent<RectTransform>();

                    rectTransform.anchoredPosition = new Vector2(myTeamPosX + (i * interval), posY);

                    //크기 초기화
                    rectTransform.localScale = Vector3.one;

                    // 이미지 설정
                    Sprite sprite = Resources.Load<Sprite>($"KIcon/{attr.Name}");
                    Image img = iconImage.GetComponent<Image>();
                    img.sprite = sprite;

                    i++;
                }
            }
        }


        //상대 능력 아이콘 업데이트
        var enemyBoolAttributes = enemyUnit.GetType().GetFields()
            .Where(f => f.FieldType == typeof(bool))
            .Select(f => new { Name = f.Name, Value = (bool)f.GetValue(enemyUnit) });

        i = 0;

        foreach (var attr in enemyBoolAttributes)
        {
            if (attr.Value)
            {
                //원거리 특성 제외
                if (attr.Name != "rangedAttack")
                {
                    GameObject iconImage = objectPool.GetAbility();
                    RectTransform rectTransform = iconImage.GetComponent<RectTransform>();

                    rectTransform.anchoredPosition = new Vector2(enemyTeamPosX + (i * interval), posY);

                    //크기 초기화
                    rectTransform.localScale = Vector3.one;

                    // 이미지 설정
                    Sprite sprite = Resources.Load<Sprite>($"KIcon/{attr.Name}");
                    Image img = iconImage.GetComponent<Image>();
                    img.sprite = sprite;

                    i++;
                }

            }
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
    public void FightEnd(int result)
    {
        fightResult.EndGame(result);
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
        if (unit==null)
        {
            Debug.Log($"{unitIndex}{isMyUnit}");
            Debug.Log("유닛 비어있음 오류임");
            
        }
        FadeOutUnit(unit);
    }

    // 유닛 검색 (활성화된 유닛만 찾음)
    private GameObject FindUnit(int unitIndex, bool isMyUnit)
    {
        // 유닛 이름 설정 (MyUnit0, MyUnit1, EnemyUnit0, EnemyUnit1 등)
        string unitName = $"{(isMyUnit ? "MyUnit" : "EnemyUnit")}{unitIndex}";

        // Canvas의 자식들 중 활성화된 유닛만 검색
        foreach (Transform child in canvasTransform)
        {
            if (child.name == unitName && child.gameObject.activeSelf)
            {
                return child.gameObject; // 활성화된 유닛 발견 시 반환
            }
        }

        // 유닛이 없거나 비활성화된 경우 null 반환
        return null;
    }


    //유닛 투명
    private void FadeOutUnit(GameObject unit)
    {
        Image unitImage = unit.GetComponent<Image>();
            unitImage.DOFade(0f, waittingTime/1000f).OnComplete(() =>
            {
                unit.SetActive(false); // 투명화 후 유닛을 비활성화
            });
    }

    //대기 시간 변경
    public void ChangeWaittingTime(float multiple)
    {
        waittingTime *= multiple;

    }
}

