using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class SendPacketCommand : RealmServerCommand
  {
    protected SendPacketCommand()
    {
    }

    protected override void Initialize()
    {
      Init("SendPacket", "SendP");
      EnglishParamInfo = "<packet> <args>";
      EnglishDescription = "Sends the given packet with corresponding args to the client";
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Player; }
    }

    public class SendSpellDamageLogCommand : SubCommand
    {
      protected SendSpellDamageLogCommand()
      {
      }

      protected override void Initialize()
      {
        Init("SpellLog", "SLog");
        EnglishParamInfo =
          "[<unkBool> [<flags> [<spell> [<damage> [<overkill> [<schools> [<absorbed> [<resisted> [<blocked>]]]]]]]]]";
        EnglishDescription =
          "Sends a SpellMissLog packet to everyone in the area where you are the caster and everyone within 10y radius is the targets.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
      }
    }

    public class SendBGErrorCommand : SubCommand
    {
      protected SendBGErrorCommand()
      {
      }

      protected override void Initialize()
      {
        Init("BGError", "BGErr");
        EnglishParamInfo = "<err> [<bg>]";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        BattlegroundJoinError err = trigger.Text.NextEnum(BattlegroundJoinError.None);
        BattlegroundHandler.SendBattlegroundError(trigger.Args.Character, err);
      }
    }

    public class SendTotemCreated : SubCommand
    {
      protected SendTotemCreated()
      {
      }

      protected override void Initialize()
      {
        Init("TotemCreated", "TC");
        EnglishParamInfo = "[<spellId>]";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
      }
    }
  }
}