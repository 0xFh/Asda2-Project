using System.Collections.Generic;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.Asda2Fishing
{
    public static class Asda2FishingMgr
    {
        public static Dictionary<int, FishingBookTemplate> FishingBookTemplates =
            new Dictionary<int, FishingBookTemplate>();

        public static Dictionary<int, FishingFishInfoRecord> FishRecords = new Dictionary<int, FishingFishInfoRecord>();

        public static Dictionary<int, FishingBaseInfoRecord> FishingBaseInfos =
            new Dictionary<int, FishingBaseInfoRecord>();

        public static Dictionary<int, FishingBaseInfoRecord> PremiumFishingBaseInfos =
            new Dictionary<int, FishingBaseInfoRecord>();

        public static Dictionary<int, List<FishingSpot>> FishingSpotsByMaps = new Dictionary<int, List<FishingSpot>>();

        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth, Name = "Fishing system.")]
        public static void Init()
        {
            ContentMgr.Load<FishingFishInfoRecord>();
            ContentMgr.Load<FishingBaseInfoRecord>();
            ContentMgr.Load<FishingSpotInfoRecord>();
            ContentMgr.Load<FishingBookTemplate>();
        }
    }
}