using System;
using WCell.Constants;

namespace WCell.Core.Network
{
  public struct PacketId
  {
    public static readonly PacketId Unknown = new PacketId(ServiceType.None, uint.MaxValue);
    public ServiceType Service;
    public uint RawId;

    public PacketId(ServiceType service, uint id)
    {
      Service = service;
      RawId = id;
    }

    public PacketId(AuthServerOpCode id)
    {
      Service = ServiceType.Authentication;
      RawId = (uint) id;
    }

    public PacketId(RealmServerOpCode id)
    {
      Service = ServiceType.Realm;
      RawId = (uint) id;
    }

    public bool IsUpdatePacket
    {
      get { return RawId == 169U || RawId == 502U; }
    }

    public static implicit operator PacketId(AuthServerOpCode val)
    {
      return new PacketId(val);
    }

    public static implicit operator PacketId(RealmServerOpCode val)
    {
      return new PacketId(val);
    }

    public static bool operator ==(PacketId a, PacketId b)
    {
      return (int) a.RawId == (int) b.RawId && a.Service == b.Service;
    }

    public static bool operator !=(PacketId a, PacketId b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj != null && this == (PacketId) obj;
    }

    public override int GetHashCode()
    {
      return RawId.GetHashCode() ^ int.MaxValue * (int) Service;
    }

    public override string ToString()
    {
      string name;
      switch(Service)
      {
        case ServiceType.Authentication:
          name = Enum.GetName(typeof(AuthServerOpCode), RawId);
          break;
        case ServiceType.Realm:
          name = Enum.GetName(typeof(RealmServerOpCode), RawId);
          break;
        default:
          return "Unknown";
      }

      return name ?? RawId.ToString();
    }
  }
}