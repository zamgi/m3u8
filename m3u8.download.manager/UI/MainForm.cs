using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.ipc;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using m3u8.download.manager.ui.infrastructure;
using _ReceivedInputParamsArrayEventHandler_ = m3u8.download.manager.ipc.PipeIPC.NamedPipeServer__in.ReceivedInputParamsArrayEventHandler;
using _CollectionChangedTypeEnum_            = m3u8.download.manager.models.DownloadListModel.CollectionChangedTypeEnum;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class MainForm : Form
    {
        #region [.fields.]
        private (string m3u8FileUrl, bool autoStartDownload)[] _InputParamsArray;
        private _ReceivedInputParamsArrayEventHandler_ _ReceivedInputParamsArrayEventHandler;

        private DownloadListModel                _DownloadListModel;
        private DownloadController               _DownloadController;
        private SettingsPropertyChangeController _SettingsController;
        private LogRowsHeightStorer              _LogRowsHeightStorer;
        private Action< DownloadRow, string >    _DownloadListModel_RowPropertiesChangedAction;
        private bool                             _ShowDownloadStatistics;
        #endregion

        #region [.ctor().]
        private MainForm()
        {
            InitializeComponent();
            this.Text = Resources.APP_TITLE;
            //----------------------------------------//

            _DownloadListModel_RowPropertiesChangedAction = new Action< DownloadRow, string >( DownloadListModel_RowPropertiesChanged );

            _SettingsController = new SettingsPropertyChangeController();
            _LogRowsHeightStorer = new LogRowsHeightStorer();

            _DownloadListModel  = new DownloadListModel();
            _DownloadListModel.RowPropertiesChanged += DownloadListModel_RowPropertiesChanged;            
            _DownloadController = new DownloadController( _DownloadListModel , _SettingsController );

            _SettingsController.SettingsPropertyChanged += SettingsController_PropertyChanged;
            SettingsController_PropertyChanged( _SettingsController.Settings, nameof(Settings.ShowDownloadStatisticsInMainFormTitle) );

            logUC.SetModel( null );
            logUC.SetSettingsController( _SettingsController );
            logUC.SetLogRowsHeightStorer( _LogRowsHeightStorer );

            downloadListUC.SetModel( _DownloadListModel );
            downloadListUC.KeyDown += (s, e) => this.OnKeyDown( e );
            downloadListUC.MouseClickRightButton   += downloadListUC_MouseClickRightButton;
            downloadListUC.UpdatedSingleRunningRow += downloadListUC_UpdatedSingleRunningRow;
            statusBarUC.SetDownloadController( _DownloadController );
            statusBarUC.SetSettingsController( _SettingsController );

            _ReceivedInputParamsArrayEventHandler = ((string m3u8FileUrl, bool autoStartDownload)[] array) => AddNewDownloads( array );
            PipeIPC.NamedPipeServer__in.ReceivedInputParamsArray += NamedPipeServer__in_ReceivedInputParamsArray;

            NameCleaner.ResetExcludesWords( _SettingsController.NameCleanerExcludesWords );

            showLogToolButton.Checked = Settings.Default.ShowLog;
            showLogToolButton_Click( showLogToolButton, EventArgs.Empty );

            downloadInstanceToolButton.Visible = _SettingsController.MaxCrossDownloadInstance.HasValue;
            if ( _SettingsController.MaxCrossDownloadInstance.HasValue )
            {
                downloadInstanceToolButton.Value = _SettingsController.MaxCrossDownloadInstance.Value;
            }
            //degreeOfParallelismToolButton.Visible = ;
            degreeOfParallelismToolButton.Value = _SettingsController.MaxDegreeOfParallelism;
        }
        public MainForm( in (string m3u8FileUrl, bool autoStartDownload)[] array ) : this() => _InputParamsArray = array;
        #endregion

        #region [.override methods.]
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !base.DesignMode )
            {
                FormPositionStorer.Load( this, _SettingsController.MainFormPositionJson );
            }
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            if ( !base.DesignMode )
            {
                _SettingsController.MainFormPositionJson = FormPositionStorer.Save( this );
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
            else
            if ( Extensions.TryGetM3u8FileUrlsFromClipboard( out var m3u8FileUrls ) )
            {
                AddNewDownload( (m3u8FileUrls.FirstOrDefault(), false) );
            }
        }
        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );

            //still downloading?
            if ( _DownloadController.IsDownloading )
            {
                if ( this.WindowState == FormWindowState.Minimized )
                {
                    this.WindowState = FormWindowState.Normal;
                }
                if ( this.MessageBox_ShowQuestion( "Dou you want to _cancel_ all downloading and exit ?", Resources.APP_TITLE, MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2 ) == DialogResult.Yes )
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
                            AddNewDownloads( (m3u8FileUrls, false) );
                            return;
                        }
                    break;

                    case Keys.C: //Copy
                        if ( downloadListUC.HasFocus )
                        {
                            var row = downloadListUC.GetSelectedDownloadRow();
                            if ( row != null )
                            {
                                e.SuppressKeyPress = true;
                                Extensions.CopyM3u8FileUrlToClipboard( row.Url );
                                return;
                            }
                        }
                    break;

                    case Keys.S: //Start download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand( DownloadCommandEnum.Start );
                        }
                    break;

                    case Keys.P: //Pause download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand( DownloadCommandEnum.Pause );
                        }
                    break;

                    case Keys.Z: //Cancel download
                        if ( downloadListUC.HasFocus )
                        {
                            ProcessDownloadCommand( DownloadCommandEnum.Cancel );
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
                            var row = downloadListUC.GetSelectedDownloadRow();
                            if ( AskDeleteDownloadDialog( row, askOnlyOutputFileExists: false, deleteOutputFile: e.Shift ) )
                            {
                                DeleteDownload( row, deleteOutputFile: e.Shift );
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
        private void NamedPipeServer__in_ReceivedInputParamsArray( (string m3u8FileUrl, bool autoStartDownload)[] array ) => this.BeginInvoke( _ReceivedInputParamsArrayEventHandler, array );

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

                    this.Text = $"run: {runningCount}{waitText}{finishedText}{errorText},  [{Resources.APP_TITLE}]";
                }
                else
                {
                    var createdCount  = stats[ DownloadStatus.Created  ];
                    var pausedCount   = stats[ DownloadStatus.Paused   ];
                    var canceledCount = stats[ DownloadStatus.Canceled ];

                    var pauseText    = ((0 < pausedCount  ) ? $", pause: {pausedCount}"    : null);
                    var canceledText = ((0 < canceledCount) ? $", cancel: {canceledCount}" : null);
                    this.Text = $"new: {createdCount}{pauseText}{canceledText}{finishedText}{errorText},  [{Resources.APP_TITLE}]";
                }
            }
            else
            {
                this.Text = Resources.APP_TITLE;
            }
        }

        private void downloadListUC_UpdatedSingleRunningRow( DownloadRow row )
        {
            if ( _ShowDownloadStatistics )
            {
                this.Text = $"{DownloadListUC.GetDownloadInfoText( row )},  [{Resources.APP_TITLE}]";
            }
            //else if ( this.Text != Resources.APP_TITLE )
            //{
            //    this.Text = Resources.APP_TITLE;
            //}
        }
        private void DownloadListModel_CollectionChanged( _CollectionChangedTypeEnum_ collectionChangedType )
        {
            if ( collectionChangedType != _CollectionChangedTypeEnum_.Sort )
            {
                ShowDownloadStatisticsInTitle();

                switch ( collectionChangedType )
                {
                    case _CollectionChangedTypeEnum_.BulkUpdate:
                    case _CollectionChangedTypeEnum_.Remove:                    
                        _LogRowsHeightStorer.LeaveOnly( (from row in _DownloadListModel.GetRows() select row.Log) );
                    break;

                    case _CollectionChangedTypeEnum_.Clear:
                        _LogRowsHeightStorer.Clear();
                    break;
                }
            }
        }
        private void DownloadListModel_RowPropertiesChanged( DownloadRow row, string propertyName )
        {
            if ( propertyName == nameof(DownloadRow.Status) )
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
            }
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
                        _DownloadController.Start( row );
                    break;

                    case DownloadCommandEnum.Pause:
                        _DownloadController.Pause( row );
                    break;

                    case DownloadCommandEnum.Cancel:
                        _DownloadController.Cancel( row );
                    break;

                    case DownloadCommandEnum.Delete:
                        #region comm. with waiting
                        /*
                        using ( var cts = new CancellationTokenSource() )
                        using ( WaitBannerUC.Create( this, cts, visibleDelayInMilliseconds: 1500 ) )
                        {
                            try
                            {
                                await _DownloadController.CancelWithWait( row, cts.Token );
                                _DownloadListModel.RemoveRow( row );
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
        private async void DeleteDownload( DownloadRow row, bool deleteOutputFile = true )
        {
            if ( row == null )
            {
                return;
            }

            if ( deleteOutputFile )
            {
                this.SetEnabledAllChildControls( false );
                try
                {
                    using ( var cts = new CancellationTokenSource() )
                    using ( WaitBannerUC.Create( this, cts, visibleDelayInMilliseconds: 2_000 ) )
                    {
                        ProcessDownloadCommand( DownloadCommandEnum.Delete, row );
                        await TryDeleteFiles_Async( row.GetOutputFullFileNames(), cts.Token );
                    }
                }
                catch ( OperationCanceledException ) //( Exception ex ) when (cts.IsCancellationRequested)
                {
                    ; //Debug.WriteLine( ex );
                }
                finally
                {
                    this.SetEnabledAllChildControls( true );
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

            this.SetEnabledAllChildControls( false );
            try
            {
                using ( var cts = new CancellationTokenSource() )
                using ( WaitBannerUC.Create( this, cts, visibleDelayInMilliseconds: 2_000 ) )
                {
                    foreach ( var row in rows )
                    {
                        _DownloadController.Cancel( row );
                        _DownloadListModel .RemoveRow( row );

                        await TryDeleteFiles_Async( row.GetOutputFullFileNames(), cts.Token );
                    }
                }
            }
            catch ( OperationCanceledException ) //( Exception ex ) when (cts.IsCancellationRequested)
            {
                ; //Debug.WriteLine( ex );
            }
            finally
            {
                this.SetEnabledAllChildControls( true );
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
                var defaultButton        = MessageBoxDefaultButton.Button1; //---(anyOutputFileExists ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1);
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
                var r = (this.MessageBox_ShowQuestion( msg, this.Text, MessageBoxButtons.OKCancel, defaultButton ) == DialogResult.OK);
                return (r);
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
                        ((string m3u8FileUrl, bool autoStartDownload) tp, (int n, int total) seriesInfo ) => AddNewDownload( in tp, seriesInfo ) );

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
                AddNewDownload( ((string) null, false) );
            }
        }
        private void AddNewDownload( in (string m3u8FileUrl, bool autoStartDownload) p, (int n, int total)? seriesInfo = null )
        {
            if ( p.autoStartDownload && !p.m3u8FileUrl.IsNullOrWhiteSpace() &&
                 FileNameCleaner.TryGetOutputFileNameByUrl( p.m3u8FileUrl, out var outputFileName ) 
               )
            {
                if ( _SettingsController.UniqueUrlsOnly && !_DownloadListModel.ContainsUrl( p.m3u8FileUrl ) )
                {
                    var row = _DownloadListModel.AddRow( (p.m3u8FileUrl, outputFileName, _SettingsController.OutputFileDirectory) );
                    downloadListUC.SelectDownloadRow( row );
                    _DownloadController.Start( row );
                }
                return;
            }

            using ( var f = new AddNewDownloadForm( _DownloadController, _SettingsController, p.m3u8FileUrl, seriesInfo ) )
            {
                if ( f.ShowDialog( this ) == DialogResult.OK )
                {
                    var row = _DownloadListModel.AddRow( (f.M3u8FileUrl, f.GetOutputFileName(), f.GetOutputDirectory()) );
                    downloadListUC.SelectDownloadRow( row );
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
        }
        #endregion

        #region [.menu.]
#if DEBUG
        private void addNewDownloadToolButton_Click( object sender, EventArgs e ) => AddNewDownload( ($"http://xzxzzxzxxz.ru/{(new Random().Next())}/asd.egf", false) );
#else
        private void addNewDownloadToolButton_Click( object sender, EventArgs e ) => AddNewDownload( (null, false) );
#endif
        private void showLogToolButton_Click( object sender, EventArgs e )
        {
            var showLog = showLogToolButton.Checked;
            Settings.Default.ShowLog = showLog;
            mainSplitContainer.Panel2Collapsed = !showLog; //m3u8FileResultUC.Visible = showLog;
            logUC.SetModel( (showLog ? downloadListUC.GetSelectedDownloadRow()?.Log : null) );
        }
        private void copyToolButton_Click( object sender, EventArgs e )
        {
            var row = downloadListUC.GetSelectedDownloadRow();
            if ( row != null )
            {
                Extensions.CopyM3u8FileUrlToClipboard( row.Url );
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
                AddNewDownloads( (m3u8FileUrls, false) );
            }
            else
            {
                this.MessageBox_ShowError( "Nothing for paste from clipboard.", this.Text );
            }
        }
        private void aboutToolButton_Click( object sender, EventArgs e )
        {
            var text = $"\"{AssemblyInfoHelper.AssemblyTitle}\"" + Environment.NewLine +
                       //AssemblyInfoHelper.AssemblyProduct + Environment.NewLine +
                       AssemblyInfoHelper.AssemblyCopyright + Environment.NewLine +
                       //AssemblyInfoHelper.AssemblyCompany + Environment.NewLine +
                       //AssemblyInfoHelper.AssemblyDescription + Environment.NewLine +
                       Environment.NewLine +
                       $"Version {AssemblyInfoHelper.AssemblyVersion}, ({AssemblyInfoHelper.AssemblyLastWriteTime})" +
                       Environment.NewLine +
                       Environment.NewLine +
                       "Shortcut's:" + Environment.NewLine +
                       "  Ctrl+C:\t Copy selected download url to clipboard" + Environment.NewLine +
                       "  Ctrl+B:\t Browse output file (if exists)" + Environment.NewLine +
                       "  Ctrl+D:\t Minimized application window" + Environment.NewLine +
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
            this.MessageBox_ShowInformation( text, "about" ); //$"'{this.Text}' version: {Assembly.GetExecutingAssembly().GetName().Version}", this.Text );
        }

        private void startDownloadToolButton_Click ( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Start  );
        private void pauseDownloadToolButton_Click ( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Pause  );
        private void cancelDownloadToolButton_Click( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Cancel );
        private void deleteDownloadToolButton_Click( object sender, EventArgs e )
        {
            var row = downloadListUC.GetSelectedDownloadRow();
            if ( AskDeleteDownloadDialog( row, askOnlyOutputFileExists: true, deleteOutputFile: false ) )
            {
                DeleteDownload( row, deleteOutputFile: false );
            }
        }
        private void deleteAllFinishedDownloadToolButton_Click( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.DeleteAllFinished );

        private void downloadInstanceToolButton_ValueChanged( int downloadInstanceValue )
        {
            if ( _SettingsController.UseCrossDownloadInstanceParallelism )
            {
                _SettingsController.Settings.MaxCrossDownloadInstance = downloadInstanceValue;
            }
        }
        private void degreeOfParallelismToolButton_ValueChanged( int value ) => _SettingsController.Settings.MaxDegreeOfParallelism = value;
        #endregion

        #region [.context menu.]
        private void downloadListUC_MouseClickRightButton( MouseEventArgs e, DownloadRow row )
        {
            if ( (row != null) || (0 < _DownloadListModel.RowsCount) )
            {                
                startDownloadMenuItem            .Enabled = startDownloadToolButton .Enabled;
                cancelDownloadMenuItem           .Enabled = cancelDownloadToolButton.Enabled;
                pauseDownloadMenuItem            .Enabled = pauseDownloadToolButton .Enabled;
                deleteDownloadMenuItem           .Enabled = deleteDownloadToolButton.Enabled;
                deleteWithOutputFileMenuItem     .Enabled = deleteDownloadToolButton.Enabled && Extensions.AnyFileExists( row?.GetOutputFullFileNames() );
                browseOutputFileMenuItem         .Visible = deleteWithOutputFileMenuItem.Enabled;
                openOutputFileMenuItem           .Visible = deleteWithOutputFileMenuItem.Enabled;
                deleteAllFinishedDownloadMenuItem.Enabled = deleteAllFinishedDownloadToolButton.Enabled;

                var allowedAll = (row == null) || (1 < _DownloadListModel.RowsCount);
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

                void set_enabled_and_text( ToolStripMenuItem menuItem, int count )
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
                }

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

        private void startDownloadMenuItem_Click ( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Start  );
        private void pauseDownloadMenuItem_Click ( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Pause  );
        private void cancelDownloadMenuItem_Click( object sender, EventArgs e ) => ProcessDownloadCommand( DownloadCommandEnum.Cancel );

        private void deleteDownloadMenuItem_Click( object sender, EventArgs e ) => deleteDownloadToolButton_Click( sender, e );
        private void deleteWithOutputFileMenuItem_Click( object sender, EventArgs e ) => DeleteDownload( downloadListUC.GetSelectedDownloadRow() );

        private void browseOutputFileMenuItem_Click( object sender, EventArgs e )
        {
            var row = downloadListUC.GetSelectedDownloadRow();
            if ( Extensions.TryGetFirstFileExists( row?.GetOutputFullFileNames(), out var outputFileName ) )
            {
                using ( Process.Start( "explorer", "/e,/select," + outputFileName ) )
                {
                    ;
                }
            }
        }
        private void openOutputFileMenuItem_Click( object sender, EventArgs e )
        {
            var row = downloadListUC.GetSelectedDownloadRow();
            if ( Extensions.TryGetFirstFileExists( row?.GetOutputFullFileNames(), out var outputFileName ) )
            {
                using ( Process.Start( outputFileName ) )
                {
                    ;
                }
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

        private void deleteAllDownloadsMenuItem_Click( object sender, EventArgs e )
        {
            var rows = _DownloadListModel.GetRows().ToArray();
            foreach ( var row in rows )
            {
                _DownloadController.Cancel( row );
                //_DownloadListModel .RemoveRow( row );
            }
            _DownloadListModel.RemoveAll( rows );

            //-2-//
            SetDownloadToolButtonsStatus( downloadListUC.GetSelectedDownloadRow() );
        }
        private void deleteAllWithOutputFilesMenuItem_Click( object sender, EventArgs e )
        {
            DeleteDownloadsWithOutputFiles( _DownloadListModel.GetRows().ToArray() );

            //-2-//
            SetDownloadToolButtonsStatus( downloadListUC.GetSelectedDownloadRow() );
        }
        #endregion

        #region [.ChangeOutputFileForm.]
        private ChangeOutputFileForm _ChangeOutputFileForm;
        private DownloadRow          _ChangeOutputFileForm_Row;
        private void downloadListUC_OutputFileNameClick( DownloadRow row )
        {
            if ( (_ChangeOutputFileForm == null) || _ChangeOutputFileForm.IsDisposed )
            {
                _ChangeOutputFileForm = new ChangeOutputFileForm() { Owner = this };
                _ChangeOutputFileForm.FormClosed += _ChangeOutputFileForm_FormClosed;
            }
            _ChangeOutputFileForm_Row = row;
            _ChangeOutputFileForm.OutputFileName = row.OutputFileName;
            if ( !_ChangeOutputFileForm.Visible )
            {
                _ChangeOutputFileForm.Show( this );
            }
        }

        private void _ChangeOutputFileForm_FormClosed( object sender, FormClosedEventArgs e )
        {
            if ( (e.CloseReason == CloseReason.UserClosing) && (_ChangeOutputFileForm.DialogResult == DialogResult.OK) &&
                 FileNameCleaner.TryGetOutputFileName( _ChangeOutputFileForm.OutputFileName, out var outputFileName )
               )
            {
                _ChangeOutputFileForm_Row?.SetOutputFileName( outputFileName );
                downloadListUC.Invalidate( true );
            }
            _ChangeOutputFileForm.FormClosed -= _ChangeOutputFileForm_FormClosed;
            _ChangeOutputFileForm = null;
        }


        private void downloadListUC_OutputDirectoryClick( DownloadRow row )
        {
            using ( var d = new FolderBrowserDialog() { SelectedPath        = row.OutputDirectory,
                                                        Description         = $"Select output directory: '{row.OutputFileName}'",
                                                        ShowNewFolderButton = true } )
            {
                if ( d.ShowDialog( this ) == DialogResult.OK )
                {
                    row.SetOutputDirectory( d.SelectedPath );
                    downloadListUC.Invalidate( true );
                }
            }
        }
        #endregion
    }
}
