using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using WCell.Constants;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.DBC;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.Skills
{
    public static class SkillHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The max amount of professions that every player may learn (Blizzlike: 2)
        /// </summary>
        public static uint MaxProfessionsPerChar = 2;

        /// <summary>
        /// Whether to automatically remove all spells that belong to a skill when removing it.
        /// </summary>
        public static bool RemoveAbilitiesWithSkill = true;

        /// <summary>All skills, indexed by their id</summary>
        public static readonly SkillLine[] ById = new SkillLine[2000];

        /// <summary>
        /// All lists of all Race/Class-specific skillinfos: Use RaceClassInfos[race][class]
        /// </summary>
        public static readonly Dictionary<SkillId, SkillRaceClassInfo>[][] RaceClassInfos =
            new Dictionary<SkillId, SkillRaceClassInfo>[WCellConstants.RaceTypeLength][];

        /// <summary>All SkillAbility-lists, indexed by their SkillId</summary>
        public static readonly SkillAbility[][] AbilitiesBySkill = new SkillAbility[2000][];

        public const uint MaxSkillId = 2000;

        /// <summary>
        /// The maximum amount of skills allowed (limited by the amount of PlayerUpdateFields for skills)
        /// </summary>
        public const int MaxAmount = 128;

        public const PlayerFields HighestField = PlayerFields.CHARACTER_POINTS1;
        private static MappedDBCReader<SkillTiers, SkillHandler.SkillTierConverter> TierReader;
        private static MappedDBCReader<SkillRaceClassInfo, SkillHandler.SkillRaceClassInfoConverter> RaceClassReader;

        internal static void Initialize()
        {
            SkillHandler.TierReader =
                new MappedDBCReader<SkillTiers, SkillHandler.SkillTierConverter>(
                    RealmServerConfiguration.GetDBCFile("SkillTiers.dbc"));
            foreach (SkillLine skillLine in new MappedDBCReader<SkillLine, SkillHandler.SkillLineConverter>(
                RealmServerConfiguration.GetDBCFile("SkillLine.dbc")).Entries.Values)
            {
                SkillHandler.ById[(uint) skillLine.Id] = skillLine;
                if (skillLine.Category == SkillCategory.Language)
                {
                    LanguageDescription languageDescBySkillType =
                        LanguageHandler.GetLanguageDescBySkillType(skillLine.Id);
                    if (languageDescBySkillType != null)
                        skillLine.Language = languageDescBySkillType.Language;
                }
            }

            SkillHandler.RaceClassReader =
                new MappedDBCReader<SkillRaceClassInfo, SkillHandler.SkillRaceClassInfoConverter>(
                    RealmServerConfiguration.GetDBCFile("SkillRaceClassInfo.dbc"));
            MappedDBCReader<SkillAbility, SkillHandler.SkillAbilityConverter> mappedDbcReader =
                new MappedDBCReader<SkillAbility, SkillHandler.SkillAbilityConverter>(
                    RealmServerConfiguration.GetDBCFile("SkillLineAbility.dbc"));
            List<SkillAbility>[] arr = new List<SkillAbility>[2000];
            foreach (SkillAbility skillAbility in mappedDbcReader.Entries.Values)
            {
                if (skillAbility.Spell != null)
                    skillAbility.Spell.Ability = skillAbility;
                if (skillAbility.NextSpellId > SpellId.None)
                {
                    Spell spell = SpellHandler.Get(skillAbility.NextSpellId);
                    if (spell != null)
                    {
                        skillAbility.NextAbility = spell.Ability;
                        if (spell.Ability != null)
                            spell.Ability.PreviousAbility = skillAbility;
                    }
                }

                List<SkillAbility> val = arr.Get<List<SkillAbility>>((uint) skillAbility.Skill.Id);
                if (val == null)
                {
                    val = new List<SkillAbility>();
                    ArrayUtil.Set<List<SkillAbility>>(ref arr, (uint) skillAbility.Skill.Id, val);
                }

                val.Add(skillAbility);
            }

            for (int index = 0; index < arr.Length; ++index)
            {
                if (arr[index] != null)
                    SkillHandler.AbilitiesBySkill[index] = arr[index].ToArray();
            }
        }

        public static void Initialize2()
        {
            foreach (SkillAbility[] skillAbilityArray in SkillHandler.AbilitiesBySkill)
            {
                if (skillAbilityArray != null)
                {
                    foreach (SkillAbility skillAbility in skillAbilityArray)
                    {
                        if ((skillAbility.Skill.Category == SkillCategory.Profession ||
                             skillAbility.Skill.Category == SkillCategory.SecondarySkill) &&
                            skillAbility.Spell.HasEffect(SpellEffectType.Skill))
                            skillAbility.Skill.TeachingSpells.Add(skillAbility.Spell);
                    }
                }
            }

            foreach (SkillAbility[] skillAbilityArray in SkillHandler.AbilitiesBySkill)
            {
                if (skillAbilityArray != null)
                {
                    foreach (SkillAbility skillAbility in skillAbilityArray)
                    {
                        if ((skillAbility.Skill.Category == SkillCategory.Profession ||
                             skillAbility.Skill.Category == SkillCategory.SecondarySkill) &&
                            (skillAbility.AcquireMethod == SkillAcquireMethod.OnLearningSkill &&
                             skillAbility.Spell.BaseLevel == 0) && skillAbility.Spell.Rank == 0)
                        {
                            Spell spellForTier = skillAbility.Skill.GetSpellForTier(SkillTierId.Apprentice);
                            if (spellForTier != null)
                                spellForTier.AdditionallyTaughtSpells.Add(skillAbility.Spell);
                        }
                    }
                }
            }
        }

        public static SkillLine Get(SkillId id)
        {
            if ((long) id >= (long) SkillHandler.ById.Length)
                return (SkillLine) null;
            return SkillHandler.ById[(uint) id];
        }

        public static SkillLine Get(uint id)
        {
            if ((long) id >= (long) SkillHandler.ById.Length)
                return (SkillLine) null;
            return SkillHandler.ById[id];
        }

        public static SkillAbility[] GetAbilities(SkillId id)
        {
            if ((long) id >= (long) SkillHandler.AbilitiesBySkill.Length)
                return (SkillAbility[]) null;
            return SkillHandler.AbilitiesBySkill[(uint) id];
        }

        public static SkillAbility[] GetAbilities(uint id)
        {
            if ((long) id >= (long) SkillHandler.AbilitiesBySkill.Length)
                return (SkillAbility[]) null;
            return SkillHandler.AbilitiesBySkill[id];
        }

        public static SkillAbility GetAbility(SkillId skill, SpellId spell)
        {
            return Array.Find<SkillAbility>(SkillHandler.GetAbilities(skill),
                (Predicate<SkillAbility>) (ability => ability.Spell.SpellId == spell));
        }

        public static SkillId GetSkill(SkinningType skinType)
        {
            switch (skinType)
            {
                case SkinningType.Skinning:
                    return SkillId.Skinning;
                case SkinningType.Herbalism:
                    return SkillId.Herbalism;
                case SkinningType.Mining:
                    return SkillId.Mining;
                case SkinningType.Engineering:
                    return SkillId.Engineering;
                default:
                    return SkillId.Skinning;
            }
        }

        public static void HandleUnlearnSkill(IRealmClient client, RealmPacketIn packet)
        {
            uint num = packet.ReadUInt32();
            client.ActiveCharacter.Skills.Remove((SkillId) num);
        }

        public class SkillLineConverter : AdvancedDBCRecordConverter<SkillLine>
        {
            public override SkillLine ConvertTo(byte[] rawData, ref int id)
            {
                SkillLine line = new SkillLine();
                int offset = 0;
                id = (int) (line.Id = (SkillId) DBCRecordConverter.GetUInt32(rawData, offset++));
                line.Category = (SkillCategory) DBCRecordConverter.GetInt32(rawData, offset++);
                line.SkillCostsDataId = DBCRecordConverter.GetInt32(rawData, offset++);
                line.Name = base.GetString(rawData, ref offset);
                base.GetString(rawData, ref offset);
                DBCRecordConverter.GetInt32(rawData, offset++);
                base.GetString(rawData, ref offset);
                DBCRecordConverter.GetInt32(rawData, offset);
                if (line.Category == SkillCategory.Profession)
                {
                    line.Abandonable = 1;
                }

                return line;
            }
        }

        public class SkillAbilityConverter : AdvancedDBCRecordConverter<SkillAbility>
        {
            public override SkillAbility ConvertTo(byte[] rawData, ref int id)
            {
                SkillAbility ability = new SkillAbility();
                int field = 0;
                id = (int) (ability.AbilityId = DBCRecordConverter.GetUInt32(rawData, field++));
                ability.Skill = SkillHandler.ById[DBCRecordConverter.GetUInt32(rawData, field++)];
                SpellId spellId = (SpellId) DBCRecordConverter.GetUInt32(rawData, field++);
                if (spellId > SpellId.None)
                {
                    Spell spell = SpellHandler.Get(spellId);
                    if (spell != null)
                    {
                        ability.Spell = spell;
                    }
                }

                ability.RaceMask = (RaceMask) DBCRecordConverter.GetUInt32(rawData, field++);
                ability.ClassMask = (ClassMask) DBCRecordConverter.GetUInt32(rawData, field++);
                DBCRecordConverter.GetUInt32(rawData, field++);
                DBCRecordConverter.GetUInt32(rawData, field++);
                DBCRecordConverter.GetInt32(rawData, field++);
                ability.NextSpellId = (SpellId) DBCRecordConverter.GetUInt32(rawData, field++);
                ability.AcquireMethod = (SkillAcquireMethod) DBCRecordConverter.GetInt32(rawData, field++);
                ability.GreyValue = DBCRecordConverter.GetUInt32(rawData, field++);
                ability.YellowValue = DBCRecordConverter.GetUInt32(rawData, field);
                uint num2 = ability.GreyValue - ability.YellowValue;
                int num3 = (int) (ability.YellowValue - (num2 / 2));
                ability.RedValue = (num3 < 0) ? 0 : ((uint) num3);
                ability.GreenValue = ability.YellowValue + (num2 / 2);
                ability.CanGainSkill = ability.GreenValue > 0;
                return ability;
            }
        }

        public class SkillTierConverter : AdvancedDBCRecordConverter<SkillTiers>
        {
            public override SkillTiers ConvertTo(byte[] rawData, ref int id)
            {
                SkillTiers result = default(SkillTiers);
                int num = 0;
                id = (int) (result.Id = DBCRecordConverter.GetUInt32(rawData, num++));
                result.Id = (uint) id;
                uint[] array = new uint[16];
                uint[] array2 = new uint[16];
                for (int j = 0; j < 16; j++)
                {
                    array[j] = DBCRecordConverter.GetUInt32(rawData, num + j);
                    array2[j] = DBCRecordConverter.GetUInt32(rawData, num + j + 16);
                }

                result.MaxValues = (from i in array2
                    where i != 0u
                    select i).ToArray<uint>();
                result.Costs = array.Take(result.MaxValues.Length).ToArray<uint>();
                return result;
            }
        }

        public class SkillRaceClassInfoConverter : AdvancedDBCRecordConverter<SkillRaceClassInfo>
        {
            public override SkillRaceClassInfo ConvertTo(byte[] rawData, ref int id)
            {
                id = DBCRecordConverter.GetInt32(rawData, 0);
                int num1 = 0;
                SkillRaceClassInfo skillRaceClassInfo1 = new SkillRaceClassInfo();
                SkillRaceClassInfo skillRaceClassInfo2 = skillRaceClassInfo1;
                byte[] data1 = rawData;
                int field1 = num1;
                int num2 = field1 + 1;
                int uint32_1 = (int) DBCRecordConverter.GetUInt32(data1, field1);
                skillRaceClassInfo2.Id = (uint) uint32_1;
                byte[] data2 = rawData;
                int field2 = num2;
                int num3 = field2 + 1;
                SkillId uint32_2 = (SkillId) DBCRecordConverter.GetUInt32(data2, field2);
                SkillRaceClassInfo skillRaceClassInfo3 = skillRaceClassInfo1;
                byte[] data3 = rawData;
                int field3 = num3;
                int num4 = field3 + 1;
                int uint32_3 = (int) DBCRecordConverter.GetUInt32(data3, field3);
                skillRaceClassInfo3.RaceMask = (RaceMask) uint32_3;
                SkillRaceClassInfo skillRaceClassInfo4 = skillRaceClassInfo1;
                byte[] data4 = rawData;
                int field4 = num4;
                int num5 = field4 + 1;
                int uint32_4 = (int) DBCRecordConverter.GetUInt32(data4, field4);
                skillRaceClassInfo4.ClassMask = (ClassMask) uint32_4;
                SkillRaceClassInfo skillRaceClassInfo5 = skillRaceClassInfo1;
                byte[] data5 = rawData;
                int field5 = num5;
                int num6 = field5 + 1;
                int uint32_5 = (int) DBCRecordConverter.GetUInt32(data5, field5);
                skillRaceClassInfo5.Flags = (SkillRaceClassFlags) uint32_5;
                SkillRaceClassInfo skillRaceClassInfo6 = skillRaceClassInfo1;
                byte[] data6 = rawData;
                int field6 = num6;
                int num7 = field6 + 1;
                int uint32_6 = (int) DBCRecordConverter.GetUInt32(data6, field6);
                skillRaceClassInfo6.MinimumLevel = (uint) uint32_6;
                byte[] data7 = rawData;
                int field7 = num7;
                int field8 = field7 + 1;
                int int32 = DBCRecordConverter.GetInt32(data7, field7);
                if (int32 > 0)
                    SkillHandler.TierReader.Entries.TryGetValue(int32, out skillRaceClassInfo1.Tiers);
                skillRaceClassInfo1.SkillCostIndex = DBCRecordConverter.GetUInt32(rawData, field8);
                skillRaceClassInfo1.SkillLine = SkillHandler.ById.Get<SkillLine>((uint) uint32_2);
                if (skillRaceClassInfo1.SkillLine != null)
                {
                    foreach (ClassId allClassId in WCellConstants.AllClassIds)
                    {
                        if (allClassId < ClassId.End)
                        {
                            ClassMask mask = allClassId.ToMask();
                            foreach (RaceMask key in WCellConstants.RaceTypesByMask.Keys)
                            {
                                RaceId raceType = WCellConstants.GetRaceType(key);
                                if (skillRaceClassInfo1.RaceMask.HasAnyFlag(key) &&
                                    skillRaceClassInfo1.ClassMask.HasAnyFlag(mask))
                                {
                                    Dictionary<SkillId, SkillRaceClassInfo>[] dictionaryArray =
                                        SkillHandler.RaceClassInfos[(int) raceType];
                                    if (dictionaryArray == null)
                                        SkillHandler.RaceClassInfos[(int) raceType] = dictionaryArray =
                                            new Dictionary<SkillId, SkillRaceClassInfo>[WCellConstants.ClassTypeLength];
                                    Dictionary<SkillId, SkillRaceClassInfo> dictionary =
                                        dictionaryArray[(int) allClassId];
                                    if (dictionary == null)
                                        dictionaryArray[(int) allClassId] =
                                            dictionary = new Dictionary<SkillId, SkillRaceClassInfo>();
                                    SkillRaceClassInfo skillRaceClassInfo7;
                                    if (dictionary.TryGetValue(uint32_2, out skillRaceClassInfo7))
                                    {
                                        skillRaceClassInfo1.RaceMask |= skillRaceClassInfo7.RaceMask;
                                        skillRaceClassInfo1.ClassMask |= skillRaceClassInfo7.ClassMask;
                                    }
                                    else if (skillRaceClassInfo1.SkillLine.Tiers.Id == 0U &&
                                             skillRaceClassInfo1.Tiers.Id != 0U)
                                        skillRaceClassInfo1.SkillLine.Tiers = skillRaceClassInfo1.Tiers;

                                    dictionary[uint32_2] = skillRaceClassInfo1;
                                }
                            }
                        }
                    }
                }

                return skillRaceClassInfo1;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct SkillCostsData
        {
        }

        public class SkillCostsDataConverter : AdvancedDBCRecordConverter<SkillHandler.SkillCostsData>
        {
            public override SkillHandler.SkillCostsData ConvertTo(byte[] rawData, ref int id)
            {
                return base.ConvertTo(rawData, ref id);
            }
        }
    }
}