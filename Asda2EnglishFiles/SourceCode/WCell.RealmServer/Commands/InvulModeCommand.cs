using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class InvulModeCommand : RealmServerCommand
    {
        protected InvulModeCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Invul");
            this.EnglishParamInfo = "[0|1]";
            this.EnglishDescription = "Toggles Invulnerability";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Unit target = trigger.Args.Target;
            bool flag = trigger.Text.NextBool(!target.IsInvulnerable);
            target.IsInvulnerable = flag;
            trigger.Reply("{0} is now " + (flag ? "Invulnerable" : "Vulnerable"), (object) target.Name);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }
    }
}