using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Global
{
    /// <summary>
    /// Delegate used for events when a <see cref="T:WCell.RealmServer.Entities.Character" /> is changed, like logging in or out.
    /// </summary>
    /// <param name="chr">the <see cref="T:WCell.RealmServer.Entities.Character" /> being changed</param>
    public delegate void CharacterChangedHandler(Character chr);
}