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
      new List<WCellAddonContext>();

    /// <summary>
    /// All existing AddonContexts by name of the addon's type
    /// </summary>
    public static readonly IDictionary<string, WCellAddonContext> ContextsByTypeName =
      new Dictionary<string, WCellAddonContext>(
        StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// All existing AddonContexts by ShortName (case-insensitive)
    /// </summary>
    public static readonly Dictionary<string, WCellAddonContext> ContextsByName =
      new Dictionary<string, WCellAddonContext>(
        StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// All existing AddonContexts by Filename (case-insensitive)
    /// </summary>
    public static readonly Dictionary<string, WCellAddonContext> ContextsByFile =
      new Dictionary<string, WCellAddonContext>(
        StringComparer.InvariantCultureIgnoreCase);

    [NotVariable]public static Assembly[] CoreLibs;

    public static IWCellAddon GetAddon(string shortName)
    {
      return GetContextByName(shortName)?.Addon;
    }

    public static WCellAddonContext GetContextByName(string shortName)
    {
      WCellAddonContext wcellAddonContext;
      ContextsByName.TryGetValue(shortName, out wcellAddonContext);
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
      ContextsByTypeName.TryGetValue(typeName, out wcellAddonContext);
      return wcellAddonContext;
    }

    public static WCellAddonContext GetContext(Type addonType)
    {
      return GetContextByTypeName(addonType.FullName);
    }

    public static WCellAddonContext GetContext<A>() where A : IWCellAddon
    {
      return GetContextByTypeName(typeof(A).FullName);
    }

    /// <summary>
    /// Automatically loads all Addons from the given folder, ignoring any sub-folders or files
    /// that are in ignoreString, seperated by semicolon.
    /// </summary>
    /// <param name="folderName">The dir to look in for the Addon-Assemblies.</param>
    /// <param name="ignoreString">eg.: MyDllFile; My2ndFileIsJustALib; AnotherAddonFile</param>
    public static void LoadAddons(string folderName, string ignoreString)
    {
      LoadAddons(new DirectoryInfo(folderName), ignoreString.Split(
          new char[1]
          {
            ';'
          }, StringSplitOptions.RemoveEmptyEntries)
        .TransformArray(s => s.ToLower().Trim().Replace(".dll", "")));
    }

    public static void LoadAddons(DirectoryInfo folder, string[] ignoredNames)
    {
      if(CoreLibs == null)
        CoreLibs = AppDomain.CurrentDomain.GetAssemblies();
      if(!folder.Exists)
        return;
      RecurseLoadAddons(folder, ignoredNames);
      foreach(WCellAddonContext context in Contexts)
        context.InitAddon();
    }

    private static void RecurseLoadAddons(DirectoryInfo folder, string[] ignoredNames)
    {
      foreach(FileSystemInfo fileSystemInfo in folder.GetFileSystemInfos())
      {
        if(!ignoredNames.Contains(fileSystemInfo.Name.ToLower()
          .Replace(".dll", "")))
        {
          if(fileSystemInfo is DirectoryInfo)
            LoadAddons((DirectoryInfo) fileSystemInfo, ignoredNames);
          else if(fileSystemInfo.Name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
          {
            bool flag = true;
            foreach(Assembly coreLib in CoreLibs)
            {
              if(coreLib.FullName.Equals(fileSystemInfo.Name.Replace(".dll", ""),
                StringComparison.CurrentCultureIgnoreCase))
              {
                LogManager.GetCurrentClassLogger().Warn(
                  "The core Assembly \"" + fileSystemInfo.FullName +
                  "\" has been found in the Addon folder where it does not belong.- When compiling custom Addons, please make sure to set 'Copy Local' of all core-references to 'False'!");
                flag = false;
                break;
              }
            }

            if(flag)
              LoadAddon((FileInfo) fileSystemInfo);
          }
        }
      }
    }

    public static WCellAddonContext LoadAddon(FileInfo file)
    {
      string fullName = file.FullName;
      WCellAddonContext context;
      if(ContextsByFile.TryGetValue(fullName, out context))
      {
        if(!Unload(context))
          return null;
      }

      Assembly asm;
      try
      {
        asm = Assembly.LoadFrom(fullName);
      }
      catch(BadImageFormatException ex)
      {
        LogManager.GetCurrentClassLogger().Error(
          "Failed to load Assembly \"{0}\" because it has the wrong format - Make sure that you only load .NET assemblies that are compiled for the correct target platform: {1}",
          file.Name, Environment.Is64BitProcess ? "xx64" : "x86");
        return null;
      }

      WCellAddonContext wcellAddonContext = new WCellAddonContext(file, asm);
      Contexts.Add(wcellAddonContext);
      ContextsByFile.Add(wcellAddonContext.File.FullName, wcellAddonContext);
      return wcellAddonContext;
    }

    public WCellAddonContext TryLoadAddon(string libName)
    {
      WCellAddonContext contextByName = GetContextByName(libName);
      if(contextByName != null)
        return LoadAndInitAddon(contextByName.File);
      if(!libName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
        libName += ".dll";
      return LoadAndInitAddon(libName);
    }

    public virtual WCellAddonContext LoadAndInitAddon(string libName)
    {
      FileInfo file = new FileInfo(libName);
      if(!file.Exists)
        return null;
      return LoadAndInitAddon(file);
    }

    /// <summary>
    /// Loads an Addon from the given file.
    /// Returns null if file does not exist.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public WCellAddonContext LoadAndInitAddon(FileInfo file)
    {
      WCellAddonContext wcellAddonContext = LoadAddon(file);
      if(wcellAddonContext != null)
        wcellAddonContext.InitAddon();
      return wcellAddonContext;
    }

    internal static void RegisterAddon(WCellAddonContext context)
    {
      string fullName = context.Addon.GetType().FullName;
      if(context.ShortName.Length == 0)
      {
        LogManager.GetCurrentClassLogger().Warn("Addon of Type \"{0}\" did not specify a ShortName.",
          context.Addon.GetType().FullName);
      }
      else
      {
        if(context.ShortName.ContainsIgnoreCase("addon"))
          LogManager.GetCurrentClassLogger()
            .Warn(
              "The Addon ShortName \"{0}\" contains the word \"Addon\" - The name should be short and not contain unnecessary information.",
              context.ShortName);
        if(ContextsByName.ContainsKey(context.ShortName))
          throw new ArgumentException(string.Format(
            "Found more than one addon with ShortName \"{0}\": {1} and {2}", context.ShortName,
            GetAddon(context.ShortName), context.Addon));
        ContextsByName.Add(context.ShortName, context);
        if(fullName.Equals(context.ShortName, StringComparison.InvariantCultureIgnoreCase))
          return;
      }

      if(ContextsByTypeName.ContainsKey(fullName))
        throw new InvalidProgramException("Tried to register two Addons with the same TypeName: " + fullName);
      ContextsByTypeName.Add(fullName, context);
    }

    public static bool Unload(WCellAddonContext context)
    {
      IWCellAddon addon = context.Addon;
      if(addon == null)
        return false;
      Logger currentClassLogger = LogManager.GetCurrentClassLogger();
      currentClassLogger.Info("Unloading Addon: " + context + " ...");
      TearDown(addon);
      Contexts.Remove(context);
      ContextsByFile.Remove(context.File.FullName);
      ContextsByName.Remove(context.ShortName);
      ContextsByTypeName.Remove(addon.GetType().FullName);
      currentClassLogger.Info("Done. - Unloaded Addon: " + context);
      return true;
    }

    private static void TearDown(IWCellAddon addon)
    {
      addon.TearDown();
    }
  }
}