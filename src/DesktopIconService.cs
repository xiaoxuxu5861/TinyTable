using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TinyTable
{
    public sealed class DesktopIconService
    {
        public IntPtr FindDesktopListView()
        {
            IntPtr progman = NativeMethods.FindWindow("Progman", null);
            IntPtr defView = NativeMethods.FindWindowEx(
                progman, IntPtr.Zero, "SHELLDLL_DefView", null);

            if (defView == IntPtr.Zero)
            {
                NativeMethods.EnumWindows(delegate(IntPtr window, IntPtr parameter)
                {
                    IntPtr child = NativeMethods.FindWindowEx(
                        window, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (child != IntPtr.Zero)
                    {
                        defView = child;
                        return false;
                    }
                    return true;
                }, IntPtr.Zero);
            }

            if (defView == IntPtr.Zero)
                return IntPtr.Zero;
            return NativeMethods.FindWindowEx(
                defView, IntPtr.Zero, "SysListView32", "FolderView");
        }

        public bool IsAutoArrangeEnabled()
        {
            IntPtr listView = RequireDesktopListView();
            int style = NativeMethods.GetWindowLong(listView, NativeMethods.GWL_STYLE);
            return (style & NativeMethods.LVS_AUTOARRANGE) != 0;
        }

        public bool DisableAutoArrange()
        {
            IntPtr listView = RequireDesktopListView();
            int style = NativeMethods.GetWindowLong(listView, NativeMethods.GWL_STYLE);
            if ((style & NativeMethods.LVS_AUTOARRANGE) == 0)
                return true;
            NativeMethods.SetWindowLong(
                listView,
                NativeMethods.GWL_STYLE,
                style & ~NativeMethods.LVS_AUTOARRANGE);
            return !IsAutoArrangeEnabled();
        }

        public List<DesktopIcon> ReadIcons()
        {
            IntPtr listView = RequireDesktopListView();
            int count = NativeMethods.SendMessage(
                listView, NativeMethods.LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();
            Dictionary<string, List<PathCandidate>> paths = BuildDisplayNamePathMap();
            List<DesktopIcon> icons = new List<DesktopIcon>();
            HashSet<string> usedPaths =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> keyCounts =
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            uint processId;
            NativeMethods.GetWindowThreadProcessId(listView, out processId);
            IntPtr process = NativeMethods.OpenProcess(
                NativeMethods.PROCESS_VM_OPERATION |
                NativeMethods.PROCESS_VM_READ |
                NativeMethods.PROCESS_VM_WRITE |
                NativeMethods.PROCESS_QUERY_INFORMATION,
                false,
                processId);
            if (process == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(),
                    "无法读取 Windows 桌面图标。");

            int itemSize = Marshal.SizeOf(typeof(NativeMethods.LVITEM));
            const int textChars = 520;
            int allocationSize = itemSize + textChars * 2 + 32;
            IntPtr remote = NativeMethods.VirtualAllocEx(
                process,
                IntPtr.Zero,
                (UIntPtr)allocationSize,
                NativeMethods.MEM_COMMIT | NativeMethods.MEM_RESERVE,
                NativeMethods.PAGE_READWRITE);

            if (remote == IntPtr.Zero)
            {
                NativeMethods.CloseHandle(process);
                throw new Win32Exception(Marshal.GetLastWin32Error(),
                    "无法分配桌面读取缓冲区。");
            }

            try
            {
                for (int index = 0; index < count; index++)
                {
                    ItemDetails details = ReadItemDetails(
                        listView, process, remote, itemSize, textChars, index);
                    NativeMethods.POINT point = ReadItemPosition(
                        listView, process, remote, index);
                    bool selected = (NativeMethods.SendMessage(
                        listView,
                        NativeMethods.LVM_GETITEMSTATE,
                        (IntPtr)index,
                        (IntPtr)NativeMethods.LVIS_SELECTED).ToInt32()
                        & NativeMethods.LVIS_SELECTED) != 0;

                    string path = ResolvePath(
                        details.DisplayName,
                        details.ImageIndex,
                        paths,
                        usedPaths);
                    string baseKey = BuildKey(details.DisplayName, path);
                    int occurrence;
                    keyCounts.TryGetValue(baseKey, out occurrence);
                    occurrence++;
                    keyCounts[baseKey] = occurrence;
                    string key = occurrence == 1
                        ? baseKey
                        : baseKey + "#desktop-" + occurrence;
                    DesktopIcon icon = new DesktopIcon
                    {
                        Index = index,
                        DisplayName = details.DisplayName,
                        Path = path,
                        Key = key,
                        X = point.X,
                        Y = point.Y,
                        IsSelected = selected
                    };
                    icon.Category = DesktopItemClassifier.Classify(
                        icon.Path, icon.IsVirtual);
                    icon.LastWriteTimeUtc = DesktopItemClassifier.GetWriteTimeUtc(icon.Path);
                    icons.Add(icon);
                }
            }
            finally
            {
                NativeMethods.VirtualFreeEx(
                    process, remote, UIntPtr.Zero, NativeMethods.MEM_RELEASE);
                NativeMethods.CloseHandle(process);
            }
            return icons;
        }

        public IconPosition GetSpacing()
        {
            IntPtr listView = RequireDesktopListView();
            IntPtr result = NativeMethods.SendMessage(
                listView, NativeMethods.LVM_GETITEMSPACING, IntPtr.Zero, IntPtr.Zero);
            int x = NativeMethods.LowWord(result);
            int y = NativeMethods.HighWord(result);
            return new IconPosition(x > 20 ? x : 76, y > 20 ? y : 110);
        }

        public IconPosition GetDesktopSize()
        {
            IntPtr listView = RequireDesktopListView();
            NativeMethods.RECT rect;
            if (!NativeMethods.GetClientRect(listView, out rect))
                return new IconPosition(
                    System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                    System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            return new IconPosition(rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        public int ApplyPositions(Dictionary<string, IconPosition> positions)
        {
            IntPtr listView = RequireDesktopListView();
            const int maxPasses = 6;
            for (int pass = 0; pass < maxPasses; pass++)
            {
                List<DesktopIcon> current = ReadIcons();
                List<DesktopIcon> pending = current.Where(icon =>
                {
                    IconPosition desired;
                    return positions.TryGetValue(icon.Key, out desired)
                        && !IsAtPosition(icon, desired);
                }).ToList();
                if (pending.Count == 0)
                    break;

                foreach (DesktopIcon icon in pending)
                {
                    IconPosition position = positions[icon.Key];
                    NativeMethods.SendMessage(
                        listView,
                        NativeMethods.LVM_SETITEMPOSITION,
                        (IntPtr)icon.Index,
                        NativeMethods.MakeLParam(position.X, position.Y));
                }
                NativeMethods.InvalidateRect(listView, IntPtr.Zero, true);
                Thread.Sleep(25);
            }

            NativeMethods.InvalidateRect(listView, IntPtr.Zero, true);
            return ReadIcons().Count(icon =>
            {
                IconPosition desired;
                return positions.TryGetValue(icon.Key, out desired)
                    && IsAtPosition(icon, desired);
            });
        }

        private static bool IsAtPosition(
            DesktopIcon icon, IconPosition position)
        {
            return Math.Abs(icon.X - position.X) <= 2
                && Math.Abs(icon.Y - position.Y) <= 2;
        }

        public static string BuildKey(string displayName, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                try { return "path:" + Path.GetFullPath(path).ToLowerInvariant(); }
                catch { return "path:" + path.ToLowerInvariant(); }
            }
            return "virtual:" + (displayName ?? string.Empty).Trim().ToLowerInvariant();
        }

        private IntPtr RequireDesktopListView()
        {
            IntPtr listView = FindDesktopListView();
            if (listView == IntPtr.Zero)
                throw new InvalidOperationException(
                    "没有找到 Windows 桌面图标窗口。请确认 Explorer 正在运行。");
            return listView;
        }

        private static ItemDetails ReadItemDetails(
            IntPtr listView,
            IntPtr process,
            IntPtr remote,
            int itemSize,
            int textChars,
            int index)
        {
            IntPtr remoteText = IntPtr.Add(remote, itemSize);
            NativeMethods.LVITEM item = new NativeMethods.LVITEM();
            item.mask = NativeMethods.LVIF_TEXT | NativeMethods.LVIF_IMAGE;
            item.iItem = index;
            item.pszText = remoteText;
            item.cchTextMax = textChars;
            byte[] itemBytes = StructToBytes(item);
            IntPtr written;
            NativeMethods.WriteProcessMemory(
                process, remote, itemBytes, itemBytes.Length, out written);
            NativeMethods.SendMessage(
                listView, NativeMethods.LVM_GETITEMW, IntPtr.Zero, remote);

            byte[] textBytes = new byte[textChars * 2];
            byte[] returnedItemBytes = new byte[itemSize];
            IntPtr read;
            NativeMethods.ReadProcessMemory(
                process, remoteText, textBytes, textBytes.Length, out read);
            NativeMethods.ReadProcessMemory(
                process, remote, returnedItemBytes, returnedItemBytes.Length, out read);
            string value = Encoding.Unicode.GetString(textBytes);
            int nullIndex = value.IndexOf('\0');
            NativeMethods.LVITEM returnedItem =
                BytesToStruct<NativeMethods.LVITEM>(returnedItemBytes);
            return new ItemDetails
            {
                DisplayName = nullIndex >= 0 ? value.Substring(0, nullIndex) : value,
                ImageIndex = returnedItem.iImage
            };
        }

        private static NativeMethods.POINT ReadItemPosition(
            IntPtr listView,
            IntPtr process,
            IntPtr remote,
            int index)
        {
            NativeMethods.SendMessage(
                listView, NativeMethods.LVM_GETITEMPOSITION, (IntPtr)index, remote);
            byte[] pointBytes = new byte[Marshal.SizeOf(typeof(NativeMethods.POINT))];
            IntPtr read;
            NativeMethods.ReadProcessMemory(
                process, remote, pointBytes, pointBytes.Length, out read);
            GCHandle handle = GCHandle.Alloc(pointBytes, GCHandleType.Pinned);
            try
            {
                return (NativeMethods.POINT)Marshal.PtrToStructure(
                    handle.AddrOfPinnedObject(), typeof(NativeMethods.POINT));
            }
            finally
            {
                handle.Free();
            }
        }

        private static byte[] StructToBytes(object value)
        {
            int size = Marshal.SizeOf(value);
            byte[] bytes = new byte[size];
            IntPtr pointer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(value, pointer, false);
                Marshal.Copy(pointer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        private static T BytesToStruct<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(
                    handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        private static Dictionary<string, List<PathCandidate>> BuildDisplayNamePathMap()
        {
            Dictionary<string, List<PathCandidate>> result =
                new Dictionary<string, List<PathCandidate>>(
                    StringComparer.CurrentCultureIgnoreCase);
            AddDesktopFolder(result,
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            AddDesktopFolder(result,
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory));
            return result;
        }

        private static void AddDesktopFolder(
            Dictionary<string, List<PathCandidate>> result, string folder)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                return;
            foreach (string path in Directory.EnumerateFileSystemEntries(folder))
            {
                if (string.Equals(
                    Path.GetFileName(path), "desktop.ini",
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                PathCandidate candidate = GetShellCandidate(path);
                AddCandidate(result, candidate.DisplayName, candidate);
                string fileName = Path.GetFileName(path);
                AddCandidate(result, fileName, candidate);
                string withoutExtension = Path.GetFileNameWithoutExtension(path);
                AddCandidate(result, withoutExtension, candidate);
            }
        }

        private static PathCandidate GetShellCandidate(string path)
        {
            NativeMethods.SHFILEINFO info;
            IntPtr result = NativeMethods.SHGetFileInfo(
                path,
                0,
                out info,
                (uint)Marshal.SizeOf(typeof(NativeMethods.SHFILEINFO)),
                NativeMethods.SHGFI_DISPLAYNAME
                    | NativeMethods.SHGFI_SYSICONINDEX
                    | NativeMethods.SHGFI_SMALLICON);
            return new PathCandidate
            {
                Path = path,
                DisplayName = result == IntPtr.Zero
                    ? Path.GetFileName(path)
                    : info.szDisplayName,
                ImageIndex = result == IntPtr.Zero ? -1 : info.iIcon
            };
        }

        private static void AddCandidate(
            Dictionary<string, List<PathCandidate>> result,
            string name,
            PathCandidate candidate)
        {
            if (string.IsNullOrEmpty(name))
                return;
            List<PathCandidate> list;
            if (!result.TryGetValue(name, out list))
            {
                list = new List<PathCandidate>();
                result.Add(name, list);
            }
            if (!list.Any(x => string.Equals(
                x.Path, candidate.Path, StringComparison.OrdinalIgnoreCase)))
                list.Add(candidate);
        }

        private static string ResolvePath(
            string displayName,
            int imageIndex,
            Dictionary<string, List<PathCandidate>> candidates,
            HashSet<string> usedPaths)
        {
            List<PathCandidate> list;
            if (!candidates.TryGetValue(displayName, out list))
                return null;
            PathCandidate selected = list.FirstOrDefault(x =>
                !usedPaths.Contains(x.Path) && x.ImageIndex == imageIndex);
            if (selected == null)
                selected = list.FirstOrDefault(x => !usedPaths.Contains(x.Path));
            if (selected == null)
                return null;
            usedPaths.Add(selected.Path);
            return selected.Path;
        }

        private sealed class ItemDetails
        {
            public string DisplayName { get; set; }
            public int ImageIndex { get; set; }
        }

        private sealed class PathCandidate
        {
            public string Path { get; set; }
            public string DisplayName { get; set; }
            public int ImageIndex { get; set; }
        }
    }
}
