using WCell.Constants.Spells;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class ShowCastFailCommand : RealmServerCommand
  {
    protected ShowCastFailCommand()
    {
    }

    protected override void Initialize()
    {
      Init("ShowCastFail");
      EnglishParamInfo = "<spell> <reason>";
      EnglishDescription = "Sends a spell failed packet";
      Enabled = false;
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      int num1 = (int) trigger.Text.NextEnum(SpellId.None);
      int num2 = (int) trigger.Text.NextEnum(SpellFailedReason.Interrupted);
      trigger.Reply("Done.");
    }
  }
}