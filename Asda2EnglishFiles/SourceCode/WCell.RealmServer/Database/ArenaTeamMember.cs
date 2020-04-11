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
    [Castle.ActiveRecord.ActiveRecord("ArenaTeamMember", Access = PropertyAccess.Property)]
    public class ArenaTeamMember : WCellRecord<ArenaTeamMember>, INamed
    {
        private Character m_chr;
        private ArenaTeam m_Team;
        [Field("ArenaTeamId", NotNull = true)] private int _arenaTeamId;
        [Field("Name", NotNull = true)] private string _name;
        [Field("Class", NotNull = true)] private int _class;
        [Field("GamesWeek", NotNull = true)] private int _gamesWeek;
        [Field("WinsWeek", NotNull = true)] private int _winsWeek;
        [Field("GamesSeason", NotNull = true)] private int _gamesSeason;
        [Field("WinsSeason", NotNull = true)] private int _winsSeason;

        [Field("PersonalRating", NotNull = true)]
        private int _personalRating;

        /// <summary>
        /// The low part of the Character's EntityId. Use EntityId.GetPlayerId(Id) to get a full EntityId
        /// </summary>
        public uint Id
        {
            get { return (uint) this.CharacterLowId; }
        }

        /// <summary>The name of this ArenaTeamMember's character</summary>
        public string Name
        {
            get { return this._name; }
        }

        public ArenaTeam ArenaTeam
        {
            get { return this.m_Team; }
            private set
            {
                this.m_Team = value;
                this.ArenaTeamId = value.Id;
            }
        }

        /// <summary>Class of ArenaTeamMember</summary>
        public ClassId Class
        {
            get
            {
                if (this.m_chr != null)
                    return this.m_chr.Class;
                return (ClassId) this._class;
            }
            internal set { this._class = (int) value; }
        }

        /// <summary>
        /// Returns the Character or null, if this member is offline
        /// </summary>
        public Character Character
        {
            get { return this.m_chr; }
            internal set
            {
                this.m_chr = value;
                if (this.m_chr == null)
                    return;
                this._name = this.m_chr.Name;
                this.m_chr.ArenaTeamMember[(int) this.ArenaTeam.Slot] = this;
            }
        }

        public uint GamesWeek
        {
            get { return (uint) this._gamesWeek; }
            set { this._gamesWeek = (int) value; }
        }

        public uint WinsWeek
        {
            get { return (uint) this._winsWeek; }
            set { this._winsWeek = (int) value; }
        }

        public uint GamesSeason
        {
            get { return (uint) this._gamesSeason; }
            set { this._gamesSeason = (int) value; }
        }

        public uint WinsSeason
        {
            get { return (uint) this._winsSeason; }
            set { this._winsSeason = (int) value; }
        }

        public uint PersonalRating
        {
            get { return (uint) this._personalRating; }
            set { this._personalRating = (int) value; }
        }

        internal void Init(ArenaTeam team)
        {
            this.Init(team, World.GetCharacter((uint) this.CharacterLowId));
        }

        internal void Init(ArenaTeam team, Character chr)
        {
            this.ArenaTeam = team;
            this.Character = chr;
        }

        /// <summary>Removes this member from its team</summary>
        public void LeaveArenaTeam()
        {
            this.ArenaTeam.RemoveMember(this, true);
        }

        [PrimaryKey(PrimaryKeyType.Assigned)] public int CharacterLowId { get; private set; }

        public uint ArenaTeamId
        {
            get { return (uint) this._arenaTeamId; }
            set { this._arenaTeamId = (int) value; }
        }

        public static ArenaTeamMember[] FindAll(ArenaTeam team)
        {
            return ActiveRecordBase<ArenaTeamMember>.FindAllByProperty("_arenaTeamId", (object) (int) team.Id);
        }

        public ArenaTeamMember()
        {
        }

        public ArenaTeamMember(CharacterRecord chr, ArenaTeam team, bool isLeader)
            : this()
        {
            this.ArenaTeam = team;
            this.CharacterLowId = (int) chr.EntityLowId;
            this.ArenaTeamId = team.Id;
            this._name = chr.Name;
            this._class = (int) chr.Class;
            this._gamesWeek = 0;
            this._winsWeek = 0;
            this._gamesSeason = 0;
            this._winsSeason = 0;
            this._personalRating = 1500;
        }
    }
}