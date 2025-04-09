using static EventManager;
using static RelicManager;

public class CursedMirror : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 복사 + 저주 유산
                {
                    // idx로 row 데이터 가져오기
                    var row = GoogleSheetLoader.Instance.GetRowUnitData(unit.idx);
                    if (row == null) return "유닛 데이터를 찾을 수 없습니다.";

                    // row 데이터를 기반으로 유닛 생성 (UniqueId는 내부에서 자동 부여됨)
                    var copiedUnit = RogueUnitDataBase.ConvertToUnitDataBase(row);
                    copiedUnit.energy = unit.energy;

                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    myUnits.Add(copiedUnit);
                    RogueLikeData.Instance.SetAllMyUnits(myUnits); // 부대에 추가

                    var cursed = RelicManager.HandleRandomRelic(0, RelicAction.Acquire);
                    return $"[텍스트 처리] '{unit.unitName}'과 동일한 능력의 병사가 복제되어 부대에 합류했습니다.\n(저주 유산 '{cursed.name}' 획득)";
                }

            case 1: // 저주 유산 제거
                {
                    var removed = RelicManager.HandleRandomRelic(0, RelicAction.Remove);
                    if (removed == null)
                        return "제거할 저주 유산이 없습니다.";

                    return $"거울을 파괴하며 저주를 일부 정화했습니다. 저주 유산 '{removed.name}' 제거됨.";
                }

            default: // 내버려둠
                return "위험해 보이는 거울을 무시하고 지나쳤습니다.";
        }
    }
}
