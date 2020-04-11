using Castle.ActiveRecord;
using NHibernate.Criterion;
using System;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord(Table = "AccountData")]
    public class AccountDataRecord : ActiveRecordBase<AccountDataRecord>
    {
        [Field] public byte[][] DataHolder = new byte[8][];

        /// <summary>TODO: Can be changed to uint (unix time)</summary>
        [Field] public int[] TimeStamps = new int[8];

        [Field] public int[] SizeHolder = new int[8];

        [PrimaryKey(PrimaryKeyType.Assigned)] public long accountId { get; set; }

        public static AccountDataRecord GetAccountData(long accountID)
        {
            return ActiveRecordBase<AccountDataRecord>.FindOne(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("accountId", (object) accountID)
            });
        }

        /// <summary>
        /// This is used to initialize *skeleton* data for accounts that do not already have data stored server side.
        /// We initialize with DateTime.MinValue to cause the client to update the server side data.
        /// </summary>
        /// <param name="accountID">GUID of the account that needs to be initialized</param>
        /// <returns>An AccountDataRecord reference</returns>
        public static AccountDataRecord InitializeNewAccount(long accountID)
        {
            AccountDataRecord accountDataRecord = new AccountDataRecord()
            {
                accountId = accountID
            };
            for (uint index = 7; index > 0U; --index)
                accountDataRecord.TimeStamps[index] = 0;
            accountDataRecord.Create();
            accountDataRecord.SoulmateIntroduction = "User has not entered introduction about him self.";
            return accountDataRecord;
        }

        public void SetAccountData(uint dataType, uint time, byte[] data, uint compressedSize)
        {
            this.DataHolder[dataType] = data;
            this.TimeStamps[dataType] = (int) time;
            this.SizeHolder[dataType] = (int) compressedSize;
        }

        [Property(Length = 127)] public string SoulmateIntroduction { get; set; }
    }
}