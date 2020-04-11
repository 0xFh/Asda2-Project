using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class FactionHandler
    {
        /// <summary>User starts/ends war with a faction</summary>
        public static void HandleStartWar(IRealmClient client, RealmPacketIn packet)
        {
            FactionReputationIndex reputationIndex = (FactionReputationIndex) packet.ReadUInt32();
            bool hostile = packet.ReadBoolean();
            client.ActiveCharacter.Reputations.DeclareWar(reputationIndex, hostile, true);
        }

        /// <summary>User watches Faction-status</summary>
        public static void HandleSetWatchedFaction(IRealmClient client, RealmPacketIn packet)
        {
            int num = packet.ReadInt32();
            client.ActiveCharacter.WatchedFaction = num;
        }

        /// <summary>Sets the specified faction to the inactive state</summary>
        public static void HandleStopWatchingFaction(IRealmClient client, RealmPacketIn packet)
        {
            FactionReputationIndex reputationIndex = (FactionReputationIndex) packet.ReadInt32();
            bool inactive = packet.ReadBoolean();
            client.ActiveCharacter.Reputations.SetInactive(reputationIndex, inactive);
        }

        /// <summary>Makes the given faction visible to the client.</summary>
        public static void SendVisible(IPacketReceiver client, FactionReputationIndex reputationIndex)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SET_FACTION_VISIBLE, 4))
            {
                packet.Write((int) reputationIndex);
                client.Send(packet, false);
            }
        }

        /// <summary>
        /// Lets player know they are at war with a certain faction.
        /// </summary>
        public static void SendSetAtWar(IPacketReceiver client, Reputation rep)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SET_FACTION_ATWAR, 5))
            {
                packet.Write((int) rep.Faction.ReputationIndex);
                packet.Write((byte) rep.Flags);
                client.Send(packet, false);
            }
        }

        /// <summary>Sends a reputation update.</summary>
        public static void SendReputationStandingUpdate(IPacketReceiver client, Reputation rep)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SET_FACTION_STANDING, 16))
            {
                packet.Write(0.0f);
                packet.Write((byte) 0);
                packet.Write(1);
                packet.Write((uint) rep.Faction.ReputationIndex);
                packet.Write(rep.Value);
                client.Send(packet, false);
            }
        }

        /// <summary>
        /// Sends all known factions to the client (only used right after connecting).
        /// </summary>
        public static void SendFactionList(Character chr)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_INITIALIZE_FACTIONS, 644))
            {
                packet.Write(128);
                for (int index = 0; index < 128; ++index)
                {
                    Reputation reputation = chr.Reputations[(FactionReputationIndex) index];
                    if (reputation != null)
                    {
                        packet.Write((byte) reputation.Flags);
                        packet.Write(reputation.Value);
                    }
                    else
                    {
                        packet.Write((byte) 0);
                        packet.Write(0);
                    }
                }

                chr.Client.Send(packet, false);
            }
        }
    }
}