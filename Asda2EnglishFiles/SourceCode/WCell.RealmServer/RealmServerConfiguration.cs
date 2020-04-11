using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.Realm;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Res;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Variables;

namespace WCell.RealmServer
{
    /// <summary>
    /// Configuration for the realm server
    /// TODO: Allow to re-load config during runtime (using World-sync)
    /// </summary>
    [XmlRoot("WCellConfig")]
    public class RealmServerConfiguration : WCellConfig<RealmServerConfiguration>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The extra config file (contains privileges amongst others)
        /// </summary>
        public static string ConfigDir = "cfg";

        /// <summary>The default priv Level for new Accounts</summary>
        public static string DefaultRole = "Player";

        public static readonly string BinaryRoot = "../";
        private static string _contentDirName = RealmServerConfiguration.BinaryRoot + "Content/";
        public static readonly HashSet<string> BadWords = new HashSet<string>();
        public static string LangDirName = "Lang";
        private static ClientLocale _defaultLocale = ClientLocale.English;

        /// <summary>The host address to listen on for game connections.</summary>
        public static string Host = IPAddress.Loopback.ToString();

        /// <summary>The address to be sent to the players to connect to.</summary>
        public static string ExternalAddress = RealmServerConfiguration.Host;

        /// <summary>The port to listen on for game connections.</summary>
        public static int Port = 5000;

        private static string _realmName = "Change the RealmName in the Config!";

        /// <summary>The location of the configuration dir</summary>
        private static RealmServerType _serverType = RealmServerType.PVP;

        private static RealmStatus _status = RealmStatus.Open;
        private static RealmCategory _category = RealmCategory.Development;
        private static RealmFlags _flags = RealmFlags.Recommended;

        /// <summary>
        /// The type of database we're connecting to. (e.g. MySQL, mssql2005, Oracle, etc)
        /// </summary>
        public static string DatabaseType = "mysql5";

        private static string _dbConnectionString =
            "Server=127.0.0.1;Port=3306;Database=WCellRealmServer;CharSet=utf8;Uid=root;Pwd=;";

        /// <summary>The address of the auth server.</summary>
        public static string AuthenticationServerAddress = "net.tcp://127.0.0.1:7470";

        /// <summary>
        /// The amount of players this server can hold/allows for.
        /// </summary>
        public static int MaxClientCount = 3000;

        /// <summary>The highest supported version</summary>
        public static ClientId ClientId = ClientId.Wotlk;

        /// <summary>
        /// whether or not to use blizz like character name restrictions (blizzlike = 'Lama', not blizzlike = 'lAmA').
        /// </summary>
        public static bool CapitalizeCharacterNames = true;

        /// <summary>
        /// The level to use for Zlib compression. (1 = fastest, 9 = best compression)
        /// </summary>
        public static int CompressionLevel = 7;

        private static float _ingameMinutePerSecond = 0.01666667f;
        public static string DBCFolderName = "dbc" + WCellInfo.RequiredVersion.BasicString;
        public static string CacheDirName = "Cache";
        public static string RealExternalAddress = "127.0.0.1";
        public static string IPCAddress = "net.tcp://127.0.0.1:7470";

        /// <summary>
        /// The highest level, a Character can reach.
        /// Also see:
        /// TODO: Wrap access to *all* level-related arrays
        /// </summary>
        [NotVariable] public static int MaxCharacterLevel = 80;

        private const string ConfigFilename = "RealmServerConfig.xml";
        private static bool _loaded;
        private readonly AppConfig _cfg;
        private static bool _registerExternalAddress;

        public static RealmServerConfiguration Instance { get; private set; }

        public override string FilePath
        {
            get { return RealmServerConfiguration.GetFullPath("RealmServerConfig.xml"); }
            set { throw new InvalidOperationException("Cannot modify Filename"); }
        }

        [NotVariable] public override bool AutoSave { get; set; }

        public static bool Loaded
        {
            get { return RealmServerConfiguration._loaded; }
            protected set
            {
                RealmServerConfiguration._loaded = value;
                ServerApp<WCell.RealmServer.RealmServer>.Instance.SetTitle("{0} - {1} ...",
                    (object) ServerApp<WCell.RealmServer.RealmServer>.Instance,
                    (object) RealmLocalizer.Instance.Translate(RealmServerConfiguration.DefaultLocale,
                        RealmLangKey.Initializing, new object[0]));
            }
        }

        public static string LangDir
        {
            get { return RealmServerConfiguration.GetContentPath(RealmServerConfiguration.LangDirName) + "/"; }
        }

