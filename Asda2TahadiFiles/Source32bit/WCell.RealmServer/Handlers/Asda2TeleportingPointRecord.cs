using Castle.ActiveRecord;
using NHibernate.Criterion;
using System;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Handlers
{
  [ActiveRecord(Access = PropertyAccess.Property, Table = "Asda2TeleportingPointRecord")]
  public class Asda2TeleportingPointRecord : WCellRecord<Asda2TeleportingPointRecord>
  {
    private static readonly NHIdGenerator IDGenerator =
      new NHIdGenerator(typeof(Asda2TeleportingPointRecord), nameof(Guid), 1L);

    /// <summary>Returns the next unique Id for a new Item</summary>
    public static long NextId()
    {
      return IDGenerator.Next();
    }

    [Property(NotNull = true)]
    public string Name { get; set; }

    internal static Asda2TeleportingPointRecord CreateRecord()
    {
      try
      {
        Asda2TeleportingPointRecord teleportingPointRecord = new Asda2TeleportingPointRecord();
        teleportingPointRecord.Guid = (uint) IDGenerator.Next();
        teleportingPointRecord.State = RecordState.New;
        return teleportingPointRecord;
      }
      catch(Exception ex)
      {
        throw new WCellException(ex, "Unable to create new Asda2FastItemSlotRecord.");
      }
    }

    [Property(NotNull = true)]
    public uint OwnerId { get; set; }

    [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
    public long Guid { get; set; }

    [Property(NotNull = true)]
    public short X { get; set; }

    [Property(NotNull = true)]
    public short Y { get; set; }

    [Property(NotNull = true)]
    public MapId MapId { get; set; }

    public static Asda2TeleportingPointRecord[] LoadItems(uint lowCharId)
    {
      return FindAll((ICriterion) Restrictions.Eq("OwnerId", lowCharId));
    }

    public static Asda2TeleportingPointRecord GetRecordByID(long id)
    {
      return FindOne((ICriterion) Restrictions.Eq("Guid", id));
    }

    public static Asda2TeleportingPointRecord CreateRecord(uint ownerAccId, short x, short y, MapId mapId)
    {
      Asda2TeleportingPointRecord record = CreateRecord();
      record.OwnerId = ownerAccId;
      record.X = x;
      record.Y = y;
      return record;
    }
  }
}