using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Handlers
{
  [DataHolder]
  public class MineTableRecord : IDataHolder
  {
    public int Id { get; set; }

    public int MapId { get; set; }

    public int IsPremium { get; set; }

    public int DigTime { get; set; }

    public int MinLevel { get; set; }

    [Persistent(Length = 50)]
    public int[] ItemIds { get; set; }

    [Persistent(Length = 50)]
    public int[] Chances { get; set; }

    public void FinalizeDataHolder()
    {
      if(IsPremium == 1)
        Asda2DigMgr.PremiumMapDiggingTemplates.Add((byte) MapId, this);
      else
        Asda2DigMgr.MapDiggingTemplates.Add((byte) MapId, this);
    }

    public int GetRandomItem()
    {
      int num1 = Utility.Random(0, 100000);
      int num2 = 0;
      for(int index = 0; index < 50; ++index)
      {
        num2 += Chances[index];
        if(num2 >= num1)
          return ItemIds[index];
      }

      return 20622;
    }
  }
}