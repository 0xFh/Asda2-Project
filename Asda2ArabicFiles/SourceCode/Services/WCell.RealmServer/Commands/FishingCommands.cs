/*************************************************************************
 *
 *   file		: GOCommands.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-23 15:13:50 +0200 (fr, 23 apr 2010) $

 *   revision		: $Rev: 1282 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Global;
using WCell.Util.Collections;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	public class FishingCommand : RealmServerCommand
	{
        protected FishingCommand() { }

		protected override void Initialize()
		{
			base.Init("Fishing", "fish", "F");
			EnglishDescription = "Asda2 fishing commands";
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.None; }
		}

		public override bool RequiresCharacter
		{
			get { return true; }
		}
        public class SetFishingLevelCommand : SubCommand
        {
            #region Overrides of BaseCommand<RealmServerCmdArgs>

            protected override void Initialize()
            {
                Init("Level", "lvl", "l");
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var target = trigger.Args.Target as Character;
                if(target == null)
                {
                    trigger.Reply("Wrong target.");
                    return;
                }
                target.FishingLevel = trigger.Text.NextInt(0);
                trigger.Reply("Done.");
            }

            #endregion
        }
        public class CompleteBooksCommand : SubCommand
        {
            #region Overrides of BaseCommand<RealmServerCmdArgs>

            protected override void Initialize()
            {
                Init("CompleteBooks", "Book", "b");
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var target = trigger.Args.Target as Character;
                if (target == null)
                {
                    trigger.Reply("Wrong target.");
                    return;
                }
                if(trigger.Text.String.Contains("all"))
                {
                    foreach (var book in target.RegisteredFishingBooks.Values)
                    {
                        book.Complete();
                    }
                    trigger.Reply("Done. Teleport to another location to refresh books.");
                    return;
                }
                var indexOfBook = trigger.Text.NextInt(0);
                if(target.RegisteredFishingBooks.ContainsKey((byte) indexOfBook))
                    target.RegisteredFishingBooks[(byte) indexOfBook].Complete();
                else
                {
                    trigger.Reply("Book not founded.");
                    return;
                }
                trigger.Reply("Done.");
            }

            #endregion
        }
		
	}

}