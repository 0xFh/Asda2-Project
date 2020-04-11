using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
    public class IOWaitCommand : RealmServerCommand
    {
        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        protected override void Initialize()
        {
            this.Init("IOWait");
            this.EnglishDescription =
                "Lets the current Thread wait for the next tick of the IO Queue. Don't use on public servers if you don't know what you are doing!";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.WaitOne();
        }
    }
}