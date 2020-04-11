using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class SetBroadCastRangeCommand : RealmServerCommand
    {
        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        protected override void Initialize()
        {
            this.Init("SetBroadCastRange", "Broadcastrange", "BCR");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            WorldObject.BroadcastRange = trigger.Text.NextFloat(50f);
        }
    }
}