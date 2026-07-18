using System;
using System.Runtime.InteropServices;

namespace TinyTable
{
    public static class DpiHelper
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        public static void Enable()
        {
            try
            {
                if (SetProcessDpiAwarenessContext((IntPtr)(-4)))
                    return;
            }
            catch (EntryPointNotFoundException) { }
            catch { }
            try { SetProcessDPIAware(); } catch { }
        }
    }
}
