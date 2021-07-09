using System;
using System.Threading;

namespace GuimoSoft.Cache.Utils
{
    internal sealed class DisposableLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;

        public DisposableLock(ReaderWriterLockSlim @lock)
        {
            _lock = @lock;
            _lock.EnterWriteLock();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _lock.ExitWriteLock();
        }
    }
}
