using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Asda2BattleGround
{
    public static class Asda2BattlegroundMgr
    {
        public static int MinimumPlayersToStartWar = 1;

        [NotVariable]
        public static int[] TotalWars = new int[6];

        [NotVariable]
        public static int[] LightWins = new int[6];

        [NotVariable]
        public static int[] DarkWins = new int[6];

        [NotVariable]
        public static Dictionary<Asda2BattlegroundTown, List<Asda2Battleground>> AllBattleGrounds =
          new Dictionary<Asda2BattlegroundTown, List<Asda2Battleground>>();

        public static byte BeetweenWarsMinutes = 20;
        public static byte WarLengthMinutes = 15;

        private readonly static List<Asda2BattleGroundTimeEntry> TimeEntries = new List<Asda2BattleGroundTimeEntry>();

        [Initialization(InitializationPass.Tenth, "Asda2 battleground system.")]
        public static void InitBattlegrounds()
        {
            InitTimeEntries();
            AllBattleGrounds.Add(Asda2BattlegroundTown.Alpia, new List<Asda2Battleground>());
            AllBattleGrounds.Add(Asda2BattlegroundTown.Silaris, new List<Asda2Battleground>());
            AllBattleGrounds.Add(Asda2BattlegroundTown.Flamio, new List<Asda2Battleground>());
            AllBattleGrounds.Add(Asda2BattlegroundTown.Aquaton, new List<Asda2Battleground>());
            AddBattleGround(Asda2BattlegroundTown.Alpia);
            AddBattleGround(Asda2BattlegroundTown.Silaris);
            AddBattleGround(Asda2BattlegroundTown.Flamio);
            AddBattleGround(Asda2BattlegroundTown.Aquaton);
            var btgrndreslts = BattlegroundResultRecord.FindAll();
            foreach (var battlegroundResultRecord in btgrndreslts)
            {
                ProcessBattlegroundResultRecord(battlegroundResultRecord);
            }
        }

        private static void InitTimeEntries()
        {
            var now = DateTime.Now.Date;
            var endDate = DateTime.Now.AddDays(30);

            while (now < endDate)
            {
                foreach (Asda2BattlegroundType type in Enum.GetValues(typeof(Asda2BattlegroundType)))
                {
                    foreach (Asda2BattlegroundTown town in Enum.GetValues(typeof(Asda2BattlegroundTown)))
                    {
                        TimeEntries.Add(new Asda2BattleGroundTimeEntry
                        {
                            Type = type,
                            Town = town,
                            Time = now
                        });
                        now = now.AddMinutes(BeetweenWarsMinutes);
                    }
                }
            }
        }

        public static void ProcessBattlegroundResultRecord(BattlegroundResultRecord battlegroundResultRecord)
        {
            var town = (int)battlegroundResultRecord.Town;
            TotalWars[town]++;
            if (battlegroundResultRecord.IsLightWins != null)
            {
                if ((bool)battlegroundResultRecord.IsLightWins)
                    LightWins[town]++;
                else
                    DarkWins[town]++;
            }
        }

        public static void AddBattleGround(Asda2BattlegroundTown town)
        {
            var newBtgrnd = new Asda2Battleground { Town = town };
            switch (town)
            {
                case Asda2BattlegroundTown.Alpia:
                    newBtgrnd.MinEntryLevel = 10;
                    newBtgrnd.MaxEntryLevel = 29;
                    break;
                case Asda2BattlegroundTown.Silaris:
                    newBtgrnd.MinEntryLevel = 30;
                    newBtgrnd.MaxEntryLevel = 49;
                    break;
                case Asda2BattlegroundTown.Flamio:
                    newBtgrnd.MinEntryLevel = 10;
                    newBtgrnd.MaxEntryLevel = 250;
                    break;
                case Asda2BattlegroundTown.Aquaton:
                    newBtgrnd.MinEntryLevel = 10;
                    newBtgrnd.MaxEntryLevel = 250;
                    break;
            }
            var nextStartTimeEntry = GetNextStartTime(town);
            if (nextStartTimeEntry == null)
            {
                World.BroadcastMsg("WarManager", "War system broken, restart server!", Color.Red);
                return;
            }

            newBtgrnd.StartTime = nextStartTimeEntry.Time;
            newBtgrnd.WarType = nextStartTimeEntry.Type;
            newBtgrnd.EndTime = newBtgrnd.StartTime.AddMinutes(WarLengthMinutes);

            newBtgrnd.Points.Add(new Asda2WarPoint() { Id = 0, X = 258, Y = 165, BattleGround = newBtgrnd });
            newBtgrnd.Points.Add(new Asda2WarPoint() { Id = 1, X = 211, Y = 218, BattleGround = newBtgrnd });
            newBtgrnd.Points.Add(new Asda2WarPoint() { Id = 2, X = 308, Y = 221, BattleGround = newBtgrnd });
            newBtgrnd.Points.Add(new Asda2WarPoint() { Id = 3, X = 260, Y = 250, BattleGround = newBtgrnd });
            newBtgrnd.Points.Add(new Asda2WarPoint() { Id = 4, X = 209, Y = 284, BattleGround = newBtgrnd });
            newBtgrnd.Points.Add(new Asda2WarPoint() { Id = 5, X = 307, Y = 285, BattleGround = newBtgrnd });
            newBtgrnd.Points.Add(new Asda2WarPoint() { Id = 6, X = 258, Y = 340, BattleGround = newBtgrnd });

            foreach (var asda2WarPoint in newBtgrnd.Points)
            {
                asda2WarPoint.OwnedFaction = -1;
                World.TaskQueue.RegisterUpdatableLater(asda2WarPoint);
            }
            AllBattleGrounds[town].Add(newBtgrnd);
            World.TaskQueue.RegisterUpdatableLater(newBtgrnd);
        }

        public static Asda2BattleGroundTimeEntry GetNextStartTime(Asda2BattlegroundTown town)
        {
            var now = DateTime.Now;

            var entry = TimeEntries.FirstOrDefault(x => x.Time > now && x.Town == town);

            return entry;
        }

        public static void OnCharacterLogout(Character character)
        {
            if (character.CurrentBattleGround != null)
            {
                character.CurrentBattleGround.Leave(character);
            }
        }
    }

    public class Asda2BattleGroundTimeEntry
    {
        public Asda2BattlegroundType Type { get; set; }
        public Asda2BattlegroundTown Town { get; set; }
        public DateTime Time { get; set; }
    }

    [ActiveRecord("BattlegroundResultRecord", Access = PropertyAccess.Property)]
    public class BattlegroundResultRecord : WCellRecord<BattlegroundResultRecord>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(BattlegroundResultRecord), "Guid");

        public BattlegroundResultRecord()
        {
        }

        public BattlegroundResultRecord(Asda2BattlegroundTown town, string mvpCharacterName, uint mvpCharacterGuid,
          int lightScores, int darkScores)
        {
            Town = town;
            MvpCharacterName = mvpCharacterName;
            MvpCharacterGuid = mvpCharacterGuid;
            LightScores = lightScores;
            DarkScores = darkScores;
            Guid = _idGenerator.Next();
        }

        [Property]
        public Asda2BattlegroundTown Town { get; set; }

        [Property]
        public string MvpCharacterName { get; set; }

        [Property]
        public uint MvpCharacterGuid { get; set; }

        [Property]
        public int LightScores { get; set; }

        [Property]
        public int DarkScores { get; set; }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        public bool? IsLightWins
        {
            get
            {
                if (LightScores == DarkScores) return null;
                return LightScores > DarkScores;
            }
        }
    }

    [ActiveRecord("BattlegroundCharacterResultRecord", Access = PropertyAccess.Property)]
    public class BattlegroundCharacterResultRecord : WCellRecord<BattlegroundCharacterResultRecord>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(BattlegroundCharacterResultRecord),
          "Guid");

        public BattlegroundCharacterResultRecord()
        {
        }

        public BattlegroundCharacterResultRecord(long warGuid, string characterName, uint characterGuid, int actScores,
          int kills, int deathes)
        {
            WarGuid = warGuid;
            CharacterName = characterName;
            CharacterGuid = characterGuid;
            ActScores = actScores;
            Kills = kills;
            Deathes = deathes;
            Guid = _idGenerator.Next();
        }

        [Property]
        public long WarGuid { get; set; }

        [Property]
        public string CharacterName { get; set; }

        [Property]
        public uint CharacterGuid { get; set; }

        [Property]
        public int ActScores { get; set; }

        [Property]
        public int Kills { get; set; }

        [Property]
        public int Deathes { get; set; }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }
    }

    public class Asda2WarPoint : IUpdatable
    {
        private bool _isCapturing;
        private int _timeToNextGainPoints = CharacterFormulas.DefaultTimeGainExpReward;
        private int _timeToStartCapturing = CharacterFormulas.DefaultTimeToStartCapture;

        private int _tomeToCaprute = CharacterFormulas.DefaultCaptureTime;
        public Character CapturingCharacter { get; set; }
        public short Id { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short OwnedFaction { get; set; }
        public Asda2WarPointStatus Status { get; set; }
        public Asda2Battleground BattleGround { get; set; }

        public void Update(int dt)
        {
            if (Status == Asda2WarPointStatus.Owned)
            {
                //gain scores each one minute to team
                _timeToNextGainPoints -= dt;
                if (_timeToNextGainPoints <= 0)
                {
                    BattleGround.GainScores(OwnedFaction, CharacterFormulas.FactionWarPointsPerTicForCapturedPoints);
                    _timeToNextGainPoints += CharacterFormulas.DefaultTimeGainExpReward;
                }
            }
            if (_isCapturing)
            {
                _tomeToCaprute -= dt;
                if (_tomeToCaprute <= 0)
                {
                    //point captured
                    Status = Asda2WarPointStatus.Owned;
                    Asda2BattlegroundHandler.SendUpdatePointInfoResponse(null, this);
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(BattleGround,
                      BattleGroundInfoMessageType.SuccessToCompletelyOccuptyTheNumOccupationPoints, Id, null, OwnedFaction);
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(BattleGround,
                      BattleGroundInfoMessageType.TheOtherSideHasTemporarilyOccupiedTheNumOccupationPoint, Id, null,
                      (short?)(OwnedFaction == 0 ? 1 : 0));
                    BattleGround.GainScores(OwnedFaction, CharacterFormulas.FactionWarPointsPerTicForCapturedPoints);
                    _isCapturing = false;
                }
            }
            else
            {
                if (CapturingCharacter == null || !BattleGround.IsStarted)
                    return;
                _timeToStartCapturing -= dt;
                if (_timeToStartCapturing <= 0)
                {
                    _tomeToCaprute = CharacterFormulas.DefaultCaptureTime;
                    _isCapturing = true;
                    OwnedFaction = CapturingCharacter.Asda2FactionId;
                    CapturingCharacter.GainActPoints(1);
                    BattleGround.GainScores(CapturingCharacter, 1);
                    Status = Asda2WarPointStatus.Capturing;
                    Asda2BattlegroundHandler.SendUpdatePointInfoResponse(null, this);
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(BattleGround,
                      BattleGroundInfoMessageType.SuccessToTemporarilyOccuptyTheNumOccupationPoints, Id, null,
                      CapturingCharacter.Asda2FactionId);
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(BattleGround,
                      BattleGroundInfoMessageType.TheOtherSideHasTemporarilyOccupiedTheNumOccupationPoint, Id, null,
                      (short?)(CapturingCharacter.Asda2FactionId == 1 ? 0 : 1));
                    Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(CapturingCharacter.Client, Id,
                      OcupationPointStartedStatus.Fail);
                    CapturingCharacter.CurrentCapturingPoint = null;

                    CapturingCharacter = null;
                    _timeToNextGainPoints = CharacterFormulas.DefaultTimeGainExpReward;
                }
            }
        }

        //todo disable on move\take dmg\stun
        public void TryCapture(Character activeCharacter)
        {
            lock (this)
            {
                if (CapturingCharacter != null)
                {
                    activeCharacter.SendWarMsg(string.Format("Point {0} already capturing by {1}.", Id + 1,
                      CapturingCharacter.Name));
                    Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(activeCharacter.Client, Id,
                      OcupationPointStartedStatus.Fail);
                    return;
                }
                if (activeCharacter.Asda2Position.GetDistance(new Vector3(X, Y)) > 7)
                {
                    activeCharacter.SendWarMsg(string.Format("Distance to {0} is too big.", Id + 1));
                    Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(activeCharacter.Client, Id,
                      OcupationPointStartedStatus.Fail);
                    return;
                }
                if (Status != Asda2WarPointStatus.NotOwned && OwnedFaction == activeCharacter.Asda2FactionId)
                {
                    Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(activeCharacter.Client, Id,
                      OcupationPointStartedStatus.YouAreOcupaingTheSameSide);
                    return;
                }
                CapturingCharacter = activeCharacter;
                activeCharacter.CurrentCapturingPoint = this;
                CapturingCharacter.IsMoving = false;
                CapturingCharacter.IsFighting = false;
                Asda2MovmentHandler.SendEndMoveByFastInstantRegularMoveResponse(CapturingCharacter);
                _isCapturing = false;
                _timeToStartCapturing = CharacterFormulas.DefaultTimeToStartCapture;
                Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(activeCharacter.Client, Id,
                  OcupationPointStartedStatus.Ok);
            }
        }

        public void StopCapture()
        {
            Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(BattleGround,
              BattleGroundInfoMessageType.FailedToTemporarilyOccuptyTheNumOccupationPoints, Id, null,
              CapturingCharacter.Asda2FactionId);
            Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(CapturingCharacter.Client, Id,
              OcupationPointStartedStatus.Fail);
            CapturingCharacter.CurrentCapturingPoint = null;
            CapturingCharacter = null;
            _isCapturing = false;
        }
    }

    public enum OcupationPointStartedStatus
    {
        Fail = 0,
        Ok = 1,
        YouAreOcupaingTheSameSide = 3
    }

    public enum Asda2WarPointStatus : short
    {
        NotOwned = 0,
        Capturing = 1,
        Owned = 2
    }

    public enum Asda2BattlegroundType
    {
        Occupation = 0,
        Deathmatch = 1
    }

    public enum Asda2BattlegroundWarCanceledReason
    {
        CurrentWaitingListHasBeenDeleted = 1,
        BattleFieldHasBeenClosed = 2,
        WarCanceledDueLowPlayers = 3
    }

    public enum Asda2BattlegroundTown
    {
        Alpia = 0,
        Silaris = 1,
        Flamio = 2,
        Aquaton = 3,
        End
    }

    public enum BattleGroundInfoMessageType
    {
        FailedToTemporarilyOccuptyTheNumOccupationPoints = 0,
        SuccessToTemporarilyOccuptyTheNumOccupationPoints = 1,
        CanceledToTemporarilyOccuptyTheNumOccupationPoints = 2,
        FailedToCompletelyOccuptyTheNumOccupationPoints = 3,
        SuccessToCompletelyOccuptyTheNumOccupationPoints = 4,
        CanceledToCompletelyOccuptyTheNumOccupationPoints = 5,
        TheOtherSideHasTemporarilyOccupiedTheNumOccupationPoint = 6,
        WarStartsInNumMins = 7,
        WarStarted = 8,
        WarEndsInNumMins = 9,
        PreWarCircle = 10,
        DarkWillReciveBuffs = 11,
        DarkBuffsHasBeedRemoved = 12
    }

    public enum RegisterToBattlegroundStatus
    {
        Fail = 0,
        Ok = 1,
        YouRegisterAsFactionWarCandidat = 2,
        YouMustCHangeYourJobTwiceToEnterWar = 3,
        BattleGroupInfoIsInvalid = 4,
        YouHaveAlreadyRegistered = 5,
        YouCanJoinTheFActionWarOnlyOncePerDay = 6,
        GamesInfoStrange = 8,
        YouCantEnterCauseYouHaveBeenDissmised = 9,
        WrongLevel = 10,
        WarHasBeenCanceledCauseLowPlayers = 11
    }
}