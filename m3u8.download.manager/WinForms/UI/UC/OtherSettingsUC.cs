using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using m3u8.download.manager.Properties;
using B = System.ComponentModel.BrowsableAttribute;

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
        private Timer _GetTotalMemoryTimer;
        #endregion

        #region [.ctor().]
        public OtherSettingsUC()
        {
            InitializeComponent();

            this.SetForeColor4ParentOnly< GroupBox >( Color.DodgerBlue );
            currentMemoryLabel.ForeColor = Color.DimGray;
        }
        public OtherSettingsUC( DownloadController dc ) : this() => Init( dc );
        public void Init( DownloadController dc )
        {
            _DownloadController = dc ?? throw (new ArgumentNullException( nameof(dc) ));
            _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged;
            _DownloadController.IsDownloadingChanged += DownloadController_IsDownloadingChanged;

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
            }
            base.Dispose( disposing );
        }
        #endregion

        public void OnShown() => _ExternalProgFilePath_InitValue = this.ExternalProgFilePath;
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
            }
        }


        #region [.public props.]
        [B(false)]
        public int      AttemptRequestCountByPart
        {
            get => Convert.ToInt32( attemptRequestCountByPartNUD.Value );
            set => attemptRequestCountByPartNUD.Value = value;
        }
        [B(false)]
        public TimeSpan RequestTimeoutByPart
        {
            get => requestTimeoutByPartDTP.Value.TimeOfDay; // TimeSpan.FromTicks( (requestTimeoutByPartDTP.Value.TimeOfDay - requestTimeoutByPartDTP.MinDate.Date).Ticks );
            set => requestTimeoutByPartDTP.Value = requestTimeoutByPartDTP.MinDate.Date + value;
        }
        [B(false)]
        public bool     ShowOnlyRequestRowsWithErrors
        {
            get => showOnlyRequestRowsWithErrorsCheckBox.Checked;
            set => showOnlyRequestRowsWithErrorsCheckBox.Checked = value;
        }
        [B(false)]
        public bool     ShowDownloadStatisticsInMainFormTitle
        {
            get => showDownloadStatisticsInMainFormTitleCheckBox.Checked;
            set => showDownloadStatisticsInMainFormTitleCheckBox.Checked = value;
        }
        [B(false)]
        public bool     ShowAllDownloadsCompleted_Notification
        {
            get => showAllDownloadsCompleted_NotificationCheckBox.Checked;
            set => showAllDownloadsCompleted_NotificationCheckBox.Checked = value;
        }        
        [B(false)]
        public bool     UniqueUrlsOnly
        {
            get => uniqueUrlsOnlyCheckBox.Checked;
            set => uniqueUrlsOnlyCheckBox.Checked = value;
        }
        [B(false)]
        public string   OutputFileExtension
        {
            get => CorrectOutputFileExtension( outputFileExtensionTextBox.Text );
            set => outputFileExtensionTextBox.Text = CorrectOutputFileExtension( value );
        }
        [B(false)]
        public string   ExternalProgCaption
        {
            get => externalProgCaptionTextBox.Text?.Trim();
            set => externalProgCaptionTextBox.Text = value?.Trim();
        }
        [B(false)]
        public string   ExternalProgFilePath
        {
            get => externalProgFilePathTextBox.Text?.Trim();
            set => externalProgFilePathTextBox.Text = value?.Trim();
        }
        [B(false)]
        public bool     ExternalProgApplyByDefault
        {
            get => externalProgApplyByDefaultCheckBox.Checked;
            set => externalProgApplyByDefaultCheckBox.Checked = value;
        }
        [B(false)]
        public bool     UseDirectorySelectDialogModern
        {
            get => useDirectorySelectDialogModernCheckBox.Checked;
            set => useDirectorySelectDialogModernCheckBox.Checked = value;
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
                _GetTotalMemoryTimer = new Timer( components ) { Interval = 1_000, Enabled = true };
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
    }
}
