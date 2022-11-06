using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
//using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.ipc;
using m3u8.download.manager.Properties;
using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Program
    {
        /// <summary>
        ///
        /// </summary>
        [STAThread] private static void/*async Task*/ Main( string[] args )
        {
            Application.ThreadException                  += (sender, e) => e.Exception.MessageBox_ShowError( "Application.ThreadException" );
            AppDomain  .CurrentDomain.UnhandledException += (sender, e) => Extensions.MessageBox_ShowError( e.ExceptionObject.ToString(), " AppDomain.CurrentDomain.UnhandledException" );
            Application.SetUnhandledExceptionMode( UnhandledExceptionMode.Automatic, true );

            using ( var sca = SingleCopyApplication.Current )
            {
                #region [.parse if opened from 'chrome-extension' || 'firefox-extension'.]
                var inputParams      = default((string m3u8FileUrl, bool autoStartDownload));
                var inputParamsArray = default((string m3u8FileUrl, bool autoStartDownload)[]);
                var success = false;

                var browserType = BrowserIPC.CommandLine.GetBrowserType( args );
                switch ( browserType )
                {
                    case BrowserIPC.BrowserTypeEnum.Chrome:
                    {
                        var text = BrowserIPC.ReadFromStandardInput();
                        if ( !text.IsNullOrWhiteSpace() )
                        {
                            success = BrowserIPC.TryParseAsExtensionInputParams( text, out inputParamsArray );
                        }
                        BrowserIPC.WriteToStandardOutput( success );
                    }
                    break;

                    case BrowserIPC.BrowserTypeEnum.Firefox:
                    {
                        var text = BrowserIPC.ReadFromStandardInput();
                        if ( !text.IsNullOrWhiteSpace() )
                        {
                            success = BrowserIPC.TryParseAsExtensionInputParams( text, out inputParamsArray );                            

                            if ( success )
                            {
                                if ( sca.IsFirstCopy )
                                {
                                    var executeFileName = Process.GetCurrentProcess().MainModule.FileName;
                                    var cmdLine         = BrowserIPC.CommandLine.Create_4_CreateAsBreakawayFromJob( executeFileName );

                                    sca.Release(); //!!!

                                    success = ProcessCreator.CreateAsBreakawayFromJob( cmdLine );
                                    if ( !success )
                                    {
                                        MessageBox.Show( "Error while trying run additional native application's process", executeFileName, MessageBoxButtons.OK, MessageBoxIcon.Error );
                                    }
                                }
                            }
                        }
                        BrowserIPC.WriteToStandardOutput( success );
                    }
                    break;

                    default:
                    {
                        //Debugger.Launch();
                        success = BrowserIPC.CommandLine.TryParse( args, out inputParams );
                    }
                    break;
                }
                #endregion

                #region [.set/correct inputParams.]
                if ( success && (inputParamsArray == null) )
                {
                    inputParamsArray = new[] { inputParams };
                }
                #endregion

                #region [.PipeIPC for SingleCopyApplication.]
                if ( sca.IsFirstCopy )
                {
                    #region [.check for upgrade user-settings for new version.]
                    // Copy user settings from previous application version if necessary
                    Settings.Default.UpgradeIfNeed();
                    #endregion

                    #region [.set SecurityProtocol to 'Tls + Tls11 + Tls12 + Ssl3'.]
                    static void set_SecurityProtocol( SecurityProtocolType spt )
                    {
                        try
                        {
                            ServicePointManager.SecurityProtocol |= spt;
                        }
                        catch ( Exception ex )
                        {
                            Debug.WriteLine( ex );
                        }
                    };
                    #region comm. other vers.
                    //set_SecurityProtocol( SecurityProtocolType.Tls   );
                    //set_SecurityProtocol( SecurityProtocolType.Tls11 );
                    //set_SecurityProtocol( SecurityProtocolType.Tls12 );
                    //set_SecurityProtocol( SecurityProtocolType.Tls13 );
                    //set_SecurityProtocol( SecurityProtocolType.Ssl3  );                    
                    #endregion
                    foreach ( var spt in Enum.GetValues( typeof(SecurityProtocolType) ).Cast< SecurityProtocolType >() )
                    {
                        set_SecurityProtocol( spt );
                    }
                    #endregion
#if NETCOREAPP
                    Application.SetHighDpiMode( HighDpiMode.SystemAware ); 
#endif
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault( false );

                    var mainform = new MainForm( inputParamsArray );

                    PipeIPC.NamedPipeServer__in.RunListener( sca.MutexName );

                    Application.Run( mainform );
                }
                else if ( success )
                {
                    //disallowed async/await => don't work Drag-N-Drop
                    //---await PipeIPC.NamedPipeClient__out.Send_Async( sca.MutexName, inputParamsArray ).CAX();
                    PipeIPC.NamedPipeClient__out.Send_Async( sca.MutexName, inputParamsArray ).Wait();
                }
                else
                {
                    //await PipeIPC.NamedPipeClient__out.Send2FirstCopy_Async( sca.MutexName ).CAX();
                    PipeIPC.NamedPipeClient__out.Send2FirstCopy_Async( sca.MutexName ).Wait();
                }
                #endregion
            }
        }
    }
}
