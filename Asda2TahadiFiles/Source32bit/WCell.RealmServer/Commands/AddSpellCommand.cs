using WCell.Constants.Updates;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  /// <summary>
  /// 
  /// </summary>
  public class AddSpellCommand : RealmServerCommand
  {
    protected AddSpellCommand()
    {
    }

    protected override void Initialize()
    {
      Init("spelladd", "addspell");
      EnglishParamInfo = "";
      EnglishDescription = "Deprecated - Use \"spell add\" instead.";
      Enabled = false;
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      SpellCommand.SpellAddCommand.Instance.Process(trigger);
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Unit; }
    }
  }
}