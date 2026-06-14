# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Project Overview

**War Tactics Fight** is a Unity-based tactical roguelike auto-battler where players deploy units on a battlefield, manage resources, and progress through procedurally-generated campaigns. The game features a deck-building strategy system with relics (permanent upgrades), turn-based combat, and a roguelike progression map.

**Target Platforms:** Windows, Android, iOS

## Development Commands

### Data Pipeline (Excel → JSON)

The game uses Excel files as the source of truth for game data. To convert Excel data to JSON:

1. Open Unity Editor
2. In Project window, select the `Assets/Excels` folder
3. Right-click → `Excel to Json` (or use menu: Assets → Excel to Json)
4. This executes `Assets/Excels/Editor/ExcelToJson.cs`
5. Configuration is controlled by `Assets/Excels/config.txt`
6. Excel files are in `Assets/Excels/excel_files/`
7. JSON output goes to `Assets/Excels/json_output/` and is then manually copied to `Assets/Resources/JsonData/`

**Important:** After modifying Excel data, you must:
1. Run "Excel to Json" from Unity Editor
2. Copy generated JSON files to `Assets/Resources/JsonData/`
3. Verify data loads correctly in-game

### Building the Game

Unity builds are configured in **File → Build Settings** with these enabled scenes:
1. Title.unity (entry point)
2. Main.unity
3. AutoBattleScene.unity
4. Difficulty.unity
5. Faction.unity
6. Unit_UI.unity
7. Test.unity (testing)
8. RLmap.unity (roguelike map)
9. Event.unity

Standard Unity build process applies (File → Build Settings → Build).

### Testing

- **Test Scene:** Open `Assets/KimChang/KimScenes/Test.unity` in Unity Editor
- **Test Scripts:** Located in `Assets/KimChang/KimScript/Test/` and `Assets/KimChang/Test/`
  - `TestLoad.cs` - Validates data loading
  - `TestModeUI.cs` - UI testing utilities
  - `RelicTestHelper.cs` - Relic system validation
- No automated test framework; testing is manual via test scenes and helper scripts

## Code Architecture

### Developer Branch Organization

The codebase is split into **3 independent developer branches** with distinct responsibilities:

#### **Assets/chan/** (~47 scripts)
- **Ownership:** Game flow, scene management, roguelike map system
- **Key Systems:**
  - Roguelike map generation (`RLmap` scene)
  - Stage/node progression system
  - Unit placement UI (`Unit_UI` scene)
  - Blacksmith system (unit upgrades)
  - Main menu and difficulty/faction selection
- **Primary Location:** `Assets/chan/Scripts/RL/STS/`

#### **Assets/KimChang/** (~73 scripts - largest subsystem)
- **Ownership:** Battle system, data management, game content
- **Key Systems:**
  - Auto-battle combat system (`AutoBattleManager`, `AutoBattleScene`)
  - Data loading pipeline (JSON → game objects)
  - Relic/War Relic system (collectible permanent upgrades)
  - Event system (random encounters with conditional triggers)
  - Reward system (post-battle loot)
  - Store/shop system
  - Sound management
  - Localization (Korean, English, Japanese via `GameTextData.json`)
- **Structure:** `Assets/KimChang/KimScript/` contains most subsystems

#### **Assets/LMJ/** (~3 scripts)
- **Ownership:** Visual effects and animations
- **Key Systems:**
  - DoTween-based combat animations
  - Effect timing/cooldown system via ScriptableObjects (`Assets/LMJ/EffectCD/*.asset`)

### Scene Flow

```
Title (entry) → Main (menu) → Difficulty → Faction → Unit_UI → RLmap (core loop)
                                                                    ↓
                                    ┌───────────────────────────────┴───────────────────────┐
                                    ↓                                                       ↓
                            AutoBattleScene (combat)                                 Event.unity
                                    ↓                                                       ↓
                            Reward → Upgrades/Relics/Shop → Next Map Node
```

### Core Singleton Managers

The game uses extensive singleton pattern for cross-scene state management:

- **`PlayerData`** - Player progression, unit inventory, faction selection (persistent)
- **`RogueLikeData`** - Central state store for current roguelike run:
  - Stage position (x, y coordinates)
  - Team composition and enemy data
  - Chapter progress (1-3)
  - Gold, morale, unit upgrades, relic collection
  - Seeded RNG for deterministic randomness
