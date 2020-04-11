using System;
using System.Collections.Generic;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.Core.Timers;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Network;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Asda2BattleGround
{
    public class Asda2Battleground : IUpdatable
    {
        public List<Asda2WarPoint> Points = new List<Asda2WarPoint>(7);
        public WorldLocation LightStartLocation = new WorldLocation(MapId.BatleField, new Vector3(19254f, 19107f), 1U);
        public WorldLocation DarkStartLocation = new WorldLocation(MapId.BatleField, new Vector3(19255f, 19397f), 1U);
        public Dictionary<byte, Character> LightTeam = new Dictionary<byte, Character>();
        public Dictionary<byte, Character> DarkTeam = new Dictionary<byte, Character>();
        public List<byte> FreeLightIds = new List<byte>();
        public List<byte> FreeDarkIds = new List<byte>();
        public List<string> DissmisedCharacterNames = new List<string>();
        public readonly object JoinLock = new object();
        private int _notificationsAboutStart = 3;
        public List<Character> DissmissYes = new List<Character>();
        public List<Character> DissmissNo = new List<Character>();
        public bool IsDismissInProgress;

        public bool IsStarted { get; set; }

        public byte CurrentWarDurationMins
        {
            get
            {
                if (this.WarType != Asda2BattlegroundType.Occupation)
                    return Asda2BattlegroundMgr.DeathMatchDurationMins;
                return Asda2BattlegroundMgr.OccupationDurationMins;
            }
        }

        public short LightWins
        {
            get { return (short) Asda2BattlegroundMgr.LightWins[(int) this.Town]; }
        }

        public short LightLooses
        {
            get { return (short) Asda2BattlegroundMgr.DarkWins[(int) this.Town]; }
        }

        public short DarkWins
        {
            get { return (short) Asda2BattlegroundMgr.DarkWins[(int) this.Town]; }
        }

        public short DarkLooses
        {
            get { return (short) Asda2BattlegroundMgr.LightWins[(int) this.Town]; }
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
            get { return (byte) Asda2BattlegroundMgr.AllBattleGrounds[this.Town].Count; }
        }

        public Asda2BattlegroundType WarType { get; set; }

        public bool IsRunning { get; set; }

        public Asda2Battleground()
        {
            for (byte index = 0; index < byte.MaxValue; ++index)
            {
                this.FreeDarkIds.Add(index);
                this.FreeLightIds.Add(index);
            }
        }

        public bool Join(Character chr)
        {
            lock (this.JoinLock)
            {
                chr.BattlegroundActPoints = (short) 0;
                chr.BattlegroundKills = 0;
                chr.BattlegroundDeathes = 0;
                if (chr.Asda2FactionId == (short) 0)
                {
                    if (this.FreeLightIds.Count == 0)
                        return false;
                    byte freeLightId = this.FreeLightIds[0];
                    this.LightTeam.Add(freeLightId, chr);
                    this.FreeLightIds.RemoveAt(0);
                    chr.CurrentBattleGround = this;
                    chr.CurrentBattleGroundId = freeLightId;
                    chr.LocatonBeforeOnEnterWar = new WorldLocation(chr.Map, chr.Position, 1U);
                    if (this.IsRunning)
                        Asda2BattlegroundHandler.SendYouCanEnterWarResponse(chr.Client);
                    return true;
                }

                if (chr.Asda2FactionId != (short) 1 || this.FreeDarkIds.Count == 0)
                    return false;
                byte freeDarkId = this.FreeDarkIds[0];
                this.DarkTeam.Add(freeDarkId, chr);
                this.FreeDarkIds.RemoveAt(0);
                chr.CurrentBattleGround = this;
                chr.CurrentBattleGroundId = freeDarkId;
                chr.LocatonBeforeOnEnterWar = new WorldLocation(chr.Map, chr.Position, 1U);
                if (this.IsRunning)
                    Asda2BattlegroundHandler.SendYouCanEnterWarResponse(chr.Client);
                return true;
            }
        }

        public bool Leave(Character chr)
        {
            lock (this.JoinLock)
            {
                if (chr.Asda2FactionId == (short) 0)
                {
                    if (!this.LightTeam.ContainsValue(chr))
                        return false;
                    this.LightTeam.Remove(chr.CurrentBattleGroundId);
                    this.FreeLightIds.Add(chr.CurrentBattleGroundId);
                    chr.CurrentBattleGround = (Asda2Battleground) null;
                    chr.Map.CallDelayed(1, (Action) (() =>
                    {
                        Asda2BattlegroundHandler.SendHowManyPeopleInWarTeamsResponse(this, (Character) null);
                        Asda2BattlegroundHandler.SendCharacterHasLeftWarResponse(this, (int) chr.AccId,
                            chr.CurrentBattleGroundId, chr.Name, (int) chr.Asda2FactionId);
                    }));
                    if (chr.MapId == MapId.BatleField)
                        chr.TeleportTo((IWorldLocation) chr.LocatonBeforeOnEnterWar);
                    if (chr.IsStunned)
                        --chr.Stunned;
                    return true;
                }

                if (chr.Asda2FactionId != (short) 1 || !this.DarkTeam.ContainsValue(chr))
                    return false;
                this.DarkTeam.Remove(chr.CurrentBattleGroundId);
                this.FreeDarkIds.Add(chr.CurrentBattleGroundId);
                chr.CurrentBattleGround = (Asda2Battleground) null;
                chr.Map.CallDelayed(1, (Action) (() =>
                {
                    Asda2BattlegroundHandler.SendHowManyPeopleInWarTeamsResponse(this, (Character) null);
                    Asda2BattlegroundHandler.SendCharacterHasLeftWarResponse(this, (int) chr.AccId,
                        chr.CurrentBattleGroundId, chr.Name, (int) chr.Asda2FactionId);
                }));
                if (chr.MapId == MapId.BatleField)
                    chr.TeleportTo((IWorldLocation) chr.LocatonBeforeOnEnterWar);
                if (chr.IsStunned)
                    --chr.Stunned;
                return true;
            }
        }

        public void Update(int dt)
        {
            switch (this._notificationsAboutStart)
            {
                case 1:
                    if (DateTime.Now > this.StartTime.Subtract(new TimeSpan(0, 5, 0)))
                    {
                        --this._notificationsAboutStart;
                        WCell.RealmServer.Global.World.BroadcastMsg("War Manager",
                            string.Format("{1} in {0} starts in 5 mins.", (object) this.Town, (object) this.WarType),
                            Color.Firebrick);
                        Asda2BattlegroundHandler.SendMessageServerAboutWarStartsResponse((byte) 5);
                        break;
                    }

                    break;
                case 2:
                    if (DateTime.Now > this.StartTime.Subtract(new TimeSpan(0, 15, 0)))
                    {
                        --this._notificationsAboutStart;
                        WCell.RealmServer.Global.World.BroadcastMsg("War Manager",
                            string.Format("{1} in {0} starts in 15 mins.", (object) this.Town, (object) this.WarType),
                            Color.Firebrick);
                        Asda2BattlegroundHandler.SendMessageServerAboutWarStartsResponse((byte) 15);
                        break;
                    }

                    break;
                case 3:
                    if (DateTime.Now > this.StartTime.Subtract(new TimeSpan(0, 30, 0)))
                    {
                        --this._notificationsAboutStart;
                        WCell.RealmServer.Global.World.BroadcastMsg("War Manager",
                            string.Format("{1} in {0} starts in 30 mins.", (object) this.Town, (object) this.WarType),
                            Color.Firebrick);
                        Asda2BattlegroundHandler.SendMessageServerAboutWarStartsResponse((byte) 30);
                        break;
                    }

                    break;
            }

            if (DateTime.Now > this.EndTime && this.IsRunning)
            {
                this.Stop();
            }
            else
            {
                if (!(DateTime.Now > this.StartTime) || !(DateTime.Now < this.EndTime))
                    return;
                this.Start();
            }
        }

        public int WiningFactionId
        {
            get
            {
                if (this.LightScores == this.DarkScores)
                    return 2;
                return this.LightScores <= this.DarkScores ? 1 : 0;
            }
        }

        public long CurrentWarResultRecordGuid { get; set; }

        public void Stop()
        {
            if (!this.IsRunning)
                return;
            this._notificationsAboutStart = 3;
            this.IsStarted = false;
            WCell.RealmServer.Global.World.Broadcast(string.Format(
                "War in {0} has ended. Light scores {1} vs {2} dark scores.", (object) this.Town,
                (object) this.LightScores, (object) this.DarkScores));
            this.IsRunning = false;
            this.SetNextWarParametrs();
            lock (this.JoinLock)
            {
                foreach (Character character in this.LightScores > this.DarkScores
                    ? this.LightTeam.Values
                    : this.DarkTeam.Values)
                {
                    if (this.MvpCharacter == null)
                        this.MvpCharacter = character;
                    else if ((int) this.MvpCharacter.BattlegroundActPoints < (int) character.BattlegroundActPoints)
                        this.MvpCharacter = character;
                }

                Asda2BattlegroundHandler.SendWiningFactionInfoResponse(this.Town, this.WiningFactionId,
                    this.MvpCharacter == null ? "[No character]" : this.MvpCharacter.Name);
                if (this.MvpCharacter != null)
                    ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                    {
                        BattlegroundResultRecord battlegroundResultRecord = new BattlegroundResultRecord(this.Town,
                            this.MvpCharacter.Name, this.MvpCharacter.EntityId.Low, this.LightScores, this.DarkScores);
                        battlegroundResultRecord.CreateLater();
                        this.CurrentWarResultRecordGuid = battlegroundResultRecord.Guid;
                        Asda2BattlegroundMgr.ProcessBattlegroundResultRecord(battlegroundResultRecord);
                    }));
                foreach (Character character in this.LightTeam.Values)
                    this.ProcessEndWar(character);
                foreach (Character character in this.DarkTeam.Values)
                    this.ProcessEndWar(character);
                foreach (Asda2WarPoint point in this.Points)
                {
                    point.Status = Asda2WarPointStatus.NotOwned;
                    point.OwnedFaction = (short) -1;
                    Asda2BattlegroundHandler.SendUpdatePointInfoResponse((IRealmClient) null, point);
                }

                WCell.RealmServer.Global.World.TaskQueue.CallDelayed(60000, new Action(this.KickAll));
            }
        }

        private void SetNextWarParametrs()
        {
            Asda2BattlegroundType type;
            this.StartTime = Asda2BattlegroundMgr.GetNextWarTime(this.Town, out type, DateTime.Now);
            this.WarType = type;
            this.EndTime = this.StartTime.AddMinutes(this.WarType == Asda2BattlegroundType.Occupation
                ? (double) Asda2BattlegroundMgr.OccupationDurationMins
                : (double) Asda2BattlegroundMgr.DeathMatchDurationMins);
        }

        private void ProcessEndWar(Character character)
        {
            ++character.Stunned;
            GlobalHandler.SendFightingModeChangedResponse(character.Client, character.SessionId, (int) character.AccId,
                (short) -1);
            if (this.MvpCharacter != null)
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                    new BattlegroundCharacterResultRecord(this.CurrentWarResultRecordGuid, character.Name,
                        character.EntityId.Low, (int) character.BattlegroundActPoints, character.BattlegroundKills,
                        character.BattlegroundDeathes).CreateLater()));
            int honorPoints = this.WiningFactionId == 2
                ? 0
                : CharacterFormulas.CalcHonorPoints(character.Level, character.BattlegroundActPoints,
                    this.LightScores > this.DarkScores, character.BattlegroundDeathes, character.BattlegroundKills,
                    this.MvpCharacter == character, this.Town);
            short honorCoins = this.WiningFactionId == 2
                ? (short) 0
                : (short) ((double) honorPoints / (double) CharacterFormulas.HonorCoinsDivider);
            if (character.BattlegroundActPoints < (short) 5)
                character.BattlegroundActPoints = (short) 5;
            if (honorPoints <= 0)
                honorPoints = 1;
            if (honorCoins <= (short) 0)
                honorCoins = (short) 1;
            Asda2Item asda2Item = (Asda2Item) null;
            if (honorCoins > (short) 0)
            {
                int num = (int) character.Asda2Inventory.TryAdd(20614, (int) honorCoins, true, ref asda2Item,
                    new Asda2InventoryType?(), (Asda2Item) null);
                Log.Create(Log.Types.ItemOperations, LogSourceType.Character, character.EntryId)
                    .AddAttribute("source", 0.0, "honor_coins_for_bg").AddItemAttributes(asda2Item, "")
                    .AddAttribute("amount", (double) honorCoins, "").Write();
            }

            int bonusExp = this.WiningFactionId == 2
                ? 0
                : (int) ((double) XpGenerator.GetBaseExpForLevel(character.Level) *
                         (double) character.BattlegroundActPoints / 2.5);
            character.GainXp(bonusExp, "battle_ground", false);
            character.Asda2HonorPoints += honorPoints;
            AchievementProgressRecord progressRecord = character.Achievements.GetOrCreateProgressRecord(20U);
            if (character.FactionId == (FactionId) this.WiningFactionId)
            {
                switch (++progressRecord.Counter)
                {
                    case 5:
                        character.DiscoverTitle(Asda2TitleId.Challenger125);
                        break;
                    case 10:
                        character.GetTitle(Asda2TitleId.Challenger125);
                        break;
                    case 25:
                        character.DiscoverTitle(Asda2TitleId.Winner126);
                        break;
                    case 50:
                        character.GetTitle(Asda2TitleId.Winner126);
                        break;
                    case 75:
                        character.DiscoverTitle(Asda2TitleId.Champion127);
                        break;
                    case 100:
                        character.GetTitle(Asda2TitleId.Champion127);
                        break;
                    case 250:
                        character.DiscoverTitle(Asda2TitleId.Conqueror128);
                        break;
                    case 500:
                        character.GetTitle(Asda2TitleId.Conqueror128);
                        break;
                }

                progressRecord.SaveAndFlush();
            }

            character.Resurrect();
            character.Map.CallDelayed(500,
                (Action) (() => Asda2BattlegroundHandler.SendWarEndedResponse(character.Client,
                    (byte) this.WiningFactionId,
                    this.LightScores > this.DarkScores ? this.LightScores : this.DarkScores,
                    this.LightScores > this.DarkScores ? this.DarkScores : this.LightScores, honorPoints, honorCoins,
                    (long) bonusExp, this.MvpCharacter == null ? "" : this.MvpCharacter.Name)));
            Asda2BattlegroundHandler.SendWarEndedOneResponse(character.Client,
                (IEnumerable<Asda2Item>) new List<Asda2Item>()
                {
                    asda2Item
                });
            character.SendWarMsg("You will automaticly teleported to town in 1 minute.");
        }

        public void KickAll()
        {
            lock (this.JoinLock)
            {
                List<Character> characterList = new List<Character>();
                characterList.AddRange((IEnumerable<Character>) this.LightTeam.Values);
                characterList.AddRange((IEnumerable<Character>) this.DarkTeam.Values);
                foreach (Character chr in characterList)
                    this.Leave(chr);
            }
        }

        public void Start()
        {
            if (this.IsRunning)
                return;
            this.StartTime = DateTime.Now;
            this.EndTime = DateTime.Now.AddMinutes(this.WarType == Asda2BattlegroundType.Occupation
                ? (double) Asda2BattlegroundMgr.EveryDayDeathMatchHour
                : (double) Asda2BattlegroundMgr.DeathMatchDurationMins);
            if (this.LightTeam.Count < Asda2BattlegroundMgr.MinimumPlayersToStartWar ||
                this.DarkTeam.Count < Asda2BattlegroundMgr.MinimumPlayersToStartWar)
            {
                WCell.RealmServer.Global.World.Broadcast(string.Format("War terminated due not enough players in {0}.",
                    (object) this.Town));
                this.SetNextWarParametrs();
            }
            else
            {
                WCell.RealmServer.Global.World.Broadcast(string.Format("War started in {0}. Availible lvls {1}-{2}.",
                    (object) this.Town, (object) this.MinEntryLevel, (object) this.MaxEntryLevel));
                foreach (Asda2WarPoint point in this.Points)
                {
                    point.Status = Asda2WarPointStatus.NotOwned;
                    point.OwnedFaction = (short) -1;
                }

                this.DissmisedCharacterNames.Clear();
                this.IsRunning = true;
                this.LightScores = 0;
                this.DarkScores = 0;
                this.MvpCharacter = (Character) null;
                this.WarNotofocationStep = (byte) 0;
                lock (this.JoinLock)
                {
                    foreach (Character character in this.LightTeam.Values)
                        Asda2BattlegroundHandler.SendYouCanEnterWarResponse(character.Client);
                    foreach (Character character in this.DarkTeam.Values)
                        Asda2BattlegroundHandler.SendYouCanEnterWarResponse(character.Client);
                }

                WCell.RealmServer.Global.World.TaskQueue.CallDelayed(60000, new Action(this.SendWarTimeMotofocation));
            }
        }

        private void SendWarTimeMotofocation()
        {
            if (!this.IsRunning)
                return;
            switch (this.WarNotofocationStep)
            {
                case 0:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this,
                        BattleGroundInfoMessageType.WarStartsInNumMins, (short) 1, (Character) null, new short?());
                    WCell.RealmServer.Global.World.TaskQueue.CallDelayed(60000,
                        new Action(this.SendWarTimeMotofocation));
                    break;
                case 1:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this,
                        BattleGroundInfoMessageType.WarStarted, (short) 0, (Character) null, new short?());
                    WCell.RealmServer.Global.World.TaskQueue.CallDelayed(1380000,
                        new Action(this.SendWarTimeMotofocation));
                    this.IsStarted = true;
                    break;
                case 2:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this,
                        BattleGroundInfoMessageType.WarEndsInNumMins, (short) 5, (Character) null, new short?());
                    WCell.RealmServer.Global.World.TaskQueue.CallDelayed(60000,
                        new Action(this.SendWarTimeMotofocation));
                    break;
                case 3:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this,
                        BattleGroundInfoMessageType.WarEndsInNumMins, (short) 4, (Character) null, new short?());
                    WCell.RealmServer.Global.World.TaskQueue.CallDelayed(60000,
                        new Action(this.SendWarTimeMotofocation));
                    break;
                case 4:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this,
                        BattleGroundInfoMessageType.WarEndsInNumMins, (short) 3, (Character) null, new short?());
                    WCell.RealmServer.Global.World.TaskQueue.CallDelayed(60000,
                        new Action(this.SendWarTimeMotofocation));
                    break;
                case 5:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this,
                        BattleGroundInfoMessageType.WarEndsInNumMins, (short) 2, (Character) null, new short?());
                    WCell.RealmServer.Global.World.TaskQueue.CallDelayed(60000,
                        new Action(this.SendWarTimeMotofocation));
                    break;
                case 6:
                    Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this,
                        BattleGroundInfoMessageType.WarEndsInNumMins, (short) 1, (Character) null, new short?());
                    break;
            }

            ++this.WarNotofocationStep;
        }

        public void Send(RealmPacketOut packet, bool addEnd = false, short? asda2FactionId = null,
            Locale locale = Locale.Any)
        {
            lock (this.JoinLock)
            {
                short? nullable1 = asda2FactionId;
                if (!(nullable1.HasValue ? new int?((int) nullable1.GetValueOrDefault()) : new int?()).HasValue)
                {
                    foreach (Character character in this.DarkTeam.Values)
                    {
                        if (locale == Locale.Any || character.Client.Locale == locale)
                            character.Send(packet, addEnd);
                    }

                    foreach (Character character in this.LightTeam.Values)
                    {
                        if (locale == Locale.Any || character.Client.Locale == locale)
                            character.Send(packet, addEnd);
                    }
                }
                else
                {
                    short? nullable2 = asda2FactionId;
                    foreach (Character character in (nullable2.GetValueOrDefault() != (short) 0
                                                        ? 0
                                                        : (nullable2.HasValue ? 1 : 0)) != 0
                        ? this.LightTeam.Values
                        : this.DarkTeam.Values)
                    {
                        if (locale == Locale.Any || character.Client.Locale == locale)
                            character.Send(packet, false);
                    }
                }
            }
        }

        public void TeleportToWar(Character activeCharacter)
        {
            if (activeCharacter.Asda2FactionId == (short) 0)
                activeCharacter.TeleportTo((IWorldLocation) this.LightStartLocation);
            else
                activeCharacter.TeleportTo((IWorldLocation) this.DarkStartLocation);
        }

        public void GainScores(Character killer, short points)
        {
            this.GainScores(killer.Asda2FactionId, points);
        }

        public void GainScores(short factionId, short points)
        {
            if (factionId == (short) 0)
                this.LightScores += (int) points;
            else
                this.DarkScores += (int) points;
            Asda2BattlegroundHandler.SendTeamPointsResponse(this, (Character) null);
        }

        public void SendCurrentProgress(Character character)
        {
            if (DateTime.Now < this.StartTime.AddMinutes(2.0))
                Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this,
                    BattleGroundInfoMessageType.WarStartsInNumMins,
                    (short) (this.StartTime.AddMinutes(2.0) - DateTime.Now).TotalMinutes, character, new short?());
            else
                Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this, BattleGroundInfoMessageType.WarStarted,
                    (short) 0, character, new short?());
        }

        public Vector3 GetBasePosition(Character activeCharacter)
        {
            if (activeCharacter.Asda2FactionId != (short) 0)
                return this.DarkStartLocation.Position;
            return this.LightStartLocation.Position;
        }

        public Vector3 GetForeigLocation(Character activeCharacter)
        {
            if (activeCharacter.Asda2FactionId != (short) 0)
                return this.LightStartLocation.Position;
            return this.DarkStartLocation.Position;
        }

        public short DissmisFaction { get; set; }

        public Character DissmissingCharacter { get; set; }

        public DateTime DissmissTimeouted { get; set; }

        public void AnswerDismiss(bool kick, Character answerer)
        {
            lock (this)
            {
                if (!this.IsDismissInProgress || (int) answerer.Asda2FactionId != (int) this.DissmisFaction ||
                    answerer == this.DissmissingCharacter)
                    return;
                if (kick)
                {
                    if (this.DissmissYes.Contains(answerer))
                        return;
                    this.DissmissYes.Add(answerer);
                    if ((double) this.DissmissYes.Count <= (this.DissmisFaction == (short) 0
                            ? (double) this.LightTeam.Count * 0.65
                            : (double) this.DarkTeam.Count * 0.65))
                        return;
                    Asda2BattlegroundHandler.SendDissmissResultResponse(this, DismissPlayerResult.Ok,
                        this.DissmissingCharacter.SessionId, (int) this.DissmissingCharacter.AccId);
                    this.Leave(this.DissmissingCharacter);
                    this.IsDismissInProgress = false;
                    this.DissmissingCharacter = (Character) null;
                }
                else
                {
                    if (this.DissmissNo.Contains(answerer))
                        return;
                    this.DissmissNo.Add(answerer);
                    if ((double) this.DissmissNo.Count <= (this.DissmisFaction == (short) 0
                            ? (double) this.LightTeam.Count * 0.3
                            : (double) this.DarkTeam.Count * 0.3))
                        return;
                    Asda2BattlegroundHandler.SendDissmissResultResponse(this, DismissPlayerResult.Fail,
                        this.DissmissingCharacter.SessionId, (int) this.DissmissingCharacter.AccId);
                    this.IsDismissInProgress = false;
                    this.DissmissingCharacter = (Character) null;
                }
            }
        }

        public bool TryStartDissmisProgress(Character initer, Character dissmiser)
        {
            lock (this)
            {
                if (this.IsDismissInProgress)
                {
                    if (!(this.DissmissTimeouted < DateTime.Now))
                        return false;
                    Asda2BattlegroundHandler.SendDissmissResultResponse(this, DismissPlayerResult.Fail,
                        this.DissmissingCharacter.SessionId, (int) this.DissmissingCharacter.AccId);
                }

                this.IsDismissInProgress = true;
                Asda2BattlegroundHandler.SendQuestionDismissPlayerOrNotResponse(this, initer, dissmiser);
                this.DissmissingCharacter = dissmiser;
                this.DissmissYes.Clear();
                this.DissmissNo.Clear();
                this.DissmissTimeouted = DateTime.Now.AddMinutes(1.0);
                this.DissmisFaction = initer.Asda2FactionId;
                return true;
            }
        }

        public Character GetCharacter(short asda2FactionId, byte warId)
        {
            if (warId == (byte) 0)
            {
                if (this.LightTeam.Count > (int) warId)
                    return this.LightTeam[warId];
                return (Character) null;
            }

            if (this.DarkTeam.Count > (int) warId)
                return this.DarkTeam[warId];
            return (Character) null;
        }
    }
}