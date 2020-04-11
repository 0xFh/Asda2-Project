using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
    [DataHolder]
    public class Asda2GuildWaveItemRecord : IDataHolder
    {
        public int Id { get; set; }

        public int Wave { get; set; }

        public int Lvl { get; set; }

        public int Difficulty { get; set; }

        public int Item1 { get; set; }

        public int Item2 { get; set; }

        public int Item3 { get; set; }

        public int Item4 { get; set; }

        public int Item5 { get; set; }

        public int Item6 { get; set; }

        public int Item7 { get; set; }

        public int Item8 { get; set; }

        public int Chance1 { get; set; }

        public int Chance2 { get; set; }

        public int Chance3 { get; set; }

        public int Chance4 { get; set; }

        public int Chance5 { get; set; }

        public int Chance6 { get; set; }

        public int Chance7 { get; set; }

        public int Chance8 { get; set; }

        public void FinalizeDataHolder()
        {
            Asda2ItemMgr.GuildWaveRewardRecords.SetValue((object) this, this.Id);
        }
    }
}