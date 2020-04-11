using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Core.DBC;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Targeting;
using WCell.RealmServer.Talents;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;
using WCell.Util.NLog;

namespace WCell.RealmServer.Spells
{
    /// <summary>Aura-related information of a Spell</summary>
    /// <summary>Represents any spell action or aura</summary>
    /// <summary>
    /// Represents a Spell (which -in fact- is any kind of effect or action) in WoW.
    /// </summary>
    [DataHolder(RequirePersistantAttr = true)]
    [Serializable]
    public class Spell : IDataHolder, ISpellGroup, IEnumerable<Spell>, IEnumerable
    {
        /// <summary>
        /// This Range will be used for all Spells that have MaxRange = 0
        /// </summary>
        public static int DefaultSpellRange = 30;

        private static readonly Regex numberRegex = new Regex("\\d+");
        public static readonly Spell[] EmptyArray = new Spell[0];

        /// <summary>
        /// Wheter this spell can be cast on players (automatically false for all taunts)
        /// </summary>
        public bool CanCastOnPlayer = true;

        /// <summary>
        /// Whether this is an Aura that can override other instances of itself if they have the same rank (true by default).
        /// Else the spell cast will fail when trying to do so.
        /// </summary>
        public bool CanOverrideEqualAuraRank = true;

        [NotPersistent] public EquipmentSlot EquipmentSlot = EquipmentSlot.End;
        [NotPersistent] public uint[] AllAffectingMasks = new uint[3];
        [Persistent] public RequiredSpellTargetType RequiredTargetType = RequiredSpellTargetType.Default;

        /// <summary>List of Spells to be learnt when this Spell is learnt</summary>
        public readonly List<Spell> AdditionallyTaughtSpells = new List<Spell>(0);

        [Persistent(3)] public uint[] SpellClassMask = new uint[3];

        /// <summary>
        /// Used for effect-value damping when using chain targets, eg:
        /// 	DamageMultipliers: 0.6, 1, 1
        /// 	"Each jump reduces the effectiveness of the heal by 40%.  Heals $x1 total targets."
        /// </summary>
        [Persistent(3)] public float[] DamageMultipliers = new float[3];

        [NotPersistent] public ToolCategory[] RequiredToolCategories = new ToolCategory[2];

        /// <summary>Whether this Spell is an Aura</summary>
        public bool IsAura;

        /// <summary>AuraUID, the same for all Auras that may not stack</summary>
        public uint AuraUID;

        public bool HasPeriodicAuraEffects;
        public bool HasNonPeriodicAuraEffects;

        /// <summary>
        /// AuraFlags to be applied to all Auras resulting from this Spell
        /// </summary>
        public AuraFlags DefaultAuraFlags;

        /// <summary>
        /// General Amplitude for Spells that represent Auras (can only have one for the time being)
        /// </summary>
        public int AuraAmplitude;

        /// <summary>whether this Spell is an AreaAura</summary>
        public bool IsAreaAura;

        /// <summary>Modal Auras cannot be updated, but must be replaced</summary>
        public bool IsAutoRepeating;

        /// <summary>
        /// General Amplitude for Spells that represent AreaAuras (can only have one per spell)
        /// </summary>
        public int AreaAuraAmplitude;

        /// <summary>All effects that belong to an Aura</summary>
        public SpellEffect[] AuraEffects;

        /// <summary>All effects that belong to an AreaAura</summary>
        public SpellEffect[] AreaAuraEffects;

        /// <summary>
        /// Whether the Aura's effects should multiply it's effect value by the amount of its Applications
        /// </summary>
        public bool CanStack;

        /// <summary>The amount of initial Aura-Applications</summary>
        public int InitialStackCount;

        /// <summary>Only has Aura effects</summary>
        public bool IsPureAura;

        /// <summary>Only has positive Aura effects</summary>
        public bool IsPureBuff;

        /// <summary>Only has negative Aura effects</summary>
        public bool IsPureDebuff;

        /// <summary>whether this Spell applies the death effect</summary>
        public bool IsGhost;

        public bool IsVehicle;

        /// <summary>Spell lets one shapeshift into another creature</summary>
        public bool IsShapeshift;

        /// <summary>whether this spell applies makes the targets fly</summary>
        public bool HasFlyEffect;

        /// <summary>Does this Spell apply a Flying-Mount Aura?</summary>
        public bool IsFlyingMount;

        /// <summary>
        /// 
        /// </summary>
        public bool CanApplyMultipleTimes;

        /// <summary>
        /// Whether the Aura has effects that depend on the wearer's Shapeshift form
        /// </summary>
        public bool HasShapeshiftDependentEffects;

        /// <summary>
        /// Whether the Aura is in any way dependent on the wearer's shapeshift form
        /// </summary>
        public bool IsModalShapeshiftDependentAura;

        /// <summary>
        /// 
        /// </summary>
        public AuraCasterGroup AuraCasterGroup;

        /// <summary>
        /// Whether this is a Spell that is only used to prevent other Spells (cannot be cancelled etc)
        /// </summary>
        public bool IsPreventionDebuff;

        [NotPersistent] public Spell[] TargetTriggerSpells;
        [NotPersistent] public Spell[] CasterTriggerSpells;
        [NotPersistent] public HashSet<Spell> CasterProcSpells;
        [NotPersistent] public HashSet<Spell> TargetProcSpells;
        [NotPersistent] public List<ProcHandlerTemplate> ProcHandlers;

        /// <summary>Used for teleport spells amongst others</summary>
        public Vector3 SpellTargetLocation;

        /// <summary>Wheter this Aura can proc</summary>
        public bool IsProc;

        /// <summary>
        /// Whether this spell is supposed to proc something.
        /// If set to true, this Spell will generate a SpellCast proc event when casted.
        /// Don't use for damage spells, else they will generate 2 events!
        /// </summary>
        public bool GeneratesProcEventOnCast;

        /// <summary>
        /// Amount of millis before this Spell may proc another time (if it is a proc)
        /// </summary>
        public int ProcDelay;

        /// <summary>Whether this Spell's spell damage is increased by AP</summary>
        public bool DamageIncreasedByAP;

        /// <summary>
        /// The effect whose value represents the max amount of targets to be selected.
        /// This is a way to boost the max target amount with a simple EffectValue modifier.
        /// (Of course one could just have added a new modifier for this, but well.)
        /// </summary>
        public SpellEffect MaxTargetEffect;

        /// <summary>
        /// Optional set of SpellEffects to be applied, only if certain Auras are applied
        /// </summary>
        public SpellEffect[] AuraConditionalEffects;

        [NotPersistent] public AISpellSettings AISettings;

        /// <summary>
        /// Whether this is a Combat ability that will be triggered on next weapon strike (like Heroic Strike etc)
        /// </summary>
        public bool IsOnNextStrike;

        /// <summary>
        /// whether this is an ability involving any kind of weapon-attack
        /// </summary>
        public bool IsPhysicalAbility;

        /// <summary>Whether this can trigger an instant Strike</summary>
        public bool IsStrikeSpell;

        /// <summary>whether this is actually a passive buff</summary>
        public bool IsPassive;

        /// <summary>Whether this is a ranged attack (includes wands)</summary>
        public bool IsRanged;

        /// <summary>
        /// Whether this is a ranged attack (includes wands), that is not triggered
        /// </summary>
        public bool IsRangedAbility;

        /// <summary>
        /// whether this is a throw (used for any kind of throwing weapon)
        /// </summary>
        public bool IsThrow;

        /// <summary>whether this is an actual SpellCaster spell</summary>
        public bool IsProfession;

        /// <summary>whether this teaches the initial Profession</summary>
        public bool TeachesApprenticeAbility;

        /// <summary>whether this is teaching another spell</summary>
        public bool IsTeachSpell;

        /// <summary>Whether it has any individual or category cooldown</summary>
        public bool HasCooldown;

        /// <summary>
        /// Whether this spell has an individual cooldown (unlike a category or "global" cooldown)
        /// </summary>
        public bool HasIndividualCooldown;

        /// <summary>Tame Beast (Id: 13481) amongst others</summary>
        public bool IsTameEffect;

        /// <summary>Whether this spell enchants an Item</summary>
        public bool IsEnchantment;

        /// <summary>
        /// Fishing spawns a FishingNode which needs to be removed upon canceling
        /// </summary>
        public bool IsFishing;

        /// <summary>The spell which teaches this spell (if any)</summary>
        public Spell LearnSpell;

        /// <summary>whether this spell is triggered by another one</summary>
        public bool IsTriggeredSpell;

        /// <summary>
        /// Indicates whether this Spell has at least one harmful effect
        /// </summary>
        public bool HasHarmfulEffects;

        /// <summary>
        /// Indicates whether this Spell has at least one beneficial effect
        /// </summary>
        public bool HasBeneficialEffects;

        public HarmType HarmType;

        /// <summary>
        /// The SpellEffect of this Spell that represents a PersistentAreaAura and thus a DO (or null if it has none)
        /// </summary>
        public SpellEffect DOEffect;

        /// <summary>whether this is a Heal-spell</summary>
        public bool IsHealSpell;

        /// <summary>
        /// Whether this is a weapon ability that attacks with both weapons
        /// </summary>
        public bool IsDualWieldAbility;

        /// <summary>whether this is a Skinning-Spell</summary>
        public bool IsSkinning;

        [NotPersistent] public Spell.SpecialCastHandler SpecialCast;
        [NotPersistent] public TalentEntry Talent;
        [NotPersistent] private SkillAbility m_Ability;
        [NotPersistent] public SkillTierId SkillTier;
        [NotPersistent] public ItemTemplate[] RequiredTools;
        [NotPersistent] public Spell NextRank;
        [NotPersistent] public Spell PreviousRank;

        /// <summary>Indicates whether this Spell has any targets at all</summary>
        public bool HasTargets;

        /// <summary>
        /// Indicates whether this Spell has at least one effect on the caster
        /// </summary>
        public bool CasterIsTarget;

        /// <summary>
        /// Indicates whether this Spell teleports the Uni back to its bound location
        /// </summary>
        public bool IsHearthStoneSpell;

        public bool IsAreaSpell;
        public bool IsDamageSpell;
        [NotPersistent] public SpellEffect TotemEffect;
        [NotPersistent] public SpellEffect[] ProcTriggerEffects;
        public bool GeneratesComboPoints;
        public bool IsFinishingMove;
        public bool RequiresDeadTarget;

        /// <summary>whether this is a channel-spell</summary>
        public bool IsChanneled;

        public int ChannelAmplitude;
        public bool RequiresCasterOutOfCombat;

        /// <summary>
        /// Whether this spell costs default power (does not include Runes)
        /// </summary>
        public bool CostsPower;

        /// <summary>Whether this Spell has any Rune costs</summary>
        public bool CostsRunes;

        /// <summary>
        /// Auras with modifier effects require existing Auras to be re-evaluated
        /// </summary>
        public bool HasModifierEffects;

        [Persistent] public Asda2ClassMask ClassMask;
        [Persistent] public int Cost;
        [Persistent] public byte LearnLevel;
        [Persistent] public byte ProffNum;
        public bool HasManaShield;
        [Persistent] public int Duration;
        public bool IsEnhancer;
        private bool init1;
        private bool init2;
        [Persistent] public byte MaxRange;
        public SpellLine Line;

        /// <summary>
        /// Whether this spell has effects that require other Auras to be active to be activated
        /// </summary>
        public bool HasAuraDependentEffects;

        [Persistent] public uint RequiredTargetId;
        [Persistent] public WCell.RealmServer.Spells.SpellTargetLocation TargetLocation;
        [Persistent] public float TargetOrientation;
        public byte SoulGuardProffLevel;

        [NotPersistent]
        public static MappedDBCReader<Spell.DurationEntry, Spell.DBCDurationConverter> mappeddbcDurationReader;

        [NotPersistent] public static MappedDBCReader<float, Spell.DBCRadiusConverter> mappeddbcRadiusReader;
        [NotPersistent] public static MappedDBCReader<uint, Spell.DBCCastTimeConverter> mappeddbcCastTimeReader;
        [NotPersistent] public static MappedDBCReader<SimpleRange, Spell.DBCRangeConverter> mappeddbcRangeReader;
        [NotPersistent] public static MappedDBCReader<string, Spell.DBCMechanicConverter> mappeddbcMechanicReader;

        [NotPersistent]
        public static MappedDBCReader<RuneCostEntry, Spell.DBCSpellRuneCostConverter> mappeddbcRuneCostReader;

        [Persistent] public short RealId;
        [Persistent] public uint Id;
        public SpellId SpellId;
        [Persistent] public uint Category;
        [Persistent] public DispelType DispelType;
        [Persistent] public SpellMechanic Mechanic;
        [Persistent] public SpellAttributes Attributes;
        [Persistent] public SpellAttributesEx AttributesEx;
        [Persistent] public SpellAttributesExB AttributesExB;
        [Persistent] public SpellAttributesExC AttributesExC;
        [Persistent] public SpellAttributesExD AttributesExD;
        [Persistent] public SpellAttributesExE AttributesExE;
        [Persistent] public SpellAttributesExF AttributesExF;
        public uint Unk_322_1;
        public uint Unk_322_2;
        public uint Unk_322_3;
        public float Unk_322_4_1;
        public float Unk_322_4_2;
        public float Unk_322_4_3;

        /// <summary>3.2.2 related to description?</summary>
        public uint spellDescriptionVariablesID;

        /// <summary>SpellShapeshiftForm.dbc</summary>
        public ShapeshiftMask RequiredShapeshiftMask;

        /// <summary>SpellShapeshiftForm.dbc</summary>
        public ShapeshiftMask ExcludeShapeshiftMask;

        [Persistent] public SpellTargetFlags TargetFlags;

        /// <summary>CreatureType.dbc</summary>
        public CreatureMask CreatureMask;

        /// <summary>SpellFocusObject.dbc</summary>
        public SpellFocus RequiredSpellFocus;

        public SpellFacingFlags FacingFlags;
        [Persistent] public AuraState RequiredCasterAuraState;
        [Persistent] public AuraState RequiredTargetAuraState;
        [Persistent] public AuraState ExcludeCasterAuraState;
        [Persistent] public AuraState ExcludeTargetAuraState;
        [Persistent] public SpellId RequiredCasterAuraId;
        [Persistent] public SpellId RequiredTargetAuraId;
        [Persistent] public SpellId ExcludeCasterAuraId;
        [Persistent] public SpellId ExcludeTargetAuraId;
        [Persistent] public uint CastDelay;
        [Persistent] public int CooldownTime;
        [Persistent] public int categoryCooldownTime;
        [Persistent] public InterruptFlags InterruptFlags;
        [Persistent] public AuraInterruptFlags AuraInterruptFlags;
        [Persistent] public ChannelInterruptFlags ChannelInterruptFlags;
        [Persistent] public ProcTriggerFlags ProcTriggerFlags;
        [Persistent] public uint ProcChance;
        [Persistent] public int ProcCharges;
        [Persistent] public int MaxLevel;
        [Persistent] public int BaseLevel;
        [Persistent] public int Level;