- **`GameManager`** (RLmap scene) - Master orchestrator for map generation, placement UI, rewards
- **`AutoBattleManager`** - Turn-based battle state machine with async/await animation flow
- **`UnitDataManager`** / **`UnitLoader`** - Unit database access with caching
- **`RelicManager`** - Active relic collection and effect callbacks
- **`SaveSystem`** - Persist map state to `Application.persistentDataPath`

All singleton managers use `DontDestroyOnLoad` to persist across scene transitions.

### Data Architecture

#### Data Flow Pipeline

```
Excel Files (Assets/Excels/excel_files/)
    ↓
ExcelToJson.cs (Unity Editor menu: Assets → Excel to Json)
    ↓
JSON Files (Assets/Resources/JsonData/)
    ↓
Newtonsoft.Json deserialization
    ↓
Runtime Database Classes (UnitDataBase, EventData, WarRelicDatabase, etc.)
    ↓
In-Memory Cache (UnitLoader static Dictionary)
```

#### Key JSON Data Files

Located in `Assets/Resources/JsonData/`:
- **`UnitStatus_Re.json`** - Unit stats and abilities (primary unit database)
- **`EventDataBase.json`** + **`EventChoiceDataBase.json`** - Event definitions and outcomes
- **`WarRelicsList.json`** - Relic definitions and effect types
- **`StoreItemData.json`** - Shop inventory and pricing
- **`GameTextData.json`** - Localized UI text (KR/EN/JP, ~430KB)
- **`QuestDataBase.json`** - Quest definitions
- **`StagePreset.json`** / **`StagePresets.json`** - Map generation templates

#### Database Class Conventions

- **`UnitDataBase`** - Legacy unit format with 40+ stat fields
- **`RogueUnitDataBase`** - Enhanced format with rarity, energy mechanics
- Data loading uses Newtonsoft.Json (`JsonConvert.DeserializeObject<T>()`)
- Loaders cache parsed data in static dictionaries to avoid repeated parsing

### Battle System

The auto-battle system uses a turn-based state machine:

```
Check → Start → Preparation → Crash (combat) → Support → Animation → Death → End
```

**Key Components:**
- **`AutoBattleManager`** - Controls battle flow using async/await
- **`AutoBattleUI`** - Real-time battle display
- **`AbilityManager`** - Resolves 30+ ability types with configurable values
- **`BattleCrashAnimation`** - DoTween-based combat animations
- Relics modify ability effects via callback system

Battles load team data from `RogueLikeData`, execute turn-by-turn with animations, then return rewards to `RewardManager`.

### Relic System

Relics are permanent upgrades collected during roguelike runs:

- **Types:** AllEffect, SpecialEffect, StateBoost, BattleActive, GetEffect, NoneBattle, RewardEffect, Delete
- **Storage:** `RogueLikeData.warRelics` (active collection)
- **Effects:** Relics can modify ability calculations, stat boosts, reward multipliers
- **UI:** `RelicBagUI`, `WarRelicBoxUI`, `WarRelicScrollUI`

Relics integrate into ability calculations via callback pattern in `AbilityManager`.

### Event System

Random encounters with conditional triggers:

- **`EventManager`** - Event orchestration
- **`EventDataLoader`** - Load from `EventDataBase.json` + `EventChoiceDataBase.json`
- **`EventUIManager`** - Render choices and outcomes
- Events have chapter requirements and conditional appearance (based on morale, owned relics, etc.)

### Roguelike Map Progression

- **`MapGenerator`** - Creates stage nodes based on `StagePreset.json`
- **Stage Types:** Combat, Elite, Treasure, Boss, Event, Shop, Rest
- **Progression:** Player navigates (x, y) coordinates, encounters trigger scene loads
- **State Persistence:** `SaveSystem` saves map state between sessions
- **Seeded RNG:** `RogueLikeData` maintains seed for reproducible runs

## Critical Implementation Notes

### Singleton Pattern Usage

When modifying or creating managers:
- Use `public static ClassName Instance { get; private set; }`
- Initialize in `Awake()` with singleton enforcement
- Add `DontDestroyOnLoad(gameObject)` for cross-scene persistence
- Check existing manager patterns in `PlayerData.cs`, `RogueLikeData.cs`, `GameManager.cs`

