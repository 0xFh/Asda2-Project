using NLog;
using System.Collections.Generic;
using System.IO;
using WCell.Core.Addons;
using WCell.Core.Initialization;
using WCell.RealmServer.Commands;

namespace WCell.RealmServer.Addons
{
    /// <summary>
    /// Static helper and container class of all kinds of Addons
    /// </summary>
    public class RealmAddonMgr : WCellAddonMgr<RealmAddonMgr>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static string AddonDir = "RealmServerAddons";

        /// <summary>
        /// A semicolon-separated (;) list of all libs or folders in the AddonDir that are not to be loaded.
        /// </summary>
        public static string IgnoredAddonFiles = "";

        private static bool inited;

        [WCell.Core.Initialization.Initialization(InitializationPass.First, "Initialize Addons")]
        public static void Initialize(InitMgr mgr)
        {
            if (RealmAddonMgr.inited)
                return;
            RealmAddonMgr.inited = true;
            WCellAddonMgr.LoadAddons(RealmServerConfiguration.BinaryRoot + RealmAddonMgr.AddonDir,
                RealmAddonMgr.IgnoredAddonFiles);
            if (WCellAddonMgr.Contexts.Count > 0)
            {
                RealmAddonMgr.log.Info("Found {0} Addon(s):", WCellAddonMgr.Contexts.Count);
                foreach (WCellAddonContext context in (IEnumerable<WCellAddonContext>) WCellAddonMgr.Contexts)
                {
                    RealmAddonMgr.log.Info(" Loaded: " + (context.Addon != null
                                               ? context.Addon.GetDefaultDescription()
                                               : context.Assembly.GetName().Name));
                    RealmAddonMgr.InitAddon(context, mgr);
                }
            }
            else
                RealmAddonMgr.log.Info("No addons found.");
        }

        public static void InitAddon(WCellAddonContext context)
        {
            InitMgr mgr = new InitMgr();
            RealmAddonMgr.InitAddon(context, mgr);
            mgr.AddGlobalMgrsOfAsm(typeof(RealmAddonMgr).Assembly);
            mgr.PerformInitialization();
        }

        protected static void InitAddon(WCellAddonContext context, InitMgr mgr)
        {
            IWCellAddon addon = context.Addon;
            mgr.AddStepsOfAsm(context.Assembly);
            RealmCommandHandler.Instance.AddCmdsOfAsm(context.Assembly);
            if (addon == null || !(addon is WCellAddonBase))
                return;
            ((WCellAddonBase) addon).InitAddon(context);
        }

        public override WCellAddonContext LoadAndInitAddon(string libName)
        {
            if (!Path.IsPathRooted(libName))
                libName = Path.Combine(RealmAddonMgr.AddonDir, libName);
            FileInfo file = new FileInfo(libName);
            if (!file.Exists)
            {
                libName = Path.Combine(file.Directory.FullName, "WCell." + file.Name);
                file = new FileInfo(libName);
                if (!file.Exists)
                    return (WCellAddonContext) null;
            }

            return this.LoadAndInitAddon(file);
        }
    }
}