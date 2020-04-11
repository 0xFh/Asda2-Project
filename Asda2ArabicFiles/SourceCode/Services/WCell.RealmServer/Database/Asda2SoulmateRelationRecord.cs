using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Database
{
    [ActiveRecord(Access = PropertyAccess.Property)]
    public class Asda2SoulmateRelationRecord : WCellRecord<Asda2SoulmateRelationRecord>
    {
        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(Asda2SoulmateRelationRecord), "SoulmateRelationGuid");


        public Dictionary<Asda2SoulmateSkillId, Asda2SoulmateSkill> Skills = new Dictionary<Asda2SoulmateSkillId, Asda2SoulmateSkill>();
        /// <summary>
        /// Returns the next unique Id for a new SpellRecord
        /// </summary>
        public static long NextId()
        {
            return _idGenerator.Next();
        }
        public ulong DirectId
        {
            get
            {
                return ((ulong)AccId << 32) + RelatedAccId;
            }
        }
        public ulong RevercedId
        {
            get
            {
                return ((ulong)RelatedAccId << 32) + AccId;
            }
        }
        /*[Field("AccId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
        private long _accId;

        [Field("RelatedAccId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
        private long _relatedAccId;*/

        private float _expirience;
        private byte _applePoints;
        private byte _friendShipPoints;

        public Asda2SoulmateRelationRecord()
        {
            InitSkills();
        }

        public Asda2SoulmateRelationRecord(uint accId, uint relatedAccId)
        {
            State = RecordState.New;
            AccId = accId;
            RelatedAccId = relatedAccId;
            SoulmateRelationGuid = NextId();
            Level = 1;
            InitSkills();
        }
        void InitSkills()
        {
            var h = new Asda2SoulmateSkillHeal();
            var e = new Asda2SoulmateSkillEmpower();
            Skills.Add(Asda2SoulmateSkillId.Call, new Asda2SoulmateSkillCall());
            Skills.Add(Asda2SoulmateSkillId.Empower, e);
            Skills.Add(Asda2SoulmateSkillId.Empower1,e);
            Skills.Add(Asda2SoulmateSkillId.Heal, h);
            Skills.Add(Asda2SoulmateSkillId.Heal1,h);
            Skills.Add(Asda2SoulmateSkillId.Heal2,h );
            Skills.Add(Asda2SoulmateSkillId.Resurect, new Asda2SoulmateSkillResurect());
            Skills.Add(Asda2SoulmateSkillId.SoulSave, new Asda2SoulmateSkillSoulSave());
            Skills.Add(Asda2SoulmateSkillId.SoulSong, new Asda2SoulmateSkillSoulSong());
            Skills.Add(Asda2SoulmateSkillId.Teleport, new Asda2SoulmateSkillTeleport());
        }
        [PrimaryKey(PrimaryKeyType.Assigned)]
        public long SoulmateRelationGuid
        {
            get;
            set;
        }
        [Property]
        public uint AccId { get; set; }
        [Property]
        public uint RelatedAccId { get; set; }
        
        [Property]
        public float Expirience
        {
            get { return _expirience; }
            set {
                _expirience = value;
                TryLevelUp();
            var firstChar = World.GetCharacterByAccId(AccId);
            var secChar = World.GetCharacterByAccId(RelatedAccId);
            if (firstChar == null || secChar == null)
                return;
                Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(firstChar.Client);
                Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(secChar.Client);
                this.SaveLater();
            }
        }

        private void TryLevelUp()
        {
            if(Level>=ExpTable.Length)
                return;
            if (ExpTable[Level] < Expirience)
            {
                Level++;
                var firstChar = World.GetCharacterByAccId(AccId);
                var secChar = World.GetCharacterByAccId(RelatedAccId);
                Asda2TitleChecker.OnSoulmatingLevelChanged(Level, firstChar);
                Asda2TitleChecker.OnSoulmatingLevelChanged(Level, secChar);
            }
        }
        public static int[] ExpTable = new [] { 0, 28, 66, 116, 184, 274, 396, 560, 782, 1080, 1484, 2038, 2610, 3198, 3804, 4428, 5070, 5732, 6414, 7116, 7840, 8840, 9840, 10840, 11840, 12840, 13840, 14840, 15840, 16840 };
        [Property]
        public bool IsActive { get; set; }

        public void UpdateCharacters()
        {
            //update characters
                SaveAndFlush();
                var firstChrAcc = RealmServer.Instance.GetLoggedInAccount(AccId);
                var secondChrAcc = RealmServer.Instance.GetLoggedInAccount(RelatedAccId);
                if (firstChrAcc != null && firstChrAcc.ActiveCharacter != null)
                {
                    if (!IsActive)
                        firstChrAcc.ActiveCharacter.RemovaAllSoulmateBonuses();
                    firstChrAcc.ActiveCharacter.ProcessSoulmateRelation(false);
                    if (IsActive && secondChrAcc != null && secondChrAcc.ActiveCharacter != null)
                        Asda2SoulmateHandler.SendYouHaveSoulmatedWithResponse(firstChrAcc.ActiveCharacter.Client,SoulmatingResult.Ok,(uint) SoulmateRelationGuid,RelatedAccId,secondChrAcc.ActiveCharacter.Name);
                }
                if (secondChrAcc != null && secondChrAcc.ActiveCharacter != null)
                {

                    if (!IsActive)
                        secondChrAcc.ActiveCharacter.RemovaAllSoulmateBonuses();
                    secondChrAcc.ActiveCharacter.ProcessSoulmateRelation(false);
                    if (IsActive && firstChrAcc != null && firstChrAcc.ActiveCharacter != null)
                    Asda2SoulmateHandler.SendYouHaveSoulmatedWithResponse(secondChrAcc.ActiveCharacter.Client, SoulmatingResult.Ok, (uint)SoulmateRelationGuid, AccId, firstChrAcc.ActiveCharacter.Name);
                }
        }
        [Property]
        public byte Level { get; set; }
        [Property]
        public byte FriendShipPoints
        {
            get { return _friendShipPoints; }
            private set
            {
                _friendShipPoints = value;
                var firstChar = World.GetCharacterByAccId(AccId);
                var secChar = World.GetCharacterByAccId(RelatedAccId);
                if (firstChar == null || secChar == null)
                    return;
                Asda2SoulmateHandler.SendUpdateFriendShipPointsResponse(firstChar);
                Asda2SoulmateHandler.SendUpdateFriendShipPointsResponse(secChar);
            }
        }

        public byte ApplePoints
        {
            get { return _applePoints; }
            set
            {
                if (value != 0 && _applePoints == 100)
                    return;
                _applePoints = value;
                var firstChar = World.GetCharacterByAccId(AccId);
                var secChar = World.GetCharacterByAccId(RelatedAccId);
                if (firstChar == null || secChar == null)
                    return;
                Asda2SoulmateHandler.SendAppleExpGainedResponse(firstChar);
                Asda2SoulmateHandler.SendAppleExpGainedResponse(secChar);
            }
        }

        public uint NextUpdate { get; set; }

        public void OnUpdateTick()
        {
            lock (this)
            {
                if (NextUpdate > Environment.TickCount)
                    return;
                NextUpdate = (uint)(Environment.TickCount + 60000);
            }
            var firstChar = World.GetCharacterByAccId(AccId);
            var secChar = World.GetCharacterByAccId(RelatedAccId);
            if (firstChar == null || secChar == null ||firstChar.GetDist(secChar)>40)
            {
                FriendShipPoints--;

                if (firstChar != null) {firstChar.RemoveFromFriendDamageBonus(); firstChar.RemoveFriendEmpower();}
                if (secChar != null) { secChar.RemoveFromFriendDamageBonus(); secChar.RemoveFriendEmpower(); }
                return;
            }
            
            if(DateTime.Now>firstChar.SoulmateEmpowerEndTime)
                firstChar.RemoveFriendEmpower();
            if(DateTime.Now>secChar.SoulmateEmpowerEndTime)
                secChar.RemoveFriendEmpower();
            if (DateTime.Now > firstChar.SoulmateSongEndTime)
                firstChar.RemoveSoulmateSong();
            if (DateTime.Now > secChar.SoulmateSongEndTime)
                secChar.RemoveSoulmateSong();
            firstChar.AddFromFriendDamageBonus();
            secChar.AddFromFriendDamageBonus();
            
            Expirience += CharacterFormulas.SoulmateExpGainPerMinuteNearFriend;
            if(FriendShipPoints<100)
                FriendShipPoints++;
        }
        
        public uint FriendAccId(uint accId)
        {
            return AccId == accId ? RelatedAccId : AccId;
        }

        public void OnExpGained(bool fromMonster)
        {
            var firstChar = World.GetCharacterByAccId(AccId);
            var secChar = World.GetCharacterByAccId(RelatedAccId);
            if (firstChar == null || secChar == null || secChar.SoulmateRecord ==null || firstChar.SoulmateRecord == null ||  firstChar.GetDist(secChar) > 40)
            {
                FriendShipPoints--;
                return;
            }
            Expirience += (fromMonster? CharacterFormulas.SoulmatExpFromMonstrKilled: CharacterFormulas.SoulmatExpFromAnyExp)*(float)Math.Pow(FriendShipPoints+1,0.1);
            ApplePoints++;
        }
    }
}