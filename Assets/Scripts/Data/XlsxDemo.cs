using System.IO;
using UnityEngine;

namespace Data
{
    public class XlsxDemo : MonoBehaviour
    {
        [Tooltip("xlsx file name inside StreamingAssets folder")]
        public string fileName = "污水除磷数字孪生平台全维度水质监测数据.xlsx";

        [Tooltip("Title rows to skip before the header row (0 = first row is header).")]
        public int headerRowOffset = 1;

        void Start()
        {
            string path = Path.Combine(Application.streamingAssetsPath, fileName);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"File not found: {path}");
                return;
            }

            XlsxWorkbook workbook = XlsxReader.Read(path);
            Debug.Log($"Loaded workbook with {workbook.SheetCount} sheet(s)");

            // Set header row for all sheets (skip title row)
            foreach (var s in workbook.Sheets)
                s.HeaderRowIndex = headerRowOffset;

            // === Deserialize each sheet into typed C# objects ===
            var daily = XlsxSerializer.Deserialize<DailyWaterQuality>(workbook);
            Debug.Log($"DailyWaterQuality: {daily.Count} records, first = Date:{daily[0].Date} Stage:{daily[0].Stage} COD:{daily[0].COD}");

            var weekly = XlsxSerializer.Deserialize<WeeklyWaterQuality>(workbook);
            Debug.Log($"WeeklyWaterQuality: {weekly.Count} records, first = Year:{weekly[0].Year} Week:{weekly[0].Week} Stage:{weekly[0].Stage}");

            var monthly = XlsxSerializer.Deserialize<MonthlyWaterQuality>(workbook);
            Debug.Log($"MonthlyWaterQuality: {monthly.Count} records, first = Year:{monthly[0].Year} Month:{monthly[0].Month} Stage:{monthly[0].Stage}");
        }
    }
}
