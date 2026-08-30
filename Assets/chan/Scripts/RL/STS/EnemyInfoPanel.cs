using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnemyInfoPanel : MonoBehaviour
{
    [Header("할당할 프리팹 & Content")]
    public GameObject enemyUIPrefab;
    public RectTransform enemyContainer;    // ScrollView → Content
    [Header("전투 타입 텍스트")]
    public TextMeshProUGUI battleTypeText;
    [Header("UI 레퍼런스")]
    public TextMeshProUGUI unitCountText;       // “적 유닛 수: X” 를 보여줄 텍스트
    
    [Header("지휘관 정보")]
    public GameObject commanderInfo;
    public TextMeshProUGUI commanderNameText;   // 지휘관 이름
    public TextMeshProUGUI commanderSkillText;  // 지휘관 스킬 이름
    [Header("버튼")]
    [SerializeField] private Button placeButton;
    [Header("맹인 효과")]
    [SerializeField] private GameObject blindText;

    private bool combinedMode = false;

    public void ShowEnemyInfo(StageType stageType,
                              List<RogueUnitDataBase> enemies,
                              string commanderName,
                              bool combined = false,
                              int? eliteCommanderNumericId = null)
    
        {
        combinedMode = combined;
        // 👉 맹인 유산 확인
        if (RelicManager.CheckRelicById(36))
        {
            // 모든 기존 UI 비활성화
            battleTypeText.gameObject.SetActive(false);
            commanderInfo.SetActive(false);
            unitCountText.gameObject.SetActive(false);
            enemyContainer.gameObject.SetActive(false);
            placeButton.gameObject.SetActive(false);

            blindText.SetActive(true);
            blindText.GetComponent<TextMeshProUGUI>().text = "맹인전사의 안대 보유 효과로 적 정보가 표시되지 않습니다.";
            gameObject.SetActive(true);
            return;
        }

        // ✨ 평소처럼 UI 표시
        blindText.SetActive(false);
        battleTypeText.gameObject.SetActive(true);
        commanderInfo.SetActive(true);
        unitCountText.gameObject.SetActive(true);
        enemyContainer.gameObject.SetActive(true);
        placeButton.gameObject.SetActive(!combinedMode);

        // 1) 전투 타입 문구
        switch (stageType)
        {
            case StageType.Combat: battleTypeText.text = "일반 전투"; break;
            case StageType.Elite: battleTypeText.text = "엘리트 전투"; break;
            case StageType.Boss: battleTypeText.text = "보스 전투"; break;
            default: battleTypeText.text = ""; break;
        }
        // 2) 지휘관 정보 (항상 표시)
        commanderInfo.SetActive(true);
        if (!string.IsNullOrEmpty(commanderName))
        {
            commanderNameText.text = commanderName;
            commanderSkillText.text = CommanderSkillData.GetSkillText(commanderName, stageType, eliteCommanderNumericId);
        }
        else
        {
            commanderNameText.text = "없음";
            commanderSkillText.text = "없음";
        }
        // 2) 기존 표시 지우기
        foreach (Transform child in enemyContainer)
            Destroy(child.gameObject);

        // 생성 순서 표시
        int order = 1;
        bool hideEnemyDeployment = CommanderCatalog.GetId(stageType, eliteCommanderNumericId, commanderName) == 215;

        // 3) 적 유닛마다 UI 생성
        foreach (var enemy in enemies)
        {
            var go = Instantiate(enemyUIPrefab, enemyContainer);
            var ui = go.GetComponent<UnitUIPrefab>();
            ui.SetupIMG(enemy, Context.Enemy, order);
            ui.unitNumbering.text = order.ToString();
            ui.SetHidden(hideEnemyDeployment);
            order++;
        }
        unitCountText.text = $"{enemies.Count}";
        // 4) 패널 켜기
        gameObject.SetActive(true);
    }
    public void OnPlaceButtonClicked()
    {
        if (!combinedMode)
        {
            gameObject.SetActive(false);
        }
        GameManager.Instance.TogglePlacePanel(true);
        GameManager.Instance.PlacePanelComponent.UpdateMaxUnitText();
    }
}
