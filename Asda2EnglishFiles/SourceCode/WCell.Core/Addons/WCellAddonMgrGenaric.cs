using System;

namespace WCell.Core.Addons
{
    public class WCellAddonMgr<T> : WCellAddonMgr where T : WCellAddonMgr, new()
    {
        public static readonly T Instance = Activator.CreateInstance<T>();
    }
}