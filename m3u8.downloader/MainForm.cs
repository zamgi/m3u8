using System;
using System.Collections.Specialized;
using System.ComponentModel;
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
        private const int    ATTEMPT_REQUEST_COUNT_BY_PART = 10;
        private const string M3U8_EXTENSION                = ".m3u8";

        private m3u8_client             _Mc;
        private CancellationTokenSource _Cts;
        private WaitBannerUC            _Wb;

        private string _m3u8FileUrl;
        private string _OutputFileName;
        #endregion

        #region [.ctor().]
        public MainForm()
        {
            InitializeComponent();

            m3u8FileUrlTextBox_TextChanged( this, EventArgs.Empty );
            maxDegreeOfParallelismLabel_set();
            autoMinimizeWindowWhenStartsDownloadLabel_set();
            autoCloseApplicationWhenEndsDownloadLabel_set();

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
                var text = Clipboard.GetText( TextDataFormat.Text | TextDataFormat.UnicodeText )?.Trim();
                if ( text.IsNullOrEmpty() ) return;
                if ( text.EndsWith( M3U8_EXTENSION, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    m3u8FileUrlTextBox.Text = text;
                }
                else
                {
                    var i = text.IndexOf( M3U8_EXTENSION + '?', StringComparison.InvariantCultureIgnoreCase );
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
        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );

            _Cts?.Cancel();
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            //still dowloading?
            if ( (_Mc != null) || outputFileNameTextBox.ReadOnly /*(_Cts?.IsCancellationRequested).GetValueOrDefault()*/ )
            {
                Extensions.DeleteFile_NoThrow( _OutputFileName );
            }
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
            try
            {                
                var m3u8FileUrl = new Uri( m3u8FileUrlText );

                var outputFileName = PathnameCleaner.CleanPathnameAndFilename( Uri.UnescapeDataString( m3u8FileUrl.AbsolutePath ) ).TrimStart( '-' );
                outputFileNameTextBox_Text = outputFileName;

                await Task.Delay( 500 );
                outputFileName = NameCleaner.Clean( outputFileName );
                outputFileNameTextBox_Text = outputFileName;

                await Task.Delay( 500 );
                if ( !outputFileName.EndsWith( Settings.Default.OutputFileExtension, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    if ( Settings.Default.OutputFileExtension.HasFirstCharNotDot() )
                    {
                        outputFileName += '.';
                    }
                    outputFileName += Settings.Default.OutputFileExtension;
                }
                outputFileNameTextBox_Text = outputFileName;
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }

        private string _LastManualInputed_outputFileNameText;
        private void outputFileNameTextBox_TextChanged( object sender, EventArgs e ) => _LastManualInputed_outputFileNameText = outputFileNameTextBox_Text;
        private string outputFileNameTextBox_Text
        {
            get => outputFileNameTextBox.Text.Trim();
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
            outputFileNameTextBox.Focus();
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
        private void autoMinimizeWindowWhenStartsDownloadLabel_Click( object sender, EventArgs e )
        {
            Settings.Default.AutoMinimizeWindowWhenStartsDownload = !Settings.Default.AutoMinimizeWindowWhenStartsDownload;
            Settings.Default.Save();
            autoMinimizeWindowWhenStartsDownloadLabel_set();
        }
        private void autoCloseApplicationWhenEndsDownloadLabel_Click( object sender, EventArgs e )
        {
            Settings.Default.AutoCloseApplicationWhenEndsDownload = !Settings.Default.AutoCloseApplicationWhenEndsDownload;
            Settings.Default.Save();
            autoCloseApplicationWhenEndsDownloadLabel_set();
        }

        private void statusBarLabel_MouseHover( object sender, EventArgs e )
        {
            if ( ((ToolStripItem) sender).Enabled && this.Cursor == Cursors.Default )
            {
                this.Cursor = Cursors.Hand;
            }
        }
        private void statusBarLabel_MouseLeave( object sender, EventArgs e )
        {
            if ( ((ToolStripItem) sender).Enabled && this.Cursor == Cursors.Hand )
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void maxDegreeOfParallelismLabel_set() =>
            maxDegreeOfParallelismLabel.Text = $"max degree of parallelism: {((Settings.Default.MaxDegreeOfParallelism == int.MaxValue) ? "Infinity" : Settings.Default.MaxDegreeOfParallelism.ToString())}";

        private void autoMinimizeWindowWhenStartsDownloadLabel_set() =>
            autoMinimizeWindowWhenStartsDownloadLabel.Image = (Settings.Default.AutoMinimizeWindowWhenStartsDownload ? Resources.check_16 : Resources.uncheck_16).ToBitmap();

        private void autoCloseApplicationWhenEndsDownloadLabel_set() =>
            autoCloseApplicationWhenEndsDownloadLabel.Image = (Settings.Default.AutoCloseApplicationWhenEndsDownload ? Resources.check_16 : Resources.uncheck_16).ToBitmap();
        #endregion

        private Uri TryGet_m3u8FileUrl( out Exception error )
        {
            try
            {
                var m3u8FileUrlText = m3u8FileUrlTextBox.Text.Trim();
                var m3u8FileUrl     = new Uri( m3u8FileUrlText );
                if ( (m3u8FileUrl.Scheme != Uri.UriSchemeHttp) && (m3u8FileUrl.Scheme != Uri.UriSchemeHttps) )
                {
                    throw (new ArgumentException( $"Only '{Uri.UriSchemeHttp}' and '{Uri.UriSchemeHttps}' schemes are allowed.", nameof(m3u8FileUrl) ));
                }
                error = null;
                return (m3u8FileUrl);
            }
            catch ( Exception ex )
            {
                error = ex;
                return (null);
            }
        }

        private async void m3u8FileTextContentLoadButton_Click( object sender, EventArgs e )
        {
            #region [.url.]
            var m3u8FileUrl = TryGet_m3u8FileUrl( out var error );
            if ( error != null )
            {
                Output2ResultTextBox_WithDelay( error );
                return;
            }
            #endregion
            //----------------------------------------------------//

            try
            {
                BeginOpAction( 1 );

                try
                {
                    var m3u8File = await _Mc.DownloadFile( m3u8FileUrl, _Cts.Token );

                    Output2ResultTextBox( m3u8File );
                }
                catch ( Exception innerEx )
                {
                    Output2ResultTextBox_IfNotIsCancellationRequested( innerEx );
                }

                FinishOpAction( m3u8FileTextContentLoadButton );
            }
            catch ( Exception outerEx )
            {
                FinishOpAction_With_MessageBox_ShowError( outerEx, m3u8FileTextContentLoadButton );
            }
        }

        private async void m3u8FileWholeLoadAndSaveButton_Click( object sender, EventArgs e )
        {
            #region [.url.]
            var m3u8FileUrl = TryGet_m3u8FileUrl( out var error );
            if ( error != null )
            {
                Output2ResultTextBox_WithDelay( error );
                return;
            }
            #endregion

            #region [.save file name dialog.]
            using ( var sfd = new SaveFileDialog() { InitialDirectory = Settings.Default.OutputFileDirectory, DefaultExt = Settings.Default.OutputFileExtension, AddExtension = true, } )
            {
                sfd.FileName = PathnameCleaner.CleanPathnameAndFilename( outputFileNameTextBox_Text ).TrimStart( '-' );
                if ( sfd.ShowDialog() != DialogResult.OK )
                {
                    return;
                }

                _OutputFileName = sfd.FileName;
                Settings.Default.OutputFileDirectory = Path.GetDirectoryName( _OutputFileName );
                Settings.Default.Save();
                outputFileNameTextBox_Text = Path.GetFileName( _OutputFileName );
                m3u8FileResultTextBox.Focus();
            }
            #endregion
            //----------------------------------------------------//

            #region [.auto minimize window when starts download.]
            if ( Settings.Default.AutoMinimizeWindowWhenStartsDownload )
            {
                this.WindowState = FormWindowState.Minimized;
            } 
            #endregion
            
            try
            {
                var sw = Stopwatch.StartNew();

                BeginOpAction();
                
                try
                {
                    //-1-//
                    var m3u8File = await _Mc.DownloadFile( m3u8FileUrl, _Cts.Token );

                    Output2ResultTextBox( m3u8File );

                    //-2-//
                    await Task.Delay( 3000, _Cts.Token );
                    UnsetResultTextBox();

                    _Wb.SetTotalSteps( m3u8File.Parts.Count );

                    //-3-//
                    try
                    {
                        var res = await Task.Run( () =>
                        {
                            var stepAction_UI = new m3u8_processor.StepActionDelegate( p =>
                            {
                                m3u8FileResultTextBox.AppendText( $"#{p.PartOrderNumber} of {p.TotalPartCount}). '{p.Part.RelativeUrlName}'..." );
                                if ( !p.Success )
                                {
                                    m3u8FileResultTextBox.AppendText( $" => '{p.Error.Message}' ----FAILED");
                                }
                                m3u8FileResultTextBox.AppendText( Environment.NewLine );
                            } );
                            var stepAction = new m3u8_processor.StepActionDelegate( p => this.BeginInvoke( stepAction_UI, p ) );

                            var sw_download = new Stopwatch();
                            var totalBytesLength = 0L;
                            var progressStepAction_UI = new m3u8_processor.ProgressStepActionDelegate( p =>
                            {
                                endStepActionLabel.Text = $"received {p.SuccessReceivedPartCount} of {p.TotalPartCount}";
                                if ( p.FailedReceivedPartCount != 0 )
                                {
                                    endStepActionLabel.Text += $", (failed: {p.FailedReceivedPartCount})";
                                }


                                totalBytesLength += p.BytesLength;
                                var speedText = default(string);
                                if ( !sw_download.IsRunning )
                                {
                                    sw_download.Start();
                                }
                                else
                                {
                                    var elapsedSeconds = (sw_download.ElapsedMilliseconds / 1000.0);
                                    if ( (1000 < totalBytesLength) && (2.5 <= elapsedSeconds) )
                                    {
                                        //if ( totalBytesLength < 1000   ) speedText = (totalBytesLength / elapsedSeconds).ToString("N2") + " bit/s";
                                        if ( totalBytesLength < 100000 ) speedText = ((totalBytesLength / elapsedSeconds) / 1000).ToString("N2") + " Kbit/s";
                                        else                             speedText = ((totalBytesLength / elapsedSeconds) / 1000000).ToString("N1") + " Mbit/s";

                                        endStepActionLabel.Text += $", [speed: {speedText}]";
                                    }
                                }

                                _Wb.IncreaseSteps( speedText );
                            } );
                            var progressStepAction = new m3u8_processor.ProgressStepActionDelegate( p => this.BeginInvoke( progressStepAction_UI, p ) );
                            var ip = new m3u8_processor.DownloadPartsAndSaveInputParams()
                            {
                                mc                     = _Mc,
                                m3u8File               = m3u8File,
                                OutputFileName         = _OutputFileName,
                                Cts                    = _Cts,
                                MaxDegreeOfParallelism = Settings.Default.MaxDegreeOfParallelism,
                                StepAction             = stepAction,
                                ProgressStepAction     = progressStepAction,         
                            };
                            //sw_download.Start();
                            var result = m3u8_processor.DownloadPartsAndSave( ip );
                            if ( sw_download.IsRunning ) sw_download.Stop();
                            return (result);
                        } );

                        //-4-//
                        sw.Stop();
                        
                        var renameOutputFileException = default(Exception);
                        #region [.remane output file if changed.]
                        var outputOnlyFileName = Path.GetFileName( res.OutputFileName );
                        if ( outputOnlyFileName != outputFileNameTextBox_Text )
                        {
                            _OutputFileName = Path.Combine( Path.GetDirectoryName( res.OutputFileName ), outputFileNameTextBox_Text );
                            try
                            {
                                Extensions.DeleteFile_NoThrow( _OutputFileName );
                                File.Move( res.OutputFileName, _OutputFileName );
                                res.ResetOutputFileName( _OutputFileName );
                            }
                            catch ( Exception ex )
                            {
                                renameOutputFileException = ex;                                    
                            }
                        }
                        #endregion

                        m3u8FileResultTextBox.AppendText( $"\r\n downloaded & writed parts {res.PartsSuccessCount} of {res.TotalParts}\r\n\r\n" );
                        m3u8FileResultTextBox.AppendText( $" elapsed: {sw.Elapsed}\r\n" );
                        m3u8FileResultTextBox.AppendText( $"         file: '{res.OutputFileName}'\r\n" );
                        m3u8FileResultTextBox.AppendText( $"       size: {(res.TotalBytes >> 20).ToString("0,0")} mb" );

                        FinishOpAction( m3u8FileResultTextBox );

                        #region [.error rename MessageBox.]
                        if ( renameOutputFileException != null )
                        {
                            this.MessageBox_ShowError( $"Rename output file error => '{renameOutputFileException}'", this.Text );
                        }
                        #endregion

                        this.MessageBox_ShowInformation( $"SUCCESS.\r\n\r\nelapsed: {sw.Elapsed}\r\n       file: '{res.OutputFileName}'\r\n      size: {(res.TotalBytes >> 20).ToString( "0,0" )} mb.", this.Text );

                        #region [.auto close application when ends download.]
                        if ( Settings.Default.AutoCloseApplicationWhenEndsDownload )
                        {
                            this.Close();
                        }
                        #endregion
                    }
                    catch ( Exception ex )
                    {
                        if ( _Cts.IsCancellationRequested )
                        {
                            Extensions.DeleteFile_NoThrow( _OutputFileName );
                        }
                        else
                        {
                            Append2ResultTextBox( ex );
                        }

                        FinishOpAction( m3u8FileResultTextBox );
                    }
                }
                catch ( Exception innerEx )
                {
                    Output2ResultTextBox_IfNotIsCancellationRequested( innerEx );

                    FinishOpAction( m3u8FileResultTextBox );
                }
            }
            catch ( Exception outerEx )
            {
                FinishOpAction_With_MessageBox_ShowError( outerEx, m3u8FileResultTextBox );
            }
        }

        private void BeginOpAction( int attemptRequestCountByPart = ATTEMPT_REQUEST_COUNT_BY_PART )
        {
            SetEnabledUI( false );
            UnsetResultTextBox();

            _Mc  = m3u8_client.CreateDefault( attemptRequestCountByPart );
            _Cts = new CancellationTokenSource();
            _Wb  = WaitBannerUC.Create( this, _Cts );
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
            _ChangeOutputFileForm?.Close();
        }
        private void FinishOpAction_With_MessageBox_ShowError( Exception ex, Control control4SetFocus = null )
        {
            FinishOpAction( control4SetFocus );
            this.MessageBox_ShowError( ex.ToString(), this.Text );
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

            outputFileNameTextBox.Click -= outputFileNameTextBox_Click;
            if ( outputFileNameTextBox.ReadOnly )
            {
                outputFileNameTextBox.Cursor = Cursors.Hand;
                outputFileNameTextBox.Click += outputFileNameTextBox_Click;
            }
            else
            {
                outputFileNameTextBox.Cursor = Cursors.IBeam;
            }

            m3u8FileTextContentLoadButton .Enabled = enabled;
            m3u8FileWholeLoadAndSaveButton.Enabled = enabled;
            outputFileNameClearButton     .Enabled = enabled;

            maxDegreeOfParallelismLabel.Enabled = enabled;
            excludesWordsLabel         .Enabled = enabled;
        }

        private void Output2ResultTextBox( m3u8_file_t m3u8File )
        {
            m3u8FileResultTextBox.ForeColor = Color.FromKnownColor( KnownColor.WindowText );
            m3u8FileResultTextBox.Lines     = m3u8File.RawText?.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
            m3u8FileResultTextBox.AppendText( $"\r\n\r\n patrs count: {m3u8File.Parts.Count}\r\n" );
        }
        private void Output2ResultTextBox( Exception ex )
        {
            m3u8FileResultTextBox.ForeColor = Color.Red;
            m3u8FileResultTextBox.Text      = ex.ToString();
        }
        private async void Output2ResultTextBox_WithDelay( Exception ex, int millisecondsDelay = 250 )
        {
            m3u8FileResultTextBox.Text = null;
            await Task.Delay( millisecondsDelay );
            Output2ResultTextBox( ex );
        }
        private void Output2ResultTextBox_IfNotIsCancellationRequested( Exception ex )
        {
            if ( !_Cts.IsCancellationRequested )
            {
                Output2ResultTextBox( ex );
            }
        }
        private void Append2ResultTextBox( Exception ex )
        {
            m3u8FileResultTextBox.ForeColor = Color.Red;
            m3u8FileResultTextBox.AppendText( Environment.NewLine );
            m3u8FileResultTextBox.AppendText( Environment.NewLine );
            m3u8FileResultTextBox.AppendText( ex.ToString() );
        }

        #region [.ChangeOutputFileForm.]
        private ChangeOutputFileForm _ChangeOutputFileForm;
        private void outputFileNameTextBox_Click( object sender, EventArgs e )
        {
            if ( outputFileNameTextBox.ReadOnly )
            {
                if ( (_ChangeOutputFileForm == null) || _ChangeOutputFileForm.IsDisposed )
                {
                    _ChangeOutputFileForm = new ChangeOutputFileForm() { Owner = this };
                    _ChangeOutputFileForm.FormClosed += _ChangeOutputFileForm_FormClosed;
                }
                _ChangeOutputFileForm.OutputFileName = outputFileNameTextBox.Text;
                _ChangeOutputFileForm.Show( this );
            }
        }

        private void _ChangeOutputFileForm_FormClosed( object sender, FormClosedEventArgs e )
        {
            if ( (e.CloseReason == CloseReason.UserClosing) && (_ChangeOutputFileForm.DialogResult == DialogResult.OK) && outputFileNameTextBox.ReadOnly )
            {                
                var outputFileName = PathnameCleaner.CleanPathnameAndFilename( _ChangeOutputFileForm.OutputFileName ).TrimStart( '-' );
                if ( !outputFileName.IsNullOrWhiteSpace() )
                {
                    outputFileName = Path.GetFileName( outputFileName );
                    var ext = Path.GetExtension( outputFileName );
                    if ( !ext.Equals( Settings.Default.OutputFileExtension, StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        outputFileName += Settings.Default.OutputFileExtension;
                    }
                    outputFileNameTextBox_Text = outputFileName;
                }
            }
            _ChangeOutputFileForm.FormClosed -= _ChangeOutputFileForm_FormClosed;
            _ChangeOutputFileForm = null;
        }
        #endregion
    }
}
