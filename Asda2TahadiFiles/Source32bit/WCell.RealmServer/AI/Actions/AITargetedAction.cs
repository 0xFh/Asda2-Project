using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.AI.Actions
{
    /// <summary>
    /// Abstract class representing an AI Action that has a target
    /// (casting spell for example)
    /// </summary>
    public abstract class AITargetedAction : AIAction
    {
        protected SimpleRange m_range;

        protected AITargetedAction(Unit owner)
            : base(owner)
        {
        }

        /// <summary>Range in which the action can be executed</summary>
        public SimpleRange Range
        {
            get { return this.m_range; }
            set { this.m_range = value; }
        }
    }
}