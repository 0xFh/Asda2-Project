using Castle.ActiveRecord;
using System;
using WCell.Constants.World;
using WCell.Core.Database;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Instances
{
  [ActiveRecord(Access = PropertyAccess.Property)]
  public class InstanceBinding : WCellRecord<InstanceBinding>, IMapId
  {
    private int _difficultyIndex;
    private int _instanceId;

    public InstanceBinding()
    {
      BindTime = DateTime.Now;
      DifficultyIndex = 0U;
      InstanceId = 0U;
      MapId = MapId.None;
    }

    public InstanceBinding(uint id, MapId mapId, uint difficultyIndex)
    {
      BindTime = DateTime.Now;
      DifficultyIndex = difficultyIndex;
      InstanceId = id;
      MapId = mapId;
      State = RecordState.New;
    }

    public uint InstanceId
    {
      get { return (uint) _instanceId; }
      set { _instanceId = (int) value; }
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

    public MapId MapId { get; set; }

    [Property(NotNull = true)]
    public uint DifficultyIndex
    {
      get { return (uint) _difficultyIndex; }
      set { _difficultyIndex = (int) value; }
    }

    [Property(NotNull = true)]
    public DateTime BindTime { get; set; }

    public DateTime NextResetTime
    {
      get { return InstanceMgr.GetNextResetTime(Difficulty); }
    }

    public MapTemplate MapTemplate
    {
      get { return World.GetMapTemplate(MapId); }
    }

    public MapDifficultyEntry Difficulty
    {
      get { return MapTemplate.Difficulties.Get(DifficultyIndex); }
    }

    /// <summary>Might Return null</summary>
    public InstancedMap Instance
    {
      get { return InstanceMgr.Instances.GetInstance(MapId, InstanceId); }
    }

    public static bool operator ==(InstanceBinding left, InstanceBinding right)
    {
      if(ReferenceEquals(left, null))
        return ReferenceEquals(right, null);
      if(!ReferenceEquals(right, null) &&
         left._difficultyIndex == right._difficultyIndex && left.MapId == right.MapId)
        return (int) left.InstanceId == (int) right.InstanceId;
      return false;
    }

    public static bool operator !=(InstanceBinding left, InstanceBinding right)
    {
      return !(left == right);
    }

    public bool Equals(InstanceBinding obj)
    {
      if(ReferenceEquals(null, obj))
        return false;
      if(ReferenceEquals(this, obj))
        return true;
      if(obj._difficultyIndex == _difficultyIndex &&
         (obj.BindTime.Equals(BindTime) && Equals(obj.MapId, MapId)))
        return (int) obj.InstanceId == (int) InstanceId;
      return false;
    }

    public override bool Equals(object obj)
    {
      if(ReferenceEquals(null, obj))
        return false;
      if(ReferenceEquals(this, obj))
        return true;
      if((object) (obj as InstanceBinding) == null)
        return false;
      return Equals((InstanceBinding) obj);
    }

    public override int GetHashCode()
    {
      return ((_difficultyIndex * 397 ^ BindTime.GetHashCode()) * 397 ^ MapId.GetHashCode()) *
             397 ^ InstanceId.GetHashCode();
    }
  }
}