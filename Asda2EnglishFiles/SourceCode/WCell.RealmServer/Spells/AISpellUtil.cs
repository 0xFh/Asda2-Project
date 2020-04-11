using WCell.Constants.Spells;
using WCell.RealmServer.Spells.Targeting;

namespace WCell.RealmServer.Spells
{
    public static class AISpellUtil
    {
        public static AISpellCooldownCategory GetAISpellCooldownCategory(this Spell spell)
        {
            bool isBeneficial = spell.IsBeneficial;
            if (spell.IsAura)
                return !isBeneficial ? AISpellCooldownCategory.AuraHarmful : AISpellCooldownCategory.AuraBeneficial;
            return !isBeneficial ? AISpellCooldownCategory.DirectHarmful : AISpellCooldownCategory.DirectBeneficial;
        }

        /// <summary>Called on every SpellEffect, after initialization</summary>
        internal static void DecideDefaultTargetHandlerDefintion(SpellEffect effect)
        {
            if (effect.AITargetHandlerDefintion != null || effect.AITargetEvaluator != null ||
                (effect.HarmType != HarmType.Beneficial || effect.HasTarget(ImplicitSpellTargetType.Self)))
                return;
            if (!effect.IsAreaEffect)
            {
                effect.Spell.MaxTargets = 1U;
                effect.SetAITargetDefinition(new TargetAdder(DefaultTargetAdders.AddAreaSource),
                    new TargetFilter(DefaultTargetFilters.IsFriendly));
            }
            else
                effect.SetAITargetDefinition(new TargetAdder(DefaultTargetAdders.AddAreaSource),
                    new TargetFilter(DefaultTargetFilters.IsFriendly));

            if (effect.IsHealEffect)
                effect.AITargetEvaluator = new TargetEvaluator(DefaultTargetEvaluators.MostWoundedEvaluator);
            else
                effect.AITargetEvaluator = new TargetEvaluator(DefaultTargetEvaluators.RandomEvaluator);
        }
    }
}