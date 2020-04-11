using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Mounts
{
    [Castle.ActiveRecord.ActiveRecord("Asda2MountRecord", Access = PropertyAccess.Property)]
    public class Asda2MountRecord : WCellRecord<Asda2MountRecord>
    {
        private static readonly NHIdGenerator IdGenerator =
            new NHIdGenerator(typeof(Asda2MountRecord), nameof(Guid), 1L);

        [PrimaryKey] public int Guid { get; set; }

        public Asda2MountRecord(MountTemplate mount, Character owner)
        {
            this.Guid = (int) Asda2MountRecord.IdGenerator.Next();
            this.Id = mount.Id;
            this.OwnerId = owner.EntityId.Low;
        }

        [Property] protected uint OwnerId { get; set; }

        [Property] public int Id { get; set; }

        public Asda2MountRecord()
        {
        }

        public static Asda2MountRecord[] GetAllRecordsOfCharacter(uint charId)
        {
            return ActiveRecordBase<Asda2MountRecord>.FindAllByProperty("OwnerId", (object) charId);
        }
    }
}