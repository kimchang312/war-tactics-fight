using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum StageType
{
    Combat,   // 전투
    Elite,    // 엘리트 (하층부에서는 배치 안 됨)
    Event,    // 이벤트
    Shop,     // 상점
    Rest,     // 휴식(강화)
    Treasure, // 보물
    Boss      // 보스
}

public class StageNode
{
    public int level;  // 0-indexed (예: 0: 레벨1, 14: 레벨15)
    public int row;    // 0~6 (0:A, 1:B, ... 6:G)
    public StageType stageType;
    public Color stageColor; // 스테이지 색상
    public List<StageNode> connectedNodes; // 다음 레벨과 연결된 노드 목록

    public StageNode(int level, int row, StageType stageType)
    {
        this.level = level;
        this.row = row;
        this.stageType = stageType;
        connectedNodes = new List<StageNode>();
    }

    public override string ToString()
    {
        char rowChar = (char)('A' + row);
        return $"L{level + 1}{rowChar} - {stageType}";
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

    

    void Start()
    {
        // 보스 스테이지 제외 일반 경로 생성
        GeneratePathsNonCrossing();
    }

    /// <summary>
    /// 보스 스테이지를 제외한 일반 스테이지(레벨 0~totalLevels-2)의 경로를 생성합니다.
    /// 각 레벨마다 totalPaths개의 행 값을 비내림차순으로 결정하며,
    /// 규칙에 따라 생성합니다.
    /// </summary>
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
            for (int i = 0; i < totalPaths; i++)
            {
                arr[i] = Random.Range(0, totalRows);
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
        allowed = allowed.Distinct().OrderBy(x => Random.value).ToList();
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
                        float rand = Random.Range(0f, total);
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
}
