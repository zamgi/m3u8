using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    internal sealed partial class AddNewDownloadForm : Form
    {
        #region [.fields.]
        private LogListModel      _Model;
        private bool              _DownloadLater;
        private _DC_              _DC;
        private _SC_              _SC;
        private Settings          _Settings;
        private DownloadListModel _DownloadListModel;
        private FileNameCleaner4UI.Processor _FNCP;
        private bool _WasFocusSet2outputFileNameTextBoxAfterFirstChanges;
        private (int n, int total) _SeriesInfo;
        private string             _Initial_M3u8FileUrl;
        private OutputFileNamePatternProcessor _OutputFileNamePatternProcessor;
        private bool _IsInEditMode;
        private Func< AddNewDownloadForm, Task > _FormClosedAction;
        #endregion

        private static string GetCaptionBySeriesInfo( in (int n, int total) seriesInfo ) => $" ({seriesInfo.n} of {seriesInfo.total})";
        private static string GetCaptionBySeriesInfo( in (int n, int total)? seriesInfo ) => seriesInfo.HasValue ? GetCaptionBySeriesInfo( seriesInfo.Value ) : null;

        #region [.ctor().]
        private AddNewDownloadForm( _DC_ dc, _SC_ sc )
        {
            _DC       = dc;
            _SC       = sc;
            _Settings = sc.Settings;

            InitializeComponent( dc, sc );
            //----------------------------------------//

            //---statusBarUC.IsVisibleParallelismLabel = false;
            logPanel.Visible = false;
            logUC.ShowResponseColumn = false;

            _FNCP = new FileNameCleaner4UI.Processor( outputFileNameTextBox, () => this.OutputFileName, setOutputFileName );
        }

        /// <summary>
        /// Edit
        /// </summary>
        private AddNewDownloadForm( _DC_ dc, _SC_ sc
            , DownloadRow row
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor ) : this( dc, sc )
        {
            _IsInEditMode      = true;
            _DownloadListModel = dc?.Model;
            requestHeadersEditor.SetRequestHeaders( row.RequestHeaders, sc.IgnoreHostHttpHeader );

            this.OutputFileName               = row.OutputFileName;
            this.OutputDirectory              = row.OutputDirectory;
            this.IsLiveStream                 = row.IsLiveStream;
            this.LiveStreamMaxFileSizeInBytes = row.LiveStreamMaxFileSizeInBytes;
            
            _Initial_M3u8FileUrl = row.Url;
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
        }

        /// <summary>
        /// Add
        /// </summary>
        private AddNewDownloadForm( _DC_ dc, _SC_ sc
            , string m3u8FileUrl
            , IDictionary< string, string > requestHeaders
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor
            , in (int n, int total)? seriesInfo = null ) : this( dc, sc )
        {
            _DownloadListModel = dc?.Model;
            requestHeadersEditor.SetRequestHeaders( requestHeaders, sc.IgnoreHostHttpHeader );

            _Initial_M3u8FileUrl = m3u8FileUrl;
            _OutputFileNamePatternProcessor = outputFileNamePatternProcessor;

            #region [.if setted outputFileName.]
            //before 'this.M3u8FileUrl = m3u8FileUrl;'
            Process_use_OutputFileNamePatternProcessor_on_Init();
            #endregion

            this.M3u8FileUrl     = m3u8FileUrl;
            this.OutputDirectory = _Settings.OutputFileDirectory;
            _WasFocusSet2outputFileNameTextBoxAfterFirstChanges = m3u8FileUrl.IsNullOrWhiteSpace();

            _Model = new LogListModel();
            logUC.SetModel( _Model );

            #region [.seriesInfo.]
            this.Text += GetCaptionBySeriesInfo( seriesInfo );
            _SeriesInfo = seriesInfo.GetValueOrDefault( (1, 1) );
            #endregion

            this.LiveStreamMaxFileSizeInMb = _Settings.LiveStreamMaxSingleFileSizeInMb;           
        }

        private (string audio, string video) _GroupedUrls;
        /// <summary>
        /// Add Grouped
        /// </summary>
        private AddNewDownloadForm( _DC_ dc, _SC_ sc
            , string m3u8FileUrl, in (string audio, string video) groupedUrls
            , string outputFileName
            , ICollection< KeyValuePair< string, string > > requestHeaders
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor
            , in (int n, int total)? seriesInfo = null ) : this( dc, sc )
        {
            _DownloadListModel = dc?.Model;
            requestHeadersEditor.SetRequestHeaders( requestHeaders, sc.IgnoreHostHttpHeader );

            _Initial_M3u8FileUrl = m3u8FileUrl;
            _OutputFileNamePatternProcessor = outputFileNamePatternProcessor;

            _GroupedUrls = groupedUrls;
            #region [.if setted outputFileName.]
            //before 'this.M3u8FileUrl = m3u8FileUrl;'
            var suc = Process_use_OutputFileNamePatternProcessor_on_Init();
            if ( !suc )
            {
                m3u8FileUrlTextBox.TextChanged -= m3u8FileUrlTextBox_TextChanged;
                this.OutputFileName = outputFileName;
            }
            #endregion

            this.M3u8FileUrl     = m3u8FileUrl;
            this.OutputDirectory = _Settings.OutputFileDirectory;
            _WasFocusSet2outputFileNameTextBoxAfterFirstChanges = m3u8FileUrl.IsNullOrWhiteSpace();

            _Model = new LogListModel();
            logUC.SetModel( _Model );

            #region [.seriesInfo.]
            this.Text += GetCaptionBySeriesInfo( seriesInfo );
            _SeriesInfo = seriesInfo.GetValueOrDefault( (1, 1) );
            #endregion

            this.LiveStreamMaxFileSizeInMb = _Settings.LiveStreamMaxSingleFileSizeInMb;           
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

        public static void AddGrouped( IWin32Window owner, _DC_ dc, _SC_ sc
            , string m3u8FileUrl, in (string audio, string video) groupedUrls
            , string outputFileName
            , ICollection< KeyValuePair< string, string > > requestHeaders
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor
            , in (int n, int total)? seriesInfo
            , Func< AddNewDownloadForm, Task > formClosedAction )
        {
            var f = new AddNewDownloadForm( dc, sc, m3u8FileUrl, groupedUrls, outputFileName, requestHeaders, outputFileNamePatternProcessor, seriesInfo )
            {
                Icon = Resources.group_by_audio_video.CreateSafeIcon(), 
                Text = "add new download (grouped by audio-video urls)" + GetCaptionBySeriesInfo( seriesInfo ),
                _FormClosedAction = formClosedAction,
            };
            f.m3u8FileUrlTextBox       .ReadOnly = true;
            f.requestHeadersEditor     .ReadOnly = true;
            f.loadM3u8FileContentButton.ReadOnly = true;
            f.InitAndShowWhenAdd( owner, m3u8FileUrl, formClosedAction );
        }
        public static void Add( IWin32Window owner, _DC_ dc, _SC_ sc
            , string m3u8FileUrl
            , IDictionary< string, string > requestHeaders
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor
            , in (int n, int total)? seriesInfo
            , Func< AddNewDownloadForm, Task > formClosedAction )
        {
            var f = new AddNewDownloadForm( dc, sc, m3u8FileUrl, requestHeaders, outputFileNamePatternProcessor, seriesInfo ) { _FormClosedAction = formClosedAction };
            f.InitAndShowWhenAdd( owner, m3u8FileUrl, formClosedAction );
        }
        private void InitAndShowWhenAdd( IWin32Window owner, string m3u8FileUrl, Func< AddNewDownloadForm, Task > formClosedAction )
        {
            this.FormClosed += (_, _) => formClosedAction?.Invoke( this );
            var close = new EventHandler( (_, _) => this.Close() );
            this.downloadStartButton.Click += close;
            this.downloadLaterButton.Click += close;
            if ( m3u8FileUrl.IsNullOrWhiteSpace() ) this.Shown += (_, _) => this.m3u8FileUrlTextBox.Focus();
            this.Show( owner );
        }
        public static void Edit( IWin32Window owner, _DC_ dc, _SC_ sc
            , DownloadRow row
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor
            , Action< FormClosingEventArgs > formClosingAction
            , Action< AddNewDownloadForm, DownloadRow > formClosedAction
            , Func< AddNewDownloadForm, Task > formClosedAction_4_DownloadAdditionalM3u8Url )
        {
            var f = new AddNewDownloadForm( dc, sc, row, outputFileNamePatternProcessor ) 
            { 
                Icon = Resources.edit.CreateSafeIcon(),
                Text = $"Edit, / '{row.OutputFileName}' /",
                _FormClosedAction = formClosedAction_4_DownloadAdditionalM3u8Url,
            };

            f.FormClosing += (_, e) => formClosingAction?.Invoke( e );
            f.FormClosed  += (_, _) => formClosedAction?.Invoke( f, row );
            var close = new EventHandler( (_, _) => f.Close() );
            f.downloadStartButton.Text = "Ok";
            f.downloadLaterButton.Text = "Cancel";
            f.downloadStartButton.Click += close;
            f.downloadLaterButton.Click += close;
            f.Shown += (_, _) => f.setFocus2outputFileNameTextBox_Core();
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
        protected override void OnFormClosed( FormClosedEventArgs e )
        {
            base.OnFormClosed( e );
            _Settings.PropertyChanged -= _Settings_PropertyChanged;

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

            this.ExternalProgApplyByDefault = _Settings.ExternalProgApplyByDefault;
            _Settings.PropertyChanged += _Settings_PropertyChanged;

            _LastForegroundWnd = WinApi.GetForegroundWindow(); //WinApi.GetTopForegroundWindow();

            this.Activate();
            WinApi.SetForegroundWindow( this.Handle );
            WinApi.SetForceForegroundWindow( this.Handle, _LastForegroundWnd );
        }

        private void _Settings_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == nameof(Settings.ExternalProgApplyByDefault) )
            {
                this.ExternalProgApplyByDefault = _Settings.ExternalProgApplyByDefault;
            }
        }
        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            base.OnFormClosing( e );

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
                if ( !_IsInEditMode && _Settings.UniqueUrlsOnly && (_DownloadListModel?.ContainsAnyUrls( this.M3u8FileUrl, _GroupedUrls.audio, _GroupedUrls.video )).GetValueOrDefault() )
                {
                    e.Cancel = true;
                    this.MessageBox_ShowError( $"Url already exists in list:\n '{this.M3u8FileUrl}'", this.Text );
                    m3u8FileUrlTextBox.FocusAndBlinkBackColor();
                }
                else
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
        public  string GetOutputFileName()
        {
            var outputFileName_1 = GetOutputFileName_Internal();
            var outputFileName_2 = _OutputFileNamePatternProcessor.Process( outputFileName_1 );
            return (outputFileName_2);
        }
        private string GetOutputFileName_Internal() => FileNameCleaner4UI.GetOutputFileName( this.OutputFileName, _Settings.OutputFileExtension, _OutputFileNamePatternProcessor.PatternChar );
        public  string GetOutputDirectory() => this.OutputDirectory;
        public  IDictionary< string, string > GetRequestHeaders() => requestHeadersEditor.GetRequestHeaders();
        public  bool   IsLiveStream
        { 
            get => isLiveStreamCheckBox.Checked;
            set => isLiveStreamCheckBox.Checked = value;
        }
        public int     LiveStreamMaxFileSizeInMb
        {
            get => (int) liveStreamMaxSizeInMbNumUpDn.Value;
            set => liveStreamMaxSizeInMbNumUpDn.Value = Math.Max( liveStreamMaxSizeInMbNumUpDn.Minimum, Math.Min( liveStreamMaxSizeInMbNumUpDn.Maximum, value ) );
        }
        public long    LiveStreamMaxFileSizeInBytes
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

        public bool ExternalProgApplyByDefault
        {
            get => externalProgApplyByDefaultCheckBox.Checked;
            set
            {
                externalProgApplyByDefaultCheckBox.Checked = value;
                set_externalProgApplyByDefaultCheckBox_Color( value );
            }
        }

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

            Process_use_OutputFileNamePatternProcessor();
        }

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
        private void externalProgApplyByDefaultCheckBox_Click( object sender, EventArgs e )
        {
            var externalProgApplyByDefault = this.ExternalProgApplyByDefault;

            set_externalProgApplyByDefaultCheckBox_Color( externalProgApplyByDefault );
            _Settings.ExternalProgApplyByDefault = externalProgApplyByDefault;
        }
        private void set_externalProgApplyByDefaultCheckBox_Color( bool externalProgApplyByDefault ) => externalProgApplyByDefaultCheckBox.ForeColor = externalProgApplyByDefault ? Color.FromArgb( 70, 70, 70 ) : Color.Silver;
        private void set_mainLayoutPanel_Height( bool? isLiveStream_or_patternOutputFileName_visible = null )
        {
            const int DEFAULT_HEIGHT_isLiveStream    = 30;
            const int DEFAULT_HEIGHT_mainLayoutPanel = 70 + 20;
            const int DEFAULT_HEIGHT_this            = 315 + 20;

            var is_extra_visible = isLiveStream_or_patternOutputFileName_visible.GetValueOrDefault( this.IsLiveStream || patternOutputFileNameLabel.Visible );
            var extra_height = is_extra_visible ? DEFAULT_HEIGHT_isLiveStream : 0;

            mainLayoutPanel.Height = DEFAULT_HEIGHT_mainLayoutPanel + extra_height;
            if ( is_extra_visible && (this.Height <= DEFAULT_HEIGHT_this) ) this.Height += extra_height;
        }

        private bool Process_use_OutputFileNamePatternProcessor_on_Init()
        {
            bool suc;
            if ( suc = _OutputFileNamePatternProcessor.TryGet_Patterned_Last_OutputFileName( out var t ) )
            {
                m3u8FileUrlTextBox.TextChanged -= m3u8FileUrlTextBox_TextChanged;
                this.OutputFileName = t.Patterned_Last_OutputFileName;
                Set_patternOutputFileNameLabel( t.Last_OutputFileName_As_Pattern );
                patternOutputFileNameNumUpDn.ValueAsInt32 = t.Last_OutputFileName_Num;
                patternOutputFileNameLabelCaption.Visible = patternOutputFileNameLabel.Visible = patternOutputFileNameNumUpDn.Visible = true;
                set_mainLayoutPanel_Height( isLiveStream_or_patternOutputFileName_visible: true );

                if ( !_Initial_M3u8FileUrl.IsNullOrWhiteSpace() )
                {
                    var shown = default(EventHandler);
                    shown = (_, _) => { setFocus2outputFileNameTextBox_Core( t.Patterned_Last_OutputFileName ); this.Shown -= shown; };
                    this.Shown += shown;
                }
            }

            //TEMP
#if DEBUG
            //else if ( !_Initial_M3u8FileUrl.IsNullOrWhiteSpace() )
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
            return (suc);
        }
        private void Process_use_OutputFileNamePatternProcessor()
        {
            var outputFileName = this.OutputFileName;
            var has = _OutputFileNamePatternProcessor.HasPatternChar( outputFileName );
            if ( has )
            {
                Set_patternOutputFileNameLabel( outputFileName );
                if ( _OutputFileNamePatternProcessor.IsEqualPattern( outputFileName ) )
                {
                    patternOutputFileNameNumUpDn.ValueAsInt32 = _OutputFileNamePatternProcessor.Last_OutputFileName_Num;
                    patternOutputFileNameNumUpDn_ValueChanged( null, null );
                }
                else
                {
                    patternOutputFileNameNumUpDn.ValueAsInt32 = 1;
                    _OutputFileNamePatternProcessor.Set_Last_OutputFileName_Num( patternOutputFileNameNumUpDn.ValueAsInt32 );
                }

                //---Set_patternOutputFileNameLabel( outputFileName );
                //---patternOutputFileNameNumUpDn.ValueAsInt32 = _OutputFileNamePatternProcessor.IsEqualPattern( outputFileName ) ? _OutputFileNamePatternProcessor.Last_OutputFileName_Num : 1;
                //---patternOutputFileNameNumUpDn_ValueChanged( null, null );
            }
            patternOutputFileNameLabelCaption.Visible = patternOutputFileNameLabel.Visible = patternOutputFileNameNumUpDn.Visible = has;
            set_mainLayoutPanel_Height();
        }
        private void patternOutputFileNameNumUpDn_ValueChanged( object sender, EventArgs e )
        {
            _OutputFileNamePatternProcessor.Set_Last_OutputFileName_Num( patternOutputFileNameNumUpDn.ValueAsInt32 );

            if ( _OutputFileNamePatternProcessor.HasLast_OutputFileName_As_Pattern && 
                 _OutputFileNamePatternProcessor.IsEqualPattern( this.OutputFileName ) 
               )
            {
                this.OutputFileName = _OutputFileNamePatternProcessor.Get_Patterned_Last_OutputFileName();
            }
        }
        private void patternOutputFileNameLabel_VisibleChanged( object sender, EventArgs e ) => mainLayoutPanel.ColumnStyles[ 1 ].SizeType = patternOutputFileNameLabel.Visible ? SizeType.AutoSize : SizeType.Absolute;
        private void Set_patternOutputFileNameLabel( string outputFileName )
        {
            patternOutputFileNameLabel.Text = outputFileName;
            toolTip.SetToolTip( patternOutputFileNameLabel, outputFileName );
        }
        #endregion

        #region [.loadM3u8FileContentButton.]
        private async void loadM3u8FileContentButton_Click( object sender, EventArgs e )
        {
            if ( !logPanel.Visible )
            {
                logPanel.Visible = true;
                Set_logPanel_Visible();
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
                _Model.AddBeginRequest2Log( this.M3u8FileUrl, requestHeaders, clearLog: false );
                var t = await _DC_.GetFileTextContent( x.m3u8FileUrl, requestHeaders, _Settings.RequestTimeoutByPart, cts ); //all possible exceptions are thrown within inside

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
        private void Set_logPanel_Visible()
        {
            var json = _Settings.AddNewDownloadFormPositionLogVisibleJson;
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


        private bool logUC_AllowDownloadAdditionalM3u8Url( string m3u8FileUrl ) => !this.M3u8FileUrl.Equals( m3u8FileUrl, StringComparison.InvariantCultureIgnoreCase );
        private void logUC_DownloadAdditionalM3u8Url( Uri m3u8FileUrl )
        => AddNewDownloadForm.Add( this, _DC, _SC, m3u8FileUrl.ToString(), this.requestHeadersEditor.GetRequestHeaders(), _OutputFileNamePatternProcessor, seriesInfo: null, _FormClosedAction );
    }
}
