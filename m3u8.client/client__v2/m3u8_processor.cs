using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
#if NETCOREAPP
using System.Runtime.CompilerServices; 
#endif
using System.Threading;
using System.Threading.Tasks;

using m3u8.helpers;
using m3u8.infrastructure;

using _DownloadPartInputParams_            = m3u8.client__v2.i_m3u8_client.DownloadPartInputParams;
using _DownloadPartStepActionDelegate_     = m3u8.client__v2.i_m3u8_client.DownloadPartStepActionDelegate;
using _RestoreAndContinueDownloadDelegate_ = m3u8.client__v2.i_m3u8_client.RestoreAndContinueDownloadDelegate;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

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
        [M(O.AggressiveInlining)] private static void FillFrom< T >( this List< T > lst, IEnumerable< T > seq )
        {
            lst.Clear();
            lst.AddRange( seq );
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
            required public IDictionary< string, string >  requestHeaders                   { [M(O.AggressiveInlining)] get; init; }
            required public string                         OutputFileName                   { [M(O.AggressiveInlining)] get; init; }
            required public int                            MaxDegreeOfParallelism           { [M(O.AggressiveInlining)] get; init; }
            required public i_download_threads_semaphore   DownloadThreadsSemaphore         { [M(O.AggressiveInlining)] get; init; }
            required public i_download_threads_semaphore   DownloadThreadsSemaphore_4_Parts { [M(O.AggressiveInlining)] get; init; }
            required public WaitIfPausedHolder             WaitIfPausedHolder               { [M(O.AggressiveInlining)] get; init; }
            required public WaitIfPausedHolder             WaitIfPausedHolder_4_Parts       { [M(O.AggressiveInlining)] get; init; }
            required public i_throttler_by_speed_t         ThrottlerBySpeed                 { [M(O.AggressiveInlining)] get; init; }
            required public IObjectPool< Stream >          StreamPool                       { [M(O.AggressiveInlining)] get; init; }
            required public IObjectPool< byte[] >          RespBufPool                      { [M(O.AggressiveInlining)] get; init; }
            required public CtsTimerPool                   TimeoutCtsPool                   { [M(O.AggressiveInlining)] get; init; }

            public RequestStepActionDelegate        RequestStepAction      { [M(O.AggressiveInlining)] get; init; }
            public ResponseStepActionDelegate       ResponseStepAction     { [M(O.AggressiveInlining)] get; init; }
            public _DownloadPartStepActionDelegate_ DownloadPartStepAction { [M(O.AggressiveInlining)] get; init; }

            public IReceivedAndWritedPartsProcessor     ReceivedAndWritedPartsProcessor  { get; init; }
            public _RestoreAndContinueDownloadDelegate_ RestoreAndContinueDownloadAction { get; init; }
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
        //-----------------------------------------------------------------------------//

        public static async Task< DownloadPartsAndSaveResult > DownloadPartsAndSave_PREV( DownloadPartsAndSaveInputParams ip, CancellationToken ct = default )
        {            
            if ( ip.mc == null )                               throw (new m3u8_ArgumentException( nameof(ip.mc) ));
            if ( !ip.m3u8File.Parts.AnyEx() )                  throw (new m3u8_ArgumentException( nameof(ip.m3u8File) ));
            if ( ip.OutputFileName.IsNullOrWhiteSpace() )      throw (new m3u8_ArgumentException( nameof(ip.OutputFileName) ));
            if ( ip.DownloadThreadsSemaphore         == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore) ));
            if ( ip.DownloadThreadsSemaphore_4_Parts == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore_4_Parts) ));
            if ( ip.WaitIfPausedHolder               == null ) throw (new m3u8_ArgumentException( nameof(ip.WaitIfPausedHolder) ));
            if ( ip.StreamPool                       == null ) throw (new m3u8_ArgumentException( nameof(ip.StreamPool) ));
            if ( ip.TimeoutCtsPool                   == null ) throw (new m3u8_ArgumentException( nameof(ip.TimeoutCtsPool) ));
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
                var downloadParts = DownloadParts_Routine_PREV( ip, already_successReceivedPartCount: res.PartsSuccessCount, ct );
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

                        await downloadPart.Stream.CopyToAsyncEx( fs, ct ).CAX();
                        await fs.FlushAsync( ct ).CAX();
                        await storer.Store( downloadPart.OrderNumber, fs.Position, ct ).CAX();

                        res.PartsSuccessCount++;
                        res.TotalBytes += (uint) downloadPart.Stream.Length;
                    }
                }

                return (res);
            }
        }

