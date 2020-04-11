using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.Talents;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Talents;

namespace WCell.RealmServer.Handlers
{
    public static class PetHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static void HandleNameQuery(IRealmClient client, RealmPacketIn packet)
        {
            int num = (int) packet.ReadUInt32();
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id) as NPC;
            if (npc == null)
                return;
            PetHandler.SendName((IPacketReceiver) activeCharacter, npc.PetNumber, npc.Name, npc.PetNameTimestamp);
        }

        public static void HandleInfoRequest(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void HandleAction(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id) as NPC;
            if (npc == null || npc != activeCharacter.ActivePet || (!npc.IsAlive || npc.PetRecord == null))
                return;
            PetActionEntry petActionEntry = (PetActionEntry) packet.ReadUInt32();
            switch (petActionEntry.Type)
            {
                case PetActionType.SetMode:
                    npc.SetPetAttackMode(petActionEntry.AttackMode);
                    break;
                case PetActionType.SetAction:
                    PetAction action = petActionEntry.Action;
                    npc.SetPetAction(action);
                    break;
                default:
                    WorldObject target = activeCharacter.Map.GetObject(packet.ReadEntityId());
                    npc.CastPetSpell(petActionEntry.SpellId, target);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="packet"></param>
        public static void HandleSetAction(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC pet = activeCharacter.Map.GetObject(id) as NPC;
            if (pet == null || pet.PermanentPetRecord == null ||
                pet != activeCharacter.ActivePet && !activeCharacter.GodMode)
                return;
            while (packet.Length - packet.Position >= 8)
                PetHandler.ReadButton(pet, packet);
        }

        private static void ReadButton(NPC pet, RealmPacketIn packet)
        {
            uint num = packet.ReadUInt32();
            PetActionEntry petActionEntry = (PetActionEntry) packet.ReadUInt32();
            PermanentPetRecord permanentPetRecord = pet.PermanentPetRecord;
            if ((long) num > (long) permanentPetRecord.ActionButtons.Length)
                return;
            permanentPetRecord.ActionButtons[num] = (uint) petActionEntry;
        }

        public static void HandleRename(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id) as NPC;
            if (npc == null || npc != activeCharacter.ActivePet && !activeCharacter.GodMode)
                return;
            string name = packet.ReadCString();
            PetNameInvalidReason reason = npc.TrySetPetName(activeCharacter, name);
            if (reason == PetNameInvalidReason.Ok)
                return;
            PetHandler.SendNameInvalid((IPacketReceiver) activeCharacter, reason, name);
        }

        public static void HandleAbandon(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id) as NPC;
            if (npc == null || !npc.IsAlive || !npc.IsInContext ||
                npc != activeCharacter.ActivePet && !activeCharacter.GodMode)
                return;
            activeCharacter.AbandonActivePet();
        }

        public static void HandleAutocast(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id) as NPC;
            if (npc == null || npc == activeCharacter.ActivePet)
                return;
            int num = activeCharacter.GodMode ? 1 : 0;
        }

        public static void HandlePetCastSpell(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id) as NPC;
            if (npc == null || npc != activeCharacter.ActivePet && activeCharacter.Vehicle != npc &&
                (activeCharacter.Charm != npc && !activeCharacter.GodMode))
                return;
            int num1 = (int) packet.ReadByte();
            uint spellId = packet.ReadUInt32();
            int num2 = (int) packet.ReadByte();
            if (SpellHandler.Get(spellId) == null)
                return;
            SpellCast spellCast = npc.SpellCast;
        }

        public static void HandleCancelAura(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            SpellId index = (SpellId) packet.ReadUInt32();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id) as NPC;
            if (npc == null || npc.Master != activeCharacter)
                return;
            Aura aura = npc.Auras[index, true];
            if (aura == null || !aura.CanBeRemoved)
                return;
            aura.TryRemove(true);
        }

        public static void HandleStopAttack(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id) as NPC;
            if (npc == null || !npc.IsAlive || npc != activeCharacter.ActivePet)
                return;
            npc.Brain.EnterDefaultState();
        }

        public static void SendTameFailure(IPacketReceiver receiver, TameFailReason reason)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_TAME_FAILURE, 1))
            {
                packet.Write((byte) reason);
                receiver.Send(packet, false);
            }
        }

        public static void SendPetGUIDs(Character chr)
        {
            if (chr.ActivePet == null)
            {
                using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_GUIDS, 12))
                {
                    packet.Write(0);
                    chr.Send(packet, false);
                }
            }
            else
            {
                using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_GUIDS, 12))
                {
                    packet.Write(1);
                    packet.Write((ulong) chr.ActivePet.EntityId);
                    chr.Send(packet, false);
                }
            }
        }

        /// <summary>
        /// Sends any kind of extra command-bar to control other entities, such as NPCs, vehicles etc
        /// </summary>
        /// <param name="owner"></param>
        public static void SendSpells(Character owner, NPC pet, PetAction currentAction)
        {
        }

        public static void SendPlayerPossessedPetSpells(Character owner, Character possessed)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_SPELLS, 62))
            {
                packet.Write((ulong) possessed.EntityId);
                packet.Write((ushort) 0);
                packet.Write(0);
                packet.Write((byte) 0);
                packet.Write((byte) 0);
                packet.Write((ushort) 0);
                uint raw1 = new PetActionEntry()
                {
                    Action = PetAction.Attack,
                    Type = PetActionType.SetAction
                }.Raw;
                packet.Write(raw1);
                for (int index = 1; index < 10; ++index)
                {
                    uint raw2 = new PetActionEntry()
                    {
                        Type = PetActionType.SetAction
                    }.Raw;
                    packet.Write(raw2);
                }

                packet.Write((byte) 0);
                packet.Write((byte) 0);
                owner.Send(packet, false);
            }
        }

        public static void SendEmptySpells(IPacketReceiver receiver)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_SPELLS, 8))
            {
                packet.Write(0L);
                receiver.Send(packet, false);
            }
        }

        public static void SendVehicleSpells(IPacketReceiver receiver, NPC vehicle)
        {
            uint[] numArray = vehicle.BuildVehicleActionBar();
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_SPELLS, 18))
            {
                packet.Write((ulong) vehicle.EntityId);
                packet.Write((ushort) 0);
                packet.Write(0);
                packet.Write((byte) 1);
                packet.Write((byte) 1);
                packet.Write((ushort) 0);
                for (int index = 0; index < 10; ++index)
                {
                    uint num = numArray[index];
                    packet.Write(num);
                }

                packet.Write((byte) 0);
                packet.Write((byte) 0);
                receiver.Send(packet, false);
            }
        }

        public static void SendCastFailed(IPacketReceiver receiver, SpellId spellId, SpellFailedReason reason)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_CAST_FAILED, 8))
            {
                packet.Write(0);
                packet.Write((uint) spellId);
                packet.Write((byte) reason);
                receiver.Send(packet, false);
            }
        }

        public static void SendPetLearnedSpell(IPacketReceiver receiver, SpellId spellId)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_LEARNED_SPELL, 4))
            {
                packet.Write((uint) spellId);
                receiver.Send(packet, false);
            }
        }

        public static void SendUnlearnedSpell(IPacketReceiver receiver, ushort spell)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_REMOVED_SPELL, 4))
            {
                packet.Write(spell);
                receiver.Send(packet, false);
            }
        }

        public static void SendName(IPacketReceiver receiver, uint petId, string name, uint timestamp)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_NAME_QUERY_RESPONSE,
                9 + name.Length))
            {
                packet.Write(petId);
                packet.WriteCString(name);
                packet.Write(timestamp);
                packet.Write((byte) 0);
                receiver.Send(packet, false);
            }
        }

        public static void SendNameInvalid(IPacketReceiver receiver, PetNameInvalidReason reason, string name)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_PET_NAME_INVALID))
            {
                packet.Write((uint) reason);
                packet.WriteCString(name);
                packet.WriteByte(0);
                receiver.Send(packet, false);
            }
        }

        public static void SendActionSound(IPacketReceiver receiver, IEntity pet, uint soundId)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_ACTION_SOUND, 12))
            {
                packet.Write((ulong) pet.EntityId);
                packet.Write(soundId);
                receiver.Send(packet, false);
            }
        }

        public static void SendMode(IPacketReceiver receiver, IEntity pet, PetAttackMode attackMode, PetAction action,
            PetFlags flags)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_MODE, 12))
            {
                packet.Write((ulong) pet.EntityId);
                packet.Write((byte) attackMode);
                packet.Write((byte) action);
                packet.Write((ushort) flags);
                receiver.Send(packet, false);
            }
        }

        public static void SendActionFeedback(IPacketReceiver receiver, PetActionFeedback feedback)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_ACTION_FEEDBACK, 1))
            {
                packet.Write((byte) feedback);
                receiver.Send(packet, false);
            }
        }

        public static void SendPetRenameable(IPacketReceiver receiver)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_RENAMEABLE, 0))
                receiver.Send(packet, false);
        }

        public static void SendPetBroken(IPacketReceiver receiver)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PET_BROKEN, 0))
                receiver.Send(packet, false);
        }

        public static void HandleUnstablePet(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            uint petNumber = packet.ReadUInt32();
            Character activeCharacter = client.ActiveCharacter;
            NPC stableMaster = activeCharacter.Map.GetObject(id) as NPC;
            PetMgr.DeStablePet(activeCharacter, stableMaster, petNumber);
        }

        public static void HandleStablePet(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC stableMaster = activeCharacter.Map.GetObject(id) as NPC;
            PetMgr.StablePet(activeCharacter, stableMaster);
        }

        public static void HandleBuyStableSlot(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC stableMaster = activeCharacter.Map.GetObject(id) as NPC;
            PetMgr.BuyStableSlot(activeCharacter, stableMaster);
        }

        public static void HandleStableSwapPet(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            uint petNumber = packet.ReadUInt32();
            Character activeCharacter = client.ActiveCharacter;
            NPC stableMaster = activeCharacter.Map.GetObject(id) as NPC;
            PetMgr.SwapStabledPet(activeCharacter, stableMaster, petNumber);
        }

        public static void HandleListStabledPets(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC stableMaster = activeCharacter.Map.GetObject(id) as NPC;
            PetMgr.ListStabledPets(activeCharacter, stableMaster);
        }

        public static void SendStableResult(IPacketReceiver receiver, StableResult result)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_STABLE_RESULT, 1))
            {
                packet.Write((byte) result);
                receiver.Send(packet, false);
            }
        }

        /// <summary>Send the stabled pets list packet to the client</summary>
        /// <param name="receiver">The client to receive the packet.</param>
        /// <param name="stableMaster">The stable the client is interacting with.</param>
        /// <param name="numStableSlots">The number of stable slots the character owns.</param>
        /// <param name="pets">An array of NPCs containing the ActivePet and the StabledPets</param>
        public static void SendStabledPetsList(IPacketReceiver receiver, Unit stableMaster, byte numStableSlots,
            List<PermanentPetRecord> pets)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_LIST_STABLED_PETS))
            {
                byte num = 1;
                packet.Write((ulong) stableMaster.EntityId);
                packet.Write((byte) pets.Count);
                packet.Write(numStableSlots);
                foreach (PermanentPetRecord pet in pets)
                {
                    packet.Write(pet.PetNumber);
                    packet.Write((uint) pet.EntryId);
                    packet.Write(pet.Level);
                    packet.Write(pet.Name);
                    if (pet.IsActivePet && !pet.Flags.HasFlag((Enum) PetFlags.Stabled))
                        packet.Write((byte) 1);
                    else if (!pet.IsActivePet && pet.Flags.HasFlag((Enum) PetFlags.Stabled))
                    {
                        packet.Write((byte) 2);
                        ++num;
                    }
                    else
                        PetHandler.log.Warn(
                            "{0} tried to send a pet list that included a pet that is marked as both active and stabled.",
                            (object) receiver);
                }

                receiver.Send(packet, false);
            }
        }

        public static void HandlePetLearnTalent(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id1 = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id1) as NPC;
            if (npc == null || !npc.IsAlive || npc != activeCharacter.ActivePet)
                return;
            TalentCollection talents = npc.Talents;
            TalentId id2 = (TalentId) packet.ReadUInt32();
            int rank = packet.ReadInt32();
            talents.Learn(id2, rank);
            TalentHandler.SendTalentGroupList(talents);
        }

        public static void HandlePetUnlearn(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id) as NPC;
            if (npc == null || !npc.HasTalents || npc.Master != activeCharacter)
                return;
            npc.Talents.ResetTalents();
        }

        public static void SavePetTalentChanges(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id1 = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id1) as NPC;
            if (npc == null || !npc.IsAlive || npc != activeCharacter.ActivePet)
                return;
            int num = packet.ReadInt32();
            TalentCollection talents = npc.Talents;
            for (int index = 0; index < num; ++index)
            {
                TalentId id2 = (TalentId) packet.ReadUInt32();
                int rank = packet.ReadInt32();
                talents.Learn(id2, rank);
            }

            TalentHandler.SendTalentGroupList(talents);
        }
    }
}