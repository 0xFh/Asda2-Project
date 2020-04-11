using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;
using WCell.Util.Variables;

namespace WCell.RealmServer.Commands
{
  /// <summary>
  /// 
  /// </summary>
  public class ConfigCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("Config", "Cfg");
      EnglishDescription = "Provides commands to manage the Configuration.";
    }

    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.Admin; }
    }

    public class SetGlobalCommand : SubCommand
    {
      protected SetGlobalCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Set", "S");
        EnglishParamInfo = "<globalVar> <value>";
        EnglishDescription = "Sets the value of the given global variable.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        IConfiguration config = CommandUtil.GetConfig(RealmServerConfiguration.Instance,
          trigger);
        if(config == null)
          return;
        CommandUtil.SetCfgValue(config, trigger);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GetGlobalCommand : SubCommand
    {
      protected GetGlobalCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Get", "G");
        EnglishParamInfo = "<globalVar>";
        EnglishDescription = "Gets the value of the given global variable.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        IConfiguration config = CommandUtil.GetConfig(RealmServerConfiguration.Instance,
          trigger);
        if(config == null)
          return;
        CommandUtil.GetCfgValue(config, trigger);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ListGlobalsCommand : SubCommand
    {
      protected ListGlobalsCommand()
      {
      }

      protected override void Initialize()
      {
        Init("List", "L");
        EnglishParamInfo = "[<name Part>]";
        EnglishDescription =
          "Lists all global variables. If specified only shows variables that contain the given name Part.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        IConfiguration config = CommandUtil.GetConfig(RealmServerConfiguration.Instance,
          trigger);
        if(config == null)
          return;
        CommandUtil.ListCfgValues(config, trigger);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SaveConfigCommand : SubCommand
    {
      protected SaveConfigCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Save");
        EnglishParamInfo = "";
        EnglishDescription = "Saves the current configuration.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        IConfiguration config = CommandUtil.GetConfig(RealmServerConfiguration.Instance,
          trigger);
        if(config == null)
          return;
        config.Save(true, false);
        trigger.Reply("Done.");
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LoadConfigCommand : SubCommand
    {
      protected LoadConfigCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Load");
        EnglishParamInfo = "";
        EnglishDescription = "Loads the configuration again.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        IConfiguration config = CommandUtil.GetConfig(RealmServerConfiguration.Instance,
          trigger);
        if(config == null)
          return;
        config.Load();
        trigger.Reply("Done.");
      }
    }
  }
}