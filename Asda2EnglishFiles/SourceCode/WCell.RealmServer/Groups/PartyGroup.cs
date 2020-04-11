using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Groups
{
    /// <summary>Represents a party group.</summary>
    public sealed class PartyGroup : Group, IGroupConverter<RaidGroup>
    {
        /// <summary>Max Amount of allowed sub-groups</summary>
        public const byte MaxSubGroupCount = 1;

        /// <summary>Max amount of allowed members in a Party</summary>
        public const int MaxMemberCount = 5;

        /// <summary>
        /// Creates a party group with the given character as the leader.
        /// </summary>
        /// <param name="leader"></param>
        public PartyGroup(Character leader)
            : base(leader, (byte) 1)
        {
        }

        /// <summary>Tye type of group.</summary>
        public override GroupFlags Flags
        {
            get { return GroupFlags.Party; }
        }

        public RaidGroup ConvertTo()
        {
            return new RaidGroup((Group) this);
        }
    }
}