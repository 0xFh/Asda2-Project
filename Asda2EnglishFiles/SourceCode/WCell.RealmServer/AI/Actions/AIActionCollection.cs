using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
    public class AIActionCollection : IAIActionCollection
    {
        public readonly AIAction[] Actions = new AIAction[12];
        protected Unit m_owner;

        public Unit Owner
        {
            get { return this.m_owner; }
        }

        public bool IsInitialized
        {
            get { return this.m_owner != null; }
        }

        public AIAction this[BrainState state]
        {
            get { return this.Actions[(int) state]; }
            set
            {
                AIAction action = this.Actions[(int) state];
                if (action == value)
                    return;
                this.Actions[(int) state] = value;
                IBrain brain = this.m_owner.Brain;
                if (brain == null || brain.State != state || brain.CurrentAction != action)
                    return;
                brain.CurrentAction = (IAIAction) value;
            }
        }

        public virtual void Init(Unit owner)
        {
            this.m_owner = owner;
        }
    }
}