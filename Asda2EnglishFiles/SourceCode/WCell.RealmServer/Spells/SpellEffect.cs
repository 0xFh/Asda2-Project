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

        public static HashSet<AuraType> ProcAuraTypes = new HashSet<AuraType>()
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
        [Persistent(3)] public uint[] AffectMask = new uint[3];

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
        [NotPersistent] public Spell TriggerSpell;

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

        [NotPersistent] public SpellEffectHandlerCreator SpellEffectHandlerCreator;
        [NotPersistent] public AuraEffectHandlerCreator AuraEffectHandlerCreator;

        /// <summary>
        /// Explicitely defined spells that are somehow related to this effect.
        /// Is used for procs, talent-modifiers and AddTargetTrigger-relations mostly.
        /// Can be used for other things.
        /// </summary>
        [NotPersistent] public HashSet<Spell> AffectSpellSet;

        [NotPersistent] public Spell[] RequiredActivationAuras;

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
        [NotPersistent] public Spell Spell;

        [NotPersistent] public int ValueMin;
        [NotPersistent] public int ValueMax;
        [NotPersistent] public bool IsAuraEffect;

        /// <summary>Applies to targets in a specific area</summary>
        [NotPersistent] public bool IsAreaEffect;

        /// <summary>Whether this requires the caster to target the area</summary>
        [NotPersistent] public bool IsTargetAreaEffect;

        [NotPersistent] public bool HasSingleTarget;

        /// <summary>Applies to targets in a specific area</summary>
        [NotPersistent] public bool IsAreaAuraEffect;

        /// <summary>Summons something</summary>
        [NotPersistent] public bool IsSummon;

        /// <summary>
        /// Whether it happens multiple times (certain Auras or channeled effects)
        /// </summary>
        [NotPersistent] public bool IsPeriodic;

        /// <summary>Probably useless</summary>
        [NotPersistent] public bool _IsPeriodicAura;

        /// <summary>Whether this effect has actual Objects as targets</summary>
        [NotPersistent] public bool HasTargets;

        /// <summary>Whether this is a heal-effect</summary>
        [NotPersistent] public bool IsHealEffect;

        /// <summary>Whether this is a damage effect</summary>
        [NotPersistent] public bool IsDamageEffect;

        /// <summary>Whether this Effect is triggered by Procs</summary>
        [NotPersistent] public bool IsProc;

        /// <summary>Harmful, neutral or beneficial</summary>
        [NotPersistent] public HarmType HarmType;

        /// <summary>
        /// Whether this effect gives a flat bonus to your strike's damage
        /// </summary>
        [NotPersistent] public bool IsStrikeEffectFlat;

        /// <summary>
        /// Whether this effect gives a percent bonus to your strike's damage
        /// </summary>
        [NotPersistent] public bool IsStrikeEffectPct;

        /// <summary>Wheter this Effect enchants an Item</summary>
        public bool IsEnchantmentEffect;

        /// <summary>
        /// All set bits of the MiscValue field.
        /// This is useful for all SpellEffects whose MiscValue is a flag field.
        /// </summary>
        [NotPersistent] public uint[] MiscBitSet;

        /// <summary>Set to the actual (min) EffectValue</summary>
        [NotPersistent] public int MinValue;

        /// <summary>Whether this effect boosts other Spells</summary>
        [NotPersistent] public bool IsEnhancer;

        /// <summary>Whether this Effect summons a Totem</summary>
        [NotPersistent] public bool IsTotem;

        public bool HasAffectMask;
        public bool IsModifierEffect;

        /// <summary>
        /// 
        /// </summary>
        public uint[] AffectMaskBitSet;

        /// <summary>Returns the max amount of ticks of this Effect</summary>
        public int GetMaxTicks()
        {
            if (this.Amplitude == 0)
                return 0;
            return this.Spell.Durations.Max / this.Amplitude;
        }

        public Spell GetTriggerSpell()
        {
            Spell spell = SpellHandler.Get(this.TriggerSpellId);
            if (spell == null && ContentMgr.ForceDataPresence)
                throw new ContentException("Spell {0} does not have a valid TriggerSpellId: {1}", new object[2]
                {
                    (object) this,
                    (object) this.TriggerSpellId
                });
            return spell;
        }

        public bool IsDependentOnOtherAuras
        {
            get { return this.RequiredActivationAuras != null; }
        }

        public TargetDefinition GetTargetDefinition()
        {
            return this.CustomTargetHandlerDefintion;
        }

        public TargetEvaluator GetTargetEvaluator(bool isAiCast)
        {
            if (!isAiCast || this.CustomTargetEvaluator != null)
                return this.CustomTargetEvaluator;
            return this.AITargetEvaluator;
        }

        public void SetCustomTargetDefinition(TargetAdder adder, params TargetFilter[] filters)
        {
            this.CustomTargetHandlerDefintion = new TargetDefinition(adder, filters);
        }

        public void SetCustomTargetDefinition(TargetAdder adder, TargetEvaluator eval, params TargetFilter[] filters)
        {
            this.CustomTargetHandlerDefintion = new TargetDefinition(adder, filters);
            if (eval == null)
                return;
            this.CustomTargetEvaluator = eval;
        }

        public void SetAITargetDefinition(TargetAdder adder, params TargetFilter[] filters)
        {
            this.AITargetHandlerDefintion = new TargetDefinition(adder, filters);
        }

        public void SetAITargetDefinition(TargetAdder adder, TargetEvaluator eval, params TargetFilter[] filters)
        {
            this.AITargetHandlerDefintion = new TargetDefinition(adder, filters);
            if (eval == null)
                return;
            this.CustomTargetEvaluator = eval;
        }

        /// <summary>
        /// Whether this is an effect that applies damage on strike
        /// </summary>
        public bool IsStrikeEffect
        {
            get
            {
                if (!this.IsStrikeEffectFlat)
                    return this.IsStrikeEffectPct;
                return true;
            }
        }

        public bool HasAffectingSpells
        {
            get
            {
                if (!this.HasAffectMask)
                    return this.AffectSpellSet != null;
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
                if (this.EffectType != SpellEffectType.Dummy)
                    return this.EffectType == SpellEffectType.ScriptEffect;
                return true;
            }
        }

        /// <summary>Only valid for SpellEffects of type Summon</summary>
        public SpellSummonEntry SummonEntry
        {
            get
            {
                if (this.EffectType != SpellEffectType.Summon || this.MiscValueB == 0)
                    return (SpellSummonEntry) null;
                return SpellHandler.GetSummonEntry((SummonType) this.MiscValueB);
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
                if (this.Spell.ClassId != ClassId.NoClass)
                {
                    HashSet<SpellLine> affectedSpellLines =
                        SpellHandler.GetAffectedSpellLines(this.Spell.ClassId, this.AffectMask);
                    set.AddRange<SpellLine>((IEnumerable<SpellLine>) affectedSpellLines);
                }

                if (this.AffectSpellSet != null)
                {
                    IEnumerable<SpellLine> elements = this.AffectSpellSet
                        .Select<Spell, SpellLine>((Func<Spell, SpellLine>) (spell => spell.Line)).Distinct<SpellLine>();
                    set.AddRange<SpellLine>(elements);
                }

                return set;
            }
        }

        public SpellEffect()
        {
        }

        public SpellEffect(Spell spell, EffectIndex index)
        {
            this.Spell = spell;
            this.EffectIndex = index;
        }

        internal void Init2()
        {
            this.ValueMin = this.BasePoints + 1;
            this.ValueMax = this.BasePoints + this.DiceSides;
            this.IsTargetAreaEffect = SpellEffect.TargetAreaEffects.Contains(this.ImplicitTargetA) ||
                                      SpellEffect.TargetAreaEffects.Contains(this.ImplicitTargetB);
            if (SpellEffect.AreaEffects.Contains(this.ImplicitTargetA))
            {
                this.IsAreaEffect = true;
                if (this.ImplicitTargetB != ImplicitSpellTargetType.None &&
                    SpellEffect.AreaEffects.Contains(this.ImplicitTargetB))
                    this.ImplicitTargetB = ImplicitSpellTargetType.None;
            }
            else if (this.ImplicitTargetB != ImplicitSpellTargetType.None &&
                     SpellEffect.AreaEffects.Contains(this.ImplicitTargetB))
            {
                this.IsAreaEffect = true;
                this.ImplicitTargetA = ImplicitSpellTargetType.None;
            }

            if (this.IsPeriodic = this.Amplitude > 0)
                this._IsPeriodicAura = this.AuraType == AuraType.PeriodicDamage ||
                                       this.AuraType == AuraType.PeriodicDamagePercent ||
                                       (this.AuraType == AuraType.PeriodicEnergize ||
                                        this.AuraType == AuraType.PeriodicHeal) ||
                                       (this.AuraType == AuraType.PeriodicHealthFunnel ||
                                        this.AuraType == AuraType.PeriodicLeech ||
                                        this.AuraType == AuraType.PeriodicManaLeech) ||
                                       this.AuraType == AuraType.PeriodicTriggerSpell;
            if (this.Spell.IsPassive)
            {
                this.HarmType = HarmType.Beneficial;
            }
            else
            {
                if (!this.HasTarget(ImplicitSpellTargetType.AllEnemiesAroundCaster,
                    ImplicitSpellTargetType.AllEnemiesInArea, ImplicitSpellTargetType.AllEnemiesInAreaChanneled,
                    ImplicitSpellTargetType.AllEnemiesInAreaInstant, ImplicitSpellTargetType.CurrentSelection))
                {
                    if (!this.HasTarget(ImplicitSpellTargetType.InFrontOfCaster,
                        ImplicitSpellTargetType.InvisibleOrHiddenEnemiesAtLocationRadius,
                        ImplicitSpellTargetType.LocationInFrontCaster,
                        ImplicitSpellTargetType.NetherDrakeSummonLocation,
                        ImplicitSpellTargetType.SelectedEnemyChanneled,
                        ImplicitSpellTargetType.SelectedEnemyDeadlyPoison, ImplicitSpellTargetType.SingleEnemy,
                        ImplicitSpellTargetType.SpreadableDesease, ImplicitSpellTargetType.TargetAtOrientationOfCaster))
                        goto label_13;
                }

                if (!this.HasTarget(ImplicitSpellTargetType.Self, ImplicitSpellTargetType.AllFriendlyInAura,
                        ImplicitSpellTargetType.AllParty, ImplicitSpellTargetType.AllPartyAroundCaster,
                        ImplicitSpellTargetType.AllPartyInArea, ImplicitSpellTargetType.PartyAroundCaster,
                        ImplicitSpellTargetType.AllPartyInAreaChanneled) || this.Spell.Mechanic.IsNegative())
                {
                    this.HarmType = HarmType.Harmful;
                    goto label_15;
                }

                label_13:
                if (!this.HasTarget(ImplicitSpellTargetType.Duel) &&
                    (this.ImplicitTargetA != ImplicitSpellTargetType.None ||
                     this.ImplicitTargetB != ImplicitSpellTargetType.None))
                    this.HarmType = HarmType.Beneficial;
            }

            label_15:
            if (this.AuraType == AuraType.ModManaRegen && this.Amplitude == 0)
                this.Amplitude = ModManaRegenHandler.DefaultAmplitude;
            if (this.HasTarget(ImplicitSpellTargetType.AllFriendlyInAura))
            {
                this.ImplicitTargetA = ImplicitSpellTargetType.AllFriendlyInAura;
                this.ImplicitTargetB = ImplicitSpellTargetType.None;
            }

            this.HasTargets =
                !((IEnumerable<ImplicitSpellTargetType>) SpellEffect.NoTargetTypes).Contains<ImplicitSpellTargetType>(
                    this.ImplicitTargetA) ||
                !((IEnumerable<ImplicitSpellTargetType>) SpellEffect.NoTargetTypes).Contains<ImplicitSpellTargetType>(
                    this.ImplicitTargetB);
            this.HasSingleTarget = this.HasTargets && !this.IsAreaEffect;
            this.IsAreaAuraEffect = this.EffectType == SpellEffectType.PersistantAreaAura ||
                                    this.EffectType == SpellEffectType.ApplyAreaAura ||
                                    this.EffectType == SpellEffectType.ApplyRaidAura;
            if (this.EffectType == SpellEffectType.ApplyRaidAura)
                this.ImplicitTargetA = (double) this.Radius <= 0.0
                    ? ImplicitSpellTargetType.AllParty
                    : ImplicitSpellTargetType.AllPartyInArea;
            this.IsAuraEffect = this.IsAreaAuraEffect || this.EffectType == SpellEffectType.ApplyAura ||
                                (this.EffectType == SpellEffectType.ApplyAuraToMaster ||
                                 this.EffectType == SpellEffectType.ApplyPetAura) ||
                                this.EffectType == SpellEffectType.ApplyStatAura ||
                                this.EffectType == SpellEffectType.ApplyStatAuraPercent;
            this.IsEnhancer = this.IsAuraEffect &&
                              (this.AuraType == AuraType.AddModifierFlat ||
                               this.AuraType == AuraType.AddModifierPercent);
            if (this.MiscValueType == typeof(DamageSchoolMask))
                this.MiscValue &= (int) sbyte.MaxValue;
            this.MiscBitSet = this.MiscValue > 0 ? Utility.GetSetIndices((uint) this.MiscValue) : new uint[0];
            this.MinValue = this.BasePoints;
            this.IsStrikeEffectFlat = this.EffectType == SpellEffectType.WeaponDamage ||
                                      this.EffectType == SpellEffectType.WeaponDamageNoSchool ||
                                      this.EffectType == SpellEffectType.NormalizedWeaponDamagePlus;
            this.IsStrikeEffectPct = this.EffectType == SpellEffectType.WeaponPercentDamage;
            this.IsTotem = this.HasTarget(ImplicitSpellTargetType.TotemAir) ||
                           this.HasTarget(ImplicitSpellTargetType.TotemEarth) ||
                           this.HasTarget(ImplicitSpellTargetType.TotemFire) ||
                           this.HasTarget(ImplicitSpellTargetType.TotemWater);
            this.IsProc = this.IsProc || SpellEffect.ProcAuraTypes.Contains(this.AuraType);
            this.OverrideEffectValue =
                this.OverrideEffectValue || this.AuraType == AuraType.ProcTriggerSpellWithOverride;
            this.IsHealEffect = this.EffectType == SpellEffectType.Heal ||
                                this.EffectType == SpellEffectType.HealMaxHealth ||
                                this.AuraType == AuraType.PeriodicHeal ||
                                this.TriggerSpell != null && this.TriggerSpell.IsHealSpell;
            this.IsDamageEffect = this.EffectType == SpellEffectType.SchoolDamage || this.IsStrikeEffect;
            this.IsModifierEffect =
                this.AuraType == AuraType.AddModifierFlat || this.AuraType == AuraType.AddModifierPercent;
            this.HasAffectMask =
                ((IEnumerable<uint>) this.AffectMask).Any<uint>((Func<uint, bool>) (mask => mask != 0U));
            if (this.HasAffectMask)
                this.AffectMaskBitSet = Utility.GetSetIndices(this.AffectMask);
            if (this.SpellEffectHandlerCreator == null)
                this.SpellEffectHandlerCreator = SpellHandler.SpellEffectCreators[(int) this.EffectType];
            if (this.IsAuraEffect && this.AuraEffectHandlerCreator == null)
            {
                this.AuraEffectHandlerCreator = AuraHandler.EffectHandlers[(int) this.AuraType];
                if (this.AuraEffectHandlerCreator == null)
                    this.AuraEffectHandlerCreator = AuraHandler.EffectHandlers[0];
            }

            this.RepairBrokenTargetPairs();
            this.IsEnchantmentEffect = this.EffectType == SpellEffectType.EnchantHeldItem ||
                                       this.EffectType == SpellEffectType.EnchantItem ||
                                       this.EffectType == SpellEffectType.EnchantItemTemporary;
            AISpellUtil.DecideDefaultTargetHandlerDefintion(this);
        }

        /// <summary>
        /// More or less frequently occuring target pairs that make no sense
        /// </summary>
        private void RepairBrokenTargetPairs()
        {
            if (this.ImplicitTargetA != ImplicitSpellTargetType.Self ||
                this.ImplicitTargetB != ImplicitSpellTargetType.Duel)
                return;
            this.ImplicitTargetA = ImplicitSpellTargetType.None;
        }

        /// <summary>
        /// Whether this effect can share targets with the given effect
        /// </summary>
        public bool SharesTargetsWith(SpellEffect b)
        {
            TargetDefinition targetDefinition = this.GetTargetDefinition();
            if (targetDefinition != null && targetDefinition.Equals((object) b.GetTargetDefinition()))
                return true;
            if (this.ImplicitTargetA == b.ImplicitTargetA)
                return this.ImplicitTargetB == b.ImplicitTargetB;
            return false;
        }

        public bool HasTarget(ImplicitSpellTargetType target)
        {
            if (this.ImplicitTargetA != target)
                return this.ImplicitTargetB == target;
            return true;
        }

        public bool HasTarget(params ImplicitSpellTargetType[] targets)
        {
            return ((IEnumerable<ImplicitSpellTargetType>) targets).FirstOrDefault<ImplicitSpellTargetType>(
                       new Func<ImplicitSpellTargetType, bool>(this.HasTarget)) != ImplicitSpellTargetType.None;
        }

        public void CopyValuesTo(SpellEffect effect)
        {
            effect.BasePoints = this.BasePoints;
            effect.DiceSides = this.DiceSides;
        }

        /// <summary>
        /// Adds a set of Auras of which at least one need to be active for this SpellEffect to activate
        /// </summary>
        public void AddRequiredActivationAuras(params SpellLineId[] lines)
        {
            foreach (SpellLineId line in lines)
                this.AddRequiredActivationAuras(line.GetLine().ToArray<Spell>());
        }

        public void AddRequiredActivationAuras(params SpellId[] ids)
        {
            Spell[] spellArray = new Spell[ids.Length];
            for (int index = 0; index < ids.Length; ++index)
            {
                SpellId id = ids[index];
                Spell spell = SpellHandler.Get(id);
                if (spell == null)
                    throw new ArgumentException("Invalid spell in AddRequiredActivationAuras: " + (object) id);
                spellArray[index] = spell;
            }

            this.AddRequiredActivationAuras(spellArray);
        }

        public void AddRequiredActivationAuras(params Spell[] spells)
        {
            if (this.RequiredActivationAuras == null)
                this.RequiredActivationAuras = spells;
            else
                ArrayUtil.Concat<Spell>(ref this.RequiredActivationAuras, spells);
        }

        public AuraEffectHandler CreateAuraEffectHandler(ObjectReference caster, Unit target,
            ref SpellFailedReason failedReason)
        {
            return this.CreateAuraEffectHandler(caster, target, ref failedReason, (SpellCast) null);
        }

        internal AuraEffectHandler CreateAuraEffectHandler(ObjectReference caster, Unit target,
            ref SpellFailedReason failedReason, SpellCast triggeringCast)
        {
            AuraEffectHandler auraEffectHandler = this.AuraEffectHandlerCreator();
            if (triggeringCast != null && triggeringCast.TriggerEffect != null &&
                triggeringCast.TriggerEffect.OverrideEffectValue)
            {
                if (this.Spell.Effects.Length > 1)
                    SpellEffect.log.Warn(
                        "Spell {0} had overriding SpellEffect although the spell that was triggered had {2} (> 1) effects",
                        (object) this.Spell, (object) this.Spell.Effects.Length);
                auraEffectHandler.m_spellEffect = triggeringCast.TriggerEffect;
            }
            else
                auraEffectHandler.m_spellEffect = this;

            auraEffectHandler.BaseEffectValue = this.CalcEffectValue(caster);
            auraEffectHandler.CheckInitialize(triggeringCast, caster, target, ref failedReason);
            return auraEffectHandler;
        }

        public int CalcEffectValue(ObjectReference casterReference)
        {
            Unit unitMaster = casterReference.UnitMaster;
            if (unitMaster != null)
                return this.CalcEffectValue(unitMaster);
            return this.CalcEffectValue(casterReference.Level, 0, false);
        }

        public int CalcEffectValue(Unit caster)
        {
            int num = caster == null
                ? this.CalcEffectValue(1, 0, false)
                : this.CalcEffectValue(caster.Level, caster.ComboPoints, true);
            return this.CalcEffectValue(caster, num);
        }

        public int CalcEffectValue(Unit caster, int value)
        {
            if (this.EffectValueOverrideEffect != null &&
                caster.Spells.Contains<Spell>(this.EffectValueOverrideEffect.Spell))
                return this.EffectValueOverrideEffect.CalcEffectValue(caster, value);
            if (caster == null)
                return value;
            if ((double) this.APValueFactor != 0.0 || (double) this.APPerComboPointValueFactor != 0.0)
            {
                float num1 = this.APValueFactor + this.APPerComboPointValueFactor * (float) caster.ComboPoints;
                int num2 = this.Spell.IsRanged ? caster.TotalRangedAP : caster.TotalMeleeAP;
                value += (int) ((double) num2 * (double) num1 + 0.5);
            }

            if (caster is Character && this.SpellPowerValuePct != 0)
                value += (this.SpellPowerValuePct * caster.GetDamageDoneMod(this.Spell.Schools[0]) + 50) / 100;
            SpellModifierType type;
            switch (this.EffectIndex)
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

            value = caster.Auras.GetModifiedInt(type, this.Spell, value);
            value = caster.Auras.GetModifiedInt(SpellModifierType.AllEffectValues, this.Spell, value);
            return value;
        }

        public int CalcEffectValue()
        {
            return this.CalcEffectValue(0, 0, false);
        }

        public int CalcEffectValue(int level, int comboPoints, bool useOverride)
        {
            if (this.EffectValueOverrideEffect != null && useOverride)
                return this.EffectValueOverrideEffect.CalcEffectValue(level, comboPoints, false);
            if (this.BasePoints == 0)
                this.BasePoints = this.MiscValue;
            return this.BasePoints;
        }

        public int GetMultipliedValue(Unit caster, int val, int currentTargetNo)
        {
            int effectIndex = (int) this.EffectIndex;
            if (effectIndex >= this.Spell.DamageMultipliers.Length || effectIndex < 0 || currentTargetNo == 0)
                return val;
            float num = this.Spell.DamageMultipliers[effectIndex];
            if (caster != null)
                num = caster.Auras.GetModifiedFloat(SpellModifierType.ChainValueFactor, this.Spell, num);
            if ((double) num != 1.0)
                return val = MathUtil.RoundInt((float) Math.Pow((double) num, (double) currentTargetNo) * (float) val);
            return val;
        }

        public float GetRadius(ObjectReference caster)
        {
            float num = this.Radius;
            Unit unitMaster = caster.UnitMaster;
            if (unitMaster != null)
                num = unitMaster.Auras.GetModifiedFloat(SpellModifierType.Radius, this.Spell, num);
            return num;
        }

        public void ClearAffectMask()
        {
            this.AffectMask = new uint[3];
        }

        public void SetAffectMask(params SpellLineId[] abilities)
        {
            this.ClearAffectMask();
            this.AddToAffectMask(abilities);
        }

        /// <summary>
        /// Adds a set of spells to the explicite relationship set of this effect, which is used to determine whether
        /// a certain Spell and this effect have some kind of influence on one another (for procs, talent modifiers etc).
        /// Only adds the spells, will not work on the spells' trigger spells.
        /// </summary>
        /// <param name="abilities"></param>
        public void AddAffectingSpells(params SpellLineId[] abilities)
        {
            if (this.AffectSpellSet == null)
                this.AffectSpellSet = new HashSet<Spell>();
            foreach (SpellLineId ability in abilities)
                this.AffectSpellSet.AddRange<Spell>((IEnumerable<Spell>) ability.GetLine());
        }

        /// <summary>
        /// Adds a set of spells to the explicite relationship set of this effect, which is used to determine whether
        /// a certain Spell and this effect have some kind of influence on one another (for procs, talent modifiers etc).
        /// Only adds the spells, will not work on the spells' trigger spells.
        /// </summary>
        /// <param name="abilities"></param>
        public void AddAffectingSpells(params SpellId[] spells)
        {
            if (this.AffectSpellSet == null)
                this.AffectSpellSet = new HashSet<Spell>();
            foreach (SpellId spell in spells)
                this.AffectSpellSet.Add(SpellHandler.Get(spell));
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
            if (abilities.Length != 1)
            {
                foreach (SpellLineId ability in abilities)
                {
                    Spell firstRank = ability.GetLine().FirstRank;
                    for (int index = 0; index < 3; ++index)
                        mask[index] |= firstRank.SpellClassMask[index];
                }
            }
            else
                abilities[0].GetLine().FirstRank.SpellClassMask.CopyTo((Array) mask, 0);

            HashSet<SpellLine> affectedSpellLines = SpellHandler.GetAffectedSpellLines(this.Spell.ClassId, mask);
            if (affectedSpellLines.Count != abilities.Length)
                LogManager.GetCurrentClassLogger().Warn(
                    "[SPELL Inconsistency for {0}] Invalid affect mask affects a different set than the one intended: {1} (intended: {2}) - You might want to use AddAffectingSpells instead!",
                    (object) this.Spell, (object) affectedSpellLines.ToString<SpellLine>(", "),
                    (object) ((IEnumerable<SpellLineId>) abilities).ToString<SpellLineId>(", "));
            for (int index = 0; index < 3; ++index)
                this.AffectMask[index] |= mask[index];
        }

        public void CopyAffectMaskTo(uint[] mask)
        {
            for (int index = 0; index < this.AffectMask.Length; ++index)
                mask[index] |= this.AffectMask[index];
        }

        public void RemoveAffectMaskFrom(uint[] mask)
        {
            for (int index = 0; index < this.AffectMask.Length; ++index)
                mask[index] ^= this.AffectMask[index];
        }

        public bool MatchesSpell(Spell spell)
        {
            if (spell.SpellClassSet == this.Spell.SpellClassSet && spell.MatchesMask(this.AffectMask))
                return true;
            if (this.AffectSpellSet != null)
                return this.AffectSpellSet.Contains(spell);
            return false;
        }

        public void MakeProc(AuraEffectHandlerCreator creator, params SpellLineId[] exclusiveTriggers)
        {
            this.IsProc = true;
            this.ClearAffectMask();
            this.AddAffectingSpells(exclusiveTriggers);
            this.AuraEffectHandlerCreator = creator;
        }

        /// <summary>
        /// Uses the AffectMask, rather than exclusive trigger spells. This is important if also spells
        /// that are triggerd by the triggered spells are allowed to trigger this proc.
        /// </summary>
        public void MakeProcWithMask(AuraEffectHandlerCreator creator, params SpellLineId[] exclusiveTriggers)
        {
            this.IsProc = true;
            this.SetAffectMask(exclusiveTriggers);
            this.AuraEffectHandlerCreator = creator;
        }

        public bool CanProcBeTriggeredBy(Spell spell)
        {
            if (spell != null && this.HasAffectingSpells)
                return this.MatchesSpell(spell);
            return true;
        }

        public void DumpInfo(TextWriter writer, string indent)
        {
            writer.WriteLine(indent + "Effect: " + (object) this);
            indent += "\t";
            if (this.ImplicitTargetA != ImplicitSpellTargetType.None)
                writer.WriteLine(indent + "ImplicitTargetA: {0}", (object) this.ImplicitTargetA);
            if (this.ImplicitTargetB != ImplicitSpellTargetType.None)
                writer.WriteLine(indent + "ImplicitTargetB: {0}", (object) this.ImplicitTargetB);
            if (this.MiscValue != 0 || this.MiscValueType != (Type) null)
                writer.WriteLine(indent + "MiscValue: " + this.GetMiscStr(this.MiscValueType, this.MiscValue));
            if (this.MiscValueB != 0)
                writer.WriteLine(indent + "MiscValueB: " + this.GetMiscStr(this.MiscValueBType, this.MiscValueB));
            HashSet<SpellLine> affectedLines = this.AffectedLines;
            if (affectedLines.Count > 0)
                writer.WriteLine(indent + "Affects: {0}", (object) affectedLines.ToString<SpellLine>(", "));
            else if (this.AffectMask[0] != 0U || this.AffectMask[1] != 0U || this.AffectMask[2] != 0U)
                writer.WriteLine(indent + "Affects: <Nothing> ({0}{1}{2})", (object) this.AffectMask[0].ToString("X8"),
                    (object) this.AffectMask[1].ToString("X8"), (object) this.AffectMask[2].ToString("X8"));
            if (this.BasePoints != 0)
                writer.WriteLine(indent + "BasePoints: {0}", (object) this.BasePoints);
            if (this.DiceSides != 0)
                writer.WriteLine(indent + "DiceSides: {0}", (object) this.DiceSides);
            if (this.Amplitude != 0)
                writer.WriteLine(indent + "Amplitude: {0}", (object) this.Amplitude);
            if (this.ChainTargets != 0)
                writer.WriteLine(indent + "ChainTarget: {0}", (object) this.ChainTargets);
            if (this.ItemId != 0U)
                writer.WriteLine(indent + "ItemId: {0} ({1})", (object) (Asda2ItemId) this.ItemId,
                    (object) this.ItemId);
            if ((double) this.PointsPerComboPoint != 0.0)
                writer.WriteLine(indent + "PointsPerComboPoint: {0}", (object) this.PointsPerComboPoint);
            if ((double) this.ProcValue != 0.0)
                writer.WriteLine(indent + "ProcValue: {0}", (object) this.ProcValue);
            if ((double) this.Radius != 0.0)
                writer.WriteLine(indent + "Radius: {0}", (object) this.Radius);
            if ((double) this.RealPointsPerLevel != 0.0)
                writer.WriteLine(indent + "RealPointsPerLevel: {0}", (object) this.RealPointsPerLevel);
            if (this.Mechanic != SpellMechanic.None)
                writer.WriteLine(indent + "Mechanic: {0}", (object) this.Mechanic);
            if (this.TriggerSpellId != SpellId.None)
                writer.WriteLine(indent + "Triggers: {0} ({1})", (object) this.TriggerSpellId,
                    (object) this.TriggerSpellId);
            SpellSummonEntry summonEntry = this.SummonEntry;
            if (summonEntry == null)
                return;
            writer.WriteLine(indent + "Summon information:");
            indent += "\t";
            writer.WriteLine(indent + "Summon ID: {0}", (object) summonEntry.Id);
            if (summonEntry.Group != SummonGroup.Wild)
                writer.WriteLine(indent + "Summon Group: {0}", (object) summonEntry.Group);
            if (summonEntry.FactionTemplateId != FactionTemplateId.None)
                writer.WriteLine(indent + "Summon Faction: {0}", (object) summonEntry.FactionTemplateId);
            if (summonEntry.Type != SummonPropertyType.None)
                writer.WriteLine(indent + "Summon Type: {0}", (object) summonEntry.Type);
            if (summonEntry.Flags != SummonFlags.None)
                writer.WriteLine(indent + "Summon Flags: {0}", (object) summonEntry.Flags);
            if (summonEntry.Slot == 0U)
                return;
            writer.WriteLine(indent + "Summon Slot: {0}", (object) summonEntry.Slot);
        }

        public string GetTargetString()
        {
            int num = 0;
            List<string> stringList = new List<string>(2);
            if (this.ImplicitTargetA != ImplicitSpellTargetType.None)
            {
                ++num;
                stringList.Add("A: " + (object) this.ImplicitTargetA);
            }

            if (this.ImplicitTargetB != ImplicitSpellTargetType.None)
            {
                ++num;
                stringList.Add("B: " + (object) this.ImplicitTargetB);
            }

            if (stringList.Count <= 0)
                return "Targets: None";
            return "Targets (" + (object) num + ") - " +
                   ((IEnumerable<string>) stringList.ToArray()).ToString<string>(", ");
        }

        private string GetMiscStr(Type type, int val)
        {
            object obj = (object) null;
            if (type != (Type) null && StringParser.Parse(val.ToString(), type, ref obj))
                return string.Format("{0} ({1})", obj, (object) val);
            return val.ToString();
        }

        public static Type GetSpellEffectEffectMiscValueType(SpellEffectType type)
        {
            if (type < SpellEffectType.End)
                return SpellEffect.SpellEffectMiscValueTypes[(int) type];
            SpellEffect.log.Warn("Found invalid SpellEffectType {0}.", (object) type);
            return (Type) null;
        }

        public static void SetSpellEffectEffectMiscValueType(SpellEffectType effectType, Type type)
        {
            SpellEffect.SpellEffectMiscValueTypes[(int) effectType] = type;
        }

        public static Type GetAuraEffectMiscValueType(AuraType type)
        {
            if (type < AuraType.End)
                return SpellEffect.AuraEffectMiscValueTypes[(int) type];
            SpellEffect.log.Warn("Found invalid AuraType {0}.", (object) type);
            return (Type) null;
        }

        public static void SetAuraEffectMiscValueType(AuraType auraType, Type type)
        {
            SpellEffect.AuraEffectMiscValueTypes[(int) auraType] = type;
        }

        public static Type GetSpellEffectEffectMiscValueBType(SpellEffectType type)
        {
            if (type < SpellEffectType.End)
                return SpellEffect.SpellEffectMiscValueBTypes[(int) type];
            SpellEffect.log.Warn("Found invalid SpellEffectType {0}.", (object) type);
            return (Type) null;
        }

        public static void SetSpellEffectEffectMiscValueBType(SpellEffectType effectType, Type type)
        {
            SpellEffect.SpellEffectMiscValueBTypes[(int) effectType] = type;
        }

        public static Type GetAuraEffectMiscValueBType(AuraType type)
        {
            if (type < AuraType.End)
                return SpellEffect.AuraEffectMiscValueBTypes[(int) type];
            SpellEffect.log.Warn("Found invalid AuraType {0}.", (object) type);
            return (Type) null;
        }

        public static void SetAuraEffectMiscValueBType(AuraType auraType, Type type)
        {
            SpellEffect.AuraEffectMiscValueBTypes[(int) auraType] = type;
        }

        internal static void InitMiscValueTypes()
        {
            SpellEffect.AuraEffectMiscValueTypes[108] = typeof(SpellModifierType);
            SpellEffect.AuraEffectMiscValueTypes[107] = typeof(SpellModifierType);
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModDamageDone, typeof(DamageSchoolMask));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModDamageDonePercent, typeof(DamageSchoolMask));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModDamageDoneToCreatureType, typeof(DamageSchoolMask));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModDamageDoneVersusCreatureType, typeof(CreatureMask));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModDamageTaken, typeof(DamageSchoolMask));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModDamageTakenPercent, typeof(DamageSchoolMask));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModPowerCost, typeof(PowerType));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModPowerCostForSchool, typeof(PowerType));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModPowerRegen, typeof(PowerType));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModPowerRegenPercent, typeof(PowerType));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModRating, typeof(CombatRatingMask));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModSkill, typeof(SkillId));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModSkillTalent, typeof(SkillId));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModStat, typeof(StatType));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModStatPercent, typeof(StatType));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModTotalStatPercent, typeof(StatType));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.DispelImmunity, typeof(DispelType));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.MechanicImmunity, typeof(SpellMechanic));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.Mounted, typeof(NPCId));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModShapeshift, typeof(ShapeshiftForm));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.Transform, typeof(NPCId));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModSpellDamageByPercentOfStat, typeof(DamageSchoolMask));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModSpellHealingByPercentOfStat, typeof(DamageSchoolMask));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.DamagePctAmplifier, typeof(DamageSchoolMask));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModSilenceDurationPercent, typeof(SpellMechanic));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModMechanicDurationPercent, typeof(SpellMechanic));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.TrackCreatures, typeof(CreatureType));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModSpellHitChance, typeof(DamageSchoolMask));
            SpellEffect.SetAuraEffectMiscValueType(AuraType.ModSpellHitChance2, typeof(DamageSchoolMask));
            SpellEffect.SetAuraEffectMiscValueBType(AuraType.ModSpellDamageByPercentOfStat, typeof(StatType));
            SpellEffect.SetAuraEffectMiscValueBType(AuraType.ModSpellHealingByPercentOfStat, typeof(StatType));
            SpellEffect.SetSpellEffectEffectMiscValueType(SpellEffectType.Dispel, typeof(DispelType));
            SpellEffect.SetSpellEffectEffectMiscValueType(SpellEffectType.DispelMechanic, typeof(SpellMechanic));
            SpellEffect.SetSpellEffectEffectMiscValueType(SpellEffectType.Skill, typeof(SkillId));
            SpellEffect.SetSpellEffectEffectMiscValueType(SpellEffectType.SkillStep, typeof(SkillId));
            SpellEffect.SetSpellEffectEffectMiscValueType(SpellEffectType.Skinning, typeof(SkinningType));
            SpellEffect.SetSpellEffectEffectMiscValueType(SpellEffectType.Summon, typeof(NPCId));
            SpellEffect.SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObject, typeof(GOEntryId));
            SpellEffect.SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObjectSlot1, typeof(GOEntryId));
            SpellEffect.SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObjectSlot2, typeof(GOEntryId));
            SpellEffect.SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObjectWild, typeof(GOEntryId));
            SpellEffect.SetSpellEffectEffectMiscValueBType(SpellEffectType.Summon, typeof(SummonType));
            SpellEffect.TargetAreaEffects.AddRange<ImplicitSpellTargetType>(
                (IEnumerable<ImplicitSpellTargetType>) new ImplicitSpellTargetType[7]
                {
                    ImplicitSpellTargetType.AllAroundLocation,
                    ImplicitSpellTargetType.AllEnemiesInArea,
                    ImplicitSpellTargetType.AllEnemiesInAreaChanneled,
                    ImplicitSpellTargetType.AllEnemiesInAreaInstant,
                    ImplicitSpellTargetType.AllPartyInArea,
                    ImplicitSpellTargetType.AllPartyInAreaChanneled,
                    ImplicitSpellTargetType.InvisibleOrHiddenEnemiesAtLocationRadius
                });
            SpellEffect.AreaEffects.AddRange<ImplicitSpellTargetType>(
                (IEnumerable<ImplicitSpellTargetType>) SpellEffect.TargetAreaEffects);
            SpellEffect.AreaEffects.AddRange<ImplicitSpellTargetType>(
                (IEnumerable<ImplicitSpellTargetType>) new ImplicitSpellTargetType[11]
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
                if (this.IsAuraEffect)
                    return SpellEffect.GetAuraEffectMiscValueType(this.AuraType);
                return SpellEffect.GetSpellEffectEffectMiscValueType(this.EffectType);
            }
        }

        public Type MiscValueBType
        {
            get
            {
                if (this.IsAuraEffect)
                    return SpellEffect.GetAuraEffectMiscValueBType(this.AuraType);
                return SpellEffect.GetSpellEffectEffectMiscValueBType(this.EffectType);
            }
        }

        /// <summary>
        /// Get's Basepoints for a spell after applying DamageMods.
        /// </summary>
        public int GetModifiedDamage(Unit caster)
        {
            if (this.IsPeriodic)
                return caster.Auras.GetModifiedInt(SpellModifierType.PeriodicEffectValue, this.Spell,
                    this.CalcEffectValue());
            return caster.GetFinalDamage(caster.GetLeastResistantSchool(this.Spell), this.CalcEffectValue(),
                this.Spell);
        }

        public override bool Equals(object obj)
        {
            if (obj is SpellEffect)
                return ((SpellEffect) obj).EffectType == this.EffectType;
            return false;
        }

        public override int GetHashCode()
        {
            return this.EffectType.GetHashCode();
        }

        public override string ToString()
        {
            return ((int) this.EffectType).ToString() +
                   (this.TriggerSpell == null ? (object) "" : (object) (" (" + (object) this.TriggerSpell + ")")) +
                   (this.AuraType == AuraType.None ? (object) "" : (object) (" (" + (object) this.AuraType + ")"));
        }
    }
}