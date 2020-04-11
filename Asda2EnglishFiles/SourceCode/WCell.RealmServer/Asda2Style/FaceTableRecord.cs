using WCell.Util.Data;

namespace WCell.RealmServer.Asda2Style
{
    [DataHolder]
    public class FaceTableRecord : IDataHolder
    {
        public short Id { get; set; }

        public byte IsEnabled { get; set; }

        public byte OneOrTwo { get; set; }

        public int FaceId { get; set; }

        public int Price { get; set; }

        public int CuponCount { get; set; }

        public void FinalizeDataHolder()
        {
            if (Asda2StyleMgr.FaceTemplates.ContainsKey(this.Id))
                return;
            Asda2StyleMgr.FaceTemplates.Add(this.Id, this);
        }
    }
}