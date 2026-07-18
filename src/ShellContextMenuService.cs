using Microsoft.Win32;
using System;

namespace TinyTable
{
    public static class ShellContextMenuService
    {
        private const string FileRoot = @"Software\Classes\*\shell";
        private const string DirectoryRoot = @"Software\Classes\Directory\shell";

        public static void Install()
        {
            SnapshotDatabase database = new SnapshotStore().Load();
            Localization.SetLanguage(database.AutomationSettings.LanguageCode);
            string executable = typeof(ShellContextMenuService).Assembly.Location;
            InstallVerb(FileRoot, "TinyTable.Add",
                Localization.T("加入 TinyTable 备份列表"), "--add", executable);
            InstallVerb(FileRoot, "TinyTable.Remove",
                Localization.T("从 TinyTable 备份列表移除"), "--remove", executable);
            InstallVerb(DirectoryRoot, "TinyTable.Add",
                Localization.T("加入 TinyTable 备份列表"), "--add", executable);
            InstallVerb(DirectoryRoot, "TinyTable.Remove",
                Localization.T("从 TinyTable 备份列表移除"), "--remove", executable);
            NativeMethods.SHChangeNotify(
                NativeMethods.SHCNE_ASSOCCHANGED,
                NativeMethods.SHCNF_IDLIST,
                IntPtr.Zero,
                IntPtr.Zero);
        }

        public static void Remove()
        {
            DeleteVerb(FileRoot, "TinyTable.Add");
            DeleteVerb(FileRoot, "TinyTable.Remove");
            DeleteVerb(DirectoryRoot, "TinyTable.Add");
            DeleteVerb(DirectoryRoot, "TinyTable.Remove");
            NativeMethods.SHChangeNotify(
                NativeMethods.SHCNE_ASSOCCHANGED,
                NativeMethods.SHCNF_IDLIST,
                IntPtr.Zero,
                IntPtr.Zero);
        }

        private static void InstallVerb(
            string root,
            string verb,
            string title,
            string action,
            string executable)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(root + "\\" + verb))
            {
                key.SetValue(null, title);
                key.SetValue("Icon", executable);
                key.SetValue("MultiSelectModel", "Player");
                using (RegistryKey command = key.CreateSubKey("command"))
                {
                    command.SetValue(
                        null,
                        "\"" + executable + "\" " + action + " %*");
                }
            }
        }

        private static void DeleteVerb(string root, string verb)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(root, true))
            {
                if (key != null)
                    key.DeleteSubKeyTree(verb, false);
            }
        }
    }
}
