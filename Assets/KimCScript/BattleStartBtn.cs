using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 버튼 사용을 위해 추가

public class BattleStartBtn : MonoBehaviour
{

    public Button battleStartButton;  // UI Button을 연결할 변수
    public AutoBattleManager battleManager; //자동전투 코드 연결

    // Start is called before the first frame update
    void Start()
    {
        // 버튼이 눌렸을 때 AutoBattle 함수 실행
        battleStartButton.onClick.AddListener(OnBattleStart);
        
    }

    // 버튼이 눌렸을 때 호출되는 함수
    void OnBattleStart()
    {
        if (battleManager != null)
        {
            int result=battleManager.StartBattle();
            
            Debug.Log(result);
        }
    }

}
