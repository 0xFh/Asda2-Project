using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Modifiers
{
	internal static class ItemMods
	{
		public delegate void ItemModHandler(Character owner, int value);

		public static readonly ItemModHandler[] AddHandlers = new ItemModHandler[(int)ItemModType.End];
		public static readonly ItemModHandler[] RemoveHandlers = new ItemModHandler[(int)ItemModType.End];

		static ItemMods()
		{
			AddHandlers[(int)ItemModType.Unused] = AddUnused;
			AddHandlers[(int)ItemModType.Power] = AddPower;
			AddHandlers[(int)ItemModType.Health] = AddHealth;
			AddHandlers[(int)ItemModType.Agility] = AddAgility;
			AddHandlers[(int)ItemModType.Strength] = AddStrength;
			AddHandlers[(int)ItemModType.Intellect] = AddIntellect;
			AddHandlers[(int)ItemModType.Spirit] = AddSpirit;
			AddHandlers[(int)ItemModType.Stamina] = AddStamina;
			AddHandlers[(int)ItemModType.WeaponSkillRating] = AddWeaponSkillRating;
			AddHandlers[(int)ItemModType.DefenseRating] = AddDefenseRating;
			AddHandlers[(int)ItemModType.DodgeRating] = AddDodgeRating;
			AddHandlers[(int)ItemModType.ParryRating] = AddParryRating;
			AddHandlers[(int)ItemModType.BlockRating] = AddBlockRating;
			AddHandlers[(int)ItemModType.MeleeHitRating] = AddMeleeHitRating;
			AddHandlers[(int)ItemModType.RangedHitRating] = AddRangedHitRating;
			AddHandlers[(int)ItemModType.SpellHitRating] = AddSpellHitRating;
			AddHandlers[(int)ItemModType.MeleeCriticalStrikeRating] = AddMeleeCriticalStrikeRating;
			AddHandlers[(int)ItemModType.RangedCriticalStrikeRating] = AddRangedCriticalStrikeRating;
			AddHandlers[(int)ItemModType.SpellCriticalStrikeRating] = AddSpellCriticalStrikeRating;
			AddHandlers[(int)ItemModType.MeleeHitAvoidanceRating] = AddMeleeHitAvoidanceRating;
			AddHandlers[(int)ItemModType.RangedHitAvoidanceRating] = AddRangedHitAvoidanceRating;
			AddHandlers[(int)ItemModType.SpellHitAvoidanceRating] = AddSpellHitAvoidanceRating;
			AddHandlers[(int)ItemModType.MeleeCriticalAvoidanceRating] = AddMeleeCriticalAvoidanceRating;
			AddHandlers[(int)ItemModType.RangedCriticalAvoidanceRating] = AddRangedCriticalAvoidanceRating;
			AddHandlers[(int)ItemModType.SpellCriticalAvoidanceRating] = AddSpellCriticalAvoidanceRating;
			AddHandlers[(int)ItemModType.MeleeHasteRating] = AddMeleeHasteRating;
			AddHandlers[(int)ItemModType.RangedHasteRating] = AddRangedHasteRating;
			AddHandlers[(int)ItemModType.SpellHasteRating] = AddSpellHasteRating;
			AddHandlers[(int)ItemModType.HitRating] = AddHitRating;
			AddHandlers[(int)ItemModType.CriticalStrikeRating] = AddCriticalStrikeRating;
			AddHandlers[(int)ItemModType.HitAvoidanceRating] = AddHitAvoidanceRating;
			AddHandlers[(int)ItemModType.CriticalAvoidanceRating] = AddCriticalAvoidanceRating;
			AddHandlers[(int)ItemModType.ResilienceRating] = AddResilienceRating;
			AddHandlers[(int)ItemModType.HasteRating] = AddHasteRating;
			AddHandlers[(int)ItemModType.ExpertiseRating] = AddExpertiseRating;

			RemoveHandlers[(int)ItemModType.Unused] = RemoveUnused;
			RemoveHandlers[(int)ItemModType.Power] = RemovePower;
			RemoveHandlers[(int)ItemModType.Health] = RemoveHealth;
			RemoveHandlers[(int)ItemModType.Agility] = RemoveAgility;
			RemoveHandlers[(int)ItemModType.Strength] = RemoveStrength;
			RemoveHandlers[(int)ItemModType.Intellect] = RemoveIntellect;
			RemoveHandlers[(int)ItemModType.Spirit] = RemoveSpirit;
			RemoveHandlers[(int)ItemModType.Stamina] = RemoveStamina;
			RemoveHandlers[(int)ItemModType.WeaponSkillRating] = RemoveWeaponSkillRating;
			RemoveHandlers[(int)ItemModType.DefenseRating] = RemoveDefenseRating;
			RemoveHandlers[(int)ItemModType.DodgeRating] = RemoveDodgeRating;
			RemoveHandlers[(int)ItemModType.ParryRating] = RemoveParryRating;
			RemoveHandlers[(int)ItemModType.BlockRating] = RemoveBlockRating;
			RemoveHandlers[(int)ItemModType.MeleeHitRating] = RemoveMeleeHitRating;
			RemoveHandlers[(int)ItemModType.RangedHitRating] = RemoveRangedHitRating;
			RemoveHandlers[(int)ItemModType.SpellHitRating] = RemoveSpellHitRating;
			RemoveHandlers[(int)ItemModType.MeleeCriticalStrikeRating] = RemoveMeleeCriticalStrikeRating;
			RemoveHandlers[(int)ItemModType.RangedCriticalStrikeRating] = RemoveRangedCriticalStrikeRating;
			RemoveHandlers[(int)ItemModType.SpellCriticalStrikeRating] = RemoveSpellCriticalStrikeRating;
			RemoveHandlers[(int)ItemModType.MeleeHitAvoidanceRating] = RemoveMeleeHitAvoidanceRating;
			RemoveHandlers[(int)ItemModType.RangedHitAvoidanceRating] = RemoveRangedHitAvoidanceRating;
			RemoveHandlers[(int)ItemModType.SpellHitAvoidanceRating] = RemoveSpellHitAvoidanceRating;
			RemoveHandlers[(int)ItemModType.MeleeCriticalAvoidanceRating] = RemoveMeleeCriticalAvoidanceRating;
			RemoveHandlers[(int)ItemModType.RangedCriticalAvoidanceRating] = RemoveRangedCriticalAvoidanceRating;
			RemoveHandlers[(int)ItemModType.SpellCriticalAvoidanceRating] = RemoveSpellCriticalAvoidanceRating;
			RemoveHandlers[(int)ItemModType.MeleeHasteRating] = RemoveMeleeHasteRating;
			RemoveHandlers[(int)ItemModType.RangedHasteRating] = RemoveRangedHasteRating;
			RemoveHandlers[(int)ItemModType.SpellHasteRating] = RemoveSpellHasteRating;
			RemoveHandlers[(int)ItemModType.HitRating] = RemoveHitRating;
			RemoveHandlers[(int)ItemModType.CriticalStrikeRating] = RemoveCriticalStrikeRating;
			RemoveHandlers[(int)ItemModType.HitAvoidanceRating] = RemoveHitAvoidanceRating;
			RemoveHandlers[(int)ItemModType.CriticalAvoidanceRating] = RemoveCriticalAvoidanceRating;
			RemoveHandlers[(int)ItemModType.ResilienceRating] = RemoveResilienceRating;
			RemoveHandlers[(int)ItemModType.HasteRating] = RemoveHasteRating;
			RemoveHandlers[(int)ItemModType.ExpertiseRating] = RemoveExpertiseRating;


			// new modifiers
			AddHandlers[(int)ItemModType.SpellDamageDone] = AddSpellDamageDone;
			RemoveHandlers[(int)ItemModType.SpellDamageDone] = RemoveSpellDamageDone;
			AddHandlers[(int)ItemModType.SpellHealingDone] = AddSpellHealingDone;
			RemoveHandlers[(int)ItemModType.SpellHealingDone] = RemoveSpellHealingDone;
			AddHandlers[(int)ItemModType.SpellPower] = AddSpellPower;
			RemoveHandlers[(int)ItemModType.SpellPower] = RemoveSpellPower;

			AddHandlers[(int)ItemModType.BlockValue] = AddBlockValue;
			RemoveHandlers[(int)ItemModType.BlockValue] = RemoveBlockValue;


			AddHandlers[(int)ItemModType.ManaRegeneration] = AddManaRegen;	// TODO: Depends on PowerType
			RemoveHandlers[(int)ItemModType.ManaRegeneration] = RemoveManaRegen;
			AddHandlers[(int)ItemModType.HealthRegenration] = AddHealthRegen;
			RemoveHandlers[(int)ItemModType.HealthRegenration] = RemoveHealthRegen;
            AddHandlers[(int)ItemModType.HealthRegenrationInCombat] = AddHealthRegenInCombat;
            RemoveHandlers[(int)ItemModType.HealthRegenrationInCombat] = RemoveHealthRegenInCombat;
            #region form asda2
            AddHandlers[(int)ItemModType.Luck] = AddLuckValue;
            RemoveHandlers[(int)ItemModType.Luck] = RemoveLuckValue;

            AddHandlers[(int)ItemModType.SellingCost] = AddSellingCost;
            RemoveHandlers[(int)ItemModType.SellingCost] = RemoveSellingCost;
            AddHandlers[(int)ItemModType.AtackTimePrc] = AddAtackTimePrcValue;
            RemoveHandlers[(int)ItemModType.AtackTimePrc] = RemoveAtackTimePrcValue;
            AddHandlers[(int)ItemModType.Asda2Defence] = AddAsda2DefenceValue;
            RemoveHandlers[(int)ItemModType.Asda2Defence] = RemoveAsda2DefenceValue;
            AddHandlers[(int)ItemModType.Asda2DefencePrc] = AddAsda2DefencePrcValue;
            RemoveHandlers[(int)ItemModType.Asda2DefencePrc] = RemoveAsda2DefencePrcValue;
            AddHandlers[(int)ItemModType.Asda2MagicDefence] = AddAsda2MagicDefenceValue;
            RemoveHandlers[(int)ItemModType.Asda2MagicDefence] = RemoveAsda2MagicDefenceValue;
            AddHandlers[(int)ItemModType.Asda2MagicDefencePrc] = AddAsda2MagicDefencePrcValue;
            RemoveHandlers[(int)ItemModType.Asda2MagicDefencePrc] = RemoveAsda2MagicDefencePrcValue;
            AddHandlers[(int)ItemModType.DropChance] = AddAsda2DropChanceValue;
            RemoveHandlers[(int)ItemModType.DropChance] = RemoveAsda2DropChanceValue;
            AddHandlers[(int)ItemModType.DropGoldByPrc] = AddAsda2GoldAmount;
            RemoveHandlers[(int)ItemModType.DropGoldByPrc] = RemoveAsda2GoldAmount;
            AddHandlers[(int)ItemModType.Asda2Expirience] = AddAsda2ExpAmount;
            RemoveHandlers[(int)ItemModType.Asda2Expirience] = RemoveAsda2ExpAmount;
            AddHandlers[(int)ItemModType.DamagePrc] = AddDamagePrc;
            RemoveHandlers[(int)ItemModType.DamagePrc] = RemoveDamagePrc;
            AddHandlers[(int)ItemModType.MagicDamagePrc] = AddMagicDamagePrc;
            RemoveHandlers[(int)ItemModType.MagicDamagePrc] = RemoveMagicDamagePrc;
            AddHandlers[(int)ItemModType.Damage] = AddDamage;
            RemoveHandlers[(int)ItemModType.Damage] = RemoveDamage;
            AddHandlers[(int)ItemModType.MagicDamage] = AddMagicDamage;
            RemoveHandlers[(int)ItemModType.MagicDamage] = RemoveMagicDamage;
            AddHandlers[(int)ItemModType.StrengthPrc] = AddStrengthPrc;
            RemoveHandlers[(int)ItemModType.StrengthPrc] = RemoveStrengthPrc;
            AddHandlers[(int)ItemModType.AgilityPrc] = AddAgilityPrc;
            RemoveHandlers[(int)ItemModType.AgilityPrc] = RemoveAgilityPrc;
            AddHandlers[(int)ItemModType.IntelectPrc] = AddIntelectPrc;
            RemoveHandlers[(int)ItemModType.IntelectPrc] = RemoveIntelectPrc;
            AddHandlers[(int)ItemModType.StaminaPrc] = AddStaminaPrc;
            RemoveHandlers[(int)ItemModType.StaminaPrc] = RemoveStaminaPrc;
            AddHandlers[(int)ItemModType.LuckPrc] = AddLuckPrc;
            RemoveHandlers[(int)ItemModType.LuckPrc] = RemoveLuckPrc;
            AddHandlers[(int)ItemModType.EnergyPrc] = AddEnergyPrc;
            RemoveHandlers[(int)ItemModType.EnergyPrc] = RemoveEnergyPrc;
            AddHandlers[(int)ItemModType.AllMagicResistance] = AddAllMagicResistance;
            RemoveHandlers[(int)ItemModType.AllMagicResistance] = RemoveAllMagicResistance;
            AddHandlers[(int)ItemModType.DarkResistance] = AddDarkResistance;
            RemoveHandlers[(int)ItemModType.DarkResistance] = RemoveDarkResistance;
            AddHandlers[(int)ItemModType.LightResistance] = AddLightResistance;
            RemoveHandlers[(int)ItemModType.LightResistance] = RemoveLightResistance;
            AddHandlers[(int)ItemModType.WaterResistance] = AddWaterResistance;
            RemoveHandlers[(int)ItemModType.WaterResistance] = RemoveWaterResistance;
            AddHandlers[(int)ItemModType.ClimateResistance] = AddClimateResistance;
            RemoveHandlers[(int)ItemModType.ClimateResistance] = RemoveClimateResistance;
            AddHandlers[(int)ItemModType.EarthResistance] = AddEarthResistance;
            RemoveHandlers[(int)ItemModType.EarthResistance] = RemoveEarthResistance;
            AddHandlers[(int)ItemModType.FireResistance] = AddFireResistance;
            RemoveHandlers[(int)ItemModType.FireResistance] = RemoveFireResistance;

            AddHandlers[(int)ItemModType.DarkAttribute] = AddDarkAttribute;
            RemoveHandlers[(int)ItemModType.DarkAttribute] = RemoveDarkAttribute;
            AddHandlers[(int)ItemModType.LightAttribute] = AddLightAttribute;
            RemoveHandlers[(int)ItemModType.LightAttribute] = RemoveLightAttribute;
            AddHandlers[(int)ItemModType.WaterAttribute] = AddWaterAttribute;
            RemoveHandlers[(int)ItemModType.WaterAttribute] = RemoveWaterAttribute;
            AddHandlers[(int)ItemModType.ClimateAttribute] = AddClimateAttribute;
            RemoveHandlers[(int)ItemModType.ClimateAttribute] = RemoveClimateAttribute;
            AddHandlers[(int)ItemModType.EarthAttribute] = AddEarthAttribute;
            RemoveHandlers[(int)ItemModType.EarthAttribute] = RemoveEarthAttribute;
            AddHandlers[(int)ItemModType.FireAttribute] = AddFireAttribute;
            RemoveHandlers[(int)ItemModType.FireAttribute] = RemoveFireAttribute;
            AddHandlers[(int)ItemModType.Speed] = AddSpeed;
            RemoveHandlers[(int)ItemModType.Speed] = RemoveSpeed;
            AddHandlers[(int)ItemModType.CastingDistance] = AddCastingDistance;
            RemoveHandlers[(int)ItemModType.CastingDistance] = RemoveCastingDistance;
            AddHandlers[(int)ItemModType.FishingGauge] = AddFishingGauge;
            RemoveHandlers[(int)ItemModType.FishingGauge] = RemoveFishingGauge;
            AddHandlers[(int)ItemModType.FishingSkill] = AddFishingSkill;
            RemoveHandlers[(int)ItemModType.FishingSkill] = RemoveFishingSkill;
            #endregion
        }



	    public static void ApplyStatMods(this ItemTemplate template, Character owner)
		{
			for (var i = 0; i < template.Mods.Length; i++)
			{
				var mod = template.Mods[i];
				if (mod.Value != 0)
				{
					ApplyStatMod(owner, mod.Type, mod.Value);
				}
			}
		}

		public static void ApplyStatMod(this Character owner, ItemModType modType, int value)
		{
			var handler = AddHandlers[(int)modType];
			if (handler != null)
			{
				handler(owner, value);
			}
		}

		public static void RemoveStatMods(this ItemTemplate template, Character owner)
		{
			foreach (var mod in template.Mods)
			{
				if (mod.Value != 0)
				{
					RemoveStatMod(owner, mod.Type, mod.Value);
				}
			}
		}

		public static void RemoveStatMod(this Character owner, ItemModType modType, int value)
		{
			var handler = RemoveHandlers[(int)modType];
			if (handler != null)
			{
				handler(owner, value);
			}
		}

		#region Add
		static void AddPower(Character owner, int value)
		{
			owner.ChangeModifier(StatModifierInt.Power, value);
		}
		static void AddHealth(Character owner, int value)
		{
            owner.ChangeModifier(StatModifierInt.Health, value);
		}

		static void AddUnused(Character owner, int value)
		{
		}
		

		static void AddWeaponSkillRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.WeaponSkill, value);
		}
		static void AddDefenseRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.DefenseSkill, value);
		}
		static void AddDodgeRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.Dodge, value);
		}
		static void AddParryRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.Parry, value);
		}
		static void AddBlockRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.Block, value);
		}
		static void AddMeleeHitRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeHitChance, value);
		}
		static void AddRangedHitRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.RangedHitChance, value);
		}
		static void AddSpellHitRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.SpellHitChance, value);
		}
		static void AddMeleeCriticalStrikeRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeCritChance, value);
		}
		static void AddRangedCriticalStrikeRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.RangedCritChance, value);
		}
		static void AddSpellCriticalStrikeRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.SpellCritChance, value);
		}
		static void AddMeleeHitAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeAttackerHit, value);
		}
		static void AddRangedHitAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.RangedAttackerHit, value);
		}
		static void AddSpellHitAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.SpellAttackerHit, value);
		}
		static void AddMeleeCriticalAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeResilience, value);
		}
		static void AddRangedCriticalAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.RangedResilience, value);
		}
		static void AddSpellCriticalAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.SpellResilience, value);
		}
		static void AddMeleeHasteRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeHaste, value);
		}
		static void AddRangedHasteRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.RangedHaste, value);
		}
		static void AddSpellHasteRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.SpellHaste, value);
		}
		static void AddHitRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeHitChance, value);
			owner.ModCombatRating(CombatRating.RangedHitChance, value);
			owner.ModCombatRating(CombatRating.SpellHitChance, value);
		}

		static void AddCriticalStrikeRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeCritChance, value);
			owner.ModCombatRating(CombatRating.RangedCritChance, value);
            owner.ModCombatRating(CombatRating.SpellCritChance, value);
		}
		static void AddHitAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeAttackerHit, -value);
		}
		static void AddCriticalAvoidanceRating(Character owner, int value)
		{
		}
		static void AddResilienceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeResilience, value);
			owner.ModCombatRating(CombatRating.RangedResilience, value);
			owner.ModCombatRating(CombatRating.SpellResilience, value);
		}
		static void AddHasteRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeHaste, value);
			owner.ModCombatRating(CombatRating.RangedHaste, value);
		}
		static void AddExpertiseRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.Expertise, value);
		}
		#endregion

		#region Remove
		static void RemovePower(Character owner, int value)
		{
			owner.ChangeModifier(StatModifierInt.Power, -value);
		}
		static void RemoveHealth(Character owner, int value)
		{
            owner.ChangeModifier(StatModifierInt.Health, -value);
		}

		static void RemoveUnused(Character owner, int value)
		{
		}
		

		static void RemoveWeaponSkillRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.WeaponSkill, -value);
		}
		static void RemoveDefenseRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.DefenseSkill, -value);
		}
		static void RemoveDodgeRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.Dodge, -value);
		}
		static void RemoveParryRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.Parry, -value);
		}
		static void RemoveBlockRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.Block, -value);
		}
		static void RemoveMeleeHitRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeHitChance, -value);
		}
		static void RemoveRangedHitRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.RangedHitChance, -value);
		}
		static void RemoveSpellHitRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.SpellHitChance, -value);
		}
		static void RemoveMeleeCriticalStrikeRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeCritChance, -value);
		}
		static void RemoveRangedCriticalStrikeRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.RangedCritChance, -value);
		}
		static void RemoveSpellCriticalStrikeRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.SpellCritChance, -value);
		}
		static void RemoveMeleeHitAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeAttackerHit, -value);
		}
		static void RemoveRangedHitAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.RangedAttackerHit, -value);
		}
		static void RemoveSpellHitAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.SpellAttackerHit, -value);
		}
		static void RemoveMeleeCriticalAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeResilience, -value);
		}
		static void RemoveRangedCriticalAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.RangedResilience, -value);
		}
		static void RemoveSpellCriticalAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.SpellResilience, -value);
		}
		static void RemoveMeleeHasteRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeHaste, -value);
		}
		static void RemoveRangedHasteRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.RangedHaste, -value);
		}
		static void RemoveSpellHasteRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.SpellHaste, -value);
		}
		static void RemoveHitRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeHitChance, -value);
			owner.ModCombatRating(CombatRating.RangedHitChance, -value);
		}

		static void RemoveCriticalStrikeRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeCritChance, -value);
			owner.ModCombatRating(CombatRating.RangedCritChance, -value);
            owner.ModCombatRating(CombatRating.SpellCritChance, -value);
		}
		static void RemoveHitAvoidanceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeAttackerHit, -value);
		}
		static void RemoveCriticalAvoidanceRating(Character owner, int value)
		{
		}
		static void RemoveResilienceRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeResilience, -value);
			owner.ModCombatRating(CombatRating.RangedResilience, -value);
			owner.ModCombatRating(CombatRating.SpellResilience, -value);
		}
		static void RemoveHasteRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.MeleeHaste, -value);
			owner.ModCombatRating(CombatRating.RangedHaste, -value);
		}
		static void RemoveExpertiseRating(Character owner, int value)
		{
			owner.ModCombatRating(CombatRating.Expertise, -value);
		}

		#endregion


		static void AddSpellPower(Character owner, int value)
		{
			AddSpellDamageDone(owner, value);
			AddSpellHealingDone(owner, value);
		}
		static void RemoveSpellPower(Character owner, int value)
		{
			RemoveSpellDamageDone(owner, value);
			RemoveSpellHealingDone(owner, value);
		}

		static void AddSpellDamageDone(Character owner, int value)
		{
		    owner.IntMods[(int) StatModifierInt.SpellDamage] += value;
			//owner.AddDamageDoneMod(SpellConstants.AllDamageSchoolSet, value);
		}
		static void RemoveSpellDamageDone(Character owner, int value)
        {
            owner.IntMods[(int)StatModifierInt.SpellDamage] -= value;
			//owner.RemoveDamageDoneMod(SpellConstants.AllDamageSchoolSet, value);
		}

		static void AddSpellHealingDone(Character owner, int value)
		{
			owner.HealingDoneMod += value;
		}
		static void RemoveSpellHealingDone(Character owner, int value)
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
			if (owner.PowerType == PowerType.Mana)
			{
				owner.ChangeModifier(StatModifierInt.PowerRegen, value);
			}
		}
		private static void RemoveManaRegen(Character owner, int value)
		{
			if (owner.PowerType == PowerType.Mana)
			{
				owner.ChangeModifier(StatModifierInt.PowerRegen, -value);
			}
		}

		private static void AddHealthRegen(Character owner, int value)
		{
			if (owner.PowerType == PowerType.Mana)
			{
				owner.ChangeModifier(StatModifierInt.HealthRegen, value);
			}
		}
		private static void RemoveHealthRegen(Character owner, int value)
		{
			if (owner.PowerType == PowerType.Mana)
			{
				owner.ChangeModifier(StatModifierInt.HealthRegen, -value);
			}
        }
        private static void AddHealthRegenInCombat(Character owner, int value)
        {
            
                owner.ChangeModifier(StatModifierInt.HealthRegenInCombat, value);
            
        }
        private static void RemoveHealthRegenInCombat(Character owner, int value)
        {
            
                owner.ChangeModifier(StatModifierInt.HealthRegenInCombat, -value);
           
        }
        #region from asda2
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
            owner.ChangeModifier(StatModifierFloat.SellingCost, (float)value / 100);
        }
        private static void RemoveSellingCost(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.SellingCost, -(float)value / 100);
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
            owner.ChangeModifier(StatModifierFloat.Asda2Defence, (float)value / 100);
        }
        private static void RemoveAsda2DefencePrcValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2Defence, -(float)value / 100);
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
            owner.ChangeModifier(StatModifierFloat.Asda2MagicDefence, (float)value / 100);
        }
        private static void RemoveAsda2MagicDefencePrcValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2MagicDefence, -(float)value / 100);
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
            owner.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float)value / 100);
        }
        private static void RemoveAsda2ExpAmount(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Asda2ExpAmount, -(float)value / 100);
        }
        private static void AddDamagePrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Damage, (float)value / 100);
        }
        private static void RemoveDamagePrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Damage, -(float)value / 100);
        }
        private static void AddMagicDamagePrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.MagicDamage, (float)value / 100);
        }
        private static void RemoveMagicDamagePrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.MagicDamage, -(float)value / 100);
        }
        private static void AddDamage(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Damage, value);
        }
        private static void RemoveDamage(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Damage,- value);
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
            owner.ChangeModifier(StatModifierFloat.Strength, (float)value / 100);
        }
        private static void RemoveStrengthPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Strength, -(float)value / 100);
        }
        private static void AddAgilityPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Agility, (float)value / 100);
        }
        private static void RemoveAgilityPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Agility, -(float)value / 100);
        }
        private static void AddStaminaPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Stamina, (float)value / 100);
        }
        private static void RemoveStaminaPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Stamina, -(float)value / 100);
        }
        private static void AddIntelectPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Intelect, (float)value / 100);
        }
        private static void RemoveIntelectPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Intelect, -(float)value / 100);
        }
        private static void AddEnergyPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Spirit, (float)value / 100);
        }
        private static void RemoveEnergyPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Spirit, -(float)value / 100);
        }
        private static void AddAllMagicResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireResist, (float)value / 100);
            owner.ChangeModifier(StatModifierFloat.EarthResit, (float)value / 100);
            owner.ChangeModifier(StatModifierFloat.DarkResit, (float)value / 100);
            owner.ChangeModifier(StatModifierFloat.LightResist, (float)value / 100);
            owner.ChangeModifier(StatModifierFloat.ClimateResist, (float)value / 100);
            owner.ChangeModifier(StatModifierFloat.WaterResist, (float)value / 100);
        }
        private static void RemoveAllMagicResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireResist, -(float)value / 100);
            owner.ChangeModifier(StatModifierFloat.EarthResit, -(float)value / 100);
            owner.ChangeModifier(StatModifierFloat.DarkResit, -(float)value / 100);
            owner.ChangeModifier(StatModifierFloat.LightResist, -(float)value / 100);
            owner.ChangeModifier(StatModifierFloat.ClimateResist, -(float)value / 100);
            owner.ChangeModifier(StatModifierFloat.WaterResist, -(float)value / 100);
        }
        private static void AddLuckPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Luck, (float)value / 100);
        }
        private static void RemoveLuckPrc(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Luck, -(float)value / 100);
        }
        static void AddAgility(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Agility, value);
        }
        static void AddStrength(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Strength, value);
        }
        static void AddIntellect(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Intellect, value);
        }
        static void AddSpirit(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Spirit, value);
        }
        static void AddStamina(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Stamina, value);
        }
        static void RemoveAgility(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Agility, -value);
        }
        static void RemoveStrength(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Strength, -value);
        }
        static void RemoveIntellect(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Intellect, -value);
        }
        static void RemoveSpirit(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Spirit, -value);
        }
        static void RemoveStamina(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Stamina, -value);
        }
        private static void RemoveSpeed(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Speed, -(float)value / 100);
        }

        private static void AddSpeed(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.Speed, (float)value / 100);
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
        #region resist
        private static void RemoveFireResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireResist, -(float)value/100);
        }

        private static void AddFireResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireResist, (float)value / 100);
        }

        private static void RemoveEarthResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.EarthResit, -(float)value / 100);
        }

        private static void AddEarthResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.EarthResit, (float)value / 100);
        }

        private static void RemoveClimateResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.ClimateResist, -(float)value / 100);
        }

        private static void AddClimateResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.ClimateResist, (float)value / 100);
        }

        private static void RemoveWaterResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.WaterResist, -(float)value / 100);
        }

        private static void AddWaterResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.WaterResist, (float)value / 100);
        }

        private static void RemoveLightResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.LightResist, -(float)value / 100);
        }

        private static void AddLightResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.LightResist, (float)value / 100);
        }

        private static void RemoveDarkResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.DarkResit, -(float)value / 100);
        }

        private static void AddDarkResistance(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.DarkResit, (float)value / 100);
        }
        #endregion

        #region attrib
        private static void RemoveFireAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireAttribute, -(float)value / 100);
        }

        private static void AddFireAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.FireAttribute, (float)value / 100);
        }

        private static void RemoveEarthAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.EarthAttribute, -(float)value / 100);
        }

        private static void AddEarthAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.EarthAttribute, (float)value / 100);
        }

        private static void RemoveClimateAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.ClimateAttribute, -(float)value / 100);
        }

        private static void AddClimateAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.ClimateAttribute, (float)value / 100);
        }

        private static void RemoveWaterAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.WaterAttribute, -(float)value / 100);
        }

        private static void AddWaterAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.WaterAttribute, (float)value / 100);
        }

        private static void RemoveLightAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.LightAttribute, -(float)value / 100);
        }

        private static void AddLightAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.LightAttribute, (float)value / 100);
        }

        private static void RemoveDarkAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.DarkAttribute, -(float)value / 100);
        }

        private static void AddDarkAttribute(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.DarkAttribute, (float)value / 100);
        }
#endregion
        #endregion
    }
}