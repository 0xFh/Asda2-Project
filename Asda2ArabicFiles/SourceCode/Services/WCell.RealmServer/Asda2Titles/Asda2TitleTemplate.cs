using WCell.Util.Data;
using WCell.Util.Variables;

namespace WCell.RealmServer.Asda2Titles
{
    [DataHolder]
    public class Asda2TitleTemplate : IDataHolder
    {
        [NotVariable]
        public static Asda2TitleTemplate[] Templates = new Asda2TitleTemplate[512]; 
        public short Id { get; set; }
        public string Name { get; set; }
        public string LongDescr { get; set; }
        public string ShortDescr { get; set; }
        public byte Rarity { get; set; }
        public short Points { get; set; }
        public bool IsEnabled { get; set; }
        public byte Category { get; set; }

        #region Implementation of IDataHolder

        public void FinalizeDataHolder()
        {
            Templates[Id] = this;
        }

        #endregion
    }
}