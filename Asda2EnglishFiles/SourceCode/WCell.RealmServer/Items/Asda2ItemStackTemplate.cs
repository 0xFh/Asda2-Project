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
            if (this.m_Template == null)
                throw new ArgumentException("ItemId " + (object) id + " is invalid.");
        }

        public Asda2ItemStackTemplate(Asda2ItemId id, int amount)
        {
            this = new Asda2ItemStackTemplate(Asda2ItemMgr.GetTemplate(id), amount);
            if (this.m_Template == null)
                throw new ArgumentException("id " + (object) id + " is invalid.");
        }

        public Asda2ItemStackTemplate(Asda2ItemTemplate templ)
        {
            this = new Asda2ItemStackTemplate(templ, templ.MaxAmount);
        }

        public Asda2ItemStackTemplate(Asda2ItemTemplate templ, int amount)
        {
            this.m_Template = templ;
            this.m_Amount = amount;
        }

        public Asda2ItemTemplate Template
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