using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImg;
    [SerializeField] private Image NPCImag;
    [SerializeField] private TextMeshProUGUI NPCLineText;
    [SerializeField] private Button lineBtn;
    [SerializeField] private GameObject relicBox;
    [SerializeField] private GameObject itemToolTip;
    [SerializeField] private ObjectPool objectPool;

    private readonly Dictionary<int,List<string>> lineList = new()
    {
        {0, new List<string> { "먼저, 맵에 있는 첫번째 노드를 클릭해주세요." } },
        { 1, new List<string>{ ""} },

    };

    private int tutorialLevel = 0;
    private int lineIndex = 0;

    private void SetUI()
    {
        WarRelicBoxUI.SetRelicBox(relicBox,itemToolTip,objectPool);

    }


    private void NextLineText()
    {
        if (lineIndex >= lineList[tutorialLevel].Count) return;
        NPCLineText.text = lineList[tutorialLevel][lineIndex++];

    }
}
