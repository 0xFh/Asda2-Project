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
            this.Init("ShowBankSlotResult");
            this.EnglishParamInfo = "<value>";
            this.EnglishDescription = "Sends the BankSlotResult packet";
            this.Enabled = false;
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            BuyBankBagResponse response = trigger.Text.NextEnum<BuyBankBagResponse>(BuyBankBagResponse.Ok);
            WCell.RealmServer.Handlers.NPCHandler.SendBankSlotResult(trigger.Args.Character, response);
            trigger.Reply("Done.");
        }

        public override bool RequiresCharacter
        {
            get { return true; }
        }
    }
}