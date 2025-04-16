using UnityEngine;
using UnityEngine.EventSystems;

public class StageClickHandler : MonoBehaviour, IPointerClickHandler
{
    // 이 StageClickHandler가 붙은 UI의 정보(예: StageNodeUI)를 참조
    public StageNodeUI stageNodeUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        // GameManager 싱글톤으로 만들어서 스테이지 이동 등을 관리할지 정보를 저장, 전달 방식으로 할지.
        // GameManager(또는 StageManager)에 해당 스테이지 클릭을 알림
        //GameManager.Instance.OnStageClicked(stageNodeUI);
    }
}
