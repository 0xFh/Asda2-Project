using WCell.Core.Paths;

namespace WCell.RealmServer.Entities
{
    public interface INamedWorldZoneLocation : IWorldZoneLocation, IWorldLocation, IHasPosition
    {
        string[] Names { get; set; }

        string DefaultName { get; }
    }
}