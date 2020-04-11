using Castle.ActiveRecord;
using System;
using WCell.Constants.World;
using WCell.Core.Database;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Instances
{
  /// <summary>
  /// Defines the progress inside an instance that can also be saved to DB
  /// </summary>
  [ActiveRecord(Access = PropertyAccess.Property)]
  public class InstanceProgress : WCellRecord<InstanceProgress>, IMapId
  {
    [Field("InstanceId", Access = PropertyAccess.Field, NotNull = true)]
    private int _instanceId;

    public InstanceProgress()
    {
      MapId = MapId.None;
      InstanceId = 0U;
    }

    public InstanceProgress(MapId mapId, uint instanceId)
    {
      MapId = mapId;
      InstanceId = instanceId;
      State = RecordState.New;
    }

    [PrimaryKey(PrimaryKeyType.Assigned)]
    public long Guid
    {
      get { return Utility.MakeLong((int) MapId, _instanceId); }
      set
      {
        int low = 0;
        Utility.UnpackLong(value, ref low, ref _instanceId);
        MapId = (MapId) low;
      }
    }

    [Property(NotNull = true)]
    public MapId MapId { get; set; }

    public uint InstanceId
    {
      get { return (uint) _instanceId; }
      set { _instanceId = (int) value; }
    }

    [Property(NotNull = true)]
    public uint DifficultyIndex { get; set; }

    [Property]
    public DateTime ResetTime { get; set; }

    [Property]
    public int CustomDataVersion { get; set; }

    [Property]
    public byte[] CustomData { get; set; }
  }
}