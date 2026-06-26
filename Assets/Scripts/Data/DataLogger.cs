using UnityEngine;

namespace Data
{
    /// <summary>
    /// Test consumer — logs filtered typed data to console when dropdowns change.
    /// </summary>
    public class DataLogger : MonoBehaviour
    {
        public WaterQualityDataManager dataManager;

        private void Start()
        {
            if (dataManager == null) return;

            dataManager.OnDataChanged += () =>
            {
                int count = dataManager.TimeIndex switch
                {
                    0 => dataManager.FilteredDaily.Count,
                    1 => dataManager.FilteredWeekly.Count,
                    2 => dataManager.FilteredMonthly.Count,
                    _ => 0
                };

                Debug.Log($"<color=cyan>[DataLogger]</color> TimeIdx={dataManager.TimeIndex} Stage={dataManager.CurrentStage} Element={dataManager.CurrentElement} → {count} records");

                // Print first 3
                for (int i = 0; i < Mathf.Min(count, 3); i++)
                {
                    WaterQualityBase row = dataManager.TimeIndex switch
                    {
                        0 => dataManager.FilteredDaily[i],
                        1 => dataManager.FilteredWeekly[i],
                        2 => dataManager.FilteredMonthly[i],
                        _ => null
                    };
                    float val = WaterQualityDataManager.GetElementValue(row, dataManager.ElementIndex);
                    Debug.Log($"  [{i}] {row} → {dataManager.CurrentElement}={val}");
                }
                if (count > 3) Debug.Log($"  ... and {count - 3} more");
            };
        }
    }
}
