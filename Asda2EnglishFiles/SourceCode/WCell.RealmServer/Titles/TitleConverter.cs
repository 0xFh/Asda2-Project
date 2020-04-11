using WCell.Constants.Misc;
using WCell.Core.DBC;

namespace WCell.RealmServer.Titles
{
    public class TitleConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            CharacterTitleEntry characterTitleEntry = new CharacterTitleEntry()
            {
                TitleId = (TitleId) DBCRecordConverter.GetUInt32(rawData, 0),
                Names = this.GetStrings(rawData, 2),
                BitIndex = (TitleBitId) DBCRecordConverter.GetUInt32(rawData, 36)
            };
            TitleMgr.CharacterTitleEntries[characterTitleEntry.TitleId] = characterTitleEntry;
        }
    }
}