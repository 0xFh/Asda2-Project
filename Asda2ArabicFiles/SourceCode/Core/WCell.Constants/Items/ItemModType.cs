namespace WCell.Constants.Items
{
	/// <summary>
	/// Item modifiers
	/// </summary>
    public enum ItemModType
    {
        None = -1,
        /// <summary>
        /// Unused?
        /// </summary>
        Power = 0,
        /// <summary>
        /// Unused?
        /// </summary>
        Health = 1,
        /// <summary>
        /// Unused
        /// </summary>
        Unused = 2,
        Agility = 3,
        Strength = 4,
        Intellect = 5,
        Spirit = 6,
        Stamina = 7,

        WeaponSkillRating = 11,
        DefenseRating = 12,
        DodgeRating = 13,
        ParryRating = 14,
        BlockRating = 15,
        /// <summary>
        /// Unused
        /// </summary>
        MeleeHitRating = 16,
        /// <summary>
        /// Unused
        /// </summary>
        RangedHitRating = 17,
        /// <summary>
        /// Unused
        /// </summary>
        SpellHitRating = 18,
        MeleeCriticalStrikeRating = 19,
        RangedCriticalStrikeRating = 20,
        SpellCriticalStrikeRating = 21,
        /// <summary>
        /// Unused
        /// </summary>
        MeleeHitAvoidanceRating = 22,
        /// <summary>
        /// Unused
        /// </summary>
        RangedHitAvoidanceRating = 23,
        /// <summary>
        /// Unused
        /// </summary>
        SpellHitAvoidanceRating = 24,
        /// <summary>
        /// Unused (see Resilience)
        /// </summary>
        MeleeCriticalAvoidanceRating = 25,
        /// <summary>
        /// Unused (see Resilience)
        /// </summary>
        RangedCriticalAvoidanceRating = 26,
        /// <summary>
        /// Unused (see Resilience)
        /// </summary>
        SpellCriticalAvoidanceRating = 27,
        MeleeHasteRating = 28,
        RangedHasteRating = 29,
        SpellHasteRating = 30,
        /// <summary>
        /// Melee, Ranged and Spell HitRating
        /// </summary>
        HitRating = 31,

        /// <summary>
        /// Used
        /// </summary>
        CriticalStrikeRating = 32,
        /// <summary>
        /// Unused
        /// </summary>
        HitAvoidanceRating = 33,
        /// <summary>
        /// Unused (see Resilience)
        /// </summary>
        CriticalAvoidanceRating = 34,
        ResilienceRating = 35,
        HasteRating = 36,
        ExpertiseRating = 37,

        // 3.x
        AttackPower = 38,
        RangedAttackPower = 39,
        FeralAttackPower = 40,
        SpellHealingDone = 41,
        SpellDamageDone = 42,
        ManaRegeneration = 43,
        ArmorRegenRating = 44,
        SpellPower = 45,

        // 3.2.2
        HealthRegenration = 46,
        SpellPenetration = 47,
        BlockValue = 48,

        AtackTimePrc,

        Asda2Defence,
        Asda2MagicDefence,
        DropChance,
        DropGoldByPrc,
        Luck,
        Asda2Expirience,
        DamagePrc,
        MagicDamagePrc,
        StrengthPrc,
        AgilityPrc,
        IntelectPrc,
        LuckPrc,
        EnergyPrc,
        StaminaPrc,

        Asda2DefencePrc,
        Asda2MagicDefencePrc,
        AllMagicResistance,
        DarkResistance,
        LightResistance,
        WaterResistance,
        ClimateResistance,
        EarthResistance,
        FireResistance,
        DarkAttribute,
        LightAttribute,
        WaterAttribute,
        ClimateAttribute,
        EarthAttribute,
        FireAttribute,
        Speed,
        HealthRegenrationInCombat,
        CastingDistance,//todo
        Damage,
        MagicDamage,
	    SellingCost,
	    FishingSkill,
	    FishingGauge,
        End = 100,
    }
}