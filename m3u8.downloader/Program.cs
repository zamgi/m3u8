using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private struct ChromeExtensionParams
        {
            [DataMember(Name="m3u8_url", IsRequired=true)]
            public string m3u8FileUrl { get; set; }

            [DataMember(Name="auto_start_download", IsRequired=false)]
            public bool autoStartDownload { get; set; }
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

        private static ChromeExtensionParams ToChromeExtensionParams( this string json )
        {
            using ( var ms = new MemoryStream( Encoding.UTF8.GetBytes( json ) ) )
            {
                var ser = new DataContractJsonSerializer( typeof(ChromeExtensionParams) );
                var chromeExtensionParams = (ChromeExtensionParams) ser.ReadObject( ms );
                return (chromeExtensionParams);
            }
        }
        private static ChromeExtensionParams? ToChromeExtensionParams_NoThrow( this string json )
        {
            try
            {
                return (json.ToChromeExtensionParams());
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
            if ( args.Any( a => a.StartsWith( "chrome-extension://", StringComparison.InvariantCultureIgnoreCase ) ) )
            {
                var text = ReadStandardInputFromChrome();
                if ( !text.IsNullOrWhiteSpace() )
                {
                    var p = text.ToChromeExtensionParams_NoThrow();
                    if ( p.HasValue )
                    {
                        m3u8FileUrl       = p.Value.m3u8FileUrl;
                        autoStartDownload = p.Value.autoStartDownload;
                    }
                }
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

        private static void Application_ThreadException( object sender, ThreadExceptionEventArgs e ) => e.Exception.MessageBox_ShowError( "Application.ThreadException" );
        private static void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e ) => Extensions.MessageBox_ShowError( e.ExceptionObject.ToString(), " AppDomain.CurrentDomain.UnhandledException" );
    }
}
