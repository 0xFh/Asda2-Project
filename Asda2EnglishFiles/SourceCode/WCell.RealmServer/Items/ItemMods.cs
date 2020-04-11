using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Modifiers
{
    internal static class ItemMods
    {
        public static readonly ItemMods.ItemModHandler[] AddHandlers = new ItemMods.ItemModHandler[100];
        public static readonly ItemMods.ItemModHandler[] RemoveHandlers = new ItemMods.ItemModHandler[100];

        static ItemMods()
        {
            ItemMods.AddHandlers[2] = new ItemMods.ItemModHandler(ItemMods.AddUnused);
            ItemMods.AddHandlers[0] = new ItemMods.ItemModHandler(ItemMods.AddPower);
            ItemMods.AddHandlers[1] = new ItemMods.ItemModHandler(ItemMods.AddHealth);
            ItemMods.AddHandlers[3] = new ItemMods.ItemModHandler(ItemMods.AddAgility);
            ItemMods.AddHandlers[4] = new ItemMods.ItemModHandler(ItemMods.AddStrength);
            ItemMods.AddHandlers[5] = new ItemMods.ItemModHandler(ItemMods.AddIntellect);
            ItemMods.AddHandlers[6] = new ItemMods.ItemModHandler(ItemMods.AddSpirit);
            ItemMods.AddHandlers[7] = new ItemMods.ItemModHandler(ItemMods.AddStamina);
            ItemMods.AddHandlers[11] = new ItemMods.ItemModHandler(ItemMods.AddWeaponSkillRating);
            ItemMods.AddHandlers[12] = new ItemMods.ItemModHandler(ItemMods.AddDefenseRating);
            ItemMods.AddHandlers[13] = new ItemMods.ItemModHandler(ItemMods.AddDodgeRating);
            ItemMods.AddHandlers[14] = new ItemMods.ItemModHandler(ItemMods.AddParryRating);
            ItemMods.AddHandlers[15] = new ItemMods.ItemModHandler(ItemMods.AddBlockRating);
            ItemMods.AddHandlers[16] = new ItemMods.ItemModHandler(ItemMods.AddMeleeHitRating);
            ItemMods.AddHandlers[17] = new ItemMods.ItemModHandler(ItemMods.AddRangedHitRating);
            ItemMods.AddHandlers[18] = new ItemMods.ItemModHandler(ItemMods.AddSpellHitRating);
            ItemMods.AddHandlers[19] = new ItemMods.ItemModHandler(ItemMods.AddMeleeCriticalStrikeRating);
            ItemMods.AddHandlers[20] = new ItemMods.ItemModHandler(ItemMods.AddRangedCriticalStrikeRating);
            ItemMods.AddHandlers[21] = new ItemMods.ItemModHandler(ItemMods.AddSpellCriticalStrikeRating);
            ItemMods.AddHandlers[22] = new ItemMods.ItemModHandler(ItemMods.AddMeleeHitAvoidanceRating);
            ItemMods.AddHandlers[23] = new ItemMods.ItemModHandler(ItemMods.AddRangedHitAvoidanceRating);
            ItemMods.AddHandlers[24] = new ItemMods.ItemModHandler(ItemMods.AddSpellHitAvoidanceRating);
            ItemMods.AddHandlers[25] = new ItemMods.ItemModHandler(ItemMods.AddMeleeCriticalAvoidanceRating);
            ItemMods.AddHandlers[26] = new ItemMods.ItemModHandler(ItemMods.AddRangedCriticalAvoidanceRating);
            ItemMods.AddHandlers[27] = new ItemMods.ItemModHandler(ItemMods.AddSpellCriticalAvoidanceRating);
            ItemMods.AddHandlers[28] = new ItemMods.ItemModHandler(ItemMods.AddMeleeHasteRating);
            ItemMods.AddHandlers[29] = new ItemMods.ItemModHandler(ItemMods.AddRangedHasteRating);
            ItemMods.AddHandlers[30] = new ItemMods.ItemModHandler(ItemMods.AddSpellHasteRating);
            ItemMods.AddHandlers[31] = new ItemMods.ItemModHandler(ItemMods.AddHitRating);
            ItemMods.AddHandlers[32] = new ItemMods.ItemModHandler(ItemMods.AddCriticalStrikeRating);
            ItemMods.AddHandlers[33] = new ItemMods.ItemModHandler(ItemMods.AddHitAvoidanceRating);
            ItemMods.AddHandlers[34] = new ItemMods.ItemModHandler(ItemMods.AddCriticalAvoidanceRating);
            ItemMods.AddHandlers[35] = new ItemMods.ItemModHandler(ItemMods.AddResilienceRating);
            ItemMods.AddHandlers[36] = new ItemMods.ItemModHandler(ItemMods.AddHasteRating);
            ItemMods.AddHandlers[37] = new ItemMods.ItemModHandler(ItemMods.AddExpertiseRating);
            ItemMods.RemoveHandlers[2] = new ItemMods.ItemModHandler(ItemMods.RemoveUnused);
            ItemMods.RemoveHandlers[0] = new ItemMods.ItemModHandler(ItemMods.RemovePower);
            ItemMods.RemoveHandlers[1] = new ItemMods.ItemModHandler(ItemMods.RemoveHealth);
            ItemMods.RemoveHandlers[3] = new ItemMods.ItemModHandler(ItemMods.RemoveAgility);
            ItemMods.RemoveHandlers[4] = new ItemMods.ItemModHandler(ItemMods.RemoveStrength);
            ItemMods.RemoveHandlers[5] = new ItemMods.ItemModHandler(ItemMods.RemoveIntellect);
            ItemMods.RemoveHandlers[6] = new ItemMods.ItemModHandler(ItemMods.RemoveSpirit);
            ItemMods.RemoveHandlers[7] = new ItemMods.ItemModHandler(ItemMods.RemoveStamina);
            ItemMods.RemoveHandlers[11] = new ItemMods.ItemModHandler(ItemMods.RemoveWeaponSkillRating);
            ItemMods.RemoveHandlers[12] = new ItemMods.ItemModHandler(ItemMods.RemoveDefenseRating);
            ItemMods.RemoveHandlers[13] = new ItemMods.ItemModHandler(ItemMods.RemoveDodgeRating);
            ItemMods.RemoveHandlers[14] = new ItemMods.ItemModHandler(ItemMods.RemoveParryRating);
            ItemMods.RemoveHandlers[15] = new ItemMods.ItemModHandler(ItemMods.RemoveBlockRating);
            ItemMods.RemoveHandlers[16] = new ItemMods.ItemModHandler(ItemMods.RemoveMeleeHitRating);
            ItemMods.RemoveHandlers[17] = new ItemMods.ItemModHandler(ItemMods.RemoveRangedHitRating);
            ItemMods.RemoveHandlers[18] = new ItemMods.ItemModHandler(ItemMods.RemoveSpellHitRating);
            ItemMods.RemoveHandlers[19] = new ItemMods.ItemModHandler(ItemMods.RemoveMeleeCriticalStrikeRating);
            ItemMods.RemoveHandlers[20] = new ItemMods.ItemModHandler(ItemMods.RemoveRangedCriticalStrikeRating);
            ItemMods.RemoveHandlers[21] = new ItemMods.ItemModHandler(ItemMods.RemoveSpellCriticalStrikeRating);
            ItemMods.RemoveHandlers[22] = new ItemMods.ItemModHandler(ItemMods.RemoveMeleeHitAvoidanceRating);
            ItemMods.RemoveHandlers[23] = new ItemMods.ItemModHandler(ItemMods.RemoveRangedHitAvoidanceRating);
            ItemMods.RemoveHandlers[24] = new ItemMods.ItemModHandler(ItemMods.RemoveSpellHitAvoidanceRating);
            ItemMods.RemoveHandlers[25] = new ItemMods.ItemModHandler(ItemMods.RemoveMeleeCriticalAvoidanceRating);
            ItemMods.RemoveHandlers[26] = new ItemMods.ItemModHandler(ItemMods.RemoveRangedCriticalAvoidanceRating);
            ItemMods.RemoveHandlers[27] = new ItemMods.ItemModHandler(ItemMods.RemoveSpellCriticalAvoidanceRating);
            ItemMods.RemoveHandlers[28] = new ItemMods.ItemModHandler(ItemMods.RemoveMeleeHasteRating);
            ItemMods.RemoveHandlers[29] = new ItemMods.ItemModHandler(ItemMods.RemoveRangedHasteRating);
            ItemMods.RemoveHandlers[30] = new ItemMods.ItemModHandler(ItemMods.RemoveSpellHasteRating);
            ItemMods.RemoveHandlers[31] = new ItemMods.ItemModHandler(ItemMods.RemoveHitRating);
            ItemMods.RemoveHandlers[32] = new ItemMods.ItemModHandler(ItemMods.RemoveCriticalStrikeRating);
            ItemMods.RemoveHandlers[33] = new ItemMods.ItemModHandler(ItemMods.RemoveHitAvoidanceRating);
            ItemMods.RemoveHandlers[34] = new ItemMods.ItemModHandler(ItemMods.RemoveCriticalAvoidanceRating);
            ItemMods.RemoveHandlers[35] = new ItemMods.ItemModHandler(ItemMods.RemoveResilienceRating);
            ItemMods.RemoveHandlers[36] = new ItemMods.ItemModHandler(ItemMods.RemoveHasteRating);
            ItemMods.RemoveHandlers[37] = new ItemMods.ItemModHandler(ItemMods.RemoveExpertiseRating);
            ItemMods.AddHandlers[42] = new ItemMods.ItemModHandler(ItemMods.AddSpellDamageDone);
            ItemMods.RemoveHandlers[42] = new ItemMods.ItemModHandler(ItemMods.RemoveSpellDamageDone);
            ItemMods.AddHandlers[41] = new ItemMods.ItemModHandler(ItemMods.AddSpellHealingDone);
            ItemMods.RemoveHandlers[41] = new ItemMods.ItemModHandler(ItemMods.RemoveSpellHealingDone);
            ItemMods.AddHandlers[45] = new ItemMods.ItemModHandler(ItemMods.AddSpellPower);
            ItemMods.RemoveHandlers[45] = new ItemMods.ItemModHandler(ItemMods.RemoveSpellPower);
            ItemMods.AddHandlers[48] = new ItemMods.ItemModHandler(ItemMods.AddBlockValue);
            ItemMods.RemoveHandlers[48] = new ItemMods.ItemModHandler(ItemMods.RemoveBlockValue);
            ItemMods.AddHandlers[43] = new ItemMods.ItemModHandler(ItemMods.AddManaRegen);
            ItemMods.RemoveHandlers[43] = new ItemMods.ItemModHandler(ItemMods.RemoveManaRegen);
            ItemMods.AddHandlers[46] = new ItemMods.ItemModHandler(ItemMods.AddHealthRegen);
            ItemMods.RemoveHandlers[46] = new ItemMods.ItemModHandler(ItemMods.RemoveHealthRegen);
            ItemMods.AddHandlers[80] = new ItemMods.ItemModHandler(ItemMods.AddHealthRegenInCombat);
            ItemMods.RemoveHandlers[80] = new ItemMods.ItemModHandler(ItemMods.RemoveHealthRegenInCombat);
            ItemMods.AddHandlers[54] = new ItemMods.ItemModHandler(ItemMods.AddLuckValue);
            ItemMods.RemoveHandlers[54] = new ItemMods.ItemModHandler(ItemMods.RemoveLuckValue);
            ItemMods.AddHandlers[84] = new ItemMods.ItemModHandler(ItemMods.AddSellingCost);
            ItemMods.RemoveHandlers[84] = new ItemMods.ItemModHandler(ItemMods.RemoveSellingCost);
            ItemMods.AddHandlers[49] = new ItemMods.ItemModHandler(ItemMods.AddAtackTimePrcValue);
            ItemMods.RemoveHandlers[49] = new ItemMods.ItemModHandler(ItemMods.RemoveAtackTimePrcValue);
            ItemMods.AddHandlers[50] = new ItemMods.ItemModHandler(ItemMods.AddAsda2DefenceValue);
            ItemMods.RemoveHandlers[50] = new ItemMods.ItemModHandler(ItemMods.RemoveAsda2DefenceValue);
            ItemMods.AddHandlers[64] = new ItemMods.ItemModHandler(ItemMods.AddAsda2DefencePrcValue);
            ItemMods.RemoveHandlers[64] = new ItemMods.ItemModHandler(ItemMods.RemoveAsda2DefencePrcValue);
            ItemMods.AddHandlers[51] = new ItemMods.ItemModHandler(ItemMods.AddAsda2MagicDefenceValue);
            ItemMods.RemoveHandlers[51] = new ItemMods.ItemModHandler(ItemMods.RemoveAsda2MagicDefenceValue);
            ItemMods.AddHandlers[65] = new ItemMods.ItemModHandler(ItemMods.AddAsda2MagicDefencePrcValue);
            ItemMods.RemoveHandlers[65] = new ItemMods.ItemModHandler(ItemMods.RemoveAsda2MagicDefencePrcValue);
            ItemMods.AddHandlers[52] = new ItemMods.ItemModHandler(ItemMods.AddAsda2DropChanceValue);
            ItemMods.RemoveHandlers[52] = new ItemMods.ItemModHandler(ItemMods.RemoveAsda2DropChanceValue);
            ItemMods.AddHandlers[53] = new ItemMods.ItemModHandler(ItemMods.AddAsda2GoldAmount);
            ItemMods.RemoveHandlers[53] = new ItemMods.ItemModHandler(ItemMods.RemoveAsda2GoldAmount);
            ItemMods.AddHandlers[55] = new ItemMods.ItemModHandler(ItemMods.AddAsda2ExpAmount);
            ItemMods.RemoveHandlers[55] = new ItemMods.ItemModHandler(ItemMods.RemoveAsda2ExpAmount);
            ItemMods.AddHandlers[56] = new ItemMods.ItemModHandler(ItemMods.AddDamagePrc);
            ItemMods.RemoveHandlers[56] = new ItemMods.ItemModHandler(ItemMods.RemoveDamagePrc);
            ItemMods.AddHandlers[57] = new ItemMods.ItemModHandler(ItemMods.AddMagicDamagePrc);
            ItemMods.RemoveHandlers[57] = new ItemMods.ItemModHandler(ItemMods.RemoveMagicDamagePrc);
            ItemMods.AddHandlers[82] = new ItemMods.ItemModHandler(ItemMods.AddDamage);
            ItemMods.RemoveHandlers[82] = new ItemMods.ItemModHandler(ItemMods.RemoveDamage);
            ItemMods.AddHandlers[83] = new ItemMods.ItemModHandler(ItemMods.AddMagicDamage);
            ItemMods.RemoveHandlers[83] = new ItemMods.ItemModHandler(ItemMods.RemoveMagicDamage);
            ItemMods.AddHandlers[58] = new ItemMods.ItemModHandler(ItemMods.AddStrengthPrc);
            ItemMods.RemoveHandlers[58] = new ItemMods.ItemModHandler(ItemMods.RemoveStrengthPrc);
            ItemMods.AddHandlers[59] = new ItemMods.ItemModHandler(ItemMods.AddAgilityPrc);
            ItemMods.RemoveHandlers[59] = new ItemMods.ItemModHandler(ItemMods.RemoveAgilityPrc);
            ItemMods.AddHandlers[60] = new ItemMods.ItemModHandler(ItemMods.AddIntelectPrc);
            ItemMods.RemoveHandlers[60] = new ItemMods.ItemModHandler(ItemMods.RemoveIntelectPrc);
            ItemMods.AddHandlers[63] = new ItemMods.ItemModHandler(ItemMods.AddStaminaPrc);
            ItemMods.RemoveHandlers[63] = new ItemMods.ItemModHandler(ItemMods.RemoveStaminaPrc);
            ItemMods.AddHandlers[61] = new ItemMods.ItemModHandler(ItemMods.AddLuckPrc);
            ItemMods.RemoveHandlers[61] = new ItemMods.ItemModHandler(ItemMods.RemoveLuckPrc);
            ItemMods.AddHandlers[62] = new ItemMods.ItemModHandler(ItemMods.AddEnergyPrc);
            ItemMods.RemoveHandlers[62] = new ItemMods.ItemModHandler(ItemMods.RemoveEnergyPrc);
            ItemMods.AddHandlers[66] = new ItemMods.ItemModHandler(ItemMods.AddAllMagicResistance);
            ItemMods.RemoveHandlers[66] = new ItemMods.ItemModHandler(ItemMods.RemoveAllMagicResistance);
            ItemMods.AddHandlers[67] = new ItemMods.ItemModHandler(ItemMods.AddDarkResistance);
            ItemMods.RemoveHandlers[67] = new ItemMods.ItemModHandler(ItemMods.RemoveDarkResistance);
            ItemMods.AddHandlers[68] = new ItemMods.ItemModHandler(ItemMods.AddLightResistance);
            ItemMods.RemoveHandlers[68] = new ItemMods.ItemModHandler(ItemMods.RemoveLightResistance);
            ItemMods.AddHandlers[69] = new ItemMods.ItemModHandler(ItemMods.AddWaterResistance);
            ItemMods.RemoveHandlers[69] = new ItemMods.ItemModHandler(ItemMods.RemoveWaterResistance);
            ItemMods.AddHandlers[70] = new ItemMods.ItemModHandler(ItemMods.AddClimateResistance);
            ItemMods.RemoveHandlers[70] = new ItemMods.ItemModHandler(ItemMods.RemoveClimateResistance);
            ItemMods.AddHandlers[71] = new ItemMods.ItemModHandler(ItemMods.AddEarthResistance);
            ItemMods.RemoveHandlers[71] = new ItemMods.ItemModHandler(ItemMods.RemoveEarthResistance);
            ItemMods.AddHandlers[72] = new ItemMods.ItemModHandler(ItemMods.AddFireResistance);
            ItemMods.RemoveHandlers[72] = new ItemMods.ItemModHandler(ItemMods.RemoveFireResistance);
            ItemMods.AddHandlers[73] = new ItemMods.ItemModHandler(ItemMods.AddDarkAttribute);
            ItemMods.RemoveHandlers[73] = new ItemMods.ItemModHandler(ItemMods.RemoveDarkAttribute);
            ItemMods.AddHandlers[74] = new ItemMods.ItemModHandler(ItemMods.AddLightAttribute);
            ItemMods.RemoveHandlers[74] = new ItemMods.ItemModHandler(ItemMods.RemoveLightAttribute);
            ItemMods.AddHandlers[75] = new ItemMods.ItemModHandler(ItemMods.AddWaterAttribute);
            ItemMods.RemoveHandlers[75] = new ItemMods.ItemModHandler(ItemMods.RemoveWaterAttribute);
            ItemMods.AddHandlers[76] = new ItemMods.ItemModHandler(ItemMods.AddClimateAttribute);
            ItemMods.RemoveHandlers[76] = new ItemMods.ItemModHandler(ItemMods.RemoveClimateAttribute);
            ItemMods.AddHandlers[77] = new ItemMods.ItemModHandler(ItemMods.AddEarthAttribute);
            ItemMods.RemoveHandlers[77] = new ItemMods.ItemModHandler(ItemMods.RemoveEarthAttribute);
            ItemMods.AddHandlers[78] = new ItemMods.ItemModHandler(ItemMods.AddFireAttribute);
            ItemMods.RemoveHandlers[78] = new ItemMods.ItemModHandler(ItemMods.RemoveFireAttribute);
            ItemMods.AddHandlers[79] = new ItemMods.ItemModHandler(ItemMods.AddSpeed);
            ItemMods.RemoveHandlers[79] = new ItemMods.ItemModHandler(ItemMods.RemoveSpeed);
            ItemMods.AddHandlers[81] = new ItemMods.ItemModHandler(ItemMods.AddCastingDistance);
            ItemMods.RemoveHandlers[81] = new ItemMods.ItemModHandler(ItemMods.RemoveCastingDistance);
            ItemMods.AddHandlers[86] = new ItemMods.ItemModHandler(ItemMods.AddFishingGauge);
            ItemMods.RemoveHandlers[86] = new ItemMods.ItemModHandler(ItemMods.RemoveFishingGauge);
            ItemMods.AddHandlers[85] = new ItemMods.ItemModHandler(ItemMods.AddFishingSkill);
            ItemMods.RemoveHandlers[85] = new ItemMods.ItemModHandler(ItemMods.RemoveFishingSkill);
        }

        public static void ApplyStatMods(this ItemTemplate template, Character owner)
        {
            for (int index = 0; index < template.Mods.Length; ++index)
            {
                StatModifier mod = template.Mods[index];
                if (mod.Value != 0)
                    owner.ApplyStatMod(mod.Type, mod.Value);
            }
        }

        public static void ApplyStatMod(this Character owner, ItemModType modType, int value)
        {
            ItemMods.ItemModHandler addHandler = ItemMods.AddHandlers[(int) modType];
            if (addHandler == null)
                return;
            addHandler(owner, value);
        }

        public static void RemoveStatMods(this ItemTemplate template, Character owner)
        {
            foreach (StatModifier mod in template.Mods)
            {
                if (mod.Value != 0)
                    owner.RemoveStatMod(mod.Type, mod.Value);
            }
        }

        public static void RemoveStatMod(this Character owner, ItemModType modType, int value)
        {
            ItemMods.ItemModHandler removeHandler = ItemMods.RemoveHandlers[(int) modType];
            if (removeHandler == null)
                return;
            removeHandler(owner, value);
        }

        private static void AddPower(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Power, value);
        }

        private static void AddHealth(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Health, value);
        }

        private static void AddUnused(Character owner, int value)
        {
        }

        private static void AddWeaponSkillRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.WeaponSkill, value);
        }

        private static void AddDefenseRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.DefenseSkill, value);
        }

        private static void AddDodgeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Dodge, value);
        }

        private static void AddParryRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Parry, value);
        }

        private static void AddBlockRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Block, value);
        }

        private static void AddMeleeHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHitChance, value);
        }

        private static void AddRangedHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedHitChance, value);
        }

        private static void AddSpellHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellHitChance, value);
        }

        private static void AddMeleeCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeCritChance, value);
        }

        private static void AddRangedCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedCritChance, value);
        }

        private static void AddSpellCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellCritChance, value);
        }

        private static void AddMeleeHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeAttackerHit, value);
        }

        private static void AddRangedHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedAttackerHit, value);
        }

        private static void AddSpellHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellAttackerHit, value);
        }

        private static void AddMeleeCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeResilience, value);
        }

        private static void AddRangedCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedResilience, value);
        }

        private static void AddSpellCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellResilience, value);
        }

        private static void AddMeleeHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHaste, value);
        }

        private static void AddRangedHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedHaste, value);
        }

        private static void AddSpellHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellHaste, value);
        }

        private static void AddHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHitChance, value);
            owner.ModCombatRating(CombatRating.RangedHitChance, value);
            owner.ModCombatRating(CombatRating.SpellHitChance, value);
        }

        private static void AddCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeCritChance, value);
            owner.ModCombatRating(CombatRating.RangedCritChance, value);
            owner.ModCombatRating(CombatRating.SpellCritChance, value);
        }

        private static void AddHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeAttackerHit, -value);
        }

        private static void AddCriticalAvoidanceRating(Character owner, int value)
        {
        }

        private static void AddResilienceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeResilience, value);
            owner.ModCombatRating(CombatRating.RangedResilience, value);
            owner.ModCombatRating(CombatRating.SpellResilience, value);
        }

        private static void AddHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHaste, value);
            owner.ModCombatRating(CombatRating.RangedHaste, value);
        }

        private static void AddExpertiseRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Expertise, value);
        }

        private static void RemovePower(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Power, -value);
        }

        private static void RemoveHealth(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Health, -value);
        }

        private static void RemoveUnused(Character owner, int value)
        {
        }

        private static void RemoveWeaponSkillRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.WeaponSkill, -value);
        }

        private static void RemoveDefenseRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.DefenseSkill, -value);
        }

        private static void RemoveDodgeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Dodge, -value);
        }

        private static void RemoveParryRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Parry, -value);
        }

        private static void RemoveBlockRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Block, -value);
        }

        private static void RemoveMeleeHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHitChance, -value);
        }

        private static void RemoveRangedHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedHitChance, -value);
        }

        private static void RemoveSpellHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellHitChance, -value);
        }

        private static void RemoveMeleeCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeCritChance, -value);
        }

        private static void RemoveRangedCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedCritChance, -value);
        }

        private static void RemoveSpellCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellCritChance, -value);
        }

        private static void RemoveMeleeHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeAttackerHit, -value);
        }

        private static void RemoveRangedHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedAttackerHit, -value);
        }

        private static void RemoveSpellHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellAttackerHit, -value);
        }

        private static void RemoveMeleeCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeResilience, -value);
        }

        private static void RemoveRangedCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedResilience, -value);
        }

        private static void RemoveSpellCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellResilience, -value);
        }

        private static void RemoveMeleeHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHaste, -value);
        }

        private static void RemoveRangedHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedHaste, -value);
        }

        private static void RemoveSpellHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellHaste, -value);
        }

        private static void RemoveHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHitChance, -value);
            owner.ModCombatRating(CombatRating.RangedHitChance, -value);
        }

        private static void RemoveCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeCritChance, -value);
            owner.ModCombatRating(CombatRating.RangedCritChance, -value);
            owner.ModCombatRating(CombatRating.SpellCritChance, -value);
        }

        private static void RemoveHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeAttackerHit, -value);
        }

        private static void RemoveCriticalAvoidanceRating(Character owner, int value)
        {
        }

        private static void RemoveResilienceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeResilience, -value);
            owner.ModCombatRating(CombatRating.RangedResilience, -value);
            owner.ModCombatRating(CombatRating.SpellResilience, -value);
        }

        private static void RemoveHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHaste, -value);
            owner.ModCombatRating(CombatRating.RangedHaste, -value);
        }

        private static void RemoveExpertiseRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Expertise, -value);
        }

        private static void AddSpellPower(Character owner, int value)
        {
            ItemMods.AddSpellDamageDone(owner, value);
            ItemMods.AddSpellHealingDone(owner, value);
        }

        private static void RemoveSpellPower(Character owner, int value)
        {
            ItemMods.RemoveSpellDamageDone(owner, value);
            ItemMods.RemoveSpellHealingDone(owner, value);
        }

        private static void AddSpellDamageDone(Character owner, int value)
        {
            owner.IntMods[34] += value;
        }

        private static void RemoveSpellDamageDone(Character owner, int value)
        {
            owner.IntMods[34] -= value;
        }

        private static void AddSpellHealingDone(Character owner, int value)
        {
            owner.HealingDoneMod += value;
        }

        private static void RemoveSpellHealingDone(Character owner, int value)
        {
            owner.HealingDoneMod -= value;
        }

        private static void AddBlockValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.BlockValue, (float) value);
        }

        private static void RemoveBlockValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.BlockValue, (float) -value);
        }

        private static void AddManaRegen(Character owner, int value)
        {
            if (owner.PowerType != PowerType.Mana)
                return;
            owner.ChangeModifier(StatModifierInt.PowerRegen, value);
        }

        private static void RemoveManaRegen(Character owner, int value)
        {
            if (owner.PowerType != PowerType.Mana)
                return;
            owner.ChangeModifier(StatModifierInt.PowerRegen, -value);
        }

        private static void AddHealthRegen(Character owner, int value)
        {
            if (owner.PowerType != PowerType.Mana)
                return;
            owner.ChangeModifier(StatModifierInt.HealthRegen, value);
        }

        private static void RemoveHealthRegen(Character owner, int value)
        {
            if (owner.PowerType != PowerType.Mana)
                return;
            owner.ChangeModifier(StatModifierInt.HealthRegen, -value);
        }

        private static void AddHealthRegenInCombat(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.HealthRegenInCombat, value);
        }

        private static void RemoveHealthRegenInCombat(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.HealthRegenInCombat, -value);
        }

        private static void AddLuckValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Luck, value);
        }

        private static void RemoveLuckValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Luck, -value);
        }

        private static void AddSellingCost(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.SellingCost, (float) value / 100f);
        }

        private static void RemoveSellingCost(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.SellingCost, (float) (-(double) value / 100.0));
        }

        private static void AddAtackTimePrcValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.MeleeAttackTime, (float) value);
        }

        private static void RemoveAtackTimePrcValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.MeleeAttackTime, (float) -value);
        }

        private static void AddAsda2DefenceValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Asda2Defence, value);
        }

        private static void RemoveAsda2DefenceValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Asda2Defence, -value);
        }

        private static void AddAsda2DefencePrcValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2Defence, (float) value / 100f);
        }

        private static void RemoveAsda2DefencePrcValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2Defence, (float) (-(double) value / 100.0));
        }

        private static void AddAsda2MagicDefenceValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Asda2MagicDefence, value);
        }

        private static void RemoveAsda2MagicDefenceValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Asda2MagicDefence, -value);
        }

        private static void AddAsda2MagicDefencePrcValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2MagicDefence, (float) value / 100f);
        }

        private static void RemoveAsda2MagicDefencePrcValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2MagicDefence, (float) (-(double) value / 100.0));
        }

        private static void AddAsda2DropChanceValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2DropChance, (float) value);
        }

        private static void RemoveAsda2DropChanceValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2DropChance, (float) -value);
        }

        private static void AddAsda2GoldAmount(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2GoldAmount, (float) value);
        }

        private static void RemoveAsda2GoldAmount(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2GoldAmount, (float) -value);
        }

        private static void AddAsda2ExpAmount(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float) value / 100f);
        }

        private static void RemoveAsda2ExpAmount(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float) (-(double) value / 100.0));
        }

        private static void AddDamagePrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Damage, (float) value / 100f);
        }

        private static void RemoveDamagePrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Damage, (float) (-(double) value / 100.0));
        }

        private static void AddMagicDamagePrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.MagicDamage, (float) value / 100f);
        }

        private static void RemoveMagicDamagePrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.MagicDamage, (float) (-(double) value / 100.0));
        }

        private static void AddDamage(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Damage, value);
        }

        private static void RemoveDamage(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Damage, -value);
        }

        private static void AddMagicDamage(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.MagicDamage, value);
        }

        private static void RemoveMagicDamage(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.MagicDamage, -value);
        }

        private static void AddStrengthPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Strength, (float) value / 100f);
        }

        private static void RemoveStrengthPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Strength, (float) (-(double) value / 100.0));
        }

        private static void AddAgilityPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Agility, (float) value / 100f);
        }

        private static void RemoveAgilityPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Agility, (float) (-(double) value / 100.0));
        }

        private static void AddStaminaPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Stamina, (float) value / 100f);
        }

        private static void RemoveStaminaPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Stamina, (float) (-(double) value / 100.0));
        }

        private static void AddIntelectPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Intelect, (float) value / 100f);
        }

        private static void RemoveIntelectPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Intelect, (float) (-(double) value / 100.0));
        }

        private static void AddEnergyPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Spirit, (float) value / 100f);
        }

        private static void RemoveEnergyPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Spirit, (float) (-(double) value / 100.0));
        }

        private static void AddAllMagicResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireResist, (float) value / 100f);
            owner.ChangeModifier(StatModifierFloat.EarthResit, (float) value / 100f);
            owner.ChangeModifier(StatModifierFloat.DarkResit, (float) value / 100f);
            owner.ChangeModifier(StatModifierFloat.LightResist, (float) value / 100f);
            owner.ChangeModifier(StatModifierFloat.ClimateResist, (float) value / 100f);
            owner.ChangeModifier(StatModifierFloat.WaterResist, (float) value / 100f);
        }

        private static void RemoveAllMagicResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireResist, (float) (-(double) value / 100.0));
            owner.ChangeModifier(StatModifierFloat.EarthResit, (float) (-(double) value / 100.0));
            owner.ChangeModifier(StatModifierFloat.DarkResit, (float) (-(double) value / 100.0));
            owner.ChangeModifier(StatModifierFloat.LightResist, (float) (-(double) value / 100.0));
            owner.ChangeModifier(StatModifierFloat.ClimateResist, (float) (-(double) value / 100.0));
            owner.ChangeModifier(StatModifierFloat.WaterResist, (float) (-(double) value / 100.0));
        }

        private static void AddLuckPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Luck, (float) value / 100f);
        }

        private static void RemoveLuckPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Luck, (float) (-(double) value / 100.0));
        }

        private static void AddAgility(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Agility, value);
        }

        private static void AddStrength(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Strength, value);
        }

        private static void AddIntellect(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Intellect, value);
        }

        private static void AddSpirit(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Spirit, value);
        }

        private static void AddStamina(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Stamina, value);
        }

        private static void RemoveAgility(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Agility, -value);
        }

        private static void RemoveStrength(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Strength, -value);
        }

        private static void RemoveIntellect(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Intellect, -value);
        }

        private static void RemoveSpirit(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Spirit, -value);
        }

        private static void RemoveStamina(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Stamina, -value);
        }

        private static void RemoveSpeed(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Speed, (float) (-(double) value / 100.0));
        }

        private static void AddSpeed(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Speed, (float) value / 100f);
        }

        private static void RemoveCastingDistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.CastingDistance, -value);
        }

        private static void AddCastingDistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.CastingDistance, value);
        }

        private static void RemoveFishingSkill(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Asda2FishingSkill, -value);
        }

        private static void AddFishingSkill(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Asda2FishingSkill, value);
        }

        private static void RemoveFishingGauge(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Asda2FishingGauge, -value);
        }

        private static void AddFishingGauge(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Asda2FishingGauge, value);
        }

        private static void RemoveFireResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireResist, (float) (-(double) value / 100.0));
        }

        private static void AddFireResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireResist, (float) value / 100f);
        }

        private static void RemoveEarthResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.EarthResit, (float) (-(double) value / 100.0));
        }

        private static void AddEarthResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.EarthResit, (float) value / 100f);
        }

        private static void RemoveClimateResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.ClimateResist, (float) (-(double) value / 100.0));
        }

        private static void AddClimateResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.ClimateResist, (float) value / 100f);
        }

        private static void RemoveWaterResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.WaterResist, (float) (-(double) value / 100.0));
        }

        private static void AddWaterResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.WaterResist, (float) value / 100f);
        }

        private static void RemoveLightResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.LightResist, (float) (-(double) value / 100.0));
        }

        private static void AddLightResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.LightResist, (float) value / 100f);
        }

        private static void RemoveDarkResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.DarkResit, (float) (-(double) value / 100.0));
        }

        private static void AddDarkResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.DarkResit, (float) value / 100f);
        }

        private static void RemoveFireAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireAttribute, (float) (-(double) value / 100.0));
        }

        private static void AddFireAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireAttribute, (float) value / 100f);
        }

        private static void RemoveEarthAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.EarthAttribute, (float) (-(double) value / 100.0));
        }

        private static void AddEarthAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.EarthAttribute, (float) value / 100f);
        }

        private static void RemoveClimateAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.ClimateAttribute, (float) (-(double) value / 100.0));
        }

        private static void AddClimateAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.ClimateAttribute, (float) value / 100f);
        }

        private static void RemoveWaterAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.WaterAttribute, (float) (-(double) value / 100.0));
        }

        private static void AddWaterAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.WaterAttribute, (float) value / 100f);
        }

        private static void RemoveLightAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.LightAttribute, (float) (-(double) value / 100.0));
        }

        private static void AddLightAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.LightAttribute, (float) value / 100f);
        }

        private static void RemoveDarkAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.DarkAttribute, (float) (-(double) value / 100.0));
        }

        private static void AddDarkAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.DarkAttribute, (float) value / 100f);
        }

        public delegate void ItemModHandler(Character owner, int value);
    }
}