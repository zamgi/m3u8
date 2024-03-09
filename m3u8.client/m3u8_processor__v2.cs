using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using m3u8.ext;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    public static class m3u8_processor__v2
    {
        private const int MAX_SEMAPHORE_NAME_LENGTH = 255;

        /// <summary>
        /// 
        /// </summary>
        private struct semaphore_download_threads_t : IDisposable
        {
            private SemaphoreSlim _SelfAppSemaphore;
            private Semaphore     _CrossAppSemaphore;
            private bool          _IsCrossAppSemaphoreOwner;
            private int           _CrossAppSemaphoreWaitAcquireCount;

            public semaphore_download_threads_t( bool useCrossAppInstanceDegreeOfParallelism, int maxDegreeOfParallelism )
            {
                if ( useCrossAppInstanceDegreeOfParallelism )
                {
                    _SelfAppSemaphore = null;
                    using ( var p = Process.GetCurrentProcess() )
                    {
                        _CrossAppSemaphore = new Semaphore( maxDegreeOfParallelism, maxDegreeOfParallelism,
                                                    p.MainModule.FileName.Replace( '\\', '/' ).TrimFromBegin( MAX_SEMAPHORE_NAME_LENGTH ) //need for escape-replace '\'
                                                    , out _IsCrossAppSemaphoreOwner );
                    }
                }
                else
                {
                    _SelfAppSemaphore         = new SemaphoreSlim( maxDegreeOfParallelism );
                    _CrossAppSemaphore        = null;
                    _IsCrossAppSemaphoreOwner = false;
                }
                _CrossAppSemaphoreWaitAcquireCount = 0;
            }
            public void Dispose()
            {
                if ( _SelfAppSemaphore != null )
                {
                    _SelfAppSemaphore.Dispose();
                    _SelfAppSemaphore = null;
                }

                if ( _CrossAppSemaphore != null )
                {
                    for ( ; 0 < _CrossAppSemaphoreWaitAcquireCount; _CrossAppSemaphoreWaitAcquireCount-- )
                    {
                        _CrossAppSemaphore.Release();
                    }
                    //if ( _IsCrossAppSemaphoreOwner )
                    //{
                        _CrossAppSemaphore.Dispose();
                    //}
                    _CrossAppSemaphore = null;
                }
            }

            public bool UseCrossAppInstanceDegreeOfParallelism { [M(O.AggressiveInlining)] get => (_CrossAppSemaphore != null); }

            public void Wait( CancellationToken ct )
            {
                if ( UseCrossAppInstanceDegreeOfParallelism )
                {
                    const int SEMAPHORE_millisecondsTimeout = 250;
                    const int CT_millisecondsTimeout        = 1;

                    //--!!!-- required here (because the calling context (task) is interrupted by the ct-CancellationToken --!!!--//
                    Thread.BeginCriticalRegion();
                    try
                    {
                        try { }
                        finally
                        {
                            for ( var i = 0; ; i = SEMAPHORE_millisecondsTimeout )
                            {
                                if ( _CrossAppSemaphore.WaitOne( i ) )
                                {
                                    Interlocked.Increment( ref _CrossAppSemaphoreWaitAcquireCount );
                                    break;
                                }
                                if ( ct.WaitHandle.WaitOne( CT_millisecondsTimeout ) )
                                {
                                    break;
                                }
                            }
                        }
                    }
                    finally
                    {
                        Thread.EndCriticalRegion();
                    }

                    ct.ThrowIfCancellationRequested();
                }
                else
                {
                    _SelfAppSemaphore.Wait( ct );
                }
            }
            public int Release()
            {
                if ( UseCrossAppInstanceDegreeOfParallelism )
                {
                    //Interlocked.Decrement( ref _CrossAppSemaphoreWaitAcquireCount );
                    //return (_CrossAppSemaphore.Release());

                    //--!!!-- non-required here (because calling context not-interrupted) --!!!--//
                    Thread.BeginCriticalRegion();
                    try
                    {
                        int r;
                        try { }
                        finally
                        {
                            Interlocked.Decrement( ref _CrossAppSemaphoreWaitAcquireCount );
                            r = _CrossAppSemaphore.Release();
                        }
                        return (r);
                    }
                    finally
                    {
                        Thread.EndCriticalRegion();
                    }
                }
                else
                {
                    return (_SelfAppSemaphore.Release());
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private struct semaphore_app_instance_t : IDisposable
        {
            private Semaphore _Semaphore;
            private bool      _IsSemaphoreOwner;
            private bool      _IsSemaphoreWasAcquired;

            private semaphore_app_instance_t( int maxDownloadAppInstance )
            {                
                using ( var p = Process.GetCurrentProcess() )
                {
                    _Semaphore = new Semaphore( maxDownloadAppInstance, maxDownloadAppInstance,
                                                $"{p.MainModule.FileName.Replace( '\\', '/' )}--app-instance".TrimFromBegin( MAX_SEMAPHORE_NAME_LENGTH ) //need for escape-replace '\'
                                                , out _IsSemaphoreOwner );
                }
                _IsSemaphoreWasAcquired = false;
            }
            public void Dispose()
            {
                if ( _Semaphore != null )
                {
                    if ( _IsSemaphoreWasAcquired )
                    {
                        _Semaphore.Release();
                    }
                    //if ( _IsSemaphoreOwner )
                    //{
                        _Semaphore.Dispose();
                    //}
                    _Semaphore = null;
                }
            }

            private void Wait( CancellationToken ct )
            {
                const int SEMAPHORE_millisecondsTimeout = 250;
                const int CT_millisecondsTimeout        = 250;

                //--!!!-- non-required here (seemingly), (because calling context not-interrupted) --!!!--//
                Thread.BeginCriticalRegion();
                try
                {
                    try { }
                    finally
                    {
                        _IsSemaphoreWasAcquired = false;
                        while ( !_Semaphore.WaitOne( SEMAPHORE_millisecondsTimeout ) )
                        {
                            if ( ct.WaitHandle.WaitOne( CT_millisecondsTimeout ) )
                            {
                                ct.ThrowIfCancellationRequested();
                            }
                        }
                        _IsSemaphoreWasAcquired = true;
                    }
                }
                finally
                {
                    Thread.EndCriticalRegion();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            private struct dummy_disposable : IDisposable
            {
                void IDisposable.Dispose() { }
            }

            public static IDisposable WaitForOtherAppInstance( int? maxDownloadAppInstance, CancellationToken ct, Action waitingForOtherAppInstanceFinished, int delayTimeout )
            {
                if ( maxDownloadAppInstance.HasValue )
                {
                    using ( var delayTaskCts = new CancellationTokenSource() )
                    using ( var joinedCts    = CancellationTokenSource.CreateLinkedTokenSource( ct, delayTaskCts.Token ) )
                    {
                        var delayTask = Task.Delay( delayTimeout, joinedCts.Token );
                        delayTask.ContinueWith( t => waitingForOtherAppInstanceFinished?.Invoke(), joinedCts.Token );

                        var semaphore_app_instance = new semaphore_app_instance_t( maxDownloadAppInstance.Value );
                        try
                        {
                            semaphore_app_instance.Wait( ct );
                            delayTaskCts.Cancel();

                            return (semaphore_app_instance);
                        }
                        catch ( Exception ex )
                        {
                            Debug.WriteLine( ex );

                            semaphore_app_instance.Dispose();
                        }                                               
                    }
                }
                return (default(dummy_disposable));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private struct download_m3u8File_parts_parallel_params_t
        {
            public download_m3u8File_parts_parallel_params_t( m3u8_client _mc, in m3u8_file_t__v2 _m3u8File, in DownloadFileAndSaveInputParams ip )
            {
                mc                     = _mc;
                m3u8File               = _m3u8File;
                cancellationToken      = ip.CancellationToken;
                requestStepAction      = ip.RequestStepAction;
                responseStepAction     = ip.ResponseStepAction;
                maxDegreeOfParallelism = ip.MaxDegreeOfParallelism;
                useCrossAppInstanceDegreeOfParallelism = ip.UseCrossAppInstanceDegreeOfParallelism;
            }
            public download_m3u8File_parts_parallel_params_t( in DownloadPartsAndSaveInputParams ip )
            {
                mc                     = ip.mc;
                m3u8File               = ip.m3u8File;
                cancellationToken      = ip.CancellationToken;
                waitIfPausedEvent      = ip.WaitIfPausedEvent;
                waitingIfPaused        = ip.WaitingIfPaused;
                requestStepAction      = ip.RequestStepAction;
                responseStepAction     = ip.ResponseStepAction;
                maxDegreeOfParallelism = ip.MaxDegreeOfParallelism;
                useCrossAppInstanceDegreeOfParallelism = ip.UseCrossAppInstanceDegreeOfParallelism;
            }
        
            public m3u8_client     mc       { get; set; }
            public m3u8_file_t__v2 m3u8File { get; set; }

            public CancellationToken          cancellationToken      { get; set; }
            public ManualResetEventSlim       waitIfPausedEvent      { get; set; }
            public Action                     waitingIfPaused        { get; set; }
            public RequestStepActionDelegate  requestStepAction      { get; set; }
            public ResponseStepActionDelegate responseStepAction     { get; set; }
            public int                        maxDegreeOfParallelism { get; set; }
            public bool                       useCrossAppInstanceDegreeOfParallelism { get; set; }

            public int?                       poolStreamCapacity { get; set; }
        }

        private static IEnumerable< m3u8_part_ts__v2 > download_m3u8File_parts_parallel( download_m3u8File_parts_parallel_params_t ip )
        {
            var baseAddress              = ip.m3u8File.BaseAddress;
            var totalPatrs               = ip.m3u8File.Parts.Count;
            var successReceivedPartCount = 0;
            var failedReceivedPartCount  = 0;

            ip.responseStepAction?.Invoke( new ResponseStepActionParams( totalPatrs ) );

            var expectedPartNumber = ip.m3u8File.Parts.FirstOrDefault().OrderNumber;
            var maxPartNumber      = ip.m3u8File.Parts.LastOrDefault ().OrderNumber;
            var sourceQueue        = new Queue< m3u8_part_ts__v2 >( ip.m3u8File.Parts );
            var downloadPartsSet   = new SortedSet< m3u8_part_ts__v2 >( m3u8_part_ts__v2.Comparer.Inst );
            var poolStreamCapacity = ip.poolStreamCapacity.GetValueOrDefault( 1024 * 1024 * 5 );

            using ( DefaultConnectionLimitSaver.Create( ip.maxDegreeOfParallelism ) )
            using ( var innerCts            = new CancellationTokenSource() )
            using ( var joinedCts           = CancellationTokenSource.CreateLinkedTokenSource( ip.cancellationToken, innerCts.Token ) )
            using ( var canExtractPartEvent = new AutoResetEvent( false ) )
            using ( var semaphore           = new semaphore_download_threads_t( ip.useCrossAppInstanceDegreeOfParallelism, ip.maxDegreeOfParallelism ) )
            using ( var pool                = new ObjectPoolDisposable< MemoryStream >( ip.maxDegreeOfParallelism, () => new MemoryStream( poolStreamCapacity ) ) )
            {
                //-1-//
                var task_download = Task.Run( () =>
                {
                    for ( var n = 1; sourceQueue.Count != 0; n++ )
                    {
                        if ( !(ip.waitIfPausedEvent?.IsSet).GetValueOrDefault( true ) )
                        {
                            ip.waitingIfPaused?.Invoke();
                            ip.waitIfPausedEvent.Wait( joinedCts.Token );
                        }

                        semaphore.Wait( /*ct*/ joinedCts.Token );
                        var part = sourceQueue.Dequeue();

                        var rq = RequestStepActionParams.CreateSuccess( totalPatrs, n, part );
                        ip.requestStepAction?.Invoke( rq );

                        var holder = pool.GetHolder();                        
                        part.SetStreamHolder( holder );

//---#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        ip.mc.DownloadPart__v2( part, baseAddress, /*ct*/ joinedCts.Token )
                            .ContinueWith( continuationTask =>
                            {
                                var rsp = new ResponseStepActionParams( totalPatrs );

                                if ( continuationTask.IsFaulted )
                                {
                                    Interlocked.Increment( ref expectedPartNumber );

                                    part.SetError( continuationTask.Exception );

                                    rsp.SuccessReceivedPartCount = successReceivedPartCount;
                                    rsp.FailedReceivedPartCount  = Interlocked.Increment( ref failedReceivedPartCount );
                                    rsp.Part                     = part;

                                    ip.responseStepAction?.Invoke( rsp );

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
                                    ip.responseStepAction?.Invoke( rsp );

                                    lock ( downloadPartsSet )
                                    {
                                        downloadPartsSet.Add( downloadPart );
                                        canExtractPartEvent.Set();
                                    }
                                }
                            }, /*ct*/ joinedCts.Token );
//---#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }, /*ct*/ joinedCts.Token );

                //-2-//
                for ( var localReadyParts = new Queue< m3u8_part_ts__v2 >( Math.Min( 0x1000, ip.maxDegreeOfParallelism ) );
                          expectedPartNumber <= maxPartNumber; )
                {
                    var idx = WaitHandle.WaitAny( new[] { canExtractPartEvent /*0*/, /*ct*/ joinedCts.Token.WaitHandle /*1*/, } );
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

                                semaphore.Release();

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
                    throw (new m3u8_Exception( "Canceled after part download error." ));
                }

                //-3-//
                task_download.Wait();
            }

            //-4-//
            ip.cancellationToken.ThrowIfCancellationRequested();
        }

        [M(O.AggressiveInlining)] private static Task CopyToAsyncEx( this Stream source, Stream destination, CancellationToken ct )
        {
            source.Seek( 0, SeekOrigin.Begin );
#if NETCOREAPP
            return (source.CopyToAsync( destination, ct ));
#else
            return (source.CopyToAsync( destination, bufferSize: 81920, ct ));
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

            internal static RequestStepActionParams CreateSuccess( int totalPartCount, int partOrderNumber, m3u8_part_ts__v2 part ) 
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
            internal ResponseStepActionParams( int totalPartCount ) => TotalPartCount = totalPartCount;

            public int TotalPartCount           { get; private  set; }
            public int SuccessReceivedPartCount { get; internal set; }
            public int FailedReceivedPartCount  { get; internal set; }
            public int BytesLength              { get; internal set; }
            public m3u8_part_ts__v2 Part        { get; internal set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public delegate void ResponseStepActionDelegate( ResponseStepActionParams p );

        /// <summary>
        /// 
        /// </summary>
        public struct DownloadFileAndSaveInputParams
        {
            public const int DEFAULT_MAXDEGREEOFPARALLELISM    = 64;
            public const int DEFAULT_ATTEMPTREQUESTCOUNTBYPART = 10;

            public string m3u8FileUrl    { get; set; }
            public string OutputFileName { get; set; }

            public CancellationToken          CancellationToken  { get; set; }
            public RequestStepActionDelegate  RequestStepAction  { get; set; }
            public ResponseStepActionDelegate ResponseStepAction { get; set; }
            public bool UseCrossAppInstanceDegreeOfParallelism   { get; set; }

            private int? _MaxDegreeOfParallelism;
            public int MaxDegreeOfParallelism
            {
                get => _MaxDegreeOfParallelism.GetValueOrDefault( DEFAULT_MAXDEGREEOFPARALLELISM );
                set => _MaxDegreeOfParallelism = Math.Max( 1, value );
            }

            private m3u8_client.init_params? _NetParams;
            public m3u8_client.init_params NetParams
            {
                get
                {
                    if ( !_NetParams.HasValue )
                    {
                        _NetParams = new m3u8_client.init_params()
                        {
                            AttemptRequestCount = DEFAULT_ATTEMPTREQUESTCOUNTBYPART,
                        };
                    }
                    return (_NetParams.Value);
                }
                set
                {
                    value.AttemptRequestCount = Math.Max( 1, value.AttemptRequestCount.GetValueOrDefault( DEFAULT_ATTEMPTREQUESTCOUNTBYPART ) );
                    _NetParams = value;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public struct DownloadFileAndSaveResult
        {
            internal DownloadFileAndSaveResult( DownloadFileAndSaveInputParams ip )
            {
                m3u8FileUrl    = ip.m3u8FileUrl;
                OutputFileName = ip.OutputFileName;
            }

            public string m3u8FileUrl    { get; internal set; }
            public string OutputFileName { get; internal set; }

            public int PartsSuccessCount { get; internal set; }
            public int PartsErrorCount   { get; internal set; }
            public int TotalBytes        { get; internal set; }

            public int TotalParts => (PartsSuccessCount + PartsErrorCount);
        }

        public static async Task< DownloadFileAndSaveResult > DownloadFileAndSave( DownloadFileAndSaveInputParams ip )
        {
            if ( ip.m3u8FileUrl   .IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(ip.m3u8FileUrl) ));
            if ( ip.OutputFileName.IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(ip.OutputFileName) ));
            //---------------------------------------------------------------------------------------------------------//

            var m3u8FileUrl = new Uri( ip.m3u8FileUrl );
            
            using ( var mc = m3u8_client_factory.Create( ip.NetParams ) )
            {
                var res = new DownloadFileAndSaveResult( ip );

                await Task.Run( async () =>
                {
                    var ct = ip.CancellationToken;

                    //-1-//
                    var mf = await mc.DownloadFile( m3u8FileUrl, ct ).CAX();
                    var m3u8File = m3u8_file_t__v2.Parse( mf );

                    //-2-//
                    //var resp_dict          = default(ConcurrentDictionary< int, ResponseStepActionParams >);
                    //var responseStepAction = default(ResponseStepActionDelegate);
                    //if ( ip.ResponseStepAction != null )
                    //{
                    //    resp_dict = new ConcurrentDictionary< int, ResponseStepActionParams >();
                    //    responseStepAction = ip.ResponseStepAction;
                    //    ip.ResponseStepAction = t => resp_dict[ t.Part.OrderNumber ] = t;
                    //}

                    var tp = new download_m3u8File_parts_parallel_params_t( mc, m3u8File, ip );
                    var downloadParts = download_m3u8File_parts_parallel( tp );

                    //-3.1-//
                    var directoryName = Path.GetDirectoryName( ip.OutputFileName );
                    if ( !Directory.Exists( directoryName ) ) Directory.CreateDirectory( directoryName );

                    //-3-//                    
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

                                await downloadPart.Stream.CopyToAsyncEx( fs, ct ).CAX();

                                res.PartsSuccessCount++;
                                res.TotalBytes += (int) downloadPart.Stream.Length;

                                //------------------------------------//
                                //if ( (resp_dict != null) && resp_dict.TryRemove( downloadPart.OrderNumber, out var rsp ) )
                                //{
                                //    responseStepAction.Invoke( rsp );
                                //}
                            }
                        }
                    }

                }, ip.CancellationToken ).CAX();

                return (res);
            }
        }
        //-----------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public struct DownloadPartsAndSaveInputParams
        {
            public m3u8_client     mc             { get; set; }
            public m3u8_file_t__v2 m3u8File       { get; set; }
            public string          OutputFileName { get; set; }

            public CancellationToken          CancellationToken      { get; set; }
            public ManualResetEventSlim       WaitIfPausedEvent      { get; set; }
            public RequestStepActionDelegate  RequestStepAction      { get; set; }
            public ResponseStepActionDelegate ResponseStepAction     { get; set; }
            public int                        MaxDegreeOfParallelism { get; set; }
            public bool                       UseCrossAppInstanceDegreeOfParallelism { get; set; }

            public int?   MaxDownloadAppInstance             { get; set; }
            public Action WaitingForOtherAppInstanceFinished { get; set; }
            public Action WaitingIfPaused                    { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public struct DownloadPartsAndSaveResult
        {
            internal DownloadPartsAndSaveResult( string outputFileName ) => OutputFileName = outputFileName;

            public string OutputFileName   { get; private set; }

            public int   PartsSuccessCount { get; internal set; }
            public int   PartsErrorCount   { get; internal set; }
            public ulong TotalBytes        { get; internal set; }

            public int TotalParts => (PartsSuccessCount + PartsErrorCount);
            public void ResetOutputFileName( string outputFileName ) => OutputFileName = outputFileName;
            public bool IsEmpty() => ((OutputFileName == null) && (PartsSuccessCount == 0) && (PartsErrorCount == 0) && (TotalBytes == 0UL));
        }

        public static async Task< DownloadPartsAndSaveResult > DownloadPartsAndSave( DownloadPartsAndSaveInputParams ip )
        {            
            if ( ip.mc == null )                          throw (new m3u8_ArgumentException( nameof(ip.mc) ));
            if ( !ip.m3u8File.Parts.AnyEx() )             throw (new m3u8_ArgumentException( nameof(ip.m3u8File) ));
            if ( ip.OutputFileName.IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(ip.OutputFileName) ));
            //---------------------------------------------------------------------------------------------------------//

            //-1-//
            var res = new DownloadPartsAndSaveResult( ip.OutputFileName );

            using ( var semaphore_app_instance = semaphore_app_instance_t.WaitForOtherAppInstance( ip.MaxDownloadAppInstance, 
                                                                                                   ip.CancellationToken,
                                                                                                   ip.WaitingForOtherAppInstanceFinished,
                                                                                                   delayTimeout: 1000 ) )
            {
                //-2-//
                //var resp_dict          = default(ConcurrentDictionary< int, ResponseStepActionParams >);
                //var responseStepAction = default(ResponseStepActionDelegate);
                //if ( ip.ResponseStepAction != null )
                //{
                //    resp_dict = new ConcurrentDictionary< int, ResponseStepActionParams >();
                //    responseStepAction = ip.ResponseStepAction;
                //    ip.ResponseStepAction = t => resp_dict[ t.Part.OrderNumber ] = t;
                //}

                var tp = new download_m3u8File_parts_parallel_params_t( ip );
                var downloadParts = download_m3u8File_parts_parallel( tp );

                //-3.1-//
                var directoryName = Path.GetDirectoryName( ip.OutputFileName );
                if ( !Directory.Exists( directoryName ) ) Directory.CreateDirectory( directoryName );

                //-3-//
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

                            //------------------------------------//
                            //if ( (resp_dict != null) && resp_dict.TryRemove( downloadPart.OrderNumber, out var rsp ) )
                            //{
                            //    responseStepAction.Invoke( rsp );
                            //}
                        }

                    }
                }
            }

            return (res);
        }
        //-----------------------------------------------------------------------------//
    }
}
