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
        public UnityEngine.UI.Image quarterlyImage;
        public Sprite[] quarterlySprites;
        public Sprite[] yearlySprites;

        private void Start()
        {
            if (dataManager == null || chart == null) return;

            dataManager.OnDataChanged += UpdateChart;
            UpdateChart(); // initial draw
        }

        private void UpdateChart()
        {
            int timeIdx = dataManager.TimeIndex;

            bool showChart = timeIdx < 3;
            chart.gameObject.SetActive(showChart);

            if (quarterlyImage != null)
            {
                bool showImage = timeIdx >= 3;
                quarterlyImage.gameObject.SetActive(showImage);
                if (showImage)
                {
                    if (timeIdx == 3 && quarterlySprites != null && dataManager.StageIndex < quarterlySprites.Length)
                    {
                        quarterlyImage.sprite = quarterlySprites[dataManager.StageIndex];
                        quarterlyImage.SetNativeSize();
                    }
                    else if (timeIdx == 4 && yearlySprites != null && dataManager.ElementIndex < yearlySprites.Length)
                    {
                        quarterlyImage.sprite = yearlySprites[dataManager.ElementIndex];
                        quarterlyImage.SetNativeSize();
                    }
                }
            }

            if (!showChart) return;

            var (labels, values) = dataManager.GetLabelsAndValues();
            if (labels.Count == 0) return;

            string timeLabel = dataManager.TimeIndex switch
            {
                0 => "每日",
                1 => "每周",
                2 => "每月",
                3 => "季度",
                4 => "年度",
                _ => ""
            };
            string title = $"{timeLabel} — {dataManager.CurrentStage} — {dataManager.CurrentElement}";

            chart.ClearData();

            string xUnit = timeLabel switch { "每日" => "天", "每周" => "周", "每月" => "月", _ => "" };

            var xAxis = chart.EnsureChartComponent<XAxis>();
            xAxis.axisName.show = true;
            xAxis.axisName.name = xUnit;
            xAxis.axisName.labelStyle.textStyle.color = Color.white;

            chart.EnsureChartComponent<Title>().text = title;

            var yAxis = chart.EnsureChartComponent<YAxis>();
            yAxis.axisLabel.numericFormatter = "0.##";
            yAxis.axisName.show = true;
            yAxis.axisName.name = "mg/L";
            yAxis.axisName.labelStyle.textStyle.color = Color.white;

            var tooltip = chart.EnsureChartComponent<Tooltip>();
            tooltip.titleFormatter = "{b}";
            tooltip.itemFormatter = "{c}";

            for (int i = 0; i < labels.Count; i++)
            {
                chart.AddXAxisData(labels[i]);
                chart.AddData(0, i + 1, (double)values[i]);
            }
        }
    }
}
