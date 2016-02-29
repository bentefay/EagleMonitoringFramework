using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Emf.Web.Ui.Services.Settings
{
    public class SettingStore
    {
        private static readonly string _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BuildMonitor");

        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

        public void Set<T>(string settingName, T setting)
        {
            _settings[settingName] = setting;

            var filePath = GetFilePath(settingName);
            var directory = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var streamWriter = new StreamWriter(fileStream))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                jsonWriter.Formatting = Formatting.Indented;

                var serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, setting);
            }
        }

        public bool TryGet<T>(string settingName, out T setting)
        {
            object settingUntyped;
            if (_settings.TryGetValue(settingName, out settingUntyped))
            {
                setting = (T)settingUntyped;
                return true;
            }

            var filePath = GetFilePath(settingName);

            if (!File.Exists(filePath))
            {
                setting = default(T);
                return false;
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                setting = serializer.Deserialize<T>(jsonReader);
                return true;
            }
        }

        public T GetOrCreate<T>(string settingName, Func<T> factory)
        {
            T setting;
            return TryGet(settingName, out setting) ? setting : factory();
        }

        public bool Delete(string settingName)
        {
            var filePath = GetFilePath(settingName);

            _settings.Remove(settingName);

            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }

        private static string GetFilePath(string settingName)
        {
            return Path.Combine(_basePath, settingName + ".json");
        }
    }

    public class SettingStore<T>
    {
        private readonly SettingStore _settingStore;
        private readonly string _settingName;

        public SettingStore(SettingStore settingStore, string settingName)
        {
            _settingStore = settingStore;
            _settingName = settingName;
        }

        public void Set(T setting)
        {
            _settingStore.Set(_settingName, setting);
        }

        public bool TryGet(out T setting)
        {
            return _settingStore.TryGet(_settingName, out setting);
        }

        public T GetOrCreate(Func<T> factory)
        {
            T setting;
            return TryGet(out setting) ? setting : factory();
        }

        public bool Delete()
        {
            return _settingStore.Delete(_settingName);
        }
    }

    public static class SettingStoreExtensions
    {
        public static SettingStore<T> ForKey<T>(this SettingStore store, SettingKey<T> settingKey)
        {
            return new SettingStore<T>(store, settingKey.SettingName);
        }
    }

    public class SettingKey<T>
    {
        public SettingKey(string settingName)
        {
            SettingName = settingName;
        }

        public string SettingName { get; }
    }
}