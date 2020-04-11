namespace WCell.Util.ObjectPools
{
    /// <summary>
    /// A structure that contains information about an object pool.
    /// </summary>
    public struct ObjectPoolInfo
    {
        /// <summary>The number of hard references contained in the pool.</summary>
        public int HardReferences;

        /// <summary>The number of weak references contained in the pool.</summary>
        public int WeakReferences;

        /// <summary>Constructor</summary>
        /// <param name="weak">The number of weak references in the pool.</param>
        /// <param name="hard">The number of hard references in the pool.</param>
        public ObjectPoolInfo(int weak, int hard)
        {
            this.HardReferences = hard;
            this.WeakReferences = weak;
        }
    }
}