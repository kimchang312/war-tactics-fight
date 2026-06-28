using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlacePanel : MonoBehaviour
{
    [SerializeField] private Button startBattleButton;   // 전투 시작 버튼
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject enemyInfoPanel;

    [SerializeField] public TextMeshProUGUI maxUnitCount;
    [SerializeField] public TextMeshProUGUI currentUnitCount;

    [Header("플레이어 유닛 배치")]
    public GameObject battleUnitPrefab;
    [SerializeField] private GameObject emptyBattleSlotPrefab;
    public RectTransform PrefabContainer;
    
    [Header("적 유닛 배치")]
    public GameObject enemyUnitPrefab;
    public RectTransform EnemyPrefabsContainer;
    [SerializeField] public TextMeshProUGUI enemyUnitCountText;  // 적 유닛 수 표시용 텍스트
    
    [Header("지휘관 정보")]
    [SerializeField] private GameObject commanderInfoPanel;     // 지휘관 정보 패널
    [SerializeField] private TextMeshProUGUI commanderNameText; // 지휘관 이름
    [SerializeField] private TextMeshProUGUI commanderSkillText; // 지휘관 스킬 효과
    [SerializeField] private TextMeshProUGUI battlefieldEffectText; // 전장 효과

    //프리팹 식별용 unitOrderingNum 리스트?
    public List<int> PlacedUniqueIds { get; } = new List<int>();

    private List<RogueUnitDataBase> placedUnits = new List<RogueUnitDataBase>();

    private PlacePanelStripScroll _playerStripScroll;
    private PlacePanelStripScroll _enemyStripScroll;

    private void Awake()
    {
        _playerStripScroll = PrefabContainer != null
            ? PrefabContainer.GetComponentInParent<PlacePanelStripScroll>()
            : null;
        _enemyStripScroll = EnemyPrefabsContainer != null
            ? EnemyPrefabsContainer.GetComponentInParent<PlacePanelStripScroll>()
            : null;

        startBattleButton.onClick.AddListener(OnStartBattleClicked);
        // 뒤로가기 리스너
        backButton.onClick.AddListener(OnBackClicked);

        // 현재 배치 수 초기화
        UpdateCountTexts();
        // 적 유닛 수 초기화
        UpdateEnemyUnitCount(0);
        // 패널 처음 열릴 때는 항상 초기화
        ClearPlacePanel();

        // 스크롤: UnitPrefabsP·EnemyPrefabsP에 PlacePanelStripScroll + ScrollRect(인스펙터에서 개수·뷰포트 조건 설정).

        // 전장효과 툴팁 컴포넌트 확보(텍스트 세팅은 ShowBattlefieldEffect에서)
        if (battlefieldEffectText != null && battlefieldEffectText.GetComponent<BattlefieldEffectTooltip>() == null)
        {
            battlefieldEffectText.gameObject.AddComponent<BattlefieldEffectTooltip>();
        }
    }
    private void OnBackClicked()
    {
        // 결합 모드에서도 뒤로가기를 누르면 적 정보만 남기고 배치 패널을 닫을 수 있게 처리
        gameObject.SetActive(false);
        enemyInfoPanel.SetActive(true);
    }
    public int AddUnitToBattle(RogueUnitDataBase unit)
    {
        // 배치 최대치 초과 방지
        if (placedUnits.Count >= RogueLikeData.Instance.GetMaxUnits())
        return 0;


        // 리스트에 추가
        placedUnits.Add(unit);
        PlacedUniqueIds.Add(unit.UniqueId);
        // UI 생성 & 순서 반환
        int order = placedUnits.Count;

        RebuildPlayerSlots();

        // 배치 유닛 수 갱신
        UpdateCountTexts();
        return order;
    }
    private void CreateBattleUnitUI(RogueUnitDataBase unit, int uniqueId, int order)
    {
        // 인스턴스화
        var go = Instantiate(battleUnitPrefab, PrefabContainer);
        var ui = go.GetComponent<UnitUIPrefab>();

        // 1) 유닛 이미지, 기력 세팅
        ui.SetupIMG(unit,Context.Placed,uniqueId);
        ui.SetupEnergy(unit);

        // 2) 번호 세팅 (UnitUIPrefab 에 SetNumber 메서드 필요)
        ui.SetNumber(order);
        RefreshPlayerUnitStripLayout();
    }
    public void ClearPlacePanel()
    {
        placedUnits.Clear();
        PlacedUniqueIds.Clear();
        RebuildPlayerSlots();
        // 적 유닛 프리팹도 정리
        ClearEnemyPrefabs();
        // 지휘관 정보 초기화
        HideCommanderInfo();
        // 초기화 후 현재 유닛 수 갱신
        UpdateCountTexts();
        RefreshPlayerUnitStripLayout();
    }
    
    public void ClearEnemyPrefabs()
    {
        if (EnemyPrefabsContainer != null)
        {
            foreach (Transform child in EnemyPrefabsContainer)
                Destroy(child.gameObject);
        }
        
        // 적 유닛 수 텍스트 초기화
        UpdateEnemyUnitCount(0);
        RefreshEnemyUnitStripLayout();
    }
    public void RemoveUnitFromBattle(RogueUnitDataBase unit)
    {
        var lineupUI = GameManager.Instance.LineUpBarComponent.GetUnitUIByUniqueId(unit.UniqueId);
        if (lineupUI != null)
            lineupUI.RestoreFromPlaced();

        // 데이터 리스트에서 UniqueId로 제거
        placedUnits.RemoveAll(u => u.UniqueId == unit.UniqueId);
        PlacedUniqueIds.Remove(unit.UniqueId);

        RebuildPlayerSlots();
        UpdateCountTexts();
        //배치 유닛 제거 후 MyPrefabs숫자 갱신
        GameManager.Instance.LineUpBarComponent.UpdateLineupNumbers(PlacedUniqueIds);
        RefreshPlayerUnitStripLayout();
    }
    
    private void OnStartBattleClicked()
    {
        RogueLikeData.Instance.SetAllMyUnits(placedUnits);
        RogueLikeData.Instance.SetProgressState(SaveProgressState.BattlePlacement);
        RogueLikeData.Instance.SaveNow();
        GameManager.Instance.HideAllPanels();
        ClearPlacePanel();
        SceneManager.LoadScene("AutoBattleScene");
    }
    
     // 현재UnitCountText를 placedUnits.Count로 갱신
     private void UpdateCountTexts()
     {
        int count = placedUnits.Count;
        currentUnitCount.text = count.ToString();

        // 👉 유닛이 하나 이상 있어야 전투 시작 가능
        startBattleButton.interactable = count > 0;

    }
    public void UpdateMaxUnitText()
    {
        // 최대 배치 가능 수 표시
        int maxUnits = RogueLikeData.Instance.GetMaxUnits();
        maxUnitCount.text = $"/ {maxUnits.ToString()}";
        RebuildPlayerSlots();
    }
    
    public void UpdateEnemyUnitCount(int count)
    {
        if (enemyUnitCountText != null)
        {
            enemyUnitCountText.text = count.ToString();
        }
        else
        {
            Debug.LogWarning("Enemy unit count text is not assigned!");
        }
    }
    
    public void CreateEnemyPrefabs(List<RogueUnitDataBase> enemies)
    {
        if (enemyUnitPrefab == null || EnemyPrefabsContainer == null)
        {
            Debug.LogWarning("Enemy prefab or container not assigned!");
            return;
        }
        
        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("Enemy list is null or empty!");
            return;
        }
        
        ClearEnemyPrefabs();
        
        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy == null)
            {
                Debug.LogWarning($"Enemy at index {i} is null, skipping...");
                continue;
            }
            
            var go = Instantiate(enemyUnitPrefab, EnemyPrefabsContainer);
            var ui = go.GetComponent<UnitUIPrefab>();
            
            if (ui == null)
            {
                Debug.LogError("UnitUIPrefab component not found on enemy prefab!");
                Destroy(go);
                continue;
            }
            
            // 적 유닛 설정 (Context.Enemy로 설정)
            ui.SetupIMG(enemy, Context.Enemy, enemy.UniqueId);
            ui.SetupEnergy(enemy);
            ui.SetNumber(i + 1); // 적 유닛 번호 설정
        }
        
        // 적 유닛 수 텍스트 업데이트
        UpdateEnemyUnitCount(enemies.Count);
        RefreshEnemyUnitStripLayout();
    }

    private void RefreshPlayerUnitStripLayout()
    {
        if (_playerStripScroll != null)
            _playerStripScroll.RefreshAfterContentChange();
        else
            RebuildStripLayoutOnly(PrefabContainer);
    }

    private void RefreshEnemyUnitStripLayout()
    {
        if (_enemyStripScroll != null)
            _enemyStripScroll.RefreshAfterContentChange();
        else
            RebuildStripLayoutOnly(EnemyPrefabsContainer);
    }

    private static void RebuildStripLayoutOnly(RectTransform content)
    {
        if (content == null) return;
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();
    }

    private void RebuildPlayerSlots()
    {
        if (PrefabContainer == null)
            return;

        foreach (Transform child in PrefabContainer)
            Destroy(child.gameObject);

        int maxUnits = RogueLikeData.Instance.GetMaxUnits();
        for (int i = 0; i < maxUnits; i++)
        {
            if (i < placedUnits.Count)
            {
                var unit = placedUnits[i];
                CreateBattleUnitUI(unit, unit.UniqueId, i + 1);
            }
            else
            {
                CreateEmptyBattleSlotUI();
            }
        }

        RefreshPlayerUnitStripLayout();
    }

    private void CreateEmptyBattleSlotUI()
    {
        if (emptyBattleSlotPrefab == null)
            return;

        Instantiate(emptyBattleSlotPrefab, PrefabContainer);
    }

    // 지휘관 정보를 표시하는 메서드
    public void ShowCommanderInfo(string commanderName, StageType stageType = StageType.Combat, int? eliteCommanderNumericId = null)
    {
        // 지휘관이 없어도 패널은 항상 표시
        if (commanderInfoPanel != null)
            commanderInfoPanel.SetActive(true);

        if (string.IsNullOrEmpty(commanderName))
        {
            // 지휘관이 없을 때의 기본 텍스트
            if (commanderNameText != null)
                commanderNameText.text = "지휘관: 없음";
            
            if (commanderSkillText != null)
                commanderSkillText.text = "스킬: 없음";
        }
        else
        {
            // 지휘관이 있을 때의 정상 처리
            if (commanderNameText != null)
                commanderNameText.text = $"지휘관: {commanderName}";

            if (commanderSkillText != null)
                commanderSkillText.text = CommanderSkillData.GetSkillText(commanderName, stageType, eliteCommanderNumericId);
        }

        // 전장 효과는 별도로 설정해야 함 (ShowBattlefieldEffect 메서드 사용)
        if (battlefieldEffectText != null)
            battlefieldEffectText.text = "전장효과: -";
    }

    // 전장효과를 표시하는 메서드
    public void ShowBattlefieldEffect(BattlefieldEffect effect)
    {
        if (battlefieldEffectText != null)
        {
            string effectName = MapGenerator.GetBattlefieldEffectKoreanName(effect);
            battlefieldEffectText.text = $"전장효과: {effectName}";

            // 툴팁 텍스트(간단 설명): effectName + id
            var tip = battlefieldEffectText.GetComponent<BattlefieldEffectTooltip>();
            if (tip != null)
            {
                // 상세 설명 소스가 따로 없어서 최소 정보로 제공 (필요하면 MapGenerator에 설명 테이블 추가 가능)
                tip.SetText($"전장효과: {effectName}\n(effect: {effect})");
            }
        }
    }

    // 지휘관 정보를 숨기는 메서드
    public void HideCommanderInfo()
    {
        if (commanderInfoPanel != null)
            commanderInfoPanel.SetActive(false);
    }
}
