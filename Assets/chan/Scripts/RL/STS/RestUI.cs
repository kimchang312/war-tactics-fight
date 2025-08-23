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

    [Header("페이드 설정")]
    [SerializeField] private Image fadeImage;
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
        GameManager.Instance.itemToolTip.SetActive(false);
        Debug.Log("[RestUI] Hide() 호출됨");
    }

    private void OnTraining()
    {
        Debug.Log("훈련");
        //다음 전술 개량의 비용을 0으로
        PlayFadeEffect(() =>
        {
            RogueLikeData.Instance.SetIsFreeUpgrade();
            Hide();
        });
    }

    private void OnParty()
    {
        Debug.Log("연회");
        //부대 전체의 사기를 30만큼 회복
        PlayFadeEffect(() =>
        {
            RogueLikeData.Instance.ChangeMorale(30);
            UIManager.Instance.UpdateMorale();
            Hide();
        });
    }
    private void OnRest()
    {
        Debug.Log("휴식");
        PlayFadeEffect(() =>
        {
            var myUnits = RogueLikeData.Instance.GetMyTeam();
            foreach (var unit in myUnits)
            {
                unit.energy = Mathf.Min(unit.maxEnergy, unit.energy + 2);
            }

            var lineupBar = FindObjectOfType<LineUpBar>();
            if (lineupBar != null)
            {
                var unitUIs = lineupBar.contentParent.GetComponentsInChildren<UnitUIPrefab>();
                foreach (var ui in unitUIs)
                    ui.SetupEnergy(ui.unitData);
            }

            Hide();
        });
    }
    private void PlayFadeEffect(System.Action onMidFade)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // 완전 투명

        // 어두워짐 → 중간처리 → 밝아짐
        fadeImage.DOFade(1f, fadeDuration)
            .OnComplete(() =>
            {
                onMidFade?.Invoke();
                fadeImage.DOFade(0f, fadeDuration)
                    .OnComplete(() =>
                    {
                        fadeImage.gameObject.SetActive(false);
                    });
            });
    }
}
