using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.RacesClasses;
using WCell.Util;

namespace WCell.RealmServer.Modifiers
{
  /// <summary>
  /// Container for methods to update Unit-Fields.
  /// DEPRECATED: Move to Unit.StatUpdates.cs, Character.StatUpdates.cs and NPC.StatUpdates
  /// TODO: Get rid of the UpdateHandlers and this whole array of update values (make them properties instead)
  /// </summary>
  public static class UnitUpdates
  {
    private static readonly UpdateHandler NothingHandler = unit => { };
    public static readonly int FlatIntModCount = (int) Utility.GetMaxEnum<StatModifierInt>();
    public static readonly int MultiplierModCount = (int) Utility.GetMaxEnum<StatModifierFloat>();

    private static readonly UpdateHandler[] FlatIntModHandlers =
      new UpdateHandler[FlatIntModCount + 1];

    private static readonly UpdateHandler[] MultiModHandlers =
      new UpdateHandler[MultiplierModCount + 1];

    static UnitUpdates()
    {
      FlatIntModHandlers[1] = unit => unit.UpdateMaxPower();
      FlatIntModHandlers[2] = unit => unit.UpdateMaxPower();
      FlatIntModHandlers[31] = UpdateHealth;
      FlatIntModHandlers[3] = UpdateHealthRegen;
      FlatIntModHandlers[4] = UpdateCombatHealthRegen;
      FlatIntModHandlers[5] = UpdateNormalHealthRegen;
      FlatIntModHandlers[6] = UpdatePowerRegen;
      FlatIntModHandlers[7] = UpdatePowerRegen;
      FlatIntModHandlers[19] = UpdatePowerRegen;
      FlatIntModHandlers[13] = UpdateDodgeChance;
      FlatIntModHandlers[9] = UpdateBlockChance;
      FlatIntModHandlers[8] = UpdateBlockChance;
      FlatIntModHandlers[11] = UpdateCritChance;
      FlatIntModHandlers[38] = UpdateCritChance;
      FlatIntModHandlers[10] = UpdateParryChance;
      FlatIntModHandlers[15] = UpdateMeleeHitChance;
      FlatIntModHandlers[16] = UpdateRangedHitChance;
      FlatIntModHandlers[23] = UpdateExpertise;
      FlatIntModHandlers[20] = Asda2Defence;
      FlatIntModHandlers[22] = Asda2Luck;
      FlatIntModHandlers[21] = Asda2MagicDefence;
      FlatIntModHandlers[26] = Asda2Agility;
      FlatIntModHandlers[28] = Asda2Intellect;
      FlatIntModHandlers[27] = Asda2Strength;
      FlatIntModHandlers[30] = Asda2Stamina;
      FlatIntModHandlers[29] = Asda2Spirit;
      FlatIntModHandlers[25] = Asda2Damage;
      FlatIntModHandlers[24] = Asda2MagicDamage;
      MultiModHandlers[0] = UpdateBlockChance;
      MultiModHandlers[1] = UpdateMeleeAttackTimes;
      MultiModHandlers[2] = UpdateMeleeAttackTimes;
      MultiModHandlers[3] = UpdateHealthRegen;
      MultiModHandlers[4] = Asda2Defence;
      MultiModHandlers[5] = Asda2MagicDefence;
      MultiModHandlers[6] = Asda2DropChance;
      MultiModHandlers[7] = Asda2GoldAmount;
      MultiModHandlers[8] = Asda2ExpAmount;
      MultiModHandlers[10] = Asda2Damage;
      MultiModHandlers[11] = Asda2MagicDamage;
      MultiModHandlers[12] = Asda2Strength;
      MultiModHandlers[14] = Asda2Stamina;
      MultiModHandlers[15] = Asda2Intellect;
      MultiModHandlers[16] = Asda2Spirit;
      MultiModHandlers[9] = Asda2Luck;
      MultiModHandlers[13] = Asda2Agility;
      MultiModHandlers[21] = UpdateClimateResistance;
      MultiModHandlers[19] = UpdateDarkResistance;
      MultiModHandlers[18] = UpdateEarthResistance;
      MultiModHandlers[17] = UpdateFireResistance;
      MultiModHandlers[22] = UpdateWaterResistance;
      MultiModHandlers[20] = UpdateLightResistance;
      MultiModHandlers[27] = UpdateClimateAttribute;
      MultiModHandlers[24] = UpdateDarkAttribute;
      MultiModHandlers[25] = UpdateEarthAttribute;
      MultiModHandlers[26] = UpdateFireAttribute;
      MultiModHandlers[28] = UpdateWaterAttribute;
      MultiModHandlers[23] = UpdateLightAttribute;
      MultiModHandlers[29] = UpdateSpeed;
      MultiModHandlers[31] = UpdateHealth;
    }