        /// <summary>SpellDuration.dbc</summary>
        public int DurationIndex;

        [NotPersistent] public Spell.DurationEntry Durations;
        [Persistent] public PowerType PowerType;
        [Persistent] public int PowerCost;
        [Persistent] public int PowerCostPerlevel;
        [Persistent] public int PowerPerSecond;

        /// <summary>Unused so far</summary>
        public int PowerPerSecondPerLevel;

        /// <summary>SpellRange.dbc</summary>
        public int RangeIndex;

        /// <summary>Read from SpellRange.dbc</summary>
        [NotPersistent] public SimpleRange Range;

        /// <summary>The speed of the projectile in yards per second</summary>
        public float ProjectileSpeed;

        /// <summary>
        /// Hunter ranged spells have this. It seems always to be 75
        /// </summary>
        public SpellId ModalNextSpell;

        [Persistent] public int MaxStackCount;
        [Persistent(2)] public uint[] RequiredToolIds;
        [Persistent(8)] public uint[] ReagentIds;
        [Persistent(8)] public uint[] ReagentCounts;
        [NotPersistent] public ItemStackDescription[] Reagents;

        /// <summary>ItemClass.dbc</summary>
        public ItemClass RequiredItemClass;

        /// <summary>
        /// Mask of ItemSubClasses, used for Enchants and Combat Abilities
        /// </summary>
        public ItemSubClassMask RequiredItemSubClassMask;

        /// <summary>Mask of InventorySlots, used for Enchants only</summary>
        public InventorySlotTypeMask RequiredItemInventorySlotMask;

        /// <summary>Does not count void effect handlers</summary>
        [NotPersistent] public int EffectHandlerCount;

        [NotPersistent] public SpellEffect[] Effects;

        /// <summary>SpellVisual.dbc</summary>
        public uint Visual;

        /// <summary>SpellVisual.dbc</summary>
        public uint Visual2;

        /// <summary>SpellIcon.dbc</summary>
        public uint SpellbookIconId;

        /// <summary>SpellIcon.dbc</summary>
        public uint BuffIconId;

        public uint Priority;
        [Persistent] public string Name;
        private string m_RankDesc;
        public int Rank;
        [Persistent] public string Description;
        public string BuffDescription;
        public int PowerCostPercentage;

        /// <summary>Always 0?</summary>
        public int StartRecoveryTime;

        public int StartRecoveryCategory;
        public uint MaxTargetLevel;
        private SpellClassSet spellClassSet;
        public ClassId ClassId;
        public uint MaxTargets;
        [Persistent] public DamageType DamageType;
        public SpellPreventionType PreventionType;
        public int StanceBarOrder;

        /// <summary>only one spellid:6994 has this value = 369</summary>
        public uint MinFactionId;

        /// <summary>only one spellid:6994 has this value = 4</summary>
        public uint MinReputation;

        /// <summary>only one spellid:26869  has this flag = 1</summary>
        public uint RequiredAuraVision;

        /// <summary>AreaGroup.dbc</summary>
        public uint AreaGroupId;

        [Persistent] public DamageSchoolMask SchoolMask;

        /// <summary>SpellRuneCost.dbc</summary>
        public RuneCostEntry RuneCostEntry;

        /// <summary>SpellMissile.dbc</summary>
        public uint MissileId;

        /// <summary>PowerDisplay.dbc</summary>
        /// <remarks>Added in 3.1.0</remarks>
        public int PowerDisplayId;

        [NotPersistent] public DamageSchool[] Schools;
        [Persistent] public SpellEffectType Effect0_EffectType;
        [Persistent] public SpellMechanic Effect0_Mehanic;
        [Persistent] public ImplicitSpellTargetType Effect0_ImplicitTargetA;
        [Persistent] public ImplicitSpellTargetType Effect0_ImplicitTargetB;
        [Persistent] public float Effect0_Radius;
        [Persistent] public AuraType Effect0_AuraType;
        [Persistent] public int Effect0_Amplitude;
        [Persistent] public float Effect0_ProcValue;
        [Persistent] public int Effect0_MiscValue;
        [Persistent] public int Effect0_MiscValueB;
        [Persistent] public int Effect0_MiscValueC;
        [Persistent] public SpellEffectType Effect1_EffectType;
        [Persistent] public SpellMechanic Effect1_Mehanic;
        [Persistent] public ImplicitSpellTargetType Effect1_ImplicitTargetA;
        [Persistent] public ImplicitSpellTargetType Effect1_ImplicitTargetB;
        [Persistent] public float Effect1_Radius;
        [Persistent] public AuraType Effect1_AuraType;
        [Persistent] public int Effect1_Amplitude;
        [Persistent] public float Effect1_ProcValue;
        [Persistent] public int Effect1_MiscValue;
        [Persistent] public int Effect1_MiscValueB;
        [Persistent] public int Effect1_MiscValueC;

        /// <summary>
        /// Is called after all preparations have been made and the Spell is about to start casting.
        /// Return anything but <c>SpellFailedReason.None</c> to cancel casting.
        /// </summary>
        public event Func<SpellCast, SpellFailedReason> Casting;

        /// <summary>
        /// Is called before SpellCast is cancelled for the given reason.
        /// </summary>
        public event Action<SpellCast, SpellFailedReason> Cancelling;

        /// <summary>Is called after a SpellCast has been casted.</summary>
        public event Action<SpellCast> Casted;

        /// <summary>
        /// Is called before SpellCast is cancelled for the given reason.
        /// </summary>
        public event Action<Aura> AuraRemoved;

        /// <summary>Triggers the Casting event</summary>
        internal SpellFailedReason NotifyCasting(SpellCast cast)
        {
            Func<SpellCast, SpellFailedReason> casting = this.Casting;
            if (casting != null)
            {
                SpellFailedReason reason = casting(cast);
                if (reason != SpellFailedReason.Ok)
                {
                    cast.Cancel(reason);
                    return reason;
                }
            }

            return SpellFailedReason.Ok;
        }

        internal void NotifyCancelled(SpellCast cast, SpellFailedReason reason)
        {
            Action<SpellCast, SpellFailedReason> cancelling = this.Cancelling;
            if (cancelling == null)
                return;
            cancelling(cast, reason);
        }

        internal void NotifyCasted(SpellCast cast)
        {
            Action<SpellCast> casted = this.Casted;
            if (casted == null)
                return;
            casted(cast);
        }

        internal void NotifyAuraRemoved(Aura aura)
        {
            Action<Aura> auraRemoved = this.AuraRemoved;
            if (auraRemoved == null)
                return;
            auraRemoved(aura);
        }

        /// <summary>
        /// Will let the Caster play the given text and sound after casting
        /// </summary>
        public void AddTextAndSoundEvent(NPCAiText text)
        {
            if (text == null)
                return;
            this.Casted += (Action<SpellCast>) (cast => cast.CasterObject.PlayTextAndSound(text));
        }

        /// <summary>
        /// Whether this is a proc and whether its own effects handle procs (or false, if not a proc or custom proc handlers have been added)
        /// </summary>
        public bool IsAuraProcHandler
        {
            get
            {
                if (this.IsProc)
                    return this.ProcHandlers == null;
                return false;
            }
        }

        /// <summary>Does this Spell apply a Mount-Aura?</summary>
        public bool IsMount
        {
            get { return this.Mechanic == SpellMechanic.Mounted; }
        }

        private void InitAura()
        {
            if (this.ProcTriggerFlagsProp != ProcTriggerFlags.None || this.CasterProcSpells != null)
            {
                this.ProcTriggerEffects = ((IEnumerable<SpellEffect>) this.Effects)
                    .Where<SpellEffect>((Func<SpellEffect, bool>) (effect => effect.IsProc)).ToArray<SpellEffect>();
                if (this.ProcTriggerEffects.Length == 0)
                    this.ProcTriggerEffects = (SpellEffect[]) null;
                this.IsProc = this.ProcTriggerEffects != null || this.ProcHandlers != null ||
                              this.CasterProcSpells != null || this.ProcCharges > 0;
            }

            this.IsAura = this.IsProc ||
                          this.HasEffectWith((Predicate<SpellEffect>) (effect => effect.AuraType != AuraType.None));
            this.ForeachEffect((Action<SpellEffect>) (effect =>
            {
                if (!effect.IsAuraEffect)
                    return;
                this.HasNonPeriodicAuraEffects = this.HasNonPeriodicAuraEffects || !effect.IsPeriodic;
                this.HasPeriodicAuraEffects = this.HasPeriodicAuraEffects || effect.IsPeriodic;
            }));
            this.IsAutoRepeating = this.AttributesExB.HasFlag((Enum) SpellAttributesExB.AutoRepeat);
            this.HasManaShield =
                this.HasEffectWith((Predicate<SpellEffect>) (effect => effect.AuraType == AuraType.ManaShield));
            this.AuraEffects =
                this.GetEffectsWhere((Predicate<SpellEffect>) (effect => effect.AuraEffectHandlerCreator != null));
            this.AreaAuraEffects = this.GetEffectsWhere((Predicate<SpellEffect>) (effect => effect.IsAreaAuraEffect));
            this.IsAreaAura = this.AreaAuraEffects != null;
            this.IsPureAura = !this.IsDamageSpell && !this.HasEffectWith((Predicate<SpellEffect>) (effect =>
            {
                if (effect.EffectType == SpellEffectType.ApplyAura &&
                    effect.EffectType == SpellEffectType.ApplyAuraToMaster &&
                    effect.EffectType == SpellEffectType.ApplyStatAura)
                    return effect.EffectType != SpellEffectType.ApplyStatAuraPercent;
                return true;
            }));
            this.IsPureBuff = this.IsPureAura && this.HasBeneficialEffects && !this.HasHarmfulEffects;
            this.IsPureDebuff = this.IsPureAura && this.HasHarmfulEffects && !this.HasBeneficialEffects;
            this.IsVehicle =
                this.HasEffectWith((Predicate<SpellEffect>) (effect => effect.AuraType == AuraType.Vehicle));
            this.IsShapeshift = this.HasEffectWith((Predicate<SpellEffect>) (effect =>
            {
                if (effect.AuraType != AuraType.ModShapeshift)
                    return effect.AuraType == AuraType.Transform;
                return true;
            }));
            this.CanStack = this.MaxStackCount > 0;
            this.InitialStackCount = this.ProcCharges <= 0 ? 1 : this.ProcCharges;
            this.IsGhost = this.HasEffectWith((Predicate<SpellEffect>) (effect => effect.AuraType == AuraType.Ghost));
            this.HasFlyEffect =
                this.HasEffectWith((Predicate<SpellEffect>) (effect => effect.AuraType == AuraType.Fly));
            this.IsFlyingMount = this.IsMount &&
                                 this.HasEffectWith((Predicate<SpellEffect>) (effect =>
                                     effect.AuraType == AuraType.ModSpeedMountedFlight));
            this.CanApplyMultipleTimes = this.Attributes == (SpellAttributes.Passive | SpellAttributes.InvisibleAura) &&
                                         this.Ability == null && this.Talent == null;
            this.HasShapeshiftDependentEffects =
                this.HasEffectWith((Predicate<SpellEffect>) (effect =>
                    effect.RequiredShapeshiftMask != ShapeshiftMask.None));
            this.IsModalShapeshiftDependentAura = this.IsPassive &&
                                                  (this.RequiredShapeshiftMask != ShapeshiftMask.None ||
                                                   this.HasShapeshiftDependentEffects);
            if (this.AuraUID != 0U)
                return;
            this.CreateAuraUID();
        }

        private void CreateAuraUID()
        {
            int count = AuraHandler.AuraIdEvaluators.Count;
            for (uint index = 0; (long) index < (long) count; ++index)
            {
                if (AuraHandler.AuraIdEvaluators[(int) index](this))
                {
                    this.AuraUID = 1078U + index;
                    break;
                }
            }

            if (this.AuraUID != 0U)
                return;
            if (this.Line != null)
                this.AuraUID = this.Line.AuraUID;
            else
                this.AuraUID = AuraHandler.GetNextAuraUID();
        }

        /// <summary>
        /// Add Spells which, when casted by the owner of this Aura, can cause it to trigger this spell's procs.
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddCasterProcSpells(params SpellId[] spellIds)
        {
            Spell[] spellArray = new Spell[spellIds.Length];
            for (int index = 0; index < spellIds.Length; ++index)
            {
                SpellId spellId = spellIds[index];
                Spell spell = SpellHandler.Get(spellId);
                if (spell == null)
                    throw new InvalidSpellDataException("Invalid SpellId: " + (object) spellId, new object[0]);
                spellArray[index] = spell;
            }

            this.AddCasterProcSpells(spellArray);
        }

        /// <summary>
        /// Add Spells which, when casted by the owner of this Aura, can cause it to trigger this spell's procs.
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddCasterProcSpells(params SpellLineId[] spellSetIds)
        {
            List<Spell> spellList = new List<Spell>(spellSetIds.Length * 6);
            foreach (SpellLineId spellSetId in spellSetIds)
            {
                SpellLine line = spellSetId.GetLine();
                spellList.AddRange((IEnumerable<Spell>) line);
            }

            this.AddCasterProcSpells(spellList.ToArray());
        }

        /// <summary>
        /// Add Spells which, when casted by the owner of this Aura, can cause it to trigger this spell's procs.
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddCasterProcSpells(params Spell[] spells)
        {
            if (this.CasterProcSpells == null)
                this.CasterProcSpells = new HashSet<Spell>();
            foreach (Spell spell in spells)
                spell.GeneratesProcEventOnCast = true;
            this.CasterProcSpells.AddRange<Spell>((IEnumerable<Spell>) spells);
        }

