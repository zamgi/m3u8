using System.Threading;

namespace m3u8.download.manager.controllers
{
    /// <summary>
    /// 
    /// </summary>
    internal struct interlocked_lock
    {
        private int _Lock;
        public bool TryEnter() => (Interlocked.CompareExchange( ref _Lock, 1, 0 ) == 0);
        public void Exit() => Interlocked.Exchange( ref _Lock, 0 );
    }
}
