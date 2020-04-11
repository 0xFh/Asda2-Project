using System;
using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class ExploreAreaAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public WorldMapOverlayId WorldMapOverlayId;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if ((WorldMapOverlayId) value1 != this.WorldMapOverlayId)
                return;
            WorldMapOverlayEntry worldMapOverlayEntry = WCell.RealmServer.Global.World.s_WorldMapOverlayEntries[value1];
            if (worldMapOverlayEntry == null)
                return;
            bool flag = false;
            foreach (ZoneId zoneID in worldMapOverlayEntry.ZoneTemplateId)
            {
                if (zoneID != ZoneId.None)
                {
                    ZoneTemplate zoneInfo = WCell.RealmServer.Global.World.GetZoneInfo(zoneID);
                    if (zoneInfo.ExplorationBit >= 0 && achievements.Owner.IsZoneExplored(zoneInfo.ExplorationBit))
                    {
                        flag = true;
                        break;
                    }
                }
                else
                    break;
            }

            if (!flag)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, 1U, ProgressType.ProgressSet);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1U;
        }
    }
}