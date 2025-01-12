using System;
using System.Collections.Generic;
using UnityEngine;

public class StageMapManager : MonoBehaviour
{
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


        StageNode startNode = new StageNode(1, new Vector2(-709, -59),"stage1");
        allStages.Add(startNode);
        currentStage = startNode;

        StageNode stage2_1 = new StageNode(2, new Vector2(-180, 280),"stage2_1");
        StageNode stage2_2 = new StageNode(2, new Vector2(-160, -59),"stage2_2");
        StageNode stage2_3 = new StageNode(2, new Vector2(-160, -352),"stage2_3");

        startNode.nextStages.Add(stage2_1);
        startNode.nextStages.Add(stage2_2);
        startNode.nextStages.Add(stage2_3);

        allStages.Add(stage2_1);
        allStages.Add(stage2_2);
        allStages.Add(stage2_3);

        StageNode stage3_1 = new StageNode(3, new Vector2(245, 404),"stage3_1");  
        StageNode stage3_2 = new StageNode(3, new Vector2(269, -45),"stage3_2");    
        StageNode stage3_3 = new StageNode(3, new Vector2(330, -363),"stage3_3"); 

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
        }
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


}

