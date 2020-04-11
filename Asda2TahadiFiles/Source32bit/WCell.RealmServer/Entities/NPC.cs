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
      if(m_damageDoneMods == null)
        m_damageDoneMods = new int[7];
      m_damageDoneMods[(int) school] += delta;
    }

    /// <summary>
    /// Modifies the damage for the given school by the given delta.
    /// Requires a call to <see cref="M:WCell.RealmServer.Modifiers.UnitUpdates.UpdateAllDamages(WCell.RealmServer.Entities.Unit)" /> afterwards.
    /// </summary>
    /// <param name="school"></param>
    /// <param name="delta"></param>
    protected internal override void RemoveDamageDoneModSilently(DamageSchool school, int delta)
    {
      if(m_damageDoneMods == null)
        return;
      m_damageDoneMods[(int) school] -= delta;
    }

    protected internal override void ModDamageDoneFactorSilently(DamageSchool school, float delta)
    {
      if(m_damageDoneFactors == null)
        m_damageDoneFactors = new float[7];
      m_damageDoneFactors[(int) school] += delta;
    }

    public override int GetDamageDoneMod(DamageSchool school)
    {
      int num = 0;
      if(IsHunterPet && m_master != null && school != DamageSchool.Physical)
      {
        int damageDoneMod = m_master.GetDamageDoneMod(school);
        num += (damageDoneMod * PetMgr.PetSpellDamageOfOwnerPercent + 50) / 100;
      }

      if(m_damageDoneMods != null)
        num += m_damageDoneMods[(int) school];
      return num;
    }

    public override float GetDamageDoneFactor(DamageSchool school)
    {
      if(m_damageDoneFactors == null)
        return 1f;
      return 1f + m_damageDoneFactors[(int) school];
    }

    public override float GetResiliencePct()
    {
      if(HasPlayerMaster)
        return ((Character) m_master).GetCombatRating(CombatRating.MeleeResilience) / GameTables
                 .GetCRTable(CombatRating.MeleeResilience).GetMax((uint) (Level - 1));
      return 0.0f;
    }

    protected internal override void UpdateStrength()
    {
      base.UpdateStrength();
      if(!(MainWeapon is GenericWeapon))
        return;
      UpdatePetDamage((GenericWeapon) MainWeapon);
      this.UpdateMainDamage();
    }

    protected internal override void UpdateStamina()
    {
      int num1 = StaminaBuffPositive - StaminaBuffNegative;
      if(IsHunterPet)
      {
        Character master = m_master as Character;
        if(master != null)
        {
          PetLevelStatInfo petLevelStatInfo = Entry.GetPetLevelStatInfo(Level);
          int num2 = (master.Stamina * PetMgr.PetStaminaOfOwnerPercent + 50) / 100;
          if(petLevelStatInfo != null)
            num2 += petLevelStatInfo.BaseStats[2];
          m_baseStats[2] = num2;
          num1 += num2;
        }
      }
      else
        num1 += GetBaseStatValue(StatType.Stamina);

      SetInt32(UnitFields.STAT2, num1);
      UpdateMaxHealth();
    }

    private void UpdatePetDamage(GenericWeapon weapon)
    {
      int num = (Strength - 20) / 2;
      weapon.Damages[0].Minimum = num - num / 5;
      weapon.Damages[0].Maximum = num + num / 5;
    }

    internal void UpdateSize()
    {
      int level = Level;
      if(HasMaster && m_entry.Family != null)
      {
        if(level >= m_entry.Family.MaxScaleLevel)
          ScaleX = m_entry.Family.MaxScale * m_entry.Scale;
        else
          ScaleX =
            (m_entry.Family.MinScale + (m_entry.Family.MaxScaleLevel - level) *
             m_entry.Family.ScaleStep) * m_entry.Scale;
      }
      else
        ScaleX = m_entry.Scale;
    }

    public int Expirience
    {
      get { return Entry.Expirience; }
      set { Entry.Expirience = value; }
    }

    public static NPC Create(NPCId id)
    {
      return NPCMgr.GetEntry(id).Create(uint.MaxValue);
    }

    protected internal NPC()
    {
      m_threatCollection = new ThreatCollection();
      m_auras = new NPCAuraCollection(this);
      m_spells = NPCSpellCollection.Obtain(this);
    }

    protected internal virtual void SetupNPC(NPCEntry entry, NPCSpawnPoint spawnPoint)
    {
      if(spawnPoint != null)
      {
        NPCSpawnEntry spawnEntry = spawnPoint.SpawnEntry;
        m_spawnPoint = spawnPoint;
        Phase = spawnEntry.Phase;
        m_orientation = spawnEntry.Orientation;
        if(spawnEntry.DisplayIdOverride != 0U)
          DisplayId = spawnEntry.DisplayIdOverride;
      }
      else
      {
        Phase = 1U;
        if(entry.SpawnEntries == null)
          entry.SpawnEntries = new List<NPCSpawnEntry>(3);
        NPCSpawnEntry firstSpawnEntry = entry.FirstSpawnEntry;
      }

      GenerateId(entry.Id);
      SetEntry(entry);
    }

    public void SetEntry(NPCId entryId)
    {
      SetEntry(NPCMgr.GetEntry(entryId));
    }

    public void SetEntry(NPCEntry entry)
    {
      Entry = entry;
      if(m_spawnPoint == null || m_spawnPoint.SpawnEntry.DisplayIdOverride == 0U)
        Model = entry.ModelInfo;
      NativeDisplayId = DisplayId;
      if(m_brain == null)
      {
        m_brain = m_entry.BrainCreator(this);
        m_brain.IsRunning = true;
      }

      if(m_Movement == null)
        m_Movement = new Movement(this);
      Name = m_entry.DefaultName;
      NPCFlags = NPCFlags.None;
      UnitFlags = UnitFlags.None;
      DynamicFlags = UnitDynamicFlags.None;
      Class = ClassId.OHS;
      Race = RaceId.Human;
      YieldsXpOrHonor = entry.GeneratesXp;
      SheathType = SheathType.Melee;
      if(m_spawnPoint != null && m_spawnPoint.Map != null)
        Faction = DefaultFaction;
      if(Faction == null)
        Faction = entry.RandomFaction;
      m_runSpeed = entry.RunSpeed;
      m_walkSpeed = entry.WalkSpeed;
      m_walkBackSpeed = entry.WalkSpeed;
      Array.Copy(entry.Resistances, m_baseResistances, m_baseResistances.Length);
      MainWeapon = m_entry.CreateMainHandWeapon();
      Model = m_entry.GetRandomModel();
      GossipMenu = m_spawnPoint == null || m_spawnPoint.SpawnEntry.DefaultGossip == null
        ? entry.DefaultGossip
        : m_spawnPoint.SpawnEntry.DefaultGossip;
      if(entry.Resistances != null)
      {
        IntMods[20] += entry.Resistances[0];
        IntMods[21] += entry.Resistances[1];
        FloatMods[17] += entry.Resistances[2];
        FloatMods[18] += entry.Resistances[3];
        FloatMods[22] += entry.Resistances[4];
        FloatMods[19] += entry.Resistances[5];
        FloatMods[20] += entry.Resistances[6];
      }

      UpdateAsda2Defence();
      UpdateAsda2MagicDefence();
      UpdateFireResistence();
      UpdateClimateResistence();
      UpdateDarkResistence();
      UpdateLightResistence();
      UpdateWaterResistence();
      UpdateEarthResistence();
      PowerType = PowerType.Mana;
      SetBaseStat(StatType.Strength, 1, false);
      SetBaseStat(StatType.Agility, 1, false);
      SetBaseStat(StatType.Intellect, 1, false);
      SetBaseStat(StatType.Stamina, 1, false);
      SetBaseStat(StatType.Spirit, 1, false);
      int randomHealth = entry.GetRandomHealth();
      SetInt32(UnitFields.MAXHEALTH, randomHealth);
      SetInt32(UnitFields.BASE_HEALTH, randomHealth);
      if(m_entry.IsDead || m_spawnPoint == null || !m_spawnPoint.SpawnEntry.IsDead)
        SetInt32(UnitFields.HEALTH, randomHealth);
      else if(m_entry.Regenerates)
      {
        Regenerates = true;
        HealthRegenPerTickNoCombat = Math.Max(m_entry.MaxHealth / 10, 1);
      }

      int randomMana = entry.GetRandomMana();
      if(randomMana == 0)
      {
        SetInt32(UnitFields.MAXPOWER1, 1);
        SetInt32(UnitFields.BASE_MANA, 1);
      }
      else
      {
        SetInt32(UnitFields.MAXPOWER1, randomMana);
        SetInt32(UnitFields.BASE_MANA, randomMana);
      }

      SetInt32(UnitFields.POWER1, randomMana);
      Power = randomMana;
      HoverHeight = entry.HoverHeight;
      PowerCostMultiplier = 1f;
      if(PowerType == PowerType.Mana)
        ManaRegenPerTickInterrupted = 20;
      UpdateUnitState();
      if(m_entry.InhabitType.HasFlag(InhabitType.Air))
        ++Flying;
      AddStandardEquipment();
      if(m_mainWeapon != GenericWeapon.Peace)
        IncMeleePermissionCounter();
      if(IsImmovable)
        InitImmovable();
      Level = entry.GetRandomLevel();
      AddMessage(UpdateSpellRanks);
    }

    /// <summary>Update Unit-fields, according to given flags</summary>
    private void UpdateUnitState()
    {
      UnitFlags unitFlags = UnitFlags;
      if(unitFlags == UnitFlags.None)
        return;
      if(unitFlags.HasAnyFlag(UnitFlags.SelectableNotAttackable | UnitFlags.SelectableNotAttackable_2 |
                              UnitFlags.NotAttackable | UnitFlags.SelectableNotAttackable_3))
        ++Invulnerable;
      if(unitFlags.HasAnyFlag(UnitFlags.NotSelectable))
        IsEvading = true;
      if(unitFlags.HasFlag(UnitFlags.Combat))
        IsInCombat = true;
      if(unitFlags.HasFlag(UnitFlags.Confused))
        IncMechanicCount(SpellMechanic.Disoriented, false);
      if(unitFlags.HasFlag(UnitFlags.Disarmed))
        IncMechanicCount(SpellMechanic.Disarmed, false);
      if(unitFlags.HasFlag(UnitFlags.Stunned))
        ++Stunned;
      if(unitFlags.HasFlag(UnitFlags.Silenced))
        IncMechanicCount(SpellMechanic.Silenced, false);
      if(!unitFlags.HasFlag(UnitFlags.Passive))
        return;
      HasPermissionToMove = false;
    }

    private void InitImmovable()
    {
      m_Movement.MayMove = false;
      if(!HasSpells || Spells.Count != 1)
        return;
      Spell spell = Spells.First();
      if(!spell.IsAreaAura)
        return;
      int num;
      AddMessage(() => num = (int) SpellCast.Start(spell, true));
    }

    private void AddStandardEquipment()
    {
      NPCEquipmentEntry equipment =
        m_spawnPoint == null || m_spawnPoint.SpawnEntry == null ||
        m_spawnPoint.SpawnEntry.Equipment == null
          ? m_entry.Equipment
          : m_spawnPoint.SpawnEntry.Equipment;
      if(equipment == null)
        return;
      SetEquipment(equipment);
    }

    private void AddAddonData(NPCAddonData data)
    {
      SetUInt32(UnitFields.BYTES_0, data.Bytes);
      SetUInt32(UnitFields.BYTES_2, data.Bytes2);
      EmoteState = data.EmoteState;
      if(data.MountModelId == 0U)
        return;
      Mount(data.MountModelId);
    }

    public NPCEntry Entry
    {
      get { return m_entry; }
      private set
      {
        m_entry = value;
        EntryId = value.Id;
      }
    }

    public override ObjectTemplate Template
    {
      get { return Entry; }
    }

    public override string Name
    {
      get { return m_name; }
      set
      {
        m_name = value;
        PetNameTimestamp = Utility.GetEpochTime();
      }
    }

    internal void SetName(string name, uint timeStamp)
    {
      m_name = name;
      PetNameTimestamp = timeStamp;
    }

    /// <summary>Uncontrolled NPCs that are not summoned can evade</summary>
    public bool CanEvade
    {
      get
      {
        if(!m_Map.CanNPCsEvade || m_spawnPoint == null)
          return false;
        if(m_master != this)
          return m_master == null;
        return true;
      }
    }

    public bool IsImmovable
    {
      get
      {
        if(m_entry.Type != CreatureType.Totem)
          return m_entry.Type == CreatureType.None;
        return true;
      }
    }

    public override UpdatePriority UpdatePriority
    {
      get { return IsAreaActive ? UpdatePriority.HighPriority : UpdatePriority.Inactive; }
    }

    public override Faction DefaultFaction
    {
      get
      {
        if(Map != null)
          return m_entry.GetFaction(Map.OwningFaction);
        return m_entry.HordeFaction;
      }
    }

    public ThreatCollection ThreatCollection
    {
      get { return m_threatCollection; }
    }

    protected internal override void OnDamageAction(IDamageAction action)
    {
      Character attacker = action.Attacker as Character;
      if(attacker != null)
      {
        Group group = attacker.Group;
        int actualDamage = action.ActualDamage;
        if(group != null)
        {
          if(Map.DefenceTownEvent != null)
          {
            if(Map.DefenceTownEvent.Damages.ContainsKey(group))
            {
              Dictionary<object, long> damages;
              object index;
              (damages = Map.DefenceTownEvent.Damages)[
                  (Group) (index = group)] =
                damages[index] + actualDamage;
            }
            else
              Map.DefenceTownEvent.Damages.Add(group, actualDamage);
          }

          if(Damages.ContainsKey(group))
          {
            Dictionary<object, double> damages;
            object index;
            (damages = Damages)[(Group) (index = group)] =
              damages[index] + actualDamage;
          }
          else
            Damages.Add(group, actualDamage);
        }

        if(Map.DefenceTownEvent != null)
        {
          if(Map.DefenceTownEvent.Damages.ContainsKey(attacker))
          {
            Dictionary<object, long> damages;
            object index;
            (damages = Map.DefenceTownEvent.Damages)[(Character) (index = attacker)]
              = damages[index] + actualDamage;
          }
          else
            Map.DefenceTownEvent.Damages.Add(attacker, actualDamage);
        }

        if(Damages.ContainsKey(attacker))
        {
          Dictionary<object, double> damages;
          object index;
          (damages = Damages)[(Character) (index = attacker)] =
            damages[index] + actualDamage;
        }
        else
          Damages.Add(attacker, actualDamage);
      }

      base.OnDamageAction(action);
    }

    /// <summary>Gets a random Unit from those who generated Threat</summary>
    public Unit GetRandomAttacker()
    {
      int num1 = Utility.Random(m_threatCollection.AggressorPairs.Count);
      int num2 = 0;
      foreach(KeyValuePair<Unit, int> aggressorPair in m_threatCollection.AggressorPairs)
      {
        if(!CanBeAggroedBy(aggressorPair.Key))
          --num1;
        if(num2++ >= num1)
          return aggressorPair.Key;
      }

      return null;
    }

    /// <summary>The AIGroup this NPC currently belongs to.</summary>
    public AIGroup Group
    {
      get { return m_group; }
      set
      {
        if(m_group == value)
          return;
        m_brain.OnGroupChange(value);
        m_group = value;
        m_threatCollection.Group = value;
      }
    }

    public NPCSpellCollection NPCSpells
    {
      get { return (NPCSpellCollection) m_spells; }
    }

    public override SpellCollection Spells
    {
      get { return m_spells; }
    }

    public override NPCSpawnPoint SpawnPoint
    {
      get { return m_spawnPoint; }
    }

    public NPCSpawnEntry SpawnEntry
    {
      get
      {
        if(m_spawnPoint == null)
          return null;
        return m_spawnPoint.SpawnEntry;
      }
    }

    public override LinkedList<WaypointEntry> Waypoints
    {
      get
      {
        if(m_spawnPoint == null)
          return null;
        return m_spawnPoint.SpawnEntry.Waypoints;
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
      get { return m_decayTimer != null; }
      set
      {
        if(!value && IsDecaying)
        {
          StopDecayTimer();
        }
        else
        {
          if(!value || IsDecaying)
            return;
          RemainingDecayDelayMillis = m_entry.DefaultDecayDelayMillis;
        }
      }
    }

    public override uint LootMoney
    {
      get { return m_entry.MoneyDrop; }
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
        if(m_decayTimer == null)
          return 0;
        return m_decayTimer.RemainingInitialDelayMillis;
      }
      set
      {
        if(value <= 0)
        {
          StopDecayTimer();
        }
        else
        {
          if(m_decayTimer == null)
            m_decayTimer = new TimerEntry(DecayNow);
          m_decayTimer.Start(value);
        }
      }
    }

    public IRealmClient Client
    {
      get
      {
        if(m_master != null && m_master is Character)
          return ((Character) m_master).Client;
        return null;
      }
    }

    public override int StaminaWithoutHealthContribution
    {
      get
      {
        if(IsHunterPet)
          return GetBaseStatValue(StatType.Stamina);
        return 0;
      }
    }

    public override bool IsRegenerating
    {
      get
      {
        if(IsAreaActive)
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
        if(!IsActivePet || !(m_master is Character))
          return;
        ((Character) m_master).GroupUpdateFlags |= GroupUpdateFlags.PetDisplayId;
      }
    }

    public override int Health
    {
      get { return base.Health; }
      set
      {
        base.Health = value;
        if(m_PetRecord == null || !(m_master is Character))
          return;
        ((Character) m_master).GroupUpdateFlags |= GroupUpdateFlags.PetHealth;
      }
    }

    public override int MaxHealth
    {
      get { return base.MaxHealth; }
      internal set
      {
        base.MaxHealth = value;
        if(m_PetRecord == null || !(m_master is Character))
          return;
        ((Character) m_master).GroupUpdateFlags |= GroupUpdateFlags.PetMaxHealth;
      }
    }

    public override int Power
    {
      get { return base.Power; }
      set
      {
        base.Power = value;
        if(m_PetRecord == null || !(m_master is Character))
          return;
        ((Character) m_master).GroupUpdateFlags |= GroupUpdateFlags.PetPower;
      }
    }

    public override PowerType PowerType
    {
      get { return base.PowerType; }
      set
      {
        base.PowerType = value;
        if(m_PetRecord == null || !(m_master is Character))
          return;
        ((Character) m_master).GroupUpdateFlags |=
          GroupUpdateFlags.PetPowerType | GroupUpdateFlags.PetPower | GroupUpdateFlags.PetMaxPower;
      }
    }

    public override int MaxPower
    {
      get { return base.MaxPower; }
      internal set
      {
        base.MaxPower = value;
        if(m_PetRecord == null || !(m_master is Character))
          return;
        ((Character) m_master).GroupUpdateFlags |= GroupUpdateFlags.PetMaxPower;
      }
    }

    /// <summary>Sets this NPC's equipment to the given entry</summary>
    public void SetEquipment(NPCEquipmentEntry equipment)
    {
      for(int index = 0; index < equipment.ItemIds.Length; ++index)
      {
        Asda2ItemId itemId = equipment.ItemIds[index];
        if(itemId != 0)
        {
          ItemTemplate template = ItemMgr.GetTemplate(itemId);
          if(template != null)
            SheathType = template.SheathType;
          SetUInt32((UnitFields) (56 + index), (uint) itemId);
        }
      }
    }

    public void SetMainWeaponVisual(Asda2ItemId item)
    {
      SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID, (uint) item);
    }

    public void SetOffhandWeaponVisual(Asda2ItemId item)
    {
      SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_2, (uint) item);
    }

    public void SetRangedWeaponVisual(Asda2ItemId item)
    {
      SetUInt32(UnitFields.VIRTUAL_ITEM_SLOT_ID_3, (uint) item);
    }

    /// <summary>
    /// NPCs only have their default items which may always be used, so no invalidation
    /// takes place.
    /// </summary>
    protected override IAsda2Weapon GetOrInvalidateItem(InventorySlotType slot)
    {
      if(slot == InventorySlotType.WeaponMainHand)
        return m_entry.CreateMainHandWeapon();
      return null;
    }

    public override float GetCritChance(DamageSchool school)
    {
      float num = m_entry.Rank > CreatureRank.Normal ? BossSpellCritChance : 0.0f;
      if(m_CritMods != null)
        num += m_CritMods[(int) school];
      return num;
    }

    public override NPC SpawnMinion(NPCEntry entry, ref Vector3 position, int durationMillis)
    {
      NPC npc = base.SpawnMinion(entry, ref position, durationMillis);
      if(Group == null)
        Group = new AIGroup(this);
      Group.Add(npc);
      return npc;
    }

    protected override bool OnBeforeDeath()
    {
      if(m_entry.NotifyBeforeDeath(this))
        return true;
      if(Health == 0)
        Health = 1;
      return false;
    }

    protected override void OnDeath()
    {
      if(m_brain != null)
        m_brain.IsRunning = false;
      if(m_Map != null || this != null)
      {
        ICollection<IRealmClient> rcvrs = this.GetNearbyClients(false);
        Character triggerChar = CalcLooter();
        Asda2Loot loot = null;
        if(triggerChar != null && m_Map.DefenceTownEvent == null)
        {
          loot = Asda2LootMgr.GetOrCreateLoot(this, triggerChar, Asda2LootEntryType.Npc);
          if(loot != null && Template != null)
            loot.MonstrId = (short) Template.Id;
        }

        Loot = null;
        Map map = m_Map;
        m_Map.CallDelayed(LastDamageDelay, () =>
        {
          Asda2CombatHandler.SendMostrDeadToAreaResponse(rcvrs, (short) UniqIdOnMap,
            (short) Asda2Position.X, (short) Asda2Position.Y);
          map.OnNPCDied(this);
          Character chr = LastKiller as Character;
          if(loot != null && loot.Lootable is NPC)
          {
            NPC lootable = (NPC) loot.Lootable;
            if(lootable != null && lootable.Map != null)
              lootable.Map.SpawnLoot(loot);
          }

          if(m_entry != null && Entry != null && Template != null)
          {
            if(Entry.Rank >= CreatureRank.Boss)
            {
              if(LastKiller != null)
              {
                AchievementProgressRecord progressRecord =
                  chr.Achievements.GetOrCreateProgressRecord(22U);
                switch(++progressRecord.Counter)
                {
                  case 500:
                    chr.Map.CallDelayed(500,
                      () => chr.DiscoverTitle(Asda2TitleId.Boss154));
                    break;
                  case 1000:
                    chr.Map.CallDelayed(500, () => chr.GetTitle(Asda2TitleId.Boss154));
                    break;
                }

                progressRecord.SaveAndFlush();
                ChatMgr.SendGlobalMessageResponse(LastKiller.Name,
                  ChatMgr.Asda2GlobalMessageType.HasDefeated, 0, 0, (short) Template.Id);
              }

              if(chr != null)
                chr.GuildPoints +=
                  m_entry.MinLevel * CharacterFormulas.BossKillingGuildPointsPerLevel;
            }
            else
            {
              if(chr != null && chr.Level < m_entry.MinLevel + 3)
                chr.GuildPoints += CharacterFormulas.MobKillingGuildPoints;
              if(chr != null)
              {
                AchievementProgressRecord progressRecord =
                  chr.Achievements.GetOrCreateProgressRecord(0U);
                switch(++progressRecord.Counter)
                {
                  case 50:
                    chr.Map.CallDelayed(500,
                      () => chr.DiscoverTitle(Asda2TitleId.Hunter150));
                    break;
                  case 100:
                    chr.Map.CallDelayed(500, () => chr.GetTitle(Asda2TitleId.Hunter150));
                    break;
                  case 500:
                    chr.Map.CallDelayed(500,
                      () => chr.DiscoverTitle(Asda2TitleId.Exterminator151));
                    break;
                  case 1000:
                    chr.Map.CallDelayed(500,
                      () => chr.GetTitle(Asda2TitleId.Exterminator151));
                    break;
                  case 5000:
                    chr.Map.CallDelayed(500,
                      () => chr.DiscoverTitle(Asda2TitleId.Slayer152));
                    break;
                  case 10000:
                    chr.Map.CallDelayed(500, () => chr.GetTitle(Asda2TitleId.Slayer152));
                    break;
                  case 50000:
                    chr.Map.CallDelayed(500,
                      () => chr.DiscoverTitle(Asda2TitleId.Fanatic153));
                    break;
                  case 100000:
                    chr.Map.CallDelayed(500,
                      () => chr.GetTitle(Asda2TitleId.Fanatic153));
                    break;
                }

                progressRecord.SaveAndFlush();
              }
            }

            m_entry.NotifyDied(this);
          }

          EnterFinalState();
        });
      }

      if(m_currentTamer != null)
      {
        PetHandler.SendTameFailure(m_currentTamer, TameFailReason.TargetDead);
        CurrentTamer.SpellCast.Cancel(SpellFailedReason.Ok);
      }

      if(m_spawnPoint == null)
        return;
      m_spawnPoint.SignalSpawnlingDied(this);
    }

    private Character CalcLooter()
    {
      try
      {
        if(Damages == null || Damages.Count == 0)
          return null;
        List<KeyValuePair<object, double>> list = Damages
          .OrderByDescending(d => d.Value)
          .Take(CharacterFormulas.MaxDamagersDetailCount)
          .ToList();
        object key1 = list.FirstOrDefault().Key;
        Character character = null;
        if(key1 is Group)
          character = (key1 as Group).Leader.Character;
        if(key1 is Character)
          character = key1 as Character;
        if(Entry.Rank >= CreatureRank.Boss)
        {
          SendMessageToArea(
            string.Format("Top {0} damagers are : ", CharacterFormulas.MaxDamagersDetailCount),
            Color.MediumVioletRed);
          int num1 = 1;
          AchievementProgressRecord globalProgressRecord =
            AchievementProgressRecord.GetOrCreateGlobalProgressRecord(184U, 1U);
          foreach(KeyValuePair<object, double> keyValuePair in list)
          {
            Group key2 = keyValuePair.Key as Group;
            if(key2 != null)
            {
              SendMessageToArea(
                string.Format("{0} Party [{1}] deal {2} dmg", num1,
                  key2.Leader.Character.Name, (int) keyValuePair.Value),
                Color.GreenYellow);
              foreach(GroupMember groupMember in key2)
              {
                GroupMember member = groupMember;
                int num2 = 0;
                if(member.Character != null && Damages.ContainsKey(member.Character))
                  num2 = (int) Damages[member.Character];
                SendMessageToArea(
                  string.Format("--- {0} deal {1} dmg", member.Name, num2),
                  Color.LightGreen);
                if(Name == "EvilDragonEnkidu470" && globalProgressRecord.Counter == 1U)
                  member.Character.Map.CallDelayed(500,
                    () => member.Character.GetTitle(Asda2TitleId.DragonSlayer403));
              }
            }
            else
            {
              Character chr = keyValuePair.Key as Character;
              if(Name == "EvilDragonEnkidu470" && globalProgressRecord.Counter == 1U)
                chr.Map.CallDelayed(500, () => chr.GetTitle(Asda2TitleId.DragonSlayer403));
              if(chr != null)
                SendMessageToArea(
                  string.Format("{2} Char [{0}] deal {1} dmg", chr.Name,
                    (int) keyValuePair.Value, num1), Color.GreenYellow);
            }

            ++num1;
          }

          if(Entry.NPCId == NPCId.EvilDragonEnkidu470)
            ++globalProgressRecord.Counter;
          globalProgressRecord.SaveAndFlush();
        }

        Damages.Clear();
        return character;
      }
      catch(NullReferenceException ex)
      {
        return null;
      }
    }

    protected internal override void OnResurrect()
    {
      base.OnResurrect();
      StopDecayTimer();
      UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 | UnitFlags.Influenced |
                   UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation |
                   UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable |
                   UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 |
                   UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 |
                   UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight |
                   UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed |
                   UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted |
                   UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 |
                   UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
      MarkUpdate(UnitFields.DYNAMIC_FLAGS);
      if(m_spawnPoint != null)
        m_spawnPoint.Pool.SpawnedObjects.Add(this);
      m_brain.IsRunning = true;
      m_brain.EnterDefaultState();
      m_brain.OnActivate();
      m_entry.NotifyActivated(this);
    }

    /// <summary>Checks whether this NPC is of the given type</summary>
    public bool CheckCreatureType(CreatureMask mask)
    {
      return mask.HasFlag((CreatureMask) (1 << (int) (Entry.Type - 1 & (CreatureType) 31)));
    }

    protected internal override void OnEnterMap()
    {
      base.OnEnterMap();
      if(m_spawnPoint != null)
        m_spawnPoint.SignalSpawnlingActivated(this);
      int count = m_auras.Count;
      foreach(NPCTypeHandler instanceTypeHandler in m_entry.InstanceTypeHandlers)
      {
        if(instanceTypeHandler != null)
          instanceTypeHandler(this);
      }

      if(m_brain != null)
        m_brain.OnActivate();
      m_entry.NotifyActivated(this);
      if(m_spawnPoint != null)
        m_spawnPoint.SpawnEntry.NotifySpawned(this);
      if(m_master == null)
        return;
      if(m_master.IsInWorld)
        m_master.OnMinionEnteredMap(this);
      else
        Delete();
    }

    protected internal override void OnLeavingMap()
    {
      if(IsAlive && m_spawnPoint != null)
        m_spawnPoint.SignalSpawnlingDied(this);
      if(m_master == null)
        return;
      if(m_master.IsInWorld)
        m_master.OnMinionLeftMap(this);
      else
        Delete();
    }

    /// <summary>Marks this NPC lootable (after NPC died)</summary>
    private void EnterLootableState()
    {
      FirstAttacker = null;
      RemainingDecayDelayMillis = m_entry.DefaultDecayDelayMillis * 2;
      MarkUpdate(UnitFields.DYNAMIC_FLAGS);
    }

    /// <summary>Marks this NPC non-lootable (after NPC was looted)</summary>
    private void EnterFinalState()
    {
      FirstAttacker = null;
      RemainingDecayDelayMillis = m_entry.DefaultDecayDelayMillis;
      if(m_loot != null)
      {
        m_loot.ForceDispose();
        m_loot = null;
      }

      Delete();
    }

    private void DecayNow(int delay)
    {
      if(m_loot != null)
      {
        m_loot.ForceDispose();
        m_loot = null;
      }

      Delete();
    }

    protected internal override void DeleteNow()
    {
      m_Deleted = true;
      m_entry.NotifyDeleted(this);
      if(m_spawnPoint != null && m_spawnPoint.ActiveSpawnling == this)
        m_spawnPoint.SignalSpawnlingDied(this);
      m_auras.ClearWithoutCleanup();
      base.DeleteNow();
    }

    private void StopDecayTimer()
    {
      if(m_decayTimer == null)
        return;
      m_decayTimer.Stop();
      m_decayTimer = null;
    }

    public override float MaxAttackRange
    {
      get
      {
        float num = base.MaxAttackRange;
        if(m_spells != null && NPCSpells.MaxCombatSpellRange > (double) num)
          num = NPCSpells.MaxCombatSpellRange;
        return num;
      }
    }

    protected override void OnEnterCombat()
    {
      base.OnEnterCombat();
      if(m_target == null)
        return;
      m_threatCollection.AddNewIfNotExisted(m_target);
    }

    protected override void OnLeaveCombat()
    {
      base.OnLeaveCombat();
    }

    public override float AggroBaseRange
    {
      get { return m_entry.AggroBaseRange; }
    }

    /// <summary>Also sends a message to the Character, if not valid</summary>
    internal bool CheckVendorInteraction(Character chr)
    {
      if(chr.Map != m_Map ||
         !IsInRadiusSq(chr, NPCMgr.DefaultInteractionDistanceSq) ||
         !chr.CanSee(this))
      {
        NPCHandler.SendNPCError(chr, this,
          VendorInventoryError.TooFarAway);
        return false;
      }

      if(!IsAlive)
      {
        NPCHandler.SendNPCError(chr, this,
          VendorInventoryError.VendorDead);
        return false;
      }

      if(chr.IsAlive == IsSpiritHealer)
      {
        NPCHandler.SendNPCError(chr, this,
          VendorInventoryError.YouDead);
        return false;
      }

      if(!chr.CanInteract || !CanInteract)
        return false;
      Reputation reputation = chr.Reputations.GetOrCreate(Faction.ReputationIndex);
      if(reputation == null || reputation.CanInteract)
        return true;
      NPCHandler.SendNPCError(chr, this,
        VendorInventoryError.BadRep);
      return false;
    }

    public override void Say(float radius, string message)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, SpokenLanguage, message,
        radius);
    }

    public override void Say(float radius, string[] localizedMsgs)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, SpokenLanguage, localizedMsgs,
        radius);
    }

    public override void Yell(float radius, string message)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, SpokenLanguage, message,
        radius);
    }

    public override void Yell(float radius, string[] localizedMsgs)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, SpokenLanguage, localizedMsgs,
        radius);
    }

    public override void Emote(float radius, string message)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, SpokenLanguage, message,
        radius);
    }

    public override void Emote(float radius, string[] localizedMsgs)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, SpokenLanguage, localizedMsgs,
        radius);
    }

    /// <summary>Yells to everyone within the map to hear</summary>
    /// <param name="message"></param>
    public void YellToMap(string[] messages)
    {
      Yell(-1f, messages);
    }

    /// <summary>Whether this unit is a TaxiVendor</summary>
    public bool IsTaxiVendor
    {
      get { return NPCFlags.HasFlag(NPCFlags.FlightMaster); }
    }

    /// <summary>The TaxiNode this TaxiVendor is associated with</summary>
    public PathNode VendorTaxiNode
    {
      get
      {
        if(!IsTaxiVendor)
          return null;
        if(vendorTaxiNode == null)
          vendorTaxiNode = TaxiMgr.GetNearestTaxiNode(Position);
        return vendorTaxiNode;
      }
      internal set { vendorTaxiNode = value; }
    }

    /// <summary>Whether this is a Banker</summary>
    public bool IsBanker
    {
      get { return NPCFlags.HasFlag(NPCFlags.Banker); }
    }

    /// <summary>Whether this is an InnKeeper</summary>
    public bool IsInnKeeper
    {
      get { return NPCFlags.HasFlag(NPCFlags.InnKeeper); }
    }

    /// <summary>
    /// The location to which a Character can bind to when talking to this NPC
    /// or null if this is not an InnKeeper.
    /// </summary>
    public NamedWorldZoneLocation BindPoint { get; internal set; }

    /// <summary>Whether this is a Vendor</summary>
    public bool IsVendor
    {
      get { return m_entry.IsVendor; }
    }

    /// <summary>
    /// A list of Items this Vendor has for sale.
    /// Returns <c>VendorItemEntry.EmptyList</c> if this is not a Vendor
    /// </summary>
    public List<VendorItemEntry> ItemsForSale
    {
      get
      {
        if(VendorEntry == null)
          return VendorItemEntry.EmptyList;
        return VendorEntry.ItemsForSale;
      }
    }

    /// <summary>The Vendor-specific details for this NPC</summary>
    public VendorEntry VendorEntry
    {
      get { return m_VendorEntry; }
      set { m_VendorEntry = value; }
    }

    /// <summary>Whether this is a Stable Master</summary>
    public bool IsStableMaster
    {
      get { return NPCFlags.HasFlag(NPCFlags.StableMaster); }
    }

    /// <summary>Whether this NPC can issue Charters</summary>
    public bool IsPetitioner
    {
      get { return NPCFlags.HasFlag(NPCFlags.Petitioner); }
    }

    /// <summary>Whether this is a Tabard Vendor</summary>
    public bool IsTabardVendor
    {
      get { return NPCFlags.HasFlag(NPCFlags.TabardDesigner); }
    }

    /// <summary>Whether this NPC can issue Guild Charters</summary>
    public bool IsGuildPetitioner
    {
      get { return NPCFlags.HasAnyFlag(NPCFlags.Petitioner | NPCFlags.TabardDesigner); }
    }

    /// <summary>Whether this NPC can issue Arena Charters</summary>
    public bool IsArenaPetitioner
    {
      get { return NPCFlags.HasFlag(NPCFlags.Petitioner); }
    }

    /// <summary>Whether this NPC starts a quest (or multiple quests)</summary>
    public bool IsQuestGiver
    {
      get { return NPCFlags.HasFlag(NPCFlags.QuestGiver); }
      internal set
      {
        if(value)
          NPCFlags |= NPCFlags.QuestGiver;
        else
          NPCFlags &= ~NPCFlags.QuestGiver;
      }
    }

    /// <summary>
    /// All available Quest information, in case that this is a QuestGiver
    /// </summary>
    public QuestHolderInfo QuestHolderInfo
    {
      get { return m_entry.QuestHolderInfo; }
      internal set { m_entry.QuestHolderInfo = value; }
    }

    public bool CanGiveQuestTo(Character chr)
    {
      return CheckVendorInteraction(chr);
    }

    public void OnQuestGiverStatusQuery(Character chr)
    {
    }

    /// <summary>Whether this is a Trainer.</summary>
    public bool IsTrainer
    {
      get { return TrainerEntry != null; }
    }

    /// <summary>
    /// Whether this NPC can train the character in their specialty.
    /// </summary>
    /// <returns>True if able to train.</returns>
    public bool CanTrain(Character character)
    {
      if(IsTrainer)
        return TrainerEntry.CanTrain(character);
      return false;
    }

    /// <summary>Whether this is an Auctioneer.</summary>
    public bool IsAuctioneer
    {
      get { return AuctioneerEntry != null; }
    }

    /// <summary>
    /// The Character that currently tries to tame this Creature (or null if not being tamed)
    /// </summary>
    public Character CurrentTamer
    {
      get { return m_currentTamer; }
      set
      {
        if(value == m_currentTamer)
          return;
        m_currentTamer = value;
      }
    }

    /// <summary>
    /// We were controlled and reject the Controller.
    /// Does nothing if not controlled.
    /// </summary>
    public void RejectMaster()
    {
      if(m_master != null)
      {
        SetEntityId(UnitFields.SUMMONEDBY, EntityId.Zero);
        SetEntityId(UnitFields.CHARMEDBY, EntityId.Zero);
        Master = null;
      }

      if(m_PetRecord == null)
        return;
      DeletePetRecord();
    }

    public override uint GetLootId(Asda2LootEntryType lootType)
    {
      if(lootType == Asda2LootEntryType.Npc)
        return Template.Id;
      return 0;
    }

    public override bool UseGroupLoot
    {
      get { return true; }
    }

    public bool HelperBossSummoned { get; set; }

    public override void OnFinishedLooting()
    {
      EnterFinalState();
    }

    public override int GetBasePowerRegen()
    {
      if(IsPlayerOwned)
        return RegenerationFormulas.GetPowerRegen(this);
      return BasePower / 50;
    }

    public override void Update(int dt)
    {
      if(m_decayTimer != null)
        m_decayTimer.Update(dt);
      if(m_target != null && CanMove)
        SetOrientationTowards(m_target);
      base.Update(dt);
    }

    public override void Dispose(bool disposing)
    {
      if(m_Map == null)
        return;
      m_currentTamer = null;
      m_Map.UnregisterUpdatableLater(m_decayTimer);
      base.Dispose(disposing);
    }

    public override string ToString()
    {
      return Name + " (ID: " + EntryId + ", #" + EntityId.Low + ")";
    }

    public IPetRecord PetRecord
    {
      get { return m_PetRecord; }
      set { m_PetRecord = value; }
    }

    public PermanentPetRecord PermanentPetRecord
    {
      get { return m_PetRecord as PermanentPetRecord; }
    }

    /// <summary>
    /// Whether this is the active pet of it's master (with an action bar)
    /// </summary>
    public bool IsActivePet
    {
      get
      {
        if(m_master is Character)
          return ((Character) m_master).ActivePet == this;
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
        if(m_PetRecord is PermanentPetRecord)
          return PetTalentType == PetTalentType.End;
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
      if(!PetMgr.InfinitePetRenames && !chr.GodMode && !PetState.HasFlag(PetState.CanBeRenamed))
        return PetNameInvalidReason.Invalid;
      PetNameInvalidReason nameInvalidReason = PetMgr.ValidatePetName(ref name);
      if(nameInvalidReason != PetNameInvalidReason.Ok)
        return nameInvalidReason;
      Name = name;
      PetState &= ~PetState.CanBeRenamed;
      return nameInvalidReason;
    }

    /// <summary>Makes this the pet of the given owner</summary>
    internal void MakePet(uint ownerId)
    {
      PetRecord = PetMgr.CreatePermanentPetRecord(Entry, ownerId);
      if(HasTalents || !IsHunterPet)
        return;
      m_petTalents = new PetTalentCollection(this);
    }

    /// <summary>
    /// Is called when this Pet became the ActivePet of a Character
    /// </summary>
    internal void OnBecameActivePet()
    {
      OnLevelChanged();
    }

    public bool CanEat(PetFoodType petFoodType)
    {
      if(m_entry.Family != null)
        return m_entry.Family.PetFoodMask.HasAnyFlag(petFoodType);
      return false;
    }

    public int GetHappinessGain(ItemTemplate food)
    {
      if(food == null)
        return 0;
      int num = Level - (int) food.Level;
      if(num > 0)
      {
        if(num < 16)
          return PetMgr.MaxFeedPetHappinessGain;
        if(num < 26)
          return PetMgr.MaxFeedPetHappinessGain / 2;
        if(num < 36)
          return PetMgr.MaxFeedPetHappinessGain / 4;
      }
      else if(num > -16)
        return PetMgr.MaxFeedPetHappinessGain;

      return 0;
    }

    public PetTalentType PetTalentType
    {
      get
      {
        if(Entry.Family == null)
          return PetTalentType.End;
        return Entry.Family.PetTalentType;
      }
    }

    public DateTime? LastTalentResetTime { get; set; }

    public bool HasTalents
    {
      get { return m_petTalents != null; }
    }

    /// <summary>Collection of all this Pet's Talents</summary>
    public override TalentCollection Talents
    {
      get { return m_petTalents; }
    }

    public int FreeTalentPoints
    {
      get { return GetByte(UnitFields.BYTES_1, 1); }
      set
      {
        if(!(m_PetRecord is PermanentPetRecord))
          return;
        PermanentPetRecord.FreeTalentPoints = value;
        SetByte(UnitFields.BYTES_1, 1, (byte) value);
        TalentHandler.SendTalentGroupList(m_petTalents);
      }
    }

    public void UpdateFreeTalentPointsSilently(int delta)
    {
      if(m_PetRecord is PermanentPetRecord)
        PermanentPetRecord.FreeTalentPoints = delta;
      SetByte(UnitFields.BYTES_1, 1, (byte) delta);
    }

    public void ResetFreeTalentPoints()
    {
      int num = 0;
      Character master = m_master as Character;
      if(master != null)
        num += master.PetBonusTalentPoints;
      FreeTalentPoints = num + PetMgr.GetPetTalentPointsByLevel(Level);
    }

    internal void UpdatePetData(IActivePetSettings settings)
    {
      settings.PetEntryId = Entry.NPCId;
      settings.PetHealth = Health;
      settings.PetPower = Power;
      settings.PetDuration = RemainingDecayDelayMillis;
      settings.PetSummonSpellId = CreationSpellId;
      UpdateTalentSpellRecords();
      m_PetRecord.UpdateRecord(this);
    }

    private void UpdateTalentSpellRecords()
    {
      List<PetTalentSpellRecord> talentSpellRecordList = new List<PetTalentSpellRecord>();
      foreach(Spell npcSpell in NPCSpells)
      {
        int remainingCooldownMillis = NPCSpells.GetRemainingCooldownMillis(npcSpell);
        PetTalentSpellRecord talentSpellRecord = new PetTalentSpellRecord
        {
          SpellId = npcSpell.Id,
          CooldownUntil = DateTime.Now.AddMilliseconds(remainingCooldownMillis)
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
        if(IsHunterPet)
          return PetExperience < NextPetLevelExperience;
        return false;
      }
    }

    public bool MayGainLevels
    {
      get
      {
        if(HasPlayerMaster)
          return Level <= MaxLevel;
        return false;
      }
    }

    public override int MaxLevel
    {
      get
      {
        if(HasPlayerMaster)
          return m_master.Level;
        return base.MaxLevel;
      }
    }

    internal bool TryLevelUp()
    {
      return false;
    }

    protected override void OnLevelChanged()
    {
      if(HasPlayerMaster)
        AddMessage(() =>
        {
          UpdateSpellRanks();
          UpdateSize();
          int level = Level;
          if(level >= PetMgr.MinPetTalentLevel)
          {
            if(m_petTalents == null)
              m_petTalents = new PetTalentCollection(this);
            int num = Talents.GetFreeTalentPointsForLevel(level);
            if(num < 0)
            {
              if(!((Character) m_master).GodMode)
                Talents.RemoveTalents(-num);
              num = 0;
            }

            FreeTalentPoints = num;
          }

          PetLevelStatInfo petLevelStatInfo = m_entry.GetPetLevelStatInfo(level);
          if(petLevelStatInfo == null)
            return;
          ModPetStatsPerLevel(petLevelStatInfo);
          m_auras.ReapplyAllAuras();
        });
      m_entry.NotifyLeveledChanged(this);
    }

    internal void ModPetStatsPerLevel(PetLevelStatInfo levelStatInfo)
    {
      BaseHealth = levelStatInfo.Health;
      if(PowerType == PowerType.Mana && levelStatInfo.Mana > 0)
        BasePower = levelStatInfo.Mana;
      for(StatType stat = StatType.Strength; stat < StatType.End; ++stat)
        SetBaseStat(stat, levelStatInfo.BaseStats[(int) stat]);
      this.UpdatePetResistance(DamageSchool.Physical);
      SetInt32(UnitFields.HEALTH, MaxHealth);
    }

    private void UpdateSpellRanks()
    {
      if(m_entry.Spells == null)
        return;
      int level = Level;
      foreach(Spell spell in m_entry.Spells.Values)
      {
        if(spell.Level > level)
          m_spells.Remove(spell);
        else
          m_spells.AddSpell(spell);
      }
    }

    public override int GetUnmodifiedBaseStatValue(StatType stat)
    {
      if(HasPlayerMaster)
      {
        PetLevelStatInfo petLevelStatInfo = m_entry.GetPetLevelStatInfo(Level);
        if(petLevelStatInfo != null)
          return petLevelStatInfo.BaseStats[(int) stat];
      }

      return base.GetUnmodifiedBaseStatValue(stat);
    }

    public void SetPetAttackMode(PetAttackMode mode)
    {
      if(m_PetRecord != null)
        m_PetRecord.AttackMode = mode;
      if(mode == PetAttackMode.Passive)
      {
        m_brain.IsAggressive = false;
        m_brain.DefaultState = BrainState.Follow;
      }
      else
      {
        m_brain.IsAggressive = mode == PetAttackMode.Aggressive;
        m_brain.DefaultState = BrainState.Guard;
      }

      m_brain.EnterDefaultState();
    }

    public void SetPetAction(PetAction action)
    {
      switch(action)
      {
        case PetAction.Stay:
          HasPermissionToMove = false;
          break;
        case PetAction.Follow:
          HasPermissionToMove = true;
          break;
        case PetAction.Attack:
          HasPermissionToMove = true;
          Unit target = m_master.Target;
          if(target == null || !MayAttack(target))
            break;
          m_threatCollection.Clear();
          m_threatCollection[target] = int.MaxValue;
          m_brain.State = BrainState.Combat;
          break;
        case PetAction.Abandon:
          if(!(m_master is Character))
            break;
          ((Character) m_master).ActivePet = null;
          break;
      }
    }

    /// <summary>Lets this Pet cast the given spell</summary>
    public void CastPetSpell(SpellId spellId, WorldObject target)
    {
      Spell readySpell = NPCSpells.GetReadySpell(spellId);
      SpellFailedReason reason;
      if(readySpell != null)
      {
        if(readySpell.HasTargets)
          Target = m_master.Target;
        reason = readySpell.CheckCasterConstraints(this);
        if(reason == SpellFailedReason.Ok)
        {
          SpellCast spellCast = SpellCast;
          Spell spell = readySpell;
          int num = 0;
          WorldObject[] worldObjectArray;
          if(target == null)
            worldObjectArray = null;
          else
            worldObjectArray = new WorldObject[1] { target };
          reason = spellCast.Start(spell, num != 0, worldObjectArray);
        }
      }
      else
        reason = SpellFailedReason.NotReady;

      if(reason == SpellFailedReason.Ok || !(m_master is IPacketReceiver))
        return;
      PetHandler.SendCastFailed((IPacketReceiver) m_master, spellId, reason);
    }

    public uint[] BuildPetActionBar()
    {
      uint[] numArray1 = new uint[10];
      int num1 = 0;
      uint[] numArray2 = numArray1;
      int index1 = num1;
      int num2 = index1 + 1;
      int raw1 = (int) new PetActionEntry
      {
        Action = PetAction.Attack,
        Type = PetActionType.SetAction
      }.Raw;
      numArray2[index1] = (uint) raw1;
      uint[] numArray3 = numArray1;
      int index2 = num2;
      int num3 = index2 + 1;
      int raw2 = (int) new PetActionEntry
      {
        Action = PetAction.Follow,
        Type = PetActionType.SetAction
      }.Raw;
      numArray3[index2] = (uint) raw2;
      uint[] numArray4 = numArray1;
      int index3 = num3;
      int num4 = index3 + 1;
      int raw3 = (int) new PetActionEntry
      {
        Action = PetAction.Stay,
        Type = PetActionType.SetAction
      }.Raw;
      numArray4[index3] = (uint) raw3;
      if(Entry.Spells != null)
      {
        Dictionary<SpellId, Spell>.Enumerator enumerator = Entry.Spells.GetEnumerator();
        for(byte index4 = 0; index4 < (byte) 4; ++index4)
        {
          if(!enumerator.MoveNext())
          {
            numArray1[num4++] = new PetActionEntry
            {
              Type = ((PetActionType) (8U + index4))
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
        for(byte index4 = 0; index4 < (byte) 4; ++index4)
          numArray1[num4++] = new PetActionEntry
          {
            Type = ((PetActionType) (8U + index4))
          }.Raw;
      }

      uint[] numArray5 = numArray1;
      int index5 = num4;
      int num5 = index5 + 1;
      int raw4 = (int) new PetActionEntry
      {
        AttackMode = PetAttackMode.Aggressive,
        Type = PetActionType.SetMode
      }.Raw;
      numArray5[index5] = (uint) raw4;
      uint[] numArray6 = numArray1;
      int index6 = num5;
      int num6 = index6 + 1;
      int raw5 = (int) new PetActionEntry
      {
        AttackMode = PetAttackMode.Defensive,
        Type = PetActionType.SetMode
      }.Raw;
      numArray6[index6] = (uint) raw5;
      uint[] numArray7 = numArray1;
      int index7 = num6;
      int num7 = index7 + 1;
      int raw6 = (int) new PetActionEntry
      {
        AttackMode = PetAttackMode.Passive,
        Type = PetActionType.SetMode
      }.Raw;
      numArray7[index7] = (uint) raw6;
      return numArray1;
    }

    public uint GetTotemIndex()
    {
      if(CreationSpellId != SpellId.None)
      {
        Spell spell = SpellHandler.Get(CreationSpellId);
        if(spell != null && spell.TotemEffect != null)
        {
          SpellSummonTotemHandler handler = spell.TotemEffect.SummonEntry.Handler as SpellSummonTotemHandler;
          if(handler != null)
            return handler.Index;
        }
      }

      return 0;
    }

    private void DeletePetRecord()
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(m_PetRecord.Delete);
      m_PetRecord = null;
    }

    public uint[] BuildVehicleActionBar()
    {
      uint[] numArray = new uint[10];
      int num1 = 0;
      byte num2 = 0;
      if(Entry.Spells != null)
      {
        Dictionary<SpellId, Spell>.Enumerator enumerator = Entry.Spells.GetEnumerator();
        for(; num2 < (byte) 4; ++num2)
        {
          if(!enumerator.MoveNext())
          {
            numArray[num1++] = new PetActionEntry
            {
              Type = ((PetActionType) (8U + num2))
            }.Raw;
          }
          else
          {
            KeyValuePair<SpellId, Spell> current = enumerator.Current;
            PetActionEntry petActionEntry = new PetActionEntry();
            if(current.Value.IsPassive)
            {
              SpellCast spellCast = SpellCast;
              if(spellCast != null)
                spellCast.TriggerSelf(current.Value);
              petActionEntry.Type = (PetActionType) (8U + num2);
            }
            else
              petActionEntry.SetSpell(current.Key, (PetActionType) (8U + num2));

            numArray[num1++] = petActionEntry.Raw;
          }
        }
      }

      for(; num2 < (byte) 10; ++num2)
        numArray[num1++] = new PetActionEntry
        {
          Type = ((PetActionType) (8U + num2))
        }.Raw;
      return numArray;
    }
  }
}