using Cell.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using WCell.Core.Addons;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.Core.Localization;
using WCell.Core.Timers;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Variables;

namespace WCell.Core
{
    public abstract class ServerApp<T> : ServerBase where T : ServerBase, new()
    {
        [NotVariable] public bool ConsoleActive = true;
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();
        protected static readonly string[] EmptyStringArr = new string[0];

        private static readonly SelfRunningTaskQueue ioQueue =
            new SelfRunningTaskQueue(100, "Server I\\O - Queue", false);

        [Variable] public static readonly DateTime StartTime = DateTime.Now;
        protected static string s_entryLocation;
        protected TimerEntry m_shutdownTimer;
        protected static InitMgr s_initMgr;

        [Variable]
        public static TimeSpan RunTime
        {
            get { return DateTime.Now - ServerApp<T>.StartTime; }
        }

        protected ServerApp()
        {
            ServerApp<T>.Log.Debug(WCell_Core.ServerStarting);
            AppUtil.AddApplicationExitHandler(new Action(this._OnShutdown));
            AppDomain.CurrentDomain.UnhandledException += (UnhandledExceptionEventHandler) ((sender, args) =>
                LogUtil.FatalException(args.ExceptionObject as Exception, WCell_Core.FatalUnhandledException,
                    new object[0]));
            LogUtil.SystemInfoLogger = new Action<Action<string>>(this.LogSystemInfo);
        }

        /// <summary>
        /// The singleton instance of the InitMgr that runs the default Startup routine.
        /// </summary>
        public static InitMgr InitMgr
        {
            get
            {
                if (ServerApp<T>.s_initMgr == null)
                {
                    ServerApp<T>.s_initMgr = new InitMgr(InitMgr.FeedbackRepeatFailHandler);
                    ServerApp<T>.s_initMgr.AddStepsOfAsm(ServerApp<T>.Instance.GetType().Assembly);
                }

                return ServerApp<T>.s_initMgr;
            }
        }

        /// <summary>
        /// Modify this to the Location of the file whose App-config you want to load.
        /// This is needed specifically for tests, since they don't have an EntryAssembly
        /// </summary>
        [NotVariable]
        public static string EntryLocation
        {
            get
            {
                if (ServerApp<T>.s_entryLocation == null)
                {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly != (Assembly) null)
                        ServerApp<T>.s_entryLocation = entryAssembly.Location;
                }

                return ServerApp<T>.s_entryLocation;
            }
            set { ServerApp<T>.s_entryLocation = value; }
        }

        /// <summary>
        /// Returns the single instance of the implemented server class.
        /// </summary>
        public static T Instance
        {
            get { return SingletonHolder<T>.Instance; }
        }

        /// <summary>
        /// Gets the assembly version information for the entry assembly of the process.
        /// </summary>
        public static string AssemblyVersion
        {
            get
            {
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (!(entryAssembly != (Assembly) null))
                    return string.Format("[Cannot get AssemblyVersion]");
                Version version = entryAssembly.GetName().Version;
                return string.Format("{0}.{1}.{2}.{3})", (object) version.Major, (object) version.Minor,
                    (object) version.Build, (object) version.Revision);
            }
        }

        /// <summary>
        /// Used for general I/O tasks.
        /// These tasks are usually blocking, so do not use this for precise timers
        /// </summary>
        public static SelfRunningTaskQueue IOQueue
        {
            get { return ServerApp<T>.ioQueue; }
        }

        public override bool IsRunning
        {
            get { return this._running; }
            set
            {
                if (this._running == value)
                    return;
                this._running = value;
                ServerApp<T>.ioQueue.IsRunning = value;
            }
        }

        /// <summary>
        /// Whether the Server is in the process of shutting down (cannot be cancelled anymore)
        /// </summary>
        public static bool IsShuttingDown { get; private set; }

        /// <summary>
        /// Whether a timer has been started to shutdown the server.
        /// </summary>
        public static bool IsPreparingShutdown { get; private set; }

        public abstract string Host { get; }

        public abstract int Port { get; }

        public void UpdateTitle()
        {
            this.SetTitle(this.ToString());
        }

        public void SetTitle(string title)
        {
            if (!this.ConsoleActive)
                return;
            Console.Title = title;
        }

