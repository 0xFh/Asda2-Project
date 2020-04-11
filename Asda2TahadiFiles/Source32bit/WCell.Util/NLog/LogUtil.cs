using NLog;
using NLog.Config;
using NLog.Win32.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace WCell.Util.NLog
{
  public static class LogUtil
  {
    private static Logger log = LogManager.GetCurrentClassLogger();
    private static int _streamNum;
    public static Action<Action<string>> SystemInfoLogger;
    private static bool init;

    public static event Action<string, Exception> ExceptionRaised;

    /// <summary>Will enable logging to the console</summary>
    public static void SetupConsoleLogging()
    {
      if(init)
        return;
      init = true;
      LoggingConfiguration loggingConfiguration = LogManager.Configuration ?? new LoggingConfiguration();
      ColoredConsoleTarget coloredConsoleTarget1 = new ColoredConsoleTarget();
      coloredConsoleTarget1.Layout = "${processtime} [${level}] ${message} ${exception:format=tostring}";
      ColoredConsoleTarget coloredConsoleTarget2 = coloredConsoleTarget1;
      loggingConfiguration.AddTarget("console", coloredConsoleTarget2);
      loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, coloredConsoleTarget2));
      LogManager.Configuration = loggingConfiguration;
      LogManager.EnableLogging();
    }

    /// <summary>
    /// Will enable logging to the console and (if not null) the specified file
    /// </summary>
    public static void SetupStreamLogging(TextWriter stream)
    {
      LoggingConfiguration loggingConfiguration = LogManager.Configuration ?? new LoggingConfiguration();
      StreamTarget streamTarget1 = new StreamTarget();
      streamTarget1.StreamName = "Stream" + ++_streamNum;
      streamTarget1.Stream = stream;
      streamTarget1.Layout = "${processtime} [${level}] ${message} ${exception:format=tostring}";
      StreamTarget streamTarget2 = streamTarget1;
      loggingConfiguration.AddTarget(streamTarget2.Name, streamTarget2);
      loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, streamTarget2));
      LogManager.Configuration = loggingConfiguration;
      LogManager.EnableLogging();
    }

    public static void ErrorException(Exception e)
    {
      ErrorException(e, false);
    }

    public static void ErrorException(Exception e, bool addSystemInfo)
    {
      ErrorException(e, addSystemInfo, "");
    }

    public static void ErrorException(string msg, params object[] format)
    {
      ErrorException(false, msg, format);
    }

    public static void ErrorException(bool addSystemInfo, string msg, params object[] format)
    {
      LogException(log.Error, null, addSystemInfo, msg, format);
    }

    public static void ErrorException(Exception e, string msg, params object[] format)
    {
      ErrorException(e, true, msg, format);
    }

    public static void ErrorException(Exception e, bool addSystemInfo, string msg, params object[] format)
    {
      LogException(log.Error, e, addSystemInfo, msg, format);
    }

    public static void WarnException(Exception e)
    {
      WarnException(e, false);
    }

    public static void WarnException(Exception e, bool addSystemInfo)
    {
      WarnException(e, addSystemInfo, "");
    }

    public static void WarnException(string msg, params object[] format)
    {
      WarnException(false, msg, format);
    }

    public static void WarnException(bool addSystemInfo, string msg, params object[] format)
    {
      LogException(log.Warn, null, addSystemInfo, msg, format);
    }

    public static void WarnException(Exception e, string msg, params object[] format)
    {
      WarnException(e, true, msg, format);
    }

    public static void WarnException(Exception e, bool addSystemInfo, string msg, params object[] format)
    {
      LogException(log.Warn, e, addSystemInfo, msg, format);
    }

    public static void FatalException(Exception e, string msg, params object[] format)
    {
      FatalException(e, true, msg, format);
    }

    public static void FatalException(Exception e, bool addSystemInfo)
    {
      FatalException(e, addSystemInfo, "");
    }

    public static void FatalException(Exception e, bool addSystemInfo, string msg, params object[] format)
    {
      LogException(log.Fatal, e, addSystemInfo, msg, format);
    }

    public static void LogException(Action<string> logger, Exception e, bool addSystemInfo, string msg,
      params object[] format)
    {
      if(!string.IsNullOrEmpty(msg))
      {
        msg = string.Format(msg, format);
        logger(msg);
      }

      if(e != null)
      {
        LogStacktrace(logger);
        logger("");
        logger(e.ToString());
      }

      if(addSystemInfo)
      {
        logger("");
        if(SystemInfoLogger != null)
          SystemInfoLogger(logger);
        else
          LogSystemInfo(logger);
      }

      if(e != null)
      {
        logger("");
        logger(e.GetAllMessages().ToString("\n\t"));
      }

      Action<string, Exception> exceptionRaised = ExceptionRaised;
      if(exceptionRaised == null)
        return;
      exceptionRaised(msg, e);
    }

    public static void LogStacktrace(Action<string> logger)
    {
      logger(new StackTrace(Thread.CurrentThread, true).GetFrames()
        .ToString("\n\t", frame => (object) frame.ToString().Trim()));
    }

    private static void LogSystemInfo(Action<string> logger)
    {
      string str = "WCell component" + " - Debug";
      logger(str);
      logger(string.Format("OS: {0} - CLR: {1}", Environment.OSVersion, Environment.Version));
    }
  }
}