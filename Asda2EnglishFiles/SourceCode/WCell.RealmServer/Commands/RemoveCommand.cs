using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class RemoveCommand : RealmServerCommand
    {
        protected RemoveCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Remove", "Delete", "Del");
            this.EnglishDescription = "Deletes the current target (NPC or GO)";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            WorldObject selectedUnitOrGo = trigger.Args.SelectedUnitOrGO;
            if (selectedUnitOrGo != null)
            {
                selectedUnitOrGo.Delete();
                trigger.Reply("Removed {0}.", (object) selectedUnitOrGo);
            }
            else
                trigger.Reply(RealmLangKey.InvalidSelection);
        }
    }
}