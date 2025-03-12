using UnityEngine;


namespace Map
{
    // 패키지가 설치되어 있지 않다면 Dummy 클래스를 정의합니다.
    public class UILineRenderer : MonoBehaviour
    {
        public Color color;
    }

    public class LineConnection
    {
        public LineRenderer lr;
        public UILineRenderer uilr;
        public StageNode from; // 기존 MapNode -> StageNode 변경
        public StageNode to;

        public LineConnection(LineRenderer lr, UILineRenderer uilr, StageNode from, StageNode to)
        {
            this.lr = lr;
            this.uilr = uilr;
            this.from = from;
            this.to = to;
        }

        public void SetColor(Color color)
        {
            if (lr != null)
            {
                Gradient gradient = lr.colorGradient;
                GradientColorKey[] colorKeys = gradient.colorKeys;
                for (int j = 0; j < colorKeys.Length; j++)
                {
                    colorKeys[j].color = color;
                }
                gradient.colorKeys = colorKeys;
                lr.colorGradient = gradient;
            }

            if (uilr != null)
            {
                uilr.color = color;
            }
        }
    }
}
