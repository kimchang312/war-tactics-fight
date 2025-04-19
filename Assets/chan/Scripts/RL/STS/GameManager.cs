using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Marker")]
    // Canvas 내에서 움직일 마커(Root Canvas의 자식인 RectTransform)
    public RectTransform playerMarker;

    [Header("Lock Settings")]
    // 잠긴 스테이지에 적용할 색상
    public Color lockedColor = Color.white;

    // 모든 스테이지 UI를 모아두는 리스트
    private List<StageNodeUI> allStages = new List<StageNodeUI>();
    // 현재 플레이어가 위치한 스테이지
    private StageNodeUI currentStage;

    private void Awake()
    {
        // 싱글턴 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 씬에 있는 모든 StageNodeUI 컴포넌트를 수집
        allStages.AddRange(FindObjectsOfType<StageNodeUI>());

        // 1) 일단 전부 잠그고
        foreach (var s in allStages)
            s.LockStage();

        // 2) 레벨 1(0 인덱스) 스테이지만 열어준다
        foreach (var s in allStages)
        {
            if (s.level == 0)
                s.UnlockStage();
        }
    }

    /// <summary>
    /// StageNodeUI가 클릭되었을 때 호출됩니다.
    /// </summary>
    public void OnStageClicked(StageNodeUI clickedStage)
    {
        // 디버그: 클릭된 정보 찍기
        Debug.Log($"OnStageClicked → level:{clickedStage.level}, row:{clickedStage.row}, locked:{clickedStage.IsLocked}, currentStage:{(currentStage == null ? "null" : currentStage.level.ToString())}");
        // 잠겨 있으면 아무 동작 안 함
        if (clickedStage.IsLocked)
            return;

        // 첫 이동이거나, 현재 스테이지와 연결된 경우에만 이동
        if (currentStage == null )
            // 첫 이동엔 레벨 1만 허용
            if (clickedStage.level == 0)
            {
                Debug.Log(" → 첫 이동 허용 (레벨 1)");
                SetCurrentStage(clickedStage);
                return;
            }
            else
            {
                Debug.Log(" → 첫 이동이지만 레벨1이 아닙니다.");
                return;
            }

        // 3) 이후 이동은 연결된 스테이지만
        bool isConnected = currentStage.connectedStages.Contains(clickedStage);
        Debug.Log($" → 현재 스테이지({currentStage.level}) 와 clicked({clickedStage.level}) 연결 여부: {isConnected}");
        if (isConnected)
        {
            SetCurrentStage(clickedStage);
        }
        else
        {
            Debug.Log(" → 해당 스테이지로 이동할 수 없습니다.");
        }
    }

    /// <summary>
    /// 내부: 현재 스테이지를 변경(이동)하고, 잠금/해제 로직을 실행합니다.
    /// </summary>
    private void SetCurrentStage(StageNodeUI newStage)
    {
        // 1) 일단 모든 스테이지 잠금
        foreach (var s in allStages)
            s.LockStage();

        // 2) 플레이어 마커 이동
        MovePlayerMarkerTo(newStage);

        // 3) 새 스테이지 잠금 해제
        newStage.UnlockStage();
        currentStage = newStage;

        // 4) 새 스테이지와 연결된 스테이지들만 잠금 해제
        foreach (var connectstage in newStage.connectedStages)
            connectstage.UnlockStage();
    }

    /// <summary>
    /// 플레이어 마커를 해당 스테이지 UI 위치로 이동시킵니다.
    /// </summary>
    private void MovePlayerMarkerTo(StageNodeUI target)
    {
        if (playerMarker == null)
            return;
        RectTransform rt = target.GetComponent<RectTransform>();
        playerMarker.anchoredPosition = rt.anchoredPosition;
    }
}
