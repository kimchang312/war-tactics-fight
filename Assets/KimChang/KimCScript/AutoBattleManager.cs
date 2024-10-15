using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;


public class AutoBattleManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI myUnitContUI;      //내 유닛수UI에 연결할 변수
    [SerializeField] private TextMeshProUGUI enemyUnitCountUI;  //상대 유닛수UI에 연결할 변수
    [SerializeField] private TextMeshProUGUI myUnitHpUI;        //내 유닛 HP UI에 연결할 변수
    [SerializeField] private TextMeshProUGUI enemyUnitHpUI;     //상대 유닛 HP UI에 연결할 변수
    private float waittingTime=0.2f; //    

    // 유닛 id들로 유닛 데이터를 정리하는 함수
    // Tuple을 사용하여 두 개의 배열을 반환
    private (UnitDataBase[] myUnits, UnitDataBase[] enemyUnits) GetUnits(int[] myUnitIds, int[] enemyUnitIds)
    {
        // 내가 선택한 유닛들 저장 (List를 사용)
        List<UnitDataBase> myUnits = new List<UnitDataBase>();

        // 적의 유닛 저장
        List<UnitDataBase> enemyUnits = new List<UnitDataBase>();


        // 내 유닛 ID들을 기반으로 유닛을 가져와서 MyUnits에 저장
        foreach (int unitId in myUnitIds)
        {
            UnitDataBase unit = UnitDataBase.GetUnitById(unitId);
            if (unit != null)
            {
                myUnits.Add(unit);  // List에 유닛 추가
            }
        }

        // 적의 유닛 ID들을 기반으로 유닛을 가져와서 enemyUnits에 저장
        foreach (int unitId in enemyUnitIds)
        {
            UnitDataBase unit = UnitDataBase.GetUnitById(unitId);
            if (unit != null)
            {
                enemyUnits.Add(unit);  // List에 유닛 추가
            }
        }

        // List를 배열로 변환한 후 튜플로 반환
        return (myUnits.ToArray(), enemyUnits.ToArray());
    }


    //자동전투
    private int AutoBattle(int[] _myUnitIds, int[] _enemyUnitIds)
    {

        // 나의 피해량
        float myDamage;
        // 적의 피해량
        float enemyDamage;

        // 나와 적의 유닛을 호출
        var units = GetUnits(_myUnitIds, _enemyUnitIds);
        UnitDataBase[] myUnits = units.myUnits;
        UnitDataBase[] enemyUnits = units.enemyUnits;

        // 인덱스를 사용해서 현재 전투에 참여하는 유닛 추적
        int myUnitIndex = 0;
        int enemyUnitIndex = 0;

        //최초의 유닛 갯수
        int myUnitMax= _myUnitIds.Length;
        int enemyUnitMax= _enemyUnitIds.Length;



        //유닛 수 UI 초기화 함수 호출
        UpdateUnitCount(_myUnitIds.Length,_enemyUnitIds.Length);

        //유닛 Hp UI 초기화 함수 호출
        UpdateUnitHp(myUnits[0].health, enemyUnits[0].health);

        // 전투 반복
        while (myUnitIndex < myUnits.Length && enemyUnitIndex < enemyUnits.Length)
        {
            // 1차적인 데미지 계산: 데미지 = 공격력 * (1 - 적의 장갑 / (10 + 적의 장갑))
            myDamage = myUnits[myUnitIndex].attackPower * (1 - (enemyUnits[enemyUnitIndex].armor / (10 + enemyUnits[enemyUnitIndex].armor)));
            enemyDamage = enemyUnits[enemyUnitIndex].attackPower * (1 - (myUnits[myUnitIndex].armor / (10 + myUnits[myUnitIndex].armor)));

            // 체력 감소
            myUnits[myUnitIndex].health -= enemyDamage;
            enemyUnits[enemyUnitIndex].health -= myDamage;

            // 유닛의 체력이 0 이하일 경우, 다음 유닛으로 넘어감
            if (myUnits[myUnitIndex].health <= 0)
            {
                Debug.Log("내 유닛" + myUnits[myUnitIndex].name+"사망");
                myUnitIndex++;  // 다음 내 유닛
            }
            if (enemyUnits[enemyUnitIndex].health <= 0)
            {
                Debug.Log("적 유닛" + enemyUnits[enemyUnitIndex].name + "사망");
                enemyUnitIndex++;  // 다음 적 유닛
            }

            UpdateUnitCount(myUnitMax - myUnitIndex, enemyUnitMax - enemyUnitIndex);
            if(myUnitIndex== myUnitMax)
            {
                UpdateUnitHp(myUnits[myUnitMax - 1].health, enemyUnits[enemyUnitIndex].health);
            }
            else if (enemyUnitIndex==enemyUnitMax)
            {
                UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitMax-1].health);
            }
            else
            {
                UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
            }
            

            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(WaitForSecondsExample());
            }        
        }

        // 전투 종료 후 승리 여부 판단
        if (myUnitIndex < myUnits.Length && enemyUnitIndex >= enemyUnits.Length)
        {
            return 0;  // 내가 승리
        }
        else if (enemyUnitIndex < enemyUnits.Length && myUnitIndex >= myUnits.Length)
        {
            return 1;  // 적이 승리
        }
        else
        {
            return 2;  // 양쪽 모두 사망
        }
    }


    //자동전투를 다른코드에서 호출할수 있게끔 대신 호출해주는 함수
    public int StartBattle(int[] _myUnitIds, int[] _enemyUnitIds)
    {
        int result = AutoBattle(_myUnitIds,_enemyUnitIds);
        return result;
    }

    //유닛 수 UI 초기화 함수
    private void UpdateUnitCount(int myUnitLength,int enemyUnitLength)
    {
        myUnitContUI.text= $"{myUnitLength}";
        enemyUnitCountUI.text = $"{enemyUnitLength}";
    }


    //유닛 Hp UI 초기화 함수
    private void UpdateUnitHp(float myUnitHp,float enemyHp)
    {
        if(myUnitHp < 0)
        {
            myUnitHp = 0;
        }
        if (enemyHp < 0)
        {
            enemyHp = 0;
        }
        myUnitHpUI.text=$"{ Math.Floor(myUnitHp)}";
        enemyUnitHpUI.text=$"{ Math.Floor(enemyHp)}";
    }


    //기다리게 하는 함수
    IEnumerator WaitForSecondsExample()
    {
       
        // 0.2초 대기
        yield return new WaitForSeconds(waittingTime);
       
    }

}
