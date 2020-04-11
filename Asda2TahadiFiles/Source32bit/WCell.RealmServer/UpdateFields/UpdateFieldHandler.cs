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
    [NotVariable]public static DynamicUpdateFieldHandler[] DynamicObjectFieldHandlers;
    [NotVariable]public static DynamicUpdateFieldHandler[] DynamicItemFieldHandlers;
    [NotVariable]public static DynamicUpdateFieldHandler[] DynamicContainerFieldHandlers;
    [NotVariable]public static DynamicUpdateFieldHandler[] DynamicDOFieldHandlers;
    [NotVariable]public static DynamicUpdateFieldHandler[] DynamicGOHandlers;
    [NotVariable]public static DynamicUpdateFieldHandler[] DynamicCorpseHandlers;
    [NotVariable]public static DynamicUpdateFieldHandler[] DynamicUnitHandlers;
    [NotVariable]public static DynamicUpdateFieldHandler[] DynamicPlayerHandlers;

    [Initialization(InitializationPass.First)]
    public static void Init()
    {
      UpdateFieldMgr.Init();
      DynamicObjectFieldHandlers =
        new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Object).TotalLength];
      DynamicItemFieldHandlers =
        new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Item).TotalLength];
      DynamicContainerFieldHandlers =
        new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Container)
          .TotalLength];
      DynamicDOFieldHandlers =
        new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.DynamicObject)
          .TotalLength];
      DynamicGOHandlers =
        new DynamicUpdateFieldHandler
          [UpdateFieldMgr.Get(ObjectTypeId.GameObject).TotalLength];
      DynamicCorpseHandlers =
        new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Corpse).TotalLength];
      DynamicUnitHandlers =
        new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Unit).TotalLength];
      DynamicPlayerHandlers =
        new DynamicUpdateFieldHandler[UpdateFieldMgr.Get(ObjectTypeId.Player).TotalLength];
      InitHandlers();
      Inherit(DynamicItemFieldHandlers,
        DynamicObjectFieldHandlers);
      Inherit(DynamicContainerFieldHandlers,
        DynamicItemFieldHandlers);
      Inherit(DynamicDOFieldHandlers,
        DynamicObjectFieldHandlers);
      Inherit(DynamicGOHandlers,
        DynamicObjectFieldHandlers);
      Inherit(DynamicCorpseHandlers,
        DynamicObjectFieldHandlers);
      Inherit(DynamicUnitHandlers,
        DynamicObjectFieldHandlers);
      Inherit(DynamicPlayerHandlers,
        DynamicUnitHandlers);
    }

    private static void Inherit(DynamicUpdateFieldHandler[] handlers,
      DynamicUpdateFieldHandler[] baseHandlers)
    {
      Array.Copy(baseHandlers, 0, handlers, 0, baseHandlers.Length);
    }

    private static void InitHandlers()
    {
      DynamicGOHandlers[14] =
        WriteGODynamic;
      DynamicCorpseHandlers[34] =
        WriteCorpseDynFlags;
      DynamicUnitHandlers[82] =
        WriteNPCFlags;
      DynamicUnitHandlers[79] =
        WriteUnitDynFlags;
      DynamicUnitHandlers[61] =
        WriteAuraStateFlags;
    }

    private static void WriteAuraStateFlags(ObjectBase obj, Character reciever, UpdatePacket packet)
    {
      Unit unit = (Unit) obj;
      Spell strongestImmolate = unit.GetStrongestImmolate();
      if(strongestImmolate != null && unit.Auras[strongestImmolate].SpellCast.CasterChar == reciever)
        packet.Write((uint) (unit.AuraState | AuraStateMask.Immolate));
      else
        packet.Write((uint) (unit.AuraState & ~AuraStateMask.Immolate));
    }

    private static void WriteNPCFlags(ObjectBase obj, Character chr, UpdatePacket packet)
    {
      NPCFlags npcFlags = (NPCFlags) obj.GetUInt32(UnitFields.NPC_FLAGS);
      if(obj is NPC)
      {
        NPC npc = (NPC) obj;
        if(npc.IsTrainer && !npc.TrainerEntry.CanTrain(chr))
          npcFlags = NPCFlags.None;
      }

      packet.Write((uint) npcFlags);
    }

    private static void WriteGODynamic(ObjectBase obj, Character receiver, UpdatePacket packet)
    {
      GameObject gameObject = (GameObject) obj;
      if(gameObject is Transport || !gameObject.Flags.HasAnyFlag(GameObjectFlags.ConditionalInteraction))
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
      if(loot != null && receiver.LooterEntry.MayLoot(loot) && !unit.IsAlive)
      {
        dynamicFlags |= UnitDynamicFlags.Lootable;
      }
      else
      {
        Unit firstAttacker = unit.FirstAttacker;
        if(firstAttacker != null)
        {
          if((firstAttacker == receiver || firstAttacker.IsAlliedWith(receiver)) &&
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
      if(((Corpse) obj).Owner == receiver)
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