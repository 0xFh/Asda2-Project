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
    [NotVariable]public static List<WorldEventQuest> WorldEventQuests = new List<WorldEventQuest>();
    internal static uint _eventCount;
    private static DateTime LastUpdateTime;

    public static uint EventCount
    {
      get { return _eventCount; }
    }

    public static bool Loaded { get; private set; }

    public static void Initialize()
    {
      LoadAll();
    }

    /// <summary>Loads the world events.</summary>
    /// <returns></returns>
    public static bool LoadAll()
    {
      if(!Loaded)
      {
        ContentMgr.Load<WorldEvent>();
        ContentMgr.Load<WorldEventNpcData>();
        ContentMgr.Load<WorldEventQuest>();
        Loaded = true;
        LastUpdateTime = DateTime.Now;
        Log.Debug("{0} World Events loaded.", _eventCount);
        World.TaskQueue.CallPeriodically(10000, Update);
      }

      return true;
    }

    public static bool UnloadAll()
    {
      if(!Loaded)
        return false;
      Loaded = false;
      AllEvents = new WorldEvent[100];
      ActiveEvents = new WorldEvent[100];
      return true;
    }

    public static bool Reload()
    {
      if(UnloadAll())
        return LoadAll();
      return false;
    }

    public static void Update()
    {
      if(!Loaded || !QuestMgr.Loaded || (!NPCMgr.Loaded || !GOMgr.Loaded))
        return;
      TimeSpan timeSpan1 = DateTime.Now - LastUpdateTime;
      LastUpdateTime = DateTime.Now;
      foreach(WorldEvent worldEvent1 in AllEvents
        .Where(worldEvent => worldEvent != null)
        .Where(worldEvent => worldEvent.TimeUntilNextStart.HasValue))
      {
        WorldEvent worldEvent2 = worldEvent1;
        TimeSpan? timeUntilNextStart1 = worldEvent2.TimeUntilNextStart;
        TimeSpan timeSpan2 = timeSpan1;
        worldEvent2.TimeUntilNextStart = timeUntilNextStart1.HasValue
          ? timeUntilNextStart1.GetValueOrDefault() - timeSpan2
          : new TimeSpan?();
        WorldEvent worldEvent3 = worldEvent1;
        TimeSpan? timeUntilEnd1 = worldEvent3.TimeUntilEnd;
        TimeSpan timeSpan3 = timeSpan1;
        worldEvent3.TimeUntilEnd = timeUntilEnd1.HasValue
          ? timeUntilEnd1.GetValueOrDefault() - timeSpan3
          : new TimeSpan?();
        TimeSpan? timeUntilEnd2 = worldEvent1.TimeUntilEnd;
        TimeSpan zero1 = TimeSpan.Zero;
        if((timeUntilEnd2.HasValue ? (timeUntilEnd2.GetValueOrDefault() <= zero1 ? 1 : 0) : 0) != 0)
        {
          StopEvent(worldEvent1);
        }
        else
        {
          TimeSpan? timeUntilNextStart2 = worldEvent1.TimeUntilNextStart;
          TimeSpan zero2 = TimeSpan.Zero;
          if((timeUntilNextStart2.HasValue
               ? (timeUntilNextStart2.GetValueOrDefault() <= zero2 ? 1 : 0)
               : 0) != 0)
            StartEvent(worldEvent1);
        }
      }
    }

    public static WorldEvent GetEvent(uint id)
    {
      return AllEvents.Get(id);
    }

    public static void AddEvent(WorldEvent worldEvent)
    {
      if(AllEvents.Get(worldEvent.Id) == null)
        ++_eventCount;
      ArrayUtil.Set(ref AllEvents, worldEvent.Id, worldEvent);
    }

    public static bool StartEvent(uint id)
    {
      WorldEvent worldEvent = GetEvent(id);
      if(worldEvent == null)
        return false;
      StartEvent(worldEvent);
      return true;
    }

    public static void StartEvent(WorldEvent worldEvent)
    {
      WorldEvent worldEvent1 = worldEvent;
      TimeSpan? timeUntilNextStart = worldEvent1.TimeUntilNextStart;
      TimeSpan occurence = worldEvent.Occurence;
      worldEvent1.TimeUntilNextStart = timeUntilNextStart.HasValue
        ? timeUntilNextStart.GetValueOrDefault() + occurence
        : new TimeSpan?();
      if(IsEventActive(worldEvent.Id))
        return;
      Log.Info("Starting event {0}: {1}", worldEvent.Id, worldEvent.Description);
      ArrayUtil.Set(ref ActiveEvents, worldEvent.Id, worldEvent);
      SpawnEvent(worldEvent);
      ApplyEventNPCData(worldEvent);
    }

    public static bool StopEvent(uint id)
    {
      WorldEvent worldEvent = GetEvent(id);
      if(worldEvent == null)
        return false;
      StopEvent(worldEvent);
      return true;
    }

    public static void StopEvent(WorldEvent worldEvent)
    {
      WorldEvent worldEvent1 = worldEvent;
      TimeSpan? timeUntilEnd = worldEvent1.TimeUntilEnd;
      TimeSpan timeSpan = worldEvent.Occurence + worldEvent.Duration;
      worldEvent1.TimeUntilEnd = timeUntilEnd.HasValue
        ? timeUntilEnd.GetValueOrDefault() + timeSpan
        : new TimeSpan?();
      if(!IsEventActive(worldEvent.Id))
        return;
      Log.Info("Stopping event {0}: {1}", worldEvent.Id, worldEvent.Description);
      ActiveEvents[worldEvent.Id] = null;
      if(worldEvent.QuestIds.Count != 0)
        ClearActiveQuests(worldEvent.QuestIds);
      DeSpawnEvent(worldEvent);
      ResetEventNPCData(worldEvent);
    }

    private static void ClearActiveQuests(IEnumerable<uint> questIds)
    {
      World.CallOnAllChars(chr =>
      {
        foreach(uint questId in questIds)
        {
          Quest questById = chr.QuestLog.GetQuestById(questId);
          if(questById != null)
            questById.Cancel(false);
        }
      });
    }

    public static void SpawnEvent(uint eventId)
    {
      SpawnEvent(GetEvent(eventId));
    }

    public static void DeSpawnEvent(uint eventId)
    {
      DeSpawnEvent(GetEvent(eventId));
    }

    private static void SpawnEvent(WorldEvent worldEvent)
    {
      foreach(WorldEventNPC npcSpawn in worldEvent.NPCSpawns)
      {
        NPCSpawnEntry spawnEntry = NPCMgr.GetSpawnEntry(npcSpawn.Guid);
        Map map = spawnEntry.Map;
        if(map != null)
        {
          if(npcSpawn.Spawn)
          {
            map.AddNPCSpawnPool(spawnEntry.PoolTemplate);
          }
          else
          {
            foreach(SpawnPoint<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>
              spawnPoint in spawnEntry.SpawnPoints.ToArray())
              spawnPoint.Disable();
          }
        }
      }

      foreach(WorldEventGameObject goSpawn in worldEvent.GOSpawns)
      {
        GOSpawnEntry spawnEntry = GOMgr.GetSpawnEntry(goSpawn.Guid);
        Map map = spawnEntry.Map;
        if(map != null)
        {
          if(goSpawn.Spawn)
          {
            map.AddGOSpawnPoolLater(spawnEntry.PoolTemplate);
          }
          else
          {
            foreach(SpawnPoint<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>
              spawnPoint in spawnEntry.SpawnPoints.ToArray())
              spawnPoint.Disable();
          }
        }
      }
    }

    private static void DeSpawnEvent(WorldEvent worldEvent)
    {
      foreach(WorldEventNPC npcSpawn in worldEvent.NPCSpawns)
      {
        NPCSpawnEntry spawnEntry = NPCMgr.GetSpawnEntry(npcSpawn.Guid);
        Map map = spawnEntry.Map;
        if(map != null)
        {
          if(npcSpawn.Spawn)
          {
            map.RemoveNPCSpawnPool(spawnEntry.PoolTemplate);
          }
          else
          {
            foreach(SpawnPoint<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>
              spawnPoint in spawnEntry.SpawnPoints.ToArray())
              spawnPoint.Respawn();
          }
        }
      }

      foreach(WorldEventGameObject goSpawn in worldEvent.GOSpawns)
      {
        GOSpawnEntry spawnEntry = GOMgr.GetSpawnEntry(goSpawn.Guid);
        Map map = spawnEntry.Map;
        if(map != null)
        {
          if(goSpawn.Spawn)
          {
            map.RemoveGOSpawnPool(spawnEntry.PoolTemplate);
          }
          else
          {
            foreach(SpawnPoint<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>
              spawnPoint in spawnEntry.SpawnPoints.ToArray())
              spawnPoint.Respawn();
          }
        }
      }
    }

    private static void ApplyEventNPCData(WorldEvent worldEvent)
    {
      foreach(WorldEventNpcData modelEquip in worldEvent.ModelEquips)
      {
        NPCSpawnEntry spawnEntry = NPCMgr.GetSpawnEntry(modelEquip.Guid);
        if(spawnEntry == null)
        {
          Log.Warn("Invalid Spawn Entry in World Event NPC Data, Entry: {0}", modelEquip.Guid);
        }
        else
        {
          if(modelEquip.EntryId != 0)
          {
            modelEquip.OriginalEntryId = spawnEntry.EntryId;
            spawnEntry.EntryId = modelEquip.EntryId;
            spawnEntry.Entry = NPCMgr.GetEntry(spawnEntry.EntryId);
            if(spawnEntry.Entry == null)
            {
              Log.Warn("{0} had an invalid World Event EntryId.", spawnEntry);
              spawnEntry.EntryId = modelEquip.OriginalEntryId;
              spawnEntry.Entry = NPCMgr.GetEntry(spawnEntry.EntryId);
            }
          }

          if(modelEquip.ModelId != 0U)
            spawnEntry.DisplayIdOverride = modelEquip.ModelId;
          if(modelEquip.EquipmentId != 0U)
          {
            modelEquip.OriginalEquipmentId = spawnEntry.EquipmentId;
            spawnEntry.EquipmentId = modelEquip.EquipmentId;
            spawnEntry.Equipment = NPCMgr.GetEquipment(spawnEntry.EquipmentId);
          }

          foreach(NPCSpawnPoint npcSpawnPoint in
            spawnEntry.SpawnPoints.ToArray().Where(
              point => point.IsActive))
          {
            npcSpawnPoint.Respawn();
            if(modelEquip.SpellIdToCastAtStart != SpellId.None)
            {
              Spell spell = SpellHandler.Get(modelEquip.SpellIdToCastAtStart);
              if(spell != null)
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
      foreach(WorldEventNpcData modelEquip in worldEvent.ModelEquips)
      {
        NPCSpawnEntry spawnEntry = NPCMgr.GetSpawnEntry(modelEquip.Guid);
        if(spawnEntry == null)
        {
          Log.Warn("Invalid Spawn Entry in World Event NPC Data, Entry: {0}", modelEquip.Guid);
        }
        else
        {
          if(modelEquip.EntryId != 0)
          {
            spawnEntry.EntryId = modelEquip.OriginalEntryId;
            spawnEntry.Entry = NPCMgr.GetEntry(spawnEntry.EntryId);
          }

          if(modelEquip.ModelId != 0U)
            spawnEntry.DisplayIdOverride = 0U;
          if(modelEquip.EquipmentId != 0U)
          {
            spawnEntry.EquipmentId = modelEquip.OriginalEquipmentId;
            spawnEntry.Equipment = NPCMgr.GetEquipment(spawnEntry.EquipmentId);
          }

          foreach(NPCSpawnPoint npcSpawnPoint in
            spawnEntry.SpawnPoints.ToArray().Where(
              point => point.IsActive))
          {
            npcSpawnPoint.Respawn();
            if(modelEquip.SpellIdToCastAtEnd != SpellId.None)
            {
              Spell spell = SpellHandler.Get(modelEquip.SpellIdToCastAtEnd);
              if(spell != null)
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
      if(id != 0U)
        return ActiveEvents.Any(
          evnt =>
          {
            if(evnt != null)
              return (int) evnt.HolidayId == (int) id;
            return false;
          });
      return false;
    }

    public static bool IsEventActive(uint id)
    {
      if(id != 0U)
        return ActiveEvents.Get(id) != null;
      return true;
    }

    public static IEnumerable<WorldEvent> GetActiveEvents()
    {
      return ActiveEvents.Where(
        evt => evt != null);
    }
  }
}