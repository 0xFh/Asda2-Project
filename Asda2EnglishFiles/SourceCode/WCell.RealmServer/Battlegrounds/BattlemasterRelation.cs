using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.RealmServer.Content;
using WCell.RealmServer.NPCs;
using WCell.Util.Data;

namespace WCell.RealmServer.Battlegrounds
{
    public class BattlemasterRelation : IDataHolder
    {
        public BattlegroundId BattlegroundId;
        public NPCId BattleMasterId;

        public uint GetId()
        {
            return (uint) this.BattlegroundId;
        }

        public DataHolderState DataHolderState { get; set; }

        public void FinalizeDataHolder()
        {
            NPCEntry entry = NPCMgr.GetEntry(this.BattleMasterId);
            if (entry == null)
            {
                ContentMgr.OnInvalidDBData("Invalid BattleMaster in: " + (object) this);
            }
            else
            {
                BattlegroundTemplate template = BattlegroundMgr.GetTemplate(this.BattlegroundId);
                if (template == null)
                    ContentMgr.OnInvalidDBData("Invalid Battleground in: " + (object) this);
                else
                    entry.BattlegroundTemplate = template;
            }
        }

        public override string ToString()
        {
            return this.GetType().Name + string.Format(" (BG: {0} (#{1}), BattleMaster: {2} (#{3})) ",
                       (object) this.BattlegroundId, (object) (int) this.BattlegroundId, (object) this.BattleMasterId,
                       (object) (int) this.BattleMasterId);
        }
    }
}