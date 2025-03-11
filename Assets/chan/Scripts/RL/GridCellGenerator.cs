using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCellGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int columns = 7;
    public int rows = 15;
    public Vector2 cellSize = new Vector2(100, 100);
    public Vector2 spacing = new Vector2(10, 10);

    public List<RectTransform> gridCells = new List<RectTransform>();

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        gridCells.Clear();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject cell = new GameObject($"Cell_{row}_{col}", typeof(RectTransform));
                cell.transform.SetParent(transform, false);
                RectTransform rt = cell.GetComponent<RectTransform>();
                rt.sizeDelta = cellSize;
                cell.name = $"{row}-{(char)('a' + col)}";
                gridCells.Add(rt);
            }
        }
    }
}
