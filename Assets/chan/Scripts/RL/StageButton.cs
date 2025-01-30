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
        UpdateButtonState(); // ✅ 초기 버튼 상태 업데이트
    }

    public void SetStage(StageNode stage, StageTooltip tooltip, StageUIManager uiManager)
    {
        stageData = stage;
        stageTooltip = tooltip;
        stageUIManager = uiManager; // ✅ StageUIManager 인스턴스 저장
        stageText.text = "레벨 " + stage.level;

        UpdateButtonState(); // ✅ 상태 업데이트
    }
    // ✅ 버튼의 클릭 가능 여부를 직접 조절하는 메서드 추가
    public void SetInteractable(bool isInteractable)
    {
        if (button != null)
        {
            button.interactable = isInteractable;
        }
    }

    // ✅ 스테이지 상태를 UI에 반영하는 메서드 추가
    public void UpdateButtonState()
    {
        if (stageData == null) return;

        button.interactable = !stageData.isLocked; // 🔹 잠긴 상태면 클릭 불가능
        stageText.alpha = stageData.isClickable ? 1.0f : 0.5f; // 🔹 클릭 가능 여부에 따라 투명도 조절

        if (stageData.isCleared)
        {
            stageText.color = Color.green; // 🔹 클리어된 스테이지는 녹색으로 표시
        }
        else if (stageData.isLocked)
        {
            stageText.color = Color.gray; // 🔹 잠긴 스테이지는 회색으로 표시
        }
        else
        {
            stageText.color = Color.white; // 🔹 기본 상태
        }
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
