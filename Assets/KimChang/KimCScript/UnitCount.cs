using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // TextMeshProUGUI를 사용하려면 필요


public class UnitCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI myUnitContUI;      //내 유닛수UI에 연결할 변수
    [SerializeField] private TextMeshProUGUI enemyUnitCountUI;  //상대 유닛수UI에 연결할 변수

   
    

    public void CounttingUnits(int myUnitCount,int enemyUnitCount)
    {
        if (myUnitContUI != null && enemyUnitCountUI != null)
        {
            
            myUnitContUI.text = myUnitCount.ToString();
            enemyUnitCountUI.text = enemyUnitCount.ToString();
        }
        else
        {
            
            Debug.LogError("myUnitContUI 또는 enemyUnitCountUI가 연결되지 않았습니다!");
        }

    }
}
