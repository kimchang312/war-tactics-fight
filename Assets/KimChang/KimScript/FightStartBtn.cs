using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class FightStartBtn : MonoBehaviour
{
    [SerializeField] private AutoBattleManager battleManager;  // Inspector에서 직접 연결할 AutoBattleManager

    [SerializeField] private Button fightButton;               //ArranageUnitsScene에 있는 Fight 버튼과 연결

    // ���� ����, ���� ����
    private List<int> myUnitIds = new List<int> {0,0,0};
    private List<int> enemyUnitIds = new List<int> {6,6,6};

    void Start()
    {
        // 버튼 클릭 시 OnFightStartBtnClick 실행
        fightButton.onClick.AddListener(OnFightStartBtnClick);
    }

    // 버튼 클릭 시 씬 로드 + 이벤트 연결
    void OnFightStartBtnClick()
    {
        if (myUnitIds == null || enemyUnitIds == null)
        {
            Debug.LogWarning("전투 유닛 ID가 지정되지 않았습니다.");
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("AutoBattleScene");
    }

    // 씬 로드 완료 후 자동 전투 시작
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        AutoBattleManager manager = GameObject.FindObjectOfType<AutoBattleManager>();
        if (manager != null)
        {
            manager.StartBattle(myUnitIds, enemyUnitIds);
        }
        else
        {
            Debug.LogError("AutoBattleManager를 찾을 수 없습니다.");
        }
    }


    //유닛 정보 수정
    public void SetMyFightUnits(List<int> unitIds)
    {
        myUnitIds = unitIds;
    }
    public void SetEnemyFightUnits(List<int> unitIds)
    {
        enemyUnitIds = unitIds;
    }
    //유닛 정보 호출
    public List<int> GetMyFightUnits()
    {
        return myUnitIds;
    }
    public List<int> GetEnemyFightUnits()
    {
        return enemyUnitIds; 
    }

}