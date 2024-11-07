using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class FightStartBtn : MonoBehaviour
{
    [SerializeField] private AutoBattleManager battleManager;  // Inspector���� ���� ������ AutoBattleManager

    [SerializeField] private Button fightButton;               //ArranageUnitsScene�� �ִ� Fight ��ư�� ����

    private GoogleSheetLoader sheetLoader = new GoogleSheetLoader();

    // ���� ����, ���� ����
    private int[] _myUnitIds = { 2, 2, 2, 2 };
    private int[] _enemyUnitIds = { 15 };


    void Start()
    {

        //��ư�� �������� AutoBattleScene���� �̵��� ������ �����Ű��� ����� ��ư�� �߰�����
        fightButton.onClick.AddListener(OnFightButtonClick);
    }

    //��ư�� �������� AutoBattleScene���� �̵��� �ڵ����� �Լ��� ȣ��
    void OnFightButtonClick()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("AutoBattleScene");
    }


    //�ڵ� ���� �Լ��� ȣ���ϴ� �Լ�
    async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (scene.name == "AutoBattleScene")
        {
            // Inspector���� ����� battleManager ���
            if (battleManager != null)
            {

                int result = await battleManager.StartBattle(_myUnitIds, _enemyUnitIds); //�ڵ����� ����
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}