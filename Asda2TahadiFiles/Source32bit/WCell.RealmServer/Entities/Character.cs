using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.ArenaTeams;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.Login;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Quests;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Database;
using WCell.Core.Network;
using WCell.Core.Paths;
using WCell.Core.Timers;
using WCell.Intercommunication;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.AI;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Asda2BattleGround;
using WCell.RealmServer.Asda2Fishing;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Asda2Mail;
using WCell.RealmServer.Asda2PetSystem;
using WCell.RealmServer.Asda2Quest;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Auth.Accounts;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Database;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.GameObjects.Handlers;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Help.Tickets;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Mail;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Mounts;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.Paths;
using WCell.RealmServer.PerLevelItemBonuses;
using WCell.RealmServer.Privileges;
using WCell.RealmServer.Quests;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Social;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Talents;
using WCell.RealmServer.Taxi;
using WCell.RealmServer.Titles;
using WCell.RealmServer.Trade;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.Commands;
using WCell.Util.Graphics;
using WCell.Util.NLog;
using WCell.Util.Threading;
using WCell.Util.Variables;

namespace WCell.RealmServer.Entities
{
  /// <summary>
  /// TODO: Move everything Character-related from UnitUpdates in here
  /// </summary>
  /// <summary>
  /// 
  /// </summary>
  /// <summary>
  ///  Represents a unit controlled by a player in the game world
  /// </summary>
  /// <summary>
  /// TODO: Move Update and BroadcastValueUpdate for Character together, since else we sometimes
  /// have to fetch everything in our environment twice in a single map update
  /// </summary>
  public class Character : Unit, IUser, IChatter, INamedEntity, IPacketReceivingEntity, IChatTarget, IContainer,
    IEntity, ITicketHandler, IGenericChatTarget, INamed, IHasRole, IInstanceHolderSet, ICharacterSet, IPacketReceiver
  {
    public static Color GmChatColor = Color.ForestGreen;
    public static readonly List<Character> EmptyArray = new List<Character>();

    /// <summary>The delay until a normal player may logout in millis.</summary>
    [NotVariable]public static int DefaultLogoutDelayMillis = 60000;

    /// <summary>Speed increase when dead and in Ghost form</summary>
    public static float DeathSpeedFactorIncrease = 0.25f;

    /// <summary>
    /// The level at which players start to suffer from repercussion after death
    /// </summary>
    public static int ResurrectionSicknessStartLevel = 10;

    /// <summary>
    /// The factor that is applied to the maximum distance before detecting someone as a SpeedHacker
    /// </summary>
    public static float SpeedHackToleranceFactor = 1.5f;

    [NotVariable]public static int PointsToGetBan = 9999;
    public new static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Player);

    /// <summary>
    /// All objects that are currently visible by this Character.
    /// Don't manipulate this collection.
    /// </summary>
    /// <remarks>Requires map context.</remarks>
    internal HashSet<WorldObject> KnownObjects = WorldObjectSetPool.Obtain();

    /// <summary>
    /// All objects that are currently in BroadcastRadius of this Character.
    /// Don't manipulate this collection.
    /// </summary>
    /// <remarks>Requires map context.</remarks>
    public readonly ICollection<WorldObject> NearbyObjects = new List<WorldObject>();

    protected Battlegrounds.Arenas.ArenaTeamMember[] m_arenaTeamMember = new Battlegrounds.Arenas.ArenaTeamMember[3];

    /// <summary>All languages known to this Character</summary>
    protected internal readonly HashSet<ChatLanguage> KnownLanguages = new HashSet<ChatLanguage>();

    public Dictionary<long, Asda2MailMessage> MailMessages = new Dictionary<long, Asda2MailMessage>();
    public Asda2TeleportingPointRecord[] TeleportPoints = new Asda2TeleportingPointRecord[10];
    public Dictionary<uint, CharacterRecord> Friends = new Dictionary<uint, CharacterRecord>();

    public Dictionary<Asda2ItemCategory, FunctionItemBuff> PremiumBuffs =
      new Dictionary<Asda2ItemCategory, FunctionItemBuff>();

    public FunctionItemBuff[] LongTimePremiumBuffs = new FunctionItemBuff[20];
    public DateTime LastTransportUsedTime = DateTime.MinValue;
    public Dictionary<int, Asda2PetRecord> OwnedPets = new Dictionary<int, Asda2PetRecord>();
    public Dictionary<int, Asda2MountRecord> OwnedMounts = new Dictionary<int, Asda2MountRecord>();
    private int _transportItemId = -1;
    private short _asda2WingsItemId = -1;
    private short _transformationId = -1;
    private int _mountId = -1;
    public Dictionary<byte, Asda2FishingBook> RegisteredFishingBooks = new Dictionary<byte, Asda2FishingBook>();
    public Dictionary<int, byte> AppliedSets = new Dictionary<int, byte>();
    public List<Character> TargetersOnMe = new List<Character>();

    public Dictionary<Asda2PereodicActionType, PereodicAction> PereodicActions =
      new Dictionary<Asda2PereodicActionType, PereodicAction>();

    private HashSet<Item> m_itemsRequiringUpdates = new HashSet<Item>();

    /// <summary>
    /// All Characters that recently were inspecting our inventory
    /// </summary>
    private HashSet<Character> m_observers = new HashSet<Character>();

    /// <summary>
    /// Messages to be processed by the map after updating of the environment (sending of Update deltas etc).
    /// </summary>
    private readonly LockfreeQueue<Action> m_environmentQueue = new LockfreeQueue<Action>();

    public List<Character> EnemyCharacters = new List<Character>();
    public List<Asda2ItemCategory> CategoryBuffsToDelete = new List<Asda2ItemCategory>();
    protected int[] m_dmgBonusVsCreatureTypePct;
    internal int[] m_MeleeAPModByStat;
    internal int[] m_RangedAPModByStat;
    public int EndMoveCount;
    private StandState m_standState;
    private bool _isMoving;
    protected string m_name;
    protected internal CharacterRecord m_record;
    protected TimerEntry m_logoutTimer;
    protected IRealmClient m_client;
    public MoveControl MoveControl;
    private BattlegroundInfo m_bgInfo;
    protected InstanceCollection m_InstanceCollection;
    protected GroupMember m_groupMember;
    protected GroupUpdateFlags m_groupUpdateFlags;
    protected GuildMember m_guildMember;

    /// <summary>All skills of this Character</summary>
    protected SkillCollection m_skills;

    /// <summary>All talents of this Character</summary>
    protected TalentCollection m_talents;

    protected AchievementCollection m_achievements;
    protected PlayerInventory m_inventory;
    protected ReputationCollection m_reputations;
    protected MailAccount m_mailAccount;
    protected Archetype m_archetype;

    /// <summary>The current corpse of this Character or null</summary>
    protected Corpse m_corpse;

    /// <summary>Auto releases Corpse after expiring</summary>
    protected TimerEntry m_corpseReleaseTimer;

    /// <summary>The time when this Character started falling</summary>
    protected int m_fallStart;

    /// <summary>
    /// The Z coordinate of the character when this character character falling
    /// </summary>
    protected float m_fallStartHeight;

    protected DateTime m_swimStart;
    protected float m_swimSurfaceHeight;
    protected DateTime m_lastPlayTimeUpdate;

    /// <summary>
    /// The position at which this Character was last (used for speedhack check)
    /// </summary>
    public Vector3 LastPosition;

    /// <summary>
    /// A bit-mask of the indexes of the TaxiNodes known to this Character in the global TaxiNodeArray
    /// </summary>
    protected TaxiNodeMask m_taxiNodeMask;

    protected IWorldZoneLocation m_bindLocation;
    protected bool m_isLoggingOut;
    protected DateTime m_lastRestUpdate;
    protected AreaTrigger m_restTrigger;
    private List<ChatChannel> m_chatChannels;

    /// <summary>
    /// Set to the ritual, this Character is currently participating in (if any)
    /// </summary>
    protected internal SummoningRitualHandler m_currentRitual;

    protected internal SummonRequest m_summonRequest;
    private Asda2LooterEntry m_looterEntry;
    private ExtraInfo m_ExtraInfo;
    protected TradeWindow m_tradeWindow;
    protected DateTime m_lastPvPUpdateTime;
    private Asda2PlayerInventory _asda2Inventory;
    public int TimeFromLastPositionUpdate;
    private bool _isAsda2TradeDescriptionEnabled;
    private string _asda2TradeDescription;
    private short _asda2FactionRank;
    private bool _saveTaskRunning;

    /// <summary>whether to check for speedhackers</summary>
    public static bool SpeedHackCheck;

    /// <summary>QuestLog of the character</summary>
    private QuestLog m_questLog;

    /// <summary>Auto removes PvP flag after expiring</summary>
    protected TimerEntry PvPEndTime;

    private List<PermanentPetRecord> m_StabledPetRecords;
    private List<SummonedPetRecord> m_SummonedPetRecords;
    private List<GameObject> m_ownedGOs;
    private int m_petBonusTalentPoints;
    protected NPC m_activePet;
    protected NPCCollection m_minions;
    protected NPC[] m_totems;
    private bool _isPlayerLogout;
    protected bool m_initialized;
    private Unit observing;
    public uint LastPetExpGainTime;
    public uint LastPetEatingTime;
    public uint LastSendIamNotMoving;

    /// <summary>Damage bonus vs creature type in %</summary>
    public void ModDmgBonusVsCreatureTypePct(CreatureType type, int delta)
    {
      if(m_dmgBonusVsCreatureTypePct == null)
        m_dmgBonusVsCreatureTypePct = new int[14];
      int num = m_dmgBonusVsCreatureTypePct[(int) type] + delta;
      m_dmgBonusVsCreatureTypePct[(int) type] = num;
    }

    /// <summary>Damage bonus vs creature type in %</summary>
    public void ModDmgBonusVsCreatureTypePct(uint[] creatureTypes, int delta)
    {
      foreach(CreatureType creatureType in creatureTypes)
        ModDmgBonusVsCreatureTypePct(creatureType, delta);
    }

    public int GetMeleeAPModByStat(StatType stat)
    {
      if(m_MeleeAPModByStat == null)
        return 0;
      return m_MeleeAPModByStat[(int) stat];
    }

    public void SetMeleeAPModByStat(StatType stat, int value)
    {
      if(m_MeleeAPModByStat == null)
        m_MeleeAPModByStat = new int[6];
      m_baseStats[(int) stat] = value;
      this.UpdateMeleeAttackPower();
    }

    public void ModMeleeAPModByStat(StatType stat, int delta)
    {
      SetMeleeAPModByStat(stat, GetMeleeAPModByStat(stat) + delta);
    }

    public int GetRangedAPModByStat(StatType stat)
    {
      if(m_RangedAPModByStat == null)
        return 0;
      return m_RangedAPModByStat[(int) stat];
    }

    public void SetRangedAPModByStat(StatType stat, int value)
    {
      if(m_RangedAPModByStat == null)
        m_RangedAPModByStat = new int[6];
      m_baseStats[(int) stat] = value;
      this.UpdateRangedAttackPower();
    }

    public void ModRangedAPModByStat(StatType stat, int delta)
    {
      SetRangedAPModByStat(stat, GetRangedAPModByStat(stat) + delta);
    }

    /// <summary>
    /// Is called whenever the Character moves up or down in water or while flying.
    /// </summary>
    protected internal void MovePitch(float moveAngle)
    {
    }

    /// <summary>Is called whenever the Character falls</summary>
    protected internal void OnFalling()
    {
      if(m_fallStart == 0)
      {
        m_fallStart = Environment.TickCount;
        m_fallStartHeight = m_position.Z;
      }

      if(IsFlying || !IsAlive)
        return;
      int num = GodMode ? 1 : 0;
    }

    public bool IsSwimming
    {
      get { return MovementFlags.HasFlag(MovementFlags.Swimming); }
    }

    public bool IsUnderwater
    {
      get { return m_position.Z < m_swimSurfaceHeight - 0.5; }
    }

    protected internal void OnSwim()
    {
      if(IsSwimming)
        return;
      m_swimStart = DateTime.Now;
    }

    protected internal void OnStopSwimming()
    {
      m_swimSurfaceHeight = -2048f;
    }

    /// <summary>
    /// Is called whenever the Character is moved while on Taxi, Ship, elevator etc
    /// </summary>
    protected internal void MoveTransport(ref Vector4 transportLocation)
    {
      SendSystemMessage("You have been identified as cheater: Faking transport movement!");
    }

    /// <summary>Is called whenever a Character moves</summary>
    public override void OnMove()
    {
      base.OnMove();
      IsFighting = false;
      if(m_standState != StandState.Stand)
        StandState = StandState.Stand;
      if(m_currentRitual != null)
        m_currentRitual.Remove(this);
      if(IsTrading && !IsInRadius(m_tradeWindow.OtherWindow.Owner, TradeMgr.MaxTradeRadius))
        m_tradeWindow.Cancel(TradeStatus.TooFarAway);
      if(CurrentCapturingPoint != null)
        CurrentCapturingPoint.StopCapture();
      LastPosition = MoveControl.Mover.Position;
    }

    public void SetMover(WorldObject mover, bool canControl)
    {
      MoveControl.Mover = mover ?? this;
      MoveControl.CanControl = canControl;
    }

    public void ResetMover()
    {
      MoveControl.Mover = this;
      MoveControl.CanControl = true;
    }

    /// <summary>
    /// Is called whenever a new object appears within vision range of this Character
    /// </summary>
    public void OnEncountered(WorldObject obj)
    {
      if(obj != this)
        obj.OnEncounteredBy(this);
      KnownObjects.Add(obj);
    }

    /// <summary>
    /// Is called whenever an object leaves this Character's sight
    /// </summary>
    public void OnOutOfRange(WorldObject obj)
    {
      --obj.AreaCharCount;
      if(obj == Asda2DuelingOponent && Asda2Duel != null)
        Asda2Duel.StopPvp();
      if(obj == m_target)
        ClearTarget();
      if(obj == m_activePet)
        ActivePet = null;
      if(GossipConversation != null && obj == GossipConversation.Speaker && GossipConversation.Character == this)
        GossipConversation.Dispose();
      if(!(obj is Transport))
        KnownObjects.Remove(obj);
      Character chr = obj as Character;
      if(chr != null)
      {
        if(EnemyCharacters.Contains(chr))
        {
          EnemyCharacters.Remove(chr);
          CheckEnemysCount();
        }

        GlobalHandler.SendCharacterDeleteResponse(chr, Client);
      }
      else
      {
        Asda2Loot loot = obj as Asda2Loot;
        if(loot == null)
          return;
        GlobalHandler.SendRemoveLootResponse(this, loot);
      }
    }

    public void CheckEnemysCount()
    {
      if(EnemyCharacters.Count != 0 || IsAsda2BattlegroundInProgress)
        return;
      GlobalHandler.SendFightingModeChangedResponse(Client, SessionId, (int) AccId, -1);
    }

    /// <summary>
    /// Is called whenever this Character was added to a new map
    /// </summary>
    protected internal override void OnEnterMap()
    {
      base.OnEnterMap();
      if(!_saveTaskRunning)
      {
        _saveTaskRunning = true;
        ServerApp<RealmServer>.IOQueue.CallDelayed(CharacterFormulas.SaveChateterInterval, SaveCharacter);
      }

      ClearSelfKnowledge();
      m_lastMoveTime = Environment.TickCount;
      LastPosition = m_position;
      AddPostUpdateMessage(() =>
      {
        if(m_zone == null)
          return;
        int num = m_zone.Template.IsPvP ? 1 : 0;
      });
      if(!IsPetActive)
        return;
      IsPetActive = true;
    }

    protected internal override void OnLeavingMap()
    {
      if(m_activePet != null && m_activePet.IsInWorld)
        m_activePet.Map.RemoveObject(m_activePet);
      if(m_minions != null)
      {
        foreach(WorldObject minion in m_minions)
          minion.Delete();
      }

      base.OnLeavingMap();
    }

    /// <summary>
    /// Changes the character's stand state and notifies the client.
    /// </summary>
    public override StandState StandState
    {
      get { return m_standState; }
      set
      {
        if(value == StandState)
          return;
        m_standState = value;
        base.StandState = value;
        if(m_looterEntry != null && m_looterEntry.Loot != null &&
           (value != StandState.Kneeling && m_looterEntry.Loot.MustKneelWhileLooting))
          CancelLooting();
        if(value != StandState.Stand)
          return;
        m_auras.RemoveByFlag(AuraInterruptFlags.OnStandUp);
      }
    }

    protected override void OnResistanceChanged(DamageSchool school)
    {
      base.OnResistanceChanged(school);
      if(m_activePet == null || !m_activePet.IsHunterPet)
        return;
      m_activePet.UpdatePetResistance(school);
    }

    public override void ModSpellHitChance(DamageSchool school, int delta)
    {
      base.ModSpellHitChance(school, delta);
      if(m_activePet == null)
        return;
      m_activePet.ModSpellHitChance(school, delta);
    }

    public override float GetResiliencePct()
    {
      return 0.0f;
    }

