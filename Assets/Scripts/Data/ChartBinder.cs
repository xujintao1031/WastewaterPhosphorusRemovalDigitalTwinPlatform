using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime;

namespace Data
{
    /// <summary>
    /// Connects WaterQualityDataManager to an XCharts LineChart.
    /// Updates the chart whenever dropdown selections change.
    /// X axis = time labels, Y axis = selected element values.
    /// </summary>
    public class ChartBinder : MonoBehaviour
    {
        public WaterQualityDataManager dataManager;
        public LineChart chart;

        private void Start()
        {
            if (dataManager == null || chart == null) return;

            dataManager.OnDataChanged += UpdateChart;
            UpdateChart(); // initial draw
        }

        private void UpdateChart()
        {
            var (labels, values) = dataManager.GetLabelsAndValues();
            if (labels.Count == 0) return;

            string timeLabel = dataManager.TimeIndex switch
            {
                0 => "每日",
                1 => "每周",
                2 => "每月",
                _ => ""
            };
            string title = $"{timeLabel} — {dataManager.CurrentStage} — {dataManager.CurrentElement}";

            chart.ClearData();
            chart.EnsureChartComponent<Title>().text = title;
            chart.EnsureChartComponent<YAxis>().axisLabel.numericFormatter = "0.##";

            for (int i = 0; i < labels.Count; i++)
            {
                chart.AddXAxisData(labels[i]);
                chart.AddData(0, (double)values[i]);
            }
        }
    }
}
