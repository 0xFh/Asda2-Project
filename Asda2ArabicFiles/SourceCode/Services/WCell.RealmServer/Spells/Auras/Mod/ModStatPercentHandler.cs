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
			return (value * EffectValue + 50) / 100;
		}

		protected virtual int GetStatValue(StatType stat)
		{
			return Owner.GetUnmodifiedBaseStatValue(stat);
		}

		protected override void Apply()
		{
		    var owner = Owner as Character;
            if(owner == null)return;
		    switch ((ItemModType)SpellEffect.MiscValueB)
		    {
		            case ItemModType.Health:
                    m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.Health, (float)SpellEffect.MiscValue/100);
                    break;
                    case ItemModType.Power:
                    m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerPct, SpellEffect.MiscValue);
                    break;
                    default:
                    owner.ApplyStatMod((ItemModType)SpellEffect.MiscValueB, SpellEffect.MiscValue);
                    break;
		    }
		    Asda2CharacterHandler.SendUpdateStatsOneResponse(owner.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(owner.Client);
		}

		protected override void Remove(bool cancelled)
        {
            var owner = Owner as Character;
            if (owner == null) return;
            switch ((ItemModType)SpellEffect.MiscValueB)
            {
                case ItemModType.Health:
                    m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.Health, -(float)SpellEffect.MiscValue / 100);
                    break;
                case ItemModType.Power:
                    m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerPct, -SpellEffect.MiscValue);
                    break;
                default:
                    owner.RemoveStatMod((ItemModType)SpellEffect.MiscValueB, SpellEffect.MiscValue);
                    break;
            }
            Asda2CharacterHandler.SendUpdateStatsOneResponse(owner.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(owner.Client);
		}

		/// <summary>
		/// Re-evaluate effect value, if stats changed
		/// </summary>
		public override void Update()
		{
			
		}
	}
}