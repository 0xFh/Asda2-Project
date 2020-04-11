using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WCell.Util
{
  public static class AppUtil
  {
    private static readonly List<ConsoleCtrlHandler> ctrlHandlers = new List<ConsoleCtrlHandler>();
    private static readonly List<EventHandler> processHooks = new List<EventHandler>();

    /// <summary>
    /// Gets a value indicating if the operating system is a Windows 2000 or a newer one.
    /// </summary>
    public static bool IsWindows2000OrNewer
    {
      get { return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 5; }
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
      get { return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6; }
    }

    /// <summary>
    /// see: http://geekswithblogs.net/mrnat/archive/2004/09/23/11594.aspx
    /// </summary>
    /// <param name="consoleCtrlHandler"></param>
    /// <param name="Add"></param>
    /// <returns></returns>
    [DllImport("Kernel32")]
    public static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandler consoleCtrlHandler, bool Add);

    /// <summary>Removes all previously added hooks</summary>
    public static void UnhookAll()
    {
      foreach(ConsoleCtrlHandler ctrlHandler in ctrlHandlers)
        SetConsoleCtrlHandler(ctrlHandler, false);
      foreach(EventHandler processHook in processHooks)
        AppDomain.CurrentDomain.ProcessExit -= processHook;
      ctrlHandlers.Clear();
      processHooks.Clear();
    }

    /// <summary>
    /// Adds an action that will be executed when the Application exists.
    /// </summary>
    /// <param name="action"></param>
    public static void AddApplicationExitHandler(Action action)
    {
      EventHandler eventHandler = (sender, evt) => action();
      processHooks.Add(eventHandler);
      AppDomain.CurrentDomain.ProcessExit += eventHandler;
      ConsoleCtrlHandler consoleCtrlHandler = type =>
      {
        action();
        return false;
      };
      ctrlHandlers.Add(consoleCtrlHandler);
      SetConsoleCtrlHandler(consoleCtrlHandler, true);
    }

    /// <summary>
    /// Needed for <see cref="M:WCell.Util.AppUtil.SetConsoleCtrlHandler(WCell.Util.AppUtil.ConsoleCtrlHandler,System.Boolean)" />
    /// </summary>
    /// <param name="CtrlType"></param>
    /// <returns></returns>
    public delegate bool ConsoleCtrlHandler(CtrlTypes CtrlType);

    /// <summary>
    /// Needed for <see cref="M:WCell.Util.AppUtil.SetConsoleCtrlHandler(WCell.Util.AppUtil.ConsoleCtrlHandler,System.Boolean)" />
    /// </summary>
    public enum CtrlTypes
    {
      CTRL_C_EVENT = 0,
      CTRL_BREAK_EVENT = 1,
      CTRL_CLOSE_EVENT = 2,
      CTRL_LOGOFF_EVENT = 5,
      CTRL_SHUTDOWN_EVENT = 6
    }
  }
}