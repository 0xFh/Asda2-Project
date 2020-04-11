using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class TransformToPetCommand : RealmServerCommand
    {
        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            this.Init("transform", "TransformToPet", "t");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Character target = trigger.Args.Target as Character;
            if (target == null)
            {
                trigger.Reply("Wrong target.");
            }
            else
            {
                int num = trigger.Text.NextInt(3);
                if (num > 810 || num < -1)
                    num = -1;
                target.TransformationId = (short) num;
            }
        }
    }
}