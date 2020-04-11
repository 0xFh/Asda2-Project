using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Groups
{
    public static class GroupUtil
    {
        public static void EnsurePureStaffGroup(this Character chr)
        {
            Group group = chr.Group;
            if (group == null)
                return;
            group.EnsurePureStaffGroup();
        }
    }
}