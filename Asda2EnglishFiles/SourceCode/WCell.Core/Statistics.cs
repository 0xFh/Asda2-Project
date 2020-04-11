using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using WCell.Util;

namespace WCell.Core
{
    public abstract class Statistics
    {
        protected static int s_interval = 1800000;
        protected static Logger log = LogManager.GetCurrentClassLogger();
        protected DateTime m_lastUpdate = DateTime.Now;
        protected static bool inited;
        protected Timer m_statTimer;
        protected long m_lastBytesSent;
        protected long m_lastBytesReceived;
        public readonly PerformanceCounter CPUPerfCounter;
        public readonly PerformanceCounter MemPerfCounter;

        protected Statistics()
        {
            try
            {
                this.m_lastUpdate = DateTime.Now;
                this.m_lastBytesSent = 0L;
                this.m_lastBytesReceived = 0L;
                Process currentProcess = Process.GetCurrentProcess();
                this.CPUPerfCounter = new PerformanceCounter("Process", "% Processor Time", currentProcess.ProcessName);
                this.MemPerfCounter = new PerformanceCounter("Process", "Private Bytes", currentProcess.ProcessName);
                this.m_statTimer = new Timer(new TimerCallback(this.OnTick));
            }
            catch (Exception ex)
            {
                Statistics.log.Warn("Could not initialize Performance Counters.", (object) ex);
            }
        }

        public void Change(int seconds)
        {
            Statistics.s_interval = seconds;
            if (seconds > 0)
                seconds *= 1000;
            this.m_statTimer.Change(seconds, seconds);
        }

        private void OnTick(object state)
        {
            foreach (string fullStat in this.GetFullStats())
                Statistics.log.Info(fullStat);
        }

        public abstract long TotalBytesSent { get; }

        public abstract long TotalBytesReceived { get; }

        public List<string> GetFullStats()
        {
            List<string> stringList = new List<string>();
            stringList.Add("----------------- Statistics ------------------");
            this.GetStats((ICollection<string>) stringList);
            stringList.Add("-----------------------------------------------");
            return stringList;
        }

        public virtual void GetStats(ICollection<string> statLines)
        {
            Process currentProcess = Process.GetCurrentProcess();
            TimeSpan timeSpan = DateTime.Now - currentProcess.StartTime;
            long totalBytesSent = this.TotalBytesSent;
            long totalBytesReceived = this.TotalBytesReceived;
            double num1 = (double) totalBytesSent / timeSpan.TotalSeconds;
            double num2 = (double) totalBytesReceived / timeSpan.TotalSeconds;
            double totalSeconds = (DateTime.Now - this.m_lastUpdate).TotalSeconds;
            this.m_lastUpdate = DateTime.Now;
            double num3 = (double) (totalBytesSent - this.m_lastBytesSent) / totalSeconds;
            double num4 = (double) (totalBytesReceived - this.m_lastBytesReceived) / totalSeconds;
            this.m_lastBytesSent = totalBytesSent;
            this.m_lastBytesReceived = totalBytesReceived;
            float num5 = this.CPUPerfCounter.NextValue();
            float num6 = this.MemPerfCounter.NextValue();
            statLines.Add(string.Format("+ CPU Usage: {0:0.00}% <-> Memory Usage: {1}", (object) num5,
                (object) WCellUtil.FormatBytes(num6)));
            statLines.Add(string.Format("+ Upload: Total {0} - Avg {1}/s - Current {2}/s",
                (object) WCellUtil.FormatBytes((float) totalBytesSent), (object) WCellUtil.FormatBytes(num1),
                (object) WCellUtil.FormatBytes(num3)));
            statLines.Add(string.Format("+ Download: Total: {0} - Avg: {1}/s - Current {2}/s",
                (object) WCellUtil.FormatBytes((float) totalBytesReceived), (object) WCellUtil.FormatBytes(num2),
                (object) WCellUtil.FormatBytes(num4)));
            int[] numArray = new int[GC.MaxGeneration + 1];
            for (int generation = 0; generation <= GC.MaxGeneration; ++generation)
                numArray[generation] = GC.CollectionCount(generation);
            statLines.Add(string.Format("+ Thread Count: {0} - GC Counts: {1}", (object) currentProcess.Threads.Count,
                (object) ((IEnumerable<int>) numArray).ToString<int>(", ")));
        }
    }
}