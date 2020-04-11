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
                if ((object) Statistics<T>.instance == null)
                    Statistics<T>.instance = Activator.CreateInstance<T>();
                return (object) Statistics<T>.instance as Statistics<T>;
            }
        }

        /// <summary>The Statistic-timer update interval in seconds</summary>
        public int StatsPostInterval
        {
            get { return Statistics.s_interval; }
            set
            {
                if (value > 0)
                    Statistics<T>.instance.Change(value * 1000);
                else
                    Statistics<T>.instance.Change(-1);
                Statistics.s_interval = value;
            }
        }
    }
}