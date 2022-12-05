using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Program
    {
        private static async Task Main( string[] args )
        {
            try
            {
                var M3U8_URL                   = args.GetArg( "url=" ) ?? ConfigurationManager.AppSettings[ "M3U8_URL" ];
                var OUTPUT_FILENAME            = args.GetArg( "out=" ) ?? args.GetArg( "output=" ) ?? ConfigurationManager.AppSettings[ "OUTPUT_FILENAME" ];
                var MAX_OUTPUT_FILE_SZIE_IN_MB = (int.TryParse( (args.GetArg( "size=" ) ?? ConfigurationManager.AppSettings[ "MAX_OUTPUT_FILE_SZIE_IN_MB" ]), out var i ) ? i : 250);
                //-------------------------------------------------------------------//

                using var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) => cts.Cancel( e );
                //-------------------------------------------------------------------//

                $"Start download from M3U8_URL: '{M3U8_URL}'".ToConsole( to_title: true );
                $"            (OUTPUT_FILENAME: '{OUTPUT_FILENAME}')".ToConsole();
                $" (MAX_OUTPUT_FILE_SZIE_IN_MB: '{MAX_OUTPUT_FILE_SZIE_IN_MB:0,0}')".ToConsole();
                $"---------------------------------------------\r\n".ToConsole();

                var p = new m3u8_live_stream_downloader.InitParams()
                { 
                    M3u8Url        = M3U8_URL,
                    OutputFileName = OUTPUT_FILENAME,

                    DownloadContent      = (p)                        => $"[ QUEUEED]: {p}".ToConsole( cts ),
                    DownloadContentError = (p, ex)                    => $"[ QUEUEED]: {p} => {ex}".ToConsole( ConsoleColor.Red, cts ),
                    DownloadPart         = (p, partBytes, totalBytes) => $"[DOWNLOAD]: {p} => ok. (part-size: {(1.0 * partBytes / 1024):N2} KB, total-size: {(1.0 * totalBytes / (1024 * 1024)):N2} MB)".ToConsole( cts ),
                    DownloadPartError    = (p, ex)                    => $"[DOWNLOAD]: {p} => {ex}".ToConsole( ConsoleColor.Red, cts ),
                    DownloadCreateOutputFile = (fn)                   => $"Created output file: '{fn}'".ToConsole( cts )
                };

                var max_output_file_size = MAX_OUTPUT_FILE_SZIE_IN_MB * (1024 * 1024);

                using var m = new m3u8_live_stream_downloader( p );
                await m.Download( cts.Token, max_output_file_size ).CAX();
                //-------------------------------------------------------------------//
            }
            catch ( Exception ex )
            {
                ex.ToConsole();
            }
            //-----------------------------------------------------------//
            $"\r\n[.....finita.....]".ToConsole( ConsoleColor.DarkGray, to_title: true );
            Console.ReadLine();
        }        
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static string GetArg( this string[] args, string argName )
        {
            if ( !argName.EndsWith( "=" ) ) argName += '=';
            return args?.Where( a => a.StartsWith( argName, StringComparison.InvariantCultureIgnoreCase ) ).FirstOrDefault()?.Substring( argName.Length );
        }
        public static void Cancel( this CancellationTokenSource cts, ConsoleCancelEventArgs e )
        {
            e.Cancel = true;
            cts.Cancel();
        }

        public static void ToConsole( this string msg, CancellationTokenSource cts = null, bool to_title = false )
        {
            if ( (cts == null) || !cts.IsCancellationRequested )
            {
                Console.WriteLine( msg );
                if ( to_title ) Console.Title = msg;
            }
        }
        public static void ToConsole( this string msg, ConsoleColor foregroundColor, CancellationTokenSource cts = null, bool to_title = false )
        {
            if ( (cts == null) || !cts.IsCancellationRequested )
            {
                var fc = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor;
                Console.WriteLine( msg );
                Console.ForegroundColor = fc;
                if ( to_title ) Console.Title = msg;
            }
        }
        public static void ToConsole( this Exception ex ) => (Environment.NewLine + ex.ToString()).ToConsole( ConsoleColor.DarkRed );
    }
}
