using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Core;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Lang;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Pets;

namespace WCell.RealmServer.Gossips
{
    /// <summary>TODO: Localizations</summary>
    public sealed class GossipMgr : Manager<GossipMgr>, IDisposable
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        internal static IDictionary<uint, IGossipEntry> GossipEntries =
            (IDictionary<uint, IGossipEntry>) new Dictionary<uint, IGossipEntry>(5000);

        public static readonly uint DefaultTextId = 91800;

        public static readonly StaticGossipEntry DefaultGossipEntry = new StaticGossipEntry(GossipMgr.DefaultTextId,
            new string[1]
            {
                "Hello there!"
            });

        public static readonly uint DynamicTextId = 91801;

        public static IGossipEntry GetEntry(uint id)
        {
            IGossipEntry gossipEntry;
            GossipMgr.GossipEntries.TryGetValue(id, out gossipEntry);
            return gossipEntry;
        }

        public static string DefaultGossipGreetingMale
        {
            get { return GossipMgr.DefaultGossipEntry.GetText(0).TextMale; }
            set { GossipMgr.DefaultGossipEntry.GetText(0).TextMale = value; }
        }

        public static string DefaultGossipGreetingFemale
        {
            get { return GossipMgr.DefaultGossipEntry.GetText(0).TextFemale; }
            set { GossipMgr.DefaultGossipEntry.GetText(0).TextFemale = value; }
        }

        public void Dispose()
        {
            foreach (Character allCharacter in World.GetAllCharacters())
            {
                if (allCharacter.GossipConversation != null)
                    allCharacter.GossipConversation = (GossipConversation) null;
            }
        }

        public static void LoadAll()
        {
            GossipMgr.LoadEntries();
            GossipMgr.LoadNPCRelations();
        }

        public static void LoadEntries()
        {
            ContentMgr.Load<StaticGossipEntry>();
        }

        /// <summary>Automatically called after NPCs are initialized</summary>
        internal static void LoadNPCRelations()
        {
            if (!ContentMgr.Load<NPCGossipRelation>())
                return;
            GossipMgr.AddDefaultGossipOptions();
        }

        /// <summary>Add default Gossip options for Vendors etc</summary>
        private static void AddDefaultGossipOptions()
        {
            foreach (NPCEntry entry1 in NPCMgr.Entries)
            {
                NPCEntry entry = entry1;
                if (entry != null && entry.NPCFlags.HasAnyFlag(NPCFlags.Gossip))
                {
                    GossipMenu gossipMenu = entry.DefaultGossip;
                    if (gossipMenu == null)
                        entry.DefaultGossip = gossipMenu = new GossipMenu();
                    if (entry.NPCFlags.HasAnyFlag(NPCFlags.Banker))
                        gossipMenu.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(GossipMenuIcon.Bank,
                            (GossipActionHandler) (convo => convo.Character.OpenBank(convo.Speaker)),
                            RealmLangKey.GossipOptionBanker, new object[0]));
                    if (entry.NPCFlags.HasAnyFlag(NPCFlags.BattleMaster))
                        gossipMenu.AddItem((GossipMenuItemBase) new GossipMenuItem(GossipMenuIcon.Battlefield,
                            "Battlefield...", (GossipActionHandler) (convo =>
                            {
                                if (entry.BattlegroundTemplate == null)
                                    return;
                                ((NPC) convo.Speaker).TalkToBattlemaster(convo.Character);
                            })));
                    if (entry.NPCFlags.HasAnyFlag(NPCFlags.InnKeeper))
                        gossipMenu.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(GossipMenuIcon.Bind,
                            (GossipActionHandler) (convo => convo.Character.BindTo((NPC) convo.Speaker)),
                            RealmLangKey.GossipOptionInnKeeper, new object[0]));
                    if (entry.NPCFlags.HasAnyFlag(NPCFlags.GuildBanker))
                        gossipMenu.AddItem((GossipMenuItemBase) new GossipMenuItem(GossipMenuIcon.Guild,
                            "Guild Bank...",
                            (GossipActionHandler) (convo =>
                                convo.Character.SendSystemMessage(RealmLangKey.FeatureNotYetImplemented,
                                    new object[0]))));
                    if (entry.NPCFlags.HasAnyFlag(NPCFlags.SpiritHealer))
                        gossipMenu.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(GossipMenuIcon.Resurrect,
                            (GossipActionHandler) (convo => convo.Character.ResurrectWithConsequences()),
                            RealmLangKey.GossipOptionSpiritHealer, new object[0]));
                    if (entry.NPCFlags.HasAnyFlag(NPCFlags.Petitioner))
                        gossipMenu.AddItem((GossipMenuItemBase) new GossipMenuItem(GossipMenuIcon.Bank, "Petitions...",
                            (GossipActionHandler) (convo => ((NPC) convo.Speaker).SendPetitionList(convo.Character))));
                    if (entry.NPCFlags.HasAnyFlag(NPCFlags.TabardDesigner))
                        gossipMenu.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(GossipMenuIcon.Tabard,
                            (GossipActionHandler) (convo =>
                                convo.Character.SendSystemMessage(RealmLangKey.FeatureNotYetImplemented,
                                    new object[0])), RealmLangKey.GossipOptionTabardDesigner, new object[0]));
                    if (entry.NPCFlags.HasAnyFlag(NPCFlags.FlightMaster))
                        gossipMenu.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(GossipMenuIcon.Taxi,
                            (GossipActionHandler) (convo => ((NPC) convo.Speaker).TalkToFM(convo.Character)),
                            RealmLangKey.GossipOptionFlightMaster, new object[0]));
                    if (entry.NPCFlags.HasAnyFlag(NPCFlags.StableMaster))
                        gossipMenu.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(
                            (GossipActionHandler) (convo =>
                                PetMgr.ListStabledPets(convo.Character, (NPC) convo.Speaker)),
                            RealmLangKey.GossipOptionStableMaster, new object[0]));
                    if (entry.NPCFlags.HasAnyFlag(NPCFlags.AnyTrainer))
                        gossipMenu.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(GossipMenuIcon.Taxi,
                            (GossipActionHandler) (convo => ((NPC) convo.Speaker).TalkToTrainer(convo.Character)),
                            RealmLangKey.GossipOptionTrainer, new object[0]));
                    if (entry.NPCFlags.HasAnyFlag(NPCFlags.AnyVendor))
                        gossipMenu.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(GossipMenuIcon.Trade,
                            (GossipActionHandler) (convo =>
                            {
                                if (((NPC) convo.Speaker).VendorEntry == null)
                                    return;
                                ((NPC) convo.Speaker).VendorEntry.UseVendor(convo.Character);
                            }), RealmLangKey.GossipOptionVendor, new object[0]));
                }
            }
        }

        public static void AddEntry(StaticGossipEntry entry)
        {
            GossipMgr.GossipEntries[entry.GossipId] = (IGossipEntry) entry;
        }

        public static void AddText(uint id, params StaticGossipText[] entries)
        {
            IDictionary<uint, IGossipEntry> gossipEntries = GossipMgr.GossipEntries;
            int num = (int) id;
            StaticGossipEntry staticGossipEntry1 = new StaticGossipEntry();
            staticGossipEntry1.GossipId = id;
            staticGossipEntry1.GossipTexts = (GossipTextBase[]) entries;
            StaticGossipEntry staticGossipEntry2 = staticGossipEntry1;
            gossipEntries[(uint) num] = (IGossipEntry) staticGossipEntry2;
        }
    }
}