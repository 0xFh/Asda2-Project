using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Logs;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class AddFreeStatPointsCommand : RealmServerCommand
    {
        protected AddFreeStatPointsCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("AddFreeStatPoints", "addstatpoints", "asp");
            this.EnglishParamInfo = "<points>";
            this.EnglishDescription = "Info how to add stats";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (trigger.Args.Target == null || !(trigger.Args.Target is Character))
                return;
            int num = trigger.Text.NextInt(0);
            trigger.Args.Character.FreeStatPoints += num;
            Log.Create(Log.Types.StatsOperations, LogSourceType.Character, trigger.Args.Character.EntryId)
                .AddAttribute("source", 0.0, "gm_add_stats").AddAttribute("amount", (double) num, "")
                .AddAttribute("gm_name", 0.0, trigger.Args.User.Name).Write();
            trigger.Reply(string.Format("You have {0} free stat points.",
                (object) trigger.Args.Character.FreeStatPoints));
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}