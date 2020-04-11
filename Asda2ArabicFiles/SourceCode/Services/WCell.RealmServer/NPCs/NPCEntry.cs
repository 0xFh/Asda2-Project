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
using WCell.RealmServer.Lang;
using WCell.RealmServer.Looting;
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
	public delegate NPC NPCCreator(NPCEntry entry);

	/// <summary>
	/// NPC Entry
	/// </summary>
	[DataHolder]
    [Serializable]
	public partial class NPCEntry : ObjectTemplate, INPCDataHolder
	{
		#region Global Variables
		// <summary>
		// This is added to the CombatReach of all Units
		// </summary>
		//public static float BaseAttackReach = 1f;

		/// <summary>
		/// Default base-range in which a mob will aggro (in yards).
		/// Also see <see cref="AggroRangePerLevel"/>
		/// </summary>
		[NotVariable]
		public static float AggroBaseRangeDefault = 6;

		/// <summary>
		/// Amount of yards to add to the <see cref="AggroBaseRangeDefault"/> per level difference.
		/// </summary>
		public static float AggroRangePerLevel = 0f;

		/// <summary>
		/// Mobs with a distance >= this will not start aggressive actions
		/// </summary>
		public static float AggroRangeMaxDefault = 6;

		private static float aggroRangeMinDefault = 6;

		/// <summary>
		/// Mobs within this range will *definitely* aggro
		/// </summary>
		public static float AggroRangeMinDefault
		{
			get { return aggroRangeMinDefault; }
			set
			{
				aggroRangeMinDefault = value;
				AggroMinRangeSq = value * value;
			}
		}

		[NotVariable]
		public static float AggroMinRangeSq = aggroRangeMinDefault * aggroRangeMinDefault;

		#endregion
        
		public CreatureType Type = CreatureType.Undead;
        
		[NotPersistent]
		public CreatureFamily Family;

		public CreatureRank Rank;
        
		/// <summary>
		/// Whether a new NPC should be completely idle (not react to anything that happens)
		/// </summary>
		public bool IsIdle;

		/// <summary>
		/// Whether an NPC is a special event trigger like the little
		/// elementals used to cast spells or trigger concerts
		/// </summary>
		[NotPersistent]
		public bool IsEventTrigger
		{
			get;
			private set;
		}
        [System.NonSerialized]
		[NotPersistent]
		public NPCEquipmentEntry Equipment;
        
		public InvisType InvisibilityType;

		public InhabitType InhabitType = InhabitType.Ground;

	    public bool IsAgressive;
	    public float AtackRange;
		public bool Regenerates;
        
		[NotPersistent]
		public bool GeneratesXp;

		[NotPersistent]
		/// <summary>
		/// Should be called when a new NPC is created
		/// </summary>
		public NPCTypeHandler[] InstanceTypeHandlers;

		[NotPersistent]
		/// <summary>
		/// Should be called when a new NPCSpawnEntry is created
		/// </summary>
		public NPCSpawnTypeHandler[] SpawnTypeHandlers;

		public bool IsVendor
		{
			get { return VendorItems != null; }
		}

		[NotPersistent]
		public float AggroBaseRange
		{
			get;
			set;
		}

		#region Entry and substitute Entries
		[NotPersistent]
		public NPCId NPCId
		{
			get; set;
		}

		[NotPersistent]
		public NPCEntry Entry
		{
			get { return this; }
		}


		/// <summary>
		/// Returns the NPCEntry for the given difficulty
		/// </summary>
		public NPCEntry GetEntry(uint difficultyIndex)
		{
			return this;
		}

		public MapTemplate GetMapTemplate()
		{
			var spawn = SpawnEntries.FirstOrDefault();
			if (spawn != null)
			{
				return World.GetMapTemplate(spawn.MapId);
			}
			return null;
		}
		#endregion

		#region Strings

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

		#endregion

		#region Display & Model
        [NotPersistent]
	    public UnitModelInfo ModelInfo;
		public UnitModelInfo GetRandomModel()
		{
            return ModelInfo;
		}
		#endregion

		#region Stats
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
        
		// public int OffhandAttackPower;

		public float MinDamage;

		public float MaxDamage;
        
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
			return (int)(Utility.Random(MinHealth, MaxHealth) * NPCMgr.DefaultNPCHealthFactor + 0.999999f);
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
        public void SetDamage (float value)
        {
            MinDamage = value;
            MaxDamage = value;
        }
        public void SetDamage(float minDamage, float maxDamage)
        {
            MinDamage = minDamage;
            MaxDamage = maxDamage;
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

		[Persistent(ItemConstants.MaxResCount)]
		public int[] Resistances = new int[ItemConstants.MaxResCount];

		public void SetResistance(DamageSchool school, int value)
		{
			Resistances[(uint)school] = value;
		}

		public int GetResistance(DamageSchool school)
		{
			if (Resistances == null)
			{
				return 0;
			}
			return Resistances.Get((uint)school);
		}
		#endregion

		#region Stat Scaling
		public bool HasScalableStats
		{
			get { return PetLevelStatInfos != null; }
		}

		[NotPersistent]
        [System.NonSerialized]
		public PetLevelStatInfo[] PetLevelStatInfos;

		/// <summary>
		/// 
		/// </summary>
		public PetLevelStatInfo GetPetLevelStatInfo(int level)
		{
			if (PetLevelStatInfos == null)
			{
				//LogManager.GetCurrentClassLogger().Warn("Tried to get PetLevelStatInfo for NPCEntry {0} (Level {1}), which has no PetLevelStatInfos", this, level);
				// info = PetMgr.GetDefaultPetLevelStatInfo(level);
				return null;
			}
			else
			{
				var info = PetLevelStatInfos.Get(level);
				if (info == null)
				{
					//LogManager.GetCurrentClassLogger().Warn("Tried to get PetLevelStatInfo for NPCEntry {0} (Level {1}), which has no PetLevelStatInfos", this, level);
					//info = PetMgr.GetDefaultPetLevelStatInfo(level);
				}
				return info;
			}
		}
		#endregion

		#region Weapons
		/// <summary>
		/// Creates the default MainHand Weapon for all Spawns of this SpawnPoint
		/// </summary>
		public IAsda2Weapon CreateMainHandWeapon()
		{
			if (Type == CreatureType.None || Type == CreatureType.Totem || Type == CreatureType.NotSpecified)
			{
				// these kinds of NPCs do not attack ever
				return GenericWeapon.Peace;
			}

			return new GenericWeapon(InventorySlotTypeMask.WeaponMainHand, MinDamage, MaxDamage, AttackTime,AtackRange);
		}
        
		#endregion

		#region Flags
		public NPCEntryFlags EntryFlags;

		public NPCFlags NPCFlags;

		public UnitFlags UnitFlags;

		public UnitDynamicFlags DynamicFlags;

		[NotPersistent]
		public bool IsDead
		{
			get { return DynamicFlags.HasFlag(UnitDynamicFlags.Dead); }
			set { DynamicFlags |= UnitDynamicFlags.Dead; }
		}

		public UnitExtraFlags ExtraFlags;

		[NotPersistent]
		/// <summary>
		/// All bits of <see cref="Flags"/> that are set
		/// </summary>
		public uint[] SetFlagIndices;

		public bool IsTamable
		{
            get
            {
                return false; //EntryFlags.HasFlag(NPCEntryFlags.Tamable); 
            }
		}

		public bool IsExoticPet
		{
            get
            {
                return false;
                //EntryFlags.HasFlag(NPCEntryFlags.ExoticCreature); 
            }
		}
		#endregion

		#region Movement & Speed
		//TODO: Rename to something more meaningful
		public AIMotionGenerationType MovementType;

		//public MovementType MovementType;

		/// <summary>
		/// The factor to be applied to the default speed for this kind of NPC
		/// </summary>
		public float SpeedFactor = 1;

		public float WalkSpeed;

		public float RunSpeed;

		public float FlySpeed;

		#endregion

		#region Loot

		public uint MoneyDrop;

		public override List<Asda2LootItemEntry> GetLootEntries()
		{
			return Asda2LootMgr.GetEntries(Asda2LootEntryType.Npc, Id);
		}
		#endregion

		#region Factions
		private FactionTemplateId m_HordeFactionId = FactionTemplateId.Maraudine;
		private FactionTemplateId m_AllianceFactionId = FactionTemplateId.Maraudine;

		/// <summary>
		/// 
		/// </summary>
		public FactionTemplateId HordeFactionId
		{
			get { return m_HordeFactionId; }
			set
			{
				m_HordeFactionId = value;
				if (HordeFaction != null)
				{
					HordeFaction = FactionMgr.Get(value);
				}
			}
		}

		public FactionTemplateId AllianceFactionId
		{
			get { return m_AllianceFactionId; }
			set
			{
				m_AllianceFactionId = value;
				if (AllianceFaction != null)
				{
					AllianceFaction = FactionMgr.Get(value);
				}
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
				return Utility.HeadsOrTails() ? HordeFaction : AllianceFaction;
			}
		}

		public Faction GetFaction(FactionGroup fact)
		{
			return fact == FactionGroup.Alliance ? AllianceFaction : HordeFaction;
		}
		#endregion

		#region Spells
		public uint SpellGroupId;

		/// <summary>
		/// A set of default Spells for this NPC
		/// </summary>
		[Persistent(4)]
		public SpellId[] FixedSpells = new SpellId[4];

		/// <summary>
		/// Spell to be casted when a Character talks to the NPC
		/// </summary>
		[NotPersistent]
		public Spell InteractionSpell;

		/// <summary>
		/// Usable Spells to be casted by Mobs of this Type
		/// </summary>
		[NotPersistent]
		public Dictionary<SpellId, Spell> Spells;

		[NotPersistent]
		public SpellTriggerInfo SpellTriggerInfo;

		/// <summary>
		/// Adds a Spell that will be used by all NPCs of this Entry
		/// </summary>
		public void AddSpell(SpellId spellId)
		{
			var spell = SpellHandler.Get(spellId);
			if (spell != null)
			{
				AddSpell(spell);
			}
		}

		/// <summary>
		/// Adds a Spell that will be used by all NPCs of this Entry
		/// </summary>
		public void AddSpell(uint spellId)
		{
			var spell = SpellHandler.Get(spellId);
			if (spell != null)
			{
				AddSpell(spell);
			}
		}

		/// <summary>
		/// Adds a Spell that will be used by all NPCs of this Entry
		/// </summary>
		public void AddSpells(params SpellId[] ids)
		{
			foreach (var id in ids)
			{
				var spell = SpellHandler.Get(id);
				if (spell != null)
				{
					AddSpell(spell);
				}
			}
		}

		/// <summary>
		/// Adds a Spell that will be used by all NPCs of this Entry
		/// </summary>
		public void AddSpells(params uint[] ids)
		{
			foreach (var id in ids)
			{
				var spell = SpellHandler.Get(id);
				if (spell != null)
				{
					AddSpell(spell);
				}
			}
		}

		/// <summary>
		/// Adds a Spell that will be used by all NPCs of this Entry
		/// </summary>
		public void AddSpell(Spell spell)
		{
			if (Spells == null)
			{
				Spells = new Dictionary<SpellId, Spell>(5);
			}
			OnSpellAdded(spell);
			Spells[spell.SpellId] = spell;
		}

		private void OnSpellAdded(Spell spell)
		{
		}

		#endregion

		#region Spawns
        [System.NonSerialized]
		[NotPersistent]
		public List<NPCSpawnEntry> SpawnEntries = new List<NPCSpawnEntry>(3);

		[NotPersistent]
		public NPCSpawnEntry FirstSpawnEntry
		{
			get { return SpawnEntries.Count > 0 ? SpawnEntries[0] : null; }
		}
		#endregion

		#region Interactable NPCs
		/// <summary>
		/// Trainers
		/// </summary>
		public TrainerEntry TrainerEntry;

		/// <summary>
		/// Vendors
		/// </summary>
		[NotPersistent]
		public List<VendorItemEntry> VendorItems
		{
			get
			{
				List<VendorItemEntry> list;
				NPCMgr.VendorLists.TryGetValue(Id, out list);
				return list;
			}
		}

		/// <summary>
		/// BattleMasters
		/// </summary>
		[NotPersistent]
		public BattlegroundTemplate BattlegroundTemplate;
		#endregion

		#region Vehicles
		private uint m_VehicleId;

		public uint VehicleId
		{
			get { return m_VehicleId; }
			set
			{
				m_VehicleId = value;
				if (value > 0)
				{
					NPCMgr.VehicleEntries.TryGetValue((int)VehicleId, out VehicleEntry);

					if (IsVehicle && (NPCCreator == null || NPCCreator == DefaultCreator))
					{
						// set Vehicle creator by default
						NPCCreator = entry => new Vehicle();
					}
				}
			}
		}

		[NotPersistent]
		public VehicleEntry VehicleEntry;

		public bool IsVehicle
		{
			get { return VehicleEntry != null; }
		}

		public float HoverHeight;

		public float VehicleAimAdjustment;
		#endregion

		#region Decay
		/// <summary>
		/// The default decay delay in seconds.
		/// </summary>
		[NotPersistent]
		public int DefaultDecayDelayMillis;

		/// <summary>
		/// The default delay before removing the NPC after it died when not looted.
		/// </summary>
		int _DefaultDecayDelayMillis
		{
			get
			{
				if (Rank == CreatureRank.Normal)
				{
					return NPCMgr.DecayDelayNormalMillis;
				}
				if (Rank == CreatureRank.WorldBoss)
				{
					return NPCMgr.DecayDelayRareMillis;
				}
				return NPCMgr.DecayDelayEpicMillis;
			}
		}

        public int Expirience { get; set; }

	    public bool IsBoss { get; set; }

	    #endregion

		#region FinalizeDataHolder
		/// <summary>
		/// Is called to initialize the object; usually after a set of other operations have been performed or if
		/// the right time has come and other required steps have been performed.
		/// </summary>
        public void FinalizeDataHolder()
		{
		    if (string.IsNullOrEmpty(DefaultName))
		    {
		        ContentMgr.OnInvalidDBData("NPCEntry has no name: " + this);
		        return;
		    }

		    if (SpellGroupId != 0 && NPCMgr.PetSpells != null)
		    {
		        var spells = NPCMgr.PetSpells.Get(SpellGroupId) ?? Spell.EmptyArray;
		        foreach (var spell in spells)
		        {
		            AddSpell(spell);
		        }
		    }

		    if (MinMana > MaxMana)
		    {
		        MaxMana = MinMana;
		    }

		    if (MaxDamage > 0)
		    {
		        if (MinDamage < 1)
		        {
		            MinDamage = 1;
		        }
		        if (MaxDamage < MinDamage)
		        {
		            MaxDamage = MinDamage;
		        }
		    }
		    if (Rank == CreatureRank.WorldBoss || Rank == CreatureRank.Boss)
		        IsBoss = true;
		    
		    NPCId = (NPCId) Id;

		    DefaultDecayDelayMillis = _DefaultDecayDelayMillis;
		    Family = NPCMgr.GetFamily(CreatureFamilyId.Wolf);

		    if (Type == CreatureType.NotSpecified || VehicleEntry != null)
		    {
		        IsIdle = true;
		    }

		    if (Type == CreatureType.NotSpecified && UnitFlags.HasFlag((UnitFlags.Passive | UnitFlags.NotSelectable)))
		    {
		        IsEventTrigger = true;
		        IsIdle = false;
		    }

		    if (Resistances == null)
		    {
		        Resistances = new int[ItemConstants.MaxResCount];
		    }

		    SetFlagIndices = Utility.GetSetIndices((uint) NPCFlags);

		    // set/fix factions
		    HordeFaction = FactionMgr.Get(HordeFactionId);
		    AllianceFaction = FactionMgr.Get(AllianceFactionId);
		    if (HordeFaction == null)
		    {
		        HordeFaction = AllianceFaction;
		        HordeFactionId = AllianceFactionId;
		    }
		    else if (AllianceFaction == null)
		    {
		        AllianceFaction = HordeFaction;
		        AllianceFactionId = HordeFactionId;
		    }
		    if (AllianceFaction == null)
		    {
		        ContentMgr.OnInvalidDBData("NPCEntry has no valid Faction: " + this);
		        HordeFaction = AllianceFaction = NPCMgr.DefaultFaction;
		        HordeFactionId = AllianceFactionId = (FactionTemplateId) HordeFaction.Template.Id;
		    }

		    // Add all default spells
		    if (FixedSpells != null)
		    {
		        foreach (var spell in FixedSpells)
		        {
		            AddSpell(spell);
		        }
		    }

		    InstanceTypeHandlers = NPCMgr.GetNPCTypeHandlers(this);
		    SpawnTypeHandlers = NPCMgr.GetNPCSpawnTypeHandlers(this);


		    GeneratesXp = (Type != CreatureType.Critter && Type != CreatureType.None &&
		                   !ExtraFlags.HasFlag(UnitExtraFlags.NoXP));

		    AtackRange = CombatReach + 0.2f;
		    ModelInfo = new UnitModelInfo {BoundingRadius = BoundingRadius, CombatReach = CombatReach};

		    // add to container
		    if (Id < NPCMgr.Entries.Length)
		    {
		        NPCMgr.Entries[Id] = this;
		    }
		    else
		    {
		        NPCMgr.CustomEntries[Id] = this;
		    }
		    ++NPCMgr.EntryCount;

		    if (BrainCreator == null)
		    {
		        BrainCreator = DefaultBrainCreator;
		    }

		    if (NPCCreator == null)
		    {
		        NPCCreator = DefaultCreator;
		    }

		    #region calc expirience
		    Expirience =
		        (int)
		        (((MinHealth + MaxHealth)*2.2f + (MaxDamage + MinDamage)*100 + Resistances.Aggregate((a, r) => a + r*100))/
		         2000*Math.Pow(MinLevel, 0.3325f)); 
		    #endregion
		}

	    //readonly int[]_worldBosses = new []{544,543,496,530,839,507,487,470,800,532,531,837,711,472,529,525,645,776,518,827,515,612,813,511,510};
	    

	    #endregion

		#region Creators
		[NotPersistent]
		public Func<NPC, IBrain> BrainCreator;

		[NotPersistent]
		public NPCCreator NPCCreator;

		public NPC Create(uint difficulty = uint.MaxValue)
		{
			var npc = NPCCreator(GetEntry(difficulty));
            
			npc.SetupNPC(this, null);
			return npc;
		}

		public NPC Create(NPCSpawnPoint spawn)
		{
			var npc = NPCCreator(GetEntry(spawn.Map.DifficultyIndex));
			npc.SetupNPC(this, spawn);
			return npc;
		}

		public NPC SpawnAt(Map map, Vector3 pos, bool hugGround = false)
		{
			var npc = Create(map.DifficultyIndex);
			if (hugGround && InhabitType == InhabitType.Ground)
			{
				pos.Z = map.Terrain.GetGroundHeightUnderneath(pos);
			}
			map.AddObject(npc, pos);
			return npc;
		}

		public NPC SpawnAt(IWorldZoneLocation loc, bool hugGround = false)
		{
			var npc = Create(loc.Map.DifficultyIndex);
			npc.Zone = loc.GetZone();
			var pos = loc.Position;
			if (hugGround && InhabitType == InhabitType.Ground)
			{
				pos.Z = loc.Map.Terrain.GetGroundHeightUnderneath(pos);
			}
			loc.Map.AddObject(npc, pos);
			return npc;
		}

		public NPC SpawnAt(IWorldLocation loc, bool hugGround = false)
		{
			var npc = Create(loc.Map.DifficultyIndex);
            
			var pos = loc.Position;
			if (hugGround && InhabitType == InhabitType.Ground)
			{
				pos.Z = loc.Map.Terrain.GetGroundHeightUnderneath(pos);
			}
			loc.Map.AddObject(npc, loc.Position);
			npc.Phase = loc.Phase;
			return npc;
		}

		public static NPCCreator DefaultCreator = entry => new NPC();
	    public float BoundingRadius;
        public float CombatReach;
        [System.NonSerialized]
        private Faction _hordeFaction;
        [System.NonSerialized]
	    private Faction _allianceFaction;

	    public IBrain DefaultBrainCreator(NPC npc)
		{
			return new MobBrain(npc, IsIdle ? BrainState.Idle : BrainState.Roam)
			{
				IsAggressive = IsAgressive
			};
		}
		#endregion

		#region Events
		internal void NotifyActivated(NPC npc)
		{
			var evt = Activated;
			if (evt != null)
			{
				evt(npc);
			}
		}

		internal void NotifyDeleted(NPC npc)
		{
			var evt = Deleted;
			if (evt != null)
			{
				evt(npc);
			}
		}

		internal void NotifyInteracting(NPC npc, Character chr)
		{
			if (InteractionSpell != null)
			{
				chr.SpellCast.TriggerSelf(InteractionSpell);
			}

			var evt = Interacting;
			if (evt != null)
			{
				evt(chr, npc);
			}
		}

		internal bool NotifyBeforeDeath(NPC npc)
		{
			var evt = BeforeDeath;
			if (evt != null)
			{
				return evt(npc);
			}
			return true;
		}

		internal void NotifyDied(NPC npc)
		{
			var evt = Died;
			if (evt != null)
			{
				evt(npc);
			}
		}

		internal void NotifyLeveledChanged(NPC npc)
		{
			var evt = LevelChanged;
			if (evt != null)
			{
				evt(npc);
			}
		}
		#endregion

		#region Dump

		public void Dump(IndentTextWriter writer)
		{
			writer.WriteLine("{3}{0} (Id: {1}, {2})", DefaultName, Id, NPCId, Rank != 0 ? Rank + " " : "");
			if (!string.IsNullOrEmpty(DefaultTitle))
			{
				writer.WriteLine("Title: " + DefaultTitle);
			}
			if (Type != 0)
			{
				writer.WriteLine("Type: " + Type);
			}
			if (Family != null)
			{
				writer.WriteLine("Family: " + Family);
			}

			if (TrainerEntry != null)
			{
				writer.WriteLine("Trainer "
					//+ "for {0} {1}",
					//             TrainerEntry.RequiredRace != 0 ? TrainerEntry.RequiredRace.ToString() : "",
					//             TrainerEntry.RequiredClass != 0 ? TrainerEntry.RequiredClass.ToString() : ""
								 );
			}

			WriteFaction(writer);
			writer.WriteLine("Level: {0} - {1}", MinLevel, MaxLevel);
			writer.WriteLine("Health: {0} - {1}", MinHealth, MaxHealth);

			writer.WriteLineNotDefault(MinMana, "Mana: {0} - {1}", MinMana, MaxMana);
			writer.WriteLineNotDefault(NPCFlags, "Flags: " + NPCFlags);
			writer.WriteLineNotDefault(DynamicFlags, "DynamicFlags: " + DynamicFlags);
			writer.WriteLineNotDefault(UnitFlags, "UnitFlags: " + UnitFlags);
			writer.WriteLineNotDefault(ExtraFlags, "ExtraFlags: " + string.Format("0x{0:X}", ExtraFlags));
			writer.WriteLineNotDefault(AttackTime + OffhandAttackTime, "AttackTime: " + AttackTime, "Offhand: " + OffhandAttackTime);
			writer.WriteLineNotDefault(AttackPower, "AttackPower: " + AttackPower);
			//writer.WriteLineNotDefault(OffhandAttackPower, "OffhandAttackPower: " + OffhandAttackPower);
			writer.WriteLineNotDefault(MinDamage + MaxDamage, "Damage: {0} - {1}", MinDamage, MaxDamage);
			var resistances = new List<string>(8);
			for (var i = 0; i < Resistances.Length; i++)
			{
				var res = Resistances[i];
				if (res > 0)
				{
					resistances.Add(string.Format("{0}: {1}", (DamageSchool)i, res));
				}
			}
			if (Scale != 1)
			{
				writer.WriteLine("Scale: " + Scale);
			}

			var cr = GetRandomModel().CombatReach;
			var br = GetRandomModel().BoundingRadius;
			writer.WriteLine("CombatReach: " + cr);
			writer.WriteLine("BoundingRadius: " + br);
			writer.WriteLineNotDefault(resistances.Count, "Resistances: " + resistances.ToString(", "));
			writer.WriteLineNotDefault(MoneyDrop, "MoneyDrop: " + MoneyDrop);
			writer.WriteLineNotDefault(InvisibilityType, "Invisibility: " + InvisibilityType);
			writer.WriteLineNotDefault(MovementType, "MovementType: " + MovementType);
			writer.WriteLineNotDefault(WalkSpeed + RunSpeed + FlySpeed, "Speeds - Walking: {0}, Running: {1}, Flying: {2} ",
									   WalkSpeed, RunSpeed, FlySpeed);
			var spells = Spells;
			if (spells != null && spells.Count > 0)
			{
				writer.WriteLine("Spells: " + Spells.ToString(", "));
			}
			if (Equipment != null)
			{
				writer.WriteLine("Equipment: {0}", Equipment.ItemIds.Where(id => id != 0).ToString(", "));
			}

			

			//if (inclFaction)	
			//{
			//    writer.WriteLineNotDefault(DefaultFactionId, "Faction: " + DefaultFactionId);
			//}
		}

		private void WriteFaction(IndentTextWriter writer)
		{
			writer.WriteLineNotNull(HordeFaction, "HordeFaction: " + HordeFaction);
			writer.WriteLineNotNull(AllianceFaction, "AllianceFaction: " + AllianceFaction);
		}

		#endregion

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
	}

	[Flags]
	public enum InhabitType
	{
		Ground = 1,
		Water = 2,
		Amphibious = Ground | Water,
		Air = 4,
		Anywhere = Ground | Water | Air
	}
}