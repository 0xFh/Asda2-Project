using WCell.Constants.NPCs;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs.Pets
{
    public class PetLevelStatInfo : IDataHolder
    {
        [Persistent(6)] public int[] BaseStats = new int[6];
        public NPCId EntryId;
        public int Level;
        public int Health;
        public int Mana;
        public int Armor;

        public void FinalizeDataHolder()
        {
            NPCEntry entry = NPCMgr.GetEntry(this.EntryId);
            if (entry == null)
                return;
            if (entry.PetLevelStatInfos == null)
                entry.PetLevelStatInfos = new PetLevelStatInfo[100];
            ArrayUtil.Set<PetLevelStatInfo>(ref entry.PetLevelStatInfos, (uint) this.Level, this);
        }
    }
}