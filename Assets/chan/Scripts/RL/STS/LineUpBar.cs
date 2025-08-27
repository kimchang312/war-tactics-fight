using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public enum SortType
{
    Acquisition,  // 획득순
    Rarity,       // 희귀도순
    Branch,       // 병종순
    Energy,       // 기력순
    Name          // 이름순
}

public class LineUpBar : MonoBehaviour
{
    [Header("할당할 프리팹 & Content")]
    public GameObject unitUIPrefab;      // UnitItemPrefab
    public RectTransform contentParent;    // ScrollView → Content

    [Header("정렬 버튼들")]
    [SerializeField] private Button acquisitionSortButton;  // 획득순 정렬 버튼
    [SerializeField] private Button raritySortButton;       // 희귀도순 정렬 버튼
    [SerializeField] private Button branchSortButton;       // 병종순 정렬 버튼
    [SerializeField] private Button energySortButton;       // 기력순 정렬 버튼
    [SerializeField] private Button nameSortButton;         // 이름순 정렬 버튼

    [Header("정렬 버튼 텍스트들")]
    [SerializeField] private TextMeshProUGUI acquisitionSortText;  // 획득순 버튼 텍스트
    [SerializeField] private TextMeshProUGUI raritySortText;       // 희귀도순 버튼 텍스트
    [SerializeField] private TextMeshProUGUI branchSortText;       // 병종순 버튼 텍스트
    [SerializeField] private TextMeshProUGUI energySortText;       // 기력순 버튼 텍스트
    [SerializeField] private TextMeshProUGUI nameSortText;         // 이름순 버튼 텍스트

    [Header("WideBar 연동")]
    [SerializeField] private WideBarUI wideBarUI;           // WideBar UI 참조
    [SerializeField] private GameObject lineupScrollView;   // 하단바 스크롤뷰 (실제 유닛이 표시되는 곳)

    // 정렬 상태 관리
    private SortType currentSortType = SortType.Acquisition;
    private bool isAscending = true;  // true: 오름차순, false: 내림차순

