using WCell.Constants.GameObjects;
using WCell.Util.Data;

namespace WCell.RealmServer.Transports
{
    /// <summary>Transport Entry</summary>
    public class TransportEntry : IDataHolder
    {
        public GOEntryId Id;
        public string Name;
        public uint Period;

        /// <summary>
        /// Is called to initialize the object; usually after a set of other operations have been performed or if
        /// the right time has come and other required steps have been performed.
        /// </summary>
        public void FinalizeDataHolder()
        {
            TransportMgr.TransportEntries.Add(this.Id, this);
        }

        public override string ToString()
        {
            return "Entry: " + this.Name + " (Id: " + (object) this.Id + ", Period: " + (object) this.Period + ")";
        }
    }
}