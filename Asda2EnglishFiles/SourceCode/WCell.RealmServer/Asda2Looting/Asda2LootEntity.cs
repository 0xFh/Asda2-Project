using WCell.Constants.Items;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Asda2Looting
{
    public class Asda2LootEntity
    {
        public uint MonstrId;
        public int MinAmount;
        public int MaxAmount;
        public Asda2ItemId ItemId;
        public int RequiredQuestId;

        /// <summary>
        /// A value between 0 and 100 to indicate the chance of this Entry to drop
        /// </summary>
        public float DropChance;

        public Asda2ItemTemplate ItemTemplate
        {
            get { return Asda2ItemMgr.GetTemplate(this.ItemId); }
        }
    }
}