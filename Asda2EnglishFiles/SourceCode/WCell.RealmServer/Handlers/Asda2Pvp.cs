using System;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
    public class Asda2Pvp
    {
        public static int PvpTimeSecs = 300;
        private Character _losser;

        public bool IsActive { get; set; }

        public Character FirstCharacter { get; set; }

        public Character SecondCharacter { get; set; }

        public Character Losser
        {
            get { return this._losser; }
            set
            {
                this._losser = value;
                this.StopPvp();
            }
        }

        public Character Winner
        {
            get
            {
                if (this.FirstCharacter != this.Losser)
                    return this.FirstCharacter;
                return this.SecondCharacter;
            }
        }

        public int PvpTimeOuted { get; set; }

        public Asda2Pvp(Character firstCharacter, Character secondCharacter)
        {
            firstCharacter.Asda2Duel = this;
            secondCharacter.Asda2Duel = this;
            firstCharacter.Asda2DuelingOponent = secondCharacter;
            secondCharacter.Asda2DuelingOponent = firstCharacter;
            Asda2PvpHandler.SendPvpStartedResponse(Asda2PvpResponseStatus.Ok, firstCharacter, secondCharacter);
            Asda2PvpHandler.SendPvpStartedResponse(Asda2PvpResponseStatus.Ok, secondCharacter, firstCharacter);
            Asda2PvpHandler.SendPvpRoundEffectResponse(firstCharacter, secondCharacter);
            firstCharacter.Map.CallDelayed(10000, new Action(this.StartPvp));
            this.PvpTimeOuted = Environment.TickCount + Asda2Pvp.PvpTimeSecs * 1000;
            this.IsActive = true;
            this.FirstCharacter = firstCharacter;
            this.SecondCharacter = secondCharacter;
            this.FirstCharacter.EnemyCharacters.Add(secondCharacter);
            this.SecondCharacter.EnemyCharacters.Add(firstCharacter);
        }

        public void StartPvp()
        {
            if (!this.IsActive)
                return;
            GlobalHandler.SendFightingModeChangedResponse(this.FirstCharacter.Client, this.FirstCharacter.SessionId,
                (int) this.FirstCharacter.AccId, this.SecondCharacter.SessionId);
            GlobalHandler.SendFightingModeChangedResponse(this.SecondCharacter.Client, this.SecondCharacter.SessionId,
                (int) this.SecondCharacter.AccId, this.FirstCharacter.SessionId);
            this.UpdatePvp();
        }

        public void StopPvp()
        {
            if (!this.IsActive)
                return;
            this.IsActive = false;
            if (this.Losser == null)
                this.Losser = this.FirstCharacter;
            this.FirstCharacter.EnemyCharacters.Remove(this.SecondCharacter);
            this.SecondCharacter.EnemyCharacters.Remove(this.FirstCharacter);
            this.FirstCharacter.CheckEnemysCount();
            this.SecondCharacter.CheckEnemysCount();
            Asda2PvpHandler.SendDuelEndedResponse(this.Winner, this.Losser);
            this.FirstCharacter.Asda2Duel = (Asda2Pvp) null;
            this.SecondCharacter.Asda2Duel = (Asda2Pvp) null;
            this.FirstCharacter.Asda2DuelingOponent = (Character) null;
            this.SecondCharacter.Asda2DuelingOponent = (Character) null;
            this.FirstCharacter = (Character) null;
            this.SecondCharacter = (Character) null;
        }

        public void UpdatePvp()
        {
            if (this.PvpTimeOuted < Environment.TickCount || this.FirstCharacter == null ||
                (this.SecondCharacter == null || this.FirstCharacter.Map == null) || this.SecondCharacter.Map == null)
                this.StopPvp();
            else
                this.FirstCharacter.Map.CallDelayed(3000, new Action(this.UpdatePvp));
        }
    }
}