using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.Help.Tickets;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Privileges;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
    public class RealmServerCmdArgs : ICmdArgs
    {
        protected bool m_dbl;
        protected IUser m_user;
        protected Character m_chr;
        protected IGenericChatTarget m_chatTarget;
        private Unit m_target;

        public RealmServerCmdArgs(RealmServerCmdArgs args)
        {
            this.m_user = args.m_user;
            this.m_chr = args.Character;
            this.m_chatTarget = args.m_chatTarget;
            this.Role = args.Role;
            this.m_dbl = args.m_dbl;
            this.InitArgs();
        }

        public RealmServerCmdArgs(IUser user, bool dbl, IGenericChatTarget chatTarget)
        {
            this.m_user = user;
            this.m_chr = this.m_user as Character;
            this.m_chatTarget = chatTarget;
            this.Role = this.m_user != null ? this.m_user.Role : Singleton<PrivilegeMgr>.Instance.HighestRole;
            this.m_dbl = dbl;
            this.InitArgs();
        }

        private void InitArgs()
        {
            this.Context = (IContextHandler) this.m_chr;
            this.SetTarget();
        }

        /// <summary>Whether there was a double trigger-prefix</summary>
        public bool Double
        {
            get { return this.m_dbl; }
            set
            {
                this.m_dbl = value;
                this.SetTarget();
            }
        }

        /// <summary>
        /// The context for the Command.
        /// This is the Character's Context by default.
        /// Make sure to treat all Object involved properly.
        /// </summary>
        public IContextHandler Context { get; set; }

        /// <summary>Only available if IngameOnly = true</summary>
        public Unit Target
        {
            get { return this.m_target; }
            set { this.m_target = value; }
        }

        public WorldObject SelectedUnitOrGO
        {
            get
            {
                Character character = this.Character;
                if (character == null)
                    return (WorldObject) null;
                return (WorldObject) character.Target ?? (WorldObject) character.ExtraInfo.SelectedGO;
            }
        }

        /// <summary>
        /// The Channel, Group, Guild or person that the Command was directed to (if any)
        /// </summary>
        public IGenericChatTarget ChatTarget
        {
            get { return this.m_chatTarget; }
        }

        public bool HasCharacter
        {
            get { return this.Character != null; }
        }

        /// <summary>
        /// The Character who is performing this or null (if not ingame-triggered)
        /// </summary>
        public Character Character
        {
            get
            {
                if (this.m_chr == null || !this.m_chr.IsInWorld)
                    return (Character) null;
                return this.m_chr;
            }
            set
            {
                this.m_chr = value;
                this.Context = (IContextHandler) value;
                this.SetTarget();
            }
        }

        /// <summary>
        /// The User who triggered the Command (might be null if used from Console etc)
        /// </summary>
        public IUser User
        {
            get { return this.m_user; }
            set { this.m_user = value; }
        }

        /// <summary>The triggering User or null (if not ingame-triggered)</summary>
        public RoleGroup Role { get; private set; }

        /// <summary>The GO that is currently selected by the User</summary>
        public GameObject SelectedGO
        {
            get
            {
                if (!(this.m_user is Character))
                    return (GameObject) null;
                return GOSelectMgr.Instance[this.Character];
            }
        }

        /// <summary>
        /// The TicketHandler triggering this Trigger (or null if there is none).
        /// </summary>
        public virtual ITicketHandler TicketHandler
        {
            get { return this.m_user as ITicketHandler; }
        }

        public T GetTarget<T>() where T : Unit
        {
            Unit target = this.Target;
            if (target is T)
                return (T) target;
            if (this.Character != null)
                return (T) this.Character.Target;
            return default(T);
        }

        private void SetTarget()
        {
            if (this.Character == null)
                return;
            if (this.Double)
                this.m_target = this.Character.Target;
            else
                this.m_target = (Unit) this.Character;
        }

        /// <summary>
        /// Whether this the supplied arguments match the specified target criteria.
        /// </summary>
        public bool CheckArgs(Command<RealmServerCmdArgs> cmd)
        {
            if (!(cmd is RealmServerCommand))
                return true;
            RealmServerCommand realmServerCommand = (RealmServerCommand) cmd;
            Unit target = this.Target;
            ObjectTypeCustom targetTypes = realmServerCommand.TargetTypes;
            if (realmServerCommand.RequiresCharacter && this.m_chr == null)
                return false;
            if (targetTypes == ObjectTypeCustom.None)
                return true;
            if (target != null)
                return targetTypes.HasAnyFlag(target.CustomType);
            return false;
        }

        /// <summary>
        /// If the trigger has anymore text, it will get the Character whose name matches the next word, else
        /// takes the current Target.
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public Character GetCharArgumentOrTarget(CmdTrigger<RealmServerCmdArgs> trigger, string playerName)
        {
            Character character;
            if (playerName.Length > 0)
            {
                character = World.GetCharacter(playerName, false);
                if (character == null)
                {
                    trigger.Reply("Character {0} does not exist or is offline.", (object) playerName);
                    return (Character) null;
                }
            }
            else
                character = (Character) trigger.Args.Target;

            return character;
        }
    }
}