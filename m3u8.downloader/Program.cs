using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Windows.Forms;

using m3u8.Properties;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        private struct ExtensionInputParamsArray
        {
            /// <summary>
            /// 
            /// </summary>
            [DataContract]
            public struct InputParams
            {
                [DataMember(Name="m3u8_url", IsRequired=true)]
                public string m3u8FileUrl { get; set; }

                [DataMember(Name="auto_start_download", IsRequired=false)]
                public bool autoStartDownload { get; set; }
            }

            [DataMember(Name= "array", IsRequired=true)]
            public InputParams[] Array { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        private struct ExtensionOutputParams
        {
            [DataMember(Name="text", IsRequired=true)]
            public string Text { get; set; }

            private const string SUCCESS        = "success";
            private const string MISSING_PARAMS = "missing_params";
            public static string CreateAsJson( bool hasParams ) => (new ExtensionOutputParams() { Text = (hasParams ? SUCCESS : MISSING_PARAMS) }).ToJSON();
        }

        private static string ReadStandardInputFromBrowser()
        {
            var stdin = Console.OpenStandardInput();

            var lengthBytes = new byte[ 4 ];
            stdin.Read( lengthBytes, 0, 4 );
            var length = BitConverter.ToInt32( lengthBytes, 0 );

            var buffer = new char[ length ];
            using ( var reader = new StreamReader( stdin ) )
            {
                while ( 0 <= reader.Peek() )
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
        }
        private static void   WriteStandardOutputToBrowser( string json )
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

        private static ExtensionInputParamsArray.InputParams[] ToExtensionInputParams_NoThrow( this string json )
        {
            try
            {
                return (Extensions.FromJSON< ExtensionInputParamsArray >( json ).Array);
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
            return (null);
        }

        private const string CHROME_EXTENSION_PREAMBLE             = "chrome-extension://";
        private const string JSON_EXTENSION                        = ".json";
        private const string COMMAND_LINE_PARAM__m3u8FileUrl       = "m3u8FileUrl=";
        private const string COMMAND_LINE_PARAM__autoStartDownload = "autoStartDownload=";

        private static bool HasAnyWithChromeExtensionPreamble( this string[] args ) => args.Any( a => a.StartsWith( CHROME_EXTENSION_PREAMBLE, StringComparison.InvariantCultureIgnoreCase ) );
        private static bool HasAnyWithFirefoxSign( this string[] args ) => args.Any( a => JSON_EXTENSION.Equals( Path.GetExtension( a ), StringComparison.InvariantCultureIgnoreCase ) && File.Exists( a ) );

        private static string CreateCommandLine( this ExtensionInputParamsArray.InputParams p, string executeFileName ) 
            => $"\"{executeFileName}\" {COMMAND_LINE_PARAM__m3u8FileUrl}\"{p.m3u8FileUrl}\" {COMMAND_LINE_PARAM__autoStartDownload}\"{p.autoStartDownload}\"";
        private static string CreateCommandLine( this ExtensionInputParamsArray.InputParams p ) 
                                  => $"{COMMAND_LINE_PARAM__m3u8FileUrl}\"{p.m3u8FileUrl}\" {COMMAND_LINE_PARAM__autoStartDownload}\"{p.autoStartDownload}\"";

        private static string TryGetCmdArg( this string[] args, string argName ) => args.FirstOrDefault( a => a.StartsWith( argName ) )?.Substring( argName.Length );
        private static bool Try2Bool( this string s ) => (bool.TryParse( s, out var b ) ? b : default);

        /// <summary>
        ///
        /// </summary>
        [STAThread]
        private static void Main( string[] args )
        {
            #region [.exception handling.]
            Application.ThreadException                  += (s, e) => e.Exception.MessageBox_ShowError( "Application.ThreadException" ); ;
            AppDomain  .CurrentDomain.UnhandledException += (s, e) => Extensions.MessageBox_ShowError( e.ExceptionObject.ToString(), " AppDomain.CurrentDomain.UnhandledException" );
            Application.SetUnhandledExceptionMode( UnhandledExceptionMode.Automatic, true );
            #endregion

            #region [.parse if opened from 'chrome-extension' || 'firefox-extension'.]
            var inputParams = default((string m3u8FileUrl, bool autoStartDownload));

            if ( args.HasAnyWithChromeExtensionPreamble() ) //chrome
            {
                var text = ReadStandardInputFromBrowser();
                if ( !text.IsNullOrWhiteSpace() )
                {
                    var array = text.ToExtensionInputParams_NoThrow();
                    WriteStandardOutputToBrowser( ExtensionOutputParams.CreateAsJson( array.AnyEx() ) );

                    if ( array.AnyEx() )
                    {
                        ref var p0 = ref array[ 0 ];
                        inputParams = (p0.m3u8FileUrl, p0.autoStartDownload);

                        if ( 1 < array.Length )
                        {
                            var executeFileName = Process.GetCurrentProcess().MainModule.FileName;
                            foreach ( var p in array.Skip( 1 ) )
                            {
                                var arguments = p.CreateCommandLine();
                                Process.Start( executeFileName, arguments );
                            }
                        }
                    }
                }
            }
            else
            if ( args.HasAnyWithFirefoxSign() ) //firefox => 'E:\_NET2\[m3u8]\m3u8-browser-extensions\_m3u8-downloader-host\m3u8.downloader.host.ff.json')
            {
                var text = ReadStandardInputFromBrowser();
                if ( !text.IsNullOrWhiteSpace() )
                {
                    var array = text.ToExtensionInputParams_NoThrow();
                    WriteStandardOutputToBrowser( ExtensionOutputParams.CreateAsJson( array.AnyEx() ) );

                    if ( array.AnyEx() )
                    {
                        var executeFileName = Process.GetCurrentProcess().MainModule.FileName;
                        foreach ( var p in array )
                        {
                            var cmdLine = p.CreateCommandLine( executeFileName );

                            var succeeds = ProcessCreator.CreateAsBreakawayFromJob( cmdLine );
                            if ( !succeeds )
                            {
                                MessageBox.Show( "Error while trying run additional native application's process", executeFileName, MessageBoxButtons.OK, MessageBoxIcon.Error );
                            }
                        }
                        return;
                    }                    
                }
            }
            else
            {
                inputParams.m3u8FileUrl       = args.TryGetCmdArg( COMMAND_LINE_PARAM__m3u8FileUrl       );
                inputParams.autoStartDownload = args.TryGetCmdArg( COMMAND_LINE_PARAM__autoStartDownload ).Try2Bool();
            }
            #endregion

            #region [.check for upgrade user-settings for new version & copy user-settings from previous application version if necessary.]
            Settings.Default.UpgradeIfNeed();
            #endregion

            #region [.set SecurityProtocol to 'Tls + Tls11 + Tls12 + Ssl3'.]
            ServicePointManager.SecurityProtocol = (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Ssl3);
            #endregion

            #region [.goto to tu-tu.]
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new MainForm( in inputParams ) );
            #endregion
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class ProcessCreator
    {
        private const uint CREATE_BREAKAWAY_FROM_JOB = 0x01000000;

        public static bool CreateAsBreakawayFromJob( string cmdLine )
        {
            var si = new STARTUPINFO() { cb = Marshal.SizeOf(typeof(STARTUPINFO)) };

            var r = CreateProcess( null,
                                   cmdLine,
                                   IntPtr.Zero,
                                   IntPtr.Zero,
                                   false, 
                                   CREATE_BREAKAWAY_FROM_JOB, 
                                   IntPtr.Zero, 
                                   null,
                                   ref si, 
                                   out var processInformation );
            return (r);
        }

        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION 
        {
           public IntPtr hProcess;
           public IntPtr hThread;
           public int dwProcessId;
           public int dwThreadId;
        }

        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        private struct STARTUPINFO
        {
             public Int32 cb;
             public string lpReserved;
             public string lpDesktop;
             public string lpTitle;
             public Int32 dwX;
             public Int32 dwY;
             public Int32 dwXSize;
             public Int32 dwYSize;
             public Int32 dwXCountChars;
             public Int32 dwYCountChars;
             public Int32 dwFillAttribute;
             public Int32 dwFlags;
             public Int16 wShowWindow;
             public Int16 cbReserved2;
             public IntPtr lpReserved2;
             public IntPtr hStdInput;
             public IntPtr hStdOutput;
             public IntPtr hStdError;
        }

        [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
        private static extern bool CreateProcess( string lpApplicationName,
                                                  string lpCommandLine,
                                                  IntPtr lpProcessAttributes,
                                                  IntPtr lpThreadAttributes,
                                                  bool bInheritHandles,
                                                  uint dwCreationFlags,
                                                  IntPtr lpEnvironment,
                                                  string lpCurrentDirectory,
                                                  [In] ref STARTUPINFO lpStartupInfo,
                                                  out PROCESS_INFORMATION lpProcessInformation );
    }
}
