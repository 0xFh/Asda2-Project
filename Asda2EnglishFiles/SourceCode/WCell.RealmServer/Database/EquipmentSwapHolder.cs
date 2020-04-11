using WCell.Constants.Items;
using WCell.Core;

namespace WCell.RealmServer.Database
{
    public struct EquipmentSwapHolder
    {
        public EntityId ItemGuid;
        public InventorySlot SrcContainer;
        public int SrcSlot;
    }
}