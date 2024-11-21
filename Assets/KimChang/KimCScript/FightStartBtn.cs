using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class FightStartBtn : MonoBehaviour
{
    [SerializeField] private AutoBattleManager battleManager;  // Inspector에서 직접 연결할 AutoBattleManager

    [SerializeField] private Button fightButton;               //ArranageUnitsScene에 있는 Fight 버튼과 연결

    private GoogleSheetLoader sheetLoader = new GoogleSheetLoader();

    // ���� ����, ���� ����
    private int[] _myUnitIds = {19,0,1 };
    private int[] _enemyUnitIds = { 5,3,3 };



    void Start()
    {
        //버튼을 눌렀을때 AutoBattleScene으로 이동후 전투를 실행시키라는 명령을 버튼에 추가해줌
        fightButton.onClick.AddListener(OnFightButtonClick);
    }

    //버튼을 눌렀을때 AutoBattleScene으로 이동후 자동전투 함수를 호출
    void OnFightButtonClick()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("AutoBattleScene");
    }


    //자동 전투 함수를 호출하는 함수
    async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (scene.name == "AutoBattleScene")
        {
            // Inspector에서 연결된 battleManager 사용
            if (battleManager != null)
            {

                
                int result= await battleManager.StartBattle(_myUnitIds,_enemyUnitIds); //자동전투 실행

            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}