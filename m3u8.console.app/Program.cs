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
        [STAThread]
        private static void Main( string[] args )
        {
            try
            {
                var M3U8_FILE_URL   = ConfigurationManager.AppSettings[ "M3U8_FILE_URL"   ]; if ( M3U8_FILE_URL  .IsNullOrWhiteSpace() ) throw (new ArgumentNullException( nameof(M3U8_FILE_URL) ));
                var OUTPUT_FILE_DIR = ConfigurationManager.AppSettings[ "OUTPUT_FILE_DIR" ]; if ( OUTPUT_FILE_DIR.IsNullOrWhiteSpace() ) OUTPUT_FILE_DIR = Environment.CurrentDirectory;
                var OUTPUT_FILE_EXT = ConfigurationManager.AppSettings[ "OUTPUT_FILE_EXT" ]; if ( OUTPUT_FILE_EXT.IsNullOrWhiteSpace() ) OUTPUT_FILE_EXT = ".avi";
                //----------------------------------------------------------------------------------//

                CONSOLE.WriteLine( $"M3U8-FILE-URL: '{M3U8_FILE_URL}'..." );
                CONSOLE.WriteLine( "(press 'escape' for cancel)\r\n", ConsoleColor.DarkGray );

                using ( var cts = new CancellationTokenSource() )
                {
                    var outputFileName = Path.Combine( OUTPUT_FILE_DIR, PathnameCleaner.CleanPathnameAndFilename( M3U8_FILE_URL ).TrimStart( '-' ) + OUTPUT_FILE_EXT );

                    var p = new m3u8_processor.DownloadFileAndSaveInputParams()
                    {
                        Cts                    = cts,
                        m3u8FileUrl            = M3U8_FILE_URL,
                        OutputFileName         = outputFileName,
                        MaxDegreeOfParallelism = 64,
                        NetParams              = new m3u8_client.init_params()
                        {
                            AttemptRequestCount = 3,
                            Timeout             = TimeSpan.FromSeconds( 30 ),
                            ConnectionClose     = false,
                        },
                        StepAction = new m3u8_processor.StepActionDelegate( ( t ) =>
                        {
                            var msg = $"{t.PartOrderNumber} of {t.TotalPartCount}, '{t.Part.RelativeUrlName}'";
                            if ( t.Error != null )
                            {
                                CONSOLE.WriteLineError( msg + $" => {t.Error.Message}" );
                            }
                            else
                            {
                                CONSOLE.WriteLine( msg );
                            }
                        } ),
                    };

                    var res = m3u8_processor.DownloadFileAndSave_Async( p ).WaitForTaskEndsOrKeyboardBreak( cts );

                    CONSOLE.WriteLine( $"\r\nM3U8-FILE-URL: '{res.m3u8FileUrl}'" );
                    CONSOLE.WriteLine( $"OutputFileName: '{res.OutputFileName}'" );
                    CONSOLE.WriteLine( $"OutputFileName-Size: {(res.TotalBytes >> 20)}mb" );
                    CONSOLE.WriteLine( $"Total-Parts: {res.TotalParts}, Success: {res.PartsSuccessCount}, Error: {res.PartsErrorCount}" );
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
        public static T WaitForTaskEndsOrKeyboardBreak< T >( this Task< T > task, CancellationTokenSource cts )
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
            return (task.Result);
        }
    }
}
