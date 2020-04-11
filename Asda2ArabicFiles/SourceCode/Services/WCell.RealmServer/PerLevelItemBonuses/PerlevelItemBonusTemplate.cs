using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Items;
using WCell.Util.Data;

namespace WCell.RealmServer.PerLevelItemBonuses
{
    [DataHolder]
    public class PerlevelItemBonusTemplate : IDataHolder
    {
        [NotPersistent]
        public List<PerlevelItemBonusTemplateItem> Items = new List<PerlevelItemBonusTemplateItem>(); 
        public int Id { get; set; }
        public string Name { get; set; }
        public byte RebornCount { get; set; }
        public bool IsPrivate { get; set; }
        public void FinalizeDataHolder()
        {
            PerLevelItemBonusManager.Templates.Add(Id,this);
            if(IsPrivate)
                PerLevelItemBonusManager.PrivateTemplates.Add(Id,this);
        }
    }
}
