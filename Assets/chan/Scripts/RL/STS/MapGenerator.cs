using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs.LowLevel.Unsafe;

public enum StageType
{
    Unknown,  // 스테이지 첫 진입시 조건에 사용
    Combat,   // 전투
    Elite,    // 엘리트 (하층부에서는 배치 안 됨)
    Event,    // 이벤트
    Shop,     // 상점
    Rest,     // 휴식(강화)
    Treasure, // 보물
    Boss      // 보스
}

public enum BattlefieldEffect
{
    Plains,   // 평원
    Hills,    // 언덕
    Swamp,    // 늪지대
    Forest,   // 숲
    Storm     // 폭풍
}

public class StageNode
{
    public int level;  // 0-indexed (예: 0: 레벨1, 14: 레벨15)
    public int row;    // 0~6 (0:A, 1:B, ... 6:G)
    public StageType stageType;
    public Color stageColor; // 스테이지 색상
    public List<StageNode> connectedNodes; // 다음 레벨과 연결된 노드 목록
    public int presetID;
    public BattlefieldEffect battlefieldEffect; // 전장효과

    public StageNode(int level, int row, StageType stageType)
    {
        this.level = level;
        this.row = row;
        this.stageType = stageType;
        connectedNodes = new List<StageNode>();
        battlefieldEffect = BattlefieldEffect.Plains; // 기본값은 평원
    }

    public override string ToString()
    {
        char rowChar = (char)('A' + row);
        return $"L{level + 1}{rowChar} - {stageType} ({battlefieldEffect})";
    }
}

public class MapGenerator : MonoBehaviour
{
    // 전체 레벨 수(보스 스테이지 포함)
    public int totalLevels = 15;   // 예: 15이면, 레벨1~14는 일반, 레벨15는 보스
    public int totalRows = 7;      // A ~ G (인덱스 0~6)
    public int totalPaths = 6;     // 생성할 경로 수

    // 생성된 일반 스테이지 경로 정보: 각 경로는 각 레벨의 행값(List<int>)로 표현 (레벨 0 ~ totalLevels-2)
    private List<List<int>> paths = new List<List<int>>();
    // 일반 스테이지 노드들을 "level_row" 문자열을 key로 저장 (레벨 0 ~ totalLevels-2)
    private Dictionary<string, StageNode> nodeDict = new Dictionary<string, StageNode>();
    public Dictionary<string, StageNode> NodeDictionary { get { return nodeDict; } }

    public void ClearAll()
    {
        nodeDict.Clear();
        paths.Clear();
    }
    public void OverrideNodeDict(Dictionary<string, StageNode> loadedDict)
    {
        nodeDict = loadedDict;
    }

    // ─── 특수 프리셋 190, 191, 192번 관리 ─────────────────
    private static Dictionary<int, List<int>> specialPresetUnits = new Dictionary<int, List<int>>();
    private static bool specialPresetsInitialized = false;

    void Start()
    {
        // 게임 시작 시 특수 프리셋 초기화
        InitializeSpecialPresets();
    }

    /// <summary>
    /// 게임 시작 시 특수 프리셋 190, 191, 192번의 유닛 구성을 초기화합니다.
    /// </summary>
    public void InitializeSpecialPresets()
    {
        if (specialPresetsInitialized)
        {
            Debug.Log("[MapGenerator] 특수 프리셋이 이미 초기화되었습니다.");
            return;
        }

        Debug.Log("[MapGenerator] 특수 프리셋 190, 191, 192번 초기화 시작");

        // UnitLoader 초기화 확인
        if (UnitLoader.Instance == null)
        {
            Debug.LogError("[MapGenerator] UnitLoader가 초기화되지 않았습니다.");
            return;
        }

        // 유닛 데이터 로드
        UnitLoader.Instance.LoadUnitsFromJson();
        var allUnits = UnitLoader.Instance.GetAllCachedUnits();
        
        if (allUnits == null || allUnits.Count == 0)
        {
            Debug.LogError("[MapGenerator] 유닛 데이터를 로드할 수 없습니다.");
            return;
        }

        // branchIdx=7(지원병)을 제외한 유닛들만 필터링
        var availableUnits = allUnits.Where(unit => unit.branchIdx != 7).ToList();
        Debug.Log($"[MapGenerator] 사용 가능한 유닛 수: {availableUnits.Count}");

        // 각 프리셋별 유닛 구성 생성
        for (int presetId = 190; presetId <= 192; presetId++)
        {
            int budget = presetId switch
            {
                190 => 2000,
                191 => 4500,
                192 => 7000,
                _ => 2000
            };

            var selectedUnits = GenerateRandomUnitComposition(availableUnits, budget);
            specialPresetUnits[presetId] = selectedUnits;
            
            Debug.Log($"[MapGenerator] 프리셋 {presetId}번 구성 완료 - 유닛 수: {selectedUnits.Count}, 예산: {budget}");
        }

        specialPresetsInitialized = true;
        Debug.Log("[MapGenerator] 특수 프리셋 초기화 완료");
    }

