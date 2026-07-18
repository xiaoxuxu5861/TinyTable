using System;
using System.Threading;
using System.Windows.Forms;

namespace TinyTable
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                SnapshotDatabase database = new SnapshotStore().Load();
                Localization.SetLanguage(database.AutomationSettings.LanguageCode);
            }
            catch { }
            if (TryHandleShellCommand(args))
                return;

            bool created;
            using (Mutex mutex = new Mutex(true, @"Local\TinyTable.SingleInstance", out created))
            {
                if (!created)
                {
                    IntPtr mainWindow = NativeMethods.FindWindow(
                        null, "TinyTable");
                    if (mainWindow != IntPtr.Zero)
                    {
                        NativeMethods.SendMessage(
                            mainWindow,
                            NativeMethods.WM_TINYTABLE_SHOW,
                            IntPtr.Zero,
                            IntPtr.Zero);
                    }
                    return;
                }
                DpiHelper.Enable();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                try { ShellContextMenuService.Install(); } catch { }
                bool startedByWindows = args != null && Array.Exists(
                    args,
                    x => string.Equals(
                        x, "--startup", StringComparison.OrdinalIgnoreCase));
                Application.Run(new MainForm(startedByWindows));
            }
        }

        private static bool TryHandleShellCommand(string[] args)
        {
            if (args == null || args.Length == 0)
                return false;
            if (string.Equals(
                args[0], "--install-context-menu", StringComparison.OrdinalIgnoreCase))
            {
                try { ShellContextMenuService.Install(); }
                catch (Exception ex)
                {
                    MessageBox.Show("安装右键菜单失败：\n" + ex.Message, "TinyTable",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return true;
            }
            if (string.Equals(
                args[0], "--remove-context-menu", StringComparison.OrdinalIgnoreCase))
            {
                try { ShellContextMenuService.Remove(); }
                catch (Exception ex)
                {
                    MessageBox.Show("移除右键菜单失败：\n" + ex.Message, "TinyTable",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return true;
            }
            if (args.Length < 2)
                return false;
            bool add;
            if (string.Equals(args[0], "--add", StringComparison.OrdinalIgnoreCase))
                add = true;
            else if (string.Equals(args[0], "--remove", StringComparison.OrdinalIgnoreCase))
                add = false;
            else
                return false;

            try
            {
                string[] paths = new string[args.Length - 1];
                Array.Copy(args, 1, paths, 0, paths.Length);
                SelectionListService service = new SelectionListService();
                service.UpdatePaths(paths, add);
                IntPtr mainWindow = NativeMethods.FindWindow(
                    null, "TinyTable");
                if (mainWindow != IntPtr.Zero)
                {
                    NativeMethods.SendMessage(
                        mainWindow,
                        NativeMethods.WM_TINYTABLE_LIST_CHANGED,
                        IntPtr.Zero,
                        IntPtr.Zero);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "更新 TinyTable 备份列表失败：\n" + ex.Message,
                    "TinyTable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            return true;
        }
    }
}
