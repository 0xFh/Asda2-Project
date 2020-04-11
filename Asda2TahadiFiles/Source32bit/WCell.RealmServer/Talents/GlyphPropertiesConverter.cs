using WCell.Core.DBC;

namespace WCell.RealmServer.Talents
{
  public class GlyphPropertiesConverter : DBCRecordConverter
  {
    public override void Convert(byte[] rawData)
    {
      GlyphPropertiesEntry glyphPropertiesEntry = new GlyphPropertiesEntry();
      glyphPropertiesEntry.Id = GetUInt32(rawData, 0);
      glyphPropertiesEntry.SpellId = GetUInt32(rawData, 1);
      glyphPropertiesEntry.TypeFlags = GetUInt32(rawData, 2);
      glyphPropertiesEntry.Unk1 = GetUInt32(rawData, 3);
      GlyphInfoHolder.GlyphProperties.Add(glyphPropertiesEntry.Id, glyphPropertiesEntry);
    }
  }
}