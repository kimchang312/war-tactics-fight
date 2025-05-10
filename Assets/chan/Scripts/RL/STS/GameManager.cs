using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject eventManager;
    [SerializeField] private GameObject storeManager;

    public int currentStageX;
    public int currentStageY;
    public StageType currentStageType;
    public List<StageNode> nodes;      // 모델만 저장
    public List<List<int>> paths;
    public Vector3 playerMarkerPosition;

    public static GameManager Instance { get; private set; }

    // 스테이지 프리셋 변경 이벤트
    public event Action<int> OnPresetChanged;

    [Header("Player Marker")]
    // Canvas 내에서 움직일 마커(Root Canvas의 자식인 RectTransform)
    public RectTransform playerMarker;

    [Header("Rest Event")]
    public RestUI restUI;   // 에디터에서 할당

    [Header("Lock Settings")]
    // 잠긴 스테이지에 적용할 색상
    public Color lockedColor = Color.white;

    // 모든 스테이지 UI를 모아두는 리스트
    private List<StageNodeUI> allStages = new List<StageNodeUI>();
    // 현재 플레이어가 위치한 스테이지
    private StageNodeUI currentStage;

    private async void Awake()
    {
        if (Instance == null)
        {
            // 최초 인스턴스라면 여기서 고정
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            // 이미 다른 인스턴스가 살아있다면, 이 오브젝트는 파괴
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        await GoogleSheetLoader.Instance.LoadUnitSheetData();
        SaveData save = new();
        save.LoadData();
        EventManager.LoadEventData();
        StoreManager.LoadStoreData();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1) 씬에 있는 모든 StageNodeUI 를 새로 수집
        allStages = FindObjectsOfType<StageNodeUI>().ToList();

        InitializeStageLocks();
    }

    private void Start()
    {
        // 씬에 있는 모든 StageNodeUI 컴포넌트를 수집
        allStages.AddRange(FindObjectsOfType<StageNodeUI>());

    }

    public int CurrentGold
    {
        get => RogueLikeData.Instance.GetCurrentGold();
        set => RogueLikeData.Instance.SetCurrentGold(value);
    }

    public int SpentGold
    {
        get => RogueLikeData.Instance.GetSpentGold();
        set => RogueLikeData.Instance.SetSpentGold(value);
    }
    public int PlayerMorale
    {
        get => RogueLikeData.Instance.GetMorale();
        set => RogueLikeData.Instance.SetMorale(value);
    }

    /*public int RerollCount
    {
    물어보고 추가 - 내용은 보유 리롤 횟수 접근
    }*/
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
        if (currentStage == null)
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
        allStages = FindObjectsOfType<StageNodeUI>().ToList();

        RogueLikeData.Instance.SetCurrentStage(newStage.level, newStage.row, newStage.stageType);
        RogueLikeData.Instance.SetPresetID(newStage.PresetID);

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

        // 5) 스테이지 입장 후, 타입별 이벤트 처리
        // Rest 이벤트 처리
        if (newStage.stageType == StageType.Rest)
        {
            Debug.Log($"[GameManager] RestStage 진입: level={newStage.level}, row={newStage.row}");
            if (restUI != null)
            {
                restUI.Show();
            }
            else
            {
                Debug.LogError("[GameManager] restUI 레퍼런스가 없습니다!");
            }
        }
        else if (newStage.stageType == StageType.Event)
        {
            eventManager.SetActive(true);
        }
        else if (newStage.stageType == StageType.Shop)
        {
            storeManager.SetActive(true);
        }
        else if (newStage.stageType == StageType.Combat || newStage.stageType == StageType.Elite || newStage.stageType == StageType.Boss)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("AutoBattleScene");
        }
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

    public void InitializeStageLocks()
    {
        // 아직 한 번도 이동 안 한 상태 → 레벨0만 언락
        if (currentStage == null)
        {
            foreach (var ui in allStages)
                if (ui.level == 0 && ui.stageType == StageType.Combat)
                    ui.UnlockStage();
            return;
        }

        // 돌아온 상황 → 저장된 currentStage 위치를 찾아 언락
        var pos = RogueLikeData.Instance.GetCurrentStage();
        var savedUI = allStages.FirstOrDefault(ui =>
            ui.level == pos.x &&
            ui.row == pos.y &&
            ui.stageType == pos.type);

        if (savedUI != null)
        {
            savedUI.UnlockStage();
            foreach (var nxt in savedUI.connectedStages)
                nxt.UnlockStage();
            currentStage = savedUI;
        }
    }
}
