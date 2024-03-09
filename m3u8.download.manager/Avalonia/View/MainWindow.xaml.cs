using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia.Enums;

using m3u8.download.manager.infrastructure;
using m3u8.download.manager.ipc;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;

using _CollectionChangedTypeEnum_ = m3u8.download.manager.models.DownloadListModel.CollectionChangedTypeEnum;
using _Resources_ = m3u8.download.manager.Properties.Resources;
using X = (string m3u8FileUrl, string requestHeaders, bool autoStartDownload);
using Avalonia.Platform.Storage;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MainWindow : StoreBoundsWindowBase/*Window*/, IDisposable
    {
        #region [.fields from markup.]
        private DownloadListUC downloadListUC;
        private LogUC logUC;

        private MenuItem startDownloadToolButton;
        private MenuItem pauseDownloadToolButton;
        private MenuItem cancelDownloadToolButton;
        private MenuItem deleteDownloadToolButton;
        private MenuItem deleteAllFinishedDownloadToolButton;
        private MenuItem showLogToolButton;
        private MenuItem copyToolButton;
        private MenuItem pasteToolButton;
        private DegreeOfParallelismMenuItem degreeOfParallelismToolButton;
        private DownloadInstanceMenuItem downloadInstanceToolButton;
        private SpeedThresholdToolButton speedThresholdToolButton;

        private ContextMenu mainContextMenu;
        private MenuItem startDownloadMenuItem;
        private MenuItem pauseDownloadMenuItem;
        private MenuItem cancelDownloadMenuItem;
        private MenuItem editDownloadMenuItem;
        private MenuItem editDownloadMenuItem_Separator;
        private MenuItem deleteDownloadMenuItem;
        private MenuItem deleteWithOutputFileMenuItem;
        private MenuItem onlyDeleteOutputFileMenuItem;
        private MenuItem browseOutputFileMenuItem;
        private MenuItem openOutputFileMenuItem;
        private MenuItem changeOutputDirectoryMenuItem;
        private MenuItem deleteAllFinishedDownloadMenuItem;
        private MenuItem startAllDownloadsMenuItem;
        private MenuItem cancelAllDownloadsMenuItem;
        private MenuItem pauseAllDownloadsMenuItem;
        private MenuItem deleteAllDownloadsMenuItem;
        private MenuItem deleteAllWithOutputFilesMenuItem;
        #endregion

        #region [.field's.]
        private MainVM _VM;
        private X[] _InputParamsArray;
        private bool _ShowDownloadStatistics;
        private Window _HostWindow_4_Notification;
        private WindowNotificationManager _NotificationManager;
        #endregion

        #region [.ctor().]
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            PipeIPC.NamedPipeServer__Input.ReceivedSend2FirstCopy += NamedPipeServer__Input_ReceivedSend2FirstCopy;
        }
        public MainWindow( in X[] array ) : this() => _InputParamsArray = array;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );
            //----------------------------------------//

            #region [.user controls.]
            downloadListUC = this.Find_Ex< DownloadListUC >( nameof(downloadListUC) );
            logUC          = this.Find_Ex< LogUC >( nameof(logUC) );
            #endregion

            #region [.menu.]
            startDownloadToolButton  = this.Find_Ex< MenuItem >( nameof(startDownloadToolButton)  ); startDownloadToolButton.Click  += startDownloadToolButton_Click;
            pauseDownloadToolButton  = this.Find_Ex< MenuItem >( nameof(pauseDownloadToolButton)  ); pauseDownloadToolButton.Click  += pauseDownloadToolButton_Click;
            cancelDownloadToolButton = this.Find_Ex< MenuItem >( nameof(cancelDownloadToolButton) ); cancelDownloadToolButton.Click += cancelDownloadToolButton_Click;
            deleteDownloadToolButton = this.Find_Ex< MenuItem >( nameof(deleteDownloadToolButton) ); deleteDownloadToolButton.Click += deleteDownloadToolButton_Click;
            deleteAllFinishedDownloadToolButton = this.Find_Ex< MenuItem >( nameof(deleteAllFinishedDownloadToolButton) ); deleteAllFinishedDownloadToolButton.Click += deleteAllFinishedDownloadToolButton_Click;

            showLogToolButton = this.Find_Ex< MenuItem >( nameof(showLogToolButton) ); showLogToolButton.Click += showLogToolButton_Click;
            copyToolButton    = this.Find_Ex< MenuItem >( nameof(copyToolButton)    ); copyToolButton.Click    += copyToolButton_Click;
            pasteToolButton   = this.Find_Ex< MenuItem >( nameof(pasteToolButton)   ); pasteToolButton.Click   += pasteToolButton_Click;

            degreeOfParallelismToolButton = this.Find_Ex< DegreeOfParallelismMenuItem >( nameof(degreeOfParallelismToolButton) ); degreeOfParallelismToolButton.ValueChanged += degreeOfParallelismToolButton_ValueChanged;
            downloadInstanceToolButton    = this.Find_Ex< DownloadInstanceMenuItem    >( nameof(downloadInstanceToolButton)    ); downloadInstanceToolButton.ValueChanged    += downloadInstanceToolButton_ValueChanged;
            speedThresholdToolButton      = this.Find_Ex< SpeedThresholdToolButton    >( nameof(speedThresholdToolButton)      ); speedThresholdToolButton.ValueChanged      += speedThresholdToolButton_ValueChanged;
            #endregion

            #region [.context menu.]
            mainContextMenu                   = this.Find_Ex< ContextMenu >( nameof(mainContextMenu) ); //mainContextMenu.Styles.Add( GlobalStyles.Light );
            startDownloadMenuItem             = mainContextMenu.Find_MenuItem( nameof(startDownloadMenuItem) ); startDownloadMenuItem.Click += startDownloadMenuItem_Click;
            pauseDownloadMenuItem             = mainContextMenu.Find_MenuItem( nameof(pauseDownloadMenuItem) ); pauseDownloadMenuItem.Click += pauseDownloadMenuItem_Click;
            cancelDownloadMenuItem            = mainContextMenu.Find_MenuItem( nameof(cancelDownloadMenuItem) ); cancelDownloadMenuItem.Click += cancelDownloadMenuItem_Click;
            editDownloadMenuItem              = mainContextMenu.Find_MenuItem( nameof(editDownloadMenuItem) ); editDownloadMenuItem.Click += editDownloadMenuItem_Click;
            editDownloadMenuItem_Separator    = mainContextMenu.Find_MenuItem( nameof(editDownloadMenuItem_Separator) );
            deleteDownloadMenuItem            = mainContextMenu.Find_MenuItem( nameof(deleteDownloadMenuItem) ); deleteDownloadMenuItem.Click += deleteDownloadMenuItem_Click;
            deleteWithOutputFileMenuItem      = mainContextMenu.Find_MenuItem( nameof(deleteWithOutputFileMenuItem) ); deleteWithOutputFileMenuItem.Click += deleteWithOutputFileMenuItem_Click;
            onlyDeleteOutputFileMenuItem      = mainContextMenu.Find_MenuItem( nameof(onlyDeleteOutputFileMenuItem) ); onlyDeleteOutputFileMenuItem.Click += onlyDeleteOutputFileMenuItem_Click;
            browseOutputFileMenuItem          = mainContextMenu.Find_MenuItem( nameof(browseOutputFileMenuItem) ); browseOutputFileMenuItem.Click += browseOutputFileMenuItem_Click;
            openOutputFileMenuItem            = mainContextMenu.Find_MenuItem( nameof(openOutputFileMenuItem) ); openOutputFileMenuItem.Click += openOutputFileMenuItem_Click;
            changeOutputDirectoryMenuItem     = mainContextMenu.Find_MenuItem( nameof(changeOutputDirectoryMenuItem) ); changeOutputDirectoryMenuItem.Click += changeOutputDirectoryMenuItem_Click;
            deleteAllFinishedDownloadMenuItem = mainContextMenu.Find_MenuItem( nameof(deleteAllFinishedDownloadMenuItem) ); deleteAllFinishedDownloadMenuItem.Click += deleteAllFinishedDownloadToolButton_Click;
            startAllDownloadsMenuItem         = mainContextMenu.Find_MenuItem( nameof(startAllDownloadsMenuItem) ); startAllDownloadsMenuItem.Click += startAllDownloadsMenuItem_Click;
            cancelAllDownloadsMenuItem        = mainContextMenu.Find_MenuItem( nameof(cancelAllDownloadsMenuItem) ); cancelAllDownloadsMenuItem.Click += cancelAllDownloadsMenuItem_Click;
            pauseAllDownloadsMenuItem         = mainContextMenu.Find_MenuItem( nameof(pauseAllDownloadsMenuItem) ); pauseAllDownloadsMenuItem.Click += pauseAllDownloadsMenuItem_Click;
            deleteAllDownloadsMenuItem        = mainContextMenu.Find_MenuItem( nameof(deleteAllDownloadsMenuItem) ); deleteAllDownloadsMenuItem.Click += deleteAllDownloadsMenuItem_Click;
            deleteAllWithOutputFilesMenuItem  = mainContextMenu.Find_MenuItem( nameof(deleteAllWithOutputFilesMenuItem) ); deleteAllWithOutputFilesMenuItem.Click += deleteAllWithOutputFilesMenuItem_Click;
            #endregion
            //----------------------------------------//

            #region [.-1-.]
            this.Title = _Resources_.APP_TITLE;
            this.DataContext = _VM = new MainVM( this );

            _VM.DownloadListModel.RowPropertiesChanged     += DownloadListModel_RowPropertiesChanged;
            _VM.SettingsController.SettingsPropertyChanged += SettingsController_PropertyChanged;

            SettingsController_PropertyChanged( _VM.SettingsController.Settings, nameof(Settings.ShowDownloadStatisticsInMainFormTitle) );
            SettingsController_PropertyChanged( _VM.SettingsController.Settings, nameof(Settings.ShowAllDownloadsCompleted_Notification) );
            #endregion

            //----------------------------------------//

            #region [.-2-.]
            logUC.SetSettingsController( _VM.SettingsController );

            downloadListUC.SetModel( _VM.DownloadListModel );
            downloadListUC.SelectionChanged           += downloadListUC_SelectionChanged;
            downloadListUC.OutputFileNameClick        += downloadListUC_OutputFileNameClick;
            downloadListUC.OutputDirectoryClick       += downloadListUC_OutputDirectoryClick;
            downloadListUC.LiveStreamMaxFileSizeClick += downloadListUC_LiveStreamMaxFileSizeClick;
            downloadListUC.UpdatedSingleRunningRow    += downloadListUC_UpdatedSingleRunningRow;
            downloadListUC.MouseClickRightButton      += downloadListUC_MouseClickRightButton;
            downloadListUC.DoubleClickEx              += openOutputFileMenuItem_Click;
            //---downloadListUC.EnterKeyDown            += downloadListUC_EnterKeyDown;

            SetDownloadToolButtonsStatus( null );
            NameCleaner.ResetExcludesWords( _VM.SettingsController.NameCleanerExcludesWords );

            downloadInstanceToolButton.SetValueAndIsVisible( _VM.SettingsController.MaxCrossDownloadInstance );
            degreeOfParallelismToolButton.Value = _VM.SettingsController.MaxDegreeOfParallelism;
            speedThresholdToolButton     .Value = _VM.SettingsController.MaxSpeedThresholdInMbps;
            #endregion
        }

        public void Dispose() => _VM.Dispose_NoThrow();

        public void SaveDownloadListColumnsInfo()
        {
            _VM.SettingsController.SetDownloadListColumnsInfoJson( downloadListUC.GetColumnsInfoJson() );
            _VM.SettingsController.SaveNoThrow_IfAnyChanged();
        }
        #endregion

        #region [.public/internal.]
        internal DataGrid DownloadListDGV => downloadListUC.DataGrid; 
        #endregion

        #region [.override methods.]
        protected async override void OnOpened( EventArgs e )
        {
            base.OnOpened( e );

            #region [.restore settings.]
            if ( PlatformHelper.IsWinNT() )
            {
                this.RestoreBounds( _VM.SettingsController.MainFormPositionJson );
            }
            downloadListUC.RestoreColumnsInfoFromJson( _VM.SettingsController.GetDownloadListColumnsInfoJson() );
            downloadListUC.Focus();

            Set_LogUC_RowDefinition_Height( _VM.SettingsController.Settings.LogUC_RowDefinition_Height );
            #endregion

            if ( !_VM.SettingsController.ShowLog ) showLogToolButton_Click( showLogToolButton, EventArgs.Empty );

            if ( _InputParamsArray.AnyEx() )
            {
                _VM.AddCommand.AddNewDownloads( _InputParamsArray );
                _InputParamsArray = null;
            }
            else if ( !BrowserIPC.CommandLine.Is_CommandLineArgs_Has__CreateAsBreakawayFromJob() )
            {
                var (success, m3u8FileUrls) = await this.TryGetM3u8FileUrlsFromClipboard();
                if ( success )
                {
                    var frt = m3u8FileUrls.FirstOrDefault();
                    _VM.AddCommand.AddNewDownload( (frt.url, frt.requestHeaders, false) );
                }
            }
            _VM.DownloadListModel.AddRows( _VM.SettingsController.GetDownloadRows() /*DownloadRowsSerializer.FromJSON( _VM.SettingsController.DownloadRowsJson )*/ );
#if DEBUG
            if ( _VM.DownloadListModel.RowsCount == 0 )
            {
                var dir = Settings.Default.OutputFileDirectory;
                _VM.DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8"   , null, "xz-1", dir) );
                _VM.DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8-12", null, "xz-2", dir) );
                _VM.DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8-34", null, "xz-3", dir) );
            }
#endif
            var suc = downloadListUC.DataGrid.Focus();
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            #region [.save settings.]
            _VM.SettingsController.MainFormPositionJson = this.GetBounds().ToJSON();
            _VM.SettingsController.SetDownloadListColumnsInfoJson( downloadListUC.GetColumnsInfoJson() );
            _VM.SettingsController.SetDownloadRows( _VM.DownloadListModel.GetRows() ); //_VM.SettingsController.DownloadRowsJson = DownloadRowsSerializer.ToJSON( _VM.DownloadListModel.GetRows() );
            _VM.SettingsController.Settings.LogUC_RowDefinition_Height = Get_LogUC_RowDefinition_Height();
            _VM.SettingsController.SaveNoThrow_IfAnyChanged();
            #endregion

            _HostWindow_4_Notification?.Close();
        }
        protected async override void OnClosing( WindowClosingEventArgs e )
        {
            base.OnClosing( e );

            #region comm. cancel if WaitBanner shown.
            /*
            if ( this.IsWaitBannerShown() )
            {
                e.Cancel = true;
                return;
            }
            //*/
            #endregion

            //still downloading?
            if ( _VM.DownloadController.IsDownloading )
            {
                if ( this.WindowState == WindowState.Minimized )
                {
                    this.WindowState = WindowState.Normal;
                }

                e.Cancel = true;

                var result = await this.MessageBox_ShowQuestion( "Dou you want to [CANCEL] all downloading and exit ?", _Resources_.APP_TITLE );
                if ( result == ButtonResult.Yes )
                {
                    const int WAIT_Milliseconds = 10_000;

                    //waiting for all canceled and becomes finished
                    for ( var sw = Stopwatch.StartNew(); ; )
                    {
                        _VM.DownloadController.CancelAll();
                        await Task.Delay( 10 );

                        if ( !_VM.DownloadController.IsDownloading || (WAIT_Milliseconds <= sw.ElapsedMilliseconds) )
                        {
                            Close();
                            break;
                        }
                    }
                }
            }
        }

        protected async override void OnKeyDown( KeyEventArgs e )
        {
            if ( e.KeyModifiers.HasFlag( KeyModifiers.Control ) )
            {
                switch ( e.Key )
                {
                    case Key.V: //Paste
                        var (success, urls) = await this.TryGetHttpUrlsFromClipboard();
                        if ( success )
                        {
                            e.Handled = true;

                            var autoStartDownload = e.KeyModifiers.HasFlag( KeyModifiers.Shift );
                            if ( !autoStartDownload ) urls = urls.Take( 50 ).ToList( 50 );
                            _VM.AddCommand.AddNewDownloads( (urls, autoStartDownload) );
                            return;
                        }
                        else
                        {
                            await this.MessageBox_ShowError( "Nothing for Paste from clipboard.", this.Title );
                        }
                        break;

                    case Key.C: //Copy
                        if ( downloadListUC.HasFocus )
                        {
                            var rows = downloadListUC.GetSelectedDownloadRows();
                            if ( rows.Any() )
                            {
                                e.Handled = true;
                                await this.CopyToClipboard( rows );
                                return;
                            }
                            else
                            {
                                await this.MessageBox_ShowError( "Nothing for Copy to clipboard.", this.Title );
                            }
                        }
                        break;

                    case Key.S: //Start download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Start );
                        }
                        break;

                    case Key.P: //Pause download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Pause );
                        }
                        break;

                    case Key.Z: //Cancel download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Cancel );
                        }
                        break;

                    case Key.W: //Exit | Close
                        this.Close();
                        break;

                    case Key.D: //Minimized window
                        this.WindowState = WindowState.Minimized;
                        break;

                    case Key.B: //Browse output file
                        if ( downloadListUC.HasFocus )
                        {
                            browseOutputFileMenuItem_Click( this, EventArgs.Empty );
                        }
                        break;
                    case Key.O: //Open output file
                        if ( downloadListUC.HasFocus )
                        {
                            openOutputFileMenuItem_Click( this, EventArgs.Empty );
                        }
                        break;

                    case Key.G:
                        if ( e.KeyModifiers.HasFlag( KeyModifiers.Shift ) ) //Collect Garbage
                        {
                            _VM.CollectGarbageCommand.Execute( null );
                        }
                        break;

                    case Key.E: // [Ctrl + E]
                        if ( downloadListUC.HasFocus )
                        {
                            _VM.EditCommand.EditDownload( downloadListUC.GetSelectedDownloadRow() );
                        }
                        break;

                    case Key.Delete: // [Ctrl + Shift + Del]
                        if ( e.KeyModifiers.HasFlag( KeyModifiers.Shift ) && downloadListUC.HasFocus )
                        {
                            OnlyDeleteOutputFiles( downloadListUC.GetSelectedDownloadRows() );
                        }
                        break;
                }
            }
            else
            {
                switch ( e.Key )
                {
                    case Key.Insert: //add download dialog
                        {
                            e.Handled = true;
                            var m3u8FileUrls = await this.TryGetM3u8FileUrlsFromClipboardOrDefault();
#if DEBUG
                            if ( !m3u8FileUrls.AnyEx() ) m3u8FileUrls = [ ($"http://xzxzzxzxxz.ru/{(new Random().Next())}/abc.def", null) ];
#endif
                            _VM.AddCommand.AddNewDownloads( (m3u8FileUrls, false) );
                        }
                        return;

                    case Key.Delete: // Delete download
                        if ( downloadListUC.HasFocus )
                        {
                            var rows = downloadListUC.GetSelectedDownloadRows();
                            var deleteOutputFiles = e.KeyModifiers.HasFlag( KeyModifiers.Shift );
                            if ( await AskDeleteDownloadDialog( rows, askOnlyOutputFileExists: false, deleteOutputFiles ) )
                            {
                                DeleteDownloads( rows, deleteOutputFiles );
                            }
                        }
                        break;

                    case Key.F1: //about
                        _VM.AboutCommand.Execute( null );
                        break;

                    case Key.Enter: //change output-file dialog || Open output file
                        if ( downloadListUC.HasFocus )
                        {
                            var row = downloadListUC.GetSelectedDownloadRow();
                            if ( row != null )
                            {
                                if ( row.IsFinishedOrError() )
                                {
                                    openOutputFileMenuItem_Click( this, EventArgs.Empty );
                                }
                                else
                                {
                                    downloadListUC_OutputFileNameClick( row );
                                }
                            }
                        }
                        break;
                }
            }

            base.OnKeyDown( e );
        }
        #endregion

        #region [.event handlers methods.]
        private async void NamedPipeServer__Input_ReceivedSend2FirstCopy() => await Dispatcher.UIThread.InvokeAsync( () => ReceivedSend2FirstCopy() );
        private void ReceivedSend2FirstCopy()
        {
            if ( this.WindowState == WindowState.Minimized )
            {
                this.WindowState = WindowState.Normal;
            }
            this.Activate();
        }

        private void SettingsController_PropertyChanged( Settings settings, string propertyName )
        {
            var is_need_save = false;
            switch ( propertyName )
            {
                case nameof(Settings.ShowDownloadStatisticsInMainFormTitle ):
                    _ShowDownloadStatistics = settings.ShowDownloadStatisticsInMainFormTitle;

                    _VM.DownloadListModel.CollectionChanged -= DownloadListModel_CollectionChanged;
                    if ( _ShowDownloadStatistics )
                    {
                        _VM.DownloadListModel.CollectionChanged += DownloadListModel_CollectionChanged;
                    }
                    ShowDownloadStatisticsInTitle();
                    break;

                case nameof(Settings.MaxCrossDownloadInstance): // nameof(Settings.UseCrossDownloadInstanceParallelism):
                    downloadInstanceToolButton.SetValueAndIsVisible( settings.MaxCrossDownloadInstance );
                    is_need_save = true;
                    break;

                case nameof(Settings.MaxDegreeOfParallelism):
                    degreeOfParallelismToolButton.Value = settings.MaxDegreeOfParallelism;
                    is_need_save = true;
                    break;

                case nameof(Settings.MaxSpeedThresholdInMbps):
                    speedThresholdToolButton.Value = settings.MaxSpeedThresholdInMbps;
                    is_need_save = true;
                    break;

                case nameof(Settings.ShowAllDownloadsCompleted_Notification):
                    _VM.DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged;
                    if ( settings.ShowAllDownloadsCompleted_Notification )
                    {
                        _VM.DownloadController.IsDownloadingChanged += DownloadController_IsDownloadingChanged;
                    }
                    break;
            }

            if ( is_need_save )
            {
                _VM.SettingsController.SaveNoThrow_IfAnyChanged();
            }
        }
        private async void DownloadListModel_RowPropertiesChanged( DownloadRow row, string propertyName )
        {
            if ( propertyName == nameof(DownloadRow.Status ) )
            {
                if ( downloadListUC.GetSelectedDownloadRow() == row )
                {
                    await Dispatcher.UIThread.InvokeAsync( () => 
                    {
                        SetDownloadToolButtonsStatus( downloadListUC.GetSelectedDownloadRow() );
                        ShowDownloadStatisticsInTitle();
                    });
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync( () => { SetDownloadToolButtonsStatus_NonSelected( row ); ShowDownloadStatisticsInTitle(); } );
                }
            }
        }
        private void DownloadListModel_CollectionChanged( _CollectionChangedTypeEnum_ changedType, DownloadRow _ )
        {
            if ( changedType == _CollectionChangedTypeEnum_.Sort ) return;

            ShowDownloadStatisticsInTitle();

            #region comm.
            //switch ( collectionChangedType )
            //{
            //    case _CollectionChangedTypeEnum_.BulkUpdate:
            //    case _CollectionChangedTypeEnum_.Remove:                    
            //        _LogRowsHeightStorer.LeaveOnly( (from row in _VM.DownloadListModel.GetRows() select row.Log) );
            //          break;

            //    case _CollectionChangedTypeEnum_.Clear:
            //        _LogRowsHeightStorer.Clear();
            //          break;
            //} 
            #endregion

            _VM.SettingsController.SetDownloadRows_WithSaveIfChanged( _VM.DownloadListModel.GetRows() );
        }
        private void DownloadController_IsDownloadingChanged( bool isDownloading )
        {
            if ( !isDownloading )
            {
                _HostWindow_4_Notification?.Close();
                _HostWindow_4_Notification = new Window() 
                {
                    ShowInTaskbar                      = false,
                    BorderBrush                        = Brushes.Transparent,
                    Background                         = Brushes.Transparent,//null, //
                    TransparencyBackgroundFallback     = Brushes.Transparent,//null, //
                    SystemDecorations                  = SystemDecorations.None,
                    //---TransparencyLevelHint              = WindowTransparencyLevel.Transparent,
                    TransparencyLevelHint              = new[] { WindowTransparencyLevel.Transparent },
                    ShowActivated                      = false,
                    CanResize                          = false,
                    IsTabStop                          = false,
                    ExtendClientAreaToDecorationsHint  = true,
                    ExtendClientAreaChromeHints        = ExtendClientAreaChromeHints.NoChrome,
                    ExtendClientAreaTitleBarHeightHint = -1,
                };
                //---var scr = this.Screens.ScreenFromWindow( this.PlatformImpl );
                var scr = this.Screens.ScreenFromWindow( this );
                if ( scr != null )
                {
                    const int W     = 370; const int H     = 200;
                    const int OFF_X = 20;  const int OFF_Y = 50;

                    var rc = scr.WorkingArea;
                    _HostWindow_4_Notification.Width  = W;
                    _HostWindow_4_Notification.Height = H;
                    _HostWindow_4_Notification.Position = new PixelPoint( rc.Right - W - OFF_X, rc.Bottom - H - OFF_Y );
                }
                else
                {
                    _HostWindow_4_Notification.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    _HostWindow_4_Notification.WindowState           = WindowState.Maximized;
                }
                _NotificationManager = new WindowNotificationManager( _HostWindow_4_Notification ) { Position = NotificationPosition.BottomRight };

                _HostWindow_4_Notification.Show( /*this*/ );
                this.Activate();

                var notification = new Notification( _Resources_.APP_TITLE, _Resources_.ALL_DOWNLOADS_COMPLETED_NOTIFICATION, NotificationType.Information, TimeSpan.FromSeconds( 2_500 ),
                    onClose: () => { _HostWindow_4_Notification.Close(); _HostWindow_4_Notification = null; } );
                _NotificationManager.Show( notification );
            }   
        }

        private void downloadListUC_SelectionChanged( DownloadRow row )
        {
            if ( logUC.IsVisible )
            {
                logUC.SetModel( row?.Log );
            }

            SetDownloadToolButtonsStatus( row );
        }
        private void downloadListUC_UpdatedSingleRunningRow( DownloadRow row )
        {
            if ( _ShowDownloadStatistics )
            {
                this.Title = $"{DownloadListUC.GetDownloadInfoText( row )},  [{_Resources_.APP_TITLE}]";
            }
        }
        #endregion

        #region [.private methods.]
        private void SetDownloadToolButtonsStatus( DownloadRow row )
        {
            if ( row == null )
            {
                startDownloadToolButton.IsEnabled =
                    cancelDownloadToolButton.IsEnabled =
                        pauseDownloadToolButton.IsEnabled =
                            deleteDownloadToolButton.IsEnabled = false;
                deleteAllFinishedDownloadToolButton.IsEnabled = _VM.DownloadListModel.HasAnyFinished();
            }
            else
            {
                var status = row.Status;
                startDownloadToolButton .IsEnabled = status.StartDownload_IsAllowed();
                cancelDownloadToolButton.IsEnabled = status.CancelDownload_IsAllowed();
                pauseDownloadToolButton .IsEnabled = status.PauseDownload_IsAllowed();

                deleteDownloadToolButton.IsEnabled = true;
                deleteAllFinishedDownloadToolButton.IsEnabled = (status.IsFinished() || _VM.DownloadListModel.HasAnyFinished());
            }
        }
        private void SetDownloadToolButtonsStatus_NonSelected( DownloadRow row )
        {
            if ( (row != null) && row.IsFinished() )
            {
                deleteAllFinishedDownloadToolButton.IsEnabled = true;
            }
        }
        private void ShowDownloadStatisticsInTitle()
        {
            #region comm.
            /*
            if ( _ShowDownloadStatistics && (0 < _VM.DownloadListModel.RowsCount) )
            {
                #region [.if single downloads running.]
                if ( _VM.DownloadListModel.TryGetSingleRunning( out var singleRunningRow ) )
                {
                    downloadListUC_UpdatedSingleRunningRow( singleRunningRow );
                    return;
                }
                #endregion

                var stats = _VM.DownloadListModel.GetStatisticsByAllStatus();

                var finishedCount = stats[ DownloadStatus.Finished ];
                var errorCount    = stats[ DownloadStatus.Error    ];

                var finishedText  = ((0 < finishedCount) ? $", end: {finishedCount}"    : null);
                var errorText     = ((0 < errorCount   ) ? $", err: {errorCount}"       : null);

                var runningCount  = stats[ DownloadStatus.Started ] + stats[ DownloadStatus.Running ];
                var waitCount     = stats[ DownloadStatus.Wait    ];

                if ( (runningCount != 0) || (waitCount != 0) )
                {
                    var waitText = ((0 < waitCount) ? $", wait: {waitCount}" : null);

                    this.Title = $"run: {runningCount}{waitText}{finishedText}{errorText},  [{_Resources_.APP_TITLE}]";
                }
                else
                {
                    var createdCount  = stats[ DownloadStatus.Created  ];
                    var pausedCount   = stats[ DownloadStatus.Paused   ];
                    var canceledCount = stats[ DownloadStatus.Canceled ];

                    var pauseText    = ((0 < pausedCount  ) ? $", pause: {pausedCount}"    : null);
                    var canceledText = ((0 < canceledCount) ? $", cancel: {canceledCount}" : null);
                    this.Title = $"new: {createdCount}{pauseText}{canceledText}{finishedText}{errorText},  [{_Resources_.APP_TITLE}]";
                }
            }
            else
            {
                this.Title = _Resources_.APP_TITLE;
            }
            //*/
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private enum DownloadCommandEnum { Start, Pause, Cancel, Delete, DeleteAllFinished }
        private void ProcessDownloadCommand4SelectedRows( DownloadCommandEnum downloadCommand )
        {
            var rows = downloadListUC.GetSelectedDownloadRows();
            foreach ( var row in rows )
            {
                ProcessDownloadCommand( downloadCommand, row );
            }
        }
        private void ProcessDownloadCommand( DownloadCommandEnum downloadCommand, DownloadRow row = null )
        {
            if ( row == null ) row = downloadListUC.GetSelectedDownloadRow();

            if ( row != null )
            {
                switch ( downloadCommand )
                {
                    case DownloadCommandEnum.Start:
                        _VM.DownloadController.Start( row );
                        break;

                    case DownloadCommandEnum.Pause:
                        _VM.DownloadController.Pause( row );
                        break;

                    case DownloadCommandEnum.Cancel:
                        _VM.DownloadController.Cancel( row );
                        break;

                    case DownloadCommandEnum.Delete:
                        _VM.DownloadController.Cancel( row );
                        _VM.DownloadListModel.RemoveRow( row );
                        row = downloadListUC.GetSelectedDownloadRow();
                        break;

                    case DownloadCommandEnum.DeleteAllFinished:
                        _VM.DownloadListModel.RemoveAllFinished();
                        row = downloadListUC.GetSelectedDownloadRow();
                        break;
                }
            }
            else if ( downloadCommand == DownloadCommandEnum.DeleteAllFinished )
            {
                _VM.DownloadListModel.RemoveAllFinished();
            }

            SetDownloadToolButtonsStatus( row );
        }

        private async void DeleteDownloads( IReadOnlyList< DownloadRow > rows, bool deleteOutputFiles )
        {
            if ( !rows.AnyEx() ) return;

            if ( deleteOutputFiles )
            {
                this.IsEnabled = false;
                try
                {
                    using ( var cts = new CancellationTokenSource() )
                    using ( WaitBannerForm.CreateAndShow( this, cts, visibleDelayInMilliseconds: 2_000 ) )
                    {
                        await _VM.DownloadController.DeleteRowsWithOutputFiles_Parallel_UseSynchronizationContext( rows, cts.Token,
                            (row, ct) => FileHelper.TryDeleteFiles( row.GetOutputFullFileNames(), ct ),
                            (row) => _VM.DownloadListModel.RemoveRow( row ) 
                        );
                    }
                }
                catch ( OperationCanceledException )
                {
                    ;
                }
                finally
                {
                    SetDownloadToolButtonsStatus( downloadListUC.GetSelectedDownloadRow() );

                    this.IsEnabled = true;
                }
            }
            else
            {
                _VM.DownloadController.CancelAll( rows );
                _VM.DownloadListModel.RemoveRows( rows );

                SetDownloadToolButtonsStatus( downloadListUC.GetSelectedDownloadRow() );
            }
        }
        private async Task< bool > AskDeleteDownloadDialog( IReadOnlyList< DownloadRow > rows, bool askOnlyOutputFileExists, bool deleteOutputFile )
        {
            switch ( rows.Count )
            {
                case 0:
                    return (false);
                case 1:
                    return (await AskDeleteDownloadDialog( rows[ 0 ], askOnlyOutputFileExists, deleteOutputFile ));
                default:
                    var msg = $"Delete {rows.Count} downloads{(deleteOutputFile ? " with output file" : null)} ?";
                    var yes = ((await this.MessageBox_ShowQuestion( msg, this.Title, ButtonEnum.YesNo )) == ButtonResult.Yes);
                    return (yes);
            }            
        }
        private async Task< bool > AskDeleteDownloadDialog( DownloadRow row, bool askOnlyOutputFileExists, bool deleteOutputFile )
        {
            if ( row != null )
            {
                var outputFullFileName  = row.GetOutputFullFileName();
                var vfOutputFileExists  = (!outputFullFileName.EqualIgnoreCase( row.VeryFirstOutputFullFileName ) && File.Exists( row.VeryFirstOutputFullFileName ));
                var outputFileExists    = File.Exists( outputFullFileName );
                var anyOutputFileExists = (outputFileExists || vfOutputFileExists);
                if ( !anyOutputFileExists && askOnlyOutputFileExists ) return (true);
                var outputFileExistsText = (anyOutputFileExists ? "_exists_" : "no exists");
                var deleteOutputFileText = ((deleteOutputFile && anyOutputFileExists) ? " with output file" : null);
                string outputFileNameText;
                if ( vfOutputFileExists )
                {
                    outputFileNameText = $"\n\n        '{row.VeryFirstOutputFullFileName}'";

                    if ( outputFileExists )
                    {
                        outputFileNameText += $"\n        '{outputFullFileName}'";
                    }
                }
                else
                {
                    outputFileNameText = $"\n\n        '{outputFullFileName}'";
                }
                var msg = $"Delete download{deleteOutputFileText}:\n '{row.Url}' ?\n\nOutput file ({outputFileExistsText}):{outputFileNameText}";
                var yes = ((await this.MessageBox_ShowQuestion( msg, this.Title, ButtonEnum.YesNo )) == ButtonResult.Yes);
                return (yes);
            }
            return (false);
        }

        private async void OnlyDeleteOutputFiles( IReadOnlyList< DownloadRow > rows, bool ask = true )
        {
            if ( !rows.AnyEx() ) return;

            var exists_fns = rows.SelectMany( r => r.GetOutputFullFileNames() ).Where( File.Exists ).ToList( rows.Count );
            if ( exists_fns.Count == 0 ) return;

            if ( ask ) 
            {
                var msg = $"Only delete ({exists_fns.Count}) output files ? \r\n\r\n{string.Join( "\r\n", exists_fns )}";
                var yes = ((await this.MessageBox_ShowQuestion( msg, this.Title, ButtonEnum.YesNo )) == ButtonResult.Yes);
                if ( !yes ) return;
            }

            this.IsEnabled = false;
            try
            {
                using ( var cts = new CancellationTokenSource() )
                using ( WaitBannerForm.CreateAndShow( this, cts, "only delete files...", visibleDelayInMilliseconds: 2_000, out var wb ) )
                {
                    wb.SetTotalSteps( exists_fns.Count );
                    await FileHelper.DeleteFiles_UseSynchronizationContext( exists_fns, cts.Token, async (fn, ct, syncCtx) => 
                    {
                        var suc = await FileHelper.TryDeleteFile( fn, ct, fullFileName => syncCtx.Invoke(() => wb.SetCaptionText( Ellipsis.MinimizePath( fullFileName, 30 ) + ", " ) ) );
                        syncCtx.Invoke(() => wb.IncreaseSteps( null ));
                        return (suc);
                    });
                }
            }
            catch ( OperationCanceledException )
            {
                ;
            }
            finally
            {
                this.IsEnabled = true;
            }        
        }

        private bool IsWaitBannerShown() => !this.IsEnabled;

        private double? Get_LogUC_RowDefinition_Height()
        {
            UserControl uc = logUC; //downloadListUC;
            var rowDef = uc.GetValue( Grid.RowProperty );
            var grid   = uc.FindAncestorOfType< Grid >();
            if ( (grid != null) && (0 <= rowDef) && (rowDef < grid.RowDefinitions.Count) )
            {
                return (grid.RowDefinitions[ rowDef ].ActualHeight);
            }
            return (null);
        }
        private void Set_LogUC_RowDefinition_Height( double? logUC_RowDefinition_Height )
        {
            try
            {
                if ( logUC_RowDefinition_Height.HasValue )
                {
                    UserControl uc = logUC; //downloadListUC;
                    var rowDef = uc.GetValue( Grid.RowProperty );
                    var grid   = uc.FindAncestorOfType< Grid >();
                    if ( (grid != null) && (0 <= rowDef) && (rowDef < grid.RowDefinitions.Count) )
                    {
                        grid.RowDefinitions[ rowDef ].Height = new GridLength( logUC_RowDefinition_Height.Value );
                    }
                }
            }
            catch ( Exception ex)
            {
                Debug.WriteLine( ex );
            }
        }
        #endregion

        #region [.menu.]
        private GridLength? _Last_logUC_row_Height;
        private void showLogToolButton_Click( object sender, EventArgs e )
        {
            #region comm.
            /*
            var showLog = (showLogToolButton.Opacity == 1); //showLogToolButton.Checked;
            _VM.SettingsController.ShowLog = showLog;
            logUC.SetModel( (showLog ? downloadListUC.GetSelectedDownloadRow()?.Log : null) );
            //*/ 
            #endregion

            if ( logUC.Parent is Grid grid )
            {
                var row = grid.RowDefinitions.LastOrDefault();
                if ( row != null )
                {
                    var showLog = (logUC.IsVisible = !logUC.IsVisible);
                    _VM.SettingsController.ShowLog = showLog;
                    if ( showLog )
                    {
                        row.Height    = _Last_logUC_row_Height.GetValueOrDefault( new GridLength( 1, GridUnitType.Star ) );
                        row.MinHeight = 70;
                        showLogToolButton.Opacity = 1;
                    }
                    else
                    {
                        _Last_logUC_row_Height = row.Height;
                        row.Height    = new GridLength( 0, GridUnitType.Pixel );
                        row.MinHeight = 0;
                        showLogToolButton.Opacity = 0.5;
                    }
                    logUC.SetModel( (showLog ? downloadListUC.GetSelectedDownloadRow()?.Log : null) );
                }
            }
        }
        private async void copyToolButton_Click( object sender, EventArgs e )
        {
            var rows = downloadListUC.GetSelectedDownloadRows();
            if ( rows.Any() )
            {
                await this.CopyToClipboard( rows );
            }
            else
            {
                await this.MessageBox_ShowError( "Nothing for Copy to clipboard.", this.Title );
            }
        }
        private async void pasteToolButton_Click( object sender, EventArgs e )
        {
            var (success, urls) = await this.TryGetHttpUrlsFromClipboard();
            if ( success )
            {
                var autoStartDownload = KeyboardHelper.IsShiftButtonPushed().GetValueOrDefault( false );
                if ( !autoStartDownload ) urls = urls.Take( 50 ).ToList( 50 );
                _VM.AddCommand.AddNewDownloads( (urls, autoStartDownload) );
            }
            else
            {
                await this.MessageBox_ShowError( "Nothing for Paste from clipboard.", this.Title );
            }
        }

        private void startDownloadToolButton_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Start  );
        private void pauseDownloadToolButton_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Pause  );
        private void cancelDownloadToolButton_Click( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Cancel );
        private void deleteDownloadToolButton_Click( object sender, EventArgs e ) =>  DeleteDownloads( downloadListUC.GetSelectedDownloadRows(), deleteOutputFiles: KeyboardHelper.IsShiftButtonPushed().GetValueOrDefault( false ) );
        private void deleteAllFinishedDownloadToolButton_Click( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.DeleteAllFinished );

        private void downloadInstanceToolButton_ValueChanged( int downloadInstanceValue )
        {
            if ( _VM.SettingsController.UseCrossDownloadInstanceParallelism )
            {
                _VM.SettingsController.MaxCrossDownloadInstance = downloadInstanceValue;
            }
        }
        private void degreeOfParallelismToolButton_ValueChanged( int value ) => _VM.SettingsController.MaxDegreeOfParallelism = value;
        private void speedThresholdToolButton_ValueChanged( double? value ) => _VM.SettingsController.MaxSpeedThresholdInMbps = value; 
        #endregion

        #region [.context menu.]
        private void downloadListUC_MouseClickRightButton( in Point pt, DownloadRow row )
        {
            var selectedRow = row ?? downloadListUC.GetSelectedDownloadRow();
            if ( (selectedRow != null) || (0 < _VM.DownloadListModel.RowsCount) )
            {
                var selectedRow_AnyFileExists = false;
                startDownloadMenuItem            .IsEnabled = startDownloadToolButton .IsEnabled;
                cancelDownloadMenuItem           .IsEnabled = cancelDownloadToolButton.IsEnabled;
                pauseDownloadMenuItem            .IsEnabled = pauseDownloadToolButton .IsEnabled;
                deleteDownloadMenuItem           .IsEnabled = deleteDownloadToolButton.IsEnabled;
                deleteWithOutputFileMenuItem     .IsEnabled = deleteDownloadToolButton.IsEnabled && (selectedRow_AnyFileExists = FileHelper.AnyFileExists( selectedRow?.GetOutputFullFileNames() ));
                browseOutputFileMenuItem         .IsVisible = deleteWithOutputFileMenuItem.IsEnabled;
                openOutputFileMenuItem           .IsVisible = deleteWithOutputFileMenuItem.IsEnabled;
                deleteAllFinishedDownloadMenuItem.IsEnabled = deleteAllFinishedDownloadToolButton.IsEnabled;

                changeOutputDirectoryMenuItem.IsVisible = (selectedRow != null);

                SetAllDownloadsMenuItemsEnabled( allowedAll: (selectedRow == null) || (1 < _VM.DownloadListModel.RowsCount) );

                if ( selectedRow != null )
                {
                    var rows = downloadListUC.GetSelectedDownloadRows();

                    #region [.onlyDeleteOutputFileMenuItem.]
                    if ( selectedRow_AnyFileExists )
                    {
                        onlyDeleteOutputFileMenuItem.IsEnabled = !selectedRow.Status.IsRunningOrPaused();
                    }
                    else
                    {
                        var x = rows.Where( r => !r.Status.IsRunningOrPaused() ).FirstOrDefault( r => FileHelper.AnyFileExists( r.GetOutputFullFileNames() ) );
                        onlyDeleteOutputFileMenuItem.IsEnabled = (x != null);
                    }
                    #endregion

                    #region [.editDownloadMenuItem.]
                    editDownloadMenuItem.IsVisible =
                        editDownloadMenuItem_Separator.IsVisible = (rows.Count == 1) && !selectedRow.Status.IsRunningOrPaused();
                    #endregion
                }
                else
                {
                    onlyDeleteOutputFileMenuItem.IsEnabled = false;

                    editDownloadMenuItem.IsVisible =
                        editDownloadMenuItem_Separator.IsVisible = false;
                }

                #region comm.
                //pt = downloadListUC.TranslatePoint( pt, this ).GetValueOrDefault( pt );

                //var b1 = downloadListUC.Bounds;
                //var b2 = mainContextMenu.Bounds;

                //mainContextMenu.HorizontalOffset = pt.X + (- b1.Width / 2) + b2.Width / 2;
                //mainContextMenu.VerticalOffset   = pt.Y + (- b1.Height); 
                //mainContextMenu.Placement        = PlacementMode.Pointer; 
                #endregion

                mainContextMenu.Open( downloadListUC );
            }
        }
        private void SetAllDownloadsMenuItemsEnabled( bool allowedAll )
        {
            if ( allowedAll )
            {
                int start = 0, cancel = 0, pause = 0, delete = 0, deleteWithFiles = 0;
                foreach ( var row in _VM.DownloadListModel.GetRows() )
                {
                    var status = row.Status;
                    start  += status.StartDownload_IsAllowed() && !status.IsFinished() ? 1 : 0;
                    cancel += status.CancelDownload_IsAllowed() ? 1 : 0;
                    pause  += status.PauseDownload_IsAllowed()  ? 1 : 0;
                    delete++;
                    if ( FileHelper.AnyFileExists( row.GetOutputFullFileNames() ) )
                    {
                        deleteWithFiles++;
                    }
                }

                void set_enabled_and_text( MenuItem menuItem, int count )
                {
                    menuItem.IsEnabled = (0 < count);
                    if ( menuItem.Header is string text )
                    {
                        if ( menuItem.Tag == null )
                        {
                            menuItem.Tag = text;
                        }                      
                        menuItem.Header = menuItem.Tag.ToString() + ((0 < count) ? $" ({count})" : null);
                    }
                };

                set_enabled_and_text( startAllDownloadsMenuItem       , start  );
                set_enabled_and_text( cancelAllDownloadsMenuItem      , cancel );
                set_enabled_and_text( pauseAllDownloadsMenuItem       , pause  );
                set_enabled_and_text( deleteAllDownloadsMenuItem      , delete );
                set_enabled_and_text( deleteAllWithOutputFilesMenuItem, deleteWithFiles );
            }
            else
            {
                startAllDownloadsMenuItem.IsEnabled =
                    cancelAllDownloadsMenuItem.IsEnabled =
                        pauseAllDownloadsMenuItem.IsEnabled =
                            deleteAllDownloadsMenuItem.IsEnabled =
                                deleteAllWithOutputFilesMenuItem.IsEnabled = false;
            }
        }

        private void startDownloadMenuItem_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Start  );
        private void pauseDownloadMenuItem_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Pause  );
        private void cancelDownloadMenuItem_Click( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Cancel );

        private void editDownloadMenuItem_Click( object sender, EventArgs e ) => _VM.EditCommand.EditDownload( downloadListUC.GetSelectedDownloadRow() );

        private void deleteDownloadMenuItem_Click( object sender, EventArgs e ) => deleteDownloadToolButton_Click( sender, e );
        private void deleteWithOutputFileMenuItem_Click( object sender, EventArgs e ) => DeleteDownloads( downloadListUC.GetSelectedDownloadRows(), deleteOutputFiles: true );
        private void onlyDeleteOutputFileMenuItem_Click( object sender, EventArgs e ) => OnlyDeleteOutputFiles( downloadListUC.GetSelectedDownloadRows() );

        private async void browseOutputFileMenuItem_Click( object sender, EventArgs e )
        {
            if ( PlatformHelper.IsWinNT() )
            {
                var row = downloadListUC.GetSelectedDownloadRow();
                if ( FileHelper.TryGetFirstFileExists( row?.GetOutputFullFileNames(), out var outputFileName ) )
                {
                    try
                    {
                        using ( Process.Start( "explorer", "/e,/select," + outputFileName ) )
                        {
                            ;
                        }
                    }
                    catch ( Exception ex )
                    {
                        await this.MessageBox_ShowError( ex.ToString(), this.Title );
                    }
                }
            }
            else
            {
                await this.MessageBox_ShowInformation( $"'{nameof(browseOutputFileMenuItem_Click)}' => NOT IMPL", this.Title );
            }
        }
        private async void openOutputFileMenuItem_Click( object sender, EventArgs e )
        {
            var row = downloadListUC.GetSelectedDownloadRow();
            if ( (row != null) && row.IsFinishedOrError() && FileHelper.TryGetFirstFileExists( row.GetOutputFullFileNames(), out var outputFileName ) )
            {
                try
                {
                    using ( Process.Start( new ProcessStartInfo( outputFileName ) { UseShellExecute = true } ) ) {; }
                }
                catch ( Exception ex )
                {
                    await this.MessageBox_ShowError( ex.ToString(), this.Title );
                }
            }
        }

        private void startAllDownloadsMenuItem_Click( object sender, EventArgs e )
        {
            foreach ( var row in _VM.DownloadListModel.GetRows() )
            {
                var status = row.Status;
                if (  status.StartDownload_IsAllowed() && !status.IsFinished() )
                {
                    _VM.DownloadController.Start( row );
                }
            }
        }
        private void pauseAllDownloadsMenuItem_Click( object sender, EventArgs e )
        {
            foreach ( var row in _VM.DownloadListModel.GetRows() )
            {
                if ( row.Status.PauseDownload_IsAllowed() )
                {
                    _VM.DownloadController.Pause( row );
                }
            }
        }
        private void cancelAllDownloadsMenuItem_Click( object sender, EventArgs e )
        {
            foreach ( var row in _VM.DownloadListModel.GetRows() )
            {
                if ( row.Status.CancelDownload_IsAllowed() )
                {
                    _VM.DownloadController.Cancel( row );
                }
            }
        }

        private void deleteAllDownloadsMenuItem_Click( object sender, EventArgs e ) => DeleteDownloads( _VM.DownloadListModel.GetRows().ToArrayEx(), deleteOutputFiles: false );
        private void deleteAllWithOutputFilesMenuItem_Click( object sender, EventArgs e ) => DeleteDownloads( _VM.DownloadListModel.GetRows().ToArrayEx(), deleteOutputFiles: true );
        #endregion

        #region [.Change output file-name.]
        private async void downloadListUC_OutputFileNameClick( DownloadRow row )
        {
            using var f = new ChangeOutputFileForm( row );
            {
                await f.ShowDialog( this );
                if ( f.Success && FileNameCleaner4UI.TryGetOutputFileName( f.OutputFileName, out var outputFileName ) )
                {
                    f.Row.SetOutputFileName( outputFileName );
                }
            }
        }
        private async void downloadListUC_OutputDirectoryClick( DownloadRow row )
        {
            var sf = await this.StorageProvider.OpenFolderPickerAsync( new FolderPickerOpenOptions()
            {
                SuggestedStartLocation = await StorageProviderExtensions.TryGetFolderFromPathAsync( this.StorageProvider, row.OutputDirectory ),
                Title = $"Select output directory: '{row.OutputFileName}'", AllowMultiple = false,
            });
            if ( sf.AnyEx() )
            {
                var directory = sf[ 0 ].Path.LocalPath.ToString();
                if ( !directory.IsNullOrWhiteSpace() )
                {
                    row.SetOutputDirectory( directory );
                }
            }

            #region comm. prev.
            /*
            var d = new OpenFolderDialog() { Directory = row.OutputDirectory, Title = $"Select output directory: '{row.OutputFileName}'" };
            {
                var directory = await d.ShowAsync( this );
                if ( !directory.IsNullOrWhiteSpace() )
                {
                    row.SetOutputDirectory( directory );
                }
            }
            //*/
            #endregion
        }

        private string _Last_ChangeOutputDirectory;
        private async void changeOutputDirectoryMenuItem_Click( object sender, EventArgs e )
        {
            var rows = downloadListUC.GetSelectedDownloadRows();
            if ( rows.Any() )
            {
                var first_row = rows.First();
                var descr = (rows.Count == 1) ? $"Select output directory for file: '{first_row.OutputFileName}'" : $"Select output directory for ({rows.Count}) files";

                var sf = await this.StorageProvider.OpenFolderPickerAsync( new FolderPickerOpenOptions()
                {
                    SuggestedStartLocation = await StorageProviderExtensions.TryGetFolderFromPathAsync( this.StorageProvider, GetSelectedDirectory( first_row ) ),
                    Title = descr, AllowMultiple = false,
                });
                if ( sf.AnyEx() )
                {
                    var outputDirectory = sf[ 0 ].Path.LocalPath.ToString();
                    if ( !outputDirectory.IsNullOrWhiteSpace() )
                    {
                        _Last_ChangeOutputDirectory = outputDirectory;
                        foreach ( var row in rows )
                        {
                            await ChangeOutputDirectory( row, outputDirectory );
                        }
                    }
                }

                #region comm. prev.
                /*
                var d = new OpenFolderDialog() { Directory = GetSelectedDirectory( first_row ), Title = descr };
                var outputDirectory = await d.ShowAsync( this );
                if ( !outputDirectory.IsNullOrWhiteSpace() )
                {
                    _Last_ChangeOutputDirectory = outputDirectory;
                    foreach ( var row in rows )
                    {
                        await ChangeOutputDirectory( row, outputDirectory );
                    }
                }
                //*/
                #endregion
            }
        }
        private string GetSelectedDirectory( DownloadRow row ) => FileHelper.GetFirstExistsDirectory( _Last_ChangeOutputDirectory ) ?? row.OutputDirectory;
        private Task ChangeOutputFileName( DownloadRow row, string outputFileName ) => ChangeOutputFileName_Or_OutputDirectory( row, outputFileName, change_outputDirectory: false );
        private Task ChangeOutputDirectory( DownloadRow row, string outputDirectory ) => ChangeOutputFileName_Or_OutputDirectory( row, outputDirectory, change_outputDirectory: true );
        private async Task ChangeOutputFileName_Or_OutputDirectory( DownloadRow row, string outputFileName_or_outputDirectory, bool change_outputDirectory )
        {
            var prev_outputFullFileName = row.GetOutputFullFileName();

            string prev_outputFileName_or_outputDirectory;
            if ( change_outputDirectory )
            {
                prev_outputFileName_or_outputDirectory = row.OutputDirectory;
                row.SetOutputDirectory( outputFileName_or_outputDirectory );
            }
            else
            {
                prev_outputFileName_or_outputDirectory = row.OutputFileName;
                row.SetOutputFileName( outputFileName_or_outputDirectory );
            }
            var new_outputFullFileName = row.GetOutputFullFileName();

            var res = await MoveFileByRename( row, prev_outputFullFileName, new_outputFullFileName );
            switch ( res )
            {
                //case MoveFileByRenameResultEnum.Postponed: break;
                //case MoveFileByRenameResultEnum.Suc: break;
                case MoveFileByRenameResultEnum.Canceled:
                case MoveFileByRenameResultEnum.Fail:
                    //rollback
                    if ( change_outputDirectory )
                    {
                        row.SetOutputDirectory( prev_outputFileName_or_outputDirectory );
                    }
                    else
                    {
                        row.SetOutputFileName( prev_outputFileName_or_outputDirectory );
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private enum MoveFileByRenameModeEnum { OverwriteAsk, OverwriteSilent, SkipIfAlreadyExists }
        private enum MoveFileByRenameResultEnum { Postponed, Canceled, Suc, Fail }
        private async Task< MoveFileByRenameResultEnum > MoveFileByRename( DownloadRow row, string prev_outputFullFileName, string new_outputFullFileName
            , MoveFileByRenameModeEnum mode = MoveFileByRenameModeEnum.OverwriteAsk )
        {
            if ( !row.Status.IsRunningOrPaused() && File.Exists( prev_outputFullFileName ) )
            {
                switch ( mode )
                {
                    case MoveFileByRenameModeEnum.OverwriteSilent:
                        FileHelper.DeleteFile_NoThrow( new_outputFullFileName ); 
                        break;
                    case MoveFileByRenameModeEnum.OverwriteAsk: 
                        if ( File.Exists( new_outputFullFileName ) )
                        {
                            var yes = ((await this.MessageBox_ShowQuestion( $"File '{new_outputFullFileName}' already exists. Overwrite ?", "Overwrite exists file" )) != ButtonResult.Yes);
                            if ( !yes )
                            {
                                return (MoveFileByRenameResultEnum.Canceled);
                            }
                            FileHelper.DeleteFile_NoThrow( new_outputFullFileName );
                        }
                        break;
                    case MoveFileByRenameModeEnum.SkipIfAlreadyExists:
                        if ( File.Exists( new_outputFullFileName ) )
                        {
                            return (MoveFileByRenameResultEnum.Canceled);
                        }
                        break;
                }
                
                try
                {
                    File.Move( prev_outputFullFileName, new_outputFullFileName /*?? row.GetOutputFullFileName()*/ );
                    return (MoveFileByRenameResultEnum.Suc);
                }
                catch ( Exception ex )
                {
                    await this.MessageBox_ShowError( ex.ToString(), "Move/Remane output file" );
                    return  (MoveFileByRenameResultEnum.Fail);
                }
            }
            return (MoveFileByRenameResultEnum.Postponed);
        }
        #endregion

        #region [.LiveStream change max-file-size.]
        private async void downloadListUC_LiveStreamMaxFileSizeClick( DownloadRow row )
        {
            var f = new ChangeLiveStreamMaxFileSizeForm( row );
            {
                await f.ShowDialog( this );
                if ( f.Success )
                {
                    f.Row.SetLiveStreamMaxFileSizeInBytes( f.LiveStreamMaxFileSizeInBytes );
                }
            }
        }
        #endregion

    }
}
