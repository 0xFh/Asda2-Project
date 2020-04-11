using WCell.Constants.Items;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Looting
{
    public class LootEntity
    {
        public uint EntryId;
        public int MinAmount;
        public int MaxAmount;
        public Asda2ItemId ItemId;

        /// <summary>
        /// A value between 0 and 100 to indicate the chance of this Entry to drop
        /// </summary>
        public float DropChance;

        public ItemTemplate ItemTemplate
        {
            get { return ItemMgr.GetTemplate(this.ItemId); }
        }
    }
}