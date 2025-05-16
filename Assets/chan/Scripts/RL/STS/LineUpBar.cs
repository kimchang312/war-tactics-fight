using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class LineUpBar : MonoBehaviour
{
    [Header("할당할 프리팹 & Content")]
    public GameObject unitUIPrefab;      // UnitItemPrefab
    public RectTransform contentParent;    // ScrollView → Content

    private void OnEnable()
    {
        // 1) 기존에 있던 UI 전부 지우기
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        MakeUnitList();
    }
    public void MakeUnitList()
    {
        
        // 1) 데이터 가져오기
        //var testunit = RogueUnitDataBase.GetRandomUnitByRarity(1);
        //RogueLikeData.Instance.AddMyUnis(testunit);

        // 2) 데이터 꺼내오기
        var units = RogueLikeData.Instance.GetMyTeam();

        // 3) 하나씩 Instantiate + Setup
        foreach (var unit in units)
        {
            var go = Instantiate(unitUIPrefab, contentParent);
            var ui = go.GetComponent<UnitUIPrefab>();
            ui.SetupIMG(unit,Context.Lineup);
            ui.SetupEnergy(unit);
        }
    }
    public void UpdateLineupNumbers(IReadOnlyList<RogueUnitDataBase> placedUnits)
    {
        foreach (var ui in contentParent.GetComponentsInChildren<UnitUIPrefab>())
        {
            if (ui.ContextType != Context.Lineup) continue;

            int idx = -1;
            for (int i = 0; i < placedUnits.Count; i++)
                if (placedUnits[i].idx == ui.UnitIdx)
                {
                    idx = i;
                    break;
                }

            if (idx >= 0)
            {
                ui.SetNumber(idx + 1);
                ui.GetComponent<CanvasGroup>().alpha = 0.5f;
            }
            else
            {
                ui.SetNumber(0);
                ui.GetComponent<CanvasGroup>().alpha = 1f;
            }
        }
    }
}
