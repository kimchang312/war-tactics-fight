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
    private Vector3 myTeam = new(260, 280, 0);
    private Vector3 enemyTeam = new(-260, 280, 0);

    private float waittingTime = 500f;

    // 사용처: 데미지 텍스트가 "처음부터" 더 위에서 뜨게 하는 스폰 오프셋
    [SerializeField] private float damageTextSpawnYOffset = 100f;

    // 사용처: 뜬 뒤 추가로 위로 올라가는 거리(기존 80f)
    [SerializeField] private float damageTextRise = 80f;

    // 사용처: 좌표 변환(월드 -> 캔버스 로컬) 캐싱
    private RectTransform canvasRt;
    private Canvas rootCanvas;
    private Camera uiCam;
    private bool damageAnchorCacheReady;
    private readonly Dictionary<string, GameObject> unitViewMap = new(32);


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
    public void ShowDamage(float _damage, string text, bool team, bool isAttack, int unitIndex)
    {
        // 사용처: 데미지 표시 색상/문구 판정. 표기는 절댓값, 색은 부호로.
        float offsetX = 50f;
        float damage = -_damage;

        BattleAnimation(damage, text, team, isAttack);

        // team은 "피격 팀"으로 온다. 그대로 그 팀의 유닛을 찾는다.
        Vector2 pos = GetDamageAnchor(unitIndex, team, offsetX);

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

    }

    private void ClearExistingUnitImages()
    {
        unitViewMap.Clear();

        foreach (var unit in objectPool.GetActiveBattleUnits())
        {
            objectPool.ReturnBattleUnit(unit);
        }
    }

    private void CreateRangeUnit(int rangeUnitCount, Vector3 position, GameObject number, bool isMyTeam)
    {
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
        childImg.sprite = SpriteCacheManager.GetSprite($"KIcon/UI_{myTeam}SecondUnit");

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


}

