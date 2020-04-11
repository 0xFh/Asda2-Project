using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Modifiers
{
  internal static class ItemMods
  {
    public static readonly ItemModHandler[] AddHandlers = new ItemModHandler[100];
    public static readonly ItemModHandler[] RemoveHandlers = new ItemModHandler[100];

    static ItemMods()
    {
      AddHandlers[2] = AddUnused;
      AddHandlers[0] = AddPower;
      AddHandlers[1] = AddHealth;
      AddHandlers[3] = AddAgility;
      AddHandlers[4] = AddStrength;
      AddHandlers[5] = AddIntellect;
      AddHandlers[6] = AddSpirit;
      AddHandlers[7] = AddStamina;
      AddHandlers[11] = AddWeaponSkillRating;
      AddHandlers[12] = AddDefenseRating;
      AddHandlers[13] = AddDodgeRating;
      AddHandlers[14] = AddParryRating;
      AddHandlers[15] = AddBlockRating;
      AddHandlers[16] = AddMeleeHitRating;
      AddHandlers[17] = AddRangedHitRating;
      AddHandlers[18] = AddSpellHitRating;
      AddHandlers[19] = AddMeleeCriticalStrikeRating;
      AddHandlers[20] = AddRangedCriticalStrikeRating;
      AddHandlers[21] = AddSpellCriticalStrikeRating;
      AddHandlers[22] = AddMeleeHitAvoidanceRating;
      AddHandlers[23] = AddRangedHitAvoidanceRating;
      AddHandlers[24] = AddSpellHitAvoidanceRating;
      AddHandlers[25] = AddMeleeCriticalAvoidanceRating;
      AddHandlers[26] = AddRangedCriticalAvoidanceRating;
      AddHandlers[27] = AddSpellCriticalAvoidanceRating;
      AddHandlers[28] = AddMeleeHasteRating;
      AddHandlers[29] = AddRangedHasteRating;
      AddHandlers[30] = AddSpellHasteRating;
      AddHandlers[31] = AddHitRating;
      AddHandlers[32] = AddCriticalStrikeRating;
      AddHandlers[33] = AddHitAvoidanceRating;
      AddHandlers[34] = AddCriticalAvoidanceRating;
      AddHandlers[35] = AddResilienceRating;
      AddHandlers[36] = AddHasteRating;
      AddHandlers[37] = AddExpertiseRating;
      RemoveHandlers[2] = RemoveUnused;
      RemoveHandlers[0] = RemovePower;
      RemoveHandlers[1] = RemoveHealth;
      RemoveHandlers[3] = RemoveAgility;
      RemoveHandlers[4] = RemoveStrength;
      RemoveHandlers[5] = RemoveIntellect;
      RemoveHandlers[6] = RemoveSpirit;
      RemoveHandlers[7] = RemoveStamina;
      RemoveHandlers[11] = RemoveWeaponSkillRating;
      RemoveHandlers[12] = RemoveDefenseRating;
      RemoveHandlers[13] = RemoveDodgeRating;
      RemoveHandlers[14] = RemoveParryRating;
      RemoveHandlers[15] = RemoveBlockRating;
      RemoveHandlers[16] = RemoveMeleeHitRating;
      RemoveHandlers[17] = RemoveRangedHitRating;
      RemoveHandlers[18] = RemoveSpellHitRating;
      RemoveHandlers[19] = RemoveMeleeCriticalStrikeRating;
      RemoveHandlers[20] = RemoveRangedCriticalStrikeRating;
      RemoveHandlers[21] = RemoveSpellCriticalStrikeRating;
      RemoveHandlers[22] = RemoveMeleeHitAvoidanceRating;
      RemoveHandlers[23] = RemoveRangedHitAvoidanceRating;
      RemoveHandlers[24] = RemoveSpellHitAvoidanceRating;
      RemoveHandlers[25] = RemoveMeleeCriticalAvoidanceRating;
      RemoveHandlers[26] = RemoveRangedCriticalAvoidanceRating;
      RemoveHandlers[27] = RemoveSpellCriticalAvoidanceRating;
      RemoveHandlers[28] = RemoveMeleeHasteRating;
      RemoveHandlers[29] = RemoveRangedHasteRating;
      RemoveHandlers[30] = RemoveSpellHasteRating;
      RemoveHandlers[31] = RemoveHitRating;
      RemoveHandlers[32] = RemoveCriticalStrikeRating;
      RemoveHandlers[33] = RemoveHitAvoidanceRating;
      RemoveHandlers[34] = RemoveCriticalAvoidanceRating;
      RemoveHandlers[35] = RemoveResilienceRating;
      RemoveHandlers[36] = RemoveHasteRating;
      RemoveHandlers[37] = RemoveExpertiseRating;
      AddHandlers[42] = AddSpellDamageDone;
      RemoveHandlers[42] = RemoveSpellDamageDone;
      AddHandlers[41] = AddSpellHealingDone;
      RemoveHandlers[41] = RemoveSpellHealingDone;
      AddHandlers[45] = AddSpellPower;
      RemoveHandlers[45] = RemoveSpellPower;
      AddHandlers[48] = AddBlockValue;
      RemoveHandlers[48] = RemoveBlockValue;
      AddHandlers[43] = AddManaRegen;
      RemoveHandlers[43] = RemoveManaRegen;
      AddHandlers[46] = AddHealthRegen;
      RemoveHandlers[46] = RemoveHealthRegen;
      AddHandlers[80] = AddHealthRegenInCombat;
      RemoveHandlers[80] = RemoveHealthRegenInCombat;
      AddHandlers[54] = AddLuckValue;
      RemoveHandlers[54] = RemoveLuckValue;
      AddHandlers[84] = AddSellingCost;
      RemoveHandlers[84] = RemoveSellingCost;
      AddHandlers[49] = AddAtackTimePrcValue;
      RemoveHandlers[49] = RemoveAtackTimePrcValue;
      AddHandlers[50] = AddAsda2DefenceValue;
      RemoveHandlers[50] = RemoveAsda2DefenceValue;
      AddHandlers[64] = AddAsda2DefencePrcValue;
      RemoveHandlers[64] = RemoveAsda2DefencePrcValue;
      AddHandlers[51] = AddAsda2MagicDefenceValue;
      RemoveHandlers[51] = RemoveAsda2MagicDefenceValue;
      AddHandlers[65] = AddAsda2MagicDefencePrcValue;
      RemoveHandlers[65] = RemoveAsda2MagicDefencePrcValue;
      AddHandlers[52] = AddAsda2DropChanceValue;
      RemoveHandlers[52] = RemoveAsda2DropChanceValue;
      AddHandlers[53] = AddAsda2GoldAmount;
      RemoveHandlers[53] = RemoveAsda2GoldAmount;
      AddHandlers[55] = AddAsda2ExpAmount;
      RemoveHandlers[55] = RemoveAsda2ExpAmount;
      AddHandlers[56] = AddDamagePrc;
      RemoveHandlers[56] = RemoveDamagePrc;
      AddHandlers[57] = AddMagicDamagePrc;
      RemoveHandlers[57] = RemoveMagicDamagePrc;
      AddHandlers[82] = AddDamage;
      RemoveHandlers[82] = RemoveDamage;
      AddHandlers[83] = AddMagicDamage;
      RemoveHandlers[83] = RemoveMagicDamage;
      AddHandlers[58] = AddStrengthPrc;
      RemoveHandlers[58] = RemoveStrengthPrc;
      AddHandlers[59] = AddAgilityPrc;
      RemoveHandlers[59] = RemoveAgilityPrc;
      AddHandlers[60] = AddIntelectPrc;
      RemoveHandlers[60] = RemoveIntelectPrc;
      AddHandlers[63] = AddStaminaPrc;
      RemoveHandlers[63] = RemoveStaminaPrc;
      AddHandlers[61] = AddLuckPrc;
      RemoveHandlers[61] = RemoveLuckPrc;
      AddHandlers[62] = AddEnergyPrc;
      RemoveHandlers[62] = RemoveEnergyPrc;
      AddHandlers[66] = AddAllMagicResistance;
      RemoveHandlers[66] = RemoveAllMagicResistance;
      AddHandlers[67] = AddDarkResistance;
      RemoveHandlers[67] = RemoveDarkResistance;
      AddHandlers[68] = AddLightResistance;
      RemoveHandlers[68] = RemoveLightResistance;
      AddHandlers[69] = AddWaterResistance;
      RemoveHandlers[69] = RemoveWaterResistance;
      AddHandlers[70] = AddClimateResistance;
      RemoveHandlers[70] = RemoveClimateResistance;
      AddHandlers[71] = AddEarthResistance;
      RemoveHandlers[71] = RemoveEarthResistance;
      AddHandlers[72] = AddFireResistance;
      RemoveHandlers[72] = RemoveFireResistance;
      AddHandlers[73] = AddDarkAttribute;
      RemoveHandlers[73] = RemoveDarkAttribute;
      AddHandlers[74] = AddLightAttribute;
      RemoveHandlers[74] = RemoveLightAttribute;
      AddHandlers[75] = AddWaterAttribute;
      RemoveHandlers[75] = RemoveWaterAttribute;
      AddHandlers[76] = AddClimateAttribute;
      RemoveHandlers[76] = RemoveClimateAttribute;
      AddHandlers[77] = AddEarthAttribute;
      RemoveHandlers[77] = RemoveEarthAttribute;
      AddHandlers[78] = AddFireAttribute;
      RemoveHandlers[78] = RemoveFireAttribute;
      AddHandlers[79] = AddSpeed;
      RemoveHandlers[79] = RemoveSpeed;
      AddHandlers[81] = AddCastingDistance;
      RemoveHandlers[81] = RemoveCastingDistance;
      AddHandlers[86] = AddFishingGauge;
      RemoveHandlers[86] = RemoveFishingGauge;
      AddHandlers[85] = AddFishingSkill;
      RemoveHandlers[85] = RemoveFishingSkill;
    }

    public static void ApplyStatMods(this ItemTemplate template, Character owner)
    {
      for(int index = 0; index < template.Mods.Length; ++index)
      {
        StatModifier mod = template.Mods[index];
        if(mod.Value != 0)
          owner.ApplyStatMod(mod.Type, mod.Value);
      }
    }

    public static void ApplyStatMod(this Character owner, ItemModType modType, int value)
    {
      ItemModHandler addHandler = AddHandlers[(int) modType];
      if(addHandler == null)
        return;
      addHandler(owner, value);
    }

    public static void RemoveStatMods(this ItemTemplate template, Character owner)
    {
      foreach(StatModifier mod in template.Mods)
      {
        if(mod.Value != 0)
          owner.RemoveStatMod(mod.Type, mod.Value);
      }
    }

    public static void RemoveStatMod(this Character owner, ItemModType modType, int value)
    {
      ItemModHandler removeHandler = RemoveHandlers[(int) modType];
      if(removeHandler == null)
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
      AddSpellDamageDone(owner, value);
      AddSpellHealingDone(owner, value);
    }

    private static void RemoveSpellPower(Character owner, int value)
    {
      RemoveSpellDamageDone(owner, value);
      RemoveSpellHealingDone(owner, value);
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
      owner.ChangeModifier(StatModifierFloat.BlockValue, value);
    }

    private static void RemoveBlockValue(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.BlockValue, -value);
    }

    private static void AddManaRegen(Character owner, int value)
    {
      if(owner.PowerType != PowerType.Mana)
        return;
      owner.ChangeModifier(StatModifierInt.PowerRegen, value);
    }

    private static void RemoveManaRegen(Character owner, int value)
    {
      if(owner.PowerType != PowerType.Mana)
        return;
      owner.ChangeModifier(StatModifierInt.PowerRegen, -value);
    }

    private static void AddHealthRegen(Character owner, int value)
    {
      if(owner.PowerType != PowerType.Mana)
        return;
      owner.ChangeModifier(StatModifierInt.HealthRegen, value);
    }

    private static void RemoveHealthRegen(Character owner, int value)
    {
      if(owner.PowerType != PowerType.Mana)
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
      owner.ChangeModifier(StatModifierFloat.SellingCost, value / 100f);
    }

    private static void RemoveSellingCost(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.SellingCost, (float) (-(double) value / 100.0));
    }

    private static void AddAtackTimePrcValue(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.MeleeAttackTime, value);
    }

    private static void RemoveAtackTimePrcValue(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.MeleeAttackTime, -value);
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
      owner.ChangeModifier(StatModifierFloat.Asda2Defence, value / 100f);
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
      owner.ChangeModifier(StatModifierFloat.Asda2MagicDefence, value / 100f);
    }

    private static void RemoveAsda2MagicDefencePrcValue(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Asda2MagicDefence, (float) (-(double) value / 100.0));
    }

    private static void AddAsda2DropChanceValue(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Asda2DropChance, value);
    }

    private static void RemoveAsda2DropChanceValue(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Asda2DropChance, -value);
    }

    private static void AddAsda2GoldAmount(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Asda2GoldAmount, value);
    }

    private static void RemoveAsda2GoldAmount(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Asda2GoldAmount, -value);
    }

    private static void AddAsda2ExpAmount(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Asda2ExpAmount, value / 100f);
    }

    private static void RemoveAsda2ExpAmount(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float) (-(double) value / 100.0));
    }

    private static void AddDamagePrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Damage, value / 100f);
    }

    private static void RemoveDamagePrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Damage, (float) (-(double) value / 100.0));
    }

    private static void AddMagicDamagePrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.MagicDamage, value / 100f);
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
      owner.ChangeModifier(StatModifierFloat.Strength, value / 100f);
    }

    private static void RemoveStrengthPrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Strength, (float) (-(double) value / 100.0));
    }

    private static void AddAgilityPrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Agility, value / 100f);
    }

    private static void RemoveAgilityPrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Agility, (float) (-(double) value / 100.0));
    }

    private static void AddStaminaPrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Stamina, value / 100f);
    }

    private static void RemoveStaminaPrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Stamina, (float) (-(double) value / 100.0));
    }

    private static void AddIntelectPrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Intelect, value / 100f);
    }

    private static void RemoveIntelectPrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Intelect, (float) (-(double) value / 100.0));
    }

    private static void AddEnergyPrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Spirit, value / 100f);
    }

    private static void RemoveEnergyPrc(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.Spirit, (float) (-(double) value / 100.0));
    }

    private static void AddAllMagicResistance(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.FireResist, value / 100f);
      owner.ChangeModifier(StatModifierFloat.EarthResit, value / 100f);
      owner.ChangeModifier(StatModifierFloat.DarkResit, value / 100f);
      owner.ChangeModifier(StatModifierFloat.LightResist, value / 100f);
      owner.ChangeModifier(StatModifierFloat.ClimateResist, value / 100f);
      owner.ChangeModifier(StatModifierFloat.WaterResist, value / 100f);
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
      owner.ChangeModifier(StatModifierFloat.Luck, value / 100f);
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
      owner.ChangeModifier(StatModifierFloat.Speed, value / 100f);
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
      owner.ChangeModifier(StatModifierFloat.FireResist, value / 100f);
    }

    private static void RemoveEarthResistance(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.EarthResit, (float) (-(double) value / 100.0));
    }

    private static void AddEarthResistance(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.EarthResit, value / 100f);
    }

    private static void RemoveClimateResistance(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.ClimateResist, (float) (-(double) value / 100.0));
    }

    private static void AddClimateResistance(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.ClimateResist, value / 100f);
    }

    private static void RemoveWaterResistance(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.WaterResist, (float) (-(double) value / 100.0));
    }

    private static void AddWaterResistance(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.WaterResist, value / 100f);
    }

    private static void RemoveLightResistance(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.LightResist, (float) (-(double) value / 100.0));
    }

    private static void AddLightResistance(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.LightResist, value / 100f);
    }

    private static void RemoveDarkResistance(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.DarkResit, (float) (-(double) value / 100.0));
    }

    private static void AddDarkResistance(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.DarkResit, value / 100f);
    }

    private static void RemoveFireAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.FireAttribute, (float) (-(double) value / 100.0));
    }

    private static void AddFireAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.FireAttribute, value / 100f);
    }

    private static void RemoveEarthAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.EarthAttribute, (float) (-(double) value / 100.0));
    }

    private static void AddEarthAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.EarthAttribute, value / 100f);
    }

    private static void RemoveClimateAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.ClimateAttribute, (float) (-(double) value / 100.0));
    }

    private static void AddClimateAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.ClimateAttribute, value / 100f);
    }

    private static void RemoveWaterAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.WaterAttribute, (float) (-(double) value / 100.0));
    }

    private static void AddWaterAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.WaterAttribute, value / 100f);
    }

    private static void RemoveLightAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.LightAttribute, (float) (-(double) value / 100.0));
    }

    private static void AddLightAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.LightAttribute, value / 100f);
    }

    private static void RemoveDarkAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.DarkAttribute, (float) (-(double) value / 100.0));
    }

    private static void AddDarkAttribute(Character owner, int value)
    {
      owner.ChangeModifier(StatModifierFloat.DarkAttribute, value / 100f);
    }

    public delegate void ItemModHandler(Character owner, int value);
  }
}