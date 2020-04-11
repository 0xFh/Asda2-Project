using System;
using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// TODO: Move everything Unit-related from UnitUpdates in here
	/// </summary>
	public partial class Unit
	{
		/// <summary>
		/// Amount of mana to be added per point of Intelligence
		/// </summary>
		public static int ManaPerIntelligence = 15;

		/// <summary>
		/// Amount of heatlh to be added per point of Stamina
		/// </summary>
		public static int HealthPerStamina = 10;

		/// <summary>
		/// Amount of armor to be added per point of Agility
		/// </summary>
		public static int ArmorPerAgility = 2;

		#region Str, Sta, Agi, Int, Spi
		protected internal virtual void UpdateStrength()
		{
			var str = GetBaseStatValue(StatType.Strength) + StrengthBuffPositive + StrengthBuffNegative;
			//str = GetMultiMod(unit.MultiplierMods[(int)StatModifierFloat.Strength], str);
			SetInt32(UnitFields.STAT0, str);

			this.UpdateBlockChance();
			this.UpdateAllAttackPower();
		}

		protected internal virtual void UpdateStamina()
		{
			var stam = GetBaseStatValue(StatType.Stamina) + StaminaBuffPositive + StaminaBuffNegative;

			SetInt32(UnitFields.STAT2, stam);

			UpdateMaxHealth();
		}

		internal void UpdateAgility()
		{
			var oldAgil = Agility;
			var agil = GetBaseStatValue(StatType.Agility) + AgilityBuffPositive + AgilityBuffNegative;
			//agil = GetMultiMod(unit.MultiplierMods[(int)StatModifierFloat.Agility], agil);
			SetInt32(UnitFields.STAT1, agil);

			ModBaseResistance(DamageSchool.Physical, (agil - oldAgil) * ArmorPerAgility);	// armor

			this.UpdateDodgeChance();
			this.UpdateCritChance();
			this.UpdateAllAttackPower();
		}

		protected internal virtual void UpdateIntellect()
		{
			var intel = GetBaseStatValue(StatType.Intellect) + IntellectBuffPositive + IntellectBuffNegative;
			//intel = intel < 0 ? 0 : GetMultiMod(unit.MultiplierMods[(int)StatModifierFloat.Intellect], intel);
			SetInt32(UnitFields.STAT3, intel);

			UpdateMaxPower();
		}

		protected internal virtual void UpdateSpirit()
		{
			var spirit = GetBaseStatValue(StatType.Spirit) + SpiritBuffPositive + SpiritBuffNegative;

			SetInt32(UnitFields.STAT4, spirit);

			this.UpdateNormalHealthRegen();

			// We don't need to call when we are still in the process of loading
			if (Intellect != 0)
			{
				this.UpdatePowerRegen();
			}
		}

		protected internal virtual void UpdateStat(StatType stat)
		{
			switch (stat)
			{
				case StatType.Strength:
					UpdateStrength();
					break;
				case StatType.Agility:
					UpdateAgility();
					break;
				case StatType.Stamina:
					UpdateStamina();
					break;
				case StatType.Intellect:
					UpdateIntellect();
					break;
				case StatType.Spirit:
					UpdateSpirit();
					break;
			}
		}
		#endregion

		#region Health & Power
		protected internal virtual void UpdateMaxHealth()
		{
			/*var stamina = Stamina;
			var uncontributed = StaminaWithoutHealthContribution;
			var stamBonus = Math.Max(stamina, uncontributed) + (Math.Max(0, stamina - uncontributed) * HealthPerStamina);

			var value = BaseHealth + stamBonus + MaxHealthModFlat;
			value += (int)(value * MaxHealthModScalar + 0.5f);*/

            MaxHealth = (int) UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Health], IntMods[(int)StatModifierInt.Health] + BaseHealth + CharacterFormulas.CalculateHealthBonus(Level,Asda2Strength,Asda2Stamina,Class)); 
            
			this.UpdateHealthRegen();
		}

		/// <summary>
		/// Amount of mana, contributed by intellect
		/// </summary>
		protected internal virtual int IntellectManaBonus
		{
			get { return Intellect; }
		}

	    private float _asda2Defence;
	    public float Asda2Defence
	    {
	        get { return _asda2Defence; }
            set { _asda2Defence = value;
                /*var chr = this as Character;
                if(chr!=null)
                    Asda2CharacterHandler.SendUpdateStatsResponse(chr.Client);*/
            }
	    }

	    private float _asda2MagicDefence;

	    public float Asda2MagicDefence
	    {
	        get { return _asda2MagicDefence; }
	        set { _asda2MagicDefence = value;
            /*var chr = this as Character;
            if (chr != null)
                Asda2CharacterHandler.SendUpdateStatsResponse(chr.Client);*/
            }
	    }

	    protected int CritDamageBonusPrc { get; set; }

	    protected internal void UpdateMaxPower()
		{
            var value = BasePower + IntMods[(int)StatModifierInt.Power] + CharacterFormulas.CalculateManaBonus(Level,Class,Asda2Spirit);
			/*if (PowerType == PowerType.Mana)
			{
				value += IntellectManaBonus;
			}*/
			value += (value * IntMods[(int)StatModifierInt.PowerPct] + 50) / 100;
			if (value < 0)
			{
				value = 0;
			}

			MaxPower = value;

			this.UpdatePowerRegen();
		}
		#endregion

	    public void UpdateAsda2Defence()
	    {
            Asda2Defence = UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Asda2Defence], IntMods[(int)StatModifierInt.Asda2Defence] + CharacterFormulas.ClaculateDefenceBonus(Level,Class,Asda2Agility));
	    }

	   
        public void UpdateAsda2MagicDefence()
        {
            Asda2MagicDefence = UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Asda2MagicDefence], IntMods[(int)StatModifierInt.Asda2MagicDefence] + CharacterFormulas.CalculateMagicDefencePointsBonus(Level,Class,Asda2Spirit));
        }

	    
	    public void UpdateAsda2DropChance()
	    {
            Asda2DropChance = UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Asda2DropChance] + CharacterFormulas.CalculateDropChanceBoost(Asda2Luck), 1f);
	    }

	    public void UpdateAsda2GoldAmount()
	    {
            Asda2GoldAmountBoost = UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Asda2GoldAmount] + CharacterFormulas.CalculateGoldAmountDropBoost(Level,Class,Asda2Luck), 1f);
	    }

	    public void UpdateAsda2ExpAmount()
	    {
            Asda2ExpAmountBoost = UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Asda2ExpAmount], 1f);
	    }

        //todo asda2 complete refenrece stat updates
	    public void UpdateAsda2Luck()
	    {
            Asda2Luck =  UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Luck], IntMods[(int)StatModifierInt.Luck] + Asda2BaseLuck);
	        this.UpdateCritChance();
            UpdateAsda2DropChance();
            UpdateAsda2GoldAmount();
            UpdateCritDamageBonus();
	    }
        public void UpdateAsda2Spirit()
        {
            Asda2Spirit = UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Spirit], IntMods[(int)StatModifierInt.Spirit] + Asda2BaseSpirit);
            UpdateAsda2MagicDefence();
            UpdateMaxPower();
            this.UpdatePowerRegen();
        }
        public void UpdateAsda2Intellect()
        {
            Asda2Intellect = UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Intelect], IntMods[(int)StatModifierInt.Intellect] + Asda2BaseIntellect);
            this.UpdateMainDamage();
            UpdateCritDamageBonus();
        }

	    public void UpdateAsda2Stamina()
        {
            Asda2Stamina = UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Stamina], IntMods[(int)StatModifierInt.Stamina] + Asda2BaseStamina);
            UpdateMaxHealth();
        }
         public void UpdateAsda2Strength()
	    {
            Asda2Strength = UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Strength], IntMods[(int)StatModifierInt.Strength] + Asda2BaseStrength);
            this.UpdateMainDamage();
            UpdateCritDamageBonus();
            UpdateMaxHealth();
         }
         public void UpdateAsda2Agility()
	    {
            Asda2Agility =  UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Agility], IntMods[(int)StatModifierInt.Agility] + Asda2BaseAgility);
            this.UpdateCritChance();
             this.UpdateAllAttackTimes();
             this.UpdateDodgeChance();
             UpdateSpeedFactor();
             UpdateAsda2Defence();
             UpdateCritDamageBonus();
             this.UpdateMainDamage();
	    }
         public void UpdateLightResistence()
	    {
            Asda2LightResistence = IntMods[(int)StatModifierFloat.LightResist];
	    }
         public void UpdateDarkResistence()
         {
             Asda2DarkResistence = IntMods[(int)StatModifierFloat.DarkResit];
         }
         public void UpdateEarthResistence()
         {
             Asda2EarthResistence = IntMods[(int)StatModifierFloat.EarthResit];
         }
         public void UpdateFireResistence()
         {
             Asda2FireResistence = IntMods[(int)StatModifierFloat.FireResist];
         }
         public void UpdateClimateResistence()
         {
             Asda2ClimateResistence = IntMods[(int)StatModifierFloat.ClimateResist];
         }
         public void UpdateWaterResistence()
         {
             Asda2WaterResistence = IntMods[(int)StatModifierFloat.WaterResist];
         }

         public void UpdateLightAttribute()
         {
             Asda2LightAttribute = IntMods[(int)StatModifierFloat.LightAttribute];
         }
         public void UpdateDarkAttribute()
         {
             Asda2DarkAttribute = IntMods[(int)StatModifierFloat.DarkAttribute];
         }
         public void UpdateEarthAttribute()
         {
             Asda2EarthAttribute = IntMods[(int)StatModifierFloat.EarthAttribute];
         }
         public void UpdateFireAttribute()
         {
             Asda2FireAttribute = IntMods[(int)StatModifierFloat.FireAttribute];
         }
         public void UpdateClimateAttribute()
         {
             Asda2ClimateAttribute = IntMods[(int)StatModifierFloat.ClimateAttribute];
         }
         public void UpdateWaterAttribute()
         {
             Asda2WaterAttribute = IntMods[(int)StatModifierFloat.WaterAttribute];
         }
        public void UpdateSpeedFactor()
        {
            var bns = CharacterFormulas.CalcSpeedBonus(Level, Class, Asda2Agility);
            if (bns > 1)
                bns = 1;
             SpeedFactor = UnitUpdates.GetMultiMod(FloatMods[(int)StatModifierFloat.Speed] + bns, DefaultSpeedFactor);
            var chr = this as Character;
            if(chr!=null)
                GlobalHandler.SendSpeedChangedResponse(chr.Client);
         }
        public void UpdateCritDamageBonus()
        {
            CritDamageBonusPrc = CharacterFormulas.CalculateCriticalDamageBonus(Level, Class, Asda2Agility, Asda2Luck, Asda2Intellect,
                                                           Asda2Strength);
        }
	}
}
