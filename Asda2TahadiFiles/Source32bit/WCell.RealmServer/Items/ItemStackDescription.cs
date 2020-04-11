using System;
using WCell.Constants.Items;

namespace WCell.RealmServer.Items
{
  [Serializable]
  public struct ItemStackDescription : IItemStack
  {
    public static readonly ItemStackDescription Empty = new ItemStackDescription();
    public static readonly ItemStackDescription[] EmptyArray = new ItemStackDescription[0];
    public Asda2ItemId ItemId;
    private int m_Amount;

    public int Amount
    {
      get { return m_Amount; }
      set { m_Amount = value; }
    }

    public ItemStackDescription(Asda2ItemId id, int amount)
    {
      ItemId = id;
      m_Amount = amount;
    }

    public ItemTemplate Template
    {
      get { return ItemMgr.GetTemplate(ItemId); }
    }

    public override string ToString()
    {
      if(Template != null)
        return (Template.IsStackable ? Amount + "x " : "") +
               Template;
      return Amount + "x " + ItemId + " (" + (int) ItemId + ")";
    }
  }
}