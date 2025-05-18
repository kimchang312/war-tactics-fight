using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlacePanel : MonoBehaviour
{
    [SerializeField] private Button startBattleButton;   // 전투 시작 버튼
    [SerializeField] public TextMeshProUGUI maxUnitCount;
    [SerializeField] public TextMeshProUGUI currentUnitCount;

    public GameObject battleUnitPrefab;
    public RectTransform PrefabContainer;
    //프리팹 식별용 unitOrderingNum 리스트?
    public List<int> PlacedUniqueIds { get; } = new List<int>();

    private List<RogueUnitDataBase> placedUnits = new List<RogueUnitDataBase>();
    
    private void Awake()
    {
        startBattleButton.onClick.AddListener(OnStartBattleClicked);
        // 최대 배치 가능 수 표시
        int maxUnits = RogueLikeData.Instance.GetMaxUnits();
        maxUnitCount.text = $"/ {maxUnits.ToString()}";
        // 현재 배치 수 초기화
        UpdateCountTexts();
        // 패널 처음 열릴 때는 항상 초기화
        ClearPlacePanel();
    }
    public int AddUnitToBattle(RogueUnitDataBase unit)
    {
        // 배치 최대치 초과 방지
        if (placedUnits.Count >= RogueLikeData.Instance.GetMaxUnits())
        return 0;


        // 리스트에 추가
        placedUnits.Add(unit);
        PlacedUniqueIds.Add(unit.UniqueId);
        // UI 생성 & 순서 반환
        int order = placedUnits.Count;

        CreateBattleUnitUI(unit, unit.UniqueId, order);

        // 배치 유닛 수 갱신
        UpdateCountTexts();
        return order;
    }
    private void CreateBattleUnitUI(RogueUnitDataBase unit, int uniqueId, int order)
    {
        // 인스턴스화
        var go = Instantiate(battleUnitPrefab, PrefabContainer);
        var ui = go.GetComponent<UnitUIPrefab>();

        // 1) 유닛 이미지, 기력 세팅
        ui.SetupIMG(unit,Context.Placed,uniqueId);
        ui.SetupEnergy(unit);

        // 2) 번호 세팅 (UnitUIPrefab 에 SetNumber 메서드 필요)
        ui.SetNumber(order);
    }
    public void ClearPlacePanel()
    {
        placedUnits.Clear();
        PlacedUniqueIds.Clear();
        foreach (Transform child in PrefabContainer)
            Destroy(child.gameObject);
        // 초기화 후 현재 유닛 수 갱신
        UpdateCountTexts();
    }
    public void RemoveUnitFromBattle(RogueUnitDataBase unit)
    {
        // 1) PrefabContainer 안에서 이 unit.UniqueId와 매칭되는 UI를 찾는다
        var uiToRemove = PrefabContainer
            .GetComponentsInChildren<UnitUIPrefab>()
            .FirstOrDefault(ui => ui.uniqueId == unit.UniqueId);

        var lineupUI = GameManager.Instance.LineUpBarComponent.GetUnitUIByUniqueId(unit.UniqueId);
        if (lineupUI != null)
            lineupUI.RestoreFromPlaced();

        // 3) 데이터 리스트에서 UniqueId로 제거
        placedUnits.RemoveAll(u => u.UniqueId == unit.UniqueId);
        PlacedUniqueIds.Remove(unit.UniqueId);

        DestroyImmediate(uiToRemove.gameObject);

        // 5) 남은 BattleUnitP 번호 재부여
        var remainingUIs = PrefabContainer.GetComponentsInChildren<UnitUIPrefab>();
        for (int i = 0; i < remainingUIs.Length; i++)
        {
            remainingUIs[i].SetNumber(i + 1);
        }
        UpdateCountTexts();
        //배치 유닛 제거 후 MyPrefabs숫자 갱신
        GameManager.Instance.LineUpBarComponent.UpdateLineupNumbers(PlacedUniqueIds);
    }
    
    private void OnStartBattleClicked()
    {
        RogueLikeData.Instance.SetAllMyUnits(placedUnits);
        GameManager.Instance.HideAllPanels();
        ClearPlacePanel();
        SceneManager.LoadScene("AutoBattleScene");
    }
    
     // 현재UnitCountText를 placedUnits.Count로 갱신
     private void UpdateCountTexts()
     {
         currentUnitCount.text = placedUnits.Count.ToString();
     }
}
