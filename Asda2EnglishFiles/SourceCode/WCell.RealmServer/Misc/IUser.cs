using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Misc
{
    public interface IUser : IChatter, INamedEntity, INamed, IPacketReceivingEntity, IEntity, IPacketReceiver,
        IChatTarget, IGenericChatTarget, IHasRole
    {
        /// <summary>Whether this User ignores the given User</summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool IsIgnoring(IUser user);

        /// <summary>The targeted Unit of this User (or null)</summary>
        Unit Target { get; }

        /// <summary>The command that has been selected by this User</summary>
        BaseCommand<RealmServerCmdArgs> SelectedCommand { get; set; }

        List<ChatChannel> ChatChannels { get; }

        FactionGroup FactionGroup { get; }

        ClientLocale Locale { get; set; }
    }
}