using UnityEngine;

public class ItemInformation : MonoBehaviour
{
    public ItemInfoData data = new();

    // 사용처: 풀에서 재사용되는 아이콘의 이전 설명 데이터를 제거
    public void Clear()
    {
        data = new ItemInfoData
        {
            isItem = false
        };
    }

    // 사용처: 버프/디버프 아이콘에 툴팁 표시용 데이터를 주입
    public void SetBuffDeBuff(int id, int grade, int duration, string name, string description)
    {
        data = new ItemInfoData
        {
            isItem = false,
            isRelic = false,
            isBuffDeBuff = true,
            buffDeBuffId = id,
            buffDeBuffGrade = grade,
            buffDeBuffDuration = duration,
            buffDeBuffName = name,
            buffDeBuffDescription = description
        };
    }

    // 사용처: 특성/기술 아이콘에 툴팁 표시용 데이터를 주입
    public void SetAbility(int abilityId)
    {
        data = new ItemInfoData
        {
            isItem = false,
            isRelic = false,
            relicId = -1,
            abilityId = abilityId
        };
    }

    // 사용처: 우측 전술 개량 후보 카드에 단일 공격/방어 강화 설명 데이터를 주입
    public void SetUpgrade(int branchIdx, bool isAttack)
    {
        data = new ItemInfoData
        {
            isItem = false,
            isRelic = false,
            relicId = -1,
            isUpgrade = true,
            upgradeTooltipMode = UpgradeTooltipMode.Single,
            upgradeBranchIdx = branchIdx,
            upgradeIsAttack = isAttack,
            upgradeId = branchIdx * 2 + (isAttack ? 0 : 1)
        };
    }

    // 사용처: 좌측 병종 아이콘에 해당 병종의 공격/방어 강화 상태 요약 데이터를 주입
    public void SetUpgradeBranchSummary(int branchIdx)
    {
        data = new ItemInfoData
        {
            isItem = false,
            isRelic = false,
            relicId = -1,
            isUpgrade = true,
            upgradeTooltipMode = UpgradeTooltipMode.BranchSummary,
            upgradeBranchIdx = branchIdx,
            upgradeIsAttack = true,
            upgradeId = -1
        };
    }

    // 사용처: UI/스탯 텍스트에 툴팁 표시용 텍스트 ID를 주입
    public void SetGameText(int gameTextId)
    {
        data = new ItemInfoData
        {
            isItem = false,
            isRelic = false,
            relicId = -1,
            gameTextId = gameTextId
        };
    }
}
