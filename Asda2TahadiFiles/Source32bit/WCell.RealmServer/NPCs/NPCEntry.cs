using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.NPCs.Trainers;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.NPCs.Vendors;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.NPCs
{
  /// <summary>NPC Entry</summary>
  [DataHolder]
  [Serializable]
  public class NPCEntry : ObjectTemplate, INPCDataHolder, IDataHolder
  {
    /// <summary>
    /// Default base-range in which a mob will aggro (in yards).
    /// Also see <see cref="F:WCell.RealmServer.NPCs.NPCEntry.AggroRangePerLevel" />
    /// </summary>
    [NotVariable]public static float AggroBaseRangeDefault = 6f;

    /// <summary>
    /// Amount of yards to add to the <see cref="F:WCell.RealmServer.NPCs.NPCEntry.AggroBaseRangeDefault" /> per level difference.
    /// </summary>
    public static float AggroRangePerLevel = 0.0f;

    /// <summary>
    /// Mobs with a distance &gt;= this will not start aggressive actions
    /// </summary>
    public static float AggroRangeMaxDefault = 6f;

    private static float aggroRangeMinDefault = 6f;

    [NotVariable]public static float AggroMinRangeSq = aggroRangeMinDefault * aggroRangeMinDefault;

    public static NPCCreator DefaultCreator = entry => new NPC();
    public CreatureType Type = CreatureType.Undead;
    public InhabitType InhabitType = InhabitType.Ground;
    [Persistent(7)]public int[] Resistances = new int[7];

    /// <summary>
    /// The factor to be applied to the default speed for this kind of NPC
    /// </summary>
    public float SpeedFactor = 1f;

    private FactionTemplateId m_HordeFactionId = FactionTemplateId.Maraudine;
    private FactionTemplateId m_AllianceFactionId = FactionTemplateId.Maraudine;

    /// <summary>A set of default Spells for this NPC</summary>
    [Persistent(4)]public SpellId[] FixedSpells = new SpellId[4];

    [NotPersistent][NonSerialized]public List<NPCSpawnEntry> SpawnEntries = new List<NPCSpawnEntry>(3);
    [NotPersistent]public CreatureFamily Family;
    public CreatureRank Rank;

    /// <summary>
    /// Whether a new NPC should be completely idle (not react to anything that happens)
    /// </summary>
    public bool IsIdle;

    [NotPersistent][NonSerialized]public NPCEquipmentEntry Equipment;
    public InvisType InvisibilityType;
    public bool IsAgressive;
    public float AtackRange;
    public bool Regenerates;
    [NotPersistent]public bool GeneratesXp;
    [NotPersistent]public NPCTypeHandler[] InstanceTypeHandlers;
    [NotPersistent]public NPCSpawnTypeHandler[] SpawnTypeHandlers;
    [NotPersistent]public UnitModelInfo ModelInfo;
    public int MaxLevel;
    public int MinLevel;
    public int MinHealth;
    public int MaxHealth;
    public int MinMana;
    public int MaxMana;
    public DamageSchool DamageSchool;
    public int AttackTime;
    public int OffhandAttackTime;
    public int AttackPower;
    public float MinDamage;
    public float MaxDamage;
    [NotPersistent][NonSerialized]public PetLevelStatInfo[] PetLevelStatInfos;
    public NPCEntryFlags EntryFlags;
    public NPCFlags NPCFlags;
    public UnitFlags UnitFlags;
    public UnitDynamicFlags DynamicFlags;
    public UnitExtraFlags ExtraFlags;
    [NotPersistent]public uint[] SetFlagIndices;
    public AIMotionGenerationType MovementType;
    public float WalkSpeed;
    public float RunSpeed;
    public float FlySpeed;
    public uint MoneyDrop;
    public uint SpellGroupId;

    /// <summary>Spell to be casted when a Character talks to the NPC</summary>
    [NotPersistent]public Spell InteractionSpell;

    /// <summary>Usable Spells to be casted by Mobs of this Type</summary>
    [NotPersistent]public Dictionary<SpellId, Spell> Spells;

    [NotPersistent]public SpellTriggerInfo SpellTriggerInfo;

    /// <summary>Trainers</summary>
    public TrainerEntry TrainerEntry;

    /// <summary>BattleMasters</summary>
    [NotPersistent]public BattlegroundTemplate BattlegroundTemplate;

    private uint m_VehicleId;
    [NotPersistent]public VehicleEntry VehicleEntry;
    public float HoverHeight;
    public float VehicleAimAdjustment;

    /// <summary>The default decay delay in seconds.</summary>
    [NotPersistent]public int DefaultDecayDelayMillis;

    [NotPersistent]public Func<NPC, IBrain> BrainCreator;
    [NotPersistent]public NPCCreator NPCCreator;
    public float BoundingRadius;
    public float CombatReach;
    [NonSerialized]private Faction _hordeFaction;
    [NonSerialized]private Faction _allianceFaction;

    /// <summary>
    /// Called when the given NPC is added to the world or when resurrected.
    /// </summary>
    public event NPCHandler Activated;

    /// <summary>Called when NPC is deleted</summary>
    public event NPCHandler Deleted;

    /// <summary>
    /// Called when any Character interacts with the given NPC such as quests, vendors, trainers, bankers, anything that causes a gossip.
    /// </summary>
    public event Action<Character, NPC> Interacting;

    public event Func<NPC, bool> BeforeDeath;

    public event NPCHandler Died;

    /// <summary>
    /// Is called when this NPC's level changed, only if the NPC of this NPCEntry may gain levels (<see cref="P:WCell.RealmServer.Entities.NPC.MayGainExperience" />).
    /// </summary>
    public event NPCHandler LevelChanged;

    /// <summary>Mobs within this range will *definitely* aggro</summary>
    public static float AggroRangeMinDefault
    {
      get { return aggroRangeMinDefault; }
      set
      {
        aggroRangeMinDefault = value;
        AggroMinRangeSq = value * value;
      }
    }

    /// <summary>
    /// Whether an NPC is a special event trigger like the little
    /// elementals used to cast spells or trigger concerts
    /// </summary>
    [NotPersistent]
    public bool IsEventTrigger { get; private set; }

    public bool IsVendor
    {
      get { return VendorItems != null; }
    }

    [NotPersistent]
    public float AggroBaseRange { get; set; }

    [NotPersistent]
    public NPCId NPCId { get; set; }

    [NotPersistent]
    public NPCEntry Entry
    {
      get { return this; }
    }

    /// <summary>Returns the NPCEntry for the given difficulty</summary>
    public NPCEntry GetEntry(uint difficultyIndex)
    {
      return this;
    }

    public MapTemplate GetMapTemplate()
    {
      NPCSpawnEntry npcSpawnEntry = SpawnEntries.FirstOrDefault();
      if(npcSpawnEntry != null)
        return World.GetMapTemplate(npcSpawnEntry.MapId);
      return null;
    }

    [NotPersistent]
    public string DefaultName
    {
      get { return NPCId.ToString(); }
    }

    [NotPersistent]
    public string DefaultTitle
    {
      get { return "Title"; }
    }

    public UnitModelInfo GetRandomModel()
    {
      return ModelInfo;
    }

    public int GetRandomLevel()
    {
      return Utility.Random(MinLevel, MaxLevel);
    }

    public void SetLevel(int value)
    {
      MinLevel = value;
      MaxLevel = value;
    }

    public void SetLevel(int minLevel, int maxLevel)
    {
      MinLevel = minLevel;
      MaxLevel = maxLevel;
    }

    public int GetRandomHealth()
    {
      return (int) (Utility.Random(MinHealth, MaxHealth) *
                    (double) NPCMgr.DefaultNPCHealthFactor + 0.999998986721039);
    }

    public void SetHealth(int value)
    {
      MinHealth = value;
      MaxHealth = value;
    }

    public void SetHealth(int minHealth, int maxHealth)
    {
      MinHealth = minHealth;
      MaxHealth = maxHealth;
    }

    public int GetRandomMana()
    {
      return Utility.Random(MinMana, MaxMana);
    }

    public void SetMana(int mana)
    {
      MinMana = mana;
      MaxMana = mana;
    }

    public void SetMana(int minMana, int maxMana)
    {
      MinMana = minMana;
      MaxMana = maxMana;
    }

    public void SetResistance(DamageSchool school, int value)
    {
      Resistances[(uint) school] = value;
    }

    public int GetResistance(DamageSchool school)
    {
      if(Resistances == null)
        return 0;
      return Resistances.Get((uint) school);
    }

    public bool HasScalableStats
    {
      get { return PetLevelStatInfos != null; }
    }

    /// <summary>
    /// 
    /// </summary>
    public PetLevelStatInfo GetPetLevelStatInfo(int level)
    {
      if(PetLevelStatInfos == null)
        return null;
      return PetLevelStatInfos.Get(level);
    }

    /// <summary>
    /// Creates the default MainHand Weapon for all Spawns of this SpawnPoint
    /// </summary>
    public IAsda2Weapon CreateMainHandWeapon()
    {
      if(Type == CreatureType.None || Type == CreatureType.Totem ||
         Type == CreatureType.NotSpecified)
        return GenericWeapon.Peace;
      return new GenericWeapon(InventorySlotTypeMask.WeaponMainHand, MinDamage,
        MaxDamage, AttackTime, AtackRange);
    }

    [NotPersistent]
    public bool IsDead
    {
      get { return DynamicFlags.HasFlag(UnitDynamicFlags.Dead); }
      set { DynamicFlags |= UnitDynamicFlags.Dead; }
    }

    public bool IsTamable
    {
      get { return false; }
    }

    public bool IsExoticPet
    {
      get { return false; }
    }

    public override List<Asda2LootItemEntry> GetLootEntries()
    {
      return Asda2LootMgr.GetEntries(Asda2LootEntryType.Npc, Id);
    }

    /// <summary>
    /// 
    /// </summary>
    public FactionTemplateId HordeFactionId
    {
      get { return m_HordeFactionId; }
      set
      {
        m_HordeFactionId = value;
        if(HordeFaction == null)
          return;
        HordeFaction = FactionMgr.Get(value);
      }
    }

    public FactionTemplateId AllianceFactionId
    {
      get { return m_AllianceFactionId; }
      set
      {
        m_AllianceFactionId = value;
        if(AllianceFaction == null)
          return;
        AllianceFaction = FactionMgr.Get(value);
      }
    }

    [NotPersistent]
    public Faction HordeFaction
    {
      get { return _hordeFaction; }
      private set { _hordeFaction = value; }
    }

    [NotPersistent]
    public Faction AllianceFaction
    {
      get { return _allianceFaction; }
      private set { _allianceFaction = value; }
    }

    public Faction RandomFaction
    {
      get
      {
        if(!Utility.HeadsOrTails())
          return AllianceFaction;
        return HordeFaction;
      }
    }

    public Faction GetFaction(FactionGroup fact)
    {
      if(fact != FactionGroup.Alliance)
        return HordeFaction;
      return AllianceFaction;
    }

    /// <summary>
    /// Adds a Spell that will be used by all NPCs of this Entry
    /// </summary>
    public void AddSpell(SpellId spellId)
    {
      Spell spell = SpellHandler.Get(spellId);
      if(spell == null)
        return;
      AddSpell(spell);
    }

    /// <summary>
    /// Adds a Spell that will be used by all NPCs of this Entry
    /// </summary>
    public void AddSpell(uint spellId)
    {
      Spell spell = SpellHandler.Get(spellId);
      if(spell == null)
        return;
      AddSpell(spell);
    }

    /// <summary>
    /// Adds a Spell that will be used by all NPCs of this Entry
    /// </summary>
    public void AddSpells(params SpellId[] ids)
    {
      foreach(SpellId id in ids)
      {
        Spell spell = SpellHandler.Get(id);
        if(spell != null)
          AddSpell(spell);
      }
    }

    /// <summary>
    /// Adds a Spell that will be used by all NPCs of this Entry
    /// </summary>
    public void AddSpells(params uint[] ids)
    {
      foreach(uint id in ids)
      {
        Spell spell = SpellHandler.Get(id);
        if(spell != null)
          AddSpell(spell);
      }
    }

    /// <summary>
    /// Adds a Spell that will be used by all NPCs of this Entry
    /// </summary>
    public void AddSpell(Spell spell)
    {
      if(Spells == null)
        Spells = new Dictionary<SpellId, Spell>(5);
      OnSpellAdded(spell);
      Spells[spell.SpellId] = spell;
    }

    private void OnSpellAdded(Spell spell)
    {
    }

    [NotPersistent]
    public NPCSpawnEntry FirstSpawnEntry
    {
      get
      {
        if(SpawnEntries.Count <= 0)
          return null;
        return SpawnEntries[0];
      }
    }

    /// <summary>Vendors</summary>
    [NotPersistent]
    public List<VendorItemEntry> VendorItems
    {
      get
      {
        List<VendorItemEntry> vendorItemEntryList;
        NPCMgr.VendorLists.TryGetValue(Id, out vendorItemEntryList);
        return vendorItemEntryList;
      }
    }

    public uint VehicleId
    {
      get { return m_VehicleId; }
      set
      {
        m_VehicleId = value;
        if(value <= 0U)
          return;
        NPCMgr.VehicleEntries.TryGetValue((int) VehicleId, out VehicleEntry);
        if(!IsVehicle || NPCCreator != null && !(NPCCreator == DefaultCreator))
          return;
        NPCCreator = entry => (NPC) new Vehicle();
      }
    }

    public bool IsVehicle
    {
      get { return VehicleEntry != null; }
    }

    /// <summary>
    /// The default delay before removing the NPC after it died when not looted.
    /// </summary>
    private int _DefaultDecayDelayMillis
    {
      get
      {
        if(Rank == CreatureRank.Normal)
          return NPCMgr.DecayDelayNormalMillis;
        if(Rank == CreatureRank.WorldBoss)
          return NPCMgr.DecayDelayRareMillis;
        return NPCMgr.DecayDelayEpicMillis;
      }
    }

    public int Expirience { get; set; }

    public bool IsBoss { get; set; }

    /// <summary>
    /// Is called to initialize the object; usually after a set of other operations have been performed or if
    /// the right time has come and other required steps have been performed.
    /// </summary>
    public void FinalizeDataHolder()
    {
      if(string.IsNullOrEmpty(DefaultName))
      {
        ContentMgr.OnInvalidDBData("NPCEntry has no name: " + this);
      }
      else
      {
        if(SpellGroupId != 0U && NPCMgr.PetSpells != null)
        {
          foreach(Spell spell in NPCMgr.PetSpells.Get(SpellGroupId) ?? Spell.EmptyArray)
            AddSpell(spell);
        }

        if(MinMana > MaxMana)
          MaxMana = MinMana;
        if(MaxDamage > 0.0)
        {
          if(MinDamage < 1.0)
            MinDamage = 1f;
          if(MaxDamage < (double) MinDamage)
            MaxDamage = MinDamage;
        }

        if(Rank == CreatureRank.WorldBoss || Rank == CreatureRank.Boss)
          IsBoss = true;
        NPCId = (NPCId) Id;
        DefaultDecayDelayMillis = _DefaultDecayDelayMillis;
        Family = NPCMgr.GetFamily(CreatureFamilyId.Wolf);
        if(Type == CreatureType.NotSpecified || VehicleEntry != null)
          IsIdle = true;
        if(Type == CreatureType.NotSpecified &&
           UnitFlags.HasFlag(UnitFlags.Passive | UnitFlags.NotSelectable))
        {
          IsEventTrigger = true;
          IsIdle = false;
        }

        if(Resistances == null)
          Resistances = new int[7];
        SetFlagIndices = Utility.GetSetIndices((uint) NPCFlags);
        HordeFaction = FactionMgr.Get(HordeFactionId);
        AllianceFaction = FactionMgr.Get(AllianceFactionId);
        if(HordeFaction == null)
        {
          HordeFaction = AllianceFaction;
          HordeFactionId = AllianceFactionId;
        }
        else if(AllianceFaction == null)
        {
          AllianceFaction = HordeFaction;
          AllianceFactionId = HordeFactionId;
        }

        if(AllianceFaction == null)
        {
          ContentMgr.OnInvalidDBData("NPCEntry has no valid Faction: " + this);
          HordeFaction = AllianceFaction = NPCMgr.DefaultFaction;
          HordeFactionId = AllianceFactionId = (FactionTemplateId) HordeFaction.Template.Id;
        }

        if(FixedSpells != null)
        {
          foreach(SpellId fixedSpell in FixedSpells)
            AddSpell(fixedSpell);
        }

        InstanceTypeHandlers = NPCMgr.GetNPCTypeHandlers(this);
        SpawnTypeHandlers = NPCMgr.GetNPCSpawnTypeHandlers(this);
        GeneratesXp = Type != CreatureType.Critter && Type != CreatureType.None &&
                      !ExtraFlags.HasFlag(UnitExtraFlags.NoXP);
        AtackRange = CombatReach + 0.2f;
        ModelInfo = new UnitModelInfo
        {
          BoundingRadius = BoundingRadius,
          CombatReach = CombatReach
        };
        if(Id < NPCMgr.Entries.Length)
          NPCMgr.Entries[Id] = this;
        else
          NPCMgr.CustomEntries[Id] = this;
        ++NPCMgr.EntryCount;
        if(BrainCreator == null)
          BrainCreator = DefaultBrainCreator;
        if(NPCCreator == null)
          NPCCreator = DefaultCreator;
        Expirience = (int) (((MinHealth + MaxHealth) * 2.20000004768372 +
                             (MaxDamage + (double) MinDamage) * 100.0 +
                             Resistances.Aggregate(
                               (a, r) => a + r * 100)) / 2000.0 *
                            Math.Pow(MinLevel, 0.332500010728836));
      }
    }

    public NPC Create(uint difficulty = 4294967295)
    {
      NPC npc = NPCCreator(GetEntry(difficulty));
      npc.SetupNPC(this, null);
      return npc;
    }

    public NPC Create(NPCSpawnPoint spawn)
    {
      NPC npc = NPCCreator(GetEntry(spawn.Map.DifficultyIndex));
      npc.SetupNPC(this, spawn);
      return npc;
    }

    public NPC SpawnAt(Map map, Vector3 pos, bool hugGround = false)
    {
      NPC npc = Create(map.DifficultyIndex);
      if(hugGround && InhabitType == InhabitType.Ground)
        pos.Z = map.Terrain.GetGroundHeightUnderneath(pos);
      map.AddObject(npc, pos);
      return npc;
    }

    public NPC SpawnAt(IWorldZoneLocation loc, bool hugGround = false)
    {
      NPC npc = Create(loc.Map.DifficultyIndex);
      npc.Zone = loc.GetZone();
      Vector3 position = loc.Position;
      if(hugGround && InhabitType == InhabitType.Ground)
        position.Z = loc.Map.Terrain.GetGroundHeightUnderneath(position);
      loc.Map.AddObject(npc, position);
      return npc;
    }

    public NPC SpawnAt(IWorldLocation loc, bool hugGround = false)
    {
      NPC npc = Create(loc.Map.DifficultyIndex);
      Vector3 position = loc.Position;
      if(hugGround && InhabitType == InhabitType.Ground)
        position.Z = loc.Map.Terrain.GetGroundHeightUnderneath(position);
      loc.Map.AddObject(npc, loc.Position);
      npc.Phase = loc.Phase;
      return npc;
    }

    public IBrain DefaultBrainCreator(NPC npc)
    {
      MobBrain mobBrain = new MobBrain(npc, IsIdle ? BrainState.Idle : BrainState.Roam);
      mobBrain.IsAggressive = IsAgressive;
      return mobBrain;
    }

    internal void NotifyActivated(NPC npc)
    {
      NPCHandler activated = Activated;
      if(activated == null)
        return;
      activated(npc);
    }

    internal void NotifyDeleted(NPC npc)
    {
      NPCHandler deleted = Deleted;
      if(deleted == null)
        return;
      deleted(npc);
    }

    internal void NotifyInteracting(NPC npc, Character chr)
    {
      if(InteractionSpell != null)
        chr.SpellCast.TriggerSelf(InteractionSpell);
      Action<Character, NPC> interacting = Interacting;
      if(interacting == null)
        return;
      interacting(chr, npc);
    }

    internal bool NotifyBeforeDeath(NPC npc)
    {
      Func<NPC, bool> beforeDeath = BeforeDeath;
      if(beforeDeath != null)
        return beforeDeath(npc);
      return true;
    }

    internal void NotifyDied(NPC npc)
    {
      NPCHandler died = Died;
      if(died == null)
        return;
      died(npc);
    }

    internal void NotifyLeveledChanged(NPC npc)
    {
      NPCHandler levelChanged = LevelChanged;
      if(levelChanged == null)
        return;
      levelChanged(npc);
    }

    public void Dump(IndentTextWriter writer)
    {
      writer.WriteLine("{3}{0} (Id: {1}, {2})", (object) DefaultName, (object) Id, (object) NPCId,
        Rank != CreatureRank.Normal ? (object) (((int) Rank) + " ") : (object) "");
      if(!string.IsNullOrEmpty(DefaultTitle))
        writer.WriteLine("Title: " + DefaultTitle);
      if(Type != CreatureType.None)
        writer.WriteLine("Type: " + Type);
      if(Family != null)
        writer.WriteLine("Family: " + Family);
      if(TrainerEntry != null)
        writer.WriteLine("Trainer ");
      WriteFaction(writer);
      writer.WriteLine("Level: {0} - {1}", MinLevel, MaxLevel);
      writer.WriteLine("Health: {0} - {1}", MinHealth, MaxHealth);
      writer.WriteLineNotDefault(MinMana, "Mana: {0} - {1}", (object) MinMana,
        (object) MaxMana);
      writer.WriteLineNotDefault(NPCFlags, "Flags: " + NPCFlags);
      writer.WriteLineNotDefault(DynamicFlags,
        "DynamicFlags: " + DynamicFlags);
      writer.WriteLineNotDefault(UnitFlags, "UnitFlags: " + UnitFlags);
      writer.WriteLineNotDefault(ExtraFlags,
        "ExtraFlags: " + string.Format("0x{0:X}", ExtraFlags));
      writer.WriteLineNotDefault(AttackTime + OffhandAttackTime,
        "AttackTime: " + AttackTime, (object) ("Offhand: " + (object) OffhandAttackTime));
      writer.WriteLineNotDefault(AttackPower, "AttackPower: " + AttackPower);
      writer.WriteLineNotDefault((float) (MinDamage + (double) MaxDamage),
        "Damage: {0} - {1}", (object) MinDamage, (object) MaxDamage);
      List<string> collection = new List<string>(8);
      for(int index = 0; index < Resistances.Length; ++index)
      {
        int resistance = Resistances[index];
        if(resistance > 0)
          collection.Add(string.Format("{0}: {1}", (DamageSchool) index, resistance));
      }

      if(Scale != 1.0)
        writer.WriteLine("Scale: " + Scale);
      float combatReach = GetRandomModel().CombatReach;
      float boundingRadius = GetRandomModel().BoundingRadius;
      writer.WriteLine("CombatReach: " + combatReach);
      writer.WriteLine("BoundingRadius: " + boundingRadius);
      writer.WriteLineNotDefault(collection.Count, "Resistances: " + collection.ToString(", "));
      writer.WriteLineNotDefault(MoneyDrop, "MoneyDrop: " + MoneyDrop);
      writer.WriteLineNotDefault(InvisibilityType,
        "Invisibility: " + InvisibilityType);
      writer.WriteLineNotDefault(MovementType,
        "MovementType: " + MovementType);
      writer.WriteLineNotDefault(
        (float) (WalkSpeed + (double) RunSpeed + FlySpeed),
        "Speeds - Walking: {0}, Running: {1}, Flying: {2} ", (object) WalkSpeed, (object) RunSpeed,
        (object) FlySpeed);
      Dictionary<SpellId, Spell> spells = Spells;
      if(spells != null && spells.Count > 0)
        writer.WriteLine("Spells: " + Spells.ToString(", "));
      if(Equipment == null)
        return;
      writer.WriteLine("Equipment: {0}",
        Equipment.ItemIds
          .Where(id => id != (Asda2ItemId) 0)
          .ToString(", "));
    }

    private void WriteFaction(IndentTextWriter writer)
    {
      writer.WriteLineNotNull(HordeFaction, "HordeFaction: " + HordeFaction);
      writer.WriteLineNotNull(AllianceFaction, "AllianceFaction: " + AllianceFaction);
    }

    public override IWorldLocation[] GetInWorldTemplates()
    {
      return SpawnEntries.ToArray();
    }

    public override string ToString()
    {
      return "Entry: " + DefaultName + " (Id: " + Id + ")";
    }

    public static IEnumerable<NPCEntry> GetAllDataHolders()
    {
      return new NPCMgr.EntryIterator();
    }

    public delegate void NPCHandler(NPC npc);
  }
}