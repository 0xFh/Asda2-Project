/*************************************************************************
 *
 *   file		: Owner.Fields.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-20 06:16:32 +0100 (l? 20 feb 2010) $

 *   revision		: $Rev: 1257 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.ArenaTeams;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Quests;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Database;
using WCell.Core.Timers;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Asda2BattleGround;
using WCell.RealmServer.Asda2Fishing;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Asda2Mail;
using WCell.RealmServer.Asda2PetSystem;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Battlegrounds.Arenas;
using WCell.RealmServer.Chat;
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
using WCell.RealmServer.Logs;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Mail;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Mounts;
using WCell.RealmServer.Network;
using WCell.RealmServer.Privileges;
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
using WCell.Util.Graphics;
using WCell.Util;

namespace WCell.RealmServer.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Character
    {
        #region Fields

        protected string m_name;
        protected internal CharacterRecord m_record;

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

        protected TimerEntry m_logoutTimer;
        protected IRealmClient m_client;
        //protected int m_inRegear;

        public MoveControl MoveControl;

        private BattlegroundInfo m_bgInfo;
        protected InstanceCollection m_InstanceCollection;

        protected GroupMember m_groupMember;
        protected GroupUpdateFlags m_groupUpdateFlags = GroupUpdateFlags.None;

        protected GuildMember m_guildMember;
        protected ArenaTeamMember[] m_arenaTeamMember = new ArenaTeamMember[3];

        /// <summary>
        /// All skills of this Character
        /// </summary>
        protected SkillCollection m_skills;

        /// <summary>
        /// All talents of this Character
        /// </summary>
        protected TalentCollection m_talents;

        protected AchievementCollection m_achievements;

        protected PlayerInventory m_inventory;

        protected ReputationCollection m_reputations;

        protected MailAccount m_mailAccount;

        protected Archetype m_archetype;

        /// <summary>
        /// The current corpse of this Character or null
        /// </summary>
        protected Corpse m_corpse;

        /// <summary>
        /// Auto releases Corpse after expiring
        /// </summary>
        protected TimerEntry m_corpseReleaseTimer;

        /// <summary>
        /// All languages known to this Character
        /// </summary>
        protected internal readonly HashSet<ChatLanguage> KnownLanguages = new HashSet<ChatLanguage>();

        /// <summary>
        /// The time when this Character started falling
        /// </summary>
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
        #endregion
        public Dictionary<long, Asda2MailMessage> MailMessages = new Dictionary<long, Asda2MailMessage>();
        public Asda2TeleportingPointRecord[] TeleportPoints = new Asda2TeleportingPointRecord[10];
        public Dictionary<uint, CharacterRecord> Friends = new Dictionary<uint, CharacterRecord>();
        public int LastExpLooseAmount { get; set; }

        /// <summary>
        /// Contains certain info that is almost only used by staff and should usually not be available to normal players.
        /// <remarks>Guaranteed to be non-null</remarks>
        /// </summary>
        public ExtraInfo ExtraInfo
        {
            get
            {
                if (m_ExtraInfo == null)
                {
                    m_ExtraInfo = new ExtraInfo(this);
                }
                return m_ExtraInfo;
            }
        }
        public Dictionary<Asda2ItemCategory, FunctionItemBuff> PremiumBuffs = new Dictionary<Asda2ItemCategory, FunctionItemBuff>();

        public FunctionItemBuff[] LongTimePremiumBuffs = new FunctionItemBuff[20];
        public int TransportItemId
        {
            get { return _transportItemId; }
            set
            {
                if (_transportItemId == value) return;
                if (value == -1 || _transportItemId != -1)
                {
                    FunctionalItemsHandler.SendCancelCancelFunctionalItemResponse(Client, (short)_transportItemId);
                    var templ = Asda2ItemMgr.GetTemplate(_transportItemId);
                    var val = templ.ValueOnUse;
                    this.ChangeModifier(StatModifierFloat.Speed, -val / 100f);
                }
                if (value != -1)
                {
                    FunctionalItemsHandler.SendShopItemUsedResponse(Client, value);
                    var templ = Asda2ItemMgr.GetTemplate(value);
                    var val = templ.ValueOnUse;
                    this.ChangeModifier(StatModifierFloat.Speed, val / 100f);
                }
                _transportItemId = value;
            }
        }

        public DateTime LastTransportUsedTime = DateTime.MinValue;
        public int MountId
        {
            get { return _mountId; }
            set
            {
                if (_mountId == value) return;

                if (value == -1)
                {
                    var val = Asda2MountMgr.TemplatesById[MountId].Unk + IntMods[(int)StatModifierInt.MountSpeedIncreace];
                    this.ChangeModifier(StatModifierFloat.Speed, -val / 100f);
                    _mountId = value;
                    Asda2MountHandler.SendVeicheStatusChangedResponse(this, Asda2MountHandler.MountStatusChanged.Unsumon);
                    Asda2MountHandler.SendCharacterOnMountStatusChangedResponse(this, Asda2MountHandler.UseMountStatus.Ok);

                }
                else
                {
                    if (LastTransportUsedTime.AddSeconds(30) > DateTime.Now)
                    {
                        Asda2MountHandler.SendCharacterOnMountStatusChangedResponse(this, Asda2MountHandler.UseMountStatus.Fail);
                        SendInfoMsg("Mount is on cooldown.");
                        return;
                    }
                    _mountId = value;
                    Asda2MountHandler.SendVeicheStatusChangedResponse(this, Asda2MountHandler.MountStatusChanged.Summoned);
                    Asda2MountHandler.SendCharacterOnMountStatusChangedResponse(this, Asda2MountHandler.UseMountStatus.Ok);
                    var val = Asda2MountMgr.TemplatesById[value].Unk + IntMods[(int)StatModifierInt.MountSpeedIncreace];
                    this.ChangeModifier(StatModifierFloat.Speed, val / 100f);
                    LastTransportUsedTime = DateTime.Now;
                }
            }
        }

        public int ApplyFunctionItemBuff(int itemId, bool isLongTimeBuff = false)
        {
            var s = 0;
            var templ = Asda2ItemMgr.GetTemplate(itemId);
            if (isLongTimeBuff)
            {

                if (LongTimePremiumBuffs.Contains(l => l != null && l.Template.Category == templ.Category))
                    throw new AlreadyBuffedExcepton();

                var nb = new FunctionItemBuff(itemId, this) { IsLongTime = true };
                nb.EndsDate = DateTime.Now.AddDays(((nb.Template.AttackTime == 0 ? 7 : nb.Template.AttackTime)));


                nb.CreateLater();
                s = LongTimePremiumBuffs.AddElement(nb);
                ProcessFunctionalItemEffect(nb, true);

            }
            else
            {
                if (PremiumBuffs.ContainsKey(templ.Category))
                {
                    var buff = PremiumBuffs[templ.Category];
                    buff.Duration = buff.Template.AtackRange * 1000;
                    if (buff.Stacks >= buff.Template.MaxDurability)
                    {
                        throw new AlreadyBuffedExcepton();
                    }
                    ProcessFunctionalItemEffect(buff, false);
                    buff.Stacks++;
                    ProcessFunctionalItemEffect(buff, true);
                }
                else
                {
                    var nb = new FunctionItemBuff(itemId, this);
                    nb.Duration = nb.Template.AtackRange * 1000;
                    nb.CreateLater();
                    PremiumBuffs.Add(templ.Category, nb);
                    ProcessFunctionalItemEffect(nb, true);
                }
            }

            return s;
        }

        void ProcessFunctionalItemEffect(FunctionItemBuff item, bool isPositive)
        {
            var value = (isPositive ? item.Template.ValueOnUse : -item.Template.ValueOnUse) * item.Stacks;
            switch (item.Template.Category)
            {
                case Asda2ItemCategory.IncHp:
                    MaxHealthModScalar += value / 100f;
                    this.ChangeModifier(StatModifierFloat.Health, value / 100f);
                    if (isPositive)
                    {
                        var health = ((MaxHealth * value) + 50) / 100; //rounded
                        Health += health;
                    }
                    break;
                case Asda2ItemCategory.IncMp:
                    this.ChangeModifier(StatModifierInt.PowerPct, value);
                    break;
                case Asda2ItemCategory.IncAtackSpeed:
                    this.ChangeModifier(StatModifierFloat.MeleeAttackTime, -value / 100f);
                    break;
                case Asda2ItemCategory.IncDex:
                    this.ChangeModifier(StatModifierFloat.Agility, value / 100f);
                    break;
                case Asda2ItemCategory.IncDigChance:
                    this.ChangeModifier(StatModifierFloat.DigChance, value / 100f);
                    break;
                case Asda2ItemCategory.IncDropChance:
                    this.ChangeModifier(StatModifierFloat.Asda2DropChance, value / 100f);
                    break;
                case Asda2ItemCategory.IncExp:
                    this.ChangeModifier(StatModifierFloat.Asda2ExpAmount, value / 100f);
                    break;
                case Asda2ItemCategory.IncExpStackable:
                    this.ChangeModifier(StatModifierFloat.Asda2ExpAmount, value / 100f);
                    break;
                case Asda2ItemCategory.IncInt:
                    this.ChangeModifier(StatModifierFloat.Intelect, value / 100f);
                    break;
                case Asda2ItemCategory.IncLuck:
                    this.ChangeModifier(StatModifierFloat.Luck, value / 100f);
                    break;
                case Asda2ItemCategory.IncMAtk:
                    this.ChangeModifier(StatModifierFloat.MagicDamage, value / 100f);
                    break;
                case Asda2ItemCategory.IncMdef:
                    this.ChangeModifier(StatModifierFloat.Asda2MagicDefence, value / 100f);
                    break;
                case Asda2ItemCategory.IncMoveSpeed:
                    this.ChangeModifier(StatModifierFloat.Speed, value / 100f);
                    break;
                case Asda2ItemCategory.IncPAtk:
                    this.ChangeModifier(StatModifierFloat.Damage, value / 100f);
                    break;
                case Asda2ItemCategory.IncPDef:
                    this.ChangeModifier(StatModifierFloat.Asda2Defence, value / 100f);
                    break;
                case Asda2ItemCategory.IncSpi:
                    this.ChangeModifier(StatModifierFloat.Spirit, value / 100f);
                    break;
                case Asda2ItemCategory.IncSta:
                    this.ChangeModifier(StatModifierFloat.Stamina, value / 100f);
                    break;
                case Asda2ItemCategory.IncStr:
                    this.ChangeModifier(StatModifierFloat.Strength, value / 100f);
                    break;
                case Asda2ItemCategory.PremiumPotions:
                    this.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (isPositive ? 1f : -1f) * 20f / 100f);
                    this.ChangeModifier(StatModifierFloat.Asda2DropChance, (isPositive ? 1f : -1f) * 20f / 100f);
                    this.ChangeModifier(StatModifierFloat.Health, (isPositive ? 1f : -1f) * 10f / 100f);
                    this.ChangeModifier(StatModifierInt.PowerPct, (isPositive ? 1 : -1) * 10);
                    this.ChangeModifier(StatModifierFloat.Speed, (isPositive ? 1f : -1f) * 25f / 100f);

                    if (item.Template.Id == 449 || item.Template.Id == 450 || item.Template.Id == 451 || item.Template.Id == 452)
                    {
                        this.ChangeModifier(StatModifierFloat.Damage, (isPositive ? 1f : -1f) * 20f / 100f);
                    }
                    if (item.Template.Id == 453 || item.Template.Id == 454 || item.Template.Id == 455 || item.Template.Id == 456)
                    {
                        this.ChangeModifier(StatModifierFloat.MagicDamage, (isPositive ? 1f : -1f) * 20f / 100f);
                    }
                    Asda2WingsItemId = (isPositive ? (short)item.Template.Id : (short)-1);
                    break;
                case Asda2ItemCategory.ExpandInventory:
                    InventoryExpanded = isPositive;
                    break;
                case Asda2ItemCategory.RemoveDeathPenaltiesByDays:
                    RemoveDeathPenalties = isPositive;
                    break;
                case Asda2ItemCategory.ShopBanner:
                    EliteShopBannerEnabled = isPositive;
                    break;
                case Asda2ItemCategory.PetNotEatingByDays:
                    PetNotHungerEnabled = isPositive;
                    break;
            }
            if (!isPositive)
                FunctionalItemsHandler.SendCancelCancelFunctionalItemResponse(Client, (short)item.ItemId);
        }
        //todo EliteShopBannerEnabled
        public bool PetNotHungerEnabled { get; set; }
        public bool EliteShopBannerEnabled { get; set; }
        public bool RemoveDeathPenalties { get; set; }
        public bool InventoryExpanded { get; set; }
        public bool IsOnTransport { get { return TransportItemId != -1; } }
        public Vector2 CurrentMovingVector { get; set; }
        public override int GetUnmodifiedBaseStatValue(StatType stat)
        {
            if ((byte)stat >= ClassBaseStats.Stats.Length)
                return 0;
            return ClassBaseStats.Stats[(int)stat];
        }

        public override bool IsPlayer
        {
            get { return true; }
        }

        public override bool MayTeleport
        {
            get { return Role.IsStaff || (!IsKicked && CanMove && IsPlayerControlled); }
        }

        public override WorldObject Mover
        {
            get { return MoveControl.Mover; }
        }

        #region BYTES

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

        #endregion

        #region BYTES_2

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

        /// <summary>
        /// 0x10 for SpellSteal
        /// </summary>
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
        /// 0x01 -> Rested State
        /// 0x02 -> Normal State
        /// </summary>
        public RestState RestState
        {
            get { return (RestState)GetByte(PlayerFields.BYTES_2, 3); }
            set { SetByte(PlayerFields.BYTES_2, 3, (byte)value); }
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
                if (m_restTrigger != value)
                {
                    if (value == null)
                    {
                        // leaving rest state
                        UpdateRest();
                        m_record.RestTriggerId = 0;
                        RestState = RestState.Normal;
                    }
                    else
                    {
                        // start resting
                        m_lastRestUpdate = DateTime.Now;
                        m_record.RestTriggerId = (int)value.Id;
                        RestState = RestState.Resting;
                    }
                    m_restTrigger = value;
                }
            }
        }

        #endregion

        #region BYTES_3

        public byte[] PlayerBytes3
        {
            get { return GetByteArray(PlayerFields.BYTES_3); }
            set { SetByteArray(PlayerFields.BYTES_3, value); }
        }

        public override GenderType Gender
        {
            get { return (GenderType)GetByte(PlayerFields.BYTES_3, 0); }
            set
            {
                SetByte(PlayerFields.BYTES_3, 0, (byte)value);
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
                if (value > 100)
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

        #endregion

        #region PLAYER_FIELD_BYTES

        /// <summary>
        /// BYTES
        /// </summary>
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
            get { return (CorpseReleaseFlags)GetByte(PlayerFields.PLAYER_FIELD_BYTES, 0); }
            set { SetByte(PlayerFields.PLAYER_FIELD_BYTES, 0, (byte)value); }
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

        #endregion

        #region PLAYER_FIELD_BYTES2

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

        /// <summary>
        /// Set to 0x40 for mage invis
        /// </summary>
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

        #endregion

        #region Misc

        public PlayerFlags PlayerFlags
        {
            get { return (PlayerFlags)GetInt32(PlayerFields.FLAGS); }
            set { SetUInt32(PlayerFields.FLAGS, (uint)value); }
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
            get { return (uint)Record.Money; }
            set
            {
                Record.Money = value;
            }
        }
        public void SendMoneyUpdate()
        {
            if (Map != null) Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Ok, Asda2Inventory.RegularItems[0], this);
        }
        /// <summary>
        /// Adds the given amount of money
        /// </summary>
        public void AddMoney(uint amount)
        {
            Log.Create(Log.Types.ItemOperations, LogSourceType.Character, EntryId)
                           .AddAttribute("source", 0, "add_money")
                           .AddAttribute("current", Money)
                           .AddAttribute("diff", amount)
                           .Write();
            Money = Money + amount;
        }

        /// <summary>
        /// Subtracts the given amount of Money. Returns false if its more than this Character has.
        /// </summary>
        public bool SubtractMoney(uint amount)
        {
            Log.Create(Log.Types.ItemOperations, LogSourceType.Character, EntryId)
                                 .AddAttribute("source", 0, "substract_money")
                                 .AddAttribute("current", Money)
                                 .AddAttribute("diff", amount)
                                 .Write();
            var money = Money;
            if (amount > money)
            {
                return false;
            }

            Money = Money - amount;
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
            get { return (TitleBitId)GetUInt32(PlayerFields.CHOSEN_TITLE); }
            set { SetUInt32(PlayerFields.CHOSEN_TITLE, (uint)value); }
        }

        public CharTitlesMask KnownTitleMask
        {
            get { return (CharTitlesMask)GetUInt64(PlayerFields._FIELD_KNOWN_TITLES); }
            set { SetUInt64(PlayerFields._FIELD_KNOWN_TITLES, (ulong)value); }
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
            SetUInt32((int)PlayerFields.ARENA_TEAM_INFO_1_1 + ((int)slot * (int)ArenaTeamInfoType.ARENA_TEAM_END) + (int)type, value);
        }

        /// <summary>
        /// The 3 classmasks of spells to not use require reagents for
        /// </summary>
        public uint[] NoReagentCost
        {
            get
            {
                return new[] { GetUInt32(PlayerFields.NO_REAGENT_COST_1), GetUInt32(PlayerFields.NO_REAGENT_COST_1_2), GetUInt32(PlayerFields.NO_REAGENT_COST_1_3) };
            }
            internal set
            {
                SetUInt32(PlayerFields.NO_REAGENT_COST_1, value[0]);
                SetUInt32(PlayerFields.NO_REAGENT_COST_1_2, value[1]);
                SetUInt32(PlayerFields.NO_REAGENT_COST_1_3, value[2]);
            }
        }

        #endregion

        public override Faction DefaultFaction
        {
            get { return FactionMgr.Get(Race); }
        }

        public byte CharNum
        {
            get { return Record.CharNum; }
        }

        public UInt32 UniqId
        {
            get { return (uint)(Account.AccountId + 1000000 * CharNum); }
        }

        public int ReputationGainModifierPercent { get; set; }

        public int KillExperienceGainModifierPercent { get; set; }

        public int QuestExperienceGainModifierPercent
        {
            get { return 0; }
            set { QuestExperienceGainModifierPercent = value; }
        }

        #region CombatRatings

        /// <summary>
        /// Gets the total modifier of the corresponding CombatRating (in %) 
        /// </summary>
        public int GetCombatRating(CombatRating rating)
        {
            return GetInt32(PlayerFields.COMBAT_RATING_1 - 1 + (int)rating);
        }

        public void SetCombatRating(CombatRating rating, int value)
        {
            SetInt32(PlayerFields.COMBAT_RATING_1 - 1 + (int)rating, value);
            UpdateChancesByCombatRating(rating);
        }

        /// <summary>
        /// Modifies the given CombatRating modifier by the given delta
        /// </summary>
        public void ModCombatRating(CombatRating rating, int delta)
        {
            var val = GetInt32(PlayerFields.COMBAT_RATING_1 - 1 + (int)rating) + delta;
            SetInt32(PlayerFields.COMBAT_RATING_1 - 1 + (int)rating, val);
            UpdateChancesByCombatRating(rating);
        }


        public void ModCombatRating(uint[] ratings, int delta)
        {
            for (var i = 0; i < ratings.Length; i++)
            {
                var rating = ratings[i];
                ModCombatRating((CombatRating)rating, delta);
            }
        }

        #endregion

        #region Tracking of Resources & Creatures

        public CreatureMask CreatureTracking
        {
            get { return (CreatureMask)GetUInt32(PlayerFields.TRACK_CREATURES); }
            internal set { SetUInt32(PlayerFields.TRACK_CREATURES, (uint)value); }
        }

        public LockMask ResourceTracking
        {
            get { return (LockMask)GetUInt32(PlayerFields.TRACK_RESOURCES); }
            internal set { SetUInt32(PlayerFields.TRACK_RESOURCES, (uint)value); }
        }

        #endregion

        #region Misc Combat effecting Fields

        public float BlockChance
        {
            get { return GetFloat(PlayerFields.BLOCK_PERCENTAGE); }
            internal set { SetFloat(PlayerFields.BLOCK_PERCENTAGE, value); }
        }

        /// <summary>
        /// Amount of damage reduced when an attack is blocked
        /// </summary>
        public uint BlockValue
        {
            get { return GetUInt32(PlayerFields.SHIELD_BLOCK); }
            internal set { SetUInt32(PlayerFields.SHIELD_BLOCK, value); }
        }

        /// <summary>
        /// Value in %
        /// </summary>
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

        ///// <summary>
        ///// Reduces/increases the target chance to dodge the attack
        ///// </summary>
        //public int TargetDodgeChanceMod
        //{
        //    get;
        //    set;
        //}



        /// <summary>
        /// Character's hit chance in %
        /// </summary>
        public float HitChance
        {
            get;
            set;
        }

        public float RangedHitChance
        {
            get;
            set;
        }

        public override uint Defense
        {
            get;
            internal set;
        }

        /// <summary>
        /// Character spell hit chance bonus from hit rating in %
        /// </summary>
        public float SpellHitChanceFromHitRating
        {
            get
            {
                int spellHitRating = GetCombatRating(CombatRating.SpellHitChance);
                float levelFactor = GameTables.CombatRatings[CombatRating.SpellHitChance][CasterLevel - 1];
                return spellHitRating / levelFactor;
            }
        }
        #endregion

        #region Quest Fields

        public void ResetQuest(int slot)
        {
            var i = slot * QuestConstants.UpdateFieldCountPerQuest;
            SetUInt32((PlayerFields.QUEST_LOG_1_1 + i), 0);
            SetUInt32((PlayerFields.QUEST_LOG_1_2 + i), 0);
            SetUInt32((PlayerFields.QUEST_LOG_1_3 + i), 0);
            SetUInt32((PlayerFields.QUEST_LOG_1_3_2 + i), 0);
            SetUInt32((PlayerFields.QUEST_LOG_1_4 + i), 0);
        }

        /// <summary>
        /// Gets the quest field.
        /// </summary>
        /// <param name="slot">The slot.</param>
        public uint GetQuestId(int slot)
        {
            return GetUInt32(PlayerFields.QUEST_LOG_1_1 + (slot * QuestConstants.UpdateFieldCountPerQuest));
        }

        /// <summary>
        /// Sets the quest field, where fields are indexed from 0.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <param name="questid">The questid.</param>
        public void SetQuestId(int slot, uint questid)
        {
            SetUInt32((PlayerFields.QUEST_LOG_1_1 + (slot * QuestConstants.UpdateFieldCountPerQuest)), questid);
        }

        /// <summary>
        /// Gets the state of the quest.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <returns></returns>
        public QuestCompleteStatus GetQuestState(int slot)
        {
            return (QuestCompleteStatus)GetUInt32(PlayerFields.QUEST_LOG_1_2 + (slot * QuestConstants.UpdateFieldCountPerQuest));
        }

        /// <summary>
        /// Sets the state of the quest.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <param name="completeStatus">The status.</param>
        public void SetQuestState(int slot, QuestCompleteStatus completeStatus)
        {
            SetUInt32((PlayerFields.QUEST_LOG_1_2 + (slot * QuestConstants.UpdateFieldCountPerQuest)), (uint)completeStatus);
        }

        /// <summary>
        /// Sets the quest count at the given index for the given Quest to the given value.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <param name="interactionIndex">The count slot.</param>
        /// <param name="value">The value.</param>
        internal void SetQuestCount(int slot, uint interactionIndex, ushort value)
        {
            // each quest has 4 quest counters
            // each counter has 2 bytes
            var field = (slot * QuestConstants.UpdateFieldCountPerQuest) + PlayerFields.QUEST_LOG_1_3 + ((int)interactionIndex >> 1);
            var hiLo = interactionIndex % 2;
            if (hiLo == 0)
            {
                SetUInt16Low(field, value);
            }
            else
            {
                SetUInt16High(field, value);
            }
        }

        /// <summary>
        /// Gets the quest time.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <returns></returns>
        internal uint GetQuestTimeLeft(byte slot)
        {
            return GetUInt32(PlayerFields.QUEST_LOG_1_4 + (slot * QuestConstants.UpdateFieldCountPerQuest));
        }

        /// <summary>
        /// Sets the quest time.
        /// </summary>
        /// <param name="slot">The slot.</param>
        internal void SetQuestTimeLeft(byte slot, uint timeleft)
        {
            SetUInt32(PlayerFields.QUEST_LOG_1_4 + (slot * QuestConstants.UpdateFieldCountPerQuest), timeleft);
        }

        /*
        uint16 FindQuestSlot( uint32 quest_id ) const;
        uint32 GetQuestSlotQuestId(uint16 slot) const { return GetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_ID_OFFSET); }
        uint32 GetQuestSlotState(uint16 slot)   const { return GetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_STATE_OFFSET); }
        uint32 GetQuestSlotCounters(uint16 slot)const { return GetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_COUNTS_OFFSET); }
        uint8 GetQuestSlotCounter(uint16 slot,uint8 counter) const { return GetByteValue(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_COUNTS_OFFSET,counter); }
        uint32 GetQuestSlotTime(uint16 slot)    const { return GetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_TIME_OFFSET); }
        void SetQuestSlot(uint16 slot,uint32 quest_id, uint32 timer = 0)
        {
            SetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_ID_OFFSET,quest_id);
            SetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_STATE_OFFSET,0);
            SetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_COUNTS_OFFSET,0);
            SetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_TIME_OFFSET,timer);
        }
        void SetQuestSlotCounter(uint16 slot,uint8 counter,uint8 count) { SetByteValue(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_COUNTS_OFFSET,counter,count); }
        void SetQuestSlotState(uint16 slot,uint32 state) { SetFlag(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_STATE_OFFSET,state); }
        void RemoveQuestSlotState(uint16 slot,uint32 state) { RemoveFlag(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_STATE_OFFSET,state); }
        void SetQuestSlotTimer(uint16 slot,uint32 timer) { SetUInt32Value(PLAYER_QUEST_LOG_1_1 + slot*MAX_QUEST_OFFSET + QUEST_TIME_OFFSET,timer); }
        }*/

        #region Daily quests

        /// <summary>
        /// This array stores completed daily quests
        /// </summary>
        /// <returns></returns>
        //TODO change return type to Quest
        public uint[] DailyQuests
        {
            get
            {
                var dailyquestids = new uint[25];
                for (var i = 0; i < 25; i++)
                {
                    dailyquestids[i] = GetUInt32((PlayerFields.DAILY_QUESTS_1 + i));
                }
                return dailyquestids;
            }
        }

        /// <summary>
        /// Gets the quest field.
        /// </summary>
        /// <param name="slot">The slot.</param>
        public uint GetDailyQuest(byte slot)
        {
            //TODO Do we need to check if slot is > 25?
            return GetUInt32((PlayerFields.DAILY_QUESTS_1 + slot));
        }

        /// <summary>
        /// Sets the quest field, where fields are indexed from 0.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <param name="questid">The questid.</param>
        public void SetDailyQuest(byte slot, uint questid)
        {
            //TODO Do we need to check if slot is > 25?
            SetUInt32((PlayerFields.DAILY_QUESTS_1 + slot), questid);
        }

        public void ResetDailyQuests()
        {
            for (int i = 0; i < 25; i++)
            {
                SetUInt32((PlayerFields.DAILY_QUESTS_1 + i), 0);
            }
        }

        #endregion

        #endregion

        #region Damage
        /// <summary>
        /// Modifies the damage for the given school by the given delta.
        /// </summary>
        protected internal override void AddDamageDoneModSilently(DamageSchool school, int delta)
        {
            PlayerFields field;
            if (delta == 0)
            {
                return;
            }
            if (delta > 0)
            {
                field = PlayerFields.MOD_DAMAGE_DONE_POS;
            }
            else
            {
                field = PlayerFields.MOD_DAMAGE_DONE_NEG;
            }
            SetInt32(field + (int)school, GetInt32(field + (int)school) + delta);
        }

        /// <summary>
        /// Modifies the damage for the given school by the given delta.
        /// </summary>
        protected internal override void RemoveDamageDoneModSilently(DamageSchool school, int delta)
        {
            PlayerFields field;
            if (delta == 0)
            {
                return;
            }
            if (delta > 0)
            {
                field = PlayerFields.MOD_DAMAGE_DONE_POS;
            }
            else
            {
                field = PlayerFields.MOD_DAMAGE_DONE_NEG;
            }
            SetUInt32(field + (int)school, GetUInt32(field + (int)school) - (uint)delta);
        }

        protected internal override void ModDamageDoneFactorSilently(DamageSchool school, float delta)
        {
            if (delta == 0)
            {
                return;
            }
            var field = PlayerFields.MOD_DAMAGE_DONE_PCT + (int)school;
            SetFloat(field, GetFloat(field) + delta);
        }

        public override float GetDamageDoneFactor(DamageSchool school)
        {
            return GetFloat(PlayerFields.MOD_DAMAGE_DONE_PCT + (int)school);
        }

        public override int GetDamageDoneMod(DamageSchool school)
        {
            return GetInt32(PlayerFields.MOD_DAMAGE_DONE_POS + (int)school) -
                    GetInt32(PlayerFields.MOD_DAMAGE_DONE_NEG + (int)school);
        }
        #endregion

        #region Healing Done

        /// <summary>
        /// Increased healing done *by* this Character
        /// </summary>
        public int HealingDoneMod
        {
            get { return GetInt32(PlayerFields.MOD_HEALING_DONE_POS); }
            set { SetInt32(PlayerFields.MOD_HEALING_DONE_POS, value); }
        }

        /// <summary>
        /// Increased healing % done *by* this Character
        /// </summary>
        public float HealingDoneModPct
        {
            get { return GetFloat(PlayerFields.MOD_HEALING_DONE_PCT); }
            set { SetFloat(PlayerFields.MOD_HEALING_DONE_PCT, value); }
        }

        /// <summary>
        /// Increased healing done *to* this Character
        /// </summary>
        public float HealingTakenModPct
        {
            get { return GetFloat(PlayerFields.MOD_HEALING_PCT); }
            set { SetFloat(PlayerFields.MOD_HEALING_PCT, value); }
        }
        #endregion

        /// <summary>
        /// Returns the SpellCritChance for the given DamageType (0-100)
        /// </summary>
        public override float GetCritChance(DamageSchool school)
        {
            return GetFloat(PlayerFields.SPELL_CRIT_PERCENTAGE1 + (int)school);
        }

        /// <summary>
        /// Sets the SpellCritChance for the given DamageType
        /// </summary>
        internal void SetCritChance(DamageSchool school, float val)
        {
            SetFloat(PlayerFields.SPELL_CRIT_PERCENTAGE1 + (int)school, val);
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
            var offset = PlayerFields.VISIBLE_ITEM_1_ENTRYID + ((int)slot * ItemConstants.PlayerFieldVisibleItemSize);
            if (item != null)
            {
                SetUInt32(offset, item.Template.Id);
            }
            else
            {
                SetUInt32(offset, 0);
            }
        }

        #region Action Buttons
        /// <summary>
        /// Sets an ActionButton with the given information.
        /// </summary>
        public void BindActionButton(uint btnIndex, uint action, byte type, bool update = true)
        {
            CurrentSpecProfile.State = RecordState.Dirty;
            var actions = CurrentSpecProfile.ActionButtons;
            btnIndex = btnIndex * 4;
            if (action == 0)
            {
                // unset it
                Array.Copy(ActionButton.EmptyButton, 0, actions, btnIndex, ActionButton.Size);
            }
            else
            {
                actions[btnIndex] = (byte)(action & 0x0000FF);
                actions[btnIndex + 1] = (byte)((action & 0x00FF00) >> 8);
                actions[btnIndex + 2] = (byte)((action & 0xFF0000) >> 16);
                actions[btnIndex + 3] = type;
            }
        }

        public uint GetActionFromActionButton(int buttonIndex)
        {
            var actions = CurrentSpecProfile.ActionButtons;
            buttonIndex = buttonIndex * 4;

            var action = BitConverter.ToUInt32(actions, buttonIndex);
            action = action & 0x00FFFFFF;

            return action;
        }

        public byte GetTypeFromActionButton(int buttonIndex)
        {
            buttonIndex = buttonIndex * 4;
            return CurrentSpecProfile.ActionButtons[buttonIndex + 3];
        }

        /// <summary>
        /// Sets the given button to the given spell and resends it to the client
        /// </summary>
        public void BindSpellToActionButton(uint btnIndex, SpellId spell, bool update = true)
        {
            BindActionButton(btnIndex, (uint)spell, 0);
        }

        /// <summary>
        /// Sets the given action button
        /// </summary>
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
        #endregion

        #region Custom Properties

        public override ObjectTypeCustom CustomType
        {
            get { return ObjectTypeCustom.Object | ObjectTypeCustom.Unit | ObjectTypeCustom.Player; }
        }
        #endregion

        public CharacterRecord Record
        {
            get { return m_record; }
        }

        /// <summary>
        /// The active ticket of this Character or null if there is none
        /// </summary>
        public Ticket Ticket { get; internal set; }

        #region Base Unit Fields Overrides
        public override int Health
        {
            get { return base.Health; }
            set
            {
                if (Health == value)
                    return;
                base.Health = value;
                if (Map == null)
                    return;
                UpdateTargeters();
                if (IsInGroup)
                    Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
                if (IsSoulmated)
                    Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(Client);
            }
        }

        public override int MaxHealth
        {
            get { return base.MaxHealth; }
            internal set
            {
                base.MaxHealth = value;
                if (Map == null)
                    return;
                Asda2CharacterHandler.SendHealthUpdate(this);
                if (IsInGroup)
                    Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
                if (IsSoulmated)
                    Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(Client);
                UpdateTargeters();
            }
        }
        void UpdateTargeters()
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
                if (Power == value)
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
                //Update Group Update flags
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
                if (Power > MaxPower)
                    Power = MaxPower;
                Asda2CharacterHandler.SendCharMpUpdateResponse(this);
                if (IsInGroup)
                    Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
                if (IsSoulmated)
                    Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(Client);
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
                if (IsInGuild)
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
                if (Map == null)
                    return;
                if (IsInGroup)
                    Asda2GroupHandler.SendPartyMemberInitialInfoResponse(this);
                if (IsSoulmated)
                    Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(Client);
                if (IsInGuild)
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
                if (m_zone != value)
                {
                    if (value != null)
                    {
                        if (m_Map != null)
                        {
                            value.EnterZone(this, m_zone);
                        }
                    }
                    base.Zone = value;

                    //Update Group Update flags
                    GroupUpdateFlags |= GroupUpdateFlags.ZoneId;
                }
            }
        }

        public bool IsZoneExplored(ZoneId id)
        {
            var zone = World.GetZoneInfo(id);
            return zone != null && IsZoneExplored(zone);
        }

        public bool IsZoneExplored(ZoneTemplate zone)
        {
            return IsZoneExplored(zone.ExplorationBit);
        }

        public bool IsZoneExplored(int explorationBit)
        {
            // index of the byte within m_record.ExploredZones[index] that contains the bit
            var byteNo = explorationBit >> 3;

            // ExploredZones contains 512 bytes and thus 512/4 = 128 fields
            if ((byteNo >> 2) >= UpdateFieldMgr.ExplorationZoneFieldSize)
            {
                // Value is out of range, get out of here!
                return false;
            }

            // the position of the bit within it's byte
            var bit = explorationBit % 8;
            var bitMask = 1 << bit;

            return (m_record.ExploredZones[byteNo] & bitMask) != 0;
        }

        public void SetZoneExplored(ZoneId id, bool explored)
        {
            /*var zone = World.GetZoneInfo(id);
            if (zone != null)
            {
                SetZoneExplored(zone, explored);
            }*/
        }

        public void SetZoneExplored(ZoneTemplate zone, bool gainXp)
        {
            /*// index of the field that contains the bit
            var fieldNo = zone.ExplorationBit >> 5;
            if (fieldNo >= UpdateFieldMgr.ExplorationZoneFieldSize)
            {
                return;
            }

            // index of the byte that contains the bit
            var byteNo = zone.ExplorationBit >> 3;

            // the position of the bit within it's byte
            var bit = (zone.ExplorationBit) % 8;

            // the mask inside the byte
            var bitMask = 1 << bit;

            // the value of the byte
            var byteVal = m_record.ExploredZones[byteNo];

            if ((byteVal & bitMask) == 0)
            {
                // not explored yet
                if (gainXp)
                {
                    var xp = XpGenerator.GetExplorationXp(zone, this);
                    if (xp > 0)
                    {
                        if (Level >= RealmServerConfiguration.MaxCharacterLevel)
                        {
                            // already at level cap
                            CharacterHandler.SendExplorationExperience(this, zone.Id, 0);
                        }
                        else
                        {
                            // gain XP
                            GainXp( xp);
                            CharacterHandler.SendExplorationExperience(this, zone.Id, xp);
                        }
                    }
                }

                // set the bit client side
                var newValue = (byte)(byteVal | bitMask);
                SetByte((int)PlayerFields.EXPLORED_ZONES_1 + fieldNo, byteNo % 4, newValue);

                // cache the new value for easy access
                m_record.ExploredZones[byteNo] = newValue;

                // check possible achievements
                foreach (var worldMapOverlay in zone.WorldMapOverlays)
                {
                    Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.ExploreArea, (uint)worldMapOverlay);
                }

                // explore parent
                if (zone.ParentZone != null)
                {
                    SetZoneExplored(zone.ParentZone, gainXp);
                }
            }*/
        }

        public override Vector3 Position
        {
            get { return base.Position; }
            internal set
            {
                base.Position = value;
                //Update Group Update flags
                GroupUpdateFlags |= GroupUpdateFlags.Position;
            }
        }

        public override uint Phase
        {
            get { return m_Phase; }
            set
            {
                m_Phase = value;
            }
        }

        #endregion

        #region Misc Properties

        public override bool IsInWorld
        {
            get { return m_initialized; }
        }

        /// <summary>
        /// The type of this object (player, corpse, item, etc)
        /// </summary>
        public override ObjectTypeId ObjectTypeId
        {
            get { return ObjectTypeId.Player; }
        }

        /// <summary>
        /// The client currently playing the character.
        /// </summary>
        public IRealmClient Client
        {
            get { return m_client; }
            protected set { m_client = value; }
        }

        /// <summary>
        /// The status of the character.
        /// </summary>
        public CharacterStatus Status
        {
            get
            {
                CharacterStatus status = CharacterStatus.OFFLINE;

                if (IsAFK)
                    status |= CharacterStatus.AFK;

                if (IsDND)
                    status |= CharacterStatus.DND;

                if (IsInWorld)
                    status |= CharacterStatus.ONLINE;

                return status;
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
                if (m_guildMember != null)
                {
                    GuildId = (uint)m_guildMember.Guild.Id;
                    GuildRank = (uint)m_guildMember.RankId;
                }
                else
                {
                    GuildId = 0;
                    GuildRank = 0;
                }
            }
        }

        /// <summary>
        /// The ArenaTeamMember object of this Character (if it he/she is in an arena team)
        /// </summary>
        public ArenaTeamMember[] ArenaTeamMember
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
                if (m_groupMember != null)
                {
                    var subGroup = m_groupMember.SubGroup;
                    return subGroup != null ? subGroup.Group : null;
                }
                return null;
            }
        }

        /// <summary>
        /// The subgroup in which the character is (if any)
        /// </summary>
        public SubGroup SubGroup
        {
            get
            {
                if (m_groupMember != null)
                {
                    return m_groupMember.SubGroup;
                }
                return null;
            }
        }

        public GroupUpdateFlags GroupUpdateFlags
        {
            get { return m_groupUpdateFlags; }
            set { m_groupUpdateFlags = value; }
        }

        /// <summary>
        /// The guild in which the character is (if any)
        /// </summary>
        public Guild Guild
        {
            get
            {
                if (m_guildMember != null)
                {
                    return m_guildMember.Guild;
                }
                return null;
            }
        }

        /// <summary>
        /// The account this character belongs to.
        /// </summary>
        public RealmAccount Account { get; protected internal set; }

        public RoleGroup Role
        {
            get
            {
                var acc = Account;
                return acc != null ? acc.Role : Singleton<PrivilegeMgr>.Instance.LowestRole;
            }
        }

        public override ClientLocale Locale
        {
            get { return m_client.Info.Locale; }
            set { m_client.Info.Locale = value; }
        }

        /// <summary>
        /// The name of this character.
        /// </summary>
        public override string Name
        {
            get { return m_name; }
            set
            {
                throw new NotImplementedException("Dynamic renaming of Characters is not yet implemented.");
                //m_name = value;
            }
        }

        public Corpse Corpse
        {
            get { return m_corpse; }
            internal set
            {
                if (value == null && m_corpse != null)
                {
                    m_corpse.StartDecay();
                    m_record.CorpseX = null;
                }
                m_corpse = value;
            }
        }

        /// <summary>
        /// The <see cref="Archetype">Archetype</see> of this Character
        /// </summary>
        public Archetype Archetype
        {
            get { return m_archetype; }
            set
            {
                m_archetype = value;
                Race = value.Race.Id;
                Class = value.Class.Id;
                Asda2TitleChecker.OnClassChange(Class, this, RealProffLevel);
                Asda2CharacterHandler.SendChangeProfessionResponse(Client);
                if (IsInGuild)
                    Asda2GuildHandler.SendGuildNotificationResponse(Guild, GuildNotificationType.Silence, GuildMember);
            }
        }

        ///<summary>
        ///</summary>
        public byte Outfit { get; set; }

        /// <summary>
        /// The channels the character is currently joined to.
        /// </summary>
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

        /// <summary>
        /// Last login time of this character.
        /// </summary>
        public DateTime LastLogin
        {
            get { return m_record.LastLogin.Value; }
            set { m_record.LastLogin = value; }
        }

        /// <summary>
        /// Last logout time of this character.
        /// </summary>
        public DateTime? LastLogout
        {
            get { return m_record.LastLogout; }
            set { m_record.LastLogout = value; }
        }

        public bool IsFirstLogin
        {
            get { return m_record.LastLogout == null; }
        }

        public TutorialFlags TutorialFlags { get; set; }

        /// <summary>
        /// Total play time of this Character in seconds
        /// </summary>
        public uint TotalPlayTime
        {
            get { return (uint)m_record.TotalPlayTime; }
            set { m_record.TotalPlayTime = (int)value; }
        }

        /// <summary>
        /// How long is this Character already on this level in seconds
        /// </summary>
        public uint LevelPlayTime
        {
            get { return (uint)m_record.LevelPlayTime; }
            set { m_record.LevelPlayTime = (int)value; }
        }

        /// <summary>
        /// Whether or not this character has the GM-tag set.
        /// </summary>
        public bool ShowAsGameMaster
        {
            get { return PlayerFlags.HasFlag(PlayerFlags.GM); }
            set
            {
                if (value)
                {
                    PlayerFlags |= PlayerFlags.GM;
                }
                else
                {
                    PlayerFlags &= ~PlayerFlags.GM;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the godmode
        /// </summary>
        public bool GodMode
        {
            get { return m_record.GodMode; }
            set
            {
                m_record.GodMode = value;
                var cast = m_spellCast;
                if (cast != null)
                {
                    cast.GodMode = value;
                }

                if (value)
                {
                    Health = MaxHealth;
                    Power = MaxPower;
                    //NoReagentCost = new[] {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};

                    // clear cooldowns
                    m_spells.ClearCooldowns();
                    ShowAsGameMaster = true;

                    // make invulnerable
                    IncMechanicCount(SpellMechanic.Invulnerable);
                }
                else
                {
                    //NoReagentCost = new[] {0u, 0u, 0u};
                    DecMechanicCount(SpellMechanic.Invulnerable);
                    ShowAsGameMaster = false;
                }
            }
        }

        protected override void InitSpellCast()
        {
            base.InitSpellCast();
            m_spellCast.GodMode = GodMode;
        }

        /// <summary>
        /// Whether the PvP Flag is set.
        /// </summary>
        public bool IsPvPFlagSet
        {
            get { return PlayerFlags.HasFlag(PlayerFlags.PVP); }
            set
            {
                if (value)
                {
                    PlayerFlags |= PlayerFlags.PVP;
                    return;
                }
                PlayerFlags &= ~PlayerFlags.PVP;
            }
        }

        /// <summary>
        /// Whether the PvP Flag reset timer is active.
        /// </summary>
        public bool IsPvPTimerActive
        {
            get { return PlayerFlags.HasFlag(PlayerFlags.PVPTimerActive); }
            set
            {
                if (value)
                {
                    PlayerFlags |= PlayerFlags.PVPTimerActive;
                    return;
                }
                PlayerFlags &= ~PlayerFlags.PVPTimerActive;
            }
        }

        #region AFK & DND etc
        /// <summary>
        /// Whether or not this character is AFK.
        /// </summary>
        public bool IsAFK
        {
            get { return PlayerFlags.HasFlag(PlayerFlags.AFK); }
            set
            {
                if (value)
                {
                    PlayerFlags |= PlayerFlags.AFK;
                }
                else
                {
                    PlayerFlags &= ~PlayerFlags.AFK;
                }
                GroupUpdateFlags |= GroupUpdateFlags.Status;
            }
        }

        /// <summary>
        /// The custom AFK reason when player is AFK.
        /// </summary>
        public string AFKReason { get; set; }

        /// <summary>
        /// Whether or not this character is DND.
        /// </summary>
        public bool IsDND
        {
            get { return PlayerFlags.HasFlag(PlayerFlags.DND); }
            set
            {
                if (value)
                {
                    PlayerFlags |= PlayerFlags.DND;
                }
                else
                {
                    PlayerFlags &= ~PlayerFlags.DND;
                }
                GroupUpdateFlags |= GroupUpdateFlags.Status;
            }
        }

        /// <summary>
        /// The custom DND reason when player is DND.
        /// </summary>
        public string DNDReason { get; set; }

        /// <summary>
        /// Gets the chat tag for the character.
        /// </summary>
        public override ChatTag ChatTag
        {
            get
            {
                if (ShowAsGameMaster)
                {
                    return ChatTag.GM;
                }
                if (IsAFK)
                {
                    return ChatTag.AFK;
                }
                if (IsDND)
                {
                    return ChatTag.DND;
                }

                return ChatTag.None;
            }
        }
        #endregion

        #region Interfaces & Collections
        /// <summary>
        /// Collection of reputations with all factions known to this Character
        /// </summary>
        public ReputationCollection Reputations
        {
            get { return m_reputations; }
        }

        /// <summary>
        /// Collection of all this Character's skills
        /// </summary>
        public SkillCollection Skills
        {
            get { return m_skills; }
        }

        /// <summary>
        /// Collection of all this Character's Talents
        /// </summary>
        public override TalentCollection Talents
        {
            get { return m_talents; }
        }

        /// <summary>
        /// Collection of all this Character's Achievements
        /// </summary>
        public AchievementCollection Achievements
        {
            get { return m_achievements; }
        }

        /// <summary>
        /// All spells known to this chr
        /// </summary>
        public PlayerAuraCollection PlayerAuras
        {
            get { return (PlayerAuraCollection)m_auras; }
        }

        /// <summary>
        /// All spells known to this chr
        /// </summary>
        public PlayerSpellCollection PlayerSpells
        {
            get { return (PlayerSpellCollection)m_spells; }
        }

        /// <summary>
        /// Mask of the activated Flight Paths
        /// </summary>
        public TaxiNodeMask TaxiNodes
        {
            get { return m_taxiNodeMask; }
        }

        /// <summary>
        /// The Tavern-location of where the Player bound to
        /// </summary>
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

        private Asda2PlayerInventory _asda2Inventory;


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


        /// <summary>
        /// The Character's MailAccount
        /// </summary>
        public MailAccount MailAccount
        {
            get { return m_mailAccount; }
            set
            {
                if (m_mailAccount != value)
                {
                    m_mailAccount = value;
                }
            }
        }
        #endregion

        /// <summary>
        /// Unused talent-points for this Character
        /// </summary>
        public int FreeTalentPoints
        {
            get { return (int)GetUInt32(PlayerFields.CHARACTER_POINTS1); }
            set
            {
                if (value < 0)
                    value = 0;

                //m_record.FreeTalentPoints = value;
                SetUInt32(PlayerFields.CHARACTER_POINTS1, (uint)value);
                TalentHandler.SendTalentGroupList(m_talents);
            }
        }

        /// <summary>
        /// Doesn't send a packet to the client
        /// </summary>
        public void UpdateFreeTalentPointsSilently(int delta)
        {
            SetUInt32(PlayerFields.CHARACTER_POINTS1, (uint)(FreeTalentPoints + delta));
        }

        /// <summary>
        /// Forced logout must not be cancelled
        /// </summary>
        public bool IsKicked
        {
            get { return m_isLoggingOut && !IsPlayerLogout; }
        }

        /// <summary>
        /// The current GossipConversation that this Character is having
        /// </summary>
        public GossipConversation GossipConversation { get; set; }

        /// <summary>
        /// Lets the Character gossip with the given speaker
        /// </summary>
        public void StartGossip(GossipMenu menu, WorldObject speaker)
        {
            GossipConversation = new GossipConversation(menu, this, speaker, menu.KeepOpen);
            GossipConversation.DisplayCurrentMenu();
        }

        /// <summary>
        /// Lets the Character gossip with herself
        /// </summary>
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
            get { return Singleton<RelationMgr>.Instance.HasPassiveRelations(EntityId.Low, CharacterRelationType.GroupInvite); }
        }

        /// <summary>
        /// Returns whether this Character is invited into a Guild already
        /// </summary>
        /// <returns></returns>
        public bool IsInvitedToGuild
        {
            get { return Singleton<RelationMgr>.Instance.HasPassiveRelations(EntityId.Low, CharacterRelationType.GuildInvite); }
        }

        public bool HasTitle(TitleId titleId)
        {
            var titleEntry = TitleMgr.GetTitleEntry(titleId);
            if (titleEntry == null)
            {
                // TO-DO: report about an error
                return false;
            }
            var bitIndex = titleEntry.BitIndex;

            var fieldIndexOffset = (int)bitIndex / 32 + (int)PlayerFields._FIELD_KNOWN_TITLES;
            uint flag = (uint)(1 << (int)bitIndex % 32);

            return ((CharTitlesMask)GetUInt32(fieldIndexOffset)).HasFlag((CharTitlesMask)flag);
        }

        public bool HasTitle(TitleBitId titleBitId)
        {
            CharacterTitleEntry titleEntry = TitleMgr.GetTitleEntry(titleBitId);
            if (titleEntry == null)
                return false;
            return HasTitle(titleEntry.TitleId);
        }

        public void SetTitle(TitleId titleId, bool lost)
        {
            var titleEntry = TitleMgr.GetTitleEntry(titleId);
            if (titleEntry == null)
            {
                log.Warn(String.Format("TitleId: {0} could not be found.", (uint)titleId));
                return;
            }
            var bitIndex = titleEntry.BitIndex;

            var fieldIndexOffset = (int)bitIndex / 32 + (int)PlayerFields._FIELD_KNOWN_TITLES;
            var flag = (uint)(1 << (int)bitIndex % 32);

            if (lost)
            {
                if (!HasTitle(titleId))
                    return;

                var value = GetUInt32(fieldIndexOffset) & ~flag;
                SetUInt32(fieldIndexOffset, value);
            }
            else
            {
                if (HasTitle(titleId))
                    return;

                var value = GetUInt32(fieldIndexOffset) | flag;
                SetUInt32(fieldIndexOffset, value);
            }

            TitleHandler.SendTitleEarned(this, titleEntry, lost);

        }

        #endregion

        #region Glyphs
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
                switch (Class)
                {
                    case ClassId.THS:
                        return Asda2Profession.Warrior;
                    case ClassId.Spear:
                        return Asda2Profession.Warrior;
                    case ClassId.OHS:
                        return Asda2Profession.Warrior;
                    case ClassId.AtackMage:
                        return Asda2Profession.Mage;
                    case ClassId.SupportMage:
                        return Asda2Profession.Mage;
                    case ClassId.HealMage:
                        return Asda2Profession.Mage;
                    case ClassId.Bow:
                        return Asda2Profession.Archer;
                    case ClassId.Crossbow:
                        return Asda2Profession.Archer;
                    case ClassId.Balista:
                        return Asda2Profession.Archer;
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
                if (IsInGuild)
                    Asda2GuildHandler.SendGuildNotificationResponse(Guild, GuildNotificationType.Silence, GuildMember);
                Asda2TitleChecker.OnClassChange(Archetype.ClassId, this, RealProffLevel);
            }
        }




        public int MagicDefence { get; set; }

        public Asda2ClassMask Asda2ClassMask
        {
            get
            {
                return Archetype.Class.ClassMask;
            }
        }

        public byte EyesColor
        {
            get { return Record.EyesColor; }
            set { Record.EyesColor = value; }
        }
        public int TimeFromLastPositionUpdate = 0;

        //TODO Calc RATING
        public int PlaceInRating { get; set; }
        //TODO ASDA2 PET SYSTEM
        public Asda2PetRecord Asda2Pet { get; set; }
        //TODO ASDA2 FACTION SYSTEM
        /// <summary>
        /// 0 - Light; 1- Dark; 2 - Chaos; -1 - None
        /// </summary>
        public short Asda2FactionId
        {
            get { return Record.Asda2FactionId; }
            set
            {
                Record.Asda2FactionId = value;
                if (Map == null) return;
                foreach (var obj in NearbyObjects)
                {
                    if (!(obj is Character))
                        continue;
                    var chr = obj as Character;
                    CheckAtackStateWithCharacter(chr);
                    chr.CheckAtackStateWithCharacter(this);
                }
                GlobalHandler.SendCharacterFactionToNearbyCharacters(this);
            }
        }

        /// <summary>
        /// 0 - 20
        /// </summary>
        public short Asda2FactionRank
        {
            get { return _asda2FactionRank; }
            set { _asda2FactionRank = value; }
        }

        public int Asda2HonorPoints
        {
            get { return Record.Asda2HonorPoints; }
            set { if (Record.Asda2HonorPoints == value)return; Record.Asda2HonorPoints = value; Asda2CharacterHandler.SendFactionAndHonorPointsInitResponse(Client); RecalculateFactionRank(); }
        }
        private void RecalculateFactionRank(bool silent = false)
        {
            var rank = CharacterFormulas.GetFactionRank(Asda2HonorPoints);
            if (Asda2FactionRank != rank)
            {
                Asda2FactionRank = (short)rank;
                Asda2TitleChecker.OnFactionRankChanged(this,rank);
            }
            if (!silent)
            {
                GlobalHandler.SendCharacterFactionToNearbyCharacters(this);
                Asda2CharacterHandler.SendFactionAndHonorPointsInitResponse(Client);
            }

        }

        public string SoulmateIntroduction
        {
            get { return Account.AccountData.SoulmateIntroduction; }
            set { Account.AccountData.SoulmateIntroduction = value; }
        }

        public Asda2SoulmateRelationRecord SoulmateRecord { get; set; }


        public bool IsSoulmated { get { return SoulmateRecord != null; } }

        public CharacterRecord[] SoulmatedCharactersRecords { get; set; }

        public Character SoulmateCharacter
        {
            get
            {
                if (SoulmateRealmAccount == null) return null;
                return SoulmateRealmAccount.ActiveCharacter;
            }
        }

        public RealmAccount SoulmateRealmAccount { get; set; }
        public int GuildPoints
        {
            get { return Record.GuildPoints; }
            set { Record.GuildPoints = value; Asda2GuildHandler.SendUpdateGuildPointsResponse(Client); }
        }

        public static Color GmChatColor = Color.OrangeRed;
        private bool _isAsda2TradeDescriptionEnabled;
        private string _asda2TradeDescription;


        public Color ChatColor
        {
            get { return GetChatColor(); }
            set { Record.GlobalChatColor = value; }
        }

        private Color GetChatColor()
        {
            switch (Role.Status)
            {
                case RoleStatus.GA1:
                    return Color.Brown;
                case RoleStatus.GA2:
                    return Color.SaddleBrown;
                case RoleStatus.Besan:
                    return Color.LightPink;
                case RoleStatus.Erza:
                    return Color.LightBlue;
                case RoleStatus.FeDRAaLe:
                    return Color.LawnGreen;
                case RoleStatus.Hamura:
                    return Color.SeaGreen;
                case RoleStatus.Xluise:
                    return Color.HotPink;
                case RoleStatus.VIP:
                    return Color.LightPink;
                case RoleStatus.GA:
                    return Color.Green;
                case RoleStatus.Admin:
                    return GmChatColor;
                case RoleStatus.EventManager:
                    return Color.Aquamarine;
                default:
                    return Color.Yellow;
            }

        }

        public int FishingLevel
        {
            get { return Record.FishingLevel + GetIntMod(StatModifierInt.Asda2FishingSkill); }
            set { Record.FishingLevel = value; Asda2FishingHandler.SendFishingLvlResponse(Client); }
        }

        public uint AccId
        {
            get { return (uint)Account.AccountId; }
        }

        public int AvatarMask
        {
            get { return Record.AvatarMask; }
            set { Record.AvatarMask = value; Asda2CharacterHandler.SendUpdateAvatarMaskResponse(this); }
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
        public UpdateMask DiscoveredTitles { get; set; }
        public UpdateMask GetedTitles { get; set; }
        public bool IsAsda2TradeDescriptionEnabled
        {
            get { return _isAsda2TradeDescriptionEnabled; }
            set { if (_isAsda2TradeDescriptionEnabled == value) return; _isAsda2TradeDescriptionEnabled = value; Map.CallDelayed(777, () => Asda2PrivateShopHandler.SendtradeStatusTextWindowResponse(this)); }
        }

        public bool IsAsda2TradeDescriptionPremium { get; set; }

        public string Asda2TradeDescription
        {
            get { return _asda2TradeDescription ?? (_asda2TradeDescription = ""); }
            set { _asda2TradeDescription = value; if (IsAsda2TradeDescriptionEnabled) Asda2PrivateShopHandler.SendtradeStatusTextWindowResponse(this); }
        }

        public Asda2PrivateShop PrivateShop { get; set; }



        public Asda2Pvp Asda2Duel { get; set; }
        public bool IsAsda2Dueling { get { return Asda2Duel != null; } }

        public Character Asda2DuelingOponent { get; set; }

        public byte GreenCharges { get; set; }
        public byte BlueCharges { get; set; }
        public byte RedCharges { get; set; }

        public byte Asda2GuildRank
        {
            get { return (byte)(4 - GuildRank); }
            set { GuildRank = (uint)(4 - value); }
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
            get { return CurrentBattleGround != null && CurrentBattleGround.IsRunning && MapId == MapId.BatleField; }
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
            set { _asda2WingsItemId = value; if (Map != null)FunctionalItemsHandler.SendWingsInfoResponse(this, null); }
        }

        public short TransformationId
        {
            get { return _transformationId; }
            set
            {
                _transformationId = VerifyTransformationId(value);
                GlobalHandler.SendTransformToPetResponse(this, _transformationId != -1);
            }
        }

        private short VerifyTransformationId(short value)
        {
            if (value == 0
                || value == 190 || value == 192
                || value == 197 || value == 373
                || value == 551 || value == 551
                || value > 843)
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
            get
            {
                return Record.ChatBanned;
            }
            set
            {
                Record.ChatBanned = value;
            }
        }

        public DateTime? BanChatTill
        {
            get { return Record.BanChatTill; }
            set { Record.BanChatTill = value; }
        }

        public Asda2TitleProgress TitleProgress { get; set; }

        public int GetTitlesCount()
        {
            var count = 0;
            for (int i = 0; i < GetedTitles.MaxIndex; i++)
            {
                if (GetedTitles.GetBit(i))
                    count++;
            }
            return count;
        }


        public void SetGlyphSlot(byte slot, uint id)
        {
            SetUInt32(PlayerFields.GLYPH_SLOTS_1 + slot, id);
        }
        public uint GetGlyphSlot(byte slot)
        {
            return GetUInt32(PlayerFields.GLYPH_SLOTS_1 + slot);
        }
        public void SetGlyph(byte slot, uint glyph)
        {
            SetUInt32(PlayerFields.GLYPHS_1 + slot, glyph);
        }
        public uint GetGlyph(byte slot)
        {
            return GetUInt32(PlayerFields.GLYPHS_1 + slot);
        }
        #endregion

        public Dictionary<int, Asda2PetRecord> OwnedPets = new Dictionary<int, Asda2PetRecord>();
        public Dictionary<int, Asda2MountRecord> OwnedMounts = new Dictionary<int, Asda2MountRecord>();
        private int _transportItemId = -1;
        private short _asda2FactionRank;
        private short _asda2WingsItemId = -1;
        private short _transformationId = -1;
        private int _mountId = -1;
    }

    public class AlreadyBuffedExcepton : Exception
    {
    }

    [ActiveRecord("Asda2PetRecord", Access = PropertyAccess.Property)]
    public class Asda2PetRecord : WCellRecord<Asda2PetRecord>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(Asda2PetRecord), "Guid");
        public PetTemplate Template { get; set; }

        public Character Owner { get; set; }
        [Property]
        public uint OwnerId { get; set; }
        private byte _hungerPrc;
        private byte _level;

        [Property]
        public short Id { get; set; }
        [Property]
        public string Name { get; set; }
        [PrimaryKey]
        public int Guid { get; set; }
        [Property]
        public short Expirience { get; set; }
        [Property]
        public byte HungerPrc
        {
            get { return _hungerPrc; }
            set { _hungerPrc = value; if (Owner == null)return; Asda2PetHandler.SendUpdatePetHungerResponse(Owner.Client, this); }
        }
        public Asda2PetStatType Stat1Type
        {
            get
            {
                return (Asda2PetStatType)Template.Bonus1Type;
            }
        }
        public int Stat1Value { get { return Template.Bonus1Type == 0 ? 0 : Asda2PetMgr.PetOptionValues[Template.Bonus1Type][Template.Rank][Template.Rarity][Level]; } }

        public Asda2PetStatType Stat2Type { get { return (Asda2PetStatType)Template.Bonus2Type; } }

        public int Stat2Value { get { return Template.Bonus2Type == 0 ? 0 : Asda2PetMgr.PetOptionValues[Template.Bonus2Type][Template.Rank][Template.Rarity][Level]; } }

        public Asda2PetStatType Stat3Type { get { return (Asda2PetStatType)Template.Bonus3Type; } }

        public int Stat3Value { get { return Template.Bonus3Type == 0 ? 0 : Asda2PetMgr.PetOptionValues[Template.Bonus3Type][Template.Rank][Template.Rarity][Level]; } }

        [Property]
        public byte Level
        {
            get { return _level; }
            set
            {
                _level = value;
            }
        }

        public void AddStatsToOwner()
        {
            if (Stat1Type != Asda2PetStatType.None) ApplyStat(Stat1Type, Stat1Value);
            if (Stat2Type != Asda2PetStatType.None) ApplyStat(Stat2Type, Stat2Value);
            if (Stat3Type != Asda2PetStatType.None) ApplyStat(Stat3Type, Stat3Value);

        }

        private void ApplyStat(Asda2PetStatType type, int value)
        {
            switch (type)
            {
                case Asda2PetStatType.MaxMagicAtack:
                    Owner.ApplyStatMod(ItemModType.MagicDamage, value);
                    break;
                case Asda2PetStatType.MinMaxMagicAtack:
                    Owner.ApplyStatMod(ItemModType.MagicDamage, value);
                    break;
                case Asda2PetStatType.MinMaxAtack:
                    Owner.ApplyStatMod(ItemModType.Damage, value);
                    break;
                case Asda2PetStatType.MinMagicAtack:
                    Owner.ApplyStatMod(ItemModType.MagicDamage, value);
                    break;
                case Asda2PetStatType.MagicDeffence:
                    Owner.ApplyStatMod(ItemModType.Asda2MagicDefence, (int)(value * CharacterFormulas.PetMagicDeffenceMultiplier * 3 / 2));
                    break;
                case Asda2PetStatType.MinMaxDeffence:
                    Owner.ApplyStatMod(ItemModType.Asda2Defence, (int)(value * CharacterFormulas.PetMagicDeffenceMultiplier));
                    break;
                case Asda2PetStatType.DodgePrc:
                    Owner.ApplyStatMod(ItemModType.DodgeRating, value);
                    break;
                case Asda2PetStatType.MinDeffence:
                    Owner.ApplyStatMod(ItemModType.Asda2Defence, (int)(value * CharacterFormulas.PetDeffenceMultiplier));
                    break;
                case Asda2PetStatType.MaxDeffence:
                    Owner.ApplyStatMod(ItemModType.Asda2Defence, (int)(value * CharacterFormulas.PetDeffenceMultiplier));
                    break;
                case Asda2PetStatType.MaxAtack:
                    Owner.ApplyStatMod(ItemModType.Damage, value);
                    break;
                case Asda2PetStatType.Stamina:
                    Owner.ApplyStatMod(ItemModType.Stamina, value);
                    break;
                case Asda2PetStatType.Intellect:
                    Owner.ApplyStatMod(ItemModType.Intellect, value);
                    break;
                case Asda2PetStatType.Strength:
                    Owner.ApplyStatMod(ItemModType.Strength, value);
                    break;
                case Asda2PetStatType.Energy:
                    Owner.ApplyStatMod(ItemModType.Spirit, value);
                    break;
                case Asda2PetStatType.AllCapabilities:
                    Owner.ApplyStatMod(ItemModType.Strength, value);
                    Owner.ApplyStatMod(ItemModType.Agility, value);
                    Owner.ApplyStatMod(ItemModType.Intellect, value);
                    Owner.ApplyStatMod(ItemModType.Stamina, value);
                    Owner.ApplyStatMod(ItemModType.Luck, value);
                    Owner.ApplyStatMod(ItemModType.Spirit, value);
                    break;
                case Asda2PetStatType.MinAtack:
                    Owner.ApplyStatMod(ItemModType.Damage, value);
                    break;
                case Asda2PetStatType.Dexterity:
                    Owner.ApplyStatMod(ItemModType.Agility, value);
                    break;
                case Asda2PetStatType.Luck:
                    Owner.ApplyStatMod(ItemModType.Luck, value);
                    break;
                case Asda2PetStatType.MinMagicAtackPrc:
                    Owner.ApplyStatMod(ItemModType.MagicDamagePrc, value);
                    break;
                case Asda2PetStatType.MaxMagicAtackPrc:
                    Owner.ApplyStatMod(ItemModType.MagicDamagePrc, value);
                    break;
                case Asda2PetStatType.MaxAtackPrc:
                    Owner.ApplyStatMod(ItemModType.DamagePrc, value);
                    break;
                case Asda2PetStatType.MinMaxAtackPrc:
                    Owner.ApplyStatMod(ItemModType.DamagePrc, value * 2);
                    break;
                case Asda2PetStatType.MinMaxMagicAtackPrc:
                    Owner.ApplyStatMod(ItemModType.MagicDamagePrc, value * 2);
                    break;
                case Asda2PetStatType.MaxDeffencePrc:
                    Owner.ApplyStatMod(ItemModType.Asda2DefencePrc, value );
                    break;
                case Asda2PetStatType.MinMaxDeffencePrc:
                    Owner.ApplyStatMod(ItemModType.Asda2DefencePrc, value);
                    break;
                case Asda2PetStatType.MagicDeffencePrc:
                    Owner.ApplyStatMod(ItemModType.Asda2MagicDefencePrc, value);
                    break;
                case Asda2PetStatType.MinDeffencePrc:
                    Owner.ApplyStatMod(ItemModType.Asda2DefencePrc, value);
                    break;
                case Asda2PetStatType.MinAtackPrc:
                    Owner.ApplyStatMod(ItemModType.DamagePrc, value);
                    break;
                case Asda2PetStatType.StrengthPrc:
                    Owner.ApplyStatMod(ItemModType.StrengthPrc, value);
                    break;
                case Asda2PetStatType.StaminaPrc:
                    Owner.ApplyStatMod(ItemModType.StaminaPrc, value);
                    break;
                case Asda2PetStatType.CriticalPrc:
                    Owner.ApplyStatMod(ItemModType.MeleeCriticalStrikeRating, value);
                    Owner.ApplyStatMod(ItemModType.CriticalStrikeRating, value);
                    Owner.ApplyStatMod(ItemModType.SpellCriticalStrikeRating, value);
                    break;
                case Asda2PetStatType.ItemSellingPricePrc:
                    Owner.ApplyStatMod(ItemModType.SellingCost, value);
                    break;
                case Asda2PetStatType.IntellectPrc:
                    Owner.ApplyStatMod(ItemModType.IntelectPrc, value);
                    break;
                case Asda2PetStatType.LuckPrc:
                    Owner.ApplyStatMod(ItemModType.LuckPrc, value);
                    break;
                case Asda2PetStatType.AllCapabilitiesPrc:
                    Owner.ApplyStatMod(ItemModType.StrengthPrc, value);
                    Owner.ApplyStatMod(ItemModType.AgilityPrc, value);
                    Owner.ApplyStatMod(ItemModType.IntelectPrc, value);
                    Owner.ApplyStatMod(ItemModType.StaminaPrc, value);
                    Owner.ApplyStatMod(ItemModType.LuckPrc, value);
                    Owner.ApplyStatMod(ItemModType.EnergyPrc, value);
                    break;
                case Asda2PetStatType.DexterityPrc:
                    Owner.ApplyStatMod(ItemModType.AgilityPrc, value);
                    break;
            }
        }

        public void RemoveStatsFromOwner()
        {
            if (Stat1Type != Asda2PetStatType.None) ApplyStat(Stat1Type, -Stat1Value);
            if (Stat2Type != Asda2PetStatType.None) ApplyStat(Stat2Type, -Stat2Value);
            if (Stat3Type != Asda2PetStatType.None) ApplyStat(Stat3Type, -Stat3Value);
        }

        [Property]
        public byte MaxLevel { get; set; }
        [Property]
        public bool CanChangeName { get; set; }

        public bool IsMaxExpirience
        {
            get
            {
                var xpForNextLvl = Asda2PetMgr.ExpTable[Template.Rank][Template.Rarity][Level - 1];
                return Expirience >= xpForNextLvl;
            }
        }

        public Asda2PetRecord(PetTemplate template, Character owner)
        {
            Guid = (int)_idGenerator.Next();

            Id = (short)template.Id;
            OwnerId = owner.EntityId.Low;
            Name = template.Name;
            Owner = owner;
            MaxLevel = (byte)template.MaxLevel;
            _level = 1;
            Template = template;
            CanChangeName = true;
            HungerPrc = 100;
        }

        public Asda2PetRecord()
        {

        }
        public void Init(Character owner)
        {
            Template = Asda2PetMgr.PetTemplates[Id];
            Owner = owner;
        }
        public static Asda2PetRecord[] LoadAll(Character owner)
        {
            var r = FindAllByProperty("OwnerId", owner.EntityId.Low);
            foreach (var asda2PetRecord in r)
            {
                asda2PetRecord.Init(owner);
            }
            return r;
        }

        public bool GainXp(int i)
        {
            if (Level == 10)
                return false;

            var xpForNextLvl = Asda2PetMgr.ExpTable[Template.Rank][Template.Rarity][Level - 1];
            if (Level == MaxLevel)
            {
                if (Expirience >= xpForNextLvl)
                {
                    Expirience = (short)xpForNextLvl;
                    return false;
                }
            }
            Expirience += (short)i;
            if (Level == MaxLevel)
            {
                if (Expirience > xpForNextLvl)
                    Expirience = (short)xpForNextLvl;
                Asda2PetHandler.SendUpdatePetExpResponse(Owner.Client, this);
                return true;
            }
            if (Expirience > xpForNextLvl)
            {
                RemoveStatsFromOwner();
                Level++;
                Asda2TitleChecker.OnPetLevelChanged(Level, Owner);
                AddStatsToOwner();
                Asda2CharacterHandler.SendUpdateStatsResponse(Owner.Client);
                Asda2CharacterHandler.SendUpdateStatsOneResponse(Owner.Client);
                GlobalHandler.UpdateCharacterPetInfoToArea(Owner);

                Asda2PetHandler.SendUpdatePetHungerResponse(Owner.Client, this);
            }
            Asda2PetHandler.SendUpdatePetExpResponse(Owner.Client, this, true);
            return true;
        }

        public void Feed(int i)
        {
            HungerPrc += (byte)i;
            if (HungerPrc > 100)
                HungerPrc = 100;
            Asda2PetHandler.SendUpdatePetHungerResponse(Owner.Client, this);
        }

        public void RemovePrcExp(int prc)
        {
            var curLevelExp = Asda2PetMgr.ExpTable[Template.Rank][Template.Rarity][Level - 2];
            var thisLvlExp = Expirience - curLevelExp;
            var removeExpCount = thisLvlExp - thisLvlExp * prc / 100;
            Expirience -= (short)removeExpCount;
        }
    }
}