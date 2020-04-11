using NLog;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Handlers
{
    public static class TotemHandler
    {
        public static Logger Log = LogManager.GetCurrentClassLogger();

        public static void HandleDestroyTotem(IRealmClient client, RealmPacketIn packet)
        {
            uint num = packet.ReadUInt32();
            TotemHandler.Log.Debug("Received CMSG_TOTEM_DESTROYED for Slot {0}", num);
        }

        public static bool SendTotemCreated(IPacketReceiver client, Spell totemSpell, EntityId totemEntityId)
        {
            Character character = client as Character;
            if (character == null)
                return false;
            SpellEffect effect = totemSpell.GetEffect(SpellEffectType.Summon);
            if (effect == null)
                return false;
            uint num = effect.SummonEntry.Slot - 1U;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_TOTEM_CREATED))
            {
                packet.Write(num);
                packet.Write((ulong) totemEntityId);
                packet.Write(totemSpell.GetDuration(character.SharedReference));
                packet.Write(totemSpell.Id);
                client.Send(packet, false);
            }

            return true;
        }
    }
}