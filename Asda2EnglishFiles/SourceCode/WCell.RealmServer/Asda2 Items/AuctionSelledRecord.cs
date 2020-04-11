using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Asda2_Items
{
    [Castle.ActiveRecord.ActiveRecord("AuctionSelledRecord", Access = PropertyAccess.Property)]
    public class AuctionSelledRecord : WCellRecord<AuctionSelledRecord>
    {
        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(AuctionSelledRecord), nameof(Guid), 1L);

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        [Property] public uint ReciverCharacterId { get; set; }

        [Property] public int GoldAmount { get; set; }

        [Property] public int ItemAmount { get; set; }

        [Property] public int ItemId { get; set; }

        public AuctionSelledRecord()
        {
        }

        public AuctionSelledRecord(uint recieverCharId, int goldAmount, int itemAmount, int itemId)
        {
            this.Guid = AuctionSelledRecord._idGenerator.Next();
            this.ReciverCharacterId = recieverCharId;
            this.GoldAmount = goldAmount;
            this.ItemAmount = itemAmount;
            this.ItemId = itemId;
        }
    }
}