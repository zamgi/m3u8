using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.ipc;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using m3u8.download.manager.ui.infrastructure;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;
using _ReceivedInputParamsArrayEventHandler_ = m3u8.download.manager.ipc.PipeIPC.NamedPipeServer__in.ReceivedInputParamsArrayEventHandler;
using _ReceivedSend2FirstCopyEventHandler_   = m3u8.download.manager.ipc.PipeIPC.NamedPipeServer__in.ReceivedSend2FirstCopyEventHandler;
using _CollectionChangedTypeEnum_            = m3u8.download.manager.models.DownloadListModel.CollectionChangedTypeEnum;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class MainForm : Form, IDisposable
    {
        #region [.fields.]
        private (string m3u8FileUrl, bool autoStartDownload)[] _InputParamsArray;
        private _ReceivedInputParamsArrayEventHandler_ _ReceivedInputParamsArrayEventHandler;
        private _ReceivedSend2FirstCopyEventHandler_   _ReceivedSend2FirstCopyEventHandler;        

        private DownloadListModel                _DownloadListModel;
        private DownloadController               _DownloadController;
        private SettingsPropertyChangeController _SettingsController;
        private LogRowsHeightStorer              _LogRowsHeightStorer;
        private Action< DownloadRow, string >    _DownloadListModel_RowPropertiesChangedAction;
        private bool                             _ShowDownloadStatistics;
        private HashSet< string >                _ExternalProgQueue;
        private NotifyIcon                       _NotifyIcon;
#if NETCOREAPP
        private static string _APP_TITLE_ => Resources.APP_TITLE__NET_CORE;
#else
        private static string _APP_TITLE_ => Resources.APP_TITLE;
#endif 
        #endregion

        #region [.ctor().]
        private MainForm()
        {
            InitializeComponent();
            this.Text = _APP_TITLE_;
            //----------------------------------------//

            _DownloadListModel_RowPropertiesChangedAction = new Action< DownloadRow, string >( DownloadListModel_RowPropertiesChanged );

            _SettingsController = new SettingsPropertyChangeController();
            _LogRowsHeightStorer = new LogRowsHeightStorer();

            _DownloadListModel  = new DownloadListModel();
            _DownloadListModel.RowPropertiesChanged += DownloadListModel_RowPropertiesChanged;            
            _DownloadController = new DownloadController( _DownloadListModel, _SettingsController );

            _SettingsController.SettingsPropertyChanged += SettingsController_PropertyChanged;
            SettingsController_PropertyChanged( _SettingsController.Settings, nameof(Settings.ShowDownloadStatisticsInMainFormTitle) );
            SettingsController_PropertyChanged( _SettingsController.Settings, nameof(Settings.ExternalProgCaption) );
            SettingsController_PropertyChanged( _SettingsController.Settings, nameof(Settings.ShowAllDownloadsCompleted_Notification) );

            logUC.SetSettingsController( _SettingsController );
            logUC.SetLogRowsHeightStorer( _LogRowsHeightStorer );

            downloadListUC.SetModel( _DownloadListModel );
            downloadListUC.KeyDown += (s, e) => this.OnKeyDown( e );
            downloadListUC.MouseClickRightButton   += downloadListUC_MouseClickRightButton;
            downloadListUC.UpdatedSingleRunningRow += downloadListUC_UpdatedSingleRunningRow;
            downloadListUC.DoubleClickEx           += openOutputFileMenuItem_Click;
            statusBarUC.SetDownloadController( _DownloadController );
            statusBarUC.SetSettingsController( _SettingsController );
            statusBarUC.TrackItemsCount( downloadListUC );

            _ReceivedInputParamsArrayEventHandler = AddNewDownloads;
            _ReceivedSend2FirstCopyEventHandler   = ReceivedSend2FirstCopy;
            PipeIPC.NamedPipeServer__in.ReceivedInputParamsArray += NamedPipeServer__in_ReceivedInputParamsArray;
            PipeIPC.NamedPipeServer__in.ReceivedSend2FirstCopy   += NamedPipeServer__in_ReceivedSend2FirstCopy;

            NameCleaner.ResetExcludesWords( _SettingsController.NameCleanerExcludesWords );

            showLogToolButton.Checked = _SettingsController.ShowLog;
            showLogToolButton_Click( showLogToolButton, EventArgs.Empty );

            downloadInstanceToolButton.Visible = _SettingsController.MaxCrossDownloadInstance.HasValue;
            if ( _SettingsController.MaxCrossDownloadInstance.HasValue )
            {
                downloadInstanceToolButton.Value = _SettingsController.MaxCrossDownloadInstance.Value;
            }
            //degreeOfParallelismToolButton.Visible = ;
            degreeOfParallelismToolButton.Value = _SettingsController.MaxDegreeOfParallelism;
            speedThresholdToolButton     .Value = _SettingsController.MaxSpeedThresholdInMbps;

            _ExternalProgQueue = new HashSet< string >( StringComparer.InvariantCultureIgnoreCase );
        }
        public MainForm( in (string m3u8FileUrl, bool autoStartDownload)[] array ) : this() => _InputParamsArray = array;

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();

                _DownloadController.Dispose();
                _SettingsController.Dispose();
                _NotifyIcon?.Dispose();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.override methods.]
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !base.DesignMode )
            {
                FormPositionStorer.Load( this, _SettingsController.MainFormPositionJson );
                _DownloadListModel.AddRows( DownloadRowsSerializer.FromJSON( _SettingsController.DownloadRowsJson ) );
            }
#if DEBUG
            if ( _DownloadListModel.RowsCount == 0 )
            {
                _DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8"   , "xz-1", Settings.Default.OutputFileDirectory) );
                _DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8-12", "xz-2", Settings.Default.OutputFileDirectory) );
                _DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8-34", "xz-3", Settings.Default.OutputFileDirectory) );
            }