#if NETCOREAPP
        private static async IAsyncEnumerable< m3u8_part_ts > DownloadParts_Routine_PREV( DownloadPartsAndSaveInputParams ip, int already_successReceivedPartCount, [EnumeratorCancellation] CancellationToken ct )
#else
        private static IEnumerable< m3u8_part_ts > DownloadParts_Routine_PREV( DownloadPartsAndSaveInputParams ip, int already_successReceivedPartCount, CancellationToken ct )
#endif
        {
            var m3u8File = ip.m3u8File;
            if ( m3u8File.Parts.Count == 0 ) yield break; //can be for restored after full suc download.
            var runningPartsCount = 0;

            #region [.logger.]
            if ( ip.Logger != null )
            {
                const int millisecondsDelay = 500;
                var logger = ip.Logger;
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

            var expectedPartNumber = m3u8File.Parts.FirstOrDefault()?.OrderNumber ?? 0;
            var maxPartNumber      = m3u8File.Parts.LastOrDefault ()?.OrderNumber ?? 0;
            var sourceQueue        = new Queue< m3u8_part_ts >( m3u8File.Parts );
            var downloadPartsSet   = new SortedSet< m3u8_part_ts >( m3u8_part_ts.Comparer.Inst );

            using var throttlerBySpeed_User = ThrottlerBySpeed_InDownloadProcessUser.Start( ip.ThrottlerBySpeed );
            using var innerCts              = new CancellationTokenSource();
            using var joinedCts             = CancellationTokenSource.CreateLinkedTokenSource( ct, innerCts.Token );
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
                            ip.mc.DownloadPart( part, baseAddress, ip.requestHeaders, t, joinedCts.Token )
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
            ct.ThrowIfCancellationRequested();
        }
        //-----------------------------------------------------------------------------//

        public static async Task< DownloadPartsAndSaveResult > DownloadPartsAndSave( DownloadPartsAndSaveInputParams ip, CancellationToken ct = default )
        {            
            if ( ip.mc == null )                               throw (new m3u8_ArgumentException( nameof(ip.mc) ));
            if ( !ip.m3u8File.Parts.AnyEx() )                  throw (new m3u8_ArgumentException( nameof(ip.m3u8File) ));
            if ( ip.OutputFileName.IsNullOrWhiteSpace() )      throw (new m3u8_ArgumentException( nameof(ip.OutputFileName) ));
            if ( ip.DownloadThreadsSemaphore         == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore) ));
            if ( ip.DownloadThreadsSemaphore_4_Parts == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore_4_Parts) ));
            if ( ip.WaitIfPausedHolder               == null ) throw (new m3u8_ArgumentException( nameof(ip.WaitIfPausedHolder) ));
            if ( ip.StreamPool                       == null ) throw (new m3u8_ArgumentException( nameof(ip.StreamPool) ));
            if ( ip.TimeoutCtsPool                   == null ) throw (new m3u8_ArgumentException( nameof(ip.TimeoutCtsPool) ));
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
                long lastOrderedGlobalPositionInFile = 0L;
                var expectedPartNumber  = ip.m3u8File.Parts.FirstOrDefault()?.OrderNumber ?? 0;                
                var downloadPartsAccum  = new SortedSet< m3u8_part_ts >( m3u8_part_ts.Comparer.Inst );
                var downloadPartsBuf    = new List< m3u8_part_ts >( ip.MaxDegreeOfParallelism );
                var downloadPartAlreadySavedNumbers = new Dictionary< int/*order-number*/, long/*end position in file*/ >( ip.MaxDegreeOfParallelism );
                //var downloadPartsReceiveOrderSet = new HashSet< int >( ip.m3u8File.Parts.Count );
                //var downloadPartsReceiveOrder = new List< m3u8_part_ts >( ip.m3u8File.Parts.Count );

                var downloadParts = DownloadParts_Routine( ip, already_successReceivedPartCount: res.PartsSuccessCount, ct );

                //-4.1-//
                async Task< bool > process_downloadPart( m3u8_part_ts downloadPart )
                {
                    //if ( downloadPartsReceiveOrderSet.Add( downloadPart.OrderNumber ) ) downloadPartsReceiveOrder.Add( downloadPart );

                    if ( downloadPart.Error != null )
                    {
                        res.PartsErrorCount++;
                        return (true);
                    }

                    if ( downloadPart.OrderNumber == expectedPartNumber )
                    {
                        fs.Seek( lastOrderedGlobalPositionInFile, SeekOrigin.Begin );
                        await downloadPart.Stream.CopyToAsyncEx( fs, ct ).CAX();
                        await fs.FlushAsync( ct ).CAX();

                        var totalContentLength = (uint) downloadPart.Stream.Length;
                        Debug.Assert( fs.Position == lastOrderedGlobalPositionInFile + totalContentLength );
                        lastOrderedGlobalPositionInFile += totalContentLength; //lastOrderedGlobalPositionInFile = fs.Position;
                        var stored_OrderNumber = downloadPart.OrderNumber;

                        if ( downloadPartAlreadySavedNumbers.Count != 0 )
                        {
                            for (; ; )
                            {
                                if ( downloadPartAlreadySavedNumbers.TryGetValue( expectedPartNumber + 1, out var endPositionInFile ) )
                                {
                                    lastOrderedGlobalPositionInFile = endPositionInFile;
                                    expectedPartNumber++;
                                    stored_OrderNumber = expectedPartNumber;
                                    downloadPartAlreadySavedNumbers.Remove( expectedPartNumber );
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }                        
                        expectedPartNumber++;

                        await storer.Store( stored_OrderNumber, lastOrderedGlobalPositionInFile, ct ).CAX();

                        res.PartsSuccessCount++;
                        res.TotalBytes += totalContentLength;

                        return (true);
                    }
                    else if ( downloadPart.TryCalcTotalContentLengthBefore( expectedPartNumber, out var totalContentLengthBefore ) )
                    {
                        var positionInFile = lastOrderedGlobalPositionInFile + totalContentLengthBefore;
                        fs.Seek( positionInFile, SeekOrigin.Begin );

                        await downloadPart.Stream.CopyToAsyncEx( fs, ct ).CAX();
                        await fs.FlushAsync( ct ).CAX();

                        var totalContentLength = (uint) downloadPart.Stream.Length;
                        Debug.Assert( fs.Position == positionInFile + totalContentLength );
                        downloadPartAlreadySavedNumbers.Add( downloadPart.OrderNumber, positionInFile + totalContentLength );

                        res.PartsSuccessCount++;
                        res.TotalBytes += totalContentLength;

                        return (true);
                    }

                    return (false);
                }
#if NETCOREAPP
                //-4.2-//
                await foreach ( var downloadPart in downloadParts )
#else
                //-4.2-//
                foreach ( var downloadPart in downloadParts )
#endif
                {
                    bool suc_processed;
                    if ( downloadPartsAccum.Count != 0 )
                    {
                        downloadPartsBuf.FillFrom( downloadPartsAccum );
                        downloadPartsAccum.Clear();
                        foreach ( var prevDownloadPart in downloadPartsBuf )
                        {
                            suc_processed = await process_downloadPart( prevDownloadPart ).CAX();
                            if ( suc_processed )
                            {
                                prevDownloadPart.Dispose();
                            }
                            else
                            {
                                downloadPartsAccum.Add( prevDownloadPart );
                            }
                        }
                    }

                    suc_processed = await process_downloadPart( downloadPart ).CAX();
                    if ( suc_processed )
                    {
                        downloadPart.Dispose();
                    }
                    else
                    {
                        downloadPartsAccum.Add( downloadPart );
                    }

                    #region comm. prev.
                    /*
                    using ( downloadPart )
                    {
                        if ( downloadPart.Error != null )
                        {
                            res.PartsErrorCount++;
                            continue;
                        }

                        await downloadPart.Stream.CopyToAsyncEx( fs, ct ).CAX();
                        await fs.FlushAsync( ct ).CAX();
                        await storer.Store( downloadPart.OrderNumber, fs.Position, ct ).CAX();

                        res.PartsSuccessCount++;
                        res.TotalBytes += (uint) downloadPart.Stream.Length;
                    }
                    //*/
                    #endregion
                }

                Debug.Assert( downloadPartsAccum.Count == 0 );
                Debug.Assert( downloadPartAlreadySavedNumbers.Count == 0 );

                if ( 0 < ip.m3u8File.Parts.Count )
                {
                    //Debug.Assert( fs.Position == lastOrderedGlobalPositionInFile );
                    await storer.Store( /*storer.M3u8FilePartCount - 1*/expectedPartNumber - 1, fs.Length, ct ).CAX();
                }

                #region comm
                //if ( 0 < downloadPartsReceiveOrder.Count )
                //{
                //    using var ofs = FileHelper.File_Open4Write( @"E:\downloadPartsReceiveOrder.txt" );
                //    using var sw = new StreamWriter( ofs );
                //    foreach ( var p in downloadPartsReceiveOrder )
                //    {
                //        sw.WriteLine( $"ordNum={p.OrderNumber}, fn={p.RelativeUrlName}, size={p.TotalContentLength}" );
                //    }
                //}
                #endregion

                return (res);
            }
        }

