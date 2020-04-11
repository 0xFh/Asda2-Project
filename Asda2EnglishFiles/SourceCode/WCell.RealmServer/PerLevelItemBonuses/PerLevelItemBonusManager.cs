using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.PerLevelItemBonuses
{
    public static class PerLevelItemBonusManager
    {
        [NotVariable]
        public static Dictionary<int, PerlevelItemBonusTemplate> Templates =
            new Dictionary<int, PerlevelItemBonusTemplate>();

        [NotVariable]
        public static Dictionary<int, PerlevelItemBonusTemplate> PrivateTemplates =
            new Dictionary<int, PerlevelItemBonusTemplate>();

        [NotVariable] public static Dictionary<byte, List<PerlevelItemBonusTemplateItem>> BonusItemsPerLevelPublic =
            new Dictionary<byte, List<PerlevelItemBonusTemplateItem>>();

        public static IList<PerlevelItemBonusTemplateItem> GetBonusItemList(byte level, byte rebornCount,
            int extendedPlanId)
        {
            List<PerlevelItemBonusTemplateItem> bonusTemplateItemList = new List<PerlevelItemBonusTemplateItem>();
            if (PerLevelItemBonusManager.BonusItemsPerLevelPublic.ContainsKey(level))
                bonusTemplateItemList.AddRange(PerLevelItemBonusManager.BonusItemsPerLevelPublic[level]
                    .Where<PerlevelItemBonusTemplateItem>((Func<PerlevelItemBonusTemplateItem, bool>) (i =>
                    {
                        if (i.Template.RebornCount != byte.MaxValue &&
                            (int) i.Template.RebornCount != (int) rebornCount)
                            return false;
                        if (i.Chance != 100)
                            return i.Chance >= Utility.Random(0, 100);
                        return true;
                    })));
            if (extendedPlanId > 0 && PerLevelItemBonusManager.PrivateTemplates.ContainsKey(extendedPlanId))
                bonusTemplateItemList.AddRange(PerLevelItemBonusManager.PrivateTemplates[extendedPlanId].Items
                    .Where<PerlevelItemBonusTemplateItem>((Func<PerlevelItemBonusTemplateItem, bool>) (i =>
                    {
                        if ((int) i.Level != (int) level || i.Template.RebornCount != byte.MaxValue &&
                            (int) i.Template.RebornCount != (int) rebornCount)
                            return false;
                        if (i.Chance != 100)
                            return i.Chance >= Utility.Random(0, 100);
                        return true;
                    })));
            return (IList<PerlevelItemBonusTemplateItem>) bonusTemplateItemList;
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Last, "PerLevelBonuses")]
        public static void Init()
        {
            ContentMgr.Load<PerlevelItemBonusTemplate>();
            ContentMgr.Load<PerlevelItemBonusTemplateItem>();
        }
    }
}