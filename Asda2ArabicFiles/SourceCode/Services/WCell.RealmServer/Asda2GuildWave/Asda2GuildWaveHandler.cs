using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Asda2GuildWave
{
    class Asda2GuildWaveHandler
    {
        [PacketHandler(RealmServerOpCode.GuildWaveOnRequest)] //4400
        public static void GuildWaveOnRequest(IRealmClient client, RealmPacketIn packet)
        {
            var guildid = packet.ReadInt16();

            GuildWaveoOnResponse(client,guildid);
        }

        public static void GuildWaveoOnResponse(IRealmClient client, int guildid)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildWaveoOnResponse)) //4401
            {
                Asda2GuildWave guildWave = Asda2GuildWaveMgr.GetGuildWaveForId(guildid);

                packet.WriteInt16(guildid); //{GuildId}default value : 3 Len : 2

                if (guildWave != null)
                {
                    packet.WriteByte((byte)GuildWaveOnSRS.Show); //{GuildWaveOnSRS} 
                    packet.WriteByte(guildWave.Difficulty); //{GuildWaveOnDifficulty}
                    packet.WriteInt16(guildWave.PlayersRegisteredCount); //{Count Player}
                }
                else
                {
                    packet.WriteByte((byte)GuildWaveOnSRS.Select); //{GuildWaveOnSRS} 
                    packet.WriteByte(0); //{GuildWaveOnDifficulty}
                    packet.WriteInt16(0); //{Count Player}
                }
                //Asda2GuildWave.GetDiffeclity(guildWave.Difficulty);

                client.ActiveCharacter.Send(packet, addEnd: true);
            }
        }

        public static void GuildWaveoOnResponseToRegisteredPlayers(int guildid)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildWaveoOnResponse)) //4401
            {
                Asda2GuildWave guildWave = Asda2GuildWaveMgr.GetGuildWaveForId(guildid);

                if (guildWave != null)
                {
                    packet.WriteInt16(guildid); //{GuildId}default value : 3 Len : 2
                    packet.WriteByte((byte)GuildWaveOnSRS.Show); //{GuildWaveOnSRS} 
                    packet.WriteByte(guildWave.Difficulty); //{GuildWaveOnDifficulty}
                    packet.WriteInt16(guildWave.PlayersRegisteredCount); //{Count Player}
                    guildWave.SendPacketToRegisteredPlayers(packet);
                }
            }
        }

        [PacketHandler(RealmServerOpCode.GuildWaveRegisterRequest)] //4402
        public static void GuildWaveRegisterRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            packet.ReadByte(); //{Type act}
            var difficulty = packet.ReadByte(); //{Difficulty}
            
            Asda2GuildWave guildWave = Asda2GuildWaveMgr.GetGuildWaveForId((int)client.ActiveCharacter.GuildId);
            
           /* if (guildWave.IsInProgress)
            {
                GuildWaveRegisterResponse(client, GuildWaveRegisterStatus.Fail);
                return;
            }*/
            if(guildWave == null)
            {
                if (client.ActiveCharacter.Guild.WaveLimit >= CharacterFormulas.GetWaveLimit(client.ActiveCharacter.Guild.Level))
                {
                    if (CharacterFormulas.GetWaveLimit(client.ActiveCharacter.Guild.Level) > 1)
                    GuildWaveRegisterResponse(client, GuildWaveRegisterStatus.LimitDay);
                    else
                    GuildWaveRegisterResponse(client, GuildWaveRegisterStatus.LimitOneDay);
                    return;
                }
                if (client.ActiveCharacter.Guild.Level < 2)
                {
                    GuildWaveRegisterResponse(client, GuildWaveRegisterStatus.GuildLevel);
                }
                else
                {
                    guildWave = Asda2GuildWaveMgr.CreateGuildWave((int)client.ActiveCharacter.GuildId, difficulty, client.ActiveCharacter);
                    GuildWaveRegisterResponse(client, GuildWaveRegisterStatus.Ok);
                    GuildWaveoOnResponseToRegisteredPlayers(guildWave.GuildId);
                }
            }
            else
            {
                if(guildWave.PlayersRegisteredCount < 20)
                {
                    if (guildWave.isPlayerRegistered(client.ActiveCharacter))
                    {
                        GuildWaveRegisterResponse(client, GuildWaveRegisterStatus.AlreadyRegistered);
                    }
                    else
                    {
                        if (guildWave.IsInProgress)
                        {
                            GuildWaveRegisterResponse(client, GuildWaveRegisterStatus.WaveInProgress);
                        }
                        else
                        {
                            guildWave.AddRegisteringPlayer(client.ActiveCharacter);
                            GuildWaveRegisterResponse(client, GuildWaveRegisterStatus.Ok);
                            GuildWaveoOnResponseToRegisteredPlayers((int)client.ActiveCharacter.GuildId);
                        }
                    }
                }
                else
                {
                    GuildWaveRegisterResponse(client, GuildWaveRegisterStatus.Full);
                }
            }
        }

        public static void GuildWaveRegisterResponse(IRealmClient client, GuildWaveRegisterStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildWaveRegisterResponse)) //4403
            {
                packet.WriteByte((byte)status); //{GuildWaveRegisterStatus}
                client.ActiveCharacter.Send(packet, addEnd : true);
            }
        }

        [PacketHandler(RealmServerOpCode.GuildWaveUnregisterRequest)] //4404
        public static void GuildWaveUnregisterRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            packet.ReadByte(); //{Type act}

            Asda2GuildWave guildWave = Asda2GuildWaveMgr.GetGuildWaveForId((int)client.ActiveCharacter.GuildId);

            if(guildWave != null)
            {
                guildWave.RemoveRegisteringPlayer(client.ActiveCharacter);
                GuildWaveUnregisterResponse(client, GuildWaveUnregisterStatus.Ok);
                GuildWaveoOnResponseToRegisteredPlayers((int)client.ActiveCharacter.GuildId);
            }
            else
            {
                GuildWaveUnregisterResponse(client, GuildWaveUnregisterStatus.Fail);
            }
        }

        public static void GuildWaveUnregisterResponse(IRealmClient client, GuildWaveUnregisterStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildWaveUnregisterResponse)) //4405
            {
                packet.WriteByte((byte)status); //{GuildWaveUnregisterStatus}

                client.ActiveCharacter.Send(packet, addEnd: true);
            }
        }

        [PacketHandler(RealmServerOpCode.GuildWaveEnterRequest)] //4406
        public static void GuildWaveEnterRequest(IRealmClient client, RealmPacketIn packet)
        {
            Asda2GuildWave guildWave = Asda2GuildWaveMgr.GetGuildWaveForId((int)client.ActiveCharacter.GuildId);

            if(guildWave != null)
            {
                if(guildWave.RegistrationFinished)
                {
                    if (!guildWave.IsInProgress)
                    {
                        if (guildWave.isPlayerRegistered(client.ActiveCharacter))
                        {
                            client.ActiveCharacter.IsAsda2GuildWave = true;
                            guildWave.AddEnteredPlayer(client.ActiveCharacter);
                        }
                        else
                        {
                            GlobalHandler.SendTeleportedByCristalResponse(client, MapId.Alpia, 0, 0, TeleportByCristalStaus.NotRegisterWave);
                        }
                    }
                    else
                    {
                        GlobalHandler.SendTeleportedByCristalResponse(client, MapId.Alpia, 0, 0, TeleportByCristalStaus.RejoinNot);
                    }
                }
            }
            else
            {
                GlobalHandler.SendTeleportedByCristalResponse(client, MapId.Alpia, 0, 0, TeleportByCristalStaus.NoWaveInfo);
            }
        }

        [PacketHandler(RealmServerOpCode.GuildWaveExit)] //4407
        public static void GuildWaveExit(IRealmClient client, RealmPacketIn packet)
        {
            Asda2GuildWave guildWave = Asda2GuildWaveMgr.GetGuildWaveForId((int)client.ActiveCharacter.GuildId);

            if(guildWave != null)
            {
                client.ActiveCharacter.IsAsda2GuildWave = false;
                guildWave.ExitGuildWave(client.ActiveCharacter);
            }
        }

        public static void GuildWaveEnterStatus(IRealmClient client, GuildWaveEnterStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildWaveEnterStatus)) //4408
            {
                packet.WriteByte((byte)status); //{GuildWaveEnterStatus}

                client.ActiveCharacter.Send(packet, addEnd: true);
            }
        }

        public static void GuildWaveEnterStatusToGuild(Guild guild, GuildWaveEnterStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildWaveEnterStatus)) //4408
            {
                packet.WriteByte((byte)status); //{GuildWaveEnterStatus}

                guild.Send(packet, addEnd: true);
            }
        }
        public static void GuildWaveUpdateEnter(IRealmClient client, int remainingTime)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildWaveUpdate)) //4409
            {
                packet.WriteInt16(client.ActiveCharacter.GuildId); //{GuildId}
                packet.WriteByte(0); // {type} 0 - подготовка(время - подготовка к 1 волне). 1 старт волны (время волны) .2 конец волны (время - подготовка к след волне)
                packet.WriteInt16(0); // {wave}
                packet.WriteInt32(remainingTime);//{time} 5мин = 300000, 300*1000
                client.ActiveCharacter.Send(packet, addEnd: true);
            }
        }

        public static void GuildWaveUpdate(Asda2GuildWave guildWave, GuildWaveUpdateStatus status, int wave, int time)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildWaveUpdate)) //4409
            {
                packet.WriteInt16(guildWave.GuildId); //{GuildId}
                packet.WriteByte((byte)status); // {type} 0 - подготовка(время - подготовка к 1 волне). 1 старт волны (время волны) .2 конец волны (время - подготовка к след волне)
                packet.WriteInt16(wave); // {wave}
                packet.WriteInt32(time);//{time} 5мин = 300000, 300*1000
                guildWave.SendPacketToRegisteredOnGuildWavePlayers(packet);
            }
        }

        public static void GuildWaveResult(Asda2GuildWave guildWave, Character chr, int waveCoin, int itemId1, int itemId2, int itemId3)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildWaveResult)) //4411
            {
                packet.WriteInt16(guildWave.GuildId); // {guildId}
                packet.WriteByte(guildWave.Difficulty); // {Difficulty} 
                packet.WriteInt16(guildWave.PlayersRegisteredCount); // {Count players} 
                packet.WriteByte(guildWave.getAverageLevel()); // {Average Lvl} 
                packet.WriteInt16(guildWave.LastWinnedWave + 1); // {Wave} 
                packet.WriteInt32(0);
                packet.WriteInt16(waveCoin); // wave coin
                packet.WriteInt32(itemId1); // prize 1
                packet.WriteInt32(itemId2); // prize 1 hard
                packet.WriteInt32(itemId3); // prize 1 hell
                chr.Send(packet);
            }
        }

        public static void GuildWaveEnd(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildWaveEnd)) //4413
            {
                client.ActiveCharacter.Send(packet, addEnd: true);
            }
        }
    }

    public enum GuildWaveOnSRS
    {
        Select = 0,
        Show = 1,
    }

    public enum GuildWaveOnDifficulty
    {
        Normal = 0,
        Hard = 1,
        Hell = 2,
    }

    public enum GuildWaveRegisterStatus
    {
        Fail = 0, // You've failed to register for the Guild Wave
        Ok = 1,
        ProblemGuildInformation = 2, // There is a problem with the guild information
        LimitOneDay = 3, // You can only participate in the Guild Wave once per day. (Reset occurs at 6AM daily)
        LimitDay = 4, // You've already reached your daily Guild Wave Limit. (Reset occurs at 6AM daily)
        AlreadyRegistered = 5, // Already Registered
        Full = 6, // Guild Wave is currently in full
        WaveInProgress = 7, // Wave In Progress
        Ending = 8, // Wave is currently ending. Please try again after is has completed.
        GuildLevel = 9,  // The level of you guild must be 2 or more to register.
        Error10 = 10, // 10
    }

    public enum GuildWaveUnregisterStatus
    {
        Fail = 0, // Cancellation of Guild Wave registration has failed.
        Ok = 1, // Cancellation of Guild Wave registration has succeeded
        Ok2 = 2, // Cancellation of Guild Wave registration has succeeded
        ProblemGuildInformation = 3, // There is a problem with the guild information
    } 

    public enum GuildWaveEnterStatus
    {
        Fail = 0, // Due to an insufficient amount of players, Guild Wave has been cancelled.
        Ok = 1, // You may enter the Guild Wave.
        AlreadyStarted = 2, // The Guild Wave has already started. You cannot enter.
        NotPlayers = 3, // The Guild Wave has been cancelled due to no participants.
        Ended = 4, // Guild Wave has ended.
        Register = 5, // Wave registration has opened. The registration process will be closed after 5 minutes.

    }

    public enum GuildWaveUpdateStatus
    {
        Prepare = 0, // подготовка(время - подготовка к 1 волне)
        Start = 1, // старт волны (время волны)
        Finish = 2 // конец волны (время - подготовка к след волне)
    }
} 