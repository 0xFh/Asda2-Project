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
            get { return DynamicObject.UpdateFieldInfos; }
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
            if (creator == null)
                throw new ArgumentNullException(nameof(creator), "creator must not be null");
            this.Master = creator;
            this.EntityId = EntityId.GetDynamicObjectId(++DynamicObject.lastId);
            this.Type |= ObjectTypes.DynamicObject;
            this.SetEntityId((UpdateFieldId) DynamicObjectFields.CASTER, creator.EntityId);
            this.SpellId = spellId;
            this.Radius = radius;
            this.Bytes = 32435950U;
            this.ScaleX = 1f;
            this.m_position = pos;
            map.AddObjectLater((WorldObject) this);
        }

        public override int CasterLevel
        {
            get { return this.m_master.Level; }
        }

        public override string Name
        {
            get { return this.m_master.ToString() + "'s " + (object) this.SpellId + " - Object"; }
            set { }
        }

        public override Faction Faction
        {
            get { return this.m_master.Faction; }
            set { }
        }

        public override FactionId FactionId
        {
            get
            {
                if (this.m_master.Faction == null)
                    return FactionId.None;
                return this.m_master.Faction.Id;
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
            writer.Write(this.Position);
            writer.WriteFloat(this.Orientation);
        }

        public override string ToString()
        {
            return this.GetType().ToString() + " " + base.ToString();
        }

        public SpellId SpellId
        {
            get { return (SpellId) this.GetUInt32(DynamicObjectFields.SPELLID); }
            internal set { this.SetUInt32((UpdateFieldId) DynamicObjectFields.SPELLID, (uint) value); }
        }

        protected internal uint Bytes
        {
            get { return this.GetUInt32(DynamicObjectFields.BYTES); }
            internal set { this.SetUInt32((UpdateFieldId) DynamicObjectFields.BYTES, value); }
        }

        protected internal float Radius
        {
            get { return this.GetFloat((UpdateFieldId) DynamicObjectFields.RADIUS); }
            internal set { this.SetFloat((UpdateFieldId) DynamicObjectFields.RADIUS, value); }
        }

        public uint CastTime
        {
            get { return this.GetUInt32(DynamicObjectFields.CASTTIME); }
            set { this.SetUInt32((UpdateFieldId) DynamicObjectFields.CASTTIME, value); }
        }

        public override ObjectTypeCustom CustomType
        {
            get { return ObjectTypeCustom.Object | ObjectTypeCustom.DynamicObject; }
        }
    }
}