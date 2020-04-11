using WCell.Constants.Misc;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Adds a language</summary>
    public class LanguageEffectHandler : SpellEffectHandler
    {
        public LanguageEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            ((Character) target).AddLanguage((ChatLanguage) this.Effect.MiscValue);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Player; }
        }
    }
}