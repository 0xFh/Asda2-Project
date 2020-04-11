using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.NLog;

namespace WCell.RealmServer.Looting
{
	/// <summary>
	/// Represents a pile of lootable objects and its looters
	/// 
	/// TODO: Roll timeout (and loot timeout?)
	/// </summary>
	public class Asda2Loot : WorldObject
	{
		/// <summary>
		/// The set of all who are allowed to loot. 
		/// If everyone released the Loot, it becomes available to everyone else?
		/// </summary>
		public IList<Asda2LooterEntry> Looters;

		private uint m_Money;

		/// <summary>
		/// The total amount of money to be looted
		/// </summary>
		public uint Money
		{
			get { return m_Money; }
			set
			{
				m_Money = value;
				m_moneyLooted = m_Money == 0;
			}
		}

		/// <summary>
		/// All items that can be looted.
		/// </summary>
		public Asda2LootItem[] Items;

		/// <summary>
		/// The Container being looted
		/// </summary>
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

		/// <summary>
		/// Amount of items that are freely available
		/// </summary>
		protected int m_freelyAvailableCount;

		/// <summary>
		/// Whether money was already looted
		/// </summary>
		protected bool m_moneyLooted;

		/// <summary>
		/// Whether none of the initial looters is still claiming this.
		/// </summary>
		protected bool m_released;

		/// <summary>
		/// The least ItemQuality that is decided through rolls/MasterLooter correspondingly.
		/// </summary>
		public ItemQuality Threshold;

		public Asda2Loot()
		{
			Looters = new List<Asda2LooterEntry>();
		    SpawnTime = DateTime.Now;
		}

	    public DateTime SpawnTime { get; set; }

	    protected Asda2Loot(IAsda2Lootable looted, uint money, Asda2LootItem[] items)
			: this()
		{
			Money = money;
			Items = items;
			Lootable = looted;
		}

		#region Properties
		/// <summary>
		/// The amount of Items that have already been taken
		/// </summary>
		public int TakenCount
		{
			get
			{
				return m_takenCount;
			}
		}

		/// <summary>
		/// Amount of remaining items
		/// </summary>
		public int RemainingCount
		{
			get
			{
				return Items.Length - m_takenCount;
			}
		}

		/// <summary>
		/// Amount of items that are freely available to everyone:
		/// Items that are passed by everyone or that have been left over by the looter whose turn it is in RoundRobin
		/// </summary>
		public int FreelyAvailableCount
		{
			get
			{
				return m_freelyAvailableCount;
			}
			internal set
			{
				m_freelyAvailableCount = value;
			}
		}

		/// <summary>
		/// Whether RoundRobin applies (by default applies if LootMethod == RoundRobin or -for items below threshold- when using most of the other methods too)
		/// </summary>
		public bool UsesRoundRobin
		{
			get { return Method == LootMethod.RoundRobin; }
		}

		/// <summary>
		/// Whether none of the initial looters is still looking at this (everyone else may thus look at it)
		/// </summary>
		public bool IsReleased
		{
			get
			{
				return m_released;
			}
			internal set
			{
				if (m_released != value)
				{
					m_released = value;
					if (value)
					{
						if (RemainingCount == 0 && m_moneyLooted)
						{
							// last looter released and there are no more Items left
							Dispose();
						}
					}
				}
			}
		}

		/// <summary>
		/// Whether the money has already been given out
		/// </summary>
		public bool IsMoneyLooted
		{
			get
			{
				return m_moneyLooted;
			}
		}

		public bool MustKneelWhileLooting
		{
			get
			{
				return Lootable is WorldObject;
			}
		}

		public bool IsGroupLoot { get { return Lootable.UseGroupLoot; } }

        public virtual LootResponseType ResponseType { get; set; }

        public Vector2Short[] LootPositions { get; set; }

	    public bool IsAllItemsTaken
	    {
            get { return Items == null || Items.All(i => i.Taken); }
	    }

	    #endregion

		/// <summary>
		/// Adds all initial Looters of nearby Characters who may loot this Loot.
		/// When all of the initial Looters gave up the Loot, the Loot becomes free for all.
		/// </summary>
		public void Initialize(Character chr, IList<Asda2LooterEntry> looters, MapId mapid)
		{
			Looters = looters;
            AutoLoot = chr.AutoLoot;
			if (IsGroupLoot)
			{
				var groupMember = chr.GroupMember;
				if (groupMember != null)
				{
					Group = groupMember.Group;
					Method = Group.LootMethod;
					Threshold = Group.LootThreshold;

					
					return;
				}
			}
			Method = LootMethod.FreeForAll;
		}


