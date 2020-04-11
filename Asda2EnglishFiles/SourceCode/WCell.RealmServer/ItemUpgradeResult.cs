﻿namespace WCell.RealmServer
{
    public class ItemUpgradeResult
    {
        public ItemUpgradeResultStatus Status { get; set; }

        public float BoostFormGroupLuck { get; set; }

        public float BoostFromNearbyCharactersLuck { get; set; }

        public float BoostFromOwnerLuck { get; set; }

        public double Chance { get; set; }

        public ItemUpgradeResult(ItemUpgradeResultStatus success, float boostFormGroupLuck,
            float boostFromNearbyCharactersLuck, float boostFromOwnerLuck, double chance)
        {
            this.Status = success;
            this.BoostFormGroupLuck = boostFormGroupLuck;
            this.BoostFromNearbyCharactersLuck = boostFromNearbyCharactersLuck;
            this.BoostFromOwnerLuck = boostFromOwnerLuck;
            this.Chance = chance;
        }
    }
}