using System.Collections.Generic;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Handlers
{
    internal class Asda2CombatHandler
    {
        private static readonly byte[] unk8 = new byte[21]
        {
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 62,
            (byte) 239,
            (byte) 246,
            (byte) 57,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stab6 = new byte[2]
        {
            (byte) 14,
            (byte) 0
        };

        private static readonly byte[] stab10 = new byte[92]
        {
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        [PacketHandler(RealmServerOpCode.StartAtack)]
        public static void StartAtackRequest(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            ushort id = packet.ReadUInt16();
            NPC npcByUniqMapId = client.ActiveCharacter.Map.GetNpcByUniqMapId(id);
            byte status = 3;
            if (npcByUniqMapId == null)
                status = (byte) 2;
            if (npcByUniqMapId != null && activeCharacter.CanHarm((WorldObject) npcByUniqMapId) &&
                activeCharacter.CanSee((WorldObject) npcByUniqMapId))
                status = (byte) 1;
            activeCharacter.Target = (Unit) npcByUniqMapId;
            activeCharacter.IsWaitingForAtackAnimation = true;
            Asda2CombatHandler.StartAtackResponse(activeCharacter, activeCharacter.Target, status);
        }

        [PacketHandler(RealmServerOpCode.StartAtackCharacter)]
        public static void StartAtackCharacterRequest(IRealmClient client, RealmPacketIn packet)
        {
            Character characterBySessionId = World.GetCharacterBySessionId(packet.ReadUInt16());
            if (characterBySessionId == null || !characterBySessionId.IsAlive)
                return;
            if ((int) client.ActiveCharacter.Asda2FactionId == (int) characterBySessionId.Asda2FactionId &&
                characterBySessionId.IsAsda2BattlegroundInProgress &&
                client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                Asda2CombatHandler.SendStartAtackCharacterError(client.ActiveCharacter, characterBySessionId,
                    Asda2CharacterAtackStatus.Fail);
            }
            else
            {
                client.ActiveCharacter.Target = (Unit) characterBySessionId;
                Asda2CombatHandler.SendStartAtackCharacterResponseResponse(client.ActiveCharacter,
                    characterBySessionId);
                Asda2SpellHandler.SendSetAtackStateGuiResponse(client.ActiveCharacter);
            }
        }

        public static void SendStartAtackCharacterResponseResponse(Character atacker, Character victim)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.StartAtackCharacterResponse))
            {
                packet.WriteByte(1);
                packet.WriteInt16(atacker.SessionId);
                packet.WriteInt16(victim.SessionId);
                packet.WriteFloat(victim.Asda2X);
                packet.WriteFloat(victim.Asda2Y);
                atacker.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendStartAtackCharacterError(Character atacker, Character victim,
            Asda2CharacterAtackStatus status)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.StartAtackCharacterResponse))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(atacker.SessionId);
                packet.WriteInt16(victim.SessionId);
                packet.WriteFloat(victim.Asda2X);
                packet.WriteFloat(victim.Asda2Y);
                atacker.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.AtackCharacter)]
        public static void AtackCharacterRequest(IRealmClient client, RealmPacketIn packet)
        {
            Character characterBySessionId = World.GetCharacterBySessionId(packet.ReadUInt16());
            if (characterBySessionId == null || characterBySessionId == client.ActiveCharacter ||
                !client.ActiveCharacter.CanHarm((WorldObject) characterBySessionId))
            {
                Asda2CombatHandler.StartAtackResponse(client.ActiveCharacter, (Unit) characterBySessionId, (byte) 0);
            }
            else
            {
                client.ActiveCharacter.IsFighting = true;
                Asda2SpellHandler.SendSetAtackStateGuiResponse(client.ActiveCharacter);
            }
        }

        [PacketHandler(RealmServerOpCode.ContinueAtack)]
        public static void ContinueAtackRequest(IRealmClient client, RealmPacketIn packet)
        {
            Unit target = client.ActiveCharacter.Target;
            if (target == null || !client.ActiveCharacter.CanHarm((WorldObject) target))
                Asda2CombatHandler.StartAtackResponse(client.ActiveCharacter, target, (byte) 0);
            client.ActiveCharacter.IsFighting = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="target"></param>
        /// <param name="status">0 - stop;1-start;2 90% weight;3 cannot see target</param>
        public static void StartAtackResponse(Character chr, Unit target, byte status)
        {
            if (chr.IsMoving)
                Asda2MovmentHandler.SendEndMoveByFastInstantRegularMoveResponse(chr);
            NPC npc = target as NPC;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.StartAtackResponse))
            {
                packet.WriteByte(status);
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt16(npc == null ? -1 : (int) npc.UniqIdOnMap);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt32(npc == null ? -1 : npc.UniqWorldEntityId);
                chr.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        public static void SendAttackerStateUpdate(DamageAction action)
        {
            if (action.Attacker is Character && action.Victim is NPC)
            {
                Character attacker = (Character) action.Attacker;
                NPC victim = (NPC) action.Victim;
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MonstrTakeDmg))
                {
                    packet.WriteInt16(attacker.SessionId);
                    packet.WriteInt16(victim.UniqIdOnMap);
                    packet.WriteInt32(victim.UniqWorldEntityId);
                    if (action.VictimState == VictimState.Evade)
                        packet.WriteInt32(-1);
                    else if (action.VictimState == VictimState.Block)
                        packet.WriteInt32(-2);
                    else if (action.VictimState == VictimState.Immune)
                        packet.WriteInt32(-3);
                    else if (action.VictimState == VictimState.Deflect || action.VictimState == VictimState.Dodge ||
                             (action.VictimState == VictimState.Interrupt || action.VictimState == VictimState.Parry))
                        packet.WriteInt32(-1);
                    else
                        packet.WriteUInt32((long) action.ActualDamage + (action.IsCritical ? 2147483648L : 0L));
                    packet.WriteSkip(Asda2CombatHandler.unk8);
                    victim.SendPacketToArea(packet, true, true, Locale.Any, new float?());
                }
            }
            else if (action.Attacker is NPC && action.Victim is Character)
            {
                NPC attacker = (NPC) action.Attacker;
                Character victim = (Character) action.Victim;
                int dmg = action.VictimState != VictimState.Evade
                    ? (action.VictimState != VictimState.Block
                        ? (action.VictimState != VictimState.Immune
                            ? (action.VictimState == VictimState.Miss || action.VictimState == VictimState.Deflect ||
                               (action.VictimState == VictimState.Dodge ||
                                action.VictimState == VictimState.Interrupt) || action.VictimState == VictimState.Parry
                                ? 0
                                : (int) ((long) action.ActualDamage + (action.IsCritical ? 2147483648L : 0L)))
                            : -3)
                        : -2)
                    : -1;
                Asda2MovmentHandler.SendMonstMoveOrAtackResponse(victim.SessionId, attacker, dmg,
                    attacker.Asda2Position, true);
            }
            else
            {
                if (!(action.Attacker is Character) || !(action.Victim is Character))
                    return;
                int val = action.VictimState != VictimState.Evade
                    ? (action.VictimState != VictimState.Block
                        ? (action.VictimState != VictimState.Immune
                            ? (action.VictimState == VictimState.Miss || action.VictimState == VictimState.Deflect ||
                               (action.VictimState == VictimState.Dodge ||
                                action.VictimState == VictimState.Interrupt) || action.VictimState == VictimState.Parry
                                ? 0
                                : (int) ((long) action.ActualDamage + (action.IsCritical ? 2147483648L : 0L)))
                            : -3)
                        : -2)
                    : -1;
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AtackCharacterRes))
                {
                    packet.WriteInt16(action.Attacker.CharacterMaster.SessionId);
                    packet.WriteInt16(action.Victim.CharacterMaster.SessionId);
                    packet.WriteInt32(0);
                    packet.WriteInt32(val);
                    packet.WriteByte(0);
                    packet.WriteInt16(-1);
                    packet.WriteInt32(0);
                    action.Victim.CharacterMaster.SendPacketToArea(packet, true, true, Locale.Any, new float?());
                }
            }
        }

        public static void SendMostrDeadToAreaResponse(ICollection<IRealmClient> clients, short npcId, short x, short y)
        {
            using (RealmPacketOut monstrDeadPacket = Asda2CombatHandler.CreateMonstrDeadPacket(npcId, x, y))
            {
                foreach (IPacketReceiver client in (IEnumerable<IRealmClient>) clients)
                    client.Send(monstrDeadPacket, true);
            }
        }

        private static RealmPacketOut CreateMonstrDeadPacket(short npc, short x, short y)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.MonstrStateChanged);
            realmPacketOut.WriteSkip(Asda2CombatHandler.stab6);
            realmPacketOut.WriteInt16(npc);
            realmPacketOut.WriteSkip(Asda2CombatHandler.stab10);
            realmPacketOut.WriteInt16(x);
            realmPacketOut.WriteInt16(y);
            realmPacketOut.WriteInt16(8557);
            return realmPacketOut;
        }

        public static void SendMonstrStateChangedResponse(NPC npc, Asda2NpcState state)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MonstrStateChanged))
            {
                packet.WriteSkip(Asda2CombatHandler.stab6);
                packet.WriteInt16(npc.UniqIdOnMap);
                packet.WriteInt32((int) state);
                for (int index = 0; index < 28; ++index)
                {
                    Aura aura = (Aura) null;
                    if (npc.Auras.ActiveAuras.Length > index)
                        aura = npc.Auras.ActiveAuras[index].TicksLeft == 0 ? (Aura) null : npc.Auras.ActiveAuras[index];
                    packet.WriteInt16(aura == null ? -1 : (int) aura.Spell.RealId);
                }

                for (int index = 0; index < 28; ++index)
                {
                    Aura aura = (Aura) null;
                    if (npc.Auras.ActiveAuras.Length > index)
                        aura = npc.Auras.ActiveAuras[index];
                    packet.WriteByte(aura == null ? 0 : 1);
                }

                packet.WriteInt32(npc.Health);
                packet.WriteInt16((short) npc.Position.X);
                packet.WriteInt16((short) npc.Position.Y);
                npc.SendPacketToArea(packet, false, true, Locale.Any, new float?());
            }
        }

        public static void SendNpcBuffedResponse(NPC target, Aura aura)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.NpcBuffed))
            {
                packet.WriteByte(1);
                packet.WriteInt16(0);
                packet.WriteByte(0);
                packet.WriteInt16(target.UniqIdOnMap);
                packet.WriteInt16(aura.Spell.RealId);
                packet.WriteInt16(aura.Spell.RealId);
                packet.WriteByte(aura.Spell.Level);
                packet.WriteInt32(aura.Duration);
                packet.WriteInt16(aura.Amplitude);
                packet.WriteInt32(0);
                target.SendPacketToArea(packet, false, true, Locale.Any, new float?());
            }
        }
    }
}