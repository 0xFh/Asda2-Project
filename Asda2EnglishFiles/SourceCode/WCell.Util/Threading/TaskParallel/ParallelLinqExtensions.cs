using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WCell.Util.Threading.TaskParallel
{
    /// <summary>Extension methods for Parallel LINQ.</summary>
    public static class ParallelLinqExtensions
    {
        /// <summary>Takes the top elements as if they were sorted.</summary>
        /// <typeparam name="TSource">Specifies the type of the elements.</typeparam>
        /// <typeparam name="TKey">Specifies the type of the keys used to compare elements.</typeparam>
        /// <param name="source">The source elements.</param>
        /// <param name="keySelector">A function used to extract a key from each element.</param>
        /// <param name="count">The number of elements to take.</param>
        /// <returns></returns>
        public static IEnumerable<TSource> TakeTop<TSource, TKey>(this ParallelQuery<TSource> source,
            Func<TSource, TKey> keySelector, int count)
        {
            ParallelLinqExtensions.DescendingDefaultComparer<TKey> comparer =
                new ParallelLinqExtensions.DescendingDefaultComparer<TKey>();
            return source.Aggregate<TSource, SortedTopN<TKey, TSource>, IEnumerable<TSource>>(
                (Func<SortedTopN<TKey, TSource>>) (() =>
                    new SortedTopN<TKey, TSource>(count, (IComparer<TKey>) comparer)),
                (Func<SortedTopN<TKey, TSource>, TSource, SortedTopN<TKey, TSource>>) ((accum, item) =>
                {
                    accum.Add(keySelector(item), item);
                    return accum;
                }), (Func<SortedTopN<TKey, TSource>, SortedTopN<TKey, TSource>, SortedTopN<TKey, TSource>>) ((accum1,
                    accum2) =>
                {
                    foreach (KeyValuePair<TKey, TSource> keyValuePair in accum2)
                        accum1.Add(keyValuePair);
                    return accum1;
                }), (Func<SortedTopN<TKey, TSource>, IEnumerable<TSource>>) (accum => accum.Values));
        }

        /// <summary>Implements a map-reduce operation.</summary>
        /// <typeparam name="TSource">Specifies the type of the source elements.</typeparam>
        /// <typeparam name="TMapped">Specifies the type of the mapped elements.</typeparam>
        /// <typeparam name="TKey">Specifies the type of the element keys.</typeparam>
        /// <typeparam name="TResult">Specifies the type of the results.</typeparam>
        /// <param name="source">The source elements.</param>
        /// <param name="map">A function used to get the target data from a source element.</param>
        /// <param name="keySelector">A function used to get a key from the target data.</param>
        /// <param name="reduce">A function used to reduce a group of elements.</param>
        /// <returns>The result elements of the reductions.</returns>
        public static ParallelQuery<TResult> MapReduce<TSource, TMapped, TKey, TResult>(
            this ParallelQuery<TSource> source, Func<TSource, TMapped> map, Func<TMapped, TKey> keySelector,
            Func<IGrouping<TKey, TMapped>, TResult> reduce)
        {
            return ParallelEnumerable.Select<TSource, TMapped>(source, map).GroupBy<TMapped, TKey>(keySelector)
                .Select<IGrouping<TKey, TMapped>, TResult>(reduce);
        }

        /// <summary>Implements a map-reduce operation.</summary>
        /// <typeparam name="TSource">Specifies the type of the source elements.</typeparam>
        /// <typeparam name="TMapped">Specifies the type of the mapped elements.</typeparam>
        /// <typeparam name="TKey">Specifies the type of the element keys.</typeparam>
        /// <typeparam name="TResult">Specifies the type of the results.</typeparam>
        /// <param name="source">The source elements.</param>
        /// <param name="map">A function used to get an enumerable of target data from a source element.</param>
        /// <param name="keySelector">A function used to get a key from target data.</param>
        /// <param name="reduce">A function used to reduce a group of elements to an enumerable of results.</param>
        /// <returns>The result elements of the reductions.</returns>
        public static ParallelQuery<TResult> MapReduce<TSource, TMapped, TKey, TResult>(
            this ParallelQuery<TSource> source, Func<TSource, IEnumerable<TMapped>> map,
            Func<TMapped, TKey> keySelector, Func<IGrouping<TKey, TMapped>, IEnumerable<TResult>> reduce)
        {
            return ParallelEnumerable.SelectMany<TSource, TMapped>(source, map).GroupBy<TMapped, TKey>(keySelector)
                .SelectMany<IGrouping<TKey, TMapped>, TResult>(reduce);
        }

        /// <summary>Runs the query and outputs its results into the target collection.</summary>
        /// <typeparam name="TSource">Specifies the type of elements output from the query.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="target">The target collection.</param>
        public static void OutputToProducerConsumerCollection<TSource>(this ParallelQuery<TSource> source,
            IProducerConsumerCollection<TSource> target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            source.ForAll<TSource>((Action<TSource>) (item => target.TryAdd(item)));
        }

        /// <summary>This is the method to opt into Parallel LINQ.</summary>
        /// <typeparam name="TSource">Specifies the type of elements provided to the query.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="parallelOptions">The options to use for query processing.</param>
        /// <returns>The source as a ParallelQuery to bind to ParallelEnumerable extension methods.</returns>
        public static ParallelQuery<TSource> AsParallel<TSource>(this IEnumerable<TSource> source,
            ParallelLinqOptions parallelOptions)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (parallelOptions.TaskScheduler != null && parallelOptions.TaskScheduler != TaskScheduler.Default)
                throw new ArgumentException("Parallel LINQ only supports the default TaskScheduler.");
            ParallelQuery<TSource> source1 = source.AsParallel<TSource>();
            if (parallelOptions.Ordered)
                source1 = source1.AsOrdered<TSource>();
            if (parallelOptions.CancellationToken.CanBeCanceled)
                source1 = source1.WithCancellation<TSource>(parallelOptions.CancellationToken);
            if (parallelOptions.MaxDegreeOfParallelism >= 1)
                source1 = source1.WithDegreeOfParallelism<TSource>(parallelOptions.MaxDegreeOfParallelism);
            if (parallelOptions.ExecutionMode != ParallelExecutionMode.Default)
                source1 = source1.WithExecutionMode<TSource>(parallelOptions.ExecutionMode);
            if (parallelOptions.MergeOptions != ParallelMergeOptions.Default)
                source1 = source1.WithMergeOptions<TSource>(parallelOptions.MergeOptions);
            return source1;
        }

        /// <summary>A comparer that comparers using the inverse of the default comparer.</summary>
        /// <typeparam name="T">Specifies the type being compared.</typeparam>
        private class DescendingDefaultComparer<T> : IComparer<T>
        {
            private static Comparer<T> _defaultComparer = Comparer<T>.Default;

            public int Compare(T x, T y)
            {
                return ParallelLinqExtensions.DescendingDefaultComparer<T>._defaultComparer.Compare(y, x);
            }
        }
    }
}