using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    [Header("플레이어 유닛 배치 UI")]
    [SerializeField] private GameObject PlacePanel;     // 에디터에서 PlacePanel 오브젝트
    [SerializeField] private LineUpBar lineUpBar;         // 에디터에서 LineUpBar 컴포넌트

    public PlacePanel PlacePanelComponent => PlacePanel.GetComponent<PlacePanel>();
    public LineUpBar LineUpBarComponent => lineUpBar;

    [SerializeField] public GameObject eventManager;
    [SerializeField] public GameObject storeManager;

    [Header("Map UI & Enemy Info Panel")]
    [SerializeField] private GameObject mapCanvas;            // 기존에 쓰던 map 전체 Canvas
    [SerializeField] public GameObject enemyInfoPanel;       // 새로 추가: 적 정보 패널
    [SerializeField] public GameObject restPanel;
    [SerializeField] public RewardUI rewardUI;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] public ObjectPool objectPool;
    public GameObject itemToolTip;
    

    [SerializeField] private RectTransform mapPanel;

    public UIGenerator uIGenerator;
    public UnitDetailExplain unitDetail; 
    public int currentStageX;
    public int currentStageY;
    public StageType currentStageType;
    public List<StageNode> nodes;      // 모델만 저장
    public List<List<int>> paths;
    public Vector3 playerMarkerPosition;

    public static GameManager Instance { get; private set; }

    public bool IsPlaceMode { get; private set; }
    public bool _hasInitialized = false;
    public bool shouldRefreshUpgradeUI = false;

    [Header("Player Marker")]
    // Canvas 내에서 움직일 마커(Root Canvas의 자식인 RectTransform)
    [SerializeField] public GameObject playerMarkerPrefab; // ✅ 에디터에서 연결
    public RectTransform playerMarker { get; set; }  // 생성 후 보관

    [Header("Rest Event")]
    public RestUI restUI;   // 에디터에서 할당

    [Header("Lock Settings")]
    // 잠긴 스테이지에 적용할 색상
    public Color lockedColor = Color.white;

    // 모든 스테이지 UI를 모아두는 리스트
    private List<StageNodeUI> allStages = new List<StageNodeUI>();
    // 현재 플레이어가 위치한 스테이지
    private StageNodeUI currentStage;

    //[SerializeField] private UnitDetailExplain unitDetailExplain;
    public UnitListUI unitListUI;
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
        if(objectPool == null)
        {
            objectPool = GetComponentInChildren<ObjectPool>();
        }
        if(unitDetail == null)
        {
            unitDetail = GetComponentInChildren<UnitDetailExplain>(true);
        }
        if(unitListUI == null)
        {
            unitListUI = GetComponentInChildren<UnitListUI>(true);
        }
        HideAllPanels();
        SceneManager.sceneLoaded += OnSceneLoaded;

        SaveData save = new();
        save.LoadData();
        EventManager.LoadEventData();
        StoreManager.LoadStoreData();
        UnitLoader.Instance.LoadUnitsFromJson();
        GameTextDB.Boot();
    }

