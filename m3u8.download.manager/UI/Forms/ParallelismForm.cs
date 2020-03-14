using System;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using SETTINGS = m3u8.download.manager.controllers.SettingsPropertyChangeController;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class ParallelismForm : Form
    {
        #region [.fields.]
        private DownloadController _DownloadController; 
        #endregion

        #region [.ctor().]
        private ParallelismForm()
        {
            InitializeComponent();

            maxDegreeOfParallelismNUD.Maximum = SETTINGS.MAX_DEGREE_OF_PARALLELISM;
        }
        public ParallelismForm( DownloadController dc ) : this()
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

        #region [.public.]
        public int  MaxDegreeOfParallelism
        {
            get => Math.Min( SETTINGS.MAX_DEGREE_OF_PARALLELISM, Convert.ToInt32( maxDegreeOfParallelismNUD.Value ) );
            set => maxDegreeOfParallelismNUD.Value = Math.Min( SETTINGS.MAX_DEGREE_OF_PARALLELISM, value );
        }
        public bool UseCrossDownloadInstanceParallelism
        {
            get => useCrossDownloadInstanceParallelismCheckBox.Checked;
            set => useCrossDownloadInstanceParallelismCheckBox.Checked = value;
        }
        public int? MaxCrossDownloadInstance      => (useMaxCrossDownloadInstanceCheckBox.Checked ? Convert.ToInt32( maxCrossDownloadInstanceNUD.Value ) : ((int?) null));
        public int  MaxCrossDownloadInstanceSaved => Convert.ToInt32( maxCrossDownloadInstanceNUD.Value );
        public void SetMaxCrossDownloadInstance( int? maxCrossDownloadInstance, int maxCrossDownloadInstanceSaved )
        {
            maxCrossDownloadInstanceNUD.Value = maxCrossDownloadInstance.GetValueOrDefault( maxCrossDownloadInstanceSaved );
            useMaxCrossDownloadInstanceCheckBox.Checked = maxCrossDownloadInstance.HasValue;
            useMaxCrossDownloadInstanceCheckBox_CheckedChanged( useMaxCrossDownloadInstanceCheckBox, EventArgs.Empty );
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
        #endregion
    }
}
