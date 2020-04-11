using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.RealmServer.Content;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Asda2GuildWave
{
    class Asda2GuildWaveMgr : IUpdatable
    {
        private static bool isUpdated = false;

        public static int NeedPlayerToStartGW = 5; // Default 5

        public const int defaultRegistrationPreparationTime = 5 * 60 * 1000;

        private static ConcurrentDictionary<int, Asda2GuildWave> guildWaves = new ConcurrentDictionary<int, Asda2GuildWave>();

        [Initialization(InitializationPass.Tenth, "Asda2 Guild Wave System")]
        public static void InitGuildWaves()
        {
            RestoreGuildWaveCounter();
            World.TaskQueue.RegisterUpdatableLater(new Asda2GuildWaveMgr());
        }

        public Asda2GuildWaveMgr()
        {
            Asda2GuildWave.InitGuildWaveNpcList();
        }

        public static Asda2GuildWave GetGuildWaveForId(int guildId)
        {
            Asda2GuildWave guildWave = null;

            guildWaves.TryGetValue(guildId, out guildWave);

            return guildWave;
        }

        public static void DiconnectGuildWave(Character chr)
        {
            if (chr.IsAsda2GuildWave)
            {
                Asda2GuildWave guildWave = Asda2GuildWaveMgr.GetGuildWaveForId((int)chr.GuildId);

                if (guildWave != null)
                {
                    guildWave.RemoveRegisteringPlayer(chr);
                }

                chr.IsAsda2GuildWave = false;
            }
        }

        public static Asda2GuildWave CreateGuildWave(int guildId, int difficulty, Character chr)
        {
            Asda2GuildWave guildWave = new Asda2GuildWave(guildId, difficulty, chr.Guild);
            guildWave.AddRegisteringPlayer(chr);
            guildWaves.TryAdd(guildId, guildWave);
            Asda2GuildWaveHandler.GuildWaveEnterStatusToGuild(chr.Guild, GuildWaveEnterStatus.Register);
            return guildWave;
        }

        public static void RestoreGuildWaveCounter()
        {
            //List<Guild> guilds = new List<Guild>(Guild.FindAll());
            foreach (var guild in GuildMgr.GuildsById.Values)
            {
                guild.WaveLimit = 0;
                Asda2GuildHandler.SendUpdateGuildInfoResponse(guild);
                guild.Update();
            }

            //for (int i = 0; i < guilds.Count; ++i)
            //{
            //    guilds[i].WaveLimit = 0;
            //    guilds[i].SaveAndFlush();
            //}
        }

        public void Update(int dt)
        {
            if (DateTime.Now.Hour == 6)
            {
                if (!isUpdated)
                {
                    RestoreGuildWaveCounter();
                    isUpdated = true;
                }
            }
            else
            {
                if (isUpdated)
                {
                    isUpdated = false;
                }
            }

            foreach(int guildId in guildWaves.Keys)
            {
                Asda2GuildWave guildWave = null;

                guildWaves.TryGetValue(guildId, out guildWave);

                if(guildWave != null)
                {
                    if (guildWave.IsFailed || guildWave.PlayersRegisteredCount == 0 || guildWave.IsFinished)
                    {
                        guildWave.StopMap();
                        guildWaves.TryRemove(guildId, out guildWave);

                        if(guildWave.PlayersRegisteredCount == 0 && !guildWave.IsFinished)
                        {
                            //Asda2GuildWaveHandler.GuildWaveEnterStatusToGuild(guildWave.Guild, GuildWaveEnterStatus.NotPlayers);
                        }
                    }
                    else
                    {
                        guildWave.UpdateGuildWave(dt);
                    }
                }
            }
        }
    }
}
