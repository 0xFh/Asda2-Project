using NLog;
using System;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Auctioneer;

namespace WCell.RealmServer.Handlers
{
    public static class AuctionHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static void HandleAuctionHello(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            NPC auctioneer = activeCharacter.Map.GetObject(id) as NPC;
            Singleton<AuctionMgr>.Instance.AuctionHello(activeCharacter, auctioneer);
        }

        public static void HandleAuctionSellItem(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            int num = (int) packet.ReadUInt32();
            EntityId itemId = packet.ReadEntityId();
            uint stackSize = packet.ReadUInt32();
            uint bid = packet.ReadUInt32();
            uint buyout = packet.ReadUInt32();
            uint time = packet.ReadUInt32();
            NPC auctioneer = activeCharacter.Map.GetObject(id) as NPC;
            Singleton<AuctionMgr>.Instance.AuctionSellItem(activeCharacter, auctioneer, itemId, bid, buyout, time,
                stackSize);
        }

        public static void HandleAuctionListPendingSales(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.Map.GetObject(packet.ReadEntityId());
            uint num = 1;
            using (RealmPacketOut packet1 =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_AUCTION_LIST_PENDING_SALES, 14 * (int) num))
            {
                packet1.Write(num);
                for (int index = 0; (long) index < (long) num; ++index)
                {
                    packet1.Write("");
                    packet1.Write("");
                    packet1.WriteUInt(0);
                    packet1.WriteUInt(0);
                    packet1.WriteFloat(0.0f);
                    client.Send(packet1, false);
                }
            }
        }

        public static void HandleAuctionPlaceBid(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            uint auctionId = packet.ReadUInt32();
            uint bid = packet.ReadUInt32();
            NPC auctioneer = activeCharacter.Map.GetObject(id) as NPC;
            Singleton<AuctionMgr>.Instance.AuctionPlaceBid(activeCharacter, auctioneer, auctionId, bid);
        }

        public static void HandleAuctionRemoveItem(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            uint auctionId = packet.ReadUInt32();
            NPC auctioneer = activeCharacter.Map.GetObject(id) as NPC;
            Singleton<AuctionMgr>.Instance.CancelAuction(activeCharacter, auctioneer, auctionId);
        }

        public static void HandleAuctionListOwnerItems(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            NPC auctioneer = activeCharacter.Map.GetObject(id) as NPC;
            Singleton<AuctionMgr>.Instance.AuctionListOwnerItems(activeCharacter, auctioneer);
        }

        public static void HandleAuctionListBidderItems(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            uint num1 = packet.ReadUInt32();
            int num2 = 0;
            while ((long) num2 < (long) num1)
                ++num2;
            NPC auctioneer = activeCharacter.Map.GetObject(id) as NPC;
            Singleton<AuctionMgr>.Instance.AuctionListBidderItems(activeCharacter, auctioneer);
        }

        public static void HandleAuctionListItems(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            NPC auctioneer = activeCharacter.Map.GetObject(id) as NPC;
            AuctionSearch searcher = new AuctionSearch()
            {
                StartIndex = packet.ReadUInt32(),
                Name = packet.ReadCString(),
                LevelRange1 = (uint) packet.ReadByte(),
                LevelRange2 = (uint) packet.ReadByte(),
                InventoryType = (InventorySlotType) packet.ReadUInt32(),
                ItemClass = (ItemClass) packet.ReadUInt32(),
                ItemSubClass = (ItemSubClass) packet.ReadUInt32(),
                Quality = packet.ReadInt32(),
                IsUsable = packet.ReadBoolean()
            };
            Singleton<AuctionMgr>.Instance.AuctionListItems(activeCharacter, auctioneer, searcher);
        }

        public static void SendAuctionHello(IPacketReceiver client, NPC auctioneer)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.MSG_AUCTION_HELLO, 12))
            {
                packet.Write((ulong) auctioneer.EntityId);
                packet.Write((uint) auctioneer.AuctioneerEntry.LinkedHouseFaction);
                packet.Write(true);
                client.Send(packet, false);
            }
        }

        public static void SendAuctionCommandResult(IPacketReceiver client, Auction auction, AuctionAction action,
            AuctionError error)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_AUCTION_COMMAND_RESULT, 12))
            {
                if (auction != null)
                    packet.Write(auction.ItemLowId);
                else
                    packet.Write(0U);
                packet.Write((uint) action);
                packet.Write((uint) error);
                client.Send(packet, false);
            }
        }

        public static void SendAuctionOutbidNotification(IPacketReceiver client, Auction auction, uint newBid,
            uint minBidInc)
        {
            if (auction == null)
                return;
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_AUCTION_BIDDER_NOTIFICATION, 32))
            {
                packet.Write((uint) auction.HouseFaction);
                packet.Write(auction.ItemLowId);
                packet.Write(auction.BidderLowId);
                packet.Write(newBid);
                packet.Write(minBidInc);
                packet.Write(auction.ItemTemplateId);
                packet.Write(0U);
                client.Send(packet, false);
            }
        }

        public static void SendAuctionListOwnerItems(IPacketReceiver client, Auction[] auctions)
        {
            if (auctions == null || auctions.Length < 1)
                return;
            RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_AUCTION_OWNER_LIST_RESULT, 1024);
            packet.Write(auctions.Length);
            foreach (Auction auction in auctions)
                AuctionHandler.BuildAuctionPacket(auction, packet);
            packet.Write(auctions.Length);
            packet.Write(0);
            client.Send(packet, false);
            packet.Close();
        }

        public static void SendAuctionListBidderItems(IPacketReceiver client, Auction[] auctions)
        {
            if (auctions == null || auctions.Length < 1)
                return;
            RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_AUCTION_BIDDER_LIST_RESULT, 1024);
            packet.Write(auctions.Length);
            foreach (Auction auction in auctions)
                AuctionHandler.BuildAuctionPacket(auction, packet);
            packet.Write(auctions.Length);
            packet.Write(0);
            client.Send(packet, false);
            packet.Close();
        }

        public static void SendAuctionListItems(IPacketReceiver client, Auction[] auctions)
        {
            if (auctions == null || auctions.Length < 1)
                return;
            RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_AUCTION_LIST_RESULT, 7000);
            int num = 0;
            packet.Write(auctions.Length);
            foreach (Auction auction in auctions)
            {
                if (AuctionHandler.BuildAuctionPacket(auction, packet))
                    ++num;
            }

            packet.Write(num);
            packet.Write(300);
            client.Send(packet, false);
            packet.Close();
        }

        public static bool BuildAuctionPacket(Auction auction, RealmPacketOut packet)
        {
            ItemRecord auctionItem = Singleton<AuctionMgr>.Instance.AuctionItems[auction.ItemLowId];
            if (auctionItem == null)
                return false;
            TimeSpan timeSpan = auction.TimeEnds - DateTime.Now;
            if (timeSpan.TotalMilliseconds < 0.0)
                return false;
            packet.Write(auction.ItemLowId);
            packet.Write(auctionItem.Template.Id);
            for (int index = 0; index < 7; ++index)
            {
                if (auctionItem.EnchantIds != null)
                {
                    packet.Write(auctionItem.EnchantIds[index]);
                    packet.Write(index);
                    packet.Write(auctionItem.GetEnchant((EnchantSlot) index).Charges);
                }
                else
                {
                    packet.Write(0);
                    packet.Write(0);
                    packet.Write(0);
                }
            }

            packet.Write(auctionItem.RandomProperty);
            packet.Write(auctionItem.RandomSuffix);
            packet.Write(auctionItem.Amount);
            packet.Write((uint) auctionItem.Charges);
            packet.WriteUInt(0);
            packet.WriteULong(auction.OwnerLowId);
            packet.Write(auction.CurrentBid);
            packet.WriteUInt(AuctionMgr.GetMinimumNewBidIncrement(auction));
            packet.Write(auction.BuyoutPrice);
            packet.Write((int) timeSpan.TotalMilliseconds);
            packet.WriteULong(auction.BidderLowId);
            packet.Write(auction.CurrentBid);
            return true;
        }
    }
}