		/// <summary>
		/// This gives the money to everyone involved. Will only work the first time its called. 
		/// Afterwards <c>IsMoneyLooted</c> will be true.
		/// </summary>
		public void GiveMoney()
		{
            if(!(Lootable is NPC))
            return;
			if (!m_moneyLooted)
			{
				if (Group == null)
				{
					// we only have a single looter
					var looter = Looters.FirstOrDefault();
					if (looter != null && looter.Owner != null)
					{
                        m_moneyLooted = true; 
                        if (looter.Owner.Level < ((NPC)Lootable).Level + 6)
                            SendMoney(looter.Owner, Money);
					}
				}
				else
				{
					var looters = new List<Character>();
					if (UsesRoundRobin)
					{
						// we only added the RoundRobin member, so we have to find everyone in the radius for the money now

						var looter = Looters.FirstOrDefault();
						if (looter != null && looter.Owner != null)
						{
							looters.Add(looter.Owner);

							WorldObject center;
							if (Lootable is WorldObject)
							{
								center = (WorldObject)Lootable;
							}
							else
							{
								center = looter.Owner;
							}

							GroupMember otherMember;
							var chrs = center.GetObjectsInRadius(Asda2LootMgr.LootRadius, ObjectTypes.Player, false, 0);
							foreach (Character chr in chrs)
							{
								if (chr.IsAlive && (chr == looter.Owner ||
									((otherMember = chr.GroupMember) != null && otherMember.Group == Group)))
								{
									looters.Add(chr);
								}
							}
						}
					}
					else
					{
						foreach (var looter in Looters)
						{
							if (looter.m_owner != null)
							{
								looters.Add(looter.m_owner);
							}
						}
					}

					if (looters.Count > 0)
					{
						m_moneyLooted = true;

						var amount = Money / (uint)looters.Count;
						foreach (var looter in looters)
						{
                            if(looter.Level < ((NPC)Lootable).Level + 6)
                                SendMoney(looter, amount);
						}
					}
				}
				CheckFinished();
			}
		}
        
