using Cell.Core;
using WCell.Core.DBC;

namespace WCell.RealmServer.Taxi
{
    public class DBCTaxiPathConverter : AdvancedDBCRecordConverter<TaxiPath>
    {
        public override TaxiPath ConvertTo(byte[] rawData, ref int id)
        {
            TaxiPath taxiPath = new TaxiPath();
            id = (int) (taxiPath.Id = rawData.GetUInt32(0U));
            taxiPath.StartNodeId = rawData.GetUInt32(1U);
            taxiPath.EndNodeId = rawData.GetUInt32(2U);
            taxiPath.Price = rawData.GetUInt32(3U);
            return taxiPath;
        }
    }
}