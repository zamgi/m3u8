using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Taskbar;

using m3u8.download.manager.infrastructure;
using m3u8.download.manager.ipc;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using m3u8.download.manager.ui.infrastructure;

using _CollectionChangedTypeEnum_            = m3u8.download.manager.models.DownloadListModel.CollectionChangedTypeEnum;
using _DC_                                   = m3u8.download.manager.controllers.DownloadController;
using _ReceivedInputParamsArrayEventHandler_ = m3u8.download.manager.ipc.PipeIPC.NamedPipeServer__Input.ReceivedInputParamsArrayEventHandler;
using _ReceivedSend2FirstCopyEventHandler_   = m3u8.download.manager.ipc.PipeIPC.NamedPipeServer__Input.ReceivedSend2FirstCopyEventHandler;
using _SC_                                   = m3u8.download.manager.controllers.SettingsPropertyChangeController;
using _SummaryDownloadInfo_                  = m3u8.download.manager.ui.DownloadListUC.SummaryDownloadInfo;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class MainForm : Form, IDisposable
    {
        #region [.fields.]
        private const int MAX_PASTE_URLS = 75; //100;
        private UrlInputParams[] _InputParamsArray;
        private _ReceivedInputParamsArrayEventHandler_ _ReceivedInputParamsArrayEventHandler;
        private _ReceivedSend2FirstCopyEventHandler_   _ReceivedSend2FirstCopyEventHandler;        

        private DownloadListModel              _DownloadListModel;
        private UndoModel                      _UndoModel;
        private _DC_                           _DC;
        private _SC_                           _SC;
        private LogRowsHeightStorer            _LogRowsHeightStorer;
        private Action< DownloadRow, string >  _DownloadListModel_RowPropertiesChangedAction;
        private Action< _CollectionChangedTypeEnum_, DownloadRow > _DownloadListModel_CollectionChangedAction;
        private bool                           _ShowDownloadStatistics;
        private HashSet< string >              _ExternalProgQueue;
        private NotifyIcon                     _NotifyIcon;
        private OutputFileNamePatternProcessor _OutputFileNamePatternProcessor;
#if NETCOREAPP
        private static string _APP_TITLE_ => Resources.APP_TITLE__NET_CORE
        #if DEBUG
                + " / (DEBUG)"
        #endif
                ;
#else
        private static string _APP_TITLE_ => Resources.APP_TITLE__NET_FW
        #if DEBUG
                + " / (DEBUG)"
        #endif
                ;
#endif
        #endregion

        #region [.ctor().]
        private MainForm()
        {
            _SC = new _SC_( Settings.Default );

            _DownloadListModel = new DownloadListModel();
            _DownloadListModel.CollectionChanged    += DownloadListModel_CollectionChanged;
            _DownloadListModel.RowPropertiesChanged += DownloadListModel_RowPropertiesChanged;
            _DC = new _DC_( _DownloadListModel, _SC );

            _UndoModel = new UndoModel( _DownloadListModel );
            _UndoModel.UndoChanged += () => { undoToolButton.Enabled = _UndoModel.HasUndo; undoToolButton.ToolTipText = $"Undo step count: {_UndoModel.UndoCount}  (Ctrl + Z)"; };
            //----------------------------------------//

            InitializeComponent( _DC, _SC );
            this.Text = _APP_TITLE_;
            mainSplitContainer.SetCursor( Cursors.SizeNS );
            //----------------------------------------//

            _DownloadListModel_RowPropertiesChangedAction = new Action< DownloadRow, string >( DownloadListModel_RowPropertiesChanged );
            _DownloadListModel_CollectionChangedAction    = new Action< _CollectionChangedTypeEnum_, DownloadRow >( DownloadListModel_CollectionChanged );
            
            _LogRowsHeightStorer = new LogRowsHeightStorer();

            _SC.SettingsPropertyChanged += SettingsController_PropertyChanged;
            SettingsController_PropertyChanged( _SC.Settings, nameof(Settings.ShowDownloadStatisticsInMainFormTitle) );
            SettingsController_PropertyChanged( _SC.Settings, nameof(Settings.ExternalProgCaption) );
            SettingsController_PropertyChanged( _SC.Settings, nameof(Settings.ShowAllDownloadsCompleted_Notification) );

            //logUC.SetSettingsController( _SC );
            logUC.SetLogRowsHeightStorer( _LogRowsHeightStorer );

            downloadListUC.SetModel_And_SettingsController( _DownloadListModel, _SC );
            downloadListUC.KeyDown += (s, e) => this.OnKeyDown( e );
            downloadListUC.MouseClickRightButton      += downloadListUC_MouseClickRightButton;
            downloadListUC.UpdatedSingleRunningRow    += downloadListUC_UpdatedSingleRunningRow;
            downloadListUC.UpdatedSummaryDownloadInfo += downloadListUC_UpdatedSummaryDownloadInfo;
            downloadListUC.DoubleClickEx              += openOutputFileMenuItem_Click;
            //statusBarUC.SetDownloadController( _DC );
            //statusBarUC.SetSettingsController( _SC );
            statusBarUC.TrackItemsCount( downloadListUC );
            statusBarUC.SettingsChanged += statusBarUC_SettingsChanged;

            _ReceivedInputParamsArrayEventHandler = AddNewDownloads;
            _ReceivedSend2FirstCopyEventHandler   = ReceivedSend2FirstCopy;
            PipeIPC.NamedPipeServer__Input.ReceivedInputParamsArray += NamedPipeServer__Input_ReceivedInputParamsArray;
            PipeIPC.NamedPipeServer__Input.ReceivedSend2FirstCopy   += NamedPipeServer__Input_ReceivedSend2FirstCopy;

            NameCleaner.ResetExcludesWords( _SC.NameCleanerExcludesWords );

            showLogToolButton.Checked = _SC.ShowLog;
            showLogToolButton_Click( showLogToolButton, EventArgs.Empty );

            downloadInstanceToolButton.SetValueAndVisible( _SC.MaxCrossDownloadInstance );
            degreeOfParallelismToolButton.ValueWithSaved = (_SC.MaxDegreeOfParallelism , _SC.MaxDegreeOfParallelismSaved);
            speedThresholdToolButton     .ValueWithSaved = (_SC.MaxSpeedThresholdInMbps, _SC.MaxSpeedThresholdInMbpsSaved);

            _ExternalProgQueue = new HashSet< string >( StringComparer.InvariantCultureIgnoreCase );
            _OutputFileNamePatternProcessor = new OutputFileNamePatternProcessor();
        }
        public MainForm( in UrlInputParams[] array ) : this() => _InputParamsArray = array;

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();

                _DC.Dispose();
                _SC.Dispose();
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
                FormPositionStorer.Load( this, _SC.MainFormPositionJson );
                _DownloadListModel.AddRows( _SC.GetDownloadRows() /*DownloadRowsSerializer.FromJSON( _SettingsController.DownloadRowsJson )*/ );
                logUC.ShowOnlyRequestRowsWithErrors = _SC.Settings.ShowOnlyRequestRowsWithErrors;
                logUC.ScrollToLastRow               = _SC.Settings.ScrollToLastRow;
            }
