namespace WCell.Core.Addons
{
    public static class WCellAddonUtil
    {
        /// <summary>
        /// 
        /// </summary>
        public static string GetDefaultDescription(this IWCellAddon addon)
        {
            return string.Format("{0} v{1} by {2} ({3})", (object) addon.Name,
                (object) addon.GetType().Assembly.GetName().Version, (object) addon.Author, (object) addon.Website);
        }
    }
}