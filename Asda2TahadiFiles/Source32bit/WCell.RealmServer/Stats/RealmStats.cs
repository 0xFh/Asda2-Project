using Cell.Core;
using System.Collections.Generic;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Stats
{
  public class RealmStats : Statistics<RealmStats>
  {
    [Initialization(InitializationPass.Tenth)]
    public static void Init()
    {
      inited = true;
      StatsPostDelay = s_interval;
    }

    /// <summary>
    /// 
    /// </summary>
    public static int StatsPostDelay
    {
      get
      {
        if(instance == null)
          return 0;
        return instance.StatsPostInterval;
      }
      set
      {
        if(!inited)
        {
          s_interval = value;
        }
        else
        {
          if(instance == null)
          {
            if(value <= 0)
              return;
            instance = new RealmStats();
          }

          Instance.StatsPostInterval = value;
        }
      }
    }

    public override void GetStats(ICollection<string> list)
    {
      list.Add(string.Format("+ Uptime: {0}",
        ServerApp<RealmServer>.RunTime.Format()));
      list.Add(string.Format("+ Players Online: {0} (Horde: {1}, Alliance: {2})", World.CharacterCount,
        World.HordeCharCount, World.AllianceCharCount));
      base.GetStats(list);
      list.Add("+ Map Load Average: " + Map.LoadAvgStr);
    }

    public override long TotalBytesSent
    {
      get { return ClientBase.TotalBytesSent; }
    }

    public override long TotalBytesReceived
    {
      get { return ClientBase.TotalBytesReceived; }
    }
  }
}