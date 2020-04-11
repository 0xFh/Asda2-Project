using WCell.Constants.World;

namespace WCell.RealmServer.Battlegrounds
{
    public class PvPDifficultyEntry
    {
        public int Id;
        public MapId mapId;
        public int bracketId;
        public int minLevel;
        public int maxLevel;
        public int difficulty;
    }
}