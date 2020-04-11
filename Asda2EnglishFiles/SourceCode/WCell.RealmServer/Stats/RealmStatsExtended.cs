using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Core.Network;

namespace WCell.RealmServer.Stats
{
    /// <summary>Handles network statistics and trend tracking.</summary>
    public class RealmStatsExtended : NetworkStatistics
    {
        private int m_consumeIntervalMillis = 300000;

        public static RealmStatsExtended Instance
        {
            get { return RealmStatsExtended.StatisticsSingleton.Instance; }
        }

        protected override void ConsumeStatistics()
        {
            Dictionary<uint, List<int>> dictionary = new Dictionary<uint, List<int>>();
            foreach (PacketInfo queuedStat in this.m_queuedStats)
            {
                if (!dictionary.ContainsKey(queuedStat.PacketID.RawId))
                    dictionary.Add(queuedStat.PacketID.RawId, new List<int>());
                dictionary[queuedStat.PacketID.RawId].Add(queuedStat.PacketSize);
            }

            List<ExtendedPacketInfo> extendedPacketInfoList = new List<ExtendedPacketInfo>();
            foreach (KeyValuePair<uint, List<int>> keyValuePair in dictionary)
            {
                ExtendedPacketInfo extendedPacketInfo = new ExtendedPacketInfo()
                {
                    OpCode = (RealmServerOpCode) keyValuePair.Key,
                    PacketCount = keyValuePair.Value.Count,
                    TotalPacketSize = (long) keyValuePair.Value.Sum(),
                    AveragePacketSize = (int) keyValuePair.Value.Average(),
                    StartTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(5.0)),
                    EndTime = DateTime.Now
                };
                this.GetStandardDeviation((IEnumerable<int>) keyValuePair.Value);
            }
        }

        private int GetStandardDeviation(IEnumerable<int> values)
        {
            double setAverage = values.Average();
            return (int) Math.Sqrt(
                values.Sum<int>((Func<int, double>) (val =>
                    ((double) val - setAverage) * ((double) val - setAverage))) / (double) (values.Count<int>() - 1));
        }

        public void Start()
        {
            this.m_consumerTimer.Start(this.m_consumeIntervalMillis, this.m_consumeIntervalMillis);
        }

        public void Start(int interval)
        {
            this.m_consumeIntervalMillis = interval;
            this.m_consumerTimer.Start(this.m_consumeIntervalMillis, this.m_consumeIntervalMillis);
        }

        public void Stop()
        {
            this.m_consumerTimer.Stop();
        }

        private class StatisticsSingleton
        {
            internal static readonly RealmStatsExtended Instance = new RealmStatsExtended();
        }
    }
}