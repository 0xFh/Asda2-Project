using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Guilds
{
  [ActiveRecord("GuildBankTabs", Access = PropertyAccess.Property)]
  public class GuildBankTab : ActiveRecordBase<GuildBankTab>
  {
    [Field("GuildId", NotNull = true)]private int _guildId;
    private GuildBank m_Bank;
    private IList<ItemRecord> _itemRecords;

    [PrimaryKey("TabId")]
    private long _TabId { get; set; }

    public GuildBankTab()
    {
      Items = new GuildBankTabItemMapping[98];
      ItemRecords = new ItemRecord[98];
    }

    public GuildBankTab(GuildBank bank)
      : this()
    {
      Bank = bank;
    }

    public GuildBank Bank
    {
      get { return m_Bank; }
      internal set
      {
        m_Bank = value;
        _guildId = (int) m_Bank.Guild.Id;
      }
    }

    [Property]
    public string Name { get; set; }

    [Property]
    public string Text { get; set; }

    [Property]
    public string Icon { get; set; }

    /// <summary>
    /// The Slot in the Bank's Tabs that this BankTab belongs in
    /// </summary>
    [Property]
    public int BankSlot { get; set; }

    [HasMany(typeof(GuildBankTabItemMapping), Cascade = ManyRelationCascadeEnum.AllDeleteOrphan, Inverse = true)]
    private IList<GuildBankTabItemMapping> Items { get; set; }

    public IList<ItemRecord> ItemRecords
    {
      set { _itemRecords = value; }
      get
      {
        if(_itemRecords == null)
        {
          _itemRecords = new ItemRecord[98];
          foreach(GuildBankTabItemMapping bankTabItemMapping in Items)
            _itemRecords[bankTabItemMapping.TabSlot] =
              ItemRecord.GetRecordByID(bankTabItemMapping.Guid);
        }

        return _itemRecords;
      }
    }

    public ItemRecord this[int slot]
    {
      get
      {
        if(slot > 98)
          return null;
        if(slot >= ItemRecords.Count - 1)
          return null;
        return ItemRecords[slot];
      }
      set
      {
        if(slot > 98)
          return;
        if(value == null)
        {
          Items[slot] = null;
          ItemRecords[slot] = null;
        }
        else
        {
          value.Slot = slot;
          Items[slot] = new GuildBankTabItemMapping
          {
            Guid = value.Guid,
            TabSlot = (byte) slot
          };
          ItemRecords[slot] = value;
        }
      }
    }

    /// <summary>Iterates over all slots in this GuildBankTab.</summary>
    /// <param name="callback">Function that is called on each Slot. This should return true to continue iterating.</param>
    public void Iterate(Func<ItemRecord, bool> callback)
    {
      int index = 0;
      while(index < 98 && callback(ItemRecords[index]))
        ++index;
    }

    /// <summary>
    /// Applies a function to all ItemRecords in this GuildBankTab.
    /// </summary>
    /// <param name="callback"></param>
    public void ForeachItem(Action<ItemRecord> callback)
    {
      foreach(ItemRecord itemRecord in ItemRecords)
        callback(itemRecord);
    }

    /// <summary>
    /// Tries to store the given item in the given slot. No merge attempted.
    /// </summary>
    /// <param name="item">The item to store.</param>
    /// <param name="slot">The slot to store it in.</param>
    /// <returns>The <see cref="T:WCell.RealmServer.Database.ItemRecord" /> that was in the slot (or the original <see cref="T:WCell.RealmServer.Database.ItemRecord" /> minus the items that were merged)
    /// or null if the store/merge was successful.</returns>
    public ItemRecord StoreItemInSlot(ItemRecord item, int slot)
    {
      return StoreItemInSlot(item, slot, false);
    }

    /// <summary>
    /// Places the given item in the given slot (or tries mergeing at slot&gt; if indicated).
    /// Make sure that the depositer has deposit rights to this BankTab!
    /// </summary>
    /// <param name="item">The <see cref="T:WCell.RealmServer.Database.ItemRecord" /> to store.</param>
    /// <param name="slot">The slotId where you want to store.</param>
    /// <param name="allowMerge">Whether or not to try and merge the stacks.</param>
    /// <returns>The <see cref="T:WCell.RealmServer.Database.ItemRecord" /> that was in the slot (or the original <see cref="T:WCell.RealmServer.Database.ItemRecord" /> minus the items that were merged)
    /// or null if the store/merge was successful.</returns>
    public ItemRecord StoreItemInSlot(ItemRecord item, int slot, bool allowMerge)
    {
      return StoreItemInSlot(item, item.Amount, slot, allowMerge);
    }

    /// <summary>
    /// Places the given amount of the given item stack in the given slot (or tries mergeing at slot if indicated).
    /// Make sure that the depositer has deposit rights to this BankTab!
    /// </summary>
    /// <param name="item">The <see cref="T:WCell.RealmServer.Database.ItemRecord" /> to store.</param>
    /// <param name="amount">The amount of items from the stack to store.</param>
    /// <param name="slot">The slotId where you want to store.</param>
    /// <param name="allowMerge">Whether or not to try and merge the stacks.</param>
    /// <returns>The <see cref="T:WCell.RealmServer.Database.ItemRecord" /> that was in the slot (or the original <see cref="T:WCell.RealmServer.Database.ItemRecord" /> minus the items that were merged)
    /// or null if the store/merge was successful.</returns>
    public ItemRecord StoreItemInSlot(ItemRecord item, int amount, int slot, bool allowMerge)
    {
      if(slot >= 98 || item.Amount < amount)
        return item;
      ItemRecord itemRecord = this[slot];
      if(itemRecord == null)
      {
        this[slot] = item;
        return null;
      }

      if(allowMerge && (int) this[slot].EntryId == (int) item.EntryId)
      {
        int num = Math.Min(itemRecord.Template.MaxAmount - itemRecord.Amount, amount);
        itemRecord.Amount += num;
        item.Amount -= num;
        if(item.Amount <= 0)
          return null;
        return item;
      }

      this[slot] = item;
      return itemRecord;
    }

    /// <summary>
    /// Checks if the given amount of the given item stack can be stored in the given slot (optionally with merging),
    /// without necessitating an Item swap. No actual store takes place.
    /// </summary>
    /// <param name="item">The <see cref="T:WCell.RealmServer.Database.ItemRecord" /> to store.</param>
    /// <param name="amount">The amount of items from the stack to store.</param>
    /// <param name="slot">The slotId where you want to store.</param>
    /// <param name="allowMerge">Whether or not to try and merge the stacks.</param>
    /// <returns>true if no Item swap is needed.</returns>
    public bool CheckStoreItemInSlot(ItemRecord item, int amount, int slot, bool allowMerge)
    {
      if(slot >= 98 || item.Amount < amount)
        return false;
      ItemRecord itemRecord = this[slot];
      if(itemRecord == null)
        return true;
      if(allowMerge && (int) this[slot].EntryId == (int) item.EntryId)
        return itemRecord.Template.MaxAmount - itemRecord.Amount >= amount;
      return false;
    }

    /// <summary>Attempts to find an available Slot in this BankTab.</summary>
    /// <returns>The slot's id or -1</returns>
    public int FindFreeSlot()
    {
      for(int index = 0; index < 98; ++index)
      {
        if(ItemRecords[index] == null)
          return index;
      }

      return -1;
    }
  }
}