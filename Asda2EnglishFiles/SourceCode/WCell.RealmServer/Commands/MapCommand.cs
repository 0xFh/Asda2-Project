using System;
using System.Collections.Generic;
using WCell.Constants.World;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Lang;
using WCell.RealmServer.NPCs;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class MapCommand : RealmServerCommand
    {
        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            this.Init("Map");
            this.EnglishParamInfo = "";
            this.Description = new TranslatableItem(RealmLangKey.CmdMapDescription, new object[0]);
        }

        public class MapSpawnCommand : RealmServerCommand.SubCommand
        {
            protected MapSpawnCommand()
            {
            }

            public override RoleStatus DefaultRequiredStatus
            {
                get { return RoleStatus.EventManager; }
            }

            /// <summary>Spawns all active Maps</summary>
            /// <param name="trigger"></param>
            public static void SpawnAllMaps(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                foreach (Map allMap in WCell.RealmServer.Global.World.GetAllMaps())
                {
                    if (allMap.IsRunning)
                    {
                        Map map = allMap;
                        map.AddMessage((Action) (() =>
                        {
                            if (map.IsSpawned)
                                return;
                            map.SpawnMap();
                            trigger.Reply(RealmLangKey.CmdMapSpawnResponse, (object) map.ToString());
                        }));
                    }
                }
            }

            protected override void Initialize()
            {
                this.Init("Spawn", "S");
                this.ParamInfo = new TranslatableItem(RealmLangKey.CmdMapSpawnParamInfo, new object[0]);
                this.Description = new TranslatableItem(RealmLangKey.CmdMapSpawnDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Map map;
                if (trigger.Text.HasNext)
                {
                    if (trigger.Text.NextModifiers() == "a")
                    {
                        MapCommand.MapSpawnCommand.SpawnAllMaps(trigger);
                        trigger.Reply(RealmLangKey.CmdMapSpawnResponse1);
                        return;
                    }

                    map = WCell.RealmServer.Global.World.GetNonInstancedMap(trigger.Text.NextEnum<MapId>(MapId.End));
                    if (map == null)
                    {
                        trigger.Reply(RealmLangKey.CmdMapSpawnError1);
                        return;
                    }
                }
                else
                {
                    if (trigger.Args.Target == null)
                    {
                        trigger.Reply(RealmLangKey.CmdMapSpawnError2);
                        return;
                    }

                    map = trigger.Args.Target.Map;
                }

                if (map.IsSpawned)
                {
                    trigger.Reply(RealmLangKey.CmdMapSpawnError3);
                }
                else
                {
                    trigger.Reply(RealmLangKey.CmdMapSpawnResponse2, (object) map.Name);
                    if (!GOMgr.Loaded)
                        trigger.Reply(RealmLangKey.CmdMapSpawnError4);
                    if (!NPCMgr.Loaded)
                        trigger.Reply(RealmLangKey.CmdMapSpawnError5);
                    map.AddMessage((Action) (() =>
                    {
                        map.SpawnMap();
                        trigger.Reply(RealmLangKey.CmdMapSpawnResponse3, (object) map);
                    }));
                }
            }
        }

        public class ClearMapCommand : RealmServerCommand.SubCommand
        {
            protected ClearMapCommand()
            {
            }

            public override RoleStatus DefaultRequiredStatus
            {
                get { return RoleStatus.EventManager; }
            }

            protected override void Initialize()
            {
                this.Init("Clear");
                this.ParamInfo = new TranslatableItem(RealmLangKey.CmdMapClearParamInfo, new object[0]);
                this.Description = new TranslatableItem(RealmLangKey.CmdMapClearDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Map map;
                if (trigger.Text.HasNext)
                {
                    map = WCell.RealmServer.Global.World.GetNonInstancedMap(trigger.Text.NextEnum<MapId>(MapId.End));
                    if (map == null)
                    {
                        trigger.Reply(RealmLangKey.CmdMapClearError1);
                        return;
                    }
                }
                else
                {
                    if (trigger.Args.Character == null)
                    {
                        trigger.Reply(RealmLangKey.CmdMapClearError2);
                        return;
                    }

                    map = trigger.Args.Character.Map;
                }

                map.AddMessage((Action) (() =>
                {
                    foreach (WorldObject worldObject in map)
                    {
                        NPC npc = worldObject as NPC;
                        if (npc != null)
                            Asda2CombatHandler.SendMostrDeadToAreaResponse(
                                worldObject.GetNearbyClients<WorldObject>(false), (short) npc.UniqIdOnMap,
                                (short) npc.Asda2X, (short) npc.Asda2Y);
                    }

                    map.RemoveObjects();
                    trigger.Reply(RealmLangKey.CmdMapClearResponse, (object) map.ToString());
                }));
            }
        }

        public class ToggleMapUpdatesCommand : RealmServerCommand.SubCommand
        {
            protected ToggleMapUpdatesCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Updates", "Upd");
                this.EnglishParamInfo = "0|1";
                this.Description = new TranslatableItem(RealmLangKey.CmdMapUpdateDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                bool flag = trigger.Text.NextBool();
                foreach (Map map in WCell.RealmServer.Global.World.Maps)
                {
                    if (map != null && flag != map.IsRunning)
                    {
                        if (flag)
                            map.Start();
                        else
                            map.Stop();
                    }
                }

                trigger.Reply(RealmLangKey.Done);
            }
        }

        public class MapListCommand : RealmServerCommand.SubCommand
        {
            protected MapListCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("List", "L");
                this.Description = new TranslatableItem(RealmLangKey.CmdMapListDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                IEnumerable<Map> allMaps = WCell.RealmServer.Global.World.GetAllMaps();
                if (allMaps == null)
                    return;
                trigger.Reply(RealmLangKey.CmdMapListResponse);
                foreach (Map map in allMaps)
                {
                    if (map.IsRunning)
                        trigger.Reply(map.ToString());
                }
            }
        }
    }
}