        /// <summary>
        /// Add Spells which, when casted by others on the owner of this Aura, can cause it to trigger it's procs.
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddTargetProcSpells(params SpellId[] spellIds)
        {
            Spell[] spellArray = new Spell[spellIds.Length];
            for (int index = 0; index < spellIds.Length; ++index)
            {
                SpellId spellId = spellIds[index];
                Spell spell = SpellHandler.Get(spellId);
                if (spell == null)
                    throw new InvalidSpellDataException("Invalid SpellId: " + (object) spellId, new object[0]);
                spellArray[index] = spell;
            }

            this.AddTargetProcSpells(spellArray);
        }

        /// <summary>
        /// Add Spells which, when casted by others on the owner of this Aura, can cause it to trigger it's procs
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddTargetProcSpells(params SpellLineId[] spellSetIds)
        {
            List<Spell> spellList = new List<Spell>(spellSetIds.Length * 6);
            foreach (SpellLineId spellSetId in spellSetIds)
            {
                SpellLine line = spellSetId.GetLine();
                spellList.AddRange((IEnumerable<Spell>) line);
            }

            this.AddTargetProcSpells(spellList.ToArray());
        }

        /// <summary>
        /// Add Spells which, when casted by others on the owner of this Aura, can cause it to trigger it's procs
        /// Don't add damage spells (they will generate a Proc event anyway).
        /// </summary>
        public void AddTargetProcSpells(params Spell[] spells)
        {
            if (this.TargetProcSpells == null)
                this.TargetProcSpells = new HashSet<Spell>();
            foreach (Spell spell in spells)
                spell.GeneratesProcEventOnCast = true;
            this.TargetProcSpells.AddRange<Spell>((IEnumerable<Spell>) spells);
        }

        public List<AuraEffectHandler> CreateAuraEffectHandlers(ObjectReference caster, Unit target, bool beneficial)
        {
            return Spell.CreateAuraEffectHandlers(this.AuraEffects, caster, target, beneficial);
        }

        public static List<AuraEffectHandler> CreateAuraEffectHandlers(SpellEffect[] effects, ObjectReference caster,
            Unit target, bool beneficial)
        {
            if (effects == null)
                return (List<AuraEffectHandler>) null;
            try
            {
                List<AuraEffectHandler> auraEffectHandlerList = (List<AuraEffectHandler>) null;
                SpellFailedReason failedReason = SpellFailedReason.Ok;
                for (int index = 0; index < effects.Length; ++index)
                {
                    SpellEffect effect = effects[index];
                    if (effect.HarmType == HarmType.Beneficial || !beneficial)
                    {
                        AuraEffectHandler auraEffectHandler =
                            effect.CreateAuraEffectHandler(caster, target, ref failedReason);
                        if (failedReason != SpellFailedReason.Ok)
                            return (List<AuraEffectHandler>) null;
                        if (auraEffectHandlerList == null)
                            auraEffectHandlerList = new List<AuraEffectHandler>(3);
                        auraEffectHandlerList.Add(auraEffectHandler);
                    }
                }

                return auraEffectHandlerList;
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex,
                    "Failed to create AuraEffectHandlers for: " + (object) effects
                        .GetWhere<SpellEffect>((Func<SpellEffect, bool>) (effect => effect != null)).Spell,
                    new object[0]);
                return (List<AuraEffectHandler>) null;
            }
        }

        public bool CanOverride(Spell spell)
        {
            if (this.CanOverrideEqualAuraRank)
                return this.Rank >= spell.Rank;
            return this.Rank > spell.Rank;
        }

        public AuraIndexId GetAuraUID(ObjectReference caster, WorldObject target)
        {
            return this.GetAuraUID(this.IsBeneficialFor(caster, target));
        }

        public AuraIndexId GetAuraUID(bool positive)
        {
            return new AuraIndexId()
            {
                AuraUID = !this.CanApplyMultipleTimes
                    ? this.AuraUID
                    : AuraHandler.lastAuraUid + ++AuraHandler.randomAuraId,
                IsPositive = positive
            };
        }

        /// <summary>Tame Beast (Id: 1515) amongst others</summary>
        public bool IsTame
        {
            get { return this.AttributesExB.HasFlag((Enum) SpellAttributesExB.TamePet); }
        }

        /// <summary>whether Spell's effects don't wear off when dead</summary>
        public bool PersistsThroughDeath
        {
            get { return this.AttributesExC.HasFlag((Enum) SpellAttributesExC.PersistsThroughDeath); }
        }

        /// <summary>whether its a food effect</summary>
        public bool IsFood
        {
            get { return this.Category == 11U; }
        }

        /// <summary>whether its a drink effect</summary>
        public bool IsDrink
        {
            get { return this.Category == 59U; }
        }

        public bool IsTalent
        {
            get { return this.Talent != null; }
        }

        [NotPersistent]
        public SkillAbility Ability
        {
            get { return this.m_Ability; }
            internal set
            {
                this.m_Ability = value;
                if (value == null || this.ClassId != ClassId.NoClass)
                    return;
                ClassId[] ids = this.Ability.ClassMask.GetIds();
                if (ids.Length != 1)
                    return;
                this.ClassId = ids[0];
            }
        }

        [NotPersistent]
        public bool RepresentsSkillTier
        {
            get { return this.SkillTier != SkillTierId.End; }
        }

        public bool MatchesRequiredTargetType(WorldObject obj)
        {
            if (this.RequiredTargetType == RequiredSpellTargetType.GameObject)
                return obj is GameObject;
            if (obj is NPC)
                return ((Unit) obj).IsAlive == (this.RequiredTargetType == RequiredSpellTargetType.NPCAlive);
            return false;
        }

        public void FinalizeDataHolder()
        {
            try
            {
                this.SpellId = (SpellId) this.Id;
                this.PowerType = PowerType.Mana;
                this.Durations = new Spell.DurationEntry()
                {
                    Min = this.Duration,
                    Max = this.Duration
                };
                this.Range = new SimpleRange(0.0f, (float) this.MaxRange);
                this.ProjectileSpeed = 1f;
                this.RequiredToolIds = new uint[2];
                this.Reagents = ItemStackDescription.EmptyArray;
                this.RequiredItemClass = ItemClass.None;
                this.RequiredItemSubClassMask = ItemSubClassMask.None;
                if (this.Id == 2228U || this.Id == 2231U || (this.Id == 2234U || this.Id == 2237U) ||
                    (this.Id == 2240U || this.Id == 2243U || (this.Id == 2246U || this.Id == 2249U)) ||
                    this.Id == 2252U)
                    this.SoulGuardProffLevel = (byte) 1;
                if (this.Id == 2229U || this.Id == 2232U || (this.Id == 2235U || this.Id == 2238U) ||
                    (this.Id == 2241U || this.Id == 2244U || (this.Id == 2247U || this.Id == 2250U)) ||
                    this.Id == 2253U)
                    this.SoulGuardProffLevel = (byte) 2;
                if (this.Id == 2230U || this.Id == 2233U || (this.Id == 2236U || this.Id == 2239U) ||
                    (this.Id == 2242U || this.Id == 2245U || (this.Id == 2248U || this.Id == 2251U)) ||
                    this.Id == 2254U)
                    this.SoulGuardProffLevel = (byte) 3;
                this.RequiredItemInventorySlotMask = InventorySlotTypeMask.None;
                List<SpellEffect> spellEffectList = new List<SpellEffect>(3);
                SpellEffect spellEffect1 = new SpellEffect(this, EffectIndex.Zero)
                {
                    EffectType = this.Effect0_EffectType,
                    DiceSides = 0,
                    RealPointsPerLevel = 0.0f,
                    BasePoints = 0,
                    Mechanic = this.Effect0_Mehanic,
                    ImplicitTargetA = this.Effect0_ImplicitTargetA,
                    ImplicitTargetB = this.Effect0_ImplicitTargetB,
                    Radius = this.Effect0_Radius,
                    AuraType = this.Effect0_AuraType,
                    Amplitude = this.Effect0_Amplitude,
                    ProcValue = this.Effect0_ProcValue,
                    ChainTargets = 0,
                    MiscValue = this.Effect0_MiscValue,
                    MiscValueB = this.Effect0_MiscValueB,
                    MiscValueC = this.Effect0_MiscValueC,
                    TriggerSpellId = SpellId.None,
                    PointsPerComboPoint = 0.0f
                };
                spellEffect1.AffectMask[0] = 0U;
                spellEffect1.AffectMask[1] = 0U;
                spellEffect1.AffectMask[2] = 0U;
                if (spellEffect1.ImplicitTargetA == ImplicitSpellTargetType.AllEnemiesAroundCaster &&
                    spellEffect1.ImplicitTargetB == ImplicitSpellTargetType.AllEnemiesInArea)
                    spellEffect1.ImplicitTargetB = ImplicitSpellTargetType.None;
                spellEffectList.Add(spellEffect1);
                SpellEffect spellEffect2 = new SpellEffect(this, EffectIndex.One)
                {
                    EffectType = this.Effect1_EffectType,
                    DiceSides = 0,
                    RealPointsPerLevel = 0.0f,
                    BasePoints = 0,
                    Mechanic = this.Effect1_Mehanic,
                    ImplicitTargetA = this.Effect1_ImplicitTargetA,
                    ImplicitTargetB = this.Effect1_ImplicitTargetB,
                    Radius = this.Effect1_Radius,
                    AuraType = this.Effect1_AuraType,
                    Amplitude = this.Effect1_Amplitude,
                    ProcValue = this.Effect1_ProcValue,
                    ChainTargets = 0,
                    MiscValue = this.Effect1_MiscValue,
                    MiscValueB = this.Effect1_MiscValueB,
                    MiscValueC = this.Effect1_MiscValueC,
                    TriggerSpellId = SpellId.None,
                    PointsPerComboPoint = 0.0f
                };
                spellEffect2.AffectMask[0] = 0U;
                spellEffect2.AffectMask[1] = 0U;
                spellEffect2.AffectMask[2] = 0U;
                if (spellEffect2.ImplicitTargetA == ImplicitSpellTargetType.AllEnemiesAroundCaster &&
                    spellEffect2.ImplicitTargetB == ImplicitSpellTargetType.AllEnemiesInArea)
                    spellEffect2.ImplicitTargetB = ImplicitSpellTargetType.None;
                spellEffectList.Add(spellEffect2);
                this.Effects = spellEffectList.ToArray();
                this.PowerCostPercentage = 0;
                this.SpellClassSet = SpellClassSet.Generic;
                this.MaxTargets = 100U;
                this.PreventionType = this.DamageType == DamageType.Magic
                    ? SpellPreventionType.Magic
                    : SpellPreventionType.Melee;
                this.RequiredToolCategories = new ToolCategory[2];
                for (int index = 0; index < this.RequiredToolCategories.Length; ++index)
                    this.RequiredToolCategories[index] = ToolCategory.None;
                this.RuneCostEntry = new RuneCostEntry();
                if (this.CooldownTime > 5000)
                    this.CooldownTime -= 1000;
                else if (this.CooldownTime > 0)
                    this.CooldownTime -= 500;
                if (this.Name.Contains("Party"))
                {
                    this.Effect0_ImplicitTargetA = ImplicitSpellTargetType.AllParty;
                    this.Effect1_ImplicitTargetA = ImplicitSpellTargetType.AllParty;
                }

                SpellHandler.AddSpell(this);
            }
            catch (Exception ex)
            {
                LogUtil.WarnException("Error when finalizing data holder of spell {0}. {1}", new object[2]
                {
                    (object) this.Name,
                    (object) ex
                });
            }
        }

        public bool CanCast(NPC npc)
        {
            return this.CheckCasterConstraints((Unit) npc) == SpellFailedReason.Ok;
        }

        /// <summary>
        /// Checks whether the given spell can be casted by the casting Unit.
        /// Does not do range checks.
        /// </summary>
        public SpellFailedReason CheckCasterConstraints(Unit caster)
        {
            if (caster.IsInCombat && this.RequiresCasterOutOfCombat)
                return SpellFailedReason.AffectingCombat;
            if (!caster.CanDoHarm && this.HasHarmfulEffects)
                return SpellFailedReason.Pacified;
            if (this.InterruptFlags.HasFlag((Enum) InterruptFlags.OnSilence) &&
                caster.IsUnderInfluenceOf(SpellMechanic.Silenced))
                return SpellFailedReason.Silenced;
            if (!this.AttributesExD.HasFlag((Enum) SpellAttributesExD.UsableWhileStunned) && !caster.CanInteract)
                return SpellFailedReason.TooManySockets;
            if (!caster.CanCastSpells && (!this.IsPhysicalAbility ||
                                          this.InterruptFlags.HasFlag((Enum) InterruptFlags.OnSilence) &&
                                          caster.IsUnderInfluenceOf(SpellMechanic.Silenced)))
                return SpellFailedReason.Silenced;
            if (!caster.CanDoPhysicalActivity && this.IsPhysicalAbility || !caster.CanDoHarm && this.HasHarmfulEffects)
                return SpellFailedReason.Pacified;
            if (!this.AttributesExD.HasFlag((Enum) SpellAttributesExD.UsableWhileStunned) && !caster.CanInteract)
                return SpellFailedReason.Stunned;
            if (this.IsFinishingMove && caster.ComboPoints == 0)
                return SpellFailedReason.NoComboPoints;
            if (this.RequiredCasterAuraState != AuraState.None || this.ExcludeCasterAuraState != AuraState.None)
            {
                AuraStateMask auraState = caster.AuraState;
                if (this.RequiredCasterAuraState != AuraState.None &&
                    !auraState.HasAnyFlag(this.RequiredCasterAuraState) ||
                    this.ExcludeCasterAuraState != AuraState.None && auraState.HasAnyFlag(this.ExcludeCasterAuraState))
                    return SpellFailedReason.CasterAurastate;
            }

            if (this.ExcludeCasterAuraId != SpellId.None && caster.Auras.Contains(this.ExcludeCasterAuraId) ||
                this.RequiredCasterAuraId != SpellId.None && !caster.Auras.Contains(this.RequiredCasterAuraId))
                return SpellFailedReason.CasterAurastate;
            SpellCollection spells = caster.Spells;
            if (spells != null && caster.CastingTill > DateTime.Now && !spells.IsReady(this))
                return SpellFailedReason.NotReady;
            if (caster is NPC && caster.Target != null && (double) this.Range.MaxDist <
                (double) caster.GetDistance((WorldObject) caster.Target))
                return SpellFailedReason.OutOfRange;
            return !this.IsPassive && !caster.HasEnoughPowerToCast(this, (WorldObject) null)
                ? SpellFailedReason.NoPower
                : SpellFailedReason.Ok;
        }

