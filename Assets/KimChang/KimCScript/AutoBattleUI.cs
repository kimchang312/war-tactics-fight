using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AutoBattleUI : MonoBehaviour
{
    [SerializeField] private Transform canvasTransform;

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

    private Vector3 myTeam = new(11, 0, 0);                 // 아군 데미지 뜨는 위치
    private Vector3 enemyTeam = new(6, 0, 0);           // 상대 데미지지 뜨는 위치


    //유닛 갯수 
    public void UpdateUnitCountUI(int myUnitCount,int enemyUnitCount)
    {
        if (_myUnitCountUI != null && _enemyUnitCountUI)
        {
            _myUnitCountUI.text = $"{myUnitCount}";
            _enemyUnitCountUI.text = $"{enemyUnitCount}";
        }
    }


    //유닛 체력
    public void UpateUnitHPUI(float myUnitHP,float enemyUnitHP)
    {
        if (_myUnitHPUI != null && _emyUnitHPUI)
        {
            _myUnitHPUI.text = $"{myUnitHP}";
            _emyUnitHPUI.text = $"{enemyUnitHP}";
        }
    }

    //유닛 이름
    public void UpdateName(string myUnitName,string enemyUnitName)
    {
    }

    //데미지 표시
    public void ShowDamage(float damage, string text, bool team)
    {
        GameObject damageObj = objectPool.GetDamageText();
        TextMeshPro damagetext = damageObj.GetComponent<TextMeshPro>();

        damagetext.text = $"-{damage} {text}";

        //team = true== 나 false == 상대
        damageObj.transform.position = team ? myTeam : enemyTeam;
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
        /*
        Vector3 myFirstPos= new(-270,150,0);
        Vector3 enemyFirstPos = new(270, 150, 0);
        Vector3 mySecondPos = new(-130,-400,0);
        Vector3 myThirdPos = new(-520,-400,0);
        Vector3 enemySecondPos = new(130, -400, 0);
        Vector3 enemyThirdPos = new(520, -400, 0);
        Image img;
        RectTransform rectTransform ;
        Sprite sprite;
        string ability="";
        */

        Vector3[] myPositions = { new Vector3(-270, 150, 0), new Vector3(-130, -400, 0), new Vector3(-520, -400, 0) };
        Vector3[] enemyPositions = { new Vector3(270, 150, 0), new Vector3(130, -400, 0), new Vector3(520, -400, 0) };
        Vector3 myRangeUnitPos = new Vector3(-810, -84, 0);
        Vector3 enemyRangeUnitPos = new Vector3(810, -84, 0);

        float firstSize = 300;
        float secondSize = 180;
        float unitInterval = 50;

        //유닛 초기화
        ClearExistingUnitImages();

        // 나의 유닛 생성
        CreateUnitImages(myUnits, myUnitIndex, myPositions, firstSize, secondSize, unitInterval, true, myDodge);

        //나의 원거리 유닛 생성
        CreateRangeUnit(myRangeUnitCount, myRangeUnitPos);

        // 적의 유닛 생성
        CreateUnitImages(enemyUnits, enemyUnitIndex, enemyPositions, firstSize, secondSize, unitInterval, false, enemyDodge);

        //상대 원거리 유닛 생성
        CreateRangeUnit(enemyRangeUnitCount, myRangeUnitPos);

        /*
        //if (myUnits.Length > myUnitIndex && enemyUnits.Length > enemyUnitIndex)
        //{
            //나의 유닛 생성
            //0번
            GameObject myFirstUnit = Instantiate(battleUnit, canvasTransform);
            rectTransform = myFirstUnit.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = myFirstPos;
        rectTransform.sizeDelta = new Vector2(firstSize, firstSize);
        sprite = Resources.Load<Sprite>($"UnitImages/{myUnits[myUnitIndex].unitImg}");
            img = myFirstUnit.GetComponent<Image>();
            img.sprite = sprite;
            _myDodge.text = $"회피율 {myDodge}%";
            var unit = myUnits[myUnitIndex];
            var boolAttributes = unit.GetType().GetFields()
                .Where(f => f.FieldType == typeof(bool))
                .Select(f => new { Name = f.Name, Value = (bool)f.GetValue(unit) });
            foreach (var attr in boolAttributes)
            {
                if (attr.Value)
                {
                    ability +=$"{attr.Name}\n";
                }
            }

            myAbility.text = ability;

            myUnitIndex++;
            //1번
            if (myUnits.Length > myUnitIndex)
            {
                GameObject mySecondUnit = Instantiate(battleUnit, canvasTransform);
                rectTransform = mySecondUnit.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = mySecondPos;
                rectTransform.sizeDelta = new Vector2(secondSize, secondSize);
                sprite = Resources.Load<Sprite>($"UnitImages/{myUnits[myUnitIndex].unitImg}");
                img = mySecondUnit.GetComponent<Image>();
                img.sprite = sprite;

                myUnitIndex++;
                
                if(myUnits.Length > myUnitIndex)
                {
                int k = 1;
                    for (int i = myUnitIndex; i < myUnits.Length; i++) 
                    {
                        GameObject myAfterUnit = Instantiate(battleUnit, canvasTransform);
                        rectTransform = myAfterUnit.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(myThirdPos.x - (unitInterval*k), myThirdPos.y);
                    sprite = Resources.Load<Sprite>($"UnitImages/{myUnits[myUnitIndex].unitImg}");
                    img = myAfterUnit.GetComponent<Image>();
                    img.sprite = sprite;

                    k++;
                    }

                }
                
            }

            //상대 유닛 생성
            //0번
            GameObject enemyFirstUnit = Instantiate(battleUnit, canvasTransform);
            rectTransform = enemyFirstUnit.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = enemyFirstPos;
        rectTransform.sizeDelta = new Vector2(firstSize, firstSize);
        sprite = Resources.Load<Sprite>($"UnitImages/{enemyUnits[enemyUnitIndex].unitImg}");
            img = enemyFirstUnit.GetComponent<Image>();
            img.sprite = sprite;

            unit = enemyUnits[enemyUnitIndex];
            boolAttributes = unit.GetType().GetFields()
                .Where(f => f.FieldType == typeof(bool))
                .Select(f => new { Name = f.Name, Value = (bool)f.GetValue(unit) });
            foreach (var attr in boolAttributes)
            {
                if (attr.Value)
                {
                    ability += $"{attr.Name}\n";
                }
            }

            enemyAbility.text = ability;

            enemyUnitIndex++;
            //1번
            if (enemyUnits.Length > enemyUnitIndex)
            {
                GameObject enemySecondUnit = Instantiate(battleUnit, canvasTransform);
                rectTransform = enemySecondUnit.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = enemySecondPos;
                rectTransform.sizeDelta = new Vector2(secondSize, secondSize);
                sprite = Resources.Load<Sprite>($"UnitImages/{enemyUnits[enemyUnitIndex].unitImg}");
                img = enemySecondUnit.GetComponent<Image>();
                img.sprite = sprite;

                enemyUnitIndex++;

            if (enemyUnits.Length > enemyUnitIndex)
            {
                int k = 1;
                for (int i = enemyUnitIndex; i < enemyUnits.Length; i++)
                {
                    GameObject enemyAfterUnit = Instantiate(battleUnit, canvasTransform);
                    rectTransform = enemyAfterUnit.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(enemyThirdPos.x + (unitInterval * k), enemyThirdPos.y);
                    sprite = Resources.Load<Sprite>($"UnitImages/{enemyUnits[enemyUnitIndex].unitImg}");
                    img = enemyAfterUnit.GetComponent<Image>();
                    img.sprite = sprite;

                    k++;
                }

            }

        }
        */
        //}
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
    private void CreateRangeUnit(int rangeUnitCount, Vector3 position)
    {
        if(rangeUnitCount == 0) return;
        GameObject unit = objectPool.GetBattleUnit();
        RectTransform rectTransform = unit.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = position;

        // 이미지 설정
        Sprite sprite = Resources.Load<Sprite>($"UnitImages/Unit_Img_3");
        Image img = unit.GetComponent<Image>();
        img.sprite = sprite;

    }

    //유닛 이미지 생성
    private void CreateUnitImages(UnitDataBase[] units, int unitIndex, Vector3[] positions, float firstSize, float secondSize, float unitInterval, bool isMyUnit, float dodge)
    {
        string ability = "";

        for (int i = 0; unitIndex < units.Length; i++, unitIndex++)
        {
            GameObject unitImage = objectPool.GetBattleUnit();
            RectTransform rectTransform = unitImage.GetComponent<RectTransform>();

            // 위치와 크기 설정
            if (i < positions.Length)
            {
                rectTransform.anchoredPosition = positions[i];
            }
            else
            {
                float offsetX = isMyUnit ? -unitInterval : unitInterval;
                rectTransform.anchoredPosition = new Vector3(positions[2].x + offsetX * (i - positions.Length + 1), positions[2].y);
            }
            rectTransform.sizeDelta = i == 0 ? new Vector2(firstSize, firstSize) : new Vector2(secondSize, secondSize);

            // 이미지 설정
            Sprite sprite = Resources.Load<Sprite>($"UnitImages/{units[unitIndex].unitImg}");
            Image img = unitImage.GetComponent<Image>();
            img.sprite = sprite;

            // 능력치 업데이트
            var unit = units[unitIndex];
            var boolAttributes = unit.GetType().GetFields()
                .Where(f => f.FieldType == typeof(bool))
                .Select(f => new { Name = f.Name, Value = (bool)f.GetValue(unit) });

            foreach (var attr in boolAttributes)
            {
                if (attr.Value)
                {
                    ability += $"{attr.Name}\n";
                }
            }

            // UI 업데이트
            if (isMyUnit)
            {
                _myDodge.text = $"회피율 {dodge}%";
                myAbility.text = ability;
            }
            else
            {
                _enemyDodge.text = $"회피율 {dodge}%";
                enemyAbility.text = ability;
            }
        }
    }

}

