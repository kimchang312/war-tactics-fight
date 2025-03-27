using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum StageType
{
    Combat,   // 전투
    Event,    // 이벤트
    Shop,     // 상점
    Rest,     // 휴식(강화)
    Treasure, // 보물
    Boss      // 보스
}

public class StageNode
{
    public int level;  // 0-indexed (0: 레벨1, 14: 레벨15)
    public int row;    // 0~6 (0:A, 1:B, ... 6:G)
    public StageType stageType;
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
    public int totalLevels = 15;   // 레벨 1 ~ 15
    public int totalRows = 7;      // A ~ G
    public int totalPaths = 6;     // 생성할 경로 수

    // 이미 생성된 경로들의 연결정보 (key: 현재 레벨, value: (출발행, 도착행) 쌍 목록)
    private Dictionary<int, List<(int, int)>> globalConnections = new Dictionary<int, List<(int, int)>>();
    // 각 경로는 각 레벨에서 선택한 행(0~6) 정보를 저장한 리스트
    private List<List<int>> paths = new List<List<int>>();
    // 최종 맵의 노드를 (레벨_행 문자열)을 key로 저장
    private Dictionary<string, StageNode> nodeDict = new Dictionary<string, StageNode>();

    // 외부에서 접근할 수 있도록 프로퍼티 제공 (예: UIGenerator에서 사용)
    public Dictionary<string, StageNode> NodeDictionary { get { return nodeDict; } }

    /// <summary>
    /// Map 데이터를 생성합니다.
    /// 경로 생성 후, 최종 노드들을 생성하고 각 노드들을 연결합니다.
    /// </summary>
    public void GenerateMap()
    {
        paths.Clear();
        globalConnections.Clear();

        int pathCount = 0;
        int attempts = 0;
        while (pathCount < totalPaths && attempts < 1000)
        {
            attempts++;
            List<int> newPath = new List<int>();
            // 첫 경로는 아무 칸이나, 두 번째 경로는 첫 경로와 다른 시작 칸으로 선택
            int start;
            if (pathCount == 1 && paths.Count > 0)
            {
                List<int> possibleStarts = new List<int>();
                for (int r = 0; r < totalRows; r++)
                {
                    if (r != paths[0][0])
                        possibleStarts.Add(r);
                }
                start = possibleStarts[Random.Range(0, possibleStarts.Count)];
            }
            else
            {
                start = Random.Range(0, totalRows);
            }
            newPath.Add(start);
            List<(int, int)> localTransitions = new List<(int, int)>();

            if (GeneratePathRecursive(0, start, newPath, localTransitions))
            {
                // 경로 완성 시, 현재 경로의 각 단계 연결 정보를 globalConnections에 기록
                foreach (var trans in localTransitions)
                {
                    int lvl = trans.Item1;
                    if (!globalConnections.ContainsKey(lvl))
                        globalConnections[lvl] = new List<(int, int)>();
                    globalConnections[lvl].Add(trans);
                }
                paths.Add(new List<int>(newPath));
                pathCount++;
            }
        }
        if (pathCount < totalPaths)
        {
            Debug.LogError("제약조건에 맞는 경로를 모두 생성하지 못했습니다.");
        }
        else
        {
            Debug.Log("총 " + totalPaths + "개의 경로를 생성했습니다.");
            for (int i = 0; i < paths.Count; i++)
            {
                string pathStr = "경로 " + (i + 1) + ": ";
                for (int lvl = 0; lvl < paths[i].Count; lvl++)
                {
                    char rowChar = (char)('A' + paths[i][lvl]);
                    pathStr += "L" + (lvl + 1) + rowChar + " ";
                }
                Debug.Log(pathStr);
            }
        }
        BuildFinalMap();
    }

