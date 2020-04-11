using WCell.Constants.Factions;
using WCell.Util.Data;

namespace WCell.RealmServer.Factions
{
    public struct IndexedFactionReputationEntry
    {
        [NotPersistent] public uint Index;
        public FactionReputationIndex FactionId;
        public int Value;
    }
}