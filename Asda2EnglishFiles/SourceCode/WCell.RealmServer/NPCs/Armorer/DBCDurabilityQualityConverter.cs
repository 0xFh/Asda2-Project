using Cell.Core;
using WCell.Core.DBC;

namespace WCell.RealmServer.NPCs.Armorer
{
    public class DBCDurabilityQualityConverter : AdvancedDBCRecordConverter<DurabilityQuality>
    {
        public override DurabilityQuality ConvertTo(byte[] rawData, ref int id)
        {
            DurabilityQuality durabilityQuality = new DurabilityQuality();
            id = (int) (durabilityQuality.Id = rawData.GetUInt32(0U));
            durabilityQuality.CostModifierPct = (uint) ((double) rawData.GetFloat(1U) * 100.0);
            return durabilityQuality;
        }
    }
}