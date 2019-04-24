using System;
using System.Windows.Forms;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class SettingsForm : Form
    {
        #region [.ctor().]
        public SettingsForm() => InitializeComponent();
        #endregion

        #region [.private methods.]
        private void logUITextBoxCheckBox_CheckedChanged ( object sender, EventArgs e ) => this.DownloadLogUIType = DownloadLogUITypeEnum.TextBoxUIType;
        private void logUIGridViewCheckBox_CheckedChanged( object sender, EventArgs e ) => this.DownloadLogUIType = DownloadLogUITypeEnum.GridViewUIType;
        #endregion

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
        public DownloadLogUITypeEnum DownloadLogUIType
        {
            get => (logUITextBoxCheckBox.Checked ? DownloadLogUITypeEnum.TextBoxUIType : DownloadLogUITypeEnum.GridViewUIType);
            set
            {
                logUIGridViewCheckBox.CheckedChanged -= logUIGridViewCheckBox_CheckedChanged;
                logUITextBoxCheckBox .CheckedChanged -= logUITextBoxCheckBox_CheckedChanged;

                switch ( value )
                {
                    case DownloadLogUITypeEnum.TextBoxUIType:
                        logUITextBoxCheckBox .Checked = true;
                        logUIGridViewCheckBox.Checked = false;
                        showOnlyRequestRowsWithErrorsCheckBox.Enabled = false;
                    break;

                    case DownloadLogUITypeEnum.GridViewUIType:
                        logUITextBoxCheckBox .Checked = false;
                        logUIGridViewCheckBox.Checked = true;
                        showOnlyRequestRowsWithErrorsCheckBox.Enabled = true;
                    break;
                }

                logUIGridViewCheckBox.CheckedChanged += logUIGridViewCheckBox_CheckedChanged;
                logUITextBoxCheckBox .CheckedChanged += logUITextBoxCheckBox_CheckedChanged;
            }
        }
        public bool     ShowOnlyRequestRowsWithErrors
        {
            get => showOnlyRequestRowsWithErrorsCheckBox.Checked;
            set => showOnlyRequestRowsWithErrorsCheckBox.Checked = value;
        }
        public bool     StoreMainFormPosition
        {
            get => storeMainFormPositionCheckBox.Checked;
            set => storeMainFormPositionCheckBox.Checked = value;
        }
        #endregion
    }
}
