using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using X = (string m3u8FileUrl, string requestHeaders, bool autoStartDownload);

namespace m3u8.download.manager.ipc
{
    /// <summary>
    /// 
    /// </summary>
    internal static class BrowserIPC
    {
        /// <summary>
        /// 
        /// </summary>
        [DataContract] private struct ExtensionInputParamsArray
        {
            /// <summary>
            /// 
            /// </summary>
            [DataContract] public struct InputParams
            {
                [DataMember(Name="m3u8_url", IsRequired=true)]
                public string m3u8FileUrl { get; set; }

                [DataMember(Name="requestHeaders", IsRequired=false)]
                public string requestHeaders { get; set; }

                [DataMember(Name="auto_start_download", IsRequired=false)]
                public bool autoStartDownload { get; set; }
            }

            [DataMember(Name= "array", IsRequired=true)]
            public InputParams[] Array { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract] private struct ExtensionOutputParams
        {
            [DataMember(Name="text", IsRequired=true)]
            public string Text { get; set; }

            private string ToJson() => this.ToJSON();

            private const string SUCCESS        = "success";
            private const string MISSING_PARAMS = "missing_params";
            public static string CreateAsJson( bool hasParams ) => (new ExtensionOutputParams() { Text = (hasParams ? SUCCESS : MISSING_PARAMS) }).ToJson();
        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract] public struct ExtensionRequestHeader
        {
            [DataMember(Name="name", IsRequired=true)]
            public string Name { get; set; }

            [DataMember(Name="value", IsRequired=true)]
            public string Value { get; set; }

            public static string ToJson( IDictionary< string, string > dict )
            {
                var json = default(string);
                if ( dict.AnyEx() )
                {
                    json = dict.Select( p => new ExtensionRequestHeader() { Name = p.Key, Value = p.Value } ).ToJSON();                    
                }
                return (json);
            }
            public static bool Try2Dict( string json, out IDictionary< string, string > dict )
            {
                if ( !json.IsNullOrEmpty() )
                {
                    try
                    {
                        var array = Extensions.FromJSON< List< ExtensionRequestHeader > >( json );
                        var group = array.GroupBy( a => a.Name );
                        var sd = new SortedDictionary< string, string >();
                        foreach ( var g in group )
                        {
                            sd.Add( g.Key, g.First().Value );
                        }
                        dict = sd;
                        return (true);
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                    }
                }
                dict = default;
                return (false);
            }
            public static bool Try2Dict2( string json, out IDictionary< string, List< string > > dict )
            {
                if ( !json.IsNullOrEmpty() )
                {
                    try
                    {
                        var array = Extensions.FromJSON< List< ExtensionRequestHeader > >( json );
                        var group = array.GroupBy( a => a.Name );
                        var sd = new SortedDictionary< string, List< string > >();
                        foreach ( var g in group )
                        {
                            sd.Add( g.Key, g.Select( a => a.Value ).OrderBy( _ => _ ).ToList() );
                        }
                        dict = sd;
                        return (true);
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                    }
                }
                dict = default;
                return (false);
            }
        }

        public static string ReadFromStandardInput( int millisecondsDelay  = 2_500 )
        {
            using ( var cts = new CancellationTokenSource( millisecondsDelay ) )
            using ( var br  = new Barrier( 2 ) )
            {
                try
                {                    
                    var task = Task.Run( () =>
                    {
                        br.SignalAndWait();

                        var stdin = Console.OpenStandardInput();

                        var lengthBytes = new byte[ 4 ];
                        stdin.Read( lengthBytes, 0, 4 );
                        var length = BitConverter.ToInt32( lengthBytes, 0 );

                        var buffer = new char[ length ];
                        using ( var reader = new StreamReader( stdin ) )
                        {
                            while ( !reader.EndOfStream && (0 <= reader.Peek()) )
                            {
                                var n = reader.Read( buffer, 0, buffer.Length );
                                if ( n == length )
                                {
                                    break;
                                }
                            }
                        }
                        var res = new string( buffer );
                        return (res);
                    });

                    br.SignalAndWait();

                    task.Wait( cts.Token );

                    return (task.Result);
                }
                catch ( Exception ex ) when (cts.IsCancellationRequested)
                {
                    Debug.WriteLine( ex );
                }

                return (null);
            }
        }
        public static void WriteToStandardOutput( bool hasParams ) => WriteToStandardOutput( ExtensionOutputParams.CreateAsJson( hasParams ) );
        private static void WriteToStandardOutput( string json )
        {
            var stdout = Console.OpenStandardOutput();

            var lengthBytes = BitConverter.GetBytes( json.Length );
            stdout.Write( lengthBytes, 0, lengthBytes.Length );

            var buffer = json.ToCharArray();
            using ( var sw = new StreamWriter( stdout ) )
            {
                sw.Write( buffer, 0, buffer.Length );
            }
        }

        public static bool TryParseAsExtensionInputParams( string json, out X[] p )
        {
            try
            {
                var array = Extensions.FromJSON< ExtensionInputParamsArray >( json ).Array;
                if ( array.AnyEx() )
                {
                    p = new X[ array.Length ];
                    for ( var i = array.Length - 1; 0 <= i; i-- )
                    {
                        ref var a = ref array[ i ];
                        p[ i ] = (a.m3u8FileUrl, a.requestHeaders, a.autoStartDownload);
                    }
                    return (true);
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
            p = default;
            return (false);
        }


        /// <summary>
        /// 
        /// </summary>
        public enum BrowserTypeEnum
        {
            NoBrowser,

            Chrome,
            Firefox,
        }
        /// <summary>
        /// 
        /// </summary>
        public static class CommandLine
        {
            private const string CHROME_PREAMBLE        = "chrome-extension://";
            private const string FIREFOX_FILE_EXTENSION = ".json";

            private const string M3U8_FILE_URL__PARAM       = "m3u8FileUrl=";
            private const string REQUEST_HEADERS__PARAM     = "requestHeaders=";
            private const string AUTO_START_DOWNLOAD__PARAM = "autoStartDownload=";

            private const string LINES_SEPARATOR       = "\0\0";
            private const char   PARAMS_SEPARATOR      = '\0';
            private const string EMPTY_REQUEST_HEADERS = "-";

            public const string CREATE_AS_BREAKAWAY_FROM_JOB__CMD_ARG = "CREATE_AS_BREAKAWAY_FROM_JOB";

            public static BrowserTypeEnum GetBrowserType( string[] args )
            {
                if ( args.Any( a => a.StartsWith( CHROME_PREAMBLE, StringComparison.InvariantCultureIgnoreCase ) ) ) //chrome
                {
                    return  (BrowserTypeEnum.Chrome);
                }

                if ( args.Any( a => File.Exists( a ) && Path.GetExtension( a ).EqualIgnoreCase( FIREFOX_FILE_EXTENSION ) ) ) //firefox => 'E:\_NET2\[m3u8]\m3u8-browser-extensions\_m3u8-downloader-host\m3u8.download.manager.host.ff.json')
                {
                    return (BrowserTypeEnum.Firefox);
                }

                return (BrowserTypeEnum.NoBrowser);
            }

            public static string Create4PipeIPC( X[] array ) => string.Join( LINES_SEPARATOR, from p in array select Create4PipeIPC( p ) );
            private static string Create4PipeIPC( in X p ) => $"{p.m3u8FileUrl}{PARAMS_SEPARATOR}{p.requestHeaders.GetValueIfNotNullOrWhiteSpaceOrDefault( EMPTY_REQUEST_HEADERS )}{PARAMS_SEPARATOR}{p.autoStartDownload}";
            public static bool TryParse4PipeIPC_Multi( string pipeIpcCommandLine, out X[] array )
            {
                if ( pipeIpcCommandLine != null )
                {
                    var args = pipeIpcCommandLine.Split( new[] { LINES_SEPARATOR }, StringSplitOptions.None );
                    if ( 0 < args.Length )
                    {
                        array = new X[ args.Length ];

                        for ( var i = args.Length - 1; 0 <= i; i-- )
                        {
                            if ( !TryParse4PipeIPC_Single( args[ i ], out var p ) )
                            {
                                array = default;
                                return (false);
                            }

                            array[ i ] = p;
                        }
                        return (true);
                    }
                }

                array = default;
                return (false);
            }
            private static bool TryParse4PipeIPC_Single( string pipeIpcCommandLine, out X p )
            {
                var args = pipeIpcCommandLine.Split( PARAMS_SEPARATOR );
                if ( args.Length == 3 )
                {
                    var rh = args[ 1 ]; if ( rh == EMPTY_REQUEST_HEADERS ) rh = null;
                    p = (args[ 0 ], rh, args[ 2 ].Try2Bool());
                    return (!p.m3u8FileUrl.IsNullOrWhiteSpace());
                }
                p = default;
                return (false);
            }

            public static string Create_4_CreateAsBreakawayFromJob( string executeFileName ) => $"\"{executeFileName}\" {CREATE_AS_BREAKAWAY_FROM_JOB__CMD_ARG}";
            public static bool Is_CommandLineArgs_Has__CreateAsBreakawayFromJob() => (Environment.GetCommandLineArgs()?.Any( a => a == CREATE_AS_BREAKAWAY_FROM_JOB__CMD_ARG )).GetValueOrDefault();
            public static bool TryParse( string[] args, out X p )
            {
                p.m3u8FileUrl       = args.TryGetCmdArg( M3U8_FILE_URL__PARAM );
                p.requestHeaders    = args.TryGetCmdArg( REQUEST_HEADERS__PARAM );
                p.autoStartDownload = args.TryGetCmdArg( AUTO_START_DOWNLOAD__PARAM ).Try2Bool();

                return (p.m3u8FileUrl != null);
            }
        }

        private static string TryGetCmdArg( this string[] args, string argName )
        {
            var a1 = args.FirstOrDefault( a => a.StartsWith( argName, StringComparison.InvariantCultureIgnoreCase ) );
            return (a1?.Substring( argName.Length ));
        }
        private static bool Try2Bool( this string s ) => (bool.TryParse( s, out var b ) ? b : default);
    }
}
