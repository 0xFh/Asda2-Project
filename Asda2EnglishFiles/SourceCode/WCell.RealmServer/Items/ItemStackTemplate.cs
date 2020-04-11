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
            if (this.m_Template == null)
                throw new ArgumentException("ItemId " + (object) id + " is invalid.");
        }

        public ItemStackTemplate(Asda2ItemId id, int amount)
        {
            this = new ItemStackTemplate(ItemMgr.GetTemplate(id), amount);
            if (this.m_Template == null)
                throw new ArgumentException("id " + (object) id + " is invalid.");
        }

        public ItemStackTemplate(ItemTemplate templ)
        {
            this = new ItemStackTemplate(templ, templ.MaxAmount);
        }

        public ItemStackTemplate(ItemTemplate templ, int amount)
        {
            this.m_Template = templ;
            this.m_Amount = amount;
        }

        public ItemTemplate Template
        {
            get { return this.m_Template; }
            set { this.m_Template = value; }
        }

        public int Amount
        {
            get { return this.m_Amount; }
            set { this.m_Amount = value; }
        }

        public override string ToString()
        {
            if (this.Template != null)
                return (this.Template.IsStackable ? (object) (this.Amount.ToString() + "x ") : (object) "").ToString() +
                       (object) this.Template;
            return this.Amount.ToString() + "x [Unknown]";
        }
    }
}