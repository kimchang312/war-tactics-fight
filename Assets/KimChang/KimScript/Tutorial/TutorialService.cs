using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TutorialId
{
    INF_01_ROUTE,
    INF_02_DEPLOY_AUTO,
    INF_03_MATCHUP,
    INF_04_BATTLE_INFO,
    RES_01_GOLD,
    RES_02_MORALE,
    RES_03_ENERGY,
    RES_04_REROLL,
    STG_01_NORMAL,
    STG_02_ELITE,
    STG_03_BOSS,
    STG_04_EVENT,
    STG_05_SHOP,
    STG_06_TREASURE,
    STG_07_REST,
    RWD_01_BATTLE_REWARD,
    RWD_02_WAR_LEGACY,
    SYS_01_TACTICAL_UPGRADE
}

public enum TutorialRequestType
{
    ScreenDependent = 0,
    ActionDependent = 1,
    StepDependent = 2,
    DeferredResource = 3
}

public enum TutorialMode
{
    Auto,
    Manual
}

public enum TutorialPopupCloseReason
{
    Confirmed,
    Dismissed,
    ManualClosed
}

public struct TutorialPopupResult
{
    public TutorialPopupCloseReason CloseReason;
    public bool SkipAllAuto;
}

[Serializable]
public class TutorialPageData
{
    public int GlobalPage;
    public int LocalPage;
    public string Description;
    public string ImageResourcePath;
    [NonSerialized] public Sprite Image;

    public TutorialPageData(int globalPage, int localPage, string description, string imageResourcePath)
    {
        GlobalPage = globalPage;
        LocalPage = localPage;
        Description = description;
        ImageResourcePath = imageResourcePath;
    }

    public void ResolveImage()
    {
        if (Image != null || string.IsNullOrWhiteSpace(ImageResourcePath))
            return;

        Image = Resources.Load<Sprite>(ImageResourcePath);
        if (Image == null)
            Debug.LogWarning($"[TutorialCatalog] 튜토리얼 이미지를 찾을 수 없습니다. path={ImageResourcePath}");
    }
}

[Serializable]
public class TutorialGuideData
{
    public TutorialId Id;
    public string Title;
    public int ContentVersion = 1;
    public int Priority;
    public TutorialRequestType RequestType;
    public int CatalogOrder;
    public List<TutorialPageData> Pages = new List<TutorialPageData>();

    public TutorialGuideData(TutorialId id, string title, int priority, TutorialRequestType requestType, int catalogOrder)
    {
        Id = id;
        Title = title;
        Priority = priority;
        RequestType = requestType;
        CatalogOrder = catalogOrder;
    }

    public void ResolveImages()
    {
        foreach (TutorialPageData page in Pages)
            page.ResolveImage();
    }
}

public struct TutorialManualPageEntry
{
    public TutorialGuideData Guide;
    public TutorialPageData Page;

    public TutorialManualPageEntry(TutorialGuideData guide, TutorialPageData page)
    {
        Guide = guide;
        Page = page;
    }
}

public sealed class TutorialCatalog
{
    private readonly List<TutorialGuideData> guides = new List<TutorialGuideData>();
    private Dictionary<TutorialId, TutorialGuideData> guideById;

    public IReadOnlyList<TutorialGuideData> Guides => guides;

    public IEnumerable<TutorialId> GuideIds => guides.Select(g => g.Id);

    public bool TryGetGuide(TutorialId id, out TutorialGuideData guide)
    {
        EnsureLookup();
        return guideById.TryGetValue(id, out guide);
    }

    public List<TutorialManualPageEntry> GetManualPages()
    {
        return guides
            .OrderBy(g => g.CatalogOrder)
            .SelectMany(g => g.Pages.OrderBy(p => p.LocalPage).Select(p => new TutorialManualPageEntry(g, p)))
            .ToList();
    }

    public void ResolveImages()
    {
        foreach (TutorialGuideData guide in guides)
            guide.ResolveImages();
    }

    private void AddGuide(TutorialGuideData guide)
    {
        guides.Add(guide);
        guideById = null;
    }

    private void EnsureLookup()
    {
        if (guideById != null)
            return;

        guideById = new Dictionary<TutorialId, TutorialGuideData>();
        foreach (TutorialGuideData guide in guides)
        {
            if (guideById.ContainsKey(guide.Id))
            {
                Debug.LogError($"[TutorialCatalog] 중복 TutorialId가 있습니다. id={guide.Id}");
                continue;
            }

            guideById.Add(guide.Id, guide);
        }
    }

