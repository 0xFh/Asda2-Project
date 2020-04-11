using NLog;
using System;
using System.IO;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Threading;

namespace WCell.RealmServer.Handlers
{
    public static class QueryHandler
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        /// <summary>Handles an incoming time query.</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void QueryTimeRequest(IRealmClient client, RealmPacketIn packet)
        {
            QueryHandler.SendQueryTimeReply(client);
        }

        /// <summary>Send a "time query" reply to the client.</summary>
        /// <param name="client">the client to send to</param>
        public static void SendQueryTimeReply(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_QUERY_TIME_RESPONSE, 4))
            {
                packet.Write(Utility.GetEpochTime());
                client.Send(packet, false);
            }
        }

        /// <summary>Handles an incoming name query.</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void NameQueryRequest(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            ILivingEntity entity = (ILivingEntity) client.ActiveCharacter;
            if ((int) entity.EntityId.Low != (int) id.Low)
                entity = World.GetNamedEntity(id.Low) as ILivingEntity;
            if (entity != null)
                QueryHandler.SendNameQueryReply((IPacketReceiver) client, entity);
            else
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message((Action) (() =>
                {
                    CharacterRecord characterRecord = CharacterRecord.LoadRecordByEntityId(id.Low);
                    if (characterRecord == null)
                        QueryHandler.s_log.Warn("{0} queried name of non-existing Character: " + (object) id,
                            (object) client);
                    else
                        QueryHandler.SendNameQueryReply((IPacketReceiver) client, (ILivingEntity) characterRecord);
                })));
        }

        /// <summary>Sends a "name query" reply to the client.</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="entity">the character information to be sent</param>
        public static void SendNameQueryReply(IPacketReceiver client, ILivingEntity entity)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_NAME_QUERY_RESPONSE))
            {
                entity.EntityId.WritePacked((BinaryWriter) packet);
                packet.Write((byte) 0);
                packet.WriteCString(entity.Name);
                packet.Write((byte) 0);
                packet.Write((byte) entity.Race);
                packet.Write((byte) entity.Gender);
                packet.Write((byte) entity.Class);
                packet.Write((byte) 0);
                client.Send(packet, false);
            }
        }

        /// <summary>Handles an incoming creature name query.</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleCreatureQueryRequest(IRealmClient client, RealmPacketIn packet)
        {
            uint id = packet.ReadUInt32();
            if (id == 0U)
                return;
            NPCEntry entry = NPCMgr.GetEntry(id);
            if (entry == null)
                return;
            QueryHandler.SendCreatureQueryResponse(client, entry);
        }

        private static void SendCreatureQueryResponse(IRealmClient client, NPCEntry entry)
        {
        }

        /// <summary>Handles client's npc text query</summary>
        /// <param name="client">realm client</param>
        /// <param name="packet">packet incoming</param>
        public static void HandleNPCTextQuery(IRealmClient client, RealmPacketIn packet)
        {
            uint id = packet.ReadUInt32();
            packet.ReadEntityId();
            IGossipEntry entry = GossipMgr.GetEntry(id);
            if (entry == null)
                return;
            QueryHandler.SendNPCTextUpdate(client.ActiveCharacter, entry);
        }

        /// <summary>Sends a npc text update to the character</summary>
        /// <param name="character">recieving character</param>
        /// <param name="text">class holding all info about text</param>
        public static void SendNPCTextUpdate(Character character, IGossipEntry text)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_NPC_TEXT_UPDATE))
            {
                packet.Write(text.GossipId);
                int index1;
                for (index1 = 0; index1 < text.GossipTexts.Length; ++index1)
                {
                    GossipTextBase gossipText = text.GossipTexts[index1];
                    packet.WriteFloat(gossipText.Probability);
                    string textMale = gossipText.GetTextMale(character.GossipConversation);
                    string str = !text.IsDynamic ? gossipText.GetTextFemale(character.GossipConversation) : textMale;
                    packet.WriteCString(textMale);
                    packet.WriteCString(str);
                    packet.Write((uint) gossipText.Language);
                    for (int index2 = 0; index2 < 3; ++index2)
                        packet.Write(0L);
                }

                for (; index1 < 8; ++index1)
                {
                    packet.WriteFloat(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.Fill((byte) 0, 28);
                }

                character.Client.Send(packet, false);
            }
        }

        /// <summary>Sends a simple npc text update to the character</summary>
        /// <param name="character">recieving character</param>
        /// <param name="id">id of text to update</param>
        /// <param name="title">gossip window's title</param>
        /// <param name="text">gossip window's text</param>
        public static void SendNPCTextUpdateSimple(Character character, uint id, string title, string text)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_NPC_TEXT_UPDATE))
            {
                packet.Write(id);
                packet.WriteFloat(1);
                packet.WriteCString(title);
                packet.WriteCString(text);
                packet.Fill((byte) 0, 28);
                for (int index = 1; index < 8; ++index)
                {
                    packet.WriteFloat(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.Fill((byte) 0, 28);
                }

                character.Client.Send(packet, false);
            }
        }

        public static void HandlePageTextQuery(IRealmClient client, RealmPacketIn packet)
        {
            uint pageId = packet.ReadUInt32();
            QueryHandler.SendPageText(client.ActiveCharacter, pageId);
        }

        public static void SendPageText(Character chr, uint pageId)
        {
            PageTextEntry entry = PageTextEntry.GetEntry(pageId);
            if (entry != null)
            {
                do
                {
                    QueryHandler.SendPageText(chr, entry);
                    entry = entry.NextPageEntry;
                } while (entry != null);
            }
            else
            {
                using (RealmPacketOut packet =
                    new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PAGE_TEXT_QUERY_RESPONSE, 100))
                {
                    packet.Write("-page is missing-");
                    packet.Write(0);
                    chr.Send(packet, false);
                }
            }
        }

        public static void SendPageText(Character chr, PageTextEntry entry)
        {
            ClientLocale locale = chr.Locale;
            for (; entry != null; entry = entry.NextPageEntry)
            {
                using (RealmPacketOut packet =
                    new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PAGE_TEXT_QUERY_RESPONSE, 100))
                {
                    packet.Write(entry.PageId);
                    packet.Write(entry.Texts.Localize(locale));
                    packet.Write(entry.NextPageId);
                    chr.Send(packet, false);
                }
            }
        }
    }
}