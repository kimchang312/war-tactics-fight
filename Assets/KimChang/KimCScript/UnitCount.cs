using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // TextMeshProUGUI�� ����Ϸ��� �ʿ�


public class UnitCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI myUnitContUI;      //�� ���ּ�UI�� ������ ����
    [SerializeField] private TextMeshProUGUI enemyUnitCountUI;  //��� ���ּ�UI�� ������ ����

   
    

    public void CounttingUnits(int myUnitCount,int enemyUnitCount)
    {
        if (myUnitContUI != null && enemyUnitCountUI != null)
        {
            
            myUnitContUI.text = myUnitCount.ToString();
            enemyUnitCountUI.text = enemyUnitCount.ToString();
        }
        else
        {
            
            Debug.LogError("myUnitContUI �Ǵ� enemyUnitCountUI�� ������� �ʾҽ��ϴ�!");
        }

    }
}
