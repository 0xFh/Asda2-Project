using Castle.ActiveRecord;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Core.Database;

namespace WCell.RealmServer.Items
{
    [Castle.ActiveRecord.ActiveRecord("PetitionRecord", Access = PropertyAccess.Property)]
    public class PetitionRecord : WCellRecord<PetitionRecord>
    {
        [Field("Type", NotNull = true)] private int m_Type;

        [PrimaryKey(PrimaryKeyType.Assigned, "OwnerId")]
        private int m_OwnerId { get; set; }

        public PetitionRecord()
        {
        }

        public PetitionRecord(string name, uint ownerId, uint itemId, PetitionType type)
        {
            this.Name = name;
            this.OwnerId = ownerId;
            this.ItemId = (int) itemId;
            this.SignedIds = new List<uint>(9);
            this.Type = type;
        }

        [Property("ItemId", NotNull = true)] private int ItemId { get; set; }

        [Property("Name", NotNull = true, Unique = true)]
        public string Name { get; set; }

        public uint OwnerId
        {
            get { return (uint) this.m_OwnerId; }
            set { this.m_OwnerId = (int) value; }
        }

        public PetitionType Type
        {
            get { return (PetitionType) this.m_Type; }
            set { this.m_Type = (int) value; }
        }

        [Property("SignedIds", NotNull = true)]
        public List<uint> SignedIds { get; set; }

        public void AddSignature(uint signedId)
        {
            this.SignedIds.Add(signedId);
            this.Update();
        }

        public static PetitionRecord LoadRecord(int ownerId)
        {
            return ActiveRecordBase<PetitionRecord>.Find((object) ownerId);
        }

        public static bool CanBuyPetition(uint ownerId)
        {
            return !ActiveRecordBase<PetitionRecord>.Exists<int>((int) ownerId);
        }

        public static PetitionRecord LoadRecordByItemId(uint itemId)
        {
            return ActiveRecordBase<PetitionRecord>.FindAllByProperty("ItemId", (object) (int) itemId)[0];
        }
    }
}