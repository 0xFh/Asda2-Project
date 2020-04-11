namespace WCell.RealmServer.Groups
{
    /// <summary>
    /// Defines an interface that allows one type of group to convert itself into another.
    /// </summary>
    /// <typeparam name="T">the type of group to convert to</typeparam>
    internal interface IGroupConverter<T> where T : Group
    {
        /// <summary>Converts one type of group to another.</summary>
        /// <returns>the newly converter group object</returns>
        T ConvertTo();
    }
}