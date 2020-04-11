using System.Collections.Generic;
using WCell.Constants.World;
using WCell.RealmServer.Items;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Asda2Fishing
{
    [DataHolder]
    public class FishingSpotInfoRecord : IDataHolder
    {
        public int Id { get; set; }

        [Persistent(Length = 10)] public int[] RequiredFishingLvls { get; set; }

        [Persistent(Length = 20)] public int[] Points { get; set; }

        [Persistent(Length = 10)] public int[] Radius { get; set; }

        [Persistent(Length = 10)] public int[] DataKey { get; set; }

        public void FinalizeDataHolder()
        {
            List<FishingSpot> fishingSpotList = new List<FishingSpot>();
            Asda2FishingMgr.FishingSpotsByMaps.Add(this.Id, fishingSpotList);
            for (int index1 = 0; index1 < 10; ++index1)
            {
                if (this.RequiredFishingLvls[index1] != -1)
                {
                    FishingSpot fishingSpot = new FishingSpot();
                    fishingSpot.RequiredFishingLevel = this.RequiredFishingLvls[index1];
                    fishingSpot.Map = (MapId) this.Id;
                    fishingSpot.Position =
                        new Vector3((float) this.Points[index1], (float) this.Points[index1 + 10], 0.0f);
                    fishingSpot.Radius = (byte) this.Radius[index1];
                    fishingSpot.Fishes = new Dictionary<int, Fish>();
                    FishingBaseInfoRecord fishingBaseInfo = Asda2FishingMgr.FishingBaseInfos[this.DataKey[index1]];
                    int key1 = 0;
                    for (int index2 = 0; index2 < 20; ++index2)
                    {
                        if (fishingBaseInfo.Chances[index2] != 0)
                        {
                            key1 += fishingBaseInfo.Chances[index2];
                            Fish fish = new Fish();
                            fish.ItemTemplate = Asda2ItemMgr.GetTemplate(fishingBaseInfo.ItemIds[index2]) ??
                                                Asda2ItemMgr.GetTemplate(31725);
                            fish.BaitIds = new List<int>();
                            FishingFishInfoRecord fishRecord =
                                Asda2FishingMgr.FishRecords[fishingBaseInfo.ItemIds[index2]];
                            fish.FishingTime = fishRecord.FishingTime;
                            for (int index3 = 0; index3 < 6; ++index3)
                            {
                                if (fishRecord.BaitIds[index3] != -1)
                                    fish.BaitIds.Add(fishRecord.BaitIds[index3]);
                            }

                            fish.MinLength = (byte) fishingBaseInfo.MinFishLengths[index2];
                            fish.MaxLength = (byte) fishingBaseInfo.MaxFishLenghts[index2];
                            fishingSpot.Fishes.Add(key1, fish);
                        }
                    }

                    fishingSpot.PremiumFishes = new Dictionary<int, Fish>();
                    FishingBaseInfoRecord premiumFishingBaseInfo =
                        Asda2FishingMgr.PremiumFishingBaseInfos[this.DataKey[index1]];
                    int key2 = 0;
                    for (int index2 = 0; index2 < 20; ++index2)
                    {
                        if (premiumFishingBaseInfo.Chances[index2] != 0)
                        {
                            key2 += premiumFishingBaseInfo.Chances[index2];
                            Fish fish = new Fish();
                            fish.ItemTemplate = Asda2ItemMgr.GetTemplate(premiumFishingBaseInfo.ItemIds[index2]) ??
                                                Asda2ItemMgr.GetTemplate(31725);
                            fish.BaitIds = new List<int>();
                            FishingFishInfoRecord fishRecord =
                                Asda2FishingMgr.FishRecords[premiumFishingBaseInfo.ItemIds[index2]];
                            fish.FishingTime = fishRecord.FishingTime;
                            for (int index3 = 0; index3 < 6; ++index3)
                            {
                                if (fishRecord.BaitIds[index3] != -1)
                                    fish.BaitIds.Add(fishRecord.BaitIds[index3]);
                            }

                            fish.MinLength = (byte) premiumFishingBaseInfo.MinFishLengths[index2];
                            fish.MaxLength = (byte) premiumFishingBaseInfo.MaxFishLenghts[index2];
                            fishingSpot.PremiumFishes.Add(key2, fish);
                        }
                    }

                    fishingSpotList.Add(fishingSpot);
                }
            }
        }
    }
}