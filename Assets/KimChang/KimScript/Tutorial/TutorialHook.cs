using UnityEngine;

public static class TutorialHook
{
    public const string StepReady = "Ready";
    public const string StepResourceChanged = "ResourceChanged";
    public const string StepFirstUnitPlaced = "FirstUnitPlaced";
    public const string StepWarLegacy = "WarLegacy";

    public static bool EnqueueAndNotifyCurrentStage(TutorialId tutorialId, string screenName, string stepId = StepReady)
    {
        string screenVisitId = BuildCurrentStageScreenVisitId(screenName);
        string contextId = screenVisitId;
        string triggerId = BuildTriggerId(tutorialId, screenVisitId, stepId);

        bool queued = Enqueue(tutorialId, triggerId, contextId, screenVisitId, stepId);
        Notify(screenVisitId, stepId);
        return queued;
    }

    public static bool EnqueueCurrentStage(TutorialId tutorialId, string screenName, string stepId = StepReady)
    {
        string screenVisitId = BuildCurrentStageScreenVisitId(screenName);
        string contextId = screenVisitId;
        string triggerId = BuildTriggerId(tutorialId, screenVisitId, stepId);
        return Enqueue(tutorialId, triggerId, contextId, screenVisitId, stepId);
    }

    public static void NotifyCurrentStage(string screenName, string stepId = StepReady)
    {
        string screenVisitId = BuildCurrentStageScreenVisitId(screenName);
        Notify(screenVisitId, stepId);
    }

    public static void CancelCurrentStageContext(string screenName)
    {
        TutorialService.Instance?.CancelRequestsByContext(BuildCurrentStageScreenVisitId(screenName));
    }

    public static void EnqueueStageTypeGuide(StageType stageType, string screenName)
    {
        TutorialId? id = GetStageTypeGuide(stageType);
        if (!id.HasValue)
            return;

        EnqueueCurrentStage(id.Value, screenName);
    }

    public static void EnqueueStageTypeGuideAndNotify(StageType stageType, string screenName)
    {
        EnqueueStageTypeGuide(stageType, screenName);
        NotifyCurrentStage(screenName);
    }

    public static void EnqueueMapHudResource(TutorialId tutorialId)
    {
        EnqueueCurrentStage(tutorialId, "MapHud", StepResourceChanged);
    }

    public static void NotifyMapHudResource()
    {
        NotifyCurrentStage("MapHud", StepResourceChanged);
    }

    public static string BuildCurrentStageScreenVisitId(string screenName)
    {
        string runKey = "NoRun";
        RogueLikeData data = RogueLikeData.Instance;
        if (data != null)
        {
            var (x, y, type) = data.GetCurrentStage();
            runKey = $"Ch{data.GetChapter()}:{x}:{y}:{type}:P{data.GetPresetID()}";
        }

        return $"{screenName}:{runKey}";
    }

    private static bool Enqueue(TutorialId tutorialId, string triggerId, string contextId, string screenVisitId, string stepId)
    {
        TutorialService service = TutorialService.Instance;
        return service != null && service.EnqueueAuto(tutorialId, triggerId, contextId, screenVisitId, stepId);
    }

    private static void Notify(string screenVisitId, string stepId)
    {
        TutorialService.Instance?.NotifySafePoint(screenVisitId, stepId);
    }

    private static string BuildTriggerId(TutorialId tutorialId, string screenVisitId, string stepId)
    {
        return $"{tutorialId}:{screenVisitId}:{stepId}";
    }

    private static TutorialId? GetStageTypeGuide(StageType stageType)
    {
        switch (stageType)
        {
            case StageType.Combat: return TutorialId.STG_01_NORMAL;
            case StageType.Elite: return TutorialId.STG_02_ELITE;
            case StageType.Boss: return TutorialId.STG_03_BOSS;
            default: return null;
        }
    }
}
