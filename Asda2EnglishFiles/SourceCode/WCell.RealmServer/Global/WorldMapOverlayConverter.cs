using NLog;
using WCell.Constants.World;
using WCell.Core.DBC;
using WCell.Util;

namespace WCell.RealmServer.Global
{
    public class WorldMapOverlayConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            WorldMapOverlayEntry val = new WorldMapOverlayEntry();
            val.WorldMapOverlayId = (WorldMapOverlayId) DBCRecordConverter.GetUInt32(rawData, 0);
            for (int index = 0; index < val.ZoneTemplateId.Length; ++index)
            {
                ZoneId uint32 = (ZoneId) DBCRecordConverter.GetUInt32(rawData, 2 + index);
                if (uint32 != ZoneId.None)
                {
                    val.ZoneTemplateId[index] = uint32;
                    ZoneTemplate zoneTemplate = WCell.RealmServer.Global.World.s_ZoneTemplates[(int) uint32];
                    if (zoneTemplate == null)
                        LogManager.GetCurrentClassLogger().Warn(string.Format(
                            "Invalid ZoneId #{0} found at WorldMapOverlay #{1} during the DBC loading.",
                            (object) uint32, (object) val.WorldMapOverlayId));
                    else
                        zoneTemplate.WorldMapOverlays.Add(val.WorldMapOverlayId);
                }
                else
                    break;
            }

            ArrayUtil.Set<WorldMapOverlayEntry>(ref WCell.RealmServer.Global.World.s_WorldMapOverlayEntries,
                (uint) val.WorldMapOverlayId, val);
        }
    }
}