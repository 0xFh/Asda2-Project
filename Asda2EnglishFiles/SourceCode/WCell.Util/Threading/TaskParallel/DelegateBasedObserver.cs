using System;

namespace WCell.Util.Threading.TaskParallel
{
    internal class DelegateBasedObserver<T> : IObserver<T>
    {
        private Action<T> _onNext;
        private Action<Exception> _onError;
        private Action _onCompleted;

        internal DelegateBasedObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            if (onNext == null)
                throw new ArgumentNullException(nameof(onNext));
            if (onError == null)
                throw new ArgumentNullException(nameof(onError));
            if (onCompleted == null)
                throw new ArgumentNullException(nameof(onCompleted));
            this._onNext = onNext;
            this._onError = onError;
            this._onCompleted = onCompleted;
        }

        public void OnCompleted()
        {
            this._onCompleted();
        }

        public void OnError(Exception error)
        {
            this._onError(error);
        }

        public void OnNext(T value)
        {
            this._onNext(value);
        }
    }
}