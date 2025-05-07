using UnityEngine;
using System.Collections.Generic;

public class JsonTest : MonoBehaviour
{
    [Tooltip("씬에 배치한 StagePresetLoader 참조를 드래그하세요")]
    public StagePresetLoader loader;

    void Start()
    {
        if (loader == null)
        {
            Debug.LogError("[JsonTest] loader 가 할당되지 않았습니다.");
            return;
        }

        // 1) 프리셋 1번의 유닛 리스트를 가져옵니다
        List<int> unitList = loader.GetUnitList(1);
        if (unitList == null || unitList.Count == 0)
        {
            Debug.LogWarning("[JsonTest] PresetID=1의 UnitList가 비어있거나 없습니다.");
            return;
        }

        // 2) 리스트 내용을 한 줄의 문자열로 합쳐서 출력
        string unitsJoined = string.Join(", ", unitList);
        Debug.Log($"[JsonTest] Preset 1 Units: {unitsJoined}");

        // // 또는 각각 반복 출력
        // foreach (var u in unitList)
        //     Debug.Log($"Unit: {u}");
    }
}
