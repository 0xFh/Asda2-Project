using Castle.ActiveRecord;
using Castle.ActiveRecord.Queries;
using NHibernate.Criterion;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Mail;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Talents;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Database
{
  [ActiveRecord(Access = PropertyAccess.Property)]
  public class CharacterRecord : WCellRecord<CharacterRecord>, ILivingEntity, INamedEntity, IEntity, INamed, IMapId,
    IActivePetSettings
  {
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
    public static readonly CharacterRecord[] EmptyArray = new CharacterRecord[0];

    protected static readonly NHIdGenerator _idGenerator =
      new NHIdGenerator(typeof(CharacterRecord), nameof(Guid), 1000L);

    private BattlegroundSide m_BattlegroundTeam = BattlegroundSide.End;

    /// <summary>
    /// Character will not have Ids below this threshold.
    /// You can use those unused ids for self-implemented mechanisms, eg to fake participants in chat-channels etc.
    /// </summary>
    /// <remarks>
    /// Do not change this value once the first Character exists.
    /// If you want to change this value to reserve more (or less) ids for other use, make sure
    /// that none of the ids below this threshold are in the DB.
    /// </remarks>
    public const long LowestCharId = 1000;

    [Field("DisplayId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int _displayId;

    [Field("WatchedFaction", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int _watchedFaction;

    [Field("ClassId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int m_Class;

    [Field("Map", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int m_Map;

    [Field("CorpseMap", Access = PropertyAccess.FieldCamelcase)]
    private int m_CorpseMap;

    [Field("Zone", Access = PropertyAccess.FieldCamelcase)]
    private int m_zoneId;

    [Field("BindZone", Access = PropertyAccess.FieldCamelcase)]
    private int m_BindZone;

    [Field("BindMap", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int m_BindMap;

    private DateTime? m_lastLogin;

    [Field("GuildId", Access = PropertyAccess.FieldCamelcase)]
    private int m_GuildId;

    [Field("SummonSpell")]private int m_SummonSpellId;
    [Field("PetEntryId")]private int m_PetEntryId;

    [Field("TalentResetPriceTier", NotNull = true)]
    private int _talentResetPriceTier;

    [Field("KillsTotal", NotNull = true)]private int _killsTotal;
    [Field("HonorToday", NotNull = true)]private int _honorToday;

    [Field("HonorYesterday", NotNull = true)]
    private int _honorYesterday;

    [Field("LifetimeHonorableKills", NotNull = true)]
    private int _lifetimeHonorableKills;

    [Field("HonorPoints", NotNull = true)]private int _honorPoints;
    [Field("ArenaPoints", NotNull = true)]private int _arenaPoints;
    [Field("TitlePoints", NotNull = true)]private int _titlePoints;
    [Field("Rank", NotNull = true)]private int _rank;
    private ICollection<Asda2ItemRecord> _asda2LoadedItems;
    private ICollection<Asda2FastItemSlotRecord> _asda2LoadedFastItemSlots;

    /// <summary>Returns the next unique Id for a new Character</summary>
    public static uint NextId()
    {
      return (uint) _idGenerator.Next();
    }

    /// <summary>
    /// Creates a new CharacterRecord row in the database with the given information.
    /// </summary>
    /// <param name="account">the account this character is on</param>
    /// <param name="name">the name of the new character</param>
    /// <returns>the <seealso cref="T:WCell.RealmServer.Database.CharacterRecord" /> object</returns>
    public static CharacterRecord CreateNewCharacterRecord(RealmAccount account, string name)
    {
      try
      {
        return new CharacterRecord(account.AccountId)
        {
          EntityLowId = (uint) _idGenerator.Next(),
          Name = name,
          Created = DateTime.Now
        };
      }
      catch(Exception ex)
      {
        s_log.Error(
          "Character creation error (DBS: " + RealmServerConfiguration.DatabaseType + "): ", ex);
        return null;
      }
    }

    /// <summary>
    /// Retrieves a CharacterRecord based on the character name
    /// </summary>
    /// <param name="name">the character name</param>
    /// <returns>the corresponding <seealso cref="T:WCell.RealmServer.Database.CharacterRecord" /></returns>
    public static CharacterRecord GetRecordByName(string name)
    {
      try
      {
        return FindOne((ICriterion) Restrictions.Like("Name", name));
      }
      catch(Exception ex)
      {
        RealmDBMgr.OnDBError(ex);
        return null;
      }
    }

    /// <summary>
    /// Checks if a character with the given name already exists.
    /// </summary>
    /// <param name="characterName">the name to check for</param>
    /// <returns>true if the character exists; false otherwise</returns>
    public static bool Exists(string characterName)
    {
      try
      {
        return ActiveRecordBase<CharacterRecord>.Exists((ICriterion) Restrictions.Like("Name", characterName));
      }
      catch(Exception ex)
      {
        RealmDBMgr.OnDBError(ex);
        return false;
      }
    }

    /// <summary>
    /// Checks if a character with the given Id already exists.
    /// </summary>
    public static bool Exists(uint entityLowId)
    {
      try
      {
        return ActiveRecordBase<CharacterRecord>.Exists((ICriterion) Restrictions.Eq("Guid", (long) entityLowId));
      }
      catch(Exception ex)
      {
        RealmDBMgr.OnDBError(ex);
        return false;
      }
    }

    /// <summary>
    /// Retrieves a CharacterRecord based on a character's entity ID
    /// </summary>
    /// <param name="lowUid">the character unique ID</param>
    /// 		/// <returns>the corresponding <seealso cref="T:WCell.RealmServer.Database.CharacterRecord" /></returns>
    public static CharacterRecord LoadRecordByEntityId(uint lowUid)
    {
      return FindOne((ICriterion) Restrictions.Eq("Guid", (long) lowUid));
    }

    /// <summary>
    /// Retrieves a CharacterRecord based on a character's entity ID.
    /// </summary>
    /// <returns>the corresponding <seealso cref="T:WCell.RealmServer.Database.CharacterRecord" /></returns>
    public static CharacterRecord LoadRecordByID(long guid)
    {
      return FindOne((ICriterion) Restrictions.Eq("CharacterId", guid));
    }

    public static int GetCount()
    {
      return Count();
    }

    protected CharacterRecord()
    {
      CanSave = true;
      AbilitySpells = new List<SpellRecord>();
    }

    public CharacterRecord(int accountId)
      : this()
    {
      State = RecordState.New;
      JustCreated = true;
      AccountId = accountId;
      ExploredZones = new byte[UpdateFieldMgr.ExplorationZoneFieldSize * 4];
    }

    public virtual Character CreateCharacter()
    {
      return new Character();
    }

    public bool JustCreated { get; internal set; }

    /// <summary>Whether this record should be saved to DB</summary>
    public bool CanSave { get; set; }

    public DateTime LastSaveTime { get; internal set; }

    [PrimaryKey(PrimaryKeyType.Assigned, "EntityLowId")]
    public long Guid { get; set; }

    [Property(NotNull = true)]
    public int AccountId { get; set; }

    public uint EntityLowId
    {
      get { return (uint) Guid; }
      set { Guid = (int) value; }
    }

    public EntityId EntityId
    {
      get { return EntityId.GetPlayerId(EntityLowId); }
    }

    [Property(Length = 12, NotNull = true, Unique = true)]
    public string Name { get; set; }

    [Property(NotNull = true)]
    public DateTime Created { get; set; }

    [Property]
    public DateTime? BanChatTill { get; set; }

    /// <summary>
    /// Whether the Character that this Record belongs to is currently logged in.
    /// </summary>
    [Property(Access = PropertyAccess.ReadOnly)]
    public bool IsOnline
    {
      get
      {
        if(LastLogin.HasValue)
        {
          DateTime? lastLogin1 = LastLogin;
          DateTime startTime = ServerApp<RealmServer>.StartTime;
          if((lastLogin1.HasValue ? (lastLogin1.GetValueOrDefault() > startTime ? 1 : 0) : 0) != 0)
          {
            if(!LastLogout.HasValue)
              return true;
            DateTime? lastLogout = LastLogout;
            DateTime? lastLogin2 = LastLogin;
            if(!(lastLogout.HasValue & lastLogin2.HasValue))
              return false;
            return lastLogout.GetValueOrDefault() < lastLogin2.GetValueOrDefault();
          }
        }

        return false;
      }
    }

    [Property]
    public DateTime? LastLogin
    {
      get { return m_lastLogin; }
      set
      {
        m_lastLogin = value;
        if(m_lastLogin.HasValue)
          return;
        JustCreated = true;
      }
    }

    [Property]
    public DateTime? LastLogout { get; set; }

    [Property(NotNull = true)]
    public CharEnumFlags CharacterFlags { get; set; }

    [Property(NotNull = true)]
    public RaceId Race { get; set; }

    public ClassId Class
    {
      get { return (ClassId) m_Class; }
      set { m_Class = (int) value; }
    }

    [Property(NotNull = true)]
    public GenderType Gender { get; set; }

    [Property(NotNull = true)]
    public byte Skin { get; set; }

    [Property("face", NotNull = true)]
    public byte Face { get; set; }

    [Property(NotNull = true)]
    public byte HairStyle { get; set; }

    [Property(NotNull = true)]
    public byte HairColor { get; set; }

    [Property(NotNull = true)]
    public byte FacialHair { get; set; }

    [Property(NotNull = true)]
    public byte Outfit { get; set; }

    [Property(NotNull = true)]
    public int Level { get; set; }

    [Property]
    public int Xp { get; set; }

    public int WatchedFaction
    {
      get { return _watchedFaction; }
      set { _watchedFaction = value; }
    }

    public uint DisplayId
    {
      get { return (uint) _displayId; }
      set { _displayId = (int) value; }
    }

    [Property(NotNull = true)]
    public int TotalPlayTime { get; set; }

    [Property(NotNull = true)]
    public int LevelPlayTime { get; set; }

    [Property(ColumnType = "BinaryBlob", Length = 32, NotNull = true)]
    public byte[] TutorialFlags { get; set; }

    [Property(ColumnType = "BinaryBlob")]
    public byte[] ExploredZones { get; set; }

    [Property(NotNull = true)]
    public float PositionX { get; set; }

    [Property(NotNull = true)]
    public float PositionY { get; set; }

    [Property(NotNull = true)]
    public float PositionZ { get; set; }

    [Property(NotNull = true)]
    public float Orientation { get; set; }

    public MapId MapId
    {
      get { return (MapId) m_Map; }
      set { m_Map = (int) value; }
    }

    public uint InstanceId { get; set; }

    public ZoneId Zone
    {
      get { return (ZoneId) m_zoneId; }
      set { m_zoneId = (int) value; }
    }

    public DateTime LastDeathTime { get; set; }

    /// <summary>Time of last resurrection</summary>
    public DateTime LastResTime { get; set; }

    public MapId CorpseMap
    {
      get { return (MapId) m_CorpseMap; }
      set { m_CorpseMap = (int) value; }
    }

    /// <summary>If CorpseX is null, there is no Corpse</summary>
    [Property]
    public float? CorpseX { get; set; }

    [Property]
    public float CorpseY { get; set; }

    [Property]
    public float CorpseZ { get; set; }

    [Property]
    public float CorpseO { get; set; }

    [Property(NotNull = true)]
    public float BindX { get; set; }

    [Property(NotNull = true)]
    public float BindY { get; set; }

    [Property(NotNull = true)]
    public float BindZ { get; set; }

    public MapId BindMap
    {
      get { return (MapId) m_BindMap; }
      set { m_BindMap = (int) value; }
    }

    public ZoneId BindZone
    {
      get { return (ZoneId) m_BindZone; }
      set { m_BindZone = (int) value; }
    }

    /// <summary>
    /// Default spells; talents excluded.
    /// Talent spells can be found in <see cref="T:WCell.RealmServer.Talents.SpecProfile" />.
    /// </summary>
    public List<SpellRecord> AbilitySpells { get; private set; }

    [Property]
    public int RuneSetMask { get; set; }

    [Property]
    public float[] RuneCooldowns { get; set; }

    [Property(NotNull = true)]
    public int BaseStrength { get; set; }

    [Property(NotNull = true)]
    public int BaseStamina { get; set; }

    [Property(NotNull = true)]
    public int BaseSpirit { get; set; }

    [Property(NotNull = true)]
    public int BaseIntellect { get; set; }

    [Property(NotNull = true)]
    public int BaseAgility { get; set; }

    [Property]
    public bool GodMode { get; set; }

    [Property]
    public byte ProfessionLevel { get; set; }

    [Property(NotNull = true)]
    public int Health { get; set; }

    [Property(NotNull = true)]
    public int BaseHealth { get; set; }

    [Property(NotNull = true)]
    public int Power { get; set; }

    [Property(NotNull = true)]
    public int BasePower { get; set; }

    [Property(NotNull = true)]
    public long Money { get; set; }

    [Property(NotNull = true)]
    public byte PetBoxEnchants { get; set; }

    [Property(NotNull = true)]
    public byte MountBoxExpands { get; set; }

    public SkillRecord[] LoadSkills()
    {
      return SkillRecord.GetAllSkillsFor(Guid);
    }

    internal ReputationRecord CreateReputationRecord()
    {
      return new ReputationRecord { OwnerId = Guid };
    }

    [Property("FinishedQuests", NotNull = false)]
    public uint[] FinishedQuests { get; set; }

    [Property("FinishedDailyQuests", NotNull = false)]
    public uint[] FinishedDailyQuests { get; set; }

    public int MailCount
    {
      get { return ActiveRecordBase<MailMessage>.Count("ReceiverId = " + (uint) Guid); }
    }

    public uint GuildId
    {
      get { return (uint) m_GuildId; }
      set { m_GuildId = (int) value; }
    }

    private void LoadItems()
    {
      try
      {
        _asda2LoadedItems = Asda2ItemRecord.LoadItems(EntityLowId);
        _asda2LoadedFastItemSlots =
          Asda2FastItemSlotRecord.LoadItems(EntityLowId);
      }
      catch(Exception ex)
      {
        RealmDBMgr.OnDBError(ex);
        _asda2LoadedItems = Asda2ItemRecord.LoadItems(EntityLowId);
        _asda2LoadedFastItemSlots =
          Asda2FastItemSlotRecord.LoadItems(EntityLowId);
      }
    }

    private void LoadFastItemSlots()
    {
      try
      {
        _asda2LoadedFastItemSlots =
          Asda2FastItemSlotRecord.LoadItems(EntityLowId);
      }
      catch(Exception ex)
      {
        RealmDBMgr.OnDBError(ex);
        _asda2LoadedFastItemSlots =
          Asda2FastItemSlotRecord.LoadItems(EntityLowId);
      }
    }

    public ICollection<Asda2ItemRecord> GetOrLoadItems()
    {
      if(_asda2LoadedItems == null)
        LoadItems();
      return _asda2LoadedItems;
    }

    public ICollection<Asda2FastItemSlotRecord> GetOrLoadFastItemSlots()
    {
      if(_asda2LoadedFastItemSlots == null)
        LoadFastItemSlots();
      return _asda2LoadedFastItemSlots;
    }

    public List<Asda2ItemRecord> GetMailItems(long mailId, int count)
    {
      List<Asda2ItemRecord> asda2ItemRecordList = new List<Asda2ItemRecord>(count);
      foreach(Asda2ItemRecord asda2LoadedItem in Asda2LoadedItems)
      {
        if(asda2LoadedItem.MailId == mailId)
          asda2ItemRecordList.Add(asda2LoadedItem);
      }

      return asda2ItemRecordList;
    }

    public IList<EquipmentSet> EquipmentSets { get; set; }

    /// <summary>Amount of accumulated rest-XP</summary>
    [Property]
    public int RestXp { get; set; }

    /// <summary>
    /// The id of the AreaTrigger which is letting us rest (or 0 if there is none)
    /// </summary>
    [Property]
    public int RestTriggerId { get; set; }

    [Property]
    public int NextTaxiVertexId { get; set; }

    [Property]
    public uint[] TaxiMask { get; set; }

    [Property]
    public bool IsPetActive { get; set; }

    [Property]
    public int StableSlotCount { get; set; }

    /// <summary>
    /// Amount of action-bar information etc for summoned pets
    /// </summary>
    [Property]
    public int PetSummonedCount { get; set; }

    /// <summary>Amount of Hunter pets</summary>
    [Property]
    public int PetCount { get; set; }

    public NPCId PetEntryId
    {
      get { return (NPCId) m_PetEntryId; }
      set { m_PetEntryId = (int) value; }
    }

    public NPCEntry PetEntry
    {
      get
      {
        if(PetEntryId == 0)
          return null;
        return NPCMgr.GetEntry(PetEntryId);
      }
    }

    [Property]
    public int PetHealth { get; set; }

    [Property]
    public int PetPower { get; set; }

    [Property]
    public int PrivatePerLevelItemBonusTemplateId { get; set; }

    public SpellId PetSummonSpellId
    {
      get { return (SpellId) m_SummonSpellId; }
      set { m_SummonSpellId = (int) value; }
    }

    /// <summary>Remaining duration in millis</summary>
    [Property]
    public int PetDuration { get; set; }

    public int CurrentSpecIndex { get; set; }

    [Property("LastTalentResetTime")]
    public DateTime? LastTalentResetTime { get; set; }

    public int TalentResetPriceTier
    {
      get { return _talentResetPriceTier; }
      set
      {
        if(value < 0)
          value = 0;
        if(value > TalentMgr.PlayerTalentResetPricesPerTier.Length - 1)
          value = TalentMgr.PlayerTalentResetPricesPerTier.Length - 1;
        _talentResetPriceTier = value;
      }
    }

    [Property]
    public DungeonDifficulty DungeonDifficulty { get; set; }

    [Property]
    public RaidDifficulty RaidDifficulty { get; set; }

    [Property]
    public BattlegroundSide BattlegroundTeam
    {
      get { return m_BattlegroundTeam; }
      set { m_BattlegroundTeam = value; }
    }

    public uint KillsTotal
    {
      get { return (uint) _killsTotal; }
      set { _killsTotal = (int) value; }
    }

    public uint HonorToday
    {
      get { return (uint) _honorToday; }
      set { _honorToday = (int) value; }
    }

    public uint HonorYesterday
    {
      get { return (uint) _honorYesterday; }
      set { _honorYesterday = (int) value; }
    }

    public uint LifetimeHonorableKills
    {
      get { return (uint) _lifetimeHonorableKills; }
      set { _lifetimeHonorableKills = (int) value; }
    }

    public uint HonorPoints
    {
      get { return (uint) _honorPoints; }
      set { _honorPoints = (int) value; }
    }

    public uint ArenaPoints
    {
      get { return (uint) _arenaPoints; }
      set { _arenaPoints = (int) value; }
    }

    public uint TitlePoints
    {
      get { return (uint) _titlePoints; }
      set { _titlePoints = (int) value; }
    }

    public int Rank
    {
      get { return _rank; }
      set { _rank = value; }
    }

    public void DeleteLater()
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(
        new Message(Delete));
    }

    public override void Delete()
    {
      int num = (int) TryDelete();
    }

    public override void DeleteAndFlush()
    {
      int num = (int) TryDelete();
    }

    public LoginErrorCode TryDelete()
    {
      if(!DeleteCharAccessories(EntityLowId))
        return LoginErrorCode.CHAR_DELETE_FAILED;
      DeleteFromGuild(EntityLowId, GuildId);
      base.DeleteAndFlush();
      return LoginErrorCode.CHAR_DELETE_SUCCESS;
    }

    public static void DeleteChar(uint charId)
    {
      ServerApp<RealmServer>.IOQueue.ExecuteInContext(() =>
      {
        Character character = World.GetCharacter(charId);
        uint guildId;
        if(character != null)
        {
          guildId = character.GuildId;
          character.Client.Disconnect(false);
        }
        else
          guildId = GetGuildId(charId);

        if(!DeleteCharAccessories(charId))
          return;
        DeleteFromGuild(charId, guildId);
        DeleteAll("Guid = " + charId);
      });
    }

    private static void DeleteFromGuild(uint charId, uint guildId)
    {
      if(guildId == 0U)
        return;
      Guild guild = GuildMgr.GetGuild(guildId);
      if(guild == null)
        return;
      guild.RemoveMember(charId);
    }

    private static bool DeleteCharAccessories(uint charId)
    {
      try
      {
        ActiveRecordBase<SpellRecord>.DeleteAll("OwnerId = " + charId);
        ActiveRecordBase<AuraRecord>.DeleteAll("OwnerId = " + charId);
        ActiveRecordBase<ItemRecord>.DeleteAll("OwnerId = " + charId);
        ActiveRecordBase<SkillRecord>.DeleteAll("OwnerId = " + charId);
        ActiveRecordBase<SpecProfile>.DeleteAll("CharacterId = " + charId);
        ActiveRecordBase<ReputationRecord>.DeleteAll("OwnerId = " + charId);
        ActiveRecordBase<QuestRecord>.DeleteAll("OwnerId = " + charId);
        ActiveRecordBase<SummonedPetRecord>.DeleteAll("OwnerLowId = " + charId);
        ActiveRecordBase<PermanentPetRecord>.DeleteAll("OwnerLowId = " + charId);
        MailMgr.ReturnValueMailFor(charId);
        ActiveRecordBase<MailMessage>.DeleteAll("ReceiverId = " + charId);
        Singleton<RelationMgr>.Instance.RemoveRelations(charId);
        InstanceMgr.RemoveLog(charId);
        Singleton<GroupMgr>.Instance.RemoveOfflineCharacter(charId);
        ActiveRecordBase<AchievementRecord>.DeleteAll("CharacterId = " + charId);
        ActiveRecordBase<AchievementProgressRecord>.DeleteAll("CharacterId = " + charId);
        return true;
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex, "Failed to delete character with Id: " + charId);
        return false;
      }
    }

    public void SetupNewRecord(Archetype archetype)
    {
      Race = archetype.Race.Id;
      Class = archetype.Class.Id;
      Level = 1;
      PositionX = 3066f;
      PositionY = 3350f;
      PositionZ = 0.0f;
      Orientation = archetype.StartOrientation;
      MapId = MapId.Alpia;
      Zone = ZoneId.None;
      TotalPlayTime = 0;
      LevelPlayTime = 0;
      TutorialFlags = new byte[32];
      WatchedFaction = -1;
      BindMap = MapId.Alpia;
      BindX = 3130.64f;
      BindY = 3398.69f;
      BindZ = 0.0f;
      BindZone = ZoneId.None;
      FreeStatPoints = CharacterFormulas.FreestatPointsOnStart;
      Money = 1L;
      DisplayId = archetype.Race.GetDisplayId(Gender);
      GlobalChatColor = Color.Yellow;
      SettingsFlags = new byte[16];
      for(int index = 0; index < 16; ++index)
        SettingsFlags[index] = 1;
      AvatarMask = 63;
      Asda2FactionId = -1;
      DiscoveredTitles = new uint[16];
      GetedTitles = new uint[16];
      PreTitleId = -1;
      PostTitleId = -1;
      LearnedRecipes = new uint[18];
      CraftingLevel = 1;
    }

    /// <summary>Gets the characters for the given account.</summary>
    /// <param name="account">the account</param>
    /// <returns>a collection of character objects of the characters on the given account</returns>
    public static CharacterRecord[] FindAllOfAccount(RealmAccount account)
    {
      CharacterRecord[] allByProperty;
      try
      {
        allByProperty =
          FindAllByProperty("Created", "AccountId",
            account.AccountId);
        foreach(CharacterRecord characterRecord in allByProperty)
          characterRecord.LoadItems();
      }
      catch(Exception ex)
      {
        RealmDBMgr.OnDBError(ex);
        allByProperty =
          FindAllByProperty("Created", "AccountId",
            account.AccountId);
      }

      return allByProperty;
    }

    /// <summary>Gets the characters for the given account.</summary>
    /// <param name="account">the account</param>
    /// <returns>a collection of character objects of the characters on the given account</returns>
    public static CharacterRecord[] FindAllOfAccount(int accId)
    {
      CharacterRecord[] allByProperty;
      try
      {
        allByProperty =
          FindAllByProperty("Created", "AccountId", accId);
        foreach(CharacterRecord characterRecord in allByProperty)
          characterRecord.LoadItems();
      }
      catch(Exception ex)
      {
        RealmDBMgr.OnDBError(ex);
        allByProperty =
          FindAllByProperty("Created", "AccountId", accId);
      }

      return allByProperty;
    }

    public static CharacterRecord GetRecord(uint id)
    {
      Character character = World.GetCharacter(id);
      return character != null ? character.Record : LoadRecordByEntityId(id);
    }

    public static uint GetIdByName(string name)
    {
      return (uint) new ScalarQuery<long>(typeof(CharacterRecord), QueryLanguage.Sql,
        string.Format("SELECT {0} FROM {1} WHERE {2} = {3} LIMIT 1",
          (object) DatabaseUtil.Dialect.QuoteForColumnName("EntityLowId"),
          (object) DatabaseUtil.Dialect.QuoteForTableName(typeof(CharacterRecord).Name),
          (object) DatabaseUtil.Dialect.QuoteForColumnName("Name"),
          (object) DatabaseUtil.ToSqlValueString(name))).Execute();
    }

    public static uint GetGuildId(uint charId)
    {
      return (uint) new ScalarQuery<int>(typeof(CharacterRecord), QueryLanguage.Sql,
        string.Format("SELECT {0} FROM {1} WHERE {2} = {3} LIMIT 1",
          (object) DatabaseUtil.Dialect.QuoteForColumnName("GuildId"),
          (object) DatabaseUtil.Dialect.QuoteForTableName(typeof(CharacterRecord).Name),
          (object) DatabaseUtil.Dialect.QuoteForColumnName("Guid"), (object) charId)).Execute();
    }

    public byte[] RawAliases { get; set; }

    [Property(NotNull = true)]
    public byte CharNum { get; set; }

    [Property(NotNull = true)]
    public byte Zodiac { get; set; }

    public ICollection<Asda2ItemRecord> Asda2LoadedItems
    {
      get { return _asda2LoadedItems; }
    }

    [Property]
    public byte Asda2Class { get; set; }

    public ICollection<Asda2FastItemSlotRecord> Asda2LoadedFastItemSlots
    {
      get { return _asda2LoadedFastItemSlots; }
    }

    [Property]
    public byte EyesColor { get; set; }

    [Property]
    public int BaseLuck { get; set; }

    [Property]
    public int FreeStatPoints { get; set; }

    [Property]
    public int GlobalChatColorDb
    {
      get { return GlobalChatColor.ARGBValue; }
      set
      {
        GlobalChatColor = new Color
        {
          ARGBValue = value
        };
      }
    }

    [Property(Length = 16)]
    public uint[] DiscoveredTitles { get; set; }

    [Property(Length = 16)]
    public uint[] GetedTitles { get; set; }

    [Property(Length = 18)]
    public uint[] LearnedRecipes { get; set; }

    public Color GlobalChatColor { get; set; }

    [Property]
    public int FishingLevel { get; set; }

    [Property]
    public int AvatarMask { get; set; }

    [Property(Length = 16)]
    public byte[] SettingsFlags { get; set; }

    [Property]
    public short Asda2FactionId { get; set; }

    [Property]
    public int GuildPoints { get; set; }

    [Property]
    public short PreTitleId { get; set; }

    [Property]
    public short PostTitleId { get; set; }

    [Property]
    public byte MaxRepipesCount { get; set; }

    [Property]
    public byte CraftingLevel { get; set; }

    /// <summary>Опыт крафта в %</summary>
    [Property]
    public float CraftingExp { get; set; }

    [Property]
    public int BanPoints { get; set; }

    [Property]
    public byte PremiumWarehouseBagsCount { get; set; }

    [Property]
    public byte PremiumAvatarWarehouseBagsCount { get; set; }

    [Property]
    public string WarehousePassword { get; set; }

    [Property]
    public int Asda2HonorPoints { get; set; }

    [Property]
    public int RebornCount { get; set; }

    [Property]
    public bool ChatBanned { get; set; }

    public void SetAliases(IEnumerable<KeyValuePair<string, string>> aliases)
    {
      List<byte> byteList = new List<byte>(100);
      foreach(KeyValuePair<string, string> aliase in aliases)
      {
        byteList.AddRange(Encoding.UTF8.GetBytes(aliase.Key));
        byteList.Add(0);
        byteList.AddRange(Encoding.UTF8.GetBytes(aliase.Value));
        byteList.Add(0);
      }

      RawAliases = byteList.ToArray();
    }

    public Dictionary<string, string> ParseAliases()
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      if(RawAliases != null)
      {
        bool flag = true;
        int index1 = 0;
        int index2 = -1;
        for(int index3 = 0; index3 < RawAliases.Length; ++index3)
        {
          if(RawAliases[index3] == 0)
          {
            flag = !flag;
            if(flag)
            {
              if(index2 >= 0)
              {
                string index4 = Encoding.UTF8.GetString(RawAliases, index1, index2 - index1);
                string str = Encoding.UTF8.GetString(RawAliases, index2, index3 - index2);
                dictionary[index4] = str;
              }

              index1 = index3;
              index2 = -1;
            }
            else
              index2 = index3;
          }
        }
      }

      return dictionary;
    }

    public override string ToString()
    {
      return string.Format("{0} (Id: {1}, Account: {2})", Name, EntityLowId,
        AccountId);
    }
  }
}