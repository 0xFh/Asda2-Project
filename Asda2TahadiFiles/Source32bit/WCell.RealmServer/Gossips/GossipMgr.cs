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
      new Dictionary<uint, IGossipEntry>(5000);

    public static readonly uint DefaultTextId = 91800;

    public static readonly StaticGossipEntry DefaultGossipEntry = new StaticGossipEntry(DefaultTextId, "Hello there!");

    public static readonly uint DynamicTextId = 91801;

    public static IGossipEntry GetEntry(uint id)
    {
      IGossipEntry gossipEntry;
      GossipEntries.TryGetValue(id, out gossipEntry);
      return gossipEntry;
    }

    public static string DefaultGossipGreetingMale
    {
      get { return DefaultGossipEntry.GetText(0).TextMale; }
      set { DefaultGossipEntry.GetText(0).TextMale = value; }
    }

    public static string DefaultGossipGreetingFemale
    {
      get { return DefaultGossipEntry.GetText(0).TextFemale; }
      set { DefaultGossipEntry.GetText(0).TextFemale = value; }
    }

    public void Dispose()
    {
      foreach(Character allCharacter in World.GetAllCharacters())
      {
        if(allCharacter.GossipConversation != null)
          allCharacter.GossipConversation = null;
      }
    }

    public static void LoadAll()
    {
      LoadEntries();
      LoadNPCRelations();
    }

    public static void LoadEntries()
    {
      ContentMgr.Load<StaticGossipEntry>();
    }

    /// <summary>Automatically called after NPCs are initialized</summary>
    internal static void LoadNPCRelations()
    {
      if(!ContentMgr.Load<NPCGossipRelation>())
        return;
      AddDefaultGossipOptions();
    }

    /// <summary>Add default Gossip options for Vendors etc</summary>
    private static void AddDefaultGossipOptions()
    {
      foreach(NPCEntry entry1 in NPCMgr.Entries)
      {
        NPCEntry entry = entry1;
        if(entry != null && entry.NPCFlags.HasAnyFlag(NPCFlags.Gossip))
        {
          GossipMenu gossipMenu = entry.DefaultGossip;
          if(gossipMenu == null)
            entry.DefaultGossip = gossipMenu = new GossipMenu();
          if(entry.NPCFlags.HasAnyFlag(NPCFlags.Banker))
            gossipMenu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Bank,
              convo => convo.Character.OpenBank(convo.Speaker),
              RealmLangKey.GossipOptionBanker));
          if(entry.NPCFlags.HasAnyFlag(NPCFlags.BattleMaster))
            gossipMenu.AddItem(new GossipMenuItem(GossipMenuIcon.Battlefield,
              "Battlefield...", convo =>
              {
                if(entry.BattlegroundTemplate == null)
                  return;
                ((NPC) convo.Speaker).TalkToBattlemaster(convo.Character);
              }));
          if(entry.NPCFlags.HasAnyFlag(NPCFlags.InnKeeper))
            gossipMenu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Bind,
              convo => convo.Character.BindTo((NPC) convo.Speaker),
              RealmLangKey.GossipOptionInnKeeper));
          if(entry.NPCFlags.HasAnyFlag(NPCFlags.GuildBanker))
            gossipMenu.AddItem(new GossipMenuItem(GossipMenuIcon.Guild,
              "Guild Bank...",
              convo =>
                convo.Character.SendSystemMessage(RealmLangKey.FeatureNotYetImplemented)));
          if(entry.NPCFlags.HasAnyFlag(NPCFlags.SpiritHealer))
            gossipMenu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Resurrect,
              convo => convo.Character.ResurrectWithConsequences(),
              RealmLangKey.GossipOptionSpiritHealer));
          if(entry.NPCFlags.HasAnyFlag(NPCFlags.Petitioner))
            gossipMenu.AddItem(new GossipMenuItem(GossipMenuIcon.Bank, "Petitions...",
              convo => ((NPC) convo.Speaker).SendPetitionList(convo.Character)));
          if(entry.NPCFlags.HasAnyFlag(NPCFlags.TabardDesigner))
            gossipMenu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Tabard,
              convo =>
                convo.Character.SendSystemMessage(RealmLangKey.FeatureNotYetImplemented),
              RealmLangKey.GossipOptionTabardDesigner));
          if(entry.NPCFlags.HasAnyFlag(NPCFlags.FlightMaster))
            gossipMenu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Taxi,
              convo => ((NPC) convo.Speaker).TalkToFM(convo.Character),
              RealmLangKey.GossipOptionFlightMaster));
          if(entry.NPCFlags.HasAnyFlag(NPCFlags.StableMaster))
            gossipMenu.AddItem(new LocalizedGossipMenuItem(
              convo =>
                PetMgr.ListStabledPets(convo.Character, (NPC) convo.Speaker),
              RealmLangKey.GossipOptionStableMaster));
          if(entry.NPCFlags.HasAnyFlag(NPCFlags.AnyTrainer))
            gossipMenu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Taxi,
              convo => ((NPC) convo.Speaker).TalkToTrainer(convo.Character),
              RealmLangKey.GossipOptionTrainer));
          if(entry.NPCFlags.HasAnyFlag(NPCFlags.AnyVendor))
            gossipMenu.AddItem(new LocalizedGossipMenuItem(GossipMenuIcon.Trade,
              convo =>
              {
                if(((NPC) convo.Speaker).VendorEntry == null)
                  return;
                ((NPC) convo.Speaker).VendorEntry.UseVendor(convo.Character);
              }, RealmLangKey.GossipOptionVendor));
        }
      }
    }

    public static void AddEntry(StaticGossipEntry entry)
    {
      GossipEntries[entry.GossipId] = entry;
    }

    public static void AddText(uint id, params StaticGossipText[] entries)
    {
      IDictionary<uint, IGossipEntry> gossipEntries = GossipEntries;
      int num = (int) id;
      StaticGossipEntry staticGossipEntry1 = new StaticGossipEntry();
      staticGossipEntry1.GossipId = id;
      staticGossipEntry1.GossipTexts = entries;
      StaticGossipEntry staticGossipEntry2 = staticGossipEntry1;
      gossipEntries[(uint) num] = staticGossipEntry2;
    }
  }
}