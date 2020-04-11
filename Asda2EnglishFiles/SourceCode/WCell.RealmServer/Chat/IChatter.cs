using WCell.Constants;
using WCell.Constants.Misc;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Chat
{
    /// <summary>Defines an object that can actively chat.</summary>
    public interface IChatter : INamedEntity, INamed, IPacketReceivingEntity, IEntity, IPacketReceiver, IChatTarget,
        IGenericChatTarget
    {
        /// <summary>The chat tags of the object.</summary>
        ChatTag ChatTag { get; }

        /// <summary>The spoken language of the player.</summary>
        ChatLanguage SpokenLanguage { get; }
    }
}