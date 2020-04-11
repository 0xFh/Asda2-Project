using WCell.Constants.Items;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class DisarmMainHandHandler : DisarmHandler
    {
        public override InventorySlotType DisarmType
        {
            get { return InventorySlotType.WeaponMainHand; }
        }
    }
}