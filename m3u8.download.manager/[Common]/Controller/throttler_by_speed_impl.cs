﻿using System;
#if THROTTLER__V1
using System.Collections.Concurrent;
#endif
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.controllers
{
#if THROTTLER__V1
    /// <summary>
    /// 
    /// </summary>
    internal sealed class throttler_by_speed_impl__v1 : I_throttler_by_speed__v1_t, IDisposable
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

        private decimal _Max_speed_threshold_in_Mbps;
        private SpinLock _Lock;
        private CancellationTokenSource __Cts__;
        private ConcurrentDictionary< Task, download_measure_t > _DownloadMeasureDict;

        public throttler_by_speed_impl__v1( decimal? max_speed_threshold_in_Mbps ) : this( max_speed_threshold_in_Mbps.GetValueOrDefault( decimal.MaxValue ) ) { }
        public throttler_by_speed_impl__v1( decimal max_speed_threshold_in_Mbps )
        {
            _Max_speed_threshold_in_Mbps = max_speed_threshold_in_Mbps;
            _DownloadMeasureDict = new ConcurrentDictionary< Task, download_measure_t >();
            __Cts__ = new CancellationTokenSource();
            _Lock = new SpinLock();
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

        private decimal GetMaxSpeedThreshold_Internal() //=> Interlocked.CompareExchange( ref _Max_speed_threshold_in_Mbps, 0, 0 );
        {
            var lockTaken = false;
            try
            {
                _Lock.Enter( ref lockTaken );
                return (_Max_speed_threshold_in_Mbps);
            }
            finally
            {
                if ( lockTaken )
                {
                    _Lock.Exit( true );
                }
            }
        }
        public decimal? GetMaxSpeedThreshold() => GetMaxSpeedThreshold_Internal();
        public void ChangeMaxSpeedThreshold( decimal? max_speed_threshold_in_Mbps )
        {
            var lockTaken = false;
            try
            {
                _Lock.Enter( ref lockTaken );
                _Max_speed_threshold_in_Mbps = max_speed_threshold_in_Mbps.GetValueOrDefault( decimal.MaxValue );
            }
            finally
            {
                if ( lockTaken )
                {
                    _Lock.Exit( true );
                }
            }
            //Interlocked.Exchange( ref _Max_speed_threshold_in_Mbps, max_speed_threshold_in_Mbps.GetValueOrDefault( decimal.MaxValue ) );
            _DownloadMeasureDict.Clear();
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

        private static void Delay_NoThrow( int millisecondsDelay, CancellationToken ct )
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
            var secondsDelay   = (Extensions.GetMbps( totalDownloadBytes ) / (double) GetMaxSpeedThreshold_Internal()) - elapsedSeconds;

            var last = dm.GetLastDownloadBytes();
            var last_elapsedSeconds = TimeSpan.FromTicks( last.intervalDateTimeTicks ).TotalSeconds;
            var instantSpeedInMbps  = (0 < last_elapsedSeconds) ? Extensions.GetSpeedInMbps( last.downloadBytes, last_elapsedSeconds ) : (double?) null;

            if ( 0 < secondsDelay )
            {
                const double MAX_SECONDS_DELAY_BY_STEP = 1;//10;
                var secondsDelay_2 = Math.Min( MAX_SECONDS_DELAY_BY_STEP, secondsDelay );

                //---Debug.WriteLine( $"instant-speed: {instantSpeedInMbps:N2} Mbps, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#} => secondsDelay: {secondsDelay:N2}" );
                Debug.WriteLine( $"secondsDelay: {secondsDelay:N4}, secondsDelay_2: {secondsDelay_2:N4}" );

                using var join_ct = CancellationTokenSource.CreateLinkedTokenSource( ct, Get_Cts().Token );
                Delay_NoThrow( (int) (secondsDelay_2 * 1_000), join_ct.Token );
                Recreate_Cts_IfNeed();
            }
            //else
            //{
            //    Debug.WriteLine( $"instant-speed: {instantSpeedInMbps:N2} Mbps, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#}" );
            //}
            return (instantSpeedInMbps);
        }
        public void TakeIntoAccountDownloadedBytes( Task task, int downloadBytes ) 
            => _DownloadMeasureDict.GetOrAdd( task, _Create__download_measure_t_func ).AddTotalDownloadBytes( measureDateTimeTicks: Stopwatch.GetTimestamp(), downloadBytes );
#if DEBUG
        public override string ToString() => $"max_speed_threshold: {GetMaxSpeedThreshold()} Mbps";
#endif
    }
#endif
#if THROTTLER__V2
    /// <summary>
    /// 
    /// </summary>
    internal sealed class throttler_by_speed_impl__v2 : I_throttler_by_speed__v2_t, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private sealed class download_measure_t
        {
            private long _TotalDownloadBytes;
            private long _StartMeasureDateTimeTicks;
            public download_measure_t( long startMeasureDateTimeTicks ) => _StartMeasureDateTimeTicks = startMeasureDateTimeTicks;
            public long StartMeasureDateTimeTicks => _StartMeasureDateTimeTicks;
            public long GetTotalDownloadBytes() => _TotalDownloadBytes;
            
            /// <summary>
            /// 
            /// </summary>
            private readonly struct last_measure_tuple_t
            {
                public long MeasureDateTimeTicks  { get; init; }
                public long IntervalDateTimeTicks { get; init; }
                public int  DownloadBytes         { get; init; }
            }

            private last_measure_tuple_t _LastMeasureTuple = new last_measure_tuple_t();
            public void AddTotalDownloadBytes( long measureDateTimeTicks, int downloadBytes )
            {
                _TotalDownloadBytes += downloadBytes;

                var prev_MeasureDateTimeTicks = _LastMeasureTuple.MeasureDateTimeTicks;
                var lmt = new last_measure_tuple_t()
                { 
                    IntervalDateTimeTicks = measureDateTimeTicks - prev_MeasureDateTimeTicks, 
                    MeasureDateTimeTicks  = measureDateTimeTicks, 
                    DownloadBytes         = downloadBytes 
                };
                _LastMeasureTuple = lmt;
            }
            public (long intervalDateTimeTicks, int downloadBytes) GetLastDownloadBytes() => (_LastMeasureTuple.IntervalDateTimeTicks, _LastMeasureTuple.DownloadBytes);
        }

        private decimal _Max_speed_threshold_in_Mbps;
        private SpinLock _SpinLock;
        private object _Lock;
        private CancellationTokenSource __Cts__;
        private download_measure_t _DownloadMeasure;

        public throttler_by_speed_impl__v2( decimal? max_speed_threshold_in_Mbps ) : this( max_speed_threshold_in_Mbps.GetValueOrDefault( decimal.MaxValue ) ) { }
        public throttler_by_speed_impl__v2( decimal max_speed_threshold_in_Mbps )
        {
            _Max_speed_threshold_in_Mbps = max_speed_threshold_in_Mbps;
            _DownloadMeasure = Create__download_measure();
            __Cts__   = new CancellationTokenSource();
            _SpinLock = new SpinLock();
            _Lock     = new object();
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

        private decimal GetMaxSpeedThreshold_Internal()
        {
            var lockTaken = false;
            try
            {
                _SpinLock.Enter( ref lockTaken );
                return (_Max_speed_threshold_in_Mbps);
            }
            finally
            {
                if ( lockTaken )
                {
                    _SpinLock.Exit( true );
                }
            }
        }
        public decimal? GetMaxSpeedThreshold() => GetMaxSpeedThreshold_Internal();
        public void ChangeMaxSpeedThreshold( decimal? max_speed_threshold_in_Mbps )
        {
            var lockTaken = false;
            try
            {
                _SpinLock.Enter( ref lockTaken );
                _Max_speed_threshold_in_Mbps = max_speed_threshold_in_Mbps.GetValueOrDefault( decimal.MaxValue );
            }
            finally
            {
                if ( lockTaken )
                {
                    _SpinLock.Exit( true );
                }
            }
            Get_Cts().Cancel_NoThrow();
        }

        [M(O.AggressiveInlining)] private static download_measure_t Create__download_measure() => new download_measure_t( Stopwatch.GetTimestamp() );
        [M(O.AggressiveInlining)] private void Recreate__download_measure_with_lock()
        {
            lock ( _Lock )
            {
                _DownloadMeasure = Create__download_measure();
            }
        }
        public void Start() => Recreate__download_measure_with_lock();
        public void Restart() => Recreate__download_measure_with_lock();
        public void End() => Recreate__download_measure_with_lock(); //{ }

        private static void Delay_NoThrow( int millisecondsDelay, CancellationToken ct )
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
        public double? Throttle( CancellationToken ct )
        {
            lock ( _Lock )
            {
                var totalDownloadBytes = _DownloadMeasure.GetTotalDownloadBytes();
                if ( totalDownloadBytes == 0 ) return (null);

                var nowTicks = Stopwatch.GetTimestamp();
                var elapsedSeconds = new TimeSpan( nowTicks - _DownloadMeasure.StartMeasureDateTimeTicks ).TotalSeconds;
                var secondsDelay   = (Extensions.GetMbps( totalDownloadBytes ) / (double) GetMaxSpeedThreshold_Internal()) - elapsedSeconds;

                var last = _DownloadMeasure.GetLastDownloadBytes();
                var last_elapsedSeconds = TimeSpan.FromTicks( last.intervalDateTimeTicks ).TotalSeconds;
                var instantSpeedInMbps  = (0 < last_elapsedSeconds) ? Extensions.GetSpeedInMbps( last.downloadBytes, last_elapsedSeconds ) : (double?) null;

                if ( 0 < secondsDelay )
                {
                    const double MAX_SECONDS_DELAY_BY_STEP = 1; //10;
                    var secondsDelay_2 = Math.Min( MAX_SECONDS_DELAY_BY_STEP, secondsDelay );

                    //---Debug.WriteLine( $"instant-speed: {instantSpeedInMbps:N2} Mbps, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#} => secondsDelay: {secondsDelay:N2}" );
                    Debug.WriteLine( $"secondsDelay: {secondsDelay:N4}, secondsDelay_2: {secondsDelay_2:N4}" );

                    using var join_ct = CancellationTokenSource.CreateLinkedTokenSource( ct, Get_Cts().Token );
                    Delay_NoThrow( (int) (secondsDelay_2 * 1_000), join_ct.Token );
                    Recreate_Cts_IfNeed();
                }
                //else
                //{
                //    Debug.WriteLine( $"instant-speed: {instantSpeedInMbps:N2} Mbps, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#}" );
                //}
                return (instantSpeedInMbps);
            }
        }
        public void TakeIntoAccountDownloadedBytes( int downloadBytes )
        {
            lock ( _Lock )
            {
                _DownloadMeasure.AddTotalDownloadBytes( measureDateTimeTicks: Stopwatch.GetTimestamp(), downloadBytes );
            }
        }
#if DEBUG
        public override string ToString() => $"max_speed_threshold: {GetMaxSpeedThreshold()} Mbps";
#endif
    }
#endif
}
