using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// Represents a mobile Aura-Effect that is applied to everyone in the area.
    /// The SpellCast-object creates AreaAuras explicitely.
    /// </summary>
    public class ApplyAreaAuraEffectHandler : ApplyAuraEffectHandler
    {
        public ApplyAreaAuraEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}