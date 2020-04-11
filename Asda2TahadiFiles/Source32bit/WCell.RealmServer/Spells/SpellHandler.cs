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
    [NotVariable]public static bool AnimateSpellAdd = true;

    /// <summary>
    /// Minimum length of cooldowns that are to be saved to DB in milliseconds
    /// </summary>
    public static int MinCooldownSaveTimeMillis = 30;

    public static float SpellCritBaseFactor = 1.5f;
    [NotVariable]public static Spell[] ById = new Spell[2262];

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
      new SpellEffectHandlerCreator[(int) Convert.ChangeType(Utility.GetMaxEnum<SpellEffectType>(),
                                      typeof(int)) + 1];

    public static readonly Dictionary<SummonType, SpellSummonEntry> SummonEntries =
      new Dictionary<SummonType, SpellSummonEntry>();

    public static readonly SpellSummonHandler DefaultSummonHandler = new SpellSummonHandler();
    public static readonly SpellSummonHandler PetSummonHandler = new SpellSummonPetHandler();

    public static readonly SpellSummonHandler PossesedSummonHandler =
      new SpellSummonPossessedHandler();

    private static bool loaded;

    /// <summary>
    /// Sends initially all spells and item cooldowns to the character
    /// </summary>
    public static void SendSpellsAndCooldowns(Character chr)
    {
      PlayerSpellCollection playerSpells = chr.PlayerSpells;
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_INITIAL_SPELLS,
        5 + 4 * playerSpells.Count))
      {
        packet.Write((byte) 0);
        packet.Write((ushort) playerSpells.Count);
        foreach(Spell allSpell in playerSpells.AllSpells)
        {
          packet.Write(allSpell.Id);
          packet.Write((ushort) 0);
        }

        long position = packet.Position;
        ushort num = 0;
        packet.Position = position + 2L;
        long ticks1 = DateTime.Now.Ticks;
        foreach(ISpellIdCooldown idCooldown in playerSpells.IdCooldowns)
        {
          int ticks2 = (int) (idCooldown.Until.Ticks - ticks1);
          if(ticks2 > 10)
          {
            ++num;
            packet.Write(idCooldown.SpellId);
            packet.Write((ushort) idCooldown.ItemId);
            packet.Write((ushort) 0);
            packet.Write(Utility.ToMilliSecondsInt(ticks2));
            packet.Write(0);
          }
        }

        foreach(ISpellCategoryCooldown categoryCooldown in playerSpells.CategoryCooldowns)
        {
          int ticks2 = (int) (categoryCooldown.Until.Ticks - ticks1);
          if(ticks2 > 10)
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
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LEARNED_SPELL, 4))
      {
        packet.WriteUInt(spellId);
        packet.WriteUShort(0);
        client.Send(packet, false);
      }
    }

    public static void SendSpellSuperceded(IPacketReceiver client, uint spellId, uint newSpellId)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_SUPERCEDED_SPELL, 8))
      {
        packet.Write(spellId);
        packet.Write(newSpellId);
        client.Send(packet, false);
      }
    }

    /// <summary>Removes a spell from the client's spellbook</summary>
    public static void SendSpellRemoved(Character chr, uint spellId)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_REMOVED_SPELL, 4))
      {
        packet.WriteUInt(spellId);
        chr.Client.Send(packet, false);
      }
    }

    public static SpellTargetFlags GetTargetFlags(WorldObject obj)
    {
      if(obj is Unit)
        return SpellTargetFlags.Unit;
      if(obj is GameObject)
        return SpellTargetFlags.GameObject;
      return obj is Corpse ? SpellTargetFlags.PvPCorpse : SpellTargetFlags.Self;
    }

    public static void SendUnitCastStart(IRealmClient client, SpellCast cast, WorldObject target)
    {
      using(RealmPacketOut packet =
        new RealmPacketOut(RealmServerOpCode.SMSG_UNIT_SPELLCAST_START, 28))
      {
        cast.CasterReference.EntityId.WritePacked(packet);
        target.EntityId.WritePacked(packet);
        packet.Write(cast.Spell.Id);
        packet.Write(cast.Spell.CastDelay);
        packet.Write(cast.Spell.CastDelay);
        client.Send(packet, false);
      }
    }

    public static void SendCastStart(SpellCast cast)
    {
      if(cast.CasterObject != null && !cast.CasterObject.IsAreaActive)
        return;
      int maxContentLength = 150;
      Spell spell = cast.Spell;
      if(spell == null)
        return;
      using(RealmPacketOut packet =
        new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_START, maxContentLength))
      {
        WriteCaster(cast, packet);
        packet.Write(spell.Id);
        packet.Write((int) cast.StartFlags);
        packet.Write(cast.CastDelay);
        WriteTargets(packet, cast);
        if(cast.StartFlags.HasFlag(CastFlags.RunicPowerGain))
          packet.Write(0);
        if(cast.StartFlags.HasFlag(CastFlags.RuneCooldownList))
        {
          byte num1 = 0;
          byte num2 = 0;
          packet.Write(num1);
          packet.Write(num2);
          for(int index = 0; index < 6; ++index)
          {
            byte num3 = (byte) (1 << index);
            if((num3 & num1) != 0 && (num3 & num2) == 0)
              packet.WriteByte(0);
          }
        }

        if(cast.StartFlags.HasFlag(CastFlags.Ranged))
          WriteAmmoInfo(cast, packet);
        if(cast.StartFlags.HasFlag(CastFlags.Flag_0x4000000))
        {
          packet.Write(0);
          packet.Write(0);
        }

        if(cast.TargetFlags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
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
      if(flags == SpellTargetFlags.Self || flags == SpellTargetFlags.Self)
      {
        Spell spell = cast.Spell;
        if(cast.SelectedTarget is Unit && !spell.IsAreaSpell &&
           (spell.Visual != 0U || spell.IsPhysicalAbility))
          flags = SpellTargetFlags.Unit;
      }

      packet.Write((uint) flags);
      if(flags.HasAnyFlag(SpellTargetFlags.WorldObject))
      {
        if(cast.SelectedTarget == null)
          packet.Write((byte) 0);
        else
          cast.SelectedTarget.EntityId.WritePacked(packet);
      }

      if(flags.HasAnyFlag(SpellTargetFlags.AnyItem) && cast.TargetItem != null)
        cast.TargetItem.EntityId.WritePacked(packet);
      if(flags.HasAnyFlag(SpellTargetFlags.SourceLocation))
      {
        if(cast.SelectedTarget != null)
          cast.SelectedTarget.EntityId.WritePacked(packet);
        else
          packet.Write((byte) 0);
        packet.Write(cast.SourceLoc.X);
        packet.Write(cast.SourceLoc.Y);
        packet.Write(cast.SourceLoc.Z);
      }

      if(flags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
      {
        if(cast.SelectedTarget != null)
          cast.SelectedTarget.EntityId.WritePacked(packet);
        else
          packet.Write((byte) 0);
        packet.Write(cast.TargetLoc);
      }

      if(!flags.HasAnyFlag(SpellTargetFlags.String))
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
      if(cast.CasterObject != null && !cast.CasterObject.IsAreaActive || !cast.IsCasting)
        return;
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_GO,
        24 + (hitTargets != null ? hitTargets.Count * 8 : 0) +
        (missedTargets != null ? missedTargets.Count * 10 : 0)))
      {
        cast.CasterReference.EntityId.WritePacked(packet);
        caster2.EntityId.WritePacked(packet);
        packet.Write(cast.Spell.Id);
        CastFlags goFlags = cast.GoFlags;
        packet.Write((int) goFlags);
        packet.Write(Utility.GetEpochTime());
        packet.WriteByte(hitTargets != null ? hitTargets.Count : 0);
        if(hitTargets != null && cast.CasterObject != null)
        {
          foreach(WorldObject hitTarget in hitTargets)
          {
            packet.Write(hitTarget.EntityId);
            if(hitTarget is Character)
              SendCastSuccess(cast.CasterObject, cast.Spell.Id,
                hitTarget as Character);
          }
        }

        packet.WriteByte(missedTargets != null ? missedTargets.Count : 0);
        if(missedTargets != null)
        {
          foreach(MissedTarget missedTarget in missedTargets)
          {
            packet.Write(missedTarget.Target.EntityId);
            packet.Write((byte) missedTarget.Reason);
            if(missedTarget.Reason == CastMissReason.Reflect)
              packet.Write((byte) 0);
          }
        }

        WriteTargets(packet, cast);
        if(goFlags.HasFlag(CastFlags.RunicPowerGain))
          packet.Write(0);
        if(goFlags.HasFlag(CastFlags.RuneCooldownList))
        {
          byte activeRuneMask = cast.CasterChar.PlayerSpells.Runes.GetActiveRuneMask();
          packet.Write(previousRuneMask);
          packet.Write(activeRuneMask);
          for(int index = 0; index < 6; ++index)
          {
            byte num = (byte) (1 << index);
            if((num & previousRuneMask) != 0 && (num & activeRuneMask) == 0)
              packet.WriteByte(0);
          }
        }

        if(goFlags.HasFlag(CastFlags.Flag_0x20000))
        {
          packet.WriteFloat(0);
          packet.Write(0);
        }

        if(cast.StartFlags.HasFlag(CastFlags.Ranged))
          WriteAmmoInfo(cast, packet);
        if(goFlags.HasFlag(CastFlags.Flag_0x80000))
        {
          packet.Write(0);
          packet.Write(0);
        }

        if(cast.TargetFlags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
          packet.Write((byte) 0);
        cast.SendPacketToArea(packet);
      }
    }

    private static void WriteCaster(SpellCast cast, RealmPacketOut packet)
    {
      if(cast.TargetItem != null)
        cast.CasterItem.EntityId.WritePacked(packet);
      else
        cast.CasterReference.EntityId.WritePacked(packet);
      cast.CasterReference.EntityId.WritePacked(packet);
    }

    /// <summary>This is sent to caster if spell fails</summary>
    internal static void SendCastFailed(IPacketReceiver client, Spell spell, SpellFailedReason result)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CAST_FAILED,
        result == SpellFailedReason.RequiresSpellFocus || result == SpellFailedReason.RequiresArea ? 10 : 6))
      {
        packet.Write(spell.Id);
        packet.Write((byte) result);
        switch(result)
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
      if(spellCast.Client == null)
        return;
      SendCastFailed(spellCast.Client, spellCast.Spell, reason);
      SendSpellFailure(spellCast, reason);
      SendSpellFailedOther(spellCast, reason);
    }

    internal static void SendSpellFailure(SpellCast spellCast, SpellFailedReason reason)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_FAILURE, 15))
      {
        spellCast.CasterReference.EntityId.WritePacked(packet);
        packet.Write(spellCast.Spell.Id);
        packet.Write((byte) reason);
        spellCast.SendPacketToArea(packet);
      }
    }

    internal static void SendSpellFailedOther(SpellCast spellCast, SpellFailedReason reason)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_FAILED_OTHER, 15))
      {
        spellCast.CasterReference.EntityId.WritePacked(packet);
        packet.Write(spellCast.Spell.Id);
        packet.Write((byte) reason);
        spellCast.SendPacketToArea(packet);
      }
    }

    /// <summary>Delays the spell-cast</summary>
    public static void SendCastDelayed(SpellCast cast, int delay)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_DELAYED, 12))
      {
        cast.CasterReference.EntityId.WritePacked(packet);
        packet.Write(delay);
        cast.SendPacketToArea(packet);
      }
    }

    /// <summary>Starts Channeling</summary>
    public static void SendChannelStart(SpellCast cast, SpellId spellId, int duration)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_CHANNEL_START, 12))
      {
        cast.CasterReference.EntityId.WritePacked(packet);
        packet.Write((uint) spellId);
        packet.Write(duration);
        cast.SendPacketToArea(packet);
      }
    }

    /// <summary>Changes the time of the channel</summary>
    public static void SendChannelUpdate(SpellCast cast, uint delay)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_CHANNEL_UPDATE, 12))
      {
        cast.CasterReference.EntityId.WritePacked(packet);
        packet.Write(delay);
        cast.SendPacketToArea(packet);
      }
    }

    /// <summary>Shows a spell visual</summary>
    public static void SendVisual(WorldObject target, SpellId id)
    {
      Spell spell = Get(id);
      SendVisual(target, spell.Visual);
    }

    /// <summary>Shows a spell visual</summary>
    public static void SendVisual(WorldObject target, uint visualId)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_PLAY_SPELL_VISUAL, 12))
      {
        packet.Write(target.EntityId);
        packet.Write(visualId);
        target.SendPacketToArea(packet, true, false, Locale.Any, new float?());
      }
    }

    public static void SendImpact(WorldObject target, SpellId id)
    {
      Spell spell = Get(id);
      SendImpact(target, spell.Visual);
    }

    /// <summary>Shows a spell Impact animation</summary>
    public static void SendImpact(WorldObject target, uint visualId)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_PLAY_SPELL_IMPACT, 12))
      {
        packet.Write(target.EntityId);
        packet.Write(visualId);
        target.SendPacketToArea(packet, true, false, Locale.Any, new float?());
      }
    }

    /// <summary>Send a custom cooldown time to the client</summary>
    public static void SendSpellCooldown(WorldObject caster, IRealmClient client, uint spellId, ushort cooldown)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_COOLDOWN, 14))
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
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ITEM_COOLDOWN, 14))
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
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_COOLDOWN_EVENT, 12))
      {
        packet.WriteUInt((uint) spellId);
        chr.EntityId.WritePacked(packet);
        chr.Send(packet, false);
      }
    }

    /// <summary>Sends spell modifier update</summary>
    public static void SendSpellModifier(Character chr, byte groupBitNumber, SpellModifierType type, int amount,
      bool isPercent)
    {
      using(RealmPacketOut packet = new RealmPacketOut(
        isPercent
          ? RealmServerOpCode.SMSG_SET_PCT_SPELL_MODIFIER
          : RealmServerOpCode.SMSG_SET_FLAT_SPELL_MODIFIER, 6))
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
      using(RealmPacketOut packet =
        new RealmPacketOut(RealmServerOpCode.SMSG_SET_PROJECTILE_POSITION, 21))
      {
        casterId.WritePacked(packet);
        packet.WriteByte(castCount);
        packet.WriteFloat(position.X);
        packet.WriteFloat(position.Y);
        packet.WriteFloat(position.Z);
        client.Send(packet, false);
      }
    }

    public static void HandleCastSpell(IRealmClient client, RealmPacketIn packet)
    {
      int num1 = packet.ReadByte();
      uint index = packet.ReadUInt32();
      int num2 = packet.ReadByte();
      if(client.ActiveCharacter.Spells[index] == null)
        return;
      SpellCast spellCast = client.ActiveCharacter.SpellCast;
    }

    public static void HandleCancelCastSpell(IRealmClient client, RealmPacketIn packet)
    {
      int num = (int) packet.ReadUInt32();
      if(!client.ActiveCharacter.IsUsingSpell)
        return;
      client.ActiveCharacter.SpellCast.Cancel(SpellFailedReason.Interrupted);
    }

    /// <summary>Somehow seems to be the same as CMSG_CANCEL_CAST</summary>
    public static void HandleCancelChanneling(IRealmClient client, RealmPacketIn packet)
    {
      int num = (int) packet.ReadUInt32();
      Character activeCharacter = client.ActiveCharacter;
      if(activeCharacter.MoveControl.Mover != activeCharacter)
        return;
      SpellCast spellCast = activeCharacter.SpellCast;
      if(spellCast == null)
        return;
      spellCast.Cancel(SpellFailedReason.Interrupted);
    }

    public static void HandleCancelAutoRepeat(IRealmClient client, RealmPacketIn packet)
    {
      client.ActiveCharacter.AutorepeatSpell = null;
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
      if(npc == null)
        return;
      if(npc.Entry.IsVehicle)
      {
        VehicleSeat seatFor = (npc as Vehicle).GetSeatFor(activeCharacter);
        if(seatFor == null)
          return;
        seatFor.Enter(activeCharacter);
      }
      else
      {
        SpellTriggerInfo spellTriggerInfo;
        if((spellTriggerInfo = npc.Entry.SpellTriggerInfo) == null)
          return;
        int num = (int) activeCharacter.SpellCast.Start(spellTriggerInfo.Spell, false);
      }
    }

    public static void HandleUpdateMissilePosition(IRealmClient client, RealmPacketIn packet)
    {
      packet.ReadPackedEntityId();
      packet.ReadInt32();
      int num = packet.ReadByte();
      packet.ReadVector3();
    }

    public static void SendConvertRune(IRealmClient client, uint index, RuneType type)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CONVERT_RUNE, 2))
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
      if(spellId >= ById.Length)
        return null;
      return ById[spellId];
    }

    /// <summary>
    /// Returns the spell with the given spellId or null if it doesn't exist
    /// </summary>
    public static Spell Get(SpellId spellId)
    {
      if((long) spellId >= ById.Length)
        return null;
      return ById[(uint) spellId];
    }

    internal static void AddSpell(Spell spell)
    {
      ArrayUtil.Set(ref ById, spell.Id, spell);
      HighestId = Math.Max(spell.Id, HighestId);
    }

    /// <summary>Can be used to add a Spell that does not exist.</summary>
    public static Spell AddCustomSpell(string name)
    {
      return AddCustomSpell(HighestId + 1U, name);
    }

    /// <summary>
    /// Can be used to add a Spell that does not exist.
    /// Usually used for spells that are unknown to the client to signal a certain state.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Spell AddCustomSpell(uint id, string name)
    {
      if(Get(id) != null)
        throw new ArgumentException("Invalid custom spell id is already in use: " + id + " - " + name);
      Spell spell = new Spell
      {
        Id = id,
        SpellId = (SpellId) id,
        Name = "[" + RealmLocalizer.Instance.Translate(RealmLangKey.Custom).ToUpper() + "] " +
               name,
        Effects = new SpellEffect[0],
        RequiredToolIds = new uint[0]
      };
      AddSpell(spell);
      return spell;
    }

    public static void RemoveSpell(uint id)
    {
      ById[id] = null;
    }

    public static void RemoveSpell(SpellId id)
    {
      ById[(int) id] = null;
    }

    /// <summary>
    /// Apply the given action on all Spells with the given ids
    /// </summary>
    /// <param name="action"></param>
    public static void Apply(this SpellLineId id, Action<Spell> action)
    {
      SpellLine line = id.GetLine();
      if(line == null)
        throw new Exception("Invalid SpellLineId: " + id);
      action.Apply(line);
    }

    /// <summary>
    /// Apply the given action on all Spells with the given ids
    /// </summary>
    /// <param name="action"></param>
    public static void Apply(Action<Spell> action, SpellLineId id, params SpellId[] ids)
    {
      SpellLine line = id.GetLine();
      if(line == null)
        throw new Exception("Invalid SpellLineId: " + id);
      action.Apply(line);
      action.Apply((IEnumerable<SpellId>) ids);
    }

    /// <summary>
    /// Apply the given action on all Spells with the given ids
    /// </summary>
    /// <param name="action"></param>
    public static void Apply(Action<Spell> action, SpellLineId id, SpellLineId id2, params SpellId[] ids)
    {
      SpellLine line1 = id.GetLine();
      if(line1 == null)
        throw new Exception("Invalid SpellLineId: " + id);
      SpellLine line2 = id2.GetLine();
      if(line2 == null)
        throw new Exception("Invalid SpellLineId: " + id2);
      action.Apply(line1);
      action.Apply(line2);
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
      foreach(SpellLineId id in ids)
      {
        SpellLine line = id.GetLine();
        action.Apply(line);
      }
    }

    /// <summary>
    /// Apply the given action on all Spells with the given ids
    /// </summary>
    /// <param name="action"></param>
    public static void Apply(this Action<Spell> action, IEnumerable<SpellId> ids)
    {
      foreach(SpellId id in ids)
      {
        Spell spell = Get(id);
        if(spell == null)
          throw new Exception("Invalid SpellId: " + id);
        action(spell);
      }
    }

    /// <summary>
    /// Apply the given action on all Spells with the given ids
    /// </summary>
    /// <param name="action"></param>
    public static void Apply(this Action<Spell> action, IEnumerable<Spell> spells)
    {
      foreach(Spell spell in spells)
        action(spell);
    }

    /// <summary>
    /// Returns a list of all SpellLines that are affected by the given spell family set (very long bit field)
    /// </summary>
    public static HashSet<SpellLine> GetAffectedSpellLines(ClassId clss, uint[] mask)
    {
      SpellLine[] lines = SpellLines.GetLines(clss);
      HashSet<SpellLine> spellLineSet = new HashSet<SpellLine>();
      if(lines != null)
      {
        foreach(SpellLine spellLine in lines)
        {
          foreach(Spell spell in spellLine)
          {
            if(spell.MatchesMask(mask))
            {
              spellLineSet.Add(spellLine);
              break;
            }
          }
        }
      }

      return spellLineSet;
    }

    [Initialization(InitializationPass.First, "Initialize Spells")]
    public static void LoadSpells()
    {
      LoadSpells(false);
    }

    public static void LoadSpells(bool init)
    {
      if(!loaded)
      {
        InitEffectHandlers();
        SpellEffect.InitMiscValueTypes();
        loaded = true;
        LoadOverrides();
        SpellLines.InitSpellLines();
        ProcEventHelper.PatchSpells(ById);
      }

      if(!init)
        return;
      Initialize2();
    }

    /// <summary>Second initialization pass</summary>
    [Initialization(InitializationPass.Third, "Initialize Spells (2)")]
    public static void Initialize2()
    {
      List<Spell> spellList = new List<Spell>(5900);
      foreach(Spell spell in ById)
      {
        if(spell != null)
        {
          spell.Initialize();
          if(spell.IsTeachSpell)
            spellList.Add(spell);
          if(spell.DOEffect != null)
            DOSpells[spell.SpellId] = spell;
        }
      }

      AuraHandler.RegisterAuraUIDEvaluators();
      foreach(Spell spell in ById)
      {
        if(spell != null)
          spell.Init2();
      }

      SkillHandler.Initialize2();
    }

    /// <summary>Load given Spell-data from DB</summary>
    private static void LoadOverrides()
    {
      LightDBMapper mapper = ContentMgr.GetMapper<Spell>();
      mapper.AddObjectsUInt(ById);
      ContentMgr.Load(mapper);
    }

    internal static void InitTools()
    {
      foreach(Spell spellsRequiringTool in SpellsRequiringTools)
      {
        foreach(uint requiredToolId in spellsRequiringTool.RequiredToolIds)
        {
          if(requiredToolId > 0U)
          {
            ItemTemplate val = ItemMgr.Templates.Get(requiredToolId);
            if(val != null)
            {
              if(spellsRequiringTool.RequiredTools == null)
                spellsRequiringTool.RequiredTools =
                  new ItemTemplate[spellsRequiringTool.RequiredToolIds.Length];
              int num = (int) ArrayUtil.Add(ref spellsRequiringTool.RequiredTools, val);
            }
          }
        }

        if(spellsRequiringTool.RequiredTools != null)
          ArrayUtil.Prune(ref spellsRequiringTool.RequiredTools);
      }
    }

    private static void InitEffectHandlers()
    {
      SpellEffectCreators[1] = (cast, effect) =>
        (SpellEffectHandler) new InstantKillEffectHandler(cast, effect);
      SpellEffectCreators[2] = (cast, effect) =>
        (SpellEffectHandler) new SchoolDamageEffectHandler(cast, effect);
      SpellEffectCreators[5] = (cast, effect) =>
        (SpellEffectHandler) new TeleportUnitsEffectHandler(cast, effect);
      SpellEffectCreators[6] = (cast, effect) =>
        (SpellEffectHandler) new ApplyAuraEffectHandler(cast, effect);
      SpellEffectCreators[7] = (cast, effect) =>
        (SpellEffectHandler) new EnvironmentalDamageEffectHandler(cast, effect);
      SpellEffectCreators[8] = (cast, effect) =>
        (SpellEffectHandler) new PowerDrainEffectHandler(cast, effect);
      SpellEffectCreators[9] = (cast, effect) =>
        (SpellEffectHandler) new HealthLeechEffectHandler(cast, effect);
      SpellEffectCreators[10] = (cast, effect) =>
        (SpellEffectHandler) new HealEffectHandler(cast, effect);
      SpellEffectCreators[11] = (cast, effect) =>
        (SpellEffectHandler) new BindEffectHandler(cast, effect);
      SpellEffectCreators[12] = (cast, effect) =>
        (SpellEffectHandler) new PortalHandler(cast, effect);
      SpellEffectCreators[16] = (cast, effect) =>
        (SpellEffectHandler) new QuestCompleteEffectHandler(cast, effect);
      SpellEffectCreators[17] = (cast, effect) =>
        (SpellEffectHandler) new WeaponDamageNoSchoolEffectHandler(cast, effect);
      SpellEffectCreators[18] = (cast, effect) =>
        (SpellEffectHandler) new ResurrectEffectHandler(cast, effect);
      SpellEffectCreators[19] = (cast, effect) =>
        (SpellEffectHandler) new AddExtraAttacksEffectHandler(cast, effect);
      SpellEffectCreators[24] = (cast, effect) =>
        (SpellEffectHandler) new CreateItemEffectHandler(cast, effect);
      SpellEffectCreators[25] = (cast, effect) =>
        (SpellEffectHandler) new WeaponEffectHandler(cast, effect);
      SpellEffectCreators[27] = (cast, effect) =>
        (SpellEffectHandler) new PersistantAreaAuraEffectHandler(cast, effect);
      SpellEffectCreators[28] = (cast, effect) =>
        (SpellEffectHandler) new SummonEffectHandler(cast, effect);
      SpellEffectCreators[30] = (cast, effect) =>
        (SpellEffectHandler) new EnergizeEffectHandler(cast, effect);
      SpellEffectCreators[31] = (cast, effect) =>
        (SpellEffectHandler) new WeaponDamageEffectHandler(cast, effect);
      SpellEffectCreators[33] = (cast, effect) =>
        (SpellEffectHandler) new OpenLockEffectHandler(cast, effect);
      SpellEffectCreators[35] = (cast, effect) =>
        (SpellEffectHandler) new ApplyAreaAuraEffectHandler(cast, effect);
      SpellEffectCreators[65] = (cast, effect) =>
        (SpellEffectHandler) new ApplyAreaAura2EffectHandler(cast, effect);
      SpellEffectCreators[36] = (cast, effect) =>
        (SpellEffectHandler) new LearnSpellEffectHandler(cast, effect);
      SpellEffectCreators[38] = (cast, effect) =>
        (SpellEffectHandler) new DispelEffectHandler(cast, effect);
      SpellEffectCreators[39] = (cast, effect) =>
        (SpellEffectHandler) new LanguageEffectHandler(cast, effect);
      SpellEffectCreators[40] = (cast, effect) =>
        (SpellEffectHandler) new DualWeildEffectHandler(cast, effect);
      SpellEffectCreators[44] = (cast, effect) =>
        (SpellEffectHandler) new SkillStepEffectHandler(cast, effect);
      SpellEffectCreators[48] = (cast, effect) =>
        (SpellEffectHandler) new StealthEffectHandler(cast, effect);
      SpellEffectCreators[50] = (cast, effect) =>
        (SpellEffectHandler) new SummonObjectEffectHandler(cast, effect);
      SpellEffectCreators[76] = (cast, effect) =>
        (SpellEffectHandler) new SummonObjectWildEffectHandler(cast, effect);
      SpellEffectCreators[53] = (cast, effect) =>
        (SpellEffectHandler) new EnchantItemEffectHandler(cast, effect);
      SpellEffectCreators[54] = (cast, effect) =>
        (SpellEffectHandler) new EnchantItemTemporaryEffectHandler(cast, effect);
      SpellEffectCreators[55] = (cast, effect) =>
        (SpellEffectHandler) new TameCreatureEffectHandler(cast, effect);
      SpellEffectCreators[56] = (cast, effect) =>
        (SpellEffectHandler) new SummonPetEffectHandler(cast, effect);
      SpellEffectCreators[58] = (cast, effect) =>
        (SpellEffectHandler) new WeaponDamageEffectHandler(cast, effect);
      SpellEffectCreators[61] = (cast, effect) =>
        (SpellEffectHandler) new SendEventEffectHandler(cast, effect);
      SpellEffectCreators[63] = (cast, effect) =>
        (SpellEffectHandler) new ThreatHandler(cast, effect);
      SpellEffectCreators[64] = (cast, effect) =>
        (SpellEffectHandler) new TriggerSpellEffectHandler(cast, effect);
      SpellEffectCreators[66] = (cast, effect) =>
        (SpellEffectHandler) new CreateManaGemEffectHandler(cast, effect);
      SpellEffectCreators[67] = (cast, effect) =>
        (SpellEffectHandler) new HealMaxHealthEffectHandler(cast, effect);
      SpellEffectCreators[68] = (cast, effect) =>
        (SpellEffectHandler) new InterruptCastEffectHandler(cast, effect);
      SpellEffectCreators[69] = (cast, effect) =>
        (SpellEffectHandler) new DistractEffectHandler(cast, effect);
      SpellEffectCreators[74] = (cast, effect) =>
        (SpellEffectHandler) new ApplyGlyphEffectHandler(cast, effect);
      SpellEffectCreators[77] = (cast, effect) =>
        (SpellEffectHandler) new ScriptEffectHandler(cast, effect);
      SpellEffectCreators[79] = (cast, effect) =>
        (SpellEffectHandler) new RemoveImpairingEffectsHandler(cast, effect);
      SpellEffectCreators[80] = (cast, effect) =>
        (SpellEffectHandler) new AddComboPointsEffectHandler(cast, effect);
      SpellEffectCreators[83] = (cast, effect) =>
        (SpellEffectHandler) new DuelEffectHandler(cast, effect);
      SpellEffectCreators[85] = (cast, effect) =>
        (SpellEffectHandler) new SummonPlayerEffectHandler(cast, effect);
      SpellEffectCreators[87] =
        (cast, effect) => (SpellEffectHandler) new WMODamage(cast, effect);
      SpellEffectCreators[88] =
        (cast, effect) => (SpellEffectHandler) new WMORepair(cast, effect);
      SpellEffectCreators[89] =
        (cast, effect) => (SpellEffectHandler) new WMOChange(cast, effect);
      SpellEffectCreators[90] = (cast, effect) =>
        (SpellEffectHandler) new KillCreditPersonal(cast, effect);
      SpellEffectCreators[94] = (cast, effect) =>
        (SpellEffectHandler) new SelfResurrectEffectHandler(cast, effect);
      SpellEffectCreators[95] = (cast, effect) =>
        (SpellEffectHandler) new SkinningEffectHandler(cast, effect);
      SpellEffectCreators[96] = (cast, effect) =>
        (SpellEffectHandler) new ChargeEffectHandler(cast, effect);
      SpellEffectCreators[97] = (cast, effect) =>
        (SpellEffectHandler) new SummonAllTotemsHandler(cast, effect);
      SpellEffectCreators[98] = (cast, effect) =>
        (SpellEffectHandler) new KnockBackEffectHandler(cast, effect);
      SpellEffectCreators[99] = (cast, effect) =>
        (SpellEffectHandler) new DisenchantEffectHandler(cast, effect);
      SpellEffectCreators[100] =
        (cast, effect) => (SpellEffectHandler) new Inebriate(cast, effect);
      SpellEffectCreators[102] = (cast, effect) =>
        (SpellEffectHandler) new DismissPetEffectHandler(cast, effect);
      SpellEffectCreators[108] = (cast, effect) =>
        (SpellEffectHandler) new DispelMechanicEffectHandler(cast, effect);
      SpellEffectCreators[109] = (cast, effect) =>
        (SpellEffectHandler) new SummonDeadPetEffectHandler(cast, effect);
      SpellEffectCreators[113] = (cast, effect) =>
        (SpellEffectHandler) new ResurrectFlatEffectHandler(cast, effect);
      SpellEffectCreators[118] = (cast, effect) =>
        (SpellEffectHandler) new SkillEffectHandler(cast, effect);
      SpellEffectCreators[119] = (cast, effect) =>
        (SpellEffectHandler) new ApplyPetAuraEffectHandler(cast, effect);
      SpellEffectCreators[121] = (cast, effect) =>
        (SpellEffectHandler) new NormalizedWeaponDamagePlusEffectHandler(cast, effect);
      SpellEffectCreators[123] = (cast, effect) =>
        (SpellEffectHandler) new VideoEffectHandler(cast, effect);
      SpellEffectCreators[126] = (cast, effect) =>
        (SpellEffectHandler) new StealBeneficialBuffEffectHandler(cast, effect);
      SpellEffectCreators[sbyte.MaxValue] = (cast, effect) =>
        (SpellEffectHandler) new ProspectingEffectHandler(cast, effect);
      SpellEffectCreators[128] = (cast, effect) =>
        (SpellEffectHandler) new ApplyStatAuraEffectHandler(cast, effect);
      SpellEffectCreators[129] = (cast, effect) =>
        (SpellEffectHandler) new ApplyStatAuraPercentEffectHandler(cast, effect);
      SpellEffectCreators[132] = (cast, effect) =>
        (SpellEffectHandler) new PlayMusicEffectHandler(cast, effect);
      SpellEffectCreators[133] = (cast, effect) =>
        (SpellEffectHandler) new ForgetSpecializationEffectHandler(cast, effect);
      SpellEffectCreators[136] = (cast, effect) =>
        (SpellEffectHandler) new RestoreHealthPercentEffectHandler(cast, effect);
      SpellEffectCreators[137] = (cast, effect) =>
        (SpellEffectHandler) new RestoreManaPercentEffectHandler(cast, effect);
      SpellEffectCreators[139] = (cast, effect) =>
        (SpellEffectHandler) new ClearQuestEffectHandler(cast, effect);
      SpellEffectCreators[140] = (cast, effect) =>
        (SpellEffectHandler) new TriggerSpellFromTargetWithCasterAsTargetHandler(cast, effect);
      SpellEffectCreators[143] = (cast, effect) =>
        (SpellEffectHandler) new ApplyAuraToMasterEffectHandler(cast, effect);
      SpellEffectCreators[151] = (cast, effect) =>
        (SpellEffectHandler) new TriggerRitualOfSummoningEffectHandler(cast, effect);
      SpellEffectCreators[154] = (cast, effect) =>
        (SpellEffectHandler) new TeachFlightPathEffectHandler(cast, effect);
      SpellEffectCreators[101] = (cast, effect) =>
        (SpellEffectHandler) new FeedPetEffectHandler(cast, effect);
      SpellEffectCreators[104] = (cast, effect) =>
        (SpellEffectHandler) new SummonObjectSlot1Handler(cast, effect);
      SpellEffectCreators[105] = (cast, effect) =>
        (SpellEffectHandler) new SummonObjectSlot2Handler(cast, effect);
      SpellEffectCreators[106] = (cast, effect) =>
        (SpellEffectHandler) new SummonObjectSlot1Handler(cast, effect);
      SpellEffectCreators[107] = (cast, effect) =>
        (SpellEffectHandler) new SummonObjectSlot2Handler(cast, effect);
      SpellEffectCreators[110] = (cast, effect) =>
        (SpellEffectHandler) new DestroyAllTotemsHandler(cast, effect);
      SpellEffectCreators[161] = (cast, effect) =>
        (SpellEffectHandler) new SetNumberOfTalentGroupsHandler(cast, effect);
      SpellEffectCreators[162] = (cast, effect) =>
        (SpellEffectHandler) new ActivateTalentGroupHandler(cast, effect);
      SpellEffectCreators[200] = (cast, effect) =>
        (SpellEffectHandler) new DamageFromPrcAtackHandler(cast, effect);
      SpellEffectCreators[201] = (cast, effect) =>
        (SpellEffectHandler) new CastAnotherSpellHandler(cast, effect);
      SpellEffectCreators[202] = (cast, effect) =>
        (SpellEffectHandler) new PortalTeleportEffectHandler(cast, effect);
      SpellEffectCreators[203] = (cast, effect) =>
        (SpellEffectHandler) new BossSummonHelpSpellHandler(cast, effect);
      for(int index = 0; index < SpellEffectCreators.Length; ++index)
      {
        if(SpellEffectCreators[index] == null)
          SpellEffectCreators[index] = (cast, effect) =>
            (SpellEffectHandler) new NotImplementedEffectHandler(cast, effect);
      }

      UnsetHandler(SpellEffectType.None);
      UnsetHandler(SpellEffectType.Dodge);
      UnsetHandler(SpellEffectType.Defense);
      UnsetHandler(SpellEffectType.SpellDefense);
      UnsetHandler(SpellEffectType.Block);
      UnsetHandler(SpellEffectType.Detect);
      UnsetHandler(SpellEffectType.Dummy);
      UnsetHandler(SpellEffectType.Parry);
    }

    public static void UnsetHandler(SpellEffectType type)
    {
      SpellEffectCreators[(int) type] = null;
    }

    private static void InitSummonHandlers()
    {
      foreach(SpellSummonEntry spellSummonEntry in SummonEntries.Values)
      {
        if(spellSummonEntry.Id == SummonType.Totem)
          spellSummonEntry.Type = SummonPropertyType.Totem;
        if(spellSummonEntry.Type == SummonPropertyType.Totem)
        {
          spellSummonEntry.Handler =
            new SpellSummonTotemHandler(
              MathUtil.ClampMinMax(spellSummonEntry.Slot - 1U, 0U, 3U));
          spellSummonEntry.DetermineAmountBySpellEffect = false;
        }
        else
        {
          switch(spellSummonEntry.Group)
          {
            case SummonGroup.Wild:
              spellSummonEntry.Handler = DefaultSummonHandler;
              continue;
            case SummonGroup.Friendly:
              spellSummonEntry.Handler = DefaultSummonHandler;
              continue;
            case SummonGroup.Pets:
              spellSummonEntry.Handler = PetSummonHandler;
              continue;
            case SummonGroup.Controllable:
              spellSummonEntry.Handler = PossesedSummonHandler;
              continue;
            default:
              spellSummonEntry.Handler = DefaultSummonHandler;
              continue;
          }
        }
      }

      SummonEntries[SummonType.Critter].Handler = DefaultSummonHandler;
      SummonEntries[SummonType.Critter2].Handler = DefaultSummonHandler;
      SummonEntries[SummonType.Critter3].Handler = DefaultSummonHandler;
      SummonEntries[SummonType.Demon].Handler = DefaultSummonHandler;
      SummonEntries[SummonType.DoomGuard].Handler =
        new SpellSummonDoomguardHandler();
    }

    public static SpellSummonEntry GetSummonEntry(SummonType type)
    {
      SpellSummonEntry spellSummonEntry;
      if(SummonEntries.TryGetValue(type, out spellSummonEntry))
        return spellSummonEntry;
      log.Warn("Missing SpellSummonEntry for type: " + type);
      return SummonEntries[SummonType.SummonPet];
    }

    public static ShapeshiftEntry GetShapeshiftEntry(ShapeshiftForm form)
    {
      return ShapeshiftEntries[(int) form];
    }

    public static ClassId ToClassId(this SpellClassSet classSet)
    {
      switch(classSet)
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