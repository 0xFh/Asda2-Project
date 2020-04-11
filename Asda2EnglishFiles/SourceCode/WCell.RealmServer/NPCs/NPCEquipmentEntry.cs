using System;
using WCell.Constants.Items;
using WCell.RealmServer.Content;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
    [Serializable]
    public class NPCEquipmentEntry : IDataHolder
    {
        [Persistent(3)] public Asda2ItemId[] ItemIds = new Asda2ItemId[3];
        public uint EquipmentId;

        public void FinalizeDataHolder()
        {
            if (this.EquipmentId > 100000U)
                ContentMgr.OnInvalidDBData("NPCEquipmentEntry had invalid Id: " + (object) this.EquipmentId);
            else
                ArrayUtil.Set<NPCEquipmentEntry>(ref NPCMgr.EquipmentEntries, this.EquipmentId, this);
        }
    }
}