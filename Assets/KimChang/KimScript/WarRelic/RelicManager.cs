using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 전쟁 유산 매니저
/// </summary>
public class RelicManager
{
    // 보유 유산 (전투/효과 처리용 런타임 객체)
    private static readonly Dictionary<int, WarRelic> ownedRelics = new Dictionary<int, WarRelic>(64);
    private static bool curseBlock;

    private static Dictionary<int, WarRelicRecord> _catalogById;     // id → 데이터(정적 정보)
    private static Dictionary<int, List<int>> _idsByGrade;           // grade → id 리스트
    private static bool _catalogReady;

    /// <summary>
    /// 사용처: 게임 시작 시 1회 호출(카탈로그 캐시 준비)
    /// </summary>
    public static bool InitializeRelicCatalog(string resourcePath = "JsonData/WarRelicsList")
    {
        if (_catalogReady) return true;
        if (!WarRelicLoader.TryLoadFromResources(resourcePath, out var map))
            return false;

        _catalogById = map;
        _idsByGrade = new Dictionary<int, List<int>>(8);

        // 사용처: grade 인덱스 구성 + value 전달용 버퍼
        var valuesById = new Dictionary<int, string[]>(map.Count);
        foreach (var kv in map)
        {
            int id = kv.Key;
            var r = kv.Value;

            if (!_idsByGrade.TryGetValue(r.grade, out var list))
            {
                list = new List<int>(16);
                _idsByGrade[r.grade] = list;
            }
            list.Add(id);

            if (r.value != null && r.value.Length > 0)
                valuesById[id] = r.value;
        }

        // 사용처: DB에 value 일괄 바인딩(이미 생성된 객체들도 주입)
        WarRelicDatabase.BindValuesFromCatalog(valuesById);

        _catalogReady = true;

        // 사용처: 실행 함수가 이미 등록돼 있다면 모든 유산에 실행/값 바인딩 보정
        WarRelicDatabase.BindExecOnAllRelics();
        WarRelicDatabase.BindValuesOnAllRelics();

        return true;
    }

    /// <summary>
    /// 사용처: id로 카탈로그(정적 정보) 단건 조회
    /// </summary>
    public static bool TryGetRelicInfoById(int id, out WarRelicRecord info)
    {
        if (!_catalogReady) InitializeRelicCatalog();
        if (_catalogById != null && _catalogById.TryGetValue(id, out info))
            return true;
        info = null;
        return false;
    }

    /// <summary>
    /// 사용처: grade로 카탈로그 id 리스트 조회(읽기전용 참조 성격)
    /// </summary>
    public static IReadOnlyList<int> GetRelicIdsByGrade(int grade)
    {
        if (!_catalogReady) InitializeRelicCatalog();
        if (_idsByGrade != null && _idsByGrade.TryGetValue(grade, out var list) && list != null)
            return list;
        return Array.Empty<int>();
    }

    public enum RelicAction
    {
        Acquire,
        Remove
    }

