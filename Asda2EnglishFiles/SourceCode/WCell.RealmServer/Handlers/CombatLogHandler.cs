using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Handlers
{
    public static class CombatLogHandler
    {
        /// <summary>Used for any PeriodicAura (repeating ticks)</summary>
        /// <param name="extra">Always seems to be one</param>
        public static void SendPeriodicAuraLog(IPacketReceiver client, WorldObject caster, WorldObject target,
            uint spellId, uint extra, AuraTickFlags flags, int amount)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PERIODICAURALOG, 32))
            {
                caster.EntityId.WritePacked((BinaryWriter) packet);
                target.EntityId.WritePacked((BinaryWriter) packet);
                packet.WriteUInt(spellId);
                packet.WriteUInt(extra);
                packet.WriteUInt((uint) flags);
                packet.WriteUInt(amount);
                target.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        /// <summary>Used for Periodic leech effects, mostly Cannibalism</summary>
        /// <returns></returns>
        public static void SendPeriodicDamage(WorldObject caster, WorldObject target, uint spellId, AuraTickFlags type,
            int amount)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PERIODICAURALOG, 32))
            {
                caster.EntityId.WritePacked((BinaryWriter) packet);
                target.EntityId.WritePacked((BinaryWriter) packet);
                packet.WriteUInt(spellId);
                packet.WriteUInt(1);
                packet.WriteUInt((uint) type);
                packet.WriteUInt(amount);
                target.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        /// <summary>Correct 3.0.9</summary>
        public static void SendSpellMiss(SpellCast cast, bool display, ICollection<MissedTarget> missedTargets)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELLLOGMISS, 34))
            {
                packet.Write(cast.Spell.Id);
                packet.Write((ulong) cast.CasterReference.EntityId);
                packet.Write(display);
                packet.Write(missedTargets.Count);
                foreach (MissedTarget missedTarget in (IEnumerable<MissedTarget>) missedTargets)
                {
                    packet.Write((ulong) missedTarget.Target.EntityId);
                    packet.Write((byte) missedTarget.Reason);
                }

                cast.SendPacketToArea(packet);
            }
        }

        /// <summary>Correct for 3.0.9</summary>
        /// <param name="client"></param>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <param name="spellId"></param>
        /// <param name="b1"></param>
        public static void SendSpellOrDamageImmune(IPacketReceiver client, ObjectBase obj1, ObjectBase obj2,
            int spellId, bool b1)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELLORDAMAGE_IMMUNE, 21))
            {
                packet.Write((ulong) obj1.EntityId);
                packet.Write((ulong) obj2.EntityId);
                packet.Write(spellId);
                packet.Write(b1);
                client.Send(packet, false);
            }
        }

        /// <summary>Correct for 3.0.9</summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="spellId"></param>
        /// <param name="powerType"></param>
        /// <param name="value"></param>
        public static void SendEnergizeLog(WorldObject caster, Unit target, uint spellId, PowerType powerType,
            int value)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELLENERGIZELOG, 25))
            {
                target.EntityId.WritePacked((BinaryWriter) packet);
                caster.EntityId.WritePacked((BinaryWriter) packet);
                packet.Write(spellId);
                packet.Write((int) powerType);
                packet.Write(value);
                caster.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        /// <summary>Correct for 3.0.9</summary>
        /// <param name="target">Optional</param>
        /// <param name="value">Optional</param>
        public static RealmPacketOut SendSpellLogExecute(ObjectBase caster, uint spellId, SpellEffectType effect,
            ObjectBase target, uint value)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELLLOGEXECUTE, 37);
            caster.EntityId.WritePacked((BinaryWriter) realmPacketOut);
            realmPacketOut.Write(spellId);
            realmPacketOut.Write(1);
            for (int index1 = 0; index1 < 1; ++index1)
            {
                realmPacketOut.Write((int) effect);
                for (int index2 = 0; index2 < 1; ++index2)
                {
                    switch (effect)
                    {
                        case SpellEffectType.PowerDrain:
                            target.EntityId.WritePacked((BinaryWriter) realmPacketOut);
                            realmPacketOut.Write(0);
                            realmPacketOut.Write(0);
                            realmPacketOut.Write(0.0f);
                            break;
                        case SpellEffectType.Resurrect:
                        case SpellEffectType.ResurrectFlat:
                            if (target is Unit)
                            {
                                target.EntityId.WritePacked((BinaryWriter) realmPacketOut);
                                break;
                            }

                            realmPacketOut.Write((byte) 0);
                            break;
                        case SpellEffectType.AddExtraAttacks:
                            target.EntityId.WritePacked((BinaryWriter) realmPacketOut);
                            realmPacketOut.Write(0);
                            break;
                        case SpellEffectType.CreateItem:
                        case SpellEffectType.CreateItem2:
                            realmPacketOut.Write(0);
                            break;
                        case SpellEffectType.Summon:
                        case SpellEffectType.TransformItem:
                        case SpellEffectType.SummonPet:
                        case SpellEffectType.SummonObjectWild:
                        case SpellEffectType.CreateHouse:
                        case SpellEffectType.Duel:
                        case SpellEffectType.SummonObjectSlot1:
                        case SpellEffectType.SummonObjectSlot2:
                        case SpellEffectType.SummonObjectSlot3:
                        case SpellEffectType.SummonObjectSlot4:
                            if (target is Unit)
                            {
                                target.EntityId.WritePacked((BinaryWriter) realmPacketOut);
                                break;
                            }

                            realmPacketOut.Write((byte) 0);
                            break;
                        case SpellEffectType.OpenLock:
                        case SpellEffectType.OpenLockItem:
                            if (target is Item)
                            {
                                target.EntityId.WritePacked((BinaryWriter) realmPacketOut);
                                break;
                            }

                            realmPacketOut.Write((byte) 0);
                            break;
                        case SpellEffectType.InterruptCast:
                            realmPacketOut.Write(0);
                            break;
                        case SpellEffectType.FeedPet:
                            if (target is Item)
                            {
                                realmPacketOut.Write(target.EntryId);
                                break;
                            }

                            realmPacketOut.Write(0);
                            break;
                        case SpellEffectType.DismissPet:
                            target.EntityId.WritePacked((BinaryWriter) realmPacketOut);
                            break;
                        case SpellEffectType.DurabilityDamage:
                            realmPacketOut.Write(0);
                            realmPacketOut.Write(0);
                            break;
                    }
                }
            }

            return realmPacketOut;
        }

        public static void SendHealLog(WorldObject caster, Unit target, uint spellId, int value, bool critical,
            int overheal)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELLHEALLOG, 25))
            {
                target.EntityId.WritePacked((BinaryWriter) packet);
                caster.EntityId.WritePacked((BinaryWriter) packet);
                packet.Write(spellId);
                packet.Write(value);
                packet.Write(overheal);
                packet.Write(0);
                packet.Write(critical ? (byte) 1 : (byte) 0);
                packet.Write((byte) 0);
                target.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        /// <summary>
        /// Usually caused by jumping too high, diving too long, standing too close to fire etc
        /// </summary>
        public static void SendEnvironmentalDamage(WorldObject target, EnviromentalDamageType type, uint totalDamage)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ENVIRONMENTALDAMAGELOG, 21))
            {
                target.EntityId.WritePacked((BinaryWriter) packet);
                packet.WriteByte((byte) type);
                packet.WriteUInt(totalDamage);
                packet.WriteULong(0);
                target.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        /// <summary>
        /// Any spell and ranged damage
        /// SMSG_SPELLNONMELEEDAMAGELOG
        /// </summary>
        public static void SendMagicDamage(DamageAction state)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELLNONMELEEDAMAGELOG, 40))
            {
                state.Victim.EntityId.WritePacked((BinaryWriter) packet);
                if (state.Attacker != null)
                    state.Attacker.EntityId.WritePacked((BinaryWriter) packet);
                else
                    packet.Write((byte) 0);
                packet.Write(state.SpellEffect != null ? state.SpellEffect.Spell.Id : 0U);
                packet.Write(state.Damage);
                packet.Write(0);
                packet.Write((byte) state.Schools);
                packet.Write(state.Absorbed);
                packet.Write(state.Resisted);
                packet.Write(state.Schools.HasAnyFlag(DamageSchoolMask.Physical));
                packet.Write((byte) 0);
                packet.Write(state.Blocked);
                SpellLogFlags spellLogFlags = state.IsCritical ? SpellLogFlags.Critical : SpellLogFlags.None;
                packet.Write((int) spellLogFlags);
                packet.Write((byte) 0);
                state.Victim.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        public static void SendMagicDamage(WorldObject victim, IEntity attacker, SpellId spell, uint damage,
            uint overkill, DamageSchoolMask schools, uint absorbed, uint resisted, uint blocked, bool unkBool,
            SpellLogFlags flags)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELLNONMELEEDAMAGELOG, 40))
            {
                victim.EntityId.WritePacked((BinaryWriter) packet);
                if (attacker != null)
                    attacker.EntityId.WritePacked((BinaryWriter) packet);
                else
                    packet.Write((byte) 0);
                packet.Write((uint) spell);
                packet.Write(damage);
                packet.Write(overkill);
                packet.Write((byte) schools);
                packet.Write(absorbed);
                packet.Write(resisted);
                packet.Write(0);
                packet.Write(unkBool);
                packet.Write(blocked);
                packet.Write((uint) flags);
                packet.Write((byte) 0);
                victim.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }
    }
}