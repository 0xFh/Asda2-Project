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
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class InstanceProgress : WCellRecord<InstanceProgress>, IMapId
    {
        [Field("InstanceId", Access = PropertyAccess.Field, NotNull = true)]
        private int _instanceId;

        public InstanceProgress()
        {
            this.MapId = MapId.None;
            this.InstanceId = 0U;
        }

        public InstanceProgress(MapId mapId, uint instanceId)
        {
            this.MapId = mapId;
            this.InstanceId = instanceId;
            this.State = RecordState.New;
        }

        [PrimaryKey(PrimaryKeyType.Assigned)]
        public long Guid
        {
            get { return Utility.MakeLong((int) this.MapId, this._instanceId); }
            set
            {
                int low = 0;
                Utility.UnpackLong(value, ref low, ref this._instanceId);
                this.MapId = (MapId) low;
            }
        }

        [Property(NotNull = true)] public MapId MapId { get; set; }

        public uint InstanceId
        {
            get { return (uint) this._instanceId; }
            set { this._instanceId = (int) value; }
        }

        [Property(NotNull = true)] public uint DifficultyIndex { get; set; }

        [Property] public DateTime ResetTime { get; set; }

        [Property] public int CustomDataVersion { get; set; }

        [Property] public byte[] CustomData { get; set; }
    }
}