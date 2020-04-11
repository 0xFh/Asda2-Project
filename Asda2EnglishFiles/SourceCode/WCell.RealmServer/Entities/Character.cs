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
  public class Character : Unit, IUser, IChatter, INamedEntity, IPacketReceivingEntity, IChatTarget, IContainer, IEntity, ITicketHandler, IGenericChatTarget, INamed, IHasRole, IInstanceHolderSet, ICharacterSet, IPacketReceiver
  {
    public static Color GmChatColor = Color.ForestGreen;
    public static readonly List<Character> EmptyArray = new List<Character>();
    /// <summary>The delay until a normal player may logout in millis.</summary>
    [NotVariable]
    public static int DefaultLogoutDelayMillis = 60000;
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
    [NotVariable]
    public static int PointsToGetBan = 9999;
    public new static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Player);
    /// <summary>
    /// All objects that are currently visible by this Character.
    /// Don't manipulate this collection.
    /// </summary>
    /// <remarks>Requires map context.</remarks>
    internal HashSet<WorldObject> KnownObjects = WorldObject.WorldObjectSetPool.Obtain();
    /// <summary>
    /// All objects that are currently in BroadcastRadius of this Character.
    /// Don't manipulate this collection.
    /// </summary>
    /// <remarks>Requires map context.</remarks>
    public readonly ICollection<WorldObject> NearbyObjects = (ICollection<WorldObject>) new List<WorldObject>();
    protected WCell.RealmServer.Battlegrounds.Arenas.ArenaTeamMember[] m_arenaTeamMember = new WCell.RealmServer.Battlegrounds.Arenas.ArenaTeamMember[3];
    /// <summary>All languages known to this Character</summary>
    protected internal readonly HashSet<ChatLanguage> KnownLanguages = new HashSet<ChatLanguage>();
    public Dictionary<long, Asda2MailMessage> MailMessages = new Dictionary<long, Asda2MailMessage>();
    public Asda2TeleportingPointRecord[] TeleportPoints = new Asda2TeleportingPointRecord[10];
    public Dictionary<uint, CharacterRecord> Friends = new Dictionary<uint, CharacterRecord>();
    public Dictionary<Asda2ItemCategory, FunctionItemBuff> PremiumBuffs = new Dictionary<Asda2ItemCategory, FunctionItemBuff>();
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
    public Dictionary<Asda2PereodicActionType, PereodicAction> PereodicActions = new Dictionary<Asda2PereodicActionType, PereodicAction>();
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
      if (this.m_dmgBonusVsCreatureTypePct == null)
        this.m_dmgBonusVsCreatureTypePct = new int[14];
      int num = this.m_dmgBonusVsCreatureTypePct[(int) type] + delta;
      this.m_dmgBonusVsCreatureTypePct[(int) type] = num;
    }

    /// <summary>Damage bonus vs creature type in %</summary>
    public void ModDmgBonusVsCreatureTypePct(uint[] creatureTypes, int delta)
    {
      foreach (CreatureType creatureType in creatureTypes)
        this.ModDmgBonusVsCreatureTypePct(creatureType, delta);
    }

    public int GetMeleeAPModByStat(StatType stat)
    {
      if (this.m_MeleeAPModByStat == null)
        return 0;
      return this.m_MeleeAPModByStat[(int) stat];
    }

    public void SetMeleeAPModByStat(StatType stat, int value)
    {
      if (this.m_MeleeAPModByStat == null)
        this.m_MeleeAPModByStat = new int[6];
      this.m_baseStats[(int) stat] = value;
      this.UpdateMeleeAttackPower();
    }

    public void ModMeleeAPModByStat(StatType stat, int delta)
    {
      this.SetMeleeAPModByStat(stat, this.GetMeleeAPModByStat(stat) + delta);
    }

    public int GetRangedAPModByStat(StatType stat)
    {
      if (this.m_RangedAPModByStat == null)
        return 0;
      return this.m_RangedAPModByStat[(int) stat];
    }

    public void SetRangedAPModByStat(StatType stat, int value)
    {
      if (this.m_RangedAPModByStat == null)
        this.m_RangedAPModByStat = new int[6];
      this.m_baseStats[(int) stat] = value;
      this.UpdateRangedAttackPower();
    }

    public void ModRangedAPModByStat(StatType stat, int delta)
    {
      this.SetRangedAPModByStat(stat, this.GetRangedAPModByStat(stat) + delta);
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
      if (this.m_fallStart == 0)
      {
        this.m_fallStart = Environment.TickCount;
        this.m_fallStartHeight = this.m_position.Z;
      }
      if (this.IsFlying || !this.IsAlive)
        return;
      int num = this.GodMode ? 1 : 0;
    }

    public bool IsSwimming
    {
      get
      {
        return this.MovementFlags.HasFlag((Enum) MovementFlags.Swimming);
      }
    }

    public bool IsUnderwater
    {
      get
      {
        return (double) this.m_position.Z < (double) this.m_swimSurfaceHeight - 0.5;
      }
    }

    protected internal void OnSwim()
    {
      if (this.IsSwimming)
        return;
      this.m_swimStart = DateTime.Now;
    }

    protected internal void OnStopSwimming()
    {
      this.m_swimSurfaceHeight = -2048f;
    }

    /// <summary>
    /// Is called whenever the Character is moved while on Taxi, Ship, elevator etc
    /// </summary>
    protected internal void MoveTransport(ref Vector4 transportLocation)
    {
      this.SendSystemMessage("You have been identified as cheater: Faking transport movement!");
    }

    /// <summary>Is called whenever a Character moves</summary>
    public override void OnMove()
    {
      base.OnMove();
      this.IsFighting = false;
      if (this.m_standState != StandState.Stand)
        this.StandState = StandState.Stand;
      if (this.m_currentRitual != null)
        this.m_currentRitual.Remove(this);
      if (this.IsTrading && !this.IsInRadius((WorldObject) this.m_tradeWindow.OtherWindow.Owner, TradeMgr.MaxTradeRadius))
        this.m_tradeWindow.Cancel(TradeStatus.TooFarAway);
      if (this.CurrentCapturingPoint != null)
        this.CurrentCapturingPoint.StopCapture();
      this.LastPosition = this.MoveControl.Mover.Position;
    }

    public void SetMover(WorldObject mover, bool canControl)
    {
      this.MoveControl.Mover = mover ?? (WorldObject) this;
      this.MoveControl.CanControl = canControl;
    }

    public void ResetMover()
    {
      this.MoveControl.Mover = (WorldObject) this;
      this.MoveControl.CanControl = true;
    }

    /// <summary>
    /// Is called whenever a new object appears within vision range of this Character
    /// </summary>
    public void OnEncountered(WorldObject obj)
    {
      if (obj != this)
        obj.OnEncounteredBy(this);
      this.KnownObjects.Add(obj);
    }

    /// <summary>
    /// Is called whenever an object leaves this Character's sight
    /// </summary>
    public void OnOutOfRange(WorldObject obj)
    {
      --obj.AreaCharCount;
      if (obj == this.Asda2DuelingOponent && this.Asda2Duel != null)
        this.Asda2Duel.StopPvp();
      if (obj == this.m_target)
        this.ClearTarget();
      if (obj == this.m_activePet)
        this.ActivePet = (NPC) null;
      if (this.GossipConversation != null && obj == this.GossipConversation.Speaker && this.GossipConversation.Character == this)
        this.GossipConversation.Dispose();
      if (!(obj is Transport))
        this.KnownObjects.Remove(obj);
      Character chr = obj as Character;
      if (chr != null)
      {
        if (this.EnemyCharacters.Contains(chr))
        {
          this.EnemyCharacters.Remove(chr);
          this.CheckEnemysCount();
        }
        GlobalHandler.SendCharacterDeleteResponse(chr, this.Client);
      }
      else
      {
        Asda2Loot loot = obj as Asda2Loot;
        if (loot == null)
          return;
        GlobalHandler.SendRemoveLootResponse(this, loot);
      }
    }

    public void CheckEnemysCount()
    {
      if (this.EnemyCharacters.Count != 0 || this.IsAsda2BattlegroundInProgress)
        return;
      GlobalHandler.SendFightingModeChangedResponse(this.Client, this.SessionId, (int) this.AccId, (short) -1);
    }

    /// <summary>
    /// Is called whenever this Character was added to a new map
    /// </summary>
    protected internal override void OnEnterMap()
    {
      base.OnEnterMap();
      if (!this._saveTaskRunning)
      {
        this._saveTaskRunning = true;
        ServerApp<WCell.RealmServer.RealmServer>.IOQueue.CallDelayed(CharacterFormulas.SaveChateterInterval, new Action(this.SaveCharacter));
      }
      this.ClearSelfKnowledge();
      this.m_lastMoveTime = Environment.TickCount;
      this.LastPosition = this.m_position;
      this.AddPostUpdateMessage((Action) (() =>
      {
        if (this.m_zone == null)
          return;
        int num = this.m_zone.Template.IsPvP ? 1 : 0;
      }));
      if (!this.IsPetActive)
        return;
      this.IsPetActive = true;
    }

    protected internal override void OnLeavingMap()
    {
      if (this.m_activePet != null && this.m_activePet.IsInWorld)
        this.m_activePet.Map.RemoveObject((WorldObject) this.m_activePet);
      if (this.m_minions != null)
      {
        foreach (WorldObject minion in (List<NPC>) this.m_minions)
          minion.Delete();
      }
      base.OnLeavingMap();
    }

    /// <summary>
    /// Changes the character's stand state and notifies the client.
    /// </summary>
    public override StandState StandState
    {
      get
      {
        return this.m_standState;
      }
      set
      {
        if (value == this.StandState)
          return;
        this.m_standState = value;
        base.StandState = value;
        if (this.m_looterEntry != null && this.m_looterEntry.Loot != null && (value != StandState.Kneeling && this.m_looterEntry.Loot.MustKneelWhileLooting))
          this.CancelLooting();
        if (value != StandState.Stand)
          return;
        this.m_auras.RemoveByFlag(AuraInterruptFlags.OnStandUp);
      }
    }

    protected override void OnResistanceChanged(DamageSchool school)
    {
      base.OnResistanceChanged(school);
      if (this.m_activePet == null || !this.m_activePet.IsHunterPet)
        return;
      this.m_activePet.UpdatePetResistance(school);
    }

    public override void ModSpellHitChance(DamageSchool school, int delta)
    {
      base.ModSpellHitChance(school, delta);
      if (this.m_activePet == null)
        return;
      this.m_activePet.ModSpellHitChance(school, delta);
    }

    public override float GetResiliencePct()
    {
      return 0.0f;
    }

    public override void DealEnvironmentalDamage(EnviromentalDamageType dmgType, int amount)
    {
      base.DealEnvironmentalDamage(dmgType, amount);
      if (this.IsAlive)
        return;
      this.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DeathsFrom, (uint) dmgType, 1U, (Unit) null);
    }

    public new bool IsMoving
    {
      get
      {
        return this._isMoving;
      }
      set
      {
        this._isMoving = value;
        if (!value)
          return;
        this.OnMove();
      }
    }

    public BaseRelation GetRelationTo(Character chr, CharacterRelationType type)
    {
      return Singleton<RelationMgr>.Instance.GetRelation(this.EntityId.Low, chr.EntityId.Low, type);
    }

    /// <summary>
    /// Returns whether this Character ignores the Character with the given low EntityId.
    /// </summary>
    /// <returns></returns>
    public bool IsIgnoring(IUser user)
    {
      return Singleton<RelationMgr>.Instance.HasRelation(this.EntityId.Low, user.EntityId.Low, CharacterRelationType.Ignored);
    }

    /// <summary>
    /// Indicates whether the two Characters are in the same <see cref="P:WCell.RealmServer.Entities.Character.Group" />
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public bool IsAlliedWith(Character chr)
    {
      if (this.m_groupMember != null && chr.m_groupMember != null)
        return this.m_groupMember.Group == chr.m_groupMember.Group;
      return false;
    }

    /// <summary>
    /// Binds Character to start position if none other is set
    /// </summary>
    private void CheckBindLocation()
    {
      if (this.m_bindLocation.IsValid())
        return;
      this.BindTo((WorldObject) this, this.m_archetype.StartLocation);
    }

    public void TeleportToBindLocation()
    {
      this.TeleportTo((IWorldLocation) this.BindLocation);
    }

    public bool CanFly
    {
      get
      {
        if (!this.m_Map.CanFly || this.m_zone != null && (!this.m_zone.Flags.HasFlag((Enum) ZoneFlags.CanFly) || this.m_zone.Flags.HasFlag((Enum) ZoneFlags.CannotFly)))
          return this.Role.IsStaff;
        return true;
      }
    }

    public override void Mount(uint displayId)
    {
      if (this.m_activePet != null)
        this.m_activePet.RemoveFromMap();
      base.Mount(displayId);
    }

    protected internal override void DoDismount()
    {
      if (this.IsPetActive)
        this.PlaceOnTop((WorldObject) this.ActivePet);
      base.DoDismount();
    }

    public int GetRandomMagicDamage()
    {
      return Utility.Random(this.MinMagicDamage, this.MaxMagicDamage);
    }

    public float GetRandomPhysicalDamage()
    {
      return Utility.Random(this.MinDamage, this.MaxDamage);
    }

    public byte RealProffLevel
    {
      get
      {
        if (this.Class == ClassId.THS || this.Class == ClassId.OHS || (this.Class == ClassId.Spear || this.Class == ClassId.NoClass))
          return this.ProfessionLevel;
        if (this.Class == ClassId.AtackMage || this.Class == ClassId.SupportMage || this.Class == ClassId.HealMage)
          return (byte) ((uint) this.ProfessionLevel - 22U);
        if (this.Class == ClassId.Bow || this.Class == ClassId.Crossbow || this.Class == ClassId.Balista)
          return (byte) ((uint) this.ProfessionLevel - 11U);
        return 0;
      }
    }

    public Asda2PetRecord AddAsda2Pet(PetTemplate petTemplate, bool silent = false)
    {
      Asda2PetRecord pet = new Asda2PetRecord(petTemplate, this);
      pet.Create();
      this.OwnedPets.Add(pet.Guid, pet);
      if (!silent)
        Asda2PetHandler.SendInitPetInfoOnLoginResponse(this.Client, pet);
      return pet;
    }

    WCell.Core.Network.Locale IPacketReceiver.Locale { get; set; }

    protected internal override void UpdateStamina()
    {
      base.UpdateStamina();
      if (this.m_MeleeAPModByStat != null)
        this.UpdateAllAttackPower();
      if (this.m_activePet == null || !this.m_activePet.IsHunterPet)
        return;
      this.m_activePet.UpdateStamina();
    }

    protected internal override void UpdateIntellect()
    {
      base.UpdateIntellect();
      if (this.PowerType == PowerType.Mana)
        this.UpdateSpellCritChance();
      this.UpdatePowerRegen();
      if (this.m_MeleeAPModByStat == null)
        return;
      this.UpdateAllAttackPower();
    }

    protected internal override void UpdateSpirit()
    {
      base.UpdateSpirit();
      if (this.m_MeleeAPModByStat == null)
        return;
      this.UpdateAllAttackPower();
    }

    protected internal override int IntellectManaBonus
    {
      get
      {
        int intellect = this.Archetype.FirstLevelStats.Intellect;
        return intellect + (this.Intellect - intellect) * Unit.ManaPerIntelligence;
      }
    }

    public int RegenHealth
    {
      get
      {
        int health = this.Health;
        if (this.PereodicActions.ContainsKey(Asda2PereodicActionType.HpRegen))
          health += this.PereodicActions[Asda2PereodicActionType.HpRegen].RemainingHeal;
        if (this.PereodicActions.ContainsKey(Asda2PereodicActionType.HpRegenPrc))
          health += this.PereodicActions[Asda2PereodicActionType.HpRegenPrc].RemainingHeal * this.MaxHealth / 100;
        if (health <= this.MaxHealth)
          return health;
        return this.MaxHealth;
      }
    }

    public int RegenMana
    {
      get
      {
        int power = this.Power;
        if (this.PereodicActions.ContainsKey(Asda2PereodicActionType.MpRegen))
          power += this.PereodicActions[Asda2PereodicActionType.MpRegen].RemainingHeal;
        if (power <= this.MaxPower)
          return power;
        return this.MaxPower;
      }
    }

    private void UpdateChancesByCombatRating(CombatRating rating)
    {
      switch (rating)
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
        if (this.m_ExtraInfo == null)
          this.m_ExtraInfo = new ExtraInfo(this);
        return this.m_ExtraInfo;
      }
    }

    public int TransportItemId
    {
      get
      {
        return this._transportItemId;
      }
      set
      {
        if (this._transportItemId == value)
          return;
        if (value == -1 || this._transportItemId != -1)
        {
          FunctionalItemsHandler.SendCancelCancelFunctionalItemResponse(this.Client, (short) this._transportItemId);
          this.ChangeModifier(StatModifierFloat.Speed, (float) -Asda2ItemMgr.GetTemplate(this._transportItemId).ValueOnUse / 100f);
        }
        if (value != -1)
        {
          FunctionalItemsHandler.SendShopItemUsedResponse(this.Client, value, -1);
          this.ChangeModifier(StatModifierFloat.Speed, (float) Asda2ItemMgr.GetTemplate(value).ValueOnUse / 100f);
        }
        this._transportItemId = value;
      }
    }

    public int MountId
    {
      get
      {
        return this._mountId;
      }
      set
      {
        if (this._mountId == value)
          return;
        if (value == -1)
        {
          this.ChangeModifier(StatModifierFloat.Speed, (float) -(Asda2MountMgr.TemplatesById[this.MountId].Unk + this.IntMods[43]) / 100f);
          this._mountId = value;
          Asda2MountHandler.SendVeicheStatusChangedResponse(this, Asda2MountHandler.MountStatusChanged.Unsumon);
          Asda2MountHandler.SendCharacterOnMountStatusChangedResponse(this, Asda2MountHandler.UseMountStatus.Ok);
        }
        else if (this.LastTransportUsedTime.AddSeconds(30.0) > DateTime.Now)
        {
          Asda2MountHandler.SendCharacterOnMountStatusChangedResponse(this, Asda2MountHandler.UseMountStatus.Fail);
          this.SendInfoMsg("Mount is on cooldown.");
        }
        else
        {
          this._mountId = value;
          Asda2MountHandler.SendVeicheStatusChangedResponse(this, Asda2MountHandler.MountStatusChanged.Summoned);
          Asda2MountHandler.SendCharacterOnMountStatusChangedResponse(this, Asda2MountHandler.UseMountStatus.Ok);
          this.ChangeModifier(StatModifierFloat.Speed, (float) (Asda2MountMgr.TemplatesById[value].Unk + this.IntMods[43]) / 100f);
          this.LastTransportUsedTime = DateTime.Now;
        }
      }
    }

    public int ApplyFunctionItemBuff(int itemId, bool isLongTimeBuff = false)
    {
      int num = 0;
      Asda2ItemTemplate templ = Asda2ItemMgr.GetTemplate(itemId);
      if (isLongTimeBuff)
      {
        if (((IEnumerable<FunctionItemBuff>) this.LongTimePremiumBuffs).Contains<FunctionItemBuff>((Func<FunctionItemBuff, bool>) (l =>
        {
          if (l != null)
            return l.Template.Category == templ.Category;
          return false;
        })))
          throw new AlreadyBuffedExcepton();
        FunctionItemBuff functionItemBuff = new FunctionItemBuff(itemId, this)
        {
          IsLongTime = true
        };
        functionItemBuff.EndsDate = DateTime.Now.AddDays(functionItemBuff.Template.AttackTime == 0 ? 7.0 : (double) functionItemBuff.Template.AttackTime);
        functionItemBuff.CreateLater();
        num = this.LongTimePremiumBuffs.AddElement<FunctionItemBuff>(functionItemBuff);
        this.ProcessFunctionalItemEffect(functionItemBuff, true);
      }
      else if (this.PremiumBuffs.ContainsKey(templ.Category))
      {
        FunctionItemBuff premiumBuff = this.PremiumBuffs[templ.Category];
        premiumBuff.Duration = (long) ((int) premiumBuff.Template.AtackRange * 1000);
        if ((int) premiumBuff.Stacks >= (int) premiumBuff.Template.MaxDurability)
          throw new AlreadyBuffedExcepton();
        this.ProcessFunctionalItemEffect(premiumBuff, false);
        ++premiumBuff.Stacks;
        this.ProcessFunctionalItemEffect(premiumBuff, true);
      }
      else
      {
        FunctionItemBuff record = new FunctionItemBuff(itemId, this);
        record.Duration = (long) ((int) record.Template.AtackRange * 1000);
        record.CreateLater();
        this.PremiumBuffs.Add(templ.Category, record);
        this.ProcessFunctionalItemEffect(record, true);
      }
      return num;
    }

    private void ProcessFunctionalItemEffect(FunctionItemBuff item, bool isPositive)
    {
      int delta = (isPositive ? item.Template.ValueOnUse : -item.Template.ValueOnUse) * (int) item.Stacks;
      switch (item.Template.Category)
      {
        case Asda2ItemCategory.IncPAtk:
          this.ChangeModifier(StatModifierFloat.Damage, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncMAtk:
          this.ChangeModifier(StatModifierFloat.MagicDamage, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncPDef:
          this.ChangeModifier(StatModifierFloat.Asda2Defence, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncMdef:
          this.ChangeModifier(StatModifierFloat.Asda2MagicDefence, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncHp:
          this.MaxHealthModScalar += (float) delta / 100f;
          this.ChangeModifier(StatModifierFloat.Health, (float) delta / 100f);
          if (isPositive)
          {
            this.Health += (this.MaxHealth * delta + 50) / 100;
            break;
          }
          break;
        case Asda2ItemCategory.IncMp:
          this.ChangeModifier(StatModifierInt.PowerPct, delta);
          break;
        case Asda2ItemCategory.IncStr:
          this.ChangeModifier(StatModifierFloat.Strength, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncSta:
          this.ChangeModifier(StatModifierFloat.Stamina, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncInt:
          this.ChangeModifier(StatModifierFloat.Intelect, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncSpi:
          this.ChangeModifier(StatModifierFloat.Spirit, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncDex:
          this.ChangeModifier(StatModifierFloat.Agility, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncLuck:
          this.ChangeModifier(StatModifierFloat.Luck, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncMoveSpeed:
          this.ChangeModifier(StatModifierFloat.Speed, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncExp:
          this.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncDropChance:
          this.ChangeModifier(StatModifierFloat.Asda2DropChance, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncDigChance:
          this.ChangeModifier(StatModifierFloat.DigChance, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncExpStackable:
          this.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float) delta / 100f);
          break;
        case Asda2ItemCategory.IncAtackSpeed:
          this.ChangeModifier(StatModifierFloat.MeleeAttackTime, (float) -delta / 100f);
          break;
        case Asda2ItemCategory.ShopBanner:
          this.EliteShopBannerEnabled = isPositive;
          break;
        case Asda2ItemCategory.PremiumPotions:
          this.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float) ((isPositive ? 1.0 : -1.0) * 20.0 / 100.0));
          this.ChangeModifier(StatModifierFloat.Asda2DropChance, (float) ((isPositive ? 1.0 : -1.0) * 20.0 / 100.0));
          this.ChangeModifier(StatModifierFloat.Health, (float) ((isPositive ? 1.0 : -1.0) * 10.0 / 100.0));
          this.ChangeModifier(StatModifierInt.PowerPct, (isPositive ? 1 : -1) * 10);
          this.ChangeModifier(StatModifierFloat.Speed, (float) ((isPositive ? 1.0 : -1.0) * 25.0 / 100.0));
          this.ChangeModifier(StatModifierFloat.Damage, (float) ((isPositive ? 1.0 : -1.0) * 10.0 / 100.0));
          this.ChangeModifier(StatModifierFloat.MagicDamage, (float) ((isPositive ? 1.0 : -1.0) * 10.0 / 100.0));
          this.Asda2WingsItemId = isPositive ? (short) item.Template.Id : (short) -1;
          break;
        case Asda2ItemCategory.ExpandInventory:
          this.InventoryExpanded = isPositive;
          break;
        case Asda2ItemCategory.PetNotEatingByDays:
          this.PetNotHungerEnabled = isPositive;
          break;
        case Asda2ItemCategory.RemoveDeathPenaltiesByDays:
          this.RemoveDeathPenalties = isPositive;
          break;
      }
      if (isPositive)
        return;
      FunctionalItemsHandler.SendCancelCancelFunctionalItemResponse(this.Client, (short) item.ItemId);
    }

    public bool PetNotHungerEnabled { get; set; }

    public bool EliteShopBannerEnabled { get; set; }

    public bool RemoveDeathPenalties { get; set; }

    public bool InventoryExpanded { get; set; }

    public bool IsOnTransport
    {
      get
      {
        return this.TransportItemId != -1;
      }
    }

    public Vector2 CurrentMovingVector { get; set; }

    public override int GetUnmodifiedBaseStatValue(StatType stat)
    {
      if ((int) (byte) stat >= this.ClassBaseStats.Stats.Length)
        return 0;
      return this.ClassBaseStats.Stats[(int) stat];
    }

    public override bool IsPlayer
    {
      get
      {
        return true;
      }
    }

    public override bool MayTeleport
    {
      get
      {
        if (this.Role.IsStaff)
          return true;
        if (!this.IsKicked && this.CanMove)
          return this.IsPlayerControlled;
        return false;
      }
    }

    public override WorldObject Mover
    {
      get
      {
        return this.MoveControl.Mover;
      }
    }

    public byte[] PlayerBytes
    {
      get
      {
        return this.GetByteArray((UpdateFieldId) PlayerFields.BYTES);
      }
      set
      {
        this.SetByteArray((UpdateFieldId) PlayerFields.BYTES, value);
      }
    }

    public byte Skin
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.BYTES, 0);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.BYTES, 0, value);
      }
    }

    public byte Facial
    {
      get
      {
        return this.Record.Face;
      }
      set
      {
        this.Record.Face = value;
      }
    }

    public byte HairStyle
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.BYTES, 2);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.BYTES, 2, value);
      }
    }

    public byte HairColor
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.BYTES, 3);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.BYTES, 3, value);
      }
    }

    public byte[] PlayerBytes2
    {
      get
      {
        return this.GetByteArray((UpdateFieldId) PlayerFields.BYTES_2);
      }
      set
      {
        this.SetByteArray((UpdateFieldId) PlayerFields.BYTES_2, value);
      }
    }

    public byte FacialHair
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.BYTES_2, 0);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.BYTES_2, 0, value);
      }
    }

    /// <summary>0x10 for SpellSteal</summary>
    public byte PlayerBytes2_2
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.BYTES_2, 1);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.BYTES_2, 1, value);
      }
    }

    /// <summary>
    /// Use player.Inventory.BankBags.Inc/DecBagSlots() to change the amount of cont slots in use
    /// </summary>
    public byte BankBagSlots
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.BYTES_2, 2);
      }
      internal set
      {
        this.SetByte((UpdateFieldId) PlayerFields.BYTES_2, 2, value);
      }
    }

    /// <summary>
    /// 0x01 -&gt; Rested State
    /// 0x02 -&gt; Normal State
    /// </summary>
    public RestState RestState
    {
      get
      {
        return (RestState) this.GetByte((UpdateFieldId) PlayerFields.BYTES_2, 3);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.BYTES_2, 3, (byte) value);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsResting
    {
      get
      {
        return this.m_restTrigger != null;
      }
    }

    /// <summary>
    /// The AreaTrigger that triggered the current Rest-state (or null if not resting).
    /// Will automatically be set when the Character enters a Rest-Type AreaTrigger
    /// and will be unset once the Character is too far away from that trigger.
    /// </summary>
    public AreaTrigger RestTrigger
    {
      get
      {
        return this.m_restTrigger;
      }
      set
      {
        if (this.m_restTrigger == value)
          return;
        if (value == null)
        {
          this.UpdateRest();
          this.m_record.RestTriggerId = 0;
          this.RestState = RestState.Normal;
        }
        else
        {
          this.m_lastRestUpdate = DateTime.Now;
          this.m_record.RestTriggerId = (int) value.Id;
          this.RestState = RestState.Resting;
        }
        this.m_restTrigger = value;
      }
    }

    public byte[] PlayerBytes3
    {
      get
      {
        return this.GetByteArray((UpdateFieldId) PlayerFields.BYTES_3);
      }
      set
      {
        this.SetByteArray((UpdateFieldId) PlayerFields.BYTES_3, value);
      }
    }

    public override GenderType Gender
    {
      get
      {
        return (GenderType) this.GetByte((UpdateFieldId) PlayerFields.BYTES_3, 0);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.BYTES_3, 0, (byte) value);
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
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.BYTES_3, 1);
      }
      set
      {
        if (value > (byte) 100)
          value = (byte) 100;
        this.SetByte((UpdateFieldId) PlayerFields.BYTES_3, 1, value);
      }
    }

    public byte PlayerBytes3_3
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.BYTES_3, 2);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.BYTES_3, 2, value);
      }
    }

    public byte PvPRank
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.BYTES_3, 3);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.BYTES_3, 3, value);
      }
    }

    /// <summary>BYTES</summary>
    public byte[] Bytes
    {
      get
      {
        return this.GetByteArray((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES);
      }
      set
      {
        this.SetByteArray((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES, value);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public CorpseReleaseFlags CorpseReleaseFlags
    {
      get
      {
        return (CorpseReleaseFlags) this.GetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES, 0);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES, 0, (byte) value);
      }
    }

    public byte Bytes_2
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES, 1);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES, 1, value);
      }
    }

    public byte ActionBarMask
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES, 2);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES, 2, value);
      }
    }

    public byte Bytes_4
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES, 3);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES, 3, value);
      }
    }

    public byte[] Bytes2
    {
      get
      {
        return this.GetByteArray((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES2);
      }
      set
      {
        this.SetByteArray((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES2, value);
      }
    }

    public byte Bytes2_1
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES2, 0);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES2, 0, value);
      }
    }

    /// <summary>Set to 0x40 for mage invis</summary>
    public byte Bytes2_2
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES2, 1);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES2, 1, value);
      }
    }

    public byte Bytes2_3
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES2, 2);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES2, 2, value);
      }
    }

    public byte Bytes2_4
    {
      get
      {
        return this.GetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES2, 3);
      }
      set
      {
        this.SetByte((UpdateFieldId) PlayerFields.PLAYER_FIELD_BYTES2, 3, value);
      }
    }

    public PlayerFlags PlayerFlags
    {
      get
      {
        return (PlayerFlags) this.GetInt32(PlayerFields.FLAGS);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.FLAGS, (uint) value);
      }
    }

    public int Experience
    {
      get
      {
        return this.GetInt32(PlayerFields.XP);
      }
      set
      {
        this.SetInt32((UpdateFieldId) PlayerFields.XP, value);
      }
    }

    public int NextLevelXP
    {
      get
      {
        return this.GetInt32(PlayerFields.NEXT_LEVEL_XP);
      }
      set
      {
        this.SetInt32((UpdateFieldId) PlayerFields.NEXT_LEVEL_XP, value);
      }
    }

    /// <summary>
    /// The amount of experience to be gained extra due to resting
    /// </summary>
    public int RestXp
    {
      get
      {
        return this.GetInt32(PlayerFields.REST_STATE_EXPERIENCE);
      }
      set
      {
        this.SetInt32((UpdateFieldId) PlayerFields.REST_STATE_EXPERIENCE, value);
      }
    }

    public uint Money
    {
      get
      {
        return (uint) this.Record.Money;
      }
      set
      {
        this.Record.Money = (long) value;
      }
    }

    public void SendMoneyUpdate()
    {
      if (this.Map == null)
        return;
      Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Ok, this.Asda2Inventory.RegularItems[0], this);
    }

    /// <summary>Adds the given amount of money</summary>
    public void AddMoney(uint amount)
    {
      Log.Create(Log.Types.ItemOperations, LogSourceType.Character, this.EntryId).AddAttribute("source", 0.0, "add_money").AddAttribute("current", (double) this.Money, "").AddAttribute("diff", (double) amount, "").Write();
      this.Money += amount;
    }

    /// <summary>
    /// Subtracts the given amount of Money. Returns false if its more than this Character has.
    /// </summary>
    public bool SubtractMoney(uint amount)
    {
      Log.Create(Log.Types.ItemOperations, LogSourceType.Character, this.EntryId).AddAttribute("source", 0.0, "substract_money").AddAttribute("current", (double) this.Money, "").AddAttribute("diff", (double) amount, "").Write();
      uint money = this.Money;
      if (amount > money)
        return false;
      this.Money -= amount;
      return true;
    }

    /// <summary>
    /// Set to <value>-1</value> to disable the watched faction
    /// </summary>
    public int WatchedFaction
    {
      get
      {
        return this.GetInt32(PlayerFields.WATCHED_FACTION_INDEX);
      }
      set
      {
        this.SetInt32((UpdateFieldId) PlayerFields.WATCHED_FACTION_INDEX, value);
      }
    }

    public TitleBitId ChosenTitle
    {
      get
      {
        return (TitleBitId) this.GetUInt32(PlayerFields.CHOSEN_TITLE);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.CHOSEN_TITLE, (uint) value);
      }
    }

    public CharTitlesMask KnownTitleMask
    {
      get
      {
        return (CharTitlesMask) this.GetUInt64((UpdateFieldId) PlayerFields._FIELD_KNOWN_TITLES);
      }
      set
      {
        this.SetUInt64((UpdateFieldId) PlayerFields._FIELD_KNOWN_TITLES, (ulong) value);
      }
    }

    public ulong KnownTitleMask2
    {
      get
      {
        return this.GetUInt64((UpdateFieldId) PlayerFields._FIELD_KNOWN_TITLES1);
      }
      set
      {
        this.SetUInt64((UpdateFieldId) PlayerFields._FIELD_KNOWN_TITLES1, value);
      }
    }

    public ulong KnownTitleMask3
    {
      get
      {
        return this.GetUInt64((UpdateFieldId) PlayerFields._FIELD_KNOWN_TITLES2);
      }
      set
      {
        this.SetUInt64((UpdateFieldId) PlayerFields._FIELD_KNOWN_TITLES2, value);
      }
    }

    public uint KillsTotal
    {
      get
      {
        return this.GetUInt32(PlayerFields.KILLS);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.KILLS, value);
      }
    }

    public ushort KillsToday
    {
      get
      {
        return this.GetUInt16Low((UpdateFieldId) PlayerFields.KILLS);
      }
      set
      {
        this.SetUInt16Low((UpdateFieldId) PlayerFields.KILLS, value);
      }
    }

    public ushort KillsYesterday
    {
      get
      {
        return this.GetUInt16High((UpdateFieldId) PlayerFields.KILLS);
      }
      set
      {
        this.SetUInt16High((UpdateFieldId) PlayerFields.KILLS, value);
      }
    }

    public uint HonorToday
    {
      get
      {
        return this.GetUInt32(PlayerFields.TODAY_CONTRIBUTION);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.TODAY_CONTRIBUTION, value);
      }
    }

    public uint HonorYesterday
    {
      get
      {
        return this.GetUInt32(PlayerFields.YESTERDAY_CONTRIBUTION);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.YESTERDAY_CONTRIBUTION, value);
      }
    }

    public uint LifetimeHonorableKills
    {
      get
      {
        return this.GetUInt32(PlayerFields.LIFETIME_HONORBALE_KILLS);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.LIFETIME_HONORBALE_KILLS, value);
      }
    }

    public uint HonorPoints
    {
      get
      {
        return this.GetUInt32(PlayerFields.HONOR_CURRENCY);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.HONOR_CURRENCY, value);
      }
    }

    public uint ArenaPoints
    {
      get
      {
        return this.GetUInt32(PlayerFields.ARENA_CURRENCY);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.ARENA_CURRENCY, value);
      }
    }

    public uint GuildId
    {
      get
      {
        return this.GetUInt32(PlayerFields.GUILDID);
      }
      internal set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.GUILDID, value);
      }
    }

    public uint GuildRank
    {
      get
      {
        return this.GetUInt32(PlayerFields.GUILDRANK);
      }
      internal set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.GUILDRANK, value);
      }
    }

    public void SetArenaTeamInfoField(ArenaTeamSlot slot, ArenaTeamInfoType type, uint value)
    {
      this.SetUInt32((int) (1256 + (int) slot * 7 + type), value);
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
          this.GetUInt32(PlayerFields.NO_REAGENT_COST_1),
          this.GetUInt32(PlayerFields.NO_REAGENT_COST_1_2),
          this.GetUInt32(PlayerFields.NO_REAGENT_COST_1_3)
        };
      }
      internal set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.NO_REAGENT_COST_1, value[0]);
        this.SetUInt32((UpdateFieldId) PlayerFields.NO_REAGENT_COST_1_2, value[1]);
        this.SetUInt32((UpdateFieldId) PlayerFields.NO_REAGENT_COST_1_3, value[2]);
      }
    }

    public override Faction DefaultFaction
    {
      get
      {
        return FactionMgr.Get(this.Race);
      }
    }

    public byte CharNum
    {
      get
      {
        return this.Record.CharNum;
      }
    }

    public uint UniqId
    {
      get
      {
        return (uint) (this.Account.AccountId + 1000000 * (int) this.CharNum);
      }
    }

    public int ReputationGainModifierPercent { get; set; }

    public int KillExperienceGainModifierPercent { get; set; }

    public int QuestExperienceGainModifierPercent
    {
      get
      {
        return 0;
      }
      set
      {
        this.QuestExperienceGainModifierPercent = value;
      }
    }

    /// <summary>
    /// Gets the total modifier of the corresponding CombatRating (in %)
    /// </summary>
    public int GetCombatRating(CombatRating rating)
    {
      return this.GetInt32((PlayerFields) (1230U + rating));
    }

    public void SetCombatRating(CombatRating rating, int value)
    {
      this.SetInt32((UpdateFieldId) ((PlayerFields) (1230U + rating)), value);
      this.UpdateChancesByCombatRating(rating);
    }

    /// <summary>
    /// Modifies the given CombatRating modifier by the given delta
    /// </summary>
    public void ModCombatRating(CombatRating rating, int delta)
    {
      int num = this.GetInt32((PlayerFields) (1230U + rating)) + delta;
      this.SetInt32((UpdateFieldId) ((PlayerFields) (1230U + rating)), num);
      this.UpdateChancesByCombatRating(rating);
    }

    public void ModCombatRating(uint[] ratings, int delta)
    {
      for (int index = 0; index < ratings.Length; ++index)
        this.ModCombatRating((CombatRating) ratings[index], delta);
    }

    public CreatureMask CreatureTracking
    {
      get
      {
        return (CreatureMask) this.GetUInt32(PlayerFields.TRACK_CREATURES);
      }
      internal set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.TRACK_CREATURES, (uint) value);
      }
    }

    public LockMask ResourceTracking
    {
      get
      {
        return (LockMask) this.GetUInt32(PlayerFields.TRACK_RESOURCES);
      }
      internal set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.TRACK_RESOURCES, (uint) value);
      }
    }

    public float BlockChance
    {
      get
      {
        return this.GetFloat((UpdateFieldId) PlayerFields.BLOCK_PERCENTAGE);
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) PlayerFields.BLOCK_PERCENTAGE, value);
      }
    }

    /// <summary>Amount of damage reduced when an attack is blocked</summary>
    public uint BlockValue
    {
      get
      {
        return this.GetUInt32(PlayerFields.SHIELD_BLOCK);
      }
      internal set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.SHIELD_BLOCK, value);
      }
    }

    /// <summary>Value in %</summary>
    public float DodgeChance
    {
      get
      {
        return this.GetFloat((UpdateFieldId) PlayerFields.DODGE_PERCENTAGE);
      }
      set
      {
        this.SetFloat((UpdateFieldId) PlayerFields.DODGE_PERCENTAGE, value);
      }
    }

    public override float ParryChance
    {
      get
      {
        return this.GetFloat((UpdateFieldId) PlayerFields.PARRY_PERCENTAGE);
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) PlayerFields.PARRY_PERCENTAGE, value);
      }
    }

    public uint Expertise
    {
      get
      {
        return this.GetUInt32(PlayerFields.EXPERTISE);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.EXPERTISE, value);
      }
    }

    public float CritChanceMeleePct
    {
      get
      {
        return this.GetFloat((UpdateFieldId) PlayerFields.CRIT_PERCENTAGE);
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) PlayerFields.CRIT_PERCENTAGE, value);
      }
    }

    public float CritChanceRangedPct
    {
      get
      {
        return this.GetFloat((UpdateFieldId) PlayerFields.RANGED_CRIT_PERCENTAGE);
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) PlayerFields.RANGED_CRIT_PERCENTAGE, value);
      }
    }

    public float CritChanceOffHandPct
    {
      get
      {
        return this.GetFloat((UpdateFieldId) PlayerFields.OFFHAND_CRIT_PERCENTAGE);
      }
      internal set
      {
        this.SetFloat((UpdateFieldId) PlayerFields.OFFHAND_CRIT_PERCENTAGE, value);
      }
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
        return (float) this.GetCombatRating(CombatRating.SpellHitChance) / GameTables.CombatRatings[CombatRating.SpellHitChance][this.CasterLevel - 1];
      }
    }

    public void ResetQuest(int slot)
    {
      int num = slot * 5;
      this.SetUInt32((UpdateFieldId) ((PlayerFields) (158 + num)), 0U);
      this.SetUInt32((UpdateFieldId) ((PlayerFields) (159 + num)), 0U);
      this.SetUInt32((UpdateFieldId) ((PlayerFields) (160 + num)), 0U);
      this.SetUInt32((UpdateFieldId) ((PlayerFields) (161 + num)), 0U);
      this.SetUInt32((UpdateFieldId) ((PlayerFields) (162 + num)), 0U);
    }

    /// <summary>Gets the quest field.</summary>
    /// <param name="slot">The slot.</param>
    public uint GetQuestId(int slot)
    {
      return this.GetUInt32((PlayerFields) (158 + slot * 5));
    }

    /// <summary>
    /// Sets the quest field, where fields are indexed from 0.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="questid">The questid.</param>
    public void SetQuestId(int slot, uint questid)
    {
      this.SetUInt32((UpdateFieldId) ((PlayerFields) (158 + slot * 5)), questid);
    }

    /// <summary>Gets the state of the quest.</summary>
    /// <param name="slot">The slot.</param>
    /// <returns></returns>
    public QuestCompleteStatus GetQuestState(int slot)
    {
      return (QuestCompleteStatus) this.GetUInt32((PlayerFields) (159 + slot * 5));
    }

    /// <summary>Sets the state of the quest.</summary>
    /// <param name="slot">The slot.</param>
    /// <param name="completeStatus">The status.</param>
    public void SetQuestState(int slot, QuestCompleteStatus completeStatus)
    {
      this.SetUInt32((UpdateFieldId) ((PlayerFields) (159 + slot * 5)), (uint) completeStatus);
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
      if (interactionIndex % 2U == 0U)
        this.SetUInt16Low((UpdateFieldId) playerFields, value);
      else
        this.SetUInt16High((UpdateFieldId) playerFields, value);
    }

    /// <summary>Gets the quest time.</summary>
    /// <param name="slot">The slot.</param>
    /// <returns></returns>
    internal uint GetQuestTimeLeft(byte slot)
    {
      return this.GetUInt32((PlayerFields) (162 + (int) slot * 5));
    }

    /// <summary>Sets the quest time.</summary>
    /// <param name="slot">The slot.</param>
    internal void SetQuestTimeLeft(byte slot, uint timeleft)
    {
      this.SetUInt32((UpdateFieldId) ((PlayerFields) (162 + (int) slot * 5)), timeleft);
    }

    /// <summary>This array stores completed daily quests</summary>
    /// <returns></returns>
    public uint[] DailyQuests
    {
      get
      {
        uint[] numArray = new uint[25];
        for (int index = 0; index < 25; ++index)
          numArray[index] = this.GetUInt32((PlayerFields) (1280 + index));
        return numArray;
      }
    }

    /// <summary>Gets the quest field.</summary>
    /// <param name="slot">The slot.</param>
    public uint GetDailyQuest(byte slot)
    {
      return this.GetUInt32((PlayerFields) (1280 + (int) slot));
    }

    /// <summary>
    /// Sets the quest field, where fields are indexed from 0.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="questid">The questid.</param>
    public void SetDailyQuest(byte slot, uint questid)
    {
      this.SetUInt32((UpdateFieldId) ((PlayerFields) (1280 + (int) slot)), questid);
    }

    public void ResetDailyQuests()
    {
      for (int index = 0; index < 25; ++index)
        this.SetUInt32((UpdateFieldId) ((PlayerFields) (1280 + index)), 0U);
    }

    /// <summary>
    /// Modifies the damage for the given school by the given delta.
    /// </summary>
    protected internal override void AddDamageDoneModSilently(DamageSchool school, int delta)
    {
      if (delta == 0)
        return;
      PlayerFields playerFields = delta <= 0 ? PlayerFields.MOD_DAMAGE_DONE_NEG : PlayerFields.MOD_DAMAGE_DONE_POS;
      this.SetInt32((UpdateFieldId) ((PlayerFields) ((int) playerFields + (int) school)), this.GetInt32((PlayerFields) ((int) playerFields + (int) school)) + delta);
    }

    /// <summary>
    /// Modifies the damage for the given school by the given delta.
    /// </summary>
    protected internal override void RemoveDamageDoneModSilently(DamageSchool school, int delta)
    {
      if (delta == 0)
        return;
      PlayerFields playerFields = delta <= 0 ? PlayerFields.MOD_DAMAGE_DONE_NEG : PlayerFields.MOD_DAMAGE_DONE_POS;
      this.SetUInt32((UpdateFieldId) ((PlayerFields) ((int) playerFields + (int) school)), this.GetUInt32((PlayerFields) ((int) playerFields + (int) school)) - (uint) delta);
    }

    protected internal override void ModDamageDoneFactorSilently(DamageSchool school, float delta)
    {
      if ((double) delta == 0.0)
        return;
      PlayerFields playerFields = (PlayerFields) (1185U + school);
      this.SetFloat((UpdateFieldId) playerFields, this.GetFloat((UpdateFieldId) playerFields) + delta);
    }

    public override float GetDamageDoneFactor(DamageSchool school)
    {
      return this.GetFloat((UpdateFieldId) ((PlayerFields) (1185U + school)));
    }

    public override int GetDamageDoneMod(DamageSchool school)
    {
      return this.GetInt32((PlayerFields) (1171U + school)) - this.GetInt32((PlayerFields) (1178U + school));
    }

    /// <summary>Increased healing done *by* this Character</summary>
    public int HealingDoneMod
    {
      get
      {
        return this.GetInt32(PlayerFields.MOD_HEALING_DONE_POS);
      }
      set
      {
        this.SetInt32((UpdateFieldId) PlayerFields.MOD_HEALING_DONE_POS, value);
      }
    }

    /// <summary>Increased healing % done *by* this Character</summary>
    public float HealingDoneModPct
    {
      get
      {
        return this.GetFloat((UpdateFieldId) PlayerFields.MOD_HEALING_DONE_PCT);
      }
      set
      {
        this.SetFloat((UpdateFieldId) PlayerFields.MOD_HEALING_DONE_PCT, value);
      }
    }

    /// <summary>Increased healing done *to* this Character</summary>
    public float HealingTakenModPct
    {
      get
      {
        return this.GetFloat((UpdateFieldId) PlayerFields.MOD_HEALING_PCT);
      }
      set
      {
        this.SetFloat((UpdateFieldId) PlayerFields.MOD_HEALING_PCT, value);
      }
    }

    /// <summary>
    /// Returns the SpellCritChance for the given DamageType (0-100)
    /// </summary>
    public override float GetCritChance(DamageSchool school)
    {
      return this.GetFloat((UpdateFieldId) ((PlayerFields) (1032U + school)));
    }

    /// <summary>Sets the SpellCritChance for the given DamageType</summary>
    internal void SetCritChance(DamageSchool school, float val)
    {
      this.SetFloat((UpdateFieldId) ((PlayerFields) (1032U + school)), val);
    }

    public EntityId FarSight
    {
      get
      {
        return this.GetEntityId((UpdateFieldId) PlayerFields.FARSIGHT);
      }
      set
      {
        this.SetEntityId((UpdateFieldId) PlayerFields.FARSIGHT, value);
      }
    }

    /// <summary>
    /// Make sure that the given slot is actually an EquipmentSlot
    /// </summary>
    internal void SetVisibleItem(InventorySlot slot, Asda2Item item)
    {
      PlayerFields playerFields = (PlayerFields) (283 + (int) slot * 2);
      if (item != null)
        this.SetUInt32((UpdateFieldId) playerFields, item.Template.Id);
      else
        this.SetUInt32((UpdateFieldId) playerFields, 0U);
    }

    /// <summary>Sets an ActionButton with the given information.</summary>
    public void BindActionButton(uint btnIndex, uint action, byte type, bool update = true)
    {
      this.CurrentSpecProfile.State = RecordState.Dirty;
      byte[] actionButtons = this.CurrentSpecProfile.ActionButtons;
      btnIndex *= 4U;
      if (action == 0U)
      {
        Array.Copy((Array) ActionButton.EmptyButton, 0L, (Array) actionButtons, (long) btnIndex, 4L);
      }
      else
      {
        actionButtons[btnIndex] = (byte) (action & (uint) byte.MaxValue);
        actionButtons[btnIndex + 1U] = (byte) ((action & 65280U) >> 8);
        actionButtons[btnIndex + 2U] = (byte) ((action & 16711680U) >> 16);
        actionButtons[btnIndex + 3U] = type;
      }
    }

    public uint GetActionFromActionButton(int buttonIndex)
    {
      byte[] actionButtons = this.CurrentSpecProfile.ActionButtons;
      buttonIndex *= 4;
      return BitConverter.ToUInt32(actionButtons, buttonIndex) & 16777215U;
    }

    public byte GetTypeFromActionButton(int buttonIndex)
    {
      buttonIndex *= 4;
      return this.CurrentSpecProfile.ActionButtons[buttonIndex + 3];
    }

    /// <summary>
    /// Sets the given button to the given spell and resends it to the client
    /// </summary>
    public void BindSpellToActionButton(uint btnIndex, SpellId spell, bool update = true)
    {
      this.BindActionButton(btnIndex, (uint) spell, (byte) 0, true);
    }

    /// <summary>Sets the given action button</summary>
    public void BindActionButton(ActionButton btn, bool update = true)
    {
      btn.Set(this.CurrentSpecProfile.ActionButtons);
      this.CurrentSpecProfile.State = RecordState.Dirty;
    }

    /// <summary>
    /// 
    /// </summary>
    public byte[] ActionButtons
    {
      get
      {
        return this.CurrentSpecProfile.ActionButtons;
      }
    }

    public override ObjectTypeCustom CustomType
    {
      get
      {
        return ObjectTypeCustom.Object | ObjectTypeCustom.Unit | ObjectTypeCustom.Player;
      }
    }

    public CharacterRecord Record
    {
      get
      {
        return this.m_record;
      }
    }

    /// <summary>
    /// The active ticket of this Character or null if there is none
    /// </summary>
    public Ticket Ticket { get; internal set; }

    public override int Health
    {
      get
      {
        return base.Health;
      }
      set
      {
        if (this.Health == value)
          return;
        base.Health = value;
        if (this.Map == null)
          return;
        this.UpdateTargeters();
        if (this.IsInGroup)
          Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
        if (!this.IsSoulmated)
          return;
        Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(this.Client);
      }
    }

    public override int MaxHealth
    {
      get
      {
        return base.MaxHealth;
      }
      internal set
      {
        base.MaxHealth = value;
        if (this.Map == null)
          return;
        Asda2CharacterHandler.SendHealthUpdate(this, false, false);
        if (this.IsInGroup)
          Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
        if (this.IsSoulmated)
          Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(this.Client);
        this.UpdateTargeters();
      }
    }

    private void UpdateTargeters()
    {
      Asda2CharacterHandler.SendSelectedCharacterInfoToMultipyTargets(this, this.TargetersOnMe.ToArray());
    }

    public int BaseHealthCapacity
    {
      get
      {
        return this.m_archetype.Class.GetLevelSetting(this.Level).Health;
      }
    }

    public override int Power
    {
      get
      {
        return base.Power;
      }
      set
      {
        if (this.Power == value)
          return;
        base.Power = value;
        Unit.SendPowerUpdates(this);
      }
    }

    public override PowerType PowerType
    {
      get
      {
        return base.PowerType;
      }
      set
      {
        base.PowerType = value;
        this.GroupUpdateFlags |= GroupUpdateFlags.PowerType | GroupUpdateFlags.Power | GroupUpdateFlags.MaxPower;
      }
    }

    public int BaseManaPoolCapacity
    {
      get
      {
        return this.m_archetype.Class.GetLevelSetting(this.Level).Mana;
      }
    }

    public override int MaxPower
    {
      get
      {
        return base.MaxPower;
      }
      internal set
      {
        base.MaxPower = value;
        if (this.Power > this.MaxPower)
          this.Power = this.MaxPower;
        Asda2CharacterHandler.SendCharMpUpdateResponse(this);
        if (this.IsInGroup)
          Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
        if (!this.IsSoulmated)
          return;
        Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(this.Client);
      }
    }

    public override Map Map
    {
      get
      {
        return base.Map;
      }
      internal set
      {
        base.Map = value;
        if (!this.IsInGuild)
          return;
        Asda2GuildHandler.SendGuildNotificationResponse(this.Guild, GuildNotificationType.Silence, this.GuildMember);
      }
    }

    public override int Level
    {
      get
      {
        return base.Level;
      }
      set
      {
        base.Level = value;
        this.NextLevelXP = XpGenerator.GetXpForlevel(value + 1);
        if (this.Map == null)
          return;
        if (this.IsInGroup)
          Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
        if (this.IsSoulmated)
          Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(this.Client);
        if (this.IsInGuild)
          Asda2GuildHandler.SendGuildNotificationResponse(this.Guild, GuildNotificationType.Silence, this.GuildMember);
        this.UpdateTargeters();
      }
    }

    public override int MaxLevel
    {
      get
      {
        return this.GetInt32(PlayerFields.MAX_LEVEL);
      }
      internal set
      {
        this.SetInt32((UpdateFieldId) PlayerFields.MAX_LEVEL, value);
      }
    }

    public override Zone Zone
    {
      get
      {
        return base.Zone;
      }
      internal set
      {
        if (this.m_zone == value)
          return;
        if (value != null && this.m_Map != null)
          value.EnterZone(this, this.m_zone);
        base.Zone = value;
        this.GroupUpdateFlags |= GroupUpdateFlags.ZoneId;
      }
    }

    public bool IsZoneExplored(ZoneId id)
    {
      ZoneTemplate zoneInfo = WCell.RealmServer.Global.World.GetZoneInfo(id);
      if (zoneInfo != null)
        return this.IsZoneExplored(zoneInfo);
      return false;
    }

    public bool IsZoneExplored(ZoneTemplate zone)
    {
      return this.IsZoneExplored(zone.ExplorationBit);
    }

    public bool IsZoneExplored(int explorationBit)
    {
      int index = explorationBit >> 3;
      if (index >> 2 >= UpdateFieldMgr.ExplorationZoneFieldSize)
        return false;
      int num = 1 << explorationBit % 8;
      return ((int) this.m_record.ExploredZones[index] & num) != 0;
    }

    public void SetZoneExplored(ZoneId id, bool explored)
    {
    }

    public void SetZoneExplored(ZoneTemplate zone, bool gainXp)
    {
    }

    public override Vector3 Position
    {
      get
      {
        return base.Position;
      }
      internal set
      {
        base.Position = value;
        this.GroupUpdateFlags |= GroupUpdateFlags.Position;
      }
    }

    public override uint Phase
    {
      get
      {
        return this.m_Phase;
      }
      set
      {
        this.m_Phase = value;
      }
    }

    public override bool IsInWorld
    {
      get
      {
        return this.m_initialized;
      }
    }

    /// <summary>The type of this object (player, corpse, item, etc)</summary>
    public override ObjectTypeId ObjectTypeId
    {
      get
      {
        return ObjectTypeId.Player;
      }
    }

    /// <summary>The client currently playing the character.</summary>
    public IRealmClient Client
    {
      get
      {
        return this.m_client;
      }
      protected set
      {
        this.m_client = value;
      }
    }

    /// <summary>The status of the character.</summary>
    public CharacterStatus Status
    {
      get
      {
        CharacterStatus characterStatus = CharacterStatus.OFFLINE;
        if (this.IsAFK)
          characterStatus |= CharacterStatus.AFK;
        if (this.IsDND)
          characterStatus |= CharacterStatus.DND;
        if (this.IsInWorld)
          characterStatus |= CharacterStatus.ONLINE;
        return characterStatus;
      }
    }

    /// <summary>
    /// The GroupMember object of this Character (if it he/she is in any group)
    /// </summary>
    public GroupMember GroupMember
    {
      get
      {
        return this.m_groupMember;
      }
      internal set
      {
        this.m_groupMember = value;
      }
    }

    /// <summary>
    /// The GuildMember object of this Character (if it he/she is in a guild)
    /// </summary>
    public GuildMember GuildMember
    {
      get
      {
        return this.m_guildMember;
      }
      set
      {
        this.m_guildMember = value;
        if (this.m_guildMember != null)
        {
          this.GuildId = this.m_guildMember.Guild.Id;
          this.GuildRank = (uint) this.m_guildMember.RankId;
        }
        else
        {
          this.GuildId = 0U;
          this.GuildRank = 0U;
        }
      }
    }

    /// <summary>
    /// The ArenaTeamMember object of this Character (if it he/she is in an arena team)
    /// </summary>
    public WCell.RealmServer.Battlegrounds.Arenas.ArenaTeamMember[] ArenaTeamMember
    {
      get
      {
        return this.m_arenaTeamMember;
      }
    }

    /// <summary>
    /// Characters get disposed after Logout sequence completed and
    /// cannot (and must not) be used anymore.
    /// </summary>
    public bool IsDisposed
    {
      get
      {
        return this.m_auras == null;
      }
    }

    /// <summary>
    /// The Group of this Character (if it he/she is in any group)
    /// </summary>
    public Group Group
    {
      get
      {
        if (this.m_groupMember == null)
          return (Group) null;
        return this.m_groupMember.SubGroup?.Group;
      }
    }

    /// <summary>The subgroup in which the character is (if any)</summary>
    public SubGroup SubGroup
    {
      get
      {
        if (this.m_groupMember != null)
          return this.m_groupMember.SubGroup;
        return (SubGroup) null;
      }
    }

    public GroupUpdateFlags GroupUpdateFlags
    {
      get
      {
        return this.m_groupUpdateFlags;
      }
      set
      {
        this.m_groupUpdateFlags = value;
      }
    }

    /// <summary>The guild in which the character is (if any)</summary>
    public Guild Guild
    {
      get
      {
        if (this.m_guildMember != null)
          return this.m_guildMember.Guild;
        return (Guild) null;
      }
    }

    /// <summary>The account this character belongs to.</summary>
    public RealmAccount Account { get; protected internal set; }

    public RoleGroup Role
    {
      get
      {
        RealmAccount account = this.Account;
        if (account == null)
          return Singleton<PrivilegeMgr>.Instance.LowestRole;
        return account.Role;
      }
    }

    public override ClientLocale Locale
    {
      get
      {
        return this.m_client.Info.Locale;
      }
      set
      {
        this.m_client.Info.Locale = value;
      }
    }

    /// <summary>The name of this character.</summary>
    public override string Name
    {
      get
      {
        return this.m_name;
      }
      set
      {
        throw new NotImplementedException("Dynamic renaming of Characters is not yet implemented.");
      }
    }

    public Corpse Corpse
    {
      get
      {
        return this.m_corpse;
      }
      internal set
      {
        if (value == null && this.m_corpse != null)
        {
          this.m_corpse.StartDecay();
          this.m_record.CorpseX = new float?();
        }
        this.m_corpse = value;
      }
    }

    /// <summary>
    /// The <see cref="P:WCell.RealmServer.Entities.Character.Archetype">Archetype</see> of this Character
    /// </summary>
    public Archetype Archetype
    {
      get
      {
        return this.m_archetype;
      }
      set
      {
        this.m_archetype = value;
        this.Race = value.Race.Id;
        this.Class = value.Class.Id;
        Asda2CharacterHandler.SendChangeProfessionResponse(this.Client);
        if (!this.IsInGuild)
          return;
        Asda2GuildHandler.SendGuildNotificationResponse(this.Guild, GuildNotificationType.Silence, this.GuildMember);
      }
    }

    /// <summary>
    /// </summary>
    public byte Outfit { get; set; }

    /// <summary>The channels the character is currently joined to.</summary>
    public List<ChatChannel> ChatChannels
    {
      get
      {
        return this.m_chatChannels;
      }
      set
      {
        this.m_chatChannels = value;
      }
    }

    /// <summary>
    /// Whether this Character is currently trading with someone
    /// </summary>
    public bool IsTrading
    {
      get
      {
        return this.m_tradeWindow != null;
      }
    }

    /// <summary>
    /// Current trading progress of the character
    /// Null if none
    /// </summary>
    public TradeWindow TradeWindow
    {
      get
      {
        return this.m_tradeWindow;
      }
      set
      {
        this.m_tradeWindow = value;
      }
    }

    /// <summary>Last login time of this character.</summary>
    public DateTime LastLogin
    {
      get
      {
        return this.m_record.LastLogin.Value;
      }
      set
      {
        this.m_record.LastLogin = new DateTime?(value);
      }
    }

    /// <summary>Last logout time of this character.</summary>
    public DateTime? LastLogout
    {
      get
      {
        return this.m_record.LastLogout;
      }
      set
      {
        this.m_record.LastLogout = value;
      }
    }

    public bool IsFirstLogin
    {
      get
      {
        return !this.m_record.LastLogout.HasValue;
      }
    }

    public TutorialFlags TutorialFlags { get; set; }

    /// <summary>Total play time of this Character in seconds</summary>
    public uint TotalPlayTime
    {
      get
      {
        return (uint) this.m_record.TotalPlayTime;
      }
      set
      {
        this.m_record.TotalPlayTime = (int) value;
      }
    }

    /// <summary>
    /// How long is this Character already on this level in seconds
    /// </summary>
    public uint LevelPlayTime
    {
      get
      {
        return (uint) this.m_record.LevelPlayTime;
      }
      set
      {
        this.m_record.LevelPlayTime = (int) value;
      }
    }

    /// <summary>Whether or not this character has the GM-tag set.</summary>
    public bool ShowAsGameMaster
    {
      get
      {
        return this.PlayerFlags.HasFlag((Enum) PlayerFlags.GM);
      }
      set
      {
        if (value)
          this.PlayerFlags |= PlayerFlags.GM;
        else
          this.PlayerFlags &= ~PlayerFlags.GM;
      }
    }

    /// <summary>Gets/Sets the godmode</summary>
    public bool GodMode
    {
      get
      {
        return this.m_record.GodMode;
      }
      set
      {
        this.m_record.GodMode = value;
        SpellCast spellCast = this.m_spellCast;
        if (spellCast != null)
          spellCast.GodMode = value;
        if (value)
        {
          this.Health = this.MaxHealth;
          this.Power = this.MaxPower;
          this.m_spells.ClearCooldowns();
          this.ShowAsGameMaster = true;
          this.IncMechanicCount(SpellMechanic.Invulnerable, false);
        }
        else
        {
          this.DecMechanicCount(SpellMechanic.Invulnerable, false);
          this.ShowAsGameMaster = false;
        }
      }
    }

    protected override void InitSpellCast()
    {
      base.InitSpellCast();
      this.m_spellCast.GodMode = this.GodMode;
    }

    /// <summary>Whether the PvP Flag is set.</summary>
    public bool IsPvPFlagSet
    {
      get
      {
        return this.PlayerFlags.HasFlag((Enum) PlayerFlags.PVP);
      }
      set
      {
        if (value)
          this.PlayerFlags |= PlayerFlags.PVP;
        else
          this.PlayerFlags &= ~PlayerFlags.PVP;
      }
    }

    /// <summary>Whether the PvP Flag reset timer is active.</summary>
    public bool IsPvPTimerActive
    {
      get
      {
        return this.PlayerFlags.HasFlag((Enum) PlayerFlags.PVPTimerActive);
      }
      set
      {
        if (value)
          this.PlayerFlags |= PlayerFlags.PVPTimerActive;
        else
          this.PlayerFlags &= ~PlayerFlags.PVPTimerActive;
      }
    }

    /// <summary>Whether or not this character is AFK.</summary>
    public bool IsAFK
    {
      get
      {
        return this.PlayerFlags.HasFlag((Enum) PlayerFlags.AFK);
      }
      set
      {
        if (value)
          this.PlayerFlags |= PlayerFlags.AFK;
        else
          this.PlayerFlags &= ~PlayerFlags.AFK;
        this.GroupUpdateFlags |= GroupUpdateFlags.Status;
      }
    }

    /// <summary>The custom AFK reason when player is AFK.</summary>
    public string AFKReason { get; set; }

    /// <summary>Whether or not this character is DND.</summary>
    public bool IsDND
    {
      get
      {
        return this.PlayerFlags.HasFlag((Enum) PlayerFlags.DND);
      }
      set
      {
        if (value)
          this.PlayerFlags |= PlayerFlags.DND;
        else
          this.PlayerFlags &= ~PlayerFlags.DND;
        this.GroupUpdateFlags |= GroupUpdateFlags.Status;
      }
    }

    /// <summary>The custom DND reason when player is DND.</summary>
    public string DNDReason { get; set; }

    /// <summary>Gets the chat tag for the character.</summary>
    public override ChatTag ChatTag
    {
      get
      {
        if (this.ShowAsGameMaster)
          return ChatTag.GM;
        if (this.IsAFK)
          return ChatTag.AFK;
        return this.IsDND ? ChatTag.DND : ChatTag.None;
      }
    }

    /// <summary>
    /// Collection of reputations with all factions known to this Character
    /// </summary>
    public ReputationCollection Reputations
    {
      get
      {
        return this.m_reputations;
      }
    }

    /// <summary>Collection of all this Character's skills</summary>
    public SkillCollection Skills
    {
      get
      {
        return this.m_skills;
      }
    }

    /// <summary>Collection of all this Character's Talents</summary>
    public override TalentCollection Talents
    {
      get
      {
        return this.m_talents;
      }
    }

    /// <summary>Collection of all this Character's Achievements</summary>
    public AchievementCollection Achievements
    {
      get
      {
        return this.m_achievements;
      }
    }

    /// <summary>All spells known to this chr</summary>
    public PlayerAuraCollection PlayerAuras
    {
      get
      {
        return (PlayerAuraCollection) this.m_auras;
      }
    }

    /// <summary>All spells known to this chr</summary>
    public PlayerSpellCollection PlayerSpells
    {
      get
      {
        return (PlayerSpellCollection) this.m_spells;
      }
    }

    /// <summary>Mask of the activated Flight Paths</summary>
    public TaxiNodeMask TaxiNodes
    {
      get
      {
        return this.m_taxiNodeMask;
      }
    }

    /// <summary>The Tavern-location of where the Player bound to</summary>
    public IWorldZoneLocation BindLocation
    {
      get
      {
        this.CheckBindLocation();
        return this.m_bindLocation;
      }
      internal set
      {
        this.m_bindLocation = value;
      }
    }

    /// <summary>
    /// The Inventory of this Character contains all Items and Item-related things
    /// </summary>
    public PlayerInventory Inventory
    {
      get
      {
        return this.m_inventory;
      }
    }

    public Asda2PlayerInventory Asda2Inventory
    {
      get
      {
        return this._asda2Inventory;
      }
    }

    /// <summary>
    /// Returns the same as Inventory but with another type (for IContainer interface)
    /// </summary>
    public BaseInventory BaseInventory
    {
      get
      {
        return (BaseInventory) this.m_inventory;
      }
    }

    /// <summary>The Character's MailAccount</summary>
    public MailAccount MailAccount
    {
      get
      {
        return this.m_mailAccount;
      }
      set
      {
        if (this.m_mailAccount == value)
          return;
        this.m_mailAccount = value;
      }
    }

    /// <summary>Unused talent-points for this Character</summary>
    public int FreeTalentPoints
    {
      get
      {
        return (int) this.GetUInt32(PlayerFields.CHARACTER_POINTS1);
      }
      set
      {
        if (value < 0)
          value = 0;
        this.SetUInt32((UpdateFieldId) PlayerFields.CHARACTER_POINTS1, (uint) value);
        TalentHandler.SendTalentGroupList(this.m_talents);
      }
    }

    /// <summary>Doesn't send a packet to the client</summary>
    public void UpdateFreeTalentPointsSilently(int delta)
    {
      this.SetUInt32((UpdateFieldId) PlayerFields.CHARACTER_POINTS1, (uint) (this.FreeTalentPoints + delta));
    }

    /// <summary>Forced logout must not be cancelled</summary>
    public bool IsKicked
    {
      get
      {
        if (this.m_isLoggingOut)
          return !this.IsPlayerLogout;
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
      this.GossipConversation = new GossipConversation(menu, this, speaker, menu.KeepOpen);
      this.GossipConversation.DisplayCurrentMenu();
    }

    /// <summary>Lets the Character gossip with herself</summary>
    public void StartGossip(GossipMenu menu)
    {
      this.GossipConversation = new GossipConversation(menu, this, (WorldObject) this, menu.KeepOpen);
      this.GossipConversation.DisplayCurrentMenu();
    }

    /// <summary>
    /// Returns whether this Character is invited into a Group already
    /// </summary>
    /// <returns></returns>
    public bool IsInvitedToGroup
    {
      get
      {
        return Singleton<RelationMgr>.Instance.HasPassiveRelations(this.EntityId.Low, CharacterRelationType.GroupInvite);
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
        return Singleton<RelationMgr>.Instance.HasPassiveRelations(this.EntityId.Low, CharacterRelationType.GuildInvite);
      }
    }

    public bool HasTitle(TitleId titleId)
    {
      CharacterTitleEntry titleEntry = TitleMgr.GetTitleEntry(titleId);
      if (titleEntry == null)
        return false;
      TitleBitId bitIndex = titleEntry.BitIndex;
      return ((CharTitlesMask) this.GetUInt32((int) bitIndex / 32 + 626)).HasFlag((Enum) (CharTitlesMask) (uint) (1 << (int) bitIndex % 32));
    }

    public bool HasTitle(TitleBitId titleBitId)
    {
      CharacterTitleEntry titleEntry = TitleMgr.GetTitleEntry(titleBitId);
      if (titleEntry == null)
        return false;
      return this.HasTitle(titleEntry.TitleId);
    }

    public void SetTitle(TitleId titleId, bool lost)
    {
      CharacterTitleEntry titleEntry = TitleMgr.GetTitleEntry(titleId);
      if (titleEntry == null)
      {
        Unit.log.Warn(string.Format("TitleId: {0} could not be found.", (object) titleId));
      }
      else
      {
        TitleBitId bitIndex = titleEntry.BitIndex;
        int field = (int) bitIndex / 32 + 626;
        uint num1 = (uint) (1 << (int) bitIndex % 32);
        if (lost)
        {
          if (!this.HasTitle(titleId))
            return;
          uint num2 = this.GetUInt32(field) & ~num1;
          this.SetUInt32(field, num2);
        }
        else
        {
          if (this.HasTitle(titleId))
            return;
          uint num2 = this.GetUInt32(field) | num1;
          this.SetUInt32(field, num2);
        }
        TitleHandler.SendTitleEarned(this, titleEntry, lost);
      }
    }

    public uint Glyphs_Enable
    {
      get
      {
        return this.GetUInt32(PlayerFields.GLYPHS_ENABLED);
      }
      set
      {
        this.SetUInt32((UpdateFieldId) PlayerFields.GLYPHS_ENABLED, value);
      }
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
        switch (this.Class)
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
      get
      {
        return this.m_record.ProfessionLevel;
      }
      set
      {
        this.m_record.ProfessionLevel = value;
        if (!this.IsInGuild)
          return;
        Asda2GuildHandler.SendGuildNotificationResponse(this.Guild, GuildNotificationType.Silence, this.GuildMember);
      }
    }

    public int MagicDefence { get; set; }

    public Asda2ClassMask Asda2ClassMask
    {
      get
      {
        return this.Archetype.Class.ClassMask;
      }
    }

    public byte EyesColor
    {
      get
      {
        return this.Record.EyesColor;
      }
      set
      {
        this.Record.EyesColor = value;
      }
    }

    public int PlaceInRating { get; set; }

    public Asda2PetRecord Asda2Pet { get; set; }

    /// <summary>0 - Light; 1- Dark; 2 - Chaos; -1 - None</summary>
    public short Asda2FactionId
    {
      get
      {
        return this.Record.Asda2FactionId;
      }
      set
      {
        this.Record.Asda2FactionId = value;
        if (this.Map == null)
          return;
        foreach (WorldObject nearbyObject in (IEnumerable<WorldObject>) this.NearbyObjects)
        {
          if (nearbyObject is Character)
          {
            Character visibleChr = nearbyObject as Character;
            this.CheckAtackStateWithCharacter(visibleChr);
            visibleChr.CheckAtackStateWithCharacter(this);
          }
        }
        GlobalHandler.SendCharacterFactionToNearbyCharacters(this);
      }
    }

    /// <summary>0 - 20</summary>
    public short Asda2FactionRank
    {
      get
      {
        return this._asda2FactionRank;
      }
      set
      {
        this._asda2FactionRank = value;
      }
    }

    public int Asda2HonorPoints
    {
      get
      {
        return this.Record.Asda2HonorPoints;
      }
      set
      {
        if (this.Record.Asda2HonorPoints == value)
          return;
        this.Record.Asda2HonorPoints = value;
        Asda2CharacterHandler.SendFactionAndHonorPointsInitResponse(this.Client);
        this.RecalculateFactionRank(false);
      }
    }

    private void RecalculateFactionRank(bool silent = false)
    {
      int factionRank = CharacterFormulas.GetFactionRank(this.Asda2HonorPoints);
      if ((int) this.Asda2FactionRank != factionRank)
      {
        this.Asda2FactionRank = (short) factionRank;
        switch (factionRank)
        {
          case 1:
            this.Map.CallDelayed(5000, (Action) (() => this.GetTitle(Asda2TitleId.Private132)));
            break;
          case 4:
            this.Map.CallDelayed(5000, (Action) (() => this.GetTitle(Asda2TitleId.Sergeant133)));
            break;
          case 7:
            this.Map.CallDelayed(5000, (Action) (() => this.GetTitle(Asda2TitleId.Officer134)));
            break;
          case 10:
            this.Map.CallDelayed(5000, (Action) (() => this.GetTitle(Asda2TitleId.Captain135)));
            break;
          case 13:
            this.Map.CallDelayed(5000, (Action) (() => this.GetTitle(Asda2TitleId.Major136)));
            break;
          case 16:
            this.Map.CallDelayed(5000, (Action) (() => this.GetTitle(Asda2TitleId.Colonel137)));
            break;
          case 18:
            this.Map.CallDelayed(5000, (Action) (() => this.GetTitle(Asda2TitleId.General138)));
            break;
          case 20:
            this.Map.CallDelayed(5000, (Action) (() => this.GetTitle(Asda2TitleId.God139)));
            break;
        }
      }
      if (silent)
        return;
      GlobalHandler.SendCharacterFactionToNearbyCharacters(this);
      Asda2CharacterHandler.SendFactionAndHonorPointsInitResponse(this.Client);
    }

    public string SoulmateIntroduction
    {
      get
      {
        return this.Account.AccountData.SoulmateIntroduction;
      }
      set
      {
        this.Account.AccountData.SoulmateIntroduction = value;
      }
    }

    public Asda2SoulmateRelationRecord SoulmateRecord { get; set; }

    public bool IsSoulmated
    {
      get
      {
        return this.SoulmateRecord != null;
      }
    }

    public CharacterRecord[] SoulmatedCharactersRecords { get; set; }

    public Character SoulmateCharacter
    {
      get
      {
        if (this.SoulmateRealmAccount == null)
          return (Character) null;
        return this.SoulmateRealmAccount.ActiveCharacter;
      }
    }

    public RealmAccount SoulmateRealmAccount { get; set; }

    public int GuildPoints
    {
      get
      {
        return this.Record.GuildPoints;
      }
      set
      {
        this.Record.GuildPoints = value;
        Asda2GuildHandler.SendUpdateGuildPointsResponse(this.Client);
      }
    }

    public Color ChatColor
    {
      get
      {
        return this.GetChatColor();
      }
      set
      {
        this.Record.GlobalChatColor = value;
      }
    }

    private Color GetChatColor()
    {
      switch (this.Role.Status)
      {
        case RoleStatus.EventManager:
          return Color.CadetBlue;
        case RoleStatus.Admin:
          return Character.GmChatColor;
        default:
          return Color.Yellow;
      }
    }

    public int FishingLevel
    {
      get
      {
        return this.Record.FishingLevel + this.GetIntMod(StatModifierInt.Asda2FishingSkill);
      }
      set
      {
        this.Record.FishingLevel = value;
        Asda2FishingHandler.SendFishingLvlResponse(this.Client);
      }
    }

    public uint AccId
    {
      get
      {
        return (uint) this.Account.AccountId;
      }
    }

    public int AvatarMask
    {
      get
      {
        return this.Record.AvatarMask;
      }
      set
      {
        this.Record.AvatarMask = value;
        Asda2CharacterHandler.SendUpdateAvatarMaskResponse(this);
      }
    }

    public byte[] SettingsFlags
    {
      get
      {
        return this.Record.SettingsFlags;
      }
      set
      {
        this.Record.SettingsFlags = value;
        this.UpdateSettings();
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
      get
      {
        return this._isAsda2TradeDescriptionEnabled;
      }
      set
      {
        if (this._isAsda2TradeDescriptionEnabled == value)
          return;
        this._isAsda2TradeDescriptionEnabled = value;
        this.Map.CallDelayed(777, (Action) (() => Asda2PrivateShopHandler.SendtradeStatusTextWindowResponse(this)));
      }
    }

    public bool IsAsda2TradeDescriptionPremium { get; set; }

    public string Asda2TradeDescription
    {
      get
      {
        return this._asda2TradeDescription ?? (this._asda2TradeDescription = "");
      }
      set
      {
        this._asda2TradeDescription = value;
        if (!this.IsAsda2TradeDescriptionEnabled)
          return;
        Asda2PrivateShopHandler.SendtradeStatusTextWindowResponse(this);
      }
    }

    public Asda2PrivateShop PrivateShop { get; set; }

    public Asda2Pvp Asda2Duel { get; set; }

    public bool IsAsda2Dueling
    {
      get
      {
        return this.Asda2Duel != null;
      }
    }

    public Character Asda2DuelingOponent { get; set; }

    public byte GreenCharges { get; set; }

    public byte BlueCharges { get; set; }

    public byte RedCharges { get; set; }

    public byte Asda2GuildRank
    {
      get
      {
        return (byte) (4U - this.GuildRank);
      }
      set
      {
        this.GuildRank = 4U - (uint) value;
      }
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
        if (this.CurrentBattleGround != null && this.CurrentBattleGround.IsRunning)
          return this.MapId == MapId.BatleField;
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
      get
      {
        return this._asda2WingsItemId;
      }
      set
      {
        this._asda2WingsItemId = value;
        if (this.Map == null)
          return;
        FunctionalItemsHandler.SendWingsInfoResponse(this, (IRealmClient) null);
      }
    }

    public short TransformationId
    {
      get
      {
        return this._transformationId;
      }
      set
      {
        this._transformationId = this.VerifyTransformationId(value);
        GlobalHandler.SendTransformToPetResponse(this, this._transformationId != (short) -1, (IRealmClient) null);
      }
    }

    private short VerifyTransformationId(short value)
    {
      if (value == (short) 0 || value == (short) 190 || (value == (short) 192 || value == (short) 197) || (value == (short) 373 || value == (short) 551 || (value == (short) 551 || value > (short) 843)))
        return -1;
      return value;
    }

    public bool IsOnMount
    {
      get
      {
        return this.MountId != -1;
      }
    }

    public bool ExpBlock { get; set; }

    public bool IsFirstGameConnection { get; set; }

    public Vector3 TargetSummonPosition { get; set; }

    public MapId TargetSummonMap { get; set; }

    public bool IsReborning { get; set; }

    public bool ChatBanned
    {
      get
      {
        return this.Record.ChatBanned;
      }
      set
      {
        this.Record.ChatBanned = value;
      }
    }

    public DateTime? BanChatTill
    {
      get
      {
        return this.Record.BanChatTill;
      }
      set
      {
        this.Record.BanChatTill = value;
      }
    }

    public void SetGlyphSlot(byte slot, uint id)
    {
      this.SetUInt32((UpdateFieldId) ((PlayerFields) (1312 + (int) slot)), id);
    }

    public uint GetGlyphSlot(byte slot)
    {
      return this.GetUInt32((PlayerFields) (1312 + (int) slot));
    }

    public void SetGlyph(byte slot, uint glyph)
    {
      this.SetUInt32((UpdateFieldId) ((PlayerFields) (1318 + (int) slot)), glyph);
    }

    public uint GetGlyph(byte slot)
    {
      return this.GetUInt32((PlayerFields) (1318 + (int) slot));
    }

    private void SaveCharacter()
    {
      if (this.IsDisposed || this.Map == null || this.Client == null)
      {
        this._saveTaskRunning = false;
      }
      else
      {
        this.SaveNow();
        ServerApp<WCell.RealmServer.RealmServer>.IOQueue.CallDelayed(CharacterFormulas.SaveChateterInterval, new Action(this.SaveCharacter));
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
      TimeSpan timeSpan = now - this.m_lastPlayTimeUpdate;
      this.LevelPlayTime += (uint) timeSpan.TotalSeconds;
      this.TotalPlayTime += (uint) timeSpan.TotalSeconds;
      this.m_lastPlayTimeUpdate = now;
    }

    /// <summary>Check to see if character is in an instance</summary>
    public bool IsInInstance
    {
      get
      {
        if (this.m_Map != null)
          return this.m_Map.IsInstance;
        return false;
      }
    }

    /// <summary>Check to see if character is in a group</summary>
    public bool IsInGroup
    {
      get
      {
        return this.m_groupMember != null;
      }
    }

    /// <summary>Check to see if character is in a Guild</summary>
    public bool IsInGuild
    {
      get
      {
        return this.m_guildMember != null;
      }
    }

    /// <summary>Check to see if character is in a group</summary>
    public bool IsInRaid
    {
      get
      {
        return this.Group is RaidGroup;
      }
    }

    /// <summary>
    /// Check to see if character is in the same instance as group members
    /// </summary>
    public bool IsInGroupInstance
    {
      get
      {
        Group group = this.Group;
        if (group != null)
          return group.GetActiveInstance(this.m_Map.MapTemplate) != null;
        return false;
      }
    }

    /// <summary>
    /// Personal Dungeon Difficulty, might differ from current Difficulty
    /// </summary>
    public DungeonDifficulty DungeonDifficulty
    {
      get
      {
        return this.m_record.DungeonDifficulty;
      }
      set
      {
        this.m_record.DungeonDifficulty = value;
        if (this.m_groupMember != null)
          return;
        InstanceHandler.SendDungeonDifficulty(this);
      }
    }

    public RaidDifficulty RaidDifficulty
    {
      get
      {
        return this.m_record.RaidDifficulty;
      }
      set
      {
        this.m_record.RaidDifficulty = value;
        if (this.m_groupMember != null)
          return;
        InstanceHandler.SendRaidDifficulty(this);
      }
    }

    public bool IsAllowedLowLevelRaid
    {
      get
      {
        return this.PlayerFlags.HasFlag((Enum) PlayerFlags.AllowLowLevelRaid);
      }
      set
      {
        if (value)
          this.PlayerFlags |= PlayerFlags.AllowLowLevelRaid;
        else
          this.PlayerFlags &= ~PlayerFlags.AllowLowLevelRaid;
      }
    }

    public uint GetInstanceDifficulty(bool isRaid)
    {
      if (this.m_groupMember != null)
        return this.m_groupMember.Group.DungeonDifficulty;
      if (!isRaid)
        return (uint) this.m_record.DungeonDifficulty;
      return (uint) this.m_record.RaidDifficulty;
    }

    public override bool IsAlive
    {
      get
      {
        return this.Health != 0;
      }
    }

    /// <summary>
    /// whether the Corpse is reclaimable
    /// (Character must be ghost and the reclaim delay must have passed)
    /// </summary>
    public bool IsCorpseReclaimable
    {
      get
      {
        if (this.IsGhost)
          return DateTime.Now > this.m_record.LastResTime.AddMilliseconds((double) Corpse.MinReclaimDelay);
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
        if (!this.IsCorpseReclaimable)
          return false;
        if (this.m_corpse != null && this.IsInRadiusSq((IHasPosition) this.m_corpse, Corpse.ReclaimRadiusSq))
          return true;
        if (this.IsGhost && this.m_corpse == null)
          return this.KnownObjects.Contains<WorldObject>((Func<WorldObject, bool>) (obj =>
          {
            if (obj is Unit)
              return ((Unit) obj).IsSpiritHealer;
            return false;
          }));
        return false;
      }
    }

    /// <summary>Last time this Character died</summary>
    public DateTime LastDeathTime
    {
      get
      {
        return this.m_record.LastDeathTime;
      }
      set
      {
        this.m_record.LastDeathTime = value;
      }
    }

    /// <summary>Last time this Character was resurrected</summary>
    public DateTime LastResTime
    {
      get
      {
        return this.m_record.LastResTime;
      }
      set
      {
        this.m_record.LastResTime = value;
      }
    }

    protected override bool OnBeforeDeath()
    {
      if (this.Health == 0)
        this.Health = 1;
      if (!this.m_Map.MapTemplate.NotifyPlayerBeforeDeath(this))
        return false;
      if (!this.IsDueling)
        return true;
      this.Duel.OnDeath(this);
      return false;
    }

    protected override void OnDeath()
    {
      this.PereodicActions.Clear();
      this.m_record.LastDeathTime = DateTime.Now;
      if (this.IsSoulmateSoulSaved && this.SoulmateCharacter != null && (double) this.GetDistance((WorldObject) this.SoulmateCharacter) < 40.0)
      {
        this.IsSoulmateSoulSaved = false;
        this.SendInfoMsg("You don't lose exp cause you soul was saved by your soulmate.");
      }
      else
      {
        if (this.Asda2FactionId == (short) 2 || !this.RemoveDeathPenalties && !this.IsAsda2BattlegroundInProgress)
        {
          this.Asda2Inventory.OnDeath();
          this.LastExpLooseAmount = this.Experience / CharacterFormulas.ExpirienceLooseOnDeathPrc;
          if (this.Level < 20)
            this.LastExpLooseAmount /= 20 - this.Level;
          this.LastExpLooseAmount *= 1 - this.IntMods[42] / 100;
          if (this.LastExpLooseAmount < 0)
            this.LastExpLooseAmount = 0;
          NPC lastKiller1 = this.LastKiller as NPC;
          Character lastKiller2 = this.LastKiller as Character;
          Log.Create(Log.Types.ExpChanged, LogSourceType.Character, this.EntryId).AddAttribute("difference_expirience", (double) this.LastExpLooseAmount, "").AddAttribute("total_expirience", (double) this.Experience, "").AddAttribute("source", 0.0, "death").AddAttribute("killer", lastKiller1 == null ? (lastKiller2 == null ? 0.0 : (double) lastKiller2.EntryId) : (double) lastKiller1.Entry.Id, lastKiller1 == null ? (lastKiller2 == null ? "unknown_killer!" : lastKiller2.Name) : lastKiller1.Name).Write();
          this.Experience -= this.LastExpLooseAmount;
          Character lastKiller3 = this.LastKiller as Character;
          if (lastKiller3 != null)
          {
            lastKiller3.GainXp((int) ((double) this.LastExpLooseAmount * (double) CharacterFormulas.KillPkExpPercentOfLoose), "killed_pk", false);
            int num = this.Level - lastKiller3.Level;
            if (num >= -5)
            {
              if (num > 10)
                num = 10;
              if (num < 2)
                num = 2;
              lastKiller3.GuildPoints += num * CharacterFormulas.CharacterKillingGuildPoints;
            }
          }
          this.SendInfoMsg(string.Format("You have loose {0} exp on death.", (object) this.LastExpLooseAmount));
        }
        List<Asda2Item> asda2ItemList = new List<Asda2Item>();
        if (this.Asda2FactionId == (short) 2)
        {
          asda2ItemList.AddRange((IEnumerable<Asda2Item>) ((IEnumerable<Asda2Item>) this.Asda2Inventory.Equipment).Where<Asda2Item>((Func<Asda2Item, bool>) (asda2Item =>
          {
            if (asda2Item != null && (asda2Item.IsWeapon || asda2Item.IsArmor))
              return Utility.Random(0, 100000) < CharacterFormulas.PKItemDropChance;
            return false;
          })).ToList<Asda2Item>());
          asda2ItemList.AddRange(((IEnumerable<Asda2Item>) this.Asda2Inventory.RegularItems).Where<Asda2Item>((Func<Asda2Item, bool>) (asda2Item =>
          {
            if (asda2Item != null && asda2Item.ItemId != 20551)
              return Utility.Random(0, 100000) < CharacterFormulas.PKItemDropChance;
            return false;
          })));
          asda2ItemList.AddRange(((IEnumerable<Asda2Item>) this.Asda2Inventory.ShopItems).Where<Asda2Item>((Func<Asda2Item, bool>) (asda2Item =>
          {
            if (asda2Item != null && asda2Item.ItemId != 20551 && (asda2Item.IsWeapon || asda2Item.IsArmor || asda2Item.Category == Asda2ItemCategory.ItemPackage))
              return Utility.Random(0, 100000) < CharacterFormulas.PKItemDropChance;
            return false;
          })));
        }
        else
        {
          if (this.IsAsda2BattlegroundInProgress)
            return;
          asda2ItemList.AddRange(((IEnumerable<Asda2Item>) this.Asda2Inventory.RegularItems).Where<Asda2Item>((Func<Asda2Item, bool>) (asda2Item =>
          {
            if (asda2Item != null && asda2Item.ItemId != 20551)
              return Utility.Random(0, 100000) < CharacterFormulas.ItemDropChance;
            return false;
          })));
          asda2ItemList.AddRange(((IEnumerable<Asda2Item>) this.Asda2Inventory.ShopItems).Where<Asda2Item>((Func<Asda2Item, bool>) (asda2Item =>
          {
            if (asda2Item != null && asda2Item.ItemId != 20551 && asda2Item.Category == Asda2ItemCategory.ItemPackage)
              return Utility.Random(0, 100000) < CharacterFormulas.ItemDropChance;
            return false;
          })));
        }
      }
    }

    protected internal override void OnResurrect()
    {
      this.LastExpLooseAmount = 0;
      base.OnResurrect();
      this.CorpseReleaseFlags &= ~CorpseReleaseFlags.ShowCorpseAutoReleaseTimer;
      if (this.m_corpse != null)
        this.Corpse = (Corpse) null;
      this.m_record.LastResTime = DateTime.Now;
      if (this.m_Map == null)
        return;
      this.m_Map.MapTemplate.NotifyPlayerResurrected(this);
    }

    /// <summary>
    /// Resurrects, applies ResurrectionSickness and damages Items, if applicable
    /// </summary>
    public void ResurrectWithConsequences()
    {
      this.Resurrect();
      int level = this.Level;
      int sicknessStartLevel = Character.ResurrectionSicknessStartLevel;
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
      if (action.Attacker == null)
        return;
      if (this.m_activePet != null)
        this.m_activePet.ThreatCollection.AddNewIfNotExisted(action.Attacker);
      if (this.m_minions != null)
      {
        foreach (NPC minion in (List<NPC>) this.m_minions)
          minion.ThreatCollection.AddNewIfNotExisted(action.Attacker);
      }
      bool isPvPing = action.Attacker.IsPvPing;
      Character characterMaster = action.Attacker.CharacterMaster;
      if (!isPvPing || !characterMaster.IsInBattleground)
        return;
      characterMaster.Battlegrounds.Stats.TotalDamage += action.ActualDamage;
    }

    protected override void OnKilled(IDamageAction action)
    {
      base.OnKilled(action);
      if (action.Attacker != null)
      {
        Character attacker = action.Attacker as Character;
        if (attacker != null && attacker.IsAsda2BattlegroundInProgress)
        {
          short points = CharacterFormulas.CalcBattlegrounActPointsOnKill(attacker.Level, this.Level, attacker.BattlegroundActPoints, this.BattlegroundActPoints);
          ++attacker.BattlegroundKills;
          attacker.GainActPoints(points);
          this.BattlegroundActPoints -= (short) ((int) points / 2);
          ++this.BattlegroundDeathes;
          attacker.CurrentBattleGround.GainScores(attacker, points);
          Asda2BattlegroundHandler.SendSomeOneKilledSomeOneResponse(attacker.CurrentBattleGround, (int) attacker.AccId, (int) points, attacker.Name, this.Name);
        }
      }
      this.m_Map.MapTemplate.NotifyPlayerDied(action);
    }

    public void GainActPoints(short points)
    {
      if (this.IsInGroup)
      {
        this.BattlegroundActPoints += (short) ((double) points * (1.0 - (double) CharacterFormulas.BattegroundGroupDisctributePrc));
        short num = (short) ((double) points * (double) CharacterFormulas.BattegroundGroupDisctributePrc / (double) this.Group.CharacterCount);
        foreach (GroupMember groupMember in this.Group)
          groupMember.Character.BattlegroundActPoints += num;
      }
      else
        this.BattlegroundActPoints += points;
    }

    /// <summary>
    /// Finds the item for the given slot. Unequips it if it may not currently be used.
    /// Returns the item to be equipped or null, if invalid.
    /// </summary>
    protected override IAsda2Weapon GetOrInvalidateItem(InventorySlotType type)
    {
      return (IAsda2Weapon) null;
    }

    protected override void OnHeal(HealAction action)
    {
      base.OnHeal(action);
      Unit attacker = action.Attacker;
      if (!(attacker is Character))
        return;
      Character character = (Character) attacker;
      if (!character.IsInBattleground)
        return;
      character.Battlegrounds.Stats.TotalHealing += action.Value;
    }

    /// <summary>
    /// Spawns the corpse and teleports the dead Character to the nearest SpiritHealer
    /// </summary>
    internal void ReleaseCorpse()
    {
      if (this.IsAlive || !this.IsAlive)
        return;
      this.BecomeGhost();
      this.Corpse = this.SpawnCorpse(false, false);
      this.m_record.CorpseX = new float?(this.m_corpse.Position.X);
      this.m_record.CorpseY = this.m_corpse.Position.Y;
      this.m_record.CorpseZ = this.m_corpse.Position.Z;
      this.m_record.CorpseO = this.m_corpse.Orientation;
      this.m_record.CorpseMap = this.m_Map.Id;
      this.m_corpseReleaseTimer.Stop();
    }

    /// <summary>
    /// Spawns and returns a new Corpse at the Character's current location
    /// </summary>
    /// <param name="bones"></param>
    /// <param name="lootable"></param>
    /// <returns></returns>
    public Corpse SpawnCorpse(bool bones, bool lootable)
    {
      return this.SpawnCorpse(bones, lootable, this.m_Map, this.m_position, this.m_orientation);
    }

    /// <summary>Spawns and returns a new Corpse at the given location</summary>
    /// <param name="bones"></param>
    /// <param name="lootable"></param>
    /// <returns></returns>
    public Corpse SpawnCorpse(bool bones, bool lootable, Map map, Vector3 pos, float o)
    {
      Corpse corpse = new Corpse(this, pos, o, this.DisplayId, this.Facial, this.Skin, this.HairStyle, this.HairColor, this.FacialHair, this.GuildId, this.Gender, this.Race, bones ? CorpseFlags.Bones : CorpseFlags.None, lootable ? CorpseDynamicFlags.PlayerLootable : CorpseDynamicFlags.None);
      corpse.Position = pos;
      map.AddObjectLater((WorldObject) corpse);
      return corpse;
    }

    /// <summary>
    /// Tries to teleport to the next SpiritHealer, if there is any.
    /// 
    /// TODO: Graveyards
    /// </summary>
    public void TeleportToNearestGraveyard()
    {
      this.TeleportToNearestGraveyard(true);
    }

    /// <summary>
    /// Tries to teleport to the next SpiritHealer, if there is any.
    /// 
    /// TODO: Graveyards
    /// </summary>
    public void TeleportToNearestGraveyard(bool allowSameMap)
    {
      if (allowSameMap)
      {
        NPC nearestSpiritHealer = this.m_Map.GetNearestSpiritHealer(ref this.m_position);
        if (nearestSpiritHealer != null)
        {
          this.TeleportTo((IWorldLocation) nearestSpiritHealer);
          return;
        }
      }
      if (this.m_Map.MapTemplate.RepopMap != null)
        this.TeleportTo(this.m_Map.MapTemplate.RepopMap, this.m_Map.MapTemplate.RepopPosition);
      else
        this.TeleportToBindLocation();
    }

    public LevelStatInfo ClassBaseStats
    {
      get
      {
        return this.m_archetype.GetLevelStats((uint) this.Level);
      }
    }

    internal void UpdateRest()
    {
      if (this.m_restTrigger == null)
        return;
      DateTime now = DateTime.Now;
      this.RestXp += RestGenerator.GetRestXp(now - this.m_lastRestUpdate, this);
      this.m_lastRestUpdate = now;
    }

    /// <summary>Gain experience from combat</summary>
    public void GainCombatXp(int experience, INamed killed, bool gainRest)
    {
      if (this.Level >= this.MaxLevel || this.ExpBlock || (this.IsDead || this.Level >= this.MaxLevel) || this.ExpBlock)
        return;
      int xp = experience;
      NPC npc = killed as NPC;
      if (this.m_activePet != null && this.m_activePet.MayGainExperience)
      {
        this.m_activePet.PetExperience += xp;
        this.m_activePet.TryLevelUp();
      }
      if (gainRest && this.RestXp > 0)
      {
        int num = Math.Min(this.RestXp, experience);
        xp += num;
        this.RestXp -= num;
      }
      int num1 = XpGenerator.GetXpForlevel(this.Level + 1) / 4;
      if (xp > num1)
        xp = num1;
      if (xp < 0)
      {
        LogUtil.WarnException("Exp {0} Char {1} kill {2} source {3}", (object) xp, (object) this.Name, (object) killed, (object) experience);
        xp = 1;
      }
      this.Experience += xp;
      if (npc != null)
        Asda2CharacterHandler.SendExpGainedResponse(npc.UniqIdOnMap, this, xp, true);
      Log.Create(Log.Types.ExpChanged, LogSourceType.Character, this.EntryId).AddAttribute("difference_expirience", (double) experience, "").AddAttribute("total_expirience", (double) this.Experience, "").AddAttribute("source", 0.0, "combat").AddAttribute("npc_entry_id", npc == null ? 0.0 : (double) npc.Entry.Id, "").Write();
      this.TryLevelUp();
    }

    /// <summary>Gain non-combat experience (through quests etc)</summary>
    /// <param name="experience"></param>
    /// <param name="useRest">If true, subtracts the given amount of experience from RestXp and adds it ontop of the given xp</param>
    public void GainXp(int experience, string source, bool useRest = false)
    {
      if (this.Level >= this.MaxLevel || this.ExpBlock || this.IsDead)
        return;
      if (this.SoulmateRecord != null)
        this.SoulmateRecord.OnExpGained(false);
      int xp = experience;
      if (useRest && this.RestXp > 0)
      {
        int num = Math.Min(this.RestXp, experience);
        xp += num;
        this.RestXp -= num;
      }
      int num1 = XpGenerator.GetXpForlevel(this.Level + 1) / 4;
      if (xp > num1)
        xp = num1;
      this.Experience += xp;
      Asda2CharacterHandler.SendExpGainedResponse((ushort) 0, this, xp, false);
      Log.Create(Log.Types.ExpChanged, LogSourceType.Character, this.EntryId).AddAttribute("difference_expirience", (double) experience, "").AddAttribute("total_expirience", (double) this.Experience, "").AddAttribute(nameof (source), 0.0, source).Write();
      this.TryLevelUp();
    }

    internal bool TryLevelUp()
    {
      int level = this.Level;
      int experience = this.Experience;
      int num = this.NextLevelXP;
      bool flag = false;
      while (experience >= num && level < this.MaxLevel)
      {
        ++level;
        experience -= num;
        num = XpGenerator.GetXpForlevel(level + 1);
        flag = true;
      }
      if (!flag)
        return false;
      this.Experience = experience;
      this.NextLevelXP = num;
      Log.Create("level_up", LogSourceType.Character, this.EntryId).AddAttribute("from", (double) this.Level, "").AddAttribute("to", (double) level, "").AddAttribute("total exp", (double) this.Experience, "").Write();
      this.Level = level;
      return true;
    }

    protected override void OnLevelChanged()
    {
      base.OnLevelChanged();
      if (this.Level >= 10 && this.Archetype.ClassId == ClassId.NoClass)
        this.SendInfoMsg("Используйте меню внешней программы для смены профессии");
      if (this.Archetype.ClassId != ClassId.NoClass)
      {
        if (this.Level >= 30 && this.RealProffLevel == (byte) 1)
          this.SetClass(2, (int) this.Archetype.ClassId);
        else if (this.Level >= 50 && this.RealProffLevel == (byte) 2)
          this.SetClass(3, (int) this.Archetype.ClassId);
        else if (this.Level >= 70 && this.RealProffLevel == (byte) 3)
          this.SetClass(4, (int) this.Archetype.ClassId);
      }
      this.GuildPoints += CharacterFormulas.LevelupingGuildPointsPerLevel * this.Level;
      IList<PerlevelItemBonusTemplateItem> bonusItems = PerLevelItemBonusManager.GetBonusItemList((byte) this.Level, (byte) this.Record.RebornCount, this.Record.PrivatePerLevelItemBonusTemplateId);
      ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
      {
        foreach (PerlevelItemBonusTemplateItem bonusTemplateItem in (IEnumerable<PerlevelItemBonusTemplateItem>) bonusItems)
          this.Asda2Inventory.AddDonateItem(Asda2ItemMgr.GetTemplate(bonusTemplateItem.ItemId), bonusTemplateItem.Amount, "~Leveling system~", true);
      }));
      this.InitGlyphsForLevel();
      int level = this.Level;
      this.Experience = 0;
      if (this.m_activePet != null)
      {
        if (!this.m_activePet.IsHunterPet || this.m_activePet.Level > level)
          this.m_activePet.Level = level;
        else if (level - PetMgr.MaxHunterPetLevelDifference > this.m_activePet.Level)
          this.m_activePet.Level = level - PetMgr.MaxHunterPetLevelDifference;
      }
      double num = CharacterFormulas.CalcStatBonusPerLevel(level, this.Record.RebornCount);
      this.FreeStatPoints += (int) num;
      Log.Create("gain_stats", LogSourceType.Character, this.EntryId).AddAttribute("source", 0.0, "on lvl up").AddAttribute("difference_stat_points", num, "").AddAttribute("total_points", (double) this.FreeStatPoints, "").AddAttribute("level", (double) level, "").Write();
      this.SendSystemMessage(string.Format("You have {0} free stat points. Enter \"#HowToAddStats\" to see help.", (object) this.FreeStatPoints));
      this.ModStatsForLevel(level);
      this.m_auras.ReapplyAllAuras();
      this.m_achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.ReachLevel, (uint) this.Level, 0U, (Unit) null);
      this.UpdateDodgeChance();
      this.UpdateBlockChance();
      this.UpdateCritChance();
      this.UpdateAllAttackPower();
      Action<Character> levelChanged = Character.LevelChanged;
      if (levelChanged != null)
        levelChanged(this);
      this.Map.CallDelayed(100, (Action) (() => Asda2CharacterHandler.SendLvlUpResponse(this)));
      this.Map.CallDelayed(500, (Action) (() => Asda2CharacterHandler.SendUpdateStatsResponse(this.Client)));
      this.SaveLater();
    }

    public int FreeStatPoints
    {
      get
      {
        return this.Record.FreeStatPoints;
      }
      set
      {
        this.Record.FreeStatPoints = value;
      }
    }

    public void ModStatsForLevel(int level)
    {
      this.BasePower = CharacterFormulas.GetBaseMana(this.Level, this.Class);
      this.BaseHealth = CharacterFormulas.GetBaseHealth(this.Level, this.Class);
      this.SetInt32((UpdateFieldId) UnitFields.HEALTH, this.MaxHealth);
      this.UpdateAsda2Agility();
      this.UpdateAsda2Stamina();
      this.UpdateAsda2Luck();
      this.UpdateAsda2Spirit();
      this.UpdateAsda2Intellect();
      this.UpdateAsda2Strength();
      this.UpdatePlayedTime();
      if (this.TotalPlayTime / 60U / 60U > 25U)
        this.Map.CallDelayed(500, (Action) (() => this.GetTitle(Asda2TitleId.Player36)));
      if (this.TotalPlayTime / 60U / 60U > 150U)
        this.Map.CallDelayed(500, (Action) (() => this.GetTitle(Asda2TitleId.Obsessed37)));
      if (this.TotalPlayTime / 60U / 60U > 1000U)
        this.Map.CallDelayed(500, (Action) (() => this.GetTitle(Asda2TitleId.Veteran38)));
      this.LevelPlayTime = 0U;
    }

    /// <summary>Adds the given language</summary>
    public void AddLanguage(ChatLanguage lang)
    {
      this.AddLanguage(LanguageHandler.GetLanguageDescByType(lang));
    }

    public void AddLanguage(LanguageDescription desc)
    {
      if (!this.Spells.Contains((uint) desc.SpellId))
        this.Spells.AddSpell(desc.SpellId);
      if (this.m_skills.Contains(desc.SkillId))
        return;
      this.m_skills.Add(desc.SkillId, 300U, 300U, true);
    }

    /// <summary>
    /// Returns whether the given language can be understood by this Character
    /// </summary>
    public bool CanSpeak(ChatLanguage language)
    {
      return this.KnownLanguages.Contains(language);
    }

    public void SendAuctionMsg(string msg)
    {
      this.SendMessage("~Auction~", msg, Color.DeepPink);
    }

    public void SendCraftingMsg(string msg)
    {
      this.SendMessage("~Craft~", msg, Color.BurlyWood);
    }

    public void SendInfoMsg(string msg)
    {
      this.SendMessage("~Info~", msg, Color.Coral);
    }

    public void SendErrorMsg(string msg)
    {
      this.SendMessage("~Error~", msg, Color.Red);
      Unit.log.Warn(string.Format("error msg to character : {0}.\r\n{1}", (object) this.Name, (object) msg));
    }

    public void SendWarnMsg(string msg)
    {
      this.SendMessage("~Attention~", msg, Color.Red);
    }

    public void SendWarMsg(string msg)
    {
      this.SendMessage("~War~", msg, Color.OrangeRed);
    }

    public void SendMailMsg(string msg)
    {
      this.SendMessage("~Mail~", msg, Color.Gainsboro);
    }

    public void SendMessage(string sender, string msg, Color c)
    {
      ChatMgr.SendMessage((IPacketReceiver) this, sender, msg, c);
    }

    public void SendMessage(string message)
    {
      ChatMgr.SendSystemMessage((IPacketReceiver) this, message);
      ChatMgr.ChatNotify((IChatter) null, message, ChatLanguage.Universal, ChatMsgType.System, (IGenericChatTarget) this);
    }

    public void SendMessage(IChatter sender, string message)
    {
      ChatMgr.SendWhisper(sender, (IChatter) this, message);
    }

    /// <summary>Sends a message to the client.</summary>
    public void SendSystemMessage(RealmLangKey key, params object[] args)
    {
      ChatMgr.SendSystemMessage((IPacketReceiver) this, RealmLocalizer.Instance.Translate(this.Locale, key, args));
    }

    /// <summary>Sends a message to the client.</summary>
    public void SendSystemMessage(string msg)
    {
      ChatMgr.SendSystemMessage((IPacketReceiver) this, msg);
    }

    /// <summary>Sends a message to the client.</summary>
    public void SendSystemMessage(string msgFormat, params object[] args)
    {
      ChatMgr.SendSystemMessage((IPacketReceiver) this, string.Format(msgFormat, args));
    }

    public void Notify(RealmLangKey key, params object[] args)
    {
      this.Notify(RealmLocalizer.Instance.Translate(this.Locale, key, args));
    }

    /// <summary>Flashes a notification in the middle of the screen</summary>
    public void Notify(string msg, params object[] args)
    {
      MiscHandler.SendNotification((IPacketReceiver) this, string.Format(msg, args));
    }

    public void SayGroup(string msg)
    {
      this.SayGroup(this.SpokenLanguage, msg);
    }

    public void SayGroup(string msg, params object[] args)
    {
      this.SayGroup(string.Format(msg, args));
    }

    public override void Say(float radius, string msg)
    {
      this.SayYellEmote(ChatMsgType.Say, this.SpokenLanguage, msg, radius);
    }

    public override void Yell(float radius, string msg)
    {
      this.SayYellEmote(ChatMsgType.Yell, this.SpokenLanguage, msg, radius);
    }

    public override void Emote(float radius, string msg)
    {
      this.SayYellEmote(ChatMsgType.Emote, this.SpokenLanguage, msg, radius);
    }

    /// <summary>
    /// Called whenever this Character interacts with any WorldObject
    /// </summary>
    /// <param name="obj"></param>
    public void OnInteract(WorldObject obj)
    {
      this.StandState = StandState.Stand;
      if (!(obj is NPC))
        return;
      NPC npc = (NPC) obj;
      this.Reputations.OnTalkWith(npc);
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
      this.OnInteract((WorldObject) innKeeper);
      if (!(innKeeper.BindPoint != NamedWorldZoneLocation.Zero) || !innKeeper.CheckVendorInteraction(this))
        return false;
      this.BindTo(innKeeper);
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
      this.m_bindLocation = location;
    }

    /// <summary>Gets the quest log.</summary>
    /// <value>The quest log.</value>
    public QuestLog QuestLog
    {
      get
      {
        return this.m_questLog;
      }
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
        if (this.Duel != null)
          return this.Duel.IsActive;
        return false;
      }
    }

    public override bool IsFriendlyWith(IFactionMember opponent)
    {
      if (this.IsAlliedWith(opponent))
        return true;
      Faction faction = opponent.Faction;
      Reputation reputation = this.m_reputations[faction.ReputationIndex];
      if (reputation != null)
        return reputation.Standing >= Standing.Friendly;
      return this.m_faction.IsFriendlyTowards(faction);
    }

    public override bool IsAtLeastNeutralWith(IFactionMember opponent)
    {
      if (this.IsFriendlyWith(opponent))
        return true;
      Faction faction = opponent.Faction;
      Reputation reputation = this.m_reputations[faction.ReputationIndex];
      if (reputation != null)
        return reputation.Standing >= Standing.Neutral;
      return this.m_faction.Neutrals.Contains(faction);
    }

    public override bool IsHostileWith(IFactionMember opponent)
    {
      if (object.ReferenceEquals((object) opponent, (object) this) || opponent is Unit && ((WorldObject) opponent).Master == this)
        return false;
      if (opponent is NPC)
        return true;
      if (opponent is Character)
        return this.CanPvP((Character) opponent);
      Faction faction = opponent.Faction;
      if (opponent is NPC && faction.Neutrals.Contains(this.m_faction))
        return ((NPC) opponent).ThreatCollection.HasAggressor((Unit) this);
      if (this.m_faction.Friends.Contains(faction) || !this.m_faction.Enemies.Contains(faction))
        return false;
      return this.m_reputations.CanAttack(faction);
    }

    public override bool MayAttack(IFactionMember opponent)
    {
      if (object.ReferenceEquals((object) opponent, (object) this) || opponent is Unit && ((WorldObject) opponent).Master == this)
        return false;
      if (opponent is Character)
        return this.CanPvP((Character) opponent);
      return opponent is NPC;
    }

    public bool CanPvP(Character chr)
    {
      if (!chr.IsAlive || chr.Map == null)
        return false;
      if (chr.Map.MapTemplate.IsAsda2FightingMap && chr.Asda2FactionId != (short) -1 && (int) chr.Asda2FactionId != (int) this.Asda2FactionId)
        return true;
      return this.CanDuel(chr);
    }

    private bool CanDuel(Character chr)
    {
      return this.IsAsda2Dueling && chr == this.Asda2DuelingOponent;
    }

    /// <summary>
    /// One can only cast beneficial spells on people that we are allied with
    /// </summary>
    /// <param name="opponent"></param>
    /// <returns></returns>
    public override bool IsAlliedWith(IFactionMember opponent)
    {
      if (object.ReferenceEquals((object) opponent, (object) this) || opponent is Unit && ((WorldObject) opponent).Master == this)
        return true;
      if (!(opponent is Character) && opponent is WorldObject)
        opponent = (IFactionMember) ((WorldObject) opponent).Master;
      if (opponent is Character)
      {
        if (this.IsInBattleground)
          return this.Battlegrounds.Team == ((Character) opponent).Battlegrounds.Team;
        Group group = this.Group;
        if (group != null && ((Character) opponent).Group == group && this.DuelOpponent == null)
          return ((Character) opponent).DuelOpponent == null;
      }
      return false;
    }

    public override bool IsInSameDivision(IFactionMember opponent)
    {
      if (object.ReferenceEquals((object) opponent, (object) this) || opponent is Unit && ((WorldObject) opponent).Master == this)
        return true;
      if (!(opponent is Character) && opponent is WorldObject)
        opponent = (IFactionMember) ((WorldObject) opponent).Master;
      if (opponent is Character)
      {
        if (this.IsInBattleground)
          return this.Battlegrounds.Team == ((Character) opponent).Battlegrounds.Team;
        SubGroup subGroup = this.SubGroup;
        if (subGroup != null && ((Character) opponent).SubGroup == subGroup && this.DuelOpponent == null)
          return ((Character) opponent).DuelOpponent == null;
      }
      return false;
    }

    public override void OnAttack(DamageAction action)
    {
      if (action.Victim is NPC && this.m_dmgBonusVsCreatureTypePct != null)
      {
        int num = this.m_dmgBonusVsCreatureTypePct[(int) ((NPC) action.Victim).Entry.Type];
        if (num != 0)
          action.Damage += (num * action.Damage + 50) / 100;
      }
      base.OnAttack(action);
    }

    protected override void OnEnterCombat()
    {
      this.CancelTransports();
      if (this.CurrentCapturingPoint == null)
        return;
      this.CurrentCapturingPoint.StopCapture();
    }

    protected override bool CheckCombatState()
    {
      if (this.m_isFighting)
        return base.CheckCombatState();
      if (this.NPCAttackerCount == 0 && (this.m_activePet == null || this.m_activePet.NPCAttackerCount == 0) && !this.m_auras.HasHarmfulAura())
      {
        if (this.m_minions != null)
        {
          foreach (Unit minion in (List<NPC>) this.m_minions)
          {
            if (minion.NPCAttackerCount > 0)
              return base.CheckCombatState();
          }
        }
        this.IsInCombat = false;
      }
      return false;
    }

    public override int AddHealingModsToAction(int healValue, SpellEffect effect, DamageSchool school)
    {
      healValue += (int) ((double) healValue * (double) this.HealingDoneModPct / 100.0);
      healValue += this.HealingDoneMod;
      if (effect != null)
        healValue = this.Auras.GetModifiedInt(SpellModifierType.SpellPower, effect.Spell, healValue);
      return healValue;
    }

    public override int GetGeneratedThreat(int dmg, DamageSchool school, SpellEffect effect)
    {
      int num = base.GetGeneratedThreat(dmg, school, effect);
      if (effect != null)
        num = this.Auras.GetModifiedInt(SpellModifierType.Threat, effect.Spell, num);
      return num;
    }

    public override float CalcCritDamage(float dmg, Unit victim, SpellEffect effect)
    {
      dmg = base.CalcCritDamage(dmg, victim, effect);
      if (effect != null)
        return this.Auras.GetModifiedFloat(SpellModifierType.CritDamage, effect.Spell, dmg);
      return dmg;
    }

    /// <summary>Change target and/or amount of combo points</summary>
    public override bool ModComboState(Unit target, int amount)
    {
      if (!base.ModComboState(target, amount))
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
      get
      {
        return this.m_looterEntry ?? (this.m_looterEntry = new Asda2LooterEntry(this));
      }
    }

    /// <summary>whether this Character is currently looting something</summary>
    public bool IsLooting
    {
      get
      {
        if (this.m_looterEntry != null)
          return this.m_looterEntry.Loot != null;
        return false;
      }
    }

    /// <summary>
    /// Cancels looting (if this Character is currently looting something)
    /// </summary>
    public void CancelLooting()
    {
      if (this.m_looterEntry == null)
        return;
      this.m_looterEntry.Release();
    }

    public SummonRequest SummonRequest
    {
      get
      {
        return this.m_summonRequest;
      }
    }

    /// <summary>
    /// May be executed from outside of this Character's map's context
    /// </summary>
    public void StartSummon(ISummoner summoner)
    {
      this.StartSummon(summoner, SummonRequest.DefaultTimeout);
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
      if (this.m_summonRequest == null)
        return;
      if (this.m_summonRequest.Portal != null && this.m_summonRequest.Portal.IsInWorld)
        this.m_summonRequest.Portal.Delete();
      int num = notify ? 1 : 0;
      this.m_summonRequest = (SummonRequest) null;
    }

    public override int GetBasePowerRegen()
    {
      return RegenerationFormulas.GetPowerRegen((Unit) this);
    }

    public void ActivateAllTaxiNodes()
    {
      for (int index = 0; index < TaxiMgr.PathNodesById.Length; ++index)
      {
        PathNode node = TaxiMgr.PathNodesById[index];
        if (node != null)
        {
          this.TaxiNodes.Activate(node);
          this.SendSystemMessage("Activated Node: " + (object) node);
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
      if (this.m_target != null)
        this.ClearTarget();
      if (this.TradeWindow != null)
        this.TradeWindow.Cancel(TradeStatus.Cancelled);
      if (this.Asda2TradeWindow != null)
        this.Asda2TradeWindow.CancelTrade();
      if (this.CurrentCapturingPoint != null)
        this.CurrentCapturingPoint.StopCapture();
      this.CancelTransports();
    }

    public void CancelTransports()
    {
      this.MountId = -1;
      this.TransportItemId = -1;
    }

    public void ClearTarget()
    {
      this.Target = (Unit) null;
    }

    public override int GetPowerCost(DamageSchool school, Spell spell, int cost)
    {
      cost = base.GetPowerCost(school, spell, cost);
      cost = this.Auras.GetModifiedInt(SpellModifierType.PowerCost, spell, cost);
      return cost;
    }

    public SpecProfile CurrentSpecProfile
    {
      get
      {
        return this.SpecProfiles[this.m_talents.CurrentSpecIndex];
      }
    }

    /// <summary>Talent specs</summary>
    public SpecProfile[] SpecProfiles { get; protected internal set; }

    public void ApplyTalentSpec(int no)
    {
      this.SpecProfiles.Get<SpecProfile>(no);
    }

    public void InitGlyphsForLevel()
    {
      foreach (KeyValuePair<uint, GlyphSlotEntry> glyphSlot in GlyphInfoHolder.GlyphSlots)
      {
        if (glyphSlot.Value.Order != 0U)
          this.SetGlyphSlot((byte) (glyphSlot.Value.Order - 1U), glyphSlot.Value.Id);
      }
      int level = this.Level;
      uint num = 0;
      if (level >= 15)
        num |= 3U;
      if (level >= 30)
        num |= 8U;
      if (level >= 50)
        num |= 4U;
      if (level >= 70)
        num |= 16U;
      if (level >= 80)
        num |= 32U;
      this.Glyphs_Enable = num;
    }

    public void ApplyGlyph(byte slot, GlyphPropertiesEntry gp)
    {
      this.RemoveGlyph(slot);
      this.SpellCast.Trigger(SpellHandler.Get(gp.SpellId), new WorldObject[1]
      {
        (WorldObject) this
      });
      this.SetGlyph(slot, gp.Id);
      this.CurrentSpecProfile.GlyphIds[(int) slot] = gp.Id;
      TalentHandler.SendTalentGroupList(this.m_talents);
    }

    public void RemoveGlyph(byte slot)
    {
      uint glyph = this.GetGlyph(slot);
      if (glyph == 0U)
        return;
      this.Auras.Remove(SpellHandler.Get(GlyphInfoHolder.GetPropertiesEntryForGlyph(glyph).SpellId));
      this.CurrentSpecProfile.GlyphIds[(int) slot] = 0U;
      this.SetGlyph(slot, 0U);
    }

    public Character InstanceLeader
    {
      get
      {
        return this;
      }
    }

    public InstanceCollection InstanceLeaderCollection
    {
      get
      {
        return this.Instances;
      }
    }

    public bool HasInstanceCollection
    {
      get
      {
        return this.m_InstanceCollection != null;
      }
    }

    /// <summary>Auto-created if not already existing</summary>
    public InstanceCollection Instances
    {
      get
      {
        if (this.m_InstanceCollection == null)
          this.m_InstanceCollection = new InstanceCollection(this);
        return this.m_InstanceCollection;
      }
      set
      {
        this.m_InstanceCollection = value;
      }
    }

    public void ForeachInstanceHolder(Action<InstanceCollection> callback)
    {
      callback(this.Instances);
    }

    public BaseInstance GetActiveInstance(MapTemplate mapTemplate)
    {
      Map map = this.m_Map;
      if (map != null && map.Id == map.Id)
        return map as BaseInstance;
      return this.m_InstanceCollection?.GetActiveInstance(mapTemplate);
    }

    /// <summary>
    /// Whether this Character is in a Battleground at the moment
    /// </summary>
    public bool IsInBattleground
    {
      get
      {
        if (this.m_bgInfo != null)
          return this.m_bgInfo.Team != null;
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
        if (this.m_bgInfo == null)
          this.m_bgInfo = new BattlegroundInfo(this);
        return this.m_bgInfo;
      }
    }

    /// <summary>
    /// Is called when the Character kills an Honorable target.
    /// </summary>
    /// <param name="victim">The Honorable character killed.</param>
    internal void OnHonorableKill(IDamageAction action)
    {
      Character victim = (Character) action.Victim;
      uint num = this.CalcHonorForKill(victim);
      if (num == 0U)
        return;
      if (this.IsInBattleground)
      {
        BattlegroundTeam team = this.m_bgInfo.Team;
        BattlegroundStats stats = victim.Battlegrounds.Stats;
        if (team == victim.Battlegrounds.Team || stats == null || stats.Deaths > BattlegroundMgr.MaxHonorableDeaths)
          return;
        ++this.m_bgInfo.Stats.HonorableKills;
        team.DistributeSharedHonor(this, victim, num);
      }
      else if (this.Group != null)
      {
        if (this.Faction.Group == victim.Faction.Group)
          return;
        this.Group.DistributeGroupHonor(this, victim, num);
      }
      else
      {
        this.GiveHonorPoints(num);
        ++this.KillsToday;
        ++this.LifetimeHonorableKills;
        HonorHandler.SendPVPCredit((IPacketReceiver) this, num * 10U, victim);
      }
      if (this.m_zone == null)
        return;
      this.m_zone.Template.OnHonorableKill(this, victim);
    }

    private uint CalcHonorForKill(Character victim)
    {
      if (victim == this || !victim.YieldsXpOrHonor)
        return 0;
      int level1 = this.Level;
      int level2 = victim.Level;
      int maxLvlDiff = BattlegroundMgr.MaxLvlDiff;
      int num1 = BattlegroundMgr.MaxHonor - 1;
      if (num1 < 0)
        num1 = 0;
      int num2 = level1 - level2 + maxLvlDiff;
      if (num2 < 0)
        return 0;
      return (uint) Math.Round((double) ((float) num1 / (2f * (float) maxLvlDiff)) * (double) num2 + 1.0);
    }

    public void GiveHonorPoints(uint points)
    {
      this.HonorPoints += points;
      this.HonorToday += points;
    }

    public uint MaxPersonalArenaRating
    {
      get
      {
        return 0;
      }
    }

    public void TogglePvPFlag()
    {
      this.SetPvPFlag(!this.PlayerFlags.HasFlag((Enum) PlayerFlags.PVP));
    }

    public void SetPvPFlag(bool state)
    {
      if (state)
      {
        this.UpdatePvPState(true, this.PvPEndTime != null && this.PvPEndTime.IsRunning);
        this.PlayerFlags |= PlayerFlags.PVP;
      }
      else
      {
        if (this.Zone == null || this.Zone.Template.IsHostileTo(this) || !this.PvPState.HasFlag((Enum) PvPState.PVP))
          return;
        this.SetPvPResetTimer(false);
      }
    }

    public void UpdatePvPState(bool state, bool overridden = false)
    {
      if (!state || overridden)
      {
        this.SetPvPState(state);
        this.ClearPvPResetTimer();
      }
      else if (this.PvPEndTime != null && this.PvPEndTime.IsRunning)
        this.SetPvPResetTimer(true);
      else
        this.SetPvPState(true);
    }

    private void SetPvPResetTimer(bool overridden = false)
    {
      if (this.PvPEndTime == null)
        this.PvPEndTime = new TimerEntry((Action<int>) (dt => this.OnPvPTimerEnded()));
      if (!this.PvPEndTime.IsRunning || overridden)
        this.PvPEndTime.Start(300000);
      this.IsPvPTimerActive = true;
    }

    private void ClearPvPResetTimer()
    {
      if (this.PvPEndTime != null)
        this.PvPEndTime.Stop();
      this.IsPvPTimerActive = false;
    }

    private void OnPvPTimerEnded()
    {
      this.PlayerFlags &= ~PlayerFlags.PVP;
      this.IsPvPTimerActive = false;
      this.SetPvPState(false);
    }

    private void SetPvPState(bool state)
    {
      if (this.ActivePet != null)
      {
        if (state)
        {
          this.PvPState = PvPState.PVP;
          this.ActivePet.PvPState = PvPState.PVP;
        }
        else
        {
          this.PvPState &= ~PvPState.PVP;
          this.ActivePet.PvPState &= ~PvPState.PVP;
        }
      }
      else if (state)
        this.PvPState = PvPState.PVP;
      else
        this.PvPState &= ~PvPState.PVP;
    }

    /// <summary>Calculates the price of a purchase in a berber shop.</summary>
    /// <param name="newstyle"></param>
    /// <param name="newcolor"></param>
    /// <param name="newfacial"></param>
    /// <returns>The total price.</returns>
    public uint CalcBarberShopCost(byte newStyle, byte newColor, byte newFacial)
    {
      int level = this.Level;
      byte hairStyle = this.HairStyle;
      byte hairColor = this.HairColor;
      byte facialHair = this.FacialHair;
      if ((int) hairStyle == (int) newStyle && (int) hairColor == (int) newColor && (int) facialHair == (int) newFacial)
        return 0;
      float barberShopCost = GameTables.BarberShopCosts[level - 1];
      if ((double) barberShopCost == 0.0)
        return uint.MaxValue;
      float num = 0.0f;
      if ((int) hairStyle != (int) newStyle)
        num += barberShopCost;
      else if ((int) hairColor != (int) newColor)
        num += barberShopCost * 0.5f;
      if ((int) facialHair != (int) newFacial)
        num += barberShopCost * 0.75f;
      return (uint) num;
    }

    public BaseCommand<RealmServerCmdArgs> SelectedCommand
    {
      get
      {
        return this.m_ExtraInfo?.m_selectedCommand;
      }
      set
      {
        ExtraInfo extraInfo = this.m_ExtraInfo;
        if (extraInfo == null)
          return;
        extraInfo.m_selectedCommand = value;
      }
    }

    public override LinkedList<WaypointEntry> Waypoints
    {
      get
      {
        return (LinkedList<WaypointEntry>) null;
      }
    }

    public override NPCSpawnPoint SpawnPoint
    {
      get
      {
        return (NPCSpawnPoint) null;
      }
    }

    /// <summary>
    /// The ticket that is currently being handled by this <see cref="T:WCell.RealmServer.Help.Tickets.ITicketHandler" />
    /// </summary>
    public Ticket HandlingTicket
    {
      get
      {
        return this.m_ExtraInfo?.m_handlingTicket;
      }
      set
      {
        ExtraInfo extraInfo = this.ExtraInfo;
        if (extraInfo == null)
          return;
        extraInfo.m_handlingTicket = value;
      }
    }

    public bool MayHandle(Ticket ticket)
    {
      ITicketHandler handler = ticket.m_handler;
      if (handler != null)
        return handler.Role <= this.Role;
      return true;
    }

    public int CharacterCount
    {
      get
      {
        return 1;
      }
    }

    public bool AutoLoot { get; set; }

    public byte FriendShipPoints
    {
      get
      {
        return this.SoulmateRecord.FriendShipPoints;
      }
    }

    public byte MountBoxSize
    {
      get
      {
        return (byte) (6 * (1 + (int) this.Record.MountBoxExpands));
      }
    }

    public void ForeachCharacter(Action<Character> callback)
    {
      callback(this);
    }

    public Character[] GetAllCharacters()
    {
      return new Character[1]{ this };
    }

    public void Send(RealmPacketOut packet, bool addEnd = false)
    {
      this.m_client.Send(packet, addEnd);
    }

    public void Send(byte[] packet)
    {
      this.m_client.Send(packet);
    }

    public override string ToString()
    {
      return this.Name + " (ID: " + (object) this.EntityId + ", Account: " + (object) this.Account + ")";
    }

    public void SendNotifyMsg(string msg)
    {
      ChatMgr.SendSystemChatResponse(this.Client, msg);
    }

    public void YouAreFuckingCheater(string reason = "", int banPoints = 1)
    {
      this.Record.BanPoints += banPoints;
      Log.Create(Log.Types.Cheating, LogSourceType.Character, this.EntryId).AddAttribute(nameof (reason), 0.0, reason).AddAttribute("difference_ban_points", (double) banPoints, "").AddAttribute("total_ban_points", (double) this.Record.BanPoints, "").Write();
      Unit.log.Info(string.Format("{0} is trying to cheat! [{1}][{2}/{3} ban points]", (object) this.Name, (object) reason, (object) this.Record.BanPoints, (object) Character.PointsToGetBan));
      if (this.Record.BanPoints <= Character.PointsToGetBan)
        return;
      this.Account.SetAccountActive(false, new DateTime?(DateTime.MaxValue));
    }

    public void SendOnlyEnglishCharactersAllowed(string where)
    {
      this.SendInfoMsg(string.Format("Sorry, only english characters allowed in {0}.", (object) where));
    }

    /// <summary>Action-information of previously summoned pets</summary>
    public List<SummonedPetRecord> SummonedPetRecords
    {
      get
      {
        if (this.m_SummonedPetRecords == null)
          this.m_SummonedPetRecords = new List<SummonedPetRecord>();
        return this.m_SummonedPetRecords;
      }
    }

    /// <summary>
    /// All minions that belong to this Character, excluding actual Pets.
    /// Might be null.
    /// </summary>
    public NPCCollection Minions
    {
      get
      {
        return this.m_minions;
      }
    }

    /// <summary>All created Totems (might be null)</summary>
    public NPC[] Totems
    {
      get
      {
        return this.m_totems;
      }
    }

    /// <summary>
    /// Currently active Pet of this Character (the one with the action bar)
    /// </summary>
    public NPC ActivePet
    {
      get
      {
        return this.m_activePet;
      }
      set
      {
        if (value == this.m_activePet)
          return;
        if (this.m_activePet != null)
          this.m_activePet.Delete();
        if (this.IsPetActive = value != null)
        {
          value.PetRecord.IsActivePet = true;
          this.m_record.PetEntryId = value.Entry.NPCId;
          this.m_activePet = value;
          if (this.m_activePet.PetRecord.ActionButtons == null)
            this.m_activePet.PetRecord.ActionButtons = this.m_activePet.BuildPetActionBar();
          this.AddPostUpdateMessage((Action) (() =>
          {
            if (this.m_activePet != value || !this.m_activePet.IsInContext)
              return;
            PetHandler.SendSpells(this, this.m_activePet, PetAction.Follow);
            PetHandler.SendPetGUIDs(this);
            this.m_activePet.OnBecameActivePet();
          }));
        }
        else
        {
          this.Summon = EntityId.Zero;
          if (this.Charm == this.m_activePet)
            this.Charm = (Unit) null;
          this.m_record.PetEntryId = (NPCId) 0;
          PetHandler.SendEmptySpells((IPacketReceiver) this);
          PetHandler.SendPetGUIDs(this);
          this.m_activePet = (NPC) null;
        }
      }
    }

    /// <summary>
    /// Lets the ActivePet appear/disappear (if this Character has one)
    /// </summary>
    public bool IsPetActive
    {
      get
      {
        return this.m_record.IsPetActive;
      }
      set
      {
        if (value)
        {
          if (this.m_activePet != null && !this.m_activePet.IsInWorld)
            this.PlaceOnTop((WorldObject) this.ActivePet);
        }
        else
          this.m_activePet.RemoveFromMap();
        this.m_record.IsPetActive = value;
      }
    }

    /// <summary>Dismisses the current pet</summary>
    public void DismissActivePet()
    {
      if (this.m_activePet == null)
        return;
      if (this.m_activePet.IsSummoned)
        this.AbandonActivePet();
      else
        this.IsPetActive = false;
    }

    /// <summary>ActivePet is about to be abandoned</summary>
    public void AbandonActivePet()
    {
      if (this.m_activePet.IsInWorld && this.m_activePet.IsHunterPet && !this.m_activePet.PetRecord.IsStabled)
      {
        this.m_activePet.RejectMaster();
        this.m_activePet.IsDecaying = true;
      }
      else
        this.m_activePet.Delete();
    }

    /// <summary>
    /// Simply unsets the currently active pet without deleting or abandoning it.
    /// Make sure to take care of the pet when calling this method.
    /// </summary>
    private void UnsetActivePet()
    {
      NPC activePet = this.m_activePet;
    }

    public void Possess(int duration, Unit target, bool controllable = true, bool sendPetActionsWithSpells = true)
    {
      if (target == null)
        return;
      if (target is NPC)
      {
        this.Enslave((NPC) target, duration);
        target.Charmer = (Unit) this;
        this.Charm = target;
        target.Brain.State = BrainState.Idle;
        if (sendPetActionsWithSpells)
          PetHandler.SendSpells(this, (NPC) target, PetAction.Stay);
        else
          PetHandler.SendVehicleSpells((IPacketReceiver) this, (NPC) target);
        this.SetMover((WorldObject) target, controllable);
        target.UnitFlags |= UnitFlags.Possessed;
      }
      else if (target is Character)
      {
        PetHandler.SendPlayerPossessedPetSpells(this, (Character) target);
        this.SetMover((WorldObject) target, controllable);
      }
      this.Observing = target;
      this.FarSight = target.EntityId;
    }

    public void UnPossess(Unit target)
    {
      this.Observing = (Unit) null;
      this.SetMover((WorldObject) this, true);
      this.ResetMover();
      this.FarSight = EntityId.Zero;
      PetHandler.SendEmptySpells((IPacketReceiver) this);
      this.Charm = (Unit) null;
      if (target == null)
        return;
      target.Charmer = (Unit) null;
      target.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.SelectableNotAttackable | UnitFlags.Influenced | UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
      if (!(target is NPC))
        return;
      target.Brain.EnterDefaultState();
      ((NPC) target).RemainingDecayDelayMillis = 1;
    }

    /// <summary>Amount of stabled pets + active pet (if any)</summary>
    public int TotalPetCount
    {
      get
      {
        return (this.m_activePet != null ? 1 : 0) + (this.m_StabledPetRecords != null ? this.m_StabledPetRecords.Count : 0);
      }
    }

    public bool HasStabledPets
    {
      get
      {
        if (this.m_StabledPetRecords != null)
          return this.m_StabledPetRecords.Count > 0;
        return false;
      }
    }

    public List<PermanentPetRecord> StabledPetRecords
    {
      get
      {
        if (this.m_StabledPetRecords == null)
          this.m_StabledPetRecords = new List<PermanentPetRecord>(PetMgr.MaxStableSlots);
        return this.m_StabledPetRecords;
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
      if (!npc.IsTamable)
        return false;
      if (npc.IsExoticPet)
        return this.CanControlExoticPets;
      return true;
    }

    public NPC SpawnPet(IPetRecord record)
    {
      return this.SpawnPet(record, ref this.m_position, 0);
    }

    public NPC SpawnPet(IPetRecord record, int duration)
    {
      return this.SpawnPet(record, ref this.m_position, duration);
    }

    /// <summary>Tries to spawn a new summoned Pet for the Character.</summary>
    /// <param name="entry"></param>
    /// <param name="position"></param>
    /// <returns>null, if the Character already has that kind of Pet.</returns>
    public NPC SpawnPet(NPCEntry entry, ref Vector3 position, int durationMillis)
    {
      return this.SpawnPet((IPetRecord) this.GetOrCreateSummonedPetRecord(entry), ref position, durationMillis);
    }

    public NPC SpawnPet(IPetRecord record, ref Vector3 position, int duration)
    {
      NPC minion = this.CreateMinion(record.Entry, duration);
      minion.PetRecord = record;
      minion.Position = position;
      if (record.PetNumber != 0U)
        minion.EntityId = new EntityId(NPCMgr.GenerateUniqueLowId(), record.PetNumber, HighId.UnitPet);
      this.InitializeMinion(minion);
      if (this.IsPetActive)
        this.m_Map.AddObject((WorldObject) minion);
      return minion;
    }

    /// <summary>Makes the given NPC your Pet or Companion</summary>
    /// <param name="minion">NPC to control</param>
    /// <param name="duration">The amount of time, in miliseconds, to control the minion. 0 is infinite.</param>
    public void MakePet(NPC minion)
    {
      this.MakePet(minion, 0);
    }

    /// <summary>Makes the given NPC your Pet or Companion</summary>
    /// <param name="minion">NPC to control</param>
    /// <param name="duration">The amount of time, in miliseconds, to control the minion. 0 is infinite.</param>
    public void MakePet(NPC minion, int durationMillis)
    {
      this.Enslave(minion, durationMillis);
      minion.MakePet(this.m_record.EntityLowId);
      ++this.m_record.PetCount;
      this.InitializeMinion(minion);
      if (minion.Level < this.Level - PetMgr.MaxHunterPetLevelDifference)
      {
        minion.Level = this.Level - PetMgr.MaxHunterPetLevelDifference;
      }
      else
      {
        if (minion.Level <= this.Level)
          return;
        minion.Level = this.Level;
      }
    }

    /// <summary>
    /// Is called when this Character gets a new minion or pet or when
    /// he changes his ActivePet to the given one.
    /// </summary>
    private void InitializeMinion(NPC pet)
    {
      this.Summon = pet.EntityId;
      pet.Summoner = (Unit) this;
      pet.Creator = this.EntityId;
      pet.PetRecord.SetupPet(pet);
      pet.SetPetAttackMode(pet.PetRecord.AttackMode);
      this.ActivePet = pet;
      for (DamageSchool school = DamageSchool.Physical; school < DamageSchool.Count; ++school)
        pet.UpdatePetResistance(school);
    }

    /// <summary>Amount of purchased stable slots</summary>
    public int StableSlotCount
    {
      get
      {
        return this.Record.StableSlotCount;
      }
      set
      {
        this.Record.StableSlotCount = value;
      }
    }

    public PermanentPetRecord GetStabledPet(uint petNumber)
    {
      if (this.m_StabledPetRecords == null)
        return (PermanentPetRecord) null;
      foreach (PermanentPetRecord stabledPetRecord in this.m_StabledPetRecords)
      {
        if ((int) stabledPetRecord.PetNumber == (int) petNumber)
          return stabledPetRecord;
      }
      return (PermanentPetRecord) null;
    }

    public PermanentPetRecord GetStabledPetBySlot(uint stableSlot)
    {
      if (this.m_StabledPetRecords == null || (long) stableSlot > (long) this.m_StabledPetRecords.Count)
        return (PermanentPetRecord) null;
      return this.m_StabledPetRecords[(int) stableSlot];
    }

    /// <summary>
    /// Stable the currently ActivePet.
    /// Make sure there is at least one free StableSlot
    /// </summary>
    /// <returns>True if the pet was stabled.</returns>
    public void StablePet()
    {
      NPC activePet = this.ActivePet;
      activePet.PermanentPetRecord.StabledSince = new DateTime?(DateTime.Now);
      activePet.PetRecord.IsStabled = true;
      this.ActivePet = (NPC) null;
    }

    /// <summary>Tries to make the stabled pet the ActivePet</summary>
    /// <param name="stabledPermanentPet">The stabled pet to make Active.</param>
    /// <returns>True if the stabled was made Active.</returns>
    public void DeStablePet(PermanentPetRecord stabledPermanentPet)
    {
      this.m_StabledPetRecords.Remove(stabledPermanentPet);
      this.SpawnPet((IPetRecord) stabledPermanentPet).PermanentPetRecord.StabledSince = new DateTime?();
    }

    /// <summary>Tries to swap the ActivePet for a Stabled one.</summary>
    /// <param name="stabledPermanentPet">The stabled pet to swap out.</param>
    /// <returns>True if the Stabled Pet was swapped.</returns>
    public bool TrySwapStabledPet(PermanentPetRecord stabledPermanentPet)
    {
      if (this.StabledPetRecords.Count >= this.StableSlotCount + 1)
        return false;
      NPC activePet = this.m_activePet;
      if (activePet == null)
        return false;
      PermanentPetRecord petRecord = activePet.PetRecord as PermanentPetRecord;
      if (petRecord == null)
        return false;
      petRecord.IsStabled = true;
      this.DeStablePet(stabledPermanentPet);
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
      IPetRecord record = (IPetRecord) null;
      if (this.m_record.PetSummonedCount > 0)
      {
        foreach (SummonedPetRecord summonedPetRecord in SummonedPetRecord.LoadSummonedPetRecords(this.m_record.EntityLowId))
        {
          if (summonedPetRecord.IsActivePet)
            record = (IPetRecord) summonedPetRecord;
          this.SummonedPetRecords.Add(summonedPetRecord);
        }
      }
      if (this.m_record.PetCount > 0)
      {
        foreach (PermanentPetRecord permanentPetRecord in PermanentPetRecord.LoadPermanentPetRecords(this.m_record.EntityLowId))
        {
          if (permanentPetRecord.IsActivePet)
            record = (IPetRecord) permanentPetRecord;
          this.StabledPetRecords.Add(permanentPetRecord);
        }
      }
      if (this.m_record.PetEntryId == (NPCId) 0 || !this.IsPetActive)
        return;
      if (record != null)
      {
        if (record.Entry == null)
        {
          Unit.log.Warn("{0} has invalid PetEntryId: {1} ({2})", (object) this, (object) this.m_record.PetEntryId, (object) this.m_record.PetEntryId);
          this.AddPetRecord(record);
        }
        else
          this.SpawnActivePet(record);
      }
      else
      {
        this.m_record.PetEntryId = (NPCId) 0;
        this.m_record.IsPetActive = false;
      }
    }

    private void SpawnActivePet(IPetRecord record)
    {
      this.AddMessage((Action) (() =>
      {
        IActivePetSettings record1 = (IActivePetSettings) this.m_record;
        NPC npc = this.SpawnPet(record, ref this.m_position, record1.PetDuration);
        npc.CreationSpellId = record1.PetSummonSpellId;
        npc.Health = record1.PetHealth;
        npc.Power = record1.PetPower;
      }));
    }

    private void AddPetRecord(IPetRecord record)
    {
      if (record is SummonedPetRecord)
        this.SummonedPetRecords.Add((SummonedPetRecord) record);
      else if (record is PermanentPetRecord)
        this.StabledPetRecords.Add((PermanentPetRecord) record);
      else
        Unit.log.Warn("Unclassified PetRecord: " + (object) record);
    }

    internal void SaveEntourage()
    {
      if (this.m_activePet != null)
      {
        this.m_activePet.UpdatePetData((IActivePetSettings) this.m_record);
        this.m_activePet.PetRecord.Save();
      }
      Character.CommitPetChanges<PermanentPetRecord>((IList<PermanentPetRecord>) this.m_StabledPetRecords);
      Character.CommitPetChanges<SummonedPetRecord>((IList<SummonedPetRecord>) this.m_SummonedPetRecords);
    }

    private static void CommitPetChanges<T>(IList<T> records) where T : IPetRecord
    {
      if (records == null)
        return;
      for (int index = 0; index < records.Count; ++index)
      {
        T record = records[index];
        if (record.IsDirty)
          record.Save();
      }
    }

    public bool CanControlExoticPets { get; set; }

    /// <summary>TODO: This seems awfully unsafe and inconsistent</summary>
    public int PetBonusTalentPoints
    {
      get
      {
        return this.m_petBonusTalentPoints;
      }
      set
      {
        int num = value - this.m_petBonusTalentPoints;
        foreach (PermanentPetRecord stabledPetRecord in this.StabledPetRecords)
          stabledPetRecord.FreeTalentPoints += num;
        if (this.m_activePet != null)
          this.m_activePet.FreeTalentPoints += num;
        this.m_petBonusTalentPoints = value;
      }
    }

    internal SummonedPetRecord GetOrCreateSummonedPetRecord(NPCEntry entry)
    {
      SummonedPetRecord petRecord = this.GetOrCreatePetRecord<SummonedPetRecord>(entry, (IList<SummonedPetRecord>) this.SummonedPetRecords);
      petRecord.PetNumber = (uint) PetMgr.PetNumberGenerator.Next();
      return petRecord;
    }

    internal T GetOrCreatePetRecord<T>(NPCEntry entry, IList<T> list) where T : IPetRecord, new()
    {
      foreach (T obj in (IEnumerable<T>) list)
      {
        if (obj.EntryId == entry.NPCId)
          return obj;
      }
      if (typeof (T) == typeof (SummonedPetRecord))
        ++this.m_record.PetSummonedCount;
      else
        ++this.m_record.PetCount;
      return PetMgr.CreateDefaultPetRecord<T>(entry, this.m_record.EntityLowId);
    }

    protected internal override void OnMinionEnteredMap(NPC minion)
    {
      base.OnMinionEnteredMap(minion);
      if (minion.Entry.Type == CreatureType.Totem)
      {
        if (this.m_totems == null)
          this.m_totems = new NPC[4];
        uint totemIndex = minion.GetTotemIndex();
        if (this.m_totems[totemIndex] != null)
          this.m_totems[totemIndex].Delete();
        this.m_totems[totemIndex] = minion;
      }
      else
      {
        if (minion == this.m_activePet)
          return;
        if (this.m_minions == null)
          this.m_minions = new NPCCollection();
        this.m_minions.Add(minion);
      }
    }

    protected internal override void OnMinionLeftMap(NPC minion)
    {
      base.OnMinionLeftMap(minion);
      if (minion == this.m_activePet)
      {
        if (this.m_activePet.PetRecord == null)
          return;
        this.m_activePet.UpdatePetData((IActivePetSettings) this.m_record);
        ((ActiveRecordBase) this.m_activePet.PetRecord).SaveLater();
      }
      else if (minion.Entry.Type == CreatureType.Totem)
      {
        if (this.m_totems == null)
          return;
        uint totemIndex = minion.GetTotemIndex();
        if (this.m_totems[totemIndex] != minion)
          return;
        this.m_totems[totemIndex] = (NPC) null;
      }
      else
      {
        if (this.m_minions == null)
          return;
        this.m_minions.Remove(minion);
      }
    }

    /// <summary>Called when a Pet or</summary>
    /// <param name="minion"></param>
    protected internal override void OnMinionDied(NPC minion)
    {
      base.OnMinionDied(minion);
      if (minion == this.m_activePet)
      {
        this.IsPetActive = false;
      }
      else
      {
        if (this.m_minions == null)
          return;
        this.m_minions.Remove(minion);
      }
    }

    public void RemoveSummonedEntourage()
    {
      if (this.Minions != null)
      {
        foreach (NPC npc in this.Minions.Where<NPC>((Func<NPC, bool>) (minion => minion != null)))
          this.DeleteMinion(npc);
      }
      if (this.Totems == null)
        return;
      foreach (NPC npc in ((IEnumerable<NPC>) this.Totems).Where<NPC>((Func<NPC, bool>) (totem => totem != null)))
        this.DeleteMinion(npc);
    }

    private void DeleteMinion(NPC npc)
    {
      if (npc.Summon != EntityId.Zero)
      {
        WorldObject worldObject = this.Map.GetObject(npc.Summon);
        if (worldObject != null)
          worldObject.Delete();
        npc.Summon = EntityId.Zero;
      }
      npc.Delete();
    }

    public bool OwnsGo(GOEntryId goId)
    {
      if (this.m_ownedGOs == null)
        return false;
      foreach (GameObject ownedGo in this.m_ownedGOs)
      {
        if (ownedGo.Entry.GOId == goId)
          return true;
      }
      return false;
    }

    public GameObject GetOwnedGO(GOEntryId id)
    {
      if (this.m_ownedGOs != null)
        return this.m_ownedGOs.Find((Predicate<GameObject>) (go => id == go.Entry.GOId));
      return (GameObject) null;
    }

    public GameObject GetOwnedGO(uint slot)
    {
      if (this.m_ownedGOs != null)
      {
        foreach (GameObject ownedGo in this.m_ownedGOs)
        {
          if ((int) ownedGo.Entry.SummonSlotId == (int) slot)
            return ownedGo;
        }
      }
      return (GameObject) null;
    }

    public void RemoveOwnedGO(uint slot)
    {
      if (this.m_ownedGOs == null)
        return;
      foreach (GameObject ownedGo in this.m_ownedGOs)
      {
        if ((int) ownedGo.Entry.SummonSlotId == (int) slot)
        {
          ownedGo.Delete();
          break;
        }
      }
    }

    public void RemoveOwnedGO(GOEntryId goId)
    {
      if (this.m_ownedGOs == null)
        return;
      foreach (GameObject ownedGo in this.m_ownedGOs)
      {
        if ((GOEntryId) ownedGo.EntryId == goId)
        {
          ownedGo.Delete();
          break;
        }
      }
    }

    internal void AddOwnedGO(GameObject go)
    {
      if (this.m_ownedGOs == null)
        this.m_ownedGOs = new List<GameObject>();
      go.Master = (Unit) this;
      this.m_ownedGOs.Add(go);
    }

    internal void OnOwnedGODestroyed(GameObject go)
    {
      if (this.m_ownedGOs == null)
        return;
      this.m_ownedGOs.Remove(go);
    }

    private void DetatchFromVechicle()
    {
      VehicleSeat vehicleSeat = this.VehicleSeat;
      if (vehicleSeat == null)
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
      this.Type |= ObjectTypes.Player;
      this.ChatChannels = new List<ChatChannel>(5);
      this.m_logoutTimer = new TimerEntry(0, Character.DefaultLogoutDelayMillis, (Action<int>) (totalTime => this.FinishLogout()));
      this.Account = acc;
      this.m_client = client;
      this.m_record = record;
      this.EntityId = EntityId.GetPlayerId(this.m_record.EntityLowId & 16777215U);
      this.m_name = this.m_record.Name;
      this.Archetype = ArchetypeMgr.GetArchetype(record.Race, record.Class);
      this.MainWeapon = (IAsda2Weapon) GenericWeapon.Fists;
      this.PowerType = this.m_archetype.Class.DefaultPowerType;
      this.StandState = StandState.Sit;
      this.Money = (uint) this.m_record.Money;
      this.Outfit = this.m_record.Outfit;
      this.ScaleX = 1f;
      this.Gender = this.m_record.Gender;
      this.Skin = this.m_record.Skin;
      this.Facial = this.m_record.Face;
      this.HairStyle = this.m_record.HairStyle;
      this.HairColor = this.m_record.HairColor;
      this.FacialHair = this.m_record.FacialHair;
      this.UnitFlags = UnitFlags.PlayerControlled;
      this.Experience = this.m_record.Xp;
      this.RestXp = this.m_record.RestXp;
      this.SetInt32((UpdateFieldId) UnitFields.LEVEL, this.m_record.Level);
      this.NextLevelXP = XpGenerator.GetXpForlevel(this.m_record.Level + 1);
      this.MaxLevel = RealmServerConfiguration.MaxCharacterLevel;
      this.RestState = RestState.Normal;
      this.Orientation = this.m_record.Orientation;
      this.m_bindLocation = (IWorldZoneLocation) new WorldZoneLocation(this.m_record.BindMap, new Vector3(this.m_record.BindX, this.m_record.BindY, this.m_record.BindZ), this.m_record.BindZone);
      this.PvPRank = (byte) 1;
      this.YieldsXpOrHonor = true;
      foreach (DamageSchool allDamageSchool in SpellConstants.AllDamageSchools)
        this.SetFloat((UpdateFieldId) ((PlayerFields) (1185U + allDamageSchool)), 1f);
      this.SetFloat((UpdateFieldId) PlayerFields.DODGE_PERCENTAGE, 1f);
      this.m_auras = (AuraCollection) new PlayerAuraCollection(this);
      this.m_spells = (SpellCollection) PlayerSpellCollection.Obtain(this);
      this.WatchedFaction = this.m_record.WatchedFaction;
      this.Faction = NPCMgr.DefaultFaction;
      this.m_reputations = new ReputationCollection(this);
      this.m_skills = new SkillCollection(this);
      this.m_talents = (TalentCollection) new PlayerTalentCollection(this);
      this.m_achievements = new AchievementCollection(this);
      this._asda2Inventory = new Asda2PlayerInventory(this);
      this.m_mailAccount = new MailAccount(this);
      this.m_questLog = new QuestLog(this);
      this.TutorialFlags = new TutorialFlags(this.m_record.TutorialFlags);
      this.UpdateSpellCritChance();
      this.m_taxiNodeMask = new TaxiNodeMask();
      this.PowerCostMultiplier = 1f;
      this.m_lastPlayTimeUpdate = DateTime.Now;
      this.MoveControl.Mover = (WorldObject) this;
      this.MoveControl.CanControl = true;
      this.IncMeleePermissionCounter();
      this.SpeedFactor = Unit.DefaultSpeedFactor;
      if (record.JustCreated)
      {
        this.ModStatsForLevel(this.m_record.Level);
        this.Asda2BaseAgility = CharacterFormulas.StatOnCreation;
        this.Asda2BaseIntellect = CharacterFormulas.StatOnCreation;
        this.Asda2BaseLuck = CharacterFormulas.StatOnCreation;
        this.Asda2BaseSpirit = CharacterFormulas.StatOnCreation;
        this.Asda2BaseStamina = CharacterFormulas.StatOnCreation;
        this.Asda2BaseStrength = CharacterFormulas.StatOnCreation;
      }
      else
      {
        this.BaseHealth = this.m_record.BaseHealth;
        this.SetBasePowerDontUpdate(this.m_record.BasePower);
        this.Asda2Strength = this.m_record.BaseStrength;
        this.Asda2Intellect = this.m_record.BaseIntellect;
        this.Asda2Agility = this.m_record.BaseAgility;
        this.Asda2Stamina = this.m_record.BaseStamina;
        this.Asda2Luck = this.m_record.BaseLuck;
        this.Asda2Spirit = this.m_record.BaseSpirit;
        this.Power = this.m_record.Power;
        this.SetInt32((UpdateFieldId) UnitFields.HEALTH, this.m_record.Health);
      }
      this.UpdateAsda2Agility();
      this.UpdateAsda2Stamina();
      this.UpdateAsda2Luck();
      this.UpdateAsda2Spirit();
      this.UpdateAsda2Intellect();
      this.UpdateAsda2Strength();
    }

    /// <summary>Loads this Character from DB when logging in.</summary>
    /// <remarks>Requires IO-Context.</remarks>
    protected internal void Load()
    {
      this.NativeDisplayId = this.m_archetype.Race.GetModel(this.m_record.Gender).DisplayId;
      this.Model = UnitMgr.DefaultModel;
      this.UpdateFreeTalentPointsSilently(0);
      if (this.m_record.JustCreated)
      {
        this.SpecProfiles = new SpecProfile[1]
        {
          SpecProfile.NewSpecProfile(this, 0)
        };
        this.m_record.KillsTotal = 0U;
        this.m_record.HonorToday = 0U;
        this.m_record.HonorYesterday = 0U;
        this.m_record.LifetimeHonorableKills = 0U;
        this.m_record.HonorPoints = 0U;
        this.m_record.ArenaPoints = 0U;
        this.m_record.TitlePoints = 0U;
        this.m_record.Rank = -1;
      }
      else
      {
        try
        {
          this.Asda2BaseAgility = this.Record.BaseAgility;
          this.Asda2BaseIntellect = this.Record.BaseIntellect;
          this.Asda2BaseStrength = this.Record.BaseStrength;
          this.Asda2BaseLuck = this.Record.BaseLuck;
          this.Asda2BaseSpirit = this.Record.BaseSpirit;
          this.Asda2BaseStamina = this.Record.BaseStamina;
          this.UpdateAsda2Agility();
          this.UpdateAsda2Intellect();
          this.UpdateAsda2Luck();
          this.UpdateAsda2Spirit();
          this.UpdateAsda2Stamina();
          this.UpdateAsda2Strength();
          this.InitGlyphsForLevel();
          this.SpecProfiles = SpecProfile.LoadAllOfCharacter(this);
          if (this.SpecProfiles.Length == 0)
            this.SpecProfiles = new SpecProfile[1]
            {
              SpecProfile.NewSpecProfile(this, 0)
            };
          if (this.m_record.CurrentSpecIndex >= this.SpecProfiles.Length)
            this.m_record.CurrentSpecIndex = 0;
          try
          {
            this.m_achievements.Load();
          }
          catch (Exception ex)
          {
            LogUtil.ErrorException(ex, string.Format("failed to load achievements, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
          }
          try
          {
            ((PlayerSpellCollection) this.m_spells).LoadSpellsAndTalents();
          }
          catch (Exception ex)
          {
            LogUtil.ErrorException(ex, string.Format("failed to load LoadSpellsAndTalents, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
          }
          try
          {
            ((PlayerSpellCollection) this.m_spells).LoadCooldowns();
          }
          catch (Exception ex)
          {
            LogUtil.ErrorException(ex, string.Format("failed to load LoadCooldowns, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
          }
          try
          {
            AuraRecord[] auras = AuraRecord.LoadAuraRecords(this.EntityId.Low);
            this.AddPostUpdateMessage((Action) (() => this.m_auras.InitializeAuras(auras)));
          }
          catch (Exception ex)
          {
            LogUtil.ErrorException(ex, string.Format("failed to load LoadAuraRecords, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
          }
          try
          {
            foreach (Asda2FishingBook record in Asda2FishingBook.LoadAll(this))
            {
              if (this.RegisteredFishingBooks.ContainsKey(record.Num))
                record.DeleteLater();
              else
                this.RegisteredFishingBooks.Add(record.Num, record);
            }
          }
          catch (Exception ex)
          {
            LogUtil.ErrorException(ex, string.Format("failed to load fishing books, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
          }
          try
          {
            foreach (Asda2MailMessage asda2MailMessage in Asda2MailMessage.LoadAll(this))
              this.MailMessages.Add(asda2MailMessage.Guid, asda2MailMessage);
          }
          catch (Exception ex)
          {
            LogUtil.ErrorException(ex, string.Format("failed to load mail messages, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
          }
          try
          {
            foreach (FunctionItemBuff functionItemBuff1 in FunctionItemBuff.LoadAll(this))
            {
              FunctionItemBuff functionItemBuff = functionItemBuff1;
              if (!this.PremiumBuffs.ContainsKey(functionItemBuff.Template.Category) && (functionItemBuff.Duration > 0L && !functionItemBuff.IsLongTime || functionItemBuff.EndsDate > DateTime.Now && functionItemBuff.IsLongTime))
              {
                if (functionItemBuff.IsLongTime)
                {
                  if (((IEnumerable<FunctionItemBuff>) this.LongTimePremiumBuffs).Count<FunctionItemBuff>((Func<FunctionItemBuff, bool>) (l =>
                  {
                    if (l != null)
                      return l.Template.Category == functionItemBuff.Template.Category;
                    return false;
                  })) > 0)
                  {
                    functionItemBuff.DeleteLater();
                    continue;
                  }
                  this.LongTimePremiumBuffs.AddElement<FunctionItemBuff>(functionItemBuff);
                  if (functionItemBuff.Template.Category == Asda2ItemCategory.PremiumPotions)
                    this.Asda2WingsItemId = (short) functionItemBuff.Template.Id;
                }
                else
                  this.PremiumBuffs.Add(functionItemBuff.Template.Category, functionItemBuff);
                this.ProcessFunctionalItemEffect(functionItemBuff, true);
              }
              else
                functionItemBuff.DeleteLater();
            }
          }
          catch (Exception ex)
          {
            LogUtil.ErrorException(ex, string.Format("failed to load premium buffs, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
          }
        }
        catch (Exception ex)
        {
          RealmDBMgr.OnDBError(ex);
          return;
        }
        try
        {
          if (this.m_record.LastLogout.HasValue)
          {
            DateTime now = DateTime.Now;
            this.RestXp += RestGenerator.GetRestXp(now - this.m_record.LastLogout.Value, this);
            this.m_lastRestUpdate = now;
          }
          else
            this.m_lastRestUpdate = DateTime.Now;
          this.KillsTotal = this.m_record.KillsTotal;
          this.HonorToday = this.m_record.HonorToday;
          this.HonorYesterday = this.m_record.HonorYesterday;
          this.LifetimeHonorableKills = this.m_record.LifetimeHonorableKills;
          this.HonorPoints = this.m_record.HonorPoints;
          this.ArenaPoints = this.m_record.ArenaPoints;
          this.Asda2TitlePoints = (int) this.m_record.TitlePoints;
          this.Asda2Rank = this.m_record.Rank;
          this.Health = this.m_record.Health;
          this.Power = this.m_record.Power;
          this.RecalculateFactionRank(true);
        }
        catch (Exception ex)
        {
          LogUtil.ErrorException(ex, string.Format("failed to load last load init, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
        }
      }
      this.LoadPets();
      this.ResetUpdateInfo();
    }

    /// <summary>
    /// Ensure correct size of array of explored zones and  copy explored zones to UpdateValues array
    /// </summary>
    private unsafe void SetExploredZones()
    {
      if (this.m_record.ExploredZones.Length != UpdateFieldMgr.ExplorationZoneFieldSize * 4)
      {
        byte[] exploredZones = this.m_record.ExploredZones;
        Array.Resize<byte>(ref exploredZones, UpdateFieldMgr.ExplorationZoneFieldSize * 4);
        this.m_record.ExploredZones = exploredZones;
      }
      fixed (byte* numPtr = this.m_record.ExploredZones)
      {
        int num = 0;
        for (PlayerFields playerFields = PlayerFields.EXPLORED_ZONES_1; playerFields < (PlayerFields) (1041 + UpdateFieldMgr.ExplorationZoneFieldSize); ++playerFields)
        {
          this.SetUInt32((UpdateFieldId) playerFields, *(uint*) (numPtr + num));
          num += 4;
        }
      }
    }

    internal void LoadQuests()
    {
      this.m_questLog.Load();
    }

    private void LoadEquipmentState()
    {
      if (this.m_record.CharacterFlags.HasFlag((Enum) CharEnumFlags.HideCloak))
        this.PlayerFlags |= PlayerFlags.HideCloak;
      if (!this.m_record.CharacterFlags.HasFlag((Enum) CharEnumFlags.HideHelm))
        return;
      this.PlayerFlags |= PlayerFlags.HideHelm;
    }

    private void LoadDeathState()
    {
      if (this.m_record.CorpseX.HasValue)
      {
        Map nonInstancedMap = WCell.RealmServer.Global.World.GetNonInstancedMap(this.m_record.CorpseMap);
        if (nonInstancedMap != null)
        {
          this.m_corpse = this.SpawnCorpse(false, false, nonInstancedMap, new Vector3(this.m_record.CorpseX.Value, this.m_record.CorpseY, this.m_record.CorpseZ), this.m_record.CorpseO);
          this.BecomeGhost();
        }
        else
        {
          if (!Unit.log.IsWarnEnabled)
            return;
          Unit.log.Warn("Player {0}'s Corpse was spawned in invalid map: {1}", (object) this, (object) this.m_record.CorpseMap);
        }
      }
      else
      {
        if (this.m_record.Health != 0)
          return;
        int initialDelay = DateTime.Now.Subtract(this.m_record.LastDeathTime).ToMilliSecondsInt() + Corpse.AutoReleaseDelay;
        this.m_corpseReleaseTimer = new TimerEntry((Action<int>) (dt => this.ReleaseCorpse()));
        if (initialDelay > 0)
        {
          this.MarkDead();
          this.m_corpseReleaseTimer.Start(initialDelay, 0);
        }
        else
          this.ReleaseCorpse();
      }
    }

    public void SetClass(int proffLevel, int proff)
    {
      if (this.Archetype.ClassId != ClassId.NoClass)
      {
        switch ((byte) this.Archetype.ClassId)
        {
          case 1:
            this.Spells.Remove(SpellId.FistofIronRank1);
            this.Spells.Remove(SpellId.FistofDestructionRank1);
            this.Spells.Remove(SpellId.FistofSplinterRank1);
            break;
          case 2:
            this.Spells.Remove(SpellId.RaidofThiefRank1);
            this.Spells.Remove(SpellId.RaidofBurglarRank1);
            this.Spells.Remove(SpellId.RaidofTraitorRank1);
            break;
          case 3:
            this.Spells.Remove(SpellId.SlicetheLandRank1);
            this.Spells.Remove(SpellId.SlicetheOceanRank1);
            this.Spells.Remove(SpellId.SlicetheSkyRank1);
            break;
          case 4:
            this.Spells.Remove(SpellId.SilencingShotRank1);
            this.Spells.Remove(SpellId.DestructiveShotRank1);
            this.Spells.Remove(SpellId.DarkShotRank1);
            break;
          case 5:
            this.Spells.Remove(SpellId.ExplosiveShotRank1);
            this.Spells.Remove(SpellId.DefeatingShotRank1);
            this.Spells.Remove(SpellId.SlicingShotRank1);
            break;
          case 6:
            this.Spells.Remove(SpellId.FirePiercingShotRank1);
            this.Spells.Remove(SpellId.IronPiercingShotRank1);
            this.Spells.Remove(SpellId.ImmortalPiercingShotRank1);
            break;
          case 7:
            this.Spells.Remove(SpellId.FlameofSinRank1);
            this.Spells.Remove(SpellId.FlameofPunishmentRank1);
            this.Spells.Remove(SpellId.FlameofExtinctionRank1);
            break;
          case 8:
            this.Spells.Remove(SpellId.CallofEarthquakeRank1);
            this.Spells.Remove(SpellId.CallofCrisisRank1);
            this.Spells.Remove(SpellId.CallofAnnihilationRank1);
            break;
          case 9:
            this.Spells.Remove(SpellId.FirstShockWaveRank1);
            this.Spells.Remove(SpellId.SecondShockWaveRank1);
            this.Spells.Remove(SpellId.ThirdShockWaveRank1);
            break;
        }
      }
      if (proff >= 1 && proff <= 3)
        this.ProfessionLevel = (byte) proffLevel;
      if (proff >= 4 && proff <= 6)
        this.ProfessionLevel = (byte) (proffLevel + 11);
      if (proff >= 7 && proff <= 9)
        this.ProfessionLevel = (byte) (proffLevel + 22);
      this.Archetype = ArchetypeMgr.GetArchetype(RaceId.Human, (ClassId) proff);
      switch (proff)
      {
        case 1:
          switch (this.RealProffLevel)
          {
            case 1:
              this.Spells.AddSpell(SpellId.FistofIronRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Impenetrable7)));
              return;
            case 2:
              this.Spells.AddSpell(SpellId.FistofIronRank1);
              this.Spells.AddSpell(SpellId.FistofDestructionRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Warrior15)));
              return;
            case 3:
              this.Spells.AddSpell(SpellId.FistofIronRank1);
              this.Spells.AddSpell(SpellId.FistofDestructionRank1);
              this.Spells.AddSpell(SpellId.FistofSplinterRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Soldier18)));
              return;
            case 4:
              this.Spells.AddSpell(SpellId.FistofIronRank1);
              this.Spells.AddSpell(SpellId.FistofDestructionRank1);
              this.Spells.AddSpell(SpellId.FistofSplinterRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Battlemaster21)));
              return;
            default:
              return;
          }
        case 2:
          switch (this.RealProffLevel)
          {
            case 1:
              this.Spells.AddSpell(SpellId.RaidofThiefRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Berserk9)));
              return;
            case 2:
              this.Spells.AddSpell(SpellId.RaidofThiefRank1);
              this.Spells.AddSpell(SpellId.RaidofBurglarRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Warrior15)));
              return;
            case 3:
              this.Spells.AddSpell(SpellId.RaidofThiefRank1);
              this.Spells.AddSpell(SpellId.RaidofBurglarRank1);
              this.Spells.AddSpell(SpellId.RaidofTraitorRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Soldier18)));
              return;
            case 4:
              this.Spells.AddSpell(SpellId.RaidofThiefRank1);
              this.Spells.AddSpell(SpellId.RaidofBurglarRank1);
              this.Spells.AddSpell(SpellId.RaidofTraitorRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Battlemaster21)));
              return;
            default:
              return;
          }
        case 3:
          switch (this.RealProffLevel)
          {
            case 1:
              this.Spells.AddSpell(SpellId.SlicetheLandRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Mighty8)));
              return;
            case 2:
              this.Spells.AddSpell(SpellId.SlicetheLandRank1);
              this.Spells.AddSpell(SpellId.SlicetheOceanRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Warrior15)));
              return;
            case 3:
              this.Spells.AddSpell(SpellId.SlicetheLandRank1);
              this.Spells.AddSpell(SpellId.SlicetheOceanRank1);
              this.Spells.AddSpell(SpellId.SlicetheSkyRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Soldier18)));
              return;
            case 4:
              this.Spells.AddSpell(SpellId.SlicetheLandRank1);
              this.Spells.AddSpell(SpellId.SlicetheOceanRank1);
              this.Spells.AddSpell(SpellId.SlicetheSkyRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Battlemaster21)));
              return;
            default:
              return;
          }
        case 4:
          switch (this.RealProffLevel)
          {
            case 1:
              this.Spells.AddSpell(SpellId.SilencingShotRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Critical10)));
              return;
            case 2:
              this.Spells.AddSpell(SpellId.SilencingShotRank1);
              this.Spells.AddSpell(SpellId.DestructiveShotRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Archer16)));
              return;
            case 3:
              this.Spells.AddSpell(SpellId.SilencingShotRank1);
              this.Spells.AddSpell(SpellId.DestructiveShotRank1);
              this.Spells.AddSpell(SpellId.DarkShotRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Sharpshooter19)));
              return;
            case 4:
              this.Spells.AddSpell(SpellId.SilencingShotRank1);
              this.Spells.AddSpell(SpellId.DestructiveShotRank1);
              this.Spells.AddSpell(SpellId.DarkShotRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Chaser22)));
              return;
            default:
              return;
          }
        case 5:
          switch (this.RealProffLevel)
          {
            case 1:
              this.Spells.AddSpell(SpellId.ExplosiveShotRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Bloody11)));
              return;
            case 2:
              this.Spells.AddSpell(SpellId.ExplosiveShotRank1);
              this.Spells.AddSpell(SpellId.DefeatingShotRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Archer16)));
              return;
            case 3:
              this.Spells.AddSpell(SpellId.ExplosiveShotRank1);
              this.Spells.AddSpell(SpellId.DefeatingShotRank1);
              this.Spells.AddSpell(SpellId.SlicingShotRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Sharpshooter19)));
              return;
            case 4:
              this.Spells.AddSpell(SpellId.ExplosiveShotRank1);
              this.Spells.AddSpell(SpellId.DefeatingShotRank1);
              this.Spells.AddSpell(SpellId.SlicingShotRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Chaser22)));
              return;
            default:
              return;
          }
        case 6:
          switch (this.RealProffLevel)
          {
            case 1:
              this.Spells.AddSpell(SpellId.FirePiercingShotRank1);
              return;
            case 2:
              this.Spells.AddSpell(SpellId.FirePiercingShotRank1);
              this.Spells.AddSpell(SpellId.IronPiercingShotRank1);
              return;
            case 3:
              this.Spells.AddSpell(SpellId.FirePiercingShotRank1);
              this.Spells.AddSpell(SpellId.IronPiercingShotRank1);
              this.Spells.AddSpell(SpellId.ImmortalPiercingShotRank1);
              return;
            case 4:
              this.Spells.AddSpell(SpellId.FirePiercingShotRank1);
              this.Spells.AddSpell(SpellId.IronPiercingShotRank1);
              this.Spells.AddSpell(SpellId.ImmortalPiercingShotRank1);
              return;
            default:
              return;
          }
        case 7:
          switch (this.RealProffLevel)
          {
            case 1:
              this.Spells.AddSpell(SpellId.FlameofSinRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Hells12)));
              return;
            case 2:
              this.Spells.AddSpell(SpellId.FlameofSinRank1);
              this.Spells.AddSpell(SpellId.FlameofPunishmentRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Mage17)));
              return;
            case 3:
              this.Spells.AddSpell(SpellId.FlameofSinRank1);
              this.Spells.AddSpell(SpellId.FlameofPunishmentRank1);
              this.Spells.AddSpell(SpellId.FlameofExtinctionRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Elementalist20)));
              return;
            case 4:
              this.Spells.AddSpell(SpellId.FlameofSinRank1);
              this.Spells.AddSpell(SpellId.FlameofPunishmentRank1);
              this.Spells.AddSpell(SpellId.FlameofExtinctionRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Archmage23)));
              return;
            default:
              return;
          }
        case 8:
          switch (this.RealProffLevel)
          {
            case 1:
              this.Spells.AddSpell(SpellId.CallofEarthquakeRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Earths13)));
              return;
            case 2:
              this.Spells.AddSpell(SpellId.CallofEarthquakeRank1);
              this.Spells.AddSpell(SpellId.CallofCrisisRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Mage17)));
              return;
            case 3:
              this.Spells.AddSpell(SpellId.CallofEarthquakeRank1);
              this.Spells.AddSpell(SpellId.CallofCrisisRank1);
              this.Spells.AddSpell(SpellId.CallofAnnihilationRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Elementalist20)));
              return;
            case 4:
              this.Spells.AddSpell(SpellId.CallofEarthquakeRank1);
              this.Spells.AddSpell(SpellId.CallofCrisisRank1);
              this.Spells.AddSpell(SpellId.CallofAnnihilationRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Archmage23)));
              return;
            default:
              return;
          }
        case 9:
          switch (this.RealProffLevel)
          {
            case 1:
              this.Spells.AddSpell(SpellId.FirstShockWaveRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Heavens14)));
              return;
            case 2:
              this.Spells.AddSpell(SpellId.FirstShockWaveRank1);
              this.Spells.AddSpell(SpellId.SecondShockWaveRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Mage17)));
              return;
            case 3:
              this.Spells.AddSpell(SpellId.FirstShockWaveRank1);
              this.Spells.AddSpell(SpellId.SecondShockWaveRank1);
              this.Spells.AddSpell(SpellId.ThirdShockWaveRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Elementalist20)));
              return;
            case 4:
              this.Spells.AddSpell(SpellId.FirstShockWaveRank1);
              this.Spells.AddSpell(SpellId.SecondShockWaveRank1);
              this.Spells.AddSpell(SpellId.ThirdShockWaveRank1);
              this.Map.CallDelayed(700, (Action) (() => this.GetTitle(Asda2TitleId.Archmage23)));
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
      this.m_Map = WCell.RealmServer.Global.World.GetMap((IMapId) this.m_record);
      InstanceMgr.RetrieveInstances(this);
      ++this.AreaCharCount;
      if (!this.Role.IsStaff)
        ++this.Stunned;
      bool isStaff = this.Role.IsStaff;
      if (this.m_Map == null && (!isStaff || (this.m_Map = (Map) InstanceMgr.CreateInstance(this, this.m_record.MapId)) == null))
      {
        this.Load();
        this.TeleportToBindLocation();
        this.AddMessage(new Action(this.InitializeCharacter));
      }
      else
      {
        this.Load();
        if (!this.m_Map.IsDisposed)
        {
          if (this.m_Map.IsInstance && !isStaff)
          {
            DateTime creationTime = this.m_Map.CreationTime;
            DateTime? lastLogout = this.m_record.LastLogout;
            if ((lastLogout.HasValue ? (creationTime > lastLogout.GetValueOrDefault() ? 1 : 0) : 0) != 0 || !this.m_Map.CanEnter(this))
              goto label_7;
          }
          this.m_Map.AddMessage((Action) (() =>
          {
            if (this.m_Map is Battleground && !((Battleground) this.m_Map).LogBackIn(this))
            {
              this.AddMessage(new Action(this.InitializeCharacter));
            }
            else
            {
              this.m_position = new Vector3(this.m_record.PositionX, this.m_record.PositionY, this.m_record.PositionZ);
              this.m_zone = this.m_Map.GetZone(this.m_record.Zone);
              if (this.m_zone != null && this.m_record.JustCreated)
                this.SetZoneExplored(this.m_zone.Id, false);
              this.InitializeCharacter();
            }
          }));
          return;
        }
label_7:
        this.m_Map.TeleportOutside(this);
        this.AddMessage(new Action(this.InitializeCharacter));
      }
    }

    /// <summary>
    /// Is called after Character has been added to a map the first time and
    /// before it receives the first Update packet
    /// </summary>
    protected internal void InitializeCharacter()
    {
      WCell.RealmServer.Global.World.AddCharacter(this);
      this.m_initialized = true;
      try
      {
        this.Regenerates = true;
        ((PlayerSpellCollection) this.m_spells).PlayerInitialize();
        if (this.m_record.JustCreated)
        {
          if (this.m_zone != null)
            this.m_zone.EnterZone(this, (Zone) null);
          this.m_spells.AddDefaultSpells();
          this.m_reputations.Initialize();
          this.Skills.UpdateSkillsForLevel(this.Level);
        }
        else
        {
          this.LoadDeathState();
          this.LoadEquipmentState();
        }
        this.InitItems();
        this.LoadAsda2Pets();
        this.LoadAsda2Mounts();
        this.LoadAsda2TeleportPoints();
        this.LoadFriends();
        Ticket ticket = TicketMgr.Instance.GetTicket(this.EntityId.Low);
        if (ticket != null)
        {
          this.Ticket = ticket;
          this.Ticket.OnOwnerLogin(this);
        }
        Singleton<GroupMgr>.Instance.OnCharacterLogin(this);
        Singleton<GuildMgr>.Instance.OnCharacterLogin(this);
        Singleton<RelationMgr>.Instance.OnCharacterLogin(this);
        this.GetedTitles = new UpdateMask(this.Record.GetedTitles);
        this.DiscoveredTitles = new UpdateMask(this.Record.DiscoveredTitles);
        this.LearnedRecipes = new UpdateMask(this.Record.LearnedRecipes);
        for (int index = 0; index < 576; ++index)
        {
          if (this.LearnedRecipes.GetBit(index))
            ++this.LearnedRecipesCount;
        }
        for (int index = 0; index < this.GetedTitles.HighestIndex; ++index)
        {
          if (this.GetedTitles.GetBit(index))
            this.Asda2TitlePoints += (int) Asda2TitleTemplate.Templates[index].Points;
        }
        this.LastLogin = DateTime.Now;
        bool isNew = this.m_record.JustCreated;
        this.AddMessage((Action) (() =>
        {
          if (!this.LastLogout.HasValue)
            RealmCommandHandler.ExecFirstLoginFileFor(this);
          RealmCommandHandler.ExecAllCharsFileFor(this);
          if (this.Account.Role.IsStaff)
            RealmCommandHandler.ExecFileFor(this);
          --this.Stunned;
          if (this.m_record.NextTaxiVertexId != 0)
          {
            PathVertex vertex = TaxiMgr.GetVertex(this.m_record.NextTaxiVertexId);
            if (vertex != null && vertex.MapId == this.m_Map.Id && (vertex.ListEntry.Next != null && this.IsInRadius(vertex.Pos, vertex.ListEntry.Next.Value.DistFromPrevious)))
            {
              this.TaxiPaths.Enqueue(vertex.Path);
              TaxiMgr.FlyUnit((Unit) this, true, vertex.ListEntry);
            }
            else
              this.m_record.NextTaxiVertexId = 0;
          }
          else
            this.StandState = StandState.Stand;
          this.GodMode = this.m_record.GodMode;
          if (isNew)
          {
            Action<Character> created = Character.Created;
            if (created != null)
              created(this);
          }
          if (this.GodMode)
            this.Map.CallDelayed(5000, (Action) (() => this.SendSystemMessage("God mode is activated.")));
          Character.CharacterLoginHandler loggedIn = Character.LoggedIn;
          if (loggedIn == null)
            return;
          loggedIn(this, true);
        }));
        if (isNew)
        {
          this.SaveLater();
          this.m_record.JustCreated = false;
        }
        else
          ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
          {
            try
            {
              this.m_record.Update();
            }
            catch (Exception ex)
            {
              this.SaveLater();
              LogUtil.ErrorException(ex, "Failed to Update CharacterRecord: " + (object) this.m_record, new object[0]);
            }
          }));
        this.OnLogin();
      }
      catch (Exception ex)
      {
        if (this.m_record.JustCreated)
        {
          this.m_record.CanSave = false;
          this.m_record.Delete();
        }
        WCell.RealmServer.Global.World.RemoveCharacter(this);
        LogUtil.ErrorException(ex, "Failed to initialize Character: " + (object) this, new object[0]);
        this.m_client.Disconnect(false);
      }
    }

    private void LoadFriends()
    {
      this.FriendRecords = Asda2FriendshipRecord.LoadAll(this.EntityId.Low);
      foreach (Asda2FriendshipRecord friendRecord in this.FriendRecords)
      {
        uint friendId = friendRecord.GetFriendId(this.EntityId.Low);
        CharacterRecord characterRecord = CharacterRecord.LoadRecordByEntityId(friendId);
        if (characterRecord == null)
          Unit.log.Warn(string.Format("Friendship record broken cause character {0} not founded.", (object) friendId));
        else if (this.Friends.ContainsKey((uint) characterRecord.AccountId))
        {
          friendRecord.DeleteLater();
        }
        else
        {
          this.Friends.Add((uint) characterRecord.AccountId, characterRecord);
          Character characterByAccId = WCell.RealmServer.Global.World.GetCharacterByAccId((uint) characterRecord.AccountId);
          if (characterByAccId != null)
            characterByAccId.SendInfoMsg(string.Format("Your friend {0} is now online.", (object) this.Name));
        }
      }
    }

    private void LoadAsda2TeleportPoints()
    {
      Asda2TeleportingPointRecord[] teleportingPointRecordArray = Asda2TeleportingPointRecord.LoadItems(this.EntityId.Low);
      for (int index = 0; index < teleportingPointRecordArray.Length; ++index)
        this.TeleportPoints[index] = teleportingPointRecordArray[index];
    }

    private void LoadAsda2Pets()
    {
      foreach (Asda2PetRecord asda2PetRecord in Asda2PetRecord.LoadAll(this))
        this.OwnedPets.Add(asda2PetRecord.Guid, asda2PetRecord);
    }

    private void LoadAsda2Mounts()
    {
      foreach (Asda2MountRecord asda2MountRecord in Asda2MountRecord.GetAllRecordsOfCharacter(this.EntityId.Low))
        this.OwnedMounts.Add(asda2MountRecord.Id, asda2MountRecord);
    }

    /// <summary>
    /// Load items from DB or (if new char) add initial Items.
    /// Happens either on login or when items have been loaded during runtime
    /// </summary>
    protected internal void InitItems()
    {
      if (this.m_record.JustCreated)
        this._asda2Inventory.FillOnCharacterCreate();
      else
        this._asda2Inventory.AddOwnedItems();
    }

    /// <summary>
    /// Called within Map Context.
    /// Sends initial packets
    /// </summary>
    private void OnLogin()
    {
      this.IsConnected = true;
      if (this.IsLoginServerStep)
      {
        Asda2LoginHandler.SendEnterGameResposeResponse(this.m_client);
        Asda2LoginHandler.SendEnterGameResponseItemsOnCharacterResponse(this.m_client);
        Asda2LoginHandler.SendEnterWorldIpeResponseResponse(this.m_client);
        this.Client.Disconnect(true);
      }
      else
      {
        this.Map.AddObjectNow((WorldObject) this);
        if (this.IsFirstGameConnection)
          this.IsAsda2Teleporting = false;
        if (this.Experience < 0)
        {
          this.Experience = 1;
          LogUtil.WarnException("Character {0} has negative exp. Set it to 1.", new object[1]
          {
            (object) this.Name
          });
        }
        if (this.Record.WarehousePassword != null)
          this.IsWarehouseLocked = true;
        Asda2CharacterHandler.SendSomeInitGSResponse(this.m_client);
        Asda2CharacterHandler.SendSomeInitGSOneResponse(this.m_client);
        Asda2CharacterHandler.SendCharacterInfoSessIdPositionResponse(this.m_client);
        Asda2LoginHandler.SendInventoryInfoResponse(this.m_client);
        Asda2CharacterHandler.SendUpdateStatsResponse(this.m_client);
        Asda2CharacterHandler.SendUpdateStatsOneResponse(this.m_client);
        Asda2InventoryHandler.SendAllFastItemSlotsInfo(this);
        if (this.IsFirstGameConnection)
          Asda2CharacterHandler.SendLearnedSkillsInfo(this);
        Asda2CharacterHandler.SendMySessionIdResponse(this.m_client);
        Asda2CharacterHandler.SendPetBoxSizeInitResponse(this);
        Asda2QuestHandler.SendQuestsListResponse(this.m_client);
        Asda2TitlesHandler.SendDiscoveredTitlesResponse(this.Client);
        Asda2TitlesHandler.SendGetedTitlesResponse(this.Client);
        GlobalHandler.SendCharacterPlaceInTitleRatingResponse(this.Client, this);
        Asda2CraftingHandler.SendLeanedRecipesResponse(this.Client);
        Asda2MountHandler.SendMountBoxSizeInitResponse(this.Client);
        if (this.OwnedMounts.Count > 0)
          Asda2MountHandler.SendOwnedMountsListResponse(this.Client);
        if (this.Asda2Pet != null)
          GlobalHandler.SendCharacterInfoPetResponse(this.Client, this);
        if (this.RegisteredFishingBooks.Count > 0)
        {
          foreach (KeyValuePair<byte, Asda2FishingBook> registeredFishingBook in this.RegisteredFishingBooks)
            Asda2FishingHandler.SendFishingBooksInfoResponse(this.Client, registeredFishingBook.Value);
          Asda2FishingHandler.SendFishingBookListEndedResponse(this.Client);
        }
        if (this.IsInGuild)
        {
          GlobalHandler.SendCharacterInfoClanNameResponse(this.Client, this);
          Asda2GuildHandler.SendClanFlagAndClanNameInfoSelfResponse(this);
          Asda2GuildHandler.SendGuildInfoOnLoginResponse(this, this.Guild);
          Asda2GuildHandler.SendGuildSkillsInfoResponse(this);
          Asda2GuildHandler.SendGuildNotificationResponse(this.Guild, GuildNotificationType.LoggedIn, this.GuildMember);
          this.Map.CallDelayed(2000, (Action) (() => Asda2GuildHandler.SendGuildMembersInfoResponse(this.Client, this.Guild)));
        }
        if (this.IsInGroup)
          this.Group.SendUpdate();
        GlobalHandler.SendCharacterFactionAndFactionRankResponse(this.Client, this);
        Asda2SoulmateHandler.SendCharacterSoulMateIntrodactionUpdateResponse(this.Client);
        if (this.FreeStatPoints > 0)
          this.Map.CallDelayed(15000, (Action) (() =>
          {
            this.SendInfoMsg(string.Format("You have {0} rest expirience.", (object) this.RestXp));
            this.SendInfoMsg(string.Format("You have {0} free stat points. Stat point can be used in VCHRMRG.", (object) this.FreeStatPoints));
          }));
        Asda2LoginHandler.SendLongTimeBuffsInfoResponse(this.Client);
        this.ProcessSoulmateRelation(true);
        Asda2CharacterHandler.SendFactionAndHonorPointsInitResponse(this.Client);
        Asda2FishingHandler.SendFishingLvlResponse(this.Client);
        GlobalHandler.SendSetClientTimeResponse(this.Client);
        if (this.PrivateShop != null)
          this.Map.CallDelayed(4000, (Action) (() => this.PrivateShop.ShowOnLogin(this)));
        if (this.Asda2TradeWindow != null)
          this.Asda2TradeWindow.CancelTrade();
        this.UpdateSettings();
        this.Map.CallDelayed(3800, (Action) (() =>
        {
          if (this.Asda2TradeDescription.Contains("[OFFLINE]") && this.Asda2TradeDescription.Length > 10)
            this.Asda2TradeDescription = this.Asda2TradeDescription.Substring(10);
          if (this.Asda2TradeDescription.Contains("[OFFLINE]"))
          {
            this.IsAsda2TradeDescriptionEnabled = false;
            this.Asda2TradeDescription = "";
          }
          this.IsSitting = false;
          if (this.IsOnTransport)
            FunctionalItemsHandler.SendShopItemUsedResponse(this.Client, this.TransportItemId, -1);
          if (this.IsOnMount)
            Asda2MountHandler.SendCharacterOnMountStatusChangedResponse(this, Asda2MountHandler.UseMountStatus.Ok);
          if (this.IsDead)
            Asda2CharacterHandler.SendSelfDeathResponse(this);
          Asda2AuctionMgr.OnLogin(this);
          Asda2CharacterHandler.SendRates(this, 3, 3);
          if (this.IsAsda2BattlegroundInProgress)
          {
            this.CurrentBattleGround.SendCurrentProgress(this);
            Asda2BattlegroundHandler.SendWarTeamListResponse(this);
            Asda2BattlegroundHandler.SendTeamPointsResponse(this.CurrentBattleGround, this);
            Asda2BattlegroundHandler.SendHowManyPeopleInWarTeamsResponse(this.CurrentBattleGround, this);
            GlobalHandler.SendFightingModeChangedOnWarResponse(this.Client, this.SessionId, (int) this.AccId, (int) this.Asda2FactionId);
            Asda2BattlegroundHandler.SendWarRemainingTimeResponse(this.Client);
            if (!this.CurrentBattleGround.IsStarted)
              Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this.CurrentBattleGround, BattleGroundInfoMessageType.PreWarCircle, (short) -1, (Character) null, new short?());
            if (this.CurrentBattleGround.WarType == Asda2BattlegroundType.Occupation)
            {
              foreach (Asda2WarPoint point in this.CurrentBattleGround.Points)
                Asda2BattlegroundHandler.SendWarPointsPreInitResponse(this.Client, point);
            }
          }
          if (this.MailMessages.Count > 0)
          {
            int amount = this.MailMessages.Values.Count<Asda2MailMessage>((Func<Asda2MailMessage, bool>) (asda2MailMessage => !asda2MailMessage.IsReaded));
            if (amount > 0)
            {
              this.SendMailMsg(string.Format("You have {0} unreaded messages.", (object) amount));
              Asda2MailHandler.SendYouHaveNewMailResponse(this.Client, amount);
            }
          }
          if (((IEnumerable<Asda2TeleportingPointRecord>) this.TeleportPoints).Count<Asda2TeleportingPointRecord>((Func<Asda2TeleportingPointRecord, bool>) (c => c != null)) > 0)
            this.Map.CallDelayed(1000, (Action) (() => Asda2TeleportHandler.SendSavedLocationsInitResponse(this.Client)));
          if (this.Asda2Inventory.DonationItems.Count<KeyValuePair<int, Asda2DonationItem>>((Func<KeyValuePair<int, Asda2DonationItem>, bool>) (di => !di.Value.Recived)) > 0)
            Asda2InventoryHandler.SendSomeNewItemRecivedResponse(this.Client, 20551, (byte) 102);
          FunctionalItemsHandler.SendWingsInfoResponse(this, this.Client);
          foreach (KeyValuePair<Asda2ItemCategory, FunctionItemBuff> premiumBuff in this.PremiumBuffs)
          {
            for (int index = 0; index < (int) premiumBuff.Value.Stacks; ++index)
              FunctionalItemsHandler.SendShopItemUsedResponse(this.Client, premiumBuff.Value.ItemId, (int) premiumBuff.Value.Duration / 1000);
          }
          foreach (DonationRecord donationRecord in ((IEnumerable<DonationRecord>) ActiveRecordBase<DonationRecord>.FindAllByProperty("CharacterName", (object) this.Name)).Where<DonationRecord>((Func<DonationRecord, bool>) (r => !r.IsDelivered)).ToList<DonationRecord>())
          {
            DonationRecord record = donationRecord;
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() => this.Asda2Inventory.AddDonateItem(Asda2ItemMgr.GetTemplate(CharacterFormulas.DonationItemId), record.Amount, "donation_system", false)));
            donationRecord.IsDelivered = true;
            donationRecord.DeliveredDateTime = new DateTime?(DateTime.Now);
            donationRecord.Update();
          }
        }));
        foreach (KeyValuePair<int, Asda2PetRecord> ownedPet in this.OwnedPets)
          Asda2PetHandler.SendInitPetInfoOnLoginResponse(this.Client, ownedPet.Value);
        if (this.IsFirstGameConnection)
        {
          this.SendInfoMsg("This server is running temporarily");
          this.IsFirstGameConnection = false;
        }
      }
      this.IsLoginServerStep = false;
    }

    public void ProcessSoulmateRelation(bool callOnSoulmate)
    {
      this.SoulmateRecord = Asda2SoulmateMgr.GetSoulmateRecord((uint) this.Account.AccountId);
      if (this.SoulmateRecord == null)
      {
        Asda2SoulmateHandler.SendDisbandSoulMateResultResponse(this.Client, DisbandSoulmateResult.SoulmateReleased, "friend");
        this.SoulmateRealmAccount = (RealmAccount) null;
        this.SoulmatedCharactersRecords = (CharacterRecord[]) null;
      }
      else
      {
        uint num = (long) this.SoulmateRecord.AccId == (long) this.Account.AccountId ? this.SoulmateRecord.RelatedAccId : this.SoulmateRecord.AccId;
        WCell.RealmServer.Auth.Accounts.Account account = AccountMgr.GetAccount((long) num);
        if (account == null)
        {
          this.SoulmateRecord.DeleteLater();
          this.SoulmateRecord = (Asda2SoulmateRelationRecord) null;
          this.SoulmatedCharactersRecords = (CharacterRecord[]) null;
          Asda2SoulmateHandler.SendDisbandSoulMateResultResponse(this.Client, DisbandSoulmateResult.SoulmateReleased, "friend");
        }
        else
        {
          this.SoulmatedCharactersRecords = CharacterRecord.FindAllOfAccount((int) num);
          this.SoulmateRealmAccount = ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(account.Name);
          if (this.SoulmateRealmAccount != null && this.SoulmateRealmAccount.ActiveCharacter != null)
          {
            this.SoulmateRealmAccount.ActiveCharacter.SoulmateRecord = this.SoulmateRecord;
            if (callOnSoulmate)
              this.SoulmateCharacter.ProcessSoulmateRelation(false);
            this.Map.CallDelayed(500, (Action) (() => Asda2SoulmateHandler.SendSoulMateInfoInitResponse(this, true)));
            this.Map.CallDelayed(1000, (Action) (() => Asda2SoulmateHandler.SendSoulmateEnterdGameResponse(this.Client)));
            this.Map.CallDelayed(1500, (Action) (() => Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(this.Client)));
            this.Map.CallDelayed(2000, (Action) (() => Asda2SoulmateHandler.SendSoulmatePositionResponse(this.Client)));
          }
          else
            Asda2SoulmateHandler.SendSoulMateInfoInitResponse(this, false);
        }
      }
    }

    public void DiscoverTitle(Asda2TitleId titleId)
    {
      if (this.DiscoveredTitles.GetBit((int) titleId))
        return;
      this.DiscoveredTitles.SetBit((int) titleId);
      Asda2TitlesHandler.SendTitleDiscoveredResponse(this.Client, (short) titleId);
    }

    public void GetTitle(Asda2TitleId titleId)
    {
      if (this.GetedTitles.GetBit((int) titleId))
        return;
      AchievementProgressRecord progressRecord = this.Achievements.GetOrCreateProgressRecord(5U);
      switch (++progressRecord.Counter)
      {
        case 25:
          this.DiscoverTitle(Asda2TitleId.Collector42);
          break;
        case 50:
          this.GetTitle(Asda2TitleId.Collector42);
          break;
        case 75:
          this.DiscoverTitle(Asda2TitleId.Maniac43);
          break;
        case 150:
          this.GetTitle(Asda2TitleId.Maniac43);
          break;
      }
      progressRecord.SaveAndFlush();
      this.GetedTitles.SetBit((int) titleId);
      this.Asda2TitlePoints += (int) Asda2TitleTemplate.Templates[(int) titleId].Points;
      Asda2TitlesHandler.SendYouGetNewTitleResponse(this, (short) titleId);
    }

    public bool isTitleGetted(Asda2TitleId titleId)
    {
      return this.GetedTitles.GetBit((int) titleId);
    }

    public bool isTitleDiscovered(Asda2TitleId titleId)
    {
      return this.DiscoveredTitles.GetBit((int) titleId);
    }

    /// <summary>
    /// Reconnects a client to a character that was logging out.
    /// Resends required initial packets.
    /// Called from within the map context.
    /// </summary>
    /// <param name="newClient"></param>
    internal void ReconnectCharacter(IRealmClient newClient)
    {
      this.CancelLogout(false);
      newClient.ActiveCharacter = this;
      this.m_client = newClient;
      this.ClearSelfKnowledge();
      this.OnLogin();
      this.m_lastPlayTimeUpdate = DateTime.Now;
      Character.CharacterLoginHandler loggedIn = Character.LoggedIn;
      if (loggedIn == null)
        return;
      loggedIn(this, false);
    }

    /// <summary>
    /// Enqueues saving of this Character to the IO-Queue.
    /// <see cref="M:WCell.RealmServer.Entities.Character.SaveNow" />
    /// </summary>
    public void SaveLater()
    {
      ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message((Action) (() => this.SaveNow())));
    }

    /// <summary>
    /// Saves the Character to the DB instantly.
    /// Blocking call.
    /// See: <see cref="M:WCell.RealmServer.Entities.Character.SaveLater" />.
    /// When calling this method directly, make sure to set m_saving = true
    /// </summary>
    protected internal bool SaveNow()
    {
      if (!this.m_record.CanSave)
        return false;
      try
      {
        if (this.m_record == null)
          throw new InvalidOperationException("Cannot save Character while not in world.");
        try
        {
          this.UpdatePlayedTime();
          this.m_record.Race = this.Race;
          this.m_record.Class = this.Class;
          this.m_record.Gender = this.Gender;
          this.m_record.Skin = this.Skin;
          this.m_record.Face = this.Facial;
          this.m_record.HairStyle = this.HairStyle;
          this.m_record.HairColor = this.HairColor;
          this.m_record.FacialHair = this.FacialHair;
          this.m_record.Outfit = this.Outfit;
          this.m_record.Name = this.Name;
          this.m_record.Level = this.Level;
          if (this.m_Map != null)
          {
            this.m_record.PositionX = this.Position.X;
            this.m_record.PositionY = this.Position.Y;
            this.m_record.PositionZ = this.Position.Z;
            this.m_record.Orientation = this.Orientation;
            this.m_record.MapId = this.m_Map.Id;
            this.m_record.InstanceId = this.m_Map.InstanceId;
            this.m_record.Zone = this.ZoneId;
          }
          this.m_record.DisplayId = this.DisplayId;
          this.m_record.BindX = this.m_bindLocation.Position.X;
          this.m_record.BindY = this.m_bindLocation.Position.Y;
          this.m_record.BindZ = this.m_bindLocation.Position.Z;
          this.m_record.BindMap = this.m_bindLocation.MapId;
          this.m_record.BindZone = this.m_bindLocation.ZoneId;
          this.m_record.Health = this.Health;
          this.m_record.BaseHealth = this.BaseHealth;
          this.m_record.Power = this.Power;
          this.m_record.BasePower = this.BasePower;
          this.m_record.Money = (long) this.Money;
          this.m_record.WatchedFaction = this.WatchedFaction;
          this.m_record.BaseStrength = this.Asda2BaseStrength;
          this.m_record.BaseStamina = this.Asda2BaseStamina;
          this.m_record.BaseSpirit = this.Asda2BaseSpirit;
          this.m_record.BaseIntellect = this.Asda2BaseIntellect;
          this.m_record.BaseAgility = this.Asda2BaseAgility;
          this.m_record.BaseLuck = this.Asda2BaseLuck;
          this.m_record.Xp = this.Experience;
          this.m_record.RestXp = this.RestXp;
          this.m_record.KillsTotal = this.KillsTotal;
          this.m_record.HonorToday = this.HonorToday;
          this.m_record.HonorYesterday = this.HonorYesterday;
          this.m_record.LifetimeHonorableKills = this.LifetimeHonorableKills;
          this.m_record.HonorPoints = this.HonorPoints;
          this.m_record.ArenaPoints = this.ArenaPoints;
          this.m_record.TitlePoints = (uint) this.Asda2TitlePoints;
          this.m_record.Rank = this.Asda2Rank;
        }
        catch (Exception ex)
        {
          LogUtil.WarnException(ex, string.Format("failed to save pre basic ops, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
        }
        try
        {
          this.PlayerSpells.OnSave();
        }
        catch (Exception ex)
        {
          LogUtil.WarnException(ex, string.Format("failed to save spells, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
        }
        try
        {
          foreach (KeyValuePair<int, Asda2PetRecord> ownedPet in this.OwnedPets)
            ownedPet.Value.Save();
        }
        catch (Exception ex)
        {
          LogUtil.WarnException(ex, string.Format("failed to save pets, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
        }
        try
        {
          foreach (KeyValuePair<byte, Asda2FishingBook> registeredFishingBook in this.RegisteredFishingBooks)
            registeredFishingBook.Value.Save();
        }
        catch (Exception ex)
        {
          LogUtil.WarnException(ex, string.Format("failed to save fishing books, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
        }
      }
      catch (Exception ex)
      {
        this.OnSaveFailed(ex);
        return false;
      }
      try
      {
        try
        {
          this.Account.AccountData.Save();
        }
        catch (Exception ex)
        {
          LogUtil.WarnException(ex, string.Format("failed to save account data, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
        }
        try
        {
          this._asda2Inventory.SaveAll();
        }
        catch (Exception ex)
        {
          LogUtil.WarnException(ex, string.Format("failed to save inventory, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
        }
        try
        {
          if (this.m_auras != null)
            this.m_auras.SaveAurasNow();
        }
        catch (Exception ex)
        {
          LogUtil.WarnException(ex, string.Format("failed to save auras, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
        }
        try
        {
          foreach (FunctionItemBuff functionItemBuff in this.PremiumBuffs.Values.ToArray<FunctionItemBuff>())
          {
            if (functionItemBuff != null)
              functionItemBuff.Save();
          }
        }
        catch (Exception ex)
        {
          LogUtil.WarnException(ex, string.Format("failed to save functional item buffs, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
        }
        try
        {
          foreach (FunctionItemBuff functionItemBuff in ((IEnumerable<FunctionItemBuff>) this.LongTimePremiumBuffs).ToArray<FunctionItemBuff>())
          {
            if (functionItemBuff != null)
              functionItemBuff.Save();
          }
        }
        catch (Exception ex)
        {
          LogUtil.WarnException(ex, string.Format("failed to save long time buffs, character {0} acc {1}[{2}]", (object) this.Name, (object) this.Account.Name, (object) this.AccId), new object[0]);
        }
        this.m_record.LastSaveTime = DateTime.Now;
        this.m_record.Save();
        return true;
      }
      catch (Exception ex)
      {
        this.OnSaveFailed(ex);
        return false;
      }
    }

    private void OnSaveFailed(Exception ex)
    {
      this.SendSystemMessage("Saving failed - Please excuse the inconvenience!");
      LogUtil.ErrorException(ex, "Could not save Character " + (object) this, new object[0]);
    }

    public bool CanLogoutInstantly
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// whether the Logout sequence initialized (Client might already be disconnected)
    /// </summary>
    public bool IsLoggingOut
    {
      get
      {
        return this.m_isLoggingOut;
      }
    }

    /// <summary>
    /// whether the player is currently logging out by itself (not forcefully being logged out).
    /// Players who are forced to logout cannot cancel.
    /// Is false while Client is logged in.
    /// </summary>
    public bool IsPlayerLogout
    {
      get
      {
        return this._isPlayerLogout;
      }
      internal set
      {
        this._isPlayerLogout = value;
      }
    }

    public bool CanLogout
    {
      get
      {
        if (!this.m_IsPinnedDown)
          return !this.IsInCombat;
        return false;
      }
    }

    /// <summary>Enqueues logout of this player to the Map's queue</summary>
    /// <param name="forced">whether the Character is forced to logout (as oppose to initializeing logout oneself)</param>
    public void LogoutLater(bool forced)
    {
      this.AddMessage((Action) (() => this.Logout(forced)));
    }

    /// <summary>
    /// Starts the logout process with the default delay (or instantly if
    /// in city or staff)
    /// Requires map context.
    /// </summary>
    /// <param name="forced"></param>
    public void Logout(bool forced)
    {
      this.Logout(forced, this.CanLogoutInstantly ? 0 : Character.DefaultLogoutDelayMillis);
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
      if (!this.m_isLoggingOut)
      {
        this.m_isLoggingOut = true;
        this.IsPlayerLogout = !forced;
        this.CancelAllActions();
        if (forced)
          ++this.Stunned;
        if (delay <= 0 || forced)
          this.FinishLogout();
        else
          this.m_logoutTimer.Start(delay);
      }
      else
      {
        if (!forced)
          return;
        this.IsPlayerLogout = false;
        if (delay <= 0)
        {
          this.m_logoutTimer.Stop();
          this.FinishLogout();
        }
        else
          this.m_logoutTimer.Start(delay);
      }
    }

    /// <summary>
    /// Cancels logout of this Character.
    /// Requires map context.
    /// </summary>
    public void CancelLogout()
    {
      this.CancelLogout(true);
    }

    /// <summary>
    /// Cancels logout of this Character.
    /// Requires map context.
    /// </summary>
    /// <param name="sendCancelReply">whether to send the Cancel-reply (if client did not disconnect in the meantime)</param>
    public void CancelLogout(bool sendCancelReply)
    {
      if (!this.m_isLoggingOut)
        return;
      if (!this.IsPlayerLogout)
        --this.Stunned;
      this.m_isLoggingOut = false;
      this.IsPlayerLogout = false;
      this.m_logoutTimer.Stop();
      this.DecMechanicCount(SpellMechanic.Frozen, false);
      this.IsSitting = false;
    }

    /// <summary>Saves and then removes Character</summary>
    /// <remarks>Requires map context for synchronization.</remarks>
    internal void FinishLogout()
    {
      ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message((Action) (() =>
      {
        this.Record.LastLogout = new DateTime?(DateTime.Now);
        this.SaveNow();
        if (this.ContextHandler != null)
          this.ContextHandler.AddMessage((Action) (() => this.DoFinishLogout()));
        else
          this.DoFinishLogout();
      })));
    }

    internal void DoFinishLogout()
    {
      if (!this.m_isLoggingOut)
        return;
      try
      {
        if (this.IsInGuild)
          Asda2GuildHandler.SendGuildNotificationResponse(this.Guild, GuildNotificationType.LoggedOut, this.GuildMember);
        if (this.SoulmateCharacter != null)
          Asda2SoulmateHandler.SendSoulmateLoggedOutResponse(this.SoulmateCharacter.Client);
      }
      catch (Exception ex)
      {
        LogUtil.ErrorException("Failed to notify guild or friend about logut {0},{1},{2}", new object[3]
        {
          (object) this.Name,
          (object) this.EntryId,
          (object) ex.Message
        });
      }
      Character.CharacterLogoutHandler loggingOut = Character.LoggingOut;
      if (loggingOut != null)
        loggingOut(this);
      if (!WCell.RealmServer.Global.World.RemoveCharacter(this))
        return;
      this.m_client.ActiveCharacter = (Character) null;
      this.Account.ActiveCharacter = (Character) null;
      this.m_isLoggingOut = false;
      this.RemoveSummonedEntourage();
      this.DetatchFromVechicle();
      for (int index = this.ChatChannels.Count - 1; index >= 0; --index)
        this.ChatChannels[index].Leave((IUser) this, true);
      if (this.Ticket != null)
      {
        this.Ticket.OnOwnerLogout();
        this.Ticket = (Ticket) null;
      }
      if (this.m_TaxiMovementTimer != null)
        this.m_TaxiMovementTimer.Stop();
      if (this.Asda2TradeWindow != null)
        this.Asda2TradeWindow.CancelTrade();
      if (this.PrivateShop != null)
        this.PrivateShop.Exit(this);
      Singleton<GroupMgr>.Instance.OnCharacterLogout(this.m_groupMember);
      Singleton<GuildMgr>.Instance.OnCharacterLogout(this.m_guildMember);
      Singleton<RelationMgr>.Instance.OnCharacterLogout(this);
      InstanceMgr.OnCharacterLogout(this);
      Asda2BattlegroundMgr.OnCharacterLogout(this);
      this.Battlegrounds.OnLogout();
      this.LastLogout = new DateTime?(DateTime.Now);
      if (this.m_corpse != null)
        this.m_corpse.Delete();
      this.CancelAllActions();
      this.m_auras.CleanupAuras();
      this.m_Map.RemoveObjectNow((WorldObject) this);
      if (!this.Account.IsActive)
        this.m_client.Disconnect(false);
      this.m_initialized = false;
      ServerApp<WCell.RealmServer.RealmServer>.Instance.UnregisterAccount(this.Account);
      this.Client.Disconnect(false);
      this.Dispose();
    }

    /// <summary>Kicks this Character with the given msg instantly.</summary>
    /// <remarks>Requires map context.</remarks>
    public void Kick(string msg)
    {
      this.Kick((INamed) null, msg, 0);
    }

    /// <summary>
    /// Kicks this Character with the given msg after the given delay in seconds.
    /// Requires map context.
    /// </summary>
    /// <param name="delay">The delay until the Client should be disconnected in seconds</param>
    public void Kick(string reason, float delay)
    {
      this.Kick(reason, delay);
    }

    /// <summary>
    /// Broadcasts a kick message and then kicks this Character after the default delay.
    /// Requires map context.
    /// </summary>
    public void Kick(Character kicker, string reason)
    {
      this.Kick((INamed) kicker, reason, Character.DefaultLogoutDelayMillis);
    }

    /// <summary>
    /// Broadcasts a kick message and then kicks this Character after the default delay.
    /// Requires map context.
    /// </summary>
    public void Kick(INamed kicker, string reason, int delay)
    {
      string str = (kicker != null ? " by " + kicker.Name : "") + (!string.IsNullOrEmpty(reason) ? " (" + reason + ")" : ".");
      WCell.RealmServer.Global.World.Broadcast("{0} has been kicked{1}", new object[2]
      {
        (object) this.Name,
        (object) str
      });
      this.SendSystemMessage("You have been kicked" + str);
      this.CancelTaxiFlight();
      this.Logout(true, delay);
    }

    /// <summary>Performs any needed object/object pool cleanup.</summary>
    public override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      this.CancelSummon(false);
      if (this.m_bgInfo != null)
      {
        this.m_bgInfo.Character = (Character) null;
        this.m_bgInfo = (BattlegroundInfo) null;
      }
      this.m_InstanceCollection = (InstanceCollection) null;
      if (this.m_activePet != null)
      {
        this.m_activePet.Delete();
        this.m_activePet = (NPC) null;
      }
      this.m_minions = (NPCCollection) null;
      this.m_activePet = (NPC) null;
      if (this.m_skills != null)
      {
        this.m_skills.m_owner = (Character) null;
        this.m_skills = (SkillCollection) null;
      }
      if (this.m_talents != null)
      {
        this.m_talents.Owner = (Unit) null;
        this.m_talents = (TalentCollection) null;
      }
      this._asda2Inventory = (Asda2PlayerInventory) null;
      if (this.m_mailAccount != null)
      {
        this.m_mailAccount.Owner = (Character) null;
        this.m_mailAccount = (MailAccount) null;
      }
      this.m_groupMember = (GroupMember) null;
      if (this.m_reputations != null)
      {
        this.m_reputations.Owner = (Character) null;
        this.m_reputations = (ReputationCollection) null;
      }
      if (this.m_InstanceCollection != null)
        this.m_InstanceCollection.Dispose();
      if (this.m_achievements != null)
      {
        this.m_achievements.m_owner = (Character) null;
        this.m_achievements = (AchievementCollection) null;
      }
      if (this.m_CasterReference != null)
      {
        this.m_CasterReference.Object = (WorldObject) null;
        this.m_CasterReference = (ObjectReference) null;
      }
      if (this.m_looterEntry != null)
      {
        this.m_looterEntry.m_owner = (Character) null;
        this.m_looterEntry = (Asda2LooterEntry) null;
      }
      if (this.m_ExtraInfo != null)
      {
        this.m_ExtraInfo.Dispose();
        this.m_ExtraInfo = (ExtraInfo) null;
      }
      this.KnownObjects.Clear();
      WorldObject.WorldObjectSetPool.Recycle(this.KnownObjects);
    }

    /// <summary>
    /// Throws an exception, since logged in Characters may not be deleted
    /// </summary>
    protected internal override void DeleteNow()
    {
      this.Client.Disconnect(false);
    }

    /// <summary>
    /// Throws an exception, since logged in Characters may not be deleted
    /// </summary>
    public override void Delete()
    {
      this.Client.Disconnect(false);
    }

    public string TryAddStatPoints(Asda2StatType statType, int points)
    {
      if (this.FreeStatPoints <= 0)
        return "Sorry, but you have not free stat points.";
      if (points <= 0 || points > this.FreeStatPoints)
        return string.Format("You must enter stat points count from {0} to {1}, but you enter {2}. Failed to increace {3}", (object) 1, (object) this.FreeStatPoints, (object) points, (object) statType);
      this.FreeStatPoints -= points;
      Log.Create(Log.Types.StatsOperations, LogSourceType.Character, this.EntryId).AddAttribute("source", 0.0, "add_stat_points").AddAttribute("amount", (double) points, "").AddAttribute("free", (double) this.FreeStatPoints, "").AddAttribute("stat", (double) statType, statType.ToString()).Write();
      switch (statType)
      {
        case Asda2StatType.Strength:
          this.Asda2BaseStrength += points;
          this.UpdateAsda2Strength();
          break;
        case Asda2StatType.Dexterity:
          this.Asda2BaseAgility += points;
          this.UpdateAsda2Agility();
          break;
        case Asda2StatType.Stamina:
          this.Asda2BaseStamina += points;
          this.UpdateAsda2Stamina();
          break;
        case Asda2StatType.Luck:
          this.Asda2BaseLuck += points;
          this.UpdateAsda2Luck();
          break;
        case Asda2StatType.Intelect:
          this.Asda2BaseIntellect += points;
          this.UpdateAsda2Intellect();
          break;
        case Asda2StatType.Spirit:
          this.Asda2BaseSpirit += points;
          this.UpdateAsda2Spirit();
          break;
      }
      Asda2CharacterHandler.SendUpdateStatsResponse(this.Client);
      Asda2CharacterHandler.SendUpdateStatsOneResponse(this.Client);
      return string.Format("Succeful increase {0}. Now you have {1} free stat points.", (object) statType, (object) this.FreeStatPoints);
    }

    public void ResetStatPoints()
    {
      this.Asda2BaseStrength = 1;
      this.Asda2BaseIntellect = 1;
      this.Asda2BaseAgility = 1;
      this.Asda2BaseSpirit = 1;
      this.Asda2BaseStamina = 1;
      this.Asda2BaseLuck = 1;
      this.FreeStatPoints = CharacterFormulas.CalculateFreeStatPointForLevel(this.Level, this.Record.RebornCount);
      Log.Create(Log.Types.StatsOperations, LogSourceType.Character, this.EntryId).AddAttribute("source", 0.0, "reset_stat_points").AddAttribute("free", (double) this.FreeStatPoints, "").Write();
      this.UpdateAsda2Agility();
      this.UpdateAsda2Strength();
      this.UpdateAsda2Stamina();
      this.UpdateAsda2Luck();
      this.UpdateAsda2Spirit();
      this.UpdateAsda2Intellect();
      Asda2CharacterHandler.SendUpdateStatsResponse(this.Client);
      Asda2CharacterHandler.SendUpdateStatsOneResponse(this.Client);
    }

    public static uint CharacterIdFromAccIdAndCharNum(int targetAccId, short targetCharNumOnAcc)
    {
      return (uint) (targetAccId + 1000000 * (int) targetCharNumOnAcc);
    }

    public bool IsRussianClient { get; set; }

    public bool IsFromFriendDamageBonusApplied { get; set; }

    public bool IsSoulmateEmpowerPositive { get; set; }

    public bool IsSoulSongEnabled { get; set; }

    public DateTime SoulmateSongEndTime { get; set; }

    public void AddSoulmateSong()
    {
      this.SoulmateSongEndTime = DateTime.Now.AddMinutes(30.0);
      if (this.IsSoulSongEnabled)
        return;
      this.IsSoulSongEnabled = true;
      this.SendInfoMsg("You feeling soulmate song effect !!!");
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
      Asda2CharacterHandler.SendUpdateStatsOneResponse(this.Client);
      Asda2CharacterHandler.SendUpdateStatsResponse(this.Client);
    }

    public void RemoveSoulmateSong()
    {
      if (!this.IsSoulSongEnabled)
        return;
      this.IsSoulSongEnabled = false;
      this.SendInfoMsg("Soulmate song effect removed.");
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
      Asda2CharacterHandler.SendUpdateStatsOneResponse(this.Client);
      Asda2CharacterHandler.SendUpdateStatsResponse(this.Client);
    }

    public void AddFriendEmpower(bool positive)
    {
      this.SoulmateEmpowerEndTime = DateTime.Now.AddMinutes(5.0);
      if (this.IsSoulmateEmpowerEnabled)
      {
        this.SendInfoMsg("Soulmate empower duration updated.");
      }
      else
      {
        this.IsSoulmateEmpowerEnabled = true;
        this.IsSoulmateEmpowerPositive = positive;
        if (this.IsSoulmateEmpowerPositive)
        {
          this.SendInfoMsg("You feeling positive soulmate empower effect.");
          this.ChangeModifier(StatModifierFloat.Damage, CharacterFormulas.FriendEmpowerDamageBonus);
          this.ChangeModifier(StatModifierFloat.MagicDamage, CharacterFormulas.FriendEmpowerDamageBonus);
        }
        else
        {
          this.ChangeModifier(StatModifierFloat.Damage, (float) (-(double) CharacterFormulas.FriendEmpowerDamageBonus * 2.0));
          this.ChangeModifier(StatModifierFloat.MagicDamage, (float) (-(double) CharacterFormulas.FriendEmpowerDamageBonus * 2.0));
          this.SendInfoMsg("You feeling negative soulmate empower effect.");
        }
        Asda2CharacterHandler.SendUpdateStatsOneResponse(this.Client);
        Asda2CharacterHandler.SendUpdateStatsResponse(this.Client);
      }
    }

    public void RemoveFriendEmpower()
    {
      if (!this.IsSoulmateEmpowerEnabled)
        return;
      this.SendInfoMsg("Soulmate empower effect removed.");
      this.IsSoulmateEmpowerEnabled = false;
      if (this.IsSoulmateEmpowerPositive)
      {
        this.ChangeModifier(StatModifierFloat.Damage, -CharacterFormulas.FriendEmpowerDamageBonus);
        this.ChangeModifier(StatModifierFloat.MagicDamage, -CharacterFormulas.FriendEmpowerDamageBonus);
      }
      else
      {
        this.ChangeModifier(StatModifierFloat.Damage, CharacterFormulas.FriendEmpowerDamageBonus * 2f);
        this.ChangeModifier(StatModifierFloat.MagicDamage, CharacterFormulas.FriendEmpowerDamageBonus * 2f);
      }
      Asda2CharacterHandler.SendUpdateStatsOneResponse(this.Client);
      Asda2CharacterHandler.SendUpdateStatsResponse(this.Client);
    }

    public void AddFromFriendDamageBonus()
    {
      if (this.IsFromFriendDamageBonusApplied)
        return;
      this.IsFromFriendDamageBonusApplied = true;
      this.ChangeModifier(StatModifierFloat.Damage, CharacterFormulas.NearFriendDamageBonus);
      this.ChangeModifier(StatModifierFloat.MagicDamage, CharacterFormulas.NearFriendDamageBonus);
      this.ChangeModifier(StatModifierFloat.Asda2MagicDefence, CharacterFormulas.NearFriendDeffenceBonus);
      this.ChangeModifier(StatModifierFloat.Asda2Defence, CharacterFormulas.NearFriendDeffenceBonus);
      this.ChangeModifier(StatModifierFloat.Speed, CharacterFormulas.NearFriendSpeedBonus);
      Asda2CharacterHandler.SendUpdateStatsOneResponse(this.Client);
      Asda2CharacterHandler.SendUpdateStatsResponse(this.Client);
    }

    public void RemoveFromFriendDamageBonus()
    {
      if (!this.IsFromFriendDamageBonusApplied)
        return;
      this.IsFromFriendDamageBonusApplied = false;
      this.ChangeModifier(StatModifierFloat.Damage, -CharacterFormulas.NearFriendDamageBonus);
      this.ChangeModifier(StatModifierFloat.MagicDamage, -CharacterFormulas.NearFriendDamageBonus);
      this.ChangeModifier(StatModifierFloat.Asda2MagicDefence, -CharacterFormulas.NearFriendDeffenceBonus);
      this.ChangeModifier(StatModifierFloat.Asda2Defence, -CharacterFormulas.NearFriendDeffenceBonus);
      this.ChangeModifier(StatModifierFloat.Speed, -CharacterFormulas.NearFriendSpeedBonus);
      Asda2CharacterHandler.SendUpdateStatsOneResponse(this.Client);
      Asda2CharacterHandler.SendUpdateStatsResponse(this.Client);
    }

    public void RemovaAllSoulmateBonuses()
    {
      this.RemoveFriendEmpower();
      this.RemoveFromFriendDamageBonus();
      this.RemoveSoulmateSong();
      this.IsSoulmateSoulSaved = false;
    }

    /// <summary>
    /// Is called when the Player logs in or reconnects to a Character that was logged in before and not logged out yet (due to logout delay).
    /// </summary>
    public static event Character.CharacterLoginHandler LoggedIn;

    /// <summary>
    /// Is called right befrore the Character is disposed and removed.
    /// </summary>
    public static event Character.CharacterLogoutHandler LoggingOut;

    /// <summary>
    /// Is called when the given newly created Character logs in the first time.
    /// </summary>
    public static event Action<Character> Created;

    /// <summary>Is called when the given Character gains a new Level.</summary>
    public static event Action<Character> LevelChanged;

    protected override UpdateFieldCollection _UpdateFieldInfos
    {
      get
      {
        return Character.UpdateFieldInfos;
      }
    }

    public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
    {
      get
      {
        return UpdateFieldHandler.DynamicPlayerHandlers;
      }
    }

    public Unit Observing
    {
      get
      {
        return this.observing ?? (Unit) this;
      }
      set
      {
        this.observing = value;
      }
    }

    /// <summary>
    /// Will be executed by the current map we are currently in or enqueued and executed,
    /// once we re-enter a map
    /// </summary>
    public void AddPostUpdateMessage(Action action)
    {
      this.m_environmentQueue.Enqueue(action);
    }

    public HashSet<Character> Observers
    {
      get
      {
        return this.m_observers;
      }
    }

    internal void AddItemToUpdate(Item item)
    {
      this.m_itemsRequiringUpdates.Add(item);
    }

    /// <summary>
    /// Removes the given item visually from the Client.
    /// Do not call this method - but use Item.Remove instead.
    /// </summary>
    internal void RemoveOwnedItem(Item item)
    {
      this.m_itemsRequiringUpdates.Remove(item);
      this.m_environmentQueue.Enqueue((Action) (() =>
      {
        item.SendDestroyToPlayer(this);
        if (this.m_observers == null)
          return;
        foreach (Character observer in this.m_observers)
          item.SendDestroyToPlayer(observer);
      }));
    }

    /// <summary>Resends all updates of everything</summary>
    public void ResetOwnWorld()
    {
      MovementHandler.SendNewWorld(this.Client, this.MapId, ref this.m_position, this.Orientation);
      this.ClearSelfKnowledge();
    }

    /// <summary>
    /// Clears known objects and leads to resending of the creation packet
    /// during the next Map-Update.
    /// This is only needed for teleporting or body-transfer.
    /// Requires map context.
    /// </summary>
    internal void ClearSelfKnowledge()
    {
      this.KnownObjects.Clear();
      this.NearbyObjects.Clear();
      if (this.m_observers == null)
        return;
      this.m_observers.Clear();
    }

    /// <summary>Will resend update packet of the given object</summary>
    public void InvalidateKnowledgeOf(WorldObject obj)
    {
      this.KnownObjects.Remove(obj);
      this.NearbyObjects.Remove(obj);
      obj.SendDestroyToPlayer(this);
    }

    /// <summary>
    /// Whether the given Object is visible to (and thus in broadcast-range of) this Character
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool KnowsOf(WorldObject obj)
    {
      return this.KnownObjects.Contains(obj);
    }

    /// <summary>Collects all update-masks from nearby objects</summary>
    internal void UpdateEnvironment(HashSet<WorldObject> updatedObjects)
    {
      HashSet<WorldObject> toRemove = WorldObject.WorldObjectSetPool.Obtain();
      toRemove.AddRange<WorldObject>((IEnumerable<WorldObject>) this.KnownObjects);
      toRemove.Remove((WorldObject) this);
      this.NearbyObjects.Clear();
      if (this.m_initialized)
      {
        this.Observing.IterateEnvironment(WorldObject.BroadcastRange, (Func<WorldObject, bool>) (obj =>
        {
          if (this.Client == null || this.Client.ActiveCharacter == null)
          {
            if (this.Client == null)
              LogUtil.WarnException("Client is null. removeing from map and world? {0}[{1}]", new object[2]
              {
                (object) this.Name,
                (object) this.AccId
              });
            else if (this.Client.ActiveCharacter == null)
              LogUtil.WarnException("Client.ActiveCharacter is null. removeing from map and world? {0}[{1}]", new object[2]
              {
                (object) this.Name,
                (object) this.AccId
              });
            if (this.Map != null)
              this.Map.AddMessage((Action) (() =>
              {
                this.Map.RemoveObject((WorldObject) this);
                WCell.RealmServer.Global.World.RemoveCharacter(this);
              }));
            return false;
          }
          if (!this.Observing.IsInPhase(obj) || obj is GameObject && (double) obj.GetDistance((WorldObject) this) > (double) WorldObject.BroadcastRangeNpc)
            return true;
          this.NearbyObjects.Add(obj);
          if (!this.Observing.CanSee(obj) && !object.ReferenceEquals((object) obj, (object) this))
            return true;
          if (!this.KnownObjects.Contains(obj))
          {
            Character visibleChr = obj as Character;
            if (visibleChr != null && visibleChr != this)
            {
              GlobalHandler.SendCharacterVisibleNowResponse(this.Client, visibleChr);
              this.Map.CallDelayed(200, (Action) (() =>
              {
                if (visibleChr.Asda2Pet != null)
                  GlobalHandler.SendCharacterInfoPetResponse(this.Client, visibleChr);
                if (visibleChr.IsAsda2TradeDescriptionEnabled)
                  Asda2PrivateShopHandler.SendtradeStatusTextWindowResponseToOne(visibleChr, this.Client);
                GlobalHandler.SendCharacterPlaceInTitleRatingResponse(this.Client, visibleChr);
                GlobalHandler.SendBuffsOnCharacterInfoResponse(this.Client, visibleChr);
                if (visibleChr.IsInGuild)
                  GlobalHandler.SendCharacterInfoClanNameResponse(this.Client, visibleChr);
                GlobalHandler.SendCharacterFactionAndFactionRankResponse(this.Client, visibleChr);
                GlobalHandler.SendCharacterFriendShipResponse(this.Client, visibleChr);
                if (visibleChr.ChatRoom != null)
                  ChatMgr.SendChatRoomVisibleResponse(visibleChr, ChatRoomVisibilityStatus.Visible, visibleChr.ChatRoom, this);
                this.CheckAtackStateWithCharacter(visibleChr);
                if (visibleChr.Asda2WingsItemId != (short) -1)
                  FunctionalItemsHandler.SendWingsInfoResponse(visibleChr, this.Client);
                if (visibleChr.TransformationId == (short) -1)
                  return;
                GlobalHandler.SendTransformToPetResponse(visibleChr, true, this.Client);
              }));
              if (visibleChr.IsOnTransport)
                this.Map.CallDelayed(400, (Action) (() => FunctionalItemsHandler.SendShopItemUsedResponse(this.Client, visibleChr, int.MaxValue)));
              if (visibleChr.IsOnMount)
                this.Map.CallDelayed(500, (Action) (() => Asda2MountHandler.SendCharacterOnMountStatusChangedToPneClientResponse(this.Client, visibleChr)));
            }
            else
            {
              NPC visibleNpc = obj as NPC;
              if (visibleNpc != null && visibleNpc.IsAlive)
              {
                GlobalHandler.SendMonstrVisibleNowResponse(this.Client, visibleNpc);
              }
              else
              {
                GameObject npc = obj as GameObject;
                if (npc != null && npc.GoId != GOEntryId.Portal)
                {
                  if (!this.IsAsda2BattlegroundInProgress || this.CurrentBattleGround.WarType != Asda2BattlegroundType.Deathmatch || this.MapId != MapId.BatleField)
                    GlobalHandler.SendNpcVisiableNowResponse(this.Client, npc);
                }
                else
                {
                  Asda2Loot loot = obj as Asda2Loot;
                  if (loot != null)
                    GlobalHandler.SendItemVisible(this, loot);
                }
              }
            }
            this.OnEncountered(obj);
          }
          toRemove.Remove(obj);
          return true;
        }));
        if (this.m_groupMember != null)
          this.m_groupMember.Group.UpdateOutOfRangeMembers(this.m_groupMember);
        foreach (WorldObject worldObject in toRemove)
          this.OnOutOfRange(worldObject);
      }
      Action action;
      while (this.m_environmentQueue.TryDequeue(out action))
        this.AddMessage(action);
      if (this.m_restTrigger != null)
        this.UpdateRestState();
      toRemove.Clear();
      WorldObject.WorldObjectSetPool.Recycle(toRemove);
    }

    private void CheckAtackStateWithCharacter(Character visibleChr)
    {
      if (this.MayAttack((IFactionMember) visibleChr))
      {
        this.EnemyCharacters.Add(visibleChr);
        this.Map.CallDelayed(800, (Action) (() =>
        {
          if (this.IsAsda2BattlegroundInProgress)
            return;
          GlobalHandler.SendFightingModeChangedResponse(this.Client, this.SessionId, (int) this.AccId, visibleChr.SessionId);
        }));
      }
      else
      {
        if (!this.EnemyCharacters.Contains(visibleChr))
          return;
        this.EnemyCharacters.Remove(visibleChr);
        this.CheckEnemysCount();
      }
    }

    /// <summary>
    /// Check if this Character is still resting (if it was resting before)
    /// </summary>
    private void UpdateRestState()
    {
      if (this.m_restTrigger.IsInArea(this))
        return;
      this.RestTrigger = (AreaTrigger) null;
    }

    /// <summary>
    /// Sends Item-information and Talents to the given Character and keeps them updated until they
    /// are out of range.
    /// </summary>
    /// <param name="chr"></param>
    public override UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
    {
      if (chr == this)
        return UpdateFieldFlags.Public | UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.GroupOnly;
      return base.GetUpdateFieldVisibilityFor(chr);
    }

    protected override UpdateType GetCreationUpdateType(UpdateFieldFlags flags)
    {
      return !flags.HasAnyFlag(UpdateFieldFlags.Private) ? UpdateType.Create : UpdateType.CreateSelf;
    }

    public void PushFieldUpdate(UpdateFieldId field, uint value)
    {
      if (!this.IsInWorld)
      {
        this.SetUInt32(field, value);
      }
      else
      {
        using (UpdatePacket fieldUpdatePacket = this.GetFieldUpdatePacket(field, value))
          ObjectBase.SendUpdatePacket(this, fieldUpdatePacket);
      }
    }

    public void PushFieldUpdate(UpdateFieldId field, EntityId value)
    {
    }

    public override void Update(int dt)
    {
      base.Update(dt);
      if (this.m_isLoggingOut)
        this.m_logoutTimer.Update(dt);
      if (!this.IsMoving && this.LastSendIamNotMoving < (uint) Environment.TickCount)
      {
        this.LastSendIamNotMoving = (uint) (Environment.TickCount + CharacterFormulas.TimeBetweenImNotMovingPacketSendMillis);
        Asda2MovmentHandler.SendStartMoveCommonToAreaResponse(this, true, false);
      }
      Asda2MovmentHandler.CalculateAndSetRealPos(this, dt);
      if (this.Asda2Pet != null)
      {
        if (this.LastPetExpGainTime < (uint) Environment.TickCount)
        {
          this.Asda2Pet.GainXp(1);
          this.LastPetExpGainTime = (uint) (Environment.TickCount + (int) CharacterFormulas.TimeBetweenPetExpGainSecs * 1000);
        }
        if (!this.PetNotHungerEnabled && this.LastPetEatingTime < (uint) Environment.TickCount)
        {
          if (this.Asda2Pet.HungerPrc == (byte) 1)
          {
            Asda2PetHandler.SendPetGoesSleepDueStarvationResponse(this.Client, this.Asda2Pet);
            this.Asda2Pet.RemoveStatsFromOwner();
            this.Asda2Pet.HungerPrc = (byte) 0;
            this.Asda2Pet = (Asda2PetRecord) null;
            GlobalHandler.UpdateCharacterPetInfoToArea(this);
          }
          else
          {
            --this.Asda2Pet.HungerPrc;
            this.LastPetEatingTime = (uint) (Environment.TickCount + (int) CharacterFormulas.TimeBetweenPetEatingsSecs * 1000);
          }
        }
      }
      if (this.PremiumBuffs.Count > 0)
      {
        foreach (FunctionItemBuff record in this.PremiumBuffs.Values)
        {
          if (record.Duration < (long) dt)
          {
            this.ProcessFunctionalItemEffect(record, false);
            this.CategoryBuffsToDelete.Add(record.Template.Category);
            record.DeleteLater();
          }
          else
            record.Duration -= (long) dt;
        }
      }
      foreach (FunctionItemBuff longTimePremiumBuff in this.LongTimePremiumBuffs)
      {
        if (longTimePremiumBuff != null && longTimePremiumBuff.EndsDate < DateTime.Now)
        {
          this.ProcessFunctionalItemEffect(longTimePremiumBuff, false);
          this.CategoryBuffsToDelete.Add(longTimePremiumBuff.Template.Category);
          longTimePremiumBuff.DeleteLater();
        }
      }
      if (this.CategoryBuffsToDelete.Count > 0)
      {
        foreach (Asda2ItemCategory key in this.CategoryBuffsToDelete)
        {
          this.PremiumBuffs.Remove(key);
          for (int index = 0; index < this.LongTimePremiumBuffs.Length; ++index)
          {
            if (this.LongTimePremiumBuffs[index] != null && this.LongTimePremiumBuffs[index].Template.Category == key)
            {
              this.LongTimePremiumBuffs[index] = (FunctionItemBuff) null;
              break;
            }
          }
        }
        this.CategoryBuffsToDelete.Clear();
      }
      List<Asda2PereodicActionType> pereodicActionTypeList = new List<Asda2PereodicActionType>();
      foreach (KeyValuePair<Asda2PereodicActionType, PereodicAction> pereodicAction in this.PereodicActions)
      {
        pereodicAction.Value.Update(dt);
        if (pereodicAction.Value.CallsNum <= 0)
          pereodicActionTypeList.Add(pereodicAction.Key);
      }
      foreach (Asda2PereodicActionType key in pereodicActionTypeList)
        this.PereodicActions.Remove(key);
      if (this.SoulmateRecord != null)
        this.SoulmateRecord.OnUpdateTick();
      DateTime? banChatTill = this.BanChatTill;
      DateTime now = DateTime.Now;
      if ((banChatTill.HasValue ? (banChatTill.GetValueOrDefault() < now ? 1 : 0) : 0) == 0)
        return;
      this.BanChatTill = new DateTime?();
      this.ChatBanned = false;
      this.SendInfoMsg("Chat is unbanned.");
    }

    public override UpdatePriority UpdatePriority
    {
      get
      {
        return UpdatePriority.HighPriority;
      }
    }

    private void UpdateSettings()
    {
      if (this.SettingsFlags == null)
        return;
      for (int index = 0; index < this.SettingsFlags.Length; ++index)
      {
        bool flag = this.SettingsFlags[index] == (byte) 1;
        switch (index)
        {
          case 5:
            this.EnableWishpers = flag;
            break;
          case 7:
            this.EnableSoulmateRequest = flag;
            break;
          case 8:
            this.EnableFriendRequest = flag;
            break;
          case 9:
            this.EnablePartyRequest = flag;
            break;
          case 10:
            this.EnableGuildRequest = flag;
            break;
          case 11:
            this.EnableGeneralTradeRequest = flag;
            break;
          case 12:
            this.EnableGearTradeRequest = flag;
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
