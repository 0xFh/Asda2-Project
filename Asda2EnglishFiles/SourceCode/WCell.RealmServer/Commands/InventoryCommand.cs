using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class InventoryCommand : RealmServerCommand
    {
        protected InventoryCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Inv", "Item");
            this.EnglishDescription = "Used for manipulation of Items and Inventory.";
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }

        public class ItemAddCommand : RealmServerCommand.SubCommand
        {
            protected ItemAddCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Add", "Create");
                this.EnglishParamInfo = "[-ea] <itemid> [<amount> [<stacks>]]";
                this.EnglishDescription =
                    "Adds the given amount of stacks (default: 1) of the given amount of the given item to your backpack (if there is space left). -a switch auto-equips, -e switch only adds if not already present.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string str = trigger.Text.NextModifiers();
                Asda2ItemId id = trigger.Text.NextEnum<Asda2ItemId>(Asda2ItemId.None);
                Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(id);
                if (template == null)
                {
                    trigger.Reply("Invalid ItemId.");
                }
                else
                {
                    int amount = trigger.Text.NextInt(1);
                    if (id == Asda2ItemId.Gold20551)
                    {
                        ((Character) trigger.Args.Target).AddMoney((uint) amount);
                        ((Character) trigger.Args.Target).SendMoneyUpdate();
                    }
                    else
                    {
                        uint num1 = trigger.Text.NextUInt(1U);
                        str.Contains("a");
                        int num2 = 0;
                        while ((long) num2 < (long) num1 &&
                               InventoryCommand.ItemAddCommand.AddItem((Character) trigger.Args.Target, template,
                                   amount, trigger.Args.Character))
                            ++num2;
                    }
                }
            }

            public static bool AddItem(Character chr, Asda2ItemTemplate templ, int amount, Character triggerer)
            {
                int num = amount;
                Asda2PlayerInventory asda2Inventory = chr.Asda2Inventory;
                Asda2Item asda2Item = (Asda2Item) null;
                Asda2InventoryError asda2InventoryError = asda2Inventory.TryAdd((int) templ.ItemId, amount, false,
                    ref asda2Item, new Asda2InventoryType?(), (Asda2Item) null);
                if (asda2InventoryError != Asda2InventoryError.Ok || amount < num)
                {
                    if (asda2InventoryError != Asda2InventoryError.Ok)
                        Asda2InventoryHandler.SendItemReplacedResponse(chr.Client, Asda2InventoryError.NotInfoAboutItem,
                            (short) 0, (byte) 0, 0, (short) 0, 0, (byte) 0, 0, (short) 0, false);
                    return false;
                }

                Log.Create(Log.Types.ItemOperations, LogSourceType.Character, chr.EntryId)
                    .AddAttribute("source", 0.0, "created_by_gm").AddItemAttributes(asda2Item, "")
                    .AddAttribute("map", (double) chr.MapId, chr.MapId.ToString())
                    .AddAttribute("x", (double) chr.Asda2Position.X, "")
                    .AddAttribute("y", (double) chr.Asda2Position.Y, "")
                    .AddAttribute("gm", (double) triggerer.EntryId, triggerer.Name).Write();
                Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, chr, new Asda2Item[5]
                {
                    asda2Item,
                    null,
                    null,
                    null,
                    null
                });
                return true;
            }
        }
    }
}