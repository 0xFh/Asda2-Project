using System;
using System.Collections;
using System.Collections.Generic;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.Util;

namespace WCell.RealmServer.Guilds
{
    public class GuildEventLog : IEnumerable<GuildEventLogEntry>, IEnumerable
    {
        protected const int MAX_ENTRIES_COUNT = 100;
        protected readonly Guild m_guild;
        protected readonly StaticCircularList<GuildEventLogEntry> entries;

        internal GuildEventLog(Guild guild, bool isNew)
            : this(guild)
        {
            if (isNew)
                return;
            foreach (GuildEventLogEntry guildEventLogEntry in GuildEventLogEntry.LoadAll(guild.Id))
                this.entries.Insert(guildEventLogEntry);
        }

        internal GuildEventLog(Guild guild)
        {
            this.m_guild = guild;
            this.entries =
                new StaticCircularList<GuildEventLogEntry>(100,
                    new Action<GuildEventLogEntry>(GuildEventLog.OnEntryDeleted));
        }

        public void AddEvent(GuildEventLogEntryType type, uint character1LowId, uint character2LowId, int newRankId)
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
            {
                GuildEventLogEntry record = new GuildEventLogEntry(this.m_guild, type, (int) character1LowId,
                    (int) character2LowId, newRankId, DateTime.Now);
                record.CreateLater();
                lock (this.entries)
                    this.entries.Insert(record);
            }));
        }

        public void AddInviteEvent(uint inviterLowId, uint inviteeLowId)
        {
            this.AddEvent(GuildEventLogEntryType.INVITE_PLAYER, inviterLowId, inviteeLowId, 0);
        }

        public void AddRemoveEvent(uint removerLowId, uint removedLowId)
        {
            this.AddEvent(GuildEventLogEntryType.UNINVITE_PLAYER, removerLowId, removedLowId, 0);
        }

        public void AddJoinEvent(uint playerLowId)
        {
            this.AddEvent(GuildEventLogEntryType.JOIN_GUILD, playerLowId, 0U, 0);
        }

        public void AddPromoteEvent(uint promoterLowId, uint targetLowId, int newRankId)
        {
            this.AddEvent(GuildEventLogEntryType.PROMOTE_PLAYER, promoterLowId, targetLowId, newRankId);
        }

        public void AddDemoteEvent(uint demoterLowId, uint targetLowId, int newRankId)
        {
            this.AddEvent(GuildEventLogEntryType.DEMOTE_PLAYER, demoterLowId, targetLowId, newRankId);
        }

        public void AddLeaveEvent(uint playerLowId)
        {
            this.AddEvent(GuildEventLogEntryType.LEAVE_GUILD, playerLowId, 0U, 0);
        }

        private static void OnEntryDeleted(GuildEventLogEntry obj)
        {
            obj.DeleteLater();
        }

        public IEnumerator<GuildEventLogEntry> GetEnumerator()
        {
            lock (this.entries)
            {
                foreach (GuildEventLogEntry entry in this.entries)
                    yield return entry;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }
    }
}