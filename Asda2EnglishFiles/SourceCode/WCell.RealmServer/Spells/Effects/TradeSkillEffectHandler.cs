namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Learns a Trade Skill (add skill checks here?)</summary>
    public class TradeSkillEffectHandler : SpellEffectHandler
    {
        public TradeSkillEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
            int basePoints = this.Effect.BasePoints;
            int diceSides = this.Effect.DiceSides;
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }
    }
}