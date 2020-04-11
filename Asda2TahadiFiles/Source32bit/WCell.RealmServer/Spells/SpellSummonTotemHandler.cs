namespace WCell.RealmServer.Spells
{
  public class SpellSummonTotemHandler : SpellSummonHandler
  {
    public SpellSummonTotemHandler(uint index)
    {
      Index = index;
    }

    public uint Index { get; private set; }
  }
}