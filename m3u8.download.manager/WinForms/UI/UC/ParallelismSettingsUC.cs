using System;
using System.Drawing;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using SETTINGS = m3u8.download.manager.controllers.SettingsPropertyChangeController;
using B = System.ComponentModel.BrowsableAttribute;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class ParallelismSettingsUC : UserControl
    {
        #region [.fields.]
        private DownloadController _DownloadController; 
        #endregion

        #region [.ctor().]
        public ParallelismSettingsUC()
        {
            InitializeComponent();

            maxDegreeOfParallelismNUD.Maximum = SETTINGS.MAX_DEGREE_OF_PARALLELISM;

            this.SetForeColor4ParentOnly< GroupBox >( Color.DodgerBlue );
            useMaxCrossDownloadInstanceCheckBox.ForeColor = Color.DodgerBlue;
        }
        public ParallelismSettingsUC( DownloadController dc ) : this() => Init( dc );
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

        #region [.public.]
        [B(false)]
        public int  MaxDegreeOfParallelism
        {
            get => Math.Min( SETTINGS.MAX_DEGREE_OF_PARALLELISM, Math.Max( 1, Convert.ToInt32( maxDegreeOfParallelismNUD.Value ) ) );
            set => maxDegreeOfParallelismNUD.Value = Math.Min( SETTINGS.MAX_DEGREE_OF_PARALLELISM, Math.Max( 1, value ) );
        }
        [B(false)]
        public bool UseCrossDownloadInstanceParallelism
        {
            get => useCrossDownloadInstanceParallelismCheckBox.Checked;
            set => useCrossDownloadInstanceParallelismCheckBox.Checked = value;
        }

        [B(false)]
        public int? MaxCrossDownloadInstance      => (useMaxCrossDownloadInstanceCheckBox.Checked ? MaxCrossDownloadInstanceSaved : null);
        [B(false)]
        public int  MaxCrossDownloadInstanceSaved => Math.Max( 1, Convert.ToInt32( maxCrossDownloadInstanceNUD.Value ) );
        public void SetMaxCrossDownloadInstance( int? maxCrossDownloadInstance, int maxCrossDownloadInstanceSaved )
        {
            maxCrossDownloadInstanceNUD.Value = Math.Max( 1, maxCrossDownloadInstance.GetValueOrDefault( maxCrossDownloadInstanceSaved ) );
            useMaxCrossDownloadInstanceCheckBox.Checked = maxCrossDownloadInstance.HasValue;
            useMaxCrossDownloadInstanceCheckBox_CheckedChanged( useMaxCrossDownloadInstanceCheckBox, EventArgs.Empty );
        }

        [B(false)]
        public double? MaxSpeedThresholdInMbps => (!isUnlimMaxSpeedThresholdCheckBox.Checked ? MaxSpeedThresholdInMbpsSaved : null);
        [B(false)]
        public double MaxSpeedThresholdInMbpsSaved => Math.Max( 0.1, Convert.ToDouble( maxSpeedThresholdNUD.Value ) );
        public void SetMaxSpeedThresholdInMbps( double? maxSpeedThresholdInMbps, double maxSpeedThresholdInMbpsSaved )
        {
            maxSpeedThresholdNUD.Value = Convert.ToDecimal( Math.Max( 0.1, maxSpeedThresholdInMbps.GetValueOrDefault( maxSpeedThresholdInMbpsSaved ) ) );
            isUnlimMaxSpeedThresholdCheckBox.Checked = !maxSpeedThresholdInMbps.HasValue;
            isUnlimMaxSpeedThresholdCheckBox_CheckedChanged( isUnlimMaxSpeedThresholdCheckBox, EventArgs.Empty );
        }
        #endregion

        #region [.private methods.]
        private void DownloadController_IsDownloadingChanged( bool isDownloading )
        {
            useMaxCrossDownloadInstanceCheckBox.Enabled =
                useCrossDownloadInstanceParallelismCheckBox.Enabled = !isDownloading;
        }
        private void useMaxCrossDownloadInstanceCheckBox_CheckedChanged( object sender, EventArgs e )
        {
            maxCrossDownloadInstanceLabel.Enabled = useMaxCrossDownloadInstanceCheckBox.Checked;
            maxCrossDownloadInstanceNUD  .Enabled = useMaxCrossDownloadInstanceCheckBox.Checked;
        }
        //private void useCrossDownloadInstanceParallelismCheckBox_CheckedChanged( object sender, EventArgs e ) 
        //    => useCrossDownloadInstanceParallelismCheckBox.Text = useCrossDownloadInstanceParallelismCheckBox.Checked
        //        ? "share \"max download threads\"\r\n between all downloads-instance"
        //        : "use \"max download threads\"\r\n per each downloads-instance";
        private void isUnlimMaxSpeedThresholdCheckBox_CheckedChanged( object sender, EventArgs e )
        {
            maxSpeedThresholdLabel.Enabled = !isUnlimMaxSpeedThresholdCheckBox.Checked;
            maxSpeedThresholdNUD  .Enabled = !isUnlimMaxSpeedThresholdCheckBox.Checked;
        }
        #endregion
    }
}