        /// <summary>Whether this spell has certain requirements on items</summary>
        public bool HasItemRequirements
        {
            get
            {
                if ((this.RequiredItemClass == ItemClass.Consumable || this.RequiredItemClass == ItemClass.None) &&
                    (this.RequiredItemInventorySlotMask == InventorySlotTypeMask.None && this.RequiredTools == null) &&
                    this.RequiredToolCategories.Length <= 0)
                    return this.EquipmentSlot != EquipmentSlot.End;
                return true;
            }
        }

        public SpellFailedReason CheckItemRestrictions(Item usedItem, PlayerInventory inv)
        {
            if (this.RequiredItemClass != ItemClass.None)
            {
                if (this.EquipmentSlot != EquipmentSlot.End)
                    usedItem = inv[this.EquipmentSlot];
                if (usedItem == null)
                    return SpellFailedReason.EquippedItem;
                if (this.RequiredItemClass > ItemClass.Consumable &&
                    (usedItem.Template.Class != this.RequiredItemClass ||
                     this.RequiredItemSubClassMask > ItemSubClassMask.None &&
                     !usedItem.Template.SubClassMask.HasAnyFlag(this.RequiredItemSubClassMask)))
                    return SpellFailedReason.EquippedItemClass;
            }

            if (this.RequiredItemInventorySlotMask != InventorySlotTypeMask.None && usedItem != null &&
                (usedItem.Template.InventorySlotMask & this.RequiredItemInventorySlotMask) ==
                InventorySlotTypeMask.None)
                return SpellFailedReason.EquippedItemClass;
            return this.CheckGeneralItemRestrictions(inv);
        }

        /// <summary>
        /// Checks whether the given inventory satisfies this Spell's item restrictions
        /// </summary>
        public SpellFailedReason CheckItemRestrictions(PlayerInventory inv)
        {
            return this.CheckItemRestrictionsWithout(inv, (Item) null);
        }

        /// <summary>
        /// Checks whether the given inventory satisfies this Spell's item restrictions
        /// </summary>
        public SpellFailedReason CheckItemRestrictionsWithout(PlayerInventory inv, Item exclude)
        {
            if (this.RequiredItemClass == ItemClass.Armor || this.RequiredItemClass == ItemClass.Weapon)
            {
                if (this.EquipmentSlot != EquipmentSlot.End)
                {
                    Item obj = inv[this.EquipmentSlot];
                    if (obj == null || obj == exclude)
                        return SpellFailedReason.EquippedItem;
                    if (!this.CheckItemRestriction(obj))
                        return SpellFailedReason.EquippedItemClass;
                }
                else if (inv.Iterate(ItemMgr.EquippableInvSlotsByClass[(int) this.RequiredItemClass],
                    (Func<Item, bool>) (i =>
                    {
                        if (i != exclude)
                            return !this.CheckItemRestriction(i);
                        return true;
                    })))
                    return SpellFailedReason.EquippedItemClass;
            }

            if (this.RequiredItemInventorySlotMask != InventorySlotTypeMask.None && inv.Iterate(
                    this.RequiredItemInventorySlotMask, (Func<Item, bool>) (item =>
                    {
                        if (item != exclude)
                            return (item.Template.InventorySlotMask & this.RequiredItemInventorySlotMask) ==
                                   InventorySlotTypeMask.None;
                        return true;
                    })))
                return SpellFailedReason.EquippedItemClass;
            return this.CheckGeneralItemRestrictions(inv);
        }

        private bool CheckItemRestriction(Item item)
        {
            return item.Template.Class == this.RequiredItemClass &&
                   (this.RequiredItemSubClassMask <= ItemSubClassMask.None ||
                    item.Template.SubClassMask.HasAnyFlag(this.RequiredItemSubClassMask));
        }

        public SpellFailedReason CheckGeneralItemRestrictions(PlayerInventory inv)
        {
            if (this.RequiredTools != null)
            {
                foreach (ItemTemplate requiredTool in this.RequiredTools)
                {
                    if (!inv.Contains(requiredTool.Id, 1, false))
                        return SpellFailedReason.ItemNotFound;
                }
            }

            if (this.RequiredToolCategories.Length > 0 && !inv.CheckTotemCategories(this.RequiredToolCategories))
                return SpellFailedReason.TotemCategory;
            if (this.EquipmentSlot != EquipmentSlot.End)
            {
                Item obj = inv[this.EquipmentSlot];
                if (obj == null ||
                    this.AttributesExC.HasFlag((Enum) SpellAttributesExC.RequiresWand) &&
                    obj.Template.SubClass != ItemSubClass.WeaponWand ||
                    this.AttributesExC.HasFlag((Enum) SpellAttributesExC.ShootRangedWeapon) &&
                    !obj.Template.IsRangedWeapon)
                    return SpellFailedReason.EquippedItem;
            }

            return SpellFailedReason.Ok;
        }

        /// <summary>
        /// Checks whether the given target is valid for the given caster.
        /// Is called automatically when SpellCast selects Targets.
        /// Does not do maximum range check.
        /// </summary>
        public SpellFailedReason CheckValidTarget(WorldObject caster, WorldObject target)
        {
            if (this.AttributesEx.HasAnyFlag(SpellAttributesEx.CannotTargetSelf) && target == caster)
                return SpellFailedReason.NoValidTargets;
            if (target is Unit)
            {
                if (this.RequiredTargetAuraState != AuraState.None || this.ExcludeTargetAuraState != AuraState.None)
                {
                    AuraStateMask auraState = ((Unit) target).AuraState;
                    if (this.RequiredTargetAuraState != AuraState.None &&
                        !auraState.HasAnyFlag(this.RequiredTargetAuraState) ||
                        this.ExcludeTargetAuraState != AuraState.None &&
                        auraState.HasAnyFlag(this.ExcludeTargetAuraState))
                        return SpellFailedReason.TargetAurastate;
                }

                if (this.ExcludeTargetAuraId != SpellId.None &&
                    ((Unit) target).Auras.Contains(this.ExcludeTargetAuraId) ||
                    this.RequiredTargetAuraId != SpellId.None &&
                    !((Unit) target).Auras.Contains(this.RequiredTargetAuraId))
                    return SpellFailedReason.TargetAurastate;
            }

            if (this.TargetFlags.HasAnyFlag(SpellTargetFlags.UnkUnit_0x100) &&
                (!(target is GameObject) || !target.IsInWorld))
                return SpellFailedReason.BadTargets;
            if (!this.CanCastOnPlayer && target is Character)
                return SpellFailedReason.BadImplicitTargets;
            if (this.RequiresDeadTarget)
            {
                if (this.TargetFlags.HasAnyFlag(SpellTargetFlags.PvPCorpse | SpellTargetFlags.Corpse))
                {
                    if (!(target is Corpse) || this.TargetFlags.HasAnyFlag(SpellTargetFlags.PvPCorpse) &&
                        caster != null && !caster.IsHostileWith((IFactionMember) target))
                        return SpellFailedReason.BadImplicitTargets;
                }
                else if (target is NPC)
                {
                    if (((Unit) target).IsAlive || target.Loot != null)
                        return SpellFailedReason.TargetNotDead;
                }
                else if (target is Character && ((Unit) target).IsAlive)
                    return SpellFailedReason.TargetNotDead;
            }
            else if (target is Unit && !((Unit) target).IsAlive)
                return SpellFailedReason.TargetsDead;

            if (this.AttributesExB.HasFlag((Enum) SpellAttributesExB.RequiresBehindTarget) && caster != null &&
                !caster.IsBehind(target))
                return SpellFailedReason.NotBehind;
            return (double) this.Range.MinDist > 0.0 && caster != null && caster.IsInRadius(target, this.Range.MinDist)
                ? SpellFailedReason.TooClose
                : SpellFailedReason.Ok;
        }

        public bool CanProcBeTriggeredBy(Unit owner, IUnitAction action, bool active)
        {
            if (action.Spell != null)
            {
                if (active)
                {
                    if (this.CasterProcSpells != null)
                        return this.CasterProcSpells.Contains(action.Spell);
                }
                else if (this.TargetProcSpells != null)
                    return this.TargetProcSpells.Contains(action.Spell);

                if (action.Spell == this)
                    return false;
            }

            if (this.RequiredItemClass == ItemClass.None)
                return true;
            if (!(action is DamageAction))
                return false;
            DamageAction damageAction = (DamageAction) action;
            if (damageAction.Weapon == null || !(damageAction.Weapon is Item))
                return false;
            ItemTemplate template = ((Item) damageAction.Weapon).Template;
            if (template.Class != this.RequiredItemClass)
                return false;
            if (this.RequiredItemSubClassMask != ItemSubClassMask.None)
                return template.SubClassMask.HasAnyFlag(this.RequiredItemSubClassMask);
            return true;
        }

        public int GetCooldown(Unit unit)
        {
            int cd = !(unit is NPC) || this.AISettings.Cooldown.MaxDelay <= 0
                ? this.CooldownTime
                : this.AISettings.Cooldown.GetRandomCooldown();
            if (cd == 0)
            {
                if (this.HasIndividualCooldown)
                {
                    if (unit is Character)
                    {
                        Item obj = ((Character) unit).Inventory[this.EquipmentSlot];
                        if (obj != null)
                            cd = obj.AttackTime;
                    }
                    else if (unit is NPC)
                        cd = ((NPC) unit).Entry.AttackTime;
                }
            }
            else
                cd = this.GetModifiedCooldown(unit, cd);

            return cd;
        }

        public int GetModifiedCooldown(Unit unit, int cd)
        {
            return unit.Auras.GetModifiedInt(SpellModifierType.CooldownTime, this, cd) + unit.IntMods[33];
        }

        public Spell()
        {
            this.AISettings = new AISpellSettings(this);
        }

        /// <summary>Add Spells to be casted on the targets of this Spell</summary>
        public void AddTargetTriggerSpells(params SpellId[] spellIds)
        {
            Spell[] spellArray = new Spell[spellIds.Length];
            for (int index = 0; index < spellIds.Length; ++index)
            {
                SpellId spellId = spellIds[index];
                Spell spell = SpellHandler.Get(spellId);
                if (spell == null)
                    throw new InvalidSpellDataException("Invalid SpellId: " + (object) spellId, new object[0]);
                spellArray[index] = spell;
            }

            this.AddTargetTriggerSpells(spellArray);
        }

        /// <summary>Add Spells to be casted on the targets of this Spell</summary>
        public void AddTargetTriggerSpells(params Spell[] spells)
        {
            if (this.TargetTriggerSpells == null)
            {
                this.TargetTriggerSpells = spells;
            }
            else
            {
                int length = this.TargetTriggerSpells.Length;
                Array.Resize<Spell>(ref this.TargetTriggerSpells, length + spells.Length);
                Array.Copy((Array) spells, 0, (Array) this.TargetTriggerSpells, length, spells.Length);
            }
        }

        /// <summary>Add Spells to be casted on the targets of this Spell</summary>
        public void AddCasterTriggerSpells(params SpellId[] spellIds)
        {
            Spell[] spellArray = new Spell[spellIds.Length];
            for (int index = 0; index < spellIds.Length; ++index)
            {
                SpellId spellId = spellIds[index];
                Spell spell = SpellHandler.Get(spellId);
                if (spell == null)
                    throw new InvalidSpellDataException("Invalid SpellId: " + (object) spellId, new object[0]);
                spellArray[index] = spell;
            }

            this.AddCasterTriggerSpells(spellArray);
        }

        /// <summary>Add Spells to be casted on the targets of this Spell</summary>
        public void AddCasterTriggerSpells(params Spell[] spells)
        {
            if (this.CasterTriggerSpells == null)
            {
                this.CasterTriggerSpells = spells;
            }
            else
            {
                int length = this.CasterTriggerSpells.Length;
                Array.Resize<Spell>(ref this.CasterTriggerSpells, length + spells.Length);
                Array.Copy((Array) spells, 0, (Array) this.CasterTriggerSpells, length, spells.Length);
            }
        }

        /// <summary>
        /// Add Handler to be enabled when this aura spell is active
        /// </summary>
        public void AddProcHandler(ProcHandlerTemplate handler)
        {
            if (this.ProcHandlers == null)
                this.ProcHandlers = new List<ProcHandlerTemplate>();
            this.ProcHandlers.Add(handler);
            if (this.Effects.Length != 0)
                return;
            this.AddAuraEffect(AuraType.Dummy);
        }

        /// <summary>Sets all default variables</summary>
        public void Initialize()
        {
            this.init1 = true;
            SpellEffect spellEffect = this.GetEffect(SpellEffectType.LearnSpell) ??
                                      this.GetEffect(SpellEffectType.LearnPetSpell);
            if (spellEffect != null && spellEffect.TriggerSpellId != SpellId.None)
                this.IsTeachSpell = true;
            for (int index = 0; index < this.Effects.Length; ++index)
            {
                SpellEffect effect = this.Effects[index];
                if (effect.TriggerSpellId != SpellId.None || effect.AuraType == AuraType.PeriodicTriggerSpell)
                {
                    Spell spell = SpellHandler.Get((uint) effect.TriggerSpellId);
                    if (spell != null)
                    {
                        if (!this.IsTeachSpell)
                            spell.IsTriggeredSpell = true;
                        else
                            this.LearnSpell = spell;
                        effect.TriggerSpell = spell;
                    }
                    else if (this.IsTeachSpell)
                        this.IsTeachSpell = this.GetEffect(SpellEffectType.LearnSpell) != null;
                }
            }

            foreach (SpellEffect effect in this.Effects)
            {
                if (effect.EffectType == SpellEffectType.PersistantAreaAura)
                {
                    this.DOEffect = effect;
                    break;
                }
            }
        }

