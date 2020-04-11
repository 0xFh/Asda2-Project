using WCell.Constants.Items;

namespace WCell.RealmServer.NPCs.Armorer
{
    public class DurabilityCost
    {
        public uint ItemLvl;
        public uint[] Multipliers;

        public uint GetModifierBySubClassId(ItemClass itemClass, ItemSubClass itemSubClass)
        {
            switch (itemClass)
            {
                case ItemClass.Weapon:
                    return this.Multipliers[(int) itemSubClass];
                case ItemClass.Armor:
                    return this.Multipliers[(int) (itemSubClass + 21)];
                default:
                    return 0;
            }
        }

        public DurabilityCost()
        {
            this.Multipliers = new uint[29];
        }
    }
}