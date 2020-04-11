using System;
using System.IO;
using System.Runtime.InteropServices;
using WCell.Constants.GameObjects;
using WCell.Constants.Updates;
using WCell.Core.Network;
using WCell.Util;

namespace WCell.Core
{
  [StructLayout(LayoutKind.Explicit, Size = 8)]
  public struct EntityId : IComparable, IEquatable<EntityId>, IConvertible
  {
    public static readonly EntityId Zero = new EntityId(0UL);
    private static readonly byte[] _zeroRaw = new byte[8];
    private const uint LowMask = 16777215;
    private const uint EntryMask = 16777215;
    private const uint HighMask = 4294901760;
    private const uint High7Mask = 16711680;
    private const uint High8Mask = 4278190080;
    [FieldOffset(0)]public ulong Full;
    [FieldOffset(0)]private uint m_low;
    [FieldOffset(3)]private uint m_entry;
    [FieldOffset(4)]private uint m_high;

    public static byte[] ZeroRaw
    {
      get { return _zeroRaw.Clone() as byte[]; }
    }

    public EntityId(byte[] fullRaw)
    {
      m_low = 0U;
      m_high = 0U;
      m_entry = 0U;
      Full = BitConverter.ToUInt64(fullRaw, 0);
    }

    public EntityId(ulong full)
    {
      m_low = 0U;
      m_high = 0U;
      m_entry = 0U;
      Full = full;
    }

    public EntityId(uint low, uint high)
    {
      Full = 0UL;
      m_entry = 0U;
      m_low = low;
      m_high = high;
    }

    public EntityId(uint low, HighId high)
    {
      Full = 0UL;
      m_high = 0U;
      m_entry = 0U;
      m_low = low;
      High = high;
    }

    public EntityId(uint low, uint entry, HighId high)
    {
      Full = 0UL;
      m_high = 0U;
      m_low = low;
      m_entry = entry;
      High = high;
    }

    public uint Low
    {
      get { return m_low & 16777215U; }
      private set
      {
        m_low &= 4278190080U;
        m_low |= value & 16777215U;
      }
    }

    public uint Entry
    {
      get { return m_entry & 16777215U; }
    }

    public HighId High
    {
      get { return (HighId) (m_high >> 16); }
      private set
      {
        m_high &= ushort.MaxValue;
        m_high |= (uint) value << 16;
      }
    }

    public bool HasEntry
    {
      get { return SeventhByte != HighGuidType.NoEntry; }
    }

    public uint LowRaw
    {
      get { return m_low; }
    }

    public uint HighRaw
    {
      get { return m_high; }
    }

    public bool IsItem
    {
      get { return EighthByte == HighGuid8.Item; }
    }

    public HighGuidType SeventhByte
    {
      get { return (HighGuidType) ((m_high & 16711680U) >> 16); }
    }

    public HighGuid8 EighthByte
    {
      get { return (HighGuid8) ((m_high & 4278190080U) >> 24); }
    }

    public ObjectTypeId ObjectType
    {
      get
      {
        HighGuidType seventhByte = SeventhByte;
        if((uint) seventhByte <= 32U)
        {
          switch(seventhByte)
          {
            case HighGuidType.NoEntry:
              return IsItem ? ObjectTypeId.Item : ObjectTypeId.Player;
            case HighGuidType.GameObject:
              return ObjectTypeId.GameObject;
            case HighGuidType.Transport:
              return ObjectTypeId.GameObject;
          }
        }
        else if((uint) seventhByte <= 64U)
        {
          if(seventhByte == HighGuidType.Unit || seventhByte == HighGuidType.Pet)
            return ObjectTypeId.Unit;
        }
        else
        {
          if(seventhByte == HighGuidType.Vehicle)
            return ObjectTypeId.Unit;
          if(seventhByte == HighGuidType.MapObjectTransport)
            return ObjectTypeId.GameObject;
        }

        return ObjectTypeId.Object;
      }
    }

