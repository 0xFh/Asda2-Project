using System.Collections.Generic;
using WCell.Constants.World;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Asda2Fishing
{
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
            int num = Utility.Random(0, 100000);
            if (prem)
            {
                foreach (KeyValuePair<int, Fish> premiumFish in this.PremiumFishes)
                {
                    if (premiumFish.Key >= num)
                        return premiumFish.Value;
                }
            }
            else
            {
                foreach (KeyValuePair<int, Fish> fish in this.Fishes)
                {
                    if (fish.Key >= num)
                        return fish.Value;
                }
            }

            return this.Fishes[0];
        }
    }
}