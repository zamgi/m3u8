using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.controllers
{
    #region [.throttler_by_speed.]
//    /// <summary>
//    /// 
//    /// </summary>
//    internal sealed class _unlimited_throttler_by_speed_t : I_throttler_by_speed_t, IDisposable
//    {
//        public _unlimited_throttler_by_speed_t() { }
//        public void Dispose() { }

//        public double? GetMaxSpeedThreshold() => null;
//        public void ChangeMaxSpeedThreshold( double? max_speed_threshold_in_Mbps ) { }
//        public void Start( Task task ) { }
//        public void Restart( Task task ) { }
//        public void End( Task task ) { }
//        public double? Throttle( Task task, CancellationToken ct ) => null;
//        public void TakeIntoAccountDownloadedBytes( Task task, int downloadedBytes ) { }
//#if DEBUG
//        public override string ToString() => "max_speed_threshold_in_Mbps: Max (Unlim)";
//#endif
//    }

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
            //public void AddTotalDownloadBytes( int downloadBytes ) => Interlocked.Add( ref _TotalDownloadBytes, downloadBytes );
            
            /// <summary>
            /// 
            /// </summary>
            private sealed class last_measure_tuple_t
            {
                public long MeasureDateTimeTicks  { get; set; }
                public long IntervalDateTimeTicks { get; set; }
                public int  DownloadBytes         { get; set; }
            }

            private last_measure_tuple_t _LastMeasureTuple = new last_measure_tuple_t();
            public void AddTotalDownloadBytes( long measureDateTimeTicks, int downloadBytes )
            {
                Interlocked.Add( ref _TotalDownloadBytes, downloadBytes );

                var prev_MeasureDateTimeTicks = Interlocked.CompareExchange( ref _LastMeasureTuple, null, null ).MeasureDateTimeTicks;
                var lmt = new last_measure_tuple_t()
                { 
                    IntervalDateTimeTicks = measureDateTimeTicks - prev_MeasureDateTimeTicks, 
                    MeasureDateTimeTicks  = measureDateTimeTicks, 
                    DownloadBytes         = downloadBytes 
                };
                Interlocked.Exchange( ref _LastMeasureTuple, lmt );
            }
            public (long intervalDateTimeTicks, int downloadBytes) GetLastDownloadBytes()
            {
                var lmt = Interlocked.CompareExchange( ref _LastMeasureTuple, null, null );
                return (lmt.IntervalDateTimeTicks, lmt.DownloadBytes);
            }
        }

        private double _Max_speed_threshold_in_Mbps;
        private CancellationTokenSource __Cts__;
        private ConcurrentDictionary< Task, download_measure_t > _DownloadMeasureDict;

        public throttler_by_speed_t( double? max_speed_threshold_in_Mbps ) : this( max_speed_threshold_in_Mbps.GetValueOrDefault( double.MaxValue ) ) { }
        public throttler_by_speed_t( double max_speed_threshold_in_Mbps )
        {
            _Max_speed_threshold_in_Mbps = max_speed_threshold_in_Mbps;
            _DownloadMeasureDict = new ConcurrentDictionary< Task, download_measure_t >();
            __Cts__ = new CancellationTokenSource();
        }
        public void Dispose()
        {
            var cts = Get_Cts();
            cts.Cancel_NoThrow();
            cts.Dispose_NoThrow();
        }

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

        private static Func< Task, download_measure_t > _Create__download_measure_t_func = new Func< Task, download_measure_t >( _ => Create__download_measure_t() );
        private static download_measure_t Create__download_measure_t() => new download_measure_t( Stopwatch.GetTimestamp() );
        public void Start( Task task )
        {
            var suc = _DownloadMeasureDict.TryAdd( task, Create__download_measure_t() );
            Debug.Assert( suc );
        }
        public void Restart( Task task ) => _DownloadMeasureDict[ task ] = new download_measure_t( Stopwatch.GetTimestamp() );
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
        public double? Throttle( Task task, CancellationToken ct )
        {
            var dm = _DownloadMeasureDict.GetOrAdd( task, _Create__download_measure_t_func );

            var totalDownloadBytes = dm.GetTotalDownloadBytes();
            if ( totalDownloadBytes == 0 ) return (null);

            var nowTicks = Stopwatch.GetTimestamp();
            var elapsedSeconds = new TimeSpan( nowTicks - dm.StartMeasureDateTimeTicks ).TotalSeconds;
            var secondsDelay   = (Extensions.GetMbps( totalDownloadBytes ) / GetMaxSpeedThreshold_Internal()) - elapsedSeconds;

            var last = dm.GetLastDownloadBytes();
            var last_elapsedSeconds = new TimeSpan( last.intervalDateTimeTicks ).TotalSeconds;
            var instantSpeedInMbps  = Extensions.GetSpeedInMbps( last.downloadBytes, last_elapsedSeconds );

            if ( 0 < secondsDelay )
            {
                Debug.WriteLine( $"(task: #{task.Id}), instant-speed_in_Mbps: {instantSpeedInMbps:N2}, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#} => secondsDelay: {secondsDelay:N2}" );

                using var join_ct = CancellationTokenSource.CreateLinkedTokenSource( ct, Get_Cts().Token );
                Delay( (int) (secondsDelay * 1_000), join_ct.Token );
                Recreate_Cts_IfNeed();
            }
            else
            {
                Debug.WriteLine( $"(task: #{task.Id}), instant-speed_in_Mbps: {instantSpeedInMbps:N2}, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#}" );
            }
            return (instantSpeedInMbps);

            #region comm. prev. total-speed_in_Mbps.
            /*
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
            return (speed_in_Mbps);
            //*/
            #endregion
        }
        public void TakeIntoAccountDownloadedBytes( Task task, int downloadBytes ) 
            => _DownloadMeasureDict.GetOrAdd( task, _Create__download_measure_t_func ).AddTotalDownloadBytes( measureDateTimeTicks: Stopwatch.GetTimestamp(), downloadBytes );
