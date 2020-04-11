namespace WCell.Constants.Items
{
    public static class Extensions
    {
        public static bool HasAnyFlag(this ItemSubClassMask flags, ItemSubClassMask otherFlags)
        {
            return (flags & otherFlags) != ItemSubClassMask.None;
        }

        public static bool HasAnyFlag(this InventorySlotTypeMask flags, InventorySlotTypeMask otherFlags)
        {
            return (flags & otherFlags) != InventorySlotTypeMask.None;
        }

        public static bool HasAnyFlag(this InventorySlotTypeMask flags, InventorySlotType type)
        {
            return (flags & type.ToMask()) != InventorySlotTypeMask.None;
        }

        public static bool HasAnyFlag(this SocketColor flags, SocketColor otherFlags)
        {
            return (flags & otherFlags) != SocketColor.None;
        }

        public static bool HasAnyFlag(this ItemBagFamilyMask flags, ItemBagFamilyMask otherFlags)
        {
            return (flags & otherFlags) != ItemBagFamilyMask.None;
        }

        public static bool HasAnyFlag(this ItemFlags flags, ItemFlags otherFlags)
        {
            return (flags & otherFlags) != ItemFlags.None;
        }

        public static bool HasAnyFlag(this ItemFlags2 flags, ItemFlags2 otherFlags)
        {
            return (flags & otherFlags) != (ItemFlags2) 0;
        }

        public static InventorySlotTypeMask ToMask(this InventorySlotType type)
        {
            return (InventorySlotTypeMask)
                (1 << (int) (type & (InventorySlotType.WeaponRanged | InventorySlotType.Cloak)));
        }
    }
}