#endif
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            if ( !base.DesignMode )
            {
                _SettingsController.MainFormPositionJson = FormPositionStorer.Save( this );
                _SettingsController.DownloadRowsJson     = DownloadRowsSerializer.ToJSON( _DownloadListModel.GetRows() );
                _SettingsController.SaveNoThrow();
            }
        }
        protected override void OnShown( EventArgs e )
        {
            base.OnShown( e );

            if ( _InputParamsArray.AnyEx() )
            {
                AddNewDownloads( _InputParamsArray );
                _InputParamsArray = null;
            }
            else if ( !BrowserIPC.CommandLine.Is_CommandLineArgs_Has__CreateAsBreakawayFromJob() && 
                       Extensions.TryGetM3u8FileUrlsFromClipboard( out var m3u8FileUrls ) 
                    )
            {
                AddNewDownload( (m3u8FileUrls.FirstOrDefault(), false) );
            }
        }
        protected override void OnClosing( CancelEventArgs e )
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
            if ( _DownloadController.IsDownloading )
            {
                if ( this.WindowState == FormWindowState.Minimized )
                {
                    this.WindowState = FormWindowState.Normal;
                }

                if ( this.MessageBox_ShowQuestion( "Dou you want to [CANCEL] all downloading and exit ?", _APP_TITLE_, MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2 ) == DialogResult.Yes )
                {
                    const int WAIT_Milliseconds = 10_000;

                    //waiting for all canceled and becomes finished
                    for ( var sw = Stopwatch.StartNew(); ; )
                    {
                        _DownloadController.CancelAll();
                        Application.DoEvents();

                        if ( !_DownloadController.IsDownloading || (WAIT_Milliseconds <= sw.ElapsedMilliseconds) )
                        {
                            break;
                        }
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        protected override void OnKeyDown( KeyEventArgs e )
        {
            if ( e.Control )  //Ctrl
            {
                switch ( e.KeyCode )
                {
                    case Keys.V: //Paste
                        if ( Extensions.TryGetM3u8FileUrlsFromClipboard( out var m3u8FileUrls ) )
                        {
                            e.SuppressKeyPress = true;

                            var autoStartDownload = e.Shift;
                            if ( !autoStartDownload ) m3u8FileUrls = m3u8FileUrls.Take( 50/*100*/ ).ToArray();
                            AddNewDownloads( (m3u8FileUrls, autoStartDownload) );
                            return;
                        }
                    break;

                    case Keys.C: //Copy
                        if ( downloadListUC.HasFocus )
                        {
                            var rows = downloadListUC.GetSelectedDownloadRows();
                            if ( rows.Any() )
                            {
                                e.SuppressKeyPress = true;
                                Extensions.CopyM3u8FileUrlToClipboard( rows );
                                return;
                            }
                        }
                    break;

                    case Keys.S: //Start download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Start );
                        }
                    break;

                    case Keys.P: //Pause download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Pause );
                        }
                    break;

                    case Keys.Z: //Cancel download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Cancel );
                        }
                    break;

                    case Keys.W: //Exit | Close
                        this.Close();
                    break;

                    case Keys.D: //Minimized window
                        this.WindowState = FormWindowState.Minimized;
                    break;

                    case Keys.B: //Browse output file
                        if ( downloadListUC.HasFocus )
                        {
                            browseOutputFileMenuItem_Click( this, EventArgs.Empty );
                        }
                    break;
                    case Keys.O: //Open output file
                        if ( downloadListUC.HasFocus )
                        {
                            openOutputFileMenuItem_Click( this, EventArgs.Empty );
                        }
                    break;
                    case Keys.E: //Open output file with External progs
                        if ( downloadListUC.HasFocus )
                        {
                            openOutputFilesWithExternalMenuItem_Click( this, EventArgs.Empty );
                        }
                    break;
                }
            }
            else
            {
                switch ( e.KeyCode )
                {
                    case Keys.Insert: //add download dialog
                    {
                        e.SuppressKeyPress = true;
                        AddNewDownloads( (Extensions.TryGetM3u8FileUrlsFromClipboardOrDefault(), false) );
                    }
                    return;

                    case Keys.Delete: // Delete download
                        if ( downloadListUC.HasFocus )
                        {
                            var rows = downloadListUC.GetSelectedDownloadRows();
                            if ( AskDeleteDownloadDialog( rows, askOnlyOutputFileExists: false, deleteOutputFile: e.Shift ) )
                            {
                                DeleteDownloads( rows.ToArrayEx(), deleteOutputFiles: e.Shift );
                            }
                        }
                    break;

                    case Keys.F1: //about
                        aboutToolButton_Click( this, EventArgs.Empty );
                    break;

                    case Keys.Enter: //change output-file dialog || Open output file
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

        #region [.private methods.]
        private void NamedPipeServer__in_ReceivedInputParamsArray( (string m3u8FileUrl, bool autoStartDownload)[] array ) => this.BeginInvoke( _ReceivedInputParamsArrayEventHandler, array );
        private void NamedPipeServer__in_ReceivedSend2FirstCopy() => this.BeginInvoke( _ReceivedSend2FirstCopyEventHandler );
        private void ReceivedSend2FirstCopy()
        {
            if ( this.WindowState == FormWindowState.Minimized )
            {
                this.WindowState = WinApi.GetWindowPlacement( this.Handle ).ToFormWindowState();
            }
            this.Activate();
            WinApi.SetForegroundWindow( this.Handle );

            statusBarUC.ShowDisappearingMessage( $"Received signal about try start other copy of this application: {DateTime.Now}." );
        }

        private void SettingsController_PropertyChanged( Settings settings, string propertyName )
        {
            switch ( propertyName )
            {
                case nameof(Settings.ShowDownloadStatisticsInMainFormTitle):
                    _ShowDownloadStatistics = settings.ShowDownloadStatisticsInMainFormTitle;
                
                    _DownloadListModel.CollectionChanged -= DownloadListModel_CollectionChanged;
                    if ( _ShowDownloadStatistics )
                    {
                        _DownloadListModel.CollectionChanged += DownloadListModel_CollectionChanged;
                    }
                    ShowDownloadStatisticsInTitle();
                    break;

                case nameof(Settings.MaxCrossDownloadInstance): // nameof(Settings.UseCrossDownloadInstanceParallelism):
                    downloadInstanceToolButton.Visible = settings.MaxCrossDownloadInstance.HasValue;
                    if ( settings.MaxCrossDownloadInstance.HasValue )
                    {
                        downloadInstanceToolButton.Value = settings.MaxCrossDownloadInstance.Value;
                    }
                    break;

                case nameof(Settings.MaxDegreeOfParallelism):
                    degreeOfParallelismToolButton.Value = settings.MaxDegreeOfParallelism;
                    break;

                case nameof(Settings.ExternalProgCaption):
                    openOutputFilesWithExternalMenuItem.Text = $"    Open with '{settings.ExternalProgCaption}'";
                    break;

                case nameof(Settings.MaxSpeedThresholdInMbps):
                    speedThresholdToolButton.Value = settings.MaxSpeedThresholdInMbps;
                    break;

                case nameof(Settings.ShowAllDownloadsCompleted_Notification):
                    _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged;
                    if ( settings.ShowAllDownloadsCompleted_Notification )
                    {
                        _DownloadController.IsDownloadingChanged += DownloadController_IsDownloadingChanged;
                    }
                    break;
            }
        }
        private void ShowDownloadStatisticsInTitle()
        {
            if ( _ShowDownloadStatistics && (0 < _DownloadListModel.RowsCount) )
            {
                #region [.if single downloads running.]
                if ( _DownloadListModel.TryGetSingleRunning( out var singleRunningRow ) )
                {
                    downloadListUC_UpdatedSingleRunningRow( singleRunningRow );
                    return;
                }
                #endregion

                var stats = _DownloadListModel.GetStatisticsByAllStatus();

                var finishedCount = stats[ DownloadStatus.Finished ];
                var errorCount    = stats[ DownloadStatus.Error    ];

                var finishedText  = ((0 < finishedCount) ? $", end: {finishedCount}"    : null);
                var errorText     = ((0 < errorCount   ) ? $", err: {errorCount}"       : null);

                var runningCount  = stats[ DownloadStatus.Started ] + stats[ DownloadStatus.Running ];
                var waitCount     = stats[ DownloadStatus.Wait    ];

                if ( (runningCount != 0) || (waitCount != 0) )
                {
                    var waitText = ((0 < waitCount) ? $", wait: {waitCount}" : null);

                    this.Text = $"run: {runningCount}{waitText}{finishedText}{errorText},  [{_APP_TITLE_}]";
                }
                else
                {
                    var createdCount  = stats[ DownloadStatus.Created  ];
                    var pausedCount   = stats[ DownloadStatus.Paused   ];
                    var canceledCount = stats[ DownloadStatus.Canceled ];

                    var pauseText    = ((0 < pausedCount  ) ? $", pause: {pausedCount}"    : null);
                    var canceledText = ((0 < canceledCount) ? $", cancel: {canceledCount}" : null);
                    this.Text = $"new: {createdCount}{pauseText}{canceledText}{finishedText}{errorText},  [{_APP_TITLE_}]";
                }
            }
            else
            {
                this.Text = _APP_TITLE_;
            }
        }

        private void DownloadListModel_CollectionChanged( _CollectionChangedTypeEnum_ changedType, DownloadRow row )
        {
            if ( changedType != _CollectionChangedTypeEnum_.Sort )
            {
                if ( this.InvokeRequired )
                {
                    this.BeginInvoke( DownloadListModel_CollectionChanged, changedType, row );
                    return;
                }

                ShowDownloadStatisticsInTitle();

                switch ( changedType )
                {
                    case _CollectionChangedTypeEnum_.BulkUpdate:
                    case _CollectionChangedTypeEnum_.Remove:                    
                        _LogRowsHeightStorer.LeaveOnly( (from _row in _DownloadListModel.GetRows() select _row.Log) );
                        if ( changedType == _CollectionChangedTypeEnum_.Remove )
                        {
                            _ExternalProgQueue.Remove( row?.GetOutputFullFileNames() );
                        }
                    break;

                    case _CollectionChangedTypeEnum_.Clear:
                        _LogRowsHeightStorer.Clear();
                        _ExternalProgQueue  .Clear();
                    break;

                    case _CollectionChangedTypeEnum_.Add:
                        if ( Settings.Default.ExternalProgApplyByDefault )
                        {
                            _ExternalProgQueue.AddIf( row?.GetOutputFullFileName() /*(from _row in _DownloadListModel.GetRows() select _row.GetOutputFullFileName())*/ );
                        }
                    break;
                }
            }
        }
        private void DownloadListModel_RowPropertiesChanged( DownloadRow row, string propertyName )
        {
            switch ( propertyName )
            {
                case nameof(DownloadRow.Status):
                {
                    if ( this.InvokeRequired )
                    {
                        this.BeginInvoke( _DownloadListModel_RowPropertiesChangedAction, row, propertyName );
                        return;
                    }

                    if ( downloadListUC.GetSelectedDownloadRow() == row )
                    {
                        SetDownloadToolButtonsStatus( row );

                        if ( row.IsError() )
                        {
                            logUC.AdjustRowsHeightAndColumnsWidthSprain();
                        }
                    }
                    else
                    {
                        SetDownloadToolButtonsStatus_NonSelected( row );
                    }
                    ShowDownloadStatisticsInTitle();

                    #region [.run External progs if need.]                    
                    if ( row.IsFinished()/*.IsFinishedOrError()*/ && Extensions.TryGetFirstFileExists/*NonZeroLength*/( row.GetOutputFullFileNames(), out var outputFileName ) && _ExternalProgQueue.Contains( outputFileName ) )
                    {
                        _ExternalProgQueue.Remove( row.GetOutputFullFileNames() );

                        const long MIN_NON_ZERO_FILE_LENGTH_IN_BYTES = 1_024 * 100; //100KB
                        if ( MIN_NON_ZERO_FILE_LENGTH_IN_BYTES <= new FileInfo( outputFileName ).Length ) //NonZeroLength
                        {
                            ExternalProg_Run_IfExists( Settings.Default.ExternalProgFilePath, $"\"{outputFileName}\"" );
                        }
                    }
                    #endregion
                }
                break;
            }
        }

        private void DownloadController_IsDownloadingChanged( bool isDownloading )
        {
            if ( isDownloading ) 
            {
                if ( _NotifyIcon != null ) _NotifyIcon.Visible = false;
            }
            else
            {
                if ( _NotifyIcon == null )
                {
                    _NotifyIcon = new NotifyIcon() { Visible = true, Icon = Resources.m3u8_32x36, Text = _APP_TITLE_ };
                    var eventHandler = new EventHandler( (_, _) => _NotifyIcon.Visible = false );
                    _NotifyIcon.BalloonTipClicked += eventHandler;
                    _NotifyIcon.BalloonTipClosed  += eventHandler;
                }
                else
                {
                    _NotifyIcon.Visible = true;
                }
                _NotifyIcon.ShowBalloonTip( 2_500, _APP_TITLE_, Resources.ALL_DOWNLOADS_COMPLETED_NOTIFICATION, ToolTipIcon.Info );
            }
        }

        private void downloadListUC_UpdatedSingleRunningRow( DownloadRow row )
        {
            if ( _ShowDownloadStatistics )
            {
                this.Text = $"{DownloadListUC.GetDownloadInfoText( row )},  [{_APP_TITLE_}]";
            }
            //else if ( this.Text != _APP_TITLE_ )
            //{
            //    this.Text = _APP_TITLE_;
            //}
        }
        private void downloadListUC_SelectionChanged( DownloadRow row )
        {
            if ( !mainSplitContainer.Panel2Collapsed )
            {
                if ( row == null )
                {
                    logUC.SetModel( null );
                }
                else
                {
                    logUC.SetModel( row.Log );

                    if ( !_LogRowsHeightStorer.ContainsModel( row.Log ) )
                    {
                        logUC.AdjustRowsHeightAndColumnsWidthSprain();
                    }
                }
            }

            SetDownloadToolButtonsStatus( row );
        }
        private bool downloadListUC_IsDrawCheckMark( DownloadRow row ) => _ExternalProgQueue.Contains( row.GetOutputFullFileName() );

        /// <summary>
        /// 
        /// </summary>
        private enum DownloadCommandEnum { Start, Pause, Cancel,   Delete, DeleteAllFinished }
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
                        _DownloadController.Start( row );
                        break;

                    case DownloadCommandEnum.Pause:
                        _DownloadController.Pause( row );
                        break;

                    case DownloadCommandEnum.Cancel:
                        _DownloadController.Cancel( row );
                        break;

                    case DownloadCommandEnum.Delete:
                        _DownloadController.Cancel   ( row );
                        _DownloadListModel .RemoveRow( row );
                        row = downloadListUC.GetSelectedDownloadRow();
                        break;

                    case DownloadCommandEnum.DeleteAllFinished:
                        _DownloadListModel.RemoveAllFinished();
                        row = downloadListUC.GetSelectedDownloadRow();
                        break;
                }
            }
            else if ( downloadCommand == DownloadCommandEnum.DeleteAllFinished )
            {
                _DownloadListModel.RemoveAllFinished();
            }

            SetDownloadToolButtonsStatus( row );
        }
        private void SetDownloadToolButtonsStatus( DownloadRow row )
        {
            if ( row == null )
            {
                startDownloadToolButton.Enabled =
                    cancelDownloadToolButton.Enabled =
                        pauseDownloadToolButton.Enabled =
                            deleteDownloadToolButton.Enabled = false;
                deleteAllFinishedDownloadToolButton.Enabled = _DownloadListModel.HasAnyFinished();
            }
            else
            {
                var status = row.Status;
                startDownloadToolButton .Enabled = StartDownload_IsAllowed ( status );
                cancelDownloadToolButton.Enabled = CancelDownload_IsAllowed( status );
                pauseDownloadToolButton .Enabled = PauseDownload_IsAllowed ( status );

                deleteDownloadToolButton.Enabled = true;
                deleteAllFinishedDownloadToolButton.Enabled = (status.IsFinished() || _DownloadListModel.HasAnyFinished());
            }
        }
        private void SetDownloadToolButtonsStatus_NonSelected( DownloadRow row )
        {
            if ( (row != null) && row.IsFinished() )
            {
                deleteAllFinishedDownloadToolButton.Enabled = true;
            }
        }
        private async void DeleteDownloads( DownloadRow[] rows, bool deleteOutputFiles )
        {
            if ( !rows.AnyEx() )
            {
                return;
            }

            if ( deleteOutputFiles )
            {
                this.SetEnabledAllChildControls( false );
                //SuspendDrawing_DownloadListUC_And_Log();
                try
                {
                    using ( var cts = new CancellationTokenSource() )
                    using ( WaitBannerUC.Create( this, cts, "delete files...", visibleDelayInMilliseconds: 2_000 ) )
                    {
                        await _DownloadController.DeleteRowsWithOutputFiles_Parallel_UseSynchronizationContext( rows, cts.Token,
                            (row, ct) => FileDeleter.TryDeleteFiles( row.GetOutputFullFileNames(), ct ),
                            (row) =>
                            {
                                _DownloadListModel.RemoveRow( row );
                                _ExternalProgQueue.Remove( row.GetOutputFullFileNames() );
                            }
                        );

                        #region [.parallel.]
                        /*
                        var syncCtx = SynchronizationContext.Current;
                        await Task.Run(() =>
                        {
                            Parallel.ForEach( rows, new ParallelOptions() { CancellationToken = cts.Token, MaxDegreeOfParallelism = rows.Length }, row =>
                            {
                                using ( var statusTran = _DownloadController.CancelIfInProgress_WithTransaction( row ) )
                                {
                                    FileDeleter.TryDeleteFiles( row.GetOutputFullFileNames(), cts.Token );
                                    statusTran.CommitStatus();

                                    syncCtx.Invoke(() =>
                                    {
                                        _DownloadListModel.RemoveRow( row );
                                        _ExternalProgQueue.Remove( row.GetOutputFullFileNames() );
                                    });
                                }
       
                            });
                        });
                        */
                        #endregion

                        #region comm prev. [.consecutively.]
                        //await _DownloadController.DeleteRowsWithOutputFiles_Consecutively( rows, cts.Token,
                        //    (row, ct) => FileDeleter.TryDeleteFiles_Async( row.GetOutputFullFileNames(), ct ),
                        //    (row) =>
                        //    {
                        //        _DownloadListModel.RemoveRow( row );
                        //        _ExternalProgQueue.Remove( row.GetOutputFullFileNames() );
                        //    }
                        //);

                        /*foreach ( var row in rows )
                        {
                            using ( var statusTran = _DownloadController.CancelIfInProgress_WithTransaction( row ) )
                            {
                                await FileDeleter.TryDeleteFiles_Async( row.GetOutputFullFileNames(), cts.Token );
                                statusTran.CommitStatus();

                                _DownloadListModel.RemoveRow( row );
                                _ExternalProgQueue.Remove( row.GetOutputFullFileNames() );
                            }
                        }
                        //*/
                        #endregion
                    }
                }
                catch ( OperationCanceledException )
                {
                    ;
                }
                finally
                {
                    SetDownloadToolButtonsStatus( downloadListUC.GetSelectedDownloadRow() );

                    //ResumeDrawing_DownloadListUC_And_Log();
                    this.SetEnabledAllChildControls( true );                    
                }
            }
            else
            {
                #region comm. [.variant #1.]
                /*foreach ( var row in rows )
                {
                    ProcessDownloadCommand( DownloadCommandEnum.Delete, row );
                    _ExternalProgQueue.Remove( row.GetOutputFullFileNames() );
                }*/
                #endregion

                #region [.variant #2.]
                _DownloadController.CancelAll( rows );
                _DownloadListModel.RemoveAll( rows );
                _ExternalProgQueue.Remove( rows.SelectMany( row => row.GetOutputFullFileNames() ) );

                SetDownloadToolButtonsStatus( downloadListUC.GetSelectedDownloadRow() );
                #endregion
            }            
        }
        private bool AskDeleteDownloadDialog( IReadOnlyList< DownloadRow > rows, bool askOnlyOutputFileExists, bool deleteOutputFile )
        {
            switch ( rows.Count )
            {
                case 0:
                    return (false);
                case 1:
                    return (AskDeleteDownloadDialog( rows[ 0 ], askOnlyOutputFileExists, deleteOutputFile ));
                default:
                    var msg = $"Delete {rows.Count} downloads{(deleteOutputFile ? " with output file" : null)} ?";
                    var r = (this.MessageBox_ShowQuestion( msg, this.Text, MessageBoxButtons.OKCancel, MessageBoxDefaultButton.Button1 ) == DialogResult.OK);
                    return (r);
            }            
        }
        private bool AskDeleteDownloadDialog( DownloadRow row, bool askOnlyOutputFileExists, bool deleteOutputFile )
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
                var r = (this.MessageBox_ShowQuestion( msg, this.Text, MessageBoxButtons.OKCancel, MessageBoxDefaultButton.Button1 ) == DialogResult.OK);
                return (r);
            }
            return (false);
        }

        #region [.allowed Command by current status.]
        [M(O.AggressiveInlining)] private static bool StartDownload_IsAllowed ( DownloadStatus status ) => (status == DownloadStatus.Created ) ||
                                                                                                           (status == DownloadStatus.Paused  ) ||
                                                                                                           (status == DownloadStatus.Canceled) ||
                                                                                                           (status == DownloadStatus.Finished) ||
                                                                                                           (status == DownloadStatus.Error   );
        [M(O.AggressiveInlining)] private static bool CancelDownload_IsAllowed( DownloadStatus status ) => (status == DownloadStatus.Started ) ||
                                                                                                           (status == DownloadStatus.Running ) ||
                                                                                                           (status == DownloadStatus.Wait    ) ||
                                                                                                           (status == DownloadStatus.Paused  );
        [M(O.AggressiveInlining)] private static bool PauseDownload_IsAllowed ( DownloadStatus status ) => (status == DownloadStatus.Started ) ||
                                                                                                           (status == DownloadStatus.Running );
        #endregion

        private void AddNewDownloads( (string m3u8FileUrl, bool autoStartDownload)[] array )
        {
            var p = (m3u8FileUrls     : (from t in array select t.m3u8FileUrl).ToArray(), 
                     autoStartDownload: array.FirstOrDefault().autoStartDownload);
            AddNewDownloads( p );
        }
        private void AddNewDownloads( in (IReadOnlyCollection< string > m3u8FileUrls, bool autoStartDownload) p ) //, bool forceShowEmptyDialog )
        {
            if ( p.m3u8FileUrls.AnyEx() )
            {
                if ( p.m3u8FileUrls.Count == 1 )
                {
                    AddNewDownload( (p.m3u8FileUrls.First(), p.autoStartDownload) );
                }
                else
                {
                    var action = new Action< (string m3u8FileUrl, bool autoStartDownload), (int n, int total) >( 
                        ((string m3u8FileUrl, bool autoStartDownload) tp, (int n, int total) seriesInfo ) => AddNewDownload( tp, seriesInfo ) );

                    var n     = p.m3u8FileUrls.Count;
                    var count = n;
                    foreach ( var m3u8FileUrl in p.m3u8FileUrls.Reverse() )
                    {
                        var seriesInfo = (n--, count);
                        this.BeginInvoke( action, (m3u8FileUrl, p.autoStartDownload), seriesInfo );
                    }
                }
            }
            else //if ( forceShowEmptyDialog )
            {
                AddNewDownload( (null, false) );
            }
        }
        private async void AddNewDownload( (string m3u8FileUrl, bool autoStartDownload) p, (int n, int total)? seriesInfo = null )
        {
            if ( p.autoStartDownload && !p.m3u8FileUrl.IsNullOrWhiteSpace() && FileNameCleaner.TryGetOutputFileNameByUrl( p.m3u8FileUrl, out var outputFileName ) )
            {
                if ( !_SettingsController.UniqueUrlsOnly || !_DownloadListModel.ContainsUrl( p.m3u8FileUrl ) )
                {
                    var row = _DownloadListModel.AddRow( (p.m3u8FileUrl, outputFileName, _SettingsController.OutputFileDirectory/*, IsLiveStream: false, default*/) );
                    await downloadListUC.SelectDownloadRowDelay( row );
                    _DownloadController.Start( row );
                }
                return;
            }

            #region [.AddNewDownloadForm as top-always-owner-form.]
            AddNewDownloadForm.Show( this, _DownloadController, _SettingsController, p.m3u8FileUrl, seriesInfo, async f =>
            {
                if ( f.DialogResult == DialogResult.OK )
                {
                    var row = _DownloadListModel.AddRow( (f.M3u8FileUrl, f.GetOutputFileName(), f.GetOutputDirectory(), f.IsLiveStream, f.LiveStreamMaxFileSizeInBytes) );
                    await downloadListUC.SelectDownloadRowDelay( row );
                    if ( f.AutoStartDownload )
                    {
                        _DownloadController.Start( row );
                    }
                }

                if ( AddNewDownloadForm.TryGetOpenedForm( out var openedForm ) )
                {
                    openedForm.ActivateAfterCloseOther();
                }
            });
            #endregion

            #region comm. [.AddNewDownloadForm as modal-dialog.]
            /*
            AddNewDownloadForm.ShowModalDialog( this, _DownloadController, _SettingsController, p.m3u8FileUrl, seriesInfo, async f =>
            {
                var row = _DownloadListModel.AddRow( (f.M3u8FileUrl, f.GetOutputFileName(), f.GetOutputDirectory()) );
                await downloadListUC.SelectDownloadRowDelay( row );
                if ( f.AutoStartDownload )
                {
                    _DownloadController.Start( row );
                }
            });

            if ( AddNewDownloadForm.TryGetOpenedForm( out var openedForm ) )
            {
                openedForm.ActivateAfterCloseOther();
            }
            //*/

            /*
            using ( var f = new AddNewDownloadForm( _DownloadController, _SettingsController, p.m3u8FileUrl, seriesInfo ) )
            {
                if ( f.ShowDialog( this ) == DialogResult.OK )
                {
                    var row = _DownloadListModel.AddRow( (f.M3u8FileUrl, f.GetOutputFileName(), f.GetOutputDirectory()) );
                    await downloadListUC.SelectDownloadRowDelay( row );
                    if ( f.AutoStartDownload )
                    {
                        _DownloadController.Start( row );
                    }
                }
            }

            if ( AddNewDownloadForm.TryGetOpenedForm( out var openedForm ) )
            {
                openedForm.ActivateAfterCloseOther();
            }
            //*/
            #endregion
        }

        private bool IsWaitBannerShown() => this.Controls.OfType< WaitBannerUC >().Any();
        private void SuspendDrawing_DownloadListUC_And_Log()
        {
            downloadListUC.SuspendDrawing();
            logUC.SuspendDrawing();
        }
        private void ResumeDrawing_DownloadListUC_And_Log()
        {
            downloadListUC.ResumeDrawing();
            logUC.ResumeDrawing();
        }
        #endregion

        #region [.menu.]
