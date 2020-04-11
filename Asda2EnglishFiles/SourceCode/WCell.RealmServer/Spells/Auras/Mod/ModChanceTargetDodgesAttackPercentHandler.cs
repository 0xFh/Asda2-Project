using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    /// <summary>Only used for WarriorArmsWeaponMastery</summary>
    public class ModChanceTargetDodgesAttackPercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.IntMods[17] += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.IntMods[17] -= this.EffectValue;
        }
    }
}