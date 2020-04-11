using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core;
using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Events.Asda2
{
  public abstract class DeffenceTownEvent
  {
    private int _lives = CharacterFormulas.DefenceTownLives;
    protected List<NpcSpawnEntry> NpcEntries = new List<NpcSpawnEntry>();
    protected List<MovingPath> MovingPaths = new List<MovingPath>();
    public Dictionary<object, long> Damages = new Dictionary<object, long>();
    private readonly SelfRunningTaskQueue _spawnTasks;
    protected MapId Town;
    protected readonly Map _map;
    protected readonly float _amountMod;
    protected readonly float _healthMod;
    protected readonly float _otherStatsMod;
    protected readonly float _speedMod;
    protected readonly float _difficulty;

    protected bool Started { get; set; }

    protected int MaxLevel { get; private set; }

    protected int MinLevel { get; private set; }

    protected abstract int ExpPortionsTotal { get; }

    protected abstract int EventItemsTotal { get; }

    protected DeffenceTownEvent(Map map, int minLevel, int maxLevel, float amountMod, float healthMod,
      float otherStatsMod, float speedMod, float difficulty)
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
      if(Started)
        return;
      Started = true;
      Asda2EventMgr.SendMessageToWorld("Deffence town event started in {0}. [{1}-{2}]", (object) Town,
        (object) MinLevel, (object) MaxLevel);
      _spawnTasks.IsRunning = true;
      _map.DefenceTownEvent = this;
      foreach(NpcSpawnEntry npcEntry in NpcEntries)
      {
        NpcSpawnEntry entry = npcEntry;
        _spawnTasks.CallDelayed(entry.TimeToSpawnMillis, () =>
        {
          NPC npc = entry.NpcEntry.SpawnAt(_map, entry.MovingPoints[0], false);
          npc.Brain.State = BrainState.DefenceTownEventMove;
          npc.Brain.MovingPoints = entry.MovingPoints;
          npc.Brain.DefaultState = BrainState.DefenceTownEventMove;
        });
      }

      _spawnTasks.CallDelayed(1000,
        () => World.BroadcastMsg("Event Manager",
          string.Format("{0} town defence WAVE 1 stated.", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(180000,
        () => World.BroadcastMsg("Event Manager",
          string.Format("{0} town defence WAVE 2 stated.", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(360000,
        () => World.BroadcastMsg("Event Manager",
          string.Format("{0} town defence WAVE 3 stated.", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(540000,
        () => World.BroadcastMsg("Event Manager",
          string.Format("{0} town defence WAVE 4 stated.", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(720000,
        () => World.BroadcastMsg("Event Manager",
          string.Format("{0} town defence WAVE 5 stated.", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(940000,
        () => World.BroadcastMsg("Event Manager",
          string.Format("{0} town defence FINAL BOSS WAVE!", _map.Name), Color.Red));
      _spawnTasks.CallDelayed(1400000, () => Stop(true));
    }

    public virtual void Stop(bool success)
    {
      if(!Started)
        return;
      Started = false;
      Asda2EventMgr.SendMessageToWorld("Deffence town event stoped in {0} [{2}-{3}]. Status : {1}",
        (object) Town, success ? (object) "Win" : (object) "Loose", (object) MinLevel,
        (object) MaxLevel);
      _spawnTasks.IsRunning = false;
      _spawnTasks.Clear();
      _map.DefenceTownEvent = null;
      if(success)
      {
        float num1 = _lives / (float) CharacterFormulas.DefenceTownLives;
        float num2 = ExpPortionsTotal * num1;
        float num3 = EventItemsTotal * num1;
        NotifyBestDamagers();
        List<object> objectList = new List<object>();
        Dictionary<object, long> dictionary = new Dictionary<object, long>();
        foreach(KeyValuePair<object, long> damage in Damages)
        {
          Group key = damage.Key as Group;
          if(key != null)
          {
            List<GroupMember> list = key.Where(m =>
            {
              if(m.Character != null && m.Character.Level >= MinLevel)
                return m.Character.Level <= MaxLevel;
              return false;
            }).ToList();
            foreach(GroupMember groupMember in list)
            {
              if(Damages.ContainsKey(groupMember.Character))
                objectList.Add(groupMember.Character);
              dictionary.Add(groupMember.Character, damage.Value / list.Count);
            }

            objectList.Add(damage.Key);
          }
        }

        World.BroadcastMsg("Rewarder",
          string.Format("Damagers count is {0}.", Damages.Count), Color.OrangeRed);
        foreach(object key in objectList)
          Damages.Remove(key);
        foreach(KeyValuePair<object, long> keyValuePair in dictionary)
          Damages.Add(keyValuePair.Key, keyValuePair.Value);
        long num4 = Damages.Sum(
          d => d.Value);
        World.BroadcastMsg("Rewarder",
          string.Format("Total damage is {0}.", num4), Color.OrangeRed);
        foreach(KeyValuePair<object, long> keyValuePair in Damages.OrderByDescending(
          kvp => kvp.Value))
        {
          Character chr = keyValuePair.Key as Character;
          try
          {
            if(chr != null)
            {
              if(chr.Asda2Inventory != null)
              {
                float num5 = keyValuePair.Value / (float) num4;
                int experience = (int) (num5 * (double) num2 *
                                        XpGenerator.GetBaseExpForLevel(chr.Level));
                int num6 = (int) (num5 * (double) num3);
                chr.GainXp(experience, "defence_town_event", false);
                if(num6 > 0)
                  ServerApp<RealmServer>.IOQueue.AddMessage(() =>
                    chr.Asda2Inventory.AddDonateItem(
                      Asda2ItemMgr.GetTemplate(CharacterFormulas.DonationItemId), 100,
                      "defence_town_event", false));
                World.BroadcastMsg("Rw",
                  string.Format("{2} gain {0} coins and {1} exp for {3} dmg.", (object) 100,
                    (object) experience, (object) chr.Name, (object) keyValuePair.Value),
                  Color.OrangeRed);
              }
            }
          }
          catch(Exception ex)
          {
            World.BroadcastMsg("Event manager",
              "exception when rewarding." + ex.Message, Color.Red);
          }
        }
      }

      Damages.Clear();
    }

    private void NotifyBestDamagers()
    {
      List<KeyValuePair<object, long>> list = Damages
        .OrderByDescending(d => d.Value)
        .Take(CharacterFormulas.MaxDamagersDetailCount)
        .ToList();
      World.BroadcastMsg("Event Mgr",
        string.Format("Top {0} damagers are : ", CharacterFormulas.MaxDamagersDetailCount),
        Color.MediumVioletRed);
      int num1 = 1;
      foreach(KeyValuePair<object, long> keyValuePair in list)
      {
        Group key1 = keyValuePair.Key as Group;
        if(key1 != null)
        {
          if(key1.Leader != null && key1.Leader.Character != null)
            World.BroadcastMsg("Event Mgr",
              string.Format("{0} Party [{1}] deal {2} dmg", num1,
                key1.Leader.Character.Name, (int) keyValuePair.Value),
              Color.GreenYellow);
          foreach(GroupMember groupMember in key1)
          {
            int num2 = 0;
            if(groupMember.Character != null && Damages.ContainsKey(groupMember.Character))
              num2 = (int) Damages[groupMember.Character];
            World.BroadcastMsg("Event Mgr",
              string.Format("--- {0} deal {1} dmg", groupMember.Name, num2),
              Color.LightGreen);
          }
        }
        else
        {
          Character key2 = keyValuePair.Key as Character;
          if(key2 != null)
            World.BroadcastMsg("Event Mgr",
              string.Format("{2} Char [{0}] deal {1} dmg", key2.Name,
                (int) keyValuePair.Value, num1), Color.GreenYellow);
        }

        ++num1;
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
      MovingPath movingPath = new MovingPath((int) map.Id * 1000);
      MovingPaths.Add(movingPath);
      return movingPath;
    }

    protected void AddSpawnEntry(NpcCustomEntryId id, int timeToSpawnsecs, int amount, float amountMod,
      BrainState brainState = BrainState.DefenceTownEventMove)
    {
      int num1 = 0;
      int num2 = (int) (amount * (double) amountMod);
      for(int index1 = 1; index1 <= num2; ++index1)
      {
        int index2 = index1 % MovingPaths.Count;
        if(index2 == 0)
          num1 += 2000;
        NPCEntry entry = NPCMgr.GetEntry((uint) id);
        if(entry.Rank >= CreatureRank.Boss)
          index2 = Utility.Random(0, MovingPaths.Count - 1);
        NpcEntries.Add(new NpcSpawnEntry
        {
          BrainState = brainState,
          TimeToSpawnMillis = timeToSpawnsecs * 1000 + num1,
          NpcEntry = entry,
          MovingPoints = MovingPaths[index2]
        });
      }
    }

    public void SubstractPoints(int value)
    {
      _lives -= value;
      if(_lives > 0)
        World.BroadcastMsg("Event manager",
          string.Format("Town {0} lost {1} lives. {2} lives left.", _map.Name, value,
            _lives), Color.Red);
      else
        Stop(false);
    }
  }
}