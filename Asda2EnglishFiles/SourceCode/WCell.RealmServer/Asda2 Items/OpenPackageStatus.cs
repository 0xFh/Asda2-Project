namespace WCell.RealmServer.Asda2_Items
{
    public enum OpenPackageStatus
    {
        Ok = 1,
        UserInfoError = 2,
        PackageItemError = 3,
        ItemCountErrorInpackage = 6,
        InfoErrorInEmptyInventry = 7,
        ItemInformationErrorInPackage = 10, // 0x0000000A
        ItemLocationErrorInPackage = 11, // 0x0000000B
        Rariry = 12, // 0x0000000C
        LowLevel = 13, // 0x0000000D
    }
}