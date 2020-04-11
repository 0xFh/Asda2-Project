using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iesi.Collections;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.Core.Timers;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Logs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Asda2BattleGround
{
    public class Asda2Battleground : IUpdatable
    {
        public readonly object JoinLock = new object();
        public WorldLocation DarkStartLocation = new WorldLocation(MapId.BatleField, new Vector3(19255, 19397));
       // public WorldLocation DarkPlacePosstion = new WorldLocation(MapId.BatleField, new Vector3(19356, 19654));
        public Dictionary<byte, Character> DarkTeam = new Dictionary<byte, Character>();

        public List<string> DissmisedCharacterNames = new List<string>();
        public List<Character> DissmissNo = new List<Character>();
        public List<Character> DissmissYes = new List<Character>();
        public List<byte> FreeDarkIds = new List<byte>();
        public List<byte> FreeLightIds = new List<byte>();
        public bool IsDismissInProgress;
        public WorldLocation LightStartLocation = new WorldLocation(MapId.BatleField, new Vector3(19254, 19107));
        public WorldLocation LightPlacePosstion = new WorldLocation(MapId.BatleField, new Vector3(19259, 19257));

        public Dictionary<byte, Character> LightTeam = new Dictionary<byte, Character>();
        public List<Asda2WarPoint> Points = new List<Asda2WarPoint>(7);

        public Asda2Battleground()
        {
            for (byte i = 0; i < 255; i++)
            {
                FreeDarkIds.Add(i);
                FreeLightIds.Add(i);
            }
        }

        public bool IsStarted { get; set; }
        public byte CurrentWarDurationMins
        {
            get { return Asda2BattlegroundMgr.WarLengthMinutes; }
            set { }
        }

        public short LightWins
        {
            get { return (short)Asda2BattlegroundMgr.LightWins[(int)Town]; }
        }

        public short LightLooses
        {
            get { return (short)Asda2BattlegroundMgr.DarkWins[(int)Town]; }
        }

        public short DarkWins
        {
            get { return (short)Asda2BattlegroundMgr.DarkWins[(int)Town]; }
        }

        public short DarkLooses
        {
            get { return (short)Asda2BattlegroundMgr.LightWins[(int)Town]; }
        }

        public Asda2BattlegroundTown Town { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int LightScores { get; set; }
        public int DarkScores { get; set; }
        public byte MinEntryLevel { get; set; }
        public byte MaxEntryLevel { get; set; }
        public Character MvpCharacter { get; set; }
        public byte WarNotofocationStep { get; set; }

        public byte AmountOfBattleGroundsInList
        {
            get { return (byte)Asda2BattlegroundMgr.AllBattleGrounds[Town].Count; }
        }

        public Asda2BattlegroundType WarType { get; set; }
        public bool IsRunning { get; set; }

        public short DissmisFaction { get; set; }
        public Character DissmissingCharacter { get; set; }
        public DateTime DissmissTimeouted { get; set; }

        public bool Join(Character chr)
        {
            lock (JoinLock)
            {
                chr.BattlegroundActPoints = 0;
                chr.BattlegroundKills = 0;
                chr.BattlegroundDeathes = 0;
                if (chr.Asda2FactionId == 0) //light
                {
                    if (FreeLightIds.Count == 0)
                        return false;
                    var id = FreeLightIds[0];
                    LightTeam.Add(id, chr);
                    FreeLightIds.RemoveAt(0);
                    chr.CurrentBattleGround = this;
                    chr.CurrentBattleGroundId = id;
                    chr.LocatonBeforeOnEnterWar = new WorldLocation(chr.Map, chr.Position);
                    if (IsRunning)
                    {
                        //TeleportToWar(chr);
                        Asda2BattlegroundHandler.SendYouCanEnterWarResponse(chr.Client);
                        //Asda2BattlegroundHandler.SendYouCanEnterWarAfterResponse(chr.Client);
                    }
                    //chr.Map.CallDelayed(1,()=> Asda2BattlegroundHandler.SendHowManyPeopleInWarTeamsResponse(this));
                    return true;
                }
                if (chr.Asda2FactionId == 1) //Dark
                {
                    if (FreeDarkIds.Count == 0)
                        return false;
                    var id = FreeDarkIds[0];
                    DarkTeam.Add(id, chr);
                    FreeDarkIds.RemoveAt(0);
                    chr.CurrentBattleGround = this;
                    chr.CurrentBattleGroundId = id;
                    chr.LocatonBeforeOnEnterWar = new WorldLocation(chr.Map, chr.Position);
                    if (IsRunning)
                    {
                        //TeleportToWar(chr);
                        Asda2BattlegroundHandler.SendYouCanEnterWarResponse(chr.Client);
                        //Asda2BattlegroundHandler.SendYouCanEnterWarAfterResponse(chr.Client);
                    }
                    //chr.Map.CallDelayed(1, () => Asda2BattlegroundHandler.SendHowManyPeopleInWarTeamsResponse(this));
                    return true;
                }
                return false;
            }
        }

        public bool Leave(Character chr)
        {
            lock (JoinLock)
            {
                chr.IsOnWar = false;
                if (chr.Asda2FactionId == 0) //light
                {
                    if (!LightTeam.ContainsValue(chr))
                        return false;
                    LightTeam.Remove(chr.CurrentBattleGroundId);
                    FreeLightIds.Add(chr.CurrentBattleGroundId);
                    chr.CurrentBattleGround = null;

                    chr.Map.CallDelayed(1, () =>
                    {
                        Asda2BattlegroundHandler.SendHowManyPeopleInWarTeamsResponse(this);
                        Asda2BattlegroundHandler.SendCharacterHasLeftWarResponse(this,
                          (int)
                            chr.AccId,
                          chr.
                            CurrentBattleGroundId,
                          chr.Name,
                          chr.
                            Asda2FactionId);
                    });
                    if (chr.MapId == MapId.BatleField)
                        chr.TeleportTo(chr.LocatonBeforeOnEnterWar);
                    if (chr.IsStunned)
                        chr.Stunned--;
                    return true;
                }
                if (chr.Asda2FactionId == 1) //Dark
                {
                    if (!DarkTeam.ContainsValue(chr))
                        return false;
                    DarkTeam.Remove(chr.CurrentBattleGroundId);
                    FreeDarkIds.Add(chr.CurrentBattleGroundId);
                    chr.CurrentBattleGround = null;
                    chr.Map.CallDelayed(1, () =>
                    {
                        Asda2BattlegroundHandler.SendHowManyPeopleInWarTeamsResponse(this);
                        Asda2BattlegroundHandler.SendCharacterHasLeftWarResponse(this,
                          (int)
                            chr.AccId,
                          chr.
                            CurrentBattleGroundId,
                          chr.Name,
                          chr.
                            Asda2FactionId);
                    });
                    if (chr.MapId == MapId.BatleField)
                        chr.TeleportTo(chr.LocatonBeforeOnEnterWar);
                    if (chr.IsStunned)
                        chr.Stunned--;
                    return true;
                }
                return false;
            }
        }

        public void Send(RealmPacketOut packet, bool addEnd = false, short? asda2FactionId = null,
          Locale locale = Locale.Any)
        {
            lock (JoinLock)
            {
                if (asda2FactionId == null)
                {
                    foreach (var chr in DarkTeam.Values)
                    {
                        if (locale == Locale.Any || chr.Client.Locale == locale)
                            chr.Send(packet, addEnd);
                    }
                    foreach (var chr in LightTeam.Values)
                    {
                        if (locale == Locale.Any || chr.Client.Locale == locale)
                            chr.Send(packet, addEnd);
                    }
                }
                else
                {
                    foreach (var chr in asda2FactionId == 0 ? LightTeam.Values : DarkTeam.Values)
                    {
                        if (locale == Locale.Any || chr.Client.Locale == locale)
                            chr.Send(packet);
                    }
                }
            }
        }

        public void TeleportToWar(Character activeCharacter)
        {
            if (activeCharacter.Asda2FactionId == 0)
                activeCharacter.TeleportTo(LightStartLocation);
            else
                activeCharacter.TeleportTo(DarkStartLocation);
        }

        public void GainScores(Character killer, short points)
        {
            GainScores(killer.Asda2FactionId, points);
        }

        public void GainScores(short factionId, short points)
        {
            if (factionId == 0)
                LightScores += points;
            else
                DarkScores += points;
            Asda2BattlegroundHandler.SendTeamPointsResponse(this);
        }

        public void SendCurrentProgress(Character character)
        {
            if (DateTime.Now < StartTime.AddMinutes(2))
                Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this, BattleGroundInfoMessageType.WarStartsInNumMins,
                  (short)(StartTime.AddMinutes(2) - DateTime.Now).TotalMinutes, character);
            else
                Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this, BattleGroundInfoMessageType.WarStarted, 0,
                  character);
        }

        public Vector3 GetBasePosition(Character activeCharacter)
        {
            return activeCharacter.Asda2FactionId == 0 ? LightStartLocation.Position : DarkStartLocation.Position;
        }

        public Vector3 GetForeigLocation(Character activeCharacter)
        {
            return activeCharacter.Asda2FactionId == 0 ? DarkStartLocation.Position : LightStartLocation.Position;

        }

        public void Possion(Character chr)
        {
            DateTime time = DateTime.Now;
            var lightpossion = World.GetNonInstancedMap(MapId.BatleField);
            var darkpossion = World.GetNonInstancedMap(MapId.BatleField);
            var vector = new Vector3(258f + lightpossion.Offset, 120f + lightpossion.Offset, 0f);
            var vector2 = new Vector3(259f + darkpossion.Offset, 382f + darkpossion.Offset, 0f);
          if (chr.Position.GetDistance(vector) < 30 && chr.IsAsda2BattlegroundInProgress && chr.Asda2FactionId == 0)
            {
               if (DateTime.Now.AddSeconds(30) < DateTime.Now)
                {
                    chr.TeleportTo(LightPlacePosstion);
                }
            }
            if (chr.Position.GetDistance(vector2) < 30 && chr.CurrentBattleGround.IsRunning && chr.Asda2FactionId == 1)
            {
                if (DateTime.Now.AddSeconds(30) < DateTime.Now)
                {
                    chr.TeleportTo(LightPlacePosstion);
                }
            }
        }

        public void AnswerDismiss(bool kick, Character answerer)
        {
            lock (this)
            {
                if (!IsDismissInProgress || answerer.Asda2FactionId != DissmisFaction || answerer == DissmissingCharacter)
                    return;
                if (kick)
                {
                    if (DissmissYes.Contains(answerer))
                        return;
                    DissmissYes.Add(answerer);
                    if (DissmissYes.Count > (DissmisFaction == 0 ? LightTeam.Count * 0.65 : DarkTeam.Count * 0.65))
                    {
                        //kick him
                        Asda2BattlegroundHandler.SendDissmissResultResponse(this, DismissPlayerResult.Ok,
                          DissmissingCharacter.SessionId,
                          (int)DissmissingCharacter.AccId);
                        Leave(DissmissingCharacter);
                        IsDismissInProgress = false;
                        DissmissingCharacter = null;
                    }
                }
                else
                {
                    if (DissmissNo.Contains(answerer))
                        return;
                    DissmissNo.Add(answerer);
                    if (DissmissNo.Count > (DissmisFaction == 0 ? LightTeam.Count * 0.3 : DarkTeam.Count * 0.3))
                    {
                        //CANcel dissmis
                        Asda2BattlegroundHandler.SendDissmissResultResponse(this, DismissPlayerResult.Fail,
                          DissmissingCharacter.SessionId,
                          (int)DissmissingCharacter.AccId);
                        IsDismissInProgress = false;
                        DissmissingCharacter = null;
                    }
                }
            }
        }

        public bool TryStartDissmisProgress(Character initer, Character dissmiser)
        {
            lock (this)
            {
                if (IsDismissInProgress)
                {
                    if (DissmissTimeouted < DateTime.Now)
                    {
                        Asda2BattlegroundHandler.SendDissmissResultResponse(this, DismissPlayerResult.Fail,
                          DissmissingCharacter.SessionId, (int)DissmissingCharacter.AccId);
                    }
                    else
                        return false;
                }
                IsDismissInProgress = true;
                Asda2BattlegroundHandler.SendQuestionDismissPlayerOrNotResponse(this, initer, dissmiser);
                DissmissingCharacter = dissmiser;
                DissmissYes.Clear();
                DissmissNo.Clear();
                DissmissTimeouted = DateTime.Now.AddMinutes(1);
                DissmisFaction = initer.Asda2FactionId;
                return true;
            }
        }

        public Character GetCharacter(short asda2FactionId, byte warId)
        {
            if (warId == 0)
            {
                return LightTeam.Count <= warId ? null : LightTeam[warId];
            }
            return DarkTeam.Count <= warId ? null : DarkTeam[warId];
        }

        #region Implementation of IUpdatable

        private int _notificationsAboutStart = 3;

        public void Update(int dt)
        {
            switch (_notificationsAboutStart)
            {
                case 3:
                    if (DateTime.Now > StartTime.Subtract(new TimeSpan(0, 30, 0)))
                    {
                        _notificationsAboutStart--;
                        World.BroadcastMsg("Õ—» «·›—ﬁ", string.Format("{1} in {0} Ì»œ√ »⁄œ 30 œﬁÌﬁ…", Town, WarType),
                          Color.Firebrick);
                        Asda2BattlegroundHandler.SendMessageServerAboutWarStartsResponse(30);
                    }
                    break;
                case 2:
                    if (DateTime.Now > StartTime.Subtract(new TimeSpan(0, 15, 0)))
                    {
                        _notificationsAboutStart--;
                        World.BroadcastMsg("Õ—» «·›—ﬁ", string.Format("{1} in {0} Ì»œ√ »⁄œ 15 œﬁÌﬁ…", Town, WarType),
                          Color.Firebrick);
                        Asda2BattlegroundHandler.SendMessageServerAboutWarStartsResponse(15);
                    }
                    break;
                case 1:
                    if (DateTime.Now > StartTime.Subtract(new TimeSpan(0, 5, 0)))
                    {
                        _notificationsAboutStart--;
                        World.BroadcastMsg("Õ—» «·›—ﬁ", string.Format("{1} in {0} Ì»œ√ »⁄œ 5 œﬁ«∆ﬁ", Town, WarType),
                          Color.Firebrick);
                        Asda2BattlegroundHandler.SendMessageServerAboutWarStartsResponse(5);
                    }
                    break;
                default:
                    break;
            }
            if (DateTime.Now > EndTime && IsRunning)
                Stop();
            else if (DateTime.Now > StartTime && DateTime.Now < EndTime)
                Start();
        }

        public int WiningFactionId
        {
            get { return LightScores == DarkScores ? 2 : LightScores > DarkScores ? 0 : 1; }
        }

        public long CurrentWarResultRecordGuid { get; set; }

        public void Stop()
        {
            if (!IsRunning)
                return;
            _notificationsAboutStart = 3;
            IsStarted = false;
            World.Broadcast(string.Format("«·Õ—» ›Ì {0} «‰ Â .. ‰ﬁ«ÿ «·‰Ê— {1} ÷œ {2} ‰ﬁ«ÿ «·Ÿ·«„", Town, LightScores,
              DarkScores));
            IsRunning = false;
            SetNextWarParametrs();
            //Notify players war ended.
            lock (JoinLock)
            {
                //find mvp
                foreach (var character in LightScores > DarkScores ? LightTeam.Values : DarkTeam.Values)
                {
                    if (MvpCharacter == null)
                    {
                        MvpCharacter = character;
                        continue;
                    }
                    if (MvpCharacter.BattlegroundActPoints < character.BattlegroundActPoints)
                        MvpCharacter = character;
                }
                Asda2BattlegroundHandler.SendWiningFactionInfoResponse(Town, WiningFactionId,
                  MvpCharacter == null ? "[·« √Õœ]" : MvpCharacter.Name);

                if (MvpCharacter != null)
                {
                    //create db records about war

                    RealmServer.IOQueue.AddMessage(() =>
                    {
                        var warResRec = new BattlegroundResultRecord(Town, MvpCharacter.Name, MvpCharacter.EntityId.Low,
                          LightScores, DarkScores);
                        warResRec.CreateLater();
                        CurrentWarResultRecordGuid = warResRec.Guid;
                        Asda2BattlegroundMgr.ProcessBattlegroundResultRecord(warResRec);
                    });
                }
                foreach (var character in LightTeam.Values)
                {
                    ProcessEndWar(character);
                    if (WiningFactionId == 0)
                    {
                        Asda2TitleChecker.OnWinWar(character);
                    }
                    else
                    {
                        Asda2TitleChecker.OnLoseWar(character);
                    }
                }
                foreach (var character in DarkTeam.Values)
                {
                    ProcessEndWar(character);
                    if (WiningFactionId == 1)
                    {
                        Asda2TitleChecker.OnWinWar(character);
                    }
                    else
                    {
                        Asda2TitleChecker.OnLoseWar(character);
                    }
                }
                foreach (var asda2WarPoint in Points)
                {
                    asda2WarPoint.Status = Asda2WarPointStatus.NotOwned;
                    asda2WarPoint.OwnedFaction = -1;

                    Asda2BattlegroundHandler.SendUpdatePointInfoResponse(null, asda2WarPoint);
                }
                World.TaskQueue.CallDelayed(60000, KickAll);
            }
        }

        private void SetNextWarParametrs()
        {
            var nextStartTimeEntry = Asda2BattlegroundMgr.GetNextStartTime(Town);
            if (nextStartTimeEntry == null)
            {
                World.BroadcastMsg("Õ—» «·›—ﬁ", "ÕœÀ Œÿ√ ›Ì ‰Ÿ«„ «·Õ—», «·—Ã«¡ ≈⁄«œ…  ‘€Ì· «·”Ì—›—.", Color.Red);
                return;
            }
            WarType = nextStartTimeEntry.Type;
            StartTime = nextStartTimeEntry.Time;
            EndTime = StartTime.AddMinutes(Asda2BattlegroundMgr.WarLengthMinutes);
        }

        private void ProcessEndWar(Character character)
        {
            character.Stunned++;
            GlobalHandler.SendFightingModeChangedResponse(character.Client, character.SessionId, (int)character.AccId, -1);
            //create db record
            if (MvpCharacter != null)
            {
                RealmServer.IOQueue.AddMessage(() =>
                {
                    var rec = new BattlegroundCharacterResultRecord(CurrentWarResultRecordGuid, character.Name,
                      character.EntityId.Low, character.BattlegroundActPoints,
                      character.BattlegroundKills,
                      character.BattlegroundDeathes);
                    rec.CreateLater();
                });
            }
            var honorPoints = WiningFactionId == 2
              ? 0
              : CharacterFormulas.CalcHonorPoints(character.Level, character.BattlegroundActPoints,
                LightScores > DarkScores,
                character.BattlegroundDeathes,
                character.BattlegroundKills, MvpCharacter == character, Town);
            var honorCoins = (short)(WiningFactionId == 2 ? 0 : (honorPoints / CharacterFormulas.HonorCoinsDivider));
            if (character.BattlegroundActPoints < 5)
                character.BattlegroundActPoints = 5;
            if (honorPoints <= 0)
                honorPoints = 1;
            if (honorCoins <= 0)
                honorCoins = 1;
            Asda2Item itemCoins = null;
            if (honorCoins > 0)
            {
                character.Asda2Inventory.TryAdd(
                  20614, honorCoins,
                  true, ref itemCoins);
                Log.Create(Log.Types.ItemOperations, LogSourceType.Character, character.EntryId)
                  .AddAttribute("source", 0, "honor_coins_for_bg")
                  .AddItemAttributes(itemCoins)
                  .AddAttribute("amount", honorCoins)
                  .Write();
            }
            var bonusExp = WiningFactionId == 2
              ? 0
              : (int)((float)XpGenerator.GetBaseExpForLevel(character.Level) * character.BattlegroundActPoints / 2.5);
            character.GainXp(bonusExp, "battle_ground");

            character.Asda2HonorPoints += honorPoints;
            Asda2BattlegroundHandler.SendWarEndedResponse(character.Client, (byte)WiningFactionId,
              LightScores > DarkScores ? LightScores : DarkScores,
              LightScores > DarkScores ? DarkScores : LightScores, honorPoints,
              honorCoins, bonusExp, MvpCharacter == null ? "" : MvpCharacter.Name);
            Asda2BattlegroundHandler.SendWarEndedOneResponse(character.Client, new List<Asda2Item> { itemCoins });
            character.SendWarMsg("”Ê› Ì „ ≈—”«·ﬂ ≈·Ï «·„œÌ‰… »⁄œ 1 œﬁÌﬁ… „‰ «·«‰.");
        }

        public void KickAll()
        {
            lock (JoinLock)
            {
                var list = new List<Character>();
                list.AddRange(LightTeam.Values);
                list.AddRange(DarkTeam.Values);
                foreach (var character in list)
                {
                    Leave(character);
                }
            }
        }

        public void Start()
        {
            if (IsRunning)
                return;
            StartTime = DateTime.Now;
            EndTime = DateTime.Now.AddMinutes(Asda2BattlegroundMgr.WarLengthMinutes);
            if (LightTeam.Count < Asda2BattlegroundMgr.MinimumPlayersToStartWar ||
                DarkTeam.Count < Asda2BattlegroundMgr.MinimumPlayersToStartWar)
            {
                World.Broadcast(string.Format(" „ ≈·€«¡ «·Õ—» »”»» ⁄œ„ ﬂ›«Ì… «··«⁄»Ì‰ ›Ì »·«œ {0}.", Town));
                SetNextWarParametrs();
                return;
            }
            World.Broadcast(string.Format("·ﬁœ »œ√  «·Õ—» ›Ì »·«œ {0}. «·„” ÊÌ«  «·„ «Õ… : {1}-{2}.", Town, MinEntryLevel, MaxEntryLevel));
            foreach (var asda2WarPoint in Points)
            {
                asda2WarPoint.Status = Asda2WarPointStatus.NotOwned;
                asda2WarPoint.OwnedFaction = -1;
            }
            DissmisedCharacterNames.Clear();
            IsRunning = true;
            LightScores = 0;
            DarkScores = 0;
            MvpCharacter = null;
            WarNotofocationStep = 0;
            //Notify character that they can login
            lock (JoinLock)
            {
                foreach (var character in LightTeam.Values)
                {
                    //TeleportToWar(character);
                    Asda2BattlegroundHandler.SendYouCanEnterWarResponse(character.Client);
                    //Asda2BattlegroundHandler.SendYouCanEnterWarAfterResponse(character.Client);
                }
                foreach (var character in DarkTeam.Values)
                {
                    //TeleportToWar(character);
                    Asda2BattlegroundHandler.SendYouCanEnterWarResponse(character.Client);
                    // Asda2BattlegroundHandler.SendYouCanEnterWarAfterResponse(character.Client);
                }
            }
            World.TaskQueue.CallDelayed(60000, SendWarTimeMotofocation);
        }

        private void SendWarTimeMotofocation()
        {
            if (!IsRunning) return;
            switch (WarNotofocationStep)
            {
                case 0:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this, BattleGroundInfoMessageType.WarStartsInNumMins,
                      1);
                    World.TaskQueue.CallDelayed(60000, SendWarTimeMotofocation);
                    break;
                case 1:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this, BattleGroundInfoMessageType.WarStarted, 0);
                    World.TaskQueue.CallDelayed(23 * 60000, SendWarTimeMotofocation);
                    IsStarted = true;
                    break;
                case 2:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this, BattleGroundInfoMessageType.WarEndsInNumMins,
                      5);
                    World.TaskQueue.CallDelayed(60000, SendWarTimeMotofocation);
                    break;
                case 3:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this, BattleGroundInfoMessageType.WarEndsInNumMins,
                      4);
                    World.TaskQueue.CallDelayed(60000, SendWarTimeMotofocation);
                    break;
                case 4:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this, BattleGroundInfoMessageType.WarEndsInNumMins,
                      3);
                    World.TaskQueue.CallDelayed(60000, SendWarTimeMotofocation);
                    break;
                case 5:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this, BattleGroundInfoMessageType.WarEndsInNumMins,
                      2);
                    World.TaskQueue.CallDelayed(60000, SendWarTimeMotofocation);
                    break;
                case 6:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this, BattleGroundInfoMessageType.WarEndsInNumMins,
                      1);
                    break;
            }
            WarNotofocationStep++;
        }

        #endregion
    }
}