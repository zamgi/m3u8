using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
#if !(NETCOREAPP)
using System.Security.Authentication;
#endif
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Win32.SafeHandles;

using m3u8.client;
using m3u8.helpers;
using m3u8.infrastructure;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Program
    {
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
                //await Create_ts_files_with_number_series().CAX();
                //await Test_ts_files_with_number_series().CAX();
#if NETCOREAPP
                //Test_openFile_2();
                //Test_openFile_4();
                Test_openFile_5();

                //AuditCheck_ts_files_with_number_series();
#endif
                //await Merge_ts_files_2_one_avi().CAX();
                //await Test__obj_pool().CAX();

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
    
            /// <summary>
            /// 
            /// </summary>
            private sealed class throttler_by_speed_impl : i_throttler_by_speed_t
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
                using var throttler_by_speed = new throttler_by_speed_impl();
                using var streamPool         = new ObjectPoolDisposable< Stream >( maxDegreeOfParallelism, () => new MemoryStream( streamInPoolCapacity ) );
                using var respBufPool        = new ObjectPool< byte[] >( maxDegreeOfParallelism, () => new byte[ bufInPoolCapacity ] );
                using var timeoutCtsPool     = new CtsTimerPool( maxDegreeOfParallelism );
                using var fileWriterHolder   = FileHelper.CreateFileWriterHolder( outputFileName );

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
                    requestHeaders                   = requestHeaders,
                    //OutputFileName                   = outputFileName,
                    FileWriterHolder                 = fileWriterHolder,
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

                await m3u8_processor.DownloadPartsAndSave( p, ct ).CAX();
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
                using var throttler_by_speed       = new throttler_by_speed_impl();
                using var streamPool               = new ObjectPoolDisposable< Stream >( maxDegreeOfParallelism, () => new MemoryStream( streamInPoolCapacity ) );
                using var respBufPool              = new ObjectPool< byte[] >( maxDegreeOfParallelism, () => new byte[ bufInPoolCapacity ] );
                using var timeoutCtsPool           = new CtsTimerPool( maxDegreeOfParallelism );
                using var fileWriterHolder         = FileHelper.CreateFileWriterHolder( outputFileName );

                var responseStepAction = new m3u8_processor.ResponseStepActionDelegate( (in m3u8_processor.ResponseStepActionParams p) => ConsoleHelper.WriteLine( $"{p.Part.OrderNumber + 1} of {p.TotalPartCount}, '{p.Part.RelativeUrlName}'" ) );
                var waitIfPausedHolder = new WaitIfPausedHolder( waitIfPausedEventWrapper );

                var p = new m3u8_processor.DownloadPartsAndSaveInputParams()
                {
                    mc                               = mc,
                    m3u8File                         = m3u8File,
                    //OutputFileName                   = outputFileName,
                    FileWriterHolder                 = fileWriterHolder,
                    requestHeaders                   = requestHeaders,
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

                await m3u8_processor.DownloadPartsAndSave( p, ct ).CAX();
            }
        }

        private static string to_text_format( int size ) => to_text_format( (ulong) size );
        private static string to_text_format( ulong size ) => (0 < size) ? size.ToString("0,0") : "0";

