using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Asda2Fishing
{
    public static class Asda2FishingMgr
    {
        public static Dictionary<int, FishingBookTemplate> FishingBookTemplates = new Dictionary<int, FishingBookTemplate>();
        public static Dictionary<int, FishingFishInfoRecord> FishRecords = new Dictionary<int, FishingFishInfoRecord>();

        public static Dictionary<int, FishingBaseInfoRecord> FishingBaseInfos = new Dictionary<int, FishingBaseInfoRecord>();
        public static Dictionary<int, FishingBaseInfoRecord> PremiumFishingBaseInfos = new Dictionary<int, FishingBaseInfoRecord>();
        public static Dictionary<int, List<FishingSpot>> FishingSpotsByMaps = new Dictionary<int, List<FishingSpot>>();

        [Initialization (InitializationPass.Tenth,Name = "Fishing system.")]
        public static void Init()
        {
            Content.ContentMgr.Load<FishingFishInfoRecord>();
            Content.ContentMgr.Load<FishingBaseInfoRecord>();
            Content.ContentMgr.Load<FishingSpotInfoRecord>();
            Content.ContentMgr.Load<FishingBookTemplate>();
        }
    }
    public class FishingSpot
    {
        public int RequiredFishingLevel { get; set; }
        public MapId Map { get; set; }
        public Vector3 Position { get; set; }
        public byte Radius { get; set; }
        public Dictionary<int, Fish> Fishes { get; set; }
        public Dictionary<int, Fish> PremiumFishes { get; set; }
        public Fish GetRandomFish(bool prem)
        {
            var rnd = Utility.Random(0, 100000);
            if(prem)
            {
                foreach (var premiumFish in PremiumFishes)
                {
                    if (premiumFish.Key >= rnd)
                        return premiumFish.Value;
                }
            }
            else
            {
                foreach (var premiumFish in Fishes)
                {
                    if (premiumFish.Key >= rnd)
                        return premiumFish.Value;
                }
            }
            return Fishes[0];
        }
    }
    public class Fish
    {
        public Asda2ItemTemplate ItemTemplate { get; set; }
        public List<int> BaitIds
        {
            get; set;
        }

        public byte MinLength { get; set; }
        public byte MaxLength { get; set; }
        public int FishingTime { get; set; }
    }
}
