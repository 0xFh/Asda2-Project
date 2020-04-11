using System.Collections.Generic;

namespace WCell.RealmServer.Looting
{
    /// <summary>Necessary for UDB's awful loot-relations</summary>
    public class ResolvedLootItemList : List<LootEntity>
    {
        public byte ResolveStatus;
        public List<LootGroup> Groups;
    }
}