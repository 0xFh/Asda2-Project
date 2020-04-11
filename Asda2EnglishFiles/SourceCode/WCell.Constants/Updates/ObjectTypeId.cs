namespace WCell.Constants.Updates
{
    /// <summary>
    /// Object Type Ids are used in SMSG_UPDATE_OBJECT inside realm server
    /// </summary>
    public enum ObjectTypeId
    {
        Object = 0,
        Item = 1,
        Container = 2,
        Unit = 3,
        Player = 4,
        GameObject = 5,
        DynamicObject = 6,
        Corpse = 7,
        Loot = 8,
        Count = 9,
        None = 255, // 0x000000FF
    }
}