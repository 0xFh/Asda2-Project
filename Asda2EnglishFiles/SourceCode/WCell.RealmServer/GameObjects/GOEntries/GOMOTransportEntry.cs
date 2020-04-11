using NLog;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Taxi;
using WCell.RealmServer.Transports;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOMOTransportEntry : GOEntry
    {
        private static readonly Logger sLog = LogManager.GetCurrentClassLogger();
        protected TaxiPath m_path;

        /// <summary>The TaxiPathId from TaxiPaths.dbc</summary>
        public int TaxiPathId
        {
            get { return this.Fields[0]; }
        }

        /// <summary>The speed this object moves at.</summary>
        public int MoveSpeed
        {
            get { return this.Fields[1]; }
        }

        /// <summary>The rate this object accelerates at.</summary>
        public int AccelRate
        {
            get { return this.Fields[2]; }
        }

        /// <summary>
        /// The Id of an Event to call when this object is activated (?)
        /// </summary>
        public int StartEventId
        {
            get { return this.Fields[3]; }
        }

        /// <summary>
        /// The Id of an Event to call when this object is deactivated (?)
        /// </summary>
        public int StopEventId
        {
            get { return this.Fields[4]; }
        }

        /// <summary>Ref to TransportPhysics.dbc</summary>
        public int TransportPhysics
        {
            get { return this.Fields[5]; }
        }

        /// <summary>The Id of a Map this object is associated with (?)</summary>
        public int MapId
        {
            get { return this.Fields[6]; }
        }

        public int WorldState1
        {
            get { return this.Fields[7]; }
        }

        public TaxiPath Path
        {
            get { return this.m_path; }
            internal set { this.m_path = value; }
        }

        public override void FinalizeDataHolder()
        {
            this.m_path = TaxiMgr.PathsById.Get<TaxiPath>((uint) this.TaxiPathId);
            TransportEntry transportEntry;
            TransportMgr.TransportEntries.TryGetValue(this.GOId, out transportEntry);
            if (this.m_path == null)
            {
                ContentMgr.OnInvalidDBData(
                    "GOEntry for MOTransport \"{0}\" has invalid Path-id (Field 0): " + (object) this.TaxiPathId,
                    (object) this);
            }
            else
            {
                TransportMovement transportMovement =
                    new TransportMovement(this, transportEntry == null ? 0U : transportEntry.Period);
                base.FinalizeDataHolder();
            }
        }

        public override bool IsTransport
        {
            get { return true; }
        }

        protected internal override void InitGO(GameObject go)
        {
            base.InitGO(go);
        }
    }
}