using WCell.Constants.Chat;

namespace WCell.Core.DBC
{
  public class ChatChannelConverter : AdvancedDBCRecordConverter<ChatChannelEntry>
  {
    public override ChatChannelEntry ConvertTo(byte[] rawData, ref int id)
    {
      id = GetInt32(rawData, 0);
      return new ChatChannelEntry
      {
        Id = GetUInt32(rawData, 0),
        ChannelFlags = (ChatChannelFlags) GetUInt32(rawData, 1)
      };
    }
  }
}