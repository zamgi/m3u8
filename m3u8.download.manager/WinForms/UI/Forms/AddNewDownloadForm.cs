﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using m3u8.download.manager.ui.infrastructure;

using _DC_ = m3u8.download.manager.controllers.DownloadController;
using _SC_ = m3u8.download.manager.controllers.SettingsPropertyChangeController;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class AddNewDownloadForm : Form
    {
        #region [.fields.]
        private LogListModel      _Model;
        private bool              _DownloadLater;
        private _SC_              _SC;
        private Settings          _Settings;
        private DownloadListModel _DownloadListModel;
        private FileNameCleaner.Processor _FNCP;
        private bool _WasFocusSet2outputFileNameTextBoxAfterFirstChanges;
        private (int n, int total) _SeriesInfo;
        private string             _Initial_M3u8FileUrl;
        #endregion

        #region [.ctor().]
        private AddNewDownloadForm()
        {
            InitializeComponent();
            //----------------------------------------//            

            //---statusBarUC.IsVisibleParallelismLabel = false;
            logPanel.Visible = false;
            logPanel.VisibleChanged += logPanel_VisibleChanged;
            logUC.ShowResponseColumn = false;

            _FNCP = new FileNameCleaner.Processor( outputFileNameTextBox, () => this.OutputFileName, setOutputFileName );
        }
        public AddNewDownloadForm( _DC_ dc, _SC_ sc, string m3u8FileUrl, in (int n, int total)? seriesInfo = null ) : this()
        {
            _SC                = sc;
            _Settings          = sc.Settings;
            _DownloadListModel = dc?.Model;

            _Initial_M3u8FileUrl = m3u8FileUrl;
            this.M3u8FileUrl     = m3u8FileUrl;
            this.OutputDirectory = _Settings.OutputFileDirectory;
            _WasFocusSet2outputFileNameTextBoxAfterFirstChanges = m3u8FileUrl.IsNullOrWhiteSpace();

            _Model = new LogListModel();
            logUC.SetModel( _Model );
            logUC.SetSettingsController( sc );
            statusBarUC.SetDownloadController( dc );
            statusBarUC.SetSettingsController( sc );
            
            if ( seriesInfo.HasValue )
            {
                var x = seriesInfo.Value;
                this.Text += $" ({x.n} of {x.total})";
            }
            _SeriesInfo = seriesInfo.GetValueOrDefault( (1, 1) );
            
            this.LiveStreamMaxFileSizeInMb = _Settings.LiveStreamMaxSingleFileSizeInMb;
            //this.IsLiveStream              = _Settings.IsLiveStream;
            //if ( isLiveStreamCheckBox.Checked ) isLiveStreamCheckBox_Click( isLiveStreamCheckBox, EventArgs.Empty );
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                _FNCP.Dispose();
            }
            base.Dispose( disposing );
        }
        #endregion

        public static bool ShowModalDialog( IWin32Window owner, _DC_ dc, _SC_ sc, string m3u8FileUrl, in (int n, int total)? seriesInfo, Func< AddNewDownloadForm, Task > okAction )
        {
            using ( var f = new AddNewDownloadForm( dc, sc, m3u8FileUrl, seriesInfo ) )
            {
                if ( f.ShowDialog( owner ) == DialogResult.OK )
                {
                    okAction?.Invoke( f );
                    return (true);
                }
            }
            return (false);
        }
        public static void Show( IWin32Window owner, _DC_ dc, _SC_ sc, string m3u8FileUrl, in (int n, int total)? seriesInfo, Func< AddNewDownloadForm, Task > formClosedAction )
        {
            var f = new AddNewDownloadForm( dc, sc, m3u8FileUrl, seriesInfo );
            f.FormClosed += (_, _) => formClosedAction?.Invoke( f );
            f.downloadStartButton.Click += (_, _) => f.Close();
            f.downloadLaterButton.Click += (_, _) => f.Close();
            f.Show( owner );
        }

        #region [.TryGetOtherOpenedForm.]
        public static bool TryGetOpenedForm( out AddNewDownloadForm openedForm )
        {
            openedForm = Application.OpenForms.OfType< AddNewDownloadForm >().LastOrDefault();
            return (openedForm != null);
        }
        private bool TryGetOtherOpenedForm( out AddNewDownloadForm otherForm )
        {
            otherForm = Application.OpenForms.OfType< AddNewDownloadForm >().Where( f => f != this ).LastOrDefault();
            return (otherForm != null);
        }
        public void ActivateAfterCloseOther()
        {
            this.Activate();
            setFocus2outputFileNameTextBox();
        }
        #endregion

        #region [.override & private methods.]
        private void logPanel_VisibleChanged( object sender, EventArgs e )
        {
            var json = _Settings.AddNewDownloadFormPositionLogVisibleJson;
            if ( !json.IsNullOrEmpty() )
            {
                var saved_height = this.Height;
                FormPositionStorer.LoadOnlyHeight( this, json );
                if ( (this.Height - saved_height) < 50 )
                {
                    this.Height = saved_height + 250;
                }
            }
            else
            {
                this.Height += 250;
            }
        }
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !base.DesignMode )
            {
                var json = (logPanel.Visible ? _Settings.AddNewDownloadFormPositionLogVisibleJson : _Settings.AddNewDownloadFormPositionJson);
                FormPositionStorer.Load( this, json );

                if ( TryGetOtherOpenedForm( out var otherForm ) )
                {
                    if ( _SeriesInfo.n == _SeriesInfo.total )
                    {
                        this.Location = new Point( otherForm.Left + 10, otherForm.Top + 10 );
                    }
                    else if ( 1 < _SeriesInfo.total )
                    {
                        this.Location = otherForm.Location;
                    }

                    var rc = Screen.GetWorkingArea( this );
                    var x = this.Right  - rc.Width;
                    var y = this.Bottom - rc.Height;
                    if ( (0 < x) || (0 < y) )
                    {
                        this.Location = new Point( this.Left - Math.Max( 0, x ), otherForm.Top - Math.Max( 0, y ) );
                    }
                }
            }
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            if ( !base.DesignMode )
            {
                if ( logPanel.Visible )
                {
                    _Settings.AddNewDownloadFormPositionLogVisibleJson = FormPositionStorer.SaveOnlyPos( this );
                    _Settings.AddNewDownloadFormPositionJson           = FormPositionStorer.SaveOnlyPos( this, (this.Height - logPanel.Height) );
                }
                else
                {
                    _Settings.AddNewDownloadFormPositionJson = FormPositionStorer.SaveOnlyPos( this );
                }
                if ( DialogResult != DialogResult.Cancel )
                {
                    _Settings.OutputFileDirectory             = this.OutputDirectory;
                    _Settings.LiveStreamMaxSingleFileSizeInMb = this.LiveStreamMaxFileSizeInMb;
                    _Settings.IsLiveStream                    = this.IsLiveStream;
                }
                _SC.SaveNoThrow_IfAnyChanged();
            }
        }
        private IntPtr _LastForegroundWnd;
        protected override void OnShown( EventArgs e )
        {
            base.OnShown( e );

            _LastForegroundWnd = WinApi.GetForegroundWindow(); //WinApi.GetTopForegroundWindow();

            this.Activate();
            WinApi.SetForegroundWindow( this.Handle );
            WinApi.SetForceForegroundWindow( this.Handle, _LastForegroundWnd );
        }
        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );

            if ( this.IsWaitBannerShown() )
            {
                e.Cancel = true;
                return;
            }

            if ( (DialogResult == DialogResult.OK) || (DialogResult == DialogResult.Yes) )
            {
                if ( this.M3u8FileUrl.IsNullOrWhiteSpace() )
                {
                    e.Cancel = true;
                    m3u8FileUrlTextBox.FocusAndBlinkBackColor();
                }
                else
                if ( _Settings.UniqueUrlsOnly && (_DownloadListModel?.ContainsUrl( this.M3u8FileUrl )).GetValueOrDefault() )
                {
                    e.Cancel = true;
                    this.MessageBox_ShowError( $"Url already exists in list:\n '{this.M3u8FileUrl}'", this.Text );
                    m3u8FileUrlTextBox.FocusAndBlinkBackColor();
                }
                else
                if ( this.GetOutputFileName().IsNullOrWhiteSpace() )
                {
                    e.Cancel = true;
                    outputFileNameTextBox.FocusAndBlinkBackColor();
                }
                else
                if ( this.GetOutputDirectory().IsNullOrWhiteSpace() )
                {
                    e.Cancel = true;
                    outputDirectoryTextBox.FocusAndBlinkBackColor();
                }

                if ( !e.Cancel && (DialogResult == DialogResult.Yes) )
                {
                    _DownloadLater = true;
                    DialogResult   = DialogResult.OK;
                }
            }

            if ( e.Cancel )
            {
                if ( DialogResult != DialogResult.Cancel ) DialogResult = DialogResult.Cancel;
            }
            else if ( (_LastForegroundWnd != this.Handle) && !_Initial_M3u8FileUrl.IsNullOrEmpty() )
            {
                WinApi.SetForegroundWindow( _LastForegroundWnd );
            }
        }
        protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
        {
            switch ( keyData )
            {
                case Keys.Escape:
                    DialogResult = DialogResult.Cancel;
                    this.Close();
                    return (true);

                case Keys.Enter                 : //AutoStartDownload
                case (Keys.Enter | Keys.Shift)  : //DownloadLater
                case (Keys.Enter | Keys.Control): //DownloadLater
                case (Keys.Enter | Keys.Alt)    : //DownloadLater
                    if ( !(this.ActiveControl is Button button) || !button.Focused ) //if ( !m3u8FileTextContentLoadButton.Focused && !outputFileNameClearButton.Focused && !outputFileNameSelectButton.Focused )
                    {
                        DialogResult = (keyData == Keys.Enter) ? DialogResult.OK : DialogResult.Yes;
                        this.Close();
                        return (true);
                    }
                break;
            }
            return (base.ProcessCmdKey( ref msg, keyData ));
        }
        #endregion

        #region [.public methods.]
        public  bool   AutoStartDownload => !_DownloadLater;
        public  string M3u8FileUrl
        {
            get => m3u8FileUrlTextBox.Text.Trim();
            private set
            {
                value = value?.Trim();
                if ( m3u8FileUrlTextBox.Text.Trim() != value )
                {
                    m3u8FileUrlTextBox.Text = value;
                }
            }
        }
        public  string GetOutputFileName() => FileNameCleaner.GetOutputFileName( this.OutputFileName );
        public  string GetOutputDirectory() => this.OutputDirectory;
        public  bool IsLiveStream
        { 
            get => isLiveStreamCheckBox.Checked;
            set => isLiveStreamCheckBox.Checked = value;
        }
        public int   LiveStreamMaxFileSizeInMb
        {
            get => (int) liveStreamMaxSizeInMbNumUpDn.Value;
            set => liveStreamMaxSizeInMbNumUpDn.Value = Math.Max( liveStreamMaxSizeInMbNumUpDn.Minimum, Math.Min( liveStreamMaxSizeInMbNumUpDn.Maximum, value ) );
        }
        public long  LiveStreamMaxFileSizeInBytes
        {
            get => this.LiveStreamMaxFileSizeInMb << 20;
            set => this.LiveStreamMaxFileSizeInMb = (int) (value >> 20);
        }
        private string OutputFileName
        {
            get => outputFileNameTextBox.Text.Trim();
            set
            {
                value = value?.Trim();
                if ( outputFileNameTextBox.Text.Trim() != value )
                {
                    outputFileNameTextBox.TextChanged -= outputFileNameTextBox_TextChanged;
                    outputFileNameTextBox.Text = value;
                    outputFileNameTextBox.TextChanged += outputFileNameTextBox_TextChanged;
                }
            }
        }
        private string OutputDirectory
        {
            get => outputDirectoryTextBox.Text.Trim();
            set
            {
                value = value?.Trim();
                if ( outputDirectoryTextBox.Text.Trim() != value )
                {
                    outputDirectoryTextBox.Text = value;
                }
            }
        }

        public bool IsWaitBannerShown() => this.Controls.OfType< WaitBannerUC >().Any();
        #endregion

        #region [.text-boxes.]
        private const int TEXTBOX_MILLISECONDS_DELAY = 150;
        private string _Last_m3u8FileUrlText;
        private string _LastManualInputed_outputFileNameText;

        private void setFocus2outputFileNameTextBox()
        {
            if ( !_WasFocusSet2outputFileNameTextBoxAfterFirstChanges )
            {
                _WasFocusSet2outputFileNameTextBoxAfterFirstChanges = outputFileNameTextBox.Focus();
            }
        }
        private async void m3u8FileUrlTextBox_TextChanged( object sender, EventArgs e )
        {
            var m3u8FileUrlText = this.M3u8FileUrl;
            if ( (_Last_m3u8FileUrlText == m3u8FileUrlText) && !this.OutputFileName.IsNullOrWhiteSpace() )
            {
                return;
            }
            if ( !_LastManualInputed_outputFileNameText.IsNullOrWhiteSpace() )
            {
                return;
            }
            _Last_m3u8FileUrlText = m3u8FileUrlText;

            await FileNameCleaner.SetOutputFileNameByUrl_Async( m3u8FileUrlText, setOutputFileName, TEXTBOX_MILLISECONDS_DELAY );

            setFocus2outputFileNameTextBox();
        }

        private void setOutputFileName( string outputFileName ) => this.OutputFileName = outputFileName;
        private void outputFileNameTextBox_TextChanged( object sender, EventArgs e ) => _FNCP.FileNameTextBox_TextChanged( outputFileName => _LastManualInputed_outputFileNameText = outputFileName );

        private void outputFileNameClearButton_Click( object sender, EventArgs e )
        {
            this.OutputFileName = null;
            outputFileNameTextBox.Focus();
        }
        private void outputFileNameSelectButton_Click( object sender, EventArgs e )
        {
            using ( var sfd = new SaveFileDialog() { InitialDirectory = this.OutputDirectory,
                                                     DefaultExt       = _Settings.OutputFileExtension,
                                                     AddExtension     = true, } )
            {
                sfd.FileName = FileNameCleaner.GetOutputFileName( this.OutputFileName );
                if ( sfd.ShowDialog( this ) == DialogResult.OK )
                {
                    var outputFullFileName = sfd.FileName;
                    this.OutputFileName  = Path.GetFileName( outputFullFileName );
                    this.OutputDirectory = Path.GetDirectoryName( outputFullFileName );
                }
            }
        }
        private void outputDirectorySelectButton_Click( object sender, EventArgs e )
        {
            if ( DirectorySelectDialog.Show( this, this.OutputDirectory, "Select output directory", out var selectedPath ) )
            {
                this.OutputDirectory = selectedPath;
            }
        }

        private void isLiveStreamCheckBox_Click( object sender, EventArgs e )
        {
            var isLiveStream = this.IsLiveStream;

            isLiveStreamCheckBox.ForeColor = isLiveStream ? Color.FromArgb(70, 70, 70) : Color.Silver;
            liveStreamMaxSizeInMbNumUpDn.Visible = liveStreamMaxSizeInMbLabel.Visible = isLiveStream;

            const int DEFAULT_HEIGHT_isLiveStream    = 30;
            const int DEFAULT_HEIGHT_mainLayoutPanel = 60;
            const int DEFAULT_HEIGHT_this            = 255;

            mainLayoutPanel.Height =  DEFAULT_HEIGHT_mainLayoutPanel + (isLiveStream ? DEFAULT_HEIGHT_isLiveStream : 0);
            if ( isLiveStream ) this.Height = Math.Max( DEFAULT_HEIGHT_this + DEFAULT_HEIGHT_isLiveStream, this.Height );
        }
        #endregion

        #region [.loadM3u8FileContentButton.]
        private async void loadM3u8FileContentButton_Click( object sender, EventArgs e )
        {
            logPanel.Visible = true;

            _Model.Clear();
            #region [.url.]
            if ( !Extensions.TryGetM3u8FileUrl( this.M3u8FileUrl, out var x ) )
            {
                _Model.AddRequestErrorRow( x.error.ToString() );
                logUC.ClearSelection();
                logUC.AdjustRowsHeightAndColumnsWidthSprain();
                return;
            }
            #endregion

            this.SetEnabledAllChildControls( false );
            await Task.Delay( millisecondsDelay: 250 );

            using ( var cts = new CancellationTokenSource() )
            using ( WaitBannerUC.Create( this, cts, visibleDelayInMilliseconds: 1_500 ) )
            {
                var t = await _DC_.GetFileTextContent( x.m3u8FileUrl, _Settings.RequestTimeoutByPart, cts ); //all possible exceptions are thrown within inside
                if ( cts.IsCancellationRequested )
                {
                    ;
                }
                else if ( t.error != null )
                {
                    _Model.AddRequestErrorRow( t.error.ToString() );
                }
                else
                {
                    _Model.Output( in t.m3u8File );
                }

                logUC.ClearSelection();
                logUC.AdjustRowsHeightAndColumnsWidthSprain();
            }

            this.SetEnabledAllChildControls( true );
        }
        #endregion
    }
}
