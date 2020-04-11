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
    public readonly object CustomData = new ExpandoObject();
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
      return m_updateValues[field.RawId].Float;
    }

    public short GetInt16Low(UpdateFieldId field)
    {
      return m_updateValues[field.RawId].Int16Low;
    }

    public short GetInt16Low(int field)
    {
      return m_updateValues[field].Int16Low;
    }

    public short GetInt16High(UpdateFieldId field)
    {
      return m_updateValues[field.RawId].Int16High;
    }

    public short GetInt16High(int field)
    {
      return m_updateValues[field].Int16High;
    }

    public ushort GetUInt16Low(UpdateFieldId field)
    {
      return m_updateValues[field.RawId].UInt16Low;
    }

    public ushort GetUInt16Low(int field)
    {
      return m_updateValues[field].UInt16Low;
    }

    public ushort GetUInt16High(UpdateFieldId field)
    {
      return m_updateValues[field.RawId].UInt16High;
    }

    public ushort GetUInt16High(int field)
    {
      return m_updateValues[field].UInt16High;
    }

    public int GetInt32(int field)
    {
      return m_updateValues[field].Int32;
    }

    public int GetInt32(ObjectFields field)
    {
      return m_updateValues[(int) field].Int32;
    }

    public int GetInt32(UnitFields field)
    {
      return m_updateValues[(int) field].Int32;
    }

    public int GetInt32(PlayerFields field)
    {
      return m_updateValues[(int) field].Int32;
    }

    public int GetInt32(ItemFields field)
    {
      return m_updateValues[(int) field].Int32;
    }

    public int GetInt32(ContainerFields field)
    {
      return m_updateValues[(int) field].Int32;
    }

    public int GetInt32(GameObjectFields field)
    {
      return m_updateValues[(int) field].Int32;
    }

    public int GetInt32(CorpseFields field)
    {
      return m_updateValues[(int) field].Int32;
    }

    public int GetInt32(DynamicObjectFields field)
    {
      return m_updateValues[(int) field].Int32;
    }

    public uint GetUInt32(int field)
    {
      return m_updateValues[field].UInt32;
    }

    public uint GetUInt32(ObjectFields field)
    {
      return m_updateValues[(int) field].UInt32;
    }

    public uint GetUInt32(UnitFields field)
    {
      return m_updateValues[(int) field].UInt32;
    }

    public uint GetUInt32(PlayerFields field)
    {
      return m_updateValues[(int) field].UInt32;
    }

    public uint GetUInt32(GameObjectFields field)
    {
      return m_updateValues[(int) field].UInt32;
    }

    public uint GetUInt32(ItemFields field)
    {
      return m_updateValues[(int) field].UInt32;
    }

    public uint GetUInt32(ContainerFields field)
    {
      return m_updateValues[(int) field].UInt32;
    }

    public uint GetUInt32(DynamicObjectFields field)
    {
      return m_updateValues[(int) field].UInt32;
    }

    public uint GetUInt32(CorpseFields field)
    {
      return m_updateValues[(int) field].UInt32;
    }

    public ulong GetUInt64(int field)
    {
      return m_updateValues[field].UInt32 | (ulong) m_updateValues[field + 1].UInt32 << 32;
    }

    public ulong GetUInt64(UpdateFieldId field)
    {
      return m_updateValues[field.RawId].UInt32 |
             (ulong) m_updateValues[field.RawId + 1].UInt32 << 32;
    }

    public EntityId GetEntityId(UpdateFieldId field)
    {
      return GetEntityId(field.RawId);
    }

    public EntityId GetEntityId(int field)
    {
      return new EntityId(m_updateValues[field].UInt32, m_updateValues[field + 1].UInt32);
    }

    public byte[] GetByteArray(UpdateFieldId field)
    {
      return m_updateValues[field.RawId].ByteArray;
    }

    public byte GetByte(int field, int index)
    {
      return m_updateValues[field].GetByte(index);
    }

    public byte GetByte(UpdateFieldId field, int index)
    {
      return m_updateValues[field.RawId].GetByte(index);
    }

    public void SetFloat(UpdateFieldId field, float value)
    {
      SetFloat(field.RawId, value);
    }

    public void SetFloat(int field, float value)
    {
      if(m_updateValues[field].Float == (double) value)
        return;
      m_updateValues[field].Float = value;
      MarkUpdate(field);
    }

    public void SetInt16Low(UpdateFieldId field, short value)
    {
      SetInt16Low(field.RawId, value);
    }

    public void SetInt16Low(int field, short value)
    {
      if(m_updateValues[field].Int16Low == value)
        return;
      m_updateValues[field].Int16Low = value;
      MarkUpdate(field);
    }

    public void SetInt16High(UpdateFieldId field, short value)
    {
      SetInt16High(field.RawId, value);
    }

    public void SetInt16High(int field, short value)
    {
      if(m_updateValues[field].Int16High == value)
        return;
      m_updateValues[field].Int16High = value;
      MarkUpdate(field);
    }

    public void SetUInt16Low(UpdateFieldId field, ushort value)
    {
      SetUInt16Low(field.RawId, value);
    }

    public void SetUInt16Low(int field, ushort value)
    {
      if(m_updateValues[field].UInt16Low == value)
        return;
      m_updateValues[field].UInt16Low = value;
      MarkUpdate(field);
    }

    public void SetUInt16High(UpdateFieldId field, ushort value)
    {
      SetUInt16High(field.RawId, value);
    }

    public void SetUInt16High(int field, ushort value)
    {
      if(m_updateValues[field].UInt16High == value)
        return;
      m_updateValues[field].UInt16High = value;
      MarkUpdate(field);
    }

    public void SetInt32(UpdateFieldId field, int value)
    {
      SetInt32(field.RawId, value);
    }

    public void SetInt32(int field, int value)
    {
      if(m_updateValues[field].Int32 == value)
        return;
      m_updateValues[field].Int32 = value;
      MarkUpdate(field);
    }

    public void SetUInt32(UpdateFieldId field, uint value)
    {
      SetUInt32(field.RawId, value);
    }

    public void SetUInt32(int field, uint value)
    {
      if((int) m_updateValues[field].UInt32 == (int) value)
        return;
      m_updateValues[field].UInt32 = value;
      MarkUpdate(field);
    }

    public void SetInt64(int field, long value)
    {
      SetInt32(field, (int) (value & uint.MaxValue));
      SetInt32(field + 1, (int) (value >> 32));
    }

    public void SetInt64(UpdateFieldId field, long value)
    {
      SetInt64(field.RawId, value);
    }

    public void SetUInt64(UpdateFieldId field, ulong value)
    {
      SetUInt64(field.RawId, value);
    }

    public void SetUInt64(int field, ulong value)
    {
      SetUInt32(field, (uint) (value & uint.MaxValue));
      SetUInt32(field + 1, (uint) (value >> 32));
    }

    public void SetEntityId(UpdateFieldId field, EntityId id)
    {
      SetEntityId(field.RawId, id);
    }

    public void SetEntityId(int field, EntityId id)
    {
      SetUInt64(field, id.Full);
    }

    public void SetByteArray(UpdateFieldId field, byte[] value)
    {
      SetByteArray(field.RawId, value);
    }

    public unsafe void SetByteArray(int field, byte[] value)
    {
      if(value.Length != 4)
      {
        LogUtil.ErrorException(new Exception("Invalid length"),
          "Tried to set a byte array with invalid length: ");
      }
      else
      {
        fixed(byte* numPtr = value)
          SetUInt32(field, *(uint*) numPtr);
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
      SetByte(field.RawId, index, value);
    }

    /// <summary>
    /// Sets a specified byte of an updatefield to the specified value
    /// </summary>
    /// <param name="field">The field to set</param>
    /// <param name="value">The value to set</param>
    /// <param name="index">The index of the byte in the 4-byte field. (Ranges from 0-3)</param>
    public void SetByte(int field, int index, byte value)
    {
      if(m_updateValues[field].GetByte(index) == value)
        return;
      m_updateValues[field].SetByte(index, value);
      MarkUpdate(field);
    }

    /// <summary>
    /// Is called whenever a field has changed.
    /// Adds the given index to the corresponding UpdateMasks.
    /// </summary>
    protected internal void MarkUpdate(int index)
    {
      m_privateUpdateMask.SetBit(index);
      if(index > m_highestUsedUpdateIndex)
        m_highestUsedUpdateIndex = index;
      if(m_publicUpdateMask != m_privateUpdateMask && IsUpdateFieldPublic(index))
        m_publicUpdateMask.SetBit(index);
      if(m_requiresUpdate || !IsInWorld)
        return;
      RequestUpdate();
    }

    /// <summary>
    /// Marks the given UpdateField for an Update.
    /// Marked UpdateFields will be re-sent to all surrounding Characters.
    /// </summary>
    protected internal void MarkUpdate(UpdateFieldId index)
    {
      MarkUpdate(index.RawId);
    }

    /// <summary>The entity ID of the object</summary>
    public EntityId EntityId
    {
      get { return GetEntityId(ObjectFields.GUID); }
      protected internal set { SetEntityId(ObjectFields.GUID, value); }
    }

    public ObjectTypes Type
    {
      get { return (ObjectTypes) GetUInt32(ObjectFields.TYPE); }
      protected set { SetUInt32(ObjectFields.TYPE, (uint) value); }
    }

    public uint EntryId
    {
      get
      {
        if(this is Character)
          return EntityId.Low;
        return GetUInt32(ObjectFields.ENTRY);
      }
      protected set { SetUInt32(ObjectFields.ENTRY, value); }
    }

    public float ScaleX
    {
      get { return GetFloat(ObjectFields.SCALE_X); }
      set
      {
        SetFloat(ObjectFields.SCALE_X, value);
        if(!(this is Unit) || ((Unit) this).Model == null)
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
      get { return m_loot; }
      set { m_loot = value; }
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
      get { return m_requiresUpdate; }
    }

    public abstract UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers { get; }

    internal void ResetUpdateInfo()
    {
      m_privateUpdateMask.Clear();
      if(m_privateUpdateMask != m_publicUpdateMask)
        m_publicUpdateMask.Clear();
      m_requiresUpdate = false;
    }

    /// <summary>Whether the given field is public</summary>
    /// <param name="fieldIndex"></param>
    /// <returns></returns>
    public bool IsUpdateFieldPublic(int fieldIndex)
    {
      return _UpdateFieldInfos.FieldFlags[fieldIndex]
        .HasAnyFlag(UpdateFieldFlags.Public | UpdateFieldFlags.Dynamic);
    }

    /// <summary>Whether this Object has any private Update fields</summary>
    /// <returns></returns>
    private bool HasPrivateUpdateFields
    {
      get { return _UpdateFieldInfos.HasPrivateFields; }
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
      writer.Write(EntityId.LowRaw);
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
      if(_UpdateFieldInfos.FieldFlags[index].HasAnyFlag(UpdateFieldFlags.Dynamic))
        DynamicUpdateFieldHandlers[index](this, receiver, packet);
      else
        packet.Write(m_updateValues[index].UInt32);
    }

    public void SendSpontaneousUpdate(Character receiver, params UpdateFieldId[] indices)
    {
      SendSpontaneousUpdate(receiver, true, indices);
    }

    public void SendSpontaneousUpdate(Character receiver, bool visible, params UpdateFieldId[] indices)
    {
    }

    protected void WriteSpontaneousUpdate(UpdateMask mask, UpdatePacket packet, Character receiver,
      UpdateFieldId[] indices, bool visible)
    {
      for(int index1 = 0; index1 < indices.Length; ++index1)
      {
        int rawId = indices[index1].RawId;
        UpdateField field = UpdateFieldMgr.Get(ObjectTypeId).Fields[rawId];
        for(int index2 = 0; (long) index2 < (long) field.Size; ++index2)
          mask.SetBit(rawId + index2);
      }

      mask.WriteTo(packet);
      for(int lowestIndex = mask.m_lowestIndex; lowestIndex <= mask.m_highestIndex; ++lowestIndex)
      {
        if(mask.GetBit(lowestIndex))
        {
          if(visible)
            WriteUpdateValue(packet, receiver, lowestIndex);
          else
            packet.Write(0);
        }
      }
    }

    public RealmPacketOut CreateDestroyPacket()
    {
      RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.SMSG_DESTROY_OBJECT, 9);
      realmPacketOut.Write(EntityId);
      realmPacketOut.Write((byte) 0);
      return realmPacketOut;
    }

    public void SendDestroyToPlayer(Character c)
    {
      using(RealmPacketOut destroyPacket = CreateDestroyPacket())
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
      EntityId.WritePacked(updatePacket2);
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
      EntityId.WritePacked(updatePacket2);
      updatePacket2.Write((byte) num);
      updatePacket2.Zero(len);
      updatePacket2.Write(1 << field.RawId);
      updatePacket2.Write(value);
      return updatePacket2;
    }

    public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, int value)
    {
      using(UpdatePacket fieldUpdatePacket = GetFieldUpdatePacket(field, (uint) value))
        SendUpdatePacket(character, fieldUpdatePacket);
    }

    public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, uint value)
    {
      using(UpdatePacket fieldUpdatePacket = GetFieldUpdatePacket(field, value))
        SendUpdatePacket(character, fieldUpdatePacket);
    }

    public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, byte[] value)
    {
      using(UpdatePacket fieldUpdatePacket = GetFieldUpdatePacket(field, value))
        SendUpdatePacket(character, fieldUpdatePacket);
    }

    protected static void SendUpdatePacket(Character character, UpdatePacket packet)
    {
      packet.SendTo(character.Client);
    }

    protected ObjectBase()
    {
      int length = _UpdateFieldInfos.Fields.Length;
      m_privateUpdateMask = new UpdateMask(length);
      m_publicUpdateMask = !HasPrivateUpdateFields ? m_privateUpdateMask : new UpdateMask(length);
      m_updateValues = new CompoundType[length];
      Type = ObjectTypes.Object;
      SetFloat(ObjectFields.SCALE_X, 1f);
    }

    protected abstract UpdateFieldCollection _UpdateFieldInfos { get; }

    public abstract UpdateFlags UpdateFlags { get; }

    /// <summary>The type of this object (player, corpse, item, etc)</summary>
    public abstract ObjectTypeId ObjectTypeId { get; }

    public CompoundType[] UpdateValues
    {
      get { return m_updateValues; }
    }

    public UpdateMask UpdateMask
    {
      get { return m_privateUpdateMask; }
    }

    public abstract bool IsInWorld { get; }

    public abstract void Dispose(bool disposing);

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      Dispose(true);
    }

    public bool CheckObjType(ObjectTypes type)
    {
      if(type != ObjectTypes.None)
        return Type.HasAnyFlag(type);
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
      return EntityId.GetHashCode();
    }

    public override string ToString()
    {
      return GetType().Name + " (ID: " + EntityId + ")";
    }
  }
}