        /// <summary>
        /// For all things that depend on info of all spells from first Init-round and other things
        /// </summary>
        internal void Init2()
        {
            if (this.init2)
            {
                return;
            }

            this.init2 = true;
            this.IsPassive = this.Attributes.HasFlag(SpellAttributes.Passive);
            this.IsChanneled =
                ((!this.IsPassive &&
                  this.AttributesEx.HasAnyFlag(SpellAttributesEx.Channeled_1 | SpellAttributesEx.Channeled_2)) ||
                 this.ChannelInterruptFlags > ChannelInterruptFlags.None);
            foreach (SpellEffect spellEffect in this.Effects)
            {
                spellEffect.Init2();
                if (spellEffect.IsHealEffect)
                {
                    this.IsHealSpell = true;
                }

                if (spellEffect.EffectType == SpellEffectType.NormalizedWeaponDamagePlus)
                {
                    this.IsDualWieldAbility = true;
                }
            }

            this.InitAura();
            if (this.IsChanneled)
            {
                if (this.Durations.Min == 0)
                {
                    this.Durations.Min = (this.Durations.Max = 1000);
                }

                foreach (SpellEffect spellEffect2 in this.Effects)
                {
                    if (spellEffect2.IsPeriodic)
                    {
                        this.ChannelAmplitude = spellEffect2.Amplitude;
                        break;
                    }
                }
            }

            this.IsOnNextStrike =
                this.Attributes.HasAnyFlag(SpellAttributes.OnNextMelee | SpellAttributes.OnNextMelee_2);
            this.IsRanged = (this.Attributes.HasAnyFlag(SpellAttributes.Ranged) ||
                             this.AttributesExC.HasFlag(SpellAttributesExC.ShootRangedWeapon));
            this.IsRangedAbility = (this.IsRanged && !this.IsTriggeredSpell);
            this.IsStrikeSpell = this.HasEffectWith((SpellEffect effect) => effect.IsStrikeEffect);
            this.IsPhysicalAbility = ((this.IsRangedAbility || this.IsOnNextStrike || this.IsStrikeSpell) &&
                                      !this.HasEffect(SpellEffectType.SchoolDamage));
            this.DamageIncreasedByAP = false;
            this.GeneratesComboPoints =
                this.HasEffectWith((SpellEffect effect) => effect.EffectType == SpellEffectType.AddComboPoints);
            bool isFinishingMove;
            if (!this.AttributesEx.HasAnyFlag(SpellAttributesEx.FinishingMove))
            {
                isFinishingMove = this.HasEffectWith((SpellEffect effect) =>
                    effect.PointsPerComboPoint > 0f && effect.EffectType != SpellEffectType.Dummy);
            }
            else
            {
                isFinishingMove = true;
            }

            this.IsFinishingMove = isFinishingMove;
            this.TotemEffect = this.GetFirstEffectWith((SpellEffect effect) => effect.HasTarget(
                new ImplicitSpellTargetType[]
                {
                    ImplicitSpellTargetType.TotemAir,
                    ImplicitSpellTargetType.TotemEarth,
                    ImplicitSpellTargetType.TotemFire,
                    ImplicitSpellTargetType.TotemWater
                }));
            this.IsEnchantment = this.HasEffectWith((SpellEffect effect) => effect.IsEnchantmentEffect);
            if (!this.IsEnchantment && this.EquipmentSlot == EquipmentSlot.End)
            {
                if (this.RequiredItemClass == ItemClass.Armor &&
                    this.RequiredItemSubClassMask == ItemSubClassMask.Shield)
                {
                    this.EquipmentSlot = EquipmentSlot.OffHand;
                }
                else if (this.AttributesExC.HasFlag(SpellAttributesExC.RequiresOffHandWeapon))
                {
                    this.EquipmentSlot = EquipmentSlot.OffHand;
                }
                else if (this.IsRangedAbility || this.AttributesExC.HasFlag(SpellAttributesExC.RequiresWand))
                {
                    this.EquipmentSlot = EquipmentSlot.ExtraWeapon;
                }
                else if (this.AttributesExC.HasFlag(SpellAttributesExC.RequiresMainHandWeapon))
                {
                    this.EquipmentSlot = EquipmentSlot.MainHand;
                }
                else if (this.RequiredItemClass == ItemClass.Weapon)
                {
                    if (this.RequiredItemSubClassMask == ItemSubClassMask.AnyMeleeWeapon)
                    {
                        this.EquipmentSlot = EquipmentSlot.MainHand;
                    }
                    else if (this.RequiredItemSubClassMask.HasAnyFlag(ItemSubClassMask.AnyRangedAndThrownWeapon))
                    {
                        this.EquipmentSlot = EquipmentSlot.ExtraWeapon;
                    }
                }
                else if (this.IsPhysicalAbility)
                {
                    this.EquipmentSlot = EquipmentSlot.MainHand;
                }
            }

            this.HasIndividualCooldown = (this.CooldownTime > 0 ||
                                          (this.IsPhysicalAbility && !this.IsOnNextStrike &&
                                           this.EquipmentSlot != EquipmentSlot.End));
            this.HasCooldown = (this.HasIndividualCooldown || this.CategoryCooldownTime > 0);
            SpellEffect effect2 = this.GetEffect(SpellEffectType.SkillStep);
            if (effect2 != null)
            {
                this.TeachesApprenticeAbility = (effect2.BasePoints == 0);
            }

            this.IsProfession = (!this.IsRangedAbility && this.Ability != null &&
                                 this.Ability.Skill.Category == SkillCategory.Profession);
            this.IsEnhancer = this.HasEffectWith((SpellEffect effect) => effect.IsEnhancer);
            this.IsFishing =
                this.HasEffectWith((SpellEffect effect) => effect.HasTarget(ImplicitSpellTargetType.SelfFishing));
            this.IsSkinning = this.HasEffectWith((SpellEffect effect) => effect.EffectType == SpellEffectType.Skinning);
            this.IsTameEffect =
                this.HasEffectWith((SpellEffect effect) => effect.EffectType == SpellEffectType.TameCreature);
            if (this.AttributesEx.HasAnyFlag(SpellAttributesEx.Negative) || this.IsPreventionDebuff ||
                this.Mechanic.IsNegative())
            {
                this.HasHarmfulEffects = true;
                this.HasBeneficialEffects = false;
                this.HarmType = HarmType.Harmful;
            }
            else
            {
                this.HasHarmfulEffects =
                    this.HasEffectWith((SpellEffect effect) => effect.HarmType == HarmType.Harmful);
                this.HasBeneficialEffects =
                    this.HasEffectWith((SpellEffect effect) => effect.HarmType == HarmType.Beneficial);
                if (this.HasHarmfulEffects != this.HasBeneficialEffects)
                {
                    if (!this.HasEffectWith((SpellEffect effect) => effect.HarmType == HarmType.Neutral))
                    {
                        this.HarmType = (this.HasHarmfulEffects ? HarmType.Harmful : HarmType.Beneficial);
                        goto IL_59E;
                    }
                }

                this.HarmType = HarmType.Neutral;
            }

            IL_59E:
            this.RequiresDeadTarget = (this.HasEffect(SpellEffectType.Resurrect) ||
                                       this.HasEffect(SpellEffectType.ResurrectFlat) ||
                                       this.HasEffect(SpellEffectType.SelfResurrect));
            this.CostsPower = (this.PowerCost > 0 || this.PowerCostPercentage > 0);
            this.CostsRunes = (this.RuneCostEntry != null && this.RuneCostEntry.CostsRunes);
            this.HasTargets = this.HasEffectWith((SpellEffect effect) => effect.HasTargets);
            bool casterIsTarget;
            if (this.HasTargets)
            {
                casterIsTarget =
                    this.HasEffectWith((SpellEffect effect) => effect.HasTarget(ImplicitSpellTargetType.Self));
            }
            else
            {
                casterIsTarget = false;
            }

            this.CasterIsTarget = casterIsTarget;
            this.IsAreaSpell = this.HasEffectWith((SpellEffect effect) => effect.IsAreaEffect);
            bool isDamageSpell;
            if (this.HasHarmfulEffects && !this.HasBeneficialEffects)
            {
                isDamageSpell = this.HasEffectWith((SpellEffect effect) =>
                    effect.EffectType == SpellEffectType.Attack ||
                    effect.EffectType == SpellEffectType.EnvironmentalDamage ||
                    effect.EffectType == SpellEffectType.InstantKill ||
                    effect.EffectType == SpellEffectType.SchoolDamage || effect.IsStrikeEffect);
            }
            else
            {
                isDamageSpell = false;
            }

            this.IsDamageSpell = isDamageSpell;
            if (this.DamageMultipliers[0] <= 0f)
            {
                this.DamageMultipliers[0] = 1f;
            }

            this.IsHearthStoneSpell = this.HasEffectWith((SpellEffect effect) =>
                effect.HasTarget(ImplicitSpellTargetType.HeartstoneLocation));
            this.ForeachEffect(delegate(SpellEffect effect)
            {
                if (effect.ImplicitTargetA == ImplicitSpellTargetType.None &&
                    effect.EffectType == SpellEffectType.ResurrectFlat)
                {
                    effect.ImplicitTargetA = ImplicitSpellTargetType.SingleFriend;
                }
            });
            this.Schools = Utility.GetSetIndices<DamageSchool>((uint) this.SchoolMask);
            if (this.Schools.Length == 0)
            {
                DamageSchool[] schools = new DamageSchool[1];
                this.Schools = schools;
            }

            this.RequiresCasterOutOfCombat = (!this.HasHarmfulEffects &&
                                              (this.Attributes.HasFlag(SpellAttributes.CannotBeCastInCombat) ||
                                               this.AttributesEx.HasFlag(SpellAttributesEx.RemainOutOfCombat) ||
                                               this.AuraInterruptFlags.HasFlag(AuraInterruptFlags.OnStartAttack)));
            if (this.RequiresCasterOutOfCombat)
            {
                this.InterruptFlags |= InterruptFlags.OnTakeDamage;
            }

            this.IsThrow = (this.AttributesExC.HasFlag(SpellAttributesExC.ShootRangedWeapon) &&
                            this.Attributes.HasFlag(SpellAttributes.Ranged) && this.Ability != null &&
                            this.Ability.Skill.Id == SkillId.Thrown);
            bool hasModifierEffects;
            if (!this.HasModifierEffects)
            {
                hasModifierEffects = this.HasEffectWith((SpellEffect effect) =>
                    effect.AuraType == AuraType.AddModifierFlat || effect.AuraType == AuraType.AddModifierPercent);
            }
            else
            {
                hasModifierEffects = true;
            }

            this.HasModifierEffects = hasModifierEffects;
            this.CanCastOnPlayer = (this.CanCastOnPlayer && !this.HasEffect(AuraType.ModTaunt));
            this.HasAuraDependentEffects = this.HasEffectWith((SpellEffect effect) => effect.IsDependentOnOtherAuras);
            this.ForeachEffect(delegate(SpellEffect effect)
            {
                for (int k = 0; k < 3; k++)
                {
                    this.AllAffectingMasks[k] |= effect.AffectMask[k];
                }
            });
            if (this.Range.MaxDist == 0f)
            {
                this.Range.MaxDist = 5f;
            }

            if (this.RequiredToolIds == null)
            {
                this.RequiredToolIds = new uint[0];
            }
            else
            {
                if (this.RequiredToolIds.Length > 0 && (this.RequiredToolIds[0] > 0u || this.RequiredToolIds[1] > 0u))
                {
                    SpellHandler.SpellsRequiringTools.Add(this);
                }

                ArrayUtil.PruneVals<uint>(ref this.RequiredToolIds);
            }

            SpellEffect firstEffectWith = this.GetFirstEffectWith((SpellEffect effect) =>
                effect.EffectType == SpellEffectType.SkillStep || effect.EffectType == SpellEffectType.Skill);
            if (firstEffectWith != null)
            {
                this.SkillTier = (SkillTierId) firstEffectWith.BasePoints;
            }
            else
            {
                this.SkillTier = SkillTierId.End;
            }

            ArrayUtil.PruneVals<ToolCategory>(ref this.RequiredToolCategories);
            this.ForeachEffect(delegate(SpellEffect effect)
            {
                if (effect.SpellEffectHandlerCreator != null)
                {
                    this.EffectHandlerCount++;
                }
            });
            if (this.GetEffect(SpellEffectType.QuestComplete) != null)
            {
                SpellHandler.QuestCompletors.Add(this);
            }

            this.AISettings.InitializeAfterLoad();
        }

        /// <summary>Sets the AITargetHandlerDefintion of all effects</summary>
        public void OverrideCustomTargetDefinitions(TargetAdder adder, params TargetFilter[] filters)
        {
            this.OverrideCustomTargetDefinitions(new TargetDefinition(adder, filters), (TargetEvaluator) null);
        }

        /// <summary>Sets the CustomTargetHandlerDefintion of all effects</summary>
        public void OverrideCustomTargetDefinitions(TargetAdder adder, TargetEvaluator evaluator = null,
            params TargetFilter[] filters)
        {
            this.OverrideCustomTargetDefinitions(new TargetDefinition(adder, filters), evaluator);
        }

        public void OverrideCustomTargetDefinitions(TargetDefinition def, TargetEvaluator evaluator = null)
        {
            this.ForeachEffect((Action<SpellEffect>) (effect => effect.CustomTargetHandlerDefintion = def));
            if (evaluator == null)
                return;
            this.OverrideCustomTargetEvaluators(evaluator);
        }

        /// <summary>Sets the AITargetHandlerDefintion of all effects</summary>
        public void OverrideAITargetDefinitions(TargetAdder adder, params TargetFilter[] filters)
        {
            this.OverrideAITargetDefinitions(new TargetDefinition(adder, filters), (TargetEvaluator) null);
        }

        /// <summary>Sets the AITargetHandlerDefintion of all effects</summary>
        public void OverrideAITargetDefinitions(TargetAdder adder, TargetEvaluator evaluator = null,
            params TargetFilter[] filters)
        {
            this.OverrideAITargetDefinitions(new TargetDefinition(adder, filters), evaluator);
        }

        public void OverrideAITargetDefinitions(TargetDefinition def, TargetEvaluator evaluator = null)
        {
            this.ForeachEffect((Action<SpellEffect>) (effect => effect.AITargetHandlerDefintion = def));
            if (evaluator == null)
                return;
            this.OverrideCustomTargetEvaluators(evaluator);
        }

        /// <summary>Sets the CustomTargetEvaluator of all effects</summary>
        public void OverrideCustomTargetEvaluators(TargetEvaluator eval)
        {
            this.ForeachEffect((Action<SpellEffect>) (effect => effect.CustomTargetEvaluator = eval));
        }

        /// <summary>Sets the AITargetEvaluator of all effects</summary>
        public void OverrideAITargetEvaluators(TargetEvaluator eval)
        {
            this.ForeachEffect((Action<SpellEffect>) (effect => effect.AITargetEvaluator = eval));
        }

        public void ForeachEffect(Action<SpellEffect> callback)
        {
            for (int index = 0; index < this.Effects.Length; ++index)
            {
                SpellEffect effect = this.Effects[index];
                callback(effect);
            }
        }

        public bool HasEffectWith(Predicate<SpellEffect> predicate)
        {
            for (int index = 0; index < this.Effects.Length; ++index)
            {
                SpellEffect effect = this.Effects[index];
                if (predicate(effect))
                    return true;
            }

            return false;
        }

