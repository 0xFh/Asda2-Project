namespace WCell.Core
{
    /// <summary>Private class for instances of a singleton object.</summary>
    /// <typeparam name="TSingle">the type of the singleton object</typeparam>
    internal static class SingletonHolder<TSingle> where TSingle : new()
    {
        internal static readonly TSingle Instance = new TSingle();
    }
}