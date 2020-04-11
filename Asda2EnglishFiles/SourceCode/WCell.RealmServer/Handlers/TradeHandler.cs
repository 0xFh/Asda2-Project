using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Trade;

namespace WCell.RealmServer.Handlers
{
    public class TradeHandler
    {
        public static void HandleProposeTrade(IRealmClient client, RealmPacketIn packet)
        {
            Character character = World.GetCharacter(packet.ReadEntityId().Low);
            if (!TradeMgr.MayProposeTrade(client.ActiveCharacter, character))
                return;
            TradeMgr.Propose(client.ActiveCharacter, character);
        }

        public static void HandleBeginTrade(IRealmClient client, RealmPacketIn packet)
        {
            TradeWindow tradeWindow = client.ActiveCharacter.TradeWindow;
            if (tradeWindow == null)
                TradeHandler.SendTradeStatus((IPacketReceiver) client, TradeStatus.PlayerNotFound);
            else
                tradeWindow.AcceptTradeProposal();
        }

        public static void HandleBusyTrade(IRealmClient client, RealmPacketIn packet)
        {
            TradeWindow tradeWindow = client.ActiveCharacter.TradeWindow;
            if (tradeWindow == null)
                TradeHandler.SendTradeStatus((IPacketReceiver) client, TradeStatus.PlayerNotFound);
            else
                tradeWindow.StopTrade(TradeStatus.PlayerBusy, false);
        }

        public static void HandleIgnoreTrade(IRealmClient client, RealmPacketIn packet)
        {
            TradeWindow tradeWindow = client.ActiveCharacter.TradeWindow;
            if (tradeWindow == null)
                TradeHandler.SendTradeStatus((IPacketReceiver) client, TradeStatus.PlayerNotFound);
            else
                tradeWindow.StopTrade(TradeStatus.PlayerIgnored, false);
        }

        public static void HandleCancelTrade(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter == null || client.ActiveCharacter.TradeWindow == null)
                return;
            client.ActiveCharacter.TradeWindow.Cancel(TradeStatus.Cancelled);
        }

        public static void HandleAcceptTrade(IRealmClient client, RealmPacketIn packet)
        {
            TradeWindow tradeWindow = client.ActiveCharacter.TradeWindow;
            if (tradeWindow == null)
                return;
            tradeWindow.AcceptTrade(false);
        }

        public static void HandleUnacceptTrade(IRealmClient client, RealmPacketIn packet)
        {
            TradeWindow tradeWindow = client.ActiveCharacter.TradeWindow;
            if (tradeWindow == null)
                return;
            tradeWindow.UnacceptTrade(false);
        }

        public static void HandleSetTradeGold(IRealmClient client, RealmPacketIn packet)
        {
            TradeWindow tradeWindow = client.ActiveCharacter.TradeWindow;
            if (tradeWindow == null)
                return;
            uint money = packet.ReadUInt32();
            tradeWindow.SetMoney(money, false);
        }

        public static void HandleSetTradeItem(IRealmClient client, RealmPacketIn packet)
        {
            TradeWindow tradeWindow = client.ActiveCharacter.TradeWindow;
            if (tradeWindow == null)
                return;
            byte tradeSlot = packet.ReadByte();
            byte bag = packet.ReadByte();
            byte slot = packet.ReadByte();
            tradeWindow.SetTradeItem(tradeSlot, bag, slot, false);
        }

        public static void HandleClearTradeItem(IRealmClient client, RealmPacketIn packet)
        {
            TradeWindow tradeWindow = client.ActiveCharacter.TradeWindow;
            if (tradeWindow == null)
                return;
            byte tradeSlot = packet.ReadByte();
            tradeWindow.ClearTradeItem(tradeSlot, false);
        }

        public static void SendTradeProposal(IPacketReceiver client, Character initiater)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_TRADE_STATUS))
            {
                packet.WriteUInt(1U);
                packet.Write((ulong) initiater.EntityId);
                client.Send(packet, false);
            }
        }

        public static void SendTradeStatus(IPacketReceiver client, TradeStatus tradeStatus)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_TRADE_STATUS))
            {
                packet.WriteUInt((uint) tradeStatus);
                client.Send(packet, false);
            }
        }

        public static void SendTradeProposalAccepted(IPacketReceiver client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_TRADE_STATUS))
            {
                packet.WriteUInt(1U);
                packet.Write(0);
                client.Send(packet, false);
            }
        }

        /// <summary>
        /// Sends the new state of the trading window to other party
        /// </summary>
        /// <param name="otherParty">Whether this is sending the own info to the other party (or, if false, to oneself)</param>
        /// <param name="client">receiving party</param>
        /// <param name="money">new amount of money</param>
        /// <param name="items">new items</param>
        public static void SendTradeUpdate(IPacketReceiver client, bool otherParty, uint money, Item[] items)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_TRADE_STATUS_EXTENDED,
                30 + 72 * items.Length))
            {
                packet.Write(otherParty);
                packet.Write(0);
                packet.Write(items.Length);
                packet.Write(items.Length);
                packet.Write(money);
                packet.Write(0);
                for (int val = 0; val < items.Length; ++val)
                {
                    packet.WriteByte(val);
                    Item obj = items[val];
                    if (obj != null)
                    {
                        packet.Write(obj.EntryId);
                        packet.Write(obj.Template.DisplayId);
                        packet.Write(obj.Amount);
                        packet.Write(0);
                        packet.Write((ulong) obj.GiftCreator);
                        ItemEnchantment enchantment = obj.GetEnchantment(EnchantSlot.Permanent);
                        packet.Write(enchantment != null ? enchantment.Entry.Id : 0U);
                        packet.Zero(12);
                        packet.Write((ulong) obj.Creator);
                        packet.Write(obj.SpellCharges);
                        packet.Write(obj.Template.RandomSuffixFactor);
                        packet.Write(obj.RandomPropertiesId);
                        LockEntry lockEntry = obj.Lock;
                        packet.Write(lockEntry != null ? lockEntry.Id : 0U);
                        packet.Write(obj.MaxDurability);
                        packet.Write(obj.Durability);
                    }
                    else
                        packet.Zero(72);
                }

                client.Send(packet, false);
            }
        }
    }
}