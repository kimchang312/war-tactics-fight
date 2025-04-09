using System.Collections.Generic;
using UnityEngine;
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
    public int totalLevels = 15;   // 예: 15면, 레벨1~14는 일반, 레벨15는 보스
    public int totalRows = 7;      // A~G (인덱스 0~6)
    public int totalPaths = 6;     // 생성할 경로 수

    // 일반 스테이지 경로: 각 경로는 각 레벨(보스 스테이지 제외)에서의 행값 List<int>
    private List<List<int>> paths = new List<List<int>>();
    // 일반 스테이지 노드를 "level_row" 문자열 키로 저장 (레벨 0 ~ totalLevels-2)
    private Dictionary<string, StageNode> nodeDict = new Dictionary<string, StageNode>();
    public Dictionary<string, StageNode> NodeDictionary { get { return nodeDict; } }

    void Start()
    {
        // 보스 스테이지를 제외한 일반 스테이지 경로 생성
        GeneratePathsNonCrossing();
    }

    /// <summary>
    /// 보스 스테이지(마지막 레벨)를 제외한 일반 스테이지(레벨 0~totalLevels-2) 경로를 생성합니다.
    /// 경로는 각 레벨마다 totalPaths개의 행 값을 결정하는데, 
    /// - 각 레벨의 값은 비내림차순(즉, 교차 안 함)으로 생성됩니다.
    /// - 하층부(1~7레벨, 1-indexed)에서는 엘리트(stageType.Elite)가 허용되지 않습니다.
    /// </summary>
    public void GeneratePathsNonCrossing()
    {
        int normalLevels = totalLevels - 1; // 보스 스테이지 제외한 레벨 수

        List<int[]> matrix = new List<int[]>();

        // 1. 레벨 0: 시작 위치 생성
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

        // 일반 스테이지 노드를 생성한 후, 보스 스테이지를 단일 노드로 생성 및 연결
        BuildFinalMap(newPaths, normalLevels);
    }

    /// <summary>
    /// 레벨 0의 시작 위치를 생성합니다.
    /// totalPaths개의 랜덤 값을 생성하여 오름차순 정렬한 후,
    /// 최소 2개 이상의 값이 다른 배열을 반환합니다.
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
    /// 레벨 1부터 normalLevels-1까지(일반 스테이지) 재귀적으로 경로 배열을 생성합니다.
    /// 이전 레벨 배열을 기반으로 각 경로의 값은 인접 이동(현재, -1, +1) 후보 중에서 결정되며,
    /// 선택된 값들이 비내림차순으로 유지되어 경로 간 교차를 방지합니다.
    /// </summary>
    /// <param name="level">현재 처리할 레벨 (1부터 normalLevels-1까지)</param>
    /// <param name="matrix">이전 레벨까지 채워진 경로 행렬</param>
    /// <param name="normalLevels">보스 스테이지를 제외한 일반 레벨 수</param>
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

    /// <summary>
    /// 현재 레벨의 경로 배열을 재귀적으로 채웁니다.
    /// 각 경로 i에 대해 가능한 후보는 prev[i]-1, prev[i], prev[i]+1이며,
    /// 후보들은 무작위로 섞인 후, 이전 경로(동일 레벨 내 왼쪽 경로)의 값보다 작지 않은 값만 채택합니다.
    /// </summary>
    /// <param name="index">현재 처리할 경로 인덱스</param>
    /// <param name="last">현재까지 선택된 값 중 마지막 값 (비내림차순 유지 기준)</param>
    /// <param name="prev">이전 레벨의 경로 배열</param>
    /// <param name="current">채워질 현재 레벨의 경로 배열</param>
    /// <param name="currentLevel">현재 레벨 번호 (일반 레벨 1~normalLevels-1)</param>
    private bool GenerateCurrentLevelRec(int index, int last, int[] prev, int[] current, int currentLevel)
    {
        if (index == totalPaths)
            return true;

        List<int> allowed = new List<int>();
        int baseVal = prev[index];
        if (baseVal - 1 >= 0) allowed.Add(baseVal - 1);
        allowed.Add(baseVal);
        if (baseVal + 1 < totalRows) allowed.Add(baseVal + 1);
        // 후보들을 무작위 섞음
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
    /// 생성된 일반 스테이지 경로(newPaths) 정보를 바탕으로 최종 맵 데이터를 구성합니다.
    /// 1) 각 경로에 대해, 레벨 0부터 일반 스테이지 레벨까지 StageNode를 생성하는데, 
    ///    이때 스테이지 타입은 다음 규칙에 따라 결정됩니다:
    ///    - 레벨 0: 무조건 전투 (Combat)
    ///    - 하층부(1~7 레벨; 1-indexed)에서는 엘리트(Elite) stage는 배치되지 않음
    ///    - 엘리트(Elite), 상점(Shop), 휴식(Rest) 스테이지는 연속 배치되지 않음 (이전 스테이지와 같으면 제거)
    ///    - 플레이어의 갈림길에서는 같은 스테이지 종류가 반복되지 않도록 (이 부분은 경로 생성 단계에서 이미 어느 정도 분리됨)
    ///    - 레벨 7: Treasure (보물)
    ///    - 레벨 13: Rest (휴식)는 배치되지 않도록 허용 타입에서 제외하고 랜덤 선택
    ///    - 그 외의 레벨: 조건에 따른 랜덤 선택 (허용 타입에 따라)
    /// 2) 생성된 일반 스테이지 노드들을 연속해서 연결한 후,
    /// 3) 별도의 보스 스테이지 노드를 생성(레벨 = normalLevels, row = 3으로 고정)하고,
    ///    각 경로의 마지막 일반 스테이지 노드와 연결합니다.
    /// </summary>
    /// <param name="newPaths">생성된 일반 스테이지 경로들</param>
    /// <param name="normalLevels">보스 스테이지 제외한 일반 레벨 수</param>
    void BuildFinalMap(List<List<int>> newPaths, int normalLevels)
    {
        nodeDict.Clear();

        // 1) 일반 스테이지 노드 생성 (레벨 0 ~ normalLevels-1)
        // 여기서 normalLevels = totalLevels - 1 (보스 스테이지 제외)
        foreach (var path in newPaths)
        {
            for (int lvl = 0; lvl < newPaths[0].Count; lvl++)
            {
                int row = path[lvl];
                string key = lvl + "_" + row;
                if (!nodeDict.ContainsKey(key))
                {
                    StageType type;
                    // 고정 스테이지 배치 규칙 적용 (1-indexed 기준)
                    if (lvl == 0)
                    {
                        // 레벨 1: 일반 몬스터 스테이지 → Combat
                        type = StageType.Combat;
                    }
                    else if (lvl == 7)
                    {
                        // 레벨 8: 보물 스테이지
                        type = StageType.Treasure;
                    }
                    else if (lvl == 13)
                    {
                        // 레벨 14: 휴식 스테이지
                        type = StageType.Rest;
                    }
                    else if (lvl == newPaths[0].Count - 1)
                    {
                        // 이 부분은 보스 스테이지는 별도로 처리하므로 여기서는 일반 노드 생성 X.
                        // (보스는 BuildFinalMap() 하단에서 생성하여 연결합니다.)
                        continue;
                    }
                    else
                    {
                        // 일반 레벨에 대해 무작위 배치
                        // 추가 조건: 하층부(레벨 2~7; index 1~6)에서는 Elite, Shop, Rest 배제.
                        // 또한, 연속 배치를 피하기 위해 이전 노드와 같은 종류가 있으면 제거.
                        List<StageType> allowed;
                        if (lvl < 7)
                        {
                            allowed = new List<StageType> { StageType.Combat, StageType.Event, StageType.Treasure };
                        }
                        else if (lvl == 12)
                        {
                            // 레벨 13: Rest는 배치되지 않도록 (allowed에서 Rest 제거)
                            allowed = new List<StageType> { StageType.Combat, StageType.Elite, StageType.Event, StageType.Shop, StageType.Treasure };
                        }
                        else
                        {
                            allowed = new List<StageType> { StageType.Combat, StageType.Elite, StageType.Event, StageType.Shop, StageType.Rest, StageType.Treasure };
                        }

                        // 연속 배치 제한: 만약 이전 레벨의 노드가 Elite, Shop, Rest라면 동일한 타입을 제거
                        if (lvl - 1 >= 0)
                        {
                            string prevKey = (lvl - 1) + "_" + path[lvl - 1];
                            if (nodeDict.ContainsKey(prevKey))
                            {
                                StageType prevType = nodeDict[prevKey].stageType;
                                if (prevType == StageType.Elite || prevType == StageType.Shop || prevType == StageType.Rest)
                                {
                                    allowed.Remove(prevType);
                                }
                            }
                        }
                        // 선택 가능한 타입들 중 무작위로 결정
                        type = allowed[Random.Range(0, allowed.Count)];
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
        // 보스 노드는 보통 맵의 마지막 레벨, 즉 normalLevels (totalLevels-1)에서 생성됩니다.
        // 여기서는 고정 행(예: 3번, D열)로 생성합니다.
        StageNode bossNode = new StageNode(normalLevels, 3, StageType.Boss);
        string bossKey = normalLevels + "_3";
        nodeDict[bossKey] = bossNode;

        // 각 경로의 마지막 일반 스테이지 노드와 보스 노드를 연결합니다.
        foreach (var path in newPaths)
        {
            int lastIndex = newPaths[0].Count - 1; // 마지막 일반 스테이지 레벨 (예: 13 if totalLevels is 15)
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
