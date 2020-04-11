using WCell.Constants;
using WCell.Constants.Items;

namespace WCell.Core.DBC
{
    public struct CharStartOutfit
    {
        public uint Id;
        public ClassId Class;
        public RaceId Race;
        public GenderType Gender;
        public uint[] ItemIds;
        public InventorySlotType[] ItemSlots;
    }
}