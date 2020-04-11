using WCell.Constants.Misc;

namespace WCell.RealmServer.Titles
{
  public class CharacterTitleEntry
  {
    public TitleId TitleId;
    public string[] Names;
    public TitleBitId BitIndex;

    public CharacterTitleEntry()
    {
      Names = new string[16];
    }
  }
}