    public static TutorialCatalog CreateRuntimeDefault()
    {
        var catalog = new TutorialCatalog();
        int order = 1;
        int globalPage = 1;

        void Add(TutorialId id, string title, int priority, TutorialRequestType requestType, params string[] descriptions)
        {
            var guide = new TutorialGuideData(id, title, priority, requestType, order++);
            for (int i = 0; i < descriptions.Length; i++)
            {
                string imagePath = $"Tutorial/{globalPage:00}";
                guide.Pages.Add(new TutorialPageData(globalPage, i + 1, descriptions[i], imagePath));
                globalPage++;
            }

            catalog.AddGuide(guide);
        }

        Add(TutorialId.INF_01_ROUTE, "원정 가이드: 경로", 20, TutorialRequestType.ScreenDependent,
            "연결된 스테이지 중 하나를 선택해 왼쪽에서 오른쪽으로 이동합니다. 현재 위치와 실선으로 이어진 노드만 선택할 수 있습니다.",
            "각 챕터의 경로는 마지막 보스로 이어집니다. 보스를 처치하면 다음 챕터로 진행하며, 마지막 챕터 보스를 처치하면 원정을 완료합니다.");

        Add(TutorialId.INF_02_DEPLOY_AUTO, "전투 가이드: 배치와 자동 전투", 10, TutorialRequestType.ScreenDependent,
            "전투 전에 유닛을 전열에 나설 순서대로 배치합니다. 먼저 배치한 유닛부터 차례로 적과 맞섭니다.",
            "전투를 시작하면 이동·공격·기술 발동은 모두 자동으로 진행됩니다. 시작 전 배치와 상성 판단이 전투의 핵심입니다.");

        Add(TutorialId.INF_03_MATCHUP, "전투 가이드: 병종 상성", 15, TutorialRequestType.ActionDependent,
            "병종은 유닛의 역할과 전투 방식을 나타냅니다. 적의 병종 아이콘을 확인해 앞에 세울 유닛을 고르세요.",
            "상성은 절대적이지 않지만 유리한 상대를 만나면 효율이 크게 높아집니다. 대표적으로 창병은 기병에 강합니다.");

        Add(TutorialId.INF_04_BATTLE_INFO, "전투 가이드: 전투 정보", 10, TutorialRequestType.ScreenDependent,
            "전투 전에는 적 부대와 전장 효과를 확인하세요. 전장 효과는 아군과 적 모두에게 적용됩니다.",
            "엘리트와 보스는 고유한 지휘관 효과를 가집니다. 전투 시작 전에 효과를 읽고 배치 순서를 조정하세요.");

        Add(TutorialId.RES_01_GOLD, "자원 가이드: 금화", 50, TutorialRequestType.DeferredResource,
            "금화는 상단 HUD에서 확인하는 원정의 기본 재화입니다. 획득하거나 사용하면 보유량이 즉시 갱신됩니다.",
            "금화는 전투 승리, 이벤트, 보물 등에서 획득합니다. 보상 화면과 상자에서 얻은 금화는 보유량에 더해집니다.",
            "금화는 상점 구매와 전술 개량에 사용합니다. 당장 필요한 회복과 장기 강화를 비교해 사용하세요.");

        Add(TutorialId.RES_02_MORALE, "자원 가이드: 사기", 40, TutorialRequestType.DeferredResource,
            "사기는 부대 전체에 적용되는 0~100의 원정 자원입니다. 원정은 50에서 시작하며 현재 수치는 상단 HUD에서 확인합니다.",
            "70 이상이면 체력·공격력이 10% 증가하고, 90 이상이면 20% 증가합니다. 30 이하에서는 10% 감소합니다.",
            "10 이하에서는 전투 시작 시 희귀도 1~3 무작위 유닛 1명이 부대를 영구적으로 떠납니다. 0 이하가 되면 원정이 즉시 종료됩니다.");

        Add(TutorialId.RES_03_ENERGY, "자원 가이드: 기력", 45, TutorialRequestType.DeferredResource,
            "기력은 각 유닛이 전투에 참가할 수 있는 횟수입니다. 현재 기력은 하단 유닛 카드의 번개 아이콘에서 확인합니다.",
            "전투에 참가한 유닛은 종료 후 기력이 1 감소합니다. 0 미만이 되면 부대에서 제거되므로 회복과 배치를 관리하세요.");

        Add(TutorialId.RES_04_REROLL, "자원 가이드: 리롤", 35, TutorialRequestType.ScreenDependent,
            "리롤은 현재 제시된 선택지를 새 목록으로 바꾸는 소모 자원입니다. 사용하면 보유 리롤이 1 감소합니다.",
            "보상·전술 개량 등 리롤 버튼이 있는 화면에서 사용할 수 있습니다. 전술 개량에서는 선택지 3개가 한꺼번에 갱신됩니다.");

        Add(TutorialId.STG_01_NORMAL, "스테이지 가이드: 일반 전투", 20, TutorialRequestType.ScreenDependent,
            "가장 자주 만나는 기본 전투 스테이지입니다. 승리하면 금화와 유닛 보상을 얻습니다.");

        Add(TutorialId.STG_02_ELITE, "스테이지 가이드: 엘리트", 20, TutorialRequestType.ScreenDependent,
            "고유 효과를 지닌 지휘관과 강한 적이 등장합니다. 위험이 큰 만큼 금화·유닛과 전쟁 유산 등 더 높은 가치의 보상을 얻습니다.");

        Add(TutorialId.STG_03_BOSS, "스테이지 가이드: 보스", 15, TutorialRequestType.ScreenDependent,
            "챕터의 마지막 전투이며 강력한 보스 지휘관이 등장합니다. 승리하면 다음 챕터로 진행하고, 마지막 챕터에서는 원정을 완료합니다.");

        Add(TutorialId.STG_04_EVENT, "스테이지 가이드: 이벤트", 10, TutorialRequestType.ScreenDependent,
            "이벤트는 선택에 따라 결과가 달라지는 스테이지입니다. 각 선택지의 조건과 예상 결과를 확인한 뒤 결정하세요.",
            "금화·사기·기력·유닛·전쟁 유산을 얻거나 잃을 수 있고 전투가 발생할 수도 있습니다. 영구 손실은 선택 전에 별도 경고로 표시됩니다.");

        Add(TutorialId.STG_05_SHOP, "스테이지 가이드: 상점", 10, TutorialRequestType.ScreenDependent,
            "상점에서는 금화로 기력·사기 회복, 리롤, 전쟁 유산, 유닛 패키지를 구입할 수 있습니다.",
            "상품의 종류와 가격을 확인한 뒤 구매합니다. 회복·보강과 금화 저축 사이에서 현재 원정에 필요한 항목을 선택하세요.");

        Add(TutorialId.STG_06_TREASURE, "스테이지 가이드: 보물", 20, TutorialRequestType.ScreenDependent,
            "전투 없이 보상을 얻는 안전한 스테이지입니다. 보물 상자를 열어 금화와 전쟁 유산을 획득합니다.");

        Add(TutorialId.STG_07_REST, "스테이지 가이드: 휴식", 10, TutorialRequestType.ScreenDependent,
            "휴식 스테이지에서는 휴식·연회·훈련 중 하나만 선택합니다. 효과가 적용되면 자동으로 맵 화면으로 돌아갑니다.",
            "휴식은 부대 전체 유닛의 기력을 2 회복합니다. 연회는 부대 사기를 20 증가시킵니다.",
            "훈련을 선택하면 다음 전술 개량 1회의 금화 비용이 0이 됩니다. 전술 개량 구매가 성공할 때 효과가 소비됩니다.");

        Add(TutorialId.RWD_01_BATTLE_REWARD, "보상 가이드: 전투 보상", 10, TutorialRequestType.StepDependent,
            "전투에서 승리하면 종류에 따라 금화, 유닛, 전쟁 유산을 보상으로 얻습니다. 금화는 자동 지급되고 선택 보상은 카드로 표시됩니다.",
            "각 선택 영역에서 필요한 카드 하나를 고른 뒤 확정합니다. 리롤이 가능하면 목록을 바꿀 수 있으므로 효과를 확인하고 결정하세요.");

        Add(TutorialId.RWD_02_WAR_LEGACY, "보상 가이드: 전쟁 유산", 10, TutorialRequestType.StepDependent,
            "전쟁 유산은 부대와 원정에 특별한 효과를 더하는 보상입니다. 카드의 효과를 확인하고 현재 전략에 맞는 유산을 선택하세요.",
            "획득한 전쟁 유산은 보유 유산 목록에서 다시 확인할 수 있습니다. 효과는 전투·자원·보상·상점 등 원정 전반에 적용될 수 있습니다.");

        Add(TutorialId.SYS_01_TACTICAL_UPGRADE, "성장 가이드: 전술 개량", 10, TutorialRequestType.ScreenDependent,
            "맵 상단 우측의 업그레이드 버튼을 누르면 전술 개량 팝업이 열립니다. 현재 병종별 강화 현황을 먼저 확인하세요.",
            "병종과 공격력·방어력 조합으로 무작위 선택지 3개가 제시됩니다. 금화를 사용해 하나를 구매하면 해당 병종의 모든 유닛에 적용됩니다.",
            "리롤 1개를 사용하면 선택지 3개가 모두 새로 바뀝니다. 휴식의 훈련 효과가 있으면 다음 구매의 금화 비용이 0이 됩니다.");

        return catalog;
    }
}

