using Castle.ActiveRecord;
using System;
using WCell.Constants.World;
using WCell.Core.Database;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Instances
{
    [Castle.ActiveRecord.ActiveRecord]
    public class GlobalInstanceTimer : WCellRecord<GlobalInstanceTimer>
    {
        public MapId MapId;

        /// <summary>Load and verify timers</summary>
        public static GlobalInstanceTimer[] LoadTimers()
        {
            GlobalInstanceTimer[] all = ActiveRecordBase<GlobalInstanceTimer>.FindAll();
            GlobalInstanceTimer[] globalInstanceTimerArray = new GlobalInstanceTimer[1727];
            foreach (GlobalInstanceTimer globalInstanceTimer in all)
                globalInstanceTimerArray[(int) globalInstanceTimer.MapId] = globalInstanceTimer;
            MapTemplate[] mapTemplates = WCell.RealmServer.Global.World.MapTemplates;
            for (int index1 = 0; index1 < mapTemplates.Length; ++index1)
            {
                MapTemplate mapTemplate = mapTemplates[index1];
                GlobalInstanceTimer globalInstanceTimer = globalInstanceTimerArray[index1];
                if (mapTemplate != null && mapTemplate.IsInstance)
                {
                    if (globalInstanceTimer != null && globalInstanceTimer.LastResets.Length != 4)
                    {
                        DateTime[] lastResets = globalInstanceTimer.LastResets;
                        Array.Resize<DateTime>(ref lastResets, 4);
                        globalInstanceTimer.LastResets = lastResets;
                        for (int index2 = 0; index2 < globalInstanceTimer.LastResets.Length; ++index2)
                            globalInstanceTimer.LastResets[index2] = DateTime.Now;
                    }
                    else
                    {
                        for (int index2 = 0; index2 < mapTemplate.Difficulties.Length; ++index2)
                        {
                            MapDifficultyEntry difficulty = mapTemplate.Difficulties[index2];
                            if (difficulty != null && globalInstanceTimer == null && difficulty.ResetTime > 0)
                            {
                                globalInstanceTimerArray[index1] =
                                    globalInstanceTimer = new GlobalInstanceTimer(mapTemplate.Id);
                                globalInstanceTimer.LastResets[index2] = DateTime.Now;
                                globalInstanceTimer.Save();
                            }
                        }
                    }
                }
                else if (globalInstanceTimer != null)
                {
                    globalInstanceTimer.Delete();
                    globalInstanceTimerArray[index1] = (GlobalInstanceTimer) null;
                }
            }

            return globalInstanceTimerArray;
        }

        public GlobalInstanceTimer(MapId id)
        {
            this.State = RecordState.New;
            this.MapId = id;
            this.LastResets = new DateTime[4];
        }

        public GlobalInstanceTimer()
        {
        }

        [PrimaryKey(PrimaryKeyType.Assigned, "MapId")]
        private int m_MapId
        {
            get { return (int) this.MapId; }
            set { this.MapId = (MapId) value; }
        }

        [Property(NotNull = true)] public DateTime[] LastResets { get; set; }
    }
}