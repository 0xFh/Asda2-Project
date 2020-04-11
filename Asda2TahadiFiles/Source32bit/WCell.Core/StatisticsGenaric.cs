using System;

namespace WCell.Core
{
  public abstract class Statistics<T> : Statistics where T : Statistics, new()
  {
    protected static T instance;

    public static Statistics<T> Instance
    {
      get
      {
        if(instance == null)
          instance = Activator.CreateInstance<T>();
        return (object) instance as Statistics<T>;
      }
    }

    /// <summary>The Statistic-timer update interval in seconds</summary>
    public int StatsPostInterval
    {
      get { return s_interval; }
      set
      {
        if(value > 0)
          instance.Change(value * 1000);
        else
          instance.Change(-1);
        s_interval = value;
      }
    }
  }
}