public sealed class TutorialRequest
{
    public TutorialId TutorialId { get; private set; }
    public string TriggerInstanceId { get; private set; }
    public string ContextId { get; private set; }
    public string ScreenVisitId { get; private set; }
    public string StepId { get; private set; }
    public int Priority { get; set; }
    public TutorialRequestType RequestType { get; set; }
    public long EnqueueSequence { get; set; }
    public int CatalogOrder { get; set; }
    public TutorialGuideData Guide { get; set; }

    public string ScreenStepKey => $"{ScreenVisitId}::{StepId}";
    public string PendingKey => $"{TutorialId}::{TriggerInstanceId}";

    private TutorialRequest() { }

    public static TutorialRequest Create(TutorialId tutorialId, string triggerInstanceId, string contextId, string screenVisitId, string stepId)
    {
        return new TutorialRequest
        {
            TutorialId = tutorialId,
            TriggerInstanceId = triggerInstanceId ?? string.Empty,
            ContextId = contextId ?? string.Empty,
            ScreenVisitId = screenVisitId ?? string.Empty,
            StepId = string.IsNullOrWhiteSpace(stepId) ? TutorialHook.StepReady : stepId
        };
    }
}

public class TutorialService : MonoBehaviour
{
    public static TutorialService Instance { get; private set; }

