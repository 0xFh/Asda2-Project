using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ResetStatsCommand : RealmServerCommand
    {
        protected ResetStatsCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("ResetStats");
            this.EnglishParamInfo = "";
            this.EnglishDescription = "Reset stats to 5 and adding free stat points";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (trigger.Args.Character == null)
                return;
            Character target = trigger.Args.Target as Character;
            if (target == null)
            {
                trigger.Reply("Target is not character");
            }
            else
            {
                target.ResetStatPoints();
                trigger.Reply(string.Format("You have {0} free stat points.",
                    (object) trigger.Args.Character.FreeStatPoints));
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}