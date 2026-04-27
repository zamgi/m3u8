using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    public interface i_m3u8_client : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public struct init_params
        {
            public const bool      DEFAULT_CONNECTIONCLOSE    = true;
            public const int       DEFAULT_TIMEOUT_IN_SECONDS = 100;
            public static TimeSpan DEFAULT_TIMEOUT            => TimeSpan.FromSeconds( DEFAULT_TIMEOUT_IN_SECONDS );

            public int?  AttemptRequestCount { get; set; }
            public bool? ConnectionClose     { get; set; }
            public HttpCompletionOption? HttpCompletionOption { get; set; }
            public IWebProxy WebProxy { get; set; }

            private TimeSpan? _Timeout;
            public TimeSpan Timeout { get => _Timeout.GetValueOrDefault( DEFAULT_TIMEOUT ); set => _Timeout = value; }
        }

        public init_params InitParams { get; }
        public IWebProxy   WebProxy   { get; }

        Task< m3u8_file_t > DownloadFile( Uri url, CancellationToken ct = default, IDictionary< string, string > requestHeaders = null );
        Task< m3u8_part_ts > DownloadPart( m3u8_part_ts part, Uri baseAddress, CancellationToken ct = default );

        Task< m3u8_part_ts__v2 > DownloadPart__v2( m3u8_part_ts__v2 part, Uri baseAddress, CancellationToken ct = default );
    }
}
