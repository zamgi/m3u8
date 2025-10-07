using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
        //The main thread must be STA, and [STAThread] is ignored on async Task Main and prevent set this to MTA.//
        //disallowed async/await => don't work Drag-N-Drop//
        [STAThread] private static void/*async Task*/ Main( string[] args )
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault( false );
            //Application.Run( new Form1() );
            //return;
            //----------------------------------------------------//

            Application.ThreadException                  += (_, e) => e.Exception.MessageBox_ShowError( "Application.ThreadException" );
            AppDomain  .CurrentDomain.UnhandledException += (_, e) => Extensions.MessageBox_ShowError( e.ExceptionObject.ToString(), " AppDomain.CurrentDomain.UnhandledException" );
            Application.SetUnhandledExceptionMode( UnhandledExceptionMode.Automatic, true );

            using ( var sca = SingleCopyApplication.Current )
            {
                #region [.parse if opened from 'chrome-extension' || 'firefox-extension'.]
                var inputParams      = default((string m3u8FileUrl, string requestHeaders, bool autoStartDownload));
                var inputParamsArray = default((string m3u8FileUrl, string requestHeaders, bool autoStartDownload)[]);
                var success = false;

                var browserType = BrowserIPC.CommandLine.GetBrowserType( args );
                switch ( browserType )
                {
                    #region comm. [2023.05.10; Now chrome launches native application process just like firefox]
                    /*case BrowserIPC.BrowserTypeEnum.Chrome:
                    {
                        var text = BrowserIPC.ReadFromStandardInput();
                        if ( !text.IsNullOrWhiteSpace() )
                        {
                            success = BrowserIPC.TryParseAsExtensionInputParams( text, out inputParamsArray );
                        }
                        BrowserIPC.WriteToStandardOutput( success );
                    }
                    break;*/
                    #endregion

                    case BrowserIPC.BrowserTypeEnum.Chrome:
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
                    inputParamsArray = [ inputParams ];
                }
                #endregion

                #region [.PipeIPC for SingleCopyApplication.]
                if ( sca.IsFirstCopy )
                {
                    #region [.check for upgrade user-settings for new version.]
                    // Copy user settings from previous application version if necessary
                    Settings.Default.UpgradeIfNeed();
                    #endregion
#if !(NETCOREAPP)
                    #region [.set SecurityProtocol to 'Tls + Tls11 + Tls12 + Ssl3'.]
                    Set_SecurityProtocols();
                    #endregion
#endif
#if NETCOREAPP
                    Application.SetHighDpiMode( HighDpiMode.SystemAware ); 
#endif
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault( false );

                    var mainform = new MainForm( inputParamsArray );

                    PipeIPC.NamedPipeServer__Input.RunListener( sca.MutexName );

                    Application.Run( mainform );
                }
                else if ( success )
                {
                    //disallowed async/await => don't work Drag-N-Drop
                    //---await PipeIPC.NamedPipeClient__out.Send_Async( sca.MutexName, inputParamsArray ).CAX();
                    PipeIPC.NamedPipeClient__Output.Send_Async( sca.MutexName, inputParamsArray ).Wait();
                }
                else
                {
                    //disallowed async/await => don't work Drag-N-Drop
                    //await PipeIPC.NamedPipeClient__out.Send2FirstCopy_Async( sca.MutexName ).CAX();
                    PipeIPC.NamedPipeClient__Output.Send2FirstCopy_Async( sca.MutexName ).Wait();
                }
                #endregion
            }
        }

#if !(NETCOREAPP)
        private static void Set_SecurityProtocols()
        {
            foreach ( var spt in Enum.GetValues( typeof(SecurityProtocolType) ).Cast< SecurityProtocolType >() )
            {
                try
                {
                    ServicePointManager.SecurityProtocol |= spt;
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
        }
#endif
    }
}
