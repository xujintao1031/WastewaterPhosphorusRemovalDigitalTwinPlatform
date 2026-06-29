using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Data
{
    /// <summary>
    /// Loads water quality xlsx and provides filtered typed lists based on
    /// three dropdown selections: time period, stage, and element.
    ///
    /// Subscribe to OnDataChanged and read FilteredDaily / FilteredWeekly / FilteredMonthly
    /// depending on TimeIndex. Use GetElementValue() to extract a float from any record.
    /// </summary>
    public class WaterQualityDataManager : MonoBehaviour
    {
        [Header("UI Dropdowns")]
        public Dropdown timeDropdown;
        public Dropdown stageDropdown;
        public Dropdown elementDropdown;

        [Header("Settings")]
        public string xlsxFileName = "污水除磷数字孪生平台全维度水质监测数据.xlsx";
        public int headerRowOffset = 1;

        // ==================== Full loaded data ====================

        private List<DailyWaterQuality> _dailyAll;
        private List<WeeklyWaterQuality> _weeklyAll;
        private List<MonthlyWaterQuality> _monthlyAll;

        // ==================== Filtered data (by current stage) ====================

        public List<DailyWaterQuality> FilteredDaily { get; private set; } = new List<DailyWaterQuality>();
        public List<WeeklyWaterQuality> FilteredWeekly { get; private set; } = new List<WeeklyWaterQuality>();
        public List<MonthlyWaterQuality> FilteredMonthly { get; private set; } = new List<MonthlyWaterQuality>();

        // ==================== Current selections ====================

        public int TimeIndex    { get; private set; }
        public int StageIndex   { get; private set; }
        public int ElementIndex { get; private set; }

        public string CurrentStage   => Stages[StageIndex];
        public string CurrentElement => ElementNames[ElementIndex];

        // ==================== Static config ====================

        public static readonly string[] Stages = { "进水", "厌氧池", "缺氧池", "好氧池", "出水" };
        public static readonly string[] ElementNames = { "COD", "SS", "NH₃⁻N", "NO₃⁻N", "TN", "TP" };

        // ==================== Events ====================

        /// <summary>Fires after any dropdown changes. Read the Filtered* properties and TimeIndex.</summary>
        public event Action OnDataChanged;

        // ==================== Unity lifecycle ====================

        private void Start()
        {
            LoadData();
            SetupDropdowns();
        }

        // ==================== Load ====================

        private void LoadData()
        {
            string p = Path.Combine(Application.streamingAssetsPath, xlsxFileName);
            if (!File.Exists(p))
            {
                Debug.LogError($"xlsx not found: {p}");
                return;
            }

            var wb = XlsxReader.Read(p);
            foreach (var s in wb.Sheets)
                s.HeaderRowIndex = headerRowOffset;

            _dailyAll   = XlsxSerializer.Deserialize<DailyWaterQuality>(wb);
            _weeklyAll  = XlsxSerializer.Deserialize<WeeklyWaterQuality>(wb);
            _monthlyAll = XlsxSerializer.Deserialize<MonthlyWaterQuality>(wb);

            Debug.Log($"Loaded — daily:{_dailyAll.Count}  weekly:{_weeklyAll.Count}  monthly:{_monthlyAll.Count}");
        }

        // ==================== Dropdowns ====================

        private void SetupDropdowns()
        {
            timeDropdown.ClearOptions();
            timeDropdown.AddOptions(new List<string> { "每天", "每周", "每月", "季度", "年度" });
            timeDropdown.onValueChanged.AddListener(idx =>
            {
                TimeIndex = idx;
                stageDropdown.transform.parent.gameObject.SetActive(idx != 4);
                elementDropdown.transform.parent.gameObject.SetActive(idx != 3);
                RebuildFiltered();
            });

            stageDropdown.ClearOptions();
            stageDropdown.AddOptions(new List<string>(Stages));
            stageDropdown.onValueChanged.AddListener(idx => { StageIndex = idx; RebuildFiltered(); });

            elementDropdown.ClearOptions();
            elementDropdown.AddOptions(new List<string>(ElementNames));
            elementDropdown.onValueChanged.AddListener(idx => { ElementIndex = idx; RebuildFiltered(); });

            RebuildFiltered();
        }

        private void RebuildFiltered()
        {
            string stage = Stages[StageIndex];

            FilteredDaily   = _dailyAll.FindAll(d => d.Stage == stage);
            FilteredWeekly  = _weeklyAll.FindAll(w => w.Stage == stage);
            FilteredMonthly = _monthlyAll.FindAll(m => m.Stage == stage);

            OnDataChanged?.Invoke();
        }

        // ==================== Utility ====================

        /// <summary>
        /// Extract the float value for the given element index from any record.
        /// 0=COD, 1=SS, 2=氨氮, 3=硝态氮, 4=总氮, 5=总磷
        /// </summary>
        public static float GetElementValue(WaterQualityBase d, int elemIdx) => elemIdx switch
        {
            0 => d.COD,
            1 => d.SS,
            2 => d.AmmoniaNitrogen,
            3 => d.NitrateNitrogen,
            4 => d.TotalNitrogen,
            5 => d.TotalPhosphorus,
            _ => 0f
        };

        /// <summary>
        /// Quick (labels, values) pair for chart binding — current time-mode + current element.
        /// </summary>
        public (List<string> labels, List<float> values) GetLabelsAndValues()
        {
            var labels = new List<string>();
            var values = new List<float>();

            switch (TimeIndex)
            {
                case 0:
                    foreach (var d in FilteredDaily) { labels.Add(d.Date); values.Add(GetElementValue(d, ElementIndex)); }
                    break;
                case 1:
                    foreach (var w in FilteredWeekly) { labels.Add($"{w.Year} {w.Week}"); values.Add(GetElementValue(w, ElementIndex)); }
                    break;
                case 2:
                    foreach (var m in FilteredMonthly) { labels.Add($"{m.Year} {m.Month}"); values.Add(GetElementValue(m, ElementIndex)); }
                    break;
                case 3:
                    BuildQuarterlyAggregate(labels, values);
                    break;
                case 4:
                    BuildYearlyAggregate(labels, values);
                    break;
            }

            return (labels, values);
        }

        private void BuildQuarterlyAggregate(List<string> labels, List<float> values)
        {
            var groups = new Dictionary<(int year, int quarter), (float sum, int count)>();
            foreach (var d in FilteredDaily)
            {
                if (!TryParseDate(d.Date, out var year, out var month, out _))
                    continue;
                int q = (month - 1) / 3 + 1;
                var key = (year, q);
                var v = GetElementValue(d, ElementIndex);
                if (groups.TryGetValue(key, out var acc))
                    groups[key] = (acc.sum + v, acc.count + 1);
                else
                    groups[key] = (v, 1);
            }

            foreach (var kv in groups.OrderBy(kv => kv.Key.year).ThenBy(kv => kv.Key.quarter))
            {
                labels.Add($"{kv.Key.year} Q{kv.Key.quarter}");
                values.Add(kv.Value.sum / kv.Value.count);
            }
        }

        private void BuildYearlyAggregate(List<string> labels, List<float> values)
        {
            var groups = new Dictionary<int, (float sum, int count)>();
            foreach (var d in FilteredDaily)
            {
                if (!TryParseDate(d.Date, out var year, out _, out _))
                    continue;
                var v = GetElementValue(d, ElementIndex);
                if (groups.TryGetValue(year, out var acc))
                    groups[year] = (acc.sum + v, acc.count + 1);
                else
                    groups[year] = (v, 1);
            }

            foreach (var kv in groups.OrderBy(kv => kv.Key))
            {
                labels.Add(kv.Key.ToString());
                values.Add(kv.Value.sum / kv.Value.count);
            }
        }

        private static bool TryParseDate(string s, out int year, out int month, out int day)
        {
            year = month = day = 0;
            if (string.IsNullOrEmpty(s)) return false;
            // Try common formats
            string[] fmts = { "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd", "yyyyMMdd", "yyyy-M-d", "yyyy/M/d", "yyyy.M.d" };
            if (System.DateTime.TryParseExact(s, fmts, null, System.Globalization.DateTimeStyles.None, out var dt))
            {
                year = dt.Year;
                month = dt.Month;
                day = dt.Day;
                return true;
            }
            return false;
        }
    }
}
