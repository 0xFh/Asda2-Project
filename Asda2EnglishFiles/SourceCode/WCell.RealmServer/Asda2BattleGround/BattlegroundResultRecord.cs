using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Asda2BattleGround
{
    [Castle.ActiveRecord.ActiveRecord("BattlegroundResultRecord", Access = PropertyAccess.Property)]
    public class BattlegroundResultRecord : WCellRecord<BattlegroundResultRecord>
    {
        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(BattlegroundResultRecord), nameof(Guid), 1L);

        [Property] public Asda2BattlegroundTown Town { get; set; }

        [Property] public string MvpCharacterName { get; set; }

        [Property] public uint MvpCharacterGuid { get; set; }

        [Property] public int LightScores { get; set; }

        [Property] public int DarkScores { get; set; }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        public bool? IsLightWins
        {
            get
            {
                if (this.LightScores == this.DarkScores)
                    return new bool?();
                return new bool?(this.LightScores > this.DarkScores);
            }
        }

        public BattlegroundResultRecord()
        {
        }

        public BattlegroundResultRecord(Asda2BattlegroundTown town, string mvpCharacterName, uint mvpCharacterGuid,
            int lightScores, int darkScores)
        {
            this.Town = town;
            this.MvpCharacterName = mvpCharacterName;
            this.MvpCharacterGuid = mvpCharacterGuid;
            this.LightScores = lightScores;
            this.DarkScores = darkScores;
            this.Guid = BattlegroundResultRecord._idGenerator.Next();
        }
    }
}