    public override void DealEnvironmentalDamage(EnviromentalDamageType dmgType, int amount)
    {
      base.DealEnvironmentalDamage(dmgType, amount);
      if(IsAlive)
        return;
      Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DeathsFrom, (uint) dmgType, 1U, null);
    }

    public new bool IsMoving
    {
      get { return _isMoving; }
      set
      {
        _isMoving = value;
        if(!value)
          return;
        OnMove();
      }
    }

    public BaseRelation GetRelationTo(Character chr, CharacterRelationType type)
    {
      return Singleton<RelationMgr>.Instance.GetRelation(EntityId.Low, chr.EntityId.Low, type);
    }

    /// <summary>
    /// Returns whether this Character ignores the Character with the given low EntityId.
    /// </summary>
    /// <returns></returns>
    public bool IsIgnoring(IUser user)
    {
      return Singleton<RelationMgr>.Instance.HasRelation(EntityId.Low, user.EntityId.Low,
        CharacterRelationType.Ignored);
    }

    /// <summary>
    /// Indicates whether the two Characters are in the same <see cref="P:WCell.RealmServer.Entities.Character.Group" />
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public bool IsAlliedWith(Character chr)
    {
      if(m_groupMember != null && chr.m_groupMember != null)
        return m_groupMember.Group == chr.m_groupMember.Group;
      return false;
    }

    /// <summary>
    /// Binds Character to start position if none other is set
    /// </summary>
    private void CheckBindLocation()
    {
      if(m_bindLocation.IsValid())
        return;
      BindTo(this, m_archetype.StartLocation);
    }

    public void TeleportToBindLocation()
    {
      TeleportTo(BindLocation);
    }

    public bool CanFly
    {
      get
      {
        if(!m_Map.CanFly || m_zone != null &&
           (!m_zone.Flags.HasFlag(ZoneFlags.CanFly) || m_zone.Flags.HasFlag(ZoneFlags.CannotFly)))
          return Role.IsStaff;
        return true;
      }
    }

    public override void Mount(uint displayId)
    {
      if(m_activePet != null)
        m_activePet.RemoveFromMap();
      base.Mount(displayId);
    }

    protected internal override void DoDismount()
    {
      if(IsPetActive)
        PlaceOnTop(ActivePet);
      base.DoDismount();
    }

    public int GetRandomMagicDamage()
    {
      return Utility.Random(MinMagicDamage, MaxMagicDamage);
    }

    public float GetRandomPhysicalDamage()
    {
      return Utility.Random(MinDamage, MaxDamage);
    }

    public byte RealProffLevel
    {
      get
      {
        if(Class == ClassId.THS || Class == ClassId.OHS || (Class == ClassId.Spear || Class == ClassId.NoClass))
          return ProfessionLevel;
        if(Class == ClassId.AtackMage || Class == ClassId.SupportMage || Class == ClassId.HealMage)
          return (byte) (ProfessionLevel - 22U);
        if(Class == ClassId.Bow || Class == ClassId.Crossbow || Class == ClassId.Balista)
          return (byte) (ProfessionLevel - 11U);
        return 0;
      }
    }

    public Asda2PetRecord AddAsda2Pet(PetTemplate petTemplate, bool silent = false)
    {
      Asda2PetRecord pet = new Asda2PetRecord(petTemplate, this);
      pet.Create();
      OwnedPets.Add(pet.Guid, pet);
      if(!silent)
        Asda2PetHandler.SendInitPetInfoOnLoginResponse(Client, pet);
      return pet;
    }

    Locale IPacketReceiver.Locale { get; set; }

    protected internal override void UpdateStamina()
    {
      base.UpdateStamina();
      if(m_MeleeAPModByStat != null)
        this.UpdateAllAttackPower();
      if(m_activePet == null || !m_activePet.IsHunterPet)
        return;
      m_activePet.UpdateStamina();
    }

    protected internal override void UpdateIntellect()
    {
      base.UpdateIntellect();
      if(PowerType == PowerType.Mana)
        this.UpdateSpellCritChance();
      this.UpdatePowerRegen();
      if(m_MeleeAPModByStat == null)
        return;
      this.UpdateAllAttackPower();
    }

    protected internal override void UpdateSpirit()
    {
      base.UpdateSpirit();
      if(m_MeleeAPModByStat == null)
        return;
      this.UpdateAllAttackPower();
    }

    protected internal override int IntellectManaBonus
    {
      get
      {
        int intellect = Archetype.FirstLevelStats.Intellect;
        return intellect + (Intellect - intellect) * ManaPerIntelligence;
      }
    }

    public int RegenHealth
    {
      get
      {
        int health = Health;
        if(PereodicActions.ContainsKey(Asda2PereodicActionType.HpRegen))
          health += PereodicActions[Asda2PereodicActionType.HpRegen].RemainingHeal;
        if(PereodicActions.ContainsKey(Asda2PereodicActionType.HpRegenPrc))
          health += PereodicActions[Asda2PereodicActionType.HpRegenPrc].RemainingHeal * MaxHealth / 100;
        if(health <= MaxHealth)
          return health;
        return MaxHealth;
      }
    }

    public int RegenMana
    {
      get
      {
        int power = Power;
        if(PereodicActions.ContainsKey(Asda2PereodicActionType.MpRegen))
          power += PereodicActions[Asda2PereodicActionType.MpRegen].RemainingHeal;
        if(power <= MaxPower)
          return power;
        return MaxPower;
      }
    }

    private void UpdateChancesByCombatRating(CombatRating rating)
    {
      switch(rating)
      {
        case CombatRating.DefenseSkill:
          this.UpdateDefense();
          break;
        case CombatRating.Dodge:
          this.UpdateDodgeChance();
          break;
        case CombatRating.Parry:
          this.UpdateParryChance();
          break;
        case CombatRating.Block:
          this.UpdateBlockChance();
          break;
        case CombatRating.MeleeHitChance:
          this.UpdateMeleeHitChance();
          break;
        case CombatRating.RangedHitChance:
          this.UpdateRangedHitChance();
          break;
        case CombatRating.MeleeCritChance:
          this.UpdateCritChance();
          break;
        case CombatRating.RangedCritChance:
          this.UpdateCritChance();
          break;
        case CombatRating.SpellCritChance:
          this.UpdateSpellCritChance();
          break;
        case CombatRating.Expertise:
          this.UpdateExpertise();
          break;
      }
    }

    public int LastExpLooseAmount { get; set; }

    /// <summary>
    /// Contains certain info that is almost only used by staff and should usually not be available to normal players.
    /// <remarks>Guaranteed to be non-null</remarks>
    /// </summary>
    public ExtraInfo ExtraInfo
    {
      get
      {
        if(m_ExtraInfo == null)
          m_ExtraInfo = new ExtraInfo(this);
        return m_ExtraInfo;
      }
    }

    public int TransportItemId
    {
      get { return _transportItemId; }
      set
      {
        if(_transportItemId == value)
          return;
        if(value == -1 || _transportItemId != -1)
        {
          FunctionalItemsHandler.SendCancelCancelFunctionalItemResponse(Client, (short) _transportItemId);
          this.ChangeModifier(StatModifierFloat.Speed, -Asda2ItemMgr.GetTemplate(_transportItemId).ValueOnUse / 100f);
        }

        if(value != -1)
        {
          FunctionalItemsHandler.SendShopItemUsedResponse(Client, value, -1);
          this.ChangeModifier(StatModifierFloat.Speed, Asda2ItemMgr.GetTemplate(value).ValueOnUse / 100f);
        }

        _transportItemId = value;
      }
    }

    public int MountId
    {
      get { return _mountId; }
      set
      {
        if(_mountId == value)
          return;
        if(value == -1)
        {
          this.ChangeModifier(StatModifierFloat.Speed,
            -(Asda2MountMgr.TemplatesById[MountId].Unk + IntMods[43]) / 100f);
          _mountId = value;
          Asda2MountHandler.SendVeicheStatusChangedResponse(this, Asda2MountHandler.MountStatusChanged.Unsumon);
          Asda2MountHandler.SendCharacterOnMountStatusChangedResponse(this, Asda2MountHandler.UseMountStatus.Ok);
        }
        else if(LastTransportUsedTime.AddSeconds(30.0) > DateTime.Now)
        {
          Asda2MountHandler.SendCharacterOnMountStatusChangedResponse(this, Asda2MountHandler.UseMountStatus.Fail);
          SendInfoMsg("Mount is on cooldown.");
        }
        else
        {
          _mountId = value;
          Asda2MountHandler.SendVeicheStatusChangedResponse(this, Asda2MountHandler.MountStatusChanged.Summoned);
          Asda2MountHandler.SendCharacterOnMountStatusChangedResponse(this, Asda2MountHandler.UseMountStatus.Ok);
          this.ChangeModifier(StatModifierFloat.Speed, (Asda2MountMgr.TemplatesById[value].Unk + IntMods[43]) / 100f);
          LastTransportUsedTime = DateTime.Now;
        }
      }
    }

    public int ApplyFunctionItemBuff(int itemId, bool isLongTimeBuff = false)
    {
      int num = 0;
      Asda2ItemTemplate templ = Asda2ItemMgr.GetTemplate(itemId);
      if(isLongTimeBuff)
      {
        if(LongTimePremiumBuffs.Contains(l =>
        {
          if(l != null)
            return l.Template.Category == templ.Category;
          return false;
        }))
          throw new AlreadyBuffedExcepton();
        FunctionItemBuff functionItemBuff = new FunctionItemBuff(itemId, this)
        {
          IsLongTime = true
        };
        functionItemBuff.EndsDate =
          DateTime.Now.AddDays(functionItemBuff.Template.AttackTime == 0 ? 7.0 : functionItemBuff.Template.AttackTime);
        functionItemBuff.CreateLater();
        num = LongTimePremiumBuffs.AddElement(functionItemBuff);
        ProcessFunctionalItemEffect(functionItemBuff, true);
      }
      else if(PremiumBuffs.ContainsKey(templ.Category))
      {
        FunctionItemBuff premiumBuff = PremiumBuffs[templ.Category];
        premiumBuff.Duration = premiumBuff.Template.AtackRange * 1000;
        if(premiumBuff.Stacks >= premiumBuff.Template.MaxDurability)
          throw new AlreadyBuffedExcepton();
        ProcessFunctionalItemEffect(premiumBuff, false);
        ++premiumBuff.Stacks;
        ProcessFunctionalItemEffect(premiumBuff, true);
      }
      else
      {
        FunctionItemBuff record = new FunctionItemBuff(itemId, this);
        record.Duration = record.Template.AtackRange * 1000;
        record.CreateLater();
        PremiumBuffs.Add(templ.Category, record);
        ProcessFunctionalItemEffect(record, true);
      }

      return num;
    }

    private void ProcessFunctionalItemEffect(FunctionItemBuff item, bool isPositive)
    {
      int delta = (isPositive ? item.Template.ValueOnUse : -item.Template.ValueOnUse) * item.Stacks;
      switch(item.Template.Category)
      {
        case Asda2ItemCategory.IncPAtk:
          this.ChangeModifier(StatModifierFloat.Damage, delta / 100f);
          break;
        case Asda2ItemCategory.IncMAtk:
          this.ChangeModifier(StatModifierFloat.MagicDamage, delta / 100f);
          break;
        case Asda2ItemCategory.IncPDef:
          this.ChangeModifier(StatModifierFloat.Asda2Defence, delta / 100f);
          break;
        case Asda2ItemCategory.IncMdef:
          this.ChangeModifier(StatModifierFloat.Asda2MagicDefence, delta / 100f);
          break;
        case Asda2ItemCategory.IncHp:
          MaxHealthModScalar += delta / 100f;
          this.ChangeModifier(StatModifierFloat.Health, delta / 100f);
          if(isPositive)
          {
            Health += (MaxHealth * delta + 50) / 100;
          }

          break;
        case Asda2ItemCategory.IncMp:
          this.ChangeModifier(StatModifierInt.PowerPct, delta);
          break;
        case Asda2ItemCategory.IncStr:
          this.ChangeModifier(StatModifierFloat.Strength, delta / 100f);
          break;
        case Asda2ItemCategory.IncSta:
          this.ChangeModifier(StatModifierFloat.Stamina, delta / 100f);
          break;
        case Asda2ItemCategory.IncInt:
          this.ChangeModifier(StatModifierFloat.Intelect, delta / 100f);
          break;
        case Asda2ItemCategory.IncSpi:
          this.ChangeModifier(StatModifierFloat.Spirit, delta / 100f);
          break;
        case Asda2ItemCategory.IncDex:
          this.ChangeModifier(StatModifierFloat.Agility, delta / 100f);
          break;
        case Asda2ItemCategory.IncLuck:
          this.ChangeModifier(StatModifierFloat.Luck, delta / 100f);
          break;
        case Asda2ItemCategory.IncMoveSpeed:
          this.ChangeModifier(StatModifierFloat.Speed, delta / 100f);
          break;
        case Asda2ItemCategory.IncExp:
          this.ChangeModifier(StatModifierFloat.Asda2ExpAmount, delta / 100f);
          break;
        case Asda2ItemCategory.IncDropChance:
          this.ChangeModifier(StatModifierFloat.Asda2DropChance, delta / 100f);
          break;
        case Asda2ItemCategory.IncDigChance:
          this.ChangeModifier(StatModifierFloat.DigChance, delta / 100f);
          break;
        case Asda2ItemCategory.IncExpStackable:
          this.ChangeModifier(StatModifierFloat.Asda2ExpAmount, delta / 100f);
          break;
        case Asda2ItemCategory.IncAtackSpeed:
          this.ChangeModifier(StatModifierFloat.MeleeAttackTime, -delta / 100f);
          break;
        case Asda2ItemCategory.ShopBanner:
          EliteShopBannerEnabled = isPositive;
          break;
        case Asda2ItemCategory.PremiumPotions:
          this.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float) ((isPositive ? 1.0 : -1.0) * 20.0 / 100.0));
          this.ChangeModifier(StatModifierFloat.Asda2DropChance, (float) ((isPositive ? 1.0 : -1.0) * 20.0 / 100.0));
          this.ChangeModifier(StatModifierFloat.Health, (float) ((isPositive ? 1.0 : -1.0) * 10.0 / 100.0));
          this.ChangeModifier(StatModifierInt.PowerPct, (isPositive ? 1 : -1) * 10);
          this.ChangeModifier(StatModifierFloat.Speed, (float) ((isPositive ? 1.0 : -1.0) * 25.0 / 100.0));
          this.ChangeModifier(StatModifierFloat.Damage, (float) ((isPositive ? 1.0 : -1.0) * 10.0 / 100.0));
          this.ChangeModifier(StatModifierFloat.MagicDamage, (float) ((isPositive ? 1.0 : -1.0) * 10.0 / 100.0));
          Asda2WingsItemId = isPositive ? (short) item.Template.Id : (short) -1;
          break;
        case Asda2ItemCategory.ExpandInventory:
          InventoryExpanded = isPositive;
          break;
        case Asda2ItemCategory.PetNotEatingByDays:
          PetNotHungerEnabled = isPositive;
          break;
        case Asda2ItemCategory.RemoveDeathPenaltiesByDays:
          RemoveDeathPenalties = isPositive;
          break;
      }

      if(isPositive)
        return;
      FunctionalItemsHandler.SendCancelCancelFunctionalItemResponse(Client, (short) item.ItemId);
    }

    public bool PetNotHungerEnabled { get; set; }

    public bool EliteShopBannerEnabled { get; set; }

    public bool RemoveDeathPenalties { get; set; }

    public bool InventoryExpanded { get; set; }

    public bool IsOnTransport
    {
      get { return TransportItemId != -1; }
    }

    public Vector2 CurrentMovingVector { get; set; }

    public override int GetUnmodifiedBaseStatValue(StatType stat)
    {
      if((byte) stat >= ClassBaseStats.Stats.Length)
        return 0;
      return ClassBaseStats.Stats[(int) stat];
    }

    public override bool IsPlayer
    {
      get { return true; }
    }

    public override bool MayTeleport
    {
      get
      {
        if(Role.IsStaff)
          return true;
        if(!IsKicked && CanMove)
          return IsPlayerControlled;
        return false;
      }
    }

    public override WorldObject Mover
    {
      get { return MoveControl.Mover; }
    }

    public byte[] PlayerBytes
    {
      get { return GetByteArray(PlayerFields.BYTES); }
      set { SetByteArray(PlayerFields.BYTES, value); }
    }

    public byte Skin
    {
      get { return GetByte(PlayerFields.BYTES, 0); }
      set { SetByte(PlayerFields.BYTES, 0, value); }
    }

    public byte Facial
    {
      get { return Record.Face; }
      set { Record.Face = value; }
    }

    public byte HairStyle
    {
      get { return GetByte(PlayerFields.BYTES, 2); }
      set { SetByte(PlayerFields.BYTES, 2, value); }
    }

    public byte HairColor
    {
      get { return GetByte(PlayerFields.BYTES, 3); }
      set { SetByte(PlayerFields.BYTES, 3, value); }
    }

    public byte[] PlayerBytes2
    {
      get { return GetByteArray(PlayerFields.BYTES_2); }
      set { SetByteArray(PlayerFields.BYTES_2, value); }
    }

    public byte FacialHair
    {
      get { return GetByte(PlayerFields.BYTES_2, 0); }
      set { SetByte(PlayerFields.BYTES_2, 0, value); }
    }

    /// <summary>0x10 for SpellSteal</summary>
    public byte PlayerBytes2_2
    {
      get { return GetByte(PlayerFields.BYTES_2, 1); }
      set { SetByte(PlayerFields.BYTES_2, 1, value); }
    }

    /// <summary>
    /// Use player.Inventory.BankBags.Inc/DecBagSlots() to change the amount of cont slots in use
    /// </summary>
    public byte BankBagSlots
    {
      get { return GetByte(PlayerFields.BYTES_2, 2); }
      internal set { SetByte(PlayerFields.BYTES_2, 2, value); }
    }

    /// <summary>
    /// 0x01 -&gt; Rested State
    /// 0x02 -&gt; Normal State
    /// </summary>
    public RestState RestState
    {
      get { return (RestState) GetByte(PlayerFields.BYTES_2, 3); }
      set { SetByte(PlayerFields.BYTES_2, 3, (byte) value); }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsResting
    {
      get { return m_restTrigger != null; }
    }

    /// <summary>
    /// The AreaTrigger that triggered the current Rest-state (or null if not resting).
    /// Will automatically be set when the Character enters a Rest-Type AreaTrigger
    /// and will be unset once the Character is too far away from that trigger.
    /// </summary>
    public AreaTrigger RestTrigger
    {
      get { return m_restTrigger; }
      set
      {
        if(m_restTrigger == value)
          return;
        if(value == null)
        {
          UpdateRest();
          m_record.RestTriggerId = 0;
          RestState = RestState.Normal;
        }
        else
        {
          m_lastRestUpdate = DateTime.Now;
          m_record.RestTriggerId = (int) value.Id;
          RestState = RestState.Resting;
        }

        m_restTrigger = value;
      }
    }

    public byte[] PlayerBytes3
    {
      get { return GetByteArray(PlayerFields.BYTES_3); }
      set { SetByteArray(PlayerFields.BYTES_3, value); }
    }

    public override GenderType Gender
    {
      get { return (GenderType) GetByte(PlayerFields.BYTES_3, 0); }
      set
      {
        SetByte(PlayerFields.BYTES_3, 0, (byte) value);
        base.Gender = value;
      }
    }

    /// <summary>
    /// 100 Totally smashed
    /// 60 Drunk
    /// 30 Tipsy
    /// </summary>
    public byte DrunkState
    {
      get { return GetByte(PlayerFields.BYTES_3, 1); }
      set
      {
        if(value > 100)
          value = 100;
        SetByte(PlayerFields.BYTES_3, 1, value);
      }
    }

    public byte PlayerBytes3_3
    {
      get { return GetByte(PlayerFields.BYTES_3, 2); }
      set { SetByte(PlayerFields.BYTES_3, 2, value); }
    }

    public byte PvPRank
    {
      get { return GetByte(PlayerFields.BYTES_3, 3); }
      set { SetByte(PlayerFields.BYTES_3, 3, value); }
    }

    /// <summary>BYTES</summary>
    public byte[] Bytes
    {
      get { return GetByteArray(PlayerFields.PLAYER_FIELD_BYTES); }
      set { SetByteArray(PlayerFields.PLAYER_FIELD_BYTES, value); }
    }

    /// <summary>
    /// 
    /// </summary>
    public CorpseReleaseFlags CorpseReleaseFlags
    {
      get { return (CorpseReleaseFlags) GetByte(PlayerFields.PLAYER_FIELD_BYTES, 0); }
      set { SetByte(PlayerFields.PLAYER_FIELD_BYTES, 0, (byte) value); }
    }

    public byte Bytes_2
    {
      get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES, 1); }
      set { SetByte(PlayerFields.PLAYER_FIELD_BYTES, 1, value); }
    }

    public byte ActionBarMask
    {
      get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES, 2); }
      set { SetByte(PlayerFields.PLAYER_FIELD_BYTES, 2, value); }
    }

    public byte Bytes_4
    {
      get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES, 3); }
      set { SetByte(PlayerFields.PLAYER_FIELD_BYTES, 3, value); }
    }

    public byte[] Bytes2
    {
      get { return GetByteArray(PlayerFields.PLAYER_FIELD_BYTES2); }
      set { SetByteArray(PlayerFields.PLAYER_FIELD_BYTES2, value); }
    }

    public byte Bytes2_1
    {
      get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES2, 0); }
      set { SetByte(PlayerFields.PLAYER_FIELD_BYTES2, 0, value); }
    }

    /// <summary>Set to 0x40 for mage invis</summary>
    public byte Bytes2_2
    {
      get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES2, 1); }
      set { SetByte(PlayerFields.PLAYER_FIELD_BYTES2, 1, value); }
    }

    public byte Bytes2_3
    {
      get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES2, 2); }
      set { SetByte(PlayerFields.PLAYER_FIELD_BYTES2, 2, value); }
    }

    public byte Bytes2_4
    {
      get { return GetByte(PlayerFields.PLAYER_FIELD_BYTES2, 3); }
      set { SetByte(PlayerFields.PLAYER_FIELD_BYTES2, 3, value); }
    }

    public PlayerFlags PlayerFlags
    {
      get { return (PlayerFlags) GetInt32(PlayerFields.FLAGS); }
      set { SetUInt32(PlayerFields.FLAGS, (uint) value); }
    }

    public int Experience
    {
      get { return GetInt32(PlayerFields.XP); }
      set { SetInt32(PlayerFields.XP, value); }
    }

    public int NextLevelXP
    {
      get { return GetInt32(PlayerFields.NEXT_LEVEL_XP); }
      set { SetInt32(PlayerFields.NEXT_LEVEL_XP, value); }
    }

    /// <summary>
    /// The amount of experience to be gained extra due to resting
    /// </summary>
    public int RestXp
    {
      get { return GetInt32(PlayerFields.REST_STATE_EXPERIENCE); }
      set { SetInt32(PlayerFields.REST_STATE_EXPERIENCE, value); }
    }

    public uint Money
    {
      get { return (uint) Record.Money; }
      set { Record.Money = value; }
    }

    public void SendMoneyUpdate()
    {
      if(Map == null)
        return;
      Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Ok, Asda2Inventory.RegularItems[0], this);
    }

    /// <summary>Adds the given amount of money</summary>
    public void AddMoney(uint amount)
    {
      Log.Create(Log.Types.ItemOperations, LogSourceType.Character, EntryId).AddAttribute("source", 0.0, "add_money")
        .AddAttribute("current", Money, "").AddAttribute("diff", amount, "").Write();
      Money += amount;
    }

    /// <summary>
    /// Subtracts the given amount of Money. Returns false if its more than this Character has.
    /// </summary>
    public bool SubtractMoney(uint amount)
    {
      Log.Create(Log.Types.ItemOperations, LogSourceType.Character, EntryId)
        .AddAttribute("source", 0.0, "substract_money").AddAttribute("current", Money, "")
        .AddAttribute("diff", amount, "").Write();
      uint money = Money;
      if(amount > money)
        return false;
      Money -= amount;
      return true;
    }

    /// <summary>
    /// Set to <value>-1</value> to disable the watched faction
    /// </summary>
    public int WatchedFaction
    {
      get { return GetInt32(PlayerFields.WATCHED_FACTION_INDEX); }
      set { SetInt32(PlayerFields.WATCHED_FACTION_INDEX, value); }
    }

    public TitleBitId ChosenTitle
    {
      get { return (TitleBitId) GetUInt32(PlayerFields.CHOSEN_TITLE); }
      set { SetUInt32(PlayerFields.CHOSEN_TITLE, (uint) value); }
    }

    public CharTitlesMask KnownTitleMask
    {
      get { return (CharTitlesMask) GetUInt64(PlayerFields._FIELD_KNOWN_TITLES); }
      set { SetUInt64(PlayerFields._FIELD_KNOWN_TITLES, (ulong) value); }
    }

    public ulong KnownTitleMask2
    {
      get { return GetUInt64(PlayerFields._FIELD_KNOWN_TITLES1); }
      set { SetUInt64(PlayerFields._FIELD_KNOWN_TITLES1, value); }
    }

    public ulong KnownTitleMask3
    {
      get { return GetUInt64(PlayerFields._FIELD_KNOWN_TITLES2); }
      set { SetUInt64(PlayerFields._FIELD_KNOWN_TITLES2, value); }
    }

    public uint KillsTotal
    {
      get { return GetUInt32(PlayerFields.KILLS); }
      set { SetUInt32(PlayerFields.KILLS, value); }
    }

    public ushort KillsToday
    {
      get { return GetUInt16Low(PlayerFields.KILLS); }
      set { SetUInt16Low(PlayerFields.KILLS, value); }
    }

    public ushort KillsYesterday
    {
      get { return GetUInt16High(PlayerFields.KILLS); }
      set { SetUInt16High(PlayerFields.KILLS, value); }
    }

    public uint HonorToday
    {
      get { return GetUInt32(PlayerFields.TODAY_CONTRIBUTION); }
      set { SetUInt32(PlayerFields.TODAY_CONTRIBUTION, value); }
    }

    public uint HonorYesterday
    {
      get { return GetUInt32(PlayerFields.YESTERDAY_CONTRIBUTION); }
      set { SetUInt32(PlayerFields.YESTERDAY_CONTRIBUTION, value); }
    }

    public uint LifetimeHonorableKills
    {
      get { return GetUInt32(PlayerFields.LIFETIME_HONORBALE_KILLS); }
      set { SetUInt32(PlayerFields.LIFETIME_HONORBALE_KILLS, value); }
    }

    public uint HonorPoints
    {
      get { return GetUInt32(PlayerFields.HONOR_CURRENCY); }
      set { SetUInt32(PlayerFields.HONOR_CURRENCY, value); }
    }

    public uint ArenaPoints
    {
      get { return GetUInt32(PlayerFields.ARENA_CURRENCY); }
      set { SetUInt32(PlayerFields.ARENA_CURRENCY, value); }
    }

    public uint GuildId
    {
      get { return GetUInt32(PlayerFields.GUILDID); }
      internal set { SetUInt32(PlayerFields.GUILDID, value); }
    }

    public uint GuildRank
    {
      get { return GetUInt32(PlayerFields.GUILDRANK); }
      internal set { SetUInt32(PlayerFields.GUILDRANK, value); }
    }

    public void SetArenaTeamInfoField(ArenaTeamSlot slot, ArenaTeamInfoType type, uint value)
    {
      SetUInt32((int) (1256 + (int) slot * 7 + type), value);
    }

    /// <summary>
    /// The 3 classmasks of spells to not use require reagents for
    /// </summary>
    public uint[] NoReagentCost
    {
      get
      {
        return new uint[3]
        {
          GetUInt32(PlayerFields.NO_REAGENT_COST_1),
          GetUInt32(PlayerFields.NO_REAGENT_COST_1_2),
          GetUInt32(PlayerFields.NO_REAGENT_COST_1_3)
        };
      }
      internal set
      {
        SetUInt32(PlayerFields.NO_REAGENT_COST_1, value[0]);
        SetUInt32(PlayerFields.NO_REAGENT_COST_1_2, value[1]);
        SetUInt32(PlayerFields.NO_REAGENT_COST_1_3, value[2]);
      }
    }

    public override Faction DefaultFaction
    {
      get { return FactionMgr.Get(Race); }
    }

    public byte CharNum
    {
      get { return Record.CharNum; }
    }

    public uint UniqId
    {
      get { return (uint) (Account.AccountId + 1000000 * CharNum); }
    }

    public int ReputationGainModifierPercent { get; set; }

    public int KillExperienceGainModifierPercent { get; set; }

    public int QuestExperienceGainModifierPercent
    {
      get { return 0; }
      set { QuestExperienceGainModifierPercent = value; }
    }

    /// <summary>
    /// Gets the total modifier of the corresponding CombatRating (in %)
    /// </summary>
    public int GetCombatRating(CombatRating rating)
    {
      return GetInt32((PlayerFields) (1230U + rating));
    }

    public void SetCombatRating(CombatRating rating, int value)
    {
      SetInt32((PlayerFields) (1230U + rating), value);
      UpdateChancesByCombatRating(rating);
    }

    /// <summary>
    /// Modifies the given CombatRating modifier by the given delta
    /// </summary>
    public void ModCombatRating(CombatRating rating, int delta)
    {
      int num = GetInt32((PlayerFields) (1230U + rating)) + delta;
      SetInt32((PlayerFields) (1230U + rating), num);
      UpdateChancesByCombatRating(rating);
    }

    public void ModCombatRating(uint[] ratings, int delta)
    {
      for(int index = 0; index < ratings.Length; ++index)
        ModCombatRating((CombatRating) ratings[index], delta);
    }

    public CreatureMask CreatureTracking
    {
      get { return (CreatureMask) GetUInt32(PlayerFields.TRACK_CREATURES); }
      internal set { SetUInt32(PlayerFields.TRACK_CREATURES, (uint) value); }
    }

    public LockMask ResourceTracking
    {
      get { return (LockMask) GetUInt32(PlayerFields.TRACK_RESOURCES); }
      internal set { SetUInt32(PlayerFields.TRACK_RESOURCES, (uint) value); }
    }

    public float BlockChance
    {
      get { return GetFloat(PlayerFields.BLOCK_PERCENTAGE); }
      internal set { SetFloat(PlayerFields.BLOCK_PERCENTAGE, value); }
    }

    /// <summary>Amount of damage reduced when an attack is blocked</summary>
    public uint BlockValue
    {
      get { return GetUInt32(PlayerFields.SHIELD_BLOCK); }
      internal set { SetUInt32(PlayerFields.SHIELD_BLOCK, value); }
    }

    /// <summary>Value in %</summary>
    public float DodgeChance
    {
      get { return GetFloat(PlayerFields.DODGE_PERCENTAGE); }
      set { SetFloat(PlayerFields.DODGE_PERCENTAGE, value); }
    }

    public override float ParryChance
    {
      get { return GetFloat(PlayerFields.PARRY_PERCENTAGE); }
      internal set { SetFloat(PlayerFields.PARRY_PERCENTAGE, value); }
    }

    public uint Expertise
    {
      get { return GetUInt32(PlayerFields.EXPERTISE); }
      set { SetUInt32(PlayerFields.EXPERTISE, value); }
    }

    public float CritChanceMeleePct
    {
      get { return GetFloat(PlayerFields.CRIT_PERCENTAGE); }
      internal set { SetFloat(PlayerFields.CRIT_PERCENTAGE, value); }
    }

    public float CritChanceRangedPct
    {
      get { return GetFloat(PlayerFields.RANGED_CRIT_PERCENTAGE); }
      internal set { SetFloat(PlayerFields.RANGED_CRIT_PERCENTAGE, value); }
    }

    public float CritChanceOffHandPct
    {
      get { return GetFloat(PlayerFields.OFFHAND_CRIT_PERCENTAGE); }
      internal set { SetFloat(PlayerFields.OFFHAND_CRIT_PERCENTAGE, value); }
    }

    /// <summary>Character's hit chance in %</summary>
    public float HitChance { get; set; }

    public float RangedHitChance { get; set; }

    public override uint Defense { get; internal set; }

    /// <summary>Character spell hit chance bonus from hit rating in %</summary>
    public float SpellHitChanceFromHitRating
    {
      get
      {
        return GetCombatRating(CombatRating.SpellHitChance) /
               GameTables.CombatRatings[CombatRating.SpellHitChance][CasterLevel - 1];
      }
    }

    public void ResetQuest(int slot)
    {
      int num = slot * 5;
      SetUInt32((PlayerFields) (158 + num), 0U);
      SetUInt32((PlayerFields) (159 + num), 0U);
      SetUInt32((PlayerFields) (160 + num), 0U);
      SetUInt32((PlayerFields) (161 + num), 0U);
      SetUInt32((PlayerFields) (162 + num), 0U);
    }

    /// <summary>Gets the quest field.</summary>
    /// <param name="slot">The slot.</param>
    public uint GetQuestId(int slot)
    {
      return GetUInt32((PlayerFields) (158 + slot * 5));
    }

    /// <summary>
    /// Sets the quest field, where fields are indexed from 0.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="questid">The questid.</param>
    public void SetQuestId(int slot, uint questid)
    {
      SetUInt32((PlayerFields) (158 + slot * 5), questid);
    }

    /// <summary>Gets the state of the quest.</summary>
    /// <param name="slot">The slot.</param>
    /// <returns></returns>
    public QuestCompleteStatus GetQuestState(int slot)
    {
      return (QuestCompleteStatus) GetUInt32((PlayerFields) (159 + slot * 5));
    }

    /// <summary>Sets the state of the quest.</summary>
    /// <param name="slot">The slot.</param>
    /// <param name="completeStatus">The status.</param>
    public void SetQuestState(int slot, QuestCompleteStatus completeStatus)
    {
      SetUInt32((PlayerFields) (159 + slot * 5), (uint) completeStatus);
    }

    /// <summary>
    /// Sets the quest count at the given index for the given Quest to the given value.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="interactionIndex">The count slot.</param>
    /// <param name="value">The value.</param>
    internal void SetQuestCount(int slot, uint interactionIndex, ushort value)
    {
      PlayerFields playerFields = (PlayerFields) (slot * 5 + 160 + ((int) interactionIndex >> 1));
      if(interactionIndex % 2U == 0U)
        SetUInt16Low(playerFields, value);
      else
        SetUInt16High(playerFields, value);
    }

    /// <summary>Gets the quest time.</summary>
    /// <param name="slot">The slot.</param>
    /// <returns></returns>
    internal uint GetQuestTimeLeft(byte slot)
    {
      return GetUInt32((PlayerFields) (162 + slot * 5));
    }

    /// <summary>Sets the quest time.</summary>
    /// <param name="slot">The slot.</param>
    internal void SetQuestTimeLeft(byte slot, uint timeleft)
    {
      SetUInt32((PlayerFields) (162 + slot * 5), timeleft);
    }

    /// <summary>This array stores completed daily quests</summary>
    /// <returns></returns>
    public uint[] DailyQuests
    {
      get
      {
        uint[] numArray = new uint[25];
        for(int index = 0; index < 25; ++index)
          numArray[index] = GetUInt32((PlayerFields) (1280 + index));
        return numArray;
      }
    }

    /// <summary>Gets the quest field.</summary>
    /// <param name="slot">The slot.</param>
    public uint GetDailyQuest(byte slot)
    {
      return GetUInt32((PlayerFields) (1280 + slot));
    }

    /// <summary>
    /// Sets the quest field, where fields are indexed from 0.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="questid">The questid.</param>
    public void SetDailyQuest(byte slot, uint questid)
    {
      SetUInt32((PlayerFields) (1280 + slot), questid);
    }

    public void ResetDailyQuests()
    {
      for(int index = 0; index < 25; ++index)
        SetUInt32((PlayerFields) (1280 + index), 0U);
    }

    /// <summary>
    /// Modifies the damage for the given school by the given delta.
    /// </summary>
    protected internal override void AddDamageDoneModSilently(DamageSchool school, int delta)
    {
      if(delta == 0)
        return;
      PlayerFields playerFields = delta <= 0 ? PlayerFields.MOD_DAMAGE_DONE_NEG : PlayerFields.MOD_DAMAGE_DONE_POS;
      SetInt32((PlayerFields) ((int) playerFields + (int) school),
        GetInt32((PlayerFields) ((int) playerFields + (int) school)) + delta);
    }

    /// <summary>
    /// Modifies the damage for the given school by the given delta.
    /// </summary>
    protected internal override void RemoveDamageDoneModSilently(DamageSchool school, int delta)
    {
      if(delta == 0)
        return;
      PlayerFields playerFields = delta <= 0 ? PlayerFields.MOD_DAMAGE_DONE_NEG : PlayerFields.MOD_DAMAGE_DONE_POS;
      SetUInt32((PlayerFields) ((int) playerFields + (int) school),
        GetUInt32((PlayerFields) ((int) playerFields + (int) school)) - (uint) delta);
    }

    protected internal override void ModDamageDoneFactorSilently(DamageSchool school, float delta)
    {
      if(delta == 0.0)
        return;
      PlayerFields playerFields = (PlayerFields) (1185U + school);
      SetFloat(playerFields, GetFloat(playerFields) + delta);
    }

    public override float GetDamageDoneFactor(DamageSchool school)
    {
      return GetFloat((PlayerFields) (1185U + school));
    }

    public override int GetDamageDoneMod(DamageSchool school)
    {
      return GetInt32((PlayerFields) (1171U + school)) - GetInt32((PlayerFields) (1178U + school));
    }

    /// <summary>Increased healing done *by* this Character</summary>
    public int HealingDoneMod
    {
      get { return GetInt32(PlayerFields.MOD_HEALING_DONE_POS); }
      set { SetInt32(PlayerFields.MOD_HEALING_DONE_POS, value); }
    }

    /// <summary>Increased healing % done *by* this Character</summary>
    public float HealingDoneModPct
    {
      get { return GetFloat(PlayerFields.MOD_HEALING_DONE_PCT); }
      set { SetFloat(PlayerFields.MOD_HEALING_DONE_PCT, value); }
    }

    /// <summary>Increased healing done *to* this Character</summary>
    public float HealingTakenModPct
    {
      get { return GetFloat(PlayerFields.MOD_HEALING_PCT); }
      set { SetFloat(PlayerFields.MOD_HEALING_PCT, value); }
    }

    /// <summary>
    /// Returns the SpellCritChance for the given DamageType (0-100)
    /// </summary>
    public override float GetCritChance(DamageSchool school)
    {
      return GetFloat((PlayerFields) (1032U + school));
    }

    /// <summary>Sets the SpellCritChance for the given DamageType</summary>
    internal void SetCritChance(DamageSchool school, float val)
    {
      SetFloat((PlayerFields) (1032U + school), val);
    }

    public EntityId FarSight
    {
      get { return GetEntityId(PlayerFields.FARSIGHT); }
      set { SetEntityId(PlayerFields.FARSIGHT, value); }
    }

    /// <summary>
    /// Make sure that the given slot is actually an EquipmentSlot
    /// </summary>
    internal void SetVisibleItem(InventorySlot slot, Asda2Item item)
    {
      PlayerFields playerFields = (PlayerFields) (283 + (int) slot * 2);
      if(item != null)
        SetUInt32(playerFields, item.Template.Id);
      else
        SetUInt32(playerFields, 0U);
    }

    /// <summary>Sets an ActionButton with the given information.</summary>
    public void BindActionButton(uint btnIndex, uint action, byte type, bool update = true)
    {
      CurrentSpecProfile.State = RecordState.Dirty;
      byte[] actionButtons = CurrentSpecProfile.ActionButtons;
      btnIndex *= 4U;
      if(action == 0U)
      {
        Array.Copy(ActionButton.EmptyButton, 0L, actionButtons, btnIndex, 4L);
      }
      else
      {
        actionButtons[btnIndex] = (byte) (action & byte.MaxValue);
        actionButtons[btnIndex + 1U] = (byte) ((action & 65280U) >> 8);
        actionButtons[btnIndex + 2U] = (byte) ((action & 16711680U) >> 16);
        actionButtons[btnIndex + 3U] = type;
      }
    }

    public uint GetActionFromActionButton(int buttonIndex)
    {
      byte[] actionButtons = CurrentSpecProfile.ActionButtons;
      buttonIndex *= 4;
      return BitConverter.ToUInt32(actionButtons, buttonIndex) & 16777215U;
    }

    public byte GetTypeFromActionButton(int buttonIndex)
    {
      buttonIndex *= 4;
      return CurrentSpecProfile.ActionButtons[buttonIndex + 3];
    }

    /// <summary>
    /// Sets the given button to the given spell and resends it to the client
    /// </summary>
    public void BindSpellToActionButton(uint btnIndex, SpellId spell, bool update = true)
    {
      BindActionButton(btnIndex, (uint) spell, 0, true);
    }

    /// <summary>Sets the given action button</summary>
    public void BindActionButton(ActionButton btn, bool update = true)
    {
      btn.Set(CurrentSpecProfile.ActionButtons);
      CurrentSpecProfile.State = RecordState.Dirty;
    }

    /// <summary>
    /// 
    /// </summary>
    public byte[] ActionButtons
    {
      get { return CurrentSpecProfile.ActionButtons; }
    }

    public override ObjectTypeCustom CustomType
    {
      get { return ObjectTypeCustom.Object | ObjectTypeCustom.Unit | ObjectTypeCustom.Player; }
    }

    public CharacterRecord Record
    {
      get { return m_record; }
    }

    /// <summary>
    /// The active ticket of this Character or null if there is none
    /// </summary>
    public Ticket Ticket { get; internal set; }

    public override int Health
    {
      get { return base.Health; }
      set
      {
        if(Health == value)
          return;
        base.Health = value;
        if(Map == null)
          return;
        UpdateTargeters();
        if(IsInGroup)
          Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
        if(!IsSoulmated)
          return;
        Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(Client);
      }
    }

    public override int MaxHealth
    {
      get { return base.MaxHealth; }
      internal set
      {
        base.MaxHealth = value;
        if(Map == null)
          return;
        Asda2CharacterHandler.SendHealthUpdate(this, false, false);
        if(IsInGroup)
          Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
        if(IsSoulmated)
          Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(Client);
        UpdateTargeters();
      }
    }

    private void UpdateTargeters()
    {
      Asda2CharacterHandler.SendSelectedCharacterInfoToMultipyTargets(this, TargetersOnMe.ToArray());
    }

    public int BaseHealthCapacity
    {
      get { return m_archetype.Class.GetLevelSetting(Level).Health; }
    }

    public override int Power
    {
      get { return base.Power; }
      set
      {
        if(Power == value)
          return;
        base.Power = value;
        SendPowerUpdates(this);
      }
    }

    public override PowerType PowerType
    {
      get { return base.PowerType; }
      set
      {
        base.PowerType = value;
        GroupUpdateFlags |= GroupUpdateFlags.PowerType | GroupUpdateFlags.Power | GroupUpdateFlags.MaxPower;
      }
    }

    public int BaseManaPoolCapacity
    {
      get { return m_archetype.Class.GetLevelSetting(Level).Mana; }
    }

    public override int MaxPower
    {
      get { return base.MaxPower; }
      internal set
      {
        base.MaxPower = value;
        if(Power > MaxPower)
          Power = MaxPower;
        Asda2CharacterHandler.SendCharMpUpdateResponse(this);
        if(IsInGroup)
          Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
        if(!IsSoulmated)
          return;
        Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(Client);
      }
    }

    public override Map Map
    {
      get { return base.Map; }
      internal set
      {
        base.Map = value;
        if(!IsInGuild)
          return;
        Asda2GuildHandler.SendGuildNotificationResponse(Guild, GuildNotificationType.Silence, GuildMember);
      }
    }

    public override int Level
    {
      get { return base.Level; }
      set
      {
        base.Level = value;
        NextLevelXP = XpGenerator.GetXpForlevel(value + 1);
        if(Map == null)
          return;
        if(IsInGroup)
          Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
        if(IsSoulmated)
          Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(Client);
        if(IsInGuild)
          Asda2GuildHandler.SendGuildNotificationResponse(Guild, GuildNotificationType.Silence, GuildMember);
        UpdateTargeters();
      }
    }

    public override int MaxLevel
    {
      get { return GetInt32(PlayerFields.MAX_LEVEL); }
      internal set { SetInt32(PlayerFields.MAX_LEVEL, value); }
    }

    public override Zone Zone
    {
      get { return base.Zone; }
      internal set
      {
        if(m_zone == value)
          return;
        if(value != null && m_Map != null)
          value.EnterZone(this, m_zone);
        base.Zone = value;
        GroupUpdateFlags |= GroupUpdateFlags.ZoneId;
      }
    }

    public bool IsZoneExplored(ZoneId id)
    {
      ZoneTemplate zoneInfo = World.GetZoneInfo(id);
      if(zoneInfo != null)
        return IsZoneExplored(zoneInfo);
      return false;
    }

    public bool IsZoneExplored(ZoneTemplate zone)
    {
      return IsZoneExplored(zone.ExplorationBit);
    }

    public bool IsZoneExplored(int explorationBit)
    {
      int index = explorationBit >> 3;
      if(index >> 2 >= UpdateFieldMgr.ExplorationZoneFieldSize)
        return false;
      int num = 1 << explorationBit % 8;
      return (m_record.ExploredZones[index] & num) != 0;
    }

    public void SetZoneExplored(ZoneId id, bool explored)
    {
    }

    public void SetZoneExplored(ZoneTemplate zone, bool gainXp)
    {
    }

    public override Vector3 Position
    {
      get { return base.Position; }
      internal set
      {
        base.Position = value;
        GroupUpdateFlags |= GroupUpdateFlags.Position;
      }
    }

    public override uint Phase
    {
      get { return m_Phase; }
      set { m_Phase = value; }
    }

    public override bool IsInWorld
    {
      get { return m_initialized; }
    }

    /// <summary>The type of this object (player, corpse, item, etc)</summary>
    public override ObjectTypeId ObjectTypeId
    {
      get { return ObjectTypeId.Player; }
    }

    /// <summary>The client currently playing the character.</summary>
    public IRealmClient Client
    {
      get { return m_client; }
      protected set { m_client = value; }
    }

    /// <summary>The status of the character.</summary>
    public CharacterStatus Status
    {
      get
      {
        CharacterStatus characterStatus = CharacterStatus.OFFLINE;
        if(IsAFK)
          characterStatus |= CharacterStatus.AFK;
        if(IsDND)
          characterStatus |= CharacterStatus.DND;
        if(IsInWorld)
          characterStatus |= CharacterStatus.ONLINE;
        return characterStatus;
      }
    }

    /// <summary>
    /// The GroupMember object of this Character (if it he/she is in any group)
    /// </summary>
    public GroupMember GroupMember
    {
      get { return m_groupMember; }
      internal set { m_groupMember = value; }
    }

    /// <summary>
    /// The GuildMember object of this Character (if it he/she is in a guild)
    /// </summary>
    public GuildMember GuildMember
    {
      get { return m_guildMember; }
      set
      {
        m_guildMember = value;
        if(m_guildMember != null)
        {
          GuildId = m_guildMember.Guild.Id;
          GuildRank = (uint) m_guildMember.RankId;
        }
        else
        {
          GuildId = 0U;
          GuildRank = 0U;
        }
      }
    }

    /// <summary>
    /// The ArenaTeamMember object of this Character (if it he/she is in an arena team)
    /// </summary>
    public Battlegrounds.Arenas.ArenaTeamMember[] ArenaTeamMember
    {
      get { return m_arenaTeamMember; }
    }

    /// <summary>
    /// Characters get disposed after Logout sequence completed and
    /// cannot (and must not) be used anymore.
    /// </summary>
    public bool IsDisposed
    {
      get { return m_auras == null; }
    }

    /// <summary>
    /// The Group of this Character (if it he/she is in any group)
    /// </summary>
    public Group Group
    {
      get
      {
        if(m_groupMember == null)
          return null;
        return m_groupMember.SubGroup?.Group;
      }
    }

    /// <summary>The subgroup in which the character is (if any)</summary>
    public SubGroup SubGroup
    {
      get
      {
        if(m_groupMember != null)
          return m_groupMember.SubGroup;
        return null;
      }
    }

    public GroupUpdateFlags GroupUpdateFlags
    {
      get { return m_groupUpdateFlags; }
      set { m_groupUpdateFlags = value; }
    }

    /// <summary>The guild in which the character is (if any)</summary>
    public Guild Guild
    {
      get
      {
        if(m_guildMember != null)
          return m_guildMember.Guild;
        return null;
      }
    }

    /// <summary>The account this character belongs to.</summary>
    public RealmAccount Account { get; protected internal set; }

    public RoleGroup Role
    {
      get
      {
        RealmAccount account = Account;
        if(account == null)
          return Singleton<PrivilegeMgr>.Instance.LowestRole;
        return account.Role;
      }
    }

    public override ClientLocale Locale
    {
      get { return m_client.Info.Locale; }
      set { m_client.Info.Locale = value; }
    }

    /// <summary>The name of this character.</summary>
    public override string Name
    {
      get { return m_name; }
      set { m_name = value; }
    }

    public Corpse Corpse
    {
      get { return m_corpse; }
      internal set
      {
        if(value == null && m_corpse != null)
        {
          m_corpse.StartDecay();
          m_record.CorpseX = new float?();
        }

        m_corpse = value;
      }
    }

    /// <summary>
    /// The <see cref="P:WCell.RealmServer.Entities.Character.Archetype">Archetype</see> of this Character
    /// </summary>
    public Archetype Archetype
    {
      get { return m_archetype; }
      set
      {
        m_archetype = value;
        Race = value.Race.Id;
        Class = value.Class.Id;
        Asda2CharacterHandler.SendChangeProfessionResponse(Client);
        if(!IsInGuild)
          return;
        Asda2GuildHandler.SendGuildNotificationResponse(Guild, GuildNotificationType.Silence, GuildMember);
      }
    }

    /// <summary>
    /// </summary>
    public byte Outfit { get; set; }

    /// <summary>The channels the character is currently joined to.</summary>
    public List<ChatChannel> ChatChannels
    {
      get { return m_chatChannels; }
      set { m_chatChannels = value; }
    }

    /// <summary>
    /// Whether this Character is currently trading with someone
    /// </summary>
    public bool IsTrading
    {
      get { return m_tradeWindow != null; }
    }

    /// <summary>
    /// Current trading progress of the character
    /// Null if none
    /// </summary>
    public TradeWindow TradeWindow
    {
      get { return m_tradeWindow; }
      set { m_tradeWindow = value; }
    }

    /// <summary>Last login time of this character.</summary>
    public DateTime LastLogin
    {
      get { return m_record.LastLogin.Value; }
      set { m_record.LastLogin = value; }
    }

    /// <summary>Last logout time of this character.</summary>
    public DateTime? LastLogout
    {
      get { return m_record.LastLogout; }
      set { m_record.LastLogout = value; }
    }

    public bool IsFirstLogin
    {
      get { return !m_record.LastLogout.HasValue; }
    }

    public TutorialFlags TutorialFlags { get; set; }

    /// <summary>Total play time of this Character in seconds</summary>
    public uint TotalPlayTime
    {
      get { return (uint) m_record.TotalPlayTime; }
      set { m_record.TotalPlayTime = (int) value; }
    }

    /// <summary>
    /// How long is this Character already on this level in seconds
    /// </summary>
    public uint LevelPlayTime
    {
      get { return (uint) m_record.LevelPlayTime; }
      set { m_record.LevelPlayTime = (int) value; }
    }

    /// <summary>Whether or not this character has the GM-tag set.</summary>
    public bool ShowAsGameMaster
    {
      get { return PlayerFlags.HasFlag(PlayerFlags.GM); }
      set
      {
        if(value)
          PlayerFlags |= PlayerFlags.GM;
        else
          PlayerFlags &= ~PlayerFlags.GM;
      }
    }

    /// <summary>Gets/Sets the godmode</summary>
    public bool GodMode
    {
      get { return m_record.GodMode; }
      set
      {
        m_record.GodMode = value;
        SpellCast spellCast = m_spellCast;
        if(spellCast != null)
          spellCast.GodMode = value;
        if(value)
        {
          Health = MaxHealth;
          Power = MaxPower;
          m_spells.ClearCooldowns();
          ShowAsGameMaster = true;
          IncMechanicCount(SpellMechanic.Invulnerable, false);
        }
        else
        {
          DecMechanicCount(SpellMechanic.Invulnerable, false);
          ShowAsGameMaster = false;
        }
      }
    }

    protected override void InitSpellCast()
    {
      base.InitSpellCast();
      m_spellCast.GodMode = GodMode;
    }

    /// <summary>Whether the PvP Flag is set.</summary>
    public bool IsPvPFlagSet
    {
      get { return PlayerFlags.HasFlag(PlayerFlags.PVP); }
      set
      {
        if(value)
          PlayerFlags |= PlayerFlags.PVP;
        else
          PlayerFlags &= ~PlayerFlags.PVP;
      }
    }

    /// <summary>Whether the PvP Flag reset timer is active.</summary>
    public bool IsPvPTimerActive
    {
      get { return PlayerFlags.HasFlag(PlayerFlags.PVPTimerActive); }
      set
      {
        if(value)
          PlayerFlags |= PlayerFlags.PVPTimerActive;
        else
          PlayerFlags &= ~PlayerFlags.PVPTimerActive;
      }
    }

    /// <summary>Whether or not this character is AFK.</summary>
    public bool IsAFK
    {
      get { return PlayerFlags.HasFlag(PlayerFlags.AFK); }
      set
      {
        if(value)
          PlayerFlags |= PlayerFlags.AFK;
        else
          PlayerFlags &= ~PlayerFlags.AFK;
        GroupUpdateFlags |= GroupUpdateFlags.Status;
      }
    }

    /// <summary>The custom AFK reason when player is AFK.</summary>
    public string AFKReason { get; set; }

    /// <summary>Whether or not this character is DND.</summary>
    public bool IsDND
    {
      get { return PlayerFlags.HasFlag(PlayerFlags.DND); }
      set
      {
        if(value)
          PlayerFlags |= PlayerFlags.DND;
        else
          PlayerFlags &= ~PlayerFlags.DND;
        GroupUpdateFlags |= GroupUpdateFlags.Status;
      }
    }

    /// <summary>The custom DND reason when player is DND.</summary>
    public string DNDReason { get; set; }

    /// <summary>Gets the chat tag for the character.</summary>
    public override ChatTag ChatTag
    {
      get
      {
        if(ShowAsGameMaster)
          return ChatTag.GM;
        if(IsAFK)
          return ChatTag.AFK;
        return IsDND ? ChatTag.DND : ChatTag.None;
      }
    }

    /// <summary>
    /// Collection of reputations with all factions known to this Character
    /// </summary>
    public ReputationCollection Reputations
    {
      get { return m_reputations; }
    }

    /// <summary>Collection of all this Character's skills</summary>
    public SkillCollection Skills
    {
      get { return m_skills; }
    }

    /// <summary>Collection of all this Character's Talents</summary>
    public override TalentCollection Talents
    {
      get { return m_talents; }
    }

    /// <summary>Collection of all this Character's Achievements</summary>
    public AchievementCollection Achievements
    {
      get { return m_achievements; }
    }

    /// <summary>All spells known to this chr</summary>
    public PlayerAuraCollection PlayerAuras
    {
      get { return (PlayerAuraCollection) m_auras; }
    }

    /// <summary>All spells known to this chr</summary>
    public PlayerSpellCollection PlayerSpells
    {
      get { return (PlayerSpellCollection) m_spells; }
    }

    /// <summary>Mask of the activated Flight Paths</summary>
    public TaxiNodeMask TaxiNodes
    {
      get { return m_taxiNodeMask; }
    }

    /// <summary>The Tavern-location of where the Player bound to</summary>
    public IWorldZoneLocation BindLocation
    {
      get
      {
        CheckBindLocation();
        return m_bindLocation;
      }
      internal set { m_bindLocation = value; }
    }

    /// <summary>
    /// The Inventory of this Character contains all Items and Item-related things
    /// </summary>
    public PlayerInventory Inventory
    {
      get { return m_inventory; }
    }

    public Asda2PlayerInventory Asda2Inventory
    {
      get { return _asda2Inventory; }
    }

    /// <summary>
    /// Returns the same as Inventory but with another type (for IContainer interface)
    /// </summary>
    public BaseInventory BaseInventory
    {
      get { return m_inventory; }
    }

    /// <summary>The Character's MailAccount</summary>
    public MailAccount MailAccount
    {
      get { return m_mailAccount; }
      set
      {
        if(m_mailAccount == value)
          return;
        m_mailAccount = value;
      }
    }

    /// <summary>Unused talent-points for this Character</summary>
    public int FreeTalentPoints
    {
      get { return (int) GetUInt32(PlayerFields.CHARACTER_POINTS1); }
      set
      {
        if(value < 0)
          value = 0;
        SetUInt32(PlayerFields.CHARACTER_POINTS1, (uint) value);
        TalentHandler.SendTalentGroupList(m_talents);
      }
    }

    /// <summary>Doesn't send a packet to the client</summary>
    public void UpdateFreeTalentPointsSilently(int delta)
    {
      SetUInt32(PlayerFields.CHARACTER_POINTS1, (uint) (FreeTalentPoints + delta));
    }

    /// <summary>Forced logout must not be cancelled</summary>
    public bool IsKicked
    {
      get
      {
        if(m_isLoggingOut)
          return !IsPlayerLogout;
        return false;
      }
    }

    /// <summary>
    /// The current GossipConversation that this Character is having
    /// </summary>
    public GossipConversation GossipConversation { get; set; }

    /// <summary>Lets the Character gossip with the given speaker</summary>
    public void StartGossip(GossipMenu menu, WorldObject speaker)
    {
      GossipConversation = new GossipConversation(menu, this, speaker, menu.KeepOpen);
      GossipConversation.DisplayCurrentMenu();
    }

    /// <summary>Lets the Character gossip with herself</summary>
    public void StartGossip(GossipMenu menu)
    {
      GossipConversation = new GossipConversation(menu, this, this, menu.KeepOpen);
      GossipConversation.DisplayCurrentMenu();
    }

    /// <summary>
    /// Returns whether this Character is invited into a Group already
    /// </summary>
    /// <returns></returns>
    public bool IsInvitedToGroup
    {
      get
      {
        return Singleton<RelationMgr>.Instance.HasPassiveRelations(EntityId.Low, CharacterRelationType.GroupInvite);
      }
    }

    /// <summary>
    /// Returns whether this Character is invited into a Guild already
    /// </summary>
    /// <returns></returns>
    public bool IsInvitedToGuild
    {
      get
      {
        return Singleton<RelationMgr>.Instance.HasPassiveRelations(EntityId.Low, CharacterRelationType.GuildInvite);
      }
    }

    public bool HasTitle(TitleId titleId)
    {
      CharacterTitleEntry titleEntry = TitleMgr.GetTitleEntry(titleId);
      if(titleEntry == null)
        return false;
      TitleBitId bitIndex = titleEntry.BitIndex;
      return ((CharTitlesMask) GetUInt32((int) bitIndex / 32 + 626)).HasFlag(
        (CharTitlesMask) (uint) (1 << (int) bitIndex % 32));
    }

    public bool HasTitle(TitleBitId titleBitId)
    {
      CharacterTitleEntry titleEntry = TitleMgr.GetTitleEntry(titleBitId);
      if(titleEntry == null)
        return false;
      return HasTitle(titleEntry.TitleId);
    }

    public void SetTitle(TitleId titleId, bool lost)
    {
      CharacterTitleEntry titleEntry = TitleMgr.GetTitleEntry(titleId);
      if(titleEntry == null)
      {
        log.Warn(string.Format("TitleId: {0} could not be found.", titleId));
      }
      else
      {
        TitleBitId bitIndex = titleEntry.BitIndex;
        int field = (int) bitIndex / 32 + 626;
        uint num1 = (uint) (1 << (int) bitIndex % 32);
        if(lost)
        {
          if(!HasTitle(titleId))
            return;
          uint num2 = GetUInt32(field) & ~num1;
          SetUInt32(field, num2);
        }
        else
        {
          if(HasTitle(titleId))
            return;
          uint num2 = GetUInt32(field) | num1;
          SetUInt32(field, num2);
        }

        TitleHandler.SendTitleEarned(this, titleEntry, lost);
      }
    }

    public uint Glyphs_Enable
    {
      get { return GetUInt32(PlayerFields.GLYPHS_ENABLED); }
      set { SetUInt32(PlayerFields.GLYPHS_ENABLED, value); }
    }

    public short SessionId { get; set; }

    public Vector3 LastNewPosition { get; set; }

    protected bool Initialized { get; set; }

    public bool IsLoginServerStep { get; set; }

    public bool IsConnected { get; set; }

    public Asda2Profession Profession
    {
      get
      {
        switch(Class)
        {
          case ClassId.OHS:
            return Asda2Profession.Warrior;
          case ClassId.Spear:
            return Asda2Profession.Warrior;
          case ClassId.THS:
            return Asda2Profession.Warrior;
          case ClassId.Crossbow:
            return Asda2Profession.Archer;
          case ClassId.Bow:
            return Asda2Profession.Archer;
          case ClassId.Balista:
            return Asda2Profession.Archer;
          case ClassId.AtackMage:
            return Asda2Profession.Mage;
          case ClassId.SupportMage:
            return Asda2Profession.Mage;
          case ClassId.HealMage:
            return Asda2Profession.Mage;
          default:
            return Asda2Profession.NoProfession;
        }
      }
    }

    public byte ProfessionLevel
    {
      get { return m_record.ProfessionLevel; }
      set
      {
        m_record.ProfessionLevel = value;
        if(!IsInGuild)
          return;
        Asda2GuildHandler.SendGuildNotificationResponse(Guild, GuildNotificationType.Silence, GuildMember);
      }
    }

    public int MagicDefence { get; set; }

    public Asda2ClassMask Asda2ClassMask
    {
      get { return Archetype.Class.ClassMask; }
    }

    public byte EyesColor
    {
      get { return Record.EyesColor; }
      set { Record.EyesColor = value; }
    }

    public int PlaceInRating { get; set; }

    public Asda2PetRecord Asda2Pet { get; set; }

    /// <summary>0 - Light; 1- Dark; 2 - Chaos; -1 - None</summary>
    public short Asda2FactionId
    {
      get { return Record.Asda2FactionId; }
      set
      {
        Record.Asda2FactionId = value;
        if(Map == null)
          return;
        foreach(WorldObject nearbyObject in NearbyObjects)
        {
          if(nearbyObject is Character)
          {
            Character visibleChr = nearbyObject as Character;
            CheckAtackStateWithCharacter(visibleChr);
            visibleChr.CheckAtackStateWithCharacter(this);
          }
        }

        GlobalHandler.SendCharacterFactionToNearbyCharacters(this);
      }
    }

    /// <summary>0 - 20</summary>
    public short Asda2FactionRank
    {
      get { return _asda2FactionRank; }
      set { _asda2FactionRank = value; }
    }

    public int Asda2HonorPoints
    {
      get { return Record.Asda2HonorPoints; }
      set
      {
        if(Record.Asda2HonorPoints == value)
          return;
        Record.Asda2HonorPoints = value;
        Asda2CharacterHandler.SendFactionAndHonorPointsInitResponse(Client);
        RecalculateFactionRank(false);
      }
    }

    private void RecalculateFactionRank(bool silent = false)
    {
      int factionRank = CharacterFormulas.GetFactionRank(Asda2HonorPoints);
      if(Asda2FactionRank != factionRank)
      {
        Asda2FactionRank = (short) factionRank;
        switch(factionRank)
        {
          case 1:
            Map.CallDelayed(5000, () => GetTitle(Asda2TitleId.Private132));
            break;
          case 4:
            Map.CallDelayed(5000, () => GetTitle(Asda2TitleId.Sergeant133));
            break;
          case 7:
            Map.CallDelayed(5000, () => GetTitle(Asda2TitleId.Officer134));
            break;
          case 10:
            Map.CallDelayed(5000, () => GetTitle(Asda2TitleId.Captain135));
            break;
          case 13:
            Map.CallDelayed(5000, () => GetTitle(Asda2TitleId.Major136));
            break;
          case 16:
            Map.CallDelayed(5000, () => GetTitle(Asda2TitleId.Colonel137));
            break;
          case 18:
            Map.CallDelayed(5000, () => GetTitle(Asda2TitleId.General138));
            break;
          case 20:
            Map.CallDelayed(5000, () => GetTitle(Asda2TitleId.God139));
            break;
        }
      }

      if(silent)
        return;
      GlobalHandler.SendCharacterFactionToNearbyCharacters(this);
      Asda2CharacterHandler.SendFactionAndHonorPointsInitResponse(Client);
    }

    public string SoulmateIntroduction
    {
      get { return Account.AccountData.SoulmateIntroduction; }
      set { Account.AccountData.SoulmateIntroduction = value; }
    }

    public Asda2SoulmateRelationRecord SoulmateRecord { get; set; }

    public bool IsSoulmated
    {
      get { return SoulmateRecord != null; }
    }

    public CharacterRecord[] SoulmatedCharactersRecords { get; set; }

    public Character SoulmateCharacter
    {
      get
      {
        if(SoulmateRealmAccount == null)
          return null;
        return SoulmateRealmAccount.ActiveCharacter;
      }
    }

    public RealmAccount SoulmateRealmAccount { get; set; }

    public int GuildPoints
    {
      get { return Record.GuildPoints; }
      set
      {
        Record.GuildPoints = value;
        Asda2GuildHandler.SendUpdateGuildPointsResponse(Client);
      }
    }

    public Color ChatColor
    {
      get { return GetChatColor(); }
      set { Record.GlobalChatColor = value; }
    }

    private Color GetChatColor()
    {
      switch(Role.Status)
      {
        case RoleStatus.EventManager:
          return Color.CadetBlue;
        case RoleStatus.Admin:
          return GmChatColor;
        default:
          return Color.Yellow;
      }
    }

    public int FishingLevel
    {
      get { return Record.FishingLevel + GetIntMod(StatModifierInt.Asda2FishingSkill); }
      set
      {
        Record.FishingLevel = value;
        Asda2FishingHandler.SendFishingLvlResponse(Client);
      }
    }

    public uint AccId
    {
      get { return (uint) Account.AccountId; }
    }

    public int AvatarMask
    {
      get { return Record.AvatarMask; }
      set
      {
        Record.AvatarMask = value;
        Asda2CharacterHandler.SendUpdateAvatarMaskResponse(this);
      }
    }

    public byte[] SettingsFlags
    {
      get { return Record.SettingsFlags; }
      set
      {
        Record.SettingsFlags = value;
        UpdateSettings();
      }
    }

    public bool EnableSoulmateRequest { get; set; }

    public bool EnableFriendRequest { get; set; }

    public bool EnableGearTradeRequest { get; set; }

    public bool EnableGeneralTradeRequest { get; set; }

    public bool EnableGuildRequest { get; set; }

    public bool EnablePartyRequest { get; set; }

    public bool EnableWishpers { get; set; }

    public bool IsDigging { get; set; }

    public Asda2TradeWindow Asda2TradeWindow { get; set; }

    public int Asda2TitlePoints { get; set; }

    public int Asda2Rank { get; set; }

    public UpdateMask DiscoveredTitles { get; set; }

    public UpdateMask GetedTitles { get; set; }

    public bool IsAsda2TradeDescriptionEnabled
    {
      get { return _isAsda2TradeDescriptionEnabled; }
      set
      {
        if(_isAsda2TradeDescriptionEnabled == value)
          return;
        _isAsda2TradeDescriptionEnabled = value;
        Map.CallDelayed(777, () => Asda2PrivateShopHandler.SendtradeStatusTextWindowResponse(this));
      }
    }

    public bool IsAsda2TradeDescriptionPremium { get; set; }

    public string Asda2TradeDescription
    {
      get { return _asda2TradeDescription ?? (_asda2TradeDescription = ""); }
      set
      {
        _asda2TradeDescription = value;
        if(!IsAsda2TradeDescriptionEnabled)
          return;
        Asda2PrivateShopHandler.SendtradeStatusTextWindowResponse(this);
      }
    }

    public Asda2PrivateShop PrivateShop { get; set; }

    public Asda2Pvp Asda2Duel { get; set; }

    public bool IsAsda2Dueling
    {
      get { return Asda2Duel != null; }
    }

    public Character Asda2DuelingOponent { get; set; }

    public byte GreenCharges { get; set; }

    public byte BlueCharges { get; set; }

    public byte RedCharges { get; set; }

    public byte Asda2GuildRank
    {
      get { return (byte) (4U - GuildRank); }
      set { GuildRank = 4U - value; }
    }

    public byte LearnedRecipesCount { get; set; }

    public UpdateMask LearnedRecipes { get; set; }

    public Fish CurrentFish { get; set; }

    public uint FishReadyTime { get; set; }

    public bool IsWarehouseLocked { get; set; }

    public Asda2Battleground CurrentBattleGround { get; set; }

    public byte CurrentBattleGroundId { get; set; }

    public WorldLocation LocatonBeforeOnEnterWar { get; set; }

    public short BattlegroundActPoints { get; set; }

    public int BattlegroundKills { get; set; }

    public int BattlegroundDeathes { get; set; }

    public bool IsAsda2BattlegroundInProgress
    {
      get
      {
        if(CurrentBattleGround != null && CurrentBattleGround.IsRunning)
          return MapId == MapId.BatleField;
        return false;
      }
    }

    public Asda2WarPoint CurrentCapturingPoint { get; set; }

    public Asda2Chatroom ChatRoom { get; set; }

    public Character CurrentFriendInviter { get; set; }

    public List<Asda2FriendshipRecord> FriendRecords { get; set; }

    public byte EatingAppleStep { get; set; }

    public bool CanTeleportToFriend { get; set; }

    public bool IsSoulmateEmpowerEnabled { get; set; }

    public DateTime SoulmateEmpowerEndTime { get; set; }

    public bool IsSoulmateSoulSaved { get; set; }

    public bool ErrorTeleportationEnabled { get; set; }

    public uint Last100PrcRecoveryUsed { get; set; }

    public short Asda2WingsItemId
    {
      get { return _asda2WingsItemId; }
      set
      {
        _asda2WingsItemId = value;
        if(Map == null)
          return;
        FunctionalItemsHandler.SendWingsInfoResponse(this, null);
      }
    }

    public short TransformationId
    {
      get { return _transformationId; }
      set
      {
        _transformationId = VerifyTransformationId(value);
        GlobalHandler.SendTransformToPetResponse(this, _transformationId != -1, null);
      }
    }

    private short VerifyTransformationId(short value)
    {
      if(value == 0 || value == 190 || (value == 192 || value == 197) ||
         (value == 373 || value == 551 || (value == 551 || value > 843)))
        return -1;
      return value;
    }

    public bool IsOnMount
    {
      get { return MountId != -1; }
    }

    public bool ExpBlock { get; set; }

    public bool IsFirstGameConnection { get; set; }

    public Vector3 TargetSummonPosition { get; set; }

    public MapId TargetSummonMap { get; set; }

    public bool IsReborning { get; set; }

    public bool ChatBanned
    {
      get { return Record.ChatBanned; }
      set { Record.ChatBanned = value; }
    }

    public DateTime? BanChatTill
    {
      get { return Record.BanChatTill; }
      set { Record.BanChatTill = value; }
    }

    public void SetGlyphSlot(byte slot, uint id)
    {
      SetUInt32((PlayerFields) (1312 + slot), id);
    }

    public uint GetGlyphSlot(byte slot)
    {
      return GetUInt32((PlayerFields) (1312 + slot));
    }

    public void SetGlyph(byte slot, uint glyph)
    {
      SetUInt32((PlayerFields) (1318 + slot), glyph);
    }

    public uint GetGlyph(byte slot)
    {
      return GetUInt32((PlayerFields) (1318 + slot));
    }

    private void SaveCharacter()
    {
      if(IsDisposed || Map == null || Client == null)
      {
        _saveTaskRunning = false;
      }
      else
      {
        SaveNow();
        ServerApp<RealmServer>.IOQueue.CallDelayed(CharacterFormulas.SaveChateterInterval, SaveCharacter);
      }
    }

    public DateTime ArggredDateTime { get; set; }

    public bool IsAggred { get; set; }

    public bool IsFirstMoveAfterAtack { get; set; }

    public bool IsWaitingForAtackAnimation { get; set; }

    /// <summary>Clears all trade-related fields for the character.</summary>
    public void ClearTrade()
    {
    }

    public void UpdatePlayedTime()
    {
      DateTime now = DateTime.Now;
      TimeSpan timeSpan = now - m_lastPlayTimeUpdate;
      LevelPlayTime += (uint) timeSpan.TotalSeconds;
      TotalPlayTime += (uint) timeSpan.TotalSeconds;
      m_lastPlayTimeUpdate = now;
    }

    /// <summary>Check to see if character is in an instance</summary>
    public bool IsInInstance
    {
      get
      {
        if(m_Map != null)
          return m_Map.IsInstance;
        return false;
      }
    }

    /// <summary>Check to see if character is in a group</summary>
    public bool IsInGroup
    {
      get { return m_groupMember != null; }
    }

    /// <summary>Check to see if character is in a Guild</summary>
    public bool IsInGuild
    {
      get { return m_guildMember != null; }
    }

    /// <summary>Check to see if character is in a group</summary>
    public bool IsInRaid
    {
      get { return Group is RaidGroup; }
    }

    /// <summary>
    /// Check to see if character is in the same instance as group members
    /// </summary>
    public bool IsInGroupInstance
    {
      get
      {
        Group group = Group;
        if(group != null)
          return group.GetActiveInstance(m_Map.MapTemplate) != null;
        return false;
      }
    }

    /// <summary>
    /// Personal Dungeon Difficulty, might differ from current Difficulty
    /// </summary>
    public DungeonDifficulty DungeonDifficulty
    {
      get { return m_record.DungeonDifficulty; }
      set
      {
        m_record.DungeonDifficulty = value;
        if(m_groupMember != null)
          return;
        InstanceHandler.SendDungeonDifficulty(this);
      }
    }

    public RaidDifficulty RaidDifficulty
    {
      get { return m_record.RaidDifficulty; }
      set
      {
        m_record.RaidDifficulty = value;
        if(m_groupMember != null)
          return;
        InstanceHandler.SendRaidDifficulty(this);
      }
    }

    public bool IsAllowedLowLevelRaid
    {
      get { return PlayerFlags.HasFlag(PlayerFlags.AllowLowLevelRaid); }
      set
      {
        if(value)
          PlayerFlags |= PlayerFlags.AllowLowLevelRaid;
        else
          PlayerFlags &= ~PlayerFlags.AllowLowLevelRaid;
      }
    }

    public uint GetInstanceDifficulty(bool isRaid)
    {
      if(m_groupMember != null)
        return m_groupMember.Group.DungeonDifficulty;
      if(!isRaid)
        return (uint) m_record.DungeonDifficulty;
      return (uint) m_record.RaidDifficulty;
    }

    public override bool IsAlive
    {
      get { return Health != 0; }
    }

    /// <summary>
    /// whether the Corpse is reclaimable
    /// (Character must be ghost and the reclaim delay must have passed)
    /// </summary>
    public bool IsCorpseReclaimable
    {
      get
      {
        if(IsGhost)
          return DateTime.Now > m_record.LastResTime.AddMilliseconds(Corpse.MinReclaimDelay);
        return false;
      }
    }

    /// <summary>
    /// Character can reclaim if Corpse is reclaimable and Character is close to Corpse,
    /// or if there is no Corpse, Character must be somewhere near a SpiritHealer
    /// </summary>
    public bool CanReclaimCorpse
    {
      get
      {
        if(!IsCorpseReclaimable)
          return false;
        if(m_corpse != null && IsInRadiusSq(m_corpse, Corpse.ReclaimRadiusSq))
          return true;
        if(IsGhost && m_corpse == null)
          return KnownObjects.Contains(obj =>
          {
            if(obj is Unit)
              return ((Unit) obj).IsSpiritHealer;
            return false;
          });
        return false;
      }
    }

    /// <summary>Last time this Character died</summary>
    public DateTime LastDeathTime
    {
      get { return m_record.LastDeathTime; }
      set { m_record.LastDeathTime = value; }
    }

    /// <summary>Last time this Character was resurrected</summary>
    public DateTime LastResTime
    {
      get { return m_record.LastResTime; }
      set { m_record.LastResTime = value; }
    }

    protected override bool OnBeforeDeath()
    {
      if(Health == 0)
        Health = 1;
      if(!m_Map.MapTemplate.NotifyPlayerBeforeDeath(this))
        return false;
      if(!IsDueling)
        return true;
      Duel.OnDeath(this);
      return false;
    }

    protected override void OnDeath()
    {
      PereodicActions.Clear();
      m_record.LastDeathTime = DateTime.Now;
      if(IsSoulmateSoulSaved && SoulmateCharacter != null && GetDistance(SoulmateCharacter) < 40.0)
      {
        IsSoulmateSoulSaved = false;
        SendInfoMsg("You don't lose exp cause you soul was saved by your soulmate.");
      }
      else
      {
        if(Asda2FactionId == 2 || !RemoveDeathPenalties && !IsAsda2BattlegroundInProgress)
        {
          Asda2Inventory.OnDeath();
          LastExpLooseAmount = Experience / CharacterFormulas.ExpirienceLooseOnDeathPrc;
          if(Level < 20)
            LastExpLooseAmount /= 20 - Level;
          LastExpLooseAmount *= 1 - IntMods[42] / 100;
          if(LastExpLooseAmount < 0)
            LastExpLooseAmount = 0;
          NPC lastKiller1 = LastKiller as NPC;
          Character lastKiller2 = LastKiller as Character;
          Log.Create(Log.Types.ExpChanged, LogSourceType.Character, EntryId)
            .AddAttribute("difference_expirience", LastExpLooseAmount, "")
            .AddAttribute("total_expirience", Experience, "").AddAttribute("source", 0.0, "death").AddAttribute(
              "killer", lastKiller1 == null ? (lastKiller2 == null ? 0.0 : lastKiller2.EntryId) : lastKiller1.Entry.Id,
              lastKiller1 == null ? (lastKiller2 == null ? "unknown_killer!" : lastKiller2.Name) : lastKiller1.Name)
            .Write();
          Experience -= LastExpLooseAmount;
          Character lastKiller3 = LastKiller as Character;
          if(lastKiller3 != null)
          {
            lastKiller3.GainXp((int) (LastExpLooseAmount * (double) CharacterFormulas.KillPkExpPercentOfLoose),
              "killed_pk", false);
            int num = Level - lastKiller3.Level;
            if(num >= -5)
            {
              if(num > 10)
                num = 10;
              if(num < 2)
                num = 2;
              lastKiller3.GuildPoints += num * CharacterFormulas.CharacterKillingGuildPoints;
            }
          }

          SendInfoMsg(string.Format("You have loose {0} exp on death.", LastExpLooseAmount));
        }

        List<Asda2Item> asda2ItemList = new List<Asda2Item>();
        if(Asda2FactionId == 2)
        {
          asda2ItemList.AddRange(Asda2Inventory.Equipment.Where(asda2Item =>
          {
            if(asda2Item != null && (asda2Item.IsWeapon || asda2Item.IsArmor))
              return Utility.Random(0, 100000) < CharacterFormulas.PKItemDropChance;
            return false;
          }).ToList());
          asda2ItemList.AddRange(Asda2Inventory.RegularItems.Where(asda2Item =>
          {
            if(asda2Item != null && asda2Item.ItemId != 20551)
              return Utility.Random(0, 100000) < CharacterFormulas.PKItemDropChance;
            return false;
          }));
          asda2ItemList.AddRange(Asda2Inventory.ShopItems.Where(asda2Item =>
          {
            if(asda2Item != null && asda2Item.ItemId != 20551 &&
               (asda2Item.IsWeapon || asda2Item.IsArmor || asda2Item.Category == Asda2ItemCategory.ItemPackage))
              return Utility.Random(0, 100000) < CharacterFormulas.PKItemDropChance;
            return false;
          }));
        }
        else
        {
          if(IsAsda2BattlegroundInProgress)
            return;
          asda2ItemList.AddRange(Asda2Inventory.RegularItems.Where(asda2Item =>
          {
            if(asda2Item != null && asda2Item.ItemId != 20551)
              return Utility.Random(0, 100000) < CharacterFormulas.ItemDropChance;
            return false;
          }));
          asda2ItemList.AddRange(Asda2Inventory.ShopItems.Where(asda2Item =>
          {
            if(asda2Item != null && asda2Item.ItemId != 20551 && asda2Item.Category == Asda2ItemCategory.ItemPackage)
              return Utility.Random(0, 100000) < CharacterFormulas.ItemDropChance;
            return false;
          }));
        }
      }
    }

    protected internal override void OnResurrect()
    {
      LastExpLooseAmount = 0;
      base.OnResurrect();
      CorpseReleaseFlags &= ~CorpseReleaseFlags.ShowCorpseAutoReleaseTimer;
      if(m_corpse != null)
        Corpse = null;
      m_record.LastResTime = DateTime.Now;
      if(m_Map == null)
        return;
      m_Map.MapTemplate.NotifyPlayerResurrected(this);
    }

    /// <summary>
    /// Resurrects, applies ResurrectionSickness and damages Items, if applicable
    /// </summary>
    public void ResurrectWithConsequences()
    {
      Resurrect();
      int level = Level;
      int sicknessStartLevel = ResurrectionSicknessStartLevel;
    }

    /// <summary>
    /// Marks this Character dead (just died, Corpse not released)
    /// </summary>
    private void MarkDead()
    {
    }

    /// <summary>
    /// Characters become Ghosts after they released the Corpse
    /// </summary>
    private void BecomeGhost()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    protected internal override void OnDamageAction(IDamageAction action)
    {
      base.OnDamageAction(action);
      if(action.Attacker == null)
        return;
      if(m_activePet != null)
        m_activePet.ThreatCollection.AddNewIfNotExisted(action.Attacker);
      if(m_minions != null)
      {
        foreach(NPC minion in m_minions)
          minion.ThreatCollection.AddNewIfNotExisted(action.Attacker);
      }

      bool isPvPing = action.Attacker.IsPvPing;
      Character characterMaster = action.Attacker.CharacterMaster;
      if(!isPvPing || !characterMaster.IsInBattleground)
        return;
      characterMaster.Battlegrounds.Stats.TotalDamage += action.ActualDamage;
    }

    protected override void OnKilled(IDamageAction action)
    {
      base.OnKilled(action);
      if(action.Attacker != null)
      {
        Character attacker = action.Attacker as Character;
        if(attacker != null && attacker.IsAsda2BattlegroundInProgress)
        {
          short points = CharacterFormulas.CalcBattlegrounActPointsOnKill(attacker.Level, Level,
            attacker.BattlegroundActPoints, BattlegroundActPoints);
          ++attacker.BattlegroundKills;
          attacker.GainActPoints(points);
          BattlegroundActPoints -= (short) (points / 2);
          ++BattlegroundDeathes;
          attacker.CurrentBattleGround.GainScores(attacker, points);
          Asda2BattlegroundHandler.SendSomeOneKilledSomeOneResponse(attacker.CurrentBattleGround, (int) attacker.AccId,
            points, attacker.Name, Name);
        }
      }

      m_Map.MapTemplate.NotifyPlayerDied(action);
    }

    public void GainActPoints(short points)
    {
      if(IsInGroup)
      {
        BattlegroundActPoints += (short) (points * (1.0 - CharacterFormulas.BattegroundGroupDisctributePrc));
        short num = (short) (points * (double) CharacterFormulas.BattegroundGroupDisctributePrc / Group.CharacterCount);
        foreach(GroupMember groupMember in Group)
          groupMember.Character.BattlegroundActPoints += num;
      }
      else
        BattlegroundActPoints += points;
    }

    /// <summary>
    /// Finds the item for the given slot. Unequips it if it may not currently be used.
    /// Returns the item to be equipped or null, if invalid.
    /// </summary>
    protected override IAsda2Weapon GetOrInvalidateItem(InventorySlotType type)
    {
      return null;
    }

    protected override void OnHeal(HealAction action)
    {
      base.OnHeal(action);
      Unit attacker = action.Attacker;
      if(!(attacker is Character))
        return;
      Character character = (Character) attacker;
      if(!character.IsInBattleground)
        return;
      character.Battlegrounds.Stats.TotalHealing += action.Value;
    }

    /// <summary>
    /// Spawns the corpse and teleports the dead Character to the nearest SpiritHealer
    /// </summary>
    internal void ReleaseCorpse()
    {
      if(IsAlive || !IsAlive)
        return;
      BecomeGhost();
      Corpse = SpawnCorpse(false, false);
      m_record.CorpseX = m_corpse.Position.X;
      m_record.CorpseY = m_corpse.Position.Y;
      m_record.CorpseZ = m_corpse.Position.Z;
      m_record.CorpseO = m_corpse.Orientation;
      m_record.CorpseMap = m_Map.Id;
      m_corpseReleaseTimer.Stop();
    }

    /// <summary>
    /// Spawns and returns a new Corpse at the Character's current location
    /// </summary>
    /// <param name="bones"></param>
    /// <param name="lootable"></param>
    /// <returns></returns>
    public Corpse SpawnCorpse(bool bones, bool lootable)
    {
      return SpawnCorpse(bones, lootable, m_Map, m_position, m_orientation);
    }

    /// <summary>Spawns and returns a new Corpse at the given location</summary>
    /// <param name="bones"></param>
    /// <param name="lootable"></param>
    /// <returns></returns>
    public Corpse SpawnCorpse(bool bones, bool lootable, Map map, Vector3 pos, float o)
    {
      Corpse corpse = new Corpse(this, pos, o, DisplayId, Facial, Skin, HairStyle, HairColor, FacialHair, GuildId,
        Gender, Race, bones ? CorpseFlags.Bones : CorpseFlags.None,
        lootable ? CorpseDynamicFlags.PlayerLootable : CorpseDynamicFlags.None);
      corpse.Position = pos;
      map.AddObjectLater(corpse);
      return corpse;
    }

    /// <summary>
    /// Tries to teleport to the next SpiritHealer, if there is any.
    /// 
    /// TODO: Graveyards
    /// </summary>
    public void TeleportToNearestGraveyard()
    {
      TeleportToNearestGraveyard(true);
    }

    /// <summary>
    /// Tries to teleport to the next SpiritHealer, if there is any.
    /// 
    /// TODO: Graveyards
    /// </summary>
    public void TeleportToNearestGraveyard(bool allowSameMap)
    {
      if(allowSameMap)
      {
        NPC nearestSpiritHealer = m_Map.GetNearestSpiritHealer(ref m_position);
        if(nearestSpiritHealer != null)
        {
          TeleportTo(nearestSpiritHealer);
          return;
        }
      }

      if(m_Map.MapTemplate.RepopMap != null)
        TeleportTo(m_Map.MapTemplate.RepopMap, m_Map.MapTemplate.RepopPosition);
      else
        TeleportToBindLocation();
    }

    public LevelStatInfo ClassBaseStats
    {
      get { return m_archetype.GetLevelStats((uint) Level); }
    }

    internal void UpdateRest()
    {
      if(m_restTrigger == null)
        return;
      DateTime now = DateTime.Now;
      RestXp += RestGenerator.GetRestXp(now - m_lastRestUpdate, this);
      m_lastRestUpdate = now;
    }

    /// <summary>Gain experience from combat</summary>
    public void GainCombatXp(int experience, INamed killed, bool gainRest)
    {
      if(Level >= MaxLevel || ExpBlock || (IsDead || Level >= MaxLevel) || ExpBlock)
        return;
      int xp = experience;
      NPC npc = killed as NPC;
      if(m_activePet != null && m_activePet.MayGainExperience)
      {
        m_activePet.PetExperience += xp;
        m_activePet.TryLevelUp();
      }

      if(gainRest && RestXp > 0)
      {
        int num = Math.Min(RestXp, experience);
        xp += num;
        RestXp -= num;
      }

      int num1 = XpGenerator.GetXpForlevel(Level + 1) / 4;
      if(xp > num1)
        xp = num1;
      if(xp < 0)
      {
        LogUtil.WarnException("Exp {0} Char {1} kill {2} source {3}", (object) xp, (object) Name, (object) killed,
          (object) experience);
        xp = 1;
      }

      Experience += xp;
      if(npc != null)
        Asda2CharacterHandler.SendExpGainedResponse(npc.UniqIdOnMap, this, xp, true);
      Log.Create(Log.Types.ExpChanged, LogSourceType.Character, EntryId)
        .AddAttribute("difference_expirience", experience, "").AddAttribute("total_expirience", Experience, "")
        .AddAttribute("source", 0.0, "combat").AddAttribute("npc_entry_id", npc == null ? 0.0 : npc.Entry.Id, "")
        .Write();
      TryLevelUp();
    }

    /// <summary>Gain non-combat experience (through quests etc)</summary>
    /// <param name="experience"></param>
    /// <param name="useRest">If true, subtracts the given amount of experience from RestXp and adds it ontop of the given xp</param>
    public void GainXp(int experience, string source, bool useRest = false)
    {
      if(Level >= MaxLevel || ExpBlock || IsDead)
        return;
      if(SoulmateRecord != null)
        SoulmateRecord.OnExpGained(false);
      int xp = experience;
      if(useRest && RestXp > 0)
      {
        int num = Math.Min(RestXp, experience);
        xp += num;
        RestXp -= num;
      }

      int num1 = XpGenerator.GetXpForlevel(Level + 1) / 4;
      if(xp > num1)
        xp = num1;
      Experience += xp;
      Asda2CharacterHandler.SendExpGainedResponse(0, this, xp, false);
      Log.Create(Log.Types.ExpChanged, LogSourceType.Character, EntryId)
        .AddAttribute("difference_expirience", experience, "").AddAttribute("total_expirience", Experience, "")
        .AddAttribute(nameof(source), 0.0, source).Write();
      TryLevelUp();
    }

    internal bool TryLevelUp()
    {
      int level = Level;
      int experience = Experience;
      int num = NextLevelXP;
      bool flag = false;
      while(experience >= num && level < MaxLevel)
      {
        ++level;
        experience -= num;
        num = XpGenerator.GetXpForlevel(level + 1);
        flag = true;
      }

      if(!flag)
        return false;
      Experience = experience;
      NextLevelXP = num;
      Log.Create("level_up", LogSourceType.Character, EntryId).AddAttribute("from", Level, "")
        .AddAttribute("to", level, "").AddAttribute("total exp", Experience, "").Write();
      Level = level;
      return true;
    }

    protected override void OnLevelChanged()
    {
      base.OnLevelChanged();
      if(Level >= 10 && Archetype.ClassId == ClassId.NoClass)
        SendInfoMsg("Используйте меню внешней программы для смены профессии");
      if(Archetype.ClassId != ClassId.NoClass)
      {
        if(Level >= 30 && RealProffLevel == 1)
          SetClass(2, (int) Archetype.ClassId);
        else if(Level >= 50 && RealProffLevel == 2)
          SetClass(3, (int) Archetype.ClassId);
        else if(Level >= 70 && RealProffLevel == 3)
          SetClass(4, (int) Archetype.ClassId);
      }

      GuildPoints += CharacterFormulas.LevelupingGuildPointsPerLevel * Level;
      IList<PerlevelItemBonusTemplateItem> bonusItems = PerLevelItemBonusManager.GetBonusItemList((byte) Level,
        (byte) Record.RebornCount, Record.PrivatePerLevelItemBonusTemplateId);
      ServerApp<RealmServer>.IOQueue.AddMessage(() =>
      {
        foreach(PerlevelItemBonusTemplateItem bonusTemplateItem in bonusItems)
          Asda2Inventory.AddDonateItem(Asda2ItemMgr.GetTemplate(bonusTemplateItem.ItemId), bonusTemplateItem.Amount,
            "~Leveling system~", true);
      });
      InitGlyphsForLevel();
      int level = Level;
      Experience = 0;
      if(m_activePet != null)
      {
        if(!m_activePet.IsHunterPet || m_activePet.Level > level)
          m_activePet.Level = level;
        else if(level - PetMgr.MaxHunterPetLevelDifference > m_activePet.Level)
          m_activePet.Level = level - PetMgr.MaxHunterPetLevelDifference;
      }

      double num = CharacterFormulas.CalcStatBonusPerLevel(level, Record.RebornCount);
      FreeStatPoints += (int) num;
      Log.Create("gain_stats", LogSourceType.Character, EntryId).AddAttribute("source", 0.0, "on lvl up")
        .AddAttribute("difference_stat_points", num, "").AddAttribute("total_points", FreeStatPoints, "")
        .AddAttribute("level", level, "").Write();
      SendSystemMessage(string.Format("You have {0} free stat points. Enter \"#HowToAddStats\" to see help.",
        FreeStatPoints));
      ModStatsForLevel(level);
      m_auras.ReapplyAllAuras();
      m_achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.ReachLevel, (uint) Level, 0U, null);
      this.UpdateDodgeChance();
      this.UpdateBlockChance();
      this.UpdateCritChance();
      this.UpdateAllAttackPower();
      Action<Character> levelChanged = LevelChanged;
      if(levelChanged != null)
        levelChanged(this);
      Map.CallDelayed(100, () => Asda2CharacterHandler.SendLvlUpResponse(this));
      Map.CallDelayed(500, () => Asda2CharacterHandler.SendUpdateStatsResponse(Client));
      SaveLater();
    }

    public int FreeStatPoints
    {
      get { return Record.FreeStatPoints; }
      set { Record.FreeStatPoints = value; }
    }

    public void ModStatsForLevel(int level)
    {
      BasePower = CharacterFormulas.GetBaseMana(Level, Class);
      BaseHealth = CharacterFormulas.GetBaseHealth(Level, Class);
      SetInt32(UnitFields.HEALTH, MaxHealth);
      UpdateAsda2Agility();
      UpdateAsda2Stamina();
      UpdateAsda2Luck();
      UpdateAsda2Spirit();
      UpdateAsda2Intellect();
      UpdateAsda2Strength();
      UpdatePlayedTime();
      if(TotalPlayTime / 60U / 60U > 25U)
        Map.CallDelayed(500, () => GetTitle(Asda2TitleId.Player36));
      if(TotalPlayTime / 60U / 60U > 150U)
        Map.CallDelayed(500, () => GetTitle(Asda2TitleId.Obsessed37));
      if(TotalPlayTime / 60U / 60U > 1000U)
        Map.CallDelayed(500, () => GetTitle(Asda2TitleId.Veteran38));
      LevelPlayTime = 0U;
    }

    /// <summary>Adds the given language</summary>
    public void AddLanguage(ChatLanguage lang)
    {
      AddLanguage(LanguageHandler.GetLanguageDescByType(lang));
    }

    public void AddLanguage(LanguageDescription desc)
    {
      if(!Spells.Contains((uint) desc.SpellId))
        Spells.AddSpell(desc.SpellId);
      if(m_skills.Contains(desc.SkillId))
        return;
      m_skills.Add(desc.SkillId, 300U, 300U, true);
    }

    /// <summary>
    /// Returns whether the given language can be understood by this Character
    /// </summary>
    public bool CanSpeak(ChatLanguage language)
    {
      return KnownLanguages.Contains(language);
    }

    public void SendAuctionMsg(string msg)
    {
      SendMessage("~Auction~", msg, Color.DeepPink);
    }

    public void SendCraftingMsg(string msg)
    {
      SendMessage("~Craft~", msg, Color.BurlyWood);
    }

    public void SendInfoMsg(string msg)
    {
      SendMessage("~Info~", msg, Color.Coral);
    }

    public void SendErrorMsg(string msg)
    {
      SendMessage("~Error~", msg, Color.Red);
      log.Warn(string.Format("error msg to character : {0}.\r\n{1}", Name, msg));
    }

    public void SendWarnMsg(string msg)
    {
      SendMessage("~Attention~", msg, Color.Red);
    }

    public void SendWarMsg(string msg)
    {
      SendMessage("~War~", msg, Color.OrangeRed);
    }

    public void SendMailMsg(string msg)
    {
      SendMessage("~Mail~", msg, Color.Gainsboro);
    }

    public void SendMessage(string sender, string msg, Color c)
    {
      ChatMgr.SendMessage(this, sender, msg, c);
    }

    public void SendMessage(string message)
    {
      ChatMgr.SendSystemMessage(this, message);
      ChatMgr.ChatNotify(null, message, ChatLanguage.Universal, ChatMsgType.System, this);
    }

    public void SendMessage(IChatter sender, string message)
    {
      ChatMgr.SendWhisper(sender, this, message);
    }

    /// <summary>Sends a message to the client.</summary>
    public void SendSystemMessage(RealmLangKey key, params object[] args)
    {
      ChatMgr.SendSystemMessage(this, RealmLocalizer.Instance.Translate(Locale, key, args));
    }

    /// <summary>Sends a message to the client.</summary>
    public void SendSystemMessage(string msg)
    {
      ChatMgr.SendSystemMessage(this, msg);
    }

    /// <summary>Sends a message to the client.</summary>
    public void SendSystemMessage(string msgFormat, params object[] args)
    {
      ChatMgr.SendSystemMessage(this, string.Format(msgFormat, args));
    }

    public void Notify(RealmLangKey key, params object[] args)
    {
      Notify(RealmLocalizer.Instance.Translate(Locale, key, args));
    }

    /// <summary>Flashes a notification in the middle of the screen</summary>
    public void Notify(string msg, params object[] args)
    {
      MiscHandler.SendNotification(this, string.Format(msg, args));
    }

    public void SayGroup(string msg)
    {
      this.SayGroup(SpokenLanguage, msg);
    }

    public void SayGroup(string msg, params object[] args)
    {
      SayGroup(string.Format(msg, args));
    }

    public override void Say(float radius, string msg)
    {
      this.SayYellEmote(ChatMsgType.Say, SpokenLanguage, msg, radius);
    }

    public override void Yell(float radius, string msg)
    {
      this.SayYellEmote(ChatMsgType.Yell, SpokenLanguage, msg, radius);
    }

    public override void Emote(float radius, string msg)
    {
      this.SayYellEmote(ChatMsgType.Emote, SpokenLanguage, msg, radius);
    }

    /// <summary>
    /// Called whenever this Character interacts with any WorldObject
    /// </summary>
    /// <param name="obj"></param>
    public void OnInteract(WorldObject obj)
    {
      StandState = StandState.Stand;
      if(!(obj is NPC))
        return;
      NPC npc = (NPC) obj;
      Reputations.OnTalkWith(npc);
      npc.Entry.NotifyInteracting(npc, this);
    }

    /// <summary>Opens this character's bankbox</summary>
    public void OpenBank(WorldObject banker)
    {
    }

    /// <summary>Tries to bind this Character to the given NPC.</summary>
    /// <returns>whether the given NPC is an actual InnKeeper and this char could be bound to that Inn.</returns>
    public bool TryBindTo(NPC innKeeper)
    {
      OnInteract(innKeeper);
      if(!(innKeeper.BindPoint != NamedWorldZoneLocation.Zero) || !innKeeper.CheckVendorInteraction(this))
        return false;
      BindTo(innKeeper);
      return true;
    }

    /// <summary>
    /// Binds this Character to that Location and will teleport him/her whenever the Hearthston is used.
    /// Adds a new HearthStone if the Character doesn't have one.
    /// Make sure that the given NPC is an actual InnKeeper and has a valid BindPoint (else use <c>TryBindTo</c> instead).
    /// </summary>
    public void BindTo(NPC binder)
    {
    }

    public void BindTo(WorldObject binder, IWorldZoneLocation location)
    {
      m_bindLocation = location;
    }

    /// <summary>Gets the quest log.</summary>
    /// <value>The quest log.</value>
    public QuestLog QuestLog
    {
      get { return m_questLog; }
    }

    /// <summary>
    /// The <see cref="P:WCell.RealmServer.Entities.Character.Duel" /> this Character is currently engaged in (or null if not dueling)
    /// </summary>
    public Duel Duel { get; internal set; }

    /// <summary>
    /// The opponent that this Character is currently dueling with (or null if not dueling)
    /// </summary>
    public Character DuelOpponent { get; internal set; }

    /// <summary>
    /// whether this Character is currently dueling with someone else
    /// </summary>
    public bool IsDueling
    {
      get
      {
        if(Duel != null)
          return Duel.IsActive;
        return false;
      }
    }

    public override bool IsFriendlyWith(IFactionMember opponent)
    {
      if(IsAlliedWith(opponent))
        return true;
      Faction faction = opponent.Faction;
      Reputation reputation = m_reputations[faction.ReputationIndex];
      if(reputation != null)
        return reputation.Standing >= Standing.Friendly;
      return m_faction.IsFriendlyTowards(faction);
    }

    public override bool IsAtLeastNeutralWith(IFactionMember opponent)
    {
      if(IsFriendlyWith(opponent))
        return true;
      Faction faction = opponent.Faction;
      Reputation reputation = m_reputations[faction.ReputationIndex];
      if(reputation != null)
        return reputation.Standing >= Standing.Neutral;
      return m_faction.Neutrals.Contains(faction);
    }

    public override bool IsHostileWith(IFactionMember opponent)
    {
      if(ReferenceEquals(opponent, this) || opponent is Unit && ((WorldObject) opponent).Master == this)
        return false;
      if(opponent is NPC)
        return true;
      if(opponent is Character)
        return CanPvP((Character) opponent);
      Faction faction = opponent.Faction;
      if(opponent is NPC && faction.Neutrals.Contains(m_faction))
        return ((NPC) opponent).ThreatCollection.HasAggressor(this);
      if(m_faction.Friends.Contains(faction) || !m_faction.Enemies.Contains(faction))
        return false;
      return m_reputations.CanAttack(faction);
    }

    public override bool MayAttack(IFactionMember opponent)
    {
      if(ReferenceEquals(opponent, this) || opponent is Unit && ((WorldObject) opponent).Master == this)
        return false;
      if(opponent is Character)
        return CanPvP((Character) opponent);
      return opponent is NPC;
    }

    public bool CanPvP(Character chr)
    {
      if(!chr.IsAlive || chr.Map == null)
        return false;
      if(chr.Map.MapTemplate.IsAsda2FightingMap && chr.Asda2FactionId != -1 && chr.Asda2FactionId != Asda2FactionId)
        return true;
      return CanDuel(chr);
    }

    private bool CanDuel(Character chr)
    {
      return IsAsda2Dueling && chr == Asda2DuelingOponent;
    }

    /// <summary>
    /// One can only cast beneficial spells on people that we are allied with
    /// </summary>
    /// <param name="opponent"></param>
    /// <returns></returns>
    public override bool IsAlliedWith(IFactionMember opponent)
    {
      if(ReferenceEquals(opponent, this) || opponent is Unit && ((WorldObject) opponent).Master == this)
        return true;
      if(!(opponent is Character) && opponent is WorldObject)
        opponent = ((WorldObject) opponent).Master;
      if(opponent is Character)
      {
        if(IsInBattleground)
          return Battlegrounds.Team == ((Character) opponent).Battlegrounds.Team;
        Group group = Group;
        if(group != null && ((Character) opponent).Group == group && DuelOpponent == null)
          return ((Character) opponent).DuelOpponent == null;
      }

      return false;
    }

    public override bool IsInSameDivision(IFactionMember opponent)
    {
      if(ReferenceEquals(opponent, this) || opponent is Unit && ((WorldObject) opponent).Master == this)
        return true;
      if(!(opponent is Character) && opponent is WorldObject)
        opponent = ((WorldObject) opponent).Master;
      if(opponent is Character)
      {
        if(IsInBattleground)
          return Battlegrounds.Team == ((Character) opponent).Battlegrounds.Team;
        SubGroup subGroup = SubGroup;
        if(subGroup != null && ((Character) opponent).SubGroup == subGroup && DuelOpponent == null)
          return ((Character) opponent).DuelOpponent == null;
      }

      return false;
    }

    public override void OnAttack(DamageAction action)
    {
      if(action.Victim is NPC && m_dmgBonusVsCreatureTypePct != null)
      {
        int num = m_dmgBonusVsCreatureTypePct[(int) ((NPC) action.Victim).Entry.Type];
        if(num != 0)
          action.Damage += (num * action.Damage + 50) / 100;
      }

      base.OnAttack(action);
    }

    protected override void OnEnterCombat()
    {
      CancelTransports();
      if(CurrentCapturingPoint == null)
        return;
      CurrentCapturingPoint.StopCapture();
    }

    protected override bool CheckCombatState()
    {
      if(m_isFighting)
        return base.CheckCombatState();
      if(NPCAttackerCount == 0 && (m_activePet == null || m_activePet.NPCAttackerCount == 0) &&
         !m_auras.HasHarmfulAura())
      {
        if(m_minions != null)
        {
          foreach(Unit minion in m_minions)
          {
            if(minion.NPCAttackerCount > 0)
              return base.CheckCombatState();
          }
        }

        IsInCombat = false;
      }

      return false;
    }

    public override int AddHealingModsToAction(int healValue, SpellEffect effect, DamageSchool school)
    {
      healValue += (int) (healValue * (double) HealingDoneModPct / 100.0);
      healValue += HealingDoneMod;
      if(effect != null)
        healValue = Auras.GetModifiedInt(SpellModifierType.SpellPower, effect.Spell, healValue);
      return healValue;
    }

    public override int GetGeneratedThreat(int dmg, DamageSchool school, SpellEffect effect)
    {
      int num = base.GetGeneratedThreat(dmg, school, effect);
      if(effect != null)
        num = Auras.GetModifiedInt(SpellModifierType.Threat, effect.Spell, num);
      return num;
    }

    public override float CalcCritDamage(float dmg, Unit victim, SpellEffect effect)
    {
      dmg = base.CalcCritDamage(dmg, victim, effect);
      if(effect != null)
        return Auras.GetModifiedFloat(SpellModifierType.CritDamage, effect.Spell, dmg);
      return dmg;
    }

    /// <summary>Change target and/or amount of combo points</summary>
    public override bool ModComboState(Unit target, int amount)
    {
      if(!base.ModComboState(target, amount))
        return false;
      CombatHandler.SendComboPoints(this);
      return true;
    }

    /// <summary>
    /// Whether this Character will automatically pass on loot rolls.
    /// </summary>
    public bool PassOnLoot { get; set; }

    /// <summary>
    /// The LooterEntry represents this Character's current loot status
    /// </summary>
    public Asda2LooterEntry LooterEntry
    {
      get { return m_looterEntry ?? (m_looterEntry = new Asda2LooterEntry(this)); }
    }

    /// <summary>whether this Character is currently looting something</summary>
    public bool IsLooting
    {
      get
      {
        if(m_looterEntry != null)
          return m_looterEntry.Loot != null;
        return false;
      }
    }

    /// <summary>
    /// Cancels looting (if this Character is currently looting something)
    /// </summary>
    public void CancelLooting()
    {
      if(m_looterEntry == null)
        return;
      m_looterEntry.Release();
    }

    public SummonRequest SummonRequest
    {
      get { return m_summonRequest; }
    }

    /// <summary>
    /// May be executed from outside of this Character's map's context
    /// </summary>
    public void StartSummon(ISummoner summoner)
    {
      StartSummon(summoner, SummonRequest.DefaultTimeout);
    }

    /// <summary>
    /// May be executed from outside of this Character's map's context
    /// </summary>
    /// <param name="summoner"></param>
    /// <param name="timeoutSeconds"></param>
    public void StartSummon(ISummoner summoner, int timeoutSeconds)
    {
    }

    /// <summary>Cancels a current summon request</summary>
    public void CancelSummon(bool notify)
    {
      if(m_summonRequest == null)
        return;
      if(m_summonRequest.Portal != null && m_summonRequest.Portal.IsInWorld)
        m_summonRequest.Portal.Delete();
      int num = notify ? 1 : 0;
      m_summonRequest = null;
    }

    public override int GetBasePowerRegen()
    {
      return RegenerationFormulas.GetPowerRegen(this);
    }

    public void ActivateAllTaxiNodes()
    {
      for(int index = 0; index < TaxiMgr.PathNodesById.Length; ++index)
      {
        PathNode node = TaxiMgr.PathNodesById[index];
        if(node != null)
        {
          TaxiNodes.Activate(node);
          SendSystemMessage("Activated Node: " + node);
        }
      }
    }

    public override void SetZone(Zone newZone)
    {
      base.SetZone(newZone);
    }

    public override void CancelAllActions()
    {
      base.CancelAllActions();
      if(m_target != null)
        ClearTarget();
      if(TradeWindow != null)
        TradeWindow.Cancel(TradeStatus.Cancelled);
      if(Asda2TradeWindow != null)
        Asda2TradeWindow.CancelTrade();
      if(CurrentCapturingPoint != null)
        CurrentCapturingPoint.StopCapture();
      CancelTransports();
    }

    public void CancelTransports()
    {
      MountId = -1;
      TransportItemId = -1;
    }

    public void ClearTarget()
    {
      Target = null;
    }

    public override int GetPowerCost(DamageSchool school, Spell spell, int cost)
    {
      cost = base.GetPowerCost(school, spell, cost);
      cost = Auras.GetModifiedInt(SpellModifierType.PowerCost, spell, cost);
      return cost;
    }

    public SpecProfile CurrentSpecProfile
    {
      get { return SpecProfiles[m_talents.CurrentSpecIndex]; }
    }

    /// <summary>Talent specs</summary>
    public SpecProfile[] SpecProfiles { get; protected internal set; }

    public void ApplyTalentSpec(int no)
    {
      SpecProfiles.Get(no);
    }

    public void InitGlyphsForLevel()
    {
      foreach(KeyValuePair<uint, GlyphSlotEntry> glyphSlot in GlyphInfoHolder.GlyphSlots)
      {
        if(glyphSlot.Value.Order != 0U)
          SetGlyphSlot((byte) (glyphSlot.Value.Order - 1U), glyphSlot.Value.Id);
      }

      int level = Level;
      uint num = 0;
      if(level >= 15)
        num |= 3U;
      if(level >= 30)
        num |= 8U;
      if(level >= 50)
        num |= 4U;
      if(level >= 70)
        num |= 16U;
      if(level >= 80)
        num |= 32U;
      Glyphs_Enable = num;
    }

    public void ApplyGlyph(byte slot, GlyphPropertiesEntry gp)
    {
      RemoveGlyph(slot);
      SpellCast.Trigger(SpellHandler.Get(gp.SpellId), (WorldObject) this);
      SetGlyph(slot, gp.Id);
      CurrentSpecProfile.GlyphIds[slot] = gp.Id;
      TalentHandler.SendTalentGroupList(m_talents);
    }

    public void RemoveGlyph(byte slot)
    {
      uint glyph = GetGlyph(slot);
      if(glyph == 0U)
        return;
      Auras.Remove(SpellHandler.Get(GlyphInfoHolder.GetPropertiesEntryForGlyph(glyph).SpellId));
      CurrentSpecProfile.GlyphIds[slot] = 0U;
      SetGlyph(slot, 0U);
    }

    public Character InstanceLeader
    {
      get { return this; }
    }

    public InstanceCollection InstanceLeaderCollection
    {
      get { return Instances; }
    }

    public bool HasInstanceCollection
    {
      get { return m_InstanceCollection != null; }
    }

    /// <summary>Auto-created if not already existing</summary>
    public InstanceCollection Instances
    {
      get
      {
        if(m_InstanceCollection == null)
          m_InstanceCollection = new InstanceCollection(this);
        return m_InstanceCollection;
      }
      set { m_InstanceCollection = value; }
    }

    public void ForeachInstanceHolder(Action<InstanceCollection> callback)
    {
      callback(Instances);
    }

    public BaseInstance GetActiveInstance(MapTemplate mapTemplate)
    {
      Map map = m_Map;
      if(map != null && map.Id == map.Id)
        return map as BaseInstance;
      return m_InstanceCollection?.GetActiveInstance(mapTemplate);
    }

    /// <summary>
    /// Whether this Character is in a Battleground at the moment
    /// </summary>
    public bool IsInBattleground
    {
      get
      {
        if(m_bgInfo != null)
          return m_bgInfo.Team != null;
        return false;
      }
    }

    /// <summary>
    /// Represents all <see cref="T:WCell.RealmServer.Battlegrounds.Battleground" />-related information of this Character
    /// </summary>
    public BattlegroundInfo Battlegrounds
    {
      get
      {
        if(m_bgInfo == null)
          m_bgInfo = new BattlegroundInfo(this);
        return m_bgInfo;
      }
    }

    /// <summary>
    /// Is called when the Character kills an Honorable target.
    /// </summary>
    /// <param name="victim">The Honorable character killed.</param>
    internal void OnHonorableKill(IDamageAction action)
    {
      Character victim = (Character) action.Victim;
      uint num = CalcHonorForKill(victim);
      if(num == 0U)
        return;
      if(IsInBattleground)
      {
        BattlegroundTeam team = m_bgInfo.Team;
        BattlegroundStats stats = victim.Battlegrounds.Stats;
        if(team == victim.Battlegrounds.Team || stats == null || stats.Deaths > BattlegroundMgr.MaxHonorableDeaths)
          return;
        ++m_bgInfo.Stats.HonorableKills;
        team.DistributeSharedHonor(this, victim, num);
      }
      else if(Group != null)
      {
        if(Faction.Group == victim.Faction.Group)
          return;
        Group.DistributeGroupHonor(this, victim, num);
      }
      else
      {
        GiveHonorPoints(num);
        ++KillsToday;
        ++LifetimeHonorableKills;
        HonorHandler.SendPVPCredit(this, num * 10U, victim);
      }

      if(m_zone == null)
        return;
      m_zone.Template.OnHonorableKill(this, victim);
    }

    private uint CalcHonorForKill(Character victim)
    {
      if(victim == this || !victim.YieldsXpOrHonor)
        return 0;
      int level1 = Level;
      int level2 = victim.Level;
      int maxLvlDiff = BattlegroundMgr.MaxLvlDiff;
      int num1 = BattlegroundMgr.MaxHonor - 1;
      if(num1 < 0)
        num1 = 0;
      int num2 = level1 - level2 + maxLvlDiff;
      if(num2 < 0)
        return 0;
      return (uint) Math.Round(num1 / (2f * maxLvlDiff) * (double) num2 + 1.0);
    }

    public void GiveHonorPoints(uint points)
    {
      HonorPoints += points;
      HonorToday += points;
    }

    public uint MaxPersonalArenaRating
    {
      get { return 0; }
    }

    public void TogglePvPFlag()
    {
      SetPvPFlag(!PlayerFlags.HasFlag(PlayerFlags.PVP));
    }

    public void SetPvPFlag(bool state)
    {
      if(state)
      {
        UpdatePvPState(true, PvPEndTime != null && PvPEndTime.IsRunning);
        PlayerFlags |= PlayerFlags.PVP;
      }
      else
      {
        if(Zone == null || Zone.Template.IsHostileTo(this) || !PvPState.HasFlag(PvPState.PVP))
          return;
        SetPvPResetTimer(false);
      }
    }

    public void UpdatePvPState(bool state, bool overridden = false)
    {
      if(!state || overridden)
      {
        SetPvPState(state);
        ClearPvPResetTimer();
      }
      else if(PvPEndTime != null && PvPEndTime.IsRunning)
        SetPvPResetTimer(true);
      else
        SetPvPState(true);
    }

    private void SetPvPResetTimer(bool overridden = false)
    {
      if(PvPEndTime == null)
        PvPEndTime = new TimerEntry(dt => OnPvPTimerEnded());
      if(!PvPEndTime.IsRunning || overridden)
        PvPEndTime.Start(300000);
      IsPvPTimerActive = true;
    }

    private void ClearPvPResetTimer()
    {
      if(PvPEndTime != null)
        PvPEndTime.Stop();
      IsPvPTimerActive = false;
    }

    private void OnPvPTimerEnded()
    {
      PlayerFlags &= ~PlayerFlags.PVP;
      IsPvPTimerActive = false;
      SetPvPState(false);
    }

    private void SetPvPState(bool state)
    {
      if(ActivePet != null)
      {
        if(state)
        {
          PvPState = PvPState.PVP;
          ActivePet.PvPState = PvPState.PVP;
        }
        else
        {
          PvPState &= ~PvPState.PVP;
          ActivePet.PvPState &= ~PvPState.PVP;
        }
      }
      else if(state)
        PvPState = PvPState.PVP;
      else
        PvPState &= ~PvPState.PVP;
    }

    /// <summary>Calculates the price of a purchase in a berber shop.</summary>
    /// <param name="newstyle"></param>
    /// <param name="newcolor"></param>
    /// <param name="newfacial"></param>
    /// <returns>The total price.</returns>
    public uint CalcBarberShopCost(byte newStyle, byte newColor, byte newFacial)
    {
      int level = Level;
      byte hairStyle = HairStyle;
      byte hairColor = HairColor;
      byte facialHair = FacialHair;
      if(hairStyle == newStyle && hairColor == newColor && facialHair == newFacial)
        return 0;
      float barberShopCost = GameTables.BarberShopCosts[level - 1];
      if(barberShopCost == 0.0)
        return uint.MaxValue;
      float num = 0.0f;
      if(hairStyle != newStyle)
        num += barberShopCost;
      else if(hairColor != newColor)
        num += barberShopCost * 0.5f;
      if(facialHair != newFacial)
        num += barberShopCost * 0.75f;
      return (uint) num;
    }

    public BaseCommand<RealmServerCmdArgs> SelectedCommand
    {
      get { return m_ExtraInfo?.m_selectedCommand; }
      set
      {
        ExtraInfo extraInfo = m_ExtraInfo;
        if(extraInfo == null)
          return;
        extraInfo.m_selectedCommand = value;
      }
    }

    public override LinkedList<WaypointEntry> Waypoints
    {
      get { return null; }
    }

    public override NPCSpawnPoint SpawnPoint
    {
      get { return null; }
    }

    /// <summary>
    /// The ticket that is currently being handled by this <see cref="T:WCell.RealmServer.Help.Tickets.ITicketHandler" />
    /// </summary>
    public Ticket HandlingTicket
    {
      get { return m_ExtraInfo?.m_handlingTicket; }
      set
      {
        ExtraInfo extraInfo = ExtraInfo;
        if(extraInfo == null)
          return;
        extraInfo.m_handlingTicket = value;
      }
    }

    public bool MayHandle(Ticket ticket)
    {
      ITicketHandler handler = ticket.m_handler;
      if(handler != null)
        return handler.Role <= Role;
      return true;
    }

    public int CharacterCount
    {
      get { return 1; }
    }

    public bool AutoLoot { get; set; }

    public byte FriendShipPoints
    {
      get { return SoulmateRecord.FriendShipPoints; }
    }

    public byte MountBoxSize
    {
      get { return (byte) (6 * (1 + Record.MountBoxExpands)); }
    }

    public void ForeachCharacter(Action<Character> callback)
    {
      callback(this);
    }

    public Character[] GetAllCharacters()
    {
      return new Character[1] { this };
    }

    public void Send(RealmPacketOut packet, bool addEnd = false)
    {
      m_client.Send(packet, addEnd);
    }

    public void Send(byte[] packet)
    {
      m_client.Send(packet);
    }

    public override string ToString()
    {
      return Name + " (ID: " + EntityId + ", Account: " + Account + ")";
    }

    public void SendNotifyMsg(string msg)
    {
      ChatMgr.SendSystemChatResponse(Client, msg);
    }

    public void YouAreFuckingCheater(string reason = "", int banPoints = 1)
    {
      Record.BanPoints += banPoints;
      Log.Create(Log.Types.Cheating, LogSourceType.Character, EntryId).AddAttribute(nameof(reason), 0.0, reason)
        .AddAttribute("difference_ban_points", banPoints, "").AddAttribute("total_ban_points", Record.BanPoints, "")
        .Write();
      log.Info(string.Format("{0} is trying to cheat! [{1}][{2}/{3} ban points]", (object) Name, (object) reason,
        (object) Record.BanPoints, (object) PointsToGetBan));
      if(Record.BanPoints <= PointsToGetBan)
        return;
      Account.SetAccountActive(false, DateTime.MaxValue);
    }

    public void SendOnlyEnglishCharactersAllowed(string where)
    {
      SendInfoMsg(string.Format("Sorry, only english characters allowed in {0}.", where));
    }

    /// <summary>Action-information of previously summoned pets</summary>
    public List<SummonedPetRecord> SummonedPetRecords
    {
      get
      {
        if(m_SummonedPetRecords == null)
          m_SummonedPetRecords = new List<SummonedPetRecord>();
        return m_SummonedPetRecords;
      }
    }

    /// <summary>
    /// All minions that belong to this Character, excluding actual Pets.
    /// Might be null.
    /// </summary>
    public NPCCollection Minions
    {
      get { return m_minions; }
    }

    /// <summary>All created Totems (might be null)</summary>
    public NPC[] Totems
    {
      get { return m_totems; }
    }

    /// <summary>
    /// Currently active Pet of this Character (the one with the action bar)
    /// </summary>
    public NPC ActivePet
    {
      get { return m_activePet; }
      set
      {
        if(value == m_activePet)
          return;
        if(m_activePet != null)
          m_activePet.Delete();
        if(IsPetActive = value != null)
        {
          value.PetRecord.IsActivePet = true;
          m_record.PetEntryId = value.Entry.NPCId;
          m_activePet = value;
          if(m_activePet.PetRecord.ActionButtons == null)
            m_activePet.PetRecord.ActionButtons = m_activePet.BuildPetActionBar();
          AddPostUpdateMessage(() =>
          {
            if(m_activePet != value || !m_activePet.IsInContext)
              return;
            PetHandler.SendSpells(this, m_activePet, PetAction.Follow);
            PetHandler.SendPetGUIDs(this);
            m_activePet.OnBecameActivePet();
          });
        }
        else
        {
          Summon = EntityId.Zero;
          if(Charm == m_activePet)
            Charm = null;
          m_record.PetEntryId = 0;
          PetHandler.SendEmptySpells(this);
          PetHandler.SendPetGUIDs(this);
          m_activePet = null;
        }
      }
    }

    /// <summary>
    /// Lets the ActivePet appear/disappear (if this Character has one)
    /// </summary>
    public bool IsPetActive
    {
      get { return m_record.IsPetActive; }
      set
      {
        if(value)
        {
          if(m_activePet != null && !m_activePet.IsInWorld)
            PlaceOnTop(ActivePet);
        }
        else
          m_activePet.RemoveFromMap();

        m_record.IsPetActive = value;
      }
    }

    /// <summary>Dismisses the current pet</summary>
    public void DismissActivePet()
    {
      if(m_activePet == null)
        return;
      if(m_activePet.IsSummoned)
        AbandonActivePet();
      else
        IsPetActive = false;
    }

    /// <summary>ActivePet is about to be abandoned</summary>
    public void AbandonActivePet()
    {
      if(m_activePet.IsInWorld && m_activePet.IsHunterPet && !m_activePet.PetRecord.IsStabled)
      {
        m_activePet.RejectMaster();
        m_activePet.IsDecaying = true;
      }
      else
        m_activePet.Delete();
    }

    /// <summary>
    /// Simply unsets the currently active pet without deleting or abandoning it.
    /// Make sure to take care of the pet when calling this method.
    /// </summary>
    private void UnsetActivePet()
    {
      NPC activePet = m_activePet;
    }

    public void Possess(int duration, Unit target, bool controllable = true, bool sendPetActionsWithSpells = true)
    {
      if(target == null)
        return;
      if(target is NPC)
      {
        Enslave((NPC) target, duration);
        target.Charmer = this;
        Charm = target;
        target.Brain.State = BrainState.Idle;
        if(sendPetActionsWithSpells)
          PetHandler.SendSpells(this, (NPC) target, PetAction.Stay);
        else
          PetHandler.SendVehicleSpells(this, (NPC) target);
        SetMover(target, controllable);
        target.UnitFlags |= UnitFlags.Possessed;
      }
      else if(target is Character)
      {
        PetHandler.SendPlayerPossessedPetSpells(this, (Character) target);
        SetMover(target, controllable);
      }

      Observing = target;
      FarSight = target.EntityId;
    }

    public void UnPossess(Unit target)
    {
      Observing = null;
      SetMover(this, true);
      ResetMover();
      FarSight = EntityId.Zero;
      PetHandler.SendEmptySpells(this);
      Charm = null;
      if(target == null)
        return;
      target.Charmer = null;
      target.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable |
                          UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 |
                          UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 |
                          UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat |
                          UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 |
                          UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat |
                          UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared |
                          UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted |
                          UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 |
                          UnitFlags.Flag_31_0x80000000;
      if(!(target is NPC))
        return;
      target.Brain.EnterDefaultState();
      ((NPC) target).RemainingDecayDelayMillis = 1;
    }

    /// <summary>Amount of stabled pets + active pet (if any)</summary>
    public int TotalPetCount
    {
      get { return (m_activePet != null ? 1 : 0) + (m_StabledPetRecords != null ? m_StabledPetRecords.Count : 0); }
    }

    public bool HasStabledPets
    {
      get
      {
        if(m_StabledPetRecords != null)
          return m_StabledPetRecords.Count > 0;
        return false;
      }
    }

    public List<PermanentPetRecord> StabledPetRecords
    {
      get
      {
        if(m_StabledPetRecords == null)
          m_StabledPetRecords = new List<PermanentPetRecord>(PetMgr.MaxStableSlots);
        return m_StabledPetRecords;
      }
    }

    /// <summary>
    /// Indicate whether the Character can get a PetSpell bar to
    /// control the given NPC
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public bool CanControl(NPCEntry npc)
    {
      if(!npc.IsTamable)
        return false;
      if(npc.IsExoticPet)
        return CanControlExoticPets;
      return true;
    }

    public NPC SpawnPet(IPetRecord record)
    {
      return SpawnPet(record, ref m_position, 0);
    }

    public NPC SpawnPet(IPetRecord record, int duration)
    {
      return SpawnPet(record, ref m_position, duration);
    }

    /// <summary>Tries to spawn a new summoned Pet for the Character.</summary>
    /// <param name="entry"></param>
    /// <param name="position"></param>
    /// <returns>null, if the Character already has that kind of Pet.</returns>
    public NPC SpawnPet(NPCEntry entry, ref Vector3 position, int durationMillis)
    {
      return SpawnPet(GetOrCreateSummonedPetRecord(entry), ref position, durationMillis);
    }

    public NPC SpawnPet(IPetRecord record, ref Vector3 position, int duration)
    {
      NPC minion = CreateMinion(record.Entry, duration);
      minion.PetRecord = record;
      minion.Position = position;
      if(record.PetNumber != 0U)
        minion.EntityId = new EntityId(NPCMgr.GenerateUniqueLowId(), record.PetNumber, HighId.UnitPet);
      InitializeMinion(minion);
      if(IsPetActive)
        m_Map.AddObject(minion);
      return minion;
    }

    /// <summary>Makes the given NPC your Pet or Companion</summary>
    /// <param name="minion">NPC to control</param>
    /// <param name="duration">The amount of time, in miliseconds, to control the minion. 0 is infinite.</param>
    public void MakePet(NPC minion)
    {
      MakePet(minion, 0);
    }

    /// <summary>Makes the given NPC your Pet or Companion</summary>
    /// <param name="minion">NPC to control</param>
    /// <param name="duration">The amount of time, in miliseconds, to control the minion. 0 is infinite.</param>
    public void MakePet(NPC minion, int durationMillis)
    {
      Enslave(minion, durationMillis);
      minion.MakePet(m_record.EntityLowId);
      ++m_record.PetCount;
      InitializeMinion(minion);
      if(minion.Level < Level - PetMgr.MaxHunterPetLevelDifference)
      {
        minion.Level = Level - PetMgr.MaxHunterPetLevelDifference;
      }
      else
      {
        if(minion.Level <= Level)
          return;
        minion.Level = Level;
      }
    }

    /// <summary>
    /// Is called when this Character gets a new minion or pet or when
    /// he changes his ActivePet to the given one.
    /// </summary>
    private void InitializeMinion(NPC pet)
    {
      Summon = pet.EntityId;
      pet.Summoner = this;
      pet.Creator = EntityId;
      pet.PetRecord.SetupPet(pet);
      pet.SetPetAttackMode(pet.PetRecord.AttackMode);
      ActivePet = pet;
      for(DamageSchool school = DamageSchool.Physical; school < DamageSchool.Count; ++school)
        pet.UpdatePetResistance(school);
    }

    /// <summary>Amount of purchased stable slots</summary>
    public int StableSlotCount
    {
      get { return Record.StableSlotCount; }
      set { Record.StableSlotCount = value; }
    }

    public PermanentPetRecord GetStabledPet(uint petNumber)
    {
      if(m_StabledPetRecords == null)
        return null;
      foreach(PermanentPetRecord stabledPetRecord in m_StabledPetRecords)
      {
        if((int) stabledPetRecord.PetNumber == (int) petNumber)
          return stabledPetRecord;
      }

      return null;
    }

    public PermanentPetRecord GetStabledPetBySlot(uint stableSlot)
    {
      if(m_StabledPetRecords == null || stableSlot > m_StabledPetRecords.Count)
        return null;
      return m_StabledPetRecords[(int) stableSlot];
    }

    /// <summary>
    /// Stable the currently ActivePet.
    /// Make sure there is at least one free StableSlot
    /// </summary>
    /// <returns>True if the pet was stabled.</returns>
    public void StablePet()
    {
      NPC activePet = ActivePet;
      activePet.PermanentPetRecord.StabledSince = DateTime.Now;
      activePet.PetRecord.IsStabled = true;
      ActivePet = null;
    }

    /// <summary>Tries to make the stabled pet the ActivePet</summary>
    /// <param name="stabledPermanentPet">The stabled pet to make Active.</param>
    /// <returns>True if the stabled was made Active.</returns>
    public void DeStablePet(PermanentPetRecord stabledPermanentPet)
    {
      m_StabledPetRecords.Remove(stabledPermanentPet);
      SpawnPet(stabledPermanentPet).PermanentPetRecord.StabledSince = new DateTime?();
    }

    /// <summary>Tries to swap the ActivePet for a Stabled one.</summary>
    /// <param name="stabledPermanentPet">The stabled pet to swap out.</param>
    /// <returns>True if the Stabled Pet was swapped.</returns>
    public bool TrySwapStabledPet(PermanentPetRecord stabledPermanentPet)
    {
      if(StabledPetRecords.Count >= StableSlotCount + 1)
        return false;
      NPC activePet = m_activePet;
      if(activePet == null)
        return false;
      PermanentPetRecord petRecord = activePet.PetRecord as PermanentPetRecord;
      if(petRecord == null)
        return false;
      petRecord.IsStabled = true;
      DeStablePet(stabledPermanentPet);
      return true;
    }

    /// <summary>
    /// Tries to have this Character purchase another Stable Slot.
    /// </summary>
    /// <returns>True if successful.</returns>
    public bool TryBuyStableSlot()
    {
      return true;
    }

    private void LoadPets()
    {
      IPetRecord record = null;
      if(m_record.PetSummonedCount > 0)
      {
        foreach(SummonedPetRecord summonedPetRecord in SummonedPetRecord.LoadSummonedPetRecords(m_record.EntityLowId))
        {
          if(summonedPetRecord.IsActivePet)
            record = summonedPetRecord;
          SummonedPetRecords.Add(summonedPetRecord);
        }
      }

      if(m_record.PetCount > 0)
      {
        foreach(PermanentPetRecord permanentPetRecord in PermanentPetRecord.LoadPermanentPetRecords(
          m_record.EntityLowId))
        {
          if(permanentPetRecord.IsActivePet)
            record = permanentPetRecord;
          StabledPetRecords.Add(permanentPetRecord);
        }
      }

      if(m_record.PetEntryId == 0 || !IsPetActive)
        return;
      if(record != null)
      {
        if(record.Entry == null)
        {
          log.Warn("{0} has invalid PetEntryId: {1} ({2})", this, m_record.PetEntryId, m_record.PetEntryId);
          AddPetRecord(record);
        }
        else
          SpawnActivePet(record);
      }
      else
      {
        m_record.PetEntryId = 0;
        m_record.IsPetActive = false;
      }
    }

    private void SpawnActivePet(IPetRecord record)
    {
      AddMessage(() =>
      {
        IActivePetSettings record1 = m_record;
        NPC npc = SpawnPet(record, ref m_position, record1.PetDuration);
        npc.CreationSpellId = record1.PetSummonSpellId;
        npc.Health = record1.PetHealth;
        npc.Power = record1.PetPower;
      });
    }

    private void AddPetRecord(IPetRecord record)
    {
      if(record is SummonedPetRecord)
        SummonedPetRecords.Add((SummonedPetRecord) record);
      else if(record is PermanentPetRecord)
        StabledPetRecords.Add((PermanentPetRecord) record);
      else
        log.Warn("Unclassified PetRecord: " + record);
    }

    internal void SaveEntourage()
    {
      if(m_activePet != null)
      {
        m_activePet.UpdatePetData(m_record);
        m_activePet.PetRecord.Save();
      }

      CommitPetChanges(m_StabledPetRecords);
      CommitPetChanges(m_SummonedPetRecords);
    }

    private static void CommitPetChanges<T>(IList<T> records) where T : IPetRecord
    {
      if(records == null)
        return;
      for(int index = 0; index < records.Count; ++index)
      {
        T record = records[index];
        if(record.IsDirty)
          record.Save();
      }
    }

    public bool CanControlExoticPets { get; set; }

    /// <summary>TODO: This seems awfully unsafe and inconsistent</summary>
    public int PetBonusTalentPoints
    {
      get { return m_petBonusTalentPoints; }
      set
      {
        int num = value - m_petBonusTalentPoints;
        foreach(PermanentPetRecord stabledPetRecord in StabledPetRecords)
          stabledPetRecord.FreeTalentPoints += num;
        if(m_activePet != null)
          m_activePet.FreeTalentPoints += num;
        m_petBonusTalentPoints = value;
      }
    }

    internal SummonedPetRecord GetOrCreateSummonedPetRecord(NPCEntry entry)
    {
      SummonedPetRecord petRecord = GetOrCreatePetRecord(entry, SummonedPetRecords);
      petRecord.PetNumber = (uint) PetMgr.PetNumberGenerator.Next();
      return petRecord;
    }

    internal T GetOrCreatePetRecord<T>(NPCEntry entry, IList<T> list) where T : IPetRecord, new()
    {
      foreach(T obj in list)
      {
        if(obj.EntryId == entry.NPCId)
          return obj;
      }

      if(typeof(T) == typeof(SummonedPetRecord))
        ++m_record.PetSummonedCount;
      else
        ++m_record.PetCount;
      return PetMgr.CreateDefaultPetRecord<T>(entry, m_record.EntityLowId);
    }

    protected internal override void OnMinionEnteredMap(NPC minion)
    {
      base.OnMinionEnteredMap(minion);
      if(minion.Entry.Type == CreatureType.Totem)
      {
        if(m_totems == null)
          m_totems = new NPC[4];
        uint totemIndex = minion.GetTotemIndex();
        if(m_totems[totemIndex] != null)
          m_totems[totemIndex].Delete();
        m_totems[totemIndex] = minion;
      }
      else
      {
        if(minion == m_activePet)
          return;
        if(m_minions == null)
          m_minions = new NPCCollection();
        m_minions.Add(minion);
      }
    }

    protected internal override void OnMinionLeftMap(NPC minion)
    {
      base.OnMinionLeftMap(minion);
      if(minion == m_activePet)
      {
        if(m_activePet.PetRecord == null)
          return;
        m_activePet.UpdatePetData(m_record);
        ((ActiveRecordBase) m_activePet.PetRecord).SaveLater();
      }
      else if(minion.Entry.Type == CreatureType.Totem)
      {
        if(m_totems == null)
          return;
        uint totemIndex = minion.GetTotemIndex();
        if(m_totems[totemIndex] != minion)
          return;
        m_totems[totemIndex] = null;
      }
      else
      {
        if(m_minions == null)
          return;
        m_minions.Remove(minion);
      }
    }

    /// <summary>Called when a Pet or</summary>
    /// <param name="minion"></param>
    protected internal override void OnMinionDied(NPC minion)
    {
      base.OnMinionDied(minion);
      if(minion == m_activePet)
      {
        IsPetActive = false;
      }
      else
      {
        if(m_minions == null)
          return;
        m_minions.Remove(minion);
      }
    }

    public void RemoveSummonedEntourage()
    {
      if(Minions != null)
      {
        foreach(NPC npc in Minions.Where(minion => minion != null))
          DeleteMinion(npc);
      }

      if(Totems == null)
        return;
      foreach(NPC npc in Totems.Where(totem => totem != null))
        DeleteMinion(npc);
    }

    private void DeleteMinion(NPC npc)
    {
      if(npc.Summon != EntityId.Zero)
      {
        WorldObject worldObject = Map.GetObject(npc.Summon);
        if(worldObject != null)
          worldObject.Delete();
        npc.Summon = EntityId.Zero;
      }

      npc.Delete();
    }

    public bool OwnsGo(GOEntryId goId)
    {
      if(m_ownedGOs == null)
        return false;
      foreach(GameObject ownedGo in m_ownedGOs)
      {
        if(ownedGo.Entry.GOId == goId)
          return true;
      }

      return false;
    }

    public GameObject GetOwnedGO(GOEntryId id)
    {
      if(m_ownedGOs != null)
        return m_ownedGOs.Find(go => id == go.Entry.GOId);
      return null;
    }

    public GameObject GetOwnedGO(uint slot)
    {
      if(m_ownedGOs != null)
      {
        foreach(GameObject ownedGo in m_ownedGOs)
        {
          if((int) ownedGo.Entry.SummonSlotId == (int) slot)
            return ownedGo;
        }
      }

      return null;
    }

    public void RemoveOwnedGO(uint slot)
    {
      if(m_ownedGOs == null)
        return;
      foreach(GameObject ownedGo in m_ownedGOs)
      {
        if((int) ownedGo.Entry.SummonSlotId == (int) slot)
        {
          ownedGo.Delete();
          break;
        }
      }
    }

    public void RemoveOwnedGO(GOEntryId goId)
    {
      if(m_ownedGOs == null)
        return;
      foreach(GameObject ownedGo in m_ownedGOs)
      {
        if((GOEntryId) ownedGo.EntryId == goId)
        {
          ownedGo.Delete();
          break;
        }
      }
    }

    internal void AddOwnedGO(GameObject go)
    {
      if(m_ownedGOs == null)
        m_ownedGOs = new List<GameObject>();
      go.Master = this;
      m_ownedGOs.Add(go);
    }

    internal void OnOwnedGODestroyed(GameObject go)
    {
      if(m_ownedGOs == null)
        return;
      m_ownedGOs.Remove(go);
    }

    private void DetatchFromVechicle()
    {
      VehicleSeat vehicleSeat = VehicleSeat;
      if(vehicleSeat == null)
        return;
      vehicleSeat.ClearSeat();
    }

    /// <summary>
    /// Creates a new character and loads all required character data from the database
    /// </summary>
    /// <param name="acc">The account the character is associated with</param>
    /// <param name="record">The name of the character to load</param>
    /// <param name="client">The client to associate with this character</param>
    protected internal void Create(RealmAccount acc, CharacterRecord record, IRealmClient client)
    {
      client.ActiveCharacter = this;
      acc.ActiveCharacter = this;
      Type |= ObjectTypes.Player;
      ChatChannels = new List<ChatChannel>(5);
      m_logoutTimer = new TimerEntry(0, DefaultLogoutDelayMillis, totalTime => FinishLogout());
      Account = acc;
      m_client = client;
      m_record = record;
      EntityId = EntityId.GetPlayerId(m_record.EntityLowId & 16777215U);
      m_name = m_record.Name;
      Archetype = ArchetypeMgr.GetArchetype(record.Race, record.Class);
      MainWeapon = GenericWeapon.Fists;
      PowerType = m_archetype.Class.DefaultPowerType;
      StandState = StandState.Sit;
      Money = (uint) m_record.Money;
      Outfit = m_record.Outfit;
      ScaleX = 1f;
      Gender = m_record.Gender;
      Skin = m_record.Skin;
      Facial = m_record.Face;
      HairStyle = m_record.HairStyle;
      HairColor = m_record.HairColor;
      FacialHair = m_record.FacialHair;
      UnitFlags = UnitFlags.PlayerControlled;
      Experience = m_record.Xp;
      RestXp = m_record.RestXp;
      SetInt32(UnitFields.LEVEL, m_record.Level);
      NextLevelXP = XpGenerator.GetXpForlevel(m_record.Level + 1);
      MaxLevel = RealmServerConfiguration.MaxCharacterLevel;
      RestState = RestState.Normal;
      Orientation = m_record.Orientation;
      m_bindLocation = new WorldZoneLocation(m_record.BindMap,
        new Vector3(m_record.BindX, m_record.BindY, m_record.BindZ), m_record.BindZone);
      PvPRank = 1;
      YieldsXpOrHonor = true;
      foreach(DamageSchool allDamageSchool in SpellConstants.AllDamageSchools)
        SetFloat((PlayerFields) (1185U + allDamageSchool), 1f);
      SetFloat(PlayerFields.DODGE_PERCENTAGE, 1f);
      m_auras = new PlayerAuraCollection(this);
      m_spells = PlayerSpellCollection.Obtain(this);
      WatchedFaction = m_record.WatchedFaction;
      Faction = NPCMgr.DefaultFaction;
      m_reputations = new ReputationCollection(this);
      m_skills = new SkillCollection(this);
      m_talents = new PlayerTalentCollection(this);
      m_achievements = new AchievementCollection(this);
      _asda2Inventory = new Asda2PlayerInventory(this);
      m_mailAccount = new MailAccount(this);
      m_questLog = new QuestLog(this);
      TutorialFlags = new TutorialFlags(m_record.TutorialFlags);
      this.UpdateSpellCritChance();
      m_taxiNodeMask = new TaxiNodeMask();
      PowerCostMultiplier = 1f;
      m_lastPlayTimeUpdate = DateTime.Now;
      MoveControl.Mover = this;
      MoveControl.CanControl = true;
      IncMeleePermissionCounter();
      SpeedFactor = DefaultSpeedFactor;
      if(record.JustCreated)
      {
        ModStatsForLevel(m_record.Level);
        Asda2BaseAgility = CharacterFormulas.StatOnCreation;
        Asda2BaseIntellect = CharacterFormulas.StatOnCreation;
        Asda2BaseLuck = CharacterFormulas.StatOnCreation;
        Asda2BaseSpirit = CharacterFormulas.StatOnCreation;
        Asda2BaseStamina = CharacterFormulas.StatOnCreation;
        Asda2BaseStrength = CharacterFormulas.StatOnCreation;
      }
      else
      {
        BaseHealth = m_record.BaseHealth;
        SetBasePowerDontUpdate(m_record.BasePower);
        Asda2Strength = m_record.BaseStrength;
        Asda2Intellect = m_record.BaseIntellect;
        Asda2Agility = m_record.BaseAgility;
        Asda2Stamina = m_record.BaseStamina;
        Asda2Luck = m_record.BaseLuck;
        Asda2Spirit = m_record.BaseSpirit;
        Power = m_record.Power;
        SetInt32(UnitFields.HEALTH, m_record.Health);
      }

      UpdateAsda2Agility();
      UpdateAsda2Stamina();
      UpdateAsda2Luck();
      UpdateAsda2Spirit();
      UpdateAsda2Intellect();
      UpdateAsda2Strength();
    }

    /// <summary>Loads this Character from DB when logging in.</summary>
    /// <remarks>Requires IO-Context.</remarks>
    protected internal void Load()
    {
      NativeDisplayId = m_archetype.Race.GetModel(m_record.Gender).DisplayId;
      Model = UnitMgr.DefaultModel;
      UpdateFreeTalentPointsSilently(0);
      if(m_record.JustCreated)
      {
        SpecProfiles = new SpecProfile[1]
        {
          SpecProfile.NewSpecProfile(this, 0)
        };
        m_record.KillsTotal = 0U;
        m_record.HonorToday = 0U;
        m_record.HonorYesterday = 0U;
        m_record.LifetimeHonorableKills = 0U;
        m_record.HonorPoints = 0U;
        m_record.ArenaPoints = 0U;
        m_record.TitlePoints = 0U;
        m_record.Rank = -1;
      }
      else
      {
        try
        {
          Asda2BaseAgility = Record.BaseAgility;
          Asda2BaseIntellect = Record.BaseIntellect;
          Asda2BaseStrength = Record.BaseStrength;
          Asda2BaseLuck = Record.BaseLuck;
          Asda2BaseSpirit = Record.BaseSpirit;
          Asda2BaseStamina = Record.BaseStamina;
          UpdateAsda2Agility();
          UpdateAsda2Intellect();
          UpdateAsda2Luck();
          UpdateAsda2Spirit();
          UpdateAsda2Stamina();
          UpdateAsda2Strength();
          InitGlyphsForLevel();
          SpecProfiles = SpecProfile.LoadAllOfCharacter(this);
          if(SpecProfiles.Length == 0)
            SpecProfiles = new SpecProfile[1]
            {
              SpecProfile.NewSpecProfile(this, 0)
            };
          if(m_record.CurrentSpecIndex >= SpecProfiles.Length)
            m_record.CurrentSpecIndex = 0;
          try
          {
            m_achievements.Load();
          }
          catch(Exception ex)
          {
            LogUtil.ErrorException(ex,
              string.Format("failed to load achievements, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
          }

          try
          {
            ((PlayerSpellCollection) m_spells).LoadSpellsAndTalents();
          }
          catch(Exception ex)
          {
            LogUtil.ErrorException(ex,
              string.Format("failed to load LoadSpellsAndTalents, character {0} acc {1}[{2}]", Name, Account.Name,
                AccId));
          }

          try
          {
            ((PlayerSpellCollection) m_spells).LoadCooldowns();
          }
          catch(Exception ex)
          {
            LogUtil.ErrorException(ex,
              string.Format("failed to load LoadCooldowns, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
          }

          try
          {
            AuraRecord[] auras = AuraRecord.LoadAuraRecords(EntityId.Low);
            AddPostUpdateMessage(() => m_auras.InitializeAuras(auras));
          }
          catch(Exception ex)
          {
            LogUtil.ErrorException(ex,
              string.Format("failed to load LoadAuraRecords, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
          }

          try
          {
            foreach(Asda2FishingBook record in Asda2FishingBook.LoadAll(this))
            {
              if(RegisteredFishingBooks.ContainsKey(record.Num))
                record.DeleteLater();
              else
                RegisteredFishingBooks.Add(record.Num, record);
            }
          }
          catch(Exception ex)
          {
            LogUtil.ErrorException(ex,
              string.Format("failed to load fishing books, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
          }

          try
          {
            foreach(Asda2MailMessage asda2MailMessage in Asda2MailMessage.LoadAll(this))
              MailMessages.Add(asda2MailMessage.Guid, asda2MailMessage);
          }
          catch(Exception ex)
          {
            LogUtil.ErrorException(ex,
              string.Format("failed to load mail messages, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
          }

          try
          {
            foreach(FunctionItemBuff functionItemBuff1 in FunctionItemBuff.LoadAll(this))
            {
              FunctionItemBuff functionItemBuff = functionItemBuff1;
              if(!PremiumBuffs.ContainsKey(functionItemBuff.Template.Category) &&
                 (functionItemBuff.Duration > 0L && !functionItemBuff.IsLongTime ||
                  functionItemBuff.EndsDate > DateTime.Now && functionItemBuff.IsLongTime))
              {
                if(functionItemBuff.IsLongTime)
                {
                  if(LongTimePremiumBuffs.Count(l =>
                  {
                    if(l != null)
                      return l.Template.Category == functionItemBuff.Template.Category;
                    return false;
                  }) > 0)
                  {
                    functionItemBuff.DeleteLater();
                    continue;
                  }

                  LongTimePremiumBuffs.AddElement(functionItemBuff);
                  if(functionItemBuff.Template.Category == Asda2ItemCategory.PremiumPotions)
                    Asda2WingsItemId = (short) functionItemBuff.Template.Id;
                }
                else
                  PremiumBuffs.Add(functionItemBuff.Template.Category, functionItemBuff);

                ProcessFunctionalItemEffect(functionItemBuff, true);
              }
              else
                functionItemBuff.DeleteLater();
            }
          }
          catch(Exception ex)
          {
            LogUtil.ErrorException(ex,
              string.Format("failed to load premium buffs, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
          }
        }
        catch(Exception ex)
        {
          RealmDBMgr.OnDBError(ex);
          return;
        }

        try
        {
          if(m_record.LastLogout.HasValue)
          {
            DateTime now = DateTime.Now;
            RestXp += RestGenerator.GetRestXp(now - m_record.LastLogout.Value, this);
            m_lastRestUpdate = now;
          }
          else
            m_lastRestUpdate = DateTime.Now;

          KillsTotal = m_record.KillsTotal;
          HonorToday = m_record.HonorToday;
          HonorYesterday = m_record.HonorYesterday;
          LifetimeHonorableKills = m_record.LifetimeHonorableKills;
          HonorPoints = m_record.HonorPoints;
          ArenaPoints = m_record.ArenaPoints;
          Asda2TitlePoints = (int) m_record.TitlePoints;
          Asda2Rank = m_record.Rank;
          Health = m_record.Health;
          Power = m_record.Power;
          RecalculateFactionRank(true);
        }
        catch(Exception ex)
        {
          LogUtil.ErrorException(ex,
            string.Format("failed to load last load init, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
        }
      }

      LoadPets();
      ResetUpdateInfo();
    }

    /// <summary>
    /// Ensure correct size of array of explored zones and  copy explored zones to UpdateValues array
    /// </summary>
    private unsafe void SetExploredZones()
    {
      if(m_record.ExploredZones.Length != UpdateFieldMgr.ExplorationZoneFieldSize * 4)
      {
        byte[] exploredZones = m_record.ExploredZones;
        Array.Resize(ref exploredZones, UpdateFieldMgr.ExplorationZoneFieldSize * 4);
        m_record.ExploredZones = exploredZones;
      }

      fixed(byte* numPtr = m_record.ExploredZones)
      {
        int num = 0;
        for(PlayerFields playerFields = PlayerFields.EXPLORED_ZONES_1;
          playerFields < (PlayerFields) (1041 + UpdateFieldMgr.ExplorationZoneFieldSize);
          ++playerFields)
        {
          SetUInt32(playerFields, *(uint*) (numPtr + num));
          num += 4;
        }
      }
    }

    internal void LoadQuests()
    {
      m_questLog.Load();
    }

    private void LoadEquipmentState()
    {
      if(m_record.CharacterFlags.HasFlag(CharEnumFlags.HideCloak))
        PlayerFlags |= PlayerFlags.HideCloak;
      if(!m_record.CharacterFlags.HasFlag(CharEnumFlags.HideHelm))
        return;
      PlayerFlags |= PlayerFlags.HideHelm;
    }

    private void LoadDeathState()
    {
      if(m_record.CorpseX.HasValue)
      {
        Map nonInstancedMap = World.GetNonInstancedMap(m_record.CorpseMap);
        if(nonInstancedMap != null)
        {
          m_corpse = SpawnCorpse(false, false, nonInstancedMap,
            new Vector3(m_record.CorpseX.Value, m_record.CorpseY, m_record.CorpseZ), m_record.CorpseO);
          BecomeGhost();
        }
        else
        {
          if(!log.IsWarnEnabled)
            return;
          log.Warn("Player {0}'s Corpse was spawned in invalid map: {1}", this, m_record.CorpseMap);
        }
      }
      else
      {
        if(m_record.Health != 0)
          return;
        int initialDelay = DateTime.Now.Subtract(m_record.LastDeathTime).ToMilliSecondsInt() + Corpse.AutoReleaseDelay;
        m_corpseReleaseTimer = new TimerEntry(dt => ReleaseCorpse());
        if(initialDelay > 0)
        {
          MarkDead();
          m_corpseReleaseTimer.Start(initialDelay, 0);
        }
        else
          ReleaseCorpse();
      }
    }

    public void SetClass(int proffLevel, int proff)
    {
      if(Archetype.ClassId != ClassId.NoClass)
      {
        switch((byte) Archetype.ClassId)
        {
          case 1:
            Spells.Remove(SpellId.FistofIronRank1);
            Spells.Remove(SpellId.FistofDestructionRank1);
            Spells.Remove(SpellId.FistofSplinterRank1);
            break;
          case 2:
            Spells.Remove(SpellId.RaidofThiefRank1);
            Spells.Remove(SpellId.RaidofBurglarRank1);
            Spells.Remove(SpellId.RaidofTraitorRank1);
            break;
          case 3:
            Spells.Remove(SpellId.SlicetheLandRank1);
            Spells.Remove(SpellId.SlicetheOceanRank1);
            Spells.Remove(SpellId.SlicetheSkyRank1);
            break;
          case 4:
            Spells.Remove(SpellId.SilencingShotRank1);
            Spells.Remove(SpellId.DestructiveShotRank1);
            Spells.Remove(SpellId.DarkShotRank1);
            break;
          case 5:
            Spells.Remove(SpellId.ExplosiveShotRank1);
            Spells.Remove(SpellId.DefeatingShotRank1);
            Spells.Remove(SpellId.SlicingShotRank1);
            break;
          case 6:
            Spells.Remove(SpellId.FirePiercingShotRank1);
            Spells.Remove(SpellId.IronPiercingShotRank1);
            Spells.Remove(SpellId.ImmortalPiercingShotRank1);
            break;
          case 7:
            Spells.Remove(SpellId.FlameofSinRank1);
            Spells.Remove(SpellId.FlameofPunishmentRank1);
            Spells.Remove(SpellId.FlameofExtinctionRank1);
            break;
          case 8:
            Spells.Remove(SpellId.CallofEarthquakeRank1);
            Spells.Remove(SpellId.CallofCrisisRank1);
            Spells.Remove(SpellId.CallofAnnihilationRank1);
            break;
          case 9:
            Spells.Remove(SpellId.FirstShockWaveRank1);
            Spells.Remove(SpellId.SecondShockWaveRank1);
            Spells.Remove(SpellId.ThirdShockWaveRank1);
            break;
        }
      }

      if(proff >= 1 && proff <= 3)
        ProfessionLevel = (byte) proffLevel;
      if(proff >= 4 && proff <= 6)
        ProfessionLevel = (byte) (proffLevel + 11);
      if(proff >= 7 && proff <= 9)
        ProfessionLevel = (byte) (proffLevel + 22);
      Archetype = ArchetypeMgr.GetArchetype(RaceId.Human, (ClassId) proff);
      switch(proff)
      {
        case 1:
          switch(RealProffLevel)
          {
            case 1:
              Spells.AddSpell(SpellId.FistofIronRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Impenetrable7));
              return;
            case 2:
              Spells.AddSpell(SpellId.FistofIronRank1);
              Spells.AddSpell(SpellId.FistofDestructionRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Warrior15));
              return;
            case 3:
              Spells.AddSpell(SpellId.FistofIronRank1);
              Spells.AddSpell(SpellId.FistofDestructionRank1);
              Spells.AddSpell(SpellId.FistofSplinterRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Soldier18));
              return;
            case 4:
              Spells.AddSpell(SpellId.FistofIronRank1);
              Spells.AddSpell(SpellId.FistofDestructionRank1);
              Spells.AddSpell(SpellId.FistofSplinterRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Battlemaster21));
              return;
            default:
              return;
          }
        case 2:
          switch(RealProffLevel)
          {
            case 1:
              Spells.AddSpell(SpellId.RaidofThiefRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Berserk9));
              return;
            case 2:
              Spells.AddSpell(SpellId.RaidofThiefRank1);
              Spells.AddSpell(SpellId.RaidofBurglarRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Warrior15));
              return;
            case 3:
              Spells.AddSpell(SpellId.RaidofThiefRank1);
              Spells.AddSpell(SpellId.RaidofBurglarRank1);
              Spells.AddSpell(SpellId.RaidofTraitorRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Soldier18));
              return;
            case 4:
              Spells.AddSpell(SpellId.RaidofThiefRank1);
              Spells.AddSpell(SpellId.RaidofBurglarRank1);
              Spells.AddSpell(SpellId.RaidofTraitorRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Battlemaster21));
              return;
            default:
              return;
          }
        case 3:
          switch(RealProffLevel)
          {
            case 1:
              Spells.AddSpell(SpellId.SlicetheLandRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Mighty8));
              return;
            case 2:
              Spells.AddSpell(SpellId.SlicetheLandRank1);
              Spells.AddSpell(SpellId.SlicetheOceanRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Warrior15));
              return;
            case 3:
              Spells.AddSpell(SpellId.SlicetheLandRank1);
              Spells.AddSpell(SpellId.SlicetheOceanRank1);
              Spells.AddSpell(SpellId.SlicetheSkyRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Soldier18));
              return;
            case 4:
              Spells.AddSpell(SpellId.SlicetheLandRank1);
              Spells.AddSpell(SpellId.SlicetheOceanRank1);
              Spells.AddSpell(SpellId.SlicetheSkyRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Battlemaster21));
              return;
            default:
              return;
          }
        case 4:
          switch(RealProffLevel)
          {
            case 1:
              Spells.AddSpell(SpellId.SilencingShotRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Critical10));
              return;
            case 2:
              Spells.AddSpell(SpellId.SilencingShotRank1);
              Spells.AddSpell(SpellId.DestructiveShotRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Archer16));
              return;
            case 3:
              Spells.AddSpell(SpellId.SilencingShotRank1);
              Spells.AddSpell(SpellId.DestructiveShotRank1);
              Spells.AddSpell(SpellId.DarkShotRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Sharpshooter19));
              return;
            case 4:
              Spells.AddSpell(SpellId.SilencingShotRank1);
              Spells.AddSpell(SpellId.DestructiveShotRank1);
              Spells.AddSpell(SpellId.DarkShotRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Chaser22));
              return;
            default:
              return;
          }
        case 5:
          switch(RealProffLevel)
          {
            case 1:
              Spells.AddSpell(SpellId.ExplosiveShotRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Bloody11));
              return;
            case 2:
              Spells.AddSpell(SpellId.ExplosiveShotRank1);
              Spells.AddSpell(SpellId.DefeatingShotRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Archer16));
              return;
            case 3:
              Spells.AddSpell(SpellId.ExplosiveShotRank1);
              Spells.AddSpell(SpellId.DefeatingShotRank1);
              Spells.AddSpell(SpellId.SlicingShotRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Sharpshooter19));
              return;
            case 4:
              Spells.AddSpell(SpellId.ExplosiveShotRank1);
              Spells.AddSpell(SpellId.DefeatingShotRank1);
              Spells.AddSpell(SpellId.SlicingShotRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Chaser22));
              return;
            default:
              return;
          }
        case 6:
          switch(RealProffLevel)
          {
            case 1:
              Spells.AddSpell(SpellId.FirePiercingShotRank1);
              return;
            case 2:
              Spells.AddSpell(SpellId.FirePiercingShotRank1);
              Spells.AddSpell(SpellId.IronPiercingShotRank1);
              return;
            case 3:
              Spells.AddSpell(SpellId.FirePiercingShotRank1);
              Spells.AddSpell(SpellId.IronPiercingShotRank1);
              Spells.AddSpell(SpellId.ImmortalPiercingShotRank1);
              return;
            case 4:
              Spells.AddSpell(SpellId.FirePiercingShotRank1);
              Spells.AddSpell(SpellId.IronPiercingShotRank1);
              Spells.AddSpell(SpellId.ImmortalPiercingShotRank1);
              return;
            default:
              return;
          }
        case 7:
          switch(RealProffLevel)
          {
            case 1:
              Spells.AddSpell(SpellId.FlameofSinRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Hells12));
              return;
            case 2:
              Spells.AddSpell(SpellId.FlameofSinRank1);
              Spells.AddSpell(SpellId.FlameofPunishmentRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Mage17));
              return;
            case 3:
              Spells.AddSpell(SpellId.FlameofSinRank1);
              Spells.AddSpell(SpellId.FlameofPunishmentRank1);
              Spells.AddSpell(SpellId.FlameofExtinctionRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Elementalist20));
              return;
            case 4:
              Spells.AddSpell(SpellId.FlameofSinRank1);
              Spells.AddSpell(SpellId.FlameofPunishmentRank1);
              Spells.AddSpell(SpellId.FlameofExtinctionRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Archmage23));
              return;
            default:
              return;
          }
        case 8:
          switch(RealProffLevel)
          {
            case 1:
              Spells.AddSpell(SpellId.CallofEarthquakeRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Earths13));
              return;
            case 2:
              Spells.AddSpell(SpellId.CallofEarthquakeRank1);
              Spells.AddSpell(SpellId.CallofCrisisRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Mage17));
              return;
            case 3:
              Spells.AddSpell(SpellId.CallofEarthquakeRank1);
              Spells.AddSpell(SpellId.CallofCrisisRank1);
              Spells.AddSpell(SpellId.CallofAnnihilationRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Elementalist20));
              return;
            case 4:
              Spells.AddSpell(SpellId.CallofEarthquakeRank1);
              Spells.AddSpell(SpellId.CallofCrisisRank1);
              Spells.AddSpell(SpellId.CallofAnnihilationRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Archmage23));
              return;
            default:
              return;
          }
        case 9:
          switch(RealProffLevel)
          {
            case 1:
              Spells.AddSpell(SpellId.FirstShockWaveRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Heavens14));
              return;
            case 2:
              Spells.AddSpell(SpellId.FirstShockWaveRank1);
              Spells.AddSpell(SpellId.SecondShockWaveRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Mage17));
              return;
            case 3:
              Spells.AddSpell(SpellId.FirstShockWaveRank1);
              Spells.AddSpell(SpellId.SecondShockWaveRank1);
              Spells.AddSpell(SpellId.ThirdShockWaveRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Elementalist20));
              return;
            case 4:
              Spells.AddSpell(SpellId.FirstShockWaveRank1);
              Spells.AddSpell(SpellId.SecondShockWaveRank1);
              Spells.AddSpell(SpellId.ThirdShockWaveRank1);
              Map.CallDelayed(700, () => GetTitle(Asda2TitleId.Archmage23));
              return;
            default:
              return;
          }
      }
    }

    /// <summary>Loads and adds the Character to its Map.</summary>
    /// <remarks>Called initially from the IO-Context</remarks>
    internal void LoadAndLogin()
    {
      m_Map = World.GetMap(m_record);
      InstanceMgr.RetrieveInstances(this);
      ++AreaCharCount;
      if(!Role.IsStaff)
        ++Stunned;
      bool isStaff = Role.IsStaff;
      if(m_Map == null && (!isStaff || (m_Map = InstanceMgr.CreateInstance(this, m_record.MapId)) == null))
      {
        Load();
        TeleportToBindLocation();
        AddMessage(InitializeCharacter);
      }
      else
      {
        Load();
        if(!m_Map.IsDisposed)
        {
          if(m_Map.IsInstance && !isStaff)
          {
            DateTime creationTime = m_Map.CreationTime;
            DateTime? lastLogout = m_record.LastLogout;
            if((lastLogout.HasValue ? (creationTime > lastLogout.GetValueOrDefault() ? 1 : 0) : 0) != 0 ||
               !m_Map.CanEnter(this))
              goto label_7;
          }

          m_Map.AddMessage(() =>
          {
            if(m_Map is Battleground && !((Battleground) m_Map).LogBackIn(this))
            {
              AddMessage(InitializeCharacter);
            }
            else
            {
              m_position = new Vector3(m_record.PositionX, m_record.PositionY, m_record.PositionZ);
              m_zone = m_Map.GetZone(m_record.Zone);
              if(m_zone != null && m_record.JustCreated)
                SetZoneExplored(m_zone.Id, false);
              InitializeCharacter();
            }
          });
          return;
        }

        label_7:
        m_Map.TeleportOutside(this);
        AddMessage(InitializeCharacter);
      }
    }

    /// <summary>
    /// Is called after Character has been added to a map the first time and
    /// before it receives the first Update packet
    /// </summary>
    protected internal void InitializeCharacter()
    {
      World.AddCharacter(this);
      m_initialized = true;
      try
      {
        Regenerates = true;
        ((PlayerSpellCollection) m_spells).PlayerInitialize();
        if(m_record.JustCreated)
        {
          if(m_zone != null)
            m_zone.EnterZone(this, null);
          m_spells.AddDefaultSpells();
          m_reputations.Initialize();
          Skills.UpdateSkillsForLevel(Level);
        }
        else
        {
          LoadDeathState();
          LoadEquipmentState();
        }

        InitItems();
        LoadAsda2Pets();
        LoadAsda2Mounts();
        LoadAsda2TeleportPoints();
        LoadFriends();
        Ticket ticket = TicketMgr.Instance.GetTicket(EntityId.Low);
        if(ticket != null)
        {
          Ticket = ticket;
          Ticket.OnOwnerLogin(this);
        }

        Singleton<GroupMgr>.Instance.OnCharacterLogin(this);
        Singleton<GuildMgr>.Instance.OnCharacterLogin(this);
        Singleton<RelationMgr>.Instance.OnCharacterLogin(this);
        GetedTitles = new UpdateMask(Record.GetedTitles);
        DiscoveredTitles = new UpdateMask(Record.DiscoveredTitles);
        LearnedRecipes = new UpdateMask(Record.LearnedRecipes);
        for(int index = 0; index < 576; ++index)
        {
          if(LearnedRecipes.GetBit(index))
            ++LearnedRecipesCount;
        }

        for(int index = 0; index < GetedTitles.HighestIndex; ++index)
        {
          if(GetedTitles.GetBit(index))
            Asda2TitlePoints += Asda2TitleTemplate.Templates[index].Points;
        }

        LastLogin = DateTime.Now;
        bool isNew = m_record.JustCreated;
        AddMessage(() =>
        {
          if(!LastLogout.HasValue)
            RealmCommandHandler.ExecFirstLoginFileFor(this);
          RealmCommandHandler.ExecAllCharsFileFor(this);
          if(Account.Role.IsStaff)
            RealmCommandHandler.ExecFileFor(this);
          --Stunned;
          if(m_record.NextTaxiVertexId != 0)
          {
            PathVertex vertex = TaxiMgr.GetVertex(m_record.NextTaxiVertexId);
            if(vertex != null && vertex.MapId == m_Map.Id &&
               (vertex.ListEntry.Next != null && IsInRadius(vertex.Pos, vertex.ListEntry.Next.Value.DistFromPrevious)))
            {
              TaxiPaths.Enqueue(vertex.Path);
              TaxiMgr.FlyUnit(this, true, vertex.ListEntry);
            }
            else
              m_record.NextTaxiVertexId = 0;
          }
          else
            StandState = StandState.Stand;

          GodMode = m_record.GodMode;
          if(isNew)
          {
            Action<Character> created = Created;
            if(created != null)
              created(this);
          }

          if(GodMode)
            Map.CallDelayed(5000, () => SendSystemMessage("God mode is activated."));
          CharacterLoginHandler loggedIn = LoggedIn;
          if(loggedIn == null)
            return;
          loggedIn(this, true);
        });
        if(isNew)
        {
          SaveLater();
          m_record.JustCreated = false;
        }
        else
          ServerApp<RealmServer>.IOQueue.AddMessage(() =>
          {
            try
            {
              m_record.Update();
            }
            catch(Exception ex)
            {
              SaveLater();
              LogUtil.ErrorException(ex, "Failed to Update CharacterRecord: " + m_record);
            }
          });

        OnLogin();
      }
      catch(Exception ex)
      {
        if(m_record.JustCreated)
        {
          m_record.CanSave = false;
          m_record.Delete();
        }

        World.RemoveCharacter(this);
        LogUtil.ErrorException(ex, "Failed to initialize Character: " + this);
        m_client.Disconnect(false);
      }
    }

    private void LoadFriends()
    {
      FriendRecords = Asda2FriendshipRecord.LoadAll(EntityId.Low);
      foreach(Asda2FriendshipRecord friendRecord in FriendRecords)
      {
        uint friendId = friendRecord.GetFriendId(EntityId.Low);
        CharacterRecord characterRecord = CharacterRecord.LoadRecordByEntityId(friendId);
        if(characterRecord == null)
          log.Warn(string.Format("Friendship record broken cause character {0} not founded.", friendId));
        else if(Friends.ContainsKey((uint) characterRecord.AccountId))
        {
          friendRecord.DeleteLater();
        }
        else
        {
          Friends.Add((uint) characterRecord.AccountId, characterRecord);
          Character characterByAccId = World.GetCharacterByAccId((uint) characterRecord.AccountId);
          if(characterByAccId != null)
            characterByAccId.SendInfoMsg(string.Format("Your friend {0} is now online.", Name));
        }
      }
    }

    private void LoadAsda2TeleportPoints()
    {
      Asda2TeleportingPointRecord[] teleportingPointRecordArray = Asda2TeleportingPointRecord.LoadItems(EntityId.Low);
      for(int index = 0; index < teleportingPointRecordArray.Length; ++index)
        TeleportPoints[index] = teleportingPointRecordArray[index];
    }

    private void LoadAsda2Pets()
    {
      foreach(Asda2PetRecord asda2PetRecord in Asda2PetRecord.LoadAll(this))
        OwnedPets.Add(asda2PetRecord.Guid, asda2PetRecord);
    }

    private void LoadAsda2Mounts()
    {
      foreach(Asda2MountRecord asda2MountRecord in Asda2MountRecord.GetAllRecordsOfCharacter(EntityId.Low))
        OwnedMounts.Add(asda2MountRecord.Id, asda2MountRecord);
    }

    /// <summary>
    /// Load items from DB or (if new char) add initial Items.
    /// Happens either on login or when items have been loaded during runtime
    /// </summary>
    protected internal void InitItems()
    {
      if(m_record.JustCreated)
        _asda2Inventory.FillOnCharacterCreate();
      else
        _asda2Inventory.AddOwnedItems();
    }

    /// <summary>
    /// Called within Map Context.
    /// Sends initial packets
    /// </summary>
    private void OnLogin()
    {
      IsConnected = true;
      if(IsLoginServerStep)
      {
        Asda2LoginHandler.SendEnterGameResposeResponse(m_client);
        Asda2LoginHandler.SendEnterGameResponseItemsOnCharacterResponse(m_client);
        Asda2LoginHandler.SendEnterWorldIpeResponseResponse(m_client);
        Client.Disconnect(true);
      }
      else
      {
        Map.AddObjectNow(this);
        if(IsFirstGameConnection)
          IsAsda2Teleporting = false;
        if(Experience < 0)
        {
          Experience = 1;
          LogUtil.WarnException("Character {0} has negative exp. Set it to 1.", (object) Name);
        }

        if(Record.WarehousePassword != null)
          IsWarehouseLocked = true;
        Asda2CharacterHandler.SendSomeInitGSResponse(m_client);
        Asda2CharacterHandler.SendSomeInitGSOneResponse(m_client);
        Asda2CharacterHandler.SendCharacterInfoSessIdPositionResponse(m_client);
        Asda2LoginHandler.SendInventoryInfoResponse(m_client);
        Asda2CharacterHandler.SendUpdateStatsResponse(m_client);
        Asda2CharacterHandler.SendUpdateStatsOneResponse(m_client);
        Asda2InventoryHandler.SendAllFastItemSlotsInfo(this);
        if(IsFirstGameConnection)
          Asda2CharacterHandler.SendLearnedSkillsInfo(this);
        Asda2CharacterHandler.SendMySessionIdResponse(m_client);
        Asda2CharacterHandler.SendPetBoxSizeInitResponse(this);
        Asda2QuestHandler.SendQuestsListResponse(m_client);
        Asda2TitlesHandler.SendDiscoveredTitlesResponse(Client);
        Asda2TitlesHandler.SendGetedTitlesResponse(Client);
        GlobalHandler.SendCharacterPlaceInTitleRatingResponse(Client, this);
        Asda2CraftingHandler.SendLeanedRecipesResponse(Client);
        Asda2MountHandler.SendMountBoxSizeInitResponse(Client);
        if(OwnedMounts.Count > 0)
          Asda2MountHandler.SendOwnedMountsListResponse(Client);
        if(Asda2Pet != null)
          GlobalHandler.SendCharacterInfoPetResponse(Client, this);
        if(RegisteredFishingBooks.Count > 0)
        {
          foreach(KeyValuePair<byte, Asda2FishingBook> registeredFishingBook in RegisteredFishingBooks)
            Asda2FishingHandler.SendFishingBooksInfoResponse(Client, registeredFishingBook.Value);
          Asda2FishingHandler.SendFishingBookListEndedResponse(Client);
        }

        if(IsInGuild)
        {
          GlobalHandler.SendCharacterInfoClanNameResponse(Client, this);
          Asda2GuildHandler.SendClanFlagAndClanNameInfoSelfResponse(this);
          Asda2GuildHandler.SendGuildInfoOnLoginResponse(this, Guild);
          Asda2GuildHandler.SendGuildSkillsInfoResponse(this);
          Asda2GuildHandler.SendGuildNotificationResponse(Guild, GuildNotificationType.LoggedIn, GuildMember);
          Map.CallDelayed(2000, () => Asda2GuildHandler.SendGuildMembersInfoResponse(Client, Guild));
        }

        if(IsInGroup)
          Group.SendUpdate();
        GlobalHandler.SendCharacterFactionAndFactionRankResponse(Client, this);
        Asda2SoulmateHandler.SendCharacterSoulMateIntrodactionUpdateResponse(Client);
        if(FreeStatPoints > 0)
          Map.CallDelayed(15000, () =>
          {
            SendInfoMsg(string.Format("You have {0} rest expirience.", RestXp));
            SendInfoMsg(string.Format("You have {0} free stat points. Stat point can be used in VCHRMRG.",
              FreeStatPoints));
          });
        Asda2LoginHandler.SendLongTimeBuffsInfoResponse(Client);
        ProcessSoulmateRelation(true);
        Asda2CharacterHandler.SendFactionAndHonorPointsInitResponse(Client);
        Asda2FishingHandler.SendFishingLvlResponse(Client);
        GlobalHandler.SendSetClientTimeResponse(Client);
        if(PrivateShop != null)
          Map.CallDelayed(4000, () => PrivateShop.ShowOnLogin(this));
        if(Asda2TradeWindow != null)
          Asda2TradeWindow.CancelTrade();
        UpdateSettings();
        Map.CallDelayed(3800, () =>
        {
          if(Asda2TradeDescription.Contains("[OFFLINE]") && Asda2TradeDescription.Length > 10)
            Asda2TradeDescription = Asda2TradeDescription.Substring(10);
          if(Asda2TradeDescription.Contains("[OFFLINE]"))
          {
            IsAsda2TradeDescriptionEnabled = false;
            Asda2TradeDescription = "";
          }

          IsSitting = false;
          if(IsOnTransport)
            FunctionalItemsHandler.SendShopItemUsedResponse(Client, TransportItemId, -1);
          if(IsOnMount)
            Asda2MountHandler.SendCharacterOnMountStatusChangedResponse(this, Asda2MountHandler.UseMountStatus.Ok);
          if(IsDead)
            Asda2CharacterHandler.SendSelfDeathResponse(this);
          Asda2AuctionMgr.OnLogin(this);
          Asda2CharacterHandler.SendRates(this, 3, 3);
          if(IsAsda2BattlegroundInProgress)
          {
            CurrentBattleGround.SendCurrentProgress(this);
            Asda2BattlegroundHandler.SendWarTeamListResponse(this);
            Asda2BattlegroundHandler.SendTeamPointsResponse(CurrentBattleGround, this);
            Asda2BattlegroundHandler.SendHowManyPeopleInWarTeamsResponse(CurrentBattleGround, this);
            GlobalHandler.SendFightingModeChangedOnWarResponse(Client, SessionId, (int) AccId, Asda2FactionId);
            Asda2BattlegroundHandler.SendWarRemainingTimeResponse(Client);
            if(!CurrentBattleGround.IsStarted)
              Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(CurrentBattleGround,
                BattleGroundInfoMessageType.PreWarCircle, -1, null, new short?());
            if(CurrentBattleGround.WarType == Asda2BattlegroundType.Occupation)
            {
              foreach(Asda2WarPoint point in CurrentBattleGround.Points)
                Asda2BattlegroundHandler.SendWarPointsPreInitResponse(Client, point);
            }
          }

          if(MailMessages.Count > 0)
          {
            int amount = MailMessages.Values.Count(asda2MailMessage => !asda2MailMessage.IsReaded);
            if(amount > 0)
            {
              SendMailMsg(string.Format("You have {0} unreaded messages.", amount));
              Asda2MailHandler.SendYouHaveNewMailResponse(Client, amount);
            }
          }

          if(TeleportPoints.Count(c => c != null) > 0)
            Map.CallDelayed(1000, () => Asda2TeleportHandler.SendSavedLocationsInitResponse(Client));
          if(Asda2Inventory.DonationItems.Count(di => !di.Value.Recived) > 0)
            Asda2InventoryHandler.SendSomeNewItemRecivedResponse(Client, 20551, 102);
          FunctionalItemsHandler.SendWingsInfoResponse(this, Client);
          foreach(KeyValuePair<Asda2ItemCategory, FunctionItemBuff> premiumBuff in PremiumBuffs)
          {
            for(int index = 0; index < (int) premiumBuff.Value.Stacks; ++index)
              FunctionalItemsHandler.SendShopItemUsedResponse(Client, premiumBuff.Value.ItemId,
                (int) premiumBuff.Value.Duration / 1000);
          }

          foreach(DonationRecord donationRecord in ActiveRecordBase<DonationRecord>
            .FindAllByProperty("CharacterName", Name).Where(r => !r.IsDelivered).ToList())
          {
            DonationRecord record = donationRecord;
            ServerApp<RealmServer>.IOQueue.AddMessage(() =>
              Asda2Inventory.AddDonateItem(Asda2ItemMgr.GetTemplate(CharacterFormulas.DonationItemId), record.Amount,
                "donation_system", false));
            donationRecord.IsDelivered = true;
            donationRecord.DeliveredDateTime = DateTime.Now;
            donationRecord.Update();
          }
        });
        foreach(KeyValuePair<int, Asda2PetRecord> ownedPet in OwnedPets)
          Asda2PetHandler.SendInitPetInfoOnLoginResponse(Client, ownedPet.Value);
        if(IsFirstGameConnection)
        {
          SendInfoMsg("This server is running temporarily");
          IsFirstGameConnection = false;
        }
      }

      IsLoginServerStep = false;
    }

    public void ProcessSoulmateRelation(bool callOnSoulmate)
    {
      SoulmateRecord = Asda2SoulmateMgr.GetSoulmateRecord((uint) Account.AccountId);
      if(SoulmateRecord == null)
      {
        Asda2SoulmateHandler.SendDisbandSoulMateResultResponse(Client, DisbandSoulmateResult.SoulmateReleased,
          "friend");
        SoulmateRealmAccount = null;
        SoulmatedCharactersRecords = null;
      }
      else
      {
        uint num = (long) SoulmateRecord.AccId == (long) Account.AccountId
          ? SoulmateRecord.RelatedAccId
          : SoulmateRecord.AccId;
        Account account = AccountMgr.GetAccount(num);
        if(account == null)
        {
          SoulmateRecord.DeleteLater();
          SoulmateRecord = null;
          SoulmatedCharactersRecords = null;
          Asda2SoulmateHandler.SendDisbandSoulMateResultResponse(Client, DisbandSoulmateResult.SoulmateReleased,
            "friend");
        }
        else
        {
          SoulmatedCharactersRecords = CharacterRecord.FindAllOfAccount((int) num);
          SoulmateRealmAccount = ServerApp<RealmServer>.Instance.GetLoggedInAccount(account.Name);
          if(SoulmateRealmAccount != null && SoulmateRealmAccount.ActiveCharacter != null)
          {
            SoulmateRealmAccount.ActiveCharacter.SoulmateRecord = SoulmateRecord;
            if(callOnSoulmate)
              SoulmateCharacter.ProcessSoulmateRelation(false);
            Map.CallDelayed(500, () => Asda2SoulmateHandler.SendSoulMateInfoInitResponse(this, true));
            Map.CallDelayed(1000, () => Asda2SoulmateHandler.SendSoulmateEnterdGameResponse(Client));
            Map.CallDelayed(1500, () => Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(Client));
            Map.CallDelayed(2000, () => Asda2SoulmateHandler.SendSoulmatePositionResponse(Client));
          }
          else
            Asda2SoulmateHandler.SendSoulMateInfoInitResponse(this, false);
        }
      }
    }

    public void DiscoverTitle(Asda2TitleId titleId)
    {
      if(DiscoveredTitles.GetBit((int) titleId))
        return;
      DiscoveredTitles.SetBit((int) titleId);
      Asda2TitlesHandler.SendTitleDiscoveredResponse(Client, (short) titleId);
    }

    public void GetTitle(Asda2TitleId titleId)
    {
      if(GetedTitles.GetBit((int) titleId))
        return;
      AchievementProgressRecord progressRecord = Achievements.GetOrCreateProgressRecord(5U);
      switch(++progressRecord.Counter)
      {
        case 25:
          DiscoverTitle(Asda2TitleId.Collector42);
          break;
        case 50:
          GetTitle(Asda2TitleId.Collector42);
          break;
        case 75:
          DiscoverTitle(Asda2TitleId.Maniac43);
          break;
        case 150:
          GetTitle(Asda2TitleId.Maniac43);
          break;
      }

      progressRecord.SaveAndFlush();
      GetedTitles.SetBit((int) titleId);
      Asda2TitlePoints += Asda2TitleTemplate.Templates[(int) titleId].Points;
      Asda2TitlesHandler.SendYouGetNewTitleResponse(this, (short) titleId);
    }

    public bool isTitleGetted(Asda2TitleId titleId)
    {
      return GetedTitles.GetBit((int) titleId);
    }

    public bool isTitleDiscovered(Asda2TitleId titleId)
    {
      return DiscoveredTitles.GetBit((int) titleId);
    }

    /// <summary>
    /// Reconnects a client to a character that was logging out.
    /// Resends required initial packets.
    /// Called from within the map context.
    /// </summary>
    /// <param name="newClient"></param>
    internal void ReconnectCharacter(IRealmClient newClient)
    {
      CancelLogout(false);
      newClient.ActiveCharacter = this;
      m_client = newClient;
      ClearSelfKnowledge();
      OnLogin();
      m_lastPlayTimeUpdate = DateTime.Now;
      CharacterLoginHandler loggedIn = LoggedIn;
      if(loggedIn == null)
        return;
      loggedIn(this, false);
    }

    /// <summary>
    /// Enqueues saving of this Character to the IO-Queue.
    /// <see cref="M:WCell.RealmServer.Entities.Character.SaveNow" />
    /// </summary>
    public void SaveLater()
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(new Message(() => SaveNow()));
    }

    /// <summary>
    /// Saves the Character to the DB instantly.
    /// Blocking call.
    /// See: <see cref="M:WCell.RealmServer.Entities.Character.SaveLater" />.
    /// When calling this method directly, make sure to set m_saving = true
    /// </summary>
    protected internal bool SaveNow()
    {
      if(!m_record.CanSave)
        return false;
      try
      {
        if(m_record == null)
          throw new InvalidOperationException("Cannot save Character while not in world.");
        try
        {
          UpdatePlayedTime();
          m_record.Race = Race;
          m_record.Class = Class;
          m_record.Gender = Gender;
          m_record.Skin = Skin;
          m_record.Face = Facial;
          m_record.HairStyle = HairStyle;
          m_record.HairColor = HairColor;
          m_record.FacialHair = FacialHair;
          m_record.Outfit = Outfit;
          m_record.Name = Name;
          m_record.Level = Level;
          if(m_Map != null)
          {
            m_record.PositionX = Position.X;
            m_record.PositionY = Position.Y;
            m_record.PositionZ = Position.Z;
            m_record.Orientation = Orientation;
            m_record.MapId = m_Map.Id;
            m_record.InstanceId = m_Map.InstanceId;
            m_record.Zone = ZoneId;
          }

          m_record.DisplayId = DisplayId;
          m_record.BindX = m_bindLocation.Position.X;
          m_record.BindY = m_bindLocation.Position.Y;
          m_record.BindZ = m_bindLocation.Position.Z;
          m_record.BindMap = m_bindLocation.MapId;
          m_record.BindZone = m_bindLocation.ZoneId;
          m_record.Health = Health;
          m_record.BaseHealth = BaseHealth;
          m_record.Power = Power;
          m_record.BasePower = BasePower;
          m_record.Money = Money;
          m_record.WatchedFaction = WatchedFaction;
          m_record.BaseStrength = Asda2BaseStrength;
          m_record.BaseStamina = Asda2BaseStamina;
          m_record.BaseSpirit = Asda2BaseSpirit;
          m_record.BaseIntellect = Asda2BaseIntellect;
          m_record.BaseAgility = Asda2BaseAgility;
          m_record.BaseLuck = Asda2BaseLuck;
          m_record.Xp = Experience;
          m_record.RestXp = RestXp;
          m_record.KillsTotal = KillsTotal;
          m_record.HonorToday = HonorToday;
          m_record.HonorYesterday = HonorYesterday;
          m_record.LifetimeHonorableKills = LifetimeHonorableKills;
          m_record.HonorPoints = HonorPoints;
          m_record.ArenaPoints = ArenaPoints;
          m_record.TitlePoints = (uint) Asda2TitlePoints;
          m_record.Rank = Asda2Rank;
        }
        catch(Exception ex)
        {
          LogUtil.WarnException(ex,
            string.Format("failed to save pre basic ops, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
        }

        try
        {
          PlayerSpells.OnSave();
        }
        catch(Exception ex)
        {
          LogUtil.WarnException(ex,
            string.Format("failed to save spells, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
        }

        try
        {
          foreach(KeyValuePair<int, Asda2PetRecord> ownedPet in OwnedPets)
            ownedPet.Value.Save();
        }
        catch(Exception ex)
        {
          LogUtil.WarnException(ex,
            string.Format("failed to save pets, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
        }

        try
        {
          foreach(KeyValuePair<byte, Asda2FishingBook> registeredFishingBook in RegisteredFishingBooks)
            registeredFishingBook.Value.Save();
        }
        catch(Exception ex)
        {
          LogUtil.WarnException(ex,
            string.Format("failed to save fishing books, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
        }
      }
      catch(Exception ex)
      {
        OnSaveFailed(ex);
        return false;
      }

      try
      {
        try
        {
          Account.AccountData.Save();
        }
        catch(Exception ex)
        {
          LogUtil.WarnException(ex,
            string.Format("failed to save account data, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
        }

        try
        {
          _asda2Inventory.SaveAll();
        }
        catch(Exception ex)
        {
          LogUtil.WarnException(ex,
            string.Format("failed to save inventory, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
        }

        try
        {
          if(m_auras != null)
            m_auras.SaveAurasNow();
        }
        catch(Exception ex)
        {
          LogUtil.WarnException(ex,
            string.Format("failed to save auras, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
        }

        try
        {
          foreach(FunctionItemBuff functionItemBuff in PremiumBuffs.Values.ToArray())
          {
            if(functionItemBuff != null)
              functionItemBuff.Save();
          }
        }
        catch(Exception ex)
        {
          LogUtil.WarnException(ex,
            string.Format("failed to save functional item buffs, character {0} acc {1}[{2}]", Name, Account.Name,
              AccId));
        }

        try
        {
          foreach(FunctionItemBuff functionItemBuff in LongTimePremiumBuffs.ToArray())
          {
            if(functionItemBuff != null)
              functionItemBuff.Save();
          }
        }
        catch(Exception ex)
        {
          LogUtil.WarnException(ex,
            string.Format("failed to save long time buffs, character {0} acc {1}[{2}]", Name, Account.Name, AccId));
        }

        m_record.LastSaveTime = DateTime.Now;
        m_record.Save();
        return true;
      }
      catch(Exception ex)
      {
        OnSaveFailed(ex);
        return false;
      }
    }

    private void OnSaveFailed(Exception ex)
    {
      SendSystemMessage("Saving failed - Please excuse the inconvenience!");
      LogUtil.ErrorException(ex, "Could not save Character " + this);
    }

    public bool CanLogoutInstantly
    {
      get { return false; }
    }

    /// <summary>
    /// whether the Logout sequence initialized (Client might already be disconnected)
    /// </summary>
    public bool IsLoggingOut
    {
      get { return m_isLoggingOut; }
    }

    /// <summary>
    /// whether the player is currently logging out by itself (not forcefully being logged out).
    /// Players who are forced to logout cannot cancel.
    /// Is false while Client is logged in.
    /// </summary>
    public bool IsPlayerLogout
    {
      get { return _isPlayerLogout; }
      internal set { _isPlayerLogout = value; }
    }

    public bool CanLogout
    {
      get
      {
        if(!m_IsPinnedDown)
          return !IsInCombat;
        return false;
      }
    }

    /// <summary>Enqueues logout of this player to the Map's queue</summary>
    /// <param name="forced">whether the Character is forced to logout (as oppose to initializeing logout oneself)</param>
    public void LogoutLater(bool forced)
    {
      AddMessage(() => Logout(forced));
    }

    /// <summary>
    /// Starts the logout process with the default delay (or instantly if
    /// in city or staff)
    /// Requires map context.
    /// </summary>
    /// <param name="forced"></param>
    public void Logout(bool forced)
    {
      Logout(forced, CanLogoutInstantly ? 0 : DefaultLogoutDelayMillis);
    }

    /// <summary>
    /// Starts the logout process.
    /// Disconnects the Client after the given delay in seconds, if not in combat (or instantly if delay = 0)
    /// Requires map context.
    /// </summary>
    /// <param name="forced">whether the Character is forced to logout (as opposed to initializing logout oneself)</param>
    /// <param name="delay">The delay until the client will be disconnected in seconds</param>
    public void Logout(bool forced, int delay)
    {
      if(!m_isLoggingOut)
      {
        m_isLoggingOut = true;
        IsPlayerLogout = !forced;
        CancelAllActions();
        if(forced)
          ++Stunned;
        if(delay <= 0 || forced)
          FinishLogout();
        else
          m_logoutTimer.Start(delay);
      }
      else
      {
        if(!forced)
          return;
        IsPlayerLogout = false;
        if(delay <= 0)
        {
          m_logoutTimer.Stop();
          FinishLogout();
        }
        else
          m_logoutTimer.Start(delay);
      }
    }

    /// <summary>
    /// Cancels logout of this Character.
    /// Requires map context.
    /// </summary>
    public void CancelLogout()
    {
      CancelLogout(true);
    }

    /// <summary>
    /// Cancels logout of this Character.
    /// Requires map context.
    /// </summary>
    /// <param name="sendCancelReply">whether to send the Cancel-reply (if client did not disconnect in the meantime)</param>
    public void CancelLogout(bool sendCancelReply)
    {
      if(!m_isLoggingOut)
        return;
      if(!IsPlayerLogout)
        --Stunned;
      m_isLoggingOut = false;
      IsPlayerLogout = false;
      m_logoutTimer.Stop();
      DecMechanicCount(SpellMechanic.Frozen, false);
      IsSitting = false;
    }

    /// <summary>Saves and then removes Character</summary>
    /// <remarks>Requires map context for synchronization.</remarks>
    internal void FinishLogout()
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(new Message(() =>
      {
        Record.LastLogout = DateTime.Now;
        SaveNow();
        if(ContextHandler != null)
          ContextHandler.AddMessage(() => DoFinishLogout());
        else
          DoFinishLogout();
      }));
    }

    internal void DoFinishLogout()
    {
      if(!m_isLoggingOut)
        return;
      try
      {
        if(IsInGuild)
          Asda2GuildHandler.SendGuildNotificationResponse(Guild, GuildNotificationType.LoggedOut, GuildMember);
        if(SoulmateCharacter != null)
          Asda2SoulmateHandler.SendSoulmateLoggedOutResponse(SoulmateCharacter.Client);
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException("Failed to notify guild or friend about logut {0},{1},{2}", (object) Name,
          (object) EntryId, (object) ex.Message);
      }

      CharacterLogoutHandler loggingOut = LoggingOut;
      if(loggingOut != null)
        loggingOut(this);
      if(!World.RemoveCharacter(this))
        return;
      m_client.ActiveCharacter = null;
      Account.ActiveCharacter = null;
      m_isLoggingOut = false;
      RemoveSummonedEntourage();
      DetatchFromVechicle();
      for(int index = ChatChannels.Count - 1; index >= 0; --index)
        ChatChannels[index].Leave(this, true);
      if(Ticket != null)
      {
        Ticket.OnOwnerLogout();
        Ticket = null;
      }

      if(m_TaxiMovementTimer != null)
        m_TaxiMovementTimer.Stop();
      if(Asda2TradeWindow != null)
        Asda2TradeWindow.CancelTrade();
      if(PrivateShop != null)
        PrivateShop.Exit(this);
      Singleton<GroupMgr>.Instance.OnCharacterLogout(m_groupMember);
      Singleton<GuildMgr>.Instance.OnCharacterLogout(m_guildMember);
      Singleton<RelationMgr>.Instance.OnCharacterLogout(this);
      InstanceMgr.OnCharacterLogout(this);
      Asda2BattlegroundMgr.OnCharacterLogout(this);
      Battlegrounds.OnLogout();
      LastLogout = DateTime.Now;
      if(m_corpse != null)
        m_corpse.Delete();
      CancelAllActions();
      m_auras.CleanupAuras();
      m_Map.RemoveObjectNow(this);
      if(!Account.IsActive)
        m_client.Disconnect(false);
      m_initialized = false;
      ServerApp<RealmServer>.Instance.UnregisterAccount(Account);
      Client.Disconnect(false);
      Dispose();
    }

    /// <summary>Kicks this Character with the given msg instantly.</summary>
    /// <remarks>Requires map context.</remarks>
    public void Kick(string msg)
    {
      Kick(null, msg, 0);
    }

    /// <summary>
    /// Kicks this Character with the given msg after the given delay in seconds.
    /// Requires map context.
    /// </summary>
    /// <param name="delay">The delay until the Client should be disconnected in seconds</param>
    public void Kick(string reason, float delay)
    {
      Kick(reason, delay);
    }

    /// <summary>
    /// Broadcasts a kick message and then kicks this Character after the default delay.
    /// Requires map context.
    /// </summary>
    public void Kick(Character kicker, string reason)
    {
      Kick(kicker, reason, DefaultLogoutDelayMillis);
    }

    /// <summary>
    /// Broadcasts a kick message and then kicks this Character after the default delay.
    /// Requires map context.
    /// </summary>
    public void Kick(INamed kicker, string reason, int delay)
    {
      string str = (kicker != null ? " by " + kicker.Name : "") +
                   (!string.IsNullOrEmpty(reason) ? " (" + reason + ")" : ".");
      World.Broadcast("{0} has been kicked{1}", (object) Name, (object) str);
      SendSystemMessage("You have been kicked" + str);
      CancelTaxiFlight();
      Logout(true, delay);
    }

    /// <summary>Performs any needed object/object pool cleanup.</summary>
    public override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      CancelSummon(false);
      if(m_bgInfo != null)
      {
        m_bgInfo.Character = null;
        m_bgInfo = null;
      }

      m_InstanceCollection = null;
      if(m_activePet != null)
      {
        m_activePet.Delete();
        m_activePet = null;
      }

      m_minions = null;
      m_activePet = null;
      if(m_skills != null)
      {
        m_skills.m_owner = null;
        m_skills = null;
      }

      if(m_talents != null)
      {
        m_talents.Owner = null;
        m_talents = null;
      }

      _asda2Inventory = null;
      if(m_mailAccount != null)
      {
        m_mailAccount.Owner = null;
        m_mailAccount = null;
      }

      m_groupMember = null;
      if(m_reputations != null)
      {
        m_reputations.Owner = null;
        m_reputations = null;
      }

      if(m_InstanceCollection != null)
        m_InstanceCollection.Dispose();
      if(m_achievements != null)
      {
        m_achievements.m_owner = null;
        m_achievements = null;
      }

      if(m_CasterReference != null)
      {
        m_CasterReference.Object = null;
        m_CasterReference = null;
      }

      if(m_looterEntry != null)
      {
        m_looterEntry.m_owner = null;
        m_looterEntry = null;
      }

      if(m_ExtraInfo != null)
      {
        m_ExtraInfo.Dispose();
        m_ExtraInfo = null;
      }

      KnownObjects.Clear();
      WorldObjectSetPool.Recycle(KnownObjects);
    }

    /// <summary>
    /// Throws an exception, since logged in Characters may not be deleted
    /// </summary>
    protected internal override void DeleteNow()
    {
      Client.Disconnect(false);
    }

    /// <summary>
    /// Throws an exception, since logged in Characters may not be deleted
    /// </summary>
    public override void Delete()
    {
      Client.Disconnect(false);
    }

    public string TryAddStatPoints(Asda2StatType statType, int points)
    {
      if(FreeStatPoints <= 0)
        return "Sorry, but you have not free stat points.";
      if(points <= 0 || points > FreeStatPoints)
        return string.Format(
          "You must enter stat points count from {0} to {1}, but you enter {2}. Failed to increace {3}", (object) 1,
          (object) FreeStatPoints, (object) points, (object) statType);
      FreeStatPoints -= points;
      Log.Create(Log.Types.StatsOperations, LogSourceType.Character, EntryId)
        .AddAttribute("source", 0.0, "add_stat_points").AddAttribute("amount", points, "")
        .AddAttribute("free", FreeStatPoints, "").AddAttribute("stat", (double) statType, statType.ToString()).Write();
      switch(statType)
      {
        case Asda2StatType.Strength:
          Asda2BaseStrength += points;
          UpdateAsda2Strength();
          break;
        case Asda2StatType.Dexterity:
          Asda2BaseAgility += points;
          UpdateAsda2Agility();
          break;
        case Asda2StatType.Stamina:
          Asda2BaseStamina += points;
          UpdateAsda2Stamina();
          break;
        case Asda2StatType.Luck:
          Asda2BaseLuck += points;
          UpdateAsda2Luck();
          break;
        case Asda2StatType.Intelect:
          Asda2BaseIntellect += points;
          UpdateAsda2Intellect();
          break;
        case Asda2StatType.Spirit:
          Asda2BaseSpirit += points;
          UpdateAsda2Spirit();
          break;
      }

      Asda2CharacterHandler.SendUpdateStatsResponse(Client);
      Asda2CharacterHandler.SendUpdateStatsOneResponse(Client);
      return string.Format("Succeful increase {0}. Now you have {1} free stat points.", statType, FreeStatPoints);
    }

    public void ResetStatPoints()
    {
      Asda2BaseStrength = 1;
      Asda2BaseIntellect = 1;
      Asda2BaseAgility = 1;
      Asda2BaseSpirit = 1;
      Asda2BaseStamina = 1;
      Asda2BaseLuck = 1;
      FreeStatPoints = CharacterFormulas.CalculateFreeStatPointForLevel(Level, Record.RebornCount);
      Log.Create(Log.Types.StatsOperations, LogSourceType.Character, EntryId)
        .AddAttribute("source", 0.0, "reset_stat_points").AddAttribute("free", FreeStatPoints, "").Write();
      UpdateAsda2Agility();
      UpdateAsda2Strength();
      UpdateAsda2Stamina();
      UpdateAsda2Luck();
      UpdateAsda2Spirit();
      UpdateAsda2Intellect();
      Asda2CharacterHandler.SendUpdateStatsResponse(Client);
      Asda2CharacterHandler.SendUpdateStatsOneResponse(Client);
    }

    public static uint CharacterIdFromAccIdAndCharNum(int targetAccId, short targetCharNumOnAcc)
    {
      return (uint) (targetAccId + 1000000 * targetCharNumOnAcc);
    }

    public bool IsRussianClient { get; set; }

    public bool IsFromFriendDamageBonusApplied { get; set; }

    public bool IsSoulmateEmpowerPositive { get; set; }

    public bool IsSoulSongEnabled { get; set; }

    public DateTime SoulmateSongEndTime { get; set; }

    public void AddSoulmateSong()
    {
      SoulmateSongEndTime = DateTime.Now.AddMinutes(30.0);
      if(IsSoulSongEnabled)
        return;
      IsSoulSongEnabled = true;
      SendInfoMsg("You feeling soulmate song effect !!!");
      this.ChangeModifier(StatModifierFloat.Strength, CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Luck, CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Agility, CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Intelect, CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Spirit, CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Stamina, CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Damage, CharacterFormulas.SoulmateSongDamageBonusPrc);
      this.ChangeModifier(StatModifierFloat.MagicDamage, CharacterFormulas.SoulmateSongDamageBonusPrc);
      this.ChangeModifier(StatModifierFloat.Asda2Defence, CharacterFormulas.SoulmateSongDeffenceBonusPrc);
      this.ChangeModifier(StatModifierFloat.Asda2MagicDefence, CharacterFormulas.SoulmateSongDeffenceBonusPrc);
      Asda2CharacterHandler.SendUpdateStatsOneResponse(Client);
      Asda2CharacterHandler.SendUpdateStatsResponse(Client);
    }

    public void RemoveSoulmateSong()
    {
      if(!IsSoulSongEnabled)
        return;
      IsSoulSongEnabled = false;
      SendInfoMsg("Soulmate song effect removed.");
      this.ChangeModifier(StatModifierFloat.Strength, -CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Luck, -CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Agility, -CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Intelect, -CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Spirit, -CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Stamina, -CharacterFormulas.SoulmateSongStatBonusPrc);
      this.ChangeModifier(StatModifierFloat.Damage, -CharacterFormulas.SoulmateSongDamageBonusPrc);
      this.ChangeModifier(StatModifierFloat.MagicDamage, -CharacterFormulas.SoulmateSongDamageBonusPrc);
      this.ChangeModifier(StatModifierFloat.Asda2Defence, -CharacterFormulas.SoulmateSongDeffenceBonusPrc);
      this.ChangeModifier(StatModifierFloat.Asda2MagicDefence, -CharacterFormulas.SoulmateSongDeffenceBonusPrc);
      Asda2CharacterHandler.SendUpdateStatsOneResponse(Client);
      Asda2CharacterHandler.SendUpdateStatsResponse(Client);
    }

    public void AddFriendEmpower(bool positive)
    {
      SoulmateEmpowerEndTime = DateTime.Now.AddMinutes(5.0);
      if(IsSoulmateEmpowerEnabled)
      {
        SendInfoMsg("Soulmate empower duration updated.");
      }
      else
      {
        IsSoulmateEmpowerEnabled = true;
        IsSoulmateEmpowerPositive = positive;
        if(IsSoulmateEmpowerPositive)
        {
          SendInfoMsg("You feeling positive soulmate empower effect.");
          this.ChangeModifier(StatModifierFloat.Damage, CharacterFormulas.FriendEmpowerDamageBonus);
          this.ChangeModifier(StatModifierFloat.MagicDamage, CharacterFormulas.FriendEmpowerDamageBonus);
        }
        else
        {
          this.ChangeModifier(StatModifierFloat.Damage,
            (float) (-(double) CharacterFormulas.FriendEmpowerDamageBonus * 2.0));
          this.ChangeModifier(StatModifierFloat.MagicDamage,
            (float) (-(double) CharacterFormulas.FriendEmpowerDamageBonus * 2.0));
          SendInfoMsg("You feeling negative soulmate empower effect.");
        }

        Asda2CharacterHandler.SendUpdateStatsOneResponse(Client);
        Asda2CharacterHandler.SendUpdateStatsResponse(Client);
      }
    }

    public void RemoveFriendEmpower()
    {
      if(!IsSoulmateEmpowerEnabled)
        return;
      SendInfoMsg("Soulmate empower effect removed.");
      IsSoulmateEmpowerEnabled = false;
      if(IsSoulmateEmpowerPositive)
      {
        this.ChangeModifier(StatModifierFloat.Damage, -CharacterFormulas.FriendEmpowerDamageBonus);
        this.ChangeModifier(StatModifierFloat.MagicDamage, -CharacterFormulas.FriendEmpowerDamageBonus);
      }
      else
      {
        this.ChangeModifier(StatModifierFloat.Damage, CharacterFormulas.FriendEmpowerDamageBonus * 2f);
        this.ChangeModifier(StatModifierFloat.MagicDamage, CharacterFormulas.FriendEmpowerDamageBonus * 2f);
      }

      Asda2CharacterHandler.SendUpdateStatsOneResponse(Client);
      Asda2CharacterHandler.SendUpdateStatsResponse(Client);
    }

    public void AddFromFriendDamageBonus()
    {
      if(IsFromFriendDamageBonusApplied)
        return;
      IsFromFriendDamageBonusApplied = true;
      this.ChangeModifier(StatModifierFloat.Damage, CharacterFormulas.NearFriendDamageBonus);
      this.ChangeModifier(StatModifierFloat.MagicDamage, CharacterFormulas.NearFriendDamageBonus);
      this.ChangeModifier(StatModifierFloat.Asda2MagicDefence, CharacterFormulas.NearFriendDeffenceBonus);
      this.ChangeModifier(StatModifierFloat.Asda2Defence, CharacterFormulas.NearFriendDeffenceBonus);
      this.ChangeModifier(StatModifierFloat.Speed, CharacterFormulas.NearFriendSpeedBonus);
      Asda2CharacterHandler.SendUpdateStatsOneResponse(Client);
      Asda2CharacterHandler.SendUpdateStatsResponse(Client);
    }

    public void RemoveFromFriendDamageBonus()
    {
      if(!IsFromFriendDamageBonusApplied)
        return;
      IsFromFriendDamageBonusApplied = false;
      this.ChangeModifier(StatModifierFloat.Damage, -CharacterFormulas.NearFriendDamageBonus);
      this.ChangeModifier(StatModifierFloat.MagicDamage, -CharacterFormulas.NearFriendDamageBonus);
      this.ChangeModifier(StatModifierFloat.Asda2MagicDefence, -CharacterFormulas.NearFriendDeffenceBonus);
      this.ChangeModifier(StatModifierFloat.Asda2Defence, -CharacterFormulas.NearFriendDeffenceBonus);
      this.ChangeModifier(StatModifierFloat.Speed, -CharacterFormulas.NearFriendSpeedBonus);
      Asda2CharacterHandler.SendUpdateStatsOneResponse(Client);
      Asda2CharacterHandler.SendUpdateStatsResponse(Client);
    }

    public void RemovaAllSoulmateBonuses()
    {
      RemoveFriendEmpower();
      RemoveFromFriendDamageBonus();
      RemoveSoulmateSong();
      IsSoulmateSoulSaved = false;
    }

    /// <summary>
    /// Is called when the Player logs in or reconnects to a Character that was logged in before and not logged out yet (due to logout delay).
    /// </summary>
    public static event CharacterLoginHandler LoggedIn;

    /// <summary>
    /// Is called right befrore the Character is disposed and removed.
    /// </summary>
    public static event CharacterLogoutHandler LoggingOut;

    /// <summary>
    /// Is called when the given newly created Character logs in the first time.
    /// </summary>
    public static event Action<Character> Created;

    /// <summary>Is called when the given Character gains a new Level.</summary>
    public static event Action<Character> LevelChanged;

    protected override UpdateFieldCollection _UpdateFieldInfos
    {
      get { return UpdateFieldInfos; }
    }

    public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
    {
      get { return UpdateFieldHandler.DynamicPlayerHandlers; }
    }

    public Unit Observing
    {
      get { return observing ?? this; }
      set { observing = value; }
    }

    /// <summary>
    /// Will be executed by the current map we are currently in or enqueued and executed,
    /// once we re-enter a map
    /// </summary>
    public void AddPostUpdateMessage(Action action)
    {
      m_environmentQueue.Enqueue(action);
    }

    public HashSet<Character> Observers
    {
      get { return m_observers; }
    }

    internal void AddItemToUpdate(Item item)
    {
      m_itemsRequiringUpdates.Add(item);
    }

    /// <summary>
    /// Removes the given item visually from the Client.
    /// Do not call this method - but use Item.Remove instead.
    /// </summary>
    internal void RemoveOwnedItem(Item item)
    {
      m_itemsRequiringUpdates.Remove(item);
      m_environmentQueue.Enqueue(() =>
      {
        item.SendDestroyToPlayer(this);
        if(m_observers == null)
          return;
        foreach(Character observer in m_observers)
          item.SendDestroyToPlayer(observer);
      });
    }

    /// <summary>Resends all updates of everything</summary>
    public void ResetOwnWorld()
    {
      MovementHandler.SendNewWorld(Client, MapId, ref m_position, Orientation);
      ClearSelfKnowledge();
    }

    /// <summary>
    /// Clears known objects and leads to resending of the creation packet
    /// during the next Map-Update.
    /// This is only needed for teleporting or body-transfer.
    /// Requires map context.
    /// </summary>
    internal void ClearSelfKnowledge()
    {
      KnownObjects.Clear();
      NearbyObjects.Clear();
      if(m_observers == null)
        return;
      m_observers.Clear();
    }

    /// <summary>Will resend update packet of the given object</summary>
    public void InvalidateKnowledgeOf(WorldObject obj)
    {
      KnownObjects.Remove(obj);
      NearbyObjects.Remove(obj);
      obj.SendDestroyToPlayer(this);
    }

    /// <summary>
    /// Whether the given Object is visible to (and thus in broadcast-range of) this Character
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool KnowsOf(WorldObject obj)
    {
      return KnownObjects.Contains(obj);
    }

    /// <summary>Collects all update-masks from nearby objects</summary>
    internal void UpdateEnvironment(HashSet<WorldObject> updatedObjects)
    {
      HashSet<WorldObject> toRemove = WorldObjectSetPool.Obtain();
      toRemove.AddRange(KnownObjects);
      toRemove.Remove(this);
      NearbyObjects.Clear();
      if(m_initialized)
      {
        Observing.IterateEnvironment(BroadcastRange, obj =>
        {
          if(Client == null || Client.ActiveCharacter == null)
          {
            if(Client == null)
              LogUtil.WarnException("Client is null. removeing from map and world? {0}[{1}]", (object) Name,
                (object) AccId);
            else if(Client.ActiveCharacter == null)
              LogUtil.WarnException("Client.ActiveCharacter is null. removeing from map and world? {0}[{1}]",
                (object) Name, (object) AccId);
            if(Map != null)
              Map.AddMessage(() =>
              {
                Map.RemoveObject(this);
                World.RemoveCharacter(this);
              });
            return false;
          }

          if(!Observing.IsInPhase(obj) || obj is GameObject && obj.GetDistance(this) > (double) BroadcastRangeNpc)
            return true;
          NearbyObjects.Add(obj);
          if(!Observing.CanSee(obj) && !ReferenceEquals(obj, this))
            return true;
          if(!KnownObjects.Contains(obj))
          {
            Character visibleChr = obj as Character;
            if(visibleChr != null && visibleChr != this)
            {
              GlobalHandler.SendCharacterVisibleNowResponse(Client, visibleChr);
              Map.CallDelayed(200, () =>
              {
                if(visibleChr.Asda2Pet != null)
                  GlobalHandler.SendCharacterInfoPetResponse(Client, visibleChr);
                if(visibleChr.IsAsda2TradeDescriptionEnabled)
                  Asda2PrivateShopHandler.SendtradeStatusTextWindowResponseToOne(visibleChr, Client);
                GlobalHandler.SendCharacterPlaceInTitleRatingResponse(Client, visibleChr);
                GlobalHandler.SendBuffsOnCharacterInfoResponse(Client, visibleChr);
                if(visibleChr.IsInGuild)
                  GlobalHandler.SendCharacterInfoClanNameResponse(Client, visibleChr);
                GlobalHandler.SendCharacterFactionAndFactionRankResponse(Client, visibleChr);
                GlobalHandler.SendCharacterFriendShipResponse(Client, visibleChr);
                if(visibleChr.ChatRoom != null)
                  ChatMgr.SendChatRoomVisibleResponse(visibleChr, ChatRoomVisibilityStatus.Visible, visibleChr.ChatRoom,
                    this);
                CheckAtackStateWithCharacter(visibleChr);
                if(visibleChr.Asda2WingsItemId != -1)
                  FunctionalItemsHandler.SendWingsInfoResponse(visibleChr, Client);
                if(visibleChr.TransformationId == -1)
                  return;
                GlobalHandler.SendTransformToPetResponse(visibleChr, true, Client);
              });
              if(visibleChr.IsOnTransport)
                Map.CallDelayed(400,
                  () => FunctionalItemsHandler.SendShopItemUsedResponse(Client, visibleChr, int.MaxValue));
              if(visibleChr.IsOnMount)
                Map.CallDelayed(500,
                  () => Asda2MountHandler.SendCharacterOnMountStatusChangedToPneClientResponse(Client, visibleChr));
            }
            else
            {
              NPC visibleNpc = obj as NPC;
              if(visibleNpc != null && visibleNpc.IsAlive)
              {
                GlobalHandler.SendMonstrVisibleNowResponse(Client, visibleNpc);
              }
              else
              {
                GameObject npc = obj as GameObject;
                if(npc != null && npc.GoId != GOEntryId.Portal)
                {
                  if(!IsAsda2BattlegroundInProgress ||
                     CurrentBattleGround.WarType != Asda2BattlegroundType.Deathmatch || MapId != MapId.BatleField)
                    GlobalHandler.SendNpcVisiableNowResponse(Client, npc);
                }
                else
                {
                  Asda2Loot loot = obj as Asda2Loot;
                  if(loot != null)
                    GlobalHandler.SendItemVisible(this, loot);
                }
              }
            }

            OnEncountered(obj);
          }

          toRemove.Remove(obj);
          return true;
        });
        if(m_groupMember != null)
          m_groupMember.Group.UpdateOutOfRangeMembers(m_groupMember);
        foreach(WorldObject worldObject in toRemove)
          OnOutOfRange(worldObject);
      }

      Action action;
      while(m_environmentQueue.TryDequeue(out action))
        AddMessage(action);
      if(m_restTrigger != null)
        UpdateRestState();
      toRemove.Clear();
      WorldObjectSetPool.Recycle(toRemove);
    }

    private void CheckAtackStateWithCharacter(Character visibleChr)
    {
      if(MayAttack(visibleChr))
      {
        EnemyCharacters.Add(visibleChr);
        Map.CallDelayed(800, () =>
        {
          if(IsAsda2BattlegroundInProgress)
            return;
          GlobalHandler.SendFightingModeChangedResponse(Client, SessionId, (int) AccId, visibleChr.SessionId);
        });
      }
      else
      {
        if(!EnemyCharacters.Contains(visibleChr))
          return;
        EnemyCharacters.Remove(visibleChr);
        CheckEnemysCount();
      }
    }

    /// <summary>
    /// Check if this Character is still resting (if it was resting before)
    /// </summary>
    private void UpdateRestState()
    {
      if(m_restTrigger.IsInArea(this))
        return;
      RestTrigger = null;
    }

    /// <summary>
    /// Sends Item-information and Talents to the given Character and keeps them updated until they
    /// are out of range.
    /// </summary>
    /// <param name="chr"></param>
    public override UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
    {
      if(chr == this)
        return UpdateFieldFlags.Public | UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly |
               UpdateFieldFlags.GroupOnly;
      return base.GetUpdateFieldVisibilityFor(chr);
    }

    protected override UpdateType GetCreationUpdateType(UpdateFieldFlags flags)
    {
      return !flags.HasAnyFlag(UpdateFieldFlags.Private) ? UpdateType.Create : UpdateType.CreateSelf;
    }

    public void PushFieldUpdate(UpdateFieldId field, uint value)
    {
      if(!IsInWorld)
      {
        SetUInt32(field, value);
      }
      else
      {
        using(UpdatePacket fieldUpdatePacket = GetFieldUpdatePacket(field, value))
          SendUpdatePacket(this, fieldUpdatePacket);
      }
    }

    public void PushFieldUpdate(UpdateFieldId field, EntityId value)
    {
    }

    public override void Update(int dt)
    {
      base.Update(dt);
      if(m_isLoggingOut)
        m_logoutTimer.Update(dt);
      if(!IsMoving && LastSendIamNotMoving < (uint) Environment.TickCount)
      {
        LastSendIamNotMoving =
          (uint) (Environment.TickCount + CharacterFormulas.TimeBetweenImNotMovingPacketSendMillis);
        Asda2MovmentHandler.SendStartMoveCommonToAreaResponse(this, true, false);
      }

      Asda2MovmentHandler.CalculateAndSetRealPos(this, dt);
      if(Asda2Pet != null)
      {
        if(LastPetExpGainTime < (uint) Environment.TickCount)
        {
          Asda2Pet.GainXp(1);
          LastPetExpGainTime =
            (uint) (Environment.TickCount + (int) CharacterFormulas.TimeBetweenPetExpGainSecs * 1000);
        }

        if(!PetNotHungerEnabled && LastPetEatingTime < (uint) Environment.TickCount)
        {
          if(Asda2Pet.HungerPrc == 1)
          {
            Asda2PetHandler.SendPetGoesSleepDueStarvationResponse(Client, Asda2Pet);
            Asda2Pet.RemoveStatsFromOwner();
            Asda2Pet.HungerPrc = 0;
            Asda2Pet = null;
            GlobalHandler.UpdateCharacterPetInfoToArea(this);
          }
          else
          {
            --Asda2Pet.HungerPrc;
            LastPetEatingTime =
              (uint) (Environment.TickCount + (int) CharacterFormulas.TimeBetweenPetEatingsSecs * 1000);
          }
        }
      }

      if(PremiumBuffs.Count > 0)
      {
        foreach(FunctionItemBuff record in PremiumBuffs.Values)
        {
          if(record.Duration < dt)
          {
            ProcessFunctionalItemEffect(record, false);
            CategoryBuffsToDelete.Add(record.Template.Category);
            record.DeleteLater();
          }
          else
            record.Duration -= dt;
        }
      }

      foreach(FunctionItemBuff longTimePremiumBuff in LongTimePremiumBuffs)
      {
        if(longTimePremiumBuff != null && longTimePremiumBuff.EndsDate < DateTime.Now)
        {
          ProcessFunctionalItemEffect(longTimePremiumBuff, false);
          CategoryBuffsToDelete.Add(longTimePremiumBuff.Template.Category);
          longTimePremiumBuff.DeleteLater();
        }
      }

      if(CategoryBuffsToDelete.Count > 0)
      {
        foreach(Asda2ItemCategory key in CategoryBuffsToDelete)
        {
          PremiumBuffs.Remove(key);
          for(int index = 0; index < LongTimePremiumBuffs.Length; ++index)
          {
            if(LongTimePremiumBuffs[index] != null && LongTimePremiumBuffs[index].Template.Category == key)
            {
              LongTimePremiumBuffs[index] = null;
              break;
            }
          }
        }

        CategoryBuffsToDelete.Clear();
      }

      List<Asda2PereodicActionType> pereodicActionTypeList = new List<Asda2PereodicActionType>();
      foreach(KeyValuePair<Asda2PereodicActionType, PereodicAction> pereodicAction in PereodicActions)
      {
        pereodicAction.Value.Update(dt);
        if(pereodicAction.Value.CallsNum <= 0)
          pereodicActionTypeList.Add(pereodicAction.Key);
      }

      foreach(Asda2PereodicActionType key in pereodicActionTypeList)
        PereodicActions.Remove(key);
      if(SoulmateRecord != null)
        SoulmateRecord.OnUpdateTick();
      DateTime? banChatTill = BanChatTill;
      DateTime now = DateTime.Now;
      if((banChatTill.HasValue ? (banChatTill.GetValueOrDefault() < now ? 1 : 0) : 0) == 0)
        return;
      BanChatTill = new DateTime?();
      ChatBanned = false;
      SendInfoMsg("Chat is unbanned.");
    }

    public override UpdatePriority UpdatePriority
    {
      get { return UpdatePriority.HighPriority; }
    }

    private void UpdateSettings()
    {
      if(SettingsFlags == null)
        return;
      for(int index = 0; index < SettingsFlags.Length; ++index)
      {
        bool flag = SettingsFlags[index] == 1;
        switch(index)
        {
          case 5:
            EnableWishpers = flag;
            break;
          case 7:
            EnableSoulmateRequest = flag;
            break;
          case 8:
            EnableFriendRequest = flag;
            break;
          case 9:
            EnablePartyRequest = flag;
            break;
          case 10:
            EnableGuildRequest = flag;
            break;
          case 11:
            EnableGeneralTradeRequest = flag;
            break;
          case 12:
            EnableGearTradeRequest = flag;
            break;
        }
      }
    }

    /// <param name="character"></param>
    /// <param name="firstLogin">Indicates whether the Character starts a new session or if
    /// the client re-connected to a Character that was already logged in.</param>
    public delegate void CharacterLoginHandler(Character chr, bool firstLogin);

    public delegate void CharacterLogoutHandler(Character chr);
  }
}