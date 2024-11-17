using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

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
        #endregion

        #region [.ctor().]
        public StatusBarUC()
        {
            InitializeComponent();
            //----------------------------------------//

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
                _DetachSettingsControllerAction?.Invoke();
                _DetachTrackItemsCountAction?.Invoke();
                _C2CTProcessor.Dispose();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.public.]
        private Settings GetSettings() => (_SC?.Settings ?? Settings.Default);

        public void SetDownloadController( _DC_ dc ) => _DC = dc;
        private Action _DetachSettingsControllerAction;
        public void SetSettingsController( _SC_ sc )
        {
            _DetachSettingsControllerAction?.Invoke();

            _SC = sc ?? throw (new ArgumentNullException( nameof(sc) ));
            _SC.SettingsPropertyChanged -= SettingsController_PropertyChanged;
            _SC.SettingsPropertyChanged += SettingsController_PropertyChanged;

            _DetachSettingsControllerAction = () =>
            {
                if ( _SC != null )
                {
                    _SC.SettingsPropertyChanged -= SettingsController_PropertyChanged;
                    _SC = null;
                }
            };
        }

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
            using var f = new ColumnsVisibilityEditor( dataGridColumns, immutableDataGridColumns );
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
        public void ShowDialog_Settings( SettingsForm.SettingsTabEnum? settingsTab = default )
        {
            var st = GetSettings();
            using ( var f = new SettingsForm( _DC, settingsTab ) )
            {
                f.Parallelism.MaxDegreeOfParallelism              = st.MaxDegreeOfParallelism;
                f.Parallelism.UseCrossDownloadInstanceParallelism = st.UseCrossDownloadInstanceParallelism;
                f.Parallelism.SetMaxCrossDownloadInstance( st.MaxCrossDownloadInstance, st.MaxCrossDownloadInstanceSaved );
                f.Parallelism.SetMaxSpeedThresholdInMbps ( st.MaxSpeedThresholdInMbps , st.MaxSpeedThresholdInMbpsSaved  );

                f.Other.AttemptRequestCountByPart              = st.AttemptRequestCountByPart;
                f.Other.RequestTimeoutByPart                   = st.RequestTimeoutByPart;
                f.Other.ShowOnlyRequestRowsWithErrors          = st.ShowOnlyRequestRowsWithErrors;
                f.Other.ShowDownloadStatisticsInMainFormTitle  = st.ShowDownloadStatisticsInMainFormTitle;
                f.Other.ShowAllDownloadsCompleted_Notification = st.ShowAllDownloadsCompleted_Notification;
                f.Other.OutputFileExtension                    = st.OutputFileExtension;
                f.Other.ExternalProgCaption                    = st.ExternalProgCaption;
                f.Other.ExternalProgFilePath                   = st.ExternalProgFilePath;
                f.Other.ExternalProgApplyByDefault             = st.ExternalProgApplyByDefault;
                f.Other.UseDirectorySelectDialogModern         = st.UseDirectorySelectDialogModern;
                f.Other.UniqueUrlsOnly                         = st.UniqueUrlsOnly;

                if ( f.ShowDialog() == DialogResult.OK )
                {
                    st.MaxDegreeOfParallelism              = f.Parallelism.MaxDegreeOfParallelism;
                    st.UseCrossDownloadInstanceParallelism = f.Parallelism.UseCrossDownloadInstanceParallelism;
                    st.MaxCrossDownloadInstance            = f.Parallelism.MaxCrossDownloadInstance;
                    st.MaxCrossDownloadInstanceSaved       = f.Parallelism.MaxCrossDownloadInstanceSaved;
                    st.MaxSpeedThresholdInMbps             = f.Parallelism.MaxSpeedThresholdInMbps;
                    st.MaxSpeedThresholdInMbpsSaved        = f.Parallelism.MaxSpeedThresholdInMbpsSaved;

                    st.AttemptRequestCountByPart              = f.Other.AttemptRequestCountByPart;
                    st.RequestTimeoutByPart                   = f.Other.RequestTimeoutByPart;
                    st.ShowOnlyRequestRowsWithErrors          = f.Other.ShowOnlyRequestRowsWithErrors;
                    st.ShowDownloadStatisticsInMainFormTitle  = f.Other.ShowDownloadStatisticsInMainFormTitle;
                    st.ShowAllDownloadsCompleted_Notification = f.Other.ShowAllDownloadsCompleted_Notification;
                    st.OutputFileExtension                    = f.Other.OutputFileExtension;
                    st.ExternalProgCaption                    = f.Other.ExternalProgCaption;
                    st.ExternalProgFilePath                   = f.Other.ExternalProgFilePath;
                    st.ExternalProgApplyByDefault             = f.Other.ExternalProgApplyByDefault;
                    st.UseDirectorySelectDialogModern         = f.Other.UseDirectorySelectDialogModern;
                    st.UniqueUrlsOnly                         = f.Other.UniqueUrlsOnly;
                    
                    if ( _SC == null )
                    {
                        parallelismLabel_set();
                        settingsLabel_set();
                    }
                    else
                    {
                        _SC.SaveNoThrow_IfAnyChanged();
                    }

                    SettingsChanged?.Invoke( this, EventArgs.Empty );
                }
            }
        }
        public void ShowDialog_ParallelismSettings() => ShowDialog_Settings( SettingsForm.SettingsTabEnum.Parallelism );
        public void ShowDialog_OtherSettings() => ShowDialog_Settings( SettingsForm.SettingsTabEnum.Other );
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

                case nameof(Settings.UseCrossDownloadInstanceParallelism):
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
            if ( GetSettings().UseCrossDownloadInstanceParallelism )
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
            parallelismLabel.Text        = $"degree of parallelism:  {st.MaxDegreeOfParallelism} " +
                                           (st.MaxCrossDownloadInstance.HasValue ? $"\r\ndownload-instances:  {st.MaxCrossDownloadInstance.Value} " : null);
            parallelismLabel.ToolTipText = $"use cross download-instance parallelism:  {st.UseCrossDownloadInstanceParallelism.ToString().ToLower()}";
#if NETCOREAPP
            parallelismLabel.ForeColor = (st.UseCrossDownloadInstanceParallelism ? Color.FromKnownColor( KnownColor.ControlText ) : Color.DimGray);
            parallelismLabel.BackColor = (st.UseCrossDownloadInstanceParallelism ? Color.FromKnownColor( KnownColor.Control     ) : Color.FromKnownColor( KnownColor.Control ));
#else
            parallelismLabel.ForeColor = (st.UseCrossDownloadInstanceParallelism ? Color.White   : Color.FromKnownColor( KnownColor.ControlText ));
            parallelismLabel.BackColor = (st.UseCrossDownloadInstanceParallelism ? Color.DimGray : Color.FromKnownColor( KnownColor.Control ));
#endif
            //--------------------------------------------//

            exceptionWordsLabel.Text = (st.MaxCrossDownloadInstance.HasValue ? "file name exception\r\nword editor" : "file name exceptions");
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
