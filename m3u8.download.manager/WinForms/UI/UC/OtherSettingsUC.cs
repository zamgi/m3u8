using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.client;
using m3u8.download.manager.controllers;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.Properties;

using WinTimer = System.Windows.Forms.Timer;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class OtherSettingsUC : UserControl
    {
        #region [.fields.]
        private DownloadController _DownloadController;
        private string _ExternalProgFilePath_InitValue;
        private string _FFmpegFileLocation_InitValue; 
        private WinTimer _GetTotalMemoryTimer;
        private CancellationTokenSource _CalcReceivedAndWritedPartsTask_Cts;
        private IReceivedAndWritedPartsProcessor _ReceivedAndWritedPartsProcessor;
        #endregion

        #region [.ctor().]
        public OtherSettingsUC()
        {
            InitializeComponent();

            this.SetForeColor4ParentOnly< GroupBox >( Color.DodgerBlue );
            currentMemoryLabel.ForeColor = Color.DimGray;
            receivedAndWritedPartsClearAllButton.ForeColor = Color.Maroon;
        }
        public OtherSettingsUC( DownloadController dc, IReceivedAndWritedPartsProcessor receivedAndWritedPartsProcessor ) : this() => Init( dc, receivedAndWritedPartsProcessor );
        public void Init( DownloadController dc, IReceivedAndWritedPartsProcessor receivedAndWritedPartsProcessor )
        {
            _DownloadController = dc ?? throw (new ArgumentNullException( nameof(dc) ));
            _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged;
            _DownloadController.IsDownloadingChanged += DownloadController_IsDownloadingChanged;
            _ReceivedAndWritedPartsProcessor = receivedAndWritedPartsProcessor;            

            DownloadController_IsDownloadingChanged( _DownloadController.IsDownloading );
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                if ( _DownloadController != null )
                {
                    _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged;
                }

                _CalcReceivedAndWritedPartsTask_Cts?.Dispose_NoThrow();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.public methods.]
        public void OnShown() 
        { 
            _ExternalProgFilePath_InitValue = this.ExternalProgFilePath; 
            _FFmpegFileLocation_InitValue   = this.FFmpegFileLocation;

            CheckBoxImitationDisabled.Set_AllCheckBox_ForeColorAndImage( this.Controls );
        }
        public void OnClosing( DialogResult dialogResult, CancelEventArgs e )
        {
            if ( dialogResult == DialogResult.OK )
            {
                if ( this.OutputFileExtension.IsNullOrEmpty() )
                {
                    e.Cancel = true;
                    outputFileExtensionTextBox.FocusAndBlinkBackColor();
                }

                var externalProgFilePath = this.ExternalProgFilePath;
                if ( (_ExternalProgFilePath_InitValue != externalProgFilePath) && !externalProgFilePath.IsNullOrEmpty() && !File.Exists( externalProgFilePath ) )
                {
                    if ( this.MessageBox_ShowQuestion( $"External program file doesn't exists:\r\n\r\n'{externalProgFilePath}'.\r\n\r\nContinue?", "External program" ) != DialogResult.Yes )
                    {
                        e.Cancel = true;
                        return;
                    }
                    //e.Cancel = true;
                    //externalProgFilePathTextBox.FocusAndBlinkBackColor();
                }
                if ( this.ExternalProgCaption.IsNullOrEmpty() )
                {
                    this.ExternalProgCaption = GetFileName_NoThrow( externalProgFilePath );
                }

                var ffmpegFilePath = this.FFmpegFileLocation;
                if ( (_FFmpegFileLocation_InitValue != ffmpegFilePath) && !ffmpegFilePath.IsNullOrEmpty() && !File.Exists( ffmpegFilePath ) )
                {
                    if ( this.MessageBox_ShowQuestion( $"FFmpeg program file doesn't exists:\r\n\r\n'{ffmpegFilePath}'.\r\n\r\nContinue?", "FFmpeg converter" ) != DialogResult.Yes )
                    {
                        e.Cancel = true;
                        return;
                    }
                    //e.Cancel = true;
                    //ffmpegFilePathTextBox.FocusAndBlinkBackColor();
                }
                if ( this.FFmpegConverterCaption.IsNullOrEmpty() )
                {
                    this.FFmpegConverterCaption = GetFileName_NoThrow( ffmpegFilePath );
                }
            }

            if ( !e.Cancel )
            {
                _CalcReceivedAndWritedPartsTask_Cts?.Cancel_NoThrow();
            }
        }
        public void ActiveteTab()
        {
            StartShowTotalMemory();
            StartCalcReceivedAndWritedParts();
        }
        #endregion

        #region [.public props.]
        public int      AttemptRequestCountByPart
        {
            get => attemptRequestCountByPartNUD.ValueAsInt32;
            set => attemptRequestCountByPartNUD.ValueAsInt32 = value;
        }
        public TimeSpan RequestTimeoutByPart
        {
            get => requestTimeoutByPartDTP.Value.TimeOfDay; // TimeSpan.FromTicks( (requestTimeoutByPartDTP.Value.TimeOfDay - requestTimeoutByPartDTP.MinDate.Date).Ticks );
            set => requestTimeoutByPartDTP.Value = requestTimeoutByPartDTP.MinDate.Date + value;
        }
        public bool     ShowOnlyRequestRowsWithErrors
        {
            get => showOnlyRequestRowsWithErrorsCheckBox.Checked;
            set => showOnlyRequestRowsWithErrorsCheckBox.Checked = value;
        }
        public bool     ShowDownloadStatisticsInMainFormTitle
        {
            get => showDownloadStatisticsInMainFormTitleCheckBox.Checked;
            set => showDownloadStatisticsInMainFormTitleCheckBox.Checked = value;
        }
        public bool     ShowAllDownloadsCompleted_Notification
        {
            get => showAllDownloadsCompleted_NotificationCheckBox.Checked;
            set => showAllDownloadsCompleted_NotificationCheckBox.Checked = value;
        }
        public bool     DownloadList_DrawOutputfileExistsMark
        {
            get => downloadList_DrawOutputfileExistsMarkCheckBox.Checked;
            set => downloadList_DrawOutputfileExistsMarkCheckBox.Checked = value;
        }
        public bool     UniqueUrlsOnly
        {
            get => uniqueUrlsOnlyCheckBox.Checked;
            set => uniqueUrlsOnlyCheckBox.Checked = value;
        }
        public string   OutputFileExtension
        {
            get => CorrectOutputFileExtension( outputFileExtensionTextBox.Text );
            set => outputFileExtensionTextBox.Text = CorrectOutputFileExtension( value );
        }
        public string   ExternalProgCaption
        {
            get => externalProgCaptionTextBox.Text?.Trim();
            set => externalProgCaptionTextBox.Text = value?.Trim();
        }
        public string   ExternalProgFilePath
        {
            get => externalProgFilePathTextBox.Text?.Trim();
            set => externalProgFilePathTextBox.Text = value?.Trim();
        }
        public bool     ExternalProgApplyByDefault
        {
            get => externalProgApplyByDefaultCheckBox.Checked;
            set
            {
                externalProgApplyByDefaultCheckBox.Checked = value;
                externalProgApplyByDefaultCheckBox_CheckedChanged( externalProgApplyByDefaultCheckBox, EventArgs.Empty );
            }
        }
        public string   FFmpegConverterCaption
        {
            get => ffmpegCaptionTextBox.Text?.Trim();
            set => ffmpegCaptionTextBox.Text = value?.Trim();
        }
        public string   FFmpegFileLocation
        {
            get => ffmpegFilePathTextBox.Text?.Trim();
            set => ffmpegFilePathTextBox.Text = value?.Trim();
        }
        public bool     FFmpegApplyByDefault
        {
            get => ffmpegApplyByDefaultCheckBox.Checked;
            set
            {
                ffmpegApplyByDefaultCheckBox.Checked = value;
                ffmpegApplyByDefaultCheckBox_CheckedChanged( ffmpegApplyByDefaultCheckBox, EventArgs.Empty );
            }
        }
        public int      FFmpegDegreeOfParallelism
        {
            get => ffmpegDegreeOfParallelismNUD.ValueAsInt32;
            set => ffmpegDegreeOfParallelismNUD.ValueAsInt32 = value;
        }
        public bool     FFmpeg_RenameAfterFFmpegConverter
        {
            get => ffmpeg_RenameAfterFFmpegConverterCheckBox.Checked;
            set
            {
                ffmpeg_RenameAfterFFmpegConverterCheckBox.Checked = value;
                ffmpeg_RenameAfterFFmpegConverterCheckBox_CheckedChanged( ffmpeg_RenameAfterFFmpegConverterCheckBox, EventArgs.Empty );
            }
        }
        public bool     FFmpeg_OpenAfterWithExternalProgRunner
        {
            get => ffmpeg_OpenAfterWithExternalProgRunnerCheckBox.Checked;
            set => ffmpeg_OpenAfterWithExternalProgRunnerCheckBox.Checked = value;
        }
        public bool     UseDirectorySelectDialogModern
        {
            get => useDirectorySelectDialogModernCheckBox.Checked;
            set => useDirectorySelectDialogModernCheckBox.Checked = value;
        }
        public bool     IgnoreHostHttpHeader
        {
            get => ignoreHostHttpHeaderCheckBox.Checked;
            set => ignoreHostHttpHeaderCheckBox.Checked = value;
        }
        #endregion

        #region [.private methods.]
        private static string CorrectOutputFileExtension( string ext )
        {
            ext = ext?.Trim();
            if ( !ext.IsNullOrEmpty() && ext.HasFirstCharNotDot() )
            {
                ext = '.' + ext;
            }
            return (ext);
        }

        private void DownloadController_IsDownloadingChanged( bool isDownloading )
        {
            only4NotRunLabel1.Visible =
                only4NotRunLabel2.Visible = isDownloading;
        }

        private void externalProgResetButton_Click( object sender, EventArgs e )
        {
            if ( this.FindForm().MessageBox_ShowQuestion( "Reset params to default ?", "External program", MessageBoxButtons.OKCancel, MessageBoxDefaultButton.Button1 ) == DialogResult.OK )
            {
                this.ExternalProgFilePath = Resources.ExternalProgFilePath;
                this.ExternalProgCaption  = Resources.ExternalProgCaption;
                this.ExternalProgApplyByDefault = false;
            }
        }
        private void externalProgFilePathTextBox_TextChanged( object sender, EventArgs e )
        {
            var externalProgFilePath = this.ExternalProgFilePath;
            if ( externalProgFilePath.IsNullOrEmpty() )
            {
                toolTip.SetToolTip( (Control) sender, null );
            }
            else
            {
                var allowed = File.Exists( externalProgFilePath ) ? "allowed" : "file doesn't exists !";
                toolTip.SetToolTip( (Control) sender, externalProgFilePath + $"  =>  ({allowed})" );

                if ( this.ExternalProgCaption.IsNullOrEmpty() )
                {
                    this.ExternalProgCaption = GetFileName_NoThrow( externalProgFilePath );
                }
            }
        }
        private void externalProgCaptionTextBox_TextChanged ( object sender, EventArgs e ) => toolTip.SetToolTip( (Control) sender, this.ExternalProgCaption );
        private void externalProgFilePathButton_Click( object sender, EventArgs e )
        {
            var externalProgFilePath = this.ExternalProgFilePath;
            using var ofd = new OpenFileDialog()
            {
                RestoreDirectory = true,
                Multiselect      = false,
                CheckFileExists  = true,                
                InitialDirectory = GetDirectoryName_NoThrow( externalProgFilePath )
            };
            if ( File.Exists( externalProgFilePath ) )
            {
                ofd.FileName = externalProgFilePath; //GetFileName_NoThrow( externalProgFilePath ), 
            }
            if ( ofd.ShowDialog( this ) == DialogResult.OK )
            {
                this.ExternalProgFilePath = ofd.FileName;
            }
        }
        private void externalProgApplyByDefaultCheckBox_CheckedChanged( object sender, EventArgs e ) => externalProgPictureBox.Enabled = externalProgApplyByDefaultCheckBox.Checked;

        private void ffmpegResetButton_Click( object sender, EventArgs e )
        {
            if ( this.FindForm().MessageBox_ShowQuestion( "Reset params to default ?", "FFmpeg converter", MessageBoxButtons.OKCancel, MessageBoxDefaultButton.Button1 ) == DialogResult.OK )
            {
                this.FFmpegFileLocation     = Resources.FFmpegFileLocation;
                this.FFmpegConverterCaption = Resources.FFmpegConverterCaption;
                this.FFmpegApplyByDefault                   = false;
                this.FFmpegDegreeOfParallelism              = 1;
                this.FFmpeg_RenameAfterFFmpegConverter      = false;
                this.FFmpeg_OpenAfterWithExternalProgRunner = false;
            }
        }
        private void ffmpegFilePathTextBox_TextChanged( object sender, EventArgs e )
        {
            var ffmpegFilePath = this.FFmpegFileLocation;
            if ( ffmpegFilePath.IsNullOrEmpty() )
            {
                toolTip.SetToolTip( (Control) sender, null );
            }
            else
            {
                var allowed = File.Exists( ffmpegFilePath ) ? "allowed" : "file doesn't exists !";
                toolTip.SetToolTip( (Control) sender, ffmpegFilePath + $"  =>  ({allowed})" );

                if ( this.FFmpegConverterCaption.IsNullOrEmpty() )
                {
                    this.FFmpegConverterCaption = GetFileName_NoThrow( ffmpegFilePath );
                }
            }
        }
        private void ffmpegCaptionTextBox_TextChanged ( object sender, EventArgs e ) => toolTip.SetToolTip( (Control) sender, this.FFmpegConverterCaption );
        private void ffmpegFilePathButton_Click( object sender, EventArgs e )
        {
            var ffmpegFilePath = this.FFmpegFileLocation;
            using var ofd = new OpenFileDialog()
            {
                RestoreDirectory = true,
                Multiselect      = false,
                CheckFileExists  = true,                
                InitialDirectory = GetDirectoryName_NoThrow( ffmpegFilePath )
            };
            if ( File.Exists( ffmpegFilePath ) )
            {
                ofd.FileName = ffmpegFilePath; //GetFileName_NoThrow( ffmpegFilePath ), 
            }
            if ( ofd.ShowDialog( this ) == DialogResult.OK )
            {
                this.FFmpegFileLocation = ofd.FileName;
            }
        }
        private void ffmpeg_RenameAfterFFmpegConverterCheckBox_CheckedChanged( object sender, EventArgs e ) => ffmpeg_OpenAfterWithExternalProgRunnerCheckBox.Enabled = ffmpeg_RenameAfterFFmpegConverterCheckBox.Checked;
        private void ffmpegApplyByDefaultCheckBox_CheckedChanged( object sender, EventArgs e ) => ffmpegPictureBox.Enabled = ffmpegApplyByDefaultCheckBox.Checked;

        private void testDirectorySelectDialog_Click( object sender, EventArgs e )
        {
            if ( UseDirectorySelectDialogModern )
            {
                DirectorySelectDialog.Show_AsFileSelectDialog( this, Environment.CurrentDirectory, this.toolTip.GetToolTip( testDirectorySelectDialog ), out var _ );
            }
            else
            {
                DirectorySelectDialog.Show_Classic( this, Environment.CurrentDirectory, this.toolTip.GetToolTip( testDirectorySelectDialog ), out var _ );
            }
        }

        private void CheckFilesExistence()
        {
            void set_error( TextBox textBox, string filePath )
            {
                var exists = File.Exists( filePath );
                errorProvider.SetError( textBox, exists ? null : $"file not exists: '{filePath}'." );
                if ( !exists )
                {
                    errorProvider.SetIconPadding  ( textBox, -17 ); 
                    errorProvider.SetIconAlignment( textBox, ErrorIconAlignment.MiddleRight );
                }
            }

            set_error( externalProgFilePathTextBox, this.ExternalProgFilePath );
            set_error( ffmpegFilePathTextBox      , this.FFmpegFileLocation   );
        }

        private static string GetDirectoryName_NoThrow( string path )
        {
            try
            {
                return (Path.GetDirectoryName( path ));
            }
            catch
            {
                return (path);
            }
        }
        private static string GetFileName_NoThrow( string path )
        {
            try
            {
                return (Path.GetFileName( path ));
            }
            catch
            {
                return (path);
            }
        }
        #endregion

        #region [.Collect_Garbage.]
        public void StartShowTotalMemory()
        {
            if ( _GetTotalMemoryTimer == null )
            {
                var tick = new EventHandler((_, _) =>
                {
                    CollectGarbage.GetTotalMemory( out var totalMemoryBytes );
                    currentMemoryLabel.Text    = $"Current Memory: {GetTotalMemoryFormatText( totalMemoryBytes )}.";
                    currentMemoryLabel.Visible = true;

                    CheckFilesExistence();
                });
                _GetTotalMemoryTimer = new WinTimer( components ) { Interval = 1_000, Enabled = true };
                _GetTotalMemoryTimer.Tick += tick;
                tick( _GetTotalMemoryTimer, EventArgs.Empty );
            }            
        }

        private static string GetTotalMemoryFormatText( long totalMemoryBytes ) => $"{(totalMemoryBytes / (1024.0 * 1024)):N2} MB";
        private void collectGarbageButton_Click( object sender, EventArgs e )
        {
            var btn = (Button) sender;
            btn.Text = "...";
            btn.Enabled = false;

            CollectGarbage.Collect_Garbage( out var totalMemoryBytes );
            //var totalMemoryBytes = await Task.Run( () => { CollectGarbage.Collect_Garbage( out var totalMemoryBytes_ ); return (totalMemoryBytes_); } );

            var text        = GetTotalMemoryFormatText( totalMemoryBytes );
            var toolTipText = $"Collect Garbage. Total Memory: {text}.";

            btn.Text = text;
            toolTip.SetToolTip( btn, toolTipText );
            btn.Enabled = true;
        }
        #endregion

        #region [.ReceivedAndWritedPartsStorer.]
        public void StartCalcReceivedAndWritedParts()
        {
            if ( _CalcReceivedAndWritedPartsTask_Cts == null )
            {
                _CalcReceivedAndWritedPartsTask_Cts = new CancellationTokenSource();
                var task_calc = Task.Run( async () =>
                {
                    for ( var ct = _CalcReceivedAndWritedPartsTask_Cts.Token; !_CalcReceivedAndWritedPartsTask_Cts.IsCancellationRequested; )
                    {
                        var suc = _ReceivedAndWritedPartsProcessor.TryCalcStats( out var storeFilesCount, out var storeFilesSize );
                        if ( suc )
                        {
                            await this.BeginInvoke_UseTask(() =>
                            { 
                                receivedAndWritedPartsLabel.Text    = $"Store files: {storeFilesCount}, Total size: {FileHelperEx.GetDisplaySizeText( storeFilesSize )}.";
                                receivedAndWritedPartsLabel.Visible = true;
                            });
                        }
#if NETCOREAPP
                        await Task.Delay( 2_000 ).WaitAsync( ct ).CAX();
#else
                        Task.Delay( 2_000 ).Wait( ct ); 
#endif
                    }
                });
            }
        }

        private const string STORED_FILES_CAPTION = "Stored files";
        private async void receivedAndWritedPartsClearAllButton_Click( object sender, EventArgs e )
        {            
            if ( this.MessageBox_ShowQuestion( "Want to clear all info about stored files ?", STORED_FILES_CAPTION, MessageBoxButtons.OKCancel ) == DialogResult.OK )
            {
                receivedAndWritedPartsClearAllButton.Enabled = false;
                try
                {
                    var suc = _ReceivedAndWritedPartsProcessor.TryDeleteAllStorerFiles();
                    await Task.Delay( 250 );
                    if ( suc ) this.MessageBox_ShowInformation( "Success clear all stored files.", STORED_FILES_CAPTION );
                    else this.MessageBox_ShowError( "Failed", STORED_FILES_CAPTION );
                }
                finally
                {
                    receivedAndWritedPartsClearAllButton.Enabled = true;
                }
            }
        }

        private void browseReceivedAndWritedPartsDirectoryButton_Click( object sender, EventArgs e )
        {
            string errorMsg;
            var directoryLocation4StoreFiles = _ReceivedAndWritedPartsProcessor?.DirectoryLocation4StoreFiles;
            if ( directoryLocation4StoreFiles.IsNullOrEmpty() )
            {
                errorMsg = "Directory location for store files is null or empty.";
            }
            else if ( !Directory.Exists( directoryLocation4StoreFiles ) )
            {
                errorMsg = $"Directory location for store files not exists.\r\n(path: '{directoryLocation4StoreFiles}')";
            }
            else
            {
                var suc = WinApi.ShellExploreBrowseDirectory( this.FindForm()?.Handle ?? IntPtr.Zero, directoryLocation4StoreFiles, out var error );
                errorMsg = suc ? default : error.ToString();
            }

            if ( errorMsg != null )
            {
                this.MessageBox_ShowError( errorMsg, STORED_FILES_CAPTION );
            }
        }
        #endregion
    }
}
