using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Chat;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.Core.Paths;
using WCell.Core.Timers;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.Paths;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;
using WCell.RealmServer.Talents;
using WCell.RealmServer.Taxi;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.NLog;
using WCell.Util.Threading;
using WCell.Util.Variables;

namespace WCell.RealmServer.Entities
{
  /// <summary>
  /// TODO: Move everything Unit-related from UnitUpdates in here
  /// </summary>
  /// <summary>
  /// Base class for Players and NPCs (also Totems and similar).
  /// 
  /// 
  /// </summary>
  /// 
  ///             All damage-related UnitFields are to be found in this file
  ///             <summary>
  /// 
  /// </summary>
  public abstract class Unit : WorldObject, ILivingEntity, ISummoner, INamedEntity, IEntity, INamed, IWorldZoneLocation,
    IWorldLocation, IHasPosition
  {
    /// <summary>Amount of mana to be added per point of Intelligence</summary>
    public static int ManaPerIntelligence = 15;

    /// <summary>Amount of heatlh to be added per point of Stamina</summary>
    public static int HealthPerStamina = 10;

    /// <summary>Amount of armor to be added per point of Agility</summary>
    public static int ArmorPerAgility = 2;

    protected static Logger log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Time in milliseconds between last move and when one officially stands still
    /// </summary>
    public static uint MinStandStillDelay = 400;

    public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Unit);

    /// <summary>Used to determine melee distance</summary>
    public static float DefaultMeleeAttackRange = 1f;

    /// <summary>Used to determine ranged attack distance</summary>
    public static float DefaultRangedAttackRange = 20f;

    /// <summary>Time in milliseconds until a Player can leave combat</summary>
    public static int PvPDeactivationDelay = 6000;

    [NotVariable]public static float DefaultSpeedFactor = 0.7f;
    public static float DefaultWalkSpeed = 2.5f;
    public static float DefaultWalkBackSpeed = 2.5f;
    public static float DefaultRunSpeed = 0.37f;
    public static float DefaultSwimSpeed = 4.7222f;
    public static float DefaultSwimBackSpeed = 4.5f;
    public static float DefaultFlightSpeed = 7f;
    public static float DefaultFlightBackSpeed = 4.5f;
    [NotVariable]public static readonly float DefaultTurnSpeed = 3.141593f;
    [NotVariable]public static readonly float DefaulPitchSpeed = 3.141593f;

    public static readonly int MechanicCount =
      (int) Convert.ChangeType(Utility.GetMaxEnum<SpellMechanic>(), typeof(int)) + 1;

    public static readonly int DamageSchoolCount = 7;

    public static readonly int DispelTypeCount =
      (int) Convert.ChangeType(Utility.GetMaxEnum<DispelType>(), typeof(int)) + 1;

    /// <summary>All CombatRatings</summary>
    public static readonly CombatRating[] CombatRatings = (CombatRating[]) Enum.GetValues(typeof(CombatRating));

    protected internal int[] m_baseStats = new int[5];
    protected int[] m_baseResistances = new int[DamageSchoolCount];
    internal readonly int[] IntMods = new int[UnitUpdates.FlatIntModCount + 1];
    internal readonly float[] FloatMods = new float[UnitUpdates.MultiplierModCount + 1];
    private float _asda2DropChance = 1f;
    private float _asda2GoldAmountBoost = 1f;
    private float _asda2ExpAmountBoost = 1f;
    private bool _isVisible = true;

    /// <summary>
    /// A list of the TaxiPaths the unit is currently travelling to. The TaxiPath currently being travelled is first.
    /// </summary>
    protected Queue<TaxiPath> m_TaxiPaths = new Queue<TaxiPath>(5);

    /// <summary>Applies modifications to your attacks</summary>
    public readonly List<IAttackEventHandler> AttackEventHandlers = new List<IAttackEventHandler>(1);

    [NotVariable]private const int DefaultHealthAndPowerUpdateTime = 5000;
    private float _asda2Defence;
    private float _asda2MagicDefence;
    protected UnitModelInfo m_model;
    protected Unit m_target;
    protected Unit m_charm;
    protected WorldObject m_channeled;
    protected Transport m_transport;
    protected Vector3 m_transportPosition;
    protected float m_transportOrientation;
    protected uint m_transportTime;
    private int m_maxHealthModFlat;
    [NotVariable]private static int _timeFromLastMpUpdate;
    private float _tempPower;
    private bool _isSitting;
    private int _power;
    protected IBrain m_brain;
    protected Faction m_faction;
    protected SpellCollection m_spells;
    protected AuraCollection m_auras;
    protected int m_comboPoints;
    protected Unit m_comboTarget;
    protected ulong m_auraUpdateMask;

    /// <summary>
    /// Indicates whether regeneration of Health and Power is currently activated
    /// </summary>
    protected bool m_regenerates;

    /// <summary>Flat, school-specific PowerCostMods</summary>
    protected int[] m_schoolPowerCostMods;

    /// <summary>
    /// The time of when this Unit last moved (used for speedhack check)
    /// </summary>
    protected internal int m_lastMoveTime;

    protected Unit m_FirstAttacker;
    private Unit m_LastKiller;
    protected bool m_IsPinnedDown;
    protected internal TimerEntry m_TaxiMovementTimer;
    internal int taxiTime;

    /// <summary>The currently occupied VehicleSeat (if any)</summary>
    protected internal VehicleSeat m_vehicleSeat;

    private uint m_DeathPrevention;
    private int m_PowerRegenPerTick;
    private int _manaRegenPerTickInterrupted;
    private float _tempHealthRegen;
    private int _timeFromLastHealthUpdate;
    private LinkedListNode<PathVertex> m_LatestTaxiPathNode;
    protected internal List<IProcHandler> m_procHandlers;
    protected GossipMenu m_gossipMenu;
    public float SplinterEffect;
    public int SplinterEffectChange;

    /// <summary>Times our melee and ranged attacks</summary>
    protected TimerEntry m_attackTimer;

    /// <summary>
    /// whether this Unit is currently in Combat-mode (effects regeneration etc).
    /// </summary>
    protected bool m_isInCombat;

    /// <summary>whether this Unit is currently actively fighting.</summary>
    protected bool m_isFighting;

    protected DateTime m_lastCombatTime;
    protected DamageAction m_DamageAction;
    protected int m_extraAttacks;

    /// <summary>The Environment.TickCount of your last strikes</summary>
    protected int m_lastStrike;

    /// <summary>The Environment.TickCount of your last strikes</summary>
    protected int m_lastOffhandStrike;

    protected Spell m_AutorepeatSpell;
    protected IAsda2Weapon m_mainWeapon;
    protected IAsda2Weapon m_offhandWeapon;
    protected IAsda2Weapon m_RangedWeapon;
    private InventorySlotTypeMask m_DisarmMask;
    public int LastDamageDelay;
    protected uint m_flying;
    protected uint m_waterWalk;
    protected uint m_hovering;
    protected uint m_featherFalling;
    protected int m_stealthed;
    private int m_Pacified;
    protected int[] m_mechanics;
    protected int[] m_mechanicImmunities;
    protected int[] m_mechanicResistances;
    protected int[] m_mechanicDurationMods;
    protected int[] m_debuffResistances;
    protected int[] m_dispelImmunities;
    protected int[] m_TargetResMods;
    protected int[] m_spellInterruptProt;
    protected int[] m_threatMods;
    protected int[] m_attackerSpellHitChance;
    protected int[] m_SpellHitChance;
    protected int[] m_CritMods;
    protected int[] m_damageTakenMods;
    protected int[] m_damageTakenPctMods;

    /// <summary>Immunities against damage-schools</summary>
    protected int[] m_dmgImmunities;

    /// <summary>
    /// Whether the physical state of this Unit allows it to move
    /// </summary>
    protected bool m_canMove;

    protected bool m_canInteract;
    protected bool m_canHarm;
    protected bool m_canCastSpells;
    protected bool m_evades;
    protected bool m_canDoPhysicalActivity;
    protected float m_speedFactor;
    protected float m_swimFactor;
    protected float m_flightFactor;
    protected float m_mountMod;
    protected float m_walkSpeed;
    protected float m_walkBackSpeed;
    protected float m_runSpeed;
    protected float m_swimSpeed;
    protected float m_swimBackSpeed;
    protected float m_flightSpeed;
    protected float m_flightBackSpeed;
    protected float m_turnSpeed;
    protected float m_pitchSpeed;
    private MovementFlags m_movementFlags;
    private MovementFlags2 m_movementFlags2;
    protected Movement m_Movement;

    protected internal virtual void UpdateStrength()
    {
      SetInt32(UnitFields.STAT0, GetBaseStatValue(StatType.Strength) + StrengthBuffPositive + StrengthBuffNegative);
      this.UpdateBlockChance();
      this.UpdateAllAttackPower();
    }

    protected internal virtual void UpdateStamina()
    {
      SetInt32(UnitFields.STAT2, GetBaseStatValue(StatType.Stamina) + StaminaBuffPositive + StaminaBuffNegative);
      UpdateMaxHealth();
    }

    internal void UpdateAgility()
    {
      int agility = Agility;
      int num = GetBaseStatValue(StatType.Agility) + AgilityBuffPositive + AgilityBuffNegative;
      SetInt32(UnitFields.STAT1, num);
      ModBaseResistance(DamageSchool.Physical, (num - agility) * ArmorPerAgility);
      this.UpdateDodgeChance();
      this.UpdateCritChance();
      this.UpdateAllAttackPower();
    }

    protected internal virtual void UpdateIntellect()
    {
      SetInt32(UnitFields.STAT3, GetBaseStatValue(StatType.Intellect) + IntellectBuffPositive + IntellectBuffNegative);
      UpdateMaxPower();
    }

    protected internal virtual void UpdateSpirit()
    {
      SetInt32(UnitFields.STAT4, GetBaseStatValue(StatType.Spirit) + SpiritBuffPositive + SpiritBuffNegative);
      this.UpdateNormalHealthRegen();
      if(Intellect == 0)
        return;
      this.UpdatePowerRegen();
    }

    protected internal virtual void UpdateStat(StatType stat)
    {
      switch(stat)
      {
        case StatType.Strength:
          UpdateStrength();
          break;
        case StatType.Agility:
          UpdateAgility();
          break;
        case StatType.Stamina:
          UpdateStamina();
          break;
        case StatType.Intellect:
          UpdateIntellect();
          break;
        case StatType.Spirit:
          UpdateSpirit();
          break;
      }
    }

    protected internal virtual void UpdateMaxHealth()
    {
      MaxHealth = (int) UnitUpdates.GetMultiMod(FloatMods[31],
        IntMods[31] + BaseHealth + CharacterFormulas.CalculateHealthBonus(Level, Asda2Strength, Asda2Stamina, Class));
      this.UpdateHealthRegen();
    }

    /// <summary>Amount of mana, contributed by intellect</summary>
    protected internal virtual int IntellectManaBonus
    {
      get { return Intellect; }
    }

    public float Asda2Defence
    {
      get { return _asda2Defence; }
      set { _asda2Defence = value; }
    }

    public float Asda2MagicDefence
    {
      get { return _asda2MagicDefence; }
      set { _asda2MagicDefence = value; }
    }

    protected int CritDamageBonusPrc { get; set; }

    protected internal void UpdateMaxPower()
    {
      int num1 = BasePower + IntMods[1] + CharacterFormulas.CalculateManaBonus(Level, Class, Asda2Spirit);
      int num2 = num1 + (num1 * IntMods[2] + 50) / 100;
      if(num2 < 0)
        num2 = 0;
      MaxPower = num2;
      this.UpdatePowerRegen();
    }

    public void UpdateAsda2Defence()
    {
      Asda2Defence = UnitUpdates.GetMultiMod(FloatMods[4],
        IntMods[20] + CharacterFormulas.ClaculateDefenceBonus(Level, Class, Asda2Agility));
    }

    public void UpdateAsda2MagicDefence()
    {
      Asda2MagicDefence = UnitUpdates.GetMultiMod(FloatMods[5],
        IntMods[21] + CharacterFormulas.CalculateMagicDefencePointsBonus(Level, Class, Asda2Spirit));
    }

    public void UpdateAsda2DropChance()
    {
      Asda2DropChance =
        UnitUpdates.GetMultiMod(FloatMods[6] + CharacterFormulas.CalculateDropChanceBoost(Asda2Luck), 1f);
    }

    public void UpdateAsda2GoldAmount()
    {
      Asda2GoldAmountBoost =
        UnitUpdates.GetMultiMod(FloatMods[7] + CharacterFormulas.CalculateGoldAmountDropBoost(Level, Class, Asda2Luck),
          1f);
    }

    public void UpdateAsda2ExpAmount()
    {
      Asda2ExpAmountBoost = UnitUpdates.GetMultiMod(FloatMods[8], 1f);
    }

    public void UpdateAsda2Luck()
    {
      Asda2Luck = UnitUpdates.GetMultiMod(FloatMods[9], IntMods[22] + Asda2BaseLuck);
      this.UpdateCritChance();
      UpdateAsda2DropChance();
      UpdateAsda2GoldAmount();
      UpdateCritDamageBonus();
    }

    public void UpdateAsda2Spirit()
    {
      Asda2Spirit = UnitUpdates.GetMultiMod(FloatMods[16], IntMods[29] + Asda2BaseSpirit);
      UpdateAsda2MagicDefence();
      UpdateMaxPower();
      this.UpdatePowerRegen();
    }

    public void UpdateAsda2Intellect()
    {
      Asda2Intellect = UnitUpdates.GetMultiMod(FloatMods[15], IntMods[28] + Asda2BaseIntellect);
      this.UpdateMainDamage();
      UpdateCritDamageBonus();
    }

    public void UpdateAsda2Stamina()
    {
      Asda2Stamina = UnitUpdates.GetMultiMod(FloatMods[14], IntMods[30] + Asda2BaseStamina);
      UpdateMaxHealth();
    }

    public void UpdateAsda2Strength()
    {
      Asda2Strength = UnitUpdates.GetMultiMod(FloatMods[12], IntMods[27] + Asda2BaseStrength);
      this.UpdateMainDamage();
      UpdateCritDamageBonus();
      UpdateMaxHealth();
    }

    public void UpdateAsda2Agility()
    {
      Asda2Agility = UnitUpdates.GetMultiMod(FloatMods[13], IntMods[26] + Asda2BaseAgility);
      this.UpdateCritChance();
      this.UpdateAllAttackTimes();
      this.UpdateDodgeChance();
      UpdateSpeedFactor();
      UpdateAsda2Defence();
      UpdateCritDamageBonus();
      this.UpdateMainDamage();
    }

    public void UpdateLightResistence()
    {
      Asda2LightResistence = IntMods[20];
    }

    public void UpdateDarkResistence()
    {
      Asda2DarkResistence = IntMods[19];
    }

    public void UpdateEarthResistence()
    {
      Asda2EarthResistence = IntMods[18];
    }

    public void UpdateFireResistence()
    {
      Asda2FireResistence = IntMods[17];
    }

    public void UpdateClimateResistence()
    {
      Asda2ClimateResistence = IntMods[21];
    }

    public void UpdateWaterResistence()
    {
      Asda2WaterResistence = IntMods[22];
    }

    public void UpdateLightAttribute()
    {
      Asda2LightAttribute = IntMods[23];
    }

    public void UpdateDarkAttribute()
    {
      Asda2DarkAttribute = IntMods[24];
    }

    public void UpdateEarthAttribute()
    {
      Asda2EarthAttribute = IntMods[25];
    }

    public void UpdateFireAttribute()
    {
      Asda2FireAttribute = IntMods[26];
    }

    public void UpdateClimateAttribute()
    {
      Asda2ClimateAttribute = IntMods[27];
    }

    public void UpdateWaterAttribute()
    {
      Asda2WaterAttribute = IntMods[28];
    }

    public void UpdateSpeedFactor()
    {
      float num = CharacterFormulas.CalcSpeedBonus(Level, Class, Asda2Agility);
      if(num > 1.0)
        num = 1f;
      SpeedFactor = UnitUpdates.GetMultiMod(FloatMods[29] + num, DefaultSpeedFactor);
      Character character = this as Character;
      if(character == null)
        return;
      GlobalHandler.SendSpeedChangedResponse(character.Client);
    }

    public void UpdateCritDamageBonus()
    {
      CritDamageBonusPrc =
        CharacterFormulas.CalculateCriticalDamageBonus(Level, Class, Asda2Agility, Asda2Luck, Asda2Intellect,
          Asda2Strength);
    }

    public Unit Charm
    {
      get { return m_charm; }
      set
      {
        m_charm = value;
        if(value != null)
          SetEntityId(UnitFields.CHARM, value.EntityId);
        else
          SetEntityId(UnitFields.CHARM, EntityId.Zero);
      }
    }

    public Unit Charmer
    {
      get { return m_master; }
      set
      {
        SetEntityId(UnitFields.CHARMEDBY, value != null ? value.EntityId : EntityId.Zero);
        Master = value;
      }
    }

    public Character CharmerCharacter
    {
      get { return m_master as Character; }
      set { Charmer = value; }
    }

    public bool IsCharmed
    {
      get { return m_master != null; }
    }

    public Unit Summoner
    {
      get { return m_master; }
      set
      {
        SetEntityId(UnitFields.SUMMONEDBY, value != null ? value.EntityId : EntityId.Zero);
        Master = value;
      }
    }

    public EntityId Creator
    {
      get { return GetEntityId(UnitFields.CREATEDBY); }
      set { SetEntityId(UnitFields.CREATEDBY, value); }
    }

    public EntityId Summon
    {
      get { return GetEntityId(UnitFields.SUMMON); }
      set { SetEntityId(UnitFields.SUMMON, value); }
    }

    /// <summary>
    /// The Unit's currently selected target.
    /// If set to null, also forces this Unit to leave combat mode.
    /// </summary>
    public Unit Target
    {
      get
      {
        if(m_target != null && !m_target.IsInWorld)
          Target = null;
        return m_target;
      }
      set
      {
        if(m_target == value)
          return;
        if(value != null)
        {
          SetEntityId(UnitFields.TARGET, value.EntityId);
          if(this is NPC)
            Orientation = GetAngleTowards(value);
          else if(this is Character && value is Character && this != value)
            (value as Character).TargetersOnMe.Add((Character) this);
        }
        else
        {
          SetEntityId(UnitFields.TARGET, EntityId.Zero);
          IsFighting = false;
          if(this is Character && m_target is Character && this != value)
            (m_target as Character).TargetersOnMe.Remove((Character) this);
        }

        m_target = value;
        CancelPendingAbility();
      }
    }

    /// <summary>As long as this count is up, cannot leave combat</summary>
    public int NPCAttackerCount { get; internal set; }

    public WorldObject ChannelObject
    {
      get { return m_channeled; }
      set
      {
        SetEntityId(UnitFields.CHANNEL_OBJECT, value != null ? value.EntityId : EntityId.Zero);
        m_channeled = value;
      }
    }

    public ITransportInfo TransportInfo
    {
      get
      {
        if(m_vehicleSeat == null)
          return m_transport;
        return m_vehicleSeat.Vehicle;
      }
    }

    /// <summary>
    /// The <see cref="P:WCell.RealmServer.Entities.Unit.Transport" /> that this Unit is on (if any).
    /// </summary>
    public Transport Transport
    {
      get { return m_transport; }
      internal set { m_transport = value; }
    }

    public Vector3 TransportPosition
    {
      get { return m_transportPosition; }
      internal set { m_transportPosition = value; }
    }

    public float TransportOrientation
    {
      get { return m_transportOrientation; }
      internal set { m_transportOrientation = value; }
    }

    public uint TransportTime
    {
      get { return Utility.GetSystemTime() - m_transportTime; }
      internal set { m_transportTime = value; }
    }

    public byte TransportSeat
    {
      get
      {
        if(VehicleSeat == null)
          return 0;
        return VehicleSeat.Index;
      }
    }

    /// <summary>Currently occupied VehicleSeat (if riding in vehicle)</summary>
    public VehicleSeat VehicleSeat
    {
      get { return m_vehicleSeat; }
    }

    public Vehicle Vehicle
    {
      get
      {
        if(m_vehicleSeat == null)
          return null;
        return m_vehicleSeat.Vehicle;
      }
    }

    public virtual int MaxLevel
    {
      get { return int.MaxValue; }
      internal set { }
    }

    /// <summary>The Level of this Unit.</summary>
    public virtual int Level
    {
      get { return GetInt32(UnitFields.LEVEL); }
      set
      {
        SetInt32(UnitFields.LEVEL, value);
        OnLevelChanged();
      }
    }

    protected virtual void OnLevelChanged()
    {
    }

    public override int CasterLevel
    {
      get { return Level; }
    }

    public override Faction Faction
    {
      get { return m_faction; }
      set
      {
        if(value == null)
          throw new NullReferenceException(string.Format("Faction cannot be set to null (Unit: {0}, Map: {1})", this,
            m_Map));
        m_faction = value;
        SetUInt32(UnitFields.FACTIONTEMPLATE, value.Template.Id);
      }
    }

    public abstract Faction DefaultFaction { get; }

    public override FactionId FactionId
    {
      get { return m_faction.Id; }
      set
      {
        Faction faction = FactionMgr.Get(value);
        if(faction == null)
          return;
        Faction = faction;
      }
    }

    public FactionGroup FactionGroup
    {
      get { return m_faction.Group; }
    }

    public uint FactionTemplateId
    {
      get { return m_faction.Template.Id; }
    }

    public UnitFlags UnitFlags
    {
      get { return (UnitFlags) GetUInt32(UnitFields.FLAGS); }
      set { SetUInt32(UnitFields.FLAGS, (uint) value); }
    }

    public UnitFlags2 UnitFlags2
    {
      get { return (UnitFlags2) GetUInt32(UnitFields.FLAGS_2); }
      set { SetUInt32(UnitFields.FLAGS_2, (uint) value); }
    }

    public float BoundingRadius
    {
      get { return GetFloat(UnitFields.BOUNDINGRADIUS); }
      set { SetFloat(UnitFields.BOUNDINGRADIUS, value); }
    }

    public float BoundingCollisionRadius { get; set; }

    public UnitModelInfo Model
    {
      get { return m_model; }
      set
      {
        m_model = value;
        SetUInt32(UnitFields.DISPLAYID, m_model.DisplayId);
        BoundingRadius = m_model.BoundingRadius * ScaleX;
        BoundingCollisionRadius = BoundingRadius * 2.1f;
        CombatReach = m_model.CombatReach * ScaleX;
      }
    }

    public virtual uint DisplayId
    {
      get { return GetUInt32(UnitFields.DISPLAYID); }
      set
      {
        UnitModelInfo modelInfo = UnitMgr.GetModelInfo(value);
        if(modelInfo == null)
          log.Error("Trying to set DisplayId of {0} to an invalid value: {1}", this, value);
        else
          Model = modelInfo;
      }
    }

    public uint NativeDisplayId
    {
      get { return GetUInt32(UnitFields.NATIVEDISPLAYID); }
      set { SetUInt32(UnitFields.NATIVEDISPLAYID, value); }
    }

    public uint MountDisplayId
    {
      get { return GetUInt32(UnitFields.MOUNTDISPLAYID); }
      set { SetUInt32(UnitFields.MOUNTDISPLAYID, value); }
    }

