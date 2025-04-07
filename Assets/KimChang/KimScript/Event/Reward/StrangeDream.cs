using static EventManager;
using System;
using System.Linq;
using Unity.VisualScripting;
using static RelicManager;

public class StrangeDream : IEventRewardHandler
{
    private Random random = new Random();

    public string GetReward(int choice, RogueUnitDataBase unit = null)
    {
        if (choice != 0)
            return "아무 일도 일어나지 않았습니다.";

        int roll = random.Next(9); // 0~8 중 하나 선택

        switch (roll)
        {
            case 0: // 일반 등급 전쟁 유산
                string nomalName = RelicManager.HandleRandomRelic(1, action: RelicAction.Acquire).name;
                return $"이상한 꿈 속에서 무언가를 손에 쥐고 깨어났다. 그것은 {nomalName}이었다.";
            case 1: // 저주 등급 전쟁 유산
                string cursedName = RelicManager.HandleRandomRelic(0, action: RelicAction.Acquire).name;
                return $"당신은 뭔가 꺼림칙한 {cursedName}을 손에 넣었다.";
            case 2: // 무작위 유닛 전직
                return "무작위 유닛 전직-미구현";
            case 3: // 무작위 유닛 기력 1로
                return DrainRandomUnitEnergy();
            case 4: // 무작위 유닛 획득
                return AcquireRandomUnit();
            case 5: // 무작위 병종 무작위 강화
                return "무작위 병종 무작위 강화-미구현";
            case 6: // 사기 최대 회복
                RogueLikeData.Instance.SetMorale(100);
                return "병사들은 알 수 없는 자신감에 사로잡혔다. 사기가 최고조에 달했다.";
            case 7: // 사기 -25
                RogueLikeData.Instance.SetMorale(Math.Max(0, RogueLikeData.Instance.GetMorale() - 25));
                return "병사들은 깊은 두려움과 공포에 시달렸다. 사기가 크게 떨어졌다.";
            case 8: // 금화 중간량 획득
                int gold = RogueLikeData.Instance.AddGoldByEventChapter(150);
                return $"주머니 속에 금화 {gold}개가 들어있었다.";
            default:
                return "꿈은 곧 사라졌다.";
        }
    }
    private string AcquireRandomUnit()
    {
        int newUnitId = UnityEngine.Random.Range(0, 52); // 0부터 51까지 포함
        //RogueUnitDataBase newUnit = RogueUnitLoader.CreateUnitById(newUnitId);

        //var myUnits = RogueLikeData.Instance.GetMyUnits();
        //myUnits.Add(newUnit);
        //RogueLikeData.Instance.AllMyUnits(myUnits);

        return $"꿈에서 만난 낯선 자가 당신의 병력이 되었다. ({newUnitId})-반 구현";
    }
    private string DrainRandomUnitEnergy()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        if (units.Count == 0) return "병사가 없다.";
        var unit = units[random.Next(units.Count)];
        unit.energy = 1;
        return $"{unit.unitName}(은)는 악몽에 시달리며 기력을 거의 잃었다.";
    }
}
