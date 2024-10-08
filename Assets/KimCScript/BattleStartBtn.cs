using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // ��ư ����� ���� �߰�

public class BattleStartBtn : MonoBehaviour
{

    public Button battleStartButton;  // UI Button�� ������ ����
    public AutoBattleManager battleManager; //�ڵ����� �ڵ� ����

    // Start is called before the first frame update
    void Start()
    {
        // ��ư�� ������ �� AutoBattle �Լ� ����
        battleStartButton.onClick.AddListener(OnBattleStart);
        
    }

    // ��ư�� ������ �� ȣ��Ǵ� �Լ�
    void OnBattleStart()
    {
        if (battleManager != null)
        {
            int result=battleManager.StartBattle();
            
            Debug.Log(result);
        }
    }

}
