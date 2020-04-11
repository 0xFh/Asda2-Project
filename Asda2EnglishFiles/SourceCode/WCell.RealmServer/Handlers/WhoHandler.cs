using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class WhoHandler
    {
        /// <summary>Handles an incoming who list request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void WhoListRequest(IRealmClient client, RealmPacketIn packet)
        {
            WhoSearch whoSearch1 = new WhoSearch();
            whoSearch1.MaxResultCount = WhoList.MaxResultCount;
            whoSearch1.Faction = client.ActiveCharacter.Faction.Group;
            whoSearch1.MinLevel = (byte) packet.ReadUInt32();
            whoSearch1.MaxLevel = (byte) packet.ReadUInt32();
            whoSearch1.Name = packet.ReadCString();
            whoSearch1.GuildName = packet.ReadCString();
            whoSearch1.RaceMask = (RaceMask2) packet.ReadUInt32();
            whoSearch1.ClassMask = (ClassMask2) packet.ReadUInt32();
            WhoSearch whoSearch2 = whoSearch1;
            uint num1 = packet.ReadUInt32();
            if (num1 > 0U && num1 <= 10U)
            {
                for (int index = 0; (long) index < (long) num1; ++index)
                    whoSearch2.Zones.Add((ZoneId) packet.ReadUInt32());
            }

            uint num2 = packet.ReadUInt32();
            if (num2 > 0U && num2 <= 10U)
            {
                for (int index = 0; (long) index < (long) num2; ++index)
                    whoSearch2.Names.Add(packet.ReadCString().ToLower());
            }

            ICollection<Character> characters = whoSearch2.RetrieveMatchedCharacters();
            WhoHandler.SendWhoList((IPacketReceiver) client, characters);
        }

        /// <summary>
        /// Sends to the specified client the Who List based on the given characters
        /// </summary>
        /// <param name="client">The client to send the list</param>
        /// <param name="characters">The list of characters that matched the Who List search</param>
        public static void SendWhoList(IPacketReceiver client, ICollection<Character> characters)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_WHO))
            {
                packet.Write(characters.Count);
                packet.Write(characters.Count);
                foreach (Character character in (IEnumerable<Character>) characters)
                {
                    packet.WriteCString(character.Name);
                    packet.WriteCString(character.Guild != null ? character.Guild.Name : string.Empty);
                    packet.Write(character.Level);
                    packet.WriteUInt((byte) character.Class);
                    packet.WriteUInt((byte) character.Race);
                    packet.WriteByte(0);
                    packet.Write(character.Zone != null ? (uint) character.Zone.Id : 0U);
                }

                client.Send(packet, false);
            }
        }
    }
}