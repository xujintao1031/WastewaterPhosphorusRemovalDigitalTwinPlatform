using System;
using System.Collections.Generic;
using System.Reflection;

namespace Data
{
    /// <summary>
    /// Marks a class to be deserialized from a specific sheet by name.
    /// If not applied, the serializer uses the class name as the sheet name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SheetAttribute : Attribute
    {
        public string Name;
        public SheetAttribute(string name) => Name = name;
    }

    /// <summary>
    /// Maps a field or property to an xlsx column by header name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public string Name;
        public ColumnAttribute(string name) => Name = name;
    }

    /// <summary>
    /// Deserializes xlsx sheets into typed C# objects via reflection and attributes.
    /// </summary>
    public static class XlsxSerializer
    {
        /// <summary>
        /// Deserialize a sheet into a list of T. The sheet is located by [Sheet] attribute
        /// on T, or by T's class name if no attribute is present.
        /// </summary>
        public static List<T> Deserialize<T>(XlsxWorkbook workbook) where T : new()
        {
            string sheetName = GetSheetName<T>();
            XlsxSheet sheet = workbook[sheetName];
            if (sheet == null)
                throw new Exception($"Sheet '{sheetName}' not found in workbook. Available: {string.Join(", ", workbook.Sheets.ConvertAll(s => s.Name))}");
            return Deserialize<T>(sheet);
        }

        /// <summary>
        /// Deserialize a specific sheet into a list of T.
        /// </summary>
        public static List<T> Deserialize<T>(XlsxSheet sheet) where T : new()
        {
            var bindings = GetBindings<T>(sheet);
            var result = new List<T>();

            foreach (var row in sheet.DataRows)
            {
                var obj = new T();
                foreach (var binding in bindings)
                {
                    string cellValue = row[binding.colIndex];
                    object converted = ConvertTo(cellValue, binding.fieldType);
                    binding.SetValue(obj, converted);
                }
                result.Add(obj);
            }

            return result;
        }

        private static string GetSheetName<T>()
        {
            var attr = typeof(T).GetCustomAttribute<SheetAttribute>();
            return attr?.Name ?? typeof(T).Name;
        }

        private struct FieldBinding
        {
            public int colIndex;
            public Type fieldType;
            public Action<object, object> SetValue;
        }

        private static List<FieldBinding> GetBindings<T>(XlsxSheet sheet)
        {
            var bindings = new List<FieldBinding>();

            // Walk up the type hierarchy so [Column] on base class fields/properties are included
            for (Type type = typeof(T); type != null && type != typeof(object); type = type.BaseType)
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    var binding = CreateBinding(field, field.FieldType, sheet);
                    if (binding.HasValue)
                        bindings.Add(binding.Value);
                }

                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (!prop.CanWrite) continue;
                    var binding = CreateBinding(prop, prop.PropertyType, sheet);
                    if (binding.HasValue)
                        bindings.Add(binding.Value);
                }
            }

            return bindings;
        }

        private static FieldBinding? CreateBinding(MemberInfo member, Type memberType, XlsxSheet sheet)
        {
            var colAttr = member.GetCustomAttribute<ColumnAttribute>();
            if (colAttr == null) return null;

            int colIndex = sheet.IndexOfHeader(colAttr.Name);
            if (colIndex < 0)
                throw new Exception($"Column '{colAttr.Name}' not found in sheet '{sheet.Name}'. Available headers: {string.Join(", ", sheet.Headers)}");

            Action<object, object> setter;
            if (member is FieldInfo fi)
                setter = (obj, val) => fi.SetValue(obj, val);
            else if (member is PropertyInfo pi)
                setter = (obj, val) => pi.SetValue(obj, val);
            else
                return null;

            return new FieldBinding
            {
                colIndex = colIndex,
                fieldType = memberType,
                SetValue = setter,
            };
        }

        private static object ConvertTo(string raw, Type targetType)
        {
            if (string.IsNullOrEmpty(raw)) return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            try
            {
                if (targetType == typeof(string)) return raw;
                if (targetType == typeof(int)) return int.Parse(raw);
                if (targetType == typeof(float)) return float.Parse(raw);
                if (targetType == typeof(double)) return double.Parse(raw);
                if (targetType == typeof(long)) return long.Parse(raw);
                if (targetType == typeof(bool))
                {
                    if (bool.TryParse(raw, out bool b)) return b;
                    if (raw == "1") return true;
                    if (raw == "0") return false;
                }
                if (targetType.IsEnum) return Enum.Parse(targetType, raw, true);
                return Convert.ChangeType(raw, targetType);
            }
            catch
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }
        }
    }
}
