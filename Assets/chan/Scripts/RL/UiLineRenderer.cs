using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    public Vector2[] Points;

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (Points == null || Points.Length < 2)
            return;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        vertex.position = Points[0];
        vh.AddVert(vertex);
        vertex.position = Points[1];
        vh.AddVert(vertex);
        vh.AddTriangle(0, 1, 1); // 단순 선 표시를 위한 더미 처리
    }
}
