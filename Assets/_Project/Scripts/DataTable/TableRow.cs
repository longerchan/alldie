using System.Collections.Generic;

namespace FrostShelter.DataTable
{
    /// <summary>
    /// 数据表行。键值对形式存储一行数据。
    /// </summary>
    public class TableRow
    {
        private readonly Dictionary<string, string> _fields = new();

        public string this[string key]
        {
            get => _fields.TryGetValue(key, out var v) ? v : string.Empty;
            set => _fields[key] = value;
        }

        public bool HasField(string key) => _fields.ContainsKey(key);

        public int GetInt(string key, int defaultValue = 0)
        {
            return int.TryParse(_fields.GetValueOrDefault(key), out var v) ? v : defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            return float.TryParse(_fields.GetValueOrDefault(key), out var v) ? v : defaultValue;
        }

        public string GetString(string key, string defaultValue = "")
        {
            return _fields.TryGetValue(key, out var v) ? v : defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            var val = _fields.GetValueOrDefault(key)?.ToLower();
            return val == "1" || val == "true" || val == "yes";
        }

        public int[] GetIntArray(string key, char separator = '|')
        {
            var raw = _fields.GetValueOrDefault(key);
            if (string.IsNullOrEmpty(raw)) return new int[0];
            var parts = raw.Split(separator);
            var result = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                int.TryParse(parts[i], out result[i]);
            return result;
        }

        public float[] GetFloatArray(string key, char separator = '|')
        {
            var raw = _fields.GetValueOrDefault(key);
            if (string.IsNullOrEmpty(raw)) return new float[0];
            var parts = raw.Split(separator);
            var result = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                float.TryParse(parts[i], out result[i]);
            return result;
        }

        public void SetField(string key, string value) => _fields[key] = value;
    }
}
