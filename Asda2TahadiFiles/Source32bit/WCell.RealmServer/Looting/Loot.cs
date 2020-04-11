using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;
using WCell.Util.NLog;

namespace WCell.RealmServer.Looting
{
  /// <summary>
  /// Represents a pile of lootable objects and its looters
  /// 
  /// TODO: Roll timeout (and loot timeout?)
  /// </summary>
  public abstract class Loot
  {
    /// <summary>
    /// The set of all who are allowed to loot.
    /// If everyone released the Loot, it becomes available to everyone else?
    /// </summary>
    public IList<LooterEntry> Looters;

    private uint m_Money;

    /// <summary>All items that can be looted.</summary>
    public LootItem[] Items;

    /// <summary>The Container being looted</summary>
    public ILootable Lootable;

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

    /// <summary>The total amount of money to be looted</summary>
    public uint Money
    {
      get { return m_Money; }
      set
      {
        m_Money = value;
        m_moneyLooted = m_Money == 0U;
      }
    }

    protected Loot()
    {
      Looters = new List<LooterEntry>();
    }

    protected Loot(ILootable looted, uint money, LootItem[] items)
      : this()
    {
      Money = money;
      Items = items;
      Lootable = looted;
    }

    /// <summary>The amount of Items that have already been taken</summary>
    public int TakenCount
    {
      get { return m_takenCount; }
    }

    /// <summary>Amount of remaining items</summary>
    public int RemainingCount
    {
      get { return Items.Length - m_takenCount; }
    }

    /// <summary>
    /// Amount of items that are freely available to everyone:
    /// Items that are passed by everyone or that have been left over by the looter whose turn it is in RoundRobin
    /// </summary>
    public int FreelyAvailableCount
    {
      get { return m_freelyAvailableCount; }
      internal set { m_freelyAvailableCount = value; }
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
      get { return m_released; }
      internal set
      {
        if(m_released == value)
          return;
        m_released = value;
        if(!value || RemainingCount != 0 || !m_moneyLooted)
          return;
        Dispose();
      }
    }

    /// <summary>Whether the money has already been given out</summary>
    public bool IsMoneyLooted
    {
      get { return m_moneyLooted; }
    }

    public bool MustKneelWhileLooting
    {
      get { return Lootable is WorldObject; }
    }

    public bool IsGroupLoot
    {
      get { return Lootable.UseGroupLoot; }
    }

    public abstract LootResponseType ResponseType { get; }

    /// <summary>
    /// Adds all initial Looters of nearby Characters who may loot this Loot.
    /// When all of the initial Looters gave up the Loot, the Loot becomes free for all.
    /// </summary>
    public void Initialize(Character chr, IList<LooterEntry> looters, MapId mapid)
    {
    }

    /// <summary>
    /// This gives the money to everyone involved. Will only work the first time its called.
    /// Afterwards <c>IsMoneyLooted</c> will be true.
    /// </summary>
    public void GiveMoney()
    {
      if(m_moneyLooted)
        return;
      if(Group == null)
      {
        LooterEntry looterEntry = Looters.FirstOrDefault();
        if(looterEntry != null && looterEntry.Owner != null)
        {
          m_moneyLooted = true;
          SendMoney(looterEntry.Owner, Money);
        }
      }
      else
      {
        List<Character> characterList = new List<Character>();
        if(UsesRoundRobin)
        {
          LooterEntry looterEntry = Looters.FirstOrDefault();
          if(looterEntry != null && looterEntry.Owner != null)
          {
            characterList.Add(looterEntry.Owner);
            if(Lootable is WorldObject)
            {
              WorldObject lootable = (WorldObject) Lootable;
            }
            else
            {
              Character owner = looterEntry.Owner;
            }
          }
        }
        else
        {
          foreach(LooterEntry looter in Looters)
          {
            if(looter.m_owner != null)
              characterList.Add(looter.m_owner);
          }
        }

        if(characterList.Count > 0)
        {
          m_moneyLooted = true;
          uint amount = Money / (uint) characterList.Count;
          foreach(Character character in characterList)
          {
            SendMoney(character, amount);
            LootHandler.SendMoneyNotify(character, amount);
          }
        }
      }

      CheckFinished();
    }

    /// <summary>
    /// Gives the receiver the money and informs everyone else
    /// </summary>
    /// <param name="receiver"></param>
    /// <param name="amount"></param>
    protected void SendMoney(Character receiver, uint amount)
    {
      receiver.AddMoney(amount);
      LootHandler.SendClearMoney(this);
    }

    /// <summary>
    /// Checks whether this Loot has been fully looted and if so, dispose and dismember the corpse or consumable object
    /// </summary>
    public void CheckFinished()
    {
      if(!m_moneyLooted || m_takenCount != Items.Length)
        return;
      Dispose();
    }

