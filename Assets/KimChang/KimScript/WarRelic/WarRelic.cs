public class WarRelic
{
    public int id;                // 유물 고유 ID
    public string name;           // 유물 이름
    public string description;    // 유물 설명
    public RelicType type;        // 유물 타입 (예: StatBoost, SpecialEffect 등)
    public int rank;              // 유물 등급
    public float attackAdd;       // 추가 공격력
    public float healthAdd;       // 추가 체력
    public float armorAdd;        // 추가 방어력
    public float mobilityAdd;     // 추가 기동성

    public WarRelic(int id, string name, int rank, string description, RelicType type, 
                    float attackAdd = 0, float healthAdd = 0, float armorAdd = 0, float mobilityAdd = 0)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.type = type;
        this.rank = rank;
        this.attackAdd = attackAdd;
        this.healthAdd = healthAdd;
        this.armorAdd = armorAdd;
        this.mobilityAdd = mobilityAdd;
    }
}

public enum RelicType
{
    StatBoost,      // 스탯 강화형
    SpecialEffect   // 특수 효과형
}
