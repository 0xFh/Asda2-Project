namespace WCell.RealmServer.Items
{
    public struct ItemStack
    {
        public ItemTemplate Template;
        public int Amount;

        public override string ToString()
        {
            return this.Amount.ToString() + "x " + (object) this.Template;
        }
    }
}