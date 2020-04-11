using NLog;
using System;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Taxi;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
    public static class TaxiHandler
    {
        private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>Talk to a Taxi Vendor</summary>
        public static void HandleEnable(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            NPC taxiVendor = client.ActiveCharacter.Map.GetObject(id) as NPC;
            if (taxiVendor == null)
                return;
            taxiVendor.TalkToFM(client.ActiveCharacter);
        }

        /// <summary>Talk to a Taxi Vendor</summary>
        public static void HandleAvailableNodesQuery(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            NPC taxiVendor = client.ActiveCharacter.Map.GetObject(id) as NPC;
            if (taxiVendor == null)
                return;
            taxiVendor.TalkToFM(client.ActiveCharacter);
        }

        /// <summary>Fly away</summary>
        public static void HandleTaxiActivate(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            uint index1 = packet.ReadUInt32();
            uint index2 = packet.ReadUInt32();
            NPC vendor = client.ActiveCharacter.Map.GetObject(id) as NPC;
            PathNode[] destinations = new PathNode[2]
            {
                TaxiMgr.PathNodesById.Get<PathNode>(index1),
                TaxiMgr.PathNodesById.Get<PathNode>(index2)
            };
            TaxiMgr.TryFly(client.ActiveCharacter, vendor, destinations);
        }

        /// <summary>
        /// Fly far away. (For taxi paths that include more than one stop.)
        /// </summary>
        public static void HandleTaxiActivateFar(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            uint num = packet.ReadUInt32();
            PathNode[] destinations = new PathNode[num];
            for (int index = 0; (long) index < (long) num; ++index)
                destinations[index] = TaxiMgr.PathNodesById.Get<PathNode>(packet.ReadUInt32());
            NPC vendor = client.ActiveCharacter.Map.GetObject(id) as NPC;
            TaxiMgr.TryFly(client.ActiveCharacter, vendor, destinations);
        }

        /// <summary>Client asks "Is this TaxiNode activated yet?"</summary>
        public static void HandleTaxiStatusQuery(IRealmClient client, RealmPacketIn packet)
        {
            EntityId entityId = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(entityId) as NPC;
            if (npc == null)
                return;
            bool isActiveNode = activeCharacter.GodMode || activeCharacter.TaxiNodes.IsActive(npc.VendorTaxiNode);
            TaxiHandler.SendTaxiNodeStatus((IPacketReceiver) client, npc.VendorTaxiNode, entityId, isActiveNode);
        }

        /// <summary>
        /// Asked by the client upon landing at each stop in a multinode trip
        /// </summary>
        public static void HandleNextTaxiDestination(IRealmClient client, RealmPacketIn packet)
        {
            TaxiMgr.ContinueFlight((Unit) client.ActiveCharacter);
        }

        public static void SendTaxiPathActivated(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_NEW_TAXI_PATH, 0))
                client.Send(packet, false);
        }

        public static void SendTaxiPathUpdate(IPacketReceiver client, EntityId vendorId, bool activated)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_TAXINODE_STATUS, 9))
            {
                packet.Write(vendorId.Full);
                packet.Write(activated);
                client.Send(packet, false);
            }
        }

        public static void ShowTaxiList(Character chr, PathNode node)
        {
            if (node == null)
                return;
            TaxiHandler.ShowTaxiList(chr, (IEntity) chr, node);
        }

        public static void ShowTaxiList(Character chr, IEntity vendor, PathNode curNode)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SHOWTAXINODES, 48))
            {
                packet.Write(1);
                if (vendor != null)
                    packet.Write(vendor.EntityId.Full);
                else
                    packet.Write((ulong) EntityId.Zero);
                packet.Write(curNode.Id);
                for (int index = 0; index < chr.TaxiNodes.Mask.Length; ++index)
                    packet.Write(chr.TaxiNodes.Mask[index]);
                chr.Send(packet, false);
            }
        }

        public static void SendTaxiNodeStatus(IPacketReceiver client, PathNode curNode, EntityId vendorId,
            bool isActiveNode)
        {
            if (curNode != null)
                TaxiHandler.SendTaxiPathUpdate(client, vendorId, isActiveNode);
            else
                TaxiHandler.sLog.Warn("Vendor: {0} not associated with a TaxiNode", vendorId.Full);
        }

        public static void SendActivateTaxiReply(IPacketReceiver client, TaxiActivateResponse response)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ACTIVATETAXIREPLY, 4))
            {
                packet.Write((uint) response);
                client.Send(packet, false);
            }
        }
    }
}