    private void Awake()
    {
        Debug.Log("🔧 LineUpBar Awake 시작");
        
        // 정렬 버튼 이벤트 연결
        acquisitionSortButton?.onClick.AddListener(() => SetSortType(SortType.Acquisition));
        raritySortButton?.onClick.AddListener(() => SetSortType(SortType.Rarity));
        branchSortButton?.onClick.AddListener(() => SetSortType(SortType.Branch));
        energySortButton?.onClick.AddListener(() => SetSortType(SortType.Energy));
        nameSortButton?.onClick.AddListener(() => SetSortType(SortType.Name));

        // 초기 정렬 상태 설정
        UpdateSortButtonTexts();
        
        // WideBar가 없으면 기존 하단바 기능으로 동작
        if (wideBarUI == null)
        {
            Debug.Log("🟢 WideBar가 없음 - 기존 하단바 기능으로 동작");
            // 기존 하단바 기능: 하단바 스크롤을 활성화 상태로 유지하고 유닛 리스트 생성
            gameObject.SetActive(true);
            RefreshUnitList();
        }
        else
        {
            Debug.Log("🔴 WideBar가 있음 - WideBar 연동 모드로 동작");
            // WideBar 연동 모드: 초기에는 비활성화 (WideBar가 펼쳐질 때만 활성화)
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // WideBar가 없으면 기존 하단바 기능으로 동작
        if (wideBarUI == null)
        {
            RefreshUnitList();
        }
        // WideBar가 있으면 펼쳐졌을 때만 유닛 리스트 생성
        else if (wideBarUI.IsExpanded)
        {
            RefreshUnitList();
        }
    }

    // 유닛 리스트 새로고침 (WideBar에서 호출)
    public void RefreshUnitList()
    {
        Debug.Log("🔄 RefreshUnitList 호출됨");
        
        // 1) 기존에 있던 UI 전부 지우기
        if (contentParent != null)
        {
            foreach (Transform child in contentParent)
                Destroy(child.gameObject);
        }

        // 2) 유닛 리스트 생성
        MakeUnitList();
    }

    // 정렬 타입 설정 및 토글
    private void SetSortType(SortType sortType)
    {
        if (currentSortType == sortType)
        {
            // 같은 정렬 타입이면 오름차순/내림차순 토글
            isAscending = !isAscending;
        }
        else
        {
            // 다른 정렬 타입이면 해당 타입으로 변경하고 오름차순으로 설정
            currentSortType = sortType;
            isAscending = true;
        }

        UpdateSortButtonTexts();
        RefreshUnitList(); // 정렬된 유닛 리스트 다시 생성
    }

    // 정렬 버튼 텍스트 업데이트
    private void UpdateSortButtonTexts()
    {
        string arrow = isAscending ? "↑" : "↓";
        
        acquisitionSortText.text = $"획득순 {arrow}";
        raritySortText.text = $"희귀도순 {arrow}";
        branchSortText.text = $"병종순 {arrow}";
        energySortText.text = $"기력순 {arrow}";
        nameSortText.text = $"이름순 {arrow}";

        // 현재 선택된 정렬 타입 강조 표시
        ResetButtonColors();
        HighlightCurrentSortButton();
    }

    // 버튼 색상 초기화
    private void ResetButtonColors()
    {
        var buttons = new Button[] { acquisitionSortButton, raritySortButton, branchSortButton, energySortButton, nameSortButton };
        foreach (var button in buttons)
        {
            if (button != null)
            {
                var colors = button.colors;
                colors.normalColor = Color.white;
                button.colors = colors;
            }
        }
    }

    // 현재 선택된 정렬 버튼 강조 표시
    private void HighlightCurrentSortButton()
    {
        Button currentButton = currentSortType switch
        {
            SortType.Acquisition => acquisitionSortButton,
            SortType.Rarity => raritySortButton,
            SortType.Branch => branchSortButton,
            SortType.Energy => energySortButton,
            SortType.Name => nameSortButton,
            _ => acquisitionSortButton
        };

        if (currentButton != null)
        {
            var colors = currentButton.colors;
            colors.normalColor = new Color(0.8f, 0.8f, 1f, 1f); // 연한 파란색으로 강조
            currentButton.colors = colors;
        }
    }

    public void MakeUnitList()
    {
        // WideBar가 있고 펼쳐져 있지 않으면 유닛 리스트를 생성하지 않음
        if (wideBarUI != null && !wideBarUI.IsExpanded)
        {
            return;
        }

        // contentParent가 null이면 리턴
        if (contentParent == null)
        {
            Debug.LogError("❌ contentParent가 할당되지 않았습니다!");
            return;
        }

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
        
        // 데이터 꺼내오기
        var units = RogueLikeData.Instance.GetMyTeam();

        // 정렬된 유닛 리스트 생성
        var sortedUnits = SortUnits(units);

        // 하나씩 Instantiate + Setup
        for (int i = 0; i < sortedUnits.Count; i++)
        {
            var u = sortedUnits[i];
            var go = Instantiate(unitUIPrefab, contentParent);
            var ui = go.GetComponent<UnitUIPrefab>();
            ui.SetupIMG(u, Context.Lineup, u.UniqueId);
            ui.SetupEnergy(u);
        }
    }

    // 유닛 리스트 정렬
    private List<RogueUnitDataBase> SortUnits(List<RogueUnitDataBase> units)
    {
        if (units == null || units.Count == 0)
            return new List<RogueUnitDataBase>();

        var sortedUnits = currentSortType switch
        {
            SortType.Acquisition => SortByAcquisition(units),
            SortType.Rarity => SortByRarity(units),
            SortType.Branch => SortByBranch(units),
            SortType.Energy => SortByEnergy(units),
            SortType.Name => SortByName(units),
            _ => SortByAcquisition(units)
        };

        return sortedUnits;
    }

    // 획득순 정렬 (UniqueId 기준)
    private List<RogueUnitDataBase> SortByAcquisition(List<RogueUnitDataBase> units)
    {
        return isAscending 
            ? units.OrderBy(u => u.UniqueId).ToList()
            : units.OrderByDescending(u => u.UniqueId).ToList();
    }

    // 희귀도순 정렬
    private List<RogueUnitDataBase> SortByRarity(List<RogueUnitDataBase> units)
    {
        return isAscending 
            ? units.OrderBy(u => u.rarity).ThenBy(u => u.UniqueId).ToList()
            : units.OrderByDescending(u => u.rarity).ThenBy(u => u.UniqueId).ToList();
    }

    // 병종순 정렬
    private List<RogueUnitDataBase> SortByBranch(List<RogueUnitDataBase> units)
    {
        return isAscending 
            ? units.OrderBy(u => u.branchIdx).ThenBy(u => u.unitName).ThenBy(u => u.UniqueId).ToList()
            : units.OrderByDescending(u => u.branchIdx).ThenBy(u => u.unitName).ThenBy(u => u.UniqueId).ToList();
    }

    // 기력순 정렬 (현재 기력 기준)
    private List<RogueUnitDataBase> SortByEnergy(List<RogueUnitDataBase> units)
    {
        return isAscending 
            ? units.OrderBy(u => u.energy).ThenBy(u => u.UniqueId).ToList()
            : units.OrderByDescending(u => u.energy).ThenBy(u => u.UniqueId).ToList();
    }

    // 이름순 정렬
    private List<RogueUnitDataBase> SortByName(List<RogueUnitDataBase> units)
    {
        return isAscending 
            ? units.OrderBy(u => u.unitName).ThenBy(u => u.UniqueId).ToList()
            : units.OrderByDescending(u => u.unitName).ThenBy(u => u.UniqueId).ToList();
    }

    public void UpdateLineupNumbers(List<int> placedUniqueIds)
    {
        // WideBar가 있고 펼쳐져 있지 않으면 업데이트하지 않음
        if (wideBarUI != null && !wideBarUI.IsExpanded)
        {
            return;
        }

        foreach (var ui in contentParent.GetComponentsInChildren<UnitUIPrefab>())
        {
            if (ui.PrefabType != Context.Lineup) continue;
            int idx = placedUniqueIds.IndexOf(ui.uniqueId);
            var cg = ui.GetComponent<CanvasGroup>();

            if (idx >= 0)
            { // → 배치된 MyUnitPrefab만 잠그고, 번호 표시
                cg.alpha = 0.5f;
                ui.SetOrderNumber(idx + 1);
            }
            else 
            { 
                // → 나머지 MyUnitPrefab 해제
                cg.alpha = 1f;
                ui.SetOrderNumber(0);
            }
        }
    }

    public UnitUIPrefab GetUnitUIByUniqueId(int uniqueId)
    {
        return contentParent
            .GetComponentsInChildren<UnitUIPrefab>()
            .FirstOrDefault(u =>
                u.PrefabType == Context.Lineup &&
                u.unitData.UniqueId == uniqueId
            );
    }

    // 현재 정렬 상태 반환 (디버그용)
    public string GetCurrentSortInfo()
    {
        string sortTypeName = currentSortType switch
        {
            SortType.Acquisition => "획득순",
            SortType.Rarity => "희귀도순",
            SortType.Branch => "병종순",
            SortType.Energy => "기력순",
            SortType.Name => "이름순",
            _ => "알 수 없음"
        };

        string order = isAscending ? "오름차순" : "내림차순";
        return $"{sortTypeName} ({order})";
    }

    // WideBar 상태에 따른 활성화/비활성화
    public void SetWideBarState(bool isExpanded)
    {
        if (isExpanded)
        {
            // WideBar가 펼쳐졌을 때 LineUpBar 활성화 및 유닛 리스트 새로고침
            gameObject.SetActive(true);
            RefreshUnitList();
        }
        else
        {
            // WideBar가 접혔을 때 LineUpBar 비활성화
            gameObject.SetActive(false);
        }
    }
}
