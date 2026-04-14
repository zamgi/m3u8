using System;
using System.Collections.Generic;
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
        private const string APP_TITLE      = ".m3u8 file downloader";
        private const string M3U8_EXTENSION = ".m3u8";

        private m3u8_client             _Mc;
        private CancellationTokenSource _Cts;
        private WaitBannerUC            _Wb;

        private (string M3u8FileUrl, bool AutoStartDownload) _InputParams;
        private string               _OutputFileName;
        private M3u8FileResultUCBase _m3U8FileResultUC;
        private Settings _Settings;
        #endregion

        #region [.ctor().]
        public MainForm()
        {
            InitializeComponent();
            this.Text = APP_TITLE;
            _Settings = Settings.Default;
            //-------------------------------------//
            this.DownloadLogUIType = _Settings.DownloadLogUIType.Try2Enum< DownloadLogUITypeEnum >().GetValueOrDefault( DownloadLogUITypeEnum.GridViewUIType );
            //-------------------------------------//

            m3u8FileUrlTextBox_TextChanged( this, EventArgs.Empty );
            autoMinimizeWindowWhenStartsDownloadLabel_set();
            autoCloseApplicationWhenEndsDownloadLabel_set();
            parallelismLabel_set();
            settingsLabel_set();

            NameCleaner.ResetExcludesWords( _Settings.NameCleanerExcludesWords?.Cast< string >() );
        }
        public MainForm( in (string m3u8FileUrl, bool autoStartDownload) inputParams ) : this() => _InputParams = inputParams;

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();

                _Mc?.Dispose();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.override methods.]
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !base.DesignMode && _Settings.StoreMainFormPosition )
            {
                FormPositionStorer.Load( this, _Settings.MainFormPositionJson );
            }
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            if ( !base.DesignMode && _Settings.StoreMainFormPosition )
            {
                _Settings.MainFormPositionJson = FormPositionStorer.Save( this );
                _Settings.SaveNoThrow();
            }
        }
        protected override void OnShown( EventArgs e )
        {
            base.OnShown( e );

            if ( !_InputParams.M3u8FileUrl.IsNullOrWhiteSpace() )
            {
                m3u8FileUrlTextBox.Text = _InputParams.M3u8FileUrl;
                return;
            }

            try
            {
                var text = Clipboard.GetText( TextDataFormat.Text )?.Trim();
                if ( text.IsNullOrEmpty() )
                    text = Clipboard.GetText( TextDataFormat.UnicodeText )?.Trim();
                if ( text.IsNullOrEmpty() )
                    return;
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
                        m3u8FileWholeLoadAndSave();
                    }
                    return;


                    case Keys.G:
                    {
                        e.SuppressKeyPress = true;
                        outputFileNameTextBox_Click( this, EventArgs.Empty );
                    }
                    return;
                }
            }

            base.OnKeyDown( e );
        }
        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );

            //still downloading?
            if ( this.IsDownloading )
            {
                if ( this.WindowState == FormWindowState.Minimized )
                {
                    this.WindowState = FormWindowState.Normal;
                }
                if ( this.MessageBox_ShowQuestion( "Dou you want to [CANCEL] downloading and exit ?", APP_TITLE, MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2 ) == DialogResult.Yes )
                {
                    _Cts?.Cancel();

                    //waiting for all canceled and becomes finished
                    do
                    {
                        Application.DoEvents();
                    }
                    while ( this.IsDownloading );

                    Extensions.DeleteFile_NoThrow( _OutputFileName );
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
        #endregion

        #region [.downloading in progress.]
        private bool IsDownloading => (_Mc != null);
        #endregion

        #region [.DownloadLogUITypeEnum.]
        private DownloadLogUITypeEnum DownloadLogUIType
        {
            get => ((_m3U8FileResultUC is M3u8FileResultTextBox) ? DownloadLogUITypeEnum.TextBoxUIType : DownloadLogUITypeEnum.GridViewUIType);
            set
            {
                if ( (_m3U8FileResultUC == null) || (this.DownloadLogUIType != value) )
                {
                    switch ( value )
                    {
                        case DownloadLogUITypeEnum.GridViewUIType: _m3U8FileResultUC = new M3u8FileResultDGV    (); break;
                        case DownloadLogUITypeEnum.TextBoxUIType : _m3U8FileResultUC = new M3u8FileResultTextBox(); break;
                        default: throw (new ArgumentException( value.ToString() ));
                    }

                    _m3U8FileResultUC.Dock = DockStyle.Fill;

                    m3u8FileResultPanel.Controls.Clear();
                    m3u8FileResultPanel.Controls.Add( _m3U8FileResultUC );

                    _Settings.DownloadLogUIType = value.ToString();
                    _Settings.SaveNoThrow();
                }
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
                if ( outputFileName.IsNullOrEmpty() ) return;
                outputFileNameTextBox_Text = outputFileName;

                await Task.Delay( 500 );
                outputFileName = NameCleaner.Clean( outputFileName );
                outputFileNameTextBox_Text = outputFileName;

                await Task.Delay( 500 );
                if ( !outputFileName.EndsWith( _Settings.OutputFileExtension, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    if ( _Settings.OutputFileExtension.HasFirstCharNotDot() )
                    {
                        outputFileName += '.';
                    }
                    outputFileName += _Settings.OutputFileExtension;
                }
                outputFileNameTextBox_Text = outputFileName;

                #region [.check 'autoStartDownload'.]
                if ( _InputParams.AutoStartDownload )
                {
                    _InputParams.AutoStartDownload = false;
                    if ( !outputFileName.IsNullOrWhiteSpace() )
                    {
                        var fullOutputFileName = Path.Combine( _Settings.OutputFileDirectory, outputFileName );
                        m3u8FileWholeLoadAndSave( fullOutputFileName );
                    }
                }
                #endregion
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

        private void parallelismLabel_Click( object sender, EventArgs e )
        {
            using ( var f = new ParallelismForm() )
            {
                f.UseCrossAppInstanceDegreeOfParallelism = _Settings.UseCrossAppInstanceDegreeOfParallelism;
                f.MaxDegreeOfParallelism                 = _Settings.MaxDegreeOfParallelism;
                f.IsInfinity                             = (_Settings.MaxDegreeOfParallelism == int.MaxValue);
                f.MaxDownloadAppInstance                 = _Settings.MaxDownloadAppInstance;
                if ( f.ShowDialog() == DialogResult.OK )
                {
                    _Settings.UseCrossAppInstanceDegreeOfParallelism = f.UseCrossAppInstanceDegreeOfParallelism;
                    _Settings.MaxDegreeOfParallelism                 = f.MaxDegreeOfParallelism;
                    _Settings.MaxDownloadAppInstance                 = f.MaxDownloadAppInstance;
                    _Settings.SaveNoThrow();
                    parallelismLabel_set();
                }
            }
        }
        private void parallelismLabel_EnabledChanged( object sender, EventArgs e )
        {
            if ( _Settings.UseCrossAppInstanceDegreeOfParallelism )
            {
                parallelismLabel.BackColor = (parallelismLabel.Enabled ? Color.DimGray : Color.FromKnownColor( KnownColor.Control ));
            }
        }
        private void excludesWordsLabel_Click( object sender, EventArgs e )
        {
            using ( var f = new FileNameExcludesWordsEditor( NameCleaner.ExcludesWords ) )
            {
                if ( f.ShowDialog() == DialogResult.OK )
                {
                    NameCleaner.ResetExcludesWords( f.GetFileNameExcludesWords() );

                    if ( _Settings.NameCleanerExcludesWords == null )
                    {
                        _Settings.NameCleanerExcludesWords = new StringCollection();
                    }
                    else
                    {
                        _Settings.NameCleanerExcludesWords.Clear();
                    }                    
                    _Settings.NameCleanerExcludesWords.AddRange( NameCleaner.ExcludesWords.ToArray() );
                    _Settings.SaveNoThrow();
                }
            }
        }
        private void autoMinimizeWindowWhenStartsDownloadLabel_Click( object sender, EventArgs e )
        {
            _Settings.AutoMinimizeWindowWhenStartsDownload = !_Settings.AutoMinimizeWindowWhenStartsDownload;
            _Settings.SaveNoThrow();
            autoMinimizeWindowWhenStartsDownloadLabel_set();
        }
        private void autoCloseApplicationWhenEndsDownloadLabel_Click( object sender, EventArgs e )
        {
            _Settings.AutoCloseApplicationWhenEndsDownload = !_Settings.AutoCloseApplicationWhenEndsDownload;
            _Settings.SaveNoThrow();
            autoCloseApplicationWhenEndsDownloadLabel_set();
        }
        private void settingsLabel_Click( object sender, EventArgs e )
        {
            using ( var f = new SettingsForm() )
            {
                f.AttemptRequestCountByPart     = _Settings.AttemptRequestCountByPart;
                f.RequestTimeoutByPart          = _Settings.RequestTimeoutByPart;
                f.StoreMainFormPosition         = _Settings.StoreMainFormPosition;
                f.DownloadLogUIType             = this.DownloadLogUIType;
                f.ShowOnlyRequestRowsWithErrors = _m3U8FileResultUC.ShowOnlyRequestRowsWithErrors;

                if ( f.ShowDialog() == DialogResult.OK )
                {
                    _Settings.AttemptRequestCountByPart      = f.AttemptRequestCountByPart;
                    _Settings.RequestTimeoutByPart           = f.RequestTimeoutByPart;
                    _Settings.StoreMainFormPosition          = f.StoreMainFormPosition;
                    this.DownloadLogUIType                          = f.DownloadLogUIType;
                    _m3U8FileResultUC.ShowOnlyRequestRowsWithErrors = f.ShowOnlyRequestRowsWithErrors;
                    _Settings.SaveNoThrow();
                    settingsLabel_set();
                }
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

        private void autoMinimizeWindowWhenStartsDownloadLabel_set()
        {
            autoMinimizeWindowWhenStartsDownloadLabel.Image     = (_Settings.AutoMinimizeWindowWhenStartsDownload ? Resources._checked : Resources._unchecked).ToBitmap();
            autoMinimizeWindowWhenStartsDownloadLabel.ForeColor = (_Settings.AutoMinimizeWindowWhenStartsDownload ? Color.Empty : Color.DimGray);
        }

        private void autoCloseApplicationWhenEndsDownloadLabel_set()
        {
            autoCloseApplicationWhenEndsDownloadLabel.Image     = (_Settings.AutoCloseApplicationWhenEndsDownload ? Resources._checked : Resources._unchecked).ToBitmap();
            autoCloseApplicationWhenEndsDownloadLabel.ForeColor = (_Settings.AutoCloseApplicationWhenEndsDownload ? Color.Empty : Color.DimGray);
        }

        private void parallelismLabel_set()
        {
            var maxDownloadAppInstance   = (_Settings.MaxDownloadAppInstance.HasValue ? $"\r\napp-instance download:  {_Settings.MaxDownloadAppInstance} " : null);
            parallelismLabel.Text        = $"degree of parallelism:  {((_Settings.MaxDegreeOfParallelism == int.MaxValue) ? "Infinity" : _Settings.MaxDegreeOfParallelism.ToString())} "
                                           + maxDownloadAppInstance;
            parallelismLabel.ToolTipText = $"use cross app-instance parallelism:  {_Settings.UseCrossAppInstanceDegreeOfParallelism.ToString().ToLower()}";

            parallelismLabel.ForeColor   = _Settings.UseCrossAppInstanceDegreeOfParallelism ? Color.White   : Color.FromKnownColor( KnownColor.ControlText );
            parallelismLabel.BackColor   = _Settings.UseCrossAppInstanceDegreeOfParallelism ? Color.DimGray : Color.FromKnownColor( KnownColor.Control );
            //--------------------------------------------//

            exceptionWordsLabel.Text = (_Settings.MaxDownloadAppInstance.HasValue ? "file name exception\r\nword editor" : "file name exceptions");
        }

        private void settingsLabel_set() =>
            settingsLabel.ToolTipText = $"other settings =>\r\n attempt request count by part:  {_Settings.AttemptRequestCountByPart}\r\n request timeout by part:  {_Settings.RequestTimeoutByPart}";
        #endregion

        #region [.m3u8FileTextContentLoadButton_Click.]
        private void m3u8FileTextContentLoadButton_Click( object sender, EventArgs e ) => m3u8FileTextContentLoad();

        private async void m3u8FileTextContentLoad()
        {
            #region [.still downloading?.]
            if ( this.IsDownloading )
            {
                return;
            } 
            #endregion

            #region [.url.]
            var m3u8FileUrl = TryGet_m3u8FileUrl( out var error );
            if ( error != null )
            {
                Output2ResultUC_WithDelay( error );
                return;
            }
            #endregion
            //----------------------------------------------------//

            try
            {
                BeginOpAction( attemptRequestCountByPart: 1 );

                try
                {
                    var m3u8File = await _Mc.DownloadFile( m3u8FileUrl, _Cts.Token );

                    Output2ResultUC( m3u8File );
                }
                catch ( Exception innerEx )
                {
                    Output2ResultUC_IfNotIsCancellationRequested( innerEx );
                }

                FinishOpAction( m3u8FileTextContentLoadButton );
            }
            catch ( Exception outerEx )
            {
                FinishOpAction_With_MessageBox_ShowError( outerEx, m3u8FileTextContentLoadButton );
            }
        }
        #endregion

        #region [.m3u8FileWholeLoadAndSaveButton_Click.]
        private void m3u8FileWholeLoadAndSaveButton_Click( object sender, EventArgs e ) => m3u8FileWholeLoadAndSave();

        private async void m3u8FileWholeLoadAndSave( string outputFileName = null )
        {
            #region [.still downloading?.]
            if ( this.IsDownloading )
            {
                return;
            }
            #endregion

            #region [.url.]
            var m3u8FileUrl = TryGet_m3u8FileUrl( out var error );
            if ( error != null )
            {
                Output2ResultUC_WithDelay( error );
                return;
            }
            #endregion

            #region [.save file-name dialog.]
            if ( outputFileName == null )
            {
                using ( var sfd = new SaveFileDialog() { InitialDirectory = _Settings.OutputFileDirectory,
                                                         DefaultExt       = _Settings.OutputFileExtension,
                                                         AddExtension     = true, } )
                {
                    sfd.FileName = PathnameCleaner.CleanPathnameAndFilename( outputFileNameTextBox_Text ).TrimStart( '-' );
                    if ( sfd.ShowDialog() != DialogResult.OK )
                    {
                        return;
                    }

                    _OutputFileName = sfd.FileName;
                    _Settings.OutputFileDirectory = Path.GetDirectoryName( _OutputFileName );
                    _Settings.SaveNoThrow();
                    outputFileNameTextBox_Text = Path.GetFileName( _OutputFileName );
                    _m3U8FileResultUC.SetFocus();
                }
            }
            else
            {
                _OutputFileName = outputFileName;
                _m3U8FileResultUC.SetFocus();
            }
            #endregion
            //----------------------------------------------------//

            #region [.auto minimize window when starts download.]
            if ( _Settings.AutoMinimizeWindowWhenStartsDownload )
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

                    Output2ResultUC( m3u8File );

                    //-2-//
                    await Task.Delay( 3000, _Cts.Token );
                    UnsetResultUC();

                    _Wb.SetTotalSteps( m3u8File.Parts.Count );

                    //-3-//
                    try
                    {
                        var anyErrorHappend = false;
                        var res = await Task.Run( () =>
                        {
                            var rows_Dict = new Dictionary< int, IRowHolder >( m3u8File.Parts.Count );
                            var vscrollBarVisible = false;

                            var requestStepAction_UI = new m3u8_processor.RequestStepActionDelegate( p =>
                            {
                                var requestText = $"#{p.PartOrderNumber} of {p.TotalPartCount}). '{p.Part.RelativeUrlName}'...";
                                if ( p.Success )
                                {
                                    var row = _m3U8FileResultUC.AppendRequestText( requestText );
                                    if ( !vscrollBarVisible && _m3U8FileResultUC.IsVerticalScrollBarVisible )
                                    {
                                        vscrollBarVisible = true;
                                        _m3U8FileResultUC.AdjustColumnsWidthSprain();
                                    }
                                    rows_Dict.Add( p.Part.OrderNumber, row );
                                }
                                else
                                {
                                    anyErrorHappend = true;
                                    _m3U8FileResultUC.AppendRequestAndResponseErrorText( requestText, p.Error );
                                }
                            });
                            var requestStepAction = new m3u8_processor.RequestStepActionDelegate( p => this.BeginInvoke( requestStepAction_UI, p ) );

                            var sw_download = new Stopwatch();
                            var totalBytesLength = 0L;
                            var responseStepAction_UI = new m3u8_processor.ResponseStepActionDelegate( p =>
                            {
                                responseStepActionLabel.Text = $"received {p.SuccessReceivedPartCount} of {p.TotalPartCount}";
                                if ( p.FailedReceivedPartCount != 0 )
                                {
                                    responseStepActionLabel.Text += $", [failed: {p.FailedReceivedPartCount}]";
                                }

                                if ( rows_Dict.TryGetValue( p.Part.OrderNumber, out var row ) )
                                {
                                    rows_Dict.Remove( p.Part.OrderNumber );
                                    if ( p.Part.Error != null )
                                    {
                                        anyErrorHappend = true;
                                        _m3U8FileResultUC.SetResponseErrorText( row, p.Part.Error );
                                    }
                                    else
                                    {
                                        _m3U8FileResultUC.SetResponseReceivedText( row, "received" );
                                    }
                                }

                                #region [.speed.]
                                totalBytesLength += p.BytesLength;
                                var speedText = default(string);
                                if ( !sw_download.IsRunning )
                                {
                                    sw_download.Start();
                                }
                                else
                                {
                                    var elapsedSeconds = (sw_download.ElapsedMilliseconds / 1000.0);
                                    if ( (1_024 < totalBytesLength) && (2.5 <= elapsedSeconds) )
                                    {
                                        speedText = Extensions.GetSpeedText( totalBytesLength, elapsedSeconds );
                                        responseStepActionLabel.Text += $", [speed: {speedText}]";
                                    }
                                }

                                _Wb.IncreaseSteps( speedText );                                
                                #endregion
                            });
                            var responseStepAction = new m3u8_processor.ResponseStepActionDelegate( p => this.BeginInvoke( responseStepAction_UI, p ) );

                            var ip = new m3u8_processor.DownloadPartsAndSaveInputParams()
                            {
                                mc                                     = _Mc,
                                m3u8File                               = m3u8File,
                                OutputFileName                         = _OutputFileName,
                                CancellationToken                      = _Cts.Token,
                                MaxDegreeOfParallelism                 = _Settings.MaxDegreeOfParallelism,
                                RequestStepAction                      = requestStepAction,
                                ResponseStepAction                     = responseStepAction,
                                UseCrossAppInstanceDegreeOfParallelism = _Settings.UseCrossAppInstanceDegreeOfParallelism,
                                MaxDownloadAppInstance                 = _Settings.MaxDownloadAppInstance,
                                WaitingForOtherAppInstanceFinished     = () => this.BeginInvoke( new Action( () => _Wb.WaitingForOtherAppInstanceFinished() ) ),
                            };                            
                            var result = m3u8_processor.DownloadPartsAndSave( ip );
                            if ( sw_download.IsRunning ) sw_download.Stop();
                            return (result);
                        });

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
                                if ( string.Compare( res.OutputFileName, _OutputFileName, true ) != 0 )
                                {
                                    Extensions.DeleteFile_NoThrow( _OutputFileName );
                                }
                                File.Move( res.OutputFileName, _OutputFileName );
                                res.ResetOutputFileName( _OutputFileName );
                            }
                            catch ( Exception ex )
                            {
                                renameOutputFileException = ex;
                            }
                        }
                        #endregion

                        static string to_text_format( ulong size ) => (0 < size) ? size.ToString( "0,0" ) : "0";

                        _m3U8FileResultUC.AppendEmptyLine();
                        _m3U8FileResultUC.AppendRequestText( $" downloaded & writed parts {res.PartsSuccessCount} of {res.TotalParts}" );
                        _m3U8FileResultUC.AppendEmptyLine();
                        _m3U8FileResultUC.AppendRequestText( $" elapsed: {sw.Elapsed}" );
                        _m3U8FileResultUC.AppendRequestText( $"         file: '{res.OutputFileName}'" );
                        _m3U8FileResultUC.AppendRequestText( $"       size: {to_text_format( res.TotalBytes >> 20 )} mb" );

                        FinishOpAction( _m3U8FileResultUC );

                        #region [.error rename MessageBox.]
                        if ( renameOutputFileException != null )
                        {
                            this.MessageBox_ShowError( $"Rename output file error => '{renameOutputFileException}'", this.Text );
                        }
                        #endregion

                        this.MessageBox_ShowInformation( $"SUCCESS.\r\n\r\nelapsed: {sw.Elapsed}\r\n       file: '{res.OutputFileName}'\r\n      size: {to_text_format( res.TotalBytes >> 20 )} mb.", this.Text );

                        #region [.auto close application when ends download.]
                        if ( _Settings.AutoCloseApplicationWhenEndsDownload && !anyErrorHappend && (renameOutputFileException == null) )
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
                            var mex = ex as m3u8_Exception;
                            if ( mex != null )
                            {
                                Extensions.DeleteFile_NoThrow( _OutputFileName );
                                Append2ResultUC( mex );
                            }
                            else
                            {
                                Append2ResultUC( ex );
                            }
                        }

                        FinishOpAction( _m3U8FileResultUC );
                    }
                }
                catch ( Exception innerEx )
                {
                    Output2ResultUC_IfNotIsCancellationRequested( innerEx );

                    FinishOpAction( _m3U8FileResultUC );
                }
            }
            catch ( Exception outerEx )
            {
                FinishOpAction_With_MessageBox_ShowError( outerEx, _m3U8FileResultUC );
            }
        }
        #endregion

        #region [.all other method's.]
        private Uri TryGet_m3u8FileUrl( out Exception error )
        {
            try
            {
                var m3u8FileUrlText = m3u8FileUrlTextBox.Text.Trim();
                var m3u8FileUrl = new Uri( m3u8FileUrlText );
                if ( (m3u8FileUrl.Scheme != Uri.UriSchemeHttp) && (m3u8FileUrl.Scheme != Uri.UriSchemeHttps) )
                {
                    throw (new ArgumentException( $"Only '{Uri.UriSchemeHttp}' and '{Uri.UriSchemeHttps}' schemes are allowed.", nameof( m3u8FileUrl ) ));
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

        private void BeginOpAction( int? attemptRequestCountByPart = null )
        {
            SetEnabledUI( false );
            UnsetResultUC();

            _Mc  = m3u8_client_factory.Create( webProxy: null, _Settings.RequestTimeoutByPart, 
                                               attemptRequestCountByPart.GetValueOrDefault( _Settings.AttemptRequestCountByPart )
                                             );
            _Cts = new CancellationTokenSource();
            _Wb  = WaitBannerUC.Create( this, _Cts );
        }
        private void FinishOpAction( Control control4SetFocus = null )
        {
            if ( (_Cts?.IsCancellationRequested).GetValueOrDefault() )
            {
                _m3U8FileResultUC.AppendEmptyLine();
                _m3U8FileResultUC.AppendRequestErrorText( ".....Canceled by User....." );
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

        private void UnsetResultUC()
        {
            _m3U8FileResultUC.Clear();
            responseStepActionLabel.Text = string.Empty;
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

            exceptionWordsLabel.Enabled = enabled;
            parallelismLabel  .Enabled = enabled;
            settingsLabel     .Enabled = enabled;
        }

        private void Output2ResultUC( m3u8_file_t m3u8File )
        {
            var lines = m3u8File.RawText?.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries )
                                .Where( line => !line.IsNullOrWhiteSpace() );

            _m3U8FileResultUC.Output( m3u8File, lines );
        }
        private void Output2ResultUC( Exception ex )
        {
            _m3U8FileResultUC.Clear();
            _m3U8FileResultUC.AppendRequestErrorText( ex );
            _m3U8FileResultUC.AdjustColumnsWidthSprain();
        }
        private async void Output2ResultUC_WithDelay( Exception ex, int millisecondsDelay = 250 )
        {
            _m3U8FileResultUC.Clear();
            await Task.Delay( millisecondsDelay );
            Output2ResultUC( ex );
        }
        private void Output2ResultUC_IfNotIsCancellationRequested( Exception ex )
        {
            if ( !_Cts.IsCancellationRequested )
            {
                Output2ResultUC( ex );
            }
        }
        private void Append2ResultUC( Exception ex )
        {
            _m3U8FileResultUC.AppendEmptyLine();
            _m3U8FileResultUC.AppendRequestErrorText( ex );
            _m3U8FileResultUC.AdjustColumnsWidthSprain();
        }
        private void Append2ResultUC( m3u8_Exception ex )
        {
            _m3U8FileResultUC.AppendEmptyLine();
            _m3U8FileResultUC.AppendRequestErrorText( ex.Message );
            _m3U8FileResultUC.AdjustColumnsWidthSprain();
        }
        #endregion

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
                    if ( !ext.Equals( _Settings.OutputFileExtension, StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        outputFileName += _Settings.OutputFileExtension;
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
