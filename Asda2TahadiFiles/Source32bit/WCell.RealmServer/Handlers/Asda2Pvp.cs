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
      get { return _losser; }
      set
      {
        _losser = value;
        StopPvp();
      }
    }

    public Character Winner
    {
      get
      {
        if(FirstCharacter != Losser)
          return FirstCharacter;
        return SecondCharacter;
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
      firstCharacter.Map.CallDelayed(10000, StartPvp);
      PvpTimeOuted = Environment.TickCount + PvpTimeSecs * 1000;
      IsActive = true;
      FirstCharacter = firstCharacter;
      SecondCharacter = secondCharacter;
      FirstCharacter.EnemyCharacters.Add(secondCharacter);
      SecondCharacter.EnemyCharacters.Add(firstCharacter);
    }

    public void StartPvp()
    {
      if(!IsActive)
        return;
      GlobalHandler.SendFightingModeChangedResponse(FirstCharacter.Client, FirstCharacter.SessionId,
        (int) FirstCharacter.AccId, SecondCharacter.SessionId);
      GlobalHandler.SendFightingModeChangedResponse(SecondCharacter.Client, SecondCharacter.SessionId,
        (int) SecondCharacter.AccId, FirstCharacter.SessionId);
      UpdatePvp();
    }

    public void StopPvp()
    {
      if(!IsActive)
        return;
      IsActive = false;
      if(Losser == null)
        Losser = FirstCharacter;
      FirstCharacter.EnemyCharacters.Remove(SecondCharacter);
      SecondCharacter.EnemyCharacters.Remove(FirstCharacter);
      FirstCharacter.CheckEnemysCount();
      SecondCharacter.CheckEnemysCount();
      Asda2PvpHandler.SendDuelEndedResponse(Winner, Losser);
      FirstCharacter.Asda2Duel = null;
      SecondCharacter.Asda2Duel = null;
      FirstCharacter.Asda2DuelingOponent = null;
      SecondCharacter.Asda2DuelingOponent = null;
      FirstCharacter = null;
      SecondCharacter = null;
    }

    public void UpdatePvp()
    {
      if(PvpTimeOuted < Environment.TickCount || FirstCharacter == null ||
         (SecondCharacter == null || FirstCharacter.Map == null) || SecondCharacter.Map == null)
        StopPvp();
      else
        FirstCharacter.Map.CallDelayed(3000, UpdatePvp);
    }
  }
}