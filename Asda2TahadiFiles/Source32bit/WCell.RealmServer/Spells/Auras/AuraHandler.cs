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
        [(int) Convert.ChangeType(Utility.GetMaxEnum<AuraType>(), typeof(int)) + 1];

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
      if(index == null)
        return;
      Aura aura = client.ActiveCharacter.Auras[index, true];
      if(aura == null || !aura.CanBeRemoved)
        return;
      aura.TryRemove(true);
    }

    public static void SendAuraUpdate(Unit owner, Aura aura)
    {
      if(owner is NPC)
      {
        Asda2CombatHandler.SendNpcBuffedResponse((NPC) owner, aura);
      }
      else
      {
        if(!(owner is Character))
          return;
        Character character = (Character) owner;
        if(!character.CanMove)
          Asda2MovmentHandler.SendEndMoveByFastInstantRegularMoveResponse(character);
        Asda2SpellHandler.SendCharacterBuffedResponse(character, aura);
        if(character.IsInGroup)
          Asda2GroupHandler.SendPartyMemberBuffInfoResponse(character);
        if(character.SoulmateCharacter == null)
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
      owner.EntityId.WritePacked(realmPacketOut);
      foreach(Aura aura in owner.Auras)
      {
        if(aura.IsVisible)
          WriteAura(aura, realmPacketOut);
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
      if(!aura.Flags.HasFlag(AuraFlags.TargetIsCaster))
        aura.CasterReference.EntityId.WritePacked(packet);
      if(!aura.Flags.HasFlag(AuraFlags.HasDuration))
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
      foreach(uint affectMaskBit in effect.AffectMaskBitSet)
      {
        int amount = 0;
        uint num1 = affectMaskBit >> 5;
        uint num2 = affectMaskBit - (num1 << 5);
        for(int index = 0; index < modifierEffectHandlerList.Count; ++index)
        {
          AddModifierEffectHandler modifierEffectHandler = modifierEffectHandlerList[index];
          if((SpellModifierType) modifierEffectHandler.SpellEffect.MiscValue == miscValue &&
             modifierEffectHandler.SpellEffect.Spell.SpellClassSet == effect.Spell.SpellClassSet &&
             (modifierEffectHandler.SpellEffect.AffectMask[num1] & 1 << (int) num2) != 0L)
            amount += modifierEffectHandler.SpellEffect.ValueMin;
        }

        SpellHandler.SendSpellModifier(chr, (byte) affectMaskBit, miscValue, amount, isPercent);
      }
    }

    static AuraHandler()
    {
      EffectHandlers[0] =
        () => (AuraEffectHandler) new AuraVoidHandler();
      EffectHandlers[2] =
        () => (AuraEffectHandler) new ModPossessAuraHandler();
      EffectHandlers[3] =
        () => (AuraEffectHandler) new PeriodicDamageHandler();
      EffectHandlers[4] = () => (AuraEffectHandler) new DummyHandler();
      EffectHandlers[5] =
        () => (AuraEffectHandler) new ModConfuseHandler();
      EffectHandlers[6] =
        () => (AuraEffectHandler) new CharmAuraHandler();
      EffectHandlers[7] = () => (AuraEffectHandler) new FearHandler();
      EffectHandlers[8] =
        () => (AuraEffectHandler) new PeriodicHealHandler();
      EffectHandlers[9] =
        () => (AuraEffectHandler) new ModAttackSpeedHandler();
      EffectHandlers[10] =
        () => (AuraEffectHandler) new ModThreatHandler();
      EffectHandlers[11] =
        () => (AuraEffectHandler) new ModTauntAuraHandler();
      EffectHandlers[12] = () => (AuraEffectHandler) new StunHandler();
      EffectHandlers[13] =
        () => (AuraEffectHandler) new ModDamageDoneHandler();
      EffectHandlers[14] =
        () => (AuraEffectHandler) new ModDamageTakenHandler();
      EffectHandlers[15] =
        () => (AuraEffectHandler) new DamageShieldEffectHandler();
      EffectHandlers[16] =
        () => (AuraEffectHandler) new ModStealthHandler();
      EffectHandlers[18] =
        () => (AuraEffectHandler) new ModInvisibilityHandler();
      EffectHandlers[20] =
        () => (AuraEffectHandler) new RegenPercentOfTotalHealthHandler();
      EffectHandlers[21] =
        () => (AuraEffectHandler) new RegenPercentOfTotalManaHandler();
      EffectHandlers[22] =
        () => (AuraEffectHandler) new ModResistanceHandler();
      EffectHandlers[23] =
        () => (AuraEffectHandler) new PeriodicTriggerSpellHandler();
      EffectHandlers[24] =
        () => (AuraEffectHandler) new PeriodicEnergizeHandler();
      EffectHandlers[25] =
        () => (AuraEffectHandler) new ModPacifyHandler();
      EffectHandlers[26] = () => (AuraEffectHandler) new RootHandler();
      EffectHandlers[27] =
        () => (AuraEffectHandler) new ModSilenceHandler();
      EffectHandlers[29] =
        () => (AuraEffectHandler) new ModStatHandler();
      EffectHandlers[31] =
        () => (AuraEffectHandler) new ModIncreaseSpeedHandler();
      EffectHandlers[32] =
        () => (AuraEffectHandler) new ModIncreaseMountedSpeedHandler();
      EffectHandlers[33] =
        () => (AuraEffectHandler) new ModDecreaseSpeedHandler();
      EffectHandlers[34] =
        () => (AuraEffectHandler) new ModIncreaseHealthHandler();
      EffectHandlers[35] =
        () => (AuraEffectHandler) new ModIncreaseEnergyHandler();
      EffectHandlers[36] =
        () => (AuraEffectHandler) new ShapeshiftHandler();
      EffectHandlers[38] =
        () => (AuraEffectHandler) new StateImmunityHandler();
      EffectHandlers[39] =
        () => (AuraEffectHandler) new SchoolImmunityHandler();
      EffectHandlers[40] =
        () => (AuraEffectHandler) new DamageImmunityHandler();
      EffectHandlers[41] =
        () => (AuraEffectHandler) new DispelImmunityHandler();
      EffectHandlers[42] =
        () => (AuraEffectHandler) new ProcTriggerSpellHandler();
      EffectHandlers[43] =
        () => (AuraEffectHandler) new ProcTriggerDamageHandler();
      EffectHandlers[44] =
        () => (AuraEffectHandler) new TrackCreaturesHandler();
      EffectHandlers[45] =
        () => (AuraEffectHandler) new TrackResourcesHandler();
      EffectHandlers[47] =
        () => (AuraEffectHandler) new ModParryPercentHandler();
      EffectHandlers[49] =
        () => (AuraEffectHandler) new ModDodgePercentHandler();
      EffectHandlers[50] =
        () => (AuraEffectHandler) new ModCritHealValuePctHandler();
      EffectHandlers[51] =
        () => (AuraEffectHandler) new ModBlockPercentHandler();
      EffectHandlers[52] =
        () => (AuraEffectHandler) new ModCritPercentHandler();
      EffectHandlers[53] =
        () => (AuraEffectHandler) new PeriodicLeechHandler();
      EffectHandlers[54] =
        () => (AuraEffectHandler) new ModHitChanceHandler();
      EffectHandlers[55] =
        () => (AuraEffectHandler) new ModSpellHitChanceHandler();
      EffectHandlers[56] =
        () => (AuraEffectHandler) new TransformHandler();
      EffectHandlers[57] =
        () => (AuraEffectHandler) new ModSpellCritChanceHandler();
      EffectHandlers[58] =
        () => (AuraEffectHandler) new ModIncreaseSwimSpeedHandler();
      EffectHandlers[60] =
        () => (AuraEffectHandler) new ModPacifySilenceHandler();
      EffectHandlers[61] =
        () => (AuraEffectHandler) new ModScaleHandler();
      EffectHandlers[62] =
        () => (AuraEffectHandler) new PeriodicHealthFunnelHandler();
      EffectHandlers[64] =
        () => (AuraEffectHandler) new PeriodicManaLeechHandler();
      EffectHandlers[65] =
        () => (AuraEffectHandler) new ModCastingSpeedHandler();
      EffectHandlers[67] =
        () => (AuraEffectHandler) new DisarmMainHandHandler();
      EffectHandlers[69] =
        () => (AuraEffectHandler) new SchoolAbsorbHandler();
      EffectHandlers[71] =
        () => (AuraEffectHandler) new ModSpellCritChanceForSchoolHandler();
      EffectHandlers[72] =
        () => (AuraEffectHandler) new ModPowerCostHandler();
      EffectHandlers[73] =
        () => (AuraEffectHandler) new ModPowerCostForSchoolHandler();
      EffectHandlers[75] =
        () => (AuraEffectHandler) new ModLanguageHandler();
      EffectHandlers[77] =
        () => (AuraEffectHandler) new MechanicImmunityHandler();
      EffectHandlers[78] =
        () => (AuraEffectHandler) new MountedHandler();
      EffectHandlers[79] =
        () => (AuraEffectHandler) new ModDamageDonePercentHandler();
      EffectHandlers[80] =
        () => (AuraEffectHandler) new ModStatPercentHandler();
      EffectHandlers[81] =
        () => (AuraEffectHandler) new SplitDamageHandler();
      EffectHandlers[85] =
        () => (AuraEffectHandler) new ModPowerRegenHandler();
      EffectHandlers[86] =
        () => (AuraEffectHandler) new CreateItemOnTargetDeathHandler();
      EffectHandlers[87] =
        () => (AuraEffectHandler) new ModDamageTakenPercentHandler();
      EffectHandlers[89] =
        () => (AuraEffectHandler) new PeriodicDamagePercentHandler();
      EffectHandlers[91] =
        () => (AuraEffectHandler) new ModDetectRangeHandler();
      EffectHandlers[93] =
        () => (AuraEffectHandler) new UnattackableHandler();
      EffectHandlers[94] =
        () => (AuraEffectHandler) new InterruptRegenHandler();
      EffectHandlers[95] = () => (AuraEffectHandler) new GhostHandler();
      EffectHandlers[97] =
        () => (AuraEffectHandler) new ManaShieldHandler();
      EffectHandlers[98] =
        () => (AuraEffectHandler) new ModSkillTalentHandler();
      EffectHandlers[99] =
        () => (AuraEffectHandler) new ModMeleeAttackPowerHandler();
      EffectHandlers[101] =
        () => (AuraEffectHandler) new ModResistancePctHandler();
      EffectHandlers[104] =
        () => (AuraEffectHandler) new WaterWalkHandler();
      EffectHandlers[105] =
        () => (AuraEffectHandler) new FeatherFallHandler();
      EffectHandlers[106] = () => (AuraEffectHandler) new HoverHandler();
      EffectHandlers[107] =
        () => (AuraEffectHandler) new AddModifierFlatHandler();
      EffectHandlers[108] =
        () => (AuraEffectHandler) new AddModifierPercentHandler();
      EffectHandlers[109] =
        () => (AuraEffectHandler) new AddTargetTriggerHandler();
      EffectHandlers[110] =
        () => (AuraEffectHandler) new ModPowerRegenPercentHandler();
      EffectHandlers[111] =
        () => (AuraEffectHandler) new AddCasterHitTriggerHandler();
      EffectHandlers[117] =
        () => (AuraEffectHandler) new ModMechanicResistanceHandler();
      EffectHandlers[118] =
        () => (AuraEffectHandler) new ModHealingTakenPctHandler();
      EffectHandlers[120] =
        () => (AuraEffectHandler) new UntrackableHandler();
      EffectHandlers[122] =
        () => (AuraEffectHandler) new ModOffhandDamagePercentHandler();
      EffectHandlers[123] =
        () => (AuraEffectHandler) new ModTargetResistanceHandler();
      EffectHandlers[124] =
        () => (AuraEffectHandler) new ModRangedAttackPowerHandler();
      EffectHandlers[129] =
        () => (AuraEffectHandler) new ModIncreaseSpeedAlwaysHandler();
      EffectHandlers[130] =
        () => (AuraEffectHandler) new ModMountedSpeedAlwaysHandler();
      EffectHandlers[132] =
        () => (AuraEffectHandler) new ModIncreaseEnergyPercentHandler();
      EffectHandlers[133] =
        () => (AuraEffectHandler) new ModIncreaseHealthPercentHandler();
      EffectHandlers[134] =
        () => (AuraEffectHandler) new ModManaRegenInterruptHandler();
      EffectHandlers[135] =
        () => (AuraEffectHandler) new ModHealingDoneHandler();
      EffectHandlers[136] =
        () => (AuraEffectHandler) new ModHealingDonePctHandler();
      EffectHandlers[137] =
        () => (AuraEffectHandler) new ModTotalStatPercentHandler();
      EffectHandlers[138] =
        () => (AuraEffectHandler) new ModHasteHandler();
      EffectHandlers[139] =
        () => (AuraEffectHandler) new ForceReactionHandler();
      EffectHandlers[142] =
        () => (AuraEffectHandler) new ModBaseResistancePercentHandler();
      EffectHandlers[143] =
        () => (AuraEffectHandler) new ModResistanceExclusiveHandler();
      EffectHandlers[144] =
        () => (AuraEffectHandler) new SafeFallHandler();
      EffectHandlers[145] =
        () => (AuraEffectHandler) new ModPetTalentPointsHandler();
      EffectHandlers[146] =
        () => (AuraEffectHandler) new ControlExoticPetsHandler();
      EffectHandlers[148] =
        () => (AuraEffectHandler) new RetainComboPointsHandler();
      EffectHandlers[149] = () =>
        (AuraEffectHandler) new ModResistSpellInterruptionPercentHandler();
      EffectHandlers[150] =
        () => (AuraEffectHandler) new ModShieldBlockValuePercentHandler();
      EffectHandlers[156] =
        () => (AuraEffectHandler) new ModReputationGainHandler();
      EffectHandlers[159] =
        () => (AuraEffectHandler) new NoPvPCreditHandler();
      EffectHandlers[161] =
        () => (AuraEffectHandler) new ModHealthRegenInCombatHandler();
      EffectHandlers[162] =
        () => (AuraEffectHandler) new PowerBurnHandler();
      EffectHandlers[163] =
        () => (AuraEffectHandler) new ModMeleeCritDamageBonusHandler();
      EffectHandlers[166] =
        () => (AuraEffectHandler) new ModMeleeAttackPowerPercentHandler();
      EffectHandlers[167] =
        () => (AuraEffectHandler) new ModRangedAttackPowerPercentHandler();
      EffectHandlers[168] =
        () => (AuraEffectHandler) new ModDamageDoneVersusCreatureTypeHandler();
      EffectHandlers[174] =
        () => (AuraEffectHandler) new ModSpellDamageByPercentOfStatHandler();
      EffectHandlers[175] =
        () => (AuraEffectHandler) new ModHealingByPercentOfStatHandler();
      EffectHandlers[178] =
        () => (AuraEffectHandler) new ModDebuffResistancePercentHandler();
      EffectHandlers[179] =
        () => (AuraEffectHandler) new ModAttackerSpellCritChanceHandler();
      EffectHandlers[182] =
        () => (AuraEffectHandler) new ModArmorByPercentOfIntellectHandler();
      EffectHandlers[184] =
        () => (AuraEffectHandler) new ModAttackerMeleeHitChanceHandler();
      EffectHandlers[185] =
        () => (AuraEffectHandler) new ModAttackerRangedHitChanceHandler();
      EffectHandlers[186] =
        () => (AuraEffectHandler) new ModAttackerSpellHitChanceHandler();
      EffectHandlers[189] =
        () => (AuraEffectHandler) new ModRatingHandler();
      EffectHandlers[192] =
        () => (AuraEffectHandler) new ModMeleeHastePercentHandler();
      EffectHandlers[193] =
        () => (AuraEffectHandler) new ModHastePercentHandler();
      EffectHandlers[196] =
        () => (AuraEffectHandler) new ModAllCooldownDurationHandler();
      EffectHandlers[197] =
        () => (AuraEffectHandler) new ModAttackerCritChancePercentHandler();
      EffectHandlers[199] =
        () => (AuraEffectHandler) new ModSpellHitChanceHandler();
      EffectHandlers[200] =
        () => (AuraEffectHandler) new ModKillXpPctHandler();
      EffectHandlers[201] = () => (AuraEffectHandler) new FlyHandler();
      EffectHandlers[207] =
        () => (AuraEffectHandler) new ModSpeedMountedFlightHandler();
      EffectHandlers[212] = () =>
        (AuraEffectHandler) new ModRangedAttackPowerByPercentOfStatHandler();
      EffectHandlers[213] =
        () => (AuraEffectHandler) new ModRageFromDamageDealtPercentHandler();
      EffectHandlers[215] =
        () => (AuraEffectHandler) new ArenaPreparationHandler();
      EffectHandlers[216] =
        () => (AuraEffectHandler) new ModSpellHastePercentHandler();
      EffectHandlers[219] =
        () => (AuraEffectHandler) new ModManaRegenHandler();
      EffectHandlers[220] =
        () => (AuraEffectHandler) new ModCombatRatingStat();
      EffectHandlers[227] =
        () => (AuraEffectHandler) new PeriodicTriggerSpellHandler();
      EffectHandlers[229] =
        () => (AuraEffectHandler) new ModAOEDamagePercentHandler();
      EffectHandlers[230] =
        () => (AuraEffectHandler) new ModMaxHealthHandler();
      EffectHandlers[231] =
        () => (AuraEffectHandler) new ProcTriggerSpellHandler();
      EffectHandlers[232] =
        () => (AuraEffectHandler) new ModSilenceDurationPercentHandler();
      EffectHandlers[234] =
        () => (AuraEffectHandler) new ModMechanicDurationPercentHandler();
      EffectHandlers[236] =
        () => (AuraEffectHandler) new VehicleAuraHandler();
      EffectHandlers[237] =
        () => (AuraEffectHandler) new ModSpellPowerByAPPctHandler();
      EffectHandlers[239] =
        () => (AuraEffectHandler) new ModScaleHandler();
      EffectHandlers[240] =
        () => (AuraEffectHandler) new ModExpertiseHandler();
      EffectHandlers[241] =
        () => (AuraEffectHandler) new ForceAutoRunForwardHandler();
      EffectHandlers[247] = () =>
        (AuraEffectHandler) new Misc.MirrorImageHandler();
      EffectHandlers[248] = () =>
        (AuraEffectHandler) new ModChanceTargetDodgesAttackPercentHandler();
      EffectHandlers[253] =
        () => (AuraEffectHandler) new CriticalBlockPctHandler();
      EffectHandlers[254] =
        () => (AuraEffectHandler) new DisarmOffHandHandler();
      EffectHandlers[byte.MaxValue] =
        () => (AuraEffectHandler) new AuraVoidHandler();
      EffectHandlers[261] =
        () => (AuraEffectHandler) new PhaseAuraHandler();
      EffectHandlers[268] = () =>
        (AuraEffectHandler) new ModMeleeAttackPowerByPercentOfStatHandler();
      EffectHandlers[271] =
        () => (AuraEffectHandler) new DamagePctAmplifierHandler();
      EffectHandlers[278] =
        () => (AuraEffectHandler) new DisarmRangedHandler();
      EffectHandlers[280] =
        () => (AuraEffectHandler) new ModArmorPenetrationHandler();
      EffectHandlers[284] =
        () => (AuraEffectHandler) new ToggleAuraHandler();
      EffectHandlers[285] =
        () => (AuraEffectHandler) new ModAPByArmorHandler();
      EffectHandlers[286] =
        () => (AuraEffectHandler) new EnableCriticalHandler();
      EffectHandlers[291] =
        () => (AuraEffectHandler) new ModQuestXpPctHandler();
      EffectHandlers[292] =
        () => (AuraEffectHandler) new CallStabledPetHandler();
      EffectHandlers[309] =
        () => (AuraEffectHandler) new WhirlwindEffectHandler();
      EffectHandlers[310] =
        () => (AuraEffectHandler) new WhirlwindEffectHandler();
      EffectHandlers[311] =
        () => (AuraEffectHandler) new TrapEffectHandler();
      EffectHandlers[312] =
        () => (AuraEffectHandler) new DragonSlayerEffectHandler();
      EffectHandlers[313] =
        () => (AuraEffectHandler) new FlashLightEffectHandler();
      EffectHandlers[314] =
        () => (AuraEffectHandler) new SurpriseEffectHandler();
      EffectHandlers[315] =
        () => (AuraEffectHandler) new TimeBombEffectHandler();
      EffectHandlers[318] =
        () => (AuraEffectHandler) new ThunderBoltEffectHandler();
      EffectHandlers[316] =
        () => (AuraEffectHandler) new ResurectOnDeathPlaceEffectHandler();
      EffectHandlers[317] =
        () => (AuraEffectHandler) new ExploitBloodEffectHandler();
      EffectHandlers[319] =
        () => (AuraEffectHandler) new AbsorbMagicEffectHandler();
      EffectHandlers[320] =
        () => (AuraEffectHandler) new ExplosiveArrowEffectHandler();
      for(int index = 0; index < 500; ++index)
      {
        if(EffectHandlers[index] == null)
          EffectHandlers[index] =
            () => (AuraEffectHandler) new AuraVoidHandler();
      }
    }

    internal static uint GetNextAuraUID()
    {
      if(lastAuraUid == 0U)
        lastAuraUid = 11078U;
      return ++lastAuraUid;
    }

    internal static void RegisterAuraUIDEvaluators()
    {
      AddAuraGroupEvaluator(IsTransform);
      AddAuraGroupEvaluator(IsStealth);
      AddAuraGroupEvaluator(IsTracker);
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
      return spell.HasEffectWith(effect => effect.AuraType == AuraType.ModStealth);
    }

    private static bool IsTracker(Spell spell)
    {
      if(!spell.HasEffect(AuraType.TrackCreatures) && !spell.HasEffect(AuraType.TrackResources))
        return spell.HasEffect(AuraType.TrackStealthed);
      return true;
    }

    public static void AddAuraGroupEvaluator(AuraIdEvaluator eval)
    {
      if(ServerApp<RealmServer>.Instance.IsRunning &&
         ServerApp<RealmServer>.Instance.ClientCount > 0)
        throw new InvalidOperationException(
          "Cannot set an Aura Group Evaluator at runtime because Aura Group IDs cannot be re-evaluated at this time. Please register the evaluator during startup.");
      AuraIdEvaluators.Add(eval);
    }

    /// <summary>Defines a set of Auras that are mutually exclusive</summary>
    public static uint AddAuraGroup(IEnumerable<Spell> auras)
    {
      uint nextAuraUid = GetNextAuraUID();
      foreach(Spell aura in auras)
        aura.AuraUID = nextAuraUid;
      return nextAuraUid;
    }

    /// <summary>Defines a set of Auras that are mutually exclusive</summary>
    public static uint AddAuraGroup(params SpellId[] auras)
    {
      uint nextAuraUid = GetNextAuraUID();
      foreach(SpellId aura in auras)
      {
        Spell spell = SpellHandler.Get(aura);
        if(spell == null)
          throw new ArgumentException("Invalid SpellId: " + aura);
        spell.AuraUID = nextAuraUid;
      }

      return nextAuraUid;
    }

    /// <summary>Defines a set of Auras that are mutually exclusive</summary>
    public static uint AddAuraGroup(SpellId auraId, params SpellLineId[] auraLines)
    {
      uint nextAuraUid = GetNextAuraUID();
      SpellHandler.Get(auraId).AuraUID = nextAuraUid;
      foreach(SpellLineId auraLine in auraLines)
      {
        SpellLine line = auraLine.GetLine();
        line.AuraUID = nextAuraUid;
        foreach(Spell spell in line)
          spell.AuraUID = nextAuraUid;
      }

      return nextAuraUid;
    }

    /// <summary>Defines a set of Auras that are mutually exclusive</summary>
    public static uint AddAuraGroup(SpellId auraId, SpellId auraId2, params SpellLineId[] auraLines)
    {
      uint nextAuraUid = GetNextAuraUID();
      SpellHandler.Get(auraId).AuraUID = nextAuraUid;
      SpellHandler.Get(auraId2).AuraUID = nextAuraUid;
      foreach(SpellLineId auraLine in auraLines)
      {
        SpellLine line = auraLine.GetLine();
        line.AuraUID = nextAuraUid;
        foreach(Spell spell in line)
          spell.AuraUID = nextAuraUid;
      }

      return nextAuraUid;
    }

    /// <summary>Defines a set of Auras that are mutually exclusive</summary>
    public static uint AddAuraGroup(SpellLineId auraLine, params SpellId[] auras)
    {
      uint nextAuraUid = GetNextAuraUID();
      SpellLine line = auraLine.GetLine();
      line.AuraUID = nextAuraUid;
      foreach(Spell spell in line)
        spell.AuraUID = nextAuraUid;
      foreach(SpellId aura in auras)
      {
        Spell spell = SpellHandler.Get(aura);
        if(spell == null)
          throw new ArgumentException("Invalid SpellId: " + aura);
        spell.AuraUID = nextAuraUid;
      }

      return nextAuraUid;
    }

    /// <summary>Defines a set of Auras that are mutually exclusive</summary>
    public static uint AddAuraGroup(SpellLineId auraLine, SpellLineId auraLine2, params SpellId[] auras)
    {
      uint nextAuraUid = GetNextAuraUID();
      SpellLine line1 = auraLine.GetLine();
      line1.AuraUID = nextAuraUid;
      foreach(Spell spell in line1)
        spell.AuraUID = nextAuraUid;
      SpellLine line2 = auraLine2.GetLine();
      line2.AuraUID = nextAuraUid;
      foreach(Spell spell in line2)
        spell.AuraUID = nextAuraUid;
      foreach(SpellId aura in auras)
      {
        Spell spell = SpellHandler.Get(aura);
        if(spell == null)
          throw new ArgumentException("Invalid SpellId: " + aura);
        spell.AuraUID = nextAuraUid;
      }

      return nextAuraUid;
    }

    /// <summary>Defines a set of Auras that are mutually exclusive</summary>
    public static uint AddAuraGroup(params SpellLineId[] auraLines)
    {
      uint nextAuraUid = GetNextAuraUID();
      foreach(SpellLineId auraLine in auraLines)
      {
        SpellLine line = auraLine.GetLine();
        line.AuraUID = nextAuraUid;
        foreach(Spell spell in line)
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
      foreach(Spell spell in auraCasterGroup2)
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
      foreach(Spell spell in auraCasterGroup2)
        spell.AuraCasterGroup = auraCasterGroup2;
      return auraCasterGroup2;
    }
  }
}