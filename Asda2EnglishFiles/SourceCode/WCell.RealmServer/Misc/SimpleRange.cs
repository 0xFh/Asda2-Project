using System;

namespace WCell.RealmServer.Misc
{
    [Serializable]
    public struct SimpleRange
    {
        public float MinDist;
        public float MaxDist;

        public SimpleRange(float min, float max)
        {
            this.MinDist = min;
            this.MaxDist = max;
        }

        public float Average
        {
            get { return this.MinDist + (float) (((double) this.MaxDist - (double) this.MinDist) / 2.0); }
        }
    }
}