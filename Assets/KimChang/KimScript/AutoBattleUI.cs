using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.Playables;
#endif



public class AutoBattleUI : MonoBehaviour
{
    [SerializeField] private Transform canvasTransform;

    [SerializeField] private FightResult fightResult;           //전투 종료 시 보여주는 화면

    [SerializeField] private TextMeshProUGUI _myUnitCountUI;
    [SerializeField] private TextMeshProUGUI _enemyUnitCountUI;
    [SerializeField] private TextMeshProUGUI _myUnitHPUI;
    [SerializeField] private TextMeshProUGUI _emyUnitHPUI;

    [SerializeField] private ObjectPool objectPool;         //  obj풀링

    [SerializeField] private GameObject battleUnit;         // 전투 화면 유닛
    [SerializeField] private TextMeshProUGUI _myDodge;       // 내 회피율
    [SerializeField] private TextMeshProUGUI _enemyDodge;    // 상대 회피율
    [SerializeField] private TextMeshProUGUI myAbility;     // 내 특성+기술
    [SerializeField] private TextMeshProUGUI enemyAbility;  // 상대 특성+기술
    [SerializeField] private Slider myHpBar;                // 내 체력 바
    [SerializeField] private Slider enemyHpBar;           // 상대 체력 바
    [SerializeField] private TextMeshProUGUI myRangeText;   //내 원거리 유닛 수
    [SerializeField] private TextMeshProUGUI enemyRangeText;    //상대 원거리 유닛 수
    [SerializeField] private GameObject myRangeCount;                    //내 원거리 유닛 수
    [SerializeField] private GameObject enemyRangeCount;                 //상대 원거리 유닛 수


    private Vector3 myTeam = new(270, 280, 0);                 // 아군 데미지 뜨는 위치
    private Vector3 enemyTeam = new(-270, 280, 0);           // 상대 데미지지 뜨는 위치


    private void Start()
    {
        myHpBar.interactable = false;
        enemyHpBar.interactable = false;
    }


    //유닛 갯수 
    public void UpdateUnitCountUI(int myUnitCount,int enemyUnitCount)
    {
         _myUnitCountUI.text = $"{myUnitCount}";
         _enemyUnitCountUI.text = $"{enemyUnitCount}";

    }


    //유닛 체력
    public void UpateUnitHPUI(float myUnitHP,float enemyUnitHP,float myMaxHp,float enemyMaxHp)
    {
        if (_myUnitHPUI != null && _emyUnitHPUI)
        {
            _myUnitHPUI.text = $"{myUnitHP}/{myMaxHp}";
            _emyUnitHPUI.text = $"{enemyUnitHP}/{enemyMaxHp}";
        }

        //체력바 업데이트
        UpdateHpBar(myUnitHP,enemyUnitHP,myMaxHp,enemyMaxHp);
    }

    //유닛 체력 바
    private void UpdateHpBar(float myUnitHP, float enemyUnitHP, float myMaxHp, float enemyMaxHp)
    {
        myHpBar.maxValue = myMaxHp;
        myHpBar.value = myUnitHP;

        enemyHpBar.maxValue =enemyMaxHp;
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

        // 데미지 제거
        StartCoroutine(HideAfterDelay(damageObj));
    }

    private IEnumerator HideAfterDelay(GameObject damageObj)
    {
        yield return new WaitForSeconds(0.5f);
        objectPool.ReturnDamageText(damageObj);
    }


    //유닛 갯수 만큼 유닛 이미지 생성
    public void CreateUnitBox(UnitDataBase[] myUnits,UnitDataBase[] enemyUnits,int myUnitIndex,int enemyUnitIndex,float myDodge,float enemyDodge,int myRangeUnitCount,int enemyRangeUnitCount)
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

        //아이콘 생성
        CreateAbilityIcons(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex]);

        // 나의 유닛 생성
        CreateUnitImages(myUnits, myUnitIndex, myPositions, firstSize, secondSize, unitInterval, true, myDodge);

        //나의 원거리 유닛 생성
        CreateRangeUnit(myRangeUnitCount, myRangeUnitPos,myRangeCount,true);

        // 적의 유닛 생성
        CreateUnitImages(enemyUnits, enemyUnitIndex, enemyPositions, firstSize, secondSize, unitInterval, false, enemyDodge);

        //상대 원거리 유닛 생성
        CreateRangeUnit(enemyRangeUnitCount, enemyRangeUnitPos, enemyRangeCount,false);

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
    private void CreateRangeUnit(int rangeUnitCount, Vector3 position,GameObject number,bool isMyTeam)
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
        rectTransform.sizeDelta = new Vector2(size-10,size-10);
        rectTransform.anchoredPosition = position;

        // 이미지 설정
        Sprite sprite = Resources.Load<Sprite>($"KIcon/rangedAttack");
        Image img = unit.GetComponent<Image>();
        img.sprite = sprite;

        //자식 관리
        Transform childUnit = unit.transform.GetChild(0);
        RectTransform childReckTransform= childUnit.GetComponent<RectTransform>();
        Image childImg = childUnit.GetComponent<Image>();

        childReckTransform.sizeDelta = new Vector2(size, size);
        Sprite childSprite = Resources.Load<Sprite>($"KIcon/UI_{myTeam}SecondUnit");
        childImg.sprite = childSprite;

        //숫자 관리
        Sprite numberSprite= Resources.Load<Sprite>($"KIcon/UI_{myTeam}X{rangeUnitCount}");
        numberImg.sprite = numberSprite;

        //숫자 이미지 뒤로
        int siblingCount = number.transform.parent.childCount;
        number.transform.SetSiblingIndex(siblingCount-1);
    }

    //유닛 이미지 생성
    private void CreateUnitImages(UnitDataBase[] units, int unitIndex, Vector3[] positions, float firstSize, float secondSize, float unitInterval, bool isMyUnit, float dodge)
    {
        string ability = "";
        

        for (int i = 0; unitIndex < units.Length; i++, unitIndex++)
        {
            string unitTeam = isMyUnit ? "My" : "Enemy";

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
                    case 2 :
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
            rectTransform.sizeDelta = i == 0 ? new Vector2(firstSize-10, firstSize-10) : new Vector2(secondSize-10, secondSize-10);
            childRectTrasform.sizeDelta =i ==0 ? new Vector2(firstSize, firstSize) : new Vector2 (secondSize, secondSize);

            // 이미지 설정
            Sprite sprite = Resources.Load<Sprite>($"UnitImages/{units[unitIndex].unitImg}");
            Image img = unitImage.GetComponent<Image>();
            img.sprite = sprite;

            //유닛 테두리 설정
            Sprite frameSprite = Resources.Load<Sprite>($"KIcon/UI_{unitTeam}");
            //활성화
            unitFrame.sprite = frameSprite;

            if (i == 0)
            {
                // UI 업데이트
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

            
        }
    }

    //능력치 아이콘 생성
    private void CreateAbilityIcons(UnitDataBase myUnit,UnitDataBase enemyUnit)
    {
        float posY = -103;
        float myTeamPosX = -280;
        float enemyTeamPosX =88;
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
                if(attr.Name != "rangedAttack")
                {
                    GameObject iconImage = objectPool.GetAbility();
                    RectTransform rectTransform = iconImage.GetComponent<RectTransform>();

                    rectTransform.anchoredPosition = new Vector2(myTeamPosX - (i * interval), posY);

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

}