#if DEBUG
        private void addNewDownloadToolButton_Click( object sender, EventArgs e ) => AddNewDownload( ($"http://xzxzzxzxxz.ru/{(new Random().Next())}/abc.def", false) );
#else
        private void addNewDownloadToolButton_Click( object sender, EventArgs e ) => AddNewDownload( (null, false) );
#endif
        private void showLogToolButton_Click( object sender, EventArgs e )
        {
            var showLog = showLogToolButton.Checked;
            _SettingsController.ShowLog = showLog;
            mainSplitContainer.Panel2Collapsed = !showLog; //m3u8FileResultUC.Visible = showLog;
            logUC.SetModel( (showLog ? downloadListUC.GetSelectedDownloadRow()?.Log : null) );
        }
        private void copyToolButton_Click( object sender, EventArgs e )
        {
            var rows = downloadListUC.GetSelectedDownloadRows();
            if ( rows.Any() )
            {
                Extensions.CopyM3u8FileUrlToClipboard( rows );
            }
            else
            {
                this.MessageBox_ShowError( "Nothing for copy to clipboard.", this.Text );
            }
        }
        private void pasteToolButton_Click( object sender, EventArgs e )
        {
            if ( Extensions.TryGetM3u8FileUrlsFromClipboard( out var m3u8FileUrls ) )
            {
                var autoStartDownload = ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
                if ( !autoStartDownload ) m3u8FileUrls = m3u8FileUrls.Take( 50/*100*/ ).ToArray();
                AddNewDownloads( (m3u8FileUrls, autoStartDownload) );
            }
            else
            {
                this.MessageBox_ShowError( "Nothing for paste from clipboard.", this.Text );
            }
        }
        private void columnsVisibilityEditorToolButton_Click( object sender, EventArgs e ) => statusBarUC.ShowDialog_ColumnsVisibilityEditor( downloadListUC.GetDataGridColumns(), downloadListUC.GetAlwaysVisibleDataGridColumns() );
        private void fileNameExcludesWordsEditorToolButton_Click( object sender, EventArgs e ) => statusBarUC.ShowDialog_FileNameExcludesWordsEditor();
        private void parallelismSettingsToolButton_Click( object sender, EventArgs e ) => statusBarUC.ShowDialog_ParallelismSettings();
        private void otherSettingsToolButton_Click( object sender, EventArgs e ) => statusBarUC.ShowDialog_OtherSettings();
        private void aboutToolButton_Click( object sender, EventArgs e )
        {
            var text = $"\"{AssemblyInfoHelper.AssemblyTitle}\"" + Environment.NewLine +
                       AssemblyInfoHelper.AssemblyCopyright + Environment.NewLine +
                       Environment.NewLine +
                       $"Version {AssemblyInfoHelper.AssemblyVersion}, ({AssemblyInfoHelper.AssemblyLastWriteTime})" +
                       Environment.NewLine +
                       Environment.NewLine +
                       "Shortcut's:" + Environment.NewLine +
                       "  Ctrl+C:\t Copy selected download url to clipboard" + Environment.NewLine +
                       "  Ctrl+B:\t Browse output file (if exists)" + Environment.NewLine +
                       "  Ctrl+D:\t Minimized application window" + Environment.NewLine +
                       "  Ctrl+E:\t Open with External program" + Environment.NewLine +
                       "  Ctrl+O:\t Open output file (if exists)" + Environment.NewLine +
                       "  Ctrl+P:\t Pause selected download" + Environment.NewLine +
                       "  Ctrl+S:\t Start selected download" + Environment.NewLine +
                       "  Ctrl+V:\t Paste download url from clipboard" + Environment.NewLine +
                       "  Ctrl+W:\t Exit application" + Environment.NewLine +
                       "  Ctrl+Z:\t Cancel selected download" + Environment.NewLine +
                       "  Insert:\t Open add new download dialog" + Environment.NewLine +
                       "  Delete:\t Delete download (with or without output file)" + Environment.NewLine +
                       "  Enter:\t Open rename output file dialog" + Environment.NewLine +
                       "  F1:\t About dialog" + Environment.NewLine;
            this.MessageBox_ShowInformation( text, "about" );
        }

        private void startDownloadToolButton_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Start  );
        private void pauseDownloadToolButton_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Pause  );
        private void cancelDownloadToolButton_Click( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Cancel );
        private void deleteDownloadToolButton_Click( object sender, EventArgs e ) => DeleteDownloads( downloadListUC.GetSelectedDownloadRows().ToArrayEx(), deleteOutputFiles: false );
        private void deleteAllFinishedDownloadToolButton_Click( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.DeleteAllFinished );

        private void downloadInstanceToolButton_ValueChanged( int downloadInstanceValue )
        {
            if ( _SettingsController.UseCrossDownloadInstanceParallelism )
            {
                _SettingsController.Settings.MaxCrossDownloadInstance = downloadInstanceValue;
            }
        }
        private void degreeOfParallelismToolButton_ValueChanged( int value ) => _SettingsController.Settings.MaxDegreeOfParallelism = value;
        private void speedThrottlerToolButton_ValueChanged( double? value ) => _SettingsController.Settings.MaxSpeedThresholdInMbps = value; 
        #endregion

        #region [.context menu.]
        private void downloadListUC_MouseClickRightButton( MouseEventArgs e, DownloadRow selectedRow, bool outOfGridArea )
        {
            if ( (selectedRow != null) || (0 < _DownloadListModel.RowsCount) )
            {
                startDownloadMenuItem              .Enabled = startDownloadToolButton .Enabled;
                cancelDownloadMenuItem             .Enabled = cancelDownloadToolButton.Enabled;
                pauseDownloadMenuItem              .Enabled = pauseDownloadToolButton .Enabled;
                deleteDownloadMenuItem             .Enabled = deleteDownloadToolButton.Enabled;
                deleteWithOutputFileMenuItem       .Enabled = deleteDownloadToolButton.Enabled && Extensions.AnyFileExists( selectedRow?.GetOutputFullFileNames() );
                browseOutputFileMenuItem           .Visible = deleteWithOutputFileMenuItem.Enabled;
                openOutputFileMenuItem             .Visible = deleteWithOutputFileMenuItem.Enabled;     
                deleteAllFinishedDownloadMenuItem  .Enabled = deleteAllFinishedDownloadToolButton.Enabled;

                #region [.CheckState-of-openOutputFilesWithExternalMenuItem & changeOutputDirectoryMenuItem.]
                if ( selectedRow != null )
                {
                    var rows = downloadListUC.GetSelectedDownloadRows();

                    openOutputFilesWithExternalMenuItem.Visible = true;
                    if ( _ExternalProgQueue.Any() )
                    {                        
                        var outputFileNames = (from r in rows select r.GetOutputFullFileName()).ToList();
                        if ( outputFileNames.Any( fn => _ExternalProgQueue.Contains( fn ) ) )
                        {
                            openOutputFilesWithExternalMenuItem.CheckState = outputFileNames.All( fn => _ExternalProgQueue.Contains( fn ) ) ? CheckState.Checked : CheckState.Indeterminate;
                        }
                        else
                        {
                            openOutputFilesWithExternalMenuItem.CheckState = CheckState.Unchecked;
                        }
                    }
                    else
                    {
                        openOutputFilesWithExternalMenuItem.CheckState = CheckState.Unchecked;
                    }

                    changeOutputDirectoryMenuItem.Visible = true;// (from r in rows where !r.IsFinished() select selectedRow).Any();
                }
                else
                {
                    changeOutputDirectoryMenuItem.Visible =
                        openOutputFilesWithExternalMenuItem.Visible = false;
                }
                #endregion

                var allowedAll = (selectedRow == null) || (1 < _DownloadListModel.RowsCount);
                SetAllDownloadsMenuItemsEnabled( allowedAll );

                mainContextMenu.Show( downloadListUC, e.Location );
            }
        }
        private void SetAllDownloadsMenuItemsEnabled( bool allowedAll )
        {
            if ( allowedAll )
            {
                int start = 0, cancel = 0, pause = 0, delete = 0, deleteWithFiles = 0;
                foreach ( var row in _DownloadListModel.GetRows() )
                {
                    var status = row.Status;
                    start  += StartDownload_IsAllowed ( status ) && !status.IsFinished() ? 1 : 0;
                    cancel += CancelDownload_IsAllowed( status ) ? 1 : 0;
                    pause  += PauseDownload_IsAllowed ( status ) ? 1 : 0;
                    delete++;
                    if ( Extensions.AnyFileExists( row.GetOutputFullFileNames() ) )
                    {
                        deleteWithFiles++;
                    }
                }

                static void set_enabled_and_text( ToolStripMenuItem menuItem, int count )
                {
                    menuItem.Enabled = (0 < count);
                    if ( menuItem.Tag == null )
                    {
                        menuItem.Tag = menuItem.Text;
                    }
                    menuItem.Text = menuItem.Tag?.ToString();
                    if ( 0 < count )
                    {
                        menuItem.Text += $" ({count})";
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
                startAllDownloadsMenuItem.Enabled =
                    cancelAllDownloadsMenuItem.Enabled =
                        pauseAllDownloadsMenuItem.Enabled =
                            deleteAllDownloadsMenuItem.Enabled =
                                deleteAllWithOutputFilesMenuItem.Enabled = false;
            }
        }

        private void startDownloadMenuItem_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Start  );
        private void pauseDownloadMenuItem_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Pause  );
        private void cancelDownloadMenuItem_Click( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Cancel );

        private void deleteDownloadMenuItem_Click( object sender, EventArgs e ) => deleteDownloadToolButton_Click( sender, e );
        private void deleteWithOutputFileMenuItem_Click( object sender, EventArgs e ) => DeleteDownloads( downloadListUC.GetSelectedDownloadRows().ToArrayEx(), deleteOutputFiles: true );

        private void browseOutputFileMenuItem_Click( object sender, EventArgs e )
        {
            var row = downloadListUC.GetSelectedDownloadRow();
            if ( Extensions.TryGetFirstFileExists( row?.GetOutputFullFileNames(), out var outputFileName ) )
            {
                using ( Process.Start( "explorer", "/e,/select," + outputFileName ) ) {; }
            }
        }
        private void openOutputFileMenuItem_Click( object sender, EventArgs e )
        {
            var row = downloadListUC.GetSelectedDownloadRow();
            if ( (row != null) && row.IsFinishedOrError() && Extensions.TryGetFirstFileExists( row.GetOutputFullFileNames(), out var outputFileName ) )
            {
#if NETCOREAPP
                using ( Process.Start( new ProcessStartInfo( outputFileName ) { UseShellExecute = true } ) ) {; }
#else
                using ( Process.Start( outputFileName ) ) {; }
#endif
            }
        }
        private void openOutputFilesWithExternalMenuItem_Click( object sender, EventArgs e )
        {            
            var externalProgFilePath = Settings.Default.ExternalProgFilePath;
            if ( !File.Exists( externalProgFilePath ) )
            {
                this.MessageBox_ShowError( $"External program file doesn't exists: '{externalProgFilePath}'", _APP_TITLE_ );
                return;
            }

            var rows = downloadListUC.GetSelectedDownloadRows();
            var outputFileNames = (from row in rows
                                   where row.IsFinishedOrError()
                                   let t = Extensions.TryGetFirstFileExists( row.GetOutputFullFileNames() )
                                   where t.success
                                   select t.outputFileName
                                  )
                                  .ToArray();
            if ( outputFileNames.Any() )
            {
                var buf = new StringBuilder( 0x100 * outputFileNames.Length );
                foreach ( var fn in outputFileNames )
                {
                    buf.Append( '"' ).Append( fn ).Append( '"' ).Append( ' ' );
                }
                var args = buf.ToString( 0, buf.Length - 1 );

                ExternalProg_Run( externalProgFilePath, args );
            }

            var outputFileNamesQueue = (from row in rows
                                        where !row.IsFinishedOrError()
                                        select row.GetOutputFullFileName()
                                       ).ToList();
            if ( outputFileNamesQueue.Any() )
            {
                var cst = openOutputFilesWithExternalMenuItem.CheckState;
                if ( cst == CheckState.Unchecked )
                {
                    _ExternalProgQueue.Add( outputFileNamesQueue );
                }
                else
                {
                    _ExternalProgQueue.Remove( outputFileNamesQueue );
                }
                downloadListUC.Invalidate( true );
            }
        }
        /*private void openOutputFilesWithExternalMenuItem_Click__PREV( object sender, EventArgs e )
        {            
            var externalProgFilePath = Settings.Default.ExternalProgFilePath;
            if ( !File.Exists( externalProgFilePath ) )
            {
                this.MessageBox_ShowError( $"External program file doesn't exists: '{externalProgFilePath}'", _APP_TITLE_ );
                return;
            }

            var rows = downloadListUC.GetSelectedDownloadRows();
            var outputFileNames = (from row in rows
                                   where row.IsFinished()
                                   let t = Extensions.TryGetFirstFileExists( row.GetOutputFullFileNames() )
                                   where t.success
                                   select t.outputFileName
                                  )
                                  .ToArray();
            if ( outputFileNames.Any() )
            {
                var buf = new StringBuilder( 0x100 * outputFileNames.Length );
                foreach ( var fn in outputFileNames )
                {
                    buf.Append( '"' ).Append( fn ).Append( '"' ).Append( ' ' );
                }
                var args = buf.ToString( 0, buf.Length - 1 );

                ExternalProg_Run( externalProgFilePath, args );
            }

            //var cst = openOutputFilesWithExternalMenuItem.CheckState;

            var outputFileNamesQueue = (from row in rows
                                        where !row.IsFinished()
                                        select row.GetOutputFullFileName()
                                       ).ToArray();
            if ( outputFileNamesQueue.All( fn => _ExternalProgQueue.Contains( fn ) ) )
            {
                _ExternalProgQueue.Remove( outputFileNamesQueue );
            }
            else
            {
                _ExternalProgQueue.Add( outputFileNamesQueue );
            }
        }
        //*/
        private static void ExternalProg_Run( string externalProgFilePath, string args )
        {
            using ( Process.Start( externalProgFilePath, args ) ) {; }
        }
        private static void ExternalProg_Run_IfExists( string externalProgFilePath, string args )
        {
            if ( File.Exists( externalProgFilePath ) )
            {
                ExternalProg_Run( externalProgFilePath, args );
            }
        }

        private void startAllDownloadsMenuItem_Click( object sender, EventArgs e )
        {
            foreach ( var row in _DownloadListModel.GetRows() )
            {
                var status = row.Status;
                if ( StartDownload_IsAllowed( status ) && !status.IsFinished() )
                {
                    _DownloadController.Start( row );
                }
            }
        }
        private void pauseAllDownloadsMenuItem_Click( object sender, EventArgs e )
        {
            foreach ( var row in _DownloadListModel.GetRows() )
            {
                if ( PauseDownload_IsAllowed( row.Status ) )
                {
                    _DownloadController.Pause( row );
                }
            }
        }
        private void cancelAllDownloadsMenuItem_Click( object sender, EventArgs e )
        {
            foreach ( var row in _DownloadListModel.GetRows() )
            {
                if ( CancelDownload_IsAllowed( row.Status ) )
                {
                    _DownloadController.Cancel( row );
                }
            }
        }

        private void deleteAllDownloadsMenuItem_Click( object sender, EventArgs e ) => DeleteDownloads( _DownloadListModel.GetRows().ToArrayEx(), deleteOutputFiles: false );
        private void deleteAllWithOutputFilesMenuItem_Click( object sender, EventArgs e ) => DeleteDownloads( _DownloadListModel.GetRows().ToArrayEx(), deleteOutputFiles: true );
        #endregion

        #region [.change OutputFileName & OutputDirectory.]
        private void downloadListUC_OutputFileNameClick( DownloadRow row )
        {
            if ( ChangeOutputFileForm.TryChangeOutputFile( this, row, out var outputFileName ) )
            {
                ChangeOutputFileName( row, outputFileName );
                downloadListUC.Invalidate( true );
            }
        }

        private string _Last_ChangeOutputDirectory;
        private void changeOutputDirectoryMenuItem_Click( object sender, EventArgs e )
        {
            var rows = downloadListUC.GetSelectedDownloadRows();
            if ( rows.Any() )
            {
                var first_row = rows.First();
                var descr = (rows.Count == 1) ? $"Select output directory for file: '{first_row.OutputFileName}'" : $"Select output directory for {rows.Count} files";
                if ( DirectorySelectDialog.Show( this, GetSelectedDirectory( first_row ), descr, out var outputDirectory ) )
                {
                    _Last_ChangeOutputDirectory = outputDirectory;
                    foreach ( var row in rows )
                    {
                        ChangeOutputDirectory( row, outputDirectory );
                    }
                    downloadListUC.Invalidate( true );
                }
            }
        }
        private void downloadListUC_OutputDirectoryClick( DownloadRow row )
        {
            if ( DirectorySelectDialog.Show( this, GetSelectedDirectory( row ), $"Select output directory for file: '{row.OutputFileName}'", out var outputDirectory ) )
            {
                _Last_ChangeOutputDirectory = outputDirectory;
                ChangeOutputDirectory( row, outputDirectory );
                downloadListUC.Invalidate( true );
            }
        }
        private string GetSelectedDirectory( DownloadRow row ) => Extensions.GetFirstExistsDirectory( _Last_ChangeOutputDirectory ) ?? row.OutputDirectory;

        private void ChangeOutputFileName( DownloadRow row, string outputFileName ) => ChangeOutputFileName_Or_OutputDirectory( row, outputFileName, change_outputDirectory: false );
        private void ChangeOutputDirectory( DownloadRow row, string outputDirectory ) => ChangeOutputFileName_Or_OutputDirectory( row, outputDirectory, change_outputDirectory: true );
        private void ChangeOutputFileName_Or_OutputDirectory( DownloadRow row, string outputFileName_or_outputDirectory, bool change_outputDirectory )
        {
            var prev_outputFullFileName = row.GetOutputFullFileName();
            var need_add = _ExternalProgQueue.Remove( prev_outputFullFileName );

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

            if ( need_add ) _ExternalProgQueue.Add( new_outputFullFileName );

            var res = MoveFileByRename( row, prev_outputFullFileName, new_outputFullFileName );
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
                    if ( need_add )
                    {
                        _ExternalProgQueue.Remove( new_outputFullFileName );
                        _ExternalProgQueue.Add( prev_outputFullFileName );
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private enum MoveFileByRenameModeEnum { OverwriteAsk, OverwriteSilent, SkipIfAlreadyExists }
        private enum MoveFileByRenameResultEnum { Postponed, Canceled, Suc, Fail }
        private MoveFileByRenameResultEnum MoveFileByRename( DownloadRow row, string prev_outputFullFileName, string new_outputFullFileName
            , MoveFileByRenameModeEnum mode = MoveFileByRenameModeEnum.OverwriteAsk )
        {
            if ( !row.Status.IsRunningOrPaused() && File.Exists( prev_outputFullFileName ) )
            {
                switch ( mode )
                {
                    case MoveFileByRenameModeEnum.OverwriteSilent: 
                        Extensions.DeleteFile_NoThrow( new_outputFullFileName ); 
                        break;
                    case MoveFileByRenameModeEnum.OverwriteAsk: 
                        if ( File.Exists( new_outputFullFileName ) )
                        {
                            if ( this.MessageBox_ShowQuestion( $"File '{new_outputFullFileName}' already exists. Overwrite ?", "Overwrite exists file" ) != DialogResult.Yes )
                            {
                                return (MoveFileByRenameResultEnum.Canceled);
                            }
                            Extensions.DeleteFile_NoThrow( new_outputFullFileName );
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
                    this.MessageBox_ShowError( ex.ToString(), "Move/Remane output file" );
                    return  (MoveFileByRenameResultEnum.Fail);
                }
            }
            return (MoveFileByRenameResultEnum.Postponed);
        }
        #endregion

        #region [.LiveStream change max-file-size.]
        private void downloadListUC_LiveStreamMaxFileSizeClick( DownloadRow row )
        {
            if ( ChangeLiveStreamMaxFileSizeForm.Show( this, row.LiveStreamMaxFileSizeInBytes, out var liveStreamMaxFileSizeInBytes ) )
            {
                row.SetLiveStreamMaxFileSizeInBytes( liveStreamMaxFileSizeInBytes );
                downloadListUC.Invalidate( true );
            }
        }
        #endregion
    }
}
