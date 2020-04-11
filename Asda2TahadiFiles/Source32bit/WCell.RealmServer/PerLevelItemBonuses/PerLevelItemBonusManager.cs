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
    [NotVariable]public static Dictionary<int, PerlevelItemBonusTemplate> Templates =
      new Dictionary<int, PerlevelItemBonusTemplate>();

    [NotVariable]public static Dictionary<int, PerlevelItemBonusTemplate> PrivateTemplates =
      new Dictionary<int, PerlevelItemBonusTemplate>();

    [NotVariable]public static Dictionary<byte, List<PerlevelItemBonusTemplateItem>> BonusItemsPerLevelPublic =
      new Dictionary<byte, List<PerlevelItemBonusTemplateItem>>();

    public static IList<PerlevelItemBonusTemplateItem> GetBonusItemList(byte level, byte rebornCount,
      int extendedPlanId)
    {
      List<PerlevelItemBonusTemplateItem> bonusTemplateItemList = new List<PerlevelItemBonusTemplateItem>();
      if(BonusItemsPerLevelPublic.ContainsKey(level))
        bonusTemplateItemList.AddRange(BonusItemsPerLevelPublic[level]
          .Where(i =>
          {
            if(i.Template.RebornCount != byte.MaxValue &&
               i.Template.RebornCount != rebornCount)
              return false;
            if(i.Chance != 100)
              return i.Chance >= Utility.Random(0, 100);
            return true;
          }));
      if(extendedPlanId > 0 && PrivateTemplates.ContainsKey(extendedPlanId))
        bonusTemplateItemList.AddRange(PrivateTemplates[extendedPlanId].Items
          .Where(i =>
          {
            if(i.Level != level || i.Template.RebornCount != byte.MaxValue &&
               i.Template.RebornCount != rebornCount)
              return false;
            if(i.Chance != 100)
              return i.Chance >= Utility.Random(0, 100);
            return true;
          }));
      return bonusTemplateItemList;
    }

    [Initialization(InitializationPass.Last, "PerLevelBonuses")]
    public static void Init()
    {
      ContentMgr.Load<PerlevelItemBonusTemplate>();
      ContentMgr.Load<PerlevelItemBonusTemplateItem>();
    }
  }
}