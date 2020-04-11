using Castle.ActiveRecord;
using System;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.Util.Threading;

namespace WCell.RealmServer.Guilds
{
    [Castle.ActiveRecord.ActiveRecord("GuildRank", Access = PropertyAccess.Property)]
    public class GuildRank : WCellRecord<GuildRank>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(GuildRank), nameof(_id), 1L);
        [Field("GuildId", NotNull = true)] private int m_GuildId;
        [Field("Privileges", NotNull = true)] private int _privileges;
        [Field("BankMoneyPerDay")] private int _moneyPerDay;
        private GuildBankTabRights[] m_BankTabRights;

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        private int _id { get; set; }

        public uint GuildId
        {
            get { return (uint) this.m_GuildId; }
            set { this.m_GuildId = (int) value; }
        }

        [Property(NotNull = true)] public string Name { get; set; }

        [Property(NotNull = true)] public int RankIndex { get; set; }

        public GuildBankTabRights[] BankTabRights
        {
            get { return this.m_BankTabRights; }
            set { this.m_BankTabRights = value; }
        }

        public GuildRank(Guild guild, string name, GuildPrivileges privileges, int id)
        {
            this._id = (int) GuildRank._idGenerator.Next();
            this.GuildId = guild.Id;
            this.Name = name;
            this._privileges = (int) privileges;
            this.RankIndex = id;
            this.BankTabRights = new GuildBankTabRights[6];
            for (int tabId = 0; tabId < 6; ++tabId)
                this.BankTabRights[tabId] = new GuildBankTabRights(tabId, (uint) this._id);
        }

        public static GuildRank[] FindAll(Guild guild)
        {
            return ActiveRecordBase<GuildRank>.FindAllByProperty("m_GuildId", (object) (int) guild.Id);
        }

        public void SaveLater()
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage(
                (IMessage) new Message(new Action(((ActiveRecordBase) this).Update)));
        }

        public override void Create()
        {
            base.Create();
            this.OnCreated();
        }

        public override void CreateAndFlush()
        {
            base.CreateAndFlush();
            this.OnCreated();
        }

        private void OnCreated()
        {
            foreach (ActiveRecordBase bankTabRight in this.m_BankTabRights)
                bankTabRight.Create();
        }

        protected override void OnDelete()
        {
            base.OnDelete();
            foreach (ActiveRecordBase bankTabRight in this.m_BankTabRights)
                bankTabRight.Delete();
        }

        /// <summary>Init a loaded Rank</summary>
        internal void InitRank()
        {
            this.m_BankTabRights =
                ActiveRecordBase<GuildBankTabRights>.FindAllByProperty("m_GuildRankId", (object) this._id);
            int length = this.m_BankTabRights.Length;
            Array.Resize<GuildBankTabRights>(ref this.m_BankTabRights, 6);
            for (int tabId = length; tabId < this.m_BankTabRights.Length; ++tabId)
                this.m_BankTabRights[tabId] = new GuildBankTabRights(tabId, (uint) this._id);
        }

        /// <summary>
        /// The daily money withdrawl allowance from the Guild Bank
        /// </summary>
        public uint DailyBankMoneyAllowance
        {
            get { return (uint) this._moneyPerDay; }
            set
            {
                if (this.RankIndex == 0)
                    this._moneyPerDay = int.MaxValue;
                else
                    this._moneyPerDay = (int) value;
            }
        }

        public GuildPrivileges Privileges
        {
            get { return (GuildPrivileges) this._privileges; }
            set
            {
                this._privileges = (int) value;
                this.SaveLater();
            }
        }

        public GuildRank()
        {
        }
    }
}