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
        // 데이터 꺼내오기
        var units = RogueLikeData.Instance.GetMyTeam();

        // 하나씩 Instantiate + Setup
        for (int i = 0; i < units.Count; i++)
        {
            var u = units[i];
            var go = Instantiate(unitUIPrefab, contentParent);
            var ui = go.GetComponent<UnitUIPrefab>();
            ui.SetupIMG(u,Context.Lineup,u.UniqueId);
            ui.SetupEnergy(u);
           
        }
    }
    public void UpdateLineupNumbers(List<int> placedUniqueIds)
    {
        foreach (var ui in contentParent.GetComponentsInChildren<UnitUIPrefab>())
        {
            if (ui.PrefabType != Context.Lineup) continue;
            int idx = placedUniqueIds.IndexOf(ui.uniqueId);
            var cg = ui.GetComponent<CanvasGroup>();


            if (idx >= 0)
            { // → 배치된 MyUnitPrefab만 잠그고, 번호 표시
                cg.alpha = 0.5f;
                ui.SetOrderNumber(idx + 1);
            }
            else 
            { 
            // → 나머지 MyUnitPrefab 해제
            cg.alpha = 1f;
            ui.SetOrderNumber(0);
            }
        }
    }
    public UnitUIPrefab GetUnitUIByUniqueId(int uniqueId)
    {
        return contentParent
            .GetComponentsInChildren<UnitUIPrefab>()
            .FirstOrDefault(u =>
                u.PrefabType == Context.Lineup &&
                u.unitData.UniqueId == uniqueId
            );
    }
}
