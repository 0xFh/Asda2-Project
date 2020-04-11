using NLog;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOCapturePointEntry : GOEntry
    {
        private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>The activation radius (?)</summary>
        public int Radius
        {
            get { return this.Fields[0]; }
        }

        /// <summary>
        /// Unknown, possibly a server-side dummy spell-effect. Not a SpellId from Spells.dbc
        /// </summary>
        public int SpellId
        {
            get { return this.Fields[1]; }
        }

        /// <summary>???</summary>
        public int WorldState1
        {
            get { return this.Fields[2]; }
        }

        /// <summary>???</summary>
        public int WorldState2
        {
            get { return this.Fields[3]; }
        }

        /// <summary>???</summary>
        public int WinEventId1
        {
            get { return this.Fields[4]; }
        }

        /// <summary>???</summary>
        public int WinEventId2
        {
            get { return this.Fields[5]; }
        }

        /// <summary>???</summary>
        public int ContestedEventId1
        {
            get { return this.Fields[6]; }
        }

        /// <summary>???</summary>
        public int ContestedEventId2
        {
            get { return this.Fields[7]; }
        }

        /// <summary>???</summary>
        public int ProgressEventId1
        {
            get { return this.Fields[8]; }
        }

        /// <summary>???</summary>
        public int ProgressEventId2
        {
            get { return this.Fields[9]; }
        }

        /// <summary>???</summary>
        public int NeutralEventId1
        {
            get { return this.Fields[10]; }
        }

        /// <summary>???</summary>
        public int NeutralEventId2
        {
            get { return this.Fields[11]; }
        }

        /// <summary>???</summary>
        public int NeutralPercent
        {
            get { return this.Fields[12]; }
        }

        /// <summary>???</summary>
        public int WorldState3
        {
            get { return this.Fields[13]; }
        }

        /// <summary>???</summary>
        public int MinSuperiority
        {
            get { return this.Fields[14]; }
        }

        /// <summary>???</summary>
        public int MaxSuperiority
        {
            get { return this.Fields[15]; }
        }

        /// <summary>???</summary>
        public int MinTime
        {
            get { return this.Fields[16]; }
        }

        /// <summary>???</summary>
        public int MaxTime
        {
            get { return this.Fields[17]; }
        }

        /// <summary>???</summary>
        public bool Large
        {
            get { return this.Fields[18] != 0; }
        }

        /// <summary>??? Is this a bool?</summary>
        public bool Highlight
        {
            get { return this.Fields[19] != 0; }
        }

        public int StartingValue
        {
            get { return this.Fields[20]; }
        }

        public bool Unidirectional
        {
            get { return this.Fields[21] != 0; }
        }
    }
}