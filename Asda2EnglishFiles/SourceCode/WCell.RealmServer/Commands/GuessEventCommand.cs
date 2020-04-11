using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Events.Asda2;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class GuessEventCommand : RealmServerCommand
    {
        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            this.Init("guessword", "ge");
        }

        public class StartCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("start");
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string word = trigger.Text.NextWord();
                if (word.Length < 3)
                    trigger.Reply("Minimum length of secret word is 3");
                else if (Asda2EventMgr.IsGuessWordEventStarted)
                {
                    trigger.Reply("Guess word event is already started.");
                }
                else
                {
                    int precison = trigger.Text.NextInt(100);
                    Asda2EventMgr.StartGueesWordEvent(word, precison, trigger.Args.Character.Name);
                    trigger.Reply("Ok, guess word event started. Word is {0}, percision is {1}.", (object) word,
                        (object) precison);
                }
            }
        }

        public class StopCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("stop");
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                if (!Asda2EventMgr.IsGuessWordEventStarted)
                {
                    trigger.Reply("Guess word event is not started.");
                }
                else
                {
                    Asda2EventMgr.StopGueesWordEvent();
                    trigger.Reply("Guess word event stoped.");
                }
            }
        }
    }
}