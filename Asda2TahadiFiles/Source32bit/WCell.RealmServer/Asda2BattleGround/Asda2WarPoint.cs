using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Asda2BattleGround
{
    public class Asda2WarPoint : IUpdatable
    {
        private int _tomeToCaprute = CharacterFormulas.DefaultCaptureTime;
        private int _timeToStartCapturing = CharacterFormulas.DefaultTimeToStartCapture;
        private int _timeToNextGainPoints = CharacterFormulas.DefaultTimeGainExpReward;
        private bool _isCapturing;

        public Character CapturingCharacter { get; set; }

        public short Id { get; set; }

        public short X { get; set; }

        public short Y { get; set; }

        public short OwnedFaction { get; set; }

        public Asda2WarPointStatus Status { get; set; }

        public Asda2Battleground BattleGround { get; set; }

        public void TryCapture(Character activeCharacter)
        {
            lock (this)
            {
                if (this.CapturingCharacter != null)
                {
                    activeCharacter.SendWarMsg(string.Format("Point {0} already capturing by {1}.",
                        (object) ((int) this.Id + 1), (object) this.CapturingCharacter.Name));
                    Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(activeCharacter.Client, this.Id,
                        OcupationPointStartedStatus.Fail);
                }
                else if ((double) activeCharacter.Asda2Position.GetDistance(new Vector3((float) this.X,
                             (float) this.Y)) > 7.0)
                {
                    activeCharacter.SendWarMsg(string.Format("Distance to {0} is too big.",
                        (object) ((int) this.Id + 1)));
                    Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(activeCharacter.Client, this.Id,
                        OcupationPointStartedStatus.Fail);
                }
                else if (this.Status != Asda2WarPointStatus.NotOwned &&
                         (int) this.OwnedFaction == (int) activeCharacter.Asda2FactionId)
                {
                    Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(activeCharacter.Client, this.Id,
                        OcupationPointStartedStatus.YouAreOcupaingTheSameSide);
                }
                else
                {
                    this.CapturingCharacter = activeCharacter;
                    activeCharacter.CurrentCapturingPoint = this;
                    this.CapturingCharacter.IsMoving = false;
                    this.CapturingCharacter.IsFighting = false;
                    Asda2MovmentHandler.SendEndMoveByFastInstantRegularMoveResponse(this.CapturingCharacter);
                    this._isCapturing = false;
                    this._timeToStartCapturing = CharacterFormulas.DefaultTimeToStartCapture;
                    Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(activeCharacter.Client, this.Id,
                        OcupationPointStartedStatus.Ok);
                }
            }
        }

        public void StopCapture()
        {
            Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this.BattleGround,
                BattleGroundInfoMessageType.FailedToTemporarilyOccuptyTheNumOccupationPoints, this.Id, (Character) null,
                new short?(this.CapturingCharacter.Asda2FactionId));
            Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(this.CapturingCharacter.Client, this.Id,
                OcupationPointStartedStatus.Fail);
            this.CapturingCharacter.CurrentCapturingPoint = (Asda2WarPoint) null;
            this.CapturingCharacter = (Character) null;
            this._isCapturing = false;
        }

        public void Update(int dt)
        {
            if (this.Status == Asda2WarPointStatus.Owned)
            {
                this._timeToNextGainPoints -= dt;
                if (this._timeToNextGainPoints <= 0)
                {
                    this.BattleGround.GainScores(this.OwnedFaction,
                        CharacterFormulas.FactionWarPointsPerTicForCapturedPoints);
                    this._timeToNextGainPoints += CharacterFormulas.DefaultTimeGainExpReward;
                }
            }

            if (this._isCapturing)
            {
                this._tomeToCaprute -= dt;
                if (this._tomeToCaprute > 0)
                    return;
                this.Status = Asda2WarPointStatus.Owned;
                Asda2BattlegroundHandler.SendUpdatePointInfoResponse((IRealmClient) null, this);
                Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this.BattleGround,
                    BattleGroundInfoMessageType.SuccessToCompletelyOccuptyTheNumOccupationPoints, this.Id,
                    (Character) null, new short?(this.OwnedFaction));
                Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this.BattleGround,
                    BattleGroundInfoMessageType.TheOtherSideHasTemporarilyOccupiedTheNumOccupationPoint, this.Id,
                    (Character) null, new short?(this.OwnedFaction == (short) 0 ? (short) 1 : (short) 0));
                this.BattleGround.GainScores(this.OwnedFaction,
                    CharacterFormulas.FactionWarPointsPerTicForCapturedPoints);
                this._isCapturing = false;
            }
            else
            {
                if (this.CapturingCharacter == null || !this.BattleGround.IsStarted)
                    return;
                this._timeToStartCapturing -= dt;
                if (this._timeToStartCapturing > 0)
                    return;
                this._tomeToCaprute = CharacterFormulas.DefaultCaptureTime;
                this._isCapturing = true;
                this.OwnedFaction = this.CapturingCharacter.Asda2FactionId;
                this.CapturingCharacter.GainActPoints((short) 1);
                this.BattleGround.GainScores(this.CapturingCharacter, (short) 1);
                this.Status = Asda2WarPointStatus.Capturing;
                Asda2BattlegroundHandler.SendUpdatePointInfoResponse((IRealmClient) null, this);
                Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this.BattleGround,
                    BattleGroundInfoMessageType.SuccessToTemporarilyOccuptyTheNumOccupationPoints, this.Id,
                    (Character) null, new short?(this.CapturingCharacter.Asda2FactionId));
                Asda2BattlegroundHandler.SendWarCurrentActionInfoResponse(this.BattleGround,
                    BattleGroundInfoMessageType.TheOtherSideHasTemporarilyOccupiedTheNumOccupationPoint, this.Id,
                    (Character) null,
                    new short?(this.CapturingCharacter.Asda2FactionId == (short) 1 ? (short) 0 : (short) 1));
                Asda2BattlegroundHandler.SendOccupyingPointStartedResponse(this.CapturingCharacter.Client, this.Id,
                    OcupationPointStartedStatus.Fail);
                this.CapturingCharacter.CurrentCapturingPoint = (Asda2WarPoint) null;
                this.CapturingCharacter = (Character) null;
                this._timeToNextGainPoints = CharacterFormulas.DefaultTimeGainExpReward;
            }
        }
    }
}