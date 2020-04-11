namespace WCell.RealmServer.Instances
{
    public abstract class InstanceSettings
    {
        protected InstanceSettings(BaseInstance instance)
        {
            this.Instance = instance;
        }

        public BaseInstance Instance { get; private set; }
    }
}