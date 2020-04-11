using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using WCell.Constants.Achievements;
using WCell.Core;
using WCell.Core.Database;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class Asda2SoulmateRelationRecord : WCellRecord<Asda2SoulmateRelationRecord>
    {
        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(Asda2SoulmateRelationRecord), nameof(SoulmateRelationGuid), 1L);

        public static int[] ExpTable = new int[30]
        {
            0,
            28,
            66,
            116,
            184,
            274,
            396,
            560,
            782,
            1080,
            1484,
            2038,
            2610,
            3198,
            3804,
            4428,
            5070,
            5732,
            6414,
            7116,
            7840,
            8840,
            9840,
            10840,
            11840,
            12840,
            13840,
            14840,
            15840,
            16840
        };

        public Dictionary<Asda2SoulmateSkillId, Asda2SoulmateSkill> Skills =
            new Dictionary<Asda2SoulmateSkillId, Asda2SoulmateSkill>();

        private float _expirience;
        private byte _applePoints;
        private byte _friendShipPoints;

        /// <summary>Returns the next unique Id for a new SpellRecord</summary>
        public static long NextId()
        {
            return Asda2SoulmateRelationRecord._idGenerator.Next();
        }

        public ulong DirectId
        {
            get { return ((ulong) this.AccId << 32) + (ulong) this.RelatedAccId; }
        }

        public ulong RevercedId
        {
            get { return ((ulong) this.RelatedAccId << 32) + (ulong) this.AccId; }
        }

        public Asda2SoulmateRelationRecord()
        {
            this.InitSkills();
        }

        public Asda2SoulmateRelationRecord(uint accId, uint relatedAccId)
        {
            this.State = RecordState.New;
            this.AccId = accId;
            this.RelatedAccId = relatedAccId;
            this.SoulmateRelationGuid = Asda2SoulmateRelationRecord.NextId();
            this.Level = (byte) 1;
            this.InitSkills();
        }

        private void InitSkills()
        {
            Asda2SoulmateSkillHeal soulmateSkillHeal = new Asda2SoulmateSkillHeal();
            Asda2SoulmateSkillEmpower soulmateSkillEmpower = new Asda2SoulmateSkillEmpower();
            this.Skills.Add(Asda2SoulmateSkillId.Call, (Asda2SoulmateSkill) new Asda2SoulmateSkillCall());
            this.Skills.Add(Asda2SoulmateSkillId.Empower, (Asda2SoulmateSkill) soulmateSkillEmpower);
            this.Skills.Add(Asda2SoulmateSkillId.Empower1, (Asda2SoulmateSkill) soulmateSkillEmpower);
            this.Skills.Add(Asda2SoulmateSkillId.Heal, (Asda2SoulmateSkill) soulmateSkillHeal);
            this.Skills.Add(Asda2SoulmateSkillId.Heal1, (Asda2SoulmateSkill) soulmateSkillHeal);
            this.Skills.Add(Asda2SoulmateSkillId.Heal2, (Asda2SoulmateSkill) soulmateSkillHeal);
            this.Skills.Add(Asda2SoulmateSkillId.Resurect, (Asda2SoulmateSkill) new Asda2SoulmateSkillResurect());
            this.Skills.Add(Asda2SoulmateSkillId.SoulSave, (Asda2SoulmateSkill) new Asda2SoulmateSkillSoulSave());
            this.Skills.Add(Asda2SoulmateSkillId.SoulSong, (Asda2SoulmateSkill) new Asda2SoulmateSkillSoulSong());
            this.Skills.Add(Asda2SoulmateSkillId.Teleport, (Asda2SoulmateSkill) new Asda2SoulmateSkillTeleport());
        }

        [PrimaryKey(PrimaryKeyType.Assigned)] public long SoulmateRelationGuid { get; set; }

        [Property] public uint AccId { get; set; }

        [Property] public uint RelatedAccId { get; set; }

        [Property]
        public float Expirience
        {
            get { return this._expirience; }
            set
            {
                this._expirience = value;
                this.TryLevelUp();
                Character characterByAccId1 = World.GetCharacterByAccId(this.AccId);
                Character characterByAccId2 = World.GetCharacterByAccId(this.RelatedAccId);
                if (characterByAccId1 == null || characterByAccId2 == null)
                    return;
                Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(characterByAccId1.Client);
                Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(characterByAccId2.Client);
                this.SaveLater();
            }
        }

        private void TryLevelUp()
        {
            if ((int) this.Level >= Asda2SoulmateRelationRecord.ExpTable.Length)
                return;
            if ((double) Asda2SoulmateRelationRecord.ExpTable[(int) this.Level] < (double) this.Expirience)
                ++this.Level;
            Character characterByAccId1 = World.GetCharacterByAccId(this.AccId);
            Character characterByAccId2 = World.GetCharacterByAccId(this.RelatedAccId);
            if (characterByAccId1 == null || characterByAccId2 == null)
                return;
            if (this.Level == (byte) 15)
            {
                characterByAccId1.GetTitle(Asda2TitleId.Companion87);
                characterByAccId2.GetTitle(Asda2TitleId.Companion87);
            }

            if (this.Level == (byte) 30)
            {
                characterByAccId1.GetTitle(Asda2TitleId.Soulmate88);
                characterByAccId2.GetTitle(Asda2TitleId.Soulmate88);
            }

            if (characterByAccId1.isTitleGetted(Asda2TitleId.Searching85) &&
                characterByAccId1.isTitleGetted(Asda2TitleId.Friend86) &&
                (characterByAccId1.isTitleGetted(Asda2TitleId.Companion87) &&
                 characterByAccId1.isTitleGetted(Asda2TitleId.Soulmate88)) &&
                (characterByAccId1.isTitleGetted(Asda2TitleId.Heartbreaker89) &&
                 characterByAccId1.isTitleGetted(Asda2TitleId.LoveNote90) &&
                 (characterByAccId1.isTitleGetted(Asda2TitleId.Cherished91) &&
                  characterByAccId1.isTitleGetted(Asda2TitleId.Devoted92))) &&
                characterByAccId1.isTitleGetted(Asda2TitleId.SnowWhite93))
                characterByAccId1.GetTitle(Asda2TitleId.TrueLove94);
            if (!characterByAccId2.isTitleGetted(Asda2TitleId.Searching85) ||
                !characterByAccId2.isTitleGetted(Asda2TitleId.Friend86) ||
                (!characterByAccId2.isTitleGetted(Asda2TitleId.Companion87) ||
                 !characterByAccId2.isTitleGetted(Asda2TitleId.Soulmate88)) ||
                (!characterByAccId2.isTitleGetted(Asda2TitleId.Heartbreaker89) ||
                 !characterByAccId2.isTitleGetted(Asda2TitleId.LoveNote90) ||
                 (!characterByAccId2.isTitleGetted(Asda2TitleId.Cherished91) ||
                  !characterByAccId2.isTitleGetted(Asda2TitleId.Devoted92))) ||
                !characterByAccId2.isTitleGetted(Asda2TitleId.SnowWhite93))
                return;
            characterByAccId2.GetTitle(Asda2TitleId.TrueLove94);
        }

        [Property] public bool IsActive { get; set; }

        public void UpdateCharacters()
        {
            this.SaveAndFlush();
            RealmAccount loggedInAccount1 =
                ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(this.AccId);
            RealmAccount loggedInAccount2 =
                ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(this.RelatedAccId);
            if (loggedInAccount1 != null && loggedInAccount1.ActiveCharacter != null)
            {
                if (!this.IsActive)
                    loggedInAccount1.ActiveCharacter.RemovaAllSoulmateBonuses();
                loggedInAccount1.ActiveCharacter.ProcessSoulmateRelation(false);
                if (this.IsActive && loggedInAccount2 != null && loggedInAccount2.ActiveCharacter != null)
                    Asda2SoulmateHandler.SendYouHaveSoulmatedWithResponse(loggedInAccount1.ActiveCharacter.Client,
                        SoulmatingResult.Ok, (uint) this.SoulmateRelationGuid, this.RelatedAccId,
                        loggedInAccount2.ActiveCharacter.Name);
            }

            if (loggedInAccount2 == null || loggedInAccount2.ActiveCharacter == null)
                return;
            if (!this.IsActive)
                loggedInAccount2.ActiveCharacter.RemovaAllSoulmateBonuses();
            loggedInAccount2.ActiveCharacter.ProcessSoulmateRelation(false);
            if (!this.IsActive || loggedInAccount1 == null || loggedInAccount1.ActiveCharacter == null)
                return;
            Asda2SoulmateHandler.SendYouHaveSoulmatedWithResponse(loggedInAccount2.ActiveCharacter.Client,
                SoulmatingResult.Ok, (uint) this.SoulmateRelationGuid, this.AccId,
                loggedInAccount1.ActiveCharacter.Name);
        }

        [Property] public byte Level { get; set; }

        [Property]
        public byte FriendShipPoints
        {
            get { return this._friendShipPoints; }
            private set
            {
                this._friendShipPoints = value;
                Character characterByAccId1 = World.GetCharacterByAccId(this.AccId);
                Character characterByAccId2 = World.GetCharacterByAccId(this.RelatedAccId);
                if (characterByAccId1 == null || characterByAccId2 == null)
                    return;
                Asda2SoulmateHandler.SendUpdateFriendShipPointsResponse(characterByAccId1);
                Asda2SoulmateHandler.SendUpdateFriendShipPointsResponse(characterByAccId2);
            }
        }

        public byte ApplePoints
        {
            get { return this._applePoints; }
            set
            {
                if (value != (byte) 0 && this._applePoints == (byte) 100)
                    return;
                this._applePoints = value;
                Character characterByAccId1 = World.GetCharacterByAccId(this.AccId);
                Character characterByAccId2 = World.GetCharacterByAccId(this.RelatedAccId);
                if (characterByAccId1 == null || characterByAccId2 == null)
                    return;
                Asda2SoulmateHandler.SendAppleExpGainedResponse(characterByAccId1);
                Asda2SoulmateHandler.SendAppleExpGainedResponse(characterByAccId2);
            }
        }

        public uint NextUpdate { get; set; }

        public void OnUpdateTick()
        {
            lock (this)
            {
                if ((long) this.NextUpdate > (long) Environment.TickCount)
                    return;
                this.NextUpdate = (uint) (Environment.TickCount + 60000);
            }

            Character characterByAccId1 = World.GetCharacterByAccId(this.AccId);
            Character characterByAccId2 = World.GetCharacterByAccId(this.RelatedAccId);
            if (characterByAccId1 == null || characterByAccId2 == null ||
                (double) characterByAccId1.GetDist((IHasPosition) characterByAccId2) > 40.0)
            {
                --this.FriendShipPoints;
                if (characterByAccId1 != null)
                {
                    characterByAccId1.RemoveFromFriendDamageBonus();
                    characterByAccId1.RemoveFriendEmpower();
                }

                if (characterByAccId2 == null)
                    return;
                characterByAccId2.RemoveFromFriendDamageBonus();
                characterByAccId2.RemoveFriendEmpower();
            }
            else
            {
                if (DateTime.Now > characterByAccId1.SoulmateEmpowerEndTime)
                    characterByAccId1.RemoveFriendEmpower();
                if (DateTime.Now > characterByAccId2.SoulmateEmpowerEndTime)
                    characterByAccId2.RemoveFriendEmpower();
                if (DateTime.Now > characterByAccId1.SoulmateSongEndTime)
                    characterByAccId1.RemoveSoulmateSong();
                if (DateTime.Now > characterByAccId2.SoulmateSongEndTime)
                    characterByAccId2.RemoveSoulmateSong();
                characterByAccId1.AddFromFriendDamageBonus();
                characterByAccId2.AddFromFriendDamageBonus();
                this.Expirience += CharacterFormulas.SoulmateExpGainPerMinuteNearFriend;
                if (this.FriendShipPoints >= (byte) 100)
                    return;
                ++this.FriendShipPoints;
            }
        }

        public uint FriendAccId(uint accId)
        {
            if ((int) this.AccId != (int) accId)
                return this.AccId;
            return this.RelatedAccId;
        }

        public void OnExpGained(bool fromMonster)
        {
            Character characterByAccId1 = World.GetCharacterByAccId(this.AccId);
            Character characterByAccId2 = World.GetCharacterByAccId(this.RelatedAccId);
            if (characterByAccId1 == null || characterByAccId2 == null ||
                (characterByAccId2.SoulmateRecord == null || characterByAccId1.SoulmateRecord == null) ||
                (double) characterByAccId1.GetDist((IHasPosition) characterByAccId2) > 40.0)
            {
                --this.FriendShipPoints;
            }
            else
            {
                this.Expirience +=
                    (float) ((fromMonster
                                 ? (double) CharacterFormulas.SoulmatExpFromMonstrKilled
                                 : (double) CharacterFormulas.SoulmatExpFromAnyExp) *
                             Math.Pow((double) ((int) this.FriendShipPoints + 1), 0.1));
                ++this.ApplePoints;
            }
        }
    }
}