using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Asda2Looting
{
    /// <summary>
    /// Static utility and container class for Looting.
    /// </summary>
    [GlobalMgr]
    public static class Asda2LootMgr
    {
        #region Global Variables
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
        #endregion

        public static readonly List<Asda2LootItemEntry>[][] LootEntries = new List<Asda2LootItemEntry>[(int)LootEntryType.Count][];

        #region Init & Load
        static Asda2LootMgr()
        {
            LootEntries[(int)Asda2LootEntryType.Digging] = new List<Asda2LootItemEntry>[2000];
            LootEntries[(int)Asda2LootEntryType.Disassebling] = new List<Asda2LootItemEntry>[2000];
            LootEntries[(int)Asda2LootEntryType.Fishing] = new List<Asda2LootItemEntry>[2000];
            LootEntries[(int)Asda2LootEntryType.Friends] = new List<Asda2LootItemEntry>[2000];
            LootEntries[(int)Asda2LootEntryType.Npc] = new List<Asda2LootItemEntry>[2000];
            LootEntries[(int)Asda2LootEntryType.Lotobox] = new List<Asda2LootItemEntry>[2000];
            LootEntries[(int)Asda2LootEntryType.Quest] = new List<Asda2LootItemEntry>[2000];
        }

        private static bool loaded;

        public static bool Loaded
        {
            get { return loaded; }
            private set
            {
                if (loaded = value)
                {
                    RealmServer.InitMgr.SignalGlobalMgrReady(typeof(Asda2LootMgr));
                }
            }
        }

        //#if !DEV
        [Initialization(InitializationPass.Tenth, "Load Loot")]
        //#endif
        public static void LoadAll()
        {
            /*if (!Loaded)
            {*/
            ContentMgr.Load<Asda2NPCLootItemEntry>(true);

            Loaded = true;
            /*}*/
        }

        /// <summary>
        /// Adds the new LootItemEntry to the global container.
        /// Keeps the set of entries sorted by rarity.
        /// </summary>
        public static void AddEntry(Asda2LootItemEntry entry)
        {
            var entries = LootEntries[(uint)entry.LootType];

            if (entry.LootType == Asda2LootEntryType.None)
            {
                LogUtil.WarnException(string.Format("Bad drop template in db [MonstrId{0},itemID{1},guid{2}] cause type NONE", entry.MonstrId, entry.ItemId, entry.Guid));
                return;
            }
            if (entry.MonstrId >= entries.Length)
            {
                ArrayUtil.EnsureSize(ref entries, (int)(entry.MonstrId * ArrayUtil.LoadConstant) + 1);
                LootEntries[(uint)entry.LootType] = entries;
            }

            var list = entries[entry.MonstrId];
            if (list == null)
            {
                entries[entry.MonstrId] = list = new List<Asda2LootItemEntry>();
            }

            // add entry sorted
            var added = false;
            for (var i = 0; i < list.Count; i++)
            {
                var ent = list[i];
                if (ent.DropChance > entry.DropChance)
                {
                    added = true;
                    list.Insert(i, entry);
                    break;
                }
            }

            if (!added)
            {
                list.Add(entry);
            }
        }
        #endregion

        public static List<Asda2LootItemEntry>[] GetEntries(Asda2LootEntryType type)
        {
            return LootEntries[(uint)type];
        }

        public static List<Asda2LootItemEntry> GetEntries(Asda2LootEntryType type, uint id)
        {
            var entries = LootEntries[(uint)type];
            var list = entries.Get(id);
            return list;
        }

        #region Loot Generation
        /// <summary>
        /// Creates a new Loot object and returns it or null, if there is nothing to be looted.
        /// </summary>
        /// <typeparam name="T"><see cref="ObjectLoot"/> or <see cref="NPCLoot"/></typeparam>
        /// <param name="lootable"></param>
        /// <param name="initialLooter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T CreateLoot<T>(IAsda2Lootable lootable, Character initialLooter, Asda2LootEntryType type, MapId mapid)
            where T : Asda2Loot, new()
        {
            var looters = FindLooters(lootable, initialLooter);

            var dropChanceBoost = 0f;
            var goldAmountBoost = 0f;
            foreach (var asda2LootItemEntry in looters)
            {
                dropChanceBoost += asda2LootItemEntry.Owner.Asda2DropChance - 1;
                goldAmountBoost += goldAmountBoost + asda2LootItemEntry.Owner.Asda2GoldAmountBoost - 1;
            }
            var items = CreateLootItems(lootable.GetLootId(type), type, looters, dropChanceBoost);
            var money = lootable.LootMoney * (DefaultMoneyDropFactor + goldAmountBoost);
            if (items.Length == 0 && money == 0)
            {
                if (lootable is GameObject)
                {
                    // TODO: Don't mark GO as lootable if it has nothing to loot
                    money = 1;
                }
            }

            if (items.Length > 0 || money > 0)
            {
                var loot = new T { Lootable = lootable, Money = (uint)(money > UInt32.MaxValue ? 1 : money), Items = items };
                foreach (var asda2LootItem in items)
                {
                    asda2LootItem.Loot = loot;
                }
                loot.Initialize(initialLooter, looters, mapid);
                return loot;
            }
            else
            {
                //var loot = new T { Lootable = lootable, Money = 1, Items = LootItem.EmptyArray };
                //loot.Initialize(initialLooter, looters);
                //return loot;
                return null;
            }
        }

        /// <summary>
        /// Generates loot for Items and GOs. 
        /// </summary>
        /// <param name="lootable">The Object or Unit that is being looted</param>
        /// <returns>The object's loot or null if there is nothing to get or the given Character can't access the loot.</returns>
        public static Asda2ObjectLoot CreateAndSendObjectLoot(IAsda2Lootable lootable, Character initialLooter,
            Asda2LootEntryType type)
        {
            var oldLoot = initialLooter.LooterEntry.Loot;
            if (oldLoot != null)
            {
                oldLoot.ForceDispose();
            }
            var looters = FindLooters(lootable, initialLooter);

            var loot = CreateLoot<Asda2ObjectLoot>(lootable, initialLooter, type, 0); // TODO: pass mapid
            if (loot != null)
            {
                initialLooter.LooterEntry.Loot = loot;
                loot.Initialize(initialLooter, looters, 0); // TODO: pass mapid
                //LootHandler.SendLootResponse(initialLooter, loot);
            }
            else
            {
                //lootable.OnFinishedLooting();
                // empty Item -> Don't do anything
            }
            return loot;
        }

        /// <summary>
        /// Generates normal loot (usually for dead mob-corpses). 
        /// Returns null, if the loot is empty.
        /// </summary>
        /// <param name="lootable">The Object or Unit that is being looted</param>
        public static Asda2Loot GetOrCreateLoot(IAsda2Lootable lootable, Character triggerChar, Asda2LootEntryType type)
        {
            var loot = lootable.Loot;
            if (loot != null)
            {
                // apparently mob got killed a 2nd time
                if (loot.IsMoneyLooted && loot.RemainingCount == 0)
                {
                    // already looted empty
                    return null;
                }
                loot.Looters.Clear();
            }
            else
            {
                lootable.Loot = loot = CreateLoot<Asda2NPCLoot>(lootable, triggerChar, type, 0); // TODO: pass mapid
            }

            return loot;
        }

        /// <summary>
        /// Returns all Items that can be looted off the given lootable
        /// </summary>
        public static Asda2LootItem[] CreateLootItems(uint lootId, Asda2LootEntryType type, IList<Asda2LooterEntry> looters, float dropChanceBoost)
        {
#if DEBUG
            if (!Asda2ItemMgr.Loaded)
            {
                return Asda2LootItem.EmptyArray;
            }
#endif
            var entries = GetEntries(type, lootId);
            if (entries == null)
            {
                return Asda2LootItem.EmptyArray;
            }
            var items = new Asda2LootItem[Math.Min(CharacterFormulas.MaxLootCount, entries.Count)];
            //var i = max;
            var i = 0;
            foreach (var entry in entries)
            {
                var chance = entry.DropChance * (LootItemDropFactor + dropChanceBoost);
                if ((100 * Utility.RandomFloat()) >= chance) continue;

                var template = entry.ItemTemplate;
                if (template == null)
                {
                    // weird
                    continue;
                }

                /*if (!looters.Any(looter => template.CheckLootConstraints(looter.Owner)))
                {
                    continue;
                }*/

                items[i] = new Asda2LootItem(template,
                                        Utility.Random(entry.MinAmount, entry.MaxAmount),
                                        (uint)i);
                i++;

                if (i == CharacterFormulas.MaxLootCount)
                {
                    break;
                }
            }

            if (i == 0)
            {
                return Asda2LootItem.EmptyArray;
            }

            Array.Resize(ref items, i);
            return items;
        }
        #endregion

        #region FindLooters
        public static IList<Asda2LooterEntry> FindLooters(IAsda2Lootable lootable, Character initialLooter)
        {
            var looters = new List<Asda2LooterEntry>();
            FindLooters(lootable, initialLooter, looters);
            return looters;
        }

        public static void FindLooters(IAsda2Lootable lootable, Character initialLooter, IList<Asda2LooterEntry> looters)
        {
            if (lootable.UseGroupLoot)
            {
                var groupMember = initialLooter.GroupMember;
                if (groupMember != null)
                {
                    var group = groupMember.Group;
                    var method = group.LootMethod;
                    var usesRoundRobin = method == LootMethod.RoundRobin;

                    if (usesRoundRobin)
                    {
                        var member = group.GetNextRoundRobinMember();
                        if (member != null)
                        {
                            looters.Add(member.Character.LooterEntry);
                        }
                    }
                    else
                    {
                        group.GetNearbyLooters(lootable, initialLooter, looters);
                    }
                    return;
                }
            }

            looters.Add(initialLooter.LooterEntry);
        }
        #endregion

        #region Extension methods
        public static List<Asda2LootItemEntry> GetEntries(this IAsda2Lootable lootable, Asda2LootEntryType type)
        {
            return GetEntries(type, lootable.GetLootId(type));
        }

        /// <summary>
        /// Returns whether this lockable can be opened by the given Character
        /// </summary>
        /// <param name="lockable"></param>
        /// <returns></returns>
        public static bool CanOpen(this ILockable lockable, Character chr)
        {
            /*var lck = lockable.Lock;

            if (lck != null && !lck.IsUnlocked && lck.Keys.Length > 0)
            {
                // chests may only be opened if they are unlocked or we have a key
                // Skill-related opening is handled through spells
                var found = false;
                for (var i = 0; i < lck.Keys.Length; i++)
                {
                    var key = lck.Keys[i];
                    if (chr.Inventory.KeyRing.Contains(key.KeyId))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }*/
            return true;
        }

        public static bool TryLoot(this ILockable lockable, Character chr)
        {
            if (CanOpen(lockable, chr))
            {
                // just open it
                LockEntry.Loot(lockable, chr);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Whether the given lootable contains quest items for the given Character when looting with the given type
        /// </summary>
        public static bool ContainsQuestItemsFor(this IAsda2Lootable lootable, Character chr, Asda2LootEntryType type)
        {
            var loot = lootable.Loot;
            if (loot != null)
            {
                // loot has already been created
                return loot.Items.Any(item => item.Template.HasQuestRequirements && item.Template.CheckQuestConstraints(chr));
            }

            // no loot yet -> check what happens if we create any
            var entries = lootable.GetEntries(type);
            if (entries != null)
            {
                return entries.Any(entry => entry.ItemTemplate.HasQuestRequirements && entry.ItemTemplate.CheckQuestConstraints(chr));
            }
            return false;
        }
        #endregion

        public static void ClearLootData()
        {
            foreach (var t in LootEntries)
            {
                if (t == null)
                    continue;
                for (var j = 0; j < t.Length; j++)
                {
                    t[j] = null;
                }
            }
        }

        public static void EnableLuckyDropEvent()
        {
            for (uint i = 0; i < 800; i++)
            {
                var npcTemplate = NPCMgr.GetEntry(i);
                if(npcTemplate == null)continue;
                var newLootEntry = new Asda2NPCLootItemEntry();
                newLootEntry.ItemId = (Asda2ItemId) 36842;
                newLootEntry.Guid = (int)i;
                newLootEntry.LootType = Asda2LootEntryType.Npc;
                newLootEntry.MonstrId = i;
                switch (npcTemplate.Rank)
                {
                    case CreatureRank.Normal:
                        newLootEntry.DropChance = 6;
                        newLootEntry.MinAmount = 1;
                        newLootEntry.MaxAmount = 1;
                        
                        break;
                    case CreatureRank.Elite:
                        newLootEntry.DropChance = 6;
                        newLootEntry.MinAmount = 1;
                        newLootEntry.MaxAmount = 1;
                        break;
                    case CreatureRank.Boss:
                        newLootEntry.DropChance = 6;
                        newLootEntry.MinAmount = 1;
                        newLootEntry.MaxAmount = 1;
                        break;
                    case CreatureRank.WorldBoss:
                        newLootEntry.DropChance = 6;
                        newLootEntry.MinAmount = 1;
                        newLootEntry.MaxAmount = 1;
                        break;
                }
                newLootEntry.FinalizeDataHolder();
            }
        }
    }
}