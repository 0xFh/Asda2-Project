using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Targeting;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Spells
{
  [Serializable]
  public class SpellEffect
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    private static readonly ImplicitSpellTargetType[] NoTargetTypes = new ImplicitSpellTargetType[9]
    {
      ImplicitSpellTargetType.None,
      ImplicitSpellTargetType.CaliriEggs,
      ImplicitSpellTargetType.DynamicObject,
      ImplicitSpellTargetType.GameObject,
      ImplicitSpellTargetType.GameObjectOrItem,
      ImplicitSpellTargetType.HeartstoneLocation,
      ImplicitSpellTargetType.ScriptedGameObject,
      ImplicitSpellTargetType.ScriptedLocation,
      ImplicitSpellTargetType.ScriptedObjectLocation
    };

    public static HashSet<AuraType> ProcAuraTypes = new HashSet<AuraType>
    {
      AuraType.ProcTriggerSpell,
      AuraType.ProcTriggerDamage,
      AuraType.ProcTriggerSpellWithOverride
    };

    private static readonly Type[] SpellEffectMiscValueTypes = new Type[250];
    private static readonly Type[] AuraEffectMiscValueTypes = new Type[500];
    private static readonly Type[] SpellEffectMiscValueBTypes = new Type[250];
    private static readonly Type[] AuraEffectMiscValueBTypes = new Type[500];

    private static readonly HashSet<ImplicitSpellTargetType> TargetAreaEffects =
      new HashSet<ImplicitSpellTargetType>();

    private static readonly HashSet<ImplicitSpellTargetType> AreaEffects = new HashSet<ImplicitSpellTargetType>();

    /// <summary>
    /// Multi purpose.
    /// 1. If it is a proc effect, determines set of spells that can proc this proc (use <see cref="M:WCell.RealmServer.Spells.SpellEffect.AddToAffectMask(WCell.Constants.Spells.SpellLineId[])" />)
    /// 2. If it is a modifier effect, determines set of spells to be affected by this effect
    /// 3. Ignored in some cases
    /// 4. Special applications in some cases
    /// </summary>
    [Persistent(3)]public uint[] AffectMask = new uint[3];

    public EffectIndex EffectIndex = EffectIndex.Custom;

    /// <summary>SpellEffectNames.dbc - no longer included in mpqs</summary>
    public SpellEffectType EffectType;

    /// <summary>Random value max (BaseDice to DiceSides)</summary>
    public int DiceSides;

    public float RealPointsPerLevel;

    /// <summary>
    /// Base value
    /// Value = BasePoints + rand(BaseDice, DiceSides)
    /// </summary>
    public int BasePoints;

    public SpellMechanic Mechanic;
    public ImplicitSpellTargetType ImplicitTargetA;
    public ImplicitSpellTargetType ImplicitTargetB;

    /// <summary>
    /// SpellRadius.dbc
    /// Is always at least 5y.
    /// If area-related spells dont have a radius we just look for very close targets
    /// </summary>
    public float Radius;

    /// <summary>SpellAuraNames.dbc - no longer included in mpqs</summary>
    public AuraType AuraType;

    /// <summary>Interval-delay in milliseconds</summary>
    public int Amplitude;

    /// <summary>$e1/2/3 in Description</summary>
    public float ProcValue;

    public int ChainTargets;

    /// <summary>
    /// 
    /// </summary>
    public uint ItemId;

    public int MiscValue;
    public int MiscValueB;
    public int MiscValueC;

    /// <summary>
    /// Not set during InitializationPass 2, so
    /// for fixing things, use GetTriggerSpell() instead.
    /// </summary>
    [NotPersistent]public Spell TriggerSpell;

    public SpellId TriggerSpellId;

    /// <summary>
    /// 
    /// </summary>
    public float PointsPerComboPoint;

    /// <summary>
    /// Factor of the amount of AP to be added to the EffectValue (1.0f = +100%)
    /// </summary>
    public float APValueFactor;

    /// <summary>
    /// Amount of Spell Power to be added to the EffectValue in % (1 = +1%)
    /// </summary>
    public int SpellPowerValuePct;

    /// <summary>
    /// Factor of the amount of AP to be added to the EffectValue per combo point
    /// </summary>
    public float APPerComboPointValueFactor;

    /// <summary>
    /// Only use this effect if the caster is in the given form (if given)
    /// </summary>
    public ShapeshiftMask RequiredShapeshiftMask;

    /// <summary>
    /// If set, it will use the SpellEffect that triggered or proc'ed this SpellEffect (if any)
    /// instead of this one.
    /// </summary>
    public bool OverrideEffectValue;

    [NotPersistent]public SpellEffectHandlerCreator SpellEffectHandlerCreator;
    [NotPersistent]public AuraEffectHandlerCreator AuraEffectHandlerCreator;

    /// <summary>
    /// Explicitely defined spells that are somehow related to this effect.
    /// Is used for procs, talent-modifiers and AddTargetTrigger-relations mostly.
    /// Can be used for other things.
    /// </summary>
    [NotPersistent]public HashSet<Spell> AffectSpellSet;

    [NotPersistent]public Spell[] RequiredActivationAuras;

    /// <summary>
    /// If the caster has the spell of the EffectValueOverrideEffect it uses it for EffectValue calculation.
    /// If not it uses this Effect's original value.
    /// </summary>
    public SpellEffect EffectValueOverrideEffect;

    /// <summary>Used to determine the targets for this effect</summary>
    public TargetDefinition CustomTargetHandlerDefintion;

    /// <summary>Used only by AI to determine targets</summary>
    public TargetDefinition AITargetHandlerDefintion;

    /// <summary>Evaluates targets for non-AI spell casts</summary>
    public TargetEvaluator CustomTargetEvaluator;

    /// <summary>Evaluates targets for AI spell casts</summary>
    public TargetEvaluator AITargetEvaluator;

    /// <summary>The spell to which this effect belongs</summary>
    [NotPersistent]public Spell Spell;

    [NotPersistent]public int ValueMin;
    [NotPersistent]public int ValueMax;
    [NotPersistent]public bool IsAuraEffect;

    /// <summary>Applies to targets in a specific area</summary>
    [NotPersistent]public bool IsAreaEffect;

    /// <summary>Whether this requires the caster to target the area</summary>
    [NotPersistent]public bool IsTargetAreaEffect;

    [NotPersistent]public bool HasSingleTarget;

    /// <summary>Applies to targets in a specific area</summary>
    [NotPersistent]public bool IsAreaAuraEffect;

    /// <summary>Summons something</summary>
    [NotPersistent]public bool IsSummon;

    /// <summary>
    /// Whether it happens multiple times (certain Auras or channeled effects)
    /// </summary>
    [NotPersistent]public bool IsPeriodic;

    /// <summary>Probably useless</summary>
    [NotPersistent]public bool _IsPeriodicAura;

    /// <summary>Whether this effect has actual Objects as targets</summary>
    [NotPersistent]public bool HasTargets;

    /// <summary>Whether this is a heal-effect</summary>
    [NotPersistent]public bool IsHealEffect;

    /// <summary>Whether this is a damage effect</summary>
    [NotPersistent]public bool IsDamageEffect;

    /// <summary>Whether this Effect is triggered by Procs</summary>
    [NotPersistent]public bool IsProc;

    /// <summary>Harmful, neutral or beneficial</summary>
    [NotPersistent]public HarmType HarmType;

    /// <summary>
    /// Whether this effect gives a flat bonus to your strike's damage
    /// </summary>
    [NotPersistent]public bool IsStrikeEffectFlat;

    /// <summary>
    /// Whether this effect gives a percent bonus to your strike's damage
    /// </summary>
    [NotPersistent]public bool IsStrikeEffectPct;

    /// <summary>Wheter this Effect enchants an Item</summary>
    public bool IsEnchantmentEffect;

    /// <summary>
    /// All set bits of the MiscValue field.
    /// This is useful for all SpellEffects whose MiscValue is a flag field.
    /// </summary>
    [NotPersistent]public uint[] MiscBitSet;

    /// <summary>Set to the actual (min) EffectValue</summary>
    [NotPersistent]public int MinValue;

    /// <summary>Whether this effect boosts other Spells</summary>
    [NotPersistent]public bool IsEnhancer;

    /// <summary>Whether this Effect summons a Totem</summary>
    [NotPersistent]public bool IsTotem;

    public bool HasAffectMask;
    public bool IsModifierEffect;

    /// <summary>
    /// 
    /// </summary>
    public uint[] AffectMaskBitSet;

    /// <summary>Returns the max amount of ticks of this Effect</summary>
    public int GetMaxTicks()
    {
      if(Amplitude == 0)
        return 0;
      return Spell.Durations.Max / Amplitude;
    }

    public Spell GetTriggerSpell()
    {
      Spell spell = SpellHandler.Get(TriggerSpellId);
      if(spell == null && ContentMgr.ForceDataPresence)
        throw new ContentException("Spell {0} does not have a valid TriggerSpellId: {1}", (object) this,
          (object) TriggerSpellId);
      return spell;
    }

    public bool IsDependentOnOtherAuras
    {
      get { return RequiredActivationAuras != null; }
    }

    public TargetDefinition GetTargetDefinition()
    {
      return CustomTargetHandlerDefintion;
    }

    public TargetEvaluator GetTargetEvaluator(bool isAiCast)
    {
      if(!isAiCast || CustomTargetEvaluator != null)
        return CustomTargetEvaluator;
      return AITargetEvaluator;
    }

    public void SetCustomTargetDefinition(TargetAdder adder, params TargetFilter[] filters)
    {
      CustomTargetHandlerDefintion = new TargetDefinition(adder, filters);
    }

    public void SetCustomTargetDefinition(TargetAdder adder, TargetEvaluator eval, params TargetFilter[] filters)
    {
      CustomTargetHandlerDefintion = new TargetDefinition(adder, filters);
      if(eval == null)
        return;
      CustomTargetEvaluator = eval;
    }

    public void SetAITargetDefinition(TargetAdder adder, params TargetFilter[] filters)
    {
      AITargetHandlerDefintion = new TargetDefinition(adder, filters);
    }

    public void SetAITargetDefinition(TargetAdder adder, TargetEvaluator eval, params TargetFilter[] filters)
    {
      AITargetHandlerDefintion = new TargetDefinition(adder, filters);
      if(eval == null)
        return;
      CustomTargetEvaluator = eval;
    }

    /// <summary>
    /// Whether this is an effect that applies damage on strike
    /// </summary>
    public bool IsStrikeEffect
    {
      get
      {
        if(!IsStrikeEffectFlat)
          return IsStrikeEffectPct;
        return true;
      }
    }

    public bool HasAffectingSpells
    {
      get
      {
        if(!HasAffectMask)
          return AffectSpellSet != null;
        return true;
      }
    }

    /// <summary>
    /// Whether this spell effect (probably needs special handling)
    /// </summary>
    [NotPersistent]
    public bool IsScripted
    {
      get
      {
        if(EffectType != SpellEffectType.Dummy)
          return EffectType == SpellEffectType.ScriptEffect;
        return true;
      }
    }

    /// <summary>Only valid for SpellEffects of type Summon</summary>
    public SpellSummonEntry SummonEntry
    {
      get
      {
        if(EffectType != SpellEffectType.Summon || MiscValueB == 0)
          return null;
        return SpellHandler.GetSummonEntry((SummonType) MiscValueB);
      }
    }

    /// <summary>
    /// All specific SpellLines that are affected by this SpellEffect
    /// </summary>
    public HashSet<SpellLine> AffectedLines
    {
      get
      {
        HashSet<SpellLine> set = new HashSet<SpellLine>();
        if(Spell.ClassId != ClassId.NoClass)
        {
          HashSet<SpellLine> affectedSpellLines =
            SpellHandler.GetAffectedSpellLines(Spell.ClassId, AffectMask);
          set.AddRange(affectedSpellLines);
        }

        if(AffectSpellSet != null)
        {
          IEnumerable<SpellLine> elements = AffectSpellSet
            .Select(spell => spell.Line).Distinct();
          set.AddRange(elements);
        }

        return set;
      }
    }

    public SpellEffect()
    {
    }

    public SpellEffect(Spell spell, EffectIndex index)
    {
      Spell = spell;
      EffectIndex = index;
    }

    internal void Init2()
    {
      ValueMin = BasePoints + 1;
      ValueMax = BasePoints + DiceSides;
      IsTargetAreaEffect = TargetAreaEffects.Contains(ImplicitTargetA) ||
                           TargetAreaEffects.Contains(ImplicitTargetB);
      if(AreaEffects.Contains(ImplicitTargetA))
      {
        IsAreaEffect = true;
        if(ImplicitTargetB != ImplicitSpellTargetType.None &&
           AreaEffects.Contains(ImplicitTargetB))
          ImplicitTargetB = ImplicitSpellTargetType.None;
      }
      else if(ImplicitTargetB != ImplicitSpellTargetType.None &&
              AreaEffects.Contains(ImplicitTargetB))
      {
        IsAreaEffect = true;
        ImplicitTargetA = ImplicitSpellTargetType.None;
      }

      if(IsPeriodic = Amplitude > 0)
        _IsPeriodicAura = AuraType == AuraType.PeriodicDamage ||
                          AuraType == AuraType.PeriodicDamagePercent ||
                          (AuraType == AuraType.PeriodicEnergize ||
                           AuraType == AuraType.PeriodicHeal) ||
                          (AuraType == AuraType.PeriodicHealthFunnel ||
                           AuraType == AuraType.PeriodicLeech ||
                           AuraType == AuraType.PeriodicManaLeech) ||
                          AuraType == AuraType.PeriodicTriggerSpell;
      if(Spell.IsPassive)
      {
        HarmType = HarmType.Beneficial;
      }
      else
      {
        if(!HasTarget(ImplicitSpellTargetType.AllEnemiesAroundCaster,
          ImplicitSpellTargetType.AllEnemiesInArea, ImplicitSpellTargetType.AllEnemiesInAreaChanneled,
          ImplicitSpellTargetType.AllEnemiesInAreaInstant, ImplicitSpellTargetType.CurrentSelection))
        {
          if(!HasTarget(ImplicitSpellTargetType.InFrontOfCaster,
            ImplicitSpellTargetType.InvisibleOrHiddenEnemiesAtLocationRadius,
            ImplicitSpellTargetType.LocationInFrontCaster,
            ImplicitSpellTargetType.NetherDrakeSummonLocation,
            ImplicitSpellTargetType.SelectedEnemyChanneled,
            ImplicitSpellTargetType.SelectedEnemyDeadlyPoison, ImplicitSpellTargetType.SingleEnemy,
            ImplicitSpellTargetType.SpreadableDesease, ImplicitSpellTargetType.TargetAtOrientationOfCaster))
            goto label_13;
        }

        if(!HasTarget(ImplicitSpellTargetType.Self, ImplicitSpellTargetType.AllFriendlyInAura,
             ImplicitSpellTargetType.AllParty, ImplicitSpellTargetType.AllPartyAroundCaster,
             ImplicitSpellTargetType.AllPartyInArea, ImplicitSpellTargetType.PartyAroundCaster,
             ImplicitSpellTargetType.AllPartyInAreaChanneled) || Spell.Mechanic.IsNegative())
        {
          HarmType = HarmType.Harmful;
          goto label_15;
        }

        label_13:
        if(!HasTarget(ImplicitSpellTargetType.Duel) &&
           (ImplicitTargetA != ImplicitSpellTargetType.None ||
            ImplicitTargetB != ImplicitSpellTargetType.None))
          HarmType = HarmType.Beneficial;
      }

      label_15:
      if(AuraType == AuraType.ModManaRegen && Amplitude == 0)
        Amplitude = ModManaRegenHandler.DefaultAmplitude;
      if(HasTarget(ImplicitSpellTargetType.AllFriendlyInAura))
      {
        ImplicitTargetA = ImplicitSpellTargetType.AllFriendlyInAura;
        ImplicitTargetB = ImplicitSpellTargetType.None;
      }

      HasTargets =
        !NoTargetTypes.Contains(
          ImplicitTargetA) ||
        !NoTargetTypes.Contains(
          ImplicitTargetB);
      HasSingleTarget = HasTargets && !IsAreaEffect;
      IsAreaAuraEffect = EffectType == SpellEffectType.PersistantAreaAura ||
                         EffectType == SpellEffectType.ApplyAreaAura ||
                         EffectType == SpellEffectType.ApplyRaidAura;
      if(EffectType == SpellEffectType.ApplyRaidAura)
        ImplicitTargetA = (double) Radius <= 0.0
          ? ImplicitSpellTargetType.AllParty
          : ImplicitSpellTargetType.AllPartyInArea;
      IsAuraEffect = IsAreaAuraEffect || EffectType == SpellEffectType.ApplyAura ||
                     (EffectType == SpellEffectType.ApplyAuraToMaster ||
                      EffectType == SpellEffectType.ApplyPetAura) ||
                     EffectType == SpellEffectType.ApplyStatAura ||
                     EffectType == SpellEffectType.ApplyStatAuraPercent;
      IsEnhancer = IsAuraEffect &&
                   (AuraType == AuraType.AddModifierFlat ||
                    AuraType == AuraType.AddModifierPercent);
      if(MiscValueType == typeof(DamageSchoolMask))
        MiscValue &= sbyte.MaxValue;
      MiscBitSet = MiscValue > 0 ? Utility.GetSetIndices((uint) MiscValue) : new uint[0];
      MinValue = BasePoints;
      IsStrikeEffectFlat = EffectType == SpellEffectType.WeaponDamage ||
                           EffectType == SpellEffectType.WeaponDamageNoSchool ||
                           EffectType == SpellEffectType.NormalizedWeaponDamagePlus;
      IsStrikeEffectPct = EffectType == SpellEffectType.WeaponPercentDamage;
      IsTotem = HasTarget(ImplicitSpellTargetType.TotemAir) ||
                HasTarget(ImplicitSpellTargetType.TotemEarth) ||
                HasTarget(ImplicitSpellTargetType.TotemFire) ||
                HasTarget(ImplicitSpellTargetType.TotemWater);
      IsProc = IsProc || ProcAuraTypes.Contains(AuraType);
      OverrideEffectValue =
        OverrideEffectValue || AuraType == AuraType.ProcTriggerSpellWithOverride;
      IsHealEffect = EffectType == SpellEffectType.Heal ||
                     EffectType == SpellEffectType.HealMaxHealth ||
                     AuraType == AuraType.PeriodicHeal ||
                     TriggerSpell != null && TriggerSpell.IsHealSpell;
      IsDamageEffect = EffectType == SpellEffectType.SchoolDamage || IsStrikeEffect;
      IsModifierEffect =
        AuraType == AuraType.AddModifierFlat || AuraType == AuraType.AddModifierPercent;
      HasAffectMask =
        AffectMask.Any(mask => mask != 0U);
      if(HasAffectMask)
        AffectMaskBitSet = Utility.GetSetIndices(AffectMask);
      if(SpellEffectHandlerCreator == null)
        SpellEffectHandlerCreator = SpellHandler.SpellEffectCreators[(int) EffectType];
      if(IsAuraEffect && AuraEffectHandlerCreator == null)
      {
        AuraEffectHandlerCreator = AuraHandler.EffectHandlers[(int) AuraType];
        if(AuraEffectHandlerCreator == null)
          AuraEffectHandlerCreator = AuraHandler.EffectHandlers[0];
      }

      RepairBrokenTargetPairs();
      IsEnchantmentEffect = EffectType == SpellEffectType.EnchantHeldItem ||
                            EffectType == SpellEffectType.EnchantItem ||
                            EffectType == SpellEffectType.EnchantItemTemporary;
      AISpellUtil.DecideDefaultTargetHandlerDefintion(this);
    }

    /// <summary>
    /// More or less frequently occuring target pairs that make no sense
    /// </summary>
    private void RepairBrokenTargetPairs()
    {
      if(ImplicitTargetA != ImplicitSpellTargetType.Self ||
         ImplicitTargetB != ImplicitSpellTargetType.Duel)
        return;
      ImplicitTargetA = ImplicitSpellTargetType.None;
    }

    /// <summary>
    /// Whether this effect can share targets with the given effect
    /// </summary>
    public bool SharesTargetsWith(SpellEffect b)
    {
      TargetDefinition targetDefinition = GetTargetDefinition();
      if(targetDefinition != null && targetDefinition.Equals(b.GetTargetDefinition()))
        return true;
      if(ImplicitTargetA == b.ImplicitTargetA)
        return ImplicitTargetB == b.ImplicitTargetB;
      return false;
    }

    public bool HasTarget(ImplicitSpellTargetType target)
    {
      if(ImplicitTargetA != target)
        return ImplicitTargetB == target;
      return true;
    }

    public bool HasTarget(params ImplicitSpellTargetType[] targets)
    {
      return targets.FirstOrDefault(
               HasTarget) != ImplicitSpellTargetType.None;
    }

    public void CopyValuesTo(SpellEffect effect)
    {
      effect.BasePoints = BasePoints;
      effect.DiceSides = DiceSides;
    }

    /// <summary>
    /// Adds a set of Auras of which at least one need to be active for this SpellEffect to activate
    /// </summary>
    public void AddRequiredActivationAuras(params SpellLineId[] lines)
    {
      foreach(SpellLineId line in lines)
        AddRequiredActivationAuras(line.GetLine().ToArray());
    }

    public void AddRequiredActivationAuras(params SpellId[] ids)
    {
      Spell[] spellArray = new Spell[ids.Length];
      for(int index = 0; index < ids.Length; ++index)
      {
        SpellId id = ids[index];
        Spell spell = SpellHandler.Get(id);
        if(spell == null)
          throw new ArgumentException("Invalid spell in AddRequiredActivationAuras: " + id);
        spellArray[index] = spell;
      }

      AddRequiredActivationAuras(spellArray);
    }

    public void AddRequiredActivationAuras(params Spell[] spells)
    {
      if(RequiredActivationAuras == null)
        RequiredActivationAuras = spells;
      else
        ArrayUtil.Concat(ref RequiredActivationAuras, spells);
    }

    public AuraEffectHandler CreateAuraEffectHandler(ObjectReference caster, Unit target,
      ref SpellFailedReason failedReason)
    {
      return CreateAuraEffectHandler(caster, target, ref failedReason, null);
    }

    internal AuraEffectHandler CreateAuraEffectHandler(ObjectReference caster, Unit target,
      ref SpellFailedReason failedReason, SpellCast triggeringCast)
    {
      AuraEffectHandler auraEffectHandler = AuraEffectHandlerCreator();
      if(triggeringCast != null && triggeringCast.TriggerEffect != null &&
         triggeringCast.TriggerEffect.OverrideEffectValue)
      {
        if(Spell.Effects.Length > 1)
          log.Warn(
            "Spell {0} had overriding SpellEffect although the spell that was triggered had {2} (> 1) effects",
            Spell, Spell.Effects.Length);
        auraEffectHandler.m_spellEffect = triggeringCast.TriggerEffect;
      }
      else
        auraEffectHandler.m_spellEffect = this;

      auraEffectHandler.BaseEffectValue = CalcEffectValue(caster);
      auraEffectHandler.CheckInitialize(triggeringCast, caster, target, ref failedReason);
      return auraEffectHandler;
    }

    public int CalcEffectValue(ObjectReference casterReference)
    {
      Unit unitMaster = casterReference.UnitMaster;
      if(unitMaster != null)
        return CalcEffectValue(unitMaster);
      return CalcEffectValue(casterReference.Level, 0, false);
    }

    public int CalcEffectValue(Unit caster)
    {
      int num = caster == null
        ? CalcEffectValue(1, 0, false)
        : CalcEffectValue(caster.Level, caster.ComboPoints, true);
      return CalcEffectValue(caster, num);
    }

    public int CalcEffectValue(Unit caster, int value)
    {
      if(EffectValueOverrideEffect != null &&
         caster.Spells.Contains(EffectValueOverrideEffect.Spell))
        return EffectValueOverrideEffect.CalcEffectValue(caster, value);
      if(caster == null)
        return value;
      if(APValueFactor != 0.0 || APPerComboPointValueFactor != 0.0)
      {
        float num1 = APValueFactor + APPerComboPointValueFactor * caster.ComboPoints;
        int num2 = Spell.IsRanged ? caster.TotalRangedAP : caster.TotalMeleeAP;
        value += (int) (num2 * (double) num1 + 0.5);
      }

      if(caster is Character && SpellPowerValuePct != 0)
        value += (SpellPowerValuePct * caster.GetDamageDoneMod(Spell.Schools[0]) + 50) / 100;
      SpellModifierType type;
      switch(EffectIndex)
      {
        case EffectIndex.Zero:
          type = SpellModifierType.EffectValue1;
          break;
        case EffectIndex.One:
          type = SpellModifierType.EffectValue2;
          break;
        case EffectIndex.Two:
          type = SpellModifierType.EffectValue3;
          break;
        default:
          type = SpellModifierType.EffectValue4AndBeyond;
          break;
      }

      value = caster.Auras.GetModifiedInt(type, Spell, value);
      value = caster.Auras.GetModifiedInt(SpellModifierType.AllEffectValues, Spell, value);
      return value;
    }

    public int CalcEffectValue()
    {
      return CalcEffectValue(0, 0, false);
    }

    public int CalcEffectValue(int level, int comboPoints, bool useOverride)
    {
      if(EffectValueOverrideEffect != null && useOverride)
        return EffectValueOverrideEffect.CalcEffectValue(level, comboPoints, false);
      if(BasePoints == 0)
        BasePoints = MiscValue;
      return BasePoints;
    }

    public int GetMultipliedValue(Unit caster, int val, int currentTargetNo)
    {
      int effectIndex = (int) EffectIndex;
      if(effectIndex >= Spell.DamageMultipliers.Length || effectIndex < 0 || currentTargetNo == 0)
        return val;
      float num = Spell.DamageMultipliers[effectIndex];
      if(caster != null)
        num = caster.Auras.GetModifiedFloat(SpellModifierType.ChainValueFactor, Spell, num);
      if(num != 1.0)
        return val = MathUtil.RoundInt((float) Math.Pow(num, currentTargetNo) * val);
      return val;
    }

    public float GetRadius(ObjectReference caster)
    {
      float num = Radius;
      Unit unitMaster = caster.UnitMaster;
      if(unitMaster != null)
        num = unitMaster.Auras.GetModifiedFloat(SpellModifierType.Radius, Spell, num);
      return num;
    }

    public void ClearAffectMask()
    {
      AffectMask = new uint[3];
    }

    public void SetAffectMask(params SpellLineId[] abilities)
    {
      ClearAffectMask();
      AddToAffectMask(abilities);
    }

    /// <summary>
    /// Adds a set of spells to the explicite relationship set of this effect, which is used to determine whether
    /// a certain Spell and this effect have some kind of influence on one another (for procs, talent modifiers etc).
    /// Only adds the spells, will not work on the spells' trigger spells.
    /// </summary>
    /// <param name="abilities"></param>
    public void AddAffectingSpells(params SpellLineId[] abilities)
    {
      if(AffectSpellSet == null)
        AffectSpellSet = new HashSet<Spell>();
      foreach(SpellLineId ability in abilities)
        AffectSpellSet.AddRange(ability.GetLine());
    }

    /// <summary>
    /// Adds a set of spells to the explicite relationship set of this effect, which is used to determine whether
    /// a certain Spell and this effect have some kind of influence on one another (for procs, talent modifiers etc).
    /// Only adds the spells, will not work on the spells' trigger spells.
    /// </summary>
    /// <param name="abilities"></param>
    public void AddAffectingSpells(params SpellId[] spells)
    {
      if(AffectSpellSet == null)
        AffectSpellSet = new HashSet<Spell>();
      foreach(SpellId spell in spells)
        AffectSpellSet.Add(SpellHandler.Get(spell));
    }

    /// <summary>
    /// Adds a set of spells to this Effect's AffectMask, which is used to determine whether
    /// a certain Spell and this effect have some kind of influence on one another (for procs, talent modifiers etc).
    /// Usually the mask also contains any spell that is triggered by the original spell.
    /// 
    /// If you get a warning that the wrong set is affected, use AddAffectingSpells instead.
    /// </summary>
    public void AddToAffectMask(params SpellLineId[] abilities)
    {
      uint[] mask = new uint[3];
      if(abilities.Length != 1)
      {
        foreach(SpellLineId ability in abilities)
        {
          Spell firstRank = ability.GetLine().FirstRank;
          for(int index = 0; index < 3; ++index)
            mask[index] |= firstRank.SpellClassMask[index];
        }
      }
      else
        abilities[0].GetLine().FirstRank.SpellClassMask.CopyTo(mask, 0);

      HashSet<SpellLine> affectedSpellLines = SpellHandler.GetAffectedSpellLines(Spell.ClassId, mask);
      if(affectedSpellLines.Count != abilities.Length)
        LogManager.GetCurrentClassLogger().Warn(
          "[SPELL Inconsistency for {0}] Invalid affect mask affects a different set than the one intended: {1} (intended: {2}) - You might want to use AddAffectingSpells instead!",
          Spell, affectedSpellLines.ToString(", "),
          abilities.ToString(", "));
      for(int index = 0; index < 3; ++index)
        AffectMask[index] |= mask[index];
    }

    public void CopyAffectMaskTo(uint[] mask)
    {
      for(int index = 0; index < AffectMask.Length; ++index)
        mask[index] |= AffectMask[index];
    }

    public void RemoveAffectMaskFrom(uint[] mask)
    {
      for(int index = 0; index < AffectMask.Length; ++index)
        mask[index] ^= AffectMask[index];
    }

    public bool MatchesSpell(Spell spell)
    {
      if(spell.SpellClassSet == Spell.SpellClassSet && spell.MatchesMask(AffectMask))
        return true;
      if(AffectSpellSet != null)
        return AffectSpellSet.Contains(spell);
      return false;
    }

    public void MakeProc(AuraEffectHandlerCreator creator, params SpellLineId[] exclusiveTriggers)
    {
      IsProc = true;
      ClearAffectMask();
      AddAffectingSpells(exclusiveTriggers);
      AuraEffectHandlerCreator = creator;
    }

    /// <summary>
    /// Uses the AffectMask, rather than exclusive trigger spells. This is important if also spells
    /// that are triggerd by the triggered spells are allowed to trigger this proc.
    /// </summary>
    public void MakeProcWithMask(AuraEffectHandlerCreator creator, params SpellLineId[] exclusiveTriggers)
    {
      IsProc = true;
      SetAffectMask(exclusiveTriggers);
      AuraEffectHandlerCreator = creator;
    }

    public bool CanProcBeTriggeredBy(Spell spell)
    {
      if(spell != null && HasAffectingSpells)
        return MatchesSpell(spell);
      return true;
    }

    public void DumpInfo(TextWriter writer, string indent)
    {
      writer.WriteLine(indent + "Effect: " + this);
      indent += "\t";
      if(ImplicitTargetA != ImplicitSpellTargetType.None)
        writer.WriteLine(indent + "ImplicitTargetA: {0}", ImplicitTargetA);
      if(ImplicitTargetB != ImplicitSpellTargetType.None)
        writer.WriteLine(indent + "ImplicitTargetB: {0}", ImplicitTargetB);
      if(MiscValue != 0 || MiscValueType != null)
        writer.WriteLine(indent + "MiscValue: " + GetMiscStr(MiscValueType, MiscValue));
      if(MiscValueB != 0)
        writer.WriteLine(indent + "MiscValueB: " + GetMiscStr(MiscValueBType, MiscValueB));
      HashSet<SpellLine> affectedLines = AffectedLines;
      if(affectedLines.Count > 0)
        writer.WriteLine(indent + "Affects: {0}", affectedLines.ToString(", "));
      else if(AffectMask[0] != 0U || AffectMask[1] != 0U || AffectMask[2] != 0U)
        writer.WriteLine(indent + "Affects: <Nothing> ({0}{1}{2})", AffectMask[0].ToString("X8"),
          AffectMask[1].ToString("X8"), AffectMask[2].ToString("X8"));
      if(BasePoints != 0)
        writer.WriteLine(indent + "BasePoints: {0}", BasePoints);
      if(DiceSides != 0)
        writer.WriteLine(indent + "DiceSides: {0}", DiceSides);
      if(Amplitude != 0)
        writer.WriteLine(indent + "Amplitude: {0}", Amplitude);
      if(ChainTargets != 0)
        writer.WriteLine(indent + "ChainTarget: {0}", ChainTargets);
      if(ItemId != 0U)
        writer.WriteLine(indent + "ItemId: {0} ({1})", (Asda2ItemId) ItemId,
          ItemId);
      if(PointsPerComboPoint != 0.0)
        writer.WriteLine(indent + "PointsPerComboPoint: {0}", PointsPerComboPoint);
      if(ProcValue != 0.0)
        writer.WriteLine(indent + "ProcValue: {0}", ProcValue);
      if(Radius != 0.0)
        writer.WriteLine(indent + "Radius: {0}", Radius);
      if(RealPointsPerLevel != 0.0)
        writer.WriteLine(indent + "RealPointsPerLevel: {0}", RealPointsPerLevel);
      if(Mechanic != SpellMechanic.None)
        writer.WriteLine(indent + "Mechanic: {0}", Mechanic);
      if(TriggerSpellId != SpellId.None)
        writer.WriteLine(indent + "Triggers: {0} ({1})", TriggerSpellId,
          TriggerSpellId);
      SpellSummonEntry summonEntry = SummonEntry;
      if(summonEntry == null)
        return;
      writer.WriteLine(indent + "Summon information:");
      indent += "\t";
      writer.WriteLine(indent + "Summon ID: {0}", summonEntry.Id);
      if(summonEntry.Group != SummonGroup.Wild)
        writer.WriteLine(indent + "Summon Group: {0}", summonEntry.Group);
      if(summonEntry.FactionTemplateId != FactionTemplateId.None)
        writer.WriteLine(indent + "Summon Faction: {0}", summonEntry.FactionTemplateId);
      if(summonEntry.Type != SummonPropertyType.None)
        writer.WriteLine(indent + "Summon Type: {0}", summonEntry.Type);
      if(summonEntry.Flags != SummonFlags.None)
        writer.WriteLine(indent + "Summon Flags: {0}", summonEntry.Flags);
      if(summonEntry.Slot == 0U)
        return;
      writer.WriteLine(indent + "Summon Slot: {0}", summonEntry.Slot);
    }

    public string GetTargetString()
    {
      int num = 0;
      List<string> stringList = new List<string>(2);
      if(ImplicitTargetA != ImplicitSpellTargetType.None)
      {
        ++num;
        stringList.Add("A: " + ImplicitTargetA);
      }

      if(ImplicitTargetB != ImplicitSpellTargetType.None)
      {
        ++num;
        stringList.Add("B: " + ImplicitTargetB);
      }

      if(stringList.Count <= 0)
        return "Targets: None";
      return "Targets (" + num + ") - " +
             stringList.ToArray().ToString(", ");
    }

    private string GetMiscStr(Type type, int val)
    {
      object obj = null;
      if(type != null && StringParser.Parse(val.ToString(), type, ref obj))
        return string.Format("{0} ({1})", obj, val);
      return val.ToString();
    }

    public static Type GetSpellEffectEffectMiscValueType(SpellEffectType type)
    {
      if(type < SpellEffectType.End)
        return SpellEffectMiscValueTypes[(int) type];
      log.Warn("Found invalid SpellEffectType {0}.", type);
      return null;
    }

    public static void SetSpellEffectEffectMiscValueType(SpellEffectType effectType, Type type)
    {
      SpellEffectMiscValueTypes[(int) effectType] = type;
    }

    public static Type GetAuraEffectMiscValueType(AuraType type)
    {
      if(type < AuraType.End)
        return AuraEffectMiscValueTypes[(int) type];
      log.Warn("Found invalid AuraType {0}.", type);
      return null;
    }

    public static void SetAuraEffectMiscValueType(AuraType auraType, Type type)
    {
      AuraEffectMiscValueTypes[(int) auraType] = type;
    }

    public static Type GetSpellEffectEffectMiscValueBType(SpellEffectType type)
    {
      if(type < SpellEffectType.End)
        return SpellEffectMiscValueBTypes[(int) type];
      log.Warn("Found invalid SpellEffectType {0}.", type);
      return null;
    }

    public static void SetSpellEffectEffectMiscValueBType(SpellEffectType effectType, Type type)
    {
      SpellEffectMiscValueBTypes[(int) effectType] = type;
    }

    public static Type GetAuraEffectMiscValueBType(AuraType type)
    {
      if(type < AuraType.End)
        return AuraEffectMiscValueBTypes[(int) type];
      log.Warn("Found invalid AuraType {0}.", type);
      return null;
    }

    public static void SetAuraEffectMiscValueBType(AuraType auraType, Type type)
    {
      AuraEffectMiscValueBTypes[(int) auraType] = type;
    }

    internal static void InitMiscValueTypes()
    {
      AuraEffectMiscValueTypes[108] = typeof(SpellModifierType);
      AuraEffectMiscValueTypes[107] = typeof(SpellModifierType);
      SetAuraEffectMiscValueType(AuraType.ModDamageDone, typeof(DamageSchoolMask));
      SetAuraEffectMiscValueType(AuraType.ModDamageDonePercent, typeof(DamageSchoolMask));
      SetAuraEffectMiscValueType(AuraType.ModDamageDoneToCreatureType, typeof(DamageSchoolMask));
      SetAuraEffectMiscValueType(AuraType.ModDamageDoneVersusCreatureType, typeof(CreatureMask));
      SetAuraEffectMiscValueType(AuraType.ModDamageTaken, typeof(DamageSchoolMask));
      SetAuraEffectMiscValueType(AuraType.ModDamageTakenPercent, typeof(DamageSchoolMask));
      SetAuraEffectMiscValueType(AuraType.ModPowerCost, typeof(PowerType));
      SetAuraEffectMiscValueType(AuraType.ModPowerCostForSchool, typeof(PowerType));
      SetAuraEffectMiscValueType(AuraType.ModPowerRegen, typeof(PowerType));
      SetAuraEffectMiscValueType(AuraType.ModPowerRegenPercent, typeof(PowerType));
      SetAuraEffectMiscValueType(AuraType.ModRating, typeof(CombatRatingMask));
      SetAuraEffectMiscValueType(AuraType.ModSkill, typeof(SkillId));
      SetAuraEffectMiscValueType(AuraType.ModSkillTalent, typeof(SkillId));
      SetAuraEffectMiscValueType(AuraType.ModStat, typeof(StatType));
      SetAuraEffectMiscValueType(AuraType.ModStatPercent, typeof(StatType));
      SetAuraEffectMiscValueType(AuraType.ModTotalStatPercent, typeof(StatType));
      SetAuraEffectMiscValueType(AuraType.DispelImmunity, typeof(DispelType));
      SetAuraEffectMiscValueType(AuraType.MechanicImmunity, typeof(SpellMechanic));
      SetAuraEffectMiscValueType(AuraType.Mounted, typeof(NPCId));
      SetAuraEffectMiscValueType(AuraType.ModShapeshift, typeof(ShapeshiftForm));
      SetAuraEffectMiscValueType(AuraType.Transform, typeof(NPCId));
      SetAuraEffectMiscValueType(AuraType.ModSpellDamageByPercentOfStat, typeof(DamageSchoolMask));
      SetAuraEffectMiscValueType(AuraType.ModSpellHealingByPercentOfStat, typeof(DamageSchoolMask));
      SetAuraEffectMiscValueType(AuraType.DamagePctAmplifier, typeof(DamageSchoolMask));
      SetAuraEffectMiscValueType(AuraType.ModSilenceDurationPercent, typeof(SpellMechanic));
      SetAuraEffectMiscValueType(AuraType.ModMechanicDurationPercent, typeof(SpellMechanic));
      SetAuraEffectMiscValueType(AuraType.TrackCreatures, typeof(CreatureType));
      SetAuraEffectMiscValueType(AuraType.ModSpellHitChance, typeof(DamageSchoolMask));
      SetAuraEffectMiscValueType(AuraType.ModSpellHitChance2, typeof(DamageSchoolMask));
      SetAuraEffectMiscValueBType(AuraType.ModSpellDamageByPercentOfStat, typeof(StatType));
      SetAuraEffectMiscValueBType(AuraType.ModSpellHealingByPercentOfStat, typeof(StatType));
      SetSpellEffectEffectMiscValueType(SpellEffectType.Dispel, typeof(DispelType));
      SetSpellEffectEffectMiscValueType(SpellEffectType.DispelMechanic, typeof(SpellMechanic));
      SetSpellEffectEffectMiscValueType(SpellEffectType.Skill, typeof(SkillId));
      SetSpellEffectEffectMiscValueType(SpellEffectType.SkillStep, typeof(SkillId));
      SetSpellEffectEffectMiscValueType(SpellEffectType.Skinning, typeof(SkinningType));
      SetSpellEffectEffectMiscValueType(SpellEffectType.Summon, typeof(NPCId));
      SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObject, typeof(GOEntryId));
      SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObjectSlot1, typeof(GOEntryId));
      SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObjectSlot2, typeof(GOEntryId));
      SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObjectWild, typeof(GOEntryId));
      SetSpellEffectEffectMiscValueBType(SpellEffectType.Summon, typeof(SummonType));
      TargetAreaEffects.AddRange(
        new ImplicitSpellTargetType[7]
        {
          ImplicitSpellTargetType.AllAroundLocation,
          ImplicitSpellTargetType.AllEnemiesInArea,
          ImplicitSpellTargetType.AllEnemiesInAreaChanneled,
          ImplicitSpellTargetType.AllEnemiesInAreaInstant,
          ImplicitSpellTargetType.AllPartyInArea,
          ImplicitSpellTargetType.AllPartyInAreaChanneled,
          ImplicitSpellTargetType.InvisibleOrHiddenEnemiesAtLocationRadius
        });
      AreaEffects.AddRange(
        TargetAreaEffects);
      AreaEffects.AddRange(
        new ImplicitSpellTargetType[11]
        {
          ImplicitSpellTargetType.AllEnemiesAroundCaster,
          ImplicitSpellTargetType.AllPartyAroundCaster,
          ImplicitSpellTargetType.AllTargetableAroundLocationInRadiusOverTime,
          ImplicitSpellTargetType.BehindTargetLocation,
          ImplicitSpellTargetType.LocationInFrontCaster,
          ImplicitSpellTargetType.LocationInFrontCasterAtRange,
          ImplicitSpellTargetType.ConeInFrontOfCaster,
          ImplicitSpellTargetType.AreaEffectPartyAndClass,
          ImplicitSpellTargetType.NatureSummonLocation,
          ImplicitSpellTargetType.TargetAtOrientationOfCaster,
          ImplicitSpellTargetType.Tranquility
        });
    }

    public Type MiscValueType
    {
      get
      {
        if(IsAuraEffect)
          return GetAuraEffectMiscValueType(AuraType);
        return GetSpellEffectEffectMiscValueType(EffectType);
      }
    }

    public Type MiscValueBType
    {
      get
      {
        if(IsAuraEffect)
          return GetAuraEffectMiscValueBType(AuraType);
        return GetSpellEffectEffectMiscValueBType(EffectType);
      }
    }

    /// <summary>
    /// Get's Basepoints for a spell after applying DamageMods.
    /// </summary>
    public int GetModifiedDamage(Unit caster)
    {
      if(IsPeriodic)
        return caster.Auras.GetModifiedInt(SpellModifierType.PeriodicEffectValue, Spell,
          CalcEffectValue());
      return caster.GetFinalDamage(caster.GetLeastResistantSchool(Spell), CalcEffectValue(),
        Spell);
    }

    public override bool Equals(object obj)
    {
      if(obj is SpellEffect)
        return ((SpellEffect) obj).EffectType == EffectType;
      return false;
    }

    public override int GetHashCode()
    {
      return EffectType.GetHashCode();
    }

    public override string ToString()
    {
      return ((int) EffectType).ToString() +
             (TriggerSpell == null ? "" : (object) (" (" + TriggerSpell + ")")) +
             (AuraType == AuraType.None ? "" : (object) (" (" + AuraType + ")"));
    }
  }
}