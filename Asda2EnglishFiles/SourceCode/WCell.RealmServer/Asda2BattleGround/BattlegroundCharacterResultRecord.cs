using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Asda2BattleGround
{
    [Castle.ActiveRecord.ActiveRecord("BattlegroundCharacterResultRecord", Access = PropertyAccess.Property)]
    public class BattlegroundCharacterResultRecord : WCellRecord<BattlegroundCharacterResultRecord>
    {
        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(BattlegroundCharacterResultRecord), nameof(Guid), 1L);

        [Property] public long WarGuid { get; set; }

        [Property] public string CharacterName { get; set; }

        [Property] public uint CharacterGuid { get; set; }

        [Property] public int ActScores { get; set; }

        [Property] public int Kills { get; set; }

        [Property] public int Deathes { get; set; }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        public BattlegroundCharacterResultRecord()
        {
        }

        public BattlegroundCharacterResultRecord(long warGuid, string characterName, uint characterGuid, int actScores,
            int kills, int deathes)
        {
            this.WarGuid = warGuid;
            this.CharacterName = characterName;
            this.CharacterGuid = characterGuid;
            this.ActScores = actScores;
            this.Kills = kills;
            this.Deathes = deathes;
            this.Guid = BattlegroundCharacterResultRecord._idGenerator.Next();
        }
    }
}