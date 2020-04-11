using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class RespawnCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Respawn");
            this.EnglishParamInfo = "[<radius>]";
            this.EnglishDescription = "Respawns all NPCs in the area. Default Radius = 50";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            float radius = trigger.Text.NextFloat(50f);
            Unit target = trigger.Args.Target;
            Map map = target.Map;
            int objectCount = map.ObjectCount;
            map.RespawnInRadius(target.Position, radius);
            trigger.Reply("Done. Spawned {0} objects.", (object) (map.ObjectCount - objectCount));
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}