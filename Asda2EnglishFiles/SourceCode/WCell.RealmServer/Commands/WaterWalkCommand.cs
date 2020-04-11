using WCell.Constants.Updates;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class WaterWalkCommand : RealmServerCommand
    {
        protected WaterWalkCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("WaterWalk", "WalkWater");
            this.EnglishParamInfo = "[0/1]";
            this.EnglishDescription = "Toggles the ability to walk on water";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            bool flag = trigger.Text.HasNext && trigger.Text.NextBool() || trigger.Args.Target.WaterWalk == 0U;
            if (flag)
                ++trigger.Args.Target.WaterWalk;
            else
                trigger.Args.Target.WaterWalk = 0U;
            trigger.Reply("WaterWalking " + (flag ? "on" : "off"));
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}