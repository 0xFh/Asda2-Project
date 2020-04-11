using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AreaTriggers
{
    /// <summary>
    /// Returns whether the given Character has triggered the given trigger or false if not allowed.
    /// </summary>
    /// <param name="chr"></param>
    /// <param name="trigger"></param>
    /// <returns></returns>
    public delegate bool AreaTriggerHandler(Character chr, AreaTrigger trigger);
}