    /// <summary>
    /// Returns whether the given looter may loot the given Item.
    /// Make sure the Looter is logged in before calling this Method.
    /// 
    /// TODO: Find the right error messages
    /// TODO: Only give every MultiLoot item to everyone once! Also check for quest-dependencies etc.
    /// </summary>
    public InventoryError CheckTakeItemConditions(LooterEntry looter, LootItem item)
    {
      if(item.Taken)
        return InventoryError.ALREADY_LOOTED;
      if(item.RollProgress != null)
        return InventoryError.DONT_OWN_THAT_ITEM;
      if(!looter.MayLoot(this))
        return InventoryError.DontReport;
      ICollection<LooterEntry> multiLooters = item.MultiLooters;
      if(multiLooters != null)
      {
        if(multiLooters.Contains(looter))
          return InventoryError.OK;
        if(looter.Owner != null)
          LootHandler.SendLootRemoved(looter.Owner, item.Index);
        return InventoryError.DONT_OWN_THAT_ITEM;
      }

      return !item.Template.CheckLootConstraints(looter.Owner) || Method != LootMethod.FreeForAll &&
             (item.Template.Quality > Group.LootThreshold && !item.Passed ||
              Group.MasterLooter != null && Group.MasterLooter != looter.Owner.GroupMember)
        ? InventoryError.DONT_OWN_THAT_ITEM
        : InventoryError.OK;
    }

    /// <summary>
    /// Try to loot the item at the given index of the current loot
    /// </summary>
    /// <returns>The looted Item or null if Item could not be taken</returns>
    public void TakeItem(LooterEntry entry, uint index, BaseInventory targetCont, int targetSlot)
    {
      LootItem lootItem = null;
      try
      {
        Character owner = entry.Owner;
        if(owner == null || index >= Items.Length)
          return;
        lootItem = Items[index];
        InventoryError itemConditions = CheckTakeItemConditions(entry, lootItem);
        if(itemConditions == InventoryError.OK)
          HandoutItem(owner, lootItem, targetCont, targetSlot);
        else
          ItemHandler.SendInventoryError(owner.Client, null, null,
            itemConditions);
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex, "{0} threw an Exception while looting \"{1}\" (index = {2}) from {3}",
          (object) entry.Owner, (object) lootItem, (object) index, (object) targetCont);
      }
    }

    /// <summary>Hands out the given LootItem to the given Character.</summary>
    /// <remarks>Adds the given container at the given slot or -if not specified- to the next free slot</remarks>
    /// <param name="chr"></param>
    /// <param name="lootItem"></param>
    /// <param name="targetCont"></param>
    /// <param name="targetSlot"></param>
    public void HandoutItem(Character chr, LootItem lootItem, BaseInventory targetCont, int targetSlot)
    {
    }

    /// <summary>
    /// Marks the given Item as taken and removes it from the list of available Items
    /// </summary>
    /// <param name="lootItem"></param>
    public void RemoveItem(LootItem lootItem)
    {
      lootItem.Taken = true;
      ++m_takenCount;
      foreach(LooterEntry looter in Looters)
      {
        if(looter.Owner != null)
          LootHandler.SendLootRemoved(looter.Owner, lootItem.Index);
      }

      CheckFinished();
    }

    /// <summary>
    /// Lets the given Character roll for the item at the given index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="type"></param>
    public void Roll(Character chr, uint index, LootRollType type)
    {
      LootItem lootItem = Items[index];
      if(lootItem.RollProgress == null)
        return;
      lootItem.RollProgress.Roll(chr, type);
      if(!lootItem.RollProgress.IsRollFinished)
        return;
      Character highestParticipant = lootItem.RollProgress.HighestParticipant;
      if(highestParticipant != null)
      {
        LootHandler.SendRollWon(highestParticipant, this, lootItem, lootItem.RollProgress.HighestEntry);
        HandoutItem(highestParticipant, lootItem, null, byte.MaxValue);
      }
      else
        RemoveItem(lootItem);

      lootItem.RollProgress.Dispose();
    }

    /// <summary>
    /// Disposes this loot, despite the fact that it could still contain something valuable
    /// </summary>
    public void ForceDispose()
    {
      Dispose();
    }

    protected void Dispose()
    {
      if(Looters == null)
        return;
      IList<LooterEntry> looters = Looters;
      Looters = null;
      for(int index = 0; index < looters.Count; ++index)
      {
        LooterEntry looterEntry = looters[index];
        Character owner = looterEntry.Owner;
        if(owner != null)
          LootHandler.SendLootReleaseResponse(owner, this);
        if(looterEntry.Loot == this)
          looterEntry.Loot = null;
      }

      OnDispose();
    }

    protected virtual void OnDispose()
    {
      if(Lootable == null)
        return;
      Lootable.OnFinishedLooting();
      Lootable = null;
    }

    public void RemoveLooter(LooterEntry entry)
    {
      Looters.Remove(entry);
    }

    /// <summary>
    /// Lets the master looter give the item in the given slot to the given receiver
    /// </summary>
    public void GiveLoot(Character master, Character receiver, byte lootSlot)
    {
      if(master.Group == null || master.Group.MasterLooter.Character != master ||
         (receiver.Group == null || master.Group != receiver.Group) ||
         (!master.Loot.Looters.Contains(receiver.LooterEntry) || Items[lootSlot] == null))
        return;
      HandoutItem(receiver, Items[lootSlot], null, byte.MaxValue);
    }
  }
}