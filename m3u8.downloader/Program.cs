using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
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
        private struct ChromeExtensionInputParams
        {
            [DataMember(Name="m3u8_url", IsRequired=true)]
            public string m3u8FileUrl { get; set; }

            [DataMember(Name="auto_start_download", IsRequired=false)]
            public bool autoStartDownload { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        private struct ChromeExtensionOutputParams
        {
            [DataMember(Name="text", IsRequired=true)]
            public string Text { get; set; }

            private string ToJson()
            {
                using ( var ms = new MemoryStream() )
                {
                    var ser = new DataContractJsonSerializer( typeof(ChromeExtensionOutputParams) );
                    ser.WriteObject( ms, this );
                    var json = Encoding.UTF8.GetString( ms.ToArray() );
                    return (json);
                }
            }

            private const string SUCCESS        = "success";
            private const string MISSING_PARAMS = "missing_params";
            public static string CreateAsJson( bool hasParams ) => (new ChromeExtensionOutputParams() { Text = (hasParams ? SUCCESS : MISSING_PARAMS) }).ToJson();
        }

        private static string ReadStandardInputFromChrome()
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
                    reader.Read( buffer, 0, buffer.Length );
                }
            }
            var res = new string( buffer );
            return (res);
        }
        private static void   WriteStandardOutputToChrome( string json )
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

        private static ChromeExtensionInputParams ToChromeExtensionInputParams( this string json )
        {
            using ( var ms = new MemoryStream( Encoding.UTF8.GetBytes( json ) ) )
            {
                var ser = new DataContractJsonSerializer( typeof(ChromeExtensionInputParams) );
                var chromeExtensionParams = (ChromeExtensionInputParams) ser.ReadObject( ms );
                return (chromeExtensionParams);
            }
        }
        private static ChromeExtensionInputParams? ToChromeExtensionInputParams_NoThrow( this string json )
        {
            try
            {
                return (json.ToChromeExtensionInputParams());
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                return (null);
            }
        }

        /// <summary>
        ///
        /// </summary>
        [STAThread]
        private static void Main( string[] args )
        {
            Application.ThreadException                += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.SetUnhandledExceptionMode( UnhandledExceptionMode.Automatic, true );

            #region [.parse if opened from 'chrome-extension'.]
            var m3u8FileUrl       = default(string);
            var autoStartDownload = default(bool);
            if ( args.Any( a => a.StartsWith( "chrome-extension://", StringComparison.InvariantCultureIgnoreCase ) ) ) //chrome
            {
                var text = ReadStandardInputFromChrome();
                if ( !text.IsNullOrWhiteSpace() )
                {
                    var p = text.ToChromeExtensionInputParams_NoThrow();
                    if ( p.HasValue )
                    {
                        m3u8FileUrl       = p.Value.m3u8FileUrl;
                        autoStartDownload = p.Value.autoStartDownload;  
                    }

                    WriteStandardOutputToChrome( ChromeExtensionOutputParams.CreateAsJson( p.HasValue ) );
                }
            }
            else
            if ( args.Any( a => File.Exists( a ) && (Path.GetExtension( a ) == ".json") ) ) //firefox => 'E:\_NET2\[m3u8]\m3u8-browser-extensions\_m3u8-downloader-host\m3u8.downloader.host.ff.json')
            {
                var text = ReadStandardInputFromChrome();
                if ( !text.IsNullOrWhiteSpace() )
                {
                    var p = text.ToChromeExtensionInputParams_NoThrow();
                    if ( p.HasValue )
                    {
                        m3u8FileUrl       = p.Value.m3u8FileUrl;
                        autoStartDownload = p.Value.autoStartDownload;

                        WriteStandardOutputToChrome( ChromeExtensionOutputParams.CreateAsJson( true ) );

                        var cmdLine = $"\"{Process.GetCurrentProcess().MainModule.FileName}\" {nameof(m3u8FileUrl)}=\"{m3u8FileUrl}\" {nameof(autoStartDownload)}=\"{autoStartDownload}\"";
                        var succeeds = ProcessCreator.ReCreateCurrentProcess( cmdLine );
                        if ( !succeeds )
                        {
                            MessageBox.Show( "Error while trying run additional native application's process", Process.GetCurrentProcess().MainModule.FileName, MessageBoxButtons.OK, MessageBoxIcon.Error );
                        }
                        return;
                    }
                    else
                    {
                        WriteStandardOutputToChrome( ChromeExtensionOutputParams.CreateAsJson( false ) );
                    }
                }
            }
            else
            {
                m3u8FileUrl       = args.TryGetCmdArg( nameof(m3u8FileUrl) + '=' );
                autoStartDownload = args.TryGetCmdArg( nameof(autoStartDownload) + '=' ).Try2Bool();
            }
            #endregion

            #region [.check for upgrade user-settings for new version.]
            // Copy user settings from previous application version if necessary
            if ( !Settings.Default._IsUpgradedInThisVersion )
            {
                Settings.Default.Upgrade();
                Settings.Default._IsUpgradedInThisVersion = true;
                Settings.Default.SaveNoThrow();
            }
            #endregion

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new MainForm( m3u8FileUrl, autoStartDownload ) );
        }

        private static string TryGetCmdArg( this string[] args, string argName )
        {
            var a1 = args.FirstOrDefault( a => a.StartsWith( argName ) );
            return (a1?.Substring( argName.Length ));
        }
        private static bool Try2Bool( this string s ) => bool.TryParse( s, out var b ) ? b : default;

        private static void Application_ThreadException( object sender, ThreadExceptionEventArgs e ) => e.Exception.MessageBox_ShowError( "Application.ThreadException" );
        private static void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e ) => Extensions.MessageBox_ShowError( e.ExceptionObject.ToString(), " AppDomain.CurrentDomain.UnhandledException" );
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class ProcessCreator
    {
        public static bool ReCreateCurrentProcess( string cmdLine )
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
                                   out var pROCESS_INFORMATION );
            return (r);
        }


        private const uint CREATE_BREAKAWAY_FROM_JOB = 0x01000000;

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION 
        {
           public IntPtr hProcess;
           public IntPtr hThread;
           public int dwProcessId;
           public int dwThreadId;
        }

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
