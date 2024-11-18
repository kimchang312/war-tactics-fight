using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private TextMeshPro _myUnitName;
    [SerializeField] private TextMeshPro _enemyUnitName;

    [SerializeField] private ObjectPool objectPool;         //  obj풀링

    [SerializeField] private GameObject battleUnit;         // 전투 화면 유닛

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
        _myUnitName.text = $"{myUnitName}";
        _enemyUnitName.text=$"{enemyUnitName}";
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
    public void CreateUnitBox(UnitDataBase[] myUnits,UnitDataBase[] enemyUnits,int myUnitIndex,int enemyUnitIndex)
    {
        Vector3 myFirstPos= new(-270,150,0);
        Vector3 enemyFirstPos = new(270, 150, 0);
        Vector3 mySecondPos = new(-130,-400,0);
        Vector3 myThirdPos = new(-520,-400,0);
        Vector3 enemySecondPos = new(130, -400, 0);
        Vector3 enemyThirdPos = new(520, -400, 0);

        Image img;
        RectTransform rectTransform ;
        Sprite sprite;

        float firstSize = 300;
        float secondSize = 180;
        float unitInterval = 50;

        if (myUnits.Length > myUnitIndex && enemyUnits.Length > enemyUnitIndex)
        {
            //나의 유닛 생성
            //0번
            GameObject myFirstUnit = Instantiate(battleUnit, canvasTransform);
            rectTransform = myFirstUnit.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = myFirstPos;
            sprite = Resources.Load<Sprite>($"UnitImages/{myUnits[myUnitIndex].unitImg}");
            img = myFirstUnit.GetComponent<Image>();
            img.sprite = sprite;

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
                /*
                if(myUnits.Length > myUnitIndex)
                {
                    for (int i = myUnitIndex; i < myUnits.Length; i++) 
                    {
                        GameObject myUnit = Instantiate(battleUnit, canvasTransform);
                        rectTransform =myUnit.GetComponent<RectTransform>();
                        rectTransform.anchoredPosition=
                    }

                }
                */
            }

            //상대 유닛 생성
            //0번
            GameObject enemyFirstUnit = Instantiate(battleUnit, canvasTransform);
            rectTransform = enemyFirstUnit.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = enemyFirstPos;
            sprite = Resources.Load<Sprite>($"UnitImages/{enemyUnits[enemyUnitIndex].unitImg}");
            img = enemyFirstUnit.GetComponent<Image>();
            img.sprite = sprite;

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
                
            }

        }
    }
}

