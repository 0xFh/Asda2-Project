using System;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Database;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Entities
{
    public partial class Asda2Item
    {

        public int ItemId
        {
            get { return IsDeleted ? _itemId : m_record.ItemId; }
            set
            {
                if (m_record != null)
                    m_record.ItemId = value;
                _itemId = value;
            }
        }
        private int _itemId;
        public Character OwningCharacter
        {
            get { return m_owner; }
            internal set
            {
                if (m_owner == value)
                    return;
                m_owner = value;
                if (m_owner != null)
                {
                    m_isInWorld = m_unknown = true;
                    //SetEntityId(ItemFields.OWNER, value.EntityId);
                    m_record.OwnerId = value.EntityId.Low;
                    m_record.OwnerName = value.Name;
                }
                else
                {
                    //SetEntityId(ItemFields.OWNER, EntityId.Zero);
                    m_record.OwnerId = 0;
                    m_record.OwnerName = "No owner.";
                }
            }
        }

        public int CountForNextSell { get; set; }
        /*/// <summary>
        /// The Inventory of the Container that contains this Item
        /// </summary>
        public BaseInventory Container
        {
            get { return m_container; }
            internal set
            {
                if (m_container != value)
                {
                    if (value != null)
                    {
                        var cont = value.Container;
                        SetEntityId(ItemFields.CONTAINED, cont.EntityId);
                        m_record.ContainerSlot = cont.BaseInventory.Slot;
                    }
                    else
                    {
                        SetEntityId(ItemFields.CONTAINED, EntityId.Zero);
                        m_record.ContainerSlot = 0;
                    }
                    m_container = value;
                }
            }
        }*/

        /// <summary>
        /// The life-time of this Item in seconds
        /// </summary>
        //public uint ExistingDuration
        //{
        //    get
        //    {
        //        return m_record.ExistingDuration;
        //    }
        //    set
        //    {
        //        m_record.ExistingDuration = value;
        //    }
        //}

        public EntityId Creator
        {
            get { return new EntityId((ulong)m_record.CreatorEntityId); }
            set
            {
                //SetEntityId(ItemFields.CREATOR, value);
                m_record.CreatorEntityId = (long)value.Full;
            }
        }

        /*public EntityId GiftCreator
        {
            get { return new EntityId((ulong)m_record.GiftCreatorEntityId); }
            set
            {
                SetEntityId(ItemFields.GIFTCREATOR, value);
                m_record.GiftCreatorEntityId = (long)value.Full;
            }
        }*/

        /// <summary>
        /// The Slot of this Item within its <see cref="Container">Container</see>.
        /// </summary>
        public short Slot
        {
            get
            {
                return IsDeleted ? _slot : m_record.Slot;
            }
            internal set
            {
                m_record.Slot = value;
                _slot = value;
            }
        }

        private short _slot;
        private byte _inventoryType;

        /// <summary>
        /// Modifies the amount of this item (size of this stack).
        /// Ensures that new value won't exceed UniqueCount.
        /// Returns how many items actually got added. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int ModAmount(int value)
        {
            if (value != 0)
            {
                Amount += value;

                //SetInt32(ItemFields.STACK_COUNT, m_record.Amount);
                return value;
            }
            return 0;
        }

        /// <summary>
        /// Current amount of items in this stack.
        /// Setting the Amount to 0 will destroy the Item.
        /// Keep in mind that this is uint and thus can never become smaller than 0!
        /// </summary>
        public int Amount
        {
            get
            {
                var r = IsDeleted ? -1 : m_record.Amount;
                return r;
            }
            set
            {
                if (value <= 0)
                {
                    m_record.Amount = 0;
                    Destroy();
                }
                else
                {
                    var diff = value - m_record.Amount;
                    if (diff != 0)
                    {
                        m_record.Amount = value;
                    }
                }
            }
        }

        public uint Duration
        {
            get { return (uint)(IsDeleted ? 0 : m_record.Duration); }
            set
            {
                //SetUInt32(ItemFields.DURATION, value);
                m_record.Duration = (int)value;
            }
        }

        /// <summary>
        /// Charges of the <c>UseSpell</c> of this Item.
        /// </summary>
        /*public int SpellCharges
        {
            get
            {
                return (int)m_record.Charges;
            }
            set
            {
                if (value == 0 && m_record.Charges < 0)
                {
                    Destroy();
                    return;
                }
                m_record.Charges = (short)value;
                if (m_template.UseSpell != null)
                {
                    SetSpellCharges(m_template.UseSpell.Index, value);
                }
            }
        }*/

        /*public uint GetSpellCharges(uint index)
        {
            return GetUInt32(ItemFields.SPELL_CHARGES + (int)index);
        }

        public void ModSpellCharges(uint index, int delta)
        {
            SetUInt32((int)ItemFields.SPELL_CHARGES + (int)index, (uint)(GetSpellCharges(index) + delta));
        }

        public void SetSpellCharges(uint index, int value)
        {
            SetUInt32((int)ItemFields.SPELL_CHARGES + (int)index, (uint)Math.Abs(value));
        }
*/
        /*public ItemFlags Flags
        {
            get { return m_record.Flags; }
            set
            {
                SetUInt32(ItemFields.FLAGS, (uint)value);
                m_record.Flags = value;
            }
        }*/

        public bool IsAuctioned
        {
            get { return !IsDeleted && m_record.IsAuctioned; }
            set { m_record.IsAuctioned = true; }
        }

        public int AuctionPrice
        {
            get { return Record.AuctionPrice; }
            set { Record.AuctionPrice = value; }
        }

        #region ItemFlag Helpers

        public bool IsSoulbound
        {
            get { return IsDeleted ? false : m_record.IsSoulBound; }
            set { m_record.IsSoulBound = value; }
        }

        /*public bool IsGiftWrapped
        {
            get { return Flags.HasFlag(ItemFlags.GiftWrapped); }
        }

        public bool IsConjured
        {
            get { return Flags.HasFlag(ItemFlags.Conjured); }
        }*/

        #endregion

        /*public uint PropertySeed
		{
			get { return GetUInt32(ItemFields.PROPERTY_SEED); }
			set
			{
				SetUInt32(ItemFields.PROPERTY_SEED, value);
				m_record.RandomSuffix = (int)value;
			}
		}*/

        /*public uint RandomPropertiesId
        {
            get { return (uint)m_record.RandomProperty; }
            set
            {
                SetUInt32(ItemFields.RANDOM_PROPERTIES_ID, value);
                m_record.RandomProperty = (int)value;
            }
        }*/

        public byte Durability
        {
            get { return (byte)(IsDeleted ? 0 : m_record.Durability); }
            set
            {
                m_record.Durability = value;
                if (value == 0 && Template.IsEquipment)
                {
                    Asda2TitleChecker.OnItemBroken(OwningCharacter);
                }
            }
        }

        public byte MaxDurability
        {
            get
            {
                return (byte)(IsDeleted ? 0 : Template.MaxDurability);
                //return m_Template.MaxDurability;
            }
            protected set
            {
                Template.MaxDurability = value;
            }
        }

        public void RepairDurability()
        {
            Durability = MaxDurability;
        }

        #region IWeapon

        private DamageInfo[] _damages;

        public DamageInfo[] Damages
        {
            get { return _damages; }
            private set { _damages = value; }
        }

        public int BonusDamage
        {
            get;
            set;
        }

        public bool IsRanged
        {
            get { return !IsDeleted && m_template.IsRangedWeapon; }
        }

        public bool IsMelee
        {
            get { return !IsDeleted && m_template.IsMeleeWeapon; }
        }

        /// <summary>
        /// The minimum Range of this weapon
        /// TODO: temporary values
        /// </summary>
        public float MinRange
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// The maximum Range of this Weapon
        /// TODO: temporary values
        /// </summary>
        public float MaxRange
        {
            get
            {
                return IsDeleted ? 0 : m_template.AtackRange;
            }
        }

        /// <summary>
        /// The time in milliseconds between 2 attacks
        /// </summary>
        public int AttackTime
        {
            get
            {
                return IsDeleted ? 0 : m_template.AttackTime;
            }
        }
        #endregion

        public Asda2ItemRecord Record
        {
            get
            {
                return m_record;
            }
        }

        public override ObjectTypeCustom CustomType
        {
            get
            {
                return ObjectTypeCustom.Object | ObjectTypeCustom.Item;
            }
        }

        public Asda2InventoryType InventoryType
        {
            get { return IsDeleted ? (Asda2InventoryType)_inventoryType : (Asda2InventoryType)m_record.InventoryType; }
            set
            {
                m_record.InventoryType = (byte)value;
                _inventoryType = m_record.InventoryType;
            }
        }

        public int Soul1Id
        {
            get { return IsDeleted ? 0 : m_record.Soul1Id; }
            set
            {

                m_record.Soul1Id = value;
            }
        }

        public int Soul2Id
        {
            get { return IsDeleted ? 0 : m_record.Soul2Id; }
            set { m_record.Soul2Id = value; }
        }

        public int Soul3Id
        {
            get { return IsDeleted ? 0 : m_record.Soul3Id; }
            set { m_record.Soul3Id = value; }
        }
        public int Soul4Id
        {
            get { return IsDeleted ? 0 : m_record.Soul4Id; }
            set { m_record.Soul4Id = value; }
        }
        bool IsValidSowel(int id)
        {
            return IsValidSowel(Asda2ItemMgr.GetTemplate(id));
        }
        bool IsValidSowel(Asda2ItemTemplate sowel)
        {
            if (sowel == null || sowel.Category != Asda2ItemCategory.Sowel)
                return false;
            if (!IsValidSowelEquipSlot(sowel))
                return false;
            if (sowel.RequiredLevel > Owner.Level)
                return false;
            return true;
        }
        public bool InsertSowel(Asda2Item sowel, byte slot)
        {
            if (!IsValidSowel(sowel.Template) || slot > SocketsCount - 1)
                return false;
            switch (slot)
            {
                case 0:
                    Soul1Id = sowel.ItemId;
                    break;
                case 1:
                    Soul2Id = sowel.ItemId;
                    break;
                case 2:
                    Soul3Id = sowel.ItemId;
                    break;
                case 3:
                    Soul4Id = sowel.ItemId;
                    break;
            }
            RecalculateItemParametrs();
            return true;
        }

        private bool IsValidSowelEquipSlot(Asda2ItemTemplate sowel)
        {
            switch (sowel.SowelEquipmentType)
            {
                default:
                    if ((int)Template.EquipmentSlot != (int)sowel.SowelEquipmentType)
                        return false;
                    break;
            }
            //Todo sowel equipent slot logic
            return true;
        }

        public byte Enchant
        {
            get { return (byte)(IsDeleted ? 0 : m_record.Enchant); }
            set
            {
                if (value == Enchant)
                    return;
                m_record.Enchant = value;
                if (Enchant >= CharacterFormulas.OptionStatStartsWithEnchantValue)
                {
                    GenerateOptionsByUpgrade();
                }
                RecalculateItemParametrs();
            }
        }

        public Asda2ItemBonusType Parametr1Type
        {
            get { return (Asda2ItemBonusType)(IsDeleted ? 0 : m_record.Parametr1Type); }
            set { m_record.Parametr1Type = (short)value; }
        }

        public short Parametr1Value
        {
            get { return (short)(IsDeleted ? 0 : m_record.Parametr1Value); }
            set { m_record.Parametr1Value = value; }
        }

        public Asda2ItemBonusType Parametr2Type
        {
            get { return (Asda2ItemBonusType)(IsDeleted ? 0 : m_record.Parametr2Type); }
            set { m_record.Parametr2Type = (short)value; }
        }

        public short Parametr2Value
        {
            get { return (short)(IsDeleted ? 0 : m_record.Parametr2Value); }
            set { m_record.Parametr2Value = value; }
        }
        public Asda2ItemBonusType Parametr3Type
        {
            get { return (Asda2ItemBonusType)(IsDeleted ? 0 : m_record.Parametr3Type); }
            set { m_record.Parametr3Type = (short)value; }
        }

        public short Parametr3Value
        {
            get { return (short)(IsDeleted ? 0 : m_record.Parametr1Value); }
            set { m_record.Parametr1Value = value; }
        }
        public Asda2ItemBonusType Parametr4Type
        {
            get { return (Asda2ItemBonusType)(IsDeleted ? 0 : m_record.Parametr4Type); }
            set { m_record.Parametr4Type = (short)value; }
        }

        public short Parametr4Value
        {
            get { return (short)(IsDeleted ? 0 : m_record.Parametr4Value); }
            set { m_record.Parametr4Value = value; }
        }
        public Asda2ItemBonusType Parametr5Type
        {
            get { return (Asda2ItemBonusType)(IsDeleted ? 0 : m_record.Parametr5Type); }
            set { m_record.Parametr5Type = (short)value; }
        }

        public short Parametr5Value
        {
            get { return (short)(IsDeleted ? 0 : m_record.Parametr5Value); }
            set { m_record.Parametr5Value = value; }
        }

        public ushort Weight
        {
            get { return (ushort)(IsDeleted ? 0 : m_record.Weight); }
            set { m_record.Weight = value; }
        }

        public byte SealCount
        {
            get { return (byte)(IsDeleted ? 0 : m_record.SealCount); }
            set { m_record.SealCount = value; }
        }

        public Asda2ItemCategory Category
        {
            get { return IsDeleted ? 0 : Template.Category; }
        }

        public byte SowelSlots
        {
            get { return (byte)(IsDeleted ? 0 : Template.SowelSocketsCount); }
        }

        public int AuctionId
        {
            get { return (int)Record.Guid; }
        }

        public uint RepairCost()
        {
            return CharacterFormulas.CalculteItemRepairCost(MaxDurability, Durability, Template.SellPrice, Enchant, (byte)Template.AuctionLevelCriterion, (byte)Template.Quality);
        }
    }
}