#if NETCOREAPP
        private static async IAsyncEnumerable< m3u8_part_ts > DownloadParts_Routine( DownloadPartsAndSaveInputParams ip, int already_successReceivedPartCount, [EnumeratorCancellation] CancellationToken ct )
#else
        private static IEnumerable< m3u8_part_ts > DownloadParts_Routine( DownloadPartsAndSaveInputParams ip, int already_successReceivedPartCount, CancellationToken ct )
#endif
        {
            var m3u8File = ip.m3u8File;
            if ( m3u8File.Parts.Count == 0 ) yield break; //can be for restored after full suc download.
            var runningPartsCount = 0;

            #region [.logger.]
            if ( ip.Logger != null )
            {
                const int millisecondsDelay = 500;
                var logger = ip.Logger;
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

            var expectedPartNumber = m3u8File.Parts.FirstOrDefault()?.OrderNumber ?? 0;
            var maxPartNumber      = m3u8File.Parts.LastOrDefault ()?.OrderNumber ?? 0;
            var sourceQueue        = new Queue< m3u8_part_ts >( m3u8File.Parts );
            var downloadPartsSet   = new SortedSet< m3u8_part_ts >( m3u8_part_ts.Comparer.Inst );

            using var throttlerBySpeed_User = ThrottlerBySpeed_InDownloadProcessUser.Start( ip.ThrottlerBySpeed );
            using var innerCts              = new CancellationTokenSource();
            using var joinedCts             = CancellationTokenSource.CreateLinkedTokenSource( ct, innerCts.Token );
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
                var task_download = Task.Run(() =>
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

                        ip.RequestStepAction?.Invoke( RequestStepActionParams.CreateSuccess( totalPatrs, n, part ) );

                        part.SetStreamHolder( ip.StreamPool.GetHolder() );

                        Interlocked.Increment( ref runningPartsCount );

                        var task_download_part = 
                        ip.mc.DownloadPart( part, baseAddress, ip.requestHeaders, t, joinedCts.Token )
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
                                        part.SetError( downloadPart.Error );

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
                , joinedCts.Token );

                //-2-//
                var waitHandles = new[] { canExtractPartEvent /*0*/, joinedCts.Token.WaitHandle /*1*/};
                for ( var localReadyParts = new Queue< m3u8_part_ts >( Math.Min( 0x1000, ip.MaxDegreeOfParallelism ) );
                          expectedPartNumber <= maxPartNumber; )
                {
                    var idx = WaitHandle.WaitAny( waitHandles );
                    if ( idx == 1 ) //[ct.IsCancellationRequested := 1]
                        break;
                    if ( idx != 0 ) //[canExtractPartEvent := 0]
                        continue;

                    lock ( downloadPartsSet )
                    {
                        for ( ; downloadPartsSet.Count != 0; )
                        {
                            var min_part = downloadPartsSet.Min;
                            downloadPartsSet.Remove( min_part );
                            localReadyParts.Enqueue( min_part );

                            Interlocked.Increment( ref expectedPartNumber );
                            ip.DownloadThreadsSemaphore.Release();
                        }
                    }

                    #region comm. prev.
                    /*
                    lock ( downloadPartsSet )
                    {
                        for ( ; downloadPartsSet.Count != 0; )
                        {
                            var min_part = downloadPartsSet.Min;
                            if ( expectedPartNumber == min_part.OrderNumber )
                            {
                                downloadPartsSet.Remove( min_part );
                                localReadyParts.Enqueue( min_part );

                                Interlocked.Increment( ref expectedPartNumber );
                                ip.DownloadThreadsSemaphore.Release();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    //*/
                    #endregion

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
            ct.ThrowIfCancellationRequested();
        }
        //-----------------------------------------------------------------------------//


        public static async Task< DownloadPartsAndSaveResult > GetTotalContentLengthParts( DownloadPartsAndSaveInputParams ip, CancellationToken ct = default )
        {            
            if ( ip.mc == null )                       throw (new m3u8_ArgumentException( nameof(ip.mc) ));
            if ( !ip.m3u8File.Parts.AnyEx() )          throw (new m3u8_ArgumentException( nameof(ip.m3u8File) ));
            if ( ip.DownloadThreadsSemaphore == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore) ));
            if ( ip.TimeoutCtsPool           == null ) throw (new m3u8_ArgumentException( nameof(ip.TimeoutCtsPool) ));
            //---------------------------------------------------------------------------------------------------------//

            //-2.1-//
            var res = new DownloadPartsAndSaveResult();

            //-3-//
            var downloadParts = GetTotalContentLengthParts_Routine( ip, ct );
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
                    if ( (downloadPart.Error != null) || !downloadPart.TotalContentLength.HasValue )
                    {
                        res.PartsErrorCount++;
                        continue;
                    }

                    res.PartsSuccessCount++;
                    res.TotalBytes += (uint) downloadPart.TotalContentLength.Value;
                }

                await Task.Delay( 1 ).CAX();
            }

            return (res);
        }

