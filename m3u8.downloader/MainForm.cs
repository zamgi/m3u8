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
    public sealed partial class MainForm : Form
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

        /*private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MINIMIZE   = 0xF020;

        protected override void WndProc( ref Message m )
        {
            switch ( m.Msg )
            {
                case WM_SYSCOMMAND:
                    int cmd = m.WParam.ToInt32() & 0xfff0;
                    if ( cmd == SC_MINIMIZE )
                    {
                        
                    }
                break;
            }
            base.WndProc( ref m );
        }*/
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

            m3u8FileUrlTextBox.Focus();
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
                    NameCleaner.ResetExcludesWords( f.FileNameExcludesWords );

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
//Task.Delay( 5000 ).Wait( cts.Token );

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
                    #region comm. prev.
                    /*
                    //-2-//
                    var stepAction_UI = new StepActionDelegate( (p) =>
                    {
                        //$"#{n} of {totalPatrs}). '{part.RelativeUrlName}'..."
                        m3u8FileResultTextBox.AppendText( p.Message );
                        if ( !p.Success ) m3u8FileResultTextBox.AppendText( " ----FAILED" );
                        m3u8FileResultTextBox.AppendText( Environment.NewLine );

                        _Wb.IncreaseSteps();
                    } );
                    var stepAction = new StepActionDelegate( (p) => this.BeginInvoke( stepAction_UI, p ) );

                    var progressStepAction_UI = new ProgressStepActionDelegate( (p) =>
                    {
                        endStepActionLabel.Text = $"received {p.SuccessReceivedPartCount} of {p.TotalPartCount}";
                        if ( p.FailedReceivedPartCount != 0 )
                        {
                            endStepActionLabel.Text += $", (failed: {p.FailedReceivedPartCount})";
                        }
                    } );
                    var progressStepAction = new ProgressStepActionDelegate( (p) => this.BeginInvoke( progressStepAction_UI, p ) );
                    var ip = new download_m3u8File_params_t()
                    {
                        mc                     = _Mc,
                        m3u8File               = m3u8File,
                        cts                    = _Cts,
                        maxDegreeOfParallelism = _MaxDegreeOfParallelism,
                        stepAction             = stepAction,
                        progressStepAction     = progressStepAction,         
                    };
                    var downloadParts = download_m3u8File_parallel( ip );

                    //-3-//
                    #region [.write output file.]
                    using ( var fs = File.OpenWrite( outputFileName ) )
                    {
                        fs.SetLength( 0 );

                        foreach ( var downloadPart in downloadParts )
                        {
                            if ( downloadPart.Error != null ) //|| downloadPart.Bytes == null )
                            {
                                continue;
                            }
                            var bytes = downloadPart.Bytes;
                            fs.Write( bytes, 0, bytes.Length );
                        }
                    }
                    #endregion
                        
                    var r = new m3u8FileWholeLoadAndSaveResult()
                    {
                        TotalParts            = downloadParts.Count,
                        DownloadedWritedParts = downloadParts.Count( p => p.Error == null ),
                        OutputFileName        = outputFileName,
                        TotalBytes            = downloadParts.SumEx( p => (p.Bytes?.Length).GetValueOrDefault() ),
                    };
                    return (r);
                    */
                    #endregion

                    //-2-//
                    var stepAction_UI = new m3u8_processor.StepActionDelegate( (p) =>
                    {
                        m3u8FileResultTextBox.AppendText( $"#{p.PartOrderNumber} of {p.TotalPartCount}). '{p.Part.RelativeUrlName}'..." );
                        if ( !p.Success )
                        {
                            m3u8FileResultTextBox.AppendText( $" => '{p.Error.Message}' ----FAILED");
                        }
                        m3u8FileResultTextBox.AppendText( Environment.NewLine );

                        _Wb.IncreaseSteps();
                    } );
                    var stepAction = new m3u8_processor.StepActionDelegate( (p) => this.BeginInvoke( stepAction_UI, p ) );

                    var progressStepAction_UI = new m3u8_processor.ProgressStepActionDelegate( (p) =>
                    {
                        endStepActionLabel.Text = $"received {p.SuccessReceivedPartCount} of {p.TotalPartCount}";
                        if ( p.FailedReceivedPartCount != 0 )
                        {
                            endStepActionLabel.Text += $", (failed: {p.FailedReceivedPartCount})";
                        }
                    } );
                    var progressStepAction = new m3u8_processor.ProgressStepActionDelegate( (p) => this.BeginInvoke( progressStepAction_UI, p ) );
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

        #region comm. prev.
        /*
        /// <summary>
        /// 
        /// </summary>
        private struct m3u8FileWholeLoadAndSaveResult
        {
            public int    TotalParts { get; set; }
            public int    DownloadedWritedParts { get; set; }
            public string OutputFileName { get; set; }
            public long   TotalBytes { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private struct StepActionParams
        {
            public int    TotalPartCount  { get; private set; }
            public int    PartOrderNumber { get; private set; }
            public string Message         { get; private set; }
            public bool   Success         { get; private set; }
            public m3u8_part_ts Part      { get; private set; }

            public StepActionParams SetMessage( string message )
            {
                Message = message;
                return (this);
            }
            public StepActionParams SetMessageFailed( string message )
            {
                Message = message;
                Success = false;
                return (this);
            }

            public static StepActionParams CreateSuccess( int totalPartCount, int partOrderNumber, m3u8_part_ts part )
            {
                var o = new StepActionParams() { TotalPartCount = totalPartCount, Success = true, PartOrderNumber = partOrderNumber, Part = part };
                return (o);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private delegate void StepActionDelegate( StepActionParams p );
        /// <summary>
        /// 
        /// </summary>
        private struct ProgressStepActionParams
        {
            public ProgressStepActionParams( int totalPartCount ) : this()
            {
                TotalPartCount = totalPartCount;
            }

            public int TotalPartCount { get; private set; }
            public int SuccessReceivedPartCount { get; internal set; }
            public int FailedReceivedPartCount  { get; internal set; }
        }
        /// <summary>
        /// 
        /// </summary>
        private delegate void ProgressStepActionDelegate( ProgressStepActionParams p );
        /// <summary>
        /// 
        /// </summary>
        private struct download_m3u8File_params_t
        {
            public m3u8_client mc { get; set; }
            public m3u8_file_t m3u8File { get; set; }
            public CancellationTokenSource cts { get; set; }
            public StepActionDelegate stepAction { get; set; }
            public ProgressStepActionDelegate progressStepAction { get; set; }
            public int maxDegreeOfParallelism { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        private static IReadOnlyCollection< m3u8_part_ts > download_m3u8File_parallel( download_m3u8File_params_t ip )
        {
            var ct = (ip.cts?.Token).GetValueOrDefault( CancellationToken.None );
            var baseAddress = ip.m3u8File.BaseAddress;
            var totalPatrs  = ip.m3u8File.Parts.Count;
            var globalPartNumber = 0;
            var successReceivedPartCount = 0;
            var failedReceivedPartCount  = 0;
            var downloadPartsSet = new SortedSet< m3u8_part_ts >( default(m3u8_part_ts_comparer) );

            ip.progressStepAction?.Invoke( new ProgressStepActionParams( totalPatrs ) );

            using ( DefaultConnectionLimitSaver.Create( ip.maxDegreeOfParallelism ) )
            {
                Parallel.ForEach( ip.m3u8File.Parts, new ParallelOptions() { MaxDegreeOfParallelism = ip.maxDegreeOfParallelism, CancellationToken = ct }, 
                (part, loopState, idx) =>
                {
                    var n = Interlocked.Increment( ref globalPartNumber );
                    var p = StepActionParams.CreateSuccess( totalPatrs, n, part );
                    try
                    {
                        ip.stepAction?.Invoke( p.SetMessage( $"#{n} of {totalPatrs}). '{part.RelativeUrlName}'..." ) );

                        var downloadPart = ip.mc.DownloadPart( part, baseAddress, ct ).Result;

                        var ep = new ProgressStepActionParams( totalPatrs );
                        if ( (downloadPart.Error != null) && !ct.IsCancellationRequested )
                        {
                            ip.stepAction?.Invoke( p.SetMessageFailed( $"#{n} of {totalPatrs}). FAILED: {downloadPart.Error}{Environment.NewLine}" ) );
                            ep.SuccessReceivedPartCount = successReceivedPartCount;
                            ep.FailedReceivedPartCount  = Interlocked.Increment( ref failedReceivedPartCount );
                        }
                        else
                        {
                            ep.SuccessReceivedPartCount = Interlocked.Increment( ref successReceivedPartCount );
                            ep.FailedReceivedPartCount  = failedReceivedPartCount;
                        }
                        ip.progressStepAction?.Invoke( ep );

                        lock ( downloadPartsSet )
                        {
                            downloadPartsSet.Add( downloadPart );
                        }
                    }
                    catch ( Exception ex )
                    {
                        #region [.code.]
                        var aex = ex as AggregateException;
                        if ( (aex == null) || !aex.InnerExceptions.All( (_ex) => (_ex is OperationCanceledException /*TaskCanceledException* /) ) )
                        {
                            ip.stepAction?.Invoke( p.SetMessageFailed( "ERROR: " + ex ) );
                        }
                        #endregion
                    }
                });
            }

            ct.ThrowIfCancellationRequested();

            return (downloadPartsSet);
        }
        */
        #endregion
    }
}
