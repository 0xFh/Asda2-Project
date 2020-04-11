using WCell.Constants;
using WCell.RealmServer.Content;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.RacesClasses
{
  [DataHolder]
  public class ClassLevelSetting : IDataHolder
  {
    public ClassId ClassId;
    public uint Level;
    public int Health;
    public int Mana;

    public void FinalizeDataHolder()
    {
      BaseClass baseClass = ArchetypeMgr.GetClass(ClassId);
      if(baseClass == null)
        ContentMgr.OnInvalidDBData("Invalid ClassId in BaseClassSetting: {0} ({1})", (object) ClassId,
          (object) (int) ClassId);
      else
        ArrayUtil.Set(ref baseClass.Settings, Level, this);
    }
  }
}