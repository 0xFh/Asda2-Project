using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class SoulmateExpCommand : RealmServerCommand
    {
        protected SoulmateExpCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("smexp");
            this.EnglishParamInfo = "<amount>";
            this.EnglishDescription = "Sets the given amount of soulmating experience.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Character target = (Character) trigger.Args.Target;
            int num = trigger.Text.NextInt(1);
            if (target.SoulmateRecord == null)
                return;
            target.SoulmateRecord.Expirience = (float) num;
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}