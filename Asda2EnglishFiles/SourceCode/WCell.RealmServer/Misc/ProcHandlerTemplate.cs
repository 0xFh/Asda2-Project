using WCell.Constants.Spells;

namespace WCell.RealmServer.Misc
{
    /// <summary>Default implementation for IProcHandler</summary>
    public class ProcHandlerTemplate
    {
        protected int m_stackCount;

        protected ProcHandlerTemplate()
        {
        }

        public ProcHandlerTemplate(ProcTriggerFlags triggerFlags, ProcHitFlags hitFlags, ProcCallback procAction,
            uint procChance = 100, int stackCount = 0)
        {
            this.ProcTriggerFlags = triggerFlags;
            this.ProcHitFlags = hitFlags;
            this.ProcChance = procChance;
            this.Validator = (ProcValidator) null;
            this.ProcAction = procAction;
            this.m_stackCount = stackCount;
        }

        public ProcHandlerTemplate(ProcTriggerFlags triggerFlags, ProcHitFlags hitFlags, ProcCallback procAction,
            ProcValidator validator = null, uint procChance = 100, int stackCount = 0)
        {
            this.ProcTriggerFlags = triggerFlags;
            this.ProcHitFlags = hitFlags;
            this.ProcChance = procChance;
            this.Validator = validator;
            this.ProcAction = procAction;
            this.m_stackCount = stackCount;
        }

        public ProcValidator Validator { get; set; }

        public ProcCallback ProcAction { get; set; }

        /// <summary>The amount of times that this Aura has been applied</summary>
        public int StackCount
        {
            get { return this.m_stackCount; }
            set { this.m_stackCount = value; }
        }

        public ProcTriggerFlags ProcTriggerFlags { get; set; }

        public ProcHitFlags ProcHitFlags { get; set; }

        /// <summary>Chance to proc in %</summary>
        public uint ProcChance { get; set; }

        /// <summary>In Milliseconds</summary>
        public int MinProcDelay { get; set; }
    }
}