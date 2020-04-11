using WCell.RealmServer.Content;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Variables;

namespace WCell.RealmServer.Misc
{
    public class PageTextEntry : IDataHolder
    {
        [NotVariable] public static PageTextEntry[] Entries = new PageTextEntry[10000];
        [Persistent(8)] public string[] Texts = new string[8];
        public uint PageId;
        public uint NextPageId;
        [NotPersistent] public PageTextEntry NextPageEntry;

        public static PageTextEntry GetEntry(uint id)
        {
            return PageTextEntry.Entries.Get<PageTextEntry>(id);
        }

        public static void InitPageTexts()
        {
            ContentMgr.Load<PageTextEntry>();
            foreach (PageTextEntry entry in PageTextEntry.Entries)
            {
                if (entry != null && entry.NextPageId != 0U)
                    entry.NextPageEntry = PageTextEntry.GetEntry(entry.NextPageId);
            }
        }

        public void FinalizeDataHolder()
        {
            ArrayUtil.Set<PageTextEntry>(ref PageTextEntry.Entries, this.PageId, this);
        }
    }
}