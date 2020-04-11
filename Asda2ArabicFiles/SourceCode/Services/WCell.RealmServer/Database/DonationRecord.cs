using System;
using Castle.ActiveRecord;
using WCell.Core.Database;

namespace WCell.RealmServer.Database
{
    [ActiveRecord("donations", Access = PropertyAccess.Property)]
    public class DonationRecord : WCellRecord<DonationRecord>
	{
        [PrimaryKey(PrimaryKeyType.Assigned)]
        public int Id
        {
            get;
            set;
        }

        [Property(NotNull = true)]
        public int Wallet
        {
            get;
            set;
        }

        [Property(NotNull = true)]
        public int Amount
        {
            get;
            set;
        }

        [Property(NotNull = false)]
        public string CharacterName
        {
            get;
            set;
        }

        [Property(NotNull = false)]
        public long? TransactionId
        {
            get;
            set;
        }

        [Property(NotNull = true)]
        public bool IsDelivered
        {
            get;
            set;
        }

        [Property(NotNull = true)]
        public DateTime CreateDateTime
        {
            get;
            set;
        }

        [Property(NotNull = false)]
        public DateTime? DeliveredDateTime
        {
            get;
            set;
        }

        [Property(NotNull = false)]
        public string PayerName
        {
            get;
            set;
        }



    }
}
