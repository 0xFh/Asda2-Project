using WCell.Constants.Items;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class DisarmOffHandHandler : DisarmHandler
    {
        public override InventorySlotType DisarmType
        {
            get { return InventorySlotType.WeaponOffHand; }
        }
    }
}