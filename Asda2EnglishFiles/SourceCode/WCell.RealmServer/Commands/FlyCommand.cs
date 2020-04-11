using WCell.Constants.Updates;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class FlyCommand : RealmServerCommand
    {
        protected FlyCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Fly");
            this.EnglishParamInfo = "[0/1]";
            this.EnglishDescription = "Toggles flying mode";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            bool flag = trigger.Text.HasNext && trigger.Text.NextBool() || trigger.Args.Target.Flying == 0U;
            if (flag)
                ++trigger.Args.Target.Flying;
            else
                trigger.Args.Target.Flying = 0U;
            trigger.Reply("Flying " + (flag ? "on" : "off"));
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}