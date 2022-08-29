using System;
using System.Collections.Concurrent;
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

    /// <summary>
    /// 
    /// </summary>
    internal struct interlocked_lock
    {
        private int _Lock;

        public bool TryEnter() => (Interlocked.CompareExchange( ref _Lock, 1, 0 ) == 0);
        public void Exit() => Interlocked.Exchange( ref _Lock, 0 );
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
            this.UseCrossDownloadInstanceParallelism = useCrossDownloadInstanceParallelism;            
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

        public bool UseCrossDownloadInstanceParallelism { get; set; }
        public bool UseMaxDegreeOfParallelism           { get; set; }
        public int  MaxDegreeOfParallelism              { get; private set; }

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

    #region [.throttler_by_speed.]
    /// <summary>
    /// 
    /// </summary>
    internal sealed class _unlimited_throttler_by_speed_t : I_throttler_by_speed_t, IDisposable
    {
        public _unlimited_throttler_by_speed_t() { }
        public void Dispose() { }

        public double? GetMaxSpeedThreshold() => null;
        public void ChangeMaxSpeedThreshold( double? max_speed_threshold_in_Mbps ) { }
        public void Start( Task task ) { }
        public void Restart( Task task ) { }
        public void End( Task task ) { }
        public void Throttle( Task task, CancellationToken ct ) { }
        public void TakeIntoAccountDownloadedBytes( Task task, int downloadedBytes ) { }
#if DEBUG
        public override string ToString() => "max_speed_threshold_in_Mbps: Max (Unlim)";
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class throttler_by_speed_t : I_throttler_by_speed_t, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private sealed class download_measure_t
        {
            private long _TotalDownloadBytes;
            public download_measure_t( long startMeasureDateTimeTicks ) => StartMeasureDateTimeTicks = startMeasureDateTimeTicks;
            public long StartMeasureDateTimeTicks { get; }
            public long GetTotalDownloadBytes() => Interlocked.Read( ref _TotalDownloadBytes );
            public void AddTotalDownloadBytes( int downloadBytes ) => Interlocked.Add( ref _TotalDownloadBytes, downloadBytes );
        }

        private double _Max_speed_threshold_in_Mbps;
        private CancellationTokenSource __Cts__;
        private ConcurrentDictionary< Task, download_measure_t > _DownloadMeasureDict;

        public throttler_by_speed_t( double max_speed_threshold_in_Mbps )
        {
            _Max_speed_threshold_in_Mbps = max_speed_threshold_in_Mbps;
            _DownloadMeasureDict = new ConcurrentDictionary< Task, download_measure_t >();
            __Cts__ = new CancellationTokenSource();
        }
        public void Dispose() => Get_Cts().Dispose_NoThrow();

        private CancellationTokenSource Get_Cts()
        {
            lock ( __Cts__ )
            {
                return (__Cts__);
            }
        }

        private double GetMaxSpeedThreshold_Internal() => Interlocked.CompareExchange( ref _Max_speed_threshold_in_Mbps, 0, 0 );
        public double? GetMaxSpeedThreshold() => GetMaxSpeedThreshold_Internal();
        public void ChangeMaxSpeedThreshold( double? max_speed_threshold_in_Mbps )
        {
            Interlocked.Exchange( ref _Max_speed_threshold_in_Mbps, max_speed_threshold_in_Mbps.GetValueOrDefault( double.MaxValue ) );
            Get_Cts().Cancel_NoThrow();
        }
        
        public void Start( Task task )
        {
            var suc = _DownloadMeasureDict.TryAdd( task, new download_measure_t( DateTime.Now.Ticks ) );
            Debug.Assert( suc );
        }
        public void Restart( Task task ) => _DownloadMeasureDict[ task ] = new download_measure_t( DateTime.Now.Ticks );
        public void End( Task task ) => _DownloadMeasureDict.TryRemove( task, out var _ );

        private static void Delay( int millisecondsDelay, CancellationToken ct )
        {
            try
            {
                Task.Delay( millisecondsDelay, ct ).Wait( ct );
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        private void Recreate_Cts_IfNeed()
        {
            lock ( __Cts__ )
            {
                if ( __Cts__.IsCancellationRequested )
                {
                    __Cts__.Dispose_NoThrow();
                    __Cts__ = new CancellationTokenSource();
                }
            }
        }
        public void Throttle( Task task, CancellationToken ct )
        {
            var dm = _DownloadMeasureDict[ task ];

            var totalDownloadBytes = dm.GetTotalDownloadBytes();
            if ( totalDownloadBytes == 0 ) return;

            var elapsedSeconds = new TimeSpan( DateTime.Now.Ticks - dm.StartMeasureDateTimeTicks ).TotalSeconds;
            var secondsDelay   = (Extensions.GetMbps( totalDownloadBytes ) / GetMaxSpeedThreshold_Internal()) - elapsedSeconds;
            var speed_in_Mbps  = Extensions.GetSpeedInMbps( totalDownloadBytes, elapsedSeconds );

            if ( 0 < secondsDelay )
            {
                Debug.WriteLine( $"(task: #{task.Id}), speed_in_Mbps: {speed_in_Mbps:N2}, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#} => secondsDelay: {secondsDelay:N2}" );

                using var join_ct = CancellationTokenSource.CreateLinkedTokenSource( ct, Get_Cts().Token );
                Delay( (int) (secondsDelay * 1_000), join_ct.Token );
                Recreate_Cts_IfNeed();
            }
            else
            {
                Debug.WriteLine( $"(task: #{task.Id}), speed_in_Mbps: {speed_in_Mbps:N2}, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#}" );
            }
        }
        public void TakeIntoAccountDownloadedBytes( Task task, int downloadBytes ) => _DownloadMeasureDict[ task ].AddTotalDownloadBytes( downloadBytes );
#if DEBUG
        public override string ToString() => $"max_speed_threshold_in_Mbps: {GetMaxSpeedThreshold()} Mbps";
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class throttler_by_speed_holder : I_throttler_by_speed_t, IDisposable
    {
        private I_throttler_by_speed_t __throttler_by_speed__;
        private ReaderWriterLockSlim   _RWLS;
        public throttler_by_speed_holder( double? max_speed_threshold_in_Mbps )
        {
            __throttler_by_speed__ = (0 < max_speed_threshold_in_Mbps.GetValueOrDefault()) ? new throttler_by_speed_t( max_speed_threshold_in_Mbps.Value ) : new _unlimited_throttler_by_speed_t();
            _RWLS = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );
        }
        public void Dispose()
        {            
            _throttler_by_speed.Dispose();
            _RWLS.Dispose();
        }

        private I_throttler_by_speed_t _throttler_by_speed
        {
            get
            {
                _RWLS.EnterReadLock();
                var p = __throttler_by_speed__;
                _RWLS.ExitReadLock();
                return (p);
            }
            set
            {
                _RWLS.EnterWriteLock();
                __throttler_by_speed__ = value;
                _RWLS.ExitWriteLock();
            }
        }

        public double? GetMaxSpeedThreshold() => _throttler_by_speed.GetMaxSpeedThreshold();
        public void ChangeMaxSpeedThreshold( double? max_speed_threshold_in_Mbps )
        {
            var current_max_speed_threshold_in_Mbps = _throttler_by_speed.GetMaxSpeedThreshold();
            if ( max_speed_threshold_in_Mbps.HasValue )
            {
                if ( current_max_speed_threshold_in_Mbps.HasValue )
                {
                    _throttler_by_speed.ChangeMaxSpeedThreshold( max_speed_threshold_in_Mbps );
                }
                else
                {
                    _throttler_by_speed = new throttler_by_speed_t( max_speed_threshold_in_Mbps.Value ); //_throttler_by_speed = StartNew( max_speed_threshold_in_Mbps );
                }
            }
            else if ( current_max_speed_threshold_in_Mbps.HasValue )
            {
                _throttler_by_speed = new _unlimited_throttler_by_speed_t(); //_throttler_by_speed = StartNew( max_speed_threshold_in_Mbps );
            }
            //else
            //{
            //    _throttler_by_speed.ChangeMaxSpeedThreshold( max_speed_threshold_in_Mbps );
            //}
        }
        public void Start( Task task ) => _throttler_by_speed.Start( task );
        public void Restart( Task task ) => _throttler_by_speed.Restart( task );
        public void End( Task task ) => _throttler_by_speed.End( task );
        public void Throttle( Task task, CancellationToken ct ) => _throttler_by_speed.Throttle( task, ct );
        public void TakeIntoAccountDownloadedBytes( Task task, int downloadedBytes ) => _throttler_by_speed.TakeIntoAccountDownloadedBytes( task, downloadedBytes );
    }
    #endregion
}
