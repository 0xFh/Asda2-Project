using Castle.ActiveRecord;
using System.Collections.Generic;
using WCell.Core;

namespace WCell.RealmServer.Database
{
    /// <summary>
    /// This feature allows players to store sets of equipment, easily swap between saved sets using hotkeys,
    /// and pull items directly from backpacks or bank slots (must be at the bank to equip inventory from the bank).
    /// </summary>
    [Castle.ActiveRecord.ActiveRecord("EquipmentSets", Access = PropertyAccess.Property)]
    public class EquipmentSet : ActiveRecordBase<EquipmentSet>
    {
        public static readonly IList<EquipmentSet> EmptyList = (IList<EquipmentSet>) new List<EquipmentSet>(1);

        private static readonly NHIdGenerator m_idGenerator =
            new NHIdGenerator(typeof(EquipmentSet), "EquipmentSets", "EntityLowId", 1L);

        [Field] public int Id;
        [Field] public string Name;
        [Field] public string Icon;

        [PrimaryKey(PrimaryKeyType.Assigned, "EntityLowId")]
        private long lowId { get; set; }

        public EntityId SetGuid
        {
            get { return EntityId.GetPlayerId((uint) this.lowId); }
        }

        public IList<EquipmentSetItemMapping> Items { get; set; }

        /// <summary>Returns the next unique Id for a new Set</summary>
        public static long NextId()
        {
            return EquipmentSet.m_idGenerator.Next();
        }

        public static EquipmentSet CreateSet()
        {
            return new EquipmentSet()
            {
                lowId = EquipmentSet.NextId()
            };
        }

        public void Fill(int setId, string name, string icon, EquipmentSetItemMapping[] setItemMappings)
        {
            this.Id = setId;
            this.Name = name;
            this.Icon = icon;
            this.Items = (IList<EquipmentSetItemMapping>) setItemMappings;
        }

        private EquipmentSet()
        {
        }
    }
}