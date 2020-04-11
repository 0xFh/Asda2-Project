using WCell.Constants.World;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spawns
{
    public interface ISpawnPoint
    {
        Map Map { get; }

        MapId MapId { get; }

        Vector3 Position { get; }

        uint Phase { get; }

        bool HasSpawned { get; }

        /// <summary>
        /// Whether timer is running and will spawn a new NPC when the timer elapses
        /// </summary>
        bool IsSpawning { get; }

        /// <summary>Inactive and autospawns</summary>
        bool IsReadyToSpawn { get; }

        /// <summary>Whether NPC is alread spawned or timer is running</summary>
        bool IsActive { get; }

        void Respawn();

        void SpawnNow();

        void SpawnLater();

        /// <summary>Restarts the spawn timer with the given delay</summary>
        void SpawnAfter(int delay);

        /// <summary>Stops the Respawn timer, if it was running</summary>
        void StopTimer();

        void RemoveSpawnedObject();

        /// <summary>Stops timer and deletes spawnling</summary>
        void Disable();
    }
}