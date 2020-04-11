using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class LoveAllCommand : RealmServerCommand
    {
        protected LoveAllCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("LoveAll");
            this.EnglishDescription = "Makes all factions fall in love with the Owner";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            ((Character) trigger.Args.Target).Reputations.LoveAll();
            trigger.Reply("Everyone loves {0} now.", (object) trigger.Args.Target);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}