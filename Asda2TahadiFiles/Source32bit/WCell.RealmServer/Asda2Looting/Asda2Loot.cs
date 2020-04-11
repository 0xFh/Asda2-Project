using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Logs;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Looting
{
    /// <summary>
    /// Represents a pile of lootable objects and its looters
    /// 
    /// TODO: Roll timeout (and loot timeout?)
    /// </summary>
    public class Asda2Loot : WorldObject
    {
        public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Loot);

        /// <summary>
        /// The set of all who are allowed to loot.
        /// If everyone released the Loot, it becomes available to everyone else?
        /// </summary>
        public IList<Asda2LooterEntry> Looters;

        private uint m_Money;

        /// <summary>All items that can be looted.</summary>
        public Asda2LootItem[] Items;

        /// <summary>The Container being looted</summary>
        public IAsda2Lootable Lootable;

        /// <summary>
        /// The method that determines how to distribute the Items
        /// </summary>
        public LootMethod Method;

        /// <summary>
        /// The Group who is looting this Loot.
        /// If all members of the group release it, the Loot becomes available to everyone else.
        /// </summary>
        public Group Group;

        protected int m_takenCount;

        /// <summary>Amount of items that are freely available</summary>
        protected int m_freelyAvailableCount;

        /// <summary>Whether money was already looted</summary>
        protected bool m_moneyLooted;

        /// <summary>
        /// Whether none of the initial looters is still claiming this.
        /// </summary>
        protected bool m_released;

        /// <summary>
        /// The least ItemQuality that is decided through rolls/MasterLooter correspondingly.
        /// </summary>
        public ItemQuality Threshold;

        public short? MonstrId;

        /// <summary>The total amount of money to be looted</summary>
        public uint Money
        {
            get { return this.m_Money; }
            set
            {
                this.m_Money = value;
                this.m_moneyLooted = this.m_Money == 0U;
            }
        }

        public Asda2Loot()
        {
            this.Looters = (IList<Asda2LooterEntry>) new List<Asda2LooterEntry>();
            this.SpawnTime = DateTime.Now;
        }

        public DateTime SpawnTime { get; set; }

        protected Asda2Loot(IAsda2Lootable looted, uint money, Asda2LootItem[] items)
            : this()
        {
            this.Money = money;
            this.Items = items;
            this.Lootable = looted;
        }

        /// <summary>The amount of Items that have already been taken</summary>
        public int TakenCount
        {
            get { return this.m_takenCount; }
        }

        /// <summary>Amount of remaining items</summary>
        public int RemainingCount
        {
            get { return this.Items.Length - this.m_takenCount; }
        }

        /// <summary>
        /// Amount of items that are freely available to everyone:
        /// Items that are passed by everyone or that have been left over by the looter whose turn it is in RoundRobin
        /// </summary>
        public int FreelyAvailableCount
        {
            get { return this.m_freelyAvailableCount; }
            internal set { this.m_freelyAvailableCount = value; }
        }

        /// <summary>
        /// Whether RoundRobin applies (by default applies if LootMethod == RoundRobin or -for items below threshold- when using most of the other methods too)
        /// </summary>
        public bool UsesRoundRobin
        {
            get { return this.Method == LootMethod.RoundRobin; }
        }

        /// <summary>
        /// Whether none of the initial looters is still looking at this (everyone else may thus look at it)
        /// </summary>
        public bool IsReleased
        {
            get { return this.m_released; }
            internal set
            {
                if (this.m_released == value)
                    return;
                this.m_released = value;
                if (!value || this.RemainingCount != 0 || !this.m_moneyLooted)
                    return;
                this.Dispose();
            }
        }

        /// <summary>Whether the money has already been given out</summary>
        public bool IsMoneyLooted
        {
            get { return this.m_moneyLooted; }
        }

        public bool MustKneelWhileLooting
        {
            get { return this.Lootable is WorldObject; }
        }

        public bool IsGroupLoot
        {
            get { return this.Lootable.UseGroupLoot; }
        }

        public virtual LootResponseType ResponseType { get; set; }

        public Vector2Short[] LootPositions { get; set; }

        public bool IsAllItemsTaken
        {
            get
            {
                if (this.Items != null)
                    return ((IEnumerable<Asda2LootItem>) this.Items).All<Asda2LootItem>(
                        (Func<Asda2LootItem, bool>) (i => i.Taken));
                return true;
            }
        }

        /// <summary>
        /// Adds all initial Looters of nearby Characters who may loot this Loot.
        /// When all of the initial Looters gave up the Loot, the Loot becomes free for all.
        /// </summary>
        public void Initialize(Character chr, IList<Asda2LooterEntry> looters, MapId mapid)
        {
            this.Looters = looters;
            this.AutoLoot = chr.AutoLoot;
            if (this.IsGroupLoot)
            {
                GroupMember groupMember = chr.GroupMember;
                if (groupMember != null)
                {
                    this.Group = groupMember.Group;
                    this.Method = this.Group.LootMethod;
                    this.Threshold = this.Group.LootThreshold;
                    return;
                }
            }

            this.Method = LootMethod.FreeForAll;
        }

        /// <summary>
        /// This gives the money to everyone involved. Will only work the first time its called.
        /// Afterwards <c>IsMoneyLooted</c> will be true.
        /// </summary>
        public void GiveMoney()
        {
            if (!(this.Lootable is NPC) || this.m_moneyLooted)
                return;
            if (this.Group == null)
            {
                Asda2LooterEntry asda2LooterEntry = this.Looters.FirstOrDefault<Asda2LooterEntry>();
                if (asda2LooterEntry != null && asda2LooterEntry.Owner != null)
                {
                    this.m_moneyLooted = true;
                    if (asda2LooterEntry.Owner.Level < ((Unit) this.Lootable).Level + 6)
                        this.SendMoney(asda2LooterEntry.Owner, this.Money);
                }
            }
            else
            {
                List<Character> characterList = new List<Character>();
                if (this.UsesRoundRobin)
                {
                    Asda2LooterEntry asda2LooterEntry = this.Looters.FirstOrDefault<Asda2LooterEntry>();
                    if (asda2LooterEntry != null && asda2LooterEntry.Owner != null)
                    {
                        characterList.Add(asda2LooterEntry.Owner);
                        foreach (Character objectsInRadiu in (IEnumerable<WorldObject>) (!(this.Lootable is WorldObject)
                                ? (WorldObject) asda2LooterEntry.Owner
                                : (WorldObject) this.Lootable)
                            .GetObjectsInRadius<WorldObject>(Asda2LootMgr.LootRadius, ObjectTypes.Player, false, 0))
                        {
                            GroupMember groupMember;
                            if (objectsInRadiu.IsAlive &&
                                (objectsInRadiu == asda2LooterEntry.Owner ||
                                 (groupMember = objectsInRadiu.GroupMember) != null && groupMember.Group == this.Group))
                                characterList.Add(objectsInRadiu);
                        }
                    }
                }
                else
                {
                    foreach (Asda2LooterEntry looter in (IEnumerable<Asda2LooterEntry>) this.Looters)
                    {
                        if (looter.m_owner != null)
                            characterList.Add(looter.m_owner);
                    }
                }

                if (characterList.Count > 0)
                {
                    this.m_moneyLooted = true;
                    uint amount = this.Money / (uint) characterList.Count;
                    foreach (Character receiver in characterList)
                    {
                        if (receiver.Level < ((Unit) this.Lootable).Level + 6)
                            this.SendMoney(receiver, amount);
                    }
                }
            }

            this.CheckFinished();
        }

        /// <summary>
        /// This gives items to everyone involved. Will only work the first time its called.
        /// Afterwards <c>IsMoneyLooted</c> will be true.
        /// </summary>
        public bool GiveItems()
        {
            if (!(this.Lootable is NPC))
                return false;
            if (this.Group == null)
            {
                Asda2LooterEntry asda2LooterEntry = this.Looters.FirstOrDefault<Asda2LooterEntry>();
                if (asda2LooterEntry != null && asda2LooterEntry.Owner != null)
                {
                    foreach (Asda2LootItem asda2LootItem in this.Items)
                    {
                        Asda2Item asda2Item = (Asda2Item) null;
                        Asda2InventoryError asda2InventoryError =
                            asda2LooterEntry.Owner.Asda2Inventory.TryAdd((int) asda2LootItem.Template.ItemId,
                                asda2LootItem.Amount, true, ref asda2Item, new Asda2InventoryType?(), (Asda2Item) null);
                        Log.Create(Log.Types.ItemOperations, LogSourceType.Character, asda2LooterEntry.Owner.EntryId)
                            .AddAttribute("source", 0.0, "loot").AddItemAttributes(asda2Item, "")
                            .AddAttribute("map", (double) asda2LooterEntry.Owner.MapId,
                                asda2LooterEntry.Owner.MapId.ToString())
                            .AddAttribute("x", (double) asda2LooterEntry.Owner.Asda2Position.X, "")
                            .AddAttribute("y", (double) asda2LooterEntry.Owner.Asda2Position.Y, "")
                            .AddAttribute("monstrId",
                                (double) (this.MonstrId.HasValue ? this.MonstrId : new short?((short) 0)).Value, "")
                            .Write();
                        if (asda2InventoryError != Asda2InventoryError.Ok)
                        {
                            Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.NoSpace,
                                (Asda2Item) null, asda2LooterEntry.Owner);
                            break;
                        }

                        Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Ok, asda2Item,
                            asda2LooterEntry.Owner);
                        if (asda2Item.Template.Quality >= Asda2ItemQuality.Green)
                            ChatMgr.SendGlobalMessageResponse(asda2LooterEntry.Owner.Name,
                                ChatMgr.Asda2GlobalMessageType.HasObinedItem, asda2Item.ItemId, (short) 0, (short) 0);
                        asda2LootItem.Taken = true;
                    }
                }
            }
            else
            {
                List<Character> characterList = new List<Character>();
                if (this.UsesRoundRobin)
                {
                    Asda2LooterEntry asda2LooterEntry = this.Looters.FirstOrDefault<Asda2LooterEntry>();
                    if (asda2LooterEntry != null && asda2LooterEntry.Owner != null)
                    {
                        characterList.Add(asda2LooterEntry.Owner);
                        foreach (Character objectsInRadiu in (IEnumerable<WorldObject>) (!(this.Lootable is WorldObject)
                                ? (WorldObject) asda2LooterEntry.Owner
                                : (WorldObject) this.Lootable)
                            .GetObjectsInRadius<WorldObject>(Asda2LootMgr.LootRadius, ObjectTypes.Player, false, 0))
                        {
                            GroupMember groupMember;
                            if (objectsInRadiu.IsAlive &&
                                (objectsInRadiu == asda2LooterEntry.Owner ||
                                 (groupMember = objectsInRadiu.GroupMember) != null && groupMember.Group == this.Group))
                                characterList.Add(objectsInRadiu);
                        }
                    }
                }
                else
                {
                    foreach (Asda2LooterEntry looter in (IEnumerable<Asda2LooterEntry>) this.Looters)
                    {
                        if (looter.m_owner != null)
                            characterList.Add(looter.m_owner);
                    }
                }

                if (characterList.Count > 0)
                {
                    List<List<Asda2LootItem>> asda2LootItemListList = new List<List<Asda2LootItem>>();
                    int index1 = 0;
                    if (characterList.Count == 1)
                    {
                        asda2LootItemListList.Add(new List<Asda2LootItem>((IEnumerable<Asda2LootItem>) this.Items));
                    }
                    else
                    {
                        foreach (Character character in characterList)
                            asda2LootItemListList.Add(new List<Asda2LootItem>());
                        foreach (Asda2LootItem asda2LootItem in this.Items)
                        {
                            int index2 = Utility.Random(0, characterList.Count - 1);
                            asda2LootItemListList[index2].Add(asda2LootItem);
                        }
                    }

                    foreach (Character chr in characterList)
                    {
                        foreach (Asda2LootItem asda2LootItem in asda2LootItemListList[index1])
                        {
                            Asda2Item asda2Item = (Asda2Item) null;
                            Asda2InventoryError asda2InventoryError =
                                chr.Asda2Inventory.TryAdd((int) asda2LootItem.Template.ItemId, asda2LootItem.Amount,
                                    true, ref asda2Item, new Asda2InventoryType?(), (Asda2Item) null);
                            Log.Create(Log.Types.ItemOperations, LogSourceType.Character, chr.EntryId)
                                .AddAttribute("source", 0.0, "loot").AddItemAttributes(asda2Item, "")
                                .AddAttribute("map", (double) chr.MapId, chr.MapId.ToString())
                                .AddAttribute("x", (double) chr.Asda2Position.X, "")
                                .AddAttribute("y", (double) chr.Asda2Position.Y, "").AddAttribute("monstrId",
                                    (double) (this.MonstrId.HasValue ? this.MonstrId : new short?((short) 0)).Value, "")
                                .Write();
                            if (asda2InventoryError != Asda2InventoryError.Ok)
                            {
                                Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.NoSpace,
                                    (Asda2Item) null, chr);
                                break;
                            }

                            Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Ok, asda2Item, chr);
                            asda2LootItem.Taken = true;
                        }

                        ++index1;
                    }
                }
            }

            if (!this.IsAllItemsTaken)
                return false;
            this.Dispose();
            return true;
        }

        /// <summary>
        /// Gives the receiver the money and informs everyone else
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="amount"></param>
        protected void SendMoney(Character receiver, uint amount)
        {
            Asda2InventoryHandler.SendGoldPickupedResponse(receiver.Money + amount, receiver);
            receiver.AddMoney(amount);
        }

        /// <summary>
        /// Checks whether this Loot has been fully looted and if so, dispose and dismember the corpse or consumable object
        /// </summary>
        public void CheckFinished()
        {
            if (!this.m_moneyLooted || this.m_takenCount != this.Items.Length)
                return;
            this.Dispose();
        }

        /// <summary>
        /// Returns whether the given looter may loot the given Item.
        /// Make sure the Looter is logged in before calling this Method.
        /// 
        /// TODO: Find the right error messages
        /// TODO: Only give every MultiLoot item to everyone once! Also check for quest-dependencies etc.
        /// </summary>
        public InventoryError CheckTakeItemConditions(Asda2LooterEntry looter, Asda2LootItem item)
        {
            if (item.Taken)
                return InventoryError.ALREADY_LOOTED;
            if (!looter.MayLoot(this))
                return InventoryError.DontReport;
            ICollection<Asda2LooterEntry> multiLooters = item.MultiLooters;
            if (multiLooters != null)
            {
                if (multiLooters.Contains(looter))
                    return InventoryError.OK;
                if (looter.Owner != null)
                    LootHandler.SendLootRemoved(looter.Owner, item.Index);
                return InventoryError.DONT_OWN_THAT_ITEM;
            }

            if (!item.Template.CheckLootConstraints(looter.Owner))
                return InventoryError.DONT_OWN_THAT_ITEM;
            int method = (int) this.Method;
            return InventoryError.OK;
        }

        /// <summary>
        /// Marks the given Item as taken and removes it from the list of available Items
        /// </summary>
        /// <param name="lootItem"></param>
        public void RemoveItem(LootItem lootItem)
        {
            lootItem.Taken = true;
            ++this.m_takenCount;
            foreach (Asda2LooterEntry looter in (IEnumerable<Asda2LooterEntry>) this.Looters)
            {
                if (looter.Owner != null)
                    LootHandler.SendLootRemoved(looter.Owner, lootItem.Index);
            }

            this.CheckFinished();
        }

        /// <summary>
        /// Disposes this loot, despite the fact that it could still contain something valuable
        /// </summary>
        public void ForceDispose()
        {
            this.Dispose();
        }

        protected override UpdateFieldCollection _UpdateFieldInfos
        {
            get { return Asda2Loot.UpdateFieldInfos; }
        }

        public override UpdateFlags UpdateFlags
        {
            get { return UpdateFlags.StationaryObject; }
        }

        public override ObjectTypeId ObjectTypeId
        {
            get { return ObjectTypeId.Loot; }
        }

        public new void Dispose()
        {
            if (this.Map != null)
                this.Map.Loots.Remove(this);
            this.OnDispose();
            this.Dispose(true);
        }

        public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
        {
            get { throw new NotImplementedException(); }
        }

        protected virtual void OnDispose()
        {
            if (this.Lootable == null)
                return;
            this.Lootable.OnFinishedLooting();
            this.Lootable = (IAsda2Lootable) null;
        }

        public void RemoveLooter(Asda2LooterEntry entry)
        {
            this.Looters.Remove(entry);
        }

        public override string Name
        {
            get
            {
                if (this.Template != null)
                    return this.Template.ToString();
                return "";
            }
            set { }
        }

        public override Faction Faction
        {
            get { return FactionMgr.AlliancePlayerFactions[0]; }
            set { }
        }

        public override FactionId FactionId
        {
            get { return FactionMgr.AlliancePlayerFactions[0].Id; }
            set { }
        }

        public bool AutoLoot { get; set; }
    }
}