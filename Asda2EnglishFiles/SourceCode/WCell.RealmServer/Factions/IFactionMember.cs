using WCell.Constants.Factions;

namespace WCell.RealmServer.Factions
{
    /// <summary>
    /// An interface for any WorldObject that can have a Faction
    /// </summary>
    public interface IFactionMember
    {
        Faction Faction { get; set; }

        FactionId FactionId { get; set; }

        bool IsInWorld { get; }

        /// <summary>
        /// Indicates whether the 2 objects have a good relationship
        /// </summary>
        bool IsFriendlyWith(IFactionMember opponent);

        /// <summary>
        /// Indicates whether this Object is in party or otherwise allied with the given obj
        /// </summary>
        bool IsAlliedWith(IFactionMember obj);
    }
}