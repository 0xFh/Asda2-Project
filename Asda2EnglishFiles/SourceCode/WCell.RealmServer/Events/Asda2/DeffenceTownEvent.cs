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
            this.MinLevel = minLevel;
            this.MaxLevel = maxLevel;
            this.Town = map.Id;
            this._map = map;
            this._amountMod = amountMod;
            this._healthMod = healthMod;
            this._otherStatsMod = otherStatsMod;
            this._speedMod = speedMod;
            this._difficulty = difficulty;
            this._spawnTasks = new SelfRunningTaskQueue(1000, "Defence town event " + (object) this.Town, false);
            NpcCustomEntries.Init(maxLevel, healthMod, otherStatsMod, speedMod);
            this.InitMovingPaths();
            this.InitMonsterSpawn(amountMod);
        }

        public virtual void Start()
        {
            if (this.Started)
                return;
            this.Started = true;
            Asda2EventMgr.SendMessageToWorld("Deffence town event started in {0}. [{1}-{2}]", (object) this.Town,
                (object) this.MinLevel, (object) this.MaxLevel);
            this._spawnTasks.IsRunning = true;
            this._map.DefenceTownEvent = this;
            foreach (NpcSpawnEntry npcEntry in this.NpcEntries)
            {
                NpcSpawnEntry entry = npcEntry;
                this._spawnTasks.CallDelayed(entry.TimeToSpawnMillis, (Action) (() =>
                {
                    NPC npc = entry.NpcEntry.SpawnAt(this._map, entry.MovingPoints[0], false);
                    npc.Brain.State = BrainState.DefenceTownEventMove;
                    npc.Brain.MovingPoints = entry.MovingPoints;
                    npc.Brain.DefaultState = BrainState.DefenceTownEventMove;
                }));
            }

            this._spawnTasks.CallDelayed(1000,
                (Action) (() => WCell.RealmServer.Global.World.BroadcastMsg("Event Manager",
                    string.Format("{0} town defence WAVE 1 stated.", (object) this._map.Name), Color.Red)));
            this._spawnTasks.CallDelayed(180000,
                (Action) (() => WCell.RealmServer.Global.World.BroadcastMsg("Event Manager",
                    string.Format("{0} town defence WAVE 2 stated.", (object) this._map.Name), Color.Red)));
            this._spawnTasks.CallDelayed(360000,
                (Action) (() => WCell.RealmServer.Global.World.BroadcastMsg("Event Manager",
                    string.Format("{0} town defence WAVE 3 stated.", (object) this._map.Name), Color.Red)));
            this._spawnTasks.CallDelayed(540000,
                (Action) (() => WCell.RealmServer.Global.World.BroadcastMsg("Event Manager",
                    string.Format("{0} town defence WAVE 4 stated.", (object) this._map.Name), Color.Red)));
            this._spawnTasks.CallDelayed(720000,
                (Action) (() => WCell.RealmServer.Global.World.BroadcastMsg("Event Manager",
                    string.Format("{0} town defence WAVE 5 stated.", (object) this._map.Name), Color.Red)));
            this._spawnTasks.CallDelayed(940000,
                (Action) (() => WCell.RealmServer.Global.World.BroadcastMsg("Event Manager",
                    string.Format("{0} town defence FINAL BOSS WAVE!", (object) this._map.Name), Color.Red)));
            this._spawnTasks.CallDelayed(1400000, (Action) (() => this.Stop(true)));
        }

        public virtual void Stop(bool success)
        {
            if (!this.Started)
                return;
            this.Started = false;
            Asda2EventMgr.SendMessageToWorld("Deffence town event stoped in {0} [{2}-{3}]. Status : {1}",
                (object) this.Town, success ? (object) "Win" : (object) "Loose", (object) this.MinLevel,
                (object) this.MaxLevel);
            this._spawnTasks.IsRunning = false;
            this._spawnTasks.Clear();
            this._map.DefenceTownEvent = (DeffenceTownEvent) null;
            if (success)
            {
                float num1 = (float) this._lives / (float) CharacterFormulas.DefenceTownLives;
                float num2 = (float) this.ExpPortionsTotal * num1;
                float num3 = (float) this.EventItemsTotal * num1;
                this.NotifyBestDamagers();
                List<object> objectList = new List<object>();
                Dictionary<object, long> dictionary = new Dictionary<object, long>();
                foreach (KeyValuePair<object, long> damage in this.Damages)
                {
                    Group key = damage.Key as Group;
                    if (key != null)
                    {
                        List<GroupMember> list = key.Where<GroupMember>((Func<GroupMember, bool>) (m =>
                        {
                            if (m.Character != null && m.Character.Level >= this.MinLevel)
                                return m.Character.Level <= this.MaxLevel;
                            return false;
                        })).ToList<GroupMember>();
                        foreach (GroupMember groupMember in list)
                        {
                            if (this.Damages.ContainsKey((object) groupMember.Character))
                                objectList.Add((object) groupMember.Character);
                            dictionary.Add((object) groupMember.Character, damage.Value / (long) list.Count);
                        }

                        objectList.Add(damage.Key);
                    }
                }

                WCell.RealmServer.Global.World.BroadcastMsg("Rewarder",
                    string.Format("Damagers count is {0}.", (object) this.Damages.Count), Color.OrangeRed);
                foreach (object key in objectList)
                    this.Damages.Remove(key);
                foreach (KeyValuePair<object, long> keyValuePair in dictionary)
                    this.Damages.Add(keyValuePair.Key, keyValuePair.Value);
                long num4 = this.Damages.Sum<KeyValuePair<object, long>>(
                    (Func<KeyValuePair<object, long>, long>) (d => d.Value));
                WCell.RealmServer.Global.World.BroadcastMsg("Rewarder",
                    string.Format("Total damage is {0}.", (object) num4), Color.OrangeRed);
                foreach (KeyValuePair<object, long> keyValuePair in (IEnumerable<KeyValuePair<object, long>>) this
                    .Damages.OrderByDescending<KeyValuePair<object, long>, long>(
                        (Func<KeyValuePair<object, long>, long>) (kvp => kvp.Value)))
                {
                    Character chr = keyValuePair.Key as Character;
                    try
                    {
                        if (chr != null)
                        {
                            if (chr.Asda2Inventory != null)
                            {
                                float num5 = (float) keyValuePair.Value / (float) num4;
                                int experience = (int) ((double) num5 * (double) num2 *
                                                        (double) XpGenerator.GetBaseExpForLevel(chr.Level));
                                int num6 = (int) ((double) num5 * (double) num3);
                                chr.GainXp(experience, "defence_town_event", false);
                                if (num6 > 0)
                                    ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                                        chr.Asda2Inventory.AddDonateItem(
                                            Asda2ItemMgr.GetTemplate(CharacterFormulas.DonationItemId), 100,
                                            "defence_town_event", false)));
                                WCell.RealmServer.Global.World.BroadcastMsg("Rw",
                                    string.Format("{2} gain {0} coins and {1} exp for {3} dmg.", (object) 100,
                                        (object) experience, (object) chr.Name, (object) keyValuePair.Value),
                                    Color.OrangeRed);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WCell.RealmServer.Global.World.BroadcastMsg("Event manager",
                            "exception when rewarding." + ex.Message, Color.Red);
                    }
                }
            }

            this.Damages.Clear();
        }

        private void NotifyBestDamagers()
        {
            List<KeyValuePair<object, long>> list = this.Damages
                .OrderByDescending<KeyValuePair<object, long>, long
                >((Func<KeyValuePair<object, long>, long>) (d => d.Value))
                .Take<KeyValuePair<object, long>>(CharacterFormulas.MaxDamagersDetailCount)
                .ToList<KeyValuePair<object, long>>();
            WCell.RealmServer.Global.World.BroadcastMsg("Event Mgr",
                string.Format("Top {0} damagers are : ", (object) CharacterFormulas.MaxDamagersDetailCount),
                Color.MediumVioletRed);
            int num1 = 1;
            foreach (KeyValuePair<object, long> keyValuePair in list)
            {
                Group key1 = keyValuePair.Key as Group;
                if (key1 != null)
                {
                    if (key1.Leader != null && key1.Leader.Character != null)
                        WCell.RealmServer.Global.World.BroadcastMsg("Event Mgr",
                            string.Format("{0} Party [{1}] deal {2} dmg", (object) num1,
                                (object) key1.Leader.Character.Name, (object) (int) keyValuePair.Value),
                            Color.GreenYellow);
                    foreach (GroupMember groupMember in key1)
                    {
                        int num2 = 0;
                        if (groupMember.Character != null && this.Damages.ContainsKey((object) groupMember.Character))
                            num2 = (int) this.Damages[(object) groupMember.Character];
                        WCell.RealmServer.Global.World.BroadcastMsg("Event Mgr",
                            string.Format("--- {0} deal {1} dmg", (object) groupMember.Name, (object) num2),
                            Color.LightGreen);
                    }
                }
                else
                {
                    Character key2 = keyValuePair.Key as Character;
                    if (key2 != null)
                        WCell.RealmServer.Global.World.BroadcastMsg("Event Mgr",
                            string.Format("{2} Char [{0}] deal {1} dmg", (object) key2.Name,
                                (object) (int) keyValuePair.Value, (object) num1), Color.GreenYellow);
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
            this.MovingPaths.Add(movingPath);
            return movingPath;
        }

        protected void AddSpawnEntry(NpcCustomEntryId id, int timeToSpawnsecs, int amount, float amountMod,
            BrainState brainState = BrainState.DefenceTownEventMove)
        {
            int num1 = 0;
            int num2 = (int) ((double) amount * (double) amountMod);
            for (int index1 = 1; index1 <= num2; ++index1)
            {
                int index2 = index1 % this.MovingPaths.Count;
                if (index2 == 0)
                    num1 += 2000;
                NPCEntry entry = NPCMgr.GetEntry((uint) id);
                if (entry.Rank >= CreatureRank.Boss)
                    index2 = Utility.Random(0, this.MovingPaths.Count - 1);
                this.NpcEntries.Add(new NpcSpawnEntry()
                {
                    BrainState = brainState,
                    TimeToSpawnMillis = timeToSpawnsecs * 1000 + num1,
                    NpcEntry = entry,
                    MovingPoints = (List<Vector3>) this.MovingPaths[index2]
                });
            }
        }

        public void SubstractPoints(int value)
        {
            this._lives -= value;
            if (this._lives > 0)
                WCell.RealmServer.Global.World.BroadcastMsg("Event manager",
                    string.Format("Town {0} lost {1} lives. {2} lives left.", (object) this._map.Name, (object) value,
                        (object) this._lives), Color.Red);
            else
                this.Stop(false);
        }
    }
}