using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.controllers
{
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

    #region [.IDownloadThreadsSemaphoreEx.]    
    /// <summary>
    /// 
    /// </summary>
    public enum ResetSemaphoreModeEnum { SetInitalCountAsCurrent, SetInitalCount2Max }

    /// <summary>
    /// 
    /// </summary>
    internal interface IDownloadThreadsSemaphoreEx : I_download_threads_semaphore
    {
        void ResetSemaphore( int degreeOfParallelism, ResetSemaphoreModeEnum resetSemaphoreMode );
    }
    #endregion

    #region comm. [.v0.]    
    //#region [.SemaphoreHolder_v0.]
    ///// <summary>
    ///// 
    ///// </summary>
    //internal sealed class SemaphoreHolder_v0 : IDisposable
    //{
    //    private volatile SemaphoreSlim _Semaphore;
    //    private ReaderWriterLockSlim   _RWLS;

    //    public SemaphoreHolder_v0( SemaphoreSlim semaphore )
    //    {
    //        _Semaphore = semaphore;
    //        _RWLS      = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );
    //    }
    //    public void Dispose()
    //    {
    //        if ( _RWLS != null )
    //        {
    //            ResetSemaphore( null );

    //            _RWLS.Dispose();
    //            _RWLS = null;
    //        }
    //    }

    //    public bool HasSemaphore
    //    {
    //        [M(O.AggressiveInlining)] get
    //        {
    //            bool hasSemaphore;
    //            _RWLS.EnterReadLock();
    //            {
    //                hasSemaphore = (_Semaphore != null);
    //            }
    //            _RWLS.ExitReadLock();
    //            return (hasSemaphore);
    //        }
    //    }

    //    [M(O.AggressiveInlining)] public void ResetSemaphore( SemaphoreSlim semaphore )
    //    {
    //        _RWLS.EnterWriteLock();
    //        {
    //            _Semaphore = semaphore;
    //        }
    //        _RWLS.ExitWriteLock();
    //    }

    //    [M(O.AggressiveInlining)] public void Wait( CancellationToken ct )
    //    {
    //        SemaphoreSlim semaphore;
    //        _RWLS.EnterReadLock();
    //        {
    //            semaphore = _Semaphore;
    //        }
    //        _RWLS.ExitReadLock();

    //        semaphore.Wait( ct );  //thrown 'System.OperationCanceledException'
    //    }
    //    [M(O.AggressiveInlining)] public Task WaitAsync( CancellationToken ct )
    //    {
    //        SemaphoreSlim semaphore;
    //        _RWLS.EnterReadLock();
    //        {
    //            semaphore = _Semaphore;
    //        }
    //        _RWLS.ExitReadLock();

    //        return (semaphore.WaitAsync( ct ));  //thrown 'System.OperationCanceledException'
    //    }
    //    [M(O.AggressiveInlining)] public bool Release()
    //    {
    //        _RWLS.EnterReadLock();
    //        try
    //        {
    //            _Semaphore.Release(); //thrown 'System.Threading.SemaphoreFullException'
    //            return (true);
    //        }
    //        finally
    //        {
    //            _RWLS.ExitReadLock();
    //        }
    //    }
    //    [M(O.AggressiveInlining)] public bool Release( int releaseCount )
    //    {
    //        if ( releaseCount <= 0 )
    //        {
    //            return (false);
    //        }

    //        _RWLS.EnterReadLock();
    //        try
    //        {
    //            _Semaphore.Release( releaseCount ); //thrown 'System.Threading.SemaphoreFullException'
    //            return (true); 
    //        }
    //        finally
    //        {
    //            _RWLS.ExitReadLock();
    //        }
    //    }
    //}
    //#endregion

    //#region [.download_threads_semaphore(s) v0.]
    ///// <summary>
    ///// 
    ///// </summary>
    //internal sealed class cross_download_threads_semaphore_v0 : IDownloadThreadsSemaphoreEx
    //{
    //    private SemaphoreHolder_v0 _SemaphoreHolder;
    //    private int             _WaitAcquireCount;

    //    public cross_download_threads_semaphore_v0( SemaphoreHolder_v0 semaphoreHolder ) => _SemaphoreHolder = semaphoreHolder;
    //    void IDisposable.Dispose()
    //    {
    //        if ( _SemaphoreHolder != null )
    //        {
    //            var waitAcquireCount = Interlocked.Exchange( ref _WaitAcquireCount, 0 );
    //            if ( 0 < waitAcquireCount ) _SemaphoreHolder.Release( waitAcquireCount );
    //            _SemaphoreHolder = null;
    //        }
    //    }

    //    public bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance { [M(O.AggressiveInlining)] get => true; }

    //    public void ResetSemaphore( int degreeOfParallelism, ResetSemaphoreModeEnum resetSemaphoreMode ) => throw (new InvalidOperationException()); //changed over download_threads_semaphore_factory

    //    public void Wait( CancellationToken ct )
    //    {
    //        //--!!!-- required here (because the calling context (task) is interrupted by the ct-CancellationToken --!!!--//
    //        Thread.BeginCriticalRegion();
    //        try
    //        {
    //            try { }
    //            finally
    //            {
    //                _SemaphoreHolder.Wait( ct );
    //                Interlocked.Increment( ref _WaitAcquireCount );
    //            }
    //        }
    //        finally
    //        {
    //            Thread.EndCriticalRegion();
    //        }
    //    }
    //    public async Task WaitAsync( CancellationToken ct )
    //    {
    //        //--!!!-- required here (because the calling context (task) is interrupted by the ct-CancellationToken --!!!--//
    //        Thread.BeginCriticalRegion();
    //        try
    //        {
    //            try { }
    //            finally
    //            {
    //                await _SemaphoreHolder.WaitAsync( ct ).CAX();
    //                Interlocked.Increment( ref _WaitAcquireCount );
    //            }
    //        }
    //        finally
    //        {
    //            Thread.EndCriticalRegion();
    //        }
    //    }
    //    public bool Release()
    //    {
    //        //--!!!-- non-required here (because calling context not-interrupted) --!!!--//
    //        Thread.BeginCriticalRegion();
    //        try
    //        {
    //            bool suc;
    //            try { }
    //            finally
    //            {
    //                Interlocked.Decrement( ref _WaitAcquireCount );
    //                suc = _SemaphoreHolder.Release();
    //            }
    //            return (suc);
    //        }
    //        finally
    //        {
    //            Thread.EndCriticalRegion();
    //        }
    //    }
    //    public bool Release_NoThrow()
    //    {
    //        try
    //        {
    //            return (Release());
    //        }
    //        catch ( SemaphoreFullException ex )
    //        {
    //            Debug.WriteLine( ex );
    //            return (false);
    //        }
    //    }
    //}
    ///// <summary>
    ///// 
    ///// </summary>
    //internal sealed class self_download_threads_semaphore_v0 : IDownloadThreadsSemaphoreEx
    //{
    //    private SemaphoreSlim   _Semaphore;
    //    private SemaphoreHolder_v0 _SemaphoreHolder;

    //    public self_download_threads_semaphore_v0( int degreeOfParallelism )
    //    {
    //        _Semaphore       = new SemaphoreSlim( degreeOfParallelism, degreeOfParallelism );
    //        _SemaphoreHolder = new SemaphoreHolder_v0( _Semaphore );
    //    }
    //    public void Dispose()
    //    {
    //        if ( _SemaphoreHolder != null )
    //        {
    //            _SemaphoreHolder.ResetSemaphore( null );
    //            _SemaphoreHolder.Dispose();
    //            _SemaphoreHolder = null;
    //        }

    //        var semaphore = Interlocked.Exchange( ref _Semaphore, null );
    //        if ( semaphore != null )
    //        {
    //            semaphore.Dispose();
    //        }
    //    }

    //    public bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance { [M(O.AggressiveInlining)] get => false; }

    //    public void ResetSemaphore( int degreeOfParallelism, ResetSemaphoreModeEnum resetSemaphoreMode )
    //    {
    //        var semaphore = new SemaphoreSlim( degreeOfParallelism, degreeOfParallelism );
    //        _SemaphoreHolder.ResetSemaphore( semaphore );

    //        var semaphore_prev = Interlocked.Exchange( ref _Semaphore, semaphore );
    //        semaphore_prev.Release_NoThrow();
    //        semaphore_prev.Dispose_NoThrow(); //.FullReleaseAndDispose( /*semaphore, ct*/ );//.WaitAsyncAndDispose();
    //    }

    //    [M(O.AggressiveInlining)] public void Wait( CancellationToken ct ) => _SemaphoreHolder.Wait( ct );
    //    [M(O.AggressiveInlining)] public Task WaitAsync( CancellationToken ct ) => _SemaphoreHolder.WaitAsync( ct );
    //    [M(O.AggressiveInlining)] public bool Release() => _SemaphoreHolder.Release();
    //    [M(O.AggressiveInlining)] public bool Release_NoThrow()
    //    {
    //        try
    //        {
    //            return (_SemaphoreHolder.Release());
    //        }
    //        catch ( SemaphoreFullException ex )
    //        {
    //            Debug.WriteLine( ex );
    //            return (false);
    //        }
    //    }

    //    public void ResetSemaphore_SetInitalCount2Max( int degreeOfParallelism ) => throw new NotImplementedException();
    //    public void ResetSemaphore_SetInitalCountAsCurrent( int degreeOfParallelism ) => throw new NotImplementedException();
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    //internal sealed class download_threads_semaphore_factory_v0 : IDisposable
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    private sealed class dummy_download_threads_semaphore : IDownloadThreadsSemaphoreEx
    //    {
    //        [M(O.AggressiveInlining)] public dummy_download_threads_semaphore() { }
    //        [M(O.AggressiveInlining)] public void Dispose() { }

    //        public bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance { [M(O.AggressiveInlining)] get => false; }

    //        [M(O.AggressiveInlining)] public void ResetSemaphore( int degreeOfParallelism, ResetSemaphoreModeEnum resetSemaphoreMode ) { }

    //        [M(O.AggressiveInlining)] public void Wait( CancellationToken ct ) { }
    //        [M(O.AggressiveInlining)] public Task WaitAsync( CancellationToken ct ) => Task.CompletedTask;
    //        [M(O.AggressiveInlining)] public bool Release() => true;
    //        [M(O.AggressiveInlining)] public bool Release_NoThrow() => true;
    //    }

    //    #region [.fields.]
    //    private SemaphoreSlim   _CrossSemaphore;
    //    private SemaphoreHolder_v0 _CrossSemaphoreHolder;
    //    private dummy_download_threads_semaphore _dummy_download_threads_semaphore;
    //    #endregion

    //    #region [.ctor().]
    //    public download_threads_semaphore_factory_v0( bool shareMaxDownloadThreadsBetweenAllDownloadsInstance, int maxDegreeOfParallelism )
    //    {
    //        this.ShareMaxDownloadThreadsBetweenAllDownloadsInstance = shareMaxDownloadThreadsBetweenAllDownloadsInstance;            
    //        this.MaxDegreeOfParallelism                             = maxDegreeOfParallelism;
    //        this.UseMaxDegreeOfParallelism                          = (0 < maxDegreeOfParallelism);

    //        _CrossSemaphore       = new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism );
    //        _CrossSemaphoreHolder = new SemaphoreHolder_v0( _CrossSemaphore );

    //        _dummy_download_threads_semaphore = new dummy_download_threads_semaphore();
    //    }
    //    public void Dispose()
    //    {
    //        _dummy_download_threads_semaphore = null;

    //        if ( _CrossSemaphoreHolder != null )
    //        {
    //            _CrossSemaphoreHolder.ResetSemaphore( null );
    //            _CrossSemaphoreHolder.Dispose();
    //            _CrossSemaphoreHolder = null;
    //        }

    //        var crossSemaphore = Interlocked.Exchange( ref _CrossSemaphore, null );
    //        if ( crossSemaphore != null )
    //        {
    //            crossSemaphore.Dispose();
    //        }
    //    }
    //    #endregion

    //    public bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance { get; set; }
    //    public bool UseMaxDegreeOfParallelism                          { get; set; }
    //    public int  MaxDegreeOfParallelism                             { get; private set; }

    //    public async Task ResetMaxDegreeOfParallelism( int maxDegreeOfParallelism, int millisecondsDelay = 10 )
    //    {
    //        if ( this.MaxDegreeOfParallelism != maxDegreeOfParallelism )
    //        {
    //            var newCrossSemaphore = new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism );
    //            var prevCrossSemaphore = Interlocked.Exchange( ref _CrossSemaphore, newCrossSemaphore );

    //            var releaseCount = this.MaxDegreeOfParallelism - prevCrossSemaphore.CurrentCount;
    //            if ( 0 < releaseCount )
    //            {
    //                try
    //                {
    //                    prevCrossSemaphore.Release( releaseCount );
    //                }
    //                catch ( SemaphoreFullException ex )
    //                {
    //                    Debug.Write( ex );
    //                }
    //            }

    //            for ( ; prevCrossSemaphore.CurrentCount != this.MaxDegreeOfParallelism; )
    //            {
    //                var suc = await prevCrossSemaphore.WaitAsync( millisecondsDelay ).CAX();
    //                if ( suc )
    //                {
    //                    prevCrossSemaphore.Release();
    //                }
    //                Debug.WriteLine( "[prevCrossSemaphore]: wait for (prevCrossSemaphore.CurrentCount == this.MaxDegreeOfParallelism)..." );
    //            }

    //            _CrossSemaphoreHolder.ResetSemaphore( newCrossSemaphore );
    //            prevCrossSemaphore.Dispose_NoThrow(); //.FullReleaseAndDispose( newCrossSemaphore, CancellationToken.None );//.WaitAsyncAndDispose();

    //            this.MaxDegreeOfParallelism = maxDegreeOfParallelism;
    //        }
    //    }

    //    public IDownloadThreadsSemaphoreEx Get()
    //    {
    //        if ( this.ShareMaxDownloadThreadsBetweenAllDownloadsInstance )
    //        {
    //            return (new cross_download_threads_semaphore_v0( _CrossSemaphoreHolder ));
    //        }

    //        if ( this.UseMaxDegreeOfParallelism )
    //        {
    //            return (new self_download_threads_semaphore_v2( this.MaxDegreeOfParallelism ));
    //            //return (new self_download_threads_semaphore( this.MaxDegreeOfParallelism ));
    //        }

    //        return (_dummy_download_threads_semaphore);
    //    }
    //}
    //#endregion
    #endregion

    #region [.actual.]
    /// <summary>
    /// 
    /// </summary>
    internal sealed class SemaphoreHolder : IDisposable
    {
        private volatile SemaphoreSlim _Semaphore;
        private ReaderWriterLockSlim   _RWLS;
        private int _DegreeOfParallelism;

        public SemaphoreHolder( int degreeOfParallelism )
        {
            _DegreeOfParallelism = degreeOfParallelism;
            _Semaphore = new SemaphoreSlim( degreeOfParallelism, degreeOfParallelism );
            _RWLS      = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );
        }
        public void Dispose()
        {
            if ( _RWLS != null )
            {
                _RWLS.EnterWriteLock();
                {
                    var freeCount = _Semaphore.CurrentCount;
                    var workingCount = _DegreeOfParallelism - freeCount;
                    if ( 0 < workingCount )
                    {
                        _Semaphore.Release( workingCount );
                    }
                }
                _RWLS.ExitWriteLock();

                _Semaphore.Dispose();
                _Semaphore = null;

                _RWLS.Dispose();
                _RWLS = null;
            }
        }

        [M(O.AggressiveInlining)] private SemaphoreSlim GetSemaphoreThreadSafe()
        {
            SemaphoreSlim semaphore;
            _RWLS.EnterReadLock();
            {
                semaphore = _Semaphore;
            }
            _RWLS.ExitReadLock();

            return (semaphore);
        }
        //[M(O.AggressiveInlining)] public WaitHandle GetAvailableWaitHandle() => GetSemaphoreThreadSafe().AvailableWaitHandle;

        [M(O.AggressiveInlining)] public void ResetSemaphore( int degreeOfParallelism, ResetSemaphoreModeEnum resetSemaphoreMode )
        {
            _RWLS.EnterUpgradeableReadLock();
            {                
                if ( _DegreeOfParallelism != degreeOfParallelism )
                {
                    _RWLS.EnterWriteLock();
                    {
                        var freeCount    = _Semaphore.CurrentCount;
                        var workingCount = _DegreeOfParallelism - freeCount;
                        int newFreeCount;
                        switch ( resetSemaphoreMode )
                        {
                            case ResetSemaphoreModeEnum.SetInitalCount2Max:
                                newFreeCount = degreeOfParallelism;
                                break;
                            case ResetSemaphoreModeEnum.SetInitalCountAsCurrent:
                            default:
                                newFreeCount = degreeOfParallelism - workingCount;
                                newFreeCount = (0 < newFreeCount) ? newFreeCount : degreeOfParallelism;
                                break;
                        }

                        if ( 0 < workingCount )
                        {
                            _Semaphore.Release( workingCount );
                        }

                        var newSemaphore = new SemaphoreSlim( newFreeCount, degreeOfParallelism );
                        _DegreeOfParallelism = degreeOfParallelism;
                        _Semaphore           = newSemaphore;
                    }
                    _RWLS.ExitWriteLock();
                }
            }
            _RWLS.ExitUpgradeableReadLock();
        }

        [M(O.AggressiveInlining)] public void Wait( CancellationToken ct ) => GetSemaphoreThreadSafe().Wait( ct );  //thrown 'System.OperationCanceledException'
        [M(O.AggressiveInlining)] public Task WaitAsync( CancellationToken ct ) => GetSemaphoreThreadSafe().WaitAsync( ct );  //thrown 'System.OperationCanceledException'
        [M(O.AggressiveInlining)] public bool Release()
        {
            _RWLS.EnterReadLock();
            try
            {
                if ( _Semaphore.CurrentCount < _DegreeOfParallelism )
                {
                    _Semaphore.Release(); //thrown 'System.Threading.SemaphoreFullException'
                    return (true);  
                }
                return (false);
            }
            finally
            {
                _RWLS.ExitReadLock();
            }
        }
        [M(O.AggressiveInlining)] public void ReleaseAll()
        {
            _RWLS.EnterReadLock();
            try
            {
                while ( _Semaphore.CurrentCount != _DegreeOfParallelism )
                {
                    _Semaphore.Release(); //thrown 'System.Threading.SemaphoreFullException'
                }
            }
            finally
            {
                _RWLS.ExitReadLock();
            }
        }

        public override string ToString() => (_Semaphore != null) ? $"MAX = {_DegreeOfParallelism}, CurrentCount = {_Semaphore.CurrentCount}" : "Semaphore=NULL";
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class self_download_threads_semaphore : IDownloadThreadsSemaphoreEx
    {
        private SemaphoreHolder _SemaphoreHolder;

        public self_download_threads_semaphore( int degreeOfParallelism ) => _SemaphoreHolder = new SemaphoreHolder( degreeOfParallelism );
        public void Dispose() => _SemaphoreHolder.Dispose();

        public bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance { [M(O.AggressiveInlining)] get => false; }

        //[M(O.AggressiveInlining)] public WaitHandle GetAvailableWaitHandle() => _SemaphoreHolder.GetAvailableWaitHandle();
        [M(O.AggressiveInlining)] public void ResetSemaphore( int degreeOfParallelism, ResetSemaphoreModeEnum resetSemaphoreMode ) => _SemaphoreHolder.ResetSemaphore( degreeOfParallelism, resetSemaphoreMode );

        [M(O.AggressiveInlining)] public void Wait( CancellationToken ct ) => _SemaphoreHolder.Wait( ct );
        [M(O.AggressiveInlining)] public Task WaitAsync( CancellationToken ct ) => _SemaphoreHolder.WaitAsync( ct );
        [M(O.AggressiveInlining)] public bool Release() => _SemaphoreHolder.Release();
        [M(O.AggressiveInlining)] public bool Release_NoThrow()
        {
            try
            {
                return (_SemaphoreHolder.Release());
            }
            catch ( SemaphoreFullException ex )
            {
                Debug.WriteLine( ex );
                return (false);
            }
        }

        public override string ToString() => _SemaphoreHolder.ToString();
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
                var waitAcquireCount = Interlocked.Exchange( ref _WaitAcquireCount, 0 );
                //if ( 0 < waitAcquireCount ) _SemaphoreHolder.Release( waitAcquireCount );
                _SemaphoreHolder = null;
            }
        }

        public bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance { [M(O.AggressiveInlining)] get => true; }

        //[M(O.AggressiveInlining)] public WaitHandle GetAvailableWaitHandle() => _SemaphoreHolder.GetAvailableWaitHandle();
        public void ResetSemaphore( int degreeOfParallelism, ResetSemaphoreModeEnum resetSemaphoreMode ) => throw (new InvalidOperationException()); //changed over download_threads_semaphore_factory

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
        public bool Release()
        {
            //--!!!-- non-required here (because calling context not-interrupted) --!!!--//
            Thread.BeginCriticalRegion();
            try
            {
                bool suc;
                try { }
                finally
                {
                    Interlocked.Decrement( ref _WaitAcquireCount );
                    suc = _SemaphoreHolder.Release();
                }
                return (suc);
            }
            finally
            {
                Thread.EndCriticalRegion();
            }
        }
        public bool Release_NoThrow()
        {
            try
            {
                return (Release());
            }
            catch ( SemaphoreFullException ex )
            {
                Debug.WriteLine( ex );
                return (false);
            }
        }

        public override string ToString() => $"[CROSS]: {_SemaphoreHolder}";
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

            public bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance { [M(O.AggressiveInlining)] get => false; }

            //[M( O.AggressiveInlining )] public WaitHandle GetAvailableWaitHandle() => null; //throw new NotImplementedException();
            [M(O.AggressiveInlining)] public void ResetSemaphore( int degreeOfParallelism, ResetSemaphoreModeEnum resetSemaphoreMode ) { }

            [M(O.AggressiveInlining)] public void Wait( CancellationToken ct ) { }
            [M(O.AggressiveInlining)] public Task WaitAsync( CancellationToken ct ) => Task.CompletedTask;
            [M(O.AggressiveInlining)] public bool Release() => true;
            [M(O.AggressiveInlining)] public bool Release_NoThrow() => true;
        }

        #region [.fields.]
        private SemaphoreHolder _CrossSemaphoreHolder;
        private dummy_download_threads_semaphore _dummy_download_threads_semaphore;
        #endregion

        #region [.ctor().]
        public download_threads_semaphore_factory( bool shareMaxDownloadThreadsBetweenAllDownloadsInstance, int maxDegreeOfParallelism )
        {
            this.ShareMaxDownloadThreadsBetweenAllDownloadsInstance = shareMaxDownloadThreadsBetweenAllDownloadsInstance;            
            this.MaxDegreeOfParallelism                             = maxDegreeOfParallelism;
            this.UseMaxDegreeOfParallelism                          = (0 < maxDegreeOfParallelism);

            _CrossSemaphoreHolder = new SemaphoreHolder( maxDegreeOfParallelism );

            _dummy_download_threads_semaphore = new dummy_download_threads_semaphore();
        }
        public void Dispose()
        {
            _dummy_download_threads_semaphore = null;

            if ( _CrossSemaphoreHolder != null )
            {
                _CrossSemaphoreHolder.Dispose();
                _CrossSemaphoreHolder = null;
            }
        }
        #endregion

        public bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance { get; set; }
        public bool UseMaxDegreeOfParallelism                          { get; set; }
        public int  MaxDegreeOfParallelism                             { get; private set; }

        public void ResetMaxDegreeOfParallelism( int maxDegreeOfParallelism, ResetSemaphoreModeEnum _/*resetSemaphoreMode*/ )
        {
            if ( this.MaxDegreeOfParallelism != maxDegreeOfParallelism )
            {
                _CrossSemaphoreHolder.ResetSemaphore( maxDegreeOfParallelism, ResetSemaphoreModeEnum.SetInitalCount2Max /*resetSemaphoreMode*/ );
                this.MaxDegreeOfParallelism = maxDegreeOfParallelism;
            }
        }
        public void ReleaseAll() => _CrossSemaphoreHolder.ReleaseAll();

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


        public override string ToString() => $"ShareMaxDownloadThreadsBetweenAllDownloadsInstance = {ShareMaxDownloadThreadsBetweenAllDownloadsInstance}, [CrossSemaphore]: {_CrossSemaphoreHolder}";
    }
    #endregion
}