        /// <summary>
        /// This gives items to everyone involved. Will only work the first time its called. 
        /// Afterwards <c>IsMoneyLooted</c> will be true.
        /// </summary>
        public bool GiveItems()
        {
            if (!(Lootable is NPC))
                return false;
            if (Group == null)
            {
                // we only have a single looter
                var looter = Looters.FirstOrDefault();
                if (looter != null && looter.Owner != null)
                {
                   // var items = new List<Asda2Item>();
                    foreach (var asda2LootItem in Items)
                    {
                        Asda2Item item = null;
                        var result = looter.Owner.Asda2Inventory.TryAdd((int)asda2LootItem.Template.ItemId, asda2LootItem.Amount,
                                                                        true,
                                                                        ref item);
                        Log.Create(Log.Types.ItemOperations, LogSourceType.Character, looter.Owner.EntryId)
                                                     .AddAttribute("source", 0, "loot")
                                                     .AddItemAttributes(item)
                                                     .AddAttribute("map", (double)looter.Owner.MapId, looter.Owner.MapId.ToString())
                                                     .AddAttribute("x", looter.Owner.Asda2Position.X)
                                                     .AddAttribute("y", looter.Owner.Asda2Position.Y)
                                                     .AddAttribute("monstrId",(double) (MonstrId.HasValue?MonstrId:0))
                                                     .Write();
                        
                        if (result != Asda2InventoryError.Ok)
                        {
                            Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.NoSpace, null,looter.Owner);
                            break;
                        }
                        Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Ok, item,looter.Owner);
                        if(item.Template.Quality>=Asda2ItemQuality.Green)
                            ChatMgr.SendGlobalMessageResponse(looter.Owner.Name,ChatMgr.Asda2GlobalMessageType.HasObinedItem,item.ItemId);
                        // items.Add(item);
                        asda2LootItem.Taken = true;
                    }
                    /*if (items.Count != 0)
                        Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, looter.Owner, items.ToArray());*/

                }
            }
            else
            {
                var looters = new List<Character>();
                if (UsesRoundRobin)
                {
                    // we only added the RoundRobin member, so we have to find everyone in the radius for the money now

                    var looter = Looters.FirstOrDefault();
                    if (looter != null && looter.Owner != null)
                    {
                        looters.Add(looter.Owner);

                        WorldObject center;
                        if (Lootable is WorldObject)
                        {
                            center = (WorldObject) Lootable;
                        }
                        else
                        {
                            center = looter.Owner;
                        }

                        GroupMember otherMember;
                        var chrs = center.GetObjectsInRadius(Asda2LootMgr.LootRadius, ObjectTypes.Player, false, 0);
                        foreach (Character chr in chrs)
                        {
                            if (chr.IsAlive && (chr == looter.Owner ||
                                                ((otherMember = chr.GroupMember) != null && otherMember.Group == Group)))
                            {
                                looters.Add(chr);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var looter in Looters)
                    {
                        if (looter.m_owner != null)
                        {
                            looters.Add(looter.m_owner);
                        }
                    }
                }

                if (looters.Count > 0)
                {
                    var loots = new List<List<Asda2LootItem>>();
                    var looterIndex = 0;
                    if (looters.Count == 1)
                        loots.Add(new List<Asda2LootItem>(Items));
                    else
                    {
                        foreach (var t in looters)
                            loots.Add(new List<Asda2LootItem>());
                        foreach (var asda2LootItem in Items)
                        {
                            var index = Utility.Random(0, looters.Count - 1);
                            loots[index].Add(asda2LootItem);
                        }
                    }
                    foreach (var looter in looters)
                    {
                        /*var items = new List<Asda2Item>();*/
                        foreach (var asda2LootItem in loots[looterIndex])
                        {
                            Asda2Item item = null;
                            var result = looter.Asda2Inventory.TryAdd((int)asda2LootItem.Template.ItemId, asda2LootItem.Amount,
                                                                            true,
                                                                            ref item);
                            Log.Create(Log.Types.ItemOperations, LogSourceType.Character, looter.EntryId)
                                                     .AddAttribute("source", 0, "loot")
                                                     .AddItemAttributes(item)
                                                     .AddAttribute("map", (double)looter.MapId, looter.MapId.ToString())
                                                     .AddAttribute("x", looter.Asda2Position.X)
                                                     .AddAttribute("y", looter.Asda2Position.Y)
                                                     .AddAttribute("monstrId", (double)(MonstrId.HasValue ? MonstrId : 0))
                                                     .Write();
                            if (result != Asda2InventoryError.Ok)
                            {
                                Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.NoSpace, null,looter);
                                break;
                            }
                            else
                                Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Ok, item, looter);
                            //items.Add(item);
                            asda2LootItem.Taken = true;
                        }
                        /*if (items.Count != 0)
                            Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, looter,
                                                                      items.ToArray());*/
                        looterIndex++;
                    }
                }
            }
            if (IsAllItemsTaken)
            {
                Dispose();
                return true;
            }
            return false;
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
            Asda2TitleChecker.OnGoldPickUp(receiver);
		    //LootHandler.SendClearMoney(this);
		}

		/// <summary>
		/// Checks whether this Loot has been fully looted and if so, dispose and dismember the corpse or consumable object
		/// </summary>
		public void CheckFinished()
		{
			if (m_moneyLooted && m_takenCount == Items.Length)
			{
				Dispose();
			}
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
			{
				return InventoryError.ALREADY_LOOTED;
			}
			if (!looter.MayLoot(this))
			{
				return InventoryError.DontReport;
			}

			var multiLooters = item.MultiLooters;
			if (multiLooters != null)
			{
				if (!multiLooters.Contains(looter))
				{
					if (looter.Owner != null)
					{
						// make sure, Item cannot be seen by client anymore
						LootHandler.SendLootRemoved(looter.Owner, item.Index);
					}
					return InventoryError.DONT_OWN_THAT_ITEM;
				}
				return InventoryError.OK;
			}

			if (!item.Template.CheckLootConstraints(looter.Owner))
			{
				return InventoryError.DONT_OWN_THAT_ITEM;
			}

			if (Method != LootMethod.FreeForAll)
			{
				/*// definitely Group-Loot
				if ((item.Template.Quality > Group.LootThreshold && !item.Passed) ||
					(Group.MasterLooter != null &&
					 Group.MasterLooter != looter.Owner.GroupMember))
				{
					return InventoryError.DONT_OWN_THAT_ITEM;
				}*/
			}

			return InventoryError.OK;
		}
        
		
		/// <summary>
		/// Marks the given Item as taken and removes it from the list of available Items
		/// </summary>
		/// <param name="lootItem"></param>
		public void RemoveItem(LootItem lootItem)
		{
			lootItem.Taken = true;
			m_takenCount++;

			// TODO: Have correct collection of all observing Characters
			foreach (var looter in Looters)
			{
				if (looter.Owner != null)
				{
					LootHandler.SendLootRemoved(looter.Owner, lootItem.Index);
				}
			}
			CheckFinished();
		}

		
		/// <summary>
		/// Disposes this loot, despite the fact that it could still contain something valuable
		/// </summary>
		public void ForceDispose()
		{
			Dispose();
		}
        public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Loot);
        protected override UpdateFieldCollection _UpdateFieldInfos{get
        {
            return UpdateFieldInfos;
        }
        }

	    public override UpdateFlags UpdateFlags
	    {
            get { return UpdateFlags.StationaryObject; }
	    }

	    public override ObjectTypeId ObjectTypeId
	    {
	        get { return ObjectTypeId.Loot;}
	    }

	    public new void Dispose()
		{
            if (Map != null)
                Map.Loots.Remove(this);

			OnDispose();
            Dispose(true);
		}

	    public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
	    {
	        get { throw new NotImplementedException(); }
	    }

	    protected virtual void OnDispose()
		{
			if (Lootable != null)
			{
				Lootable.OnFinishedLooting();
				Lootable = null;
			}
		}

		public void RemoveLooter(Asda2LooterEntry entry)
		{
			Looters.Remove(entry);
		}

	    #region Overrides of WorldObject

	    public override string Name
	    {
            get
            {
                return  Template==null?"":Template.ToString(); }
	        set {  }
	    }

	    public override Faction Faction
	    {
            get { return FactionMgr.AlliancePlayerFactions[0]; }
	        set {  }
	    }

	    public override FactionId FactionId
	    {
            get { return FactionMgr.AlliancePlayerFactions[0].Id; }
	        set { }
	    }

	    public bool AutoLoot { get; set; }

	    public short? MonstrId;

	    #endregion

	}
}