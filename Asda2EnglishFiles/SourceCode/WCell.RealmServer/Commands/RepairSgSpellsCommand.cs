using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class RepairSgSpellsCommand : RealmServerCommand
    {
        protected RepairSgSpellsCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Player; }
        }

        protected override void Initialize()
        {
            this.Init("rsg");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            trigger.Args.Character.SetClass((int) trigger.Args.Character.RealProffLevel,
                (int) trigger.Args.Character.Archetype.ClassId);
            trigger.Reply("Repaired.");
        }
    }
}