using System;
using System.Collections.Generic;
using System.Globalization;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class EventCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("evt");
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        public class TransformCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("transform", "trfm");
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                int id = trigger.Text.NextInt(1);
                string mod = trigger.Text.NextModifiers();
                if (mod == "t")
                {
                    int num1 = trigger.Text.NextInt(800);
                    int num2 = trigger.Text.NextInt(2500);
                    int num3 = 0;
                    for (int index = id; index < num1; ++index)
                    {
                        ++num3;
                        int i1 = index;
                        trigger.Args.Character.CallDelayed(num3 * num2, (Action<WorldObject>) (o =>
                        {
                            EventCommand.TransformCommand.TransformCharacters(trigger.Args.Character, mod, i1);
                            trigger.Args.Character.SendInfoMsg(
                                i1.ToString((IFormatProvider) CultureInfo.InvariantCulture));
                        }));
                    }
                }
                else
                    EventCommand.TransformCommand.TransformCharacters(trigger.Args.Character, mod, id);
            }

            private static void TransformCharacters(Character c, string mod, int id)
            {
                foreach (Character nearbyCharacter in (IEnumerable<Character>) c.GetNearbyCharacters<Character>())
                {
                    if (mod == "r")
                    {
                        nearbyCharacter.TransformationId = (short) 0;
                        nearbyCharacter.TransformationId = (short) Utility.Random(1, 800);
                    }
                    else
                    {
                        nearbyCharacter.TransformationId = (short) 0;
                        nearbyCharacter.TransformationId = (short) id;
                    }
                }
            }
        }

        public class DanceCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("dance", "dnc", "d");
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                short emote = (short) trigger.Text.NextInt(1);
                foreach (Character nearbyCharacter in (IEnumerable<Character>) trigger.Args.Character
                    .GetNearbyCharacters<Character>(true))
                    Asda2CharacterHandler.SendEmoteResponse(nearbyCharacter, emote, (byte) 1, 0.0596617f, -0.9982219f);
            }
        }

        public class LuckyDropCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("luckydrop", "ld");
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Asda2LootMgr.EnableLuckyDropEvent();
                trigger.Reply("done");
            }
        }
    }
}