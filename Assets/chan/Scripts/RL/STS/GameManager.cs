using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("플레이어 유닛 배치 UI")]
    [SerializeField] private GameObject PlacePanel;     // 에디터에서 PlacePanel 오브젝트
    [SerializeField] private LineUpBar lineUpBar;         // 에디터에서 LineUpBar 컴포넌트

    public PlacePanel PlacePanelComponent => PlacePanel.GetComponent<PlacePanel>();
    public LineUpBar LineUpBarComponent => lineUpBar;

    [SerializeField] private GameObject eventManager;
    [SerializeField] private GameObject storeManager;

    [Header("Map UI & Enemy Info Panel")]
    [SerializeField] private GameObject mapCanvas;            // 기존에 쓰던 map 전체 Canvas
    [SerializeField] private GameObject enemyInfoPanel;       // 새로 추가: 적 정보 패널
   

    public int currentStageX;
    public int currentStageY;
    public StageType currentStageType;
    public List<StageNode> nodes;      // 모델만 저장
    public List<List<int>> paths;
    public Vector3 playerMarkerPosition;

    public static GameManager Instance { get; private set; }

    public bool IsPlaceMode { get; private set; }
    

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

        HideAllPanels();
        SceneManager.sceneLoaded += OnSceneLoaded;

        await GoogleSheetLoader.Instance.LoadUnitSheetData();
        SaveData save = new();
        save.LoadData();
        EventManager.LoadEventData();
        StoreManager.LoadStoreData();
        UnitLoader.Instance.LoadUnitsFromJson();
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


    ///현재 스테이지를 변경(이동)하고, 잠금/해제 로직을 실행합니다.

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

            enemyInfoPanel.SetActive(true);
            var enemies = LoadEnemyUnits(newStage.PresetID);
            var preset = StagePresetLoader.I.GetByID(newStage.PresetID);

            string cmdName = preset.Commander ?? "";
            /*string cmdSkill = !string.IsNullOrEmpty(preset.CommanderID)
                              ? SkillLoader.Instance.GetSkillNameById(preset.CommanderID)
                              : "";*/
            var panel = enemyInfoPanel.GetComponent<EnemyInfoPanel>();
            panel.ShowEnemyInfo(newStage.stageType, enemies, cmdName/*, cmdSkill*/);
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
            restUI.Show();
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


    /// 플레이어 마커를 해당 스테이지 UI 위치로 이동시킵니다.

    private void MovePlayerMarkerTo(StageNodeUI target)
    {
        if (playerMarker == null)
            return;
        RectTransform rt = target.GetComponent<RectTransform>();
        playerMarker.anchoredPosition = rt.anchoredPosition;
    }

    public void InitializeStageLocks()
    {
        mapCanvas.SetActive(true);
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
        enemyInfoPanel.SetActive(false); //250515 적 정보 패널 false

        currentStage.UnlockStage();
        foreach (var nxt in currentStage.connectedStages)
            nxt.UnlockStage();

        
    }
    private List<RogueUnitDataBase> LoadEnemyUnits(int presetID)
    {
        // 1) StagePresetLoader에서 프리셋 가져오기
        var preset = StagePresetLoader.I.GetByID(presetID);
        if (preset == null)
        {
            Debug.LogError($"[GameManager] Preset {presetID} 을(를) 찾을 수 없습니다.");
            return new List<RogueUnitDataBase>();
        }

        // 2) 프리셋의 UnitList(int idx 리스트) → UnitLoader로부터 복제해서 반환
        return preset.UnitList
                     .Select(idx => UnitLoader.Instance.GetCloneUnitById(idx, /*isTeam=*/ false))
                     .Where(u => u != null)
                     .ToList();
    }
    public void TogglePlacePanel(bool open)
    {
        PlacePanel.SetActive(open);
        IsPlaceMode = open;
    }
    
    public void HideAllPanels()
    {
        mapCanvas.SetActive(false);
        enemyInfoPanel.SetActive(false);
        PlacePanel.SetActive(false);
    }
}
