using WCell.Core;
using WCell.Core.Timers;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.Util.Variables;

namespace WCell.RealmServer.Events.Asda2.Managers
{
  public static class StayOnlineEventManager
  {
    private static int _intervalMins;
    private static int _itemId;
    private static readonly int _tickInterval = 60 * 1000;

    [NotVariable]
    public static bool Started;
    private static readonly SelfRunningTaskQueue GetRewardQueue = new SelfRunningTaskQueue(1000, "Stay online event");
    private static SimpleTimerEntry _timer;

    public static void Start(int intervalMins, int itemId)
    {
      var template = Asda2ItemMgr.GetTemplate(itemId);
      if (Started || template == null)
        return;

      _timer = GetRewardQueue.CallPeriodically(_tickInterval, GiveReward);

      Asda2EventMgr.SendMessageToWorld("STAY ONLINE Event started. Every ~{0} mins you will get a prize! Stay ONLINE!",
        intervalMins);
      Started = true;
      _itemId = itemId;
      _intervalMins = intervalMins;
    }

    public static void Stop()
    {
      Asda2EventMgr.SendMessageToWorld("STAY ONLINE Event ended.");
      GetRewardQueue.CancelTimer(_timer);
      _timer = null;
      Started = false;
    }

    public static void GiveReward()
    {
      var characters = World.GetAllCharacters();

      foreach (var character in characters)
      {
        character.StayOnlineCounter += _tickInterval;
        if (character.StayOnlineCounter / (60 * 1000f) < _intervalMins)
          continue;
        if (character.MaxLevel < 60)
                {
                    break;
                    
                }
        else { 
        character.StayOnlineCounter = 0;

        RealmServer.IOQueue.AddMessage(
          () =>
            character.Asda2Inventory.AddDonateItem(Asda2ItemMgr.GetTemplate(_itemId), 1,
              "stay_online"));

        Log.Create(Log.Types.EventOperations, LogSourceType.Character, character.EntryId)
          .AddAttribute("win", 0, "stay_online")
          .Write();
                }
            }
    }
  }
}