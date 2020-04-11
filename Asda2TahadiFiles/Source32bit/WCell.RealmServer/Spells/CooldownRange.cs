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
      MinDelay = min;
      MaxDelay = max;
    }

    public int GetRandomCooldown()
    {
      return Utility.Random(MinDelay, MaxDelay);
    }
  }
}