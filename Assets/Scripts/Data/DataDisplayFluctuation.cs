using UnityEngine;
using UnityEngine.UI;

namespace Data
{
    /// <summary>
    /// Randomly fluctuates water quality display values for inflow/outflow data panels.
    /// </summary>
    public class DataDisplayFluctuation : MonoBehaviour
    {
        [System.Serializable]
        public class ElementConfig
        {
            public string label;
            public Text text;
            public float baseValue = 10f;
            public float amplitude = 1f;
        }

        [Header("Update Interval (seconds)")]
        public float updateInterval = 2f;

        [Header("进水 (Inflow)")]
        public ElementConfig[] inflowElements = new ElementConfig[6];

        [Header("出水 (Outflow)")]
        public ElementConfig[] outflowElements = new ElementConfig[6];

        private float _timer;

        private void Start()
        {
            // Update immediately on start, then every updateInterval seconds
            Fluctuate(inflowElements);
            Fluctuate(outflowElements);
            _timer = updateInterval;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = updateInterval;
                Fluctuate(inflowElements);
                Fluctuate(outflowElements);
            }
        }

        private static void Fluctuate(ElementConfig[] elements)
        {
            if (elements == null) return;
            foreach (var e in elements)
            {
                if (e.text == null) continue;
                float min = e.baseValue - e.amplitude;
                float max = e.baseValue + e.amplitude;
                float v = Random.Range(min, max);
                e.text.text = v.ToString("F1");
            }
        }
    }
}