    /// <summary>
    /// 사용처: RogueLikeData에서 보유 유산을 읽어와 런타임 사전 재구성
    /// </summary>
    public static void GetRelicData()
    {
        if (!_catalogReady) InitializeRelicCatalog();
        ownedRelics.Clear();

        void AddRelics(List<WarRelic> relics)
        {
            if (relics == null || relics.Count == 0) return;
            for (int i = 0; i < relics.Count; i++)
            {
                var r = relics[i];
                if (r == null) continue;
                if (!ownedRelics.ContainsKey(r.id))
                    ownedRelics.Add(r.id, r);
            }
        }

        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.AllEffect));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.SpecialEffect));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.StateBoost));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.BattleActive));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.ActiveState));
    }

    /// <summary>
    /// 사용처: 보유 유산의 런타임 객체 조회(전투/실행용)
    /// </summary>
    public static WarRelic GetRelicById(int id)
    {
        return ownedRelics.TryGetValue(id, out var warRelic) ? warRelic : null;
    }

    /// <summary>
    /// 사용처: 특정 등급에서 중복 여부를 고려하여 선택 가능한 유산 목록 반환(런타임 객체)
    /// </summary>
    public static List<WarRelic> GetAvailableRelics(int grade, RelicAction action)
    {
        var random = RogueLikeData.Instance.GetRandomBySeed();
        if (grade == 5) grade = random.Next(0, 10) < 2 ? 10 : 1; // 20% 전설
        else if (grade == 7) grade = random.Next(0, 10) < 5 ? 10 : 1; // 50% 전설

      
        var srcIds = GetRelicIdsByGrade(grade);
        if (srcIds.Count == 0) return new List<WarRelic>(0);

        // 보유 집합을 HashSet으로 1회 구성
        var owned = RogueLikeData.Instance.GetAllOwnedRelicIds();
        HashSet<int> ownedSet = owned != null ? new HashSet<int>(owned) : null;

        // 필터 후 런타임 WarRelic로 매핑
        var result = new List<WarRelic>(srcIds.Count);
        for (int i = 0; i < srcIds.Count; i++)
        {
            int id = srcIds[i];
            bool isOwned = ownedSet != null && ownedSet.Contains(id);
            if ((action == RelicAction.Acquire && !isOwned) ||
                (action == RelicAction.Remove && isOwned))
            {
                var wr = WarRelicDatabase.GetRelicById(id);
                if (wr != null) result.Add(wr);
            }
        }
        return result;
    }

    /// <summary>
    /// 사용처: 랜덤 유산 id 하나 반환(등급/획득/삭제 공용)
    /// </summary>
    public static int GetRandomRelicId(int grade, RelicAction action)
    {
        var available = GetAvailableRelics(grade, action);
        if (available.Count == 0) return -1;

        return available[RogueLikeData.Instance.GetRandomInt(0, available.Count)].id;
    }

    /// <summary>
    /// 사용처: 무작위 유산 추가/제거(등급 대상)
    /// </summary>
    public static WarRelic HandleRandomRelic(int grade, RelicAction action)
    {
        List<WarRelic> available = GetAvailableRelics(grade, action);
        if (available.Count == 0) return null;
        Debug.Log(available.Count);
        WarRelic selected = available[RogueLikeData.Instance.GetRandomInt(0, available.Count)];

        if (action == RelicAction.Acquire)
        {
            RogueLikeData.Instance.AcquireRelic(selected.id);
            if (!ownedRelics.ContainsKey(selected.id))
                ownedRelics.Add(selected.id, selected);
        }
        else // Remove
        {
            RogueLikeData.Instance.RemoveRelicById(selected.id);
            ownedRelics.Remove(selected.id);
        }

        return selected;
    }

    //유산9
    public static float RunPulsatingDoll(RogueUnitDataBase unit,bool isTeam)
    {
        if (!isTeam) return 0;
        float damage = 0;
        if (CheckRelicById(9))
        {
            WarRelic relic = GetRelicById(9);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                if (RogueLikeData.Instance.GetRandomInt(0, (int)vals[1]) == 0)
                {
                    damage = unit.attackDamage * ((1 - unit.Armor) / (unit.Armor + 10));
                }
            }
        }
        
        return damage;
    }

    //유산 12
    public static int RunPantheonModel()
    {
        if (CheckRelicById(12))
        {
            WarRelic relic = GetRelicById(12);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                var myTeam = RogueLikeData.Instance.GetMyTeam();
                HashSet<int> seenIdx = new HashSet<int>();

                for (int i = 0; i < myTeam.Count; i++)
                {
                    int idx = myTeam[i].idx;
                    if (!seenIdx.Add(idx))
                    {
                        return 0;
                    }
                }
                return (int)vals[0];
            }
        }
        return 0;
    }
    //유산 13
    public static bool RunChessboard()
    {
        if (CheckRelicById(13))
        {
            var myTeam = RogueLikeData.Instance.GetMyTeam();

            int[] branchCounts = new int[8];

            for (int i = 0; i < myTeam.Count; i++)
            {
                int branch = myTeam[i].branchIdx;
                if (branch >= 0 && branch < 8)
                    branchCounts[branch]++;
            }

            // 조건 검사
            bool allHaveAtLeastTwo = true;
            for (int i = 0; i < 8; i++)
            {
                if (branchCounts[i] < 2)
                {
                    allHaveAtLeastTwo = false;
                }
            }
            return allHaveAtLeastTwo;
        }
        return false;
    }

    /// <summary>
    /// 사용처: 합성 체크(23,24,25 → 26)
    /// </summary>
    public static void CheckFusion()
    {
        if (ownedRelics.ContainsKey(26)) return;

        int[] requiredRelicIds = { 23, 24, 25 };
        for (int i = 0; i < requiredRelicIds.Length; i++)
            if (!ownedRelics.ContainsKey(requiredRelicIds[i])) return;

        for (int i = 0; i < requiredRelicIds.Length; i++)
            ownedRelics[requiredRelicIds[i]].used = true;

        var newRelic = WarRelicDatabase.GetRelicById(26);
        if (newRelic != null)
        {
            RogueLikeData.Instance.AcquireRelic(26);
            if (!ownedRelics.ContainsKey(26))
                ownedRelics.Add(26, newRelic);
        }
    }

    /// <summary>
    /// 사용처: 유산 34 효과
    /// </summary>
    public static void SurvivorOfRag(List<RogueUnitDataBase> units, bool isTeam)
    {
        int id = 34;
        if (!isTeam || !RelicManager.CheckRelicById(id)) return;

        int alive = 0;
        int lastIdx = -1;
        WarRelic relic = GetRelicById(id);
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        for (int i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            if (unit == null || unit.health <= 0) continue;

            alive++;
            lastIdx = i;

            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }

        if (alive == 1 && lastIdx >= 0)
        {
            var unit = units[lastIdx];
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * vals[1],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Mobility,
                value = vals[2],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }

    //유산 36
    public static int RunBlindWarriorsEyepatch()
    {
        int max = 0;
        if (CheckRelicById(36))
        {
            WarRelic relic = GetRelicById(36);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                max = (int)vals[2];
            }
        }
        return max;
    }

    //유산 44
    public static int RunMistakenOrderReceipt()
    {
        int max = 0;
        if (CheckRelicById(44))
        {
            WarRelic relic = GetRelicById(44);
            var vals = relic?.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                max = (int)vals[0]; 
            }
        }
        return max;
    }

    //유산 66
    public static int RunExpandedFormationDiagram()
    {
        int max = 0;
        if (CheckRelicById(66))
        {
            WarRelic relic = GetRelicById(66);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if(vals != null)
            {
                max = (int)vals[0];
            }

        }
        return max;

    }

    //유산 67
    public static int RunWarlordsInsignia()
    {
        int max = 0;
        if (CheckRelicById(67))
        {
            WarRelic relic = GetRelicById(67);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                max = (int)vals[0];
            }

        }
        return max;

    }

    /// <summary>
    /// 사용처: 전투 시작 전 상태형 유산 실행(StateBoost/ActiveState)
    /// </summary>
    public static int RunStateRelic()
    {
        if (ownedRelics.Count == 0) return 0;

        curseBlock = CheckRelicById(22);

        int executed = 0;
        foreach (var kv in ownedRelics)
        {
            var relic = kv.Value;
            if (relic == null) continue;

            if (relic.type == RelicType.StateBoost || relic.type == RelicType.ActiveState)
            {
                if (curseBlock && relic.grade == 0) continue;
                relic.Execute();
                executed++;
            }
        }
        return executed;
    }

    /// <summary>
    /// 사용처: 보유/미사용 여부 검사
    /// </summary>
    public static bool CheckRelicById(int relicId)
    {
        return ownedRelics.TryGetValue(relicId, out var r) && !r.used;
    }



    /// <summary>
    /// 사용처: 특정 등급에서 선택 가능한 유산 id 목록 반환
    /// 성능: grade 캐시 + HashSet으로 O(n)
    /// </summary>
    public static List<int> GetAvailableRelicIds(int grade, RelicAction action)
    {
        var srcIds = GetRelicIdsByGrade(grade);
        if (srcIds.Count == 0) return new List<int>(0);

        var owned = RogueLikeData.Instance.GetAllOwnedRelicIds();
        HashSet<int> ownedSet = owned != null ? new HashSet<int>(owned) : null;

        var result = new List<int>(srcIds.Count);
        for (int i = 0; i < srcIds.Count; i++)
        {
            int id = srcIds[i];
            bool isOwned = ownedSet != null && ownedSet.Contains(id);
            if ((action == RelicAction.Acquire && !isOwned) ||
                (action == RelicAction.Remove && isOwned))
            {
                result.Add(id);
            }
        }
        return result;
    }

    /// <summary>
    /// 사용처: 모든 등급에서 선택 가능한 유산(런타임) 목록
    /// </summary>
    public static List<WarRelic> GetAvailableRelicsAllGrades(RelicAction action)
    {
        if (!_catalogReady) InitializeRelicCatalog();

        var owned = RogueLikeData.Instance.GetAllOwnedRelicIds();
        HashSet<int> ownedSet = owned != null ? new HashSet<int>(owned) : null;

        var list = new List<WarRelic>(_catalogById != null ? _catalogById.Count : 16);
        if (_catalogById != null)
        {
            foreach (var kv in _catalogById)
            {
                int id = kv.Key;
                bool isOwned = ownedSet != null && ownedSet.Contains(id);
                if ((action == RelicAction.Acquire && !isOwned) ||
                    (action == RelicAction.Remove && isOwned))
                {
                    var wr = WarRelicDatabase.GetRelicById(id);
                    if (wr != null) list.Add(wr);
                }
            }
        }
        return list;
    }

    /// <summary>
    /// 사용처: 모든 등급에서 무작위 유산 추가/제거
    /// </summary>
    public static WarRelic HandleRandomRelicAllGrades(RelicAction action)
    {
        var available = GetAvailableRelicsAllGrades(action);
        if (available.Count == 0) return null;

        var selected = available[RogueLikeData.Instance.GetRandomInt(0, available.Count)];

        if (action == RelicAction.Acquire)
        {
            RogueLikeData.Instance.AcquireRelic(selected.id);
            if (!ownedRelics.ContainsKey(selected.id))
                ownedRelics.Add(selected.id, selected);
        }
        else // Remove
        {
            RogueLikeData.Instance.RemoveRelicById(selected.id);
            ownedRelics.Remove(selected.id);
        }
        return selected;
    }

    /// <summary>
    /// 사용처: 전투 입장 시 유산 처리
    /// </summary>
    public static void EnterBattleRelic()
    {
        //11 무작위 재배치
        if (CheckRelicById(11))
        {
            var units = RogueLikeData.Instance.GetMyUnits();
            var random = RogueLikeData.Instance.GetRandomBySeed();
            for (int i = units.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (units[i], units[j]) = (units[j], units[i]);
            }
            RogueLikeData.Instance.SetAllMyUnits(units);
        }

        //72 영웅 복제 스폰
        if (CheckRelicById(72))
        {
            var myUnits = RogueLikeData.Instance.GetMyUnits();
            var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();

            int minIdx = 52;
            int maxIdx = 66;

            var allUnits = RogueLikeData.Instance.GetMyTeam();
            var heroUnits = allUnits.FindAll(unit => unit.idx >= minIdx && unit.idx <= maxIdx && unit.health > 0);
            if (heroUnits.Count == 0) return;

            var randomUnit = heroUnits[RogueLikeData.Instance.GetRandomInt(0, heroUnits.Count)];
            var newUnit = randomUnit.Clone();

            if (RogueLikeData.Instance.GetRandomFloat() <= 0.1f)
            {
                int uId = RogueUnitDataBase.BuildUnitUniqueId(newUnit.branchIdx, newUnit.idx, false);
                newUnit.UniqueId = uId;
                enemyUnits.Insert(RogueLikeData.Instance.GetRandomInt(0, enemyUnits.Count + 1), newUnit);
                RogueLikeData.Instance.SetAllEnemyUnits(enemyUnits);
            }
            else
            {
                int uId = RogueUnitDataBase.BuildUnitUniqueId(newUnit.branchIdx, newUnit.idx, true);
                newUnit.UniqueId = uId;
                myUnits.Insert(RogueLikeData.Instance.GetRandomInt(0, myUnits.Count + 1), newUnit);
                RogueLikeData.Instance.SetAllMyUnits(myUnits);
            }
        }
    }

    //유산 89
    public static int RunTornList()
    {
        int max = 0;
        WarRelic relic = GetRelicById(89);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals != null)
        {
            max = (int)vals[0];
        }
        return max;
    }
    //유산 94
    public static int RunCastIronHelmet()
    {
        int gold = 0;
        WarRelic relic = GetRelicById(94);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals !=null)
        {
            int heavyCount = 0;
            var myUnits = RogueLikeData.Instance.GetMyUnits();
            foreach (var unit in myUnits)
            {
                if (unit.heavyArmor)
                {
                    heavyCount++;
                }
            }
            gold = (int)vals[0]* heavyCount;
        }
        return gold;
    }

    //유산 99 스테이지 클릭시 골드 및 사기 변화
    public static void RunJarOfDesire()
    {
        WarRelic relic = GetRelicById(99);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        RogueLikeData.Instance.EarnGold((int)vals[0]);
        RogueLikeData.Instance.ChangeMorale((int)vals[1]);

    }

    //유산 104
    public static void RunDoubleEdgedAxeOfPride(List<RogueUnitDataBase> myUnits,
                                              List<RogueUnitDataBase> myDeads,
                                              List<RogueUnitDataBase> enemyUnits,
                                              List<RogueUnitDataBase> enemyDeads)
    {
        const int relicId = 104;

        WarRelic relic = GetRelicById(relicId);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals == null || vals.Count <= 2 || relic.used)
            return;

        int deathThreshold = (int)vals[1];
        float ampRatio = vals[2];

        List<RogueUnitDataBase> targetTeam = null;

        if (enemyDeads != null && enemyDeads.Count >= deathThreshold)
            targetTeam = myUnits;
        else if (myDeads != null && myDeads.Count >= deathThreshold)
            targetTeam = enemyUnits;

        if (targetTeam == null || targetTeam.Count == 0)
            return;


        for (int i = 0; i < targetTeam.Count; i++)
        {
            var u = targetTeam[i];
            if (u == null || u.stats == null) continue;

            u.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = u.baseAttackDamage * ampRatio,
                source = SourceType.Relic,
                modifierId = relicId,
                isPercent = false
            });
        }

        relic.used = true;
    }

    //유산 106
    public static float RunControlTorch()
    {
        float damage = 0;
        WarRelic relic = GetRelicById(106);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals != null)
        {
            damage += vals[1];
        }
        return damage;
    }
    //유산 107
    public static float RunPileOfMedals()
    {
        float multy = 0;
        WarRelic relic = GetRelicById(107);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals != null)
        {
            multy = vals[1];
        }
        return multy;
    }

    //유산 109
    public static void RunTrainingSandbagsOfWar()
    {
        WarRelic relic = GetRelicById(109);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        var front = RogueLikeData.Instance.GetMyUnits()[0];
        for (int i = (int)vals[0]; i > 0; i--)
        {
            bool isAttack = RogueLikeData.Instance.GetRandomInt(0, 2)==0;
            RogueLikeData.Instance.IncreaseUpgrade(front.branchIdx, isAttack,false);
        }
    }

    //유산 110
    public static int RunEpic()
    {
        int max = 0;
        WarRelic relic = GetRelicById(110);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if(vals != null)
        {
            max = (int)vals[0];
        }
        return max;
    }

    //유산 113
    public static void RunBloodSoakedDye(int deadCount)
    {
        int id = 113;
        WarRelic relic = GetRelicById(113);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        vals[4] += deadCount;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        
        foreach (var unit in myUnits)
        {
            unit.stats.RemoveModifiersBySourceAndId(SourceType.Relic, id);
        }

        relic.Execute();

    }

    //유산 114
    public static void RunTerracottaArmy()
    {
        WarRelic relic = GetRelicById(114);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        float fullHp=0;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach(var unit in myUnits)
        {
            fullHp += unit.maxHealth;
            if(fullHp > vals[0])
            {
                RogueLikeData.Instance.EarnGold((int)vals[1]);
                return;
            }
        }
    }
    //유산 123
    public static void RunEndlessBarrage(int deadCount)
    {
        int id = 123;
        WarRelic relic = GetRelicById(123);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        vals[1] = vals[0] * deadCount;
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in myUnits)
        {
            unit.stats.RemoveModifiersBySourceAndId(SourceType.Relic, id);
        }

        relic.Execute();
    }
    //유산 124
    public static void RunPartingShot(List<RogueUnitDataBase> myDeadMyUnits, RogueUnitDataBase enemyFront, RogueUnitDataBase myFront)
    {
        WarRelic relic = GetRelicById(124);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        foreach (var unit in myDeadMyUnits)
        {
            if(unit.branchIdx == 2 && unit != myFront)
            {
                float damage = unit.attackDamage * vals[0];
                Mathf.Round(damage);
                enemyFront.health -= damage;

                return;
            }
        }
    }
    //유산 127
    public static void RunGuardiansCloak(List<RogueUnitDataBase> units, bool isTeam,ref int unitIdx, ref float damage)
    {
        //공격 당하는 유닛
        RogueUnitDataBase target = units[unitIdx];

        WarRelic relic = GetRelicById(127);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        if (!isTeam && !relic.used) return;

        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if(unit.UniqueId == target.UniqueId)
            {
                unitIdx = i;
            }
            if (unit.effectDictionary.ContainsKey(15) && target.UniqueId != unit.UniqueId)
            {
                if (unit.health > 0)
                {
                    damage *= 1 + vals[0];
                }
                else
                {
                    relic.used = false;
                }
            }
        }

        return;
    }

    //유산 129
    public static void RunContractInvoice(List<RogueUnitDataBase> units,RogueUnitDataBase frontUnit)
    {
        WarRelic relic = GetRelicById(129);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (var unit in units)
        {
            if(unit != frontUnit)
            {
                RogueLikeData.Instance.EarnGold((int)vals[0]);
            }
        }
    }

    //유산 132
    public static void RunTyphoonCallingEye(int battleturn)
    {
        int id = 132;
        WarRelic relic = GetRelicById(132);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        if (battleturn <= vals[0]) return;

        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in myUnits)
        {
            unit.stats.RemoveModifiersBySourceAndId(SourceType.Relic, id);
        }

    }

    //전투당 한번 유산 초기화
    public static void ResetBattleOnceRelic()
    {
        if (CheckRelicById(27))
        {
            WarRelic heartRelic = GetRelicById(27);
            heartRelic.used = false;

        }
        if (CheckRelicById(104))
        {
            WarRelic doubleEdgedAxeOfPride = GetRelicById(104);
            doubleEdgedAxeOfPride.used = false; 

        }
        if (CheckRelicById(119))
        {
            WarRelic relic = GetRelicById(119);
            relic.used = false;
        }
        if (CheckRelicById(127))
        {
            WarRelic relic = GetRelicById(127);
            relic.used = false;
        }

    }
    //채크 페이즈 발동 유산
    public static void RunCheckPhaseRelic()
    {
        RunTerracottaArmy();

    }
}
