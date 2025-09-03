using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class LineUpBar : MonoBehaviour
{
    [Header("할당할 프리팹 & Content")]
    public GameObject unitUIPrefab;      // UnitItemPrefab
    public RectTransform contentParent;    // ScrollView → Content

    private void Awake()
    {
        Debug.Log("🔧 LineUpBar Awake 시작");
        
        // 하단바 활성화 및 유닛 리스트 생성
        gameObject.SetActive(true);
        RefreshUnitList();
    }

    private void OnEnable()
    {
        RefreshUnitList();
    }

    // 유닛 리스트 새로고침
    public void RefreshUnitList()
    {
        Debug.Log("🔄 RefreshUnitList 호출됨");
        
        // 1) 기존에 있던 UI 전부 지우기
        if (contentParent != null)
        {
            foreach (Transform child in contentParent)
                Destroy(child.gameObject);
        }

        // 2) 유닛 리스트 생성
        MakeUnitList();
    }

    public void MakeUnitList()
    {
        // contentParent가 null이면 리턴
        if (contentParent == null)
        {
            Debug.LogError("❌ contentParent가 할당되지 않았습니다!");
            return;
        }

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
        
        // 데이터 꺼내오기
        var units = RogueLikeData.Instance.GetMyTeam();

        // 획득순으로 정렬된 유닛 리스트 생성
        var sortedUnits = units.OrderBy(u => u.UniqueId).ToList();

        // 하나씩 Instantiate + Setup
        for (int i = 0; i < sortedUnits.Count; i++)
        {
            var u = sortedUnits[i];
            var go = Instantiate(unitUIPrefab, contentParent);
            var ui = go.GetComponent<UnitUIPrefab>();
            ui.SetupIMG(u, Context.Lineup, u.UniqueId);
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
