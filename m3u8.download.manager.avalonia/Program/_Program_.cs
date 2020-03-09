using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
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
        private sealed class AdvancedEncodingProvider : EncodingProvider
        {
            private Dictionary< int   , Encoding > _Dict_1 = new Dictionary< int   , Encoding >();
            private Dictionary< string, Encoding > _Dict_2 = new Dictionary< string, Encoding >( StringComparer.InvariantCultureIgnoreCase );
            public AdvancedEncodingProvider( EncodingInfo[] encodingInfos )
            {
                foreach ( var ei in encodingInfos )
                {
                    var encoding = Encoding.GetEncoding( ei.CodePage );
                    _Dict_1.Add( ei.CodePage, encoding );
                    _Dict_2.Add( ei.Name    , encoding );
                }

                if ( !_Dict_2.ContainsKey( "utf8" ) && _Dict_2.TryGetValue( "utf-8", out var _encoding ) )
                {
                    _Dict_2.Add( "utf8", _encoding );
                }
            }
            public override Encoding GetEncoding( string name ) => (_Dict_2.TryGetValue( name, out var encoding ) ? encoding : default);
            public override Encoding GetEncoding( int codepage ) => (_Dict_1.TryGetValue( codepage, out var encoding ) ? encoding : default);
        }

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
                                    var cmdLine         = BrowserIPC.CommandLine.Create( executeFileName );

                                    sca.Release(); //!!!

                                    success = ProcessCreator.CreateAsBreakawayFromJob( cmdLine );
                                    if ( !success )
                                    {
                                        Extensions.MessageBox_ShowError( "Error while trying run additional native application's process", executeFileName ).Wait();
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
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);// | SecurityProtocolType.Ssl3);
                    #endregion

                    #region [.register encoding provider.]
                    Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );
                    Encoding.RegisterProvider( new AdvancedEncodingProvider( Encoding.GetEncodings() ) );
                    #endregion

                    //Application.EnableVisualStyles();
                    //Application.SetCompatibleTextRenderingDefault( false );

                    //var mainform = new MainForm( inputParamsArray );

                    App._InputParamsArray = inputParamsArray;
                    PipeIPC.NamedPipeServer__in.RunListener( sca.MutexName );

                    //Application.Run( mainform );
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
