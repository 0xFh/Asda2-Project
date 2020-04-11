using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.Util.Variables;

namespace WCell.RealmServer.Events.Asda2.Managers
{
  public static class GuessEventManager
  {

    #region GueesWord

    private static float _percision;
    private static string _word;
    [NotVariable]
    public static bool Started = false;
    /// <summary>
    /// Начинает эвент угадай слово
    /// </summary>
    /// <param name="word">секретное слово</param>
    /// <param name="precison">точность от 50 до 100 в процентах</param>
    public static void Start(string word, int precison, string gmName)
    {
      if (Started || word == null)
        return;
      Asda2EventMgr.SendMessageToWorld("Guess word event started. {0} is event manager. Type your answer to global chat.", gmName);
      Started = true;
      _percision = 100f / precison;
      _word = word.ToLower();
    }

    public static void Stop()
    {
      Asda2EventMgr.SendMessageToWorld("Guess word event ended.");
      Started = false;
    }

    public static void TryGuess(string word, Character senderChr)
    {
      lock (typeof(Asda2EventMgr))
      {
        if (!Started)
          return;
        var fixedWord = word.ToLower();
        float correctHits = 0f;
        for (int i = 0; i < fixedWord.Length; i++)
        {
          if (i >= _word.Length)
            break;
          if (fixedWord[i] == _word[i])
            correctHits++;
        }
        if (correctHits / _word.Length >= _percision)
        {
          //character is winner
          var exp = CharacterFormulas.CalcExpForGuessWordEvent(senderChr.Level);
          var eventItems = CharacterFormulas.EventItemsForGuessEvent;
          Asda2EventMgr.SendMessageToWorld("{0} is winner. Prize is {1} exp and {2} event items.", senderChr.Name, exp, eventItems);
          senderChr.GainXp(exp, "guess_event");
          RealmServer.IOQueue.AddMessage(() => senderChr.Asda2Inventory.AddDonateItem(Asda2ItemMgr.GetTemplate(CharacterFormulas.EventItemId), eventItems, "guess_event"));
          Stop();
          Log.Create(Log.Types.EventOperations, LogSourceType.Character, senderChr.EntryId)
            .AddAttribute("win", eventItems, "guess_event")
            .Write();
        }
      }
    }
    #endregion
  }
}