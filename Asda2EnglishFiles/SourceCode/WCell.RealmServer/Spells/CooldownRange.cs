using System;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
    [Serializable]
    public class CooldownRange
    {
        public int MinDelay;
        public int MaxDelay;

        public CooldownRange()
        {
        }

        public CooldownRange(int min, int max)
        {
            this.MinDelay = min;
            this.MaxDelay = max;
        }

        public int GetRandomCooldown()
        {
            return Utility.Random(this.MinDelay, this.MaxDelay);
        }
    }
}