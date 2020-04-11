using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.Core.Addons
{
    /// <summary>Static helper and container class</summary>
    public class WCellAddonMgr
    {
        /// <summary>All contexts of all Addons and utility libraries.</summary>
        public static readonly IList<WCellAddonContext> Contexts =
            (IList<WCellAddonContext>)new List<WCellAddonContext>();

        /// <summary>
        /// All existing AddonContexts by name of the addon's type
        /// </summary>
        public static readonly IDictionary<string, WCellAddonContext> ContextsByTypeName =
            (IDictionary<string, WCellAddonContext>)new Dictionary<string, WCellAddonContext>(
                (IEqualityComparer<string>)StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// All existing AddonContexts by ShortName (case-insensitive)
        /// </summary>
        public static readonly Dictionary<string, WCellAddonContext> ContextsByName =
            new Dictionary<string, WCellAddonContext>(
                (IEqualityComparer<string>)StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// All existing AddonContexts by Filename (case-insensitive)
        /// </summary>
        public static readonly Dictionary<string, WCellAddonContext> ContextsByFile =
            new Dictionary<string, WCellAddonContext>(
                (IEqualityComparer<string>)StringComparer.InvariantCultureIgnoreCase);

        [NotVariable] public static Assembly[] CoreLibs;

        public static IWCellAddon GetAddon(string shortName)
        {
            return WCellAddonMgr.GetContextByName(shortName)?.Addon;
        }

        public static WCellAddonContext GetContextByName(string shortName)
        {
            WCellAddonContext wcellAddonContext;
            WCellAddonMgr.ContextsByName.TryGetValue(shortName, out wcellAddonContext);
            return wcellAddonContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName">The full typename of the WCellAddon</param>
        /// <returns></returns>
        public static WCellAddonContext GetContextByTypeName(string typeName)
        {
            WCellAddonContext wcellAddonContext;
            WCellAddonMgr.ContextsByTypeName.TryGetValue(typeName, out wcellAddonContext);
            return wcellAddonContext;
        }

        public static WCellAddonContext GetContext(Type addonType)
        {
            return WCellAddonMgr.GetContextByTypeName(addonType.FullName);
        }

        public static WCellAddonContext GetContext<A>() where A : IWCellAddon
        {
            return WCellAddonMgr.GetContextByTypeName(typeof(A).FullName);
        }

        /// <summary>
        /// Automatically loads all Addons from the given folder, ignoring any sub-folders or files
        /// that are in ignoreString, seperated by semicolon.
        /// </summary>
        /// <param name="folderName">The dir to look in for the Addon-Assemblies.</param>
        /// <param name="ignoreString">eg.: MyDllFile; My2ndFileIsJustALib; AnotherAddonFile</param>
        public static void LoadAddons(string folderName, string ignoreString)
        {
            WCellAddonMgr.LoadAddons(new DirectoryInfo(folderName), ((IEnumerable<string>)ignoreString.Split(
                    new char[1]
                    {
                        ';'
                    }, StringSplitOptions.RemoveEmptyEntries))
                .TransformArray<string, string>((Func<string, string>)(s => s.ToLower().Trim().Replace(".dll", ""))));
        }

        public static void LoadAddons(DirectoryInfo folder, string[] ignoredNames)
        {
            if (WCellAddonMgr.CoreLibs == null)
                WCellAddonMgr.CoreLibs = AppDomain.CurrentDomain.GetAssemblies();
            if (!folder.Exists)
                return;
            WCellAddonMgr.RecurseLoadAddons(folder, ignoredNames);
            foreach (WCellAddonContext context in (IEnumerable<WCellAddonContext>)WCellAddonMgr.Contexts)
                context.InitAddon();
        }

        private static void RecurseLoadAddons(DirectoryInfo folder, string[] ignoredNames)
        {
            foreach (FileSystemInfo fileSystemInfo in folder.GetFileSystemInfos())
            {
                if (!((IEnumerable<string>)ignoredNames).Contains<string>(fileSystemInfo.Name.ToLower()
                    .Replace(".dll", "")))
                {
                    if (fileSystemInfo is DirectoryInfo)
                        WCellAddonMgr.LoadAddons((DirectoryInfo)fileSystemInfo, ignoredNames);
                    else if (fileSystemInfo.Name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                    {
                        bool flag = true;
                        foreach (Assembly coreLib in WCellAddonMgr.CoreLibs)
                        {
                            if (coreLib.FullName.Equals(fileSystemInfo.Name.Replace(".dll", ""),
                                StringComparison.CurrentCultureIgnoreCase))
                            {
                                LogManager.GetCurrentClassLogger().Warn(
                                    "The core Assembly \"" + fileSystemInfo.FullName +
                                    "\" has been found in the Addon folder where it does not belong.- When compiling custom Addons, please make sure to set 'Copy Local' of all core-references to 'False'!");
                                flag = false;
                                break;
                            }
                        }

                        if (flag)
                            WCellAddonMgr.LoadAddon((FileInfo)fileSystemInfo);
                    }
                }
            }
        }

        public static WCellAddonContext LoadAddon(FileInfo file)
        {
            string fullName = file.FullName;
            WCellAddonContext context;
            if (WCellAddonMgr.ContextsByFile.TryGetValue(fullName, out context))
            {
                if (!WCellAddonMgr.Unload(context))
                    return (WCellAddonContext)null;
            }

            Assembly asm;
            try
            {
                asm = Assembly.LoadFrom(fullName);
            }
            catch (BadImageFormatException ex)
            {
                LogManager.GetCurrentClassLogger().Error(
                    "Failed to load Assembly \"{0}\" because it has the wrong format - Make sure that you only load .NET assemblies that are compiled for the correct target platform: {1}",
                    (object)file.Name, Environment.Is64BitProcess ? (object)"xx64" : (object)"x86");
                return (WCellAddonContext)null;
            }

            WCellAddonContext wcellAddonContext = new WCellAddonContext(file, asm);
            WCellAddonMgr.Contexts.Add(wcellAddonContext);
            WCellAddonMgr.ContextsByFile.Add(wcellAddonContext.File.FullName, wcellAddonContext);
            return wcellAddonContext;
        }

        public WCellAddonContext TryLoadAddon(string libName)
        {
            WCellAddonContext contextByName = WCellAddonMgr.GetContextByName(libName);
            if (contextByName != null)
                return this.LoadAndInitAddon(contextByName.File);
            if (!libName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                libName += ".dll";
            return this.LoadAndInitAddon(libName);
        }

        public virtual WCellAddonContext LoadAndInitAddon(string libName)
        {
            FileInfo file = new FileInfo(libName);
            if (!file.Exists)
                return (WCellAddonContext)null;
            return this.LoadAndInitAddon(file);
        }

        /// <summary>
        /// Loads an Addon from the given file.
        /// Returns null if file does not exist.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public WCellAddonContext LoadAndInitAddon(FileInfo file)
        {
            WCellAddonContext wcellAddonContext = WCellAddonMgr.LoadAddon(file);
            if (wcellAddonContext != null)
                wcellAddonContext.InitAddon();
            return wcellAddonContext;
        }

        internal static void RegisterAddon(WCellAddonContext context)
        {
            string fullName = context.Addon.GetType().FullName;
            if (context.ShortName.Length == 0)
            {
                LogManager.GetCurrentClassLogger().Warn("Addon of Type \"{0}\" did not specify a ShortName.",
                    context.Addon.GetType().FullName);
            }
            else
            {
                if (context.ShortName.ContainsIgnoreCase("addon"))
                    LogManager.GetCurrentClassLogger()
                        .Warn(
                            "The Addon ShortName \"{0}\" contains the word \"Addon\" - The name should be short and not contain unnecessary information.",
                            context.ShortName);
                if (WCellAddonMgr.ContextsByName.ContainsKey(context.ShortName))
                    throw new ArgumentException(string.Format(
                        "Found more than one addon with ShortName \"{0}\": {1} and {2}", (object)context.ShortName,
                        (object)WCellAddonMgr.GetAddon(context.ShortName), (object)context.Addon));
                WCellAddonMgr.ContextsByName.Add(context.ShortName, context);
                if (fullName.Equals(context.ShortName, StringComparison.InvariantCultureIgnoreCase))
                    return;
            }

            if (WCellAddonMgr.ContextsByTypeName.ContainsKey(fullName))
                throw new InvalidProgramException("Tried to register two Addons with the same TypeName: " + fullName);
            WCellAddonMgr.ContextsByTypeName.Add(fullName, context);
        }

        public static bool Unload(WCellAddonContext context)
        {
            IWCellAddon addon = context.Addon;
            if (addon == null)
                return false;
            Logger currentClassLogger = LogManager.GetCurrentClassLogger();
            currentClassLogger.Info("Unloading Addon: " + (object)context + " ...");
            WCellAddonMgr.TearDown(addon);
            WCellAddonMgr.Contexts.Remove(context);
            WCellAddonMgr.ContextsByFile.Remove(context.File.FullName);
            WCellAddonMgr.ContextsByName.Remove(context.ShortName);
            WCellAddonMgr.ContextsByTypeName.Remove(addon.GetType().FullName);
            currentClassLogger.Info("Done. - Unloaded Addon: " + (object)context);
            return true;
        }

        private static void TearDown(IWCellAddon addon)
        {
            addon.TearDown();
        }
    }
}