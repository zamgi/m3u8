using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal interface i_m3u8_client_next : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public struct init_params
        {
            public HttpCompletionOption? HttpCompletionOption { get; set; }
            public int?                  AttemptRequestCount  { get; set; }
            public bool?                 ConnectionClose      { get; set; }
            public IWebProxy             WebProxy             { get; set; }

            private TimeSpan? _Timeout;
            public TimeSpan Timeout { get => _Timeout.GetValueOrDefault( m3u8_Consts.DEFAULT_TIMEOUT ); set => _Timeout = value; }
        }

        init_params InitParams { get; }
        IWebProxy   WebProxy   { get; }

        /// <summary>
        /// 
        /// </summary>
        public struct ChangeSettingsParams
        {
            public (HttpClient         httpClient , IDisposable disposableObj)? NetHttpClient  { get; set; }
            public (HttpMessageInvoker httpInvoker, IDisposable disposableObj)? NetHttpInvoker { get; set; }
            public IWebProxy WebProxy            { get; set; }
            public int?      AttemptRequestCount { get; set; }
            public TimeSpan? Timeout             { get; set; }
        }
        void ChangeSettings( in ChangeSettingsParams csp );

        Task< m3u8_file_t > DownloadFile( Uri url, IDictionary< string, string > requestHeaders = null, CancellationToken ct = default );

        //------------------------------------------------------------------------------------------//
        /// <summary>
        /// 
        /// </summary>
        public struct DownloadPartStepActionParams
        {
            public DownloadPartStepActionParams( in m3u8_part_ts__v2 part ) => Part = part;
            public m3u8_part_ts__v2 Part        { get; init; }
            public long?   TotalContentLength   { get; internal set; }
            public long    TotalBytesReaded     { get; internal set; }            
            public int     BytesReaded          { get; internal set; }
            public double? InstantSpeedInMbps   { get; internal set; }
            public int     AttemptRequestNumber { get; internal set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public delegate void DownloadPartStepActionDelegate( in DownloadPartStepActionParams ip );

        /// <summary>
        /// 
        /// </summary>
        public struct DownloadPartInputParams
        {
            public I_ThrottlerBySpeed_InDownloadProcessUser ThrottlerBySpeed_User  { [M(O.AggressiveInlining)] get; set; }
            public ObjectPool< byte[] >                     RespBufPool            { [M(O.AggressiveInlining)] get; set; }
            public DownloadPartStepActionDelegate           DownloadPartStepAction { [M(O.AggressiveInlining)] get; set; }

            public I_download_threads_semaphore             DownloadThreadsSemaphore { [M(O.AggressiveInlining)] get; set; }
        
            public ManualResetEventSlim                     WaitIfPausedEvent        { [M(O.AggressiveInlining)] get; set; }
            public Action< m3u8_part_ts__v2 >               WaitingIfPausedBefore    { [M(O.AggressiveInlining)] get; set; }
            public Action< m3u8_part_ts__v2 >               WaitingIfPausedAfter     { [M(O.AggressiveInlining)] get; set; }

            public ManualResetEventSlim                     WasSettedWaitIfPausedEvent  { [M(O.AggressiveInlining)] get; set; }

            public string OutputFileName { [M(O.AggressiveInlining)] get; set; }
            public override string ToString() => OutputFileName;
        }

        Task< m3u8_part_ts__v2 > DownloadPart( m3u8_part_ts__v2 part, Uri baseAddress
            , IDictionary< string, string > requestHeaders, DownloadPartInputParams ip, CancellationToken ct = default );

        Task< m3u8_part_ts__v2 > DownloadPart__v2( m3u8_part_ts__v2 part, Uri baseAddress
            , IDictionary< string, string > requestHeaders, DownloadPartInputParams ip, CancellationToken commonToken, CancellationTokenSourceWraper waitIfPausedEventTokenSourceWraper );
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class CancellationTokenSourceWraper : IDisposable
    {
        private CancellationTokenSource _Cts;
        public CancellationTokenSourceWraper() => _Cts = new CancellationTokenSource();
        public void Dispose() => _Cts.Dispose();

        public void Cancel() => _Cts.Cancel();
        public void Reset()
        {
            //var suc = _Cts.TryReset();
            //if ( !suc ) { _Cts.Dispose(); _Cts = new CancellationTokenSource(); }
            _Cts.Dispose(); 
            _Cts = new CancellationTokenSource();
        }
        public CancellationToken Token => _Cts.Token;
        public bool IsCancellationRequested => _Cts.IsCancellationRequested;

        public override string ToString() => _Cts.ToString();
    }
}
