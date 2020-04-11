using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class RaceCommand : RealmServerCommand
    {
        protected RaceCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Race", "SetRace");
            this.EnglishParamInfo = "<race>";
            this.EnglishDescription = "Sets the Unit's race. Also adds the Race's language.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            string input = trigger.Text.NextWord();
            RaceId result;
            if (EnumUtil.TryParse<RaceId>(input, out result))
            {
                trigger.Args.Target.Race = result;
                if (!(trigger.Args.Target is Character))
                    return;
                LanguageDescription languageDescByRace = LanguageHandler.GetLanguageDescByRace(result);
                ((Character) trigger.Args.Target).AddLanguage(languageDescByRace);
            }
            else
                trigger.Reply("Invalid Race: " + input);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }
    }
}