using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Increases healing done by %</summary>
    public class ModHealingTakenPctHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.HealingTakenModPct += (float) this.SpellEffect.MiscValue;
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.HealingTakenModPct -= (float) this.SpellEffect.MiscValue;
        }
    }
}