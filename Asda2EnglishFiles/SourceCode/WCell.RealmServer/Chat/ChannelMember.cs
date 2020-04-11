using System;
using WCell.Constants.Chat;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Chat
{
    /// <summary>Defines a member of a chat channel.</summary>
    public class ChannelMember
    {
        /// <summary>The member of the channel.</summary>
        public readonly IUser User;

        /// <summary>The member's channel flags.</summary>
        public ChannelMemberFlags Flags;

        /// <summary>
        /// Creates a new <see cref="T:WCell.RealmServer.Chat.ChannelMember" /> given the user.
        /// </summary>
        /// <param name="user">the user being represented</param>
        public ChannelMember(IUser user)
        {
            this.User = user;
        }

        /// <summary>
        /// Creates a new <see cref="T:WCell.RealmServer.Chat.ChannelMember" /> given the user and their flags.
        /// </summary>
        /// <param name="user">the user being represented</param>
        /// <param name="flags">the flags of the user</param>
        public ChannelMember(IUser user, ChannelMemberFlags flags)
        {
            this.User = user;
            this.Flags = flags;
        }

        /// <summary>Whether or not the user is the owner of the channel.</summary>
        public bool IsOwner
        {
            get { return this.Flags.HasFlag((Enum) ChannelMemberFlags.Owner); }
            set
            {
                if (value)
                    this.Flags |= ChannelMemberFlags.Owner;
                else
                    this.Flags &= ~ChannelMemberFlags.Owner;
            }
        }

        /// <summary>
        /// Whether or not the user is a moderator on the channel.
        /// </summary>
        public bool IsModerator
        {
            get { return this.Flags.HasFlag((Enum) ChannelMemberFlags.Moderator); }
            set
            {
                if (value)
                    this.Flags |= ChannelMemberFlags.Moderator;
                else
                    this.Flags &= ~ChannelMemberFlags.Moderator;
            }
        }

        /// <summary>Whether the user is voiced on the channel.</summary>
        public bool IsVoiced
        {
            get { return this.Flags.HasFlag((Enum) ChannelMemberFlags.Voiced); }
            set
            {
                if (value)
                    this.Flags |= ChannelMemberFlags.Voiced;
                else
                    this.Flags &= ~ChannelMemberFlags.Voiced;
            }
        }

        /// <summary>Whether the user is muted on the channel.</summary>
        public bool IsMuted
        {
            get { return this.Flags.HasFlag((Enum) ChannelMemberFlags.Muted); }
            set
            {
                if (value)
                    this.Flags |= ChannelMemberFlags.Muted;
                else
                    this.Flags &= ~ChannelMemberFlags.Muted;
            }
        }

        /// <summary>Whether the user is voice muted on the channel.</summary>
        public bool IsVoiceMuted
        {
            get { return this.Flags.HasFlag((Enum) ChannelMemberFlags.VoiceMuted); }
            set
            {
                if (value)
                    this.Flags |= ChannelMemberFlags.VoiceMuted;
                else
                    this.Flags &= ~ChannelMemberFlags.VoiceMuted;
            }
        }

        /// <summary>Operator overload for the greater-than operator.</summary>
        /// <param name="member">the first <see cref="T:WCell.RealmServer.Chat.ChannelMember" /></param>
        /// <param name="member2">the second <see cref="T:WCell.RealmServer.Chat.ChannelMember" /></param>
        /// <returns>true if the first member is greater than the second, based on role, ownership
        /// of the channel, and moderator status on the channel</returns>
        public static bool operator >(ChannelMember member, ChannelMember member2)
        {
            if (member.User.Role.IsStaff && member.User.Role > member2.User.Role)
                return true;
            if (member2.IsOwner)
                return false;
            if (member.IsOwner)
                return true;
            if (!member2.IsModerator)
                return member.IsModerator;
            return false;
        }

        /// <summary>Operator overload for the greater-than operator.</summary>
        /// <param name="member1">the first <see cref="T:WCell.RealmServer.Chat.ChannelMember" /></param>
        /// <param name="member2">the second <see cref="T:WCell.RealmServer.Chat.ChannelMember" /></param>
        /// <returns>true if the first member is greater than the second, based on role, ownership
        /// of the channel, and moderator status on the channel</returns>
        public static bool operator <(ChannelMember member1, ChannelMember member2)
        {
            return member2 > member1;
        }
    }
}