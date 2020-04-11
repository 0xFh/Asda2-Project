using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Items
{
    public class ItemStatBonus
    {
        public Asda2ItemBonusType Type;
        public short MinValue;
        public short MaxValue;
        public int Chance;

        public short GetValue()
        {
            return (short) Utility.Random((int) this.MinValue, (int) this.MaxValue);
        }
    }
}