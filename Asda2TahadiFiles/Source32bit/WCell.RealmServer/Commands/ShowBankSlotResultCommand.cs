using WCell.RealmServer.NPCs;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class ShowBankSlotResultCommand : RealmServerCommand
  {
    protected ShowBankSlotResultCommand()
    {
    }

    protected override void Initialize()
    {
      Init("ShowBankSlotResult");
      EnglishParamInfo = "<value>";
      EnglishDescription = "Sends the BankSlotResult packet";
      Enabled = false;
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      BuyBankBagResponse response = trigger.Text.NextEnum(BuyBankBagResponse.Ok);
      Handlers.NPCHandler.SendBankSlotResult(trigger.Args.Character, response);
      trigger.Reply("Done.");
    }

    public override bool RequiresCharacter
    {
      get { return true; }
    }
  }
}