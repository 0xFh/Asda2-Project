using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
    public class QuestPOIPoints : IDataHolder
    {
        public uint QuestId;
        public uint PoiId;
        public float X;
        public float Y;

        public void FinalizeDataHolder()
        {
            List<QuestPOI> source;
            if (!QuestMgr.POIs.TryGetValue(this.QuestId, out source))
                return;
            foreach (QuestPOI questPoi in source.Where<QuestPOI>((Func<QuestPOI, bool>) (questpoi =>
                (int) questpoi.PoiId == (int) this.PoiId)))
                questPoi.Points.Add(this);
        }
    }
}