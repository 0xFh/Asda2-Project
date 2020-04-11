using WCell.Constants.World;

namespace WCell.RealmServer.Calendars
{
    internal class CalendarStaticRaidReset
    {
        private static readonly CalendarStaticRaidReset[] raidResets = new CalendarStaticRaidReset[19]
        {
            new CalendarStaticRaidReset(MapId.TempestKeep, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.BlackTemple, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.RuinsOfAhnQiraj, 259200U, 68400U),
            new CalendarStaticRaidReset(MapId.AhnQirajTemple, 604800U, 68400U),
            new CalendarStaticRaidReset(MapId.MagtheridonsLair, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.ZulGurub, 259200U, 68400U),
            new CalendarStaticRaidReset(MapId.Karazhan, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.TheObsidianSanctum, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.CoilfangSerpentshrineCavern, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.Naxxramas, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.TheEyeOfEternity, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.TheBattleForMountHyjal, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.GruulsLair, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.ZulAman, 259200U, 0U),
            new CalendarStaticRaidReset(MapId.OnyxiasLair, 432000U, 0U),
            new CalendarStaticRaidReset(MapId.MoltenCore, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.BlackwingLair, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.Ulduar, 604800U, 0U),
            new CalendarStaticRaidReset(MapId.TheSunwell, 604800U, 0U)
        };

        private MapId m_mapId;
        private uint m_resetInseconds;
        private uint m_negativeOffset;

        public CalendarStaticRaidReset(MapId mapId, uint seconds, uint offset)
        {
            this.m_mapId = mapId;
            this.m_resetInseconds = seconds;
            this.m_negativeOffset = offset;
        }
    }
}