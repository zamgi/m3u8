using System;
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
        private Settings          _Settings;
        private DownloadListModel _DownloadListModel;
        private FileNameCleaner.Processor _FNCP;
        private bool _WasFocusSet2outputFileNameTextBoxAfterFirstChanges;
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

            _FNCP = new FileNameCleaner.Processor( outputFileNameTextBox, 
                                                   () => this.OutputFileName,
                                                   setOutputFileName );
        }
        public AddNewDownloadForm( DownloadController dc, SettingsPropertyChangeController sc, string m3u8FileUrl, string additionalTitle = null ) : this()
        {
            _Settings = sc.Settings;
            _DownloadListModel = dc?.Model;

            this.M3u8FileUrl     = m3u8FileUrl;
            this.OutputDirectory = _Settings.OutputFileDirectory;
            _WasFocusSet2outputFileNameTextBoxAfterFirstChanges = m3u8FileUrl.IsNullOrWhiteSpace();

            _Model = new LogListModel();
            logUC.SetModel( _Model );
            logUC.SetSettingsController( sc );
            statusBarUC.SetDownloadController( dc );
            statusBarUC.SetSettingsController( sc );

            this.Text += additionalTitle;
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
                FormPositionStorer.LoadOnlyHeight( this, json );
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
                    this.Location = new Point( otherForm.Left + 10, otherForm.Top + 10 ); 
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
                    _Settings.OutputFileDirectory = this.OutputDirectory;
                }
                _Settings.SaveNoThrow();
            }
        }
        protected override void OnShown( EventArgs e )
        {
            base.OnShown( e );

            this.Activate();
            WinApi.SetForegroundWindow( this.Handle );
            WinApi.SetForceForegroundWindow( this.Handle );
        }
        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );

            if ( (DialogResult == DialogResult.OK) || (DialogResult == DialogResult.Yes) )
            {
                if ( this.M3u8FileUrl.IsNullOrWhiteSpace() )
                {
                    e.Cancel = true;
                    m3u8FileUrlTextBox.FocusAndBlinkBackColor();
                }
                else
                if ( _Settings.UniqueUrlsOnly && ( _DownloadListModel?.ContainsUrl( this.M3u8FileUrl )).GetValueOrDefault() )
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
                    var button = (this.ActiveControl as Button);
                    if ( (button == null) || !button.Focused ) //if ( !m3u8FileTextContentLoadButton.Focused && !outputFileNameClearButton.Focused && !outputFileNameSelectButton.Focused )
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
        private void outputFileNameTextBox_TextChanged( object sender, EventArgs e )
            => _FNCP.FileNameTextBox_TextChanged( outputFileName => _LastManualInputed_outputFileNameText = outputFileName );

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
            using ( var d = new FolderBrowserDialog() { SelectedPath        = this.OutputDirectory,
                                                        Description         = "Select Output directory",
                                                        ShowNewFolderButton = true } )
            {
                if ( d.ShowDialog( this ) == DialogResult.OK )
                {
                    this.OutputDirectory = d.SelectedPath;
                }
            }
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
                var t = await DownloadController.GetFileTextContent( x.m3u8FileUrl /*this.M3u8FileUrl*/, cts ); //all possible exceptions are thrown within inside
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
