using UnityEngine;
using System.Linq;

public class LineUpBar : MonoBehaviour
{
    [Header("할당할 프리팹 & Content")]
    public GameObject unitUIPrefab;      // UnitItemPrefab
    public RectTransform contentParent;    // ScrollView → Content

    private void OnEnable()
    {
        RefreshUnitList();
    }
    public void RefreshUnitList()
    {
        // 1) 기존에 있던 UI 전부 지우기
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // 1) 데이터 가져오기
        //var testunit = RogueUnitDataBase.GetRandomUnitByRarity(1);
        //RogueLikeData.Instance.AddMyUnis(testunit);

        // 2) 데이터 꺼내오기
        var units = RogueLikeData.Instance.GetMyUnits();

        // 3) 하나씩 Instantiate + Setup
        foreach (var unit in units)
        {
            var go = Instantiate(unitUIPrefab, contentParent);
            var ui = go.GetComponent<UnitUIPrefab>();
            ui.Setup(unit);
        }
    }
}
