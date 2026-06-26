using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace Data
{
    /// <summary>
    /// Runtime xlsx reader. Reads an xlsx file (zip of xml) and returns a workbook
    /// with all sheets and their row data accessible by column name or index.
    /// </summary>
    public static class XlsxReader
    {
        private static readonly XNamespace Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace NsR = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace NsPRel = "http://schemas.openxmlformats.org/package/2006/relationships";

        public static XlsxWorkbook Read(string filePath)
        {
            // Use FileShare.ReadWrite so it works even when Excel has the file open
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Read(fs);
        }

        public static XlsxWorkbook Read(Stream stream)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            return ReadArchive(archive);
        }

        public static XlsxWorkbook Read(byte[] data)
        {
            using var stream = new MemoryStream(data);
            return Read(stream);
        }

        private static XlsxWorkbook ReadArchive(ZipArchive archive)
        {
            // Read shared strings
            List<string> sharedStrings = ReadSharedStrings(archive);

            // Read workbook to get sheet names
            var sheetsMeta = ReadWorkbookSheets(archive);

            var workbook = new XlsxWorkbook();
            foreach (var (name, sheetPath) in sheetsMeta)
            {
                var sheet = ReadSheet(archive, sheetPath, sharedStrings);
                sheet.Name = name;
                workbook.Sheets.Add(sheet);
            }

            return workbook;
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var result = new List<string>();
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null) return result;

            using var stream = entry.Open();
            var doc = XDocument.Load(stream);
            var root = doc.Element(Ns + "sst");
            if (root == null) return result;

            foreach (var si in root.Elements(Ns + "si"))
            {
                // Simple text
                var t = si.Element(Ns + "t");
                if (t != null)
                {
                    result.Add(t.Value);
                    continue;
                }

                // Rich text — concatenate all <r><t> pieces
                var sb = new System.Text.StringBuilder();
                foreach (var r in si.Elements(Ns + "r"))
                {
                    var rt = r.Element(Ns + "t");
                    if (rt != null) sb.Append(rt.Value);
                }
                result.Add(sb.ToString());
            }

            return result;
        }

        private static List<(string name, string path)> ReadWorkbookSheets(ZipArchive archive)
        {
            var result = new List<(string, string)>();
            var entry = archive.GetEntry("xl/workbook.xml");
            if (entry == null) return result;

            using var stream = entry.Open();
            var doc = XDocument.Load(stream);
            var root = doc.Element(Ns + "workbook");
            if (root == null) return result;

            var sheetsEl = root.Element(Ns + "sheets");
            if (sheetsEl == null) return result;

            foreach (var sheet in sheetsEl.Elements(Ns + "sheet"))
            {
                string name = sheet.Attribute("name")?.Value ?? "Sheet";
                string relId = sheet.Attribute(NsR + "id")?.Value ?? "";
                string path = ResolveSheetPath(archive, relId);
                result.Add((name, path));
            }

            return result;
        }

        private static string ResolveSheetPath(ZipArchive archive, string relId)
        {
            var relsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels");
            if (relsEntry == null) return "xl/worksheets/sheet1.xml";

            using var stream = relsEntry.Open();
            var doc = XDocument.Load(stream);
            var root = doc.Element(NsPRel + "Relationships");
            if (root == null) return "xl/worksheets/sheet1.xml";

            foreach (var rel in root.Elements(NsPRel + "Relationship"))
            {
                if (rel.Attribute("Id")?.Value == relId)
                {
                    string target = rel.Attribute("Target")?.Value ?? "";
                    return "xl/" + target;
                }
            }

            return "xl/worksheets/sheet1.xml";
        }

        private static XlsxSheet ReadSheet(ZipArchive archive, string sheetPath, List<string> sharedStrings)
        {
            var sheet = new XlsxSheet();
            var entry = archive.GetEntry(sheetPath);
            if (entry == null) return sheet;

            using var stream = entry.Open();
            var doc = XDocument.Load(stream);
            var root = doc.Element(Ns + "worksheet");
            if (root == null) return sheet;

            var sheetData = root.Element(Ns + "sheetData");
            if (sheetData == null) return sheet;

            foreach (var rowEl in sheetData.Elements(Ns + "row"))
            {
                var row = new XlsxRow();
                int maxColIndex = -1;

                foreach (var cell in rowEl.Elements(Ns + "c"))
                {
                    string cellRef = cell.Attribute("r")?.Value ?? "";
                    int colIndex = ParseColumnIndex(cellRef);
                    string cellType = cell.Attribute("t")?.Value ?? "";
                    string value = ReadCellValue(cell, cellType, sharedStrings);

                    // Ensure the values list is large enough
                    while (row.values.Count <= colIndex)
                        row.values.Add(null);
                    row.values[colIndex] = value;

                    if (colIndex > maxColIndex)
                        maxColIndex = colIndex;
                }

                // Trim trailing nulls (but keep internal gaps as empty strings)
                while (row.values.Count > maxColIndex + 1)
                    row.values.RemoveAt(row.values.Count - 1);
                // Fill internal gaps with empty strings for consistent indexing
                for (int i = 0; i < row.values.Count; i++)
                {
                    if (row.values[i] == null)
                        row.values[i] = "";
                }

                if (row.values.Count > 0)
                {
                    row.owner = sheet;
                    sheet.Rows.Add(row);
                }
            }

            // Default: use row 0 as header
            sheet.HeaderRowIndex = 0;

            return sheet;
        }

        private static string ReadCellValue(XElement cell, string cellType, List<string> sharedStrings)
        {
            if (cellType == "inlineStr")
            {
                var isEl = cell.Element(Ns + "is");
                var t = isEl?.Element(Ns + "t");
                return t?.Value ?? "";
            }

            var v = cell.Element(Ns + "v");
            string rawValue = v?.Value ?? "";

            if (cellType == "s") // shared string
            {
                if (int.TryParse(rawValue, out int idx) && idx >= 0 && idx < sharedStrings.Count)
                    return sharedStrings[idx];
                return "";
            }

            if (cellType == "b") // boolean
            {
                return rawValue == "1" ? "TRUE" : "FALSE";
            }

            // number, empty type, or formula — return as-is
            return rawValue;
        }

        /// <summary>
        /// Parse column letters to 0-based index. A=0, B=1, ..., Z=25, AA=26, etc.
        /// </summary>
        private static int ParseColumnIndex(string cellRef)
        {
            int col = 0;
            int i = 0;
            while (i < cellRef.Length && char.IsLetter(cellRef[i]))
            {
                col = col * 26 + (char.ToUpper(cellRef[i]) - 'A' + 1);
                i++;
            }
            return col - 1; // 0-based
        }
    }

    public class XlsxWorkbook
    {
        public List<XlsxSheet> Sheets = new List<XlsxSheet>();

        public XlsxSheet this[int index] => Sheets[index];
        public XlsxSheet this[string name]
        {
            get
            {
                foreach (var s in Sheets)
                    if (s.Name == name) return s;
                return null;
            }
        }

        public int SheetCount => Sheets.Count;
    }

    public class XlsxSheet
    {
        public string Name;

        /// <summary>
        /// The rows of this sheet. Includes all rows including header/title rows.
        /// </summary>
        public List<XlsxRow> Rows = new List<XlsxRow>();

        private int _headerRowIndex = -1;
        private List<string> _headers = new List<string>();
        private Dictionary<string, int> _headerMap;

        /// <summary>
        /// The 0-based row index used for column headers.
        /// Set to -1 to disable header mode (all rows are data).
        /// Default after reading: 0 (first row is header).
        /// </summary>
        public int HeaderRowIndex
        {
            get => _headerRowIndex;
            set
            {
                _headerRowIndex = value;
                RebuildHeaders();
            }
        }

        /// <summary>
        /// Convenience: true when HeaderRowIndex >= 0.
        /// </summary>
        public bool HasHeaders => _headerRowIndex >= 0 && _headerRowIndex < Rows.Count;

        /// <summary>
        /// Current header names (from the row at HeaderRowIndex).
        /// </summary>
        public List<string> Headers => _headers;

        /// <summary>
        /// Convenience method: set which row contains column headers and rebuild.
        /// </summary>
        public void UseHeaderRow(int rowIndex)
        {
            HeaderRowIndex = rowIndex;
        }

        private void RebuildHeaders()
        {
            _headers.Clear();
            _headerMap = null;

            if (_headerRowIndex < 0 || _headerRowIndex >= Rows.Count)
                return;

            var headerRow = Rows[_headerRowIndex];
            for (int i = 0; i < headerRow.values.Count; i++)
                _headers.Add(headerRow.values[i]);
        }

        /// <summary>
        /// Returns the 0-based index of a header name, or -1 if not found.
        /// </summary>
        public int IndexOfHeader(string name)
        {
            if (_headerMap == null)
            {
                _headerMap = new Dictionary<string, int>();
                for (int i = 0; i < _headers.Count; i++)
                {
                    string key = _headers[i]?.Trim();
                    if (!string.IsNullOrEmpty(key) && !_headerMap.ContainsKey(key))
                        _headerMap[key] = i;
                }
            }
            return _headerMap.TryGetValue(name.Trim(), out int idx) ? idx : -1;
        }

        /// <summary>
        /// Returns all data rows (rows after the header row).
        /// When HeaderRowIndex is -1, all rows are data rows.
        /// </summary>
        public IEnumerable<XlsxRow> DataRows
        {
            get
            {
                int start = _headerRowIndex >= 0 ? _headerRowIndex + 1 : 0;
                for (int i = start; i < Rows.Count; i++)
                    yield return Rows[i];
            }
        }

        public int RowCount => Rows.Count;
        public int DataRowCount => _headerRowIndex >= 0 ? Rows.Count - (_headerRowIndex + 1) : Rows.Count;
    }

    public class XlsxRow
    {
        /// <summary>
        /// Cell values by column index. 0-based, gap cells are empty strings.
        /// </summary>
        internal List<string> values = new List<string>();

        internal XlsxSheet owner;

        public int Count => values.Count;

        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= values.Count) return null;
                return values[index];
            }
        }

        public string this[string columnName]
        {
            get
            {
                if (owner == null) return null;
                int idx = owner.IndexOfHeader(columnName);
                return idx >= 0 ? this[idx] : null;
            }
        }

        public T Get<T>(int index, T defaultValue = default)
        {
            string v = this[index];
            return ConvertValue(v, defaultValue);
        }

        public T Get<T>(string columnName, T defaultValue = default)
        {
            string v = this[columnName];
            return ConvertValue(v, defaultValue);
        }

        private static T ConvertValue<T>(string v, T defaultValue)
        {
            if (string.IsNullOrEmpty(v)) return defaultValue;
            try
            {
                Type t = typeof(T);
                if (t == typeof(string)) return (T)(object)v;
                if (t == typeof(int)) return (T)(object)int.Parse(v);
                if (t == typeof(float)) return (T)(object)float.Parse(v);
                if (t == typeof(double)) return (T)(object)double.Parse(v);
                if (t == typeof(long)) return (T)(object)long.Parse(v);
                if (t == typeof(bool))
                {
                    if (bool.TryParse(v, out bool b)) return (T)(object)b;
                    if (v == "1") return (T)(object)true;
                    if (v == "0") return (T)(object)false;
                    return defaultValue;
                }
                if (t.IsEnum) return (T)Enum.Parse(t, v, true);
                return (T)Convert.ChangeType(v, t);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
