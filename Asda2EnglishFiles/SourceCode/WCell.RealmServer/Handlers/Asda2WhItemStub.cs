using WCell.RealmServer.Asda2_Items;

namespace WCell.RealmServer.Handlers
{
    public class Asda2WhItemStub
    {
        private short _slot = -1;

        public Asda2InventoryType Invtentory { get; set; }

        public short Slot
        {
            get { return this._slot; }
            set { this._slot = value; }
        }

        public int Amount { get; set; }

        public short Weight { get; set; }
    }
}