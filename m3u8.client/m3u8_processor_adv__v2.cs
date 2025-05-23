﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using m3u8.ext;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;
#if THROTTLER__V1
using ThrottlerBySpeed_InDownloadProcessUser = m3u8.ThrottlerBySpeed_InDownloadProcessUser__v1;
#endif
#if THROTTLER__V2
using ThrottlerBySpeed_InDownloadProcessUser = m3u8.ThrottlerBySpeed_InDownloadProcessUser__v2;
#endif

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal static class m3u8_processor_adv__v2
    {
        private static IEnumerable< m3u8_part_ts__v2 > download_m3u8File_parts_parallel( DownloadPartsAndSaveInputParams ip )
        {
            var m3u8File                 = ip.m3u8File;
            var baseAddress              = m3u8File.BaseAddress;
            var totalPatrs               = m3u8File.Parts.Count;
            var successReceivedPartCount = 0;
            var failedReceivedPartCount  = 0;

            ip.ResponseStepAction?.Invoke( new ResponseStepActionParams( totalPatrs ) );

            var expectedPartNumber = m3u8File.Parts.FirstOrDefault().OrderNumber;
            var maxPartNumber      = m3u8File.Parts.LastOrDefault ().OrderNumber;
            var sourceQueue        = new Queue< m3u8_part_ts__v2 >( m3u8File.Parts );
            var downloadPartsSet   = new SortedSet< m3u8_part_ts__v2 >( m3u8_part_ts__v2.Comparer.Inst );
            //var poolStreamCapacity = ip.PoolStreamCapacity.GetValueOrDefault( 1024 * 1024 * 5 );

            using ( var throttlerBySpeed_User = ThrottlerBySpeed_InDownloadProcessUser.Start( ip.ThrottlerBySpeed ) )
            using ( var innerCts              = new CancellationTokenSource() )
            using ( var joinedCts             = CancellationTokenSource.CreateLinkedTokenSource( ip.CancellationToken, innerCts.Token ) )
            using ( var canExtractPartEvent   = new AutoResetEvent( false ) )
            //using ( var pool                = new ObjectPoolDisposable< MemoryStream >( ip.MaxDegreeOfParallelism, () => new MemoryStream( poolStreamCapacity ) ) )
            {
                //-1-//
                var task_download = Task.Run( () =>
                {
                    try
                    {
                        for ( var n = 1; sourceQueue.Count != 0; n++ )
                        {
                            #region [.check 'waitIfPausedEvent'.]
                            if ( !ip.WaitIfPausedEvent.IsSet )
                            {
                                ip.WaitingIfPaused?.Invoke();
                                ip.WaitIfPausedEvent.Wait( joinedCts.Token );
                                throttlerBySpeed_User.Restart();
                            }
                            #endregion

                            #region [.throttler by speed.]
                            var instantSpeedInMbps = throttlerBySpeed_User.Throttle( joinedCts.Token );
                            #endregion

                            ip.DownloadThreadsSemaphore.Wait( /*ct*/ joinedCts.Token );
                            var part = sourceQueue.Dequeue();

                            var rq = RequestStepActionParams.CreateSuccess( totalPatrs, n, part );
                            ip.RequestStepAction?.Invoke( rq );

                            var holder = ip.StreamPool.GetHolder();
                            part.SetStreamHolder( holder );

//---#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            ip.mc.DownloadPart__v2( part, baseAddress, /*ct*/ joinedCts.Token )
                                 .ContinueWith( continuationTask =>
                                 {
                                    var rsp = new ResponseStepActionParams( totalPatrs, instantSpeedInMbps );

                                    if ( continuationTask.IsFaulted )
                                    {
                                        Interlocked.Increment( ref expectedPartNumber );

                                        part.SetError( continuationTask.Exception );

                                        rsp.SuccessReceivedPartCount = successReceivedPartCount;
                                        rsp.FailedReceivedPartCount  = Interlocked.Increment( ref failedReceivedPartCount );
                                        rsp.Part                     = part;

                                        ip.ResponseStepAction?.Invoke( rsp );

                                        innerCts.Cancel();
                                    }
                                    else if ( !continuationTask.IsCanceled )
                                    {
                                        var downloadPart = continuationTask.Result;
                                        if ( downloadPart.Error != null )
                                        {
                                            rsp.SuccessReceivedPartCount = successReceivedPartCount;
                                            rsp.FailedReceivedPartCount  = Interlocked.Increment( ref failedReceivedPartCount );
                                        }
                                        else
                                        {
                                            rsp.SuccessReceivedPartCount = Interlocked.Increment( ref successReceivedPartCount );
                                            rsp.FailedReceivedPartCount  = failedReceivedPartCount;
                                            rsp.BytesLength              = (int) downloadPart.Stream.Length;

                                            throttlerBySpeed_User.TakeIntoAccountDownloadedBytes( (int) downloadPart.Stream.Length );
                                        }
                                        rsp.Part = downloadPart;
                                        ip.ResponseStepAction?.Invoke( rsp );

                                        lock ( downloadPartsSet )
                                        {
                                            downloadPartsSet.Add( downloadPart );
                                            canExtractPartEvent.Set();
                                        }
                                    }
                                 }
                                 , /*ct*/ joinedCts.Token );
//---#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                        throw;
                    }
                }
                , /*ct*/ joinedCts.Token );

                //-2-//
                for ( var localReadyParts = new Queue< m3u8_part_ts__v2 >( Math.Min( 0x1000, ip.MaxDegreeOfParallelism ) );
                          expectedPartNumber <= maxPartNumber; )
                {
                    var idx = WaitHandle.WaitAny( [canExtractPartEvent /*0*/, /*ct*/ joinedCts.Token.WaitHandle /*1*/] );
                    if ( idx == 1 ) //[ct.IsCancellationRequested := 1]
                        break;
                    if ( idx != 0 ) //[canExtractPartEvent := 0]
                        continue;

                    lock ( downloadPartsSet )
                    {
                        for ( ; downloadPartsSet.Count != 0; )
                        {
                            var min_part = downloadPartsSet.Min;
                            if ( expectedPartNumber == min_part.OrderNumber )
                            {
                                downloadPartsSet.Remove( min_part );

                                Interlocked.Increment( ref expectedPartNumber );

                                ip.DownloadThreadsSemaphore.Release();

                                localReadyParts.Enqueue( min_part );
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    for ( ; localReadyParts.Count != 0; )
                    {
                        var part = localReadyParts.Dequeue();
                        yield return (part);
                    }
                }

                //-3.0-//
                if ( innerCts.IsCancellationRequested )
                {
                    throw (new m3u8_Exception( "Canceled after part download error" ));
                }

                //-3-//
                task_download.Wait();
            }

            //-4-//
            ip.CancellationToken.ThrowIfCancellationRequested();
        }
#if NETCOREAPP
        private static async IAsyncEnumerable< m3u8_part_ts__v2 > download_m3u8File_parts_parallel_Async( DownloadPartsAndSaveInputParams ip )
        {
            var m3u8File                 = ip.m3u8File;
            var baseAddress              = m3u8File.BaseAddress;
            var totalPatrs               = m3u8File.Parts.Count;
            var successReceivedPartCount = 0;
            var failedReceivedPartCount  = 0;

            ip.ResponseStepAction?.Invoke( new ResponseStepActionParams( totalPatrs ) );

            var expectedPartNumber = m3u8File.Parts.FirstOrDefault().OrderNumber;
            var maxPartNumber      = m3u8File.Parts.LastOrDefault ().OrderNumber;
            var sourceQueue        = new Queue< m3u8_part_ts__v2 >( m3u8File.Parts );
            var downloadPartsSet   = new SortedSet< m3u8_part_ts__v2 >( m3u8_part_ts__v2.Comparer.Inst );
            //var poolStreamCapacity = ip.PoolStreamCapacity.GetValueOrDefault( 1024 * 1024 * 5 );

            using ( var throttlerBySpeed_User = ThrottlerBySpeed_InDownloadProcessUser.Start( ip.ThrottlerBySpeed ) )
            using ( var innerCts              = new CancellationTokenSource() )
            using ( var joinedCts             = CancellationTokenSource.CreateLinkedTokenSource( ip.CancellationToken, innerCts.Token ) )
            using ( var canExtractPartEvent   = new AutoResetEvent( false ) )
            //using ( var pool                = new ObjectPoolDisposable< MemoryStream >( ip.MaxDegreeOfParallelism, () => new MemoryStream( poolStreamCapacity ) ) )
            {
                //-1-//
                var task_download = Task.Run( () =>
                {
                    try
                    {
                        for ( var n = 1; sourceQueue.Count != 0; n++ )
                        {
                            #region [.check 'waitIfPausedEvent'.]
                            if ( !ip.WaitIfPausedEvent.IsSet )
                            {
                                ip.WaitingIfPaused?.Invoke();
                                ip.WaitIfPausedEvent.Wait( joinedCts.Token );
                                throttlerBySpeed_User.Restart();
                            }
                            #endregion

                            #region [.throttler by speed.]                            
                            var instantSpeedInMbps = throttlerBySpeed_User.Throttle( joinedCts.Token );
                            #endregion

                            ip.DownloadThreadsSemaphore.Wait( /*ct*/ joinedCts.Token );
                            var part = sourceQueue.Dequeue();

                            var rq = RequestStepActionParams.CreateSuccess( totalPatrs, n, part );
                            ip.RequestStepAction?.Invoke( rq );

                            var holder = ip.StreamPool.GetHolder();                        
                            part.SetStreamHolder( holder );

//---#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            ip.mc.DownloadPart__v2( part, baseAddress, /*ct*/ joinedCts.Token )
                                 .ContinueWith( continuationTask =>
                                 {
                                    var rsp = new ResponseStepActionParams( totalPatrs, instantSpeedInMbps );

                                    if ( continuationTask.IsFaulted )
                                    {
                                        Interlocked.Increment( ref expectedPartNumber );

                                        part.SetError( continuationTask.Exception );

                                        rsp.SuccessReceivedPartCount = successReceivedPartCount;
                                        rsp.FailedReceivedPartCount  = Interlocked.Increment( ref failedReceivedPartCount );
                                        rsp.Part                     = part;

                                        ip.ResponseStepAction?.Invoke( rsp );

                                        innerCts.Cancel();
                                    }
                                    else if ( !continuationTask.IsCanceled )
                                    {
                                        var downloadPart = continuationTask.Result;
                                        if ( downloadPart.Error != null )
                                        {
                                            rsp.SuccessReceivedPartCount = successReceivedPartCount;
                                            rsp.FailedReceivedPartCount  = Interlocked.Increment( ref failedReceivedPartCount );
                                        }
                                        else
                                        {
                                            rsp.SuccessReceivedPartCount = Interlocked.Increment( ref successReceivedPartCount );
                                            rsp.FailedReceivedPartCount  = failedReceivedPartCount;
                                            rsp.BytesLength              = (int) downloadPart.Stream.Length;
                                            
                                            throttlerBySpeed_User.TakeIntoAccountDownloadedBytes( (int) downloadPart.Stream.Length );
                                        }
                                        rsp.Part = downloadPart;
                                        ip.ResponseStepAction?.Invoke( rsp );

                                        lock ( downloadPartsSet )
                                        {
                                            downloadPartsSet.Add( downloadPart );
                                            canExtractPartEvent.Set();
                                        }
                                    }
                                 }
                                 , /*ct*/ joinedCts.Token );
//---#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                        throw;
                    }
                }
                , /*ct*/ joinedCts.Token );

                //-2-//
                for ( var localReadyParts = new Queue< m3u8_part_ts__v2 >( Math.Min( 0x1000, ip.MaxDegreeOfParallelism ) );
                          expectedPartNumber <= maxPartNumber; )
                {
                    var idx = WaitHandle.WaitAny( [ canExtractPartEvent /*0*/, /*ct*/ joinedCts.Token.WaitHandle /*1*/, ] );
                    if ( idx == 1 ) //[ct.IsCancellationRequested := 1]
                        break;
                    if ( idx != 0 ) //[canExtractPartEvent := 0]
                        continue;

                    lock ( downloadPartsSet )
                    {
                        for ( ; downloadPartsSet.Count != 0; )
                        {
                            var min_part = downloadPartsSet.Min;
                            if ( expectedPartNumber == min_part.OrderNumber )
                            {
                                downloadPartsSet.Remove( min_part );

                                Interlocked.Increment( ref expectedPartNumber );

                                ip.DownloadThreadsSemaphore.Release();

                                localReadyParts.Enqueue( min_part );
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    for ( ; localReadyParts.Count != 0; )
                    {
                        var part = localReadyParts.Dequeue();
                        yield return (part);
                    }
                }

                //-3.0-//
                if ( innerCts.IsCancellationRequested )
                {
                    throw (new m3u8_Exception( "Canceled after part download error" ));
                }

                //-3-//
                await task_download.CAX();
            }

            //-4-//
            ip.CancellationToken.ThrowIfCancellationRequested();
        }
#endif
        [M(O.AggressiveInlining)] private static Task CopyToAsyncEx( this Stream source, Stream destination, CancellationToken ct )
        {
            source.Seek( 0, SeekOrigin.Begin );
#if NETCOREAPP
            return (source.CopyToAsync( destination, ct ));
#else
            return (source.CopyToAsync( destination, bufferSize: 80 * 1_024/*81920*/, ct ));
#endif
        }
        //-----------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public struct RequestStepActionParams
        {
            public int              TotalPartCount  { get; private set; }
            public int              PartOrderNumber { get; private set; }
            public m3u8_part_ts__v2 Part            { get; private set; }
            public Exception        Error           { get; private set; }
            public bool             Success         => (Error == null);

            internal RequestStepActionParams SetError( Exception error )
            {
                Error = error;
                return (this);
            }

            internal static RequestStepActionParams CreateSuccess( int totalPartCount, int partOrderNumber, in m3u8_part_ts__v2 part ) 
                => new RequestStepActionParams() { TotalPartCount = totalPartCount, PartOrderNumber = partOrderNumber, Part = part };
        }
        /// <summary>
        /// 
        /// </summary>
        public delegate void RequestStepActionDelegate( RequestStepActionParams p );
        /// <summary>
        /// 
        /// </summary>
        public struct ResponseStepActionParams
        {
            internal ResponseStepActionParams( int totalPartCount, double? instantSpeedInMbps = null )
            {
                TotalPartCount     = totalPartCount;
                InstantSpeedInMbps = instantSpeedInMbps;
            }

            public int     TotalPartCount           { get; }
            public double? InstantSpeedInMbps       { get; }
            public int     SuccessReceivedPartCount { get; internal set; }
            public int     FailedReceivedPartCount  { get; internal set; }
            public int     BytesLength              { get; internal set; }
            public m3u8_part_ts__v2 Part            { get; internal set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public delegate void ResponseStepActionDelegate( ResponseStepActionParams p );
        //-----------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public struct DownloadPartsAndSaveInputParams
        {
            public m3u8_client                  mc                       { [M(O.AggressiveInlining)] get; set; }
            public m3u8_file_t__v2              m3u8File                 { [M(O.AggressiveInlining)] get; set; }
            public string                       OutputFileName           { [M(O.AggressiveInlining)] get; set; }
            public CancellationToken            CancellationToken        { [M(O.AggressiveInlining)] get; set; }
            public RequestStepActionDelegate    RequestStepAction        { [M(O.AggressiveInlining)] get; set; }
            public ResponseStepActionDelegate   ResponseStepAction       { [M(O.AggressiveInlining)] get; set; }
            public int                          MaxDegreeOfParallelism   { [M(O.AggressiveInlining)] get; set; }
            public I_download_threads_semaphore DownloadThreadsSemaphore { [M(O.AggressiveInlining)] get; set; }
            public ManualResetEventSlim         WaitIfPausedEvent        { [M(O.AggressiveInlining)] get; set; }
            public Action                       WaitingIfPaused          { [M(O.AggressiveInlining)] get; set; }
#if THROTTLER__V1
            public I_throttler_by_speed__v1_t   ThrottlerBySpeed         { [M(O.AggressiveInlining)] get; set; }
#endif
#if THROTTLER__V2
            public I_throttler_by_speed__v2_t   ThrottlerBySpeed         { [M(O.AggressiveInlining)] get; set; }
#endif
            //public int?                         PoolStreamCapacity       { get; set; }
            public ObjectPool< Stream >         StreamPool               { [M(O.AggressiveInlining)] get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public struct DownloadPartsAndSaveResult
        {
            internal DownloadPartsAndSaveResult( string outputFileName ) : this() => OutputFileName = outputFileName;

            public string OutputFileName   { get; private set; }

            public int   PartsSuccessCount { get; internal set; }
            public int   PartsErrorCount   { get; internal set; }
            public ulong TotalBytes        { get; internal set; }

            public int  TotalParts => (PartsSuccessCount + PartsErrorCount);
            public void ResetOutputFileName( string outputFileName ) => OutputFileName = outputFileName;
            public bool IsEmpty() => ((OutputFileName == null) && (PartsSuccessCount == 0) && (PartsErrorCount == 0) && (TotalBytes == 0UL));
        }

        public static async Task< DownloadPartsAndSaveResult > DownloadPartsAndSave( DownloadPartsAndSaveInputParams ip )
        {            
            if ( ip.mc == null )                          throw (new m3u8_ArgumentException( nameof(ip.mc) ));
            if ( !ip.m3u8File.Parts.AnyEx() )             throw (new m3u8_ArgumentException( nameof(ip.m3u8File) ));
            if ( ip.OutputFileName.IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(ip.OutputFileName) ));
            if ( ip.DownloadThreadsSemaphore    == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore) ));
            if ( ip.WaitIfPausedEvent           == null ) throw (new m3u8_ArgumentException( nameof(ip.WaitIfPausedEvent) ));
            //---Allowed been null---// if ( ip.ThrottlerBySpeed == null ) throw (new m3u8_ArgumentException( nameof(ip.ThrottlerBySpeed) ));
            if ( ip.StreamPool                  == null ) throw (new m3u8_ArgumentException( nameof(ip.StreamPool) ));
            //---------------------------------------------------------------------------------------------------------//            

            //-1-//
            var res = new DownloadPartsAndSaveResult( ip.OutputFileName );

            //-2-//
            var downloadParts = download_m3u8File_parts_parallel( ip );

            //-3.1-//
            var directoryName = Path.GetDirectoryName( ip.OutputFileName );
            if ( !Directory.Exists( directoryName ) ) Directory.CreateDirectory( directoryName );
            
            //-3.2-//
            using ( var fs = Extensions.File_Open4Write( ip.OutputFileName ) )
            {
                foreach ( var downloadPart in downloadParts )
                {
                    using ( downloadPart )
                    {
                        if ( downloadPart.Error != null ) 
                        {
                            res.PartsErrorCount++;
                            continue;
                        }

                        await downloadPart.Stream.CopyToAsyncEx( fs, ip.CancellationToken ).CAX();

                        res.PartsSuccessCount++;
                        res.TotalBytes += (uint) downloadPart.Stream.Length;
                    }
                }
            }

            return (res);
        }
#if NETCOREAPP
        public static async Task< DownloadPartsAndSaveResult > DownloadPartsAndSave_Async( DownloadPartsAndSaveInputParams ip )
        {            
            if ( ip.mc == null )                           throw (new m3u8_ArgumentException( nameof(ip.mc) ));
            if ( !ip.m3u8File.Parts.AnyEx() )              throw (new m3u8_ArgumentException( nameof(ip.m3u8File) ));
            if ( ip.OutputFileName.IsNullOrWhiteSpace() )  throw (new m3u8_ArgumentException( nameof(ip.OutputFileName) ));
            if ( ip.DownloadThreadsSemaphore    == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore) ));
            if ( ip.WaitIfPausedEvent           == null ) throw (new m3u8_ArgumentException( nameof(ip.WaitIfPausedEvent) ));
            if ( ip.StreamPool                  == null ) throw (new m3u8_ArgumentException( nameof(ip.StreamPool) ));
            //---------------------------------------------------------------------------------------------------------//

            //-1-//
            var res = new DownloadPartsAndSaveResult( ip.OutputFileName );

            //-2-//
            var downloadParts = download_m3u8File_parts_parallel_Async( ip );

            //-3.1-//
            var directoryName = Path.GetDirectoryName( ip.OutputFileName );
            if ( !Directory.Exists( directoryName ) )
            {
                Directory.CreateDirectory( directoryName );
            }
            //-3.2-//
            using ( var fs = Extensions.File_Open4Write( ip.OutputFileName ) )
            {
                await foreach ( var downloadPart in downloadParts )
                {
                    using ( downloadPart )
                    {
                        if ( downloadPart.Error != null )
                        {
                            res.PartsErrorCount++;
                            continue;
                        }

                        await downloadPart.Stream.CopyToAsyncEx( fs, ip.CancellationToken ).CAX();

                        res.PartsSuccessCount++;
                        res.TotalBytes += (uint) downloadPart.Stream.Length;
                    }
                }
            }

            return (res);
        }
#endif
        //-----------------------------------------------------------------------------//
    }
}
