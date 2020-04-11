using Cell.Core;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class DumpNetworkInfoCommand : RealmServerCommand
    {
        protected DumpNetworkInfoCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("DumpNetworkInfo");
            this.EnglishDescription = "Dumps network information including data sent and received, buffer pools, etc.";
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        public override bool RequiresCharacter
        {
            get { return false; }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.None; }
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            long totalBytesSent = ClientBase.TotalBytesSent;
            long totalBytesReceived = ClientBase.TotalBytesReceived;
            int totalSegmentCount = BufferManager.Default.TotalSegmentCount;
            int availableSegmentsCount = BufferManager.Default.AvailableSegmentsCount;
            long globalAllocatedMemory = BufferManager.GlobalAllocatedMemory;
            trigger.Reply("[Network] Total data sent: {0}, Total data received: {1}",
                (object) WCellUtil.FormatBytes((float) totalBytesSent),
                (object) WCellUtil.FormatBytes((float) totalBytesReceived));
            trigger.Reply("[Buffers] {0} available packet buffers out of {1}",
                (object) availableSegmentsCount.ToString(), (object) totalSegmentCount.ToString());
            trigger.Reply("[Buffers] {0} allocated globally",
                (object) WCellUtil.FormatBytes((float) globalAllocatedMemory));
        }
    }
}