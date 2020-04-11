using System;
using System.Collections.Generic;
using WCell.Util.Data;

namespace WCell.RealmServer.Global
{
    /// <summary>
    /// Holds all information regarding a World Event
    /// such as Darkmoon Faire
    /// </summary>
    public class WorldEvent : IDataHolder
    {
        [NotPersistent] public List<WorldEventNPC> NPCSpawns = new List<WorldEventNPC>();
        [NotPersistent] public List<WorldEventGameObject> GOSpawns = new List<WorldEventGameObject>();
        [NotPersistent] public List<WorldEventNpcData> ModelEquips = new List<WorldEventNpcData>();
        [NotPersistent] public List<uint> QuestIds = new List<uint>();

        /// <summary>Id of the game event</summary>
        public uint Id;

        /// <summary>
        /// Absolute start date, the event will never start before
        /// </summary>
        public DateTime From;

        /// <summary>Absolute end date, the event will never start after</summary>
        public DateTime Until;

        [NotPersistent] public TimeSpan? TimeUntilNextStart;
        [NotPersistent] public TimeSpan? TimeUntilEnd;

        /// <summary>
        /// Do not use, time in minutes read from database.
        /// Instead you should use TimeSpan <seealso cref="F:WCell.RealmServer.Global.WorldEvent.Occurence" />
        /// </summary>
        public long _Occurence;

        /// <summary>
        /// Do not use, time in minutes read from database.
        /// Instead you should use TimeSpan <seealso cref="F:WCell.RealmServer.Global.WorldEvent.Duration" />
        /// </summary>
        public long _Length;

        /// <summary>Delay in minutes between occurences of the event</summary>
        [NotPersistent] public TimeSpan Occurence;

        /// <summary>Length in minutes of the event</summary>
        [NotPersistent] public TimeSpan Duration;

        /// <summary>Client side holiday id</summary>
        public uint HolidayId;

        /// <summary>Generally the event's name</summary>
        public string Description;

        public void FinalizeDataHolder()
        {
            this.Occurence = TimeSpan.FromMinutes((double) this._Occurence);
            this.Duration = TimeSpan.FromMinutes((double) this._Length);
            WorldEvent.CalculateEventDelays(this);
            WorldEventMgr.AddEvent(this);
        }

        public static void CalculateEventDelays(WorldEvent worldEvent)
        {
            DateTime now = DateTime.Now;
            if (now < worldEvent.Until)
            {
                if (worldEvent.From > now)
                {
                    worldEvent.TimeUntilNextStart = new TimeSpan?(worldEvent.From - now);
                }
                else
                {
                    DateTime from = worldEvent.From;
                    while (from < now && (!(from + worldEvent.Duration > now) ||
                                          !(from + worldEvent.Duration < from + worldEvent.Occurence)))
                        from += worldEvent.Occurence;
                    worldEvent.TimeUntilNextStart = new TimeSpan?(from - now);
                }

                WorldEvent worldEvent1 = worldEvent;
                TimeSpan? timeUntilNextStart = worldEvent.TimeUntilNextStart;
                TimeSpan duration = worldEvent.Duration;
                TimeSpan? nullable = timeUntilNextStart.HasValue
                    ? new TimeSpan?(timeUntilNextStart.GetValueOrDefault() + duration)
                    : new TimeSpan?();
                worldEvent1.TimeUntilEnd = nullable;
            }
            else
            {
                worldEvent.TimeUntilNextStart = new TimeSpan?();
                worldEvent.TimeUntilEnd = new TimeSpan?();
            }
        }
    }
}