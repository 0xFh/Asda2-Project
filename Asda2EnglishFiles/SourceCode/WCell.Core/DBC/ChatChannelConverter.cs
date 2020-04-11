using WCell.Constants.Chat;

namespace WCell.Core.DBC
{
    public class ChatChannelConverter : AdvancedDBCRecordConverter<ChatChannelEntry>
    {
        public override ChatChannelEntry ConvertTo(byte[] rawData, ref int id)
        {
            id = DBCRecordConverter.GetInt32(rawData, 0);
            return new ChatChannelEntry()
            {
                Id = DBCRecordConverter.GetUInt32(rawData, 0),
                ChannelFlags = (ChatChannelFlags) DBCRecordConverter.GetUInt32(rawData, 1)
            };
        }
    }
}