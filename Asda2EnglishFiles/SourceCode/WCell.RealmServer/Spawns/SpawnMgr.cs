using System.Collections.Generic;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.Spawns
{
    public static class SpawnMgr
    {
        /// <summary>All entries for SpawnPoolTemplates</summary>
        public static readonly Dictionary<uint, SpawnPoolTemplateEntry> SpawnPoolTemplateEntries =
            new Dictionary<uint, SpawnPoolTemplateEntry>();

        public static SpawnPoolTemplateEntry GetSpawnPoolTemplateEntry(uint poolId)
        {
            SpawnPoolTemplateEntry poolTemplateEntry;
            SpawnMgr.SpawnPoolTemplateEntries.TryGetValue(poolId, out poolTemplateEntry);
            return poolTemplateEntry;
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Third)]
        public static void InitializeSpawnMgr()
        {
            ContentMgr.Load<SpawnPoolTemplateEntry>();
        }
    }
}