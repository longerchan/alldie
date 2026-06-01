using System;
using Newtonsoft.Json;

namespace FrostShelter.Utils
{
    /// <summary>
    /// JSON 序列化辅助工具。
    /// </summary>
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings DefaultSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
        };

        public static string ToJson<T>(T obj, bool prettyPrint = true)
        {
            var settings = prettyPrint
                ? DefaultSettings
                : new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                };
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, DefaultSettings);
        }

        public static bool TryFromJson<T>(string json, out T result)
        {
            try
            {
                result = FromJson<T>(json);
                return result != null;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
