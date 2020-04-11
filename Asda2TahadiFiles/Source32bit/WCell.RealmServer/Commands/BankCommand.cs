using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class BankCommand : RealmServerCommand
  {
    protected BankCommand()
    {
    }

    protected override void Initialize()
    {
      Init("Bank");
      EnglishParamInfo = "";
      EnglishDescription =
        "Opens the bank for the target through oneself (if one leaves the target, it won't be allowed to continue using the Bank).";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      ((Character) trigger.Args.Target).OpenBank(trigger.Args.Character);
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Player; }
    }
  }
}