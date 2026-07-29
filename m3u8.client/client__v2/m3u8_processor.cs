using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using m3u8.helpers;
using m3u8.infrastructure;

using _DownloadPartInputParams_ = m3u8.client__v2.i_m3u8_client.DownloadPartInputParams;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;
#if THROTTLER__V1
using ThrottlerBySpeed_InDownloadProcessUser = m3u8.ThrottlerBySpeed_InDownloadProcessUser__v1;
#endif
#if THROTTLER__V2
using ThrottlerBySpeed_InDownloadProcessUser = m3u8.ThrottlerBySpeed_InDownloadProcessUser__v2;
#endif

namespace m3u8.client__v2
{
    /// <summary>
    /// 
    /// </summary>
    internal static class m3u8_processor
    {
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
            public int          TotalPartCount  { get; private set; }
            public int          PartOrderNumber { get; private set; }
            public m3u8_part_ts Part            { get; private set; }
            public Exception    Error           { get; private set; }
            public bool         Success         => (Error == null);

            internal RequestStepActionParams SetError( Exception error )
            {
                Error = error;
                return (this);
            }

            internal static RequestStepActionParams CreateSuccess( int totalPartCount, int partOrderNumber, in m3u8_part_ts part ) 
                => new RequestStepActionParams() { TotalPartCount = totalPartCount, PartOrderNumber = partOrderNumber, Part = part };
        }
        /// <summary>
        /// 
        /// </summary>
        public delegate void RequestStepActionDelegate( in RequestStepActionParams p );
        /// <summary>
        /// 
        /// </summary>
        public struct ResponseStepActionParams
        {
            internal ResponseStepActionParams( int totalPartCount ) => TotalPartCount = totalPartCount;

            public int          TotalPartCount           { get; }
            public int          SuccessReceivedPartCount { get; internal set; }
            public int          FailedReceivedPartCount  { get; internal set; }
            public int          BytesLength              { get; internal set; }
            public m3u8_part_ts Part                     { get; internal set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public delegate void ResponseStepActionDelegate( in ResponseStepActionParams p );
        //-----------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public interface ILogger
        {
            void Write( string msg );
            void Write_4_Parts( string msg );
        }

        /// <summary>
        /// 
        /// </summary>
        public struct DownloadPartsAndSaveInputParams
        {
            required public i_m3u8_client                  mc                               { [M(O.AggressiveInlining)] get; init; }
            required public m3u8_file_t                    m3u8File                         { [M(O.AggressiveInlining)] get; set; }
            required public string                         OutputFileName                   { [M(O.AggressiveInlining)] get; init; }
            required public CancellationToken              CancellationToken                { [M(O.AggressiveInlining)] get; init; }
            required public int                            MaxDegreeOfParallelism           { [M(O.AggressiveInlining)] get; init; }
            required public i_download_threads_semaphore   DownloadThreadsSemaphore         { [M(O.AggressiveInlining)] get; init; }
            required public i_download_threads_semaphore   DownloadThreadsSemaphore_4_Parts { [M(O.AggressiveInlining)] get; init; }
            required public WaitIfPausedHolder             WaitIfPausedHolder               { [M(O.AggressiveInlining)] get; init; }
            required public WaitIfPausedHolder             WaitIfPausedHolder_4_Parts       { [M(O.AggressiveInlining)] get; init; }
            required public i_throttler_by_speed__v2_t     ThrottlerBySpeed                 { [M(O.AggressiveInlining)] get; init; }
            required public IObjectPool< Stream >          StreamPool                       { [M(O.AggressiveInlining)] get; init; }
            required public IObjectPool< byte[] >          RespBufPool                      { [M(O.AggressiveInlining)] get; init; }
            required public IObjectPool< CancellationTokenSource > TimeoutCtsPool           { [M(O.AggressiveInlining)] get; init; }

            public RequestStepActionDelegate  RequestStepAction  { [M(O.AggressiveInlining)] get; init; }
            public ResponseStepActionDelegate ResponseStepAction { [M(O.AggressiveInlining)] get; init; }
            public i_m3u8_client.DownloadPartStepActionDelegate DownloadPartStepAction { [M(O.AggressiveInlining)] get; init; }

            public IReceivedAndWritedPartsProcessor ReceivedAndWritedPartsProcessor { get; init; }
            public Action< m3u8_file_t/*old*/, m3u8_file_t /*new*/, long /*outputFileStreamPosition*/ > RestoreAndContinueDownloadAction { get; init; }
            public ILogger Logger { get; init; }

            public override string ToString() => OutputFileName;
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

        public static async Task< DownloadPartsAndSaveResult > DownloadPartsAndSave( DownloadPartsAndSaveInputParams ip, IDictionary< string, string > requestHeaders = null )
        {            
            if ( ip.mc == null )                               throw (new m3u8_ArgumentException( nameof(ip.mc) ));
            if ( !ip.m3u8File.Parts.AnyEx() )                  throw (new m3u8_ArgumentException( nameof(ip.m3u8File) ));
            if ( ip.OutputFileName.IsNullOrWhiteSpace() )      throw (new m3u8_ArgumentException( nameof(ip.OutputFileName) ));
            if ( ip.DownloadThreadsSemaphore         == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore) ));
            if ( ip.DownloadThreadsSemaphore_4_Parts == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore_4_Parts) ));
            if ( ip.WaitIfPausedHolder               == null ) throw (new m3u8_ArgumentException( nameof(ip.WaitIfPausedHolder) ));
            if ( ip.StreamPool                       == null ) throw (new m3u8_ArgumentException( nameof(ip.StreamPool) ));
            if ( ip.TimeoutCtsPool                   == null ) throw (new m3u8_ArgumentException( nameof(ip.TimeoutCtsPool) ));
            //---if ( ip.ReceivedAndWritedPartsProcessor == null ) throw (new m3u8_ArgumentException( nameof(ip.ReceivedAndWritedPartsProcessor) ));
            var receivedAndWritedPartsProcessor = ip.ReceivedAndWritedPartsProcessor ?? ReceivedAndWritedPartsProcessor._Dummy_.Inst;
            //---------------------------------------------------------------------------------------------------------//

