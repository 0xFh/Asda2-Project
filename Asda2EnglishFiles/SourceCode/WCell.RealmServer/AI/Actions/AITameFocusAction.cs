using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
    public class AITameFocusAction : AIFollowTargetAction
    {
        public AITameFocusAction(NPC owner)
            : base((Unit) owner)
        {
        }

        public override void Start()
        {
            Character currentTamer = ((NPC) this.m_owner).CurrentTamer;
            if (currentTamer == null)
            {
                this.Stop();
                this.m_owner.Brain.EnterDefaultState();
            }
            else
            {
                this.Target = (Unit) currentTamer;
                base.Start();
            }
        }

        public override void Update()
        {
            if (((NPC) this.m_owner).CurrentTamer == null)
            {
                this.Stop();
                this.m_owner.Brain.EnterDefaultState();
            }
            else
                base.Update();
        }

        public override void Stop()
        {
            ((NPC) this.m_owner).CurrentTamer = (Character) null;
            base.Stop();
        }
    }
}