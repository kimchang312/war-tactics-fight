using UnityEngine;
using UnityEngine.UI;

public class GridCellGenerator : MonoBehaviour
{
    public RectTransform gridParent; // StageContainer 역할 (이미 Canvas 아래에 존재하는 오브젝트)
    public GameObject gridCellPrefab; // 빈 UI 오브젝트 프리팹 (반드시 RectTransform 컴포넌트 포함)

    public int levels = 15;   // 열(레벨) 수
    public int rows = 7;      // 행(칸) 수
    public float xSpacing = 230f; // 각 열 사이의 X 간격
    public float ySpacing = 140f; // 각 행 사이의 Y 간격
    public float startX = -700f;  // 시작 X 좌표
    public float startY = 425f;   // 시작 Y 좌표

    void Start()
    {
        GenerateGridCells();
    }

    void GenerateGridCells()
    {
        // gridCellPrefab이 지정되지 않았다면, 기본 GameObject 생성
        if (gridCellPrefab == null)
        {
            gridCellPrefab = new GameObject("GridCell", typeof(RectTransform));
        }

        // 레벨별(열)로 반복
        for (int level = 1; level <= levels; level++)
        {
            // 행별(칸)로 반복
            for (int row = 0; row < rows; row++)
            {
                // **특별 조건:** 15레벨에서는 오직 행 인덱스 3 (즉, "d")만 생성
                if (level == 15 && row != 3)
                {
                    continue; // 나머지 행은 건너뜁니다.
                }

                // 각 셀의 위치 계산
                float xPos = startX + (level - 1) * xSpacing;
                float yPos = startY - row * ySpacing;
                Vector2 cellPosition = new Vector2(xPos, yPos);

                // gridParent의 자식으로 셀 인스턴스 생성
                GameObject cell = Instantiate(gridCellPrefab, gridParent);

                // RectTransform 설정
                RectTransform rt = cell.GetComponent<RectTransform>();
                rt.anchoredPosition = cellPosition;
                rt.sizeDelta = new Vector2(xSpacing * 0.8f, ySpacing * 0.8f);

                // 셀 이름 지정: 예를 들어 "1-a", "1-b", …, "15-d"
                string rowLetter = ((char)('a' + row)).ToString();  // a, b, c, d, e, f, g
                cell.name = $"{level}-{rowLetter}";
            }
        }
    }
}
