using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using m3u8.client;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;

using _DC_ = m3u8.download.manager.controllers.DownloadController;
using _SC_ = m3u8.download.manager.controllers.SettingsPropertyChangeController;
using Color2ColorTransitionProcessor = m3u8.download.manager.ui.ToolStripStatusLabelEx.Color2ColorTransitionProcessor;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class StatusBarUC : UserControl
    {
        #region [.fields.]
        public event EventHandler SettingsChanged;

        private _DC_ _DC;
        private _SC_ _SC;
        private Color2ColorTransitionProcessor _C2CTProcessor;
        private IReceivedAndWritedPartsProcessor _ReceivedAndWritedPartsProcessor;
        #endregion

        #region [.ctor().]
        public StatusBarUC( _DC_ dc, _SC_ sc, IReceivedAndWritedPartsProcessor receivedAndWritedPartsProcessor )
        {
            InitializeComponent();
            //----------------------------------------//

            _DC = dc ?? throw (new ArgumentNullException( nameof(dc) ));
            _SC = sc ?? throw (new ArgumentNullException( nameof(sc) ));
            _ReceivedAndWritedPartsProcessor = receivedAndWritedPartsProcessor ?? throw (new ArgumentNullException( nameof(receivedAndWritedPartsProcessor) ));
            _SC.SettingsPropertyChanged += SettingsController_PropertyChanged;

            //LeftSideTextLabelText = null;
            parallelismLabel_set();
            settingsLabel_set();

            _C2CTProcessor = new Color2ColorTransitionProcessor( leftSideTextLabel_2 );
        }
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                _DetachTrackItemsCountAction?.Invoke();
                _C2CTProcessor.Dispose();
                _SC.SettingsPropertyChanged -= SettingsController_PropertyChanged;
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.public.]
        private Settings GetSettings() => _SC.Settings;

        #region [.TrackItemsCount.]
        private Action _DetachTrackItemsCountAction;
        public void TrackItemsCount( DownloadListUC downloadListUC )
        {
            _DetachTrackItemsCountAction?.Invoke();

            if ( downloadListUC == null ) throw (new ArgumentNullException( nameof(downloadListUC) ));
            var model = downloadListUC.Model ?? throw (new ArgumentNullException( nameof(downloadListUC.Model) ));

            var setItemsCount = default(Action);
            setItemsCount = new Action(() =>
            {
                if ( this.InvokeRequired )
                {
                    this.BeginInvoke( setItemsCount );
                    return;
                }
                var cnt = downloadListUC.GetSelectedDownloadRowsCount();
                leftSideTextLabel.Text = (1 < cnt) ? $"{cnt} items selected" : $"{model.RowsCount} items";
            });

            var downloadListUC_SelectionChanged     = new DownloadListUC.SelectionChangedEventHandler( _ => setItemsCount() );
            var downloadListModel_CollectionChanged = new ListModel< DownloadRow >.CollectionChangedEventHandler( (collectionChangedType, _) => setItemsCount() );

            downloadListUC.SelectionChanged  += downloadListUC_SelectionChanged;
            model         .CollectionChanged += downloadListModel_CollectionChanged;

            _DetachTrackItemsCountAction = () =>
            {
                if ( downloadListUC != null )
                {
                    downloadListUC.SelectionChanged -= downloadListUC_SelectionChanged;
                    downloadListUC = null;
                }
                if ( model != null )
                {
                    model.CollectionChanged -= downloadListModel_CollectionChanged;
                    model = null;
                }
            };

            setItemsCount();
        }
        #endregion

        #region [.Show disappearing message.]
        public void ShowDisappearingMessage( string message, KnownColor foreColor = KnownColor.DodgerBlue, int millisecondsDelay = 3 * 1_000 )
            => _C2CTProcessor.Run( message, foreColor, millisecondsDelay );
        #endregion

        public bool IsVisibleSettingsLabel      { get => settingsLabel      .Visible; set => settingsLabel      .Visible = value; }
        public bool IsVisibleParallelismLabel   { get => parallelismLabel   .Visible; set => parallelismLabel   .Visible = value; }
        public bool IsVisibleExcludesWordsLabel { get => exceptionWordsLabel.Visible; set => exceptionWordsLabel.Visible = value; }
        //---public string LeftSideTextLabelText     { get => leftSideTextLabel  .Text   ; set => leftSideTextLabel  .Text    = value; }
        //public bool IsVisibleLeftSideTextLabel  { get => leftSideTextLabel .Visible; set => leftSideTextLabel .Visible = value; }

        public void ShowDialog_ColumnsVisibilityEditor( IEnumerable< DataGridViewColumn > dataGridColumns, IEnumerable< DataGridViewColumn > immutableDataGridColumns )
        {
            using ( var f = new ColumnsVisibilityEditor( dataGridColumns, immutableDataGridColumns ) )
            {
                if ( f.ShowDialog() == DialogResult.OK )
                {
                    Debug.WriteLine( "apply columns visibility" );
                    SettingsChanged?.Invoke( this, EventArgs.Empty );
                }
            }
        }
        public void ShowDialog_FileNameExcludesWordsEditor()
        {
            if ( FileNameExcludesWordsEditor.TryEdit( NameCleaner.ExcludesWords, _SC, out var resultExcludesWords ) )
            {
                _SC.Settings.ResetNameCleanerExcludesWords( NameCleaner.ResetExcludesWords( resultExcludesWords ) );
                _SC.SaveNoThrow_IfAnyChanged();
            }
        }
        public void ShowDialog_Settings( SettingsForm.TabPageKind? tabPageKind = default )
        {
            var st = GetSettings();
            using ( var f = new SettingsForm( _DC/*, _SC*/, _ReceivedAndWritedPartsProcessor, tabPageKind ) )
            {
                var f_p = f.Parallelism;
                f_p.MaxDegreeOfParallelism = st.MaxDegreeOfParallelism;
                f_p.ShareMaxDownloadThreadsBetweenAllDownloadsInstance = st.ShareMaxDownloadThreadsBetweenAllDownloadsInstance;
                f_p.SetMaxCrossDownloadInstance( st.MaxCrossDownloadInstance, st.MaxCrossDownloadInstanceSaved );
                f_p.SetMaxSpeedThresholdInMbps ( st.MaxSpeedThresholdInMbps , st.MaxSpeedThresholdInMbpsSaved  );

                var f_o = f.Other;
                f_o.AttemptRequestCountByPart              = st.AttemptRequestCountByPart;
                f_o.RequestTimeoutByPart                   = st.RequestTimeoutByPart;
                f_o.ShowOnlyRequestRowsWithErrors          = st.ShowOnlyRequestRowsWithErrors;
                f_o.ShowDownloadStatisticsInMainFormTitle  = st.ShowDownloadStatisticsInMainFormTitle;
                f_o.ShowAllDownloadsCompleted_Notification = st.ShowAllDownloadsCompleted_Notification;
                f_o.OutputFileExtension                    = st.OutputFileExtension;
                f_o.ExternalProgCaption                    = st.ExternalProgCaption;
                f_o.ExternalProgFilePath                   = st.ExternalProgFilePath;
                f_o.ExternalProgApplyByDefault             = st.ExternalProgApplyByDefault;
                f_o.FFmpegFileLocation                     = st.FFmpegFileLocation;
                f_o.FFmpegConverterCaption                 = st.FFmpegConverterCaption;
                f_o.FFmpegApplyByDefault                   = st.FFmpegApplyByDefault;
                f_o.FFmpegDegreeOfParallelism              = st.FFmpegDegreeOfParallelism;
                f_o.FFmpeg_RenameAfterFFmpegConverter      = st.FFmpeg_RenameAfterFFmpegConverter;
                f_o.FFmpeg_OpenAfterWithExternalProgRunner = st.FFmpeg_OpenAfterWithExternalProgRunner;
                f_o.UseDirectorySelectDialogModern         = st.UseDirectorySelectDialogModern;
                f_o.UniqueUrlsOnly                         = st.UniqueUrlsOnly;
                f_o.IgnoreHostHttpHeader                   = st.IgnoreHostHttpHeader;

                f.WebProxy.SetWebProxyInfo( _SC.GetDefaultWebProxyInfo() );

                if ( f.ShowDialog() == DialogResult.OK )
                {
                    //f_p = f.Parallelism;
                    st.MaxDegreeOfParallelism              = f_p.MaxDegreeOfParallelism;
                    st.ShareMaxDownloadThreadsBetweenAllDownloadsInstance 
                                                           = f_p.ShareMaxDownloadThreadsBetweenAllDownloadsInstance;
                    st.MaxCrossDownloadInstance            = f_p.MaxCrossDownloadInstance;
                    st.MaxCrossDownloadInstanceSaved       = f_p.MaxCrossDownloadInstanceSaved;
                    st.MaxSpeedThresholdInMbps             = f_p.MaxSpeedThresholdInMbps;
                    st.MaxSpeedThresholdInMbpsSaved        = f_p.MaxSpeedThresholdInMbpsSaved;

                    //f_o = f.Other;
                    st.AttemptRequestCountByPart              = f_o.AttemptRequestCountByPart;
                    st.RequestTimeoutByPart                   = f_o.RequestTimeoutByPart;
                    st.ShowOnlyRequestRowsWithErrors          = f_o.ShowOnlyRequestRowsWithErrors;
                    st.ShowDownloadStatisticsInMainFormTitle  = f_o.ShowDownloadStatisticsInMainFormTitle;
                    st.ShowAllDownloadsCompleted_Notification = f_o.ShowAllDownloadsCompleted_Notification;
                    st.OutputFileExtension                    = f_o.OutputFileExtension;
                    st.ExternalProgCaption                    = f_o.ExternalProgCaption;
                    st.ExternalProgFilePath                   = f_o.ExternalProgFilePath;
                    st.ExternalProgApplyByDefault             = f_o.ExternalProgApplyByDefault;
                    st.FFmpegConverterCaption                 = f_o.FFmpegConverterCaption;
                    st.FFmpegFileLocation                     = f_o.FFmpegFileLocation;
                    st.FFmpegApplyByDefault                   = f_o.FFmpegApplyByDefault;
                    st.FFmpegDegreeOfParallelism              = f_o.FFmpegDegreeOfParallelism;
                    st.FFmpeg_RenameAfterFFmpegConverter      = f_o.FFmpeg_RenameAfterFFmpegConverter;
                    st.FFmpeg_OpenAfterWithExternalProgRunner = f_o.FFmpeg_OpenAfterWithExternalProgRunner;
                    st.UseDirectorySelectDialogModern         = f_o.UseDirectorySelectDialogModern;
                    st.UniqueUrlsOnly                         = f_o.UniqueUrlsOnly;
                    st.IgnoreHostHttpHeader                   = f_o.IgnoreHostHttpHeader;

                    _SC.SetDefaultWebProxyInfo( f.WebProxy.GetWebProxyInfo() );

                    _SC.SaveNoThrow_IfAnyChanged();

                    SettingsChanged?.Invoke( this, EventArgs.Empty );
                }
            }
        }
        public void ShowDialog_ParallelismSettings() => ShowDialog_Settings( SettingsForm.TabPageKind.Parallelism );
        public void ShowDialog_OtherSettings() => ShowDialog_Settings( SettingsForm.TabPageKind.Other );
        public void ShowDialog_WebProxySettings() => ShowDialog_Settings( SettingsForm.TabPageKind.WebProxy );
        #endregion

        #region [.private methods.]
        private void SettingsController_PropertyChanged( Settings settings, string propertyName )
        {
            switch ( propertyName )
            {
                case nameof(Settings.AttemptRequestCountByPart):
                case nameof(Settings.RequestTimeoutByPart):
                    settingsLabel_set();
                    break;

                case nameof(Settings.ShareMaxDownloadThreadsBetweenAllDownloadsInstance):
                case nameof(Settings.MaxDegreeOfParallelism):
                case nameof(Settings.MaxCrossDownloadInstance):
                    parallelismLabel_set();
                    break;
            }
        }

        private void statusBarLabel_MouseEnter( object sender, EventArgs e )
        {
            if ( (this.Cursor == Cursors.Default) && ((ToolStripItem) sender).Enabled )
            {
                this.Cursor = Cursors.Hand;
            }
        }
        private void statusBarLabel_MouseLeave( object sender, EventArgs e )
        {
            if ( (this.Cursor == Cursors.Hand) && ((ToolStripItem) sender).Enabled )
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void parallelismLabel_Click( object sender, EventArgs e ) => ShowDialog_ParallelismSettings();
        private void parallelismLabel_EnabledChanged( object sender, EventArgs e )
        {
            if ( GetSettings().ShareMaxDownloadThreadsBetweenAllDownloadsInstance )
            {
#if NETCOREAPP
                parallelismLabel.BackColor = (parallelismLabel.Enabled ? Color.FromKnownColor( KnownColor.Control ) : Color.FromKnownColor( KnownColor.Control ));
#else
                parallelismLabel.BackColor = (parallelismLabel.Enabled ? Color.DimGray                              : Color.FromKnownColor( KnownColor.Control ));
#endif
            }
        }
        private void exceptionWordsLabel_Click( object sender, EventArgs e ) => ShowDialog_FileNameExcludesWordsEditor();
        private void settingsLabel_Click( object sender, EventArgs e ) => ShowDialog_OtherSettings();

        private void parallelismLabel_set()
        {
            var st = GetSettings();
            var shareMaxDownloadThreads  = st.ShareMaxDownloadThreadsBetweenAllDownloadsInstance;
            var maxCrossDownloadInstance = st.MaxCrossDownloadInstance;

            parallelismLabel.Text        = $"degree of parallelism:  {st.MaxDegreeOfParallelism} " +
                                           (maxCrossDownloadInstance.HasValue ? $"\r\ndownload-instances:  {maxCrossDownloadInstance.Value} " : null);
            parallelismLabel.ToolTipText = $"share \"max download threads\"\r\nbetween all downloads-instance:  {shareMaxDownloadThreads.ToString().ToLower()}";
#if NETCOREAPP
            parallelismLabel.ForeColor = (shareMaxDownloadThreads ? Color.FromKnownColor( KnownColor.ControlText ) : Color.DimGray);
            parallelismLabel.BackColor = (shareMaxDownloadThreads ? Color.FromKnownColor( KnownColor.Control     ) : Color.FromKnownColor( KnownColor.Control ));
#else
            parallelismLabel.ForeColor = (shareMaxDownloadThreads ? Color.White   : Color.FromKnownColor( KnownColor.ControlText ));
            parallelismLabel.BackColor = (shareMaxDownloadThreads ? Color.DimGray : Color.FromKnownColor( KnownColor.Control ));
#endif
            //--------------------------------------------//

            exceptionWordsLabel.Text = (maxCrossDownloadInstance.HasValue ? "file name exception\r\nword editor" : "file name exceptions");
        }
        private void settingsLabel_set()
        {
            var st = GetSettings();
            settingsLabel.ToolTipText = $"other settings =>\r\n attempt request count by part:  {st.AttemptRequestCountByPart}" +
                                        $"\r\n request timeout by part:  {st.RequestTimeoutByPart}";
        }
        #endregion
    }
}
