using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Handlers
{
    class Asda2CombatHandler
    {
        [PacketHandler(RealmServerOpCode.StartAtack)]//4026
        public static void StartAtackRequest(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            var mobId = packet.ReadUInt16();//default : 0Len : 2
            var target = client.ActiveCharacter.Map.GetNpcByUniqMapId(mobId);
            byte atackStatus = 3;
            if (target == null)
            {
                atackStatus = 2; //перегруз 90%
            }
            if (target != null &&
                    chr.CanHarm(target) &&
                    chr.CanSee(target))
            {
                atackStatus = 1;
            }
            
            chr.Target = target;
            chr.IsWaitingForAtackAnimation = true;
            StartAtackResponse(chr, chr.Target, atackStatus);
        }
        [PacketHandler(RealmServerOpCode.StartAtackCharacter)]//4201
        public static void StartAtackCharacterRequest(IRealmClient client, RealmPacketIn packet)
        {
            var targetSessId = packet.ReadUInt16();
            var victim = World.GetCharacterBySessionId(targetSessId);
            if(victim == null || !victim.IsAlive)
                return;
            client.ActiveCharacter.Target = victim;
            SendStartAtackCharacterResponseResponse(client.ActiveCharacter,victim);
            Asda2SpellHandler.SendSetAtackStateGuiResponse(client.ActiveCharacter);
        }

        public static void SendStartAtackCharacterResponseResponse(Character atacker,Character victim)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.StartAtackCharacterResponse))//4202
            {
                packet.WriteByte(1);//{status}default value : 1 Len : 1
                packet.WriteInt16(atacker.SessionId);//{atackerSessId}default value : 11 Len : 2
                packet.WriteInt16(victim.SessionId);//{victimSessId}default value : 105 Len : 2
                packet.WriteFloat(victim.Asda2X);//{x}default value : 0 Len : 4
                packet.WriteFloat(victim.Asda2Y);//{y}default value : 0 Len : 4
                atacker.SendPacketToArea(packet,true, true);
            }
        }

        [PacketHandler(RealmServerOpCode.AtackCharacter)]//4203
        public static void AtackCharacterRequest(IRealmClient client, RealmPacketIn packet)
        {
            var targetSessId = packet.ReadUInt16();//default : 105Len : 2
            var target = World.GetCharacterBySessionId(targetSessId);
            if(target == null || target ==client.ActiveCharacter || !client.ActiveCharacter.CanHarm(target))
            {
                StartAtackResponse(client.ActiveCharacter, target, 0);
                return;
            }
            client.ActiveCharacter.IsFighting = true;
            Asda2SpellHandler.SendSetAtackStateGuiResponse(client.ActiveCharacter);
        }
            

        [PacketHandler(RealmServerOpCode.ContinueAtack)]//4028
        public static void ContinueAtackRequest(IRealmClient client, RealmPacketIn packet)
        {
            var target = client.ActiveCharacter.Target;
            if (target == null || !client.ActiveCharacter.CanHarm(target))
                StartAtackResponse(client.ActiveCharacter, target, 0);
            client.ActiveCharacter.IsFighting = true;
        }
            

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="target"></param>
        /// <param name="status">0 - stop;1-start;2 90% weight;3 cannot see target</param>
        public static void StartAtackResponse(Character chr,Unit target, byte status)
        {
            if(chr.IsMoving)
                Asda2MovmentHandler.SendEndMoveByFastInstantRegularMoveResponse(chr);
            var npc = target as NPC;
            
                using (var p = new RealmPacketOut(RealmServerOpCode.StartAtackResponse)) //4027
                {
                    p.WriteByte(status); //{status}default value : 1 Len : 1
                    p.WriteInt16(chr.SessionId); //{sessId}default value : 13 Len : 2
                    p.WriteInt16(npc==null?-1:npc.UniqIdOnMap); //{monstrMapId}default value : 123 Len : 2
                    p.WriteInt16(0);
                    p.WriteInt16(0);
                    p.WriteInt32(npc==null?-1:npc.UniqWorldEntityId); //{UniqMonstrId}default value : 40 Len : 4
                    chr.SendPacketToArea(p, true, false);
                }
           
        }
        static readonly byte[] unk8 = new byte[] { 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x3E, 0xEF, 0xF6, 0x39, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public static void SendAttackerStateUpdate(DamageAction action)
        {
            if(action.Attacker is Character && action.Victim is NPC)
            {
                var atacker = (Character)action.Attacker;
                var victim = (NPC) action.Victim;
                using (var packet = new RealmPacketOut(RealmServerOpCode.MonstrTakeDmg))//4029
                {
                    packet.WriteInt16(atacker.SessionId);//{killerSessId}default value : 17 Len : 2
                    packet.WriteInt16(victim.UniqIdOnMap);//{monstrMapId}default value : 125 Len : 2
                    packet.WriteInt32(victim.UniqWorldEntityId);//value name : unk6 default value : 0Len : 4
                    if (action.VictimState == VictimState.Evade)
                        packet.WriteInt32(-1);
                    else if (action.VictimState == VictimState.Block)
                        packet.WriteInt32(-2);
                    else if (action.VictimState == VictimState.Immune)
                        packet.WriteInt32(-3);
                    else if (action.VictimState == VictimState.Deflect || action.VictimState == VictimState.Dodge || action.VictimState == VictimState.Interrupt || action.VictimState == VictimState.Parry)
                        packet.WriteInt32(-1);
                    else 
                        packet.WriteUInt32(action.ActualDamage + (action.IsCritical?2147483648:0));//{AmountOfDmg}default value : 482 Len : 4
                    packet.WriteSkip(unk8);//value name : unk8 default value : unk8Len : 21
                    victim.SendPacketToArea(packet);
                }
                return;
            }
            else if(action.Attacker is NPC && action.Victim is Character)
            {
                var atacker = (NPC)action.Attacker;
                var victim = (Character)action.Victim;
                var dmg = 0;
                 if (action.VictimState == VictimState.Evade)
                        dmg = -1;
                    else if (action.VictimState == VictimState.Block)
                        dmg = -2;
                    else if (action.VictimState == VictimState.Immune)
                        dmg = -3;
                    else if (action.VictimState == VictimState.Miss || action.VictimState == VictimState.Deflect || action.VictimState == VictimState.Dodge || action.VictimState == VictimState.Interrupt || action.VictimState == VictimState.Parry)
                        dmg = 0;
                    else 
                        dmg = (int) (action.ActualDamage + (action.IsCritical?2147483648:0));//{AmountOfDmg}default value : 482 Len : 4
                 
                Asda2MovmentHandler.SendMonstMoveOrAtackResponse(victim.SessionId,atacker,dmg,atacker.Asda2Position,true);
                
            }
            else if ( action.Attacker is Character && action.Victim is Character)
            {
                var dmg = 0;
                if (action.VictimState == VictimState.Evade)
                    dmg = -1;
                else if (action.VictimState == VictimState.Block)
                    dmg = -2;
                else if (action.VictimState == VictimState.Immune)
                    dmg = -3;
                else if (action.VictimState == VictimState.Miss || action.VictimState == VictimState.Deflect || action.VictimState == VictimState.Dodge || action.VictimState == VictimState.Interrupt || action.VictimState == VictimState.Parry)
                    dmg = 0;
                else
                    dmg = (int)(action.ActualDamage + (action.IsCritical ? 2147483648 : 0));//{AmountOfDmg}default value : 482 Len : 4
                using (var packet = new RealmPacketOut(RealmServerOpCode.AtackCharacterRes))//4204
                {
                    packet.WriteInt16(action.Attacker.CharacterMaster.SessionId);//{atackerSessId}default value : 11 Len : 2
                    packet.WriteInt16(action.Victim.CharacterMaster.SessionId);//{victimSessId}default value : 105 Len : 2
                    packet.WriteInt32(0);//value name : unk6 default value : 0Len : 4
                    packet.WriteInt32(dmg);//{dmg}default value : 1 Len : 4
                    packet.WriteByte(0);//value name : unk8 default value : 0Len : 1
                    packet.WriteInt16(-1);//value name : unk9 default value : -1Len : 2
                    packet.WriteInt32(0);//value name : unk4 default value : 0Len : 4
                    action.Victim.CharacterMaster.SendPacketToArea(packet,true, true);
                }

            }
        }

        public static void SendMostrDeadToAreaResponse(ICollection<IRealmClient> clients, short npcId, short x, short y)
        {
            using (var packet = CreateMonstrDeadPacket(npcId, x, y))
            {
                foreach (var realmClient in clients)
                {
                    realmClient.Send(packet, addEnd: true);
                }
            }
        }

        /* public static void SendMonstrDeadToOneClientRespone(NPC npc,IRealmClient client)
        {
            using (var packet = CreateMonstrDeadPacket(npc))
                client.Send(packet);
        }*/
        static RealmPacketOut CreateMonstrDeadPacket(short npc,short x,short y)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.MonstrStateChanged); //4017

            packet.WriteSkip(stab6); //value name : stab6 default value : stab6Len : 2
            packet.WriteInt16(npc); //{monstrMapId}default value : 128 Len : 2
            packet.WriteSkip(stab10); //value name : stab10 default value : stab10Len : 92
            packet.WriteInt16(x); //{x}default value : 74 Len : 2
            packet.WriteInt16(y); //{y}default value : 246 Len : 2
            packet.WriteInt16(8557); //value name : unk2 default value : 8557Len : 2
            return packet;
        }
        public static void SendMonstrStateChangedResponse(NPC npc, Asda2NpcState state)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.MonstrStateChanged))//4017
            {
                packet.WriteSkip(stab6);//value name : stab6 default value : stab6Len : 2
                packet.WriteInt16(npc.UniqIdOnMap);//{mobId}default value : 253 Len : 2
                packet.WriteInt32((int) state);//{status}default value : 0 Len : 4

                
                for (int i = 0; i < 28; i += 1)
                {
                    Spells.Auras.Aura aura = null;
                    if (npc.Auras.ActiveAuras.Length > i)
                        aura = npc.Auras.ActiveAuras[i].TicksLeft == 0 ? null : npc.Auras.ActiveAuras[i];//npc.Auras.VisibleAuras[i];
                    packet.WriteInt16(aura == null?-1:aura.Spell.RealId);//{effect}default value : -1 Len : 2
                }
                for (int i = 0; i < 28; i += 1)
                {
                    Spells.Auras.Aura aura = null;
                    if (npc.Auras.ActiveAuras.Length > i)
                        aura = npc.Auras.ActiveAuras[i];//npc.Auras.VisibleAuras[i];
                    packet.WriteByte(aura == null?0:1);//{effectExist}default value : 0 Len : 1

                }
                packet.WriteInt32(npc.Health);//{hp}default value : 0 Len : 4
                packet.WriteInt16((short)npc.Position.X); //{x}default value : 74 Len : 2
                packet.WriteInt16((short)npc.Position.Y); //{y}default value : 246 Len : 2
                npc.SendPacketToArea(packet,false,true);
            }
        }
        static readonly byte[] stab6 = new byte[] { 0x0E, 0x00 };
        private static readonly byte[] stab10 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static void SendNpcBuffedResponse(NPC target,Aura aura)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.NpcBuffed))//6079
            {
                packet.WriteByte(1);//{status}default value : 1 Len : 1
                packet.WriteInt16(0);//{casterSessId}default value : 23 Len : 2
                packet.WriteByte(0);//value name : unk7 default value : 0Len : 1
                packet.WriteInt16(target.UniqIdOnMap);//{targetId}default value : 83 Len : 2
                packet.WriteInt16(aura.Spell.RealId);//{spellId}default value : 912 Len : 2
                packet.WriteInt16(aura.Spell.RealId);//{effectId}default value : 71 Len : 2
                packet.WriteByte(aura.Spell.Level);//{spellLevel}default value : 3 Len : 1
                packet.WriteInt32(aura.Duration);//{duration}default value : 5000 Len : 4
                packet.WriteInt16(aura.Amplitude);//{amplitude}default value : 1000 Len : 2
                packet.WriteInt32(0);//{damagePerTick}default value : 32 Len : 4
                target.SendPacketToArea(packet,false,true);
            }
        }

    }
    public enum Asda2NpcState
    {
        Dead =0,
        Ok =1,
        IncreaceAura = 3
    }
}
