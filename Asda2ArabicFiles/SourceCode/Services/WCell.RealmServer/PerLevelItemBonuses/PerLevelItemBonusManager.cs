using System.Collections.Generic;
using WCell.Core.Initialization;
using System.Linq;
using WCell.RealmServer.Content;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.PerLevelItemBonuses
{
    public static class PerLevelItemBonusManager
    {
        [NotVariable]
        public static Dictionary<int, PerlevelItemBonusTemplate> Templates = new Dictionary<int, PerlevelItemBonusTemplate>();
        [NotVariable]
        public static Dictionary<int, PerlevelItemBonusTemplate> PrivateTemplates = new Dictionary<int, PerlevelItemBonusTemplate>();
        [NotVariable]
        public static Dictionary<byte,List<PerlevelItemBonusTemplateItem>> BonusItemsPerLevelPublic = new Dictionary<byte, List<PerlevelItemBonusTemplateItem>>();
        public static IList<PerlevelItemBonusTemplateItem> GetBonusItemList(byte level, byte rebornCount, int extendedPlanId)
        {
            var r = new List<PerlevelItemBonusTemplateItem>();
            if(BonusItemsPerLevelPublic.ContainsKey(level))
                r.AddRange(BonusItemsPerLevelPublic[level].Where(i=>(i.Template.RebornCount==255||i.Template.RebornCount==rebornCount)&&(i.Chance==100||i.Chance>=Utility.Random(0,100))));
            if(extendedPlanId>0&& PrivateTemplates.ContainsKey(extendedPlanId))
            {
                r.AddRange(PrivateTemplates[extendedPlanId].Items.Where(i=>(i.Level==level)&&(i.Template.RebornCount==255||i.Template.RebornCount==rebornCount)&&(i.Chance==100||i.Chance>=Utility.Random(0,100))));
            }
            return r;
        }
        [Initialization(InitializationPass.Last,"PerLevelBonuses")]
        public static void Init()
        {
            ContentMgr.Load<PerlevelItemBonusTemplate>();
            ContentMgr.Load<PerlevelItemBonusTemplateItem>();
        }
    }
}