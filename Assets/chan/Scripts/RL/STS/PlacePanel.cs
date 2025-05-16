using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlacePanel : MonoBehaviour
{
    [SerializeField] private Button startBattleButton;   // 전투 시작 버튼


    public GameObject battleUnitPrefab;
    public RectTransform PrefabContainer;

    private List<RogueUnitDataBase> placedUnits = new List<RogueUnitDataBase>();
    public IReadOnlyList<RogueUnitDataBase> PlacedUnits => placedUnits;

    private void Awake()
    {
        startBattleButton.onClick.AddListener(OnStartBattleClicked);
    }
    public int AddUnitToBattle(int unitId)
    {
        // 1) 데이터베이스에서 해당 유닛 검색
        var unitClone = UnitLoader.Instance.GetCloneUnitById(unitId);
        if (unitClone == null) return 0;
        // 2) 리스트에 추가
        placedUnits.Add(unitClone);

        // 3) UI 생성 & 순서 반환
        int order = placedUnits.Count;
        CreateBattleUnitUI(unitClone, order);
        Debug.Log(order);
        return order;
    }
    private void CreateBattleUnitUI(RogueUnitDataBase unit, int index)
    {
        // 인스턴스화
        var go = Instantiate(battleUnitPrefab, PrefabContainer);
        var ui = go.GetComponent<UnitUIPrefab>();

        // 1) 유닛 이미지, 기력 세팅
        ui.SetupIMG(unit,Context.Placed);
        ui.SetupEnergy(unit);

        // 2) 번호 세팅 (UnitUIPrefab 에 SetNumber 메서드 필요)
        ui.SetNumber(index);
    }
    public void ClearPlacePanel()
    {
        placedUnits.Clear();
        foreach (Transform child in PrefabContainer)
            Destroy(child.gameObject);
    }
    public void RemoveUnitFromBattle(int unitId)
    {
        int idx = placedUnits.FindIndex(u => u.idx == unitId);
        if (idx < 0) return;
        placedUnits.RemoveAt(idx);
        Destroy(PrefabContainer.GetChild(idx).gameObject);
        UpdateBattleNumbers();
    }
    public void UpdateBattleNumbers()
    {
        for (int i = 0; i < placedUnits.Count; i++)
        {
            var ui = PrefabContainer.GetChild(i).GetComponent<UnitUIPrefab>();
            ui.UpdateSelectionNumber(i + 1);
        }
    }
    private void OnStartBattleClicked()
    {
        RogueLikeData.Instance.SetAllMyUnits(placedUnits);
        GameManager.Instance.HideAllPanels();
        ClearPlacePanel();
        SceneManager.LoadScene("AutoBattleScene");
    }
}
