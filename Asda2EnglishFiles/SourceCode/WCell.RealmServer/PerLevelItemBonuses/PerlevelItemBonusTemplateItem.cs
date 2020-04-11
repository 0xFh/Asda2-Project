using System.Collections.Generic;
using WCell.Util.Data;
using WCell.Util.NLog;

namespace WCell.RealmServer.PerLevelItemBonuses
{
    [DataHolder]
    public class PerlevelItemBonusTemplateItem : IDataHolder
    {
        [NotPersistent] public PerlevelItemBonusTemplate Template { get; set; }

        public int Id { get; set; }

        public int TemplateId { get; set; }

        public int ItemId { get; set; }

        public byte Level { get; set; }

        public int Amount { get; set; }

        public int Chance { get; set; }

        public void FinalizeDataHolder()
        {
            if (!PerLevelItemBonusManager.Templates.ContainsKey(this.TemplateId))
            {
                LogUtil.WarnException("For PerlevelItemBonusTemplateItem {1} primaritemplate {2} not founded!",
                    new object[2]
                    {
                        (object) this.Id,
                        (object) this.TemplateId
                    });
            }
            else
            {
                this.Template = PerLevelItemBonusManager.Templates[this.TemplateId];
                this.Template.Items.Add(this);
                if (this.Template.IsPrivate)
                    return;
                if (!PerLevelItemBonusManager.BonusItemsPerLevelPublic.ContainsKey(this.Level))
                    PerLevelItemBonusManager.BonusItemsPerLevelPublic.Add(this.Level,
                        new List<PerlevelItemBonusTemplateItem>());
                PerLevelItemBonusManager.BonusItemsPerLevelPublic[this.Level].Add(this);
            }
        }
    }
}