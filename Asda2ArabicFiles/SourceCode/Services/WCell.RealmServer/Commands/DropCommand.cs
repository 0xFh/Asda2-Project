using WCell.Constants.Updates;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
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
                var id = trigger.Text.NextInt(20622);
                var character = trigger.Args.Target as Character;
                if (character != null)
                {
                    var loot = new Asda2NPCLoot();
                    var itemTempl = Asda2ItemMgr.GetTemplate(id) ?? Asda2ItemMgr.GetTemplate(20622);
                    loot.Items = new[] { new Asda2LootItem(itemTempl, 1, 0) { Loot = loot } };
                    loot.Lootable = trigger.Args.Character;
                    loot.Looters.Add(new Asda2LooterEntry(trigger.Args.Character));
                    loot.MonstrId = 12345;
                    trigger.Args.Character.Map.SpawnLoot(loot);
                    trigger.Reply("Done.");
                }
                else
                {
                    trigger.Reply("Wrong target.");
                }
            }
        }
    }
}