using System;
using WCell.Constants.Items;

namespace WCell.RealmServer.Items
{
  public struct ItemStackTemplate : IItemStack
  {
    public static readonly ItemStackTemplate[] EmptyArray = new ItemStackTemplate[0];
    private ItemTemplate m_Template;
    private int m_Amount;

    public ItemStackTemplate(Asda2ItemId id)
    {
      this = new ItemStackTemplate(ItemMgr.GetTemplate(id), 1);
      if(m_Template == null)
        throw new ArgumentException("ItemId " + id + " is invalid.");
    }

    public ItemStackTemplate(Asda2ItemId id, int amount)
    {
      this = new ItemStackTemplate(ItemMgr.GetTemplate(id), amount);
      if(m_Template == null)
        throw new ArgumentException("id " + id + " is invalid.");
    }

    public ItemStackTemplate(ItemTemplate templ)
    {
      this = new ItemStackTemplate(templ, templ.MaxAmount);
    }

    public ItemStackTemplate(ItemTemplate templ, int amount)
    {
      m_Template = templ;
      m_Amount = amount;
    }

    public ItemTemplate Template
    {
      get { return m_Template; }
      set { m_Template = value; }
    }

    public int Amount
    {
      get { return m_Amount; }
      set { m_Amount = value; }
    }

    public override string ToString()
    {
      if(Template != null)
        return (Template.IsStackable ? Amount + "x " : "") +
               Template;
      return Amount + "x [Unknown]";
    }
  }
}