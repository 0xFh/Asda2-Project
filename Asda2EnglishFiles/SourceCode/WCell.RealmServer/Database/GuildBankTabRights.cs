using Castle.ActiveRecord;
using System;
using WCell.Constants.Guilds;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord("GuildBankTabRights", Access = PropertyAccess.Property)]
    public class GuildBankTabRights : ActiveRecordBase<GuildBankTabRights>
    {
        [Field("GuildRankId", NotNull = true)] private int m_GuildRankId;
        [Field("Privileges", NotNull = true)] private int _priveleges;

        [Field("WithdrawlAllowance", NotNull = true)]
        private int _withdrawlAllowance;

        public GuildBankTabRights()
        {
        }

        public GuildBankTabRights(int tabId, uint rankId)
        {
            this.Privileges = GuildBankTabPrivileges.None;
            this.WithdrawlAllowance = 0U;
            this.TabId = tabId;
            this.GuildRankId = rankId;
        }

        [PrimaryKey(PrimaryKeyType.Increment)] internal long Id { get; set; }

        public uint GuildRankId
        {
            get { return (uint) this.m_GuildRankId; }
            set { this.m_GuildRankId = (int) value; }
        }

        [Property(NotNull = true)] public DateTime AllowanceResetTime { get; set; }

        public GuildBankTabPrivileges Privileges
        {
            get { return (GuildBankTabPrivileges) this._priveleges; }
            set { this._priveleges = (int) value; }
        }

        public uint WithdrawlAllowance
        {
            get { return (uint) this._withdrawlAllowance; }
            set { this._withdrawlAllowance = (int) value; }
        }

        [Property] public int TabId { get; set; }
    }
}