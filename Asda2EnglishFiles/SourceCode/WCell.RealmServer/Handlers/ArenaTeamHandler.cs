using NLog;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.ArenaTeams;
using WCell.Core.Network;
using WCell.RealmServer.Battlegrounds.Arenas;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class ArenaTeamHandler
    {
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

        public static void HandleArenaTeamQuery(IRealmClient client, RealmPacketIn packet)
        {
            ArenaTeam arenaTeam = ArenaMgr.GetArenaTeam(packet.ReadUInt32());
            if (arenaTeam == null)
                return;
            ArenaTeamHandler.SendArenaTeamQueryResponse((IPacketReceiver) client, arenaTeam);
        }

        public static void HandleArenaTeamRoster(IRealmClient client, RealmPacketIn packet)
        {
            ArenaTeam arenaTeam = ArenaMgr.GetArenaTeam(packet.ReadUInt32());
            if (arenaTeam == null)
                return;
            ArenaTeamHandler.SendArenaTeamRosterResponse((IPacketReceiver) client, arenaTeam);
        }

        /// <summary>Sends an arena team query response to the client.</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="team">arena team to be sent</param>
        public static void SendArenaTeamQueryResponse(IPacketReceiver client, ArenaTeam team)
        {
            using (RealmPacketOut queryResponsePacket = ArenaTeamHandler.CreateArenaTeamQueryResponsePacket(team))
                client.Send(queryResponsePacket, false);
            using (RealmPacketOut statsResponsePacket = ArenaTeamHandler.CreateArenaTeamStatsResponsePacket(team))
                client.Send(statsResponsePacket, false);
        }

        public static void SendArenaTeamRosterResponse(IPacketReceiver client, ArenaTeam team)
        {
            using (RealmPacketOut rosterResponsePacket = ArenaTeamHandler.CreateArenaTeamRosterResponsePacket(team))
                client.Send(rosterResponsePacket, false);
        }

        private static RealmPacketOut CreateArenaTeamQueryResponsePacket(ArenaTeam team)
        {
            RealmPacketOut realmPacketOut =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ARENA_TEAM_QUERY_RESPONSE,
                    28 + team.Name.Length + 1);
            realmPacketOut.WriteUInt((byte) team.Id);
            realmPacketOut.WriteCString(team.Name);
            realmPacketOut.WriteUInt(team.Type);
            return realmPacketOut;
        }

        private static RealmPacketOut CreateArenaTeamStatsResponsePacket(ArenaTeam team)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ARENA_TEAM_STATS, 28);
            realmPacketOut.WriteUInt((byte) team.Id);
            realmPacketOut.WriteUInt(team.Stats.rating);
            realmPacketOut.WriteUInt(team.Stats.gamesWeek);
            realmPacketOut.WriteUInt(team.Stats.winsWeek);
            realmPacketOut.WriteUInt(team.Stats.gamesSeason);
            realmPacketOut.WriteUInt(team.Stats.winsSeason);
            realmPacketOut.WriteUInt(team.Stats.rank);
            return realmPacketOut;
        }

        private static RealmPacketOut CreateArenaTeamRosterResponsePacket(ArenaTeam team)
        {
            RealmPacketOut realmPacketOut =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ARENA_TEAM_ROSTER, 100);
            realmPacketOut.WriteUInt(team.Id);
            realmPacketOut.WriteByte(0);
            realmPacketOut.WriteUInt(team.MemberCount);
            realmPacketOut.WriteUInt(team.Type);
            foreach (ArenaTeamMember arenaTeamMember in (IEnumerable<ArenaTeamMember>) team.Members.Values)
            {
                realmPacketOut.WriteULong(arenaTeamMember.Character.EntityId.Full);
                Character character = World.GetCharacter(arenaTeamMember.Character.EntityId.Low);
                realmPacketOut.WriteByte(character != null ? 1 : 0);
                realmPacketOut.WriteCString(arenaTeamMember.Character.Name);
                realmPacketOut.WriteByte(team.Leader == arenaTeamMember ? 0 : 1);
                realmPacketOut.WriteByte(character != null ? character.Level : 0);
                realmPacketOut.WriteUInt((uint) arenaTeamMember.Class);
                realmPacketOut.WriteUInt(arenaTeamMember.GamesWeek);
                realmPacketOut.WriteUInt(arenaTeamMember.WinsWeek);
                realmPacketOut.WriteUInt(arenaTeamMember.GamesSeason);
                realmPacketOut.WriteUInt(arenaTeamMember.WinsSeason);
                realmPacketOut.WriteUInt(arenaTeamMember.PersonalRating);
                realmPacketOut.WriteFloat(0.0f);
                realmPacketOut.WriteFloat(0.0f);
            }

            return realmPacketOut;
        }

        /// <summary>Sends result of actions connected with arenas</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="commandId">command executed</param>
        /// <param name="name">name of player event has happened to</param>
        /// <param name="resultCode">The <see cref="T:WCell.Constants.ArenaTeams.ArenaTeamResult" /> result code</param>
        public static void SendResult(IPacketReceiver client, ArenaTeamCommandId commandId, string team, string player,
            ArenaTeamResult resultCode)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ARENA_TEAM_COMMAND_RESULT))
            {
                packet.WriteUInt((uint) commandId);
                packet.WriteCString(team);
                packet.WriteCString(player);
                packet.WriteUInt((uint) resultCode);
                client.Send(packet, false);
            }
        }

        /// <summary>Sends result of actions connected with arenas</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="commandId">command executed</param>
        /// <param name="resultCode">The <see cref="T:WCell.Constants.ArenaTeams.ArenaTeamResult" /> result code</param>
        public static void SendResult(IPacketReceiver client, ArenaTeamCommandId commandId, ArenaTeamResult resultCode)
        {
            ArenaTeamHandler.SendResult(client, commandId, string.Empty, string.Empty, resultCode);
        }
    }
}