using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;

namespace TinyTable
{
    public sealed class SnapshotStore
    {
        private readonly string _folder;
        private readonly string _path;
        private readonly bool _portable;
        private readonly object _sync = new object();

        public SnapshotStore()
        {
            string executableFolder = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            string localMarker = Path.Combine(
                executableFolder, "TinyTable.local-data");
            _portable = File.Exists(Path.Combine(
                executableFolder, "TinyTable.portable"));
            _folder = File.Exists(localMarker)
                ? Path.Combine(executableFolder, "data")
                : Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "TinyTable");
            _path = Path.Combine(_folder, "layout-snapshots.json");
        }

        public SnapshotDatabase Load()
        {
            lock (_sync)
            {
                try
                {
                    if (!File.Exists(_path))
                        return CreateEmptyDatabase();
                    using (FileStream stream = File.OpenRead(_path))
                    {
                        DataContractJsonSerializer serializer =
                            new DataContractJsonSerializer(typeof(SnapshotDatabase));
                        SnapshotDatabase data = serializer.ReadObject(stream) as SnapshotDatabase;
                        if (data == null)
                            return CreateEmptyDatabase();
                        if (data.Snapshots == null)
                            data.Snapshots = new System.Collections.Generic.List<LayoutSnapshot>();
                        if (data.ProtectedKeys == null)
                            data.ProtectedKeys = new System.Collections.Generic.List<string>();
                        if (data.SelectionPresets == null)
                            data.SelectionPresets = new System.Collections.Generic.List<SelectionPreset>();
                        foreach (SelectionPreset preset in data.SelectionPresets)
                        {
                            if (string.IsNullOrEmpty(preset.Id))
                                preset.Id = Guid.NewGuid().ToString("N");
                            if (preset.UpdatedUtc == DateTime.MinValue)
                                preset.UpdatedUtc = DateTime.UtcNow;
                            if (preset.Keys == null)
                                preset.Keys = new System.Collections.Generic.List<string>();
                        }
                        if (data.RestorePreferences == null)
                            data.RestorePreferences = RestorePreferences.CreateDefault();
                        if (data.RestorePreferences.TypeSorts == null
                            || data.RestorePreferences.TypeSorts.Count == 0)
                            data.RestorePreferences.TypeSorts =
                                RestorePreferences.CreateDefault().TypeSorts;
                        if (data.AutomationSettings == null)
                            data.AutomationSettings = AutomationSettings.CreateDefault();
                        if (string.IsNullOrEmpty(data.AutomationSettings.LanguageCode))
                            data.AutomationSettings.LanguageCode = "zh-CN";
                        return data;
                    }
                }
                catch
                {
                    return CreateEmptyDatabase();
                }
            }
        }

        private SnapshotDatabase CreateEmptyDatabase()
        {
            SnapshotDatabase database = new SnapshotDatabase();
            if (_portable)
            {
                database.AutomationSettings.LanguageCode =
                    Localization.GetBestLanguageCode(
                        System.Globalization.CultureInfo.CurrentUICulture);
            }
            else
            {
                string initialLanguage = ReadInitialLanguage();
                if (!string.IsNullOrEmpty(initialLanguage))
                    database.AutomationSettings.LanguageCode = initialLanguage;
            }
            return database;
        }

        private static string ReadInitialLanguage()
        {
            try
            {
                string folder = Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(folder, "TinyTable.initial-language");
                if (!File.Exists(path))
                    return null;
                string code = File.ReadAllText(path).Trim();
                foreach (LanguageInfo language in Localization.SupportedLanguages)
                {
                    if (string.Equals(language.Code, code,
                        StringComparison.OrdinalIgnoreCase))
                        return language.Code;
                }
            }
            catch { }
            return null;
        }

        public void Save(SnapshotDatabase data)
        {
            lock (_sync)
            {
                Directory.CreateDirectory(_folder);
                string temp = _path + ".tmp";
                using (FileStream stream = File.Create(temp))
                {
                    DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(typeof(SnapshotDatabase));
                    serializer.WriteObject(stream, data);
                }
                if (File.Exists(_path))
                    File.Replace(temp, _path, null);
                else
                    File.Move(temp, _path);
            }
        }
    }
}