        public bool HasEffect(SpellEffectType type)
        {
            return this.GetEffect(type, false) != null;
        }

        public bool HasEffect(AuraType type)
        {
            return this.GetEffect(type, false) != null;
        }

        /// <summary>
        /// Returns the first SpellEffect of the given Type within this Spell
        /// </summary>
        public SpellEffect GetEffect(SpellEffectType type)
        {
            return this.GetEffect(type, true);
        }

        /// <summary>
        /// Returns the first SpellEffect of the given Type within this Spell
        /// </summary>
        public SpellEffect GetEffect(SpellEffectType type, bool force)
        {
            foreach (SpellEffect effect in this.Effects)
            {
                if (effect.EffectType == type)
                    return effect;
            }

            if (!this.init1 && force)
                throw new ContentException("Spell {0} does not contain Effect of type {1}", new object[2]
                {
                    (object) this,
                    (object) type
                });
            return (SpellEffect) null;
        }

        /// <summary>
        /// Returns the first SpellEffect of the given Type within this Spell
        /// </summary>
        public SpellEffect GetEffect(AuraType type)
        {
            return this.GetEffect(type, ContentMgr.ForceDataPresence);
        }

        /// <summary>
        /// Returns the first SpellEffect of the given Type within this Spell
        /// </summary>
        public SpellEffect GetEffect(AuraType type, bool force)
        {
            foreach (SpellEffect effect in this.Effects)
            {
                if (effect.AuraType == type)
                    return effect;
            }

            if (!this.init1 && force)
                throw new ContentException("Spell {0} does not contain Aura Effect of type {1}", new object[2]
                {
                    (object) this,
                    (object) type
                });
            return (SpellEffect) null;
        }

        public SpellEffect GetFirstEffectWith(Predicate<SpellEffect> predicate)
        {
            foreach (SpellEffect effect in this.Effects)
            {
                if (predicate(effect))
                    return effect;
            }

            return (SpellEffect) null;
        }

        public SpellEffect[] GetEffectsWhere(Predicate<SpellEffect> predicate)
        {
            List<SpellEffect> spellEffectList = (List<SpellEffect>) null;
            foreach (SpellEffect effect in this.Effects)
            {
                if (predicate(effect))
                {
                    if (spellEffectList == null)
                        spellEffectList = new List<SpellEffect>();
                    spellEffectList.Add(effect);
                }
            }

            return spellEffectList?.ToArray();
        }

        /// <summary>Adds a new Effect to this Spell</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public SpellEffect AddEffect(SpellEffectHandlerCreator creator, ImplicitSpellTargetType target)
        {
            SpellEffect spellEffect = this.AddEffect(SpellEffectType.Dummy, target);
            spellEffect.SpellEffectHandlerCreator = creator;
            return spellEffect;
        }

        /// <summary>Adds a new Effect to this Spell</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public SpellEffect AddEffect(SpellEffectType type, ImplicitSpellTargetType target)
        {
            SpellEffect spellEffect = new SpellEffect(this, EffectIndex.Custom)
            {
                EffectType = type
            };
            SpellEffect[] spellEffectArray = new SpellEffect[this.Effects.Length + 1];
            Array.Copy((Array) this.Effects, (Array) spellEffectArray, this.Effects.Length);
            this.Effects = spellEffectArray;
            this.Effects[spellEffectArray.Length - 1] = spellEffect;
            spellEffect.ImplicitTargetA = target;
            return spellEffect;
        }

        /// <summary>
        /// Adds a SpellEffect that will trigger the given Spell on oneself
        /// </summary>
        public SpellEffect AddTriggerSpellEffect(SpellId triggerSpell)
        {
            return this.AddTriggerSpellEffect(triggerSpell, ImplicitSpellTargetType.Self);
        }

        /// <summary>
        /// Adds a SpellEffect that will trigger the given Spell on the given type of target
        /// </summary>
        public SpellEffect AddTriggerSpellEffect(SpellId triggerSpell, ImplicitSpellTargetType targetType)
        {
            SpellEffect spellEffect = this.AddEffect(SpellEffectType.TriggerSpell, targetType);
            spellEffect.TriggerSpellId = triggerSpell;
            return spellEffect;
        }

        /// <summary>
        /// Adds a SpellEffect that will trigger the given Spell on oneself
        /// </summary>
        public SpellEffect AddPeriodicTriggerSpellEffect(SpellId triggerSpell)
        {
            return this.AddPeriodicTriggerSpellEffect(triggerSpell, ImplicitSpellTargetType.Self);
        }

        /// <summary>
        /// Adds a SpellEffect that will trigger the given Spell on the given type of target
        /// </summary>
        public SpellEffect AddPeriodicTriggerSpellEffect(SpellId triggerSpell, ImplicitSpellTargetType targetType)
        {
            SpellEffect spellEffect = this.AddAuraEffect(AuraType.PeriodicTriggerSpell);
            spellEffect.TriggerSpellId = triggerSpell;
            spellEffect.ImplicitTargetA = targetType;
            return spellEffect;
        }

        /// <summary>
        /// Adds a SpellEffect that will be applied to an Aura to be casted on oneself
        /// </summary>
        public SpellEffect AddAuraEffect(AuraType type)
        {
            return this.AddAuraEffect(type, ImplicitSpellTargetType.Self);
        }

        /// <summary>
        /// Adds a SpellEffect that will be applied to an Aura to be casted on the given type of target
        /// </summary>
        public SpellEffect AddAuraEffect(AuraType type, ImplicitSpellTargetType targetType)
        {
            SpellEffect spellEffect = this.AddEffect(SpellEffectType.ApplyAura, targetType);
            spellEffect.AuraType = type;
            return spellEffect;
        }

        /// <summary>
        /// Adds a SpellEffect that will be applied to an Aura to be casted on the given type of target
        /// </summary>
        public SpellEffect AddAuraEffect(AuraEffectHandlerCreator creator)
        {
            return this.AddAuraEffect(creator, ImplicitSpellTargetType.Self);
        }

        /// <summary>
        /// Adds a SpellEffect that will be applied to an Aura to be casted on the given type of target
        /// </summary>
        public SpellEffect AddAuraEffect(AuraEffectHandlerCreator creator, ImplicitSpellTargetType targetType)
        {
            SpellEffect spellEffect = this.AddEffect(SpellEffectType.ApplyAura, targetType);
            spellEffect.AuraType = AuraType.Dummy;
            spellEffect.AuraEffectHandlerCreator = creator;
            return spellEffect;
        }

        public void ClearEffects()
        {
            this.Effects = new SpellEffect[0];
        }

        public SpellEffect RemoveEffect(AuraType type)
        {
            SpellEffect effect = this.GetEffect(type);
            this.RemoveEffect(effect);
            return effect;
        }

        public SpellEffect RemoveEffect(SpellEffectType type)
        {
            SpellEffect effect = this.GetEffect(type);
            this.RemoveEffect(effect);
            return effect;
        }

        public void RemoveEffect(SpellEffect toRemove)
        {
            SpellEffect[] spellEffectArray = new SpellEffect[this.Effects.Length - 1];
            int num = 0;
            foreach (SpellEffect effect in this.Effects)
            {
                if (effect != toRemove)
                    spellEffectArray[num++] = effect;
            }

            this.Effects = spellEffectArray;
        }

        public void RemoveEffect(Func<SpellEffect, bool> predicate)
        {
            foreach (SpellEffect toRemove in ((IEnumerable<SpellEffect>) this.Effects).ToArray<SpellEffect>())
            {
                if (predicate(toRemove))
                    this.RemoveEffect(toRemove);
            }
        }

        public bool IsAffectedBy(Spell spell)
        {
            return this.MatchesMask(spell.AllAffectingMasks);
        }

        public bool MatchesMask(uint[] masks)
        {
            for (int index = 0; index < this.SpellClassMask.Length; ++index)
            {
                if (((int) masks[index] & (int) this.SpellClassMask[index]) != 0)
                    return true;
            }

            return false;
        }

        public int GetMaxLevelDiff(int casterLevel)
        {
            if (this.MaxLevel >= this.BaseLevel && this.MaxLevel < casterLevel)
                return this.MaxLevel - this.BaseLevel;
            return Math.Abs(casterLevel - this.BaseLevel);
        }

        public int CalcBasePowerCost(Unit caster)
        {
            int num = this.PowerCost + this.PowerCostPerlevel * this.GetMaxLevelDiff(caster.Level);
            if (this.PowerCostPercentage > 0)
                num += this.PowerCostPercentage *
                       (this.PowerType == PowerType.Health ? caster.BaseHealth : caster.BasePower) / 100;
            return num;
        }

        public int CalcPowerCost(Unit caster, DamageSchool school)
        {
            return caster.GetPowerCost(school, this, this.CalcBasePowerCost(caster));
        }

        public bool IsVisibleToClient
        {
            get
            {
                if (!this.IsRangedAbility && this.Visual == 0U && (this.Visual2 == 0U && !this.IsChanneled) &&
                    this.CastDelay <= 0U)
                    return this.HasCooldown;
                return true;
            }
        }

        public void SetDuration(int duration)
        {
            this.Durations.Min = this.Durations.Max = duration;
        }

        /// <summary>
        /// Returns the max duration for this Spell in milliseconds,
        /// including all modifiers.
        /// </summary>
        public int GetDuration(ObjectReference caster)
        {
            return this.GetDuration(caster, (Unit) null);
        }

        /// <summary>
        /// Returns the max duration for this Spell in milliseconds,
        /// including all modifiers.
        /// </summary>
        public int GetDuration(ObjectReference caster, Unit target)
        {
            int num = this.Durations.Min;
            if (this.Durations.Max > this.Durations.Min && this.IsFinishingMove && caster.UnitMaster != null)
                num += caster.UnitMaster.ComboPoints * ((this.Durations.Max - this.Durations.Min) / 5);
            if (target != null && this.Mechanic != SpellMechanic.None)
            {
                int mechanicDurationMod = target.GetMechanicDurationMod(this.Mechanic);
                if (mechanicDurationMod != 0)
                    num = UnitUpdates.GetMultiMod((float) mechanicDurationMod / 100f, num);
            }

            Unit unitMaster = caster.UnitMaster;
            if (unitMaster != null)
                num = unitMaster.Auras.GetModifiedInt(SpellModifierType.Duration, this, num);
            return num;
        }

        public bool IsAffectedByInvulnerability
        {
            get { return !this.Attributes.HasFlag((Enum) SpellAttributes.UnaffectedByInvulnerability); }
        }

        public bool CanFailDueToImmuneAgainstTarget(Unit spellTarget)
        {
            Character character = spellTarget as Character;
            if (this.IsAffectedByInvulnerability)
                return true;
            if (character != null)
                return character.Role.IsStaff;
            return false;
        }

        /// <summary>Fully qualified name</summary>
        public string FullName
        {
            get
            {
                bool flag1 = this.Talent != null;
                bool flag2 = this.Ability != null;
                string str = !flag1 ? this.Name : this.Talent.FullName;
                if (flag2 && !flag1 && (this.Ability.Skill.Category != SkillCategory.Language &&
                                        this.Ability.Skill.Category != SkillCategory.Invalid))
                    str = ((int) this.Ability.Skill.Category).ToString() + " " + str;
                if (this.IsTeachSpell && !this.Name.StartsWith("Learn", StringComparison.InvariantCultureIgnoreCase))
                    str = "Learn " + str;
                else if (this.IsTriggeredSpell)
                    str = "Effect: " + str;
                if (!flag2)
                {
                    if (this.IsDeprecated)
                        str = "Unused " + str;
                    else if (this.Description != null)
                    {
                        int length = this.Description.Length;
                    }
                }

                return str;
            }
        }

        /// <summary>Spells that contain "zzOld", "test", "unused"</summary>
        public bool IsDeprecated
        {
            get { return Spell.IsDeprecatedSpellName(this.Name); }
        }

        public static bool IsDeprecatedSpellName(string name)
        {
            return true;
        }

        public override string ToString()
        {
            return this.FullName + (this.RankDesc != "" ? (object) (" " + this.RankDesc) : (object) "") + " (Id: " +
                   (object) this.Id + ")";
        }