#if NETCOREAPP
        private static async IAsyncEnumerable< m3u8_part_ts > GetTotalContentLengthParts_Routine( DownloadPartsAndSaveInputParams ip, [EnumeratorCancellation] CancellationToken ct )
#else
        private static IEnumerable< m3u8_part_ts > GetTotalContentLengthParts_Routine( DownloadPartsAndSaveInputParams ip, CancellationToken ct )
#endif
        {
            var m3u8File = ip.m3u8File;
            if ( m3u8File.Parts.Count == 0 ) yield break; //can be for restored after full suc download.
            var runningPartsCount = 0;

            #region [.logger.]
            if ( ip.Logger != null )
            {
                const int millisecondsDelay = 500;
                var logger = ip.Logger;
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
            var successReceivedPartCount = 0;
            var failedReceivedPartCount  = 0;

            ip.ResponseStepAction?.Invoke( new ResponseStepActionParams( totalPatrs ) { SuccessReceivedPartCount = successReceivedPartCount } );

            var expectedPartNumber = m3u8File.Parts.FirstOrDefault()?.OrderNumber ?? 0;
            var maxPartNumber      = m3u8File.Parts.LastOrDefault ()?.OrderNumber ?? 0;
            var sourceQueue        = new Queue< m3u8_part_ts >( m3u8File.Parts );
            var downloadPartsSet   = new SortedSet< m3u8_part_ts >( m3u8_part_ts.Comparer.Inst );

            using var canExtractPartEvent = new AutoResetEvent( false );

            var t = new _DownloadPartInputParams_()
            {
                TimeoutCtsPool           = ip.TimeoutCtsPool,
                DownloadPartStepAction   = ip.DownloadPartStepAction,
                ThrottlerBySpeed_User    = null, //throttlerBySpeed_User,
                RespBufPool              = null, //ip.RespBufPool,
                DownloadThreadsSemaphore = null, //ip.DownloadThreadsSemaphore_4_Parts,
                WaitIfPausedHolder       = null, //ip.WaitIfPausedHolder_4_Parts,
                OutputFileName           = null, //ip.OutputFileName,
            };

            //-1-//
            var task_download = Task.Run( () =>
            {
                for ( var n = 1; sourceQueue.Count != 0; n++ )
                {
                    ip.DownloadThreadsSemaphore.Wait( ct );

                    var part = sourceQueue.Dequeue();

                    ip.RequestStepAction?.Invoke( RequestStepActionParams.CreateSuccess( totalPatrs, n, part ) );

                    Interlocked.Increment( ref runningPartsCount );

                    var task_download_part = 
                    ip.mc.GetTotalContentLengthPart( part, baseAddress, ip.requestHeaders, t, ct )
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
                            }
                            else if ( !continuationTask.IsCanceled )
                            {
                                var downloadPart = continuationTask.Result;
                                if ( (downloadPart.Error != null) || !downloadPart.TotalContentLength.HasValue )
                                {
                                    rsp.SuccessReceivedPartCount = successReceivedPartCount;
                                    rsp.FailedReceivedPartCount  = Interlocked.Increment( ref failedReceivedPartCount );
                                }
                                else
                                {
                                    rsp.SuccessReceivedPartCount = Interlocked.Increment( ref successReceivedPartCount );
                                    rsp.FailedReceivedPartCount = failedReceivedPartCount;
                                    rsp.BytesLength             = (int) downloadPart.TotalContentLength.Value; //(int) downloadPart.Stream.Length;
                                }
                                rsp.Part = downloadPart;
                                ip.ResponseStepAction?.Invoke( rsp );

                                lock ( downloadPartsSet )
                                {
                                    downloadPartsSet.Add( part );
                                    canExtractPartEvent.Set();
                                }
                            }
                        }
                        , ct );
                }
            }
            , ct );

            //-2-//
            var waitHandles = new[] { canExtractPartEvent /*0*/, ct.WaitHandle /*1*/ };
            for ( var localReadyParts = new Queue< m3u8_part_ts >( Math.Min( 0x1000, ip.MaxDegreeOfParallelism ) );
                      expectedPartNumber <= maxPartNumber; )
            {
                var idx = WaitHandle.WaitAny( waitHandles );
                if ( idx == 1 ) //[ct.IsCancellationRequested := 1]
                    break;
                if ( idx != 0 ) //[canExtractPartEvent := 0]
                    continue;

                lock ( downloadPartsSet )
                {
                    for ( ; downloadPartsSet.Count != 0; )
                    {
                        var min_part = downloadPartsSet.Min;
                        downloadPartsSet.Remove( min_part );
                        localReadyParts.Enqueue( min_part );

                        Interlocked.Increment( ref expectedPartNumber );
                        ip.DownloadThreadsSemaphore.Release();
                    }
                }

                for ( ; localReadyParts.Count != 0; )
                {
                    var part = localReadyParts.Dequeue();
                    yield return (part);
                }
            }

            //-3.0-//
            if ( ct.IsCancellationRequested ) throw (new m3u8_Exception( "Canceled after part download error" ));
