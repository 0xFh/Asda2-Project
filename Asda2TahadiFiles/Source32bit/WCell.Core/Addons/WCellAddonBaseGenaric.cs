using System;

namespace WCell.Core.Addons
{
  public abstract class WCellAddonBase<A> : WCellAddonBase where A : WCellAddonBase
  {
    public static A Instance { get; private set; }

    protected WCellAddonBase()
    {
      if(Instance != null)
        throw new InvalidOperationException("Tried to create Addon twice: " + this);
      Instance = this as A;
      if(Instance == null)
        throw new InvalidOperationException("Addon has wrong Type parameter - Expected: " + typeof(A).FullName +
                                            " - Found: " + GetType());
    }
  }
}