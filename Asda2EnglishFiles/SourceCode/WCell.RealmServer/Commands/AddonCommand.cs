using System.Collections.Generic;
using WCell.Core.Addons;
using WCell.RealmServer.Addons;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class AddonCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Addon");
            this.EnglishDescription = "Provides commands for managing Addons";
        }

        public class ListAddonsCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("List", "L");
                this.EnglishParamInfo = "[-l]";
                this.EnglishDescription = "Lists all active Addons. -l to also list libraries.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                int num = 0;
                bool flag = trigger.Text.NextModifiers().Contains("l");
                foreach (WCellAddonContext context in (IEnumerable<WCellAddonContext>) WCellAddonMgr.Contexts)
                {
                    ++num;
                    if (context.Addon != null)
                        trigger.Reply(num.ToString() + ". " + trigger.Translate(RealmLangKey.Addon, new object[0]) +
                                      " " + (object) context.Addon);
                    else if (flag)
                        trigger.Reply(num.ToString() + ". " + trigger.Translate(RealmLangKey.Library, new object[0]) +
                                      " " + (object) context.Assembly);
                }
            }
        }

        public class LoadAddonCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Load");
                this.EnglishParamInfo = "<Path>";
                this.EnglishDescription = "Loads a new Addon from the given file.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string libName = trigger.Text.NextWord();
                if (libName.Length == 0)
                {
                    trigger.Reply("No Path given.");
                }
                else
                {
                    trigger.Reply("Loading addon from " + libName + "...");
                    WCellAddonContext wcellAddonContext = WCellAddonMgr<RealmAddonMgr>.Instance.TryLoadAddon(libName);
                    if (wcellAddonContext == null)
                        trigger.Reply("File does not exist or has invalid format: " + libName);
                    else
                        trigger.Reply("Done: " + (object) wcellAddonContext);
                }
            }
        }
    }
}