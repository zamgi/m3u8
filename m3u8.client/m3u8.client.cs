using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.ext
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        [M(O.AggressiveInlining)] public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
        [M(O.AggressiveInlining)] public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );
        [M(O.AggressiveInlining)] public static string AsPartExceptionMessage( this string responseText ) => (responseText.IsNullOrWhiteSpace() ? string.Empty : ($", '{responseText}'"));
        [M(O.AggressiveInlining)] public static string CreateExceptionMessage( this HttpResponseMessage response, string responseText ) => ($"{(int) response.StatusCode}, {response.ReasonPhrase}{responseText.AsPartExceptionMessage()}");
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IEnumerable< T > seq ) => (seq != null && seq.Any());

        public static string ReadAsStringAsyncEx( this HttpContent content, CancellationToken ct )
        {
            var t = content.ReadAsStringAsync();
            t.Wait( ct );
            return (t.Result);
        }
        public static byte[] ReadAsByteArrayAsyncEx( this HttpContent content, CancellationToken ct )
        {
            var t = content.ReadAsByteArrayAsync();
            t.Wait( ct );
            return (t.Result);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal struct DefaultConnectionLimitSaver : IDisposable
    {
        private int _DefaultConnectionLimit;
        private DefaultConnectionLimitSaver( int connectionLimit )
        {
            _DefaultConnectionLimit = ServicePointManager.DefaultConnectionLimit;
            ServicePointManager.DefaultConnectionLimit = connectionLimit;
        }
        public static DefaultConnectionLimitSaver Create( int connectionLimit ) => new DefaultConnectionLimitSaver( connectionLimit );
        public void Dispose() => ServicePointManager.DefaultConnectionLimit = _DefaultConnectionLimit;
    }
}

namespace m3u8
{
    using m3u8.ext;

    /// <summary>
    /// 
    /// </summary>
    public struct m3u8_part_ts
    {
        public m3u8_part_ts( string relativeUrlName, int orderNumber ) : this()
        {
            RelativeUrlName = relativeUrlName;
            OrderNumber     = orderNumber;
        }

        public string RelativeUrlName { get; private set; }
        public int OrderNumber { get; private set; }

        public byte[] Bytes { get; private set; }
        public void SetBytes( byte[] bytes ) => Bytes = bytes;

        public Exception Error { get; private set; }
        public void SetError( Exception error ) => Error = error;
#if DEBUG
        public override string ToString() => ($"{OrderNumber}, '{RelativeUrlName}'");
#endif
    }
    /// <summary>
    /// 
    /// </summary>
    public struct m3u8_part_ts_comparer: IComparer< m3u8_part_ts >
    {
        public int Compare( m3u8_part_ts x, m3u8_part_ts y ) => (x.OrderNumber - y.OrderNumber);
    }

    /// <summary>
    /// 
    /// </summary>
    public struct m3u8_file_t
    {
        public IReadOnlyList< m3u8_part_ts > Parts { get; private set; }

        public Uri BaseAddress { get; private set; }

        public string RawText { get; private set; }