#if NETCOREAPP
            //-3.1-//
            await task_download.CAX();	
#else
            //-3.1-//
            task_download.Wait();
#endif

            //-4-//
            ct.ThrowIfCancellationRequested();
        }
        //-----------------------------------------------------------------------------//

        public static async Task< DownloadPartsAndSaveResult > DownloadPartsAndSave_As_SeparateFiles( DownloadPartsAndSaveInputParams ip, CancellationToken ct = default )
        {            
            if ( ip.mc == null )                               throw (new m3u8_ArgumentException( nameof(ip.mc) ));
            if ( !ip.m3u8File.Parts.AnyEx() )                  throw (new m3u8_ArgumentException( nameof(ip.m3u8File) ));
            if ( ip.OutputFileName.IsNullOrWhiteSpace() )      throw (new m3u8_ArgumentException( nameof(ip.OutputFileName) ));
            if ( ip.DownloadThreadsSemaphore         == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore) ));
            if ( ip.DownloadThreadsSemaphore_4_Parts == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore_4_Parts) ));
            if ( ip.WaitIfPausedHolder               == null ) throw (new m3u8_ArgumentException( nameof(ip.WaitIfPausedHolder) ));
            if ( ip.StreamPool                       == null ) throw (new m3u8_ArgumentException( nameof(ip.StreamPool) ));
            if ( ip.TimeoutCtsPool                   == null ) throw (new m3u8_ArgumentException( nameof(ip.TimeoutCtsPool) ));
            var receivedAndWritedPartsProcessor = ip.ReceivedAndWritedPartsProcessor ?? ReceivedAndWritedPartsProcessor._Dummy_.Inst;
            //---------------------------------------------------------------------------------------------------------//

            //-1-//
            var directoryName = Path.GetDirectoryName( ip.OutputFileName );
            bool outputDirectoryExists;
            if ( !(outputDirectoryExists = Directory.Exists( directoryName )) ) Directory.CreateDirectory( directoryName );

            //-2-//
            var m3u8File_fn             = Path.GetFileName( ip.m3u8File.BaseAddress.LocalPath );
            var m3u8File_OutputFileName = Path.Combine( directoryName, m3u8File_fn );
            File.WriteAllText( m3u8File_OutputFileName, ip.m3u8File.RawText );

            //-3.1-//
            var res = new DownloadPartsAndSaveResult( m3u8File_OutputFileName );

            //-3.2-//
            var downloadParts = DownloadParts_Routine( ip, already_successReceivedPartCount: 0, ct );
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

                    var downloadPart_url            = downloadPart.GetPartUrl( ip.m3u8File.BaseAddress );
                    var downloadPart_fn             = Path.GetFileName( downloadPart_url.LocalPath );
                    var downloadPart_OutputFileName = Path.Combine( directoryName, downloadPart_fn );
                    using ( var fs = FileHelper.File_Open4Write( downloadPart_OutputFileName ) )
                    {
                        await downloadPart.Stream.CopyToAsyncEx( fs, ct ).CAX();
                        await fs.FlushAsync( ct ).CAX();
                    }

                    res.PartsSuccessCount++;
                    res.TotalBytes += (uint) downloadPart.Stream.Length;
                }
            }

            return (res);
        }
    }
}
