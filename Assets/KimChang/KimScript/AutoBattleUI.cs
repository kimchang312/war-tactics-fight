using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AutoBattleManager;

public class AutoBattleUI : MonoBehaviour
{
    [SerializeField] private Transform canvasTransform;
    [SerializeField] private GameObject canvas;

    private RewardUI rewardUI;

    [SerializeField] private TextMeshProUGUI _myUnitCountUI;
    [SerializeField] private TextMeshProUGUI _enemyUnitCountUI;
    [SerializeField] private TextMeshProUGUI _myUnitHPUI;
    [SerializeField] private TextMeshProUGUI _emyUnitHPUI;
    [SerializeField] private TextMeshProUGUI _mySecondHpText;
    [SerializeField] private TextMeshProUGUI _enemySecondHpText;


    [SerializeField] private TextMeshProUGUI moraleText;
    [SerializeField] private ObjectPool objectPool;
    [SerializeField] private GameObject abilityPool;

    [SerializeField] private TextMeshProUGUI _myDodge;
    [SerializeField] private TextMeshProUGUI _enemyDodge;
    [SerializeField] private Slider myHpBar;
    [SerializeField] private Slider enemyHpBar;
    [SerializeField] private Slider mySecondHpBar;
    [SerializeField] private Slider enemySecondHpBar;
    [SerializeField] private GameObject myRangeCount;
    [SerializeField] private GameObject enemyRangeCount;

    [SerializeField] private Transform myBackUnitsParent;
    [SerializeField] private Transform enemyBackUnitsParent;

    [SerializeField] private GameObject relicBox;
    [SerializeField] private Transform myAbilityBox;
    [SerializeField] private Transform enemyAbilityBox;

    [SerializeField] private GameObject itemToolTip;
    [SerializeField] private Image background;
    [SerializeField] private GameObject goTestBtn;
    [SerializeField] private BattleCrashAnimation battleAnim;

    [SerializeField] private GameObject myBuffDeBuff;
    [SerializeField] private GameObject enemyBuffDeBuff;

    private Vector3 myTeam = new(260, 280, 0);
    private Vector3 enemyTeam = new(-260, 280, 0);

    private float waittingTime = 500f;

    // 사용처: 데미지 텍스트가 "처음부터" 더 위에서 뜨게 하는 스폰 오프셋
    [SerializeField] private float damageTextSpawnYOffset = 100f;

    // 사용처: 뜬 뒤 추가로 위로 올라가는 거리(기존 80f)
    [SerializeField] private float damageTextRise = 80f;

    // 사용처: 원거리/투창 투사체 이미지 Resources 경로
    private const string ThrowSpearProjectilePath = "BattleAnimation/ThrowSpear";
    private const string RangedProjectilePath = "BattleAnimation/RanageAttack";

    // 사용처: 투창/원거리 투사체 크기 조절
    [SerializeField] private Vector2 throwSpearProjectileSize = new Vector2(150f, 150f);
    [SerializeField] private Vector2 rangedProjectileSize = new Vector2(110f, 110f);

    // 사용처: 투사체가 유닛 중심이 아니라 몸통 위쪽에서 출발/도착하도록 보정
    [SerializeField] private float projectileStartYOffset = 60f;
    [SerializeField] private float projectileTargetYOffset = 70f;

    // 사용처: 곡선 비행 높이 조절
    [SerializeField] private float projectileArcMinHeight = 120f;
    [SerializeField] private float projectileArcRate = 0.22f;

    // 사용처: 여러 화살이 동시에 날아갈 때 겹치지 않도록 벌리는 간격
    [SerializeField] private float projectileSpread = 18f;

    // 사용처: 원거리 유닛이 많을 때 화면/성능 부담을 줄이기 위한 최대 화살 수
    [SerializeField] private int maxRangedProjectileCount = 8;

    // 사용처: 여러 화살 발사 시 미세한 시간차
    [SerializeField] private float rangedProjectileStaggerSec = 0.035f;

    // 사용처: 현재 투사체 스프라이트가 기본적으로 우상단 45도 방향을 바라보는 기준 각도
    [SerializeField] private float projectileSpriteForwardAngle = 45f;

    // 사용처: 현재 화면에 표시 중인 원거리 공격 유닛 수 캐싱
    private int myRangeAttackCount;
    private int enemyRangeAttackCount;



    // 사용처: 좌표 변환(월드 -> 캔버스 로컬) 캐싱
    private RectTransform canvasRt;
    private Canvas rootCanvas;
    private Camera uiCam;
    private bool damageAnchorCacheReady;
    private readonly Dictionary<string, GameObject> unitViewMap = new(32);
    [SerializeField] private float buffDeBuffIconSize = 60f;
    private static readonly int[] visibleBuffDeBuffIds =
{
    0, // 작열
    1, // 상흔
    2, // 연막
    3, // 추적자 표식
    4, // 제국 시너지
    8  // 위압
};
    // 사용처: 전투 유닛 화면 오브젝트를 UniqueId 기준으로 빠르게 찾기 위한 키 생성
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetUnitViewKey(RogueUnitDataBase unit)
    {
        if (unit == null)
            return string.Empty;

        object keyObj = unit.UniqueId;
        return keyObj?.ToString() ?? string.Empty;
    }

    // 사용처: 생성된 유닛 UI를 UniqueId 기준으로 등록
    private void RegisterUnitView(RogueUnitDataBase unit, GameObject view)
    {
        string key = GetUnitViewKey(unit);
        if (!string.IsNullOrEmpty(key) && view != null)
        {
            unitViewMap[key] = view;
        }
    }

    // 사용처: 사망 처리 시 현재 화면에 남아 있는 해당 유닛 UI를 빠르게 조회
    private bool TryGetUnitView(RogueUnitDataBase unit, out GameObject view)
    {
        view = null;

        string key = GetUnitViewKey(unit);
        if (string.IsNullOrEmpty(key))
            return false;

        if (!unitViewMap.TryGetValue(key, out view))
            return false;

        return view != null;
    }

    // 사용처: 사망 페이드 시작 시 더 이상 재조회되지 않도록 등록 제거
    private void RemoveUnitView(RogueUnitDataBase unit)
    {
        string key = GetUnitViewKey(unit);
        if (!string.IsNullOrEmpty(key))
        {
            unitViewMap.Remove(key);
        }
    }

