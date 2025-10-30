using Kingmaker;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BetterGameplay.Util
{
    internal static class LocaleStringUtils
    {
        public static readonly string prefix = "Localization";

        public static LocalizedString Empty = new();

        private static readonly Dictionary<string, Dictionary<string, string>> _mappedStrings = [];
        public static LocalizedString UseLocalizedString(this string key, string value = null)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            var map = GetStringMap();
            if (!map.ContainsKey(key))
            {
                map.Add(key, value);
                LocalizationManager.CurrentPack.PutString(key, value);
            }

            return new LocalizedString() { Key = key };
        }

        internal static LocalizedString CreateLocalizedString(string key, string value)
        {
            var localizedString = new LocalizedString() { m_Key = key };
            LocalizationManager.CurrentPack.PutString(key, value);
            return localizedString;
        }

        private static Dictionary<string, string> GetStringMap()
        {
            _mappedStrings.Ensure(Main.ModPath, out var map, Load);
            return map;
        }

        public static bool Ensure<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, out TValue value, Func<TValue> getter)
        {
            if (dic.TryGetValue(key, out value))
                return false;
            dic[key] = value = getter();
            return true;
        }

        static Dictionary<string, string> Load()
        {
            try
            {
                string path = Path.Combine(Main.ModPath, prefix, $"{LocalizationManager.CurrentPack.Locale}.json");

                if (File.Exists(path))
                {
                    var data = Deserialize<Dictionary<string, string>>(path: path);
                    var pack = LocalizationManager.CurrentPack;
                    foreach (var entry in data)
                        pack.PutString(entry.Key, entry.Value);
                    return data;
                }
            }
            catch (Exception) { PFLog.Mods.Error("读取本地文本失败"); }
            return [];
        }

        public static T Deserialize<T>(string path = null, string value = null)
        {
            if (path != null)
            {
                using var sr = new StreamReader(path);
                value = sr.ReadToEnd();
                sr.Close();
            }

            if (value != null)
                return (T)JsonConvert.DeserializeObject(value, typeof(T), _jsetting);
            return default;
        }

        public static JsonSerializerSettings _jsetting = new()
        {
            Converters = [.. DefaultJsonSettings.CommonConverters],
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };
    }
}