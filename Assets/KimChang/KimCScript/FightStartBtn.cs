using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FightStartBtn : MonoBehaviour
{
    [SerializeField] private AutoBattleManager battleManager;  // Inspector���� ���� ������ AutoBattleManager
    [SerializeField] private UnitCount unitCount;              // Inspector���� ���� ������ UnitCount


    [SerializeField] private Button fightButton;               //ArranageUnitsScene�� �ִ� Fight ��ư�� ����

    // ���� ����, ���� ����
    private int[] _myUnitIds = { 3, 4, 5, 6 };
    private int[] _enemyUnitIds = { 1, 2, 8, 7, 6 };


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
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "AutoBattleScene")
        {
            // Inspector���� ����� battleManager ���
            if (battleManager != null)
            {
                unitCount.CounttingUnits(_myUnitIds.Length,_enemyUnitIds.Length);//���� �� UI������Ʈ
                int result= battleManager.StartBattle(_myUnitIds,_enemyUnitIds); //�ڵ����� ����
                Debug.Log(result);
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
