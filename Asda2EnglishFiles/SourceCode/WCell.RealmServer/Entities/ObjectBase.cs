using NLog;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using WCell.Constants;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.UpdateFields;
using WCell.Util.NLog;

namespace WCell.RealmServer.Entities
{
    /// <summary>The base class for all in-game Objects</summary>
    public abstract class ObjectBase : IDisposable, IAsda2Lootable, IEntity
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public readonly object CustomData = (object) new ExpandoObject();
        protected Asda2Loot m_loot;
        protected internal bool m_requiresUpdate;

        /// <summary>
        /// This is a reference to <see cref="F:WCell.RealmServer.Entities.ObjectBase.m_privateUpdateMask" /> if there are no private values in this object
        /// else its handled seperately
        /// </summary>
        protected internal UpdateMask m_publicUpdateMask;

        protected UpdateMask m_privateUpdateMask;
        protected internal int m_highestUsedUpdateIndex;
        protected readonly CompoundType[] m_updateValues;

        public float GetFloat(UpdateFieldId field)
        {
            return this.m_updateValues[field.RawId].Float;
        }

        public short GetInt16Low(UpdateFieldId field)
        {
            return this.m_updateValues[field.RawId].Int16Low;
        }

        public short GetInt16Low(int field)
        {
            return this.m_updateValues[field].Int16Low;
        }

        public short GetInt16High(UpdateFieldId field)
        {
            return this.m_updateValues[field.RawId].Int16High;
        }

        public short GetInt16High(int field)
        {
            return this.m_updateValues[field].Int16High;
        }

        public ushort GetUInt16Low(UpdateFieldId field)
        {
            return this.m_updateValues[field.RawId].UInt16Low;
        }

        public ushort GetUInt16Low(int field)
        {
            return this.m_updateValues[field].UInt16Low;
        }

        public ushort GetUInt16High(UpdateFieldId field)
        {
            return this.m_updateValues[field.RawId].UInt16High;
        }

        public ushort GetUInt16High(int field)
        {
            return this.m_updateValues[field].UInt16High;
        }

        public int GetInt32(int field)
        {
            return this.m_updateValues[field].Int32;
        }

        public int GetInt32(ObjectFields field)
        {
            return this.m_updateValues[(int) field].Int32;
        }

        public int GetInt32(UnitFields field)
        {
            return this.m_updateValues[(int) field].Int32;
        }

        public int GetInt32(PlayerFields field)
        {
            return this.m_updateValues[(int) field].Int32;
        }

        public int GetInt32(ItemFields field)
        {
            return this.m_updateValues[(int) field].Int32;
        }

        public int GetInt32(ContainerFields field)
        {
            return this.m_updateValues[(int) field].Int32;
        }

        public int GetInt32(GameObjectFields field)
        {
            return this.m_updateValues[(int) field].Int32;
        }

        public int GetInt32(CorpseFields field)
        {
            return this.m_updateValues[(int) field].Int32;
        }

        public int GetInt32(DynamicObjectFields field)
        {
            return this.m_updateValues[(int) field].Int32;
        }

        public uint GetUInt32(int field)
        {
            return this.m_updateValues[field].UInt32;
        }

        public uint GetUInt32(ObjectFields field)
        {
            return this.m_updateValues[(int) field].UInt32;
        }

        public uint GetUInt32(UnitFields field)
        {
            return this.m_updateValues[(int) field].UInt32;
        }

        public uint GetUInt32(PlayerFields field)
        {
            return this.m_updateValues[(int) field].UInt32;
        }

        public uint GetUInt32(GameObjectFields field)
        {
            return this.m_updateValues[(int) field].UInt32;
        }

        public uint GetUInt32(ItemFields field)
        {
            return this.m_updateValues[(int) field].UInt32;
        }

        public uint GetUInt32(ContainerFields field)
        {
            return this.m_updateValues[(int) field].UInt32;
        }

        public uint GetUInt32(DynamicObjectFields field)
        {
            return this.m_updateValues[(int) field].UInt32;
        }

        public uint GetUInt32(CorpseFields field)
        {
            return this.m_updateValues[(int) field].UInt32;
        }

