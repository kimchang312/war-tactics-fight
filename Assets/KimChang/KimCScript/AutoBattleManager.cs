using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using System.Threading.Tasks;


public class AutoBattleManager : MonoBehaviour
{

    [SerializeField] private AutoBattleUI autoBattleUI;       //UI 관리 스크립트

    private float waittingTime=0.2f; //    

    private GoogleSheetLoader sheetLoader = new GoogleSheetLoader();

    


    private async Task<(UnitDataBase[], UnitDataBase[])> GetUnits(int[] myUnitIds, int[] enemyUnitIds)
    {
        List<UnitDataBase> myUnits = new List<UnitDataBase>();
        List<UnitDataBase> enemyUnits = new List<UnitDataBase>();

        // 구글 시트 데이터를 로드
        await sheetLoader.LoadGoogleSheetData();

        // 내 유닛 ID들을 기반으로 유닛을 가져와서 MyUnits에 저장
        foreach (int unitId in myUnitIds)
        {
            List<string> rowData = sheetLoader.GetRowData(unitId); // rowData는 List<string> 형식
           
            if (rowData != null)
            {
                // rowData를 UnitDataBase로 변환
                UnitDataBase unit = UnitDataBase.ConvertToUnitDataBase(rowData);
            
                if (unit != null)
                {
                    myUnits.Add(unit);  // List에 유닛 추가
                }
            }
        }

        // 적의 유닛 ID들을 기반으로 유닛을 가져와서 enemyUnits에 저장
        foreach (int unitId in enemyUnitIds)
        {
            List<string> rowData = sheetLoader.GetRowData(unitId); // rowData는 List<string> 형식
            if (rowData != null)
            {
                // rowData를 UnitDataBase로 변환
                UnitDataBase unit = UnitDataBase.ConvertToUnitDataBase(rowData);
                if (unit != null)
                {
                    enemyUnits.Add(unit);  // List에 유닛 추가
                }
            }
        }

        // List를 배열로 변환한 후 튜플로 반환
        return (myUnits.ToArray(), enemyUnits.ToArray());
    }


    //자동전투
    private async Task<int> AutoBattle(int[] _myUnitIds, int[] _enemyUnitIds)
    {

        // 나의 피해량
        float myDamage;
        // 적의 피해량
        float enemyDamage;

        // 나와 적의 유닛을 호출
        (UnitDataBase[] myUnits, UnitDataBase[] enemyUnits) = await GetUnits(_myUnitIds, _enemyUnitIds);


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

        UpdateUnitName(myUnits[0].unitBranch, enemyUnits[0].unitBranch);

        // 전투 반복
        while (myUnitIndex < myUnits.Length && enemyUnitIndex < enemyUnits.Length)
        {
            // 1차적인 데미지 계산: 데미지 = 공격력 * (1 - 적의 장갑 / (10 + 적의 장갑))
            myDamage = myUnits[myUnitIndex].attackDamage * (1 - (enemyUnits[enemyUnitIndex].armor / (10 + enemyUnits[enemyUnitIndex].armor)));
            enemyDamage = enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor / (10 + myUnits[myUnitIndex].armor)));

            // 체력 감소
            myUnits[myUnitIndex].health -= enemyDamage;
            enemyUnits[enemyUnitIndex].health -= myDamage;

            UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);
            UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);
            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);

            // 유닛의 체력이 0 이하일 경우, 다음 유닛으로 넘어감
            if (myUnits[myUnitIndex].health <= 0)
            {
                Debug.Log("내 유닛 " + myUnits[myUnitIndex].unitName + "사망");
                myUnitIndex++;  // 다음 내 유닛
                UpdateUnitCount(myUnitMax - myUnitIndex, enemyUnitMax - enemyUnitIndex);
                //UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);
            }
            if (enemyUnits[enemyUnitIndex].health <= 0)
            {
                Debug.Log("적 유닛 " + enemyUnits[enemyUnitIndex].unitName + "사망");
                enemyUnitIndex++;  // 다음 적 유닛
                UpdateUnitCount(myUnitMax - myUnitIndex, enemyUnitMax - enemyUnitIndex);
                //UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);
            }


            //유닛 체력 UI관련
            if(myUnitIndex == myUnitMax && enemyUnitIndex == enemyUnitMax)
            {
                UpdateUnitName(myUnits[myUnitMax - 1].unitBranch, enemyUnits[enemyUnitIndex-1].unitBranch);
                UpdateUnitHp(myUnits[myUnitMax - 1].health, enemyUnits[enemyUnitIndex-1].health);
            }
            else if (myUnitIndex== myUnitMax)
            {
                UpdateUnitName(myUnits[myUnitMax - 1].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);
                UpdateUnitHp(myUnits[myUnitMax - 1].health, enemyUnits[enemyUnitIndex].health);
            }
            else if (enemyUnitIndex==enemyUnitMax)
            {
                UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitMax - 1].unitBranch);
                UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitMax-1].health);
            }
           
            

            await WaitForSecondsAsync(0.5f);

        }

        // 전투 종료 후 승리 여부 판단
        if (myUnitIndex < myUnits.Length && enemyUnitIndex >= enemyUnits.Length)
        {
            Debug.Log($"나의 승리 {myUnits[myUnitIndex].unitName + myUnits[myUnitIndex].health}");
            return 0;  // 내가 승리
        }
        else if (enemyUnitIndex < enemyUnits.Length && myUnitIndex >= myUnits.Length)
        {
            Debug.Log($"나의 승리 {enemyUnits[enemyUnitIndex].unitName + enemyUnits[enemyUnitIndex].health}");
            return 1;  // 적이 승리
        }
        else
        {
            Debug.Log("무승부");
            return 2;  // 양쪽 모두 사망
        }
    }


    //자동전투를 다른코드에서 호출할수 있게끔 대신 호출해주는 함수
    public async Task<int> StartBattle(int[] _myUnitIds, int[] _enemyUnitIds)
    {
        if (autoBattleUI == null)
        {
            autoBattleUI = FindObjectOfType<AutoBattleUI>();
        }
        int result = await AutoBattle(_myUnitIds,_enemyUnitIds);
        return result;
    }

    //유닛 수 UI 초기화 함수
    private void UpdateUnitCount(int myUnitLength,int enemyUnitLength)
    {
          autoBattleUI.UpdateUnitCountUI(myUnitLength, enemyUnitLength);
       

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

        autoBattleUI.UpateUnitHPUI(MathF.Floor( myUnitHp),MathF.Floor( enemyHp));
        
 
    }

    //기다리는 함수
    private async Task WaitForSecondsAsync(float seconds)
    {
        await Task.Delay((int)(seconds * 1000)); // 밀리초 단위
    }


    private void UpdateUnitName(string myUnitName, string enemyUnitName)
    {
        autoBattleUI.UpdateName(myUnitName, enemyUnitName);
    }

}
