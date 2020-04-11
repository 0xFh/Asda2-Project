using WCell.Constants.Items;

namespace WCell.RealmServer.Misc
{
    public class LockKeyEntry
    {
        /// <summary>The index within the LockEntry</summary>
        public uint Index;

        /// <summary>Id of the required Key-Item</summary>
        public readonly Asda2ItemId KeyId;

        public LockKeyEntry(uint index, uint keyId)
        {
            this.Index = index;
            this.KeyId = (Asda2ItemId) keyId;
        }

        public override string ToString()
        {
            return this.KeyId.ToString();
        }
    }
}