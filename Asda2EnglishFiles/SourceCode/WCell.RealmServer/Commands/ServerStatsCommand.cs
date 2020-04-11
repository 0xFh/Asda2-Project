using WCell.Core;
using WCell.RealmServer.Stats;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ServerStatsCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Stats");
            this.EnglishDescription = "Provides commands to show and manage server-statistics.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            foreach (string fullStat in Statistics<RealmStats>.Instance.GetFullStats())
                trigger.Reply(fullStat);
        }
    }
}