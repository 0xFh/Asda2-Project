using System.Collections.Generic;
using WCell.Util.Data;
using WCell.Util.NLog;

namespace WCell.RealmServer.PerLevelItemBonuses
{
    [DataHolder]
    public class PerlevelItemBonusTemplateItem : IDataHolder
    {
        [NotPersistent]
        public PerlevelItemBonusTemplate Template { get; set; }
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public int ItemId { get; set; }
        public byte Level { get; set; }
        public int Amount { get; set; }
        public int Chance { get; set; }
        public void FinalizeDataHolder()
        {
            if(!PerLevelItemBonusManager.Templates.ContainsKey(TemplateId))
            {
                LogUtil.WarnException("For PerlevelItemBonusTemplateItem {1} primaritemplate {2} not founded!",Id,TemplateId);
                return;
            }
            Template = PerLevelItemBonusManager.Templates[TemplateId];
            Template.Items.Add(this);
            if(!Template.IsPrivate)
            {
                if(!PerLevelItemBonusManager.BonusItemsPerLevelPublic.ContainsKey(Level))
                    PerLevelItemBonusManager.BonusItemsPerLevelPublic.Add(Level,new List<PerlevelItemBonusTemplateItem>());
                PerLevelItemBonusManager.BonusItemsPerLevelPublic[Level].Add(this);
            }
        }
    }
}