    public int WritePacked(BinaryWriter binWriter)
    {
      return binWriter.WritePackedUInt64(Full);
    }

    public override bool Equals(object obj)
    {
      return obj != null && obj is EntityId && Equals((EntityId) obj);
    }

    public int CompareTo(object obj)
    {
      if(obj is EntityId)
        return Full.CompareTo(((EntityId) obj).Full);
      if(obj is ulong)
        return Full.CompareTo((ulong) obj);
      return -1;
    }

    public bool Equals(EntityId other)
    {
      return (long) other.Full == (long) Full;
    }

    public static bool operator ==(EntityId left, EntityId right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(EntityId left, EntityId right)
    {
      return !left.Equals(right);
    }

    public override string ToString()
    {
      if(Full == 0UL)
        return "<null>";
      ObjectTypeId objectType = ObjectType;
      if(!HasEntry)
        return string.Format("High: 0x{0:X4} ({1}) - Low: {2}", m_high, objectType,
          m_low);
      string str = objectType.ToString(Entry.ToString());
      return string.Format("High: 0x{0} ({1}) - Low: {2} - Entry: {3}",
        (object) ((ushort) High).ToString("X2"), (object) objectType, (object) Low, (object) str);
    }

    public static EntityId ReadPacked(PacketIn packet)
    {
      uint[] setIndices = Utility.GetSetIndices(packet.ReadByte());
      byte[] fullRaw = new byte[8];
      foreach(uint num in setIndices)
        fullRaw[num] = packet.ReadByte();
      return new EntityId(fullRaw);
    }

    public static implicit operator ulong(EntityId id)
    {
      return id.Full;
    }

    public static EntityId GetCorpseId(uint id)
    {
      return new EntityId(id, 0U, HighId.Corpse);
    }

    public static EntityId GetUnitId(uint id, uint entry)
    {
      return new EntityId(id, entry, HighId.Unit);
    }

    public static EntityId GetPetId(uint id, uint petNumber)
    {
      return new EntityId(id, petNumber, HighId.UnitPet);
    }

    public static EntityId GetPlayerId(uint low)
    {
      return new EntityId(low, 0U, HighId.Player);
    }

    public static EntityId GetItemId(uint low)
    {
      return new EntityId(low, HighId.Item);
    }

    public static EntityId GetDynamicObjectId(uint low)
    {
      return new EntityId(low, 0U, HighId.DynamicObject);
    }

    public static EntityId GetGameObjectId(uint low, GOEntryId entry)
    {
      return new EntityId(low, (uint) entry, HighId.GameObject);
    }

    public static EntityId GetMOTransportId(uint low, uint entry)
    {
      return new EntityId(low, entry, HighId.MoTransport);
    }

    public override int GetHashCode()
    {
      return Full.GetHashCode();
    }

    public TypeCode GetTypeCode()
    {
      return TypeCode.Object;
    }

    public bool ToBoolean(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to Boolean");
    }

    public byte ToByte(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to Byte");
    }

    public char ToChar(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to Char");
    }

    public DateTime ToDateTime(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to DateTime");
    }

    public Decimal ToDecimal(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to Decimal");
    }

    public double ToDouble(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to Double");
    }

    public short ToInt16(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to Int16");
    }

    public int ToInt32(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to Int32");
    }

    public long ToInt64(IFormatProvider provider)
    {
      return (long) Full;
    }

    public sbyte ToSByte(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to SByte");
    }

    public float ToSingle(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to Single");
    }

    public string ToString(IFormatProvider provider)
    {
      return ToString();
    }

    public object ToType(Type conversionType, IFormatProvider provider)
    {
      return Convert.ChangeType(Full, conversionType);
    }

    public ushort ToUInt16(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to UInt16");
    }

    public uint ToUInt32(IFormatProvider provider)
    {
      throw new InvalidCastException("Cannot cast EntityId to UInt32");
    }

    public ulong ToUInt64(IFormatProvider provider)
    {
      return Full;
    }
  }
}