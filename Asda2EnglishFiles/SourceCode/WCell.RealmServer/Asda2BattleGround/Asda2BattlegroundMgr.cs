using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Variables;

namespace WCell.RealmServer.Asda2BattleGround
{
    public static class Asda2BattlegroundMgr
    {
        public static int MinimumPlayersToStartWar = 1;
        [NotVariable] public static int[] TotalWars = new int[4];
        [NotVariable] public static int[] LightWins = new int[4];
        [NotVariable] public static int[] DarkWins = new int[4];

        [NotVariable] public static Dictionary<Asda2BattlegroundTown, List<Asda2Battleground>> AllBattleGrounds =
            new Dictionary<Asda2BattlegroundTown, List<Asda2Battleground>>();

        public static byte OccupationDurationMins = 30;
        public static byte DeathMatchDurationMins = 30;
        public static byte BeetweenWarsMinutes = 40;
        public static byte WeekendDeathMatchHour = 10;
        public static byte EveryDayDeathMatchHour = 13;
        public static byte WeekendOcupationHour = 16;
        public static byte EveryDayOcupationHour = 19;

        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth, "Asda2 battleground system.")]
        public static void InitBattlegrounds()
        {
            Asda2BattlegroundMgr.AllBattleGrounds.Add(Asda2BattlegroundTown.Alpia, new List<Asda2Battleground>());
            Asda2BattlegroundMgr.AllBattleGrounds.Add(Asda2BattlegroundTown.Silaris, new List<Asda2Battleground>());
            Asda2BattlegroundMgr.AllBattleGrounds.Add(Asda2BattlegroundTown.Flamio, new List<Asda2Battleground>());
            Asda2BattlegroundMgr.AllBattleGrounds.Add(Asda2BattlegroundTown.Aquaton, new List<Asda2Battleground>());
            Asda2BattlegroundMgr.AddBattleGround(Asda2BattlegroundTown.Alpia);
            Asda2BattlegroundMgr.AddBattleGround(Asda2BattlegroundTown.Silaris);
            Asda2BattlegroundMgr.AddBattleGround(Asda2BattlegroundTown.Flamio);
            Asda2BattlegroundMgr.AddBattleGround(Asda2BattlegroundTown.Aquaton);
            foreach (BattlegroundResultRecord battlegroundResultRecord in ActiveRecordBase<BattlegroundResultRecord>
                .FindAll())
                Asda2BattlegroundMgr.ProcessBattlegroundResultRecord(battlegroundResultRecord);
        }

        public static void ProcessBattlegroundResultRecord(BattlegroundResultRecord battlegroundResultRecord)
        {
            int town = (int) battlegroundResultRecord.Town;
            ++Asda2BattlegroundMgr.TotalWars[town];
            if (!battlegroundResultRecord.IsLightWins.HasValue)
                return;
            if (battlegroundResultRecord.IsLightWins.Value)
                ++Asda2BattlegroundMgr.LightWins[town];
            else
                ++Asda2BattlegroundMgr.DarkWins[town];
        }

        public static void AddBattleGround(Asda2BattlegroundTown town)
        {
            Asda2Battleground asda2Battleground = new Asda2Battleground()
            {
                Town = town
            };
            switch (town)
            {
                case Asda2BattlegroundTown.Alpia:
                    asda2Battleground.MinEntryLevel = (byte) 10;
                    asda2Battleground.MaxEntryLevel = (byte) 29;
                    break;
                case Asda2BattlegroundTown.Silaris:
                    asda2Battleground.MinEntryLevel = (byte) 30;
                    asda2Battleground.MaxEntryLevel = (byte) 49;
                    break;
                case Asda2BattlegroundTown.Flamio:
                    asda2Battleground.MinEntryLevel = (byte) 50;
                    asda2Battleground.MaxEntryLevel = (byte) 69;
                    break;
                case Asda2BattlegroundTown.Aquaton:
                    asda2Battleground.MinEntryLevel = (byte) 70;
                    asda2Battleground.MaxEntryLevel = (byte) 90;
                    break;
            }

            Asda2BattlegroundType type;
            asda2Battleground.StartTime = Asda2BattlegroundMgr.GetNextWarTime(town, out type, DateTime.Now);
            asda2Battleground.WarType = type;
            asda2Battleground.EndTime = asda2Battleground.StartTime.AddMinutes(type == Asda2BattlegroundType.Occupation
                ? (double) Asda2BattlegroundMgr.OccupationDurationMins
                : (double) Asda2BattlegroundMgr.DeathMatchDurationMins);
            asda2Battleground.Points.Add(new Asda2WarPoint()
            {
                Id = (short) 0,
                X = (short) 258,
                Y = (short) 165,
                BattleGround = asda2Battleground
            });
            asda2Battleground.Points.Add(new Asda2WarPoint()
            {
                Id = (short) 1,
                X = (short) 211,
                Y = (short) 218,
                BattleGround = asda2Battleground
            });
            asda2Battleground.Points.Add(new Asda2WarPoint()
            {
                Id = (short) 2,
                X = (short) 308,
                Y = (short) 221,
                BattleGround = asda2Battleground
            });
            asda2Battleground.Points.Add(new Asda2WarPoint()
            {
                Id = (short) 3,
                X = (short) 260,
                Y = (short) 250,
                BattleGround = asda2Battleground
            });
            asda2Battleground.Points.Add(new Asda2WarPoint()
            {
                Id = (short) 4,
                X = (short) 209,
                Y = (short) 284,
                BattleGround = asda2Battleground
            });
            asda2Battleground.Points.Add(new Asda2WarPoint()
            {
                Id = (short) 5,
                X = (short) 307,
                Y = (short) 285,
                BattleGround = asda2Battleground
            });
            asda2Battleground.Points.Add(new Asda2WarPoint()
            {
                Id = (short) 6,
                X = (short) 258,
                Y = (short) 340,
                BattleGround = asda2Battleground
            });
            foreach (Asda2WarPoint point in asda2Battleground.Points)
            {
                point.OwnedFaction = (short) -1;
                World.TaskQueue.RegisterUpdatableLater((IUpdatable) point);
            }

            Asda2BattlegroundMgr.AllBattleGrounds[town].Add(asda2Battleground);
            World.TaskQueue.RegisterUpdatableLater((IUpdatable) asda2Battleground);
        }

        public static DateTime GetNextWarTime(Asda2BattlegroundTown town, out Asda2BattlegroundType type, DateTime now)
        {
            type = Asda2BattlegroundType.Deathmatch;
            switch (now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    DateTime dateTime1 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.WeekendDeathMatchHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < dateTime1)
                        return dateTime1;
                    DateTime dateTime2 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayDeathMatchHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < dateTime2)
                        return dateTime2;
                    DateTime dateTime3 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.WeekendOcupationHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < dateTime3)
                    {
                        type = Asda2BattlegroundType.Occupation;
                        return dateTime3;
                    }

                    DateTime now1 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayOcupationHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < now1)
                    {
                        type = Asda2BattlegroundType.Occupation;
                        return now1;
                    }

                    now1 = now1.AddDays(1.0).Subtract(new TimeSpan(now1.Hour, now1.Minute, now1.Millisecond))
                        .AddMinutes(1.0);
                    return Asda2BattlegroundMgr.GetNextWarTime(town, out type, now1);
                case DayOfWeek.Monday:
                    DateTime dateTime4 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayDeathMatchHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < dateTime4)
                        return dateTime4;
                    DateTime now2 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayOcupationHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < now2)
                    {
                        type = Asda2BattlegroundType.Occupation;
                        return now2;
                    }

                    now2 = now2.AddDays(1.0).Subtract(new TimeSpan(now2.Hour, now2.Minute, now2.Millisecond))
                        .AddMinutes(1.0);
                    return Asda2BattlegroundMgr.GetNextWarTime(town, out type, now2);
                case DayOfWeek.Tuesday:
                    DateTime dateTime5 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayDeathMatchHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < dateTime5)
                        return dateTime5;
                    DateTime now3 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayOcupationHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < now3)
                    {
                        type = Asda2BattlegroundType.Occupation;
                        return now3;
                    }

                    now3 = now3.AddDays(1.0).Subtract(new TimeSpan(now3.Hour, now3.Minute, now3.Millisecond))
                        .AddMinutes(1.0);
                    return Asda2BattlegroundMgr.GetNextWarTime(town, out type, now3);
                case DayOfWeek.Wednesday:
                    DateTime dateTime6 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayDeathMatchHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < dateTime6)
                        return dateTime6;
                    DateTime now4 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayOcupationHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < now4)
                    {
                        type = Asda2BattlegroundType.Occupation;
                        return now4;
                    }

                    now4 = now4.AddDays(1.0).Subtract(new TimeSpan(now4.Hour, now4.Minute, now4.Millisecond))
                        .AddMinutes(1.0);
                    return Asda2BattlegroundMgr.GetNextWarTime(town, out type, now4);
                case DayOfWeek.Thursday:
                    DateTime dateTime7 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayDeathMatchHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < dateTime7)
                        return dateTime7;
                    DateTime now5 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayOcupationHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < now5)
                    {
                        type = Asda2BattlegroundType.Occupation;
                        return now5;
                    }

                    now5 = now5.AddDays(1.0).Subtract(new TimeSpan(now5.Hour, now5.Minute, now5.Millisecond))
                        .AddMinutes(1.0);
                    return Asda2BattlegroundMgr.GetNextWarTime(town, out type, now5);
                case DayOfWeek.Friday:
                    DateTime dateTime8 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayDeathMatchHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < dateTime8)
                        return dateTime8;
                    DateTime now6 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayOcupationHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < now6)
                    {
                        type = Asda2BattlegroundType.Occupation;
                        return now6;
                    }

                    now6 = now6.AddDays(1.0).Subtract(new TimeSpan(now6.Hour, now6.Minute, now6.Millisecond))
                        .AddMinutes(1.0);
                    return Asda2BattlegroundMgr.GetNextWarTime(town, out type, now6);
                case DayOfWeek.Saturday:
                    DateTime dateTime9 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayDeathMatchHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < dateTime9)
                        return dateTime9;
                    DateTime now7 =
                        new DateTime(now.Year, now.Month, now.Day, (int) Asda2BattlegroundMgr.EveryDayOcupationHour, 0,
                            0).AddMinutes(
                            (double) ((int) (byte) town * (int) Asda2BattlegroundMgr.BeetweenWarsMinutes));
                    if (now < now7)
                    {
                        type = Asda2BattlegroundType.Occupation;
                        return now7;
                    }

                    now7 = now7.AddDays(1.0).Subtract(new TimeSpan(now7.Hour, now7.Minute, now7.Millisecond))
                        .AddMinutes(1.0);
                    return Asda2BattlegroundMgr.GetNextWarTime(town, out type, now7);
                default:
                    return DateTime.MaxValue;
            }
        }

        public static void OnCharacterLogout(Character character)
        {
            if (character.CurrentBattleGround == null)
                return;
            character.CurrentBattleGround.Leave(character);
        }
    }
}