    /// <summary>
    /// Updates the amount of Health regenerated per regen-tick, independent on the combat state
    /// </summary>
    internal static void UpdateHealthRegen(this Unit unit)
    {
      unit.UpdateNormalHealthRegen();
      unit.UpdateCombatHealthRegen();
    }

    /// <summary>
    /// Updates the amount of Health regenerated per regen-tick
    /// </summary>
    internal static void UpdateNormalHealthRegen(this Unit unit)
    {
      int num = (!(unit is Character) ? 0 : ((Character) unit).Archetype.Class.CalculateHealthRegen(unit)) +
                (unit.IntMods[3] + unit.IntMods[5]);
      int multiMod = GetMultiMod((int) unit.FloatMods[3], num);
      unit.HealthRegenPerTickNoCombat = multiMod;
    }

    /// <summary>
    /// Updates the amount of Health regenerated per regen-tick
    /// </summary>
    internal static void UpdateCombatHealthRegen(this Unit unit)
    {
      unit.HealthRegenPerTickCombat = unit.IntMods[3] + unit.IntMods[4];
    }

    /// <summary>
    /// Updates the amount of Power regenerated per regen-tick
    /// </summary>
    internal static void UpdatePowerRegen(this Unit unit)
    {
      if(unit.IsSitting)
      {
        unit.PowerRegenPerTickActual = unit.MaxPower * 0.009f;
      }
      else
      {
        float num = 0.0f + unit.Asda2Spirit * CharacterFormulas.ManaPointsPerOneSpirit;
        float multiMod = GetMultiMod(unit.FloatMods[32], num);
        unit.PowerRegenPerTickActual = (double) multiMod < 0.300000011920929 ? 0.3f : multiMod;
      }
    }

    internal static void UpdateAllAttackPower(this Unit unit)
    {
      unit.UpdateMeleeAttackPower();
      unit.UpdateRangedAttackPower();
    }

    internal static void UpdateMeleeAttackPower(this Unit unit)
    {
      if(unit is Character)
      {
        Character character = (Character) unit;
        BaseClass baseClass = character.Archetype.Class;
        int level = character.Level;
        int agility = character.Agility;
        int strength = unit.Strength;
        int meleeAp = baseClass.CalculateMeleeAP(level, strength, agility);
        if(character.m_MeleeAPModByStat != null)
        {
          for(StatType stat = StatType.Strength; stat < StatType.End; ++stat)
            meleeAp += (character.GetMeleeAPModByStat(stat) * character.GetTotalStatValue(stat) + 50) / 100;
        }

        character.MeleeAttackPower = meleeAp;
      }
      else if(unit is NPC)
      {
        NPC npc = (NPC) unit;
        if(npc.HasPlayerMaster)
        {
          Character master = (Character) npc.Master;
          BaseClass baseClass = master.Archetype.Class;
          int level = unit.Level;
          int agility = unit.Agility;
          int strength = unit.Strength;
          int meleeAp = baseClass.CalculateMeleeAP(level, strength, agility);
          if(npc.IsHunterPet)
            meleeAp += (master.TotalMeleeAP * PetMgr.PetAPOfOwnerPercent + 50) / 100;
          npc.MeleeAttackPower = meleeAp;
        }
      }

      unit.UpdateMainDamage();
    }

