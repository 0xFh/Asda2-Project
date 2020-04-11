using System;

namespace WCell.Constants
{
  public class HeightMapFraction : IEquatable<HeightMapFraction>
  {
    public float FractionX;
    public float FractionY;

    public override bool Equals(object obj)
    {
      if(ReferenceEquals(null, obj))
        return false;
      if(ReferenceEquals(this, obj))
        return true;
      if(obj.GetType() != typeof(HeightMapFraction))
        return false;
      return Equals((HeightMapFraction) obj);
    }

    public bool Equals(HeightMapFraction obj)
    {
      if(ReferenceEquals(null, obj))
        return false;
      if(ReferenceEquals(this, obj))
        return true;
      return obj.FractionX == (double) FractionX &&
             obj.FractionY == (double) FractionY;
    }

    public override int GetHashCode()
    {
      return FractionX.GetHashCode() * 397 ^ FractionY.GetHashCode();
    }

    public static bool operator ==(HeightMapFraction left, HeightMapFraction right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(HeightMapFraction left, HeightMapFraction right)
    {
      return !Equals(left, right);
    }
  }
}