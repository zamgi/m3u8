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
                var downloadPartsSet = new SortedSet< m3u8_part_ts >( default(m3u8_part_ts_comparer) );

                using ( DefaultConnectionLimitSaver.Create( maxDegreeOfParallelism ) )
                {                 
                    Parallel.ForEach( m3u8_file.Parts, new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism, CancellationToken = ct }, 
                    (part, loopState, idx) =>
                    {
                        #region comm
                        //if ( ct.IsCancellationRequested )
                        //{
                        //    loopState.Stop();
                        //    return;
                        //} 
                        #endregion

                        var n = Interlocked.Increment( ref globalPartNumber );                
                        try
                        { 
                            CONSOLE.WriteLine( $"#{n} of {totalPatrs}). '{part.RelativeUrlName}'..." );
                            var downloadPart = mc.DownloadPart( part, baseAddress, ct ).Result;
                            if ( (downloadPart.Error != null) && !ct.IsCancellationRequested )
                            {
                                CONSOLE.WriteLineError( $"#{n} of {totalPatrs}). FAILED: {downloadPart.Error}{Environment.NewLine}" );
                            }
                            #region comm
                            /*else
                            {
                                CONSOLE.WriteLine( $"#{n} of {totalPatrs}). success: {downloadPart.Bytes.Length}{Environment.NewLine}" ); 
                            }*/ 
                            #endregion

                            lock ( downloadPartsSet )
                            {
                                downloadPartsSet.Add( downloadPart );
                            }

                            #region commented
                            /*
                            var cfn = PathnameCleaner.CleanFilename( exportResult.FileName );
                            var ext = Path.GetExtension( cfn );
                            var fn_no_ext = Path.GetFileNameWithoutExtension( cfn );
                            var fn = (ext.IsNullOrWhiteSpace() || fn_no_ext.IsNullOrWhiteSpace()) 
                                        ? TrimIfGreatThen( cfn )
                                        : TrimIfGreatThen( fn_no_ext ) + ext;
                            fn = fn_no_ext.StartsWith( part.document.name, StringComparison.InvariantCultureIgnoreCase ) ? fn : $"name={TrimIfGreatThen( part.document.name )}, {fn}";
                            var filename = PathnameCleaner.CleanFilename( $"dId={part.document.id}, {fn}" );
                            var fullFilename = Path.Combine( part.projectDirectory, filename );
                            File.WriteAllBytes( fullFilename, exportResult.Bytes );
                            */ 
                            #endregion
                        }
                        catch ( Exception ex )
                        {
                            #region [.code.]
                            var aex = ex as AggregateException;
                            if ( (aex == null) || !aex.InnerExceptions.All( (_ex) => (_ex is OperationCanceledException /*TaskCanceledException*/) ) )
                            {
                                CONSOLE.WriteLineError( "ERROR: " + ex );

                                #region commented
                                /*
                                try
                                {
                                    var filename = PathnameCleaner.CleanFilename( $"error, part={part.RelativeUrlName}.error.txt" );
                                    var fullFilename = Path.Combine( part.projectDirectory, filename );
                                    File.WriteAllText( fullFilename, ex.ToString() );
                                }
                                catch ( Exception exx )
                                {
                                    Debug.WriteLine( exx );
                                }
                                */ 
                                #endregion
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
                using ( var mc = m3u8_client.Create() )
                using ( var cts = new CancellationTokenSource() )
                {
                    var task = Task.Run( async () =>
                    {
                        var sw = Stopwatch.StartNew();

                        //-1-//
                        CONSOLE.WriteLine( $"download m3u8-file: '{M3U8_FILE_URL}'..." );
                        #region commented
                        /*
                        var baseAddress = new Uri( "http://hls.kinokrad.co/hls/terminator-4-da-pridet-spasitel-2009_HDRip/" );
                        var m3u8FileName = @"E:\[.m3u8]\playlist.m3u8--.txt";
                        var content = File.ReadAllText( m3u8FileName );
                        var m3u8File = m3u8_file_t.Parse( content, baseAddress );
                        */ 
                        #endregion
                        var m3u8FileUrl = new Uri( M3U8_FILE_URL );
                        var m3u8File = await mc.DownloadFile( m3u8FileUrl, cts.Token );
                        CONSOLE.WriteLine( $"success. parts count: {m3u8File.Parts.Count}\r\n" );

                        //-2-//
                        var downloadParts = download_m3u8File_parallel( mc, m3u8File, cts );

                        //-3-//
                        //---var outputFileName = Path.Combine( OUTPUT_FILE_DIR, PathnameCleaner.CleanPathnameAndFilename( m3u8FileUrl.Segments.Last() ) + OUTPUT_FILE_EXT );
                        var outputFileName = Path.Combine( OUTPUT_FILE_DIR, PathnameCleaner.CleanPathnameAndFilename( m3u8FileUrl.AbsolutePath ).TrimStart( '-' ) + OUTPUT_FILE_EXT );
                        using ( var fs = File.OpenWrite( outputFileName ) )
                        {
                            fs.SetLength( 0 );

                            foreach ( var downloadPart in downloadParts )
                            {
                                if ( downloadPart.Error != null ) //|| downloadPart.Bytes == null )
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

                    }/*, cts.Token*/ )
                    .ContinueWith( (continuationTask) =>
                    {
                        if ( continuationTask.IsFaulted )
                        {
                            CONSOLE.WriteLineError( "ERROR: " + continuationTask.Exception );
                        }
                    } );

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

                var __parts__ = m3u8_file.Parts;//.Take( 15 ).ToArray();

                var expectedPartNumber = __parts__.FirstOrDefault().OrderNumber;
                var maxPartNumber      = __parts__.LastOrDefault ().OrderNumber;
                var sourceQueue = new Queue< m3u8_part_ts >( __parts__ );
                var downloadPartsSet = new SortedSet< m3u8_part_ts >( default(m3u8_part_ts_comparer) );
                var downloadPartsResult = new LinkedList< m3u8_part_ts >();

                using ( var canExtractPartEvent = new AutoResetEvent( false ) )
                using ( var semaphore = new SemaphoreSlim( maxDegreeOfParallelism ) )                
                using ( DefaultConnectionLimitSaver.Create( maxDegreeOfParallelism ) )
                {
                    var task_download = Task.Run( () =>
                    {
                        for ( ; sourceQueue.Count != 0; )
                        {
                            semaphore.Wait();
                            var part = sourceQueue.Dequeue();

                            CONSOLE.WriteLine( $"start download part: {part}..." );

                            mc.DownloadPart( part, baseAddress, ct )
                              .ContinueWith( (continuationTask) =>
                              {
                                  if ( continuationTask.IsFaulted )
                                  {
                                      Interlocked.Increment( ref expectedPartNumber );

                                      CONSOLE.WriteLine( $"'{continuationTask.Exception.GetType().Name}': '{continuationTask.Exception.Message}'.", ConsoleColor.Red );
                                  }
                                  else if ( !continuationTask.IsCanceled )
                                  {
                                      Interlocked.Increment( ref successPartNumber );

                                      var downloadPart = continuationTask.Result;

                                      CONSOLE.WriteLine( $"end download part: {downloadPart}." );

                                      lock ( downloadPartsSet )
                                      {
                                          downloadPartsSet.Add( downloadPart );
                                          canExtractPartEvent.Set();
                                      }
                                  }
                              } );
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

            public static void run__v1( string M3U8_FILE_URL, string OUTPUT_FILE_DIR, string OUTPUT_FILE_EXT )
            {
                using ( var mc = m3u8_client.Create() )
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
                        using ( var fs = File.OpenWrite( outputFileName ) )
                        {
                            fs.SetLength( 0 );

                            foreach ( var downloadPart in downloadParts )
                            {
                                if ( downloadPart.Error != null ) //|| downloadPart.Bytes == null )
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

                    }/*, cts.Token*/ )
                    .ContinueWith( (continuationTask) =>
                    {
                        if ( continuationTask.IsFaulted )
                        {
                            CONSOLE.WriteLineError( "ERROR: " + continuationTask.Exception );
                        }
                    } );

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
                var downloadPartsSet   = new SortedSet< m3u8_part_ts >( default(m3u8_part_ts_comparer) );

                using ( var canExtractPartEvent = new AutoResetEvent( false ) )
                using ( var semaphore = new SemaphoreSlim( maxDegreeOfParallelism ) )                
                using ( DefaultConnectionLimitSaver.Create( maxDegreeOfParallelism ) )
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
                              .ContinueWith( (continuationTask) =>
                              {
                                  if ( continuationTask.IsFaulted )
                                  {
                                      Interlocked.Increment( ref expectedPartNumber );

                                      CONSOLE.WriteLine( $"'{continuationTask.Exception.GetType().Name}': '{continuationTask.Exception.Message}'.", ConsoleColor.Red );
                                  }
                                  else if ( !continuationTask.IsCanceled )
                                  {
                                      Interlocked.Increment( ref successPartNumber );

                                      var downloadPart = continuationTask.Result;
#if DEBUG
                                      CONSOLE.WriteLine( $"end download part: {downloadPart}." ); 
#endif
                                      lock ( downloadPartsSet )
                                      {
                                          downloadPartsSet.Add( downloadPart );
                                          canExtractPartEvent.Set();
                                      }
                                  }
                              } );
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

            public static void run__v2( string M3U8_FILE_URL, string OUTPUT_FILE_DIR, string OUTPUT_FILE_EXT )
            {
                using ( var mc = m3u8_client.Create() )
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
                        //var dpn = -1;

                        var outputFileName = Path.Combine( OUTPUT_FILE_DIR, PathnameCleaner.CleanPathnameAndFilename( m3u8FileUrl.AbsolutePath ).TrimStart( '-' ) + OUTPUT_FILE_EXT );
                        using ( var fs = File.OpenWrite( outputFileName ) )
                        {
                            fs.SetLength( 0 );

                            foreach ( var downloadPart in downloadParts )
                            {
                                #region comm
                                //if ( dpn < downloadPart.OrderNumber )
                                //{
                                //    dpn = downloadPart.OrderNumber;
                                //}
                                //else
                                //{
                                //    Debugger.Break();
                                //} 
                                #endregion

                                if ( downloadPart.Error != null ) //|| downloadPart.Bytes == null )
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

                    }/*, cts.Token*/ )
                    .ContinueWith( (continuationTask) =>
                    {
                        if ( continuationTask.IsFaulted )
                        {
                            CONSOLE.WriteLineError( "ERROR: " + continuationTask.Exception );
                        }
                    } );

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
                    {
                        var msg = $"{t.Part.OrderNumber} of {t.TotalPartCount}, '{t.Part.RelativeUrlName}'";
                        CONSOLE.WriteLine( msg );
                        //if ( t.Error != null )
                        //{
                        //    CONSOLE.WriteLine( msg + $" => {t.Error.Message}", ConsoleColor.Red );
                        //}
                        //else
                        //{
                        //    CONSOLE.WriteLine( msg ); 
                        //}
                    }),
                };

                await m3u8_processor.DownloadFileAndSave_Async( p );
            }
        }

        [STAThread]
        private static void Main( string[] args )
        {
            try
            {
                var M3U8_FILE_URL   = ConfigurationManager.AppSettings[ "M3U8_FILE_URL"   ]; if ( M3U8_FILE_URL  .IsNullOrWhiteSpace() ) throw (new ArgumentNullException( nameof(M3U8_FILE_URL) ));
                var OUTPUT_FILE_DIR = ConfigurationManager.AppSettings[ "OUTPUT_FILE_DIR" ]; if ( OUTPUT_FILE_DIR.IsNullOrWhiteSpace() ) OUTPUT_FILE_DIR = @"E:\\";
                var OUTPUT_FILE_EXT = ConfigurationManager.AppSettings[ "OUTPUT_FILE_EXT" ]; if ( OUTPUT_FILE_EXT.IsNullOrWhiteSpace() ) OUTPUT_FILE_EXT = ".avi";

                //---v1.run( M3U8_FILE_URL, OUTPUT_FILE_DIR, OUTPUT_FILE_EXT );
                //v2.run__v1( M3U8_FILE_URL, OUTPUT_FILE_DIR, OUTPUT_FILE_EXT );
                //v2.run__v2( M3U8_FILE_URL, OUTPUT_FILE_DIR, OUTPUT_FILE_EXT );

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
            lock (typeof(CONSOLE))
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
            lock (typeof(CONSOLE))
            {
                var fc = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( Environment.NewLine + text );
                Console.ForegroundColor = fc;
            }
        }

        public static string ReadLine()
        {
            return (Console.ReadLine());
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

    /// <summary>
    /// 
    /// </summary>
    internal static class _Extensions
    {
        public static bool IsNullOrEmpty( this string s )
        {
            return (string.IsNullOrEmpty( s ));
        }
        public static bool IsNullOrWhiteSpace( this string s )
        {
            return (string.IsNullOrWhiteSpace( s ));
        }

        public static bool AnyEx< T >( this IEnumerable< T > seq )
        {
            return (seq != null && seq.Any());
        }

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
