using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.Properties;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class MainForm : Form
    {
        #region [.fileds.]
        private string OUTPUT_FILE_DIR;
        private string OUTPUT_FILE_EXT;

        private m3u8_client             _Mc;
        private CancellationTokenSource _Cts;
        private WaitBannerUC_v1         _Wb;

        private string _m3u8FileUrl;
        #endregion

        #region [.ctor().]
        public MainForm()
        {
            InitializeComponent();

            OUTPUT_FILE_DIR = ConfigurationManager.AppSettings[ "OUTPUT_FILE_DIR" ]; if ( OUTPUT_FILE_DIR.IsNullOrWhiteSpace() ) OUTPUT_FILE_DIR = @"E:\\";
            OUTPUT_FILE_EXT = ConfigurationManager.AppSettings[ "OUTPUT_FILE_EXT" ]; if ( OUTPUT_FILE_EXT.IsNullOrWhiteSpace() ) OUTPUT_FILE_EXT = ".avi";

            m3u8FileUrlTextBox_TextChanged( this, EventArgs.Empty );
            maxDegreeOfParallelismLabel_set();

            NameCleaner.ResetExcludesWords( Settings.Default.NameCleanerExcludesWords?.Cast< string >() );
        }
        public MainForm( string m3u8FileUrl ) : this()
        {
            _m3u8FileUrl = m3u8FileUrl;
        }

        protected override void OnShown( EventArgs e )
        {
            base.OnShown( e );

            if ( !_m3u8FileUrl.IsNullOrWhiteSpace() )
            {
                m3u8FileUrlTextBox.Text = _m3u8FileUrl;
                return;
            }

            try
            {
                var text = Clipboard.GetText( TextDataFormat.Text | TextDataFormat.UnicodeText );
                    text = text?.Trim();
                if ( text.IsNullOrEmpty() ) return;
                if ( text.EndsWith( ".m3u8", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    m3u8FileUrlTextBox.Text = text;
                }
                else
                {
                    var i = text.IndexOf( ".m3u8?", StringComparison.InvariantCultureIgnoreCase );
                    if ( 10 < i )
                    {
                        m3u8FileUrlTextBox.Text = text;
                    }
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        protected override void OnKeyDown( KeyEventArgs e )
        {
            if ( (e.Modifiers & Keys.Control) == Keys.Control )
            {
                switch ( e.KeyCode )
                {
                    case Keys.W:
                    {
                        e.SuppressKeyPress = true;
                        //base.OnKeyDown( e );
                        this.Close();
                    }
                    return;

                    case Keys.D:
                    case Keys.M:
                    {
                        e.SuppressKeyPress = true;
                        this.WindowState = FormWindowState.Minimized;
                    }
                    return;

                    case Keys.S:
                    {
                        e.SuppressKeyPress = true;
                        m3u8FileWholeLoadAndSaveButton_Click( this, EventArgs.Empty );
                    }
                    return;
                }
            }

            base.OnKeyDown( e );
        }
        #endregion

        #region [.text-boxes.]
        private string _Last_m3u8FileUrlText;
        private async void m3u8FileUrlTextBox_TextChanged( object sender, EventArgs e )
        {
            var m3u8FileUrlText = m3u8FileUrlTextBox.Text.Trim();
            if ( (_Last_m3u8FileUrlText == m3u8FileUrlText) && !outputFileNameTextBox_Text.IsNullOrWhiteSpace() )
            {
                return;
            }
            if ( !_LastManualInputed_outputFileNameText.IsNullOrWhiteSpace() )
            {
                return;
            }
            _Last_m3u8FileUrlText = m3u8FileUrlText;
            
            outputFileNameTextBox_Text = null;
            //---toolTip.SetToolTip( outputFileNameTextBox, null );
            try
            {                
                var m3u8FileUrl = new Uri( m3u8FileUrlText );

                var outputFileName = PathnameCleaner.CleanPathnameAndFilename( Uri.UnescapeDataString( m3u8FileUrl.AbsolutePath ) ).TrimStart( '-' );
                outputFileNameTextBox_Text = outputFileName;

                await Task.Delay( 500 );
                outputFileName = NameCleaner.Clean( outputFileName );
                outputFileNameTextBox_Text = outputFileName;

                await Task.Delay( 500 );
                if ( !outputFileName.EndsWith( OUTPUT_FILE_EXT, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    if ( OUTPUT_FILE_EXT.HasFirstCharNotDot() )
                    {
                        outputFileName += '.';
                    }
                    outputFileName += OUTPUT_FILE_EXT;
                }
                outputFileNameTextBox_Text = outputFileName;
                //---toolTip.SetToolTip( outputFileNameTextBox, Path.Combine( OUTPUT_FILE_DIR, outputFileName ) );
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }

        private string _LastManualInputed_outputFileNameText;
        private void outputFileNameTextBox_TextChanged( object sender, EventArgs e )
        {
            _LastManualInputed_outputFileNameText = outputFileNameTextBox_Text;
        }
        private string outputFileNameTextBox_Text
        {
            get { return (outputFileNameTextBox.Text.Trim()); }
            set
            {
                if ( outputFileNameTextBox.Text.Trim() != value )
                {
                    outputFileNameTextBox.TextChanged -= outputFileNameTextBox_TextChanged;
                    outputFileNameTextBox.Text = value;
                    outputFileNameTextBox.TextChanged += outputFileNameTextBox_TextChanged;
                }
            }
        }

        private void outputFileNameClearButton_Click( object sender, EventArgs e )
        {
            outputFileNameTextBox_Text = null;

            outputFileNameTextBox.Focus(); // m3u8FileUrlTextBox.Focus();
        }


        private void maxDegreeOfParallelismLabel_Click( object sender, EventArgs e )
        {
            using ( var f = new MaxDegreeOfParallelismForm() )
            {
                f.MaxDegreeOfParallelism = Settings.Default.MaxDegreeOfParallelism;
                f.IsInfinity             = (Settings.Default.MaxDegreeOfParallelism == int.MaxValue);
                if ( f.ShowDialog() == DialogResult.OK )
                {
                    Settings.Default.MaxDegreeOfParallelism = f.MaxDegreeOfParallelism;
                    Settings.Default.Save();
                    maxDegreeOfParallelismLabel_set();
                }
            }
        }
        private void excludesWordsLabel_Click( object sender, EventArgs e )
        {
            using ( var f = new FileNameExcludesWordsEditor( NameCleaner.ExcludesWords ) )
            {
                if ( f.ShowDialog() == DialogResult.OK )
                {
                    NameCleaner.ResetExcludesWords( f.GetFileNameExcludesWords() );

                    if ( Settings.Default.NameCleanerExcludesWords == null )
                    {
                        Settings.Default.NameCleanerExcludesWords = new StringCollection();
                    }
                    else
                    {
                        Settings.Default.NameCleanerExcludesWords.Clear();
                    }                    
                    Settings.Default.NameCleanerExcludesWords.AddRange( NameCleaner.ExcludesWords.ToArray() );
                    Settings.Default.Save();
                }
            }
        }
        private void maxDegreeOfParallelismLabel_MouseHover( object sender, EventArgs e )
        {
            if ( ((ToolStripItem) sender).Enabled && this.Cursor == Cursors.Default )
            {
                this.Cursor = Cursors.Hand;
            }
        }
        private void maxDegreeOfParallelismLabel_MouseLeave( object sender, EventArgs e )
        {
            if ( ((ToolStripItem) sender).Enabled && this.Cursor == Cursors.Hand )
            {
                this.Cursor = Cursors.Default;
            }
        }
        private void maxDegreeOfParallelismLabel_set()
        {
            maxDegreeOfParallelismLabel.Text = $"max degree of parallelism: {((Settings.Default.MaxDegreeOfParallelism == int.MaxValue) ? "Infinity" : Settings.Default.MaxDegreeOfParallelism.ToString())}";
        }
        #endregion


        private void m3u8FileTextContentLoadButton_Click( object sender, EventArgs e )
        {
            try
            {
                var m3u8FileUrlText = m3u8FileUrlTextBox.Text.Trim();

                BeginOpAction( 1 );

                var task = Task.Run( () =>
                {
                    //-1-//
                    var m3u8FileUrl = new Uri( m3u8FileUrlText );
                    var m3u8File = _Mc.DownloadFile( m3u8FileUrl, _Cts.Token ).Result;
                    return (m3u8File);

                }/*, cts.Token*/ )
                .ContinueWith( (continuationTask) =>
                {
                    if ( !_Cts.IsCancellationRequested )
                    {
                        if ( continuationTask.IsFaulted )
                        {
                            m3u8FileResultTextBox.ForeColor = Color.Red;
                            m3u8FileResultTextBox.Text = continuationTask.Exception.ToString();
                        }
                        else if ( continuationTask.IsCompleted )
                        {
                            var m3u8File = continuationTask.Result;

                            m3u8FileResultTextBox.ForeColor = Color.FromKnownColor( KnownColor.WindowText );
                            m3u8FileResultTextBox.Lines = m3u8File.RawText?.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
                            m3u8FileResultTextBox.AppendText( $"\r\n\r\n patrs count: {m3u8File.Parts.Count}\r\n" );
                        }
                    }

                    FinishOpAction( m3u8FileTextContentLoadButton );

                }, TaskScheduler.FromCurrentSynchronizationContext() );
            }
            catch ( Exception ex )
            {
                FinishOpAction( m3u8FileTextContentLoadButton );

                MessageBox.Show( this, ex.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }            
        }

        private void m3u8FileWholeLoadAndSaveButton_Click( object sender, EventArgs e )
        {
            #region [.save file name dialog.]
            var m3u8FileUrlText = m3u8FileUrlTextBox.Text.Trim();
            var m3u8FileUrl     = new Uri( m3u8FileUrlText );

            var outputFileName = default(string);
            using ( var sfd = new SaveFileDialog() { InitialDirectory = OUTPUT_FILE_DIR, DefaultExt = OUTPUT_FILE_EXT, AddExtension = true } )
            {
                //---sfd.FileName = PathnameCleaner.CleanPathnameAndFilename( Uri.UnescapeDataString( m3u8FileUrl.AbsolutePath ) ).TrimStart( '-' );
                sfd.FileName = PathnameCleaner.CleanPathnameAndFilename( outputFileNameTextBox_Text ).TrimStart( '-' );
                if ( sfd.ShowDialog() != DialogResult.OK )
                {
                    return;
                }

                outputFileName  = sfd.FileName;
                OUTPUT_FILE_DIR = Path.GetDirectoryName( outputFileName );
                outputFileNameTextBox_Text = Path.GetFileName( outputFileName );
                m3u8FileResultTextBox.Focus();
            } 
            #endregion
            //----------------------------------------------------//

            try
            {
                var sw = Stopwatch.StartNew();

                BeginOpAction();
                
                var task = Task.Run( () =>
                {                        
                    //-1-//
                    var m3u8File = _Mc.DownloadFile( m3u8FileUrl, _Cts.Token ).Result;
                    return (m3u8File);

                }/*, cts.Token*/ )
                .ContinueWith( (continuationTask) =>
                {
                    if ( !_Cts.IsCancellationRequested )
                    {
                        if ( continuationTask.IsFaulted )
                        {
                            m3u8FileResultTextBox.ForeColor = Color.Red;
                            m3u8FileResultTextBox.Text = continuationTask.Exception.ToString();
                        }
                        else if ( continuationTask.IsCompleted )
                        {
                            var m3u8File = continuationTask.Result;

                            m3u8FileResultTextBox.ForeColor = Color.FromKnownColor( KnownColor.WindowText );
                            m3u8FileResultTextBox.Lines = m3u8File.RawText?.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
                            m3u8FileResultTextBox.AppendText( $"\r\n\r\n patrs count: {m3u8File.Parts.Count}\r\n" );

                            return (m3u8File);
                        }
                    }

                    FinishOpAction( m3u8FileResultTextBox );

                    return (default(m3u8_file_t));

                }, TaskScheduler.FromCurrentSynchronizationContext() )
                .ContinueWith( (continuationTask) =>
                {
                    Task.Delay( 3000 ).Wait( _Cts.Token );
                    this.BeginInvoke( new Action( UnsetResultTextBox ) );


                    var m3u8File = continuationTask.Result;

                    _Wb.SetTotalSteps( m3u8File.Parts.Count );

                    //-1-//
                    var stepAction_UI = new m3u8_processor.StepActionDelegate( (p) =>
                    {
                        m3u8FileResultTextBox.AppendText( $"#{p.PartOrderNumber} of {p.TotalPartCount}). '{p.Part.RelativeUrlName}'..." );
                        if ( !p.Success )
                        {
                            m3u8FileResultTextBox.AppendText( $" => '{p.Error.Message}' ----FAILED");
                        }
                        m3u8FileResultTextBox.AppendText( Environment.NewLine );

                        //---_Wb.IncreaseSteps();
                    } );
                    var stepAction = new m3u8_processor.StepActionDelegate( (p) => this.BeginInvoke( stepAction_UI, p ) );
                    //-2-//
                    var progressStepAction_UI = new m3u8_processor.ProgressStepActionDelegate( (p) =>
                    {
                        endStepActionLabel.Text = $"received {p.SuccessReceivedPartCount} of {p.TotalPartCount}";
                        if ( p.FailedReceivedPartCount != 0 )
                        {
                            endStepActionLabel.Text += $", (failed: {p.FailedReceivedPartCount})";
                        }

                        _Wb.IncreaseSteps();
                    } );
                    var progressStepAction = new m3u8_processor.ProgressStepActionDelegate( (p) => this.BeginInvoke( progressStepAction_UI, p ) );
                    //-3-//
                    var ip = new m3u8_processor.DownloadPartsAndSaveInputParams()
                    {
                        mc                     = _Mc,
                        m3u8File               = m3u8File,
                        OutputFileName         = outputFileName,
                        Cts                    = _Cts,
                        MaxDegreeOfParallelism = Settings.Default.MaxDegreeOfParallelism,
                        StepAction             = stepAction,
                        ProgressStepAction     = progressStepAction,         
                    };
                    //-4-//
                    var result = m3u8_processor.DownloadPartsAndSave( ip );
                    return (result);

                }, TaskContinuationOptions.OnlyOnRanToCompletion )
                .ContinueWith( (continuationTask) =>
                {
                    sw.Stop();

                    var success = default(bool);
                    if ( !_Cts.IsCancellationRequested )
                    {
                        if ( continuationTask.IsFaulted )
                        {
                            m3u8FileResultTextBox.ForeColor = Color.Red;
                            m3u8FileResultTextBox.AppendText( Environment.NewLine );
                            m3u8FileResultTextBox.AppendText( Environment.NewLine );
                            m3u8FileResultTextBox.AppendText( continuationTask.Exception.ToString() );
                        }
                        else if ( continuationTask.IsCompleted )
                        {
                            var res = continuationTask.Result;

                            m3u8FileResultTextBox.AppendText( $"\r\n downloaded & writed parts {res.PartsSuccessCount} of {res.TotalParts}\r\n" );
                            m3u8FileResultTextBox.AppendText( $" elapsed: {sw.Elapsed}\r\n" );
                            m3u8FileResultTextBox.AppendText( $" file: '{res.OutputFileName}'\r\n" );
                            m3u8FileResultTextBox.AppendText( $" size: {(res.TotalBytes >> 20).ToString( "0,0" )} mb" );

                            success = true;                            
                        }
                    }
                    else
                    {
                        Extensions.DeleteFile_NoThrow( outputFileName );
                    }

                    FinishOpAction( m3u8FileResultTextBox );

                    if ( success )
                    {
                        MessageBox.Show( this, new string(' ', 25) + "SUCCESS" + new string(' ', 25), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information );
                    }

                }, TaskScheduler.FromCurrentSynchronizationContext() );
            }
            catch ( Exception ex )
            {
                FinishOpAction( m3u8FileResultTextBox );

                MessageBox.Show( this, ex.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }


        private void BeginOpAction( int? attemptRequestCountByPart = null )
        {
            SetEnabledUI( false );
            UnsetResultTextBox();

            _Mc  = m3u8_client.CreateDefault( attemptRequestCountByPart.GetValueOrDefault( 10 ) );
            _Cts = new CancellationTokenSource();
            _Wb  = WaitBannerUC_v1.Create( this, _Cts );
        }
        private void FinishOpAction( Control control4SetFocus = null )
        {
            if ( (_Cts?.IsCancellationRequested).GetValueOrDefault() )
            {
                m3u8FileResultTextBox.AppendText( Environment.NewLine );
                m3u8FileResultTextBox.AppendText( Environment.NewLine );
                m3u8FileResultTextBox.AppendText( ".....cancel....." );
            }

            _Mc? .Dispose(); _Mc  = null;
            _Cts?.Dispose(); _Cts = null;
            _Wb? .Dispose(); _Wb  = null;

            SetEnabledUI( true );

            if ( this.WindowState == FormWindowState.Minimized )
            {
                this.WindowState = WinApi.GetWindowPlacement( this.Handle ).ToFormWindowState();
            }
            this.Activate();
            WinApi.SetForegroundWindow( this.Handle );

            control4SetFocus?.Focus();
        }

        private void UnsetResultTextBox()
        {
            m3u8FileResultTextBox.ForeColor = Color.FromKnownColor( KnownColor.WindowText );
            m3u8FileResultTextBox.Clear();
            endStepActionLabel.Text = string.Empty;
            Application.DoEvents();
        }
        private void SetEnabledUI( bool enabled )
        {
            m3u8FileUrlTextBox   .ReadOnly = !enabled;
            outputFileNameTextBox.ReadOnly = !enabled;
            m3u8FileTextContentLoadButton .Enabled = enabled;
            m3u8FileWholeLoadAndSaveButton.Enabled = enabled;
            outputFileNameClearButton     .Enabled = enabled;

            maxDegreeOfParallelismLabel.Enabled = enabled;
            excludesWordsLabel         .Enabled = enabled;
            //statusBar.Visible = enabled;
        }
    }
}
