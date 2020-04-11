using System.Threading;
using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class DumpThreadPoolInfoCommand : RealmServerCommand
    {
        protected DumpThreadPoolInfoCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("DumpTPInfo");
            this.EnglishDescription = "Dumps information about the thread pool.";
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
            int workerThreads1;
            int completionPortThreads1;
            ThreadPool.GetMinThreads(out workerThreads1, out completionPortThreads1);
            int workerThreads2;
            int completionPortThreads2;
            ThreadPool.GetMaxThreads(out workerThreads2, out completionPortThreads2);
            int workerThreads3;
            int completionPortThreads3;
            ThreadPool.GetAvailableThreads(out workerThreads3, out completionPortThreads3);
            trigger.Reply("[Thread Pool] {0} available worker threads out of {1} ({2} minimum)",
                (object) workerThreads3.ToString(), (object) workerThreads2.ToString(),
                (object) workerThreads1.ToString());
            trigger.Reply("[Thread Pool] {0} available IOCP threads out of {1} ({2} minimum)",
                (object) completionPortThreads3.ToString(), (object) completionPortThreads2.ToString(),
                (object) completionPortThreads1.ToString());
        }
    }
}