using System;
using System.Diagnostics;

namespace WCell.RealmServer.Stats
{
    /// <summary>
    /// Creates and manages custom WCell performance counters.
    /// </summary>
    public static class PerformanceCounters
    {
        private const string CategoryName = "WCell";

        /// <summary>
        /// Performance counter for the number of packets sent per second.
        /// </summary>
        public static PerformanceCounter PacketsSentPerSecond { get; internal set; }

        /// <summary>
        /// Performance counter for the number of packets received per second.
        /// </summary>
        public static PerformanceCounter PacketsReceivedPerSecond { get; internal set; }

        /// <summary>
        /// Performance counter for the total number of bytes sent.
        /// </summary>
        public static PerformanceCounter TotalBytesSent { get; internal set; }

        /// <summary>
        /// Performance counter for the total numbers of bytes received.
        /// </summary>
        public static PerformanceCounter TotalBytesReceived { get; internal set; }

        /// <summary>
        /// Performance counter for the number of clients in the auth queue.
        /// </summary>
        public static PerformanceCounter NumbersOfClientsInAuthQueue { get; internal set; }

        /// <summary>
        /// Initializes the performance counters if they haven't already been created.
        /// </summary>
        public static void Initialize()
        {
            if (!PerformanceCounterCategory.Exists("WCell"))
            {
                Console.WriteLine("Installing Performance Counters...");
                Process process = Process.Start(new ProcessStartInfo("WCell.PerformanceCounterInstaller.exe")
                {
                    UseShellExecute = true
                });
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception(
                        "WCell.PerformanceCounterInstaller.exe has not been run. Please run it and restart the application");
                Console.WriteLine("Done...");
            }

            PerformanceCounters.InitCounters();
        }

        public static void InitCounters()
        {
            if (!PerformanceCounterCategory.Exists("WCell"))
                throw new Exception(
                    "WCell.PerformanceCounterInstaller.exe has not been run. Please run it and restart the application");
            PerformanceCounters.PacketsSentPerSecond = new PerformanceCounter("WCell", "Packets Sent/sec", false);
            PerformanceCounters.PacketsReceivedPerSecond =
                new PerformanceCounter("WCell", "Packets Received/sec", false);
            PerformanceCounters.TotalBytesSent = new PerformanceCounter("WCell", "Bytes Sent", false);
            PerformanceCounters.TotalBytesReceived = new PerformanceCounter("WCell", "Bytes Received", false);
            PerformanceCounters.NumbersOfClientsInAuthQueue = new PerformanceCounter("WCell", "Auth Queue Size", false);
        }
    }
}