using System.Collections.Generic;
using UnityEngine;

public class StageNode
{
    public int level; // 스테이지의 레벨
    public Vector2 position; // UI에서 표시할 위치
    public string name;
    public List<StageNode> nextStages; // 다음 스테이지 리스트
    public bool isCleared = false; // 해당 스테이지 클리어 여부

    public StageNode(int level, Vector2 position,string name)
    {
        this.level = level;
        this.position = position;
        this.name = name;
        nextStages = new List<StageNode>();
    }
}
