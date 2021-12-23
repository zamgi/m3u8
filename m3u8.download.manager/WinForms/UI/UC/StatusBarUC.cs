using System;
using System.Drawing;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using m3u8.download.manager.infrastructure;
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
        #endregion

        #region [.ctor().]
        public StatusBarUC()
        {
            InitializeComponent();
            //----------------------------------------//

            _Settings = Settings.Default;

            LeftSideTextLabelText = null;
            parallelismLabel_set();
            settingsLabel_set();
        }
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                DetachSettingsController();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.public.]
        public void SetDownloadController( DownloadController dc ) => _DownloadController = dc;
        public void SetSettingsController( SettingsPropertyChangeController sc )
        {
            DetachSettingsController();

            _SettingsController = sc ?? throw (new ArgumentNullException( nameof(sc) ));
            _SettingsController.SettingsPropertyChanged -= SettingsController_PropertyChanged;
            _SettingsController.SettingsPropertyChanged += SettingsController_PropertyChanged;

        }
        private void DetachSettingsController()
        {
            if ( _SettingsController != null )
            {
                _SettingsController.SettingsPropertyChanged -= SettingsController_PropertyChanged;
                _SettingsController = null;
            }
        }

        public bool IsVisibleSettingsLabel      { get => settingsLabel      .Visible; set => settingsLabel      .Visible = value; }
        public bool IsVisibleParallelismLabel   { get => parallelismLabel   .Visible; set => parallelismLabel   .Visible = value; }
        public bool IsVisibleExcludesWordsLabel { get => exceptionWordsLabel.Visible; set => exceptionWordsLabel.Visible = value; }
        public string LeftSideTextLabelText     { get => leftSideTextLabel  .Text   ; set => leftSideTextLabel  .Text    = value; }
        //public bool IsVisibleLeftSideTextLabel  { get => leftSideTextLabel .Visible; set => leftSideTextLabel .Visible = value; }
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

        private void parallelismLabel_Click( object sender, EventArgs e )
        {
            using ( var f = new ParallelismForm( _DownloadController ) )
            {
                f.UseCrossDownloadInstanceParallelism = _Settings.UseCrossDownloadInstanceParallelism;
                f.MaxDegreeOfParallelism              = _Settings.MaxDegreeOfParallelism;
                f.SetMaxCrossDownloadInstance( _Settings.MaxCrossDownloadInstance, _Settings.MaxCrossDownloadInstanceSaved );
                if ( f.ShowDialog() == DialogResult.OK )
                {
                    _Settings.UseCrossDownloadInstanceParallelism = f.UseCrossDownloadInstanceParallelism;
                    _Settings.MaxDegreeOfParallelism              = f.MaxDegreeOfParallelism;
                    _Settings.MaxCrossDownloadInstance            = f.MaxCrossDownloadInstance;
                    _Settings.MaxCrossDownloadInstanceSaved       = f.MaxCrossDownloadInstanceSaved;
                    _Settings.SaveNoThrow();
                    if ( _SettingsController == null )
                    {
                        parallelismLabel_set();
                    }

                    SettingsChanged?.Invoke( this, EventArgs.Empty );
                }
            }
        }
        private void parallelismLabel_EnabledChanged( object sender, EventArgs e )
        {
            if ( _Settings.UseCrossDownloadInstanceParallelism )
            {
                parallelismLabel.BackColor = (parallelismLabel.Enabled ? Color.DimGray : Color.FromKnownColor( KnownColor.Control ));
            }
        }
        private void exceptionWordsLabel_Click( object sender, EventArgs e )
        {
            using ( var f = new FileNameExcludesWordsEditor( NameCleaner.ExcludesWords ) )
            {
                if ( f.ShowDialog() == DialogResult.OK )
                {
                    NameCleaner.ResetExcludesWords( f.GetFileNameExcludesWords() );
                    _Settings.ResetNameCleanerExcludesWords( NameCleaner.ExcludesWords );
                    _Settings.SaveNoThrow();
                }
            }
        }
        private void settingsLabel_Click( object sender, EventArgs e )
        {
            using ( var f = new SettingsForm( _DownloadController ) )
            {
                f.AttemptRequestCountByPart             = _Settings.AttemptRequestCountByPart;
                f.RequestTimeoutByPart                  = _Settings.RequestTimeoutByPart;
                f.ShowOnlyRequestRowsWithErrors         = _Settings.ShowOnlyRequestRowsWithErrors;
                f.UniqueUrlsOnly                        = _Settings.UniqueUrlsOnly;
                f.ShowDownloadStatisticsInMainFormTitle = _Settings.ShowDownloadStatisticsInMainFormTitle;
                f.OutputFileExtension                   = _Settings.OutputFileExtension;

                if ( f.ShowDialog() == DialogResult.OK )
                {
                    _Settings.AttemptRequestCountByPart             = f.AttemptRequestCountByPart;
                    _Settings.RequestTimeoutByPart                  = f.RequestTimeoutByPart;
                    _Settings.ShowOnlyRequestRowsWithErrors         = f.ShowOnlyRequestRowsWithErrors;
                    _Settings.UniqueUrlsOnly                        = f.UniqueUrlsOnly;
                    _Settings.ShowDownloadStatisticsInMainFormTitle = f.ShowDownloadStatisticsInMainFormTitle;
                    _Settings.OutputFileExtension                   = f.OutputFileExtension;
                    _Settings.SaveNoThrow();
                    if ( _SettingsController == null )
                    {
                        settingsLabel_set();
                    }

                    SettingsChanged?.Invoke( this, EventArgs.Empty );
                }
            }
        }

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
