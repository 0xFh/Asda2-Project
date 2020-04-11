using NLog;
using System;
using System.Globalization;
using System.IO;
using WCell.Core.Variables;
using WCell.Util.Variables;

namespace WCell.Core.Addons
{
  public abstract class WCellAddonBase : IWCellAddon
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    protected IConfiguration config;

    /// <summary>
    /// The <see cref="T:WCell.Core.Addons.WCellAddonContext" /> that was used to load this Addon.
    /// </summary>
    public WCellAddonContext Context { get; private set; }

    private static void OnError(string msg)
    {
      log.Warn("<Config>" + msg);
    }

    public virtual bool UseConfig
    {
      get { return false; }
    }

    public virtual IConfiguration CreateConfig()
    {
      VariableConfiguration<WCellVariableDefinition> variableConfiguration =
        new VariableConfiguration<WCellVariableDefinition>(OnError);
      variableConfiguration.FilePath =
        Path.Combine(Context.File.DirectoryName, GetType().Name + "Config.xml");
      variableConfiguration.AddVariablesOfAsm<VariableAttribute>(GetType().Assembly);
      return variableConfiguration;
    }

    public abstract string Name { get; }

    public abstract string ShortName { get; }

    public abstract string Author { get; }

    public abstract string Website { get; }

    public abstract void TearDown();

    public IConfiguration Config
    {
      get { return config; }
    }

    public abstract string GetLocalizedName(CultureInfo culture);

    public override string ToString()
    {
      return Name + " (" + ShortName + ") by " + Author;
    }

    public void InitAddon(WCellAddonContext context)
    {
      Context = context;
      if(!UseConfig)
        return;
      config = CreateConfig();
      if(!config.Load())
      {
        try
        {
          config.Save(true, true);
        }
        catch(Exception ex)
        {
          throw new Exception("Unable to save " + config.GetType().Name + " of addon: " + this,
            ex);
        }
      }
    }
  }
}