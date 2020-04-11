using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ForgetSelfCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("ResetWorld");
            this.EnglishParamInfo = "";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            ((Character) trigger.Args.Target).ResetOwnWorld();
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}