using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Auras.Effects;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Auras.Mod;
using WCell.RealmServer.Spells.Auras.Passive;
using WCell.RealmServer.Spells.Auras.Periodic;
using WCell.Util;

namespace WCell.RealmServer.Spells.Auras
{
    /// <summary>
    /// Static Aura helper class
    /// 
    /// TODO: CMSG_PET_CANCEL_AURA
    /// TODO: CMSG_CANCEL_GROWTH_AURA unused?
    /// </summary>
    public static class AuraHandler
    {
        public static readonly AuraEffectHandlerCreator[] EffectHandlers =
            new AuraEffectHandlerCreator
                [(int) Convert.ChangeType((object) Utility.GetMaxEnum<AuraType>(), typeof(int)) + 1];

        /// <summary>
        /// Every Aura that is evaluated to true is only stackable with Auras that are also evaluated to true by the same evaluator.
        /// Spells that are not covered by any evaluator have no restrictions.
        /// </summary>
        public static readonly List<AuraIdEvaluator> AuraIdEvaluators = new List<AuraIdEvaluator>();

        /// <summary>The maximum amount of positive Auras</summary>
        public const int MaxPositiveAuras = 28;

        /// <summary>The maximum amount of any kind of Auras</summary>
        public const int MaxAuras = 56;

        internal static uint lastAuraUid;

        /// <summary>
        /// Used to make sure that certain auras can be applied multiple times
        /// </summary>
        internal static uint randomAuraId;

        /// <summary>
        /// Cancels a positive aura (by right-clicking on the corresponding icon)
        /// </summary>
        public static void HandleCancelCastSpell(IRealmClient client, RealmPacketIn packet)
        {
            Spell index = SpellHandler.Get((SpellId) packet.ReadUInt32());
            if (index == null)
                return;
            Aura aura = client.ActiveCharacter.Auras[index, true];
            if (aura == null || !aura.CanBeRemoved)
                return;
            aura.TryRemove(true);
        }

        public static void SendAuraUpdate(Unit owner, Aura aura)
        {
            if (owner is NPC)
            {
                Asda2CombatHandler.SendNpcBuffedResponse((NPC) owner, aura);
            }
            else
            {
                if (!(owner is Character))
                    return;
                Character character = (Character) owner;
                if (!character.CanMove)
                    Asda2MovmentHandler.SendEndMoveByFastInstantRegularMoveResponse(character);
                Asda2SpellHandler.SendCharacterBuffedResponse(character, aura);
                if (character.IsInGroup)
                    Asda2GroupHandler.SendPartyMemberBuffInfoResponse(character);
                if (character.SoulmateCharacter == null)
                    return;
                Asda2SoulmateHandler.SendSoulmateBuffUpdateInfoResponse(character);
            }
        }

        public static void SendAllAuras(IPacketReceiver rcv, Unit owner)
        {
        }

        public static void SendAllAuras(Unit owner)
        {
        }

        public static RealmPacketOut CreateAllAuraPacket(Unit owner)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.SMSG_AURA_UPDATE_ALL);
            owner.EntityId.WritePacked((BinaryWriter) realmPacketOut);
            foreach (Aura aura in owner.Auras)
            {
                if (aura.IsVisible)
                    AuraHandler.WriteAura(aura, (BinaryWriter) realmPacketOut);
            }

