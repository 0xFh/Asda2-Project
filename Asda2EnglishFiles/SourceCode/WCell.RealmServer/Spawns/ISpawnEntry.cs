using WCell.Core.Paths;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spawns
{
    public interface ISpawnEntry : IWorldLocation, IHasPosition
    {
        float Orientation { get; set; }

        int RespawnSeconds { get; set; }

        int DespawnSeconds { get; set; }

        /// <summary>
        /// Min Delay in milliseconds until the unit should be respawned
        /// </summary>
        int RespawnSecondsMin { get; set; }

        /// <summary>
        /// Max Delay in milliseconds until the unit should be respawned
        /// </summary>
        int RespawnSecondsMax { get; set; }

        uint EventId { get; set; }

        /// <summary>
        /// Whether this Entry spawns automatically (or is spawned by certain events)
        /// </summary>
        bool AutoSpawns { get; set; }

        int GetRandomRespawnMillis();
    }
}