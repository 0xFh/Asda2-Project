using Castle.ActiveRecord;
using NHibernate.Criterion;
using System;
using WCell.Constants.Guilds;

namespace WCell.RealmServer.Guilds
{
    [Castle.ActiveRecord.ActiveRecord("GuildEventLogEntries", Access = PropertyAccess.Property)]
    public class GuildEventLogEntry : ActiveRecordBase<GuildEventLogEntry>
    {
        private static readonly Order CreatedOrder = new Order(nameof(TimeStamp), false);
        [Field("GuildId", NotNull = true)] private int m_GuildId;

        public static GuildEventLogEntry[] LoadAll(uint guildId)
        {
            return ActiveRecordBase<GuildEventLogEntry>.FindAll(GuildEventLogEntry.CreatedOrder, new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("m_GuildId", (object) (int) guildId)
            });
        }

        [PrimaryKey(PrimaryKeyType.Increment)] private long Guid { get; set; }

        public uint GuildId
        {
            get { return (uint) this.m_GuildId; }
            set { this.m_GuildId = (int) value; }
        }

        [Property(NotNull = true)] public GuildEventLogEntryType Type { get; set; }

        [Property(NotNull = true)] public int Character1LowId { get; set; }

        [Property(NotNull = true)] public int Character2LowId { get; set; }

        [Property(NotNull = true)] public int NewRankId { get; set; }

        [Property(NotNull = true)] public DateTime TimeStamp { get; set; }

        public GuildEventLogEntry(Guild guild, GuildEventLogEntryType type, int character1LowId, int character2LowId,
            int newRankId, DateTime timeStamp)
        {
            this.GuildId = guild.Id;
            this.Type = type;
            this.Character1LowId = character1LowId;
            this.Character2LowId = character2LowId;
            this.NewRankId = newRankId;
            this.TimeStamp = timeStamp;
        }

        public GuildEventLogEntry()
        {
        }

        public static GuildEventLogEntry[] FindAll(Guild guild)
        {
            return ActiveRecordBase<GuildEventLogEntry>.FindAllByProperty("m_GuildId", (object) (int) guild.Id);
        }
    }
}