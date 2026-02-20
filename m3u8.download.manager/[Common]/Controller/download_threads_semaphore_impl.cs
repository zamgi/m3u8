using System;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.controllers
{
    #region [.SemaphoreHolder.]
    /// <summary>
    /// 
    /// </summary>
    internal sealed class SemaphoreHolder : IDisposable
    {
        private volatile SemaphoreSlim _Semaphore;
        private ReaderWriterLockSlim   _RWLS;

        public SemaphoreHolder( SemaphoreSlim semaphore, bool useCrossDownloadInstanceParallelism )
        {
            _Semaphore = semaphore;
            _RWLS      = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );
            UseCrossDownloadInstanceParallelism = useCrossDownloadInstanceParallelism;
        }
        public void Dispose()
        {
            if ( _RWLS != null )
            {
                ResetSemaphore( null );

                _RWLS.Dispose();
                _RWLS = null;
            }
        }

        public bool UseCrossDownloadInstanceParallelism { [M(O.AggressiveInlining)] get; }
        public bool HasSemaphore
        {
            [M(O.AggressiveInlining)] get
            {
                bool hasSemaphore;
                _RWLS.EnterReadLock();
                {
                    hasSemaphore = (_Semaphore != null);
                }
                _RWLS.ExitReadLock();
                return (hasSemaphore);
            }
        }

        [M(O.AggressiveInlining)] public void ResetSemaphore( SemaphoreSlim semaphore )
        {
            _RWLS.EnterWriteLock();
            {
                _Semaphore = semaphore;
            }
            _RWLS.ExitWriteLock();
        }

        [M(O.AggressiveInlining)] public void Wait( CancellationToken ct )
        {
            SemaphoreSlim semaphore;
            _RWLS.EnterReadLock();
            {
                semaphore = _Semaphore;
            }
            _RWLS.ExitReadLock();

            semaphore.Wait( ct );  //thrown 'System.OperationCanceledException'
        }
        [M(O.AggressiveInlining)] public Task WaitAsync( CancellationToken ct )
        {
            SemaphoreSlim semaphore;
            _RWLS.EnterReadLock();
            {
                semaphore = _Semaphore;
            }
            _RWLS.ExitReadLock();

            return (semaphore.WaitAsync( ct ));  //thrown 'System.OperationCanceledException'
        }
        [M(O.AggressiveInlining)] public int Release()
        {
            _RWLS.EnterReadLock();
            try
            {
                return (_Semaphore.Release());  //thrown 'System.Threading.SemaphoreFullException'
            }
            finally
            {
                _RWLS.ExitReadLock();
            }
        }
        [M(O.AggressiveInlining)] public int Release( int releaseCount )
        {
            if ( releaseCount <= 0 )
            {
                return (0);
            }

            _RWLS.EnterReadLock();
            try
            {
                return (_Semaphore.Release( releaseCount )); //thrown 'System.Threading.SemaphoreFullException'
            }
            finally
            {
                _RWLS.ExitReadLock();
            }
        }
    }
    #endregion

    #region [.cross download instance restriction.]
    /// <summary>
    /// 
    /// </summary>
    internal struct cross_download_instance_restriction
    {
        private int? _MaxCrossDownloadInstance;
        private object _Lock;

        public cross_download_instance_restriction( int? maxCrossDownloadInstance )
        {
            _MaxCrossDownloadInstance = maxCrossDownloadInstance;
            _Lock = new object();
        }

        public int? GetMaxCrossDownloadInstance()
        {
            lock ( _Lock )
            {
                return (_MaxCrossDownloadInstance);
            }
        }
        public void SetMaxCrossDownloadInstance( int? maxCrossDownloadInstance )
        {
            lock ( _Lock )
            {
                _MaxCrossDownloadInstance = maxCrossDownloadInstance;
            }
        }
    }
    #endregion

    #region [.download_threads_semaphore.]
    /// <summary>
    /// 
    /// </summary>
    internal interface IDownloadThreadsSemaphoreEx : I_download_threads_semaphore
    {
        void ResetSemaphore( int degreeOfParallelism );
    }
    /// <summary>
    /// 
    /// </summary>
    internal sealed class cross_download_threads_semaphore : IDownloadThreadsSemaphoreEx
    {
        private SemaphoreHolder _SemaphoreHolder;
        private int             _WaitAcquireCount;

        public cross_download_threads_semaphore( SemaphoreHolder semaphoreHolder ) => _SemaphoreHolder = semaphoreHolder;
        void IDisposable.Dispose()
        {
            if ( _SemaphoreHolder != null )
            {
                _SemaphoreHolder.Release( _WaitAcquireCount );
                _SemaphoreHolder = null;
            }
        }

        public bool UseCrossDownloadInstanceParallelism { [M(O.AggressiveInlining)] get => true; }
        public void ResetSemaphore( int degreeOfParallelism ) => throw (new InvalidOperationException());

        public void Wait( CancellationToken ct )
        {
            //--!!!-- required here (because the calling context (task) is interrupted by the ct-CancellationToken --!!!--//
            Thread.BeginCriticalRegion();
            try
            {
                try { }
                finally
                {
                    _SemaphoreHolder.Wait( ct );
                    Interlocked.Increment( ref _WaitAcquireCount );
                }
            }
            finally
            {
                Thread.EndCriticalRegion();
            }
        }
        public async Task WaitAsync( CancellationToken ct )
        {
            //--!!!-- required here (because the calling context (task) is interrupted by the ct-CancellationToken --!!!--//
            Thread.BeginCriticalRegion();
            try
            {
                try { }
                finally
                {
                    await _SemaphoreHolder.WaitAsync( ct ).CAX();
                    Interlocked.Increment( ref _WaitAcquireCount );
                }
            }
            finally
            {
                Thread.EndCriticalRegion();
            }
        }
        public void Release()
        {
            //--!!!-- non-required here (because calling context not-interrupted) --!!!--//
            Thread.BeginCriticalRegion();
            try
            {
                try { }
                finally
                {
                    Interlocked.Decrement( ref _WaitAcquireCount );
                    _SemaphoreHolder.Release();
                }
            }
            finally
            {
                Thread.EndCriticalRegion();
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    internal sealed class self_download_threads_semaphore : IDownloadThreadsSemaphoreEx
    {
        private SemaphoreSlim   _Semaphore;
        private SemaphoreHolder _SemaphoreHolder;

        public self_download_threads_semaphore( int degreeOfParallelism )
        {
            _Semaphore       = new SemaphoreSlim( degreeOfParallelism, degreeOfParallelism );
            _SemaphoreHolder = new SemaphoreHolder( _Semaphore, UseCrossDownloadInstanceParallelism );
        }
        public void Dispose()
        {
            if ( _SemaphoreHolder != null )
            {
                _SemaphoreHolder.ResetSemaphore( null );
                _SemaphoreHolder.Dispose();
                _SemaphoreHolder = null;
            }

            var semaphore = Interlocked.Exchange( ref _Semaphore, null );
            if ( semaphore != null )
            {
                semaphore.Dispose();
            }
        }

        public bool UseCrossDownloadInstanceParallelism { [M(O.AggressiveInlining)] get => false; }

        public void ResetSemaphore( int degreeOfParallelism )
        {
            var semaphore = new SemaphoreSlim( degreeOfParallelism, degreeOfParallelism );
            _SemaphoreHolder.ResetSemaphore( semaphore );

            var semaphore_prev = Interlocked.Exchange( ref _Semaphore, semaphore );
            semaphore_prev.WaitAsyncAndDispose();
        }

        [M(O.AggressiveInlining)] public void Wait( CancellationToken ct ) => _SemaphoreHolder.Wait( ct );
        [M(O.AggressiveInlining)] public Task WaitAsync( CancellationToken ct ) => _SemaphoreHolder.WaitAsync( ct );
        [M(O.AggressiveInlining)] public void Release() => _SemaphoreHolder.Release();
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class download_threads_semaphore_factory : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private sealed class dummy_download_threads_semaphore : IDownloadThreadsSemaphoreEx
        {
            [M(O.AggressiveInlining)] public dummy_download_threads_semaphore() { }
            [M(O.AggressiveInlining)] public void Dispose() { }

            public bool UseCrossDownloadInstanceParallelism { [M(O.AggressiveInlining)] get => false; }

            [M(O.AggressiveInlining)] public void ResetSemaphore( int degreeOfParallelism ) { }

            [M(O.AggressiveInlining)] public void Wait( CancellationToken ct ) { }
            [M(O.AggressiveInlining)] public Task WaitAsync( CancellationToken ct ) => Task.CompletedTask;
            [M(O.AggressiveInlining)] public void Release() { }
        }

        #region [.fields.]
        private SemaphoreSlim   _CrossSemaphore;
        private SemaphoreHolder _CrossSemaphoreHolder;
        private dummy_download_threads_semaphore _dummy_download_threads_semaphore;
        #endregion

        #region [.ctor().]
        public download_threads_semaphore_factory( bool useCrossDownloadInstanceParallelism, int maxDegreeOfParallelism )
        {
            this.ShareMaxDownloadThreadsBetweenAllDownloadsInstance = useCrossDownloadInstanceParallelism;            
            this.MaxDegreeOfParallelism              = maxDegreeOfParallelism;
            this.UseMaxDegreeOfParallelism           = (0 < maxDegreeOfParallelism);

            _CrossSemaphore       = new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism );
            _CrossSemaphoreHolder = new SemaphoreHolder( _CrossSemaphore, true );

            _dummy_download_threads_semaphore = new dummy_download_threads_semaphore();
        }
        public void Dispose()
        {
            _dummy_download_threads_semaphore = null;

            if ( _CrossSemaphoreHolder != null )
            {
                _CrossSemaphoreHolder.ResetSemaphore( null );
                _CrossSemaphoreHolder.Dispose();
                _CrossSemaphoreHolder = null;
            }

            var crossSemaphore = Interlocked.Exchange( ref _CrossSemaphore, null );
            if ( crossSemaphore != null )
            {
                crossSemaphore.Dispose();
            }
        }
        #endregion

        public bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance { get; set; }
        public bool UseMaxDegreeOfParallelism                          { get; set; }
        public int  MaxDegreeOfParallelism                             { get; private set; }

        public async Task ResetMaxDegreeOfParallelism( int maxDegreeOfParallelism, int millisecondsDelay = 10 )
        {
            if ( this.MaxDegreeOfParallelism != maxDegreeOfParallelism )
            {
                var newCrossSemaphore = new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism );
                var actCrossSemaphore = Interlocked.Exchange( ref _CrossSemaphore, newCrossSemaphore );

                for ( ; actCrossSemaphore.CurrentCount != this.MaxDegreeOfParallelism; )
                {
                    var success = await actCrossSemaphore.WaitAsync( millisecondsDelay );
                    if ( success )
                    {
                        actCrossSemaphore.Release();
                    }
                }

                _CrossSemaphoreHolder.ResetSemaphore( newCrossSemaphore );
                actCrossSemaphore.WaitAsyncAndDispose();

                this.MaxDegreeOfParallelism = maxDegreeOfParallelism;
            }
        }

        public IDownloadThreadsSemaphoreEx Get()
        {
            if ( this.ShareMaxDownloadThreadsBetweenAllDownloadsInstance )
            {
                return (new cross_download_threads_semaphore( _CrossSemaphoreHolder ));
            }

            if ( this.UseMaxDegreeOfParallelism )
            {
                return (new self_download_threads_semaphore( this.MaxDegreeOfParallelism ));
            }

            return (_dummy_download_threads_semaphore);
        }
    }
    #endregion
}
