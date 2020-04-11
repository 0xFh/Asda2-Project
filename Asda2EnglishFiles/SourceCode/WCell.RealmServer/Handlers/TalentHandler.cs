using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Constants.Talents;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.Talents;

namespace WCell.RealmServer.Handlers
{
    public static class TalentHandler
    {
        public static void HandleLearnTalent(IRealmClient client, RealmPacketIn packet)
        {
            TalentId id = (TalentId) packet.ReadUInt32();
            int rank = packet.ReadInt32();
            TalentCollection talents = client.ActiveCharacter.Talents;
            if (talents.Learn(id, rank) == null)
                return;
            TalentHandler.SendTalentGroupList(talents);
        }

        public static void HandleClearTalents(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.Talents.ResetTalents();
        }

        public static void HandleSaveTalentGroup(IRealmClient client, RealmPacketIn packet)
        {
            int num = packet.ReadInt32();
            TalentCollection talents = client.ActiveCharacter.Talents;
            for (int index = 0; index < num; ++index)
            {
                TalentId id = (TalentId) packet.ReadUInt32();
                int rank = packet.ReadInt32();
                talents.Learn(id, rank);
            }

            TalentHandler.SendTalentGroupList(talents);
        }

        public static void HandleRemoveGlyph(IRealmClient client, RealmPacketIn packet)
        {
            uint num = packet.ReadUInt32();
            Character activeCharacter = client.ActiveCharacter;
            activeCharacter.RemoveGlyph((byte) num);
            TalentHandler.SendTalentGroupList(activeCharacter.Talents);
        }

        /// <summary>
        /// Sends a request to wipe all talents, which must be confirmed by the player
        /// </summary>
        public static void SendClearQuery(TalentCollection talents)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.MSG_TALENT_WIPE_CONFIRM, 12))
            {
                packet.Write((ulong) talents.Owner.EntityId);
                packet.Write(talents.GetResetPrice());
                talents.OwnerCharacter.Send(packet, false);
            }
        }

        public static void SendTalentGroupList(TalentCollection talents)
        {
            TalentHandler.SendTalentGroupList(talents, talents.CurrentSpecIndex);
        }

        /// <summary>Sends the client the list of talents</summary>
        /// <param name="hasTalents">The IHasTalents to send the list from</param>
        public static void SendTalentGroupList(TalentCollection talents, int talentGroupId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_TALENTS_INFO))
            {
                Unit owner = talents.Owner;
                bool flag = owner is Character;
                packet.Write(flag ? (byte) 0 : (byte) 1);
                if (flag)
                {
                    TalentHandler.WritePlayerTalentList((BinaryWriter) packet, (Character) owner, talentGroupId);
                }
                else
                {
                    packet.Write(talents.FreeTalentPoints);
                    packet.Write((byte) talents.Count);
                    foreach (Talent talent in talents)
                    {
                        packet.Write((int) talent.Entry.Id);
                        packet.Write((byte) talent.Rank);
                    }
                }

                talents.OwnerCharacter.Send(packet, false);
            }
        }

        public static void SendInspectTalents(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_INSPECT_TALENT))
            {
                chr.EntityId.WritePacked((BinaryWriter) packet);
                TalentHandler.WritePlayerTalentList((BinaryWriter) packet, chr, chr.Talents.CurrentSpecIndex);
                chr.Client.Send(packet, false);
            }
        }

        private static void WritePlayerTalentList(BinaryWriter packet, Character chr, int talentGroupId)
        {
            SpecProfile currentSpecProfile = chr.CurrentSpecProfile;
            byte specProfileCount = (byte) chr.Talents.SpecProfileCount;
            packet.Write(chr.FreeTalentPoints);
            packet.Write(specProfileCount);
            packet.Write((byte) talentGroupId);
            if (specProfileCount <= (byte) 0)
                return;
            Dictionary<TalentId, Talent> byId = chr.Talents.ById;
            uint[] glyphIds = currentSpecProfile.GlyphIds;
            for (int index1 = 0; index1 < (int) specProfileCount; ++index1)
            {
                packet.Write((byte) byId.Count);
                foreach (KeyValuePair<TalentId, Talent> keyValuePair in byId)
                {
                    packet.Write((int) keyValuePair.Key);
                    packet.Write((byte) keyValuePair.Value.Rank);
                }

                if (glyphIds != null)
                {
                    packet.Write((byte) 6);
                    for (int index2 = 0; index2 < 6; ++index2)
                        packet.Write((short) glyphIds[index2]);
                }
                else
                    packet.Write((byte) 0);
            }
        }
    }
}