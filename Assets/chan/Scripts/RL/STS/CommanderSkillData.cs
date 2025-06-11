using System.Collections.Generic;

public static class CommanderSkillData
{
    private static readonly Dictionary<string, string> commanderSkills = new()
    {
        { "슬래시", "파괴자 : 적 전사와 암살자의 공격력 +20%, 체력 -10%" },
        { "프레디", "요새 : 상대 시 플레이어의 유닛 배치 수가 이 지휘관의 배치 수만큼 제한" },
        { "레논", "맹진 : 적 전사가 전열에 있을 때, 아군 유닛이 처치되면 사기 -2" },
        { "헨드릭스", "회오리 헨드릭스(Hendrix) : 전투 시작 시 경기병·암살자의 회피율 +0.05, 아군 경기병 기동력 -1" },
        { "커트", "망령 : 암살, 선제 타격이 적중하면 50% 확률로 즉시 처치" },
        { "잰더", "용광로 : 피해를 입은 아군의 장갑 -2 (누적). 최소 장갑 0" },
        { "액슬", "멸시 : 현재 체력 ≤ 50인 유닛은 즉시 체력 -50" },
        { "페이지", "혼돈 : 더 많은 부대 가치" },
        { "라자루스", "흡혈귀 : 모든 적 유닛이 착취, 흡혈 특성을 얻음" },
        { "호쉬", "눈보라 : 모든 아군 유닛의 기동력 1, 회피율 0으로 고정" },
        { "벨페고르", "악몽 : 적 배치 비공개. 전투 시 아군 중 한 명이 적으로 등장, 체력·공격력 +30%" },
        { "아마록", "집념 : 돌격·암살 기술이 33% 확률로 추가 발동 (기술 당 1회)" },
        { "시리온", "재앙 : 적 사망 시 지원 사격 수만큼 전열 아군에게 피해" },
        { "슈타인", "잊힌 전설 : 노인 기사 체력 2배. 모든 적 특성 1개당 공격력 +5" }
    };

    public static string GetSkillText(string commanderName)
    {
        string[] split = commanderName.Trim().Split(' ');
        string key = split.Length > 1 ? split[1] : split[0];
        return commanderSkills.TryGetValue(key, out string result) ? result : "";
    }
}