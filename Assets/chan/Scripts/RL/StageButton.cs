using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class StageButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action<StageNode> OnStageButtonClicked; // ✅ Action 이벤트 추가

    public Button button;
    public TextMeshProUGUI stageText;
    private StageNode stageData;
    private StageTooltip stageTooltip;
    private StageUIManager stageUIManager; // ✅ StageUIManager 참조 추가

    private void Start()
    {
        button.onClick.AddListener(OnStageSelected); // ✅ 버튼 클릭 시 이동 요청
    }

    public void SetStage(StageNode stage, StageTooltip tooltip, StageUIManager uiManager)
    {
        stageData = stage;
        stageTooltip = tooltip;
        stageUIManager = uiManager; // ✅ StageUIManager 인스턴스 저장
        stageText.text = "레벨 " + stage.level;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (stageData.level == 1) return; // ✅ 시작 노드는 툴팁 표시 안함

        if (stageTooltip != null)
        {
            string stageName = stageData.name;
            string tooltipMessage = $"{stageName}\n지휘관 : ???\n적 유닛 : ???"; // ✅ 툴팁 메시지 업데이트
            RectTransform buttonRect = GetComponent<RectTransform>();

            if (stageTooltip.tooltipPanel != null)
            {
                stageTooltip.ShowTooltip(tooltipMessage, buttonRect);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (stageTooltip != null)
        {
            stageTooltip.HideTooltip();
        }
    }

    public void OnStageSelected()
    {
        Debug.Log($"🟢 스테이지 버튼 클릭됨: {stageData.level}");

        if (stageUIManager != null && stageUIManager.GetCurrentStage() != null)
        {
            // ✅ 같은 레벨 또는 이전 레벨 이동 차단
            if (stageData.level <= stageUIManager.GetCurrentStage().level)
            {
                Debug.Log("🛑 이동 불가: 같은 레벨 또는 이전 레벨로 이동할 수 없습니다!");
                return;
            }
        }

        Debug.Log($"✅ 이동 요청: {stageData.level}");
        OnStageButtonClicked?.Invoke(stageData);
    }

    public StageNode GetStageData()
    {
        return stageData;
    }
}
