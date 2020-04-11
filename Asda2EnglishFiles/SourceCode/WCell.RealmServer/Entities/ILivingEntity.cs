using WCell.Constants;
using WCell.Util;

namespace WCell.RealmServer.Entities
{
    /// <summary>Defines a living in-game entity.</summary>
    public interface ILivingEntity : INamedEntity, IEntity, INamed
    {
        GenderType Gender { get; }

        RaceId Race { get; }

        ClassId Class { get; }
    }
}