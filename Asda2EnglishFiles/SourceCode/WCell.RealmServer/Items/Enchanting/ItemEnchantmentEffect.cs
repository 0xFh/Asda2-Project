using WCell.Constants.Items;
using WCell.Util;

namespace WCell.RealmServer.Items.Enchanting
{
    public class ItemEnchantmentEffect
    {
        public ItemEnchantmentType Type;
        public int MinAmount;
        public int MaxAmount;

        /// <summary>
        /// Depending on the <see cref="F:WCell.RealmServer.Items.Enchanting.ItemEnchantmentEffect.Type" />:
        /// SpellId
        /// DamageSchool
        /// other
        /// </summary>
        public uint Misc;

        public int RandomAmount
        {
            get { return Utility.Random(this.MinAmount, this.MaxAmount); }
        }

        public override string ToString()
        {
            return string.Format("EnchantEffect - Type: {0}, Amount: {1}, Misc: {2}", (object) this.Type,
                (object) this.MinAmount, (object) this.Misc);
        }
    }
}