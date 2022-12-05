using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MessageBox.Avalonia.Enums;

using m3u8.download.manager.infrastructure;
using m3u8.download.manager.ipc;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using _CollectionChangedTypeEnum_ = m3u8.download.manager.models.DownloadListModel.CollectionChangedTypeEnum;
using _Resources_                 = m3u8.download.manager.Properties.Resources;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MainWindow : Window, IDisposable
    {
        #region [.fields from markup.]
        private DownloadListUC downloadListUC;
        private LogUC          logUC;

        private MenuItem startDownloadToolButton;
        private MenuItem pauseDownloadToolButton;
        private MenuItem cancelDownloadToolButton;
        private MenuItem deleteDownloadToolButton;
        private MenuItem deleteAllFinishedDownloadToolButton;
        private MenuItem showLogToolButton;
        private MenuItem copyToolButton;
        private MenuItem pasteToolButton;
        private DegreeOfParallelismMenuItem degreeOfParallelismToolButton;
        private DownloadInstanceMenuItem    downloadInstanceToolButton;
        private SpeedThresholdToolButton    speedThresholdToolButton;

        private ContextMenu mainContextMenu;
        private MenuItem    startDownloadMenuItem;
        private MenuItem    pauseDownloadMenuItem;
        private MenuItem    cancelDownloadMenuItem;
        private MenuItem    deleteDownloadMenuItem;
        private MenuItem    deleteWithOutputFileMenuItem;
        private MenuItem    browseOutputFileMenuItem;
        private MenuItem    openOutputFileMenuItem;
        private MenuItem    deleteAllFinishedDownloadMenuItem;
        private MenuItem    startAllDownloadsMenuItem;
        private MenuItem    cancelAllDownloadsMenuItem;
        private MenuItem    pauseAllDownloadsMenuItem;
        private MenuItem    deleteAllDownloadsMenuItem;
        private MenuItem    deleteAllWithOutputFilesMenuItem;
        #endregion

        #region [.field's.]
        private MainVM _VM;
        private (string m3u8FileUrl, bool autoStartDownload)[] _InputParamsArray;
        private bool _ShowDownloadStatistics;
        #endregion

        #region [.ctor().]
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            PipeIPC.NamedPipeServer__in.ReceivedSend2FirstCopy += NamedPipeServer__in_ReceivedSend2FirstCopy;
        }
        public MainWindow( in (string m3u8FileUrl, bool autoStartDownload)[] array ) : this() => _InputParamsArray = array;
        public void Dispose() => _VM.Dispose_NoThrow();
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );
            //----------------------------------------//

            #region [.user controls.]
            downloadListUC = this.Find< DownloadListUC >( nameof(downloadListUC) );
            logUC          = this.Find< LogUC          >( nameof(logUC) );
            #endregion

            #region [.menu.]
            startDownloadToolButton  = this.Find< MenuItem >( nameof(startDownloadToolButton)  ); startDownloadToolButton .Click += startDownloadToolButton_Click;
            pauseDownloadToolButton  = this.Find< MenuItem >( nameof(pauseDownloadToolButton)  ); pauseDownloadToolButton .Click += pauseDownloadToolButton_Click;
            cancelDownloadToolButton = this.Find< MenuItem >( nameof(cancelDownloadToolButton) ); cancelDownloadToolButton.Click += cancelDownloadToolButton_Click;
            deleteDownloadToolButton = this.Find< MenuItem >( nameof(deleteDownloadToolButton) ); deleteDownloadToolButton.Click += deleteDownloadToolButton_Click;
            deleteAllFinishedDownloadToolButton = this.Find< MenuItem >( nameof(deleteAllFinishedDownloadToolButton) ); deleteAllFinishedDownloadToolButton.Click += deleteAllFinishedDownloadToolButton_Click;

            showLogToolButton = this.Find< MenuItem >( nameof(showLogToolButton) ); showLogToolButton.Click += showLogToolButton_Click;
            copyToolButton    = this.Find< MenuItem >( nameof(copyToolButton)    ); copyToolButton   .Click += copyToolButton_Click;
            pasteToolButton   = this.Find< MenuItem >( nameof(pasteToolButton)   ); pasteToolButton  .Click += pasteToolButton_Click;

            degreeOfParallelismToolButton = this.Find< DegreeOfParallelismMenuItem >( nameof(degreeOfParallelismToolButton) ); degreeOfParallelismToolButton.ValueChanged += degreeOfParallelismToolButton_ValueChanged;
            downloadInstanceToolButton    = this.Find< DownloadInstanceMenuItem    >( nameof(downloadInstanceToolButton)    ); downloadInstanceToolButton   .ValueChanged += downloadInstanceToolButton_ValueChanged;
            speedThresholdToolButton      = this.Find< SpeedThresholdToolButton    >( nameof(speedThresholdToolButton)      ); speedThresholdToolButton     .ValueChanged += speedThresholdToolButton_ValueChanged;
            #endregion

            #region [.context menu.]
            mainContextMenu                   = this.Find< ContextMenu >( nameof(mainContextMenu) ); //mainContextMenu.Styles.Add( GlobalStyles.Light );
            startDownloadMenuItem             = this.Find< MenuItem >( nameof(startDownloadMenuItem)  );            startDownloadMenuItem            .Click += startDownloadMenuItem_Click;
            pauseDownloadMenuItem             = this.Find< MenuItem >( nameof(pauseDownloadMenuItem)  );            pauseDownloadMenuItem            .Click += pauseDownloadMenuItem_Click;
            cancelDownloadMenuItem            = this.Find< MenuItem >( nameof(cancelDownloadMenuItem) );            cancelDownloadMenuItem           .Click += cancelDownloadMenuItem_Click;
            deleteDownloadMenuItem            = this.Find< MenuItem >( nameof(deleteDownloadMenuItem) );            deleteDownloadMenuItem           .Click += deleteDownloadMenuItem_Click;
            deleteWithOutputFileMenuItem      = this.Find< MenuItem >( nameof(deleteWithOutputFileMenuItem) );      deleteWithOutputFileMenuItem     .Click += deleteWithOutputFileMenuItem_Click;
            browseOutputFileMenuItem          = this.Find< MenuItem >( nameof(browseOutputFileMenuItem) );          browseOutputFileMenuItem         .Click += browseOutputFileMenuItem_Click;
            openOutputFileMenuItem            = this.Find< MenuItem >( nameof(openOutputFileMenuItem) );            openOutputFileMenuItem           .Click += openOutputFileMenuItem_Click;
            deleteAllFinishedDownloadMenuItem = this.Find< MenuItem >( nameof(deleteAllFinishedDownloadMenuItem) ); deleteAllFinishedDownloadMenuItem.Click += deleteAllFinishedDownloadToolButton_Click;
            startAllDownloadsMenuItem         = this.Find< MenuItem >( nameof(startAllDownloadsMenuItem) );         startAllDownloadsMenuItem        .Click += startAllDownloadsMenuItem_Click;
            cancelAllDownloadsMenuItem        = this.Find< MenuItem >( nameof(cancelAllDownloadsMenuItem) );        cancelAllDownloadsMenuItem       .Click += cancelAllDownloadsMenuItem_Click;
            pauseAllDownloadsMenuItem         = this.Find< MenuItem >( nameof(pauseAllDownloadsMenuItem) );         pauseAllDownloadsMenuItem        .Click += pauseAllDownloadsMenuItem_Click;
            deleteAllDownloadsMenuItem        = this.Find< MenuItem >( nameof(deleteAllDownloadsMenuItem) );        deleteAllDownloadsMenuItem       .Click += deleteAllDownloadsMenuItem_Click;
            deleteAllWithOutputFilesMenuItem  = this.Find< MenuItem >( nameof(deleteAllWithOutputFilesMenuItem) );  deleteAllWithOutputFilesMenuItem .Click += deleteAllWithOutputFilesMenuItem_Click;
            #endregion
            //----------------------------------------//

            #region [.-1-.]
            this.Title = _Resources_.APP_TITLE;
            this.DataContext = _VM = new MainVM( this );

            _VM.DownloadListModel.RowPropertiesChanged += DownloadListModel_RowPropertiesChanged;
            _VM.SettingsController.SettingsPropertyChanged += SettingsController_PropertyChanged;
            SettingsController_PropertyChanged( _VM.SettingsController.Settings, nameof(Settings.ShowDownloadStatisticsInMainFormTitle) );
            #endregion

            //----------------------------------------//

            #region [.-2-.]
            logUC.SetModel( null );
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

            downloadInstanceToolButton.IsVisible = _VM.SettingsController.MaxCrossDownloadInstance.HasValue;
            if ( _VM.SettingsController.MaxCrossDownloadInstance.HasValue )
            {
                downloadInstanceToolButton.Value = _VM.SettingsController.MaxCrossDownloadInstance.Value;
            }
            degreeOfParallelismToolButton.Value = _VM.SettingsController.MaxDegreeOfParallelism;
            speedThresholdToolButton     .Value = _VM.SettingsController.MaxSpeedThresholdInMbps;
            #endregion
        }
        #endregion

        #region [.override methods.]
        private PixelPoint _Position;
        private void RestoreBounds( string json )
        {
            if ( !json.IsNullOrEmpty() )
            {
                try
                {
                    var (x, y, width, height) = Extensions.FromJSON< (int x, int y, double width, double height) >( json );
                    if ( (double.Epsilon < Math.Abs( width )) && (double.Epsilon < Math.Abs( height )) ) //---if ( (width != default) && (height != default) )
                    {
                        this.Position   = new PixelPoint( x, y );
                        this.ClientSize = new Size( width, height );
                    }                    
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
        }
        private (int x, int y, double width, double height) GetBounds() => (x: _Position.X, y: _Position.Y, width: this.Width, height: this.Height);

        protected async override void OnOpened( EventArgs e )
        {
            base.OnOpened( e );

            #region [.restore settings.]
            if ( PlatformHelper.IsWinNT() )
            {
                this.RestoreBounds( _VM.SettingsController.MainFormPositionJson );
            }
            downloadListUC.SetColumnsWidthFromJson( _VM.SettingsController.GetDownloadListColumnsWidthJson() );
            downloadListUC.Focus();
            #endregion

            if ( !_VM.SettingsController.ShowLog ) showLogToolButton_Click( showLogToolButton, EventArgs.Empty );

            if ( _InputParamsArray.AnyEx() )
            {
                _VM.AddCommand.AddNewDownloads( _InputParamsArray );
                _InputParamsArray = null;
            }
            else if ( !BrowserIPC.CommandLine.Is_CommandLineArgs_Has__CreateAsBreakawayFromJob() )
            {
                var (success, m3u8FileUrls) = await Extensions.TryGetM3u8FileUrlsFromClipboard();
                if ( success )
                {
                    _VM.AddCommand.AddNewDownload( (m3u8FileUrls.FirstOrDefault(), false) );
                }
            }
            _VM.DownloadListModel.AddRows( DownloadRowsSerializer.FromJSON( _VM.SettingsController.DownloadRowsJson ) );
#if DEBUG
            if ( _VM.DownloadListModel.RowsCount == 0 )
            {
                _VM.DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8"   , "xz-1", Settings.Default.OutputFileDirectory) );
                _VM.DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8-12", "xz-2", Settings.Default.OutputFileDirectory) );
                _VM.DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8-34", "xz-3", Settings.Default.OutputFileDirectory) );
            }
#endif
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            #region [.save settings.]
            _VM.SettingsController.MainFormPositionJson = this.GetBounds().ToJSON();
            _VM.SettingsController.SetDownloadListColumnsWidthJson( downloadListUC.GetColumnsWidth().ToJSON() );
            _VM.SettingsController.DownloadRowsJson = DownloadRowsSerializer.ToJSON( _VM.DownloadListModel.GetRows() );
            _VM.SettingsController.SaveNoThrow();
            #endregion
        }
        protected async override void OnClosing( CancelEventArgs e )
        {
            _Position = this.Position;

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

                var result = await this.MessageBox_ShowQuestion( "Dou you want to _cancel_ all downloading and exit ?", _Resources_.APP_TITLE );
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
            if ( (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control )
            {
                switch ( e.Key )
                {
                    case Key.V: //Paste
                        var (success, m3u8FileUrls) = await Extensions.TryGetM3u8FileUrlsFromClipboard();
                        if ( success )
                        {
                            e.Handled = true;

                            var autoStartDownload = ((e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift);
                            if ( !autoStartDownload ) m3u8FileUrls = m3u8FileUrls.Take( 50/*100*/ ).ToArray();
                            _VM.AddCommand.AddNewDownloads( (m3u8FileUrls, autoStartDownload) );
                            return;
                        }
                    break;

                    case Key.C: //Copy
                        if ( downloadListUC.HasFocus )
                        {
                            var row = downloadListUC.GetSelectedDownloadRow();
                            if ( row != null )
                            {
                                e.Handled = true;
                                await Extensions.CopyM3u8FileUrlToClipboard( row.Url );
                                return;
                            }
                        }
                    break;

                    case Key.S: //Start download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand( DownloadCommandEnum.Start );
                        }
                    break;

                    case Key.P: //Pause download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand( DownloadCommandEnum.Pause );
                        }
                    break;

                    case Key.Z: //Cancel download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand( DownloadCommandEnum.Cancel );
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
                }
            }
            else
            {
                switch ( e.Key )
                {
                    case Key.Insert: //add download dialog
                    {
                        e.Handled = true;
                        var m3u8FileUrls = await Extensions.TryGetM3u8FileUrlsFromClipboardOrDefault();
                        _VM.AddCommand.AddNewDownloads( (m3u8FileUrls, false) );
                    }
                    return;

                    case Key.Delete: // Delete download
                        if ( downloadListUC.HasFocus )
                        {
                            var row = downloadListUC.GetSelectedDownloadRow();
                            var shiftPushed = ((e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift);
                            if ( await AskDeleteDownloadDialog( row, askOnlyOutputFileExists: false, deleteOutputFile: shiftPushed ) )
                            {
                                DeleteDownload( row, deleteOutputFile: shiftPushed );
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
                                if ( row.IsFinished() )
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

        #region [.private methods.]
        private async void NamedPipeServer__in_ReceivedSend2FirstCopy() => await Dispatcher.UIThread.InvokeAsync( () => ReceivedSend2FirstCopy() );
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
            switch ( propertyName )
            {
                case nameof(Settings.ShowDownloadStatisticsInMainFormTitle):
                    _ShowDownloadStatistics = settings.ShowDownloadStatisticsInMainFormTitle;
                
                    _VM.DownloadListModel.CollectionChanged -= DownloadListModel_CollectionChanged;
                    if ( _ShowDownloadStatistics )
                    {
                        _VM.DownloadListModel.CollectionChanged += DownloadListModel_CollectionChanged;
                    }
                    ShowDownloadStatisticsInTitle();
                break;

                case nameof(Settings.MaxCrossDownloadInstance): // nameof(Settings.UseCrossDownloadInstanceParallelism):
                    downloadInstanceToolButton.IsVisible = settings.MaxCrossDownloadInstance.HasValue;
                    if ( settings.MaxCrossDownloadInstance.HasValue )
                    {
                        downloadInstanceToolButton.Value = settings.MaxCrossDownloadInstance.Value;
                    }
                break;

                case nameof(Settings.MaxDegreeOfParallelism):
                    degreeOfParallelismToolButton.Value = settings.MaxDegreeOfParallelism;
                break;

                case nameof(Settings.MaxSpeedThresholdInMbps):
                    speedThresholdToolButton.Value = settings.MaxSpeedThresholdInMbps;
                break;
            }
        }
        private async void DownloadListModel_RowPropertiesChanged( DownloadRow row, string propertyName )
        {
            if ( propertyName == nameof(DownloadRow.Status) )
            {
                if ( downloadListUC.GetSelectedDownloadRow() == row )
                {
                    await Dispatcher.UIThread.InvokeAsync( () => { SetDownloadToolButtonsStatus( row ); ShowDownloadStatisticsInTitle(); } );
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync( () => { SetDownloadToolButtonsStatus_NonSelected( row ); ShowDownloadStatisticsInTitle(); } );
                }
            }
        }
        private void DownloadListModel_CollectionChanged( _CollectionChangedTypeEnum_ collectionChangedType )
        {
            if ( collectionChangedType != _CollectionChangedTypeEnum_.Sort )
            {
                ShowDownloadStatisticsInTitle();

                #region comm.
                //switch ( collectionChangedType )
                //{
                //    case _CollectionChangedTypeEnum_.BulkUpdate:
                //    case _CollectionChangedTypeEnum_.Remove:                    
                //        _LogRowsHeightStorer.LeaveOnly( (from row in _VM.DownloadListModel.GetRows() select row.Log) );
                //    break;

                //    case _CollectionChangedTypeEnum_.Clear:
                //        _LogRowsHeightStorer.Clear();
                //    break;
                //} 
                #endregion
            }
        }

        private void downloadListUC_SelectionChanged( DownloadRow row )
        {
            #region comm.
            //if ( !mainSplitContainer.Panel2Collapsed )
            {
                logUC.SetModel( row?.Log );
            } 
            #endregion

            SetDownloadToolButtonsStatus( row );
        }
        private void downloadListUC_UpdatedSingleRunningRow( DownloadRow row )
        {
            if ( _ShowDownloadStatistics )
            {
                this.Title = $"{DownloadListUC.GetDownloadInfoText( row )},  [{_Resources_.APP_TITLE}]";
            }
        }
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
        private enum DownloadCommandEnum { Start, Pause, Cancel,   Delete, DeleteAllFinished }
        private void ProcessDownloadCommand( DownloadCommandEnum downloadCommand, DownloadRow row = null )
        {
            if ( row == null )
            {
                row = downloadListUC.GetSelectedDownloadRow();
            }

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
                        #region comm. with waiting
                        /*
                        using ( var cts = new CancellationTokenSource() )
                        using ( WaitBannerUC.Create( this, cts, visibleDelayInMilliseconds: 1500 ) )
                        {
                            try
                            {
                                await _VM.DownloadController.CancelWithWait( row, cts.Token );
                                _VM.DownloadListModel.RemoveRow( row );
                            }
                            catch ( Exception ex )
                            {
                                if ( !cts.IsCancellationRequested )
                                {
                                    Debug.WriteLine( ex );
                                    this.MessageBox_ShowError( ex.ToString(), this.Text );
                                    //---throw;
                                }
                            }
                        }
                        //*/
                        #endregion

                        _VM.DownloadController.Cancel  ( row );
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

        private async void DeleteDownload( DownloadRow row, bool deleteOutputFile )
        {
            if ( row == null )
            {
                return;
            }

            if ( deleteOutputFile )
            {
                this.IsEnabled = false;
                try
                {
                    using ( var cts = new CancellationTokenSource() )
                    using ( WaitBannerForm.CreateAndShow( this, cts, visibleDelayInMilliseconds: 2_000 ) )
                    {
                        _VM.DownloadController.Cancel( row );

                        await TryDeleteFiles_Async( row.GetOutputFullFileNames(), cts.Token );
                        ProcessDownloadCommand( DownloadCommandEnum.Delete, row );                        
                    }
                }
                catch ( OperationCanceledException ) //( Exception ex ) when (cts.IsCancellationRequested)
                {
                    ; //Debug.WriteLine( ex );
                }
                finally
                {
                    this.IsEnabled = true;
                }
            }
            else
            {
                ProcessDownloadCommand( DownloadCommandEnum.Delete, row );
            }

        }
        private async void DeleteDownloadsWithOutputFiles( IList< DownloadRow > rows )
        {
            if ( !rows.AnyEx() )
            {
                return;
            }

            this.IsEnabled = false;
            try
            {
                using ( var cts = new CancellationTokenSource() )
                using ( WaitBannerForm.CreateAndShow( this, cts, visibleDelayInMilliseconds: 2_000 ) )
                {
                    foreach ( var row in rows )
                    {
                        _VM.DownloadController.Cancel( row );
                        _VM.DownloadListModel .RemoveRow( row );

                        await TryDeleteFiles_Async( row.GetOutputFullFileNames(), cts.Token );
                    }
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
        private async Task< bool > AskDeleteDownloadDialog( DownloadRow row, bool askOnlyOutputFileExists, bool deleteOutputFile )
        {
            if ( row != null )
            {
                var outputFullFileName  = row.GetOutputFullFileName();
                var vfOutputFileExists  = (!outputFullFileName.EqualIgnoreCase( row.VeryFirstOutputFullFileName ) && File.Exists( row.VeryFirstOutputFullFileName ));
                var outputFileExists    = File.Exists( outputFullFileName );
                var anyOutputFileExists = (outputFileExists || vfOutputFileExists);
                if ( !anyOutputFileExists && askOnlyOutputFileExists )
                {
                    return (true);
                }
                var outputFileExistsText = (anyOutputFileExists ? "_exists_" : "no exists");
                var deleteOutputFileText = ((deleteOutputFile && anyOutputFileExists) ? " with output file" : null);
                //---var defaultButton        = MessageBoxDefaultButton.Button1; //---(anyOutputFileExists ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1);
                var outputFileNameText   = default(string);
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
                var msg = $"Delete download{deleteOutputFileText}:\n '{row.Url}'    ?\n\nOutput file ({outputFileExistsText}):{outputFileNameText}";
                var r = await this.MessageBox_ShowQuestion( msg, this.Title, ButtonEnum.OkCancel/*, defaultButton*/ );
                return (r == ButtonResult.Ok);
            }
            return (false);
        }

        /*private static bool TryDeleteFile( string fullFileName, int attemptCount = 20, int millisecondsDelay = 100 )
        {
            for ( var n = 0; (n < attemptCount); n++ )
            {
                try
                {
                    if ( File.Exists( fullFileName ) )
                    {
                        File.Delete( fullFileName );
                    }
                    return (true);
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                    Task.Delay( millisecondsDelay ).Wait();
                }
            }
            var success = !File.Exists( fullFileName );
            return (success);
        }*/
        private static Task< bool > TryDeleteFiles_Async( string[] fullFileNames, CancellationToken ct, int millisecondsDelay = 100 )
        {
            if ( !fullFileNames.AnyEx() )
            {
                return (Task.FromResult( true ));
            }

            var task = Task.Run( () =>
            {
                var hs = fullFileNames.ToHashSet( StringComparer.InvariantCultureIgnoreCase );

                Parallel.ForEach( hs, new ParallelOptions() { CancellationToken = ct }, fullFileName =>
                {
                    for ( ; !ct.IsCancellationRequested; )
                    {
                        try
                        {
                            if ( File.Exists( fullFileName ) )
                            {
                                File.Delete( fullFileName );
                            }
                            return;
                        }
                        catch ( Exception ex )
                        {
                            Debug.WriteLine( ex );
                            Task.Delay( millisecondsDelay ).Wait( ct );
                        }
                    }
                });

                //var success = !Extensions.AnyFileExists( hs );
                //return (success);
                return (true);
            }, ct );
            return (task);
        }

        private bool IsWaitBannerShown() => !this.IsEnabled;
        #endregion

        #region [.menu.]
#if DEBUG
        private void addNewDownloadToolButton_Click( object sender, EventArgs e ) => _VM.AddCommand.AddNewDownload( ($"http://xzxzzxzxxz.ru/{(new Random().Next())}/asd.egf", false) );
#else
        private void addNewDownloadToolButton_Click( object sender, EventArgs e ) => _VM.AddCommand.AddNewDownload( (null, false) );
#endif
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
                        row.Height    = _Last_logUC_row_Height.GetValueOrDefault( new GridLength( default, GridUnitType.Star ) );
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
            var row = downloadListUC.GetSelectedDownloadRow();
            if ( row != null )
            {
                await Extensions.CopyM3u8FileUrlToClipboard( row.Url );
            }
            else
            {
                await this.MessageBox_ShowError( "Nothing for copy to clipboard.", this.Title );
            }
        }
        private async void pasteToolButton_Click( object sender, EventArgs e )
        {
            var (success, m3u8FileUrls) = await Extensions.TryGetM3u8FileUrlsFromClipboard();
            if ( success )
            {
                var autoStartDownload = KeyboardHelper.IsShiftButtonPushed().GetValueOrDefault( false );
                if ( !autoStartDownload ) m3u8FileUrls = m3u8FileUrls.Take( 50/*100*/ ).ToArray();
                _VM.AddCommand.AddNewDownloads( (m3u8FileUrls, autoStartDownload) );
            }
            else
            {
                await this.MessageBox_ShowError( "Nothing for paste from clipboard.", this.Title );
            }
        }
        private void aboutToolButton_Click( object sender, EventArgs e ) => _VM.AboutCommand.Execute( null );

        private void startDownloadToolButton_Click ( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Start  );
        private void pauseDownloadToolButton_Click ( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Pause  );
        private void cancelDownloadToolButton_Click( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Cancel );
        private async void deleteDownloadToolButton_Click( object sender, EventArgs e )
        {
            var row = downloadListUC.GetSelectedDownloadRow();
            var shiftPushed = KeyboardHelper.IsShiftButtonPushed().GetValueOrDefault( false );
            //---var shiftPushed = (KeyboardDevice.Instance is Avalonia.Win32.Input.WindowsKeyboardDevice wkd) ? ((wkd.Modifiers & ) == ) : false;
            if ( await AskDeleteDownloadDialog( row, askOnlyOutputFileExists: true, deleteOutputFile: shiftPushed ) )
            {
                DeleteDownload( row, deleteOutputFile: shiftPushed );
            }
        }
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
        private void downloadListUC_MouseClickRightButton( Point pt, DownloadRow row )
        {
            if ( (row != null) || (0 < _VM.DownloadListModel.RowsCount) )
            {
                startDownloadMenuItem            .IsEnabled = startDownloadToolButton .IsEnabled;
                cancelDownloadMenuItem           .IsEnabled = cancelDownloadToolButton.IsEnabled;
                pauseDownloadMenuItem            .IsEnabled = pauseDownloadToolButton .IsEnabled;
                deleteDownloadMenuItem           .IsEnabled = deleteDownloadToolButton.IsEnabled;
                deleteWithOutputFileMenuItem     .IsEnabled = deleteDownloadToolButton.IsEnabled && Extensions.AnyFileExists( row?.GetOutputFullFileNames() );
                browseOutputFileMenuItem         .IsVisible = deleteWithOutputFileMenuItem.IsEnabled;
                openOutputFileMenuItem           .IsVisible = deleteWithOutputFileMenuItem.IsEnabled;
                deleteAllFinishedDownloadMenuItem.IsEnabled = deleteAllFinishedDownloadToolButton.IsEnabled;

                var allowedAll = (row == null) || (1 < _VM.DownloadListModel.RowsCount);
                SetAllDownloadsMenuItemsEnabled( allowedAll );

                var is_zero = (new Random().Next( 0, 2 ) == 0);
                var ctrl    = is_zero ? (Control) this : downloadListUC;
                pt = is_zero ? pt : this.TranslatePoint( pt, downloadListUC ).GetValueOrDefault( pt );

                mainContextMenu.HorizontalOffset = pt.X;
                mainContextMenu.VerticalOffset   = pt.Y;
                mainContextMenu.PlacementMode    = PlacementMode.Left;
                mainContextMenu.Open( ctrl );
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
                    if ( Extensions.AnyFileExists( row.GetOutputFullFileNames() ) )
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

        private void startDownloadMenuItem_Click ( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Start  );
        private void pauseDownloadMenuItem_Click ( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Pause  );
        private void cancelDownloadMenuItem_Click( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Cancel );

        private void deleteDownloadMenuItem_Click( object sender, EventArgs e ) => deleteDownloadToolButton_Click( sender, e );
        private void deleteWithOutputFileMenuItem_Click( object sender, EventArgs e ) => DeleteDownload( downloadListUC.GetSelectedDownloadRow(), deleteOutputFile: true );

        private async void browseOutputFileMenuItem_Click( object sender, EventArgs e )
        {
            if ( PlatformHelper.IsWinNT() )
            {
                var row = downloadListUC.GetSelectedDownloadRow();
                if ( Extensions.TryGetFirstFileExists( row?.GetOutputFullFileNames(), out var outputFileName ) )
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
            if ( (row != null) && row.IsFinished() && Extensions.TryGetFirstFileExists( row.GetOutputFullFileNames(), out var outputFileName ) )
            {
                try
                {
                    using ( Process.Start( new ProcessStartInfo( outputFileName ) { UseShellExecute = true } ) )
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
        /*private void downloadListUC_EnterKeyDown( object sender, EventArgs e )
        {
            var row = downloadListUC.GetSelectedDownloadRow();
            if ( row != null )
            {
                if ( row.IsFinished() )
                {
                    openOutputFileMenuItem_Click( this, e );
                }
                else
                {
                    downloadListUC_OutputFileNameClick( row );
                }
            }
        }*/

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

        private void deleteAllDownloadsMenuItem_Click( object sender, EventArgs e )
        {
            var rows = _VM.DownloadListModel.GetRows().ToArray();
            foreach ( var row in rows )
            {
                _VM.DownloadController.Cancel( row );
                //_VM.DownloadListModel.RemoveRow( row );
            }
            _VM.DownloadListModel.RemoveAll( rows );

            //-2-//
            SetDownloadToolButtonsStatus( downloadListUC.GetSelectedDownloadRow() );
        }
        private void deleteAllWithOutputFilesMenuItem_Click( object sender, EventArgs e )
        {
            DeleteDownloadsWithOutputFiles( _VM.DownloadListModel.GetRows().ToArray() );

            //-2-//
            SetDownloadToolButtonsStatus( downloadListUC.GetSelectedDownloadRow() );
        }
        #endregion

        #region [.ChangeOutputFileForm.]
        private async void downloadListUC_OutputFileNameClick( DownloadRow row )
        {
            using var f = new ChangeOutputFileForm( row );
            {
                await f.ShowDialog( this );
                if ( f.Success && FileNameCleaner.TryGetOutputFileName( f.OutputFileName, out var outputFileName ) )
                {
                    f.Row.SetOutputFileName( outputFileName );
                }
            }
        }
        private async void downloadListUC_OutputDirectoryClick( DownloadRow row )
        {
            var d = new OpenFolderDialog() { Directory = row.OutputDirectory,
                                             Title     = $"Select output directory: '{row.OutputFileName}'" };
            {
                var directory = await d.ShowAsync( this );
                if ( !directory.IsNullOrWhiteSpace() )
                {
                    row.SetOutputDirectory( directory );
                    //---downloadListUC.InvalidateVisual();
                }
            }
        }
        #endregion

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
    }
}
