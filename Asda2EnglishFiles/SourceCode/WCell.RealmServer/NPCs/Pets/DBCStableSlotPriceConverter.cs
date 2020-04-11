using WCell.Core.DBC;
using WCell.Util;

namespace WCell.RealmServer.NPCs.Pets
{
    public class DBCStableSlotPriceConverter : AdvancedDBCRecordConverter<uint>
    {
        public override uint ConvertTo(byte[] rawData, ref int id)
        {
            uint field = 0;
            id = rawData.GetInt32(field++);
            return rawData.GetUInt32(field);
        }
    }
}