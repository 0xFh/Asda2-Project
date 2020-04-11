using System;
using WCell.Constants.Items;

namespace WCell.RealmServer.Items
{
  public struct Asda2ItemStackTemplate : IAsda2ItemStack
  {
    public static readonly Asda2ItemStackTemplate[] EmptyArray = new Asda2ItemStackTemplate[0];
    private Asda2ItemTemplate m_Template;
    private int m_Amount;

    public Asda2ItemStackTemplate(Asda2ItemId id)
    {
      this = new Asda2ItemStackTemplate(Asda2ItemMgr.GetTemplate(id), 1);
      if(m_Template == null)
        throw new ArgumentException("ItemId " + id + " is invalid.");
    }

    public Asda2ItemStackTemplate(Asda2ItemId id, int amount)
    {
      this = new Asda2ItemStackTemplate(Asda2ItemMgr.GetTemplate(id), amount);
      if(m_Template == null)
        throw new ArgumentException("id " + id + " is invalid.");
    }

    public Asda2ItemStackTemplate(Asda2ItemTemplate templ)
    {
      this = new Asda2ItemStackTemplate(templ, templ.MaxAmount);
    }

    public Asda2ItemStackTemplate(Asda2ItemTemplate templ, int amount)
    {
      m_Template = templ;
      m_Amount = amount;
    }

    public Asda2ItemTemplate Template
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