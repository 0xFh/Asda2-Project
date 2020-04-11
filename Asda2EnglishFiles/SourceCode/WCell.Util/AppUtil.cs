using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WCell.Util
{
    public static class AppUtil
    {
        private static readonly List<AppUtil.ConsoleCtrlHandler> ctrlHandlers = new List<AppUtil.ConsoleCtrlHandler>();
        private static readonly List<EventHandler> processHooks = new List<EventHandler>();

        /// <summary>
        /// Gets a value indicating if the operating system is a Windows 2000 or a newer one.
        /// </summary>
        public static bool IsWindows2000OrNewer
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 5;
            }
        }

        /// <summary>
        /// Gets a value indicating if the operating system is a Windows XP or a newer one.
        /// </summary>
        public static bool IsWindowsXpOrNewer
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT &&
                       (Environment.OSVersion.Version.Major >= 6 || Environment.OSVersion.Version.Major == 5 &&
                        Environment.OSVersion.Version.Minor >= 1);
            }
        }

        /// <summary>
        /// Gets a value indicating if the operating system is a Windows Vista or a newer one.
        /// </summary>
        public static bool IsWindowsVistaOrNewer
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6;
            }
        }

        /// <summary>
        /// see: http://geekswithblogs.net/mrnat/archive/2004/09/23/11594.aspx
        /// </summary>
        /// <param name="consoleCtrlHandler"></param>
        /// <param name="Add"></param>
        /// <returns></returns>
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(AppUtil.ConsoleCtrlHandler consoleCtrlHandler, bool Add);

        /// <summary>Removes all previously added hooks</summary>
        public static void UnhookAll()
        {
            foreach (AppUtil.ConsoleCtrlHandler ctrlHandler in AppUtil.ctrlHandlers)
                AppUtil.SetConsoleCtrlHandler(ctrlHandler, false);
            foreach (EventHandler processHook in AppUtil.processHooks)
                AppDomain.CurrentDomain.ProcessExit -= processHook;
            AppUtil.ctrlHandlers.Clear();
            AppUtil.processHooks.Clear();
        }

        /// <summary>
        /// Adds an action that will be executed when the Application exists.
        /// </summary>
        /// <param name="action"></param>
        public static void AddApplicationExitHandler(Action action)
        {
            EventHandler eventHandler = (EventHandler) ((sender, evt) => action());
            AppUtil.processHooks.Add(eventHandler);
            AppDomain.CurrentDomain.ProcessExit += eventHandler;
            AppUtil.ConsoleCtrlHandler consoleCtrlHandler = (AppUtil.ConsoleCtrlHandler) (type =>
            {
                action();
                return false;
            });
            AppUtil.ctrlHandlers.Add(consoleCtrlHandler);
            AppUtil.SetConsoleCtrlHandler(consoleCtrlHandler, true);
        }

        /// <summary>
        /// Needed for <see cref="M:WCell.Util.AppUtil.SetConsoleCtrlHandler(WCell.Util.AppUtil.ConsoleCtrlHandler,System.Boolean)" />
        /// </summary>
        /// <param name="CtrlType"></param>
        /// <returns></returns>
        public delegate bool ConsoleCtrlHandler(AppUtil.CtrlTypes CtrlType);

        /// <summary>
        /// Needed for <see cref="M:WCell.Util.AppUtil.SetConsoleCtrlHandler(WCell.Util.AppUtil.ConsoleCtrlHandler,System.Boolean)" />
        /// </summary>
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6,
        }
    }
}