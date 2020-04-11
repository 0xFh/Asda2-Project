using System;
using System.Globalization;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Looting;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core;
using WCell.RealmServer.AI;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Graphics;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
    public class NPCCommand : RealmServerCommand
    {
        protected NPCCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("NPC");
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.None; }
        }

        public static NPCSpawnEntry GetNPCSpawnEntry(CmdTrigger<RealmServerCmdArgs> trigger, bool closest, out Map map)
        {
            Unit target = trigger.Args.Target;
            map = (Map) null;
            NPCSpawnEntry npcSpawnEntry;
            if (closest)
            {
                if (target == null)
                {
                    trigger.Reply("Cannot use the -c switch without active Target.");
                    return (NPCSpawnEntry) null;
                }

                npcSpawnEntry = NPCMgr.GetClosestSpawnEntry((IWorldLocation) target);
                if (npcSpawnEntry == null)
                {
                    trigger.Reply("No Spawnpoint found.");
                    return (NPCSpawnEntry) null;
                }
            }
            else
            {
                string str = trigger.Text.NextWord();
                NPCId result1;
                if (!EnumUtil.TryParse<NPCId>(str, out result1))
                {
                    uint result2;
                    uint.TryParse(str, out result2);
                    npcSpawnEntry = NPCMgr.GetSpawnEntry(result2);
                    if (npcSpawnEntry == null)
                    {
                        trigger.Reply("Invalid SpawnId: " + (object) result2);
                        return (NPCSpawnEntry) null;
                    }
                }
                else
                {
                    NPCEntry entry = NPCMgr.GetEntry(result1);
                    if (entry == null)
                    {
                        trigger.Reply("Entry not found: " + (object) result1);
                        return (NPCSpawnEntry) null;
                    }

                    if (entry.SpawnEntries.Count == 0)
                    {
                        trigger.Reply("Entry has no SpawnEntries: " + (object) entry);
                        return (NPCSpawnEntry) null;
                    }

                    npcSpawnEntry = target != null
                        ? entry.SpawnEntries.GetClosestSpawnEntry((IWorldLocation) target)
                        : entry.SpawnEntries.First<NPCSpawnEntry>();
                }
            }

            map = npcSpawnEntry.Map;
            if (map == null)
            {
                if (target != null && npcSpawnEntry.MapId == target.MapId)
                    map = target.Map;
                else if (WCell.RealmServer.Global.World.IsInstance(npcSpawnEntry.MapId))
                {
                    map = (Map) InstanceMgr.CreateInstance(target as Character, npcSpawnEntry.MapId);
                    if (map == null)
                    {
                        trigger.Reply("Failed to create instance: " + (object) npcSpawnEntry.MapId);
                        return (NPCSpawnEntry) null;
                    }
                }
                else
                {
                    trigger.Reply("Cannot spawn NPC for map: " + (object) npcSpawnEntry.MapId);
                    return (NPCSpawnEntry) null;
                }
            }

            return npcSpawnEntry;
        }

        public class AddNPCCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Add", "A", "Create");
                this.EnglishParamInfo = "[-[i][d <dest>]] <entry> [<amount>]";
                this.EnglishDescription =
                    "Creates one or more NPCs with the given entry id. NPCs are set to active by default. Use -i to spawn an idle NPC. Use -d <dest> to spawn an NPC at a given location.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string str1 = trigger.Text.NextModifiers();
                string name = (string) null;
                if (str1.Contains("d"))
                    name = trigger.Text.NextWord();
                if (str1.Contains("r"))
                {
                    NPC target = trigger.Args.Target as NPC;
                    if (target == null)
                    {
                        trigger.Reply("Wrong target.");
                    }
                    else
                    {
                        EntityId entityId = target.EntityId;
                        NPCSpawnEntry spawnEntry = target.SpawnEntry;
                        spawnEntry.CommitDeleteAndFlush();
                        trigger.Reply("Monstr {0} deleted from spawn.", (object) spawnEntry);
                    }
                }
                else
                {
                    NPCId id = trigger.Text.NextEnum<NPCId>(NPCId.End);
                    if (id == NPCId.End)
                    {
                        trigger.Reply("Invalid NPC.");
                    }
                    else
                    {
                        uint num1 = trigger.Text.NextUInt(1U);
                        if (num1 < 1U)
                        {
                            trigger.Reply("Invalid amount: " + (object) num1);
                        }
                        else
                        {
                            string str2 = (string) null;
                            NPCEntry entry = NPCMgr.GetEntry(id);
                            if (entry == null)
                            {
                                trigger.Reply("Invalid NPCId: " + (object) id);
                            }
                            else
                            {
                                IWorldLocation target;
                                if (!string.IsNullOrEmpty(name))
                                {
                                    target = (IWorldLocation) WorldLocationMgr.Get(name);
                                    if (target == null)
                                    {
                                        trigger.Reply("Invalid destination: " + name);
                                        return;
                                    }
                                }
                                else
                                {
                                    if (trigger.Args.Target == null)
                                    {
                                        trigger.Reply("No destination given.");
                                        return;
                                    }

                                    target = (IWorldLocation) trigger.Args.Target;
                                }

                                int num2 = 1;
                                int x1 = -num2;
                                int y1 = -num2;
                                int num3 = 8;
                                long num4 = (long) num1 + (long) num3;
                                int num5 = 0;
                                while (true)
                                {
                                    while (y1 > num2 || (long) num5++ >= num4)
                                    {
                                        int y2 = num2;
                                        int x2 = x1 + 1;
                                        if ((long) num5 < num4)
                                        {
                                            for (; x2 <= num2 && (long) num5++ < num4; ++x2)
                                            {
                                                if (num3 < num5)
                                                    this.SpawnMob(x2, y2, entry, target);
                                            }

                                            int x3 = num2;
                                            int y3 = y2 - 1;
                                            if ((long) num5 < num4)
                                            {
                                                for (; y3 >= -num2 && (long) num5++ < num4; --y3)
                                                {
                                                    if (num3 < num5)
                                                        this.SpawnMob(x3, y3, entry, target);
                                                }

                                                y1 = -num2;
                                                x1 = x3 - 1;
                                                if ((long) num5 < num4)
                                                {
                                                    for (; x1 >= -num2; --x1)
                                                    {
                                                        if (x1 == -num2)
                                                        {
                                                            ++num2;
                                                            x1 = -num2;
                                                            y1 = -num2;
                                                            break;
                                                        }

                                                        if ((long) num5++ < num4)
                                                        {
                                                            if (num3 < num5)
                                                                this.SpawnMob(x1, y1, entry, target);
                                                        }
                                                        else
                                                            break;
                                                    }

                                                    if ((long) num5 < num4)
                                                        continue;
                                                }
                                            }
                                        }

                                        trigger.Reply("Created {0}.", (object) str2);
                                        return;
                                    }

                                    if (num3 < num5)
                                        this.SpawnMob(x1, y1, entry, target);
                                    ++y1;
                                }
                            }
                        }
                    }
                }
            }

            private void SpawnMob(int x, int y, NPCEntry entry, IWorldLocation dest)
            {
                Vector3 pos = new Vector3(dest.Position.X + (float) x, dest.Position.Y + (float) y);
                WorldLocation worldLocation = new WorldLocation(dest.Map, pos, 1U);
                entry.SpawnAt((IWorldLocation) worldLocation, false).Brain.State = BrainState.GmMove;
            }
        }

        public class NPCSpawnCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("AddSpawn", "Spawn");
                this.EnglishParamInfo = "[-c]|[<NPCId or spawnid> [<amount>]]";
                this.EnglishDescription =
                    "Creates the NPC-spawnpoint with the given id. -c switch simply creates the spawnpoint that is closest to you";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string str = trigger.Text.NextModifiers();
                Map map;
                NPCSpawnEntry npcSpawnEntry = NPCCommand.GetNPCSpawnEntry(trigger, str == "c", out map);
                if (npcSpawnEntry == null)
                    return;
                uint num = trigger.Text.NextUInt(1U);
                if (num < 1U)
                {
                    trigger.Reply("Invalid amount: " + (object) num);
                }
                else
                {
                    map.AddNPCSpawnPool(npcSpawnEntry.PoolTemplate);
                    if (trigger.Args.Target != null)
                        trigger.Args.Target.TeleportTo(map, npcSpawnEntry.Position);
                    trigger.Reply("Created spawn: {0}", (object) npcSpawnEntry);
                }
            }
        }

        public class NPCGotoCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Goto");
                this.EnglishParamInfo = "[-c]|[<NPCId or spawnid>";
                this.EnglishDescription =
                    "Teleports the target to the first (or given spawn-index of) NPC of the given type.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string str = trigger.Text.NextModifiers();
                Map map;
                NPCSpawnEntry npcSpawnEntry = NPCCommand.GetNPCSpawnEntry(trigger, str == "c", out map);
                if (npcSpawnEntry == null)
                    return;
                if (trigger.Args.Target != null)
                    trigger.Args.Target.TeleportTo(map, npcSpawnEntry.Position);
                trigger.Reply("Created spawn: {0}", (object) npcSpawnEntry);
            }
        }

        public class SelectNPCCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Select", "Sel");
                this.EnglishParamInfo = "[-[n][d] [<name>][<destination>]]";
                this.EnglishDescription =
                    "Selects the NPC whose name matches the given name and is closest to the given location. All arguments are optional. If no arguments are supplied, the first available NPC will be selected. If the destination is not given, it will search, starting at the current Target or oneself.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string str1 = trigger.Text.NextModifiers();
                string name = "";
                if (str1.Contains("n"))
                    name = trigger.Text.NextWord();
                Map rgn;
                if (str1.Contains("d"))
                {
                    string str2 = trigger.Text.NextWord();
                    INamedWorldZoneLocation worldZoneLocation = WorldLocationMgr.Get(str2);
                    if (worldZoneLocation == null)
                    {
                        MapId result;
                        rgn = !EnumUtil.TryParse<MapId>(str2, out result)
                            ? (Map) null
                            : WCell.RealmServer.Global.World.GetNonInstancedMap(result);
                        if (rgn == null)
                        {
                            trigger.Reply("Invalid Destination: " + str2);
                            return;
                        }
                    }
                    else
                        rgn = worldZoneLocation.Map;
                }
                else
                {
                    Unit target = trigger.Args.Target;
                    if (target == null)
                    {
                        trigger.Reply("Must have target or specify destination (using the -d switch).");
                        return;
                    }

                    rgn = target.Map;
                    int phase = (int) target.Phase;
                }

                if (rgn == null)
                {
                    trigger.Reply("Instance-destinations are currently not supported.");
                }
                else
                {
                    NPC npc = (NPC) null;
                    rgn.ExecuteInContext((Action) (() =>
                    {
                        foreach (WorldObject worldObject in rgn)
                        {
                            if (worldObject is NPC && (name == "" || worldObject.Name.ContainsIgnoreCase(name)))
                            {
                                npc = (NPC) worldObject;
                                break;
                            }
                        }

                        if (npc == null)
                        {
                            trigger.Reply("Could not find a matching NPC.");
                        }
                        else
                        {
                            Character character = trigger.Args.Character;
                            if (trigger.Args.HasCharacter)
                            {
                                if (name == "" && character.Target != null)
                                {
                                    if (character.Target is NPC)
                                        npc = character.Target as NPC;
                                    else
                                        character.Target = (Unit) npc;
                                }
                                else
                                    character.Target = (Unit) npc;
                            }
                            else
                            {
                                trigger.Args.Target = (Unit) npc;
                                trigger.Args.Context = (IContextHandler) npc;
                            }

                            trigger.Reply("Selected: {0}", (object) npc);
                        }
                    }));
                }
            }
        }

        public class FlagsNPCCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Flags", "F");
                this.EnglishParamInfo = "[-[n] [<name>]]";
                this.EnglishDescription =
                    "Selects the NPC that is the characters current selection or whose name matches the given name and is closest to the given location. All arguments are optional. If no arguments are supplied and there is no current selection, the first available NPC will be selected. ";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string str = trigger.Text.NextModifiers();
                string name = "";
                if (str.Contains("n"))
                    name = trigger.Text.NextWord();
                Unit target = trigger.Args.Target;
                if (target == null)
                {
                    trigger.Reply("No target found.");
                }
                else
                {
                    Map rgn = target.Map;
                    if (rgn == null)
                    {
                        trigger.Reply("Instances are currently not supported.");
                    }
                    else
                    {
                        NPC npc = (NPC) null;
                        rgn.ExecuteInContext((Action) (() =>
                        {
                            foreach (WorldObject worldObject in rgn)
                            {
                                if (worldObject is NPC && (name == "" || worldObject.Name.ContainsIgnoreCase(name)))
                                {
                                    npc = (NPC) worldObject;
                                    break;
                                }
                            }
                        }));
                        if (npc == null)
                        {
                            trigger.Reply("Could not find a matching NPC.");
                        }
                        else
                        {
                            Character character = trigger.Args.Character;
                            if (trigger.Args.HasCharacter)
                            {
                                if (name == "" && character.Target != null)
                                {
                                    if (character.Target is NPC)
                                        npc = character.Target as NPC;
                                    else
                                        character.Target = (Unit) npc;
                                }
                                else
                                    character.Target = (Unit) npc;
                            }
                            else
                            {
                                trigger.Args.Target = (Unit) npc;
                                trigger.Args.Context = (IContextHandler) npc;
                            }

                            trigger.Reply("Selected: {0}", (object) npc);
                            NPCFlags npcFlags = npc.NPCFlags;
                            trigger.Reply("NPCFlags {0}:{1}", (object) npcFlags, (object) npcFlags);
                            UnitDynamicFlags dynamicFlags = npc.DynamicFlags;
                            trigger.Reply("DynamicFlags {0}:{1}", (object) dynamicFlags, (object) dynamicFlags);
                            UnitExtraFlags extraFlags = npc.ExtraFlags;
                            trigger.Reply("ExtraFlags {0}:{1}", (object) extraFlags, (object) extraFlags);
                            StateFlag stateFlags = npc.StateFlags;
                            trigger.Reply("StateFlags {0}:{1}", (object) stateFlags, (object) stateFlags);
                            UnitFlags unitFlags = npc.UnitFlags;
                            trigger.Reply("UnitFlags {0}:{1}", (object) unitFlags, (object) (int) unitFlags);
                            UnitFlags2 unitFlags2 = npc.UnitFlags2;
                            trigger.Reply("UnitFlags2 {0}:{1}", (object) unitFlags2, (object) unitFlags2);
                        }
                    }
                }
            }
        }

        public class SelectableNPCCommand : RealmServerCommand
        {
            protected override void Initialize()
            {
                this.Init("Selectable", "NPCSel");
                this.EnglishDescription = "Makes all NPCs on the current Map selectable";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Map rgn = trigger.Args.Character.Map;
                if (rgn == null)
                {
                    trigger.Reply("Instances are currently not supported.");
                }
                else
                {
                    rgn.ExecuteInContext((Action) (() =>
                    {
                        foreach (WorldObject worldObject in rgn)
                        {
                            if (worldObject is NPC)
                                ((NPC) worldObject).UnitFlags &= UnitFlags.CanPerformAction_Mask1 |
                                                                 UnitFlags.Flag_0_0x1 |
                                                                 UnitFlags.SelectableNotAttackable |
                                                                 UnitFlags.Influenced | UnitFlags.PlayerControlled |
                                                                 UnitFlags.Flag_0x10 | UnitFlags.Preparation |
                                                                 UnitFlags.PlusMob |
                                                                 UnitFlags.SelectableNotAttackable_2 |
                                                                 UnitFlags.NotAttackable | UnitFlags.Passive |
                                                                 UnitFlags.Looting | UnitFlags.PetInCombat |
                                                                 UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced |
                                                                 UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 |
                                                                 UnitFlags.SelectableNotAttackable_3 |
                                                                 UnitFlags.Combat | UnitFlags.TaxiFlight |
                                                                 UnitFlags.Disarmed | UnitFlags.Confused |
                                                                 UnitFlags.Feared | UnitFlags.Possessed |
                                                                 UnitFlags.Skinnable | UnitFlags.Mounted |
                                                                 UnitFlags.Flag_28_0x10000000 |
                                                                 UnitFlags.Flag_29_0x20000000 |
                                                                 UnitFlags.Flag_30_0x40000000 |
                                                                 UnitFlags.Flag_31_0x80000000;
                        }
                    }));
                    trigger.Reply("Done.");
                }
            }

            public override bool RequiresCharacter
            {
                get { return true; }
            }
        }

        public class NPCLootCommand : RealmServerCommand
        {
            protected override void Initialize()
            {
                this.Init("NPCLoot", "Loot", "L");
                this.EnglishDescription = "Shows loot of selected target.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                if (trigger.Args.Character.Target == null)
                {
                    trigger.Reply("Select target.");
                }
                else
                {
                    NPC target = trigger.Args.Character.Target as NPC;
                    if (target == null)
                    {
                        trigger.Reply("You must select a monstr.");
                    }
                    else
                    {
                        trigger.Reply("Target is {0},", (object) target);
                        foreach (Asda2LootItemEntry entry in Asda2LootMgr.GetEntries(Asda2LootEntryType.Npc,
                            target.EntryId))
                            trigger.Reply("{0} [{1}] [{2}-{3}] [{4}]", (object) entry.ItemId,
                                (object) (int) entry.ItemId, (object) entry.MinAmount, (object) entry.MaxAmount,
                                (object) entry.DropChance.ToString((IFormatProvider) CultureInfo.InvariantCulture)
                                    .Replace('0', 'O'), (object) entry.RequiredQuestId);
                    }
                }
            }

            public override bool RequiresCharacter
            {
                get { return true; }
            }
        }
    }
}