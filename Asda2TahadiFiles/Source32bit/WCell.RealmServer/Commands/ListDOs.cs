using WCell.RealmServer.Spells;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class ListDOs : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init(nameof(ListDOs));
      EnglishDescription = "Shows a list of all available Dynamic Objects";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      foreach(Spell spell in SpellHandler.DOSpells.Values)
        trigger.Reply("{0} (Id: {1})", (object) spell.Name, (object) spell.Id);
    }
  }
}