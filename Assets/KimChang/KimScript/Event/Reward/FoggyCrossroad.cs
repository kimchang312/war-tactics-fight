using static EventManager;

public class FoggyCrossroad : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int morale = RogueLikeData.Instance.GetMorale();

        if (morale < 25)
            return "사기가 부족해 안개의 길을 제대로 인식할 수 없습니다.";

        switch (choice)
        {
            case 0: // 힘의 길 → 전직 텍스트 + 사기 -5 + 반복
                RogueLikeData.Instance.SetMorale(morale - 5);
                return $"[텍스트 처리] '{unit.unitName}'이 전직의 길을 걸었습니다. (사기 -5)\n[이벤트 반복] 다시 갈림길이 나타납니다...";

            case 1: // 재물의 길 → 금화(소), 사기 -5 + 반복
                int gold = UnityEngine.Random.Range(50, 101);
                RogueLikeData.Instance.AddGoldByEventChapter(gold);
                RogueLikeData.Instance.SetMorale(morale - 5);
                return $"금화 {gold}를 얻었지만 마음이 무거워졌습니다. (사기 -5)\n[이벤트 반복] 안개는 아직 걷히지 않았습니다...";

            default: // 되돌아감 → 사기 -5 + 종료
                RogueLikeData.Instance.SetMorale(morale - 5);
                return "사기를 잃으며 길을 되돌아갔습니다. 더 이상 갈림길은 나타나지 않습니다. (사기 -5)";
        }
    }
}
