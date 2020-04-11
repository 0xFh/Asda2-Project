using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WCell.Util.Threading.TaskParallel
{
    /// <summary>
    /// Provides LINQ support for Tasks by implementing the primary standard query operators.
    /// </summary>
    public static class LinqToTasks
    {
        public static Task<TResult> Select<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            return source.ContinueWith<TResult>((Func<Task<TSource>, TResult>) (t => selector(t.Result)),
                TaskContinuationOptions.NotOnCanceled);
        }

        public static Task<TResult> SelectMany<TSource, TResult>(this Task<TSource> source,
            Func<TSource, Task<TResult>> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            return source.ContinueWith<Task<TResult>>((Func<Task<TSource>, Task<TResult>>) (t => selector(t.Result)),
                TaskContinuationOptions.NotOnCanceled).Unwrap<TResult>();
        }

        public static Task<TResult> SelectMany<TSource, TCollection, TResult>(this Task<TSource> source,
            Func<TSource, Task<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (collectionSelector == null)
                throw new ArgumentNullException(nameof(collectionSelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));
            return source.ContinueWith<Task<TResult>>(
                    (Func<Task<TSource>, Task<TResult>>) (t =>
                        collectionSelector(t.Result).ContinueWith<TResult>(
                            (Func<Task<TCollection>, TResult>) (c => resultSelector(t.Result, c.Result)),
                            TaskContinuationOptions.NotOnCanceled)), TaskContinuationOptions.NotOnCanceled)
                .Unwrap<TResult>();
        }

        public static Task<TSource> Where<TSource>(this Task<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            CancellationTokenSource cts = new CancellationTokenSource();
            return source.ContinueWith<TSource>((Func<Task<TSource>, TSource>) (t =>
            {
                TSource result = t.Result;
                if (!predicate(result))
                    cts.CancelAndThrow();
                return result;
            }), cts.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);
        }

        public static Task<TResult> Join<TOuter, TInner, TKey, TResult>(this Task<TOuter> outer, Task<TInner> inner,
            Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.Join<TOuter, TInner, TKey, TResult>(inner, outerKeySelector, innerKeySelector, resultSelector,
                (IEqualityComparer<TKey>) EqualityComparer<TKey>.Default);
        }

        public static Task<TResult> Join<TOuter, TInner, TKey, TResult>(this Task<TOuter> outer, Task<TInner> inner,
            Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null)
                throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw new ArgumentNullException(nameof(innerKeySelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            return outer.ContinueWith<Task<TResult>>((Func<Task<TOuter>, Task<TResult>>) (param0_1 =>
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                return inner.ContinueWith<TResult>((Func<Task<TInner>, TResult>) (param0_2 =>
                {
                    Task.WaitAll(new Task[2]
                    {
                        (Task) outer,
                        (Task) inner
                    });
                    if (comparer.Equals(outerKeySelector(outer.Result), innerKeySelector(inner.Result)))
                        return resultSelector(outer.Result, inner.Result);
                    cts.CancelAndThrow();
                    return default(TResult);
                }), cts.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);
            }), TaskContinuationOptions.NotOnCanceled).Unwrap<TResult>();
        }

        public static Task<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this Task<TOuter> outer,
            Task<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, Task<TInner>, TResult> resultSelector)
        {
            return outer.GroupJoin<TOuter, TInner, TKey, TResult>(inner, outerKeySelector, innerKeySelector,
                resultSelector, (IEqualityComparer<TKey>) EqualityComparer<TKey>.Default);
        }

        public static Task<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this Task<TOuter> outer,
            Task<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, Task<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null)
                throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw new ArgumentNullException(nameof(innerKeySelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            return outer.ContinueWith<Task<TResult>>((Func<Task<TOuter>, Task<TResult>>) (param0_1 =>
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                return inner.ContinueWith<TResult>((Func<Task<TInner>, TResult>) (param0_2 =>
                {
                    Task.WaitAll(new Task[2]
                    {
                        (Task) outer,
                        (Task) inner
                    });
                    if (comparer.Equals(outerKeySelector(outer.Result), innerKeySelector(inner.Result)))
                        return resultSelector(outer.Result, inner);
                    cts.CancelAndThrow();
                    return default(TResult);
                }), cts.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);
            }), TaskContinuationOptions.NotOnCanceled).Unwrap<TResult>();
        }

        public static Task<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this Task<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));
            return source.ContinueWith<IGrouping<TKey, TElement>>((Func<Task<TSource>, IGrouping<TKey, TElement>>) (t =>
            {
                TSource result = t.Result;
                TKey key = keySelector(result);
                TElement element = elementSelector(result);
                return (IGrouping<TKey, TElement>) new LinqToTasks.OneElementGrouping<TKey, TElement>()
                {
                    Key = key,
                    Element = element
                };
            }), TaskContinuationOptions.NotOnCanceled);
        }

        public static Task<TSource> OrderBy<TSource, TKey>(this Task<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source;
        }

        public static Task<TSource> OrderByDescending<TSource, TKey>(this Task<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source;
        }

        public static Task<TSource> ThenBy<TSource, TKey>(this Task<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source;
        }

        public static Task<TSource> ThenByDescending<TSource, TKey>(this Task<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source;
        }

        /// <summary>Represents a grouping of one element.</summary>
        /// <typeparam name="TKey">The type of the key for the element.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        private class OneElementGrouping<TKey, TElement> : IGrouping<TKey, TElement>, IEnumerable<TElement>, IEnumerable
        {
            public TKey Key { get; internal set; }

            internal TElement Element { get; set; }

            public IEnumerator<TElement> GetEnumerator()
            {
                yield return this.Element;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator) this.GetEnumerator();
            }
        }
    }
}