    internal static void UpdateRangedAttackPower(this Unit unit)
    {
      if(!(unit is Character))
        return;
      Character character = (Character) unit;
      BaseClass baseClass = character.Archetype.Class;
      int level = character.Level;
      int agility = character.Agility;
      int strength = unit.Strength;
      int rangedAp = baseClass.CalculateRangedAP(level, strength, agility);
      if(character.m_MeleeAPModByStat != null)
      {
        for(StatType stat = StatType.Strength; stat < StatType.End; ++stat)
          rangedAp += (character.GetRangedAPModByStat(stat) * character.GetTotalStatValue(stat) + 50) / 100;
      }

      character.RangedAttackPower = rangedAp;
      NPC activePet = character.ActivePet;
      if(activePet != null && activePet.IsHunterPet)
        activePet.UpdateMeleeAttackPower();
      unit.UpdateRangedDamage();
    }

    internal static void UpdateBlockChance(this Unit unit)
    {
    }

    internal static void UpdateSpellCritChance(this Character chr)
    {
      chr.UpdateCritChance();
    }

    internal static void UpdateParryChance(this Unit unit)
    {
    }

    /// <summary>Weapon changed</summary>
    internal static void UpdateDamage(this Character unit, InventorySlot slot)
    {
      switch(slot)
      {
        case InventorySlot.AvLeftHead:
          unit.UpdateMainDamage();
          unit.UpdateMainAttackTime();
          break;
        case InventorySlot.AvCloak:
        case InventorySlot.Invalid:
          unit.UpdateRangedDamage();
          break;
      }
    }

    internal static void UpdateAllDamages(this Unit unit)
    {
      unit.UpdateMainDamage();
      unit.UpdateRangedDamage();
    }

    /// <summary>
    /// Re-calculates the mainhand melee damage of this Unit
    /// References:
    /// http://www.wowwiki.com/Attack_power
    /// </summary>
    internal static void UpdateMainDamage(this Unit unit)
    {
      float num1 = 0.0f;
      float num2 = 0.0f;
      float num3 = 0.0f;
      float num4 = 0.0f;
      IAsda2Weapon mainWeapon = unit.MainWeapon;
      foreach(DamageInfo damage in mainWeapon.Damages)
      {
        if(damage.School == DamageSchoolMask.Magical)
        {
          num3 += CalcMagicDamage(unit, damage.Minimum);
          num4 += CalcMagicDamage(unit, damage.Maximum);
        }
        else
        {
          num1 += GetModifiedDamage(unit, damage.Minimum);
          num2 += GetModifiedDamage(unit, damage.Maximum);
        }
      }

      unit.MinDamage = num1 + mainWeapon.BonusDamage;
      unit.MaxDamage = num2 + mainWeapon.BonusDamage;
      unit.MinMagicDamage = (int) num3;
      unit.MaxMagicDamage = (int) num4;
      Character character = unit as Character;
      if(character == null)
        return;
      Asda2CharacterHandler.SendUpdateStatsResponse(character.Client);
    }

    private static float CalcMagicDamage(Unit unit, float dmg)
    {
      return GetMultiMod(unit.FloatMods[11],
        dmg + unit.IntMods[24] +
        CharacterFormulas.CalculateMagicDamageBonus(unit.Level, unit.Class, unit.Asda2Intellect));
    }

    private static float GetModifiedDamage(Unit unit, float dmg)
    {
      float psysicalDamageBonus =
        CharacterFormulas.CalculatePsysicalDamageBonus(unit.Level, unit.Asda2Agility, unit.Asda2Strength,
          unit.Class);
      return GetMultiMod(unit.FloatMods[10], dmg + unit.IntMods[25] + psysicalDamageBonus);
    }

