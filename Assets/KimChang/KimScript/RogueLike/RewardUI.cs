using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class RewardUI : MonoBehaviour
{
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

    [SerializeField] private GameObject itemToolTip;
    SaveData saveData = new SaveData();

    private void OnEnable()
    {
        ResetUI();
        transform.SetAsLastSibling();
    }

    public void CreateTeasureUI()
    {
        backFrame.SetActive(true);
        BattleRewardData reward = new();
        
        int gold, relicGrade;
        (gold, relicGrade) = RewardManager.GetRewardTeasure();
        reward.gold = gold;
        reward.relicGrade.Add(relicGrade);

        resultText.text = "전리품";
        goldResult.GetComponentInChildren<TextMeshProUGUI>().text = $"  금화 {gold}";
        RogueLikeData.Instance.EarnGold(gold);

        relicResult.onClick.AddListener(()=>OpenReward(false));
        goldResult.SetActive(true);
        relicResult.gameObject.SetActive(true);

        leaveBtn.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void CreateRewardUI()
    {
        backFrame.SetActive(true);

        BattleRewardData reward = RogueLikeData.Instance.GetBattleReward();
        bool isGameOver = RewardManager.CheckGameOver();
        if (isGameOver) reward.battleResult = 5;
        moraleResult.SetActive(true);
        moraleResult.GetComponentInChildren<TextMeshProUGUI>().text = $"  사기 {reward.morale}";
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
                resultText.text = "정복";
                moraleResult.GetComponentInChildren<TextMeshProUGUI>().text = $"점수: {RogueLikeData.Instance.GetScore()}";
                goldResult.GetComponentInChildren<TextMeshProUGUI>().text = "  타이틀 화면으로";
                goldResult.AddComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene("Title"));
                goldResult.transform.SetAsLastSibling();
                leaveBtn.gameObject.SetActive(false);
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

            RogueLikeData.Instance.AddRerollChange(reward.rerollChance);
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

        RogueLikeData.Instance.EarnGold(reward.gold);
    }

    private void ResetUI()
    {
        backFrame.SetActive(false);
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

        foreach (Transform child in selectRewards.transform)
            child.gameObject.SetActive(false);

        rerollBtn.onClick.RemoveAllListeners();
        skipBtn.onClick.RemoveAllListeners();
        rerollBtn.onClick.AddListener(RerollReward);
        skipBtn.onClick.AddListener(SkipSelectReward);
        unitSelectUI.gameObject.SetActive(false);
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
            backFrame.SetActive(true);
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
        backFrame.SetActive(false);
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
            if (info.relicId == 79)
            {
                var myUnits = RogueLikeData.Instance.GetMyTeam();
                var randomUnit = myUnits[UnityEngine.Random.Range(0, myUnits.Count)];
                randomUnit.endless = true;
            }
        }
        itemToolTip.SetActive(false);
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
        //if (info.isItem) return;       
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
            backFrame.SetActive(true);
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
        btn.GetComponent<Image>().sprite = SpriteCacheManager.GetSprite($"UnitImages/{unit.unitImg}");
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
        saveData.ResetGameData();
        GameManager.Instance.uIGenerator.RegenerateMap();
        SceneManager.LoadScene("RLmap");
    }
    private void ClickGoTitleBtn()
    {
        saveData.ResetGameData();
        GameManager.Instance.uIGenerator.RegenerateMap();
        SceneManager.LoadScene("Title");
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "RLmap")
        {

            if (RogueLikeData.Instance.GetClearChpater())
            {
                RogueLikeData.Instance.SetClearChapter(false);
                Debug.Log("보스클");
                GameManager.Instance.uIGenerator.RegenerateMap();
            }
            
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}