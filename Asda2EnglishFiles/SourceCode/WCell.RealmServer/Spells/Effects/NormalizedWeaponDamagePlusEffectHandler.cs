namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Adds flat melee damage to the attack</summary>
    public class NormalizedWeaponDamagePlusEffectHandler : WeaponDamageEffectHandler
    {
        public NormalizedWeaponDamagePlusEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }
    }
}