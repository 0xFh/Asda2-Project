namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOTrapDoorEntry : GOEntry
    {
        public int WhenToPause
        {
            get { return this.Fields[0]; }
        }

        public int StartOpen
        {
            get { return this.Fields[1]; }
        }

        public int AutoClose
        {
            get { return this.Fields[2]; }
        }
    }
}