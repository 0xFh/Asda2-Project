using WCell.Constants.Spells;
using WCell.RealmServer.Talents;

namespace WCell.RealmServer.Spells.Effects
{
    public class ApplyGlyphEffectHandler : SpellEffectHandler
    {
        public ApplyGlyphEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            return this.m_cast.GlyphSlot != 0U &&
                   (int) GlyphInfoHolder.GetPropertiesEntryForGlyph((uint) this.m_cast.Spell.Effects[0].MiscValue)
                       .TypeFlags !=
                   (int) GlyphInfoHolder
                       .GetGlyphSlotEntryForGlyphSlotId(
                           this.m_cast.CasterChar.GetGlyphSlot((byte) this.m_cast.GlyphSlot)).TypeFlags
                ? SpellFailedReason.InvalidGlyph
                : SpellFailedReason.Ok;
        }

        public override void Apply()
        {
            this.m_cast.CasterChar.ApplyGlyph((byte) this.m_cast.GlyphSlot,
                GlyphInfoHolder.GetPropertiesEntryForGlyph((uint) this.m_cast.Spell.Effects[0].MiscValue));
        }
    }
}