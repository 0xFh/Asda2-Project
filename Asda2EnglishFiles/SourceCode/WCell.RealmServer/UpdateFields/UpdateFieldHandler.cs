using System;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Spells;
using WCell.Util.Variables;

namespace WCell.RealmServer.UpdateFields
{
    /// <summary>
    /// Similar to an UpdateMask, it filters out the bits only needed for the player
    /// </summary>
    public static class UpdateFieldHandler
    {
        [NotVariable] public static UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicObjectFieldHandlers;
        [NotVariable] public static UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicItemFieldHandlers;
        [NotVariable] public static UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicContainerFieldHandlers;
        [NotVariable] public static UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicDOFieldHandlers;
        [NotVariable] public static UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicGOHandlers;
        [NotVariable] public static UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicCorpseHandlers;
        [NotVariable] public static UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUnitHandlers;
        [NotVariable] public static UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicPlayerHandlers;

        [WCell.Core.Initialization.Initialization(InitializationPass.First)]
        public static void Init()
        {
            UpdateFieldMgr.Init();
            UpdateFieldHandler.DynamicObjectFieldHandlers =
                new UpdateFieldHandler.DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Object).TotalLength];
            UpdateFieldHandler.DynamicItemFieldHandlers =
                new UpdateFieldHandler.DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Item).TotalLength];
            UpdateFieldHandler.DynamicContainerFieldHandlers =
                new UpdateFieldHandler.DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Container)
                    .TotalLength];
            UpdateFieldHandler.DynamicDOFieldHandlers =
                new UpdateFieldHandler.DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.DynamicObject)
                    .TotalLength];
            UpdateFieldHandler.DynamicGOHandlers =
                new UpdateFieldHandler.DynamicUpdateFieldHandler
                    [UpdateFieldMgr.Get(ObjectTypeId.GameObject).TotalLength];
            UpdateFieldHandler.DynamicCorpseHandlers =
                new UpdateFieldHandler.DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Corpse).TotalLength];
            UpdateFieldHandler.DynamicUnitHandlers =
                new UpdateFieldHandler.DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Unit).TotalLength];
            UpdateFieldHandler.DynamicPlayerHandlers =
                new UpdateFieldHandler.DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Player).TotalLength];
            UpdateFieldHandler.InitHandlers();
            UpdateFieldHandler.Inherit(UpdateFieldHandler.DynamicItemFieldHandlers,
                UpdateFieldHandler.DynamicObjectFieldHandlers);
            UpdateFieldHandler.Inherit(UpdateFieldHandler.DynamicContainerFieldHandlers,
                UpdateFieldHandler.DynamicItemFieldHandlers);
            UpdateFieldHandler.Inherit(UpdateFieldHandler.DynamicDOFieldHandlers,
                UpdateFieldHandler.DynamicObjectFieldHandlers);
            UpdateFieldHandler.Inherit(UpdateFieldHandler.DynamicGOHandlers,
                UpdateFieldHandler.DynamicObjectFieldHandlers);
            UpdateFieldHandler.Inherit(UpdateFieldHandler.DynamicCorpseHandlers,
                UpdateFieldHandler.DynamicObjectFieldHandlers);
            UpdateFieldHandler.Inherit(UpdateFieldHandler.DynamicUnitHandlers,
                UpdateFieldHandler.DynamicObjectFieldHandlers);
            UpdateFieldHandler.Inherit(UpdateFieldHandler.DynamicPlayerHandlers,
                UpdateFieldHandler.DynamicUnitHandlers);
        }

        private static void Inherit(UpdateFieldHandler.DynamicUpdateFieldHandler[] handlers,
            UpdateFieldHandler.DynamicUpdateFieldHandler[] baseHandlers)
        {
            Array.Copy((Array) baseHandlers, 0, (Array) handlers, 0, baseHandlers.Length);
        }

        private static void InitHandlers()
        {
            UpdateFieldHandler.DynamicGOHandlers[14] =
                new UpdateFieldHandler.DynamicUpdateFieldHandler(UpdateFieldHandler.WriteGODynamic);
            UpdateFieldHandler.DynamicCorpseHandlers[34] =
                new UpdateFieldHandler.DynamicUpdateFieldHandler(UpdateFieldHandler.WriteCorpseDynFlags);
            UpdateFieldHandler.DynamicUnitHandlers[82] =
                new UpdateFieldHandler.DynamicUpdateFieldHandler(UpdateFieldHandler.WriteNPCFlags);
            UpdateFieldHandler.DynamicUnitHandlers[79] =
                new UpdateFieldHandler.DynamicUpdateFieldHandler(UpdateFieldHandler.WriteUnitDynFlags);
            UpdateFieldHandler.DynamicUnitHandlers[61] =
                new UpdateFieldHandler.DynamicUpdateFieldHandler(UpdateFieldHandler.WriteAuraStateFlags);
        }

        private static void WriteAuraStateFlags(ObjectBase obj, Character reciever, UpdatePacket packet)
        {
            Unit unit = (Unit) obj;
            Spell strongestImmolate = unit.GetStrongestImmolate();
            if (strongestImmolate != null && unit.Auras[strongestImmolate].SpellCast.CasterChar == reciever)
                packet.Write((uint) (unit.AuraState | AuraStateMask.Immolate));
            else
                packet.Write((uint) (unit.AuraState & ~AuraStateMask.Immolate));
        }

        private static void WriteNPCFlags(ObjectBase obj, Character chr, UpdatePacket packet)
        {
            NPCFlags npcFlags = (NPCFlags) obj.GetUInt32(UnitFields.NPC_FLAGS);
            if (obj is NPC)
            {
                NPC npc = (NPC) obj;
                if (npc.IsTrainer && !npc.TrainerEntry.CanTrain(chr))
                    npcFlags = NPCFlags.None;
            }

            packet.Write((uint) npcFlags);
        }

        private static void WriteGODynamic(ObjectBase obj, Character receiver, UpdatePacket packet)
        {
            GameObject gameObject = (GameObject) obj;
            if (gameObject is Transport || !gameObject.Flags.HasAnyFlag(GameObjectFlags.ConditionalInteraction))
            {
                packet.Write(obj.GetUInt32(GameObjectFields.DYNAMIC));
            }
            else
            {
                GODynamicLowFlags goDynamicLowFlags = !gameObject.CanBeUsedBy(receiver)
                    ? GODynamicLowFlags.None
                    : GODynamicLowFlags.Clickable | GODynamicLowFlags.Sparkle;
                packet.Write((ushort) goDynamicLowFlags);
                packet.Write(ushort.MaxValue);
            }
        }

        private static void WriteUnitDynFlags(ObjectBase obj, Character receiver, UpdatePacket packet)
        {
            Unit unit = (Unit) obj;
            UnitDynamicFlags dynamicFlags = unit.DynamicFlags;
            Asda2Loot loot = obj.Loot;
            if (loot != null && receiver.LooterEntry.MayLoot(loot) && !unit.IsAlive)
            {
                dynamicFlags |= UnitDynamicFlags.Lootable;
            }
            else
            {
                Unit firstAttacker = unit.FirstAttacker;
                if (firstAttacker != null)
                {
                    if ((firstAttacker == receiver || firstAttacker.IsAlliedWith((IFactionMember) receiver)) &&
                        unit.IsAlive)
                        dynamicFlags |= UnitDynamicFlags.TaggedByMe;
                    else
                        dynamicFlags |= UnitDynamicFlags.TaggedByOther;
                }
            }

            packet.Write((uint) dynamicFlags);
        }

        private static void WriteCorpseDynFlags(ObjectBase obj, Character receiver, UpdatePacket packet)
        {
            if (((Corpse) obj).Owner == receiver)
                packet.Write(1U);
            else
                packet.Write(0U);
        }

        /// <summary>
        /// Handles writing of Dynamic UpdateFields. Be sure to definitely
        /// *always* write 4 bytes when a Handler is called.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="receiver"></param>
        /// <param name="packet"></param>
        public delegate void DynamicUpdateFieldHandler(ObjectBase obj, Character receiver, UpdatePacket packet);
    }
}