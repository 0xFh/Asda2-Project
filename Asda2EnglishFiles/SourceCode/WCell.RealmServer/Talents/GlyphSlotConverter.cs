using WCell.Core.DBC;

namespace WCell.RealmServer.Talents
{
    public class GlyphSlotConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            GlyphSlotEntry glyphSlotEntry = new GlyphSlotEntry();
            glyphSlotEntry.Id = DBCRecordConverter.GetUInt32(rawData, 0);
            glyphSlotEntry.TypeFlags = DBCRecordConverter.GetUInt32(rawData, 1);
            glyphSlotEntry.Order = DBCRecordConverter.GetUInt32(rawData, 2);
            GlyphInfoHolder.GlyphSlots.Add(glyphSlotEntry.Id, glyphSlotEntry);
        }
    }
}