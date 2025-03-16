using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCellGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int columns = 7;
    public int rows = 15;
    public Vector2 cellSize = new Vector2(150, 150);
    public Vector2 spacing = new Vector2(20, 20);

    public List<RectTransform> gridCells = new List<RectTransform>();

    private RectTransform rectTransform;

    void Awake()
    {
        // 이 스크립트가 붙은 오브젝트(예: GridCellGenerator의 부모)의 RectTransform을 가져와서
        // 앵커와 피벗을 왼쪽 상단으로 설정합니다.
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // 기존 자식 오브젝트 삭제
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        gridCells.Clear();

        // 각 셀의 위치를 계산하여 생성
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // 새로운 셀(GameObject) 생성
                GameObject cell = new GameObject($"Cell_{row}_{col}", typeof(RectTransform));
                // 이 셀을 현재 GridCellGenerator 오브젝트의 자식으로 설정
                cell.transform.SetParent(transform, false);
                RectTransform rt = cell.GetComponent<RectTransform>();
                rt.sizeDelta = cellSize;

                // 각 셀의 anchoredPosition을 수동 계산
                // x: col * (cellSize.x + spacing.x)
                // y: -row * (cellSize.y + spacing.y) (행은 아래로 배치하기 위해 음수 사용)
                float xPos = col * (cellSize.x + spacing.x);
                float yPos = -row * (cellSize.y + spacing.y);
                rt.anchoredPosition = new Vector2(xPos, yPos);

                // StageMapManager에서 참조할 gridID와 일치하도록 이름 설정 (예: "1-a", "1-b", ...)
                cell.name = $"{row + 1}-{(char)('a' + col)}";

                gridCells.Add(rt);
            }
        }
    }
}
