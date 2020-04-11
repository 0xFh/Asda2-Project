using System.IO;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class CombatHandler
    {
        public static void HandleAttackSwing(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (!activeCharacter.CanDoPhysicalActivity)
                return;
            EntityId id = packet.ReadEntityId();
            Unit opponent = activeCharacter.Map.GetObject(id) as Unit;
            if (opponent == null || !activeCharacter.CanHarm((WorldObject) opponent) ||
                !activeCharacter.CanSee((WorldObject) opponent))
                return;
            activeCharacter.Target = opponent;
            activeCharacter.IsFighting = true;
            CombatHandler.SendCombatStart((Unit) activeCharacter, opponent, true);
            CombatHandler.SendAIReaction((IPacketReceiver) client, opponent.EntityId, AIReaction.Hostile);
        }

        public static void HandleSheathedStateChanged(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.SheathType = (SheathType) packet.ReadByte();
        }

        /// <summary>The client signals stop melee fighting</summary>
        /// <param name="client"></param>
        /// <param name="packet"></param>
        public static void HandleAttackStop(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.AutorepeatSpell != null)
                return;
            client.ActiveCharacter.IsFighting = false;
        }

        public static void SendCombatStart(Unit unit, Unit opponent, bool includeSelf)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ATTACKSTART, 16))
            {
                packet.Write((ulong) unit.EntityId);
                packet.Write((ulong) opponent.EntityId);
                unit.SendPacketToArea(packet, includeSelf, false, Locale.Any, new float?());
            }
        }

        public static void SendCombatStop(Unit attacker, Unit opponent, int extraArg)
        {
            if (!attacker.IsAreaActive)
                return;
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ATTACKSTOP, 22))
            {
                attacker.EntityId.WritePacked((BinaryWriter) packet);
                if (opponent != null)
                    opponent.EntityId.WritePacked((BinaryWriter) packet);
                else
                    packet.Write((byte) 0);
                packet.Write(extraArg);
                attacker.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        public static void SendAttackerStateUpdate(DamageAction action)
        {
            if (!action.Attacker.IsAreaActive)
                return;
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ATTACKERSTATEUPDATE, 100))
            {
                bool flag = action.VictimState == VictimState.Evade;
                int num = flag ? 1 : 0;
                packet.Write((uint) action.HitFlags);
                action.Attacker.EntityId.WritePacked((BinaryWriter) packet);
                action.Victim.EntityId.WritePacked((BinaryWriter) packet);
                int actualDamage = action.ActualDamage;
                packet.Write(actualDamage);
                packet.Write(0);
                packet.Write((byte) 1);
                for (byte index = 0; index < (byte) 1; ++index)
                {
                    packet.Write((uint) action.Schools);
                    packet.Write((float) actualDamage);
                    packet.Write(actualDamage);
                }

                if (action.HitFlags.HasAnyFlag(HitFlags.AbsorbType1 | HitFlags.AbsorbType2))
                {
                    for (byte index = 0; index < (byte) 1; ++index)
                        packet.Write(action.Absorbed);
                }

                if (action.HitFlags.HasAnyFlag(HitFlags.ResistType1 | HitFlags.ResistType2))
                {
                    for (byte index = 0; index < (byte) 1; ++index)
                        packet.Write(action.Resisted);
                }

                packet.Write((byte) action.VictimState);
                if (flag)
                    packet.Write(16777218);
                else
                    packet.Write(actualDamage > 0 ? -1 : 0);
                packet.Write(0);
                if (action.HitFlags.HasAnyFlag(HitFlags.Block))
                    packet.Write(action.Blocked);
                action.Victim.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        public static void SendAttackSwingBadFacing(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ATTACKSWING_BADFACING))
                chr.Send(packet, false);
        }

        public static void SendAttackSwingNotInRange(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ATTACKSWING_NOTINRANGE))
                chr.Send(packet, false);
        }

        /// <summary>
        /// Updates the player's combo-points, will be called automatically whenever the combo points or combotarget change.
        /// </summary>
        public static void SendComboPoints(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_UPDATE_COMBO_POINTS, 9))
            {
                if (chr.ComboTarget != null)
                    chr.ComboTarget.EntityId.WritePacked((BinaryWriter) packet);
                else
                    packet.Write((byte) 0);
                packet.Write((byte) chr.ComboPoints);
                chr.Client.Send(packet, false);
            }
        }

        public static void SendAIReaction(IPacketReceiver recv, EntityId guid, AIReaction reaction)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_AI_REACTION, 12))
            {
                packet.Write((ulong) guid);
                packet.Write((uint) reaction);
                recv.Send(packet, false);
            }
        }
    }
}