    /// <summary>
    /// 예산 내에서 무작위로 유닛을 선택하여 구성합니다.
    /// </summary>
    /// <param name="availableUnits">사용 가능한 유닛 리스트</param>
    /// <param name="budget">예산</param>
    /// <returns>선택된 유닛 ID 리스트</returns>
    private List<int> GenerateRandomUnitComposition(List<RogueUnitDataBase> availableUnits, int budget)
    {
        var selectedUnits = new List<int>();
        int remainingBudget = budget;
        var random = RogueLikeData.Instance.GetRandomBySeed();

        // 최대 100번 시도하여 예산을 최대한 활용
        for (int attempt = 0; attempt < 100 && remainingBudget > 0; attempt++)
        {
            // 예산 내에서 구매 가능한 유닛들 필터링
            var affordableUnits = availableUnits.Where(unit => unit.unitPrice <= remainingBudget).ToList();
            
            if (affordableUnits.Count == 0)
                break;

            // 무작위로 유닛 선택
            int randomIndex = random.Next(0, affordableUnits.Count);
            var selectedUnit = affordableUnits[randomIndex];
            
            // 선택된 유닛을 리스트에 추가하고 예산 차감
            selectedUnits.Add(selectedUnit.idx);
            remainingBudget -= selectedUnit.unitPrice;
        }

        return selectedUnits;
    }

    /// <summary>
    /// 특수 프리셋의 유닛 구성을 가져옵니다.
    /// </summary>
    /// <param name="presetId">프리셋 ID (190, 191, 192)</param>
    /// <returns>유닛 ID 리스트</returns>
    public List<int> GetSpecialPresetUnits(int presetId)
    {
        if (!specialPresetsInitialized)
        {
            Debug.LogWarning($"[MapGenerator] 특수 프리셋이 초기화되지 않았습니다. 프리셋 {presetId}번 초기화 중...");
            InitializeSpecialPresets();
        }

        if (specialPresetUnits.ContainsKey(presetId))
        {
            return specialPresetUnits[presetId];
        }

        Debug.LogError($"[MapGenerator] 프리셋 {presetId}번의 유닛 구성을 찾을 수 없습니다.");
        return new List<int>();
    }

    /// <summary>
    /// 특수 프리셋 데이터를 저장용 딕셔너리로 반환합니다.
    /// </summary>
    /// <returns>저장용 딕셔너리</returns>
    public Dictionary<int, List<int>> GetSpecialPresetDataForSave()
    {
        return new Dictionary<int, List<int>>(specialPresetUnits);
    }

