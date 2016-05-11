using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Emf.Web.Ui.Hubs.Core;
using Newtonsoft.Json;

namespace Emf.Web.Ui.Services.Settings
{
    public class SettingStore
    {
        private static readonly string _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BuildMonitor");

        private readonly Dictionary<string, SettingRecord> _settings = new Dictionary<string, SettingRecord>();

        public void Set<T>(string settingId, T setting)
        {
            _settings.GetValueOrAdd(settingId, () => new SettingRecord()).Value = setting;

            var filePath = GetFilePath(settingId);
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

        public bool TryGet<T>(string settingId, out T setting)
        {
            SettingRecord record;
            if (_settings.TryGetValue(settingId, out record))
            {
                setting = (T)record.Value;
                return true;
            }

            var filePath = GetFilePath(settingId);

            if (!File.Exists(filePath))
            {
                setting = default(T);
                return false;
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                if (fileStream.Length == 0)
                {
                    setting = default(T);
                    return false;
                }

                var serializer = new JsonSerializer();
                setting = serializer.Deserialize<T>(jsonReader);
                _settings.GetValueOrAdd(settingId, () => new SettingRecord()).Value = setting;
                return true;
            }
        }

        public T GetOrCreate<T>(string settingId, Func<T> factory)
        {
            T setting;
            return TryGet(settingId, out setting) ? setting : factory();
        }

        public IObservable<T> GetObservable<T>(string settingId) => _settings.GetValueOrAdd(settingId, () => new SettingRecord()).Observable.OfType<T>();

        public bool Delete(string settingId)
        {
            var filePath = GetFilePath(settingId);

            SettingRecord record;
            if (_settings.TryGetValue(settingId, out record))
                record.Dispose();

            _settings.Remove(settingId);

            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }

        private static string GetFilePath(string settingName)
        {
            return Path.Combine(_basePath, settingName + ".json");
        }

        private class SettingRecord : IDisposable
        {
            private readonly ReplaySubject<object> _subject = new ReplaySubject<object>();
            private object _value;

            public object Value
            {
                get { return _value; }
                set
                {
                    if (Equals(_value, value))
                        return;
                    _value = value;
                    _subject.OnNext(value);
                }
            }

            public IObservable<object> Observable => _subject;

            public void Dispose()
            {
                _subject.OnCompleted();
                _subject.Dispose();
            }
        }
    }

    public class SettingStore<T>
    {
        private readonly SettingStore _settingStore;
        private readonly string _settingId;

        public SettingStore(SettingStore settingStore, string settingId)
        {
            _settingStore = settingStore;
            _settingId = settingId;
        }

        public void Set(T setting)
        {
            _settingStore.Set(_settingId, setting);
        }

        public bool TryGet(out T setting)
        {
            return _settingStore.TryGet(_settingId, out setting);
        }

        public IObservable<T> GetObservable()
        {
            return _settingStore.GetObservable<T>(_settingId);
        }

        public T GetOrCreate(Func<T> factory)
        {
            T setting;
            return TryGet(out setting) ? setting : factory();
        }

        public bool Delete()
        {
            return _settingStore.Delete(_settingId);
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