using WCell.Constants.Items;

namespace WCell.RealmServer.Items
{
  public struct Asda2ItemStackDescription : IAsda2ItemStack
  {
    public static readonly Asda2ItemStackDescription Empty = new Asda2ItemStackDescription();
    public static readonly Asda2ItemStackDescription[] EmptyArray = new Asda2ItemStackDescription[0];
    public Asda2ItemId ItemId;
    private int m_Amount;

    public int Amount
    {
      get { return m_Amount; }
      set { m_Amount = value; }
    }

    public Asda2ItemStackDescription(Asda2ItemId id, int amount)
    {
      ItemId = id;
      m_Amount = amount;
    }

    public Asda2ItemTemplate Template
    {
      get { return Asda2ItemMgr.GetTemplate(ItemId); }
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