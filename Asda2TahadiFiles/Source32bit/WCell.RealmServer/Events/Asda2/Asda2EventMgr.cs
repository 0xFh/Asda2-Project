using System;
using System.Collections.Generic;
using WCell.Constants.World;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Events.Asda2
{
  public static class Asda2EventMgr
  {
    [NotVariable]public static bool IsGuessWordEventStarted;

    internal static Dictionary<MapId, DeffenceTownEvent> DefenceTownEvents =
      new Dictionary<MapId, DeffenceTownEvent>();

    private static float _percision;
    private static string _word;

    /// <summary>Начинает эвент угадай слово</summary>
    /// <param name="word">секретное слово</param>
    /// <param name="precison">точность от 50 до 100 в процентах</param>
    public static void StartGueesWordEvent(string word, int precison, string gmName)
    {
      if(IsGuessWordEventStarted || word == null)
        return;
      SendMessageToWorld(
        "Guess word event started. {0} is event manager. Type your answer to global chat.", (object) gmName);
      IsGuessWordEventStarted = true;
      _percision = 100f / precison;
      _word = word.ToLower();
    }

    public static void StopGueesWordEvent()
    {
      SendMessageToWorld("Guess word event ended.");
      IsGuessWordEventStarted = false;
    }

    public static void TryGuess(string word, Character senderChr)
    {
      lock(typeof(Asda2EventMgr))
      {
        if(!IsGuessWordEventStarted)
          return;
        string lower = word.ToLower();
        float num = 0.0f;
        for(int index = 0; index < lower.Length && index < _word.Length; ++index)
        {
          if(lower[index] == _word[index])
            ++num;
        }

        if(num / (double) _word.Length < _percision)
          return;
        int experience = CharacterFormulas.CalcExpForGuessWordEvent(senderChr.Level);
        int eventItems = CharacterFormulas.EventItemsForGuessEvent;
        SendMessageToWorld("{0} is winner. Prize is {1} exp and {2} event items.",
          (object) senderChr.Name, (object) experience, (object) eventItems);
        senderChr.GainXp(experience, "guess_event", false);
        ServerApp<RealmServer>.IOQueue.AddMessage(() =>
          senderChr.Asda2Inventory.AddDonateItem(Asda2ItemMgr.GetTemplate(CharacterFormulas.EventItemId),
            eventItems, "guess_event", false));
        StopGueesWordEvent();
        Log.Create(Log.Types.EventOperations, LogSourceType.Character, senderChr.EntryId)
          .AddAttribute("win", eventItems, "guess_event").Write();
      }
    }

    public static void SendMessageToWorld(string message, params object[] p)
    {
      World.BroadcastMsg("Event Manager", string.Format(message, p), Color.LightPink);
    }

    public static bool StartDeffenceTownEvent(Map map, int minLevel, int maxLevel, float amountMod, float healthMod,
      float otherStatsMod, float speedMod, float difficulty)
    {
      if(map.Id != MapId.Alpia)
        return false;
      if(DefenceTownEvents.ContainsKey(map.Id))
      {
        DefenceTownEvents[map.Id].Stop(false);
        DefenceTownEvents.Remove(map.Id);
      }

      DefenceTownEventAplia defenceTownEventAplia = new DefenceTownEventAplia(map, minLevel, maxLevel, amountMod,
        healthMod, otherStatsMod, speedMod, difficulty);
      DefenceTownEvents.Add(map.Id, defenceTownEventAplia);
      defenceTownEventAplia.Start();
      return true;
    }

    public static bool StopDeffenceTownEvent(Map map, bool success)
    {
      if(map.Id != MapId.Alpia)
        return false;
      if(DefenceTownEvents.ContainsKey(map.Id))
      {
        DefenceTownEvents[map.Id].Stop(success);
        DefenceTownEvents.Remove(map.Id);
      }

      return true;
    }
  }
}