using System;

namespace WCell.Core.Initialization
{
    public class InitializationDependency
    {
        private string m_Name;
        private Type m_DependentType;

        public InitializationDependency(DependentInitializationAttribute attr)
        {
            this.m_Name = attr.Name;
            this.m_DependentType = attr.DependentType;
        }

        public string Name
        {
            get { return this.m_Name; }
        }

        public Type DependentType
        {
            get { return this.m_DependentType; }
        }

        public GlobalMgrInfo DependentMgr { get; internal set; }
    }
}