using WCell.Constants.Items;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class DisarmRangedHandler : DisarmHandler
    {
        public override InventorySlotType DisarmType
        {
            get { return InventorySlotType.WeaponRanged; }
        }
    }
}