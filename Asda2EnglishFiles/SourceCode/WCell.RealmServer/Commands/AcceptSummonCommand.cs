using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class AcceptSummonCommand : RealmServerCommand
    {
        protected AcceptSummonCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Player; }
        }

        protected override void Initialize()
        {
            this.Init("acpts", "acceptsummon");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (!trigger.Args.Character.CanTeleportToFriend)
                return;
            trigger.Args.Character.CanTeleportToFriend = false;
            if (trigger.Args.Character.SoulmateCharacter == null)
                return;
            trigger.Args.Character.TeleportTo(trigger.Args.Character.TargetSummonMap,
                trigger.Args.Character.TargetSummonPosition);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}