        public ulong GetUInt64(int field)
        {
            return (ulong) this.m_updateValues[field].UInt32 | (ulong) this.m_updateValues[field + 1].UInt32 << 32;
        }

        public ulong GetUInt64(UpdateFieldId field)
        {
            return (ulong) this.m_updateValues[field.RawId].UInt32 |
                   (ulong) this.m_updateValues[field.RawId + 1].UInt32 << 32;
        }

        public EntityId GetEntityId(UpdateFieldId field)
        {
            return this.GetEntityId(field.RawId);
        }

        public EntityId GetEntityId(int field)
        {
            return new EntityId(this.m_updateValues[field].UInt32, this.m_updateValues[field + 1].UInt32);
        }

        public byte[] GetByteArray(UpdateFieldId field)
        {
            return this.m_updateValues[field.RawId].ByteArray;
        }

        public byte GetByte(int field, int index)
        {
            return this.m_updateValues[field].GetByte(index);
        }

        public byte GetByte(UpdateFieldId field, int index)
        {
            return this.m_updateValues[field.RawId].GetByte(index);
        }

        public void SetFloat(UpdateFieldId field, float value)
        {
            this.SetFloat(field.RawId, value);
        }

        public void SetFloat(int field, float value)
        {
            if ((double) this.m_updateValues[field].Float == (double) value)
                return;
            this.m_updateValues[field].Float = value;
            this.MarkUpdate(field);
        }

        public void SetInt16Low(UpdateFieldId field, short value)
        {
            this.SetInt16Low(field.RawId, value);
        }

        public void SetInt16Low(int field, short value)
        {
            if ((int) this.m_updateValues[field].Int16Low == (int) value)
                return;
            this.m_updateValues[field].Int16Low = value;
            this.MarkUpdate(field);
        }

        public void SetInt16High(UpdateFieldId field, short value)
        {
            this.SetInt16High(field.RawId, value);
        }

        public void SetInt16High(int field, short value)
        {
            if ((int) this.m_updateValues[field].Int16High == (int) value)
                return;
            this.m_updateValues[field].Int16High = value;
            this.MarkUpdate(field);
        }

        public void SetUInt16Low(UpdateFieldId field, ushort value)
        {
            this.SetUInt16Low(field.RawId, value);
        }

        public void SetUInt16Low(int field, ushort value)
        {
            if ((int) this.m_updateValues[field].UInt16Low == (int) value)
                return;
            this.m_updateValues[field].UInt16Low = value;
            this.MarkUpdate(field);
        }

        public void SetUInt16High(UpdateFieldId field, ushort value)
        {
            this.SetUInt16High(field.RawId, value);
        }

        public void SetUInt16High(int field, ushort value)
        {
            if ((int) this.m_updateValues[field].UInt16High == (int) value)
                return;
            this.m_updateValues[field].UInt16High = value;
            this.MarkUpdate(field);
        }

        public void SetInt32(UpdateFieldId field, int value)
        {
            this.SetInt32(field.RawId, value);
        }

        public void SetInt32(int field, int value)
        {
            if (this.m_updateValues[field].Int32 == value)
                return;
            this.m_updateValues[field].Int32 = value;
            this.MarkUpdate(field);
        }

        public void SetUInt32(UpdateFieldId field, uint value)
        {
            this.SetUInt32(field.RawId, value);
        }

        public void SetUInt32(int field, uint value)
        {
            if ((int) this.m_updateValues[field].UInt32 == (int) value)
                return;
            this.m_updateValues[field].UInt32 = value;
            this.MarkUpdate(field);
        }

        public void SetInt64(int field, long value)
        {
            this.SetInt32(field, (int) (value & (long) uint.MaxValue));
            this.SetInt32(field + 1, (int) (value >> 32));
        }

        public void SetInt64(UpdateFieldId field, long value)
        {
            this.SetInt64(field.RawId, value);
        }

        public void SetUInt64(UpdateFieldId field, ulong value)
        {
            this.SetUInt64(field.RawId, value);
        }

        public void SetUInt64(int field, ulong value)
        {
            this.SetUInt32(field, (uint) (value & (ulong) uint.MaxValue));
            this.SetUInt32(field + 1, (uint) (value >> 32));
        }

