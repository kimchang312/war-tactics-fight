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
            Debug.Log($"[{nameof(StagePresetLoader)}] 프리셋 {presets[0].PresetID}개 로드 완료");
            Debug.Log($"[{nameof(StagePresetLoader)}] 프리셋 {presets[1].PresetID}개 로드 완료");
            Debug.Log($"[{nameof(StagePresetLoader)}] 프리셋 {presets[2].PresetID}개 로드 완료");
        }
        catch (JsonException je)
        {
            Debug.LogError($"[{nameof(StagePresetLoader)}] JSON 파싱 오류: {je.Message}");
            presets = new List<StagePreset>();
        }
    }

    /*public StagePreset GetPresetByID(int id)
    {
        
        return presets.FirstOrDefault(p => p.PresetID == id);
        
    }

    public List<int> GetUnitList(int presetID)
    {
        var p = GetPresetByID(presetID);
        return p != null ? p.UnitList : new List<int>();
    }*/
    // chapter, level, stageType 으로 후보 목록 필터링
    public List<StagePreset> GetPresets(int chapter, int level, string stageType)
        => presets.Where(p =>
                p.Chapter == chapter &&
                p.Level == level &&
                p.StageType == stageType
           ).ToList();

    // ID 로 딱 하나 꺼내는 용
    public StagePreset GetByID(int id)
        => presets.FirstOrDefault(p => p.PresetID == id);
}
