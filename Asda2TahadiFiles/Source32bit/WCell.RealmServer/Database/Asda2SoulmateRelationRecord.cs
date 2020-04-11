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
  [ActiveRecord(Access = PropertyAccess.Property)]
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
      return _idGenerator.Next();
    }

    public ulong DirectId
    {
      get { return ((ulong) AccId << 32) + RelatedAccId; }
    }

    public ulong RevercedId
    {
      get { return ((ulong) RelatedAccId << 32) + AccId; }
    }

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

    private void InitSkills()
    {
      Asda2SoulmateSkillHeal soulmateSkillHeal = new Asda2SoulmateSkillHeal();
      Asda2SoulmateSkillEmpower soulmateSkillEmpower = new Asda2SoulmateSkillEmpower();
      Skills.Add(Asda2SoulmateSkillId.Call, new Asda2SoulmateSkillCall());
      Skills.Add(Asda2SoulmateSkillId.Empower, soulmateSkillEmpower);
      Skills.Add(Asda2SoulmateSkillId.Empower1, soulmateSkillEmpower);
      Skills.Add(Asda2SoulmateSkillId.Heal, soulmateSkillHeal);
      Skills.Add(Asda2SoulmateSkillId.Heal1, soulmateSkillHeal);
      Skills.Add(Asda2SoulmateSkillId.Heal2, soulmateSkillHeal);
      Skills.Add(Asda2SoulmateSkillId.Resurect, new Asda2SoulmateSkillResurect());
      Skills.Add(Asda2SoulmateSkillId.SoulSave, new Asda2SoulmateSkillSoulSave());
      Skills.Add(Asda2SoulmateSkillId.SoulSong, new Asda2SoulmateSkillSoulSong());
      Skills.Add(Asda2SoulmateSkillId.Teleport, new Asda2SoulmateSkillTeleport());
    }

    [PrimaryKey(PrimaryKeyType.Assigned)]
    public long SoulmateRelationGuid { get; set; }

    [Property]
    public uint AccId { get; set; }

    [Property]
    public uint RelatedAccId { get; set; }

    [Property]
    public float Expirience
    {
      get { return _expirience; }
      set
      {
        _expirience = value;
        TryLevelUp();
        Character characterByAccId1 = World.GetCharacterByAccId(AccId);
        Character characterByAccId2 = World.GetCharacterByAccId(RelatedAccId);
        if(characterByAccId1 == null || characterByAccId2 == null)
          return;
        Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(characterByAccId1.Client);
        Asda2SoulmateHandler.SendSoulMateHpMpUpdateResponse(characterByAccId2.Client);
        this.SaveLater();
      }
    }

    private void TryLevelUp()
    {
      if(Level >= ExpTable.Length)
        return;
      if(ExpTable[Level] < (double) Expirience)
        ++Level;
      Character characterByAccId1 = World.GetCharacterByAccId(AccId);
      Character characterByAccId2 = World.GetCharacterByAccId(RelatedAccId);
      if(characterByAccId1 == null || characterByAccId2 == null)
        return;
      if(Level == 15)
      {
        characterByAccId1.GetTitle(Asda2TitleId.Companion87);
        characterByAccId2.GetTitle(Asda2TitleId.Companion87);
      }

      if(Level == 30)
      {
        characterByAccId1.GetTitle(Asda2TitleId.Soulmate88);
        characterByAccId2.GetTitle(Asda2TitleId.Soulmate88);
      }

      if(characterByAccId1.isTitleGetted(Asda2TitleId.Searching85) &&
         characterByAccId1.isTitleGetted(Asda2TitleId.Friend86) &&
         (characterByAccId1.isTitleGetted(Asda2TitleId.Companion87) &&
          characterByAccId1.isTitleGetted(Asda2TitleId.Soulmate88)) &&
         (characterByAccId1.isTitleGetted(Asda2TitleId.Heartbreaker89) &&
          characterByAccId1.isTitleGetted(Asda2TitleId.LoveNote90) &&
          (characterByAccId1.isTitleGetted(Asda2TitleId.Cherished91) &&
           characterByAccId1.isTitleGetted(Asda2TitleId.Devoted92))) &&
         characterByAccId1.isTitleGetted(Asda2TitleId.SnowWhite93))
        characterByAccId1.GetTitle(Asda2TitleId.TrueLove94);
      if(!characterByAccId2.isTitleGetted(Asda2TitleId.Searching85) ||
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

    [Property]
    public bool IsActive { get; set; }

    public void UpdateCharacters()
    {
      SaveAndFlush();
      RealmAccount loggedInAccount1 =
        ServerApp<RealmServer>.Instance.GetLoggedInAccount(AccId);
      RealmAccount loggedInAccount2 =
        ServerApp<RealmServer>.Instance.GetLoggedInAccount(RelatedAccId);
      if(loggedInAccount1 != null && loggedInAccount1.ActiveCharacter != null)
      {
        if(!IsActive)
          loggedInAccount1.ActiveCharacter.RemovaAllSoulmateBonuses();
        loggedInAccount1.ActiveCharacter.ProcessSoulmateRelation(false);
        if(IsActive && loggedInAccount2 != null && loggedInAccount2.ActiveCharacter != null)
          Asda2SoulmateHandler.SendYouHaveSoulmatedWithResponse(loggedInAccount1.ActiveCharacter.Client,
            SoulmatingResult.Ok, (uint) SoulmateRelationGuid, RelatedAccId,
            loggedInAccount2.ActiveCharacter.Name);
      }

      if(loggedInAccount2 == null || loggedInAccount2.ActiveCharacter == null)
        return;
      if(!IsActive)
        loggedInAccount2.ActiveCharacter.RemovaAllSoulmateBonuses();
      loggedInAccount2.ActiveCharacter.ProcessSoulmateRelation(false);
      if(!IsActive || loggedInAccount1 == null || loggedInAccount1.ActiveCharacter == null)
        return;
      Asda2SoulmateHandler.SendYouHaveSoulmatedWithResponse(loggedInAccount2.ActiveCharacter.Client,
        SoulmatingResult.Ok, (uint) SoulmateRelationGuid, AccId,
        loggedInAccount1.ActiveCharacter.Name);
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
        Character characterByAccId1 = World.GetCharacterByAccId(AccId);
        Character characterByAccId2 = World.GetCharacterByAccId(RelatedAccId);
        if(characterByAccId1 == null || characterByAccId2 == null)
          return;
        Asda2SoulmateHandler.SendUpdateFriendShipPointsResponse(characterByAccId1);
        Asda2SoulmateHandler.SendUpdateFriendShipPointsResponse(characterByAccId2);
      }
    }

    public byte ApplePoints
    {
      get { return _applePoints; }
      set
      {
        if(value != 0 && _applePoints == 100)
          return;
        _applePoints = value;
        Character characterByAccId1 = World.GetCharacterByAccId(AccId);
        Character characterByAccId2 = World.GetCharacterByAccId(RelatedAccId);
        if(characterByAccId1 == null || characterByAccId2 == null)
          return;
        Asda2SoulmateHandler.SendAppleExpGainedResponse(characterByAccId1);
        Asda2SoulmateHandler.SendAppleExpGainedResponse(characterByAccId2);
      }
    }

    public uint NextUpdate { get; set; }

    public void OnUpdateTick()
    {
      lock(this)
      {
        if(NextUpdate > Environment.TickCount)
          return;
        NextUpdate = (uint) (Environment.TickCount + 60000);
      }

      Character characterByAccId1 = World.GetCharacterByAccId(AccId);
      Character characterByAccId2 = World.GetCharacterByAccId(RelatedAccId);
      if(characterByAccId1 == null || characterByAccId2 == null ||
         characterByAccId1.GetDist(characterByAccId2) > 40.0)
      {
        --FriendShipPoints;
        if(characterByAccId1 != null)
        {
          characterByAccId1.RemoveFromFriendDamageBonus();
          characterByAccId1.RemoveFriendEmpower();
        }

        if(characterByAccId2 == null)
          return;
        characterByAccId2.RemoveFromFriendDamageBonus();
        characterByAccId2.RemoveFriendEmpower();
      }
      else
      {
        if(DateTime.Now > characterByAccId1.SoulmateEmpowerEndTime)
          characterByAccId1.RemoveFriendEmpower();
        if(DateTime.Now > characterByAccId2.SoulmateEmpowerEndTime)
          characterByAccId2.RemoveFriendEmpower();
        if(DateTime.Now > characterByAccId1.SoulmateSongEndTime)
          characterByAccId1.RemoveSoulmateSong();
        if(DateTime.Now > characterByAccId2.SoulmateSongEndTime)
          characterByAccId2.RemoveSoulmateSong();
        characterByAccId1.AddFromFriendDamageBonus();
        characterByAccId2.AddFromFriendDamageBonus();
        Expirience += CharacterFormulas.SoulmateExpGainPerMinuteNearFriend;
        if(FriendShipPoints >= 100)
          return;
        ++FriendShipPoints;
      }
    }

    public uint FriendAccId(uint accId)
    {
      if((int) AccId != (int) accId)
        return AccId;
      return RelatedAccId;
    }

    public void OnExpGained(bool fromMonster)
    {
      Character characterByAccId1 = World.GetCharacterByAccId(AccId);
      Character characterByAccId2 = World.GetCharacterByAccId(RelatedAccId);
      if(characterByAccId1 == null || characterByAccId2 == null ||
         (characterByAccId2.SoulmateRecord == null || characterByAccId1.SoulmateRecord == null) ||
         characterByAccId1.GetDist(characterByAccId2) > 40.0)
      {
        --FriendShipPoints;
      }
      else
      {
        Expirience +=
          (float) ((fromMonster
                     ? (double) CharacterFormulas.SoulmatExpFromMonstrKilled
                     : (double) CharacterFormulas.SoulmatExpFromAnyExp) *
                   Math.Pow(FriendShipPoints + 1, 0.1));
        ++ApplePoints;
      }
    }
  }
}