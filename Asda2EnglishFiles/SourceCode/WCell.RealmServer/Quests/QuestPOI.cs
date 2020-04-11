using System.Collections.Generic;
using WCell.Constants.World;
using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
    public class QuestPOI : IDataHolder
    {
        [NotPersistent] public List<QuestPOIPoints> Points = new List<QuestPOIPoints>();
        public uint QuestId;
        public uint PoiId;
        public int ObjectiveIndex;
        public MapId MapID;
        public ZoneId ZoneId;
        public uint FloorId;
        public uint Unk3;
        public uint Unk4;

        public void FinalizeDataHolder()
        {
            if (QuestMgr.POIs.ContainsKey(this.QuestId))
            {
                QuestMgr.POIs[this.QuestId].Add(this);
            }
            else
            {
                List<QuestPOI> questPoiList = new List<QuestPOI>()
                {
                    this
                };
                QuestMgr.POIs.Add(this.QuestId, questPoiList);
            }
        }
    }
}