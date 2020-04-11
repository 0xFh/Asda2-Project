using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class StunAllCommand : RealmServerCommand
    {
        protected StunAllCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Stunall");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            foreach (Character allCharacter in World.GetAllCharacters())
            {
                if (allCharacter != trigger.Args.Character && allCharacter.Role.Status < RoleStatus.EventManager &&
                    !allCharacter.IsStunned)
                    allCharacter.IncMechanicCount(SpellMechanic.Stunned, false);
            }
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}