    public Asda2ItemId VirtualItem1
    {
      get { return (Asda2ItemId) GetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID); }
      set { SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID, (uint) value); }
    }

    public Asda2ItemId VirtualItem2
    {
      get { return (Asda2ItemId) GetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_2); }
      set { SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_2, (uint) value); }
    }

    public Asda2ItemId VirtualItem3
    {
      get { return (Asda2ItemId) GetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_3); }
      set { SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_3, (uint) value); }
    }

    public uint PetNumber
    {
      get { return GetUInt32(UnitFields.PETNUMBER); }
      set { SetUInt32(UnitFields.PETNUMBER, value); }
    }

    /// <summary>Changing this makes clients send a pet name query</summary>
    public uint PetNameTimestamp
    {
      get { return GetUInt32(UnitFields.PET_NAME_TIMESTAMP); }
      set { SetUInt32(UnitFields.PET_NAME_TIMESTAMP, value); }
    }

    public int PetExperience
    {
      get { return GetInt32(UnitFields.PETEXPERIENCE); }
      set { SetInt32(UnitFields.PETEXPERIENCE, value); }
    }

    /// <summary>
    /// 
    /// </summary>
    public int NextPetLevelExperience
    {
      get { return GetInt32(UnitFields.PETNEXTLEVELEXP); }
      set { SetInt32(UnitFields.PETNEXTLEVELEXP, value); }
    }

    public UnitDynamicFlags DynamicFlags
    {
      get { return (UnitDynamicFlags) GetUInt32(UnitFields.DYNAMIC_FLAGS); }
      set { SetUInt32(UnitFields.DYNAMIC_FLAGS, (uint) value); }
    }

    public SpellId ChannelSpell
    {
      get { return (SpellId) GetUInt32(UnitFields.CHANNEL_SPELL); }
      set { SetUInt32(UnitFields.CHANNEL_SPELL, (uint) value); }
    }

    public float CastSpeedFactor
    {
      get { return GetFloat(UnitFields.MOD_CAST_SPEED); }
      set { SetFloat(UnitFields.MOD_CAST_SPEED, value); }
    }

    /// <summary>Whether this Unit is summoned</summary>
    public bool IsSummoned
    {
      get { return CreationSpellId != SpellId.None; }
    }

    /// <summary>Whether this Unit belongs to someone</summary>
    public bool IsMinion
    {
      get { return m_master != this; }
    }

    /// <summary>The spell that created this Unit</summary>
    public SpellId CreationSpellId
    {
      get { return (SpellId) GetUInt32(UnitFields.CREATED_BY_SPELL); }
      set { SetUInt32(UnitFields.CREATED_BY_SPELL, (uint) value); }
    }

    public NPCFlags NPCFlags
    {
      get { return (NPCFlags) GetUInt32(UnitFields.NPC_FLAGS); }
      set
      {
        SetUInt32(UnitFields.NPC_FLAGS, (uint) value);
        MarkUpdate(UnitFields.DYNAMIC_FLAGS);
      }
    }

    public EmoteType EmoteState
    {
      get { return (EmoteType) GetUInt32(UnitFields.NPC_EMOTESTATE); }
      set { SetUInt32(UnitFields.NPC_EMOTESTATE, (uint) value); }
    }

    public float HoverHeight
    {
      get { return GetFloat(UnitFields.HOVERHEIGHT); }
      set { SetFloat(UnitFields.HOVERHEIGHT, value); }
    }

    /// <summary>Pet's Training Points, deprecated</summary>
    public uint TrainingPoints { get; set; }

    public int Strength
    {
      get { return GetInt32(UnitFields.STAT0); }
    }

    public int Agility
    {
      get { return GetInt32(UnitFields.STAT1); }
    }

    public int Stamina
    {
      get { return GetInt32(UnitFields.STAT2); }
    }

    /// <summary>
    /// The amount of stamina that does not contribute to health.
    /// </summary>
    public virtual int StaminaWithoutHealthContribution
    {
      get { return 20; }
    }

    public int Intellect
    {
      get { return GetInt32(UnitFields.STAT3); }
    }

    public int Spirit
    {
      get { return GetInt32(UnitFields.STAT4); }
    }

    internal int[] BaseStats
    {
      get { return m_baseStats; }
    }

    /// <summary>Stat value, after modifiers</summary>
    public int GetTotalStatValue(StatType stat)
    {
      return GetInt32((UnitFields) (84 + stat));
    }

    public int GetBaseStatValue(StatType stat)
    {
      return m_baseStats[(int) stat];
    }

    public virtual int GetUnmodifiedBaseStatValue(StatType stat)
    {
      return m_baseStats[(int) stat];
    }

    public void SetBaseStat(StatType stat, int value)
    {
      SetBaseStat(stat, value, true);
    }

    public void SetBaseStat(StatType stat, int value, bool update)
    {
      m_baseStats[(int) stat] = value;
      if(!update)
        return;
      UpdateStat(stat);
    }

    public void ModBaseStat(StatType stat, int delta)
    {
      SetBaseStat(stat, m_baseStats[(int) stat] + delta);
    }

    public void AddStatMod(StatType stat, int delta, bool passive)
    {
      if(passive)
        ModBaseStat(stat, delta);
      else
        AddStatMod(stat, delta);
    }

    public void AddStatMod(StatType stat, int delta)
    {
      if(delta == 0)
        return;
      UnitFields unitFields = delta <= 0 ? UnitFields.NEGSTAT0 : UnitFields.POSSTAT0;
      SetInt32(unitFields + (int) stat, GetInt32(unitFields + (int) stat) + delta);
      UpdateStat(stat);
    }

    public void RemoveStatMod(StatType stat, int delta, bool passive)
    {
      if(passive)
        ModBaseStat(stat, -delta);
      else
        RemoveStatMod(stat, delta);
    }

    /// <summary>
    /// Removes the given delta from positive or negative stat buffs correspondingly
    /// </summary>
    public void RemoveStatMod(StatType stat, int delta)
    {
      if(delta == 0)
        return;
      UnitFields unitFields = delta <= 0 ? UnitFields.NEGSTAT0 : UnitFields.POSSTAT0;
      SetInt32(unitFields + (int) stat, GetInt32(unitFields + (int) stat) - delta);
      UpdateStat(stat);
    }

    public int StrengthBuffPositive
    {
      get { return GetInt32(UnitFields.POSSTAT0); }
      set
      {
        SetInt32(UnitFields.POSSTAT0, value);
        UpdateStrength();
      }
    }

    public int AgilityBuffPositive
    {
      get { return GetInt32(UnitFields.POSSTAT1); }
      set
      {
        SetInt32(UnitFields.POSSTAT1, value);
        UpdateAgility();
      }
    }

    public int StaminaBuffPositive
    {
      get { return GetInt32(UnitFields.POSSTAT2); }
      set
      {
        SetInt32(UnitFields.POSSTAT2, value);
        UpdateStamina();
      }
    }

    public int IntellectBuffPositive
    {
      get { return GetInt32(UnitFields.POSSTAT3); }
      set
      {
        SetInt32(UnitFields.POSSTAT3, value);
        UpdateIntellect();
      }
    }

    public int SpiritBuffPositive
    {
      get { return GetInt32(UnitFields.POSSTAT4); }
      set
      {
        SetInt32(UnitFields.POSSTAT4, value);
        UpdateSpirit();
      }
    }

    public int StrengthBuffNegative
    {
      get { return GetInt32(UnitFields.NEGSTAT0); }
      set
      {
        SetInt32(UnitFields.NEGSTAT0, value);
        UpdateStrength();
      }
    }

    public int AgilityBuffNegative
    {
      get { return GetInt32(UnitFields.NEGSTAT1); }
      set
      {
        SetInt32(UnitFields.NEGSTAT1, value);
        UpdateAgility();
      }
    }

    public int StaminaBuffNegative
    {
      get { return GetInt32(UnitFields.NEGSTAT2); }
      set
      {
        SetInt32(UnitFields.NEGSTAT2, value);
        UpdateStamina();
      }
    }

    public int IntellectBuffNegative
    {
      get { return GetInt32(UnitFields.NEGSTAT3); }
      set
      {
        SetInt32(UnitFields.NEGSTAT3, value);
        UpdateIntellect();
      }
    }

    public int SpiritBuffNegative
    {
      get { return GetInt32(UnitFields.NEGSTAT4); }
      set
      {
        SetInt32(UnitFields.NEGSTAT4, value);
        UpdateSpirit();
      }
    }

    /// <summary>Physical resist</summary>
    public int Armor
    {
      get { return GetInt32(UnitFields.RESISTANCES); }
      internal set { SetInt32(UnitFields.RESISTANCES, value); }
    }

    public int HolyResist
    {
      get { return GetInt32(UnitFields.RESISTANCES_2); }
      internal set { SetInt32(UnitFields.RESISTANCES_2, value); }
    }

    public int FireResist
    {
      get { return GetInt32(UnitFields.RESISTANCES_3); }
      internal set { SetInt32(UnitFields.RESISTANCES_3, value); }
    }

    public int NatureResist
    {
      get { return GetInt32(UnitFields.RESISTANCES_4); }
      internal set { SetInt32(UnitFields.RESISTANCES_4, value); }
    }

    public int FrostResist
    {
      get { return GetInt32(UnitFields.RESISTANCES_5); }
      internal set { SetInt32(UnitFields.RESISTANCES_5, value); }
    }

    public int ShadowResist
    {
      get { return GetInt32(UnitFields.RESISTANCES_6); }
      internal set { SetInt32(UnitFields.RESISTANCES_6, value); }
    }

    public int ArcaneResist
    {
      get { return GetInt32(UnitFields.RESISTANCES_7); }
      internal set { SetInt32(UnitFields.RESISTANCES_7, value); }
    }

    internal int[] BaseResistances
    {
      get { return m_baseResistances; }
    }

    /// <summary>
    /// Returns the total resistance-value of the given school
    /// </summary>
    public int GetResistance(DamageSchool school)
    {
      int num = GetBaseResistance(school) + GetInt32((UnitFields) (106U + school)) +
                GetInt32((UnitFields) (113U + school));
      if(num < 0)
        num = 0;
      return num;
    }

    /// <summary>Returns the base resistance-value of the given school</summary>
    public int GetBaseResistance(DamageSchool school)
    {
      return m_baseResistances[(int) school];
    }

    public void SetBaseResistance(DamageSchool school, int value)
    {
      if(value < 0)
        value = 0;
      m_baseResistances[(uint) school] = value;
      OnResistanceChanged(school);
    }

    /// <summary>
    /// Adds the given amount to the base of the given resistance for the given school
    /// </summary>
    public void ModBaseResistance(DamageSchool school, int delta)
    {
      SetBaseResistance(school, m_baseResistances[(int) school] + delta);
    }

    /// <summary>
    /// Adds the given amount to the base of all given resistance-schools
    /// </summary>
    public void ModBaseResistance(uint[] schools, int delta)
    {
      foreach(DamageSchool school in schools)
        ModBaseResistance(school, delta);
    }

    public void AddResistanceBuff(DamageSchool school, int delta)
    {
      if(delta == 0)
        return;
      UnitFields unitFields =
        delta <= 0 ? UnitFields.RESISTANCEBUFFMODSNEGATIVE : UnitFields.RESISTANCEBUFFMODSPOSITIVE;
      SetInt32((UnitFields) ((int) unitFields + (int) school),
        GetInt32((UnitFields) ((int) unitFields + (int) school)) + delta);
      OnResistanceChanged(school);
    }

    /// <summary>
    /// Removes the given delta from positive or negative stat buffs correspondingly
    /// </summary>
    public void RemoveResistanceBuff(DamageSchool school, int delta)
    {
      if(delta == 0)
        return;
      UnitFields unitFields =
        delta <= 0 ? UnitFields.RESISTANCEBUFFMODSNEGATIVE : UnitFields.RESISTANCEBUFFMODSPOSITIVE;
      SetInt32((UnitFields) ((int) unitFields + (int) school),
        GetInt32((UnitFields) ((int) unitFields + (int) school)) - delta);
      OnResistanceChanged(school);
    }

    protected virtual void OnResistanceChanged(DamageSchool school)
    {
      SetInt32((UnitFields) (99U + school),
        GetBaseResistance(school) + GetResistanceBuffPositive(school) - GetResistanceBuffNegative(school));
    }

    public int GetResistanceBuffPositive(DamageSchool school)
    {
      return GetInt32((UnitFields) (106U + school));
    }

    public int GetResistanceBuffNegative(DamageSchool school)
    {
      return GetInt32((UnitFields) (113U + school));
    }

    public int ArmorBuffPositive
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE); }
    }

    public int HolyResistBuffPositive
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_2); }
    }

    public int FireResistBuffPositive
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_3); }
    }

    public int NatureResistBuffPositive
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_4); }
    }

    public int FrostResistBuffPositive
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_5); }
    }

    public int ShadowResistBuffPositive
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_6); }
    }

    public int ArcaneResistBuffPositive
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_7); }
    }

    public int ArmorBuffNegative
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE); }
    }

    public int HolyResistBuffNegative
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_2); }
    }

    public int FireResistBuffNegative
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_3); }
    }

    public int NatureResistBuffNegative
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_4); }
    }

    public int FrostResistBuffNegative
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_5); }
    }

    public int ShadowResistBuffNegative
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_6); }
    }

    public int ArcaneResistBuffNegative
    {
      get { return GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_7); }
    }

    public AuraStateMask AuraState
    {
      get { return (AuraStateMask) GetUInt32(UnitFields.AURASTATE); }
      set
      {
        SetUInt32(UnitFields.AURASTATE, (uint) value);
        if(!(m_auras is PlayerAuraCollection) || AuraState == value)
          return;
        ((PlayerAuraCollection) m_auras).OnAuraStateChanged();
      }
    }

    /// <summary>
    /// Helper function for Aurastate related fix and Conflagrate spell.
    /// see UpdateFieldHandler/Warlockfixes
    /// </summary>
    public Spell GetStrongestImmolate()
    {
      return null;
    }

    public byte[] UnitBytes0
    {
      get { return GetByteArray(UnitFields.BYTES_0); }
      set { SetByteArray(UnitFields.BYTES_0, value); }
    }

    public virtual RaceId Race
    {
      get { return (RaceId) GetByte(UnitFields.BYTES_0, 0); }
      set { SetByte(UnitFields.BYTES_0, 0, (byte) value); }
    }

    public virtual ClassId Class
    {
      get { return (ClassId) GetByte(UnitFields.BYTES_0, 1); }
      set { SetByte(UnitFields.BYTES_0, 1, (byte) value); }
    }

    public BaseClass GetBaseClass()
    {
      return ArchetypeMgr.GetClass(Class);
    }

    /// <summary>Race of the character.</summary>
    public RaceMask RaceMask
    {
      get { return (RaceMask) (1 << (int) (Race - 1 & (RaceId.Skeleton | RaceId.End))); }
    }

    /// <summary>RaceMask2 of the character.</summary>
    public RaceMask2 RaceMask2
    {
      get { return (RaceMask2) (1 << (int) (Race & (RaceId.Skeleton | RaceId.End))); }
    }

    /// <summary>Class of the character.</summary>
    public ClassMask ClassMask
    {
      get { return (ClassMask) (1 << (int) (Class - 1U & (ClassId) 31)); }
    }

    /// <summary>ClassMask2 of the character.</summary>
    public ClassMask2 ClassMask2
    {
      get { return (ClassMask2) (1 << (int) (Class & (ClassId) 31)); }
    }

    public virtual GenderType Gender
    {
      get { return (GenderType) GetByte(UnitFields.BYTES_0, 2); }
      set { SetByte(UnitFields.BYTES_0, 2, (byte) value); }
    }

    /// <summary>
    /// Make sure the PowerType is valid or it will crash the client
    /// </summary>
    public virtual PowerType PowerType
    {
      get { return (PowerType) GetByte(UnitFields.BYTES_0, 3); }
      set { SetByte(UnitFields.BYTES_0, 3, (byte) ((byte) value % 7U)); }
    }

    public byte[] UnitBytes1
    {
      get { return GetByteArray(UnitFields.BYTES_1); }
      set { SetByteArray(UnitFields.BYTES_1, value); }
    }

    public virtual StandState StandState
    {
      get { return (StandState) GetByte(UnitFields.BYTES_1, 0); }
      set { SetByte(UnitFields.BYTES_1, 0, (byte) value); }
    }

    public StateFlag StateFlags
    {
      get { return (StateFlag) GetByte(UnitFields.BYTES_1, 2); }
      set { SetByte(UnitFields.BYTES_1, 2, (byte) value); }
    }

    public byte UnitBytes1_3
    {
      get { return GetByte(UnitFields.BYTES_1, 3); }
      set { SetByte(UnitFields.BYTES_1, 3, value); }
    }

    public byte[] UnitBytes2
    {
      get { return GetByteArray(UnitFields.BYTES_2); }
      set { SetByteArray(UnitFields.BYTES_2, value); }
    }

    /// <summary>Set to 0x01 for Spirit Healers, Totems (?)</summary>
    public SheathType SheathType
    {
      get { return (SheathType) GetByte(UnitFields.BYTES_2, 0); }
      set { SetByte(UnitFields.BYTES_2, 0, (byte) value); }
    }

    /// <summary>
    /// Flags
    /// 0x1 - In PVP
    /// 0x4 - Free for all PVP
    /// 0x8 - In PVP Sanctuary
    /// </summary>
    public PvPState PvPState
    {
      get { return (PvPState) GetByte(UnitFields.BYTES_2, 1); }
      set { SetByte(UnitFields.BYTES_2, 1, (byte) value); }
    }

    /// <summary>
    /// </summary>
    public PetState PetState
    {
      get { return (PetState) GetByte(UnitFields.BYTES_2, 2); }
      set { SetByte(UnitFields.BYTES_2, 2, (byte) value); }
    }

    /// <summary>The entry of the current shapeshift form</summary>
    public ShapeshiftEntry ShapeshiftEntry
    {
      get { return SpellHandler.ShapeshiftEntries.Get((uint) ShapeshiftForm); }
    }

    public ShapeshiftForm ShapeshiftForm
    {
      get { return (ShapeshiftForm) GetByte(UnitFields.BYTES_2, 3); }
      set
      {
        ShapeshiftForm shapeshiftForm = ShapeshiftForm;
        if(shapeshiftForm != ShapeshiftForm.Normal)
        {
          ShapeshiftEntry shapeshiftEntry = SpellHandler.ShapeshiftEntries.Get((uint) value);
          if(shapeshiftEntry != null && HasSpells)
          {
            foreach(SpellId defaultActionBarSpell in shapeshiftEntry.DefaultActionBarSpells)
            {
              if(defaultActionBarSpell != SpellId.None)
                Spells.Remove(defaultActionBarSpell);
            }
          }
        }

        ShapeshiftEntry shapeshiftEntry1 = SpellHandler.ShapeshiftEntries.Get((uint) value);
        if(shapeshiftEntry1 != null)
        {
          UnitModelInfo unitModelInfo = FactionGroup != FactionGroup.Horde || shapeshiftEntry1.ModelIdHorde == 0U
            ? shapeshiftEntry1.ModelAlliance
            : shapeshiftEntry1.ModelHorde;
          if(unitModelInfo != null)
            Model = unitModelInfo;
          if(IsPlayer)
          {
            foreach(SpellId defaultActionBarSpell in shapeshiftEntry1.DefaultActionBarSpells)
            {
              if(defaultActionBarSpell != SpellId.None)
                Spells.AddSpell(defaultActionBarSpell);
            }
          }

          if(shapeshiftEntry1.PowerType != PowerType.End)
            PowerType = shapeshiftEntry1.PowerType;
          else
            SetDefaultPowerType();
        }
        else
        {
          if(shapeshiftForm != ShapeshiftForm.Normal)
            DisplayId = NativeDisplayId;
          SetDefaultPowerType();
        }

        SetByte(UnitFields.BYTES_2, 3, (byte) value);
        if(!(m_auras is PlayerAuraCollection))
          return;
        ((PlayerAuraCollection) m_auras).OnShapeshiftFormChanged();
      }
    }

    /// <summary>Sets this Unit's default PowerType</summary>
    public void SetDefaultPowerType()
    {
      BaseClass baseClass = GetBaseClass();
      if(baseClass != null)
        PowerType = baseClass.DefaultPowerType;
      else
        PowerType = PowerType.Mana;
    }

    public ShapeshiftMask ShapeshiftMask
    {
      get
      {
        if(ShapeshiftForm == ShapeshiftForm.Normal)
          return ShapeshiftMask.None;
        return (ShapeshiftMask) (1 << (int) (ShapeshiftForm - 1 & ShapeshiftForm.Moonkin));
      }
    }

    /// <summary>Resets health, Power and Auras</summary>
    public void Cleanse()
    {
      foreach(Aura aura in m_auras)
      {
        if(aura.CasterUnit != this)
          aura.Remove(true);
      }

      Health = MaxHealth;
      Power = BasePower;
    }

    /// <summary>
    /// Whether this is actively controlled by a player.
    /// Not to be confused with IsOwnedByPlayer.
    /// </summary>
    public override bool IsPlayerControlled
    {
      get { return UnitFlags.HasAnyFlag(UnitFlags.PlayerControlled); }
    }

    /// <summary>If this is not an Honorless Target</summary>
    public bool YieldsXpOrHonor { get; set; }

    public UnitExtraFlags ExtraFlags { get; set; }

    public void Kill()
    {
      Kill(null);
    }

    public void Kill(Unit killer)
    {
      if(killer != null)
      {
        if(FirstAttacker == null)
          FirstAttacker = killer;
        LastKiller = killer;
      }

      Health = 0;
    }

    /// <summary>
    /// This Unit's current Health.
    /// Health cannot exceed MaxHealth.
    /// If Health reaches 0, the Unit dies.
    /// If Health is 0 and increases, the Unit gets resurrected.
    /// </summary>
    public virtual int Health
    {
      get { return (int) GetUInt32(UnitFields.HEALTH); }
      set
      {
        int health = Health;
        int maxHealth = MaxHealth;
        if(value >= maxHealth)
          value = maxHealth;
        else if(value < 0)
          value = 0;
        if(value == health)
          return;
        if(value < 1)
        {
          Character character = this as Character;
          if(character != null && character.IsAsda2Dueling)
          {
            character.Asda2Duel.Losser = character;
            SetUInt32(UnitFields.HEALTH, (uint) maxHealth);
          }
          else
            Die(false);
        }
        else
        {
          SetUInt32(UnitFields.HEALTH, (uint) value);
          UpdateHealthAuraState();
          if(health == 0)
            DecMechanicCount(SpellMechanic.Rooted, false);
          if(IsAlive && health >= 1)
            return;
          OnResurrect();
        }
      }
    }

    /// <summary>Base maximum health, before modifiers.</summary>
    public int BaseHealth
    {
      get { return GetInt32(UnitFields.BASE_HEALTH); }
      set
      {
        SetInt32(UnitFields.BASE_HEALTH, value);
        UpdateMaxHealth();
      }
    }

    /// <summary>
    /// Total maximum Health of this Unit.
    /// In order to change this value, set BaseHealth.
    /// </summary>
    public virtual int MaxHealth
    {
      get { return GetInt32(UnitFields.MAXHEALTH); }
      internal set
      {
        if(Health > value)
          Health = value;
        SetInt32(UnitFields.MAXHEALTH, value);
      }
    }

    public int MaxHealthModFlat
    {
      get { return m_maxHealthModFlat; }
      set
      {
        m_maxHealthModFlat = value;
        UpdateMaxHealth();
      }
    }

    public float MaxHealthModScalar
    {
      get { return GetFloat(UnitFields.MAXHEALTHMODIFIER); }
      set
      {
        SetFloat(UnitFields.MAXHEALTHMODIFIER, value);
        UpdateMaxHealth();
      }
    }

    /// <summary>Current amount of health in percent</summary>
    public int HealthPct
    {
      get
      {
        int maxHealth = MaxHealth;
        return (100 * Health + (maxHealth >> 1)) / maxHealth;
      }
      set { Health = (value * MaxHealth + 50) / 100; }
    }

    /// <summary>Current amount of power in percent</summary>
    public int PowerPct
    {
      get
      {
        int maxPower = MaxPower;
        return (100 * Power + (maxPower >> 1)) / maxPower;
      }
      set { Power = (value * MaxPower + 50) / 100; }
    }

    /// <summary>
    /// </summary>
    protected void UpdateHealthAuraState()
    {
      int healthPct = HealthPct;
      if(healthPct <= 20)
        AuraState = AuraState & ~(AuraStateMask.Health35Percent | AuraStateMask.HealthAbove75Pct) |
                    AuraStateMask.Health20Percent;
      else if(healthPct <= 35)
        AuraState = AuraState & ~(AuraStateMask.Health20Percent | AuraStateMask.HealthAbove75Pct) |
                    AuraStateMask.Health35Percent;
      else if(healthPct <= 75)
        AuraState &= ~(AuraStateMask.Health20Percent | AuraStateMask.Health35Percent | AuraStateMask.HealthAbove75Pct);
      else
        AuraState = AuraState & ~(AuraStateMask.Health20Percent | AuraStateMask.Health35Percent) |
                    AuraStateMask.HealthAbove75Pct;
    }

    /// <summary>The flat PowerCostModifier for your default Power</summary>
    public int PowerCostModifier
    {
      get { return GetInt32((UnitFields) (131 + PowerType)); }
      internal set { SetInt32((UnitFields) (131 + PowerType), value); }
    }

    /// <summary>The PowerCostMultiplier for your default Power</summary>
    public float PowerCostMultiplier
    {
      get { return GetFloat((UnitFields) (138 + PowerType)); }
      internal set { SetFloat((UnitFields) (138 + PowerType), value); }
    }

    /// <summary>Base maximum power, before modifiers.</summary>
    public int BasePower
    {
      get { return GetInt32(UnitFields.BASE_MANA); }
      set
      {
        SetInt32(UnitFields.BASE_MANA, value);
        UpdateMaxPower();
        if(PowerType == PowerType.Rage || PowerType == PowerType.Energy)
          return;
        Power = MaxPower;
      }
    }

    public void SetBasePowerDontUpdate(int value)
    {
      SetInt32(UnitFields.BASE_MANA, value);
      if(PowerType == PowerType.Rage || PowerType == PowerType.Energy)
        return;
      Power = MaxPower;
    }

    /// <summary>
    /// The amount of the Unit's default Power (Mana, Energy, Rage, Happiness etc)
    /// </summary>
    public virtual int Power
    {
      get { return _power; }
      set
      {
        if(value > MaxPower)
          _power = MaxPower;
        else
          _power = value;
      }
    }

    public bool IsSitting
    {
      get { return _isSitting; }
      set
      {
        if(_isSitting == value)
          return;
        _isSitting = value;
        Character chr = this as Character;
        if(Map == null || chr == null)
          return;
        this.UpdatePowerRegen();
        Map.CallDelayed(100,
          () => Asda2CharacterHandler.SendEmoteResponse(chr, value ? (short) 108 : (short) 109, 0, 0.0f, 0.0f));
      }
    }

    internal void UpdatePower(int delayMillis)
    {
      _tempPower += PowerRegenPerTickActual * delayMillis / RegenerationFormulas.RegenTickDelayMillis;
      _timeFromLastMpUpdate += delayMillis;
      if(_timeFromLastMpUpdate <= 5000)
        return;
      _timeFromLastMpUpdate = 0;
      if(_tempPower <= 1.0)
        return;
      int tempPower = (int) _tempPower;
      _tempPower -= tempPower;
      if(Power >= MaxPower)
        return;
      Power += tempPower;
    }

    protected static void SendPowerUpdates(Character chr)
    {
      Asda2CharacterHandler.SendCharMpUpdateResponse(chr);
      if(chr.IsInGroup)
        Asda2GroupHandler.SendPartyMemberInitialInfoResponse(chr);
      if(!chr.IsSoulmated)
        return;
      Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(chr.Client);
    }

    /// <summary>
    /// The max amount of the Unit's default Power (Mana, Energy, Rage, Happiness etc)
    /// NOTE: This is not related to Homer Simpson nor to any brand of hair blowers
    /// </summary>
    public virtual int MaxPower
    {
      get { return GetInt32((UnitFields) (33 + PowerType)); }
      internal set { SetInt32((UnitFields) (33 + PowerType), value); }
    }

    public virtual float ParryChance
    {
      get { return 5f; }
      internal set { }
    }

    /// <summary>
    /// Amount of additional yards to be allowed to jump without having any damage inflicted.
    /// TODO: Implement correctly (needs client packets)
    /// </summary>
    public int SafeFall { get; internal set; }

    public int AoEDamageModifierPct { get; set; }

    public virtual uint Defense
    {
      get { return (uint) (5 * Level); }
      internal set { }
    }

    public virtual TalentCollection Talents
    {
      get { return null; }
    }

    public float Asda2DropChance
    {
      get { return _asda2DropChance; }
      set { _asda2DropChance = value; }
    }

    public int Asda2BaseLuck { get; set; }

    public int Asda2BaseStrength { get; set; }

    public int Asda2BaseAgility { get; set; }

    public int Asda2BaseIntellect { get; set; }

    public int Asda2BaseSpirit { get; set; }

    public int Asda2BaseStamina { get; set; }

    public int Asda2Luck { get; set; }

    public int Asda2Strength { get; set; }

    public int Asda2Agility { get; set; }

    public int Asda2Intellect { get; set; }

    public int Asda2Spirit { get; set; }

    public int Asda2Stamina { get; set; }

    public float Asda2LightResistence { get; set; }

    public float Asda2DarkResistence { get; set; }

    public float Asda2FireResistence { get; set; }

    public float Asda2EarthResistence { get; set; }

    public float Asda2ClimateResistence { get; set; }

    public float Asda2WaterResistence { get; set; }

    public float Asda2LightAttribute { get; set; }

    public float Asda2DarkAttribute { get; set; }

    public float Asda2FireAttribute { get; set; }

    public float Asda2EarthAttribute { get; set; }

    public float Asda2ClimateAttribute { get; set; }

    public float Asda2WaterAttribute { get; set; }

    public float Asda2GoldAmountBoost
    {
      get { return _asda2GoldAmountBoost; }
      set { _asda2GoldAmountBoost = value; }
    }

    public float Asda2ExpAmountBoost
    {
      get { return _asda2ExpAmountBoost; }
      set { _asda2ExpAmountBoost = value; }
    }

    public bool IsVisible
    {
      get { return _isVisible; }
      set { _isVisible = value; }
    }

    public DateTime CastingTill { get; set; }

    public DateTime NextSpellUpdate { get; set; }

    protected override UpdateFieldCollection _UpdateFieldInfos
    {
      get { return UpdateFieldInfos; }
    }

    protected Unit()
    {
      Type |= ObjectTypes.Unit;
      m_isInCombat = false;
      m_attackTimer = new TimerEntry(CombatTick);
      CastSpeedFactor = 1f;
      ResetMechanicDefaults();
      m_flying = m_waterWalk = m_hovering = m_featherFalling = 0U;
      m_canMove = m_canInteract = m_canHarm = m_canCastSpells = true;
    }

    public int GetBaseLuck()
    {
      return 10 * Level;
    }

    /// <summary>The Unit that attacked this NPC first.</summary>
    public Unit FirstAttacker
    {
      get { return m_FirstAttacker; }
      set
      {
        if(value != null)
          value = value.Master ?? value;
        m_FirstAttacker = value;
        MarkUpdate(UnitFields.DYNAMIC_FLAGS);
      }
    }

    /// <summary>
    /// The Unit that last killed this guy or null, if none or gone (is not reliable over time).
    /// </summary>
    public Unit LastKiller
    {
      get
      {
        if(m_LastKiller == null || !m_LastKiller.IsInWorld)
          m_LastKiller = null;
        return m_LastKiller;
      }
      internal set { m_LastKiller = value; }
    }

    /// <summary>
    /// Whether this Unit is currently participating in PvP.
    /// That is if both participants are players and/or belong to players.
    /// </summary>
    public bool IsPvPing
    {
      get
      {
        if(m_FirstAttacker != null && IsPlayerOwned)
          return m_FirstAttacker.IsPlayerOwned;
        return false;
      }
    }

    public IBrain Brain
    {
      get { return m_brain; }
      set { m_brain = value; }
    }

    /// <summary>Whether this is a Spirit Guide/Spirit Healer.</summary>
    public bool IsSpiritHealer
    {
      get { return NPCFlags.HasFlag(NPCFlags.SpiritHealer); }
    }

    /// <summary>
    /// A collection of all Auras (talents/buffs/debuffs) of this Unit
    /// </summary>
    public AuraCollection Auras
    {
      get { return m_auras; }
    }

    public ulong AuraUpdateMask
    {
      get { return m_auraUpdateMask; }
      set { m_auraUpdateMask = value; }
    }

    /// <summary>Gets the chat tag for the character.</summary>
    public virtual ChatTag ChatTag
    {
      get { return ChatTag.None; }
    }

    public int LastMoveTime
    {
      get { return m_lastMoveTime; }
    }

    /// <summary>Amount of current combo points with last combo target</summary>
    public int ComboPoints
    {
      get { return m_comboPoints; }
    }

    /// <summary>Current holder of combo-points for this chr</summary>
    public Unit ComboTarget
    {
      get { return m_comboTarget; }
    }

    public void ResetComboPoints()
    {
      if(m_comboTarget == null)
        return;
      ModComboState(null, 0);
    }

    /// <summary>Change combo target and/or amount of combo points</summary>
    /// <returns>If there is a change</returns>
    public virtual bool ModComboState(Unit target, int amount)
    {
      if(amount == 0 && target == m_comboTarget)
        return false;
      if(target == null)
      {
        m_comboPoints = 0;
      }
      else
      {
        if(target == m_comboTarget)
          m_comboPoints += amount;
        else
          m_comboPoints = amount;
        m_comboPoints = MathUtil.ClampMinMax(m_comboPoints, 0, 5);
      }

      m_comboTarget = target;
      return true;
    }

    /// <summary>Returns one of the arbitrary modifier values</summary>
    public int GetIntMod(StatModifierInt stat)
    {
      if(IntMods != null)
        return IntMods[(int) stat];
      return 0;
    }

    public virtual bool IsAlive
    {
      get
      {
        if(IsInWorld)
          return Health > 0;
        return false;
      }
    }

    public bool IsDead
    {
      get { return !IsAlive; }
    }

    /// <summary>Whether this is a ghost</summary>
    public bool IsGhost
    {
      get { return m_auras.GhostAura != null; }
    }

    /// <summary>
    /// This is used to prevent this Unit from dying during a
    /// critical process, such as damage application.
    /// If health is at 0 this Unit won't "<see cref="M:WCell.RealmServer.Entities.Unit.Die(System.Boolean)" />" until
    /// DeathPrevention is set to 0 again. This prevents certain problems from happening.
    /// </summary>
    protected internal uint DeathPrevention
    {
      get { return m_DeathPrevention; }
      set
      {
        if((int) m_DeathPrevention == (int) value)
          return;
        m_DeathPrevention = value;
        if(value != 0U || Health != 0)
          return;
        Die(true);
      }
    }

    /// <summary>
    /// Different from <see cref="M:WCell.RealmServer.Entities.Unit.Kill" /> which actively kills the Unit.
    /// Is called when this Unit dies, i.e. Health gets smaller than 1.
    /// </summary>
    protected void Die(bool force)
    {
      if(force || !IsAlive || !OnBeforeDeath())
        return;
      SetUInt32(UnitFields.HEALTH, 0U);
      Character chr = this as Character;
      if(chr != null)
        Asda2CharacterHandler.SendHealthUpdate(chr, false, false);
      MarkUpdate(UnitFields.DYNAMIC_FLAGS);
      Dismount();
      SpellCast spellCast = m_spellCast;
      if(spellCast != null && spellCast.Spell != null)
        m_spellCast.Cancel(SpellFailedReason.Ok);
      m_auras.RemoveWhere(aura => !aura.Spell.PersistsThroughDeath);
      this.UpdatePowerRegen();
      Power = 0;
      IsInCombat = false;
      CancelTaxiFlight();
      if(m_brain != null)
        m_brain.OnDeath();
      OnDeath();
      Target = null;
      if(chr == null || Map == null)
        return;
      Map.CallDelayed(LastDamageDelay, () => Asda2CharacterHandler.SendSelfDeathResponse(chr));
    }

    protected abstract bool OnBeforeDeath();

    protected abstract void OnDeath();

    /// <summary>Resurrects this Unit if dead</summary>
    public void Resurrect()
    {
      if(IsAlive)
        return;
      Health = MaxHealth / 2;
      if(PowerType == PowerType.Mana)
        Power = MaxPower / 2;
      else if(PowerType == PowerType.Rage)
        Power = 0;
      else if(PowerType == PowerType.Energy)
        Power = MaxPower;
      Character chr = this as Character;
      if(chr == null)
        return;
      Asda2CharacterHandler.SendResurectResponse(chr);
    }

    /// <summary>Called automatically when Unit re-gains Health.</summary>
    protected internal virtual void OnResurrect()
    {
      Character chr = this as Character;
      if(chr != null)
        Asda2CharacterHandler.SendHealthUpdate(chr, false, false);
      MarkUpdate(UnitFields.DYNAMIC_FLAGS);
    }

    /// <summary>whether this Unit is sitting on a ride</summary>
    public bool IsMounted
    {
      get
      {
        if(m_auras != null)
          return m_auras.MountAura != null;
        return false;
      }
    }

    public void Mount(MountId mountEntry)
    {
    }

    public void Mount(NPCId mountId)
    {
    }

    /// <summary>Mounts the given displayId</summary>
    public virtual void Mount(uint displayId)
    {
      Dismount();
      SetUInt32(UnitFields.MOUNTDISPLAYID, displayId);
      IncMechanicCount(SpellMechanic.Mounted, false);
    }

    /// <summary>Takes the mount off this Unit's butt (if mounted)</summary>
    public void Dismount()
    {
      if(!IsUnderInfluenceOf(SpellMechanic.Mounted))
        return;
      if(m_auras.MountAura != null)
        m_auras.MountAura.Remove(false);
      else
        DoDismount();
    }

    /// <summary>
    /// Is called internally.
    /// <see cref="M:WCell.RealmServer.Entities.Unit.Dismount" />
    /// </summary>
    protected internal virtual void DoDismount()
    {
      m_auras.MountAura = null;
      SetUInt32(UnitFields.MOUNTDISPLAYID, 0U);
      DecMechanicCount(SpellMechanic.Mounted, false);
    }

    /// <summary>whether the Unit is allowed to regenerate at all.</summary>
    public bool Regenerates
    {
      get { return m_regenerates; }
      set
      {
        if(value == m_regenerates)
          return;
        if(m_regenerates = value)
        {
          this.UpdatePowerRegen();
          UnitFlags2 |= UnitFlags2.RegeneratePower;
        }
        else
          UnitFlags2 ^= UnitFlags2.RegeneratePower;
      }
    }

    public virtual bool IsRegenerating
    {
      get
      {
        if(m_regenerates)
          return IsAlive;
        return false;
      }
    }

    /// <summary>
    /// Mana regen is in the "interrupted" state for Spell-Casters 5 seconds after a SpellCast and during SpellChanneling.
    /// See http://www.wowwiki.com/Mana_regeneration#Five-Second_Rule
    /// </summary>
    public bool IsManaRegenInterrupted
    {
      get
      {
        if(PowerType != PowerType.Mana || m_spellCast == null)
          return false;
        if(Environment.TickCount - m_spellCast.StartTime >= RegenerationFormulas.PowerRegenInterruptedCooldown)
          return m_spellCast.IsChanneling;
        return true;
      }
    }

    /// <summary>The real amount of Power that is added per regen-tick</summary>
    public float PowerRegenPerTickActual { get; internal set; }

    /// <summary>
    /// The amount of Power to add per regen-tick (while not being "interrupted").
    /// Value is automatically set, depending on stats
    /// </summary>
    public int PowerRegenPerTick
    {
      get { return m_PowerRegenPerTick; }
      internal set
      {
        if(m_PowerRegenPerTick == value)
          return;
        m_PowerRegenPerTick = value;
        SetFloat((UnitFields) (40 + PowerType), value);
      }
    }

    public float PowerRegenPerMillis
    {
      get { return m_PowerRegenPerTick / (float) RegenerationFormulas.RegenTickDelayMillis; }
    }

    /// <summary>
    /// The amount of power to be generated during combat per regen tick (while being "interrupted")
    /// Only used for PowerType.Mana units
    /// </summary>
    public int ManaRegenPerTickInterrupted
    {
      get { return _manaRegenPerTickInterrupted; }
      internal set
      {
        if(_manaRegenPerTickInterrupted == value)
          return;
        _manaRegenPerTickInterrupted = value;
        SetFloat((UnitFields) (47 + PowerType), value);
      }
    }

    /// <summary>
    /// The amount of Health to add per regen-tick while not in combat
    /// </summary>
    public int HealthRegenPerTickNoCombat { get; internal set; }

    /// <summary>
    /// The amount of Health to add per regen-tick during combat
    /// </summary>
    public int HealthRegenPerTickCombat { get; internal set; }

    /// <summary>Is called on Regeneration ticks</summary>
    protected void Regenerate(int dt)
    {
      if(!IsRegenerating)
        return;
      int health = Health;
      _tempHealthRegen +=
        (!IsSitting ? (m_isInCombat ? HealthRegenPerTickCombat : HealthRegenPerTickNoCombat) : MaxHealth * 0.009f) *
        dt / RegenerationFormulas.RegenTickDelayMillis;
      _timeFromLastHealthUpdate += dt;
      if(_timeFromLastHealthUpdate > 5000)
      {
        _timeFromLastHealthUpdate -= 5000;
        if(_tempHealthRegen > 1.0)
        {
          int tempHealthRegen = (int) _tempHealthRegen;
          _tempHealthRegen -= tempHealthRegen;
          if(Health < MaxHealth)
          {
            Health = health + tempHealthRegen;
            Character chr = this as Character;
            if(chr != null)
              Asda2CharacterHandler.SendHealthUpdate(chr, false, false);
          }
        }
      }

      UpdatePower(dt);
    }

    public virtual int GetBasePowerRegen()
    {
      return 0;
    }

    /// <summary>
    /// Returns the modified power-cost needed to cast a Spell of the given DamageSchool
    /// and the given base amount of power required
    /// </summary>
    public virtual int GetPowerCost(DamageSchool school, Spell spell, int cost)
    {
      int powerCostModifier = PowerCostModifier;
      if(m_schoolPowerCostMods != null)
        powerCostModifier += m_schoolPowerCostMods[(int) school];
      cost += powerCostModifier;
      cost = (int) (Math.Round(PowerCostMultiplier) * cost);
      return cost;
    }

    /// <summary>
    /// Modifies the power-cost for the given DamageSchool by value
    /// </summary>
    public void ModPowerCost(DamageSchool type, int value)
    {
      if(m_schoolPowerCostMods == null)
        m_schoolPowerCostMods = new int[DamageSchoolCount];
      m_schoolPowerCostMods[(int) type] += value;
    }

    /// <summary>
    /// Modifies the power-cost for all of the given DamageSchools by value
    /// </summary>
    public void ModPowerCost(uint[] schools, int value)
    {
      if(m_schoolPowerCostMods == null)
        m_schoolPowerCostMods = new int[DamageSchoolCount];
      foreach(uint school in schools)
        m_schoolPowerCostMods[school] += value;
    }

    /// <summary>
    /// Modifies the power-cost for the given DamageSchool by value
    /// </summary>
    public void ModPowerCostPct(DamageSchool type, int value)
    {
      if(m_schoolPowerCostMods == null)
        m_schoolPowerCostMods = new int[DamageSchoolCount];
      m_schoolPowerCostMods[(int) type] += value;
    }

    /// <summary>
    /// Modifies the power-cost for all of the given DamageSchools by value
    /// </summary>
    public void ModPowerCostPct(uint[] schools, int value)
    {
      if(m_schoolPowerCostMods == null)
        m_schoolPowerCostMods = new int[DamageSchoolCount];
      foreach(uint school in schools)
        m_schoolPowerCostMods[school] += value;
    }

    /// <summary>
    /// Tries to consume the given amount of Power, also considers modifiers to Power-cost.
    /// </summary>
    public bool ConsumePower(DamageSchool type, Spell spell, int neededPower)
    {
      neededPower = GetPowerCost(type, spell, neededPower);
      if(Power < neededPower)
        return false;
      Power -= neededPower;
      return true;
    }

    public int GetHealthPercent(int value)
    {
      return (value * MaxHealth + 50) / 100;
    }

    /// <summary>
    /// Heals this unit and sends the corresponding animation (healer might be null)
    /// </summary>
    /// <param name="effect">The effect of the spell that triggered the healing (or null)</param>
    /// <param name="healer">The object that heals this Unit (or null)</param>
    /// <param name="value">The amount to be healed</param>
    public void HealPercent(int value, Unit healer = null, SpellEffect effect = null)
    {
      Heal((value * MaxHealth + 50) / 100, healer, effect);
    }

    /// <summary>
    /// Heals this unit and sends the corresponding animation (healer might be null)
    /// </summary>
    /// <param name="value">The amount to be healed</param>
    /// <param name="healer">The object that heals this Unit (or null)</param>
    /// <param name="effect">The effect of the spell that triggered the healing (or null)</param>
    public void Heal(int value, Unit healer = null, SpellEffect effect = null)
    {
      Character chr = this as Character;
      bool flag = false;
      int num1 = 0;
      if(effect != null)
      {
        int num2 = value;
        if(healer != null)
          value = !effect.IsPeriodic
            ? healer.AddHealingModsToAction(value, effect, effect.Spell.Schools[0])
            : healer.Auras.GetModifiedInt(SpellModifierType.PeriodicEffectValue, effect.Spell, value);
        if(chr != null)
          value += (int) (num2 * (double) chr.HealingTakenModPct / 100.0);
        float num3 = GetCritChance(effect.Spell.Schools[0]) * 100f;
        if(!effect.Spell.AttributesExB.HasFlag(SpellAttributesExB.CannotCrit) && num3 != 0.0 &&
           Utility.Random(1f, 10001f) <= (double) num3)
        {
          value = (int) (value * (SpellHandler.SpellCritBaseFactor +
                                  (double) GetIntMod(StatModifierInt.CriticalHealValuePct)));
          flag = true;
        }
      }

      if(value > 0)
      {
        value = (int) (value * (double) Utility.Random(0.95f, 1.05f));
        if(chr != null)
          value += (int) (value * (double) chr.HealingTakenModPct / 100.0);
        if(Health + value > MaxHealth)
        {
          num1 = Health + value - MaxHealth;
          value = MaxHealth - Health;
        }

        Health += value;
        value += num1;
        if(chr != null)
          Asda2CharacterHandler.SendHealthUpdate(chr, true, false);
      }

      if(healer == null)
        return;
      HealAction action = new HealAction();
      action.Attacker = healer;
      action.Victim = this;
      action.Spell = effect?.Spell;
      action.IsCritical = flag;
      action.IsHot = effect.IsPeriodic;
      action.Value = value;
      OnHeal(action);
    }

    /// <summary>
    /// This method is called whenever a heal is placed on a Unit by another Unit
    /// </summary>
    /// <param name="healer">The healer</param>
    /// <param name="value">The amount of points healed</param>
    protected virtual void OnHeal(HealAction action)
    {
      if(action.Value > 0)
        TriggerProcOnHeal(action);
      this.IterateEnvironment(15f, obj =>
      {
        if(obj is Unit && ((Unit) obj).m_brain != null)
          ((Unit) obj).m_brain.OnHeal(action.Attacker, this, action.Value);
        return true;
      });
    }

    private void TriggerProcOnHeal(HealAction action)
    {
      if(!action.IsHot)
        return;
      ProcHitFlags hitFlags = action.IsCritical ? ProcHitFlags.CriticalHit : ProcHitFlags.NormalHit;
      action.Attacker.Proc(ProcTriggerFlags.DonePeriodicDamageOrHeal, this, action, true, hitFlags);
      Proc(ProcTriggerFlags.ReceivedPeriodicDamageOrHeal, action.Attacker, action, true, hitFlags);
    }

    /// <summary>
    /// Leeches the given amount of health from this Unit and adds it to the receiver (if receiver != null and is Unit).
    /// </summary>
    /// <param name="factor">The factor applied to the amount that was leeched before adding it to the receiver</param>
    public void LeechHealth(Unit receiver, int amount, float factor, SpellEffect effect)
    {
      int health = Health;
      DealSpellDamage(receiver != null ? receiver.Master : this, effect, amount, true, true, false, true);
      amount = health - Health;
      if(factor > 0.0)
        amount = (int) (amount * (double) factor);
      if(receiver == null)
        return;
      receiver.Heal(amount, this, effect);
    }

    /// <summary>Restores Power and sends the corresponding Packet</summary>
    public void EnergizePercent(int value, Unit energizer = null, SpellEffect effect = null)
    {
      Energize((value * MaxPower + 50) / 100, energizer, effect);
    }

    /// <summary>Restores Power and sends the corresponding Packet</summary>
    public void Energize(int value, Unit energizer = null, SpellEffect effect = null)
    {
      if(value == 0)
        return;
      int power = Power;
      value = MathUtil.ClampMinMax(value, -power, MaxPower - value);
      CombatLogHandler.SendEnergizeLog(energizer, this, effect != null ? effect.Spell.Id : 0U, PowerType, value);
      Power = power + value;
    }

    /// <summary>
    /// Leeches the given amount of power from this Unit and adds it to the receiver (if receiver != null and is Unit).
    /// </summary>
    public void LeechPower(int amount, float factor = 1f, Unit receiver = null, SpellEffect effect = null)
    {
      int power = Power;
      amount -= MathUtil.RoundInt((float) (amount * (double) GetResiliencePct() * 2.20000004768372));
      if(amount > power)
        amount = power;
      Power = power - amount;
      if(receiver == null)
        return;
      receiver.Energize(amount, this, effect);
    }

    /// <summary>
    /// Drains the given amount of power and applies damage for it
    /// </summary>
    /// <param name="dmgTyp">The type of the damage applied</param>
    /// <param name="dmgFactor">The factor to be applied to amount for the damage to be received by this unit</param>
    public void BurnPower(int amount, float dmgFactor = 1f, Unit attacker = null, SpellEffect effect = null)
    {
      int power = Power;
      amount -= MathUtil.RoundInt((float) (amount * (double) GetResiliencePct() * 2.20000004768372));
      if(amount > power)
        amount = power;
      Power = power - amount;
      DealSpellDamage(attacker, effect, (int) (amount * (double) dmgFactor), true, true, false, true);
    }

    protected internal override void OnEnterMap()
    {
      m_lastMoveTime = Environment.TickCount;
      if(Flying <= 0U)
        return;
      MovementHandler.SendFlyModeStart(this);
    }

    /// <summary>Is called whenever a Unit moves</summary>
    public virtual void OnMove()
    {
      IsSitting = false;
      SpellCast spellCast = m_spellCast;
      if(m_auras == null)
        return;
      m_auras.RemoveByFlag(AuraInterruptFlags.OnMovement);
      m_lastMoveTime = Environment.TickCount;
    }

    /// <summary>whether this Unit is currently moving</summary>
    public virtual bool IsMoving
    {
      get { return Environment.TickCount - m_lastMoveTime < MinStandStillDelay; }
    }

    /// <summary>
    /// Makes this Unit move their face towards the given object
    /// </summary>
    public void Face(WorldObject obj)
    {
      if(obj != m_target || IsPlayerControlled)
        Face(m_orientation);
      else
        m_orientation = GetAngleTowards(obj);
    }

    /// <summary>Makes this Unit look at the given location</summary>
    public void Face(Vector3 pos)
    {
      Face(GetAngleTowards(pos));
    }

    /// <summary>
    /// Makes this Unit move their face towards the given orientation
    /// </summary>
    public void Face(float orientation)
    {
      m_orientation = orientation;
      MovementHandler.SendFacingPacket(this, orientation, (uint) (314.0 / TurnSpeed));
    }

    /// <summary>
    /// Checks whether this Unit can currently see the given obj
    /// 
    /// TODO: Higher staff ranks can always see lower staff ranks (too bad there are no ranks)
    /// TODO: Line of Sight
    /// </summary>
    public override bool CanSee(WorldObject obj)
    {
      if(!base.CanSee(obj) || !obj.IsInWorld)
      {
        return false;
      }

      if(this == obj)
      {
        return true;
      }

      switch(obj.DetermineVisibilityFor(this))
      {
        case VisibilityStatus.Invisible:
          return false;

        case VisibilityStatus.Visible:
          return true;
      }

      if(((this is Character) && ((Character) this).Role.IsStaff) &&
         (!(obj is Character) || (((Character) obj).Role < ((Character) this).Role)))
      {
        if((obj is Unit) && ((Unit) obj).IsSpiritHealer)
        {
          return !IsAlive;
        }

        return true;
      }

      if(!(obj is Unit))
      {
        return true;
      }

      Unit unit = (Unit) obj;
      if(IsGhost)
      {
        if(this is Character)
        {
          Corpse pos = ((Character) this).Corpse;
          if(pos != null)
          {
            return unit.IsInRadiusSq(pos, Corpse.GhostVisibilityRadiusSq);
          }
        }

        return false;
      }

      if(obj is Character)
      {
        Character character = (Character) obj;
        if((character.Role.IsStaff && (character.Stealthed > 0)) &&
           (!(this is Character) || (((Character) this).Role < character.Role)))
        {
          return false;
        }

        if(((this is Character) && (character.GroupMember != null)) && (((Character) this).Group == character.Group))
        {
          return true;
        }
      }

      if(!unit.IsSpiritHealer && !unit.IsGhost)
      {
        return HandleStealthDetection(unit);
      }

      return IsGhost;
    }

    public bool HandleStealthDetection(Unit unit)
    {
      if(unit.Stealthed <= 0)
        return true;
      if((UnitFlags & UnitFlags.Stunned) != UnitFlags.None)
        return false;
      if(GetDistance(unit.Position) <= 0.239999994635582)
        return true;
      if(!unit.IsInFrontOf(this))
        return false;
      bool flag = false;
      if(Auras.GetTotalAuraModifier(AuraType.Aura_228) > 0)
        flag = true;
      if(flag)
        return true;
      float num1 = (float) (10.5 - unit.Stealthed / 100.0) + (Level - unit.Level);
      int num2 = Auras.GetTotalAuraModifier(AuraType.ModStealthLevel);
      if(num2 < 0)
        num2 = 0;
      int totalAuraModifier = Auras.GetTotalAuraModifier(AuraType.ModDetect, 0);
      float num3 = num1 - (totalAuraModifier - num2) / 5f;
      float num4 = (double) num3 > 45.0 ? 45f : num3;
      return GetDistance(unit.Position) <= (double) num4;
    }

    /// <summary>
    /// The spoken language of this Unit.
    /// If Character's have a SpokenLanguage, they cannot use any other.
    /// Default: <c>ChatLanguage.Universal</c>
    /// </summary>
    public ChatLanguage SpokenLanguage { get; set; }

    /// <summary>Cancels whatever this Unit is currently doing.</summary>
    public virtual void CancelAllActions()
    {
      if(m_spellCast != null)
        m_spellCast.Cancel(SpellFailedReason.Interrupted);
      Target = null;
    }

    public virtual void CancelSpellCast()
    {
      if(m_spellCast == null)
        return;
      m_spellCast.Cancel(SpellFailedReason.Interrupted);
    }

    public virtual void CancelEmote()
    {
      EmoteState = EmoteType.None;
    }

    /// <summary>Makes this Unit show an animation</summary>
    public void Emote(EmoteType emote)
    {
      EmoteHandler.SendEmote(this, emote);
    }

    /// <summary>Makes this Unit do a text emote</summary>
    /// <param name="emote">Anything that has a name (to do something with) or null</param>
    public void TextEmote(TextEmote emote, INamed target)
    {
      if(this is Character)
        ((Character) this).Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DoEmote, (uint) emote,
          0U, null);
      EmoteHandler.SendTextEmote(this, emote, target);
    }

    /// <summary>
    /// When pinned down, a Character cannot be
    /// logged out, moved or harmed.
    /// </summary>
    public bool IsPinnedDown
    {
      get { return m_IsPinnedDown; }
      set
      {
        if(!IsInWorld)
        {
          LogUtil.ErrorException(
            new InvalidOperationException("Character was already disposed when pinning down: " + this), true);
        }
        else
        {
          m_Map.EnsureContext();
          if(m_IsPinnedDown == value)
            return;
          m_IsPinnedDown = value;
          if(m_IsPinnedDown)
          {
            IsEvading = true;
            ++Stunned;
          }
          else if(this is Character && ((Character) this).Client.IsOffline)
          {
            ((Character) this).Logout(true, 0);
          }
          else
          {
            IsEvading = false;
            --Stunned;
          }
        }
      }
    }

    public bool IsStunned
    {
      get { return UnitFlags.HasFlag(UnitFlags.Stunned); }
    }

    internal void OnTaxiStart()
    {
      UnitFlags |= UnitFlags.Influenced;
      IsOnTaxi = true;
      taxiTime = 0;
      m_TaxiMovementTimer = new TimerEntry(0, TaxiMgr.InterpolationDelayMillis, TaxiTimerCallback);
      m_TaxiMovementTimer.Start();
      IsEvading = true;
    }

    internal void OnTaxiStop()
    {
      TaxiPaths.Clear();
      LatestTaxiPathNode = null;
      DoDismount();
      IsOnTaxi = false;
      UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable |
                   UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob |
                   UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive |
                   UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced |
                   UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 |
                   UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused |
                   UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable |
                   UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 |
                   UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
      m_TaxiMovementTimer.Stop();
      IsEvading = false;
    }

    /// <summary>Time spent on the current taxi-ride in millis.</summary>
    public int TaxiTime
    {
      get { return taxiTime; }
    }

    protected virtual void TaxiTimerCallback(int elapsedTime)
    {
      TaxiMgr.InterpolatePosition(this, elapsedTime);
    }

    /// <summary>Returns the players currently planned taxi paths.</summary>
    public Queue<TaxiPath> TaxiPaths
    {
      get { return m_TaxiPaths; }
    }

    /// <summary>
    /// The point on the currently travelled TaxiPath that the Unit past most recently, or null if not on a taxi.
    /// </summary>
    public LinkedListNode<PathVertex> LatestTaxiPathNode
    {
      get { return m_LatestTaxiPathNode; }
      internal set { m_LatestTaxiPathNode = value; }
    }

    /// <summary>
    /// Whether or not this unit is currently flying on a taxi.
    /// </summary>
    public bool IsOnTaxi
    {
      get { return UnitFlags.HasFlag(UnitFlags.TaxiFlight); }
      set
      {
        if(value == IsOnTaxi)
          return;
        if(value)
          UnitFlags |= UnitFlags.TaxiFlight;
        else
          UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable |
                       UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation |
                       UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable |
                       UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 |
                       UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 |
                       UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.Disarmed |
                       UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable |
                       UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 |
                       UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
      }
    }

    /// <summary>
    /// Whether or not this Unit is currently under the influence of an effect that won't allow it to be controled by itself or its master
    /// </summary>
    public bool IsInfluenced
    {
      get { return UnitFlags.HasFlag(UnitFlags.Influenced); }
      set
      {
        if(value)
          UnitFlags |= UnitFlags.Influenced;
        else
          UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable |
                       UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob |
                       UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive |
                       UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced |
                       UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 |
                       UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused |
                       UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable |
                       UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 |
                       UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
      }
    }

    public void CancelTaxiFlight()
    {
      if(!IsOnTaxi)
        return;
      MovementHandler.SendStopMovementPacket(this);
      OnTaxiStop();
    }

    /// <summary>Cancel any enforced movement</summary>
    public void CancelMovement()
    {
      CancelTaxiFlight();
      if(m_Movement == null)
        return;
      m_Movement.Stop();
    }

    public bool HasSpells
    {
      get { return m_spells != null; }
    }

    /// <summary>
    /// All spells known to this unit.
    /// Could be null for NPCs that are not spell-casters (check with <see cref="P:WCell.RealmServer.Entities.Unit.HasSpells" />).
    /// Use <see cref="P:WCell.RealmServer.Entities.NPC.NPCSpells" /> to enforce a SpellCollection.
    /// </summary>
    public virtual SpellCollection Spells
    {
      get { return m_spells; }
    }

    public bool HasEnoughPowerToCast(Spell spell, WorldObject selected)
    {
      if(!spell.CostsPower)
        return true;
      if(selected is Unit)
        return Power >= spell.CalcPowerCost(this, ((Unit) selected).GetLeastResistantSchool(spell));
      return Power >= spell.CalcPowerCost(this, spell.Schools[0]);
    }

    public DamageSchool GetLeastResistantSchool(Spell spell)
    {
      if(spell.Schools.Length == 1)
        return spell.Schools[0];
      int num = int.MaxValue;
      DamageSchool damageSchool = DamageSchool.Physical;
      foreach(DamageSchool school in spell.Schools)
      {
        int resistance = GetResistance(school);
        if(resistance < num)
        {
          num = resistance;
          damageSchool = school;
        }
      }

      return damageSchool;
    }

    public virtual bool MaySpawnPet(NPCEntry entry)
    {
      return true;
    }

    /// <summary>Tries to spawn the given pet for this Unit.</summary>
    /// <returns>null, if the Character already has that kind of Pet.</returns>
    public NPC SpawnMinion(NPCId id)
    {
      return SpawnMinion(id, 0);
    }

    /// <summary>Tries to spawn the given pet for this Unit.</summary>
    /// <returns>null, if the Character already has that kind of Pet.</returns>
    public NPC SpawnMinion(NPCId id, int durationMillis)
    {
      NPCEntry entry = NPCMgr.GetEntry(id);
      if(entry != null)
        return SpawnMinion(entry, ref m_position, durationMillis);
      return null;
    }

    /// <summary>
    /// Creates and makes visible the Unit's controlled Minion
    /// </summary>
    /// <param name="entry">The template for the Minion</param>
    /// <param name="position">The place to spawn the minion.</param>
    /// <param name="duration">Time till the minion goes away.</param>
    /// <returns>A reference to the minion.</returns>
    public NPC CreateMinion(NPCEntry entry, int durationMillis)
    {
      NPC minion = entry.Create(uint.MaxValue);
      minion.Phase = Phase;
      minion.Zone = Zone;
      minion.RemainingDecayDelayMillis = durationMillis;
      minion.Brain.IsRunning = true;
      if(Health > 0)
        Enslave(minion, durationMillis);
      return minion;
    }

    /// <summary>
    /// Creates and makes visible the Unit's controlled Minion
    /// </summary>
    /// <param name="entry">The template for the Minion</param>
    /// <param name="position">The place to spawn the minion.</param>
    /// <param name="duration">Time till the minion goes away.</param>
    /// <returns>A reference to the minion.</returns>
    public virtual NPC SpawnMinion(NPCEntry entry, ref Vector3 position, int durationMillis)
    {
      NPC minion = CreateMinion(entry, durationMillis);
      minion.Position = position;
      m_Map.AddObjectLater(minion);
      return minion;
    }

    public void Enslave(NPC minion)
    {
      Enslave(minion, 0);
    }

    public void Enslave(NPC minion, int durationMillis)
    {
      minion.Phase = Phase;
      minion.Master = this;
      switch(minion.Entry.Type)
      {
        case CreatureType.None:
        case CreatureType.NotSpecified:
          goto Label_0064;

        case CreatureType.NonCombatPet:
          minion.Brain.DefaultState = BrainState.Follow;
          break;

        case CreatureType.Totem:
          minion.Brain.DefaultState = BrainState.Roam;
          break;

        default:
          minion.Brain.DefaultState = BrainState.Guard;
          break;
      }

      minion.Brain.EnterDefaultState();
      Label_0064:
      if(durationMillis != 0)
      {
        minion.RemainingDecayDelayMillis = durationMillis;
      }
    }

    protected internal virtual void OnMinionDied(NPC minion)
    {
    }

    protected internal virtual void OnMinionEnteredMap(NPC minion)
    {
    }

    protected internal virtual void OnMinionLeftMap(NPC minion)
    {
    }

    /// <summary>Can be null if no handlers have been added.</summary>
    public List<IProcHandler> ProcHandlers
    {
      get { return m_procHandlers; }
    }

    public IProcHandler GetProcHandler(Func<IProcHandler, bool> predicate)
    {
      if(m_procHandlers != null)
      {
        foreach(IProcHandler procHandler in m_procHandlers)
        {
          if(predicate(procHandler))
            return procHandler;
        }
      }

      return null;
    }

    /// <summary>Returns the first proc handler of the given type</summary>
    public T GetProcHandler<T>() where T : IProcHandler
    {
      if(m_procHandlers != null)
      {
        foreach(IProcHandler procHandler in m_procHandlers)
        {
          if(procHandler is T)
            return (T) procHandler;
        }
      }

      return default(T);
    }

    public void AddProcHandler(ProcHandlerTemplate templ)
    {
      AddProcHandler(new ProcHandler(this, this, templ));
    }

    public void AddProcHandler(IProcHandler handler)
    {
      if(m_procHandlers == null)
        m_procHandlers = new List<IProcHandler>(5);
      m_procHandlers.Add(handler);
    }

    public void RemoveProcHandler(IProcHandler handler)
    {
      if(m_procHandlers == null)
        return;
      m_procHandlers.Remove(handler);
    }

    /// <summary>Remnoves the first proc that triggers the given spell</summary>
    public void RemoveProcHandler(SpellId procId)
    {
      if(m_procHandlers == null)
        return;
      foreach(IProcHandler procHandler in m_procHandlers)
      {
        if(procHandler.ProcSpell != null && procHandler.ProcSpell.SpellId == procId)
        {
          m_procHandlers.Remove(procHandler);
          break;
        }
      }
    }

    public void RemoveProcHandler(Func<IProcHandler, bool> predicate)
    {
      IProcHandler procHandler = GetProcHandler(predicate);
      if(procHandler == null)
        return;
      m_procHandlers.Remove(procHandler);
    }

    public void RemoveProcHandler<T>() where T : IProcHandler
    {
      T procHandler = GetProcHandler<T>();
      if(procHandler == null)
        return;
      m_procHandlers.Remove(procHandler);
    }

    /// <summary>
    /// Removes the first custom ProcHandler that uses the given template.
    /// </summary>
    public void RemoveProcHandler(ProcHandlerTemplate template)
    {
      if(m_procHandlers == null)
        return;
      foreach(IProcHandler procHandler in m_procHandlers)
      {
        if(procHandler is ProcHandler && ((ProcHandler) procHandler).Template == template)
        {
          m_procHandlers.Remove(procHandler);
          break;
        }
      }
    }

    /// <summary>
    /// Trigger all procs that can be triggered by the given action
    /// </summary>
    /// <param name="active">Whether the triggerer is the attacker/caster (true), or the victim (false)</param>
    public void Proc(ProcTriggerFlags flags, Unit triggerer, IUnitAction action, bool active,
      ProcHitFlags hitFlags = ProcHitFlags.None)
    {
      if(m_brain != null && m_brain.CurrentAction != null && m_brain.CurrentAction.InterruptFlags.HasAnyFlag(flags))
        m_brain.StopCurrentAction();
      if(m_procHandlers == null || flags == ProcTriggerFlags.None)
        return;
      if(triggerer == null)
      {
        log.Error("triggerer was null when triggering Proc by action: {0} (Flags: {1})", action, flags);
      }
      else
      {
        DateTime now = DateTime.Now;
        for(int index = m_procHandlers.Count - 1; index >= 0; --index)
        {
          if(index < m_procHandlers.Count)
          {
            IProcHandler procHandler = m_procHandlers[index];
            bool flag1 = procHandler.ProcTriggerFlags.HasAnyFlag(flags);
            bool flag2 = !flags.RequireHitFlags() || procHandler.ProcHitFlags.HasAnyFlag(hitFlags);
            if(procHandler.NextProcTime <= now && flag1 &&
               (flag2 && procHandler.CanBeTriggeredBy(triggerer, action, active)))
            {
              int num = (int) procHandler.ProcChance;
              if(num > 0 && action.Spell != null)
                num = Auras.GetModifiedInt(SpellModifierType.ProcChance, action.Spell, num);
              if(procHandler.ProcChance <= 0U || Utility.Random(0, 101) <= num)
              {
                int stackCount = procHandler.StackCount;
                procHandler.TriggerProc(triggerer, action);
                if(procHandler.MinProcDelay > 0)
                  procHandler.NextProcTime = now.AddMilliseconds(procHandler.MinProcDelay);
                if(stackCount > 0 && procHandler.StackCount == 0)
                  procHandler.Dispose();
              }
            }
          }
        }
      }
    }

    /// <summary>The GossipMenu, associated with this WorldObject.</summary>
    public GossipMenu GossipMenu
    {
      get { return m_gossipMenu; }
      set
      {
        m_gossipMenu = value;
        if(value != null)
          NPCFlags |= NPCFlags.Gossip;
        else
          NPCFlags &= ~NPCFlags.Gossip;
      }
    }

    /// <summary>
    /// Is called when Master / Faction has changed and this Unit now has a different circle of friends
    /// </summary>
    protected virtual void OnAffinityChanged()
    {
      if(!IsPlayerOwned)
        return;
      m_auras.RemoveOthersAuras();
    }

    public override void Dispose(bool disposing)
    {
      if(m_auras == null)
        return;
      if(m_Movement != null)
      {
        m_Movement.m_owner = null;
        m_Movement = null;
      }

      base.Dispose(disposing);
      m_attackTimer = null;
      m_target = null;
      if(m_brain != null)
      {
        m_brain.Dispose();
        m_brain = null;
      }

      m_spells.Recycle();
      m_spells = null;
      m_auras.Owner = null;
      m_auras = null;
      m_charm = null;
      m_channeled = null;
    }

    protected internal override void DeleteNow()
    {
      IsFighting = false;
      if(m_brain != null)
        m_brain.IsRunning = false;
      Target = null;
      base.DeleteNow();
    }

    protected virtual HighId HighId
    {
      get { return HighId.Unit; }
    }

    protected void GenerateId(uint entryId)
    {
      EntityId = new EntityId(NPCMgr.GenerateUniqueLowId(), entryId, HighId);
    }

    /// <summary>Whether this Unit can aggro NPCs.</summary>
    public bool CanGenerateThreat
    {
      get
      {
        if(IsInWorld && IsAlive)
          return !IsEvading;
        return false;
      }
    }

    public abstract LinkedList<WaypointEntry> Waypoints { get; }

    public abstract NPCSpawnPoint SpawnPoint { get; }

    public bool CanBeAggroedBy(Unit target)
    {
      if(target.CanGenerateThreat && IsHostileWith(target))
        return CanSee(target);
      return false;
    }

    /// <summary>
    /// Is called when a Unit successfully evaded (and arrived at its original location)
    /// </summary>
    internal void OnEvaded()
    {
      IsEvading = false;
      if(m_brain == null)
        return;
      m_brain.EnterDefaultState();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <remarks>Requires Brain</remarks>
    public void Follow(Unit target)
    {
      if(!CheckBrain())
        return;
      Target = target;
      m_brain.CurrentAction = new AIFollowTargetAction(this);
    }

    /// <summary>
    /// Moves towards the given target and then executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Vector3 pos, UnitActionCallback actionCallback)
    {
      if(!CheckBrain())
        return;
      m_Movement.MoveTo(pos, true);
      m_brain.CurrentAction = new AIMoveThenExecAction(this, actionCallback);
    }

    /// <summary>
    /// Moves towards the given target and then executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToPointsThenExecute(List<Vector3> points, UnitActionCallback actionCallback)
    {
      if(!CheckBrain())
        return;
      m_Movement.MoveToPoints(points);
      m_brain.CurrentAction = new AIMoveThenExecAction(this, actionCallback);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, UnitActionCallback actionCallback)
    {
      MoveToThenExecute(unit, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, UnitActionCallback actionCallback, int millisTimeout)
    {
      if(!CheckBrain())
        return;
      Target = unit;
      m_brain.CurrentAction = new AIMoveToThenExecAction(this, actionCallback);
    }

    /// <summary>
    /// Moves in front of the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveInFrontThenExecute(Unit unit, UnitActionCallback actionCallback)
    {
      MoveInFrontThenExecute(unit, actionCallback, 0);
    }

    /// <summary>
    /// Moves in front of the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveInFrontThenExecute(GameObject go, UnitActionCallback actionCallback)
    {
      MoveInFrontThenExecute(go, actionCallback, 0);
    }

    /// <summary>
    /// Moves in front of the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveInFrontThenExecute(Unit unit, UnitActionCallback actionCallback, int millisTimeout)
    {
      MoveToThenExecute(unit, 0.0f, actionCallback);
    }

    /// <summary>
    /// Moves in front of the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveInFrontThenExecute(GameObject go, UnitActionCallback actionCallback, int millisTimeout)
    {
      MoveToThenExecute(go, 0.0f, actionCallback);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveBehindThenExecute(Unit unit, UnitActionCallback actionCallback)
    {
      MoveBehindThenExecute(unit, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveBehindThenExecute(GameObject go, UnitActionCallback actionCallback)
    {
      MoveBehindThenExecute(go, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveBehindThenExecute(Unit unit, UnitActionCallback actionCallback, int millisTimeout)
    {
      MoveToThenExecute(unit, 3.141593f, actionCallback);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveBehindThenExecute(GameObject go, UnitActionCallback actionCallback, int millisTimeout)
    {
      MoveToThenExecute(go, 3.141593f, actionCallback);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, float angle, UnitActionCallback actionCallback)
    {
      MoveToThenExecute(unit, angle, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(GameObject go, float angle, UnitActionCallback actionCallback)
    {
      MoveToThenExecute(go, angle, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, float angle, UnitActionCallback callback, int millisTimeout)
    {
      if(!CheckBrain())
        return;
      Target = unit;
      AIMoveIntoAngleThenExecAction angleThenExecAction = new AIMoveIntoAngleThenExecAction(this, angle, callback);
      angleThenExecAction.TimeoutMillis = millisTimeout;
      m_brain.CurrentAction = angleThenExecAction;
    }

    /// <summary>
    /// Moves to the given gameobject and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(GameObject go, float angle, UnitActionCallback callback, int millisTimeout)
    {
      if(!CheckBrain())
        return;
      AIMoveToGameObjectIntoAngleThenExecAction angleThenExecAction =
        new AIMoveToGameObjectIntoAngleThenExecAction(this, go, angle, callback);
      angleThenExecAction.TimeoutMillis = millisTimeout;
      m_brain.CurrentAction = angleThenExecAction;
    }

    /// <summary>
    /// Moves to the given target and once within the given range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, SimpleRange range, UnitActionCallback actionCallback)
    {
      MoveToThenExecute(unit, range, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within the given range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(GameObject go, SimpleRange range, UnitActionCallback actionCallback)
    {
      MoveToThenExecute(go, range, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within the given range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, SimpleRange range, UnitActionCallback actionCallback, int millisTimeout)
    {
      if(!CheckBrain())
        return;
      Target = unit;
      IBrain brain = m_brain;
      AIMoveIntoRangeThenExecAction rangeThenExecAction1 =
        new AIMoveIntoRangeThenExecAction(this, range, actionCallback);
      rangeThenExecAction1.TimeoutMillis = millisTimeout;
      AIMoveIntoRangeThenExecAction rangeThenExecAction2 = rangeThenExecAction1;
      brain.CurrentAction = rangeThenExecAction2;
    }

    /// <summary>
    /// Moves to the given target and once within the given range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(GameObject go, SimpleRange range, UnitActionCallback actionCallback,
      int millisTimeout)
    {
      if(!CheckBrain())
        return;
      IBrain brain = m_brain;
      AIMoveIntoRangeOfGOThenExecAction goThenExecAction1 =
        new AIMoveIntoRangeOfGOThenExecAction(this, go, range, actionCallback);
      goThenExecAction1.TimeoutMillis = millisTimeout;
      AIMoveIntoRangeOfGOThenExecAction goThenExecAction2 = goThenExecAction1;
      brain.CurrentAction = goThenExecAction2;
    }

    /// <summary>
    /// Moves this Unit to the given position and then goes Idle
    /// </summary>
    /// <param name="pos"></param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenIdle(ref Vector3 pos)
    {
      MoveToThenEnter(ref pos, BrainState.Idle);
    }

    /// <summary>
    /// Moves this Unit to the given position and then goes Idle
    /// </summary>
    /// <param name="pos"></param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenIdle(IHasPosition pos)
    {
      MoveToThenEnter(pos, BrainState.Idle);
    }

    /// <summary>
    /// Moves this Unit to the given position and then assumes arrivedState
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="arrivedState">The BrainState to enter once arrived</param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenEnter(ref Vector3 pos, BrainState arrivedState)
    {
      MoveToThenEnter(ref pos, true, arrivedState);
    }

    /// <summary>
    /// Moves this Unit to the given position and then assumes arrivedState
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="arrivedState">The BrainState to enter once arrived</param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenEnter(ref Vector3 pos, bool findPath, BrainState arrivedState)
    {
      if(!CheckBrain())
        return;
      m_Movement.MoveTo(pos, findPath);
      m_brain.CurrentAction = new AIMoveThenEnterAction(this, arrivedState);
    }

    /// <summary>
    /// Moves this Unit to the given position and then assumes arrivedState
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="arrivedState">The BrainState to enter once arrived</param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenEnter(IHasPosition pos, BrainState arrivedState)
    {
      MoveToThenEnter(pos, true, arrivedState);
    }

    /// <summary>
    /// Moves this Unit to the given position and then assumes arrivedState
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="arrivedState">The BrainState to enter once arrived</param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenEnter(IHasPosition pos, bool findPath, BrainState arrivedState)
    {
      if(!CheckBrain())
        return;
      m_Movement.MoveTo(pos.Position, findPath);
      m_brain.CurrentAction = new AIMoveThenEnterAction(this, arrivedState);
    }

    /// <summary>
    /// Idles for the given time and then executes the given action.
    /// Also will unset the current Target and stop fighting.
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void IdleThenExecute(int millis, Action action)
    {
      IdleThenExecute(millis, action, ProcTriggerFlags.None);
    }

    /// <summary>
    /// Idles for the given time and then executes the given action.
    /// Also will unset the current Target and stop fighting.
    /// </summary>
    /// <param name="interruptFlags">What can interrupt the action.</param>
    /// <remarks>Requires Brain</remarks>
    public void IdleThenExecute(int millis, Action action, ProcTriggerFlags interruptFlags)
    {
      if(!CheckBrain())
        return;
      Target = null;
      m_brain.CurrentAction = new AITemporaryIdleAction(millis, interruptFlags, () =>
      {
        m_brain.StopCurrentAction();
        action();
      });
    }

    /// <summary>
    /// Idles for the given time before resuming its normal activities
    /// Also will unset the current Target and stop fighting.
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void Idle(int millis)
    {
      Idle(millis, ProcTriggerFlags.None);
    }

    /// <summary>
    /// Idles until the given flags have been triggered.
    /// Also will unset the current Target and stop fighting.
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void Idle(ProcTriggerFlags interruptFlags)
    {
      Idle(int.MaxValue, interruptFlags);
    }

    /// <summary>
    /// Idles for the given time before resuming its normal activities
    /// Also will unset the current Target and stop fighting.
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void Idle(int millis, ProcTriggerFlags interruptFlags)
    {
      if(!CheckBrain())
        return;
      Target = null;
      m_brain.CurrentAction = new AITemporaryIdleAction(millis, interruptFlags, () => m_brain.StopCurrentAction());
    }

    protected bool CheckBrain()
    {
      if(m_brain != null)
        return true;
      Say("I do not have a Brain.");
      return false;
    }

    /// <summary>
    /// The last time when this Unit was still actively Fighting
    /// </summary>
    public DateTime LastCombatTime
    {
      get { return m_lastCombatTime; }
      set { m_lastCombatTime = value; }
    }

    public int MillisSinceLastCombatAction
    {
      get { return (DateTime.Now - m_lastCombatTime).ToMilliSecondsInt(); }
    }

    /// <summary>
    /// While in combat, this method will reset the current swing delay (swing timer is reset)
    /// </summary>
    public void ResetSwingDelay()
    {
      m_lastCombatTime = m_lastUpdateTime;
    }

    public void CancelPendingAbility()
    {
      if(m_spellCast == null || !m_spellCast.IsPending)
        return;
      m_spellCast.Cancel(SpellFailedReason.DontReport);
    }

    /// <summary>
    /// The spell that is currently being triggered automatically by the CombatTimer
    /// </summary>
    public Spell AutorepeatSpell
    {
      get { return m_AutorepeatSpell; }
      set
      {
        if(m_AutorepeatSpell == value)
          return;
        m_AutorepeatSpell = value;
        if(value != null)
        {
          if(!value.IsRangedAbility)
            return;
          SheathType = SheathType.Ranged;
        }
        else
          SheathType = SheathType.Melee;
      }
    }

    /// <summary>
    /// Whether this Unit is currently attacking with a ranged weapon
    /// </summary>
    public bool IsUsingRangedWeapon
    {
      get
      {
        if(m_AutorepeatSpell != null)
          return m_AutorepeatSpell.IsRangedAbility;
        return false;
      }
    }

    /// <summary>Amount of extra attacks to hit on next thit</summary>
    public int ExtraAttacks
    {
      get { return m_extraAttacks; }
      set { m_extraAttacks = value; }
    }

    /// <summary>Adds damage mods to the given AttackAction</summary>
    public virtual void OnAttack(DamageAction action)
    {
      for(int index = AttackEventHandlers.Count - 1; index >= 0; --index)
        AttackEventHandlers[index].OnAttack(action);
    }

    /// <summary>Adds damage mods to the given AttackAction</summary>
    public virtual void OnDefend(DamageAction action)
    {
      IsSitting = false;
      for(int index = AttackEventHandlers.Count - 1; index >= 0; --index)
        AttackEventHandlers[index].OnDefend(action);
    }

    /// <summary>Adds damage mods to the given AttackAction</summary>
    public virtual int AddHealingModsToAction(int healValue, SpellEffect effect, DamageSchool school)
    {
      return healValue;
    }

    internal DamageAction GetUnusedAction()
    {
      if(m_DamageAction == null || m_DamageAction.ReferenceCount > 0)
        return new DamageAction(this);
      return m_DamageAction;
    }

    /// <summary>
    /// Whether this unit has an ability pending for the given weapon (Heroic Strike for melee, Poison Dart for throwing, Stun Shot for ranged weapons etc)
    /// </summary>
    public bool UsesPendingAbility(IAsda2Weapon weapon)
    {
      if(m_spellCast != null && m_spellCast.IsPending)
        return m_spellCast.GetWeapon() == weapon;
      return false;
    }

    /// <summary>Strike using mainhand weapon</summary>
    public void Strike()
    {
      Strike(MainWeapon);
    }

    /// <summary>Strike using given weapon</summary>
    public void Strike(IAsda2Weapon weapon)
    {
      DamageAction unusedAction = GetUnusedAction();
      Unit target = m_target;
      Strike(weapon, unusedAction, target);
    }

    /// <summary>Strike the target using mainhand weapon</summary>
    public void Strike(Unit target)
    {
      Strike(MainWeapon, target);
    }

    /// <summary>Strike the target using given weapon</summary>
    public void Strike(IAsda2Weapon weapon, Unit target)
    {
      Strike(weapon, GetUnusedAction(), target);
    }

    public void Strike(DamageAction action, Unit target)
    {
      Strike(MainWeapon, action, target);
    }

    /// <summary>
    /// Do a single attack using the given weapon and action on the target
    /// </summary>
    public void Strike(IAsda2Weapon weapon, DamageAction action, Unit target)
    {
      IsInCombat = true;
      if(UsesPendingAbility(weapon))
      {
        int num1 = (int) m_spellCast.Perform();
      }
      else
      {
        int num2 = (int) Strike(weapon, action, target, null);
      }
    }

    /// <summary>
    /// Do a single attack on the target using given weapon and ability.
    /// </summary>
    public ProcHitFlags Strike(IAsda2Weapon weapon, Unit target, SpellCast ability)
    {
      return Strike(weapon, GetUnusedAction(), target, ability);
    }

    /// <summary>
    /// Do a single attack on the target using given weapon, ability and action.
    /// </summary>
    public ProcHitFlags Strike(IAsda2Weapon weapon, DamageAction action, Unit target, SpellCast ability)
    {
      ProcHitFlags procHitFlags = ProcHitFlags.None;
      EnsureContext();
      if(!IsAlive || !target.IsInContext || !target.IsAlive)
        return procHitFlags;
      if(weapon == null)
      {
        log.Info("Trying to strike without weapon: " + this);
        return procHitFlags;
      }

      target.IsInCombat = true;
      action.Victim = target;
      action.Attacker = this;
      action.Weapon = weapon;
      if(ability != null)
      {
        action.Schools = ability.Spell.SchoolMask;
        action.SpellEffect = ability.Spell.Effects[0];
        GetWeaponDamage(action, weapon, ability, 0);
        procHitFlags = action.DoAttack();
        if(ability.Spell.AttributesExC.HasFlag(SpellAttributesExC.RequiresTwoWeapons) && m_offhandWeapon != null)
        {
          action.Reset(this, target, m_offhandWeapon);
          GetWeaponDamage(action, m_offhandWeapon, ability, 0);
          procHitFlags |= action.DoAttack();
          m_lastOffhandStrike = Environment.TickCount;
        }
      }
      else
      {
        ++m_extraAttacks;
        do
        {
          GetWeaponDamage(action, weapon, null, 0);
          action.Schools = weapon.Damages.AllSchools();
          if(action.Schools == DamageSchoolMask.None)
            action.Schools = DamageSchoolMask.Physical;
          int num = (int) action.DoAttack();
        } while(--m_extraAttacks > 0);
      }

      action.OnFinished();
      return procHitFlags;
    }

    /// <summary>Returns random damage for the given weapon</summary>
    public void GetWeaponDamage(DamageAction action, IAsda2Weapon weapon, SpellCast usedAbility, int targetNo = 0)
    {
      int num1 = weapon != m_offhandWeapon
        ? Utility.Random((int) MinDamage, (int) MaxDamage)
        : Utility.Random((int) MinOffHandDamage, (int) MaxOffHandDamage + 1);
      if(this is NPC)
        num1 = (int) (num1 * (double) NPCMgr.DefaultNPCDamageFactor + 0.999998986721039);
      if(usedAbility != null && usedAbility.IsCasting)
      {
        int num2 = 0;
        foreach(SpellEffectHandler handler in usedAbility.Handlers)
        {
          if(handler.Effect.IsStrikeEffectFlat)
            num1 += handler.CalcDamageValue(targetNo);
          else if(handler.Effect.IsStrikeEffectPct)
            num2 += handler.CalcDamageValue(targetNo);
        }

        action.Damage = num2 <= 0 ? num1 : (num1 * num2 + 50) / 100;
        foreach(SpellEffectHandler handler in usedAbility.Handlers)
        {
          if(handler is WeaponDamageEffectHandler)
            ((WeaponDamageEffectHandler) handler).OnHit(action);
        }
      }
      else
        action.Damage = num1;
    }

    /// <summary>Does spell-damage to this Unit</summary>
    public DamageAction DealSpellDamage(Unit attacker, SpellEffect effect, int dmg, bool addDamageBonuses = true,
      bool mayCrit = true, bool forceCrit = false, bool clearAction = true)
    {
      EnsureContext();
      if(!IsAlive)
        return null;
      if(attacker != null && !attacker.IsInContext)
        attacker = null;
      if(attacker is NPC)
        dmg = (int) (dmg * (double) NPCMgr.DefaultNPCDamageFactor + 0.999998986721039);
      DamageSchool school = effect == null ? DamageSchool.Physical : GetLeastResistantSchool(effect.Spell);
      if(IsEvading || IsImmune(school) || (IsInvulnerable || !IsAlive))
        return null;
      DamageAction unusedAction = GetUnusedAction();
      unusedAction.Attacker = attacker;
      unusedAction.HitFlags = HitFlags.NormalSwing;
      unusedAction.VictimState = VictimState.Miss;
      unusedAction.Weapon = null;
      if(effect != null)
      {
        unusedAction.UsedSchool = school;
        unusedAction.Schools = effect.Spell.SchoolMask;
        unusedAction.IsDot = effect.IsPeriodic;
      }
      else
      {
        unusedAction.UsedSchool = DamageSchool.Physical;
        unusedAction.Schools = DamageSchoolMask.Physical;
        unusedAction.IsDot = false;
      }

      unusedAction.Damage = dmg;
      float def = (float) ((Asda2MagicDefence + (double) Asda2Defence) / 2.0);
      Character attacker1 = unusedAction.Attacker as Character;
      if(attacker1 != null)
      {
        switch(attacker1.Archetype.ClassId)
        {
          case ClassId.OHS:
            def = Asda2Defence;
            break;
          case ClassId.Spear:
            def = Asda2Defence;
            break;
          case ClassId.THS:
            def = Asda2Defence;
            break;
          case ClassId.Crossbow:
            def = Asda2Defence;
            break;
          case ClassId.Bow:
            def = Asda2Defence;
            break;
          case ClassId.Balista:
            def = Asda2Defence;
            break;
          case ClassId.AtackMage:
            def = Asda2MagicDefence;
            break;
          case ClassId.SupportMage:
            def = Asda2MagicDefence;
            break;
          case ClassId.HealMage:
            def = Asda2MagicDefence;
            break;
        }
      }

      unusedAction.ResistPct =
        DamageAction.CalcResistPrc(def, unusedAction.Damage, GetResistChancePct(this, unusedAction.UsedSchool));
      unusedAction.Absorbed = 0;
      unusedAction.SpellEffect = effect;
      unusedAction.Victim = this;
      if(attacker != null)
        ++attacker.DeathPrevention;
      ++DeathPrevention;
      try
      {
        if(attacker != null)
        {
          int num1 = Utility.Random(1, 10000);
          int num2 = unusedAction.CalcHitChance();
          if(num1 > num2)
          {
            unusedAction.Miss();
          }
          else
          {
            int num3 = unusedAction.CalcBlockChance();
            if(num1 > num2 - num3)
            {
              unusedAction.Block();
            }
            else
            {
              int num4 = unusedAction.CalcCritChance();
              if(forceCrit || num1 > num2 - num4 - num3)
                unusedAction.StrikeCritical();
            }

            if(addDamageBonuses)
              unusedAction.AddDamageMods();
          }

          OnDefend(unusedAction);
          attacker.OnAttack(unusedAction);
        }

        unusedAction.Resisted = (int) Math.Round(unusedAction.Damage * (double) unusedAction.ResistPct / 100.0);
        DoRawDamage(unusedAction);
      }
      finally
      {
        --DeathPrevention;
        if(attacker != null)
          --attacker.DeathPrevention;
        if(clearAction)
          unusedAction.OnFinished();
      }

      if(clearAction)
        return null;
      return unusedAction;
    }

    public float GetBaseCritChance(DamageSchool dmgSchool, Spell spell, IAsda2Weapon weapon)
    {
      Character character = this as Character;
      if(character != null)
        return character.CritChanceMeleePct;
      NPC npc = this as NPC;
      if(npc != null)
      {
        switch(npc.Entry.Rank)
        {
          case CreatureRank.Normal:
            return 200f;
          case CreatureRank.Elite:
            return 1000f;
          case CreatureRank.Boss:
            return 1500f;
          case CreatureRank.WorldBoss:
            return 2000f;
        }
      }

      return 100f;
    }

    /// <summary>
    /// Calculates this Unit's chance to resist the given school.
    /// Value is between 0 and 100
    /// </summary>
    public float GetResistChancePct(Unit attacker, DamageSchool school)
    {
      int resistance = GetResistance(school);
      int num1;
      if(attacker != null)
      {
        num1 = Math.Max(1, attacker.Level);
        resistance -= attacker.GetTargetResistanceMod(school);
      }
      else
        num1 = 1;

      float num2 = (float) (Math.Max(0, resistance) / (num1 * 5.0) * 0.75);
      if(num2 > 75.0)
        num2 = 75f;
      if(num2 < 0.0)
        num2 = 0.0f;
      return num2;
    }

    /// <summary>
    /// Returns percent * 100 of chance to dodge
    /// Modified by expertise.
    /// </summary>
    public int CalcDodgeChance(WorldObject attacker)
    {
      float num = !(this is Character) ? 5f : ((Character) this).DodgeChance;
      if(attacker is Character)
      {
        Character character = (Character) attacker;
        num = num - character.Expertise * 0.25f + character.IntMods[17];
      }

      return (int) (num * 100f);
    }

    /// <summary>
    /// Returns percent * 100 of chance to parry.
    /// Modified by expertise.
    /// </summary>
    /// <returns></returns>
    public int CalcParryChance(Unit attacker)
    {
      float parryChance = ParryChance;
      if(attacker is Character)
      {
        Character character = (Character) attacker;
        parryChance -= character.Expertise * 0.25f;
      }

      return (int) (parryChance * 100f);
    }

    /// <summary>Modified by victim's resilience</summary>
    /// <param name="dmg"></param>
    /// <param name="victim"></param>
    /// <param name="effect"></param>
    /// <returns></returns>
    public virtual float CalcCritDamage(float dmg, Unit victim, SpellEffect effect)
    {
      int num = 200 - (int) victim.GetResiliencePct() +
                (victim.GetIntMod(StatModifierInt.CritDamageBonusPct) + CritDamageBonusPrc);
      return (float) ((dmg * (double) num + 50.0) / 100.0);
    }

    /// <summary>
    /// whether this Unit resists a debuff (independent on resistances)
    /// </summary>
    public bool CheckDebuffResist(int attackerLevel, DamageSchool school)
    {
      return Utility.Random(0, 100) < GetDebuffResistance(school) - GetAttackerSpellHitChanceMod(school);
    }

    /// <summary>
    /// whether this Unit is currently in Combat.
    /// If it is actively fighting (rather than being forced into CombatMode),
    /// IsFighting must be true.
    /// </summary>
    public bool IsInCombat
    {
      get { return m_isInCombat; }
      set
      {
        if(m_isInCombat == value)
          return;
        if(m_isInCombat = value)
        {
          UnitFlags |= UnitFlags.Combat;
          StandState = StandState.Stand;
          m_auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);
          if(m_spellCast != null)
          {
            Spell spell = m_spellCast.Spell;
            if(spell != null && spell.RequiresCasterOutOfCombat)
              m_spellCast.Cancel(SpellFailedReason.Interrupted);
          }

          if(HasMaster)
            Master.IsInCombat = true;
          OnEnterCombat();
          m_attackTimer.Start(1);
        }
        else
        {
          if(this is NPC)
            IsFighting = false;
          CancelPendingAbility();
          UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable |
                       UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation |
                       UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable |
                       UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 |
                       UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 |
                       UnitFlags.SelectableNotAttackable_3 | UnitFlags.TaxiFlight | UnitFlags.Disarmed |
                       UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable |
                       UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 |
                       UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          m_attackTimer.Stop();
          OnLeaveCombat();
        }

        this.UpdatePowerRegen();
      }
    }

    /// <summary>
    /// Indicates whether this Unit is currently trying to swing at its target.
    /// If <c>IsInCombat</c> is set but Unit is not fighting, it will leave Combat mode after <c>CombatDeactivationDelay</c> without combat activity.
    /// </summary>
    public bool IsFighting
    {
      get { return m_isFighting; }
      set
      {
        if(m_isFighting == value)
          return;
        if(m_isFighting = value)
        {
          if(m_target == null)
          {
            m_isFighting = false;
          }
          else
          {
            Dismount();
            if(this is NPC)
              IsInCombat = true;
            else
              m_attackTimer.Start(1);
          }
        }
        else
        {
          CancelPendingAbility();
          NPC npc = this as NPC;
          if(npc != null)
            npc.Brain.StopCurrentAction();
          CheckCombatState();
        }
      }
    }

    /// <summary>
    /// Tries to land a mainhand hit + maybe offhand hit on the current Target
    /// </summary>
    protected virtual void CombatTick(int timeElapsed)
    {
      if(IsUsingSpell && !m_spellCast.IsPending)
        m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
      else if(!CheckCombatState())
      {
        if(!m_isInCombat)
          return;
        m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
      }
      else if(!CanDoHarm || !CanMelee)
      {
        m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
      }
      else
      {
        Unit target = m_target;
        if(target == null || target.IsDead)
        {
          IsFighting = false;
          if(!(this is NPC))
            return;
          Movement.MayMove = true;
        }
        else
        {
          int tickCount = Environment.TickCount;
          bool usingRangedWeapon = IsUsingRangedWeapon;
          int num1 = m_lastStrike + (usingRangedWeapon ? RangedAttackTime : MainHandAttackTime) - tickCount;
          if(num1 <= 0)
          {
            if(this is NPC)
              Movement.MayMove = true;
            float distanceSq = GetDistanceSq(target);
            IAsda2Weapon weapon = usingRangedWeapon ? m_RangedWeapon : m_mainWeapon;
            if(weapon != null)
            {
              if(IsInAttackRangeSq(weapon, target, distanceSq))
              {
                if(m_AutorepeatSpell != null)
                {
                  if(!IsMoving)
                  {
                    SpellCast.TargetFlags = SpellTargetFlags.Unit;
                    SpellCast.SelectedTarget = target;
                    int num2 = (int) SpellCast.Start(m_AutorepeatSpell, false);
                    m_lastStrike = tickCount;
                    num1 += RangedAttackTime;
                  }
                }
                else
                {
                  Character character = this as Character;
                  if(character != null)
                    character.IsMoving = false;
                  Strike(weapon);
                  m_lastStrike = tickCount;
                  num1 += MainHandAttackTime;
                  if(this is NPC)
                    Movement.MayMove = false;
                }
              }
              else
              {
                if(UsesPendingAbility(weapon))
                  m_spellCast.Cancel(SpellFailedReason.OutOfRange);
                if(this is Character)
                  CombatHandler.SendAttackSwingNotInRange(this as Character);
                else if(this is NPC)
                  Brain.OnCombatTargetOutOfRange();
              }
            }
          }

          m_attackTimer.Start(num1 <= 0 ? 1000 : num1);
        }
      }
    }

    /// <summary>
    /// Checks whether the Unit can attack.
    /// Also deactivates combat mode, if unit has left combat for long enough.
    /// </summary>
    protected virtual bool CheckCombatState()
    {
      if(m_comboTarget != null && (!m_comboTarget.IsInContext || !m_comboTarget.IsAlive))
        ResetComboPoints();
      if(m_target == null || !CanHarm(m_target))
        IsFighting = false;
      else if(!CanSee(m_target))
      {
        Target = null;
        IsFighting = false;
      }
      else if(!CanDoHarm)
        return false;

      return m_isFighting;
    }

    /// <summary>
    /// Resets the attack timer to delay the next strike by the current weapon delay,
    /// if Unit is fighting.
    /// </summary>
    public void ResetAttackTimer()
    {
      if(m_isFighting)
      {
        int num = m_offhandWeapon == null ? MainHandAttackTime : Math.Min(MainHandAttackTime, OffHandAttackTime);
        if(m_RangedWeapon != null && m_RangedWeapon.IsRanged)
          num = Math.Min(num, RangedAttackTime);
        m_attackTimer.Start(num);
      }
      else
        m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
    }

    /// <summary>Is called whenever this Unit enters Combat mode</summary>
    protected virtual void OnEnterCombat()
    {
      SheathType = IsUsingRangedWeapon ? SheathType.Ranged : SheathType.Melee;
      StandState = StandState.Stand;
      m_lastCombatTime = m_lastUpdateTime;
      if(m_brain == null)
        return;
      m_brain.OnEnterCombat();
    }

    /// <summary>Is called whenever this Unit leaves Combat mode</summary>
    protected virtual void OnLeaveCombat()
    {
      ResetComboPoints();
      if(m_brain == null)
        return;
      m_brain.OnLeaveCombat();
    }

    /// <summary>
    /// Is called whenever this Unit receives any kind of damage
    /// 
    /// TODO: There is a small chance with each hit by your weapon that it will lose 1 durability point.
    /// TODO: There is a small chance with each spell cast that you will lose 1 durability point to your weapon.
    /// TODO: There is a small chance with each hit absorbed by your armor that it will lose 1 durability point.
    /// </summary>
    protected internal virtual void OnDamageAction(IDamageAction action)
    {
      IsSitting = false;
      if(!(action is DamageAction) || action.Attacker == null)
        return;
      if(action.ActualDamage <= 0)
        return;
      if(!action.IsDot)
      {
        Unit attacker = action.Attacker;
        if(action.Weapon != null)
        {
          int num = attacker.IsPvPing ? 1 : 0;
        }

        m_auras.RemoveByFlag(AuraInterruptFlags.OnDamage);
        attacker.m_lastCombatTime = attacker.m_lastUpdateTime;
        StandState = StandState.Stand;
        if(IsAlive)
        {
          IsInCombat = true;
          m_lastCombatTime = m_lastUpdateTime;
        }
      }

      TriggerProcOnDamageReceived(action);
    }

    private void TriggerProcOnDamageReceived(IDamageAction action)
    {
      ProcHitFlags hitFlags = action.IsCritical ? ProcHitFlags.CriticalHit : ProcHitFlags.None;
      ProcTriggerFlags flags = ProcTriggerFlags.ReceivedAnyDamage;
      if(action.IsDot)
      {
        action.Attacker.Proc(ProcTriggerFlags.DonePeriodicDamageOrHeal, this, action, true, hitFlags);
        flags |= ProcTriggerFlags.ReceivedPeriodicDamageOrHeal;
      }

      Proc(flags, action.Attacker, action, true, hitFlags);
    }

    public float GetAttackRange(IAsda2Weapon weapon, Unit target)
    {
      return weapon.MaxRange + CombatReach + target.CombatReach;
    }

    public float GetBaseAttackRange(Unit target)
    {
      return MaxAttackRange + target.CombatReach;
    }

    public bool IsInBaseAttackRange(Unit target)
    {
      float distanceSq = GetDistanceSq(target);
      float baseAttackRange = GetBaseAttackRange(target);
      return distanceSq <= baseAttackRange * (double) baseAttackRange;
    }

    public bool IsInBaseAttackRangeSq(Unit target, float distSq)
    {
      float baseAttackRange = GetBaseAttackRange(target);
      return distSq <= baseAttackRange * (double) baseAttackRange;
    }

    public float GetMinAttackRange(IAsda2Weapon weapon, Unit target)
    {
      return weapon.MinRange;
    }

    /// <summary>
    /// Returns whether the given Object is in range of Main- or Extra (Ranged)- weapon
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool IsInAttackRange(Unit target)
    {
      float distanceSq = GetDistanceSq(target);
      return IsInAttackRangeSq(target, distanceSq);
    }

    /// <summary>
    /// Whether the suitable target is in reach to be attacked
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool CanReachForCombat(Unit target)
    {
      if(!CanMove)
        return IsInAttackRange(target);
      return true;
    }

    public bool IsInAttackRangeSq(Unit target, float distSq)
    {
      if(!CanMelee)
      {
        if(this is NPC)
          return IsInBaseAttackRangeSq(target, distSq);
        return false;
      }

      if(UsesRangedWeapon && IsInAttackRangeSq(m_RangedWeapon, target, distSq))
        return true;
      return IsInMeleeAttackRangeSq(m_mainWeapon, target, distSq);
    }

    public bool IsInMaxRange(Spell spell, WorldObject target)
    {
      float spellMaxRange = GetSpellMaxRange(spell, target);
      return GetDistanceSq(target) <= spellMaxRange * (double) spellMaxRange;
    }

    public bool IsInSpellRange(Spell spell, WorldObject target)
    {
      float spellMaxRange = GetSpellMaxRange(spell, target);
      float distanceSq = GetDistanceSq(target);
      if(spell.Range.MinDist > 0.0)
      {
        float minDist = spell.Range.MinDist;
        if(distanceSq < minDist * (double) minDist)
          return false;
      }

      return distanceSq <= spellMaxRange * (double) spellMaxRange;
    }

    /// <summary>Melee has no Min range</summary>
    public bool IsInMeleeAttackRangeSq(IAsda2Weapon weapon, Unit target, float distSq)
    {
      float attackRange = GetAttackRange(weapon, target);
      return distSq <= attackRange * (double) attackRange;
    }

    public bool IsInAttackRangeSq(IAsda2Weapon weapon, Unit target, float distSq)
    {
      float range = GetAttackRange(weapon, target);
      if(UsesPendingAbility(weapon))
        range = GetSpellMaxRange(m_spellCast.Spell, range);
      if(distSq > range * (double) range)
        return false;
      if(weapon.IsRanged)
      {
        float minAttackRange = GetMinAttackRange(weapon, target);
        if(distSq < minAttackRange * (double) minAttackRange)
          return false;
      }

      return true;
    }

    public bool IsInRange(SimpleRange range, WorldObject obj)
    {
      float distanceSq = GetDistanceSq(obj);
      if(distanceSq > range.MaxDist * (double) range.MaxDist)
        return false;
      if(range.MinDist >= 1.0)
        return distanceSq >= range.MinDist * (double) range.MinDist;
      return true;
    }

    public virtual float AggroBaseRange
    {
      get { return NPCEntry.AggroBaseRangeDefault + BoundingRadius; }
    }

    public virtual float GetAggroRange(Unit victim)
    {
      return Math.Max(AggroBaseRange + (Level - victim.Level) * NPCEntry.AggroRangePerLevel,
        NPCEntry.AggroRangeMinDefault);
    }

    public float GetAggroRangeSq(Unit victim)
    {
      float aggroRange = GetAggroRange(victim);
      return aggroRange * aggroRange;
    }

    /// <summary>
    /// Checks for hostility etc
    /// 
    /// TODO: Restrict interference in Duels
    /// </summary>
    public SpellFailedReason CanCastSpellOn(Unit target, Spell spell)
    {
      bool flag = CanHarm(target);
      if((!flag || spell.HasHarmfulEffects) && (flag || spell.HasBeneficialEffects))
        return SpellFailedReason.Ok;
      return !flag ? SpellFailedReason.TargetEnemy : SpellFailedReason.TargetFriendly;
    }

    /// <summary>
    /// The maximum distance in yards to a valid attackable target
    /// </summary>
    public float CombatReach
    {
      get { return GetFloat(UnitFields.COMBATREACH); }
      set { SetFloat(UnitFields.COMBATREACH, value); }
    }

    public virtual float MaxAttackRange
    {
      get { return CombatReach + m_mainWeapon.MaxRange; }
    }

    /// <summary>
    /// Modifies the damage for the given school by the given delta.
    /// </summary>
    protected internal virtual void AddDamageDoneModSilently(DamageSchool school, int delta)
    {
    }

    /// <summary>
    /// Modifies the damage for the given school by the given delta.
    /// </summary>
    public void AddDamageDoneMod(DamageSchool school, int delta)
    {
      AddDamageDoneModSilently(school, delta);
    }

    /// <summary>
    /// Modifies the damage for the given school by the given delta.
    /// </summary>
    protected internal virtual void RemoveDamageDoneModSilently(DamageSchool school, int delta)
    {
    }

    /// <summary>
    /// Modifies the damage for the given school by the given delta.
    /// </summary>
    public void RemoveDamageDoneMod(DamageSchool school, int delta)
    {
      RemoveDamageDoneModSilently(school, delta);
    }

    protected internal virtual void ModDamageDoneFactorSilently(DamageSchool school, float delta)
    {
    }

    public virtual float GetDamageDoneFactor(DamageSchool school)
    {
      return 1f;
    }

    public virtual int GetDamageDoneMod(DamageSchool school)
    {
      return 0;
    }

    /// <summary>
    /// Adds/Removes a flat modifier to all of the given damage schools
    /// </summary>
    public void AddDamageDoneMod(uint[] schools, int delta)
    {
      foreach(DamageSchool school in schools)
        AddDamageDoneModSilently(school, delta);
    }

    /// <summary>
    /// Adds/Removes a flat modifier to all of the given damage schools
    /// </summary>
    public void RemoveDamageDoneMod(uint[] schools, int delta)
    {
      foreach(DamageSchool school in schools)
        RemoveDamageDoneModSilently(school, delta);
    }

    public void ModDamageDoneFactor(DamageSchool school, float delta)
    {
      ModDamageDoneFactorSilently(school, delta);
    }

    /// <summary>
    /// Adds/Removes a percent modifier to all of the given damage schools
    /// </summary>
    public void ModDamageDoneFactor(uint[] schools, float delta)
    {
      foreach(DamageSchool school in schools)
        ModDamageDoneFactorSilently(school, delta);
    }

    /// <summary>
    /// Get total damage, after adding/subtracting all modifiers (is not used for DoT)
    /// </summary>
    public int GetFinalDamage(DamageSchool school, int dmg, Spell spell = null)
    {
      if(spell != null)
        dmg = Auras.GetModifiedInt(SpellModifierType.SpellPower, spell, dmg);
      return dmg;
    }

    /// <summary>
    /// Whether this Unit currently has a ranged weapon equipped
    /// </summary>
    public bool UsesRangedWeapon
    {
      get
      {
        if(m_RangedWeapon != null)
          return m_RangedWeapon.IsRanged;
        return false;
      }
    }

    /// <summary>
    /// Whether this Character is currently using DualWield (attacking with 2 melee weapons)
    /// </summary>
    public bool UsesDualWield
    {
      get
      {
        if(SheathType != SheathType.Ranged)
          return m_offhandWeapon != null;
        return false;
      }
    }

    public int MinMagicDamage { get; set; }

    public int MaxMagicDamage { get; set; }

    /// <summary>
    /// The Unit's current mainhand weapon
    /// Is set by the Unit's ItemInventory
    /// </summary>
    public IAsda2Weapon MainWeapon
    {
      get { return m_mainWeapon; }
      set
      {
        if(value == m_mainWeapon)
          return;
        if(value == null)
        {
          m_mainWeapon = GenericWeapon.Fists;
          this.UpdateMainDamage();
          this.UpdateMainAttackTime();
        }
        else
        {
          m_mainWeapon = value;
          Asda2Item mainWeapon = m_mainWeapon as Asda2Item;
          if(mainWeapon != null)
          {
            if(!mainWeapon.Template.IsWeapon)
              return;
            this.UpdateMainDamage();
            this.UpdateMainAttackTime();
          }
          else
          {
            this.UpdateMainDamage();
            this.UpdateMainAttackTime();
          }
        }
      }
    }

    public IAsda2Weapon GetWeapon(EquipmentSlot slot)
    {
      switch(slot)
      {
        case EquipmentSlot.MainHand:
          return m_mainWeapon;
        case EquipmentSlot.OffHand:
          return m_offhandWeapon;
        case EquipmentSlot.ExtraWeapon:
          return m_RangedWeapon;
        default:
          return null;
      }
    }

    public IAsda2Weapon GetWeapon(InventorySlotType slot)
    {
      switch(slot)
      {
        case InventorySlotType.WeaponRanged:
          return m_RangedWeapon;
        case InventorySlotType.WeaponMainHand:
          return m_mainWeapon;
        case InventorySlotType.WeaponOffHand:
          return m_offhandWeapon;
        default:
          return null;
      }
    }

    public void SetWeapon(InventorySlotType slot, IAsda2Weapon weapon)
    {
      if(slot != InventorySlotType.WeaponMainHand)
        return;
      MainWeapon = weapon;
    }

    /// <summary>Whether this Unit is allowed to melee at all</summary>
    public bool CanMelee
    {
      get
      {
        if(MeleePermissionCounter > 0)
          return m_canInteract;
        return false;
      }
    }

    /// <summary>If greater 0, may melee, else not</summary>
    public int MeleePermissionCounter { get; internal set; }

    public void IncMeleePermissionCounter()
    {
      ++MeleePermissionCounter;
    }

    public void DecMeleePermissionCounter()
    {
      --MeleePermissionCounter;
    }

    public bool MayCarry(InventorySlotTypeMask itemMask)
    {
      return (itemMask & DisarmMask) == InventorySlotTypeMask.None;
    }

    /// <summary>The mask of slots of currently disarmed items.</summary>
    public InventorySlotTypeMask DisarmMask
    {
      get { return m_DisarmMask; }
    }

    /// <summary>
    /// Disarms the weapon of the given type (WeaponMainHand, WeaponRanged or WeaponOffHand)
    /// </summary>
    public void SetDisarmed(InventorySlotType type)
    {
      InventorySlotTypeMask mask = type.ToMask();
      if(m_DisarmMask.HasAnyFlag(mask))
        return;
      m_DisarmMask |= mask;
      SetWeapon(type, null);
    }

    /// <summary>
    /// Rearms the weapon of the given type (WeaponMainHand, WeaponRanged or WeaponOffHand)
    /// </summary>
    public void UnsetDisarmed(InventorySlotType type)
    {
      InventorySlotTypeMask mask = type.ToMask();
      if(!m_DisarmMask.HasAnyFlag(mask))
        return;
      m_DisarmMask &= ~mask;
      SetWeapon(type, GetOrInvalidateItem(type));
    }

    /// <summary>
    /// Finds the item for the given slot.
    /// Unequips it and returns null, if it may not currently be used.
    /// </summary>
    protected virtual IAsda2Weapon GetOrInvalidateItem(InventorySlotType type)
    {
      return null;
    }

    /// <summary>Time in millis between 2 Main-hand strikes</summary>
    public int MainHandAttackTime
    {
      get { return GetInt32(UnitFields.BASEATTACKTIME); }
      set { SetInt32(UnitFields.BASEATTACKTIME, value); }
    }

    /// <summary>Time in millis between 2 Off-hand strikes</summary>
    public int OffHandAttackTime
    {
      get { return GetInt32(UnitFields.BASEATTACKTIME_2); }
      set { SetInt32(UnitFields.BASEATTACKTIME_2, value); }
    }

    /// <summary>Time in millis between 2 ranged strikes</summary>
    public int RangedAttackTime
    {
      get { return GetInt32(UnitFields.RANGEDATTACKTIME); }
      set { SetInt32(UnitFields.RANGEDATTACKTIME, value); }
    }

    public float MinDamage
    {
      get { return GetFloat(UnitFields.MINDAMAGE); }
      internal set { SetFloat(UnitFields.MINDAMAGE, value); }
    }

    public float MaxDamage
    {
      get { return GetFloat(UnitFields.MAXDAMAGE); }
      internal set { SetFloat(UnitFields.MAXDAMAGE, value); }
    }

    public float MinOffHandDamage
    {
      get { return GetFloat(UnitFields.MINOFFHANDDAMAGE); }
      internal set { SetFloat(UnitFields.MINOFFHANDDAMAGE, value); }
    }

    public float MaxOffHandDamage
    {
      get { return GetFloat(UnitFields.MAXOFFHANDDAMAGE); }
      internal set { SetFloat(UnitFields.MAXOFFHANDDAMAGE, value); }
    }

    public float MinRangedDamage
    {
      get { return GetFloat(UnitFields.MINRANGEDDAMAGE); }
      internal set { SetFloat(UnitFields.MINRANGEDDAMAGE, value); }
    }

    public float MaxRangedDamage
    {
      get { return GetFloat(UnitFields.MAXRANGEDDAMAGE); }
      internal set { SetFloat(UnitFields.MAXRANGEDDAMAGE, value); }
    }

    public int MeleeAttackPower
    {
      get { return GetInt32(UnitFields.ATTACK_POWER); }
      internal set { SetInt32(UnitFields.ATTACK_POWER, value); }
    }

    public int MeleeAttackPowerModsPos
    {
      get { return GetUInt16Low(UnitFields.ATTACK_POWER_MODS); }
      set
      {
        SetUInt16Low(UnitFields.ATTACK_POWER_MODS, (ushort) value);
        this.UpdateMeleeAttackPower();
      }
    }

    public int MeleeAttackPowerModsNeg
    {
      get { return GetUInt16High(UnitFields.ATTACK_POWER_MODS); }
      set
      {
        SetUInt16High(UnitFields.ATTACK_POWER_MODS, (ushort) value);
        this.UpdateMeleeAttackPower();
      }
    }

    public float MeleeAttackPowerMultiplier
    {
      get { return GetFloat(UnitFields.ATTACK_POWER_MULTIPLIER); }
      set
      {
        SetFloat(UnitFields.ATTACK_POWER_MULTIPLIER, value);
        this.UpdateMeleeAttackPower();
      }
    }

    public int TotalMeleeAP
    {
      get
      {
        return MathUtil.RoundInt((1f + MeleeAttackPowerMultiplier) *
                                 (MeleeAttackPower + MeleeAttackPowerModsPos - MeleeAttackPowerModsNeg));
      }
    }

    public int RangedAttackPower
    {
      get { return GetInt32(UnitFields.RANGED_ATTACK_POWER); }
      internal set { SetInt32(UnitFields.RANGED_ATTACK_POWER, value); }
    }

    public int RangedAttackPowerModsPos
    {
      get { return GetInt16Low(UnitFields.RANGED_ATTACK_POWER_MODS); }
      set
      {
        SetInt16Low(UnitFields.RANGED_ATTACK_POWER_MODS, (short) value);
        this.UpdateRangedAttackPower();
      }
    }

    public int RangedAttackPowerModsNeg
    {
      get { return GetInt16High(UnitFields.RANGED_ATTACK_POWER_MODS); }
      set
      {
        SetInt16High(UnitFields.RANGED_ATTACK_POWER_MODS, (short) value);
        this.UpdateRangedAttackPower();
      }
    }

    public float RangedAttackPowerMultiplier
    {
      get { return GetFloat(UnitFields.RANGED_ATTACK_POWER_MULTIPLIER); }
      set
      {
        SetFloat(UnitFields.RANGED_ATTACK_POWER_MULTIPLIER, value);
        this.UpdateRangedAttackPower();
      }
    }

    public int TotalRangedAP
    {
      get
      {
        return MathUtil.RoundInt((1f + RangedAttackPowerMultiplier) *
                                 (RangedAttackPower + RangedAttackPowerModsPos - RangedAttackPowerModsNeg));
      }
    }

    public float RandomMagicDamage
    {
      get { return Utility.Random(MinMagicDamage, MaxMagicDamage); }
    }

    public float RandomDamage
    {
      get { return Utility.Random(MinDamage, MaxDamage); }
    }

    /// <summary>
    /// Deals environmental damage to this Unit (cannot be resisted)
    /// </summary>
    public virtual void DealEnvironmentalDamage(EnviromentalDamageType dmgType, int amount)
    {
      DoRawDamage(new SimpleDamageAction
      {
        Damage = amount,
        Victim = this
      });
      CombatLogHandler.SendEnvironmentalDamage(this, dmgType, (uint) amount);
    }

    public void CalcFallingDamage(int speed)
    {
    }

    /// <summary>
    /// Deals damage, cancels damage-sensitive Auras, checks for spell interruption etc
    /// </summary>
    public void DoRawDamage(IDamageAction action)
    {
      if(m_FirstAttacker == null && action.Attacker != null)
        FirstAttacker = action.Attacker;
      if(m_damageTakenMods != null)
        action.Damage += m_damageTakenMods[(int) action.UsedSchool];
      if(m_damageTakenPctMods != null)
      {
        int damageTakenPctMod = m_damageTakenPctMods[(int) action.UsedSchool];
        if(damageTakenPctMod != 0)
          action.Damage -= (damageTakenPctMod * action.Damage + 50) / 100;
      }

      if(action.Spell != null && action.Spell.IsAreaSpell && AoEDamageModifierPct != 0)
        action.Damage -= (action.Damage * AoEDamageModifierPct + 50) / 100;
      action.Victim.OnDamageAction(action);
      int actualDamage = action.ActualDamage;
      if(actualDamage <= 0)
        return;
      if(m_brain != null)
        m_brain.OnDamageReceived(action);
      if(action.Attacker != null && action.Attacker.Brain != null)
        action.Attacker.m_brain.OnDamageDealt(action);
      LastDamageDelay = action.Spell == null ? 300 : (int) action.Spell.CastDelay;
      Health -= actualDamage;
      if(IsAlive)
        return;
      OnKilled(action);
    }

    /// <summary>
    /// Called after this unit has been killed by damage action
    /// </summary>
    /// <param name="action">Action which killed this unit</param>
    protected virtual void OnKilled(IDamageAction action)
    {
      TriggerProcOnKilled(action);
      LastKiller = action.Attacker;
    }

    private void TriggerProcOnKilled(IDamageAction killingAction)
    {
      if(YieldsXpOrHonor && killingAction.Attacker != null)
        killingAction.Attacker.Proc(ProcTriggerFlags.KilledTargetThatYieldsExperienceOrHonor, this, killingAction, true,
          ProcHitFlags.None);
      Proc(ProcTriggerFlags.Death, killingAction.Attacker, killingAction, true, ProcHitFlags.None);
    }

    [Initialization(InitializationPass.Tenth)]
    public static void InitSpeeds()
    {
    }

    /// <summary>Creates an array for a set of SpellMechanics</summary>
    protected static int[] CreateMechanicsArr()
    {
      return new int[MechanicCount];
    }

    protected static int[] CreateDamageSchoolArr()
    {
      return new int[DamageSchoolCount];
    }

    protected static int[] CreateDispelTypeArr()
    {
      return new int[DispelTypeCount];
    }

    /// <summary>
    /// Whether the Unit is allowed to cast spells that are not physical abilities
    /// </summary>
    public bool CanCastSpells
    {
      get { return m_canCastSpells; }
    }

    /// <summary>
    /// Whether the Unit is allowed to attack and use physical abilities
    /// </summary>
    public bool CanDoPhysicalActivity
    {
      get { return m_canDoPhysicalActivity; }
      private set
      {
        if(m_canDoPhysicalActivity != value)
          return;
        m_canDoPhysicalActivity = value;
        if(!value)
          return;
        IsFighting = false;
        if(m_spellCast == null || !m_spellCast.IsCasting || !m_spellCast.Spell.IsPhysicalAbility)
          return;
        m_spellCast.Cancel(SpellFailedReason.Pacified);
      }
    }

    public bool CanMove
    {
      get
      {
        if(m_canMove)
          return HasPermissionToMove;
        return false;
      }
    }

    /// <summary>
    /// Whether the owner or controlling AI allows this unit to move.
    /// Always returns true for uncontrolled players.
    /// </summary>
    public bool HasPermissionToMove
    {
      get
      {
        if(m_Movement != null)
          return m_Movement.MayMove;
        return true;
      }
      set
      {
        if(m_Movement == null)
          return;
        m_Movement.MayMove = value;
      }
    }

    /// <summary>
    /// Whether the Unit is currently evading (cannot be hit, generate threat etc)
    /// </summary>
    public bool IsEvading
    {
      get { return m_evades; }
      set
      {
        m_evades = value;
        if(value)
        {
          m_auras.RemoveOthersAuras();
          IncMechanicCount(SpellMechanic.Invulnerable, false);
        }
        else
          DecMechanicCount(SpellMechanic.Invulnerable, false);
      }
    }

    /// <summary>whether the Unit can be interacted with</summary>
    public bool CanInteract
    {
      get { return m_canInteract; }
    }

    /// <summary>
    /// 
    /// </summary>
    public override bool CanDoHarm
    {
      get
      {
        if(m_canHarm)
          return base.CanDoHarm;
        return false;
      }
    }

    /// <summary>Whether this Unit is currently stunned (!= rooted)</summary>
    public int Stunned
    {
      get
      {
        if(m_mechanics == null)
          return 0;
        return m_mechanics[12];
      }
      set
      {
        if(m_mechanics == null)
          m_mechanics = CreateMechanicsArr();
        if(value <= 0)
        {
          m_mechanics[12] = 1;
          DecMechanicCount(SpellMechanic.Stunned, false);
        }
        else
        {
          if(Stunned != 0)
            return;
          IncMechanicCount(SpellMechanic.Stunned, false);
          m_mechanics[12] = value;
        }
      }
    }

    public int Invulnerable
    {
      get
      {
        if(m_mechanics == null)
          return 0;
        return m_mechanics[25];
      }
      set
      {
        if(m_mechanics == null)
          m_mechanics = CreateMechanicsArr();
        if(value <= 0)
        {
          m_mechanics[25] = 1;
          DecMechanicCount(SpellMechanic.Invulnerable, false);
        }
        else
        {
          if(Invulnerable != 0)
            return;
          IncMechanicCount(SpellMechanic.Invulnerable, false);
          m_mechanics[25] = value;
        }
      }
    }

    /// <summary>
    /// Pacified units cannot attack or use physical abilities
    /// </summary>
    public int Pacified
    {
      get { return m_Pacified; }
      set
      {
        if(m_Pacified == value)
          return;
        if(value <= 0 && m_Pacified > 0)
          UnitFlags &= UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.Influenced |
                       UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob |
                       UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive |
                       UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced |
                       UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 |
                       UnitFlags.Stunned | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed |
                       UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable |
                       UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 |
                       UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
        else if(value > 0)
          UnitFlags |= UnitFlags.Pacified;
        m_Pacified = value;
        SetCanHarmState();
      }
    }

    /// <summary>Return whether the given Mechanic applies to the Unit</summary>
    public bool IsUnderInfluenceOf(SpellMechanic mechanic)
    {
      if(m_mechanics == null)
        return false;
      return m_mechanics[(int) mechanic] > 0;
    }

    /// <summary>
    /// Increase the mechanic modifier count for the given SpellMechanic
    /// </summary>
    public void IncMechanicCount(SpellMechanic mechanic, bool isCustom = false)
    {
      if(m_mechanics == null)
        m_mechanics = CreateMechanicsArr();
      if(m_mechanics[(int) mechanic] == 0)
      {
        if(!isCustom)
        {
          if(m_canMove && SpellConstants.MoveMechanics[(int) mechanic])
          {
            m_canMove = false;
            if(!IsPlayer)
              Target = null;
            UnitFlags |= UnitFlags.Stunned;
            CancelTaxiFlight();
            if(this is Character)
              MovementHandler.SendRooted((Character) this, 1);
            if(IsUsingSpell && SpellCast.Spell.InterruptFlags.HasFlag(InterruptFlags.OnStunned))
              SpellCast.Cancel(SpellFailedReason.Interrupted);
            StopMoving();
          }

          if(m_canInteract && SpellConstants.InteractMechanics[(int) mechanic])
            m_canInteract = false;
          if(m_canHarm && SpellConstants.HarmPreventionMechanics[(int) mechanic])
            SetCanHarmState();
          if(m_canCastSpells && SpellConstants.SpellCastPreventionMechanics[(int) mechanic])
          {
            m_canCastSpells = false;
            if(IsUsingSpell && SpellCast.Spell.InterruptFlags.HasFlag(InterruptFlags.OnSilence))
              SpellCast.Cancel(SpellFailedReason.Interrupted);
            if(!m_canDoPhysicalActivity && m_canHarm)
              SetCanHarmState();
          }
        }

        switch(mechanic)
        {
          case SpellMechanic.Disoriented:
            UnitFlags |= UnitFlags.Confused;
            break;
          case SpellMechanic.Fleeing:
            UnitFlags |= UnitFlags.Feared;
            break;
          case SpellMechanic.Silenced:
            UnitFlags |= UnitFlags.Silenced;
            break;
          case SpellMechanic.Frozen:
            AuraState |= AuraStateMask.Frozen;
            break;
          case SpellMechanic.Bleeding:
            AuraState |= AuraStateMask.Bleeding;
            break;
          case SpellMechanic.Mounted:
            UnitFlags |= UnitFlags.Mounted;
            SpeedFactor += MountSpeedMod;
            m_auras.RemoveByFlag(AuraInterruptFlags.OnMount);
            break;
          case SpellMechanic.Invulnerable:
            UnitFlags |= UnitFlags.SelectableNotAttackable;
            break;
          case SpellMechanic.Enraged:
            AuraState |= AuraStateMask.Enraged;
            break;
          case SpellMechanic.Custom_Immolate:
            AuraState |= AuraStateMask.Immolate;
            break;
        }
      }

      ++m_mechanics[(int) mechanic];
    }

    /// <summary>
    /// Decrease the mechanic modifier count for the given SpellMechanic
    /// </summary>
    public void DecMechanicCount(SpellMechanic mechanic, bool isCustom = false)
    {
      if(m_mechanics == null)
        return;
      int mechanic1 = m_mechanics[(int) mechanic];
      if(mechanic1 <= 0)
        return;
      m_mechanics[(int) mechanic] = mechanic1 - 1;
      if(mechanic1 != 1)
        return;
      if(!isCustom)
      {
        if(!m_canMove && SpellConstants.MoveMechanics[(int) mechanic] && !IsAnySetNoCheck(SpellConstants.MoveMechanics))
        {
          m_canMove = true;
          UnitFlags &= UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.Influenced |
                       UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob |
                       UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive |
                       UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced |
                       UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 |
                       UnitFlags.Pacified | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed |
                       UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable |
                       UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 |
                       UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          m_lastMoveTime = Environment.TickCount;
          if(this is Character)
            MovementHandler.SendUnrooted((Character) this);
        }

        if(!m_canInteract && SpellConstants.InteractMechanics[(int) mechanic] &&
           !IsAnySetNoCheck(SpellConstants.InteractMechanics))
          m_canInteract = true;
        if(!m_canHarm && SpellConstants.HarmPreventionMechanics[(int) mechanic])
          SetCanHarmState();
        if(!m_canCastSpells && SpellConstants.SpellCastPreventionMechanics[(int) mechanic] &&
           !IsAnySetNoCheck(SpellConstants.SpellCastPreventionMechanics))
        {
          m_canCastSpells = true;
          if(!m_canDoPhysicalActivity && !m_canHarm)
            SetCanHarmState();
        }
      }

      switch(mechanic)
      {
        case SpellMechanic.Disoriented:
          UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable |
                       UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation |
                       UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable |
                       UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 |
                       UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 |
                       UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight |
                       UnitFlags.Disarmed | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable |
                       UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 |
                       UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          break;
        case SpellMechanic.Fleeing:
          UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable |
                       UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation |
                       UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable |
                       UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 |
                       UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 |
                       UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight |
                       UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Possessed | UnitFlags.NotSelectable |
                       UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 |
                       UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          break;
        case SpellMechanic.Silenced:
          UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable |
                       UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation |
                       UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable |
                       UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 |
                       UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 |
                       UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused |
                       UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable |
                       UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 |
                       UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          break;
        case SpellMechanic.Frozen:
          AuraState ^= AuraStateMask.Frozen;
          break;
        case SpellMechanic.Bleeding:
          AuraState ^= AuraStateMask.Bleeding;
          break;
        case SpellMechanic.Mounted:
          UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable |
                       UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation |
                       UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable |
                       UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 |
                       UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 |
                       UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight |
                       UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed |
                       UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Flag_28_0x10000000 |
                       UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          SpeedFactor -= MountSpeedMod;
          break;
        case SpellMechanic.Invulnerable:
          UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.Influenced |
                       UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob |
                       UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive |
                       UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced |
                       UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 |
                       UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused |
                       UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable |
                       UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 |
                       UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          break;
        case SpellMechanic.Enraged:
          AuraState &= ~AuraStateMask.Enraged;
          break;
        case SpellMechanic.Custom_Immolate:
          AuraState ^= AuraStateMask.Immolate;
          break;
      }
    }

    /// <summary>
    /// Checks whether any of the mechanics of the given set are influencing the owner
    /// </summary>
    private bool IsAnySetNoCheck(bool[] set)
    {
      if(m_mechanics == null)
        return false;
      for(int index = 0; index < set.Length; ++index)
      {
        if(set[index] && m_mechanics[index] > 0)
          return true;
      }

      return false;
    }

    private void SetCanHarmState()
    {
      if(!IsAnySetNoCheck(SpellConstants.HarmPreventionMechanics))
      {
        CanDoPhysicalActivity = m_Pacified <= 0;
        m_canHarm = m_canDoPhysicalActivity || !IsUnderInfluenceOf(SpellMechanic.Silenced);
      }
      else
      {
        CanDoPhysicalActivity = false;
        m_canHarm = false;
      }
    }

    /// <summary>Whether the owner is completely invulnerable</summary>
    public bool IsInvulnerable
    {
      get
      {
        if(m_mechanics == null)
          return false;
        if(m_mechanics[25] <= 0)
          return m_mechanics[29] > 0;
        return true;
      }
      set
      {
        if(m_mechanics == null)
          m_mechanics = CreateMechanicsArr();
        if(value)
          ++m_mechanics[25];
        else
          m_mechanics[25] = 0;
      }
    }

    /// <summary>
    /// Indicates whether the owner is immune against the given SpellMechanic
    /// </summary>
    public bool IsImmune(SpellMechanic mechanic)
    {
      if(mechanic == SpellMechanic.None || m_mechanicImmunities == null)
        return false;
      return m_mechanicImmunities[(int) mechanic] > 0;
    }

    /// <summary>
    /// Indicates whether the owner is immune against the given DamageSchool
    /// </summary>
    public bool IsImmune(DamageSchool school)
    {
      if(m_dmgImmunities != null)
        return m_dmgImmunities[(int) school] > 0;
      return false;
    }

    public bool IsImmuneToSpell(Spell spell)
    {
      return spell.Mechanic.IsNegative() && spell.IsAffectedByInvulnerability &&
             (spell.Mechanic == SpellMechanic.Invulnerable_2 || spell.Mechanic == SpellMechanic.Invulnerable) &&
             (IsInvulnerable || IsImmune(SpellMechanic.Invulnerable_2) ||
              (IsImmune(SpellMechanic.Invulnerable) || IsImmune(spell.Mechanic)) || IsImmune(spell.DispelType));
    }

    /// <summary>Adds immunity against given damage-school</summary>
    public void IncDmgImmunityCount(DamageSchool school)
    {
      if(m_dmgImmunities == null)
        m_dmgImmunities = CreateDamageSchoolArr();
      if(m_dmgImmunities[(int) school] == 0)
        Auras.RemoveWhere(aura =>
          aura.Spell.SchoolMask.HasAnyFlag((DamageSchoolMask) (1 << (int) (school & (DamageSchool) 31))));
      ++m_dmgImmunities[(int) school];
    }

    /// <summary>Adds immunity against given damage-schools</summary>
    public void IncDmgImmunityCount(uint[] schools)
    {
      foreach(DamageSchool school in schools)
        IncDmgImmunityCount(school);
    }

    /// <summary>Adds immunity against given damage-schools</summary>
    public void IncDmgImmunityCount(SpellEffect effect)
    {
      if(m_dmgImmunities == null)
        m_dmgImmunities = CreateDamageSchoolArr();
      foreach(int miscBit in effect.MiscBitSet)
        ++m_dmgImmunities[miscBit];
      Auras.RemoveWhere(aura =>
      {
        if((int) aura.Spell.AuraUID != (int) effect.Spell.AuraUID &&
           aura.Spell.SchoolMask.HasAnyFlag(effect.Spell.SchoolMask))
          return !aura.Spell.Attributes.HasFlag(SpellAttributes.UnaffectedByInvulnerability);
        return false;
      });
    }

    /// <summary>Decreases immunity-count against given damage-school</summary>
    public void DecDmgImmunityCount(DamageSchool school)
    {
      if(m_dmgImmunities == null || m_dmgImmunities[(int) school] <= 0)
        return;
      --m_dmgImmunities[(int) school];
    }

    /// <summary>Decreases immunity-count against given damage-schools</summary>
    public void DecDmgImmunityCount(uint[] damageSchools)
    {
      foreach(DamageSchool damageSchool in damageSchools)
        DecDmgImmunityCount(damageSchool);
    }

    /// <summary>Adds immunity against given SpellMechanic-school</summary>
    public void IncMechImmunityCount(SpellMechanic mechanic, Spell exclude)
    {
      if(m_mechanicImmunities == null)
        m_mechanicImmunities = CreateMechanicsArr();
      if(m_mechanicImmunities[(int) mechanic] == 0)
        Auras.RemoveWhere(aura =>
        {
          if(aura.Spell.Mechanic != mechanic || aura.Spell == exclude ||
             aura.Spell.TargetTriggerSpells != null && aura.Spell.TargetTriggerSpells.Contains(exclude) ||
             aura.Spell.CasterTriggerSpells != null && aura.Spell.CasterTriggerSpells.Contains(exclude))
            return false;
          if(mechanic == SpellMechanic.Invulnerable || mechanic == SpellMechanic.Invulnerable_2)
            return !aura.Spell.Attributes.HasFlag(SpellAttributes.UnaffectedByInvulnerability);
          return true;
        });
      ++m_mechanicImmunities[(int) mechanic];
    }

    /// <summary>
    /// Decreases immunity-count against given SpellMechanic-school
    /// </summary>
    public void DecMechImmunityCount(SpellMechanic mechanic)
    {
      if(m_mechanicImmunities == null || m_mechanicImmunities[(int) mechanic] <= 0)
        return;
      --m_mechanicImmunities[(int) mechanic];
    }

    /// <summary>
    /// Returns the resistance chance for the given SpellMechanic
    /// </summary>
    public int GetMechanicResistance(SpellMechanic mechanic)
    {
      if(m_mechanicResistances == null)
        return 0;
      return m_mechanicResistances[(int) mechanic];
    }

    /// <summary>
    /// Changes the amount of resistance against certain SpellMechanics
    /// </summary>
    public void ModMechanicResistance(SpellMechanic mechanic, int delta)
    {
      if(m_mechanicResistances == null)
        m_mechanicResistances = CreateMechanicsArr();
      int num = m_mechanicResistances[(int) mechanic] + delta;
      if(num < 0)
        num = 0;
      m_mechanicResistances[(int) mechanic] = num;
    }

    /// <summary>
    /// Returns the duration modifier for a certain SpellMechanic
    /// </summary>
    public int GetMechanicDurationMod(SpellMechanic mechanic)
    {
      if(m_mechanicDurationMods == null || mechanic == SpellMechanic.None)
        return 0;
      return m_mechanicDurationMods[(int) mechanic];
    }

    /// <summary>
    /// Changes the duration-modifier for a certain SpellMechanic in %
    /// </summary>
    public void ModMechanicDurationMod(SpellMechanic mechanic, int delta)
    {
      if(m_mechanicDurationMods == null)
        m_mechanicDurationMods = CreateMechanicsArr();
      int num = m_mechanicDurationMods[(int) mechanic] + delta;
      if(num < 0)
        num = 0;
      m_mechanicDurationMods[(int) mechanic] = num;
    }

    public int GetDebuffResistance(DamageSchool school)
    {
      if(m_debuffResistances == null)
        return 0;
      return m_debuffResistances[(int) school];
    }

    public void SetDebuffResistance(DamageSchool school, int value)
    {
      if(m_debuffResistances == null)
        m_debuffResistances = CreateDamageSchoolArr();
      m_debuffResistances[(uint) school] = value;
    }

    public void ModDebuffResistance(DamageSchool school, int delta)
    {
      if(m_debuffResistances == null)
        m_debuffResistances = CreateDamageSchoolArr();
      m_debuffResistances[(int) school] += delta;
    }

    public bool IsImmune(DispelType school)
    {
      if(m_dispelImmunities == null)
        return false;
      return m_dispelImmunities[(int) school] > 0;
    }

    public void IncDispelImmunity(DispelType school)
    {
      if(m_dispelImmunities == null)
        m_dispelImmunities = CreateDispelTypeArr();
      int dispelImmunity = m_dispelImmunities[(uint) school];
      if(dispelImmunity == 0)
        Auras.RemoveWhere(aura => aura.Spell.DispelType == school);
      m_dispelImmunities[(uint) school] = dispelImmunity + 1;
    }

    public void DecDispelImmunity(DispelType school)
    {
      if(m_dispelImmunities == null)
        return;
      int dispelImmunity = m_dispelImmunities[(uint) school];
      if(dispelImmunity <= 0)
        return;
      m_dispelImmunities[(int) school] = dispelImmunity - 1;
    }

    public int GetTargetResistanceMod(DamageSchool school)
    {
      if(m_TargetResMods == null)
        return 0;
      return m_TargetResMods[(int) school];
    }

    private void SetTargetResistanceMod(DamageSchool school, int value)
    {
      if(m_TargetResMods == null)
        m_TargetResMods = CreateDamageSchoolArr();
      m_TargetResMods[(uint) school] = value;
      if(school != DamageSchool.Physical || !(this is Character))
        return;
      SetInt32(PlayerFields.MOD_TARGET_PHYSICAL_RESISTANCE, value);
    }

    internal void ModTargetResistanceMod(DamageSchool school, int delta)
    {
      if(m_TargetResMods == null)
        m_TargetResMods = CreateDamageSchoolArr();
      int num = m_TargetResMods[(int) school] + delta;
      m_TargetResMods[(int) school] = num;
      if(school != DamageSchool.Physical || !(this is Character))
        return;
      SetInt32(PlayerFields.MOD_TARGET_PHYSICAL_RESISTANCE, num);
    }

    /// <summary>
    /// If modifying a single value, we have a simple bonus that will not be displayed
    /// to the user (except for physical), if there are more than one, we assume it's a change
    /// in all-over spell penetration
    /// </summary>
    /// <param name="dmgTypes"></param>
    /// <param name="delta"></param>
    public void ModTargetResistanceMod(int delta, params uint[] dmgTypes)
    {
      for(int index = 0; index < dmgTypes.Length; ++index)
        ModTargetResistanceMod((DamageSchool) dmgTypes[index], delta);
      if(dmgTypes.Length <= 0 || !(this is Character))
        return;
      SetInt32(PlayerFields.MOD_TARGET_RESISTANCE, GetInt32(PlayerFields.MOD_TARGET_RESISTANCE) + delta);
    }

    public void ModSpellInterruptProt(DamageSchool school, int delta)
    {
      if(m_spellInterruptProt == null)
        m_spellInterruptProt = CreateDamageSchoolArr();
      int num = m_spellInterruptProt[(int) school] + delta;
      m_spellInterruptProt[(int) school] = num;
    }

    public void ModSpellInterruptProt(uint[] dmgTypes, int delta)
    {
      foreach(DamageSchool dmgType in dmgTypes)
        ModSpellInterruptProt(dmgType, delta);
    }

    public int GetSpellInterruptProt(Spell spell)
    {
      if(m_spellInterruptProt == null)
        return 0;
      return m_spellInterruptProt[(int) spell.Schools[0]];
    }

    public int GetSpellHitChanceMod(DamageSchool school)
    {
      if(m_SpellHitChance == null)
        return 0;
      return m_SpellHitChance[(int) school];
    }

    public int GetHighestSpellHitChanceMod(DamageSchool[] schools)
    {
      if(m_SpellHitChance == null)
        return 0;
      return schools.Select(school => m_SpellHitChance[(int) school]).Max();
    }

    /// <summary>Spell avoidance</summary>
    public virtual void ModSpellHitChance(DamageSchool school, int delta)
    {
      if(m_SpellHitChance == null)
        m_SpellHitChance = CreateDamageSchoolArr();
      int num = m_SpellHitChance[(int) school] + delta;
      m_SpellHitChance[(int) school] = num;
    }

    /// <summary>Spell avoidance</summary>
    public void ModSpellHitChance(uint[] schools, int delta)
    {
      foreach(DamageSchool school in schools)
        ModSpellHitChance(school, delta);
    }

    /// <summary>Returns the SpellCritChance for the given DamageType</summary>
    public virtual float GetCritChance(DamageSchool school)
    {
      return GetCritMod(school);
    }

    public int GetCritMod(DamageSchool school)
    {
      if(m_CritMods == null)
        return 0;
      return m_CritMods[(int) school];
    }

    public void SetCritMod(DamageSchool school, int value)
    {
      if(m_CritMods == null)
        m_CritMods = CreateDamageSchoolArr();
      m_CritMods[(uint) school] = value;
      if(!(this is Character))
        return;
      ((Character) this).UpdateSpellCritChance();
    }

    public void ModCritMod(DamageSchool school, int delta)
    {
      if(m_CritMods == null)
        m_CritMods = CreateDamageSchoolArr();
      m_CritMods[(int) school] += delta;
      if(!(this is Character))
        return;
      ((Character) this).UpdateSpellCritChance();
    }

    public void ModCritMod(DamageSchool[] schools, int delta)
    {
      if(m_CritMods == null)
        m_CritMods = CreateDamageSchoolArr();
      foreach(int school in schools)
        m_CritMods[school] += delta;
      if(!(this is Character))
        return;
      ((Character) this).UpdateSpellCritChance();
    }

    public void ModCritMod(uint[] schools, int delta)
    {
      if(m_CritMods == null)
        m_CritMods = CreateDamageSchoolArr();
      foreach(uint school in schools)
        m_CritMods[school] += delta;
      if(!(this is Character))
        return;
      ((Character) this).UpdateSpellCritChance();
    }

    /// <summary>
    /// Returns the damage taken modifier for the given DamageSchool
    /// </summary>
    public int GetDamageTakenMod(DamageSchool school)
    {
      if(m_damageTakenMods == null)
        return 0;
      return m_damageTakenMods[(int) school];
    }

    public void SetDamageTakenMod(DamageSchool school, int value)
    {
      if(m_damageTakenMods == null)
        m_damageTakenMods = CreateDamageSchoolArr();
      m_damageTakenMods[(uint) school] = value;
    }

    public void ModDamageTakenMod(DamageSchool school, int delta)
    {
      if(m_damageTakenMods == null)
        m_damageTakenMods = CreateDamageSchoolArr();
      m_damageTakenMods[(int) school] += delta;
    }

    public void ModDamageTakenMod(DamageSchool[] schools, int delta)
    {
      if(m_damageTakenMods == null)
        m_damageTakenMods = CreateDamageSchoolArr();
      foreach(int school in schools)
        m_damageTakenMods[school] += delta;
    }

    public void ModDamageTakenMod(uint[] schools, int delta)
    {
      if(m_damageTakenMods == null)
        m_damageTakenMods = CreateDamageSchoolArr();
      foreach(uint school in schools)
        m_damageTakenMods[school] += delta;
    }

    /// <summary>
    /// Returns the damage taken modifier for the given DamageSchool
    /// </summary>
    public int GetDamageTakenPctMod(DamageSchool school)
    {
      if(m_damageTakenPctMods == null)
        return 0;
      return m_damageTakenPctMods[(int) school];
    }

    public void SetDamageTakenPctMod(DamageSchool school, int value)
    {
      if(m_damageTakenPctMods == null)
        m_damageTakenPctMods = CreateDamageSchoolArr();
      m_damageTakenPctMods[(uint) school] = value;
    }

    public void ModDamageTakenPctMod(DamageSchool school, int delta)
    {
      if(m_damageTakenPctMods == null)
        m_damageTakenPctMods = CreateDamageSchoolArr();
      m_damageTakenPctMods[(int) school] += delta;
    }

    public void ModDamageTakenPctMod(DamageSchool[] schools, int delta)
    {
      if(m_damageTakenPctMods == null)
        m_damageTakenPctMods = CreateDamageSchoolArr();
      foreach(int school in schools)
        m_damageTakenPctMods[school] += delta;
    }

    public void ModDamageTakenPctMod(uint[] schools, int delta)
    {
      if(m_damageTakenPctMods == null)
        m_damageTakenPctMods = CreateDamageSchoolArr();
      foreach(uint school in schools)
        m_damageTakenPctMods[school] += delta;
    }

    /// <summary>Threat mod in percent</summary>
    public void ModThreat(DamageSchool school, int delta)
    {
      if(m_threatMods == null)
        m_threatMods = CreateDamageSchoolArr();
      int num = m_threatMods[(int) school] + delta;
      m_threatMods[(int) school] = num;
    }

    /// <summary>Threat mod in percent</summary>
    public void ModThreat(uint[] dmgTypes, int delta)
    {
      foreach(DamageSchool dmgType in dmgTypes)
        ModThreat(dmgType, delta);
    }

    public int GetGeneratedThreat(IDamageAction action)
    {
      return GetGeneratedThreat(action.ActualDamage, action.UsedSchool, action.SpellEffect);
    }

    /// <summary>Threat mod in percent</summary>
    public virtual int GetGeneratedThreat(int dmg, DamageSchool school, SpellEffect effect)
    {
      if(m_threatMods == null)
        return dmg;
      return Math.Max(0, dmg + dmg * m_threatMods[(int) school] / 100);
    }

    public int GetAttackerSpellHitChanceMod(DamageSchool school)
    {
      if(m_attackerSpellHitChance == null)
        return 0;
      return m_attackerSpellHitChance[(int) school];
    }

    /// <summary>Spell avoidance</summary>
    public void ModAttackerSpellHitChance(DamageSchool school, int delta)
    {
      if(m_attackerSpellHitChance == null)
        m_attackerSpellHitChance = CreateDamageSchoolArr();
      int num = m_attackerSpellHitChance[(int) school] + delta;
      m_attackerSpellHitChance[(int) school] = num;
    }

    /// <summary>Spell avoidance</summary>
    public void ModAttackerSpellHitChance(uint[] schools, int delta)
    {
      foreach(DamageSchool school in schools)
        ModAttackerSpellHitChance(school, delta);
    }

    /// <summary>
    /// Whether this Character is currently allowed to teleport
    /// </summary>
    public virtual bool MayTeleport
    {
      get { return true; }
    }

    /// <summary>
    /// Teleports the owner to the given position in the current map.
    /// </summary>
    /// <returns>Whether the Zone had a globally unique Site.</returns>
    public void TeleportTo(Vector3 pos)
    {
      TeleportTo(m_Map, ref pos, new float?());
    }

    /// <summary>
    /// Teleports the owner to the given position in the current map.
    /// </summary>
    /// <returns>Whether the Zone had a globally unique Site.</returns>
    public void TeleportTo(ref Vector3 pos)
    {
      TeleportTo(m_Map, ref pos, new float?());
    }

    /// <summary>
    /// Teleports the owner to the given position in the current map.
    /// </summary>
    /// <returns>Whether the Zone had a globally unique Site.</returns>
    public void TeleportTo(ref Vector3 pos, float? orientation)
    {
      TeleportTo(m_Map, ref pos, orientation);
    }

    /// <summary>
    /// Teleports the owner to the given position in the current map.
    /// </summary>
    /// <returns>Whether the Zone had a globally unique Site.</returns>
    public void TeleportTo(Vector3 pos, float? orientation)
    {
      TeleportTo(m_Map, ref pos, orientation);
    }

    /// <summary>
    /// Teleports the owner to the given position in the given map.
    /// </summary>
    /// <param name="map">the target <see cref="T:WCell.RealmServer.Global.Map" /></param>
    /// <param name="pos">the target <see cref="T:WCell.Util.Graphics.Vector3">position</see></param>
    public void TeleportTo(MapId map, ref Vector3 pos)
    {
      TeleportTo(World.GetNonInstancedMap(map), ref pos, new float?());
    }

    /// <summary>
    /// Teleports the owner to the given position in the given map.
    /// </summary>
    /// <param name="map">the target <see cref="T:WCell.RealmServer.Global.Map" /></param>
    /// <param name="pos">the target <see cref="T:WCell.Util.Graphics.Vector3">position</see></param>
    public void TeleportTo(MapId map, Vector3 pos)
    {
      TeleportTo(World.GetNonInstancedMap(map), ref pos, new float?());
    }

    /// <summary>
    /// Teleports the owner to the given position in the given map.
    /// </summary>
    /// <param name="map">the target <see cref="T:WCell.RealmServer.Global.Map" /></param>
    /// <param name="pos">the target <see cref="T:WCell.Util.Graphics.Vector3">position</see></param>
    public void TeleportTo(Map map, ref Vector3 pos)
    {
      TeleportTo(map, ref pos, new float?());
    }

    /// <summary>
    /// Teleports the owner to the given position in the given map.
    /// </summary>
    /// <param name="map">the target <see cref="T:WCell.RealmServer.Global.Map" /></param>
    /// <param name="pos">the target <see cref="T:WCell.Util.Graphics.Vector3">position</see></param>
    public void TeleportTo(Map map, Vector3 pos)
    {
      TeleportTo(map, ref pos, new float?());
    }

    /// <summary>Teleports the owner to the given WorldObject.</summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public bool TeleportTo(IWorldLocation location)
    {
      Vector3 position = location.Position;
      Map map = location.Map;
      if(map == null)
      {
        if(Map.Id != location.MapId)
          return false;
        map = Map;
      }

      TeleportTo(map, ref position, m_orientation);
      Phase = location.Phase;
      if(location is WorldObject)
        Zone = ((WorldObject) location).Zone;
      return true;
    }

    /// <summary>
    /// Teleports the owner to the given position in the given map.
    /// </summary>
    /// <param name="map">the target <see cref="T:WCell.RealmServer.Global.Map" /></param>
    /// <param name="pos">the target <see cref="T:WCell.Util.Graphics.Vector3">position</see></param>
    /// <param name="orientation">the target orientation</param>
    public void TeleportTo(Map map, Vector3 pos, float? orientation)
    {
      TeleportTo(map, ref pos, orientation);
    }

    /// <summary>
    /// Teleports the owner to the given position in the given map.
    /// </summary>
    /// <param name="map">the target <see cref="T:WCell.RealmServer.Global.Map" /></param>
    /// <param name="pos">the target <see cref="T:WCell.Util.Graphics.Vector3">position</see></param>
    /// <param name="orientation">the target orientation</param>
    public void TeleportTo(Map map, ref Vector3 pos, float? orientation)
    {
      Map map1 = m_Map;
      if(map.IsDisposed)
        return;
      Character chr = this as Character;
      if(chr != null)
      {
        if(map.MapTemplate.IsDisabled && !chr.GodMode)
        {
          chr.SendInfoMsg(string.Format("Map {0} is disabled.", map.Name));
          return;
        }

        Log.Create(Log.Types.ChangePosition, LogSourceType.Character, chr.EntryId)
          .AddAttribute("source", 0.0, "teleport").AddAttribute(nameof(map), (double) map.Id, map.Id.ToString())
          .AddAttribute("x", pos.X, "").AddAttribute("y", pos.Y, "").Write();
      }

      CancelMovement();
      CancelAllActions();
      if(map1 == map)
      {
        if(!Map.MoveObject(this, ref pos))
          return;
        if(orientation.HasValue)
          Orientation = orientation.Value;
        if(chr == null)
          return;
        chr.IsMoving = false;
        chr.LastPosition = pos;
        MovementFlags = MovementFlags.None;
        MovementFlags2 = MovementFlags2.None;
        Asda2CharacterHandler.SendResurectResponse(chr);
      }
      else if(map1 != null && !map1.IsInContext)
      {
        Vector3 position = pos;
        map1.AddMessage(new Message(() => TeleportTo(map, ref position, orientation)));
      }
      else if(map.TransferObjectLater(this, pos))
      {
        if(orientation.HasValue)
          Orientation = orientation.Value;
        if(chr == null)
          return;
        chr.LastPosition = pos;
        MovementFlags = MovementFlags.None;
        MovementFlags2 = MovementFlags2.None;
      }
      else
        log.Error("ERROR: Tried to teleport object, but failed to add player to the new map - " + this);
    }

    /// <summary>Count of stealth-modifiers</summary>
    public int Stealthed
    {
      get { return m_stealthed; }
      set
      {
        if(m_stealthed == value)
          return;
        if(m_stealthed > 0 && value <= 0)
          StateFlags &= ~StateFlag.Sneaking;
        else if(m_stealthed <= 0 && value > 0)
        {
          StateFlags |= StateFlag.Sneaking;
          Auras.RemoveByFlag(AuraInterruptFlags.OnStealth);
        }

        m_stealthed = value;
      }
    }

    public MovementFlags MovementFlags
    {
      get { return m_movementFlags; }
      set { m_movementFlags = value; }
    }

    public MovementFlags2 MovementFlags2
    {
      get { return m_movementFlags2; }
      set { m_movementFlags2 = value; }
    }

    public bool IsMovementControlled
    {
      get { return m_Movement != null; }
    }

    /// <summary>
    /// Stops this Unit's movement (if it's movement is controlled)
    /// </summary>
    public void StopMoving()
    {
      if(m_Movement == null)
        return;
      m_Movement.Stop();
    }

    /// <summary>
    /// An object to control this Unit's movement.
    /// Only used for NPCs and posessed Characters.
    /// </summary>
    public Movement Movement
    {
      get
      {
        if(m_Movement == null)
          m_Movement = new Movement(this);
        return m_Movement;
      }
    }

    /// <summary>Whether the Unit is currently flying</summary>
    public bool IsFlying
    {
      get { return m_flying > 0U; }
    }

    /// <summary>Whether the character may walk over water</summary>
    public uint WaterWalk
    {
      get { return m_waterWalk; }
      set
      {
        if(m_waterWalk == 0U != (value == 0U) && this is Character)
        {
          if(value == 0U)
            MovementHandler.SendWalk((Character) this);
          else
            MovementHandler.SendWaterWalk((Character) this);
        }

        m_waterWalk = value;
      }
    }

    /// <summary>Whether a character can fly or not</summary>
    public uint Flying
    {
      get { return m_flying; }
      set
      {
        if(m_flying == 0U != (value == 0U))
        {
          if(value > 0U)
            MovementFlags |= MovementFlags.Flying;
          else
            MovementFlags &= MovementFlags.MaskMoving | MovementFlags.PitchUp | MovementFlags.PitchDown |
                             MovementFlags.WalkMode | MovementFlags.OnTransport | MovementFlags.DisableGravity |
                             MovementFlags.Root | MovementFlags.PendingStop | MovementFlags.PendingStrafeStop |
                             MovementFlags.PendingForward | MovementFlags.PendingBackward |
                             MovementFlags.PendingStrafeLeft | MovementFlags.PendingStrafeRight |
                             MovementFlags.PendingRoot | MovementFlags.Swimming | MovementFlags.CanFly |
                             MovementFlags.SplineElevation | MovementFlags.SplineEnabled | MovementFlags.Waterwalking |
                             MovementFlags.CanSafeFall | MovementFlags.Hover | MovementFlags.LocalDirty;
          if(this is Character)
          {
            if(value == 0U)
              MovementHandler.SendFlyModeStop(this);
            else
              MovementHandler.SendFlyModeStart(this);
          }
        }

        m_flying = value;
      }
    }

    /// <summary>Whether a character can hover</summary>
    public uint Hovering
    {
      get { return m_hovering; }
      set
      {
        if(m_hovering == 0U != (value == 0U) && this is Character)
        {
          if(value == 0U)
            MovementHandler.SendHoverModeStop(this);
          else
            MovementHandler.SendHoverModeStart(this);
        }

        m_hovering = value;
      }
    }

    /// <summary>Whether a character would take falling damage or not</summary>
    public uint FeatherFalling
    {
      get { return m_featherFalling; }
      set
      {
        if(m_featherFalling == 0U != (value == 0U) && this is Character)
        {
          if(value == 0U)
            MovementHandler.SendFeatherModeStop(this);
          else
            MovementHandler.SendFeatherModeStart(this);
        }

        m_featherFalling = value;
      }
    }

    /// <summary>
    /// The overall-factor for all speeds. Set by the owner's ModifierCollection
    /// </summary>
    public float SpeedFactor
    {
      get { return m_speedFactor; }
      set
      {
        if(value == (double) m_speedFactor)
          return;
        m_speedFactor = value;
        WalkSpeed = DefaultWalkSpeed * m_speedFactor;
        RunBackSpeed = DefaultWalkBackSpeed * m_speedFactor;
        RunSpeed = DefaultRunSpeed * m_speedFactor;
        SwimSpeed = DefaultSwimSpeed * (m_speedFactor + m_swimFactor);
        SwimBackSpeed = DefaultSwimBackSpeed * (m_speedFactor + m_swimFactor);
        FlightSpeed = DefaultFlightSpeed * (m_speedFactor + m_flightFactor);
        FlightBackSpeed = DefaultFlightBackSpeed * (m_speedFactor + m_flightFactor);
      }
    }

    /// <summary>
    /// The factor for all flying-related speeds. Set by the owner's ModifierCollection
    /// </summary>
    public float FlightSpeedFactor
    {
      get { return m_flightFactor; }
      internal set
      {
        if(value == (double) m_flightFactor)
          return;
        m_flightFactor = value;
        FlightSpeed = DefaultFlightSpeed * (m_speedFactor + m_flightFactor);
        FlightBackSpeed = DefaultFlightBackSpeed * (m_speedFactor + m_flightFactor);
      }
    }

    /// <summary>The factor for mounted speed</summary>
    public float MountSpeedMod
    {
      get { return m_mountMod; }
      internal set
      {
        if(value == (double) m_mountMod)
          return;
        if(IsMounted)
          SpeedFactor += value - m_mountMod;
        m_mountMod = value;
      }
    }

    /// <summary>The factor for all swimming-related speeds</summary>
    public float SwimSpeedFactor
    {
      get { return m_swimFactor; }
      internal set
      {
        if(value == (double) m_swimFactor)
          return;
        m_swimFactor = value;
        SwimSpeed = DefaultSwimSpeed * (m_speedFactor + m_swimFactor);
        SwimBackSpeed = DefaultSwimBackSpeed * (m_speedFactor + m_swimFactor);
      }
    }

    /// <summary>Forward walking speed.</summary>
    public float WalkSpeed
    {
      get { return m_walkSpeed; }
      set
      {
        if(m_walkSpeed == (double) value)
          return;
        m_walkSpeed = value;
      }
    }

    /// <summary>Backwards walking speed.</summary>
    public float RunBackSpeed
    {
      get { return m_walkBackSpeed; }
      set
      {
        if(m_walkBackSpeed == (double) value)
          return;
        m_walkBackSpeed = value;
        MovementHandler.SendSetRunBackSpeed(this);
      }
    }

    /// <summary>Forward running speed.</summary>
    public float RunSpeed
    {
      get { return m_runSpeed; }
      set
      {
        if(m_runSpeed == (double) value)
          return;
        m_runSpeed = value;
      }
    }

    /// <summary>Forward swimming speed.</summary>
    public float SwimSpeed
    {
      get { return m_swimSpeed; }
      set
      {
        if(m_swimSpeed == (double) value)
          return;
        m_swimSpeed = value;
        MovementHandler.SendSetSwimSpeed(this);
      }
    }

    /// <summary>Backwards swimming speed.</summary>
    public float SwimBackSpeed
    {
      get { return m_swimBackSpeed; }
      set
      {
        if(m_swimBackSpeed == (double) value)
          return;
        m_swimBackSpeed = value;
        MovementHandler.SendSetSwimBackSpeed(this);
      }
    }

    /// <summary>Forward flying speed.</summary>
    public float FlightSpeed
    {
      get { return m_flightSpeed; }
      set
      {
        if(m_flightSpeed == (double) value)
          return;
        m_flightSpeed = value;
        MovementHandler.SendSetFlightSpeed(this);
      }
    }

    /// <summary>Backwards flying speed.</summary>
    public float FlightBackSpeed
    {
      get { return m_flightBackSpeed; }
      set
      {
        if(m_flightBackSpeed == (double) value)
          return;
        m_flightBackSpeed = value;
        MovementHandler.SendSetFlightBackSpeed(this);
      }
    }

    /// <summary>Turning speed.</summary>
    public float TurnSpeed
    {
      get { return m_turnSpeed; }
      set
      {
        if(m_turnSpeed == (double) value)
          return;
        m_turnSpeed = value;
        MovementHandler.SendSetTurnRate(this);
      }
    }

    public float PitchRate
    {
      get { return m_pitchSpeed; }
      set
      {
        if(m_pitchSpeed == (double) value)
          return;
        m_pitchSpeed = value;
        MovementHandler.SendSetPitchRate(this);
      }
    }

    public void ResetMechanicDefaults()
    {
      SpeedFactor = 1f;
      m_mountMod = 0.0f;
      m_flying = m_waterWalk = m_hovering = m_featherFalling = 0U;
      m_canMove = m_canHarm = m_canInteract = m_canCastSpells = m_canDoPhysicalActivity = true;
      m_walkSpeed = DefaultWalkSpeed;
      m_walkBackSpeed = DefaultWalkBackSpeed;
      m_runSpeed = DefaultRunSpeed;
      m_swimSpeed = DefaultSwimSpeed;
      m_swimBackSpeed = DefaultSwimBackSpeed;
      m_flightSpeed = DefaultFlightSpeed;
      m_flightBackSpeed = DefaultFlightBackSpeed;
      m_turnSpeed = DefaultTurnSpeed;
      m_pitchSpeed = DefaulPitchSpeed;
    }

    public virtual float GetResiliencePct()
    {
      return 0.0f;
    }

    public int AttackerSpellCritChancePercentMod { get; set; }

    public int AttackerPhysicalCritChancePercentMod { get; set; }

    public float IsCollisionWith(Unit unit)
    {
      float num1 = Position.X - unit.Position.X;
      float num2 = Position.Y - unit.Position.Y;
      float num3 = (float) Math.Sqrt(num1 * (double) num1 + num2 * (double) num2);
      return BoundingRadius + unit.BoundingRadius - num3;
    }

    public override UpdateFlags UpdateFlags
    {
      get { return UpdateFlags.Flag_0x10 | UpdateFlags.Living | UpdateFlags.StationaryObject; }
    }

    public override ObjectTypeId ObjectTypeId
    {
      get { return ObjectTypeId.Unit; }
    }

    public override UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
    {
      if(chr == m_master)
        return UpdateFieldFlags.Public | UpdateFieldFlags.OwnerOnly;
      return IsAlliedWith(chr) ? UpdateFieldFlags.Public | UpdateFieldFlags.GroupOnly : UpdateFieldFlags.Public;
    }

    public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
    {
      get { return UpdateFieldHandler.DynamicUnitHandlers; }
    }

    public virtual WorldObject Mover
    {
      get { return this; }
    }

    protected override void WriteMovementUpdate(PrimitiveWriter packet, UpdateFieldFlags relation)
    {
      WriteMovementPacketInfo(packet);
      packet.Write(WalkSpeed);
      packet.Write(RunSpeed);
      packet.Write(RunBackSpeed);
      packet.Write(SwimSpeed);
      packet.Write(SwimBackSpeed);
      packet.Write(FlightSpeed);
      packet.Write(FlightBackSpeed);
      packet.Write(TurnSpeed);
      packet.Write(PitchRate);
      MovementFlags.HasFlag(MovementFlags.SplineEnabled);
    }

    protected override void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation,
      UpdateFlags updateFlags)
    {
      if(!updateFlags.HasFlag(UpdateFlags.AttackingTarget))
        return;
      writer.Write((byte) 0);
    }

    protected override void WriteUpdateFlag_0x10(PrimitiveWriter writer, UpdateFieldFlags relation)
    {
      writer.Write(150754760);
    }

    /// <summary>
    /// Writes the data shared in movement packets and the create block of the update packet
    /// This is used in
    /// <list type="String">
    /// SMSG_UPDATE_OBJECT
    /// MSG_MOVE_*
    /// MSG_MOVE_SET_*_SPEED
    /// </list>
    /// </summary>
    /// <param name="packet"></param>
    public void WriteMovementPacketInfo(PrimitiveWriter packet)
    {
      WriteMovementPacketInfo(packet, ref m_position, m_orientation);
    }

    /// <summary>
    /// Writes the data shared in movement packets and the create block of the update packet
    /// This is used in
    /// <list type="String">
    /// SMSG_UPDATE_OBJECT
    /// MSG_MOVE_*
    /// MSG_MOVE_SET_*_SPEED
    /// </list>
    /// </summary>
    /// <param name="packet"></param>
    public void WriteMovementPacketInfo(PrimitiveWriter packet, ref Vector3 pos, float orientation)
    {
      MovementFlags movementFlags = MovementFlags;
      MovementFlags2 movementFlags2 = MovementFlags2;
      if(movementFlags.HasAnyFlag(MovementFlags.OnTransport) && TransportInfo == null)
        movementFlags ^= MovementFlags.OnTransport;
      packet.Write((uint) movementFlags);
      packet.Write((ushort) movementFlags2);
      packet.Write(Utility.GetSystemTime());
      packet.Write(pos.X);
      packet.Write(pos.Y);
      packet.Write(pos.Z);
      packet.Write(orientation);
      if(movementFlags.HasAnyFlag(MovementFlags.OnTransport))
      {
        TransportInfo.EntityId.WritePacked(packet);
        packet.Write(TransportPosition.X);
        packet.Write(TransportPosition.Y);
        packet.Write(TransportPosition.Z);
        packet.Write(TransportOrientation);
        packet.Write(TransportTime);
        packet.Write(TransportSeat);
      }

      if(movementFlags.HasAnyFlag(MovementFlags.Swimming | MovementFlags.Flying) ||
         movementFlags2.HasFlag(MovementFlags2.AlwaysAllowPitching))
        packet.Write(PitchRate);
      packet.Write(0);
      if(movementFlags.HasAnyFlag(MovementFlags.Falling))
      {
        packet.Write(0.0f);
        packet.Write(8f);
        packet.Write(0.2f);
        packet.Write(1f);
      }

      if(!movementFlags.HasAnyFlag(MovementFlags.SplineElevation))
        return;
      packet.Write(0.0f);
    }

    public void WriteTeleportPacketInfo(RealmPacketOut packet, int param)
    {
      EntityId.WritePacked(packet);
      packet.Write(param);
      WriteMovementPacketInfo(packet);
    }

    public override void Update(int dt)
    {
      base.Update(dt);
      Regenerate(dt);
      if(m_brain != null)
        m_brain.Update(dt);
      if(m_attackTimer != null)
        m_attackTimer.Update(dt);
      if(m_auras == null)
        return;
      foreach(Aura aura in m_auras)
      {
        if(aura != null)
          aura.Update(dt);
      }
    }
  }
}