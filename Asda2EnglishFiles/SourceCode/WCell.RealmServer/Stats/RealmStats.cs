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
        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth)]
        public static void Init()
        {
            Statistics.inited = true;
            RealmStats.StatsPostDelay = Statistics.s_interval;
        }

        /// <summary>
        /// 
        /// </summary>
        public static int StatsPostDelay
        {
            get
            {
                if (Statistics<RealmStats>.instance == null)
                    return 0;
                return Statistics<RealmStats>.instance.StatsPostInterval;
            }
            set
            {
                if (!Statistics.inited)
                {
                    Statistics.s_interval = value;
                }
                else
                {
                    if (Statistics<RealmStats>.instance == null)
                    {
                        if (value <= 0)
                            return;
                        Statistics<RealmStats>.instance = new RealmStats();
                    }

                    Statistics<RealmStats>.Instance.StatsPostInterval = value;
                }
            }
        }

        public override void GetStats(ICollection<string> list)
        {
            list.Add(string.Format("+ Uptime: {0}",
                (object) ServerApp<WCell.RealmServer.RealmServer>.RunTime.Format()));
            list.Add(string.Format("+ Players Online: {0} (Horde: {1}, Alliance: {2})", (object) World.CharacterCount,
                (object) World.HordeCharCount, (object) World.AllianceCharCount));
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