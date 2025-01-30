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

    public List<StageNode> allStages = new List<StageNode>();
    public StageNode currentStage;

    public static Action<List<StageNode>> OnStageGenerated;  // UI에서 스테이지 리스트 받기
    public static Action<StageNode> OnStageChanged;


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

    void GenerateStages()
    {
        int levels = 15; // 총 레벨 수
        float xSpacing = 300f; // 레벨 간 X축 간격
        float ySpacing = 200f; // 스테이지 간 Y축 간격
        float startX = -700f; // X축 시작 위치
        float startY = 425f; // Y축 시작 위치

        System.Random random = new System.Random();
        int totalStages = 0;

        // allStages 리스트 초기화 확인
        if (allStages == null)
        {
            allStages = new List<StageNode>();
            Debug.Log("✅ allStages 리스트 초기화 완료");
        }
        // 모든 스테이지를 클릭 불가능하도록 설정
        foreach (var stage in allStages)
        {
            stage.SetLocked(true);
            stage.SetClickable(false);
        }
        // 스테이지 생성
        for (int level = 1; level <= levels; level++)
        {
            float xOffset = startX + (level - 1) * xSpacing; // X축 위치 (레벨별)
            int stagesPerLevel = (level == 1) ? 1 : random.Next(2, 6); // 레벨 1은 스테이지 1개 고정, 나머지는 2~5개의 스테이지 생성
            
            for (int i = 0; i < stagesPerLevel; i++)
            {
                float yOffset = startY - i * ySpacing; // Y축 위치 (스테이지별)
                Vector2 position = new Vector2(xOffset, yOffset); // 위치 설정
                string stageName = $"Stage {level}-{i + 1}";
                StageNode newStage = new StageNode(level, position, stageName);

                newStage.SetLocked(true); // 기본적으로 모든 스테이지를 잠금 상태로 설정
                newStage.SetClickable(false); // ✅ 모든 스테이지 기본적으로 클릭 불가능하도록 설정

                allStages.Add(newStage);
                totalStages++;

                if (level == 1 && i == 0)
                {
                    currentStage = newStage;
                    currentStage.isCleared = true; // ✅ 초기 스테이지 방문 상태 설정
                    currentStage.SetLocked(false);
                    currentStage.SetClickable(true);
                }
            }
        }
        Debug.Log($"[GenerateStages] 생성된 총 스테이지 수: {totalStages}");

        if (currentStage == null)
        {
            Debug.LogError("❌ currentStage가 설정되지 않았습니다. 레벨 1의 첫 번째 스테이지를 확인하세요.");
            return;
        }

        Debug.Log($"[GenerateStages] 총 생성된 스테이지 수: {totalStages}");

        /* 스테이지 연결
        for (int level = 1; level < levels; level++)
        {
            List<StageNode> currentLevelStages = allStages.FindAll(stage => stage.level == level);
            List<StageNode> nextLevelStages = allStages.FindAll(stage => stage.level == level + 1);

            foreach (StageNode stage in currentLevelStages)
            {
                ConnectToNextStagesWithRules(stage, nextLevelStages);
            }
        }*/

        Debug.Log($"🟢 총 생성된 스테이지 개수: {allStages.Count}");

        // UI 업데이트를 위한 이벤트 호출
        OnStageGenerated?.Invoke(allStages);
        // ✅ 스테이지 연결을 여기서 호출
        ConnectStages();

        // ✅ 스테이지 상태 초기화
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

    void ConnectToLevel2(StageNode currentStage, List<StageNode> nextLevelStages)
    {
        foreach (StageNode nextStage in nextLevelStages)
        {
            // 레벨 1 스테이지에서 레벨 2로만 연결
            currentStage.nextStages.Add(nextStage);
            nextStage.previousStages.Add(currentStage);
            DrawConnection(currentStage, nextStage);
        }
    }
    void ConnectToNextStagesWithRules(StageNode currentStage, List<StageNode> nextLevelStages)
    {
        int maxConnections = 3;
        int currentConnections = 0;

        foreach (StageNode nextStage in nextLevelStages.OrderBy(stage => Vector2.Distance(currentStage.position, stage.position)))
        {
            if (currentConnections >= maxConnections)
                break;

            if (!currentStage.nextStages.Contains(nextStage))
            {
                currentStage.nextStages.Add(nextStage);
                nextStage.previousStages.Add(currentStage);
                DrawConnection(currentStage, nextStage);
                currentConnections++;
            }
        }
    }

    StageNode FindClosestStage(List<StageNode> currentLevelStages, StageNode targetStage)
    {
        StageNode closestStage = null;
        float closestDistance = float.MaxValue;

        foreach (StageNode stage in currentLevelStages)
        {
            float distance = Vector2.Distance(stage.position, targetStage.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestStage = stage;
            }
        }

        return closestStage;
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

        for (int level = 1; level < 15; level++)
        {
            List<StageNode> currentLevelStages = allStages.FindAll(stage => stage.level == level);
            List<StageNode> nextLevelStages = allStages.FindAll(stage => stage.level == level + 1);

            if (nextLevelStages.Count == 0)
            {
                if (level < 14) // ✅ 15레벨 이후는 디버그 로그 출력 안 함
                {
                    Debug.LogWarning($"⚠️ 레벨 {level}에 연결할 다음 레벨 스테이지가 없습니다!");
                }
                continue;
            }

            foreach (StageNode stage in currentLevelStages)
            {
                int connections = UnityEngine.Random.Range(1, Mathf.Min(4, nextLevelStages.Count + 1)); // ✅ 1~3개 랜덤 연결
                HashSet<StageNode> connectedStages = new HashSet<StageNode>();

                while (connectedStages.Count < connections)
                {
                    StageNode nextStage = nextLevelStages[UnityEngine.Random.Range(0, nextLevelStages.Count)];
                    if (!connectedStages.Contains(nextStage))
                    {
                        stage.nextStages.Add(nextStage);
                        nextStage.previousStages.Add(stage);
                        connectedStages.Add(nextStage);
                        Debug.Log($"✅ {stage.name} → {nextStage.name} 연결됨.");
                    }
                }
            }
        }

        Debug.Log("🟢 스테이지 연결 완료");
    }

}

