using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Spawns;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.Global
{
    /// <summary>Manages world events</summary>
    public static class WorldEventMgr
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        internal static WorldEvent[] AllEvents = new WorldEvent[100];
        internal static WorldEvent[] ActiveEvents = new WorldEvent[100];
        [NotVariable] public static List<WorldEventQuest> WorldEventQuests = new List<WorldEventQuest>();
        internal static uint _eventCount;
        private static DateTime LastUpdateTime;

        public static uint EventCount
        {
            get { return WorldEventMgr._eventCount; }
        }

        public static bool Loaded { get; private set; }

        public static void Initialize()
        {
            WorldEventMgr.LoadAll();
        }

        /// <summary>Loads the world events.</summary>
        /// <returns></returns>
        public static bool LoadAll()
        {
            if (!WorldEventMgr.Loaded)
            {
                ContentMgr.Load<WorldEvent>();
                ContentMgr.Load<WorldEventNpcData>();
                ContentMgr.Load<WorldEventQuest>();
                WorldEventMgr.Loaded = true;
                WorldEventMgr.LastUpdateTime = DateTime.Now;
                WorldEventMgr.Log.Debug("{0} World Events loaded.", WorldEventMgr._eventCount);
                World.TaskQueue.CallPeriodically(10000, new Action(WorldEventMgr.Update));
            }

            return true;
        }

        public static bool UnloadAll()
        {
            if (!WorldEventMgr.Loaded)
                return false;
            WorldEventMgr.Loaded = false;
            WorldEventMgr.AllEvents = new WorldEvent[100];
            WorldEventMgr.ActiveEvents = new WorldEvent[100];
            return true;
        }

        public static bool Reload()
        {
            if (WorldEventMgr.UnloadAll())
                return WorldEventMgr.LoadAll();
            return false;
        }

        public static void Update()
        {
            if (!WorldEventMgr.Loaded || !QuestMgr.Loaded || (!NPCMgr.Loaded || !GOMgr.Loaded))
                return;
            TimeSpan timeSpan1 = DateTime.Now - WorldEventMgr.LastUpdateTime;
            WorldEventMgr.LastUpdateTime = DateTime.Now;
            foreach (WorldEvent worldEvent1 in ((IEnumerable<WorldEvent>) WorldEventMgr.AllEvents)
                .Where<WorldEvent>((Func<WorldEvent, bool>) (worldEvent => worldEvent != null))
                .Where<WorldEvent>((Func<WorldEvent, bool>) (worldEvent => worldEvent.TimeUntilNextStart.HasValue)))
            {
                WorldEvent worldEvent2 = worldEvent1;
                TimeSpan? timeUntilNextStart1 = worldEvent2.TimeUntilNextStart;
                TimeSpan timeSpan2 = timeSpan1;
                worldEvent2.TimeUntilNextStart = timeUntilNextStart1.HasValue
                    ? new TimeSpan?(timeUntilNextStart1.GetValueOrDefault() - timeSpan2)
                    : new TimeSpan?();
                WorldEvent worldEvent3 = worldEvent1;
                TimeSpan? timeUntilEnd1 = worldEvent3.TimeUntilEnd;
                TimeSpan timeSpan3 = timeSpan1;
                worldEvent3.TimeUntilEnd = timeUntilEnd1.HasValue
                    ? new TimeSpan?(timeUntilEnd1.GetValueOrDefault() - timeSpan3)
                    : new TimeSpan?();
                TimeSpan? timeUntilEnd2 = worldEvent1.TimeUntilEnd;
                TimeSpan zero1 = TimeSpan.Zero;
                if ((timeUntilEnd2.HasValue ? (timeUntilEnd2.GetValueOrDefault() <= zero1 ? 1 : 0) : 0) != 0)
                {
                    WorldEventMgr.StopEvent(worldEvent1);
                }
                else
                {
                    TimeSpan? timeUntilNextStart2 = worldEvent1.TimeUntilNextStart;
                    TimeSpan zero2 = TimeSpan.Zero;
                    if ((timeUntilNextStart2.HasValue
                            ? (timeUntilNextStart2.GetValueOrDefault() <= zero2 ? 1 : 0)
                            : 0) != 0)
                        WorldEventMgr.StartEvent(worldEvent1);
                }
            }
        }

        public static WorldEvent GetEvent(uint id)
        {
            return WorldEventMgr.AllEvents.Get<WorldEvent>(id);
        }

        public static void AddEvent(WorldEvent worldEvent)
        {
            if (WorldEventMgr.AllEvents.Get<WorldEvent>(worldEvent.Id) == null)
                ++WorldEventMgr._eventCount;
            ArrayUtil.Set<WorldEvent>(ref WorldEventMgr.AllEvents, worldEvent.Id, worldEvent);
        }

        public static bool StartEvent(uint id)
        {
            WorldEvent worldEvent = WorldEventMgr.GetEvent(id);
            if (worldEvent == null)
                return false;
            WorldEventMgr.StartEvent(worldEvent);
            return true;
        }

        public static void StartEvent(WorldEvent worldEvent)
        {
            WorldEvent worldEvent1 = worldEvent;
            TimeSpan? timeUntilNextStart = worldEvent1.TimeUntilNextStart;
            TimeSpan occurence = worldEvent.Occurence;
            worldEvent1.TimeUntilNextStart = timeUntilNextStart.HasValue
                ? new TimeSpan?(timeUntilNextStart.GetValueOrDefault() + occurence)
                : new TimeSpan?();
            if (WorldEventMgr.IsEventActive(worldEvent.Id))
                return;
            WorldEventMgr.Log.Info("Starting event {0}: {1}", (object) worldEvent.Id, (object) worldEvent.Description);
            ArrayUtil.Set<WorldEvent>(ref WorldEventMgr.ActiveEvents, worldEvent.Id, worldEvent);
            WorldEventMgr.SpawnEvent(worldEvent);
            WorldEventMgr.ApplyEventNPCData(worldEvent);
        }

        public static bool StopEvent(uint id)
        {
            WorldEvent worldEvent = WorldEventMgr.GetEvent(id);
            if (worldEvent == null)
                return false;
            WorldEventMgr.StopEvent(worldEvent);
            return true;
        }

        public static void StopEvent(WorldEvent worldEvent)
        {
            WorldEvent worldEvent1 = worldEvent;
            TimeSpan? timeUntilEnd = worldEvent1.TimeUntilEnd;
            TimeSpan timeSpan = worldEvent.Occurence + worldEvent.Duration;
            worldEvent1.TimeUntilEnd = timeUntilEnd.HasValue
                ? new TimeSpan?(timeUntilEnd.GetValueOrDefault() + timeSpan)
                : new TimeSpan?();
            if (!WorldEventMgr.IsEventActive(worldEvent.Id))
                return;
            WorldEventMgr.Log.Info("Stopping event {0}: {1}", (object) worldEvent.Id, (object) worldEvent.Description);
            WorldEventMgr.ActiveEvents[worldEvent.Id] = (WorldEvent) null;
            if (worldEvent.QuestIds.Count != 0)
                WorldEventMgr.ClearActiveQuests((IEnumerable<uint>) worldEvent.QuestIds);
            WorldEventMgr.DeSpawnEvent(worldEvent);
            WorldEventMgr.ResetEventNPCData(worldEvent);
        }

        private static void ClearActiveQuests(IEnumerable<uint> questIds)
        {
            World.CallOnAllChars((Action<Character>) (chr =>
            {
                foreach (uint questId in questIds)
                {
                    Quest questById = chr.QuestLog.GetQuestById(questId);
                    if (questById != null)
                        questById.Cancel(false);
                }
            }));
        }

        public static void SpawnEvent(uint eventId)
        {
            WorldEventMgr.SpawnEvent(WorldEventMgr.GetEvent(eventId));
        }

        public static void DeSpawnEvent(uint eventId)
        {
            WorldEventMgr.DeSpawnEvent(WorldEventMgr.GetEvent(eventId));
        }

        private static void SpawnEvent(WorldEvent worldEvent)
        {
            foreach (WorldEventNPC npcSpawn in worldEvent.NPCSpawns)
            {
                NPCSpawnEntry spawnEntry = NPCMgr.GetSpawnEntry(npcSpawn.Guid);
                Map map = spawnEntry.Map;
                if (map != null)
                {
                    if (npcSpawn.Spawn)
                    {
                        map.AddNPCSpawnPool(spawnEntry.PoolTemplate);
                    }
                    else
                    {
                        foreach (SpawnPoint<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>
                            spawnPoint in spawnEntry.SpawnPoints.ToArray())
                            spawnPoint.Disable();
                    }
                }
            }

            foreach (WorldEventGameObject goSpawn in worldEvent.GOSpawns)
            {
                GOSpawnEntry spawnEntry = GOMgr.GetSpawnEntry(goSpawn.Guid);
                Map map = spawnEntry.Map;
                if (map != null)
                {
                    if (goSpawn.Spawn)
                    {
                        map.AddGOSpawnPoolLater(spawnEntry.PoolTemplate);
                    }
                    else
                    {
                        foreach (SpawnPoint<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>
                            spawnPoint in spawnEntry.SpawnPoints.ToArray())
                            spawnPoint.Disable();
                    }
                }
            }
        }

        private static void DeSpawnEvent(WorldEvent worldEvent)
        {
            foreach (WorldEventNPC npcSpawn in worldEvent.NPCSpawns)
            {
                NPCSpawnEntry spawnEntry = NPCMgr.GetSpawnEntry(npcSpawn.Guid);
                Map map = spawnEntry.Map;
                if (map != null)
                {
                    if (npcSpawn.Spawn)
                    {
                        map.RemoveNPCSpawnPool(spawnEntry.PoolTemplate);
                    }
                    else
                    {
                        foreach (SpawnPoint<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>
                            spawnPoint in spawnEntry.SpawnPoints.ToArray())
                            spawnPoint.Respawn();
                    }
                }
            }

            foreach (WorldEventGameObject goSpawn in worldEvent.GOSpawns)
            {
                GOSpawnEntry spawnEntry = GOMgr.GetSpawnEntry(goSpawn.Guid);
                Map map = spawnEntry.Map;
                if (map != null)
                {
                    if (goSpawn.Spawn)
                    {
                        map.RemoveGOSpawnPool(spawnEntry.PoolTemplate);
                    }
                    else
                    {
                        foreach (SpawnPoint<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>
                            spawnPoint in spawnEntry.SpawnPoints.ToArray())
                            spawnPoint.Respawn();
                    }
                }
            }
        }

        private static void ApplyEventNPCData(WorldEvent worldEvent)
        {
            foreach (WorldEventNpcData modelEquip in worldEvent.ModelEquips)
            {
                NPCSpawnEntry spawnEntry = NPCMgr.GetSpawnEntry(modelEquip.Guid);
                if (spawnEntry == null)
                {
                    WorldEventMgr.Log.Warn("Invalid Spawn Entry in World Event NPC Data, Entry: {0}", modelEquip.Guid);
                }
                else
                {
                    if (modelEquip.EntryId != (NPCId) 0)
                    {
                        modelEquip.OriginalEntryId = spawnEntry.EntryId;
                        spawnEntry.EntryId = modelEquip.EntryId;
                        spawnEntry.Entry = NPCMgr.GetEntry(spawnEntry.EntryId);
                        if (spawnEntry.Entry == null)
                        {
                            WorldEventMgr.Log.Warn("{0} had an invalid World Event EntryId.", (object) spawnEntry);
                            spawnEntry.EntryId = modelEquip.OriginalEntryId;
                            spawnEntry.Entry = NPCMgr.GetEntry(spawnEntry.EntryId);
                        }
                    }

                    if (modelEquip.ModelId != 0U)
                        spawnEntry.DisplayIdOverride = modelEquip.ModelId;
                    if (modelEquip.EquipmentId != 0U)
                    {
                        modelEquip.OriginalEquipmentId = spawnEntry.EquipmentId;
                        spawnEntry.EquipmentId = modelEquip.EquipmentId;
                        spawnEntry.Equipment = NPCMgr.GetEquipment(spawnEntry.EquipmentId);
                    }

                    foreach (NPCSpawnPoint npcSpawnPoint in
                        ((IEnumerable<NPCSpawnPoint>) spawnEntry.SpawnPoints.ToArray()).Where<NPCSpawnPoint>(
                            (Func<NPCSpawnPoint, bool>) (point => point.IsActive)))
                    {
                        npcSpawnPoint.Respawn();
                        if (modelEquip.SpellIdToCastAtStart != SpellId.None)
                        {
                            Spell spell = SpellHandler.Get(modelEquip.SpellIdToCastAtStart);
                            if (spell != null)
                            {
                                int num = (int) npcSpawnPoint.ActiveSpawnling.SpellCast.Start(spell);
                            }
                        }
                    }
                }
            }
        }

        private static void ResetEventNPCData(WorldEvent worldEvent)
        {
            foreach (WorldEventNpcData modelEquip in worldEvent.ModelEquips)
            {
                NPCSpawnEntry spawnEntry = NPCMgr.GetSpawnEntry(modelEquip.Guid);
                if (spawnEntry == null)
                {
                    WorldEventMgr.Log.Warn("Invalid Spawn Entry in World Event NPC Data, Entry: {0}", modelEquip.Guid);
                }
                else
                {
                    if (modelEquip.EntryId != (NPCId) 0)
                    {
                        spawnEntry.EntryId = modelEquip.OriginalEntryId;
                        spawnEntry.Entry = NPCMgr.GetEntry(spawnEntry.EntryId);
                    }

                    if (modelEquip.ModelId != 0U)
                        spawnEntry.DisplayIdOverride = 0U;
                    if (modelEquip.EquipmentId != 0U)
                    {
                        spawnEntry.EquipmentId = modelEquip.OriginalEquipmentId;
                        spawnEntry.Equipment = NPCMgr.GetEquipment(spawnEntry.EquipmentId);
                    }

                    foreach (NPCSpawnPoint npcSpawnPoint in
                        ((IEnumerable<NPCSpawnPoint>) spawnEntry.SpawnPoints.ToArray()).Where<NPCSpawnPoint>(
                            (Func<NPCSpawnPoint, bool>) (point => point.IsActive)))
                    {
                        npcSpawnPoint.Respawn();
                        if (modelEquip.SpellIdToCastAtEnd != SpellId.None)
                        {
                            Spell spell = SpellHandler.Get(modelEquip.SpellIdToCastAtEnd);
                            if (spell != null)
                            {
                                int num = (int) npcSpawnPoint.ActiveSpawnling.SpellCast.Start(spell);
                            }
                        }
                    }
                }
            }
        }

        public static bool IsHolidayActive(uint id)
        {
            if (id != 0U)
                return ((IEnumerable<WorldEvent>) WorldEventMgr.ActiveEvents).Any<WorldEvent>(
                    (Func<WorldEvent, bool>) (evnt =>
                    {
                        if (evnt != null)
                            return (int) evnt.HolidayId == (int) id;
                        return false;
                    }));
            return false;
        }

        public static bool IsEventActive(uint id)
        {
            if (id != 0U)
                return WorldEventMgr.ActiveEvents.Get<WorldEvent>(id) != null;
            return true;
        }

        public static IEnumerable<WorldEvent> GetActiveEvents()
        {
            return ((IEnumerable<WorldEvent>) WorldEventMgr.ActiveEvents).Where<WorldEvent>(
                (Func<WorldEvent, bool>) (evt => evt != null));
        }
    }
}