        public void SetTitle(string title, params object[] args)
        {
            if (!this.ConsoleActive)
                return;
            Console.Title = string.Format(title, args);
        }

        private void LogSystemInfo(Action<string> logger)
        {
            string str = this.ToString() + " - Debug";
            logger(str);
            logger(string.Format("OS: {0} - CLR: {1}", (object) Environment.OSVersion, (object) Environment.Version));
            logger(string.Format("Using: {0}",
                DatabaseUtil.Dialect != null
                    ? (object) DatabaseUtil.Dialect.GetType().Name
                    : (object) "<not initialized>"));
        }

        /// <summary>
        /// Gets the type from the App's own or any of the currently registered Addon Assemblies.
        /// </summary>
        /// <returns></returns>
        public static Type GetType(string name)
        {
            Type type = ServerApp<T>.Instance.GetType().Assembly.GetType(name);
            if (type == (Type) null)
            {
                foreach (WCellAddonContext context in (IEnumerable<WCellAddonContext>) WCellAddonMgr.Contexts)
                {
                    type = context.Assembly.GetType(name);
                    if (type != (Type) null)
                        return type;
                }
            }

            return type;
        }

        /// <summary>Is executed when the server finished starting up</summary>
        public static event Action Started;

        /// <summary>
        /// Starts the server and performs and needed initialization.
        /// </summary>
        public virtual void Start()
        {
            if (this._running)
                return;
            if (ServerApp<T>.InitMgr.PerformInitialization())
            {
                this._tcpEndpoint = new IPEndPoint(Utility.ParseOrResolve(this.Host), this.Port);
                this.Start(true, false);
                if (!(this._running = this.TcpEnabledEnabled))
                {
                    ServerApp<T>.Log.Fatal(WCell_Core.InitFailed);
                    this.Stop();
                }
                else
                {
                    ServerApp<T>.Log.Info("Server started - Max Working Set Size: {0}",
                        (object) Process.GetCurrentProcess().MaxWorkingSet);
                    this.UpdateTitle();
                    Action started = ServerApp<T>.Started;
                    if (started == null)
                        return;
                    started();
                }
            }
            else
            {
                ServerApp<T>.Log.Fatal(WCell_Core.InitFailed);
                this.Stop();
            }
        }

        /// <summary>Triggered when the App shuts down.</summary>
        public static event Action Shutdown;

        /// <summary>Forces the server to shutdown after the given delay.</summary>
        /// <param name="delayMillis">the time to wait before shutting down</param>
        public virtual void ShutdownIn(uint delayMillis)
        {
            this.m_shutdownTimer = new TimerEntry((int) delayMillis, 0, (Action<int>) (upd =>
            {
                AppUtil.UnhookAll();
                if (!this.IsRunning)
                    return;
                this._OnShutdown();
            }));
            this.m_shutdownTimer.Start();
            if (ServerApp<T>.IsPreparingShutdown)
                return;
            ServerApp<T>.ioQueue.RegisterUpdatable((IUpdatable) this.m_shutdownTimer);
            ServerApp<T>.IsPreparingShutdown = true;
        }

        public virtual void CancelShutdown()
        {
            if (!ServerApp<T>.IsPreparingShutdown)
                return;
            ServerApp<T>.ioQueue.UnregisterUpdatable((IUpdatable) this.m_shutdownTimer);
            this.m_shutdownTimer.Stop();
            ServerApp<T>.IsPreparingShutdown = false;
        }

        private void _OnShutdown()
        {
            if (ServerApp<T>.IsShuttingDown)
                return;
            ServerApp<T>.IsShuttingDown = true;
            Action shutdown = ServerApp<T>.Shutdown;
            if (shutdown != null)
                shutdown();
            this.OnShutdown();
            this.Stop();
            ServerApp<T>.Log.Info(WCell_Core.ProcessExited);
            Thread.Sleep(1000);
            Process.GetCurrentProcess().CloseMainWindow();
        }

        protected virtual void OnShutdown()
        {
        }

        public override string ToString()
        {
            return string.Format("WCell {0} (v{1})", (object) this.GetType().Name,
                (object) ServerApp<T>.AssemblyVersion);
        }
    }
}