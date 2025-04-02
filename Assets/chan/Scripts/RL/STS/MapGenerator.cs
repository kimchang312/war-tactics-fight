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
    public int level;  // 0-indexed (예: 0: 레벨1, 14: 레벨15)
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
    // totalLevels는 보스 스테이지를 포함한 전체 레벨 수.
    public int totalLevels = 15;   // 예: 15이면, 레벨1~14는 일반 스테이지, 레벨15는 보스 스테이지
    public int totalRows = 7;      // A ~ G (인덱스 0~6)
    public int totalPaths = 6;     // 생성할 경로 수

    // 생성된 일반 스테이지 경로 정보: 각 경로는 각 레벨의 행값(List<int>)로 표현 (레벨 0 ~ totalLevels-2)
    private List<List<int>> paths = new List<List<int>>();
    // 일반 스테이지 노드들을 (레벨_행 문자열)을 key로 저장 (레벨 0~totalLevels-2)
    private Dictionary<string, StageNode> nodeDict = new Dictionary<string, StageNode>();
    public Dictionary<string, StageNode> NodeDictionary { get { return nodeDict; } }

    void Start()
    {
        // 일반 스테이지(보스 제외) 경로 생성
        GeneratePathsNonCrossing();
    }

    /// <summary>
    /// 보스 스테이지 제외한 일반 스테이지(레벨 0~totalLevels-2)를 생성합니다.
    /// 각 레벨마다 totalPaths개의 경로(행 값)를 비내림차순(정렬된)으로 결정하며,
    /// 경로 생성 규칙은 기존과 동일합니다.
    /// </summary>
    public void GeneratePathsNonCrossing()
    {
        // normalLevels: 보스 스테이지를 제외한 레벨 수.
        int normalLevels = totalLevels - 1;

        List<int[]> matrix = new List<int[]>();

        // 1. 레벨 0: 시작 위치 생성 (랜덤하게 totalPaths개의 값을 생성 후 오름차순 정렬)
        int[] level0 = GenerateStartingPositions();
        if (level0 == null)
        {
            Debug.LogError("시작 위치 생성에 실패했습니다.");
            return;
        }
        matrix.Add(level0);

        // 2. 레벨 1부터 normalLevels-1까지 순차적으로 생성
        if (!GenerateLevel(1, matrix, normalLevels))
        {
            Debug.LogError("비교차 경로 생성에 실패했습니다.");
            return;
        }

        // 3. 생성된 행렬(각 행: 일반 스테이지 레벨, 열: 경로)을 열 단위로 분리하여 paths에 저장
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

        // 일반 스테이지 노드를 구성한 후, 보스 스테이지를 하나 생성하여 연결합니다.
        BuildFinalMap(newPaths, normalLevels);
    }

    /// <summary>
    /// 레벨 0의 시작 위치를 생성합니다.
    /// totalPaths개의 랜덤 값을 생성하여 오름차순 정렬한 후,
    /// 모두 동일하지 않은(최소 2개 이상의 값이 다른) 배열을 반환합니다.
    /// </summary>
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

    /// <summary>
    /// 재귀적으로 현재 레벨의 경로 배열을 생성하여 matrix에 추가합니다.
    /// 이전 레벨의 배열(matrix의 마지막 요소)을 기반으로, 각 경로별로 인접 이동(현재, 위, 아래) 후보 중에서
    /// 비내림차순(정렬된) 배열을 생성합니다.
    /// </summary>
    /// <param name="level">현재 처리할 레벨 (1부터 normalLevels-1까지)</param>
    /// <param name="matrix">이전 레벨까지 채워진 행렬</param>
    /// <param name="normalLevels">보스 제외한 일반 스테이지 레벨 수</param>
    private bool GenerateLevel(int level, List<int[]> matrix, int normalLevels)
    {
        if (level >= normalLevels)
            return true; // 보스 스테이지 제외, 일반 레벨만 생성

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

    /// <summary>
    /// 현재 레벨의 경로 배열(current)을 재귀적으로 채웁니다.
    /// 각 경로 i의 후보는 기본적으로 prev[i]의 인접 값({prev[i]-1, prev[i], prev[i]+1})을 사용합니다.
    /// 단, 비내림차순 조건(후보는 이전 경로의 선택(last) 이상)은 항상 유지합니다.
    /// (일반 레벨에서는 이 규칙을 적용하며, 별도 추가 제약은 없음)
    /// </summary>
    private bool GenerateCurrentLevelRec(int index, int last, int[] prev, int[] current, int currentLevel)
    {
        if (index == totalPaths)
            return true;

        List<int> allowed = new List<int>();
        int baseVal = prev[index];
        if (baseVal - 1 >= 0) allowed.Add(baseVal - 1);
        allowed.Add(baseVal);
        if (baseVal + 1 < totalRows) allowed.Add(baseVal + 1);
        // 후보 순서를 무작위로 섞기
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
    /// 생성된 일반 스테이지 경로(newPaths) 정보를 기반으로 최종 맵 데이터를 구성합니다.
    /// 1) 일반 스테이지 노드들(레벨 0 ~ normalLevels-1)을 생성 및 연결하고,
    /// 2) 별도로 보스 스테이지(단일 노드)를 생성하여, 각 경로의 마지막 노드(레벨 normalLevels-1)와 연결합니다.
    /// 스테이지 타입은 레벨별 규칙에 따라 부여합니다.
    /// </summary>
    /// <param name="newPaths">생성된 경로들</param>
    /// <param name="normalLevels">보스 제외한 일반 스테이지 레벨 수</param>
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
                    StageType type = StageType.Combat;
                    if (lvl == 0)
                        type = StageType.Combat;
                    else if (lvl == 7)
                        type = StageType.Treasure;
                    else if (lvl == 13)
                        type = StageType.Rest;
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

        // 3) 보스 스테이지(단일 노드) 생성 및 연결
        // 보스 노드는 레벨 = normalLevels (즉, totalLevels-1)에 생성됩니다.
        StageNode bossNode = new StageNode(normalLevels, 3, StageType.Boss); // 보스는 원하는 셀(여기서는 3, 즉 D)
        string bossKey = normalLevels + "_3";
        nodeDict[bossKey] = bossNode;

        // 각 경로의 마지막 노드(레벨 normalLevels-1)와 보스 노드를 연결합니다.
        foreach (var path in newPaths)
        {
            int lastRow = path[newPaths[0].Count - 1];
            string lastKey = (newPaths[0].Count - 1) + "_" + lastRow;
            if (nodeDict.ContainsKey(lastKey))
            {
                StageNode lastNode = nodeDict[lastKey];
                if (!lastNode.connectedNodes.Contains(bossNode))
                    lastNode.connectedNodes.Add(bossNode);
            }
        }
    }
}
