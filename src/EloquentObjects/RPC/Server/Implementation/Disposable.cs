using System;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class Disposable : IDisposable
    {
        private Action _disposeAction;

        public Disposable(Action disposeAction)
        {
            _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));
        }
        
        #region IDisposable

        public void Dispose()
        {
            if (_disposeAction == null)
                throw new ObjectDisposedException(nameof(Disposable));
            _disposeAction();
            _disposeAction = null;
        }

        #endregion
    }
}