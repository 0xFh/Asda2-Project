using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class DistanceCommand : RealmServerCommand
    {
        protected DistanceCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Distance", "Dist");
            this.EnglishDescription =
                "Measures the distance between you and the currently target object (including selected GOs).";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            WorldObject selectedUnitOrGo = trigger.Args.SelectedUnitOrGO;
            if (selectedUnitOrGo != null)
                trigger.Reply("The distance is: " + (object) trigger.Args.Character.GetDistance(selectedUnitOrGo));
            else
                trigger.Reply("Nothing selected.");
        }
    }
}