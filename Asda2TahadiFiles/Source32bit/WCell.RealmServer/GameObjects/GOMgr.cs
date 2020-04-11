using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.GameObjects.Handlers;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Network;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Spawns;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.GameObjects
{
  /// <summary>General GameObject -utility and -container class</summary>
  [GlobalMgr]
  public static class GOMgr
  {
    private static Logger log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Usually a Unit must be at least this close (in yards) to a GO in order to be allowed to interact with it
    /// </summary>
    public static uint DefaultInteractDistanceSq = 100;

    [NotVariable]public static readonly Dictionary<uint, GOEntry> Entries = new Dictionary<uint, GOEntry>(10000);

    /// <summary>All templates for spawn pools</summary>
    public static readonly Dictionary<uint, GOSpawnPoolTemplate> SpawnPoolTemplates =
      new Dictionary<uint, GOSpawnPoolTemplate>();

    [NotVariable]public static GOSpawnEntry[] SpawnEntries = new GOSpawnEntry[40000];

    [NotVariable]public static List<GOSpawnPoolTemplate>[] SpawnPoolTemplatesByMap = new List<GOSpawnPoolTemplate>[727];

    /// <summary>
    /// Contains a set of <see cref="T:WCell.RealmServer.GameObjects.GameObjectHandler" />, indexed by <see cref="T:WCell.Constants.GameObjects.GameObjectType" />.
    /// </summary>
    public static readonly Func<GameObjectHandler>[] Handlers =
      new Func<GameObjectHandler>[(int) (Utility.GetMaxEnum<GameObjectType>() + 1U)];

    /// <summary>
    /// Contains a set of GOEntry-creators, indexed by <see cref="T:WCell.Constants.GameObjects.GameObjectType" />.
    /// Override these to create custom GOs.
    /// </summary>
    public static readonly Func<GOEntry>[] GOEntryCreators = ((Func<Func<GOEntry>[]>) (() =>
    {
      Func<GOEntry>[] funcArray = new Func<GOEntry>[(int) (Utility.GetMaxEnum<GameObjectType>() + 1U)];
      funcArray[0] = (Func<GOEntry>) (() => (GOEntry) new GODoorEntry());
      funcArray[1] = (Func<GOEntry>) (() => (GOEntry) new GOButtonEntry());
      funcArray[2] = (Func<GOEntry>) (() => (GOEntry) new GOQuestGiverEntry());
      funcArray[3] = (Func<GOEntry>) (() => (GOEntry) new GOChestEntry());
      funcArray[4] = (Func<GOEntry>) (() => (GOEntry) new GOBinderEntry());
      funcArray[5] = (Func<GOEntry>) (() => (GOEntry) new GOGenericEntry());
      funcArray[6] = (Func<GOEntry>) (() => (GOEntry) new GOTrapEntry());
      funcArray[7] = (Func<GOEntry>) (() => (GOEntry) new GOChairEntry());
      funcArray[8] = (Func<GOEntry>) (() => (GOEntry) new GOSpellFocusEntry());
      funcArray[9] = (Func<GOEntry>) (() => (GOEntry) new GOTextEntry());
      funcArray[10] = (Func<GOEntry>) (() => (GOEntry) new GOGooberEntry());
      funcArray[11] = (Func<GOEntry>) (() => (GOEntry) new GOTransportEntry());
      funcArray[12] = (Func<GOEntry>) (() => (GOEntry) new GOAreaDamageEntry());
      funcArray[13] = (Func<GOEntry>) (() => (GOEntry) new GOCameraEntry());
      funcArray[14] = (Func<GOEntry>) (() => (GOEntry) new GOMapObjectEntry());
      funcArray[15] = (Func<GOEntry>) (() => (GOEntry) new GOMOTransportEntry());
      funcArray[16] = (Func<GOEntry>) (() => (GOEntry) new GODuelFlagEntry());
      funcArray[17] = (Func<GOEntry>) (() => (GOEntry) new GOFishingNodeEntry());
      funcArray[18] = (Func<GOEntry>) (() => (GOEntry) new GOSummoningRitualEntry());
      funcArray[19] = (Func<GOEntry>) (() => (GOEntry) new GOMailboxEntry());
      funcArray[20] = (Func<GOEntry>) (() => (GOEntry) new GOAuctionHouseEntry());
      funcArray[21] = (Func<GOEntry>) (() => (GOEntry) new GOGuardPostEntry());
      funcArray[22] = (Func<GOEntry>) (() => (GOEntry) new GOSpellCasterEntry());
      funcArray[23] = (Func<GOEntry>) (() => (GOEntry) new GOMeetingStoneEntry());
      funcArray[24] = (Func<GOEntry>) (() => (GOEntry) new GOFlagStandEntry());
      funcArray[25] = (Func<GOEntry>) (() => (GOEntry) new GOFishingHoleEntry());
      funcArray[26] = (Func<GOEntry>) (() => (GOEntry) new GOFlagDropEntry());
      funcArray[27] = (Func<GOEntry>) (() => (GOEntry) new GOMiniGameEntry());
      funcArray[28] = (Func<GOEntry>) (() => (GOEntry) new GOLotteryKioskEntry());
      funcArray[29] = (Func<GOEntry>) (() => (GOEntry) new GOCapturePointEntry());
      funcArray[30] = (Func<GOEntry>) (() => (GOEntry) new GOAuraGeneratorEntry());
      funcArray[31] = (Func<GOEntry>) (() => (GOEntry) new GODungeonDifficultyEntry());
      funcArray[32] = (Func<GOEntry>) (() => (GOEntry) new GOBarberChairEntry());
      funcArray[33] = (Func<GOEntry>) (() => (GOEntry) new GODestructibleBuildingEntry());
      funcArray[34] = (Func<GOEntry>) (() => (GOEntry) new GOGuildBankEntry());
      funcArray[35] = (Func<GOEntry>) (() => (GOEntry) new GOTrapDoorEntry());
      return funcArray;
    }))();

    private static bool loaded;

    public static void HandleGOQuery(IRealmClient client, RealmPacketIn packet)
    {
      uint id = packet.ReadUInt32();
      if(!Loaded)
        return;
      GOEntry entry = GetEntry(id);
      if(entry == null)
        return;
      SendGameObjectInfo(client, entry);
    }

    public static void HandleGameObjectUse(IRealmClient client, RealmPacketIn packet)
    {
      EntityId id = packet.ReadEntityId();
      GameObject gameObject = client.ActiveCharacter.Map.GetObject(id) as GameObject;
      Character activeCharacter = client.ActiveCharacter;
      if(gameObject == null || !gameObject.CanUseInstantly(activeCharacter) ||
         activeCharacter.LooterEntry.Loot != null &&
         ReferenceEquals(activeCharacter.LooterEntry.Loot.Lootable, gameObject))
        return;
      gameObject.Use(client.ActiveCharacter);
    }

    public static void SendGameObjectInfo(IRealmClient client, GOEntry entry)
    {
      string str = entry.Names.Localize(client);
      using(RealmPacketOut packet =
        new RealmPacketOut(RealmServerOpCode.SMSG_GAMEOBJECT_QUERY_RESPONSE, 19 + str.Length + 96))
      {
        packet.Write(entry.Id);
        packet.Write((uint) entry.Type);
        packet.Write(entry.DisplayId);
        packet.Write(str);
        packet.Write((byte) 0);
        packet.Write((byte) 0);
        packet.Write((byte) 0);
        packet.Write((byte) 0);
        packet.Write((byte) 0);
        packet.Write((byte) 0);
        int index1;
        for(index1 = 0; index1 < entry.Fields.Length; ++index1)
          packet.Write(entry.Fields[index1]);
        for(; index1 < 24; ++index1)
          packet.Write(0);
        packet.Write(entry.Scale);
        for(int index2 = 0; index2 < 4; ++index2)
          packet.Write(0);
        client.Send(packet, false);
      }
    }

    public static IEnumerator<GOSpawnEntry> GetAllSpawnEntries()
    {
      return SpawnPoolTemplates.Values
        .SelectMany(
          pool =>
            (IEnumerable<GOSpawnEntry>) pool.Entries).GetEnumerator();
    }

    public static GOEntry GetEntry(uint id)
    {
      GOEntry goEntry;
      Entries.TryGetValue(id, out goEntry);
      return goEntry;
    }

    public static GOEntry GetEntry(GOEntryId id, bool force = true)
    {
      if(!loaded && force)
      {
        log.Warn("Tried to get GOEntry but GOs are not loaded: {0}", id);
        return null;
      }

      GOEntry goEntry;
      if(!Entries.TryGetValue((uint) id, out goEntry) && force)
        throw new ContentException("Tried to get non-existing GOEntry: {0}", (object) id);
      return goEntry;
    }

    public static GOSpawnEntry GetSpawnEntry(uint id)
    {
      if(id >= SpawnEntries.Length)
        return null;
      return SpawnEntries[id];
    }

    internal static GOSpawnPoolTemplate GetOrCreateSpawnPoolTemplate(uint poolId)
    {
      GOSpawnPoolTemplate spawnPoolTemplate;
      if(poolId == 0U)
      {
        spawnPoolTemplate = new GOSpawnPoolTemplate();
        SpawnPoolTemplates.Add(spawnPoolTemplate.PoolId, spawnPoolTemplate);
      }
      else if(!SpawnPoolTemplates.TryGetValue(poolId, out spawnPoolTemplate))
      {
        SpawnPoolTemplateEntry poolTemplateEntry = SpawnMgr.GetSpawnPoolTemplateEntry(poolId);
        spawnPoolTemplate = poolTemplateEntry == null
          ? new GOSpawnPoolTemplate()
          : new GOSpawnPoolTemplate(poolTemplateEntry);
        SpawnPoolTemplates.Add(spawnPoolTemplate.PoolId, spawnPoolTemplate);
      }

      return spawnPoolTemplate;
    }

    public static List<GOSpawnPoolTemplate> GetSpawnPoolTemplatesByMap(MapId map)
    {
      return SpawnPoolTemplatesByMap.Get((uint) map);
    }

    public static List<GOSpawnPoolTemplate> GetOrCreateSpawnPoolTemplatesByMap(MapId map)
    {
      List<GOSpawnPoolTemplate> spawnPoolTemplateList =
        SpawnPoolTemplatesByMap.Get((uint) map);
      if(spawnPoolTemplateList == null)
        SpawnPoolTemplatesByMap[(uint) map] = spawnPoolTemplateList = new List<GOSpawnPoolTemplate>();
      return spawnPoolTemplateList;
    }

    [Initialization(InitializationPass.Fifth, "Initialize GameObjects")]
    public static void Initialize()
    {
      LoadAll();
    }

    /// <summary>Loaded flag</summary>
    public static bool Loaded
    {
      get { return loaded; }
      private set
      {
        if(!(loaded = value) || ServerApp<RealmServer>.InitMgr == null)
          return;
        ServerApp<RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(GOMgr));
      }
    }

    public static void LoadAllLater()
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(() => LoadAll());
    }

    public static void LoadAll()
    {
      if(Loaded)
        return;
      ContentMgr.Load<GOEntry>();
      ContentMgr.Load<GOSpawnEntry>();
      ContentMgr.Load<Asda2Portal>();
      new GOPortalEntry().FinalizeDataHolder();
      foreach(GOEntry goEntry in Entries.Values)
      {
        if(goEntry.LinkedTrapId > 0U)
          goEntry.LinkedTrap = GetEntry(goEntry.LinkedTrapId) as GOTrapEntry;
        if(goEntry.SpawnEntries.Count == 0)
          goEntry.SpawnEntries.Add(new GOSpawnEntry
          {
            Entry = goEntry,
            Rotations = new float[0],
            State = GameObjectState.Enabled
          });
      }

      SetSummonSlots();
      Loaded = true;
    }

    private static void SetSummonSlots()
    {
      foreach(Spell spell in SpellHandler.ById)
      {
        if(spell != null)
        {
          foreach(SpellEffect effect in spell.Effects)
          {
            if(effect.EffectType >= SpellEffectType.SummonObjectSlot1 &&
               effect.EffectType <= SpellEffectType.SummonObjectSlot4)
            {
              GOEntry entry = GetEntry((uint) effect.MiscValue);
              if(entry != null)
                entry.SummonSlotId = (uint) (effect.EffectType - 104);
            }
          }
        }
      }
    }

    static GOMgr()
    {
      Handlers[0] = () => (GameObjectHandler) new DoorHandler();
      Handlers[1] = () => (GameObjectHandler) new ButtonHandler();
      Handlers[2] = () => (GameObjectHandler) new QuestGiverHandler();
      Handlers[3] = () => (GameObjectHandler) new ChestHandler();
      Handlers[4] = () => (GameObjectHandler) new BinderHandler();
      Handlers[5] = () => (GameObjectHandler) new GenericHandler();
      Handlers[6] = () => (GameObjectHandler) new TrapHandler();
      Handlers[7] = () => (GameObjectHandler) new ChairHandler();
      Handlers[8] = () => (GameObjectHandler) new SpellFocusHandler();
      Handlers[9] = () => (GameObjectHandler) new TextHandler();
      Handlers[10] = () => (GameObjectHandler) new GooberHandler();
      Handlers[11] = () => (GameObjectHandler) new TransportHandler();
      Handlers[12] = () => (GameObjectHandler) new AreaDamageHandler();
      Handlers[13] = () => (GameObjectHandler) new CameraHandler();
      Handlers[14] = () => (GameObjectHandler) new MapObjectHandler();
      Handlers[15] = () => (GameObjectHandler) new MOTransportHandler();
      Handlers[16] = () => (GameObjectHandler) new DuelFlagHandler();
      Handlers[17] = () => (GameObjectHandler) new FishingNodeHandler();
      Handlers[18] = () => (GameObjectHandler) new SummoningRitualHandler();
      Handlers[19] = () => (GameObjectHandler) new MailboxHandler();
      Handlers[21] = () => (GameObjectHandler) new GuardPostHandler();
      Handlers[22] = () => (GameObjectHandler) new SpellCasterHandler();
      Handlers[23] = () => (GameObjectHandler) new MeetingStoneHandler();
      Handlers[24] = () => (GameObjectHandler) new FlagStandHandler();
      Handlers[25] = () => (GameObjectHandler) new FishingHoleHandler();
      Handlers[26] = () => (GameObjectHandler) new FlagDropHandler();
      Handlers[27] = () => (GameObjectHandler) new MiniGameHandler();
      Handlers[28] = () => (GameObjectHandler) new LotteryKioskHandler();
      Handlers[29] = () => (GameObjectHandler) new CapturePointHandler();
      Handlers[30] = () => (GameObjectHandler) new AuraGeneratorHandler();
      Handlers[31] = () => (GameObjectHandler) new DungeonDifficultyHandler();
      Handlers[32] = () => (GameObjectHandler) new BarberChairHandler();
      Handlers[33] =
        () => (GameObjectHandler) new DestructibleBuildingHandler();
      Handlers[34] = () => (GameObjectHandler) new GuildBankHandler();
      Handlers[35] = () => (GameObjectHandler) new TrapDoorHandler();
      Handlers[100] = () => (GameObjectHandler) new CustomGOHandler();
    }

    [DependentInitialization(typeof(Asda2LootMgr))]
    [DependentInitialization(typeof(GOMgr))]
    [Initialization]
    [DependentInitialization(typeof(QuestMgr))]
    [DependentInitialization(typeof(ItemMgr))]
    public static void InitQuestGOs()
    {
      foreach(GOEntry goEntry in Entries.Values)
      {
        if(goEntry.Flags.HasFlag(GameObjectFlags.ConditionalInteraction) &&
           goEntry.Type == GameObjectType.Chest)
        {
          List<Asda2LootItemEntry> lootEntries = goEntry.GetLootEntries();
          if(lootEntries != null)
          {
            foreach(Asda2LootEntity asda2LootEntity in lootEntries)
            {
              Asda2ItemTemplate itemTemplate = asda2LootEntity.ItemTemplate;
              if(itemTemplate != null && itemTemplate.CollectQuests != null)
                goEntry.RequiredQuests.AddRange(
                  itemTemplate.CollectQuests);
            }
          }
        }
      }
    }

    public static GOSpawnEntry GetClosestEntry(this ICollection<GOSpawnEntry> entries, IWorldLocation pos)
    {
      if(pos == null)
        return entries.First();
      float num1 = float.MaxValue;
      GOSpawnEntry goSpawnEntry = null;
      foreach(GOSpawnEntry entry in entries)
      {
        if(entry.MapId == pos.MapId && (int) entry.Phase == (int) pos.Phase)
        {
          float num2 = pos.Position.DistanceSquared(entry.Position);
          if(num2 < (double) num1)
          {
            num1 = num2;
            goSpawnEntry = entry;
          }
        }
      }

      return goSpawnEntry;
    }

    public static GOSpawnEntry GetClosestEntry(this ICollection<GOSpawnPoolTemplate> templates, IWorldLocation pos)
    {
      if(pos == null)
        return templates.First().Entries.FirstOrDefault();
      float num1 = float.MaxValue;
      GOSpawnEntry goSpawnEntry = null;
      foreach(GOSpawnPoolTemplate template in templates)
      {
        if(template != null && template.MapId == pos.MapId)
        {
          foreach(GOSpawnEntry entry in template.Entries)
          {
            if((int) entry.Phase == (int) pos.Phase)
            {
              float num2 = pos.Position.DistanceSquared(entry.Position);
              if(num2 < (double) num1)
              {
                num1 = num2;
                goSpawnEntry = entry;
              }
            }
          }
        }
      }

      return goSpawnEntry;
    }
  }
}