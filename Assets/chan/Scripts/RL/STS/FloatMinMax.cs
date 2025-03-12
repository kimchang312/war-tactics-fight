using UnityEngine;

namespace Map
{
    [System.Serializable]
    public class FloatMinMax
    {
        public float min;
        public float max;

        public float GetValue()
        {
            return Random.Range(min, max);
        }
    }
}
