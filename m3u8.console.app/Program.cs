using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using m3u8.ext;

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
            private static IReadOnlyCollection< m3u8_part_ts > download_m3u8File_parallel( m3u8_client mc, m3u8_file_t m3u8_file
                , CancellationTokenSource cts = null, int maxDegreeOfParallelism = 64 )
            {
                var ct = (cts?.Token).GetValueOrDefault( CancellationToken.None );
                var baseAddress = m3u8_file.BaseAddress;
                var totalPatrs  = m3u8_file.Parts.Count;
                var globalPartNumber = 0;
                var downloadPartsSet = new SortedSet< m3u8_part_ts >( m3u8_part_ts.Comparer.Inst );

                using ( DefaultConnectionLimitSaver.Create( maxDegreeOfParallelism ) )
                {                 
                    Parallel.ForEach( m3u8_file.Parts, new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism, CancellationToken = ct }, 
                    (part, loopState, idx) =>
                    {
                        var n = Interlocked.Increment( ref globalPartNumber );                
                        try
                        { 
                            CONSOLE.WriteLine( $"#{n} of {totalPatrs}). '{part.RelativeUrlName}'..." );
                            var downloadPart = mc.DownloadPart( part, baseAddress, ct ).Result;
                            if ( (downloadPart.Error != null) && !ct.IsCancellationRequested )
                            {
                                CONSOLE.WriteLineError( $"#{n} of {totalPatrs}). FAILED: {downloadPart.Error}{Environment.NewLine}" );
                            }

                            lock ( downloadPartsSet )
                            {
                                downloadPartsSet.Add( downloadPart );
                            }
                        }
                        catch ( Exception ex )
                        {
                            #region [.code.]
                            var aex = ex as AggregateException;
                            if ( (aex == null) || !aex.InnerExceptions.All( e => (e is OperationCanceledException) ) )
                            {
                                CONSOLE.WriteLineError( "ERROR: " + ex );
                            }
                            #endregion
                        }
                    });
                }

                ct.ThrowIfCancellationRequested();

                CONSOLE.WriteLine( $"\r\n total parts processed: {globalPartNumber} of {totalPatrs}" );

                return (downloadPartsSet);
            }

            public static void run( string M3U8_FILE_URL, string OUTPUT_FILE_DIR, string OUTPUT_FILE_EXT )
            {
                using ( var mc  = m3u8_client_factory.Create() )
                using ( var cts = new CancellationTokenSource() )
                {
                    var task = Task.Run( async () =>
                    {
                        var sw = Stopwatch.StartNew();

                        //-1-//
                        CONSOLE.WriteLine( $"download m3u8-file: '{M3U8_FILE_URL}'..." );
                        var m3u8FileUrl = new Uri( M3U8_FILE_URL );
                        var m3u8File = await mc.DownloadFile( m3u8FileUrl, cts.Token );
                        CONSOLE.WriteLine( $"success. parts count: {m3u8File.Parts.Count}\r\n" );

                        //-2-//
                        var downloadParts = download_m3u8File_parallel( mc, m3u8File, cts );

                        //-3-//
                        var outputFileName = Path.Combine( OUTPUT_FILE_DIR, PathnameCleaner.CleanPathnameAndFilename( m3u8FileUrl.AbsolutePath ).TrimStart( '-' ) + OUTPUT_FILE_EXT );
                        using ( var fs = Extensions.File_Open4Write( outputFileName ) )
                        {
                            foreach ( var downloadPart in downloadParts )
                            {
                                if ( downloadPart.Error != null )
                                {
                                    continue;
                                }
                                var bytes = downloadPart.Bytes;
                                fs.Write( bytes, 0, bytes.Length );
                            }
                        }

                        sw.Stop();
                        var totalBytes = downloadParts.Sum( p => (p.Bytes?.Length).GetValueOrDefault() );
                        CONSOLE.WriteLine( $"\r\nSuccess: downloaded & writed parts {downloadParts.Count( p => p.Error == null )} of {downloadParts.Count}\r\n" + 
                                           $"(elapsed: {sw.Elapsed}, file: '{outputFileName}', size: {to_text_format( totalBytes >> 20 )} mb)\r\n" );

                    })
                    .ContinueWith( t =>
                    {
                        if ( t.IsFaulted )
                        {
                            CONSOLE.WriteLineError( "ERROR: " + t.Exception );
                        }
                    });

                    #region [.wait for keyboard break.]
                    task.WaitForTaskEndsOrKeyboardBreak( cts );
                    #endregion
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static class v2
        {
            private static IReadOnlyCollection< m3u8_part_ts > download_m3u8File_parallel__v1( m3u8_client mc, m3u8_file_t m3u8_file
                , CancellationTokenSource cts = null, int maxDegreeOfParallelism = 64 )
            {
                var ct                = (cts?.Token).GetValueOrDefault( CancellationToken.None );
                var baseAddress       = m3u8_file.BaseAddress;
                var totalPatrs        = m3u8_file.Parts.Count;
                var successPartNumber = 0;

                var parts = m3u8_file.Parts;

                var expectedPartNumber  = parts.FirstOrDefault().OrderNumber;
                var maxPartNumber       = parts.LastOrDefault ().OrderNumber;
                var sourceQueue         = new Queue< m3u8_part_ts >( parts );
                var downloadPartsSet    = new SortedSet< m3u8_part_ts >( m3u8_part_ts.Comparer.Inst );
                var downloadPartsResult = new LinkedList< m3u8_part_ts >();

                using ( DefaultConnectionLimitSaver.Create( maxDegreeOfParallelism ) )
                using ( var canExtractPartEvent = new AutoResetEvent( false ) )
                using ( var semaphore           = new SemaphoreSlim( maxDegreeOfParallelism ) )
                {
                    var task_download = Task.Run( () =>
                    {
                        for ( ; sourceQueue.Count != 0; )
                        {
                            semaphore.Wait();
                            var part = sourceQueue.Dequeue();

                            CONSOLE.WriteLine( $"start download part: {part}..." );

                            mc.DownloadPart( part, baseAddress, ct )
                              .ContinueWith( t =>
                              {
                                  if ( t.IsFaulted )
                                  {
                                      Interlocked.Increment( ref expectedPartNumber );

                                      CONSOLE.WriteLine( $"'{t.Exception.GetType().Name}': '{t.Exception.Message}'.", ConsoleColor.Red );
                                  }
                                  else if ( !t.IsCanceled )
                                  {
                                      Interlocked.Increment( ref successPartNumber );

                                      var downloadPart = t.Result;

                                      CONSOLE.WriteLine( $"end download part: {downloadPart}." );

                                      lock ( downloadPartsSet )
                                      {
                                          downloadPartsSet.Add( downloadPart );
                                          canExtractPartEvent.Set();
                                      }
                                  }
                              });
                        }
                    }, ct );

                    var task_save = Task.Run( () =>
                    {
                        for ( ; expectedPartNumber <= maxPartNumber; )
                        {
                            CONSOLE.WriteLine( $"wait part #{expectedPartNumber}..." );

                            canExtractPartEvent.WaitOne();

                            lock ( downloadPartsSet )
                            {
                                for ( ; downloadPartsSet.Count != 0; )
                                {
                                    var min_part = downloadPartsSet.Min;
                                    if ( expectedPartNumber == min_part.OrderNumber )
                                    {
                                        CONSOLE.WriteLine( $"receive part #{expectedPartNumber}." );

                                        downloadPartsResult.AddLast( min_part );
                                        downloadPartsSet.Remove( min_part );

                                        Interlocked.Increment( ref expectedPartNumber );

                                        semaphore.Release();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }, ct );

                    Task.WaitAll( task_download, task_save );
                }

                var successs = downloadPartsResult.SequenceEqual( downloadPartsResult.OrderBy( p => p.OrderNumber ) );

                ct.ThrowIfCancellationRequested();

                CONSOLE.WriteLine( $"\r\n total parts processed: {successPartNumber} of {totalPatrs}" );

                return (downloadPartsResult);
            }

            public static void run__1( string M3U8_FILE_URL, string OUTPUT_FILE_DIR, string OUTPUT_FILE_EXT )
            {
                using ( var mc  = m3u8_client_factory.Create() )
                using ( var cts = new CancellationTokenSource() )
                {
                    var task = Task.Run( async () =>
                    {
                        var sw = Stopwatch.StartNew();

                        //-1-//
                        CONSOLE.WriteLine( $"download m3u8-file: '{M3U8_FILE_URL}'..." );
                        var m3u8FileUrl = new Uri( M3U8_FILE_URL );
                        var m3u8File = await mc.DownloadFile( m3u8FileUrl, cts.Token );
                        CONSOLE.WriteLine( $"success. parts count: {m3u8File.Parts.Count}\r\n" );

                        //-2-//
                        var downloadParts = download_m3u8File_parallel__v1( mc, m3u8File, cts );

                        //-3-//
                        var outputFileName = Path.Combine( OUTPUT_FILE_DIR, PathnameCleaner.CleanPathnameAndFilename( m3u8FileUrl.AbsolutePath ).TrimStart( '-' ) + OUTPUT_FILE_EXT );
                        using ( var fs = Extensions.File_Open4Write( outputFileName ) )
                        {
                            foreach ( var downloadPart in downloadParts )
                            {
                                if ( downloadPart.Error != null )
                                {
                                    continue;
                                }
                                var bytes = downloadPart.Bytes;
                                fs.Write( bytes, 0, bytes.Length );
                            }
                        }

                        sw.Stop();
                        var totalBytes = downloadParts.Sum( p => (p.Bytes?.Length).GetValueOrDefault() );
                        CONSOLE.WriteLine( $"\r\nSuccess: downloaded & writed parts {downloadParts.Count( p => p.Error == null )} of {downloadParts.Count}\r\n" + 
                                           $"(elapsed: {sw.Elapsed}, file: '{outputFileName}', size: {to_text_format( totalBytes >> 20 )} mb)\r\n" );

                    })
                    .ContinueWith( t =>
                    {
                        if ( t.IsFaulted )
                        {
                            CONSOLE.WriteLineError( "ERROR: " + t.Exception );
                        }
                    });

                    #region [.wait for keyboard break.]
                    task.WaitForTaskEndsOrKeyboardBreak( cts );
                    #endregion
                }
            }


            private static IEnumerable< m3u8_part_ts > download_m3u8File_parallel__v2( m3u8_client mc, m3u8_file_t m3u8_file
                , CancellationTokenSource cts = null, int maxDegreeOfParallelism = 64 )
            {
                var ct                = (cts?.Token).GetValueOrDefault( CancellationToken.None );
                var baseAddress       = m3u8_file.BaseAddress;
                var totalPatrs        = m3u8_file.Parts.Count;
                var successPartNumber = 0;

                var expectedPartNumber = m3u8_file.Parts.FirstOrDefault().OrderNumber;
                var maxPartNumber      = m3u8_file.Parts.LastOrDefault ().OrderNumber;
                var sourceQueue        = new Queue< m3u8_part_ts >( m3u8_file.Parts );
                var downloadPartsSet   = new SortedSet< m3u8_part_ts >( m3u8_part_ts.Comparer.Inst );

                using ( DefaultConnectionLimitSaver.Create( maxDegreeOfParallelism ) )
                using ( var canExtractPartEvent = new AutoResetEvent( false ) )
                using ( var semaphore           = new SemaphoreSlim( maxDegreeOfParallelism ) )
                {
                    var task_download = Task.Run( () =>
                    {
                        for ( ; sourceQueue.Count != 0; )
                        {
                            semaphore.Wait();
                            var part = sourceQueue.Dequeue();
#if DEBUG
                            CONSOLE.WriteLine( $"start download part: {part}..." ); 
#endif
                            mc.DownloadPart( part, baseAddress, ct )
                              .ContinueWith( t =>
                              {
                                  if ( t.IsFaulted )
                                  {
                                      Interlocked.Increment( ref expectedPartNumber );

                                      CONSOLE.WriteLine( $"'{t.Exception.GetType().Name}': '{t.Exception.Message}'.", ConsoleColor.Red );
                                  }
                                  else if ( !t.IsCanceled )
                                  {
                                      Interlocked.Increment( ref successPartNumber );

                                      var downloadPart = t.Result;
#if DEBUG
                                      CONSOLE.WriteLine( $"end download part: {downloadPart}." ); 
#endif
                                      lock ( downloadPartsSet )
                                      {
                                          downloadPartsSet.Add( downloadPart );
                                          canExtractPartEvent.Set();
                                      }
                                  }
                              });
                        }
                    }, ct );

                    for ( ; expectedPartNumber <= maxPartNumber && !ct.IsCancellationRequested; )
                    {
#if DEBUG
                        CONSOLE.WriteLine( $"wait part #{expectedPartNumber}..." ); 
#endif
                        canExtractPartEvent.WaitOne();

                        lock ( downloadPartsSet )
                        {
                            for ( ; downloadPartsSet.Count != 0; )
                            {
                                var min_part = downloadPartsSet.Min;
                                if ( expectedPartNumber == min_part.OrderNumber )
                                {
                                    CONSOLE.WriteLine( $"receive part #{expectedPartNumber}." );

                                    downloadPartsSet.Remove( min_part );

                                    Interlocked.Increment( ref expectedPartNumber );

                                    semaphore.Release();

                                    yield return(min_part);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                ct.ThrowIfCancellationRequested();

                CONSOLE.WriteLine( $"\r\n total parts processed: {successPartNumber} of {totalPatrs}" );
            }

            public static void run__2( string M3U8_FILE_URL, string OUTPUT_FILE_DIR, string OUTPUT_FILE_EXT )
            {
                using ( var mc  = m3u8_client_factory.Create() )
                using ( var cts = new CancellationTokenSource() )
                {
                    var task = Task.Run( async () =>
                    {
                        var sw = Stopwatch.StartNew();

                        //-1-//
                        CONSOLE.WriteLine( $"download m3u8-file: '{M3U8_FILE_URL}'..." );
                        var m3u8FileUrl = new Uri( M3U8_FILE_URL );
                        var m3u8File = await mc.DownloadFile( m3u8FileUrl, cts.Token );
                        CONSOLE.WriteLine( $"success. parts count: {m3u8File.Parts.Count}\r\n" );

                        //-2-//                        
                        var downloadParts = download_m3u8File_parallel__v2( mc, m3u8File, cts );

                        //-3-//
                        var downloadPartsSuccessCount = 0;
                        var downloadPartsErrorCount   = 0;
                        var totalBytes = 0;

                        var outputFileName = Path.Combine( OUTPUT_FILE_DIR, PathnameCleaner.CleanPathnameAndFilename( m3u8FileUrl.AbsolutePath ).TrimStart( '-' ) + OUTPUT_FILE_EXT );
                        using ( var fs = Extensions.File_Open4Write( outputFileName ) )
                        {
                            foreach ( var downloadPart in downloadParts )
                            {
                                if ( downloadPart.Error != null )
                                {
                                    downloadPartsErrorCount++;
                                    continue;
                                }
                                var bytes = downloadPart.Bytes;
                                fs.Write( bytes, 0, bytes.Length );

                                downloadPartsSuccessCount++;
                                totalBytes += bytes.Length;
                            }
                        }

                        sw.Stop();
                        CONSOLE.WriteLine( $"\r\nSuccess: downloaded & writed parts {downloadPartsSuccessCount} of {(downloadPartsErrorCount + downloadPartsSuccessCount)}\r\n" + 
                                           $"(elapsed: {sw.Elapsed}, file: '{outputFileName}', size: {to_text_format( totalBytes >> 20 )} mb)\r\n" );

                    })
                    .ContinueWith( t =>
                    {
                        if ( t.IsFaulted )
                        {
                            CONSOLE.WriteLineError( "ERROR: " + t.Exception );
                        }
                    });

                    #region [.wait for keyboard break.]
                    task.WaitForTaskEndsOrKeyboardBreak( cts );
                    #endregion
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static class v3
        {
            public static async Task run( string m3u8FileUrl, string outputFileName, CancellationToken ct )
            {
                var p = new m3u8_processor.DownloadFileAndSaveInputParams()
                {
                    CancellationToken  = ct,
                    m3u8FileUrl        = m3u8FileUrl,
                    OutputFileName     = outputFileName,
                    NetParams          = new m3u8_client.init_params() { AttemptRequestCount = 1, },
                    ResponseStepAction = new m3u8_processor.ResponseStepActionDelegate( t => CONSOLE.WriteLine( $"{t.Part.OrderNumber} of {t.TotalPartCount}, '{t.Part.RelativeUrlName}'" ) ),                    
                };

                await m3u8_processor.DownloadFileAndSave_Async( p ).CAX();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static class v4
        {
            public static async Task run( string m3u8FileUrl, string outputFileName, CancellationToken ct )
            {
                var p = new m3u8_processor__v2.DownloadFileAndSaveInputParams()
                {
                    CancellationToken  = ct,
                    m3u8FileUrl        = m3u8FileUrl,
                    OutputFileName     = outputFileName,
                    NetParams          = new m3u8_client.init_params() { AttemptRequestCount = 1, HttpCompletionOption = HttpCompletionOption.ResponseHeadersRead },
                    ResponseStepAction = new m3u8_processor__v2.ResponseStepActionDelegate( t => CONSOLE.WriteLine( $"{t.Part.OrderNumber + 1} of {t.TotalPartCount}, '{t.Part.RelativeUrlName}'" ) ),
                    //MaxDegreeOfParallelism = 8,
                };

                await m3u8_processor__v2.DownloadFileAndSave( p ).CAX();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static class next1
        {
            /// <summary>
            /// 
            /// </summary>
            private sealed class download_threads_semaphore_impl : I_download_threads_semaphore
            {
                private SemaphoreSlim _Semaphore;
                public download_threads_semaphore_impl( int maxDegreeOfParallelism ) => _Semaphore = new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism );
                public void Dispose() => _Semaphore.Dispose();
                public bool UseCrossDownloadInstanceParallelism => false;
                public void Release() => _Semaphore.Release();
                public void Wait( CancellationToken ct ) => _Semaphore.Wait( ct );
                public Task WaitAsync( CancellationToken ct ) => _Semaphore.WaitAsync( ct );
            }
            /// <summary>
            /// 
            /// </summary>
            private sealed class throttler_by_speed_impl : I_throttler_by_speed_t
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

            public static async Task run( string m3u8FileUrl, string outputFileName, CancellationToken ct, IDictionary< string, string > requestHeaders = null )
            {
                using var mc = m3u8_client_next_factory.Create( new m3u8_client_next.init_params() { AttemptRequestCount = 1, HttpCompletionOption = HttpCompletionOption.ResponseHeadersRead } );

                var m3u8File = await mc.DownloadFile( new Uri( m3u8FileUrl ), ct, requestHeaders ).CAX();

                var maxDegreeOfParallelism = 8;
                var streamInPoolCapacity   = 1_024 * 1_024 * 5;
                var bufInPoolCapacity      = 1_024 * 100;
                using var waitIfPausedEvent  = new ManualResetEventSlim( true, 0 );
                using var dts                = new download_threads_semaphore_impl( maxDegreeOfParallelism );
                using var dts_2              = new download_threads_semaphore_impl( maxDegreeOfParallelism );
                using var throttler_by_speed = new throttler_by_speed_impl();
                using var streamPool         = new ObjectPoolDisposable< Stream >( maxDegreeOfParallelism, () => new MemoryStream( streamInPoolCapacity ) );
                using var respBufPool        = new ObjectPool< byte[] >( maxDegreeOfParallelism, () => new byte[ bufInPoolCapacity ] );

                #region comm.
                //var requestStepAction      = new m3u8_processor_next.RequestStepActionDelegate( (in m3u8_processor_next.RequestStepActionParams p) =>
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
                var responseStepAction = new m3u8_processor_next.ResponseStepActionDelegate( (in m3u8_processor_next.ResponseStepActionParams p) => CONSOLE.WriteLine( $"{p.Part.OrderNumber + 1} of {p.TotalPartCount}, '{p.Part.RelativeUrlName}'" ) );                
                //var downloadPartStepAction = new m3u8_client_next.DownloadPartStepActionDelegate( (in m3u8_client_next.DownloadPartStepActionParams p) => );

                var p = new m3u8_processor_next.DownloadPartsAndSaveInputParams()
                {
                    mc                         = mc,
                    m3u8File                   = m3u8_file_t__v2.Parse( m3u8File ),
                    OutputFileName             = outputFileName,
                    CancellationToken          = ct,
                    //RequestStepAction          = requestStepAction,
                    ResponseStepAction         = responseStepAction,
                    //DownloadPartStepAction     = downloadPartStepAction,
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    DownloadThreadsSemaphore   = dts,
                    DownloadThreadsSemaphore_2 = dts_2,
                    WaitIfPausedEvent          = waitIfPausedEvent,
                    //WaitingIfPaused            = , //public Action
                    //WaitingIfPausedBefore_2    = , //public Action< m3u8_part_ts__v2 >   
                    //WaitingIfPausedAfter_2     = , //public Action< m3u8_part_ts__v2 >   
                    ThrottlerBySpeed           = throttler_by_speed,
                    StreamPool                 = streamPool,
                    RespBufPool                = respBufPool,                    
                };
#if NETCOREAPP
                await m3u8_processor_next.DownloadPartsAndSave_Async( p, requestHeaders ).CAX();
#else
                await m3u8_processor_next.DownloadPartsAndSave( p, requestHeaders ).CAX();
#endif
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
                #region [.set SecurityProtocol to 'Tls + Tls11 + Tls12 + Ssl3'.]
#if NETCOREAPP
                ServicePointManager.SecurityProtocol = (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13);
#else
                ServicePointManager.SecurityProtocol = (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Ssl3);
#endif
                #endregion

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
                    await next1.run( M3U8_FILE_URL, outputFileName, cts.Token, requestHeaders ).CAX(); //.WaitForTaskEndsOrKeyboardBreak( cts );
                }
            }
            catch ( Exception ex )
            {
                CONSOLE.WriteLineError( $"ERROR: {ex}" );
            }
            CONSOLE.WriteLine( "\r\n\r\n[.....finita fusking comedy.....]\r\n\r\n", ConsoleColor.DarkGray );
            CONSOLE.ReadLine();
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
    internal static class CONSOLE
    {
        public static void WriteLine( string text, ConsoleColor? foregroundColor = null )
        {
            lock ( typeof(CONSOLE) )
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
            lock ( typeof(CONSOLE) )
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
    internal struct DefaultConnectionLimitSaver : IDisposable
    {
        private readonly int _DefaultConnectionLimit;
        private DefaultConnectionLimitSaver( int connectionLimit )
        {
            if ( ServicePointManager.DefaultConnectionLimit < connectionLimit )
            {
                _DefaultConnectionLimit = ServicePointManager.DefaultConnectionLimit;
                ServicePointManager.DefaultConnectionLimit = connectionLimit;
            }
            else
            {
                _DefaultConnectionLimit = -1;
            }
        }        
        public void Dispose()
        {
            if ( 0 < _DefaultConnectionLimit )
            {
                ServicePointManager.DefaultConnectionLimit = _DefaultConnectionLimit;
            }
        }

        public static DefaultConnectionLimitSaver Create( int connectionLimit ) => new DefaultConnectionLimitSaver( connectionLimit );
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class _Extensions
    {
        public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
        public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );

        public static bool AnyEx< T >( this IEnumerable< T > seq ) => (seq != null && seq.Any());

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