    /// <summary>
    /// 저장된 특수 프리셋 데이터를 로드합니다.
    /// </summary>
    /// <param name="savedData">저장된 데이터</param>
    public void LoadSpecialPresetData(Dictionary<int, List<int>> savedData)
    {
        if (savedData == null || savedData.Count == 0)
        {
            specialPresetsInitialized = false;
            InitializeSpecialPresets();
            return;
        }

        if (savedData != null)
        {
            specialPresetUnits = new Dictionary<int, List<int>>(savedData);
            specialPresetsInitialized = true;
            foreach (int presetId in specialPresetUnits.Keys)
            {
                UpdateSpecialPresetForStage(presetId);
            }
            Debug.Log($"[MapGenerator] 특수 프리셋 데이터 로드 완료 - 프리셋 수: {savedData.Count}");
        }
    }
    // ─── Combat/Elite/Boss 에 맞춰 presetID 선정 함수 ─────────────────
    private int PickPresetID(int level, StageType stageType)
    {
        int chapter = RogueLikeData.Instance.GetChapter();

        // 챕터 2 이상 일반 전투는 고정 StagePreset이 아니라 EnemyBudgetComposer가 적 편성을 만든다.
        if (chapter >= 2 && stageType == StageType.Combat)
            return -1;

        // StageType → JSON 문자열 매핑
        string jsonType = stageType switch
        {
            StageType.Combat => "normal",
            StageType.Elite => "elite",
            StageType.Boss => "boss",
            _ => null
        };
        if (jsonType == null)
            return -1;

        // JSON에서 후보 리스트 가져오기
        var candidates = StagePresetLoader.I.GetPresets(chapter, level, jsonType);
        if (candidates == null || candidates.Count == 0)
        {
            Debug.LogWarning($"[{chapter}-{level}-{jsonType}] 후보 프리셋이 없습니다.");
            return -1;
        }

        // Combat, Elite, Boss 모두 후보 중 랜덤 선택
        var random = RogueLikeData.Instance.GetRandomBySeed();
        int idx = random.Next(0, candidates.Count);
        int selectedPresetID = candidates[idx].PresetID;

        // 특수 프리셋 190, 191, 192번이 선택된 경우 동적 업데이트
        if (selectedPresetID == 190 || selectedPresetID == 191 || selectedPresetID == 192)
        {
            UpdateSpecialPresetForStage(selectedPresetID);
        }

        return selectedPresetID;
    }

    /// <summary>
    /// 특수 프리셋이 선택된 경우 StagePresetLoader를 업데이트합니다.
    /// </summary>
    /// <param name="presetId">선택된 프리셋 ID</param>
    private void UpdateSpecialPresetForStage(int presetId)
    {
        if (StagePresetLoader.I == null)
        {
            Debug.LogError("[MapGenerator] StagePresetLoader가 초기화되지 않았습니다.");
            return;
        }

        // 특수 프리셋의 유닛 구성을 가져옴
        var unitList = GetSpecialPresetUnits(presetId);
        if (unitList.Count > 0)
        {
            // StagePresetLoader의 프리셋 업데이트
            var preset = StagePresetLoader.I.GetByID(presetId);
            if (preset != null)
            {
                preset.UnitList = new List<int>(unitList);
                preset.UnitCount = unitList.Count;
                
                // 총 가치 계산
                if (UnitLoader.Instance != null)
                {
                    int totalValue = 0;
                    foreach (int unitId in unitList)
                    {
                        var unit = UnitLoader.Instance.GetUnitById(unitId);
                        if (unit != null)
                        {
                            totalValue += unit.unitPrice;
                        }
                    }
                    preset.Value = totalValue;
                }
                
                Debug.Log($"[MapGenerator] 프리셋 {presetId}번이 스테이지용으로 업데이트되었습니다 - 유닛 수: {unitList.Count}, 총 가치: {preset.Value}");
            }
        }
        else
        {
            Debug.LogError($"[MapGenerator] 프리셋 {presetId}번의 유닛 구성을 가져올 수 없습니다.");
        }
    }

    // ─── 전장효과 생성 함수 ─────────────────
    private BattlefieldEffect GenerateBattlefieldEffect(int level, StageType stageType, string commanderName = "")
    {
        // 전투 스테이지가 아닌 경우 전장 효과 없음 (기본값 반환)
        if (stageType != StageType.Combat && stageType != StageType.Elite && stageType != StageType.Boss)
        {
            return BattlefieldEffect.Plains; // 기본값 (실제로는 사용되지 않음)
        }

        // 지휘관 특성으로 정해지는 전장효과 우선 적용
        if (!string.IsNullOrEmpty(commanderName))
        {
            BattlefieldEffect commanderEffect = GetCommanderBattlefieldEffect(commanderName);
            if (commanderEffect != BattlefieldEffect.Plains)
            {
                return commanderEffect; // 지휘관 특성 우선 적용
            }
        }

        // 챕터 1의 레벨 1-8은 평원으로 고정
        int chapter = RogueLikeData.Instance.GetChapter();
        if (chapter == 1 && level >= 1 && level <= 8)
        {
            return BattlefieldEffect.Plains;
        }

        // 지휘관 특성이 없거나 평원인 경우 확률에 따라 전장효과 결정
        var random = RogueLikeData.Instance.GetRandomBySeed();
        float randomValue = (float)random.NextDouble() * 100f;
        
        if (randomValue < 50f)
        {
            return BattlefieldEffect.Plains;      // 평원 50%
        }
        else if (randomValue < 65f)
        {
            return BattlefieldEffect.Hills;       // 언덕 15%
        }
        else if (randomValue < 80f)
        {
            return BattlefieldEffect.Swamp;       // 늪지대 15%
        }
        else if (randomValue < 95f)
        {
            return BattlefieldEffect.Forest;      // 숲 15%
        }
        else
        {
            return BattlefieldEffect.Storm;       // 폭풍 5%
        }
    }

