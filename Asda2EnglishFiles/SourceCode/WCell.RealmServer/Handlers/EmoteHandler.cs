using WCell.Constants;
using WCell.Constants.Chat;
using WCell.Constants.Misc;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
    /// <summary>Handler class for emote-related packets.</summary>
    public static class EmoteHandler
    {
        public static void HandleTextEmote(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (!activeCharacter.CanMove || !activeCharacter.CanInteract)
                return;
            TextEmote emote1 = (TextEmote) packet.ReadUInt32();
            packet.SkipBytes(4);
            EntityId id = packet.ReadEntityId();
            INamed target = (INamed) activeCharacter.Map.GetObject(id);
            if (target != null)
                EmoteHandler.SendTextEmote((WorldObject) activeCharacter, emote1, target);
            EmoteType emote2;
            EmoteDBC.EmoteRelationReader.Entries.TryGetValue((int) emote1, out emote2);
            switch (emote2)
            {
                case EmoteType.StateDance:
                case EmoteType.StateSleep:
                case EmoteType.StateSit:
                case EmoteType.StateKneel:
                    activeCharacter.EmoteState = emote2;
                    break;
                default:
                    activeCharacter.Emote(emote2);
                    break;
            }
        }

        public static void SendTextEmote(WorldObject obj, TextEmote emote, INamed target)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_TEXT_EMOTE,
                target == null ? 20 : target.Name.Length + 21))
            {
                packet.Write((ulong) obj.EntityId);
                packet.WriteUInt((uint) emote);
                packet.WriteInt(-1);
                packet.WriteUIntPascalString(target != null ? target.Name : "");
                obj.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        public static void SendEmote(WorldObject obj, EmoteType emote)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_EMOTE, 12))
            {
                packet.WriteUInt((uint) emote);
                packet.Write((ulong) obj.EntityId);
                obj.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }
    }
}