            return realmPacketOut;
        }

        private static void WriteAura(Aura aura, BinaryWriter packet)
        {
            packet.Write(aura.Index);
            packet.Write(aura.Spell.Id);
            packet.Write((byte) aura.Flags);
            packet.Write(aura.Level);
            packet.Write((byte) aura.StackCount);
            if (!aura.Flags.HasFlag((Enum) AuraFlags.TargetIsCaster))
                aura.CasterReference.EntityId.WritePacked(packet);
            if (!aura.Flags.HasFlag((Enum) AuraFlags.HasDuration))
                return;
            packet.Write(aura.Duration);
            packet.Write(aura.TimeLeft);
        }

        /// <summary>Sends updates to the client for spell-modifier</summary>
        public static void SendModifierUpdate(Character chr, SpellEffect effect, bool isPercent)
        {
            SpellModifierType miscValue = (SpellModifierType) effect.MiscValue;
            List<AddModifierEffectHandler> modifierEffectHandlerList =
                isPercent ? chr.PlayerAuras.SpellModifiersPct : chr.PlayerAuras.SpellModifiersFlat;
            foreach (uint affectMaskBit in effect.AffectMaskBitSet)
            {
                int amount = 0;
                uint num1 = affectMaskBit >> 5;
                uint num2 = affectMaskBit - (num1 << 5);
                for (int index = 0; index < modifierEffectHandlerList.Count; ++index)
                {
                    AddModifierEffectHandler modifierEffectHandler = modifierEffectHandlerList[index];
                    if ((SpellModifierType) modifierEffectHandler.SpellEffect.MiscValue == miscValue &&
                        modifierEffectHandler.SpellEffect.Spell.SpellClassSet == effect.Spell.SpellClassSet &&
                        ((long) modifierEffectHandler.SpellEffect.AffectMask[num1] & (long) (1 << (int) num2)) != 0L)
                        amount += modifierEffectHandler.SpellEffect.ValueMin;
                }

                SpellHandler.SendSpellModifier(chr, (byte) affectMaskBit, miscValue, amount, isPercent);
            }
        }

        static AuraHandler()
        {
            AuraHandler.EffectHandlers[0] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new AuraVoidHandler());
            AuraHandler.EffectHandlers[2] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModPossessAuraHandler());
            AuraHandler.EffectHandlers[3] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new PeriodicDamageHandler());
            AuraHandler.EffectHandlers[4] = (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new DummyHandler());
            AuraHandler.EffectHandlers[5] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModConfuseHandler());
            AuraHandler.EffectHandlers[6] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new CharmAuraHandler());
            AuraHandler.EffectHandlers[7] = (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new FearHandler());
            AuraHandler.EffectHandlers[8] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new PeriodicHealHandler());
            AuraHandler.EffectHandlers[9] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModAttackSpeedHandler());
            AuraHandler.EffectHandlers[10] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModThreatHandler());
            AuraHandler.EffectHandlers[11] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModTauntAuraHandler());
            AuraHandler.EffectHandlers[12] = (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new StunHandler());
            AuraHandler.EffectHandlers[13] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModDamageDoneHandler());
            AuraHandler.EffectHandlers[14] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModDamageTakenHandler());
            AuraHandler.EffectHandlers[15] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new DamageShieldEffectHandler());
            AuraHandler.EffectHandlers[16] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModStealthHandler());
            AuraHandler.EffectHandlers[18] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModInvisibilityHandler());
            AuraHandler.EffectHandlers[20] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new RegenPercentOfTotalHealthHandler());
            AuraHandler.EffectHandlers[21] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new RegenPercentOfTotalManaHandler());
            AuraHandler.EffectHandlers[22] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModResistanceHandler());
            AuraHandler.EffectHandlers[23] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new PeriodicTriggerSpellHandler());
            AuraHandler.EffectHandlers[24] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new PeriodicEnergizeHandler());
            AuraHandler.EffectHandlers[25] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModPacifyHandler());
            AuraHandler.EffectHandlers[26] = (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new RootHandler());
            AuraHandler.EffectHandlers[27] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModSilenceHandler());
            AuraHandler.EffectHandlers[29] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModStatHandler());
            AuraHandler.EffectHandlers[31] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModIncreaseSpeedHandler());
            AuraHandler.EffectHandlers[32] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModIncreaseMountedSpeedHandler());
            AuraHandler.EffectHandlers[33] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModDecreaseSpeedHandler());
            AuraHandler.EffectHandlers[34] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModIncreaseHealthHandler());
            AuraHandler.EffectHandlers[35] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModIncreaseEnergyHandler());
            AuraHandler.EffectHandlers[36] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ShapeshiftHandler());
            AuraHandler.EffectHandlers[38] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new StateImmunityHandler());
            AuraHandler.EffectHandlers[39] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new SchoolImmunityHandler());
            AuraHandler.EffectHandlers[40] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new DamageImmunityHandler());
            AuraHandler.EffectHandlers[41] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new DispelImmunityHandler());
            AuraHandler.EffectHandlers[42] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ProcTriggerSpellHandler());
            AuraHandler.EffectHandlers[43] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ProcTriggerDamageHandler());
            AuraHandler.EffectHandlers[44] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new TrackCreaturesHandler());
            AuraHandler.EffectHandlers[45] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new TrackResourcesHandler());
            AuraHandler.EffectHandlers[47] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModParryPercentHandler());
            AuraHandler.EffectHandlers[49] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModDodgePercentHandler());
            AuraHandler.EffectHandlers[50] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModCritHealValuePctHandler());
            AuraHandler.EffectHandlers[51] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModBlockPercentHandler());
            AuraHandler.EffectHandlers[52] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModCritPercentHandler());
            AuraHandler.EffectHandlers[53] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new PeriodicLeechHandler());
            AuraHandler.EffectHandlers[54] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModHitChanceHandler());
            AuraHandler.EffectHandlers[55] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModSpellHitChanceHandler());
            AuraHandler.EffectHandlers[56] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new TransformHandler());
            AuraHandler.EffectHandlers[57] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModSpellCritChanceHandler());
            AuraHandler.EffectHandlers[58] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModIncreaseSwimSpeedHandler());
            AuraHandler.EffectHandlers[60] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModPacifySilenceHandler());
            AuraHandler.EffectHandlers[61] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModScaleHandler());
            AuraHandler.EffectHandlers[62] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new PeriodicHealthFunnelHandler());
            AuraHandler.EffectHandlers[64] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new PeriodicManaLeechHandler());
            AuraHandler.EffectHandlers[65] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModCastingSpeedHandler());
            AuraHandler.EffectHandlers[67] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new DisarmMainHandHandler());
            AuraHandler.EffectHandlers[69] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new SchoolAbsorbHandler());
            AuraHandler.EffectHandlers[71] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModSpellCritChanceForSchoolHandler());
            AuraHandler.EffectHandlers[72] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModPowerCostHandler());
            AuraHandler.EffectHandlers[73] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModPowerCostForSchoolHandler());
            AuraHandler.EffectHandlers[75] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModLanguageHandler());
            AuraHandler.EffectHandlers[77] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new MechanicImmunityHandler());
            AuraHandler.EffectHandlers[78] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new MountedHandler());
            AuraHandler.EffectHandlers[79] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModDamageDonePercentHandler());
            AuraHandler.EffectHandlers[80] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModStatPercentHandler());
            AuraHandler.EffectHandlers[81] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new SplitDamageHandler());
            AuraHandler.EffectHandlers[85] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModPowerRegenHandler());
            AuraHandler.EffectHandlers[86] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new CreateItemOnTargetDeathHandler());
            AuraHandler.EffectHandlers[87] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModDamageTakenPercentHandler());
            AuraHandler.EffectHandlers[89] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new PeriodicDamagePercentHandler());
            AuraHandler.EffectHandlers[91] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModDetectRangeHandler());
            AuraHandler.EffectHandlers[93] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new UnattackableHandler());
            AuraHandler.EffectHandlers[94] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new InterruptRegenHandler());
            AuraHandler.EffectHandlers[95] = (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new GhostHandler());
            AuraHandler.EffectHandlers[97] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ManaShieldHandler());
            AuraHandler.EffectHandlers[98] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModSkillTalentHandler());
            AuraHandler.EffectHandlers[99] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModMeleeAttackPowerHandler());
            AuraHandler.EffectHandlers[101] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModResistancePctHandler());
            AuraHandler.EffectHandlers[104] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new WaterWalkHandler());
            AuraHandler.EffectHandlers[105] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new FeatherFallHandler());
            AuraHandler.EffectHandlers[106] = (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new HoverHandler());
            AuraHandler.EffectHandlers[107] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new AddModifierFlatHandler());
            AuraHandler.EffectHandlers[108] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new AddModifierPercentHandler());
            AuraHandler.EffectHandlers[109] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new AddTargetTriggerHandler());
            AuraHandler.EffectHandlers[110] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModPowerRegenPercentHandler());
            AuraHandler.EffectHandlers[111] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new AddCasterHitTriggerHandler());
            AuraHandler.EffectHandlers[117] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModMechanicResistanceHandler());
            AuraHandler.EffectHandlers[118] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModHealingTakenPctHandler());
            AuraHandler.EffectHandlers[120] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new UntrackableHandler());
            AuraHandler.EffectHandlers[122] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModOffhandDamagePercentHandler());
            AuraHandler.EffectHandlers[123] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModTargetResistanceHandler());
            AuraHandler.EffectHandlers[124] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModRangedAttackPowerHandler());
            AuraHandler.EffectHandlers[129] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModIncreaseSpeedAlwaysHandler());
            AuraHandler.EffectHandlers[130] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModMountedSpeedAlwaysHandler());
            AuraHandler.EffectHandlers[132] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModIncreaseEnergyPercentHandler());
            AuraHandler.EffectHandlers[133] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModIncreaseHealthPercentHandler());
            AuraHandler.EffectHandlers[134] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModManaRegenInterruptHandler());
            AuraHandler.EffectHandlers[135] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModHealingDoneHandler());
            AuraHandler.EffectHandlers[136] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModHealingDonePctHandler());
            AuraHandler.EffectHandlers[137] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModTotalStatPercentHandler());
            AuraHandler.EffectHandlers[138] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModHasteHandler());
            AuraHandler.EffectHandlers[139] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ForceReactionHandler());
            AuraHandler.EffectHandlers[142] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModBaseResistancePercentHandler());
            AuraHandler.EffectHandlers[143] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModResistanceExclusiveHandler());
            AuraHandler.EffectHandlers[144] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new SafeFallHandler());
            AuraHandler.EffectHandlers[145] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModPetTalentPointsHandler());
            AuraHandler.EffectHandlers[146] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ControlExoticPetsHandler());
            AuraHandler.EffectHandlers[148] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new RetainComboPointsHandler());
            AuraHandler.EffectHandlers[149] = (AuraEffectHandlerCreator) (() =>
                (AuraEffectHandler) new ModResistSpellInterruptionPercentHandler());
            AuraHandler.EffectHandlers[150] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModShieldBlockValuePercentHandler());
            AuraHandler.EffectHandlers[156] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModReputationGainHandler());
            AuraHandler.EffectHandlers[159] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new NoPvPCreditHandler());
            AuraHandler.EffectHandlers[161] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModHealthRegenInCombatHandler());
            AuraHandler.EffectHandlers[162] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new PowerBurnHandler());
            AuraHandler.EffectHandlers[163] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModMeleeCritDamageBonusHandler());
            AuraHandler.EffectHandlers[166] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModMeleeAttackPowerPercentHandler());
            AuraHandler.EffectHandlers[167] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModRangedAttackPowerPercentHandler());
            AuraHandler.EffectHandlers[168] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModDamageDoneVersusCreatureTypeHandler());
            AuraHandler.EffectHandlers[174] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModSpellDamageByPercentOfStatHandler());
            AuraHandler.EffectHandlers[175] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModHealingByPercentOfStatHandler());
            AuraHandler.EffectHandlers[178] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModDebuffResistancePercentHandler());
            AuraHandler.EffectHandlers[179] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModAttackerSpellCritChanceHandler());
            AuraHandler.EffectHandlers[182] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModArmorByPercentOfIntellectHandler());
            AuraHandler.EffectHandlers[184] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModAttackerMeleeHitChanceHandler());
            AuraHandler.EffectHandlers[185] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModAttackerRangedHitChanceHandler());
            AuraHandler.EffectHandlers[186] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModAttackerSpellHitChanceHandler());
            AuraHandler.EffectHandlers[189] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModRatingHandler());
            AuraHandler.EffectHandlers[192] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModMeleeHastePercentHandler());
            AuraHandler.EffectHandlers[193] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModHastePercentHandler());
            AuraHandler.EffectHandlers[196] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModAllCooldownDurationHandler());
            AuraHandler.EffectHandlers[197] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModAttackerCritChancePercentHandler());
            AuraHandler.EffectHandlers[199] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModSpellHitChanceHandler());
            AuraHandler.EffectHandlers[200] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModKillXpPctHandler());
            AuraHandler.EffectHandlers[201] = (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new FlyHandler());
            AuraHandler.EffectHandlers[207] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModSpeedMountedFlightHandler());
            AuraHandler.EffectHandlers[212] = (AuraEffectHandlerCreator) (() =>
                (AuraEffectHandler) new ModRangedAttackPowerByPercentOfStatHandler());
            AuraHandler.EffectHandlers[213] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModRageFromDamageDealtPercentHandler());
            AuraHandler.EffectHandlers[215] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ArenaPreparationHandler());
            AuraHandler.EffectHandlers[216] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModSpellHastePercentHandler());
            AuraHandler.EffectHandlers[219] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModManaRegenHandler());
            AuraHandler.EffectHandlers[220] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModCombatRatingStat());
            AuraHandler.EffectHandlers[227] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new PeriodicTriggerSpellHandler());
            AuraHandler.EffectHandlers[229] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModAOEDamagePercentHandler());
            AuraHandler.EffectHandlers[230] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModMaxHealthHandler());
            AuraHandler.EffectHandlers[231] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ProcTriggerSpellHandler());
            AuraHandler.EffectHandlers[232] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModSilenceDurationPercentHandler());
            AuraHandler.EffectHandlers[234] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModMechanicDurationPercentHandler());
            AuraHandler.EffectHandlers[236] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new VehicleAuraHandler());
            AuraHandler.EffectHandlers[237] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModSpellPowerByAPPctHandler());
            AuraHandler.EffectHandlers[239] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModScaleHandler());
            AuraHandler.EffectHandlers[240] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModExpertiseHandler());
            AuraHandler.EffectHandlers[241] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ForceAutoRunForwardHandler());
            AuraHandler.EffectHandlers[247] = (AuraEffectHandlerCreator) (() =>
                (AuraEffectHandler) new WCell.RealmServer.Spells.Auras.Misc.MirrorImageHandler());
            AuraHandler.EffectHandlers[248] = (AuraEffectHandlerCreator) (() =>
                (AuraEffectHandler) new ModChanceTargetDodgesAttackPercentHandler());
            AuraHandler.EffectHandlers[253] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new CriticalBlockPctHandler());
            AuraHandler.EffectHandlers[254] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new DisarmOffHandHandler());
            AuraHandler.EffectHandlers[(int) byte.MaxValue] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new AuraVoidHandler());
            AuraHandler.EffectHandlers[261] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new PhaseAuraHandler());
            AuraHandler.EffectHandlers[268] = (AuraEffectHandlerCreator) (() =>
                (AuraEffectHandler) new ModMeleeAttackPowerByPercentOfStatHandler());
            AuraHandler.EffectHandlers[271] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new DamagePctAmplifierHandler());
            AuraHandler.EffectHandlers[278] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new DisarmRangedHandler());
            AuraHandler.EffectHandlers[280] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModArmorPenetrationHandler());
            AuraHandler.EffectHandlers[284] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ToggleAuraHandler());
            AuraHandler.EffectHandlers[285] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModAPByArmorHandler());
            AuraHandler.EffectHandlers[286] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new EnableCriticalHandler());
            AuraHandler.EffectHandlers[291] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ModQuestXpPctHandler());
            AuraHandler.EffectHandlers[292] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new CallStabledPetHandler());
            AuraHandler.EffectHandlers[309] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new WhirlwindEffectHandler());
            AuraHandler.EffectHandlers[310] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new WhirlwindEffectHandler());
            AuraHandler.EffectHandlers[311] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new TrapEffectHandler());
            AuraHandler.EffectHandlers[312] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new DragonSlayerEffectHandler());
            AuraHandler.EffectHandlers[313] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new FlashLightEffectHandler());
            AuraHandler.EffectHandlers[314] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new SurpriseEffectHandler());
            AuraHandler.EffectHandlers[315] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new TimeBombEffectHandler());
            AuraHandler.EffectHandlers[318] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ThunderBoltEffectHandler());
            AuraHandler.EffectHandlers[316] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ResurectOnDeathPlaceEffectHandler());
            AuraHandler.EffectHandlers[317] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ExploitBloodEffectHandler());
            AuraHandler.EffectHandlers[319] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new AbsorbMagicEffectHandler());
            AuraHandler.EffectHandlers[320] =
                (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new ExplosiveArrowEffectHandler());
            for (int index = 0; index < 500; ++index)
            {
                if (AuraHandler.EffectHandlers[index] == null)
                    AuraHandler.EffectHandlers[index] =
                        (AuraEffectHandlerCreator) (() => (AuraEffectHandler) new AuraVoidHandler());
            }
        }

        internal static uint GetNextAuraUID()
        {
            if (AuraHandler.lastAuraUid == 0U)
                AuraHandler.lastAuraUid = 11078U;
            return ++AuraHandler.lastAuraUid;
        }

        internal static void RegisterAuraUIDEvaluators()
        {
            AuraHandler.AddAuraGroupEvaluator(new AuraIdEvaluator(AuraHandler.IsTransform));
            AuraHandler.AddAuraGroupEvaluator(new AuraIdEvaluator(AuraHandler.IsStealth));
            AuraHandler.AddAuraGroupEvaluator(new AuraIdEvaluator(AuraHandler.IsTracker));
        }

        /// <summary>
        /// All transform and visually supported shapeshift spells are in one group
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        private static bool IsTransform(Spell spell)
        {
            return spell.IsShapeshift;
        }

        private static bool IsStealth(Spell spell)
        {
            return spell.HasEffectWith((Predicate<SpellEffect>) (effect => effect.AuraType == AuraType.ModStealth));
        }

        private static bool IsTracker(Spell spell)
        {
            if (!spell.HasEffect(AuraType.TrackCreatures) && !spell.HasEffect(AuraType.TrackResources))
                return spell.HasEffect(AuraType.TrackStealthed);
            return true;
        }

        public static void AddAuraGroupEvaluator(AuraIdEvaluator eval)
        {
            if (ServerApp<WCell.RealmServer.RealmServer>.Instance.IsRunning &&
                ServerApp<WCell.RealmServer.RealmServer>.Instance.ClientCount > 0)
                throw new InvalidOperationException(
                    "Cannot set an Aura Group Evaluator at runtime because Aura Group IDs cannot be re-evaluated at this time. Please register the evaluator during startup.");
            AuraHandler.AuraIdEvaluators.Add(eval);
        }

        /// <summary>Defines a set of Auras that are mutually exclusive</summary>
        public static uint AddAuraGroup(IEnumerable<Spell> auras)
        {
            uint nextAuraUid = AuraHandler.GetNextAuraUID();
            foreach (Spell aura in auras)
                aura.AuraUID = nextAuraUid;
            return nextAuraUid;
        }

        /// <summary>Defines a set of Auras that are mutually exclusive</summary>
        public static uint AddAuraGroup(params SpellId[] auras)
        {
            uint nextAuraUid = AuraHandler.GetNextAuraUID();
            foreach (SpellId aura in auras)
            {
                Spell spell = SpellHandler.Get(aura);
                if (spell == null)
                    throw new ArgumentException("Invalid SpellId: " + (object) aura);
                spell.AuraUID = nextAuraUid;
            }

            return nextAuraUid;
        }

        /// <summary>Defines a set of Auras that are mutually exclusive</summary>
        public static uint AddAuraGroup(SpellId auraId, params SpellLineId[] auraLines)
        {
            uint nextAuraUid = AuraHandler.GetNextAuraUID();
            SpellHandler.Get(auraId).AuraUID = nextAuraUid;
            foreach (SpellLineId auraLine in auraLines)
            {
                SpellLine line = auraLine.GetLine();
                line.AuraUID = nextAuraUid;
                foreach (Spell spell in line)
                    spell.AuraUID = nextAuraUid;
            }

            return nextAuraUid;
        }

        /// <summary>Defines a set of Auras that are mutually exclusive</summary>
        public static uint AddAuraGroup(SpellId auraId, SpellId auraId2, params SpellLineId[] auraLines)
        {
            uint nextAuraUid = AuraHandler.GetNextAuraUID();
            SpellHandler.Get(auraId).AuraUID = nextAuraUid;
            SpellHandler.Get(auraId2).AuraUID = nextAuraUid;
            foreach (SpellLineId auraLine in auraLines)
            {
                SpellLine line = auraLine.GetLine();
                line.AuraUID = nextAuraUid;
                foreach (Spell spell in line)
                    spell.AuraUID = nextAuraUid;
            }

            return nextAuraUid;
        }

        /// <summary>Defines a set of Auras that are mutually exclusive</summary>
        public static uint AddAuraGroup(SpellLineId auraLine, params SpellId[] auras)
        {
            uint nextAuraUid = AuraHandler.GetNextAuraUID();
            SpellLine line = auraLine.GetLine();
            line.AuraUID = nextAuraUid;
            foreach (Spell spell in line)
                spell.AuraUID = nextAuraUid;
            foreach (SpellId aura in auras)
            {
                Spell spell = SpellHandler.Get(aura);
                if (spell == null)
                    throw new ArgumentException("Invalid SpellId: " + (object) aura);
                spell.AuraUID = nextAuraUid;
            }

            return nextAuraUid;
        }

        /// <summary>Defines a set of Auras that are mutually exclusive</summary>
        public static uint AddAuraGroup(SpellLineId auraLine, SpellLineId auraLine2, params SpellId[] auras)
        {
            uint nextAuraUid = AuraHandler.GetNextAuraUID();
            SpellLine line1 = auraLine.GetLine();
            line1.AuraUID = nextAuraUid;
            foreach (Spell spell in line1)
                spell.AuraUID = nextAuraUid;
            SpellLine line2 = auraLine2.GetLine();
            line2.AuraUID = nextAuraUid;
            foreach (Spell spell in line2)
                spell.AuraUID = nextAuraUid;
            foreach (SpellId aura in auras)
            {
                Spell spell = SpellHandler.Get(aura);
                if (spell == null)
                    throw new ArgumentException("Invalid SpellId: " + (object) aura);
                spell.AuraUID = nextAuraUid;
            }

            return nextAuraUid;
        }

        /// <summary>Defines a set of Auras that are mutually exclusive</summary>
        public static uint AddAuraGroup(params SpellLineId[] auraLines)
        {
            uint nextAuraUid = AuraHandler.GetNextAuraUID();
            foreach (SpellLineId auraLine in auraLines)
            {
                SpellLine line = auraLine.GetLine();
                line.AuraUID = nextAuraUid;
                foreach (Spell spell in line)
                    spell.AuraUID = nextAuraUid;
            }

            return nextAuraUid;
        }

        /// <summary>
        /// Defines a set of Auras of which one Unit can only have 1 per caster
        /// </summary>
        public static AuraCasterGroup AddAuraCasterGroup(params SpellLineId[] ids)
        {
            AuraCasterGroup auraCasterGroup1 = new AuraCasterGroup();
            auraCasterGroup1.Add(ids);
            AuraCasterGroup auraCasterGroup2 = auraCasterGroup1;
            foreach (Spell spell in (List<Spell>) auraCasterGroup2)
                spell.AuraCasterGroup = auraCasterGroup2;
            return auraCasterGroup2;
        }

        /// <summary>
        /// Defines a set of Auras of which one Unit can only have the given amount per caster
        /// </summary>
        public static AuraCasterGroup AddAuraCasterGroup(int maxPerCaster, params SpellLineId[] ids)
        {
            AuraCasterGroup auraCasterGroup1 = new AuraCasterGroup(maxPerCaster);
            auraCasterGroup1.Add(ids);
            AuraCasterGroup auraCasterGroup2 = auraCasterGroup1;
            foreach (Spell spell in (List<Spell>) auraCasterGroup2)
                spell.AuraCasterGroup = auraCasterGroup2;
            return auraCasterGroup2;
        }
    }
}