    // ─── 전장효과를 한국어로 변환하는 유틸리티 메서드 ─────────────────
    public static string GetBattlefieldEffectKoreanName(BattlefieldEffect effect)
    {
        return effect switch
        {
            BattlefieldEffect.Plains => "평원",
            BattlefieldEffect.Hills => "언덕",
            BattlefieldEffect.Swamp => "늪지대",
            BattlefieldEffect.Forest => "숲",
            BattlefieldEffect.Storm => "폭풍",
            _ => "알 수 없음"
        };
    }

    // BattlefieldEffect를 fieldId로 변환하는 함수
    public static int GetFieldIdFromBattlefieldEffect(BattlefieldEffect effect)
    {
        return effect switch
        {
            BattlefieldEffect.Plains => 0,  // 기본값 (효과 없음)
            BattlefieldEffect.Hills => 1,    // 언덕: 모든 유닛 방어력 +1
            BattlefieldEffect.Swamp => 2,    // 늪지대: 모든 유닛 기동력 -2
            BattlefieldEffect.Forest => 3,   // 숲: 경장갑 기절, 궁병 공격력 -10%
            BattlefieldEffect.Storm => 4,    // 폭풍: 매 턴 랜덤 유닛 체력 -30
            _ => 0
        };
    }

    // 지휘관별 전장 효과 매핑 함수 (우선 적용용)
    public static BattlefieldEffect GetCommanderBattlefieldEffect(string commanderName)
    {
        return commanderName switch
        {
            "호쉬" => BattlefieldEffect.Storm,      // 눈보라 → 폭풍
            "모리슨" => BattlefieldEffect.Hills,    // 광채 → 언덕
            "딜런" => BattlefieldEffect.Forest,     // 안개 → 숲
            "잰더" => BattlefieldEffect.Swamp,      // 용광로 → 늪지대
            _ => BattlefieldEffect.Plains           // 기본값 (우선 적용 안함)
        };
    }

    
    public void GeneratePathsNonCrossing()
    {
        int normalLevels = totalLevels - 1; // 보스 스테이지 제외
        List<int[]> matrix = new List<int[]>();

        // 1. 레벨 0: 시작 위치 생성 (랜덤 totalPaths개의 값을 생성 후 오름차순 정렬)
        int[] level0 = GenerateStartingPositions();
        if (level0 == null)
        {
            Debug.LogError("시작 위치 생성에 실패했습니다.");
            return;
        }
        matrix.Add(level0);

        // 2. 레벨 1부터 normalLevels-1까지 재귀적으로 생성
        if (!GenerateLevel(1, matrix, normalLevels))
        {
            Debug.LogError("경로 생성에 실패했습니다.");
            return;
        }

        // 3. 행렬을 열 단위로 분리하여 paths에 저장
        List<List<int>> newPaths = new List<List<int>>();
        for (int i = 0; i < totalPaths; i++)
        {
            List<int> path = new List<int>();
            for (int lvl = 0; lvl < normalLevels; lvl++)
            {
                path.Add(matrix[lvl][i]);
            }
            newPaths.Add(path);
        }
        this.paths = newPaths;

        // (디버그) 생성된 경로 출력
        for (int i = 0; i < newPaths.Count; i++)
        {
            string pathStr = $"경로 {i + 1}: ";
            for (int lvl = 0; lvl < newPaths[i].Count; lvl++)
            {
                char rowChar = (char)('A' + newPaths[i][lvl]);
                pathStr += $"L{lvl + 1}{rowChar} ";
            }
            Debug.Log(pathStr);
        }

        // 4. 일반 스테이지 노드 생성 및 보스 노드와 연결
        BuildFinalMap(newPaths, normalLevels);
    }

