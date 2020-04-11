using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Increases healing done</summary>
    public class ModHealingDoneHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.HealingDoneMod += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.HealingDoneMod -= this.EffectValue;
        }
    }
}