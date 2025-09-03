using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class StagePreset
{
    [JsonProperty("PresetID")]
    public int PresetID;
    public int? Chapter;
    public int? Level;
    public string StageType;
    public int Value;
    [JsonConverter(typeof(CommaSeparatedStringToIntListConverter))]
    public List<int> UnitList;
    public int? UnitCount;
    public string Faction;
    public string Commander;
    public string CommanderID;
    public string Description;
    [JsonProperty("BattlefieldEffect")]
    public string BattlefieldEffect;
}

public class StagePresetLoader : MonoBehaviour
{
    public static StagePresetLoader I { get; private set; }

    [Header("Resources/StagePresets.json 파일명 (확장자 제외)")]
    public string resourceJsonName = "StagePresets";

    [HideInInspector]
    public List<StagePreset> presets;

    void Awake()
    {
        // 싱글턴 세팅
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (I != this)
        {
            Destroy(gameObject);
            return;
        }
        LoadAllPresets();
    }

    void LoadAllPresets()
    {
        var ta = Resources.Load<TextAsset>(resourceJsonName);
        if (ta == null)
        {
            Debug.LogError($"[{nameof(StagePresetLoader)}] Resources/{resourceJsonName}.json 을 찾을 수 없습니다.");
            presets = new List<StagePreset>();
            return;
        }

        try
        {
            presets = JsonConvert.DeserializeObject<List<StagePreset>>(ta.text);
        }
        catch (JsonException je)
        {
            Debug.LogError($"[{nameof(StagePresetLoader)}] JSON 파싱 오류: {je.Message}");
            presets = new List<StagePreset>();
        }
    }

    // chapter, level, stageType 으로 후보 목록 필터링
    public List<StagePreset> GetPresets(int chapter, int level, string stageType)
    {
        if (stageType == "elite" || stageType == "boss")
        {
            // 레벨 무시 → 챕터 + 타입만 필터
            return presets
                .Where(p => p.Chapter == chapter && p.StageType == stageType)
                .ToList();
        }
        else
        {
            // 기존대로 챕터·레벨·타입 모두 매칭
            return presets
                .Where(p =>
                    p.Chapter == chapter &&
                    p.Level == level &&
                    p.StageType == stageType
                )
                .ToList();
        }
    }
    public StagePreset GetByID(int id)
        => presets.FirstOrDefault(p => p.PresetID == id);
}
