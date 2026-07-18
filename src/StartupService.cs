using Microsoft.Win32;

namespace TinyTable
{
    public static class StartupService
    {
        private const string RunKey =
            @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static void Apply(bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                if (key == null)
                    return;
                if (enabled)
                {
                    key.SetValue(
                        "TinyTable",
                        "\"" + typeof(StartupService).Assembly.Location + "\" --startup");
                }
                else
                {
                    key.DeleteValue("TinyTable", false);
                }
            }
        }
    }
}
