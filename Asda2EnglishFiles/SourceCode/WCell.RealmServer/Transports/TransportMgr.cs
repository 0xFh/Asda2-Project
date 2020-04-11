using System.Collections.Generic;
using WCell.Constants.GameObjects;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Transports
{
    public class TransportMgr
    {
        public static Dictionary<GOEntryId, Transport> Transports = new Dictionary<GOEntryId, Transport>();

        public static Dictionary<GOEntryId, TransportEntry> TransportEntries =
            new Dictionary<GOEntryId, TransportEntry>();

        public static bool Loaded { get; private set; }

        public static void Initialize()
        {
            TransportMgr.LoadTransportEntries();
        }

        public static void LoadTransportEntries()
        {
            if (TransportMgr.Loaded)
                return;
            ContentMgr.Load<TransportEntry>();
            TransportMgr.Loaded = true;
        }

        public static bool SpawnTransport(GOEntryId entryId)
        {
            TransportEntry transportEntry;
            return TransportMgr.TransportEntries.TryGetValue(entryId, out transportEntry);
        }
    }
}