#if DEBUG
            if ( _DownloadListModel.RowsCount == 0 )
            {
                var dir = _SC.Settings.OutputFileDirectory;
                _DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8"   , null, "xz-1", dir) );
                _DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8-12", null, "xz-2", dir) );
                _DownloadListModel.AddRow( ("http://s12.seplay.net/content/stream/films/the.resident.s03e16.720p.octopus_173547/hls/720/index.m3u8-34", null, "xz-3", dir) );
            }
#endif
        }
        protected override void OnFormClosed( FormClosedEventArgs e )
        {
            base.OnFormClosed( e );

            if ( !base.DesignMode )
            {
                _SC.MainFormPositionJson = FormPositionStorer.Save( this );
                _SC.SetDownloadRows( _DownloadListModel.GetRows_Enumerable() );
                _SC.Settings.ShowOnlyRequestRowsWithErrors = logUC.ShowOnlyRequestRowsWithErrors;
                _SC.Settings.ScrollToLastRow               = logUC.ScrollToLastRow;
                _SC.SaveNoThrow_IfAnyChanged();
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
                       ClipboardHelper.TryGetM3u8FileUrlsFromClipboard( out var m3u8FileUrls, _SC.IgnoreHostHttpHeader ) 
                    )
            {
                m3u8FileUrls = m3u8FileUrls.Take( MAX_PASTE_URLS ).ToArray();
                AddNewDownloads( (m3u8FileUrls, false) );
            }
        }
        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            base.OnFormClosing( e );

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
            if ( _DC.IsDownloading )
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
                        _DC.CancelAll();
                        Application.DoEvents();

                        if ( !_DC.IsDownloading || (WAIT_Milliseconds <= sw.ElapsedMilliseconds) )
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
                        if ( ClipboardHelper.TryGetHttpUrlsFromClipboard( out var urls, _SC.IgnoreHostHttpHeader ) )
                        {                            
                            e.SuppressKeyPress = true;

                            var autoStartDownload = e.Shift;
                            if ( !autoStartDownload ) urls = urls.Take( MAX_PASTE_URLS ).ToList( MAX_PASTE_URLS );
                            AddNewDownloads( (urls, autoStartDownload) );
                            return;
                        }
                        else
                        {
                            this.MessageBox_ShowError( "Nothing for Paste from clipboard.", this.Text );
                        }
                        break;

                    case Keys.C: //Copy
                        if ( downloadListUC.HasFocus )
                        {
                            var rows = downloadListUC.GetSelectedDownloadRows();
                            if ( rows.Any() )
                            {
                                e.SuppressKeyPress = true;
                                ClipboardHelper.CopyUrlsToClipboard( rows );
                                return;
                            }
                            else
                            {
                                this.MessageBox_ShowError( "Nothing for Copy to clipboard.", this.Text );
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

                    case Keys.X/*Z*/: //Cancel download
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
                    case Keys.T: //Open output file with External progs
                        if ( downloadListUC.HasFocus )
                        {
                            openOutputFilesWithExternalMenuItem_Click( this, EventArgs.Empty );
                        }
                        break;

                    case Keys.G:
                        if ( e.Shift ) //Collect Garbage
                        {
                            Collect_Garbage();
                        }
                        break;

                    case Keys.Delete: // [Ctrl + Shift + Del]
                        if ( e.Shift && downloadListUC.HasFocus )
                        {
                            OnlyDeleteOutputFiles( downloadListUC.GetSelectedDownloadRows().ToArrayEx() );
                        }
                        break;

                    case Keys.E: // [Ctrl + E]
                        if ( downloadListUC.HasFocus )
                        {
                            EditDownload( downloadListUC.GetSelectedDownloadRow() );
                        }
                        break;

                    case Keys.Z: // UNDO
                        if ( _UndoModel.TryUndo( out var row ) )
                        {
                            _DownloadListModel.AddRowIf( row );
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
                        var m3u8FileUrls = ClipboardHelper.TryGetM3u8FileUrlsFromClipboardOrDefault( _SC.IgnoreHostHttpHeader );
#if DEBUG
                        if ( !m3u8FileUrls.AnyEx() ) m3u8FileUrls = [ ($"http://xzxzzxzxxz.ru/{(new Random().Next())}/abc.def", null) ];
#endif
                        AddNewDownloads( (m3u8FileUrls, false) );
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

                    case Keys.F11:
                        this.WindowState = (this.WindowState == FormWindowState.Normal) ? FormWindowState.Maximized : FormWindowState.Normal;
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

        #region [.event handlers methods.]
        private void NamedPipeServer__Input_ReceivedInputParamsArray( UrlInputParams[] array ) => this.BeginInvoke( _ReceivedInputParamsArrayEventHandler, array );
        private void NamedPipeServer__Input_ReceivedSend2FirstCopy() => this.BeginInvoke( _ReceivedSend2FirstCopyEventHandler );
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
            var is_need_save = false;
            switch ( propertyName )
            {
                case nameof(Settings.ShowDownloadStatisticsInMainFormTitle):
                    _ShowDownloadStatistics = settings.ShowDownloadStatisticsInMainFormTitle;
                    ShowDownloadStatisticsInTitle();
                    break;

                case nameof(Settings.MaxCrossDownloadInstance): // nameof(Settings.ShareMaxDownloadThreadsBetweenAllDownloadsInstance):
                    downloadInstanceToolButton.SetValueAndVisible( settings.MaxCrossDownloadInstance );
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

                case nameof(Settings.ExternalProgCaption):
                    openOutputFilesWithExternalMenuItem.Text = $"    Open with '{settings.ExternalProgCaption}'";
                    break;

                case nameof(Settings.ShowAllDownloadsCompleted_Notification):
                    //*
                    _DC.IsDownloadingChanged -= DownloadController_IsDownloadingChanged;
                    if ( settings.ShowAllDownloadsCompleted_Notification )
                    {
                        _DC.IsDownloadingChanged += DownloadController_IsDownloadingChanged;
                    }
                    //*/
                    break;
            }

            if ( is_need_save )
            {
                _SC.SaveNoThrow_IfAnyChanged();
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

                var finishedText  = ((0 < finishedCount) ? $", end: {finishedCount}" : null);
                var errorText     = ((0 < errorCount   ) ? $", err: {errorCount}"    : null);

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
            if ( changedType == _CollectionChangedTypeEnum_.Sort ) return;

            if ( this.InvokeRequired )
            {
                this.BeginInvoke( _DownloadListModel_CollectionChangedAction, changedType, row );
                return;
            }

            ShowDownloadStatisticsInTitle();

            var existsRows = default(IReadOnlyList< DownloadRow >);
            switch ( changedType )
            {                
                case _CollectionChangedTypeEnum_.Remove:
                    if ( row != null )
                    {
                        goto case _CollectionChangedTypeEnum_.Remove_Bulk;
                        //_LogRowsHeightStorer.Remove( row.Log );
                        //foreach ( var offn in row.GetOutputFullFileNames() )
                        //{
                        //    if ( !_DownloadListModel.GetRows().Any( r => r.GetOutputFullFileNames().Contains( offn, StringComparer.InvariantCultureIgnoreCase ) ) )
                        //    {
                        //        _ExternalProgQueue.Remove( offn );
                        //    }
                        //}
                    }
                    break;

                //case _CollectionChangedTypeEnum_.BulkUpdate:
                case _CollectionChangedTypeEnum_.Remove_Bulk:
                    existsRows = _DownloadListModel.GetRows_ArrayCopy();

                    var existsLogs = existsRows.Select( r => r.Log );
                    _LogRowsHeightStorer.RemoveAllExcept( existsLogs );

                    var existsOutputFullFileNames = existsRows.SelectMany( r => r.GetOutputFullFileNames() );
                    _ExternalProgQueue.RemoveAllExcept( existsOutputFullFileNames );
                    break;

                case _CollectionChangedTypeEnum_.Clear:
                    _LogRowsHeightStorer.Clear();
                    _ExternalProgQueue  .Clear();
                    break;

                case _CollectionChangedTypeEnum_.Add:
                    if ( _SC.Settings.ExternalProgApplyByDefault )
                    {
                        _ExternalProgQueue.AddIf( row?.GetOutputFullFileName() /*(from _row in _DownloadListModel.GetRows() select _row.GetOutputFullFileName())*/ );
                    }
                    break;
            }

            _SC.SetDownloadRows_WithSaveIfChanged( existsRows ?? _DownloadListModel.GetRows_ArrayCopy() );
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
                    if ( row.IsFinished()/*.IsFinishedOrError()*/ && FileHelper.TryGetFirstFileExists/*NonZeroLength*/( row.GetOutputFullFileNames(), out var outputFileName ) && _ExternalProgQueue.Contains( outputFileName ) )
                    {
                        _ExternalProgQueue.Remove( row.GetOutputFullFileNames() );

                        const long MIN_NON_ZERO_FILE_LENGTH_IN_BYTES = 1_024 * 100; //100KB
                        if ( MIN_NON_ZERO_FILE_LENGTH_IN_BYTES <= new FileInfo( outputFileName ).Length ) //NonZeroLength
                        {
                            ExternalProg_Run_IfExists( _SC.Settings.ExternalProgFilePath, $"\"{outputFileName}\"" );
                        }
                    }
                    #endregion
                }
                break;
            }
        }

        private void _NotifyIcon_BalloonTipClosed( object sender, EventArgs e ) => _NotifyIcon.Visible = false;
        private void DownloadController_IsDownloadingChanged( bool isDownloading )
        {
            bool any_running() => _DownloadListModel.GetRows_Enumerable().Any( row => row.Status switch { DownloadStatus.Started => true, DownloadStatus.Running => true, DownloadStatus.Wait => true, _ => false } );

            if ( !isDownloading && !any_running() )
            {
                if ( _SC.Settings.ShowAllDownloadsCompleted_Notification )
                {
                    if ( _NotifyIcon == null )
                    {
                        _NotifyIcon = new NotifyIcon() { Visible = true, Icon = Resources.m3u8_32x36, Text = _APP_TITLE_ };

                        _NotifyIcon.BalloonTipClicked += _NotifyIcon_BalloonTipClosed;
                        _NotifyIcon.BalloonTipShown += (_, _) => { _NotifyIcon.BalloonTipClosed -= _NotifyIcon_BalloonTipClosed; _NotifyIcon.BalloonTipClosed += _NotifyIcon_BalloonTipClosed; };
                    }
                    else
                    {
                        _NotifyIcon.Visible = true;
                    }
                    _NotifyIcon.BalloonTipClosed -= _NotifyIcon_BalloonTipClosed;
                    _NotifyIcon.ShowBalloonTip( 2_500, _APP_TITLE_, Resources.ALL_DOWNLOADS_COMPLETED_NOTIFICATION, ToolTipIcon.Info );
                }
            }
            else if ( _NotifyIcon != null ) 
            {
                _NotifyIcon.Visible = false;
            }

            /*
            if ( isDownloading || any_running() ) 
            {
                if ( _NotifyIcon != null ) _NotifyIcon.Visible = false;
            }
            else if ( _SC.Settings.ShowAllDownloadsCompleted_Notification )
            {
                if ( _NotifyIcon == null )
                {
                    _NotifyIcon = new NotifyIcon() { Visible = true, Icon = Resources.m3u8_32x36, Text = _APP_TITLE_ };

                    _NotifyIcon.BalloonTipClicked += _NotifyIcon_BalloonTipClosed;
                    _NotifyIcon.BalloonTipShown += (_, _) => { _NotifyIcon.BalloonTipClosed -= _NotifyIcon_BalloonTipClosed; _NotifyIcon.BalloonTipClosed += _NotifyIcon_BalloonTipClosed; };
                }
                else
                {
                    _NotifyIcon.Visible = true;
                }
                _NotifyIcon.BalloonTipClosed -= _NotifyIcon_BalloonTipClosed;
                _NotifyIcon.ShowBalloonTip( 2_500, _APP_TITLE_, Resources.ALL_DOWNLOADS_COMPLETED_NOTIFICATION, ToolTipIcon.Info );
            }
            //*/
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
        private _SummaryDownloadInfo_ _Last_SummaryDownloadInfo;
        private void downloadListUC_UpdatedSummaryDownloadInfo( in _SummaryDownloadInfo_ sdi )
        {
            if ( _Last_SummaryDownloadInfo == sdi ) return;
            _Last_SummaryDownloadInfo = sdi;

            var taskbarProgressBarState = TaskbarProgressBarState.NoProgress;
                 if ( sdi.State.HasFlag( _SummaryDownloadInfo_.StateEmun.HasError    ) ) taskbarProgressBarState = TaskbarProgressBarState.Error;
            else if ( sdi.State.HasFlag( _SummaryDownloadInfo_.StateEmun.Paused      ) ) taskbarProgressBarState = TaskbarProgressBarState.Paused;
            else if ( sdi.State.HasFlag( _SummaryDownloadInfo_.StateEmun.Downloading ) ) taskbarProgressBarState = TaskbarProgressBarState.Normal;

            TaskbarManager.Instance.SetProgressValue( sdi.TotalProgressValue, 100, this.Handle );
            TaskbarManager.Instance.SetProgressState( taskbarProgressBarState, this.Handle );
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

        private void statusBarUC_SettingsChanged( object sender, EventArgs e )
        {
            //"apply columns visibility" & possible other settings
            _SC.MainFormPositionJson = FormPositionStorer.Save( this );
            _SC.SaveNoThrow_IfAnyChanged();
        }
        #endregion

        #region [.private methods.]
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
                        _DC.Start( row );
                        break;

                    case DownloadCommandEnum.Pause:
                        _DC.Pause( row );
                        break;

                    case DownloadCommandEnum.Cancel:
                        _DC.Cancel( row );
                        break;

                    case DownloadCommandEnum.Delete:
                        _DC.Cancel   ( row );
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
                        await _DC.DeleteRowsWithOutputFiles_Parallel_UseSynchronizationContext( rows, cts.Token,
                            (row, ct) => FileHelper.TryDeleteFiles( row.GetOutputFullFileNames(), ct ),
                            (row) =>
                            {
                                _DownloadListModel.RemoveRow( row );
                                //---_ExternalProgQueue.Remove( row.GetOutputFullFileNames() );
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
                                        //---_ExternalProgQueue.Remove( row.GetOutputFullFileNames() );
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
                        //        //---_ExternalProgQueue.Remove( row.GetOutputFullFileNames() );
                        //    }
                        //);

                        /*foreach ( var row in rows )
                        {
                            using ( var statusTran = _DownloadController.CancelIfInProgress_WithTransaction( row ) )
                            {
                                await FileDeleter.TryDeleteFiles_Async( row.GetOutputFullFileNames(), cts.Token );
                                statusTran.CommitStatus();

                                _DownloadListModel.RemoveRow( row );
                                //---_ExternalProgQueue.Remove( row.GetOutputFullFileNames() );
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
                    //---_ExternalProgQueue.Remove( row.GetOutputFullFileNames() );
                }*/
                #endregion

                #region [.variant #2.]
                _DC.CancelAll( rows );
                _DownloadListModel.RemoveRows( rows );
                //---_ExternalProgQueue.Remove( rows.SelectMany( row => row.GetOutputFullFileNames() ) );

                SetDownloadToolButtonsStatus( downloadListUC.GetSelectedDownloadRow() );
                #endregion
            }
        }
        private async void OnlyDeleteOutputFiles( DownloadRow[] rows, bool ask = true, Action< DownloadRow > sucDelPostProcessingFunc = null )
        {
            if ( !rows.AnyEx() ) return;

            var dict = new Dictionary< string, DownloadRow >( rows.Length );

            void call_sucDelPostProcessingFunc( IEnumerable< string > fns )
            {
                foreach ( var fn in fns )
                {
                    if ( dict.TryGetValue( fn, out var r ) ) sucDelPostProcessingFunc( r );
                }
            };

            var fns  = rows.SelectMany( r => 
                            {
                                var fns = r.GetOutputFullFileNames();
                                foreach ( var fn in fns )
                                {
                                    dict[ fn ] = r;
                                }
                                return (fns);
                            })
                            .ToList( rows.Length );
            var exists_fns = fns.Where( File.Exists ).ToList( fns.Count );
            if ( exists_fns.Count == 0 )
            {
                if ( sucDelPostProcessingFunc != null ) call_sucDelPostProcessingFunc( fns );
                return;
            }
            else if ( sucDelPostProcessingFunc != null )
            {
                var non_exists_fns = fns.Except( exists_fns ).ToList( fns.Count );
                if ( non_exists_fns.Count != 0 )
                {
                    call_sucDelPostProcessingFunc( non_exists_fns );
                }
            }

            if ( ask ) 
            {
                var msg = $"Only delete ({exists_fns.Count}) output files ? \r\n\r\n{string.Join( "\r\n", exists_fns )}";
                var yes = (this.MessageBox_ShowQuestion( msg, this.Text, MessageBoxButtons.YesNoCancel/*OKCancel*/, MessageBoxDefaultButton.Button1 ) == DialogResult.Yes/*OK*/);
                if ( !yes ) return;
            }

            this.SetEnabledAllChildControls( false );
            try
            {
                using ( var cts = new CancellationTokenSource() )
                using ( var wb  = WaitBannerUC.Create( this, cts, "only delete files...", visibleDelayInMilliseconds: 2_000 ) )
                {
                    wb.SetTotalSteps( exists_fns.Count );
                    await FileHelper.DeleteFiles_UseSynchronizationContext( exists_fns, cts.Token, async (fn, ct, syncCtx) => 
                    {
                        var suc = await FileHelper.TryDeleteFile( fn, ct, fullFileName => syncCtx.Invoke(() => wb.SetCaptionText( Ellipsis.MinimizePath( fullFileName, 30 ) + ", " ) ) );
                        if ( suc && (sucDelPostProcessingFunc != null) )
                        {
                            syncCtx.Invoke(() => { wb.IncreaseSteps(); if ( dict.TryGetValue( fn, out var r ) ) sucDelPostProcessingFunc( r ); } );
                        }
                        else
                        {
                            syncCtx.Invoke(() => wb.IncreaseSteps());
                        }                        
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
                this.SetEnabledAllChildControls( true );                    
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
                    var msg = $"Delete ({rows.Count}) downloads{(deleteOutputFile ? " with output file" : null)} ?";
                    var yes = (this.MessageBox_ShowQuestion( msg, this.Text, MessageBoxButtons.YesNoCancel, MessageBoxDefaultButton.Button1 ) == DialogResult.Yes);
                    return (yes);
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
                var yes = (this.MessageBox_ShowQuestion( msg, this.Text, MessageBoxButtons.YesNoCancel, MessageBoxDefaultButton.Button1 ) == DialogResult.Yes);
                return (yes);
            }
            return (false);
        }

        private void AddNewDownloads( UrlInputParams[] array )
        {
#pragma warning disable IDE0037 // Use inferred member name
            var p = (m3u8FileUrls     : (from t in array where (!t.isGroupByAudioVideo) select (t.m3u8FileUrl, t.requestHeaders)).ToList( array.Length ), 
                     autoStartDownload: array.Where( t => !t.isGroupByAudioVideo ).FirstOrDefault().autoStartDownload);
#pragma warning restore IDE0037 // Use inferred member name
            if ( p.m3u8FileUrls.Any() )
            {
                AddNewDownloads( p );
            }

            var isGroupByAudioVideoArray = array.Where( t => t.isGroupByAudioVideo ).ToList();
            if ( isGroupByAudioVideoArray.Any() )
            {
                AddNewDownload_4_GroupedByAudioVideo( isGroupByAudioVideoArray );
            } 
            else if ( !p.m3u8FileUrls.Any() )
            {
                AddNewDownload( default );
            }
        }
        private void AddNewDownloads( in (IReadOnlyCollection< (string url, string requestHeaders) > m3u8FileUrls, bool autoStartDownload) p ) //, bool forceShowEmptyDialog )
        {
            if ( p.m3u8FileUrls.AnyEx_() )
            {
                if ( p.m3u8FileUrls.Count == 1 )
                {
                    var frt = p.m3u8FileUrls.First();
                    AddNewDownload( UrlInputParams.Create( frt.url, frt.requestHeaders, p.autoStartDownload ) );
                }
                else
                {
                    var action = new Action< UrlInputParams, (int n, int total) >( (UrlInputParams x, (int n, int total) seriesInfo) => AddNewDownload( x, seriesInfo ) );

                    var count = p.m3u8FileUrls.Count;
                    var n     = count;
                    foreach ( var t in p.m3u8FileUrls.Reverse() )
                    {
                        var seriesInfo = (n--, count);
                        this.BeginInvoke( action, UrlInputParams.Create( t.url, t.requestHeaders, p.autoStartDownload ), seriesInfo );
                    }
                }
            }
            else //if ( forceShowEmptyDialog )
            {
                AddNewDownload( default/*(null, null, false)*/ );
            }
        }
        private async void AddNewDownload( UrlInputParams x, (int n, int total)? seriesInfo = null )
        {
            var suc = BrowserIPC.ExtensionRequestHeader.Try2Dict( x.requestHeaders, out var requestHeaders, ignoreHostHeader: _SC.IgnoreHostHttpHeader /*|| x.autoStartDownload*/ );
            Debug.Assert( suc || x.requestHeaders.IsNullOrEmpty() );

            if ( x.autoStartDownload && !x.m3u8FileUrl.IsNullOrWhiteSpace() && 
                 FileNameCleaner4UI.TryGetOutputFileNameByUrl( x.m3u8FileUrl, _SC.Settings.OutputFileExtension, out var outputFileName ) 
               )
            {
                if ( !_SC.UniqueUrlsOnly || !_DownloadListModel.ContainsUrl( x.m3u8FileUrl ) )
                {
                    var outputFileDirectory = _SC.OutputFileDirectory;
                    if ( FileNameCleaner4UI.TryCutFileNameIfFullPathTooLong( outputFileDirectory, outputFileName, out var cuttedFileName ) )
                        outputFileName = cuttedFileName;

                    var row = _DownloadListModel.AddRow( (x.m3u8FileUrl, requestHeaders, outputFileName, outputFileDirectory/*, IsLiveStream: false, default*/) );
                    await downloadListUC.SelectDownloadRowDelay( row );
                    _DC.Start( row );
                }
                return;
            }

            #region [.AddNewDownloadForm as top-always-owner-form.]
            AddNewDownloadForm.Add( this, _DC, _SC, x.m3u8FileUrl, requestHeaders, _OutputFileNamePatternProcessor, seriesInfo, AddNewDownloadForm_when_Add_formClosedAction );
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
        
        private void AddNewDownload_4_GroupedByAudioVideo( IReadOnlyList< UrlInputParams > xs )
        {
            if ( xs.AnyEx_() )
            {
                if ( xs.Count == 1 )
                {
                    var x = xs.First();
                    AddNewDownload_4_GroupedByAudioVideo( x );
                }
                else
                {
                    var action = new Action< UrlInputParams, (int n, int total) >( (UrlInputParams x, (int n, int total) seriesInfo) => AddNewDownload_4_GroupedByAudioVideo( x, seriesInfo ) );

                    var count = xs.Count;
                    var n     = count;
                    foreach ( var x in xs.Reverse() )
                    {
                        var seriesInfo = (n--, count);
                        this.BeginInvoke( action, x, seriesInfo );
                    }
                }
            }
            else
            {
                AddNewDownload( default );
            }
        }
        private async void AddNewDownload_4_GroupedByAudioVideo( UrlInputParams x, (int n, int total)? seriesInfo = null, string audioOutputFileSuffix = DownloadListUC.AUDIO_OUTPUTFILE_SUFFIX )
        {
            var suc_1 = BrowserIPC.ExtensionRequestHeader.Try2Dict( x.videoRequestHeaders, out var videoRequestHeaders, ignoreHostHeader: _SC.IgnoreHostHttpHeader /*|| x.autoStartDownload*/ );
            Debug.Assert( suc_1 || x.videoRequestHeaders.IsNullOrEmpty() );

            var suc_2 = BrowserIPC.ExtensionRequestHeader.Try2Dict( x.audioRequestHeaders, out var audioRequestHeaders, ignoreHostHeader: _SC.IgnoreHostHttpHeader /*|| x.autoStartDownload*/ );
            Debug.Assert( suc_2 || x.audioRequestHeaders.IsNullOrEmpty() );

            string get_outputFileName_4_audio( string outputFileName ) => Path.GetFileNameWithoutExtension( outputFileName ) + audioOutputFileSuffix + Path.GetExtension( outputFileName );

            if ( x.autoStartDownload && !x.videoUrl.IsNullOrWhiteSpace() && 
                 FileNameCleaner4UI.TryGetOutputFileNameByUrl( x.videoUrl, _SC.Settings.OutputFileExtension, out var outputFileName ) 
               )
            {
                if ( !_SC.UniqueUrlsOnly || (!_DownloadListModel.ContainsUrl( x.videoUrl ) && !_DownloadListModel.ContainsUrl( x.audioUrl )) )
                {
                    var outputFileDirectory = _SC.OutputFileDirectory;

                    var outputFileName_a = get_outputFileName_4_audio( outputFileName );                    
                    if ( FileNameCleaner4UI.TryCutFileNameIfFullPathTooLong( outputFileDirectory, outputFileName_a, out var cuttedFileName ) )
                        outputFileName_a = cuttedFileName;

                    var row_1 = _DownloadListModel.AddRow( (x.audioUrl, audioRequestHeaders, outputFileName_a, outputFileDirectory) );
                    await downloadListUC.SelectDownloadRowDelay( row_1 );
                    _DC.Start( row_1 );

                    if ( FileNameCleaner4UI.TryCutFileNameIfFullPathTooLong( outputFileDirectory, outputFileName, out cuttedFileName ) )
                        outputFileName = cuttedFileName;

                    var row_2 = _DownloadListModel.AddRow( (x.videoUrl, videoRequestHeaders, outputFileName, outputFileDirectory) );
                    await downloadListUC.SelectDownloadRowDelay( row_2 );
                    _DC.Start( row_2 );
                }
                return;
            }

            #region [.AddNewDownloadForm as top-always-owner-form.]
            var url_a = "[audio]: " + Ellipsis.Compact( x.audioUrl, 100, EllipsisFormat.Middle );
            var url_v = "[video]: " + Ellipsis.Compact( x.videoUrl, 100, EllipsisFormat.Middle );
            var url = url_a + Environment.NewLine + 
                      new string('-', /*Math.Max( url_a.Length, url_v.Length )*/150 ) + Environment.NewLine +
                      url_v;
            var requestHeaders = new[] {  new KeyValuePair<string, string>("/-------[audio]-------/", new string('-', 120))  }.Concat( videoRequestHeaders )
                                .Concat( [new KeyValuePair<string, string>("/-------[video]-------/", new string('-', 120))] ).Concat( audioRequestHeaders )
                                .ToList( videoRequestHeaders.Count + audioRequestHeaders.Count + 2 );
            FileNameCleaner4UI.TryGetOutputFileNameByUrl( x.videoUrl, _SC.Settings.OutputFileExtension, out outputFileName );
            AddNewDownloadForm.AddGrouped( this, _DC, _SC, url, (x.audioUrl, x.videoUrl), outputFileName, requestHeaders, _OutputFileNamePatternProcessor, seriesInfo, async f =>
            {
                if ( f.DialogResult == DialogResult.OK )
                {
                    var (outFn, outDir, isLiveStream, liveStreamMaxFileSize, autoStart) = (f.GetOutputFileName(), f.GetOutputDirectory(), f.IsLiveStream, f.LiveStreamMaxFileSizeInBytes, f.AutoStartDownload);
                    var outFn_a = get_outputFileName_4_audio( outFn );

                    var row_1 = _DownloadListModel.AddRow( (x.audioUrl, audioRequestHeaders, outFn_a, outDir, isLiveStream, liveStreamMaxFileSize) );
                    await downloadListUC.SelectDownloadRowDelay( row_1 );
                    if ( autoStart ) _DC.Start( row_1 );

                    var row_2 = _DownloadListModel.AddRow( (x.videoUrl, videoRequestHeaders, outFn, outDir, isLiveStream, liveStreamMaxFileSize) );
                    await downloadListUC.SelectDownloadRowDelay( row_2 );
                    if ( autoStart ) _DC.Start( row_2 );
                }

                if ( AddNewDownloadForm.TryGetOpenedForm( out var openedForm ) ) openedForm.ActivateAfterCloseOther();
            });
            #endregion
        }

        private void EditDownload( DownloadRow row )
        {
            if ( (row == null) || row.Status.IsRunningOrPaused() ) return;

            AddNewDownloadForm.Edit( this, _DC, _SC, row, _OutputFileNamePatternProcessor, null/*e => e.Cancel = e.Cancel || row.Status.IsRunningOrPaused()*/,
                                     AddNewDownloadForm_when_Edit_formClosedAction, AddNewDownloadForm_when_Add_formClosedAction );
        }
        private async Task AddNewDownloadForm_when_Add_formClosedAction( AddNewDownloadForm f )
        {
            if ( f.DialogResult == DialogResult.OK )
            {
                var row = _DownloadListModel.AddRow( (f.M3u8FileUrl, f.GetRequestHeaders(), f.GetOutputFileName(), f.GetOutputDirectory(), f.IsLiveStream, f.LiveStreamMaxFileSizeInBytes) );
                await downloadListUC.SelectDownloadRowDelay( row );
                if ( f.AutoStartDownload )
                {
                    _DC.Start( row );
                }
            }

            if ( AddNewDownloadForm.TryGetOpenedForm( out var openedForm ) )
            {
                openedForm.ActivateAfterCloseOther();
            }
        }
        private void AddNewDownloadForm_when_Edit_formClosedAction( AddNewDownloadForm f, DownloadRow row )
        {
            if ( (f.DialogResult == DialogResult.OK) && !row.Status.IsRunningOrPaused() )
            {
                row.Update( f.M3u8FileUrl, f.GetRequestHeaders(), f.GetOutputFileName(), f.GetOutputDirectory(), f.IsLiveStream, f.LiveStreamMaxFileSizeInBytes );
            }
        }

        private bool IsWaitBannerShown() => this.Controls.OfType< WaitBannerUC >().Any();
        //private void SuspendDrawing_DownloadListUC_And_Log()
        //{
        //    downloadListUC.SuspendDrawing();
        //    logUC.SuspendDrawing();
        //}
        //private void ResumeDrawing_DownloadListUC_And_Log()
        //{
        //    downloadListUC.ResumeDrawing();
        //    logUC.ResumeDrawing();
        //}
        #endregion

        #region [.menu.]
        private void addNewDownloadToolButton_Click( object sender, EventArgs e ) => AddNewDownload( default/*(null, null, false)*/ );
        private void showLogToolButton_Click( object sender, EventArgs e )
        {
            var showLog = showLogToolButton.Checked;
            _SC.ShowLog = showLog;
            mainSplitContainer.Panel2Collapsed = !showLog; //m3u8FileResultUC.Visible = showLog;
            logUC.SetModel( (showLog ? downloadListUC.GetSelectedDownloadRow()?.Log : null) );
        }
        private void copyToolButton_Click( object sender, EventArgs e )
        {
            var rows = downloadListUC.GetSelectedDownloadRows();
            if ( rows.Any() )
            {
                ClipboardHelper.CopyUrlsToClipboard( rows );
            }
            else
            {
                this.MessageBox_ShowError( "Nothing for Copy to clipboard.", this.Text );
            }
        }
        private void pasteToolButton_Click( object sender, EventArgs e )
        {            
            if ( ClipboardHelper.TryGetHttpUrlsFromClipboard( out var urls, _SC.IgnoreHostHttpHeader ) )
            {
                var autoStartDownload = ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
                if ( !autoStartDownload ) urls = urls.Take( MAX_PASTE_URLS ).ToList( MAX_PASTE_URLS );
                AddNewDownloads( (urls, autoStartDownload) );
            }
            else
            {
                this.MessageBox_ShowError( "Nothing for Paste from clipboard.", this.Text );
            }
        }
        private void columnsVisibilityEditorToolButton_Click( object sender, EventArgs e ) => statusBarUC.ShowDialog_ColumnsVisibilityEditor( downloadListUC.GetDataGridColumns(), downloadListUC.GetAlwaysVisibleDataGridColumns() );
        private void fileNameExcludesWordsEditorToolButton_Click( object sender, EventArgs e ) => statusBarUC.ShowDialog_FileNameExcludesWordsEditor();
        private void parallelismSettingsToolButton_Click( object sender, EventArgs e ) => statusBarUC.ShowDialog_ParallelismSettings();
        private void otherSettingsToolButton_Click( object sender, EventArgs e ) => statusBarUC.ShowDialog_OtherSettings();
        private void aboutToolButton_Click( object sender, EventArgs e )
        {
            var text = $"\"{AssemblyInfoHelper.AssemblyTitle}\" {AssemblyInfoHelper.FrameWorkName}" +
#if DEBUG
                       " / (DEBUG)" +
#endif
                       Environment.NewLine +
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
                       "  Ctrl+X:\t Cancel selected download" + Environment.NewLine +
                       "  Ctrl+Z:\t Undo deleted download" + Environment.NewLine +
                       "  Insert:\t Open add new download dialog" + Environment.NewLine +
                       "  Delete:\t Delete download (with or without output file)" + Environment.NewLine +
                       "  Enter:\t Open rename output file dialog" + Environment.NewLine +
                       "  F1:\t About dialog" + Environment.NewLine +
                       "  (Ctrl+Shift+G:  Collect Garbage)" + Environment.NewLine;
            this.MessageBox_ShowInformation( text, "about" );
        }

        private void startDownloadToolButton_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Start  );
        private void pauseDownloadToolButton_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Pause  );
        private void cancelDownloadToolButton_Click( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Cancel );
        private void deleteDownloadToolButton_Click( object sender, EventArgs e ) => DeleteDownloads( downloadListUC.GetSelectedDownloadRows().ToArrayEx(), deleteOutputFiles: false );
        private void deleteAllFinishedDownloadToolButton_Click( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.DeleteAllFinished );
        private void undoToolButton_Click( object sender, EventArgs e )
        {
            if ( _UndoModel.TryUndo( out var row ) )
            {
                _DownloadListModel.AddRowIf( row );
            }
        }

        private void downloadInstanceToolButton_ValueChanged( int downloadInstanceValue )
        {
            //if ( _SC.ShareMaxDownloadThreadsBetweenAllDownloadsInstance )
            //{
            _SC.MaxCrossDownloadInstance = downloadInstanceValue;
            //}
        }
        private void degreeOfParallelismToolButton_ValueChanged( int value ) => (_SC.MaxDegreeOfParallelism, _SC.MaxDegreeOfParallelismSaved) = (value, degreeOfParallelismToolButton.ValueSaved);
        private void speedThresholdToolButton_ValueChanged( decimal? value ) => (_SC.MaxSpeedThresholdInMbps, _SC.MaxSpeedThresholdInMbpsSaved) = (value, speedThresholdToolButton.ValueSaved); 
        #endregion

        #region [.context menu.]
        private void downloadListUC_MouseClickRightButton( MouseEventArgs e, DownloadRow selectedRow, bool outOfGridArea )
        {
            if ( (selectedRow != null) || (0 < _DownloadListModel.RowsCount) )
            {
                var selectedRow_AnyFileExists = false;
                startDownloadMenuItem              .Enabled = startDownloadToolButton .Enabled;
                cancelDownloadMenuItem             .Enabled = cancelDownloadToolButton.Enabled;
                pauseDownloadMenuItem              .Enabled = pauseDownloadToolButton .Enabled;
                deleteDownloadMenuItem             .Enabled = deleteDownloadToolButton.Enabled;
                moreOpMenuItem                     .Enabled = deleteDownloadToolButton.Enabled;
                deleteWithOutputFileMenuItem       .Enabled = deleteDownloadToolButton.Enabled && (selectedRow_AnyFileExists = FileHelper.AnyFileExists( selectedRow?.GetOutputFullFileNames() ));
                browseOutputFileMenuItem           .Visible = deleteWithOutputFileMenuItem.Enabled;
                openOutputFileMenuItem             .Visible = deleteWithOutputFileMenuItem.Enabled;     
                deleteAllFinishedDownloadMenuItem  .Enabled = deleteAllFinishedDownloadToolButton.Enabled;

                if ( selectedRow != null )
                {
                    var rows = downloadListUC.GetSelectedDownloadRows();

                    #region [.changeOutputDirectoryMenuItem & openOutputFilesWithExternalMenuItem.]                    
                    openOutputFilesWithExternalMenuItem.Visible = true;
                    if ( _ExternalProgQueue.Any() )
                    {                        
                        var outputFileNames = (from r in rows select r.GetOutputFullFileName()).ToList();
                        if ( outputFileNames.Any( _ExternalProgQueue.Contains ) )
                        {
                            openOutputFilesWithExternalMenuItem.CheckState = outputFileNames.All( _ExternalProgQueue.Contains ) ? CheckState.Checked : CheckState.Indeterminate;
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
                    #endregion

                    #region [.onlyDeleteOutputFileMenuItem.]
                    if ( selectedRow_AnyFileExists )
                    {
                        onlyDeleteOutputFileMenuItem.Enabled = !selectedRow.Status.IsRunningOrPaused();
                    }
                    else
                    {
                        var x = rows.Where( r => !r.Status.IsRunningOrPaused() ).FirstOrDefault( r => FileHelper.AnyFileExists( r.GetOutputFullFileNames() ) );
                        onlyDeleteOutputFileMenuItem.Enabled = (x != null);
                    }
                    #endregion

                    #region [.editDownloadMenuItem.]
                    editDownloadMenuItem.Visible = 
                        editDownloadMenuItem_Separator.Visible = (rows.Count == 1) && !selectedRow.Status.IsRunningOrPaused();
                    #endregion
                }
                else
                {
                    changeOutputDirectoryMenuItem.Visible =
                        openOutputFilesWithExternalMenuItem.Visible = false;

                    onlyDeleteOutputFileMenuItem.Enabled = false;

                    editDownloadMenuItem.Visible = 
                        editDownloadMenuItem_Separator.Visible = false;
                }

                SetAllDownloadsMenuItemsEnabled( allowedAll: (selectedRow == null) || (1 < _DownloadListModel.RowsCount) );

                mainContextMenu.Show( downloadListUC, e.Location );
            }
        }
        private void SetAllDownloadsMenuItemsEnabled( bool allowedAll )
        {
            if ( allowedAll )
            {
                int start = 0, cancel = 0, pause = 0, delete = 0, deleteWithFiles = 0;
                foreach ( var row in _DownloadListModel.GetRows_Enumerable() )
                {
                    var status = row.Status;
                    start  += StartDownload_IsAllowed ( status ) && !status.IsFinished() ? 1 : 0;
                    cancel += CancelDownload_IsAllowed( status ) ? 1 : 0;
                    pause  += PauseDownload_IsAllowed ( status ) ? 1 : 0;
                    delete++;
                    if ( FileHelper.AnyFileExists( row.GetOutputFullFileNames() ) )
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
                        pauseAllDownloadsMenuItem.Enabled = false;

                deleteAllDownloadsMenuItem      .Enabled = deleteDownloadMenuItem      .Enabled;
                deleteAllWithOutputFilesMenuItem.Enabled = deleteWithOutputFileMenuItem.Enabled; 
            }
        }

        private void startDownloadMenuItem_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Start  );
        private void pauseDownloadMenuItem_Click ( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Pause  );
        private void cancelDownloadMenuItem_Click( object sender, EventArgs e ) => ProcessDownloadCommand4SelectedRows( DownloadCommandEnum.Cancel );

        private void editDownloadMenuItem_Click( object sender, EventArgs e ) => EditDownload( downloadListUC.GetSelectedDownloadRow() );

        private void deleteDownloadMenuItem_Click( object sender, EventArgs e ) => deleteDownloadToolButton_Click( sender, e );
        private void deleteWithOutputFileMenuItem_Click( object sender, EventArgs e ) => DeleteDownloads( downloadListUC.GetSelectedDownloadRows().ToArrayEx(), deleteOutputFiles: true );
        private void onlyDeleteOutputFileMenuItem_Click( object sender, EventArgs e ) => OnlyDeleteOutputFiles( downloadListUC.GetSelectedDownloadRows().ToArrayEx() );

        private void browseOutputFileMenuItem_Click( object sender, EventArgs e )
        {
            var row = downloadListUC.GetSelectedDownloadRow();
            if ( FileHelper.TryGetFirstFileExists( row?.GetOutputFullFileNames(), out var outputFileName ) )
            {                
                var suc = WinApi.ShellExploreAndSelectFile( outputFileName, out var error );
                //using ( Process.Start( "explorer", $"/e,/select,\"{outputFileName}\"" ) ) {; }
            }
        }
        private void openOutputFileMenuItem_Click( object sender, EventArgs e )
        {
            var row = downloadListUC.GetSelectedDownloadRow();
            if ( (row != null) && row.IsFinishedOrError() && FileHelper.TryGetFirstFileExists( row.GetOutputFullFileNames(), out var outputFileName ) )
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
            var externalProgFilePath = _SC.Settings.ExternalProgFilePath;
            if ( !File.Exists( externalProgFilePath ) )
            {
                this.MessageBox_ShowError( $"External program file doesn't exists: '{externalProgFilePath}'", _APP_TITLE_ );
                return;
            }

            var rows = downloadListUC.GetSelectedDownloadRows();
            var outputFileNames = (from row in rows
                                   where row.IsFinishedOrError()
                                   let t = FileHelper.TryGetFirstFileExists( row.GetOutputFullFileNames() )
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
            foreach ( var row in _DownloadListModel.GetRows_ArrayCopy() )
            {
                var status = row.Status;
                if ( StartDownload_IsAllowed( status ) && !status.IsFinished() )
                {
                    _DC.Start( row );
                }
            }
        }
        private void pauseAllDownloadsMenuItem_Click( object sender, EventArgs e )
        {
            foreach ( var row in _DownloadListModel.GetRows_ArrayCopy() )
            {
                if ( PauseDownload_IsAllowed( row.Status ) )
                {
                    _DC.Pause( row );
                }
            }
        }
        private void cancelAllDownloadsMenuItem_Click( object sender, EventArgs e )
        {
            foreach ( var row in _DownloadListModel.GetRows_ArrayCopy() )
            {
                if ( CancelDownload_IsAllowed( row.Status ) )
                {
                    _DC.Cancel( row );
                }
            }
        }

        private void deleteAllDownloadsMenuItem_Click( object sender, EventArgs e ) => DeleteDownloads( _DownloadListModel.GetRows_ArrayCopy(), deleteOutputFiles: false );
        private void deleteAllWithOutputFilesMenuItem_Click( object sender, EventArgs e ) => DeleteDownloads( _DownloadListModel.GetRows_ArrayCopy(), deleteOutputFiles: true );


        private void moreOp_delOutputFileAndChangeExt2Mp3_MenuItem_Click( object sender, EventArgs e ) => DelOutputFileAndChangeExt( ".mp3" );
        private void moreOp_delOutputFileAndChangeExt2Mp4_MenuItem_Click( object sender, EventArgs e ) => DelOutputFileAndChangeExt( ".mp4" );
        private void DelOutputFileAndChangeExt( /*DownloadRow[] rows,*/ string ext )
        {
            var rows = downloadListUC.GetSelectedDownloadRows().ToArrayEx();

            OnlyDeleteOutputFiles( rows, ask: true, row =>
            {
                var fn     = row.OutputFileName;
                var new_fn = Path.GetFileNameWithoutExtension( fn ) + ext;
                if ( fn != new_fn )
                {
                    var suc = _ExternalProgQueue.Remove( row.GetOutputFullFileName() );
                    row.SetOutputFileName( new_fn );
                    if ( suc ) _ExternalProgQueue.Add( row.GetOutputFullFileName() );
                }
            });
        }
        #endregion

        #region [.change OutputFileName & OutputDirectory.]
        private void downloadListUC_OutputFileNameClick( DownloadRow row )
        {
            if ( ChangeOutputFileForm.TryChangeOutputFile( this, row, _SC, out var outputFileName ) )
            {
                ChangeOutputFileName( row, outputFileName );
                downloadListUC.Invalidate( true );
            }
        }
        private void downloadListUC_OutputDirectoryClick( DownloadRow row )
        {
            if ( DirectorySelectDialog.Show( this, _SC.Settings.UseDirectorySelectDialogModern, GetSelectedDirectory( row ), $"Select output directory for file: '{row.OutputFileName}'", out var outputDirectory ) )
            {
                _SC.Settings.LastChangeOutputDirectory = outputDirectory;
                ChangeOutputDirectory( row, outputDirectory );
                downloadListUC.Invalidate( true );
            }
        }

        private void changeOutputDirectoryMenuItem_Click( object sender, EventArgs e )
        {
            var rows = downloadListUC.GetSelectedDownloadRows();
            if ( rows.Any() )
            {
                var first_row = rows.First();
                var descr = (rows.Count == 1) ? $"Select output directory for file: '{first_row.OutputFileName}'" : $"Select output directory for {rows.Count} files";
                if ( DirectorySelectDialog.Show( this, _SC.Settings.UseDirectorySelectDialogModern, GetSelectedDirectory( first_row ), descr, out var outputDirectory ) )
                {
                    _SC.Settings.LastChangeOutputDirectory = outputDirectory;
                    foreach ( var row in rows )
                    {
                        ChangeOutputDirectory( row, outputDirectory );
                    }
                    downloadListUC.Invalidate( true );
                }
            }
        }
        private string GetSelectedDirectory( DownloadRow row ) => FileHelper.GetFirstExistsDirectory( _SC.Settings.LastChangeOutputDirectory ) ?? row.OutputDirectory;

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
                case MoveFileByRenameResultEnum.Suc:
                    row.SaveVeryFirstOutputFullFileName( null );
                    break;
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
            if ( (!row.Status.IsRunningOrPaused() || FileHelper.IsSameDiskDrive( prev_outputFullFileName, new_outputFullFileName )) && File.Exists( prev_outputFullFileName ) )
            {
                switch ( mode )
                {
                    case MoveFileByRenameModeEnum.OverwriteSilent:
                        break;
                    case MoveFileByRenameModeEnum.OverwriteAsk:
                        if ( File.Exists( new_outputFullFileName ) )
                        {
                            if ( this.MessageBox_ShowQuestion( $"File '{new_outputFullFileName}' already exists. Overwrite ?", "Overwrite exists file" ) != DialogResult.Yes )
                            {
                                return (MoveFileByRenameResultEnum.Canceled);
                            }
                        }
                        break;
                    case MoveFileByRenameModeEnum.SkipIfAlreadyExists:
                        if ( File.Exists( new_outputFullFileName ) )
                        {
                            return (MoveFileByRenameResultEnum.Canceled);
                        }
                        break;
                }

                if ( FileHelper.TryMoveFile_NoThrow( prev_outputFullFileName, new_outputFullFileName, out var error ) )
                {
                    return (MoveFileByRenameResultEnum.Suc);
                }
                else
                {
                    this.MessageBox_ShowError( error.ToString(), "Move/Remane output file" );
                    return (MoveFileByRenameResultEnum.Fail);
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

        #region [.Collect_Garbage.]
        private void Collect_Garbage()
        {
            CollectGarbage.Collect_Garbage( out var totalMemoryBytes );

            statusBarUC.ShowDisappearingMessage( $"Collect Garbage. Total Memory: {(totalMemoryBytes / (1024.0 * 1024)):N2} MB." );
        }
        #endregion
    }
}
