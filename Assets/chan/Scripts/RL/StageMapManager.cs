using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class StageMapManager : MonoBehaviour
{
    public StageUIManager stageUIManager;
    public GameObject linePrefab;
    public Canvas canvas;

    public List<StageNode> allStages;
    public StageNode currentStage;

    public static Action<List<StageNode>> OnStageGenerated;  // UI에서 스테이지 리스트 받기
    public static Action<StageNode> OnStageChanged;

    // (격자 계산에 사용되는 값)
    int levels = 15;
    int totalRows = 7;
    float xSpacing = 230f;
    float ySpacing = 140f;
    float startX = -700f;
    float startY = 425f;

    // 생성 순서를 위한 카운터
    int stageCreationCounter = 0;

    private void Start()
    {
        GenerateStages();

        // ✅ allStages가 정상적으로 채워졌는지 확인
        if (allStages == null || allStages.Count == 0)
        {
            Debug.LogError("❌ allStages가 null이거나 비어 있습니다! 스테이지가 생성되지 않았습니다.");
            return;
        }

        Debug.Log($"✅ 생성된 총 스테이지 개수: {allStages.Count}");

        // ✅ 스테이지 연결이 정상적으로 실행되었는지 확인
        foreach (var stage in allStages)
        {
            if (stage.nextStages.Count == 0)
            {
                Debug.LogWarning($"⚠️ {stage.name}이(가) 연결되지 않았습니다!");
            }
        }

        // ✅ StageUIManager가 allStages를 참조하도록 초기화
        if (stageUIManager != null)
        {
            stageUIManager.InitializeUI(allStages);
        }
        else
        {
            Debug.LogError("❌ StageUIManager가 존재하지 않습니다! UI 초기화 실패");
        }
        InitializeStageStates();
    }
    public StageNode GetCurrentStage()
    {
        return currentStage;
    }
    // Helper: row index (0~6)를 'a' ~ 'g' 문자로 변환
    string GetRowLetter(int row)
    {
        return ((char)('a' + row)).ToString();
    }

    void GenerateStages()
    {
        System.Random random = new System.Random();
        int totalStagesCount = 0;


        if (allStages == null)
        {
            allStages = new List<StageNode>();
            Debug.Log("✅ allStages 리스트 초기화 완료");
        }

        // 모든 스테이지 초기 상태 설정
        foreach (var stage in allStages)
        {
            stage.SetLocked(true);
            stage.SetClickable(false);
        }

        // 각 레벨별로 스테이지 생성
        for (int level = 1; level <= levels; level++)
        {
            float xOffset = startX + (level - 1) * xSpacing;
            int stagesPerLevel = 0;
            List<int> chosenRows = new List<int>();

            if (level == 1 || level == 15)
            {
                stagesPerLevel = 1;
                if (level == 15)
                    chosenRows.Add(3); // 레벨15: 4번째 칸(인덱스3)
                else
                {
                    List<int> availableRows = Enumerable.Range(0, totalRows).ToList();
                    availableRows = availableRows.OrderBy(x => random.Next()).ToList();
                    chosenRows.Add(availableRows[0]);
                }
            }
            else
            {
                stagesPerLevel = random.Next(2, 6); // 레벨 2~14: 2~5개 생성
                List<int> availableRows = Enumerable.Range(0, totalRows).ToList();
                availableRows = availableRows.OrderBy(x => random.Next()).ToList();
                chosenRows = availableRows.Take(stagesPerLevel).ToList();
            }

            foreach (int row in chosenRows)
            {
                // 기존 방식으로 기본 좌표를 계산하되,
                // gridID는 "level-<letter>" 형식으로 결정합니다.
                float yOffset = startY - row * ySpacing;
                Vector2 position = new Vector2(xOffset, yOffset);
                string stageName = $"Stage {level}-{row + 1}";
                StageNode newStage = new StageNode(level, position, stageName);

                // gridID: 예) "1-a", "2-c", 등
                newStage.gridID = $"{level}-{GetRowLetter(row)}";

                // 생성 순서 할당
                newStage.creationIndex = stageCreationCounter++;

                newStage.SetLocked(true);
                newStage.SetClickable(false);

                allStages.Add(newStage);
                totalStagesCount++;

                // 초기 스테이지(레벨1의 첫 스테이지) 설정
                if (level == 1 && currentStage == null)
                {
                    currentStage = newStage;
                    currentStage.isCleared = true;
                    currentStage.SetLocked(false);
                    currentStage.SetClickable(true);
                }
            }
        }

        Debug.Log($"[GenerateStages] 생성된 총 스테이지 수: {totalStagesCount}");
        if (currentStage == null)
        {
            Debug.LogError("❌ currentStage가 설정되지 않았습니다. 레벨 1의 첫 번째 스테이지를 확인하세요.");
            return;
        }
        Debug.Log($"🟢 총 생성된 스테이지 개수: {allStages.Count}");

        OnStageGenerated?.Invoke(allStages);

        // 생성 순서를 반영한 연결 규칙 (예시로 기존 로직에 creationIndex 보조 조건 추가)
        ConnectStages();
        InitializeStageStates();
    }



void InitializeStageStates()
    {
        foreach (var stage in allStages)
        {
            stage.SetLocked(true);
            stage.SetClickable(false);
        }

        currentStage.SetLocked(false);
        currentStage.SetClickable(true);

        if (currentStage.nextStages != null && currentStage.nextStages.Count > 0)
        {
            foreach (var nextStage in currentStage.nextStages)
            {
                nextStage.SetLocked(false);
                nextStage.SetClickable(true);
            }
        }
        else
        {
            Debug.LogError("❌ 시작 스테이지에 연결된 스테이지가 없습니다! 스테이지 생성 로직을 확인하세요.");
        }
    }
    
    public void MoveToStage(StageNode newStage)
    {
        if (newStage == null)
        {
            Debug.LogError("❌ MoveToStage() 호출 실패: newStage가 null입니다!");
            return;
        }

        if (currentStage == null)
        {
            Debug.LogError("❌ currentStage가 null 상태입니다. 초기화 로직을 확인하세요.");
            return;
        }

        if (!currentStage.nextStages.Contains(newStage))
        {
            Debug.Log($"🛑 이동 불가: {currentStage.name}에서 {newStage.name}으로 이동할 수 없습니다! (연결되지 않은 스테이지)");
            return;
        }

        Debug.Log($"✅ 스테이지 이동: {currentStage.name} → {newStage.name}");

        currentStage.SetCleared(true);
        currentStage = newStage;

        foreach (var stage in allStages)
        {
            stage.SetLocked(true);
            stage.SetClickable(false);
        }

        currentStage.SetLocked(false);
        currentStage.SetClickable(true);

        foreach (var nextStage in currentStage.nextStages)
        {
            nextStage.SetLocked(false);
            nextStage.SetClickable(true);
        }
        // ✅ `newStage`가 null이 아닌 경우만 호출
    if (stageUIManager != null && newStage != null)
    {
        Debug.Log($"🟢 StageUIManager.UpdateStageUI() 호출: {newStage.name}");
        stageUIManager.UpdateStageUI(newStage);
    }
    else
    {
        Debug.LogError("❌ StageUIManager 또는 newStage가 null입니다!");
    }
        OnStageChanged?.Invoke(newStage);
    }

    //다음 스테이지와의 연결을 표시하는 선을 표시하는 메서드 -미적용중
    void DrawConnection(StageNode fromStage, StageNode toStage)
    {
        // LineRenderer 프리팹이 없을 경우 처리
        if (linePrefab == null)
        {
            Debug.LogError("❌ LineRenderer 프리팹이 할당되지 않았습니다. Inspector에서 확인하세요.");
            return;
        }

        // LineRenderer 프리팹 생성
        GameObject lineObj = Instantiate(linePrefab, transform);
        LineRenderer line = lineObj.GetComponent<LineRenderer>();

        // LineRenderer의 정렬 옵션 설정
        line.sortingLayerName = "UI"; // UI와 동일한 Sorting Layer 사용
        line.sortingOrder = 1;        // UI 요소보다 위에 표시되도록 설정

        // LineRenderer 선 두께 설정
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;

        // 스테이지 버튼 위치 계산 (Screen Space - Overlay에서는 localPosition 그대로 사용)
        Vector3 fromPosition = new Vector3(fromStage.position.x, fromStage.position.y, 0f);
        Vector3 toPosition = new Vector3(toStage.position.x, toStage.position.y, 0f);

        // LineRenderer의 위치 설정
        line.positionCount = 2;
        line.SetPosition(0, fromPosition);
        line.SetPosition(1, toPosition);
    }

    void ConnectStages()
    {
        Debug.Log("🔵 스테이지 연결 시작");
        int levelsCount = 15; // 생성된 레벨 수 (1~15)
        for (int level = 1; level < levelsCount; level++)
        {
            List<StageNode> currentLevelStages = allStages.FindAll(stage => stage.level == level);
            List<StageNode> nextLevelStages = allStages.FindAll(stage => stage.level == level + 1);

            if (nextLevelStages.Count == 0)
            {
                if (level < levelsCount - 1) // ✅ 15레벨 이후는 디버그 로그 출력 안 함
                {
                    Debug.LogWarning($"⚠️ 레벨 {level}에 연결할 다음 레벨 스테이지가 없습니다!");
                }
                continue;
            }

            // 필수 연결: 같은 gridID 우선, 그 외 생성 순서에 따라 가장 가까운 스테이지 연결
            foreach (StageNode cur in currentLevelStages)
            {
                if (cur.nextStages.Count == 0)
                {
                    // 우선, 다음 레벨 중 gridID가 같은(즉, 같은 칸) 스테이지를 찾음
                    StageNode candidate = nextLevelStages.Find(s => s.gridID == cur.gridID);
                    if (candidate == null)
                    {
                        // 없다면 생성 순서(creationIndex)와 거리를 함께 고려해서 후보 선택
                        candidate = nextLevelStages
                                    .OrderBy(s => Vector2.Distance(cur.position, s.position))
                                    .ThenBy(s => s.creationIndex)
                                    .FirstOrDefault();
                    }
                    if (candidate != null)
                    {
                        cur.nextStages.Add(candidate);
                        candidate.previousStages.Add(cur);
                        Debug.Log($"✅ {cur.name} → {candidate.name} 연결됨 (필수 연결)");
                    }
                }
            }
            // 추가 연결: 각 스테이지에 대해 추가 0~2 연결 (필요 시)
            foreach (StageNode cur in currentLevelStages)
            {
                int additionalConnections = UnityEngine.Random.Range(0, 3);
                for (int i = 0; i < additionalConnections; i++)
                {
                    StageNode randomNext = nextLevelStages[UnityEngine.Random.Range(0, nextLevelStages.Count)];
                    if (!cur.nextStages.Contains(randomNext))
                    {
                        cur.nextStages.Add(randomNext);
                        randomNext.previousStages.Add(cur);
                        Debug.Log($"✅ {cur.name} → {randomNext.name} 추가 연결됨");
                    }
                }
            }
        }
        Debug.Log("🟢 스테이지 연결 완료");
    }

}

