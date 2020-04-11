using System;
using System.Collections.Generic;
using WCell.Util.ObjectPools;

namespace WCell.RealmServer.Misc
{
    public static class ObjectPoolMgr
    {
        public static readonly List<IObjectPool> Pools = new List<IObjectPool>();

        public static ObjectPool<T> CreatePool<T>(Func<T> creator) where T : class
        {
            return ObjectPoolMgr.CreatePool<T>(creator, false);
        }

        public static ObjectPool<T> CreatePool<T>(Func<T> creator, bool isBalanced) where T : class
        {
            ObjectPool<T> objectPool = new ObjectPool<T>(creator, isBalanced);
            ObjectPoolMgr.Pools.Add((IObjectPool) objectPool);
            return objectPool;
        }
    }
}