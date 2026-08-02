using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.UI;
using System;

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
    [SerializeField] private GameObject topBarCanvas;         // 상단바 캔버스(씬 전환 시 표시/숨김 제어)
    [SerializeField] private GameObject nodeInfoUIImage;      // 노드 정보 UI 이미지(표시/숨김 토글용)
    [SerializeField] private Button nodeInfoButton;           // NodeInfoBTN (맵 전용 화면에서만 표시)
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
    private bool mapInitializedForScene = false;
    private bool restoredSavedGameForScene = false;
    private const float MinLoadingVisibleSeconds = 0.08f;
    private float loadingShownAt = float.NegativeInfinity;
    private Coroutine closeLoadingCoroutine;

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
    [SerializeField] private Button openUnitOrderBtn;
    private async void Awake()
    {
        if (Instance == null)
        {
            // 최초 인스턴스라면 여기서 고정
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _hasInitialized = true;
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

        if (topBarCanvas == null)
        {
            // 기존 씬 구성과 호환: 인스펙터 미할당 시 이름으로 자동 탐색
            Transform topBarTransform = transform.Find("TopBarCanvas");
            if (topBarTransform != null)
                topBarCanvas = topBarTransform.gameObject;
        }

        EnsureTutorialSystem();

        if (nodeInfoUIImage != null)
            nodeInfoUIImage.SetActive(false);
      

        HideAllPanels();
        SceneManager.sceneLoaded += OnSceneLoaded;

        UnitLoader.Instance.LoadUnitsFromJson();
        SaveData save = new();
        save.LoadData();
        EventManager.LoadEventData();
        StoreManager.LoadStoreData();
        GameTextDB.Boot();
        RelicManager.InitializeRelicCatalog();

        if (openUnitOrderBtn != null)
        {
            openUnitOrderBtn.onClick.RemoveAllListeners();
            openUnitOrderBtn.onClick.AddListener(ClickOpenUnitOrderUI);
        }
    }

    /// <summary>전투 씬 등으로 RLmap이 언로드될 때 씬 오브젝트 참조가 끊깁니다. 맵 씬 로드 직후 UIGenerator·MapGenerator를 다시 잡습니다.</summary>
    private void EnsureMapSceneUIReferences()
    {
        if (uIGenerator == null)
        {
            uIGenerator = FindAnyObjectByType<UIGenerator>();
            if (uIGenerator == null && transform.childCount > 0)
            {
                Transform c0 = transform.GetChild(0);
                if (c0.childCount > 0)
                    uIGenerator = c0.GetChild(0).GetComponent<UIGenerator>();
            }
        }

        if (uIGenerator != null)
        {
            uIGenerator.EnsureMapGeneratorReference();
            uIGenerator.RehydrateMapFromExistingUIIfNeeded();
        }

        EnsureNodeInfoButtonReference();
    }

    private void EnsureTutorialSystem()
    {
        Transform tutorialRoot = null;

        if (topBarCanvas != null)
            tutorialRoot = topBarCanvas.transform.Find("TutorialObj");

        if (tutorialRoot == null)
            tutorialRoot = transform.Find("TopBarCanvas/TutorialObj");

        if (tutorialRoot == null)
            return;

        if (!tutorialRoot.gameObject.activeSelf)
            tutorialRoot.gameObject.SetActive(true);

        TutorialPopupUI popup = tutorialRoot.GetComponent<TutorialPopupUI>();
        if (popup == null)
            popup = tutorialRoot.gameObject.AddComponent<TutorialPopupUI>();

        TutorialService service = tutorialRoot.GetComponent<TutorialService>();
        if (service == null)
            service = tutorialRoot.gameObject.AddComponent<TutorialService>();

        service.ConfigurePopup(popup);
    }

    private void EnsureNodeInfoButtonReference()
    {
        if (nodeInfoButton != null)
            return;
        if (SceneManager.GetActiveScene().name != "RLmap")
            return;

        foreach (var btn in FindObjectsOfType<Button>(true))
        {
            if (btn.gameObject.name == "NodeInfoBTN")
            {
                nodeInfoButton = btn;
                return;
            }
        }
    }

    private bool IsMapPanelViewActive()
    {
        if (SceneManager.GetActiveScene().name != "RLmap")
            return false;

        if (mapCanvas == null || !mapCanvas.activeInHierarchy)
            return false;

        if (loadingPanel != null && loadingPanel.activeInHierarchy)
            return false;

        if (enemyInfoPanel != null && enemyInfoPanel.activeInHierarchy)
            return false;

        if (PlacePanel != null && PlacePanel.activeInHierarchy)
            return false;

        if (restPanel != null && restPanel.activeInHierarchy)
            return false;

        if (eventManager != null && eventManager.activeInHierarchy)
            return false;

        if (storeManager != null && storeManager.activeInHierarchy)
            return false;

        if (rewardUI != null && rewardUI.IsTreasureRewardVisible)
            return false;

        return true;
    }

    public void RefreshNodeInfoButtonVisibility() => RefreshNodeInfoButton();

    private void RefreshNodeInfoButton()
    {
        EnsureNodeInfoButtonReference();
        if (nodeInfoButton == null)
            return;

        bool show = IsMapPanelViewActive();
        nodeInfoButton.gameObject.SetActive(show);
        nodeInfoButton.interactable = show;

        if (!show && nodeInfoUIImage != null && nodeInfoUIImage.activeSelf)
            nodeInfoUIImage.SetActive(false);
    }

private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
 {
     SetUnitListToggleVisible(scene.name == "RLmap");
     if (scene.name != "RLmap")
     {
        mapInitializedForScene = false;
        restoredSavedGameForScene = false;
        // GameManager가 DontDestroyOnLoad라서 RLmap UI가 남아있을 수 있으므로
        // 타이틀/전투 등 RLmap 외 씬 진입 시에는 관련 패널을 즉시 정리한다.
        if(scene.name != "Title")
            {
                HideAllPanels();

            }
            CloseAllUI();
        // 전투 씬에서는 옵션/유물 등을 위해 상단바 유지 (타이틀 등 그 외 씬에서는 숨김)
        SetTopBarCanvasVisible(scene.name == "AutoBattleScene");
        RefreshNodeInfoButton();
        return;
     }

        mapInitializedForScene = false;
        restoredSavedGameForScene = false;

        // RLmap 복귀 시에는 상단바를 다시 표시
        SetTopBarCanvasVisible(true);

        EnsureMapSceneUIReferences();
        EnsureTutorialSystem();

        CloseAllUI();

        // 맵 씬에 진입했을 때만
        allStages = FindObjectsOfType<StageNodeUI>().ToList();
        HideAllPanels();
        UIManager.Instance.UIUpdateAll();

        if (SaveData.HasContinueLoadRequest())
        {
            mapInitializedForScene = LoadSavedGame();
            SaveData.ClearContinueLoadRequest();

            if (mapInitializedForScene)
            {
                GameManager.Instance.shouldRefreshUpgradeUI = true;
                RefreshNodeInfoButton();
                return;
            }
        }

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
            Debug.Log("✅ mapPanel의 PosX를 0으로 초기화"+ RogueLikeData.Instance.GetChapter());
            // 🔽 챕터 텍스트 업데이트
            UIManager.Instance.UpdateChapter(RogueLikeData.Instance.GetChapter());
            
            if (uIGenerator == null) uIGenerator = transform.GetChild(0).GetChild(0).GetComponent<UIGenerator>();
            uIGenerator.RegenerateMap();
            mapInitializedForScene = true;
        }
        else if (RogueLikeData.Instance.GetResetMap())
        {
            SetCurrentStageNull();
            if (uIGenerator == null) uIGenerator = transform.GetChild(0).GetChild(0).GetComponent<UIGenerator>();
            RogueLikeData.Instance.SetResetMap(false);
            uIGenerator.RegenerateMap();
            mapInitializedForScene = true;
        }
        else
        {
            InitializeStageLocks();
        }

        // 전투 끝나고 돌아왔을 경우만 갱신 요청
        GameManager.Instance.shouldRefreshUpgradeUI = true;
        RefreshNodeInfoButton();

    }

    public void UpdateChapter() { 
    }

    private void Start()
    {
        allStages.AddRange(FindObjectsOfType<StageNodeUI>());

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "RLmap")
            return;

        if (mapInitializedForScene)
        {
            if (!restoredSavedGameForScene)
                InitializeStageLocks();
            return;
        }

        if (SaveData.HasContinueLoadRequest())
        {
            mapInitializedForScene = LoadSavedGame();
            SaveData.ClearContinueLoadRequest();
            if (mapInitializedForScene)
                return;
        }

        // 새 게임은 항상 새 맵 생성
        uIGenerator.RegenerateMap();
        mapInitializedForScene = true;
        Debug.Log("새 맵을 생성합니다.");

        InitializeStageLocks();
    }
    
    /// <summary>
    /// 저장된 게임을 불러옵니다. (불러오기 버튼에서 호출)
    /// </summary>
    public bool LoadSavedGame()
    {
        SaveData saveData = new();
        if (saveData.LoadGame(out StageFullSaveData save))
        {
            EnsureMapSceneUIReferences();
            uIGenerator.RegenerateMapFromSaveFull(save);
            
            // RogueLikeData에 저장된 플레이어 위치로 currentStage 복원
            RestorePlayerPosition();
            
            // 잠금 상태 업데이트
            InitializeStageLocks();
            RestoreSavedProgressUI();
            
            Debug.Log("✅ 저장된 맵을 불러왔습니다.");
            restoredSavedGameForScene = true;
            return true;
        }
        else
        {
            Debug.LogWarning("⚠️ 불러올 저장 데이터가 없습니다.");
            return false;
        }
    }

    public void OnStageClicked(StageNodeUI clickedStage)
    {
        // 디버그: 클릭된 정보 찍기
        Debug.Log($"OnStageClicked → chapter:{clickedStage.chapter}, level:{clickedStage.level}, row:{clickedStage.row}, locked:{clickedStage.IsLocked}, currentStage:{(currentStage == null ? "null" : currentStage.level.ToString())}");
        
        // 챕터는 진행 데이터(보스 클리어 보상)로만 변경하고, 노드 클릭으로는 변경하지 않는다.
        // 노드의 chapter 값이 기본값(1)으로 남아 있으면 챕터가 되돌아가 전투 프리셋 분기가 깨질 수 있다.
        int persistedChapter = RogueLikeData.Instance.GetChapter();
        Debug.Log($"📌 현재 챕터 유지: {persistedChapter} (노드 표기:{clickedStage.chapter})");
        
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

        // 48번 무지개 열쇠: 연결되지 않은 지역으로 이동 가능(챕터당 2회)
        // 단, "현재보다 높은 모든 스테이지"가 아니라 "다음 레벨(current+1)"만 허용.
        bool useRainbowKey = false;
        if (!isConnected && RelicManager.CheckRelicById(48))
        {
            int currentChapter = RogueLikeData.Instance.GetChapter();
            // ✅ 현재 스테이지 재플레이 방지: 다음 레벨만 허용
            bool isNextLevel = clickedStage.level == currentStage.level + 1;

            if (!isNextLevel)
            {
                Debug.Log($"[무지개 열쇠] 다음 레벨 스테이지만 이동 가능합니다. (현재:{currentStage.level}, 클릭:{clickedStage.level})");
            }
            else if (RogueLikeData.Instance.CanUseRainbowKey(currentChapter))
            {
                useRainbowKey = true;
                RogueLikeData.Instance.UseRainbowKey(currentChapter);
                int uses = RogueLikeData.Instance.GetRainbowKeyUses(currentChapter);
                Debug.Log($"[무지개 열쇠] 연결되지 않은 다음 레벨로 이동합니다. (사용 횟수: {uses}/2)");
            }
        }

        if (isConnected || useRainbowKey)
        {
            changemorale();
            SetCurrentStage(clickedStage);
        }
        else
        {
            Debug.Log(" → 해당 스테이지로 이동할 수 없습니다.");
        }
    }


    /// <summary>UIGenerator에 연결된 맵과 동일한 <see cref="MapGenerator"/>를 고릅니다. 씬에 MapGenerator가 여러 개일 때 잘못된 인스턴스를 피합니다.</summary>
    private MapGenerator ResolveMapGeneratorForNode(int level, int row)
    {
        if (uIGenerator != null)
            uIGenerator.RehydrateMapFromExistingUIIfNeeded();

        string key = $"{level}_{row}";
        MapGenerator linked = uIGenerator != null ? uIGenerator.mapGenerator : null;
        if (linked != null && linked.NodeDictionary != null && linked.NodeDictionary.ContainsKey(key))
            return linked;

        foreach (var mg in FindObjectsOfType<MapGenerator>(true))
        {
            if (mg != null && mg.NodeDictionary != null && mg.NodeDictionary.ContainsKey(key))
                return mg;
        }

        if (linked != null && linked.NodeDictionary != null && linked.NodeDictionary.Count > 0)
            return linked;

        foreach (var mg in FindObjectsOfType<MapGenerator>(true))
        {
            if (mg != null && mg.NodeDictionary != null && mg.NodeDictionary.Count > 0)
                return mg;
        }

        return linked;
    }

    private static bool TryGetStageNodeFromMap(MapGenerator mapGen, int level, int row, out StageNode stageNode)
    {
        stageNode = null;
        if (mapGen == null || mapGen.NodeDictionary == null)
            return false;

        string key = $"{level}_{row}";
        if (mapGen.NodeDictionary.TryGetValue(key, out stageNode))
            return true;

        foreach (var kv in mapGen.NodeDictionary)
        {
            StageNode n = kv.Value;
            if (n != null && n.level == level && n.row == row)
            {
                stageNode = n;
                return true;
            }
        }

        return false;
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
            var preset = StagePresetLoader.I != null ? StagePresetLoader.I.GetByID(newStage.PresetID) : null;

            string cmdName = preset?.Commander ?? "";
            int? eliteCmdId = preset?.CommanderNumericId;
            var panel = enemyInfoPanel.GetComponent<EnemyInfoPanel>();
            panel.ShowEnemyInfo(newStage.stageType, enemies, cmdName, /*combined:*/ true, eliteCmdId);
            
            // 적 프리팹을 PlacePanel에 생성
            PlacePanelComponent.CreateEnemyPrefabs(enemies);
            
            // PlacePanel에 지휘관 정보 표시
            PlacePanelComponent.ShowCommanderInfo(cmdName, newStage.stageType, eliteCmdId);
            
            // PlacePanel에 전장효과 표시 (전투 스테이지만)
            if (newStage.stageType == StageType.Combat || 
                newStage.stageType == StageType.Elite || 
                newStage.stageType == StageType.Boss)
            {
                PlacePanelComponent.ShowBattlefieldEffect(newStage.battlefieldEffect);



                // 전장 효과를 fieldId로 설정 (AbilityManager에서 사용)
                int fieldId = RogueLikeData.Instance.GetFieldId();

                fieldId = MapGenerator.GetFieldIdFromBattlefieldEffect(newStage.battlefieldEffect);


                RogueLikeData.Instance.SetFieldId(fieldId);
            }

            RogueLikeData.Instance.BeginBattleResumeSnapshot(false);
            RogueLikeData.Instance.SaveNow();
            EnqueueBattlePreparationTutorials(newStage.stageType);
            RefreshNodeInfoButton();
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

        UnlockRainbowKeyDisconnectedBranches(newStage, allStages);

        // 47번 보물지도: 다음 이벤트를 보물로 변환(진입 시 1회 소진)
        if (newStage.stageType == StageType.Event && RogueLikeData.Instance.GetNextEventToTreasure())
        {
            MapGenerator mapGen = ResolveMapGeneratorForNode(newStage.level, newStage.row);
            string nodeKey = $"{newStage.level}_{newStage.row}";
            bool converted = false;
            if (TryGetStageNodeFromMap(mapGen, newStage.level, newStage.row, out var stageNode))
            {
                stageNode.stageType = StageType.Treasure;
                newStage.stageType = StageType.Treasure;
                newStage.Setup(stageNode);
                RogueLikeData.Instance.SetCurrentStage(newStage.level, newStage.row, StageType.Treasure);
                converted = true;
                Debug.Log($"[보물지도] 이벤트 지역이 보물 지역으로 변경됨: {nodeKey}");
            }
            else
            {
                int dictCount = mapGen != null && mapGen.NodeDictionary != null ? mapGen.NodeDictionary.Count : -1;
                Debug.LogWarning(
                    $"[보물지도] 변환 실패 — 노드 '{nodeKey}' 없음 (MapGenerator='{(mapGen != null ? mapGen.name : "null")}', nodeDict.Count={dictCount}). 플래그는 유지됩니다.");
            }

            if (converted)
                RogueLikeData.Instance.SetNextEventToTreasure(false);
        }

        // 6) 타입별 처리
        if (newStage.stageType == StageType.Rest)
        {
            // 기존 restUI.Show() 대신
            RogueLikeData.Instance.SetProgressState(SaveProgressState.RestOpen);
            RogueLikeData.Instance.SaveNow();
            restPanel.SetActive(true);
            currentStage?.StopSelectableEffect();
            RefreshNodeInfoButton();
            return;
        }
        else if (newStage.stageType == StageType.Event)
        {
            // 47번 보물지도: (구버전 로직) 이벤트 진입 시 보상창을 바로 띄우는 방식은 제거.
            // 현재는 nextEventToTreasure 플래그를 통해 "다음 이벤트가 보물로 변환"되는 방식으로 처리한다.
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
            //rewardUI.gameObject.SetActive(true);
            RogueLikeData.Instance.SetProgressState(SaveProgressState.TreasureOpen);
            RogueLikeData.Instance.SaveNow();
            currentStage?.StopSelectableEffect();
            rewardUI.SetActiveTeasureBox();
        }

        RefreshNodeInfoButton();
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

    /// <summary>
    /// 48번 무지개 열쇠: 챕터당 사용 가능 횟수가 남아 있으면, 다음 레벨 중 anchor와 연결되지 않은 잠긴 스테이지를 언락합니다.
    /// </summary>
    private void UnlockRainbowKeyDisconnectedBranches(StageNodeUI anchorStage, List<StageNodeUI> stages)
    {
        if (anchorStage == null || stages == null || stages.Count == 0)
            return;
        if (!RelicManager.CheckRelicById(48))
            return;
        int chapter = RogueLikeData.Instance.GetChapter();
        if (!RogueLikeData.Instance.CanUseRainbowKey(chapter))
            return;
        int nextLevel = anchorStage.level + 1;
        int remainingUses = 2 - RogueLikeData.Instance.GetRainbowKeyUses(chapter);
        foreach (var stage in stages)
        {
            if (stage.level != nextLevel || !stage.IsLocked)
                continue;
            if (anchorStage.connectedStages.Contains(stage))
                continue;
            stage.UnlockStage();
            Debug.Log($"[무지개 열쇠] 연결되지 않은 지역 언락: 레벨 {nextLevel} (사용 가능 횟수: {remainingUses})");
        }
    }

    public void InitializeStageLocks()
    {
        mapCanvas.SetActive(true);
        loadingPanel.SetActive(false);
        if (unitListUI != null && unitListUI.gameObject.activeSelf)
            unitListUI.gameObject.SetActive(false);
        SetUnitListToggleVisible(true);
        // 1) 씬 안의 모든 StageNodeUI 다시 가져오기
        var all = FindObjectsOfType<StageNodeUI>().ToList();
        
        // 2) 일단 전부 잠급니다 (이것이 "지나온 곳 잠금" 효과)
        foreach (var s in all)
            s.LockStage();

        // 3) 첫 맵 진입(아직 어느 스테이지도 찍지 않았다면) → level==0·Combat 만 언락
        if (currentStage == null)
        {
            // 레벨 0 & Combat 타입인 노드를 전부 찾아서 언락
            foreach (var s in all.Where(s => s.level == 0 && s.stageType == StageType.Combat))
            {
                s.UnlockStage();
            }
            Debug.Log("🆕 첫 진입: 레벨 0 전투 스테이지 해제");
            RefreshNodeInfoButton();
            NotifyMapTutorialReady();
            return;
        }

        // 4) 불러오기 또는 맵 복귀 시: 지나온 곳은 잠금, 현재+다음만 해제
        int currentLevel = currentStage.level;
        
        // 4-1) 현재 레벨 이전의 모든 스테이지는 잠김 상태 유지 (지나온 곳)
        //      → 이미 위에서 모두 잠갔으므로 추가 작업 불필요
        
        // 4-2) 현재 위치는 해제하지만 선택 효과는 중지 (이미 있는 위치)
        currentStage.UnlockStage();
        currentStage.StopSelectableEffect();

        SaveProgressState progressState = RogueLikeData.Instance.GetProgressState();
        if (progressState == SaveProgressState.BattlePending ||
            progressState == SaveProgressState.BattlePlacement ||
            progressState == SaveProgressState.BattleInProgress)
        {
            enemyInfoPanel.SetActive(false);
            PlacePanel.SetActive(false);
            RefreshNodeInfoButton();
            return;
        }
        
        // 4-3) 현재 위치와 연결된 다음 스테이지만 해제 (앞으로 진행 가능)
        foreach (var nxt in currentStage.connectedStages)
        {
            // 다음 스테이지가 "다음 레벨"인 경우만 해제
            if (nxt.level == currentLevel + 1)
            {
                nxt.UnlockStage(); // 선택 가능 효과도 자동 시작
            }
            else
            {
                // 같은 레벨의 다른 경로는 잠금 유지 (되돌아가기 방지)
                nxt.LockStage();
            }
        }

        UnlockRainbowKeyDisconnectedBranches(currentStage, all);

        Debug.Log($"🔓 레벨 {currentLevel} 이전 스테이지 잠금, 다음 스테이지 해제");
        
        enemyInfoPanel.SetActive(false);
        PlacePanel.SetActive(false);
        RefreshNodeInfoButton();
        NotifyMapTutorialReady();
    }

    private void NotifyMapTutorialReady()
    {
        if (RogueLikeData.Instance != null &&
            RogueLikeData.Instance.GetProgressState() != SaveProgressState.StageSelect)
            return;

        if (!IsMapPanelViewActive())
            return;

        TutorialHook.EnqueueAndNotifyCurrentStage(TutorialId.INF_01_ROUTE, "Map");
        TutorialHook.NotifyMapHudResource();
    }

    private void EnqueueBattlePreparationTutorials(StageType stageType)
    {
        TutorialHook.EnqueueStageTypeGuide(stageType, "BattlePrep");
        TutorialHook.EnqueueCurrentStage(TutorialId.INF_02_DEPLOY_AUTO, "BattlePrep");

        if (stageType == StageType.Elite || stageType == StageType.Boss)
            TutorialHook.EnqueueCurrentStage(TutorialId.INF_04_BATTLE_INFO, "BattlePrep");

        TutorialHook.NotifyCurrentStage("BattlePrep");
    }

    private List<RogueUnitDataBase> LoadEnemyUnits(int presetID)
    {
        int chapter = RogueLikeData.Instance.GetChapter();
        StageType stageType = RogueLikeData.Instance.GetCurrentStageType();
        
        // ✅ 챕터 2 이상 + normal 스테이지일 때만 예산 기반 구성 사용
        if (chapter >= 2 && stageType == StageType.Combat)
        {
            Debug.Log($"[GameManager] 챕터 {chapter} Normal 스테이지 - 예산 기반 구성 사용");
            
            // EnemyBudgetComposer 존재 확인
            if (EnemyBudgetComposer.Instance == null)
            {
                Debug.LogError("❌ [GameManager] EnemyBudgetComposer가 씬에 없습니다! GameManager 오브젝트에 EnemyBudgetComposer 컴포넌트를 추가해주세요.");
                Debug.LogError("→ 임시로 프리셋 기반 구성을 사용합니다.");
                
                // 프리셋 기반으로 폴백
                var fallbackPreset = StagePresetLoader.I != null ? StagePresetLoader.I.GetByID(presetID) : null;
                if (fallbackPreset != null && fallbackPreset.UnitList != null)
                {
                    var fallbackEnemies = fallbackPreset.UnitList
                        .Select(idx => UnitLoader.Instance.GetCloneUnitById(idx, false))
                        .Where(u => u != null)
                        .ToList();
                    RogueLikeData.Instance.SetAllEnemyUnits(fallbackEnemies);
                    return fallbackEnemies;
                }
                var emptyEnemies = new List<RogueUnitDataBase>();
                RogueLikeData.Instance.SetAllEnemyUnits(emptyEnemies);
                return emptyEnemies;
            }
            
            int budget = CalculateEnemyBudget(chapter, currentStage?.level ?? 0);
            var composition = EnemyBudgetComposer.Instance.ComposeEnemyArmy(budget);
            var enemies = composition.finalComposition ?? new List<RogueUnitDataBase>();
            RogueLikeData.Instance.SetAllEnemyUnits(enemies);
            return enemies;
        }
        
        // ✅ 그 외(챕터 1, 엘리트, 보스) - 기존 방식: StagePresets.json 사용
        Debug.Log($"[GameManager] 챕터 {chapter} {stageType} 스테이지 - 기존 프리셋 방식 사용");
        
        // 1) StagePresetLoader에서 프리셋 가져오기
        var preset = StagePresetLoader.I != null ? StagePresetLoader.I.GetByID(presetID) : null;
        if (preset == null)
        {
            Debug.LogError($"[GameManager] Preset {presetID} 을(를) 찾을 수 없습니다.");
            var emptyEnemies = new List<RogueUnitDataBase>();
            RogueLikeData.Instance.SetAllEnemyUnits(emptyEnemies);
            return emptyEnemies;
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

        unitIdList ??= new List<int>();

        if ((presetID == 190 || presetID == 191 || presetID == 192) && unitIdList != null && unitIdList.Count > 0)
        {
            preset.UnitList = new List<int>(unitIdList);
            preset.UnitCount = unitIdList.Count;
        }

        // 2) 프리셋의 UnitList(int idx 리스트) → UnitLoader로부터 복제해서 반환
        var presetEnemies = unitIdList
                     .Select(idx => UnitLoader.Instance.GetCloneUnitById(idx, /*isTeam=*/ false))
                     .Where(u => u != null)
                     .ToList();
        RogueLikeData.Instance.SetAllEnemyUnits(presetEnemies);
        return presetEnemies;
    }
    
    /// <summary>
    /// 챕터와 레벨에 따른 적 부대 예산 계산
    /// </summary>
    private int CalculateEnemyBudget(int chapter, int level)
    {
        // 챕터 2 예산 테이블 (0-based: 레벨 0~12)
        var chapter2Budgets = new Dictionary<int, int>
        {
            { 0, 1200 },   // 레벨 1 (표시용)
            { 1, 1300 },   // 레벨 2
            { 2, 1400 },   // 레벨 3
            { 3, 1500 },   // 레벨 4
            { 4, 1650 },   // 레벨 5
            { 5, 1800 },   // 레벨 6
            { 6, 1950 },   // 레벨 7
            { 8, 2150 },   // 레벨 9 (레벨 8은 비전투)
            { 9, 2300 },   // 레벨 10
            { 10, 2450 },  // 레벨 11
            { 11, 2600 },  // 레벨 12
            { 12, 2750 }   // 레벨 13
        };
        
        // 챕터 3 예산 테이블 (0-based: 레벨 0~12)
        var chapter3Budgets = new Dictionary<int, int>
        {
            { 0, 3000 },   // 레벨 1 (표시용)
            { 1, 3200 },   // 레벨 2
            { 2, 3400 },   // 레벨 3
            { 3, 3600 },   // 레벨 4
            { 4, 3800 },   // 레벨 5
            { 5, 4000 },   // 레벨 6
            { 6, 4000 },   // 레벨 7
            { 8, 4250 },   // 레벨 9 (레벨 8은 비전투)
            { 9, 4250 },   // 레벨 10
            { 10, 4500 },  // 레벨 11
            { 11, 4500 },  // 레벨 12
            { 12, 4750 }   // 레벨 13
        };
        
        int totalBudget = 2000; // 기본값
        
        if (chapter == 2 && chapter2Budgets.ContainsKey(level))
        {
            totalBudget = chapter2Budgets[level];
        }
        else if (chapter == 3 && chapter3Budgets.ContainsKey(level))
        {
            totalBudget = chapter3Budgets[level];
        }
        else
        {
            Debug.LogWarning($"[예산 계산] 챕터 {chapter}, 레벨 {level}에 대한 예산이 정의되지 않음 → 기본값 사용: {totalBudget}");
        }
        
        Debug.Log($"[예산 계산] 챕터 {chapter}, 레벨 {level} (표시: 레벨 {level + 1}) → 예산: {totalBudget}");
        
        return totalBudget;
    }
    public void TogglePlacePanel(bool open)
    {
        PlacePanel.SetActive(open);
        IsPlaceMode = open;
        RefreshNodeInfoButton();
    }

    public bool TryCaptureBattlePlacementSnapshot()
    {
        if (RogueLikeData.Instance.GetProgressState() != SaveProgressState.BattlePlacement)
            return false;

        if (PlacePanel == null || !PlacePanel.activeInHierarchy)
            return false;

        PlacePanel placePanel = PlacePanelComponent;
        if (placePanel == null)
            return false;

        RogueLikeData.Instance.UpdateBattleResumePlacement(placePanel.PlacedUniqueIds);
        return true;
    }
    
    public void HideAllPanels()
    {
        ShowLoadingPanel();
        if (unitListUI != null && unitListUI.gameObject.activeSelf)
            unitListUI.gameObject.SetActive(false);
        SetUnitListToggleVisible(false);
        mapCanvas.SetActive(false);
        enemyInfoPanel.SetActive(false);
        PlacePanel.SetActive(false);
        restPanel.SetActive(false);
        IsPlaceMode = false;
        RefreshNodeInfoButton();
    }

    private void SetTopBarCanvasVisible(bool visible)
    {
        if (topBarCanvas == null)
        {
            Transform topBarTransform = transform.Find("TopBarCanvas");
            if (topBarTransform != null)
                topBarCanvas = topBarTransform.gameObject;
        }

        if (topBarCanvas != null)
            topBarCanvas.SetActive(visible);
    }
    public void SetCurrentStageNull()
    {
        currentStage = null;
    }

    public bool TryGetCurrentStagePosition(out int level, out int row, out StageType type)
    {
        if (currentStage == null)
        {
            level = -1;
            row = -1;
            type = StageType.Unknown;
            return false;
        }

        level = currentStage.level;
        row = currentStage.row;
        type = currentStage.stageType;
        return true;
    }

    public void CloseLoading()
    {
        if (loadingPanel == null)
            return;

        if (closeLoadingCoroutine != null)
        {
            StopCoroutine(closeLoadingCoroutine);
            closeLoadingCoroutine = null;
        }

        float remainingSeconds = MinLoadingVisibleSeconds - (Time.unscaledTime - loadingShownAt);
        if (remainingSeconds <= 0f)
        {
            loadingPanel.SetActive(false);
            return;
        }

        closeLoadingCoroutine = StartCoroutine(CloseLoadingAfterDelay(remainingSeconds));
    }

    private void ShowLoadingPanel()
    {
        if (loadingPanel == null)
            return;

        if (closeLoadingCoroutine != null)
        {
            StopCoroutine(closeLoadingCoroutine);
            closeLoadingCoroutine = null;
        }

        loadingShownAt = Time.unscaledTime;
        loadingPanel.SetActive(true);
    }

    private IEnumerator CloseLoadingAfterDelay(float delaySeconds)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        closeLoadingCoroutine = null;
    }


    public void CloseAllUI()
    {
        eventManager.SetActive(false);
        storeManager.SetActive(false);
        unitDetail.gameObject.SetActive(false);
        if (unitListUI != null && unitListUI.gameObject.activeSelf)
            unitListUI.gameObject.SetActive(false);
        restPanel.SetActive(false);
        enemyInfoPanel.SetActive(false);
        //rewardUI.gameObject.SetActive(false);
        RefreshNodeInfoButton();
    }

    public void OpenBattlePanel()
    {
        int presetId = RogueLikeData.Instance.GetPresetID();
        StageType type = RogueLikeData.Instance.GetCurrentStageType();
        BattleResumeSnapshot resume = RogueLikeData.Instance.GetBattleResumeSnapshot();
        bool hasResume = resume != null && resume.active;

        if (hasResume)
        {
            if (resume.presetID >= 0)
                presetId = resume.presetID;

            type = resume.stageType;
        }

        OpenBattlePlacePanel(presetId, type, currentStage != null ? currentStage.battlefieldEffect : null);

        if (hasResume)
            RogueLikeData.Instance.SetFieldId(resume.fieldId);
    }

    private void RestoreSavedProgressUI()
    {
        SaveProgressState state = RogueLikeData.Instance.GetProgressState();

        switch (state)
        {
            case SaveProgressState.StoreOpen:
                storeManager.SetActive(true);
                currentStage?.StopSelectableEffect();
                break;

            case SaveProgressState.EventOpen:
            case SaveProgressState.EventResult:
                eventManager.SetActive(true);
                currentStage?.StopSelectableEffect();
                break;

            case SaveProgressState.BattlePending:
            case SaveProgressState.BattlePlacement:
            case SaveProgressState.BattleInProgress:
                OpenBattlePanel();
                RestoreBattlePlacementSnapshot();
                currentStage?.StopSelectableEffect();
                if (state == SaveProgressState.BattleInProgress)
                    StartCoroutine(ResumeBattleFromSavedPlacementNextFrame());
                break;

            case SaveProgressState.RestOpen:
                restPanel.SetActive(true);
                currentStage?.StopSelectableEffect();
                break;

            case SaveProgressState.TreasureOpen:
                rewardUI.SetActiveTeasureBox();
                currentStage?.StopSelectableEffect();
                break;

            case SaveProgressState.RewardOpen:
                rewardUI.CreateRewardUI();
                currentStage?.StopSelectableEffect();
                break;
        }

        RefreshNodeInfoButton();
    }

    private void RestoreBattlePlacementSnapshot()
    {
        PlacePanel placePanel = PlacePanelComponent;
        if (placePanel == null)
            return;

        List<RogueUnitDataBase> placedUnits = RogueLikeData.Instance.GetBattleResumePlacedUnits();
        if (placedUnits.Count > 0)
            placePanel.RestorePlacedUnits(placedUnits);
    }

    private IEnumerator ResumeBattleFromSavedPlacementNextFrame()
    {
        yield return null;

        PlacePanel placePanel = PlacePanelComponent;
        if (placePanel == null)
            yield break;

        if (!placePanel.StartBattleFromResume())
        {
            RogueLikeData.Instance.SetProgressState(SaveProgressState.BattlePlacement);
            RogueLikeData.Instance.SaveNow();
        }
    }

    private void OpenBattlePlacePanel(int presetId, StageType stageType, BattlefieldEffect? battlefieldEffect)
    {
        PlacePanel placePanel = PlacePanelComponent;
        if (placePanel == null)
        {
            Debug.LogError("[GameManager] PlacePanel 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        List<RogueUnitDataBase> enemies = LoadEnemyUnits(presetId);

        var preset = StagePresetLoader.I != null ? StagePresetLoader.I.GetByID(presetId) : null;
        string commanderName = preset?.Commander ?? "";
        int? eliteCommanderId = preset?.CommanderNumericId;

        if (enemyInfoPanel != null)
            enemyInfoPanel.SetActive(false);

        TogglePlacePanel(true);

        placePanel.ClearPlacePanel();
        placePanel.UpdateMaxUnitText();
        placePanel.CreateEnemyPrefabs(enemies);
        placePanel.ShowCommanderInfo(commanderName, stageType, eliteCommanderId);

        if (battlefieldEffect.HasValue)
        {
            placePanel.ShowBattlefieldEffect(battlefieldEffect.Value);

            int fieldId = MapGenerator.GetFieldIdFromBattlefieldEffect(battlefieldEffect.Value);
            RogueLikeData.Instance.SetFieldId(fieldId);
        }

        EnqueueBattlePreparationTutorials(stageType);
        RefreshNodeInfoButton();
    }

    public void ToggleNodeInfoUI()
    {
        if (!IsMapPanelViewActive())
            return;

        if (nodeInfoUIImage == null)
        {
            Debug.LogWarning("GameManager: nodeInfoUIImage가 연결되지 않았습니다.");
            return;
        }

        nodeInfoUIImage.SetActive(!nodeInfoUIImage.activeSelf);
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

    /// <summary>
    /// RogueLikeData에 저장된 플레이어 위치를 기반으로 currentStage를 복원합니다.
    /// </summary>
    private void RestorePlayerPosition()
    {
        // RogueLikeData에서 저장된 위치 가져오기
        var (x, y, type) = RogueLikeData.Instance.GetCurrentStage();
        
        // 위치가 유효한지 확인 (초기값 -1이 아닌 경우)
        if (x < 0 || y < 0)
        {
            Debug.Log("💡 저장된 플레이어 위치가 없습니다. 새 게임으로 시작합니다.");
            return;
        }
        
        // 해당 위치의 StageNodeUI 찾기
        var allStages = FindObjectsOfType<StageNodeUI>();
        foreach (var stageUI in allStages)
        {
            if (stageUI.level == x && stageUI.row == y)
            {
                currentStage = stageUI;
                Debug.Log($"✅ 플레이어 위치 복원: Level {x}, Row {y}, Type {type}");
                
                // 마커도 해당 위치로 이동
                if (playerMarker != null)
                {
                    MovePlayerMarkerTo(currentStage);
                }
                
                return;
            }
        }
        
        Debug.LogWarning($"⚠️ Level {x}, Row {y}에 해당하는 스테이지를 찾을 수 없습니다.");
    }

    public void SetUnitListToggleVisible(bool visible)
    {
        if (openUnitOrderBtn == null)
            return;

        bool shouldShow = visible && SceneManager.GetActiveScene().name == "RLmap";
        openUnitOrderBtn.gameObject.SetActive(shouldShow);

        if (shouldShow)
            SetUnitListToggleOpenState(unitListUI != null && unitListUI.gameObject.activeInHierarchy);
    }

    public void SetUnitListToggleOpenState(bool opened)
    {
        if (openUnitOrderBtn == null)
            return;

        Image img = openUnitOrderBtn.GetComponent<Image>();
        if (img == null)
            return;

        img.sprite = SpriteCacheManager.GetSprite(opened ? "KIcon/UI/Img_CloseUnit" : "KIcon/UI/Img_OpenUnit");
    }

    private void ClickOpenUnitOrderUI()
    {
        if (unitListUI == null || openUnitOrderBtn == null)
            return;

        if (unitListUI.IsSelectionModeActive)
            return;

        if (unitListUI.gameObject.activeInHierarchy) {
            SetUnitListToggleOpenState(false);
            unitListUI.CloseWithAnimation();
        }
        else
        {
            SetUnitListToggleOpenState(true);
            unitListUI.Show();
        }
        
    }
}
