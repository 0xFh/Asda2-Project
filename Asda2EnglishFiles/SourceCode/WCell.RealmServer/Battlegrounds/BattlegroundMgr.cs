using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.DynamicAccess;
using WCell.Util.Variables;

namespace WCell.RealmServer.Battlegrounds
{
    public static class BattlegroundMgr
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>All Battleground instances</summary>
        public static readonly WorldInstanceCollection<BattlegroundId, Battleground> Instances =
            new WorldInstanceCollection<BattlegroundId, Battleground>(BattlegroundId.End);

        /// <summary>Indexed by BattlegroundId</summary>
        public static readonly BattlegroundTemplate[] Templates = new BattlegroundTemplate[32];

        private static SpellId deserterSpellId = SpellId.Deserter;

        /// <summary>
        /// Whether to flag <see cref="T:WCell.RealmServer.Entities.Character" />s with the <see cref="P:WCell.RealmServer.Battlegrounds.BattlegroundMgr.DeserterSpell" />
        /// </summary>
        [Variable("BGFlagDeserters")] public static bool FlagDeserters = true;

        /// <summary>
        /// Time until an invitation to a Battleground will be cancelled.
        /// Default: 2 minutes
        /// </summary>
        [Variable("BGInvitationTimeoutMillis")]
        public static int InvitationTimeoutMillis = 120000;

        [Variable("BGMaxAwardedHonor")] public static int MaxHonor = 10;
        [Variable("BGMaxHonorLevelDiff")] public static int MaxLvlDiff = 5;

        /// <summary>
        /// Amount of deaths that yield honor to the killing opponent
        /// </summary>
        [Variable("BGMaxHonorableDeaths")] public static int MaxHonorableDeaths = 50;

        /// <summary>
        /// Max amount of Battlegrounds one Character may queue up for at a time
        /// </summary>
        [Variable("BGMaxQueuesPerChar")] public static int MaxQueuesPerChar = 5;

        public static MappedDBCReader<BattlemasterList, BattlemasterConverter> BattlemasterListReader;
        public static MappedDBCReader<PvPDifficultyEntry, PvPDifficultyConverter> PVPDifficultyReader;
        private static Spell deserterSpell;
        [NotVariable] public static Dictionary<int, WorldSafeLocation> WorldSafeLocs;

        /// <summary>
        /// The spell casted on players who leave a battleground before completion.
        /// </summary>
        public static SpellId DeserterSpellId
        {
            get { return BattlegroundMgr.deserterSpellId; }
            set
            {
                Spell spell = SpellHandler.Get(value);
                if (spell == null)
                    return;
                BattlegroundMgr.deserterSpellId = value;
                BattlegroundMgr.DeserterSpell = spell;
            }
        }

        [NotVariable]
        public static Spell DeserterSpell
        {
            get { return BattlegroundMgr.deserterSpell; }
            set
            {
                BattlegroundMgr.deserterSpell = value;
                if (BattlegroundMgr.deserterSpell == null)
                    BattlegroundMgr.log.Error("Invalid DeserterSpellId: " + (object) BattlegroundMgr.DeserterSpellId);
                else
                    BattlegroundMgr.deserterSpellId = BattlegroundMgr.deserterSpell.SpellId;
            }
        }

        public static bool Loaded { get; private set; }

        public static void InitializeBGs()
        {
            BattlegroundMgr.BattlemasterListReader =
                new MappedDBCReader<BattlemasterList, BattlemasterConverter>(
                    RealmServerConfiguration.GetDBCFile("BattlemasterList.dbc"));
            BattlegroundMgr.PVPDifficultyReader =
                new MappedDBCReader<PvPDifficultyEntry, PvPDifficultyConverter>(
                    RealmServerConfiguration.GetDBCFile("PvpDifficulty.dbc"));
            ContentMgr.Load<BattlegroundTemplate>();
            BattlegroundMgr.DeserterSpell = SpellHandler.Get(BattlegroundMgr.DeserterSpellId);
            BattlegroundMgr.Loaded = true;
            BattlegroundConfig.LoadSettings();
            BattlegroundMgr.EnsureBattlemasterRelations();
        }

