using WCell.Constants.World;

namespace WCell.RealmServer.Global
{
    public class WorldMapOverlayEntry
    {
        public WorldMapOverlayId WorldMapOverlayId;
        public ZoneId[] ZoneTemplateId;

        public WorldMapOverlayEntry()
        {
            this.ZoneTemplateId = new ZoneId[4];
        }
    }
}