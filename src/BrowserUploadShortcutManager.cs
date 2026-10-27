using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace TinyTable
{
    public sealed class BrowserUploadShortcutManager : IDisposable
    {
        private const string StateFileName = "browser-upload-shortcuts.json";
        private const string ShelterFolderName = "browser-upload-shortcuts";
        private static readonly HashSet<string> BrowserProcesses =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "chrome", "msedge", "firefox", "brave", "vivaldi",
                "opera", "opera_gx", "arc"
            };

        private readonly string _dataFolder;
        private readonly string _statePath;
        private readonly string _shelterFolder;
        private readonly DesktopIconService _desktop;
        private readonly object _sync = new object();
        private Thread _thread;
        private volatile bool _stopRequested;
        private volatile bool _enabled;
        private bool _sheltered;
        private bool _pickerHandled;

        public BrowserUploadShortcutManager(string dataFolder)
        {
            _dataFolder = dataFolder;
            _statePath = Path.Combine(_dataFolder, StateFileName);
            _shelterFolder = Path.Combine(_dataFolder, ShelterFolderName);
            _desktop = new DesktopIconService();
        }

        public void Configure(bool enabled)
        {
            _enabled = enabled;
            if (enabled && _thread == null)
            {
                _thread = new Thread(MonitorLoop);
                _thread.IsBackground = true;
                _thread.Name = "TinyTable browser upload monitor";
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.Start();
            }
            if (!enabled)
                RestorePending();
        }

        public void RecoverAtStartup()
        {
            lock (_sync)
                RestorePendingCore();
        }

        private void MonitorLoop()
        {
            while (!_stopRequested)
            {
                try
                {
                    bool pickerOpen = _enabled && HasBrowserOpenFileDialog();
                    if (pickerOpen && !_pickerHandled)
                    {
                        try
                        {
                            lock (_sync)
                                ShelterShortcutsCore();
                        }
                        finally
                        {
                            _pickerHandled = true;
                        }
                    }
                    else if (!pickerOpen && (_sheltered || _pickerHandled))
                    {
                        lock (_sync)
                            RestorePendingCore();
                        _pickerHandled = false;
                    }
                }
                catch { }
                Thread.Sleep(_enabled ? 350 : 1000);
            }
            lock (_sync)
                RestorePendingCore();
        }

        private static bool HasBrowserOpenFileDialog()
        {
            bool found = false;
            NativeMethods.EnumWindows(delegate(IntPtr window, IntPtr parameter)
            {
                if (!NativeMethods.IsWindowVisible(window))
                    return true;
                StringBuilder className = new StringBuilder(64);
                NativeMethods.GetClassName(window, className, className.Capacity);
                if (!string.Equals(className.ToString(), "#32770",
                    StringComparison.Ordinal))
                    return true;

                uint processId;
                NativeMethods.GetWindowThreadProcessId(window, out processId);
                string processName;
                try
                {
                    using (Process process = Process.GetProcessById((int)processId))
                        processName = process.ProcessName;
                }
                catch { return true; }
                if (!BrowserProcesses.Contains(processName))
                    return true;

                StringBuilder title = new StringBuilder(260);
                NativeMethods.GetWindowText(window, title, title.Capacity);
                string text = title.ToString();
                if (IsSaveDialogTitle(text))
                    return true;
                if (!HasShellFolderView(window))
                    return true;
                found = true;
                return false;
            }, IntPtr.Zero);
            return found;
        }

        private static bool HasShellFolderView(IntPtr parent)
        {
            bool found = false;
            NativeMethods.EnumChildWindows(parent, delegate(IntPtr child, IntPtr value)
            {
                StringBuilder className = new StringBuilder(64);
                NativeMethods.GetClassName(child, className, className.Capacity);
                if (string.Equals(className.ToString(), "SHELLDLL_DefView",
                    StringComparison.Ordinal))
                {
                    found = true;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return found;
        }

        private static bool IsSaveDialogTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return false;
            string value = title.Trim().ToLowerInvariant();
            string[] saveMarkers =
            {
                "save", "另存", "保存", "儲存", "speichern", "enregistrer",
                "guardar", "salva", "salvar", "opslaan", "zapisz",
                "сохран", "зберег", "kaydet", "保存", "저장", "lưu",
                "simpan", "uložit"
            };
            return saveMarkers.Any(value.Contains);
        }

        private void ShelterShortcutsCore()
        {
            ShelterState pending = LoadState();
            if (pending != null && pending.Items != null
                && HasPendingFiles(pending))
            {
                _sheltered = true;
                return;
            }

            // Auto-arrange could shift every remaining desktop icon when the
            // shortcuts disappear. Skip the feature instead of disturbing the
            // user's layout.
            if (_desktop.IsAutoArrangeEnabled())
                return;

            List<DesktopIcon> icons = _desktop.ReadIcons();
            Dictionary<string, DesktopIcon> byPath = icons
                .Where(x => !string.IsNullOrEmpty(x.Path))
                .GroupBy(x => Path.GetFullPath(x.Path),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(),
                    StringComparer.OrdinalIgnoreCase);
            List<string> paths = EnumerateDesktopShortcuts().ToList();
            if (paths.Count == 0)
                return;

            Directory.CreateDirectory(_dataFolder);
            Directory.CreateDirectory(_shelterFolder);
            ShelterState state = new ShelterState
            {
                CreatedUtc = DateTime.UtcNow,
                Items = new List<ShelteredShortcut>()
            };
            foreach (string path in paths)
            {
                string fullPath = Path.GetFullPath(path);
                DesktopIcon icon;
                byPath.TryGetValue(fullPath, out icon);
                state.Items.Add(new ShelteredShortcut
                {
                    OriginalPath = fullPath,
                    ShelterPath = Path.Combine(
                        _shelterFolder,
                        Guid.NewGuid().ToString("N") + Path.GetExtension(path)),
                    X = icon == null ? int.MinValue : icon.X,
                    Y = icon == null ? int.MinValue : icon.Y
                });
            }

            SaveState(state);
            foreach (ShelteredShortcut item in state.Items)
            {
                try
                {
                    if (!File.Exists(item.OriginalPath))
                        continue;
                    MoveWithoutOverwrite(item.OriginalPath, item.ShelterPath);
                    item.Moved = true;
                    SaveState(state);
                }
                catch
                {
                    // Public desktop shortcuts may require elevation. Skip any
                    // item that cannot be moved and continue with safe items.
                }
            }
            _sheltered = state.Items.Any(x => x.Moved);
            if (!_sheltered && !HasPendingFiles(state))
            {
                try { File.Delete(_statePath); } catch { }
            }
        }

        private IEnumerable<string> EnumerateDesktopShortcuts()
        {
            string[] folders =
            {
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
            };
            HashSet<string> seen = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
            foreach (string folder in folders)
            {
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                    continue;
                IEnumerable<string> files;
                try { files = Directory.EnumerateFiles(folder).ToArray(); }
                catch { continue; }
                foreach (string file in files)
                {
                    string extension = Path.GetExtension(file);
                    if ((string.Equals(extension, ".lnk", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(extension, ".url", StringComparison.OrdinalIgnoreCase))
                        && seen.Add(file))
                        yield return file;
                }
            }
        }

        private void RestorePending()
        {
            lock (_sync)
                RestorePendingCore();
        }

        private void RestorePendingCore()
        {
            ShelterState state = LoadState();
            if (state == null || state.Items == null)
            {
                _sheltered = false;
                return;
            }

            Dictionary<string, IconPosition> positions =
                new Dictionary<string, IconPosition>(StringComparer.OrdinalIgnoreCase);
            bool allRestored = true;
            foreach (ShelteredShortcut item in state.Items)
            {
                if (!File.Exists(item.ShelterPath))
                    continue;
                try
                {
                    if (File.Exists(item.OriginalPath))
                    {
                        if (FilesEqual(item.OriginalPath, item.ShelterPath))
                        {
                            File.Delete(item.ShelterPath);
                            item.Moved = false;
                            SaveState(state);
                        }
                        else
                        {
                            // Never overwrite a new or modified shortcut. Keep
                            // both by returning the sheltered item under a clear
                            // recovery name beside the new one.
                            string recovered = CreateRecoveredPath(item.OriginalPath);
                            MoveWithoutOverwrite(item.ShelterPath, recovered);
                            item.Moved = false;
                            item.PositionPending = false;
                            SaveState(state);
                        }
                        continue;
                    }
                    string parent = Path.GetDirectoryName(item.OriginalPath);
                    if (!Directory.Exists(parent))
                        Directory.CreateDirectory(parent);
                    MoveWithoutOverwrite(item.ShelterPath, item.OriginalPath);
                    item.Moved = false;
                    if (item.X != int.MinValue && item.Y != int.MinValue)
                    {
                        item.PositionPending = true;
                        string key = DesktopIconService.BuildKey(
                            null, item.OriginalPath);
                        positions[key] = new IconPosition(item.X, item.Y);
                    }
                    SaveState(state);
                }
                catch { allRestored = false; }
            }

            // A crash can happen after the shortcut has returned but before
            // the state file is updated. Detect that state and still restore
            // the saved coordinate.
            foreach (ShelteredShortcut item in state.Items)
            {
                if (File.Exists(item.ShelterPath))
                    continue;
                if (item.Moved && File.Exists(item.OriginalPath))
                {
                    item.Moved = false;
                    item.PositionPending = item.X != int.MinValue
                        && item.Y != int.MinValue;
                    SaveState(state);
                }
                if (item.PositionPending && File.Exists(item.OriginalPath))
                {
                    string key = DesktopIconService.BuildKey(
                        null, item.OriginalPath);
                    positions[key] = new IconPosition(item.X, item.Y);
                }
            }

            if (positions.Count > 0)
            {
                try
                {
                    int restored = _desktop.ApplyPositions(positions);
                    if (restored == positions.Count)
                    {
                        foreach (ShelteredShortcut item in state.Items)
                            item.PositionPending = false;
                        SaveState(state);
                    }
                    else
                    {
                        allRestored = false;
                    }
                }
                catch { allRestored = false; }
            }
            if (allRestored && !HasPendingFiles(state))
            {
                try { File.Delete(_statePath); } catch { }
                try
                {
                    if (Directory.Exists(_shelterFolder)
                        && !Directory.EnumerateFileSystemEntries(_shelterFolder).Any())
                        Directory.Delete(_shelterFolder);
                }
                catch { }
                _sheltered = false;
            }
            else
            {
                _sheltered = HasPendingFiles(state);
            }
        }

        private static bool HasPendingFiles(ShelterState state)
        {
            return state.Items != null
                && state.Items.Any(x => !string.IsNullOrEmpty(x.ShelterPath)
                    && (File.Exists(x.ShelterPath)
                        || x.Moved
                        || x.PositionPending));
        }

        private static string CreateRecoveredPath(string originalPath)
        {
            string folder = Path.GetDirectoryName(originalPath);
            string name = Path.GetFileNameWithoutExtension(originalPath);
            string extension = Path.GetExtension(originalPath);
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            for (int index = 0; index < 1000; index++)
            {
                string suffix = index == 0 ? string.Empty : "-" + index;
                string candidate = Path.Combine(folder,
                    name + " (TinyTable recovered " + stamp + suffix + ")" + extension);
                if (!File.Exists(candidate))
                    return candidate;
            }
            return Path.Combine(folder,
                Guid.NewGuid().ToString("N") + extension);
        }

        private static void MoveWithoutOverwrite(string source, string destination)
        {
            try
            {
                File.Move(source, destination);
                return;
            }
            catch (IOException)
            {
                if (!File.Exists(source) || File.Exists(destination))
                    throw;
            }

            // File.Move can fail across volumes. Copying first keeps the source
            // intact until a byte-for-byte destination is safely on disk.
            using (FileStream input = File.Open(
                source, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream output = new FileStream(
                destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                input.CopyTo(output);
                output.Flush(true);
            }
            if (!FilesEqual(source, destination))
            {
                File.Delete(destination);
                throw new IOException("Shortcut copy verification failed.");
            }
            File.Delete(source);
        }

        private static bool FilesEqual(string first, string second)
        {
            FileInfo left = new FileInfo(first);
            FileInfo right = new FileInfo(second);
            if (left.Length != right.Length)
                return false;
            using (FileStream a = File.OpenRead(first))
            using (FileStream b = File.OpenRead(second))
            {
                int x;
                while ((x = a.ReadByte()) >= 0)
                {
                    if (x != b.ReadByte())
                        return false;
                }
                return b.ReadByte() < 0;
            }
        }

        private ShelterState LoadState()
        {
            try
            {
                if (!File.Exists(_statePath))
                    return null;
                using (FileStream stream = File.Open(
                    _statePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(typeof(ShelterState));
                    return serializer.ReadObject(stream) as ShelterState;
                }
            }
            catch { return null; }
        }

        private void SaveState(ShelterState state)
        {
            Directory.CreateDirectory(_dataFolder);
            string temp = _statePath + ".tmp";
            using (FileStream stream = File.Create(temp))
            {
                DataContractJsonSerializer serializer =
                    new DataContractJsonSerializer(typeof(ShelterState));
                serializer.WriteObject(stream, state);
                stream.Flush(true);
            }
            if (File.Exists(_statePath))
                File.Replace(temp, _statePath, null);
            else
                File.Move(temp, _statePath);
        }

        public void Dispose()
        {
            _stopRequested = true;
            if (_thread != null && _thread.IsAlive)
                _thread.Join(1500);
            lock (_sync)
                RestorePendingCore();
        }

        [DataContract]
        private sealed class ShelterState
        {
            [DataMember] public DateTime CreatedUtc { get; set; }
            [DataMember] public List<ShelteredShortcut> Items { get; set; }
        }

        [DataContract]
        private sealed class ShelteredShortcut
        {
            [DataMember] public string OriginalPath { get; set; }
            [DataMember] public string ShelterPath { get; set; }
            [DataMember] public bool Moved { get; set; }
            [DataMember] public int X { get; set; }
            [DataMember] public int Y { get; set; }
            [DataMember] public bool PositionPending { get; set; }
        }
    }
}
