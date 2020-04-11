using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs.Auctioneer;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.NPCs.Trainers;
using WCell.RealmServer.NPCs.Vendors;
using WCell.RealmServer.Spawns;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Taxi;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.NPCs
{
  /// <summary>
  /// Static helper and srcCont class for all NPCs and NPC-related information
  /// </summary>
  [GlobalMgr]
  public static class NPCMgr
  {
    [Variable("NormalCorpseDecayDelayMillis")]
    public static int DecayDelayNormalMillis = 60000;

    [Variable("RareCorpseDecayDelayMillis")]
    public static int DecayDelayRareMillis = 300000;

    [Variable("EpicCorpseDecayDelayMillis")]
    public static int DecayDelayEpicMillis = 3600000;

    /// <summary>
    /// Can be used to toughen up or soften down NPCs on servers with more of a "diablo feel"
    /// or custom servers.
    /// </summary>
    public static float DefaultNPCHealthFactor = 1f;

    /// <summary>
    /// Can be used to toughen up or soften down NPCs on servers with more of a "diablo feel"
    /// or custom servers.
    /// </summary>
    public static float DefaultNPCDamageFactor = 1f;

    public static float DefaultNPCFlySpeed = 16f;
    public static float DefaultNPCRunSpeed = 8f;
    public static float DefaultNPCWalkSpeed = 2f;
    public static int DefaultInteractionDistance = 15;

    public static int DefaultInteractionDistanceSq =
      DefaultInteractionDistance * DefaultInteractionDistance;

    /// <summary>
    /// Default amount of NPCs to spawn if at one spawn point, if not specified otherwise
    /// </summary>
    public static uint DefaultSpawnAmount = 1;

    /// <summary>
    /// Default min delay in milliseconds after death of a unit to spawn a new one
    /// </summary>
    public static int DefaultMinRespawnDelay = 180000;

    /// <summary>
    /// Default max delay in milliseconds after death of a unit to spawn a new one
    /// </summary>
    public static uint DefaultMaxRespawnDelay = 300000;

    [NotVariable]public static float DefaultMaxHomeDistanceInCombatSq = 900f;

    /// <summary>
    /// If a mob is in combat and is further away from his home spot than DefaultMaxHomeDistanceInCombat and there was no combat
    /// for this, it will start to evade (in millis).
    /// </summary>
    public static uint GiveUpCombatDelay = 6000;

    /// <summary>
    /// The delay between the time the NPC finished fighting (everyone killed) and
    /// evading (in millis).
    /// </summary>
    public static uint CombatEvadeDelay = 2000;

    [NotVariable]internal static readonly NPCEntry[] Entries = new NPCEntry[844];

    /// <summary>
    /// Custom entries are put in a Dictionary, so that the Entries array won't explode
    /// </summary>
    internal static readonly Dictionary<uint, NPCEntry> CustomEntries = new Dictionary<uint, NPCEntry>();

    /// <summary>All Vehicle Entries</summary>
    [NotVariable]public static Dictionary<int, VehicleEntry> VehicleEntries = new Dictionary<int, VehicleEntry>();

    /// <summary>All VehicleSeat Enteries</summary>
    [NotVariable]
    public static Dictionary<int, VehicleSeatEntry> VehicleSeatEntries = new Dictionary<int, VehicleSeatEntry>();

    /// <summary>All barber shop style entries.</summary>
    [NotVariable]public static Dictionary<int, BarberShopStyleEntry> BarberShopStyles =
      new Dictionary<int, BarberShopStyleEntry>();

    /// <summary>Entries of equipment to be added to NPCs</summary>
    [NotVariable]public static NPCEquipmentEntry[] EquipmentEntries = new NPCEquipmentEntry[2000];

    [NotVariable]public static Dictionary<int, CreatureFamily> CreatureFamilies = new Dictionary<int, CreatureFamily>();

    /// <summary>All existing Mounts by their corresponding EntryId</summary>
    public static readonly Dictionary<MountId, NPCEntry> Mounts = new Dictionary<MountId, NPCEntry>(100);

    [NotVariable]internal static Spell[][] PetSpells = new Spell[0][];

    /// <summary>All templates for spawn pools</summary>
    public static readonly Dictionary<uint, NPCSpawnPoolTemplate> SpawnPoolTemplates =
      new Dictionary<uint, NPCSpawnPoolTemplate>();

    [NotVariable]public static NPCSpawnEntry[] SpawnEntries = new NPCSpawnEntry[40000];
    [NotVariable]public static List<NPCSpawnPoolTemplate>[] SpawnPoolsByMap = new List<NPCSpawnPoolTemplate>[727];

    /// <summary>NPCTypeHandlers indexed by set bits of NPCFlags.</summary>
    public static readonly NPCTypeHandler[] NPCTypeHandlers = new NPCTypeHandler[32];

    /// <summary>NPCSpawnTypeHandlers indexed by set bits of NPCFlags.</summary>
    public static readonly NPCSpawnTypeHandler[] NPCSpawnTypeHandlers = new NPCSpawnTypeHandler[32];

    /// <summary>Trainer spell entries by trainer spell template id</summary>
    public static Dictionary<uint, List<TrainerSpellEntry>> TrainerSpellTemplates =
      new Dictionary<uint, List<TrainerSpellEntry>>();

    public static Dictionary<uint, List<VendorItemEntry>> VendorLists =
      new Dictionary<uint, List<VendorItemEntry>>(5000);

    private static FactionId defaultFactionId;
    private static int s_lastNPCUID;
    [NotVariable]public static uint[] BankBagSlotPrices;
    private static bool entriesLoaded;
    private static bool spawnsLoaded;
    public static Dictionary<int, ItemExtendedCostEntry> ItemExtendedCostEntries;

    /// <summary>
    /// This is the default FactionId for all NPCs whose Faction could not be found/does not exist.
    /// </summary>
    public static FactionId DefaultFactionId
    {
      get { return defaultFactionId; }
      set
      {
        defaultFactionId = value;
        Faction faction = FactionMgr.Get(value);
        if(faction == null)
          return;
        DefaultFaction = faction;
      }
    }

    /// <summary>
    /// This is the default faction for all NPCs whose Faction could not be found/does not exist.
    /// </summary>
    public static Faction DefaultFaction { get; internal set; }

    public static CreatureFamily DefaultFamily { get; set; }

    /// <summary>
    /// If a mob is in combat and is further away from his home spot than this and there was no combat
    /// for DefaultCombatRetreatDelay, it will start to retreat.
    /// Don't use this in code other than for informational reasons.
    /// </summary>
    public static float DefaultMaxHomeDistanceInCombat
    {
      get { return (float) Math.Sqrt(DefaultMaxHomeDistanceInCombatSq); }
      set { DefaultMaxHomeDistanceInCombatSq = value * value; }
    }

    public static int EntryCount { get; internal set; }

    public static IEnumerable<NPCEntry> GetAllEntries()
    {
      return new EntryIterator();
    }

    public static NPCEntry GetEntry(uint id, uint difficultyIndex)
    {
      return GetEntry(id)?.GetEntry(difficultyIndex);
    }

    public static NPCEntry GetEntry(uint id)
    {
      if(id < Entries.Length)
        return Entries[id];
      NPCEntry npcEntry;
      CustomEntries.TryGetValue(id, out npcEntry);
      return npcEntry;
    }

    public static NPCEntry GetEntry(NPCId id, uint difficultyIndex)
    {
      return GetEntry(id)?.GetEntry(difficultyIndex);
    }

    public static NPCEntry GetEntry(NPCId id)
    {
      if(id < (NPCId) Entries.Length)
        return Entries[(uint) id];
      NPCEntry npcEntry;
      CustomEntries.TryGetValue((uint) id, out npcEntry);
      return npcEntry;
    }

    public static NPCSpawnEntry GetSpawnEntry(uint id)
    {
      if(id >= SpawnEntries.Length)
        return null;
      return SpawnEntries[id];
    }

    internal static NPCSpawnPoolTemplate GetOrCreateSpawnPoolTemplate(uint poolId)
    {
      NPCSpawnPoolTemplate spawnPoolTemplate;
      if(poolId == 0U)
      {
        spawnPoolTemplate = new NPCSpawnPoolTemplate();
        SpawnPoolTemplates.Add(spawnPoolTemplate.PoolId, spawnPoolTemplate);
      }
      else if(!SpawnPoolTemplates.TryGetValue(poolId, out spawnPoolTemplate))
      {
        SpawnPoolTemplateEntry poolTemplateEntry = SpawnMgr.GetSpawnPoolTemplateEntry(poolId);
        spawnPoolTemplate = poolTemplateEntry == null
          ? new NPCSpawnPoolTemplate()
          : new NPCSpawnPoolTemplate(poolTemplateEntry);
        SpawnPoolTemplates.Add(spawnPoolTemplate.PoolId, spawnPoolTemplate);
      }

      return spawnPoolTemplate;
    }

    public static List<NPCSpawnPoolTemplate> GetSpawnPoolTemplatesByMap(MapId map)
    {
      return SpawnPoolsByMap[(int) map];
    }

    internal static List<NPCSpawnPoolTemplate> GetOrCreateSpawnPoolTemplatesByMap(MapId map)
    {
      List<NPCSpawnPoolTemplate> spawnPoolTemplateList = SpawnPoolsByMap[(int) map];
      if(spawnPoolTemplateList == null)
        SpawnPoolsByMap[(int) map] = spawnPoolTemplateList = new List<NPCSpawnPoolTemplate>();
      return spawnPoolTemplateList;
    }

    public static NPCEquipmentEntry GetEquipment(uint equipId)
    {
      return EquipmentEntries.Get(equipId);
    }

    /// <summary>
    /// Creates a new custom NPCEntry with the given Id.
    /// The id must be hardcoded, so the client will always recognize it in its cache.
    /// </summary>
    public static void AddEntry<E>(uint id, E entry) where E : NPCEntry, new()
    {
      if(id < 844U)
        throw new ArgumentException("Cannot create an NPCEntry with id < NPCId.End (" + 844 + ")");
      entry.Id = id;
      CustomEntries.Add(id, entry);
      entry.FinalizeDataHolder();
    }

    /// <summary>
    /// Creates a new custom NPCEntry with the given Id.
    /// The id must be hardcoded, so the client will always recognize it in its cache.
    /// </summary>
    public static void AddEntry(uint id, NPCEntry entry)
    {
      if(id < 844U)
        throw new ArgumentException("Cannot create an NPCEntry with id < NPCId.End (" + 844 + ")");
      if(CustomEntries.ContainsKey(id))
        CustomEntries.Remove(id);
      entry.Id = id;
      CustomEntries.Add(id, entry);
      entry.FinalizeDataHolder();
    }

    internal static uint GenerateUniqueLowId()
    {
      return (uint) Interlocked.Increment(ref s_lastNPCUID);
    }

    internal static void GenerateId(NPC npc)
    {
      GenerateId(npc, HighId.Unit);
    }

    internal static void GenerateId(NPC npc, HighId highId)
    {
      npc.EntityId = new EntityId(GenerateUniqueLowId(), npc.EntryId, highId);
    }

    public static void Apply(this Action<NPCEntry> cb, params NPCId[] ids)
    {
      foreach(NPCId id in ids)
      {
        NPCEntry entry = GetEntry(id);
        if(entry != null)
          cb(entry);
      }
    }

    /// <summary>
    /// Returns all NPCTypeHandlers for the given NPCPrototype
    /// </summary>
    public static NPCTypeHandler[] GetNPCTypeHandlers(NPCEntry entry)
    {
      NPCTypeHandler[] npcTypeHandlerArray = new NPCTypeHandler[entry.SetFlagIndices.Length];
      for(int index = 0; index < npcTypeHandlerArray.Length; ++index)
        npcTypeHandlerArray[index] = NPCTypeHandlers[entry.SetFlagIndices[index]];
      return npcTypeHandlerArray;
    }

    /// <summary>Calls all NPCTypeHandlers of the given spawnEntry.</summary>
    internal static void CallNPCTypeHandlers(NPC npc)
    {
      foreach(NPCTypeHandler instanceTypeHandler in npc.Entry.InstanceTypeHandlers)
        instanceTypeHandler(npc);
    }

    /// <summary>
    /// Returns all NPCSpawnTypeHandlers for the given NPCPrototype
    /// </summary>
    internal static NPCSpawnTypeHandler[] GetNPCSpawnTypeHandlers(NPCEntry entry)
    {
      NPCSpawnTypeHandler[] spawnTypeHandlerArray = new NPCSpawnTypeHandler[entry.SetFlagIndices.Length];
      for(int index = 0; index < spawnTypeHandlerArray.Length; ++index)
        spawnTypeHandlerArray[index] = NPCSpawnTypeHandlers[entry.SetFlagIndices[index]];
      return spawnTypeHandlerArray;
    }

    [Initialization(InitializationPass.Fifth, "Initialize NPCs")]
    public static void Initialize()
    {
      DefaultFaction = new Faction
      {
        Id = FactionId.None
      };
      DefaultFamily = new CreatureFamily
      {
        Id = CreatureFamilyId.None
      };
      InitDefault();
      LoadNPCDefs(false);
    }

    public static void LoadAllLater()
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(() => LoadAll(false));
    }

    public static void LoadAll(bool force = false)
    {
      if(Loaded)
        return;
      LoadNPCDefs(force);
    }

    public static void InitDefault()
    {
      InitTypeHandlers();
    }

    /// <summary>Initializes NPCTypeHandlers</summary>
    private static void InitTypeHandlers()
    {
      NPCTypeHandlers[7] = OnNewVendor;
      NPCTypeHandlers[4] = OnNewTrainer;
      NPCTypeHandlers[16] = OnNewInnKeeper;
      NPCTypeHandlers[21] = OnNewAuctioneer;
    }

    public static bool Loaded
    {
      get
      {
        if(entriesLoaded)
          return spawnsLoaded;
        return false;
      }
    }

    public static bool SpawnsLoaded
    {
      get { return spawnsLoaded; }
      private set
      {
        spawnsLoaded = value;
        CheckLoaded();
      }
    }

    public static bool EntriesLoaded
    {
      get { return entriesLoaded; }
      private set
      {
        entriesLoaded = value;
        CheckLoaded();
      }
    }

    private static void CheckLoaded()
    {
      if(!Loaded)
        return;
      ServerApp<RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(NPCMgr));
    }

    public static bool Loading { get; private set; }

    public static void LoadNPCDefs(bool force = false)
    {
      LoadEntries(force);
      LoadSpawns(force);
    }

    public static void LoadEntries(bool force)
    {
      if(!force)
      {
        if(entriesLoaded)
          return;
      }

      try
      {
        Loading = true;
        FactionMgr.Initialize();
        ContentMgr.Load<NPCEntry>(force);
        EntriesLoaded = true;
      }
      finally
      {
        Loading = false;
      }
    }

    public static void LoadSpawns(bool force)
    {
      Loading = true;
      try
      {
        OnlyLoadSpawns(force);
        LoadWaypoints(force);
        if(!ServerApp<RealmServer>.Instance.IsRunning)
          return;
        for(MapId mapId = MapId.Silaris; mapId < MapId.End; ++mapId)
        {
          Map map = World.GetNonInstancedMap(mapId);
          if(map != null && map.NPCsSpawned)
          {
            List<NPCSpawnPoolTemplate> poolTemplatesByMap = GetSpawnPoolTemplatesByMap(mapId);
            if(poolTemplatesByMap != null)
            {
              foreach(NPCSpawnPoolTemplate spawnPoolTemplate in poolTemplatesByMap)
              {
                if(spawnPoolTemplate.AutoSpawns)
                {
                  NPCSpawnPoolTemplate p = spawnPoolTemplate;
                  map.ExecuteInContext(() => map.AddNPCSpawnPoolNow(p));
                }
              }
            }
          }
        }
      }
      finally
      {
        Loading = false;
      }
    }

    public static void OnlyLoadSpawns(bool force)
    {
      if(spawnsLoaded && !force)
        return;
      if(force)
      {
        SpawnPoolTemplates.Clear();
        foreach(List<NPCSpawnPoolTemplate> spawnPoolsBy in SpawnPoolsByMap)
        {
          if(spawnPoolsBy != null)
            spawnPoolsBy.Clear();
        }
      }

      ContentMgr.Load<NPCSpawnEntry>(force);
      SpawnsLoaded = true;
    }

    public static void LoadWaypoints(bool force)
    {
      ContentMgr.Load<WaypointEntry>(force);
    }

    public static CreatureFamily GetFamily(CreatureFamilyId id)
    {
      return DefaultFamily;
    }

    public static Spell[] GetSpells(NPCId id)
    {
      return PetSpells.Get((uint) id);
    }

    public static VehicleSeatEntry GetVehicleSeatEntry(uint id)
    {
      VehicleSeatEntry vehicleSeatEntry;
      VehicleSeatEntries.TryGetValue((int) id, out vehicleSeatEntry);
      return vehicleSeatEntry;
    }

    private static void LoadTrainers(bool force)
    {
      ContentMgr.Load<TrainerSpellEntry>(force);
    }

    private static void OnNewTrainer(NPC npc)
    {
      npc.NPCFlags &= ~NPCFlags.ClassTrainer;
      npc.NPCFlags |= NPCFlags.UnkTrainer | NPCFlags.ProfessionTrainer;
      npc.TrainerEntry = npc.Entry.TrainerEntry;
    }

    public static void TalkToTrainer(this NPC trainer, Character chr)
    {
      if(!trainer.CheckTrainerCanTrain(chr))
        return;
      chr.OnInteract(trainer);
      trainer.SendTrainerList(chr, trainer.TrainerEntry.Spells.Values,
        trainer.TrainerEntry.Message);
    }

    public static bool CanLearn(this Character chr, TrainerSpellEntry trainerSpell)
    {
      return trainerSpell.Spell != null && trainerSpell.GetTrainerSpellState(chr) == TrainerSpellState.Available;
    }

    public static void BuySpell(this NPC trainer, Character chr, SpellId spellEntryId)
    {
      if(!trainer.CheckTrainerCanTrain(chr))
        return;
      TrainerSpellEntry spellEntry = trainer.TrainerEntry.GetSpellEntry(spellEntryId);
      if(spellEntry == null || !trainer.CheckBuySpellConditions(chr, spellEntry))
        return;
      chr.SubtractMoney(spellEntry.GetDiscountedCost(chr, trainer));
      NPCHandler.SendTrainerBuySucceeded(chr.Client, trainer,
        spellEntry);
      SpellHandler.SendVisual(trainer, 179U);
      if(spellEntry.Spell.IsTeachSpell)
        trainer.SpellCast.Trigger(spellEntry.Spell, (WorldObject) chr);
      else if(chr.PowerType == PowerType.Mana || spellEntry.Spell.PreviousRank == null)
      {
        chr.Spells.AddSpell(spellEntry.Spell);
        trainer.TalkToTrainer(chr);
      }
      else
        chr.Spells.Replace(spellEntry.Spell.PreviousRank, spellEntry.Spell);
    }

    private static bool CheckBuySpellConditions(this NPC trainer, Character curChar, TrainerSpellEntry trainerSpell)
    {
      if(curChar.CanLearn(trainerSpell))
        return curChar.Money >= trainerSpell.GetDiscountedCost(curChar, trainer);
      return false;
    }

    private static bool CheckTrainerCanTrain(this NPC trainer, Character curChar)
    {
      if(!trainer.IsTrainer || !trainer.CheckVendorInteraction(curChar) || !trainer.CanTrain(curChar))
        return false;
      curChar.Auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);
      return true;
    }

    private static void LoadVendors(bool force)
    {
      LoadItemExtendedCostEntries();
      ContentMgr.Load<VendorItemEntry>(force);
    }

    private static void LoadItemExtendedCostEntries()
    {
      ItemExtendedCostEntries =
        new MappedDBCReader<ItemExtendedCostEntry, DBCItemExtendedCostConverter>(
          RealmServerConfiguration.GetDBCFile("ItemExtendedCost.dbc")).Entries;
      ItemExtendedCostEntries.Add(0, ItemExtendedCostEntry.NullEntry);
    }

    internal static List<VendorItemEntry> GetOrCreateVendorList(NPCId npcId)
    {
      return GetOrCreateVendorList((uint) npcId);
    }

    internal static List<VendorItemEntry> GetOrCreateVendorList(uint npcId)
    {
      List<VendorItemEntry> vendorItemEntryList;
      if(!VendorLists.TryGetValue(npcId, out vendorItemEntryList))
        VendorLists.Add(npcId, vendorItemEntryList = new List<VendorItemEntry>());
      return vendorItemEntryList;
    }

    private static void OnNewVendor(NPC npc)
    {
      npc.VendorEntry = new VendorEntry(npc, npc.Entry.VendorItems);
    }

    private static void OnNewInnKeeper(NPC npc)
    {
      NamedWorldZoneLocation worldZoneLocation = new NamedWorldZoneLocation
      {
        Position = npc.Position,
        MapId = npc.Map.Id
      };
      if(npc.Zone != null)
        worldZoneLocation.ZoneId = npc.Zone.Id;
      npc.BindPoint = worldZoneLocation;
    }

    private static void OnNewAuctioneer(NPC npc)
    {
      npc.AuctioneerEntry = new AuctioneerEntry(npc);
    }

    public static void TalkToPetitioner(this NPC petitioner, Character chr)
    {
      if(!petitioner.CheckTrainerCanTrain(chr))
        return;
      chr.OnInteract(petitioner);
      petitioner.SendPetitionList(chr);
    }

    /// <summary>
    /// Spawns the pool to which the NPCSpawnEntry belongs which is closest to the given location
    /// </summary>
    public static NPCSpawnPoint SpawnClosestSpawnEntry(IWorldLocation pos)
    {
      NPCSpawnEntry closestSpawnEntry = GetClosestSpawnEntry(pos);
      if(closestSpawnEntry != null)
      {
        NPCSpawnPool npcSpawnPool = pos.Map.AddNPCSpawnPoolNow(closestSpawnEntry.PoolTemplate);
        if(npcSpawnPool != null)
          return npcSpawnPool.GetSpawnPoint(closestSpawnEntry);
      }

      return null;
    }

    public static NPCSpawnEntry GetClosestSpawnEntry(IWorldLocation pos)
    {
      NPCSpawnEntry npcSpawnEntry = null;
      float num1 = float.MaxValue;
      foreach(SpawnPoolTemplate<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>
        spawnPoolTemplate in SpawnPoolsByMap[(int) pos.MapId])
      {
        foreach(NPCSpawnEntry entry in spawnPoolTemplate.Entries)
        {
          if((int) entry.Phase == (int) pos.Phase)
          {
            float num2 = pos.Position.DistanceSquared(entry.Position);
            if(num2 < (double) num1)
            {
              num1 = num2;
              npcSpawnEntry = entry;
            }
          }
        }
      }

      return npcSpawnEntry;
    }

    public static NPCSpawnEntry GetClosestSpawnEntry(this IEnumerable<NPCSpawnEntry> entries, IWorldLocation pos)
    {
      NPCSpawnEntry npcSpawnEntry = null;
      float num1 = float.MaxValue;
      foreach(NPCSpawnEntry entry in entries)
      {
        if((int) entry.Phase == (int) pos.Phase)
        {
          float num2 = pos.Position.DistanceSquared(entry.Position);
          if(num2 < (double) num1)
          {
            num1 = num2;
            npcSpawnEntry = entry;
          }
        }
      }

      return npcSpawnEntry;
    }

    /// <summary>
    /// Checks which nodes are currently activated by the player and sends the results to the client.
    /// </summary>
    public static void TalkToFM(this NPC taxiVendor, Character chr)
    {
      if(!taxiVendor.CheckVendorInteraction(chr))
        return;
      PathNode vendorTaxiNode = taxiVendor.VendorTaxiNode;
      if(vendorTaxiNode == null)
        return;
      TaxiNodeMask taxiNodes = chr.TaxiNodes;
      if(!taxiNodes.IsActive(vendorTaxiNode))
      {
        if(chr.GodMode)
        {
          chr.ActivateAllTaxiNodes();
        }
        else
        {
          taxiNodes.Activate(vendorTaxiNode);
          TaxiHandler.SendTaxiPathActivated(chr.Client);
          TaxiHandler.SendTaxiPathUpdate(chr.Client, taxiVendor.EntityId, true);
          return;
        }
      }

      chr.OnInteract(taxiVendor);
      TaxiHandler.ShowTaxiList(chr, taxiVendor, vendorTaxiNode);
    }

    internal class EntryIterator : IEnumerable<NPCEntry>, IEnumerable
    {
      public IEnumerator<NPCEntry> GetEnumerator()
      {
        return new EntryEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }
    }

    private class EntryEnumerator : IEnumerator<NPCEntry>, IDisposable, IEnumerator
    {
      private IEnumerator currentIterator;
      private bool custom;

      public EntryEnumerator()
      {
        Reset();
      }

      public bool MoveNext()
      {
        if(!custom)
        {
          while(currentIterator.MoveNext())
          {
            if(currentIterator.Current != null)
              return true;
          }

          currentIterator = CustomEntries.Values.GetEnumerator();
          custom = true;
        }

        return currentIterator.MoveNext();
      }

      public void Reset()
      {
        currentIterator = Entries.GetEnumerator();
      }

      public void Dispose()
      {
        if(!(currentIterator is IEnumerator<NPCEntry>))
          return;
        ((IDisposable) currentIterator).Dispose();
      }

      public NPCEntry Current
      {
        get { return (NPCEntry) currentIterator.Current; }
      }

      object IEnumerator.Current
      {
        get { return Current; }
      }
    }
  }
}