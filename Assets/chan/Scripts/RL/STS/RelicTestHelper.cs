using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 47, 48번 전쟁유산 테스트를 위한 헬퍼 스크립트
/// </summary>
public class RelicTestHelper : MonoBehaviour
{
    [Header("테스트 UI")]
    [SerializeField] private Button addTreasureMapBtn;
    [SerializeField] private Button addRainbowKeyBtn;
    [SerializeField] private Button resetRainbowKeyBtn;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        // 버튼이 할당되어 있으면 리스너 추가
        if (addTreasureMapBtn != null)
        {
            addTreasureMapBtn.onClick.AddListener(AddTreasureMap);
        }
        if (addRainbowKeyBtn != null)
        {
            addRainbowKeyBtn.onClick.AddListener(AddRainbowKey);
        }
        if (resetRainbowKeyBtn != null)
        {
            resetRainbowKeyBtn.onClick.AddListener(ResetRainbowKeyUses);
        }
        
        UpdateStatusText();
    }

    /// <summary>
    /// 47번 보물지도 추가 (테스트용)
    /// </summary>
    public void AddTreasureMap()
    {
        RogueLikeData.Instance.AcquireRelic(47);
        Debug.Log("[테스트] 보물지도(47번) 추가 완료. 다음 이벤트 지역이 보물로 변경됩니다.");
        UpdateStatusText();
    }

    /// <summary>
    /// 48번 무지개 열쇠 추가 (테스트용)
    /// </summary>
    public void AddRainbowKey()
    {
        RogueLikeData.Instance.AcquireRelic(48);
        Debug.Log("[테스트] 무지개 열쇠(48번) 추가 완료. 챕터당 2회 연결되지 않은 지역으로 이동 가능합니다.");
        UpdateStatusText();
    }

    /// <summary>
    /// 무지개 열쇠 사용 횟수 리셋 (테스트용)
    /// </summary>
    public void ResetRainbowKeyUses()
    {
        int chapter = RogueLikeData.Instance.GetChapter();
        // 딕셔너리에서 해당 챕터 제거 (다음 사용 시 0으로 초기화됨)
        // 직접 접근이 불가능하므로 강제로 사용 횟수를 0으로 만들기 위해
        // 챕터를 변경했다가 다시 돌아오는 방식 사용
        int tempChapter = chapter == 1 ? 2 : 1;
        RogueLikeData.Instance.SetChapter(tempChapter);
        RogueLikeData.Instance.SetChapter(chapter);
        Debug.Log($"[테스트] 챕터 {chapter}의 무지개 열쇠 사용 횟수 리셋 완료.");
        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        if (statusText == null) return;

        bool hasTreasureMap = RelicManager.CheckRelicById(47);
        bool hasRainbowKey = RelicManager.CheckRelicById(48);
        int chapter = RogueLikeData.Instance.GetChapter();
        int rainbowKeyUses = RogueLikeData.Instance.GetRainbowKeyUses(chapter);
        bool nextEventToTreasure = RogueLikeData.Instance.GetNextEventToTreasure();

        string status = $"=== 전쟁유산 테스트 상태 ===\n";
        status += $"보물지도(47): {(hasTreasureMap ? "보유" : "미보유")}\n";
        status += $"다음 이벤트→보물: {(nextEventToTreasure ? "예정" : "없음")}\n";
        status += $"무지개 열쇠(48): {(hasRainbowKey ? "보유" : "미보유")}\n";
        status += $"챕터 {chapter} 사용 횟수: {rainbowKeyUses}/2\n";

        statusText.text = status;
    }

    private void Update()
    {
        // F5 키로 수동 업데이트
        if (Input.GetKeyDown(KeyCode.F5))
        {
            UpdateStatusText();
        }
    }
    
    // 외부에서 호출 가능한 업데이트 메서드 (GameManager에서 호출)
    public void RefreshStatus()
    {
        UpdateStatusText();
    }
}

