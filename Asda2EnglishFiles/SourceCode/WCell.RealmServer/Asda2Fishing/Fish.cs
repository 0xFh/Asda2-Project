using System.Collections.Generic;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Asda2Fishing
{
    public class Fish
    {
        public Asda2ItemTemplate ItemTemplate { get; set; }

        public List<int> BaitIds { get; set; }

        public byte MinLength { get; set; }

        public byte MaxLength { get; set; }

        public int FishingTime { get; set; }
    }
}