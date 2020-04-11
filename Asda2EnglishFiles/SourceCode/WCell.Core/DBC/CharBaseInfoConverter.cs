using WCell.Constants;

namespace WCell.Core.DBC
{
    public class CharBaseInfoConverter : AdvancedDBCRecordConverter<CharBaseInfo>
    {
        public override CharBaseInfo ConvertTo(byte[] rawData, ref int id)
        {
            id = 0;
            return new CharBaseInfo()
            {
                Race = (RaceId) rawData[0],
                Class = (ClassId) rawData[1]
            };
        }
    }
}