using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
#if NETCOREAPP
using System.Net.Security;
#endif
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

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
    public struct GetStreamAsync_Ex_Result : IDisposable
    {
        private HttpResponseMessage _Resp;
        private Stream _Stream;
        public GetStreamAsync_Ex_Result( HttpResponseMessage resp, Stream stream )
        {
            _Resp = resp;
            _Stream = stream;
        }
        public void Dispose()
        {
            _Resp.Dispose();
            _Stream.Dispose();
        }
        public Stream Stream => _Stream;
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //internal interface i_m3u8_live_stream_downloader
    //{
    //    string M3u8Url        { get; }
    //    string OutputFileName { get; }
    //    Task Download( CancellationToken ct, long? max_output_file_size, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 );
    //    Task Download( CancellationToken ct, Func< long > get_max_output_file_size_func, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 );
    //}

    /// <summary>
    /// 
    /// </summary>
    internal abstract class m3u8_live_stream_downloader_base< TInvoker > : /*i_m3u8_live_stream_downloader,*/ IDisposable where TInvoker : HttpMessageInvoker
    {
        public delegate void DownloadContentDelegate( string part_url );
        public delegate void DownloadContentErrorDelegate( string m3u8_url, Exception ex );        
        public delegate void DownloadPartDelegate( string part_url, long part_size_in_bytes, long total_in_bytes, double? instantSpeedInMbps );
        public delegate void DownloadPartErrorDelegate( string part_url, Exception ex );
        public delegate void DownloadCreateOutputFileDelegate( string output_file_name );

        /// <summary>
        /// 
        /// </summary>
        public struct InitParams
        {
            public string M3u8Url        { get; set; }
            public string OutputFileName { get; set; }

            public TInvoker HttpInvoker { get; set; }
            
            public ManualResetEventSlim       WaitIfPausedEvent { [M(O.AggressiveInlining)] get; set; }
            public Action                     WaitingIfPaused   { [M(O.AggressiveInlining)] get; set; }
            public I_throttler_by_speed__v2_t ThrottlerBySpeed  { [M(O.AggressiveInlining)] get; set; }

            public DownloadContentDelegate          DownloadContent          { get; set; }
            public DownloadContentErrorDelegate     DownloadContentError     { get; set; }
            public DownloadPartDelegate             DownloadPart             { get; set; }
            public DownloadPartErrorDelegate        DownloadPartError        { get; set; }
            public DownloadCreateOutputFileDelegate DownloadCreateOutputFile { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private enum PartUrlStatusEnum
        {
            Queueed,
            Processed,
        }

        #region [.ctor().]
        protected TInvoker _HttpInvoker;
        private bool       _Dispose_HttpInvoker;
        private InitParams _IP;
        private BlockingCollection< string > _PartUrls;
        private Dictionary< string, PartUrlStatusEnum > _PartUrlsStatus;
        private I_ThrottlerBySpeed_InDownloadProcessUser _ThrottlerBySpeed_User;

        protected m3u8_live_stream_downloader_base( in InitParams ip )
        {
            M3u8Url        = ip.M3u8Url        ?? throw (new ArgumentNullException( nameof(ip.M3u8Url) ));
            OutputFileName = ip.OutputFileName ?? throw (new ArgumentNullException( nameof(ip.OutputFileName) ));

            _IP = ip;

            _HttpInvoker         = ip.HttpInvoker ?? CreateHttpInvoker();
            _Dispose_HttpInvoker = (ip.HttpInvoker == null);

            _PartUrls           = new BlockingCollection< string >();
            _PartUrlsStatus     = new Dictionary< string, PartUrlStatusEnum >();
            _ThrottlerBySpeed_User = ThrottlerBySpeed_InDownloadProcessUser.Start( ip.ThrottlerBySpeed );
        }
        public void Dispose()
        {
            if ( _Dispose_HttpInvoker )
            {
                _HttpInvoker.Dispose();
            }
            _ThrottlerBySpeed_User.Dispose();
        }
        #endregion

        public string M3u8Url        { get; }
        public string OutputFileName { get; }

        public async Task Download( CancellationToken ct, long? max_output_file_size, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 )
        {
            var m3u8_url         = _IP.M3u8Url;
            var output_file_name = _IP.OutputFileName;

            var i = m3u8_url.LastIndexOf( '/' );
            var base_url = (i != -1) ? m3u8_url.Substring( 0, i + 1 ) : m3u8_url;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var task_DownloadContent = RunDownloadContent( m3u8_url, requestHeaders, ct, milliseconds_delay_between_request );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task task_DownloadParts;
            if ( max_output_file_size.HasValue )
            {
                task_DownloadParts = RunDownloadParts_LimitedFileSize( output_file_name, base_url, requestHeaders, ct, max_output_file_size.Value );
            }
            else
            {
                task_DownloadParts = RunDownloadParts_NoLimitedFileSize( output_file_name, base_url, requestHeaders, ct );
            }            
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            //---await Task.Delay( Timeout.Infinite, ct ).CAX();
            var task = await Task.WhenAny( task_DownloadContent, task_DownloadParts ).CAX();
            if ( ct.IsCancellationRequested )
            {
                Task.WaitAll( task_DownloadContent, task_DownloadParts );
            }
            task.GetAwaiter().GetResult();
            //---await task.ContinueWith( t => throw t.Exception, TaskContinuationOptions.OnlyOnFaulted ).CAX();
        }
        public async Task Download( CancellationToken ct, Func< long > get_max_output_file_size_func, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 )
        {
            var m3u8_url         = _IP.M3u8Url;
            var output_file_name = _IP.OutputFileName;

            var i = m3u8_url.LastIndexOf( '/' );
            var base_url = (i != -1) ? m3u8_url.Substring( 0, i + 1 ) : m3u8_url;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var task_DownloadContent = RunDownloadContent( m3u8_url, requestHeaders, ct, milliseconds_delay_between_request );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var task_DownloadParts = RunDownloadParts_LimitedFileSize( output_file_name, base_url, requestHeaders, ct, get_max_output_file_size_func );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            //---await Task.Delay( Timeout.Infinite, ct ).CAX();
            var task = await Task.WhenAny( task_DownloadContent, task_DownloadParts ).CAX();
            if ( ct.IsCancellationRequested )
            {
                Task.WaitAll( task_DownloadContent, task_DownloadParts );
            }
            task.GetAwaiter().GetResult();
            //---await task.ContinueWith( t => throw t.Exception, TaskContinuationOptions.OnlyOnFaulted ).CAX();
        }

        protected HttpMessageHandler CreateHandler( TimeSpan? timeout = null )
        {
#if NETCOREAPP
            SocketsHttpHandler CreateSocketsHttpHandler( in TimeSpan? _timeout )
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
                    AutomaticDecompression = DecompressionMethods.All 
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
            };

            var handler = CreateSocketsHttpHandler( timeout );
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
            };

            var handler = CreateHttpClientHandler( /*timeout*/ );
#endif
            return (handler);
        }
        protected abstract TInvoker CreateHttpInvoker( TimeSpan? timeout = null );
        protected abstract Task< string > GetStringContent( string requestUri, IDictionary< string, string > requestHeaders, CancellationToken ct );
        protected abstract Task< GetStreamAsync_Ex_Result > GetStreamAsync_Ex( string requestUri, IDictionary< string, string > requestHeaders, CancellationToken ct );

        private Task RunDownloadContent( string m3u8_url, IDictionary< string, string > requestHeaders, CancellationToken ct, int milliseconds_delay )
            => Task.Run( async () =>
            {
                //var debug_hs = new HashSet< string >();

                for ( var hs = new HashSet< string >(); !ct.IsCancellationRequested; )
                {
                    WaitIfPaused( ct );

                    try
                    {
                        var content = await GetStringContent( m3u8_url, requestHeaders, ct ).CAX();
                        var parts = content?.Split( ['\r', '\n'], StringSplitOptions.RemoveEmptyEntries )
                                            .Where( s => !s.StartsWith( "#" ) )
                                            .Select( s => s.Trim() )
                                            .Where( s => !s.IsNullOrEmpty() )
                                            .ToFillHashSet( hs );
                        if ( parts != null )
                        {
                            var new_parts = 0;
                            foreach ( var p in parts )
                            {
                                if ( _PartUrlsStatus.TryAddWithLock( p, PartUrlStatusEnum.Queueed ) )
                                {
                                    //var suc = debug_hs.Add( p ); Debug.Assert( suc );

                                    _PartUrls.Add( p, ct );
                                    _IP.DownloadContent?.Invoke( p );
                                    new_parts++;
                                }
                            }

                            if ( new_parts != 0 )
                            {
                                _PartUrlsStatus.RemoveWhereValuesWithLock( p => !parts.Contains( p ), status => status == PartUrlStatusEnum.Processed );
                            }
                        }
                        else
                        {
                            _IP.DownloadContent?.Invoke( "[empty content]" );
                        }
                    }
                    catch ( Exception ex )
                    {
                        _IP.DownloadContentError?.Invoke( m3u8_url, ex );
                    }

                    await Task.Delay( milliseconds_delay, ct ).CAX();
                }
            }
            , ct );
        private Task RunDownloadParts_LimitedFileSize( string output_file_name, string base_url, IDictionary< string, string > requestHeaders, CancellationToken ct, Func< long > get_max_output_file_size_func )
            => Task.Run( async () =>
            {
                var dir = Path.GetDirectoryName( output_file_name );
                if ( !Directory.Exists( dir ) ) Directory.CreateDirectory( dir );

                var fileNumber = 0;
                string get_next_output_file_name() => Path.Combine( dir, Path.GetFileNameWithoutExtension( output_file_name ) + $"-({++fileNumber})" + Path.GetExtension( output_file_name ) );

                long totalBytes = 0;
                var fi = default(FileInfo);
                Stream open_file_stream_4_write()
                {
                    var fn = get_next_output_file_name();
                    var fs = new FileStream( fn, FileMode.Create, FileAccess.Write, FileShare.Read );
                    _IP.DownloadCreateOutputFile?.Invoke( fn );
                    totalBytes = 0;
                    fi = new FileInfo( fn );
                    return (fs);
                }

            NEXT_FILE:
                using ( var fs = open_file_stream_4_write() )
                {
                    for ( var n = 0; !ct.IsCancellationRequested; )
                    {
                        WaitIfPaused( ct );

                        #region [.throttler by speed.]
                        var instantSpeedInMbps = _ThrottlerBySpeed_User.Throttle( ct );
                        #endregion

                        var p = _PartUrls.Take_Ex( ct );
                        var part_url = base_url + p;

                        try
                        {
                            Debug.WriteLine( p );
                            using var x = await GetStreamAsync_Ex( part_url, requestHeaders, ct ).CAX();
                            await x.Stream.CopyToAsync( fs, bufferSize: 81920, ct ).CAX();

                            if ( (++n % 10) == 0 )
                            {
                                await fs.FlushAsync( ct ).CAX();
                            }

                            fi.Refresh();
                            var file_size = fi.Length;
                            var partBytes = file_size - totalBytes;
                            totalBytes = file_size;

                            _ThrottlerBySpeed_User.TakeIntoAccountDownloadedBytes( (int) partBytes );

                            _IP.DownloadPart?.Invoke( p, partBytes, totalBytes, instantSpeedInMbps );

                            if ( get_max_output_file_size_func() <= totalBytes )
                            {
                                goto NEXT_FILE;
                            }
                        }
                        catch ( Exception ex )
                        {
                            _IP.DownloadPartError?.Invoke( p, ex );
                        }
                        finally
                        {
                            _PartUrlsStatus.UpdateWithLock( p, PartUrlStatusEnum.Processed );
                        }
                    }
                }
            }
            , ct );
        private Task RunDownloadParts_LimitedFileSize( string output_file_name, string base_url, IDictionary< string, string > requestHeaders, CancellationToken ct, long max_output_file_size )
            => RunDownloadParts_LimitedFileSize( output_file_name, base_url, requestHeaders, ct, () => max_output_file_size );
        private Task RunDownloadParts_NoLimitedFileSize( string output_file_name, string base_url, IDictionary< string, string > requestHeaders, CancellationToken ct )
            => Task.Run( async () =>
            {
                var dir = Path.GetDirectoryName( output_file_name );
                if ( !Directory.Exists( dir ) ) Directory.CreateDirectory( dir );

                using var fs = new FileStream( output_file_name, FileMode.Create, FileAccess.Write, FileShare.Read );
                _IP.DownloadCreateOutputFile?.Invoke( output_file_name );

                //var debug_lst = new List< string >();
                long totalBytes = 0;
                var fi = new FileInfo( output_file_name );
                for ( var n = 0; !ct.IsCancellationRequested; )
                {
                    WaitIfPaused( ct );

                    #region [.throttler by speed.]
                    var instantSpeedInMbps = _ThrottlerBySpeed_User.Throttle( ct );
                    #endregion

                    var p = _PartUrls.Take_Ex( ct );
                    var part_url = base_url + p;

                    try
                    {
                        //debug_lst.Add( p );
                        Debug.WriteLine( p );
                        using var x = await GetStreamAsync_Ex( part_url, requestHeaders, ct ).CAX();
                        await x.Stream.CopyToAsync( fs, bufferSize: 81920, ct ).CAX();

                        if ( (++n % 10) == 0 )
                        {
                            await fs.FlushAsync( ct ).CAX();
                        }

                        fi.Refresh();
                        var file_size = fi.Length;
                        var partBytes = file_size - totalBytes;
                        totalBytes = file_size;
                        //var partBytes = fi.Length;
                        //totalBytes += partBytes;

                        _ThrottlerBySpeed_User.TakeIntoAccountDownloadedBytes( (int) partBytes );

                        _IP.DownloadPart?.Invoke( p, partBytes, totalBytes, instantSpeedInMbps );
                    }
                    catch ( Exception ex )
                    {
                        _IP.DownloadPartError?.Invoke( p, ex );
                    }
                    finally
                    {
                        _PartUrlsStatus.UpdateWithLock( p, PartUrlStatusEnum.Processed );
                    }
                }
            }
            , ct );

        private void WaitIfPaused( CancellationToken ct )
        {
            #region [.check 'waitIfPausedEvent'.]
            if ( _IP.WaitIfPausedEvent == null )
            {
                return;
            }

            if ( !_IP.WaitIfPausedEvent.IsSet )
            {
                _IP.WaitingIfPaused?.Invoke();
                _IP.WaitIfPausedEvent.Wait( ct );

                _ThrottlerBySpeed_User.Restart();
            }
            #endregion
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class m3u8_live_stream_downloader__with_HttpClient : m3u8_live_stream_downloader_base< HttpClient >
    {
        #region [.ctor().]
        public m3u8_live_stream_downloader__with_HttpClient( in InitParams ip ) : base( ip ) { }
        #endregion

        protected override HttpClient CreateHttpInvoker( TimeSpan? timeout = null )
        {
            var handler    = CreateHandler( timeout );
            var httpClient = new HttpClient( handler, true );
            if ( timeout.HasValue )
            {
                httpClient.Timeout = timeout.Value;
            }
            return (httpClient);
        }

        protected override Task< GetStreamAsync_Ex_Result > GetStreamAsync_Ex( string requestUri, IDictionary< string, string > requestHeaders, CancellationToken ct )
            => _HttpInvoker.GetStreamAsync_Ex( requestUri, requestHeaders, ct );
        protected override Task< string > GetStringContent( string requestUri, IDictionary< string, string > requestHeaders, CancellationToken ct )
            => _HttpInvoker.GetStringAsync_Ex( requestUri, requestHeaders, ct );

        //-------------------------------------------------------------------------------------------//
        public static async Task _Download_( string m3u8_url, string output_file_name, CancellationToken ct, long? max_output_file_size, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 )
        {
            using var m = new m3u8_live_stream_downloader__with_HttpClient( new InitParams() { M3u8Url = m3u8_url, OutputFileName = output_file_name } );
            await m.Download( ct, max_output_file_size, requestHeaders, milliseconds_delay_between_request ).CAX();
        }
        public static async Task _Download_( InitParams ip, CancellationToken ct, long? max_output_file_size, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 )
        {
            using var m = new m3u8_live_stream_downloader__with_HttpClient( ip );
            await m.Download( ct, max_output_file_size, requestHeaders, milliseconds_delay_between_request ).CAX();
        }
        public static async Task _Download_( InitParams ip, CancellationToken ct, Func< long > get_max_output_file_size_func, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 )
        {
            using var m = new m3u8_live_stream_downloader__with_HttpClient( ip );
            await m.Download( ct, get_max_output_file_size_func, requestHeaders, milliseconds_delay_between_request ).CAX();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class m3u8_live_stream_downloader__with_HttpMessageInvoker : m3u8_live_stream_downloader_base< HttpMessageInvoker >
    {
        public const bool      DEFAULT_CONNECTIONCLOSE    = true;
        public const int       DEFAULT_TIMEOUT_IN_SECONDS = 100;
        public static TimeSpan DEFAULT_TIMEOUT            => TimeSpan.FromSeconds( DEFAULT_TIMEOUT_IN_SECONDS );

        #region [.ctor().]
        private TimeSpan _Timeout;
        public m3u8_live_stream_downloader__with_HttpMessageInvoker( in InitParams ip, TimeSpan? timeout = null ) : base( ip ) 
        {
            _Timeout = timeout.GetValueOrDefault( /*i_m3u8_client.init_params.*/DEFAULT_TIMEOUT );
        }
        #endregion

        protected override HttpMessageInvoker CreateHttpInvoker( TimeSpan? timeout = null )
        {
            var handler     = CreateHandler( timeout );
            var httpInvoker = new HttpMessageInvoker( handler, true );
            return (httpInvoker);
        }

        protected override Task< GetStreamAsync_Ex_Result > GetStreamAsync_Ex( string requestUri, IDictionary< string, string > requestHeaders, CancellationToken ct )
            => _HttpInvoker.GetStreamAsync_Ex( requestUri, requestHeaders, _Timeout, ct );
        protected override Task< string > GetStringContent( string requestUri, IDictionary< string, string > requestHeaders, CancellationToken ct )
            => _HttpInvoker.GetStringAsync_Ex( requestUri, requestHeaders, _Timeout, ct );


        //-------------------------------------------------------------------------------------------//
        public static async Task _Download_( string m3u8_url, string output_file_name, TimeSpan? timeout, CancellationToken ct, long? max_output_file_size, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 )
        {
            using var m = new m3u8_live_stream_downloader__with_HttpMessageInvoker( new InitParams() { M3u8Url = m3u8_url, OutputFileName = output_file_name }, timeout );
            await m.Download( ct, max_output_file_size, requestHeaders, milliseconds_delay_between_request ).CAX();
        }
        public static async Task _Download_( InitParams ip, TimeSpan? timeout, CancellationToken ct, long? max_output_file_size, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 )
        {
            using var m = new m3u8_live_stream_downloader__with_HttpMessageInvoker( ip, timeout );
            await m.Download( ct, max_output_file_size, requestHeaders, milliseconds_delay_between_request ).CAX();
        }
        public static async Task _Download_( InitParams ip, TimeSpan? timeout, CancellationToken ct, Func< long > get_max_output_file_size_func, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 )
        {
            using var m = new m3u8_live_stream_downloader__with_HttpMessageInvoker( ip, timeout );
            await m.Download( ct, get_max_output_file_size_func, requestHeaders, milliseconds_delay_between_request ).CAX();
        }
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //internal interface i_m3u8_live_stream_downloader< TInvoker > where TInvoker : HttpMessageInvoker
    //{
    //    Task _Download_( string m3u8_url, string output_file_name, CancellationToken ct, long? max_output_file_size, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 );
    //    Task _Download_( InitParams ip, CancellationToken ct, long? max_output_file_size, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 );

    //}
    ///// <summary>
    ///// 
    ///// </summary>
    //internal static class m3u8_live_stream_downloader< TInvoker > where TInvoker : HttpMessageInvoker
    //{
    //    public static async Task _Download_( string m3u8_url, string output_file_name, CancellationToken ct, long? max_output_file_size, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 )
    //    {
    //        using var m = new m3u8_live_stream_downloader( new InitParams() { M3u8Url = m3u8_url, OutputFileName = output_file_name } );
    //        await m.Download( ct, max_output_file_size, requestHeaders, milliseconds_delay_between_request ).CAX();
    //    }
    //    public static async Task _Download_( InitParams ip, CancellationToken ct, long? max_output_file_size, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 )
    //    {
    //        using var m = new m3u8_live_stream_downloader( ip );
    //        await m.Download( ct, max_output_file_size, requestHeaders, milliseconds_delay_between_request ).CAX();
    //    }
    //    public static async Task _Download_( InitParams ip, CancellationToken ct, Func< long > get_max_output_file_size_func, IDictionary< string, string > requestHeaders = null, int milliseconds_delay_between_request = 1_000 )
    //    {
    //        using var m = new m3u8_live_stream_downloader( ip );
    //        await m.Download( ct, get_max_output_file_size_func, requestHeaders, milliseconds_delay_between_request ).CAX();
    //    }
    //}


    /// <summary>
    /// 
    /// </summary>
    internal static partial class m3u8_live_stream_downloader_extensions
    {
        private static HttpRequestMessage CreateRequestGet( Uri url, IDictionary< string, string > requestHeaders )
        {
            var req = new HttpRequestMessage( HttpMethod.Get, url );
            //req.Headers.ConnectionClose = _ConnectionClose;
            if ( requestHeaders != null )
            {
                foreach ( var header in requestHeaders )
                {
                    var suc = req.Headers.TryAddWithoutValidation( header.Key, header.Value );
                    Debug.Assert( suc );
                }
            }            
            return (req);
        }
        [M(O.AggressiveInlining)] public static async Task< string > GetStringAsync_Ex( this HttpClient hc, string requestUri, IDictionary< string, string > requestHeaders, CancellationToken ct )
        {
            using ( var req  = CreateRequestGet( new Uri( requestUri ), requestHeaders ) )
            using ( var resp = await hc.SendAsync( req, /*_HttpCompletionOption*/HttpCompletionOption.ResponseContentRead, ct ).CAX() )
            using ( var content = resp.Content )
            {
#if NETCOREAPP
                var text = await content.ReadAsStringAsync( ct ).CAX();
#else
                var text = await content.ReadAsStringAsync( /*ct*/ ).CAX();
#endif
                if ( resp.IsSuccessStatusCode )
                {
                    return (text);
                }

                throw (new HttpRequestException( text ));
            }
        }
        [M(O.AggressiveInlining)] public static async Task< string > GetStringAsync_Ex( this HttpMessageInvoker hi, string requestUri, IDictionary< string, string > requestHeaders, TimeSpan timeout, CancellationToken ct )
        {
            using ( var req  = CreateRequestGet( new Uri( requestUri ), requestHeaders ) )
            using ( var resp = await hi.SendAsync_Ex( req, timeout/*HttpCompletionOption.ResponseContentRead*/, ct ).CAX() )
            using ( var content = resp.Content )
            {
#if NETCOREAPP
                var text = await content.ReadAsStringAsync( ct ).CAX();
#else
                var text = await content.ReadAsStringAsync( /*ct*/ ).CAX();
#endif
                if ( resp.IsSuccessStatusCode )
                {
                    return (text);
                }

                throw (new HttpRequestException( text ));
            }
        }

        [M(O.AggressiveInlining)] public static async Task< GetStreamAsync_Ex_Result > GetStreamAsync_Ex( this HttpClient hc, string requestUri, IDictionary< string, string > requestHeaders, CancellationToken ct )
        {
            using ( var req = CreateRequestGet( new Uri( requestUri ), requestHeaders ) )
            {
                var resp = await hc.SendAsync( req, /*_HttpCompletionOption*/HttpCompletionOption.ResponseContentRead, ct ).CAX();
                //using ( var content = resp.Content )
                {
                    if ( resp.IsSuccessStatusCode )
                    {
#if NETCOREAPP
                        var stream = await resp.Content.ReadAsStreamAsync( ct ).CAX();
#else
                        var stream = await resp.Content.ReadAsStreamAsync( /*ct*/ ).CAX();
#endif
                        return (new GetStreamAsync_Ex_Result( resp, stream ));
                    }
#if NETCOREAPP
                    var text = await resp.Content.ReadAsStringAsync( ct ).CAX();
#else
                    var text = await resp.Content.ReadAsStringAsync( /*ct*/ ).CAX();
#endif
                    using ( resp )
                    {
                        throw (new HttpRequestException( text ));
                    }
                }
            }
        }
        [M(O.AggressiveInlining)] public static async Task< GetStreamAsync_Ex_Result > GetStreamAsync_Ex( this HttpMessageInvoker hi, string requestUri, IDictionary< string, string > requestHeaders, TimeSpan timeout, CancellationToken ct )
        {
            using ( var req = CreateRequestGet( new Uri( requestUri ), requestHeaders ) )
            {
                var resp = await hi.SendAsync_Ex( req, timeout/*HttpCompletionOption.ResponseContentRead*/, ct ).CAX();
                //using ( var content = resp.Content )
                {
                    if ( resp.IsSuccessStatusCode )
                    {
#if NETCOREAPP
                        var stream = await resp.Content.ReadAsStreamAsync( ct ).CAX();
#else
                        var stream = await resp.Content.ReadAsStreamAsync( /*ct*/ ).CAX();
#endif
                        return (new GetStreamAsync_Ex_Result( resp, stream ));
                    }
#if NETCOREAPP
                    var text = await resp.Content.ReadAsStringAsync( ct ).CAX();
#else
                    var text = await resp.Content.ReadAsStringAsync( /*ct*/ ).CAX();
#endif
                    using ( resp )
                    {
                        throw (new HttpRequestException( text ));
                    }
                }
            }
        }

        [M(O.AggressiveInlining)] public static HashSet< T > ToFillHashSet< T >( this IEnumerable< T > seq, HashSet< T > hs )
        {
            hs.Clear();
            if ( seq != null )
            {
                foreach ( var t in seq )
                {
                    hs.Add( t );
                }
            }
            return (hs);
        }
        [M(O.AggressiveInlining)] public static bool TryAddWithLock< K, T >( this Dictionary< K, T > dict, K k, T t )
        {
            lock ( dict )
            {
                if ( !dict.ContainsKey( k ) )
                {
                    dict.Add( k, t );
                    return (true);
                }
            }
            return (false);
        }
        [M(O.AggressiveInlining)] public static bool UpdateWithLock< K, T >( this Dictionary< K, T > dict, K k, T t )
        {
            lock ( dict )
            {
                if ( dict.ContainsKey( k ) )
                {
                    dict[ k ] = t;
                    return (true);
                }
                //dict[ k ] = t;
            }
            return (false);
        }
        [M(O.AggressiveInlining)] public static void RemoveWhereValuesWithLock< K, T >( this Dictionary< K, T > dict, Func< K, bool > keyMatchFunc, Func< T, bool > valueMatchFunc
            , List< KeyValuePair< K, T > > buf = null )
        {
            lock ( dict )
            {
                if ( buf == null ) buf = new List< KeyValuePair< K, T > >( dict.Count );
                else buf.Clear();

                buf.AddRange( dict );
                foreach ( var p in buf )
                {
                    if ( keyMatchFunc( p.Key ) && valueMatchFunc( p.Value ) )
                    {
                        dict.Remove( p.Key );
                    }
                }
            }
        }

        [M(O.AggressiveInlining)] public static T Take_Ex< T >( this BlockingCollection< T > col, CancellationToken ct, int millisecondsTimeout = 1_000 )
        {
            for (; ; )
            {
                if ( col.TryTake( out var t, millisecondsTimeout, ct ) )
                {
                    return (t);
                }
            }
        }
    }
}
