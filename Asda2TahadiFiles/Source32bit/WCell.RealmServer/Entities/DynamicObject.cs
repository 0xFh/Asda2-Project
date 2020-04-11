using System;
using WCell.Constants.Factions;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spells;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
  /// <summary>
  /// This -contrary to what its name suggests- is a static animation or decoration in the world
  /// </summary>
  public class DynamicObject : WorldObject
  {
    public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.DynamicObject);
    internal static uint lastId;

    protected override UpdateFieldCollection _UpdateFieldInfos
    {
      get { return UpdateFieldInfos; }
    }

    public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
    {
      get { return UpdateFieldHandler.DynamicDOFieldHandlers; }
    }

    internal DynamicObject()
    {
    }

    public DynamicObject(SpellCast cast, float radius)
      : this(cast.CasterUnit, cast.Spell.SpellId, radius, cast.Map, cast.TargetLoc)
    {
    }

    public DynamicObject(Unit creator, SpellId spellId, float radius, Map map, Vector3 pos)
    {
      if(creator == null)
        throw new ArgumentNullException(nameof(creator), "creator must not be null");
      Master = creator;
      EntityId = EntityId.GetDynamicObjectId(++lastId);
      Type |= ObjectTypes.DynamicObject;
      SetEntityId(DynamicObjectFields.CASTER, creator.EntityId);
      SpellId = spellId;
      Radius = radius;
      Bytes = 32435950U;
      ScaleX = 1f;
      m_position = pos;
      map.AddObjectLater(this);
    }

    public override int CasterLevel
    {
      get { return m_master.Level; }
    }

    public override string Name
    {
      get { return m_master + "'s " + SpellId + " - Object"; }
      set { }
    }

    public override Faction Faction
    {
      get { return m_master.Faction; }
      set { }
    }

    public override FactionId FactionId
    {
      get
      {
        if(m_master.Faction == null)
          return FactionId.None;
        return m_master.Faction.Id;
      }
      set { }
    }

    public override ObjectTypeId ObjectTypeId
    {
      get { return ObjectTypeId.DynamicObject; }
    }

    public override UpdateFlags UpdateFlags
    {
      get { return UpdateFlags.Flag_0x8 | UpdateFlags.Flag_0x10 | UpdateFlags.StationaryObject; }
    }

    protected override void WriteMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation)
    {
      writer.Write(Position);
      writer.WriteFloat(Orientation);
    }

    public override string ToString()
    {
      return GetType() + " " + base.ToString();
    }

    public SpellId SpellId
    {
      get { return (SpellId) GetUInt32(DynamicObjectFields.SPELLID); }
      internal set { SetUInt32(DynamicObjectFields.SPELLID, (uint) value); }
    }

    protected internal uint Bytes
    {
      get { return GetUInt32(DynamicObjectFields.BYTES); }
      internal set { SetUInt32(DynamicObjectFields.BYTES, value); }
    }

    protected internal float Radius
    {
      get { return GetFloat(DynamicObjectFields.RADIUS); }
      internal set { SetFloat(DynamicObjectFields.RADIUS, value); }
    }

    public uint CastTime
    {
      get { return GetUInt32(DynamicObjectFields.CASTTIME); }
      set { SetUInt32(DynamicObjectFields.CASTTIME, value); }
    }

    public override ObjectTypeCustom CustomType
    {
      get { return ObjectTypeCustom.Object | ObjectTypeCustom.DynamicObject; }
    }
  }
}