using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class MultiplySpeedCommand : RealmServerCommand
    {
        public override bool RequiresEqualOrHigherRank
        {
            get { return true; }
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected MultiplySpeedCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("MultiplySpeed", "SpeedFactor", "MultSpeed", "Speed");
            this.EnglishParamInfo = "<speedFactor>";
            this.EnglishDescription = "Sets the overall speed-factor of a Unit";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            float num = trigger.Text.NextFloat(1f);
            string str = trigger.Text.NextModifiers();
            if (str == "i")
                trigger.Reply("speed : {0}", (object) trigger.Args.Target.SpeedFactor);
            else if ((double) num > 0.01)
            {
                if (str.Contains("w"))
                {
                    foreach (Unit allCharacter in World.GetAllCharacters())
                        allCharacter.SpeedFactor = num;
                }
                else if (str.Contains("n"))
                {
                    foreach (Unit nearbyCharacter in (IEnumerable<Character>) trigger.Args.Character
                        .GetNearbyCharacters<Character>())
                        nearbyCharacter.SpeedFactor = num;
                }
                else if (str.Contains("m"))
                {
                    foreach (Unit unit in trigger.Args.Character.Map.Characters.ToArray())
                        unit.SpeedFactor = num;
                }
                else
                {
                    trigger.Args.Target.SpeedFactor = num;
                    trigger.Reply("SpeedFactor set to: " + (object) num);
                }
            }
            else
                trigger.Reply("The argument must be a positive number");
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}