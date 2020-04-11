using System;
using System.Threading;

namespace WCell.Util.Collections
{
    /// <summary>
    /// Represents a lock-free, thread-safe, last-in, first-out collection of objects.
    /// </summary>
    /// <typeparam name="T">specifies the type of the elements in the stack</typeparam>
    public class LockFreeStack<T>
    {
        private SingleLinkNode<T> _head;

        /// <summary>Default constructors.</summary>
        public LockFreeStack()
        {
            this._head = new SingleLinkNode<T>();
        }

        /// <summary>Inserts an object at the top of the stack.</summary>
        /// <param name="item">the object to push onto the stack</param>
        public void Push(T item)
        {
            SingleLinkNode<T> singleLinkNode = new SingleLinkNode<T>();
            singleLinkNode.Item = item;
            do
            {
                singleLinkNode.Next = this._head.Next;
            } while (Interlocked.CompareExchange<SingleLinkNode<T>>(ref this._head.Next, singleLinkNode,
                         singleLinkNode.Next) != singleLinkNode.Next);
        }

        /// <summary>
        /// Removes and returns the object at the top of the stack.
        /// </summary>
        /// <param name="item">
        /// when the method returns, contains the object removed from the top of the stack,
        /// if the queue is not empty; otherwise it is the default value for the element type
        /// </param>
        /// <returns>
        /// true if an object from removed from the top of the stack
        /// false if the stack is empty
        /// </returns>
        public bool Pop(out T item)
        {
            SingleLinkNode<T> next;
            do
            {
                next = this._head.Next;
                if (next == null)
                {
                    item = default(T);
                    return false;
                }
            } while (Interlocked.CompareExchange<SingleLinkNode<T>>(ref this._head.Next, next.Next, next) != next);

            item = next.Item;
            return true;
        }

        /// <summary>
        /// Removes and returns the object at the top of the stack.
        /// </summary>
        /// <returns>the object that is removed from the top of the stack</returns>
        public T Pop()
        {
            T obj;
            if (!this.Pop(out obj))
                throw new InvalidOperationException("the stack is empty");
            return obj;
        }
    }
}