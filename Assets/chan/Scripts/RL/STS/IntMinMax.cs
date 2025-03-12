using UnityEngine;

namespace Map
{
    [System.Serializable]
    public class IntMinMax
    {
        public int min;
        public int max;

        public int GetValue()
        {
            return Random.Range(min, max + 1);
        }
    }
}
