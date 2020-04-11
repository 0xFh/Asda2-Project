using WCell.Constants.Items;
using WCell.RealmServer.Items.Enchanting;

namespace WCell.RealmServer.Items
{
    public class GemProperties
    {
        public uint Id;
        public ItemEnchantmentEntry Enchantment;
        public SocketColor Color;

        public override string ToString()
        {
            return string.Format("{0} (Color: {1}, Enchantment: {2})", (object) this.Id, (object) this.Color,
                (object) this.Enchantment);
        }
    }
}