using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
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
    internal sealed partial class ChangeSettingsParams4DownloadRowForm : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public enum TabPageKind
        {
            MainTabPage,
            RequestHeadersTabPage,
            WebProxyTabPage,
        }

        #region [.fields.]
        private LogListModel      _Model;
        private _DC_              _DC;
        private _SC_              _SC;
        private Settings          _Settings;
        private FileNameCleaner4UI.Processor _FNCP;
        private bool              _WasFocusSet2outputFileNameTextBoxAfterFirstChanges;
        private OutputFileNamePatternProcessor _OutputFileNamePatternProcessor;
        private Func< AddNewDownloadForm, Task > _Transitive_FormClosedAction_When_DownloadAdditionalM3u8Url;
        #endregion

        #region [.ctor().]
        private ChangeSettingsParams4DownloadRowForm( _DC_ dc, _SC_ sc )
        {
            _DC       = dc;
            _SC       = sc;
            _Settings = sc.Settings;

            InitializeComponent( dc, sc );
            //----------------------------------------//

            logPanel.Visible = false;
            logUC.ShowResponseColumn = false;

            _FNCP = new FileNameCleaner4UI.Processor( outputFileNameTextBox, () => this.OutputFileName, setOutputFileName );

            #region [.ImageList 4 tabControl.]
            var imgLst = tabControl.ImageList = new ImageList() { ImageSize = new Size(16, 16) };
            imgLst.Images.Add( Resources.m3u8_32x36      ); mainTabPage          .ImageIndex = 0;
            imgLst.Images.Add( Resources.listcheck       ); requestHeadersTabPage.ImageIndex = 1;
            imgLst.Images.Add( Resources.workgroup_16x16 ); webProxyTabPage      .ImageIndex = 2;
            #endregion
        }

        /// <summary>
        /// Edit
        /// </summary>
        private ChangeSettingsParams4DownloadRowForm( _DC_ dc, _SC_ sc
            , DownloadRow row
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor ) : this( dc, sc )
        {
            var close = new EventHandler( (_, _) => this.Close() );
            this.okButton    .Click += close;
            this.cancelButton.Click += close;

            //_DownloadListModel = dc?.Model;
            requestHeadersEditor.SetRequestHeaders( row.RequestHeaders, sc.IgnoreHostHttpHeader );

            this.OutputFileName               = row.OutputFileName;
            this.OutputDirectory              = row.OutputDirectory;
            this.IsLiveStream                 = row.IsLiveStream; 
            this.LiveStreamMaxFileSizeInBytes = row.LiveStreamMaxFileSizeInBytes;
            (var timeout, attemptRequestCountByPartNUD.ValueAsInt32) = sc.GetCreateM3u8ClientParams();
            requestTimeoutByPartDTP.Value = requestTimeoutByPartDTP.MinDate.Date + timeout;

            requestTimeoutByPartDTP     .ValueChanged += requestTimeoutByPartDTP_ValueChanged;
            attemptRequestCountByPartNUD.ValueChanged += attemptRequestCountByPartNUD_ValueChanged;

            if ( row.Timeout            .HasValue ) requestTimeoutByPartDTP     .Value        = requestTimeoutByPartDTP.MinDate.Date + row.Timeout.Value;
            if ( row.AttemptRequestCount.HasValue ) attemptRequestCountByPartNUD.ValueAsInt32 = row.AttemptRequestCount.Value;

            _OutputFileNamePatternProcessor = outputFileNamePatternProcessor;

            #region [.if setted outputFileName.]
            //before 'this.M3u8FileUrl = m3u8FileUrl;'
            Process_use_OutputFileNamePatternProcessor_on_Init();
            #endregion

            m3u8FileUrlTextBox.TextChanged -= m3u8FileUrlTextBox_TextChanged;
            this.M3u8FileUrl = row.Url;
            m3u8FileUrlTextBox.TextChanged += m3u8FileUrlTextBox_TextChanged;
            _WasFocusSet2outputFileNameTextBoxAfterFirstChanges = row.Url.IsNullOrWhiteSpace();

            _Model = new LogListModel();
            logUC.SetModel( _Model );

            set_WebProxyInfo( row.WebProxyInfo );
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

        #region [.static show-form methods.]
        public static void Edit( IWin32Window owner, _DC_ dc, _SC_ sc
            , DownloadRow row
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor
            //, Action< FormClosingEventArgs > formClosingAction
            , Action< ChangeSettingsParams4DownloadRowForm, DownloadRow > formClosedAction
            , Func< AddNewDownloadForm, Task > formClosedAction_4_DownloadAdditionalM3u8Url
            , TabPageKind? activeTabPageKind = null )
        {
            var f = new ChangeSettingsParams4DownloadRowForm( dc, sc, row, outputFileNamePatternProcessor ) 
            { 
                Icon = Resources.edit.CreateSafeIcon(),
                Text = $"Change settings, / '{row.OutputFileName}' /",
                _Transitive_FormClosedAction_When_DownloadAdditionalM3u8Url = formClosedAction_4_DownloadAdditionalM3u8Url,
            };

            f.SetActiveTabPageKind( activeTabPageKind );
            //f.FormClosing += (_, e) => formClosingAction?.Invoke( e );
            f.FormClosed  += (_, _) => formClosedAction?.Invoke( f, row );
            f.Shown += (_, _) => f.setFocus2outputFileNameTextBox_Core();
            f.Show( owner );
        }
        private void SetActiveTabPageKind( TabPageKind? tabPageKind )
        {
            if ( tabPageKind.HasValue )
            {
                switch ( tabPageKind )
                {
                    case TabPageKind.MainTabPage          : tabControl.SelectedTab = mainTabPage;           break;
                    case TabPageKind.RequestHeadersTabPage: tabControl.SelectedTab = requestHeadersTabPage; break;
                    case TabPageKind.WebProxyTabPage      : tabControl.SelectedTab = webProxyTabPage;       break;
                }
            }
        }
        #endregion

        #region [.TryGetOtherOpenedForm.]
        public static bool TryGetOpenedForm( out ChangeSettingsParams4DownloadRowForm openedForm )
        {
            openedForm = Application.OpenForms.OfType< ChangeSettingsParams4DownloadRowForm >().LastOrDefault();
            return (openedForm != null);
        }
        private bool TryGetOtherOpenedForm( out ChangeSettingsParams4DownloadRowForm otherForm )
        {
            otherForm = Application.OpenForms.OfType< ChangeSettingsParams4DownloadRowForm >().Where( f => f != this ).LastOrDefault();
            return (otherForm != null);
        }
        public void ActivateAfterCloseOther()
        {
            this.Activate();
            setFocus2outputFileNameTextBox();
        }
        #endregion

        #region [.override & private methods.]
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !base.DesignMode )
            {
                var json = (logPanel.Visible ? _Settings.ChangeSettingsParams4DownloadRowForm_PosLogVisibleJson : _Settings.ChangeSettingsParams4DownloadRowForm_PosJson);
                FormPositionStorer.Load( this, json );

                if ( this.IsLiveStream ) isLiveStreamCheckBox_Click( isLiveStreamCheckBox, EventArgs.Empty );

                if ( TryGetOtherOpenedForm( out var otherForm ) )
                {
                    //if ( _SeriesInfo.n == _SeriesInfo.total )
                    //{
                    //    this.Location = new Point( otherForm.Left + 10, otherForm.Top + 10 );
                    //}
                    //else if ( 1 < _SeriesInfo.total )
                    //{
                    //    this.Location = otherForm.Location;
                    //}

                    var rc = Screen.GetWorkingArea( this );
                    var x = this.Right  - rc.Width;
                    var y = this.Bottom - rc.Height;
                    if ( (0 < x) || (0 < y) )
                    {
                        this.Location = new Point( this.Left - Math.Max( 0, x ), otherForm.Top - Math.Max( 0, y ) );
                    }
                }
            }            

            #region [.track last active focused control for each tab-page.]            
            tabControl.TabPages.Cast< TabPage >().ForEach( p => BindFocusTracking( p, p ) );
            #endregion
        }
        protected override void OnFormClosed( FormClosedEventArgs e )
        {
            base.OnFormClosed( e );

            if ( !base.DesignMode )
            {
                if ( logPanel.Visible )
                {
                    _Settings.ChangeSettingsParams4DownloadRowForm_PosLogVisibleJson = FormPositionStorer.SaveOnlyPos( this );
                    _Settings.ChangeSettingsParams4DownloadRowForm_PosJson           = FormPositionStorer.SaveOnlyPos( this, (this.Height - logPanel.Height) );
                }
                else
                {
                    _Settings.ChangeSettingsParams4DownloadRowForm_PosJson = FormPositionStorer.SaveOnlyPos( this );
                }
                if ( DialogResult != DialogResult.Cancel )
                {
                    _Settings.OutputFileDirectory             = this.OutputDirectory;
                    _Settings.LiveStreamMaxSingleFileSizeInMb = this.LiveStreamMaxFileSizeInMb;
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

        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            base.OnFormClosing( e );

            if ( this.IsWaitBannerShown() )
            {
                e.Cancel = true;
                return;
            }

            if ( DialogResult == DialogResult.OK )
            {
                if ( this.GetOutputFileName_Internal().IsNullOrWhiteSpace() )
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
            }

            if ( !e.Cancel && (_LastForegroundWnd != this.Handle) )
            {
                WinApi.SetForegroundWindow( _LastForegroundWnd );
            }
        }
        protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
        {
            switch ( keyData )
            {
                case Keys.Escape:
                    if ( mainTabPage.IsSelected() || !requestHeadersEditor.InEditMode )
                    {
                        DialogResult = DialogResult.Cancel;
                        this.Close();
                        return (true);
                    }
                    break;

                case Keys.Enter                 : //AutoStartDownload
                case (Keys.Enter | Keys.Shift)  : //DownloadLater
                case (Keys.Enter | Keys.Control): //DownloadLater
                case (Keys.Enter | Keys.Alt)    : //DownloadLater
                    if ( mainTabPage.IsSelected() /*!_IsInEditMode*/ && (!(this.ActiveControl is Button button) || !button.Focused) ) //if ( !m3u8FileTextContentLoadButton.Focused && !outputFileNameClearButton.Focused && !outputFileNameSelectButton.Focused )
                    {
                        DialogResult = DialogResult.OK;
                        this.Close();
                        return (true);
                    }
                break;
            }
            return (base.ProcessCmdKey( ref msg, keyData ));
        }
        #endregion

        #region [.public methods.]
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
        public  string GetOutputFileName()
        {
            var outputFileName_1 = GetOutputFileName_Internal();
            var outputFileName_2 = _OutputFileNamePatternProcessor.Process( outputFileName_1 );
            return (outputFileName_2);
        }
        private string GetOutputFileName_Internal() => FileNameCleaner4UI.GetOutputFileName( this.OutputFileName, _Settings.OutputFileExtension, _OutputFileNamePatternProcessor.PatternChar );
        public  string GetOutputDirectory() => this.OutputDirectory;
        public  IDictionary< string, string > GetRequestHeaders() => requestHeadersEditor.GetRequestHeaders();
        public web_proxy_info GetWebProxyInfo() => webProxyUC.GetWebProxyInfo();
        public  bool  IsLiveStream
        { 
            get => isLiveStreamCheckBox.Checked;
            private set => isLiveStreamCheckBox.Checked = isLiveStreamCheckBox.Visible = value;
        }
        public int    LiveStreamMaxFileSizeInMb
        {
            get => (int) liveStreamMaxSizeInMbNumUpDn.Value;
            set => liveStreamMaxSizeInMbNumUpDn.Value = Math.Max( liveStreamMaxSizeInMbNumUpDn.Minimum, Math.Min( liveStreamMaxSizeInMbNumUpDn.Maximum, value ) );
        }
        public long   LiveStreamMaxFileSizeInBytes
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

        public TimeSpan? Timeout             { get; set; }
        public int?      AttemptRequestCount { get; set; }


        public (IDictionary< string, string > RequestHeaders, web_proxy_info WebProxyInfo, 
                string OutputFileName, string OutputDirectory, long LiveStreamMaxFileSizeInBytes,
                TimeSpan? Timeout, int? AttemptRequestCount) 
            GetParamsTuple() => (/*this.M3u8FileUrl,*/ this.GetRequestHeaders(), this.GetWebProxyInfo(),
                                 this.GetOutputFileName(), this.GetOutputDirectory(),
                                 /*this.IsLiveStream,*/ this.LiveStreamMaxFileSizeInBytes, 
                                 this.Timeout, this.AttemptRequestCount);

        public bool IsWaitBannerShown() => this.Controls.OfType< WaitBannerUC >().Any();
        #endregion

        #region [.text-boxes & etc.]
        private const int TEXTBOX_MILLISECONDS_DELAY = 150;
        private string _Last_m3u8FileUrlText;
        private string _LastManualInputed_outputFileNameText;

        private bool setFocus2outputFileNameTextBox_Core( string outputFileName = null )
        {
            var suc = outputFileNameTextBox.Focus();
            var i = (outputFileName ?? outputFileNameTextBox.Text).LastIndexOf( '.' );
            if ( i != -1 )
            {
                outputFileNameTextBox.SelectionStart  = i;
                outputFileNameTextBox.SelectionLength = 0;
            }
            return (suc);
        }
        private void setFocus2outputFileNameTextBox()
        {
            if ( !_WasFocusSet2outputFileNameTextBoxAfterFirstChanges )
            {
                _WasFocusSet2outputFileNameTextBoxAfterFirstChanges = setFocus2outputFileNameTextBox_Core();
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

            await FileNameCleaner4UI.SetOutputFileNameByUrl_Async( m3u8FileUrlText, _Settings.OutputFileExtension, setOutputFileName, TEXTBOX_MILLISECONDS_DELAY );

            setFocus2outputFileNameTextBox();
        }

        private void setOutputFileName( string outputFileName ) => this.OutputFileName = outputFileName;
        private void outputFileNameTextBox_TextChanged( object sender, EventArgs e )
        {
            _FNCP.FileNameTextBox_TextChanged( outputFileName => _LastManualInputed_outputFileNameText = outputFileName );

            //Process_use_OutputFileNamePatternProcessor();
        }
        private void outputFileNameTextBox_ClearButtonClick( object sender, EventArgs e ) => outputFileNameTextBox.Focus();
        private void outputFileNameSelectButton_Click( object sender, EventArgs e )
        {
            using ( var sfd = new SaveFileDialog() { InitialDirectory = this.OutputDirectory,
                                                     DefaultExt       = _Settings.OutputFileExtension,
                                                     AddExtension     = true, } )
            {
                sfd.FileName = FileNameCleaner4UI.GetOutputFileName( this.OutputFileName, _Settings.OutputFileExtension );
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
            if ( DirectorySelectDialog.Show( this, _Settings.UseDirectorySelectDialogModern, this.OutputDirectory, "Select output directory", out var selectedPath ) )
            {
                this.OutputDirectory = selectedPath;
            }
        }

        private void isLiveStreamCheckBox_Click( object sender, EventArgs e )
        {
            var isLiveStream = this.IsLiveStream;

            isLiveStreamCheckBox.ForeColor = isLiveStream ? Color.FromArgb(70, 70, 70) : Color.Silver;
            liveStreamMaxSizeInMbNumUpDn.Visible = liveStreamMaxSizeInMbLabel.Visible = isLiveStream;

            set_mainLayoutPanel_Height();
        }
        private void set_mainLayoutPanel_Height( bool? isLiveStream_or_patternOutputFileName_visible = null )
        {
            const int DEFAULT_HEIGHT_isLiveStream    = 30;
            const int DEFAULT_HEIGHT_mainLayoutPanel = 90 + 20;
            const int DEFAULT_HEIGHT_this            = 335 + 20 + 10;

            var is_extra_visible = isLiveStream_or_patternOutputFileName_visible.GetValueOrDefault( this.IsLiveStream /*|| patternOutputFileNameLabel.Visible*/ );
            var extra_height = is_extra_visible ? DEFAULT_HEIGHT_isLiveStream : 0;

            //this.SuspendDrawing();
            //this.SuspendLayout();
            mainLayoutPanel.SuspendDrawing();
            {
                mainLayoutPanel.Height = DEFAULT_HEIGHT_mainLayoutPanel + extra_height;
                if ( this.Height <= DEFAULT_HEIGHT_this )
                {
                    this.Height = DEFAULT_HEIGHT_this + (is_extra_visible ? extra_height : 0);
                }
                //if ( is_extra_visible && (this.Height <= DEFAULT_HEIGHT_this) ) this.Height += extra_height;
            }
            mainLayoutPanel.ResumeDrawing();
            //this.ResumeLayout(true);
            //this.ResumeDrawing();
        }

        private void Process_use_OutputFileNamePatternProcessor_on_Init()
        {
            //TEMP
#if DEBUG
            //if ( !_Initial_M3u8FileUrl.IsNullOrWhiteSpace() )
            //{
            //    m3u8FileUrlTextBox.TextChanged -= m3u8FileUrlTextBox_TextChanged;
            //    var shown = default(EventHandler);
            //    shown = (_, _) =>
            //    {
            //        const string txt = "Last_OutputFileName_Num - **.txt";
            //        this.OutputFileName = txt;
            //        setFocus2outputFileNameTextBox_Core( txt );
            //        outputFileNameTextBox_TextChanged( this, EventArgs.Empty );
            //        this.Shown -= shown;
            //    };
            //    this.Shown += shown;
            //}
#endif
        }
        //private void Process_use_OutputFileNamePatternProcessor()
        //{
        //    var outputFileName = this.OutputFileName;
        //    var has = _OutputFileNamePatternProcessor.HasPatternChar( outputFileName );
        //    if ( has )
        //    {
        //        Set_patternOutputFileNameLabel( outputFileName );
        //        if ( _OutputFileNamePatternProcessor.IsEqualPattern( outputFileName ) )
        //        {
        //            patternOutputFileNameNumUpDn.ValueAsInt32 = _OutputFileNamePatternProcessor.Last_OutputFileName_Num;
        //            patternOutputFileNameNumUpDn_ValueChanged( null, null );
        //        }
        //        else
        //        {
        //            patternOutputFileNameNumUpDn.ValueAsInt32 = 1;
        //            _OutputFileNamePatternProcessor.Set_Last_OutputFileName_Num( patternOutputFileNameNumUpDn.ValueAsInt32 );
        //        }
        //    }
        //    patternOutputFileNameLabelCaption.Visible = patternOutputFileNameLabel.Visible = patternOutputFileNameNumUpDn.Visible = has;
        //    set_mainLayoutPanel_Height();
        //}
        //private void patternOutputFileNameNumUpDn_ValueChanged( object sender, EventArgs e )
        //{
        //    _OutputFileNamePatternProcessor.Set_Last_OutputFileName_Num( patternOutputFileNameNumUpDn.ValueAsInt32 );

        //    if ( _OutputFileNamePatternProcessor.HasLast_OutputFileName_As_Pattern && 
        //         _OutputFileNamePatternProcessor.IsEqualPattern( this.OutputFileName ) 
        //       )
        //    {
        //        this.OutputFileName = _OutputFileNamePatternProcessor.Get_Patterned_Last_OutputFileName();
        //    }
        //}
        //private void patternOutputFileNameLabel_VisibleChanged( object sender, EventArgs e ) => mainLayoutPanel.ColumnStyles[ 1 ].SizeType = patternOutputFileNameLabel.Visible ? SizeType.AutoSize : SizeType.Absolute;
        //private void Set_patternOutputFileNameLabel( string outputFileName )
        //{
        //    patternOutputFileNameLabel.Text = outputFileName;
        //    toolTip.SetToolTip( patternOutputFileNameLabel, outputFileName );
        //}

        private void requestTimeoutByPartDTP_ValueChanged( object sender, EventArgs e ) => this.Timeout = requestTimeoutByPartDTP.Value.TimeOfDay;
        private void attemptRequestCountByPartNUD_ValueChanged( object sender, EventArgs e ) => this.AttemptRequestCount = attemptRequestCountByPartNUD.ValueAsInt32;
        #endregion

        #region [.loadM3u8FileContentButton.]
        private async void loadM3u8FileContentButton_Click( object sender, EventArgs e )
        {
            if ( !logPanel.Visible )
            {
                logPanel.Visible = true;
                SetLayout_4_logPanel();
            }

            _Model.Clear();

            #region [.url.]
            if ( !UrlHelper.TryGetM3u8FileUrl( this.M3u8FileUrl, out var x ) )
            {
                _Model.AddRequestErrorRow( x.error.ToString() );
                logUC.ClearSelection();
                logUC.AdjustRowsHeightAndColumnsWidthSprain();
                return;
            }
            #endregion

            this.SetEnabledAllChildControls( false );
            await Task.Delay( millisecondsDelay: 250 );

            var requestHeaders = this.GetRequestHeaders();

            using ( var cts = new CancellationTokenSource() )
            using ( WaitBannerUC.Create( this, cts, visibleDelayInMilliseconds: 1_500 ) )
            {
                var webProxyInfo = this.GetWebProxyInfo();
                var webProxy     = webProxyInfo.CreateWebProxyIfUsed();
                _Model.AddBeginRequest2Log( this.M3u8FileUrl, requestHeaders, webProxyInfo, clearLog: false );
                var t = await _DC.GetFileTextContent( x.m3u8FileUrl, requestHeaders,
                    webProxy, _Settings.RequestTimeoutByPart, cts ); //all possible exceptions are thrown within inside

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
                    _Model.Output( t.m3u8File );
                }
                logUC.ClearSelection();
                logUC.AdjustRowsHeightAndColumnsWidthSprain();
            }

            this.SetEnabledAllChildControls( true );
        }
        private void SetLayout_4_logPanel()
        {
            var json = _Settings.ChangeSettingsParams4DownloadRowForm_PosLogVisibleJson;
            if ( !json.IsNullOrEmpty() )
            {
                var saved_height = this.Height;
                FormPositionStorer.LoadOnlyHeight( this, json );
                var d = (this.Height - saved_height);
                if ( 0 <= d && d < 50 )
                {
                    this.Height = saved_height + 250;
                }
            }
            else
            {
                this.Height += 250;
            }
        }
        #endregion

        #region [.requestHeadersEditor.]
        private void requestHeadersEditor_OnRequestHeadersCountChanged( int requestHeadersCount, int enabledCount ) 
            => requestHeadersTabPage.Text = (requestHeadersCount == enabledCount) ? $"request headers ({requestHeadersCount})" : $"request headers ({enabledCount} of {requestHeadersCount})";
        #endregion

        #region [.web-proxy.]
        private void set_WebProxyInfo( in web_proxy_info webProxyInfo )
        {
            set_WebProxyTabPageText( webProxyInfo.UseWebProxy, webProxyInfo.GetWebProxyAddressText() );
            webProxyUC.SetWebProxyInfo( webProxyInfo );
        }
        private void set_WebProxyTabPageText( bool useRequestWebProxy, string webProxyAddress )
        {
            webProxyTabPage.Text = "web proxy";

            if ( useRequestWebProxy )
            {
                webProxyTabPage.Text += $" ({webProxyAddress.Cut( 70 )})"; //" (used)";
            }
        }
        private void webProxyUC_OnWebProxyChanged( bool enabled, string addressRaw ) => set_WebProxyTabPageText( enabled, addressRaw );
        #endregion

        #region [.download additional m3u8Url's.]
        private bool logUC_AllowDownloadAdditionalM3u8Url( string m3u8FileUrl ) => !this.M3u8FileUrl.Equals( m3u8FileUrl, StringComparison.InvariantCultureIgnoreCase );
        private void logUC_DownloadAdditionalM3u8Url( Uri additionalM3u8Url )
        {
            string m3u8FileUrlText;
            if ( additionalM3u8Url.IsAbsoluteUri )
            {
                m3u8FileUrlText = additionalM3u8Url.ToString();
            }
            else
            {
                var baseM3u8FileUrl = this.M3u8FileUrl;
                if ( !UrlHelper.TryGetM3u8FileUrl( baseM3u8FileUrl, out var t ) )
                {
                    this.MessageBox_ShowError( $"Can parse main Url:\n '{baseM3u8FileUrl}' -> \r\n\r\n{t.error}", this.Text );
                }
                var combinedUrl = new Uri( t.m3u8FileUrl, additionalM3u8Url );
                m3u8FileUrlText = combinedUrl.ToString();
            }

            AddNewDownloadForm.Add( this, _DC, _SC, m3u8FileUrlText, this.requestHeadersEditor.GetRequestHeaders(),
            _OutputFileNamePatternProcessor, seriesInfo: null, _Transitive_FormClosedAction_When_DownloadAdditionalM3u8Url );
        }
        #endregion

        #region [.track last active focused control for each tab-page.]
        private Dictionary< TabPage, Control > _LastFocusedControls = new Dictionary< TabPage, Control >();
        private void BindFocusTracking( TabPage parentPage, Control container )
        {
            foreach ( Control c in container.Controls )
            {
                c.Enter += (s, _) => _LastFocusedControls[ parentPage ] = (Control) s;
                
                if ( c.HasChildren ) BindFocusTracking( parentPage, c );
            }
        }
        private void tabControl_Selected( object sender, TabControlEventArgs e )
        {
            if ( _LastFocusedControls.TryGetValue( e.TabPage, out var lastControl ) )
            {
                if ( lastControl.IsHandleCreated &&!lastControl.IsDisposed )
                {
                    // Используем BeginInvoke, так как TabControl может пытаться перехватить фокус на себя сразу после отрисовки
                    this.BeginInvoke( new Action( () => lastControl.Focus() ) );
                }
            }
            else
            {
                if ( e.TabPage == requestHeadersTabPage )
                {
                    requestHeadersEditor.Activate();
                }
                else if ( e.TabPage == webProxyTabPage )
                {
                    webProxyUC.Activate();
                }
            }
        }
        #endregion
    }
}
