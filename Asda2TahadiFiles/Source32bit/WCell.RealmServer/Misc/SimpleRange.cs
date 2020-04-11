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
      MinDist = min;
      MaxDist = max;
    }

    public float Average
    {
      get { return MinDist + (float) ((MaxDist - (double) MinDist) / 2.0); }
    }
  }
}