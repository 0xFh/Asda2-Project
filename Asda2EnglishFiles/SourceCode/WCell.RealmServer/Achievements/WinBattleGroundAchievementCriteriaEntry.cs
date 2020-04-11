using System.Runtime.InteropServices;
using WCell.Constants.World;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class WinBattleGroundAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public MapId MapId;
        public uint WinCount;
        public uint AdditionalRequirement1Type;
        public uint AdditionalRequirement1Value;
        public uint AdditionalRequirement2Type;
        public uint AdditionalRequirement2Value;
    }
}