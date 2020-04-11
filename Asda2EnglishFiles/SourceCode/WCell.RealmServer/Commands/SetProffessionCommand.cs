using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class SetProffessionCommand : RealmServerCommand
    {
        protected SetProffessionCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("SetProff", "proff", "setpr");
            this.EnglishParamInfo = "<proffId,proffLevel>";
            this.EnglishDescription =
                "OHS = 1,Spear = 2,THS = 3,Crossbow = 4,Bow = 5,Balista = 6,AtackMage = 7,SupportMage = 8,HealMage = 9,.Ex: setpr 4 1";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (!trigger.Text.HasNext)
            {
                trigger.Reply(
                    "OHS = 1,Spear = 2,THS = 3,Crossbow = 4,Bow = 5,Balista = 6,AtackMage = 7,SupportMage = 8,HealMage = 9 Ex: proff 4 1");
            }
            else
            {
                Character target = (Character) trigger.Args.Target;
                int proff = trigger.Text.NextInt(1);
                int proffLevel = trigger.Text.NextInt(1);
                if (proffLevel <= 0 || proffLevel > 4)
                {
                    trigger.Reply(
                        "OHS = 1,Spear = 2,THS = 3,Crossbow = 4,Bow = 5,Balista = 6,AtackMage = 7,SupportMage = 8,HealMage = 9,.Ex: proff 4 1");
                    trigger.Reply("You must select proff level 1 - 4");
                }
                else if (proff < 0 || (Decimal) proff >= new Decimal(12))
                {
                    trigger.Reply(
                        "OHS = 1,Spear = 2,THS = 3,Crossbow = 4,Bow = 5,Balista = 6,AtackMage = 7,SupportMage = 8,HealMage = 9,.Ex: proff 4 1");
                    trigger.Reply("You must select real proff proffession");
                }
                else
                    target.SetClass(proffLevel, proff);
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}