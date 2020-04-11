using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    internal class ModArmorByPercentOfIntellectHandler : AuraEffectHandler
    {
        private int value;

        protected override void Apply()
        {
            this.value = (this.Owner.Intellect * 100 + 50) / this.EffectValue;
            this.Owner.AddResistanceBuff(DamageSchool.Physical, this.value);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.RemoveResistanceBuff(DamageSchool.Physical, this.value);
        }
    }
}