using System;
using System.Collections.Generic;

namespace WCell.Util.Threading.TaskParallel
{
    /// <summary>Extension methods for AggregateException.</summary>
    public static class AggregateExceptionExtensions
    {
        /// <summary>Invokes a handler on each Exception contained by this AggregateException.</summary>
        /// <param name="aggregateException">The AggregateException.</param>
        /// <param name="predicate">
        /// The predicate to execute for each exception. The predicate accepts as an argument the Exception
        /// to be processed and returns a Boolean to indicate whether the exception was handled.
        /// </param>
        /// <param name="leaveStructureIntact">
        /// Whether the rethrown AggregateException should maintain the same hierarchy as the original.
        /// </param>
        public static void Handle(this AggregateException aggregateException, Func<Exception, bool> predicate,
            bool leaveStructureIntact)
        {
            if (aggregateException == null)
                throw new ArgumentNullException(nameof(aggregateException));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if (leaveStructureIntact)
            {
                AggregateException aggregateException1 =
                    AggregateExceptionExtensions.HandleRecursively(aggregateException, predicate);
                if (aggregateException1 != null)
                    throw aggregateException1;
            }
            else
                aggregateException.Handle(predicate);
        }

        private static AggregateException HandleRecursively(AggregateException aggregateException,
            Func<Exception, bool> predicate)
        {
            List<Exception> exceptionList = (List<Exception>) null;
            foreach (Exception innerException in aggregateException.InnerExceptions)
            {
                AggregateException aggregateException1 = innerException as AggregateException;
                if (aggregateException1 != null)
                {
                    AggregateException aggregateException2 =
                        AggregateExceptionExtensions.HandleRecursively(aggregateException1, predicate);
                    if (aggregateException2 != null)
                    {
                        if (exceptionList != null)
                            exceptionList = new List<Exception>();
                        exceptionList.Add((Exception) aggregateException2);
                    }
                }
                else if (!predicate(innerException))
                {
                    if (exceptionList != null)
                        exceptionList = new List<Exception>();
                    exceptionList.Add(innerException);
                }
            }

            return exceptionList.Count > 0
                ? new AggregateException(aggregateException.Message, (IEnumerable<Exception>) exceptionList)
                : (AggregateException) null;
        }
    }
}