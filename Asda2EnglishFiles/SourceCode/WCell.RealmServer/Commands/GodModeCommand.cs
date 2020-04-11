using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class GodModeCommand : RealmServerCommand
    {
        protected GodModeCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            this.Init("GodMode", "GM");
            this.EnglishParamInfo = "[0|1]";
            this.EnglishDescription = "Toggles the GodMode";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Character target = (Character) trigger.Args.Target;
            bool flag = !trigger.Text.HasNext && !target.GodMode || trigger.Text.NextBool();
            target.GodMode = flag;
            trigger.Reply("GodMode " + (flag ? "ON" : "OFF"));
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}