using WCell.Util.Data;

namespace WCell.RealmServer.Asda2Style
{
    [DataHolder]
    public class HairTableRecord : IDataHolder
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public byte IsEnabled { get; set; }

        public byte HairId { get; set; }

        public byte OneOrTwo { get; set; }

        public byte HairColor { get; set; }

        public int Price { get; set; }

        public int CuponCount { get; set; }

        public void FinalizeDataHolder()
        {
            if (Asda2StyleMgr.HairTemplates.ContainsKey((short) this.Id))
                return;
            Asda2StyleMgr.HairTemplates.Add((short) this.Id, this);
        }
    }
}