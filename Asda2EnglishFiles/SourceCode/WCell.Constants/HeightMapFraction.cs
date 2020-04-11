using System;

namespace WCell.Constants
{
    public class HeightMapFraction : IEquatable<HeightMapFraction>
    {
        public float FractionX;
        public float FractionY;

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals((object) null, obj))
                return false;
            if (object.ReferenceEquals((object) this, obj))
                return true;
            if (obj.GetType() != typeof(HeightMapFraction))
                return false;
            return this.Equals((HeightMapFraction) obj);
        }

        public bool Equals(HeightMapFraction obj)
        {
            if (object.ReferenceEquals((object) null, (object) obj))
                return false;
            if (object.ReferenceEquals((object) this, (object) obj))
                return true;
            return (double) obj.FractionX == (double) this.FractionX &&
                   (double) obj.FractionY == (double) this.FractionY;
        }

        public override int GetHashCode()
        {
            return this.FractionX.GetHashCode() * 397 ^ this.FractionY.GetHashCode();
        }

        public static bool operator ==(HeightMapFraction left, HeightMapFraction right)
        {
            return object.Equals((object) left, (object) right);
        }

        public static bool operator !=(HeightMapFraction left, HeightMapFraction right)
        {
            return !object.Equals((object) left, (object) right);
        }
    }
}