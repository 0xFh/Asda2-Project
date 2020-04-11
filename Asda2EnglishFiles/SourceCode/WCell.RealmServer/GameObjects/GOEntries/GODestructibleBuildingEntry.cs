using NLog;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GODestructibleBuildingEntry : GOEntry
    {
        private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>Intact: Num hits</summary>
        public int IntactNumHits
        {
            get { return this.Fields[0]; }
        }

        public int CreditProxyCreature
        {
            get { return this.Fields[1]; }
        }

        public int Field103_2
        {
            get { return this.Fields[2]; }
        }

        public int IntactEvent
        {
            get { return this.Fields[3]; }
        }

        public int Field103_4
        {
            get { return this.Fields[4]; }
        }

        /// <summary>Damaged: Num hits</summary>
        public int DamagedNumHits
        {
            get { return this.Fields[5]; }
        }

        public int Field103_6
        {
            get { return this.Fields[6]; }
        }

        public int Field103_7
        {
            get { return this.Fields[7]; }
        }

        public int Field103_8
        {
            get { return this.Fields[8]; }
        }

        public int DamagedEvent
        {
            get { return this.Fields[9]; }
        }

        public int Field103_10
        {
            get { return this.Fields[10]; }
        }

        public int Field103_11
        {
            get { return this.Fields[11]; }
        }

        public int Field103_12
        {
            get { return this.Fields[12]; }
        }

        public int Field103_13
        {
            get { return this.Fields[13]; }
        }

        public int DestroyedEvent
        {
            get { return this.Fields[14]; }
        }

        public int Field103_15
        {
            get { return this.Fields[15]; }
        }

        /// <summary>Rebuilding: Time (in seconds)</summary>
        public int RebuildingTime
        {
            get { return this.Fields[16]; }
        }

        public int Field103_16
        {
            get { return this.Fields[17]; }
        }

        public int DestructibleData
        {
            get { return this.Fields[18]; }
        }

        /// <summary>Rebuilding: Event</summary>
        public int RebuildingEvent
        {
            get { return this.Fields[19]; }
        }

        public int Field103_20
        {
            get { return this.Fields[20]; }
        }

        public int Field103_21
        {
            get { return this.Fields[21]; }
        }

        public int DamageEvent
        {
            get { return this.Fields[22]; }
        }

        public int Field103_23
        {
            get { return this.Fields[23]; }
        }
    }
}