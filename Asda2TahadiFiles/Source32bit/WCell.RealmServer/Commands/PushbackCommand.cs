using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class PushbackCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("Pushback");
      ParamInfo = new TranslatableItem(RealmLangKey.CmdPushbackParams);
      Description = new TranslatableItem(RealmLangKey.CmdPushbackDescription);
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      Unit target = trigger.Args.Target;
      if(target == null)
        trigger.Reply(RealmLangKey.NoValidTarget);
      else
        target.SpellCast.Pushback(trigger.Text.NextInt(1000));
    }
  }
}