using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Commands
{
    public class SummonAllCommand : RealmServerCommand
    {
        protected SummonAllCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            this.Init("SummonAll");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            int num1 = trigger.Text.NextInt(1);
            int num2 = trigger.Text.NextInt(250);
            float num3 = trigger.Text.NextFloat(0.7f);
            string str = trigger.Text.NextModifiers();
            int num4 = 0;
            if (str == "n")
            {
                foreach (Character nearbyCharacter in (IEnumerable<Character>) trigger.Args.Character
                    .GetNearbyCharacters<Character>(200f, false))
                {
                    Vector3 position = trigger.Args.Character.Position;
                    Vector2 posDiff = Utility.GetPosDiff(num4++);
                    nearbyCharacter.TeleportTo(trigger.Args.Character.Map.Id,
                        new Vector3(position.X + posDiff.X * num3, position.Y + posDiff.Y * num3));
                }
            }
            else
            {
                foreach (Character allCharacter in World.GetAllCharacters())
                {
                    if (allCharacter != trigger.Args.Target && allCharacter.Level >= num1 && num2 >= allCharacter.Level)
                    {
                        if (str == "f")
                        {
                            Vector3 position = trigger.Args.Character.Position;
                            Vector2 posDiff = Utility.GetPosDiff(num4++);
                            allCharacter.TeleportTo(trigger.Args.Character.Map.Id,
                                new Vector3(position.X + posDiff.X * num3, position.Y + posDiff.Y * num3));
                        }
                        else
                            Asda2SoulmateHandler.SendSoulmateSummoningYouResponse(trigger.Args.Character, allCharacter);
                    }
                }
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}