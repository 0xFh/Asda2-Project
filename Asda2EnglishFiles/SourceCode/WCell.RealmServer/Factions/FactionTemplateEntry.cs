using System;
using WCell.Constants.Factions;

namespace WCell.RealmServer.Factions
{
    [Serializable]
    public struct FactionTemplateEntry
    {
        public uint Id;
        public FactionId FactionId;
        public FactionTemplateFlags Flags;

        /// <summary>The Faction-Group mask of this faction.</summary>
        public FactionGroupMask FactionGroup;

        /// <summary>
        /// Mask of Faction-Groups this faction is friendly towards
        /// </summary>
        public FactionGroupMask FriendGroup;

        /// <summary>
        /// Mask of Faction-Groups this faction is hostile towards
        /// </summary>
        public FactionGroupMask EnemyGroup;

        public FactionId[] EnemyFactions;
        public FactionId[] FriendlyFactions;
    }
}