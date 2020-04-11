using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Privileges;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public static class CommandUtility
    {
        public static bool GetRequiresContext(this BaseCommand<RealmServerCmdArgs> cmd)
        {
            RealmServerCommand rootCmd = cmd.RootCmd as RealmServerCommand;
            if (rootCmd != null)
                return rootCmd.RequiresContext;
            return false;
        }

        /// <summary>
        /// Returns one of the possible values: EvalNext if next argument is in parentheses, else target ?? user
        /// </summary>
        public static object EvalNextOrTargetOrUser(this CmdTrigger<RealmServerCmdArgs> trigger)
        {
            return trigger.EvalNext<object>((object) trigger.Args.Target ?? (object) trigger.Args.User);
        }

        /// <summary>Check for selected Target and selected Command</summary>
        public static bool InitTrigger(this CmdTrigger<RealmServerCmdArgs> trigger)
        {
            RealmServerCmdArgs args = trigger.Args;
            if (args.Role.IsStaff)
            {
                string mod = trigger.Text.NextModifiers();
                if (mod.Length > 0)
                {
                    Character character = trigger.GetCharacter(mod);
                    if (character == null)
                        return false;
                    args.Character = character;
                    args.Double = false;
                }
                else if (args.Character != null && trigger.SelectedCommand == null)
                    trigger.SelectedCommand = args.Character.ExtraInfo.SelectedCommand;
            }

            return true;
        }

        /// <summary>
        /// Sets the Character of this trigger, according to the -a or -c switch, followed by the account- or character-name
        /// </summary>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static Character GetCharacter(this CmdTrigger<RealmServerCmdArgs> trigger, string mod)
        {
            RealmServerCmdArgs args = trigger.Args;
            Character character = (Character) null;
            RoleGroup role = args.Role;
            bool flag1 = mod.Contains("a");
            bool flag2 = mod.Contains("c");
            if (flag1 || flag2)
            {
                if (flag1 && flag2)
                {
                    trigger.Reply("Invalid command-switch, cannot use -a and -c switch at the same time.");
                }
                else
                {
                    if ((object) role != null && !role.CanUseCommandsOnOthers)
                        trigger.Reply("You may not use the -c or -a command-switch!");
                    string name = trigger.Text.NextWord();
                    if (flag1)
                    {
                        RealmAccount loggedInAccount =
                            ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(name);
                        if (loggedInAccount == null || (character = loggedInAccount.ActiveCharacter) == null)
                            trigger.Reply("Account {0} is not online.", (object) name);
                    }
                    else
                    {
                        character = World.GetCharacter(name, false);
                        if (character == null)
                            trigger.Reply("Character {0} is not online.", (object) name);
                    }

                    if (character != null)
                    {
                        if ((object) role == null || !(character.Account.Role > role))
                            return character;
                        if (flag1)
                            trigger.Reply("Account {0} is not online.", (object) name);
                        else if (character.Stealthed == 0)
                            trigger.Reply("Cannot use this Command on {0}.", (object) character.Name);
                        else
                            trigger.Reply("Character {0} is not online.", (object) name);
                    }
                }
            }
            else
                trigger.Reply("Invalid Command-Switch: " + mod);

            return (Character) null;
        }

        public static void ShowMenu(this CmdTrigger<RealmServerCmdArgs> trigger, GossipMenu menu)
        {
            Character character = trigger.Args.Character;
            if (character == null)
                return;
            character.StartGossip(menu);
        }
    }
}