        internal static void EnsureBattlemasterRelations()
        {
            if (!NPCMgr.Loaded || !BattlegroundMgr.Loaded)
                return;
            ContentMgr.Load<BattlemasterRelation>();
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Fourth, "Initialize WorldSafeLocs")]
        public static void LoadWorldSafeLocs()
        {
        }

        public static void SetCreator(BattlegroundId id, string typeName)
        {
            Type type = ServerApp<WCell.RealmServer.RealmServer>.GetType(typeName);
            BattlegroundTemplate template = BattlegroundMgr.GetTemplate(id);
            if (type == (Type) null || template == null)
            {
                object obj1;
                object obj2;
                if (type == (Type) null)
                {
                    obj1 = (object) "type";
                    obj2 = (object) string.Format("({0}) - Please correct it in the Battleground-config file: {1}",
                        (object) typeName, (object) InstanceConfigBase<BattlegroundConfig, BattlegroundId>.Filename);
                }
                else
                {
                    obj1 = (object) "Template";
                    obj2 = (object) "<not in DB>";
                }

                BattlegroundMgr.log.Warn("Battleground {0} has invalid {1} {2}", (object) id, obj1, obj2);
            }
            else
            {
                IProducer producer = AccessorMgr.GetOrCreateDefaultProducer(type);
                template.Creator = (BattlegroundCreator) (() => (Battleground) producer.Produce());
            }
        }

        public static void SetCreator(BattlegroundId id, BattlegroundCreator creator)
        {
            BattlegroundMgr.GetTemplate(id).Creator = creator;
        }

        public static BattlegroundTemplate GetTemplate(BattlegroundId bgid)
        {
            return BattlegroundMgr.Templates.Get<BattlegroundTemplate>((uint) bgid);
        }

        /// <summary>
        /// Gets the global <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundQueue" /> for the given Battleground for
        /// the given Character.
        /// </summary>
        /// <param name="bgid"></param>
        /// <returns></returns>
        public static GlobalBattlegroundQueue GetGlobalQueue(BattlegroundId bgid, Unit unit)
        {
            return BattlegroundMgr.GetGlobalQueue(bgid, unit.Level);
        }

        /// <summary>
        /// Gets the global <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundQueue" /> for the given Battleground for
        /// the given Character.
        /// </summary>
        /// <param name="bgid"></param>
        /// <returns></returns>
        public static GlobalBattlegroundQueue GetGlobalQueue(BattlegroundId bgid, int level)
        {
            return BattlegroundMgr.Templates.Get<BattlegroundTemplate>((uint) bgid).GetQueue(level);
        }

        /// <summary>
        /// Gets the <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundQueue" /> for a specific instance of the given Battleground for
        /// the given Character.
        /// </summary>
        /// <param name="bgid"></param>
        /// <returns></returns>
        public static BattlegroundQueue GetInstanceQueue(BattlegroundId bgid, uint instanceId, Unit unit)
        {
            return BattlegroundMgr.GetInstanceQueue(bgid, instanceId, unit.Level);
        }

        /// <summary>
        /// Gets the <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundQueue" /> for a specific instance of the given Battleground for
        /// the given Character.
        /// </summary>
        /// <param name="bgid"></param>
        /// <param name="level">The level determines the bracket id of the queue.</param>
        /// <returns></returns>
        public static BattlegroundQueue GetInstanceQueue(BattlegroundId bgid, uint instanceId, int level)
        {
            Battleground battleground = BattlegroundMgr.Templates.Get<BattlegroundTemplate>((uint) bgid).GetQueue(level)
                .GetBattleground(instanceId);
            if (battleground != null)
                return (BattlegroundQueue) battleground.InstanceQueue;
            return (BattlegroundQueue) null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="chr"></param>
        public static void TalkToBattlemaster(this NPC bm, Character chr)
        {
            chr.OnInteract((WorldObject) bm);
            BattlegroundTemplate battlegroundTemplate = bm.Entry.BattlegroundTemplate;
            if (battlegroundTemplate == null)
                return;
            GlobalBattlegroundQueue queue = battlegroundTemplate.GetQueue(chr.Level);
            if (queue == null)
                return;
            BattlegroundHandler.SendBattlefieldList(chr, queue);
        }

        public static uint GetHolidayIdByBGId(BattlegroundId bgId)
        {
            switch (bgId)
            {
                case BattlegroundId.AlteracValley:
                    return 283;
                case BattlegroundId.WarsongGulch:
                    return 284;
                case BattlegroundId.ArathiBasin:
                    return 285;
                case BattlegroundId.EyeOfTheStorm:
                    return 353;
                case BattlegroundId.StrandOfTheAncients:
                    return 400;
                case BattlegroundId.IsleOfConquest:
                    return 420;
                default:
                    return 0;
            }
        }

        public static BattlegroundSide GetBattlegroundSide(this FactionGroup faction)
        {
            return faction == FactionGroup.Horde ? BattlegroundSide.Horde : BattlegroundSide.Alliance;
        }

        public static BattlegroundSide GetOppositeSide(this BattlegroundSide side)
        {
            return side != BattlegroundSide.Alliance ? BattlegroundSide.Alliance : BattlegroundSide.Horde;
        }

        public static FactionGroup GetFactionGroup(this BattlegroundSide side)
        {
            return side == BattlegroundSide.Horde ? FactionGroup.Horde : FactionGroup.Alliance;
        }

        /// <summary>Enqueues players in a battleground queue.</summary>
        /// <param name="chr">the character who enqueued</param>
        /// <param name="bgId">the type of battleground</param>
        /// <param name="instanceId">the instance id of the battleground</param>
        /// <param name="asGroup">whether or not to enqueue the character or his/her group</param>
        internal static void EnqueuePlayers(Character chr, BattlegroundId bgId, uint instanceId, bool asGroup)
        {
            if (!chr.Battlegrounds.HasAvailableQueueSlots)
            {
                BattlegroundHandler.SendBattlegroundError((IPacketReceiver) chr, BattlegroundJoinError.Max3Battles);
            }
            else
            {
                if (chr.Battlegrounds.IsEnqueuedFor(bgId))
                    return;
                BattlegroundTemplate template = BattlegroundMgr.GetTemplate(bgId);
                if (template == null)
                    return;
                template.TryEnqueue(chr, asGroup, instanceId);
            }
        }
    }
}