        public void SetEntityId(UpdateFieldId field, EntityId id)
        {
            this.SetEntityId(field.RawId, id);
        }

        public void SetEntityId(int field, EntityId id)
        {
            this.SetUInt64(field, id.Full);
        }

        public void SetByteArray(UpdateFieldId field, byte[] value)
        {
            this.SetByteArray(field.RawId, value);
        }

        public unsafe void SetByteArray(int field, byte[] value)
        {
            if (value.Length != 4)
            {
                LogUtil.ErrorException(new Exception("Invalid length"),
                    "Tried to set a byte array with invalid length: ", new object[0]);
            }
            else
            {
                fixed (byte* numPtr = value)
                    this.SetUInt32(field, *(uint*) numPtr);
            }
        }

        /// <summary>
        /// Sets a specified byte of an updatefield to the specified value
        /// </summary>
        /// <param name="field">The field to set</param>
        /// <param name="index">The index of the byte in the 4-byte field. (Ranges from 0-3)</param>
        /// <param name="value">The value to set</param>
        public void SetByte(UpdateFieldId field, int index, byte value)
        {
            this.SetByte(field.RawId, index, value);
        }

        /// <summary>
        /// Sets a specified byte of an updatefield to the specified value
        /// </summary>
        /// <param name="field">The field to set</param>
        /// <param name="value">The value to set</param>
        /// <param name="index">The index of the byte in the 4-byte field. (Ranges from 0-3)</param>
        public void SetByte(int field, int index, byte value)
        {
            if ((int) this.m_updateValues[field].GetByte(index) == (int) value)
                return;
            this.m_updateValues[field].SetByte(index, value);
            this.MarkUpdate(field);
        }

        /// <summary>
        /// Is called whenever a field has changed.
        /// Adds the given index to the corresponding UpdateMasks.
        /// </summary>
        protected internal void MarkUpdate(int index)
        {
            this.m_privateUpdateMask.SetBit(index);
            if (index > this.m_highestUsedUpdateIndex)
                this.m_highestUsedUpdateIndex = index;
            if (this.m_publicUpdateMask != this.m_privateUpdateMask && this.IsUpdateFieldPublic(index))
                this.m_publicUpdateMask.SetBit(index);
            if (this.m_requiresUpdate || !this.IsInWorld)
                return;
            this.RequestUpdate();
        }

        /// <summary>
        /// Marks the given UpdateField for an Update.
        /// Marked UpdateFields will be re-sent to all surrounding Characters.
        /// </summary>
        protected internal void MarkUpdate(UpdateFieldId index)
        {
            this.MarkUpdate(index.RawId);
        }

        /// <summary>The entity ID of the object</summary>
        public EntityId EntityId
        {
            get { return this.GetEntityId((UpdateFieldId) ObjectFields.GUID); }
            protected internal set { this.SetEntityId((UpdateFieldId) ObjectFields.GUID, value); }
        }

        public ObjectTypes Type
        {
            get { return (ObjectTypes) this.GetUInt32(ObjectFields.TYPE); }
            protected set { this.SetUInt32((UpdateFieldId) ObjectFields.TYPE, (uint) value); }
        }

        public uint EntryId
        {
            get
            {
                if (this is Character)
                    return this.EntityId.Low;
                return this.GetUInt32(ObjectFields.ENTRY);
            }
            protected set { this.SetUInt32((UpdateFieldId) ObjectFields.ENTRY, value); }
        }

        public float ScaleX
        {
            get { return this.GetFloat((UpdateFieldId) ObjectFields.SCALE_X); }
            set
            {
                this.SetFloat((UpdateFieldId) ObjectFields.SCALE_X, value);
                if (!(this is Unit) || ((Unit) this).Model == null)
                    return;
                ((Unit) this).UpdateModel();
            }
        }

        public virtual ObjectTypeCustom CustomType
        {
            get { return ObjectTypeCustom.Object; }
        }

        /// <summary>
        /// The current loot that can be looted of this object (if loot has been generated yet)
        /// </summary>
        public Asda2Loot Loot
        {
            get { return this.m_loot; }
            set { this.m_loot = value; }
        }

