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
        [NotVariable] public static float AggroBaseRangeDefault = 6f;

        /// <summary>
        /// Amount of yards to add to the <see cref="F:WCell.RealmServer.NPCs.NPCEntry.AggroBaseRangeDefault" /> per level difference.
        /// </summary>
        public static float AggroRangePerLevel = 0.0f;

        /// <summary>
        /// Mobs with a distance &gt;= this will not start aggressive actions
        /// </summary>
        public static float AggroRangeMaxDefault = 6f;

        private static float aggroRangeMinDefault = 6f;

        [NotVariable]
        public static float AggroMinRangeSq = NPCEntry.aggroRangeMinDefault * NPCEntry.aggroRangeMinDefault;

        public static NPCCreator DefaultCreator = (NPCCreator) (entry => new NPC());
        public CreatureType Type = CreatureType.Undead;
        public InhabitType InhabitType = InhabitType.Ground;
        [Persistent(7)] public int[] Resistances = new int[7];

        /// <summary>
        /// The factor to be applied to the default speed for this kind of NPC
        /// </summary>
        public float SpeedFactor = 1f;

        private FactionTemplateId m_HordeFactionId = FactionTemplateId.Maraudine;
        private FactionTemplateId m_AllianceFactionId = FactionTemplateId.Maraudine;

        /// <summary>A set of default Spells for this NPC</summary>
        [Persistent(4)] public SpellId[] FixedSpells = new SpellId[4];

        [NotPersistent] [NonSerialized] public List<NPCSpawnEntry> SpawnEntries = new List<NPCSpawnEntry>(3);
        [NotPersistent] public CreatureFamily Family;
        public CreatureRank Rank;

        /// <summary>
        /// Whether a new NPC should be completely idle (not react to anything that happens)
        /// </summary>
        public bool IsIdle;

        [NotPersistent] [NonSerialized] public NPCEquipmentEntry Equipment;
        public InvisType InvisibilityType;
        public bool IsAgressive;
        public float AtackRange;
        public bool Regenerates;
        [NotPersistent] public bool GeneratesXp;
        [NotPersistent] public NPCTypeHandler[] InstanceTypeHandlers;
        [NotPersistent] public NPCSpawnTypeHandler[] SpawnTypeHandlers;
        [NotPersistent] public UnitModelInfo ModelInfo;
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
        [NotPersistent] [NonSerialized] public PetLevelStatInfo[] PetLevelStatInfos;
        public NPCEntryFlags EntryFlags;
        public NPCFlags NPCFlags;
        public UnitFlags UnitFlags;
        public UnitDynamicFlags DynamicFlags;
        public UnitExtraFlags ExtraFlags;
        [NotPersistent] public uint[] SetFlagIndices;
        public AIMotionGenerationType MovementType;
        public float WalkSpeed;
        public float RunSpeed;
        public float FlySpeed;
        public uint MoneyDrop;
        public uint SpellGroupId;

        /// <summary>Spell to be casted when a Character talks to the NPC</summary>
        [NotPersistent] public Spell InteractionSpell;

        /// <summary>Usable Spells to be casted by Mobs of this Type</summary>
        [NotPersistent] public Dictionary<SpellId, Spell> Spells;

        [NotPersistent] public SpellTriggerInfo SpellTriggerInfo;

        /// <summary>Trainers</summary>
        public TrainerEntry TrainerEntry;

        /// <summary>BattleMasters</summary>
        [NotPersistent] public BattlegroundTemplate BattlegroundTemplate;

        private uint m_VehicleId;
        [NotPersistent] public VehicleEntry VehicleEntry;
        public float HoverHeight;
        public float VehicleAimAdjustment;

        /// <summary>The default decay delay in seconds.</summary>
        [NotPersistent] public int DefaultDecayDelayMillis;

        [NotPersistent] public Func<NPC, IBrain> BrainCreator;
        [NotPersistent] public NPCCreator NPCCreator;
        public float BoundingRadius;
        public float CombatReach;
        [NonSerialized] private Faction _hordeFaction;
        [NonSerialized] private Faction _allianceFaction;

        /// <summary>
        /// Called when the given NPC is added to the world or when resurrected.
        /// </summary>
        public event NPCEntry.NPCHandler Activated;

        /// <summary>Called when NPC is deleted</summary>
        public event NPCEntry.NPCHandler Deleted;

        /// <summary>
        /// Called when any Character interacts with the given NPC such as quests, vendors, trainers, bankers, anything that causes a gossip.
        /// </summary>
        public event Action<Character, NPC> Interacting;

        public event Func<NPC, bool> BeforeDeath;

        public event NPCEntry.NPCHandler Died;

        /// <summary>
        /// Is called when this NPC's level changed, only if the NPC of this NPCEntry may gain levels (<see cref="P:WCell.RealmServer.Entities.NPC.MayGainExperience" />).
        /// </summary>
        public event NPCEntry.NPCHandler LevelChanged;

        /// <summary>Mobs within this range will *definitely* aggro</summary>
        public static float AggroRangeMinDefault
        {
            get { return NPCEntry.aggroRangeMinDefault; }
            set
            {
                NPCEntry.aggroRangeMinDefault = value;
                NPCEntry.AggroMinRangeSq = value * value;
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
            get { return this.VendorItems != null; }
        }

        [NotPersistent] public float AggroBaseRange { get; set; }

        [NotPersistent] public NPCId NPCId { get; set; }

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
            NPCSpawnEntry npcSpawnEntry = this.SpawnEntries.FirstOrDefault<NPCSpawnEntry>();
            if (npcSpawnEntry != null)
                return World.GetMapTemplate(npcSpawnEntry.MapId);
            return (MapTemplate) null;
        }

        [NotPersistent]
        public string DefaultName
        {
            get { return this.NPCId.ToString(); }
        }

        [NotPersistent]
        public string DefaultTitle
        {
            get { return "Title"; }
        }

        public UnitModelInfo GetRandomModel()
        {
            return this.ModelInfo;
        }

        public int GetRandomLevel()
        {
            return Utility.Random(this.MinLevel, this.MaxLevel);
        }

        public void SetLevel(int value)
        {
            this.MinLevel = value;
            this.MaxLevel = value;
        }

        public void SetLevel(int minLevel, int maxLevel)
        {
            this.MinLevel = minLevel;
            this.MaxLevel = maxLevel;
        }

        public int GetRandomHealth()
        {
            return (int) ((double) Utility.Random(this.MinHealth, this.MaxHealth) *
                          (double) NPCMgr.DefaultNPCHealthFactor + 0.999998986721039);
        }

        public void SetHealth(int value)
        {
            this.MinHealth = value;
            this.MaxHealth = value;
        }

        public void SetHealth(int minHealth, int maxHealth)
        {
            this.MinHealth = minHealth;
            this.MaxHealth = maxHealth;
        }

        public int GetRandomMana()
        {
            return Utility.Random(this.MinMana, this.MaxMana);
        }

        public void SetMana(int mana)
        {
            this.MinMana = mana;
            this.MaxMana = mana;
        }

        public void SetMana(int minMana, int maxMana)
        {
            this.MinMana = minMana;
            this.MaxMana = maxMana;
        }

        public void SetResistance(DamageSchool school, int value)
        {
            this.Resistances[(uint) school] = value;
        }

        public int GetResistance(DamageSchool school)
        {
            if (this.Resistances == null)
                return 0;
            return this.Resistances.Get<int>((uint) school);
        }

        public bool HasScalableStats
        {
            get { return this.PetLevelStatInfos != null; }
        }

        /// <summary>
        /// 
        /// </summary>
        public PetLevelStatInfo GetPetLevelStatInfo(int level)
        {
            if (this.PetLevelStatInfos == null)
                return (PetLevelStatInfo) null;
            return this.PetLevelStatInfos.Get<PetLevelStatInfo>(level);
        }

        /// <summary>
        /// Creates the default MainHand Weapon for all Spawns of this SpawnPoint
        /// </summary>
        public IAsda2Weapon CreateMainHandWeapon()
        {
            if (this.Type == CreatureType.None || this.Type == CreatureType.Totem ||
                this.Type == CreatureType.NotSpecified)
                return (IAsda2Weapon) GenericWeapon.Peace;
            return (IAsda2Weapon) new GenericWeapon(InventorySlotTypeMask.WeaponMainHand, this.MinDamage,
                this.MaxDamage, this.AttackTime, this.AtackRange);
        }

        [NotPersistent]
        public bool IsDead
        {
            get { return this.DynamicFlags.HasFlag((Enum) UnitDynamicFlags.Dead); }
            set { this.DynamicFlags |= UnitDynamicFlags.Dead; }
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
            return Asda2LootMgr.GetEntries(Asda2LootEntryType.Npc, this.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        public FactionTemplateId HordeFactionId
        {
            get { return this.m_HordeFactionId; }
            set
            {
                this.m_HordeFactionId = value;
                if (this.HordeFaction == null)
                    return;
                this.HordeFaction = FactionMgr.Get(value);
            }
        }

        public FactionTemplateId AllianceFactionId
        {
            get { return this.m_AllianceFactionId; }
            set
            {
                this.m_AllianceFactionId = value;
                if (this.AllianceFaction == null)
                    return;
                this.AllianceFaction = FactionMgr.Get(value);
            }
        }

        [NotPersistent]
        public Faction HordeFaction
        {
            get { return this._hordeFaction; }
            private set { this._hordeFaction = value; }
        }

        [NotPersistent]
        public Faction AllianceFaction
        {
            get { return this._allianceFaction; }
            private set { this._allianceFaction = value; }
        }

        public Faction RandomFaction
        {
            get
            {
                if (!Utility.HeadsOrTails())
                    return this.AllianceFaction;
                return this.HordeFaction;
            }
        }

        public Faction GetFaction(FactionGroup fact)
        {
            if (fact != FactionGroup.Alliance)
                return this.HordeFaction;
            return this.AllianceFaction;
        }

        /// <summary>
        /// Adds a Spell that will be used by all NPCs of this Entry
        /// </summary>
        public void AddSpell(SpellId spellId)
        {
            Spell spell = SpellHandler.Get(spellId);
            if (spell == null)
                return;
            this.AddSpell(spell);
        }

        /// <summary>
        /// Adds a Spell that will be used by all NPCs of this Entry
        /// </summary>
        public void AddSpell(uint spellId)
        {
            Spell spell = SpellHandler.Get(spellId);
            if (spell == null)
                return;
            this.AddSpell(spell);
        }

        /// <summary>
        /// Adds a Spell that will be used by all NPCs of this Entry
        /// </summary>
        public void AddSpells(params SpellId[] ids)
        {
            foreach (SpellId id in ids)
            {
                Spell spell = SpellHandler.Get(id);
                if (spell != null)
                    this.AddSpell(spell);
            }
        }

        /// <summary>
        /// Adds a Spell that will be used by all NPCs of this Entry
        /// </summary>
        public void AddSpells(params uint[] ids)
        {
            foreach (uint id in ids)
            {
                Spell spell = SpellHandler.Get(id);
                if (spell != null)
                    this.AddSpell(spell);
            }
        }

        /// <summary>
        /// Adds a Spell that will be used by all NPCs of this Entry
        /// </summary>
        public void AddSpell(Spell spell)
        {
            if (this.Spells == null)
                this.Spells = new Dictionary<SpellId, Spell>(5);
            this.OnSpellAdded(spell);
            this.Spells[spell.SpellId] = spell;
        }

        private void OnSpellAdded(Spell spell)
        {
        }

        [NotPersistent]
        public NPCSpawnEntry FirstSpawnEntry
        {
            get
            {
                if (this.SpawnEntries.Count <= 0)
                    return (NPCSpawnEntry) null;
                return this.SpawnEntries[0];
            }
        }

        /// <summary>Vendors</summary>
        [NotPersistent]
        public List<VendorItemEntry> VendorItems
        {
            get
            {
                List<VendorItemEntry> vendorItemEntryList;
                NPCMgr.VendorLists.TryGetValue(this.Id, out vendorItemEntryList);
                return vendorItemEntryList;
            }
        }

        public uint VehicleId
        {
            get { return this.m_VehicleId; }
            set
            {
                this.m_VehicleId = value;
                if (value <= 0U)
                    return;
                NPCMgr.VehicleEntries.TryGetValue((int) this.VehicleId, out this.VehicleEntry);
                if (!this.IsVehicle || this.NPCCreator != null && !(this.NPCCreator == NPCEntry.DefaultCreator))
                    return;
                this.NPCCreator = (NPCCreator) (entry => (NPC) new Vehicle());
            }
        }

        public bool IsVehicle
        {
            get { return this.VehicleEntry != null; }
        }

        /// <summary>
        /// The default delay before removing the NPC after it died when not looted.
        /// </summary>
        private int _DefaultDecayDelayMillis
        {
            get
            {
                if (this.Rank == CreatureRank.Normal)
                    return NPCMgr.DecayDelayNormalMillis;
                if (this.Rank == CreatureRank.WorldBoss)
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
            if (string.IsNullOrEmpty(this.DefaultName))
            {
                ContentMgr.OnInvalidDBData("NPCEntry has no name: " + (object) this);
            }
            else
            {
                if (this.SpellGroupId != 0U && NPCMgr.PetSpells != null)
                {
                    foreach (Spell spell in NPCMgr.PetSpells.Get<Spell[]>(this.SpellGroupId) ?? Spell.EmptyArray)
                        this.AddSpell(spell);
                }

                if (this.MinMana > this.MaxMana)
                    this.MaxMana = this.MinMana;
                if ((double) this.MaxDamage > 0.0)
                {
                    if ((double) this.MinDamage < 1.0)
                        this.MinDamage = 1f;
                    if ((double) this.MaxDamage < (double) this.MinDamage)
                        this.MaxDamage = this.MinDamage;
                }

                if (this.Rank == CreatureRank.WorldBoss || this.Rank == CreatureRank.Boss)
                    this.IsBoss = true;
                this.NPCId = (NPCId) this.Id;
                this.DefaultDecayDelayMillis = this._DefaultDecayDelayMillis;
                this.Family = NPCMgr.GetFamily(CreatureFamilyId.Wolf);
                if (this.Type == CreatureType.NotSpecified || this.VehicleEntry != null)
                    this.IsIdle = true;
                if (this.Type == CreatureType.NotSpecified &&
                    this.UnitFlags.HasFlag((Enum) (UnitFlags.Passive | UnitFlags.NotSelectable)))
                {
                    this.IsEventTrigger = true;
                    this.IsIdle = false;
                }

                if (this.Resistances == null)
                    this.Resistances = new int[7];
                this.SetFlagIndices = Utility.GetSetIndices((uint) this.NPCFlags);
                this.HordeFaction = FactionMgr.Get(this.HordeFactionId);
                this.AllianceFaction = FactionMgr.Get(this.AllianceFactionId);
                if (this.HordeFaction == null)
                {
                    this.HordeFaction = this.AllianceFaction;
                    this.HordeFactionId = this.AllianceFactionId;
                }
                else if (this.AllianceFaction == null)
                {
                    this.AllianceFaction = this.HordeFaction;
                    this.AllianceFactionId = this.HordeFactionId;
                }

                if (this.AllianceFaction == null)
                {
                    ContentMgr.OnInvalidDBData("NPCEntry has no valid Faction: " + (object) this);
                    this.HordeFaction = this.AllianceFaction = NPCMgr.DefaultFaction;
                    this.HordeFactionId = this.AllianceFactionId = (FactionTemplateId) this.HordeFaction.Template.Id;
                }

                if (this.FixedSpells != null)
                {
                    foreach (SpellId fixedSpell in this.FixedSpells)
                        this.AddSpell(fixedSpell);
                }

                this.InstanceTypeHandlers = NPCMgr.GetNPCTypeHandlers(this);
                this.SpawnTypeHandlers = NPCMgr.GetNPCSpawnTypeHandlers(this);
                this.GeneratesXp = this.Type != CreatureType.Critter && this.Type != CreatureType.None &&
                                   !this.ExtraFlags.HasFlag((Enum) UnitExtraFlags.NoXP);
                this.AtackRange = this.CombatReach + 0.2f;
                this.ModelInfo = new UnitModelInfo()
                {
                    BoundingRadius = this.BoundingRadius,
                    CombatReach = this.CombatReach
                };
                if ((long) this.Id < (long) NPCMgr.Entries.Length)
                    NPCMgr.Entries[this.Id] = this;
                else
                    NPCMgr.CustomEntries[this.Id] = this;
                ++NPCMgr.EntryCount;
                if (this.BrainCreator == null)
                    this.BrainCreator = new Func<NPC, IBrain>(this.DefaultBrainCreator);
                if (this.NPCCreator == null)
                    this.NPCCreator = NPCEntry.DefaultCreator;
                this.Expirience = (int) (((double) (this.MinHealth + this.MaxHealth) * 2.20000004768372 +
                                          ((double) this.MaxDamage + (double) this.MinDamage) * 100.0 +
                                          (double) ((IEnumerable<int>) this.Resistances).Aggregate<int>(
                                              (Func<int, int, int>) ((a, r) => a + r * 100))) / 2000.0 *
                                         Math.Pow((double) this.MinLevel, 0.332500010728836));
            }
        }

        public NPC Create(uint difficulty = 4294967295)
        {
            NPC npc = this.NPCCreator(this.GetEntry(difficulty));
            npc.SetupNPC(this, (NPCSpawnPoint) null);
            return npc;
        }

        public NPC Create(NPCSpawnPoint spawn)
        {
            NPC npc = this.NPCCreator(this.GetEntry(spawn.Map.DifficultyIndex));
            npc.SetupNPC(this, spawn);
            return npc;
        }

        public NPC SpawnAt(Map map, Vector3 pos, bool hugGround = false)
        {
            NPC npc = this.Create(map.DifficultyIndex);
            if (hugGround && this.InhabitType == InhabitType.Ground)
                pos.Z = map.Terrain.GetGroundHeightUnderneath(pos);
            map.AddObject((WorldObject) npc, pos);
            return npc;
        }

        public NPC SpawnAt(IWorldZoneLocation loc, bool hugGround = false)
        {
            NPC npc = this.Create(loc.Map.DifficultyIndex);
            npc.Zone = loc.GetZone();
            Vector3 position = loc.Position;
            if (hugGround && this.InhabitType == InhabitType.Ground)
                position.Z = loc.Map.Terrain.GetGroundHeightUnderneath(position);
            loc.Map.AddObject((WorldObject) npc, position);
            return npc;
        }

        public NPC SpawnAt(IWorldLocation loc, bool hugGround = false)
        {
            NPC npc = this.Create(loc.Map.DifficultyIndex);
            Vector3 position = loc.Position;
            if (hugGround && this.InhabitType == InhabitType.Ground)
                position.Z = loc.Map.Terrain.GetGroundHeightUnderneath(position);
            loc.Map.AddObject((WorldObject) npc, loc.Position);
            npc.Phase = loc.Phase;
            return npc;
        }

        public IBrain DefaultBrainCreator(NPC npc)
        {
            MobBrain mobBrain = new MobBrain(npc, this.IsIdle ? BrainState.Idle : BrainState.Roam);
            mobBrain.IsAggressive = this.IsAgressive;
            return (IBrain) mobBrain;
        }

        internal void NotifyActivated(NPC npc)
        {
            NPCEntry.NPCHandler activated = this.Activated;
            if (activated == null)
                return;
            activated(npc);
        }

        internal void NotifyDeleted(NPC npc)
        {
            NPCEntry.NPCHandler deleted = this.Deleted;
            if (deleted == null)
                return;
            deleted(npc);
        }

        internal void NotifyInteracting(NPC npc, Character chr)
        {
            if (this.InteractionSpell != null)
                chr.SpellCast.TriggerSelf(this.InteractionSpell);
            Action<Character, NPC> interacting = this.Interacting;
            if (interacting == null)
                return;
            interacting(chr, npc);
        }

        internal bool NotifyBeforeDeath(NPC npc)
        {
            Func<NPC, bool> beforeDeath = this.BeforeDeath;
            if (beforeDeath != null)
                return beforeDeath(npc);
            return true;
        }

        internal void NotifyDied(NPC npc)
        {
            NPCEntry.NPCHandler died = this.Died;
            if (died == null)
                return;
            died(npc);
        }

        internal void NotifyLeveledChanged(NPC npc)
        {
            NPCEntry.NPCHandler levelChanged = this.LevelChanged;
            if (levelChanged == null)
                return;
            levelChanged(npc);
        }

        public void Dump(IndentTextWriter writer)
        {
            writer.WriteLine("{3}{0} (Id: {1}, {2})", (object) this.DefaultName, (object) this.Id, (object) this.NPCId,
                this.Rank != CreatureRank.Normal ? (object) (((int) this.Rank).ToString() + " ") : (object) "");
            if (!string.IsNullOrEmpty(this.DefaultTitle))
                writer.WriteLine("Title: " + this.DefaultTitle);
            if (this.Type != CreatureType.None)
                writer.WriteLine("Type: " + (object) this.Type);
            if (this.Family != null)
                writer.WriteLine("Family: " + (object) this.Family);
            if (this.TrainerEntry != null)
                writer.WriteLine("Trainer ");
            this.WriteFaction(writer);
            writer.WriteLine("Level: {0} - {1}", (object) this.MinLevel, (object) this.MaxLevel);
            writer.WriteLine("Health: {0} - {1}", (object) this.MinHealth, (object) this.MaxHealth);
            writer.WriteLineNotDefault<int>(this.MinMana, "Mana: {0} - {1}", (object) this.MinMana,
                (object) this.MaxMana);
            writer.WriteLineNotDefault<NPCFlags>(this.NPCFlags, "Flags: " + (object) this.NPCFlags);
            writer.WriteLineNotDefault<UnitDynamicFlags>(this.DynamicFlags,
                "DynamicFlags: " + (object) this.DynamicFlags);
            writer.WriteLineNotDefault<UnitFlags>(this.UnitFlags, "UnitFlags: " + (object) this.UnitFlags);
            writer.WriteLineNotDefault<UnitExtraFlags>(this.ExtraFlags,
                "ExtraFlags: " + string.Format("0x{0:X}", (object) this.ExtraFlags));
            writer.WriteLineNotDefault<int>(this.AttackTime + this.OffhandAttackTime,
                "AttackTime: " + (object) this.AttackTime, (object) ("Offhand: " + (object) this.OffhandAttackTime));
            writer.WriteLineNotDefault<int>(this.AttackPower, "AttackPower: " + (object) this.AttackPower);
            writer.WriteLineNotDefault<float>((float) ((double) this.MinDamage + (double) this.MaxDamage),
                "Damage: {0} - {1}", (object) this.MinDamage, (object) this.MaxDamage);
            List<string> collection = new List<string>(8);
            for (int index = 0; index < this.Resistances.Length; ++index)
            {
                int resistance = this.Resistances[index];
                if (resistance > 0)
                    collection.Add(string.Format("{0}: {1}", (object) (DamageSchool) index, (object) resistance));
            }

            if ((double) this.Scale != 1.0)
                writer.WriteLine("Scale: " + (object) this.Scale);
            float combatReach = this.GetRandomModel().CombatReach;
            float boundingRadius = this.GetRandomModel().BoundingRadius;
            writer.WriteLine("CombatReach: " + (object) combatReach);
            writer.WriteLine("BoundingRadius: " + (object) boundingRadius);
            writer.WriteLineNotDefault<int>(collection.Count, "Resistances: " + collection.ToString<string>(", "));
            writer.WriteLineNotDefault<uint>(this.MoneyDrop, "MoneyDrop: " + (object) this.MoneyDrop);
            writer.WriteLineNotDefault<InvisType>(this.InvisibilityType,
                "Invisibility: " + (object) this.InvisibilityType);
            writer.WriteLineNotDefault<AIMotionGenerationType>(this.MovementType,
                "MovementType: " + (object) this.MovementType);
            writer.WriteLineNotDefault<float>(
                (float) ((double) this.WalkSpeed + (double) this.RunSpeed + (double) this.FlySpeed),
                "Speeds - Walking: {0}, Running: {1}, Flying: {2} ", (object) this.WalkSpeed, (object) this.RunSpeed,
                (object) this.FlySpeed);
            Dictionary<SpellId, Spell> spells = this.Spells;
            if (spells != null && spells.Count > 0)
                writer.WriteLine("Spells: " + this.Spells.ToString<KeyValuePair<SpellId, Spell>>(", "));
            if (this.Equipment == null)
                return;
            writer.WriteLine("Equipment: {0}",
                (object) ((IEnumerable<Asda2ItemId>) this.Equipment.ItemIds)
                .Where<Asda2ItemId>((Func<Asda2ItemId, bool>) (id => id != (Asda2ItemId) 0))
                .ToString<Asda2ItemId>(", "));
        }

        private void WriteFaction(IndentTextWriter writer)
        {
            writer.WriteLineNotNull<Faction>(this.HordeFaction, "HordeFaction: " + (object) this.HordeFaction);
            writer.WriteLineNotNull<Faction>(this.AllianceFaction, "AllianceFaction: " + (object) this.AllianceFaction);
        }

        public override IWorldLocation[] GetInWorldTemplates()
        {
            return (IWorldLocation[]) this.SpawnEntries.ToArray();
        }

        public override string ToString()
        {
            return "Entry: " + this.DefaultName + " (Id: " + (object) this.Id + ")";
        }

        public static IEnumerable<NPCEntry> GetAllDataHolders()
        {
            return (IEnumerable<NPCEntry>) new NPCMgr.EntryIterator();
        }

        public delegate void NPCHandler(NPC npc);
    }
}