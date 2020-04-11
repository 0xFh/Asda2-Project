using WCell.Constants.Updates;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class DropCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("Drop");
      EnglishDescription = "Drops item";
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Player; }
    }

    public class AddCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Item", "i");
        EnglishDescription = "Drops item";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        int id = trigger.Text.NextInt(20622);
        if(trigger.Args.Target is Character)
        {
          Asda2NPCLoot asda2NpcLoot = new Asda2NPCLoot();
          Asda2ItemTemplate templ = Asda2ItemMgr.GetTemplate(id) ?? Asda2ItemMgr.GetTemplate(20622);
          asda2NpcLoot.Items = new Asda2LootItem[1]
          {
            new Asda2LootItem(templ, 1, 0U)
            {
              Loot = asda2NpcLoot
            }
          };
          asda2NpcLoot.Lootable = trigger.Args.Character;
          asda2NpcLoot.Looters.Add(new Asda2LooterEntry(trigger.Args.Character));
          asda2NpcLoot.MonstrId = 12345;
          trigger.Args.Character.Map.SpawnLoot(asda2NpcLoot);
          trigger.Reply("Done.");
        }
        else
          trigger.Reply("Wrong target.");
      }
    }
  }
}