        public virtual uint GetLootId(Asda2LootEntryType type)
        {
            return 0;
        }

        public virtual uint LootMoney
        {
            get { return 0; }
        }

        protected abstract UpdateType GetCreationUpdateType(UpdateFieldFlags relation);

        public bool RequiresUpdate
        {
            get { return this.m_requiresUpdate; }
        }

        public abstract UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers { get; }

        internal void ResetUpdateInfo()
        {
            this.m_privateUpdateMask.Clear();
            if (this.m_privateUpdateMask != this.m_publicUpdateMask)
                this.m_publicUpdateMask.Clear();
            this.m_requiresUpdate = false;
        }

        /// <summary>Whether the given field is public</summary>
        /// <param name="fieldIndex"></param>
        /// <returns></returns>
        public bool IsUpdateFieldPublic(int fieldIndex)
        {
            return this._UpdateFieldInfos.FieldFlags[fieldIndex]
                .HasAnyFlag(UpdateFieldFlags.Public | UpdateFieldFlags.Dynamic);
        }

        /// <summary>Whether this Object has any private Update fields</summary>
        /// <returns></returns>
        private bool HasPrivateUpdateFields
        {
            get { return this._UpdateFieldInfos.HasPrivateFields; }
        }

        public virtual UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
        {
            return UpdateFieldFlags.Public;
        }

        public abstract void RequestUpdate();

        protected internal virtual void WriteObjectCreationUpdate(Character chr)
        {
        }

        protected virtual void WriteUpdateFlag_0x8(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            writer.Write(this.EntityId.LowRaw);
        }

        protected virtual void WriteUpdateFlag_0x10(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            writer.Write(1);
        }

