using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WCell.Util;

namespace WCell.RealmServer.Global
{
    public class WorldInstanceCollection<TE, TM> where TE : struct, IConvertible where TM : InstancedMap
    {
        private readonly ReaderWriterLockSlim _lck = new ReaderWriterLockSlim();
        internal TM[][] Instances;
        private int _count;

        public WorldInstanceCollection(TE size)
        {
            this.Instances = new TM[size.ToInt32((IFormatProvider) null)][];
        }

        public int Count
        {
            get { return this._count; }
        }

        /// <summary>Gets an instance</summary>
        /// <returns>the <see cref="T:WCell.RealmServer.Global.Map" /> object; null if the ID is not valid</returns>
        /// s
        public TM GetInstance(TE mapId, uint instanceId)
        {
            TM[] arr = this.Instances.Get<TM[]>(mapId.ToUInt32((IFormatProvider) null));
            if (arr == null)
                return default(TM);
            return arr.Get<TM>(instanceId);
        }

        public TM[] GetInstances(TE map)
        {
            TM[] mArray = this.Instances.Get<TM[]>(map.ToUInt32((IFormatProvider) null));
            if (mArray == null)
                return new TM[0];
            return ((IEnumerable<TM>) mArray).Where<TM>((Func<TM, bool>) (instance => (object) instance != null))
                .ToArray<TM>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <remarks>Never returns null</remarks>
        public TM[] GetOrCreateInstances(TE map)
        {
            TM[] mArray = this.Instances.Get<TM[]>(map.ToUInt32((IFormatProvider) null));
            if (mArray != null)
                return mArray;
            this._lck.EnterWriteLock();
            try
            {
                this.Instances[map.ToUInt32((IFormatProvider) null)] = mArray = new TM[10];
            }
            finally
            {
                this._lck.ExitWriteLock();
            }

            return mArray;
        }

        public List<TM> GetAllInstances()
        {
            List<TM> mList = new List<TM>();
            this._lck.EnterReadLock();
            try
            {
                foreach (TM[] instance1 in this.Instances)
                {
                    if (instance1 != null)
                        mList.AddRange(
                            ((IEnumerable<TM>) instance1).Where<TM>((Func<TM, bool>) (instance =>
                                (object) instance != null)));
                }
            }
            finally
            {
                this._lck.ExitReadLock();
            }

            return mList;
        }

        internal void AddInstance(TE id, TM map)
        {
            TM[] instances = this.GetOrCreateInstances(id);
            if ((long) map.InstanceId >= (long) instances.Length)
            {
                this._lck.EnterWriteLock();
                try
                {
                    instances = this.GetOrCreateInstances(id);
                    Array.Resize<TM>(ref instances, (int) ((double) map.InstanceId * 1.5));
                    this.Instances[id.ToUInt32((IFormatProvider) null)] = instances;
                }
                finally
                {
                    this._lck.ExitWriteLock();
                }
            }

            instances[map.InstanceId] = map;
            Interlocked.Increment(ref this._count);
        }

        internal void RemoveInstance(TE mapId, uint instanceId)
        {
            this._lck.EnterWriteLock();
            try
            {
                this.GetOrCreateInstances(mapId)[instanceId] = default(TM);
                --this._count;
            }
            finally
            {
                this._lck.ExitWriteLock();
            }
        }
    }
}