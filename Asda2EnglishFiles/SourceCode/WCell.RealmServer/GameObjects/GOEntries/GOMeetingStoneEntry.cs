using NLog;
using WCell.Constants.World;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOMeetingStoneEntry : GOEntry
    {
        private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The minimum level a character must be to activate this object.
        /// </summary>
        public int MinLevel
        {
            get { return this.Fields[0]; }
        }

        /// <summary>
        /// The maximum level a character can be to activate this object.
        /// </summary>
        public int MaxLevel
        {
            get { return this.Fields[1]; }
        }

        /// <summary>The Id of the Zone to teleport to</summary>
        public ZoneId ZoneId
        {
            get { return (ZoneId) this.Fields[2]; }
        }
    }
}