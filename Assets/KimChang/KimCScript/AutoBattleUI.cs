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
    [SerializeField] private Slider myHpBar;                // 내 체력 바
    [SerializeField] private Slider myEnemyHpBar;           // 상대 체력 바


    private Vector3 myTeam = new(270, 280, 0);                 // 아군 데미지 뜨는 위치
    private Vector3 enemyTeam = new(-270, 280, 0);           // 상대 데미지지 뜨는 위치


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
    }

    //유닛 체력 바
    private void UpdateHpBar(float myUnitHP, float enemyUnitHP, float myMaxHp, float enemyMaxHp)
    {
 
    }

    //유닛 이름
    public void UpdateName(string myUnitName,string enemyUnitName)
    {
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
        Vector3[] myPositions = { new Vector3(-270, 150, 0), new Vector3(-130, -400, 0), new Vector3(-520, -400, 0) };
        Vector3[] enemyPositions = { new Vector3(270, 150, 0), new Vector3(130, -400, 0), new Vector3(520, -400, 0) };
        Vector3 myRangeUnitPos = new Vector3(-810, -84, 0);
        Vector3 enemyRangeUnitPos = new Vector3(810, -84, 0);

        float firstSize = 300;
        float secondSize = 180;
        float unitInterval = 100;

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

            if (i == 0)
            {
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

}