            //-1.1-//
            var directoryName = Path.GetDirectoryName( ip.OutputFileName );
            bool outputDirectoryExists;
            if ( !(outputDirectoryExists = Directory.Exists( directoryName )) ) Directory.CreateDirectory( directoryName );

            //-1.2-//
            using ( var fs = FileHelper.File_Open4Write_NoSetLength( ip.OutputFileName ) )
            {
                //-2.1-//
                var res = new DownloadPartsAndSaveResult( ip.OutputFileName );

                #region [.//-2.2-// receivedAndWritedPartsProcessor.]
                using var storer = receivedAndWritedPartsProcessor.CreateStorer( ip.m3u8File, ip.OutputFileName, outputDirectoryExists, fs.Length, out var exists );
                if ( exists.has )
                {                        
                    res.PartsSuccessCount = ip.m3u8File.Parts.Count - exists.new_m3u8File.Parts.Count;
                    res.TotalBytes        = (ulong) fs.Length;

                    ip.RestoreAndContinueDownloadAction?.Invoke( ip.m3u8File, exists.new_m3u8File, exists.outputFileStreamPosition );

                    ip.m3u8File = exists.new_m3u8File;

                    //can be for restored after full suc download.
                    if ( exists.new_m3u8File.Parts.Count == 0 )
                    {
                        Debug.Assert( exists.outputFileStreamPosition == fs.Length );
                        //return (res);
                    }
                    else
                    {                        
                        fs.Seek( exists.outputFileStreamPosition, SeekOrigin.Begin );
                    }                    
                }
                else
                {
                    fs.SetLength( 0 );
                }
                #endregion

                //-3-//
                var downloadParts = download_m3u8File_parts_parallel( ip, requestHeaders, already_successReceivedPartCount: res.PartsSuccessCount );
#if NETCOREAPP
                //-4-//
                await foreach ( var downloadPart in downloadParts )	
#else
                //-4-//
                foreach ( var downloadPart in downloadParts )
#endif
                {
                    using ( downloadPart )
                    {
                        if ( downloadPart.Error != null )
                        {
                            res.PartsErrorCount++;
                            continue;
                        }

                        await downloadPart.Stream.CopyToAsyncEx( fs, ip.CancellationToken ).CAX();
                        await fs.FlushAsync( ip.CancellationToken ).CAX();
                        await storer.Store( downloadPart.OrderNumber, fs.Position, ip.CancellationToken ).CAX();

                        res.PartsSuccessCount++;
                        res.TotalBytes += (uint) downloadPart.Stream.Length;
                    }
                }

                return (res);
            }
        }

