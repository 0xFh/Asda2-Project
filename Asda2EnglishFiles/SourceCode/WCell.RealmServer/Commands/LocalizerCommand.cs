using System;
using WCell.Constants;
using WCell.RealmServer.Global;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Misc;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class LocalizerCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Localizer", "Lang");
            this.Description = new TranslatableItem(RealmLangKey.CmdLocalizerDescription, new object[0]);
        }

        public class ReloadLangCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Reload", "Resync");
                this.Description = new TranslatableItem(RealmLangKey.CmdLocalizerSetLocaleDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                World.ExecuteWhilePaused((Action) (() => RealmLocalizer.Instance.Resync()));
                trigger.Reply(RealmLangKey.Done);
            }
        }

        public class SetLocaleCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("SetLocale", "Locale");
                this.ParamInfo = new TranslatableItem(RealmLangKey.CmdLocalizerSetLocaleParamInfo, new object[0]);
                this.Description = new TranslatableItem(RealmLangKey.CmdLocalizerSetLocaleDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                IUser user = trigger.Args.User;
                if (user != null)
                {
                    user.Locale = trigger.Text.NextEnum<ClientLocale>(user.Locale);
                    trigger.Reply(RealmLangKey.Done);
                }
                else
                    trigger.Reply(RealmLangKey.UnableToSetUserLocale);
            }
        }
    }
}