#if DEBUG
        public override string ToString() => $"max_speed_threshold_in_Mbps: {GetMaxSpeedThreshold()} Mbps";
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class throttler_by_speed_holder : I_throttler_by_speed_t, IDisposable
    {
        //private I_throttler_by_speed_t __throttler_by_speed__;
        //private ReaderWriterLockSlim   _RWLS;
        private I_throttler_by_speed_t _throttler_by_speed;
        public throttler_by_speed_holder( double? max_speed_threshold_in_Mbps )
        {
            _throttler_by_speed = new throttler_by_speed_t( max_speed_threshold_in_Mbps );
            //__throttler_by_speed__ = (0 < max_speed_threshold_in_Mbps.GetValueOrDefault()) ? new throttler_by_speed_t( max_speed_threshold_in_Mbps.Value ) : new _unlimited_throttler_by_speed_t();
            //_RWLS = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );
        }
        public void Dispose()
        {            
            _throttler_by_speed.Dispose();
            //_RWLS.Dispose();
        }

        //private I_throttler_by_speed_t _throttler_by_speed
        //{
        //    get
        //    {
        //        _RWLS.EnterReadLock();
        //        var p = __throttler_by_speed__;
        //        _RWLS.ExitReadLock();
        //        return (p);
        //    }
        //    set
        //    {
        //        _RWLS.EnterWriteLock();
        //        __throttler_by_speed__ = value;
        //        _RWLS.ExitWriteLock();
        //    }
        //}

        public double? GetMaxSpeedThreshold() => _throttler_by_speed.GetMaxSpeedThreshold();
        public void ChangeMaxSpeedThreshold( double? max_speed_threshold_in_Mbps ) => _throttler_by_speed.ChangeMaxSpeedThreshold( max_speed_threshold_in_Mbps );
        //{
        //    var current_max_speed_threshold_in_Mbps = _throttler_by_speed.GetMaxSpeedThreshold();
        //    if ( max_speed_threshold_in_Mbps.HasValue )
        //    {
        //        if ( current_max_speed_threshold_in_Mbps.HasValue )
        //        {
        //            _throttler_by_speed.ChangeMaxSpeedThreshold( max_speed_threshold_in_Mbps );
        //        }
        //        else
        //        {
        //            var temp = _throttler_by_speed;
        //            _throttler_by_speed = new throttler_by_speed_t( max_speed_threshold_in_Mbps.Value );
        //            temp.Dispose();
        //        }
        //    }
        //    else if ( current_max_speed_threshold_in_Mbps.HasValue )
        //    {
        //        var temp = _throttler_by_speed;
        //        _throttler_by_speed = new _unlimited_throttler_by_speed_t();
        //        temp.Dispose();
        //    }
        //}
        public void Start( Task task ) => _throttler_by_speed.Start( task );
        public void Restart( Task task ) => _throttler_by_speed.Restart( task );
        public void End( Task task ) => _throttler_by_speed.End( task );
        public double? Throttle( Task task, CancellationToken ct ) => _throttler_by_speed.Throttle( task, ct );
        public void TakeIntoAccountDownloadedBytes( Task task, int downloadedBytes ) => _throttler_by_speed.TakeIntoAccountDownloadedBytes( task, downloadedBytes );
    }
    #endregion
}
