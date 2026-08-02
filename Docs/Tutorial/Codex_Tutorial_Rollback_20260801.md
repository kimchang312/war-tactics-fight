# Tutorial Implementation Rollback Notes

Created: 2026-08-01

This change stores tutorial progress in the existing `PlayerData.json` save payload through `SavePlayerData.tutorialSaveData`. The stage/map save file remains unchanged.

## Added Files

- `Assets/KimChang/KimScript/Tutorial/TutorialHook.cs`
- `Assets/KimChang/KimScript/Tutorial/TutorialPopupUI.cs`
- `Assets/KimChang/KimScript/Tutorial/TutorialProgressStore.cs`
- `Assets/KimChang/KimScript/Tutorial/TutorialService.cs`
- matching `.meta` files for the four scripts
- `Docs/Tutorial/Codex_Tutorial_Rollback_20260801.md`

## Modified Hook Files

- `Assets/chan/Scripts/RL/STS/GameManager.cs`
- `Assets/chan/Scripts/RL/STS/PlacePanel.cs`
- `Assets/chan/Scripts/RL/STS/RestUI.cs`
- `Assets/chan/Scripts/RL/STS/UpgradeUI.cs`
- `Assets/KimChang/KimScript/RogueLike/SaveData.cs`
- `Assets/KimChang/KimScript/RogueLike/RogueLikeData.cs`
- `Assets/KimChang/KimScript/Event/EventUIManager.cs`
- `Assets/KimChang/KimScript/Reward/RewardUI.cs`
- `Assets/KimChang/KimScript/Store/StoreUI.cs`
- `Assets/paul/PScript/SettingsUI.cs`

## Rollback Commands

Run these from the repository root only if you want to remove this tutorial implementation. Review local changes first if more work was added after this note.

```powershell
git restore -- `
  Assets/chan/Scripts/RL/STS/GameManager.cs `
  Assets/chan/Scripts/RL/STS/PlacePanel.cs `
  Assets/chan/Scripts/RL/STS/RestUI.cs `
  Assets/chan/Scripts/RL/STS/UpgradeUI.cs `
  Assets/KimChang/KimScript/RogueLike/SaveData.cs `
  Assets/KimChang/KimScript/RogueLike/RogueLikeData.cs `
  Assets/KimChang/KimScript/Event/EventUIManager.cs `
  Assets/KimChang/KimScript/Reward/RewardUI.cs `
  Assets/KimChang/KimScript/Store/StoreUI.cs `
  Assets/paul/PScript/SettingsUI.cs

Remove-Item -LiteralPath `
  Assets/KimChang/KimScript/Tutorial/TutorialHook.cs, `
  Assets/KimChang/KimScript/Tutorial/TutorialHook.cs.meta, `
  Assets/KimChang/KimScript/Tutorial/TutorialPopupUI.cs, `
  Assets/KimChang/KimScript/Tutorial/TutorialPopupUI.cs.meta, `
  Assets/KimChang/KimScript/Tutorial/TutorialProgressStore.cs, `
  Assets/KimChang/KimScript/Tutorial/TutorialProgressStore.cs.meta, `
  Assets/KimChang/KimScript/Tutorial/TutorialService.cs, `
  Assets/KimChang/KimScript/Tutorial/TutorialService.cs.meta, `
  Docs/Tutorial/Codex_Tutorial_Rollback_20260801.md
```

## Runtime Reset

Use Settings UI button hooks or call:

```csharp
TutorialService.Instance.ResetTutorialProgressForDebug();
```

This clears only `SavePlayerData.tutorialSaveData` and immediately rewrites `PlayerData.json`. It does not delete the save file and does not call `PlayerPrefs.DeleteAll()`.
