using System;
using System.Diagnostics;
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
        /// <summary>
        /// 
        /// </summary>
        public delegate void ResetSemaphoreHappensEventHandler();

        public event ResetSemaphoreHappensEventHandler ResetSemaphoreHappens;

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
                    hasSemaphore = (_Semaphore != null); //(Volatile.Read( ref _Semaphore ) != null);
                }
                _RWLS.ExitReadLock();
                return (hasSemaphore);
            }
        }
        [M(O.AggressiveInlining)] public void ResetSemaphore( SemaphoreSlim semaphore )
        {
            _RWLS.EnterWriteLock();
            {
                _Semaphore = semaphore; //Volatile.Write( ref _Semaphore, semaphore );
            }
            _RWLS.ExitWriteLock();

            ResetSemaphoreHappens?.Invoke();
        }

        [M(O.AggressiveInlining)] public void Wait( CancellationToken ct )
        {
            SemaphoreSlim semaphore;
            _RWLS.EnterReadLock();
            {
                semaphore = _Semaphore; //semaphore = Volatile.Read( ref _Semaphore );
            }
            _RWLS.ExitReadLock();

            semaphore.Wait( ct );  //thrown 'System.OperationCanceledException'
        }
        [M(O.AggressiveInlining)] public int Release()
        {
            _RWLS.EnterReadLock();
            try
            {
                return (_Semaphore.Release());  //return (Volatile.Read( ref _Semaphore ).Release());  //thrown 'System.Threading.SemaphoreFullException'
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
                return (_Semaphore.Release( releaseCount ));  //return (Volatile.Read( ref _Semaphore ).Release( releaseCount ));  //thrown 'System.Threading.SemaphoreFullException'
            }
            finally
            {
                _RWLS.ExitReadLock();
            }
        }
    }
    #endregion


    #region [.cross_download_instance_semaphore.]
    /// <summary>
    /// 
    /// </summary>
    internal sealed class cross_download_instance_semaphore : I_cross_download_instance_semaphore
    {
        private SemaphoreHolder _SemaphoreHolder;
        private bool            _IsSemaphoreWasAcquired;

        public cross_download_instance_semaphore( SemaphoreHolder semaphoreHolder )
        {
            _SemaphoreHolder = semaphoreHolder;
            _SemaphoreHolder.ResetSemaphoreHappens += SemaphoreHolder_ResetSemaphoreHappens;
        }
        public void Dispose()
        {
            if ( _SemaphoreHolder != null )
            {
                if ( _IsSemaphoreWasAcquired )
                {
                    _SemaphoreHolder.Release();
                }
                _SemaphoreHolder.ResetSemaphoreHappens -= SemaphoreHolder_ResetSemaphoreHappens;
                _SemaphoreHolder = null;
            }
        }

        [M(O.AggressiveInlining)] private void Wait( CancellationToken ct )
        {
            //_SemaphoreHolder.Wait( ct );
            //-----------------------------------------//

            //--!!!-- non-required here (seemingly), (because calling context not-interrupted) --!!!--//
            Thread.BeginCriticalRegion();
            try
            {
                try { }
                finally
                {
                    _IsSemaphoreWasAcquired = false;
                    _SemaphoreHolder.Wait( ct );
                    _IsSemaphoreWasAcquired = true;
                }
            }
            finally
            {
                Thread.EndCriticalRegion();
            }
        }

        private void SemaphoreHolder_ResetSemaphoreHappens()
        {
            this.IsNeedReacquire = true;
            this.IsNeedReacquireCts?.Cancel();
        }

        private volatile int _IsNeedReacquire;
        public bool IsNeedReacquire
        {
            [M(O.AggressiveInlining)] get => (_IsNeedReacquire == 1);
            private set => Interlocked.Exchange( ref _IsNeedReacquire, (value ? 1 : 0) );
        }

        private volatile CancellationTokenSource _IsNeedReacquireCts;
        private CancellationTokenSource IsNeedReacquireCts
        {
            get => _IsNeedReacquireCts;
            set => Interlocked.Exchange( ref _IsNeedReacquireCts, value );
        }

        public void WaitForReacquire( CancellationToken ct, Action waitingForOtherDownloadInstanceFinished, int delayTimeout )
        {
            #region [.dispose exists 'IsNeedReacquireCts'.]
            var isNeedReacquireCts_prev = Interlocked.Exchange( ref _IsNeedReacquireCts, null );
            if ( isNeedReacquireCts_prev != null )
            {
                isNeedReacquireCts_prev.Cancel();
                isNeedReacquireCts_prev.Dispose();
            } 
            #endregion

            using ( var delayTaskCts   = new CancellationTokenSource() )
            using ( var joinedDelayCts = CancellationTokenSource.CreateLinkedTokenSource( ct, delayTaskCts.Token ) )
            { 
                for (; ; )
                {
                    using ( var isNeedReacquireCts = new CancellationTokenSource() )
                    using ( var joinedCancelCts    = CancellationTokenSource.CreateLinkedTokenSource( ct, isNeedReacquireCts.Token ) )
                    {
                        var delayTask = Task.Delay( delayTimeout, joinedDelayCts.Token );
                        delayTask.ContinueWith( t => waitingForOtherDownloadInstanceFinished?.Invoke(), joinedDelayCts.Token );

                        try
                        {
                            this.IsNeedReacquireCts = isNeedReacquireCts;
                            Wait( joinedCancelCts.Token );
                            this.IsNeedReacquireCts = null;
                            this.IsNeedReacquire = false;
                            delayTaskCts.Cancel();

                            return;
                        }
                        catch ( Exception ex ) when ((ex is ObjectDisposedException) || 
                                                     (ex is OperationCanceledException) || 
                                                     (ex is ArgumentNullException) /*?!?! - strange*/)
                        {
                            Debug.WriteLine( ex );

                            this.IsNeedReacquireCts = null;
                            delayTaskCts.Cancel();

                            if ( this.IsNeedReacquire && isNeedReacquireCts.IsCancellationRequested )
                            {
                                this.IsNeedReacquire = false;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    internal sealed class cross_download_instance_semaphore_factory : I_cross_download_instance_factory, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private struct dummy_disposable : I_cross_download_instance_semaphore
        {
            public bool IsNeedReacquire { [M(O.AggressiveInlining)] get => false; }
            public void WaitForReacquire( CancellationToken ct, Action waitingForOtherDownloadInstanceFinished, int delayTimeout ) { }
            void IDisposable.Dispose() { }
        }

        private SemaphoreSlim   _Semaphore;
        private SemaphoreHolder _SemaphoreHolder;

        public cross_download_instance_semaphore_factory( int? maxCrossDownloadInstance )
        {
            _SemaphoreHolder = new SemaphoreHolder( null, true );
            ResetMaxCrossDownloadInstance( maxCrossDownloadInstance );
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

        public int? MaxCrossDownloadInstance { [M(O.AggressiveInlining)] get; private set; }
        public void ResetMaxCrossDownloadInstance( int? maxCrossDownloadInstance )
        {
            if ( this.MaxCrossDownloadInstance != maxCrossDownloadInstance )
            {
                var semaphore = (maxCrossDownloadInstance.HasValue ? new SemaphoreSlim( maxCrossDownloadInstance.Value ) : null);
                _SemaphoreHolder.ResetSemaphore( semaphore ); //must raised subscribe evenet 'ResetSemaphoreHappens' for created 'I_cross_download_instance_semaphore' items.

                var semaphore_prev = Interlocked.Exchange( ref _Semaphore, semaphore );
                semaphore_prev.WaitAsyncAndDispose();

                this.MaxCrossDownloadInstance = maxCrossDownloadInstance;
            }
        }

        public I_cross_download_instance_semaphore CreateWithWait( CancellationToken ct, Action waitingForOtherDownloadInstanceFinished, int delayTimeout )
        {
            if ( _SemaphoreHolder.HasSemaphore )
            {
#if DEBUG
                Debug.Assert( _Semaphore != null );
#endif
                var semaphore = new cross_download_instance_semaphore( _SemaphoreHolder );
                semaphore.WaitForReacquire( ct, waitingForOtherDownloadInstanceFinished, delayTimeout );
                return (semaphore);

                #region comm.
                /*
                using ( var delayTaskCts = new CancellationTokenSource() )
                using ( var joinedCts    = CancellationTokenSource.CreateLinkedTokenSource( ct, delayTaskCts.Token ) )
                {
                    var delayTask = Task.Delay( delayTimeout, joinedCts.Token );
                    delayTask.ContinueWith( t => waitingForOtherDownloadInstanceFinished?.Invoke(), joinedCts.Token );

                    var cross_download_instance_semaphore = new cross_download_instance_semaphore( _SemaphoreHolder );
                    try
                    {
                        cross_download_instance_semaphore.Wait( ct );
                        delayTaskCts.Cancel();

                        return (cross_download_instance_semaphore);
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );

                        cross_download_instance_semaphore.Dispose();
                    }                                               
                }
                */
                #endregion
            }

            return (default(dummy_disposable));
        }
    }
    #endregion


    #region [.download_threads_semaphore.]
    /// <summary>
    /// 
    /// </summary>
    internal interface IDownloadThreadsSemaphoreEx : I_download_threads_semaphore
    {
        void ResetSemaphore( SemaphoreSlim semaphore );
        void ResetSemaphore( int degreeOfParallelism );
    }
    /// <summary>
    /// 
    /// </summary>
    internal sealed class cross_download_threads_semaphore : IDownloadThreadsSemaphoreEx
    {
        private SemaphoreHolder _SemaphoreHolder;
        private int             _CrossSemaphoreWaitAcquireCount;

        public cross_download_threads_semaphore( SemaphoreHolder semaphoreHolder ) => _SemaphoreHolder = semaphoreHolder;
        void IDisposable.Dispose()
        {
            if ( _SemaphoreHolder != null )
            {
                _SemaphoreHolder.Release( _CrossSemaphoreWaitAcquireCount );
                _SemaphoreHolder = null;
            }
        }

        public bool UseCrossDownloadInstanceParallelism { [M(O.AggressiveInlining)] get => true; }

        public void ResetSemaphore( SemaphoreSlim semaphore ) => _SemaphoreHolder.ResetSemaphore( semaphore );
        public void ResetSemaphore( int degreeOfParallelism ) => throw (new InvalidOperationException());

        public void Wait( CancellationToken ct ) //=> _Semaphore.Wait( ct );
        {
            //--!!!-- required here (because the calling context (task) is interrupted by the ct-CancellationToken --!!!--//
            Thread.BeginCriticalRegion();
            try
            {
                try { }
                finally
                {
                    _SemaphoreHolder.Wait( ct );
                    Interlocked.Increment( ref _CrossSemaphoreWaitAcquireCount );
                }
            }
            finally
            {
                Thread.EndCriticalRegion();
            }

            //ct.ThrowIfCancellationRequested();
        }
        public void Release() //=> _Semaphore.Release();
        {
            //--!!!-- non-required here (because calling context not-interrupted) --!!!--//
            Thread.BeginCriticalRegion();
            try
            {
                try { }
                finally
                {
                    Interlocked.Decrement( ref _CrossSemaphoreWaitAcquireCount );
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
            _Semaphore       = new SemaphoreSlim( degreeOfParallelism );
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

        public void ResetSemaphore( SemaphoreSlim semaphore ) => throw (new InvalidOperationException());
        public void ResetSemaphore( int degreeOfParallelism )
        {
            var semaphore = new SemaphoreSlim( degreeOfParallelism );
            _SemaphoreHolder.ResetSemaphore( semaphore );

            var semaphore_prev = Interlocked.Exchange( ref _Semaphore, semaphore );
            semaphore_prev.WaitAsyncAndDispose();
        }

        [M(O.AggressiveInlining)] public void Wait( CancellationToken ct ) => _SemaphoreHolder.Wait( ct );
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

            [M(O.AggressiveInlining)] public void ResetSemaphore( SemaphoreSlim semaphore ) { }
            [M(O.AggressiveInlining)] public void ResetSemaphore( int degreeOfParallelism ) { }

            [M(O.AggressiveInlining)] public void Wait( CancellationToken ct ) { }
            [M(O.AggressiveInlining)] public void Release() { }
        }

        #region [.fields.]
        private SemaphoreSlim   _CrossSemaphore;
        private SemaphoreHolder _CrossSemaphoreHolder;
        private dummy_download_threads_semaphore _dummy_download_threads_semaphore;
        #endregion

        #region [.ctor().]
        public download_threads_semaphore_factory( bool useCrossDownloadInstanceParallelism, 
                                                   //bool useMaxDegreeOfParallelism, 
                                                   int  maxDegreeOfParallelism )
        {
            this.UseCrossDownloadInstanceParallelism = useCrossDownloadInstanceParallelism;            
            this.MaxDegreeOfParallelism              = maxDegreeOfParallelism;
            this.UseMaxDegreeOfParallelism           = (0 < maxDegreeOfParallelism); //useMaxDegreeOfParallelism;

            _CrossSemaphore       = new SemaphoreSlim( this.MaxDegreeOfParallelism );
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

        public bool UseCrossDownloadInstanceParallelism { get; set; }
        public bool UseMaxDegreeOfParallelism           { get; set; }
        public int  MaxDegreeOfParallelism              { get; private set; }

        public void ResetMaxDegreeOfParallelism( int maxDegreeOfParallelism )
        {
            if ( this.MaxDegreeOfParallelism != maxDegreeOfParallelism )
            {
                var semaphore = new SemaphoreSlim( maxDegreeOfParallelism );
                _CrossSemaphoreHolder.ResetSemaphore( semaphore );

                var semaphore_prev = Interlocked.Exchange( ref _CrossSemaphore, semaphore );
                semaphore_prev.WaitAsyncAndDispose();

                this.MaxDegreeOfParallelism = maxDegreeOfParallelism;
            }
        }

        public IDownloadThreadsSemaphoreEx Get()
        {
            if ( this.UseCrossDownloadInstanceParallelism )
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
