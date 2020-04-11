namespace WCell.RealmServer.Spells.Effects
{
    public class VoidNoTargetsEffectHandler : SpellEffectHandler
    {
        public VoidNoTargetsEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }

        public override void Apply()
        {
        }
    }
}