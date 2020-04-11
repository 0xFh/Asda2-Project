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
  public abstract class Unit : WorldObject, ILivingEntity, ISummoner, INamedEntity, IEntity, INamed, IWorldZoneLocation, IWorldLocation, IHasPosition
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
    [NotVariable]
    public static float DefaultSpeedFactor = 0.7f;
    public static float DefaultWalkSpeed = 2.5f;
    public static float DefaultWalkBackSpeed = 2.5f;
    public static float DefaultRunSpeed = 0.37f;
    public static float DefaultSwimSpeed = 4.7222f;
    public static float DefaultSwimBackSpeed = 4.5f;
    public static float DefaultFlightSpeed = 7f;
    public static float DefaultFlightBackSpeed = 4.5f;
    [NotVariable]
    public static readonly float DefaultTurnSpeed = 3.141593f;
    [NotVariable]
    public static readonly float DefaulPitchSpeed = 3.141593f;
    public static readonly int MechanicCount = (int) Convert.ChangeType((object) Utility.GetMaxEnum<SpellMechanic>(), typeof (int)) + 1;
    public static readonly int DamageSchoolCount = 7;
    public static readonly int DispelTypeCount = (int) Convert.ChangeType((object) Utility.GetMaxEnum<DispelType>(), typeof (int)) + 1;
    /// <summary>All CombatRatings</summary>
    public static readonly CombatRating[] CombatRatings = (CombatRating[]) Enum.GetValues(typeof (CombatRating));
    protected internal int[] m_baseStats = new int[5];
    protected int[] m_baseResistances = new int[Unit.DamageSchoolCount];
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
    [NotVariable]
    private const int DefaultHealthAndPowerUpdateTime = 5000;
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
    [NotVariable]
    private static int _timeFromLastMpUpdate;
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
    protected WCell.RealmServer.Entities.Movement m_Movement;

    protected internal virtual void UpdateStrength()
    {
      this.SetInt32((UpdateFieldId) UnitFields.STAT0, this.GetBaseStatValue(StatType.Strength) + this.StrengthBuffPositive + this.StrengthBuffNegative);
      this.UpdateBlockChance();
      this.UpdateAllAttackPower();
    }

    protected internal virtual void UpdateStamina()
    {
      this.SetInt32((UpdateFieldId) UnitFields.STAT2, this.GetBaseStatValue(StatType.Stamina) + this.StaminaBuffPositive + this.StaminaBuffNegative);
      this.UpdateMaxHealth();
    }

    internal void UpdateAgility()
    {
      int agility = this.Agility;
      int num = this.GetBaseStatValue(StatType.Agility) + this.AgilityBuffPositive + this.AgilityBuffNegative;
      this.SetInt32((UpdateFieldId) UnitFields.STAT1, num);
      this.ModBaseResistance(DamageSchool.Physical, (num - agility) * Unit.ArmorPerAgility);
      this.UpdateDodgeChance();
      this.UpdateCritChance();
      this.UpdateAllAttackPower();
    }

    protected internal virtual void UpdateIntellect()
    {
      this.SetInt32((UpdateFieldId) UnitFields.STAT3, this.GetBaseStatValue(StatType.Intellect) + this.IntellectBuffPositive + this.IntellectBuffNegative);
      this.UpdateMaxPower();
    }

    protected internal virtual void UpdateSpirit()
    {
      this.SetInt32((UpdateFieldId) UnitFields.STAT4, this.GetBaseStatValue(StatType.Spirit) + this.SpiritBuffPositive + this.SpiritBuffNegative);
      this.UpdateNormalHealthRegen();
      if (this.Intellect == 0)
        return;
      this.UpdatePowerRegen();
    }

    protected internal virtual void UpdateStat(StatType stat)
    {
      switch (stat)
      {
        case StatType.Strength:
          this.UpdateStrength();
          break;
        case StatType.Agility:
          this.UpdateAgility();
          break;
        case StatType.Stamina:
          this.UpdateStamina();
          break;
        case StatType.Intellect:
          this.UpdateIntellect();
          break;
        case StatType.Spirit:
          this.UpdateSpirit();
          break;
      }
    }

    protected internal virtual void UpdateMaxHealth()
    {
      this.MaxHealth = (int) UnitUpdates.GetMultiMod(this.FloatMods[31], (float) (this.IntMods[31] + this.BaseHealth) + CharacterFormulas.CalculateHealthBonus(this.Level, this.Asda2Strength, this.Asda2Stamina, this.Class));
      this.UpdateHealthRegen();
    }

    /// <summary>Amount of mana, contributed by intellect</summary>
    protected internal virtual int IntellectManaBonus
    {
      get
      {
        return this.Intellect;
      }
    }

    public float Asda2Defence
    {
      get
      {
        return this._asda2Defence;
      }
      set
      {
        this._asda2Defence = value;
      }
    }

    public float Asda2MagicDefence
    {
      get
      {
        return this._asda2MagicDefence;
      }
      set
      {
        this._asda2MagicDefence = value;
      }
    }

    protected int CritDamageBonusPrc { get; set; }

    protected internal void UpdateMaxPower()
    {
      int num1 = this.BasePower + this.IntMods[1] + CharacterFormulas.CalculateManaBonus(this.Level, this.Class, this.Asda2Spirit);
      int num2 = num1 + (num1 * this.IntMods[2] + 50) / 100;
      if (num2 < 0)
        num2 = 0;
      this.MaxPower = num2;
      this.UpdatePowerRegen();
    }

    public void UpdateAsda2Defence()
    {
      this.Asda2Defence = UnitUpdates.GetMultiMod(this.FloatMods[4], (float) this.IntMods[20] + CharacterFormulas.ClaculateDefenceBonus(this.Level, this.Class, this.Asda2Agility));
    }

    public void UpdateAsda2MagicDefence()
    {
      this.Asda2MagicDefence = UnitUpdates.GetMultiMod(this.FloatMods[5], (float) this.IntMods[21] + CharacterFormulas.CalculateMagicDefencePointsBonus(this.Level, this.Class, this.Asda2Spirit));
    }

    public void UpdateAsda2DropChance()
    {
      this.Asda2DropChance = UnitUpdates.GetMultiMod(this.FloatMods[6] + CharacterFormulas.CalculateDropChanceBoost(this.Asda2Luck), 1f);
    }

    public void UpdateAsda2GoldAmount()
    {
      this.Asda2GoldAmountBoost = UnitUpdates.GetMultiMod(this.FloatMods[7] + CharacterFormulas.CalculateGoldAmountDropBoost(this.Level, this.Class, this.Asda2Luck), 1f);
    }

    public void UpdateAsda2ExpAmount()
    {
      this.Asda2ExpAmountBoost = UnitUpdates.GetMultiMod(this.FloatMods[8], 1f);
    }

    public void UpdateAsda2Luck()
    {
      this.Asda2Luck = UnitUpdates.GetMultiMod(this.FloatMods[9], this.IntMods[22] + this.Asda2BaseLuck);
      this.UpdateCritChance();
      this.UpdateAsda2DropChance();
      this.UpdateAsda2GoldAmount();
      this.UpdateCritDamageBonus();
    }

    public void UpdateAsda2Spirit()
    {
      this.Asda2Spirit = UnitUpdates.GetMultiMod(this.FloatMods[16], this.IntMods[29] + this.Asda2BaseSpirit);
      this.UpdateAsda2MagicDefence();
      this.UpdateMaxPower();
      this.UpdatePowerRegen();
    }

    public void UpdateAsda2Intellect()
    {
      this.Asda2Intellect = UnitUpdates.GetMultiMod(this.FloatMods[15], this.IntMods[28] + this.Asda2BaseIntellect);
      this.UpdateMainDamage();
      this.UpdateCritDamageBonus();
    }

    public void UpdateAsda2Stamina()
    {
      this.Asda2Stamina = UnitUpdates.GetMultiMod(this.FloatMods[14], this.IntMods[30] + this.Asda2BaseStamina);
      this.UpdateMaxHealth();
    }

    public void UpdateAsda2Strength()
    {
      this.Asda2Strength = UnitUpdates.GetMultiMod(this.FloatMods[12], this.IntMods[27] + this.Asda2BaseStrength);
      this.UpdateMainDamage();
      this.UpdateCritDamageBonus();
      this.UpdateMaxHealth();
    }

    public void UpdateAsda2Agility()
    {
      this.Asda2Agility = UnitUpdates.GetMultiMod(this.FloatMods[13], this.IntMods[26] + this.Asda2BaseAgility);
      this.UpdateCritChance();
      this.UpdateAllAttackTimes();
      this.UpdateDodgeChance();
      this.UpdateSpeedFactor();
      this.UpdateAsda2Defence();
      this.UpdateCritDamageBonus();
      this.UpdateMainDamage();
    }

    public void UpdateLightResistence()
    {
      this.Asda2LightResistence = (float) this.IntMods[20];
    }

    public void UpdateDarkResistence()
    {
      this.Asda2DarkResistence = (float) this.IntMods[19];
    }

    public void UpdateEarthResistence()
    {
      this.Asda2EarthResistence = (float) this.IntMods[18];
    }

    public void UpdateFireResistence()
    {
      this.Asda2FireResistence = (float) this.IntMods[17];
    }

    public void UpdateClimateResistence()
    {
      this.Asda2ClimateResistence = (float) this.IntMods[21];
    }

    public void UpdateWaterResistence()
    {
      this.Asda2WaterResistence = (float) this.IntMods[22];
    }

    public void UpdateLightAttribute()
    {
      this.Asda2LightAttribute = (float) this.IntMods[23];
    }

    public void UpdateDarkAttribute()
    {
      this.Asda2DarkAttribute = (float) this.IntMods[24];
    }

    public void UpdateEarthAttribute()
    {
      this.Asda2EarthAttribute = (float) this.IntMods[25];
    }

    public void UpdateFireAttribute()
    {
      this.Asda2FireAttribute = (float) this.IntMods[26];
    }

    public void UpdateClimateAttribute()
    {
      this.Asda2ClimateAttribute = (float) this.IntMods[27];
    }

    public void UpdateWaterAttribute()
    {
      this.Asda2WaterAttribute = (float) this.IntMods[28];
    }

    public void UpdateSpeedFactor()
    {
      float num = CharacterFormulas.CalcSpeedBonus(this.Level, this.Class, this.Asda2Agility);
      if ((double) num > 1.0)
        num = 1f;
      this.SpeedFactor = UnitUpdates.GetMultiMod(this.FloatMods[29] + num, Unit.DefaultSpeedFactor);
      Character character = this as Character;
      if (character == null)
        return;
      GlobalHandler.SendSpeedChangedResponse(character.Client);
    }

    public void UpdateCritDamageBonus()
    {
      this.CritDamageBonusPrc = CharacterFormulas.CalculateCriticalDamageBonus(this.Level, this.Class, this.Asda2Agility, this.Asda2Luck, this.Asda2Intellect, this.Asda2Strength);
    }

    public Unit Charm
    {
      get
      {
        return this.m_charm;
      }
      set
      {
        this.m_charm = value;
        if (value != null)
          this.SetEntityId((UpdateFieldId) UnitFields.CHARM, value.EntityId);
        else
          this.SetEntityId((UpdateFieldId) UnitFields.CHARM, EntityId.Zero);
      }
    }

    public Unit Charmer
    {
      get
      {
        return this.m_master;
      }
      set
      {
        this.SetEntityId((UpdateFieldId) UnitFields.CHARMEDBY, value != null ? value.EntityId : EntityId.Zero);
        this.Master = value;
      }
    }

    public Character CharmerCharacter
    {
      get
      {
        return this.m_master as Character;
      }
      set
      {
        this.Charmer = (Unit) value;
      }
    }

    public bool IsCharmed
    {
      get
      {
        return this.m_master != null;
      }
    }

    public Unit Summoner
    {
      get
      {
        return this.m_master;
      }
      set
      {
        this.SetEntityId((UpdateFieldId) UnitFields.SUMMONEDBY, value != null ? value.EntityId : EntityId.Zero);
        this.Master = value;
      }
    }

    public EntityId Creator
    {
      get
      {
        return this.GetEntityId((UpdateFieldId) UnitFields.CREATEDBY);
      }
      set
      {
        this.SetEntityId((UpdateFieldId) UnitFields.CREATEDBY, value);
      }
    }

    public EntityId Summon
    {
      get
      {
        return this.GetEntityId((UpdateFieldId) UnitFields.SUMMON);
      }
      set
      {
        this.SetEntityId((UpdateFieldId) UnitFields.SUMMON, value);
      }
    }

    /// <summary>
    /// The Unit's currently selected target.
    /// If set to null, also forces this Unit to leave combat mode.
    /// </summary>
    public Unit Target
    {
      get
      {
        if (this.m_target != null && !this.m_target.IsInWorld)
          this.Target = (Unit) null;
        return this.m_target;
      }
      set
      {
        if (this.m_target == value)
          return;
        if (value != null)
        {
          this.SetEntityId((UpdateFieldId) UnitFields.TARGET, value.EntityId);
          if (this is NPC)
            this.Orientation = this.GetAngleTowards((IHasPosition) value);
          else if (this is Character && value is Character && this != value)
            (value as Character).TargetersOnMe.Add((Character) this);
        }
        else
        {
          this.SetEntityId((UpdateFieldId) UnitFields.TARGET, EntityId.Zero);
          this.IsFighting = false;
          if (this is Character && this.m_target is Character && this != value)
            (this.m_target as Character).TargetersOnMe.Remove((Character) this);
        }
        this.m_target = value;
        this.CancelPendingAbility();
      }
    }

    /// <summary>As long as this count is up, cannot leave combat</summary>
    public int NPCAttackerCount { get; internal set; }

    public WorldObject ChannelObject
    {
      get
      {
        return this.m_channeled;
      }
      set
      {
        this.SetEntityId((UpdateFieldId) UnitFields.CHANNEL_OBJECT, value != null ? value.EntityId : EntityId.Zero);
        this.m_channeled = value;
      }
    }

    public ITransportInfo TransportInfo
    {
      get
      {
        if (this.m_vehicleSeat == null)
          return (ITransportInfo) this.m_transport;
        return (ITransportInfo) this.m_vehicleSeat.Vehicle;
      }
    }

    /// <summary>
    /// The <see cref="P:WCell.RealmServer.Entities.Unit.Transport" /> that this Unit is on (if any).
    /// </summary>
    public Transport Transport
    {
      get
      {
        return this.m_transport;
      }
      internal set
      {
        this.m_transport = value;
      }
    }

    public Vector3 TransportPosition
    {
      get
      {
        return this.m_transportPosition;
      }
      internal set
      {
        this.m_transportPosition = value;
      }
    }

    public float TransportOrientation
    {
      get
      {
        return this.m_transportOrientation;
      }
      internal set
      {
        this.m_transportOrientation = value;
      }
    }

    public uint TransportTime
    {
      get
      {
        return Utility.GetSystemTime() - this.m_transportTime;
      }
      internal set
      {
        this.m_transportTime = value;
      }
    }

    public byte TransportSeat
    {
      get
      {
        if (this.VehicleSeat == null)
          return 0;
        return this.VehicleSeat.Index;
      }
    }

    /// <summary>Currently occupied VehicleSeat (if riding in vehicle)</summary>
    public VehicleSeat VehicleSeat
    {
      get
      {
        return this.m_vehicleSeat;
      }
    }

    public Vehicle Vehicle
    {
      get
      {
        if (this.m_vehicleSeat == null)
          return (Vehicle) null;
        return this.m_vehicleSeat.Vehicle;
      }
    }

    public virtual int MaxLevel
    {
      get
      {
        return int.MaxValue;
      }
      internal set
      {
      }
    }

    /// <summary>The Level of this Unit.</summary>
    public virtual int Level
    {
      get
      {
        return this.GetInt32(UnitFields.LEVEL);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.LEVEL, value);
        this.OnLevelChanged();
      }
    }

    protected virtual void OnLevelChanged()
    {
    }

    public override int CasterLevel
    {
      get
      {
        return this.Level;
      }
    }

    public override Faction Faction
    {
      get
      {
        return this.m_faction;
      }
      set
      {
        if (value == null)
          throw new NullReferenceException(string.Format("Faction cannot be set to null (Unit: {0}, Map: {1})", (object) this, (object) this.m_Map));
        this.m_faction = value;
        this.SetUInt32((UpdateFieldId) UnitFields.FACTIONTEMPLATE, value.Template.Id);
      }
    }

    public abstract Faction DefaultFaction { get; }

    public override FactionId FactionId
    {
      get
      {
        return this.m_faction.Id;
      }
      set
      {
        Faction faction = FactionMgr.Get(value);
        if (faction == null)
          return;
        this.Faction = faction;
      }
    }

    public FactionGroup FactionGroup
    {
      get
      {
        return this.m_faction.Group;
      }
    }

    public uint FactionTemplateId
    {
      get
      {
        return this.m_faction.Template.Id;
      }
    }

    public UnitFlags UnitFlags
    {
      get
      {
        return (UnitFlags) this.GetUInt32(UnitFields.FLAGS);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.FLAGS, (uint) value);
      }
    }

    public UnitFlags2 UnitFlags2
    {
      get
      {
        return (UnitFlags2) this.GetUInt32(UnitFields.FLAGS_2);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.FLAGS_2, (uint) value);
      }
    }

    public float BoundingRadius
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.BOUNDINGRADIUS);
      }
      set
      {
        this.SetFloat((UpdateFieldId) UnitFields.BOUNDINGRADIUS, value);
      }
    }

    public float BoundingCollisionRadius { get; set; }

    public UnitModelInfo Model
    {
      get
      {
        return this.m_model;
      }
      set
      {
        this.m_model = value;
        this.SetUInt32((UpdateFieldId) UnitFields.DISPLAYID, this.m_model.DisplayId);
        this.BoundingRadius = this.m_model.BoundingRadius * this.ScaleX;
        this.BoundingCollisionRadius = this.BoundingRadius * 2.1f;
        this.CombatReach = this.m_model.CombatReach * this.ScaleX;
      }
    }

    public virtual uint DisplayId
    {
      get
      {
        return this.GetUInt32(UnitFields.DISPLAYID);
      }
      set
      {
        UnitModelInfo modelInfo = UnitMgr.GetModelInfo(value);
        if (modelInfo == null)
          Unit.log.Error("Trying to set DisplayId of {0} to an invalid value: {1}", (object) this, (object) value);
        else
          this.Model = modelInfo;
      }
    }

    public uint NativeDisplayId
    {
      get
      {
        return this.GetUInt32(UnitFields.NATIVEDISPLAYID);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.NATIVEDISPLAYID, value);
      }
    }

    public uint MountDisplayId
    {
      get
      {
        return this.GetUInt32(UnitFields.MOUNTDISPLAYID);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.MOUNTDISPLAYID, value);
      }
    }

    public Asda2ItemId VirtualItem1
    {
      get
      {
        return (Asda2ItemId) this.GetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.VIRTUAL_ITEM_SLOT_ID, (uint) value);
      }
    }

    public Asda2ItemId VirtualItem2
    {
      get
      {
        return (Asda2ItemId) this.GetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_2);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.VIRTUAL_ITEM_SLOT_ID_2, (uint) value);
      }
    }

    public Asda2ItemId VirtualItem3
    {
      get
      {
        return (Asda2ItemId) this.GetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_3);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.VIRTUAL_ITEM_SLOT_ID_3, (uint) value);
      }
    }

    public uint PetNumber
    {
      get
      {
        return this.GetUInt32(UnitFields.PETNUMBER);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.PETNUMBER, value);
      }
    }

    /// <summary>Changing this makes clients send a pet name query</summary>
    public uint PetNameTimestamp
    {
      get
      {
        return this.GetUInt32(UnitFields.PET_NAME_TIMESTAMP);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.PET_NAME_TIMESTAMP, value);
      }
    }

    public int PetExperience
    {
      get
      {
        return this.GetInt32(UnitFields.PETEXPERIENCE);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.PETEXPERIENCE, value);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public int NextPetLevelExperience
    {
      get
      {
        return this.GetInt32(UnitFields.PETNEXTLEVELEXP);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.PETNEXTLEVELEXP, value);
      }
    }

    public UnitDynamicFlags DynamicFlags
    {
      get
      {
        return (UnitDynamicFlags) this.GetUInt32(UnitFields.DYNAMIC_FLAGS);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.DYNAMIC_FLAGS, (uint) value);
      }
    }

    public SpellId ChannelSpell
    {
      get
      {
        return (SpellId) this.GetUInt32(UnitFields.CHANNEL_SPELL);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.CHANNEL_SPELL, (uint) value);
      }
    }

    public float CastSpeedFactor
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.MOD_CAST_SPEED);
      }
      set
      {
        this.SetFloat((UpdateFieldId) UnitFields.MOD_CAST_SPEED, value);
      }
    }

    /// <summary>Whether this Unit is summoned</summary>
    public bool IsSummoned
    {
      get
      {
        return this.CreationSpellId != SpellId.None;
      }
    }

    /// <summary>Whether this Unit belongs to someone</summary>
    public bool IsMinion
    {
      get
      {
        return this.m_master != this;
      }
    }

    /// <summary>The spell that created this Unit</summary>
    public SpellId CreationSpellId
    {
      get
      {
        return (SpellId) this.GetUInt32(UnitFields.CREATED_BY_SPELL);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.CREATED_BY_SPELL, (uint) value);
      }
    }

    public NPCFlags NPCFlags
    {
      get
      {
        return (NPCFlags) this.GetUInt32(UnitFields.NPC_FLAGS);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.NPC_FLAGS, (uint) value);
        this.MarkUpdate((UpdateFieldId) UnitFields.DYNAMIC_FLAGS);
      }
    }

    public EmoteType EmoteState
    {
      get
      {
        return (EmoteType) this.GetUInt32(UnitFields.NPC_EMOTESTATE);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.NPC_EMOTESTATE, (uint) value);
      }
    }

    public float HoverHeight
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.HOVERHEIGHT);
      }
      set
      {
        this.SetFloat((UpdateFieldId) UnitFields.HOVERHEIGHT, value);
      }
    }

    /// <summary>Pet's Training Points, deprecated</summary>
    public uint TrainingPoints { get; set; }

    public int Strength
    {
      get
      {
        return this.GetInt32(UnitFields.STAT0);
      }
    }

    public int Agility
    {
      get
      {
        return this.GetInt32(UnitFields.STAT1);
      }
    }

    public int Stamina
    {
      get
      {
        return this.GetInt32(UnitFields.STAT2);
      }
    }

    /// <summary>
    /// The amount of stamina that does not contribute to health.
    /// </summary>
    public virtual int StaminaWithoutHealthContribution
    {
      get
      {
        return 20;
      }
    }

    public int Intellect
    {
      get
      {
        return this.GetInt32(UnitFields.STAT3);
      }
    }

    public int Spirit
    {
      get
      {
        return this.GetInt32(UnitFields.STAT4);
      }
    }

    internal int[] BaseStats
    {
      get
      {
        return this.m_baseStats;
      }
    }

    /// <summary>Stat value, after modifiers</summary>
    public int GetTotalStatValue(StatType stat)
    {
      return this.GetInt32((UnitFields) (84 + stat));
    }

    public int GetBaseStatValue(StatType stat)
    {
      return this.m_baseStats[(int) stat];
    }

    public virtual int GetUnmodifiedBaseStatValue(StatType stat)
    {
      return this.m_baseStats[(int) stat];
    }

    public void SetBaseStat(StatType stat, int value)
    {
      this.SetBaseStat(stat, value, true);
    }

    public void SetBaseStat(StatType stat, int value, bool update)
    {
      this.m_baseStats[(int) stat] = value;
      if (!update)
        return;
      this.UpdateStat(stat);
    }

    public void ModBaseStat(StatType stat, int delta)
    {
      this.SetBaseStat(stat, this.m_baseStats[(int) stat] + delta);
    }

    public void AddStatMod(StatType stat, int delta, bool passive)
    {
      if (passive)
        this.ModBaseStat(stat, delta);
      else
        this.AddStatMod(stat, delta);
    }

    public void AddStatMod(StatType stat, int delta)
    {
      if (delta == 0)
        return;
      UnitFields unitFields = delta <= 0 ? UnitFields.NEGSTAT0 : UnitFields.POSSTAT0;
      this.SetInt32((UpdateFieldId) (unitFields + (int) stat), this.GetInt32(unitFields + (int) stat) + delta);
      this.UpdateStat(stat);
    }

    public void RemoveStatMod(StatType stat, int delta, bool passive)
    {
      if (passive)
        this.ModBaseStat(stat, -delta);
      else
        this.RemoveStatMod(stat, delta);
    }

    /// <summary>
    /// Removes the given delta from positive or negative stat buffs correspondingly
    /// </summary>
    public void RemoveStatMod(StatType stat, int delta)
    {
      if (delta == 0)
        return;
      UnitFields unitFields = delta <= 0 ? UnitFields.NEGSTAT0 : UnitFields.POSSTAT0;
      this.SetInt32((UpdateFieldId) (unitFields + (int) stat), this.GetInt32(unitFields + (int) stat) - delta);
      this.UpdateStat(stat);
    }

    public int StrengthBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.POSSTAT0);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.POSSTAT0, value);
        this.UpdateStrength();
      }
    }

    public int AgilityBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.POSSTAT1);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.POSSTAT1, value);
        this.UpdateAgility();
      }
    }

    public int StaminaBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.POSSTAT2);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.POSSTAT2, value);
        this.UpdateStamina();
      }
    }

    public int IntellectBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.POSSTAT3);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.POSSTAT3, value);
        this.UpdateIntellect();
      }
    }

    public int SpiritBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.POSSTAT4);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.POSSTAT4, value);
        this.UpdateSpirit();
      }
    }

    public int StrengthBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.NEGSTAT0);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.NEGSTAT0, value);
        this.UpdateStrength();
      }
    }

    public int AgilityBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.NEGSTAT1);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.NEGSTAT1, value);
        this.UpdateAgility();
      }
    }

    public int StaminaBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.NEGSTAT2);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.NEGSTAT2, value);
        this.UpdateStamina();
      }
    }

    public int IntellectBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.NEGSTAT3);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.NEGSTAT3, value);
        this.UpdateIntellect();
      }
    }

    public int SpiritBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.NEGSTAT4);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.NEGSTAT4, value);
        this.UpdateSpirit();
      }
    }

    /// <summary>Physical resist</summary>
    public int Armor
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCES);
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) UnitFields.RESISTANCES, value);
      }
    }

    public int HolyResist
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCES_2);
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) UnitFields.RESISTANCES_2, value);
      }
    }

    public int FireResist
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCES_3);
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) UnitFields.RESISTANCES_3, value);
      }
    }

    public int NatureResist
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCES_4);
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) UnitFields.RESISTANCES_4, value);
      }
    }

    public int FrostResist
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCES_5);
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) UnitFields.RESISTANCES_5, value);
      }
    }

    public int ShadowResist
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCES_6);
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) UnitFields.RESISTANCES_6, value);
      }
    }

    public int ArcaneResist
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCES_7);
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) UnitFields.RESISTANCES_7, value);
      }
    }

    internal int[] BaseResistances
    {
      get
      {
        return this.m_baseResistances;
      }
    }

    /// <summary>
    /// Returns the total resistance-value of the given school
    /// </summary>
    public int GetResistance(DamageSchool school)
    {
      int num = this.GetBaseResistance(school) + this.GetInt32((UnitFields) (106U + school)) + this.GetInt32((UnitFields) (113U + school));
      if (num < 0)
        num = 0;
      return num;
    }

    /// <summary>Returns the base resistance-value of the given school</summary>
    public int GetBaseResistance(DamageSchool school)
    {
      return this.m_baseResistances[(int) school];
    }

    public void SetBaseResistance(DamageSchool school, int value)
    {
      if (value < 0)
        value = 0;
      this.m_baseResistances[(uint)school] = value;
      this.OnResistanceChanged(school);
    }

    /// <summary>
    /// Adds the given amount to the base of the given resistance for the given school
    /// </summary>
    public void ModBaseResistance(DamageSchool school, int delta)
    {
      this.SetBaseResistance(school, this.m_baseResistances[(int) school] + delta);
    }

    /// <summary>
    /// Adds the given amount to the base of all given resistance-schools
    /// </summary>
    public void ModBaseResistance(uint[] schools, int delta)
    {
      foreach (DamageSchool school in schools)
        this.ModBaseResistance(school, delta);
    }

    public void AddResistanceBuff(DamageSchool school, int delta)
    {
      if (delta == 0)
        return;
      UnitFields unitFields = delta <= 0 ? UnitFields.RESISTANCEBUFFMODSNEGATIVE : UnitFields.RESISTANCEBUFFMODSPOSITIVE;
      this.SetInt32((UpdateFieldId) ((UnitFields) ((int) unitFields + (int) school)), this.GetInt32((UnitFields) ((int) unitFields + (int) school)) + delta);
      this.OnResistanceChanged(school);
    }

    /// <summary>
    /// Removes the given delta from positive or negative stat buffs correspondingly
    /// </summary>
    public void RemoveResistanceBuff(DamageSchool school, int delta)
    {
      if (delta == 0)
        return;
      UnitFields unitFields = delta <= 0 ? UnitFields.RESISTANCEBUFFMODSNEGATIVE : UnitFields.RESISTANCEBUFFMODSPOSITIVE;
      this.SetInt32((UpdateFieldId) ((UnitFields) ((int) unitFields + (int) school)), this.GetInt32((UnitFields) ((int) unitFields + (int) school)) - delta);
      this.OnResistanceChanged(school);
    }

    protected virtual void OnResistanceChanged(DamageSchool school)
    {
      this.SetInt32((UpdateFieldId) ((UnitFields) (99U + school)), this.GetBaseResistance(school) + this.GetResistanceBuffPositive(school) - this.GetResistanceBuffNegative(school));
    }

    public int GetResistanceBuffPositive(DamageSchool school)
    {
      return this.GetInt32((UnitFields) (106U + school));
    }

    public int GetResistanceBuffNegative(DamageSchool school)
    {
      return this.GetInt32((UnitFields) (113U + school));
    }

    public int ArmorBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE);
      }
    }

    public int HolyResistBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_2);
      }
    }

    public int FireResistBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_3);
      }
    }

    public int NatureResistBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_4);
      }
    }

    public int FrostResistBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_5);
      }
    }

    public int ShadowResistBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_6);
      }
    }

    public int ArcaneResistBuffPositive
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSPOSITIVE_7);
      }
    }

    public int ArmorBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE);
      }
    }

    public int HolyResistBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_2);
      }
    }

    public int FireResistBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_3);
      }
    }

    public int NatureResistBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_4);
      }
    }

    public int FrostResistBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_5);
      }
    }

    public int ShadowResistBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_6);
      }
    }

    public int ArcaneResistBuffNegative
    {
      get
      {
        return this.GetInt32(UnitFields.RESISTANCEBUFFMODSNEGATIVE_7);
      }
    }

    public AuraStateMask AuraState
    {
      get
      {
        return (AuraStateMask) this.GetUInt32(UnitFields.AURASTATE);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) UnitFields.AURASTATE, (uint) value);
        if (!(this.m_auras is PlayerAuraCollection) || this.AuraState == value)
          return;
        ((PlayerAuraCollection) this.m_auras).OnAuraStateChanged();
      }
    }

    /// <summary>
    /// Helper function for Aurastate related fix and Conflagrate spell.
    /// see UpdateFieldHandler/Warlockfixes
    /// </summary>
    public Spell GetStrongestImmolate()
    {
      return (Spell) null;
    }

    public byte[] UnitBytes0
    {
      get
      {
        return this.GetByteArray((UpdateFieldId) UnitFields.BYTES_0);
      }
      set
      {
        this.SetByteArray((UpdateFieldId) UnitFields.BYTES_0, value);
      }
    }

    public virtual RaceId Race
    {
      get
      {
        return (RaceId) this.GetByte((UpdateFieldId) UnitFields.BYTES_0, 0);
      }
      set
      {
        this.SetByte((UpdateFieldId) UnitFields.BYTES_0, 0, (byte) value);
      }
    }

    public virtual ClassId Class
    {
      get
      {
        return (ClassId) this.GetByte((UpdateFieldId) UnitFields.BYTES_0, 1);
      }
      set
      {
        this.SetByte((UpdateFieldId) UnitFields.BYTES_0, 1, (byte) value);
      }
    }

    public BaseClass GetBaseClass()
    {
      return ArchetypeMgr.GetClass(this.Class);
    }

    /// <summary>Race of the character.</summary>
    public RaceMask RaceMask
    {
      get
      {
        return (RaceMask) (1 << (int) (this.Race - 1 & (RaceId.Skeleton | RaceId.End)));
      }
    }

    /// <summary>RaceMask2 of the character.</summary>
    public RaceMask2 RaceMask2
    {
      get
      {
        return (RaceMask2) (1 << (int) (this.Race & (RaceId.Skeleton | RaceId.End)));
      }
    }

    /// <summary>Class of the character.</summary>
    public ClassMask ClassMask
    {
      get
      {
        return (ClassMask) (1 << (int) (this.Class - 1U & (ClassId) 31));
      }
    }

    /// <summary>ClassMask2 of the character.</summary>
    public ClassMask2 ClassMask2
    {
      get
      {
        return (ClassMask2) (1 << (int) (this.Class & (ClassId) 31));
      }
    }

    public virtual GenderType Gender
    {
      get
      {
        return (GenderType) this.GetByte((UpdateFieldId) UnitFields.BYTES_0, 2);
      }
      set
      {
        this.SetByte((UpdateFieldId) UnitFields.BYTES_0, 2, (byte) value);
      }
    }

    /// <summary>
    /// Make sure the PowerType is valid or it will crash the client
    /// </summary>
    public virtual PowerType PowerType
    {
      get
      {
        return (PowerType) this.GetByte((UpdateFieldId) UnitFields.BYTES_0, 3);
      }
      set
      {
        this.SetByte((UpdateFieldId) UnitFields.BYTES_0, 3, (byte) ((uint) (byte) value % 7U));
      }
    }

    public byte[] UnitBytes1
    {
      get
      {
        return this.GetByteArray((UpdateFieldId) UnitFields.BYTES_1);
      }
      set
      {
        this.SetByteArray((UpdateFieldId) UnitFields.BYTES_1, value);
      }
    }

    public virtual StandState StandState
    {
      get
      {
        return (StandState) this.GetByte((UpdateFieldId) UnitFields.BYTES_1, 0);
      }
      set
      {
        this.SetByte((UpdateFieldId) UnitFields.BYTES_1, 0, (byte) value);
      }
    }

    public StateFlag StateFlags
    {
      get
      {
        return (StateFlag) this.GetByte((UpdateFieldId) UnitFields.BYTES_1, 2);
      }
      set
      {
        this.SetByte((UpdateFieldId) UnitFields.BYTES_1, 2, (byte) value);
      }
    }

    public byte UnitBytes1_3
    {
      get
      {
        return this.GetByte((UpdateFieldId) UnitFields.BYTES_1, 3);
      }
      set
      {
        this.SetByte((UpdateFieldId) UnitFields.BYTES_1, 3, value);
      }
    }

    public byte[] UnitBytes2
    {
      get
      {
        return this.GetByteArray((UpdateFieldId) UnitFields.BYTES_2);
      }
      set
      {
        this.SetByteArray((UpdateFieldId) UnitFields.BYTES_2, value);
      }
    }

    /// <summary>Set to 0x01 for Spirit Healers, Totems (?)</summary>
    public SheathType SheathType
    {
      get
      {
        return (SheathType) this.GetByte((UpdateFieldId) UnitFields.BYTES_2, 0);
      }
      set
      {
        this.SetByte((UpdateFieldId) UnitFields.BYTES_2, 0, (byte) value);
      }
    }

    /// <summary>
    /// Flags
    /// 0x1 - In PVP
    /// 0x4 - Free for all PVP
    /// 0x8 - In PVP Sanctuary
    /// </summary>
    public PvPState PvPState
    {
      get
      {
        return (PvPState) this.GetByte((UpdateFieldId) UnitFields.BYTES_2, 1);
      }
      set
      {
        this.SetByte((UpdateFieldId) UnitFields.BYTES_2, 1, (byte) value);
      }
    }

    /// <summary>
    /// </summary>
    public PetState PetState
    {
      get
      {
        return (PetState) this.GetByte((UpdateFieldId) UnitFields.BYTES_2, 2);
      }
      set
      {
        this.SetByte((UpdateFieldId) UnitFields.BYTES_2, 2, (byte) value);
      }
    }

    /// <summary>The entry of the current shapeshift form</summary>
    public ShapeshiftEntry ShapeshiftEntry
    {
      get
      {
        return SpellHandler.ShapeshiftEntries.Get<ShapeshiftEntry>((uint) this.ShapeshiftForm);
      }
    }

    public ShapeshiftForm ShapeshiftForm
    {
      get
      {
        return (ShapeshiftForm) this.GetByte((UpdateFieldId) UnitFields.BYTES_2, 3);
      }
      set
      {
        ShapeshiftForm shapeshiftForm = this.ShapeshiftForm;
        if (shapeshiftForm != ShapeshiftForm.Normal)
        {
          ShapeshiftEntry shapeshiftEntry = SpellHandler.ShapeshiftEntries.Get<ShapeshiftEntry>((uint) value);
          if (shapeshiftEntry != null && this.HasSpells)
          {
            foreach (SpellId defaultActionBarSpell in shapeshiftEntry.DefaultActionBarSpells)
            {
              if (defaultActionBarSpell != SpellId.None)
                this.Spells.Remove(defaultActionBarSpell);
            }
          }
        }
        ShapeshiftEntry shapeshiftEntry1 = SpellHandler.ShapeshiftEntries.Get<ShapeshiftEntry>((uint) value);
        if (shapeshiftEntry1 != null)
        {
          UnitModelInfo unitModelInfo = this.FactionGroup != FactionGroup.Horde || shapeshiftEntry1.ModelIdHorde == 0U ? shapeshiftEntry1.ModelAlliance : shapeshiftEntry1.ModelHorde;
          if (unitModelInfo != null)
            this.Model = unitModelInfo;
          if (this.IsPlayer)
          {
            foreach (SpellId defaultActionBarSpell in shapeshiftEntry1.DefaultActionBarSpells)
            {
              if (defaultActionBarSpell != SpellId.None)
                this.Spells.AddSpell(defaultActionBarSpell);
            }
          }
          if (shapeshiftEntry1.PowerType != PowerType.End)
            this.PowerType = shapeshiftEntry1.PowerType;
          else
            this.SetDefaultPowerType();
        }
        else
        {
          if (shapeshiftForm != ShapeshiftForm.Normal)
            this.DisplayId = this.NativeDisplayId;
          this.SetDefaultPowerType();
        }
        this.SetByte((UpdateFieldId) UnitFields.BYTES_2, 3, (byte) value);
        if (!(this.m_auras is PlayerAuraCollection))
          return;
        ((PlayerAuraCollection) this.m_auras).OnShapeshiftFormChanged();
      }
    }

    /// <summary>Sets this Unit's default PowerType</summary>
    public void SetDefaultPowerType()
    {
      BaseClass baseClass = this.GetBaseClass();
      if (baseClass != null)
        this.PowerType = baseClass.DefaultPowerType;
      else
        this.PowerType = PowerType.Mana;
    }

    public ShapeshiftMask ShapeshiftMask
    {
      get
      {
        if (this.ShapeshiftForm == ShapeshiftForm.Normal)
          return ShapeshiftMask.None;
        return (ShapeshiftMask) (1 << (int) (this.ShapeshiftForm - 1 & ShapeshiftForm.Moonkin));
      }
    }

    /// <summary>Resets health, Power and Auras</summary>
    public void Cleanse()
    {
      foreach (Aura aura in this.m_auras)
      {
        if (aura.CasterUnit != this)
          aura.Remove(true);
      }
      this.Health = this.MaxHealth;
      this.Power = this.BasePower;
    }

    /// <summary>
    /// Whether this is actively controlled by a player.
    /// Not to be confused with IsOwnedByPlayer.
    /// </summary>
    public override bool IsPlayerControlled
    {
      get
      {
        return this.UnitFlags.HasAnyFlag(UnitFlags.PlayerControlled);
      }
    }

    /// <summary>If this is not an Honorless Target</summary>
    public bool YieldsXpOrHonor { get; set; }

    public UnitExtraFlags ExtraFlags { get; set; }

    public void Kill()
    {
      this.Kill((Unit) null);
    }

    public void Kill(Unit killer)
    {
      if (killer != null)
      {
        if (this.FirstAttacker == null)
          this.FirstAttacker = killer;
        this.LastKiller = killer;
      }
      this.Health = 0;
    }

    /// <summary>
    /// This Unit's current Health.
    /// Health cannot exceed MaxHealth.
    /// If Health reaches 0, the Unit dies.
    /// If Health is 0 and increases, the Unit gets resurrected.
    /// </summary>
    public virtual int Health
    {
      get
      {
        return (int) this.GetUInt32(UnitFields.HEALTH);
      }
      set
      {
        int health = this.Health;
        int maxHealth = this.MaxHealth;
        if (value >= maxHealth)
          value = maxHealth;
        else if (value < 0)
          value = 0;
        if (value == health)
          return;
        if (value < 1)
        {
          Character character = this as Character;
          if (character != null && character.IsAsda2Dueling)
          {
            character.Asda2Duel.Losser = character;
            this.SetUInt32((UpdateFieldId) UnitFields.HEALTH, (uint) maxHealth);
          }
          else
            this.Die(false);
        }
        else
        {
          this.SetUInt32((UpdateFieldId) UnitFields.HEALTH, (uint) value);
          this.UpdateHealthAuraState();
          if (health == 0)
            this.DecMechanicCount(SpellMechanic.Rooted, false);
          if (this.IsAlive && health >= 1)
            return;
          this.OnResurrect();
        }
      }
    }

    /// <summary>Base maximum health, before modifiers.</summary>
    public int BaseHealth
    {
      get
      {
        return this.GetInt32(UnitFields.BASE_HEALTH);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.BASE_HEALTH, value);
        this.UpdateMaxHealth();
      }
    }

    /// <summary>
    /// Total maximum Health of this Unit.
    /// In order to change this value, set BaseHealth.
    /// </summary>
    public virtual int MaxHealth
    {
      get
      {
        return this.GetInt32(UnitFields.MAXHEALTH);
      }
      internal set
      {
        if (this.Health > value)
          this.Health = value;
        this.SetInt32((UpdateFieldId) UnitFields.MAXHEALTH, value);
      }
    }

    public int MaxHealthModFlat
    {
      get
      {
        return this.m_maxHealthModFlat;
      }
      set
      {
        this.m_maxHealthModFlat = value;
        this.UpdateMaxHealth();
      }
    }

    public float MaxHealthModScalar
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.MAXHEALTHMODIFIER);
      }
      set
      {
        this.SetFloat((UpdateFieldId) UnitFields.MAXHEALTHMODIFIER, value);
        this.UpdateMaxHealth();
      }
    }

    /// <summary>Current amount of health in percent</summary>
    public int HealthPct
    {
      get
      {
        int maxHealth = this.MaxHealth;
        return (100 * this.Health + (maxHealth >> 1)) / maxHealth;
      }
      set
      {
        this.Health = (value * this.MaxHealth + 50) / 100;
      }
    }

    /// <summary>Current amount of power in percent</summary>
    public int PowerPct
    {
      get
      {
        int maxPower = this.MaxPower;
        return (100 * this.Power + (maxPower >> 1)) / maxPower;
      }
      set
      {
        this.Power = (value * this.MaxPower + 50) / 100;
      }
    }

    /// <summary>
    /// </summary>
    protected void UpdateHealthAuraState()
    {
      int healthPct = this.HealthPct;
      if (healthPct <= 20)
        this.AuraState = this.AuraState & ~(AuraStateMask.Health35Percent | AuraStateMask.HealthAbove75Pct) | AuraStateMask.Health20Percent;
      else if (healthPct <= 35)
        this.AuraState = this.AuraState & ~(AuraStateMask.Health20Percent | AuraStateMask.HealthAbove75Pct) | AuraStateMask.Health35Percent;
      else if (healthPct <= 75)
        this.AuraState &= ~(AuraStateMask.Health20Percent | AuraStateMask.Health35Percent | AuraStateMask.HealthAbove75Pct);
      else
        this.AuraState = this.AuraState & ~(AuraStateMask.Health20Percent | AuraStateMask.Health35Percent) | AuraStateMask.HealthAbove75Pct;
    }

    /// <summary>The flat PowerCostModifier for your default Power</summary>
    public int PowerCostModifier
    {
      get
      {
        return this.GetInt32((UnitFields) (131 + this.PowerType));
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) ((UnitFields) (131 + this.PowerType)), value);
      }
    }

    /// <summary>The PowerCostMultiplier for your default Power</summary>
    public float PowerCostMultiplier
    {
      get
      {
        return this.GetFloat((UpdateFieldId) ((UnitFields) (138 + this.PowerType)));
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) ((UnitFields) (138 + this.PowerType)), value);
      }
    }

    /// <summary>Base maximum power, before modifiers.</summary>
    public int BasePower
    {
      get
      {
        return this.GetInt32(UnitFields.BASE_MANA);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.BASE_MANA, value);
        this.UpdateMaxPower();
        if (this.PowerType == PowerType.Rage || this.PowerType == PowerType.Energy)
          return;
        this.Power = this.MaxPower;
      }
    }

    public void SetBasePowerDontUpdate(int value)
    {
      this.SetInt32((UpdateFieldId) UnitFields.BASE_MANA, value);
      if (this.PowerType == PowerType.Rage || this.PowerType == PowerType.Energy)
        return;
      this.Power = this.MaxPower;
    }

    /// <summary>
    /// The amount of the Unit's default Power (Mana, Energy, Rage, Happiness etc)
    /// </summary>
    public virtual int Power
    {
      get
      {
        return this._power;
      }
      set
      {
        if (value > this.MaxPower)
          this._power = this.MaxPower;
        else
          this._power = value;
      }
    }

    public bool IsSitting
    {
      get
      {
        return this._isSitting;
      }
      set
      {
        if (this._isSitting == value)
          return;
        this._isSitting = value;
        Character chr = this as Character;
        if (this.Map == null || chr == null)
          return;
        this.UpdatePowerRegen();
        this.Map.CallDelayed(100, (Action) (() => Asda2CharacterHandler.SendEmoteResponse(chr, value ? (short) 108 : (short) 109, (byte) 0, 0.0f, 0.0f)));
      }
    }

    internal void UpdatePower(int delayMillis)
    {
      this._tempPower += this.PowerRegenPerTickActual * (float) delayMillis / (float) RegenerationFormulas.RegenTickDelayMillis;
      Unit._timeFromLastMpUpdate += delayMillis;
      if (Unit._timeFromLastMpUpdate <= 5000)
        return;
      Unit._timeFromLastMpUpdate = 0;
      if ((double) this._tempPower <= 1.0)
        return;
      int tempPower = (int) this._tempPower;
      this._tempPower -= (float) tempPower;
      if (this.Power >= this.MaxPower)
        return;
      this.Power += tempPower;
    }

    protected static void SendPowerUpdates(Character chr)
    {
      Asda2CharacterHandler.SendCharMpUpdateResponse(chr);
      if (chr.IsInGroup)
        Asda2GroupHandler.SendPartyMemberInitialInfoResponse(chr);
      if (!chr.IsSoulmated)
        return;
      Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(chr.Client);
    }

    /// <summary>
    /// The max amount of the Unit's default Power (Mana, Energy, Rage, Happiness etc)
    /// NOTE: This is not related to Homer Simpson nor to any brand of hair blowers
    /// </summary>
    public virtual int MaxPower
    {
      get
      {
        return this.GetInt32((UnitFields) (33 + this.PowerType));
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) ((UnitFields) (33 + this.PowerType)), value);
      }
    }

    public virtual float ParryChance
    {
      get
      {
        return 5f;
      }
      internal set
      {
      }
    }

    /// <summary>
    /// Amount of additional yards to be allowed to jump without having any damage inflicted.
    /// TODO: Implement correctly (needs client packets)
    /// </summary>
    public int SafeFall { get; internal set; }

    public int AoEDamageModifierPct { get; set; }

    public virtual uint Defense
    {
      get
      {
        return (uint) (5 * this.Level);
      }
      internal set
      {
      }
    }

    public virtual TalentCollection Talents
    {
      get
      {
        return (TalentCollection) null;
      }
    }

    public float Asda2DropChance
    {
      get
      {
        return this._asda2DropChance;
      }
      set
      {
        this._asda2DropChance = value;
      }
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
      get
      {
        return this._asda2GoldAmountBoost;
      }
      set
      {
        this._asda2GoldAmountBoost = value;
      }
    }

    public float Asda2ExpAmountBoost
    {
      get
      {
        return this._asda2ExpAmountBoost;
      }
      set
      {
        this._asda2ExpAmountBoost = value;
      }
    }

    public bool IsVisible
    {
      get
      {
        return this._isVisible;
      }
      set
      {
        this._isVisible = value;
      }
    }

    public DateTime CastingTill { get; set; }

    public DateTime NextSpellUpdate { get; set; }

    protected override UpdateFieldCollection _UpdateFieldInfos
    {
      get
      {
        return Unit.UpdateFieldInfos;
      }
    }

    protected Unit()
    {
      this.Type |= ObjectTypes.Unit;
      this.m_isInCombat = false;
      this.m_attackTimer = new TimerEntry(new Action<int>(this.CombatTick));
      this.CastSpeedFactor = 1f;
      this.ResetMechanicDefaults();
      this.m_flying = this.m_waterWalk = this.m_hovering = this.m_featherFalling = 0U;
      this.m_canMove = this.m_canInteract = this.m_canHarm = this.m_canCastSpells = true;
    }

    public int GetBaseLuck()
    {
      return 10 * this.Level;
    }

    /// <summary>The Unit that attacked this NPC first.</summary>
    public Unit FirstAttacker
    {
      get
      {
        return this.m_FirstAttacker;
      }
      set
      {
        if (value != null)
          value = value.Master ?? value;
        this.m_FirstAttacker = value;
        this.MarkUpdate((UpdateFieldId) UnitFields.DYNAMIC_FLAGS);
      }
    }

    /// <summary>
    /// The Unit that last killed this guy or null, if none or gone (is not reliable over time).
    /// </summary>
    public Unit LastKiller
    {
      get
      {
        if (this.m_LastKiller == null || !this.m_LastKiller.IsInWorld)
          this.m_LastKiller = (Unit) null;
        return this.m_LastKiller;
      }
      internal set
      {
        this.m_LastKiller = value;
      }
    }

    /// <summary>
    /// Whether this Unit is currently participating in PvP.
    /// That is if both participants are players and/or belong to players.
    /// </summary>
    public bool IsPvPing
    {
      get
      {
        if (this.m_FirstAttacker != null && this.IsPlayerOwned)
          return this.m_FirstAttacker.IsPlayerOwned;
        return false;
      }
    }

    public IBrain Brain
    {
      get
      {
        return this.m_brain;
      }
      set
      {
        this.m_brain = value;
      }
    }

    /// <summary>Whether this is a Spirit Guide/Spirit Healer.</summary>
    public bool IsSpiritHealer
    {
      get
      {
        return this.NPCFlags.HasFlag((Enum) NPCFlags.SpiritHealer);
      }
    }

    /// <summary>
    /// A collection of all Auras (talents/buffs/debuffs) of this Unit
    /// </summary>
    public AuraCollection Auras
    {
      get
      {
        return this.m_auras;
      }
    }

    public ulong AuraUpdateMask
    {
      get
      {
        return this.m_auraUpdateMask;
      }
      set
      {
        this.m_auraUpdateMask = value;
      }
    }

    /// <summary>Gets the chat tag for the character.</summary>
    public virtual ChatTag ChatTag
    {
      get
      {
        return ChatTag.None;
      }
    }

    public int LastMoveTime
    {
      get
      {
        return this.m_lastMoveTime;
      }
    }

    /// <summary>Amount of current combo points with last combo target</summary>
    public int ComboPoints
    {
      get
      {
        return this.m_comboPoints;
      }
    }

    /// <summary>Current holder of combo-points for this chr</summary>
    public Unit ComboTarget
    {
      get
      {
        return this.m_comboTarget;
      }
    }

    public void ResetComboPoints()
    {
      if (this.m_comboTarget == null)
        return;
      this.ModComboState((Unit) null, 0);
    }

    /// <summary>Change combo target and/or amount of combo points</summary>
    /// <returns>If there is a change</returns>
    public virtual bool ModComboState(Unit target, int amount)
    {
      if (amount == 0 && target == this.m_comboTarget)
        return false;
      if (target == null)
      {
        this.m_comboPoints = 0;
      }
      else
      {
        if (target == this.m_comboTarget)
          this.m_comboPoints += amount;
        else
          this.m_comboPoints = amount;
        this.m_comboPoints = MathUtil.ClampMinMax(this.m_comboPoints, 0, 5);
      }
      this.m_comboTarget = target;
      return true;
    }

    /// <summary>Returns one of the arbitrary modifier values</summary>
    public int GetIntMod(StatModifierInt stat)
    {
      if (this.IntMods != null)
        return this.IntMods[(int) stat];
      return 0;
    }

    public virtual bool IsAlive
    {
      get
      {
        if (this.IsInWorld)
          return this.Health > 0;
        return false;
      }
    }

    public bool IsDead
    {
      get
      {
        return !this.IsAlive;
      }
    }

    /// <summary>Whether this is a ghost</summary>
    public bool IsGhost
    {
      get
      {
        return this.m_auras.GhostAura != null;
      }
    }

    /// <summary>
    /// This is used to prevent this Unit from dying during a
    /// critical process, such as damage application.
    /// If health is at 0 this Unit won't "<see cref="M:WCell.RealmServer.Entities.Unit.Die(System.Boolean)" />" until
    /// DeathPrevention is set to 0 again. This prevents certain problems from happening.
    /// </summary>
    protected internal uint DeathPrevention
    {
      get
      {
        return this.m_DeathPrevention;
      }
      set
      {
        if ((int) this.m_DeathPrevention == (int) value)
          return;
        this.m_DeathPrevention = value;
        if (value != 0U || this.Health != 0)
          return;
        this.Die(true);
      }
    }

    /// <summary>
    /// Different from <see cref="M:WCell.RealmServer.Entities.Unit.Kill" /> which actively kills the Unit.
    /// Is called when this Unit dies, i.e. Health gets smaller than 1.
    /// </summary>
    protected void Die(bool force)
    {
      if (force || !this.IsAlive || !this.OnBeforeDeath())
        return;
      this.SetUInt32((UpdateFieldId) UnitFields.HEALTH, 0U);
      Character chr = this as Character;
      if (chr != null)
        Asda2CharacterHandler.SendHealthUpdate(chr, false, false);
      this.MarkUpdate((UpdateFieldId) UnitFields.DYNAMIC_FLAGS);
      this.Dismount();
      SpellCast spellCast = this.m_spellCast;
      if (spellCast != null && spellCast.Spell != null)
        this.m_spellCast.Cancel(SpellFailedReason.Ok);
      this.m_auras.RemoveWhere((Predicate<Aura>) (aura => !aura.Spell.PersistsThroughDeath));
      this.UpdatePowerRegen();
      this.Power = 0;
      this.IsInCombat = false;
      this.CancelTaxiFlight();
      if (this.m_brain != null)
        this.m_brain.OnDeath();
      this.OnDeath();
      this.Target = (Unit) null;
      if (chr == null || this.Map == null)
        return;
      this.Map.CallDelayed(this.LastDamageDelay, (Action) (() => Asda2CharacterHandler.SendSelfDeathResponse(chr)));
    }

    protected abstract bool OnBeforeDeath();

    protected abstract void OnDeath();

    /// <summary>Resurrects this Unit if dead</summary>
    public void Resurrect()
    {
      if (this.IsAlive)
        return;
      this.Health = this.MaxHealth / 2;
      if (this.PowerType == PowerType.Mana)
        this.Power = this.MaxPower / 2;
      else if (this.PowerType == PowerType.Rage)
        this.Power = 0;
      else if (this.PowerType == PowerType.Energy)
        this.Power = this.MaxPower;
      Character chr = this as Character;
      if (chr == null)
        return;
      Asda2CharacterHandler.SendResurectResponse(chr);
    }

    /// <summary>Called automatically when Unit re-gains Health.</summary>
    protected internal virtual void OnResurrect()
    {
      Character chr = this as Character;
      if (chr != null)
        Asda2CharacterHandler.SendHealthUpdate(chr, false, false);
      this.MarkUpdate((UpdateFieldId) UnitFields.DYNAMIC_FLAGS);
    }

    /// <summary>whether this Unit is sitting on a ride</summary>
    public bool IsMounted
    {
      get
      {
        if (this.m_auras != null)
          return this.m_auras.MountAura != null;
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
      this.Dismount();
      this.SetUInt32((UpdateFieldId) UnitFields.MOUNTDISPLAYID, displayId);
      this.IncMechanicCount(SpellMechanic.Mounted, false);
    }

    /// <summary>Takes the mount off this Unit's butt (if mounted)</summary>
    public void Dismount()
    {
      if (!this.IsUnderInfluenceOf(SpellMechanic.Mounted))
        return;
      if (this.m_auras.MountAura != null)
        this.m_auras.MountAura.Remove(false);
      else
        this.DoDismount();
    }

    /// <summary>
    /// Is called internally.
    /// <see cref="M:WCell.RealmServer.Entities.Unit.Dismount" />
    /// </summary>
    protected internal virtual void DoDismount()
    {
      this.m_auras.MountAura = (Aura) null;
      this.SetUInt32((UpdateFieldId) UnitFields.MOUNTDISPLAYID, 0U);
      this.DecMechanicCount(SpellMechanic.Mounted, false);
    }

    /// <summary>whether the Unit is allowed to regenerate at all.</summary>
    public bool Regenerates
    {
      get
      {
        return this.m_regenerates;
      }
      set
      {
        if (value == this.m_regenerates)
          return;
        if (this.m_regenerates = value)
        {
          this.UpdatePowerRegen();
          this.UnitFlags2 |= UnitFlags2.RegeneratePower;
        }
        else
          this.UnitFlags2 ^= UnitFlags2.RegeneratePower;
      }
    }

    public virtual bool IsRegenerating
    {
      get
      {
        if (this.m_regenerates)
          return this.IsAlive;
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
        if (this.PowerType != PowerType.Mana || this.m_spellCast == null)
          return false;
        if ((long) (Environment.TickCount - this.m_spellCast.StartTime) >= (long) RegenerationFormulas.PowerRegenInterruptedCooldown)
          return this.m_spellCast.IsChanneling;
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
      get
      {
        return this.m_PowerRegenPerTick;
      }
      internal set
      {
        if (this.m_PowerRegenPerTick == value)
          return;
        this.m_PowerRegenPerTick = value;
        this.SetFloat((UpdateFieldId) ((UnitFields) (40 + this.PowerType)), (float) value);
      }
    }

    public float PowerRegenPerMillis
    {
      get
      {
        return (float) this.m_PowerRegenPerTick / (float) RegenerationFormulas.RegenTickDelayMillis;
      }
    }

    /// <summary>
    /// The amount of power to be generated during combat per regen tick (while being "interrupted")
    /// Only used for PowerType.Mana units
    /// </summary>
    public int ManaRegenPerTickInterrupted
    {
      get
      {
        return this._manaRegenPerTickInterrupted;
      }
      internal set
      {
        if (this._manaRegenPerTickInterrupted == value)
          return;
        this._manaRegenPerTickInterrupted = value;
        this.SetFloat((UpdateFieldId) ((UnitFields) (47 + this.PowerType)), (float) value);
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
      if (!this.IsRegenerating)
        return;
      int health = this.Health;
      this._tempHealthRegen += (!this.IsSitting ? (this.m_isInCombat ? (float) this.HealthRegenPerTickCombat : (float) this.HealthRegenPerTickNoCombat) : (float) this.MaxHealth * 0.009f) * (float) dt / (float) RegenerationFormulas.RegenTickDelayMillis;
      this._timeFromLastHealthUpdate += dt;
      if (this._timeFromLastHealthUpdate > 5000)
      {
        this._timeFromLastHealthUpdate -= 5000;
        if ((double) this._tempHealthRegen > 1.0)
        {
          int tempHealthRegen = (int) this._tempHealthRegen;
          this._tempHealthRegen -= (float) tempHealthRegen;
          if (this.Health < this.MaxHealth)
          {
            this.Health = health + tempHealthRegen;
            Character chr = this as Character;
            if (chr != null)
              Asda2CharacterHandler.SendHealthUpdate(chr, false, false);
          }
        }
      }
      this.UpdatePower(dt);
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
      int powerCostModifier = this.PowerCostModifier;
      if (this.m_schoolPowerCostMods != null)
        powerCostModifier += this.m_schoolPowerCostMods[(int) school];
      cost += powerCostModifier;
      cost = (int) (Math.Round((double) this.PowerCostMultiplier) * (double) cost);
      return cost;
    }

    /// <summary>
    /// Modifies the power-cost for the given DamageSchool by value
    /// </summary>
    public void ModPowerCost(DamageSchool type, int value)
    {
      if (this.m_schoolPowerCostMods == null)
        this.m_schoolPowerCostMods = new int[Unit.DamageSchoolCount];
      this.m_schoolPowerCostMods[(int) type] += value;
    }

    /// <summary>
    /// Modifies the power-cost for all of the given DamageSchools by value
    /// </summary>
    public void ModPowerCost(uint[] schools, int value)
    {
      if (this.m_schoolPowerCostMods == null)
        this.m_schoolPowerCostMods = new int[Unit.DamageSchoolCount];
      foreach (uint school in schools)
        this.m_schoolPowerCostMods[school] += value;
    }

    /// <summary>
    /// Modifies the power-cost for the given DamageSchool by value
    /// </summary>
    public void ModPowerCostPct(DamageSchool type, int value)
    {
      if (this.m_schoolPowerCostMods == null)
        this.m_schoolPowerCostMods = new int[Unit.DamageSchoolCount];
      this.m_schoolPowerCostMods[(int) type] += value;
    }

    /// <summary>
    /// Modifies the power-cost for all of the given DamageSchools by value
    /// </summary>
    public void ModPowerCostPct(uint[] schools, int value)
    {
      if (this.m_schoolPowerCostMods == null)
        this.m_schoolPowerCostMods = new int[Unit.DamageSchoolCount];
      foreach (uint school in schools)
        this.m_schoolPowerCostMods[school] += value;
    }

    /// <summary>
    /// Tries to consume the given amount of Power, also considers modifiers to Power-cost.
    /// </summary>
    public bool ConsumePower(DamageSchool type, Spell spell, int neededPower)
    {
      neededPower = this.GetPowerCost(type, spell, neededPower);
      if (this.Power < neededPower)
        return false;
      this.Power -= neededPower;
      return true;
    }

    public int GetHealthPercent(int value)
    {
      return (value * this.MaxHealth + 50) / 100;
    }

    /// <summary>
    /// Heals this unit and sends the corresponding animation (healer might be null)
    /// </summary>
    /// <param name="effect">The effect of the spell that triggered the healing (or null)</param>
    /// <param name="healer">The object that heals this Unit (or null)</param>
    /// <param name="value">The amount to be healed</param>
    public void HealPercent(int value, Unit healer = null, SpellEffect effect = null)
    {
      this.Heal((value * this.MaxHealth + 50) / 100, healer, effect);
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
      if (effect != null)
      {
        int num2 = value;
        if (healer != null)
          value = !effect.IsPeriodic ? healer.AddHealingModsToAction(value, effect, effect.Spell.Schools[0]) : healer.Auras.GetModifiedInt(SpellModifierType.PeriodicEffectValue, effect.Spell, value);
        if (chr != null)
          value += (int) ((double) num2 * (double) chr.HealingTakenModPct / 100.0);
        float num3 = this.GetCritChance(effect.Spell.Schools[0]) * 100f;
        if (!effect.Spell.AttributesExB.HasFlag((Enum) SpellAttributesExB.CannotCrit) && (double) num3 != 0.0 && (double) Utility.Random(1f, 10001f) <= (double) num3)
        {
          value = (int) ((double) value * ((double) SpellHandler.SpellCritBaseFactor + (double) this.GetIntMod(StatModifierInt.CriticalHealValuePct)));
          flag = true;
        }
      }
      if (value > 0)
      {
        value = (int) ((double) value * (double) Utility.Random(0.95f, 1.05f));
        if (chr != null)
          value += (int) ((double) value * (double) chr.HealingTakenModPct / 100.0);
        if (this.Health + value > this.MaxHealth)
        {
          num1 = this.Health + value - this.MaxHealth;
          value = this.MaxHealth - this.Health;
        }
        this.Health += value;
        value += num1;
        if (chr != null)
          Asda2CharacterHandler.SendHealthUpdate(chr, true, false);
      }
      if (healer == null)
        return;
      HealAction action = new HealAction();
      action.Attacker = healer;
      action.Victim = this;
      action.Spell = effect?.Spell;
      action.IsCritical = flag;
      action.IsHot = effect.IsPeriodic;
      action.Value = value;
      this.OnHeal(action);
    }

    /// <summary>
    /// This method is called whenever a heal is placed on a Unit by another Unit
    /// </summary>
    /// <param name="healer">The healer</param>
    /// <param name="value">The amount of points healed</param>
    protected virtual void OnHeal(HealAction action)
    {
      if (action.Value > 0)
        this.TriggerProcOnHeal(action);
      this.IterateEnvironment(15f, (Func<WorldObject, bool>) (obj =>
      {
        if (obj is Unit && ((Unit) obj).m_brain != null)
          ((Unit) obj).m_brain.OnHeal(action.Attacker, this, action.Value);
        return true;
      }));
    }

    private void TriggerProcOnHeal(HealAction action)
    {
      if (!action.IsHot)
        return;
      ProcHitFlags hitFlags = action.IsCritical ? ProcHitFlags.CriticalHit : ProcHitFlags.NormalHit;
      action.Attacker.Proc(ProcTriggerFlags.DonePeriodicDamageOrHeal, this, (IUnitAction) action, true, hitFlags);
      this.Proc(ProcTriggerFlags.ReceivedPeriodicDamageOrHeal, action.Attacker, (IUnitAction) action, true, hitFlags);
    }

    /// <summary>
    /// Leeches the given amount of health from this Unit and adds it to the receiver (if receiver != null and is Unit).
    /// </summary>
    /// <param name="factor">The factor applied to the amount that was leeched before adding it to the receiver</param>
    public void LeechHealth(Unit receiver, int amount, float factor, SpellEffect effect)
    {
      int health = this.Health;
      this.DealSpellDamage(receiver != null ? receiver.Master : this, effect, amount, true, true, false, true);
      amount = health - this.Health;
      if ((double) factor > 0.0)
        amount = (int) ((double) amount * (double) factor);
      if (receiver == null)
        return;
      receiver.Heal(amount, this, effect);
    }

    /// <summary>Restores Power and sends the corresponding Packet</summary>
    public void EnergizePercent(int value, Unit energizer = null, SpellEffect effect = null)
    {
      this.Energize((value * this.MaxPower + 50) / 100, energizer, effect);
    }

    /// <summary>Restores Power and sends the corresponding Packet</summary>
    public void Energize(int value, Unit energizer = null, SpellEffect effect = null)
    {
      if (value == 0)
        return;
      int power = this.Power;
      value = MathUtil.ClampMinMax(value, -power, this.MaxPower - value);
      CombatLogHandler.SendEnergizeLog((WorldObject) energizer, this, effect != null ? effect.Spell.Id : 0U, this.PowerType, value);
      this.Power = power + value;
    }

    /// <summary>
    /// Leeches the given amount of power from this Unit and adds it to the receiver (if receiver != null and is Unit).
    /// </summary>
    public void LeechPower(int amount, float factor = 1f, Unit receiver = null, SpellEffect effect = null)
    {
      int power = this.Power;
      amount -= MathUtil.RoundInt((float) ((double) amount * (double) this.GetResiliencePct() * 2.20000004768372));
      if (amount > power)
        amount = power;
      this.Power = power - amount;
      if (receiver == null)
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
      int power = this.Power;
      amount -= MathUtil.RoundInt((float) ((double) amount * (double) this.GetResiliencePct() * 2.20000004768372));
      if (amount > power)
        amount = power;
      this.Power = power - amount;
      this.DealSpellDamage(attacker, effect, (int) ((double) amount * (double) dmgFactor), true, true, false, true);
    }

    protected internal override void OnEnterMap()
    {
      this.m_lastMoveTime = Environment.TickCount;
      if (this.Flying <= 0U)
        return;
      MovementHandler.SendFlyModeStart(this);
    }

    /// <summary>Is called whenever a Unit moves</summary>
    public virtual void OnMove()
    {
      this.IsSitting = false;
      SpellCast spellCast = this.m_spellCast;
      if (this.m_auras == null)
        return;
      this.m_auras.RemoveByFlag(AuraInterruptFlags.OnMovement);
      this.m_lastMoveTime = Environment.TickCount;
    }

    /// <summary>whether this Unit is currently moving</summary>
    public virtual bool IsMoving
    {
      get
      {
        return (long) (Environment.TickCount - this.m_lastMoveTime) < (long) Unit.MinStandStillDelay;
      }
    }

    /// <summary>
    /// Makes this Unit move their face towards the given object
    /// </summary>
    public void Face(WorldObject obj)
    {
      if (obj != this.m_target || this.IsPlayerControlled)
        this.Face(this.m_orientation);
      else
        this.m_orientation = this.GetAngleTowards((IHasPosition) obj);
    }

    /// <summary>Makes this Unit look at the given location</summary>
    public void Face(Vector3 pos)
    {
      this.Face(this.GetAngleTowards(pos));
    }

    /// <summary>
    /// Makes this Unit move their face towards the given orientation
    /// </summary>
    public void Face(float orientation)
    {
      this.m_orientation = orientation;
      MovementHandler.SendFacingPacket(this, orientation, (uint) (314.0 / (double) this.TurnSpeed));
    }

    /// <summary>
    /// Checks whether this Unit can currently see the given obj
    /// 
    /// TODO: Higher staff ranks can always see lower staff ranks (too bad there are no ranks)
    /// TODO: Line of Sight
    /// </summary>
    public override bool CanSee(WorldObject obj)
    {
            if (!base.CanSee(obj) || !obj.IsInWorld)
            {
                return false;
            }
            if (this == obj)
            {
                return true;
            }
            switch (obj.DetermineVisibilityFor(this))
            {
                case VisibilityStatus.Invisible:
                    return false;

                case VisibilityStatus.Visible:
                    return true;
            }
            if (((this is Character) && ((Character)this).Role.IsStaff) && (!(obj is Character) || (((Character)obj).Role < ((Character)this).Role)))
            {
                if ((obj is Unit) && ((Unit)obj).IsSpiritHealer)
                {
                    return !this.IsAlive;
                }
                return true;
            }
            if (!(obj is Unit))
            {
                return true;
            }
            Unit unit = (Unit)obj;
            if (this.IsGhost)
            {
                if (this is Character)
                {
                    Corpse pos = ((Character)this).Corpse;
                    if (pos != null)
                    {
                        return unit.IsInRadiusSq(pos, Corpse.GhostVisibilityRadiusSq);
                    }
                }
                return false;
            }
            if (obj is Character)
            {
                Character character = (Character)obj;
                if ((character.Role.IsStaff && (character.Stealthed > 0)) && (!(this is Character) || (((Character)this).Role < character.Role)))
                {
                    return false;
                }
                if (((this is Character) && (character.GroupMember != null)) && (((Character)this).Group == character.Group))
                {
                    return true;
                }
            }
            if (!unit.IsSpiritHealer && !unit.IsGhost)
            {
                return this.HandleStealthDetection(unit);
            }
            return this.IsGhost;

        }

        public bool HandleStealthDetection(Unit unit)
    {
      if (unit.Stealthed <= 0)
        return true;
      if ((this.UnitFlags & UnitFlags.Stunned) != UnitFlags.None)
        return false;
      if ((double) this.GetDistance(unit.Position) <= 0.239999994635582)
        return true;
      if (!unit.IsInFrontOf((WorldObject) this))
        return false;
      bool flag = false;
      if (this.Auras.GetTotalAuraModifier(AuraType.Aura_228) > 0)
        flag = true;
      if (flag)
        return true;
      float num1 = (float) (10.5 - (double) unit.Stealthed / 100.0) + (float) (this.Level - unit.Level);
      int num2 = this.Auras.GetTotalAuraModifier(AuraType.ModStealthLevel);
      if (num2 < 0)
        num2 = 0;
      int totalAuraModifier = this.Auras.GetTotalAuraModifier(AuraType.ModDetect, 0);
      float num3 = num1 - (float) (totalAuraModifier - num2) / 5f;
      float num4 = (double) num3 > 45.0 ? 45f : num3;
      return (double) this.GetDistance(unit.Position) <= (double) num4;
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
      if (this.m_spellCast != null)
        this.m_spellCast.Cancel(SpellFailedReason.Interrupted);
      this.Target = (Unit) null;
    }

    public virtual void CancelSpellCast()
    {
      if (this.m_spellCast == null)
        return;
      this.m_spellCast.Cancel(SpellFailedReason.Interrupted);
    }

    public virtual void CancelEmote()
    {
      this.EmoteState = EmoteType.None;
    }

    /// <summary>Makes this Unit show an animation</summary>
    public void Emote(EmoteType emote)
    {
      EmoteHandler.SendEmote((WorldObject) this, emote);
    }

    /// <summary>Makes this Unit do a text emote</summary>
    /// <param name="emote">Anything that has a name (to do something with) or null</param>
    public void TextEmote(TextEmote emote, INamed target)
    {
      if (this is Character)
        ((Character) this).Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DoEmote, (uint) emote, 0U, (Unit) null);
      EmoteHandler.SendTextEmote((WorldObject) this, emote, target);
    }

    /// <summary>
    /// When pinned down, a Character cannot be
    /// logged out, moved or harmed.
    /// </summary>
    public bool IsPinnedDown
    {
      get
      {
        return this.m_IsPinnedDown;
      }
      set
      {
        if (!this.IsInWorld)
        {
          LogUtil.ErrorException((Exception) new InvalidOperationException("Character was already disposed when pinning down: " + (object) this), true);
        }
        else
        {
          this.m_Map.EnsureContext();
          if (this.m_IsPinnedDown == value)
            return;
          this.m_IsPinnedDown = value;
          if (this.m_IsPinnedDown)
          {
            this.IsEvading = true;
            ++this.Stunned;
          }
          else if (this is Character && ((Character) this).Client.IsOffline)
          {
            ((Character) this).Logout(true, 0);
          }
          else
          {
            this.IsEvading = false;
            --this.Stunned;
          }
        }
      }
    }

    public bool IsStunned
    {
      get
      {
        return this.UnitFlags.HasFlag((Enum) UnitFlags.Stunned);
      }
    }

    internal void OnTaxiStart()
    {
      this.UnitFlags |= UnitFlags.Influenced;
      this.IsOnTaxi = true;
      this.taxiTime = 0;
      this.m_TaxiMovementTimer = new TimerEntry(0, TaxiMgr.InterpolationDelayMillis, new Action<int>(this.TaxiTimerCallback));
      this.m_TaxiMovementTimer.Start();
      this.IsEvading = true;
    }

    internal void OnTaxiStop()
    {
      this.TaxiPaths.Clear();
      this.LatestTaxiPathNode = (LinkedListNode<PathVertex>) null;
      this.DoDismount();
      this.IsOnTaxi = false;
      this.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
      this.m_TaxiMovementTimer.Stop();
      this.IsEvading = false;
    }

    /// <summary>Time spent on the current taxi-ride in millis.</summary>
    public int TaxiTime
    {
      get
      {
        return this.taxiTime;
      }
    }

    protected virtual void TaxiTimerCallback(int elapsedTime)
    {
      TaxiMgr.InterpolatePosition(this, elapsedTime);
    }

    /// <summary>Returns the players currently planned taxi paths.</summary>
    public Queue<TaxiPath> TaxiPaths
    {
      get
      {
        return this.m_TaxiPaths;
      }
    }

    /// <summary>
    /// The point on the currently travelled TaxiPath that the Unit past most recently, or null if not on a taxi.
    /// </summary>
    public LinkedListNode<PathVertex> LatestTaxiPathNode
    {
      get
      {
        return this.m_LatestTaxiPathNode;
      }
      internal set
      {
        this.m_LatestTaxiPathNode = value;
      }
    }

    /// <summary>
    /// Whether or not this unit is currently flying on a taxi.
    /// </summary>
    public bool IsOnTaxi
    {
      get
      {
        return this.UnitFlags.HasFlag((Enum) UnitFlags.TaxiFlight);
      }
      set
      {
        if (value == this.IsOnTaxi)
          return;
        if (value)
          this.UnitFlags |= UnitFlags.TaxiFlight;
        else
          this.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
      }
    }

    /// <summary>
    /// Whether or not this Unit is currently under the influence of an effect that won't allow it to be controled by itself or its master
    /// </summary>
    public bool IsInfluenced
    {
      get
      {
        return this.UnitFlags.HasFlag((Enum) UnitFlags.Influenced);
      }
      set
      {
        if (value)
          this.UnitFlags |= UnitFlags.Influenced;
        else
          this.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
      }
    }

    public void CancelTaxiFlight()
    {
      if (!this.IsOnTaxi)
        return;
      MovementHandler.SendStopMovementPacket(this);
      this.OnTaxiStop();
    }

    /// <summary>Cancel any enforced movement</summary>
    public void CancelMovement()
    {
      this.CancelTaxiFlight();
      if (this.m_Movement == null)
        return;
      this.m_Movement.Stop();
    }

    public bool HasSpells
    {
      get
      {
        return this.m_spells != null;
      }
    }

    /// <summary>
    /// All spells known to this unit.
    /// Could be null for NPCs that are not spell-casters (check with <see cref="P:WCell.RealmServer.Entities.Unit.HasSpells" />).
    /// Use <see cref="P:WCell.RealmServer.Entities.NPC.NPCSpells" /> to enforce a SpellCollection.
    /// </summary>
    public virtual SpellCollection Spells
    {
      get
      {
        return this.m_spells;
      }
    }

    public bool HasEnoughPowerToCast(Spell spell, WorldObject selected)
    {
      if (!spell.CostsPower)
        return true;
      if (selected is Unit)
        return this.Power >= spell.CalcPowerCost(this, ((Unit) selected).GetLeastResistantSchool(spell));
      return this.Power >= spell.CalcPowerCost(this, spell.Schools[0]);
    }

    public DamageSchool GetLeastResistantSchool(Spell spell)
    {
      if (spell.Schools.Length == 1)
        return spell.Schools[0];
      int num = int.MaxValue;
      DamageSchool damageSchool = DamageSchool.Physical;
      foreach (DamageSchool school in spell.Schools)
      {
        int resistance = this.GetResistance(school);
        if (resistance < num)
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
      return this.SpawnMinion(id, 0);
    }

    /// <summary>Tries to spawn the given pet for this Unit.</summary>
    /// <returns>null, if the Character already has that kind of Pet.</returns>
    public NPC SpawnMinion(NPCId id, int durationMillis)
    {
      NPCEntry entry = NPCMgr.GetEntry(id);
      if (entry != null)
        return this.SpawnMinion(entry, ref this.m_position, durationMillis);
      return (NPC) null;
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
      minion.Phase = this.Phase;
      minion.Zone = this.Zone;
      minion.RemainingDecayDelayMillis = durationMillis;
      minion.Brain.IsRunning = true;
      if (this.Health > 0)
        this.Enslave(minion, durationMillis);
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
      NPC minion = this.CreateMinion(entry, durationMillis);
      minion.Position = position;
      this.m_Map.AddObjectLater((WorldObject) minion);
      return minion;
    }

    public void Enslave(NPC minion)
    {
      this.Enslave(minion, 0);
    }

    public void Enslave(NPC minion, int durationMillis)
    {
            minion.Phase = this.Phase;
            minion.Master = this;
            switch (minion.Entry.Type)
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
            if (durationMillis != 0)
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
      get
      {
        return this.m_procHandlers;
      }
    }

    public IProcHandler GetProcHandler(Func<IProcHandler, bool> predicate)
    {
      if (this.m_procHandlers != null)
      {
        foreach (IProcHandler procHandler in this.m_procHandlers)
        {
          if (predicate(procHandler))
            return procHandler;
        }
      }
      return (IProcHandler) null;
    }

    /// <summary>Returns the first proc handler of the given type</summary>
    public T GetProcHandler<T>() where T : IProcHandler
    {
      if (this.m_procHandlers != null)
      {
        foreach (IProcHandler procHandler in this.m_procHandlers)
        {
          if (procHandler is T)
            return (T) procHandler;
        }
      }
      return default (T);
    }

    public void AddProcHandler(ProcHandlerTemplate templ)
    {
      this.AddProcHandler((IProcHandler) new ProcHandler(this, this, templ));
    }

    public void AddProcHandler(IProcHandler handler)
    {
      if (this.m_procHandlers == null)
        this.m_procHandlers = new List<IProcHandler>(5);
      this.m_procHandlers.Add(handler);
    }

    public void RemoveProcHandler(IProcHandler handler)
    {
      if (this.m_procHandlers == null)
        return;
      this.m_procHandlers.Remove(handler);
    }

    /// <summary>Remnoves the first proc that triggers the given spell</summary>
    public void RemoveProcHandler(SpellId procId)
    {
      if (this.m_procHandlers == null)
        return;
      foreach (IProcHandler procHandler in this.m_procHandlers)
      {
        if (procHandler.ProcSpell != null && procHandler.ProcSpell.SpellId == procId)
        {
          this.m_procHandlers.Remove(procHandler);
          break;
        }
      }
    }

    public void RemoveProcHandler(Func<IProcHandler, bool> predicate)
    {
      IProcHandler procHandler = this.GetProcHandler(predicate);
      if (procHandler == null)
        return;
      this.m_procHandlers.Remove(procHandler);
    }

    public void RemoveProcHandler<T>() where T : IProcHandler
    {
      T procHandler = this.GetProcHandler<T>();
      if ((object) procHandler == null)
        return;
      this.m_procHandlers.Remove((IProcHandler) procHandler);
    }

    /// <summary>
    /// Removes the first custom ProcHandler that uses the given template.
    /// </summary>
    public void RemoveProcHandler(ProcHandlerTemplate template)
    {
      if (this.m_procHandlers == null)
        return;
      foreach (IProcHandler procHandler in this.m_procHandlers)
      {
        if (procHandler is ProcHandler && ((ProcHandler) procHandler).Template == template)
        {
          this.m_procHandlers.Remove(procHandler);
          break;
        }
      }
    }

    /// <summary>
    /// Trigger all procs that can be triggered by the given action
    /// </summary>
    /// <param name="active">Whether the triggerer is the attacker/caster (true), or the victim (false)</param>
    public void Proc(ProcTriggerFlags flags, Unit triggerer, IUnitAction action, bool active, ProcHitFlags hitFlags = ProcHitFlags.None)
    {
      if (this.m_brain != null && this.m_brain.CurrentAction != null && this.m_brain.CurrentAction.InterruptFlags.HasAnyFlag(flags))
        this.m_brain.StopCurrentAction();
      if (this.m_procHandlers == null || flags == ProcTriggerFlags.None)
        return;
      if (triggerer == null)
      {
        Unit.log.Error("triggerer was null when triggering Proc by action: {0} (Flags: {1})", (object) action, (object) flags);
      }
      else
      {
        DateTime now = DateTime.Now;
        for (int index = this.m_procHandlers.Count - 1; index >= 0; --index)
        {
          if (index < this.m_procHandlers.Count)
          {
            IProcHandler procHandler = this.m_procHandlers[index];
            bool flag1 = procHandler.ProcTriggerFlags.HasAnyFlag(flags);
            bool flag2 = !flags.RequireHitFlags() || procHandler.ProcHitFlags.HasAnyFlag(hitFlags);
            if (procHandler.NextProcTime <= now && flag1 && (flag2 && procHandler.CanBeTriggeredBy(triggerer, action, active)))
            {
              int num = (int) procHandler.ProcChance;
              if (num > 0 && action.Spell != null)
                num = this.Auras.GetModifiedInt(SpellModifierType.ProcChance, action.Spell, num);
              if (procHandler.ProcChance <= 0U || Utility.Random(0, 101) <= num)
              {
                int stackCount = procHandler.StackCount;
                procHandler.TriggerProc(triggerer, action);
                if (procHandler.MinProcDelay > 0)
                  procHandler.NextProcTime = now.AddMilliseconds((double) procHandler.MinProcDelay);
                if (stackCount > 0 && procHandler.StackCount == 0)
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
      get
      {
        return this.m_gossipMenu;
      }
      set
      {
        this.m_gossipMenu = value;
        if (value != null)
          this.NPCFlags |= NPCFlags.Gossip;
        else
          this.NPCFlags &= ~NPCFlags.Gossip;
      }
    }

    /// <summary>
    /// Is called when Master / Faction has changed and this Unit now has a different circle of friends
    /// </summary>
    protected virtual void OnAffinityChanged()
    {
      if (!this.IsPlayerOwned)
        return;
      this.m_auras.RemoveOthersAuras();
    }

    public override void Dispose(bool disposing)
    {
      if (this.m_auras == null)
        return;
      if (this.m_Movement != null)
      {
        this.m_Movement.m_owner = (Unit) null;
        this.m_Movement = (WCell.RealmServer.Entities.Movement) null;
      }
      base.Dispose(disposing);
      this.m_attackTimer = (TimerEntry) null;
      this.m_target = (Unit) null;
      if (this.m_brain != null)
      {
        this.m_brain.Dispose();
        this.m_brain = (IBrain) null;
      }
      this.m_spells.Recycle();
      this.m_spells = (SpellCollection) null;
      this.m_auras.Owner = (Unit) null;
      this.m_auras = (AuraCollection) null;
      this.m_charm = (Unit) null;
      this.m_channeled = (WorldObject) null;
    }

    protected internal override void DeleteNow()
    {
      this.IsFighting = false;
      if (this.m_brain != null)
        this.m_brain.IsRunning = false;
      this.Target = (Unit) null;
      base.DeleteNow();
    }

    protected virtual HighId HighId
    {
      get
      {
        return HighId.Unit;
      }
    }

    protected void GenerateId(uint entryId)
    {
      this.EntityId = new EntityId(NPCMgr.GenerateUniqueLowId(), entryId, this.HighId);
    }

    /// <summary>Whether this Unit can aggro NPCs.</summary>
    public bool CanGenerateThreat
    {
      get
      {
        if (this.IsInWorld && this.IsAlive)
          return !this.IsEvading;
        return false;
      }
    }

    public abstract LinkedList<WaypointEntry> Waypoints { get; }

    public abstract NPCSpawnPoint SpawnPoint { get; }

    public bool CanBeAggroedBy(Unit target)
    {
      if (target.CanGenerateThreat && this.IsHostileWith((IFactionMember) target))
        return this.CanSee((WorldObject) target);
      return false;
    }

    /// <summary>
    /// Is called when a Unit successfully evaded (and arrived at its original location)
    /// </summary>
    internal void OnEvaded()
    {
      this.IsEvading = false;
      if (this.m_brain == null)
        return;
      this.m_brain.EnterDefaultState();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <remarks>Requires Brain</remarks>
    public void Follow(Unit target)
    {
      if (!this.CheckBrain())
        return;
      this.Target = target;
      this.m_brain.CurrentAction = (IAIAction) new AIFollowTargetAction(this);
    }

    /// <summary>
    /// Moves towards the given target and then executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Vector3 pos, UnitActionCallback actionCallback)
    {
      if (!this.CheckBrain())
        return;
      this.m_Movement.MoveTo(pos, true);
      this.m_brain.CurrentAction = (IAIAction) new AIMoveThenExecAction(this, actionCallback);
    }

    /// <summary>
    /// Moves towards the given target and then executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToPointsThenExecute(List<Vector3> points, UnitActionCallback actionCallback)
    {
      if (!this.CheckBrain())
        return;
      this.m_Movement.MoveToPoints(points);
      this.m_brain.CurrentAction = (IAIAction) new AIMoveThenExecAction(this, actionCallback);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, UnitActionCallback actionCallback)
    {
      this.MoveToThenExecute(unit, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, UnitActionCallback actionCallback, int millisTimeout)
    {
      if (!this.CheckBrain())
        return;
      this.Target = unit;
      this.m_brain.CurrentAction = (IAIAction) new AIMoveToThenExecAction(this, actionCallback);
    }

    /// <summary>
    /// Moves in front of the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveInFrontThenExecute(Unit unit, UnitActionCallback actionCallback)
    {
      this.MoveInFrontThenExecute(unit, actionCallback, 0);
    }

    /// <summary>
    /// Moves in front of the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveInFrontThenExecute(GameObject go, UnitActionCallback actionCallback)
    {
      this.MoveInFrontThenExecute(go, actionCallback, 0);
    }

    /// <summary>
    /// Moves in front of the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveInFrontThenExecute(Unit unit, UnitActionCallback actionCallback, int millisTimeout)
    {
      this.MoveToThenExecute(unit, 0.0f, actionCallback);
    }

    /// <summary>
    /// Moves in front of the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveInFrontThenExecute(GameObject go, UnitActionCallback actionCallback, int millisTimeout)
    {
      this.MoveToThenExecute(go, 0.0f, actionCallback);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveBehindThenExecute(Unit unit, UnitActionCallback actionCallback)
    {
      this.MoveBehindThenExecute(unit, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveBehindThenExecute(GameObject go, UnitActionCallback actionCallback)
    {
      this.MoveBehindThenExecute(go, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveBehindThenExecute(Unit unit, UnitActionCallback actionCallback, int millisTimeout)
    {
      this.MoveToThenExecute(unit, 3.141593f, actionCallback);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveBehindThenExecute(GameObject go, UnitActionCallback actionCallback, int millisTimeout)
    {
      this.MoveToThenExecute(go, 3.141593f, actionCallback);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, float angle, UnitActionCallback actionCallback)
    {
      this.MoveToThenExecute(unit, angle, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(GameObject go, float angle, UnitActionCallback actionCallback)
    {
      this.MoveToThenExecute(go, angle, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, float angle, UnitActionCallback callback, int millisTimeout)
    {
      if (!this.CheckBrain())
        return;
      this.Target = unit;
      AIMoveIntoAngleThenExecAction angleThenExecAction = new AIMoveIntoAngleThenExecAction(this, angle, callback);
      angleThenExecAction.TimeoutMillis = millisTimeout;
      this.m_brain.CurrentAction = (IAIAction) angleThenExecAction;
    }

    /// <summary>
    /// Moves to the given gameobject and once within default range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(GameObject go, float angle, UnitActionCallback callback, int millisTimeout)
    {
      if (!this.CheckBrain())
        return;
      AIMoveToGameObjectIntoAngleThenExecAction angleThenExecAction = new AIMoveToGameObjectIntoAngleThenExecAction(this, go, angle, callback);
      angleThenExecAction.TimeoutMillis = millisTimeout;
      this.m_brain.CurrentAction = (IAIAction) angleThenExecAction;
    }

    /// <summary>
    /// Moves to the given target and once within the given range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, SimpleRange range, UnitActionCallback actionCallback)
    {
      this.MoveToThenExecute(unit, range, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within the given range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(GameObject go, SimpleRange range, UnitActionCallback actionCallback)
    {
      this.MoveToThenExecute(go, range, actionCallback, 0);
    }

    /// <summary>
    /// Moves to the given target and once within the given range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(Unit unit, SimpleRange range, UnitActionCallback actionCallback, int millisTimeout)
    {
      if (!this.CheckBrain())
        return;
      this.Target = unit;
      IBrain brain = this.m_brain;
      AIMoveIntoRangeThenExecAction rangeThenExecAction1 = new AIMoveIntoRangeThenExecAction(this, range, actionCallback);
      rangeThenExecAction1.TimeoutMillis = millisTimeout;
      AIMoveIntoRangeThenExecAction rangeThenExecAction2 = rangeThenExecAction1;
      brain.CurrentAction = (IAIAction) rangeThenExecAction2;
    }

    /// <summary>
    /// Moves to the given target and once within the given range, executes the given action
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenExecute(GameObject go, SimpleRange range, UnitActionCallback actionCallback, int millisTimeout)
    {
      if (!this.CheckBrain())
        return;
      IBrain brain = this.m_brain;
      AIMoveIntoRangeOfGOThenExecAction goThenExecAction1 = new AIMoveIntoRangeOfGOThenExecAction(this, go, range, actionCallback);
      goThenExecAction1.TimeoutMillis = millisTimeout;
      AIMoveIntoRangeOfGOThenExecAction goThenExecAction2 = goThenExecAction1;
      brain.CurrentAction = (IAIAction) goThenExecAction2;
    }

    /// <summary>
    /// Moves this Unit to the given position and then goes Idle
    /// </summary>
    /// <param name="pos"></param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenIdle(ref Vector3 pos)
    {
      this.MoveToThenEnter(ref pos, BrainState.Idle);
    }

    /// <summary>
    /// Moves this Unit to the given position and then goes Idle
    /// </summary>
    /// <param name="pos"></param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenIdle(IHasPosition pos)
    {
      this.MoveToThenEnter(pos, BrainState.Idle);
    }

    /// <summary>
    /// Moves this Unit to the given position and then assumes arrivedState
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="arrivedState">The BrainState to enter once arrived</param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenEnter(ref Vector3 pos, BrainState arrivedState)
    {
      this.MoveToThenEnter(ref pos, true, arrivedState);
    }

    /// <summary>
    /// Moves this Unit to the given position and then assumes arrivedState
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="arrivedState">The BrainState to enter once arrived</param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenEnter(ref Vector3 pos, bool findPath, BrainState arrivedState)
    {
      if (!this.CheckBrain())
        return;
      this.m_Movement.MoveTo(pos, findPath);
      this.m_brain.CurrentAction = (IAIAction) new AIMoveThenEnterAction(this, arrivedState);
    }

    /// <summary>
    /// Moves this Unit to the given position and then assumes arrivedState
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="arrivedState">The BrainState to enter once arrived</param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenEnter(IHasPosition pos, BrainState arrivedState)
    {
      this.MoveToThenEnter(pos, true, arrivedState);
    }

    /// <summary>
    /// Moves this Unit to the given position and then assumes arrivedState
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="arrivedState">The BrainState to enter once arrived</param>
    /// <remarks>Requires Brain</remarks>
    public void MoveToThenEnter(IHasPosition pos, bool findPath, BrainState arrivedState)
    {
      if (!this.CheckBrain())
        return;
      this.m_Movement.MoveTo(pos.Position, findPath);
      this.m_brain.CurrentAction = (IAIAction) new AIMoveThenEnterAction(this, arrivedState);
    }

    /// <summary>
    /// Idles for the given time and then executes the given action.
    /// Also will unset the current Target and stop fighting.
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void IdleThenExecute(int millis, Action action)
    {
      this.IdleThenExecute(millis, action, ProcTriggerFlags.None);
    }

    /// <summary>
    /// Idles for the given time and then executes the given action.
    /// Also will unset the current Target and stop fighting.
    /// </summary>
    /// <param name="interruptFlags">What can interrupt the action.</param>
    /// <remarks>Requires Brain</remarks>
    public void IdleThenExecute(int millis, Action action, ProcTriggerFlags interruptFlags)
    {
      if (!this.CheckBrain())
        return;
      this.Target = (Unit) null;
      this.m_brain.CurrentAction = (IAIAction) new AITemporaryIdleAction(millis, interruptFlags, (Action) (() =>
      {
        this.m_brain.StopCurrentAction();
        action();
      }));
    }

    /// <summary>
    /// Idles for the given time before resuming its normal activities
    /// Also will unset the current Target and stop fighting.
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void Idle(int millis)
    {
      this.Idle(millis, ProcTriggerFlags.None);
    }

    /// <summary>
    /// Idles until the given flags have been triggered.
    /// Also will unset the current Target and stop fighting.
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void Idle(ProcTriggerFlags interruptFlags)
    {
      this.Idle(int.MaxValue, interruptFlags);
    }

    /// <summary>
    /// Idles for the given time before resuming its normal activities
    /// Also will unset the current Target and stop fighting.
    /// </summary>
    /// <remarks>Requires Brain</remarks>
    public void Idle(int millis, ProcTriggerFlags interruptFlags)
    {
      if (!this.CheckBrain())
        return;
      this.Target = (Unit) null;
      this.m_brain.CurrentAction = (IAIAction) new AITemporaryIdleAction(millis, interruptFlags, (Action) (() => this.m_brain.StopCurrentAction()));
    }

    protected bool CheckBrain()
    {
      if (this.m_brain != null)
        return true;
      this.Say("I do not have a Brain.");
      return false;
    }

    /// <summary>
    /// The last time when this Unit was still actively Fighting
    /// </summary>
    public DateTime LastCombatTime
    {
      get
      {
        return this.m_lastCombatTime;
      }
      set
      {
        this.m_lastCombatTime = value;
      }
    }

    public int MillisSinceLastCombatAction
    {
      get
      {
        return (DateTime.Now - this.m_lastCombatTime).ToMilliSecondsInt();
      }
    }

    /// <summary>
    /// While in combat, this method will reset the current swing delay (swing timer is reset)
    /// </summary>
    public void ResetSwingDelay()
    {
      this.m_lastCombatTime = this.m_lastUpdateTime;
    }

    public void CancelPendingAbility()
    {
      if (this.m_spellCast == null || !this.m_spellCast.IsPending)
        return;
      this.m_spellCast.Cancel(SpellFailedReason.DontReport);
    }

    /// <summary>
    /// The spell that is currently being triggered automatically by the CombatTimer
    /// </summary>
    public Spell AutorepeatSpell
    {
      get
      {
        return this.m_AutorepeatSpell;
      }
      set
      {
        if (this.m_AutorepeatSpell == value)
          return;
        this.m_AutorepeatSpell = value;
        if (value != null)
        {
          if (!value.IsRangedAbility)
            return;
          this.SheathType = SheathType.Ranged;
        }
        else
          this.SheathType = SheathType.Melee;
      }
    }

    /// <summary>
    /// Whether this Unit is currently attacking with a ranged weapon
    /// </summary>
    public bool IsUsingRangedWeapon
    {
      get
      {
        if (this.m_AutorepeatSpell != null)
          return this.m_AutorepeatSpell.IsRangedAbility;
        return false;
      }
    }

    /// <summary>Amount of extra attacks to hit on next thit</summary>
    public int ExtraAttacks
    {
      get
      {
        return this.m_extraAttacks;
      }
      set
      {
        this.m_extraAttacks = value;
      }
    }

    /// <summary>Adds damage mods to the given AttackAction</summary>
    public virtual void OnAttack(DamageAction action)
    {
      for (int index = this.AttackEventHandlers.Count - 1; index >= 0; --index)
        this.AttackEventHandlers[index].OnAttack(action);
    }

    /// <summary>Adds damage mods to the given AttackAction</summary>
    public virtual void OnDefend(DamageAction action)
    {
      this.IsSitting = false;
      for (int index = this.AttackEventHandlers.Count - 1; index >= 0; --index)
        this.AttackEventHandlers[index].OnDefend(action);
    }

    /// <summary>Adds damage mods to the given AttackAction</summary>
    public virtual int AddHealingModsToAction(int healValue, SpellEffect effect, DamageSchool school)
    {
      return healValue;
    }

    internal DamageAction GetUnusedAction()
    {
      if (this.m_DamageAction == null || this.m_DamageAction.ReferenceCount > 0)
        return new DamageAction(this);
      return this.m_DamageAction;
    }

    /// <summary>
    /// Whether this unit has an ability pending for the given weapon (Heroic Strike for melee, Poison Dart for throwing, Stun Shot for ranged weapons etc)
    /// </summary>
    public bool UsesPendingAbility(IAsda2Weapon weapon)
    {
      if (this.m_spellCast != null && this.m_spellCast.IsPending)
        return this.m_spellCast.GetWeapon() == weapon;
      return false;
    }

    /// <summary>Strike using mainhand weapon</summary>
    public void Strike()
    {
      this.Strike(this.MainWeapon);
    }

    /// <summary>Strike using given weapon</summary>
    public void Strike(IAsda2Weapon weapon)
    {
      DamageAction unusedAction = this.GetUnusedAction();
      Unit target = this.m_target;
      this.Strike(weapon, unusedAction, target);
    }

    /// <summary>Strike the target using mainhand weapon</summary>
    public void Strike(Unit target)
    {
      this.Strike(this.MainWeapon, target);
    }

    /// <summary>Strike the target using given weapon</summary>
    public void Strike(IAsda2Weapon weapon, Unit target)
    {
      this.Strike(weapon, this.GetUnusedAction(), target);
    }

    public void Strike(DamageAction action, Unit target)
    {
      this.Strike(this.MainWeapon, action, target);
    }

    /// <summary>
    /// Do a single attack using the given weapon and action on the target
    /// </summary>
    public void Strike(IAsda2Weapon weapon, DamageAction action, Unit target)
    {
      this.IsInCombat = true;
      if (this.UsesPendingAbility(weapon))
      {
        int num1 = (int) this.m_spellCast.Perform();
      }
      else
      {
        int num2 = (int) this.Strike(weapon, action, target, (SpellCast) null);
      }
    }

    /// <summary>
    /// Do a single attack on the target using given weapon and ability.
    /// </summary>
    public ProcHitFlags Strike(IAsda2Weapon weapon, Unit target, SpellCast ability)
    {
      return this.Strike(weapon, this.GetUnusedAction(), target, ability);
    }

    /// <summary>
    /// Do a single attack on the target using given weapon, ability and action.
    /// </summary>
    public ProcHitFlags Strike(IAsda2Weapon weapon, DamageAction action, Unit target, SpellCast ability)
    {
      ProcHitFlags procHitFlags = ProcHitFlags.None;
      this.EnsureContext();
      if (!this.IsAlive || !target.IsInContext || !target.IsAlive)
        return procHitFlags;
      if (weapon == null)
      {
        Unit.log.Info("Trying to strike without weapon: " + (object) this);
        return procHitFlags;
      }
      target.IsInCombat = true;
      action.Victim = target;
      action.Attacker = this;
      action.Weapon = weapon;
      if (ability != null)
      {
        action.Schools = ability.Spell.SchoolMask;
        action.SpellEffect = ability.Spell.Effects[0];
        this.GetWeaponDamage(action, weapon, ability, 0);
        procHitFlags = action.DoAttack();
        if (ability.Spell.AttributesExC.HasFlag((Enum) SpellAttributesExC.RequiresTwoWeapons) && this.m_offhandWeapon != null)
        {
          action.Reset(this, target, this.m_offhandWeapon);
          this.GetWeaponDamage(action, this.m_offhandWeapon, ability, 0);
          procHitFlags |= action.DoAttack();
          this.m_lastOffhandStrike = Environment.TickCount;
        }
      }
      else
      {
        ++this.m_extraAttacks;
        do
        {
          this.GetWeaponDamage(action, weapon, (SpellCast) null, 0);
          action.Schools = weapon.Damages.AllSchools();
          if (action.Schools == DamageSchoolMask.None)
            action.Schools = DamageSchoolMask.Physical;
          int num = (int) action.DoAttack();
        }
        while (--this.m_extraAttacks > 0);
      }
      action.OnFinished();
      return procHitFlags;
    }

    /// <summary>Returns random damage for the given weapon</summary>
    public void GetWeaponDamage(DamageAction action, IAsda2Weapon weapon, SpellCast usedAbility, int targetNo = 0)
    {
      int num1 = weapon != this.m_offhandWeapon ? Utility.Random((int) this.MinDamage, (int) this.MaxDamage) : Utility.Random((int) this.MinOffHandDamage, (int) this.MaxOffHandDamage + 1);
      if (this is NPC)
        num1 = (int) ((double) num1 * (double) NPCMgr.DefaultNPCDamageFactor + 0.999998986721039);
      if (usedAbility != null && usedAbility.IsCasting)
      {
        int num2 = 0;
        foreach (SpellEffectHandler handler in usedAbility.Handlers)
        {
          if (handler.Effect.IsStrikeEffectFlat)
            num1 += handler.CalcDamageValue(targetNo);
          else if (handler.Effect.IsStrikeEffectPct)
            num2 += handler.CalcDamageValue(targetNo);
        }
        action.Damage = num2 <= 0 ? num1 : (num1 * num2 + 50) / 100;
        foreach (SpellEffectHandler handler in usedAbility.Handlers)
        {
          if (handler is WeaponDamageEffectHandler)
            ((WeaponDamageEffectHandler) handler).OnHit(action);
        }
      }
      else
        action.Damage = num1;
    }

    /// <summary>Does spell-damage to this Unit</summary>
    public DamageAction DealSpellDamage(Unit attacker, SpellEffect effect, int dmg, bool addDamageBonuses = true, bool mayCrit = true, bool forceCrit = false, bool clearAction = true)
    {
      this.EnsureContext();
      if (!this.IsAlive)
        return (DamageAction) null;
      if (attacker != null && !attacker.IsInContext)
        attacker = (Unit) null;
      if (attacker is NPC)
        dmg = (int) ((double) dmg * (double) NPCMgr.DefaultNPCDamageFactor + 0.999998986721039);
      DamageSchool school = effect == null ? DamageSchool.Physical : this.GetLeastResistantSchool(effect.Spell);
      if (this.IsEvading || this.IsImmune(school) || (this.IsInvulnerable || !this.IsAlive))
        return (DamageAction) null;
      DamageAction unusedAction = this.GetUnusedAction();
      unusedAction.Attacker = attacker;
      unusedAction.HitFlags = HitFlags.NormalSwing;
      unusedAction.VictimState = VictimState.Miss;
      unusedAction.Weapon = (IAsda2Weapon) null;
      if (effect != null)
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
      float def = (float) (((double) this.Asda2MagicDefence + (double) this.Asda2Defence) / 2.0);
      Character attacker1 = unusedAction.Attacker as Character;
      if (attacker1 != null)
      {
        switch (attacker1.Archetype.ClassId)
        {
          case ClassId.OHS:
            def = this.Asda2Defence;
            break;
          case ClassId.Spear:
            def = this.Asda2Defence;
            break;
          case ClassId.THS:
            def = this.Asda2Defence;
            break;
          case ClassId.Crossbow:
            def = this.Asda2Defence;
            break;
          case ClassId.Bow:
            def = this.Asda2Defence;
            break;
          case ClassId.Balista:
            def = this.Asda2Defence;
            break;
          case ClassId.AtackMage:
            def = this.Asda2MagicDefence;
            break;
          case ClassId.SupportMage:
            def = this.Asda2MagicDefence;
            break;
          case ClassId.HealMage:
            def = this.Asda2MagicDefence;
            break;
        }
      }
      unusedAction.ResistPct = DamageAction.CalcResistPrc(def, unusedAction.Damage, this.GetResistChancePct(this, unusedAction.UsedSchool));
      unusedAction.Absorbed = 0;
      unusedAction.SpellEffect = effect;
      unusedAction.Victim = this;
      if (attacker != null)
        ++attacker.DeathPrevention;
      ++this.DeathPrevention;
      try
      {
        if (attacker != null)
        {
          int num1 = Utility.Random(1, 10000);
          int num2 = unusedAction.CalcHitChance();
          if (num1 > num2)
          {
            unusedAction.Miss();
          }
          else
          {
            int num3 = unusedAction.CalcBlockChance();
            if (num1 > num2 - num3)
            {
              unusedAction.Block();
            }
            else
            {
              int num4 = unusedAction.CalcCritChance();
              if (forceCrit || num1 > num2 - num4 - num3)
                unusedAction.StrikeCritical();
            }
            if (addDamageBonuses)
              unusedAction.AddDamageMods();
          }
          this.OnDefend(unusedAction);
          attacker.OnAttack(unusedAction);
        }
        unusedAction.Resisted = (int) Math.Round((double) unusedAction.Damage * (double) unusedAction.ResistPct / 100.0);
        this.DoRawDamage((IDamageAction) unusedAction);
      }
      finally
      {
        --this.DeathPrevention;
        if (attacker != null)
          --attacker.DeathPrevention;
        if (clearAction)
          unusedAction.OnFinished();
      }
      if (clearAction)
        return (DamageAction) null;
      return unusedAction;
    }

    public float GetBaseCritChance(DamageSchool dmgSchool, Spell spell, IAsda2Weapon weapon)
    {
      Character character = this as Character;
      if (character != null)
        return character.CritChanceMeleePct;
      NPC npc = this as NPC;
      if (npc != null)
      {
        switch (npc.Entry.Rank)
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
      int resistance = this.GetResistance(school);
      int num1;
      if (attacker != null)
      {
        num1 = Math.Max(1, attacker.Level);
        resistance -= attacker.GetTargetResistanceMod(school);
      }
      else
        num1 = 1;
      float num2 = (float) ((double) Math.Max(0, resistance) / ((double) num1 * 5.0) * 0.75);
      if ((double) num2 > 75.0)
        num2 = 75f;
      if ((double) num2 < 0.0)
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
      if (attacker is Character)
      {
        Character character = (Character) attacker;
        num = num - (float) character.Expertise * 0.25f + (float) character.IntMods[17];
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
      float parryChance = this.ParryChance;
      if (attacker is Character)
      {
        Character character = (Character) attacker;
        parryChance -= (float) character.Expertise * 0.25f;
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
      int num = 200 - (int) victim.GetResiliencePct() + (victim.GetIntMod(StatModifierInt.CritDamageBonusPct) + this.CritDamageBonusPrc);
      return (float) (((double) dmg * (double) num + 50.0) / 100.0);
    }

    /// <summary>
    /// whether this Unit resists a debuff (independent on resistances)
    /// </summary>
    public bool CheckDebuffResist(int attackerLevel, DamageSchool school)
    {
      return Utility.Random(0, 100) < this.GetDebuffResistance(school) - this.GetAttackerSpellHitChanceMod(school);
    }

    /// <summary>
    /// whether this Unit is currently in Combat.
    /// If it is actively fighting (rather than being forced into CombatMode),
    /// IsFighting must be true.
    /// </summary>
    public bool IsInCombat
    {
      get
      {
        return this.m_isInCombat;
      }
      set
      {
        if (this.m_isInCombat == value)
          return;
        if (this.m_isInCombat = value)
        {
          this.UnitFlags |= UnitFlags.Combat;
          this.StandState = StandState.Stand;
          this.m_auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);
          if (this.m_spellCast != null)
          {
            Spell spell = this.m_spellCast.Spell;
            if (spell != null && spell.RequiresCasterOutOfCombat)
              this.m_spellCast.Cancel(SpellFailedReason.Interrupted);
          }
          if (this.HasMaster)
            this.Master.IsInCombat = true;
          this.OnEnterCombat();
          this.m_attackTimer.Start(1);
        }
        else
        {
          if (this is NPC)
            this.IsFighting = false;
          this.CancelPendingAbility();
          this.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          this.m_attackTimer.Stop();
          this.OnLeaveCombat();
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
      get
      {
        return this.m_isFighting;
      }
      set
      {
        if (this.m_isFighting == value)
          return;
        if (this.m_isFighting = value)
        {
          if (this.m_target == null)
          {
            this.m_isFighting = false;
          }
          else
          {
            this.Dismount();
            if (this is NPC)
              this.IsInCombat = true;
            else
              this.m_attackTimer.Start(1);
          }
        }
        else
        {
          this.CancelPendingAbility();
          NPC npc = this as NPC;
          if (npc != null)
            npc.Brain.StopCurrentAction();
          this.CheckCombatState();
        }
      }
    }

    /// <summary>
    /// Tries to land a mainhand hit + maybe offhand hit on the current Target
    /// </summary>
    protected virtual void CombatTick(int timeElapsed)
    {
      if (this.IsUsingSpell && !this.m_spellCast.IsPending)
        this.m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
      else if (!this.CheckCombatState())
      {
        if (!this.m_isInCombat)
          return;
        this.m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
      }
      else if (!this.CanDoHarm || !this.CanMelee)
      {
        this.m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
      }
      else
      {
        Unit target = this.m_target;
        if (target == null || target.IsDead)
        {
          this.IsFighting = false;
          if (!(this is NPC))
            return;
          this.Movement.MayMove = true;
        }
        else
        {
          int tickCount = Environment.TickCount;
          bool usingRangedWeapon = this.IsUsingRangedWeapon;
          int num1 = this.m_lastStrike + (usingRangedWeapon ? this.RangedAttackTime : this.MainHandAttackTime) - tickCount;
          if (num1 <= 0)
          {
            if (this is NPC)
              this.Movement.MayMove = true;
            float distanceSq = this.GetDistanceSq((WorldObject) target);
            IAsda2Weapon weapon = usingRangedWeapon ? this.m_RangedWeapon : this.m_mainWeapon;
            if (weapon != null)
            {
              if (this.IsInAttackRangeSq(weapon, target, distanceSq))
              {
                if (this.m_AutorepeatSpell != null)
                {
                  if (!this.IsMoving)
                  {
                    this.SpellCast.TargetFlags = SpellTargetFlags.Unit;
                    this.SpellCast.SelectedTarget = (WorldObject) target;
                    int num2 = (int) this.SpellCast.Start(this.m_AutorepeatSpell, false);
                    this.m_lastStrike = tickCount;
                    num1 += this.RangedAttackTime;
                  }
                }
                else
                {
                  Character character = this as Character;
                  if (character != null)
                    character.IsMoving = false;
                  this.Strike(weapon);
                  this.m_lastStrike = tickCount;
                  num1 += this.MainHandAttackTime;
                  if (this is NPC)
                    this.Movement.MayMove = false;
                }
              }
              else
              {
                if (this.UsesPendingAbility(weapon))
                  this.m_spellCast.Cancel(SpellFailedReason.OutOfRange);
                if (this is Character)
                  CombatHandler.SendAttackSwingNotInRange(this as Character);
                else if (this is NPC)
                  this.Brain.OnCombatTargetOutOfRange();
              }
            }
          }
          this.m_attackTimer.Start(num1 <= 0 ? 1000 : num1);
        }
      }
    }

    /// <summary>
    /// Checks whether the Unit can attack.
    /// Also deactivates combat mode, if unit has left combat for long enough.
    /// </summary>
    protected virtual bool CheckCombatState()
    {
      if (this.m_comboTarget != null && (!this.m_comboTarget.IsInContext || !this.m_comboTarget.IsAlive))
        this.ResetComboPoints();
      if (this.m_target == null || !this.CanHarm((WorldObject) this.m_target))
        this.IsFighting = false;
      else if (!this.CanSee((WorldObject) this.m_target))
      {
        this.Target = (Unit) null;
        this.IsFighting = false;
      }
      else if (!this.CanDoHarm)
        return false;
      return this.m_isFighting;
    }

    /// <summary>
    /// Resets the attack timer to delay the next strike by the current weapon delay,
    /// if Unit is fighting.
    /// </summary>
    public void ResetAttackTimer()
    {
      if (this.m_isFighting)
      {
        int num = this.m_offhandWeapon == null ? this.MainHandAttackTime : Math.Min(this.MainHandAttackTime, this.OffHandAttackTime);
        if (this.m_RangedWeapon != null && this.m_RangedWeapon.IsRanged)
          num = Math.Min(num, this.RangedAttackTime);
        this.m_attackTimer.Start(num);
      }
      else
        this.m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
    }

    /// <summary>Is called whenever this Unit enters Combat mode</summary>
    protected virtual void OnEnterCombat()
    {
      this.SheathType = this.IsUsingRangedWeapon ? SheathType.Ranged : SheathType.Melee;
      this.StandState = StandState.Stand;
      this.m_lastCombatTime = this.m_lastUpdateTime;
      if (this.m_brain == null)
        return;
      this.m_brain.OnEnterCombat();
    }

    /// <summary>Is called whenever this Unit leaves Combat mode</summary>
    protected virtual void OnLeaveCombat()
    {
      this.ResetComboPoints();
      if (this.m_brain == null)
        return;
      this.m_brain.OnLeaveCombat();
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
      this.IsSitting = false;
      if (!(action is DamageAction) || action.Attacker == null)
        return;
      if (action.ActualDamage <= 0)
        return;
      if (!action.IsDot)
      {
        Unit attacker = action.Attacker;
        if (action.Weapon != null)
        {
          int num = attacker.IsPvPing ? 1 : 0;
        }
        this.m_auras.RemoveByFlag(AuraInterruptFlags.OnDamage);
        attacker.m_lastCombatTime = attacker.m_lastUpdateTime;
        this.StandState = StandState.Stand;
        if (this.IsAlive)
        {
          this.IsInCombat = true;
          this.m_lastCombatTime = this.m_lastUpdateTime;
        }
      }
      this.TriggerProcOnDamageReceived(action);
    }

    private void TriggerProcOnDamageReceived(IDamageAction action)
    {
      ProcHitFlags hitFlags = action.IsCritical ? ProcHitFlags.CriticalHit : ProcHitFlags.None;
      ProcTriggerFlags flags = ProcTriggerFlags.ReceivedAnyDamage;
      if (action.IsDot)
      {
        action.Attacker.Proc(ProcTriggerFlags.DonePeriodicDamageOrHeal, this, (IUnitAction) action, true, hitFlags);
        flags |= ProcTriggerFlags.ReceivedPeriodicDamageOrHeal;
      }
      this.Proc(flags, action.Attacker, (IUnitAction) action, true, hitFlags);
    }

    public float GetAttackRange(IAsda2Weapon weapon, Unit target)
    {
      return weapon.MaxRange + this.CombatReach + target.CombatReach;
    }

    public float GetBaseAttackRange(Unit target)
    {
      return this.MaxAttackRange + target.CombatReach;
    }

    public bool IsInBaseAttackRange(Unit target)
    {
      float distanceSq = this.GetDistanceSq((WorldObject) target);
      float baseAttackRange = this.GetBaseAttackRange(target);
      return (double) distanceSq <= (double) baseAttackRange * (double) baseAttackRange;
    }

    public bool IsInBaseAttackRangeSq(Unit target, float distSq)
    {
      float baseAttackRange = this.GetBaseAttackRange(target);
      return (double) distSq <= (double) baseAttackRange * (double) baseAttackRange;
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
      float distanceSq = this.GetDistanceSq((WorldObject) target);
      return this.IsInAttackRangeSq(target, distanceSq);
    }

    /// <summary>
    /// Whether the suitable target is in reach to be attacked
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool CanReachForCombat(Unit target)
    {
      if (!this.CanMove)
        return this.IsInAttackRange(target);
      return true;
    }

    public bool IsInAttackRangeSq(Unit target, float distSq)
    {
      if (!this.CanMelee)
      {
        if (this is NPC)
          return this.IsInBaseAttackRangeSq(target, distSq);
        return false;
      }
      if (this.UsesRangedWeapon && this.IsInAttackRangeSq(this.m_RangedWeapon, target, distSq))
        return true;
      return this.IsInMeleeAttackRangeSq(this.m_mainWeapon, target, distSq);
    }

    public bool IsInMaxRange(Spell spell, WorldObject target)
    {
      float spellMaxRange = this.GetSpellMaxRange(spell, target);
      return (double) this.GetDistanceSq(target) <= (double) spellMaxRange * (double) spellMaxRange;
    }

    public bool IsInSpellRange(Spell spell, WorldObject target)
    {
      float spellMaxRange = this.GetSpellMaxRange(spell, target);
      float distanceSq = this.GetDistanceSq(target);
      if ((double) spell.Range.MinDist > 0.0)
      {
        float minDist = spell.Range.MinDist;
        if ((double) distanceSq < (double) minDist * (double) minDist)
          return false;
      }
      return (double) distanceSq <= (double) spellMaxRange * (double) spellMaxRange;
    }

    /// <summary>Melee has no Min range</summary>
    public bool IsInMeleeAttackRangeSq(IAsda2Weapon weapon, Unit target, float distSq)
    {
      float attackRange = this.GetAttackRange(weapon, target);
      return (double) distSq <= (double) attackRange * (double) attackRange;
    }

    public bool IsInAttackRangeSq(IAsda2Weapon weapon, Unit target, float distSq)
    {
      float range = this.GetAttackRange(weapon, target);
      if (this.UsesPendingAbility(weapon))
        range = this.GetSpellMaxRange(this.m_spellCast.Spell, range);
      if ((double) distSq > (double) range * (double) range)
        return false;
      if (weapon.IsRanged)
      {
        float minAttackRange = this.GetMinAttackRange(weapon, target);
        if ((double) distSq < (double) minAttackRange * (double) minAttackRange)
          return false;
      }
      return true;
    }

    public bool IsInRange(SimpleRange range, WorldObject obj)
    {
      float distanceSq = this.GetDistanceSq(obj);
      if ((double) distanceSq > (double) range.MaxDist * (double) range.MaxDist)
        return false;
      if ((double) range.MinDist >= 1.0)
        return (double) distanceSq >= (double) range.MinDist * (double) range.MinDist;
      return true;
    }

    public virtual float AggroBaseRange
    {
      get
      {
        return NPCEntry.AggroBaseRangeDefault + this.BoundingRadius;
      }
    }

    public virtual float GetAggroRange(Unit victim)
    {
      return Math.Max(this.AggroBaseRange + (float) (this.Level - victim.Level) * NPCEntry.AggroRangePerLevel, NPCEntry.AggroRangeMinDefault);
    }

    public float GetAggroRangeSq(Unit victim)
    {
      float aggroRange = this.GetAggroRange(victim);
      return aggroRange * aggroRange;
    }

    /// <summary>
    /// Checks for hostility etc
    /// 
    /// TODO: Restrict interference in Duels
    /// </summary>
    public SpellFailedReason CanCastSpellOn(Unit target, Spell spell)
    {
      bool flag = this.CanHarm((WorldObject) target);
      if ((!flag || spell.HasHarmfulEffects) && (flag || spell.HasBeneficialEffects))
        return SpellFailedReason.Ok;
      return !flag ? SpellFailedReason.TargetEnemy : SpellFailedReason.TargetFriendly;
    }

    /// <summary>
    /// The maximum distance in yards to a valid attackable target
    /// </summary>
    public float CombatReach
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.COMBATREACH);
      }
      set
      {
        this.SetFloat((UpdateFieldId) UnitFields.COMBATREACH, value);
      }
    }

    public virtual float MaxAttackRange
    {
      get
      {
        return this.CombatReach + this.m_mainWeapon.MaxRange;
      }
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
      this.AddDamageDoneModSilently(school, delta);
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
      this.RemoveDamageDoneModSilently(school, delta);
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
      foreach (DamageSchool school in schools)
        this.AddDamageDoneModSilently(school, delta);
    }

    /// <summary>
    /// Adds/Removes a flat modifier to all of the given damage schools
    /// </summary>
    public void RemoveDamageDoneMod(uint[] schools, int delta)
    {
      foreach (DamageSchool school in schools)
        this.RemoveDamageDoneModSilently(school, delta);
    }

    public void ModDamageDoneFactor(DamageSchool school, float delta)
    {
      this.ModDamageDoneFactorSilently(school, delta);
    }

    /// <summary>
    /// Adds/Removes a percent modifier to all of the given damage schools
    /// </summary>
    public void ModDamageDoneFactor(uint[] schools, float delta)
    {
      foreach (DamageSchool school in schools)
        this.ModDamageDoneFactorSilently(school, delta);
    }

    /// <summary>
    /// Get total damage, after adding/subtracting all modifiers (is not used for DoT)
    /// </summary>
    public int GetFinalDamage(DamageSchool school, int dmg, Spell spell = null)
    {
      if (spell != null)
        dmg = this.Auras.GetModifiedInt(SpellModifierType.SpellPower, spell, dmg);
      return dmg;
    }

    /// <summary>
    /// Whether this Unit currently has a ranged weapon equipped
    /// </summary>
    public bool UsesRangedWeapon
    {
      get
      {
        if (this.m_RangedWeapon != null)
          return this.m_RangedWeapon.IsRanged;
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
        if (this.SheathType != SheathType.Ranged)
          return this.m_offhandWeapon != null;
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
      get
      {
        return this.m_mainWeapon;
      }
      set
      {
        if (value == this.m_mainWeapon)
          return;
        if (value == null)
        {
          this.m_mainWeapon = (IAsda2Weapon) GenericWeapon.Fists;
          this.UpdateMainDamage();
          this.UpdateMainAttackTime();
        }
        else
        {
          this.m_mainWeapon = value;
          Asda2Item mainWeapon = this.m_mainWeapon as Asda2Item;
          if (mainWeapon != null)
          {
            if (!mainWeapon.Template.IsWeapon)
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
      switch (slot)
      {
        case EquipmentSlot.MainHand:
          return this.m_mainWeapon;
        case EquipmentSlot.OffHand:
          return this.m_offhandWeapon;
        case EquipmentSlot.ExtraWeapon:
          return this.m_RangedWeapon;
        default:
          return (IAsda2Weapon) null;
      }
    }

    public IAsda2Weapon GetWeapon(InventorySlotType slot)
    {
      switch (slot)
      {
        case InventorySlotType.WeaponRanged:
          return this.m_RangedWeapon;
        case InventorySlotType.WeaponMainHand:
          return this.m_mainWeapon;
        case InventorySlotType.WeaponOffHand:
          return this.m_offhandWeapon;
        default:
          return (IAsda2Weapon) null;
      }
    }

    public void SetWeapon(InventorySlotType slot, IAsda2Weapon weapon)
    {
      if (slot != InventorySlotType.WeaponMainHand)
        return;
      this.MainWeapon = weapon;
    }

    /// <summary>Whether this Unit is allowed to melee at all</summary>
    public bool CanMelee
    {
      get
      {
        if (this.MeleePermissionCounter > 0)
          return this.m_canInteract;
        return false;
      }
    }

    /// <summary>If greater 0, may melee, else not</summary>
    public int MeleePermissionCounter { get; internal set; }

    public void IncMeleePermissionCounter()
    {
      ++this.MeleePermissionCounter;
    }

    public void DecMeleePermissionCounter()
    {
      --this.MeleePermissionCounter;
    }

    public bool MayCarry(InventorySlotTypeMask itemMask)
    {
      return (itemMask & this.DisarmMask) == InventorySlotTypeMask.None;
    }

    /// <summary>The mask of slots of currently disarmed items.</summary>
    public InventorySlotTypeMask DisarmMask
    {
      get
      {
        return this.m_DisarmMask;
      }
    }

    /// <summary>
    /// Disarms the weapon of the given type (WeaponMainHand, WeaponRanged or WeaponOffHand)
    /// </summary>
    public void SetDisarmed(InventorySlotType type)
    {
      InventorySlotTypeMask mask = type.ToMask();
      if (this.m_DisarmMask.HasAnyFlag(mask))
        return;
      this.m_DisarmMask |= mask;
      this.SetWeapon(type, (IAsda2Weapon) null);
    }

    /// <summary>
    /// Rearms the weapon of the given type (WeaponMainHand, WeaponRanged or WeaponOffHand)
    /// </summary>
    public void UnsetDisarmed(InventorySlotType type)
    {
      InventorySlotTypeMask mask = type.ToMask();
      if (!this.m_DisarmMask.HasAnyFlag(mask))
        return;
      this.m_DisarmMask &= ~mask;
      this.SetWeapon(type, this.GetOrInvalidateItem(type));
    }

    /// <summary>
    /// Finds the item for the given slot.
    /// Unequips it and returns null, if it may not currently be used.
    /// </summary>
    protected virtual IAsda2Weapon GetOrInvalidateItem(InventorySlotType type)
    {
      return (IAsda2Weapon) null;
    }

    /// <summary>Time in millis between 2 Main-hand strikes</summary>
    public int MainHandAttackTime
    {
      get
      {
        return this.GetInt32(UnitFields.BASEATTACKTIME);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.BASEATTACKTIME, value);
      }
    }

    /// <summary>Time in millis between 2 Off-hand strikes</summary>
    public int OffHandAttackTime
    {
      get
      {
        return this.GetInt32(UnitFields.BASEATTACKTIME_2);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.BASEATTACKTIME_2, value);
      }
    }

    /// <summary>Time in millis between 2 ranged strikes</summary>
    public int RangedAttackTime
    {
      get
      {
        return this.GetInt32(UnitFields.RANGEDATTACKTIME);
      }
      set
      {
        this.SetInt32((UpdateFieldId) UnitFields.RANGEDATTACKTIME, value);
      }
    }

    public float MinDamage
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.MINDAMAGE);
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) UnitFields.MINDAMAGE, value);
      }
    }

    public float MaxDamage
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.MAXDAMAGE);
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) UnitFields.MAXDAMAGE, value);
      }
    }

    public float MinOffHandDamage
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.MINOFFHANDDAMAGE);
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) UnitFields.MINOFFHANDDAMAGE, value);
      }
    }

    public float MaxOffHandDamage
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.MAXOFFHANDDAMAGE);
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) UnitFields.MAXOFFHANDDAMAGE, value);
      }
    }

    public float MinRangedDamage
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.MINRANGEDDAMAGE);
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) UnitFields.MINRANGEDDAMAGE, value);
      }
    }

    public float MaxRangedDamage
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.MAXRANGEDDAMAGE);
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) UnitFields.MAXRANGEDDAMAGE, value);
      }
    }

    public int MeleeAttackPower
    {
      get
      {
        return this.GetInt32(UnitFields.ATTACK_POWER);
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) UnitFields.ATTACK_POWER, value);
      }
    }

    public int MeleeAttackPowerModsPos
    {
      get
      {
        return (int) this.GetUInt16Low((UpdateFieldId) UnitFields.ATTACK_POWER_MODS);
      }
      set
      {
        this.SetUInt16Low((UpdateFieldId) UnitFields.ATTACK_POWER_MODS, (ushort) value);
        this.UpdateMeleeAttackPower();
      }
    }

    public int MeleeAttackPowerModsNeg
    {
      get
      {
        return (int) this.GetUInt16High((UpdateFieldId) UnitFields.ATTACK_POWER_MODS);
      }
      set
      {
        this.SetUInt16High((UpdateFieldId) UnitFields.ATTACK_POWER_MODS, (ushort) value);
        this.UpdateMeleeAttackPower();
      }
    }

    public float MeleeAttackPowerMultiplier
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.ATTACK_POWER_MULTIPLIER);
      }
      set
      {
        this.SetFloat((UpdateFieldId) UnitFields.ATTACK_POWER_MULTIPLIER, value);
        this.UpdateMeleeAttackPower();
      }
    }

    public int TotalMeleeAP
    {
      get
      {
        return MathUtil.RoundInt((1f + this.MeleeAttackPowerMultiplier) * (float) (this.MeleeAttackPower + this.MeleeAttackPowerModsPos - this.MeleeAttackPowerModsNeg));
      }
    }

    public int RangedAttackPower
    {
      get
      {
        return this.GetInt32(UnitFields.RANGED_ATTACK_POWER);
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) UnitFields.RANGED_ATTACK_POWER, value);
      }
    }

    public int RangedAttackPowerModsPos
    {
      get
      {
        return (int) this.GetInt16Low((UpdateFieldId) UnitFields.RANGED_ATTACK_POWER_MODS);
      }
      set
      {
        this.SetInt16Low((UpdateFieldId) UnitFields.RANGED_ATTACK_POWER_MODS, (short) value);
        this.UpdateRangedAttackPower();
      }
    }

    public int RangedAttackPowerModsNeg
    {
      get
      {
        return (int) this.GetInt16High((UpdateFieldId) UnitFields.RANGED_ATTACK_POWER_MODS);
      }
      set
      {
        this.SetInt16High((UpdateFieldId) UnitFields.RANGED_ATTACK_POWER_MODS, (short) value);
        this.UpdateRangedAttackPower();
      }
    }

    public float RangedAttackPowerMultiplier
    {
      get
      {
        return this.GetFloat((UpdateFieldId) UnitFields.RANGED_ATTACK_POWER_MULTIPLIER);
      }
      set
      {
        this.SetFloat((UpdateFieldId) UnitFields.RANGED_ATTACK_POWER_MULTIPLIER, value);
        this.UpdateRangedAttackPower();
      }
    }

    public int TotalRangedAP
    {
      get
      {
        return MathUtil.RoundInt((1f + this.RangedAttackPowerMultiplier) * (float) (this.RangedAttackPower + this.RangedAttackPowerModsPos - this.RangedAttackPowerModsNeg));
      }
    }

    public float RandomMagicDamage
    {
      get
      {
        return (float) Utility.Random(this.MinMagicDamage, this.MaxMagicDamage);
      }
    }

    public float RandomDamage
    {
      get
      {
        return Utility.Random(this.MinDamage, this.MaxDamage);
      }
    }

    /// <summary>
    /// Deals environmental damage to this Unit (cannot be resisted)
    /// </summary>
    public virtual void DealEnvironmentalDamage(EnviromentalDamageType dmgType, int amount)
    {
      this.DoRawDamage((IDamageAction) new SimpleDamageAction()
      {
        Damage = amount,
        Victim = this
      });
      CombatLogHandler.SendEnvironmentalDamage((WorldObject) this, dmgType, (uint) amount);
    }

    public void CalcFallingDamage(int speed)
    {
    }

    /// <summary>
    /// Deals damage, cancels damage-sensitive Auras, checks for spell interruption etc
    /// </summary>
    public void DoRawDamage(IDamageAction action)
    {
      if (this.m_FirstAttacker == null && action.Attacker != null)
        this.FirstAttacker = action.Attacker;
      if (this.m_damageTakenMods != null)
        action.Damage += this.m_damageTakenMods[(int) action.UsedSchool];
      if (this.m_damageTakenPctMods != null)
      {
        int damageTakenPctMod = this.m_damageTakenPctMods[(int) action.UsedSchool];
        if (damageTakenPctMod != 0)
          action.Damage -= (damageTakenPctMod * action.Damage + 50) / 100;
      }
      if (action.Spell != null && action.Spell.IsAreaSpell && this.AoEDamageModifierPct != 0)
        action.Damage -= (action.Damage * this.AoEDamageModifierPct + 50) / 100;
      action.Victim.OnDamageAction(action);
      int actualDamage = action.ActualDamage;
      if (actualDamage <= 0)
        return;
      if (this.m_brain != null)
        this.m_brain.OnDamageReceived(action);
      if (action.Attacker != null && action.Attacker.Brain != null)
        action.Attacker.m_brain.OnDamageDealt(action);
      this.LastDamageDelay = action.Spell == null ? 300 : (int) action.Spell.CastDelay;
      this.Health -= actualDamage;
      if (this.IsAlive)
        return;
      this.OnKilled(action);
    }

    /// <summary>
    /// Called after this unit has been killed by damage action
    /// </summary>
    /// <param name="action">Action which killed this unit</param>
    protected virtual void OnKilled(IDamageAction action)
    {
      this.TriggerProcOnKilled(action);
      this.LastKiller = action.Attacker;
    }

    private void TriggerProcOnKilled(IDamageAction killingAction)
    {
      if (this.YieldsXpOrHonor && killingAction.Attacker != null)
        killingAction.Attacker.Proc(ProcTriggerFlags.KilledTargetThatYieldsExperienceOrHonor, this, (IUnitAction) killingAction, true, ProcHitFlags.None);
      this.Proc(ProcTriggerFlags.Death, killingAction.Attacker, (IUnitAction) killingAction, true, ProcHitFlags.None);
    }

    [WCell.Core.Initialization.Initialization(InitializationPass.Tenth)]
    public static void InitSpeeds()
    {
    }

    /// <summary>Creates an array for a set of SpellMechanics</summary>
    protected static int[] CreateMechanicsArr()
    {
      return new int[Unit.MechanicCount];
    }

    protected static int[] CreateDamageSchoolArr()
    {
      return new int[Unit.DamageSchoolCount];
    }

    protected static int[] CreateDispelTypeArr()
    {
      return new int[Unit.DispelTypeCount];
    }

    /// <summary>
    /// Whether the Unit is allowed to cast spells that are not physical abilities
    /// </summary>
    public bool CanCastSpells
    {
      get
      {
        return this.m_canCastSpells;
      }
    }

    /// <summary>
    /// Whether the Unit is allowed to attack and use physical abilities
    /// </summary>
    public bool CanDoPhysicalActivity
    {
      get
      {
        return this.m_canDoPhysicalActivity;
      }
      private set
      {
        if (this.m_canDoPhysicalActivity != value)
          return;
        this.m_canDoPhysicalActivity = value;
        if (!value)
          return;
        this.IsFighting = false;
        if (this.m_spellCast == null || !this.m_spellCast.IsCasting || !this.m_spellCast.Spell.IsPhysicalAbility)
          return;
        this.m_spellCast.Cancel(SpellFailedReason.Pacified);
      }
    }

    public bool CanMove
    {
      get
      {
        if (this.m_canMove)
          return this.HasPermissionToMove;
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
        if (this.m_Movement != null)
          return this.m_Movement.MayMove;
        return true;
      }
      set
      {
        if (this.m_Movement == null)
          return;
        this.m_Movement.MayMove = value;
      }
    }

    /// <summary>
    /// Whether the Unit is currently evading (cannot be hit, generate threat etc)
    /// </summary>
    public bool IsEvading
    {
      get
      {
        return this.m_evades;
      }
      set
      {
        this.m_evades = value;
        if (value)
        {
          this.m_auras.RemoveOthersAuras();
          this.IncMechanicCount(SpellMechanic.Invulnerable, false);
        }
        else
          this.DecMechanicCount(SpellMechanic.Invulnerable, false);
      }
    }

    /// <summary>whether the Unit can be interacted with</summary>
    public bool CanInteract
    {
      get
      {
        return this.m_canInteract;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public override bool CanDoHarm
    {
      get
      {
        if (this.m_canHarm)
          return base.CanDoHarm;
        return false;
      }
    }

    /// <summary>Whether this Unit is currently stunned (!= rooted)</summary>
    public int Stunned
    {
      get
      {
        if (this.m_mechanics == null)
          return 0;
        return this.m_mechanics[12];
      }
      set
      {
        if (this.m_mechanics == null)
          this.m_mechanics = Unit.CreateMechanicsArr();
        if (value <= 0)
        {
          this.m_mechanics[12] = 1;
          this.DecMechanicCount(SpellMechanic.Stunned, false);
        }
        else
        {
          if (this.Stunned != 0)
            return;
          this.IncMechanicCount(SpellMechanic.Stunned, false);
          this.m_mechanics[12] = value;
        }
      }
    }

    public int Invulnerable
    {
      get
      {
        if (this.m_mechanics == null)
          return 0;
        return this.m_mechanics[25];
      }
      set
      {
        if (this.m_mechanics == null)
          this.m_mechanics = Unit.CreateMechanicsArr();
        if (value <= 0)
        {
          this.m_mechanics[25] = 1;
          this.DecMechanicCount(SpellMechanic.Invulnerable, false);
        }
        else
        {
          if (this.Invulnerable != 0)
            return;
          this.IncMechanicCount(SpellMechanic.Invulnerable, false);
          this.m_mechanics[25] = value;
        }
      }
    }

    /// <summary>
    /// Pacified units cannot attack or use physical abilities
    /// </summary>
    public int Pacified
    {
      get
      {
        return this.m_Pacified;
      }
      set
      {
        if (this.m_Pacified == value)
          return;
        if (value <= 0 && this.m_Pacified > 0)
          this.UnitFlags &= UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Stunned | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
        else if (value > 0)
          this.UnitFlags |= UnitFlags.Pacified;
        this.m_Pacified = value;
        this.SetCanHarmState();
      }
    }

    /// <summary>Return whether the given Mechanic applies to the Unit</summary>
    public bool IsUnderInfluenceOf(SpellMechanic mechanic)
    {
      if (this.m_mechanics == null)
        return false;
      return this.m_mechanics[(int) mechanic] > 0;
    }

    /// <summary>
    /// Increase the mechanic modifier count for the given SpellMechanic
    /// </summary>
    public void IncMechanicCount(SpellMechanic mechanic, bool isCustom = false)
    {
      if (this.m_mechanics == null)
        this.m_mechanics = Unit.CreateMechanicsArr();
      if (this.m_mechanics[(int) mechanic] == 0)
      {
        if (!isCustom)
        {
          if (this.m_canMove && SpellConstants.MoveMechanics[(int) mechanic])
          {
            this.m_canMove = false;
            if (!this.IsPlayer)
              this.Target = (Unit) null;
            this.UnitFlags |= UnitFlags.Stunned;
            this.CancelTaxiFlight();
            if (this is Character)
              MovementHandler.SendRooted((Character) this, 1);
            if (this.IsUsingSpell && this.SpellCast.Spell.InterruptFlags.HasFlag((Enum) InterruptFlags.OnStunned))
              this.SpellCast.Cancel(SpellFailedReason.Interrupted);
            this.StopMoving();
          }
          if (this.m_canInteract && SpellConstants.InteractMechanics[(int) mechanic])
            this.m_canInteract = false;
          if (this.m_canHarm && SpellConstants.HarmPreventionMechanics[(int) mechanic])
            this.SetCanHarmState();
          if (this.m_canCastSpells && SpellConstants.SpellCastPreventionMechanics[(int) mechanic])
          {
            this.m_canCastSpells = false;
            if (this.IsUsingSpell && this.SpellCast.Spell.InterruptFlags.HasFlag((Enum) InterruptFlags.OnSilence))
              this.SpellCast.Cancel(SpellFailedReason.Interrupted);
            if (!this.m_canDoPhysicalActivity && this.m_canHarm)
              this.SetCanHarmState();
          }
        }
        switch (mechanic)
        {
          case SpellMechanic.Disoriented:
            this.UnitFlags |= UnitFlags.Confused;
            break;
          case SpellMechanic.Fleeing:
            this.UnitFlags |= UnitFlags.Feared;
            break;
          case SpellMechanic.Silenced:
            this.UnitFlags |= UnitFlags.Silenced;
            break;
          case SpellMechanic.Frozen:
            this.AuraState |= AuraStateMask.Frozen;
            break;
          case SpellMechanic.Bleeding:
            this.AuraState |= AuraStateMask.Bleeding;
            break;
          case SpellMechanic.Mounted:
            this.UnitFlags |= UnitFlags.Mounted;
            this.SpeedFactor += this.MountSpeedMod;
            this.m_auras.RemoveByFlag(AuraInterruptFlags.OnMount);
            break;
          case SpellMechanic.Invulnerable:
            this.UnitFlags |= UnitFlags.SelectableNotAttackable;
            break;
          case SpellMechanic.Enraged:
            this.AuraState |= AuraStateMask.Enraged;
            break;
          case SpellMechanic.Custom_Immolate:
            this.AuraState |= AuraStateMask.Immolate;
            break;
        }
      }
      ++this.m_mechanics[(int) mechanic];
    }

    /// <summary>
    /// Decrease the mechanic modifier count for the given SpellMechanic
    /// </summary>
    public void DecMechanicCount(SpellMechanic mechanic, bool isCustom = false)
    {
      if (this.m_mechanics == null)
        return;
      int mechanic1 = this.m_mechanics[(int) mechanic];
      if (mechanic1 <= 0)
        return;
      this.m_mechanics[(int) mechanic] = mechanic1 - 1;
      if (mechanic1 != 1)
        return;
      if (!isCustom)
      {
        if (!this.m_canMove && SpellConstants.MoveMechanics[(int) mechanic] && !this.IsAnySetNoCheck(SpellConstants.MoveMechanics))
        {
          this.m_canMove = true;
          this.UnitFlags &= UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Pacified | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          this.m_lastMoveTime = Environment.TickCount;
          if (this is Character)
            MovementHandler.SendUnrooted((Character) this);
        }
        if (!this.m_canInteract && SpellConstants.InteractMechanics[(int) mechanic] && !this.IsAnySetNoCheck(SpellConstants.InteractMechanics))
          this.m_canInteract = true;
        if (!this.m_canHarm && SpellConstants.HarmPreventionMechanics[(int) mechanic])
          this.SetCanHarmState();
        if (!this.m_canCastSpells && SpellConstants.SpellCastPreventionMechanics[(int) mechanic] && !this.IsAnySetNoCheck(SpellConstants.SpellCastPreventionMechanics))
        {
          this.m_canCastSpells = true;
          if (!this.m_canDoPhysicalActivity && !this.m_canHarm)
            this.SetCanHarmState();
        }
      }
      switch (mechanic)
      {
        case SpellMechanic.Disoriented:
          this.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          break;
        case SpellMechanic.Fleeing:
          this.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          break;
        case SpellMechanic.Silenced:
          this.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          break;
        case SpellMechanic.Frozen:
          this.AuraState ^= AuraStateMask.Frozen;
          break;
        case SpellMechanic.Bleeding:
          this.AuraState ^= AuraStateMask.Bleeding;
          break;
        case SpellMechanic.Mounted:
          this.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          this.SpeedFactor -= this.MountSpeedMod;
          break;
        case SpellMechanic.Invulnerable:
          this.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
          break;
        case SpellMechanic.Enraged:
          this.AuraState &= ~AuraStateMask.Enraged;
          break;
        case SpellMechanic.Custom_Immolate:
          this.AuraState ^= AuraStateMask.Immolate;
          break;
      }
    }

    /// <summary>
    /// Checks whether any of the mechanics of the given set are influencing the owner
    /// </summary>
    private bool IsAnySetNoCheck(bool[] set)
    {
      if (this.m_mechanics == null)
        return false;
      for (int index = 0; index < set.Length; ++index)
      {
        if (set[index] && this.m_mechanics[index] > 0)
          return true;
      }
      return false;
    }

    private void SetCanHarmState()
    {
      if (!this.IsAnySetNoCheck(SpellConstants.HarmPreventionMechanics))
      {
        this.CanDoPhysicalActivity = this.m_Pacified <= 0;
        this.m_canHarm = this.m_canDoPhysicalActivity || !this.IsUnderInfluenceOf(SpellMechanic.Silenced);
      }
      else
      {
        this.CanDoPhysicalActivity = false;
        this.m_canHarm = false;
      }
    }

    /// <summary>Whether the owner is completely invulnerable</summary>
    public bool IsInvulnerable
    {
      get
      {
        if (this.m_mechanics == null)
          return false;
        if (this.m_mechanics[25] <= 0)
          return this.m_mechanics[29] > 0;
        return true;
      }
      set
      {
        if (this.m_mechanics == null)
          this.m_mechanics = Unit.CreateMechanicsArr();
        if (value)
          ++this.m_mechanics[25];
        else
          this.m_mechanics[25] = 0;
      }
    }

    /// <summary>
    /// Indicates whether the owner is immune against the given SpellMechanic
    /// </summary>
    public bool IsImmune(SpellMechanic mechanic)
    {
      if (mechanic == SpellMechanic.None || this.m_mechanicImmunities == null)
        return false;
      return this.m_mechanicImmunities[(int) mechanic] > 0;
    }

    /// <summary>
    /// Indicates whether the owner is immune against the given DamageSchool
    /// </summary>
    public bool IsImmune(DamageSchool school)
    {
      if (this.m_dmgImmunities != null)
        return this.m_dmgImmunities[(int) school] > 0;
      return false;
    }

    public bool IsImmuneToSpell(Spell spell)
    {
      return spell.Mechanic.IsNegative() && spell.IsAffectedByInvulnerability && (spell.Mechanic == SpellMechanic.Invulnerable_2 || spell.Mechanic == SpellMechanic.Invulnerable) && (this.IsInvulnerable || this.IsImmune(SpellMechanic.Invulnerable_2) || (this.IsImmune(SpellMechanic.Invulnerable) || this.IsImmune(spell.Mechanic)) || this.IsImmune(spell.DispelType));
    }

    /// <summary>Adds immunity against given damage-school</summary>
    public void IncDmgImmunityCount(DamageSchool school)
    {
      if (this.m_dmgImmunities == null)
        this.m_dmgImmunities = Unit.CreateDamageSchoolArr();
      if (this.m_dmgImmunities[(int) school] == 0)
        this.Auras.RemoveWhere((Predicate<Aura>) (aura => aura.Spell.SchoolMask.HasAnyFlag((DamageSchoolMask) (1 << (int) (school & (DamageSchool) 31)))));
      ++this.m_dmgImmunities[(int) school];
    }

    /// <summary>Adds immunity against given damage-schools</summary>
    public void IncDmgImmunityCount(uint[] schools)
    {
      foreach (DamageSchool school in schools)
        this.IncDmgImmunityCount(school);
    }

    /// <summary>Adds immunity against given damage-schools</summary>
    public void IncDmgImmunityCount(SpellEffect effect)
    {
      if (this.m_dmgImmunities == null)
        this.m_dmgImmunities = Unit.CreateDamageSchoolArr();
      foreach (int miscBit in effect.MiscBitSet)
        ++this.m_dmgImmunities[miscBit];
      this.Auras.RemoveWhere((Predicate<Aura>) (aura =>
      {
        if ((int) aura.Spell.AuraUID != (int) effect.Spell.AuraUID && aura.Spell.SchoolMask.HasAnyFlag(effect.Spell.SchoolMask))
          return !aura.Spell.Attributes.HasFlag((Enum) SpellAttributes.UnaffectedByInvulnerability);
        return false;
      }));
    }

    /// <summary>Decreases immunity-count against given damage-school</summary>
    public void DecDmgImmunityCount(DamageSchool school)
    {
      if (this.m_dmgImmunities == null || this.m_dmgImmunities[(int) school] <= 0)
        return;
      --this.m_dmgImmunities[(int) school];
    }

    /// <summary>Decreases immunity-count against given damage-schools</summary>
    public void DecDmgImmunityCount(uint[] damageSchools)
    {
      foreach (DamageSchool damageSchool in damageSchools)
        this.DecDmgImmunityCount(damageSchool);
    }

    /// <summary>Adds immunity against given SpellMechanic-school</summary>
    public void IncMechImmunityCount(SpellMechanic mechanic, Spell exclude)
    {
      if (this.m_mechanicImmunities == null)
        this.m_mechanicImmunities = Unit.CreateMechanicsArr();
      if (this.m_mechanicImmunities[(int) mechanic] == 0)
        this.Auras.RemoveWhere((Predicate<Aura>) (aura =>
        {
          if (aura.Spell.Mechanic != mechanic || aura.Spell == exclude || aura.Spell.TargetTriggerSpells != null && ((IEnumerable<Spell>) aura.Spell.TargetTriggerSpells).Contains<Spell>(exclude) || aura.Spell.CasterTriggerSpells != null && ((IEnumerable<Spell>) aura.Spell.CasterTriggerSpells).Contains<Spell>(exclude))
            return false;
          if (mechanic == SpellMechanic.Invulnerable || mechanic == SpellMechanic.Invulnerable_2)
            return !aura.Spell.Attributes.HasFlag((Enum) SpellAttributes.UnaffectedByInvulnerability);
          return true;
        }));
      ++this.m_mechanicImmunities[(int) mechanic];
    }

    /// <summary>
    /// Decreases immunity-count against given SpellMechanic-school
    /// </summary>
    public void DecMechImmunityCount(SpellMechanic mechanic)
    {
      if (this.m_mechanicImmunities == null || this.m_mechanicImmunities[(int) mechanic] <= 0)
        return;
      --this.m_mechanicImmunities[(int) mechanic];
    }

    /// <summary>
    /// Returns the resistance chance for the given SpellMechanic
    /// </summary>
    public int GetMechanicResistance(SpellMechanic mechanic)
    {
      if (this.m_mechanicResistances == null)
        return 0;
      return this.m_mechanicResistances[(int) mechanic];
    }

    /// <summary>
    /// Changes the amount of resistance against certain SpellMechanics
    /// </summary>
    public void ModMechanicResistance(SpellMechanic mechanic, int delta)
    {
      if (this.m_mechanicResistances == null)
        this.m_mechanicResistances = Unit.CreateMechanicsArr();
      int num = this.m_mechanicResistances[(int) mechanic] + delta;
      if (num < 0)
        num = 0;
      this.m_mechanicResistances[(int) mechanic] = num;
    }

    /// <summary>
    /// Returns the duration modifier for a certain SpellMechanic
    /// </summary>
    public int GetMechanicDurationMod(SpellMechanic mechanic)
    {
      if (this.m_mechanicDurationMods == null || mechanic == SpellMechanic.None)
        return 0;
      return this.m_mechanicDurationMods[(int) mechanic];
    }

    /// <summary>
    /// Changes the duration-modifier for a certain SpellMechanic in %
    /// </summary>
    public void ModMechanicDurationMod(SpellMechanic mechanic, int delta)
    {
      if (this.m_mechanicDurationMods == null)
        this.m_mechanicDurationMods = Unit.CreateMechanicsArr();
      int num = this.m_mechanicDurationMods[(int) mechanic] + delta;
      if (num < 0)
        num = 0;
      this.m_mechanicDurationMods[(int) mechanic] = num;
    }

    public int GetDebuffResistance(DamageSchool school)
    {
      if (this.m_debuffResistances == null)
        return 0;
      return this.m_debuffResistances[(int) school];
    }

    public void SetDebuffResistance(DamageSchool school, int value)
    {
      if (this.m_debuffResistances == null)
        this.m_debuffResistances = Unit.CreateDamageSchoolArr();
      this.m_debuffResistances[(uint)school] = value;
    }

    public void ModDebuffResistance(DamageSchool school, int delta)
    {
      if (this.m_debuffResistances == null)
        this.m_debuffResistances = Unit.CreateDamageSchoolArr();
      this.m_debuffResistances[(int) school] += delta;
    }

    public bool IsImmune(DispelType school)
    {
      if (this.m_dispelImmunities == null)
        return false;
      return this.m_dispelImmunities[(int) school] > 0;
    }

    public void IncDispelImmunity(DispelType school)
    {
      if (this.m_dispelImmunities == null)
        this.m_dispelImmunities = Unit.CreateDispelTypeArr();
      int dispelImmunity = this.m_dispelImmunities[(uint)school];
      if (dispelImmunity == 0)
        this.Auras.RemoveWhere((Predicate<Aura>) (aura => aura.Spell.DispelType == school));
      this.m_dispelImmunities[(uint)school] = dispelImmunity + 1;
    }

    public void DecDispelImmunity(DispelType school)
    {
      if (this.m_dispelImmunities == null)
        return;
      int dispelImmunity = this.m_dispelImmunities[(uint)school];
      if (dispelImmunity <= 0)
        return;
      this.m_dispelImmunities[(int) school] = dispelImmunity - 1;
    }

    public int GetTargetResistanceMod(DamageSchool school)
    {
      if (this.m_TargetResMods == null)
        return 0;
      return this.m_TargetResMods[(int) school];
    }

    private void SetTargetResistanceMod(DamageSchool school, int value)
    {
      if (this.m_TargetResMods == null)
        this.m_TargetResMods = Unit.CreateDamageSchoolArr();
      this.m_TargetResMods[(uint)school] = value;
      if (school != DamageSchool.Physical || !(this is Character))
        return;
      this.SetInt32((UpdateFieldId) PlayerFields.MOD_TARGET_PHYSICAL_RESISTANCE, value);
    }

    internal void ModTargetResistanceMod(DamageSchool school, int delta)
    {
      if (this.m_TargetResMods == null)
        this.m_TargetResMods = Unit.CreateDamageSchoolArr();
      int num = this.m_TargetResMods[(int) school] + delta;
      this.m_TargetResMods[(int) school] = num;
      if (school != DamageSchool.Physical || !(this is Character))
        return;
      this.SetInt32((UpdateFieldId) PlayerFields.MOD_TARGET_PHYSICAL_RESISTANCE, num);
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
      for (int index = 0; index < dmgTypes.Length; ++index)
        this.ModTargetResistanceMod((DamageSchool) dmgTypes[index], delta);
      if (dmgTypes.Length <= 0 || !(this is Character))
        return;
      this.SetInt32((UpdateFieldId) PlayerFields.MOD_TARGET_RESISTANCE, this.GetInt32(PlayerFields.MOD_TARGET_RESISTANCE) + delta);
    }

    public void ModSpellInterruptProt(DamageSchool school, int delta)
    {
      if (this.m_spellInterruptProt == null)
        this.m_spellInterruptProt = Unit.CreateDamageSchoolArr();
      int num = this.m_spellInterruptProt[(int) school] + delta;
      this.m_spellInterruptProt[(int) school] = num;
    }

    public void ModSpellInterruptProt(uint[] dmgTypes, int delta)
    {
      foreach (DamageSchool dmgType in dmgTypes)
        this.ModSpellInterruptProt(dmgType, delta);
    }

    public int GetSpellInterruptProt(Spell spell)
    {
      if (this.m_spellInterruptProt == null)
        return 0;
      return this.m_spellInterruptProt[(int) spell.Schools[0]];
    }

    public int GetSpellHitChanceMod(DamageSchool school)
    {
      if (this.m_SpellHitChance == null)
        return 0;
      return this.m_SpellHitChance[(int) school];
    }

    public int GetHighestSpellHitChanceMod(DamageSchool[] schools)
    {
      if (this.m_SpellHitChance == null)
        return 0;
      return ((IEnumerable<DamageSchool>) schools).Select<DamageSchool, int>((Func<DamageSchool, int>) (school => this.m_SpellHitChance[(int) school])).Max();
    }

    /// <summary>Spell avoidance</summary>
    public virtual void ModSpellHitChance(DamageSchool school, int delta)
    {
      if (this.m_SpellHitChance == null)
        this.m_SpellHitChance = Unit.CreateDamageSchoolArr();
      int num = this.m_SpellHitChance[(int) school] + delta;
      this.m_SpellHitChance[(int) school] = num;
    }

    /// <summary>Spell avoidance</summary>
    public void ModSpellHitChance(uint[] schools, int delta)
    {
      foreach (DamageSchool school in schools)
        this.ModSpellHitChance(school, delta);
    }

    /// <summary>Returns the SpellCritChance for the given DamageType</summary>
    public virtual float GetCritChance(DamageSchool school)
    {
      return (float) this.GetCritMod(school);
    }

    public int GetCritMod(DamageSchool school)
    {
      if (this.m_CritMods == null)
        return 0;
      return this.m_CritMods[(int) school];
    }

    public void SetCritMod(DamageSchool school, int value)
    {
      if (this.m_CritMods == null)
        this.m_CritMods = Unit.CreateDamageSchoolArr();
      this.m_CritMods[(uint)school] = value;
      if (!(this is Character))
        return;
      ((Character) this).UpdateSpellCritChance();
    }

    public void ModCritMod(DamageSchool school, int delta)
    {
      if (this.m_CritMods == null)
        this.m_CritMods = Unit.CreateDamageSchoolArr();
      this.m_CritMods[(int) school] += delta;
      if (!(this is Character))
        return;
      ((Character) this).UpdateSpellCritChance();
    }

    public void ModCritMod(DamageSchool[] schools, int delta)
    {
      if (this.m_CritMods == null)
        this.m_CritMods = Unit.CreateDamageSchoolArr();
      foreach (int school in schools)
        this.m_CritMods[school] += delta;
      if (!(this is Character))
        return;
      ((Character) this).UpdateSpellCritChance();
    }

    public void ModCritMod(uint[] schools, int delta)
    {
      if (this.m_CritMods == null)
        this.m_CritMods = Unit.CreateDamageSchoolArr();
      foreach (uint school in schools)
        this.m_CritMods[school] += delta;
      if (!(this is Character))
        return;
      ((Character) this).UpdateSpellCritChance();
    }

    /// <summary>
    /// Returns the damage taken modifier for the given DamageSchool
    /// </summary>
    public int GetDamageTakenMod(DamageSchool school)
    {
      if (this.m_damageTakenMods == null)
        return 0;
      return this.m_damageTakenMods[(int) school];
    }

    public void SetDamageTakenMod(DamageSchool school, int value)
    {
      if (this.m_damageTakenMods == null)
        this.m_damageTakenMods = Unit.CreateDamageSchoolArr();
      this.m_damageTakenMods[(uint)school] = value;
    }

    public void ModDamageTakenMod(DamageSchool school, int delta)
    {
      if (this.m_damageTakenMods == null)
        this.m_damageTakenMods = Unit.CreateDamageSchoolArr();
      this.m_damageTakenMods[(int) school] += delta;
    }

    public void ModDamageTakenMod(DamageSchool[] schools, int delta)
    {
      if (this.m_damageTakenMods == null)
        this.m_damageTakenMods = Unit.CreateDamageSchoolArr();
      foreach (int school in schools)
        this.m_damageTakenMods[school] += delta;
    }

    public void ModDamageTakenMod(uint[] schools, int delta)
    {
      if (this.m_damageTakenMods == null)
        this.m_damageTakenMods = Unit.CreateDamageSchoolArr();
      foreach (uint school in schools)
        this.m_damageTakenMods[school] += delta;
    }

    /// <summary>
    /// Returns the damage taken modifier for the given DamageSchool
    /// </summary>
    public int GetDamageTakenPctMod(DamageSchool school)
    {
      if (this.m_damageTakenPctMods == null)
        return 0;
      return this.m_damageTakenPctMods[(int) school];
    }

    public void SetDamageTakenPctMod(DamageSchool school, int value)
    {
      if (this.m_damageTakenPctMods == null)
        this.m_damageTakenPctMods = Unit.CreateDamageSchoolArr();
      this.m_damageTakenPctMods[(uint)school] = value;
    }

    public void ModDamageTakenPctMod(DamageSchool school, int delta)
    {
      if (this.m_damageTakenPctMods == null)
        this.m_damageTakenPctMods = Unit.CreateDamageSchoolArr();
      this.m_damageTakenPctMods[(int) school] += delta;
    }

    public void ModDamageTakenPctMod(DamageSchool[] schools, int delta)
    {
      if (this.m_damageTakenPctMods == null)
        this.m_damageTakenPctMods = Unit.CreateDamageSchoolArr();
      foreach (int school in schools)
        this.m_damageTakenPctMods[school] += delta;
    }

    public void ModDamageTakenPctMod(uint[] schools, int delta)
    {
      if (this.m_damageTakenPctMods == null)
        this.m_damageTakenPctMods = Unit.CreateDamageSchoolArr();
      foreach (uint school in schools)
        this.m_damageTakenPctMods[school] += delta;
    }

    /// <summary>Threat mod in percent</summary>
    public void ModThreat(DamageSchool school, int delta)
    {
      if (this.m_threatMods == null)
        this.m_threatMods = Unit.CreateDamageSchoolArr();
      int num = this.m_threatMods[(int) school] + delta;
      this.m_threatMods[(int) school] = num;
    }

    /// <summary>Threat mod in percent</summary>
    public void ModThreat(uint[] dmgTypes, int delta)
    {
      foreach (DamageSchool dmgType in dmgTypes)
        this.ModThreat(dmgType, delta);
    }

    public int GetGeneratedThreat(IDamageAction action)
    {
      return this.GetGeneratedThreat(action.ActualDamage, action.UsedSchool, action.SpellEffect);
    }

    /// <summary>Threat mod in percent</summary>
    public virtual int GetGeneratedThreat(int dmg, DamageSchool school, SpellEffect effect)
    {
      if (this.m_threatMods == null)
        return dmg;
      return Math.Max(0, dmg + dmg * this.m_threatMods[(int) school] / 100);
    }

    public int GetAttackerSpellHitChanceMod(DamageSchool school)
    {
      if (this.m_attackerSpellHitChance == null)
        return 0;
      return this.m_attackerSpellHitChance[(int) school];
    }

    /// <summary>Spell avoidance</summary>
    public void ModAttackerSpellHitChance(DamageSchool school, int delta)
    {
      if (this.m_attackerSpellHitChance == null)
        this.m_attackerSpellHitChance = Unit.CreateDamageSchoolArr();
      int num = this.m_attackerSpellHitChance[(int) school] + delta;
      this.m_attackerSpellHitChance[(int) school] = num;
    }

    /// <summary>Spell avoidance</summary>
    public void ModAttackerSpellHitChance(uint[] schools, int delta)
    {
      foreach (DamageSchool school in schools)
        this.ModAttackerSpellHitChance(school, delta);
    }

    /// <summary>
    /// Whether this Character is currently allowed to teleport
    /// </summary>
    public virtual bool MayTeleport
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Teleports the owner to the given position in the current map.
    /// </summary>
    /// <returns>Whether the Zone had a globally unique Site.</returns>
    public void TeleportTo(Vector3 pos)
    {
      this.TeleportTo(this.m_Map, ref pos, new float?());
    }

    /// <summary>
    /// Teleports the owner to the given position in the current map.
    /// </summary>
    /// <returns>Whether the Zone had a globally unique Site.</returns>
    public void TeleportTo(ref Vector3 pos)
    {
      this.TeleportTo(this.m_Map, ref pos, new float?());
    }

    /// <summary>
    /// Teleports the owner to the given position in the current map.
    /// </summary>
    /// <returns>Whether the Zone had a globally unique Site.</returns>
    public void TeleportTo(ref Vector3 pos, float? orientation)
    {
      this.TeleportTo(this.m_Map, ref pos, orientation);
    }

    /// <summary>
    /// Teleports the owner to the given position in the current map.
    /// </summary>
    /// <returns>Whether the Zone had a globally unique Site.</returns>
    public void TeleportTo(Vector3 pos, float? orientation)
    {
      this.TeleportTo(this.m_Map, ref pos, orientation);
    }

    /// <summary>
    /// Teleports the owner to the given position in the given map.
    /// </summary>
    /// <param name="map">the target <see cref="T:WCell.RealmServer.Global.Map" /></param>
    /// <param name="pos">the target <see cref="T:WCell.Util.Graphics.Vector3">position</see></param>
    public void TeleportTo(MapId map, ref Vector3 pos)
    {
      this.TeleportTo(WCell.RealmServer.Global.World.GetNonInstancedMap(map), ref pos, new float?());
    }

    /// <summary>
    /// Teleports the owner to the given position in the given map.
    /// </summary>
    /// <param name="map">the target <see cref="T:WCell.RealmServer.Global.Map" /></param>
    /// <param name="pos">the target <see cref="T:WCell.Util.Graphics.Vector3">position</see></param>
    public void TeleportTo(MapId map, Vector3 pos)
    {
      this.TeleportTo(WCell.RealmServer.Global.World.GetNonInstancedMap(map), ref pos, new float?());
    }

    /// <summary>
    /// Teleports the owner to the given position in the given map.
    /// </summary>
    /// <param name="map">the target <see cref="T:WCell.RealmServer.Global.Map" /></param>
    /// <param name="pos">the target <see cref="T:WCell.Util.Graphics.Vector3">position</see></param>
    public void TeleportTo(Map map, ref Vector3 pos)
    {
      this.TeleportTo(map, ref pos, new float?());
    }

    /// <summary>
    /// Teleports the owner to the given position in the given map.
    /// </summary>
    /// <param name="map">the target <see cref="T:WCell.RealmServer.Global.Map" /></param>
    /// <param name="pos">the target <see cref="T:WCell.Util.Graphics.Vector3">position</see></param>
    public void TeleportTo(Map map, Vector3 pos)
    {
      this.TeleportTo(map, ref pos, new float?());
    }

    /// <summary>Teleports the owner to the given WorldObject.</summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public bool TeleportTo(IWorldLocation location)
    {
      Vector3 position = location.Position;
      Map map = location.Map;
      if (map == null)
      {
        if (this.Map.Id != location.MapId)
          return false;
        map = this.Map;
      }
      this.TeleportTo(map, ref position, new float?(this.m_orientation));
      this.Phase = location.Phase;
      if (location is WorldObject)
        this.Zone = ((WorldObject) location).Zone;
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
      this.TeleportTo(map, ref pos, orientation);
    }

    /// <summary>
    /// Teleports the owner to the given position in the given map.
    /// </summary>
    /// <param name="map">the target <see cref="T:WCell.RealmServer.Global.Map" /></param>
    /// <param name="pos">the target <see cref="T:WCell.Util.Graphics.Vector3">position</see></param>
    /// <param name="orientation">the target orientation</param>
    public void TeleportTo(Map map, ref Vector3 pos, float? orientation)
    {
      Map map1 = this.m_Map;
      if (map.IsDisposed)
        return;
      Character chr = this as Character;
      if (chr != null)
      {
        if (map.MapTemplate.IsDisabled && !chr.GodMode)
        {
          chr.SendInfoMsg(string.Format("Map {0} is disabled.", (object) map.Name));
          return;
        }
        Log.Create(Log.Types.ChangePosition, LogSourceType.Character, chr.EntryId).AddAttribute("source", 0.0, "teleport").AddAttribute(nameof (map), (double) map.Id, map.Id.ToString()).AddAttribute("x", (double) pos.X, "").AddAttribute("y", (double) pos.Y, "").Write();
      }
      this.CancelMovement();
      this.CancelAllActions();
      if (map1 == map)
      {
        if (!this.Map.MoveObject((WorldObject) this, ref pos))
          return;
        if (orientation.HasValue)
          this.Orientation = orientation.Value;
        if (chr == null)
          return;
        chr.IsMoving = false;
        chr.LastPosition = pos;
        this.MovementFlags = MovementFlags.None;
        this.MovementFlags2 = MovementFlags2.None;
        Asda2CharacterHandler.SendResurectResponse(chr);
      }
      else if (map1 != null && !map1.IsInContext)
      {
        Vector3 position = pos;
        map1.AddMessage((IMessage) new Message((Action) (() => this.TeleportTo(map, ref position, orientation))));
      }
      else if (map.TransferObjectLater((WorldObject) this, pos))
      {
        if (orientation.HasValue)
          this.Orientation = orientation.Value;
        if (chr == null)
          return;
        chr.LastPosition = pos;
        this.MovementFlags = MovementFlags.None;
        this.MovementFlags2 = MovementFlags2.None;
      }
      else
        Unit.log.Error("ERROR: Tried to teleport object, but failed to add player to the new map - " + (object) this);
    }

    /// <summary>Count of stealth-modifiers</summary>
    public int Stealthed
    {
      get
      {
        return this.m_stealthed;
      }
      set
      {
        if (this.m_stealthed == value)
          return;
        if (this.m_stealthed > 0 && value <= 0)
          this.StateFlags &= ~StateFlag.Sneaking;
        else if (this.m_stealthed <= 0 && value > 0)
        {
          this.StateFlags |= StateFlag.Sneaking;
          this.Auras.RemoveByFlag(AuraInterruptFlags.OnStealth);
        }
        this.m_stealthed = value;
      }
    }

    public MovementFlags MovementFlags
    {
      get
      {
        return this.m_movementFlags;
      }
      set
      {
        this.m_movementFlags = value;
      }
    }

    public MovementFlags2 MovementFlags2
    {
      get
      {
        return this.m_movementFlags2;
      }
      set
      {
        this.m_movementFlags2 = value;
      }
    }

    public bool IsMovementControlled
    {
      get
      {
        return this.m_Movement != null;
      }
    }

    /// <summary>
    /// Stops this Unit's movement (if it's movement is controlled)
    /// </summary>
    public void StopMoving()
    {
      if (this.m_Movement == null)
        return;
      this.m_Movement.Stop();
    }

    /// <summary>
    /// An object to control this Unit's movement.
    /// Only used for NPCs and posessed Characters.
    /// </summary>
    public WCell.RealmServer.Entities.Movement Movement
    {
      get
      {
        if (this.m_Movement == null)
          this.m_Movement = new WCell.RealmServer.Entities.Movement(this);
        return this.m_Movement;
      }
    }

    /// <summary>Whether the Unit is currently flying</summary>
    public bool IsFlying
    {
      get
      {
        return this.m_flying > 0U;
      }
    }

    /// <summary>Whether the character may walk over water</summary>
    public uint WaterWalk
    {
      get
      {
        return this.m_waterWalk;
      }
      set
      {
        if (this.m_waterWalk == 0U != (value == 0U) && this is Character)
        {
          if (value == 0U)
            MovementHandler.SendWalk((Character) this);
          else
            MovementHandler.SendWaterWalk((Character) this);
        }
        this.m_waterWalk = value;
      }
    }

    /// <summary>Whether a character can fly or not</summary>
    public uint Flying
    {
      get
      {
        return this.m_flying;
      }
      set
      {
        if (this.m_flying == 0U != (value == 0U))
        {
          if (value > 0U)
            this.MovementFlags |= MovementFlags.Flying;
          else
            this.MovementFlags &= MovementFlags.MaskMoving | MovementFlags.PitchUp | MovementFlags.PitchDown | MovementFlags.WalkMode | MovementFlags.OnTransport | MovementFlags.DisableGravity | MovementFlags.Root | MovementFlags.PendingStop | MovementFlags.PendingStrafeStop | MovementFlags.PendingForward | MovementFlags.PendingBackward | MovementFlags.PendingStrafeLeft | MovementFlags.PendingStrafeRight | MovementFlags.PendingRoot | MovementFlags.Swimming | MovementFlags.CanFly | MovementFlags.SplineElevation | MovementFlags.SplineEnabled | MovementFlags.Waterwalking | MovementFlags.CanSafeFall | MovementFlags.Hover | MovementFlags.LocalDirty;
          if (this is Character)
          {
            if (value == 0U)
              MovementHandler.SendFlyModeStop(this);
            else
              MovementHandler.SendFlyModeStart(this);
          }
        }
        this.m_flying = value;
      }
    }

    /// <summary>Whether a character can hover</summary>
    public uint Hovering
    {
      get
      {
        return this.m_hovering;
      }
      set
      {
        if (this.m_hovering == 0U != (value == 0U) && this is Character)
        {
          if (value == 0U)
            MovementHandler.SendHoverModeStop(this);
          else
            MovementHandler.SendHoverModeStart(this);
        }
        this.m_hovering = value;
      }
    }

    /// <summary>Whether a character would take falling damage or not</summary>
    public uint FeatherFalling
    {
      get
      {
        return this.m_featherFalling;
      }
      set
      {
        if (this.m_featherFalling == 0U != (value == 0U) && this is Character)
        {
          if (value == 0U)
            MovementHandler.SendFeatherModeStop(this);
          else
            MovementHandler.SendFeatherModeStart(this);
        }
        this.m_featherFalling = value;
      }
    }

    /// <summary>
    /// The overall-factor for all speeds. Set by the owner's ModifierCollection
    /// </summary>
    public float SpeedFactor
    {
      get
      {
        return this.m_speedFactor;
      }
      set
      {
        if ((double) value == (double) this.m_speedFactor)
          return;
        this.m_speedFactor = value;
        this.WalkSpeed = Unit.DefaultWalkSpeed * this.m_speedFactor;
        this.RunBackSpeed = Unit.DefaultWalkBackSpeed * this.m_speedFactor;
        this.RunSpeed = Unit.DefaultRunSpeed * this.m_speedFactor;
        this.SwimSpeed = Unit.DefaultSwimSpeed * (this.m_speedFactor + this.m_swimFactor);
        this.SwimBackSpeed = Unit.DefaultSwimBackSpeed * (this.m_speedFactor + this.m_swimFactor);
        this.FlightSpeed = Unit.DefaultFlightSpeed * (this.m_speedFactor + this.m_flightFactor);
        this.FlightBackSpeed = Unit.DefaultFlightBackSpeed * (this.m_speedFactor + this.m_flightFactor);
      }
    }

    /// <summary>
    /// The factor for all flying-related speeds. Set by the owner's ModifierCollection
    /// </summary>
    public float FlightSpeedFactor
    {
      get
      {
        return this.m_flightFactor;
      }
      internal set
      {
        if ((double) value == (double) this.m_flightFactor)
          return;
        this.m_flightFactor = value;
        this.FlightSpeed = Unit.DefaultFlightSpeed * (this.m_speedFactor + this.m_flightFactor);
        this.FlightBackSpeed = Unit.DefaultFlightBackSpeed * (this.m_speedFactor + this.m_flightFactor);
      }
    }

    /// <summary>The factor for mounted speed</summary>
    public float MountSpeedMod
    {
      get
      {
        return this.m_mountMod;
      }
      internal set
      {
        if ((double) value == (double) this.m_mountMod)
          return;
        if (this.IsMounted)
          this.SpeedFactor += value - this.m_mountMod;
        this.m_mountMod = value;
      }
    }

    /// <summary>The factor for all swimming-related speeds</summary>
    public float SwimSpeedFactor
    {
      get
      {
        return this.m_swimFactor;
      }
      internal set
      {
        if ((double) value == (double) this.m_swimFactor)
          return;
        this.m_swimFactor = value;
        this.SwimSpeed = Unit.DefaultSwimSpeed * (this.m_speedFactor + this.m_swimFactor);
        this.SwimBackSpeed = Unit.DefaultSwimBackSpeed * (this.m_speedFactor + this.m_swimFactor);
      }
    }

    /// <summary>Forward walking speed.</summary>
    public float WalkSpeed
    {
      get
      {
        return this.m_walkSpeed;
      }
      set
      {
        if ((double) this.m_walkSpeed == (double) value)
          return;
        this.m_walkSpeed = value;
      }
    }

    /// <summary>Backwards walking speed.</summary>
    public float RunBackSpeed
    {
      get
      {
        return this.m_walkBackSpeed;
      }
      set
      {
        if ((double) this.m_walkBackSpeed == (double) value)
          return;
        this.m_walkBackSpeed = value;
        MovementHandler.SendSetRunBackSpeed(this);
      }
    }

    /// <summary>Forward running speed.</summary>
    public float RunSpeed
    {
      get
      {
        return this.m_runSpeed;
      }
      set
      {
        if ((double) this.m_runSpeed == (double) value)
          return;
        this.m_runSpeed = value;
      }
    }

    /// <summary>Forward swimming speed.</summary>
    public float SwimSpeed
    {
      get
      {
        return this.m_swimSpeed;
      }
      set
      {
        if ((double) this.m_swimSpeed == (double) value)
          return;
        this.m_swimSpeed = value;
        MovementHandler.SendSetSwimSpeed(this);
      }
    }

    /// <summary>Backwards swimming speed.</summary>
    public float SwimBackSpeed
    {
      get
      {
        return this.m_swimBackSpeed;
      }
      set
      {
        if ((double) this.m_swimBackSpeed == (double) value)
          return;
        this.m_swimBackSpeed = value;
        MovementHandler.SendSetSwimBackSpeed(this);
      }
    }

    /// <summary>Forward flying speed.</summary>
    public float FlightSpeed
    {
      get
      {
        return this.m_flightSpeed;
      }
      set
      {
        if ((double) this.m_flightSpeed == (double) value)
          return;
        this.m_flightSpeed = value;
        MovementHandler.SendSetFlightSpeed(this);
      }
    }

    /// <summary>Backwards flying speed.</summary>
    public float FlightBackSpeed
    {
      get
      {
        return this.m_flightBackSpeed;
      }
      set
      {
        if ((double) this.m_flightBackSpeed == (double) value)
          return;
        this.m_flightBackSpeed = value;
        MovementHandler.SendSetFlightBackSpeed(this);
      }
    }

    /// <summary>Turning speed.</summary>
    public float TurnSpeed
    {
      get
      {
        return this.m_turnSpeed;
      }
      set
      {
        if ((double) this.m_turnSpeed == (double) value)
          return;
        this.m_turnSpeed = value;
        MovementHandler.SendSetTurnRate(this);
      }
    }

    public float PitchRate
    {
      get
      {
        return this.m_pitchSpeed;
      }
      set
      {
        if ((double) this.m_pitchSpeed == (double) value)
          return;
        this.m_pitchSpeed = value;
        MovementHandler.SendSetPitchRate(this);
      }
    }

    public void ResetMechanicDefaults()
    {
      this.SpeedFactor = 1f;
      this.m_mountMod = 0.0f;
      this.m_flying = this.m_waterWalk = this.m_hovering = this.m_featherFalling = 0U;
      this.m_canMove = this.m_canHarm = this.m_canInteract = this.m_canCastSpells = this.m_canDoPhysicalActivity = true;
      this.m_walkSpeed = Unit.DefaultWalkSpeed;
      this.m_walkBackSpeed = Unit.DefaultWalkBackSpeed;
      this.m_runSpeed = Unit.DefaultRunSpeed;
      this.m_swimSpeed = Unit.DefaultSwimSpeed;
      this.m_swimBackSpeed = Unit.DefaultSwimBackSpeed;
      this.m_flightSpeed = Unit.DefaultFlightSpeed;
      this.m_flightBackSpeed = Unit.DefaultFlightBackSpeed;
      this.m_turnSpeed = Unit.DefaultTurnSpeed;
      this.m_pitchSpeed = Unit.DefaulPitchSpeed;
    }

    public virtual float GetResiliencePct()
    {
      return 0.0f;
    }

    public int AttackerSpellCritChancePercentMod { get; set; }

    public int AttackerPhysicalCritChancePercentMod { get; set; }

    public float IsCollisionWith(Unit unit)
    {
      float num1 = this.Position.X - unit.Position.X;
      float num2 = this.Position.Y - unit.Position.Y;
      float num3 = (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2);
      return this.BoundingRadius + unit.BoundingRadius - num3;
    }

    public override UpdateFlags UpdateFlags
    {
      get
      {
        return UpdateFlags.Flag_0x10 | UpdateFlags.Living | UpdateFlags.StationaryObject;
      }
    }

    public override ObjectTypeId ObjectTypeId
    {
      get
      {
        return ObjectTypeId.Unit;
      }
    }

    public override UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
    {
      if (chr == this.m_master)
        return UpdateFieldFlags.Public | UpdateFieldFlags.OwnerOnly;
      return this.IsAlliedWith((IFactionMember) chr) ? UpdateFieldFlags.Public | UpdateFieldFlags.GroupOnly : UpdateFieldFlags.Public;
    }

    public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
    {
      get
      {
        return UpdateFieldHandler.DynamicUnitHandlers;
      }
    }

    public virtual WorldObject Mover
    {
      get
      {
        return (WorldObject) this;
      }
    }

    protected override void WriteMovementUpdate(PrimitiveWriter packet, UpdateFieldFlags relation)
    {
      this.WriteMovementPacketInfo(packet);
      packet.Write(this.WalkSpeed);
      packet.Write(this.RunSpeed);
      packet.Write(this.RunBackSpeed);
      packet.Write(this.SwimSpeed);
      packet.Write(this.SwimBackSpeed);
      packet.Write(this.FlightSpeed);
      packet.Write(this.FlightBackSpeed);
      packet.Write(this.TurnSpeed);
      packet.Write(this.PitchRate);
      this.MovementFlags.HasFlag((Enum) MovementFlags.SplineEnabled);
    }

    protected override void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation, UpdateFlags updateFlags)
    {
      if (!updateFlags.HasFlag((Enum) UpdateFlags.AttackingTarget))
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
      this.WriteMovementPacketInfo(packet, ref this.m_position, this.m_orientation);
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
      MovementFlags movementFlags = this.MovementFlags;
      MovementFlags2 movementFlags2 = this.MovementFlags2;
      if (movementFlags.HasAnyFlag(MovementFlags.OnTransport) && this.TransportInfo == null)
        movementFlags ^= MovementFlags.OnTransport;
      packet.Write((uint) movementFlags);
      packet.Write((ushort) movementFlags2);
      packet.Write(Utility.GetSystemTime());
      packet.Write(pos.X);
      packet.Write(pos.Y);
      packet.Write(pos.Z);
      packet.Write(orientation);
      if (movementFlags.HasAnyFlag(MovementFlags.OnTransport))
      {
        this.TransportInfo.EntityId.WritePacked((BinaryWriter) packet);
        packet.Write(this.TransportPosition.X);
        packet.Write(this.TransportPosition.Y);
        packet.Write(this.TransportPosition.Z);
        packet.Write(this.TransportOrientation);
        packet.Write(this.TransportTime);
        packet.Write(this.TransportSeat);
      }
      if (movementFlags.HasAnyFlag(MovementFlags.Swimming | MovementFlags.Flying) || movementFlags2.HasFlag((Enum) MovementFlags2.AlwaysAllowPitching))
        packet.Write(this.PitchRate);
      packet.Write(0);
      if (movementFlags.HasAnyFlag(MovementFlags.Falling))
      {
        packet.Write(0.0f);
        packet.Write(8f);
        packet.Write(0.2f);
        packet.Write(1f);
      }
      if (!movementFlags.HasAnyFlag(MovementFlags.SplineElevation))
        return;
      packet.Write(0.0f);
    }

    public void WriteTeleportPacketInfo(RealmPacketOut packet, int param)
    {
      this.EntityId.WritePacked((BinaryWriter) packet);
      packet.Write(param);
      this.WriteMovementPacketInfo((PrimitiveWriter) packet);
    }

    public override void Update(int dt)
    {
      base.Update(dt);
      this.Regenerate(dt);
      if (this.m_brain != null)
        this.m_brain.Update(dt);
      if (this.m_attackTimer != null)
        this.m_attackTimer.Update(dt);
      if (this.m_auras == null)
        return;
      foreach (Aura aura in this.m_auras)
      {
        if (aura != null)
          aura.Update(dt);
      }
    }
  }
}