        public static m3u8_file_t Parse( string content, Uri baseAddress )
        {
            var lines = from row in content.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries )
                        let line = row.Trim()
                        where (!line.IsNullOrEmpty() && !line.StartsWith( "#" ))
                        select line
                        ;
            var parts = lines.Select( (line, i) => new m3u8_part_ts( line, i ) );
            var o = new m3u8_file_t()
            {
                Parts       = parts.ToList().AsReadOnly(),
                BaseAddress = baseAddress,
                RawText     = content,
            };
            return (o);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public static class m3u8_Extensions
    {
        public static string Unwrap4DialogMessage( this Exception ex, out bool isCanceledException )
        {
            isCanceledException = false;

            var cex = ex as OperationCanceledException;
            if ( cex != null )
            {
                isCanceledException = true;
                return (cex.Message);
            }

            var sex = ex as m3u8_ArgumentException;
            if ( sex != null )
            {
                return ($"m3u8_ArgumentException: '{sex.Message} => [{sex.ParamName}]'");
            }

            var aex = ex as AggregateException;
            if ( aex != null )
            {
                if ( aex.InnerExceptions.All( _ex => _ex is OperationCanceledException ) )
                {
                    isCanceledException = true;
                    return (aex.InnerExceptions.FirstOrDefault()?.Message);
                }

                if ( aex.InnerExceptions.Count == 1 )
                {
                    if ( aex.InnerException is m3u8_Exception )
                    {
                        return ($"m3u8_Exception: '{((m3u8_Exception) aex.InnerException).Message}'");
                    }
                    else if ( aex.InnerException is HttpRequestException )
                    {
                        var message = "HttpRequestException: '";
                        for ( Exception x = ((HttpRequestException) aex.InnerException); x != null; x = x.InnerException )
                        {
                            message += x.Message + Environment.NewLine;
                        }
                        return (message + '\'');
                    }
                    else
                    {
                        return ($"{ex.GetType().Name}: '{ex}'");
                    }
                }
            }

            return (ex.ToString());
        }

        [M(O.AggressiveInlining)]
        public static string Unwrap4DialogMessage( this Exception ex, bool ignoreCanceledException = true )
        {
            var message = ex.Unwrap4DialogMessage( out var isCanceledException );
            return ((isCanceledException && ignoreCanceledException) ? null : message);
        }

        internal static string TrimFromBegin( this string s, int maxLength )
        {
            if ( maxLength < s.Length )
            {
                return (s.Substring( s.Length - maxLength ));
            }
            return (s);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_ArgumentException : ArgumentNullException
    {
        public m3u8_ArgumentException( string paramName ) : base( paramName ) { }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_Exception : HttpRequestException
    {        
        public m3u8_Exception( string message ) : base( message ) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_client : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public struct init_params
        {
            public int?      AttemptRequestCount { get; set; }
            public TimeSpan? Timeout             { get; set; }
            public bool?     ConnectionClose     { get; set; }
        }

        #region [.field's.]
        private HttpClient _HttpClient; 
        #endregion

        #region [.ctor().]
        public m3u8_client( init_params ip )
        {
            InitParams = ip;

            _HttpClient = new HttpClient();
            _HttpClient.DefaultRequestHeaders.ConnectionClose = ip.ConnectionClose; //true => //.KeepAlive = false, .Add("Connection", "close");
            if ( ip.Timeout.HasValue )
            {
                _HttpClient.Timeout = ip.Timeout.Value;
            };
        }

        public void Dispose() => _HttpClient.Dispose();
        #endregion

        public init_params InitParams { get; private set; }

        public async Task< m3u8_file_t > DownloadFile( Uri url
            , CancellationToken? cancellationToken = null )
        {
            if ( url == null ) throw (new m3u8_ArgumentException( nameof(url) ));
            //------------------------------------------------------------------//

            var ct = cancellationToken.GetValueOrDefault( CancellationToken.None );
            var attemptRequestCountByPart = InitParams.AttemptRequestCount.GetValueOrDefault( 1 );

            for ( var attemptRequestCount = attemptRequestCountByPart; 0 < attemptRequestCount; attemptRequestCount-- )
            {
                try
                {
                    using ( HttpResponseMessage response = await _HttpClient.GetAsync( url, ct ) )
                    using ( HttpContent content = response.Content )
                    {
                        if ( !response.IsSuccessStatusCode )
                        {
                            var responseText = content.ReadAsStringAsyncEx( ct );
                            throw (new m3u8_Exception( response.CreateExceptionMessage( responseText ) ));
                        }

                        var text = content.ReadAsStringAsyncEx( ct );
                        var m3u8File = m3u8_file_t.Parse( text, url );
                        return (m3u8File);
                    }
                }
                catch ( Exception /*ex*/ )
                {
                    if ( (attemptRequestCount == 1) || ct.IsCancellationRequested )
                    {
                        throw;
                    }
                }
            }

            throw (new m3u8_Exception( $"No content found while {attemptRequestCountByPart} attempt requests" ));
        }

        public async Task< m3u8_part_ts > DownloadPart( m3u8_part_ts part
            , Uri baseAddress
            , CancellationToken? cancellationToken = null )
        {
            if ( baseAddress == null )                       throw (new m3u8_ArgumentException( nameof(baseAddress) ));
            if ( part.RelativeUrlName.IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(part.RelativeUrlName) ));
            //----------------------------------------------------------------------------------------------------------------//

            var url = new Uri( baseAddress, part.RelativeUrlName );
            var ct = cancellationToken.GetValueOrDefault( CancellationToken.None );
            var attemptRequestCountByPart = InitParams.AttemptRequestCount.GetValueOrDefault( 1 );

            for ( var attemptRequestCount = attemptRequestCountByPart; 0 < attemptRequestCount; attemptRequestCount-- )
            {
                try
                {
                    using ( HttpResponseMessage response = await _HttpClient.GetAsync( url, ct ) )
                    using ( HttpContent content = response.Content )
                    {
                        if ( !response.IsSuccessStatusCode )
                        {
                            var responseText = content.ReadAsStringAsyncEx( ct );
                            throw (new m3u8_Exception( response.CreateExceptionMessage( responseText ) ));
                        }

                        var bytes = content.ReadAsByteArrayAsyncEx( ct );
                        part.SetBytes( bytes );
                        return (part);
                    }
                }
                catch ( Exception ex )
                {
                    if ( (attemptRequestCount == 1) || ct.IsCancellationRequested )
                    {
                        part.SetError( ex );
                        return (part);
                    }
                }
            }

            throw (new m3u8_Exception( $"No content found while {attemptRequestCountByPart} attempt requests" ));
        }


        public static m3u8_client Create( int attemptRequestCountByPart = 10, TimeSpan? timeout = null )
        {
            var ip = new init_params()
            {
                Timeout             = timeout,
                AttemptRequestCount = Math.Max( attemptRequestCountByPart, 1 ),
            };
            var mc = new m3u8_client( ip );
            return (mc);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class m3u8_processor
    {
        /// <summary>
        /// 
        /// </summary>
        private struct semaphore_download_threads_t : IDisposable
        {
            private SemaphoreSlim _SemaphoreSlim;
            private Semaphore     _Semaphore;
            private bool          _IsSemaphoreOwner;

            public semaphore_download_threads_t( bool useCrossAppInstanceDegreeOfParallelism, int maxDegreeOfParallelism )
            {
                if ( useCrossAppInstanceDegreeOfParallelism )
                {
                    _SemaphoreSlim = null;
                    using ( var p = Process.GetCurrentProcess() )
                    {
                        _Semaphore = new Semaphore( maxDegreeOfParallelism, maxDegreeOfParallelism,
                                                    p.MainModule.FileName.Replace( '\\', '/' ) //need for escape-replace '\'
                                                    , out _IsSemaphoreOwner );
                    }
                }
                else
                {
                    _SemaphoreSlim    = new SemaphoreSlim( maxDegreeOfParallelism );
                    _Semaphore        = null;
                    _IsSemaphoreOwner = false;
                }
            }
            public void Dispose()
            {
                if ( _SemaphoreSlim != null )
                {
                    _SemaphoreSlim.Dispose();
                    _SemaphoreSlim = null;
                }
                if ( _Semaphore != null )
                {
                    if ( _IsSemaphoreOwner )
                    {
                        _Semaphore.Dispose();
                    }
                    _Semaphore = null;
                }
            }

            public bool UseCrossAppInstanceDegreeOfParallelism { [M(O.AggressiveInlining)] get => (_Semaphore != null); }

            public void Wait( CancellationToken ct )
            {
                if ( UseCrossAppInstanceDegreeOfParallelism )
                {
                    var idx = WaitHandle.WaitAny( new[] { _Semaphore /*0*/, ct.WaitHandle /*1*/, } );
                    if ( idx == 1 ) //[ct.IsCancellationRequested := 1]
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                    /*if ( idx != 0 ) //[_Semaphore := 0]
                    {
                        //WHAT TODO HERE?!
                    }*/
                }
                else
                {
                    _SemaphoreSlim.Wait( ct );
                }
            }
            public int Release() => (UseCrossAppInstanceDegreeOfParallelism ? _Semaphore.Release() : _SemaphoreSlim.Release());
        }
        /// <summary>
        /// 
        /// </summary>
        private struct semaphore_app_instance_t : IDisposable
        {
            private Semaphore _Semaphore;
            private bool      _IsSemaphoreOwner;

            private semaphore_app_instance_t( int maxDownloadAppInstance )
            {                
                using ( var p = Process.GetCurrentProcess() )
                {
                    _Semaphore = new Semaphore( maxDownloadAppInstance, maxDownloadAppInstance,
                                                $"{p.MainModule.FileName.Replace( '\\', '/' )}--app-instance".TrimFromBegin( 255 ) //need for escape-replace '\'
                                                , out _IsSemaphoreOwner );
                }
            }
            public void Dispose()
            {
                if ( _Semaphore != null )
                {
                    _Semaphore.Release();

                    if ( _IsSemaphoreOwner )
                    {
                        _Semaphore.Dispose();
                    }
                    _Semaphore = null;
                }
            }

            private void Wait( CancellationToken ct )
            {
                var idx = WaitHandle.WaitAny( new[] { _Semaphore /*0*/, ct.WaitHandle /*1*/, } );
                if ( idx == 1 ) //[ct.IsCancellationRequested := 1]
                {
                    ct.ThrowIfCancellationRequested();
                }
                /*if ( idx != 0 ) //[_Semaphore := 0]
                {
                    //WHAT TODO HERE?!
                }*/
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
            public download_m3u8File_parts_parallel_params_t( m3u8_client _mc, m3u8_file_t _m3u8File, DownloadFileAndSaveInputParams ip ) : this()
            {
                mc                     = _mc;
                m3u8File               = _m3u8File;
                cts                    = ip.Cts;
                requestStepAction      = ip.RequestStepAction;
                responseStepAction     = ip.ResponseStepAction;
                maxDegreeOfParallelism = ip.MaxDegreeOfParallelism;
                useCrossAppInstanceDegreeOfParallelism = ip.UseCrossAppInstanceDegreeOfParallelism;
            }
            public download_m3u8File_parts_parallel_params_t( DownloadPartsAndSaveInputParams ip ) : this()
            {
                mc                     = ip.mc;
                m3u8File               = ip.m3u8File;
                cts                    = ip.Cts;
                requestStepAction      = ip.RequestStepAction;
                responseStepAction     = ip.ResponseStepAction;
                maxDegreeOfParallelism = ip.MaxDegreeOfParallelism;
                useCrossAppInstanceDegreeOfParallelism = ip.UseCrossAppInstanceDegreeOfParallelism;
            }
        
            public m3u8_client mc       { get; set; }
            public m3u8_file_t m3u8File { get; set; }

            public CancellationTokenSource    cts                    { get; set; }
            public RequestStepActionDelegate  requestStepAction      { get; set; }
            public ResponseStepActionDelegate responseStepAction     { get; set; }
            public int                        maxDegreeOfParallelism { get; set; }
            public bool                       useCrossAppInstanceDegreeOfParallelism { get; set; }
        }

        private static IEnumerable< m3u8_part_ts > download_m3u8File_parts_parallel( download_m3u8File_parts_parallel_params_t ip )
        {
            var ct = (ip.cts?.Token).GetValueOrDefault( CancellationToken.None );
            var baseAddress = ip.m3u8File.BaseAddress;
            var totalPatrs  = ip.m3u8File.Parts.Count;
            var successReceivedPartCount = 0;
            var failedReceivedPartCount  = 0;

            ip.responseStepAction?.Invoke( new ResponseStepActionParams( totalPatrs ) );

            var expectedPartNumber = ip.m3u8File.Parts.FirstOrDefault().OrderNumber;
            var maxPartNumber      = ip.m3u8File.Parts.LastOrDefault ().OrderNumber;
            var sourceQueue        = new Queue< m3u8_part_ts >( ip.m3u8File.Parts );
            var downloadPartsSet   = new SortedSet< m3u8_part_ts >( default(m3u8_part_ts_comparer) );

            using ( DefaultConnectionLimitSaver.Create( ip.maxDegreeOfParallelism ) )
            using ( var innerCts  = new CancellationTokenSource() )
            using ( var joinedCts = CancellationTokenSource.CreateLinkedTokenSource( ct, innerCts.Token ) )
            using ( var canExtractPartEvent = new AutoResetEvent( false ) )
            using ( var semaphore = new semaphore_download_threads_t( ip.useCrossAppInstanceDegreeOfParallelism, ip.maxDegreeOfParallelism ) )
            {
                //-1-//
                var task_download = Task.Run( () =>
                {
                    for ( var n = 1; sourceQueue.Count != 0; n++ )
                    {
                        semaphore.Wait( joinedCts.Token );
                        var part = sourceQueue.Dequeue();

                        var rq = RequestStepActionParams.CreateSuccess( totalPatrs, n, part );
                        ip.requestStepAction?.Invoke( rq );

                        ip.mc.DownloadPart( part, baseAddress, joinedCts.Token )
                            .ContinueWith( ( continuationTask ) =>
                            {
                                var rsp = new ResponseStepActionParams( totalPatrs );

                                if ( continuationTask.IsFaulted )
                                {
                                    Interlocked.Increment( ref expectedPartNumber );

                                    part.SetError( continuationTask.Exception );

                                    rsp.SuccessReceivedPartCount = successReceivedPartCount;
                                    rsp.FailedReceivedPartCount = Interlocked.Increment( ref failedReceivedPartCount );
                                    rsp.Part = part;

                                    ip.responseStepAction?.Invoke( rsp );

                                    innerCts.Cancel();
                                }
                                else if ( !continuationTask.IsCanceled )
                                {
                                    var downloadPart = continuationTask.Result;
                                    if ( downloadPart.Error != null )
                                    {
                                        rsp.SuccessReceivedPartCount = successReceivedPartCount;
                                        rsp.FailedReceivedPartCount = Interlocked.Increment( ref failedReceivedPartCount );
                                    }
                                    else
                                    {
                                        rsp.SuccessReceivedPartCount = Interlocked.Increment( ref successReceivedPartCount );
                                        rsp.FailedReceivedPartCount = failedReceivedPartCount;
                                        rsp.BytesLength = downloadPart.Bytes.Length;
                                    }
                                    rsp.Part = downloadPart;
                                    ip.responseStepAction?.Invoke( rsp );

                                    lock ( downloadPartsSet )
                                    {
                                        downloadPartsSet.Add( downloadPart );
                                        canExtractPartEvent.Set();
                                    }
                                }
                            }, joinedCts.Token );
                    }
                }, joinedCts.Token );

                //-2-//
                for ( var localReadyParts = new Queue< m3u8_part_ts >( Math.Min( 0x1000, ip.maxDegreeOfParallelism ) );
                        expectedPartNumber <= maxPartNumber; )
                {
                    var idx = WaitHandle.WaitAny( new[] { canExtractPartEvent /*0*/, joinedCts.Token.WaitHandle /*1*/, } );
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
                    throw (new m3u8_Exception( "Canceled after part download error" ));
                }

                //-3-//
                task_download.Wait();
            }

            //-4-//
            ct.ThrowIfCancellationRequested();
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

            internal static RequestStepActionParams CreateSuccess( int totalPartCount, int partOrderNumber, m3u8_part_ts part ) 
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
            internal ResponseStepActionParams( int totalPartCount ) : this()
                => TotalPartCount = totalPartCount;

            public int TotalPartCount           { get; private  set; }
            public int SuccessReceivedPartCount { get; internal set; }
            public int FailedReceivedPartCount  { get; internal set; }
            public int BytesLength              { get; internal set; }
            public m3u8_part_ts Part            { get; internal set; }
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

            public CancellationTokenSource    Cts                { get; set; }
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
            internal DownloadFileAndSaveResult( DownloadFileAndSaveInputParams ip ) : this()
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

        public static async Task< DownloadFileAndSaveResult > DownloadFileAndSave_Async( DownloadFileAndSaveInputParams ip )
        {
            if ( ip.m3u8FileUrl.IsNullOrWhiteSpace() )    throw (new m3u8_ArgumentException( nameof(ip.m3u8FileUrl) ));
            if ( ip.OutputFileName.IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(ip.OutputFileName) ));
            //---------------------------------------------------------------------------------------------------------//

            var m3u8FileUrl = new Uri( ip.m3u8FileUrl );

            using ( var mc = new m3u8_client( ip.NetParams ) )
            {
                var ct = (ip.Cts?.Token).GetValueOrDefault( CancellationToken.None );
                var res = new DownloadFileAndSaveResult( ip );

                await Task.Run( async () =>
                {
                    //-1-//
                    var m3u8File = await mc.DownloadFile( m3u8FileUrl, ct );

                    //-2-//
                    var tp = new download_m3u8File_parts_parallel_params_t( mc, m3u8File, ip );                    
                    var downloadParts = download_m3u8File_parts_parallel( tp );

                    //-3-//                    
                    using ( var fs = File.OpenWrite( ip.OutputFileName ) )
                    {
                        fs.SetLength( 0 );

                        foreach ( var downloadPart in downloadParts )
                        {
                            if ( downloadPart.Error != null ) //|| downloadPart.Bytes == null )
                            {
                                res.PartsErrorCount++;
                                continue;
                            }
                            var bytes = downloadPart.Bytes;
                            fs.Write( bytes, 0, bytes.Length );

                            res.PartsSuccessCount++;
                            res.TotalBytes += bytes.Length;
                        }
                    }

                }, ct );

                return (res);
            }
        }
        //-----------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public struct DownloadPartsAndSaveInputParams
        {
            public m3u8_client mc             { get; set; }
            public m3u8_file_t m3u8File       { get; set; }
            public string      OutputFileName { get; set; }

            public CancellationTokenSource    Cts                    { get; set; }
            public RequestStepActionDelegate  RequestStepAction      { get; set; }
            public ResponseStepActionDelegate ResponseStepAction     { get; set; }
            public int                        MaxDegreeOfParallelism { get; set; }
            public bool                       UseCrossAppInstanceDegreeOfParallelism { get; set; }

            public int?   MaxDownloadAppInstance             { get; set; }
            public Action WaitingForOtherAppInstanceFinished { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public struct DownloadPartsAndSaveResult
        {
            internal DownloadPartsAndSaveResult( string outputFileName ) : this()
                => OutputFileName = outputFileName;

            public string OutputFileName   { get; internal set; }

            public int   PartsSuccessCount { get; internal set; }
            public int   PartsErrorCount   { get; internal set; }
            public ulong TotalBytes        { get; internal set; }

            public int TotalParts => (PartsSuccessCount + PartsErrorCount);
            public void ResetOutputFileName( string outputFileName ) => OutputFileName = outputFileName;
            public bool IsEmpty() => ((OutputFileName == null) && (PartsSuccessCount == 0) && (PartsErrorCount == 0) && (TotalBytes == 0UL));
        }

        public static DownloadPartsAndSaveResult DownloadPartsAndSave( DownloadPartsAndSaveInputParams ip )
        {            
            if ( ip.mc == null )                          throw (new m3u8_ArgumentException( nameof(ip.mc) ));
            if ( !ip.m3u8File.Parts.AnyEx() )             throw (new m3u8_ArgumentException( nameof(ip.m3u8File) ));
            if ( ip.OutputFileName.IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(ip.OutputFileName) ));
            //---------------------------------------------------------------------------------------------------------//

            //-1-//
            var res = new DownloadPartsAndSaveResult( ip.OutputFileName );

            using ( var semaphore_app_instance = semaphore_app_instance_t.WaitForOtherAppInstance( ip.MaxDownloadAppInstance, 
                                                                                                   ip.Cts.Token,
                                                                                                   ip.WaitingForOtherAppInstanceFinished,
                                                                                                   delayTimeout: 1000 ) )
            {
                //-2-//
                var tp = new download_m3u8File_parts_parallel_params_t( ip );
                var downloadParts = download_m3u8File_parts_parallel( tp );

                //-3-//
                using ( var fs = File.OpenWrite( ip.OutputFileName ) )
                {
                    fs.SetLength( 0 );

                    foreach ( var downloadPart in downloadParts )
                    {
                        if ( downloadPart.Error != null ) //|| downloadPart.Bytes == null )
                        {
                            res.PartsErrorCount++;
                            continue;
                        }
                        var bytes = downloadPart.Bytes;
                        fs.Write( bytes, 0, bytes.Length );

                        res.PartsSuccessCount++;
                        res.TotalBytes += (uint) bytes.Length;
                    }
                }
            }

            return (res);
        }
        //-----------------------------------------------------------------------------//
    }
}
