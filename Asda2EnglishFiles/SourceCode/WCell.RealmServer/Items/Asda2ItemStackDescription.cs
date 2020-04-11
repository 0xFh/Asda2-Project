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
            get { return this.m_Amount; }
            set { this.m_Amount = value; }
        }

        public Asda2ItemStackDescription(Asda2ItemId id, int amount)
        {
            this.ItemId = id;
            this.m_Amount = amount;
        }

        public Asda2ItemTemplate Template
        {
            get { return Asda2ItemMgr.GetTemplate(this.ItemId); }
        }

        public override string ToString()
        {
            if (this.Template != null)
                return (this.Template.IsStackable ? (object) (this.Amount.ToString() + "x ") : (object) "").ToString() +
                       (object) this.Template;
            return this.Amount.ToString() + "x " + (object) this.ItemId + " (" + (object) (int) this.ItemId + ")";
        }
    }
}