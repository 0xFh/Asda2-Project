using System;

namespace WCell.Core.Addons
{
    public abstract class WCellAddonBase<A> : WCellAddonBase where A : WCellAddonBase
    {
        public static A Instance { get; private set; }

        protected WCellAddonBase()
        {
            if ((object) WCellAddonBase<A>.Instance != null)
                throw new InvalidOperationException("Tried to create Addon twice: " + (object) this);
            WCellAddonBase<A>.Instance = this as A;
            if ((object) WCellAddonBase<A>.Instance == null)
                throw new InvalidOperationException("Addon has wrong Type parameter - Expected: " + typeof(A).FullName +
                                                    " - Found: " + (object) this.GetType());
        }
    }
}