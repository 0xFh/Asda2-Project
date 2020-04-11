namespace WCell.RealmServer.Spells.Auras.Misc
{
    /// <summary>
    /// Increases your attack power by $s2 for every ${$m1*$m2} armor value you have.
    /// TODO: Update when armor changes
    /// </summary>
    public class ModAPByArmorHandler : AuraEffectHandler
    {
        private int amt;

        protected override void Apply()
        {
            this.amt = (this.Owner.Armor + this.EffectValue - 1) / this.EffectValue;
            if (this.amt > 0)
                this.Owner.MeleeAttackPowerModsPos += this.amt;
            else
                this.Owner.MeleeAttackPowerModsNeg -= this.amt;
        }

        protected override void Remove(bool cancelled)
        {
            if (this.amt > 0)
                this.Owner.MeleeAttackPowerModsPos -= this.amt;
            else
                this.Owner.MeleeAttackPowerModsNeg += this.amt;
        }
    }
}