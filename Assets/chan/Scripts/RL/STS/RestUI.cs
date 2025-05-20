using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RestUI : MonoBehaviour
{
    [Header("Rest Panel References")]
    public Button trainingButton;    // 훈련 버튼
    public Button partyButton;       // 연회 버튼
    public Button restButton;      // 휴식 버튼

    private CanvasGroup panelCG;

    [SerializeField] private Image blackFadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        // CanvasGroup 세팅 (없으면 추가)
        panelCG = gameObject.GetComponent<CanvasGroup>();
        if (panelCG == null) panelCG = gameObject.AddComponent<CanvasGroup>();

        // 처음엔 숨기고, 인터랙션 차단

        trainingButton.onClick.RemoveAllListeners();
        partyButton.onClick.RemoveAllListeners();
        restButton.onClick.RemoveAllListeners();
        trainingButton.onClick.AddListener(OnTraining);
        partyButton.onClick.AddListener(OnParty);
        restButton.onClick.AddListener(OnRest);
        
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        UIManager.Instance.UIUpdateAll();
        Debug.Log("[RestUI] Hide() 호출됨");
    }

    private void OnTraining()
    {
        Debug.Log("훈련");
        //다음 전술 개량의 비용을 0으로
        RogueLikeData.Instance.SetIsFreeUpgrade();
        FadeOutAndHide();
      
    }

    private void OnParty()
    {
        Debug.Log("연회");
        //부대 전체의 사기를 30만큼 회복
        RogueLikeData.Instance.ChangeMorale(30);
        FadeOutAndHide();
      
    }
    private void OnRest()
    {

        var myUnits = RogueLikeData.Instance.GetMyUnits();
        Debug.Log("휴식");
        //부대 전체 유닛의 기력을 2만큼 회복
        foreach (var unit in myUnits)
        {
            unit.energy = Mathf.Min(unit.maxEnergy, unit.energy + 2);
        }
        var lineupBar = FindObjectOfType<LineUpBar>();
        if (lineupBar != null)
        {
            // contentParent 아래에 있는 모든 UnitUIPrefab
            var unitUIs = lineupBar.contentParent.GetComponentsInChildren<UnitUIPrefab>();
            foreach (var ui in unitUIs)
            {
                // 각 프리팹이 가지고 있는 unitData 로 다시 SetupEnergy
                ui.SetupEnergy(ui.unitData);
            }
        }
        FadeOutAndHide();
       
    }
    private void FadeOutAndHide()
    {
        blackFadeImage.gameObject.SetActive(true); // 혹시 꺼져있다면

        blackFadeImage.color = new Color(0, 0, 0, 0); // 초기화
        blackFadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            Hide();
            blackFadeImage.color = new Color(0, 0, 0, 0); // 다음을 위해 초기화
        });
    }
}
