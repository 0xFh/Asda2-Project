using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WCell.Util.Threading.TaskParallel
{
    /// <summary>Parallel extensions for the Delegate class.</summary>
    public static class DelegateExtensions
    {
        /// <summary>Dynamically invokes (late-bound) in parallel the methods represented by the delegate.</summary>
        /// <param name="multicastDelegate">The delegate to be invoked.</param>
        /// <param name="args">An array of objects that are the arguments to pass to the delegates.</param>
        /// <returns>The return value of one of the delegate invocations.</returns>
        public static object ParallelDynamicInvoke(this Delegate multicastDelegate, params object[] args)
        {
            if ((object) multicastDelegate == null)
                throw new ArgumentNullException(nameof(multicastDelegate));
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            return ((IEnumerable<Delegate>) multicastDelegate.GetInvocationList()).AsParallel<Delegate>()
                .AsOrdered<Delegate>().Select<Delegate, object>((Func<Delegate, object>) (d => d.DynamicInvoke(args)))
                .Last<object>();
        }

        /// <summary>
        /// Provides a delegate that runs the specified action and fails fast if the action throws an exception.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <returns>The wrapper delegate.</returns>
        public static Action WithFailFast(this Action action)
        {
            return (Action) (() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    else
                        Environment.FailFast("An unhandled exception occurred.", ex);
                }
            });
        }

        /// <summary>
        /// Provides a delegate that runs the specified function and fails fast if the function throws an exception.
        /// </summary>
        /// <param name="function">The function to invoke.</param>
        /// <returns>The wrapper delegate.</returns>
        public static Func<T> WithFailFast<T>(this Func<T> function)
        {
            return (Func<T>) (() =>
            {
                try
                {
                    return function();
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    else
                        Environment.FailFast("An unhandled exception occurred.", ex);
                }

                throw new Exception("Will never get here");
            });
        }
    }
}