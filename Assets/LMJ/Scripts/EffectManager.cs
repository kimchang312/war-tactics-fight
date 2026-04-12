using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 이펙트 재생 요청 데이터
/// </summary>
public struct EffectRequest
{
    public EffectCD effectCD;
    public RectTransform targetTransform;
    public RectTransform casterTransform;
    public bool isTargetMyTeam;  // 피격자가 아군인지 (자동 flipX용)
    public bool isCasterMyTeam;  // 시전자가 아군인지
    public System.Action onComplete;

    public EffectRequest(
        EffectCD cd,
        RectTransform target = null,
        RectTransform caster = null,
        bool targetIsMyTeam = false,
        bool casterIsMyTeam = true,
        System.Action callback = null)
    {
        effectCD = cd;
        targetTransform = target;
        casterTransform = caster;
        isTargetMyTeam = targetIsMyTeam;
        isCasterMyTeam = casterIsMyTeam;
        onComplete = callback;
    }
}

/// <summary>
/// 이펙트 재생 대기열 및 풀링 관리자
/// - Queue로 연출 순차 처리
/// - Object Pooling으로 성능 최적화
/// - 자동 flipX 처리
/// </summary>
public class EffectManager : MonoBehaviour
{
    [Header("오브젝트 풀 설정")]
    [SerializeField] private GameObject effectPlayerPrefab; // EffectCDPlayer 프리팹
    [SerializeField] private int poolSize = 5; // 초기 풀 크기
    [SerializeField] private Transform poolParent; // 풀 부모 Transform

    [Header("성능 설정")]
    [SerializeField] private bool enableParallelEffects = false; // 병렬 재생 허용 여부
    [SerializeField] private int maxParallelEffects = 3; // 동시 재생 최대 개수

    private Queue<EffectRequest> effectQueue = new Queue<EffectRequest>(); // 연출 대기열
    private List<EffectCDPlayer> playerPool = new List<EffectCDPlayer>(); // 플레이어 풀
    private HashSet<EffectCDPlayer> activePlayers = new HashSet<EffectCDPlayer>(); // 현재 재생 중인 플레이어

    private bool isProcessingQueue = false; // 대기열 처리 중 여부

