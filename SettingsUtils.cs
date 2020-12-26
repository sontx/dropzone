using System;
using System.IO;
using System.Xml.Serialization;

namespace DropZone
{
    internal static class SettingsUtils
    {
        private static readonly string SettingFileName = "Settings.xml";
        private static object _cachedSettings;
        private static bool _loaded;

        public static T Get<T>() where T : class, new()
        {
            var type = typeof(T);

            if (!_loaded)
            {
                _loaded = true;

                if (File.Exists(SettingFileName))
                {
                    var text = File.ReadAllText(SettingFileName);
                    var serializer = new XmlSerializer(type);
                    var obj = serializer.Deserialize(new StringReader(text)) ?? Activator.CreateInstance<T>();
                    _cachedSettings = obj;
                    return (T)obj;
                }
            }
            else
            {
                if (_cachedSettings != null && _cachedSettings.GetType() == type)
                    return (T)_cachedSettings;
            }

            _cachedSettings = Activator.CreateInstance<T>();
            return (T)_cachedSettings;
        }

        public static void Save()
        {
            if (_cachedSettings == null)
                return;

            var serializer = new XmlSerializer(_cachedSettings.GetType());
            using (var fs = new FileStream(SettingFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                serializer.Serialize(fs, _cachedSettings);
            }
        }
    }
}