### Data Modification Workflow

**NEVER directly edit JSON files.** Always:
1. Modify Excel source files in `Assets/Excels/excel_files/`
2. Run "Excel to Json" via Unity Editor menu
3. Copy generated JSON to `Assets/Resources/JsonData/`
4. Verify in-game (check `UnitLoader`, `EventDataLoader`, etc.)

### Scene Dependencies

Scenes have strict dependencies:
- **RLmap** requires `PlayerData` and `RogueLikeData` to be initialized first
- **AutoBattleScene** loads teams from `RogueLikeData.playerUnits` and `RogueLikeData.enemyUnits`
- **Main** scene initializes `PlayerData` singleton (must load before other scenes)
- Always test scene transitions, not individual scenes in isolation

### Ability and Relic Integration

When adding new abilities or relics:
- Abilities are enum-based in `AbilityManager` (30+ types)
- Relics modify abilities via callback in `AbilityManager.ApplyRelicEffects()`
- New relic types must be added to `WarRelicDatabase` enum
- Test interactions between abilities and relics extensively

### Async/Await Animation Flow

`AutoBattleManager` uses C# async/await for turn sequencing:
- Animation waits use `await Task.Delay(milliseconds)`
- Combat phases are async methods
- DoTween animations integrate with async flow
- Avoid blocking operations; use Task-based delays

### Localization

All UI text comes from `GameTextData.json`:
- Structure: `{ "key": { "KR": "한글", "EN": "English", "JP": "日本語" } }`
- Access via text loader classes
- Add new keys to Excel source, regenerate JSON
- Default language determined by system locale or player settings

## Common Patterns

### Loading Data from JSON

```csharp
// Pattern used by UnitLoader, EventDataLoader, etc.
public class DataLoader
{
    private static Dictionary<string, DataClass> cache;

    public static void LoadData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("JsonData/FileName");
        cache = JsonConvert.DeserializeObject<Dictionary<string, DataClass>>(jsonFile.text);
    }

    public static DataClass GetData(string key) => cache[key];
}
```

### Scene Loading

```csharp
// Standard scene transition
SceneManager.LoadScene("SceneName");

// With data preparation
RogueLikeData.Instance.playerUnits = selectedUnits;
RogueLikeData.Instance.enemyUnits = GenerateEnemies();
SceneManager.LoadScene("AutoBattleScene");
```

### Prefab Instantiation

```csharp
// Pattern used throughout UI systems
GameObject prefab = Resources.Load<GameObject>("Prefabs/PrefabName");
GameObject instance = Instantiate(prefab, parent);
instance.GetComponent<UIComponent>().Initialize(data);
```

## External Dependencies

- **DoTween** (Demigiant) - Primary animation system
  - Located in `Assets/Plugins/Demigiant/DOTween/`
  - Used extensively for UI and combat animations
- **TextMesh Pro 3.0.6** - All UI text rendering
- **Newtonsoft.Json 3.2.1** - JSON serialization (prefer over JsonUtility for complex data)
- **Unity UI Extensions** - Extended UI components
- **Rounded Corners Plugin** - UI visual polish

## File Naming Conventions

- **Scene files:** PascalCase (e.g., `AutoBattleScene.unity`, `RLmap.unity`)
- **Scripts:** PascalCase (e.g., `GameManager.cs`, `AutoBattleManager.cs`)
- **JSON data:** PascalCase with underscores (e.g., `UnitStatus_Re.json`)
- **ScriptableObjects:** Prefixed with type (e.g., `ECD_S01_AbilityName.asset`)
- **Prefabs:** PascalCase, often type-suffixed (e.g., `UnitCardUI`, `RelicBoxUI`)

## Key Entry Points for Debugging

- **Game Start:** `Title.unity` → `MainScene.cs.Start()`
- **Roguelike Run Init:** `GameManager.Awake()` in RLmap scene
- **Battle Init:** `AutoBattleManager.Start()` loads team data
- **Data Loading:** Check `UnitLoader.LoadData()`, `EventDataLoader.LoadEventData()`
- **Save/Load:** `SaveSystem.SaveMapData()` / `LoadMapData()`

## Unity Version

This project uses Unity 2022.3 LTS (check `ProjectSettings/ProjectVersion.txt` for exact version).