    // 사용처: 재사용되는 풀 오브젝트의 알파를 항상 1로 복구
    private void RestoreGraphicRoot(GameObject root)
    {
        if (root == null)
            return;

        root.SetActive(true);

        Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] == null)
                continue;

            graphics[i].DOKill(false);

            Color c = graphics[i].color;
            graphics[i].color = new Color(c.r, c.g, c.b, 1f);
        }
    }

    // 사용처: 유닛 사망 연출 시 연결된 UI 전체를 같은 시간으로 페이드
    private void FadeOutGraphicRoot(GameObject root, float duration)
    {
        if (root == null || !root.activeSelf)
            return;

        Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] == null)
                continue;

            graphics[i].DOKill(false);
            graphics[i].DOFade(0f, duration);
        }

        StartCoroutine(DisableUiRootAfterFade(root, duration));
    }

    // 사용처: 페이드 종료 후 루트 오브젝트 비활성화
    private IEnumerator DisableUiRootAfterFade(GameObject root, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (root != null)
            root.SetActive(false);
    }

    // 사용처: 전열 유닛이 새로 그려질 때 체력바/회피율/어빌리티 UI를 원상복구
    private void RestoreFrontRuntimeUi(bool isMyUnit)
    {
        RestoreGraphicRoot((isMyUnit ? myHpBar : enemyHpBar)?.gameObject);
        RestoreGraphicRoot((isMyUnit ? _myUnitHPUI : _emyUnitHPUI)?.gameObject);
        RestoreGraphicRoot((isMyUnit ? myAbilityBox : enemyAbilityBox)?.gameObject);

        TMP_Text dodge = isMyUnit ? _myDodge : _enemyDodge;
        if (dodge != null)
        {
            dodge.gameObject.SetActive(true);
            Color c = dodge.color;
            dodge.color = new Color(c.r, c.g, c.b, 1f);
        }
    }

    // 사용처: 2번 슬롯 유닛이 새로 그려질 때 2번 체력 UI를 원상복구
    private void RestoreSecondRuntimeUi(bool isMyUnit)
    {
        RestoreGraphicRoot((isMyUnit ? mySecondHpBar : enemySecondHpBar)?.gameObject);
        RestoreGraphicRoot((isMyUnit ? _mySecondHpText : _enemySecondHpText)?.gameObject);
    }

    // 사용처: 전열이 없을 때 전열 전용 UI를 숨김
    private void HideFrontRuntimeUi(bool isMyUnit)
    {
        TMP_Text dodge = isMyUnit ? _myDodge : _enemyDodge;
        if (dodge != null)
            dodge.gameObject.SetActive(false);

        Transform abilityBox = isMyUnit ? myAbilityBox : enemyAbilityBox;
        if (abilityBox != null)
            abilityBox.gameObject.SetActive(false);
    }

    // 사용처: 유닛 사망 시 해당 슬롯에 연결된 체력바/어빌리티 UI도 함께 페이드
    private void FadeOutLinkedUi(int unitIndex, bool isMyUnit, float duration)
    {
        if (unitIndex == 0)
        {
            TMP_Text dodge = isMyUnit ? _myDodge : _enemyDodge;
            if (dodge != null)
                dodge.gameObject.SetActive(false);

            FadeOutGraphicRoot((isMyUnit ? myHpBar : enemyHpBar)?.gameObject, duration);
            FadeOutGraphicRoot((isMyUnit ? _myUnitHPUI : _emyUnitHPUI)?.gameObject, duration);
            FadeOutGraphicRoot((isMyUnit ? myAbilityBox : enemyAbilityBox)?.gameObject, duration);
        }
        else if (unitIndex == 1)
        {
            FadeOutGraphicRoot((isMyUnit ? mySecondHpBar : enemySecondHpBar)?.gameObject, duration);
            FadeOutGraphicRoot((isMyUnit ? _mySecondHpText : _enemySecondHpText)?.gameObject, duration);
        }
    }

    private void Start()
    {
        if (battleAnim == null) battleAnim = FindObjectOfType<BattleCrashAnimation>();
        goTestBtn.SetActive(false);
        if (rewardUI == null)
        {
            rewardUI = GameManager.Instance.rewardUI;
        }

        int fieldId = RogueLikeData.Instance.GetFieldId();
        switch (fieldId)
        {

            case 2:
                {
                    background.sprite = SpriteCacheManager.GetSprite("EventImages/Forest");
                    break;
                }
            case 3:
                {
                    background.sprite = SpriteCacheManager.GetSprite("EventImages/Mountain");
                    break;
                }
            case 4:
                {
                    background.sprite = SpriteCacheManager.GetSprite("EventImages/Swampland");
                    break;
                }
            default:
                background.sprite = SpriteCacheManager.GetSprite("EventImages/Background");

                break;
        }

        ResetUIActive();

        myHpBar.interactable = false;
        enemyHpBar.interactable = false;
        if (mySecondHpBar != null) mySecondHpBar.interactable = false;
        if (enemySecondHpBar != null) enemySecondHpBar.interactable = false;

        UpdateMorale();
    }
    public void UpdateUnitCountUI(int myUnitCount, int enemyUnitCount)
    {
        _myUnitCountUI.text = $"{myUnitCount}";
        _enemyUnitCountUI.text = $"{enemyUnitCount}";

    }
    public void UpdateUnitHPUI(float myUnitHP, float enemyUnitHP, float myMaxHp, float enemyMaxHp)
    {
        // 널 체크 버그 수정
        if (_myUnitHPUI != null && _emyUnitHPUI != null)
        {
            _myUnitHPUI.text = $"{myUnitHP}/{myMaxHp}";
            _emyUnitHPUI.text = $"{enemyUnitHP}/{enemyMaxHp}";
        }

        myHpBar.maxValue = myMaxHp;
        myHpBar.value = myUnitHP;

        enemyHpBar.maxValue = enemyMaxHp;
        enemyHpBar.value = enemyUnitHP;
    }
    //데미지 텍스트 표기 함수
    public void ShowDamage(float _damage, string text, bool team, bool isAttack, int unitIndex)
    {
        // 사용처: 데미지 표시 색상/문구 판정. 표기는 절댓값, 색은 부호로.
        float offsetX = 50f;
        float damage = -_damage;

        bool isRangedProjectile = ContainsBattleText(text, "원거리");
        bool isThrowSpearProjectile = ContainsBattleText(text, "투창");

        // 원거리/투창은 유닛 돌진 애니메이션을 사용하지 않고 투사체만 사용
        if (!isRangedProjectile && !isThrowSpearProjectile)
        {
            BattleAnimation(damage, text, team, isAttack);
        }

        // team은 피격 팀이다. 그대로 그 팀의 유닛을 찾는다.
        Vector2 pos = GetDamageAnchor(unitIndex, team, offsetX);

        if (isRangedProjectile || isThrowSpearProjectile)
        {
            StartCoroutine(PlayProjectileAndShowDamage(
                damage,
                text,
                team,
                unitIndex,
                pos,
                isRangedProjectile,
                isThrowSpearProjectile));

            return;
        }

        if (isAttack)
            StartCoroutine(DelayedDamageDisplay(damage, text, pos));
        else
            ShowDamageInternalWithPosition(damage, text, pos);
    }

    private IEnumerator DelayedDamageDisplay(float damage, string text, Vector2 displayPos)
    {
        yield return new WaitForSeconds(waittingTime * 0.0005f);
        ShowDamageInternalWithPosition(damage, text, displayPos);
    }
    private Vector2 GetDamageAnchor(int unitIndex, bool isMyUnit, float offsetX)
    {
        EnsureDamageAnchorCache();

        GameObject unit = FindUnit(unitIndex, isMyUnit);

        // 유닛이 있고 캔버스 RectTransform 캐시가 준비된 경우:
        // 유닛이 어떤 부모 아래에 있든(캔버스/백라인) 월드 -> 캔버스 로컬로 변환해서 정확한 위치를 얻는다
        if (unit != null && canvasRt != null)
        {
            RectTransform unitRt = unit.GetComponent<RectTransform>();

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, unitRt.position);

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, uiCam, out localPoint);

            // "처음부터 더 높은 위치" + 기존 X 오프셋
            localPoint.x += offsetX;
            localPoint.y += damageTextSpawnYOffset;

            return localPoint;
        }

        // 유닛을 못 찾은 예외 상황 폴백(전열 기본값) + 오프셋 적용
        Vector2 fallback = isMyUnit ? (Vector2)myTeam : (Vector2)enemyTeam;
        fallback.x += offsetX;
        fallback.y += damageTextSpawnYOffset;
        return fallback;
    }
    private void ShowDamageInternalWithPosition(float damage, string text, Vector2 anchoredPosition)
    {
        GameObject go = objectPool.GetDamageText();

        // damageText는 UI이므로 반드시 캔버스 아래에 두기
        if (go.transform.parent != canvasTransform)
            go.transform.SetParent(canvasTransform, false);

        // ObjectPool.GetDamageText()에서 이미 SetActive(true)지만 혹시 모를 케이스 방어
        if (!go.activeSelf) go.SetActive(true);

        // MoveDamageUI가 OnEnable에서 월드좌표 트윈을 자동 시작할 수 있음
        // 여기서 즉시 Kill하면 첫 프레임 이동도 막힌다
        go.transform.DOKill(false);

        RectTransform rt = go.GetComponent<RectTransform>();
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();

        // 이전에 걸린 "go 타겟 시퀀스" 정리 (complete=false로 OnComplete가 튀는 상황 방지)
        DOTween.Kill(go, false);
        rt.DOKill(false);
        tmp.DOKill(false);

        // 텍스트/색상 세팅(색상 설정 시 alpha=1로 복구됨)
        tmp.color = (damage >= 0) ? Color.green : Color.red;
        tmp.text = (damage == 0) ? $"{text}" : $"{Mathf.Abs(damage)} {text}";

        // 시작 위치(이미 GetDamageAnchor에서 스폰 Y 오프셋 적용됨)
        rt.anchoredPosition = anchoredPosition;

        float dur = Mathf.Max(0.15f, waittingTime * 0.0012f);

        DOTween.Sequence()
            .SetTarget(go) // 이후 DOTween.Kill(go)로 한 번에 정리 가능
            .Join(rt.DOAnchorPosY(anchoredPosition.y + damageTextRise, dur))
            .Join(tmp.DOFade(0f, dur))
            .OnComplete(() =>
            {
                // 풀로 돌려주기 전에 알파 복구
                var c = tmp.color;
                tmp.color = new Color(c.r, c.g, c.b, 1f);

                objectPool.ReturnDamageText(go);
            });
    }

    // 사용처: ShowDamage에서 특정 전투 텍스트가 포함되었는지 빠르게 판정
    private static bool ContainsBattleText(string text, string keyword)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(keyword))
            return false;

        return text.IndexOf(keyword, StringComparison.Ordinal) >= 0;
    }

    // 사용처: 원거리/투창 투사체 연출 후 데미지 텍스트 표시
    private IEnumerator PlayProjectileAndShowDamage(
        float damage,
        string text,
        bool hitTeam,
        int targetUnitIndex,
        Vector2 damageTextPos,
        bool isRangedProjectile,
        bool isThrowSpearProjectile)
    {
        if (objectPool == null || canvasTransform == null)
        {
            ShowDamageInternalWithPosition(damage, text, damageTextPos);
            yield break;
        }

        if (isThrowSpearProjectile)
            BGMManager.Instance?.PlaySE("se_ThrowSpear");
        else if (isRangedProjectile)
            BGMManager.Instance?.PlaySE("se_RangeAttack");

        bool attackerIsMyUnit = !hitTeam;

        Sprite projectileSprite = SpriteCacheManager.GetSprite(
            isThrowSpearProjectile ? ThrowSpearProjectilePath : RangedProjectilePath);

        if (projectileSprite == null)
        {
            ShowDamageInternalWithPosition(damage, text, damageTextPos);
            yield break;
        }

        int projectileCount = isRangedProjectile ? GetRangedProjectileCount(attackerIsMyUnit) : 1;
        Vector2 projectileSize = isThrowSpearProjectile ? throwSpearProjectileSize : rangedProjectileSize;

        Vector2 startPos = GetProjectileStartLocal(attackerIsMyUnit, isRangedProjectile);
        Vector2 endPos = GetProjectileTargetLocal(hitTeam, targetUnitIndex);

        float duration = GetProjectileDuration(isThrowSpearProjectile);
        float delayStep = isRangedProjectile ? Mathf.Min(rangedProjectileStaggerSec, duration * 0.2f) : 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            StartCoroutine(PlayOneProjectile(
                projectileSprite,
                projectileSize,
                startPos,
                endPos,
                i,
                projectileCount,
                delayStep * i,
                duration));
        }

        yield return new WaitForSeconds(duration + delayStep * Mathf.Max(0, projectileCount - 1));

        ShowDamageInternalWithPosition(damage, text, damageTextPos);
    }

    // 사용처: 현재 원거리 공격 UI 카운트를 기준으로 동시에 날릴 화살 수 계산
    private int GetRangedProjectileCount(bool attackerIsMyUnit)
    {
        int count = attackerIsMyUnit ? myRangeAttackCount : enemyRangeAttackCount;

        if (count <= 0)
            count = 1;

        return Mathf.Clamp(count, 1, Mathf.Max(1, maxRangedProjectileCount));
    }

    // 사용처: 배속에 맞춰 투사체 비행 시간 계산
    private float GetProjectileDuration(bool isThrowSpearProjectile)
    {
        float rate = isThrowSpearProjectile ? 0.0009f : 0.00075f;
        return Mathf.Clamp(waittingTime * rate, 0.15f, 1.2f);
    }

    // 사용처: 원거리 공격은 원거리 아이콘에서, 투창은 전열 유닛에서 출발
    private Vector2 GetProjectileStartLocal(bool attackerIsMyUnit, bool useRangeIcon)
    {
        GameObject startObj = null;

        if (useRangeIcon)
            startObj = FindRangeUnit(attackerIsMyUnit);

        if (startObj == null)
            startObj = FindUnit(0, attackerIsMyUnit);

        if (TryGetCanvasLocalPosition(startObj, out Vector2 localPos))
        {
            localPos.y += projectileStartYOffset;
            return localPos;
        }

        if (useRangeIcon)
            return new Vector2(attackerIsMyUnit ? -830f : 830f, -220f + projectileStartYOffset);

        return new Vector2(attackerIsMyUnit ? -290f : 290f, 60f + projectileStartYOffset);
    }

    // 사용처: 피격 대상 유닛의 캔버스 로컬 좌표 계산
    private Vector2 GetProjectileTargetLocal(bool hitTeam, int targetUnitIndex)
    {
        GameObject targetObj = FindUnit(targetUnitIndex, hitTeam);

        if (targetObj == null && targetUnitIndex != 0)
            targetObj = FindUnit(0, hitTeam);

        if (TryGetCanvasLocalPosition(targetObj, out Vector2 localPos))
        {
            localPos.y += projectileTargetYOffset;
            return localPos;
        }

        return new Vector2(hitTeam ? -290f : 290f, 60f + projectileTargetYOffset);
    }

    // 사용처: 화면에 표시된 원거리 유닛 묶음 아이콘 검색
    private GameObject FindRangeUnit(bool isMyUnit)
    {
        if (canvasTransform == null)
            return null;

        string unitName = $"{(isMyUnit ? "My" : "Enemy")}RangeUnit";

        foreach (Transform child in canvasTransform)
        {
            if (child.name == unitName && child.gameObject.activeSelf)
                return child.gameObject;
        }

        return null;
    }

    // 사용처: 어떤 부모 아래에 있는 UI든 캔버스 로컬 좌표로 변환
    private bool TryGetCanvasLocalPosition(GameObject go, out Vector2 localPos)
    {
        localPos = Vector2.zero;

        if (go == null)
            return false;

        EnsureDamageAnchorCache();

        if (canvasRt == null)
            return false;

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null)
            return false;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, rt.position);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, uiCam, out localPos);
    }

    // 사용처: WeaponImage 하나를 곡선 경로로 이동시키고 완료 후 풀에 반환
    private IEnumerator PlayOneProjectile(
        Sprite sprite,
        Vector2 size,
        Vector2 start,
        Vector2 end,
        int projectileIndex,
        int projectileCount,
        float delay,
        float duration)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        GameObject go = objectPool.GetWeaponImage();
        if (go == null)
            yield break;

        go.SetActive(true);

        RectTransform rt = go.GetComponent<RectTransform>();
        Image img = go.GetComponent<Image>();

        if (rt == null || img == null)
        {
            objectPool.ReturnWeaponImage(go);
            yield break;
        }

        DOTween.Kill(go, false);
        rt.DOKill(false);

        rt.SetParent(canvasTransform, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

        img.sprite = sprite;
        img.enabled = true;
        img.preserveAspect = true;

        Color c = img.color;
        img.color = new Color(c.r, c.g, c.b, 1f);

        Vector2 dir = end - start;
        if (dir.sqrMagnitude < 0.001f)
            dir = Vector2.right;
        else
            dir.Normalize();

        Vector2 perpendicular = new Vector2(-dir.y, dir.x);
        float spreadOffset = (projectileIndex - (projectileCount - 1) * 0.5f) * projectileSpread;

        Vector2 spreadStart = start + perpendicular * spreadOffset;
        Vector2 spreadEnd = end + perpendicular * spreadOffset * 0.25f;

        float arcHeight = GetProjectileArcHeight(spreadStart, spreadEnd);
        Vector2 control = (spreadStart + spreadEnd) * 0.5f + Vector2.up * arcHeight;

        float t = 0f;
        bool returned = false;

        UpdateProjectileTransform(rt, spreadStart, control, spreadEnd, 0f);

        Tween tween = DOTween.To(
                () => t,
                value =>
                {
                    t = value;
                    UpdateProjectileTransform(rt, spreadStart, control, spreadEnd, t);
                },
                1f,
                duration)
            .SetEase(Ease.Linear)
            .SetTarget(go)
            .OnComplete(() =>
            {
                returned = true;
                objectPool.ReturnWeaponImage(go);
            });

        yield return tween.WaitForCompletion();

        if (!returned && go != null)
            objectPool.ReturnWeaponImage(go);
    }

    // 사용처: 투사체 곡선 높이 계산
    private float GetProjectileArcHeight(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        return Mathf.Max(projectileArcMinHeight, distance * projectileArcRate);
    }

    // 사용처: 베지어 곡선 위치/회전 갱신
    private void UpdateProjectileTransform(RectTransform rt, Vector2 start, Vector2 control, Vector2 end, float t)
    {
        if (rt == null)
            return;

        float inv = 1f - t;

        Vector2 pos =
            inv * inv * start +
            2f * inv * t * control +
            t * t * end;

        Vector2 tangent =
            2f * inv * (control - start) +
            2f * t * (end - control);

        rt.anchoredPosition = pos;

        if (tangent.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - projectileSpriteForwardAngle;
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }


    private void BattleAnimation(float damage, string text, bool team, bool isAttack)
    {
        if (!isAttack) return;

        GameObject unit = FindUnit(0, team);
        if (unit == null) return;

        RectTransform rectTransform = unit.GetComponent<RectTransform>();

        rectTransform.DOKill(false);

        Vector2 originPos = rectTransform.anchoredPosition;
        float direction = team ? 1f : -1f;

        Vector2 moveBackPos = originPos + new Vector2(direction * -10f, 0f);
        Vector2 moveForwardPos = originPos + new Vector2(direction * 25f, 0f);

        const float backSec = 0.05f;
        const float waitSec = 0.20f;
        const float forwardSec = 0.20f;
        const float returnSec = 0.05f;

        // 전투 애니 시작 시점에 검을 먼저 "생성"
        StartCoroutine(RunCrashAnimation(team));

        Sequence attackSequence = DOTween.Sequence();
        attackSequence
            .Append(rectTransform.DOAnchorPos(moveBackPos, backSec))
            .AppendInterval(waitSec)
            .Append(rectTransform.DOAnchorPos(moveForwardPos, forwardSec))
            .Append(rectTransform.DOAnchorPos(originPos, returnSec));
    }


    private IEnumerator RunCrashAnimation(bool team)
    {
        if (battleAnim == null || objectPool == null) yield break;

        // 전열 기준 오브젝트 찾기
        GameObject myFrontObj = FindUnit(0, true);
        GameObject enFrontObj = FindUnit(0, false);
        if (myFrontObj == null || enFrontObj == null) yield break;

        RectTransform myAttach = myFrontObj.GetComponent<RectTransform>();
        RectTransform enAttach = enFrontObj.GetComponent<RectTransform>();

        // 테스트: 페이즈=충돌(4), id=1만 사용
        var myIds = new List<int> { 1 };
        var enIds = new List<int> { 1 };

        // Task를 코루틴으로 대기
        var task = battleAnim.PlayPhaseAsync(
            phase: 4,                  // 내부 매핑에서 4=충돌
            myAttach: myAttach,
            enemyAttach: enAttach,
            pool: objectPool,
            myStyleIds: myIds,
            enemyStyleIds: enIds
        );

        while (!task.IsCompleted) yield return null;
    }


    //유닛 생성 코드
    public void CreateUnitBox(
     List<RogueUnitDataBase> myUnits,
     List<RogueUnitDataBase> enemyUnits,
     float myDodge, float enemyDodge,
     List<RogueUnitDataBase> myRangeUnits,
     List<RogueUnitDataBase> enemyRangeUnits)
    {
        Vector3[] myPositions = { new Vector3(-290, 60, 0), new Vector3(-575, 110, 0) };
        Vector3[] enemyPositions = { new Vector3(290, 60, 0), new Vector3(575, 110, 0) };
        Vector3 myRangeUnitPos = new Vector3(-830, -220, 0);
        Vector3 enemyRangeUnitPos = new Vector3(830, -220, 0);

        // 변경: 첫 유닛 240, 이후 140
        float firstSize = 240f;
        float secondSize = 140f;

        ClearExistingUnitImages();

        ClearExistingAbilityIcons();

        RestoreFrontRuntimeUi(true);
        RestoreFrontRuntimeUi(false);
        RestoreSecondRuntimeUi(true);
        RestoreSecondRuntimeUi(false);

        if (myUnits != null && myUnits.Count > 0)
            CreateAbilityIcons(myUnits[0], true);
        else
            HideFrontRuntimeUi(true);

        if (enemyUnits != null && enemyUnits.Count > 0)
            CreateAbilityIcons(enemyUnits[0], false);
        else
            HideFrontRuntimeUi(false);

        CreateUnitImages(myUnits ?? new List<RogueUnitDataBase>(), myPositions, firstSize, secondSize, true, myDodge);

        CreateRangeUnit(myRangeUnits != null ? myRangeUnits.Count : 0, myRangeUnitPos, myRangeCount, true);

        CreateUnitImages(enemyUnits ?? new List<RogueUnitDataBase>(), enemyPositions, firstSize, secondSize, false, enemyDodge);

        CreateRangeUnit(enemyRangeUnits != null ? enemyRangeUnits.Count : 0, enemyRangeUnitPos, enemyRangeCount, false);

        UpdateBuffDeBuffUI(myUnits, enemyUnits);
    }

    private void ClearExistingUnitImages()
    {
        unitViewMap.Clear();

        foreach (var unit in objectPool.GetActiveBattleUnits())
        {
            objectPool.ReturnBattleUnit(unit);
        }
    }

    // 사용처: 원거리 공격 가능한 유닛 수를 활 아이콘과 숫자로 표시
    private void CreateRangeUnit(int rangeUnitCount, Vector3 position, GameObject number, bool isMyTeam)
    {
        if (isMyTeam)
            myRangeAttackCount = Mathf.Max(0, rangeUnitCount);
        else
            enemyRangeAttackCount = Mathf.Max(0, rangeUnitCount);

        string myTeam = isMyTeam ? "My" : "Enemy";

        Image numberImg = number.GetComponent<Image>();

        if (rangeUnitCount == 0)
        {
            numberImg.color = new Color(1, 1, 1, 0);
            return;
        }

        numberImg.color = new Color(1, 1, 1, 1);

        GameObject unit = objectPool.GetBattleUnit();
        unit.transform.SetParent(canvasTransform, false);
        unit.transform.localScale = isMyTeam ? new Vector2(1, 1) : new Vector2(-1, 1);

        RestoreGraphicRoot(unit);

        RectTransform rectTransform = unit.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Image img = unit.GetComponent<Image>();
        img.sprite = SpriteCacheManager.GetSprite("KIcon/AbilityIcon/rangedAttack");

        Transform childUnit = unit.transform.GetChild(0);
        Image childImg = childUnit.GetComponent<Image>();

        if (childImg != null)
        {
            childImg.sprite = null;
            childImg.color = new Color(1f, 1f, 1f, 0f);
        }

        numberImg.sprite = SpriteCacheManager.GetSprite($"KIcon/UI_{myTeam}X{rangeUnitCount}");

        unit.name = $"{myTeam}RangeUnit";
    }


    // 유닛 이미지 생성
    private void CreateUnitImages(
        List<RogueUnitDataBase> units,
        Vector3[] positions,
        float firstSize, float secondSize,
        bool isMyUnit, float dodge)
    {
        if (units == null)
            return;

        for (int i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            if (unit == null || unit.health <= 0) continue;

            string unitTeam = isMyUnit ? "My" : "Enemy";
            Transform backParent = isMyUnit ? myBackUnitsParent : enemyBackUnitsParent;
            Transform targetParent = i < 2 ? canvasTransform : backParent;

            GameObject unitImage = objectPool.GetBattleUnit();
            unitImage.transform.SetParent(targetParent, false);
            unitImage.transform.localScale = isMyUnit ? new(1, 1, 1) : new(-1, 1, 1);

            RestoreGraphicRoot(unitImage);

            Transform childUnit = unitImage.transform.GetChild(0);
            Image unitFrame = childUnit.GetComponent<Image>();
            unitFrame.sprite = SpriteCacheManager.GetFrameByRarity(unit.rarity);
            RectTransform rectTransform = unitImage.GetComponent<RectTransform>();
            RectTransform frameRect = unitFrame.rectTransform;

            float unitSize = (i == 0) ? firstSize : secondSize;
            float frameSize = unitSize * (unit.rarity == 4 ? 1.185f : 1.17f);

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, unitSize);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, unitSize);

            frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, frameSize);
            frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, frameSize);

            if (i < positions.Length)
            {
                rectTransform.anchoredPosition = positions[i];
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }

            Image img = unitImage.GetComponent<Image>();
            img.sprite = SpriteCacheManager.GetSprite($"UnitImages/Unit_Img_{unit.idx}");

            unitImage.name = $"{(isMyUnit ? "My" : "Enemy")}Unit{i}";
            RegisterUnitView(unit, unitImage);
        }

        if (isMyUnit)
        {
            if (_myDodge != null)
            {
                bool hasFront = units.Count > 0 && units[0] != null && units[0].health > 0;
                _myDodge.gameObject.SetActive(hasFront);
                if (hasFront) _myDodge.text = $"회피율: {dodge}%";
            }
        }
        else
        {
            if (_enemyDodge != null)
            {
                bool hasFront = units.Count > 0 && units[0] != null && units[0].health > 0;
                _enemyDodge.gameObject.SetActive(hasFront);
                if (hasFront) _enemyDodge.text = $"회피율: {dodge}%";
            }
        }
    }

    /// <summary>
    /// 유닛 인덱스로 UnitCardUI RectTransform 가져오기 (이펙트 재생용)
    /// </summary>
    public RectTransform GetUnitCardTransform(int unitIndex, bool isMyUnit)
    {
        string unitName = $"{(isMyUnit ? "My" : "Enemy")}Unit{unitIndex}";
        GameObject unitCard = GameObject.Find(unitName);

        if (unitCard != null)
        {
            return unitCard.GetComponent<RectTransform>();
        }

        Debug.LogWarning($"[AutoBattleUI] 유닛 카드 UI를 찾을 수 없습니다: {unitName}");
        return null;
    }
    private void CreateAbilityIcons(RogueUnitDataBase unit, bool isTeam)
    {
        if (unit == null)
            return;

        Transform abilityBox = isTeam ? myAbilityBox : enemyAbilityBox;
        if (abilityBox == null)
            return;

        RestoreGraphicRoot(abilityBox.gameObject);

        var fields = unit.GetType().GetFields();

        for (int i = 0; i < fields.Length; i++)
        {
            var f = fields[i];
            if (f.FieldType != typeof(bool))
                continue;

            bool hasTrait = (bool)f.GetValue(unit);
            if (!hasTrait)
                continue;

            string abilityKey = f.Name;

            if (abilityKey == "rangedAttack" || abilityKey == "alive" || abilityKey == "fStriked")
                continue;

            int abilityIdx = AbilityIdMap.GetIdx(abilityKey);

            if (abilityIdx < 0)
            {
                if (int.TryParse(abilityKey, out int parsedId))
                {
                    abilityIdx = parsedId;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"[CreateAbilityIcons] abilityId 매핑 실패: {abilityKey}");
#endif
                    continue;
                }
            }

            Sprite sprite = SpriteCacheManager.GetSprite($"KIcon/AbilityIcon/{abilityKey}");
            if (sprite == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[CreateAbilityIcons] 아이콘 스프라이트 없음: {abilityKey}");
#endif
                continue;
            }

            GameObject iconGO = objectPool.GetAbility();
            RestoreGraphicRoot(iconGO);

            Image img = iconGO.GetComponent<Image>();
            if (img == null)
            {
                objectPool.ReturnAbility(iconGO);
                continue;
            }

            img.sprite = sprite;

            ItemInformation itemInfo = iconGO.GetComponent<ItemInformation>();
            if (itemInfo != null)
            {
                itemInfo.data.isItem = false;
                itemInfo.data.abilityId = abilityIdx;
            }

            ExplainItem explainItem = iconGO.GetComponent<ExplainItem>();
            if (explainItem != null)
            {
                explainItem.ItemToolTip = itemToolTip;
            }

            iconGO.transform.SetParent(abilityBox, false);
        }
    }

    private void ClearExistingAbilityIcons()
    {
        foreach (var unit in objectPool.GetActiveAbilitys())
        {
            objectPool.ReturnAbility(unit);
        }
    }

    //전투 종료
    public void FightEnd()
    {
        if (rewardUI == null)
        {
            rewardUI = GameManager.Instance.rewardUI;
        }

        rewardUI.AnimateBattleEnd();
    }
    //true 승리, false 패배
    public void GameEnd(bool isWin)
    {
        rewardUI.StartGameOverSequence(isWin);
    }
    //능력 창 띄위기
    public void CreateAbility(string ability, bool myTeam)
    {
        List<string> abilityList = new List<string>(ability.Split(' '));

        int team = myTeam ? 0 : 5;

        if (abilityList.Count > 0)
        {
            //유격 착취 제외하고
            for (int i = 0; i < abilityList.Count - 1; i++)
            {
                // 첫 번째 능력이 "유격" 또는 "착취"인 경우 team 값을 반전
                if (abilityList[0] == "유격" || abilityList[0] == "착취")
                {
                    team = (team == 0) ? 5 : 0;
                }
                Transform child = abilityPool.transform.GetChild(i + team);
                child.gameObject.SetActive(true);
                child.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = abilityList[i];
            }

        }
    }

    //유닛 사망 시 투명하게
    public void ChangeInvisibleUnit(RogueUnitDataBase unit, int unitIndex, bool isMyUnit)
    {
        if (unit == null)
            return;

        if (!TryGetUnitView(unit, out GameObject unitView))
            return;

        RemoveUnitView(unit);
        FadeOutUnit(unitView, unitIndex, isMyUnit);
    }
    // 생성된 유닛 검색
    private GameObject FindUnit(int unitIndex, bool isMyUnit)
    {
        string unitName = $"{(isMyUnit ? "MyUnit" : "EnemyUnit")}{unitIndex}";
        Transform parent = unitIndex < 2 ? canvasTransform : (isMyUnit ? myBackUnitsParent : enemyBackUnitsParent);

        foreach (Transform child in parent)
        {
            if (child.name == unitName && child.gameObject.activeSelf)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    //유닛 투명
    private void FadeOutUnit(GameObject unit, int unitIndex, bool isMyUnit)
    {
        if (unit == null)
            return;

        float duration = waittingTime * 0.001f;

        FadeOutGraphicRoot(unit, duration);
        FadeOutLinkedUi(unitIndex, isMyUnit, duration);
    }

    //대기 시간 변경
    public void ChangeWaittingTime(float multiple)
    {
        waittingTime *= multiple;

    }

    //유산 생성
    public void CreateWarRelic()
    {
        //WarRelicBoxUI.SetRelicBox(relicBox, itemToolTip, objectPool);
    }

    public void ApplyHp(in HpViewData d)
    {
        // 1번 체력바 갱신
        UpdateUnitHPUI(d.MyHp, d.EnemyHp, d.MyMax, d.EnemyMax);

        // 2번 체력바 갱신/토글
        ToggleAndSet(mySecondHpBar, _mySecondHpText, d.MySecondActive, d.MySecondHp, d.MySecondMax);
        ToggleAndSet(enemySecondHpBar, _enemySecondHpText, d.EnemySecondActive, d.EnemySecondHp, d.EnemySecondMax);
    }
    // 사용처: 체력 갱신 타이밍에 버프/디버프 아이콘도 함께 갱신
    public void ApplyHp(in HpViewData d, List<RogueUnitDataBase> myUnits, List<RogueUnitDataBase> enemyUnits)
    {
        ApplyHp(d);
        UpdateBuffDeBuffUI(myUnits, enemyUnits);
    }
    // 체력바/텍스트를 토글하고 값 세팅
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ToggleAndSet(Slider bar, TMP_Text txt, bool active, int hp, int max)
    {
        if (bar == null) return;

        var barGo = bar.gameObject;
        if (barGo.activeSelf != active) barGo.SetActive(active);

        if (txt != null)
        {
            var txtGo = txt.gameObject;
            if (txtGo.activeSelf != active) txtGo.SetActive(active);
        }

        if (active)
        {
            bar.maxValue = max;
            bar.value = hp;
            if (txt != null) txt.text = $"{hp}/{max}";
        }
    }

    // 초기화: 두 번째 체력바/텍스트 비활성
    private void ResetUIActive()
    {
        if (rewardUI != null) rewardUI.InitializeAsIdle();
        if (mySecondHpBar != null) mySecondHpBar.gameObject.SetActive(false);
        if (enemySecondHpBar != null) enemySecondHpBar.gameObject.SetActive(false);
    }

    //사기 값 수정
    public void UpdateMorale()
    {
        int morale = RogueLikeData.Instance.GetMorale();
        moraleText.text = $"{morale}";
    }
    public void OpenGoTestBtn()
    {
        goTestBtn.SetActive(true);
    }
    public void UpdateSecondHPUI(bool myActive, float myHp, float myMax, bool enemyActive, float enemyHp, float enemyMax)
    {
        // 내 2번 유닛
        if (mySecondHpBar != null)
        {
            var barGo = mySecondHpBar.gameObject;
            if (barGo.activeSelf != myActive) barGo.SetActive(myActive);

            if (_mySecondHpText != null)
            {
                var txtGo = _mySecondHpText.gameObject;
                if (txtGo.activeSelf != myActive) txtGo.SetActive(myActive);
            }

            if (myActive)
            {
                mySecondHpBar.maxValue = myMax;
                mySecondHpBar.value = myHp;
                if (_mySecondHpText != null) _mySecondHpText.text = $"{myHp}/{myMax}";
            }
        }

        // 적 2번 유닛
        if (enemySecondHpBar != null)
        {
            var barGo = enemySecondHpBar.gameObject;
            if (barGo.activeSelf != enemyActive) barGo.SetActive(enemyActive);

            if (_enemySecondHpText != null)
            {
                var txtGo = _enemySecondHpText.gameObject;
                if (txtGo.activeSelf != enemyActive) txtGo.SetActive(enemyActive);
            }

            if (enemyActive)
            {
                enemySecondHpBar.maxValue = enemyMax;
                enemySecondHpBar.value = enemyHp;
                if (_enemySecondHpText != null) _enemySecondHpText.text = $"{enemyHp}/{enemyMax}";
            }
        }
    }

    // 사용처: GetDamageAnchor에서 월드->로컬 변환에 필요한 캔버스/카메라 캐시
    private void EnsureDamageAnchorCache()
    {
        if (damageAnchorCacheReady) return;

        canvasRt = canvasTransform as RectTransform;
        rootCanvas = (canvasTransform != null) ? canvasTransform.GetComponentInParent<Canvas>() : null;

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCam = rootCanvas.worldCamera;
        else
            uiCam = null;

        damageAnchorCacheReady = (canvasRt != null);
    }

    // 사용처: 현재 전투 중인 양측 유닛들의 버프/디버프 아이콘을 갱신
    public void UpdateBuffDeBuffUI(List<RogueUnitDataBase> myUnits, List<RogueUnitDataBase> enemyUnits)
    {
        ClearExistingBuffDeBuffIcons();

        CreateBuffDeBuffIcons(myUnits, true);
        CreateBuffDeBuffIcons(enemyUnits, false);
    }

    // 사용처: 버프/디버프 아이콘을 전부 풀로 반환
    private void ClearExistingBuffDeBuffIcons()
    {
        if (objectPool != null)
            objectPool.ClearActiveBuffDeBuffs();
    }

    // 사용처: 한 진영에 걸린 표시 대상 버프/디버프를 아이콘으로 생성
    private void CreateBuffDeBuffIcons(List<RogueUnitDataBase> units, bool isMyUnit)
    {
        if (units == null || objectPool == null)
            return;

        GameObject parentObj = isMyUnit ? myBuffDeBuff : enemyBuffDeBuff;
        if (parentObj == null)
            return;

        Transform parent = parentObj.transform;

        for (int i = 0; i < visibleBuffDeBuffIds.Length; i++)
        {
            int id = visibleBuffDeBuffIds[i];

            if (!TryGetVisibleBuffDeBuff(units, id, out int grade, out int duration))
                continue;

            GameObject iconGO = objectPool.GetBuffDeBuff();
            if (iconGO == null)
                continue;

            iconGO.transform.SetParent(parent, false);
            iconGO.transform.localScale = Vector3.one;

            RectTransform rt = iconGO.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buffDeBuffIconSize);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buffDeBuffIconSize);
            }

            Image img = iconGO.GetComponent<Image>();
            if (img == null)
            {
                objectPool.ReturnBuffDeBuff(iconGO);
                continue;
            }

            string spritePath = GetBuffDeBuffSpritePath(id, grade);
            Sprite sprite = SpriteCacheManager.GetSprite(spritePath);
            if (sprite == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[AutoBattleUI] 버프/디버프 아이콘 없음: {spritePath}");
#endif
                objectPool.ReturnBuffDeBuff(iconGO);
                continue;
            }

            img.sprite = sprite;

            string title = GetBuffDeBuffTitle(id, grade);
            string description = GetBuffDeBuffDescription(id, grade, duration);

            ItemInformation itemInfo = iconGO.GetComponent<ItemInformation>();
            if (itemInfo != null)
                itemInfo.SetBuffDeBuff(id, grade, duration, title, description);

            ExplainItem explainItem = iconGO.GetComponent<ExplainItem>();
            if (explainItem != null)
                explainItem.ItemToolTip = itemToolTip;
        }
    }

    // 사용처: 한 진영에 표시 가능한 특정 버프/디버프가 있는지 확인
    private bool TryGetVisibleBuffDeBuff(List<RogueUnitDataBase> units, int id, out int grade, out int duration)
    {
        grade = 0;
        duration = 0;

        bool found = false;

        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit == null || unit.health <= 0 || unit.effectDictionary == null)
                continue;

            if (!unit.effectDictionary.TryGetValue(id, out BuffDebuffData effect))
                continue;

            if (effect.Duration == 0)
                continue;

            found = true;

            if (effect.EffectGrade > grade)
                grade = effect.EffectGrade;

            if (effect.Duration < 0)
            {
                duration = -1;
            }
            else if (duration >= 0 && effect.Duration > duration)
            {
                duration = effect.Duration;
            }
        }

        if (!found)
            return false;

        if (grade <= 0)
            grade = 1;

        return true;
    }

    // 사용처: 버프/디버프 id와 단계에 맞는 Resources 스프라이트 경로 반환
    private static string GetBuffDeBuffSpritePath(int id, int grade)
    {
        switch (id)
        {
            case 0:
                return $"KIcon/BuffDeBuff/DeBuff_Burn{Mathf.Clamp(grade, 1, 3)}";
            case 1:
                return "KIcon/BuffDeBuff/DeBuff_Scar";
            case 2:
                return "KIcon/BuffDeBuff/Buff_Smoke";
            case 3:
                return "KIcon/BuffDeBuff/DeBuff_Track";
            case 4:
                return "KIcon/BuffDeBuff/Buff_Seven";
            case 8:
                return "KIcon/BuffDeBuff/DeBuff_Over";
            default:
                return string.Empty;
        }
    }

    // 사용처: 버프/디버프 아이콘 툴팁 제목을 GameTextDB에서 가져옴
    private static string GetBuffDeBuffTitle(int id, int grade)
    {
        string title = GameTextDB.GetByForeignKey(TextKind.BuffDeBuff, id);

        if (string.IsNullOrEmpty(title))
            title = GetFallbackBuffDeBuffTitle(id);

        if (id == 0)
            return $"{title} {Mathf.Clamp(grade, 1, 3)}단계";

        return title;
    }

    // 사용처: 버프/디버프 아이콘 툴팁 설명을 생성
    // 현재는 GameTextData.json에 BuffDeBuff 설명 번역이 완전히 준비되지 않았으므로 fallback 설명을 사용함
    // 추후 JSON에 BuffDeBuff 설명 데이터가 추가되면 GetBuffDeBuffDescriptionFromDB()가 먼저 적용됨
    private static string GetBuffDeBuffDescription(int id, int grade, int duration)
    {
        string title = GameTextDB.GetByForeignKey(TextKind.BuffDeBuff, id);
        string description = GetBuffDeBuffDescriptionFromDB(id, title);

        if (string.IsNullOrEmpty(description))
            description = GetFallbackBuffDeBuffDescription(id);

        string durationText = duration < 0 ? "지속" : $"{duration}턴";

        if (id == 0)
            return $"{description}\n현재 단계: {Mathf.Clamp(grade, 1, 3)} / 남은 시간: {durationText}";

        return $"{description}\n남은 시간: {durationText}";
    }

    // 사용처: GameTextDB에 BuffDeBuff 설명 데이터가 추가되었을 때 해당 설명을 가져옴
    // 설명 데이터가 없거나 제목과 같은 값이 반환되면 빈 문자열을 반환해서 fallback 설명을 사용하게 함
    private static string GetBuffDeBuffDescriptionFromDB(int id, string title)
    {
        int titleIdx = GameTextDB.GetIdxByForeignKey(TextKind.BuffDeBuff, id);

        if (titleIdx < 0)
            return string.Empty;

        string description = GameTextDB.Get(TextKind.BuffDeBuff, titleIdx, id);

        if (string.IsNullOrEmpty(description))
            return string.Empty;

        if (!string.IsNullOrEmpty(title) && description == title)
            return string.Empty;

        return description;
    }

    // 사용처: GameTextDB에 버프/디버프 제목 데이터가 없을 때 임시 제목을 반환
    private static string GetFallbackBuffDeBuffTitle(int id)
    {
        switch (id)
        {
            case 0:
                return "작열";
            case 1:
                return "상흔";
            case 2:
                return "연막";
            case 3:
                return "추적자 표식";
            case 4:
                return "제국 시너지";
            case 8:
                return "위압";
            default:
                return "알 수 없는 효과";
        }
    }

    // 사용처: GameTextData.json에 BuffDeBuff 설명 번역이 추가되기 전까지 임시 설명을 제공
    // 추후 JSON에 설명 데이터가 추가되면 GetBuffDeBuffDescriptionFromDB()가 먼저 값을 반환하므로 이 함수는 예비 처리만 담당
    private static string GetFallbackBuffDeBuffDescription(int id)
    {
        switch (id)
        {
            case 0:
                return "매 턴 최대 체력에 비례한 피해를 받습니다. 최대 3단계까지 중첩됩니다.";
            case 1:
                return "치유 효과를 받을 수 없습니다.";
            case 2:
                return "회피율이 증가합니다.";
            case 3:
                return "추적자에게 피해를 받아 장갑이 감소한 상태입니다.";
            case 4:
                return "제국 시너지 효과로 회피율이 증가합니다.";
            case 8:
                return "기동력이 크게 낮아진 상태로 취급됩니다.";
            default:
                return "효과 정보가 아직 등록되지 않았습니다.";
        }
    }
}

