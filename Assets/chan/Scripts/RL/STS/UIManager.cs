using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 텍스트 레퍼런스")]

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI moraleText;
    public TextMeshProUGUI rerollText;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);   // 씬 전환 시에도 파괴되지 않도록
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UIUpdateAll();
    }

    /// <summary>골드/사기/리롤 전부 갱신</summary>
    public void UIUpdateAll()
    {
        UpdateGold();
        UpdateMorale();
        UpdateReroll();
    }

    public void UpdateGold()
    {
        int g = RogueLikeData.Instance.GetCurrentGold();
        goldText.text = g.ToString(); ;
    }

    public void UpdateMorale()
    {
        int m = RogueLikeData.Instance.GetMorale();
        moraleText.text = m.ToString(); ;
    }

    public void UpdateReroll()
    {
        int r = RogueLikeData.Instance.GetRerollChance();
        rerollText.text = r.ToString(); ;
    }
}
