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
        private static readonly UnitUpdates.UpdateHandler NothingHandler = (UnitUpdates.UpdateHandler) (unit => { });
        public static readonly int FlatIntModCount = (int) Utility.GetMaxEnum<StatModifierInt>();
        public static readonly int MultiplierModCount = (int) Utility.GetMaxEnum<StatModifierFloat>();

        private static readonly UnitUpdates.UpdateHandler[] FlatIntModHandlers =
            new UnitUpdates.UpdateHandler[UnitUpdates.FlatIntModCount + 1];

        private static readonly UnitUpdates.UpdateHandler[] MultiModHandlers =
            new UnitUpdates.UpdateHandler[UnitUpdates.MultiplierModCount + 1];

        static UnitUpdates()
        {
            UnitUpdates.FlatIntModHandlers[1] = (UnitUpdates.UpdateHandler) (unit => unit.UpdateMaxPower());
            UnitUpdates.FlatIntModHandlers[2] = (UnitUpdates.UpdateHandler) (unit => unit.UpdateMaxPower());
            UnitUpdates.FlatIntModHandlers[31] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateHealth);
            UnitUpdates.FlatIntModHandlers[3] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateHealthRegen);
            UnitUpdates.FlatIntModHandlers[4] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateCombatHealthRegen);
            UnitUpdates.FlatIntModHandlers[5] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateNormalHealthRegen);
            UnitUpdates.FlatIntModHandlers[6] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdatePowerRegen);
            UnitUpdates.FlatIntModHandlers[7] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdatePowerRegen);
            UnitUpdates.FlatIntModHandlers[19] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdatePowerRegen);
            UnitUpdates.FlatIntModHandlers[13] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateDodgeChance);
            UnitUpdates.FlatIntModHandlers[9] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateBlockChance);
            UnitUpdates.FlatIntModHandlers[8] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateBlockChance);
            UnitUpdates.FlatIntModHandlers[11] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateCritChance);
            UnitUpdates.FlatIntModHandlers[38] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateCritChance);
            UnitUpdates.FlatIntModHandlers[10] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateParryChance);
            UnitUpdates.FlatIntModHandlers[15] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateMeleeHitChance);
            UnitUpdates.FlatIntModHandlers[16] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateRangedHitChance);
            UnitUpdates.FlatIntModHandlers[23] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateExpertise);
            UnitUpdates.FlatIntModHandlers[20] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Defence);
            UnitUpdates.FlatIntModHandlers[22] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Luck);
            UnitUpdates.FlatIntModHandlers[21] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2MagicDefence);
            UnitUpdates.FlatIntModHandlers[26] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Agility);
            UnitUpdates.FlatIntModHandlers[28] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Intellect);
            UnitUpdates.FlatIntModHandlers[27] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Strength);
            UnitUpdates.FlatIntModHandlers[30] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Stamina);
            UnitUpdates.FlatIntModHandlers[29] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Spirit);
            UnitUpdates.FlatIntModHandlers[25] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Damage);
            UnitUpdates.FlatIntModHandlers[24] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2MagicDamage);
            UnitUpdates.MultiModHandlers[0] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateBlockChance);
            UnitUpdates.MultiModHandlers[1] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateMeleeAttackTimes);
            UnitUpdates.MultiModHandlers[2] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateMeleeAttackTimes);
            UnitUpdates.MultiModHandlers[3] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateHealthRegen);
            UnitUpdates.MultiModHandlers[4] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Defence);
            UnitUpdates.MultiModHandlers[5] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2MagicDefence);
            UnitUpdates.MultiModHandlers[6] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2DropChance);
            UnitUpdates.MultiModHandlers[7] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2GoldAmount);
            UnitUpdates.MultiModHandlers[8] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2ExpAmount);
            UnitUpdates.MultiModHandlers[10] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Damage);
            UnitUpdates.MultiModHandlers[11] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2MagicDamage);
            UnitUpdates.MultiModHandlers[12] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Strength);
            UnitUpdates.MultiModHandlers[14] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Stamina);
            UnitUpdates.MultiModHandlers[15] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Intellect);
            UnitUpdates.MultiModHandlers[16] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Spirit);
            UnitUpdates.MultiModHandlers[9] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Luck);
            UnitUpdates.MultiModHandlers[13] = new UnitUpdates.UpdateHandler(UnitUpdates.Asda2Agility);
            UnitUpdates.MultiModHandlers[21] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateClimateResistance);
            UnitUpdates.MultiModHandlers[19] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateDarkResistance);
            UnitUpdates.MultiModHandlers[18] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateEarthResistance);
            UnitUpdates.MultiModHandlers[17] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateFireResistance);
            UnitUpdates.MultiModHandlers[22] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateWaterResistance);
            UnitUpdates.MultiModHandlers[20] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateLightResistance);
            UnitUpdates.MultiModHandlers[27] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateClimateAttribute);
            UnitUpdates.MultiModHandlers[24] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateDarkAttribute);
            UnitUpdates.MultiModHandlers[25] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateEarthAttribute);
            UnitUpdates.MultiModHandlers[26] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateFireAttribute);
            UnitUpdates.MultiModHandlers[28] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateWaterAttribute);
            UnitUpdates.MultiModHandlers[23] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateLightAttribute);
            UnitUpdates.MultiModHandlers[29] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateSpeed);
            UnitUpdates.MultiModHandlers[31] = new UnitUpdates.UpdateHandler(UnitUpdates.UpdateHealth);
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
            int multiMod = UnitUpdates.GetMultiMod((float) (int) unit.FloatMods[3], num);
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
            if (unit.IsSitting)
            {
                unit.PowerRegenPerTickActual = (float) unit.MaxPower * 0.009f;
            }
            else
            {
                float num = 0.0f + (float) unit.Asda2Spirit * CharacterFormulas.ManaPointsPerOneSpirit;
                float multiMod = UnitUpdates.GetMultiMod(unit.FloatMods[32], num);
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
            if (unit is Character)
            {
                Character character = (Character) unit;
                BaseClass baseClass = character.Archetype.Class;
                int level = character.Level;
                int agility = character.Agility;
                int strength = unit.Strength;
                int meleeAp = baseClass.CalculateMeleeAP(level, strength, agility);
                if (character.m_MeleeAPModByStat != null)
                {
                    for (StatType stat = StatType.Strength; stat < StatType.End; ++stat)
                        meleeAp += (character.GetMeleeAPModByStat(stat) * character.GetTotalStatValue(stat) + 50) / 100;
                }

                character.MeleeAttackPower = meleeAp;
            }
            else if (unit is NPC)
            {
                NPC npc = (NPC) unit;
                if (npc.HasPlayerMaster)
                {
                    Character master = (Character) npc.Master;
                    BaseClass baseClass = master.Archetype.Class;
                    int level = unit.Level;
                    int agility = unit.Agility;
                    int strength = unit.Strength;
                    int meleeAp = baseClass.CalculateMeleeAP(level, strength, agility);
                    if (npc.IsHunterPet)
                        meleeAp += (master.TotalMeleeAP * PetMgr.PetAPOfOwnerPercent + 50) / 100;
                    npc.MeleeAttackPower = meleeAp;
                }
            }

            unit.UpdateMainDamage();
        }

        internal static void UpdateRangedAttackPower(this Unit unit)
        {
            if (!(unit is Character))
                return;
            Character character = (Character) unit;
            BaseClass baseClass = character.Archetype.Class;
            int level = character.Level;
            int agility = character.Agility;
            int strength = unit.Strength;
            int rangedAp = baseClass.CalculateRangedAP(level, strength, agility);
            if (character.m_MeleeAPModByStat != null)
            {
                for (StatType stat = StatType.Strength; stat < StatType.End; ++stat)
                    rangedAp += (character.GetRangedAPModByStat(stat) * character.GetTotalStatValue(stat) + 50) / 100;
            }

            character.RangedAttackPower = rangedAp;
            NPC activePet = character.ActivePet;
            if (activePet != null && activePet.IsHunterPet)
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
            switch (slot)
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
            foreach (DamageInfo damage in mainWeapon.Damages)
            {
                if (damage.School == DamageSchoolMask.Magical)
                {
                    num3 += UnitUpdates.CalcMagicDamage(unit, damage.Minimum);
                    num4 += UnitUpdates.CalcMagicDamage(unit, damage.Maximum);
                }
                else
                {
                    num1 += UnitUpdates.GetModifiedDamage(unit, damage.Minimum);
                    num2 += UnitUpdates.GetModifiedDamage(unit, damage.Maximum);
                }
            }

            unit.MinDamage = num1 + (float) mainWeapon.BonusDamage;
            unit.MaxDamage = num2 + (float) mainWeapon.BonusDamage;
            unit.MinMagicDamage = (int) num3;
            unit.MaxMagicDamage = (int) num4;
            Character character = unit as Character;
            if (character == null)
                return;
            Asda2CharacterHandler.SendUpdateStatsResponse(character.Client);
        }

        private static float CalcMagicDamage(Unit unit, float dmg)
        {
            return UnitUpdates.GetMultiMod(unit.FloatMods[11],
                dmg + (float) unit.IntMods[24] +
                CharacterFormulas.CalculateMagicDamageBonus(unit.Level, unit.Class, unit.Asda2Intellect));
        }

        private static float GetModifiedDamage(Unit unit, float dmg)
        {
            float psysicalDamageBonus =
                CharacterFormulas.CalculatePsysicalDamageBonus(unit.Level, unit.Asda2Agility, unit.Asda2Strength,
                    unit.Class);
            return UnitUpdates.GetMultiMod(unit.FloatMods[10], dmg + (float) unit.IntMods[25] + psysicalDamageBonus);
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
            int num = UnitUpdates.GetMultiMod(
                unit.FloatMods[1] -
                CharacterFormulas.CalculateAtackTimeReduce(unit.Level, unit.Class, unit.Asda2Agility), attackTime);
            if (num < 30)
                num = 30;
            unit.MainHandAttackTime = num;
        }

        internal static void UpdateCritChance(this Unit unit)
        {
            Character character = unit as Character;
            if (character == null)
                return;
            float num = 0.0f + (((Character) unit).Archetype.Class.CalculateMeleeCritChance(unit.Level,
                                    unit.Asda2Agility, unit.Asda2Luck) + (float) unit.IntMods[38]);
            if ((double) num > 50.0)
                num = 50f;
            character.CritChanceMeleePct = num;
            character.CritChanceRangedPct = num;
            character.CritChanceOffHandPct = num;
        }

        internal static void UpdateDodgeChance(this Unit unit)
        {
            Character character = unit as Character;
            if (character == null)
                return;
            float num1 = 0.0f;
            if (character.Asda2Agility == 0)
                return;
            float num2 = num1 + ((float) unit.IntMods[13] +
                                 CharacterFormulas.CalcDodgeChanceBonus(unit.Level, unit.Class, unit.Asda2Agility));
            double multiMod = (double) UnitUpdates.GetMultiMod(unit.FloatMods[30], num2);
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
            if (!(unit is Character))
                return;
            Character character = unit as Character;
            uint num = (uint) character.IntMods[23] +
                       (uint) ((double) character.GetCombatRating(CombatRating.Expertise) /
                               (double) GameTables.GetCRTable(CombatRating.Expertise)[character.Level - 1]);
            character.Expertise = num;
        }

        /// <summary>
        /// Applies a percent modifier to the given value:
        /// 0 means unchanged; +x means multiply with x; -x means divide by (1 - x)
        /// </summary>
        public static int GetMultiMod(float modValue, int value)
        {
            return (int) ((double) value * (1.0 + (double) modValue) + 0.5);
        }

        public static float GetMultiMod(float modValue, float value)
        {
            return value * (1f + modValue);
        }

        internal static void UpdatePetResistance(this NPC pet, DamageSchool school)
        {
            int num;
            if (school == DamageSchool.Physical)
            {
                num = (pet.Armor * PetMgr.PetArmorOfOwnerPercent + 50) / 100;
                PetLevelStatInfo petLevelStatInfo = pet.Entry.GetPetLevelStatInfo(pet.Level);
                if (petLevelStatInfo != null)
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
            if (UnitUpdates.FlatIntModHandlers[(int) mod] == null)
                return;
            UnitUpdates.FlatIntModHandlers[(int) mod](unit);
        }

        /// <summary>Changes a multiplier modifier</summary>
        /// <param name="unit"></param>
        /// <param name="mod"></param>
        /// <param name="delta"></param>
        public static void ChangeModifier(this Unit unit, StatModifierFloat mod, float delta)
        {
            unit.FloatMods[(int) mod] += delta;
            if (UnitUpdates.MultiModHandlers[(int) mod] == null)
                return;
            UnitUpdates.MultiModHandlers[(int) mod](unit);
        }

        private delegate void UpdateHandler(Unit unit);
    }
}