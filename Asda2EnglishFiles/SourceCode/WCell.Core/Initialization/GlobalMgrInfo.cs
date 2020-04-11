using System.Collections.Generic;

namespace WCell.Core.Initialization
{
    public class GlobalMgrInfo
    {
        public readonly List<DependentInitializationStep> Dependencies = new List<DependentInitializationStep>(5);
        private bool m_isInitialized;

        public bool IsInitialized
        {
            get { return this.m_isInitialized; }
            internal set { this.m_isInitialized = true; }
        }
    }
}