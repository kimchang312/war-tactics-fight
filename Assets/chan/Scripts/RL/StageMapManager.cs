using System;
using System.Collections.Generic;
using UnityEngine;

public class StageMapManager : MonoBehaviour
{
    public GameObject linePrefab;
    public Canvas canvas;

    public List<StageNode> allStages = new List<StageNode>();
    public StageNode currentStage;

    public static Action<List<StageNode>> OnStageGenerated;  // UI에서 스테이지 리스트 받기
    public static Action<StageNode> OnStageChanged;


    private void Start()
    {
        // ✅ Start()에서 실행하여 OnEnable() 이후에 동작하도록 함
        Invoke(nameof(GenerateStages), 0.1f);
    }
    public StageNode GetCurrentStage()
    {
        return currentStage;
    }

    void GenerateStages()
    {
        int levels = 15; // 총 레벨 수
        int stagesPerLevel = 5; // 각 레벨당 스테이지 수
        float xSpacing = 300f; // 레벨 간 X축 간격
        float ySpacing = 200f; // 스테이지 간 Y축 간격
        float startX = -700f; // X축 시작 위치
        float startY = 425f; // Y축 시작 위치

        // 스테이지 생성
        for (int level = 1; level <= levels; level++)
        {
            float xOffset = startX + (level - 1) * xSpacing; // X축 위치 (레벨별)
            for (int i = 0; i < stagesPerLevel; i++)
            {
                float yOffset = startY - i * ySpacing; // Y축 위치 (스테이지별)

                Vector2 position = new Vector2(xOffset, yOffset); // 위치 설정
                string stageName = $"Stage {level}-{i + 1}";
                StageNode newStage = new StageNode(level, position, stageName);

                allStages.Add(newStage);

                // 레벨 1의 첫 번째 스테이지를 시작 스테이지로 설정
                if (level == 1 && i == 0)
                {
                    currentStage = newStage;
                }
            }
        }


        // 스테이지 연결
        for (int level = 0; level < levels - 1; level++)
        {
            List<StageNode> currentLevelStages = allStages.FindAll(stage => stage.level == level + 1);
            List<StageNode> nextLevelStages = allStages.FindAll(stage => stage.level == level + 2);

            foreach (StageNode stage in currentLevelStages)
            {
                ConnectToNextStages(stage, nextLevelStages);
            }
        }

        Debug.Log($"🟢 총 생성된 스테이지 개수: {allStages.Count}");

        // UI 업데이트를 위한 이벤트 호출
        OnStageGenerated?.Invoke(allStages);

        /*StageNode startNode = new StageNode(1, new Vector2(-703, 114),"stage1");
        allStages.Add(startNode);
        currentStage = startNode;

        StageNode stage2_1 = new StageNode(2, new Vector2(-177, 356),"stage2_1");
        StageNode stage2_2 = new StageNode(2, new Vector2(-160, 127),"stage2_2");
        StageNode stage2_3 = new StageNode(2, new Vector2(-160, -97),"stage2_3");

        startNode.nextStages.Add(stage2_1);
        startNode.nextStages.Add(stage2_2);
        startNode.nextStages.Add(stage2_3);

        allStages.Add(stage2_1);
        allStages.Add(stage2_2);
        allStages.Add(stage2_3);

        StageNode stage3_1 = new StageNode(3, new Vector2(245, 446),"stage3_1");  
        StageNode stage3_2 = new StageNode(3, new Vector2(266, 114),"stage3_2");    
        StageNode stage3_3 = new StageNode(3, new Vector2(327, -97),"stage3_3"); 

        // ✅ 연결 설정 (스테이지 2 → 스테이지 3)
        stage2_1.nextStages.Add(stage3_1);
        stage2_1.nextStages.Add(stage3_2);
        stage2_2.nextStages.Add(stage3_2); // ✅ 기존 연결 (stage2_2 → stage3_3)
        stage2_3.nextStages.Add(stage3_3); // ✅ 추가 연결 (stage2_2 → stage3_3)

        allStages.Add(stage3_1);
        allStages.Add(stage3_2);
        allStages.Add(stage3_3);
        Debug.Log($"🟢 총 생성된 스테이지 개수: {allStages.Count}");
        // ✅ 이벤트 호출 확인
        if (OnStageGenerated != null)
        {
            Debug.Log("✅ OnStageGenerated 이벤트 호출");
            OnStageGenerated.Invoke(allStages);
        }
        else
        {
            Debug.LogError("❌ OnStageGenerated에 구독된 함수가 없습니다!");
        }*/
    }
    void ConnectToNextStages(StageNode currentStage, List<StageNode> nextLevelStages)
    {
        int maxConnections = 3; // 한 스테이지의 최대 연결 수
        int currentConnections = 0;

        foreach (StageNode nextStage in nextLevelStages)
        {
            if (currentConnections >= maxConnections)
                break;

            // 같은 칸 우선 연결
            if (Mathf.Abs(currentStage.position.x - nextStage.position.x) < 50)
            {
                currentStage.nextStages.Add(nextStage);
                currentConnections++;

                // 연결 시각화
                DrawConnection(currentStage, nextStage);
            }
        }

        // 한 칸 떨어진 스테이지 연결
        if (currentConnections < maxConnections)
        {
            foreach (StageNode nextStage in nextLevelStages)
            {
                if (currentConnections >= maxConnections)
                    break;

                if (Mathf.Abs(currentStage.position.x - nextStage.position.x) >= 50 &&
                    Mathf.Abs(currentStage.position.x - nextStage.position.x) <= 200)
                {
                    currentStage.nextStages.Add(nextStage);
                    currentConnections++;

                    // 연결 시각화
                    DrawConnection(currentStage, nextStage);
                }
            }

        }

        // 랜덤 연결 추가
        while (currentConnections < maxConnections)
        {
            StageNode randomStage = nextLevelStages[UnityEngine.Random.Range(0, nextLevelStages.Count)];
            if (!currentStage.nextStages.Contains(randomStage))
            {
                currentStage.nextStages.Add(randomStage);
                currentConnections++;

                // 연결 시각화
                DrawConnection(currentStage, randomStage);
            }
        }
        Debug.Log($"🔗 {currentStage.name}에 연결된 스테이지: {string.Join(", ", currentStage.nextStages)}");
    }
    public void MoveToStage(StageNode newStage)
    {
        if (newStage == null)
        {
            Debug.LogError("❌ MoveToStage() 호출 실패: newStage가 null입니다!");
            return;
        }

        // ✅ 같은 레벨 또는 이전 레벨로 이동 불가
        if (currentStage != null && newStage.level <= currentStage.level)
        {
            Debug.Log($"🛑 이동 불가: 현재 레벨 {currentStage.level} → 이동하려는 레벨 {newStage.level} (같은 레벨 또는 이전 레벨)");
            return;
        }

        // ✅ 한 단계 이상 건너뛴 이동을 막음
        if (currentStage != null && newStage.level > currentStage.level + 1)
        {
            Debug.Log($"🛑 이동 불가: 현재 레벨 {currentStage.level} → 이동하려는 레벨 {newStage.level} (한 단계 이상 이동 불가능)");
            return;
        }

        // ✅ 현재 스테이지의 `nextStages`에 포함되지 않은 스테이지로 이동 불가
        if (currentStage != null && !currentStage.nextStages.Contains(newStage))
        {
            Debug.Log($"🛑 이동 불가: 현재 스테이지 {currentStage.level}에 연결되지 않은 스테이지로 이동할 수 없습니다!");
            return;
        }

        Debug.Log($"✅ 스테이지 이동: 현재 레벨 {currentStage?.level} → 이동할 레벨 {newStage.level}");

        if (currentStage != null)
        {
            currentStage.isCleared = true;
        }

        currentStage = newStage;

        // ✅ 스테이지 변경 이벤트 호출 (UI 업데이트)
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
}

