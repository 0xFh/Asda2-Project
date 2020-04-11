using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Paths;
using WCell.Core.Timers;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Groups;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Auctioneer;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.NPCs.Trainers;
using WCell.RealmServer.NPCs.Vendors;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Talents;
using WCell.RealmServer.Taxi;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
    /// <summary>
    /// TODO: Move everything NPC-related from UnitUpdates in here
    /// </summary>
    [Serializable]
    public class NPC : Unit, IQuestHolder, IEntity
    {
        public static float BossSpellCritChance = 5f;
        public Dictionary<object, double> Damages = new Dictionary<object, double>();
        private int[] m_damageDoneMods;
        private float[] m_damageDoneFactors;
        protected internal NPCSpawnPoint m_spawnPoint;
        protected NPCEntry m_entry;
        protected TimerEntry m_decayTimer;
        private string m_name;
        protected IPetRecord m_PetRecord;
        protected PetTalentCollection m_petTalents;
        protected ThreatCollection m_threatCollection;
        protected AIGroup m_group;

        /// <summary>The TaxiNode this TaxiVendor is associated with</summary>
        private PathNode vendorTaxiNode;

        private VendorEntry m_VendorEntry;

        /// <summary>The Trainer-specific details for this NPC.</summary>
        public TrainerEntry TrainerEntry;

        /// <summary>The Aucioneer-specific details of this NPC.</summary>
        public AuctioneerEntry AuctioneerEntry;

        private Character m_currentTamer;

        /// <summary>
        /// Modifies the damage for the given school by the given delta.
        /// Requires a call to <see cref="M:WCell.RealmServer.Modifiers.UnitUpdates.UpdateAllDamages(WCell.RealmServer.Entities.Unit)" /> afterwards.
        /// </summary>
        /// <param name="school"></param>
        /// <param name="delta"></param>
        protected internal override void AddDamageDoneModSilently(DamageSchool school, int delta)
        {
            if (this.m_damageDoneMods == null)
                this.m_damageDoneMods = new int[7];
            this.m_damageDoneMods[(int) school] += delta;
        }

        /// <summary>
        /// Modifies the damage for the given school by the given delta.
        /// Requires a call to <see cref="M:WCell.RealmServer.Modifiers.UnitUpdates.UpdateAllDamages(WCell.RealmServer.Entities.Unit)" /> afterwards.
        /// </summary>
        /// <param name="school"></param>
        /// <param name="delta"></param>
        protected internal override void RemoveDamageDoneModSilently(DamageSchool school, int delta)
        {
            if (this.m_damageDoneMods == null)
                return;
            this.m_damageDoneMods[(int) school] -= delta;
        }

        protected internal override void ModDamageDoneFactorSilently(DamageSchool school, float delta)
        {
            if (this.m_damageDoneFactors == null)
                this.m_damageDoneFactors = new float[7];
            this.m_damageDoneFactors[(int) school] += delta;
        }

        public override int GetDamageDoneMod(DamageSchool school)
        {
            int num = 0;
            if (this.IsHunterPet && this.m_master != null && school != DamageSchool.Physical)
            {
                int damageDoneMod = this.m_master.GetDamageDoneMod(school);
                num += (damageDoneMod * PetMgr.PetSpellDamageOfOwnerPercent + 50) / 100;
            }

            if (this.m_damageDoneMods != null)
                num += this.m_damageDoneMods[(int) school];
            return num;
        }

        public override float GetDamageDoneFactor(DamageSchool school)
        {
            if (this.m_damageDoneFactors == null)
                return 1f;
            return 1f + this.m_damageDoneFactors[(int) school];
        }

        public override float GetResiliencePct()
        {
            if (this.HasPlayerMaster)
                return (float) ((Character) this.m_master).GetCombatRating(CombatRating.MeleeResilience) / GameTables
                           .GetCRTable(CombatRating.MeleeResilience).GetMax<float>((uint) (this.Level - 1));
            return 0.0f;
        }

        protected internal override void UpdateStrength()
        {
            base.UpdateStrength();
            if (!(this.MainWeapon is GenericWeapon))
                return;
            this.UpdatePetDamage((GenericWeapon) this.MainWeapon);
            this.UpdateMainDamage();
        }

        protected internal override void UpdateStamina()
        {
            int num1 = this.StaminaBuffPositive - this.StaminaBuffNegative;
            if (this.IsHunterPet)
            {
                Character master = this.m_master as Character;
                if (master != null)
                {
                    PetLevelStatInfo petLevelStatInfo = this.Entry.GetPetLevelStatInfo(this.Level);
                    int num2 = (master.Stamina * PetMgr.PetStaminaOfOwnerPercent + 50) / 100;
                    if (petLevelStatInfo != null)
                        num2 += petLevelStatInfo.BaseStats[2];
                    this.m_baseStats[2] = num2;
                    num1 += num2;
                }
            }
            else
                num1 += this.GetBaseStatValue(StatType.Stamina);

            this.SetInt32((UpdateFieldId) UnitFields.STAT2, num1);
            this.UpdateMaxHealth();
        }

        private void UpdatePetDamage(GenericWeapon weapon)
        {
            int num = (this.Strength - 20) / 2;
            weapon.Damages[0].Minimum = (float) (num - num / 5);
            weapon.Damages[0].Maximum = (float) (num + num / 5);
        }

        internal void UpdateSize()
        {
            int level = this.Level;
            if (this.HasMaster && this.m_entry.Family != null)
            {
                if (level >= this.m_entry.Family.MaxScaleLevel)
                    this.ScaleX = this.m_entry.Family.MaxScale * this.m_entry.Scale;
                else
                    this.ScaleX =
                        (this.m_entry.Family.MinScale + (float) (this.m_entry.Family.MaxScaleLevel - level) *
                         this.m_entry.Family.ScaleStep) * this.m_entry.Scale;
            }
            else
                this.ScaleX = this.m_entry.Scale;
        }

        public int Expirience
        {
            get { return this.Entry.Expirience; }
            set { this.Entry.Expirience = value; }
        }

        public static NPC Create(NPCId id)
        {
            return NPCMgr.GetEntry(id).Create(uint.MaxValue);
        }

        protected internal NPC()
        {
            this.m_threatCollection = new ThreatCollection();
            this.m_auras = (AuraCollection) new NPCAuraCollection(this);
            this.m_spells = (SpellCollection) NPCSpellCollection.Obtain(this);
        }

        protected internal virtual void SetupNPC(NPCEntry entry, NPCSpawnPoint spawnPoint)
        {
            if (spawnPoint != null)
            {
                NPCSpawnEntry spawnEntry = spawnPoint.SpawnEntry;
                this.m_spawnPoint = spawnPoint;
                this.Phase = spawnEntry.Phase;
                this.m_orientation = spawnEntry.Orientation;
                if (spawnEntry.DisplayIdOverride != 0U)
                    this.DisplayId = spawnEntry.DisplayIdOverride;
            }
            else
            {
                this.Phase = 1U;
                if (entry.SpawnEntries == null)
                    entry.SpawnEntries = new List<NPCSpawnEntry>(3);
                NPCSpawnEntry firstSpawnEntry = entry.FirstSpawnEntry;
            }

            this.GenerateId(entry.Id);
            this.SetEntry(entry);
        }

        public void SetEntry(NPCId entryId)
        {
            this.SetEntry(NPCMgr.GetEntry(entryId));
        }

        public void SetEntry(NPCEntry entry)
        {
            this.Entry = entry;
            if (this.m_spawnPoint == null || this.m_spawnPoint.SpawnEntry.DisplayIdOverride == 0U)
                this.Model = entry.ModelInfo;
            this.NativeDisplayId = this.DisplayId;
            if (this.m_brain == null)
            {
                this.m_brain = this.m_entry.BrainCreator(this);
                this.m_brain.IsRunning = true;
            }

            if (this.m_Movement == null)
                this.m_Movement = new Movement((Unit) this);
            this.Name = this.m_entry.DefaultName;
            this.NPCFlags = NPCFlags.None;
            this.UnitFlags = UnitFlags.None;
            this.DynamicFlags = UnitDynamicFlags.None;
            this.Class = ClassId.OHS;
            this.Race = RaceId.Human;
            this.YieldsXpOrHonor = entry.GeneratesXp;
            this.SheathType = SheathType.Melee;
            if (this.m_spawnPoint != null && this.m_spawnPoint.Map != null)
                this.Faction = this.DefaultFaction;
            if (this.Faction == null)
                this.Faction = entry.RandomFaction;
            this.m_runSpeed = entry.RunSpeed;
            this.m_walkSpeed = entry.WalkSpeed;
            this.m_walkBackSpeed = entry.WalkSpeed;
            Array.Copy((Array) entry.Resistances, (Array) this.m_baseResistances, this.m_baseResistances.Length);
            this.MainWeapon = this.m_entry.CreateMainHandWeapon();
            this.Model = this.m_entry.GetRandomModel();
            this.GossipMenu = this.m_spawnPoint == null || this.m_spawnPoint.SpawnEntry.DefaultGossip == null
                ? entry.DefaultGossip
                : this.m_spawnPoint.SpawnEntry.DefaultGossip;
            if (entry.Resistances != null)
            {
                this.IntMods[20] += entry.Resistances[0];
                this.IntMods[21] += entry.Resistances[1];
                this.FloatMods[17] += (float) entry.Resistances[2];
                this.FloatMods[18] += (float) entry.Resistances[3];
                this.FloatMods[22] += (float) entry.Resistances[4];
                this.FloatMods[19] += (float) entry.Resistances[5];
                this.FloatMods[20] += (float) entry.Resistances[6];
            }

            this.UpdateAsda2Defence();
            this.UpdateAsda2MagicDefence();
            this.UpdateFireResistence();
            this.UpdateClimateResistence();
            this.UpdateDarkResistence();
            this.UpdateLightResistence();
            this.UpdateWaterResistence();
            this.UpdateEarthResistence();
            this.PowerType = PowerType.Mana;
            this.SetBaseStat(StatType.Strength, 1, false);
            this.SetBaseStat(StatType.Agility, 1, false);
            this.SetBaseStat(StatType.Intellect, 1, false);
            this.SetBaseStat(StatType.Stamina, 1, false);
            this.SetBaseStat(StatType.Spirit, 1, false);
            int randomHealth = entry.GetRandomHealth();
            this.SetInt32((UpdateFieldId) UnitFields.MAXHEALTH, randomHealth);
            this.SetInt32((UpdateFieldId) UnitFields.BASE_HEALTH, randomHealth);
            if (this.m_entry.IsDead || this.m_spawnPoint == null || !this.m_spawnPoint.SpawnEntry.IsDead)
                this.SetInt32((UpdateFieldId) UnitFields.HEALTH, randomHealth);
            else if (this.m_entry.Regenerates)
            {
                this.Regenerates = true;
                this.HealthRegenPerTickNoCombat = Math.Max(this.m_entry.MaxHealth / 10, 1);
            }

            int randomMana = entry.GetRandomMana();
            if (randomMana == 0)
            {
                this.SetInt32((UpdateFieldId) UnitFields.MAXPOWER1, 1);
                this.SetInt32((UpdateFieldId) UnitFields.BASE_MANA, 1);
            }
            else
            {
                this.SetInt32((UpdateFieldId) UnitFields.MAXPOWER1, randomMana);
                this.SetInt32((UpdateFieldId) UnitFields.BASE_MANA, randomMana);
            }

            this.SetInt32((UpdateFieldId) UnitFields.POWER1, randomMana);
            this.Power = randomMana;
            this.HoverHeight = entry.HoverHeight;
            this.PowerCostMultiplier = 1f;
            if (this.PowerType == PowerType.Mana)
                this.ManaRegenPerTickInterrupted = 20;
            this.UpdateUnitState();
            if (this.m_entry.InhabitType.HasFlag((Enum) InhabitType.Air))
                ++this.Flying;
            this.AddStandardEquipment();
            if (this.m_mainWeapon != GenericWeapon.Peace)
                this.IncMeleePermissionCounter();
            if (this.IsImmovable)
                this.InitImmovable();
            this.Level = entry.GetRandomLevel();
            this.AddMessage(new Action(this.UpdateSpellRanks));
        }

        /// <summary>Update Unit-fields, according to given flags</summary>
        private void UpdateUnitState()
        {
            UnitFlags unitFlags = this.UnitFlags;
            if (unitFlags == UnitFlags.None)
                return;
            if (unitFlags.HasAnyFlag(UnitFlags.SelectableNotAttackable | UnitFlags.SelectableNotAttackable_2 |
                                     UnitFlags.NotAttackable | UnitFlags.SelectableNotAttackable_3))
                ++this.Invulnerable;
            if (unitFlags.HasAnyFlag(UnitFlags.NotSelectable))
                this.IsEvading = true;
            if (unitFlags.HasFlag((Enum) UnitFlags.Combat))
                this.IsInCombat = true;
            if (unitFlags.HasFlag((Enum) UnitFlags.Confused))
                this.IncMechanicCount(SpellMechanic.Disoriented, false);
            if (unitFlags.HasFlag((Enum) UnitFlags.Disarmed))
                this.IncMechanicCount(SpellMechanic.Disarmed, false);
            if (unitFlags.HasFlag((Enum) UnitFlags.Stunned))
                ++this.Stunned;
            if (unitFlags.HasFlag((Enum) UnitFlags.Silenced))
                this.IncMechanicCount(SpellMechanic.Silenced, false);
            if (!unitFlags.HasFlag((Enum) UnitFlags.Passive))
                return;
            this.HasPermissionToMove = false;
        }

        private void InitImmovable()
        {
            this.m_Movement.MayMove = false;
            if (!this.HasSpells || this.Spells.Count != 1)
                return;
            Spell spell = this.Spells.First<Spell>();
            if (!spell.IsAreaAura)
                return;
            int num;
            this.AddMessage((Action) (() => num = (int) this.SpellCast.Start(spell, true)));
        }

        private void AddStandardEquipment()
        {
            NPCEquipmentEntry equipment =
                this.m_spawnPoint == null || this.m_spawnPoint.SpawnEntry == null ||
                this.m_spawnPoint.SpawnEntry.Equipment == null
                    ? this.m_entry.Equipment
                    : this.m_spawnPoint.SpawnEntry.Equipment;
            if (equipment == null)
                return;
            this.SetEquipment(equipment);
        }

        private void AddAddonData(NPCAddonData data)
        {
            this.SetUInt32((UpdateFieldId) UnitFields.BYTES_0, data.Bytes);
            this.SetUInt32((UpdateFieldId) UnitFields.BYTES_2, data.Bytes2);
            this.EmoteState = data.EmoteState;
            if (data.MountModelId == 0U)
                return;
            this.Mount(data.MountModelId);
        }

        public NPCEntry Entry
        {
            get { return this.m_entry; }
            private set
            {
                this.m_entry = value;
                this.EntryId = value.Id;
            }
        }

        public override ObjectTemplate Template
        {
            get { return (ObjectTemplate) this.Entry; }
        }

        public override string Name
        {
            get { return this.m_name; }
            set
            {
                this.m_name = value;
                this.PetNameTimestamp = Utility.GetEpochTime();
            }
        }

        internal void SetName(string name, uint timeStamp)
        {
            this.m_name = name;
            this.PetNameTimestamp = timeStamp;
        }

        /// <summary>Uncontrolled NPCs that are not summoned can evade</summary>
        public bool CanEvade
        {
            get
            {
                if (!this.m_Map.CanNPCsEvade || this.m_spawnPoint == null)
                    return false;
                if (this.m_master != this)
                    return this.m_master == null;
                return true;
            }
        }

        public bool IsImmovable
        {
            get
            {
                if (this.m_entry.Type != CreatureType.Totem)
                    return this.m_entry.Type == CreatureType.None;
                return true;
            }
        }

        public override UpdatePriority UpdatePriority
        {
            get { return this.IsAreaActive ? UpdatePriority.HighPriority : UpdatePriority.Inactive; }
        }

        public override Faction DefaultFaction
        {
            get
            {
                if (this.Map != null)
                    return this.m_entry.GetFaction(this.Map.OwningFaction);
                return this.m_entry.HordeFaction;
            }
        }

        public ThreatCollection ThreatCollection
        {
            get { return this.m_threatCollection; }
        }

        protected internal override void OnDamageAction(IDamageAction action)
        {
            Character attacker = action.Attacker as Character;
            if (attacker != null)
            {
                WCell.RealmServer.Groups.Group group = attacker.Group;
                int actualDamage = action.ActualDamage;
                if (group != null)
                {
                    if (this.Map.DefenceTownEvent != null)
                    {
                        if (this.Map.DefenceTownEvent.Damages.ContainsKey((object) group))
                        {
                            Dictionary<object, long> damages;
                            object index;
                            (damages = this.Map.DefenceTownEvent.Damages)[
                                    (object) (WCell.RealmServer.Groups.Group) (index = (object) group)] =
                                damages[index] + (long) actualDamage;
                        }
                        else
                            this.Map.DefenceTownEvent.Damages.Add((object) group, (long) actualDamage);
                    }

                    if (this.Damages.ContainsKey((object) group))
                    {
                        Dictionary<object, double> damages;
                        object index;
                        (damages = this.Damages)[(object) (WCell.RealmServer.Groups.Group) (index = (object) group)] =
                            damages[index] + (double) actualDamage;
                    }
                    else
                        this.Damages.Add((object) group, (double) actualDamage);
                }

                if (this.Map.DefenceTownEvent != null)
                {
                    if (this.Map.DefenceTownEvent.Damages.ContainsKey((object) attacker))
                    {
                        Dictionary<object, long> damages;
                        object index;
                        (damages = this.Map.DefenceTownEvent.Damages)[(object) (Character) (index = (object) attacker)]
                            = damages[index] + (long) actualDamage;
                    }
                    else
                        this.Map.DefenceTownEvent.Damages.Add((object) attacker, (long) actualDamage);
                }

                if (this.Damages.ContainsKey((object) attacker))
                {
                    Dictionary<object, double> damages;
                    object index;
                    (damages = this.Damages)[(object) (Character) (index = (object) attacker)] =
                        damages[index] + (double) actualDamage;
                }
                else
                    this.Damages.Add((object) attacker, (double) actualDamage);
            }

            base.OnDamageAction(action);
        }

        /// <summary>Gets a random Unit from those who generated Threat</summary>
        public Unit GetRandomAttacker()
        {
            int num1 = Utility.Random(this.m_threatCollection.AggressorPairs.Count);
            int num2 = 0;
            foreach (KeyValuePair<Unit, int> aggressorPair in this.m_threatCollection.AggressorPairs)
            {
                if (!this.CanBeAggroedBy(aggressorPair.Key))
                    --num1;
                if (num2++ >= num1)
                    return aggressorPair.Key;
            }

            return (Unit) null;
        }

        /// <summary>The AIGroup this NPC currently belongs to.</summary>
        public AIGroup Group
        {
            get { return this.m_group; }
            set
            {
                if (this.m_group == value)
                    return;
                this.m_brain.OnGroupChange(value);
                this.m_group = value;
                this.m_threatCollection.Group = value;
            }
        }

        public NPCSpellCollection NPCSpells
        {
            get { return (NPCSpellCollection) this.m_spells; }
        }

        public override SpellCollection Spells
        {
            get { return this.m_spells; }
        }

        public override NPCSpawnPoint SpawnPoint
        {
            get { return this.m_spawnPoint; }
        }

        public NPCSpawnEntry SpawnEntry
        {
            get
            {
                if (this.m_spawnPoint == null)
                    return (NPCSpawnEntry) null;
                return this.m_spawnPoint.SpawnEntry;
            }
        }

        public override LinkedList<WaypointEntry> Waypoints
        {
            get
            {
                if (this.m_spawnPoint == null)
                    return (LinkedList<WaypointEntry>) null;
                return this.m_spawnPoint.SpawnEntry.Waypoints;
            }
        }

        public override ObjectTypeCustom CustomType
        {
            get { return ObjectTypeCustom.Object | ObjectTypeCustom.Unit | ObjectTypeCustom.NPC; }
        }

        /// <summary>
        /// An NPC is decaying after it died and was looted empty.
        /// Setting this to true, starts or resets the decay-timer.
        /// Once the NPC decayed, it will be deleted.
        /// </summary>
        public bool IsDecaying
        {
            get { return this.m_decayTimer != null; }
            set
            {
                if (!value && this.IsDecaying)
                {
                    this.StopDecayTimer();
                }
                else
                {
                    if (!value || this.IsDecaying)
                        return;
                    this.RemainingDecayDelayMillis = this.m_entry.DefaultDecayDelayMillis;
                }
            }
        }

        public override uint LootMoney
        {
            get { return this.m_entry.MoneyDrop; }
        }

        /// <summary>
        /// Remaining time until the NPC decay (or 0 if already decayed or decaying did not start yet).
        /// Deactivates the timer if set to a value smaller or equal to 0
        /// </summary>
        /// <remarks>Requires map-context</remarks>
        public int RemainingDecayDelayMillis
        {
            get
            {
                if (this.m_decayTimer == null)
                    return 0;
                return this.m_decayTimer.RemainingInitialDelayMillis;
            }
            set
            {
                if (value <= 0)
                {
                    this.StopDecayTimer();
                }
                else
                {
                    if (this.m_decayTimer == null)
                        this.m_decayTimer = new TimerEntry(new Action<int>(this.DecayNow));
                    this.m_decayTimer.Start(value);
                }
            }
        }

        public IRealmClient Client
        {
            get
            {
                if (this.m_master != null && this.m_master is Character)
                    return ((Character) this.m_master).Client;
                return (IRealmClient) null;
            }
        }

        public override int StaminaWithoutHealthContribution
        {
            get
            {
                if (this.IsHunterPet)
                    return this.GetBaseStatValue(StatType.Stamina);
                return 0;
            }
        }

        public override bool IsRegenerating
        {
            get
            {
                if (this.IsAreaActive)
                    return base.IsRegenerating;
                return false;
            }
        }

        public override uint DisplayId
        {
            get { return base.DisplayId; }
            set
            {
                base.DisplayId = value;
                if (!this.IsActivePet || !(this.m_master is Character))
                    return;
                ((Character) this.m_master).GroupUpdateFlags |= GroupUpdateFlags.PetDisplayId;
            }
        }

        public override int Health
        {
            get { return base.Health; }
            set
            {
                base.Health = value;
                if (this.m_PetRecord == null || !(this.m_master is Character))
                    return;
                ((Character) this.m_master).GroupUpdateFlags |= GroupUpdateFlags.PetHealth;
            }
        }

        public override int MaxHealth
        {
            get { return base.MaxHealth; }
            internal set
            {
                base.MaxHealth = value;
                if (this.m_PetRecord == null || !(this.m_master is Character))
                    return;
                ((Character) this.m_master).GroupUpdateFlags |= GroupUpdateFlags.PetMaxHealth;
            }
        }

        public override int Power
        {
            get { return base.Power; }
            set
            {
                base.Power = value;
                if (this.m_PetRecord == null || !(this.m_master is Character))
                    return;
                ((Character) this.m_master).GroupUpdateFlags |= GroupUpdateFlags.PetPower;
            }
        }

        public override PowerType PowerType
        {
            get { return base.PowerType; }
            set
            {
                base.PowerType = value;
                if (this.m_PetRecord == null || !(this.m_master is Character))
                    return;
                ((Character) this.m_master).GroupUpdateFlags |=
                    GroupUpdateFlags.PetPowerType | GroupUpdateFlags.PetPower | GroupUpdateFlags.PetMaxPower;
            }
        }

        public override int MaxPower
        {
            get { return base.MaxPower; }
            internal set
            {
                base.MaxPower = value;
                if (this.m_PetRecord == null || !(this.m_master is Character))
                    return;
                ((Character) this.m_master).GroupUpdateFlags |= GroupUpdateFlags.PetMaxPower;
            }
        }

        /// <summary>Sets this NPC's equipment to the given entry</summary>
        public void SetEquipment(NPCEquipmentEntry equipment)
        {
            for (int index = 0; index < equipment.ItemIds.Length; ++index)
            {
                Asda2ItemId itemId = equipment.ItemIds[index];
                if (itemId != (Asda2ItemId) 0)
                {
                    ItemTemplate template = ItemMgr.GetTemplate(itemId);
                    if (template != null)
                        this.SheathType = template.SheathType;
                    this.SetUInt32((UpdateFieldId) ((UnitFields) (56 + index)), (uint) itemId);
                }
            }
        }

        public void SetMainWeaponVisual(Asda2ItemId item)
        {
            this.SetUInt32((UpdateFieldId) UnitFields.VIRTUAL_ITEM_SLOT_ID, (uint) item);
        }

        public void SetOffhandWeaponVisual(Asda2ItemId item)
        {
            this.SetUInt32((UpdateFieldId) UnitFields.VIRTUAL_ITEM_SLOT_ID_2, (uint) item);
        }

        public void SetRangedWeaponVisual(Asda2ItemId item)
        {
            this.SetUInt32((UpdateFieldId) UnitFields.VIRTUAL_ITEM_SLOT_ID_3, (uint) item);
        }

        /// <summary>
        /// NPCs only have their default items which may always be used, so no invalidation
        /// takes place.
        /// </summary>
        protected override IAsda2Weapon GetOrInvalidateItem(InventorySlotType slot)
        {
            if (slot == InventorySlotType.WeaponMainHand)
                return this.m_entry.CreateMainHandWeapon();
            return (IAsda2Weapon) null;
        }

        public override float GetCritChance(DamageSchool school)
        {
            float num = this.m_entry.Rank > CreatureRank.Normal ? NPC.BossSpellCritChance : 0.0f;
            if (this.m_CritMods != null)
                num += (float) this.m_CritMods[(int) school];
            return num;
        }

        public override NPC SpawnMinion(NPCEntry entry, ref Vector3 position, int durationMillis)
        {
            NPC npc = base.SpawnMinion(entry, ref position, durationMillis);
            if (this.Group == null)
                this.Group = new AIGroup(this);
            this.Group.Add(npc);
            return npc;
        }

        protected override bool OnBeforeDeath()
        {
            if (this.m_entry.NotifyBeforeDeath(this))
                return true;
            if (this.Health == 0)
                this.Health = 1;
            return false;
        }

        protected override void OnDeath()
        {
            if (this.m_brain != null)
                this.m_brain.IsRunning = false;
            if (this.m_Map != null || this != null)
            {
                ICollection<IRealmClient> rcvrs = this.GetNearbyClients<NPC>(false);
                Character triggerChar = this.CalcLooter();
                Asda2Loot loot = (Asda2Loot) null;
                if (triggerChar != null && this.m_Map.DefenceTownEvent == null)
                {
                    loot = Asda2LootMgr.GetOrCreateLoot((IAsda2Lootable) this, triggerChar, Asda2LootEntryType.Npc);
                    if (loot != null && this.Template != null)
                        loot.MonstrId = new short?((short) this.Template.Id);
                }

                this.Loot = (Asda2Loot) null;
                Map map = this.m_Map;
                this.m_Map.CallDelayed(this.LastDamageDelay, (Action) (() =>
                {
                    Asda2CombatHandler.SendMostrDeadToAreaResponse(rcvrs, (short) this.UniqIdOnMap,
                        (short) this.Asda2Position.X, (short) this.Asda2Position.Y);
                    map.OnNPCDied(this);
                    Character chr = this.LastKiller as Character;
                    if (loot != null && loot.Lootable is NPC)
                    {
                        NPC lootable = (NPC) loot.Lootable;
                        if (lootable != null && lootable.Map != null)
                            lootable.Map.SpawnLoot(loot);
                    }

                    if (this.m_entry != null && this.Entry != null && this.Template != null)
                    {
                        if (this.Entry.Rank >= CreatureRank.Boss)
                        {
                            if (this.LastKiller != null)
                            {
                                AchievementProgressRecord progressRecord =
                                    chr.Achievements.GetOrCreateProgressRecord(22U);
                                switch (++progressRecord.Counter)
                                {
                                    case 500:
                                        chr.Map.CallDelayed(500,
                                            (Action) (() => chr.DiscoverTitle(Asda2TitleId.Boss154)));
                                        break;
                                    case 1000:
                                        chr.Map.CallDelayed(500, (Action) (() => chr.GetTitle(Asda2TitleId.Boss154)));
                                        break;
                                }

                                progressRecord.SaveAndFlush();
                                ChatMgr.SendGlobalMessageResponse(this.LastKiller.Name,
                                    ChatMgr.Asda2GlobalMessageType.HasDefeated, 0, (short) 0, (short) this.Template.Id);
                            }

                            if (chr != null)
                                chr.GuildPoints +=
                                    this.m_entry.MinLevel * CharacterFormulas.BossKillingGuildPointsPerLevel;
                        }
                        else
                        {
                            if (chr != null && chr.Level < this.m_entry.MinLevel + 3)
                                chr.GuildPoints += CharacterFormulas.MobKillingGuildPoints;
                            if (chr != null)
                            {
                                AchievementProgressRecord progressRecord =
                                    chr.Achievements.GetOrCreateProgressRecord(0U);
                                switch (++progressRecord.Counter)
                                {
                                    case 50:
                                        chr.Map.CallDelayed(500,
                                            (Action) (() => chr.DiscoverTitle(Asda2TitleId.Hunter150)));
                                        break;
                                    case 100:
                                        chr.Map.CallDelayed(500, (Action) (() => chr.GetTitle(Asda2TitleId.Hunter150)));
                                        break;
                                    case 500:
                                        chr.Map.CallDelayed(500,
                                            (Action) (() => chr.DiscoverTitle(Asda2TitleId.Exterminator151)));
                                        break;
                                    case 1000:
                                        chr.Map.CallDelayed(500,
                                            (Action) (() => chr.GetTitle(Asda2TitleId.Exterminator151)));
                                        break;
                                    case 5000:
                                        chr.Map.CallDelayed(500,
                                            (Action) (() => chr.DiscoverTitle(Asda2TitleId.Slayer152)));
                                        break;
                                    case 10000:
                                        chr.Map.CallDelayed(500, (Action) (() => chr.GetTitle(Asda2TitleId.Slayer152)));
                                        break;
                                    case 50000:
                                        chr.Map.CallDelayed(500,
                                            (Action) (() => chr.DiscoverTitle(Asda2TitleId.Fanatic153)));
                                        break;
                                    case 100000:
                                        chr.Map.CallDelayed(500,
                                            (Action) (() => chr.GetTitle(Asda2TitleId.Fanatic153)));
                                        break;
                                }

                                progressRecord.SaveAndFlush();
                            }
                        }

                        this.m_entry.NotifyDied(this);
                    }

                    this.EnterFinalState();
                }));
            }

            if (this.m_currentTamer != null)
            {
                PetHandler.SendTameFailure((IPacketReceiver) this.m_currentTamer, TameFailReason.TargetDead);
                this.CurrentTamer.SpellCast.Cancel(SpellFailedReason.Ok);
            }

            if (this.m_spawnPoint == null)
                return;
            this.m_spawnPoint.SignalSpawnlingDied(this);
        }

        private Character CalcLooter()
        {
            try
            {
                if (this.Damages == null || this.Damages.Count == 0)
                    return (Character) null;
                List<KeyValuePair<object, double>> list = this.Damages
                    .OrderByDescending<KeyValuePair<object, double>, double
                    >((Func<KeyValuePair<object, double>, double>) (d => d.Value))
                    .Take<KeyValuePair<object, double>>(CharacterFormulas.MaxDamagersDetailCount)
                    .ToList<KeyValuePair<object, double>>();
                object key1 = list.FirstOrDefault<KeyValuePair<object, double>>().Key;
                Character character = (Character) null;
                if (key1 is WCell.RealmServer.Groups.Group)
                    character = (key1 as WCell.RealmServer.Groups.Group).Leader.Character;
                if (key1 is Character)
                    character = key1 as Character;
                if (this.Entry.Rank >= CreatureRank.Boss)
                {
                    this.SendMessageToArea(
                        string.Format("Top {0} damagers are : ", (object) CharacterFormulas.MaxDamagersDetailCount),
                        Color.MediumVioletRed);
                    int num1 = 1;
                    AchievementProgressRecord globalProgressRecord =
                        AchievementProgressRecord.GetOrCreateGlobalProgressRecord(184U, 1U);
                    foreach (KeyValuePair<object, double> keyValuePair in list)
                    {
                        WCell.RealmServer.Groups.Group key2 = keyValuePair.Key as WCell.RealmServer.Groups.Group;
                        if (key2 != null)
                        {
                            this.SendMessageToArea(
                                string.Format("{0} Party [{1}] deal {2} dmg", (object) num1,
                                    (object) key2.Leader.Character.Name, (object) (int) keyValuePair.Value),
                                Color.GreenYellow);
                            foreach (GroupMember groupMember in key2)
                            {
                                GroupMember member = groupMember;
                                int num2 = 0;
                                if (member.Character != null && this.Damages.ContainsKey((object) member.Character))
                                    num2 = (int) this.Damages[(object) member.Character];
                                this.SendMessageToArea(
                                    string.Format("--- {0} deal {1} dmg", (object) member.Name, (object) num2),
                                    Color.LightGreen);
                                if (this.Name == "EvilDragonEnkidu470" && globalProgressRecord.Counter == 1U)
                                    member.Character.Map.CallDelayed(500,
                                        (Action) (() => member.Character.GetTitle(Asda2TitleId.DragonSlayer403)));
                            }
                        }
                        else
                        {
                            Character chr = keyValuePair.Key as Character;
                            if (this.Name == "EvilDragonEnkidu470" && globalProgressRecord.Counter == 1U)
                                chr.Map.CallDelayed(500, (Action) (() => chr.GetTitle(Asda2TitleId.DragonSlayer403)));
                            if (chr != null)
                                this.SendMessageToArea(
                                    string.Format("{2} Char [{0}] deal {1} dmg", (object) chr.Name,
                                        (object) (int) keyValuePair.Value, (object) num1), Color.GreenYellow);
                        }

                        ++num1;
                    }

                    if (this.Entry.NPCId == NPCId.EvilDragonEnkidu470)
                        ++globalProgressRecord.Counter;
                    globalProgressRecord.SaveAndFlush();
                }

                this.Damages.Clear();
                return character;
            }
            catch (NullReferenceException ex)
            {
                return (Character) null;
            }
        }

        protected internal override void OnResurrect()
        {
            base.OnResurrect();
            this.StopDecayTimer();
            this.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.Influenced |
                              UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation |
                              UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable |
                              UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 |
                              UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 |
                              UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight |
                              UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed |
                              UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted |
                              UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 |
                              UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
            this.MarkUpdate((UpdateFieldId) UnitFields.DYNAMIC_FLAGS);
            if (this.m_spawnPoint != null)
                this.m_spawnPoint.Pool.SpawnedObjects.Add(this);
            this.m_brain.IsRunning = true;
            this.m_brain.EnterDefaultState();
            this.m_brain.OnActivate();
            this.m_entry.NotifyActivated(this);
        }

        /// <summary>Checks whether this NPC is of the given type</summary>
        public bool CheckCreatureType(CreatureMask mask)
        {
            return mask.HasFlag((Enum) (CreatureMask) (1 << (int) (this.Entry.Type - 1 & (CreatureType) 31)));
        }

        protected internal override void OnEnterMap()
        {
            base.OnEnterMap();
            if (this.m_spawnPoint != null)
                this.m_spawnPoint.SignalSpawnlingActivated(this);
            int count = this.m_auras.Count;
            foreach (NPCTypeHandler instanceTypeHandler in this.m_entry.InstanceTypeHandlers)
            {
                if (instanceTypeHandler != null)
                    instanceTypeHandler(this);
            }

            if (this.m_brain != null)
                this.m_brain.OnActivate();
            this.m_entry.NotifyActivated(this);
            if (this.m_spawnPoint != null)
                this.m_spawnPoint.SpawnEntry.NotifySpawned(this);
            if (this.m_master == null)
                return;
            if (this.m_master.IsInWorld)
                this.m_master.OnMinionEnteredMap(this);
            else
                this.Delete();
        }

        protected internal override void OnLeavingMap()
        {
            if (this.IsAlive && this.m_spawnPoint != null)
                this.m_spawnPoint.SignalSpawnlingDied(this);
            if (this.m_master == null)
                return;
            if (this.m_master.IsInWorld)
                this.m_master.OnMinionLeftMap(this);
            else
                this.Delete();
        }

        /// <summary>Marks this NPC lootable (after NPC died)</summary>
        private void EnterLootableState()
        {
            this.FirstAttacker = (Unit) null;
            this.RemainingDecayDelayMillis = this.m_entry.DefaultDecayDelayMillis * 2;
            this.MarkUpdate((UpdateFieldId) UnitFields.DYNAMIC_FLAGS);
        }

        /// <summary>Marks this NPC non-lootable (after NPC was looted)</summary>
        private void EnterFinalState()
        {
            this.FirstAttacker = (Unit) null;
            this.RemainingDecayDelayMillis = this.m_entry.DefaultDecayDelayMillis;
            if (this.m_loot != null)
            {
                this.m_loot.ForceDispose();
                this.m_loot = (Asda2Loot) null;
            }

            this.Delete();
        }

        private void DecayNow(int delay)
        {
            if (this.m_loot != null)
            {
                this.m_loot.ForceDispose();
                this.m_loot = (Asda2Loot) null;
            }

            this.Delete();
        }

        protected internal override void DeleteNow()
        {
            this.m_Deleted = true;
            this.m_entry.NotifyDeleted(this);
            if (this.m_spawnPoint != null && this.m_spawnPoint.ActiveSpawnling == this)
                this.m_spawnPoint.SignalSpawnlingDied(this);
            this.m_auras.ClearWithoutCleanup();
            base.DeleteNow();
        }

        private void StopDecayTimer()
        {
            if (this.m_decayTimer == null)
                return;
            this.m_decayTimer.Stop();
            this.m_decayTimer = (TimerEntry) null;
        }

        public override float MaxAttackRange
        {
            get
            {
                float num = base.MaxAttackRange;
                if (this.m_spells != null && (double) this.NPCSpells.MaxCombatSpellRange > (double) num)
                    num = this.NPCSpells.MaxCombatSpellRange;
                return num;
            }
        }

        protected override void OnEnterCombat()
        {
            base.OnEnterCombat();
            if (this.m_target == null)
                return;
            this.m_threatCollection.AddNewIfNotExisted(this.m_target);
        }

        protected override void OnLeaveCombat()
        {
            base.OnLeaveCombat();
        }

        public override float AggroBaseRange
        {
            get { return this.m_entry.AggroBaseRange; }
        }

        /// <summary>Also sends a message to the Character, if not valid</summary>
        internal bool CheckVendorInteraction(Character chr)
        {
            if (chr.Map != this.m_Map ||
                !this.IsInRadiusSq((IHasPosition) chr, (float) NPCMgr.DefaultInteractionDistanceSq) ||
                !chr.CanSee((WorldObject) this))
            {
                WCell.RealmServer.Handlers.NPCHandler.SendNPCError((IPacketReceiver) chr, (IEntity) this,
                    VendorInventoryError.TooFarAway);
                return false;
            }

            if (!this.IsAlive)
            {
                WCell.RealmServer.Handlers.NPCHandler.SendNPCError((IPacketReceiver) chr, (IEntity) this,
                    VendorInventoryError.VendorDead);
                return false;
            }

            if (chr.IsAlive == this.IsSpiritHealer)
            {
                WCell.RealmServer.Handlers.NPCHandler.SendNPCError((IPacketReceiver) chr, (IEntity) this,
                    VendorInventoryError.YouDead);
                return false;
            }

            if (!chr.CanInteract || !this.CanInteract)
                return false;
            Reputation reputation = chr.Reputations.GetOrCreate(this.Faction.ReputationIndex);
            if (reputation == null || reputation.CanInteract)
                return true;
            WCell.RealmServer.Handlers.NPCHandler.SendNPCError((IPacketReceiver) chr, (IEntity) this,
                VendorInventoryError.BadRep);
            return false;
        }

        public override void Say(float radius, string message)
        {
            ChatMgr.SendMonsterMessage((WorldObject) this, ChatMsgType.MonsterSay, this.SpokenLanguage, message,
                radius);
        }

        public override void Say(float radius, string[] localizedMsgs)
        {
            ChatMgr.SendMonsterMessage((WorldObject) this, ChatMsgType.MonsterSay, this.SpokenLanguage, localizedMsgs,
                radius);
        }

        public override void Yell(float radius, string message)
        {
            ChatMgr.SendMonsterMessage((WorldObject) this, ChatMsgType.MonsterYell, this.SpokenLanguage, message,
                radius);
        }

        public override void Yell(float radius, string[] localizedMsgs)
        {
            ChatMgr.SendMonsterMessage((WorldObject) this, ChatMsgType.MonsterYell, this.SpokenLanguage, localizedMsgs,
                radius);
        }

        public override void Emote(float radius, string message)
        {
            ChatMgr.SendMonsterMessage((WorldObject) this, ChatMsgType.MonsterEmote, this.SpokenLanguage, message,
                radius);
        }

        public override void Emote(float radius, string[] localizedMsgs)
        {
            ChatMgr.SendMonsterMessage((WorldObject) this, ChatMsgType.MonsterEmote, this.SpokenLanguage, localizedMsgs,
                radius);
        }

        /// <summary>Yells to everyone within the map to hear</summary>
        /// <param name="message"></param>
        public void YellToMap(string[] messages)
        {
            this.Yell(-1f, messages);
        }

        /// <summary>Whether this unit is a TaxiVendor</summary>
        public bool IsTaxiVendor
        {
            get { return this.NPCFlags.HasFlag((Enum) NPCFlags.FlightMaster); }
        }

        /// <summary>The TaxiNode this TaxiVendor is associated with</summary>
        public PathNode VendorTaxiNode
        {
            get
            {
                if (!this.IsTaxiVendor)
                    return (PathNode) null;
                if (this.vendorTaxiNode == null)
                    this.vendorTaxiNode = TaxiMgr.GetNearestTaxiNode(this.Position);
                return this.vendorTaxiNode;
            }
            internal set { this.vendorTaxiNode = value; }
        }

        /// <summary>Whether this is a Banker</summary>
        public bool IsBanker
        {
            get { return this.NPCFlags.HasFlag((Enum) NPCFlags.Banker); }
        }

        /// <summary>Whether this is an InnKeeper</summary>
        public bool IsInnKeeper
        {
            get { return this.NPCFlags.HasFlag((Enum) NPCFlags.InnKeeper); }
        }

        /// <summary>
        /// The location to which a Character can bind to when talking to this NPC
        /// or null if this is not an InnKeeper.
        /// </summary>
        public NamedWorldZoneLocation BindPoint { get; internal set; }

        /// <summary>Whether this is a Vendor</summary>
        public bool IsVendor
        {
            get { return this.m_entry.IsVendor; }
        }

        /// <summary>
        /// A list of Items this Vendor has for sale.
        /// Returns <c>VendorItemEntry.EmptyList</c> if this is not a Vendor
        /// </summary>
        public List<VendorItemEntry> ItemsForSale
        {
            get
            {
                if (this.VendorEntry == null)
                    return VendorItemEntry.EmptyList;
                return this.VendorEntry.ItemsForSale;
            }
        }

        /// <summary>The Vendor-specific details for this NPC</summary>
        public VendorEntry VendorEntry
        {
            get { return this.m_VendorEntry; }
            set { this.m_VendorEntry = value; }
        }

        /// <summary>Whether this is a Stable Master</summary>
        public bool IsStableMaster
        {
            get { return this.NPCFlags.HasFlag((Enum) NPCFlags.StableMaster); }
        }

        /// <summary>Whether this NPC can issue Charters</summary>
        public bool IsPetitioner
        {
            get { return this.NPCFlags.HasFlag((Enum) NPCFlags.Petitioner); }
        }

        /// <summary>Whether this is a Tabard Vendor</summary>
        public bool IsTabardVendor
        {
            get { return this.NPCFlags.HasFlag((Enum) NPCFlags.TabardDesigner); }
        }

        /// <summary>Whether this NPC can issue Guild Charters</summary>
        public bool IsGuildPetitioner
        {
            get { return this.NPCFlags.HasAnyFlag(NPCFlags.Petitioner | NPCFlags.TabardDesigner); }
        }

        /// <summary>Whether this NPC can issue Arena Charters</summary>
        public bool IsArenaPetitioner
        {
            get { return this.NPCFlags.HasFlag((Enum) NPCFlags.Petitioner); }
        }

        /// <summary>Whether this NPC starts a quest (or multiple quests)</summary>
        public bool IsQuestGiver
        {
            get { return this.NPCFlags.HasFlag((Enum) NPCFlags.QuestGiver); }
            internal set
            {
                if (value)
                    this.NPCFlags |= NPCFlags.QuestGiver;
                else
                    this.NPCFlags &= ~NPCFlags.QuestGiver;
            }
        }

        /// <summary>
        /// All available Quest information, in case that this is a QuestGiver
        /// </summary>
        public QuestHolderInfo QuestHolderInfo
        {
            get { return this.m_entry.QuestHolderInfo; }
            internal set { this.m_entry.QuestHolderInfo = value; }
        }

        public bool CanGiveQuestTo(Character chr)
        {
            return this.CheckVendorInteraction(chr);
        }

        public void OnQuestGiverStatusQuery(Character chr)
        {
        }

        /// <summary>Whether this is a Trainer.</summary>
        public bool IsTrainer
        {
            get { return this.TrainerEntry != null; }
        }

        /// <summary>
        /// Whether this NPC can train the character in their specialty.
        /// </summary>
        /// <returns>True if able to train.</returns>
        public bool CanTrain(Character character)
        {
            if (this.IsTrainer)
                return this.TrainerEntry.CanTrain(character);
            return false;
        }

        /// <summary>Whether this is an Auctioneer.</summary>
        public bool IsAuctioneer
        {
            get { return this.AuctioneerEntry != null; }
        }

        /// <summary>
        /// The Character that currently tries to tame this Creature (or null if not being tamed)
        /// </summary>
        public Character CurrentTamer
        {
            get { return this.m_currentTamer; }
            set
            {
                if (value == this.m_currentTamer)
                    return;
                this.m_currentTamer = value;
            }
        }

        /// <summary>
        /// We were controlled and reject the Controller.
        /// Does nothing if not controlled.
        /// </summary>
        public void RejectMaster()
        {
            if (this.m_master != null)
            {
                this.SetEntityId((UpdateFieldId) UnitFields.SUMMONEDBY, EntityId.Zero);
                this.SetEntityId((UpdateFieldId) UnitFields.CHARMEDBY, EntityId.Zero);
                this.Master = (Unit) null;
            }

            if (this.m_PetRecord == null)
                return;
            this.DeletePetRecord();
        }

        public override uint GetLootId(Asda2LootEntryType lootType)
        {
            if (lootType == Asda2LootEntryType.Npc)
                return this.Template.Id;
            return 0;
        }

        public override bool UseGroupLoot
        {
            get { return true; }
        }

        public bool HelperBossSummoned { get; set; }

        public override void OnFinishedLooting()
        {
            this.EnterFinalState();
        }

        public override int GetBasePowerRegen()
        {
            if (this.IsPlayerOwned)
                return RegenerationFormulas.GetPowerRegen((Unit) this);
            return this.BasePower / 50;
        }

        public override void Update(int dt)
        {
            if (this.m_decayTimer != null)
                this.m_decayTimer.Update(dt);
            if (this.m_target != null && this.CanMove)
                this.SetOrientationTowards((IHasPosition) this.m_target);
            base.Update(dt);
        }

        public override void Dispose(bool disposing)
        {
            if (this.m_Map == null)
                return;
            this.m_currentTamer = (Character) null;
            this.m_Map.UnregisterUpdatableLater((IUpdatable) this.m_decayTimer);
            base.Dispose(disposing);
        }

        public override string ToString()
        {
            return this.Name + " (ID: " + (object) this.EntryId + ", #" + (object) this.EntityId.Low + ")";
        }

        public IPetRecord PetRecord
        {
            get { return this.m_PetRecord; }
            set { this.m_PetRecord = value; }
        }

        public PermanentPetRecord PermanentPetRecord
        {
            get { return this.m_PetRecord as PermanentPetRecord; }
        }

        /// <summary>
        /// Whether this is the active pet of it's master (with an action bar)
        /// </summary>
        public bool IsActivePet
        {
            get
            {
                if (this.m_master is Character)
                    return ((Character) this.m_master).ActivePet == this;
                return false;
            }
        }

        /// <summary>
        /// Whether this is a Hunter pet.
        /// See http://www.wowwiki.com/Hunter_pet
        /// </summary>
        public bool IsHunterPet
        {
            get
            {
                if (this.m_PetRecord is PermanentPetRecord)
                    return this.PetTalentType == PetTalentType.End;
                return false;
            }
        }

        /// <summary>
        /// Validates the given name and sends the reason if it was not valid.
        /// </summary>
        /// <param name="chr">The pet's owner.</param>
        /// <param name="name">The proposed name.</param>
        /// <returns>True if the name is kosher.</returns>
        public PetNameInvalidReason TrySetPetName(Character chr, string name)
        {
            if (!PetMgr.InfinitePetRenames && !chr.GodMode && !this.PetState.HasFlag((Enum) PetState.CanBeRenamed))
                return PetNameInvalidReason.Invalid;
            PetNameInvalidReason nameInvalidReason = PetMgr.ValidatePetName(ref name);
            if (nameInvalidReason != PetNameInvalidReason.Ok)
                return nameInvalidReason;
            this.Name = name;
            this.PetState &= ~PetState.CanBeRenamed;
            return nameInvalidReason;
        }

        /// <summary>Makes this the pet of the given owner</summary>
        internal void MakePet(uint ownerId)
        {
            this.PetRecord = (IPetRecord) PetMgr.CreatePermanentPetRecord(this.Entry, ownerId);
            if (this.HasTalents || !this.IsHunterPet)
                return;
            this.m_petTalents = new PetTalentCollection(this);
        }

        /// <summary>
        /// Is called when this Pet became the ActivePet of a Character
        /// </summary>
        internal void OnBecameActivePet()
        {
            this.OnLevelChanged();
        }

        public bool CanEat(PetFoodType petFoodType)
        {
            if (this.m_entry.Family != null)
                return this.m_entry.Family.PetFoodMask.HasAnyFlag(petFoodType);
            return false;
        }

        public int GetHappinessGain(ItemTemplate food)
        {
            if (food == null)
                return 0;
            int num = this.Level - (int) food.Level;
            if (num > 0)
            {
                if (num < 16)
                    return PetMgr.MaxFeedPetHappinessGain;
                if (num < 26)
                    return PetMgr.MaxFeedPetHappinessGain / 2;
                if (num < 36)
                    return PetMgr.MaxFeedPetHappinessGain / 4;
            }
            else if (num > -16)
                return PetMgr.MaxFeedPetHappinessGain;

            return 0;
        }

        public PetTalentType PetTalentType
        {
            get
            {
                if (this.Entry.Family == null)
                    return PetTalentType.End;
                return this.Entry.Family.PetTalentType;
            }
        }

        public DateTime? LastTalentResetTime { get; set; }

        public bool HasTalents
        {
            get { return this.m_petTalents != null; }
        }

        /// <summary>Collection of all this Pet's Talents</summary>
        public override TalentCollection Talents
        {
            get { return (TalentCollection) this.m_petTalents; }
        }

        public int FreeTalentPoints
        {
            get { return (int) this.GetByte((UpdateFieldId) UnitFields.BYTES_1, 1); }
            set
            {
                if (!(this.m_PetRecord is PermanentPetRecord))
                    return;
                this.PermanentPetRecord.FreeTalentPoints = value;
                this.SetByte((UpdateFieldId) UnitFields.BYTES_1, 1, (byte) value);
                TalentHandler.SendTalentGroupList((TalentCollection) this.m_petTalents);
            }
        }

        public void UpdateFreeTalentPointsSilently(int delta)
        {
            if (this.m_PetRecord is PermanentPetRecord)
                this.PermanentPetRecord.FreeTalentPoints = delta;
            this.SetByte((UpdateFieldId) UnitFields.BYTES_1, 1, (byte) delta);
        }

        public void ResetFreeTalentPoints()
        {
            int num = 0;
            Character master = this.m_master as Character;
            if (master != null)
                num += master.PetBonusTalentPoints;
            this.FreeTalentPoints = num + PetMgr.GetPetTalentPointsByLevel(this.Level);
        }

        internal void UpdatePetData(IActivePetSettings settings)
        {
            settings.PetEntryId = this.Entry.NPCId;
            settings.PetHealth = this.Health;
            settings.PetPower = this.Power;
            settings.PetDuration = this.RemainingDecayDelayMillis;
            settings.PetSummonSpellId = this.CreationSpellId;
            this.UpdateTalentSpellRecords();
            this.m_PetRecord.UpdateRecord(this);
        }

        private void UpdateTalentSpellRecords()
        {
            List<PetTalentSpellRecord> talentSpellRecordList = new List<PetTalentSpellRecord>();
            foreach (Spell npcSpell in (SpellCollection) this.NPCSpells)
            {
                int remainingCooldownMillis = this.NPCSpells.GetRemainingCooldownMillis(npcSpell);
                PetTalentSpellRecord talentSpellRecord = new PetTalentSpellRecord()
                {
                    SpellId = npcSpell.Id,
                    CooldownUntil = new DateTime?(DateTime.Now.AddMilliseconds((double) remainingCooldownMillis))
                };
                talentSpellRecordList.Add(talentSpellRecord);
            }
        }

        /// <summary>
        /// Whether this NPC currently may gain levels and experience (usually only true for pets and certain kinds of minions)
        /// </summary>
        public bool MayGainExperience
        {
            get
            {
                if (this.IsHunterPet)
                    return this.PetExperience < this.NextPetLevelExperience;
                return false;
            }
        }

        public bool MayGainLevels
        {
            get
            {
                if (this.HasPlayerMaster)
                    return this.Level <= this.MaxLevel;
                return false;
            }
        }

        public override int MaxLevel
        {
            get
            {
                if (this.HasPlayerMaster)
                    return this.m_master.Level;
                return base.MaxLevel;
            }
        }

        internal bool TryLevelUp()
        {
            return false;
        }

        protected override void OnLevelChanged()
        {
            if (this.HasPlayerMaster)
                this.AddMessage((Action) (() =>
                {
                    this.UpdateSpellRanks();
                    this.UpdateSize();
                    int level = this.Level;
                    if (level >= PetMgr.MinPetTalentLevel)
                    {
                        if (this.m_petTalents == null)
                            this.m_petTalents = new PetTalentCollection(this);
                        int num = this.Talents.GetFreeTalentPointsForLevel(level);
                        if (num < 0)
                        {
                            if (!((Character) this.m_master).GodMode)
                                this.Talents.RemoveTalents(-num);
                            num = 0;
                        }

                        this.FreeTalentPoints = num;
                    }

                    PetLevelStatInfo petLevelStatInfo = this.m_entry.GetPetLevelStatInfo(level);
                    if (petLevelStatInfo == null)
                        return;
                    this.ModPetStatsPerLevel(petLevelStatInfo);
                    this.m_auras.ReapplyAllAuras();
                }));
            this.m_entry.NotifyLeveledChanged(this);
        }

        internal void ModPetStatsPerLevel(PetLevelStatInfo levelStatInfo)
        {
            this.BaseHealth = levelStatInfo.Health;
            if (this.PowerType == PowerType.Mana && levelStatInfo.Mana > 0)
                this.BasePower = levelStatInfo.Mana;
            for (StatType stat = StatType.Strength; stat < StatType.End; ++stat)
                this.SetBaseStat(stat, levelStatInfo.BaseStats[(int) stat]);
            this.UpdatePetResistance(DamageSchool.Physical);
            this.SetInt32((UpdateFieldId) UnitFields.HEALTH, this.MaxHealth);
        }

        private void UpdateSpellRanks()
        {
            if (this.m_entry.Spells == null)
                return;
            int level = this.Level;
            foreach (Spell spell in this.m_entry.Spells.Values)
            {
                if (spell.Level > level)
                    this.m_spells.Remove(spell);
                else
                    this.m_spells.AddSpell(spell);
            }
        }

        public override int GetUnmodifiedBaseStatValue(StatType stat)
        {
            if (this.HasPlayerMaster)
            {
                PetLevelStatInfo petLevelStatInfo = this.m_entry.GetPetLevelStatInfo(this.Level);
                if (petLevelStatInfo != null)
                    return petLevelStatInfo.BaseStats[(int) stat];
            }

            return base.GetUnmodifiedBaseStatValue(stat);
        }

        public void SetPetAttackMode(PetAttackMode mode)
        {
            if (this.m_PetRecord != null)
                this.m_PetRecord.AttackMode = mode;
            if (mode == PetAttackMode.Passive)
            {
                this.m_brain.IsAggressive = false;
                this.m_brain.DefaultState = BrainState.Follow;
            }
            else
            {
                this.m_brain.IsAggressive = mode == PetAttackMode.Aggressive;
                this.m_brain.DefaultState = BrainState.Guard;
            }

            this.m_brain.EnterDefaultState();
        }

        public void SetPetAction(PetAction action)
        {
            switch (action)
            {
                case PetAction.Stay:
                    this.HasPermissionToMove = false;
                    break;
                case PetAction.Follow:
                    this.HasPermissionToMove = true;
                    break;
                case PetAction.Attack:
                    this.HasPermissionToMove = true;
                    Unit target = this.m_master.Target;
                    if (target == null || !this.MayAttack((IFactionMember) target))
                        break;
                    this.m_threatCollection.Clear();
                    this.m_threatCollection[target] = int.MaxValue;
                    this.m_brain.State = BrainState.Combat;
                    break;
                case PetAction.Abandon:
                    if (!(this.m_master is Character))
                        break;
                    ((Character) this.m_master).ActivePet = (NPC) null;
                    break;
            }
        }

        /// <summary>Lets this Pet cast the given spell</summary>
        public void CastPetSpell(SpellId spellId, WorldObject target)
        {
            Spell readySpell = this.NPCSpells.GetReadySpell(spellId);
            SpellFailedReason reason;
            if (readySpell != null)
            {
                if (readySpell.HasTargets)
                    this.Target = this.m_master.Target;
                reason = readySpell.CheckCasterConstraints((Unit) this);
                if (reason == SpellFailedReason.Ok)
                {
                    SpellCast spellCast = this.SpellCast;
                    Spell spell = readySpell;
                    int num = 0;
                    WorldObject[] worldObjectArray;
                    if (target == null)
                        worldObjectArray = (WorldObject[]) null;
                    else
                        worldObjectArray = new WorldObject[1] {target};
                    reason = spellCast.Start(spell, num != 0, worldObjectArray);
                }
            }
            else
                reason = SpellFailedReason.NotReady;

            if (reason == SpellFailedReason.Ok || !(this.m_master is IPacketReceiver))
                return;
            PetHandler.SendCastFailed((IPacketReceiver) this.m_master, spellId, reason);
        }

        public uint[] BuildPetActionBar()
        {
            uint[] numArray1 = new uint[10];
            int num1 = 0;
            uint[] numArray2 = numArray1;
            int index1 = num1;
            int num2 = index1 + 1;
            int raw1 = (int) new PetActionEntry()
            {
                Action = PetAction.Attack,
                Type = PetActionType.SetAction
            }.Raw;
            numArray2[index1] = (uint) raw1;
            uint[] numArray3 = numArray1;
            int index2 = num2;
            int num3 = index2 + 1;
            int raw2 = (int) new PetActionEntry()
            {
                Action = PetAction.Follow,
                Type = PetActionType.SetAction
            }.Raw;
            numArray3[index2] = (uint) raw2;
            uint[] numArray4 = numArray1;
            int index3 = num3;
            int num4 = index3 + 1;
            int raw3 = (int) new PetActionEntry()
            {
                Action = PetAction.Stay,
                Type = PetActionType.SetAction
            }.Raw;
            numArray4[index3] = (uint) raw3;
            if (this.Entry.Spells != null)
            {
                Dictionary<SpellId, Spell>.Enumerator enumerator = this.Entry.Spells.GetEnumerator();
                for (byte index4 = 0; index4 < (byte) 4; ++index4)
                {
                    if (!enumerator.MoveNext())
                    {
                        numArray1[num4++] = new PetActionEntry()
                        {
                            Type = ((PetActionType) (8U + (uint) index4))
                        }.Raw;
                    }
                    else
                    {
                        KeyValuePair<SpellId, Spell> current = enumerator.Current;
                        PetActionEntry petActionEntry = new PetActionEntry();
                        petActionEntry.SetSpell(current.Key, PetActionType.DefaultSpellSetting);
                        numArray1[num4++] = petActionEntry.Raw;
                    }
                }
            }
            else
            {
                for (byte index4 = 0; index4 < (byte) 4; ++index4)
                    numArray1[num4++] = new PetActionEntry()
                    {
                        Type = ((PetActionType) (8U + (uint) index4))
                    }.Raw;
            }

            uint[] numArray5 = numArray1;
            int index5 = num4;
            int num5 = index5 + 1;
            int raw4 = (int) new PetActionEntry()
            {
                AttackMode = PetAttackMode.Aggressive,
                Type = PetActionType.SetMode
            }.Raw;
            numArray5[index5] = (uint) raw4;
            uint[] numArray6 = numArray1;
            int index6 = num5;
            int num6 = index6 + 1;
            int raw5 = (int) new PetActionEntry()
            {
                AttackMode = PetAttackMode.Defensive,
                Type = PetActionType.SetMode
            }.Raw;
            numArray6[index6] = (uint) raw5;
            uint[] numArray7 = numArray1;
            int index7 = num6;
            int num7 = index7 + 1;
            int raw6 = (int) new PetActionEntry()
            {
                AttackMode = PetAttackMode.Passive,
                Type = PetActionType.SetMode
            }.Raw;
            numArray7[index7] = (uint) raw6;
            return numArray1;
        }

        public uint GetTotemIndex()
        {
            if (this.CreationSpellId != SpellId.None)
            {
                Spell spell = SpellHandler.Get(this.CreationSpellId);
                if (spell != null && spell.TotemEffect != null)
                {
                    SpellSummonTotemHandler handler = spell.TotemEffect.SummonEntry.Handler as SpellSummonTotemHandler;
                    if (handler != null)
                        return handler.Index;
                }
            }

            return 0;
        }

        private void DeletePetRecord()
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage(new Action(this.m_PetRecord.Delete));
            this.m_PetRecord = (IPetRecord) null;
        }

        public uint[] BuildVehicleActionBar()
        {
            uint[] numArray = new uint[10];
            int num1 = 0;
            byte num2 = 0;
            if (this.Entry.Spells != null)
            {
                Dictionary<SpellId, Spell>.Enumerator enumerator = this.Entry.Spells.GetEnumerator();
                for (; num2 < (byte) 4; ++num2)
                {
                    if (!enumerator.MoveNext())
                    {
                        numArray[num1++] = new PetActionEntry()
                        {
                            Type = ((PetActionType) (8U + (uint) num2))
                        }.Raw;
                    }
                    else
                    {
                        KeyValuePair<SpellId, Spell> current = enumerator.Current;
                        PetActionEntry petActionEntry = new PetActionEntry();
                        if (current.Value.IsPassive)
                        {
                            SpellCast spellCast = this.SpellCast;
                            if (spellCast != null)
                                spellCast.TriggerSelf(current.Value);
                            petActionEntry.Type = (PetActionType) (8U + (uint) num2);
                        }
                        else
                            petActionEntry.SetSpell(current.Key, (PetActionType) (8U + (uint) num2));

                        numArray[num1++] = petActionEntry.Raw;
                    }
                }
            }

            for (; num2 < (byte) 10; ++num2)
                numArray[num1++] = new PetActionEntry()
                {
                    Type = ((PetActionType) (8U + (uint) num2))
                }.Raw;
            return numArray;
        }
    }
}