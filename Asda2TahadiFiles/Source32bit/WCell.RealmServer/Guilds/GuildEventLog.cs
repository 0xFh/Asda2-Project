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
      if(isNew)
        return;
      foreach(GuildEventLogEntry guildEventLogEntry in GuildEventLogEntry.LoadAll(guild.Id))
        entries.Insert(guildEventLogEntry);
    }

    internal GuildEventLog(Guild guild)
    {
      m_guild = guild;
      entries =
        new StaticCircularList<GuildEventLogEntry>(100,
          OnEntryDeleted);
    }

    public void AddEvent(GuildEventLogEntryType type, uint character1LowId, uint character2LowId, int newRankId)
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(() =>
      {
        GuildEventLogEntry record = new GuildEventLogEntry(m_guild, type, (int) character1LowId,
          (int) character2LowId, newRankId, DateTime.Now);
        record.CreateLater();
        lock(entries)
          entries.Insert(record);
      });
    }

    public void AddInviteEvent(uint inviterLowId, uint inviteeLowId)
    {
      AddEvent(GuildEventLogEntryType.INVITE_PLAYER, inviterLowId, inviteeLowId, 0);
    }

    public void AddRemoveEvent(uint removerLowId, uint removedLowId)
    {
      AddEvent(GuildEventLogEntryType.UNINVITE_PLAYER, removerLowId, removedLowId, 0);
    }

    public void AddJoinEvent(uint playerLowId)
    {
      AddEvent(GuildEventLogEntryType.JOIN_GUILD, playerLowId, 0U, 0);
    }

    public void AddPromoteEvent(uint promoterLowId, uint targetLowId, int newRankId)
    {
      AddEvent(GuildEventLogEntryType.PROMOTE_PLAYER, promoterLowId, targetLowId, newRankId);
    }

    public void AddDemoteEvent(uint demoterLowId, uint targetLowId, int newRankId)
    {
      AddEvent(GuildEventLogEntryType.DEMOTE_PLAYER, demoterLowId, targetLowId, newRankId);
    }

    public void AddLeaveEvent(uint playerLowId)
    {
      AddEvent(GuildEventLogEntryType.LEAVE_GUILD, playerLowId, 0U, 0);
    }

    private static void OnEntryDeleted(GuildEventLogEntry obj)
    {
      obj.DeleteLater();
    }

    public IEnumerator<GuildEventLogEntry> GetEnumerator()
    {
      lock(entries)
      {
        foreach(GuildEventLogEntry entry in entries)
          yield return entry;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}