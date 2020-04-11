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

    [NotPersistent]public EquipmentSlot EquipmentSlot = EquipmentSlot.End;
    [NotPersistent]public uint[] AllAffectingMasks = new uint[3];
    [Persistent]public RequiredSpellTargetType RequiredTargetType = RequiredSpellTargetType.Default;

    /// <summary>List of Spells to be learnt when this Spell is learnt</summary>
    public readonly List<Spell> AdditionallyTaughtSpells = new List<Spell>(0);

    [Persistent(3)]public uint[] SpellClassMask = new uint[3];

    /// <summary>
    /// Used for effect-value damping when using chain targets, eg:
    /// 	DamageMultipliers: 0.6, 1, 1
    /// 	"Each jump reduces the effectiveness of the heal by 40%.  Heals $x1 total targets."
    /// </summary>
    [Persistent(3)]public float[] DamageMultipliers = new float[3];

    [NotPersistent]public ToolCategory[] RequiredToolCategories = new ToolCategory[2];

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

    [NotPersistent]public Spell[] TargetTriggerSpells;
    [NotPersistent]public Spell[] CasterTriggerSpells;
    [NotPersistent]public HashSet<Spell> CasterProcSpells;
    [NotPersistent]public HashSet<Spell> TargetProcSpells;
    [NotPersistent]public List<ProcHandlerTemplate> ProcHandlers;

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

    [NotPersistent]public AISpellSettings AISettings;

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

    [NotPersistent]public SpecialCastHandler SpecialCast;
    [NotPersistent]public TalentEntry Talent;
    [NotPersistent]private SkillAbility m_Ability;
    [NotPersistent]public SkillTierId SkillTier;
    [NotPersistent]public ItemTemplate[] RequiredTools;
    [NotPersistent]public Spell NextRank;
    [NotPersistent]public Spell PreviousRank;

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
    [NotPersistent]public SpellEffect TotemEffect;
    [NotPersistent]public SpellEffect[] ProcTriggerEffects;
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

    [Persistent]public Asda2ClassMask ClassMask;
    [Persistent]public int Cost;
    [Persistent]public byte LearnLevel;
    [Persistent]public byte ProffNum;
    public bool HasManaShield;
    [Persistent]public int Duration;
    public bool IsEnhancer;
    private bool init1;
    private bool init2;
    [Persistent]public byte MaxRange;
    public SpellLine Line;

    /// <summary>
    /// Whether this spell has effects that require other Auras to be active to be activated
    /// </summary>
    public bool HasAuraDependentEffects;

    [Persistent]public uint RequiredTargetId;
    [Persistent]public SpellTargetLocation TargetLocation;
    [Persistent]public float TargetOrientation;
    public byte SoulGuardProffLevel;

    [NotPersistent]public static MappedDBCReader<DurationEntry, DBCDurationConverter> mappeddbcDurationReader;

    [NotPersistent]public static MappedDBCReader<float, DBCRadiusConverter> mappeddbcRadiusReader;
    [NotPersistent]public static MappedDBCReader<uint, DBCCastTimeConverter> mappeddbcCastTimeReader;
    [NotPersistent]public static MappedDBCReader<SimpleRange, DBCRangeConverter> mappeddbcRangeReader;
    [NotPersistent]public static MappedDBCReader<string, DBCMechanicConverter> mappeddbcMechanicReader;

    [NotPersistent]public static MappedDBCReader<RuneCostEntry, DBCSpellRuneCostConverter> mappeddbcRuneCostReader;

    [Persistent]public short RealId;
    [Persistent]public uint Id;
    public SpellId SpellId;
    [Persistent]public uint Category;
    [Persistent]public DispelType DispelType;
    [Persistent]public SpellMechanic Mechanic;
    [Persistent]public SpellAttributes Attributes;
    [Persistent]public SpellAttributesEx AttributesEx;
    [Persistent]public SpellAttributesExB AttributesExB;
    [Persistent]public SpellAttributesExC AttributesExC;
    [Persistent]public SpellAttributesExD AttributesExD;
    [Persistent]public SpellAttributesExE AttributesExE;
    [Persistent]public SpellAttributesExF AttributesExF;
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

    [Persistent]public SpellTargetFlags TargetFlags;

    /// <summary>CreatureType.dbc</summary>
    public CreatureMask CreatureMask;

    /// <summary>SpellFocusObject.dbc</summary>
    public SpellFocus RequiredSpellFocus;

    public SpellFacingFlags FacingFlags;
    [Persistent]public AuraState RequiredCasterAuraState;
    [Persistent]public AuraState RequiredTargetAuraState;
    [Persistent]public AuraState ExcludeCasterAuraState;
    [Persistent]public AuraState ExcludeTargetAuraState;
    [Persistent]public SpellId RequiredCasterAuraId;
    [Persistent]public SpellId RequiredTargetAuraId;
    [Persistent]public SpellId ExcludeCasterAuraId;
    [Persistent]public SpellId ExcludeTargetAuraId;
    [Persistent]public uint CastDelay;
    [Persistent]public int CooldownTime;
    [Persistent]public int categoryCooldownTime;
    [Persistent]public InterruptFlags InterruptFlags;
    [Persistent]public AuraInterruptFlags AuraInterruptFlags;
    [Persistent]public ChannelInterruptFlags ChannelInterruptFlags;
    [Persistent]public ProcTriggerFlags ProcTriggerFlags;
    [Persistent]public uint ProcChance;
    [Persistent]public int ProcCharges;
    [Persistent]public int MaxLevel;
    [Persistent]public int BaseLevel;
    [Persistent]public int Level;

    /// <summary>SpellDuration.dbc</summary>
    public int DurationIndex;

    [NotPersistent]public DurationEntry Durations;
    [Persistent]public PowerType PowerType;
    [Persistent]public int PowerCost;
    [Persistent]public int PowerCostPerlevel;
    [Persistent]public int PowerPerSecond;

    /// <summary>Unused so far</summary>
    public int PowerPerSecondPerLevel;

    /// <summary>SpellRange.dbc</summary>
    public int RangeIndex;

    /// <summary>Read from SpellRange.dbc</summary>
    [NotPersistent]public SimpleRange Range;

    /// <summary>The speed of the projectile in yards per second</summary>
    public float ProjectileSpeed;

    /// <summary>
    /// Hunter ranged spells have this. It seems always to be 75
    /// </summary>
    public SpellId ModalNextSpell;

    [Persistent]public int MaxStackCount;
    [Persistent(2)]public uint[] RequiredToolIds;
    [Persistent(8)]public uint[] ReagentIds;
    [Persistent(8)]public uint[] ReagentCounts;
    [NotPersistent]public ItemStackDescription[] Reagents;

    /// <summary>ItemClass.dbc</summary>
    public ItemClass RequiredItemClass;

    /// <summary>
    /// Mask of ItemSubClasses, used for Enchants and Combat Abilities
    /// </summary>
    public ItemSubClassMask RequiredItemSubClassMask;

    /// <summary>Mask of InventorySlots, used for Enchants only</summary>
    public InventorySlotTypeMask RequiredItemInventorySlotMask;

    /// <summary>Does not count void effect handlers</summary>
    [NotPersistent]public int EffectHandlerCount;

    [NotPersistent]public SpellEffect[] Effects;

    /// <summary>SpellVisual.dbc</summary>
    public uint Visual;

    /// <summary>SpellVisual.dbc</summary>
    public uint Visual2;

    /// <summary>SpellIcon.dbc</summary>
    public uint SpellbookIconId;

    /// <summary>SpellIcon.dbc</summary>
    public uint BuffIconId;

    public uint Priority;
    [Persistent]public string Name;
    private string m_RankDesc;
    public int Rank;
    [Persistent]public string Description;
    public string BuffDescription;
    public int PowerCostPercentage;

    /// <summary>Always 0?</summary>
    public int StartRecoveryTime;

    public int StartRecoveryCategory;
    public uint MaxTargetLevel;
    private SpellClassSet spellClassSet;
    public ClassId ClassId;
    public uint MaxTargets;
    [Persistent]public DamageType DamageType;
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

    [Persistent]public DamageSchoolMask SchoolMask;

    /// <summary>SpellRuneCost.dbc</summary>
    public RuneCostEntry RuneCostEntry;

    /// <summary>SpellMissile.dbc</summary>
    public uint MissileId;

    /// <summary>PowerDisplay.dbc</summary>
    /// <remarks>Added in 3.1.0</remarks>
    public int PowerDisplayId;

    [NotPersistent]public DamageSchool[] Schools;
    [Persistent]public SpellEffectType Effect0_EffectType;
    [Persistent]public SpellMechanic Effect0_Mehanic;
    [Persistent]public ImplicitSpellTargetType Effect0_ImplicitTargetA;
    [Persistent]public ImplicitSpellTargetType Effect0_ImplicitTargetB;
    [Persistent]public float Effect0_Radius;
    [Persistent]public AuraType Effect0_AuraType;
    [Persistent]public int Effect0_Amplitude;
    [Persistent]public float Effect0_ProcValue;
    [Persistent]public int Effect0_MiscValue;
    [Persistent]public int Effect0_MiscValueB;
    [Persistent]public int Effect0_MiscValueC;
    [Persistent]public SpellEffectType Effect1_EffectType;
    [Persistent]public SpellMechanic Effect1_Mehanic;
    [Persistent]public ImplicitSpellTargetType Effect1_ImplicitTargetA;
    [Persistent]public ImplicitSpellTargetType Effect1_ImplicitTargetB;
    [Persistent]public float Effect1_Radius;
    [Persistent]public AuraType Effect1_AuraType;
    [Persistent]public int Effect1_Amplitude;
    [Persistent]public float Effect1_ProcValue;
    [Persistent]public int Effect1_MiscValue;
    [Persistent]public int Effect1_MiscValueB;
    [Persistent]public int Effect1_MiscValueC;

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
      Func<SpellCast, SpellFailedReason> casting = Casting;
      if(casting != null)
      {
        SpellFailedReason reason = casting(cast);
        if(reason != SpellFailedReason.Ok)
        {
          cast.Cancel(reason);
          return reason;
        }
      }

      return SpellFailedReason.Ok;
    }

    internal void NotifyCancelled(SpellCast cast, SpellFailedReason reason)
    {
      Action<SpellCast, SpellFailedReason> cancelling = Cancelling;
      if(cancelling == null)
        return;
      cancelling(cast, reason);
    }

    internal void NotifyCasted(SpellCast cast)
    {
      Action<SpellCast> casted = Casted;
      if(casted == null)
        return;
      casted(cast);
    }

    internal void NotifyAuraRemoved(Aura aura)
    {
      Action<Aura> auraRemoved = AuraRemoved;
      if(auraRemoved == null)
        return;
      auraRemoved(aura);
    }

    /// <summary>
    /// Will let the Caster play the given text and sound after casting
    /// </summary>
    public void AddTextAndSoundEvent(NPCAiText text)
    {
      if(text == null)
        return;
      Casted += (Action<SpellCast>) (cast => cast.CasterObject.PlayTextAndSound(text));
    }

    /// <summary>
    /// Whether this is a proc and whether its own effects handle procs (or false, if not a proc or custom proc handlers have been added)
    /// </summary>
    public bool IsAuraProcHandler
    {
      get
      {
        if(IsProc)
          return ProcHandlers == null;
        return false;
      }
    }

    /// <summary>Does this Spell apply a Mount-Aura?</summary>
    public bool IsMount
    {
      get { return Mechanic == SpellMechanic.Mounted; }
    }

    private void InitAura()
    {
      if(ProcTriggerFlagsProp != ProcTriggerFlags.None || CasterProcSpells != null)
      {
        ProcTriggerEffects = Effects
          .Where(effect => effect.IsProc).ToArray();
        if(ProcTriggerEffects.Length == 0)
          ProcTriggerEffects = null;
        IsProc = ProcTriggerEffects != null || ProcHandlers != null ||
                 CasterProcSpells != null || ProcCharges > 0;
      }

      IsAura = IsProc ||
               HasEffectWith(effect => effect.AuraType != AuraType.None);
      ForeachEffect(effect =>
      {
        if(!effect.IsAuraEffect)
          return;
        HasNonPeriodicAuraEffects = HasNonPeriodicAuraEffects || !effect.IsPeriodic;
        HasPeriodicAuraEffects = HasPeriodicAuraEffects || effect.IsPeriodic;
      });
      IsAutoRepeating = AttributesExB.HasFlag(SpellAttributesExB.AutoRepeat);
      HasManaShield =
        HasEffectWith(effect => effect.AuraType == AuraType.ManaShield);
      AuraEffects =
        GetEffectsWhere(effect => effect.AuraEffectHandlerCreator != null);
      AreaAuraEffects = GetEffectsWhere(effect => effect.IsAreaAuraEffect);
      IsAreaAura = AreaAuraEffects != null;
      IsPureAura = !IsDamageSpell && !HasEffectWith(effect =>
      {
        if(effect.EffectType == SpellEffectType.ApplyAura &&
           effect.EffectType == SpellEffectType.ApplyAuraToMaster &&
           effect.EffectType == SpellEffectType.ApplyStatAura)
          return effect.EffectType != SpellEffectType.ApplyStatAuraPercent;
        return true;
      });
      IsPureBuff = IsPureAura && HasBeneficialEffects && !HasHarmfulEffects;
      IsPureDebuff = IsPureAura && HasHarmfulEffects && !HasBeneficialEffects;
      IsVehicle =
        HasEffectWith(effect => effect.AuraType == AuraType.Vehicle);
      IsShapeshift = HasEffectWith(effect =>
      {
        if(effect.AuraType != AuraType.ModShapeshift)
          return effect.AuraType == AuraType.Transform;
        return true;
      });
      CanStack = MaxStackCount > 0;
      InitialStackCount = ProcCharges <= 0 ? 1 : ProcCharges;
      IsGhost = HasEffectWith(effect => effect.AuraType == AuraType.Ghost);
      HasFlyEffect =
        HasEffectWith(effect => effect.AuraType == AuraType.Fly);
      IsFlyingMount = IsMount &&
                      HasEffectWith(effect =>
                        effect.AuraType == AuraType.ModSpeedMountedFlight);
      CanApplyMultipleTimes = Attributes == (SpellAttributes.Passive | SpellAttributes.InvisibleAura) &&
                              Ability == null && Talent == null;
      HasShapeshiftDependentEffects =
        HasEffectWith(effect =>
          effect.RequiredShapeshiftMask != ShapeshiftMask.None);
      IsModalShapeshiftDependentAura = IsPassive &&
                                       (RequiredShapeshiftMask != ShapeshiftMask.None ||
                                        HasShapeshiftDependentEffects);
      if(AuraUID != 0U)
        return;
      CreateAuraUID();
    }

    private void CreateAuraUID()
    {
      int count = AuraHandler.AuraIdEvaluators.Count;
      for(uint index = 0; (long) index < (long) count; ++index)
      {
        if(AuraHandler.AuraIdEvaluators[(int) index](this))
        {
          AuraUID = 1078U + index;
          break;
        }
      }

      if(AuraUID != 0U)
        return;
      if(Line != null)
        AuraUID = Line.AuraUID;
      else
        AuraUID = AuraHandler.GetNextAuraUID();
    }

    /// <summary>
    /// Add Spells which, when casted by the owner of this Aura, can cause it to trigger this spell's procs.
    /// Don't add damage spells (they will generate a Proc event anyway).
    /// </summary>
    public void AddCasterProcSpells(params SpellId[] spellIds)
    {
      Spell[] spellArray = new Spell[spellIds.Length];
      for(int index = 0; index < spellIds.Length; ++index)
      {
        SpellId spellId = spellIds[index];
        Spell spell = SpellHandler.Get(spellId);
        if(spell == null)
          throw new InvalidSpellDataException("Invalid SpellId: " + spellId);
        spellArray[index] = spell;
      }

      AddCasterProcSpells(spellArray);
    }

    /// <summary>
    /// Add Spells which, when casted by the owner of this Aura, can cause it to trigger this spell's procs.
    /// Don't add damage spells (they will generate a Proc event anyway).
    /// </summary>
    public void AddCasterProcSpells(params SpellLineId[] spellSetIds)
    {
      List<Spell> spellList = new List<Spell>(spellSetIds.Length * 6);
      foreach(SpellLineId spellSetId in spellSetIds)
      {
        SpellLine line = spellSetId.GetLine();
        spellList.AddRange(line);
      }

      AddCasterProcSpells(spellList.ToArray());
    }

    /// <summary>
    /// Add Spells which, when casted by the owner of this Aura, can cause it to trigger this spell's procs.
    /// Don't add damage spells (they will generate a Proc event anyway).
    /// </summary>
    public void AddCasterProcSpells(params Spell[] spells)
    {
      if(CasterProcSpells == null)
        CasterProcSpells = new HashSet<Spell>();
      foreach(Spell spell in spells)
        spell.GeneratesProcEventOnCast = true;
      CasterProcSpells.AddRange(spells);
    }

    /// <summary>
    /// Add Spells which, when casted by others on the owner of this Aura, can cause it to trigger it's procs.
    /// Don't add damage spells (they will generate a Proc event anyway).
    /// </summary>
    public void AddTargetProcSpells(params SpellId[] spellIds)
    {
      Spell[] spellArray = new Spell[spellIds.Length];
      for(int index = 0; index < spellIds.Length; ++index)
      {
        SpellId spellId = spellIds[index];
        Spell spell = SpellHandler.Get(spellId);
        if(spell == null)
          throw new InvalidSpellDataException("Invalid SpellId: " + spellId);
        spellArray[index] = spell;
      }

      AddTargetProcSpells(spellArray);
    }

    /// <summary>
    /// Add Spells which, when casted by others on the owner of this Aura, can cause it to trigger it's procs
    /// Don't add damage spells (they will generate a Proc event anyway).
    /// </summary>
    public void AddTargetProcSpells(params SpellLineId[] spellSetIds)
    {
      List<Spell> spellList = new List<Spell>(spellSetIds.Length * 6);
      foreach(SpellLineId spellSetId in spellSetIds)
      {
        SpellLine line = spellSetId.GetLine();
        spellList.AddRange(line);
      }

      AddTargetProcSpells(spellList.ToArray());
    }

    /// <summary>
    /// Add Spells which, when casted by others on the owner of this Aura, can cause it to trigger it's procs
    /// Don't add damage spells (they will generate a Proc event anyway).
    /// </summary>
    public void AddTargetProcSpells(params Spell[] spells)
    {
      if(TargetProcSpells == null)
        TargetProcSpells = new HashSet<Spell>();
      foreach(Spell spell in spells)
        spell.GeneratesProcEventOnCast = true;
      TargetProcSpells.AddRange(spells);
    }

    public List<AuraEffectHandler> CreateAuraEffectHandlers(ObjectReference caster, Unit target, bool beneficial)
    {
      return CreateAuraEffectHandlers(AuraEffects, caster, target, beneficial);
    }

    public static List<AuraEffectHandler> CreateAuraEffectHandlers(SpellEffect[] effects, ObjectReference caster,
      Unit target, bool beneficial)
    {
      if(effects == null)
        return null;
      try
      {
        List<AuraEffectHandler> auraEffectHandlerList = null;
        SpellFailedReason failedReason = SpellFailedReason.Ok;
        for(int index = 0; index < effects.Length; ++index)
        {
          SpellEffect effect = effects[index];
          if(effect.HarmType == HarmType.Beneficial || !beneficial)
          {
            AuraEffectHandler auraEffectHandler =
              effect.CreateAuraEffectHandler(caster, target, ref failedReason);
            if(failedReason != SpellFailedReason.Ok)
              return null;
            if(auraEffectHandlerList == null)
              auraEffectHandlerList = new List<AuraEffectHandler>(3);
            auraEffectHandlerList.Add(auraEffectHandler);
          }
        }

        return auraEffectHandlerList;
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex,
          "Failed to create AuraEffectHandlers for: " + effects
            .GetWhere(effect => effect != null).Spell);
        return null;
      }
    }

    public bool CanOverride(Spell spell)
    {
      if(CanOverrideEqualAuraRank)
        return Rank >= spell.Rank;
      return Rank > spell.Rank;
    }

    public AuraIndexId GetAuraUID(ObjectReference caster, WorldObject target)
    {
      return GetAuraUID(IsBeneficialFor(caster, target));
    }

    public AuraIndexId GetAuraUID(bool positive)
    {
      return new AuraIndexId
      {
        AuraUID = !CanApplyMultipleTimes
          ? AuraUID
          : AuraHandler.lastAuraUid + ++AuraHandler.randomAuraId,
        IsPositive = positive
      };
    }

    /// <summary>Tame Beast (Id: 1515) amongst others</summary>
    public bool IsTame
    {
      get { return AttributesExB.HasFlag(SpellAttributesExB.TamePet); }
    }

    /// <summary>whether Spell's effects don't wear off when dead</summary>
    public bool PersistsThroughDeath
    {
      get { return AttributesExC.HasFlag(SpellAttributesExC.PersistsThroughDeath); }
    }

    /// <summary>whether its a food effect</summary>
    public bool IsFood
    {
      get { return Category == 11U; }
    }

    /// <summary>whether its a drink effect</summary>
    public bool IsDrink
    {
      get { return Category == 59U; }
    }

    public bool IsTalent
    {
      get { return Talent != null; }
    }

    [NotPersistent]
    public SkillAbility Ability
    {
      get { return m_Ability; }
      internal set
      {
        m_Ability = value;
        if(value == null || ClassId != ClassId.NoClass)
          return;
        ClassId[] ids = Ability.ClassMask.GetIds();
        if(ids.Length != 1)
          return;
        ClassId = ids[0];
      }
    }

    [NotPersistent]
    public bool RepresentsSkillTier
    {
      get { return SkillTier != SkillTierId.End; }
    }

    public bool MatchesRequiredTargetType(WorldObject obj)
    {
      if(RequiredTargetType == RequiredSpellTargetType.GameObject)
        return obj is GameObject;
      if(obj is NPC)
        return ((Unit) obj).IsAlive == (RequiredTargetType == RequiredSpellTargetType.NPCAlive);
      return false;
    }

    public void FinalizeDataHolder()
    {
      try
      {
        SpellId = (SpellId) Id;
        PowerType = PowerType.Mana;
        Durations = new DurationEntry
        {
          Min = Duration,
          Max = Duration
        };
        Range = new SimpleRange(0.0f, MaxRange);
        ProjectileSpeed = 1f;
        RequiredToolIds = new uint[2];
        Reagents = ItemStackDescription.EmptyArray;
        RequiredItemClass = ItemClass.None;
        RequiredItemSubClassMask = ItemSubClassMask.None;
        if(Id == 2228U || Id == 2231U || (Id == 2234U || Id == 2237U) ||
           (Id == 2240U || Id == 2243U || (Id == 2246U || Id == 2249U)) ||
           Id == 2252U)
          SoulGuardProffLevel = 1;
        if(Id == 2229U || Id == 2232U || (Id == 2235U || Id == 2238U) ||
           (Id == 2241U || Id == 2244U || (Id == 2247U || Id == 2250U)) ||
           Id == 2253U)
          SoulGuardProffLevel = 2;
        if(Id == 2230U || Id == 2233U || (Id == 2236U || Id == 2239U) ||
           (Id == 2242U || Id == 2245U || (Id == 2248U || Id == 2251U)) ||
           Id == 2254U)
          SoulGuardProffLevel = 3;
        RequiredItemInventorySlotMask = InventorySlotTypeMask.None;
        List<SpellEffect> spellEffectList = new List<SpellEffect>(3);
        SpellEffect spellEffect1 = new SpellEffect(this, EffectIndex.Zero)
        {
          EffectType = Effect0_EffectType,
          DiceSides = 0,
          RealPointsPerLevel = 0.0f,
          BasePoints = 0,
          Mechanic = Effect0_Mehanic,
          ImplicitTargetA = Effect0_ImplicitTargetA,
          ImplicitTargetB = Effect0_ImplicitTargetB,
          Radius = Effect0_Radius,
          AuraType = Effect0_AuraType,
          Amplitude = Effect0_Amplitude,
          ProcValue = Effect0_ProcValue,
          ChainTargets = 0,
          MiscValue = Effect0_MiscValue,
          MiscValueB = Effect0_MiscValueB,
          MiscValueC = Effect0_MiscValueC,
          TriggerSpellId = SpellId.None,
          PointsPerComboPoint = 0.0f
        };
        spellEffect1.AffectMask[0] = 0U;
        spellEffect1.AffectMask[1] = 0U;
        spellEffect1.AffectMask[2] = 0U;
        if(spellEffect1.ImplicitTargetA == ImplicitSpellTargetType.AllEnemiesAroundCaster &&
           spellEffect1.ImplicitTargetB == ImplicitSpellTargetType.AllEnemiesInArea)
          spellEffect1.ImplicitTargetB = ImplicitSpellTargetType.None;
        spellEffectList.Add(spellEffect1);
        SpellEffect spellEffect2 = new SpellEffect(this, EffectIndex.One)
        {
          EffectType = Effect1_EffectType,
          DiceSides = 0,
          RealPointsPerLevel = 0.0f,
          BasePoints = 0,
          Mechanic = Effect1_Mehanic,
          ImplicitTargetA = Effect1_ImplicitTargetA,
          ImplicitTargetB = Effect1_ImplicitTargetB,
          Radius = Effect1_Radius,
          AuraType = Effect1_AuraType,
          Amplitude = Effect1_Amplitude,
          ProcValue = Effect1_ProcValue,
          ChainTargets = 0,
          MiscValue = Effect1_MiscValue,
          MiscValueB = Effect1_MiscValueB,
          MiscValueC = Effect1_MiscValueC,
          TriggerSpellId = SpellId.None,
          PointsPerComboPoint = 0.0f
        };
        spellEffect2.AffectMask[0] = 0U;
        spellEffect2.AffectMask[1] = 0U;
        spellEffect2.AffectMask[2] = 0U;
        if(spellEffect2.ImplicitTargetA == ImplicitSpellTargetType.AllEnemiesAroundCaster &&
           spellEffect2.ImplicitTargetB == ImplicitSpellTargetType.AllEnemiesInArea)
          spellEffect2.ImplicitTargetB = ImplicitSpellTargetType.None;
        spellEffectList.Add(spellEffect2);
        Effects = spellEffectList.ToArray();
        PowerCostPercentage = 0;
        SpellClassSet = SpellClassSet.Generic;
        MaxTargets = 100U;
        PreventionType = DamageType == DamageType.Magic
          ? SpellPreventionType.Magic
          : SpellPreventionType.Melee;
        RequiredToolCategories = new ToolCategory[2];
        for(int index = 0; index < RequiredToolCategories.Length; ++index)
          RequiredToolCategories[index] = ToolCategory.None;
        RuneCostEntry = new RuneCostEntry();
        if(CooldownTime > 5000)
          CooldownTime -= 1000;
        else if(CooldownTime > 0)
          CooldownTime -= 500;
        if(Name.Contains("Party"))
        {
          Effect0_ImplicitTargetA = ImplicitSpellTargetType.AllParty;
          Effect1_ImplicitTargetA = ImplicitSpellTargetType.AllParty;
        }

        SpellHandler.AddSpell(this);
      }
      catch(Exception ex)
      {
        LogUtil.WarnException("Error when finalizing data holder of spell {0}. {1}", (object) Name, (object) ex);
      }
    }

    public bool CanCast(NPC npc)
    {
      return CheckCasterConstraints(npc) == SpellFailedReason.Ok;
    }

    /// <summary>
    /// Checks whether the given spell can be casted by the casting Unit.
    /// Does not do range checks.
    /// </summary>
    public SpellFailedReason CheckCasterConstraints(Unit caster)
    {
      if(caster.IsInCombat && RequiresCasterOutOfCombat)
        return SpellFailedReason.AffectingCombat;
      if(!caster.CanDoHarm && HasHarmfulEffects)
        return SpellFailedReason.Pacified;
      if(InterruptFlags.HasFlag(InterruptFlags.OnSilence) &&
         caster.IsUnderInfluenceOf(SpellMechanic.Silenced))
        return SpellFailedReason.Silenced;
      if(!AttributesExD.HasFlag(SpellAttributesExD.UsableWhileStunned) && !caster.CanInteract)
        return SpellFailedReason.TooManySockets;
      if(!caster.CanCastSpells && (!IsPhysicalAbility ||
                                   InterruptFlags.HasFlag(InterruptFlags.OnSilence) &&
                                   caster.IsUnderInfluenceOf(SpellMechanic.Silenced)))
        return SpellFailedReason.Silenced;
      if(!caster.CanDoPhysicalActivity && IsPhysicalAbility || !caster.CanDoHarm && HasHarmfulEffects)
        return SpellFailedReason.Pacified;
      if(!AttributesExD.HasFlag(SpellAttributesExD.UsableWhileStunned) && !caster.CanInteract)
        return SpellFailedReason.Stunned;
      if(IsFinishingMove && caster.ComboPoints == 0)
        return SpellFailedReason.NoComboPoints;
      if(RequiredCasterAuraState != AuraState.None || ExcludeCasterAuraState != AuraState.None)
      {
        AuraStateMask auraState = caster.AuraState;
        if(RequiredCasterAuraState != AuraState.None &&
           !auraState.HasAnyFlag(RequiredCasterAuraState) ||
           ExcludeCasterAuraState != AuraState.None && auraState.HasAnyFlag(ExcludeCasterAuraState))
          return SpellFailedReason.CasterAurastate;
      }

      if(ExcludeCasterAuraId != SpellId.None && caster.Auras.Contains(ExcludeCasterAuraId) ||
         RequiredCasterAuraId != SpellId.None && !caster.Auras.Contains(RequiredCasterAuraId))
        return SpellFailedReason.CasterAurastate;
      SpellCollection spells = caster.Spells;
      if(spells != null && caster.CastingTill > DateTime.Now && !spells.IsReady(this))
        return SpellFailedReason.NotReady;
      if(caster is NPC && caster.Target != null && Range.MaxDist <
         (double) caster.GetDistance(caster.Target))
        return SpellFailedReason.OutOfRange;
      return !IsPassive && !caster.HasEnoughPowerToCast(this, null)
        ? SpellFailedReason.NoPower
        : SpellFailedReason.Ok;
    }

    /// <summary>Whether this spell has certain requirements on items</summary>
    public bool HasItemRequirements
    {
      get
      {
        if((RequiredItemClass == ItemClass.Consumable || RequiredItemClass == ItemClass.None) &&
           (RequiredItemInventorySlotMask == InventorySlotTypeMask.None && RequiredTools == null) &&
           RequiredToolCategories.Length <= 0)
          return EquipmentSlot != EquipmentSlot.End;
        return true;
      }
    }

    public SpellFailedReason CheckItemRestrictions(Item usedItem, PlayerInventory inv)
    {
      if(RequiredItemClass != ItemClass.None)
      {
        if(EquipmentSlot != EquipmentSlot.End)
          usedItem = inv[EquipmentSlot];
        if(usedItem == null)
          return SpellFailedReason.EquippedItem;
        if(RequiredItemClass > ItemClass.Consumable &&
           (usedItem.Template.Class != RequiredItemClass ||
            RequiredItemSubClassMask > ItemSubClassMask.None &&
            !usedItem.Template.SubClassMask.HasAnyFlag(RequiredItemSubClassMask)))
          return SpellFailedReason.EquippedItemClass;
      }

      if(RequiredItemInventorySlotMask != InventorySlotTypeMask.None && usedItem != null &&
         (usedItem.Template.InventorySlotMask & RequiredItemInventorySlotMask) ==
         InventorySlotTypeMask.None)
        return SpellFailedReason.EquippedItemClass;
      return CheckGeneralItemRestrictions(inv);
    }

    /// <summary>
    /// Checks whether the given inventory satisfies this Spell's item restrictions
    /// </summary>
    public SpellFailedReason CheckItemRestrictions(PlayerInventory inv)
    {
      return CheckItemRestrictionsWithout(inv, null);
    }

    /// <summary>
    /// Checks whether the given inventory satisfies this Spell's item restrictions
    /// </summary>
    public SpellFailedReason CheckItemRestrictionsWithout(PlayerInventory inv, Item exclude)
    {
      if(RequiredItemClass == ItemClass.Armor || RequiredItemClass == ItemClass.Weapon)
      {
        if(EquipmentSlot != EquipmentSlot.End)
        {
          Item obj = inv[EquipmentSlot];
          if(obj == null || obj == exclude)
            return SpellFailedReason.EquippedItem;
          if(!CheckItemRestriction(obj))
            return SpellFailedReason.EquippedItemClass;
        }
        else if(inv.Iterate(ItemMgr.EquippableInvSlotsByClass[(int) RequiredItemClass],
          i =>
          {
            if(i != exclude)
              return !CheckItemRestriction(i);
            return true;
          }))
          return SpellFailedReason.EquippedItemClass;
      }

      if(RequiredItemInventorySlotMask != InventorySlotTypeMask.None && inv.Iterate(
           RequiredItemInventorySlotMask, item =>
           {
             if(item != exclude)
               return (item.Template.InventorySlotMask & RequiredItemInventorySlotMask) ==
                      InventorySlotTypeMask.None;
             return true;
           }))
        return SpellFailedReason.EquippedItemClass;
      return CheckGeneralItemRestrictions(inv);
    }

    private bool CheckItemRestriction(Item item)
    {
      return item.Template.Class == RequiredItemClass &&
             (RequiredItemSubClassMask <= ItemSubClassMask.None ||
              item.Template.SubClassMask.HasAnyFlag(RequiredItemSubClassMask));
    }

    public SpellFailedReason CheckGeneralItemRestrictions(PlayerInventory inv)
    {
      if(RequiredTools != null)
      {
        foreach(ItemTemplate requiredTool in RequiredTools)
        {
          if(!inv.Contains(requiredTool.Id, 1, false))
            return SpellFailedReason.ItemNotFound;
        }
      }

      if(RequiredToolCategories.Length > 0 && !inv.CheckTotemCategories(RequiredToolCategories))
        return SpellFailedReason.TotemCategory;
      if(EquipmentSlot != EquipmentSlot.End)
      {
        Item obj = inv[EquipmentSlot];
        if(obj == null ||
           AttributesExC.HasFlag(SpellAttributesExC.RequiresWand) &&
           obj.Template.SubClass != ItemSubClass.WeaponWand ||
           AttributesExC.HasFlag(SpellAttributesExC.ShootRangedWeapon) &&
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
      if(AttributesEx.HasAnyFlag(SpellAttributesEx.CannotTargetSelf) && target == caster)
        return SpellFailedReason.NoValidTargets;
      if(target is Unit)
      {
        if(RequiredTargetAuraState != AuraState.None || ExcludeTargetAuraState != AuraState.None)
        {
          AuraStateMask auraState = ((Unit) target).AuraState;
          if(RequiredTargetAuraState != AuraState.None &&
             !auraState.HasAnyFlag(RequiredTargetAuraState) ||
             ExcludeTargetAuraState != AuraState.None &&
             auraState.HasAnyFlag(ExcludeTargetAuraState))
            return SpellFailedReason.TargetAurastate;
        }

        if(ExcludeTargetAuraId != SpellId.None &&
           ((Unit) target).Auras.Contains(ExcludeTargetAuraId) ||
           RequiredTargetAuraId != SpellId.None &&
           !((Unit) target).Auras.Contains(RequiredTargetAuraId))
          return SpellFailedReason.TargetAurastate;
      }

      if(TargetFlags.HasAnyFlag(SpellTargetFlags.UnkUnit_0x100) &&
         (!(target is GameObject) || !target.IsInWorld))
        return SpellFailedReason.BadTargets;
      if(!CanCastOnPlayer && target is Character)
        return SpellFailedReason.BadImplicitTargets;
      if(RequiresDeadTarget)
      {
        if(TargetFlags.HasAnyFlag(SpellTargetFlags.PvPCorpse | SpellTargetFlags.Corpse))
        {
          if(!(target is Corpse) || TargetFlags.HasAnyFlag(SpellTargetFlags.PvPCorpse) &&
             caster != null && !caster.IsHostileWith(target))
            return SpellFailedReason.BadImplicitTargets;
        }
        else if(target is NPC)
        {
          if(((Unit) target).IsAlive || target.Loot != null)
            return SpellFailedReason.TargetNotDead;
        }
        else if(target is Character && ((Unit) target).IsAlive)
          return SpellFailedReason.TargetNotDead;
      }
      else if(target is Unit && !((Unit) target).IsAlive)
        return SpellFailedReason.TargetsDead;

      if(AttributesExB.HasFlag(SpellAttributesExB.RequiresBehindTarget) && caster != null &&
         !caster.IsBehind(target))
        return SpellFailedReason.NotBehind;
      return (double) Range.MinDist > 0.0 && caster != null && caster.IsInRadius(target, Range.MinDist)
        ? SpellFailedReason.TooClose
        : SpellFailedReason.Ok;
    }

    public bool CanProcBeTriggeredBy(Unit owner, IUnitAction action, bool active)
    {
      if(action.Spell != null)
      {
        if(active)
        {
          if(CasterProcSpells != null)
            return CasterProcSpells.Contains(action.Spell);
        }
        else if(TargetProcSpells != null)
          return TargetProcSpells.Contains(action.Spell);

        if(action.Spell == this)
          return false;
      }

      if(RequiredItemClass == ItemClass.None)
        return true;
      if(!(action is DamageAction))
        return false;
      DamageAction damageAction = (DamageAction) action;
      if(damageAction.Weapon == null || !(damageAction.Weapon is Item))
        return false;
      ItemTemplate template = ((Item) damageAction.Weapon).Template;
      if(template.Class != RequiredItemClass)
        return false;
      if(RequiredItemSubClassMask != ItemSubClassMask.None)
        return template.SubClassMask.HasAnyFlag(RequiredItemSubClassMask);
      return true;
    }

    public int GetCooldown(Unit unit)
    {
      int cd = !(unit is NPC) || AISettings.Cooldown.MaxDelay <= 0
        ? CooldownTime
        : AISettings.Cooldown.GetRandomCooldown();
      if(cd == 0)
      {
        if(HasIndividualCooldown)
        {
          if(unit is Character)
          {
            Item obj = ((Character) unit).Inventory[EquipmentSlot];
            if(obj != null)
              cd = obj.AttackTime;
          }
          else if(unit is NPC)
            cd = ((NPC) unit).Entry.AttackTime;
        }
      }
      else
        cd = GetModifiedCooldown(unit, cd);

      return cd;
    }

    public int GetModifiedCooldown(Unit unit, int cd)
    {
      return unit.Auras.GetModifiedInt(SpellModifierType.CooldownTime, this, cd) + unit.IntMods[33];
    }

    public Spell()
    {
      AISettings = new AISpellSettings(this);
    }

    /// <summary>Add Spells to be casted on the targets of this Spell</summary>
    public void AddTargetTriggerSpells(params SpellId[] spellIds)
    {
      Spell[] spellArray = new Spell[spellIds.Length];
      for(int index = 0; index < spellIds.Length; ++index)
      {
        SpellId spellId = spellIds[index];
        Spell spell = SpellHandler.Get(spellId);
        if(spell == null)
          throw new InvalidSpellDataException("Invalid SpellId: " + spellId);
        spellArray[index] = spell;
      }

      AddTargetTriggerSpells(spellArray);
    }

    /// <summary>Add Spells to be casted on the targets of this Spell</summary>
    public void AddTargetTriggerSpells(params Spell[] spells)
    {
      if(TargetTriggerSpells == null)
      {
        TargetTriggerSpells = spells;
      }
      else
      {
        int length = TargetTriggerSpells.Length;
        Array.Resize(ref TargetTriggerSpells, length + spells.Length);
        Array.Copy(spells, 0, TargetTriggerSpells, length, spells.Length);
      }
    }

    /// <summary>Add Spells to be casted on the targets of this Spell</summary>
    public void AddCasterTriggerSpells(params SpellId[] spellIds)
    {
      Spell[] spellArray = new Spell[spellIds.Length];
      for(int index = 0; index < spellIds.Length; ++index)
      {
        SpellId spellId = spellIds[index];
        Spell spell = SpellHandler.Get(spellId);
        if(spell == null)
          throw new InvalidSpellDataException("Invalid SpellId: " + spellId);
        spellArray[index] = spell;
      }

      AddCasterTriggerSpells(spellArray);
    }

    /// <summary>Add Spells to be casted on the targets of this Spell</summary>
    public void AddCasterTriggerSpells(params Spell[] spells)
    {
      if(CasterTriggerSpells == null)
      {
        CasterTriggerSpells = spells;
      }
      else
      {
        int length = CasterTriggerSpells.Length;
        Array.Resize(ref CasterTriggerSpells, length + spells.Length);
        Array.Copy(spells, 0, CasterTriggerSpells, length, spells.Length);
      }
    }

    /// <summary>
    /// Add Handler to be enabled when this aura spell is active
    /// </summary>
    public void AddProcHandler(ProcHandlerTemplate handler)
    {
      if(ProcHandlers == null)
        ProcHandlers = new List<ProcHandlerTemplate>();
      ProcHandlers.Add(handler);
      if(Effects.Length != 0)
        return;
      AddAuraEffect(AuraType.Dummy);
    }

    /// <summary>Sets all default variables</summary>
    public void Initialize()
    {
      init1 = true;
      SpellEffect spellEffect = GetEffect(SpellEffectType.LearnSpell) ??
                                GetEffect(SpellEffectType.LearnPetSpell);
      if(spellEffect != null && spellEffect.TriggerSpellId != SpellId.None)
        IsTeachSpell = true;
      for(int index = 0; index < Effects.Length; ++index)
      {
        SpellEffect effect = Effects[index];
        if(effect.TriggerSpellId != SpellId.None || effect.AuraType == AuraType.PeriodicTriggerSpell)
        {
          Spell spell = SpellHandler.Get((uint) effect.TriggerSpellId);
          if(spell != null)
          {
            if(!IsTeachSpell)
              spell.IsTriggeredSpell = true;
            else
              LearnSpell = spell;
            effect.TriggerSpell = spell;
          }
          else if(IsTeachSpell)
            IsTeachSpell = GetEffect(SpellEffectType.LearnSpell) != null;
        }
      }

      foreach(SpellEffect effect in Effects)
      {
        if(effect.EffectType == SpellEffectType.PersistantAreaAura)
        {
          DOEffect = effect;
          break;
        }
      }
    }

    /// <summary>
    /// For all things that depend on info of all spells from first Init-round and other things
    /// </summary>
    internal void Init2()
    {
      if(init2)
      {
        return;
      }

      init2 = true;
      IsPassive = Attributes.HasFlag(SpellAttributes.Passive);
      IsChanneled =
        ((!IsPassive &&
          AttributesEx.HasAnyFlag(SpellAttributesEx.Channeled_1 | SpellAttributesEx.Channeled_2)) ||
         ChannelInterruptFlags > ChannelInterruptFlags.None);
      foreach(SpellEffect spellEffect in Effects)
      {
        spellEffect.Init2();
        if(spellEffect.IsHealEffect)
        {
          IsHealSpell = true;
        }

        if(spellEffect.EffectType == SpellEffectType.NormalizedWeaponDamagePlus)
        {
          IsDualWieldAbility = true;
        }
      }

      InitAura();
      if(IsChanneled)
      {
        if(Durations.Min == 0)
        {
          Durations.Min = (Durations.Max = 1000);
        }

        foreach(SpellEffect spellEffect2 in Effects)
        {
          if(spellEffect2.IsPeriodic)
          {
            ChannelAmplitude = spellEffect2.Amplitude;
            break;
          }
        }
      }

      IsOnNextStrike =
        Attributes.HasAnyFlag(SpellAttributes.OnNextMelee | SpellAttributes.OnNextMelee_2);
      IsRanged = (Attributes.HasAnyFlag(SpellAttributes.Ranged) ||
                  AttributesExC.HasFlag(SpellAttributesExC.ShootRangedWeapon));
      IsRangedAbility = (IsRanged && !IsTriggeredSpell);
      IsStrikeSpell = HasEffectWith(effect => effect.IsStrikeEffect);
      IsPhysicalAbility = ((IsRangedAbility || IsOnNextStrike || IsStrikeSpell) &&
                           !HasEffect(SpellEffectType.SchoolDamage));
      DamageIncreasedByAP = false;
      GeneratesComboPoints =
        HasEffectWith(effect => effect.EffectType == SpellEffectType.AddComboPoints);
      bool isFinishingMove;
      if(!AttributesEx.HasAnyFlag(SpellAttributesEx.FinishingMove))
      {
        isFinishingMove = HasEffectWith(effect =>
          effect.PointsPerComboPoint > 0f && effect.EffectType != SpellEffectType.Dummy);
      }
      else
      {
        isFinishingMove = true;
      }

      IsFinishingMove = isFinishingMove;
      TotemEffect = GetFirstEffectWith(effect => effect.HasTarget(ImplicitSpellTargetType.TotemAir,
        ImplicitSpellTargetType.TotemEarth, ImplicitSpellTargetType.TotemFire, ImplicitSpellTargetType.TotemWater));
      IsEnchantment = HasEffectWith(effect => effect.IsEnchantmentEffect);
      if(!IsEnchantment && EquipmentSlot == EquipmentSlot.End)
      {
        if(RequiredItemClass == ItemClass.Armor &&
           RequiredItemSubClassMask == ItemSubClassMask.Shield)
        {
          EquipmentSlot = EquipmentSlot.OffHand;
        }
        else if(AttributesExC.HasFlag(SpellAttributesExC.RequiresOffHandWeapon))
        {
          EquipmentSlot = EquipmentSlot.OffHand;
        }
        else if(IsRangedAbility || AttributesExC.HasFlag(SpellAttributesExC.RequiresWand))
        {
          EquipmentSlot = EquipmentSlot.ExtraWeapon;
        }
        else if(AttributesExC.HasFlag(SpellAttributesExC.RequiresMainHandWeapon))
        {
          EquipmentSlot = EquipmentSlot.MainHand;
        }
        else if(RequiredItemClass == ItemClass.Weapon)
        {
          if(RequiredItemSubClassMask == ItemSubClassMask.AnyMeleeWeapon)
          {
            EquipmentSlot = EquipmentSlot.MainHand;
          }
          else if(RequiredItemSubClassMask.HasAnyFlag(ItemSubClassMask.AnyRangedAndThrownWeapon))
          {
            EquipmentSlot = EquipmentSlot.ExtraWeapon;
          }
        }
        else if(IsPhysicalAbility)
        {
          EquipmentSlot = EquipmentSlot.MainHand;
        }
      }

      HasIndividualCooldown = (CooldownTime > 0 ||
                               (IsPhysicalAbility && !IsOnNextStrike &&
                                EquipmentSlot != EquipmentSlot.End));
      HasCooldown = (HasIndividualCooldown || CategoryCooldownTime > 0);
      SpellEffect effect2 = GetEffect(SpellEffectType.SkillStep);
      if(effect2 != null)
      {
        TeachesApprenticeAbility = (effect2.BasePoints == 0);
      }

      IsProfession = (!IsRangedAbility && Ability != null &&
                      Ability.Skill.Category == SkillCategory.Profession);
      IsEnhancer = HasEffectWith(effect => effect.IsEnhancer);
      IsFishing =
        HasEffectWith(effect => effect.HasTarget(ImplicitSpellTargetType.SelfFishing));
      IsSkinning = HasEffectWith(effect => effect.EffectType == SpellEffectType.Skinning);
      IsTameEffect =
        HasEffectWith(effect => effect.EffectType == SpellEffectType.TameCreature);
      if(AttributesEx.HasAnyFlag(SpellAttributesEx.Negative) || IsPreventionDebuff ||
         Mechanic.IsNegative())
      {
        HasHarmfulEffects = true;
        HasBeneficialEffects = false;
        HarmType = HarmType.Harmful;
      }
      else
      {
        HasHarmfulEffects =
          HasEffectWith(effect => effect.HarmType == HarmType.Harmful);
        HasBeneficialEffects =
          HasEffectWith(effect => effect.HarmType == HarmType.Beneficial);
        if(HasHarmfulEffects != HasBeneficialEffects)
        {
          if(!HasEffectWith(effect => effect.HarmType == HarmType.Neutral))
          {
            HarmType = (HasHarmfulEffects ? HarmType.Harmful : HarmType.Beneficial);
            goto IL_59E;
          }
        }

        HarmType = HarmType.Neutral;
      }

      IL_59E:
      RequiresDeadTarget = (HasEffect(SpellEffectType.Resurrect) ||
                            HasEffect(SpellEffectType.ResurrectFlat) ||
                            HasEffect(SpellEffectType.SelfResurrect));
      CostsPower = (PowerCost > 0 || PowerCostPercentage > 0);
      CostsRunes = (RuneCostEntry != null && RuneCostEntry.CostsRunes);
      HasTargets = HasEffectWith(effect => effect.HasTargets);
      bool casterIsTarget;
      if(HasTargets)
      {
        casterIsTarget =
          HasEffectWith(effect => effect.HasTarget(ImplicitSpellTargetType.Self));
      }
      else
      {
        casterIsTarget = false;
      }

      CasterIsTarget = casterIsTarget;
      IsAreaSpell = HasEffectWith(effect => effect.IsAreaEffect);
      bool isDamageSpell;
      if(HasHarmfulEffects && !HasBeneficialEffects)
      {
        isDamageSpell = HasEffectWith(effect =>
          effect.EffectType == SpellEffectType.Attack ||
          effect.EffectType == SpellEffectType.EnvironmentalDamage ||
          effect.EffectType == SpellEffectType.InstantKill ||
          effect.EffectType == SpellEffectType.SchoolDamage || effect.IsStrikeEffect);
      }
      else
      {
        isDamageSpell = false;
      }

      IsDamageSpell = isDamageSpell;
      if(DamageMultipliers[0] <= 0f)
      {
        DamageMultipliers[0] = 1f;
      }

      IsHearthStoneSpell = HasEffectWith(effect =>
        effect.HasTarget(ImplicitSpellTargetType.HeartstoneLocation));
      ForeachEffect(delegate(SpellEffect effect)
      {
        if(effect.ImplicitTargetA == ImplicitSpellTargetType.None &&
           effect.EffectType == SpellEffectType.ResurrectFlat)
        {
          effect.ImplicitTargetA = ImplicitSpellTargetType.SingleFriend;
        }
      });
      Schools = Utility.GetSetIndices<DamageSchool>((uint) SchoolMask);
      if(Schools.Length == 0)
      {
        DamageSchool[] schools = new DamageSchool[1];
        Schools = schools;
      }

      RequiresCasterOutOfCombat = (!HasHarmfulEffects &&
                                   (Attributes.HasFlag(SpellAttributes.CannotBeCastInCombat) ||
                                    AttributesEx.HasFlag(SpellAttributesEx.RemainOutOfCombat) ||
                                    AuraInterruptFlags.HasFlag(AuraInterruptFlags.OnStartAttack)));
      if(RequiresCasterOutOfCombat)
      {
        InterruptFlags |= InterruptFlags.OnTakeDamage;
      }

      IsThrow = (AttributesExC.HasFlag(SpellAttributesExC.ShootRangedWeapon) &&
                 Attributes.HasFlag(SpellAttributes.Ranged) && Ability != null &&
                 Ability.Skill.Id == SkillId.Thrown);
      bool hasModifierEffects;
      if(!HasModifierEffects)
      {
        hasModifierEffects = HasEffectWith(effect =>
          effect.AuraType == AuraType.AddModifierFlat || effect.AuraType == AuraType.AddModifierPercent);
      }
      else
      {
        hasModifierEffects = true;
      }

      HasModifierEffects = hasModifierEffects;
      CanCastOnPlayer = (CanCastOnPlayer && !HasEffect(AuraType.ModTaunt));
      HasAuraDependentEffects = HasEffectWith(effect => effect.IsDependentOnOtherAuras);
      ForeachEffect(delegate(SpellEffect effect)
      {
        for(int k = 0; k < 3; k++)
        {
          AllAffectingMasks[k] |= effect.AffectMask[k];
        }
      });
      if(Range.MaxDist == 0f)
      {
        Range.MaxDist = 5f;
      }

      if(RequiredToolIds == null)
      {
        RequiredToolIds = new uint[0];
      }
      else
      {
        if(RequiredToolIds.Length > 0 && (RequiredToolIds[0] > 0u || RequiredToolIds[1] > 0u))
        {
          SpellHandler.SpellsRequiringTools.Add(this);
        }

        ArrayUtil.PruneVals(ref RequiredToolIds);
      }

      SpellEffect firstEffectWith = GetFirstEffectWith(effect =>
        effect.EffectType == SpellEffectType.SkillStep || effect.EffectType == SpellEffectType.Skill);
      if(firstEffectWith != null)
      {
        SkillTier = (SkillTierId) firstEffectWith.BasePoints;
      }
      else
      {
        SkillTier = SkillTierId.End;
      }

      ArrayUtil.PruneVals(ref RequiredToolCategories);
      ForeachEffect(delegate(SpellEffect effect)
      {
        if(effect.SpellEffectHandlerCreator != null)
        {
          EffectHandlerCount++;
        }
      });
      if(GetEffect(SpellEffectType.QuestComplete) != null)
      {
        SpellHandler.QuestCompletors.Add(this);
      }

      AISettings.InitializeAfterLoad();
    }

    /// <summary>Sets the AITargetHandlerDefintion of all effects</summary>
    public void OverrideCustomTargetDefinitions(TargetAdder adder, params TargetFilter[] filters)
    {
      OverrideCustomTargetDefinitions(new TargetDefinition(adder, filters), null);
    }

    /// <summary>Sets the CustomTargetHandlerDefintion of all effects</summary>
    public void OverrideCustomTargetDefinitions(TargetAdder adder, TargetEvaluator evaluator = null,
      params TargetFilter[] filters)
    {
      OverrideCustomTargetDefinitions(new TargetDefinition(adder, filters), evaluator);
    }

    public void OverrideCustomTargetDefinitions(TargetDefinition def, TargetEvaluator evaluator = null)
    {
      ForeachEffect(effect => effect.CustomTargetHandlerDefintion = def);
      if(evaluator == null)
        return;
      OverrideCustomTargetEvaluators(evaluator);
    }

    /// <summary>Sets the AITargetHandlerDefintion of all effects</summary>
    public void OverrideAITargetDefinitions(TargetAdder adder, params TargetFilter[] filters)
    {
      OverrideAITargetDefinitions(new TargetDefinition(adder, filters), null);
    }

    /// <summary>Sets the AITargetHandlerDefintion of all effects</summary>
    public void OverrideAITargetDefinitions(TargetAdder adder, TargetEvaluator evaluator = null,
      params TargetFilter[] filters)
    {
      OverrideAITargetDefinitions(new TargetDefinition(adder, filters), evaluator);
    }

    public void OverrideAITargetDefinitions(TargetDefinition def, TargetEvaluator evaluator = null)
    {
      ForeachEffect(effect => effect.AITargetHandlerDefintion = def);
      if(evaluator == null)
        return;
      OverrideCustomTargetEvaluators(evaluator);
    }

    /// <summary>Sets the CustomTargetEvaluator of all effects</summary>
    public void OverrideCustomTargetEvaluators(TargetEvaluator eval)
    {
      ForeachEffect(effect => effect.CustomTargetEvaluator = eval);
    }

    /// <summary>Sets the AITargetEvaluator of all effects</summary>
    public void OverrideAITargetEvaluators(TargetEvaluator eval)
    {
      ForeachEffect(effect => effect.AITargetEvaluator = eval);
    }

    public void ForeachEffect(Action<SpellEffect> callback)
    {
      for(int index = 0; index < Effects.Length; ++index)
      {
        SpellEffect effect = Effects[index];
        callback(effect);
      }
    }

    public bool HasEffectWith(Predicate<SpellEffect> predicate)
    {
      for(int index = 0; index < Effects.Length; ++index)
      {
        SpellEffect effect = Effects[index];
        if(predicate(effect))
          return true;
      }

      return false;
    }

    public bool HasEffect(SpellEffectType type)
    {
      return GetEffect(type, false) != null;
    }

    public bool HasEffect(AuraType type)
    {
      return GetEffect(type, false) != null;
    }

    /// <summary>
    /// Returns the first SpellEffect of the given Type within this Spell
    /// </summary>
    public SpellEffect GetEffect(SpellEffectType type)
    {
      return GetEffect(type, true);
    }

    /// <summary>
    /// Returns the first SpellEffect of the given Type within this Spell
    /// </summary>
    public SpellEffect GetEffect(SpellEffectType type, bool force)
    {
      foreach(SpellEffect effect in Effects)
      {
        if(effect.EffectType == type)
          return effect;
      }

      if(!init1 && force)
        throw new ContentException("Spell {0} does not contain Effect of type {1}", (object) this, (object) type);
      return null;
    }

    /// <summary>
    /// Returns the first SpellEffect of the given Type within this Spell
    /// </summary>
    public SpellEffect GetEffect(AuraType type)
    {
      return GetEffect(type, ContentMgr.ForceDataPresence);
    }

    /// <summary>
    /// Returns the first SpellEffect of the given Type within this Spell
    /// </summary>
    public SpellEffect GetEffect(AuraType type, bool force)
    {
      foreach(SpellEffect effect in Effects)
      {
        if(effect.AuraType == type)
          return effect;
      }

      if(!init1 && force)
        throw new ContentException("Spell {0} does not contain Aura Effect of type {1}", (object) this, (object) type);
      return null;
    }

    public SpellEffect GetFirstEffectWith(Predicate<SpellEffect> predicate)
    {
      foreach(SpellEffect effect in Effects)
      {
        if(predicate(effect))
          return effect;
      }

      return null;
    }

    public SpellEffect[] GetEffectsWhere(Predicate<SpellEffect> predicate)
    {
      List<SpellEffect> spellEffectList = null;
      foreach(SpellEffect effect in Effects)
      {
        if(predicate(effect))
        {
          if(spellEffectList == null)
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
      SpellEffect spellEffect = AddEffect(SpellEffectType.Dummy, target);
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
      SpellEffect[] spellEffectArray = new SpellEffect[Effects.Length + 1];
      Array.Copy(Effects, spellEffectArray, Effects.Length);
      Effects = spellEffectArray;
      Effects[spellEffectArray.Length - 1] = spellEffect;
      spellEffect.ImplicitTargetA = target;
      return spellEffect;
    }

    /// <summary>
    /// Adds a SpellEffect that will trigger the given Spell on oneself
    /// </summary>
    public SpellEffect AddTriggerSpellEffect(SpellId triggerSpell)
    {
      return AddTriggerSpellEffect(triggerSpell, ImplicitSpellTargetType.Self);
    }

    /// <summary>
    /// Adds a SpellEffect that will trigger the given Spell on the given type of target
    /// </summary>
    public SpellEffect AddTriggerSpellEffect(SpellId triggerSpell, ImplicitSpellTargetType targetType)
    {
      SpellEffect spellEffect = AddEffect(SpellEffectType.TriggerSpell, targetType);
      spellEffect.TriggerSpellId = triggerSpell;
      return spellEffect;
    }

    /// <summary>
    /// Adds a SpellEffect that will trigger the given Spell on oneself
    /// </summary>
    public SpellEffect AddPeriodicTriggerSpellEffect(SpellId triggerSpell)
    {
      return AddPeriodicTriggerSpellEffect(triggerSpell, ImplicitSpellTargetType.Self);
    }

    /// <summary>
    /// Adds a SpellEffect that will trigger the given Spell on the given type of target
    /// </summary>
    public SpellEffect AddPeriodicTriggerSpellEffect(SpellId triggerSpell, ImplicitSpellTargetType targetType)
    {
      SpellEffect spellEffect = AddAuraEffect(AuraType.PeriodicTriggerSpell);
      spellEffect.TriggerSpellId = triggerSpell;
      spellEffect.ImplicitTargetA = targetType;
      return spellEffect;
    }

    /// <summary>
    /// Adds a SpellEffect that will be applied to an Aura to be casted on oneself
    /// </summary>
    public SpellEffect AddAuraEffect(AuraType type)
    {
      return AddAuraEffect(type, ImplicitSpellTargetType.Self);
    }

    /// <summary>
    /// Adds a SpellEffect that will be applied to an Aura to be casted on the given type of target
    /// </summary>
    public SpellEffect AddAuraEffect(AuraType type, ImplicitSpellTargetType targetType)
    {
      SpellEffect spellEffect = AddEffect(SpellEffectType.ApplyAura, targetType);
      spellEffect.AuraType = type;
      return spellEffect;
    }

    /// <summary>
    /// Adds a SpellEffect that will be applied to an Aura to be casted on the given type of target
    /// </summary>
    public SpellEffect AddAuraEffect(AuraEffectHandlerCreator creator)
    {
      return AddAuraEffect(creator, ImplicitSpellTargetType.Self);
    }

    /// <summary>
    /// Adds a SpellEffect that will be applied to an Aura to be casted on the given type of target
    /// </summary>
    public SpellEffect AddAuraEffect(AuraEffectHandlerCreator creator, ImplicitSpellTargetType targetType)
    {
      SpellEffect spellEffect = AddEffect(SpellEffectType.ApplyAura, targetType);
      spellEffect.AuraType = AuraType.Dummy;
      spellEffect.AuraEffectHandlerCreator = creator;
      return spellEffect;
    }

    public void ClearEffects()
    {
      Effects = new SpellEffect[0];
    }

    public SpellEffect RemoveEffect(AuraType type)
    {
      SpellEffect effect = GetEffect(type);
      RemoveEffect(effect);
      return effect;
    }

    public SpellEffect RemoveEffect(SpellEffectType type)
    {
      SpellEffect effect = GetEffect(type);
      RemoveEffect(effect);
      return effect;
    }

    public void RemoveEffect(SpellEffect toRemove)
    {
      SpellEffect[] spellEffectArray = new SpellEffect[Effects.Length - 1];
      int num = 0;
      foreach(SpellEffect effect in Effects)
      {
        if(effect != toRemove)
          spellEffectArray[num++] = effect;
      }

      Effects = spellEffectArray;
    }

    public void RemoveEffect(Func<SpellEffect, bool> predicate)
    {
      foreach(SpellEffect toRemove in Effects.ToArray())
      {
        if(predicate(toRemove))
          RemoveEffect(toRemove);
      }
    }

    public bool IsAffectedBy(Spell spell)
    {
      return MatchesMask(spell.AllAffectingMasks);
    }

    public bool MatchesMask(uint[] masks)
    {
      for(int index = 0; index < SpellClassMask.Length; ++index)
      {
        if(((int) masks[index] & (int) SpellClassMask[index]) != 0)
          return true;
      }

      return false;
    }

    public int GetMaxLevelDiff(int casterLevel)
    {
      if(MaxLevel >= BaseLevel && MaxLevel < casterLevel)
        return MaxLevel - BaseLevel;
      return Math.Abs(casterLevel - BaseLevel);
    }

    public int CalcBasePowerCost(Unit caster)
    {
      int num = PowerCost + PowerCostPerlevel * GetMaxLevelDiff(caster.Level);
      if(PowerCostPercentage > 0)
        num += PowerCostPercentage *
               (PowerType == PowerType.Health ? caster.BaseHealth : caster.BasePower) / 100;
      return num;
    }

    public int CalcPowerCost(Unit caster, DamageSchool school)
    {
      return caster.GetPowerCost(school, this, CalcBasePowerCost(caster));
    }

    public bool IsVisibleToClient
    {
      get
      {
        if(!IsRangedAbility && Visual == 0U && (Visual2 == 0U && !IsChanneled) &&
           CastDelay <= 0U)
          return HasCooldown;
        return true;
      }
    }

    public void SetDuration(int duration)
    {
      Durations.Min = Durations.Max = duration;
    }

    /// <summary>
    /// Returns the max duration for this Spell in milliseconds,
    /// including all modifiers.
    /// </summary>
    public int GetDuration(ObjectReference caster)
    {
      return GetDuration(caster, null);
    }

    /// <summary>
    /// Returns the max duration for this Spell in milliseconds,
    /// including all modifiers.
    /// </summary>
    public int GetDuration(ObjectReference caster, Unit target)
    {
      int num = Durations.Min;
      if(Durations.Max > Durations.Min && IsFinishingMove && caster.UnitMaster != null)
        num += caster.UnitMaster.ComboPoints * ((Durations.Max - Durations.Min) / 5);
      if(target != null && Mechanic != SpellMechanic.None)
      {
        int mechanicDurationMod = target.GetMechanicDurationMod(Mechanic);
        if(mechanicDurationMod != 0)
          num = UnitUpdates.GetMultiMod(mechanicDurationMod / 100f, num);
      }

      Unit unitMaster = caster.UnitMaster;
      if(unitMaster != null)
        num = unitMaster.Auras.GetModifiedInt(SpellModifierType.Duration, this, num);
      return num;
    }

    public bool IsAffectedByInvulnerability
    {
      get { return !Attributes.HasFlag(SpellAttributes.UnaffectedByInvulnerability); }
    }

    public bool CanFailDueToImmuneAgainstTarget(Unit spellTarget)
    {
      Character character = spellTarget as Character;
      if(IsAffectedByInvulnerability)
        return true;
      if(character != null)
        return character.Role.IsStaff;
      return false;
    }

    /// <summary>Fully qualified name</summary>
    public string FullName
    {
      get
      {
        bool flag1 = Talent != null;
        bool flag2 = Ability != null;
        string str = !flag1 ? Name : Talent.FullName;
        if(flag2 && !flag1 && (Ability.Skill.Category != SkillCategory.Language &&
                               Ability.Skill.Category != SkillCategory.Invalid))
          str = ((int) Ability.Skill.Category) + " " + str;
        if(IsTeachSpell && !Name.StartsWith("Learn", StringComparison.InvariantCultureIgnoreCase))
          str = "Learn " + str;
        else if(IsTriggeredSpell)
          str = "Effect: " + str;
        if(!flag2)
        {
          if(IsDeprecated)
            str = "Unused " + str;
          else if(Description != null)
          {
            int length = Description.Length;
          }
        }

        return str;
      }
    }

    /// <summary>Spells that contain "zzOld", "test", "unused"</summary>
    public bool IsDeprecated
    {
      get { return IsDeprecatedSpellName(Name); }
    }

    public static bool IsDeprecatedSpellName(string name)
    {
      return true;
    }

    public override string ToString()
    {
      return FullName + (RankDesc != "" ? " " + RankDesc : (object) "") + " (Id: " +
             Id + ")";
    }

    public void Dump(TextWriter writer, string indent)
    {
      writer.WriteLine("Spell: " + this + " [" + SpellId + "]");
      if(Category != 0U)
        writer.WriteLine(indent + "Category: " + Category);
      if(Line != null)
        writer.WriteLine(indent + "Line: " + Line);
      if(PreviousRank != null)
        writer.WriteLine(indent + "Previous Rank: " + PreviousRank);
      if(NextRank != null)
        writer.WriteLine(indent + "Next Rank: " + NextRank);
      if(DispelType != DispelType.None)
        writer.WriteLine(indent + "DispelType: " + DispelType);
      if(Mechanic != SpellMechanic.None)
        writer.WriteLine(indent + "Mechanic: " + Mechanic);
      if(Attributes != SpellAttributes.None)
        writer.WriteLine(indent + "Attributes: " + Attributes);
      if(AttributesEx != SpellAttributesEx.None)
        writer.WriteLine(indent + "AttributesEx: " + AttributesEx);
      if(AttributesExB != SpellAttributesExB.None)
        writer.WriteLine(indent + "AttributesExB: " + AttributesExB);
      if(AttributesExC != SpellAttributesExC.None)
        writer.WriteLine(indent + "AttributesExC: " + AttributesExC);
      if(AttributesExD != SpellAttributesExD.None)
        writer.WriteLine(indent + "AttributesExD: " + AttributesExD);
      if(RequiredShapeshiftMask != ShapeshiftMask.None)
        writer.WriteLine(indent + "ShapeshiftMask: " + RequiredShapeshiftMask);
      if(ExcludeShapeshiftMask != ShapeshiftMask.None)
        writer.WriteLine(indent + "ExcludeShapeshiftMask: " + ExcludeShapeshiftMask);
      if(TargetFlags != SpellTargetFlags.Self)
        writer.WriteLine(indent + "TargetType: " + TargetFlags);
      if(CreatureMask != CreatureMask.None)
        writer.WriteLine(indent + "TargetUnitTypes: " + CreatureMask);
      if(RequiredSpellFocus != SpellFocus.None)
        writer.WriteLine(indent + "RequiredSpellFocus: " + RequiredSpellFocus);
      if(FacingFlags != 0)
        writer.WriteLine(indent + "FacingFlags: " + FacingFlags);
      if(RequiredCasterAuraState != AuraState.None)
        writer.WriteLine(indent + "RequiredCasterAuraState: " + RequiredCasterAuraState);
      if(RequiredTargetAuraState != AuraState.None)
        writer.WriteLine(indent + "RequiredTargetAuraState: " + RequiredTargetAuraState);
      if(ExcludeCasterAuraState != AuraState.None)
        writer.WriteLine(indent + "ExcludeCasterAuraState: " + ExcludeCasterAuraState);
      if(ExcludeTargetAuraState != AuraState.None)
        writer.WriteLine(indent + "ExcludeTargetAuraState: " + ExcludeTargetAuraState);
      if(RequiredCasterAuraId != SpellId.None)
        writer.WriteLine(indent + "RequiredCasterAuraId: " + RequiredCasterAuraId);
      if(RequiredTargetAuraId != SpellId.None)
        writer.WriteLine(indent + "RequiredTargetAuraId: " + RequiredTargetAuraId);
      if(ExcludeCasterAuraId != SpellId.None)
        writer.WriteLine(indent + "ExcludeCasterAuraSpellId: " + ExcludeCasterAuraId);
      if(ExcludeTargetAuraId != SpellId.None)
        writer.WriteLine(indent + "ExcludeTargetAuraSpellId: " + ExcludeTargetAuraId);
      if(CastDelay != 0U)
        writer.WriteLine(indent + "StartTime: " + CastDelay);
      if(CooldownTime > 0)
        writer.WriteLine(indent + "CooldownTime: " + CooldownTime);
      if(categoryCooldownTime > 0)
        writer.WriteLine(indent + "CategoryCooldownTime: " + categoryCooldownTime);
      if(InterruptFlags != InterruptFlags.None)
        writer.WriteLine(indent + "InterruptFlags: " + InterruptFlags);
      if(AuraInterruptFlags != AuraInterruptFlags.None)
        writer.WriteLine(indent + "AuraInterruptFlags: " + AuraInterruptFlags);
      if(ChannelInterruptFlags != ChannelInterruptFlags.None)
        writer.WriteLine(indent + "ChannelInterruptFlags: " + ChannelInterruptFlags);
      if(ProcTriggerFlagsProp != ProcTriggerFlags.None)
      {
        writer.WriteLine(indent + "ProcTriggerFlags: " + ProcTriggerFlagsProp);
        if(ProcHitFlags != ProcHitFlags.None)
          writer.WriteLine(indent + "ProcHitFlags: " + ProcHitFlags);
      }

      if(ProcChance != 0U)
        writer.WriteLine(indent + "ProcChance: " + ProcChance);
      if(ProcCharges != 0)
        writer.WriteLine(indent + "ProcCharges: " + ProcCharges);
      if(MaxLevel != 0)
        writer.WriteLine(indent + "MaxLevel: " + MaxLevel);
      if(BaseLevel != 0)
        writer.WriteLine(indent + "BaseLevel: " + BaseLevel);
      if(Level != 0)
        writer.WriteLine(indent + "Level: " + Level);
      if(Durations.Max > 0)
        writer.WriteLine(indent + "Duration: " + Durations.Min + " - " +
                         Durations.Max + " (" + Durations.LevelDelta + ")");
      if(Visual != 0U)
        writer.WriteLine(indent + "Visual: " + Visual);
      if(PowerType != PowerType.Mana)
        writer.WriteLine(indent + "PowerType: " + PowerType);
      if(PowerCost != 0)
        writer.WriteLine(indent + "PowerCost: " + PowerCost);
      if(PowerCostPerlevel != 0)
        writer.WriteLine(indent + "PowerCostPerlevel: " + PowerCostPerlevel);
      if(PowerPerSecond != 0)
        writer.WriteLine(indent + "PowerPerSecond: " + PowerPerSecond);
      if(PowerPerSecondPerLevel != 0)
        writer.WriteLine(indent + "PowerPerSecondPerLevel: " + PowerPerSecondPerLevel);
      if(PowerCostPercentage != 0)
        writer.WriteLine(indent + "PowerCostPercentage: " + PowerCostPercentage);
      if(Range.MinDist != 0.0 || Range.MaxDist != (double) DefaultSpellRange)
        writer.WriteLine(indent + "Range: " + Range.MinDist + " - " +
                         Range.MaxDist);
      if((int) ProjectileSpeed != 0)
        writer.WriteLine(indent + "ProjectileSpeed: " + ProjectileSpeed);
      if(ModalNextSpell != SpellId.None)
        writer.WriteLine(indent + "ModalNextSpell: " + ModalNextSpell);
      if(MaxStackCount != 0)
        writer.WriteLine(indent + "MaxStackCount: " + MaxStackCount);
      if(RequiredTools != null)
      {
        writer.WriteLine(indent + "RequiredTools:");
        foreach(ItemTemplate requiredTool in RequiredTools)
          writer.WriteLine(indent + "\t" + requiredTool);
      }

      if(RequiredItemClass != ItemClass.None)
        writer.WriteLine(indent + "RequiredItemClass: " + RequiredItemClass);
      if(RequiredItemInventorySlotMask != InventorySlotTypeMask.None)
        writer.WriteLine(indent + "RequiredItemInventorySlotMask: " +
                         RequiredItemInventorySlotMask);
      if(RequiredItemSubClassMask != ~ItemSubClassMask.None &&
         RequiredItemSubClassMask != ItemSubClassMask.None)
        writer.WriteLine(indent + "RequiredItemSubClassMask: " + RequiredItemSubClassMask);
      if(Visual2 != 0U)
        writer.WriteLine(indent + "Visual2: " + Visual2);
      if(Priority != 0U)
        writer.WriteLine(indent + "Priority: " + Priority);
      if(StartRecoveryCategory != 0)
        writer.WriteLine(indent + "StartRecoveryCategory: " + StartRecoveryCategory);
      if(StartRecoveryTime != 0)
        writer.WriteLine(indent + "StartRecoveryTime: " + StartRecoveryTime);
      if(MaxTargetLevel != 0U)
        writer.WriteLine(indent + "MaxTargetLevel: " + MaxTargetLevel);
      if(SpellClassSet != SpellClassSet.Generic)
        writer.WriteLine(indent + "SpellClassSet: " + SpellClassSet);
      if(SpellClassMask[0] != 0U || SpellClassMask[1] != 0U || SpellClassMask[2] != 0U)
        writer.WriteLine(indent + "SpellClassMask: {0}{1}{2}", SpellClassMask[0].ToString("X8"),
          SpellClassMask[1].ToString("X8"), SpellClassMask[2].ToString("X8"));
      if(MaxTargets != 0U)
        writer.WriteLine(indent + "MaxTargets: " + MaxTargets);
      if(StanceBarOrder != 0)
        writer.WriteLine(indent + "StanceBarOrder: " + StanceBarOrder);
      if(DamageType != DamageType.None)
        writer.WriteLine(indent + "DamageType: " + DamageType);
      if(HarmType != HarmType.Neutral)
        writer.WriteLine(indent + "HarmType: " + HarmType);
      if(PreventionType != SpellPreventionType.None)
        writer.WriteLine(indent + "PreventionType: " + PreventionType);
      if(DamageMultipliers.Any(
        mult => (double) mult != 1.0))
        writer.WriteLine(indent + "DamageMultipliers: " +
                         DamageMultipliers.ToString(", "));
      for(int index = 0; index < RequiredToolCategories.Length; ++index)
      {
        if(RequiredToolCategories[index] != ToolCategory.None)
          writer.WriteLine(indent + "RequiredTotemCategoryId[" + index + "]: " +
                           RequiredToolCategories[index]);
      }

      if(AreaGroupId != 0U)
        writer.WriteLine(indent + "AreaGroupId: " + AreaGroupId);
      if(SchoolMask != DamageSchoolMask.None)
        writer.WriteLine(indent + "SchoolMask: " + SchoolMask);
      if(RuneCostEntry != null)
      {
        writer.WriteLine(indent + "RuneCostId: " + RuneCostEntry.Id);
        string str = indent + "\t";
        List<string> collection = new List<string>(3);
        if(RuneCostEntry.CostPerType[0] != 0)
          collection.Add(string.Format("Blood: {0}", RuneCostEntry.CostPerType[0]));
        if(RuneCostEntry.CostPerType[1] != 0)
          collection.Add(string.Format("Unholy: {0}", RuneCostEntry.CostPerType[1]));
        if(RuneCostEntry.CostPerType[2] != 0)
          collection.Add(string.Format("Frost: {0}", RuneCostEntry.CostPerType[2]));
        writer.WriteLine(str + "Runes - {0}",
          collection.Count == 0 ? "<None>" : collection.ToString(", "));
        writer.WriteLine(str + "RunicPowerGain: {0}", RuneCostEntry.RunicPowerGain);
      }

      if(MissileId != 0U)
        writer.WriteLine(indent + "MissileId: " + MissileId);
      if(!string.IsNullOrEmpty(Description))
        writer.WriteLine(indent + "Desc: " + Description);
      if(Reagents != null && Reagents.Length > 0)
        writer.WriteLine(indent + "Reagents: " +
                         Reagents.ToString(
                           ", "));
      if(Ability != null)
        writer.WriteLine(indent + string.Format("Skill: {0}", Ability.SkillInfo));
      if(Talent != null)
        writer.WriteLine(indent + string.Format("TalentTree: {0}", Talent.Tree));
      writer.WriteLine();
      foreach(SpellEffect effect in Effects)
        effect.DumpInfo(writer, "\t\t");
    }

    public bool IsBeneficialFor(ObjectReference casterReference, WorldObject target)
    {
      if(IsBeneficial)
        return true;
      if(!IsNeutral)
        return false;
      if(casterReference.Object != null)
        return !casterReference.Object.MayAttack(target);
      return true;
    }

    public bool IsHarmfulFor(ObjectReference casterReference, WorldObject target)
    {
      if(IsHarmful)
        return true;
      if(IsNeutral && casterReference.Object != null)
        return casterReference.Object.MayAttack(target);
      return false;
    }

    public bool IsBeneficial
    {
      get { return HarmType == HarmType.Beneficial; }
    }

    public bool IsHarmful
    {
      get { return HarmType == HarmType.Harmful; }
    }

    public bool IsNeutral
    {
      get { return HarmType == HarmType.Neutral; }
    }

    public override bool Equals(object obj)
    {
      if(obj is Spell)
        return (int) ((Spell) obj).Id == (int) Id;
      return false;
    }

    public override int GetHashCode()
    {
      return (int) Id;
    }

    public IEnumerator<Spell> GetEnumerator()
    {
      return new SingleEnumerator<Spell>(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    protected Spell Clone()
    {
      return (Spell) MemberwiseClone();
    }

    internal static void InitDbcs()
    {
    }

    public void PrintEffects(TextWriter writer)
    {
      foreach(SpellEffect effect in Effects)
        effect.DumpInfo(writer, "");
    }

    public int CategoryCooldownTime
    {
      get { return categoryCooldownTime; }
    }

    /// <summary>
    /// Indicates events which cause this spell to trigger its proc effect
    /// </summary>
    /// <remarks>
    /// This spell must be a proc <see cref="F:WCell.RealmServer.Spells.Spell.IsProc" />
    /// </remarks>
    public ProcTriggerFlags ProcTriggerFlagsProp
    {
      get { return ProcTriggerFlags; }
      set
      {
        ProcTriggerFlags = value;
        if(!ProcTriggerFlags.RequireHitFlags() || ProcHitFlags != ProcHitFlags.None)
          return;
        ProcHitFlags = ProcHitFlags.Hit;
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
      get { return m_RankDesc; }
      set
      {
        m_RankDesc = value;
        if(value.Length <= 0)
          return;
        Match match = numberRegex.Match(value);
        if(!match.Success)
          return;
        int.TryParse(match.Value, out Rank);
      }
    }

    public SpellClassSet SpellClassSet
    {
      get { return spellClassSet; }
      set
      {
        spellClassSet = value;
        ClassId = value.ToClassId();
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
        return Utility.Random(Min, Max);
      }
    }

    public class DBCDurationConverter : AdvancedDBCRecordConverter<DurationEntry>
    {
      public override DurationEntry ConvertTo(byte[] rawData, ref int id)
      {
        DurationEntry durationEntry = new DurationEntry();
        id = (int) GetUInt32(rawData, 0);
        durationEntry.Min = GetInt32(rawData, 1);
        durationEntry.LevelDelta = GetInt32(rawData, 2);
        durationEntry.Max = GetInt32(rawData, 3);
        return durationEntry;
      }
    }

    public class DBCRadiusConverter : AdvancedDBCRecordConverter<float>
    {
      public override float ConvertTo(byte[] rawData, ref int id)
      {
        id = (int) GetUInt32(rawData, 0);
        return GetFloat(rawData, 1);
      }
    }

    public class DBCCastTimeConverter : AdvancedDBCRecordConverter<uint>
    {
      public override uint ConvertTo(byte[] rawData, ref int id)
      {
        id = (int) GetUInt32(rawData, 0);
        return GetUInt32(rawData, 1);
      }
    }

    public class DBCRangeConverter : AdvancedDBCRecordConverter<SimpleRange>
    {
      public override SimpleRange ConvertTo(byte[] rawData, ref int id)
      {
        SimpleRange simpleRange = new SimpleRange();
        id = GetInt32(rawData, 0);
        simpleRange.MinDist = (uint) GetFloat(rawData, 1);
        simpleRange.MaxDist = (uint) GetFloat(rawData, 3);
        return simpleRange;
      }
    }

    public class DBCMechanicConverter : AdvancedDBCRecordConverter<string>
    {
      public override string ConvertTo(byte[] rawData, ref int id)
      {
        id = GetInt32(rawData, 0);
        return GetString(rawData, 1);
      }
    }

    public struct SpellFocusEntry
    {
      public uint Id;
      public string Name;
    }

    public class DBCSpellFocusConverter : AdvancedDBCRecordConverter<SpellFocusEntry>
    {
      public override SpellFocusEntry ConvertTo(byte[] rawData, ref int id)
      {
        return new SpellFocusEntry
        {
          Id = (uint) (id = GetInt32(rawData, 0)),
          Name = GetString(rawData, 1)
        };
      }
    }

    public class DBCSpellRuneCostConverter : AdvancedDBCRecordConverter<RuneCostEntry>
    {
      public override RuneCostEntry ConvertTo(byte[] rawData, ref int id)
      {
        RuneCostEntry runeCostEntry = new RuneCostEntry
        {
          Id = (uint) (id = GetInt32(rawData, 0)),
          RunicPowerGain = GetInt32(rawData, 4)
        };
        for(int index = 0; index < 3; ++index)
          runeCostEntry.RequiredRuneAmount += runeCostEntry.CostPerType[index] =
            GetInt32(rawData, index + 1);
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
        int uint32 = (int) GetUInt32(data, field);
        spell2.Id = (uint) uint32;
        spell1.SpellId = (SpellId) GetInt32(rawData, 0);
        Spell spell3 = spell1;
        try
        {
          spell3.Category = GetUInt32(rawData, currentIndex++);
          spell3.DispelType = (DispelType) GetUInt32(rawData, currentIndex++);
          spell3.Mechanic = (SpellMechanic) GetUInt32(rawData, currentIndex++);
          spell3.Attributes = (SpellAttributes) GetUInt32(rawData, currentIndex++);
          spell3.AttributesEx = (SpellAttributesEx) GetUInt32(rawData, currentIndex++);
          spell3.AttributesExB = (SpellAttributesExB) GetUInt32(rawData, currentIndex++);
          spell3.AttributesExC = (SpellAttributesExC) GetUInt32(rawData, currentIndex++);
          spell3.AttributesExD = (SpellAttributesExD) GetUInt32(rawData, currentIndex++);
          spell3.AttributesExE = (SpellAttributesExE) GetUInt32(rawData, currentIndex++);
          spell3.AttributesExF = (SpellAttributesExF) GetUInt32(rawData, currentIndex++);
          spell3.RequiredShapeshiftMask =
            (ShapeshiftMask) GetUInt32(rawData, currentIndex++);
          spell3.Unk_322_1 = GetUInt32(rawData, currentIndex++);
          spell3.ExcludeShapeshiftMask =
            (ShapeshiftMask) GetUInt32(rawData, currentIndex++);
          spell3.Unk_322_2 = GetUInt32(rawData, currentIndex++);
          spell3.TargetFlags = (SpellTargetFlags) GetUInt32(rawData, currentIndex++);
          spell3.Unk_322_3 = GetUInt32(rawData, currentIndex++);
          spell3.CreatureMask = (CreatureMask) GetUInt32(rawData, currentIndex++);
          spell3.RequiredSpellFocus = (SpellFocus) GetUInt32(rawData, currentIndex++);
          spell3.FacingFlags = (SpellFacingFlags) GetUInt32(rawData, currentIndex++);
          spell3.RequiredCasterAuraState = (AuraState) GetUInt32(rawData, currentIndex++);
          spell3.RequiredTargetAuraState = (AuraState) GetUInt32(rawData, currentIndex++);
          spell3.ExcludeCasterAuraState = (AuraState) GetUInt32(rawData, currentIndex++);
          spell3.ExcludeTargetAuraState = (AuraState) GetUInt32(rawData, currentIndex++);
          spell3.RequiredCasterAuraId = (SpellId) GetUInt32(rawData, currentIndex++);
          spell3.RequiredTargetAuraId = (SpellId) GetUInt32(rawData, currentIndex++);
          spell3.ExcludeCasterAuraId = (SpellId) GetUInt32(rawData, currentIndex++);
          spell3.ExcludeTargetAuraId = (SpellId) GetUInt32(rawData, currentIndex++);
          int int32_1 = GetInt32(rawData, currentIndex++);
          if(int32_1 > 0 &&
             !mappeddbcCastTimeReader.Entries.TryGetValue(int32_1, out spell3.CastDelay))
            ContentMgr.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid CastTime-Entry: {1}",
              (object) spell3.Name, (object) int32_1);
          spell3.CooldownTime = Math.Max(0,
            GetInt32(rawData, currentIndex++) - (int) spell3.CastDelay);
          spell3.categoryCooldownTime = GetInt32(rawData, currentIndex++);
          spell3.InterruptFlags = (InterruptFlags) GetUInt32(rawData, currentIndex++);
          spell3.AuraInterruptFlags =
            (AuraInterruptFlags) GetUInt32(rawData, currentIndex++);
          spell3.ChannelInterruptFlags =
            (ChannelInterruptFlags) GetUInt32(rawData, currentIndex++);
          spell3.ProcTriggerFlagsProp =
            (ProcTriggerFlags) GetUInt32(rawData, currentIndex++);
          spell3.ProcChance = GetUInt32(rawData, currentIndex++);
          spell3.ProcCharges = GetInt32(rawData, currentIndex++);
          spell3.MaxLevel = GetInt32(rawData, currentIndex++);
          spell3.BaseLevel = GetInt32(rawData, currentIndex++);
          spell3.Level = GetInt32(rawData, currentIndex++);
          int int32_2 = GetInt32(rawData, currentIndex++);
          if(int32_2 > 0 &&
             !mappeddbcDurationReader.Entries.TryGetValue(int32_2, out spell3.Durations))
            ContentMgr.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid Duration-Entry: {1}",
              (object) spell3.Name, (object) int32_2);
          spell3.PowerType = (PowerType) GetUInt32(rawData, currentIndex++);
          spell3.PowerCost = GetInt32(rawData, currentIndex++);
          spell3.PowerCostPerlevel = GetInt32(rawData, currentIndex++);
          spell3.PowerPerSecond = GetInt32(rawData, currentIndex++);
          spell3.PowerPerSecondPerLevel = GetInt32(rawData, currentIndex++);
          int int32_3 = GetInt32(rawData, currentIndex++);
          if(int32_3 > 0 && !mappeddbcRangeReader.Entries.TryGetValue(int32_3, out spell3.Range))
            ContentMgr.OnInvalidClientData("DBC Spell \"{0}\" referred to invalid Range-Entry: {1}",
              (object) spell3.Name, (object) int32_3);
          spell3.ProjectileSpeed = GetFloat(rawData, currentIndex++);
          spell3.ModalNextSpell = (SpellId) GetUInt32(rawData, currentIndex++);
          spell3.MaxStackCount = GetInt32(rawData, currentIndex++);
          spell3.RequiredToolIds = new uint[2];
          for(int index = 0; index < spell3.RequiredToolIds.Length; ++index)
            spell3.RequiredToolIds[index] = GetUInt32(rawData, currentIndex++);
          List<ItemStackDescription> list = null;
          int reagentStart = currentIndex;
          for(int reagentNum = 0; reagentNum < 8; ++reagentNum)
            ReadReagent(rawData, reagentStart, reagentNum, out currentIndex, ref list);
          spell3.Reagents = list == null ? ItemStackDescription.EmptyArray : list.ToArray();
          spell3.RequiredItemClass = (ItemClass) GetUInt32(rawData, currentIndex++);
          if(spell3.RequiredItemClass < ItemClass.Consumable)
            spell3.RequiredItemClass = ItemClass.None;
          spell3.RequiredItemSubClassMask =
            (ItemSubClassMask) GetUInt32(rawData, currentIndex++);
          if(spell3.RequiredItemSubClassMask < ItemSubClassMask.None)
            spell3.RequiredItemSubClassMask = ItemSubClassMask.None;
          spell3.RequiredItemInventorySlotMask =
            (InventorySlotTypeMask) GetUInt32(rawData, currentIndex++);
          if(spell3.RequiredItemInventorySlotMask < InventorySlotTypeMask.None)
            spell3.RequiredItemInventorySlotMask = InventorySlotTypeMask.None;
          List<SpellEffect> spellEffectList = new List<SpellEffect>(3);
          int effectStartIndex = currentIndex;
          for(int effectNum = 0; effectNum < 3; ++effectNum)
          {
            SpellEffect spellEffect = ReadEffect(spell3, rawData, effectStartIndex, effectNum,
              out currentIndex);
            if(spellEffect != null && (spellEffect.EffectType != SpellEffectType.None ||
                                       spellEffect.BasePoints > 0 ||
                                       (spellEffect.AuraType != AuraType.None ||
                                        spellEffect.TriggerSpellId != SpellId.None)))
              spellEffectList.Add(spellEffect);
          }

          spell3.Effects = spellEffectList.ToArray();
          spell3.Visual = GetUInt32(rawData, currentIndex++);
          spell3.Visual2 = GetUInt32(rawData, currentIndex++);
          spell3.SpellbookIconId = GetUInt32(rawData, currentIndex++);
          spell3.BuffIconId = GetUInt32(rawData, currentIndex++);
          spell3.Priority = GetUInt32(rawData, currentIndex++);
          spell3.Name = GetString(rawData, ref currentIndex);
          spell3.RankDesc = GetString(rawData, ref currentIndex);
          spell3.Description = GetString(rawData, ref currentIndex);
          spell3.BuffDescription = GetString(rawData, ref currentIndex);
          spell3.PowerCostPercentage = GetInt32(rawData, currentIndex++);
          spell3.StartRecoveryTime = GetInt32(rawData, currentIndex++);
          spell3.StartRecoveryCategory = GetInt32(rawData, currentIndex++);
          spell3.MaxTargetLevel = GetUInt32(rawData, currentIndex++);
          spell3.SpellClassSet = (SpellClassSet) GetUInt32(rawData, currentIndex++);
          spell3.SpellClassMask[0] = GetUInt32(rawData, currentIndex++);
          spell3.SpellClassMask[1] = GetUInt32(rawData, currentIndex++);
          spell3.SpellClassMask[2] = GetUInt32(rawData, currentIndex++);
          spell3.MaxTargets = GetUInt32(rawData, currentIndex++);
          spell3.DamageType = (DamageType) GetUInt32(rawData, currentIndex++);
          spell3.PreventionType = (SpellPreventionType) GetUInt32(rawData, currentIndex++);
          spell3.StanceBarOrder = GetInt32(rawData, currentIndex++);
          for(int index = 0; index < spell3.DamageMultipliers.Length; ++index)
            spell3.DamageMultipliers[index] = GetFloat(rawData, currentIndex++);
          spell3.MinFactionId = GetUInt32(rawData, currentIndex++);
          spell3.MinReputation = GetUInt32(rawData, currentIndex++);
          spell3.RequiredAuraVision = GetUInt32(rawData, currentIndex++);
          spell3.RequiredToolCategories = new ToolCategory[2];
          for(int index = 0; index < spell3.RequiredToolCategories.Length; ++index)
            spell3.RequiredToolCategories[index] =
              (ToolCategory) GetUInt32(rawData, currentIndex++);
          spell3.AreaGroupId = GetUInt32(rawData, currentIndex++);
          spell3.SchoolMask = (DamageSchoolMask) GetUInt32(rawData, currentIndex++);
          int int32_4 = GetInt32(rawData, currentIndex++);
          if(int32_4 != 0)
            mappeddbcRuneCostReader.Entries.TryGetValue(int32_4, out spell3.RuneCostEntry);
          spell3.MissileId = GetUInt32(rawData, currentIndex++);
          spell3.PowerDisplayId = GetInt32(rawData, currentIndex++);
          spell3.Unk_322_4_1 = GetUInt32(rawData, currentIndex++);
          spell3.Unk_322_4_2 = GetUInt32(rawData, currentIndex++);
          spell3.Unk_322_4_3 = GetUInt32(rawData, currentIndex++);
          spell3.spellDescriptionVariablesID = GetUInt32(rawData, currentIndex++);
        }
        catch(Exception ex)
        {
          throw new Exception(
            string.Format("Unable to parse Spell from DBC file. Index: " + currentIndex), ex);
        }

        SpellHandler.AddSpell(spell3);
      }

      private void ReadReagent(byte[] rawData, int reagentStart, int reagentNum, out int currentIndex,
        ref List<ItemStackDescription> list)
      {
        currentIndex = reagentStart + reagentNum;
        Asda2ItemId uint32 = (Asda2ItemId) GetUInt32(rawData, currentIndex);
        currentIndex += 8;
        int int32 = GetInt32(rawData, currentIndex);
        currentIndex += 8 - reagentNum;
        if(uint32 <= 0 || int32 <= 0)
          return;
        if(list == null)
          list = new List<ItemStackDescription>();
        ItemStackDescription stackDescription = new ItemStackDescription
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
        spellEffect.EffectType = (SpellEffectType) GetUInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.DiceSides = GetInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.RealPointsPerLevel = GetFloat(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.BasePoints = GetInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.Mechanic = (SpellMechanic) GetUInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.ImplicitTargetA =
          (ImplicitSpellTargetType) GetUInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.ImplicitTargetB =
          (ImplicitSpellTargetType) GetUInt32(rawData, currentIndex);
        currentIndex += 3;
        if(spellEffect.ImplicitTargetA == ImplicitSpellTargetType.AllEnemiesAroundCaster &&
           spellEffect.ImplicitTargetB == ImplicitSpellTargetType.AllEnemiesInArea)
          spellEffect.ImplicitTargetB = ImplicitSpellTargetType.None;
        int int32 = GetInt32(rawData, currentIndex);
        if(int32 > 0)
          mappeddbcRadiusReader.Entries.TryGetValue(int32, out spellEffect.Radius);
        currentIndex += 3;
        spellEffect.AuraType = (AuraType) GetUInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.Amplitude = GetInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.ProcValue = GetFloat(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.ChainTargets = GetInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.ItemId = GetUInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.MiscValue = GetInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.MiscValueB = GetInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.TriggerSpellId = (SpellId) GetUInt32(rawData, currentIndex);
        currentIndex += 3;
        spellEffect.PointsPerComboPoint = GetFloat(rawData, currentIndex);
        currentIndex += 3 - effectNum;
        currentIndex += effectNum * 3;
        spellEffect.AffectMask[0] = GetUInt32(rawData, currentIndex++);
        spellEffect.AffectMask[1] = GetUInt32(rawData, currentIndex++);
        spellEffect.AffectMask[2] = GetUInt32(rawData, currentIndex++);
        currentIndex += (2 - effectNum) * 3;
        return spellEffect;
      }
    }
  }
}