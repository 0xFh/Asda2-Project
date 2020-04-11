using WCell.Core.Network;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
    public static class PacketOutExt
    {
        private const int goldItemId = 20551;

        public static void WriteItemAmount(this RealmPacketOut packet, Asda2Item item,
            bool setAmountTo0WhenDeleted = false)
        {
            int val = item != null
                ? (item.ItemId != 20551
                    ? (!item.IsDeleted
                        ? (item.Template.IsStackable ? item.Amount : 0)
                        : (!setAmountTo0WhenDeleted ? -1 : 0))
                    : (item.OwningCharacter != null ? (int) item.OwningCharacter.Money : item.Amount))
                : -1;
            packet.WriteInt32(val);
        }
    }
}