    private int[] GenerateStartingPositions()
    {
        const int maxAttempts = 100;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int[] arr = new int[totalPaths];
            var random = RogueLikeData.Instance.GetRandomBySeed();
            for (int i = 0; i < totalPaths; i++)
            {
                arr[i] = random.Next(0, totalRows);
            }
            System.Array.Sort(arr);
            bool allSame = true;
            for (int i = 1; i < totalPaths; i++)
            {
                if (arr[i] != arr[0])
                {
                    allSame = false;
                    break;
                }
            }
            if (!allSame)
                return arr;
        }
        return null;
    }

    private bool GenerateLevel(int level, List<int[]> matrix, int normalLevels)
    {
        if (level >= normalLevels)
            return true;
        int[] prev = matrix[matrix.Count - 1];
        int[] current = new int[totalPaths];
        if (!GenerateCurrentLevelRec(0, 0, prev, current, level))
            return false;
        matrix.Add((int[])current.Clone());
        if (!GenerateLevel(level + 1, matrix, normalLevels))
        {
            matrix.RemoveAt(matrix.Count - 1);
            return false;
        }
        return true;
    }

    private bool GenerateCurrentLevelRec(int index, int last, int[] prev, int[] current, int currentLevel)
    {
        if (index == totalPaths)
            return true;
        List<int> allowed = new List<int>();
        int baseVal = prev[index];
        if (baseVal - 1 >= 0) allowed.Add(baseVal - 1);
        allowed.Add(baseVal);
        if (baseVal + 1 < totalRows) allowed.Add(baseVal + 1);
        var random = RogueLikeData.Instance.GetRandomBySeed();
        allowed = allowed.Distinct().OrderBy(x => (float)random.NextDouble()).ToList();
        foreach (int candidate in allowed)
        {
            if (candidate >= last)
            {
                current[index] = candidate;
                if (GenerateCurrentLevelRec(index + 1, candidate, prev, current, currentLevel))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 생성된 경로(newPaths)를 바탕으로 최종 맵(노드)을 구성합니다.
    /// 1) 일반 스테이지 노드를 생성합니다. 
    ///    - 고정 규칙: 레벨 1(0번)은 Combat, 레벨 8(7번)은 Treasure, 레벨 14(13번)은 Rest.
    ///    - 보스 스테이지(마지막 레벨)는 별도로 생성됩니다.
    /// 2) 일반 스테이지 노드들은 연속해서 연결됩니다.
    /// 3) 보스 노드를 생성하여 각 경로의 마지막 노드와 연결합니다.
    /// 스테이지 타입은 다음 확률에 따라 무작위로 결정됩니다:
    ///   Shop: 5%, Event: 22%, Rest: 12%, Elite: 8%, Combat: 53%
    /// 단, 하층부(레벨 2~7)는 Elite가 허용되지 않고, 레벨 13에는 Rest가 허용되지 않습니다.
    /// 또한, 연속 배치는 피하도록 이전 노드와 같은 타입은 제외합니다.
    /// </summary>
    /// <param name="newPaths">생성된 일반 스테이지 경로들</param>
    /// <param name="normalLevels">보스 스테이지 제외한 일반 레벨 수</param>
    void BuildFinalMap(List<List<int>> newPaths, int normalLevels)
    {
        nodeDict.Clear();
        // 1) 일반 스테이지 노드 생성 (레벨 0 ~ normalLevels-1)
        foreach (var path in newPaths)
        {
            for (int lvl = 0; lvl < newPaths[0].Count; lvl++)
            {
                int row = path[lvl];
                string key = lvl + "_" + row;
                if (!nodeDict.ContainsKey(key))
                {
                    StageType type;
                    // 고정 스테이지 배치 규칙 우선 적용 (1-indexed)
                    if (lvl == 0)
                    {
                        type = StageType.Combat; // 레벨 1: Combat (일반 전투)
                    }
                    else if (lvl == 7)
                    {
                        type = StageType.Treasure; // 레벨 8: 보물
                    }
                    else if (lvl == 13)
                    {
                        type = StageType.Rest; // 레벨 14: 휴식
                    }
                    else if (lvl == newPaths[0].Count - 1)
                    {
                        // 보스 스테이지는 별도로 처리하므로 여기서는 생성하지 않음.
                        continue;
                    }
                    else
                    {
                        // 일반 레벨: 확률 기반 무작위 선택
                        // 기본 확률 분포:
                        //   Shop: 5%, Event: 22%, Rest: 12%, Elite: 8%, Combat: 53%
                        Dictionary<StageType, float> baseProbs = new Dictionary<StageType, float>
                        {
                            { StageType.Shop, 5f },
                            { StageType.Event, 22f },
                            { StageType.Rest, 12f },
                            { StageType.Elite, 8f },
                            { StageType.Combat, 53f }
                        };

                        // 하층부(레벨 2~7; index 1~6): 엘리트 배제
                        if (lvl < 7)
                        {
                            baseProbs.Remove(StageType.Elite);
                        }
                        // 레벨 13 (index 12): 휴식(Rest) 배제
                        if (lvl == 12)
                        {
                            baseProbs.Remove(StageType.Rest);
                        }
                        // 연속 배치 제한: 만약 이전 레벨 노드가 Elite, Shop, Rest였다면 그 타입 제거
                        if (lvl - 1 >= 0)
                        {
                            string prevKey = (lvl - 1) + "_" + path[lvl - 1];
                            if (nodeDict.ContainsKey(prevKey))
                            {
                                StageType prevType = nodeDict[prevKey].stageType;
                                if (prevType == StageType.Elite || prevType == StageType.Shop || prevType == StageType.Rest)
                                    baseProbs.Remove(prevType);
                            }
                        }
                        // 확률에 따라 타입 선택
                        float total = baseProbs.Values.Sum();
                        var random = RogueLikeData.Instance.GetRandomBySeed();
                        float rand = (float)random.NextDouble() * total;
                        float cumulative = 0f;
                        StageType selected = StageType.Combat;
                        foreach (var kvpProb in baseProbs)
                        {
                            cumulative += kvpProb.Value;
                            if (rand <= cumulative)
                            {
                                selected = kvpProb.Key;
                                break;
                            }
                        }
                        type = selected;
                    }
                    StageNode node = new StageNode(lvl, row, type);
                    nodeDict[key] = node;
                    
                    // 전장효과 설정 (지휘관 정보 포함)
                    string commanderName = "";
                    int selectedPresetId = -1;
                    if (StagePresetLoader.I != null)
                    {
                        selectedPresetId = PickPresetID(lvl + 1, type);
                        node.presetID = selectedPresetId;

                        if (selectedPresetId != -1)
                        {
                            var preset = StagePresetLoader.I.GetByID(selectedPresetId);
                            commanderName = preset?.Commander ?? "";
                        }
                    }
                    node.battlefieldEffect = GenerateBattlefieldEffect(lvl + 1, type, commanderName);
                    
                    // ① StagePresetLoader.I 가 준비되어 있는지 확인
                    if (StagePresetLoader.I == null)
                        Debug.LogWarning("[MapGenerator] StagePresetLoader.I is null; presetID skipped");
                }
            }
        }

        // 2) 일반 스테이지 노드들 연결 (연속 레벨)
        foreach (var path in newPaths)
        {
            for (int lvl = 0; lvl < path.Count - 1; lvl++)
            {
                int currentRow = path[lvl];
                int nextRow = path[lvl + 1];
                string key = lvl + "_" + currentRow;
                string nextKey = (lvl + 1) + "_" + nextRow;
                if (nodeDict.ContainsKey(key) && nodeDict.ContainsKey(nextKey))
                {
                    StageNode currentNode = nodeDict[key];
                    StageNode nextNode = nodeDict[nextKey];
                    if (!currentNode.connectedNodes.Contains(nextNode))
                        currentNode.connectedNodes.Add(nextNode);
                }
            }
        }

        // 3) 보스 스테이지 생성 및 연결
        // 보스 스테이지는 전체 레벨 중 마지막(레벨 = normalLevels, 즉 totalLevels-1)에서 단일 노드로 생성합니다.
        StageNode bossNode = new StageNode(normalLevels, 3, StageType.Boss); // 여기서 row 3(예: D열) 고정
        
        // 보스 스테이지 전장효과 설정 (지휘관 정보 포함)
        string bossCommanderName = "";
        int selectedBossPresetId = -1;
        if (StagePresetLoader.I != null)
        {
            selectedBossPresetId = PickPresetID(normalLevels, StageType.Boss);
            bossNode.presetID = selectedBossPresetId;

            if (selectedBossPresetId != -1)
            {
                var bossPreset = StagePresetLoader.I.GetByID(selectedBossPresetId);
                bossCommanderName = bossPreset?.Commander ?? "";
            }
        }
        bossNode.battlefieldEffect = GenerateBattlefieldEffect(normalLevels, StageType.Boss, bossCommanderName);
        string bossKey = normalLevels + "_3";
        nodeDict[bossKey] = bossNode;

        // 각 경로의 마지막 일반 스테이지 노드와 보스 노드를 연결합니다.
        foreach (var path in newPaths)
        {
            int lastIndex = newPaths[0].Count - 1; // 마지막 일반 스테이지 레벨
            int lastRow = path[lastIndex];
            string lastKey = lastIndex + "_" + lastRow;
            if (nodeDict.ContainsKey(lastKey))
            {
                StageNode lastNode = nodeDict[lastKey];
                if (!lastNode.connectedNodes.Contains(bossNode))
                    lastNode.connectedNodes.Add(bossNode);
            }
        }
    }

    /// <summary>
    /// 특수 프리셋 생성 로직을 테스트합니다.
    /// </summary>
    [ContextMenu("테스트: 특수 프리셋 생성")]
    public void TestSpecialPresetGeneration()
    {
        Debug.Log("=== 특수 프리셋 190, 191, 192번 생성 테스트 시작 ===");
        
        // 특수 프리셋 초기화
        InitializeSpecialPresets();
        
        // 각 프리셋별 결과 확인
        for (int presetId = 190; presetId <= 192; presetId++)
        {
            var units = GetSpecialPresetUnits(presetId);
            int budget = presetId switch { 190 => 2000, 191 => 4500, 192 => 7000, _ => 2000 };
            
            Debug.Log($"프리셋 {presetId}번: 유닛 수 {units.Count}, 예산 {budget}");
            Debug.Log($"선택된 유닛 ID들: [{string.Join(", ", units)}]");
            
            // 예산 사용률 계산
            if (UnitLoader.Instance != null)
            {
                int totalValue = 0;
                foreach (int unitId in units)
                {
                    var unit = UnitLoader.Instance.GetUnitById(unitId);
                    if (unit != null)
                    {
                        totalValue += unit.unitPrice;
                    }
                }
                float usageRate = (float)totalValue / budget * 100f;
                Debug.Log($"예산 사용률: {usageRate:F1}% ({totalValue}/{budget})");
            }
        }
        
        Debug.Log("=== 테스트 완료 ===");
    }

    /// <summary>
    /// 특수 프리셋 저장/불러오기 테스트
    /// </summary>
    [ContextMenu("테스트: 특수 프리셋 저장/불러오기")]
    public void TestSpecialPresetSaveLoad()
    {
        Debug.Log("=== 특수 프리셋 저장/불러오기 테스트 시작 ===");
        
        // 1. 특수 프리셋 초기화
        InitializeSpecialPresets();
        
        // 2. 현재 데이터 확인
        Debug.Log("초기화된 특수 프리셋 데이터:");
        for (int presetId = 190; presetId <= 192; presetId++)
        {
            var units = GetSpecialPresetUnits(presetId);
            Debug.Log($"프리셋 {presetId}: 유닛 수 {units.Count}, 유닛 ID들: [{string.Join(", ", units)}]");
        }
        
        // 3. 저장용 데이터 생성
        var saveData = GetSpecialPresetDataForSave();
        Debug.Log($"저장용 데이터 생성 완료 - 프리셋 수: {saveData.Count}");
        
        // 4. 데이터 초기화 (불러오기 시뮬레이션)
        specialPresetUnits.Clear();
        specialPresetsInitialized = false;
        Debug.Log("데이터 초기화 완료");
        
        // 5. 저장된 데이터 로드
        LoadSpecialPresetData(saveData);
        Debug.Log("저장된 데이터 로드 완료");
        
        // 6. 로드된 데이터 확인
        Debug.Log("로드된 특수 프리셋 데이터:");
        for (int presetId = 190; presetId <= 192; presetId++)
        {
            var units = GetSpecialPresetUnits(presetId);
            Debug.Log($"프리셋 {presetId}: 유닛 수 {units.Count}, 유닛 ID들: [{string.Join(", ", units)}]");
        }
        
        Debug.Log("=== 저장/불러오기 테스트 완료 ===");
    }
}
