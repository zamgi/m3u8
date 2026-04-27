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
            public const bool      DEFAULT_CONNECTIONCLOSE    = true;
            public const int       DEFAULT_TIMEOUT_IN_SECONDS = 100;
            public static TimeSpan DEFAULT_TIMEOUT            => TimeSpan.FromSeconds( DEFAULT_TIMEOUT_IN_SECONDS );

            public HttpCompletionOption? HttpCompletionOption { get; set; }
            public int?                  AttemptRequestCount  { get; set; }
            public bool?                 ConnectionClose      { get; set; }
            public IWebProxy             WebProxy             { get; set; }

            private TimeSpan? _Timeout;
            public TimeSpan Timeout { get => _Timeout.GetValueOrDefault( DEFAULT_TIMEOUT ); set => _Timeout = value; }
        }


        init_params InitParams { get; }
        IWebProxy   WebProxy   { get; }

        Task< m3u8_file_t > DownloadFile( Uri url, CancellationToken ct = default, IDictionary< string, string > requestHeaders = null );

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
        }

        Task< m3u8_part_ts__v2 > DownloadPart( m3u8_part_ts__v2 part, Uri baseAddress
            , DownloadPartInputParams ip, CancellationToken ct = default, IDictionary< string, string > requestHeaders = null );
    }
}
