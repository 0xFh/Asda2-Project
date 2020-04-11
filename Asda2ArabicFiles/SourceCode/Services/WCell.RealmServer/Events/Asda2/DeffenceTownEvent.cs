using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core;
using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Events.Asda2.Managers;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Events.Asda2
{
  public abstract class DeffenceTownEvent
  {
    private readonly SelfRunningTaskQueue _spawnTasks;
    protected MapId Town;
    protected readonly Map _map;
    protected readonly float _amountMod;
    protected readonly float _healthMod;
    protected readonly float _otherStatsMod;
    protected readonly float _speedMod;
    protected readonly float _difficulty;
    int _lives = CharacterFormulas.DefenceTownLives;
    protected List<NpcSpawnEntry> NpcEntries = new List<NpcSpawnEntry>();
    protected List<MovingPath> MovingPaths = new List<MovingPath>();
    protected bool Started { get; set; }
    protected int MaxLevel { get; private set; }
    protected int MinLevel { get; private set; }
    protected abstract int ExpPortionsTotal { get; }
    protected abstract int EventItemsTotal { get; }
    public Dictionary<object, long> Damages = new Dictionary<object, long>();


    protected DeffenceTownEvent(Map map, int minLevel, int maxLevel, float amountMod, float healthMod, float otherStatsMod, float speedMod, float difficulty)
    {
      MinLevel = minLevel;
      MaxLevel = maxLevel;
      Town = map.Id;
      _map = map;
      _amountMod = amountMod;
      _healthMod = healthMod;
      _otherStatsMod = otherStatsMod;
      _speedMod = speedMod;
      _difficulty = difficulty;

      _spawnTasks = new SelfRunningTaskQueue(1000, "Defence town event " + Town, false);
      NpcCustomEntries.Init(maxLevel, healthMod, otherStatsMod, speedMod);
      InitMovingPaths();
      InitMonsterSpawn(amountMod);
    }



    public virtual void Start()
    {
      if (Started) return;
      Started = true;
      Asda2EventMgr.SendMessageToWorld("Deffence town event started in {0}. [{1}-{2}]", Town, MinLevel, MaxLevel);
      _spawnTasks.IsRunning = true;
      _map.DefenceTownEvent = this;
      foreach (var npcSpawnEntry in NpcEntries)
      {
        var entry = npcSpawnEntry;
        _spawnTasks.CallDelayed(entry.TimeToSpawnMillis, () =>
        {
          var npc = entry.NpcEntry.SpawnAt(_map, entry.MovingPoints[0]);
          npc.Brain.State = BrainState.DefenceTownEventMove;
          npc.Brain.MovingPoints = entry.MovingPoints;
          npc.Brain.DefaultState = BrainState.DefenceTownEventMove;
        });
      }

      _spawnTasks.CallDelayed(1000, () => World.BroadcastMsg("Event Manager", string.Format("{0} town defence WAVE 1 stated.", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(180000, () => World.BroadcastMsg("Event Manager", string.Format("{0} town defence WAVE 2 stated.", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(360000, () => World.BroadcastMsg("Event Manager", string.Format("{0} town defence WAVE 3 stated.", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(540000, () => World.BroadcastMsg("Event Manager", string.Format("{0} town defence WAVE 4 stated.", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(720000, () => World.BroadcastMsg("Event Manager", string.Format("{0} town defence WAVE 5 stated.", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(940000, () => World.BroadcastMsg("Event Manager", string.Format("{0} town defence FINAL BOSS WAVE!", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(1400000, () => Stop(true));
    }



    public virtual void Stop(bool success)
    {
      if (!Started) return;
      Started = false;
      Asda2EventMgr.SendMessageToWorld("Deffence town event stoped in {0} [{2}-{3}]. Status : {1}", Town, success ? "Win" : "Loose", MinLevel, MaxLevel);
      _spawnTasks.IsRunning = false;
      _spawnTasks.Clear();
      _map.DefenceTownEvent = null;
      if (success)//getting prizies to players
      {
        var livesPrc = (float)_lives / CharacterFormulas.DefenceTownLives;
        var expPortionsTotal = ExpPortionsTotal * livesPrc;
        var eventItemsTotal = EventItemsTotal * livesPrc;
        NotifyBestDamagers();
        var objectsToDelete = new List<object>();
        var objectsToAdd = new Dictionary<object, long>();
        foreach (var damage in Damages)
        {
          var grp = damage.Key as Group;
          if (grp != null)
          {
            var receivers = grp.Where(m => m.Character != null && m.Character.Level >= MinLevel && m.Character.Level <= MaxLevel).ToList();
            foreach (var member in receivers)
            {
              if (Damages.ContainsKey(member.Character))
              {
                objectsToDelete.Add(member.Character);
              }
              objectsToAdd.Add(member.Character, damage.Value / receivers.Count);
            }
            objectsToDelete.Add(damage.Key);
          }
        }
        World.BroadcastMsg("Rewarder", string.Format("Damagers count is {0}.", Damages.Count), Color.OrangeRed);
        foreach (var o in objectsToDelete)
        {
          Damages.Remove(o);
        }
        foreach (var l in objectsToAdd)
        {
          Damages.Add(l.Key, l.Value);
        }
        var totalDamage = Damages.Sum(d => d.Value);
        World.BroadcastMsg("Rewarder", string.Format("Total damage is {0}.", totalDamage), Color.OrangeRed);
        foreach (var damage in Damages.OrderByDescending(kvp => kvp.Value))
        {
          var chr = damage.Key as Character;
          try
          {
            if (chr != null && chr.Asda2Inventory != null)
            {
              var prcFromMainPrize = (float)damage.Value / totalDamage;
              var exp = (int)(prcFromMainPrize * expPortionsTotal * XpGenerator.GetBaseExpForLevel(chr.Level));
              var eventItemsCount = (int)(prcFromMainPrize * eventItemsTotal);
              chr.GainXp(exp, "defence_town_event");
              if (eventItemsCount > 0)
                RealmServer.IOQueue.AddMessage(() => chr.Asda2Inventory.AddDonateItem(Asda2ItemMgr.GetTemplate(CharacterFormulas.EventItemId), eventItemsCount, "defence_town_event"));
              World.BroadcastMsg("Rw", string.Format("{2} gain {0} coins and {1} exp for {3} dmg.", eventItemsCount, exp, chr.Name, damage.Value), Color.OrangeRed);
            }
          }
          catch (Exception ex)
          {
            World.BroadcastMsg("Event manager", "exception when rewarding." + ex.Message, Color.Red);
          }
        }
      }
      Damages.Clear();
    }

    private void NotifyBestDamagers()
    {
      var possibleLotters = Damages.OrderByDescending(d => d.Value).Take(CharacterFormulas.MaxDamagersDetailCount).ToList();
      World.BroadcastMsg("Event Mgr", string.Format("Top {0} damagers are : ", CharacterFormulas.MaxDamagersDetailCount), Color.MediumVioletRed);
      var i = 1;
      foreach (var possibleLotter in possibleLotters)
      {
        var group = possibleLotter.Key as Group;
        if (@group != null)
        {
          if (group.Leader == null || group.Leader.Character == null)
          {

          }
          else
          {
            World.BroadcastMsg("Event Mgr",
              string.Format("{0} Party [{1}] deal {2} dmg", i, @group.Leader.Character.Name,
                (int)possibleLotter.Value), Color.GreenYellow);
          }
          foreach (var member in @group)
          {
            int dmg = 0;
            if (member.Character != null)
            {
              if (Damages.ContainsKey(member.Character))
                dmg = (int)Damages[member.Character];
            }
            World.BroadcastMsg("Event Mgr", string.Format("--- {0} deal {1} dmg", member.Name, dmg), Color.LightGreen);
          }
        }
        else
        {
          var chr = possibleLotter.Key as Character;
          if (chr != null)
          {
            World.BroadcastMsg("Event Mgr",
              string.Format("{2} Char [{0}] deal {1} dmg", chr.Name, (int)possibleLotter.Value, i),
              Color.GreenYellow);
          }
        }
        i++;
      }
    }

    protected virtual void InitMonsterSpawn(float amountMod)
    {

    }
    protected virtual void InitMovingPaths()
    {

    }
    protected MovingPath AddMovingPath(Map map)
    {
      var path = new MovingPath((int)map.Id * 1000);
      MovingPaths.Add(path);
      return path;
    }

    protected void AddSpawnEntry(NpcCustomEntryId id, int timeToSpawnsecs, int amount, float amountMod, BrainState brainState = BrainState.DefenceTownEventMove)
    {
      var additionalTime = 0;
      var amountToSpawn = (int)(amount * amountMod);
      for (int i = 1; i <= amountToSpawn; i++)
      {
        var index = i % MovingPaths.Count;
        if (index == 0)
          additionalTime += 2000;

        var npcEntry = NPCMgr.GetEntry((uint)id);
        if (npcEntry.Rank >= CreatureRank.Boss)
        {
          index = Util.Utility.Random(0, MovingPaths.Count - 1);
        }
        var entry = new NpcSpawnEntry { BrainState = brainState, TimeToSpawnMillis = timeToSpawnsecs * 1000 + additionalTime, NpcEntry = npcEntry, MovingPoints = MovingPaths[index] };
        NpcEntries.Add(entry);
      }
    }

    public void SubstractPoints(int value)
    {
      _lives -= value;
      if (_lives > 0)
      {
        World.BroadcastMsg("Event manager", string.Format("Town {0} lost {1} lives. {2} lives left.", _map.Name, value, _lives), Color.Red);
      }
      else
      {
        Stop(false);
      }
    }
  }
}