using WCell.RealmServer.Global;

namespace WCell.RealmServer.Instances
{
    public class InstanceTemplate
    {
        private readonly MapTemplate m_MapTemplate;
        public InstanceCreator Creator;

        public InstanceTemplate(MapTemplate template)
        {
            this.m_MapTemplate = template;
        }

        public MapTemplate MapTemplate
        {
            get { return this.m_MapTemplate; }
        }

        internal BaseInstance Create()
        {
            if (this.Creator != null)
                return this.Creator();
            return (BaseInstance) null;
        }
    }
}