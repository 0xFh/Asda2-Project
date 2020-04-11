using WCell.Util.Data;

namespace WCell.RealmServer.Formulas
{
    public class LevelXp : IDataHolder
    {
        public int Level;
        public int Xp;

        public uint GetId()
        {
            return (uint) this.Level;
        }

        public DataHolderState DataHolderState { get; set; }

        public void FinalizeDataHolder()
        {
        }
    }
}