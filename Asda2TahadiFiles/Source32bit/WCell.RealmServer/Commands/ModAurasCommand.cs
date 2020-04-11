using System.Reflection;
using WCell.Constants.Updates;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class ModAurasCommand : RealmServerCommand
  {
    protected ModAurasCommand()
    {
    }

    protected override void Initialize()
    {
      Init("ModAuras", "MAura");
      EnglishParamInfo = "<n> <subcommand> ...";
      EnglishDescription = "Modifies the nth Aura of the target";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      ProcessNth(trigger);
    }

    public override bool RequiresCharacter
    {
      get { return false; }
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Unit; }
    }

    public class ModLevelCommand : SubCommand
    {
      protected ModLevelCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Level", "Lvl", "L");
        EnglishParamInfo = "<AuraLevel>";
        EnglishDescription = "Modifies the Level of the nth Aura.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        uint n = ((RealmServerNCmdArgs) trigger.Args).N - 1U;
        Aura at = trigger.Args.Target.Auras.GetAt(n);
        if(at != null)
          ModPropCommand.ModProp(at, at.GetType().GetProperty("Level"), trigger);
        else
          trigger.Reply("There aren't " + n + " Auras.");
      }
    }

    public class ModFlagsCommand : SubCommand
    {
      protected ModFlagsCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Flags", "Fl", "F");
        EnglishParamInfo = "<AuraFlags>";
        EnglishDescription = "Modifies the Flags of the nth Aura.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        uint n = ((RealmServerNCmdArgs) trigger.Args).N - 1U;
        Aura at = trigger.Args.Target.Auras.GetAt(n);
        if(at != null)
          ModPropCommand.ModProp(at, at.GetType().GetProperty("Flags"), trigger);
        else
          trigger.Reply("There aren't " + n + " Auras.");
      }
    }
  }
}