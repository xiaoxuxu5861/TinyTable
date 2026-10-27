using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TinyTable
{
    public sealed class DesktopIcon
    {
        public int Index { get; set; }
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string Path { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsSelected { get; set; }
        public bool IsVirtual { get { return string.IsNullOrEmpty(Path); } }
        public string Category { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
    }

    [DataContract]
    public sealed class LayoutSnapshot
    {
        [DataMember] public string Id { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public DateTime CreatedUtc { get; set; }
        [DataMember] public int DesktopWidth { get; set; }
        [DataMember] public int DesktopHeight { get; set; }
        [DataMember] public int SpacingX { get; set; }
        [DataMember] public int SpacingY { get; set; }
        [DataMember] public bool IsAutoSave { get; set; }
        [DataMember] public List<SnapshotItem> Targets { get; set; }
        [DataMember] public List<SnapshotItem> Baseline { get; set; }

        public LayoutSnapshot()
        {
            Targets = new List<SnapshotItem>();
            Baseline = new List<SnapshotItem>();
        }
    }

    [DataContract]
    public sealed class SnapshotItem
    {
        [DataMember] public string Key { get; set; }
        [DataMember] public string DisplayName { get; set; }
        [DataMember] public string Path { get; set; }
        [DataMember] public int X { get; set; }
        [DataMember] public int Y { get; set; }

        public static SnapshotItem FromIcon(DesktopIcon icon)
        {
            return new SnapshotItem
            {
                Key = icon.Key,
                DisplayName = icon.DisplayName,
                Path = icon.Path,
                X = icon.X,
                Y = icon.Y
            };
        }
    }

    [DataContract]
    public sealed class SelectionPreset
    {
        [DataMember] public string Id { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public DateTime UpdatedUtc { get; set; }
        [DataMember] public bool IsAutomatic { get; set; }
        [DataMember] public List<string> Keys { get; set; }

        public SelectionPreset()
        {
            Keys = new List<string>();
            UpdatedUtc = DateTime.UtcNow;
        }
    }

    [DataContract]
    public sealed class SnapshotDatabase
    {
        [DataMember] public List<LayoutSnapshot> Snapshots { get; set; }
        [DataMember] public LayoutSnapshot UndoSnapshot { get; set; }
        [DataMember] public List<string> ProtectedKeys { get; set; }
        [DataMember] public RestorePreferences RestorePreferences { get; set; }
        [DataMember] public AutomationSettings AutomationSettings { get; set; }
        [DataMember] public DesktopListSortMode DisplaySortMode { get; set; }
        [DataMember] public bool DisplaySortDescending { get; set; }
        [DataMember] public List<SelectionPreset> SelectionPresets { get; set; }
        [DataMember] public string ActiveSelectionPresetId { get; set; }

        public SnapshotDatabase()
        {
            Snapshots = new List<LayoutSnapshot>();
            ProtectedKeys = new List<string>();
            RestorePreferences = RestorePreferences.CreateDefault();
            AutomationSettings = AutomationSettings.CreateDefault();
            DisplaySortMode = DesktopListSortMode.DesktopPosition;
            SelectionPresets = new List<SelectionPreset>();
        }
    }

    [DataContract]
    public enum DesktopListSortMode
    {
        [EnumMember] DesktopPosition = 0,
        [EnumMember] FileType = 1,
        [EnumMember] Name = 2,
        [EnumMember] Date = 3,
        [EnumMember] Source = 4
    }

    [DataContract]
    public enum SnapshotNamingMode
    {
        [EnumMember] DateTime = 1,
        [EnumMember] Resolution = 2,
        [EnumMember] WorkArea = 3,
        [EnumMember] CustomPrefix = 4
    }

    [DataContract]
    public sealed class AutomationSettings
    {
        [DataMember] public bool TrayEnabled { get; set; }
        [DataMember] public bool MinimizeToTray { get; set; }
        [DataMember] public bool CloseToTray { get; set; }
        [DataMember] public bool StartWithWindows { get; set; }
        [DataMember] public bool StartMinimized { get; set; }
        [DataMember] public bool AutoSaveEnabled { get; set; }
        [DataMember] public int AutoSaveMinutes { get; set; }
        [DataMember] public bool OnlyWhenLayoutChanged { get; set; }
        [DataMember] public int AutoSaveRetention { get; set; }
        [DataMember] public bool SaveOnExit { get; set; }
        [DataMember] public bool SaveOnShutdown { get; set; }
        [DataMember] public bool RestoreOnProgramStart { get; set; }
        [DataMember] public bool RestoreOnWindowsStart { get; set; }
        [DataMember] public bool SaveOnSessionLock { get; set; }
        [DataMember] public bool RestoreOnSessionUnlock { get; set; }
        [DataMember] public bool DisableAutoArrangeOnStart { get; set; }
        [DataMember] public bool DisableAutoArrangeBeforeRestore { get; set; }
        [DataMember] public bool ShelterShortcutsDuringBrowserUpload { get; set; }
        [DataMember] public SnapshotNamingMode NamingMode { get; set; }
        [DataMember] public string CustomNamePrefix { get; set; }
        [DataMember] public bool SaveWindowPosition { get; set; }
        [DataMember] public int WindowX { get; set; }
        [DataMember] public int WindowY { get; set; }
        [DataMember] public int WindowWidth { get; set; }
        [DataMember] public int WindowHeight { get; set; }
        [DataMember] public string LanguageCode { get; set; }

        public static AutomationSettings CreateDefault()
        {
            return new AutomationSettings
            {
                TrayEnabled = false,
                MinimizeToTray = true,
                CloseToTray = false,
                StartWithWindows = false,
                StartMinimized = false,
                AutoSaveEnabled = false,
                AutoSaveMinutes = 60,
                OnlyWhenLayoutChanged = true,
                AutoSaveRetention = 4,
                SaveOnExit = false,
                SaveOnShutdown = false,
                RestoreOnProgramStart = false,
                RestoreOnWindowsStart = false,
                SaveOnSessionLock = false,
                RestoreOnSessionUnlock = false,
                DisableAutoArrangeOnStart = false,
                DisableAutoArrangeBeforeRestore = true,
                ShelterShortcutsDuringBrowserUpload = false,
                NamingMode = SnapshotNamingMode.Resolution,
                CustomNamePrefix = "TinyTable",
                SaveWindowPosition = true,
                WindowWidth = 1080,
                WindowHeight = 700,
                LanguageCode = "zh-CN"
            };
        }
    }

    [DataContract]
    public enum RestoreArrangementMode
    {
        [EnumMember] CollisionOnly = 1,
        [EnumMember] CompactCurrentOrder = 2,
        [EnumMember] GroupByType = 3,
        [EnumMember] SortByName = 4,
        [EnumMember] SortByDate = 5
    }

    [DataContract]
    public enum IconSortMode
    {
        [EnumMember] Name = 1,
        [EnumMember] Date = 2
    }

    [DataContract]
    public sealed class TypeSortPreference
    {
        [DataMember] public string Category { get; set; }
        [DataMember] public IconSortMode SortMode { get; set; }
    }

    [DataContract]
    public sealed class RestorePreferences
    {
        [DataMember] public RestoreArrangementMode Mode { get; set; }
        [DataMember] public List<TypeSortPreference> TypeSorts { get; set; }

        public RestorePreferences()
        {
            TypeSorts = new List<TypeSortPreference>();
        }

        public static RestorePreferences CreateDefault()
        {
            RestorePreferences result = new RestorePreferences();
            result.Mode = RestoreArrangementMode.CollisionOnly;
            foreach (string category in DesktopItemClassifier.CategoryOrder)
            {
                result.TypeSorts.Add(new TypeSortPreference
                {
                    Category = category,
                    SortMode = IconSortMode.Name
                });
            }
            return result;
        }

        public IconSortMode GetTypeSort(string category)
        {
            TypeSortPreference preference = TypeSorts == null
                ? null
                : TypeSorts.Find(x => string.Equals(
                    x.Category, category, StringComparison.OrdinalIgnoreCase));
            return preference == null ? IconSortMode.Name : preference.SortMode;
        }
    }

    public sealed class RestorePlan
    {
        public RestorePlan()
        {
            Positions = new Dictionary<string, IconPosition>(StringComparer.OrdinalIgnoreCase);
            Overflow = new List<DesktopIcon>();
            Missing = new List<SnapshotItem>();
            ReservedEmpty = new List<IconPosition>();
        }

        public Dictionary<string, IconPosition> Positions { get; private set; }
        public List<DesktopIcon> Overflow { get; private set; }
        public List<SnapshotItem> Missing { get; private set; }
        public List<IconPosition> ReservedEmpty { get; private set; }
        public int ArrangementStartColumn { get; set; }
        public int TotalColumns { get; set; }
        public int AvailableArrangementColumns { get; set; }
        public int ArrangedItemCount { get; set; }
    }

    public sealed class IconPosition
    {
        public IconPosition() { }
        public IconPosition(int x, int y) { X = x; Y = y; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
