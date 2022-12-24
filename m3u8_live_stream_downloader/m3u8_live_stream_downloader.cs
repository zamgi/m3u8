using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class m3u8_live_stream_downloader : IDisposable
    {
        public delegate void DownloadContentDelegate( string part_url );
        public delegate void DownloadContentErrorDelegate( string m3u8_url, Exception ex );        
        public delegate void DownloadPartDelegate( string part_url, long part_size_in_bytes, long total_in_bytes, double? instantaneousSpeedInMbps );
        public delegate void DownloadPartErrorDelegate( string part_url, Exception ex );
        public delegate void DownloadCreateOutputFileDelegate( string output_file_name );

        /// <summary>
        /// 
        /// </summary>
        public struct InitParams
        {
            public string M3u8Url        { get; set; }
            public string OutputFileName { get; set; }

            public HttpClient HttpClient { get; set; }

            public ManualResetEventSlim   WaitIfPausedEvent { [M(O.AggressiveInlining)] get; set; }
            public Action                 WaitingIfPaused   { [M(O.AggressiveInlining)] get; set; }
            public I_throttler_by_speed_t ThrottlerBySpeed  { [M(O.AggressiveInlining)] get; set; }

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
        private HttpClient _HttpClient;
        private bool       _Dispose_HttpClient;
        private InitParams _IP;
        private BlockingCollection< string > _PartUrls;
        private Dictionary< string, PartUrlStatusEnum > _PartUrlsStatus;
        private I_ThrottlerBySpeed_InDownloadProcessUser _ThrottlerBySpeed_User;

        public m3u8_live_stream_downloader( in InitParams ip )
        {
            M3u8Url        = ip.M3u8Url        ?? throw (new ArgumentNullException( nameof(ip.M3u8Url) ));
            OutputFileName = ip.OutputFileName ?? throw (new ArgumentNullException( nameof(ip.OutputFileName) ));

            _IP = ip;

            _HttpClient         = ip.HttpClient ?? new HttpClient();
            _Dispose_HttpClient = (ip.HttpClient == null);
            _PartUrls           = new BlockingCollection< string >();
            _PartUrlsStatus     = new Dictionary< string, PartUrlStatusEnum >();
            _ThrottlerBySpeed_User = ThrottlerBySpeed_InDownloadProcessUser.Start( ip.ThrottlerBySpeed );
        }
        public void Dispose()
        {
            if ( _Dispose_HttpClient )
            {
                _HttpClient.Dispose();
            }
            _ThrottlerBySpeed_User.Dispose();
        }
        #endregion

        public static async Task _Download_( string m3u8_url, string output_file_name, CancellationToken ct, long? max_output_file_size, int milliseconds_delay_between_request = 1_000 )
        {
            using var m = new m3u8_live_stream_downloader( new InitParams() { M3u8Url = m3u8_url, OutputFileName = output_file_name } );
            await m.Download( ct, max_output_file_size, milliseconds_delay_between_request ).CAX();
        }
        public static async Task _Download_( InitParams ip, CancellationToken ct, long? max_output_file_size, int milliseconds_delay_between_request = 1_000 )
        {
            using var m = new m3u8_live_stream_downloader( ip );
            await m.Download( ct, max_output_file_size, milliseconds_delay_between_request ).CAX();
        }
        public static async Task _Download_( InitParams ip, CancellationToken ct, Func< long > get_max_output_file_size_func, int milliseconds_delay_between_request = 1_000 )
        {
            using var m = new m3u8_live_stream_downloader( ip );
            await m.Download( ct, get_max_output_file_size_func, milliseconds_delay_between_request ).CAX();
        }

        public string M3u8Url        { get; }
        public string OutputFileName { get; }

        public async Task Download( CancellationToken ct, long? max_output_file_size, int milliseconds_delay_between_request = 1_000 )
        {
            var m3u8_url         = _IP.M3u8Url;
            var output_file_name = _IP.OutputFileName;

            var i = m3u8_url.LastIndexOf( '/' );
            var base_url = (i != -1) ? m3u8_url.Substring( 0, i + 1 ) : m3u8_url;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var task_DownloadContent = RunDownloadContent( m3u8_url, ct, milliseconds_delay_between_request );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task task_DownloadParts;
            if ( max_output_file_size.HasValue )
            {
                task_DownloadParts = RunDownloadParts_LimitedFileSize( output_file_name, base_url, ct, max_output_file_size.Value );
            }
            else
            {
                task_DownloadParts = RunDownloadParts_NoLimitedFileSize( output_file_name, base_url, ct );
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
        public async Task Download( CancellationToken ct, Func< long > get_max_output_file_size_func, int milliseconds_delay_between_request = 1_000 )
        {
            var m3u8_url         = _IP.M3u8Url;
            var output_file_name = _IP.OutputFileName;

            var i = m3u8_url.LastIndexOf( '/' );
            var base_url = (i != -1) ? m3u8_url.Substring( 0, i + 1 ) : m3u8_url;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var task_DownloadContent = RunDownloadContent( m3u8_url, ct, milliseconds_delay_between_request );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var task_DownloadParts = RunDownloadParts_LimitedFileSize( output_file_name, base_url, ct, get_max_output_file_size_func );
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

        private Task RunDownloadContent( string m3u8_url, CancellationToken ct, int milliseconds_delay )
            => Task.Run( async () =>
            {
                //var debug_hs = new HashSet< string >();

                for ( var hs = new HashSet< string >(); !ct.IsCancellationRequested; )
                {
                    WaitIfPaused( ct );

                    try
                    {
#if NETCOREAPP
                        var content = await _HttpClient.GetStringAsync( m3u8_url, ct ).CAX();
#else
                        var content = await _HttpClient.GetStringAsync( m3u8_url/*, ct*/ ).CAX();
#endif
                        var parts = content?.Split().Where( s => !s.StartsWith( "#" ) )
                                                    .Select( s => s.Trim() )
                                                    .Where( s => !s.IsNullOrEmpty() )
                                                    .ToHashSet( hs );
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
        private Task RunDownloadParts_LimitedFileSize( string output_file_name, string base_url, CancellationToken ct, Func< long > get_max_output_file_size_func )
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
                        var instantaneousSpeedInMbps = _ThrottlerBySpeed_User.Throttle( ct );
                        #endregion

                        var p = _PartUrls.Take_Ex( ct );
                        var part_url = base_url + p;

                        try
                        {
                            Debug.WriteLine( p );
#if NETCOREAPP
                            using var s = await _HttpClient.GetStreamAsync( part_url, ct ).CAX();
#else
                            using var s = await _HttpClient.GetStreamAsync( part_url/*, ct*/ ).CAX();
#endif
                            await s.CopyToAsync( fs/*, ct */).CAX();

                            if ( (++n % 10) == 0 )
                            {
                                await fs.FlushAsync(/*ct*/).CAX();
                            }

                            fi.Refresh();
                            var file_size = fi.Length;
                            var partBytes = file_size - totalBytes;
                            totalBytes = file_size;

                            _ThrottlerBySpeed_User.TakeIntoAccountDownloadedBytes( (int) partBytes );

                            _IP.DownloadPart?.Invoke( p, partBytes, totalBytes, instantaneousSpeedInMbps );

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
        private Task RunDownloadParts_LimitedFileSize( string output_file_name, string base_url, CancellationToken ct, long max_output_file_size )
            => RunDownloadParts_LimitedFileSize( output_file_name, base_url, ct, () => max_output_file_size );
        private Task RunDownloadParts_NoLimitedFileSize( string output_file_name, string base_url, CancellationToken ct )
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
                    var instantaneousSpeedInMbps = _ThrottlerBySpeed_User.Throttle( ct );
                    #endregion

                    var p = _PartUrls.Take_Ex( ct );
                    var part_url = base_url + p;

                    try
                    {
                        //debug_lst.Add( p );
                        Debug.WriteLine( p );
#if NETCOREAPP
                        using var s = await _HttpClient.GetStreamAsync( part_url, ct ).CAX();
#else
                        using var s = await _HttpClient.GetStreamAsync( part_url/*, ct*/ ).CAX();
#endif
                        await s.CopyToAsync( fs/*, ct */).CAX();

                        if ( (++n % 10) == 0 )
                        {
                            await fs.FlushAsync(/*ct*/).CAX();
                        }

                        fi.Refresh();
                        var file_size = fi.Length;
                        var partBytes = file_size - totalBytes;
                        totalBytes = file_size;
                        //var partBytes = fi.Length;
                        //totalBytes += partBytes;

                        _ThrottlerBySpeed_User.TakeIntoAccountDownloadedBytes( (int) partBytes );

                        _IP.DownloadPart?.Invoke( p, partBytes, totalBytes, instantaneousSpeedInMbps );
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
    internal static class m3u8_live_stream_downloader_extensions
    {
        [M(O.AggressiveInlining)] public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
        [M(O.AggressiveInlining)] public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );

        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable< T > CAX< T >( this Task< T > task ) => task.ConfigureAwait( false );
        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable CAX( this Task task ) => task.ConfigureAwait( false );

        [M(O.AggressiveInlining)] public static HashSet< T > ToHashSet< T >( this IEnumerable< T > seq, HashSet< T > hs )
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
