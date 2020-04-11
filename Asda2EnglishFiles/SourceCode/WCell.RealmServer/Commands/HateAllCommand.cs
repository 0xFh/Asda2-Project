using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class HateAllCommand : RealmServerCommand
    {
        protected HateAllCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("HateAll");
            this.EnglishDescription = "Makes all factions hate the Owner";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            ((Character) trigger.Args.Target).Reputations.HateAll();
            trigger.Reply("Everyone dispises of {0} now.", (object) trigger.Args.Target);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}