namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModAttackerSpellCritChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.AttackerSpellCritChancePercentMod += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.AttackerSpellCritChancePercentMod -= this.EffectValue;
        }
    }
}