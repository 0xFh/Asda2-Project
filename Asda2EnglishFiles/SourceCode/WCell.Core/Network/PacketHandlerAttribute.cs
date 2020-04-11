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
            get { return this.Id.Service; }
        }

        public PacketHandlerAttribute(PacketId identifier)
        {
            this.Id = identifier;
            this.IsGamePacket = true;
            this.RequiresLogin = true;
        }

        public PacketHandlerAttribute(AuthServerOpCode identifier)
        {
            this.Id = (PacketId) identifier;
            this.IsGamePacket = true;
            this.RequiresLogin = true;
        }

        public PacketHandlerAttribute(RealmServerOpCode identifier)
        {
            this.Id = (PacketId) identifier;
            this.IsGamePacket = true;
            this.RequiresLogin = true;
        }
    }
}