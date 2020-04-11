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
        m_lastUpdate = DateTime.Now;
        m_lastBytesSent = 0L;
        m_lastBytesReceived = 0L;
        Process currentProcess = Process.GetCurrentProcess();
        CPUPerfCounter = new PerformanceCounter("Process", "% Processor Time", currentProcess.ProcessName);
        MemPerfCounter = new PerformanceCounter("Process", "Private Bytes", currentProcess.ProcessName);
        m_statTimer = new Timer(OnTick);
      }
      catch(Exception ex)
      {
        log.Warn("Could not initialize Performance Counters.", ex);
      }
    }

    public void Change(int seconds)
    {
      s_interval = seconds;
      if(seconds > 0)
        seconds *= 1000;
      m_statTimer.Change(seconds, seconds);
    }

    private void OnTick(object state)
    {
      foreach(string fullStat in GetFullStats())
        log.Info(fullStat);
    }

    public abstract long TotalBytesSent { get; }

    public abstract long TotalBytesReceived { get; }

    public List<string> GetFullStats()
    {
      List<string> stringList = new List<string>();
      stringList.Add("----------------- Statistics ------------------");
      GetStats(stringList);
      stringList.Add("-----------------------------------------------");
      return stringList;
    }

    public virtual void GetStats(ICollection<string> statLines)
    {
      Process currentProcess = Process.GetCurrentProcess();
      TimeSpan timeSpan = DateTime.Now - currentProcess.StartTime;
      long totalBytesSent = TotalBytesSent;
      long totalBytesReceived = TotalBytesReceived;
      double num1 = totalBytesSent / timeSpan.TotalSeconds;
      double num2 = totalBytesReceived / timeSpan.TotalSeconds;
      double totalSeconds = (DateTime.Now - m_lastUpdate).TotalSeconds;
      m_lastUpdate = DateTime.Now;
      double num3 = (totalBytesSent - m_lastBytesSent) / totalSeconds;
      double num4 = (totalBytesReceived - m_lastBytesReceived) / totalSeconds;
      m_lastBytesSent = totalBytesSent;
      m_lastBytesReceived = totalBytesReceived;
      float num5 = CPUPerfCounter.NextValue();
      float num6 = MemPerfCounter.NextValue();
      statLines.Add(string.Format("+ CPU Usage: {0:0.00}% <-> Memory Usage: {1}", num5,
        WCellUtil.FormatBytes(num6)));
      statLines.Add(string.Format("+ Upload: Total {0} - Avg {1}/s - Current {2}/s",
        WCellUtil.FormatBytes(totalBytesSent), WCellUtil.FormatBytes(num1),
        WCellUtil.FormatBytes(num3)));
      statLines.Add(string.Format("+ Download: Total: {0} - Avg: {1}/s - Current {2}/s",
        WCellUtil.FormatBytes(totalBytesReceived), WCellUtil.FormatBytes(num2),
        WCellUtil.FormatBytes(num4)));
      int[] numArray = new int[GC.MaxGeneration + 1];
      for(int generation = 0; generation <= GC.MaxGeneration; ++generation)
        numArray[generation] = GC.CollectionCount(generation);
      statLines.Add(string.Format("+ Thread Count: {0} - GC Counts: {1}", currentProcess.Threads.Count,
        numArray.ToString(", ")));
    }
  }
}