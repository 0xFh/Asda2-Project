using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Looting;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.Looting
{
  /// <summary>Static utility and container class for Looting.</summary>
  [GlobalMgr]
  public static class LootMgr
  {
    /// <summary>
    /// If set, LootItems below threshold always are distributed using RoundRobin rules.
    /// Set it to false to have FFA rules apply by all default Items below threshold.
    /// </summary>
    public static bool RoundRobinDefault = true;

    /// <summary>
    /// The factor by which to multiply the amount of gold available to loot
    /// </summary>
    public static readonly ResolvedLootItemList[][] LootEntries = new ResolvedLootItemList[12][];

    internal static readonly List<KeyValuePair<ResolvedLootItemList, LootItemEntry>> ReferenceEntries =
      new List<KeyValuePair<ResolvedLootItemList, LootItemEntry>>();

    /// <summary>
    /// The highest roll that someone can get when rolling for Need before Greed.
    /// </summary>
    public const int HighestRoll = 100;

    private static bool loaded;

    static LootMgr()
    {
      LootEntries[2] = new ResolvedLootItemList[2000];
      LootEntries[3] = new ResolvedLootItemList[2000];
      LootEntries[4] = new ResolvedLootItemList[5000];
      LootEntries[5] = new ResolvedLootItemList[5000];
      LootEntries[1] = new ResolvedLootItemList[330000];
      LootEntries[6] = new ResolvedLootItemList[10000];
      LootEntries[7] = new ResolvedLootItemList[20];
      LootEntries[8] = new ResolvedLootItemList[400];
      LootEntries[9] = new ResolvedLootItemList[400];
      LootEntries[11] = new ResolvedLootItemList[10000];
    }

    public static bool Loaded
    {
      get { return loaded; }
      private set
      {
        if(!(loaded = value))
          return;
        ServerApp<RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(LootMgr));
      }
    }

    public static void LoadAll()
    {
      if(Loaded)
        return;
      ContentMgr.Load<NPCLootItemEntry>();
      ContentMgr.Load<ItemLootItemEntry>();
      ContentMgr.Load<GOLootItemEntry>();
      ContentMgr.Load<FishingLootItemEntry>();
      ContentMgr.Load<MillingLootItemEntry>();
      ContentMgr.Load<PickPocketLootItemEntry>();
      ContentMgr.Load<ProspectingLootItemEntry>();
      ContentMgr.Load<DisenchantingLootItemEntry>();
      ContentMgr.Load<ReferenceLootItemEntry>();
      for(int index = ReferenceEntries.Count - 1; index >= 0; --index)
      {
        KeyValuePair<ResolvedLootItemList, LootItemEntry> referenceEntry = ReferenceEntries[index];
        referenceEntry.Key.Remove(referenceEntry.Value);
        LookupRef(referenceEntry.Key, referenceEntry.Value);
      }

      Loaded = true;
    }

    private static void LookupRef(ResolvedLootItemList list, LootItemEntry entry)
    {
      ResolvedLootItemList entries = GetEntries(LootEntryType.Reference, entry.ReferencedEntryId);
      if(entries == null)
        return;
      if(entries.ResolveStatus < 1)
      {
        entries.ResolveStatus = 1;
        foreach(LootEntity ent in entries)
        {
          if(ent is LootItemEntry)
          {
            LootItemEntry entry1 = (LootItemEntry) ent;
            if(entry1.ReferencedEntryId > 0U)
            {
              LookupRef(list, entry1);
              continue;
            }
          }

          AddRef(list, ent);
        }

        entries.ResolveStatus = 2;
      }
      else if(list.ResolveStatus == 1)
      {
        LogManager.GetCurrentClassLogger()
          .Warn("Infinite loop in Loot references detected in: " + entry);
      }
      else
      {
        foreach(LootEntity ent in entries)
          AddRef(list, ent);
      }
    }

    private static void AddRef(ResolvedLootItemList list, LootEntity ent)
    {
      list.Add(ent);
    }

    /// <summary>
    /// Adds the new LootItemEntry to the global container.
    /// Keeps the set of entries sorted by rarity.
    /// </summary>
    public static void AddEntry(LootItemEntry entry)
    {
      ResolvedLootItemList[] lootEntry = LootEntries[(uint) entry.LootType];
      if(entry.EntryId >= lootEntry.Length)
      {
        ArrayUtil.EnsureSize(ref lootEntry, (int) (entry.EntryId * 1.5) + 1);
        LootEntries[(uint) entry.LootType] = lootEntry;
      }

      ResolvedLootItemList key = lootEntry[entry.EntryId];
      if(key == null)
        lootEntry[entry.EntryId] = key = new ResolvedLootItemList();
      if(entry.ReferencedEntryId > 0U || entry.GroupId > 0U)
        ReferenceEntries.Add(new KeyValuePair<ResolvedLootItemList, LootItemEntry>(key, entry));
      bool flag = false;
      for(int index = 0; index < key.Count; ++index)
      {
        if(key[index].DropChance > (double) entry.DropChance)
        {
          flag = true;
          key.Insert(index, entry);
          break;
        }
      }

      if(flag)
        return;
      key.Add(entry);
    }

    public static ResolvedLootItemList[] GetEntries(LootEntryType type)
    {
      return LootEntries[(uint) type];
    }

    public static ResolvedLootItemList GetEntries(LootEntryType type, uint id)
    {
      return LootEntries[(uint) type].Get(id);
    }

    /// <summary>
    /// Creates a new Loot object and returns it or null, if there is nothing to be looted.
    /// </summary>
    /// <typeparam name="T"><see cref="T:WCell.RealmServer.Looting.ObjectLoot" /> or <see cref="T:WCell.RealmServer.Looting.NPCLoot" /></typeparam>
    /// <param name="lootable"></param>
    /// <param name="initialLooter"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static T CreateLoot<T>(ILootable lootable, Character initialLooter, LootEntryType type, bool heroic,
      MapId mapid) where T : Loot, new()
    {
      return default(T);
    }

    /// <summary>Generates loot for Items and GOs.</summary>
    /// <param name="lootable">The Object or Unit that is being looted</param>
    /// <returns>The object's loot or null if there is nothing to get or the given Character can't access the loot.</returns>
    public static ObjectLoot CreateAndSendObjectLoot(ILootable lootable, Character initialLooter,
      LootEntryType type, bool heroic)
    {
      Asda2Loot loot = initialLooter.LooterEntry.Loot;
      if(loot != null)
        loot.ForceDispose();
      FindLooters(lootable, initialLooter);
      return CreateLoot<ObjectLoot>(lootable, initialLooter, type, heroic, MapId.Silaris);
    }

    /// <summary>
    /// Generates normal loot (usually for dead mob-corpses).
    /// Returns null, if the loot is empty.
    /// </summary>
    /// <param name="lootable">The Object or Unit that is being looted</param>
    public static Loot GetOrCreateLoot(ILootable lootable, Character triggerChar, LootEntryType type, bool heroic)
    {
      return null;
    }

    /// <summary>
    /// Returns all Items that can be looted off the given lootable
    /// </summary>
    public static LootItem[] CreateLootItems(uint lootId, LootEntryType type, bool heroic,
      IList<LooterEntry> looters)
    {
      ResolvedLootItemList entries = GetEntries(type, lootId);
      if(entries == null)
        return LootItem.EmptyArray;
      LootItem[] array = new LootItem[Math.Min(15, entries.Count)];
      int newSize = 0;
      foreach(LootEntity lootEntity in entries)
      {
        if(100.0 * Utility.RandomFloat() < lootEntity.DropChance)
        {
          ItemTemplate template = lootEntity.ItemTemplate;
          if(template != null &&
             looters.Any(looter =>
               template.CheckLootConstraints(looter.Owner)))
          {
            array[newSize] = new LootItem(template,
              Utility.Random(lootEntity.MinAmount, lootEntity.MaxAmount), (uint) newSize,
              template.RandomPropertiesId);
            ++newSize;
            if(newSize == 15)
              break;
          }
        }
      }

      if(newSize == 0)
        return LootItem.EmptyArray;
      Array.Resize(ref array, newSize);
      return array;
    }

    public static IList<LooterEntry> FindLooters(ILootable lootable, Character initialLooter)
    {
      List<LooterEntry> looterEntryList = new List<LooterEntry>();
      FindLooters(lootable, initialLooter, looterEntryList);
      return looterEntryList;
    }

    public static void FindLooters(ILootable lootable, Character initialLooter, IList<LooterEntry> looters)
    {
    }

    public static ResolvedLootItemList GetEntries(this ILootable lootable, LootEntryType type)
    {
      return null;
    }

    /// <summary>
    /// Returns whether this lockable can be opened by the given Character
    /// </summary>
    /// <param name="lockable"></param>
    /// <returns></returns>
    public static bool CanOpen(this ILockable lockable, Character chr)
    {
      return true;
    }

    public static bool TryLoot(this ILockable lockable, Character chr)
    {
      if(!lockable.CanOpen(chr))
        return false;
      LockEntry.Loot(lockable, chr);
      return true;
    }

    /// <summary>
    /// Whether the given lootable contains quest items for the given Character when looting with the given type
    /// </summary>
    public static bool ContainsQuestItemsFor(this ILootable lootable, Character chr, LootEntryType type)
    {
      Asda2Loot loot = lootable.Loot;
      if(loot != null)
        return loot.Items.Any(item =>
        {
          if(item.Template.HasQuestRequirements)
            return item.Template.CheckQuestConstraints(chr);
          return false;
        });
      ResolvedLootItemList entries = lootable.GetEntries(type);
      if(entries != null)
        return entries.Any(entry =>
        {
          if(entry.ItemTemplate.HasQuestRequirements)
            return entry.ItemTemplate.CheckQuestConstraints(chr);
          return false;
        });
      return false;
    }
  }
}