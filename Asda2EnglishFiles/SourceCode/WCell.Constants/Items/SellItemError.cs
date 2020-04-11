namespace WCell.Constants.Items
{
    public enum SellItemError : byte
    {
        Success,
        CantFindItem,
        CantSellItem,
        CantFindVendor,
        PlayerDoesntOwnItem,
        Unknown,
        OnlyEmptyBag,
    }
}