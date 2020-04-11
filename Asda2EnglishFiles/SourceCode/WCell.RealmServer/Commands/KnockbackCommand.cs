using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class KnockbackCommand : RealmServerCommand
    {
        protected KnockbackCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Knockback");
            this.EnglishParamInfo = "<verticalSpeed> [<horizontalSpeed>]";
            this.EnglishDescription = "Knocks the target back";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            float num = trigger.Text.NextFloat();
            float verticalSpeed = trigger.Text.NextFloat(num);
            MovementHandler.SendKnockBack((WorldObject) trigger.Args.Character, (WorldObject) trigger.Args.Target, num,
                verticalSpeed);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}