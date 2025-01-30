using UnityEngine;
using UnityEngine.UI;

public class StageUIComponent : MonoBehaviour
{
    public Button button;
    public Image stageImage; // ✅ 스테이지의 투명도를 직접 조절하기 위한 이미지 컴포넌트

    public bool isCleared = false; // ✅ 클리어 여부
    public bool isLocked = true; // ✅ 잠금 여부
    public bool isClickable = false; // ✅ 클릭 가능 여부

    void Awake()
    {
        // ✅ 프리팹 내 Button과 Image 자동 연결
        button = GetComponentInChildren<Button>();
        stageImage = GetComponentInChildren<Image>();

        if (button == null)
        {
            Debug.LogError($"❌ {gameObject.name}에 Button이 없습니다! 프리팹 설정을 확인하세요.");
        }

        if (stageImage == null)
        {
            Debug.LogError($"❌ {gameObject.name}에 Image가 없습니다! 투명도 조절을 위해 필요합니다.");
        }
    }

    // ✅ 버튼과 투명도를 직접 조절
    public void SetInteractable(bool isInteractable)
    {
        if (button != null)
        {
            button.interactable = isInteractable;
        }

        if (stageImage != null)
        {
            stageImage.color = isInteractable ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.5f); // ✅ 직접 투명도 조절
        }
    }
}