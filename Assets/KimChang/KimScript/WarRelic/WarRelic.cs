using System;

public enum RelicType
{
    AllEffect,
    SpecialEffect,
    StateBoost,
    BattleActive,
    ActiveState         //BattleActive,StateBoost의 효과가 둘다 있는경우

}
public class WarRelic
{
    public int id;
    public string name;
    public int grade; // 0: 저주, 1: 일반, 10: 전설
    public string tooltip;
    public RelicType type;
    public Action executeAction; // 유물에 연결된 실행 함수
    public bool used;       //1회성 유물의 경우 사용 여부

    public WarRelic(int id, string name, int grade, string tooltip, RelicType type, Action executeAction = null, bool used = false)
    {
        this.id = id;
        this.name = name;
        this.grade = grade;
        this.tooltip = tooltip;
        this.type = type;
        this.executeAction = executeAction;
        this.used = used;
    }

    public void Execute()
    {
        executeAction?.Invoke(); // 함수 실행
    }

    
}