    internal static void UpdateRangedDamage(this Unit unit)
    {
    }

    internal static void UpdateAllAttackTimes(this Unit unit)
    {
      unit.UpdateMainAttackTime();
    }

    internal static void UpdateMeleeAttackTimes(this Unit unit)
    {
      unit.UpdateMainAttackTime();
    }

    /// <summary>Re-calculates the mainhand melee damage of this Unit</summary>
    internal static void UpdateMainAttackTime(this Unit unit)
    {
      int attackTime = unit.MainWeapon.AttackTime;
      int num = GetMultiMod(
        unit.FloatMods[1] -
        CharacterFormulas.CalculateAtackTimeReduce(unit.Level, unit.Class, unit.Asda2Agility), attackTime);
      if(num < 30)
        num = 30;
      unit.MainHandAttackTime = num;
    }

    internal static void UpdateCritChance(this Unit unit)
    {
      Character character = unit as Character;
      if(character == null)
        return;
      float num = 0.0f + (((Character) unit).Archetype.Class.CalculateMeleeCritChance(unit.Level,
                            unit.Asda2Agility, unit.Asda2Luck) + unit.IntMods[38]);
      if(num > 50.0)
        num = 50f;
      character.CritChanceMeleePct = num;
      character.CritChanceRangedPct = num;
      character.CritChanceOffHandPct = num;
    }

    internal static void UpdateDodgeChance(this Unit unit)
    {
      Character character = unit as Character;
      if(character == null)
        return;
      float num1 = 0.0f;
      if(character.Asda2Agility == 0)
        return;
      float num2 = num1 + (unit.IntMods[13] +
                           CharacterFormulas.CalcDodgeChanceBonus(unit.Level, unit.Class, unit.Asda2Agility));
      double multiMod = GetMultiMod(unit.FloatMods[30], num2);
      character.DodgeChance = num2;
    }

    /// <summary>
    /// Increases the defense skill according to your defense rating
    /// Updates Dodge and Parry chances
    /// </summary>
    /// <param name="unit"></param>
    internal static void UpdateDefense(this Unit unit)
    {
    }

    internal static void UpdateModel(this Unit unit)
    {
      unit.BoundingRadius = unit.Model.BoundingRadius * unit.ScaleX;
    }

    internal static void UpdateMeleeHitChance(this Unit unit)
    {
    }

    internal static void UpdateRangedHitChance(this Unit unit)
    {
    }

    internal static void Asda2Luck(this Unit unit)
    {
      unit.UpdateAsda2Luck();
    }

    internal static void Asda2Agility(this Unit unit)
    {
      unit.UpdateAsda2Agility();
    }

    internal static void Asda2Stamina(this Unit unit)
    {
      unit.UpdateAsda2Stamina();
    }

    internal static void Asda2Intellect(this Unit unit)
    {
      unit.UpdateAsda2Intellect();
    }

    internal static void Asda2Spirit(this Unit unit)
    {
      unit.UpdateAsda2Spirit();
    }

    internal static void Asda2Strength(this Unit unit)
    {
      unit.UpdateAsda2Strength();
    }

    internal static void Asda2Defence(this Unit unit)
    {
      unit.UpdateAsda2Defence();
    }

    internal static void Asda2MagicDefence(this Unit unit)
    {
      unit.UpdateAsda2MagicDefence();
    }

    internal static void Asda2DropChance(this Unit unit)
    {
      unit.UpdateAsda2DropChance();
    }

    internal static void Asda2GoldAmount(this Unit unit)
    {
      unit.UpdateAsda2GoldAmount();
    }

    internal static void Asda2ExpAmount(this Unit unit)
    {
      unit.UpdateAsda2ExpAmount();
    }

    internal static void Asda2Damage(this Unit unit)
    {
      unit.UpdateMainDamage();
    }

    internal static void Asda2MagicDamage(this Unit unit)
    {
      unit.UpdateMainDamage();
    }

