using System.Collections.Generic;
using WCell.Util.Data;

namespace WCell.RealmServer.PerLevelItemBonuses
{
    [DataHolder]
    public class PerlevelItemBonusTemplate : IDataHolder
    {
        [NotPersistent] public List<PerlevelItemBonusTemplateItem> Items = new List<PerlevelItemBonusTemplateItem>();

        public int Id { get; set; }

        public string Name { get; set; }

        public byte RebornCount { get; set; }

        public bool IsPrivate { get; set; }

        public void FinalizeDataHolder()
        {
            PerLevelItemBonusManager.Templates.Add(this.Id, this);
            if (!this.IsPrivate)
                return;
            PerLevelItemBonusManager.PrivateTemplates.Add(this.Id, this);
        }
    }
}