using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.EventSystems;

public class StageClickHandler : MonoBehaviour, IPointerClickHandler
{
    // 이 StageClickHandler가 붙은 UI의 정보(예: StageNodeUI)를 참조
    public StageNodeUI stageNodeUI;

    private void Awake()
    {
        // 같은 GameObject 에 붙어있는 StageNodeUI 컴포넌트를 가져옵니다.
        stageNodeUI = GetComponent<StageNodeUI>();
        if (stageNodeUI == null)
            Debug.LogError("StageNodeUI 컴포넌트가 없습니다");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭된 스테이지가 잠겨 있으면 아무 동작하지 않습니다.
        if (stageNodeUI.IsLocked)
        {
            Debug.Log("이 스테이지는 잠겨 있습니다.");
            return;
        }

        // GameManager 에 클릭된 스테이지를 전달하여
        // 이동, 이전 스테이지 잠금, 다음 경로 잠금 해제 등을 처리하게 합니다.
        GameManager.Instance.OnStageClicked(stageNodeUI);
    }
}

