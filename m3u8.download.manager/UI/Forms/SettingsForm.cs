using System;
using System.ComponentModel;
using System.Windows.Forms;

using m3u8.download.manager.controllers;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class SettingsForm : Form
    {
        #region [.fields.]
        private DownloadController _DownloadController;
        #endregion

        #region [.ctor().]
        private SettingsForm() => InitializeComponent();
        public SettingsForm( DownloadController dc ) : this()
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
                _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged;
            }
            base.Dispose( disposing );
        }
        #endregion

        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );

            if ( (DialogResult == DialogResult.OK) && this.OutputFileExtension.IsNullOrEmpty() )
            {
                e.Cancel = true;
                outputFileExtensionTextBox.FocusAndBlinkBackColor();
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
        #endregion
    }
}
