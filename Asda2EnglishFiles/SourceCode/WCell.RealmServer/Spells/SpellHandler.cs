using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;
using WCell.RealmServer.Spells.Effects.Auras;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.DB;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Spells
{
    /// <summary>
    /// Static helper class for packet sending/receiving and container of all spells
    /// </summary>
    public static class SpellHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>Whether to cast the learn spell when adding spells</summary>
        [NotVariable] public static bool AnimateSpellAdd = true;

        /// <summary>
        /// Minimum length of cooldowns that are to be saved to DB in milliseconds
        /// </summary>
        public static int MinCooldownSaveTimeMillis = 30;

        public static float SpellCritBaseFactor = 1.5f;
        [NotVariable] public static Spell[] ById = new Spell[2262];

        /// <summary>All spells that require tools</summary>
        internal static readonly List<Spell> SpellsRequiringTools = new List<Spell>(2000);

        /// <summary>All spells that represent DynamicObjects.</summary>
        public static readonly Dictionary<SpellId, Spell> DOSpells = new Dictionary<SpellId, Spell>(500);

        /// <summary>All staticly spawned DynamicObjects</summary>
        public static readonly SynchronizedDictionary<EntityId, DynamicObject> StaticDOs =
            new SynchronizedDictionary<EntityId, DynamicObject>();

        public static readonly List<Spell> QuestCompletors = new List<Spell>(100);

        public static readonly Dictionary<uint, Dictionary<uint, Spell>> NPCSpells =
            new Dictionary<uint, Dictionary<uint, Spell>>(1000);

        public static readonly ShapeshiftEntry[] ShapeshiftEntries = new ShapeshiftEntry[43];

        /// <summary>
        /// All effect handler-creation delegates, indexed by their type
        /// </summary>
        public static readonly SpellEffectHandlerCreator[] SpellEffectCreators =
            new SpellEffectHandlerCreator[(int) Convert.ChangeType((object) Utility.GetMaxEnum<SpellEffectType>(),
                                              typeof(int)) + 1];

        public static readonly Dictionary<SummonType, SpellSummonEntry> SummonEntries =
            new Dictionary<SummonType, SpellSummonEntry>();

        public static readonly SpellSummonHandler DefaultSummonHandler = new SpellSummonHandler();
        public static readonly SpellSummonHandler PetSummonHandler = (SpellSummonHandler) new SpellSummonPetHandler();

        public static readonly SpellSummonHandler PossesedSummonHandler =
            (SpellSummonHandler) new SpellSummonPossessedHandler();

        private static bool loaded;

        /// <summary>
        /// Sends initially all spells and item cooldowns to the character
        /// </summary>
        public static void SendSpellsAndCooldowns(Character chr)
        {
            PlayerSpellCollection playerSpells = chr.PlayerSpells;
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_INITIAL_SPELLS,
                5 + 4 * playerSpells.Count))
            {
                packet.Write((byte) 0);
                packet.Write((ushort) playerSpells.Count);
                foreach (Spell allSpell in playerSpells.AllSpells)
                {
                    packet.Write(allSpell.Id);
                    packet.Write((ushort) 0);
                }

                long position = packet.Position;
                ushort num = 0;
                packet.Position = position + 2L;
                long ticks1 = DateTime.Now.Ticks;
                foreach (ISpellIdCooldown idCooldown in playerSpells.IdCooldowns)
                {
                    int ticks2 = (int) (idCooldown.Until.Ticks - ticks1);
                    if (ticks2 > 10)
                    {
                        ++num;
                        packet.Write(idCooldown.SpellId);
                        packet.Write((ushort) idCooldown.ItemId);
                        packet.Write((ushort) 0);
                        packet.Write(Utility.ToMilliSecondsInt(ticks2));
                        packet.Write(0);
                    }
                }

                foreach (ISpellCategoryCooldown categoryCooldown in playerSpells.CategoryCooldowns)
                {
                    int ticks2 = (int) (categoryCooldown.Until.Ticks - ticks1);
                    if (ticks2 > 10)
                    {
                        ++num;
                        packet.Write(categoryCooldown.SpellId);
                        packet.Write((ushort) categoryCooldown.ItemId);
                        packet.Write((ushort) categoryCooldown.CategoryId);
                        packet.Write(0);
                        packet.Write(Utility.ToMilliSecondsInt(ticks2));
                    }
                }

                packet.Position = position;
                packet.Write(num);
                chr.Client.Send(packet, false);
            }
        }

        public static void SendLearnedSpell(IPacketReceiver client, uint spellId)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_LEARNED_SPELL, 4))
            {
                packet.WriteUInt(spellId);
                packet.WriteUShort(0);
                client.Send(packet, false);
            }
        }

        public static void SendSpellSuperceded(IPacketReceiver client, uint spellId, uint newSpellId)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SUPERCEDED_SPELL, 8))
            {
                packet.Write(spellId);
                packet.Write(newSpellId);
                client.Send(packet, false);
            }
        }

        /// <summary>Removes a spell from the client's spellbook</summary>
        public static void SendSpellRemoved(Character chr, uint spellId)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_REMOVED_SPELL, 4))
            {
                packet.WriteUInt(spellId);
                chr.Client.Send(packet, false);
            }
        }

        public static SpellTargetFlags GetTargetFlags(WorldObject obj)
        {
            if (obj is Unit)
                return SpellTargetFlags.Unit;
            if (obj is GameObject)
                return SpellTargetFlags.GameObject;
            return obj is Corpse ? SpellTargetFlags.PvPCorpse : SpellTargetFlags.Self;
        }

        public static void SendUnitCastStart(IRealmClient client, SpellCast cast, WorldObject target)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_UNIT_SPELLCAST_START, 28))
            {
                cast.CasterReference.EntityId.WritePacked((BinaryWriter) packet);
                target.EntityId.WritePacked((BinaryWriter) packet);
                packet.Write(cast.Spell.Id);
                packet.Write(cast.Spell.CastDelay);
                packet.Write(cast.Spell.CastDelay);
                client.Send(packet, false);
            }
        }

        public static void SendCastStart(SpellCast cast)
        {
            if (cast.CasterObject != null && !cast.CasterObject.IsAreaActive)
                return;
            int maxContentLength = 150;
            Spell spell = cast.Spell;
            if (spell == null)
                return;
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELL_START, maxContentLength))
            {
                SpellHandler.WriteCaster(cast, packet);
                packet.Write(spell.Id);
                packet.Write((int) cast.StartFlags);
                packet.Write(cast.CastDelay);
                SpellHandler.WriteTargets(packet, cast);
                if (cast.StartFlags.HasFlag((Enum) CastFlags.RunicPowerGain))
                    packet.Write(0);
                if (cast.StartFlags.HasFlag((Enum) CastFlags.RuneCooldownList))
                {
                    byte num1 = 0;
                    byte num2 = 0;
                    packet.Write(num1);
                    packet.Write(num2);
                    for (int index = 0; index < 6; ++index)
                    {
                        byte num3 = (byte) (1 << index);
                        if (((int) num3 & (int) num1) != 0 && ((int) num3 & (int) num2) == 0)
                            packet.WriteByte(0);
                    }
                }

                if (cast.StartFlags.HasFlag((Enum) CastFlags.Ranged))
                    SpellHandler.WriteAmmoInfo(cast, packet);
                if (cast.StartFlags.HasFlag((Enum) CastFlags.Flag_0x4000000))
                {
                    packet.Write(0);
                    packet.Write(0);
                }

                if (cast.TargetFlags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
                    packet.Write((byte) 0);
                cast.SendPacketToArea(packet);
            }
        }

        private static void WriteAmmoInfo(SpellCast cast, RealmPacketOut packet)
        {
        }

        private static void WriteTargets(RealmPacketOut packet, SpellCast cast)
        {
            SpellTargetFlags flags = cast.TargetFlags;
            if (flags == SpellTargetFlags.Self || flags == SpellTargetFlags.Self)
            {
                Spell spell = cast.Spell;
                if (cast.SelectedTarget is Unit && !spell.IsAreaSpell &&
                    (spell.Visual != 0U || spell.IsPhysicalAbility))
                    flags = SpellTargetFlags.Unit;
            }

            packet.Write((uint) flags);
            if (flags.HasAnyFlag(SpellTargetFlags.WorldObject))
            {
                if (cast.SelectedTarget == null)
                    packet.Write((byte) 0);
                else
                    cast.SelectedTarget.EntityId.WritePacked((BinaryWriter) packet);
            }

            if (flags.HasAnyFlag(SpellTargetFlags.AnyItem) && cast.TargetItem != null)
                cast.TargetItem.EntityId.WritePacked((BinaryWriter) packet);
            if (flags.HasAnyFlag(SpellTargetFlags.SourceLocation))
            {
                if (cast.SelectedTarget != null)
                    cast.SelectedTarget.EntityId.WritePacked((BinaryWriter) packet);
                else
                    packet.Write((byte) 0);
                packet.Write(cast.SourceLoc.X);
                packet.Write(cast.SourceLoc.Y);
                packet.Write(cast.SourceLoc.Z);
            }

            if (flags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
            {
                if (cast.SelectedTarget != null)
                    cast.SelectedTarget.EntityId.WritePacked((BinaryWriter) packet);
                else
                    packet.Write((byte) 0);
                packet.Write(cast.TargetLoc);
            }

            if (!flags.HasAnyFlag(SpellTargetFlags.String))
                return;
            packet.WriteCString(cast.StringTarget);
        }

        /// <summary>Sent to hit targets before CastGo</summary>
        public static void SendCastSuccess(ObjectBase caster, uint spellId, Character target)
        {
            IRealmClient client = target.Client;
        }

        /// <summary>
        /// Sent after spell start. Triggers the casting animation.
        /// </summary>
        public static void SendSpellGo(IEntity caster2, SpellCast cast, ICollection<WorldObject> hitTargets,
            ICollection<MissedTarget> missedTargets, byte previousRuneMask)
        {
            if (cast.CasterObject != null && !cast.CasterObject.IsAreaActive || !cast.IsCasting)
                return;
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELL_GO,
                24 + (hitTargets != null ? hitTargets.Count * 8 : 0) +
                (missedTargets != null ? missedTargets.Count * 10 : 0)))
            {
                cast.CasterReference.EntityId.WritePacked((BinaryWriter) packet);
                caster2.EntityId.WritePacked((BinaryWriter) packet);
                packet.Write(cast.Spell.Id);
                CastFlags goFlags = cast.GoFlags;
                packet.Write((int) goFlags);
                packet.Write(Utility.GetEpochTime());
                packet.WriteByte(hitTargets != null ? hitTargets.Count : 0);
                if (hitTargets != null && cast.CasterObject != null)
                {
                    foreach (WorldObject hitTarget in (IEnumerable<WorldObject>) hitTargets)
                    {
                        packet.Write((ulong) hitTarget.EntityId);
                        if (hitTarget is Character)
                            SpellHandler.SendCastSuccess((ObjectBase) cast.CasterObject, cast.Spell.Id,
                                hitTarget as Character);
                    }
                }

                packet.WriteByte(missedTargets != null ? missedTargets.Count : 0);
                if (missedTargets != null)
                {
                    foreach (MissedTarget missedTarget in (IEnumerable<MissedTarget>) missedTargets)
                    {
                        packet.Write((ulong) missedTarget.Target.EntityId);
                        packet.Write((byte) missedTarget.Reason);
                        if (missedTarget.Reason == CastMissReason.Reflect)
                            packet.Write((byte) 0);
                    }
                }

                SpellHandler.WriteTargets(packet, cast);
                if (goFlags.HasFlag((Enum) CastFlags.RunicPowerGain))
                    packet.Write(0);
                if (goFlags.HasFlag((Enum) CastFlags.RuneCooldownList))
                {
                    byte activeRuneMask = cast.CasterChar.PlayerSpells.Runes.GetActiveRuneMask();
                    packet.Write(previousRuneMask);
                    packet.Write(activeRuneMask);
                    for (int index = 0; index < 6; ++index)
                    {
                        byte num = (byte) (1 << index);
                        if (((int) num & (int) previousRuneMask) != 0 && ((int) num & (int) activeRuneMask) == 0)
                            packet.WriteByte(0);
                    }
                }

                if (goFlags.HasFlag((Enum) CastFlags.Flag_0x20000))
                {
                    packet.WriteFloat(0);
                    packet.Write(0);
                }

                if (cast.StartFlags.HasFlag((Enum) CastFlags.Ranged))
                    SpellHandler.WriteAmmoInfo(cast, packet);
                if (goFlags.HasFlag((Enum) CastFlags.Flag_0x80000))
                {
                    packet.Write(0);
                    packet.Write(0);
                }

                if (cast.TargetFlags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
                    packet.Write((byte) 0);
                cast.SendPacketToArea(packet);
            }
        }

        private static void WriteCaster(SpellCast cast, RealmPacketOut packet)
        {
            if (cast.TargetItem != null)
                cast.CasterItem.EntityId.WritePacked((BinaryWriter) packet);
            else
                cast.CasterReference.EntityId.WritePacked((BinaryWriter) packet);
            cast.CasterReference.EntityId.WritePacked((BinaryWriter) packet);
        }

        /// <summary>This is sent to caster if spell fails</summary>
        internal static void SendCastFailed(IPacketReceiver client, Spell spell, SpellFailedReason result)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_CAST_FAILED,
                result == SpellFailedReason.RequiresSpellFocus || result == SpellFailedReason.RequiresArea ? 10 : 6))
            {
                packet.Write(spell.Id);
                packet.Write((byte) result);
                switch (result)
                {
                    case SpellFailedReason.RequiresArea:
                        packet.Write(spell.AreaGroupId);
                        break;
                    case SpellFailedReason.RequiresSpellFocus:
                        packet.Write((uint) spell.RequiredSpellFocus);
                        break;
                }

                client.Send(packet, false);
            }
        }

        /// <summary>Spell went wrong or got cancelled</summary>
        internal static void SendCastFailPackets(SpellCast spellCast, SpellFailedReason reason)
        {
            if (spellCast.Client == null)
                return;
            SpellHandler.SendCastFailed((IPacketReceiver) spellCast.Client, spellCast.Spell, reason);
            SpellHandler.SendSpellFailure(spellCast, reason);
            SpellHandler.SendSpellFailedOther(spellCast, reason);
        }

        internal static void SendSpellFailure(SpellCast spellCast, SpellFailedReason reason)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELL_FAILURE, 15))
            {
                spellCast.CasterReference.EntityId.WritePacked((BinaryWriter) packet);
                packet.Write(spellCast.Spell.Id);
                packet.Write((byte) reason);
                spellCast.SendPacketToArea(packet);
            }
        }

        internal static void SendSpellFailedOther(SpellCast spellCast, SpellFailedReason reason)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELL_FAILED_OTHER, 15))
            {
                spellCast.CasterReference.EntityId.WritePacked((BinaryWriter) packet);
                packet.Write(spellCast.Spell.Id);
                packet.Write((byte) reason);
                spellCast.SendPacketToArea(packet);
            }
        }

        /// <summary>Delays the spell-cast</summary>
        public static void SendCastDelayed(SpellCast cast, int delay)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELL_DELAYED, 12))
            {
                cast.CasterReference.EntityId.WritePacked((BinaryWriter) packet);
                packet.Write(delay);
                cast.SendPacketToArea(packet);
            }
        }

        /// <summary>Starts Channeling</summary>
        public static void SendChannelStart(SpellCast cast, SpellId spellId, int duration)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.MSG_CHANNEL_START, 12))
            {
                cast.CasterReference.EntityId.WritePacked((BinaryWriter) packet);
                packet.Write((uint) spellId);
                packet.Write(duration);
                cast.SendPacketToArea(packet);
            }
        }

        /// <summary>Changes the time of the channel</summary>
        public static void SendChannelUpdate(SpellCast cast, uint delay)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.MSG_CHANNEL_UPDATE, 12))
            {
                cast.CasterReference.EntityId.WritePacked((BinaryWriter) packet);
                packet.Write(delay);
                cast.SendPacketToArea(packet);
            }
        }

        /// <summary>Shows a spell visual</summary>
        public static void SendVisual(WorldObject target, SpellId id)
        {
            Spell spell = SpellHandler.Get(id);
            SpellHandler.SendVisual(target, spell.Visual);
        }

        /// <summary>Shows a spell visual</summary>
        public static void SendVisual(WorldObject target, uint visualId)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PLAY_SPELL_VISUAL, 12))
            {
                packet.Write((ulong) target.EntityId);
                packet.Write(visualId);
                target.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        public static void SendImpact(WorldObject target, SpellId id)
        {
            Spell spell = SpellHandler.Get(id);
            SpellHandler.SendImpact(target, spell.Visual);
        }

        /// <summary>Shows a spell Impact animation</summary>
        public static void SendImpact(WorldObject target, uint visualId)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PLAY_SPELL_IMPACT, 12))
            {
                packet.Write((ulong) target.EntityId);
                packet.Write(visualId);
                target.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        /// <summary>Send a custom cooldown time to the client</summary>
        public static void SendSpellCooldown(WorldObject caster, IRealmClient client, uint spellId, ushort cooldown)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SPELL_COOLDOWN, 14))
            {
                packet.Write(caster.EntityId.Full);
                packet.WriteByte(0);
                packet.Write(spellId);
                packet.Write((uint) cooldown);
                client.Send(packet, false);
            }
        }

        /// <summary>Send a custom cooldown time to the client</summary>
        public static void SendItemCooldown(IRealmClient client, uint spellId, IEntity item)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ITEM_COOLDOWN, 14))
            {
                packet.Write(item.EntityId.Full);
                packet.Write(spellId);
                client.Send(packet, false);
            }
        }

        /// <summary>
        /// Forces the client to start or update a cooldown timer on the given single spell
        /// (mostly important for certain talents and item spells that don't automatically start cooling down)
        /// </summary>
        public static void SendCooldownUpdate(Character chr, SpellId spellId)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_COOLDOWN_EVENT, 12))
            {
                packet.WriteUInt((uint) spellId);
                chr.EntityId.WritePacked((BinaryWriter) packet);
                chr.Send(packet, false);
            }
        }

        /// <summary>Sends spell modifier update</summary>
        public static void SendSpellModifier(Character chr, byte groupBitNumber, SpellModifierType type, int amount,
            bool isPercent)
        {
            using (RealmPacketOut packet = new RealmPacketOut(
                (PacketId) (isPercent
                    ? RealmServerOpCode.SMSG_SET_PCT_SPELL_MODIFIER
                    : RealmServerOpCode.SMSG_SET_FLAT_SPELL_MODIFIER), 6))
            {
                packet.Write(groupBitNumber);
                packet.Write((byte) type);
                packet.Write(amount);
                chr.Send(packet, false);
            }
        }

        public static void SendSetSpellMissilePosition(IPacketReceiver client, EntityId casterId, byte castCount,
            Vector3 position)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SET_PROJECTILE_POSITION, 21))
            {
                casterId.WritePacked((BinaryWriter) packet);
                packet.WriteByte(castCount);
                packet.WriteFloat(position.X);
                packet.WriteFloat(position.Y);
                packet.WriteFloat(position.Z);
                client.Send(packet, false);
            }
        }

        public static void HandleCastSpell(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = (int) packet.ReadByte();
            uint index = packet.ReadUInt32();
            int num2 = (int) packet.ReadByte();
            if (client.ActiveCharacter.Spells[index] == null)
                return;
            SpellCast spellCast = client.ActiveCharacter.SpellCast;
        }

        public static void HandleCancelCastSpell(IRealmClient client, RealmPacketIn packet)
        {
            int num = (int) packet.ReadUInt32();
            if (!client.ActiveCharacter.IsUsingSpell)
                return;
            client.ActiveCharacter.SpellCast.Cancel(SpellFailedReason.Interrupted);
        }

        /// <summary>Somehow seems to be the same as CMSG_CANCEL_CAST</summary>
        public static void HandleCancelChanneling(IRealmClient client, RealmPacketIn packet)
        {
            int num = (int) packet.ReadUInt32();
            Character activeCharacter = client.ActiveCharacter;
            if (activeCharacter.MoveControl.Mover != activeCharacter)
                return;
            SpellCast spellCast = activeCharacter.SpellCast;
            if (spellCast == null)
                return;
            spellCast.Cancel(SpellFailedReason.Interrupted);
        }

        public static void HandleCancelAutoRepeat(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.AutorepeatSpell = (Spell) null;
        }

        /// <summary>Probably can only be sent by God client</summary>
        public static void HandleUnlearnSpell(IRealmClient client, RealmPacketIn packet)
        {
            int num = (int) packet.ReadUInt32();
        }

        public static void HandleSpellClick(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            NPC npc = activeCharacter.Map.GetObject(id) as NPC;
            if (npc == null)
                return;
            if (npc.Entry.IsVehicle)
            {
                VehicleSeat seatFor = (npc as Vehicle).GetSeatFor((Unit) activeCharacter);
                if (seatFor == null)
                    return;
                seatFor.Enter((Unit) activeCharacter);
            }
            else
            {
                SpellTriggerInfo spellTriggerInfo;
                if ((spellTriggerInfo = npc.Entry.SpellTriggerInfo) == null)
                    return;
                int num = (int) activeCharacter.SpellCast.Start(spellTriggerInfo.Spell, false);
            }
        }

        public static void HandleUpdateMissilePosition(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadPackedEntityId();
            packet.ReadInt32();
            int num = (int) packet.ReadByte();
            packet.ReadVector3();
        }

        public static void SendConvertRune(IRealmClient client, uint index, RuneType type)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_CONVERT_RUNE, 2))
            {
                packet.Write((byte) index);
                packet.Write((byte) type);
                client.Send(packet, false);
            }
        }

        public static uint HighestId { get; internal set; }

        /// <summary>
        /// Returns the spell with the given spellId or null if it doesn't exist
        /// </summary>
        public static Spell Get(uint spellId)
        {
            if ((long) spellId >= (long) SpellHandler.ById.Length)
                return (Spell) null;
            return SpellHandler.ById[spellId];
        }

        /// <summary>
        /// Returns the spell with the given spellId or null if it doesn't exist
        /// </summary>
        public static Spell Get(SpellId spellId)
        {
            if ((long) spellId >= (long) SpellHandler.ById.Length)
                return (Spell) null;
            return SpellHandler.ById[(uint) spellId];
        }

        internal static void AddSpell(Spell spell)
        {
            ArrayUtil.Set<Spell>(ref SpellHandler.ById, spell.Id, spell);
            SpellHandler.HighestId = Math.Max(spell.Id, SpellHandler.HighestId);
        }

        /// <summary>Can be used to add a Spell that does not exist.</summary>
        public static Spell AddCustomSpell(string name)
        {
            return SpellHandler.AddCustomSpell(SpellHandler.HighestId + 1U, name);
        }

        /// <summary>
        /// Can be used to add a Spell that does not exist.
        /// Usually used for spells that are unknown to the client to signal a certain state.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Spell AddCustomSpell(uint id, string name)
        {
            if (SpellHandler.Get(id) != null)
                throw new ArgumentException("Invalid custom spell id is already in use: " + (object) id + " - " + name);
            Spell spell = new Spell()
            {
                Id = id,
                SpellId = (SpellId) id,
                Name = "[" + RealmLocalizer.Instance.Translate(RealmLangKey.Custom, new object[0]).ToUpper() + "] " +
                       name,
                Effects = new SpellEffect[0],
                RequiredToolIds = new uint[0]
            };
            SpellHandler.AddSpell(spell);
            return spell;
        }

        public static void RemoveSpell(uint id)
        {
            SpellHandler.ById[id] = (Spell) null;
        }

        public static void RemoveSpell(SpellId id)
        {
            SpellHandler.ById[(int) id] = (Spell) null;
        }

        /// <summary>
        /// Apply the given action on all Spells with the given ids
        /// </summary>
        /// <param name="action"></param>
        public static void Apply(this SpellLineId id, Action<Spell> action)
        {
            SpellLine line = id.GetLine();
            if (line == null)
                throw new Exception("Invalid SpellLineId: " + (object) id);
            action.Apply((IEnumerable<Spell>) line);
        }

        /// <summary>
        /// Apply the given action on all Spells with the given ids
        /// </summary>
        /// <param name="action"></param>
        public static void Apply(Action<Spell> action, SpellLineId id, params SpellId[] ids)
        {
            SpellLine line = id.GetLine();
            if (line == null)
                throw new Exception("Invalid SpellLineId: " + (object) id);
            action.Apply((IEnumerable<Spell>) line);
            action.Apply((IEnumerable<SpellId>) ids);
        }

        /// <summary>
        /// Apply the given action on all Spells with the given ids
        /// </summary>
        /// <param name="action"></param>
        public static void Apply(Action<Spell> action, SpellLineId id, SpellLineId id2, params SpellId[] ids)
        {
            SpellLine line1 = id.GetLine();
            if (line1 == null)
                throw new Exception("Invalid SpellLineId: " + (object) id);
            SpellLine line2 = id2.GetLine();
            if (line2 == null)
                throw new Exception("Invalid SpellLineId: " + (object) id2);
            action.Apply((IEnumerable<Spell>) line1);
            action.Apply((IEnumerable<Spell>) line2);
            action.Apply((IEnumerable<SpellId>) ids);
        }

        /// <summary>
        /// Apply the given action on all Spells with the given ids
        /// </summary>
        /// <param name="action"></param>
        public static void Apply(this Action<Spell> action, params SpellId[] ids)
        {
            action.Apply((IEnumerable<SpellId>) ids);
        }

        /// <summary>
        /// Apply the given action on all Spells with the given ids
        /// </summary>
        /// <param name="action"></param>
        public static void Apply(this Action<Spell> action, params SpellLineId[] ids)
        {
            foreach (SpellLineId id in ids)
            {
                SpellLine line = id.GetLine();
                action.Apply((IEnumerable<Spell>) line);
            }
        }

        /// <summary>
        /// Apply the given action on all Spells with the given ids
        /// </summary>
        /// <param name="action"></param>
        public static void Apply(this Action<Spell> action, IEnumerable<SpellId> ids)
        {
            foreach (SpellId id in ids)
            {
                Spell spell = SpellHandler.Get(id);
                if (spell == null)
                    throw new Exception("Invalid SpellId: " + (object) id);
                action(spell);
            }
        }

        /// <summary>
        /// Apply the given action on all Spells with the given ids
        /// </summary>
        /// <param name="action"></param>
        public static void Apply(this Action<Spell> action, IEnumerable<Spell> spells)
        {
            foreach (Spell spell in spells)
                action(spell);
        }

        /// <summary>
        /// Returns a list of all SpellLines that are affected by the given spell family set (very long bit field)
        /// </summary>
        public static HashSet<SpellLine> GetAffectedSpellLines(ClassId clss, uint[] mask)
        {
            SpellLine[] lines = SpellLines.GetLines(clss);
            HashSet<SpellLine> spellLineSet = new HashSet<SpellLine>();
            if (lines != null)
            {
                foreach (SpellLine spellLine in lines)
                {
                    foreach (Spell spell in spellLine)
                    {
                        if (spell.MatchesMask(mask))
                        {
                            spellLineSet.Add(spellLine);
                            break;
                        }
                    }
                }
            }

            return spellLineSet;
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.First, "Initialize Spells")]
        public static void LoadSpells()
        {
            SpellHandler.LoadSpells(false);
        }

        public static void LoadSpells(bool init)
        {
            if (!SpellHandler.loaded)
            {
                SpellHandler.InitEffectHandlers();
                SpellEffect.InitMiscValueTypes();
                SpellHandler.loaded = true;
                SpellHandler.LoadOverrides();
                SpellLines.InitSpellLines();
                ProcEventHelper.PatchSpells(SpellHandler.ById);
            }

            if (!init)
                return;
            SpellHandler.Initialize2();
        }

        /// <summary>Second initialization pass</summary>
        [WCell.Core.Initialization.Initialization(InitializationPass.Third, "Initialize Spells (2)")]
        public static void Initialize2()
        {
            List<Spell> spellList = new List<Spell>(5900);
            foreach (Spell spell in SpellHandler.ById)
            {
                if (spell != null)
                {
                    spell.Initialize();
                    if (spell.IsTeachSpell)
                        spellList.Add(spell);
                    if (spell.DOEffect != null)
                        SpellHandler.DOSpells[spell.SpellId] = spell;
                }
            }

            AuraHandler.RegisterAuraUIDEvaluators();
            foreach (Spell spell in SpellHandler.ById)
            {
                if (spell != null)
                    spell.Init2();
            }

            SkillHandler.Initialize2();
        }

        /// <summary>Load given Spell-data from DB</summary>
        private static void LoadOverrides()
        {
            LightDBMapper mapper = ContentMgr.GetMapper<Spell>();
            mapper.AddObjectsUInt<Spell>(SpellHandler.ById);
            ContentMgr.Load(mapper);
        }

        internal static void InitTools()
        {
            foreach (Spell spellsRequiringTool in SpellHandler.SpellsRequiringTools)
            {
                foreach (uint requiredToolId in spellsRequiringTool.RequiredToolIds)
                {
                    if (requiredToolId > 0U)
                    {
                        ItemTemplate val = ItemMgr.Templates.Get<ItemTemplate>(requiredToolId);
                        if (val != null)
                        {
                            if (spellsRequiringTool.RequiredTools == null)
                                spellsRequiringTool.RequiredTools =
                                    new ItemTemplate[spellsRequiringTool.RequiredToolIds.Length];
                            int num = (int) ArrayUtil.Add<ItemTemplate>(ref spellsRequiringTool.RequiredTools, val);
                        }
                    }
                }

                if (spellsRequiringTool.RequiredTools != null)
                    ArrayUtil.Prune<ItemTemplate>(ref spellsRequiringTool.RequiredTools);
            }
        }

        private static void InitEffectHandlers()
        {
            SpellHandler.SpellEffectCreators[1] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new InstantKillEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[2] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SchoolDamageEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[5] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new TeleportUnitsEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[6] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ApplyAuraEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[7] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new EnvironmentalDamageEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[8] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new PowerDrainEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[9] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new HealthLeechEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[10] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new HealEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[11] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new BindEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[12] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new PortalHandler(cast, effect));
            SpellHandler.SpellEffectCreators[16] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new QuestCompleteEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[17] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new WeaponDamageNoSchoolEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[18] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ResurrectEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[19] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new AddExtraAttacksEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[24] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new CreateItemEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[25] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new WeaponEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[27] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new PersistantAreaAuraEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[28] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SummonEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[30] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new EnergizeEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[31] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new WeaponDamageEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[33] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new OpenLockEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[35] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ApplyAreaAuraEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[65] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ApplyAreaAura2EffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[36] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new LearnSpellEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[38] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new DispelEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[39] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new LanguageEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[40] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new DualWeildEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[44] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SkillStepEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[48] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new StealthEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[50] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SummonObjectEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[76] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SummonObjectWildEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[53] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new EnchantItemEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[54] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new EnchantItemTemporaryEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[55] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new TameCreatureEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[56] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SummonPetEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[58] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new WeaponDamageEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[61] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SendEventEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[63] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ThreatHandler(cast, effect));
            SpellHandler.SpellEffectCreators[64] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new TriggerSpellEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[66] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new CreateManaGemEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[67] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new HealMaxHealthEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[68] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new InterruptCastEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[69] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new DistractEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[74] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ApplyGlyphEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[77] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ScriptEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[79] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new RemoveImpairingEffectsHandler(cast, effect));
            SpellHandler.SpellEffectCreators[80] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new AddComboPointsEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[83] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new DuelEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[85] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SummonPlayerEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[87] =
                (SpellEffectHandlerCreator) ((cast, effect) => (SpellEffectHandler) new WMODamage(cast, effect));
            SpellHandler.SpellEffectCreators[88] =
                (SpellEffectHandlerCreator) ((cast, effect) => (SpellEffectHandler) new WMORepair(cast, effect));
            SpellHandler.SpellEffectCreators[89] =
                (SpellEffectHandlerCreator) ((cast, effect) => (SpellEffectHandler) new WMOChange(cast, effect));
            SpellHandler.SpellEffectCreators[90] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new KillCreditPersonal(cast, effect));
            SpellHandler.SpellEffectCreators[94] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SelfResurrectEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[95] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SkinningEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[96] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ChargeEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[97] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SummonAllTotemsHandler(cast, effect));
            SpellHandler.SpellEffectCreators[98] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new KnockBackEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[99] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new DisenchantEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[100] =
                (SpellEffectHandlerCreator) ((cast, effect) => (SpellEffectHandler) new Inebriate(cast, effect));
            SpellHandler.SpellEffectCreators[102] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new DismissPetEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[108] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new DispelMechanicEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[109] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SummonDeadPetEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[113] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ResurrectFlatEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[118] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SkillEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[119] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ApplyPetAuraEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[121] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new NormalizedWeaponDamagePlusEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[123] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new VideoEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[126] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new StealBeneficialBuffEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[(int) sbyte.MaxValue] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ProspectingEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[128] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ApplyStatAuraEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[129] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ApplyStatAuraPercentEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[132] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new PlayMusicEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[133] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ForgetSpecializationEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[136] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new RestoreHealthPercentEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[137] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new RestoreManaPercentEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[139] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ClearQuestEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[140] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new TriggerSpellFromTargetWithCasterAsTargetHandler(cast, effect));
            SpellHandler.SpellEffectCreators[143] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ApplyAuraToMasterEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[151] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new TriggerRitualOfSummoningEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[154] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new TeachFlightPathEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[101] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new FeedPetEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[104] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SummonObjectSlot1Handler(cast, effect));
            SpellHandler.SpellEffectCreators[105] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SummonObjectSlot2Handler(cast, effect));
            SpellHandler.SpellEffectCreators[106] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SummonObjectSlot1Handler(cast, effect));
            SpellHandler.SpellEffectCreators[107] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SummonObjectSlot2Handler(cast, effect));
            SpellHandler.SpellEffectCreators[110] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new DestroyAllTotemsHandler(cast, effect));
            SpellHandler.SpellEffectCreators[161] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new SetNumberOfTalentGroupsHandler(cast, effect));
            SpellHandler.SpellEffectCreators[162] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new ActivateTalentGroupHandler(cast, effect));
            SpellHandler.SpellEffectCreators[200] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new DamageFromPrcAtackHandler(cast, effect));
            SpellHandler.SpellEffectCreators[201] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new CastAnotherSpellHandler(cast, effect));
            SpellHandler.SpellEffectCreators[202] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new PortalTeleportEffectHandler(cast, effect));
            SpellHandler.SpellEffectCreators[203] = (SpellEffectHandlerCreator) ((cast, effect) =>
                (SpellEffectHandler) new BossSummonHelpSpellHandler(cast, effect));
            for (int index = 0; index < SpellHandler.SpellEffectCreators.Length; ++index)
            {
                if (SpellHandler.SpellEffectCreators[index] == null)
                    SpellHandler.SpellEffectCreators[index] = (SpellEffectHandlerCreator) ((cast, effect) =>
                        (SpellEffectHandler) new NotImplementedEffectHandler(cast, effect));
            }

            SpellHandler.UnsetHandler(SpellEffectType.None);
            SpellHandler.UnsetHandler(SpellEffectType.Dodge);
            SpellHandler.UnsetHandler(SpellEffectType.Defense);
            SpellHandler.UnsetHandler(SpellEffectType.SpellDefense);
            SpellHandler.UnsetHandler(SpellEffectType.Block);
            SpellHandler.UnsetHandler(SpellEffectType.Detect);
            SpellHandler.UnsetHandler(SpellEffectType.Dummy);
            SpellHandler.UnsetHandler(SpellEffectType.Parry);
        }

        public static void UnsetHandler(SpellEffectType type)
        {
            SpellHandler.SpellEffectCreators[(int) type] = (SpellEffectHandlerCreator) null;
        }

        private static void InitSummonHandlers()
        {
            foreach (SpellSummonEntry spellSummonEntry in SpellHandler.SummonEntries.Values)
            {
                if (spellSummonEntry.Id == SummonType.Totem)
                    spellSummonEntry.Type = SummonPropertyType.Totem;
                if (spellSummonEntry.Type == SummonPropertyType.Totem)
                {
                    spellSummonEntry.Handler =
                        (SpellSummonHandler) new SpellSummonTotemHandler(
                            MathUtil.ClampMinMax(spellSummonEntry.Slot - 1U, 0U, 3U));
                    spellSummonEntry.DetermineAmountBySpellEffect = false;
                }
                else
                {
                    switch (spellSummonEntry.Group)
                    {
                        case SummonGroup.Wild:
                            spellSummonEntry.Handler = SpellHandler.DefaultSummonHandler;
                            continue;
                        case SummonGroup.Friendly:
                            spellSummonEntry.Handler = SpellHandler.DefaultSummonHandler;
                            continue;
                        case SummonGroup.Pets:
                            spellSummonEntry.Handler = SpellHandler.PetSummonHandler;
                            continue;
                        case SummonGroup.Controllable:
                            spellSummonEntry.Handler = SpellHandler.PossesedSummonHandler;
                            continue;
                        default:
                            spellSummonEntry.Handler = SpellHandler.DefaultSummonHandler;
                            continue;
                    }
                }
            }

            SpellHandler.SummonEntries[SummonType.Critter].Handler = SpellHandler.DefaultSummonHandler;
            SpellHandler.SummonEntries[SummonType.Critter2].Handler = SpellHandler.DefaultSummonHandler;
            SpellHandler.SummonEntries[SummonType.Critter3].Handler = SpellHandler.DefaultSummonHandler;
            SpellHandler.SummonEntries[SummonType.Demon].Handler = SpellHandler.DefaultSummonHandler;
            SpellHandler.SummonEntries[SummonType.DoomGuard].Handler =
                (SpellSummonHandler) new SpellSummonDoomguardHandler();
        }

        public static SpellSummonEntry GetSummonEntry(SummonType type)
        {
            SpellSummonEntry spellSummonEntry;
            if (SpellHandler.SummonEntries.TryGetValue(type, out spellSummonEntry))
                return spellSummonEntry;
            SpellHandler.log.Warn("Missing SpellSummonEntry for type: " + (object) type);
            return SpellHandler.SummonEntries[SummonType.SummonPet];
        }

        public static ShapeshiftEntry GetShapeshiftEntry(ShapeshiftForm form)
        {
            return SpellHandler.ShapeshiftEntries[(int) form];
        }

        public static ClassId ToClassId(this SpellClassSet classSet)
        {
            switch (classSet)
            {
                case SpellClassSet.Mage:
                    return ClassId.SupportMage;
                case SpellClassSet.Warrior:
                    return ClassId.OHS;
                case SpellClassSet.Warlock:
                    return ClassId.HealMage;
                case SpellClassSet.Priest:
                    return ClassId.Bow;
                case SpellClassSet.Druid:
                    return ClassId.Druid;
                case SpellClassSet.Rogue:
                    return ClassId.Crossbow;
                case SpellClassSet.Hunter:
                    return ClassId.THS;
                case SpellClassSet.Paladin:
                    return ClassId.Spear;
                case SpellClassSet.Shaman:
                    return ClassId.AtackMage;
                case SpellClassSet.DeathKnight:
                    return ClassId.Balista;
                default:
                    return ClassId.NoClass;
            }
        }
    }
}