using System.Collections.Generic;

namespace WCell.RealmServer.Talents
{
    public static class GlyphInfoHolder
    {
        public static Dictionary<uint, GlyphSlotEntry> GlyphSlots = new Dictionary<uint, GlyphSlotEntry>();

        public static Dictionary<uint, GlyphPropertiesEntry> GlyphProperties =
            new Dictionary<uint, GlyphPropertiesEntry>();

        public static void Init()
        {
        }

        public static GlyphPropertiesEntry GetPropertiesEntryForGlyph(uint glyphid)
        {
            return GlyphInfoHolder.GlyphProperties[glyphid];
        }

        public static GlyphSlotEntry GetGlyphSlotEntryForGlyphSlotId(uint id)
        {
            return GlyphInfoHolder.GlyphSlots[id];
        }
    }
}