        public void Dump(TextWriter writer, string indent)
        {
            writer.WriteLine("Spell: " + (object) this + " [" + (object) this.SpellId + "]");
            if (this.Category != 0U)
                writer.WriteLine(indent + "Category: " + (object) this.Category);
            if (this.Line != null)
                writer.WriteLine(indent + "Line: " + (object) this.Line);
            if (this.PreviousRank != null)
                writer.WriteLine(indent + "Previous Rank: " + (object) this.PreviousRank);
            if (this.NextRank != null)
                writer.WriteLine(indent + "Next Rank: " + (object) this.NextRank);
            if (this.DispelType != DispelType.None)
                writer.WriteLine(indent + "DispelType: " + (object) this.DispelType);
            if (this.Mechanic != SpellMechanic.None)
                writer.WriteLine(indent + "Mechanic: " + (object) this.Mechanic);
            if (this.Attributes != SpellAttributes.None)
                writer.WriteLine(indent + "Attributes: " + (object) this.Attributes);
            if (this.AttributesEx != SpellAttributesEx.None)
                writer.WriteLine(indent + "AttributesEx: " + (object) this.AttributesEx);
            if (this.AttributesExB != SpellAttributesExB.None)
                writer.WriteLine(indent + "AttributesExB: " + (object) this.AttributesExB);
            if (this.AttributesExC != SpellAttributesExC.None)
                writer.WriteLine(indent + "AttributesExC: " + (object) this.AttributesExC);
            if (this.AttributesExD != SpellAttributesExD.None)
                writer.WriteLine(indent + "AttributesExD: " + (object) this.AttributesExD);
            if (this.RequiredShapeshiftMask != ShapeshiftMask.None)
                writer.WriteLine(indent + "ShapeshiftMask: " + (object) this.RequiredShapeshiftMask);
            if (this.ExcludeShapeshiftMask != ShapeshiftMask.None)
                writer.WriteLine(indent + "ExcludeShapeshiftMask: " + (object) this.ExcludeShapeshiftMask);
            if (this.TargetFlags != SpellTargetFlags.Self)
                writer.WriteLine(indent + "TargetType: " + (object) this.TargetFlags);
            if (this.CreatureMask != CreatureMask.None)
                writer.WriteLine(indent + "TargetUnitTypes: " + (object) this.CreatureMask);
            if (this.RequiredSpellFocus != SpellFocus.None)
                writer.WriteLine(indent + "RequiredSpellFocus: " + (object) this.RequiredSpellFocus);
            if (this.FacingFlags != (SpellFacingFlags) 0)
                writer.WriteLine(indent + "FacingFlags: " + (object) this.FacingFlags);
            if (this.RequiredCasterAuraState != AuraState.None)
                writer.WriteLine(indent + "RequiredCasterAuraState: " + (object) this.RequiredCasterAuraState);
            if (this.RequiredTargetAuraState != AuraState.None)
                writer.WriteLine(indent + "RequiredTargetAuraState: " + (object) this.RequiredTargetAuraState);
            if (this.ExcludeCasterAuraState != AuraState.None)
                writer.WriteLine(indent + "ExcludeCasterAuraState: " + (object) this.ExcludeCasterAuraState);
            if (this.ExcludeTargetAuraState != AuraState.None)
                writer.WriteLine(indent + "ExcludeTargetAuraState: " + (object) this.ExcludeTargetAuraState);
            if (this.RequiredCasterAuraId != SpellId.None)
                writer.WriteLine(indent + "RequiredCasterAuraId: " + (object) this.RequiredCasterAuraId);
            if (this.RequiredTargetAuraId != SpellId.None)
                writer.WriteLine(indent + "RequiredTargetAuraId: " + (object) this.RequiredTargetAuraId);
            if (this.ExcludeCasterAuraId != SpellId.None)
                writer.WriteLine(indent + "ExcludeCasterAuraSpellId: " + (object) this.ExcludeCasterAuraId);
            if (this.ExcludeTargetAuraId != SpellId.None)
                writer.WriteLine(indent + "ExcludeTargetAuraSpellId: " + (object) this.ExcludeTargetAuraId);
            if (this.CastDelay != 0U)
                writer.WriteLine(indent + "StartTime: " + (object) this.CastDelay);
            if (this.CooldownTime > 0)
                writer.WriteLine(indent + "CooldownTime: " + (object) this.CooldownTime);
            if (this.categoryCooldownTime > 0)
                writer.WriteLine(indent + "CategoryCooldownTime: " + (object) this.categoryCooldownTime);
            if (this.InterruptFlags != InterruptFlags.None)
                writer.WriteLine(indent + "InterruptFlags: " + (object) this.InterruptFlags);
            if (this.AuraInterruptFlags != AuraInterruptFlags.None)
                writer.WriteLine(indent + "AuraInterruptFlags: " + (object) this.AuraInterruptFlags);
            if (this.ChannelInterruptFlags != ChannelInterruptFlags.None)
                writer.WriteLine(indent + "ChannelInterruptFlags: " + (object) this.ChannelInterruptFlags);
            if (this.ProcTriggerFlagsProp != ProcTriggerFlags.None)
            {
                writer.WriteLine(indent + "ProcTriggerFlags: " + (object) this.ProcTriggerFlagsProp);
                if (this.ProcHitFlags != ProcHitFlags.None)
                    writer.WriteLine(indent + "ProcHitFlags: " + (object) this.ProcHitFlags);
            }

            if (this.ProcChance != 0U)
                writer.WriteLine(indent + "ProcChance: " + (object) this.ProcChance);
            if (this.ProcCharges != 0)
                writer.WriteLine(indent + "ProcCharges: " + (object) this.ProcCharges);
            if (this.MaxLevel != 0)
                writer.WriteLine(indent + "MaxLevel: " + (object) this.MaxLevel);
            if (this.BaseLevel != 0)
                writer.WriteLine(indent + "BaseLevel: " + (object) this.BaseLevel);
            if (this.Level != 0)
                writer.WriteLine(indent + "Level: " + (object) this.Level);
            if (this.Durations.Max > 0)
                writer.WriteLine(indent + "Duration: " + (object) this.Durations.Min + " - " +
                                 (object) this.Durations.Max + " (" + (object) this.Durations.LevelDelta + ")");
            if (this.Visual != 0U)
                writer.WriteLine(indent + "Visual: " + (object) this.Visual);
            if (this.PowerType != PowerType.Mana)
                writer.WriteLine(indent + "PowerType: " + (object) this.PowerType);
            if (this.PowerCost != 0)
                writer.WriteLine(indent + "PowerCost: " + (object) this.PowerCost);
            if (this.PowerCostPerlevel != 0)
                writer.WriteLine(indent + "PowerCostPerlevel: " + (object) this.PowerCostPerlevel);
            if (this.PowerPerSecond != 0)
                writer.WriteLine(indent + "PowerPerSecond: " + (object) this.PowerPerSecond);
            if (this.PowerPerSecondPerLevel != 0)
                writer.WriteLine(indent + "PowerPerSecondPerLevel: " + (object) this.PowerPerSecondPerLevel);
            if (this.PowerCostPercentage != 0)
                writer.WriteLine(indent + "PowerCostPercentage: " + (object) this.PowerCostPercentage);
            if ((double) this.Range.MinDist != 0.0 || (double) this.Range.MaxDist != (double) Spell.DefaultSpellRange)
                writer.WriteLine(indent + "Range: " + (object) this.Range.MinDist + " - " +
                                 (object) this.Range.MaxDist);
            if ((int) this.ProjectileSpeed != 0)
                writer.WriteLine(indent + "ProjectileSpeed: " + (object) this.ProjectileSpeed);
            if (this.ModalNextSpell != SpellId.None)
                writer.WriteLine(indent + "ModalNextSpell: " + (object) this.ModalNextSpell);
            if (this.MaxStackCount != 0)
                writer.WriteLine(indent + "MaxStackCount: " + (object) this.MaxStackCount);
            if (this.RequiredTools != null)
            {
                writer.WriteLine(indent + "RequiredTools:");
                foreach (ItemTemplate requiredTool in this.RequiredTools)
                    writer.WriteLine(indent + "\t" + (object) requiredTool);
            }

            if (this.RequiredItemClass != ItemClass.None)
                writer.WriteLine(indent + "RequiredItemClass: " + (object) this.RequiredItemClass);
            if (this.RequiredItemInventorySlotMask != InventorySlotTypeMask.None)
                writer.WriteLine(indent + "RequiredItemInventorySlotMask: " +
                                 (object) this.RequiredItemInventorySlotMask);
            if (this.RequiredItemSubClassMask != ~ItemSubClassMask.None &&
                this.RequiredItemSubClassMask != ItemSubClassMask.None)
                writer.WriteLine(indent + "RequiredItemSubClassMask: " + (object) this.RequiredItemSubClassMask);
            if (this.Visual2 != 0U)
                writer.WriteLine(indent + "Visual2: " + (object) this.Visual2);
            if (this.Priority != 0U)
                writer.WriteLine(indent + "Priority: " + (object) this.Priority);
            if (this.StartRecoveryCategory != 0)
                writer.WriteLine(indent + "StartRecoveryCategory: " + (object) this.StartRecoveryCategory);
            if (this.StartRecoveryTime != 0)
                writer.WriteLine(indent + "StartRecoveryTime: " + (object) this.StartRecoveryTime);
            if (this.MaxTargetLevel != 0U)
                writer.WriteLine(indent + "MaxTargetLevel: " + (object) this.MaxTargetLevel);
            if (this.SpellClassSet != SpellClassSet.Generic)
                writer.WriteLine(indent + "SpellClassSet: " + (object) this.SpellClassSet);
            if (this.SpellClassMask[0] != 0U || this.SpellClassMask[1] != 0U || this.SpellClassMask[2] != 0U)
                writer.WriteLine(indent + "SpellClassMask: {0}{1}{2}", (object) this.SpellClassMask[0].ToString("X8"),
                    (object) this.SpellClassMask[1].ToString("X8"), (object) this.SpellClassMask[2].ToString("X8"));
            if (this.MaxTargets != 0U)
                writer.WriteLine(indent + "MaxTargets: " + (object) this.MaxTargets);
            if (this.StanceBarOrder != 0)
                writer.WriteLine(indent + "StanceBarOrder: " + (object) this.StanceBarOrder);
            if (this.DamageType != DamageType.None)
                writer.WriteLine(indent + "DamageType: " + (object) this.DamageType);
            if (this.HarmType != HarmType.Neutral)
                writer.WriteLine(indent + "HarmType: " + (object) this.HarmType);
            if (this.PreventionType != SpellPreventionType.None)
                writer.WriteLine(indent + "PreventionType: " + (object) this.PreventionType);
            if (((IEnumerable<float>) this.DamageMultipliers).Any<float>(
                (Func<float, bool>) (mult => (double) mult != 1.0)))
                writer.WriteLine(indent + "DamageMultipliers: " +
                                 ((IEnumerable<float>) this.DamageMultipliers).ToString<float>(", "));
            for (int index = 0; index < this.RequiredToolCategories.Length; ++index)
            {
                if (this.RequiredToolCategories[index] != ToolCategory.None)
                    writer.WriteLine(indent + "RequiredTotemCategoryId[" + (object) index + "]: " +
                                     (object) this.RequiredToolCategories[index]);
            }

            if (this.AreaGroupId != 0U)
                writer.WriteLine(indent + "AreaGroupId: " + (object) this.AreaGroupId);
            if (this.SchoolMask != DamageSchoolMask.None)
                writer.WriteLine(indent + "SchoolMask: " + (object) this.SchoolMask);
            if (this.RuneCostEntry != null)
            {
                writer.WriteLine(indent + "RuneCostId: " + (object) this.RuneCostEntry.Id);
                string str = indent + "\t";
                List<string> collection = new List<string>(3);
                if (this.RuneCostEntry.CostPerType[0] != 0)
                    collection.Add(string.Format("Blood: {0}", (object) this.RuneCostEntry.CostPerType[0]));
                if (this.RuneCostEntry.CostPerType[1] != 0)
                    collection.Add(string.Format("Unholy: {0}", (object) this.RuneCostEntry.CostPerType[1]));
                if (this.RuneCostEntry.CostPerType[2] != 0)
                    collection.Add(string.Format("Frost: {0}", (object) this.RuneCostEntry.CostPerType[2]));
                writer.WriteLine(str + "Runes - {0}",
                    collection.Count == 0 ? (object) "<None>" : (object) collection.ToString<string>(", "));
                writer.WriteLine(str + "RunicPowerGain: {0}", (object) this.RuneCostEntry.RunicPowerGain);
            }

            if (this.MissileId != 0U)
                writer.WriteLine(indent + "MissileId: " + (object) this.MissileId);
            if (!string.IsNullOrEmpty(this.Description))
                writer.WriteLine(indent + "Desc: " + this.Description);
            if (this.Reagents != null && this.Reagents.Length > 0)
                writer.WriteLine(indent + "Reagents: " +
                                 ((IEnumerable<ItemStackDescription>) this.Reagents).ToString<ItemStackDescription>(
                                     ", "));
            if (this.Ability != null)
                writer.WriteLine(indent + string.Format("Skill: {0}", (object) this.Ability.SkillInfo));
            if (this.Talent != null)
                writer.WriteLine(indent + string.Format("TalentTree: {0}", (object) this.Talent.Tree));
            writer.WriteLine();
            foreach (SpellEffect effect in this.Effects)
                effect.DumpInfo(writer, "\t\t");
        }

        public bool IsBeneficialFor(ObjectReference casterReference, WorldObject target)
        {
            if (this.IsBeneficial)
                return true;
            if (!this.IsNeutral)
                return false;
            if (casterReference.Object != null)
                return !casterReference.Object.MayAttack((IFactionMember) target);
            return true;
        }

        public bool IsHarmfulFor(ObjectReference casterReference, WorldObject target)
        {
            if (this.IsHarmful)
                return true;
            if (this.IsNeutral && casterReference.Object != null)
                return casterReference.Object.MayAttack((IFactionMember) target);
            return false;
        }

        public bool IsBeneficial
        {
            get { return this.HarmType == HarmType.Beneficial; }
        }

        public bool IsHarmful
        {
            get { return this.HarmType == HarmType.Harmful; }
        }

        public bool IsNeutral
        {
            get { return this.HarmType == HarmType.Neutral; }
        }

        public override bool Equals(object obj)
        {
            if (obj is Spell)
                return (int) ((Spell) obj).Id == (int) this.Id;
            return false;
        }

        public override int GetHashCode()
        {
            return (int) this.Id;
        }

        public IEnumerator<Spell> GetEnumerator()
        {
            return (IEnumerator<Spell>) new SingleEnumerator<Spell>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }

        protected Spell Clone()
        {
            return (Spell) this.MemberwiseClone();
        }

        internal static void InitDbcs()
        {
        }

        public void PrintEffects(TextWriter writer)
        {
            foreach (SpellEffect effect in this.Effects)
                effect.DumpInfo(writer, "");
        }

        public int CategoryCooldownTime
        {
            get { return this.categoryCooldownTime; }
        }

        /// <summary>
        /// Indicates events which cause this spell to trigger its proc effect
        /// </summary>
        /// <remarks>
        /// This spell must be a proc <see cref="F:WCell.RealmServer.Spells.Spell.IsProc" />
        /// </remarks>
        public ProcTriggerFlags ProcTriggerFlagsProp
        {
            get { return this.ProcTriggerFlags; }
            set
            {
                this.ProcTriggerFlags = value;
                if (!this.ProcTriggerFlags.RequireHitFlags() || this.ProcHitFlags != ProcHitFlags.None)
                    return;
                this.ProcHitFlags = ProcHitFlags.Hit;
            }
        }

        /// <summary>
        /// Contains information needed for ProcTriggerFlags depending on hit result
        /// </summary>
        /// <remarks>
        /// This spell must be a proc <see cref="F:WCell.RealmServer.Spells.Spell.IsProc" />
        /// </remarks>
        public ProcHitFlags ProcHitFlags { get; set; }

        public string RankDesc
        {
            get { return this.m_RankDesc; }
            set
            {
                this.m_RankDesc = value;
                if (value.Length <= 0)
                    return;
                Match match = Spell.numberRegex.Match(value);
                if (!match.Success)
                    return;
                int.TryParse(match.Value, out this.Rank);
            }
        }

