using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Help.Tickets;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Misc
{
    public interface IStaffUser : IUser, IChatter, INamedEntity, IPacketReceivingEntity, IEntity, IPacketReceiver,
        IChatTarget, ITicketHandler, IGenericChatTarget, INamed, IHasRole
    {
    }
}