using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
    public class Asda2ItemTradeRef
    {
        public Asda2Item Item { get; set; }

        public int Amount { get; set; }

        public int Price { get; set; }

        public byte TradeSlot { get; set; }
    }
}