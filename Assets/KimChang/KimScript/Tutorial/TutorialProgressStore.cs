using System.Collections.Generic;

public static class TutorialProgressStore
{
    public const int CurrentSchemaVersion = 1;

    public static bool IsAutoPopupDisabled()
    {
        return RogueLikeData.Instance.IsTutorialAutoPopupDisabled();
    }

    public static void SetAutoPopupDisabled(bool disabled)
    {
        RogueLikeData.Instance.SetTutorialAutoPopupDisabled(disabled);
        SaveTutorialProgress();
    }

    public static int GetCompletedVersion(TutorialId id)
    {
        return RogueLikeData.Instance.GetTutorialCompletedVersion(id);
    }

    public static bool IsCompleted(TutorialId id, int contentVersion)
    {
        return GetCompletedVersion(id) >= contentVersion;
    }

    public static void SetCompletedVersion(TutorialId id, int contentVersion)
    {
        RogueLikeData.Instance.SetTutorialCompletedVersion(id, contentVersion);
        SaveTutorialProgress();
    }

    public static void ResetTutorialProgressForDebug(IEnumerable<TutorialId> ids)
    {
        RogueLikeData.Instance.ResetTutorialProgress();
        SaveTutorialProgress();
    }

    private static void SaveTutorialProgress()
    {
        new SaveData().SaveDataFile();
    }
}
