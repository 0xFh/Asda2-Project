using System.Collections.Generic;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class GossipHandler
    {
        /// <summary>
        /// Handles gossip hello packet (client requests Gossip Menu)
        /// </summary>
        /// <param name="client">realm client</param>
        /// <param name="packet">packet incoming</param>
        public static void HandleGossipHello(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            Unit unit = activeCharacter.Map.GetObject(id) as Unit;
            if (unit == null)
                return;
            activeCharacter.GossipConversation = (GossipConversation) null;
            GossipMenu gossipMenu = unit.GossipMenu;
            if (gossipMenu == null || unit is NPC && !((NPC) unit).CheckVendorInteraction(activeCharacter) &&
                !activeCharacter.Role.IsStaff)
                return;
            activeCharacter.OnInteract((WorldObject) unit);
            activeCharacter.StartGossip(gossipMenu, (WorldObject) unit);
        }

        /// <summary>Handles option selecting in gossip menu</summary>
        /// <param name="client">realm client</param>
        /// <param name="packet">packet incoming</param>
        public static void HandleGossipSelectOption(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            int num = (int) packet.ReadUInt32();
            uint itemID = packet.ReadUInt32();
            string extra = string.Empty;
            if (packet.Position < packet.Length)
                extra = packet.ReadCString();
            Character activeCharacter = client.ActiveCharacter;
            WorldObject worldObject = activeCharacter.Map.GetObject(id);
            if (worldObject == null)
                return;
            GossipConversation gossipConversation = activeCharacter.GossipConversation;
            if (gossipConversation == null || gossipConversation.Speaker != worldObject)
                return;
            gossipConversation.HandleSelectedItem(itemID, extra);
        }

        /// <summary>Sends a page to the character</summary>
        /// <param name="chr">recieving character</param>
        /// <param name="owner">EntityID of sender</param>
        public static void SendPageToCharacter(GossipConversation convo, IList<QuestMenuItem> questItems)
        {
            WorldObject speaker = convo.Speaker;
            Character character = convo.Character;
            GossipMenu currentMenu = convo.CurrentMenu;
            IList<GossipMenuItemBase> gossipItems = currentMenu.GossipItems;
            IGossipEntry gossipEntry = currentMenu.GossipEntry;
            if (gossipEntry.IsDynamic)
                QueryHandler.SendNPCTextUpdate(character, gossipEntry);
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GOSSIP_MESSAGE))
            {
                packet.Write((ulong) speaker.EntityId);
                packet.Write(0);
                packet.Write(gossipEntry.GossipId);
                long position = packet.Position;
                packet.Position += 4L;
                int num = 0;
                if (gossipItems != null)
                {
                    for (int index = 0; index < gossipItems.Count; ++index)
                    {
                        GossipMenuItemBase gossipMenuItemBase = gossipItems[index];
                        if (gossipMenuItemBase.Action == null || gossipMenuItemBase.Action.CanUse(convo))
                        {
                            packet.Write(index);
                            packet.Write((byte) gossipMenuItemBase.Icon);
                            packet.Write(gossipMenuItemBase.Input);
                            packet.Write((uint) gossipMenuItemBase.RequiredMoney);
                            packet.WriteCString(gossipMenuItemBase.GetText(convo));
                            packet.WriteCString(gossipMenuItemBase.GetConfirmText(convo));
                            ++num;
                        }
                    }
                }

                if (questItems != null)
                {
                    packet.WriteUInt(questItems.Count);
                    for (int index = 0; index < questItems.Count; ++index)
                    {
                        QuestMenuItem questItem = questItems[index];
                        packet.Write(questItem.ID);
                        packet.Write(questItem.Status);
                        packet.Write(questItem.Level);
                        packet.Write(0);
                        packet.Write((byte) 0);
                        packet.WriteCString(questItem.Text);
                    }
                }
                else
                    packet.Write(0);

                packet.Position = position;
                packet.Write(num);
                character.Client.Send(packet, false);
            }
        }

        /// <summary>Sends a page to the character</summary>
        /// <param name="rcv">recieving character</param>
        public static void SendConversationComplete(IPacketReceiver rcv)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GOSSIP_COMPLETE))
                rcv.Send(packet, false);
        }

        /// <summary>
        /// Send Point of interest which will then appear on the minimap
        /// </summary>
        public static void SendGossipPOI(IPacketReceiver rcv, GossipPOIFlags Flags, float X, float Y, int Data,
            int Icon, string Name)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GOSSIP_POI))
            {
                packet.Write((uint) Flags);
                packet.Write(X);
                packet.Write(Y);
                packet.Write(Data);
                packet.Write(Icon);
                packet.WriteCString(Name);
                rcv.Send(packet, false);
            }
        }
    }
}