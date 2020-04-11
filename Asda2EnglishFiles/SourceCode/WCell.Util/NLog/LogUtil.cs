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
            if (LogUtil.init)
                return;
            LogUtil.init = true;
            LoggingConfiguration loggingConfiguration = LogManager.Configuration ?? new LoggingConfiguration();
            ColoredConsoleTarget coloredConsoleTarget1 = new ColoredConsoleTarget();
            coloredConsoleTarget1.Layout = "${processtime} [${level}] ${message} ${exception:format=tostring}";
            ColoredConsoleTarget coloredConsoleTarget2 = coloredConsoleTarget1;
            loggingConfiguration.AddTarget("console", (Target) coloredConsoleTarget2);
            loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, (Target) coloredConsoleTarget2));
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
            streamTarget1.StreamName = "Stream" + (object) ++LogUtil._streamNum;
            streamTarget1.Stream = stream;
            streamTarget1.Layout = "${processtime} [${level}] ${message} ${exception:format=tostring}";
            StreamTarget streamTarget2 = streamTarget1;
            loggingConfiguration.AddTarget(streamTarget2.Name, (Target) streamTarget2);
            loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, (Target) streamTarget2));
            LogManager.Configuration = loggingConfiguration;
            LogManager.EnableLogging();
        }

        public static void ErrorException(Exception e)
        {
            LogUtil.ErrorException(e, false);
        }

        public static void ErrorException(Exception e, bool addSystemInfo)
        {
            LogUtil.ErrorException(e, addSystemInfo, "", new object[0]);
        }

        public static void ErrorException(string msg, params object[] format)
        {
            LogUtil.ErrorException(false, msg, format);
        }

        public static void ErrorException(bool addSystemInfo, string msg, params object[] format)
        {
            LogUtil.LogException(new Action<string>(LogUtil.log.Error), (Exception) null, addSystemInfo, msg, format);
        }

        public static void ErrorException(Exception e, string msg, params object[] format)
        {
            LogUtil.ErrorException(e, true, msg, format);
        }

        public static void ErrorException(Exception e, bool addSystemInfo, string msg, params object[] format)
        {
            LogUtil.LogException(new Action<string>(LogUtil.log.Error), e, addSystemInfo, msg, format);
        }

        public static void WarnException(Exception e)
        {
            LogUtil.WarnException(e, false);
        }

        public static void WarnException(Exception e, bool addSystemInfo)
        {
            LogUtil.WarnException(e, addSystemInfo, "", new object[0]);
        }

        public static void WarnException(string msg, params object[] format)
        {
            LogUtil.WarnException(false, msg, format);
        }

        public static void WarnException(bool addSystemInfo, string msg, params object[] format)
        {
            LogUtil.LogException(new Action<string>(LogUtil.log.Warn), (Exception) null, addSystemInfo, msg, format);
        }

        public static void WarnException(Exception e, string msg, params object[] format)
        {
            LogUtil.WarnException(e, true, msg, format);
        }

        public static void WarnException(Exception e, bool addSystemInfo, string msg, params object[] format)
        {
            LogUtil.LogException(new Action<string>(LogUtil.log.Warn), e, addSystemInfo, msg, format);
        }

        public static void FatalException(Exception e, string msg, params object[] format)
        {
            LogUtil.FatalException(e, true, msg, format);
        }

        public static void FatalException(Exception e, bool addSystemInfo)
        {
            LogUtil.FatalException(e, addSystemInfo, "", new object[0]);
        }

        public static void FatalException(Exception e, bool addSystemInfo, string msg, params object[] format)
        {
            LogUtil.LogException(new Action<string>(LogUtil.log.Fatal), e, addSystemInfo, msg, format);
        }

        public static void LogException(Action<string> logger, Exception e, bool addSystemInfo, string msg,
            params object[] format)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                msg = string.Format(msg, format);
                logger(msg);
            }

            if (e != null)
            {
                LogUtil.LogStacktrace(logger);
                logger("");
                logger(e.ToString());
            }

            if (addSystemInfo)
            {
                logger("");
                if (LogUtil.SystemInfoLogger != null)
                    LogUtil.SystemInfoLogger(logger);
                else
                    LogUtil.LogSystemInfo(logger);
            }

            if (e != null)
            {
                logger("");
                logger(e.GetAllMessages().ToString<string>("\n\t"));
            }

            Action<string, Exception> exceptionRaised = LogUtil.ExceptionRaised;
            if (exceptionRaised == null)
                return;
            exceptionRaised(msg, e);
        }

        public static void LogStacktrace(Action<string> logger)
        {
            logger(((IEnumerable<StackFrame>) new StackTrace(Thread.CurrentThread, true).GetFrames())
                .ToString<StackFrame>("\n\t", (Func<StackFrame, object>) (frame => (object) frame.ToString().Trim())));
        }

        private static void LogSystemInfo(Action<string> logger)
        {
            string str = "WCell component" + " - Debug";
            logger(str);
            logger(string.Format("OS: {0} - CLR: {1}", (object) Environment.OSVersion, (object) Environment.Version));
        }
    }
}