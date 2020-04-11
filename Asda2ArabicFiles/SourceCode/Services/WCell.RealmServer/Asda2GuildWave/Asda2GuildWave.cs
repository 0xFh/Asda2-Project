using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.AI;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Asda2GuildWave
{
    class Asda2GuildWave
    {
        private const int mapId = 28;
        private const int mapCenterX = 64;
        private const int mapCenterY = 66;
        private const int mapPlayerSpawnX = 50;
        private const int mapPlayerSpawnY = 60;

        private static int[] waveTimers = new int[]
        {
            5 * 60 * 1000,
            5 * 60 * 1000,
            5 * 60 * 1000,
            5 * 60 * 1000,
            5 * 60 * 1000,
            5 * 60 * 1000,
            5 * 60 * 1000,
            5 * 60 * 1000,
            5 * 60 * 1000 + 30 * 1000,
            5 * 60 * 1000,
            5 * 60 * 1000,
            5 * 60 * 1000 + 30 * 1000,
            5 * 60 * 1000 + 30 * 1000,
            10 * 60 * 1000,
            10 * 60 * 1000 + 30 * 1000,
            20 * 60 * 1000,
            20 * 60 * 1000,
            20 * 60 * 1000,
            20 * 60 * 1000,
            20 * 60 * 1000,
            30 * 60 * 1000,
            30 * 60 * 1000,
            30 * 60 * 1000,
            30 * 60 * 1000,
            30 * 60 * 1000,
            30 * 60 * 1000,
            30 * 60 * 1000,
            30 * 60 * 1000,
            30 * 60 * 1000,
            40 * 60 * 1000
        };

        private static List<Asda2GuildWaveNpc>[] npcTemplates = new List<Asda2GuildWaveNpc>[30];
        private static List<NPCEntry> npcTemplate = new List<NPCEntry>();
        private static bool _npcTemplatesCreated = false;
        private static List<NPC> currentWaveNpcs = new List<NPC>();

        private const int defaultTimeToSpawnNpcs = 15 * 1000;
        private static int timeToSpawnNpcs = defaultTimeToSpawnNpcs;
        private static int npcsCount1 = 0;
        private static int npcsCount2 = 0;
        public static int Diffaclity;
        public static void GetDiffeclity(int diffeclity)
        {
            Diffaclity = diffeclity;
        }
        private static Random random = new Random();

        public static void InitGuildWaveNpcList()
        {
            if (_npcTemplatesCreated) { return; }

            int waveNumber = 0;

            for (int i = 0; i < 30; ++i)
            {
                npcTemplates[i] = new List<Asda2GuildWaveNpc>();

            }


            // Wave 1
            
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(499, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(500, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(501, 10));
            ++waveNumber;

            // Wave 2
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(500, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(501, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(503, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(504, 10));
            ++waveNumber;

            // Wave 3
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(508, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(509, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(510, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(504, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(512, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(513, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(514, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(515, 10));
            ++waveNumber;

            // Wave 4
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(504, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(503, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(501, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(500, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(510, 20));
            ++waveNumber;

            // Wave 5
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(508, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(509, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(510, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(512, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(513, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(514, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(515, 5));
            ++waveNumber;

            // Wave 6
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(504, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(506, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(515, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(516, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(517, 5));
            ++waveNumber;

            // Wave 7
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(517, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(516, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(518, 5));
            ++waveNumber;

            // Wave 8
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(510, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(515, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(504, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(518, 1));
            ++waveNumber;

            // Wave 9
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(450, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(230, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(247, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(506, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(504, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(518, 3));
            ++waveNumber;

            // Wave 10
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(450, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(230, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(247, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(506, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(504, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(516, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(517, 10));
            ++waveNumber;

            // Wave 11
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(230, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(247, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(518, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(522, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(523, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(524, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(504, 5));
            ++waveNumber;

            // Wave 12
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(518, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(715, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(716, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(520, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(717, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(764, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(525, 3));
            ++waveNumber;

            // Wave 13
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(516, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(517, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(520, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(518, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(230, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(247, 15));
            //npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(496, 5));
            ++waveNumber;

            // Wave 14
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(520, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(715, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(716, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(717, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(764, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(525, 5));
            ++waveNumber;

            // Wave 15
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(450, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(486, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(230, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(247, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(520, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(764, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(711, 5));
            ++waveNumber;

            // Wave 16
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(520, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(764, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(486, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(230, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(247, 10));
            ++waveNumber;

            // Wave 17
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(230, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(247, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(520, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(764, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(450, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(468, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(469, 15));
            ++waveNumber;

            // Wave 18
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(230, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(247, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(520, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(764, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(486, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(468, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(469, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(711, 5));
            ++waveNumber;

            // Wave 19
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(512, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(513, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(514, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(390, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(136, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(381, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(515, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(383, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(503, 1));
            ++waveNumber;

            // Wave 20
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(515, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(510, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(383, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(136, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(523, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(519, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(522, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(524, 1));
            ++waveNumber;

            // Wave 21
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(512, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(381, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(383, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(136, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(390, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(514, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(513, 1));
            ++waveNumber;

            // Wave 22
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(172, 4));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(164, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(183, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(639, 10));
            ++waveNumber;

            // Wave 23
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(353, 4));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(354, 4));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(355, 4));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(356, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(360, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(639, 15));
            ++waveNumber;

            // Wave 24
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(517, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(363, 6));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(364, 6));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(522, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(523, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(524, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(525, 1));
            ++waveNumber;

            // Wave 25
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(518, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(515, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(510, 4));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(525, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(519, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(711, 1));
            ++waveNumber;

            // Wave 26
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(735, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(730, 4));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(414, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(410, 5));
            ++waveNumber;

            // Wave 27
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(705, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(701, 13));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(636, 20));
            ++waveNumber;

            // Wave 28
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(719, 15));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(136, 10));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(636, 20));
            ++waveNumber;

            // Wave 29
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(510, 5));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(515, 4));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(518, 3));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(525, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(711, 1));
            ++waveNumber;

            // Wave 30
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(509, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(508, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(510, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(512, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(513, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(514, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(515, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(516, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(517, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(518, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(522, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(523, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(524, 2));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(525, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(711, 1));
            npcTemplates[waveNumber].Add(new Asda2GuildWaveNpc(470, 1));

            _npcTemplatesCreated = true;
        }


        //private const int defaultRegistrationPreparationTime = 5 * 60 * 1000;
        private int timeToRegistrationPreparationFinish = Asda2GuildWaveMgr.defaultRegistrationPreparationTime;

        private const int defaultTimeToPrepare = 60 * 1000;
        private const int defaultTimeToPrepareAfter20 = 70 * 1000;
        private int timeToFight = 0;
        private int timeToPrepare = 0;

        private int currentWaveNumber = 0;

        private int _guildId;
        private int _difficulty;
        private bool _inProgress = false;
        private bool _fail = false;
        private bool _registered = false;
        private bool _finished = false;

        private bool preparation = false;

        private bool terminating = false;

        private Guild _guild;

        private Map _map;

        private List<Character> playersRegistered = new List<Character>();

        private object _playersRegisteredLock = new object();

        public int GuildId
        {
            get { return _guildId; }
        }

        public Guild Guild
        {
            get { return _guild; }
        }

        public int Difficulty
        {
            get { return _difficulty; }
        }

        public bool IsInProgress
        {
            get { return _inProgress; }
        }

        public bool IsFailed
        {
            get { return _fail; }
        }

        public bool IsFinished
        {
            get { return _finished; }
        }

        public bool RegistrationFinished
        {
            get { return _registered; }
        }

        public int PlayersRegisteredCount
        {
            get { return playersRegistered.Count; }
        }

        public int LastWinnedWave
        {
            get { return currentWaveNumber; }
        }

        public Asda2GuildWave(int guildId, int difficulty, Guild guild)
        {
            this._guildId = guildId;
            this._difficulty = difficulty;
            this._guild = guild;
            this._map = new Map(World.GetMapTemplate((MapId)mapId));
           // Asda2GuildWave.GetDiffeclity(difficulty);

            _map.InitMap();
            _map.Start();
        }

        public void PrepareNpcEntriesList()
        {
            foreach (Asda2GuildWaveNpc npc in npcTemplates[currentWaveNumber])
            {
                for (int i = 0; i < npc.NpcCount; ++i)
                {
                    var entry = NPCMgr.GetEntry((uint)npc.NpcId);
                    //entry.SetHealth((entry.MaxHealth * (Difficulty == 0 ? 2 : Difficulty == 1 ? 4 : Difficulty == 2 ? 6 : 1)));
                   // npcTemplate.Add(entry);
                   if (Difficulty == 0)
                    {
                        entry.SetHealth(entry.MaxHealth);
                        entry.SetDamage(entry.MaxDamage);
                    }
                   else if (Difficulty == 1)
                    {
                        entry.SetHealth(entry.MaxHealth);
                        entry.SetDamage(entry.MaxDamage);
                    }
                   else if (Difficulty == 2)
                    {
                        entry.SetHealth(entry.MaxHealth);
                        entry.SetDamage(entry.MaxDamage);
                    }
                   else
                    {
                        npcTemplate.Add(entry);
                    }
                    npcTemplate.Add(entry);
                }
            }

            npcsCount1 = npcTemplate.Count / 2;
            npcsCount2 = npcTemplate.Count - npcsCount1;
        }

        public void SpawnNpcsForCurrentWave(int stepNumber) // 0 - Начало волны, 1 - Спустя некоторое время
        {
            int npcsCount = stepNumber == 0 ? npcsCount1 : npcsCount2;

            for(int i = 0; i < npcsCount; ++i)
            {
                NPCEntry entry = npcTemplate[0];

                int offsetX = -15 + random.Next(31);
                int offsetY = -15 + random.Next(31);

                var pos = new Vector3(offsetX + mapCenterX + _map.Offset, offsetY + mapCenterY + _map.Offset, 0);
                var wl = new WorldLocation(_map, pos);
                entry.IsAgressive = true;
                //float healh = ((getAverageLevel() / 50) > 1 ? getAverageLevel() / 50 : entry.MaxHealth) * (Difficulty == 0 ? 2 : Difficulty == 1 ? 4 : Difficulty == 2 ? 6 : 1);
                //entry.SetHealth((entry.MaxHealth * (Difficulty == 0 ? 2 : Difficulty == 1 ? 4 : Difficulty == 2 ? 6 : 1)));
                var newNpc = entry.SpawnAt(wl);
                newNpc.IsAsda2GuildWave = true;
                _map.CallDelayed(100, () => newNpc.Movement.MoveTo(pos));
                npcTemplate.Remove(entry);
                currentWaveNpcs.Add(newNpc);
            }
        }

        public void AddRegisteringPlayer(Character chr)
        {
            lock(_playersRegisteredLock)
            {
                playersRegistered.Add(chr);
            }
        }

        public void RemoveRegisteringPlayer(Character chr)
        {
            lock(_playersRegisteredLock)
            {
                playersRegistered.Remove(chr);
            }
        }

        public bool IsInRegisteringPlayer(Character chr)
        {
            lock (_playersRegisteredLock)
            {
                if (playersRegistered.Contains(chr))
                {
                    return true;
                }
                return false;
            }
        }

        public void AddEnteredPlayer(Character chr)
        {
            float? o = null;
            var pos = new Vector3(mapPlayerSpawnX + _map.Offset, mapPlayerSpawnY + _map.Offset, 0);
            chr.TeleportTo(_map, ref pos, o);
        }

        public void PostAddEnteredPlayer(Character chr)
        {
            Asda2GuildWaveHandler.GuildWaveUpdateEnter(chr.Client, timeToRegistrationPreparationFinish);
        }

        public void ExitGuildWave(Character chr)
        {
            lock(_playersRegisteredLock)
            {
                playersRegistered.Remove(chr);
            }

            float? o = null;
            Map map = null;
            map = World.GetNonInstancedMap((Constants.World.MapId)3);
            var x = 65;
            var y = 344;
            var pos = new Vector3(x + map.Offset, y + map.Offset, 0);
            chr.TeleportTo(map, ref pos, o);
        }

        public void RemoveEnteredPlayer(Character chr)
        {
            playersRegistered.Remove(chr);
            // TODO Пакет отмены гилд вейва
            float? o = null;
            Map map = null;
            map = World.GetNonInstancedMap((Constants.World.MapId)3);
            var x = 65;
            var y = 344;
            var pos = new Vector3(x + map.Offset, y + map.Offset, 0);
            chr.TeleportTo(map, ref pos, o);
        }

        public void ClearPlayersRegisteredList()
        {
            lock(_playersRegisteredLock)
                {
                    foreach(Character chr in playersRegistered)
                    {
                        if(chr == null)
                        {
                            playersRegistered.Remove(chr);
                            continue;
                        }
                        if (chr.MapId != Constants.World.MapId.Guildwave)
                        {
                            playersRegistered.Remove(chr);
                        }
                    }
                }
        }

        public bool isPlayerRegistered(Character chr)
        {
            lock(_playersRegisteredLock)
            {
                foreach(Character _chr in playersRegistered)
                {
                    if(_chr == chr)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void UpdateGuildWave(int dt)
        {
            if (terminating) { return; }

            timeToRegistrationPreparationFinish -= dt;

            if(timeToRegistrationPreparationFinish <= 0)
            {
                if (!_registered)
                {
                    if (PlayersRegisteredCount < 1)
                    {
                        Asda2GuildWaveHandler.GuildWaveEnterStatusToGuild(_guild, GuildWaveEnterStatus.Fail);
                        _fail = true;
                        lock (_playersRegisteredLock)
                        {
                            playersRegistered.Clear();
                            _map.Stop();
                        }
                    }
                    else
                    {
                        Asda2GuildWaveHandler.GuildWaveEnterStatusToGuild(_guild, GuildWaveEnterStatus.Ok);
                        _registered = true;
                        timeToRegistrationPreparationFinish = Asda2GuildWaveMgr.defaultRegistrationPreparationTime;
                    }
                }
                else
                {
                    if(!_inProgress)
                    {
                        timeToFight = waveTimers[currentWaveNumber];
                        timeToPrepare = 0;
                        preparation = false;

                        ClearPlayersRegisteredList();

                        /*if (PlayersRegisteredCount < Asda2GuildWaveMgr.NeedPlayerToStartGW)
                        {
                            Asda2GuildWaveHandler.GuildWaveEnterStatusToGuild(_guild, GuildWaveEnterStatus.Fail);
                            lock (_playersRegisteredLock)
                            {
                                foreach (Character chr in playersRegistered.ToList())
                                {
                                    ExitGuildWave(chr);
                                }
                                _map.Stop();
                                _fail = true;
                                terminating = true;
                                return;
                            }
                        }*/


                        Asda2GuildWaveHandler.GuildWaveEnterStatusToGuild(_guild, GuildWaveEnterStatus.AlreadyStarted);
                        Asda2GuildWaveHandler.GuildWaveUpdate(this, GuildWaveUpdateStatus.Start, currentWaveNumber, timeToFight);

                        PrepareNpcEntriesList();
                        SpawnNpcsForCurrentWave(0);
                        timeToSpawnNpcs = defaultTimeToSpawnNpcs;

                        _inProgress = true;
                    }
                }
            }

            if(!_inProgress)
            {
                return;
            }

            lock(_playersRegisteredLock)
            {
                for (int i = 0; i < playersRegistered.Count; ++i)
                {
                    Character chr = playersRegistered[i];

                    if(World.GetCharacter(chr.Name, false) == null)
                    {
                        playersRegistered.RemoveAt(i--);
                    }
                }

                if(playersRegistered.Count <= 0)
                {
                    Guild.WaveLimit = (Guild.WaveLimit + 1);
                    currentWaveNpcs.Clear();

                    _map.CallOnAllNPCs((NPC npc) =>
                    {
                        npc.Kill();
                    });

                    _map.Stop();

                    _finished = true;
                    terminating = true;
                    return;
                }
            }

            if(!preparation)
            {
                timeToFight -= dt;

                if(timeToFight <= 0)
                {
                    currentWaveNpcs.Clear();

                    ClearPlayersRegisteredList();

                    lock (_playersRegisteredLock)
                    {
                        foreach (Character chr in playersRegistered)
                        {
                            GuildWaveResultAndItems(chr);
                        }
                    }

                    _map.CallOnAllNPCs((NPC npc) =>
                    {
                        npc.Kill();
                    });
                    Guild.WaveLimit = (Guild.WaveLimit + 1);
                    _map.CallDelayed(60 * 1000, () =>
                    {
                        lock(_playersRegisteredLock)
                        {
                            while(playersRegistered.Count > 0)
                            {
                                RemoveEnteredPlayer(playersRegistered[0]);
                            }
                            Asda2GuildWaveHandler.GuildWaveEnterStatusToGuild(Guild,GuildWaveEnterStatus.Ended);
                            _map.Stop();
                            _finished = true;
                        }
                    });


                    terminating = true;
					return;
                }
                else
                {
                    timeToSpawnNpcs -= dt;

                    if(timeToSpawnNpcs <= 0 && npcTemplate.Count > 0)
                    {
                        SpawnNpcsForCurrentWave(1);
                    }

                    int deadNpcsCount = 0;

                    foreach(NPC npc in currentWaveNpcs)
                    {
                        if(npc.Health <= 0)
                        {
                            deadNpcsCount++;
                        }
                    }

                    if(deadNpcsCount == (npcsCount1 + npcsCount2))
                    {
                        if (currentWaveNumber < 20)
                        {
                            timeToPrepare = defaultTimeToPrepare;
                        }
                        else
                        {
                            timeToPrepare = defaultTimeToPrepareAfter20;
                        }

                        preparation = true;

                        Asda2GuildWaveHandler.GuildWaveUpdate(this, GuildWaveUpdateStatus.Finish, currentWaveNumber + 1, timeToPrepare);

                        currentWaveNpcs.Clear();

                        if (currentWaveNumber == 29)
                        {
                            Guild.WaveLimit = (Guild.WaveLimit + 1);

                            //currentWaveNpcs.Clear();

                            ClearPlayersRegisteredList();

                            lock (_playersRegisteredLock)
                            {
                                foreach (Character chr in playersRegistered)
                                {
                                    GuildWaveResultAndItems(chr);
                                }
                            }

                            //Asda2GuildWaveHandler.GuildWaveEnterStatusToGuild(_guild, GuildWaveEnterStatus.Ended);

                            _map.CallDelayed(60 * 1000, () =>
                            {
                                lock (_playersRegisteredLock)
                                {
                                    while (playersRegistered.Count > 0)
                                    {
                                        RemoveEnteredPlayer(playersRegistered[0]);
                                    }

                                    _finished = true;
                                }

                                _map.CallDelayed(60 * 1000, () =>
                                {
                                    _map.Stop();
                                });
                            });
                            Asda2GuildWaveHandler.GuildWaveEnterStatusToGuild(_guild,GuildWaveEnterStatus.Ended);
                            terminating = true;

                            return;
                        }
                    }
                }
            }
            else
            {
                timeToPrepare -= dt;

                if(timeToPrepare <= 0)
                {
                    _map.ClearLootOnGuildWaveNow(GuildId);

                    ++currentWaveNumber;

                    timeToFight = waveTimers[currentWaveNumber];
                    timeToPrepare = 0;
                    preparation = false;

                    Asda2GuildWaveHandler.GuildWaveUpdate(this, GuildWaveUpdateStatus.Start, currentWaveNumber, timeToFight);

                    PrepareNpcEntriesList();
                    SpawnNpcsForCurrentWave(0);
                    timeToSpawnNpcs = defaultTimeToSpawnNpcs;
                }
            }
        }

        public int getTemplateIdFromIndex(int index, Asda2GuildWaveItemRecord record)
        {
            switch(index)
            {
                case 0 :
                    return record.Item1;
                case 1 :
                    return record.Item2;
                case 2:
                    return record.Item3;
                case 3:
                    return record.Item4;
                case 4:
                    return record.Item5;
                case 5:
                    return record.Item6;
                case 6:
                    return record.Item7;
                case 7:
                    return record.Item8;
            }

            return record.Item1;
        }

        public void GuildWaveResultAndItems(Character chr)
        {
            if (chr.IsDead)
                chr.Resurrect();

            Asda2TitleChecker.OnGuildWaveEnd(chr, LastWinnedWave);

            Asda2GuildWaveItemRecord waveItem = null;

            foreach(Asda2GuildWaveItemRecord record in Asda2ItemMgr.GuildWaveRewardRecords)
            {
                if(record.Wave == LastWinnedWave + 1 && record.Lvl == (int)Math.Ceiling(chr.Level / 10.0F) * 10 && record.Difficulty == _difficulty)
                {
                    waveItem = record;
                    break;
                }
            }

            if (waveItem != null)
            {
                List<KeyValuePair<int, int>> pairs = new List<KeyValuePair<int, int>>();
                pairs.Add(new KeyValuePair<int, int>(1, waveItem.Chance1));
                pairs.Add(new KeyValuePair<int, int>(2, waveItem.Chance2));
                pairs.Add(new KeyValuePair<int, int>(3, waveItem.Chance3));
                pairs.Add(new KeyValuePair<int, int>(4, waveItem.Chance4));
                pairs.Add(new KeyValuePair<int, int>(5, waveItem.Chance5));
                pairs.Add(new KeyValuePair<int, int>(6, waveItem.Chance6));
                pairs.Add(new KeyValuePair<int, int>(7, waveItem.Chance7));
                pairs.Add(new KeyValuePair<int, int>(8, waveItem.Chance8));
                pairs.Sort((a, b) => a.Value.CompareTo(b.Value));

                int templateId1 = getTemplateIdFromIndex(CharacterFormulas.GetWaveRewardItems(pairs), waveItem);
                int templateId2 = getTemplateIdFromIndex(CharacterFormulas.GetWaveRewardItems(pairs), waveItem);
                int templateId3 = getTemplateIdFromIndex(CharacterFormulas.GetWaveRewardItems(pairs), waveItem);

                Asda2Item item1 = Asda2Item.CreateItem(templateId1, chr, 1);

                Asda2Item wavecoin = null;

                int amount = getAverageLevel() / CharacterFormulas.WaveCoinsDivider;

                if(amount > 0)
                {
                    wavecoin = Asda2Item.CreateItem(33712, chr, amount);
                    chr.Asda2Inventory.TryAdd(33712, amount, true, ref wavecoin);
                }

                chr.Asda2Inventory.TryAdd(templateId1, 1, true, ref item1);
                
                if(_difficulty > 0)
                {
                    Asda2Item item2 = Asda2Item.CreateItem(templateId2, chr, 1);
                    chr.Asda2Inventory.TryAdd(templateId2, 1, true, ref item2);

                    if (_difficulty == 2)
                    {
                        Asda2Item item3 = Asda2Item.CreateItem(templateId3, chr, 1);
                        chr.Asda2Inventory.TryAdd(templateId3, 1, true, ref item3);
                    }
                }

                Asda2GuildWaveHandler.GuildWaveResult(this, chr, wavecoin == null ? 0 : wavecoin.Amount, templateId1, templateId2, templateId3);
            }
        }

        public void SendPacketToRegisteredPlayers(RealmPacketOut packet)
        {
            lock(_playersRegisteredLock)
            {
                foreach(Character chr in playersRegistered)
                {
                    if(!chr.IsAsda2GuildWave)
                    chr.Send(packet, addEnd: true);
                }
            }
        }

        public void SendPacketToRegisteredOnGuildWavePlayers(RealmPacketOut packet)
        {
            lock (_playersRegisteredLock)
            {
                foreach (Character chr in playersRegistered)
                {
                    if (chr.IsAsda2GuildWave)
                        chr.Send(packet, addEnd: true);
                }
            }
        }

        public int getAverageLevel()
        {
            int averageLevel = 0;

            lock (_playersRegisteredLock)
            {
                foreach (Character chr in playersRegistered)
                {
                    averageLevel += chr.Level;
                }

                averageLevel /= playersRegistered.Count;
            }

            return averageLevel;
        }

        public void StopMap()
        {
            if (_map.IsRunning)
            {
                _map.Stop();
            }
        }
    }
}