using System;
using WCell.Constants.Items;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class AddDonationItemCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("adi", "AddDonatedItem");
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      Character targetChr = trigger.Args.Target as Character;
      if(targetChr == null)
      {
        trigger.Reply("Wrong target.");
      }
      else
      {
        Asda2ItemTemplate templ =
          Asda2ItemMgr.GetTemplate(trigger.Text.NextEnum(Asda2ItemId.None));
        if(templ == null)
        {
          trigger.Reply("Invalid ItemId.");
        }
        else
        {
          int amount = trigger.Text.NextInt(1);
          if(amount <= 0)
            trigger.Reply("Wrong amount.");
          else
            ServerApp<RealmServer>.IOQueue.AddMessage(() =>
              targetChr.Asda2Inventory.AddDonateItem(templ, amount, trigger.Args.Character.Name, false));
        }
      }
    }
  }
}