        protected virtual void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation,
            UpdateFlags updateFlags)
        {
        }

        /// <summary>
        /// Writes the major portion of the create block.
        /// This handles flags 0x20, 0x40, and 0x100, they are exclusive to each other
        /// The content depends on the object's type
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="relation"></param>
        protected virtual void WriteMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
        }

        protected void WriteUpdateValue(UpdatePacket packet, Character receiver, int index)
        {
            if (this._UpdateFieldInfos.FieldFlags[index].HasAnyFlag(UpdateFieldFlags.Dynamic))
                this.DynamicUpdateFieldHandlers[index](this, receiver, packet);
            else
                packet.Write(this.m_updateValues[index].UInt32);
        }

        public void SendSpontaneousUpdate(Character receiver, params UpdateFieldId[] indices)
        {
            this.SendSpontaneousUpdate(receiver, true, indices);
        }

        public void SendSpontaneousUpdate(Character receiver, bool visible, params UpdateFieldId[] indices)
        {
        }

        protected void WriteSpontaneousUpdate(UpdateMask mask, UpdatePacket packet, Character receiver,
            UpdateFieldId[] indices, bool visible)
        {
            for (int index1 = 0; index1 < indices.Length; ++index1)
            {
                int rawId = indices[index1].RawId;
                UpdateField field = UpdateFieldMgr.Get(this.ObjectTypeId).Fields[rawId];
                for (int index2 = 0; (long) index2 < (long) field.Size; ++index2)
                    mask.SetBit(rawId + index2);
            }

            mask.WriteTo((PrimitiveWriter) packet);
            for (int lowestIndex = mask.m_lowestIndex; lowestIndex <= mask.m_highestIndex; ++lowestIndex)
            {
                if (mask.GetBit(lowestIndex))
                {
                    if (visible)
                        this.WriteUpdateValue(packet, receiver, lowestIndex);
                    else
                        packet.Write(0);
                }
            }
        }

        public RealmPacketOut CreateDestroyPacket()
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_DESTROY_OBJECT, 9);
            realmPacketOut.Write((ulong) this.EntityId);
            realmPacketOut.Write((byte) 0);
            return realmPacketOut;
        }

        public void SendDestroyToPlayer(Character c)
        {
            using (RealmPacketOut destroyPacket = this.CreateDestroyPacket())
                c.Client.Send(destroyPacket, false);
        }

        public void SendOutOfRangeUpdate(Character receiver, HashSet<WorldObject> worldObjects)
        {
        }

        protected UpdatePacket GetFieldUpdatePacket(UpdateFieldId field, uint value)
        {
            int num = (field.RawId >> 5) + 1;
            int len = (num - 1) * 4;
            UpdatePacket updatePacket1 = new UpdatePacket();
            updatePacket1.Position = 4L;
            UpdatePacket updatePacket2 = updatePacket1;
            updatePacket2.Write(1);
            updatePacket2.Write((byte) 0);
            this.EntityId.WritePacked((BinaryWriter) updatePacket2);
            updatePacket2.Write((byte) num);
            updatePacket2.Zero(len);
            updatePacket2.Write(1 << field.RawId);
            updatePacket2.Write(value);
            return updatePacket2;
        }

        protected UpdatePacket GetFieldUpdatePacket(UpdateFieldId field, byte[] value)
        {
            int num = (field.RawId >> 5) + 1;
            int len = (num - 1) * 4;
            UpdatePacket updatePacket1 = new UpdatePacket();
            updatePacket1.Position = 4L;
            UpdatePacket updatePacket2 = updatePacket1;
            updatePacket2.Write(1);
            updatePacket2.Write((byte) 0);
            this.EntityId.WritePacked((BinaryWriter) updatePacket2);
            updatePacket2.Write((byte) num);
            updatePacket2.Zero(len);
            updatePacket2.Write(1 << field.RawId);
            updatePacket2.Write(value);
            return updatePacket2;
        }

        public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, int value)
        {
            using (UpdatePacket fieldUpdatePacket = this.GetFieldUpdatePacket(field, (uint) value))
                ObjectBase.SendUpdatePacket(character, fieldUpdatePacket);
        }

        public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, uint value)
        {
            using (UpdatePacket fieldUpdatePacket = this.GetFieldUpdatePacket(field, value))
                ObjectBase.SendUpdatePacket(character, fieldUpdatePacket);
        }

        public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, byte[] value)
        {
            using (UpdatePacket fieldUpdatePacket = this.GetFieldUpdatePacket(field, value))
                ObjectBase.SendUpdatePacket(character, fieldUpdatePacket);
        }

        protected static void SendUpdatePacket(Character character, UpdatePacket packet)
        {
            packet.SendTo(character.Client);
        }

        protected ObjectBase()
        {
            int length = this._UpdateFieldInfos.Fields.Length;
            this.m_privateUpdateMask = new UpdateMask(length);
            this.m_publicUpdateMask = !this.HasPrivateUpdateFields ? this.m_privateUpdateMask : new UpdateMask(length);
            this.m_updateValues = new CompoundType[length];
            this.Type = ObjectTypes.Object;
            this.SetFloat((UpdateFieldId) ObjectFields.SCALE_X, 1f);
        }

        protected abstract UpdateFieldCollection _UpdateFieldInfos { get; }

        public abstract UpdateFlags UpdateFlags { get; }

        /// <summary>The type of this object (player, corpse, item, etc)</summary>
        public abstract ObjectTypeId ObjectTypeId { get; }

        public CompoundType[] UpdateValues
        {
            get { return this.m_updateValues; }
        }

        public UpdateMask UpdateMask
        {
            get { return this.m_privateUpdateMask; }
        }

        public abstract bool IsInWorld { get; }

        public abstract void Dispose(bool disposing);

        public void Dispose()
        {
            GC.SuppressFinalize((object) this);
            this.Dispose(true);
        }

        public bool CheckObjType(ObjectTypes type)
        {
            if (type != ObjectTypes.None)
                return this.Type.HasAnyFlag(type);
            return true;
        }

        public virtual bool UseGroupLoot
        {
            get { return false; }
        }

        /// <summary>
        /// Called whenever everything has been looted off this object.
        /// </summary>
        public virtual void OnFinishedLooting()
        {
        }

        public virtual UpdatePriority UpdatePriority
        {
            get { return UpdatePriority.LowPriority; }
        }

        public override int GetHashCode()
        {
            return this.EntityId.GetHashCode();
        }

        public override string ToString()
        {
            return this.GetType().Name + " (ID: " + (object) this.EntityId + ")";
        }
    }
}