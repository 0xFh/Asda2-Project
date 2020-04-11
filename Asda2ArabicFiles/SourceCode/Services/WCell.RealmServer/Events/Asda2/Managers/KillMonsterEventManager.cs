using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WCell.Core;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.Util.Variables;

namespace WCell.RealmServer.Events.Asda2.Managers
{
  public static class KillMonsterEventManager
  {
    private static int _monsterId;
    private static int _itemId;

    [NotVariable]
    public static bool Started;

    public static Dictionary<uint,DateTime> CompletedCharacters = new Dictionary<uint, DateTime>(); 

    public static void Start(int monsterId, int itemId)
    {
      var template = Asda2ItemMgr.GetTemplate(itemId);
      if (Started || template == null)
        return;

      Asda2EventMgr.SendMessageToWorld("Kill monster event started.");
      Started = true;
      _itemId = itemId;
      _monsterId = monsterId;
    }

    public static void Stop()
    {
      Asda2EventMgr.SendMessageToWorld("Kill Monster Event ended.");
      lock (CompletedCharacters)
      {
        CompletedCharacters.Clear();
      }
      Started = false;
    }

    public static void TryGiveReward(Character chr, uint deadMonsterId)
    {
      if(deadMonsterId != _monsterId)
        return;

      lock (CompletedCharacters)
      {
        if (CompletedCharacters.ContainsKey(chr.EntryId) && CompletedCharacters[chr.EntryId].AddDays(1) > DateTime.Now)
        {
          chr.SendInfoMsg("You already complete Kill Monster Quest.");
        }
        else
        {
          CompletedCharacters.Add(chr.EntryId, DateTime.Now);

          RealmServer.IOQueue.AddMessage(
            () =>
              chr.Asda2Inventory.AddDonateItem(Asda2ItemMgr.GetTemplate(_itemId), 1,
                "kill_monster_event"));

          Log.Create(Log.Types.EventOperations, LogSourceType.Character, chr.EntryId)
            .AddAttribute("win", 0, "kill_monster_event")
            .Write();
        }
      }
    }
  }
}