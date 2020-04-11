using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ClearDOsCommand : RealmServerCommand
    {
        protected ClearDOsCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("ClearDOs");
            this.EnglishDescription = "Removes all staticly spawned DOs";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            foreach (WorldObject worldObject in SpellHandler.StaticDOs.Values)
                worldObject.Delete();
            SpellHandler.StaticDOs.Clear();
        }
    }
}