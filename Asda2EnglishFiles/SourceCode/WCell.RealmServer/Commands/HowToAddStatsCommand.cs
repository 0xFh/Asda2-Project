using WCell.Constants.Updates;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class HowToAddStatsCommand : RealmServerCommand
    {
        protected HowToAddStatsCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("HowToAddStats");
            this.EnglishParamInfo = "<points>";
            this.EnglishDescription = "Info how to add stats";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (trigger.Args.Character == null)
                return;
            trigger.Reply(string.Format("You have {0} free stat points. Enter the folowint commands to spend it.",
                (object) trigger.Args.Character.FreeStatPoints));
            trigger.Reply("#AddStrength [numberOfPointsToAdd]");
            trigger.Reply("#AddDexterity [numberOfPointsToAdd]");
            trigger.Reply("#AddIntellect [numberOfPointsToAdd]");
            trigger.Reply("#AddSpirit [numberOfPointsToAdd]");
            trigger.Reply("#AddLuck [numberOfPointsToAdd]");
            trigger.Reply("Ex: #AddStrength 20");
            trigger.Reply("Ex: #AStr 20");
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}