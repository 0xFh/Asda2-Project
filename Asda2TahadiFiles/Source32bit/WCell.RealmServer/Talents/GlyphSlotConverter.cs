using WCell.Core.DBC;

namespace WCell.RealmServer.Talents
{
  public class GlyphSlotConverter : DBCRecordConverter
  {
    public override void Convert(byte[] rawData)
    {
      GlyphSlotEntry glyphSlotEntry = new GlyphSlotEntry();
      glyphSlotEntry.Id = GetUInt32(rawData, 0);
      glyphSlotEntry.TypeFlags = GetUInt32(rawData, 1);
      glyphSlotEntry.Order = GetUInt32(rawData, 2);
      GlyphInfoHolder.GlyphSlots.Add(glyphSlotEntry.Id, glyphSlotEntry);
    }
  }
}