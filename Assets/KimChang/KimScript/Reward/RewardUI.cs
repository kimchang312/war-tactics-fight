using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;

public class RewardUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImg;
    [SerializeField] private GameObject backFrame;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private GameObject goldResult;
    [SerializeField] private GameObject moraleResult;
    [SerializeField] private Button unitResult;
    [SerializeField] private Button relicResult;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button goTitleBtn;
    [SerializeField] private Button leaveBtn;
    [SerializeField] private GameObject rewardSelectObj;
    [SerializeField] private TextMeshProUGUI selectText;
    [SerializeField] private GameObject selectRewards;
    [SerializeField] private Button rerollBtn;
    [SerializeField] private Button skipBtn;
    [SerializeField] private UnitSelectUI unitSelectUI;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject itemToolTip;
    [SerializeField] private Image teasureBox;
    [SerializeField] private Button teasureBtn;

    [SerializeField] private GameObject stageEndObj;

    // GameOver 루트는 이것 하나만 사용
    [SerializeField] private GameObject gameOverPanel;          // = GameOverPanel (루트)
    [SerializeField] private Image gameOverBackgroundImg;        // = GameOverDark 하위 Image

    [SerializeField] private GameObject winPanel;
    [SerializeField] private Image winImg;
    [SerializeField] private Image winText;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Image loseImg;
    [SerializeField] private Image loseText;
    [SerializeField] private Transform btns;
    [SerializeField] private Button retryGameBtn;
    [SerializeField] private Button goTitleGameBtn;

    [SerializeField] private GameObject moraleFlag;

    [Header("GameOver Timings")]
    [SerializeField] private float blackoutDuration = 3f; // 암전 시간(초)
    [SerializeField] private float revealDuration = 2f; // 승/패+버튼 표시 시간(초)

    SaveData saveData = new SaveData();
    private bool isEnd = false;

    private Graphic[] _btnGraphics;

    private void Awake()
    {
        // 이 함수는 버튼 루트 하위 Graphic을 캐시해 페이드 시 반복 탐색을 방지한다.
        if (btns != null)
            _btnGraphics = btns.GetComponentsInChildren<Graphic>(true);
    }


    private void OnEnable()
    {
        ResetUI();
        
    }
    public void InitializeAsIdle()
    {
        isEnd = false;
        ResetUI();
    }
    private void Update()
    {
        if (!isEnd) return;
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            RogueLikeData.Instance.ClearBattleReward();
            GameManager.Instance.SetCurrentStageNull();
            SceneManager.LoadScene("Title");
        }
    }

    // 이 함수는 보물 상자 UI를 활성화하고 클릭 이벤트를 세팅할 때 사용한다.
    public void SetActiveTeasureBox()
    {
        teasureBox.gameObject.SetActive(true);
        CloseTeasureBox();
        teasureBtn.onClick.RemoveAllListeners();
        teasureBtn.onClick.AddListener(CreateTeasureUI);
    }

    // 이 함수는 보물 상자 클릭 시 보상 UI를 생성할 때 사용한다.
    public void CreateTeasureUI()
    {
        teasureBtn.onClick.RemoveAllListeners();
        AbleBackground();
        OpenTeasureBox();

        AbleRewardWindow();
        BattleRewardData reward = new();

        int gold, relicGrade;
        (gold, relicGrade) = RewardManager.GetRewardTeasure();
        reward.gold = gold;
        reward.relicGrade.Add(relicGrade);

        resultText.text = "전리품";
        goldResult.GetComponentInChildren<TextMeshProUGUI>().text = $"  금화 {gold}";
        RogueLikeData.Instance.EarnGold(gold);
        RogueLikeData.Instance.SetBattleReward(reward);
        relicResult.onClick.AddListener(() => OpenReward(false));
        goldResult.SetActive(true);
        relicResult.gameObject.SetActive(true);

        leaveBtn.onClick.RemoveAllListeners();
        leaveBtn.onClick.AddListener(LeaveReward);
    }

    // 이 함수는 전투 종료 애니메이션(사기 깃발 이동)을 실행할 때 사용한다.
    public void AnimateBattleEnd()
    {
        DisableBackground();
        teasureBox.gameObject.SetActive(false);
        AnimateMoraleFlag();
    }

    public void CreateRewardUI()
    {
        ResetUI();

        SafeSetActive(stageEndObj, true);
        SafeSetActive(gameOverPanel, false);

        AbleRewardWindow();

        BattleRewardData reward = RogueLikeData.Instance.GetBattleReward();
        bool isGameOver = RewardManager.CheckGameOver();
        if (isGameOver) reward.battleResult = 5;

        resultText.text = reward.battleResult switch
        {
            5 => "전멸",
            2 => "무승부",
            1 => "패배",
            _ => resultText.text
        };

        if (reward.battleResult == 0)
        {
            goldResult.SetActive(true);
            goldResult.GetComponentInChildren<TextMeshProUGUI>().text = $"  금화 {reward.gold}";

            int chapter = RogueLikeData.Instance.GetChapter();
            var type = RogueLikeData.Instance.GetCurrentStageType();
            if (chapter == 3 && type == StageType.Boss)
            {
                isEnd = true;
                scoreText.text = $"점수: {RogueLikeData.Instance.GetScore()}";
            }

            if (HasUnitReward(reward))
            {
                unitResult.onClick.RemoveAllListeners();
                unitResult.onClick.AddListener(() => OpenReward(true));
                unitResult.gameObject.SetActive(true);
            }

            if (HasRelicReward(reward))
            {
                relicResult.onClick.RemoveAllListeners();
                relicResult.onClick.AddListener(() => OpenReward(false));
                relicResult.gameObject.SetActive(true);
            }

            RogueLikeData.Instance.AddReroll(reward.rerollChance);
        }
        else if (isGameOver)
        {
            retryBtn.onClick.RemoveAllListeners();
            goTitleBtn.onClick.RemoveAllListeners();
            retryBtn.onClick.AddListener(ClickRetryBtn);
            goTitleBtn.onClick.AddListener(ClickGoTitleBtn);

            retryBtn.gameObject.SetActive(true);
            goTitleBtn.gameObject.SetActive(true);
            leaveBtn.gameObject.SetActive(false);
            return;
        }

        int moraeReward = reward.morale;
        RogueLikeData.Instance.ChangeMorale(moraeReward);
        RogueLikeData.Instance.EarnGold(reward.gold);
    }

    private void ResetUI()
    {
        transform.SetAsLastSibling();
        btns.gameObject.SetActive(false);
        DisableRewardWindow();
        goldResult.SetActive(false);
        moraleResult.SetActive(false);

        unitResult.onClick.RemoveAllListeners();
        unitResult.gameObject.SetActive(false);
        relicResult.onClick.RemoveAllListeners();
        relicResult.gameObject.SetActive(false);

        leaveBtn.onClick.RemoveAllListeners();
        leaveBtn.onClick.AddListener(LeaveReward);

        rewardSelectObj.SetActive(false);
        retryBtn.gameObject.SetActive(false);
        goTitleBtn.gameObject.SetActive(false);

        foreach (Transform child in selectRewards.transform)
            child.gameObject.SetActive(false);

        rerollBtn.onClick.RemoveAllListeners();
        skipBtn.onClick.RemoveAllListeners();
        rerollBtn.onClick.AddListener(RerollReward);
        skipBtn.onClick.AddListener(SkipSelectReward);

        unitSelectUI.gameObject.SetActive(false);
        moraleFlag.SetActive(false);

        // 추가 초기화 (게임 종료 UI 전용)
        SafeSetActive(gameOverPanel, false);
        SafeSetActive(stageEndObj, false);

        SetAlpha(gameOverBackgroundImg, 0f);

        SafeSetActive(winPanel, false);
        SetAlpha(winImg, 0f);
        SetAlpha(winText, 0f);

        SafeSetActive(losePanel, false);
        SetAlpha(loseImg, 0f);
        SetAlpha(loseText, 0f);

        if (_btnGraphics != null)
            SetAlphaMultiple(_btnGraphics, 0f);
        else if (btns != null)
            SetAlphaMultiple(btns.GetComponentsInChildren<Graphic>(true), 0f);
    }

    // 이 함수는 유닛/유물 보상 선택창을 열 때 사용한다.
    private void OpenReward(bool isUnit)
    {
        BattleRewardData reward = RogueLikeData.Instance.GetBattleReward();
        selectText.text = isUnit ? "유닛 선택" : "유산 선택";

        if (isUnit)
        {
            if (reward.unitGrade.Count > 0)
            {
                var units = RewardManager.GetRandomUnitsByGrade(reward.unitGrade[0]);
                for (int i = 0; i < units.Count; i++)
                {
                    Button btn = selectRewards.transform.GetChild(i).GetComponent<Button>();
                    RogueUnitDataBase unit = units[i];
                    CreateUnit(btn, unit, RewardType.UnitGrade);
                }
            }
            else if (reward.newUnits.Count > 0)
            {
                Button btn = selectRewards.transform.GetChild(0).GetComponent<Button>();
                RogueUnitDataBase unit = reward.newUnits[0];
                CreateUnit(btn, unit, RewardType.NewUnit);
            }
            else if (reward.changedUnits.Count > 0)
            {
                Button btn = selectRewards.transform.GetChild(0).GetComponent<Button>();
                RogueUnitDataBase unit = reward.changedUnits[0];
                CreateUnit(btn, unit, RewardType.ChangeUnit);
            }
        }
        else
        {
            if (reward.relicGrade.Count > 0)
            {
                int grade = reward.relicGrade[0];
                var selected = new List<WarRelic>();
                var selectedIds = new HashSet<int>();

                int attempts = 0;
                while (selected.Count < 3 && attempts++ < 100)
                {
                    int id = RelicManager.GetRandomRelicId(grade, RelicManager.RelicAction.Acquire);
                    if (id == -1 || selectedIds.Contains(id)) continue;

                    var relic = WarRelicDatabase.GetRelicById(id);
                    if (relic != null)
                    {
                        selected.Add(relic);
                        selectedIds.Add(id);
                    }
                }
                for (int i = 0; i < selected.Count; i++)
                {
                    Button btn = selectRewards.transform.GetChild(i).GetComponent<Button>();
                    WarRelic relic = selected[i];
                    CreateRelic(btn, relic, RewardType.RelicGrade);
                }
            }
            else if (reward.relicIds.Count > 0)
            {
                Button btn = selectRewards.transform.GetChild(0).GetComponent<Button>();
                var relic = WarRelicDatabase.GetRelicById(reward.relicIds[0]);
                CreateRelic(btn, relic, RewardType.NewRelic);
            }
        }

        if (!HasUnitReward(reward) && !HasRelicReward(reward))
        {
            AbleRewardWindow();
            rewardSelectObj.SetActive(false);
            unitResult.gameObject.SetActive(false);
            relicResult.gameObject.SetActive(false);
        }

        (int,bool) reroll = RogueLikeData.Instance.GetRerollChance();
        var countText = rerollBtn.GetComponentInChildren<TextMeshProUGUI>();
        rerollBtn.interactable = (reroll.Item1 > 0 && reroll.Item2);
        
        countText.text = $"{reroll}";

        rewardSelectObj.SetActive(true);
        rerollBtn.gameObject.SetActive(true);
        skipBtn.gameObject.SetActive(true);
        DisableRewardWindow();
    }

    // 이 함수는 보상 버튼 클릭 시 실제 보상 적용을 처리할 때 사용한다.
    private void ClickReward(ItemInformation info)
    {
        if (info.data.isItem) return;
        BattleRewardData reward = RogueLikeData.Instance.GetBattleReward();
        if (info.data.type == RewardType.UnitGrade || info.data.type == RewardType.NewUnit || info.data.type == RewardType.ChangeUnit)
        {
            RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(info.data.unitId);
            RogueLikeData.Instance.AddMyTeam(unit);
        }
        else if (info.data.type == RewardType.RelicGrade || info.data.type == RewardType.NewRelic)
        {
            RogueLikeData.Instance.AcquireRelic(info.data.relicId);
            RewardManager.AcquireReward();
            if (info.data.relicId == 79)
            {
                unitSelectUI.gameObject.SetActive(true);
                unitSelectUI.OpenSelectUnitWindow(SelectUnitEndless, null, 1);
                return;
            }
        }
        itemToolTip.SetActive(false);
        SkipSelectReward();
    }

    // 이 함수는 유물 79 처리 후 선택 완료 시 호출한다.
    private void SelectUnitEndless()
    {
        List<RogueUnitDataBase> units = RogueLikeData.Instance.GetSelectedUnits();
        foreach (var unit in units)
        {
            unit.endless = true;
        }
        RogueLikeData.Instance.ClearSelectedUnis();
        SkipSelectReward();
    }

    // 이 함수는 보상 리롤 버튼을 눌렀을 때 사용한다.
    private void RerollReward()
    {
        var reroll = RogueLikeData.Instance.GetRerollChance();

        if (reroll.Item1 > 0 && reroll.Item2)
        {
            var countText = rerollBtn.GetComponentInChildren<TextMeshProUGUI>();
            countText.text = $"{reroll}";
            var info = selectRewards.transform.GetChild(0).GetComponent<ItemInformation>();
            OpenReward(info.data.unitId > -1);
            RogueLikeData.Instance.AddReroll(-1);
            countText.text = $"{reroll}";
            rerollBtn.interactable = true;
            return;
        }
        rerollBtn.interactable = false;
    }

    // 이 함수는 현재 보상 항목을 넘기거나 수령 후 다음 보상을 노출할 때 사용한다.
    private void SkipSelectReward()
    {
        BattleRewardData reward = RogueLikeData.Instance.GetBattleReward();
        var info = selectRewards.transform.GetChild(0).GetComponent<ItemInformation>();
        switch (info.data.type)
        {
            case RewardType.UnitGrade: reward.unitGrade.RemoveAt(0); break;
            case RewardType.NewUnit: reward.newUnits.RemoveAt(0); break;
            case RewardType.ChangeUnit: reward.changedUnits.RemoveAt(0); break;
            case RewardType.RelicGrade: reward.relicGrade.RemoveAt(0); break;
            case RewardType.NewRelic: reward.relicIds.RemoveAt(0); break;
        }
        if (HasUnitReward(reward))
        {
            OpenReward(true);
        }
        else if (HasRelicReward(reward))
        {
            OpenReward(false);
        }
        else
        {
            AbleRewardWindow();
            rewardSelectObj.SetActive(false);
            unitResult.gameObject.SetActive(false);
            relicResult.gameObject.SetActive(false);
        }
    }

    private static bool HasUnitReward(BattleRewardData r) =>
        r.unitGrade.Count > 0 || r.newUnits.Count > 0 || r.changedUnits.Count > 0;

    private static bool HasRelicReward(BattleRewardData r) =>
        r.relicGrade.Count > 0 || r.relicIds.Count > 0;

    // 이 함수는 유닛 보상 버튼 하나를 구성할 때 사용한다.
    private ItemInformation CreateUnit(Button btn, RogueUnitDataBase unit, RewardType type)
    {
        btn.GetComponent<Image>().sprite = SpriteCacheManager.GetSprite($"UnitImages/Unit_Img_{unit.idx}");
        btn.GetComponentInChildren<TextMeshProUGUI>().text = unit.unitName;

        var info = btn.GetComponent<ItemInformation>();
        info.data.unitId = unit.idx;
        info.data.relicId = -1;
        info.data.type = type;
        info.data.isItem = false;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ClickReward(info));
        btn.gameObject.SetActive(true);
        return info;
    }

    // 이 함수는 유물 보상 버튼 하나를 구성할 때 사용한다.
    private ItemInformation CreateRelic(Button btn, WarRelic relic, RewardType type)
    {
        btn.GetComponent<Image>().sprite = SpriteCacheManager.GetSprite($"KIcon/WarRelic/{relic.id}");
        btn.GetComponentInChildren<TextMeshProUGUI>().text = relic.name;

        var info = btn.GetComponent<ItemInformation>();
        info.data.relicId = relic.id;
        info.data.unitId = -1;
        info.data.type = type;
        info.data.isItem = false;
        ExplainItem exItem = btn.GetComponent<ExplainItem>();
        exItem.ItemToolTip = itemToolTip;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ClickReward(info));
        btn.gameObject.SetActive(true);
        return info;
    }

    // 이 함수는 보상 창 닫기(맵 복귀/타이틀 이동 준비) 시 사용한다.
    private void LeaveReward()
    {
        RogueLikeData.Instance.ClearBattleReward();
        new SaveData().SaveDataFile();
        if (SceneManager.GetActiveScene().name != "RLmap")
        {
            SceneManager.LoadScene("RLmap");
        }
        else
            gameObject.SetActive(false);
        return;
    }

    // 이 함수는 재도전 버튼 클릭 시 사용한다.
    private void ClickRetryBtn()
    {
        RogueLikeData.Instance.ClearBattleReward();
        saveData.ResetGameData();
        RogueLikeData.Instance.SetResetMap(true);
        SceneManager.LoadScene("RLmap");
    }

    // 이 함수는 타이틀로 이동 버튼 클릭 시 사용한다.
    private void ClickGoTitleBtn()
    {
        RogueLikeData.Instance.ClearBattleReward();
        saveData.ResetGameData();
        RogueLikeData.Instance.SetResetMap(true);
        SceneManager.LoadScene("Title");
    }

    private void OnDisable()
    {
        RogueLikeData.Instance.ClearBattleReward();
    }

    // 이 함수는 보물 상자를 닫힌 스프라이트로 바꿀 때 사용한다.
    private void CloseTeasureBox()
    {
        teasureBox.sprite = SpriteCacheManager.GetSprite("KIcon/TeasureClose");
    }

    // 이 함수는 보물 상자를 열린 스프라이트로 바꿀 때 사용한다.
    private void OpenTeasureBox()
    {
        teasureBox.sprite = SpriteCacheManager.GetSprite("KIcon/TeasuerOpen");
    }

    // 이 함수는 보상 UI 배경을 보이게 할 때 사용한다.
    private void AbleBackground()
    {
        var color = backgroundImg.color;
        color.a = 1;
        backgroundImg.color = color;
    }

    // 이 함수는 보상 UI 배경을 흐리게 할 때 사용한다.
    private void DisableBackground()
    {
        var color = backgroundImg.color;
        color.a = 0.5f;
        backgroundImg.color = color;
    }

    // 이 함수는 보상 UI 프레임을 활성화할 때 사용한다.
    private void AbleRewardWindow()
    {
        // 변경 없음: 보상 프레임 켬
        if (backFrame != null)
        {
            var root = backFrame.transform.parent ? backFrame.transform.parent.gameObject : null;
            if (root != null) root.SetActive(true); // stageEndObj라면 켜짐 유지
            backFrame.SetActive(true);
        }
    }

    // 이 함수는 보상 UI 프레임을 비활성화할 때 사용한다.
    private void DisableRewardWindow()
    {
        if (backFrame != null)
        {
            backFrame.SetActive(false);
        }
    }

    // 이 함수는 전투 후 사기 깃발 연출을 재생할 때 사용한다.
    public void AnimateMoraleFlag(int morale = 10)
    {
        RectTransform rect = moraleFlag.GetComponent<RectTransform>();
        moraleFlag.GetComponentInChildren<TextMeshProUGUI>().text = morale > 0 ? $"+ {morale}" : $"- {morale}";

        rect.anchoredPosition = Vector2.zero;
        moraleFlag.SetActive(true);

        DOTween.Kill(rect);

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.4f);
        seq.Append(rect.DOAnchorPos(new Vector2(-466f, 580f), 0.6f).SetEase(Ease.InOutSine));
        seq.OnComplete(() => {
            moraleFlag.SetActive(false);
            CreateRewardUI();
        });
    }

    // 이 함수는 게임 종료 연출(암전→승/패+버튼 표시)을 재생할 때 사용한다.
    public void StartGameOverSequence(bool isWin)
    {
        // 초기화
        ResetUI();

        // 활성 루트 세팅
        SafeSetActive(gameOverPanel, true);
        SafeSetActive(stageEndObj, false);

        // 알파 초기값
        SetAlpha(gameOverBackgroundImg, 0f);
        SetAlpha(winImg, 0f);
        SetAlpha(winText, 0f);
        SetAlpha(loseImg, 0f);
        SetAlpha(loseText, 0f);
        if (_btnGraphics != null) SetAlphaMultiple(_btnGraphics, 0f);

        // 버튼(재도전/타이틀)은 공통 표시
        retryGameBtn.onClick.RemoveAllListeners();
        goTitleGameBtn.onClick.RemoveAllListeners();
        retryGameBtn.onClick.AddListener(ClickRetryBtn);
        goTitleGameBtn.onClick.AddListener(ClickGoTitleBtn);
        btns.gameObject.SetActive(true);
        retryGameBtn.gameObject.SetActive(true);
        goTitleGameBtn.gameObject.SetActive(true);
        leaveBtn.gameObject.SetActive(false);

        // 애니메이션
        DOTween.Kill(gameOverBackgroundImg);
        if (winImg != null) DOTween.Kill(winImg);
        if (winText != null) DOTween.Kill(winText);
        if (loseImg != null) DOTween.Kill(loseImg);
        if (loseText != null) DOTween.Kill(loseText);

        // 1) 암전 3초
        Sequence seq = DOTween.Sequence();
        seq.Append(gameOverBackgroundImg.DOFade(1f, blackoutDuration));

        // 2) 승/패 UI + 버튼 2초 표시
        seq.AppendCallback(() =>
        {
            if (isWin)
            {
                SafeSetActive(winPanel, true);
                SafeSetActive(losePanel, false);
            }
            else
            {
                SafeSetActive(winPanel, false);
                SafeSetActive(losePanel, true);
            }
        });

        if (isWin)
        {
            if (winImg != null) seq.Join(winImg.DOFade(1f, revealDuration));
            if (winText != null) seq.Join(winText.DOFade(1f, revealDuration));
        }
        else
        {
            if (loseImg != null) seq.Join(loseImg.DOFade(1f, revealDuration));
            if (loseText != null) seq.Join(loseText.DOFade(1f, revealDuration));
        }

        if (_btnGraphics == null && btns != null)
            _btnGraphics = btns.GetComponentsInChildren<Graphic>(true);

        if (_btnGraphics != null)
        {
            foreach (var g in _btnGraphics)
            {
                if (g == null) continue;
                DOTween.Kill(g);
                seq.Join(g.DOFade(1f, revealDuration));
            }
        }
    }

    // 안전 활성/비활성 헬퍼
    private static void SafeSetActive(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active) go.SetActive(active);
    }

    // 단일 Graphic 알파 설정 헬퍼
    private static void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        var c = g.color; c.a = a; g.color = c;
    }

    // 여러 Graphic 알파 일괄 설정 헬퍼
    private static void SetAlphaMultiple(Graphic[] graphics, float a)
    {
        if (graphics == null) return;
        for (int i = 0; i < graphics.Length; i++)
        {
            var g = graphics[i];
            if (g == null) continue;
            var c = g.color; c.a = a; g.color = c;
        }
    }
    private void BringToFront()
    {
        // RewardUI 루트와 stageEndObj를 모두 최상단으로 올림
        transform.SetAsLastSibling();
        if (stageEndObj != null) stageEndObj.transform.SetAsLastSibling();
    }

}