        public static ClientLocale DefaultLocale
        {
            get { return RealmServerConfiguration._defaultLocale; }
            set
            {
                RealmServerConfiguration._defaultLocale = value;
                WCellConstants.DefaultLocale = value;
            }
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Config, "Initialize Config")]
        public static bool Initialize()
        {
            if (!RealmServerConfiguration.Loaded)
            {
                RealmServerConfiguration.Loaded = true;
                RealmServerConfiguration.BadWordString = "";
                RealmServerConfiguration.Instance.AddVariablesOfAsm<VariableAttribute>(typeof(RealmServerConfiguration)
                    .Assembly);
                try
                {
                    if (!RealmServerConfiguration.Instance.Load())
                    {
                        RealmServerConfiguration.Instance.Save(true, false);
                        RealmServerConfiguration.Log.Warn("Config-file \"{0}\" not found - Created new file.",
                            RealmServerConfiguration.Instance.FilePath);
                        RealmServerConfiguration.Log.Warn(
                            "Please take a little time to configure your server and then restart the Application.");
                        RealmServerConfiguration.Log.Warn(
                            "See http://wiki.wcell.org/index.php/Configuration for more information.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex, "Unable to load Configuration.", new object[0]);
                    RealmServerConfiguration.Log.Error(
                        "Please correct the invalid values in your configuration file and restart the Applicaton.");
                    return false;
                }
            }

            RealmServerConfiguration.Loaded = true;
            return true;
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Fifth)]
        public static void InitializeRoles()
        {
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Last)]
        public static void PerformAutoSave()
        {
            if (!RealmServerConfiguration.Instance.AutoSave)
                return;
            RealmServerConfiguration.Instance.Save(true, true);
        }

        internal static void OnError(string msg)
        {
            RealmServerConfiguration.Log.Warn("<Config>" + msg);
        }

        internal static void OnError(string msg, params object[] args)
        {
            RealmServerConfiguration.Log.Warn("<Config>" + string.Format(msg, args));
        }

        protected RealmServerConfiguration()
            : base(new Action<string>(RealmServerConfiguration.OnError))
        {
            this.RootNodeName = "WCellConfig";
            RealmServerConfiguration.Instance = this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executablePath">The path of the executable whose App-config to load</param>
        public RealmServerConfiguration(string executablePath)
            : this()
        {
            this._cfg = new AppConfig(executablePath);
        }

        /// <summary>The name of this server</summary>
        public static string RealmName
        {
            get { return RealmServerConfiguration._realmName; }
            set
            {
                RealmServerConfiguration._realmName = value;
                if (!ServerApp<WCell.RealmServer.RealmServer>.Instance.IsRunning)
                    return;
                ServerApp<WCell.RealmServer.RealmServer>.Instance.SetTitle(ServerApp<WCell.RealmServer.RealmServer>
                    .Instance.ToString());
                ServerApp<WCell.RealmServer.RealmServer>.Instance.UpdateRealm();
            }
        }

        /// <summary>Type of server</summary>
        public static RealmServerType ServerType
        {
            get { return RealmServerConfiguration._serverType; }
            set
            {
                if (RealmServerConfiguration._serverType == value)
                    return;
                RealmServerConfiguration._serverType = value;
                if (!ServerApp<WCell.RealmServer.RealmServer>.Instance.IsRunning)
                    return;
                ServerApp<WCell.RealmServer.RealmServer>.Instance.UpdateRealm();
            }
        }

        /// <summary>
        /// The status can be Open or Locked (a Locked Realm can only be accessed by Staff members)
        /// </summary>
        public static RealmStatus Status
        {
            get { return RealmServerConfiguration._status; }
            set
            {
                if (RealmServerConfiguration._status == value)
                    return;
                RealmStatus status = RealmServerConfiguration._status;
                RealmServerConfiguration._status = value;
                if (!ServerApp<WCell.RealmServer.RealmServer>.Instance.IsRunning)
                    return;
                ServerApp<WCell.RealmServer.RealmServer>.Instance.OnStatusChange(status);
            }
        }

        /// <summary>The Category of this RealmServer</summary>
        public static RealmCategory Category
        {
            get { return RealmServerConfiguration._category; }
            set
            {
                RealmServerConfiguration._category = value;
                if (!ServerApp<WCell.RealmServer.RealmServer>.Instance.IsRunning)
                    return;
                ServerApp<WCell.RealmServer.RealmServer>.Instance.UpdateRealm();
            }
        }

        /// <summary>The flags of this RealmServer</summary>
        public static RealmFlags Flags
        {
            get { return RealmServerConfiguration._flags; }
            set
            {
                RealmServerConfiguration._flags = value;
                if (!ServerApp<WCell.RealmServer.RealmServer>.Instance.IsRunning)
                    return;
                ServerApp<WCell.RealmServer.RealmServer>.Instance.UpdateRealm();
            }
        }

        [Variable("BadWords")]
        public static string BadWordString
        {
            get { return RealmServerConfiguration.BadWords.ToString<string>("; "); }
            set
            {
                RealmServerConfiguration.BadWords.Clear();
                RealmServerConfiguration.BadWords.AddRange<string>((IEnumerable<string>) value.Split(new char[1]
                {
                    ';'
                }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        /// <summary>
        /// Whether or not to try and register the outside-most IP this computer
        /// goes through as a realm on the authentication server.
        /// </summary>
        public static bool RegisterExternalAddress
        {
            get { return RealmServerConfiguration._registerExternalAddress; }
            set
            {
                RealmServerConfiguration._registerExternalAddress = value;
                if (!ServerApp<WCell.RealmServer.RealmServer>.Instance.IsRunning)
                    return;
                ServerApp<WCell.RealmServer.RealmServer>.Instance.UpdateRealm();
            }
        }

        /// <summary>
        /// The connection string for the authentication server database.
        /// </summary>
        [Variable(IsFileOnly = true)]
        public static string DBConnectionString
        {
            get { return RealmServerConfiguration._dbConnectionString; }
            set { RealmServerConfiguration._dbConnectionString = value; }
        }

        /// <summary>
        /// The speed of time in ingame minute per real-time second.
        /// If set to one, one minute ingame will pass by in one second.
        /// Default: 0.016666666666666666f (1/60)
        /// </summary>
        [Variable("TimeSpeed")]
        public static float IngameMinutesPerSecond
        {
            get { return RealmServerConfiguration._ingameMinutePerSecond; }
            set
            {
                if ((double) value > 60.0)
                    value = 60f;
                if (ServerApp<WCell.RealmServer.RealmServer>.Instance.IsRunning)
                {
                    WCell.RealmServer.RealmServer.ResetTimeStart();
                    RealmServerConfiguration._ingameMinutePerSecond = value;
                }
                else
                    RealmServerConfiguration._ingameMinutePerSecond = value;
            }
        }

        /// <summary>The directory in which to look for XML and DBC files</summary>
        [Variable("ContentDir")]
        public static string ContentDirName
        {
            get { return RealmServerConfiguration._contentDirName; }
            set { RealmServerConfiguration._contentDirName = value; }
        }

        public static string ContentDir
        {
            get { return RealmServerConfiguration.GetFullPath(RealmServerConfiguration.ContentDirName); }
        }

        /// <summary>The directory that holds the DBC files</summary>
        public string DBCFolder
        {
            get
            {
                return Path.Combine(RealmServerConfiguration.ContentDir, RealmServerConfiguration.DBCFolderName) + "/";
            }
        }

        public static string GetDBCFile(string filename)
        {
            if (!filename.EndsWith(".dbc", StringComparison.InvariantCultureIgnoreCase))
                filename += ".dbc";
            string path = Path.Combine(RealmServerConfiguration.Instance.DBCFolder, filename);
            if (!System.IO.File.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                throw new FileNotFoundException(string.Format(WCell_RealmServer.NotFound,
                    (object) ("DBC File (" + filename + ")"), (object) directoryInfo.FullName));
            }

            return path;
        }

        public static string GetContentPath(string file)
        {
            if (!Path.IsPathRooted(file))
                return Path.Combine(RealmServerConfiguration.GetFullPath(RealmServerConfiguration.ContentDir), file);
            return file;
        }

        public static string GetFullPath(string file)
        {
            if (!Path.IsPathRooted(file) && RealmServerConfiguration.Instance._cfg.ExecutableFile.Directory != null)
                return Path.Combine(RealmServerConfiguration.Instance._cfg.ExecutableFile.Directory.FullName, file);
            return file;
        }

        /// <summary>The directory that holds the DBC files</summary>
        public string CacheDir
        {
            get
            {
                string path = Path.Combine(RealmServerConfiguration.ContentDir, RealmServerConfiguration.CacheDirName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        public static bool AutocreateAccounts { get; set; }

        public static bool CacheAccounts { get; set; }

        public string GetCacheFile(string filename)
        {
            return Path.GetFullPath(Path.Combine(this.CacheDir, filename));
        }
    }
}