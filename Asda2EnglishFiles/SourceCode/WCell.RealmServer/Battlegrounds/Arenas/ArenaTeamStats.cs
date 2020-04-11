using Castle.ActiveRecord;
using WCell.Constants.ArenaTeams;
using WCell.Core.Database;

namespace WCell.RealmServer.Battlegrounds.Arenas
{
    [Castle.ActiveRecord.ActiveRecord("ArenaTeamStats", Access = PropertyAccess.Property)]
    public class ArenaTeamStats : WCellRecord<ArenaTeamStats>
    {
        [Field("Rating", NotNull = true)] private int _rating;
        [Field("GamesWeek", NotNull = true)] private int _gamesWeek;
        [Field("WinsWeek", NotNull = true)] private int _winsWeek;
        [Field("GamesSeason", NotNull = true)] private int _gamesSeason;
        [Field("WinsSeason", NotNull = true)] private int _winsSeason;
        [Field("Rank", NotNull = true)] private int _rank;

        public ArenaTeam team
        {
            set
            {
                this.team = value;
                this._teamId = (int) value.Id;
            }
        }

        public uint rating
        {
            get { return (uint) this._rating; }
            set { this._rating = (int) value; }
        }

        public uint gamesWeek
        {
            get { return (uint) this._gamesWeek; }
            set { this._gamesWeek = (int) value; }
        }

        public uint winsWeek
        {
            get { return (uint) this._winsWeek; }
            set { this._winsWeek = (int) value; }
        }

        public uint gamesSeason
        {
            get { return (uint) this._gamesSeason; }
            set { this._gamesSeason = (int) value; }
        }

        public uint winsSeason
        {
            get { return (uint) this._winsSeason; }
            set { this._winsSeason = (int) value; }
        }

        public uint rank
        {
            get { return (uint) this._rank; }
            set { this._rank = (int) value; }
        }

        public ArenaTeamStats()
        {
        }

        public ArenaTeamStats(ArenaTeam arenaTeam)
        {
            this.team = arenaTeam;
            this.rating = 1500U;
            this.gamesWeek = 0U;
            this.winsWeek = 0U;
            this.gamesSeason = 0U;
            this.winsSeason = 0U;
            this.rank = 0U;
        }

        public void SetStats(ArenaTeamStatsTypes stats, uint value)
        {
            switch (stats)
            {
                case ArenaTeamStatsTypes.STAT_TYPE_RATING:
                    this.rating = value;
                    break;
                case ArenaTeamStatsTypes.STAT_TYPE_GAMES_WEEK:
                    this.gamesWeek = value;
                    break;
                case ArenaTeamStatsTypes.STAT_TYPE_WINS_WEEK:
                    this.winsWeek = value;
                    break;
                case ArenaTeamStatsTypes.STAT_TYPE_GAMES_SEASON:
                    this.gamesSeason = value;
                    break;
                case ArenaTeamStatsTypes.STAT_TYPE_WINS_SEASON:
                    this.winsSeason = value;
                    break;
            }

            this.UpdateAndFlush();
        }

        [PrimaryKey(PrimaryKeyType.Assigned, "ArenaTeamId")]
        private int _teamId { get; set; }
    }
}