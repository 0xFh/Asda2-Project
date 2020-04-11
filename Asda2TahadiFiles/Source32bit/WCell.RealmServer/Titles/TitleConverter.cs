using WCell.Constants.Misc;
using WCell.Core.DBC;

namespace WCell.RealmServer.Titles
{
  public class TitleConverter : DBCRecordConverter
  {
    public override void Convert(byte[] rawData)
    {
      CharacterTitleEntry characterTitleEntry = new CharacterTitleEntry
      {
        TitleId = (TitleId) GetUInt32(rawData, 0),
        Names = GetStrings(rawData, 2),
        BitIndex = (TitleBitId) GetUInt32(rawData, 36)
      };
      TitleMgr.CharacterTitleEntries[characterTitleEntry.TitleId] = characterTitleEntry;
    }
  }
}