    private static void UpdateLightResistance(Unit unit)
    {
      unit.UpdateLightResistence();
    }

    private static void UpdateWaterResistance(Unit unit)
    {
      unit.UpdateWaterResistence();
    }

    private static void UpdateFireResistance(Unit unit)
    {
      unit.UpdateFireResistence();
    }

    private static void UpdateEarthResistance(Unit unit)
    {
      unit.UpdateEarthResistence();
    }

    private static void UpdateDarkResistance(Unit unit)
    {
      unit.UpdateDarkResistence();
    }

    private static void UpdateClimateResistance(Unit unit)
    {
      unit.UpdateClimateResistence();
    }

    private static void UpdateLightAttribute(Unit unit)
    {
      unit.UpdateLightAttribute();
    }

    private static void UpdateWaterAttribute(Unit unit)
    {
      unit.UpdateWaterAttribute();
    }

    private static void UpdateFireAttribute(Unit unit)
    {
      unit.UpdateFireAttribute();
    }

    private static void UpdateEarthAttribute(Unit unit)
    {
      unit.UpdateEarthAttribute();
    }

    private static void UpdateDarkAttribute(Unit unit)
    {
      unit.UpdateDarkAttribute();
    }

    private static void UpdateClimateAttribute(Unit unit)
    {
      unit.UpdateClimateAttribute();
    }

    private static void UpdateSpeed(Unit unit)
    {
      unit.UpdateSpeedFactor();
    }

    private static void UpdateHealth(Unit unit)
    {
      unit.UpdateMaxHealth();
    }

    internal static void UpdateExpertise(this Unit unit)
    {
      if(!(unit is Character))
        return;
      Character character = unit as Character;
      uint num = (uint) character.IntMods[23] +
                 (uint) (character.GetCombatRating(CombatRating.Expertise) /
                         (double) GameTables.GetCRTable(CombatRating.Expertise)[character.Level - 1]);
      character.Expertise = num;
    }

    /// <summary>
    /// Applies a percent modifier to the given value:
    /// 0 means unchanged; +x means multiply with x; -x means divide by (1 - x)
    /// </summary>
    public static int GetMultiMod(float modValue, int value)
    {
      return (int) (value * (1.0 + modValue) + 0.5);
    }

    public static float GetMultiMod(float modValue, float value)
    {
      return value * (1f + modValue);
    }

    internal static void UpdatePetResistance(this NPC pet, DamageSchool school)
    {
      int num;
      if(school == DamageSchool.Physical)
      {
        num = (pet.Armor * PetMgr.PetArmorOfOwnerPercent + 50) / 100;
        PetLevelStatInfo petLevelStatInfo = pet.Entry.GetPetLevelStatInfo(pet.Level);
        if(petLevelStatInfo != null)
          num += petLevelStatInfo.Armor;
      }
      else
        num = (pet.GetResistance(school) * PetMgr.PetResistanceOfOwnerPercent + 50) / 100;

      pet.SetBaseResistance(school, num);
    }

    /// <summary>Changes a flat modifier</summary>
    /// <param name="unit"></param>
    /// <param name="mod"></param>
    /// <param name="delta"></param>
    public static void ChangeModifier(this Unit unit, StatModifierInt mod, int delta)
    {
      unit.IntMods[(int) mod] += delta;
      if(FlatIntModHandlers[(int) mod] == null)
        return;
      FlatIntModHandlers[(int) mod](unit);
    }

    /// <summary>Changes a multiplier modifier</summary>
    /// <param name="unit"></param>
    /// <param name="mod"></param>
    /// <param name="delta"></param>
    public static void ChangeModifier(this Unit unit, StatModifierFloat mod, float delta)
    {
      unit.FloatMods[(int) mod] += delta;
      if(MultiModHandlers[(int) mod] == null)
        return;
      MultiModHandlers[(int) mod](unit);
    }

    private delegate void UpdateHandler(Unit unit);
  }
}