#if NETCOREAPP
        private static void Test_openFile_1()
        {
            var fn = @"E:\test-1.txt";

            // Шаг 1: Гарантируем, что файл существует и физически не пустой (> 0 байт)
            /*
            var fileInfo = new FileInfo( fn );
            if ( !fileInfo.Exists || (fileInfo.Length == 0) )
            {
                File.WriteAllBytes( fn, new byte[] { 0 } ); // 1 байт заглушки
            }
            //*/

            // Шаг 2: Открываем поток файла с флагами Read и DELETE (через FileShare.Delete)
            // Это КРИТИЧЕСКИ важно, чтобы File.Move вообще мог изменить имя файла.
            using ( var fs = new FileStream( fn, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read | FileShare.Delete ) )
            using ( var sw = new StreamWriter( fs ) )
            {
                var (suc, errorCode) = WinApi.MakeFileSparse( fs.SafeFileHandle );
                Debug.Assert( suc );

                //if ( fs.Length == 0 )
                //{
                //    fs.SetLength( 1 );
                //    fs.Flush( true );
                //}
                
                // Резервируем максимальный размер, до которого ваш лог может вырасти (например, 1 ГБ)
                // На диске файл останется весить ровно столько, сколько в него реально записано!
                const long MAX_GROWTH_SIZE = 1024 * 1024 * 1024; // 1 ГБ (можно поставить больше)


                // Шаг 3: Создаем проекцию файла в память ядра (как это делает Windows для запущенных .exe)
                // Мы проецируем файл на его текущую длину.
                using ( var mmf = MemoryMappedFile.CreateFromFile(
                    fs,
                    mapName: null,
                    capacity: MAX_GROWTH_SIZE, // 0 означает спроецировать весь текущий размер файла
                    MemoryMappedFileAccess.ReadWrite,
                    HandleInheritability.None,
                    leaveOpen: true ) )
                {
                    sw.WriteLine( "1234567890" );
                    sw.WriteLine( string.Join( string.Empty, "1234567890".Reverse() ) );
                    sw.Flush();

                    var new_fn = Path.Combine( Path.GetDirectoryName( fn ), "test-2.txt" );

                    // Шаг 4: Переименовываем файл стандартным методом .NET
                    // Так как FileStream держит FileShare.Delete, а mmf защищает от физического стирания,
                    // операционная система атомарно переносит запись файла в MFT, не закрывая дескриптор.
                    File.Move( fn, new_fn, overwrite: true );

                    sw.WriteLine( "------------------------------------------------------------------" );
                    sw.WriteLine( "1234567890" );
                    sw.WriteLine( string.Join( string.Empty, "1234567890".Reverse() ) );
                    sw.Flush();
                }
            }
        }
        private static void Test_openFile_2()
        {
            var fn = @"E:\test-1.txt";

            using var fileHandle = WinApi.File_OpenOrCreate( fn, out var errorCode );
            Debug.Assert( !fileHandle.IsInvalid );

            // Шаг 2: Открываем поток файла с флагами Read и DELETE (через FileShare.Delete)
            // Это КРИТИЧЕСКИ важно, чтобы File.Move вообще мог изменить имя файла.
            using ( var fs = new FileStream( fileHandle, FileAccess.Write/*ReadWrite*/ ) )
            using ( var sw = new StreamWriter( fs, leaveOpen: true ) )
            {
                sw.WriteLine( "1234567890" );
                sw.WriteLine( string.Join( string.Empty, "1234567890".Reverse() ) );
                sw.Flush();

                var new_fn = Path.Combine( Path.GetDirectoryName( fn ), "test-2.txt" );

                // Шаг 4: Переименовываем файл стандартным методом .NET
                // Так как FileStream держит FileShare.Delete, а mmf защищает от физического стирания,
                // операционная система атомарно переносит запись файла в MFT, не закрывая дескриптор.
                (var suc, errorCode) = WinApi.RenameViaNtDll( fs.SafeFileHandle, new_fn );
                Debug.Assert( suc );
                //---File.Move( fn, new_fn, overwrite: true );

                sw.WriteLine( "------------------------------------------------------------------" );
                sw.WriteLine( "1234567890" );
                sw.WriteLine( string.Join( string.Empty, "1234567890".Reverse() ) );
                sw.Flush();
            }
        }
        private static void Test_openFile_3()
        {
            var fn = @"E:\test-1.txt";

            using ( var fs = FileHelper.File_Open4Write( fn, FileShare.ReadWrite | FileShare.Delete ) )
            using ( var sw = new StreamWriter( fs, leaveOpen: true ) )
            {
                var lockStreamPath = fn + ":super_lock";
                var lockHandle = WinApi.File_OpenOrCreate_2( lockStreamPath, out var errorCode );
                Debug.Assert( !lockHandle.IsInvalid );
                try
                {
                    sw.WriteLine( "1234567890" );
                    sw.WriteLine( string.Join( string.Empty, "1234567890".Reverse() ) );
                    sw.Flush();

                    var new_fn = Path.Combine( Path.GetDirectoryName( fn ), "test-2.txt" );
                    lockHandle.Dispose();
                    File.Move( fn, new_fn, overwrite: true );
                    fn = new_fn;
                    lockStreamPath = fn + ":super_lock";
                    lockHandle = WinApi.File_OpenOrCreate_2( lockStreamPath, out errorCode );
                    Debug.Assert( !lockHandle.IsInvalid );

                    sw.WriteLine( "------------------------------------------------------------------" );
                    sw.WriteLine( "1234567890" );
                    sw.WriteLine( string.Join( string.Empty, "1234567890".Reverse() ) );
                    sw.Flush();
                }
                finally
                {
                    lockHandle.Dispose();
                }
            }
        }
        private static void Test_openFile_4()
        {
            var fn = @"E:\test-1.txt";

            using ( var fs = FileHelper.File_Open4Write( fn, FileShare.ReadWrite | FileShare.Delete ) )
            using ( var sw = new StreamWriter( fs, leaveOpen: true ) )
            {
                var fsShadowLock = new FileStream( fn, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
                try
                {
                    sw.WriteLine( "1234567890" );
                    sw.WriteLine( string.Join( string.Empty, "1234567890".Reverse() ) );
                    sw.Flush();

                    var new_fn = Path.Combine( Path.GetDirectoryName( fn ), "test-2.txt" );
                    fsShadowLock.Dispose();
                    var (suc, errorCode) = WinApi.MoveFileEx( fn, new_fn );//---File.Move( fn, new_fn, overwrite: true );
                    fn = new_fn;
                    fsShadowLock = new FileStream( fn, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );

                    sw.WriteLine( "------------------------------------------------------------------" );
                    sw.WriteLine( "1234567890" );
                    sw.WriteLine( string.Join( string.Empty, "1234567890".Reverse() ) );
                    sw.Flush();
                }
                finally
                {
                    fsShadowLock.Dispose();
                }
            }
        }
        private static void Test_openFile_5()
        {
            var fn = @"E:\test-1.txt";

            using ( var fwh = FileHelper.CreateFileWriterHolder( fn ) )
            using ( var fs = fwh.Open( setLength2Zero: true ) )
            using ( var sw = new StreamWriter( fs, leaveOpen: true ) )
            {
                sw.WriteLine( "1234567890" );
                sw.WriteLine( string.Join( string.Empty, "1234567890".Reverse() ) );
                sw.Flush();

                var new_fn = Path.Combine( Path.GetDirectoryName( fn ), "test-2.txt" );
                var suc = fwh.TryMoveFile( new_fn, out var error );
                Debug.Assert( suc );

                sw.WriteLine( "------------------------------------------------------------------" );
                sw.WriteLine( "1234567890" );
                sw.WriteLine( string.Join( string.Empty, "1234567890".Reverse() ) );
                sw.Flush();
            }
        }
#endif

        private static async Task Merge_ts_files_2_one_avi( string path = @"E:\Даун Хаус (2001)" )
        {
            var avi_fn = Path.Combine( path, @"Даун Хаус (2001).avi" );
            using ( var avi_fs = new FileStream( avi_fn, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read ) )
            {
                avi_fs.SetLength( 0 );

                foreach ( var fn in Directory.EnumerateFiles( path, "*.ts" ) )
                {
                    using ( var fs = File.OpenRead( fn ) )
                    {
                        await fs.CopyToAsync( avi_fs ).CAX();
                    }
                }
            }
        }

        private static async Task Create_ts_files_with_number_series( string path = @"E:\ts_files_with_number_series", int part_count = 500, int total_size = 1_024 * 1_024 * 300 )
        {
            if ( !Directory.Exists( path ) ) Directory.CreateDirectory( path );

            //var total_bytes_count = Enumerable.Range( 0, total_size ).Sum( i => (long) 2 * (i.ToString().Length + 1) );

            var m3u8_fn = Path.Combine( path, $"file.m3u8" );
            using var m3u8_fs = File.OpenWrite( m3u8_fn );
            m3u8_fs.SetLength( 0 );

            var part_size = total_size / part_count;
            var n = 0;
            for ( var i = 0; i < part_count; i++ )
            {
                var fn = $"{i + 1}.txt"; //$"{i + 1}.ts";
                var ffn = Path.Combine( path, fn );
                Console.Write( $"{i + 1} of {part_count} => {ffn}..." );
                using ( var fs = File.OpenWrite( ffn ) )
                {
                    fs.SetLength( 0 );
                    for ( ; ; n++ )
                    {
                        var bytes = Encoding.UTF8.GetBytes( $"{n},");
                        await fs.WriteAsync( bytes, default ).CAX();
                        await fs.FlushAsync().CAX();

                        if ( part_size <= fs.Position )
                        {
                            n++;
                            break;
                        }
                    }
                }

                var bytes_2 = Encoding.UTF8.GetBytes( $"{fn}{Environment.NewLine}" );
                await m3u8_fs.WriteAsync( bytes_2, default ).CAX();

                Console.WriteLine( "ok." );
            }
        }
        private static async Task Test_ts_files_with_number_series( string path = @"E:\ts_files_with_number_series" )
        {
            var ss = new SortedSet< int >( Directory.EnumerateFiles( path, "*.txt" ).Select( fn => int.Parse( Path.GetFileNameWithoutExtension( fn ) ) ) );
            var n = 0;
            foreach ( var i in ss )
            {
                var ffn = Path.Combine( path, $"{i}.txt" );
                Console.Write( $"{i + 1} of {ss.Count} => {ffn}..." );
                using ( var sr = new StreamReader( ffn ) )
                {
                    var line = await sr.ReadToEndAsync().CAX();
                    var array = line.Split( [','], StringSplitOptions.RemoveEmptyEntries );
                    foreach ( var s in array )
                    {
                        var j = int.Parse( s );
                        if ( j != n )
                        {
                            Debugger.Break();
                        }
                        n++;
                    }
                }
                Console.WriteLine( "ok." );
            }
        }
#if NETCOREAPP
        private static void AuditCheck_ts_files_with_number_series( string filename = @"E:\For Test Ts Files With Number Series File.txt" )
        {
            var n = 0;
            foreach ( var mem in GetEnumOf_AuditCheck_ts_files_with_number_series( filename ) )
            {
                //if ( n == 10042689 - 1 )
                //{
                //    Debugger.Break();
                //}

                if ( !int.TryParse( mem.Span, out var i ) )
                {
                    ConsoleHelper.WriteLineError( $"{n:#,#}" );
                    Debugger.Break();
                }
                else if ( i != n )
                {
                    ConsoleHelper.WriteLineError( $"{n:#,#}" );
                    Debugger.Break();
                }
                n++;
                if ( (n % 1_000_000) == 0 )
                {
                    Console.Write( $"{n:#,#}\r" );
                }
            }
            Console.Write( $"{n:#,#}\r" );
        }
        unsafe private static IEnumerable< Memory< char > > GetEnumOf_AuditCheck_ts_files_with_number_series( string filename )
        {
            var readBuf = new char[ 1_024 ];

            var buf = new StringBuilder( 2_048 );

            var memBuf = new char[ 2*sizeof(ulong)/*1_024*/ ];
            var mem    = new Memory<char>( memBuf );

            using ( var sr = new StreamReader( filename ) )
            {
                for (; ; )
                {
                    var readCnt = sr.ReadBlock( readBuf, 0, readBuf.Length );
                    if ( readCnt == 0 ) yield break;

                    var lastCommaEndIdx = readBuf.LastIndexOf( readCnt, ',' );
                    lastCommaEndIdx = (lastCommaEndIdx == -1) ? readCnt : lastCommaEndIdx + 1;
                    for ( var i = 0; i < lastCommaEndIdx; i++ )
                    {
                        buf.Append( readBuf[ i ] );
                    }

                    var startIdx = 0;
                    for ( int i = 0, len = buf.Length; i < len; /*i++*/ )
                    {
                        var ch = buf[ i ];
                        if ( ch == ',' )
                        {
                            var ret_len = i - startIdx;
                            Debug.Assert( 0 < ret_len );
                            buf.CopyTo( startIdx, memBuf, ret_len );
                            var ret = mem.Slice( 0, ret_len );
                            yield return (ret);
                            startIdx = ++i;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    Debug.Assert( buf.Length - startIdx  == 0 );

                    buf.Clear();
                    for ( /*endIdx++*/; lastCommaEndIdx < readCnt; lastCommaEndIdx++ )
                    {
                        buf.Append( readBuf[ lastCommaEndIdx ] );
                    }
                }
            }
        }
        private static int LastIndexOf( this char[] buf, int cnt, char ch )
        {
            for ( var i = cnt - 1; 0 <= i; i-- )
            {
                if ( buf[ i ] == ',' )
                {
                    return (i);
                }
            }
            return (-1);
        }
#endif

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
    internal static class WinApi
    {
        // Константа Windows для перевода файла в режим Sparse (разреженный)
        private const uint FSCTL_SET_SPARSE = 0x000900C4;

        // Подключаем системную функцию для управления разреженными файлами NTFS
        [DllImport( "kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true )]
        private static extern bool DeviceIoControl( SafeFileHandle hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped );

        public static (bool suc, int errorCode) MakeFileSparse( SafeFileHandle fileHandle )
        {
            uint bytesReturned;
            var suc = DeviceIoControl(
                fileHandle,
                FSCTL_SET_SPARSE,
                IntPtr.Zero, 0, IntPtr.Zero, 0,
                out bytesReturned, IntPtr.Zero
            );
            return (suc, suc ? Marshal.GetLastWin32Error() : 0);
        }
        //--------------------------------------------------------------------------//

        // Открываем файл через Win32 API, чтобы настроить права "как в БитТорренте"
        [DllImport( "kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true )]
        private static extern SafeFileHandle CreateFileW( string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile );

        // Используем низкоуровневую функцию ядра для изменения путей, она идеально дружит с FileStream .NET
        [DllImport( "ntdll.dll", SetLastError = true )]
        private static extern int NtSetInformationFile( SafeFileHandle fileHandle, IntPtr ioStatusBlock, IntPtr fileInformation, uint length, int fileInformationClass );

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint DELETE = 0x00010000; // Право на переименование для себя

        private const uint FILE_SHARE_READ = 0x00000001; // Проводнику даем только READ. Никакого SHARE_DELETE!
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint OPEN_ALWAYS = 4;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const int FileRenameInformation = 10; // Класс переименования в ntdll                                                       

        public static SafeFileHandle File_OpenOrCreate( string fileName, out int errorCode )
        {
            var handle = CreateFileW(
                fileName,
                /*GENERIC_READ |*/ GENERIC_WRITE | DELETE, // Сказали Windows, что мы и пишем, и читаем
                FILE_SHARE_READ | FILE_SHARE_WRITE,    // Внешнему миру дали читать
                IntPtr.Zero,
                OPEN_ALWAYS,
                FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero
            );
            errorCode = handle.IsInvalid ? Marshal.GetLastWin32Error() : 0;
            return (handle);
        }

        /// <summary>
        /// Низкоуровневый побайтовый маршалинг структуры FileRenameInformation для ntdll.dll
        /// </summary>
        public static (bool suc, int errorCode) RenameViaNtDll( SafeFileHandle handle, string targetFullPath )
        {
            // ntdll требует префикс \??\ для абсолютных путей тома
            if ( !targetFullPath.StartsWith( @"\??\" ) )
            {
                targetFullPath = @"\??\" + targetFullPath;
            }

            var pathBytes = Encoding.Unicode.GetBytes( targetFullPath );

            // Размер заголовка FILE_RENAME_INFORMATION для NtSetInformationFile:
            // BOOLEAN ReplaceIfExists (1 байт) + Внутренний паддинг (7 байт на x64) + HANDLE RootDirectory (8 байт) + ULONG FileNameLength (4 байта) = 20 байт.
            int headerSize = 20;
            int totalBufferSize = headerSize + pathBytes.Length;

            var pBuffer = Marshal.AllocHGlobal( totalBufferSize );
            var pIoStatus = Marshal.AllocHGlobal( 16 ); // Буфер статуса ввода-вывода
            try
            {
                // Зануляем буфер
                var zeroBuffer = new byte[ totalBufferSize ];
                Marshal.Copy( zeroBuffer, 0, pBuffer, totalBufferSize );

                // Заполняем структуру:
                Marshal.WriteByte( pBuffer, 0, 1 ); // ReplaceIfExists = TRUE
                Marshal.WriteInt32( pBuffer, 16, pathBytes.Length ); // Длина пути в байтах по смещению 16

                // Копируем сам путь в память сразу после заголовка (смещение 20)
                var pStringOffset = IntPtr.Add( pBuffer, headerSize );
                Marshal.Copy( pathBytes, 0, pStringOffset, pathBytes.Length );

                // Вызываем функцию ядра. Она возвращает NTSTATUS (0 означает успех / STATUS_SUCCESS)
                var ntStatus = NtSetInformationFile( handle, pIoStatus, pBuffer, (uint) totalBufferSize, FileRenameInformation );
                return (ntStatus == 0, ntStatus);
            }
            finally
            {
                Marshal.FreeHGlobal( pBuffer );
                Marshal.FreeHGlobal( pIoStatus );
            }
        }

        //------------------------------------------------------------------//

        public static SafeFileHandle File_OpenOrCreate_2( string fileName, out int errorCode )
        {
            var handle = CreateFileW(
                fileName,
                GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE, // БЕЗ SHARE_DELETE!
                IntPtr.Zero,
                OPEN_ALWAYS,
                FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero
            );
            errorCode = handle.IsInvalid ? Marshal.GetLastWin32Error() : 0;
            return (handle);
        }
        //------------------------------------------------------------------//

        // Используем нативный MoveFileExW для максимальной скорости операции ядра
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        private static extern bool MoveFileExW( string lpExistingFileName, string lpNewFileName, uint dwFlags );

        // Флаг заставляет ядро заменить существующий файл, если он уже есть
        private const uint MOVEFILE_REPLACE_EXISTING = 0x00000001;

        public static (bool suc, int errorCode) MoveFileEx( string fileName, string newFileName )
        {
            var suc = MoveFileExW( fileName, newFileName, MOVEFILE_REPLACE_EXISTING );
            return (suc, suc ? 0 : Marshal.GetLastWin32Error());
        }
    }
}
