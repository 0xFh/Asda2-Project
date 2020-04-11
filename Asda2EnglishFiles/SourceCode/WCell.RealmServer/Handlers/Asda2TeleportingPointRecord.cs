using Castle.ActiveRecord;
using NHibernate.Criterion;
using System;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Handlers
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property, Table = "Asda2TeleportingPointRecord")]
    public class Asda2TeleportingPointRecord : WCellRecord<Asda2TeleportingPointRecord>
    {
        private static readonly NHIdGenerator IDGenerator =
            new NHIdGenerator(typeof(Asda2TeleportingPointRecord), nameof(Guid), 1L);

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return Asda2TeleportingPointRecord.IDGenerator.Next();
        }

        [Property(NotNull = true)] public string Name { get; set; }

        internal static Asda2TeleportingPointRecord CreateRecord()
        {
            try
            {
                Asda2TeleportingPointRecord teleportingPointRecord = new Asda2TeleportingPointRecord();
                teleportingPointRecord.Guid = (long) (uint) Asda2TeleportingPointRecord.IDGenerator.Next();
                teleportingPointRecord.State = RecordState.New;
                return teleportingPointRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new Asda2FastItemSlotRecord.", new object[0]);
            }
        }

        [Property(NotNull = true)] public uint OwnerId { get; set; }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        [Property(NotNull = true)] public short X { get; set; }

        [Property(NotNull = true)] public short Y { get; set; }

        [Property(NotNull = true)] public MapId MapId { get; set; }

        public static Asda2TeleportingPointRecord[] LoadItems(uint lowCharId)
        {
            return ActiveRecordBase<Asda2TeleportingPointRecord>.FindAll(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("OwnerId", (object) lowCharId)
            });
        }

        public static Asda2TeleportingPointRecord GetRecordByID(long id)
        {
            return ActiveRecordBase<Asda2TeleportingPointRecord>.FindOne(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("Guid", (object) id)
            });
        }

        public static Asda2TeleportingPointRecord CreateRecord(uint ownerAccId, short x, short y, MapId mapId)
        {
            Asda2TeleportingPointRecord record = Asda2TeleportingPointRecord.CreateRecord();
            record.OwnerId = ownerAccId;
            record.X = x;
            record.Y = y;
            return record;
        }
    }
}