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
            this.Init("ShowCastFail");
            this.EnglishParamInfo = "<spell> <reason>";
            this.EnglishDescription = "Sends a spell failed packet";
            this.Enabled = false;
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            int num1 = (int) trigger.Text.NextEnum<SpellId>(SpellId.None);
            int num2 = (int) trigger.Text.NextEnum<SpellFailedReason>(SpellFailedReason.Interrupted);
            trigger.Reply("Done.");
        }
    }
}