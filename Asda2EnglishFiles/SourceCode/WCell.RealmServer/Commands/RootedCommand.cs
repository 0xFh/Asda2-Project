using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class RootedCommand : RealmServerCommand
    {
        protected RootedCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Rooted", "Root");
            this.EnglishParamInfo = "[0/1]";
            this.EnglishDescription = "Toggles whether the Unit can move or not";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            bool flag = trigger.Text.HasNext && trigger.Text.NextBool() ||
                        !trigger.Text.HasNext && trigger.Args.Target.CanMove;
            if (flag)
                trigger.Args.Target.IncMechanicCount(SpellMechanic.Rooted, false);
            else
                trigger.Args.Target.DecMechanicCount(SpellMechanic.Rooted, false);
            trigger.Reply((flag ? "R" : "Unr") + "ooted ");
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}