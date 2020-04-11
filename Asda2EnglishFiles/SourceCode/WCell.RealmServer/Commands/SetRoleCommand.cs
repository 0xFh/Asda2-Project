using System;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Privileges;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
    public class SetRoleCommand : RealmServerCommand
    {
        protected SetRoleCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        protected override void Initialize()
        {
            this.Init("SetRole", "Role", "SetPriv");
            this.EnglishParamInfo = "<RoleName>";
            this.EnglishDescription = "Sets the Account's Role which determines the User's rights and privileges.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            IUser user = trigger.Args.User;
            string roleGroupName = trigger.Text.NextWord();
            RoleGroup role = Singleton<PrivilegeMgr>.Instance.GetRole(roleGroupName);
            if ((object) role == null)
                trigger.Reply("Role \"{0}\" does not exist.", (object) roleGroupName);
            else if (user != null && user.Role <= role && user.Role < RoleGroupInfo.HighestRole.Rank)
            {
                trigger.Reply("You are not allowed to set the \"{0}\"-Role.", (object) role.Name);
            }
            else
            {
                Character chr = (Character) trigger.Args.Target;
                RoleGroup oldRole = chr.Account.Role;
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message((Action) (() =>
                {
                    if (chr.Account.SetRole(role))
                    {
                        chr.SendSystemMessage("You Role has changed from {0} to {1}.", (object) oldRole,
                            (object) role.Name);
                        trigger.Reply("{0}'s Account's ({1}) Role has changed from {2} to: {3}", (object) chr.Name,
                            (object) chr.Account, (object) oldRole, (object) role);
                    }
                    else
                        trigger.Reply("Role information could not be saved.");
                })));
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}