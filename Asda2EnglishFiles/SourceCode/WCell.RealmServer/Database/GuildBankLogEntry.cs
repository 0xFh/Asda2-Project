using Castle.ActiveRecord;
using NHibernate.Criterion;
using System;
using WCell.Constants.Guilds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Guilds
{
    [Castle.ActiveRecord.ActiveRecord("GuildBankLogEntries", Access = PropertyAccess.Property)]
    public class GuildBankLogEntry : ActiveRecordBase<GuildBankLogEntry>
    {
        private static readonly Order CreatedOrder = new Order(nameof(Created), false);
        [Field] private int bankLogEntryType;
        [Field] private int actorEntityLowId;
        [Field] public int DestinationTabId;

        public static GuildBankLogEntry[] LoadAll(uint guildId)
        {
            return ActiveRecordBase<GuildBankLogEntry>.FindAll(GuildBankLogEntry.CreatedOrder, new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("GuildId", (object) (int) guildId)
            });
        }

        public GuildBankLogEntry()
        {
        }

        public GuildBankLogEntry(uint guildId)
        {
            this.GuildId = (int) guildId;
        }

        [Property] public int GuildId { get; set; }

        [PrimaryKey(PrimaryKeyType.GuidComb)] public long BankLogEntryRecordId { get; set; }

        public GuildBankLog BankLog { get; set; }

        [Property] public int ItemEntryId { get; set; }

        [Property] public int ItemStackCount { get; set; }

        [Property] public int Money { get; set; }

        [Property] public DateTime Created { get; set; }

        public GuildBankLogEntryType Type
        {
            get { return (GuildBankLogEntryType) this.bankLogEntryType; }
            set { this.bankLogEntryType = (int) value; }
        }

        public Character Actor
        {
            get { return World.GetCharacter((uint) this.actorEntityLowId); }
            set { this.actorEntityLowId = (int) value.EntityId.Low; }
        }

        public GuildBankTab DestinationTab
        {
            get { return this.BankLog.Bank[this.DestinationTabId]; }
            set { this.DestinationTabId = value.BankSlot; }
        }
    }
}