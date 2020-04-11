using System.Collections.Generic;
using System.Linq;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Instances;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class InstanceCommand : RealmServerCommand
    {
        protected InstanceCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Instance", "Inst");
            this.EnglishDescription = "Provides some Commands to manage and use Instances.";
        }

        public static InstancedMap GetInstance(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (!trigger.Text.HasNext)
                trigger.Reply("No MapId specified.");
            MapId mapId = trigger.Text.NextEnum<MapId>(MapId.End);
            if (mapId == MapId.End)
            {
                trigger.Reply("Invalid MapId.");
                return (InstancedMap) null;
            }

            if (!trigger.Text.HasNext)
                trigger.Reply("No Instance-Id specified.");
            uint instanceId = trigger.Text.NextUInt();
            BaseInstance instance = InstanceMgr.Instances.GetInstance(mapId, instanceId);
            if (instance == null)
                trigger.Reply("Instance id does not exist: #{1} (for {0})", (object) mapId, (object) instanceId);
            return (InstancedMap) instance;
        }

        public class InstanceListAllCommand : RealmServerCommand.SubCommand
        {
            protected InstanceListAllCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("List", "L");
                this.EnglishParamInfo = "[<MapId>]";
                this.EnglishDescription = "Lists all active Instances, or those of the given Map.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                IEnumerable<BaseInstance> source;
                if (trigger.Text.HasNext)
                {
                    MapId map = trigger.Text.NextEnum<MapId>(MapId.End);
                    if (map == MapId.End)
                    {
                        trigger.Reply("Invalid BattlegroundId.");
                        return;
                    }

                    source = (IEnumerable<BaseInstance>) InstanceMgr.Instances.GetInstances(map);
                }
                else
                    source = (IEnumerable<BaseInstance>) InstanceMgr.Instances.GetAllInstances();

                trigger.Reply("Found {0} instances:", (object) source.Count<BaseInstance>());
                foreach (BaseInstance baseInstance in source)
                    trigger.Reply(baseInstance.ToString());
            }
        }

        public class InstanceCreateCommand : RealmServerCommand.SubCommand
        {
            protected InstanceCreateCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Create", "C");
                this.EnglishParamInfo = "[-e[d]] <MapId> [<difficulty>]";
                this.EnglishDescription =
                    "Creates a new Instance of the given Map. -d allows to specify the difficulty (value between 0 and 3). -e enters it right away.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Character target = trigger.Args.Target as Character;
                string str = trigger.Text.NextModifiers();
                MapId mapID = trigger.Text.NextEnum<MapId>(MapId.End);
                if (mapID == MapId.End)
                {
                    trigger.Reply("Invalid MapId.");
                }
                else
                {
                    MapTemplate mapTemplate = WCell.RealmServer.Global.World.GetMapTemplate(mapID);
                    if (mapTemplate != null && mapTemplate.IsInstance)
                    {
                        uint num;
                        if (str.Contains("d"))
                        {
                            num = trigger.Text.NextUInt();
                            if (mapTemplate.GetDifficulty(num) == null)
                                trigger.Reply("Invalid Difficulty: {0}");
                        }
                        else
                            num = target == null ? 0U : target.GetInstanceDifficulty(mapTemplate.IsRaid);

                        BaseInstance instance = InstanceMgr.CreateInstance(target, mapTemplate.InstanceTemplate, num);
                        if (instance != null)
                        {
                            trigger.Reply("Instance created: " + (object) instance);
                            if (!str.Contains("e") || target == null)
                                return;
                            instance.TeleportInside((Character) trigger.Args.Target);
                        }
                        else
                            trigger.Reply("Unable to create Instance of: " + (object) mapTemplate);
                    }
                    else
                        trigger.Reply("Invalid MapId.");
                }
            }
        }

        public class InstanceEnterCommand : RealmServerCommand.SubCommand
        {
            protected InstanceEnterCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Enter", "E");
                this.EnglishParamInfo = "<MapId> <InstanceId> [<entrance>]";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                InstancedMap instance = InstanceCommand.GetInstance(trigger);
                if (instance == null)
                    return;
                int entrance = trigger.Text.NextInt(0);
                instance.TeleportInside(trigger.Args.Character, entrance);
            }
        }

        public class InstanceDeleteCommand : RealmServerCommand.SubCommand
        {
            protected InstanceDeleteCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Delete", "Del");
                this.EnglishParamInfo = "[<MapId> <InstanceId>]";
                this.EnglishDescription =
                    "Delets the Instance of the given Map with the given Id, or the current one if no arguments are supplied.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                InstancedMap instancedMap;
                if (!trigger.Text.HasNext && trigger.Args.Character != null)
                {
                    instancedMap = trigger.Args.Character.Map as InstancedMap;
                    if (instancedMap == null)
                    {
                        trigger.Reply("Current Map is not an Instance.");
                        return;
                    }
                }
                else
                {
                    instancedMap = InstanceCommand.GetInstance(trigger);
                    if (instancedMap == null)
                        return;
                }

                instancedMap.Delete();
                trigger.Reply("Instance Deleted");
            }
        }
    }
}