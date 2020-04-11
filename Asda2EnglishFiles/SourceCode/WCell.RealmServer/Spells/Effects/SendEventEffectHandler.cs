using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Quest related</summary>
    public class SendEventEffectHandler : SpellEffectHandler
    {
        public SendEventEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
            int miscValue = effect.MiscValue;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
        }
    }
}