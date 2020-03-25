using System;
using System.Diagnostics;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
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
        //---[STAThread]
        private static void Main( string[] args )
        {
            //Application.ThreadException += (sender, e) => e.Exception.MessageBox_ShowError( "Application.ThreadException" );
            AppDomain.CurrentDomain.UnhandledException += async (sender, e) => await Extensions.MessageBox_ShowError( e.ExceptionObject.ToString(),"AppDomain.CurrentDomain.UnhandledException"  );                

            using ( var sca = SingleCopyApplication.Current )
            {
                #region [.parse if opened from 'chrome-extension' || 'firefox-extension'.]
                var inputParams      = default((string m3u8FileUrl, bool autoStartDownload));
                var inputParamsArray = default((string m3u8FileUrl, bool autoStartDownload)[]);
                var success = false;

                var browserType = BrowserIPC.CommandLine.GetBrowserType( args );
                switch ( browserType )
                {
                    #region comm.
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
                                    var cmdLine         = BrowserIPC.CommandLine.Create( executeFileName );

                                    sca.Release(); //!!!

                                    string errorMsg;
                                    (success, errorMsg) = ProcessCreator.CreateAsBreakawayFromJob( executeFileName, cmdLine );
                                    if ( !success )
                                    {
                                        PlatformHelper.TryMessageBox_ShowError( $"Error while trying run additional native application's process{(errorMsg.IsNullOrEmpty() ? null : $": '{errorMsg}'")}.", executeFileName );
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

                    #region comm. [.set SecurityProtocol to 'Tls + Tls11 + Tls12 + Tls13 + Ssl3'.]
                    /*
                    void set_SecurityProtocol( SecurityProtocolType securityProtocolType )
                    {
                        try
                        {
                            ServicePointManager.SecurityProtocol |= securityProtocolType;
                        }
                        catch ( Exception ex )
                        {
                            Debug.WriteLine( ex );
                        }
                    };
                    set_SecurityProtocol( SecurityProtocolType.Tls   );
                    set_SecurityProtocol( SecurityProtocolType.Tls11 );
                    set_SecurityProtocol( SecurityProtocolType.Tls12 );
                    set_SecurityProtocol( SecurityProtocolType.Tls13 );
                    //---set_SecurityProtocol( SecurityProtocolType.Ssl3 );
                    */
                    #endregion

                    #region [.register encoding provider.]
                    AdvancedEncodingProvider.Init();
                    #endregion

                    App._InputParamsArray = inputParamsArray;
                    PipeIPC.NamedPipeServer__in.RunListener( sca.MutexName );

                    try
                    {
                        BuildAvaloniaApp().StartWithClassicDesktopLifetime( args, ShutdownMode.OnLastWindowClose );
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                        GlobalExceptionHandlerWindow.Show( ex );
                    }
                }
                else if ( success )
                {
                    PipeIPC.NamedPipeClient__out.Send( sca.MutexName, inputParamsArray );
                }
                #endregion
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure< App >()
                                                                  .UsePlatformDetect()
                                                                  .LogToDebug()
                                                                  //.UseSkia()
                                                                  //.UseReactiveUI()
                                                                  ;
    }
}
