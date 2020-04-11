using NLog;
using System;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.Util.Collections;

namespace WCell.RealmServer.Battlegrounds
{
    /// <summary>
    /// A <see cref="T:WCell.RealmServer.Battlegrounds.GlobalBattlegroundQueue" /> contains all instances of a particular level-range
    /// of one <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundTemplate" />.
    /// </summary>
    public class GlobalBattlegroundQueue : BattlegroundQueue
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static int defaultBGCreationPlayerThresholdPct = 80;

        /// <summary>
        /// All <see cref="P:WCell.RealmServer.Battlegrounds.GlobalBattlegroundQueue.Battleground" />-instances of this queue's <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundTemplate" />
        /// </summary>
        public readonly ImmutableList<Battleground> Instances = new ImmutableList<Battleground>();

        private int m_CreationPlayerThreshold;

        /// <summary>
        /// A new BG is created and invitation will start once the queue contains at least this percentage
        /// of the BG's max player limit.
        /// </summary>
        public static int DefaultBGCreationPlayerThresholdPct
        {
            get { return GlobalBattlegroundQueue.defaultBGCreationPlayerThresholdPct; }
            set
            {
                GlobalBattlegroundQueue.defaultBGCreationPlayerThresholdPct = value;
                foreach (BattlegroundTemplate template in BattlegroundMgr.Templates)
                {
                    if (template != null)
                    {
                        foreach (GlobalBattlegroundQueue queue in template.Queues)
                        {
                            if (queue != null)
                                queue.SetThreshold();
                        }
                    }
                }
            }
        }

        public GlobalBattlegroundQueue(BattlegroundId bgid)
            : this(BattlegroundMgr.GetTemplate(bgid))
        {
        }

        public GlobalBattlegroundQueue(BattlegroundTemplate template)
            : this(template, 0, 0, RealmServerConfiguration.MaxCharacterLevel)
        {
        }

        public GlobalBattlegroundQueue(BattlegroundTemplate template, int lvlBracket, int minLevel, int maxLevel)
            : base(template, lvlBracket, minLevel, maxLevel)
        {
            this.SetThreshold();
        }

        protected override BattlegroundTeamQueue CreateTeamQueue(BattlegroundSide side)
        {
            return (BattlegroundTeamQueue) new GlobalBGTeamQueue(this, side);
        }

        private void SetThreshold()
        {
            this.m_CreationPlayerThreshold = this.Template.MapTemplate.MaxPlayerCount *
                                             GlobalBattlegroundQueue.defaultBGCreationPlayerThresholdPct / 100;
        }

        public int CreationPlayerThreshold
        {
            get { return this.m_CreationPlayerThreshold; }
        }

        public int CharacterCount
        {
            get
            {
                int num = 0;
                foreach (BattlegroundTeamQueue teamQueue in this.TeamQueues)
                    num += teamQueue.CharacterCount;
                return num;
            }
        }

        public override bool RequiresLocking
        {
            get { return true; }
        }

        public Battleground GetBattleground(uint instanceId)
        {
            foreach (Battleground instance in this.Instances)
            {
                if ((int) instance.InstanceId == (int) instanceId)
                    return instance;
            }

            return (Battleground) null;
        }

        public void CheckBGCreation()
        {
            if (this.CharacterCount < this.CreationPlayerThreshold)
                return;
            this.CreateBattleground();
        }

        private bool CheckBGRequirements()
        {
            if (GOMgr.Loaded)
                return true;
            GlobalBattlegroundQueue.log.Warn("Tried to create Battleground without GOs loaded.");
            return false;
        }

        /// <summary>Make sure to load GOs before calling this method</summary>
        /// <returns></returns>
        public Battleground CreateBattleground()
        {
            if (!this.CheckBGRequirements() || this.m_Template.Creator == null)
                return (Battleground) null;
            Battleground bg = this.m_Template.Creator();
            this.InitBG(bg);
            return bg;
        }

        /// <summary>Make sure to load GOs before calling this method</summary>
        /// <typeparam name="B"></typeparam>
        /// <returns></returns>
        public B CreateBattleground<B>() where B : Battleground, new()
        {
            if (!this.CheckBGRequirements())
                return default(B);
            B instance = Activator.CreateInstance<B>();
            this.InitBG((Battleground) instance);
            return instance;
        }

        private void InitBG(Battleground bg)
        {
            this.Instances.Add(bg);
            bg.ParentQueue = this;
            bg.InitMap(this.m_Template.MapTemplate);
        }

        /// <summary>Enqueues the given Character(s) for the given side</summary>
        public BattlegroundRelation Enqueue(ICharacterSet chrs, BattlegroundSide side)
        {
            BattlegroundTeamQueue teamQueue = this.GetTeamQueue(side);
            BattlegroundRelation request = new BattlegroundRelation(teamQueue, chrs);
            teamQueue.Enqueue(request);
            return request;
        }

        internal void OnRemove(Battleground bg)
        {
            this.Instances.Remove(bg);
        }

        public override Battleground Battleground
        {
            get { return (Battleground) null; }
        }

        protected internal override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}