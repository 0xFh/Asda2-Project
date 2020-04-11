namespace WCell.Constants.Spells
{
	/// <summary>
	/// Used for AddModifier* AuraEffects
	/// </summary>
	public enum SpellModifierType
	{
		/// <summary>
		/// Modifies damage
		/// </summary>
		SpellPower = 0,
		/// <summary>
		/// Modifies effect duration of Auras in ms
		/// </summary>
		Duration = 1,
		/// <summary>
		/// Modifies generated Threat
		/// </summary>
		Threat = 2,
		/// <summary>
		/// Modifies first effect's value in %
		/// </summary>
		EffectValue1 = 3,
		/// <summary>
		/// Amount of Charges
		/// </summary>
		Charges = 4,
		/// <summary>
		/// Max range of the Spell in yards
		/// </summary>
		Range = 5,
		/// <summary>
		/// Effect radius in yards
		/// </summary>
		Radius = 6,
		/// <summary>
		/// Modifies Critical Hit chance in %
		/// </summary>
		CritChance = 7,
		/// <summary>
		/// Usually increases the EffectValue of all effects
		/// but especially when it applies to pets, it can have a great variiety of effects
		/// </summary>
		AllEffectValues = 8,
		/// <summary>
		/// Reduces the pushback time when interrupted while casting
		/// </summary>
		PushbackReduction = 9,
		/// <summary>
		/// Spell cast time in ms
		/// </summary>
		CastTime = 10,
		/// <summary>
		/// Spell cooldown time in ms/%
		/// </summary>
		CooldownTime = 11,
		/// <summary>
		/// Increases EffectValue of the 2nd effect
		/// </summary>
		EffectValue2 = 12,
		/// <summary>
		/// Modifies power-cost (/10) or in %
		/// </summary>
		PowerCost = 14,
		/// <summary>
		/// Modifies Damage bonus on Critical hits in %
		/// </summary>
		CritDamage = 15,
		/// <summary>
		/// Modifies the chance of your target resisting your spell in %
		/// </summary>
		TargetResistance = 16,
		/// <summary>
		/// Adds additional ChainTargets
		/// </summary>
		ChainTargets = 17,
		/// <summary>
		/// Modifies ProcChance
		/// </summary>
		ProcChance = 18,

		/// <summary>
		/// Reduces channel time
		/// </summary>
		ActivationTime = 19,

		/// <summary>
		/// Modifies the factor applied to successive targets of chain hits (only with AddModifierPercent)
		/// </summary>
		ChainValueFactor = 20,

		/// <summary>
		/// 
		/// </summary>
		GlobalCooldown = 21,

		/// <summary>
		/// Modifies the EffectValue (positive) for periodic aura effects
		/// </summary>
		PeriodicEffectValue = 22,

		/// <summary>
		/// Modifies the third effect
		/// </summary>
		EffectValue3 = 23,
		/// <summary>
		/// Increases by % of SpellPower
		/// </summary>
		SpellPowerEffect = 24,
		/// <summary>
		/// Makes manashield cheaper, increases effects of Draining, increases healing of Death Coil etc
		/// </summary>
		HealingOrPowerGain = 27,
		/// <summary>
		/// Chance of this spell being resisted against dispel
		/// </summary>
		DispelResistance = 28,

		/// <summary>
		/// Only used in DruidFeralCombatPrimalPrecision:
		/// "you are refunded $s2% of the energy cost of a finishing move if it fails to land."
		/// </summary>
		RefundFailedFinishingMoveEnergy = 30,

		/// <summary>
		/// CUSTOM: To hack around the DBC hack-arounds
		/// </summary>
		EffectValue4AndBeyond = 100
	}
}