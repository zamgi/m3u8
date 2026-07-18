using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public void OnShown() { _ExternalProgFilePath_InitValue = this.ExternalProgFilePath; _FFmpegFileLocation_InitValue = this.FFmpegFileLocation; }
        public void OnClosing( DialogResult dialogResult, CancelEventArgs e )
        {
            _CalcReceivedAndWritedPartsTask_Cts?.Cancel_NoThrow();

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
        }


        #region [.public props.]
        public int      AttemptRequestCountByPart
        {
            get => Convert.ToInt32( attemptRequestCountByPartNUD.Value );
            set => attemptRequestCountByPartNUD.Value = value;
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
            set => externalProgApplyByDefaultCheckBox.Checked = value;
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
            set => ffmpegApplyByDefaultCheckBox.Checked = value;
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
            this.ExternalProgFilePath = Resources.ExternalProgFilePath;
            this.ExternalProgCaption  = Resources.ExternalProgCaption;
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

        private void ffmpegResetButton_Click( object sender, EventArgs e )
        {
            this.FFmpegFileLocation     = Resources.FFmpegFileLocation;
            this.FFmpegConverterCaption = Resources.FFmpegConverterCaption;
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
        private void testDirectorySelectDialog_Paint( object sender, PaintEventArgs e )
        {
            var c  = (Control) sender;
            var rc = e.ClipRectangle;
            using var sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
            e.Graphics.DrawString( "...", c.Font, Brushes.Black, rc, sf );
            if ( c.Focused )
            {
                rc.Inflate( -1, -1 );
                ControlPaint.DrawFocusRectangle( e.Graphics, rc );
            }
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
                                receivedAndWritedPartsLabel.Text    = $"Store files: {storeFilesCount}, Total size: {FileHelper.GetDisplaySizeText( storeFilesSize )}.";
                                receivedAndWritedPartsLabel.Visible = true;
                            });
                        }
#if NETCOREAPP
                        await Task.Delay( 2_000 ).WaitAsync( ct );
#else
                        Task.Delay( 1_000 ).Wait( ct ); 
#endif
                    }
                });
            }
        }

        private async void receivedAndWritedPartsClearAllButton_Click( object sender, EventArgs e )
        {
            const string CAPTION = "Stored files";
            if ( this.MessageBox_ShowQuestion( "Want to clear all info about stored files ?", CAPTION, MessageBoxButtons.OKCancel ) == DialogResult.OK )
            {
                receivedAndWritedPartsClearAllButton.Enabled = false;
                try
                {
                    var suc = _ReceivedAndWritedPartsProcessor.TryDeleteAllStorerFiles();
                    await Task.Delay( 250 );
                    if ( suc ) this.MessageBox_ShowInformation( "Success clear all stored files.", CAPTION );
                    else this.MessageBox_ShowError( "Failed", CAPTION );
                }
                finally
                {
                    receivedAndWritedPartsClearAllButton.Enabled = true;
                }
            }
        }
        #endregion

        public void ActiveteTab()
        {
            StartShowTotalMemory();
            StartCalcReceivedAndWritedParts();
        }
    }
}
