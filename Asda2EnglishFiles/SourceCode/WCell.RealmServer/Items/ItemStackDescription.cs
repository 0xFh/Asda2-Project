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
            get { return this.m_Amount; }
            set { this.m_Amount = value; }
        }

        public ItemStackDescription(Asda2ItemId id, int amount)
        {
            this.ItemId = id;
            this.m_Amount = amount;
        }

        public ItemTemplate Template
        {
            get { return ItemMgr.GetTemplate(this.ItemId); }
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