    [SerializeField] private TutorialPopupUI popupUI;

    private TutorialCatalog catalog;
    private readonly List<TutorialRequest> pendingRequests = new List<TutorialRequest>();
    private readonly HashSet<string> pendingRequestKeys = new HashSet<string>();
    private readonly HashSet<string> dismissedTriggerIds = new HashSet<string>();
    private readonly HashSet<string> openedScreenStepKeys = new HashSet<string>();
    private long nextEnqueueSequence = 1;
    private TutorialRequest activeRequest;

    public bool IsAutoPopupEnabled => !TutorialProgressStore.IsAutoPopupDisabled();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        InitializeIfNeeded();
    }

    public void ConfigurePopup(TutorialPopupUI popup)
    {
        if (Instance == null)
            Instance = this;

        popupUI = popup;
        InitializeIfNeeded();
    }

    public void InitializeIfNeeded()
    {
        if (popupUI == null)
            popupUI = GetComponent<TutorialPopupUI>() ?? GetComponentInChildren<TutorialPopupUI>(true);

        if (popupUI != null)
            popupUI.InitializeIfNeeded();

        if (catalog == null)
        {
            catalog = TutorialCatalog.CreateRuntimeDefault();
            catalog.ResolveImages();
        }
    }

    public bool EnqueueAuto(TutorialId tutorialId, string triggerInstanceId, string contextId, string screenVisitId, string stepId)
    {
        InitializeIfNeeded();

        if (TutorialProgressStore.IsAutoPopupDisabled())
            return false;

        if (string.IsNullOrWhiteSpace(triggerInstanceId) || string.IsNullOrWhiteSpace(screenVisitId))
            return false;

        if (!catalog.TryGetGuide(tutorialId, out TutorialGuideData guide))
        {
            Debug.LogWarning($"[TutorialService] 등록되지 않은 TutorialId입니다. id={tutorialId}");
            return false;
        }

        if (guide.Pages == null || guide.Pages.Count == 0)
        {
            Debug.LogWarning($"[TutorialService] 페이지가 없는 가이드는 열 수 없습니다. id={tutorialId}");
            return false;
        }

        if (TutorialProgressStore.IsCompleted(tutorialId, guide.ContentVersion))
            return false;

        if (dismissedTriggerIds.Contains(triggerInstanceId))
            return false;

        if (activeRequest != null &&
            (activeRequest.TutorialId == tutorialId || activeRequest.TriggerInstanceId == triggerInstanceId))
            return false;

        TutorialRequest request = TutorialRequest.Create(tutorialId, triggerInstanceId, contextId, screenVisitId, stepId);
        request.Guide = guide;
        request.Priority = guide.Priority;
        request.RequestType = guide.RequestType;
        request.CatalogOrder = guide.CatalogOrder;
        request.EnqueueSequence = nextEnqueueSequence++;

        if (!pendingRequestKeys.Add(request.PendingKey))
            return false;

        pendingRequests.Add(request);
        return true;
    }

    public bool NotifySafePoint(string screenVisitId, string stepId)
    {
        InitializeIfNeeded();

        if (TutorialProgressStore.IsAutoPopupDisabled())
            return false;

        if (activeRequest != null || popupUI == null || popupUI.IsOpen)
            return false;

        string normalizedStep = string.IsNullOrWhiteSpace(stepId) ? TutorialHook.StepReady : stepId;
        RemoveInvalidPendingRequests();

        TutorialRequest request = pendingRequests
            .Where(r => r.ScreenVisitId == screenVisitId && r.StepId == normalizedStep)
            .Where(r => !openedScreenStepKeys.Contains(r.ScreenStepKey))
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.RequestType)
            .ThenBy(r => r.EnqueueSequence)
            .ThenBy(r => r.CatalogOrder)
            .FirstOrDefault();

        if (request == null)
            return false;

        return OpenAutoRequest(request);
    }

    public void CancelRequestsByContext(string contextId)
    {
        if (string.IsNullOrWhiteSpace(contextId))
            return;

        for (int i = pendingRequests.Count - 1; i >= 0; i--)
        {
            TutorialRequest request = pendingRequests[i];
            if (request.ContextId != contextId)
                continue;

            pendingRequestKeys.Remove(request.PendingKey);
            pendingRequests.RemoveAt(i);
        }
    }

    public void CancelAllAutoRequests()
    {
        pendingRequests.Clear();
        pendingRequestKeys.Clear();
        activeRequest = null;
    }

    public bool OpenManualLibrary()
    {
        InitializeIfNeeded();

        if (popupUI == null || popupUI.IsOpen || activeRequest != null)
            return false;

        List<TutorialManualPageEntry> manualPages = catalog.GetManualPages();
        if (manualPages.Count == 0)
            return false;

        popupUI.OpenManual(manualPages, _ => { });
        return true;
    }

    public void SetAutoPopupEnabled(bool enabled)
    {
        TutorialProgressStore.SetAutoPopupDisabled(!enabled);
        if (!enabled)
            CancelAllAutoRequests();
    }

    public void ResetTutorialProgressForDebug()
    {
        InitializeIfNeeded();
        TutorialProgressStore.ResetTutorialProgressForDebug(catalog.GuideIds);
        pendingRequests.Clear();
        pendingRequestKeys.Clear();
        dismissedTriggerIds.Clear();
        openedScreenStepKeys.Clear();
        activeRequest = null;
    }

    private bool OpenAutoRequest(TutorialRequest request)
    {
        if (popupUI == null || request.Guide == null)
            return false;

        pendingRequests.Remove(request);
        pendingRequestKeys.Remove(request.PendingKey);
        openedScreenStepKeys.Add(request.ScreenStepKey);
        activeRequest = request;

        popupUI.OpenAuto(request.Guide, result => HandleAutoPopupClosed(request, result));
        return true;
    }

    private void HandleAutoPopupClosed(TutorialRequest request, TutorialPopupResult result)
    {
        if (result.CloseReason == TutorialPopupCloseReason.Confirmed)
        {
            TutorialProgressStore.SetCompletedVersion(request.TutorialId, request.Guide.ContentVersion);
            if (result.SkipAllAuto)
            {
                TutorialProgressStore.SetAutoPopupDisabled(true);
                CancelAllAutoRequests();
            }
        }
        else
        {
            dismissedTriggerIds.Add(request.TriggerInstanceId);
        }

        if (activeRequest == request)
            activeRequest = null;
    }

    private void RemoveInvalidPendingRequests()
    {
        for (int i = pendingRequests.Count - 1; i >= 0; i--)
        {
            TutorialRequest request = pendingRequests[i];
            bool remove =
                request.Guide == null ||
                TutorialProgressStore.IsCompleted(request.TutorialId, request.Guide.ContentVersion) ||
                dismissedTriggerIds.Contains(request.TriggerInstanceId);

            if (!remove)
                continue;

            pendingRequestKeys.Remove(request.PendingKey);
            pendingRequests.RemoveAt(i);
        }
    }
}
