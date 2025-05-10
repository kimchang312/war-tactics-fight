using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Linq;
using static UnityEngine.ParticleSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject eventManager;
    [SerializeField] private GameObject storeManager;
    [SerializeField] private GameObject canvas;

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
     if (scene.name != "RLmap")
        return;

     // 맵 씬에 진입했을 때만
     allStages = FindObjectsOfType<StageNodeUI>().ToList();     InitializeStageLocks();
        InitializeStageLocks();
 }

private void Start()
    {
        // 씬에 있는 모든 StageNodeUI 컴포넌트를 수집
        allStages.AddRange(FindObjectsOfType<StageNodeUI>());

        // (만약 RLmap 이 시작 씬이라면) 맵 진입 시 바로 잠금/언락 초기화
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "RLmap")
            InitializeStageLocks();
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

        // **currentStage를 무조건 여기서 설정**해 줍니다.
        currentStage = newStage;

        // 2) 맵 UI 전체 잠금
        foreach (var s in allStages)
            s.LockStage();

        // --- 전투/엘리트/보스 스테이지 진입 처리 먼저 ---
        if (newStage.stageType == StageType.Combat ||
            newStage.stageType == StageType.Elite ||
            newStage.stageType == StageType.Boss)
        {
            canvas.SetActive(false);
            SceneManager.LoadScene("AutoBattleScene");
            return;  // 여기서 메서드를 끝내고, 맵 UI는 건드리지 않음
        }
        // --- 그 외 맵 내 이벤트(휴식/상점/이벤트) 시에는 기존 UI 잠금/해제 로직 실행 ---
        

        // 3) 플레이어 마커 이동
        MovePlayerMarkerTo(newStage);

        // 4) 진입 스테이지 언락
        newStage.UnlockStage();

        // 5) 연결된 다음 스테이지들 언락
        foreach (var nxt in newStage.connectedStages)
            nxt.UnlockStage();

        // 6) 타입별 처리
        if (newStage.stageType == StageType.Rest)
        {
            restUI?.Show();
        }
        else if (newStage.stageType == StageType.Event)
        {
            eventManager.SetActive(true);
        }
        else if (newStage.stageType == StageType.Shop)
        {
            storeManager.SetActive(true);
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
        canvas.SetActive(true );
        // 1) 씬 안의 모든 StageNodeUI 다시 가져오기
        var all = FindObjectsOfType<StageNodeUI>().ToList();
        // ② 일단 전부 잠급니다
        foreach (var s in all)
            s.LockStage();

        // 첫 맵 진입(아직 어느 스테이지도 찍지 않았다면) → level==0·Combat 만 언락
        if (currentStage == null)
        {
            // 레벨 0 & Combat 타입인 노드를 전부 찾아서 언락
            foreach (var s in all.Where(s => s.level == 0 && s.stageType == StageType.Combat))
            {
                s.UnlockStage();
            }
            return;
        }

        // 그 외(맵 복귀) → 마지막 찍힌 currentStage + 연결된 다음 노드만 언락
        currentStage.UnlockStage();
        foreach (var nxt in currentStage.connectedStages)
            nxt.UnlockStage();
    }
}
