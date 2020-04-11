using System;

namespace WCell.Constants.Guilds
{
    /// <summary>Rights of a Guild Member</summary>
    [Flags]
    public enum GuildPrivileges : uint
    {
        None = 0,
        SetMemberPrivilegies = 1,
        Applicants = 2,
        UsePoints = 4,
        EditAnnounce = 8,
        EditRankSettings = 16, // 0x00000010
        EditCrest = 32, // 0x00000020
        InviteMembers = 64, // 0x00000040

        All = InviteMembers | EditCrest | EditRankSettings | EditAnnounce | UsePoints | Applicants |
              SetMemberPrivilegies, // 0x0000007F
        Default = 0,
    }
}