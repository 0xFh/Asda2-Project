using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class GiveXPCommand : RealmServerCommand
    {
        protected GiveXPCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("GiveXP", "XP", "Exp");
            this.EnglishParamInfo = "<amount>";
            this.EnglishDescription = "Gives the given amount of experience.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            ((Character) trigger.Args.Target).GainXp(trigger.Text.NextInt(1), "gm_command", false);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}