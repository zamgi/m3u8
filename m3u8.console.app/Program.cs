using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
                var downloadPartsSet = new SortedSet< m3u8_part_ts >( default(m3u8_part_ts.comparer) );

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
                                           $"(elapsed: {sw.Elapsed}, file: '{outputFileName}', size: {(totalBytes >> 20).ToString( "0,0" )} mb)\r\n" );

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
                var ct = (cts?.Token).GetValueOrDefault( CancellationToken.None );
                var baseAddress = m3u8_file.BaseAddress;
                var totalPatrs  = m3u8_file.Parts.Count;
                var successPartNumber = 0;

                var parts = m3u8_file.Parts;

                var expectedPartNumber  = parts.FirstOrDefault().OrderNumber;
                var maxPartNumber       = parts.LastOrDefault ().OrderNumber;
                var sourceQueue         = new Queue< m3u8_part_ts >( parts );
                var downloadPartsSet    = new SortedSet< m3u8_part_ts >( default(m3u8_part_ts.comparer) );
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
                                           $"(elapsed: {sw.Elapsed}, file: '{outputFileName}', size: {(totalBytes >> 20).ToString( "0,0" )} mb)\r\n" );

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
                var ct = (cts?.Token).GetValueOrDefault( CancellationToken.None );
                var baseAddress = m3u8_file.BaseAddress;
                var totalPatrs  = m3u8_file.Parts.Count;
                var successPartNumber = 0;

                var expectedPartNumber = m3u8_file.Parts.FirstOrDefault().OrderNumber;
                var maxPartNumber      = m3u8_file.Parts.LastOrDefault ().OrderNumber;
                var sourceQueue        = new Queue< m3u8_part_ts >( m3u8_file.Parts );
                var downloadPartsSet   = new SortedSet< m3u8_part_ts >( default(m3u8_part_ts.comparer) );

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
                                           $"(elapsed: {sw.Elapsed}, file: '{outputFileName}', size: {(totalBytes >> 20).ToString( "0,0" )} mb)\r\n" );

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
            public static async Task run( string m3u8FileUrl, string outputFileName, CancellationTokenSource cts )
            {
                var p = new m3u8_processor.DownloadFileAndSaveInputParams()
                {
                    Cts                = cts,
                    m3u8FileUrl        = m3u8FileUrl,
                    OutputFileName     = outputFileName,
                    ResponseStepAction = new m3u8_processor.ResponseStepActionDelegate( t =>
                        CONSOLE.WriteLine( $"{t.Part.OrderNumber} of {t.TotalPartCount}, '{t.Part.RelativeUrlName}'" )
                    ),
                };

                await m3u8_processor.DownloadFileAndSave_Async( p );
            }
        }

        [STAThread]
        private static void Main( string[] args )
        {
            try
            {
                #region [.set SecurityProtocol to 'Tls + Tls11 + Tls12 + Ssl3'.]
                ServicePointManager.SecurityProtocol = (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Ssl3);
                #endregion

                var M3U8_FILE_URL   = ConfigurationManager.AppSettings[ "M3U8_FILE_URL"   ]; if ( M3U8_FILE_URL  .IsNullOrWhiteSpace() ) throw (new ArgumentNullException( nameof(M3U8_FILE_URL) ));
                var OUTPUT_FILE_DIR = ConfigurationManager.AppSettings[ "OUTPUT_FILE_DIR" ]; if ( OUTPUT_FILE_DIR.IsNullOrWhiteSpace() ) OUTPUT_FILE_DIR = @"E:\\";
                var OUTPUT_FILE_EXT = ConfigurationManager.AppSettings[ "OUTPUT_FILE_EXT" ]; if ( OUTPUT_FILE_EXT.IsNullOrWhiteSpace() ) OUTPUT_FILE_EXT = ".avi";

                //v1.run( M3U8_FILE_URL, OUTPUT_FILE_DIR, OUTPUT_FILE_EXT );
                //v2.run__1( M3U8_FILE_URL, OUTPUT_FILE_DIR, OUTPUT_FILE_EXT );
                //v2.run__2( M3U8_FILE_URL, OUTPUT_FILE_DIR, OUTPUT_FILE_EXT );

                using ( var cts = new CancellationTokenSource() )
                {
                    var outputFileName = Path.Combine( OUTPUT_FILE_DIR, PathnameCleaner.CleanPathnameAndFilename( M3U8_FILE_URL ).TrimStart( '-' ) + OUTPUT_FILE_EXT );
                    v3.run( M3U8_FILE_URL, outputFileName, cts ).WaitForTaskEndsOrKeyboardBreak( cts );
                }                    
            }
            catch ( Exception ex )
            {
                CONSOLE.WriteLineError( "ERROR: " + ex );
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
            , char   replacedNameChar = '-' )
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
                pathnameAndFilename = sb.ToString();
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