        public SpellClassSet SpellClassSet
        {
            get { return this.spellClassSet; }
            set
            {
                this.spellClassSet = value;
                this.ClassId = value.ToClassId();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spell">The special Spell being casted</param>
        /// <param name="caster">The caster casting the spell</param>
        /// <param name="target">The target that the Caster selected (or null)</param>
        /// <param name="targetPos">The targetPos that was selected (or 0,0,0)</param>
        public delegate void SpecialCastHandler(Spell spell, WorldObject caster, WorldObject target,
            ref Vector3 targetPos);

        [Serializable]
        public struct DurationEntry
        {
            public int Min;

            /// <summary>The amount the duration increases per caster-level</summary>
            public int LevelDelta;

            public int Max;

            public int Random()
            {
                return Utility.Random(this.Min, this.Max);
            }
        }

        public class DBCDurationConverter : AdvancedDBCRecordConverter<Spell.DurationEntry>
        {
            public override Spell.DurationEntry ConvertTo(byte[] rawData, ref int id)
            {
                Spell.DurationEntry durationEntry = new Spell.DurationEntry();
                id = (int) DBCRecordConverter.GetUInt32(rawData, 0);
                durationEntry.Min = DBCRecordConverter.GetInt32(rawData, 1);
                durationEntry.LevelDelta = DBCRecordConverter.GetInt32(rawData, 2);
                durationEntry.Max = DBCRecordConverter.GetInt32(rawData, 3);
                return durationEntry;
            }
        }

        public class DBCRadiusConverter : AdvancedDBCRecordConverter<float>
        {
            public override float ConvertTo(byte[] rawData, ref int id)
            {
                id = (int) DBCRecordConverter.GetUInt32(rawData, 0);
                return DBCRecordConverter.GetFloat(rawData, 1);
            }
        }

        public class DBCCastTimeConverter : AdvancedDBCRecordConverter<uint>
        {
            public override uint ConvertTo(byte[] rawData, ref int id)
            {
                id = (int) DBCRecordConverter.GetUInt32(rawData, 0);
                return DBCRecordConverter.GetUInt32(rawData, 1);
            }
        }

        public class DBCRangeConverter : AdvancedDBCRecordConverter<SimpleRange>
        {
            public override SimpleRange ConvertTo(byte[] rawData, ref int id)
            {
                SimpleRange simpleRange = new SimpleRange();
                id = DBCRecordConverter.GetInt32(rawData, 0);
                simpleRange.MinDist = (float) (uint) DBCRecordConverter.GetFloat(rawData, 1);
                simpleRange.MaxDist = (float) (uint) DBCRecordConverter.GetFloat(rawData, 3);
                return simpleRange;
            }
        }

        public class DBCMechanicConverter : AdvancedDBCRecordConverter<string>
        {
            public override string ConvertTo(byte[] rawData, ref int id)
            {
                id = DBCRecordConverter.GetInt32(rawData, 0);
                return this.GetString(rawData, 1);
            }
        }

        public struct SpellFocusEntry
        {
            public uint Id;
            public string Name;
        }

        public class DBCSpellFocusConverter : AdvancedDBCRecordConverter<Spell.SpellFocusEntry>
        {
            public override Spell.SpellFocusEntry ConvertTo(byte[] rawData, ref int id)
            {
                return new Spell.SpellFocusEntry()
                {
                    Id = (uint) (id = DBCRecordConverter.GetInt32(rawData, 0)),
                    Name = this.GetString(rawData, 1)
                };
            }
        }

        public class DBCSpellRuneCostConverter : AdvancedDBCRecordConverter<RuneCostEntry>
        {
            public override RuneCostEntry ConvertTo(byte[] rawData, ref int id)
            {
                RuneCostEntry runeCostEntry = new RuneCostEntry()
                {
                    Id = (uint) (id = DBCRecordConverter.GetInt32(rawData, 0)),
                    RunicPowerGain = DBCRecordConverter.GetInt32(rawData, 4)
                };
                for (int index = 0; index < 3; ++index)
                    runeCostEntry.RequiredRuneAmount += runeCostEntry.CostPerType[index] =
                        DBCRecordConverter.GetInt32(rawData, index + 1);
                return runeCostEntry;
            }
        }

        public class SpellDBCConverter : DBCRecordConverter
        {
            public override void Convert(byte[] rawData)
            {
                int num = 0;
                Spell spell1 = new Spell();
                Spell spell2 = spell1;
                byte[] data = rawData;
                int field = num;
                int currentIndex = field + 1;
                int uint32 = (int) DBCRecordConverter.GetUInt32(data, field);
                spell2.Id = (uint) uint32;
                spell1.SpellId = (SpellId) DBCRecordConverter.GetInt32(rawData, 0);
                Spell spell3 = spell1;
                try
                {
                    spell3.Category = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.DispelType = (DispelType) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.Mechanic = (SpellMechanic) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.Attributes = (SpellAttributes) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.AttributesEx = (SpellAttributesEx) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.AttributesExB = (SpellAttributesExB) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.AttributesExC = (SpellAttributesExC) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.AttributesExD = (SpellAttributesExD) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.AttributesExE = (SpellAttributesExE) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.AttributesExF = (SpellAttributesExF) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.RequiredShapeshiftMask =
                        (ShapeshiftMask) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.Unk_322_1 = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.ExcludeShapeshiftMask =
                        (ShapeshiftMask) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.Unk_322_2 = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.TargetFlags = (SpellTargetFlags) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.Unk_322_3 = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.CreatureMask = (CreatureMask) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.RequiredSpellFocus = (SpellFocus) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.FacingFlags = (SpellFacingFlags) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.RequiredCasterAuraState = (AuraState) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.RequiredTargetAuraState = (AuraState) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.ExcludeCasterAuraState = (AuraState) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.ExcludeTargetAuraState = (AuraState) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.RequiredCasterAuraId = (SpellId) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.RequiredTargetAuraId = (SpellId) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.ExcludeCasterAuraId = (SpellId) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.ExcludeTargetAuraId = (SpellId) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    int int32_1 = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    if (int32_1 > 0 &&
                        !Spell.mappeddbcCastTimeReader.Entries.TryGetValue(int32_1, out spell3.CastDelay))
                        ContentMgr.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid CastTime-Entry: {1}",
                            (object) spell3.Name, (object) int32_1);
                    spell3.CooldownTime = Math.Max(0,
                        DBCRecordConverter.GetInt32(rawData, currentIndex++) - (int) spell3.CastDelay);
                    spell3.categoryCooldownTime = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.InterruptFlags = (InterruptFlags) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.AuraInterruptFlags =
                        (AuraInterruptFlags) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.ChannelInterruptFlags =
                        (ChannelInterruptFlags) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.ProcTriggerFlagsProp =
                        (ProcTriggerFlags) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.ProcChance = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.ProcCharges = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.MaxLevel = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.BaseLevel = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.Level = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    int int32_2 = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    if (int32_2 > 0 &&
                        !Spell.mappeddbcDurationReader.Entries.TryGetValue(int32_2, out spell3.Durations))
                        ContentMgr.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid Duration-Entry: {1}",
                            (object) spell3.Name, (object) int32_2);
                    spell3.PowerType = (PowerType) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.PowerCost = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.PowerCostPerlevel = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.PowerPerSecond = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.PowerPerSecondPerLevel = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    int int32_3 = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    if (int32_3 > 0 && !Spell.mappeddbcRangeReader.Entries.TryGetValue(int32_3, out spell3.Range))
                        ContentMgr.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid Range-Entry: {1}",
                            (object) spell3.Name, (object) int32_3);
                    spell3.ProjectileSpeed = DBCRecordConverter.GetFloat(rawData, currentIndex++);
                    spell3.ModalNextSpell = (SpellId) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.MaxStackCount = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.RequiredToolIds = new uint[2];
                    for (int index = 0; index < spell3.RequiredToolIds.Length; ++index)
                        spell3.RequiredToolIds[index] = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    List<ItemStackDescription> list = (List<ItemStackDescription>) null;
                    int reagentStart = currentIndex;
                    for (int reagentNum = 0; reagentNum < 8; ++reagentNum)
                        this.ReadReagent(rawData, reagentStart, reagentNum, out currentIndex, ref list);
                    spell3.Reagents = list == null ? ItemStackDescription.EmptyArray : list.ToArray();
                    spell3.RequiredItemClass = (ItemClass) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    if (spell3.RequiredItemClass < ItemClass.Consumable)
                        spell3.RequiredItemClass = ItemClass.None;
                    spell3.RequiredItemSubClassMask =
                        (ItemSubClassMask) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    if (spell3.RequiredItemSubClassMask < ItemSubClassMask.None)
                        spell3.RequiredItemSubClassMask = ItemSubClassMask.None;
                    spell3.RequiredItemInventorySlotMask =
                        (InventorySlotTypeMask) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    if (spell3.RequiredItemInventorySlotMask < InventorySlotTypeMask.None)
                        spell3.RequiredItemInventorySlotMask = InventorySlotTypeMask.None;
                    List<SpellEffect> spellEffectList = new List<SpellEffect>(3);
                    int effectStartIndex = currentIndex;
                    for (int effectNum = 0; effectNum < 3; ++effectNum)
                    {
                        SpellEffect spellEffect = this.ReadEffect(spell3, rawData, effectStartIndex, effectNum,
                            out currentIndex);
                        if (spellEffect != null && (spellEffect.EffectType != SpellEffectType.None ||
                                                    spellEffect.BasePoints > 0 ||
                                                    (spellEffect.AuraType != AuraType.None ||
                                                     spellEffect.TriggerSpellId != SpellId.None)))
                            spellEffectList.Add(spellEffect);
                    }

                    spell3.Effects = spellEffectList.ToArray();
                    spell3.Visual = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.Visual2 = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.SpellbookIconId = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.BuffIconId = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.Priority = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.Name = this.GetString(rawData, ref currentIndex);
                    spell3.RankDesc = this.GetString(rawData, ref currentIndex);
                    spell3.Description = this.GetString(rawData, ref currentIndex);
                    spell3.BuffDescription = this.GetString(rawData, ref currentIndex);
                    spell3.PowerCostPercentage = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.StartRecoveryTime = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.StartRecoveryCategory = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.MaxTargetLevel = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.SpellClassSet = (SpellClassSet) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.SpellClassMask[0] = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.SpellClassMask[1] = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.SpellClassMask[2] = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.MaxTargets = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.DamageType = (DamageType) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.PreventionType = (SpellPreventionType) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.StanceBarOrder = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    for (int index = 0; index < spell3.DamageMultipliers.Length; ++index)
                        spell3.DamageMultipliers[index] = DBCRecordConverter.GetFloat(rawData, currentIndex++);
                    spell3.MinFactionId = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.MinReputation = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.RequiredAuraVision = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.RequiredToolCategories = new ToolCategory[2];
                    for (int index = 0; index < spell3.RequiredToolCategories.Length; ++index)
                        spell3.RequiredToolCategories[index] =
                            (ToolCategory) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.AreaGroupId = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.SchoolMask = (DamageSchoolMask) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    int int32_4 = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    if (int32_4 != 0)
                        Spell.mappeddbcRuneCostReader.Entries.TryGetValue(int32_4, out spell3.RuneCostEntry);
                    spell3.MissileId = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.PowerDisplayId = DBCRecordConverter.GetInt32(rawData, currentIndex++);
                    spell3.Unk_322_4_1 = (float) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.Unk_322_4_2 = (float) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.Unk_322_4_3 = (float) DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                    spell3.spellDescriptionVariablesID = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        string.Format("Unable to parse Spell from DBC file. Index: " + (object) currentIndex), ex);
                }

                SpellHandler.AddSpell(spell3);
            }

            private void ReadReagent(byte[] rawData, int reagentStart, int reagentNum, out int currentIndex,
                ref List<ItemStackDescription> list)
            {
                currentIndex = reagentStart + reagentNum;
                Asda2ItemId uint32 = (Asda2ItemId) DBCRecordConverter.GetUInt32(rawData, currentIndex);
                currentIndex += 8;
                int int32 = DBCRecordConverter.GetInt32(rawData, currentIndex);
                currentIndex += 8 - reagentNum;
                if (uint32 <= (Asda2ItemId) 0 || int32 <= 0)
                    return;
                if (list == null)
                    list = new List<ItemStackDescription>();
                ItemStackDescription stackDescription = new ItemStackDescription()
                {
                    ItemId = uint32,
                    Amount = int32
                };
                list.Add(stackDescription);
            }

            private SpellEffect ReadEffect(Spell spell, byte[] rawData, int effectStartIndex, int effectNum,
                out int currentIndex)
            {
                SpellEffect spellEffect = new SpellEffect(spell, (EffectIndex) effectNum);
                currentIndex = effectStartIndex + effectNum;
                spellEffect.EffectType = (SpellEffectType) DBCRecordConverter.GetUInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.DiceSides = DBCRecordConverter.GetInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.RealPointsPerLevel = DBCRecordConverter.GetFloat(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.BasePoints = DBCRecordConverter.GetInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.Mechanic = (SpellMechanic) DBCRecordConverter.GetUInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.ImplicitTargetA =
                    (ImplicitSpellTargetType) DBCRecordConverter.GetUInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.ImplicitTargetB =
                    (ImplicitSpellTargetType) DBCRecordConverter.GetUInt32(rawData, currentIndex);
                currentIndex += 3;
                if (spellEffect.ImplicitTargetA == ImplicitSpellTargetType.AllEnemiesAroundCaster &&
                    spellEffect.ImplicitTargetB == ImplicitSpellTargetType.AllEnemiesInArea)
                    spellEffect.ImplicitTargetB = ImplicitSpellTargetType.None;
                int int32 = DBCRecordConverter.GetInt32(rawData, currentIndex);
                if (int32 > 0)
                    Spell.mappeddbcRadiusReader.Entries.TryGetValue(int32, out spellEffect.Radius);
                currentIndex += 3;
                spellEffect.AuraType = (AuraType) DBCRecordConverter.GetUInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.Amplitude = DBCRecordConverter.GetInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.ProcValue = DBCRecordConverter.GetFloat(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.ChainTargets = DBCRecordConverter.GetInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.ItemId = DBCRecordConverter.GetUInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.MiscValue = DBCRecordConverter.GetInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.MiscValueB = DBCRecordConverter.GetInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.TriggerSpellId = (SpellId) DBCRecordConverter.GetUInt32(rawData, currentIndex);
                currentIndex += 3;
                spellEffect.PointsPerComboPoint = DBCRecordConverter.GetFloat(rawData, currentIndex);
                currentIndex += 3 - effectNum;
                currentIndex += effectNum * 3;
                spellEffect.AffectMask[0] = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                spellEffect.AffectMask[1] = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                spellEffect.AffectMask[2] = DBCRecordConverter.GetUInt32(rawData, currentIndex++);
                currentIndex += (2 - effectNum) * 3;
                return spellEffect;
            }
        }
    }
}