    private static EffectManager instance;
    public static EffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EffectManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("EffectManager");
                    instance = go.AddComponent<EffectManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // 풀 초기화
        InitializePool();
    }

    /// <summary>
    /// 오브젝트 풀 초기화
    /// </summary>
    private void InitializePool()
    {
        if (poolParent == null)
        {
            GameObject poolObj = new GameObject("EffectPlayerPool");
            poolObj.transform.SetParent(transform);
            poolParent = poolObj.transform;
        }

        // 프리팹이 없으면 동적 생성
        if (effectPlayerPrefab == null)
        {
            effectPlayerPrefab = CreateDefaultEffectPlayerPrefab();
        }

        // 초기 풀 생성
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewPlayer();
        }

        Debug.Log($"[EffectManager] 오브젝트 풀 초기화 완료: {poolSize}개");
    }

    /// <summary>
    /// 기본 EffectCDPlayer 프리팹 생성
    /// </summary>
    private GameObject CreateDefaultEffectPlayerPrefab()
    {
        GameObject prefab = new GameObject("EffectCDPlayer");

        // Canvas 추가 (Sorting Order 관리용)
        Canvas canvas = prefab.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 50;

        // GraphicRaycaster 추가 (필요 시)
        prefab.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // RectTransform 설정
        RectTransform rectTransform = prefab.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 200);

        // Image 추가
        UnityEngine.UI.Image image = prefab.AddComponent<UnityEngine.UI.Image>();
        image.raycastTarget = false;

        // EffectCDPlayer 추가
        EffectCDPlayer player = prefab.AddComponent<EffectCDPlayer>();

        return prefab;
    }

    /// <summary>
    /// 새로운 플레이어 생성 및 풀에 추가
    /// </summary>
    private EffectCDPlayer CreateNewPlayer()
    {
        GameObject playerObj = Instantiate(effectPlayerPrefab, poolParent);
        playerObj.name = $"EffectPlayer_{playerPool.Count}";
        playerObj.SetActive(false);

        EffectCDPlayer player = playerObj.GetComponent<EffectCDPlayer>();
        if (player == null)
        {
            player = playerObj.AddComponent<EffectCDPlayer>();
        }

        playerPool.Add(player);
        return player;
    }

    /// <summary>
    /// 풀에서 사용 가능한 플레이어 가져오기
    /// </summary>
    private EffectCDPlayer GetAvailablePlayer()
    {
        // 비활성화된 플레이어 찾기
        foreach (var player in playerPool)
        {
            if (!player.gameObject.activeSelf && !activePlayers.Contains(player))
            {
                player.gameObject.SetActive(true);
                activePlayers.Add(player);
                return player;
            }
        }

        // 모두 사용 중이면 새로 생성 (동적 확장)
        Debug.LogWarning("[EffectManager] 풀 크기 부족. 새 플레이어 생성.");
        EffectCDPlayer newPlayer = CreateNewPlayer();
        newPlayer.gameObject.SetActive(true);
        activePlayers.Add(newPlayer);
        return newPlayer;
    }

    /// <summary>
    /// 플레이어를 풀로 반환
    /// </summary>
    private void ReturnPlayer(EffectCDPlayer player)
    {
        if (player == null) return;

        activePlayers.Remove(player);
        player.gameObject.SetActive(false);
    }

    /// <summary>
    /// 이펙트 재생 요청 (대기열에 추가)
    /// </summary>
    public void RequestEffect(EffectRequest request)
    {
        effectQueue.Enqueue(request);

        // 대기열 처리 시작
        if (!isProcessingQueue)
        {
            _ = ProcessQueueAsync();
        }
    }

    /// <summary>
    /// 이펙트 재생 요청 (간편 메서드)
    /// </summary>
    public void RequestEffect(
        EffectCD effectCD,
        RectTransform targetTransform = null,
        RectTransform casterTransform = null,
        bool isTargetMyTeam = false,
        bool isCasterMyTeam = true,
        System.Action onComplete = null)
    {
        EffectRequest request = new EffectRequest(
            effectCD,
            targetTransform,
            casterTransform,
            isTargetMyTeam,
            isCasterMyTeam,
            onComplete
        );

        RequestEffect(request);
    }

    /// <summary>
    /// 대기열 순차 처리
    /// </summary>
    private async Task ProcessQueueAsync()
    {
        isProcessingQueue = true;

        while (effectQueue.Count > 0)
        {
            // 병렬 재생 제한 확인
            if (enableParallelEffects)
            {
                // 동시 재생 개수 제한
                while (activePlayers.Count >= maxParallelEffects && effectQueue.Count > 0)
                {
                    await Task.Delay(50); // 50ms 대기
                }

                // 동시 재생 가능하면 바로 시작
                if (effectQueue.Count > 0)
                {
                    EffectRequest request = effectQueue.Dequeue();
                    _ = PlayEffectAsync(request); // Fire-and-forget (병렬 실행)
                }
            }
            else
            {
                // 순차 재생: 이전 연출이 끝날 때까지 대기
                EffectRequest request = effectQueue.Dequeue();
                await PlayEffectAsync(request);
            }
        }

        isProcessingQueue = false;
    }

    /// <summary>
    /// 이펙트 재생 실행
    /// </summary>
    private async Task PlayEffectAsync(EffectRequest request)
    {
        if (request.effectCD == null)
        {
            Debug.LogWarning("[EffectManager] EffectCD가 null입니다.");
            request.onComplete?.Invoke();
            return;
        }

        // 풀에서 플레이어 가져오기
        EffectCDPlayer player = GetAvailablePlayer();

        // 자동 flipX 결정
        bool autoFlipX = DetermineFlipX(request);
        player.flipX = autoFlipX;

        // 이펙트 재생 (타임아웃 5초)
        bool completed = await player.PlayWithTimeout(
            request.effectCD,
            timeoutSeconds: 5f,
            targetTransform: request.targetTransform,
            casterTransform: request.casterTransform,
            onComplete: () =>
            {
                // 재생 완료 후 풀로 반환
                ReturnPlayer(player);
                request.onComplete?.Invoke();
            }
        );

        if (!completed)
        {
            Debug.LogWarning($"[EffectManager] 이펙트 '{request.effectCD.name}' 타임아웃.");
            ReturnPlayer(player);
        }
    }

    /// <summary>
    /// 진영에 따른 자동 flipX 결정
    /// </summary>
    private bool DetermineFlipX(EffectRequest request)
    {
        // EffectCD에서 반전 허용 안 하면 무조건 false
        if (!request.effectCD.AllowFlip)
        {
            return false;
        }

        // EffectType에 따라 기준 진영 결정
        bool referenceTeam = request.effectCD.effectType switch
        {
            EffectType.Target => request.isTargetMyTeam,   // Target: 피격자 기준
            EffectType.Caster => request.isCasterMyTeam,   // Caster: 시전자 기준
            EffectType.Screen => false,                    // Screen: 반전 없음
            _ => false
        };

        // 적군(Enemy)이면 반전 (우측을 향하도록)
        return !referenceTeam;
    }

    /// <summary>
    /// 대기열 및 재생 중인 이펙트 모두 취소
    /// </summary>
    public void CancelAllEffects()
    {
        // 대기열 비우기
        effectQueue.Clear();

        // 재생 중인 모든 플레이어 중지
        foreach (var player in activePlayers)
        {
            player.Stop(resetVisual: true);
        }

        activePlayers.Clear();
        isProcessingQueue = false;

        Debug.Log("[EffectManager] 모든 이펙트 취소됨.");
    }

    /// <summary>
    /// 대기열 상태 확인
    /// </summary>
    public int GetQueueCount() => effectQueue.Count;

    /// <summary>
    /// 활성 플레이어 수 확인
    /// </summary>
    public int GetActivePlayerCount() => activePlayers.Count;

    private void OnDestroy()
    {
        CancelAllEffects();
    }
}
