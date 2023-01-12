using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class StatusBarUC : UserControl
    {
        #region [.fields.]
        public event EventHandler SettingsChanged;

        private DownloadController               _DownloadController;
        private SettingsPropertyChangeController _SettingsController;
        private Settings                         _Settings;
        private ToolStripStatusLabelEx.Color2ColorTransitionProcessor _C2CTProcessor;
        #endregion

        #region [.ctor().]
        public StatusBarUC()
        {
            InitializeComponent();
            //----------------------------------------//

            _Settings = Settings.Default;

            //LeftSideTextLabelText = null;
            parallelismLabel_set();
            settingsLabel_set();

            _C2CTProcessor = new ToolStripStatusLabelEx.Color2ColorTransitionProcessor( leftSideTextLabel_2 );
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
        public void SetDownloadController( DownloadController dc ) => _DownloadController = dc;
        private Action _DetachSettingsControllerAction;
        public void SetSettingsController( SettingsPropertyChangeController sc )
        {
            _DetachSettingsControllerAction?.Invoke();

            _SettingsController = sc ?? throw (new ArgumentNullException( nameof(sc) ));
            _SettingsController.SettingsPropertyChanged -= SettingsController_PropertyChanged;
            _SettingsController.SettingsPropertyChanged += SettingsController_PropertyChanged;

            _DetachSettingsControllerAction = () =>
            {
                if ( _SettingsController != null )
                {
                    _SettingsController.SettingsPropertyChanged -= SettingsController_PropertyChanged;
                    _SettingsController = null;
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
                }
            }
        }
        public void ShowDialog_FileNameExcludesWordsEditor()
        {
            if ( FileNameExcludesWordsEditor.TryEdit( NameCleaner.ExcludesWords, out var resultExcludesWords ) )
            {
                _Settings.ResetNameCleanerExcludesWords( NameCleaner.ResetExcludesWords( resultExcludesWords ) );
                _Settings.SaveNoThrow();
            }
        }
        public void ShowDialog_Settings( SettingsForm.SettingsTabEnum? settingsTab = default )
        {
            using ( var f = new SettingsForm( _DownloadController, settingsTab ) )
            {
                f.Parallelism.MaxDegreeOfParallelism              = _Settings.MaxDegreeOfParallelism;                
                f.Parallelism.UseCrossDownloadInstanceParallelism = _Settings.UseCrossDownloadInstanceParallelism;                
                f.Parallelism.SetMaxCrossDownloadInstance( _Settings.MaxCrossDownloadInstance, _Settings.MaxCrossDownloadInstanceSaved );
                f.Parallelism.MaxSpeedThresholdInMbps             = _Settings.MaxSpeedThresholdInMbps;

                f.Other.AttemptRequestCountByPart              = _Settings.AttemptRequestCountByPart;
                f.Other.RequestTimeoutByPart                   = _Settings.RequestTimeoutByPart;
                f.Other.ShowOnlyRequestRowsWithErrors          = _Settings.ShowOnlyRequestRowsWithErrors;
                f.Other.ShowDownloadStatisticsInMainFormTitle  = _Settings.ShowDownloadStatisticsInMainFormTitle;
                f.Other.ShowAllDownloadsCompleted_Notification = _Settings.ShowAllDownloadsCompleted_Notification;
                f.Other.OutputFileExtension                    = _Settings.OutputFileExtension;
                f.Other.ExternalProgCaption                    = _Settings.ExternalProgCaption;
                f.Other.ExternalProgFilePath                   = _Settings.ExternalProgFilePath;
                f.Other.ExternalProgApplyByDefault             = _Settings.ExternalProgApplyByDefault;
                f.Other.UseDirectorySelectDialogModern         = _Settings.UseDirectorySelectDialogModern;
                f.Other.UniqueUrlsOnly                         = _Settings.UniqueUrlsOnly;

                if ( f.ShowDialog() == DialogResult.OK )
                {
                    _Settings.MaxDegreeOfParallelism              = f.Parallelism.MaxDegreeOfParallelism;
                    _Settings.UseCrossDownloadInstanceParallelism = f.Parallelism.UseCrossDownloadInstanceParallelism;
                    _Settings.MaxCrossDownloadInstance            = f.Parallelism.MaxCrossDownloadInstance;
                    _Settings.MaxCrossDownloadInstanceSaved       = f.Parallelism.MaxCrossDownloadInstanceSaved;
                    _Settings.MaxSpeedThresholdInMbps             = f.Parallelism.MaxSpeedThresholdInMbps;

                    _Settings.AttemptRequestCountByPart              = f.Other.AttemptRequestCountByPart;
                    _Settings.RequestTimeoutByPart                   = f.Other.RequestTimeoutByPart;
                    _Settings.ShowOnlyRequestRowsWithErrors          = f.Other.ShowOnlyRequestRowsWithErrors;
                    _Settings.ShowDownloadStatisticsInMainFormTitle  = f.Other.ShowDownloadStatisticsInMainFormTitle;
                    _Settings.ShowAllDownloadsCompleted_Notification = f.Other.ShowAllDownloadsCompleted_Notification;
                    _Settings.OutputFileExtension                    = f.Other.OutputFileExtension;
                    _Settings.ExternalProgCaption                    = f.Other.ExternalProgCaption;
                    _Settings.ExternalProgFilePath                   = f.Other.ExternalProgFilePath;
                    _Settings.ExternalProgApplyByDefault             = f.Other.ExternalProgApplyByDefault;
                    _Settings.UseDirectorySelectDialogModern         = f.Other.UseDirectorySelectDialogModern;
                    _Settings.UniqueUrlsOnly                         = f.Other.UniqueUrlsOnly;

                    _Settings.SaveNoThrow();
                    if ( _SettingsController == null )
                    {
                        parallelismLabel_set();
                        settingsLabel_set();
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
            if ( _Settings.UseCrossDownloadInstanceParallelism )
            {
                parallelismLabel.BackColor = (parallelismLabel.Enabled ? Color.DimGray : Color.FromKnownColor( KnownColor.Control ));
            }
        }
        private void exceptionWordsLabel_Click( object sender, EventArgs e ) => ShowDialog_FileNameExcludesWordsEditor();
        private void settingsLabel_Click( object sender, EventArgs e ) => ShowDialog_OtherSettings();

        private void parallelismLabel_set()
        {
            parallelismLabel.Text        = $"degree of parallelism:  {_Settings.MaxDegreeOfParallelism} " +
                                           (_Settings.MaxCrossDownloadInstance.HasValue ? $"\r\ndownload-instances:  {_Settings.MaxCrossDownloadInstance.Value} " : null);
            parallelismLabel.ToolTipText = $"use cross download-instance parallelism:  {_Settings.UseCrossDownloadInstanceParallelism.ToString().ToLower()}";

            parallelismLabel.ForeColor   = (_Settings.UseCrossDownloadInstanceParallelism ? Color.White   : Color.FromKnownColor( KnownColor.ControlText ));
            parallelismLabel.BackColor   = (_Settings.UseCrossDownloadInstanceParallelism ? Color.DimGray : Color.FromKnownColor( KnownColor.Control     ));
            //--------------------------------------------//

            exceptionWordsLabel.Text = (_Settings.MaxCrossDownloadInstance.HasValue ? "file name exception\r\nword editor" : "file name exceptions");
        }
        private void settingsLabel_set() => settingsLabel.ToolTipText = $"other settings =>\r\n attempt request count by part:  {_Settings.AttemptRequestCountByPart}" + 
                                                                                         $"\r\n request timeout by part:  {_Settings.RequestTimeoutByPart}";
        #endregion
    }
}
