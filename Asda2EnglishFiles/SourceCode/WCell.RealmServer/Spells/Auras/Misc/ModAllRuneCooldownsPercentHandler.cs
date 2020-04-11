using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    /// <summary>
    /// Modifies all rune cooldowns by the EffectValue in percent
    /// </summary>
    public class ModAllRuneCooldownsPercentHandler : AuraEffectHandler
    {
        private float[] deltas;

        protected override void Apply()
        {
            Character owner = this.Owner as Character;
            if (owner == null || owner.PlayerSpells.Runes == null)
                return;
            this.deltas = owner.PlayerSpells.Runes.ModAllCooldownsPercent(this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            if (this.deltas == null)
                return;
            RuneSet runes = ((Character) this.Owner).PlayerSpells.Runes;
            for (RuneType type = RuneType.Blood; type < RuneType.End; ++type)
                runes.ModCooldown(type, -this.deltas[(int) type]);
        }
    }
}