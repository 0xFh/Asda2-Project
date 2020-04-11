using WCell.RealmServer.Privileges;

namespace WCell.RealmServer.Misc
{
    public interface IHasRole
    {
        /// <summary>The RoleGroup of this entity</summary>
        RoleGroup Role { get; }
    }
}