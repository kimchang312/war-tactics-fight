using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class EnemyInfoPanel : MonoBehaviour
{
    [Header("할당할 프리팹 & Content")]
    public GameObject enemyUIPrefab;
    public RectTransform enemyContainer;    // ScrollView → Content
    [Header("전투 타입 텍스트")]
    public TextMeshProUGUI battleTypeText;
    

    public void ShowEnemyInfo(StageType stageType, int presetID)
    {
        // 1) 전투 타입 문구
        switch (stageType)
        {
            case StageType.Combat: battleTypeText.text = "일반 전투"; break;
            case StageType.Elite: battleTypeText.text = "엘리트"; break;
            case StageType.Boss: battleTypeText.text = "보스"; break;
            default: battleTypeText.text = ""; break;
        }

        /* 3) presetID 로부터 유닛 IDX 리스트 가져오기
        var unitIDs = StagePresetLoader.I.GetUnitList(presetID);
        if (unitIDs == null || unitIDs.Count == 0)
        {
            Debug.LogWarning($"[{presetID}]에 바인드된 적이 없습니다.");
            return;
        }

        // 4) 하나씩 Instantiate + Setup
        foreach (int unitIdx in unitIDs)
        {
            var go = Instantiate(enemyUIPrefab, enemyContainer);
            var ui = go.GetComponent<UnitUIPrefab>();    // 혹은 EnemyUnitUI 
            ui.SetupIMG(unitIdx);
        }*/
    }
}