    /// <summary>
    /// 재귀적으로 경로를 생성합니다.
    /// 현재 레벨의 현재 행에서 다음 레벨의 인접 칸(수평, 위 대각, 아래 대각) 중 하나를 선택합니다.
    /// 마지막 레벨(15레벨)에서는 무조건 D칸(인덱스 3)으로 연결되어야 합니다.
    /// 또한, globalConnections를 참고하여 교차가 발생하지 않도록 후보를 필터링합니다.
    /// </summary>
    /// <param name="level">현재 레벨 (0부터 시작)</param>
    /// <param name="currentRow">현재 레벨의 행</param>
    /// <param name="newPath">현재까지 생성된 경로 (각 레벨의 행)</param>
    /// <param name="localTransitions">현재 경로 내의 (출발행, 도착행) 연결 목록</param>
    /// <returns>경로 생성 성공 여부</returns>
    bool GeneratePathRecursive(int level, int currentRow, List<int> newPath, List<(int, int)> localTransitions)
    {
        if (level == totalLevels - 1) // 15레벨 도달
            return true;

        int nextLevel = level + 1;
        List<int> candidates = new List<int>();

        if (nextLevel == totalLevels - 1)
        {
            // 마지막 레벨: 무조건 D칸(인덱스 3)이어야 함.
            if (Mathf.Abs(currentRow - 3) <= 1)
                candidates.Add(3);
        }
        else
        {
            // 일반 레벨: 현재 칸, 한 칸 위, 한 칸 아래
            candidates.Add(currentRow);
            if (currentRow - 1 >= 0)
                candidates.Add(currentRow - 1);
            if (currentRow + 1 < totalRows)
                candidates.Add(currentRow + 1);
            candidates = candidates.Distinct().ToList();
        }

        // globalConnections에 기록된 같은 레벨의 기존 경로와 비교해 교차가 발생하지 않도록 후보를 제한
        List<int> validCandidates = new List<int>();
        foreach (int candidate in candidates)
        {
            bool valid = true;
            if (globalConnections.ContainsKey(level))
            {
                foreach (var conn in globalConnections[level])
                {
                    int existingSource = conn.Item1;
                    int existingTarget = conn.Item2;
                    // 만약 현재 출발칸이 기존 연결의 출발칸보다 위라면, 후보는 기존 연결의 도착칸보다 위(또는 같아야)
                    if (currentRow < existingSource && candidate > existingTarget)
                    {
                        valid = false;
                        break;
                    }
                    // 만약 현재 출발칸이 기존 연결의 출발칸보다 아래라면, 후보는 기존 연결의 도착칸보다 아래(또는 같아야)
                    if (currentRow > existingSource && candidate < existingTarget)
                    {
                        valid = false;
                        break;
                    }
                }
            }
            if (valid)
                validCandidates.Add(candidate);
        }

        // 후보들을 무작위 순서로 섞습니다.
        validCandidates = validCandidates.OrderBy(x => Random.value).ToList();

        foreach (int candidate in validCandidates)
        {
            newPath.Add(candidate);
            localTransitions.Add((currentRow, candidate));
            if (GeneratePathRecursive(nextLevel, candidate, newPath, localTransitions))
                return true;

            // 실패 시 백트래킹
            newPath.RemoveAt(newPath.Count - 1);
            localTransitions.RemoveAt(localTransitions.Count - 1);
        }
        return false;
    }

    /// <summary>
    /// 생성된 경로들을 기반으로 최종 맵 데이터를 구성합니다.
    /// 각 경로의 각 레벨에 해당하는 노드를 생성하며, 이미 존재하는 노드는 중복 생성하지 않습니다.
    /// 그리고 각 경로에서 연속된 노드들을 서로 연결합니다.
    /// 스테이지 타입은 레벨별 규칙에 따라 부여합니다.
    /// </summary>
    void BuildFinalMap()
    {
        nodeDict.Clear();
        // 각 경로의 노드를 생성 (중복 없이)
        foreach (var path in paths)
        {
            for (int lvl = 0; lvl < path.Count; lvl++)
            {
                int row = path[lvl];
                string key = lvl + "_" + row;
                if (!nodeDict.ContainsKey(key))
                {
                    StageType type = StageType.Combat;
                    if (lvl == 0)
                        type = StageType.Combat;
                    else if (lvl == 7)
                        type = StageType.Treasure;
                    else if (lvl == 13)
                        type = StageType.Rest;
                    else if (lvl == totalLevels - 1)
                        type = StageType.Boss;
                    else
                    {
                        int rnd = Random.Range(0, 3);
                        if (rnd == 0)
                            type = StageType.Combat;
                        else if (rnd == 1)
                            type = StageType.Event;
                        else
                            type = StageType.Shop;
                    }
                    StageNode node = new StageNode(lvl, row, type);
                    nodeDict[key] = node;
                }
            }
        }
        // 각 경로의 연속된 노드들을 연결
        foreach (var path in paths)
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
    }
}
