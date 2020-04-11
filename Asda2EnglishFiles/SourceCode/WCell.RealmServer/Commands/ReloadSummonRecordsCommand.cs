using WCell.RealmServer.Content;
using WCell.RealmServer.Items;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ReloadSummonRecordsCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("rsr", "ReloadSummonRecords");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            ContentMgr.Load<Asda2BossSummonRecord>(true);
        }
    }
}