using System;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Spells;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Handlers
{
    internal class Asda2SpellHandler
    {
        private static readonly byte[] unk12 = new byte[15]
        {
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
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
            (byte) 0
        };

        private static readonly byte[] unk14 = new byte[21]
        {
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
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stub14 = new byte[20]
        {
            (byte) 0,
            (byte) 0,
            (byte) 198,
            (byte) 112,
            (byte) 211,
            (byte) 37,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 1,
            (byte) 0
        };

        private static readonly byte[] stab7 = new byte[2]
        {
            (byte) 5,
            (byte) 0
        };

        private static readonly byte[] stab16 = new byte[1]
        {
            (byte) 1
        };

        private static readonly byte[] stub87 = new byte[28];
        private static readonly byte[] stab12 = new byte[2];

        private static readonly byte[] stab24 = new byte[16]
        {
            (byte) 8,
            (byte) 0,
            (byte) 224,
            (byte) 147,
            (byte) 4,
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

        [PacketHandler(RealmServerOpCode.UseSkill)]
        public static void UseSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.IsFighting = false;
            client.ActiveCharacter.IsMoving = false;
            short skillId = packet.ReadInt16();
            ++packet.Position;
            int num1 = (int) packet.ReadInt16();
            int num2 = (int) packet.ReadInt16();
            byte targetType = packet.ReadByte();
            ushort targetId = packet.ReadUInt16();
            Spell spellByRealId = client.ActiveCharacter.Spells.GetSpellByRealId(skillId);
            if (spellByRealId == null)
                return;
            if (spellByRealId.SoulGuardProffLevel != (byte) 0)
                client.ActiveCharacter.YouAreFuckingCheater("Trying to use SoulguardSkill as normal skill.", 1);
            else
                Asda2SpellHandler.ProcessUseSkill(client, targetType, skillId, targetId);
        }

        [PacketHandler(RealmServerOpCode.CancelSkill)]
        public static void CancelSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            ++packet.Position;
            short skillId = packet.ReadInt16();
            client.ActiveCharacter.Auras.RemoveFirstVisibleAura((Predicate<Aura>) (a =>
            {
                if ((int) a.Spell.RealId == (int) skillId)
                    return a.IsBeneficial;
                return false;
            }));
        }

        private static void ProcessUseSkill(IRealmClient client, byte targetType, short skillId, ushort targetId)
        {
            Unit target = (Unit) null;
            switch (targetType)
            {
                case 0:
                    target = (Unit) client.ActiveCharacter.Map.GetNpcByUniqMapId(targetId);
                    break;
                case 1:
                    target = (Unit) World.GetCharacterBySessionId(targetId);
                    break;
                default:
                    client.ActiveCharacter.SendSystemMessage(string.Format(
                        "Unknown skill target type {0}. SkillId {1}. Please report to developers.", (object) targetType,
                        (object) skillId));
                    break;
            }

            if (target == null)
            {
                Asda2SpellHandler.SendUseSkillResultResponse(client.ActiveCharacter, skillId,
                    Asda2UseSkillResult.ChooseATarget);
            }
            else
            {
                if (targetType == (byte) 1)
                {
                    Character characterBySessionId = World.GetCharacterBySessionId(targetId);
                    if ((int) characterBySessionId.Asda2FactionId == (int) client.ActiveCharacter.Asda2FactionId &&
                        client.ActiveCharacter.IsAsda2BattlegroundInProgress &&
                        characterBySessionId.IsAsda2BattlegroundInProgress)
                    {
                        Asda2SpellHandler.SendUseSkillResultResponse(client.ActiveCharacter, skillId,
                            Asda2UseSkillResult.YouCannotUseSkillToTargetYet);
                        return;
                    }

                    if (characterBySessionId == null)
                    {
                        Asda2SpellHandler.SendUseSkillResultResponse(client.ActiveCharacter, skillId,
                            Asda2UseSkillResult.ChooseATarget);
                        return;
                    }
                }

                Spell spellByRealId = client.ActiveCharacter.Spells.GetSpellByRealId(skillId);
                if (spellByRealId == null)
                    return;
                switch (client.ActiveCharacter.SpellCast.Start(spellByRealId, target))
                {
                    case SpellFailedReason.OutOfRange:
                        Asda2MovmentHandler.MoveToSelectedTargetAndAttack(client.ActiveCharacter);
                        break;
                    case SpellFailedReason.Ok:
                        if (spellByRealId.LearnLevel >= (byte) 10)
                        {
                            if (spellByRealId.LearnLevel < (byte) 30)
                            {
                                if (client.ActiveCharacter.GreenCharges < (byte) 10)
                                    ++client.ActiveCharacter.GreenCharges;
                            }
                            else if (spellByRealId.LearnLevel < (byte) 50)
                            {
                                if (client.ActiveCharacter.BlueCharges < (byte) 10)
                                    ++client.ActiveCharacter.BlueCharges;
                                if (client.ActiveCharacter.GreenCharges < (byte) 10)
                                    ++client.ActiveCharacter.GreenCharges;
                            }
                            else
                            {
                                if (client.ActiveCharacter.RedCharges < (byte) 10)
                                    ++client.ActiveCharacter.RedCharges;
                                if (client.ActiveCharacter.BlueCharges < (byte) 10)
                                    ++client.ActiveCharacter.BlueCharges;
                                if (client.ActiveCharacter.GreenCharges < (byte) 10)
                                    ++client.ActiveCharacter.GreenCharges;
                            }
                        }

                        AchievementProgressRecord progressRecord =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(6U);
                        switch (++progressRecord.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Skilled44);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Skilled44);
                                break;
                        }

                        progressRecord.SaveAndFlush();
                        Asda2SpellHandler.SendSetSkiillPowersStatsResponse(client.ActiveCharacter, true, skillId);
                        break;
                }
            }
        }

        [PacketHandler(RealmServerOpCode.UseSoulGuardSkill)]
        public static void UseSoulGuardSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.IsFighting = false;
            client.ActiveCharacter.IsMoving = false;
            short skillId = packet.ReadInt16();
            ++packet.Position;
            int num1 = (int) packet.ReadInt16();
            int num2 = (int) packet.ReadInt16();
            byte targetType = packet.ReadByte();
            ushort targetId = packet.ReadUInt16();
            Spell spellByRealId = client.ActiveCharacter.Spells.GetSpellByRealId(skillId);
            if (spellByRealId == null)
                return;
            if (spellByRealId.SoulGuardProffLevel < (byte) 1 || spellByRealId.SoulGuardProffLevel > (byte) 3)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to use skill as SoulguardSkill.", 1);
            }
            else
            {
                switch (spellByRealId.SoulGuardProffLevel)
                {
                    case 1:
                        if (client.ActiveCharacter.GreenCharges < (byte) 5)
                        {
                            client.ActiveCharacter.SendInfoMsg("Not enougt charges.");
                            Asda2SpellHandler.SendSetSkiillPowersStatsResponse(client.ActiveCharacter, false,
                                (short) 0);
                            return;
                        }

                        client.ActiveCharacter.GreenCharges -= (byte) 5;
                        break;
                    case 2:
                        if (client.ActiveCharacter.BlueCharges < (byte) 5)
                        {
                            client.ActiveCharacter.SendInfoMsg("Not enougt charges.");
                            Asda2SpellHandler.SendSetSkiillPowersStatsResponse(client.ActiveCharacter, false,
                                (short) 0);
                            return;
                        }

                        client.ActiveCharacter.BlueCharges -= (byte) 5;
                        break;
                    case 3:
                        if (client.ActiveCharacter.RedCharges < (byte) 5)
                        {
                            client.ActiveCharacter.SendInfoMsg("Not enougt charges.");
                            Asda2SpellHandler.SendSetSkiillPowersStatsResponse(client.ActiveCharacter, false,
                                (short) 0);
                            return;
                        }

                        client.ActiveCharacter.RedCharges -= (byte) 5;
                        break;
                }

                Asda2SpellHandler.ProcessUseSkill(client, targetType, skillId, targetId);
                Asda2SpellHandler.SendSetSkiillPowersStatsResponse(client.ActiveCharacter, false, (short) 0);
            }
        }

        public static void SendSetSkiillPowersStatsResponse(Character chr, bool animate, short skillId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SetSkiillPowersStats))
            {
                packet.WriteInt32(355335);
                packet.WriteByte(animate ? 1 : 0);
                packet.WriteByte((byte) chr.Archetype.ClassId);
                packet.WriteInt16(skillId);
                packet.WriteByte(chr.GreenCharges);
                packet.WriteByte(chr.BlueCharges);
                packet.WriteByte(chr.RedCharges);
                chr.Send(packet, true);
            }
        }

        /// <summary>Clears a single spell's cooldown</summary>
        public static void SendClearCoolDown(Character chr, SpellId spellId)
        {
            Spell spell = SpellHandler.Get(spellId);
            if (spell == null)
                chr.SendSystemMessage(string.Format("Can't clear cooldown for {0} cause skill not exist.",
                    (object) spellId));
            else
                Asda2SpellHandler.SendClearCoolDown(chr, spell.RealId);
        }

        public static void SendClearCoolDown(Character chr, short realId)
        {
            if (chr == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SkillReady))
            {
                packet.WriteInt16(realId);
                chr.Send(packet, true);
            }
        }

        public static void SendSetSkillCooldownResponse(Character chr, Spell spell)
        {
            if (chr == null || spell == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SetSkillCooldown))
            {
                packet.WriteByte(1);
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt16(spell.RealId);
                packet.WriteInt16(2);
                chr.Send(packet, false);
            }
        }

        public static void SendBuffEndedResponse(Character chr, short buffId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.BuffEnded))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt16(buffId);
                chr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendUseSkillResultResponse(Character chr, short skillId, Asda2UseSkillResult status)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UseSkillResult))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt16(skillId);
                packet.WriteByte(0);
                packet.WriteInt16(-1);
                chr.Send(packet, false);
            }
        }

        public static void SendMonstrUsedSkillResponse(NPC caster, short skillId, Unit initialTarget,
            DamageAction[] actions)
        {
            if (caster == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MonstrUsedSkill))
            {
                Character character = initialTarget as Character;
                packet.WriteByte(0);
                packet.WriteInt16(skillId);
                packet.WriteInt16(caster.UniqIdOnMap);
                packet.WriteByte(0);
                packet.WriteByte(1);
                packet.WriteInt16(character == null ? 0 : (int) character.SessionId);
                int num1 = 0;
                if (actions != null)
                {
                    foreach (DamageAction action in actions)
                    {
                        if (num1 <= 16 && action != null)
                        {
                            Character victim = action.Victim as Character;
                            packet.WriteByte(1);
                            packet.WriteInt16(victim == null ? 0 : (int) victim.SessionId);
                            int num2 = action.ActualDamage;
                            if (num2 < 0 || num2 > 200000000)
                                num2 = 0;
                            packet.WriteInt32(actions.Length == 0 ? 0 : num2);
                            packet.WriteByte(actions.Length == 0 ? 3 : 1);
                            packet.WriteSkip(Asda2SpellHandler.unk14);
                            ++num1;
                        }
                        else
                            break;
                    }
                }

                caster.SendPacketToArea(packet, false, true, Locale.Any, new float?());
            }
        }

        public static void SendAnimateSkillStrikeResponse(Character caster, short spellRealId, DamageAction[] actions,
            Unit initialTarget)
        {
            Asda2SpellHandler.SendSetAtackStateGuiResponse(caster);
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AnimateSkillStrike))
            {
                NPC npc = initialTarget as NPC;
                Character character = initialTarget as Character;
                if (character == null && npc == null)
                    caster.SendSystemMessage(string.Format("Wrong spell target {0}. can't animate cast. SpellId {1}",
                        (object) initialTarget, (object) spellRealId));
                packet.WriteInt16(caster.SessionId);
                packet.WriteInt16(spellRealId);
                packet.WriteInt16(6);
                packet.WriteByte(1);
                packet.WriteByte(npc == null ? (byte) 1 : (byte) 0);
                if (character != null && actions != null)
                {
                    for (int index = 0; index < actions.Length; ++index)
                    {
                        DamageAction action = actions[index];
                        if (action != null)
                        {
                            SpellHitStatus spellHitStatus = SpellHitStatus.Ok;
                            if (action.IsCritical)
                                spellHitStatus = SpellHitStatus.Crit;
                            else if (action.Damage == 0)
                                spellHitStatus = SpellHitStatus.Miss;
                            else if (action.Blocked > 0)
                                spellHitStatus = SpellHitStatus.Bloced;
                            if (index < 16)
                            {
                                packet.WriteUInt16(character.SessionId);
                                packet.WriteInt32(action.ActualDamage);
                                packet.WriteInt32((byte) spellHitStatus);
                                packet.WriteInt32(797);
                                packet.WriteSkip(Asda2SpellHandler.unk12);
                            }

                            action.OnFinished();
                        }
                    }
                }
                else if (actions != null)
                {
                    for (int index = 0; index < actions.Length; ++index)
                    {
                        DamageAction action = actions[index];
                        if (action != null)
                        {
                            SpellHitStatus spellHitStatus = SpellHitStatus.Ok;
                            if (action.IsCritical)
                                spellHitStatus = SpellHitStatus.Crit;
                            else if (action.Damage == 0)
                                spellHitStatus = SpellHitStatus.Miss;
                            else if (action.Blocked > 0)
                                spellHitStatus = SpellHitStatus.Bloced;
                            ushort val = 0;
                            if (initialTarget is NPC)
                                val = action.Victim == null || !(action.Victim is NPC)
                                    ? ushort.MaxValue
                                    : action.Victim.UniqIdOnMap;
                            if (index < 16)
                            {
                                packet.WriteUInt16(val);
                                packet.WriteInt32(action.ActualDamage);
                                packet.WriteInt32((byte) spellHitStatus);
                                packet.WriteInt32(797);
                                packet.WriteSkip(Asda2SpellHandler.unk12);
                            }

                            action.OnFinished();
                        }
                    }
                }
                else if (character != null)
                {
                    packet.WriteUInt16(character.SessionId);
                    packet.WriteInt32(0);
                    packet.WriteInt32(3);
                    packet.WriteInt32(0);
                    packet.WriteSkip(Asda2SpellHandler.unk12);
                }

                caster.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        public static void SendSetAtackStateGuiResponse(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SetAtackStateGui))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt32(chr.Account.AccountId);
                chr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendMonstrTakesDamageSecondaryResponse(Character chr, Character targetChr, NPC targetNpc,
            int damage)
        {
            if (targetChr == null && targetNpc == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MonstrTakesDamageSecondary))
            {
                packet.WriteByte(targetNpc != null ? 0 : 1);
                packet.WriteInt16(targetNpc != null ? (short) targetNpc.UniqIdOnMap : targetChr.SessionId);
                packet.WriteInt16(160);
                packet.WriteInt32(damage);
                packet.WriteInt32(450);
                packet.WriteByte(1);
                packet.WriteInt16(66);
                packet.WriteByte(0);
                if (targetChr != null)
                    targetChr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
                else
                    targetNpc.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendCharacterBuffedResponse(Character target, Aura aura)
        {
            if (aura.Spell == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterBuffed))
            {
                packet.WriteInt16(target.SessionId);
                packet.WriteInt16(aura.Spell.RealId);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(1);
                packet.WriteByte(2);
                packet.WriteInt16((short) (aura.TimeLeft / 1000));
                packet.WriteByte(2);
                packet.WriteSkip(Asda2SpellHandler.stub14);
                target.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        [PacketHandler(RealmServerOpCode.LearnSkill)]
        public static void LearnSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            short skillId = packet.ReadInt16();
            byte level = packet.ReadByte();
            SkillLearnStatus status = client.ActiveCharacter.PlayerSpells.TryLearnSpell(skillId, level);
            if (status == SkillLearnStatus.Ok)
                return;
            Asda2SpellHandler.SendSkillLearnedResponse(status, client.ActiveCharacter, 0U, 0);
        }

        public static void SendSkillLearnedResponse(SkillLearnStatus status, Character ownerChar, uint id, int level)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SkillLearned))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(ownerChar.Spells.AvalibleSkillPoints);
                packet.WriteInt32(ownerChar.Money);
                packet.WriteInt16(id);
                packet.WriteByte(level);
                packet.WriteSkip(Asda2SpellHandler.stab16);
                packet.WriteInt16(ownerChar.Asda2Strength);
                packet.WriteInt16(ownerChar.Asda2Agility);
                packet.WriteInt16(ownerChar.Asda2Stamina);
                packet.WriteInt16(ownerChar.Asda2Spirit);
                packet.WriteInt16(ownerChar.Asda2Intellect);
                packet.WriteInt16(ownerChar.Asda2Luck);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(ownerChar.Asda2Strength);
                packet.WriteInt16(ownerChar.Asda2Agility);
                packet.WriteInt16(ownerChar.Asda2Stamina);
                packet.WriteInt16(ownerChar.Asda2Spirit);
                packet.WriteInt16(ownerChar.Asda2Intellect);
                packet.WriteInt16(ownerChar.Asda2Luck);
                packet.WriteInt32(ownerChar.MaxHealth);
                packet.WriteInt16(ownerChar.MaxPower);
                packet.WriteInt32(ownerChar.Health);
                packet.WriteInt16(ownerChar.Power);
                packet.WriteInt16((short) ownerChar.MinDamage);
                packet.WriteInt16((short) ownerChar.MaxDamage);
                packet.WriteInt16(ownerChar.MinMagicDamage);
                packet.WriteInt16(ownerChar.MaxMagicDamage);
                packet.WriteInt16((short) ownerChar.Asda2MagicDefence);
                packet.WriteInt16((short) ownerChar.Asda2Defence);
                packet.WriteInt16((short) ownerChar.Asda2Defence);
                packet.WriteFloat(ownerChar.BlockChance);
                packet.WriteFloat(ownerChar.BlockValue);
                packet.WriteInt16(15);
                packet.WriteInt16(7);
                packet.WriteInt16(4);
                packet.WriteSkip(Asda2SpellHandler.stub87);
                ownerChar.Send(packet, false);
            }
        }

        public static void SendSkillLearnedFirstTimeResponse(IRealmClient client, short skillId, int cooldownSecs)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SkillLearnedFirstTime))
            {
                packet.WriteInt16(skillId);
                packet.WriteByte(1);
                packet.WriteByte(1);
                packet.WriteInt16(cooldownSecs);
                packet.WriteSkip(Asda2SpellHandler.stab12);
                packet.WriteInt16(271);
                packet.WriteInt32(28);
                packet.WriteByte(100);
                packet.WriteByte(100);
                packet.WriteInt16(8);
                packet.WriteSkip(Asda2SpellHandler.stab24);
                client.Send(packet, true);
            }
        }

        [PacketHandler((RealmServerOpCode) 5430)]
        public static void U5330(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler(RealmServerOpCode.ShowMyAukItems | RealmServerOpCode.CMSG_WORLD_TELEPORT)]
        public static void U9915(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler((RealmServerOpCode) 5056)]
        public static void U5056(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler((RealmServerOpCode) 5045)]
        public static void U5045(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler(RealmServerOpCode.MSG_TABARDVENDOR_ACTIVATE | RealmServerOpCode.SMSG_LFG_TELEPORT_DENIED)]
        public static void U1010(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler(RealmServerOpCode.SkillLearnedFirstTime | RealmServerOpCode.CMSG_LEARN_SPELL)]
        public static void U6072(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler(RealmServerOpCode.CharacterSoulMateIntrodactionUpdate |
                       RealmServerOpCode.CMSG_QUERY_OBJECT_POSITION)]
        public static void U6084(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler(RealmServerOpCode.AllSkillsReseted | RealmServerOpCode.CMSG_DBLOOKUP)]
        public static void U6059(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler(RealmServerOpCode.WarEndedOne | RealmServerOpCode.CMSG_LEARN_SPELL)]
        public static void U6749(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler(RealmServerOpCode.EatApple | RealmServerOpCode.CMSG_LEARN_SPELL)]
        public static void U6591(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler(RealmServerOpCode.GlobalChatWithItemResponse | RealmServerOpCode.CMSG_LEARN_SPELL)]
        public static void U6577(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler((RealmServerOpCode) 5474)]
        public static void U5474(IRealmClient client, RealmPacketIn packet)
        {
        }
    }
}