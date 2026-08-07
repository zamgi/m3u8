using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
#if !(NETCOREAPP)
using System.Security.Authentication;
#endif
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using m3u8.client__v2;
using m3u8.helpers;
using m3u8.infrastructure;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// 
        /// </summary>
        private static class v1
        {
            public static async Task run( string m3u8FileUrl, string outputFileName, CancellationToken ct )
            {
                var p = new m3u8_processor__v0.DownloadFileAndSaveInputParams()
                {
                    CancellationToken  = ct,
                    m3u8FileUrl        = m3u8FileUrl,
                    OutputFileName     = outputFileName,
                    NetParams          = new i_m3u8_client.init_params() { AttemptRequestCount = 1, HttpCompletionOption = HttpCompletionOption.ResponseHeadersRead },
                    ResponseStepAction = new m3u8_processor__v0.ResponseStepActionDelegate( t => ConsoleHelper.WriteLine( $"{t.Part.OrderNumber + 1} of {t.TotalPartCount}, '{t.Part.RelativeUrlName}'" ) ),
                    //MaxDegreeOfParallelism = 8,
                };

                await m3u8_processor__v0.DownloadFileAndSave( p ).CAX();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static class v2
        {
            /// <summary>
            /// 
            /// </summary>
            private sealed class download_threads_semaphore_impl : i_download_threads_semaphore
            {
                private SemaphoreSlim _Semaphore;
                private int _MaxDegreeOfParallelism;
                public download_threads_semaphore_impl( int maxDegreeOfParallelism ) 
                    => (_Semaphore, _MaxDegreeOfParallelism) = (new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism ), maxDegreeOfParallelism);
                public void Dispose() => _Semaphore.Dispose();
                public bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance => false;

                public int MaxCount => _MaxDegreeOfParallelism;
                public int CurrentCount => _Semaphore.CurrentCount;

                public bool Release() { _Semaphore.Release(); return (true); }
                public bool Release_NoThrow() { try { return (Release()); } catch ( SemaphoreFullException ex ) { Debug.WriteLine( ex ); return (false); } }
                public void Wait( CancellationToken ct ) => _Semaphore.Wait( ct );
                public Task WaitAsync( CancellationToken ct ) => _Semaphore.WaitAsync( ct );
            }
#if THROTTLER__V1
            /// <summary>
            /// 
            /// </summary>
            private sealed class throttler_by_speed_impl__v1 : i_throttler_by_speed__v1_t
            {
                public void ChangeMaxSpeedThreshold( decimal? max_speed_threshold_in_Mbps ) { }
                public void Dispose() { }
                public void End( Task task ) { }
                public decimal? GetMaxSpeedThreshold() => null;
                public void Restart( Task task ) { }
                public void Start( Task task ) { }
                public void TakeIntoAccountDownloadedBytes( Task task, int downloadedBytes ) { }
                public double? Throttle( Task task, CancellationToken ct ) => null;
            }
#endif
#if THROTTLER__V2
            /// <summary>
            /// 
            /// </summary>
            private sealed class throttler_by_speed_impl__v2 : i_throttler_by_speed__v2_t
            {
                public void ChangeMaxSpeedThreshold( decimal? max_speed_threshold_in_Mbps ) { }
                public void Dispose() { }
                public void End() { }
                public decimal? GetMaxSpeedThreshold() => null;
                public void Restart() { }
                public void Start() { }
                public void TakeIntoAccountDownloadedBytes( int downloadedBytes ) { }
                public double? Throttle( CancellationToken ct ) => null;
            }
#endif
            public static async Task run( 
                  string m3u8FileUrl
                , string outputFileName
                , CancellationToken ct
                , IWebProxy webProxy = null
                , IDictionary< string, string > requestHeaders = null )
            {
                var m3u8_client_factory = m3u8_client_factory_maker.get( m3u8_client_factory_enum_type.HttpClient );
                var ip = new i_m3u8_client.init_params() 
                { 
                    AttemptRequestCount = 1, 
                    HttpCompletionOption = HttpCompletionOption.ResponseHeadersRead,
                    WebProxy = webProxy,
                };
                using var mc = m3u8_client_factory.Create( ip );

                var m3u8File = await mc.DownloadFile( new Uri( m3u8FileUrl ), requestHeaders, ct ).CAX();

                var maxDegreeOfParallelism = 8;
                var streamInPoolCapacity   = 1_024 * 1_024 * 5;
                var bufInPoolCapacity      = 1_024 * 100;
                using var waitIfPausedEventWrapper = new WaitIfPausedEventWrapper();
                using var dts                = new download_threads_semaphore_impl( maxDegreeOfParallelism );
                using var dts_4_Parts        = new download_threads_semaphore_impl( maxDegreeOfParallelism );
                using var throttler_by_speed = new throttler_by_speed_impl__v2();
                using var streamPool         = new ObjectPoolDisposable< Stream >( maxDegreeOfParallelism, () => new MemoryStream( streamInPoolCapacity ) );
                using var respBufPool        = new ObjectPool< byte[] >( maxDegreeOfParallelism, () => new byte[ bufInPoolCapacity ] );
                using var timeoutCtsPool     = new CtsTimerPool( maxDegreeOfParallelism );

                #region comm.
                //var requestStepAction      = new m3u8_processor.RequestStepActionDelegate( (in m3u8_processor.RequestStepActionParams p) =>
                //{
                //    var requestText = $"#{p.PartOrderNumber} of {p.TotalPartCount}). '{p.Part.RelativeUrlName}'...";
                //    if ( p.Success )
                //    {
                //        var logRow = row.Log.AddRequestRow( requestText, responseText: "/starting/..." );
                //        rows_Dict.Add( p.Part.OrderNumber, logRow );
                //    }
                //    else
                //    {
                //        anyErrorHappend = true;
                //        row.Log.AddResponseErrorRow( requestText, p.Error.ToString() );
                //    }
                //});
                #endregion
                var responseStepAction = new m3u8_processor.ResponseStepActionDelegate( (in m3u8_processor.ResponseStepActionParams p) => ConsoleHelper.WriteLine( $"{p.Part.OrderNumber + 1} of {p.TotalPartCount}, '{p.Part.RelativeUrlName}'" ) );
                //var downloadPartStepAction = new m3u8_client.DownloadPartStepActionDelegate( (in m3u8_client.DownloadPartStepActionParams p) => );
                var waitIfPausedHolder = new WaitIfPausedHolder( waitIfPausedEventWrapper );

                var p = new m3u8_processor.DownloadPartsAndSaveInputParams()
                {
                    mc                               = mc,
                    m3u8File                         = m3u8File,
                    OutputFileName                   = outputFileName,
                    CancellationToken                = ct,
                    //RequestStepAction                = requestStepAction,
                    ResponseStepAction               = responseStepAction,
                    //DownloadPartStepAction           = downloadPartStepAction,
                    MaxDegreeOfParallelism           = maxDegreeOfParallelism,
                    DownloadThreadsSemaphore         = dts,
                    DownloadThreadsSemaphore_4_Parts = dts_4_Parts,
                    WaitIfPausedHolder               = waitIfPausedHolder,
                    WaitIfPausedHolder_4_Parts       = waitIfPausedHolder,
                    ThrottlerBySpeed                 = throttler_by_speed,
                    StreamPool                       = streamPool,
                    RespBufPool                      = respBufPool,
                    TimeoutCtsPool                   = timeoutCtsPool,
                };

                await m3u8_processor.DownloadPartsAndSave( p, requestHeaders ).CAX();
            }

            private static HttpClient CreateHttpClient( IWebProxy webProxy, in TimeSpan? timeout = null )
            {
#if NETCOREAPP
                /*SocketsHttpHandler CreateSocketsHttpHandler( in TimeSpan? _timeout )
                {
                    static void set_Protocol( SslClientAuthenticationOptions sslOptions, SslProtocols protocol )
                    {
                        try
                        {
                            sslOptions.EnabledSslProtocols |= protocol;
                        }
                        catch ( Exception ex )
                        {
                            Debug.WriteLine( ex );
                        }
                    }

                    var h = new SocketsHttpHandler() 
                    { 
                        AutomaticDecompression = DecompressionMethods.All, 
                        Proxy = webProxy 
                    };
                    h.SslOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    //set_Protocol( h.SslOptions, SslProtocols.Tls   );
                    //set_Protocol( h.SslOptions, SslProtocols.Tls11 );
                    set_Protocol( h.SslOptions, SslProtocols.Tls12 );
                    set_Protocol( h.SslOptions, SslProtocols.Tls13 );
#pragma warning disable CS0618
                    set_Protocol( h.SslOptions, SslProtocols.Ssl2 );
                    set_Protocol( h.SslOptions, SslProtocols.Ssl3 );
#pragma warning restore CS0618

                    if ( _timeout.HasValue )
                    {
                        h.ConnectTimeout = _timeout.Value;
                    }
                    return (h);
                }
                //*/

                var handler = new HttpClientHandler() 
                { 
                    AutomaticDecompression = DecompressionMethods.All, 
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                    Proxy = webProxy,
                };
                //var handler = CreateSocketsHttpHandler( timeout );
                var httpClient = new HttpClient( handler, true );
#else
            HttpClientHandler CreateHttpClientHandler( /*in TimeSpan? _timeout*/ )
            {
                static void set_Protocol( HttpClientHandler h, SslProtocols protocol )
                {
                    try
                    {
                        h.SslProtocols |= protocol;
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                    }
                }

                var h = new HttpClientHandler() 
                { 
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true, 
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip 
                };

                //set_Protocol( h, SslProtocols.Tls   );
                //set_Protocol( h, SslProtocols.Tls11 );
                set_Protocol( h, SslProtocols.Tls12 );
                set_Protocol( h, SslProtocols.Tls13 );
#pragma warning disable CS0618
                set_Protocol( h, SslProtocols.Ssl2 );
                set_Protocol( h, SslProtocols.Ssl3 );
#pragma warning restore CS0618
                //if ( _timeout.HasValue )
                //{
                //    h.ConnectTimeout = _timeout.Value;
                //}
                return (h);
            }

            var handler    = CreateHttpClientHandler( /*timeout*/ );
            var httpClient = new HttpClient( handler, true );
#endif
                if ( timeout.HasValue ) httpClient.Timeout = timeout.Value;
                return (httpClient);
            }
            private static HttpMessageInvoker CreateHttpInvoker( IWebProxy webProxy )
            {
#if NETCOREAPP
                /*SocketsHttpHandler CreateSocketsHttpHandler()
                {
                    static void set_Protocol( SslClientAuthenticationOptions sslOptions, SslProtocols protocol )
                    {
                        try
                        {
                            sslOptions.EnabledSslProtocols |= protocol;
                        }
                        catch ( Exception ex )
                        {
                            Debug.WriteLine( ex );
                        }
                    }

                    var h = new SocketsHttpHandler() 
                    { 
                        AutomaticDecompression = DecompressionMethods.All, 
                        Proxy = webProxy 
                    };
                    h.SslOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    //set_Protocol( h.SslOptions, SslProtocols.Tls   );
                    //set_Protocol( h.SslOptions, SslProtocols.Tls11 );
                    set_Protocol( h.SslOptions, SslProtocols.Tls12 );
                    set_Protocol( h.SslOptions, SslProtocols.Tls13 );
#pragma warning disable CS0618
                    set_Protocol( h.SslOptions, SslProtocols.Ssl2 );
                    set_Protocol( h.SslOptions, SslProtocols.Ssl3 );
#pragma warning restore CS0618
                    return (h);
                }
                //*/
                //var handler = CreateSocketsHttpHandler( timeout );

                var handler = new HttpClientHandler() 
                { 
                    AutomaticDecompression = DecompressionMethods.All, 
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                    Proxy = webProxy,
                };                
                var httpInvoker = new HttpMessageInvoker( handler, true );
#else
                HttpClientHandler CreateHandler()
                {
                    static void set_Protocol( HttpClientHandler h, SslProtocols protocol )
                    {
                        try
                        {
                            h.SslProtocols |= protocol;
                        }
                        catch ( Exception ex )
                        {
                            Debug.WriteLine( ex );
                        }
                    }

                    var h = new HttpClientHandler() 
                    { 
                        ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true, 
                        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip 
                    };

                    //set_Protocol( h, SslProtocols.Tls   );
                    //set_Protocol( h, SslProtocols.Tls11 );
                    set_Protocol( h, SslProtocols.Tls12 );
                    set_Protocol( h, SslProtocols.Tls13 );
#pragma warning disable CS0618
                    set_Protocol( h, SslProtocols.Ssl2 );
                    set_Protocol( h, SslProtocols.Ssl3 );
#pragma warning restore CS0618
                    //if ( _timeout.HasValue )
                    //{
                    //    h.ConnectTimeout = _timeout.Value;
                    //}
                    return (h);
                }

                var handler     = CreateHandler( /*timeout*/ );
                var httpInvoker = new HttpMessageInvoker( handler, true );
#endif
                return (httpInvoker);
            }
            public static async Task run_over_proxy( string m3u8FileUrl, string outputFileName, CancellationToken ct, IDictionary< string, string > requestHeaders = null )
            {
                /*
                var proxySettings = ProxySettings.Parse( "127.0.0.1:9150" );
                var handler = new ProxyClientHandler< Socks5 >( proxySettings, useCookie: false, allowAutoRedirect: true, acceptAnyServerCertificate: true );
                using var hc = new HttpClient( handler, true );
                var maxDegreeOfParallelism = 1;
                //*/

                //*
                var torWebProxy = new WebProxy() { Address = new Uri( "socks5://127.0.0.1:9150" ) };
                using var httpInvoker = CreateHttpInvoker( torWebProxy );
                var maxDegreeOfParallelism = 8;
                //*/

                using var mc = new m3u8_client__with_HttpInvoker( httpInvoker, new i_m3u8_client.init_params() { AttemptRequestCount = 1, HttpCompletionOption = HttpCompletionOption.ResponseHeadersRead } );

                var m3u8File = await mc.DownloadFile( new Uri( m3u8FileUrl ), requestHeaders, ct ).CAX();
               
                const int streamInPoolCapacity     = 1_024 * 1_024 * 5;
                const int bufInPoolCapacity        = 1_024 * 100;
                using var waitIfPausedEventWrapper = new WaitIfPausedEventWrapper();
                using var dts                      = new download_threads_semaphore_impl( maxDegreeOfParallelism );
                using var dts_4_Parts              = new download_threads_semaphore_impl( maxDegreeOfParallelism );
                using var throttler_by_speed       = new throttler_by_speed_impl__v2();
                using var streamPool               = new ObjectPoolDisposable< Stream >( maxDegreeOfParallelism, () => new MemoryStream( streamInPoolCapacity ) );
                using var respBufPool              = new ObjectPool< byte[] >( maxDegreeOfParallelism, () => new byte[ bufInPoolCapacity ] );
                using var timeoutCtsPool           = new CtsTimerPool( maxDegreeOfParallelism );

                var responseStepAction = new m3u8_processor.ResponseStepActionDelegate( (in m3u8_processor.ResponseStepActionParams p) => ConsoleHelper.WriteLine( $"{p.Part.OrderNumber + 1} of {p.TotalPartCount}, '{p.Part.RelativeUrlName}'" ) );
                var waitIfPausedHolder = new WaitIfPausedHolder( waitIfPausedEventWrapper );

                var p = new m3u8_processor.DownloadPartsAndSaveInputParams()
                {
                    mc                               = mc,
                    m3u8File                         = m3u8File,
                    OutputFileName                   = outputFileName,
                    CancellationToken                = ct,
                    ResponseStepAction               = responseStepAction,
                    MaxDegreeOfParallelism           = maxDegreeOfParallelism,
                    DownloadThreadsSemaphore         = dts,
                    DownloadThreadsSemaphore_4_Parts = dts_4_Parts,
                    WaitIfPausedHolder               = waitIfPausedHolder,
                    WaitIfPausedHolder_4_Parts       = waitIfPausedHolder,
                    ThrottlerBySpeed                 = throttler_by_speed,
                    StreamPool                       = streamPool,
                    RespBufPool                      = respBufPool,
                    TimeoutCtsPool                   = timeoutCtsPool,
                };

                await m3u8_processor.DownloadPartsAndSave( p, requestHeaders ).CAX();
            }
        }

        private static string to_text_format( int size ) => to_text_format( (ulong) size );
        private static string to_text_format( ulong size ) => (0 < size) ? size.ToString("0,0") : "0";

        [STAThread] private static async Task Main( string[] args )
        {
            try
            {
#if NETCOREAPP
                Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );
#endif
#if !(NETCOREAPP)
                #region [.set SecurityProtocol to 'Tls + Tls11 + Tls12 + Ssl3'.]
                ServicePointManager.SecurityProtocol = (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Ssl3);
                #endregion
#endif
                await Test__obj_pool();

                //await Run_1().CAX();
                //---await Run_2().CAX();                
            }
            catch ( Exception ex )
            {
                ConsoleHelper.WriteLineError( $"ERROR: {ex}" );
            }
            ConsoleHelper.WriteLine( "\r\n\r\n[.....finita fusking comedy.....]\r\n\r\n", ConsoleColor.DarkGray );
            ConsoleHelper.ReadLine();
        }

        private static async Task Run_1()
        {
            var M3U8_FILE_URL   = ConfigurationManager.AppSettings[ "M3U8_FILE_URL"   ]; if ( M3U8_FILE_URL  .IsNullOrWhiteSpace() ) throw (new ArgumentNullException( nameof(M3U8_FILE_URL) ));
            var OUTPUT_FILE_DIR = ConfigurationManager.AppSettings[ "OUTPUT_FILE_DIR" ]; if ( OUTPUT_FILE_DIR.IsNullOrWhiteSpace() ) OUTPUT_FILE_DIR = @"E:\\";
            var OUTPUT_FILE_EXT = ConfigurationManager.AppSettings[ "OUTPUT_FILE_EXT" ]; if ( OUTPUT_FILE_EXT.IsNullOrWhiteSpace() ) OUTPUT_FILE_EXT = ".avi";

            //v1.run( M3U8_FILE_URL, OUTPUT_FILE_DIR, OUTPUT_FILE_EXT );
            //v2.run__1( M3U8_FILE_URL, OUTPUT_FILE_DIR, OUTPUT_FILE_EXT );
            //v2.run__2( M3U8_FILE_URL, OUTPUT_FILE_DIR, OUTPUT_FILE_EXT );
            //await v3.run( M3U8_FILE_URL, OUTPUT_FILE_DIR, default ).CAX();
            //await v4.run( M3U8_FILE_URL, OUTPUT_FILE_DIR, default ).CAX();

            var requestHeaders = new Dictionary< string, string >
            {
                //{ "Accept", "*/*" },
                //{ "Accept-Encoding", "gzip, deflate, br" },
                //{ "Accept-Language", "ru,en-US;q=0.9,en;q=0.8" },
                    
                //{ "Cache-Control", "no-cache" },
                //{ "Pragma", "no-cache" },
                //{ "Connection", "keep-alive" },
                //{ "Host", "09b-8c6-300g0.v.plground.live:10403" },
                { "Origin" , "https://ollo-as.newplayjj.com:9443"  },
                //{ "Referer", "https://ollo-as.newplayjj.com:9443/" },
                //{ "Sec-Fetch-Dest", "empty" },
                //{ "Sec-Fetch-Mode", "cors" },
                //{ "Sec-Fetch-Site", "cross-site" },
                //{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36" },
                    
                //{ "sec-ch-ua", "\"Not A(Brand\";v=\"99\", \"Google Chrome\";v=\"121\", \"Chromium\";v=\"121\"" },
                //{ "sec-ch-ua-mobile", "?0" },
                //{ "sec-ch-ua-platform", "\"Windows\"" }
            };

            using ( var cts = new CancellationTokenSource() )
            {
                var outputFileName = Path.Combine( OUTPUT_FILE_DIR, PathnameCleaner.CleanPathnameAndFilename( M3U8_FILE_URL ).TrimStart( '-' ) + OUTPUT_FILE_EXT );
                await v2.run( M3U8_FILE_URL, outputFileName, cts.Token, requestHeaders: requestHeaders ).CAX(); //.WaitForTaskEndsOrKeyboardBreak( cts );
            }
        }
        private static async Task Run_2()
        {
            var M3U8_FILE_URL =
"https://river-m9-mts-393.rtbcdn.ru/hls-vod/JmgwaYl7ElRZi2t8OUcQHg/1784397839/3494/0x5000c500e970ee66/003741a69a0d4295977cb74538f5a1b8.mp4.m3u8?i=640x360_532"
;
            var OUTPUT_FILE_DIR = ConfigurationManager.AppSettings[ "OUTPUT_FILE_DIR" ]; if ( OUTPUT_FILE_DIR.IsNullOrWhiteSpace() ) OUTPUT_FILE_DIR = @"E:\\";
            var OUTPUT_FILE_EXT = ConfigurationManager.AppSettings[ "OUTPUT_FILE_EXT" ]; if ( OUTPUT_FILE_EXT.IsNullOrWhiteSpace() ) OUTPUT_FILE_EXT = ".avi";

            //[{\"name\":\"Accept\",\"value\":\"*\\/*\"},{\"name\":\"Accept-Encoding\",\"value\":\"gzip, deflate, br, zstd\"},{\"name\":\"Accept-Language\",\"value\":\"ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7\"},{\"name\":\"Origin\",\"value\":\"https:\\/\\/rutube.ru\"},{\"name\":\"Referer\",\"value\":\"https:\\/\\/rutube.ru\\/\"},{\"name\":\"sec-ch-ua\",\"value\":\"\\\"Google Chrome\\\";v=\\\"147\\\", \\\"Not.A\\/Brand\\\";v=\\\"8\\\", \\\"Chromium\\\";v=\\\"147\\\"\"},{\"name\":\"sec-ch-ua-mobile\",\"value\":\"?0\"},{\"name\":\"sec-ch-ua-platform\",\"value\":\"\\\"Windows\\\"\"},{\"name\":\"Sec-Fetch-Dest\",\"value\":\"empty\"},{\"name\":\"Sec-Fetch-Mode\",\"value\":\"cors\"},{\"name\":\"Sec-Fetch-Site\",\"value\":\"cross-site\"},{\"name\":\"User-Agent\",\"value\":\"Mozilla\\/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit\\/537.36 (KHTML, like Gecko) Chrome\\/147.0.0.0 Safari\\/537.36\"}]
            var requestHeaders = new Dictionary< string, string >
            {
                //{"Accept","*/*"},
                //{"Accept-Encoding","gzip, deflate, br, zstd"},
                //{"Accept-Language","ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7"},
                //{"Origin","https://ladoni.pro"},
                //{"Referer","https://ladoni.pro/lat/20683?skips=1&adult_mode=2"},
                //{"sec-ch-ua","\"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"144\", \"Google Chrome\";v=\"144\""},
                //{"sec-ch-ua-mobile","?0"},
                //{"sec-ch-ua-platform","\"Windows\""},
                //{"Sec-Fetch-Dest","empty"},
                //{"Sec-Fetch-Mode","cors"},
                //{"Sec-Fetch-Site","same-site"},
                //{"User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36"}
            };

            IWebProxy torWebProxy = null; //new WebProxy() { Address = new Uri( "socks5://127.0.0.1:9150" ) };

            using ( var cts = new CancellationTokenSource() )
            {
                var outputFileName = Path.Combine( OUTPUT_FILE_DIR, PathnameCleaner.CleanPathnameAndFilename( M3U8_FILE_URL ).TrimStart( '-' ) + OUTPUT_FILE_EXT );
                //---await next1.run_over_proxy( M3U8_FILE_URL, outputFileName, cts.Token, requestHeaders ).CAX();
                await v2.run( M3U8_FILE_URL, outputFileName, cts.Token, torWebProxy, requestHeaders ).CAX();
            }
        }

        private static async Task Test__obj_pool()
        {
            using var pool = new CtsTimerPool( 1 );

            await SendAsync_Ex( pool, TimeSpan.FromSeconds( 10 ), CancellationToken.None ).CAX();
            await SendAsync_Ex( pool, TimeSpan.FromSeconds( 10 ), CancellationToken.None ).CAX();
            await SendAsync_Ex( pool, TimeSpan.FromSeconds( 10 ), CancellationToken.None ).CAX();
        }
        private static async Task SendAsync_Ex( CtsTimerPool timeoutCtsPool, TimeSpan timeout, CancellationToken ct )
        {
#if NETCOREAPP
            using var h = timeoutCtsPool.Acquire( timeout, out var timeout_cts );
            Debug.Assert( !timeout_cts.IsCancellationRequested );
#else
            using var timeout_cts = new CancellationTokenSource( timeout ); 
#endif
            using var union_cts = CancellationTokenSource.CreateLinkedTokenSource( timeout_cts.Token, ct );
            try
            {
                //var resp = await httpInvoker.SendAsync( req, union_cts.Token ).ConfigureAwait( false );
                //return (resp);

                await Task.Delay( TimeSpan.FromSeconds(1)/*Timeout.Infinite*/, union_cts.Token ).CAX();
            }
            catch ( Exception /*ex*/ ) when (ct.IsCancellationRequested)
            {
                //---throw (new OperationCanceledException( $"Http request was canceled.", ex ));
                Debug.WriteLine( $"Http request was canceled." );
            }
            catch ( Exception /*ex*/ ) when (timeout_cts.IsCancellationRequested)
            {
                //---throw (new TimeoutException( $"Http request timeout exceeded: {timeout}.", ex ));
                Debug.WriteLine( $"Http request timeout exceeded: {timeout}." );
            }
        }
    }
    

    /// <summary>
    /// 
    /// </summary>
    internal static class PathnameCleaner
    {
        private static HashSet< char > _InvalidFileNameChars;
        private static HashSet< char > _InvalidPathChars;

        static PathnameCleaner()
        {
            _InvalidFileNameChars = new HashSet< char >( Path.GetInvalidFileNameChars() );
            _InvalidPathChars     = new HashSet< char >( Path.GetInvalidPathChars    () );
        }

        public static string CleanFilename( string filename )
        {
            if ( filename != null )
            {
                filename = new string( (from ch in filename
                                        where (!_InvalidFileNameChars.Contains( ch ))
                                        select ch
                                       ).ToArray()
                                     );
            }
            return (filename);
        }
        public static string CleanPathname( string pathname )
        {
            if ( pathname != null )
            {
                pathname = new string( (from ch in pathname
                                        where (!_InvalidPathChars.Contains( ch ))
                                        select ch
                                       ).ToArray() 
                                     );
            }
            return (pathname);
        }

        public static string CleanPathnameAndFilename( string pathnameAndFilename
            , string replacedPathChar = "--"
            , char   replacedNameChar = '-'
            , int    maxLen           = 75 )
        {            
            if ( pathnameAndFilename != null )
            {
                var sb = new StringBuilder( pathnameAndFilename.Length + 10 );
                for ( var i = 0; i < pathnameAndFilename.Length; i++ )
                {
                    var ch = pathnameAndFilename[ i ];
                    if ( _InvalidPathChars.Contains( ch ) )
                    {
                        sb.Append( replacedPathChar );
                    }
                    else if ( _InvalidFileNameChars.Contains( ch ) )
                    {
                        switch ( ch )
                        {
                            case '/':
                            case '\\':
                                sb.Append( replacedPathChar );
                            break;

                            default:
                                sb.Append( replacedNameChar );
                            break;
                        }                        
                    }
                    else
                    {
                        sb.Append( ch );
                    }
                }
                pathnameAndFilename = (maxLen < sb.Length) ? sb.ToString( 0, maxLen ) : sb.ToString();
            }
            return (pathnameAndFilename);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class ConsoleHelper
    {
        public static void WriteLine( string text, ConsoleColor? foregroundColor = null )
        {
            lock ( typeof(ConsoleHelper) )
            {
                if ( foregroundColor.HasValue )
                {
                    var fc = Console.ForegroundColor;
                    Console.ForegroundColor = foregroundColor.Value;
                    Console.WriteLine( text );
                    Console.ForegroundColor = fc;
                }
                else
                {
                    Console.WriteLine( text );
                }
            }
        }
        public static void WriteLineError( string text )
        {
            lock ( typeof(ConsoleHelper) )
            {
                var fc = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( Environment.NewLine + text );
                Console.ForegroundColor = fc;
            }
        }

        public static string ReadLine() => Console.ReadLine();
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class _Extensions
    {
        public static void WaitForTaskEndsOrKeyboardBreak( this Task task, CancellationTokenSource cts )
        {
            const int TASK_WAIT_MILLISECONDS_TIMEOUT = 100;

            for ( ; !task.Wait( TASK_WAIT_MILLISECONDS_TIMEOUT ); )
            {
                if ( Console.KeyAvailable )
                {
                    var keyInfo = Console.ReadKey( true );
                    switch ( keyInfo.Key )
                    {
                        //case ConsoleKey.Enter:
                        case ConsoleKey.Escape:
                            cts.Cancel();
                            break;
                    }
                }
            }
        }
    }
}
