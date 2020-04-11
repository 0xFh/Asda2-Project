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

        [NotVariable] public static readonly Dictionary<uint, GOEntry> Entries = new Dictionary<uint, GOEntry>(10000);

        /// <summary>All templates for spawn pools</summary>
        public static readonly Dictionary<uint, GOSpawnPoolTemplate> SpawnPoolTemplates =
            new Dictionary<uint, GOSpawnPoolTemplate>();

        [NotVariable] public static GOSpawnEntry[] SpawnEntries = new GOSpawnEntry[40000];

        [NotVariable]
        public static List<GOSpawnPoolTemplate>[] SpawnPoolTemplatesByMap = new List<GOSpawnPoolTemplate>[727];

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
            if (!GOMgr.Loaded)
                return;
            GOEntry entry = GOMgr.GetEntry(id);
            if (entry == null)
                return;
            GOMgr.SendGameObjectInfo(client, entry);
        }

        public static void HandleGameObjectUse(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            GameObject gameObject = client.ActiveCharacter.Map.GetObject(id) as GameObject;
            Character activeCharacter = client.ActiveCharacter;
            if (gameObject == null || !gameObject.CanUseInstantly(activeCharacter) ||
                activeCharacter.LooterEntry.Loot != null &&
                object.ReferenceEquals((object) activeCharacter.LooterEntry.Loot.Lootable, (object) gameObject))
                return;
            gameObject.Use(client.ActiveCharacter);
        }

        public static void SendGameObjectInfo(IRealmClient client, GOEntry entry)
        {
            string str = entry.Names.Localize(client);
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GAMEOBJECT_QUERY_RESPONSE, 19 + str.Length + 96))
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
                for (index1 = 0; index1 < entry.Fields.Length; ++index1)
                    packet.Write(entry.Fields[index1]);
                for (; index1 < 24; ++index1)
                    packet.Write(0);
                packet.Write(entry.Scale);
                for (int index2 = 0; index2 < 4; ++index2)
                    packet.Write(0);
                client.Send(packet, false);
            }
        }

        public static IEnumerator<GOSpawnEntry> GetAllSpawnEntries()
        {
            return GOMgr.SpawnPoolTemplates.Values
                .SelectMany<GOSpawnPoolTemplate, GOSpawnEntry>(
                    (Func<GOSpawnPoolTemplate, IEnumerable<GOSpawnEntry>>) (pool =>
                        (IEnumerable<GOSpawnEntry>) pool.Entries)).GetEnumerator();
        }

        public static GOEntry GetEntry(uint id)
        {
            GOEntry goEntry;
            GOMgr.Entries.TryGetValue(id, out goEntry);
            return goEntry;
        }

        public static GOEntry GetEntry(GOEntryId id, bool force = true)
        {
            if (!GOMgr.loaded && force)
            {
                GOMgr.log.Warn("Tried to get GOEntry but GOs are not loaded: {0}", (object) id);
                return (GOEntry) null;
            }

            GOEntry goEntry;
            if (!GOMgr.Entries.TryGetValue((uint) id, out goEntry) && force)
                throw new ContentException("Tried to get non-existing GOEntry: {0}", new object[1]
                {
                    (object) id
                });
            return goEntry;
        }

        public static GOSpawnEntry GetSpawnEntry(uint id)
        {
            if ((long) id >= (long) GOMgr.SpawnEntries.Length)
                return (GOSpawnEntry) null;
            return GOMgr.SpawnEntries[id];
        }

        internal static GOSpawnPoolTemplate GetOrCreateSpawnPoolTemplate(uint poolId)
        {
            GOSpawnPoolTemplate spawnPoolTemplate;
            if (poolId == 0U)
            {
                spawnPoolTemplate = new GOSpawnPoolTemplate();
                GOMgr.SpawnPoolTemplates.Add(spawnPoolTemplate.PoolId, spawnPoolTemplate);
            }
            else if (!GOMgr.SpawnPoolTemplates.TryGetValue(poolId, out spawnPoolTemplate))
            {
                SpawnPoolTemplateEntry poolTemplateEntry = SpawnMgr.GetSpawnPoolTemplateEntry(poolId);
                spawnPoolTemplate = poolTemplateEntry == null
                    ? new GOSpawnPoolTemplate()
                    : new GOSpawnPoolTemplate(poolTemplateEntry);
                GOMgr.SpawnPoolTemplates.Add(spawnPoolTemplate.PoolId, spawnPoolTemplate);
            }

            return spawnPoolTemplate;
        }

        public static List<GOSpawnPoolTemplate> GetSpawnPoolTemplatesByMap(MapId map)
        {
            return GOMgr.SpawnPoolTemplatesByMap.Get<List<GOSpawnPoolTemplate>>((uint) map);
        }

        public static List<GOSpawnPoolTemplate> GetOrCreateSpawnPoolTemplatesByMap(MapId map)
        {
            List<GOSpawnPoolTemplate> spawnPoolTemplateList =
                GOMgr.SpawnPoolTemplatesByMap.Get<List<GOSpawnPoolTemplate>>((uint) map);
            if (spawnPoolTemplateList == null)
                GOMgr.SpawnPoolTemplatesByMap[(uint) map] = spawnPoolTemplateList = new List<GOSpawnPoolTemplate>();
            return spawnPoolTemplateList;
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Fifth, "Initialize GameObjects")]
        public static void Initialize()
        {
            GOMgr.LoadAll();
        }

        /// <summary>Loaded flag</summary>
        public static bool Loaded
        {
            get { return GOMgr.loaded; }
            private set
            {
                if (!(GOMgr.loaded = value) || ServerApp<WCell.RealmServer.RealmServer>.InitMgr == null)
                    return;
                ServerApp<WCell.RealmServer.RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(GOMgr));
            }
        }

        public static void LoadAllLater()
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() => GOMgr.LoadAll()));
        }

        public static void LoadAll()
        {
            if (GOMgr.Loaded)
                return;
            ContentMgr.Load<GOEntry>();
            ContentMgr.Load<GOSpawnEntry>();
            ContentMgr.Load<Asda2Portal>();
            new GOPortalEntry().FinalizeDataHolder();
            foreach (GOEntry goEntry in GOMgr.Entries.Values)
            {
                if (goEntry.LinkedTrapId > 0U)
                    goEntry.LinkedTrap = GOMgr.GetEntry(goEntry.LinkedTrapId) as GOTrapEntry;
                if (goEntry.SpawnEntries.Count == 0)
                    goEntry.SpawnEntries.Add(new GOSpawnEntry()
                    {
                        Entry = goEntry,
                        Rotations = new float[0],
                        State = GameObjectState.Enabled
                    });
            }

            GOMgr.SetSummonSlots();
            GOMgr.Loaded = true;
        }

        private static void SetSummonSlots()
        {
            foreach (Spell spell in SpellHandler.ById)
            {
                if (spell != null)
                {
                    foreach (SpellEffect effect in spell.Effects)
                    {
                        if (effect.EffectType >= SpellEffectType.SummonObjectSlot1 &&
                            effect.EffectType <= SpellEffectType.SummonObjectSlot4)
                        {
                            GOEntry entry = GOMgr.GetEntry((uint) effect.MiscValue);
                            if (entry != null)
                                entry.SummonSlotId = (uint) (effect.EffectType - 104);
                        }
                    }
                }
            }
        }

        static GOMgr()
        {
            GOMgr.Handlers[0] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new DoorHandler());
            GOMgr.Handlers[1] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new ButtonHandler());
            GOMgr.Handlers[2] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new QuestGiverHandler());
            GOMgr.Handlers[3] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new ChestHandler());
            GOMgr.Handlers[4] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new BinderHandler());
            GOMgr.Handlers[5] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new GenericHandler());
            GOMgr.Handlers[6] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new TrapHandler());
            GOMgr.Handlers[7] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new ChairHandler());
            GOMgr.Handlers[8] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new SpellFocusHandler());
            GOMgr.Handlers[9] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new TextHandler());
            GOMgr.Handlers[10] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new GooberHandler());
            GOMgr.Handlers[11] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new TransportHandler());
            GOMgr.Handlers[12] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new AreaDamageHandler());
            GOMgr.Handlers[13] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new CameraHandler());
            GOMgr.Handlers[14] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new MapObjectHandler());
            GOMgr.Handlers[15] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new MOTransportHandler());
            GOMgr.Handlers[16] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new DuelFlagHandler());
            GOMgr.Handlers[17] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new FishingNodeHandler());
            GOMgr.Handlers[18] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new SummoningRitualHandler());
            GOMgr.Handlers[19] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new MailboxHandler());
            GOMgr.Handlers[21] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new GuardPostHandler());
            GOMgr.Handlers[22] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new SpellCasterHandler());
            GOMgr.Handlers[23] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new MeetingStoneHandler());
            GOMgr.Handlers[24] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new FlagStandHandler());
            GOMgr.Handlers[25] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new FishingHoleHandler());
            GOMgr.Handlers[26] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new FlagDropHandler());
            GOMgr.Handlers[27] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new MiniGameHandler());
            GOMgr.Handlers[28] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new LotteryKioskHandler());
            GOMgr.Handlers[29] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new CapturePointHandler());
            GOMgr.Handlers[30] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new AuraGeneratorHandler());
            GOMgr.Handlers[31] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new DungeonDifficultyHandler());
            GOMgr.Handlers[32] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new BarberChairHandler());
            GOMgr.Handlers[33] =
                (Func<GameObjectHandler>) (() => (GameObjectHandler) new DestructibleBuildingHandler());
            GOMgr.Handlers[34] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new GuildBankHandler());
            GOMgr.Handlers[35] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new TrapDoorHandler());
            GOMgr.Handlers[100] = (Func<GameObjectHandler>) (() => (GameObjectHandler) new CustomGOHandler());
        }

        [DependentInitialization(typeof(Asda2LootMgr))]
        [DependentInitialization(typeof(GOMgr))]
        [WCell.Core.Initialization.Initialization]
        [DependentInitialization(typeof(QuestMgr))]
        [DependentInitialization(typeof(ItemMgr))]
        public static void InitQuestGOs()
        {
            foreach (GOEntry goEntry in GOMgr.Entries.Values)
            {
                if (goEntry.Flags.HasFlag((Enum) GameObjectFlags.ConditionalInteraction) &&
                    goEntry.Type == GameObjectType.Chest)
                {
                    List<Asda2LootItemEntry> lootEntries = goEntry.GetLootEntries();
                    if (lootEntries != null)
                    {
                        foreach (Asda2LootEntity asda2LootEntity in lootEntries)
                        {
                            Asda2ItemTemplate itemTemplate = asda2LootEntity.ItemTemplate;
                            if (itemTemplate != null && itemTemplate.CollectQuests != null)
                                goEntry.RequiredQuests.AddRange(
                                    (IEnumerable<QuestTemplate>) itemTemplate.CollectQuests);
                        }
                    }
                }
            }
        }

        public static GOSpawnEntry GetClosestEntry(this ICollection<GOSpawnEntry> entries, IWorldLocation pos)
        {
            if (pos == null)
                return entries.First<GOSpawnEntry>();
            float num1 = float.MaxValue;
            GOSpawnEntry goSpawnEntry = (GOSpawnEntry) null;
            foreach (GOSpawnEntry entry in (IEnumerable<GOSpawnEntry>) entries)
            {
                if (entry.MapId == pos.MapId && (int) entry.Phase == (int) pos.Phase)
                {
                    float num2 = pos.Position.DistanceSquared(entry.Position);
                    if ((double) num2 < (double) num1)
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
            if (pos == null)
                return templates.First<GOSpawnPoolTemplate>().Entries.FirstOrDefault<GOSpawnEntry>();
            float num1 = float.MaxValue;
            GOSpawnEntry goSpawnEntry = (GOSpawnEntry) null;
            foreach (GOSpawnPoolTemplate template in (IEnumerable<GOSpawnPoolTemplate>) templates)
            {
                if (template != null && template.MapId == pos.MapId)
                {
                    foreach (GOSpawnEntry entry in template.Entries)
                    {
                        if ((int) entry.Phase == (int) pos.Phase)
                        {
                            float num2 = pos.Position.DistanceSquared(entry.Position);
                            if ((double) num2 < (double) num1)
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