using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Asda2Looting
{
    /// <summary>Static utility and container class for Looting.</summary>
    [GlobalMgr]
    public static class Asda2LootMgr
    {
        /// <summary>
        /// Everyone in the Group who is within this Radius, can loot and gets a share of the money
        /// </summary>
        public static float LootRadius = 200f;

        /// <summary>
        /// The factor to be applied to each Item's drop-chance before determining whether it will drop or not.
        /// </summary>
        public static float LootItemDropFactor = 1f;

        /// <summary>
        /// The factor by which to multiply the amount of gold available to loot
        /// </summary>
        public static uint DefaultMoneyDropFactor = 1;

        public static readonly List<Asda2LootItemEntry>[][] LootEntries = new List<Asda2LootItemEntry>[12][];
        private static bool loaded;

        static Asda2LootMgr()
        {
            Asda2LootMgr.LootEntries[2] = new List<Asda2LootItemEntry>[2000];
            Asda2LootMgr.LootEntries[5] = new List<Asda2LootItemEntry>[2000];
            Asda2LootMgr.LootEntries[1] = new List<Asda2LootItemEntry>[2000];
            Asda2LootMgr.LootEntries[4] = new List<Asda2LootItemEntry>[2000];
            Asda2LootMgr.LootEntries[3] = new List<Asda2LootItemEntry>[2000];
            Asda2LootMgr.LootEntries[6] = new List<Asda2LootItemEntry>[2000];
            Asda2LootMgr.LootEntries[7] = new List<Asda2LootItemEntry>[2000];
        }

        public static bool Loaded
        {
            get { return Asda2LootMgr.loaded; }
            private set
            {
                if (!(Asda2LootMgr.loaded = value))
                    return;
                ServerApp<WCell.RealmServer.RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(Asda2LootMgr));
            }
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth, "Load Loot")]
        public static void LoadAll()
        {
            ContentMgr.Load<Asda2NPCLootItemEntry>(true);
            Asda2LootMgr.Loaded = true;
        }

        /// <summary>
        /// Adds the new LootItemEntry to the global container.
        /// Keeps the set of entries sorted by rarity.
        /// </summary>
        public static void AddEntry(Asda2LootItemEntry entry)
        {
            List<Asda2LootItemEntry>[] lootEntry = Asda2LootMgr.LootEntries[(uint) entry.LootType];
            if (entry.LootType == Asda2LootEntryType.None)
            {
                LogUtil.WarnException(
                    string.Format("Bad drop template in db [MonstrId{0},itemID{1},guid{2}] cause type NONE",
                        (object) entry.MonstrId, (object) entry.ItemId, (object) entry.Guid), new object[0]);
            }
            else
            {
                if ((long) entry.MonstrId >= (long) lootEntry.Length)
                {
                    ArrayUtil.EnsureSize<List<Asda2LootItemEntry>>(ref lootEntry,
                        (int) ((double) entry.MonstrId * 1.5) + 1);
                    Asda2LootMgr.LootEntries[(uint) entry.LootType] = lootEntry;
                }

                List<Asda2LootItemEntry> asda2LootItemEntryList = lootEntry[entry.MonstrId];
                if (asda2LootItemEntryList == null)
                    lootEntry[entry.MonstrId] = asda2LootItemEntryList = new List<Asda2LootItemEntry>();
                bool flag = false;
                for (int index = 0; index < asda2LootItemEntryList.Count; ++index)
                {
                    if ((double) asda2LootItemEntryList[index].DropChance > (double) entry.DropChance)
                    {
                        flag = true;
                        asda2LootItemEntryList.Insert(index, entry);
                        break;
                    }
                }

                if (flag)
                    return;
                asda2LootItemEntryList.Add(entry);
            }
        }

        public static List<Asda2LootItemEntry>[] GetEntries(Asda2LootEntryType type)
        {
            return Asda2LootMgr.LootEntries[(uint) type];
        }

        public static List<Asda2LootItemEntry> GetEntries(Asda2LootEntryType type, uint id)
        {
            return Asda2LootMgr.LootEntries[(uint) type].Get<List<Asda2LootItemEntry>>(id);
        }

        /// <summary>
        /// Creates a new Loot object and returns it or null, if there is nothing to be looted.
        /// </summary>
        /// <typeparam name="T"><see cref="T:WCell.RealmServer.Looting.ObjectLoot" /> or <see cref="T:WCell.RealmServer.Looting.NPCLoot" /></typeparam>
        /// <param name="lootable"></param>
        /// <param name="initialLooter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T CreateLoot<T>(IAsda2Lootable lootable, Character initialLooter, Asda2LootEntryType type,
            MapId mapid) where T : Asda2Loot, new()
        {
            IList<Asda2LooterEntry> looters = Asda2LootMgr.FindLooters(lootable, initialLooter);
            float dropChanceBoost = 0.0f;
            float num1 = 0.0f;
            foreach (Asda2LooterEntry asda2LooterEntry in (IEnumerable<Asda2LooterEntry>) looters)
            {
                dropChanceBoost += asda2LooterEntry.Owner.Asda2DropChance - 1f;
                num1 += (float) ((double) num1 + (double) asda2LooterEntry.Owner.Asda2GoldAmountBoost - 1.0);
            }

            Asda2LootItem[] lootItems =
                Asda2LootMgr.CreateLootItems(lootable.GetLootId(type), type, looters, dropChanceBoost);
            float num2 = (float) lootable.LootMoney * ((float) Asda2LootMgr.DefaultMoneyDropFactor + num1);
            if (lootItems.Length == 0 && (double) num2 == 0.0 && lootable is GameObject)
                num2 = 1f;
            if (lootItems.Length <= 0 && (double) num2 <= 0.0)
                return default(T);
            T instance = Activator.CreateInstance<T>();
            instance.Lootable = lootable;
            instance.Money = (double) num2 > 4294967296.0 ? 1U : (uint) num2;
            instance.Items = lootItems;
            T obj = instance;
            foreach (Asda2LootItem asda2LootItem in lootItems)
                asda2LootItem.Loot = (Asda2Loot) obj;
            obj.Initialize(initialLooter, looters, mapid);
            return obj;
        }

        /// <summary>Generates loot for Items and GOs.</summary>
        /// <param name="lootable">The Object or Unit that is being looted</param>
        /// <returns>The object's loot or null if there is nothing to get or the given Character can't access the loot.</returns>
        public static Asda2ObjectLoot CreateAndSendObjectLoot(IAsda2Lootable lootable, Character initialLooter,
            Asda2LootEntryType type)
        {
            Asda2Loot loot1 = initialLooter.LooterEntry.Loot;
            if (loot1 != null)
                loot1.ForceDispose();
            IList<Asda2LooterEntry> looters = Asda2LootMgr.FindLooters(lootable, initialLooter);
            Asda2ObjectLoot loot2 =
                Asda2LootMgr.CreateLoot<Asda2ObjectLoot>(lootable, initialLooter, type, MapId.Silaris);
            if (loot2 != null)
            {
                initialLooter.LooterEntry.Loot = (Asda2Loot) loot2;
                loot2.Initialize(initialLooter, looters, MapId.Silaris);
            }

            return loot2;
        }

        /// <summary>
        /// Generates normal loot (usually for dead mob-corpses).
        /// Returns null, if the loot is empty.
        /// </summary>
        /// <param name="lootable">The Object or Unit that is being looted</param>
        public static Asda2Loot GetOrCreateLoot(IAsda2Lootable lootable, Character triggerChar, Asda2LootEntryType type)
        {
            Asda2Loot loot = lootable.Loot;
            if (loot != null)
            {
                if (loot.IsMoneyLooted && loot.RemainingCount == 0)
                    return (Asda2Loot) null;
                loot.Looters.Clear();
            }
            else
                lootable.Loot = loot =
                    (Asda2Loot) Asda2LootMgr.CreateLoot<Asda2NPCLoot>(lootable, triggerChar, type, MapId.Silaris);

            return loot;
        }

        /// <summary>
        /// Returns all Items that can be looted off the given lootable
        /// </summary>
        public static Asda2LootItem[] CreateLootItems(uint lootId, Asda2LootEntryType type,
            IList<Asda2LooterEntry> looters, float dropChanceBoost)
        {
            List<Asda2LootItemEntry> entries = Asda2LootMgr.GetEntries(type, lootId);
            if (entries == null)
                return Asda2LootItem.EmptyArray;
            Asda2LootItem[] array = new Asda2LootItem[Math.Min(CharacterFormulas.MaxLootCount, entries.Count)];
            int newSize = 0;
            foreach (Asda2LootItemEntry asda2LootItemEntry in entries)
            {
                if (100.0 * (double) Utility.RandomFloat() <
                    (double) (asda2LootItemEntry.DropChance * (Asda2LootMgr.LootItemDropFactor + dropChanceBoost)))
                {
                    Asda2ItemTemplate itemTemplate = asda2LootItemEntry.ItemTemplate;
                    if (itemTemplate != null)
                    {
                        array[newSize] = new Asda2LootItem(itemTemplate,
                            Utility.Random(asda2LootItemEntry.MinAmount, asda2LootItemEntry.MaxAmount), (uint) newSize);
                        ++newSize;
                        if (newSize == CharacterFormulas.MaxLootCount)
                            break;
                    }
                }
            }

            if (newSize == 0)
                return Asda2LootItem.EmptyArray;
            Array.Resize<Asda2LootItem>(ref array, newSize);
            return array;
        }

        public static IList<Asda2LooterEntry> FindLooters(IAsda2Lootable lootable, Character initialLooter)
        {
            List<Asda2LooterEntry> asda2LooterEntryList = new List<Asda2LooterEntry>();
            Asda2LootMgr.FindLooters(lootable, initialLooter, (IList<Asda2LooterEntry>) asda2LooterEntryList);
            return (IList<Asda2LooterEntry>) asda2LooterEntryList;
        }

        public static void FindLooters(IAsda2Lootable lootable, Character initialLooter,
            IList<Asda2LooterEntry> looters)
        {
            if (lootable.UseGroupLoot)
            {
                GroupMember groupMember = initialLooter.GroupMember;
                if (groupMember != null)
                {
                    Group group = groupMember.Group;
                    if (group.LootMethod == LootMethod.RoundRobin)
                    {
                        GroupMember roundRobinMember = group.GetNextRoundRobinMember();
                        if (roundRobinMember == null)
                            return;
                        looters.Add(roundRobinMember.Character.LooterEntry);
                        return;
                    }

                    group.GetNearbyLooters(lootable, (WorldObject) initialLooter,
                        (ICollection<Asda2LooterEntry>) looters);
                    return;
                }
            }

            looters.Add(initialLooter.LooterEntry);
        }

        public static List<Asda2LootItemEntry> GetEntries(this IAsda2Lootable lootable, Asda2LootEntryType type)
        {
            return Asda2LootMgr.GetEntries(type, lootable.GetLootId(type));
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
            if (!lockable.CanOpen(chr))
                return false;
            LockEntry.Loot(lockable, chr);
            return true;
        }

        /// <summary>
        /// Whether the given lootable contains quest items for the given Character when looting with the given type
        /// </summary>
        public static bool ContainsQuestItemsFor(this IAsda2Lootable lootable, Character chr, Asda2LootEntryType type)
        {
            Asda2Loot loot = lootable.Loot;
            if (loot != null)
                return ((IEnumerable<Asda2LootItem>) loot.Items).Any<Asda2LootItem>((Func<Asda2LootItem, bool>) (item =>
                {
                    if (item.Template.HasQuestRequirements)
                        return item.Template.CheckQuestConstraints(chr);
                    return false;
                }));
            List<Asda2LootItemEntry> entries = lootable.GetEntries(type);
            if (entries != null)
                return entries.Any<Asda2LootItemEntry>((Func<Asda2LootItemEntry, bool>) (entry =>
                {
                    if (entry.ItemTemplate.HasQuestRequirements)
                        return entry.ItemTemplate.CheckQuestConstraints(chr);
                    return false;
                }));
            return false;
        }

        public static void ClearLootData()
        {
            foreach (List<Asda2LootItemEntry>[] lootEntry in Asda2LootMgr.LootEntries)
            {
                if (lootEntry != null)
                {
                    for (int index = 0; index < lootEntry.Length; ++index)
                        lootEntry[index] = (List<Asda2LootItemEntry>) null;
                }
            }
        }

        public static void EnableLuckyDropEvent()
        {
            for (uint id = 0; id < 800U; ++id)
            {
                NPCEntry entry = NPCMgr.GetEntry(id);
                if (entry != null)
                {
                    Asda2NPCLootItemEntry npcLootItemEntry = new Asda2NPCLootItemEntry();
                    npcLootItemEntry.ItemId = Asda2ItemId.U9036890;
                    npcLootItemEntry.Guid = (int) id;
                    npcLootItemEntry.LootType = Asda2LootEntryType.Npc;
                    npcLootItemEntry.MonstrId = id;
                    switch (entry.Rank)
                    {
                        case CreatureRank.Normal:
                            npcLootItemEntry.DropChance = 40f;
                            npcLootItemEntry.MinAmount = 1;
                            npcLootItemEntry.MaxAmount = 5;
                            break;
                        case CreatureRank.Elite:
                            npcLootItemEntry.DropChance = 60f;
                            npcLootItemEntry.MinAmount = 2;
                            npcLootItemEntry.MaxAmount = 7;
                            break;
                        case CreatureRank.Boss:
                            npcLootItemEntry.DropChance = 80f;
                            npcLootItemEntry.MinAmount = 4;
                            npcLootItemEntry.MaxAmount = 8;
                            break;
                        case CreatureRank.WorldBoss:
                            npcLootItemEntry.DropChance = 100f;
                            npcLootItemEntry.MinAmount = 5;
                            npcLootItemEntry.MaxAmount = 10;
                            break;
                    }

                    npcLootItemEntry.FinalizeDataHolder();
                }
            }
        }
    }
}