using System;
using WCell.Constants;

namespace WCell.Core.Network
{
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
  public class PacketHandlerAttribute : Attribute
  {
    public PacketId Id { get; set; }

    public bool IsGamePacket { get; set; }

    public bool RequiresLogin { get; set; }

    public ServiceType Service
    {
      get { return Id.Service; }
    }

    public PacketHandlerAttribute(PacketId identifier)
    {
      Id = identifier;
      IsGamePacket = true;
      RequiresLogin = true;
    }

    public PacketHandlerAttribute(AuthServerOpCode identifier)
    {
      Id = identifier;
      IsGamePacket = true;
      RequiresLogin = true;
    }

    public PacketHandlerAttribute(RealmServerOpCode identifier)
    {
      Id = identifier;
      IsGamePacket = true;
      RequiresLogin = true;
    }
  }
}