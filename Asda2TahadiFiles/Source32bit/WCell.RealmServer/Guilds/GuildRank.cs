using Castle.ActiveRecord;
using System;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.Util.Threading;

namespace WCell.RealmServer.Guilds
{
  [ActiveRecord("GuildRank", Access = PropertyAccess.Property)]
  public class GuildRank : WCellRecord<GuildRank>
  {
    private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(GuildRank), nameof(_id), 1L);
    [Field("GuildId", NotNull = true)]private int m_GuildId;
    [Field("Privileges", NotNull = true)]private int _privileges;
    [Field("BankMoneyPerDay")]private int _moneyPerDay;
    private GuildBankTabRights[] m_BankTabRights;

    [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
    private int _id { get; set; }

    public uint GuildId
    {
      get { return (uint) m_GuildId; }
      set { m_GuildId = (int) value; }
    }

    [Property(NotNull = true)]
    public string Name { get; set; }

    [Property(NotNull = true)]
    public int RankIndex { get; set; }

    public GuildBankTabRights[] BankTabRights
    {
      get { return m_BankTabRights; }
      set { m_BankTabRights = value; }
    }

    public GuildRank(Guild guild, string name, GuildPrivileges privileges, int id)
    {
      _id = (int) _idGenerator.Next();
      GuildId = guild.Id;
      Name = name;
      _privileges = (int) privileges;
      RankIndex = id;
      BankTabRights = new GuildBankTabRights[6];
      for(int tabId = 0; tabId < 6; ++tabId)
        BankTabRights[tabId] = new GuildBankTabRights(tabId, (uint) _id);
    }

    public static GuildRank[] FindAll(Guild guild)
    {
      return FindAllByProperty("m_GuildId", (int) guild.Id);
    }

    public void SaveLater()
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(
        new Message(Update));
    }

    public override void Create()
    {
      base.Create();
      OnCreated();
    }

    public override void CreateAndFlush()
    {
      base.CreateAndFlush();
      OnCreated();
    }

    private void OnCreated()
    {
      foreach(ActiveRecordBase bankTabRight in m_BankTabRights)
        bankTabRight.Create();
    }

    protected override void OnDelete()
    {
      base.OnDelete();
      foreach(ActiveRecordBase bankTabRight in m_BankTabRights)
        bankTabRight.Delete();
    }

    /// <summary>Init a loaded Rank</summary>
    internal void InitRank()
    {
      m_BankTabRights =
        ActiveRecordBase<GuildBankTabRights>.FindAllByProperty("m_GuildRankId", _id);
      int length = m_BankTabRights.Length;
      Array.Resize(ref m_BankTabRights, 6);
      for(int tabId = length; tabId < m_BankTabRights.Length; ++tabId)
        m_BankTabRights[tabId] = new GuildBankTabRights(tabId, (uint) _id);
    }

    /// <summary>
    /// The daily money withdrawl allowance from the Guild Bank
    /// </summary>
    public uint DailyBankMoneyAllowance
    {
      get { return (uint) _moneyPerDay; }
      set
      {
        if(RankIndex == 0)
          _moneyPerDay = int.MaxValue;
        else
          _moneyPerDay = (int) value;
      }
    }

    public GuildPrivileges Privileges
    {
      get { return (GuildPrivileges) _privileges; }
      set
      {
        _privileges = (int) value;
        SaveLater();
      }
    }

    public GuildRank()
    {
    }
  }
}