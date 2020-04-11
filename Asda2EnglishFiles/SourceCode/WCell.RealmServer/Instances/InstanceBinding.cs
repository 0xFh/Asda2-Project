using Castle.ActiveRecord;
using System;
using WCell.Constants.World;
using WCell.Core.Database;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Instances
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class InstanceBinding : WCellRecord<InstanceBinding>, IMapId
    {
        private int _difficultyIndex;
        private int _instanceId;

        public InstanceBinding()
        {
            this.BindTime = DateTime.Now;
            this.DifficultyIndex = 0U;
            this.InstanceId = 0U;
            this.MapId = MapId.None;
        }

        public InstanceBinding(uint id, MapId mapId, uint difficultyIndex)
        {
            this.BindTime = DateTime.Now;
            this.DifficultyIndex = difficultyIndex;
            this.InstanceId = id;
            this.MapId = mapId;
            this.State = RecordState.New;
        }

        public uint InstanceId
        {
            get { return (uint) this._instanceId; }
            set { this._instanceId = (int) value; }
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

        public MapId MapId { get; set; }

        [Property(NotNull = true)]
        public uint DifficultyIndex
        {
            get { return (uint) this._difficultyIndex; }
            set { this._difficultyIndex = (int) value; }
        }

        [Property(NotNull = true)] public DateTime BindTime { get; set; }

        public DateTime NextResetTime
        {
            get { return InstanceMgr.GetNextResetTime(this.Difficulty); }
        }

        public MapTemplate MapTemplate
        {
            get { return WCell.RealmServer.Global.World.GetMapTemplate(this.MapId); }
        }

        public MapDifficultyEntry Difficulty
        {
            get { return this.MapTemplate.Difficulties.Get<MapDifficultyEntry>(this.DifficultyIndex); }
        }

        /// <summary>Might Return null</summary>
        public InstancedMap Instance
        {
            get { return (InstancedMap) InstanceMgr.Instances.GetInstance(this.MapId, this.InstanceId); }
        }

        public static bool operator ==(InstanceBinding left, InstanceBinding right)
        {
            if (object.ReferenceEquals((object) left, (object) null))
                return object.ReferenceEquals((object) right, (object) null);
            if (!object.ReferenceEquals((object) right, (object) null) &&
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
            if (object.ReferenceEquals((object) null, (object) obj))
                return false;
            if (object.ReferenceEquals((object) this, (object) obj))
                return true;
            if (obj._difficultyIndex == this._difficultyIndex &&
                (obj.BindTime.Equals(this.BindTime) && object.Equals((object) obj.MapId, (object) this.MapId)))
                return (int) obj.InstanceId == (int) this.InstanceId;
            return false;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals((object) null, obj))
                return false;
            if (object.ReferenceEquals((object) this, obj))
                return true;
            if ((object) (obj as InstanceBinding) == null)
                return false;
            return this.Equals((InstanceBinding) obj);
        }

        public override int GetHashCode()
        {
            return ((this._difficultyIndex * 397 ^ this.BindTime.GetHashCode()) * 397 ^ this.MapId.GetHashCode()) *
                   397 ^ this.InstanceId.GetHashCode();
        }
    }
}