private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
 {
     if (scene.name != "RLmap")
        return;

        CloseAllUI();

        // 맵 씬에 진입했을 때만
        allStages = FindObjectsOfType<StageNodeUI>().ToList();
        HideAllPanels();
        UIManager.Instance.UIUpdateAll();
        InitializeStageLocks();

        if (playerMarker == null)
        {
            Debug.Log("🔄 playerMarker null → 새로 생성");
            uIGenerator.EnsurePlayerMarker();  // ← 프리팹에서 다시 생성
        }
        // 마커 위치도 복원
        if (playerMarker != null && currentStage != null)
        {
            MovePlayerMarkerTo(currentStage);
        }


        if (RogueLikeData.Instance.GetClearChpater())
        {
            SetCurrentStageNull();
            RogueLikeData.Instance.SetClearChapter(false);
            Vector2 pos = mapPanel.anchoredPosition;
            pos.x = 0f;
            mapPanel.anchoredPosition = pos;
            Debug.Log("✅ mapPanel의 PosX를 0으로 초기화");
            // 🔽 챕터 텍스트 업데이트
            UIManager.Instance.UpdateChapter(RogueLikeData.Instance.GetChapter());
            if (uIGenerator == null) uIGenerator = transform.GetChild(0).GetChild(0).GetComponent<UIGenerator>();
            uIGenerator.RegenerateMap();
        }
        else if (RogueLikeData.Instance.GetResetMap())
        {
            SetCurrentStageNull();
            if (uIGenerator == null) uIGenerator = transform.GetChild(0).GetChild(0).GetComponent<UIGenerator>();
            RogueLikeData.Instance.SetResetMap(false);
            uIGenerator.RegenerateMap();
        }

        // 전투 끝나고 돌아왔을 경우만 갱신 요청
        GameManager.Instance.shouldRefreshUpgradeUI = true;

    }

    private void Start()
    {
        allStages.AddRange(FindObjectsOfType<StageNodeUI>());

        // 이어하기(불러오기) 처리
        var save = SaveSystem.LoadFull();
        if (save != null)
        {
            uIGenerator.RegenerateMapFromSaveFull(save);
            Debug.Log("저장된 맵을 불러왔습니다.");
        }
        else
        {
            uIGenerator.RegenerateMap();
            Debug.Log("새 맵을 생성합니다.");
        }

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

        //유산99
        RelicManager.RunJarOfDesire();

        // 첫 이동이거나, 현재 스테이지와 연결된 경우에만 이동
        if (currentStage == null)
            // 첫 이동엔 레벨 1만 허용
            if (clickedStage.level == 0)
            {
                Debug.Log(" → 첫 이동 허용 (레벨 1)");
                changemorale();
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
            changemorale();
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

        // --- Relic 56: 전투가 아닌 지역 이동 시 사기 -5 ---
        if (RelicManager.CheckRelicById(56))
        {
            bool isNonCombat = newStage.stageType != StageType.Combat &&
                               newStage.stageType != StageType.Elite &&
                               newStage.stageType != StageType.Boss;

            if (isNonCombat)
            {
                RogueLikeData.Instance.ChangeMorale(-5);
                UIManager.Instance.UpdateMorale();
                Debug.Log(" 전투가 없는 지역으로 이동 → 사기 5 감소 (Relic 56 효과)");
            }
        }

        // 2) 맵 UI 전체 잠금
        foreach (var s in allStages)
            s.LockStage();

        // --- 전투/엘리트/보스 스테이지 진입 처리 먼저 ---
        if (newStage.stageType == StageType.Combat ||
            newStage.stageType == StageType.Elite ||
            newStage.stageType == StageType.Boss)
        {
            // 적 정보 패널과 배치 패널을 동시에 표시
            enemyInfoPanel.SetActive(true);
            TogglePlacePanel(true);
            PlacePanelComponent.UpdateMaxUnitText();

            var enemies = LoadEnemyUnits(newStage.PresetID);
            var preset = StagePresetLoader.I.GetByID(newStage.PresetID);

            string cmdName = preset.Commander ?? "";
            var panel = enemyInfoPanel.GetComponent<EnemyInfoPanel>();
            panel.ShowEnemyInfo(newStage.stageType, enemies, cmdName, /*combined:*/ true);
            
            // 적 프리팹을 PlacePanel에 생성
            PlacePanelComponent.CreateEnemyPrefabs(enemies);
            
            // PlacePanel에 지휘관 정보 표시
            PlacePanelComponent.ShowCommanderInfo(cmdName);
            
            // PlacePanel에 전장효과 표시 (전투 스테이지만)
            if (newStage.stageType == StageType.Combat || 
                newStage.stageType == StageType.Elite || 
                newStage.stageType == StageType.Boss)
            {
                PlacePanelComponent.ShowBattlefieldEffect(newStage.battlefieldEffect);
                
                // 전장 효과를 fieldId로 설정 (AbilityManager에서 사용)
                int fieldId = MapGenerator.GetFieldIdFromBattlefieldEffect(newStage.battlefieldEffect);
                RogueLikeData.Instance.SetFieldId(fieldId);
            }
            
            
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
            // 기존 restUI.Show() 대신
            restPanel.SetActive(true);
            currentStage?.StopSelectableEffect();
            return;
        }
        else if (newStage.stageType == StageType.Event)
        {
            if (RelicManager.CheckRelicById(47))
            {
                var relic = RelicManager.GetRelicById(47);
                if (!relic.used)
                {
                    relic.used = true;
                    rewardUI.gameObject.SetActive(true);
                    rewardUI.CreateTeasureUI();
                    return;
                }
            }
            if (RelicManager.CheckRelicById(52))
            {
                var relic = RelicManager.GetRelicById(52);
                var vals = relic.GetAllValuesAsFloatListOrNull();
                if (vals != null)
                {
                    if (RogueLikeData.Instance.GetRandomFloat() >= vals[0])
                    {
                        RogueLikeData.Instance.AddReroll(1);
                    }
                }

            }
            currentStage?.StopSelectableEffect();
            eventManager.SetActive(true);
        }
        else if (newStage.stageType == StageType.Shop)
        {
            storeManager.SetActive(true);
            currentStage?.StopSelectableEffect();

            //유산40
            RelicManager.GetRelicById(40)?.Execute();
        }
        else if (newStage.stageType == StageType.Treasure)
        {
            rewardUI.gameObject.SetActive(true);
            currentStage?.StopSelectableEffect();
            rewardUI.CreateTeasureUI();
        }
        
        Debug.Log($"📌 SetCurrentStage: {newStage.level}_{newStage.row}");
    }


    /// 플레이어 마커를 해당 스테이지 UI 위치로 이동시킵니다.
    private void MovePlayerMarkerTo(StageNodeUI target)
    {
        if (playerMarker == null)
        {
            Debug.Log("marker null");
            return;
        }

        RectTransform rt = target.GetComponent<RectTransform>();
        Debug.Log($"📍 마커 이동 → {rt.anchoredPosition}");
        playerMarker.anchoredPosition = rt.anchoredPosition;
        // ✅ 첫 이동 시 마커를 활성화
        if (!playerMarker.gameObject.activeSelf)
        {
            playerMarker.gameObject.SetActive(true);
            Debug.Log("🟢 PlayerMarker 첫 활성화됨");
        }

    }
    public void InitializeStageLocks()
    {
        mapCanvas.SetActive(true);
        loadingPanel.SetActive(false);
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
        PlacePanel.SetActive(false);
        currentStage.UnlockStage();
        currentStage.StopSelectableEffect();
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

        // 특수 프리셋 190, 191, 192번인 경우 MapGenerator에서 동적 유닛 구성 가져오기
        List<int> unitIdList;
        if (presetID == 190 || presetID == 191 || presetID == 192)
        {
            var mapGenerator = FindObjectOfType<MapGenerator>();
            if (mapGenerator != null)
            {
                unitIdList = mapGenerator.GetSpecialPresetUnits(presetID);
                Debug.Log($"[GameManager] 특수 프리셋 {presetID}번의 동적 유닛 구성 사용 - 유닛 수: {unitIdList.Count}");
            }
            else
            {
                Debug.LogError($"[GameManager] MapGenerator를 찾을 수 없습니다. 프리셋 {presetID}번의 기본 UnitList 사용");
                unitIdList = preset.UnitList;
            }
        }
        else
        {
            unitIdList = preset.UnitList;
        }

        // 2) 프리셋의 UnitList(int idx 리스트) → UnitLoader로부터 복제해서 반환
        return unitIdList
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
        loadingPanel.SetActive(true);
        mapCanvas.SetActive(false);
        enemyInfoPanel.SetActive(false);
        PlacePanel.SetActive(false);
        restPanel.SetActive(false);
        IsPlaceMode = false;
    }
    public void SetCurrentStageNull()
    {
        currentStage = null;
    }

    public void CloseLoading()
    {
        loadingPanel.SetActive(false);
    }


    public void CloseAllUI()
    {
        eventManager.SetActive(false);
        storeManager.SetActive(false);
        unitDetail.gameObject.SetActive(false);
        restPanel.SetActive(false);
        enemyInfoPanel.SetActive(false);
        rewardUI.gameObject.SetActive(false);
    }

    public void OpenBattlePanel()
    {
        int presetId = RogueLikeData.Instance.GetPresetID();
        StageType type = RogueLikeData.Instance.GetCurrentStageType();
        enemyInfoPanel.SetActive(true);
        var enemies = LoadEnemyUnits(presetId);
        var preset = StagePresetLoader.I.GetByID(presetId);

        string cmdName = preset.Commander ?? "";
        /*string cmdSkill = !string.IsNullOrEmpty(preset.CommanderID)
                          ? SkillLoader.Instance.GetSkillNameById(preset.CommanderID)
                          : "";*/
        var panel = enemyInfoPanel.GetComponent<EnemyInfoPanel>();
        panel.ShowEnemyInfo(type, enemies, cmdName/*, cmdSkill*/);
        
        // PlacePanel에 지휘관 정보 표시
        PlacePanelComponent.ShowCommanderInfo(cmdName);
    }

    private void changemorale()
    {
        int morale = RogueLikeData.Instance.GetMorale();
        if (morale >= 70)
        {
            RogueLikeData.Instance.ChangeMorale(-10);
            Debug.Log($"📉 사기가 70 이상이므로 -10 감소 → 현재 사기: {RogueLikeData.Instance.GetMorale()}");

            // 사기 텍스트 UI 업데이트
            UIManager.Instance.UpdateMorale();
        }
    }
    public void UpdateAllUI()
    {
        lineUpBar.MakeUnitList();
        UIManager.Instance.UIUpdateAll();
    }


}
