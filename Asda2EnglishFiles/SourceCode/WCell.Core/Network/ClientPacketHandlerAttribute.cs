using System;
using WCell.Constants;

namespace WCell.Core.Network
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class ClientPacketHandlerAttribute : PacketHandlerAttribute
    {
        public ClientPacketHandlerAttribute(PacketId identifier)
            : base(identifier)
        {
        }

        public ClientPacketHandlerAttribute(AuthServerOpCode identifier)
            : base(identifier)
        {
        }

        public ClientPacketHandlerAttribute(RealmServerOpCode identifier)
            : base(identifier)
        {
        }
    }
}