#if NETCOREAPP
        private static async IAsyncEnumerable< m3u8_part_ts > download_m3u8File_parts_parallel( 
#else
        private static IEnumerable< m3u8_part_ts > download_m3u8File_parts_parallel( 
#endif
        DownloadPartsAndSaveInputParams ip, IDictionary< string, string > requestHeaders, int already_successReceivedPartCount )
        {
            var m3u8File = ip.m3u8File;
            if ( m3u8File.Parts.Count == 0 ) yield break; //can be for restored after full suc download.
            var runningPartsCount = 0;

            #region [.logger.]
            if ( ip.Logger != null )
            {
                const int millisecondsDelay = 500;
                var logger = ip.Logger;
                var ct     = ip.CancellationToken;
                //var fomatt_msg = (I_download_threads_semaphore dts) => $"MAX = {dts.MaxCount}, CurrentCount = {dts.CurrentCount}";
                var task_4_logger = Task.Run(() =>
                {
                    var dts = ip.DownloadThreadsSemaphore;
                    for ( ; !ct.IsCancellationRequested; )
                    {
                        Task.Delay( millisecondsDelay ).Wait( ct );

                        //---logger.Write( fomatt_msg( dts ) );
                        logger.Write( $"MAX = {dts.MaxCount}, CurrentCount = {dts.CurrentCount}" );
                    }
                }, ct );
                var task_4_logger_4_Parts = Task.Run(() =>
                {
                    var dts = ip.DownloadThreadsSemaphore_4_Parts;
                    for ( ; !ct.IsCancellationRequested; )
                    {
                        Task.Delay( millisecondsDelay ).Wait( ct );

                        //---logger.Write_4_Parts( fomatt_msg( dts ) );
                        logger.Write_4_Parts( $"MAX = {dts.MaxCount}, CurrentCount = {dts.CurrentCount}, RunningPartsCount = {runningPartsCount}" );
                    }
                }, ct );
            }
            #endregion
            //---------------------------------------------------------------//
            
            var baseAddress              = m3u8File.BaseAddress;
            var totalPatrs               = m3u8File.Parts.Count;
            var successReceivedPartCount = already_successReceivedPartCount;
            var failedReceivedPartCount  = 0;

            ip.ResponseStepAction?.Invoke( new ResponseStepActionParams( totalPatrs ) { SuccessReceivedPartCount = successReceivedPartCount } );

            var expectedPartNumber = m3u8File.Parts.FirstOrDefault().OrderNumber;
            var maxPartNumber      = m3u8File.Parts.LastOrDefault ().OrderNumber;
            var sourceQueue        = new Queue< m3u8_part_ts >( m3u8File.Parts );
            var downloadPartsSet   = new SortedSet< m3u8_part_ts >( m3u8_part_ts.Comparer.Inst );

            using var throttlerBySpeed_User = ThrottlerBySpeed_InDownloadProcessUser.Start( ip.ThrottlerBySpeed );
            using var innerCts              = new CancellationTokenSource();
            using var joinedCts             = CancellationTokenSource.CreateLinkedTokenSource( ip.CancellationToken, innerCts.Token );
            using var canExtractPartEvent   = new AutoResetEvent( false );

            var joinedCts_4_DownloadThreadsSemaphore = CancellationTokenSource.CreateLinkedTokenSource( joinedCts.Token, ip.WaitIfPausedHolder.Token );
            try
            {
                var t = new _DownloadPartInputParams_()
                {
                    ThrottlerBySpeed_User    = throttlerBySpeed_User,
                    RespBufPool              = ip.RespBufPool,
                    DownloadPartStepAction   = ip.DownloadPartStepAction,
                    DownloadThreadsSemaphore = ip.DownloadThreadsSemaphore_4_Parts,
                    WaitIfPausedHolder       = ip.WaitIfPausedHolder_4_Parts,
                    OutputFileName           = ip.OutputFileName,
                    TimeoutCtsPool           = ip.TimeoutCtsPool,
                };

                //-1-//
                var task_download = Task.Run( () =>
                {
                    #region [.check 'waitIfPausedEvent'.]
                    void check_and_hanging_on_waitIfPausedEvent()
                    {
                        if ( ip.WaitIfPausedHolder.IsNeedWait )
                        {
                            ip.WaitIfPausedHolder.Wait_WithCallbacks( joinedCts.Token );
                            throttlerBySpeed_User.Restart();
                        }
                    }
                    #endregion

                    try
                    {
                        for ( var n = 1; sourceQueue.Count != 0; n++ )
                        {
                            check_and_hanging_on_waitIfPausedEvent();

                        ONE_MORE_TIME_AFTER_TEMP_BREAK:
                            try
                            {
                                ip.DownloadThreadsSemaphore.Wait( joinedCts_4_DownloadThreadsSemaphore.Token );
                            }
                            catch ( Exception ex ) when (!joinedCts.IsCancellationRequested && ip.WaitIfPausedHolder.IsNeedWait)
                            {
                                Debug.WriteLine( ex );
                                Debug.Assert( ip.WaitIfPausedHolder.Token.IsCancellationRequested );

                                check_and_hanging_on_waitIfPausedEvent();

                                joinedCts_4_DownloadThreadsSemaphore.Dispose();
                                joinedCts_4_DownloadThreadsSemaphore = CancellationTokenSource.CreateLinkedTokenSource( joinedCts.Token, ip.WaitIfPausedHolder.Token );

                                goto ONE_MORE_TIME_AFTER_TEMP_BREAK;
                            }

                            var part = sourceQueue.Dequeue();

                            var rq = RequestStepActionParams.CreateSuccess( totalPatrs, n, part );
                            ip.RequestStepAction?.Invoke( rq );

                            part.SetStreamHolder( ip.StreamPool.GetHolder() );

                            Interlocked.Increment( ref runningPartsCount );

                            var task_download_part = 
                            ip.mc.DownloadPart( part, baseAddress, requestHeaders, t, joinedCts.Token )
                                 .ContinueWith( continuationTask =>
                                 {
                                     Interlocked.Decrement( ref runningPartsCount );

                                     var rsp = new ResponseStepActionParams( totalPatrs );

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
                                 , joinedCts.Token );
                        }
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                        throw;
                    }
                }
                , joinedCts.Token );

                //-2-//
                for ( var localReadyParts = new Queue< m3u8_part_ts >( Math.Min( 0x1000, ip.MaxDegreeOfParallelism ) );
                          expectedPartNumber <= maxPartNumber; )
                {
                    var idx = WaitHandle.WaitAny( [canExtractPartEvent /*0*/, joinedCts.Token.WaitHandle /*1*/] );
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
                if ( innerCts.IsCancellationRequested ) throw (new m3u8_Exception( "Canceled after part download error" ));
#if NETCOREAPP
                //-3.1-//
                await task_download.CAX();	
#else
                //-3.1-//
                task_download.Wait();
#endif
            }
            finally
            {
                joinedCts_4_DownloadThreadsSemaphore.Dispose();
            }

            //-4-//
            ip.CancellationToken.ThrowIfCancellationRequested();
        }
    }
}
