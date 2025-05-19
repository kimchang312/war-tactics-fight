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

    public void ShowEnemyInfo(StageType stageType,
                              List<RogueUnitDataBase> enemies,
                              string commanderName /*,
                             string commanderSkill*/)
    
        {
        // 1) 전투 타입 문구
        switch (stageType)
        {
            case StageType.Combat: battleTypeText.text = "일반 전투"; break;
            case StageType.Elite: battleTypeText.text = "엘리트 전투"; break;
            case StageType.Boss: battleTypeText.text = "보스 전투"; break;
            default: battleTypeText.text = ""; break;
        }
        // 2) 지휘관 정보
        bool hasCommander = !string.IsNullOrEmpty(commanderName);
        commanderInfo.SetActive(hasCommander);
        if (hasCommander)
        {
            commanderNameText.text = commanderName;
            //commanderSkillText.text = commanderSkill;
        }
        // 2) 기존 표시 지우기
        foreach (Transform child in enemyContainer)
            Destroy(child.gameObject);

        // 생성 순서 표시
        int order = 1;

        // 3) 적 유닛마다 UI 생성
        foreach (var enemy in enemies)
        {
            var go = Instantiate(enemyUIPrefab, enemyContainer);
            go.GetComponent<UnitUIPrefab>().SetupIMG(enemy,Context.Enemy,order);
            var ui = go.GetComponent<UnitUIPrefab>();
            ui.unitNumbering.text = order.ToString();
            order++;
        }
        unitCountText.text = $"{enemies.Count}";
        // 4) 패널 켜기
        gameObject.SetActive(true);
    }
    public void OnPlaceButtonClicked()
    {
        gameObject.SetActive(false);
        GameManager.Instance.TogglePlacePanel(true);
    }
}
