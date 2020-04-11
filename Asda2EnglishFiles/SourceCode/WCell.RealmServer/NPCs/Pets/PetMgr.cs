using NLog;
using System;
using System.Linq;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Core.DBC;
using WCell.RealmServer.Content;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.NPCs.Pets
{
    public static class PetMgr
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Whether players are allowed to rename pets infinitely many times
        /// </summary>
        public static bool InfinitePetRenames = false;

        public static int MinPetNameLength = 3;
        public static int MaxPetNameLength = 12;
        public static int MaxFeedPetHappinessGain = 33000;

        /// <summary>Need at least level 20 for a pet to have talents</summary>
        public static int MinPetTalentLevel = 20;

        /// <summary>
        /// Hunter pets must not have less than ownerlevel - MaxMinionLevelDifference.
        /// They gain levels if they have less.
        /// </summary>
        public static readonly int MaxHunterPetLevelDifference = 5;

        /// <summary>Percentage of character exp that pets need to level.</summary>
        public static readonly int PetExperienceOfOwnerPercent = 5;

        /// <summary>
        /// Percentage of character Stamina that gets added to the Pet's Stamina.
        /// </summary>
        public static readonly int PetStaminaOfOwnerPercent = 45;

        /// <summary>
        /// Percentage of character Armor that gets added to the Pet's Armor.
        /// </summary>
        public static readonly int PetArmorOfOwnerPercent = 35;

        /// <summary>
        /// Percentage of character RangedAttackPower that gets added to the Pet's MeleeAttackPower.
        /// </summary>
        public static readonly int PetAPOfOwnerPercent = 22;

        /// <summary>
        /// Percentage of character Resistances that get added to the Pet's Resistances.
        /// </summary>
        public static readonly int PetResistanceOfOwnerPercent = 40;

        /// <summary>
        /// Percentage of character Spell Damage that gets added to the Pet's Resistances.
        /// </summary>
        public static readonly int PetSpellDamageOfOwnerPercent = 13;

        internal static readonly NHIdGenerator PetNumberGenerator =
            new NHIdGenerator(typeof(PermanentPetRecord), "m_PetNumber", 1L);

        public const int MaxTotemSlots = 4;
        [NotVariable] public static int MaxStableSlots;
        [NotVariable] public static uint[] StableSlotPrices;

        public static void Init()
        {
            PetMgr.InitMisc();
        }

        public static void InitEntries()
        {
            ContentMgr.Load<PetLevelStatInfo>();
        }

        private static void InitMisc()
        {
            PetMgr.StableSlotPrices =
                new ListDBCReader<uint, DBCStableSlotPriceConverter>(
                    RealmServerConfiguration.GetDBCFile("StableSlotPrices.dbc")).EntryList.ToArray();
            PetMgr.MaxStableSlots = PetMgr.StableSlotPrices.Length;
        }

        public static PetNameInvalidReason ValidatePetName(ref string petName)
        {
            if (petName.Length == 0)
                return PetNameInvalidReason.NoName;
            if (petName.Length < PetMgr.MinPetNameLength)
                return PetNameInvalidReason.TooShort;
            if (petName.Length > PetMgr.MaxPetNameLength)
                return PetNameInvalidReason.TooLong;
            if (PetMgr.DoesNameViolate(petName))
                return PetNameInvalidReason.Profane;
            int num1 = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            char ch1 = '1';
            char ch2 = '0';
            for (int index = 0; index < petName.Length; ++index)
            {
                char c = petName[index];
                if (!char.IsLetter(c))
                {
                    switch (c)
                    {
                        case ' ':
                            ++num1;
                            break;
                        case '\'':
                            ++num2;
                            break;
                        default:
                            if (char.IsDigit(c))
                            {
                                ++num4;
                                break;
                            }

                            ++num5;
                            break;
                    }
                }
                else
                {
                    if ((int) c == (int) ch1 && (int) c == (int) ch2)
                        ++num6;
                    ch2 = ch1;
                    ch1 = c;
                }

                if (char.IsUpper(c))
                    ++num3;
            }

            if (num6 > 0)
                return PetNameInvalidReason.ThreeConsecutive;
            if (num1 > 0)
                return PetNameInvalidReason.ConsecutiveSpaces;
            if (num4 > 0 || num5 > 0 || num2 > 1)
                return PetNameInvalidReason.Invalid;
            if (num2 == 1)
            {
                int num7 = petName.IndexOf("'");
                if (num7 == 0 || num7 == petName.Length - 1)
                    return PetNameInvalidReason.Invalid;
            }

            if (RealmServerConfiguration.CapitalizeCharacterNames)
                petName = petName.ToCapitalizedString();
            return PetNameInvalidReason.Ok;
        }

        private static bool DoesNameViolate(string petName)
        {
            petName = petName.ToLower();
            return RealmServerConfiguration.BadWords
                .Where<string>((Func<string, bool>) (word => petName.Contains(word))).Any<string>();
        }

        public static uint GetStableSlotPrice(int slot)
        {
            if (slot > PetMgr.StableSlotPrices.Length)
                return PetMgr.StableSlotPrices[PetMgr.StableSlotPrices.Length - 1];
            return PetMgr.StableSlotPrices[slot];
        }

        public static void DeStablePet(Character chr, NPC stableMaster, uint petNumber)
        {
            if (!PetMgr.CheckForStableMasterCheats(chr, stableMaster))
                return;
            PermanentPetRecord stabledPet = chr.GetStabledPet(petNumber);
            chr.DeStablePet(stabledPet);
            PetHandler.SendStableResult((IPacketReceiver) chr, StableResult.DeStableSuccess);
        }

        public static void StablePet(Character chr, NPC stableMaster)
        {
            if (!PetMgr.CheckForStableMasterCheats(chr, stableMaster))
                return;
            NPC activePet = chr.ActivePet;
            if (!chr.GodMode && activePet.Health == 0)
                PetHandler.SendStableResult((IPacketReceiver) chr, StableResult.Fail);
            if (chr.StabledPetRecords.Count < chr.StableSlotCount)
            {
                chr.StablePet();
                PetHandler.SendStableResult((IPacketReceiver) chr, StableResult.StableSuccess);
            }
            else
                PetHandler.SendStableResult((IPacketReceiver) chr, StableResult.Fail);
        }

        public static void SwapStabledPet(Character chr, NPC stableMaster, uint petNumber)
        {
            if (!PetMgr.CheckForStableMasterCheats(chr, stableMaster))
                return;
            NPC activePet = chr.ActivePet;
            PermanentPetRecord stabledPet = chr.GetStabledPet(petNumber);
            if (activePet.Health == 0)
                PetHandler.SendStableResult((IPacketReceiver) chr, StableResult.Fail);
            else if (!chr.TrySwapStabledPet(stabledPet))
                PetHandler.SendStableResult((IPacketReceiver) chr, StableResult.Fail);
            else
                PetHandler.SendStableResult((IPacketReceiver) chr, StableResult.DeStableSuccess);
        }

        public static void BuyStableSlot(Character chr, NPC stableMaster)
        {
            if (!PetMgr.CheckForStableMasterCheats(chr, stableMaster))
                return;
            if (!chr.TryBuyStableSlot())
                PetHandler.SendStableResult((IPacketReceiver) chr.Client, StableResult.NotEnoughMoney);
            else
                PetHandler.SendStableResult((IPacketReceiver) chr.Client, StableResult.BuySlotSuccess);
        }

        public static void ListStabledPets(Character chr, NPC stableMaster)
        {
            if (!PetMgr.CheckForStableMasterCheats(chr, stableMaster))
                return;
            PetHandler.SendStabledPetsList((IPacketReceiver) chr, (Unit) stableMaster, (byte) chr.StableSlotCount,
                chr.StabledPetRecords);
        }

        /// <summary>Checks StableMaster interactions for cheating.</summary>
        /// <param name="chr">The character doing the interacting.</param>
        /// <param name="stableMaster">The StableMaster the character is interacting with.</param>
        /// <returns>True if the interaction checks out.</returns>
        private static bool CheckForStableMasterCheats(Character chr, NPC stableMaster)
        {
            if (chr == null)
                return false;
            if (chr.GodMode)
                return true;
            if (!chr.IsAlive)
                return false;
            if (stableMaster == null || !stableMaster.IsStableMaster)
            {
                PetMgr.log.Warn("Character \"{0}\" requested retreival of stabled pet from invalid NPC: {1}",
                    (object) chr, (object) stableMaster);
                return false;
            }

            if (!stableMaster.CheckVendorInteraction(chr))
                return false;
            chr.Auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);
            return true;
        }

        /// <summary>
        /// Calculates the number of base Talent points a pet should have
        ///   based on its level.
        /// </summary>
        /// <param name="level">The pet's level.</param>
        /// <returns>The number of pet talent points.</returns>
        public static int GetPetTalentPointsByLevel(int level)
        {
            if (level > 19)
                return (level - 20) / 4 + 1;
            return 0;
        }

        internal static PermanentPetRecord CreatePermanentPetRecord(NPCEntry entry, uint ownerId)
        {
            PermanentPetRecord defaultPetRecord = PetMgr.CreateDefaultPetRecord<PermanentPetRecord>(entry, ownerId);
            defaultPetRecord.PetNumber = (uint) PetMgr.PetNumberGenerator.Next();
            defaultPetRecord.IsDirty = true;
            return defaultPetRecord;
        }

        internal static T CreateDefaultPetRecord<T>(NPCEntry entry, uint ownerId) where T : IPetRecord, new()
        {
            T obj = new T();
            PetAttackMode petAttackMode = entry.Type == CreatureType.NonCombatPet
                ? PetAttackMode.Passive
                : PetAttackMode.Defensive;
            obj.OwnerId = ownerId;
            obj.AttackMode = petAttackMode;
            obj.Flags = PetFlags.None;
            obj.EntryId = entry.NPCId;
            return obj;
        }
    }
}