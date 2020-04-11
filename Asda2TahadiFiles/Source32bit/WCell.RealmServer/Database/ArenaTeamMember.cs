using Castle.ActiveRecord;
using WCell.Constants;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Battlegrounds.Arenas
{
  /// <summary>
  /// Represents the relationship between a Character and its Arena Team.
  /// </summary>
  [ActiveRecord("ArenaTeamMember", Access = PropertyAccess.Property)]
  public class ArenaTeamMember : WCellRecord<ArenaTeamMember>, INamed
  {
    private Character m_chr;
    private ArenaTeam m_Team;
    [Field("ArenaTeamId", NotNull = true)]private int _arenaTeamId;
    [Field("Name", NotNull = true)]private string _name;
    [Field("Class", NotNull = true)]private int _class;
    [Field("GamesWeek", NotNull = true)]private int _gamesWeek;
    [Field("WinsWeek", NotNull = true)]private int _winsWeek;
    [Field("GamesSeason", NotNull = true)]private int _gamesSeason;
    [Field("WinsSeason", NotNull = true)]private int _winsSeason;

    [Field("PersonalRating", NotNull = true)]
    private int _personalRating;

    /// <summary>
    /// The low part of the Character's EntityId. Use EntityId.GetPlayerId(Id) to get a full EntityId
    /// </summary>
    public uint Id
    {
      get { return (uint) CharacterLowId; }
    }

    /// <summary>The name of this ArenaTeamMember's character</summary>
    public string Name
    {
      get { return _name; }
    }

    public ArenaTeam ArenaTeam
    {
      get { return m_Team; }
      private set
      {
        m_Team = value;
        ArenaTeamId = value.Id;
      }
    }

    /// <summary>Class of ArenaTeamMember</summary>
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
        m_chr.ArenaTeamMember[(int) ArenaTeam.Slot] = this;
      }
    }

    public uint GamesWeek
    {
      get { return (uint) _gamesWeek; }
      set { _gamesWeek = (int) value; }
    }

    public uint WinsWeek
    {
      get { return (uint) _winsWeek; }
      set { _winsWeek = (int) value; }
    }

    public uint GamesSeason
    {
      get { return (uint) _gamesSeason; }
      set { _gamesSeason = (int) value; }
    }

    public uint WinsSeason
    {
      get { return (uint) _winsSeason; }
      set { _winsSeason = (int) value; }
    }

    public uint PersonalRating
    {
      get { return (uint) _personalRating; }
      set { _personalRating = (int) value; }
    }

    internal void Init(ArenaTeam team)
    {
      Init(team, World.GetCharacter((uint) CharacterLowId));
    }

    internal void Init(ArenaTeam team, Character chr)
    {
      ArenaTeam = team;
      Character = chr;
    }

    /// <summary>Removes this member from its team</summary>
    public void LeaveArenaTeam()
    {
      ArenaTeam.RemoveMember(this, true);
    }

    [PrimaryKey(PrimaryKeyType.Assigned)]
    public int CharacterLowId { get; private set; }

    public uint ArenaTeamId
    {
      get { return (uint) _arenaTeamId; }
      set { _arenaTeamId = (int) value; }
    }

    public static ArenaTeamMember[] FindAll(ArenaTeam team)
    {
      return FindAllByProperty("_arenaTeamId", (int) team.Id);
    }

    public ArenaTeamMember()
    {
    }

    public ArenaTeamMember(CharacterRecord chr, ArenaTeam team, bool isLeader)
      : this()
    {
      ArenaTeam = team;
      CharacterLowId = (int) chr.EntityLowId;
      ArenaTeamId = team.Id;
      _name = chr.Name;
      _class = (int) chr.Class;
      _gamesWeek = 0;
      _winsWeek = 0;
      _gamesSeason = 0;
      _winsSeason = 0;
      _personalRating = 1500;
    }
  }
}