using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ResurrectCommand : RealmServerCommand
    {
        protected ResurrectCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Resurrect", "Res");
            this.EnglishDescription = "Resurrects the Unit";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Unit target = trigger.Args.Target;
            if (target == null)
                return;
            target.Resurrect();
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }
    }
}