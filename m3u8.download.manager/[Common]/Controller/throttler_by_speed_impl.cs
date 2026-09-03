using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.controllers
{
//    /// <summary>
//    /// 
//    /// </summary>
//    internal sealed class throttler_by_speed_impl__PREV : i_throttler_by_speed_t, IDisposable
//    {
//        /// <summary>
//        /// 
//        /// </summary>
//        private sealed class download_measure_t
//        {
//            private long _TotalDownloadBytes;
//            private long _StartMeasureDateTimeTicks;
//            private last_measure_tuple_t _LastMeasureTuple;
//            public download_measure_t( long startMeasureDateTimeTicks )
//            {
//                _StartMeasureDateTimeTicks = startMeasureDateTimeTicks;
//                _LastMeasureTuple          = new last_measure_tuple_t();
//            }
//            public long StartMeasureDateTimeTicks => _StartMeasureDateTimeTicks;
//            public long GetTotalDownloadBytes() => _TotalDownloadBytes;
            
//            /// <summary>
//            /// 
//            /// </summary>
//            private readonly struct last_measure_tuple_t
//            {
//                public long MeasureDateTimeTicks  { get; init; }
//                public long IntervalDateTimeTicks { get; init; }
//                public int  DownloadBytes         { get; init; }

//                public override string ToString() => $"DownloadBytes={DownloadBytes}";
//            }

//            public void AddTotalDownloadBytes( long measureDateTimeTicks, int downloadBytes )
//            {
//                _TotalDownloadBytes += downloadBytes;

//                var prev_MeasureDateTimeTicks = _LastMeasureTuple.MeasureDateTimeTicks;
//                var lmt = new last_measure_tuple_t()
//                { 
//                    IntervalDateTimeTicks = measureDateTimeTicks - prev_MeasureDateTimeTicks, 
//                    MeasureDateTimeTicks  = measureDateTimeTicks, 
//                    DownloadBytes         = downloadBytes 
//                };
//                _LastMeasureTuple = lmt;
//            }
//            public (long intervalDateTimeTicks, int downloadBytes) GetLastDownloadInfo() => (_LastMeasureTuple.IntervalDateTimeTicks, _LastMeasureTuple.DownloadBytes);

//            public override string ToString() => $"TotalDownloadBytes={_TotalDownloadBytes}, LastMeasure={_LastMeasureTuple}";
//        }

//        private decimal _Max_speed_threshold_in_Mbps;
//        private SpinLock _SpinLock;
//        private object _Lock;
//        private CancellationTokenSource __Cts__;
//        //private object _Lock_4__Cts__;
//        private download_measure_t _DownloadMeasure;

//        public throttler_by_speed_impl__PREV( decimal? max_speed_threshold_in_Mbps ) : this( max_speed_threshold_in_Mbps.GetValueOrDefault( decimal.MaxValue ) ) { }
//        public throttler_by_speed_impl__PREV( decimal max_speed_threshold_in_Mbps )
//        {
//            _Max_speed_threshold_in_Mbps = max_speed_threshold_in_Mbps;
//            _DownloadMeasure = Create__download_measure();
//            __Cts__   = new CancellationTokenSource();
//            //_Lock_4__Cts__ = new object();
//            _SpinLock = new SpinLock();
//            _Lock     = new object();
//        }
//        public void Dispose()
//        {
//            var cts = Get_Cts();
//            cts.Cancel_NoThrow();
//            cts.Dispose_NoThrow();
//        }

//        private CancellationTokenSource Get_Cts()
//        {
//            lock ( /*_Lock_4__Cts__*/__Cts__ )
//            {
//                return (__Cts__);
//            }
//        }
//        private CancellationTokenSource Get_Cts_Recreate_IfCanceled()
//        {
//            lock ( /*_Lock_4__Cts__*/__Cts__ )
//            {
//                if ( __Cts__.IsCancellationRequested )
//                {
//                    __Cts__.Dispose_NoThrow();
//                    __Cts__ = new CancellationTokenSource();
//                }
//                return (__Cts__);
//            }
//        }
//        //private void Recreate_Cts_IfNeed()
//        //{
//        //    lock ( /*_Lock_4__Cts__*/__Cts__ )
//        //    {
//        //        if ( __Cts__.IsCancellationRequested )
//        //        {
//        //            __Cts__.Dispose_NoThrow();
//        //            __Cts__ = new CancellationTokenSource();
//        //        }
//        //    }
//        //}

//        private decimal GetMaxSpeedThreshold_Internal()
//        {
//            var lockTaken = false;
//            try
//            {
//                _SpinLock.Enter( ref lockTaken );
//                return (_Max_speed_threshold_in_Mbps);
//            }
//            finally
//            {
//                if ( lockTaken )
//                {
//                    _SpinLock.Exit( true );
//                }
//            }
//        }
//        public decimal? GetMaxSpeedThreshold() => GetMaxSpeedThreshold_Internal();
//        public void ChangeMaxSpeedThreshold( decimal? max_speed_threshold_in_Mbps )
//        {
//            var lockTaken = false;
//            try
//            {
//                _SpinLock.Enter( ref lockTaken );
//                _Max_speed_threshold_in_Mbps = max_speed_threshold_in_Mbps.GetValueOrDefault( decimal.MaxValue );                
//            }
//            finally
//            {
//                if ( lockTaken )
//                {
//                    _SpinLock.Exit( true );
//                }
//            }
//            Get_Cts().Cancel_NoThrow();
//            Recreate__download_measure_with_lock();
//        }

//        [M(O.AggressiveInlining)] private static download_measure_t Create__download_measure() => new download_measure_t( Stopwatch.GetTimestamp() );
//        [M(O.AggressiveInlining)] private void Recreate__download_measure_with_lock()
//        {
//            lock ( _Lock )
//            {
//                _DownloadMeasure = Create__download_measure();
//            }
//        }
//        public void Start() => Recreate__download_measure_with_lock();
//        public void Restart() => Recreate__download_measure_with_lock();
//        public void End() => Recreate__download_measure_with_lock(); //{ }

//        private static void Delay_NoThrow( int millisecondsDelay, CancellationToken ct )
//        {
//            try
//            {
//                Task.Delay( millisecondsDelay, ct ).Wait( ct );
//            }
//            catch ( Exception ex )
//            {
//                Debug.WriteLine( ex );
//            }
//        }
//        //private List< (double secondsDelay, double? instantSpeedInMbps) > _SecondsDelays = new List< (double secondsDelay, double? instantSpeedInMbps) >();
//        public double? Throttle( CancellationToken ct )
//        {
//            lock ( _Lock )
//            {
//                var totalDownloadBytes = _DownloadMeasure.GetTotalDownloadBytes();
//                if ( totalDownloadBytes == 0 ) return (null);

//                var nowTicks       = Stopwatch.GetTimestamp();
//                var elapsedSeconds = new TimeSpan( nowTicks - _DownloadMeasure.StartMeasureDateTimeTicks ).TotalSeconds;
//                var secondsDelay   = (Extensions_4_DownloadRow.GetMbps( totalDownloadBytes ) / (double) GetMaxSpeedThreshold_Internal()) - elapsedSeconds;

//                var last                = _DownloadMeasure.GetLastDownloadInfo();
//                var last_elapsedSeconds = TimeSpan.FromTicks( last.intervalDateTimeTicks ).TotalSeconds;
//                var instantSpeedInMbps  = (0 < last_elapsedSeconds) ? Extensions_4_DownloadRow.GetSpeedInMbps( last.downloadBytes, last_elapsedSeconds ) : (double?) null;

//                //_SecondsDelays.Add( (secondsDelay, instantSpeedInMbps) );
//                if ( 0 < secondsDelay )
//                {
//                    const double MAX_SECONDS_DELAY_BY_STEP = 1; //10;
//                    var secondsDelay_2 = Math.Min( MAX_SECONDS_DELAY_BY_STEP, secondsDelay );

//                    //---Debug.WriteLine( $"instant-speed: {instantSpeedInMbps:N2} Mbps, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#} => secondsDelay: {secondsDelay:N2}" );
//                    Debug.WriteLine( $"secondsDelay: {secondsDelay:N4}, secondsDelay_2: {secondsDelay_2:N4}" );

//                    using var join_ct = CancellationTokenSource.CreateLinkedTokenSource( ct, /*Get_Cts()*/Get_Cts_Recreate_IfCanceled().Token );
//                    Delay_NoThrow( (int) (secondsDelay_2 * 1_000), join_ct.Token );
//                    //---Recreate_Cts_IfNeed();
//                }
//                //else
//                //{
//                //    Debug.WriteLine( $"instant-speed: {instantSpeedInMbps:N2} Mbps, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#}" );
//                //}
//                return (instantSpeedInMbps);
//            }
//        }
//        public void TakeIntoAccountDownloadedBytes( int downloadBytes )
//        {
//            lock ( _Lock )
//            {
//                _DownloadMeasure.AddTotalDownloadBytes( measureDateTimeTicks: Stopwatch.GetTimestamp(), downloadBytes );
//            }
//        }
//#if DEBUG
//        public override string ToString() => $"max_speed_threshold: {GetMaxSpeedThreshold()} Mbps";
//#endif
//    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class throttler_by_speed_impl : i_throttler_by_speed_t, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private sealed class download_measure_t
        {
            private long _TotalDownloadBytes;
            private long _StartMeasureDateTimeTicks;
            private last_measure_tuple_t _LastMeasureTuple;
            private decimal _Max_speed_threshold_in_Mbps;
            public download_measure_t( long startMeasureDateTimeTicks, decimal max_speed_threshold_in_Mbps )
            {
                _StartMeasureDateTimeTicks = startMeasureDateTimeTicks;
                _LastMeasureTuple          = new last_measure_tuple_t();
                _Max_speed_threshold_in_Mbps = max_speed_threshold_in_Mbps;
            }
            public long StartMeasureDateTimeTicks => _StartMeasureDateTimeTicks;
            public long GetTotalDownloadBytes() => _TotalDownloadBytes;
            public decimal Max_speed_threshold_in_Mbps => _Max_speed_threshold_in_Mbps;

            /// <summary>
            /// 
            /// </summary>
            private readonly struct last_measure_tuple_t
            {
                public long MeasureDateTimeTicks  { get; init; }
                public long IntervalDateTimeTicks { get; init; }
                public int  DownloadBytes         { get; init; }

                public override string ToString() => $"DownloadBytes={DownloadBytes}";
            }

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
            public (long intervalDateTimeTicks, int downloadBytes) GetLastDownloadInfo() => (_LastMeasureTuple.IntervalDateTimeTicks, _LastMeasureTuple.DownloadBytes);

            public override string ToString() => $"TotalDownloadBytes={_TotalDownloadBytes}, LastMeasure={_LastMeasureTuple}";
        }
        
        private CancellationTokenSource __Cts__;
        private download_measure_t _DownloadMeasure;
        private object _Lock;

        public throttler_by_speed_impl( decimal? max_speed_threshold_in_Mbps ) : this( max_speed_threshold_in_Mbps.GetValueOrDefault( decimal.MaxValue ) ) { }
        public throttler_by_speed_impl( decimal max_speed_threshold_in_Mbps )
        {
            _DownloadMeasure = new download_measure_t( Stopwatch.GetTimestamp(), max_speed_threshold_in_Mbps );
            __Cts__ = new CancellationTokenSource();
            _Lock   = new object();
        }
        public void Dispose()
        {
            var cts = Get_Cts();
            cts.Cancel_NoThrow();
            cts.Dispose_NoThrow();
        }

        private CancellationTokenSource Get_Cts()
        {
            lock ( /*_Lock_4__Cts__*/__Cts__ )
            {
                return (__Cts__);
            }
        }
        private CancellationTokenSource Get_Cts_Recreate_IfCanceled()
        {
            lock ( /*_Lock_4__Cts__*/__Cts__ )
            {
                if ( __Cts__.IsCancellationRequested )
                {
                    __Cts__.Dispose_NoThrow();
                    __Cts__ = new CancellationTokenSource();
                }
                return (__Cts__);
            }
        }

        public decimal? GetMaxSpeedThreshold()
        {
            lock ( _Lock )
            {
                return (_DownloadMeasure.Max_speed_threshold_in_Mbps);
            }
        }
        public void ChangeMaxSpeedThreshold( decimal? max_speed_threshold_in_Mbps )
        {
            //Get_Cts().Cancel_NoThrow();
            lock ( _Lock ) //здесь УИ виснет - не может долго войти если скорость понижается
            {
                _DownloadMeasure = new download_measure_t( Stopwatch.GetTimestamp(), max_speed_threshold_in_Mbps.GetValueOrDefault( decimal.MaxValue ) );
            }
            Get_Cts().Cancel_NoThrow();
        }

        [M(O.AggressiveInlining)] private void Recreate__download_measure_with_lock()
        {
            lock ( _Lock )
            {
                _DownloadMeasure = new download_measure_t( Stopwatch.GetTimestamp(), _DownloadMeasure.Max_speed_threshold_in_Mbps );
            }
        }
        public void Start() => Recreate__download_measure_with_lock();
        public void Restart() => Recreate__download_measure_with_lock();
        public void End() => Recreate__download_measure_with_lock();

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
        //private List< (double secondsDelay, double? instantSpeedInMbps) > _SecondsDelays = new List< (double secondsDelay, double? instantSpeedInMbps) >();
        public double? Throttle( CancellationToken ct )
        {
            download_measure_t local_DownloadMeasure;
            lock ( _Lock )
            {
                local_DownloadMeasure = _DownloadMeasure;
            }

            var totalDownloadBytes = local_DownloadMeasure.GetTotalDownloadBytes();
            if ( totalDownloadBytes == 0 ) return (null);

            var nowTicks       = Stopwatch.GetTimestamp();
            var elapsedSeconds = new TimeSpan( nowTicks - local_DownloadMeasure.StartMeasureDateTimeTicks ).TotalSeconds;
            var secondsDelay   = (Extensions_4_DownloadRow.GetMbps( totalDownloadBytes ) / (double) local_DownloadMeasure.Max_speed_threshold_in_Mbps) - elapsedSeconds;

            var last                = local_DownloadMeasure.GetLastDownloadInfo();
            var last_elapsedSeconds = TimeSpan.FromTicks( last.intervalDateTimeTicks ).TotalSeconds;
            var instantSpeedInMbps  = (0 < last_elapsedSeconds) ? Extensions_4_DownloadRow.GetSpeedInMbps( last.downloadBytes, last_elapsedSeconds ) : (double?) null;

            //_SecondsDelays.Add( (secondsDelay, instantSpeedInMbps) );
            if ( 0 < secondsDelay )
            {
                const double MAX_SECONDS_DELAY_BY_STEP = 1; //10;
                var secondsDelay_2 = Math.Min( MAX_SECONDS_DELAY_BY_STEP, secondsDelay );

                //---Debug.WriteLine( $"instant-speed: {instantSpeedInMbps:N2} Mbps, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#} => secondsDelay: {secondsDelay:N2}" );
                Debug.WriteLine( $"secondsDelay: {secondsDelay:N4}, secondsDelay_2: {secondsDelay_2:N4}" );

                using var join_ct = CancellationTokenSource.CreateLinkedTokenSource( ct, /*Get_Cts()*/Get_Cts_Recreate_IfCanceled().Token );
                Delay_NoThrow( (int) (secondsDelay_2 * 1_000), join_ct.Token );
                //---Recreate_Cts_IfNeed();
            }
            //else
            //{
            //    Debug.WriteLine( $"instant-speed: {instantSpeedInMbps:N2} Mbps, elapsedSeconds: {elapsedSeconds}, currentDownloadBytes: {totalDownloadBytes:#,#}" );
            //}
            return (instantSpeedInMbps);
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
}
