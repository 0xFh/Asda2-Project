using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras
{
    public class ModStatPercentHandler : PeriodicallyUpdatedAuraEffectHandler
    {
        protected int[] m_vals;
        protected int m_singleVal;

        protected int GetModifiedValue(int value)
        {
            return (value * this.EffectValue + 50) / 100;
        }

        protected virtual int GetStatValue(StatType stat)
        {
            return this.Owner.GetUnmodifiedBaseStatValue(stat);
        }

        protected override void Apply()
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            switch (this.SpellEffect.MiscValueB)
            {
                case 0:
                    this.m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerPct, this.SpellEffect.MiscValue);
                    break;
                case 1:
                    this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.Health,
                        (float) this.SpellEffect.MiscValue / 100f);
                    break;
                default:
                    owner.ApplyStatMod((ItemModType) this.SpellEffect.MiscValueB, this.SpellEffect.MiscValue);
                    break;
            }

            Asda2CharacterHandler.SendUpdateStatsOneResponse(owner.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(owner.Client);
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            switch (this.SpellEffect.MiscValueB)
            {
                case 0:
                    this.m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerPct, -this.SpellEffect.MiscValue);
                    break;
                case 1:
                    this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.Health,
                        (float) (-(double) this.SpellEffect.MiscValue / 100.0));
                    break;
                default:
                    owner.RemoveStatMod((ItemModType) this.SpellEffect.MiscValueB, this.SpellEffect.MiscValue);
                    break;
            }

            Asda2CharacterHandler.SendUpdateStatsOneResponse(owner.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(owner.Client);
        }

        /// <summary>Re-evaluate effect value, if stats changed</summary>
        public override void Update()
        {
        }
    }
}