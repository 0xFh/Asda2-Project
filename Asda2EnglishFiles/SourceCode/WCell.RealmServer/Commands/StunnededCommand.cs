using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class StunnededCommand : RealmServerCommand
    {
        protected StunnededCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Stunned", "Stun");
            this.EnglishParamInfo = "[0/1]";
            this.EnglishDescription = "Toggles whether the Unit is stunned or not";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            bool flag = trigger.Text.HasNext && trigger.Text.NextBool() ||
                        !trigger.Text.HasNext && trigger.Args.Target.CanMove;
            if (flag)
                trigger.Args.Target.IncMechanicCount(SpellMechanic.Stunned, false);
            else
                trigger.Args.Target.DecMechanicCount(SpellMechanic.Stunned, false);
            trigger.Reply((flag ? "S" : "Uns") + "tunned ");
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}