using System.Runtime.InteropServices;

namespace Cell.Core
{
    internal class NativeMethods
    {
        [DllImport("kernel32")]
        private static extern int SwitchToThread();

        public static void OsSwitchToThread()
        {
            NativeMethods.SwitchToThread();
        }
    }
}