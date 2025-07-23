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
    [SerializeField] private GameObject endingWindow;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject itemToolTip;
    [SerializeField] private Image teasureBox;
    [SerializeField] private Button teasureBtn;

    [SerializeField] private GameObject moraleFlag;
    SaveData saveData = new SaveData();

    private bool isEnd=false;
    private void OnEnable()
    {
        ResetUI();

        transform.SetAsLastSibling();
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
    public void SetActiveTeasureBox()
    {
        teasureBox.gameObject.SetActive(true);
        CloseTeasureBox();
        teasureBtn.onClick.RemoveAllListeners();
        teasureBtn.onClick.AddListener(CreateTeasureUI);
    }
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
        relicResult.onClick.AddListener(()=>OpenReward(false));
        goldResult.SetActive(true);
        relicResult.gameObject.SetActive(true);

        leaveBtn.onClick.AddListener(LeaveReward);
    }
    public void AnimateBattleEnd()
    {
        //0,0 위치에서 시작해서 활성화 시키고 모든 dotween을 종료시키고 0.2초간 0,0위치에 있다가 moraleFlag UI가 0.8초간 -490,480 위치로 이동후 비활성화
        DisableBackground();
        teasureBox.gameObject.SetActive(false);
        //AbleRewardWindow();

        AnimateMoraleFlag();
    }

    public void CreateRewardUI()
    {
        //DisableBackground();
        //teasureBox.gameObject.SetActive(false);
        AbleRewardWindow();

        BattleRewardData reward = RogueLikeData.Instance.GetBattleReward();
        bool isGameOver = RewardManager.CheckGameOver();
        if (isGameOver) reward.battleResult = 5;
       

        //moraleResult.SetActive(true);
        //moraleResult.GetComponentInChildren<TextMeshProUGUI>().text = $"  사기 {reward.morale}";
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
            if(chapter==3 && type== StageType.Boss)
            {
                endingWindow.SetActive(true);
                isEnd =true;
                scoreText.text = $"점수: {RogueLikeData.Instance.GetScore()}";
            }

            if (HasUnitReward(reward))
            {
                unitResult.onClick.AddListener(() => OpenReward(true));
                unitResult.gameObject.SetActive(true);
            }

            if (HasRelicReward(reward))
            {
                relicResult.onClick.AddListener(() => OpenReward(false));
                relicResult.gameObject.SetActive(true);
            }

            RogueLikeData.Instance.AddReroll(reward.rerollChance);
        }
        else if (isGameOver)
        {
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
        DisableRewardWindow();
        goldResult.SetActive(false);
        moraleResult.SetActive(false);
        unitResult.onClick.RemoveAllListeners();
        unitResult.gameObject.SetActive(false);
        relicResult.onClick.RemoveAllListeners();
        relicResult.gameObject.SetActive(false);
        leaveBtn.onClick.AddListener(LeaveReward);
        rewardSelectObj.SetActive(false);
        retryBtn.gameObject.SetActive(false);
        goTitleBtn.gameObject.SetActive(false);
        endingWindow?.SetActive(false);
        foreach (Transform child in selectRewards.transform)
            child.gameObject.SetActive(false);

        rerollBtn.onClick.RemoveAllListeners();
        skipBtn.onClick.RemoveAllListeners();
        rerollBtn.onClick.AddListener(RerollReward);
        skipBtn.onClick.AddListener(SkipSelectReward);
        unitSelectUI.gameObject.SetActive(false);
        moraleFlag.SetActive(false);
    }

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

        int reroll = RogueLikeData.Instance.GetRerollChance();
        var countText = rerollBtn.GetComponentInChildren<TextMeshProUGUI>();
        if(reroll < 1)
        {
            rerollBtn.interactable = false;
        }
        else
        {
            rerollBtn.interactable = true;
        }
        countText.text = $"{reroll}";

        rewardSelectObj.SetActive(true);
        rerollBtn.gameObject.SetActive(true);
        skipBtn.gameObject.SetActive(true);
        DisableRewardWindow();
    }


    private void ClickReward(ItemInformation info)
    {
        if (info.isItem) return;
        BattleRewardData reward = RogueLikeData.Instance.GetBattleReward();
        if (info.type == RewardType.UnitGrade || info.type == RewardType.NewUnit || info.type == RewardType.ChangeUnit)
        {
            RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(info.unitId);
            RogueLikeData.Instance.AddMyUnis(unit);
        }
        else if (info.type == RewardType.RelicGrade || info.type == RewardType.NewRelic)
        {
            RogueLikeData.Instance.AcquireRelic(info.relicId);
            RewardManager.AcquireReward();
            if (info.relicId == 79)
            {
                unitSelectUI.gameObject.SetActive(true);
                unitSelectUI.OpenSelectUnitWindow(SelectUnitEndless, null, 1);
                return;
            }
        }
        itemToolTip.SetActive(false);
        SkipSelectReward();
    }

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
    private void RerollReward()
    {
        int reroll = RogueLikeData.Instance.GetRerollChance();

        if (reroll > 0)
        {
            var countText = rerollBtn.GetComponentInChildren<TextMeshProUGUI>();
            countText.text = $"{reroll}";
            var info = selectRewards.transform.GetChild(0).GetComponent<ItemInformation>();
            OpenReward(info.unitId > -1);
            RogueLikeData.Instance.SetRerollChance(--reroll);
            countText.text = $"{reroll}";
            rerollBtn.interactable = true;
            return;
        }
        rerollBtn.interactable = false;
    }

    //넘기기 or 보상 받으면 해당 보상 리스트에서 해당 보상 제거 및 새로운 보상 보여주기
    private void SkipSelectReward()
    {
        BattleRewardData reward = RogueLikeData.Instance.GetBattleReward();
        var info = selectRewards.transform.GetChild(0).GetComponent<ItemInformation>();
        switch (info.type)
        {
            case RewardType.UnitGrade: reward.unitGrade.RemoveAt(0); break;
            case RewardType.NewUnit: reward.newUnits.RemoveAt(0); break;
            case RewardType.ChangeUnit: reward.changedUnits.RemoveAt(0); break;
            case RewardType.RelicGrade: reward.relicGrade.RemoveAt(0); break;
            case RewardType.NewRelic: reward.relicIds.RemoveAt(0); break;
        }
        if (HasUnitReward(reward)){
            OpenReward(true);
        }
        else if (HasRelicReward(reward)) {
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

    private ItemInformation CreateUnit(Button btn, RogueUnitDataBase unit, RewardType type)
    {
        btn.GetComponent<Image>().sprite = SpriteCacheManager.GetSprite($"UnitImages/Unit_Img_{unit.idx}");
        btn.GetComponentInChildren<TextMeshProUGUI>().text = unit.unitName;

        var info = btn.GetComponent<ItemInformation>();
        info.unitId = unit.idx;
        info.relicId = -1;
        info.type = type;
        info.isItem = false;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ClickReward(info));
        btn.gameObject.SetActive(true);
        return info;
    }

    private ItemInformation CreateRelic(Button btn, WarRelic relic, RewardType type)
    {
        btn.GetComponent<Image>().sprite = SpriteCacheManager.GetSprite($"KIcon/WarRelic/{relic.id}");
        btn.GetComponentInChildren<TextMeshProUGUI>().text = relic.name;

        var info = btn.GetComponent<ItemInformation>();
        info.relicId = relic.id;
        info.unitId = -1;
        info.type = type;
        info.isItem = false;
        ExplainItem exItem = btn.GetComponent<ExplainItem>();
        exItem.ItemToolTip = itemToolTip;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ClickReward(info));
        btn.gameObject.SetActive(true);
        return info;
    }

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
    private void ClickRetryBtn()
    {
        RogueLikeData.Instance.ClearBattleReward();
        saveData.ResetGameData();
        RogueLikeData.Instance.SetResetMap(true);
        SceneManager.LoadScene("RLmap");
    }
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
    private void CloseTeasureBox()
    {
        teasureBox.sprite = SpriteCacheManager.GetSprite("KIcon/TeasureClose");
    }
    private void OpenTeasureBox()
    {
        teasureBox.sprite = SpriteCacheManager.GetSprite("KIcon/TeasuerOpen");
    }

    private void AbleBackground()
    {
        var color = backgroundImg.color;
        color.a = 1;
        backgroundImg.color = color;
    }
    private void DisableBackground()
    {
        var color = backgroundImg.color;
        color.a = 0.5f;
        backgroundImg.color = color;
    }
    private void AbleRewardWindow()
    {
        backFrame.transform.parent?.gameObject.SetActive(true);
        backFrame.SetActive(true);
    }
    private void DisableRewardWindow()
    {
        backFrame.transform.parent?.gameObject.SetActive(false);
        backFrame.SetActive(false);
    }

    public void AnimateMoraleFlag(int morale =10)
    {
        RectTransform rect = moraleFlag.GetComponent<RectTransform>();
        moraleFlag.GetComponentInChildren<TextMeshProUGUI>().text = morale>0?$"+ {morale}":$"- {morale}";

        rect.anchoredPosition = Vector2.zero;
        moraleFlag.SetActive(true);

        DOTween.Kill(rect);

        Sequence seq = DOTween.Sequence();

        // 0.4초간 0,0 위치 유지
        seq.AppendInterval(0.4f);

        // 0.8초간 (-490, 480)으로 이동 추후에는 상단 깃발 위치에 접근해서 설정해야함
        seq.Append(rect.DOAnchorPos(new Vector2(-466f, 580f), 0.6f).SetEase(Ease.InOutSine));

        // 이동 완료 후 비활성화
        seq.OnComplete(() => { 
            moraleFlag.SetActive(false);
            CreateRewardUI();
        });
    }


}