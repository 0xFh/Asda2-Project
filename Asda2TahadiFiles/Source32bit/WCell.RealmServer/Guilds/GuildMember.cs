using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util;

namespace WCell.RealmServer.Guilds
{
  /// <summary>
  /// Represents the relationship between a Character and its Guild.
  /// </summary>
  [ActiveRecord("GuildMember", Access = PropertyAccess.Property)]
  public class GuildMember : WCellRecord<GuildMember>, INamed
  {
    [Field("Name", NotNull = true)]private string _name;
    [Field("LastLvl", NotNull = true)]private int _lastLevel;
    [Field("LastLogin", NotNull = true)]private DateTime _lastLogin;
    [Field("LastZone", NotNull = true)]private int _lastZoneId;
    [Field("Class", NotNull = true)]private int _class;
    [Field("Rank", NotNull = true)]private int _rankId;
    [Field("GuildId", NotNull = true)]private int m_GuildId;
    [Field("PublicNote")]private string _publicNote;
    [Field("OfficerNote")]private string _officerNote;
    [Field("BankRemainingMoneyAllowance")]private int _remainingMoneyAllowance;

    [Field("BankMoneyAllowanceResetTime", NotNull = true)]
    private DateTime _moneyAllowanceResetTime;

    private Character m_chr;
    private Guild m_Guild;

    [PrimaryKey(PrimaryKeyType.Assigned)]
    public long CharacterLowId { get; private set; }

    public uint GuildId
    {
      get { return (uint) m_GuildId; }
      set { m_GuildId = (int) value; }
    }

    [Property]
    public byte CharNum { get; set; }

    [Property]
    public int AccId { get; set; }

    [Property]
    public byte ProffessionLevel { get; set; }

    /// <summary>Loads all members of the given guild from the DB</summary>
    public static GuildMember[] FindAll(uint guildId)
    {
      return FindAllByProperty("m_GuildId", (int) guildId);
    }

    public GuildMember(CharacterRecord chr, Guild guild, GuildRank rank)
      : this()
    {
      int zone = (int) chr.Zone;
      Guild = guild;
      CharacterLowId = (int) chr.EntityLowId;
      _rankId = rank.RankIndex;
      _name = chr.Name;
      _lastLevel = chr.Level;
      _lastLogin = DateTime.Now;
      _lastZoneId = zone;
      _class = (int) chr.Class;
      _publicNote = string.Empty;
      _officerNote = string.Empty;
      AccId = chr.AccountId;
      ProffessionLevel = chr.ProfessionLevel;
      CharNum = chr.CharNum;
    }

    /// <summary>
    /// The low part of the Character's EntityId. Use EntityId.GetPlayerId(Id) to get a full EntityId
    /// </summary>
    public uint Id
    {
      get { return (uint) CharacterLowId; }
    }

    /// <summary>The name of this GuildMember's character</summary>
    public string Name
    {
      get { return _name; }
    }

    public Guild Guild
    {
      get { return m_Guild; }
      private set
      {
        m_Guild = value;
        GuildId = value.Id;
      }
    }

    public string PublicNote
    {
      get { return _publicNote; }
      set
      {
        _publicNote = value;
        Asda2GuildHandler.SendGuildNotificationResponse(Guild, GuildNotificationType.EditPublicMessage,
          this);
        SaveLater();
      }
    }

    public string OfficerNote
    {
      get { return _officerNote; }
      set
      {
        _officerNote = value;
        SaveLater();
      }
    }

    /// <summary>
    /// Current level of this GuildMember or his saved level if member already logged out
    /// </summary>
    public int Level
    {
      get
      {
        if(m_chr != null)
          return m_chr.Level;
        return _lastLevel;
      }
      internal set { _lastLevel = value; }
    }

    /// <summary>Time of last login of member</summary>
    public DateTime LastLogin
    {
      get
      {
        if(m_chr != null)
          return DateTime.Now;
        return _lastLogin;
      }
      internal set { _lastLogin = value; }
    }

    /// <summary>
    /// The current or last Id of zone in which this GuildMember was
    /// </summary>
    public int ZoneId
    {
      get
      {
        if(m_chr == null)
          return _lastZoneId;
        if(m_chr.Zone == null)
          return 0;
        return (int) m_chr.Zone.Id;
      }
      internal set { _lastZoneId = value; }
    }

    /// <summary>Class of GuildMember</summary>
    public ClassId Class
    {
      get
      {
        if(m_chr != null)
          return m_chr.Class;
        return (ClassId) _class;
      }
      internal set { _class = (int) value; }
    }

    public GuildRank Rank
    {
      get { return Guild.Ranks[RankId]; }
    }

    public int RankId
    {
      get { return _rankId; }
      set
      {
        _rankId = value;
        if(m_chr != null)
        {
          m_chr.GuildRank = (uint) value;
          foreach(GuildBankTabRights guildBankTabRights in
            m_chr.GuildMember.Rank.BankTabRights
              .Where(right => right != null))
            guildBankTabRights.GuildRankId = (uint) value;
        }

        SaveLater();
      }
    }

    /// <summary>
    /// Returns the Character or null, if this member is offline
    /// </summary>
    public Character Character
    {
      get { return m_chr; }
      internal set
      {
        m_chr = value;
        if(m_chr == null)
          return;
        _name = m_chr.Name;
        m_chr.GuildMember = this;
      }
    }

    public uint BankMoneyWithdrawlAllowance
    {
      get
      {
        if(IsLeader)
          return uint.MaxValue;
        if(DateTime.Now >= BankMoneyAllowanceResetTime)
        {
          BankMoneyAllowanceResetTime = DateTime.Now.AddHours(24.0);
          _remainingMoneyAllowance = (int) Rank.DailyBankMoneyAllowance;
        }

        return (uint) _remainingMoneyAllowance;
      }
      set { _remainingMoneyAllowance = (int) value; }
    }

    public DateTime BankMoneyAllowanceResetTime
    {
      get { return _moneyAllowanceResetTime; }
      set { _moneyAllowanceResetTime = value; }
    }

    public bool IsLeader
    {
      get { return Guild.Leader == this; }
    }

    public byte Asda2RankId
    {
      get { return (byte) (4 - RankId); }
      set { RankId = 4 - value; }
    }

    internal GuildMember()
    {
    }

    internal void Init(Guild guild, Character chr)
    {
      Guild = guild;
      Character = chr;
    }

    /// <summary>Removes this member from its Guild</summary>
    public void LeaveGuild(bool kicked = false)
    {
      Guild.RemoveMember(this, true, kicked);
    }

    public bool HasBankTabRight(int tabId, GuildBankTabPrivileges privilege)
    {
      if(Rank == null || Rank.BankTabRights[tabId] == null)
        return false;
      return Rank.BankTabRights[tabId].Privileges.HasAnyFlag(privilege);
    }

    public override string ToString()
    {
      return "GuildMember: " + Name;
    }

    public bool HasRight(GuildPrivileges privilege)
    {
      if(!IsLeader)
        return Rank.Privileges.HasAnyFlag(privilege);
      return true;
    }

    public void SaveLater()
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(
        SaveAndFlush);
    }
  }
}