using System;
using System.Runtime.CompilerServices;

// 사용처: 런타임 호출 전, 에디터가 생성한 g.cs에서 Initialize를 자동 호출
public static class RelicDispatcher
{
    // 타입별 핸들러 시그니처
    public delegate void StateBoostHandler(in RelicContext ctx, in WarRelicRecord rec);
    public delegate void BattleActiveHandler(in RelicContext ctx, in WarRelicRecord rec);
    public delegate void SpecialEffectHandler(in RelicContext ctx, in WarRelicRecord rec);
    public delegate void GetEffectHandler(in RelicContext ctx, in WarRelicRecord rec);

    // 정적 맵(에디터 코드가 채움). 길이는 에디터가 최대 id+1로 생성.
    public static StateBoostHandler[] StateBoostMap = Array.Empty<StateBoostHandler>();
    public static BattleActiveHandler[] BattleActiveMap = Array.Empty<BattleActiveHandler>();
    public static SpecialEffectHandler[] SpecialEffectMap = Array.Empty<SpecialEffectHandler>();
    public static GetEffectHandler[] GetEffectMap = Array.Empty<GetEffectHandler>();

    // 사용처: 해당 타입 발동 시점마다 호출(전투 시작, 장착 시 등)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke(in RelicContext ctx, in WarRelicRecord rec, RelicType type)
    {
        switch (type)
        {
            case RelicType.StateBoost:
                {
                    var map = StateBoostMap;
                    if ((uint)rec.id < (uint)map.Length && map[rec.id] != null)
                        map[rec.id](in ctx, in rec);
                }
                break;
            case RelicType.BattleActive:
                {
                    var map = BattleActiveMap;
                    if ((uint)rec.id < (uint)map.Length && map[rec.id] != null)
                        map[rec.id](in ctx, in rec);
                }
                break;
            case RelicType.SpecialEffect:
                {
                    var map = SpecialEffectMap;
                    if ((uint)rec.id < (uint)map.Length && map[rec.id] != null)
                        map[rec.id](in ctx, in rec);
                }
                break;
            case RelicType.GetEffect:
                {
                    var map = GetEffectMap;
                    if ((uint)rec.id < (uint)map.Length && map[rec.id] != null)
                        map[rec.id](in ctx, in rec);
                }
                break;
        }
    }
}

// 사용처: 유산 효과 함수가 접근할 수 있는 최소 컨텍스트(필요 필드만 추가)
public readonly struct RelicContext
{
    // 전투/월드 공용 컨텍스트 포인터들. 실제 게임 구조에 맞게 교체.
    public readonly object Team;
    public readonly object Battle;
    public readonly object World;

    public RelicContext(object team, object battle, object world)
    {
        Team = team; Battle = battle; World = world;
    }
}
