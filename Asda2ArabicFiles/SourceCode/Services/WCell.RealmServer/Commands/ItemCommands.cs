using System;
using System.Linq;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Logs;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class InventoryCommand : RealmServerCommand
    {
        protected InventoryCommand() { }

        protected override void Initialize()
        {
            Init("Inv", "Item");
            EnglishDescription = "Used for manipulation of Items and Inventory.";
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
        #region Add
        public class ItemAddCommand : SubCommand
        {
            protected ItemAddCommand() { }

            protected override void Initialize()
            {
                Init("Add", "Create");
                EnglishParamInfo = "[-ea] <itemid> [<amount> [<stacks>]]";
                EnglishDescription = "Adds the given amount of stacks (default: 1) of the given amount " +
                    "of the given item to your backpack (if there is space left). " +
                     "-a switch auto-equips, -e switch only adds if not already present.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var mods = trigger.Text.NextModifiers();
                var entry = trigger.Text.NextEnum(Asda2ItemId.None);

                var templ = Asda2ItemMgr.GetTemplate(entry);

                if (templ == null)
                {
                    trigger.Reply("Invalid ItemId.");
                    return;
                }

                var amount = trigger.Text.NextInt(1);
                if ((int)entry == 20551)
                {
                    ((Character)trigger.Args.Target).AddMoney((uint)amount);
                    ((Character)trigger.Args.Target).SendMoneyUpdate();
                    return;
                }
                var stacks = trigger.Text.NextUInt(1);
                var autoEquip = mods.Contains("a");

                for (var i = 0; i < stacks; i++)
                {
                    if (
                        !AddItem((Character)trigger.Args.Target, templ, amount,
                                 trigger.Args.Character))
                    {
                        break;
                    }
                }
            }

            public static bool AddItem(Character chr, Asda2ItemTemplate templ, int amount, Character triggerer)
            {
                var actualAmount = amount;
                var inv = chr.Asda2Inventory;

                // just add
                Asda2Item item = null;
                Asda2InventoryError err = inv.TryAdd((int)templ.ItemId, amount, false, ref item);
                if (err != Asda2InventoryError.Ok || amount < actualAmount)
                {
                    // something went wrong
                    if (err != Asda2InventoryError.Ok)
                    {
                        Asda2InventoryHandler.SendItemReplacedResponse(chr.Client);
                    }
                    return false;
                }

                Log.Create(Log.Types.ItemOperations, LogSourceType.Character, chr.EntryId)
                                                     .AddAttribute("source", 0, "created_by_gm")
                                                     .AddItemAttributes(item)
                                                     .AddAttribute("map", (double)chr.MapId, chr.MapId.ToString())
                                                     .AddAttribute("x", chr.Asda2Position.X)
                                                     .AddAttribute("y", chr.Asda2Position.Y)
                                                     .AddAttribute("gm", triggerer.EntryId, triggerer.Name)
                                                     .Write();
                Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, chr, new[] { item, null, null, null, null });
                return true;
            }
        }
        #endregion
    }
}